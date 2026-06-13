/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Infrastructure.Utils;

using Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class ArchitectureAnalyzer
{
    /// <summary>
    /// Генерирует краткое описание архитектуры проекта на основе структуры файлов.
    /// </summary>
    public static string GenerateArchitectureOverview(ProjectInfo projectInfo, ProjectType projectType)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## 🏗️ Architecture Overview");
        sb.AppendLine();

        // Выбираем анализатор по типу проекта
        var layers = AnalyzeLayers(projectInfo.Files, projectType);

        if (projectType == ProjectType.WebApp)
        {
            sb.AppendLine("Проект представляет собой **веб-приложение** (JS, CSS, HTML):");

            if (layers.ContainsKey("HTML"))
            {
                var htmlFiles = layers["HTML"]
                    .Select(f => Path.GetFileNameWithoutExtension(f.RelativePath))
                    .Take(5);
                sb.AppendLine($"- **HTML Templates**: `{string.Join("`, `", htmlFiles)}`");
            }

            if (layers.ContainsKey("JavaScript"))
            {
                var jsCount = layers["JavaScript"].Count;
                sb.AppendLine($"- **JavaScript/TypeScript** ({jsCount} файлов) - клиентская логика");
            }

            if (layers.ContainsKey("CSS"))
            {
                var cssCount = layers["CSS"].Count;
                sb.AppendLine($"- **CSS/Styles** ({cssCount} файлов) - стилизация");
            }

            if (layers.ContainsKey("Assets"))
            {
                var assetsCount = layers["Assets"].Count;
                sb.AppendLine($"- **Assets/Images** ({assetsCount} файлов) - изображения и ресурсы");
            }

            if (layers.ContainsKey("Configuration"))
            {
                sb.AppendLine($"- **Configuration** - настройки проекта (package.json, configs)");
            }

            sb.AppendLine($"\n**Сборка расширений:** JS, CSS, HTML");
        }
        else if (layers.ContainsKey("Domain") && layers.ContainsKey("Application") && layers.ContainsKey("Infrastructure"))
        {
            sb.AppendLine("Проект следует **Clean Architecture** (многослойная архитектура):");

            if (layers.ContainsKey("Domain"))
            {
                var domainFiles = layers["Domain"]
                    .Select(f => Path.GetFileNameWithoutExtension(f.RelativePath))
                    .Take(5);
                sb.AppendLine($"- **Domain Layer** содержит бизнес-сущности: `{string.Join("`, `", domainFiles)}`");
            }

            if (layers.ContainsKey("Application"))
            {
                var serviceCount = layers["Application"].Count;
                sb.AppendLine($"- **Application/Services** реализуют бизнес-логику ({serviceCount} сервисов)");
            }

            if (layers.ContainsKey("Infrastructure"))
            {
                sb.AppendLine("- **Infrastructure** обрабатывает доступ к данным и внешние зависимости");
            }

            if (layers.ContainsKey("UI"))
            {
                var uiCount = layers["UI"].Count;
                sb.AppendLine($"- **Presentation/UI** содержит пользовательский интерфейс ({uiCount} файлов)");
            }

            if (layers.ContainsKey("Tests"))
            {
                var testCount = layers["Tests"].Count;
                sb.AppendLine($"- **Tests** включают модульные и интеграционные тесты ({testCount} файлов)");
            }
        }
        else if (projectInfo.Files.Any(f => f.RelativePath.IndexOf("CONTROLLER", System.StringComparison.OrdinalIgnoreCase) >= 0))
        {
            sb.AppendLine("Проект использует **MVC/Web API** архитектуру:");
            sb.AppendLine("- Controllers обрабатывают HTTP запросы");
            sb.AppendLine("- Models представляют данные");
            sb.AppendLine("- Services содержат бизнес-логику");
        }
        else
        {
            sb.AppendLine("Проект имеет **монолитную структуру** с разделением по папкам.");
        }

        sb.AppendLine();

        var patterns = DetectPatterns(projectInfo.Files);
        if (patterns.Any())
        {
            sb.AppendLine($"**Используемые паттерны:** {string.Join(", ", patterns)}");
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();

        return sb.ToString();
    }
    
    private static Dictionary<string, List<FileMetadata>> AnalyzeLayers(
        List<FileMetadata> files,
        ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.Python => AnalyzePythonLayers(files),
            ProjectType.WebApp => AnalyzeWebAppLayers(files),
            _ => AnalyzeDotNetLayers(files)
        };
    }

    private static Dictionary<string, List<FileMetadata>> AnalyzeWebAppLayers(List<FileMetadata> files)
    {
        var layers = new Dictionary<string, List<FileMetadata>>();

        foreach (var file in files)
        {
            // Нормализация разделителей и приведение к верхнему регистру
            var normalizedPath = NormalizePath(file.RelativePath).ToUpperInvariant();
            var extension = GetNormalizedExtension(file.RelativePath);

            // HTML файлы
            if (extension == ".HTML" || extension == ".HTM" || 
                normalizedPath.Contains(".VUE") || normalizedPath.Contains(".JSX") || 
                normalizedPath.Contains(".TSX"))
            {
                AddToLayer(layers, "HTML", file);
            }
            // JavaScript/TypeScript
            else if (extension == ".JS" || extension == ".JSX" || extension == ".TS" || extension == ".TSX")
            {
                AddToLayer(layers, "JavaScript", file);
            }
            // CSS/Styles
            else if (extension == ".CSS" || extension == ".SCSS" || extension == ".SASS" || extension == ".LESS")
            {
                AddToLayer(layers, "CSS", file);
            }
            // Assets/Images
            else if (extension == ".SVG" || extension == ".PNG" || extension == ".JPG" || 
                     extension == ".JPEG" || extension == ".GIF" || extension == ".ICO" || 
                     extension == ".WEBP")
            {
                AddToLayer(layers, "Assets", file);
            }
            // Configuration
            else if (normalizedPath.Contains("PACKAGE.JSON") || normalizedPath.Contains("CONFIG.") || 
                     normalizedPath.Contains("VITE.CONFIG") || normalizedPath.Contains("WEBPACK.CONFIG"))
            {
                AddToLayer(layers, "Configuration", file);
            }
            // JSON data
            else if (extension == ".JSON")
            {
                AddToLayer(layers, "Data", file);
            }
            else
            {
                AddToLayer(layers, "Other", file);
            }
        }

        return layers;
    }

    private static Dictionary<string, List<FileMetadata>> AnalyzeDotNetLayers(List<FileMetadata> files)
    {
        var layers = new Dictionary<string, List<FileMetadata>>();

        foreach (var file in files)
        {
            // Нормализация разделителей и приведение к верхнему регистру
            var normalizedPath = NormalizePath(file.RelativePath).ToUpperInvariant();

            if (HasPathSegment(normalizedPath, "DOMAIN") || HasPathSegment(normalizedPath, "MODELS") || 
                HasPathSegment(normalizedPath, "ENTITIES"))
            {
                AddToLayer(layers, "Domain", file);
            }
            else if (HasPathSegment(normalizedPath, "APPLICATION") || HasPathSegment(normalizedPath, "SERVICES"))
            {
                AddToLayer(layers, "Application", file);
            }
            else if (HasPathSegment(normalizedPath, "INFRASTRUCTURE") || HasPathSegment(normalizedPath, "REPOSITORIES") || 
                     HasPathSegment(normalizedPath, "DATA"))
            {
                AddToLayer(layers, "Infrastructure", file);
            }
            else if (HasPathSegment(normalizedPath, "FORMS") || HasPathSegment(normalizedPath, "VIEWS") || 
                     HasPathSegment(normalizedPath, "UI") || HasPathSegment(normalizedPath, "PAGES"))
            {
                AddToLayer(layers, "UI", file);
            }
            else if (HasPathSegment(normalizedPath, "CONTROLLERS") || HasPathSegment(normalizedPath, "API"))
            {
                AddToLayer(layers, "Controllers", file);
            }
            else if (HasPathSegment(normalizedPath, "TEST") || HasPathSegment(normalizedPath, "TESTS"))
            {
                AddToLayer(layers, "Tests", file);
            }
        }

        return layers;
    }

    private static Dictionary<string, List<FileMetadata>> AnalyzePythonLayers(List<FileMetadata> files)
    {
        var layers = new Dictionary<string, List<FileMetadata>>();

        foreach (var file in files)
        {
            // Нормализация разделителей и приведение к нижнему регистру
            var normalizedPath = NormalizePath(file.RelativePath).ToLowerInvariant();
            var fileName = Path.GetFileName(normalizedPath);

            // Tests
            if (normalizedPath.Contains("/tests/") || fileName.StartsWith("test_") || fileName.EndsWith("_test.py"))
            {
                AddToLayer(layers, "Tests", file);
                continue;
            }

            // Domain
            if (normalizedPath.Contains("/models/") || normalizedPath.Contains("/entities/") || 
                normalizedPath.Contains("/domain/"))
            {
                AddToLayer(layers, "Domain", file);
                continue;
            }

            // Application / Services / UseCases
            if (normalizedPath.Contains("/services/") || normalizedPath.Contains("/use_cases/") || 
                normalizedPath.Contains("/usecases/") || normalizedPath.Contains("/application/") || 
                normalizedPath.Contains("/logic/"))
            {
                AddToLayer(layers, "Application", file);
                continue;
            }

            // Infrastructure / Data / Repositories
            if (normalizedPath.Contains("/repositories/") || normalizedPath.Contains("/repository/") || 
                normalizedPath.Contains("/db/") || normalizedPath.Contains("/database/") || 
                normalizedPath.Contains("/infra/") || normalizedPath.Contains("/infrastructure/"))
            {
                AddToLayer(layers, "Infrastructure", file);
                continue;
            }

            // Presentation / API / Handlers / CLI / UI
            if (normalizedPath.Contains("/api/") || normalizedPath.Contains("/views/") || 
                normalizedPath.Contains("/handlers/") || normalizedPath.Contains("/routers/") || 
                normalizedPath.Contains("/endpoints/") || normalizedPath.Contains("/cli/") || 
                normalizedPath.Contains("/ui/"))
            {
                AddToLayer(layers, "UI", file);
                continue;
            }
        }

        return layers;
    }

    private static void AddToLayer(Dictionary<string, List<FileMetadata>> layers, string layer, FileMetadata file)
    {
        if (!layers.ContainsKey(layer))
            layers[layer] = new List<FileMetadata>();

        layers[layer].Add(file);
    }

    private static List<string> DetectPatterns(List<FileMetadata> files)
    {
        var patterns = new List<string>();

        if (files.Any(f => NormalizePath(f.RelativePath).IndexOf("REPOSITORY", System.StringComparison.OrdinalIgnoreCase) >= 0))
            patterns.Add("Repository");

        if (files.Any(f => NormalizePath(f.RelativePath).IndexOf("SERVICE", System.StringComparison.OrdinalIgnoreCase) >= 0))
            patterns.Add("Service Layer");

        if (files.Any(f => NormalizePath(f.RelativePath).IndexOf("FACTORY", System.StringComparison.OrdinalIgnoreCase) >= 0))
            patterns.Add("Factory");

        if (files.Any(f => NormalizePath(f.RelativePath).IndexOf("SINGLETON", System.StringComparison.OrdinalIgnoreCase) >= 0))
            patterns.Add("Singleton");

        if (files.Any(f => NormalizePath(f.RelativePath).IndexOf("OBSERVER", System.StringComparison.OrdinalIgnoreCase) >= 0))
            patterns.Add("Observer");

        if (files.Any(f => Path.GetFileName(f.RelativePath).StartsWith("I") &&
                          files.Any(f2 => Path.GetFileNameWithoutExtension(f2.RelativePath) ==
                                          Path.GetFileNameWithoutExtension(f.RelativePath).Substring(1))))
            patterns.Add("Dependency Injection");

        return patterns;
    }

    #region Helper Methods

    /// <summary>
    /// Нормализует путь: заменяет все разделители на '/', приводит к единому формату.
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        return path.Replace('\\', '/');
    }

    /// <summary>
    /// Получает расширение файла в нормализованном виде (с точкой).
    /// </summary>
    private static string GetNormalizedExtension(string path)
    {
        var extension = Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension))
            return string.Empty;

        return extension.ToUpperInvariant();
    }

    /// <summary>
    /// Проверяет наличие сегмента пути (с разделителями с обеих сторон для точного совпадения).
    /// Это предотвращает ложные совпадения типа "DOMAIN" внутри "MYDOMAIN".
    /// </summary>
    private static bool HasPathSegment(string normalizedPath, string segment)
    {
        if (string.IsNullOrEmpty(normalizedPath) || string.IsNullOrEmpty(segment))
            return false;

        var fullMatch = $"/{segment}/";
        var edgeStartMatch = segment + "/";
        var edgeEndMatch = $"/{segment}";
        var exactMatch = $"/{segment}/";

        // Проверяем разные варианты расположения сегмента
        return normalizedPath.IndexOf(fullMatch, StringComparison.Ordinal) >= 0 ||
               normalizedPath.IndexOf(edgeStartMatch, StringComparison.Ordinal) >= 0 ||
               normalizedPath.IndexOf(edgeEndMatch, StringComparison.Ordinal) >= 0 ||
               normalizedPath == segment ||
               normalizedPath == $"/{segment}" ||
               normalizedPath == $"\\{segment}";
    }

    #endregion
}