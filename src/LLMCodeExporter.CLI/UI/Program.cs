/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿using System.Text;
using LLMCodeExporter.Core.Interfaces;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Services;
using LLMCodeExporter.Infrastructure.Utils;
using LLMCodeExporter.CLI.UI;
using System.IO;
using System.Linq;

Console.OutputEncoding = Encoding.UTF8;

// Парсим аргументы командной строки
var (settings, projectPath, showHelp, hasCliArgs) = ParseArguments(args);

if (showHelp)
{
    ShowHelp();
    return;
}

// Проверка для CLI режима: если есть аргументы, но путь не указан, то ошибка
if (hasCliArgs && string.IsNullOrEmpty(projectPath))
{
    Console.WriteLine("Ошибка: путь к проекту не указан.");
    Console.WriteLine("Используйте --help для справки.");
    return;
}

// Заголовок приложения (только в интерактивном режиме)
if (!hasCliArgs)
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

Logger.Log("=== Приложение запущено ===");
Logger.Log($"Режим: {(hasCliArgs ? "CLI" : "Interactive")}");

// Получаем информацию о системе
Logger.Log($"Система: {Environment.OSVersion}");
Logger.Log($"Текущая директория: {Directory.GetCurrentDirectory()}");

// Получаем путь к проекту
if (!hasCliArgs)
{
    // Интерактивный режим - всегда запрашиваем путь
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
            projectPath = path;
        }
        else if (File.Exists(path))
        {
            string directory = Path.GetDirectoryName(path);
            if (Directory.Exists(directory))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"✓ Используется директория файла: {directory}");
                Console.ResetColor();
                projectPath = directory;
            }
        }
    }

    // Если путь еще не получен, запрашиваем у пользователя
    if (string.IsNullOrEmpty(projectPath))
    {
        string pathFromUser = InputHelper.ReadDirectoryPath(
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

            // Спросим подтверждение
            if (InputHelper.ReadYesNo("Использовать текущую директорию как проект?", defaultValue: true))
            {
                projectPath = currentDir;
            }
            else
            {
                Console.WriteLine("Экспорт отменен.");
                Console.WriteLine("\nНажмите любую клавишу для выхода...");
                Console.ReadKey();
                return;
            }
        }
        else
        {
            projectPath = pathFromUser;
        }
    }
}

// В CLI режиме projectPath уже должен быть установлен
// Нормализуем путь
projectPath = NormalizeProjectPath(projectPath);

