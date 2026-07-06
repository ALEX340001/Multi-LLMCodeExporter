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
using LLMCodeExporter.Core.Interfaces;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Services;
using LLMCodeExporter.Infrastructure.Utils;

namespace LLMCodeExporter.CLI.UI;

/// <summary>
/// Обработчик интерактивного режима работы
/// </summary>
public static class InteractiveModeHandler
{
    /// <summary>
    /// Результат выполнения интерактивного режима
    /// </summary>
    public class InteractiveResult
    {
        public bool Success { get; set; }
        public ProjectInfo? ProjectInfo { get; set; }
        public ExportSettings? Settings { get; set; }
        public string? ErrorMessage { get; set; }
        public bool Cancelled { get; set; }
    }

    /// <summary>
    /// Запрашивает путь к проекту у пользователя
    /// </summary>
    public static string? GetProjectPath(string[] args)
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("📁 ВЫБОР ПРОЕКТА ДЛЯ ЭКСПОРТА");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("Вы можете:");
        Console.WriteLine("  1. 📂 Перетащить папку проекта в это окно и нажать Enter");
        Console.WriteLine("  2. 📝 Ввести полный путь к проекту вручную");
        Console.WriteLine("  3. 🚪 Нажать Ctrl+C для выхода");
        Console.WriteLine();
        Console.WriteLine($"Текущая папка: {Directory.GetCurrentDirectory()}");
        Console.WriteLine();

