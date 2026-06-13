/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.CLI.UI;
using System;
using System.IO;
using System.Linq;
using LLMCodeExporter.Core.Models;
public static class InteractiveMenu
{
    public static ExportSettings ConfigureSettings(ProjectInfo projectInfo)
    {
        var settings = new ExportSettings();
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║     ⚙️  НАСТРОЙКА ЭКСПОРТА v2.0       ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine();

        // 0. Тип проекта
        Console.WriteLine("0️⃣  Тип проекта:");
        Console.WriteLine("   [1] C# / .NET");
        Console.WriteLine("   [2] Python");
        Console.WriteLine("   [3] Веб-приложение (JS, CSS, HTML)");
        int projectTypeChoice = InputHelper.ReadChoice("\n   Выбор", 1, 3, 1);

        switch (projectTypeChoice)
        {
            case 1:
                settings.ProjectType = ProjectType.CSharp;
                settings.FileExtensions = new[] { "*.cs" };
                Console.WriteLine("   ✓ Тип проекта: C# / .NET");
                break;
            case 2:
                settings.ProjectType = ProjectType.Python;
                settings.ApplyPythonProjectPreset();
                Console.WriteLine("   ✓ Тип проекта: Python");
                Console.WriteLine($"   ✓ Расширения: {string.Join(", ", settings.FileExtensions)}");
                break;
            case 3:
                settings.ProjectType = ProjectType.WebApp;
                settings.ApplyWebAppPreset();
                Console.WriteLine("   ✓ Тип проекта: Веб-приложение");
                Console.WriteLine($"   ✓ Расширения: {string.Join(", ", settings.FileExtensions)}");
                Console.WriteLine($"   ✓ Сборка: JS, CSS, HTML файлы");
                break;
        }

        Console.WriteLine();

        // 1. Режим экспорта
        Console.WriteLine("1️⃣  Режим экспорта:");
        Console.WriteLine("   [1] 🎯 Compact - только структура и сигнатуры (~30% размера)");
        Console.WriteLine("   [2] ⚖️  Balanced - оптимизация больших методов (~50-70% размера) [рекомендуется]");
        Console.WriteLine("   [3] 📦 Full - весь код без изменений (100% размера)");
        int modeChoice = InputHelper.ReadChoice("\n   Выбор", 1, 3, 2);
        settings.Mode = modeChoice switch
        {
            1 => ExportMode.Compact,
            2 => ExportMode.Balanced,
            3 => ExportMode.Full,
            _ => ExportMode.Balanced
        };
        Console.WriteLine($"   ✓ Выбран режим: {settings.Mode}");

        Console.WriteLine();

        // 2. Настройка фильтров
        Console.WriteLine("2️⃣  Дополнительная фильтрация:");

        Console.WriteLine("   Предустановки:");
        Console.WriteLine("   [1] Без фильтров (все файлы)");
        Console.WriteLine("   [2] Backend Only (исключить UI файлы)");
        Console.WriteLine("   [3] Domain + Services (только бизнес-логика)");
        Console.WriteLine("   [4] Максимальное сжатие (compact + удаление комментариев)");
        int filterPreset = InputHelper.ReadChoice("\n   Выбор предустановки", 1, 4, 1);

        switch (filterPreset)
        {
            case 2:
                settings.ApplyBackendOnlyPreset();
                Console.WriteLine("   ✓ Применен: Backend Only (без UI)");
                break;
            case 3:
                settings.ApplyDomainServicesPreset();
                Console.WriteLine("   ✓ Применен: Domain + Services (бизнес-логика)");
                break;
            case 4:
                settings.ApplyCompactAggressivePreset();
                Console.WriteLine("   ✓ Применен: Максимальное сжатие");
                break;
            default:
                Console.WriteLine("   ✓ Без фильтров");
                break;
        }

        // Кастомные фильтры
        if (filterPreset == 1 && InputHelper.ReadYesNo("\n   Добавить кастомные фильтры?", defaultValue: false))
        {
            ConfigureCustomFilters(settings);
        }

        Console.WriteLine();

        // 3. Формат экспорта
        Console.WriteLine("3️⃣  Формат экспорта:");
        Console.WriteLine("   [1] Markdown (рекомендуется для ChatGPT/Claude)");
        Console.WriteLine("   [2] Plain Text (обычный текст)");
        int formatChoice = InputHelper.ReadChoice("\n   Выбор", 1, 2, 1);
        settings.Format = formatChoice == 2 ? ExportFormat.PlainText : ExportFormat.Markdown;
        Console.WriteLine($"   ✓ Выбран формат: {settings.Format}");

        Console.WriteLine();

        // 4. Дополнительные оптимизации
        if (settings.Mode != ExportMode.Full)
        {
            Console.WriteLine("4️⃣  Дополнительные оптимизации:");

            if (filterPreset != 4) // Если не выбран агрессивный режим
            {
                Console.WriteLine("   • Удалить пустые строки?");
                settings.RemoveEmptyLines = InputHelper.ReadYesNo("     ", defaultValue: true);

                Console.WriteLine("   • Удалить комментарии? (⚠ может удалить важную документацию)");
                settings.RemoveComments = InputHelper.ReadYesNo("     ", defaultValue: false);
            }
            else
            {
                Console.WriteLine("   ⚠ В агрессивном режиме уже включены:");
                Console.WriteLine("     • Удаление пустых строк: Да");
                Console.WriteLine("     • Удаление комментариев: Да");
                Console.WriteLine("     • Сворачивание методов: Да (порог: 30 строк)");
            }
        }

        // Итоговая сводка с предпросмотром
        ShowConfigurationSummary(settings, projectInfo);

        if (!InputHelper.ReadYesNo("\nПродолжить с этими настройками?", defaultValue: true))
        {
            Console.WriteLine("\n⚠ Экспорт отменён пользователем.");
            return null;
        }

        Console.WriteLine();
        return settings;
    }