if (string.IsNullOrEmpty(projectPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("✗ Не удалось получить путь к проекту!");
    Console.ResetColor();
    Logger.LogError("Не удалось получить путь к проекту");
    Console.WriteLine("\nНажмите любую клавишу для выхода...");
    Console.ReadKey();
    return;
}

Logger.Log($"Путь к проекту: {projectPath}");

// Проверка существования директории перед использованием
if (!Directory.Exists(projectPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"\n✗ Ошибка: Указанная папка не существует!");
    Console.WriteLine($"  Путь: {projectPath}");
    Console.ResetColor();
    Logger.LogError("Папка не найдена", new DirectoryNotFoundException(projectPath));

    // Показываем текущую директорию
    string currentDir = Directory.GetCurrentDirectory();
    Console.WriteLine($"\nТекущая директория: {currentDir}");

    // Ищем .csproj файлы в текущей директории
    var csprojFiles = Directory.GetFiles(currentDir, "*.csproj", SearchOption.TopDirectoryOnly);
    if (csprojFiles.Any())
    {
        Console.WriteLine($"\nНайдены .csproj файлы в текущей директории:");
        foreach (var file in csprojFiles)
        {
            Console.WriteLine($"  • {Path.GetFileName(file)}");
        }
    }

    Console.WriteLine("\nНажмите любую клавишу для выхода...");
    Console.ReadKey();
    return;
}

try
{
    // Инициализация сервисов
    IFileScanner scanner = new FileScanner();
    ICodeProcessor processor = new CodeProcessor();
    IExportService exporter = new ExportService(processor);

    // В ИНТЕРАКТИВНОМ РЕЖИМЕ сначала настраиваем, потом сканируем
    ProjectInfo projectInfo;

    if (!hasCliArgs)
    {
        // ИНТЕРАКТИВНЫЙ РЕЖИМ: 
        // 1. Сначала определяем/предполагаем тип проекта для предварительного сканирования
        // 2. Показываем меню настроек
        // 3. Сканируем с выбранными настройками

        Console.WriteLine("⏳ Анализ структуры проекта...");

        // Попробуем определить тип проекта автоматически
        var autoDetectedType = ProjectTypeDetector.Detect(projectPath);
        Logger.Log($"Автоопределенный тип проекта: {autoDetectedType}");

        // Временные настройки для предварительного сканирования
        var tempSettings = new ExportSettings();

        // Устанавливаем расширения в зависимости от определенного типа
        switch (autoDetectedType)
        {
            case ProjectType.Python:
                tempSettings.ApplyPythonProjectPreset();
                Console.WriteLine($"ℹ️  Определен тип проекта: Python");
                break;
            case ProjectType.WebApp:
                tempSettings.ApplyWebAppPreset();
                Console.WriteLine($"ℹ️  Определен тип проекта: Веб-приложение");
                break;
            case ProjectType.CSharp:
            default:
                // Для C# оставляем настройки по умолчанию
                tempSettings.FileExtensions = new[] { "*.cs" };
                Console.WriteLine($"ℹ️  Определен тип проекта: C#");
                break;
        }

        // Сканируем с временными настройками для статистики
        projectInfo = scanner.ScanProject(projectPath, tempSettings);

        // Если не найдено файлов с текущими настройками, попробуем другие расширения
        if (!projectInfo.Files.Any())
        {
            Logger.LogWarning("Не найдено файлов с текущими расширениями, пробуем альтернативные");

            Console.WriteLine("⚠ Не найдено файлов с определенными расширениями, пробуем другие типы...");

            // Пробуем разные типы файлов
            var alternativeExtensions = new[]
            {
                new[] { "*.js", "*.jsx", "*.ts", "*.tsx", "*.html", "*.htm", "*.css", "*.scss" }, // Web
                new[] { "*.py", "*.pyw" }, // Python
                new[] { "*.cs" } // C#
            };

            foreach (var extensions in alternativeExtensions)
            {
                tempSettings.FileExtensions = extensions;
                projectInfo = scanner.ScanProject(projectPath, tempSettings);
                if (projectInfo.Files.Any())
                {
                    Logger.Log($"Найдены файлы с расширениями: {string.Join(", ", extensions)}");

                    // Обновляем автоопределенный тип
                    if (extensions.Contains("*.js") || extensions.Contains("*.html") || extensions.Contains("*.css"))
                        autoDetectedType = ProjectType.WebApp;
                    else if (extensions.Contains("*.py"))
                        autoDetectedType = ProjectType.Python;
                    else if (extensions.Contains("*.cs"))
                        autoDetectedType = ProjectType.CSharp;

                    break;
                }
            }
        }

        if (!projectInfo.Files.Any())
        {
            // Совсем не найдено файлов - сообщаем пользователю
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n⚠ В указанной папке не найдено подходящих файлов.");
            Console.WriteLine($"  Путь: {projectPath}");
            Console.WriteLine("\nПоддерживаемые расширения:");
            Console.WriteLine("  • C#: .cs");
            Console.WriteLine("  • Python: .py, .pyw");
            Console.WriteLine("  • Веб-приложения: .js, .jsx, .ts, .tsx, .html, .htm, .css, .scss");
            Console.ResetColor();

            Console.WriteLine("\nПопробуйте:");
            Console.WriteLine("  1. Проверить путь к проекту");
            Console.WriteLine("  2. Выбрать другой тип проекта в меню");
            Console.WriteLine("  3. Использовать CLI режим с явным указанием типа (--web-app, --mode=python)");
            Console.WriteLine();

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"✓ Анализ завершен! Найдено файлов: {projectInfo.Files.Count}");
        Console.WriteLine();

        // Показываем статистику
        InteractiveMenu.ShowProjectStats(projectInfo);

        // Показываем меню настроек
        settings = InteractiveMenu.ConfigureSettings(projectInfo);
        if (settings == null)
        {
            Logger.Log("Экспорт отменён пользователем");
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
            return;
        }

        // Сканируем заново с выбранными настройками
        Console.WriteLine("⏳ Сканирование проекта с выбранными настройками...");
        projectInfo = scanner.ScanProject(projectPath, settings);
    }
    else
    {
        // CLI режим - используем настройки из аргументов
        // Если settings не был создан (только путь указан), создаем по умолчанию
        if (settings == null)
        {
            settings = new ExportSettings();

            // Автоопределение типа для CLI режима
            var autoDetectedType = ProjectTypeDetector.Detect(projectPath);
            switch (autoDetectedType)
            {
                case ProjectType.Python:
                    settings.ApplyPythonProjectPreset();
                    Console.WriteLine($"ℹ️  Определен тип проекта: Python");
                    break;
                case ProjectType.WebApp:
                    settings.ApplyWebAppPreset();
                    Console.WriteLine($"ℹ️  Определен тип проекта: Веб-приложение");
                    break;
                case ProjectType.CSharp:
                default:
                    Console.WriteLine($"ℹ️  Определен тип проекта: C#");
                    break;
            }
        }

        Console.WriteLine("⏳ Сканирование проекта...");
        projectInfo = scanner.ScanProject(projectPath, settings);

        // Показываем статистику
        Console.WriteLine($"📦 Проект: {projectInfo.ProjectName}");
        Console.WriteLine($"📊 Файлов: {projectInfo.TotalFiles}");
        Console.WriteLine($"⚙️  Режим: {settings.Mode}");
        if (settings.ExcludePatterns.Any())
        {
            Console.WriteLine($"🚫 Исключения: {string.Join(", ", settings.ExcludePatterns)}");
        }
        if (settings.IncludeOnlyPatterns.Any())
        {
            Console.WriteLine($"✅ Только: {string.Join(", ", settings.IncludeOnlyPatterns)}");
        }
        Console.WriteLine();
    }

    // Проверка на пустой экспорт (после сканирования с финальными настройками)
    if (!projectInfo.Files.Any())
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
            Console.WriteLine($"   • Попробуйте выбрать другой preset или режим 'All'");
        }

        if (settings.ExcludePatterns.Any())
        {
            Console.WriteLine($"   • Паттерны исключения слишком широкие: {string.Join(", ", settings.ExcludePatterns)}");
        }

        Console.WriteLine();
        Console.WriteLine("🔧 Что делать:");
        Console.WriteLine("   1. Запустите приложение снова");
        Console.WriteLine("   2. Выберите другой тип проекта");
        Console.WriteLine("   3. Используйте CLI режим с явным указанием типа (--web-app, --mode=python)");
        Console.WriteLine();

        Logger.LogWarning($"Экспорт отменён - нет файлов после фильтрации. " +
                          $"Всего: {projectInfo.TotalScannedFiles}, Исключено: {projectInfo.ExcludedFiles.Count}");

        Console.WriteLine("Нажмите любую клавишу для выхода...");
        Console.ReadKey();
        return;
    }

    // Экспортируем
    Console.WriteLine("⏳ Создание экспорта...");
    var result = exporter.ExportProject(projectInfo, settings);
    Console.WriteLine();

    // Показываем результат
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
}