        // Если есть аргументы командной строки (drag&drop), используем их
        if (args.Length > 0 && !args[0].StartsWith("--") && !args[0].StartsWith("-"))
        {
            string path = args[0].Trim().Trim('"');
            if (Directory.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Используется путь из аргументов: {path}");
                Console.ResetColor();
                return path;
            }
            if (File.Exists(path))
            {
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"✓ Используется директория файла: {directory}");
                    Console.ResetColor();
                    return directory;
                }
            }
        }

        // Запрашиваем путь у пользователя
        string? pathFromUser = InputHelper.ReadDirectoryPath(
            "Введите путь к проекту или перетащите папку сюда:",
            maxAttempts: 5
        );

        if (string.IsNullOrEmpty(pathFromUser))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠ Путь не указан. Используется текущая директория.");
            Console.ResetColor();
            string currentDir = Directory.GetCurrentDirectory();
            Console.WriteLine($"Текущая директория: {currentDir}");

            if (InputHelper.ReadYesNo("Использовать текущую директорию как проект?", defaultValue: true))
                return currentDir;

            return null;
        }

        return pathFromUser;
    }

    /// <summary>
    /// Выполняет экспорт в интерактивном режиме
    /// </summary>
    public static InteractiveResult Execute(string projectPath)
    {
        var result = new InteractiveResult();

        try
        {
            // Инициализация сервисов
            IFileScanner scanner = new FileScanner();
            ICodeProcessor processor = new CodeProcessor();
            IExportService exporter = new ExportService(processor);

            // Первичное сканирование для определения типа проекта
            Console.WriteLine("⏳ Анализ структуры проекта...");
            var (tempSettings, detectedType) = CreateTempSettings(projectPath);
            var projectInfo = scanner.ScanProject(projectPath, tempSettings);

            // Если файлы не найдены, пробуем альтернативные расширения
            if (!projectInfo.Files.Any())
            {
                projectInfo = TryAlternativeExtensions(scanner, projectPath, ref detectedType);
            }

            if (!projectInfo.Files.Any())
            {
                PrintNoFilesFoundError(projectPath);
                result.Cancelled = true;
                return result;
            }

            Console.WriteLine($"✓ Анализ завершен! Найдено файлов: {projectInfo.Files.Count}");
            Console.WriteLine();
            InteractiveMenu.ShowProjectStats(projectInfo);

            // Настройка экспорта через меню
            ExportSettings? configuredSettings = InteractiveMenu.ConfigureSettings(projectInfo);
            if (configuredSettings == null)
            {
                Logger.Log("Экспорт отменён пользователем");
                result.Cancelled = true;
                return result;
            }

            // Повторное сканирование с выбранными настройками
            Console.WriteLine("⏳ Сканирование проекта с выбранными настройками...");
            projectInfo = scanner.ScanProject(projectPath, configuredSettings);

            // Проверка соответствия выбранных языков содержимому проекта
            if (!ValidateLanguageMatch(projectInfo, configuredSettings))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ Хотите изменить выбор языков или продолжить с текущими настройками?");
                Console.ResetColor();
                
                if (InputHelper.ReadYesNo("Продолжить с текущими настройками?", defaultValue: false))
                {
                    Logger.LogWarning("Пользователь продолжил экспорт, несмотря на несоответствие языков");
                }
                else
                {
                    Logger.Log("Экспорт отменён пользователем после предупреждения о несоответствии языков");
                    result.Cancelled = true;
                    return result;
                }
            }

            result.Success = true;
            result.ProjectInfo = projectInfo;
            result.Settings = configuredSettings;
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            Logger.LogError("Ошибка в интерактивном режиме", ex);
            return result;
        }
    }

    /// <summary>
    /// Создаёт временные настройки для первичного сканирования
    /// </summary>
    private static (ExportSettings settings, ProjectType detectedType) CreateTempSettings(string projectPath)
    {
        var settings = new ExportSettings();
        var detectedType = ProjectTypeDetector.Detect(projectPath);
        Logger.Log($"Автоопределенный тип проекта: {detectedType}");

        switch (detectedType)
        {
            case ProjectType.Python:
                settings.ApplyPythonProjectPreset();
                Console.WriteLine($"ℹ️  Определен тип проекта: Python");
                break;

            case ProjectType.WebApp:
                settings.ApplyWebAppPreset();
                Console.WriteLine($"ℹ️  Определен тип проекта: Веб-приложение");
                break;

            case ProjectType.Hybrid:
                var langs = ProjectTypeDetector.DetectLanguages(projectPath);
                if (langs.Count >= 2)
                {
                    settings.BackendLanguage = langs[0];
                    settings.FrontendLanguage = langs[1];
                }
                else if (langs.Count == 1)
                {
                    settings.BackendLanguage = langs[0];
                    // Фронтенд оставляем по умолчанию
                }
                settings.ApplyHybridPreset();
                Console.WriteLine($"ℹ️  Определен тип проекта: Гибридный ({settings.BackendLanguage} + {settings.FrontendLanguage})");
                break;

            case ProjectType.CSharp:
            default:
                settings.FileExtensions = new[] { "*.cs" };
                Console.WriteLine($"ℹ️  Определен тип проекта: C#");
                break;
        }

        return (settings, detectedType);
    }

    /// <summary>
    /// Пробует альтернативные расширения файлов
    /// </summary>
    private static ProjectInfo TryAlternativeExtensions(IFileScanner scanner, string projectPath, ref ProjectType detectedType)
    {
        Logger.LogWarning("Не найдено файлов с текущими расширениями, пробуем альтернативные");
        Console.WriteLine("⚠ Не найдено файлов с определенными расширениями, пробуем другие типы...");

        var alternativeExtensions = new[]
        {
            new[] { "*.js", "*.jsx", "*.ts", "*.tsx", "*.html", "*.htm", "*.css", "*.scss" },
            new[] { "*.py", "*.pyw" },
            new[] { "*.cs" }
        };

        foreach (var extensions in alternativeExtensions)
        {
            var tempSettings = new ExportSettings { FileExtensions = extensions };
            var projectInfo = scanner.ScanProject(projectPath, tempSettings);

            if (projectInfo.Files.Any())
            {
                Logger.Log($"Найдены файлы с расширениями: {string.Join(", ", extensions)}");

                if (extensions.Contains("*.js") || extensions.Contains("*.html") || extensions.Contains("*.css"))
                    detectedType = ProjectType.WebApp;
                else if (extensions.Contains("*.py"))
                    detectedType = ProjectType.Python;
                else if (extensions.Contains("*.cs"))
                    detectedType = ProjectType.CSharp;

                return projectInfo;
            }
        }

        return new ProjectInfo { ProjectPath = projectPath, Files = new() };
    }

    /// <summary>
    /// Выводит сообщение об ошибке "файлы не найдены"
    /// </summary>
    private static void PrintNoFilesFoundError(string projectPath)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n⚠ В указанной папке не найдено подходящих файлов.");
        Console.WriteLine($"  Путь: {projectPath}");
        Console.WriteLine("\nПоддерживаемые расширения:");
        Console.WriteLine("  • C#: .cs");
        Console.WriteLine("  • Python: .py, .pyw");
        Console.WriteLine("  • Веб-приложения: .js, .jsx, .ts, .tsx, .html, .htm, .css, .scss");
        Console.WriteLine("  • Гибридные: комбинация C# + веб-файлы");
        Console.ResetColor();
        Console.WriteLine("\nПопробуйте:");
        Console.WriteLine("  1. Проверить путь к проекту");
        Console.WriteLine("  2. Выбрать другой тип проекта в меню");
        Console.WriteLine("  3. Использовать CLI режим с явным указанием типа (--web-app, --mode=python, --hybrid)");
        Console.WriteLine();
    }

    /// <summary>
    /// Проверяет соответствие выбранных языков содержимому проекта
    /// </summary>
    private static bool ValidateLanguageMatch(ProjectInfo projectInfo, ExportSettings settings)
    {
        if (settings.ProjectType != ProjectType.Hybrid)
            return true;

        var backendExtensions = LanguageSettings.ForLanguage(settings.BackendLanguage).FileExtensions;
        var frontendExtensions = LanguageSettings.ForLanguage(settings.FrontendLanguage).FileExtensions;
        var allExtensions = backendExtensions.Union(frontendExtensions).ToHashSet();

        bool hasMatchingFiles = projectInfo.Files.Any(f =>
            allExtensions.Contains(Path.GetExtension(f.RelativePath).ToLowerInvariant()));

        if (!hasMatchingFiles)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n⚠️ Внимание: В проекте не найдено файлов, соответствующих выбранным языкам:");
            Console.WriteLine($"   Бекенд: {settings.BackendLanguage}, Фронтенд: {settings.FrontendLanguage}");
            Console.WriteLine("   Возможно, вы выбрали не те языки, или проект содержит другие технологии.");
            Console.ResetColor();
            return false;
        }

        return true;
    }
}