    private static void ConfigureCustomFilters(ExportSettings settings)
    {
        Console.WriteLine("\n   🔧 Настройка кастомных фильтров:");

        Console.WriteLine("   Добавить паттерны исключения (Enter для пропуска):");
        Console.WriteLine("   Примеры: *.Designer.cs, Forms, *.g.cs, UI, *.min.js");
        Console.Write("   > ");
        string excludeInput = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(excludeInput))
        {
            var patterns = excludeInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());
            settings.ExcludePatterns.AddRange(patterns);
        }

        Console.WriteLine("\n   Добавить паттерны включения (Enter для пропуска):");
        Console.WriteLine("   Примеры: Domain, Services, Core, *.cs, *.py");
        Console.Write("   > ");
        string includeInput = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(includeInput))
        {
            var patterns = includeInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());
            settings.IncludeOnlyPatterns.AddRange(patterns);
        }
    }

    private static void ShowConfigurationSummary(ExportSettings settings, ProjectInfo projectInfo)
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║     📋 ИТОГОВАЯ КОНФИГУРАЦИЯ          ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine($"  Тип проекта:          {settings.ProjectType}");
        Console.WriteLine($"  Режим:                {settings.Mode}");
        Console.WriteLine($"  Формат:               {settings.Format}");
        Console.WriteLine($"  Расширения:           {string.Join(", ", settings.FileExtensions)}");

        if (settings.ExcludePatterns.Any())
        {
            Console.WriteLine($"  Исключения:           {string.Join(", ", settings.ExcludePatterns)}");
        }

        if (settings.IncludeOnlyPatterns.Any())
        {
            Console.WriteLine($"  Включены только:      {string.Join(", ", settings.IncludeOnlyPatterns)}");
        }

        if (settings.Mode != ExportMode.Full)
        {
            Console.WriteLine($"  Удалить пустые строки: {(settings.RemoveEmptyLines ? "Да" : "Нет")}");
            Console.WriteLine($"  Удалить комментарии:   {(settings.RemoveComments ? "Да" : "Нет")}");
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  📊 Оценка результата:");
        Console.WriteLine($"     Оригинал: ~{projectInfo.EstimatedTokens:N0} токенов");

        double compressionFactor = settings.Mode switch
        {
            ExportMode.Compact => 0.3,
            ExportMode.Balanced => 0.6,
            _ => 1.0
        };

        long estimatedTokens = (long)(projectInfo.EstimatedTokens * compressionFactor);
        Console.WriteLine($"     После оптимизации: ~{estimatedTokens:N0} токенов ({compressionFactor:P0})");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void ShowProjectStats(ProjectInfo projectInfo)
    {
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║     📊 СТАТИСТИКА ПРОЕКТА             ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine($"  Проект:      {projectInfo.ProjectName}");
        Console.WriteLine($"  Файлов:      {projectInfo.TotalFiles}");
        Console.WriteLine($"  Символов:    {projectInfo.TotalCharacters:N0}");
        Console.WriteLine($"  Токенов:     ~{projectInfo.EstimatedTokens:N0}");
        Console.WriteLine();
        ShowTokenWarnings(projectInfo.EstimatedTokens);
    }

    private static void ShowTokenWarnings(long estimatedTokens)
    {
        if (estimatedTokens > 200000)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ⚠️  ВНИМАНИЕ: Проект очень большой!");
            Console.WriteLine("      Превышены лимиты большинства LLM.");
            Console.WriteLine("      Рекомендуется: Compact режим + фильтрация");
            Console.ResetColor();
        }
        else if (estimatedTokens > 128000)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ⚠️  Проект большой.");
            Console.WriteLine("      Рекомендуется: Balanced режим или фильтрация");
            Console.WriteLine("      Подходит: Claude 3.5 Sonnet (200K), Gemini 1.5 Pro");
            Console.ResetColor();
        }
        else if (estimatedTokens > 8000)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ℹ️  Проект среднего размера.");
            Console.WriteLine("      Подходит для большинства LLM через API.");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ Размер оптимален для всех LLM.");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    public static void ShowExportResult(ExportResult result)
    {
        if (result.Success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║  ✓ ЭКСПОРТ ЗАВЕРШЕН УСПЕШНО           ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            var metadata = result.ProjectInfo.Metadata;
            Console.WriteLine($"📊 Файлов обработано: {metadata.IncludedFiles}");
            if (metadata.ExcludedFiles > 0)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"   (исключено: {metadata.ExcludedFiles} файлов)");
                Console.ResetColor();
            }

            Console.WriteLine($"📊 Оригинальный размер: ~{metadata.OriginalEstimatedTokens:N0} токенов");
            Console.WriteLine($"📊 Результат: ~{metadata.EstimatedTokens:N0} токенов");

            if (metadata.Mode != ExportMode.Full)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"📊 Степень сжатия: {metadata.CompressionRatio:P0}");
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine($"💾 Результат сохранен:");
            Console.WriteLine($"   {result.OutputFilePath}");

            if (result.Errors.Any())
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ Предупреждений: {result.Errors.Count}");
                foreach (var error in result.Errors.Take(3))
                {
                    Console.WriteLine($"  • {error}");
                }
                if (result.Errors.Count > 3)
                {
                    Console.WriteLine($"  ... и ещё {result.Errors.Count - 3}");
                }
                Console.ResetColor();
            }

            string appFolder = LLMCodeExporter.Infrastructure.Utils.Logger.GetAppFolderPath();
            string currentLog = LLMCodeExporter.Infrastructure.Utils.Logger.GetCurrentLogFile();
            Console.WriteLine();
            Console.WriteLine("📁 Файлы приложения:");
            Console.WriteLine($"   {appFolder}");
            Console.WriteLine($"   ├── 📂 Exports\\     (экспорты проектов)");
            Console.WriteLine($"   └── 📂 Logs\\        (логи работы)");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"📝 Лог этой сессии:");
            Console.WriteLine($"   {Path.GetFileName(currentLog)}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║  ✗ ОШИБКА ЭКСПОРТА                    ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                Console.WriteLine($"Сообщение: {result.Message}");
            }
            else
            {
                Console.WriteLine("Сообщение: Неизвестная ошибка (см. лог)");
            }

            if (result.Errors.Any())
            {
                Console.WriteLine("\nДетали:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"  • {error}");
                }
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"📝 Подробности в логе: {Path.GetFileName(LLMCodeExporter.Infrastructure.Utils.Logger.GetCurrentLogFile())}");
            Console.ResetColor();
        }
    }
}