Logger.Log("=== Приложение завершено ===");

if (!hasCliArgs)
{
    Console.WriteLine();
    Console.WriteLine("Нажмите любую клавишу для выхода...");
    Console.ReadKey();
}

// ============================================================================
// ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
// ============================================================================

static (ExportSettings settings, string projectPath, bool showHelp, bool hasCliArgs) ParseArguments(string[] args)
{
    ExportSettings settings = null;
    string projectPath = null;
    bool showHelp = false;
    bool hasCliArgs = args.Length > 1 || (args.Length == 1 && args[0].StartsWith("--"));

    for (int i = 0; i < args.Length; i++)
    {
        string arg = args[i];

        // Помощь
        if (arg == "--help" || arg == "-h" || arg == "/?")
        {
            showHelp = true;
            return (null, null, true, true);
        }

        // Путь к проекту (первый аргумент без --)
        if (!arg.StartsWith("--") && !arg.StartsWith("-") && string.IsNullOrEmpty(projectPath))
        {
            projectPath = arg.Trim('"');
            continue;
        }

        // Создаём settings если ещё не создан
        if (settings == null)
        {
            settings = new ExportSettings();
        }

        // Режим экспорта
        if (arg.StartsWith("--mode="))
        {
            string mode = arg.Substring(7).ToLower();
            settings.Mode = mode switch
            {
                "compact" => ExportMode.Compact,
                "balanced" => ExportMode.Balanced,
                "full" => ExportMode.Full,
                _ => ExportMode.Balanced
            };
        }
        // Формат
        else if (arg.StartsWith("--format="))
        {
            string format = arg.Substring(9).ToLower();
            settings.Format = format switch
            {
                "markdown" or "md" => ExportFormat.Markdown,
                "text" or "txt" or "plain" => ExportFormat.PlainText,
                _ => ExportFormat.Markdown
            };
        }
        // Паттерны исключения
        else if (arg.StartsWith("--exclude="))
        {
            string pattern = arg.Substring(10);
            settings.ExcludePatterns.Add(pattern);
        }
        // Паттерны включения
        else if (arg.StartsWith("--include-only=") || arg.StartsWith("--include="))
        {
            int startIndex = arg.StartsWith("--include-only=") ? 15 : 10;
            string pattern = arg.Substring(startIndex);
            settings.IncludeOnlyPatterns.Add(pattern);
        }
        // Preset'ы
        else if (arg == "--backend-only")
        {
            settings.ApplyBackendOnlyPreset();
        }
        else if (arg == "--domain-services" || arg == "--business-logic")
        {
            settings.ApplyDomainServicesPreset();
        }
        else if (arg == "--compact-aggressive")
        {
            settings.ApplyCompactAggressivePreset();
        }
        else if (arg == "--web-app" || arg == "--static-web")
        {
            settings.ApplyWebAppPreset();
        }
        // Флаги
        else if (arg == "--no-comments")
        {
            settings.RemoveComments = true;
        }
        else if (arg == "--keep-empty-lines")
        {
            settings.RemoveEmptyLines = false;
        }
        else if (arg == "--no-consolidate-usings")
        {
            settings.ConsolidateUsings = false;
        }
        else if (arg == "--collapse-threshold" && i + 1 < args.Length)
        {
            if (int.TryParse(args[i + 1], out int threshold))
            {
                settings.MethodCollapseThreshold = threshold;
                i++; // Пропускаем следующий аргумент
            }
        }
        // Выходная папка
        else if (arg.StartsWith("--output=") || arg.StartsWith("-o="))
        {
            int startIndex = arg.StartsWith("--output=") ? 9 : 3;
            string outputPath = arg.Substring(startIndex).Trim('"');
            if (Directory.Exists(outputPath))
            {
                settings.OutputDirectory = outputPath;
            }
        }
    }

    return (settings, projectPath, showHelp, hasCliArgs);
}

