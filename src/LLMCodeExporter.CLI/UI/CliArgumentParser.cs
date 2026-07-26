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
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Utils;
namespace LLMCodeExporter.CLI.UI;
/// <summary>
/// Парсер аргументов командной строки
/// </summary>
public static class CliArgumentParser
{
    /// <summary>
    /// Результат парсинга аргументов
    /// </summary>
    public class ParseResult
    {
        public ExportSettings? Settings { get; set; }
        public string? ProjectPath { get; set; }
        public bool ShowHelp { get; set; }
        public bool HasCliArgs { get; set; }
        public bool HasErrors { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Парсит аргументы командной строки
    /// </summary>
    public static ParseResult Parse(string[] args)
    {
        var result = new ParseResult
        {
            Settings = null,
            ProjectPath = null,
            ShowHelp = false,
            HasCliArgs = args.Length > 1 || (args.Length == 1 && args[0].StartsWith("--")),
            HasErrors = false
        };
        if (args.Length == 0)
            return result;
        ExportSettings? settings = null;
        string? projectPath = null;
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            // --help
            if (arg == "--help" || arg == "-h" || arg == "/?")
            {
                result.ShowHelp = true;
                return result;
            }

            // Путь к проекту (первый аргумент без флага)
            if (!arg.StartsWith("--") && !arg.StartsWith("-") && string.IsNullOrEmpty(projectPath))
            {
                projectPath = arg.Trim('"');
                continue;
            }

            // Инициализируем настройки при первом флаге
            if (settings == null)
                settings = new ExportSettings();
            // ---- НОВОЕ: обработка --exclude-file ----
            if (TryParseExcludeFile(arg, settings, ref i, args))
                continue;
            // Обработка остальных флагов
            if (!TryParseMode(arg, settings) &&
                !TryParseFormat(arg, settings) &&
                !TryParseExclude(arg, settings) &&
                !TryParseInclude(arg, settings) &&
                !TryParsePreset(arg, settings) &&
                !TryParseHybridLanguage(arg, settings) &&
                !TryParseOutput(arg, settings, ref i) &&
                !TryParseCollapseThreshold(arg, settings, args, ref i) &&
                !TryParseBooleanFlag(arg, settings))
            {
                // Неизвестный флаг
                result.HasErrors = true;
                result.ErrorMessage = $"Неизвестный аргумент: {arg}";
                return result;
            }
        }

        result.Settings = settings;
        result.ProjectPath = projectPath;
        return result;
    }

    #region Парсеры отдельных флагов
    private static bool TryParseMode(string arg, ExportSettings settings)
    {
        if (!arg.StartsWith("--mode="))
            return false;
        string mode = arg.Substring(7).ToLower();
        settings.Mode = mode switch
        {
            "compact" => ExportMode.Compact,
            "balanced" => ExportMode.Balanced,
            "full" => ExportMode.Full,
            _ => ExportMode.Balanced
        };
        return true;
    }

    private static bool TryParseFormat(string arg, ExportSettings settings)
    {
        if (!arg.StartsWith("--format="))
            return false;
        string format = arg.Substring(9).ToLower();
        settings.Format = format switch
        {
            "markdown" or "md" => ExportFormat.Markdown,
            "text" or "txt" or "plain" => ExportFormat.PlainText,
            "json" => ExportFormat.Json,
            "md+json" or "markdown+json" => ExportFormat.MarkdownWithJson,
            _ => ExportFormat.Markdown
        };
        return true;
    }

    private static bool TryParseExclude(string arg, ExportSettings settings)
    {
        if (!arg.StartsWith("--exclude="))
            return false;
        string pattern = arg.Substring(10);
        settings.ExcludePatterns.Add(pattern);
        return true;
    }

    private static bool TryParseInclude(string arg, ExportSettings settings)
    {
        if (!arg.StartsWith("--include-only=") && !arg.StartsWith("--include="))
            return false;
        int startIndex = arg.StartsWith("--include-only=") ? 15 : 10;
        string pattern = arg.Substring(startIndex);
        settings.IncludeOnlyPatterns.Add(pattern);
        return true;
    }

