/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Utils.Architecture.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture;

/// <summary>
/// Детектор слоёв архитектуры для разных типов проектов
/// </summary>
public static class LayerDetector
{
    public static Dictionary<string, LayerInfo> DetectLayers(List<FileMetadata> files, ProjectType projectType)
    {
        return projectType switch
        {
            ProjectType.Python => DetectPythonLayers(files),
            ProjectType.WebApp => DetectWebAppLayers(files),
            ProjectType.Hybrid => DetectHybridLayers(files, null, null),
            _ => DetectDotNetLayers(files)
        };
    }

    public static Dictionary<string, LayerInfo> DetectHybridLayers(List<FileMetadata> files, Language? backendLanguage, Language? frontendLanguage)
    {
        var layers = new Dictionary<string, LayerInfo>();

        var backendExtensions = backendLanguage.HasValue
            ? LanguageSettings.ForLanguage(backendLanguage.Value).FileExtensions
            : new[] { ".cs" };
        var frontendExtensions = frontendLanguage.HasValue
            ? LanguageSettings.ForLanguage(frontendLanguage.Value).FileExtensions
            : new[] { ".js", ".html", ".css" };

        var backendFiles = new List<FileMetadata>();
        var frontendFiles = new List<FileMetadata>();
        var configFiles = new List<FileMetadata>();

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.RelativePath).ToLowerInvariant();
            if (backendExtensions.Contains(ext))
                backendFiles.Add(file);
            else if (frontendExtensions.Contains(ext))
                frontendFiles.Add(file);
            else
                configFiles.Add(file);
        }

        layers["Backend"] = new LayerInfo
        {
            Name = "Backend",
            Files = backendFiles,
            Description = "Серверная логика, API, бизнес-слой"
        };

        layers["Frontend"] = new LayerInfo
        {
            Name = "Frontend",
            Files = frontendFiles,
            Description = "Пользовательский интерфейс, клиентские скрипты"
        };

        layers["Configuration"] = new LayerInfo
        {
            Name = "Configuration",
            Files = configFiles,
            Description = "Конфигурационные файлы, настройки"
        };

        return layers;
    }

    // ========== .NET / C# ==========
    public static Dictionary<string, LayerInfo> DetectDotNetLayers(List<FileMetadata> files)
    {
        var layers = new Dictionary<string, LayerInfo>();

        foreach (var file in files)
        {
            var normalizedPath = NormalizePath(file.RelativePath).ToUpperInvariant();
            string layerName = GetDotNetLayerName(normalizedPath);
            AddToLayer(layers, layerName, file);
        }

        return layers.Where(l => l.Value.Files.Any()).ToDictionary(l => l.Key, l => l.Value);
    }

    private static string GetDotNetLayerName(string normalizedPath)
    {
        if (HasPathSegment(normalizedPath, "DOMAIN") || HasPathSegment(normalizedPath, "MODELS") ||
            HasPathSegment(normalizedPath, "ENTITIES"))
            return "Domain";
        if (HasPathSegment(normalizedPath, "APPLICATION") || HasPathSegment(normalizedPath, "SERVICES") ||
            HasPathSegment(normalizedPath, "USECASES") || HasPathSegment(normalizedPath, "USE_CASES"))
            return "Application";
        if (HasPathSegment(normalizedPath, "INFRASTRUCTURE") || HasPathSegment(normalizedPath, "REPOSITORIES") ||
            HasPathSegment(normalizedPath, "DATA") || HasPathSegment(normalizedPath, "PERSISTENCE"))
            return "Infrastructure";
        if (HasPathSegment(normalizedPath, "FORMS") || HasPathSegment(normalizedPath, "VIEWS") ||
            HasPathSegment(normalizedPath, "UI") || HasPathSegment(normalizedPath, "PAGES") ||
            HasPathSegment(normalizedPath, "COMPONENTS"))
            return "UI";
        if (HasPathSegment(normalizedPath, "CONTROLLERS") || HasPathSegment(normalizedPath, "API") ||
            HasPathSegment(normalizedPath, "ENDPOINTS"))
            return "Controllers";
        if (HasPathSegment(normalizedPath, "TEST") || HasPathSegment(normalizedPath, "TESTS"))
            return "Tests";
        if (HasPathSegment(normalizedPath, "CONFIG") || HasPathSegment(normalizedPath, "SETTINGS"))
            return "Configuration";
        return "Other";
    }

    // ========== WebApp ==========
    public static Dictionary<string, LayerInfo> DetectWebAppLayers(List<FileMetadata> files)
    {
        var layers = new Dictionary<string, LayerInfo>();

        foreach (var file in files)
        {
            var normalizedPath = NormalizePath(file.RelativePath).ToUpperInvariant();
            var extension = GetNormalizedExtension(file.RelativePath);
            string layerName = GetWebAppLayerName(normalizedPath, extension);
            AddToLayer(layers, layerName, file);
        }

        return layers;
    }

    private static string GetWebAppLayerName(string normalizedPath, string extension)
    {
        if (extension == ".HTML" || extension == ".HTM" ||
            normalizedPath.Contains(".VUE") || normalizedPath.Contains(".JSX") ||
            normalizedPath.Contains(".TSX"))
            return "HTML";
        if (extension == ".JS" || extension == ".JSX" || extension == ".TS" || extension == ".TSX")
            return "JavaScript";
        if (extension == ".CSS" || extension == ".SCSS" || extension == ".SASS" || extension == ".LESS")
            return "CSS";
        if (extension == ".SVG" || extension == ".PNG" || extension == ".JPG" ||
            extension == ".JPEG" || extension == ".GIF" || extension == ".ICO" || extension == ".WEBP")
            return "Assets";
        if (normalizedPath.Contains("PACKAGE.JSON") || normalizedPath.Contains("CONFIG.") ||
            normalizedPath.Contains("VITE.CONFIG") || normalizedPath.Contains("WEBPACK.CONFIG"))
            return "Configuration";
        if (extension == ".JSON")
            return "Data";
        return "Other";
    }

    // ========== Python ==========
    public static Dictionary<string, LayerInfo> DetectPythonLayers(List<FileMetadata> files)
    {
        var layers = new Dictionary<string, LayerInfo>();

        foreach (var file in files)
        {
            var normalizedPath = NormalizePath(file.RelativePath).ToLowerInvariant();
            var fileName = Path.GetFileName(normalizedPath);
            string layerName = GetPythonLayerName(normalizedPath, fileName);
            AddToLayer(layers, layerName, file);
        }

        return layers;
    }

    private static string GetPythonLayerName(string normalizedPath, string fileName)
    {
        if (normalizedPath.Contains("/tests/") || fileName.StartsWith("test_") || fileName.EndsWith("_test.py"))
            return "Tests";
        if (normalizedPath.Contains("/models/") || normalizedPath.Contains("/entities/") ||
            normalizedPath.Contains("/domain/") || normalizedPath.Contains("/schemas/"))
            return "Domain";
        if (normalizedPath.Contains("/services/") || normalizedPath.Contains("/use_cases/") ||
            normalizedPath.Contains("/usecases/") || normalizedPath.Contains("/application/") ||
            normalizedPath.Contains("/logic/") || normalizedPath.Contains("/handlers/"))
            return "Application";
        if (normalizedPath.Contains("/repositories/") || normalizedPath.Contains("/repository/") ||
            normalizedPath.Contains("/db/") || normalizedPath.Contains("/database/") ||
            normalizedPath.Contains("/infra/") || normalizedPath.Contains("/infrastructure/") ||
            normalizedPath.Contains("/adapters/") || normalizedPath.Contains("/gateways/"))
            return "Infrastructure";
        if (normalizedPath.Contains("/api/") || normalizedPath.Contains("/views/") ||
            normalizedPath.Contains("/handlers/") || normalizedPath.Contains("/routers/") ||
            normalizedPath.Contains("/endpoints/") || normalizedPath.Contains("/cli/") ||
            normalizedPath.Contains("/ui/") || normalizedPath.Contains("/templates/") ||
            normalizedPath.Contains("/static/"))
            return "UI";
        if (normalizedPath.Contains("/config/") || normalizedPath.Contains("/settings/") ||
            normalizedPath.Contains(".env") || normalizedPath.Contains("settings.py"))
            return "Configuration";
        return "Other";
    }

    #region Helper Methods

    private static void AddToLayer(Dictionary<string, LayerInfo> layers, string name, FileMetadata file)
    {
        if (!layers.ContainsKey(name))
            layers[name] = new LayerInfo { Name = name, Files = new List<FileMetadata>() };
        layers[name].Files.Add(file);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        return path.Replace('\\', '/');
    }

    private static string GetNormalizedExtension(string path)
    {
        var extension = Path.GetExtension(path);
        if (string.IsNullOrEmpty(extension))
            return string.Empty;
        return extension.ToUpperInvariant();
    }

    private static bool HasPathSegment(string normalizedPath, string segment)
    {
        if (string.IsNullOrEmpty(normalizedPath) || string.IsNullOrEmpty(segment))
            return false;
        return normalizedPath.Contains($"/{segment}/", StringComparison.Ordinal) ||
               normalizedPath.Contains($"{segment}/", StringComparison.Ordinal) ||
               normalizedPath.Contains($"/{segment}", StringComparison.Ordinal) ||
               normalizedPath == segment ||
               normalizedPath == $"/{segment}" ||
               normalizedPath == $"\\{segment}";
    }

    #endregion
}