static void ShowHelp()
{
    Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║  📦 LLM Code Exporter v2.0 - Enhanced Edition                 ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
    Console.WriteLine();
    Console.WriteLine("ИСПОЛЬЗОВАНИЕ:");
    Console.WriteLine("  LLMCodeExporter.exe <путь-к-проекту> [опции]");
    Console.WriteLine();
    Console.WriteLine("РЕЖИМЫ ЭКСПОРТА:");
    Console.WriteLine("  --mode=compact      Только структура и сигнатуры (~30% размера, ~30K токенов)");
    Console.WriteLine("  --mode=balanced     Оптимизация больших методов (~50-70%, ~50-70K токенов) [по умолчанию]");
    Console.WriteLine("  --mode=full         Весь код без изменений (100%, может быть 100K+ токенов)");
    Console.WriteLine();
    Console.WriteLine("ФИЛЬТРАЦИЯ:");
    Console.WriteLine("  --exclude=pattern   Исключить файлы по паттерну (можно указать несколько раз)");
    Console.WriteLine("                      Примеры: *.Designer.cs, Forms, *.g.cs, UI");
    Console.WriteLine("  --include=pattern   Включить ТОЛЬКО файлы по паттерну");
    Console.WriteLine("                      Примеры: Domain, Services, Core");
    Console.WriteLine();
    Console.WriteLine("PRESET'Ы:");
    Console.WriteLine("  --backend-only      Исключить UI файлы (Forms, Views, Designer, xaml, razor)");
    Console.WriteLine("  --domain-services   Только бизнес-логика (Domain, Services, Application, Core)");
    Console.WriteLine("  --compact-aggressive Максимальное сжатие (compact + удаление комментариев)");
    Console.WriteLine("  --web-app           Веб-приложение (JS, CSS, HTML файлы)");
    Console.WriteLine();
    Console.WriteLine("ДОПОЛНИТЕЛЬНЫЕ ОПЦИИ:");
    Console.WriteLine("  --format=markdown   Формат вывода: markdown [по умолчанию] или text");
    Console.WriteLine("  --no-comments       Удалить комментарии из кода");
    Console.WriteLine("  --keep-empty-lines  Сохранить пустые строки (по умолчанию удаляются)");
    Console.WriteLine("  --collapse-threshold N  Порог строк для сворачивания методов [50]");
    Console.WriteLine("  --output=path       Путь для сохранения результата");
    Console.WriteLine("  --help, -h, /?      Показать эту справку");
    Console.WriteLine();
    Console.WriteLine("ПРИМЕРЫ:");
    Console.WriteLine();
    Console.WriteLine("  1. Интерактивный режим (по умолчанию):");
    Console.WriteLine("     LLMCodeExporter.exe");
    Console.WriteLine();
    Console.WriteLine("  2. Drag & Drop:");
    Console.WriteLine("     Перетащите папку с проектом на .exe файл");
    Console.WriteLine();
    Console.WriteLine("  3. Compact режим для быстрого обзора:");
    Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyApp\" --mode=compact");
    Console.WriteLine();
    Console.WriteLine("  4. Backend анализ (без UI):");
    Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyApp\" --backend-only");
    Console.WriteLine();
    Console.WriteLine("  5. Только Domain и Services:");
    Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyApp\" --include=Domain --include=Services");
    Console.WriteLine();
    Console.WriteLine("  6. Исключить Designer файлы и Forms:");
    Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyApp\" --exclude=*.Designer.cs --exclude=Forms");
    Console.WriteLine();
    Console.WriteLine("  7. Максимальное сжатие для очень большого проекта:");
    Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\LargeApp\" --compact-aggressive");
    Console.WriteLine();
    Console.WriteLine("  8. Веб-приложение (JS, CSS, HTML):");
    Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyWebApp\" --web-app");
    Console.WriteLine();
    Console.WriteLine("  9. Комбинирование опций:");
    Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyApp\" --mode=balanced --backend-only --no-comments");
    Console.WriteLine();
    Console.WriteLine("РЕКОМЕНДАЦИИ ПО РАЗМЕРУ:");
    Console.WriteLine("  • < 8K токенов    - Full режим, подходит для всех LLM");
    Console.WriteLine("  • 8-32K токенов   - Balanced, подходит для GPT-4, Claude 3");
    Console.WriteLine("  • 32-100K токенов - Balanced + фильтрация, GPT-4 Turbo, Claude 3.5");
    Console.WriteLine("  • > 100K токенов  - Compact или разбиение на модули");
    Console.WriteLine();
}