    private static bool TryParsePreset(string arg, ExportSettings settings)
    {
        switch (arg)
        {
            case "--backend-only":
                settings.ApplyBackendOnlyPreset();
                return true;
            case "--domain-services":
            case "--business-logic":
                settings.ApplyDomainServicesPreset();
                return true;
            case "--compact-aggressive":
                settings.ApplyCompactAggressivePreset();
                return true;
            case "--web-app":
            case "--static-web":
                settings.ApplyWebAppPreset();
                return true;
            case "--hybrid":
                settings.ProjectType = ProjectType.Hybrid;
                return true;
            default:
                return false;
        }
    }

    private static bool TryParseHybridLanguage(string arg, ExportSettings settings)
    {
        if (arg.StartsWith("--backend="))
        {
            string langStr = arg.Substring(10);
            if (Enum.TryParse<Language>(langStr, true, out var lang))
            {
                settings.BackendLanguage = lang;
                if (settings.ProjectType == ProjectType.AutoDetect)
                    settings.ProjectType = ProjectType.Hybrid;
                return true;
            }
            return false;
        }

        if (arg.StartsWith("--frontend="))
        {
            string langStr = arg.Substring(11);
            if (Enum.TryParse<Language>(langStr, true, out var lang))
            {
                settings.FrontendLanguage = lang;
                if (settings.ProjectType == ProjectType.AutoDetect)
                    settings.ProjectType = ProjectType.Hybrid;
                return true;
            }
            return false;
        }

        return false;
    }

    private static bool TryParseOutput(string arg, ExportSettings settings, ref int index)
    {
        if (!arg.StartsWith("--output=") && !arg.StartsWith("-o="))
            return false;
        int startIndex = arg.StartsWith("--output=") ? 9 : 3;
        string outputPath = arg.Substring(startIndex).Trim('"');
        if (Directory.Exists(outputPath))
            settings.OutputDirectory = outputPath;
        return true;
    }

    private static bool TryParseCollapseThreshold(string arg, ExportSettings settings, string[] args, ref int index)
    {
        if (arg != "--collapse-threshold")
            return false;
        if (index + 1 < args.Length && int.TryParse(args[index + 1], out int threshold))
        {
            settings.MethodCollapseThreshold = threshold;
            index++;
            return true;
        }
        return false;
    }

    private static bool TryParseBooleanFlag(string arg, ExportSettings settings)
    {
        switch (arg)
        {
            case "--no-comments":
                settings.RemoveComments = true;
                return true;
            case "--keep-empty-lines":
                settings.RemoveEmptyLines = false;
                return true;
            case "--no-consolidate-usings":
                settings.ConsolidateUsings = false;
                return true;
            default:
                return false;
        }
    }

    #endregion
    #region НОВАЯ ФУНКЦИЯ – Парсинг --exclude-file
    /// <summary>
    /// Обрабатывает аргумент --exclude-file=путь_к_файлу
    /// Загружает паттерны исключений из указанного файла (по одному на строку).
    /// </summary>
    private static bool TryParseExcludeFile(string arg, ExportSettings settings, ref int index, string[] args)
    {
        if (!arg.StartsWith("--exclude-file="))
            return false;
        string filePath = arg.Substring(15).Trim('"');
        if (!File.Exists(filePath))
        {
            Logger.LogWarning($"Файл исключений не найден: {filePath}");
            return true; // считаем, что аргумент обработан, но без добавления паттернов
        }

        try
        {
            var lines = File.ReadAllLines(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("#"))
                .Select(line => line.Trim())
                .ToList();
            if (lines.Any())
            {
                settings.ExcludePatterns.AddRange(lines);
                Logger.Log($"Загружено {lines.Count} паттернов из {filePath}");
            }
            else
            {
                Logger.LogWarning($"Файл {filePath} не содержит паттернов (или только комментарии)");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка чтения файла исключений: {filePath}", ex);
        }

        return true;
    }

    #endregion
}