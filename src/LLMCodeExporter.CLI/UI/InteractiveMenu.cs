/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
namespace LLMCodeExporter.CLI.UI;
using System;
using System.IO;
using System.Linq;
using LLMCodeExporter.Core.Models;

public static class InteractiveMenu
{
    public static ExportSettings? ConfigureSettings(ProjectInfo projectInfo)
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
        Console.WriteLine("   [4] Гибридный (выбор языков)");
        int projectTypeChoice = InputHelper.ReadChoice("\n   Выбор", 1, 4, 1);

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
                break;
            case 4:
                settings.ProjectType = ProjectType.Hybrid;
                // Запрашиваем бекенд-язык
                var backendLang = SelectLanguage("Выберите язык бекенда", Language.CSharp);
                settings.BackendLanguage = backendLang;
                // Запрашиваем фронтенд-язык
                var frontendLang = SelectLanguage("Выберите язык фронтенда", Language.JavaScript);
                settings.FrontendLanguage = frontendLang;
                settings.ApplyHybridPreset();
                Console.WriteLine($"   ✓ Тип проекта: Гибридный ({backendLang} + {frontendLang})");
                Console.WriteLine($"   ✓ Расширения: {string.Join(", ", settings.FileExtensions)}");
                break;
        }

        Console.WriteLine();

        // 1. Режим экспорта (остаётся без изменений)
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
            case 2: settings.ApplyBackendOnlyPreset(); Console.WriteLine("   ✓ Применен: Backend Only (без UI)"); break;
            case 3: settings.ApplyDomainServicesPreset(); Console.WriteLine("   ✓ Применен: Domain + Services (бизнес-логика)"); break;
            case 4: settings.ApplyCompactAggressivePreset(); Console.WriteLine("   ✓ Применен: Максимальное сжатие"); break;
            default: Console.WriteLine("   ✓ Без фильтров"); break;
        }

        if (filterPreset == 1 && InputHelper.ReadYesNo("\n   Добавить кастомные фильтры?", defaultValue: false))
            ConfigureCustomFilters(settings);

        Console.WriteLine();

        // 3. Формат экспорта 
        Console.WriteLine("3️⃣  Формат экспорта:");
        Console.WriteLine("   [1] Markdown (рекомендуется для ChatGPT/Claude)");
        Console.WriteLine("   [2] Plain Text (обычный текст)");
        Console.WriteLine("   [3] JSON (структурированный машинный формат)");
        Console.WriteLine("   [4] Markdown + JSON (отчёт + структурированные данные)");
        int formatChoice = InputHelper.ReadChoice("\n   Выбор", 1, 4, 1);
        settings.Format = formatChoice switch
        {
            2 => ExportFormat.PlainText,
            3 => ExportFormat.Json,
            4 => ExportFormat.MarkdownWithJson,
            _ => ExportFormat.Markdown
        };

        // 4. Дополнительные оптимизации
        if (settings.Mode != ExportMode.Full)
        {
            Console.WriteLine("4️⃣  Дополнительные оптимизации:");
            if (filterPreset != 4)
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

        // Итоговая сводка
        ShowConfigurationSummary(settings, projectInfo);
        if (!InputHelper.ReadYesNo("\nПродолжить с этими настройками?", defaultValue: true))
        {
            Console.WriteLine("\n⚠ Экспорт отменён пользователем.");
            return null;
        }
        return settings;
    }

    /// <summary>
    /// Вспомогательный метод для выбора языка из списка.
    /// </summary>
    private static Language SelectLanguage(string prompt, Language defaultLang)
    {
        Console.WriteLine($"\n{prompt}:");
        var languages = Enum.GetValues<Language>();
        for (int i = 0; i < languages.Length; i++)
        {
            Console.WriteLine($"   [{i + 1}] {languages[i]}");
        }
        int choice = InputHelper.ReadChoice($"   Выбор (по умолчанию {defaultLang})", 1, languages.Length, Array.IndexOf(languages, defaultLang) + 1);
        return languages[choice - 1];
    }

    private static void ConfigureCustomFilters(ExportSettings settings)
    {
        Console.WriteLine("\n   🔧 Настройка кастомных фильтров:");
        Console.WriteLine("   Добавить паттерны исключения (Enter для пропуска):");
        Console.Write("   > ");
        string? excludeInput = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(excludeInput))
        {
            var patterns = excludeInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());
            settings.ExcludePatterns.AddRange(patterns);
        }

        Console.WriteLine("\n   Добавить паттерны включения (Enter для пропуска):");
        Console.Write("   > ");
        string? includeInput = Console.ReadLine()?.Trim();
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
        if (settings.ProjectType == ProjectType.Hybrid)
        {
            Console.WriteLine($"  Бекенд язык:           {settings.BackendLanguage}");
            Console.WriteLine($"  Фронтенд язык:         {settings.FrontendLanguage}");
        }
        Console.WriteLine($"  Режим:                {settings.Mode}");
        Console.WriteLine($"  Формат:               {settings.Format}");
        Console.WriteLine($"  Расширения:           {string.Join(", ", settings.FileExtensions)}");
        if (settings.ExcludePatterns.Any())
            Console.WriteLine($"  Исключения:           {string.Join(", ", settings.ExcludePatterns)}");
        if (settings.IncludeOnlyPatterns.Any())
            Console.WriteLine($"  Включены только:      {string.Join(", ", settings.IncludeOnlyPatterns)}");
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

    // Остальные методы (ShowProjectStats, ShowExportResult) остаются без изменений
    public static void ShowProjectStats(ProjectInfo projectInfo) { /* ... */ }
    public static void ShowExportResult(ExportResult result) { /* ... */ }
}