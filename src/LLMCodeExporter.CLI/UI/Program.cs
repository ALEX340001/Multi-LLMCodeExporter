/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;          
using LLMCodeExporter.Core.Interfaces;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Services;
using LLMCodeExporter.Infrastructure.Utils;
using LLMCodeExporter.CLI.UI;
using LLMCode_Importer;                 

Console.OutputEncoding = Encoding.UTF8;

// ============================================================
//  НОВЫЙ БЛОК: если передан --start-input – запускаем импортёр
// ============================================================
if (args.Length > 0 && args[0] == "--start-input")
{
    var importerArgs = args.Skip(1).ToArray();
    // Вызываем метод Main импортёра (он асинхронный)
    return await LLMCode_Importer.Program.Main(importerArgs);
}

// === 1. Парсинг аргументов командной строки ===
var parseResult = CliArgumentParser.Parse(args);

if (parseResult.ShowHelp)
{
    HelpPrinter.Print();
    return 0;                          // <-- изменено на return 0
}

if (parseResult.HasErrors)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Ошибка: {parseResult.ErrorMessage}");
    Console.ResetColor();
    Console.WriteLine("Используйте --help для справки.");
    return 1;                          // <-- изменено на return 1
}

// === 2. Проверка CLI режима ===
if (parseResult.HasCliArgs && string.IsNullOrEmpty(parseResult.ProjectPath))
{
    Console.WriteLine("Ошибка: путь к проекту не указан.");
    Console.WriteLine("Используйте --help для справки.");
    return 1;
}

// === 3. Заголовок приложения (только в интерактивном режиме) ===
if (!parseResult.HasCliArgs)
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("╔════════════════════════════════════════╗");
    Console.WriteLine("║  📦 Multi-LLMCodeExporter v_02         ║");
    Console.WriteLine("║     Экспорт кода для нейросетей        ║");
    Console.WriteLine("╚════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

// === 4. Инициализация логгера ===
Logger.Log("=== Приложение запущено ===");
Logger.Log($"Режим: {(parseResult.HasCliArgs ? "CLI" : "Interactive")}");
Logger.Log($"Система: {Environment.OSVersion}");
Logger.Log($"Текущая директория: {Directory.GetCurrentDirectory()}");

// === 5. Получение пути к проекту ===
string? projectPath = parseResult.ProjectPath;

if (!parseResult.HasCliArgs)
{
    projectPath = InteractiveModeHandler.GetProjectPath(args);
    if (string.IsNullOrEmpty(projectPath))
    {
        Console.WriteLine("Экспорт отменен.");
        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
        return 0;
    }
}

// === 6. Нормализация пути ===
projectPath = PathNormalizer.Normalize(projectPath);
Logger.Log($"Путь к проекту: {projectPath}");

if (!PathNormalizer.ValidateAndPrintError(projectPath))
{
    Console.WriteLine("\nНажмите любую клавишу для выхода...");
    Console.ReadKey();
    return 1;
}

// === 7. Выполнение экспорта ===
try
{
    ProjectInfo projectInfo;
    ExportSettings? settings = parseResult.Settings;

    if (parseResult.HasCliArgs)
    {
        // CLI режим
        var cliResult = CliModeHandler.Execute(projectPath, settings);
        if (!cliResult.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка: {cliResult.ErrorMessage}");
            Console.ResetColor();
            Logger.LogError($"CLI ошибка: {cliResult.ErrorMessage}");
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
            return 1;
        }
        projectInfo = cliResult.ProjectInfo!;
        settings = cliResult.Settings!;
    }
    else
    {
        // Интерактивный режим
        var interactiveResult = InteractiveModeHandler.Execute(projectPath);
        if (interactiveResult.Cancelled)
        {
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
            return 0;
        }
        if (!interactiveResult.Success)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка: {interactiveResult.ErrorMessage}");
            Console.ResetColor();
            Logger.LogError($"Интерактивная ошибка: {interactiveResult.ErrorMessage}");
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
            return 1;
        }
        projectInfo = interactiveResult.ProjectInfo!;
        settings = interactiveResult.Settings!;
    }

    // === 8. Проверка на пустой экспорт ===
    if (!projectInfo.Files.Any())
    {
        PrintNoFilesError(projectInfo, settings);
        Logger.LogWarning("Экспорт отменён - нет файлов после фильтрации.");
        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
        return 0;
    }

    // === 9. Экспорт ===
    Console.WriteLine("⏳ Создание экспорта...");
    ICodeProcessor processor = new CodeProcessor();
    IExportService exporter = new ExportService(processor);
    var result = exporter.ExportProject(projectInfo, settings);
    Console.WriteLine();
    InteractiveMenu.ShowExportResult(result);
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n✗ Критическая ошибка: {ex.Message}");
    Console.WriteLine("\nПодробности в стеке вызовов:");
    Console.WriteLine(ex.StackTrace);
    Console.ResetColor();
    Logger.LogError("Критическая ошибка", ex);
    return 1;
}

// === 10. Завершение ===
Logger.Log("=== Приложение завершено ===");
if (!parseResult.HasCliArgs)
{
    Console.WriteLine();
    Console.WriteLine("Нажмите любую клавишу для выхода...");
    Console.ReadKey();
}

return 0;

// ============================================================================
// ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
// ============================================================================
static void PrintNoFilesError(ProjectInfo projectInfo, ExportSettings settings)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine();
    Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║  ⚠️  НЕТ ФАЙЛОВ ДЛЯ ЭКСПОРТА                                  ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
    Console.WriteLine($"📊 Статистика:");
    Console.WriteLine($"   Найдено всего: {projectInfo.TotalScannedFiles} файлов");
    Console.WriteLine($"   Исключено фильтрами: {projectInfo.ExcludedFiles.Count}");
    Console.WriteLine();
    Console.WriteLine("💡 Возможные причины:");
    if (settings.IncludeOnlyPatterns.Any())
    {
        Console.WriteLine($"   • Паттерны включения слишком строгие: {string.Join(", ", settings.IncludeOnlyPatterns)}");
        Console.WriteLine($"   • В проекте нет папок с такими именами");
    }
    if (settings.ExcludePatterns.Any())
    {
        Console.WriteLine($"   • Паттерны исключения слишком широкие: {string.Join(", ", settings.ExcludePatterns)}");
    }
    Console.WriteLine();
    Console.WriteLine("🔧 Что делать:");
    Console.WriteLine("   1. Запустите приложение снова");
    Console.WriteLine("   2. Выберите другой тип проекта");
    Console.WriteLine("   3. Используйте CLI режим с явным указанием типа (--web-app, --mode=python, --hybrid)");
    Console.WriteLine();
}