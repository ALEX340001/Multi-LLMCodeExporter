/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.Linq;
using LLMCodeExporter.Core.Interfaces;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Services;
using LLMCodeExporter.Infrastructure.Utils;
namespace LLMCodeExporter.CLI.UI;
/// <summary>
/// Обработчик CLI режима работы
/// </summary>
public static class CliModeHandler
{
    /// <summary>
    /// Результат выполнения CLI режима
    /// </summary>
    public class CliResult
    {
        public bool Success { get; set; }
        public ProjectInfo? ProjectInfo { get; set; }
        public ExportSettings? Settings { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Выполняет экспорт в CLI режиме
    /// </summary>
    public static CliResult Execute(string projectPath, ExportSettings? settings)
    {
        var result = new CliResult();
        try
        {
            // Инициализация сервисов
            IFileScanner scanner = new FileScanner();
            ICodeProcessor processor = new CodeProcessor();
            IExportService exporter = new ExportService(processor);
            // Настройка параметров
            if (settings == null)
                settings = new ExportSettings();
            // Автоопределение типа проекта, если не задан явно
            if (settings.ProjectType == ProjectType.AutoDetect)
            {
                var detected = ProjectTypeDetector.Detect(projectPath);
                settings.ProjectType = detected;
                if (detected == ProjectType.Hybrid)
                {
                    ApplyHybridAutoDetection(settings, projectPath);
                }
                else
                {
                    ApplySingleLanguagePreset(settings, detected);
                }
            }
            else if (settings.ProjectType == ProjectType.Hybrid)
            {
                settings.ApplyHybridPreset();
                Console.WriteLine($"ℹ️  Гибридный режим: Бекенд={settings.BackendLanguage}, Фронтенд={settings.FrontendLanguage}");
            }

            // Сканирование проекта
            Console.WriteLine("⏳ Сканирование проекта...");
            var projectInfo = scanner.ScanProject(projectPath, settings);
            // Вывод информации
            PrintProjectInfo(projectInfo, settings);
            result.Success = true;
            result.ProjectInfo = projectInfo;
            result.Settings = settings;
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            Logger.LogError("Ошибка в CLI режиме", ex);
            return result;
        }
    }

    private static void ApplyHybridAutoDetection(ExportSettings settings, string projectPath)
    {
        var langs = ProjectTypeDetector.DetectLanguages(projectPath);
        if (langs.Count >= 2)
        {
            settings.BackendLanguage = langs[0];
            settings.FrontendLanguage = langs[1];
        }
        else if (langs.Count == 1)
        {
            settings.BackendLanguage = langs[0];
        }
        settings.ApplyHybridPreset();
        Console.WriteLine($"ℹ️  Гибридный режим: Бекенд={settings.BackendLanguage}, Фронтенд={settings.FrontendLanguage}");
    }

    private static void ApplySingleLanguagePreset(ExportSettings settings, ProjectType detected)
    {
        switch (detected)
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
                settings.FileExtensions = new[] { "*.cs" };
                Console.WriteLine($"ℹ️  Определен тип проекта: C#");
                break;
            case ProjectType.JavaScript:
                settings.FileExtensions = new[] { "*.js", "*.jsx", "*.json" };
                Console.WriteLine($"ℹ️  Определен тип проекта: JavaScript");
                break;
            case ProjectType.TypeScript:
                settings.FileExtensions = new[] { "*.ts", "*.tsx", "*.js", "*.json" };
                Console.WriteLine($"ℹ️  Определен тип проекта: TypeScript");
                break;
            default:
                Console.WriteLine($"ℹ️  Тип проекта: {detected} (автоопределен)");
                break;
        }
    }

    private static void PrintProjectInfo(ProjectInfo projectInfo, ExportSettings settings)
    {
        Console.WriteLine($"📦 Проект: {projectInfo.ProjectName}");
        Console.WriteLine($"📊 Файлов: {projectInfo.TotalFiles}");
        Console.WriteLine($"⚙️  Режим: {settings.Mode}");
        if (settings.ProjectType == ProjectType.Hybrid)
        {
            Console.WriteLine($"🔹 Бекенд язык: {settings.BackendLanguage}");
            Console.WriteLine($"🔸 Фронтенд язык: {settings.FrontendLanguage}");
        }

        if (settings.ExcludePatterns.Any())
            Console.WriteLine($"🚫 Исключения: {string.Join(", ", settings.ExcludePatterns)}");
        if (settings.IncludeOnlyPatterns.Any())
            Console.WriteLine($"✅ Только: {string.Join(", ", settings.IncludeOnlyPatterns)}");
        Console.WriteLine();
    }
}