/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using LLMCodeExporter.Core.Interfaces;
using LLMCodeExporter.Core.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LLMCodeExporter.Infrastructure.Utils;
using LLMCodeExporter.Infrastructure.Utils.Architecture; 
namespace LLMCodeExporter.Infrastructure.Services;
/// <summary>
/// Анализатор гибридных проектов, поддерживающий динамический выбор языка бекенда и фронтенда.
/// </summary>
public class HybridProjectAnalyzer : UniversalProjectAnalyzer
{
    private readonly ExportSettings _settings;
    public HybridProjectAnalyzer(IExportSettings settings) : base(settings)
    {
        if (settings is ExportSettings exportSettings)
            _settings = exportSettings;
        else
            throw new ArgumentException("Ожидается ExportSettings", nameof(settings));
    }

    protected override List<FileMetadata> ScanProjectFiles(string projectPath, LanguageSettings baseSettings)
    {
        var backendSettings = LanguageSettings.ForLanguage(_settings.BackendLanguage);
        var frontendSettings = LanguageSettings.ForLanguage(_settings.FrontendLanguage);
        var combined = LanguageSettings.Combine(backendSettings, frontendSettings);
        return base.ScanProjectFiles(projectPath, combined);
    }

    /// <summary>
    /// Переопределяем анализ проекта, чтобы обогатить метаданные информацией о языках и подсчёте файлов по слоям.
    /// </summary>
    public override ProjectInfo AnalyzeProject(string projectPath)
    {
        var projectInfo = base.AnalyzeProject(projectPath);
        if (projectInfo.Metadata is ExportMetadata metadata)
        {
            metadata.BackendLanguage = _settings.BackendLanguage;
            metadata.FrontendLanguage = _settings.FrontendLanguage;
        }
        EnrichWithLayerCounts(projectInfo);
        // Добавляем архитектуру и интеграцию (база уже сделала, но переопределим для гарантии)
        projectInfo.Metadata.Architecture = ArchitectureInfoBuilder.Build(projectInfo, _settings.ProjectType);
        projectInfo.Metadata.IntegrationPoints = IntegrationAnalyzer.Analyze(projectInfo.Files, _settings);
        return projectInfo;
    }