static string NormalizeProjectPath(string path)
{
    if (string.IsNullOrEmpty(path))
        return path;

    // Убираем кавычки
    path = path.Trim().Trim('"');
    Logger.Log($"Нормализация пути: {path}");

    // Если путь к файлу .csproj, берем его директорию
    if (File.Exists(path) && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
    {
        string directory = Path.GetDirectoryName(path);
        if (Directory.Exists(directory))
        {
            Logger.Log($"Путь к файлу .csproj нормализован: {path} -> {directory}");
            return directory;
        }
    }

    // Если путь к существующему файлу, берем директорию
    if (File.Exists(path))
    {
        string directory = Path.GetDirectoryName(path);
        if (Directory.Exists(directory))
        {
            Logger.Log($"Путь к файлу нормализован: {path} -> {directory}");
            return directory;
        }
    }

    // Если путь не существует, пытаемся найти родительскую директорию
    if (!Directory.Exists(path) && !File.Exists(path))
    {
        // Проверяем, может быть это относительный путь
        string fullPath = Path.GetFullPath(path);
        if (Directory.Exists(fullPath))
        {
            Logger.Log($"Относительный путь нормализован: {path} -> {fullPath}");
            return fullPath;
        }

        // Пытаемся найти родительскую директорию
        string parentDir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
        {
            Logger.Log($"Путь нормализован к родительской директории: {path} -> {parentDir}");
            return parentDir;
        }
    }

    return path;
}