    public Dictionary<string, object> AnalyzeArchitectureHybrid(List<FileMetadata> files)
    {
        var backendExtensions = LanguageSettings.ForLanguage(_settings.BackendLanguage).FileExtensions;
        var frontendExtensions = LanguageSettings.ForLanguage(_settings.FrontendLanguage).FileExtensions;
        var backend = new List<FileMetadata>();
        var frontend = new List<FileMetadata>();
        var config = new List<FileMetadata>();
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file.RelativePath).ToLowerInvariant();
            if (backendExtensions.Contains(ext))
                backend.Add(file);
            else if (frontendExtensions.Contains(ext))
                frontend.Add(file);
            else
                config.Add(file);
        }

        return new Dictionary<string, object>
        {
            ["Backend"] = backend,
            ["Frontend"] = frontend,
            ["Configuration"] = config
        };
    }

    /// <summary>
    /// Расширенный анализ зависимостей для обоих языков с поддержкой разных менеджеров пакетов.
    /// </summary>
    public List<string> AnalyzeDependenciesHybrid(List<FileMetadata> files)
    {
        var deps = new HashSet<string>();
        var backendSettings = LanguageSettings.ForLanguage(_settings.BackendLanguage);
        var frontendSettings = LanguageSettings.ForLanguage(_settings.FrontendLanguage);
        var allPackageFiles = backendSettings.PackageFiles.Union(frontendSettings.PackageFiles).Distinct().ToList();
        foreach (var packageFileName in allPackageFiles)
        {
            foreach (var file in files.Where(f => f.RelativePath.EndsWith(packageFileName, StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var content = File.ReadAllText(file.FullPath);
                    var ext = Path.GetExtension(packageFileName).ToLowerInvariant();
                    switch (ext)
                    {
                        case ".csproj":
                            ParseNuGetDependencies(content, deps);
                            break;
                        case ".json":
                            if (packageFileName.Equals("package.json", StringComparison.OrdinalIgnoreCase))
                                ParseNpmDependencies(content, deps);
                            else if (packageFileName.Equals("composer.json", StringComparison.OrdinalIgnoreCase))
                                ParseComposerDependencies(content, deps);
                            break;
                        case ".txt":
                            if (packageFileName.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase))
                                ParsePythonRequirements(content, deps);
                            break;
                        case ".xml":
                            if (packageFileName.Equals("pom.xml", StringComparison.OrdinalIgnoreCase))
                                ParseMavenDependencies(content, deps);
                            break;
                        case ".mod":
                            if (packageFileName.Equals("go.mod", StringComparison.OrdinalIgnoreCase))
                                ParseGoModDependencies(content, deps);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Ошибка парсинга зависимостей в файле (метод {nameof(ParseNuGetDependencies)})", ex);
                }
            }
        }

        return deps.OrderBy(d => d).ToList();
    }

    #region Парсеры зависимостей для разных менеджеров
    private void ParseNuGetDependencies(string content, HashSet<string> deps)
    {
        var matches = Regex.Matches(content, @"<PackageReference\s+Include=""([^""]+)""");
        foreach (Match m in matches)
            deps.Add($"NuGet: {m.Groups[1].Value}");
    }

    private void ParseNpmDependencies(string json, HashSet<string> deps)
    {
        var depBlock = Regex.Match(json, @"""dependencies""\s*:\s*\{([^}]+)\}", RegexOptions.Singleline);
        if (depBlock.Success)
        {
            var pkgMatches = Regex.Matches(depBlock.Groups[1].Value, @"""([^""]+)""");
            foreach (Match pm in pkgMatches)
                deps.Add($"npm: {pm.Groups[1].Value}");
        }
    }

    private void ParseComposerDependencies(string json, HashSet<string> deps)
    {
        var depBlock = Regex.Match(json, @"""require""\s*:\s*\{([^}]+)\}", RegexOptions.Singleline);
        if (depBlock.Success)
        {
            var pkgMatches = Regex.Matches(depBlock.Groups[1].Value, @"""([^""]+)""");
            foreach (Match pm in pkgMatches)
                deps.Add($"Composer: {pm.Groups[1].Value}");
        }
    }

    private void ParsePythonRequirements(string content, HashSet<string> deps)
    {
        var lines = content.Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("#"))
            .Select(line => line.Trim().Split('=')[0].Trim());
        foreach (var pkg in lines)
            if (!string.IsNullOrEmpty(pkg))
                deps.Add($"pip: {pkg}");
    }

    private void ParseMavenDependencies(string xml, HashSet<string> deps)
    {
        var matches = Regex.Matches(xml, @"<dependency>\s*<groupId>([^<]+)</groupId>\s*<artifactId>([^<]+)</artifactId>", RegexOptions.Singleline);
        foreach (Match m in matches)
            deps.Add($"Maven: {m.Groups[1].Value}:{m.Groups[2].Value}");
    }

    private void ParseGoModDependencies(string content, HashSet<string> deps)
    {
        var lines = content.Split('\n')
            .Where(line => line.Trim().StartsWith("require") && !line.Contains("//"))
            .Select(line => line.Replace("require", "").Trim());
        foreach (var dep in lines)
            if (!string.IsNullOrEmpty(dep))
                deps.Add($"Go: {dep}");
    }
    #endregion
    private void EnrichWithLayerCounts(ProjectInfo projectInfo)
    {
        if (projectInfo.Metadata is not ExportMetadata metadata) return;
        var backendExtensions = LanguageSettings.ForLanguage(_settings.BackendLanguage).FileExtensions;
        var frontendExtensions = LanguageSettings.ForLanguage(_settings.FrontendLanguage).FileExtensions;
        metadata.BackendFilesCount = projectInfo.Files.Count(f =>
            backendExtensions.Contains(Path.GetExtension(f.RelativePath).ToLowerInvariant()));
        metadata.FrontendFilesCount = projectInfo.Files.Count(f =>
            frontendExtensions.Contains(Path.GetExtension(f.RelativePath).ToLowerInvariant()));
    }
}