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
using LLMCodeExporter.Infrastructure.Utils;
using LLMCodeExporter.Infrastructure.Utils.Architecture; 
namespace LLMCodeExporter.Infrastructure.Services;
/// <summary>
/// Универсальный анализатор проектов
/// </summary>
public class UniversalProjectAnalyzer : IProjectAnalyzer
{
    private readonly IExportSettings _settings;
    private readonly Dictionary<ProjectType, LanguageSettings> _languageSettings;
    public UniversalProjectAnalyzer(IExportSettings settings)
    {
        _settings = settings;
        _languageSettings = new Dictionary<ProjectType, LanguageSettings>
        {
            { ProjectType.CSharp, LanguageSettings.ForCSharp },
            { ProjectType.Python, LanguageSettings.ForPython },
            { ProjectType.JavaScript, LanguageSettings.ForJavaScript },
            { ProjectType.TypeScript, LanguageSettings.ForTypeScript },
            { ProjectType.WebApp, LanguageSettings.ForWebApp },
            { ProjectType.Generic, new LanguageSettings
                {
                    FileExtensions = new[] { ".*" },
                    ExcludeFolders = new[] { ".git", ".vs", "node_modules", "bin", "obj" }
                }
            }
        };
    }

  /// <summary>
/// Анализирует проект и возвращает информацию о нём.
/// Виртуальный метод — может быть переопределён в наследниках.
/// </summary>
public virtual ProjectInfo AnalyzeProject(string projectPath)
{
    // 1. Базовый анализ
    var projectInfo = new ProjectInfo
    {
        ProjectPath = projectPath,
        ProjectName = Path.GetFileName(projectPath)
    };
    var detectedType = DetectProjectType(projectPath);
    var languageSettings = _languageSettings[detectedType];
    var files = ScanProjectFiles(projectPath, languageSettings);
    var dependencies = AnalyzeDependencies(files, detectedType);
    var architecture = AnalyzeArchitecture(files, detectedType);
    // 2. Заполнение метаданных
    projectInfo.Files = files;
    projectInfo.Metadata = new ExportMetadata
    {
        ProjectType = detectedType,
        TotalFiles = files.Count,
        EstimatedTokens = EstimateTokens(files),
        ArchitectureLayers = architecture,
        Dependencies = dependencies
    };
    // 3. Архитектурная информация и интеграционные точки
    projectInfo.Metadata.Architecture = ArchitectureInfoBuilder.Build(projectInfo, detectedType);
    if (detectedType == ProjectType.Hybrid && _settings is ExportSettings exportSettings)
    {
        projectInfo.Metadata.IntegrationPoints = IntegrationAnalyzer.Analyze(projectInfo.Files, exportSettings);
    }

    // 4. Проверка архитектурных правил (для C# и гибридных проектов)
    if (detectedType == ProjectType.CSharp || detectedType == ProjectType.Hybrid)
    {
        var rules = DefaultRules.GetCleanArchitectureRules();
        var violations = DependencyRuleChecker.CheckRules(
            projectInfo.Files,
            projectInfo.Metadata.Architecture,
            rules
        );
        projectInfo.Metadata.DependencyViolations = violations;
        projectInfo.Metadata.AppliedRules = rules;
    }

    // ========== НОВЫЕ ФИЧИ ==========
    // 5. Семантическое описание архитектуры
    projectInfo.Metadata.SemanticDescription = SemanticDescriptionGenerator.Generate(
        projectInfo.Metadata.Architecture,
        projectInfo.Metadata.IntegrationPoints,
        detectedType
    );
    // 6. Метрики качества кода
    projectInfo.Metadata.Metrics = MetricsCollector.Collect(projectInfo, projectInfo.Metadata.Architecture);
    // 7. Ключевые компоненты с аннотациями
    var entryPoints = new List<string> { "Program.cs", "Startup.cs", "ApplicationRunner.cs", "Main.cs" };
    if (_settings is ExportSettings settings && settings.EntryPoints.Any())
        entryPoints = settings.EntryPoints;
    projectInfo.Metadata.KeyComponents = AnnotationGenerator.GenerateAnnotations(
        projectInfo,
        projectInfo.Metadata.Architecture,
        entryPoints
    );
    return projectInfo;
}

    /// <summary>
    /// Определяет тип проекта на основе файловой структуры.
    /// </summary>
    public virtual ProjectType DetectProjectType(string projectPath)
    {
        if (_settings.ProjectType != ProjectType.AutoDetect)
            return _settings.ProjectType;
        var files = Directory.GetFiles(projectPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            var normalizedFile = NormalizePath(file).ToLowerInvariant();
            if (normalizedFile.EndsWith(".csproj") || normalizedFile.EndsWith(".sln"))
                return ProjectType.CSharp;
            if (normalizedFile.EndsWith(".py") || normalizedFile.Contains("requirements.txt"))
                return ProjectType.Python;
            if (normalizedFile.EndsWith("package.json"))
                return files.Any(f => NormalizePath(f).ToLowerInvariant().EndsWith(".ts"))
                    ? ProjectType.TypeScript
                    : ProjectType.JavaScript;
        }
        return ProjectType.Generic;
    }

    /// <summary>
    /// Оценивает количество токенов в файле.
    /// </summary>
    public virtual int EstimateFileTokens(string filePath)
    {
        if (!File.Exists(filePath))
            return 0;
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var contentLength = stream.Length;
        return (int)Math.Ceiling(contentLength / 4.0);
    }

    /// <summary>
    /// Оценивает общее количество токенов в списке файлов.
    /// </summary>
    public virtual int EstimateTokens(List<FileMetadata> files)
    {
        return files.Sum(f => f.EstimatedTokens);
    }

    /// <summary>
    /// Анализирует архитектуру проекта.
    /// </summary>
    public virtual Dictionary<string, object> AnalyzeArchitecture(List<FileMetadata> files, ProjectType projectType)
    {
        var architecture = new Dictionary<string, object>();
        var folders = files
            .Select(f => GetNormalizedDirectoryName(f.RelativePath))
            .Where(dir => !string.IsNullOrEmpty(dir))
            .Distinct()
            .ToList();
        architecture["Folders"] = folders;
        architecture["FileCount"] = files.Count;
        var extensions = files
            .Select(f => Path.GetExtension(f.FullPath).ToLower())
            .GroupBy(ext => ext)
            .Select(g => new { Extension = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Extension, x => x.Count);
        architecture["FileExtensions"] = extensions;
        return architecture;
    }

    /// <summary>
    /// Анализирует зависимости проекта.
    /// </summary>
    public virtual List<string> AnalyzeDependencies(List<FileMetadata> files, ProjectType projectType)
    {
        if (projectType == ProjectType.Python)
            return AnalyzePythonDependencies(files);
        var dependencies = new List<string>();
        switch (projectType)
        {
            case ProjectType.CSharp:
                var csprojFiles = files.Where(f => 
                    NormalizePath(f.FullPath).ToLowerInvariant().EndsWith(".csproj")).ToList();
                foreach (var file in csprojFiles)
                {
                    try
                    {
                        var content = File.ReadAllText(file.FullPath);
                        if (content.Contains("PackageReference"))
                            dependencies.Add("NuGet packages detected");
                    }
                    catch (Exception ex)
                    {
                       Logger.LogError($"Ошибка чтения файла {file.RelativePath} при анализе интеграций", ex);
                    }
                }
                break;
            case ProjectType.JavaScript:
            case ProjectType.TypeScript:
                var packageJsonFiles = files.Where(f => 
                    NormalizePath(f.FullPath).ToLowerInvariant().EndsWith("package.json")).ToList();
                if (packageJsonFiles.Any())
                    dependencies.Add("Node.js package.json detected");
                break;
        }
        return dependencies;
    }

    /// <summary>
    /// Анализ зависимостей для Python проектов.
    /// </summary>
    public virtual List<string> AnalyzePythonDependencies(List<FileMetadata> files)
    {
        var dependencies = new List<string>();
        var requirementsFile = files.FirstOrDefault(f =>
            f.RelativePath.EndsWith("requirements.txt") ||
            f.FullPath.EndsWith("requirements.txt"));
        if (requirementsFile != null)
        {
            try
            {
                var content = File.ReadAllText(requirementsFile.FullPath);
                var lines = content.Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("#"))
                    .Select(line => line.Trim())
                    .ToList();
                dependencies.AddRange(lines);
            }

            catch (Exception ex)
                {
                    Logger.LogError($"Ошибка чтения requirements.txt: {requirementsFile.FullPath}", ex);
                    dependencies.Add("requirements.txt (ошибка чтения)");
                }
        }
        var setupFile = files.FirstOrDefault(f =>
            f.RelativePath.EndsWith("setup.py") ||
            f.FullPath.EndsWith("setup.py"));
        if (setupFile != null)
            dependencies.Add("setup.py обнаружен");
        var pyprojectFile = files.FirstOrDefault(f =>
            f.RelativePath.EndsWith("pyproject.toml") ||
            f.FullPath.EndsWith("pyproject.toml"));
        if (pyprojectFile != null)
            dependencies.Add("pyproject.toml обнаружен");
        if (!dependencies.Any())
            dependencies.Add("Python dependencies not explicitly defined");
        return dependencies;
    }

    /// <summary>
    /// Сканирует файлы проекта с учётом исключений.
    /// </summary>
    protected virtual List<FileMetadata> ScanProjectFiles(string projectPath, LanguageSettings settings)
    {
        var files = new List<FileMetadata>();
        foreach (var extension in settings.FileExtensions)
        {
            foreach (var file in Directory.GetFiles(projectPath, $"*{extension}", SearchOption.AllDirectories))
            {
                if (ShouldExclude(file, settings.ExcludeFolders))
                    continue;
                var fileInfo = new FileInfo(file);
                var estimatedTokens = EstimateFileTokens(file);
                files.Add(new FileMetadata
                {
                    FullPath = file,
                    RelativePath = NormalizePath(Path.GetRelativePath(projectPath, file)),
                    SizeInBytes = fileInfo.Length,
                    EstimatedTokens = estimatedTokens
                });
            }
        }
        return files;
    }

    /// <summary>
    /// Проверяет, следует ли исключить файл из анализа.
    /// </summary>
    protected virtual bool ShouldExclude(string filePath, string[] excludePatterns)
    {
        var fileName = Path.GetFileName(filePath);
        var directoryName = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(directoryName))
            return false;
        string normalizedDir = NormalizePath(directoryName).ToLowerInvariant();
        foreach (var pattern in excludePatterns)
        {
            string normalizedPattern = NormalizePath(pattern).ToLowerInvariant();
            if (HasPathSegment(normalizedDir, normalizedPattern))
                return true;
            if (pattern.Contains("*") || pattern.Contains("?"))
            {
                var regexPattern = WildcardToRegex(pattern);
                if (Regex.IsMatch(fileName, regexPattern, RegexOptions.IgnoreCase))
                    return true;
            }
            else if (fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Конвертирует wildcard-паттерн в регулярное выражение.
    /// </summary>
    protected virtual string WildcardToRegex(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentException("Паттерн не может быть пустым.", nameof(pattern));
        var escaped = Regex.Escape(pattern);
        return "^" + escaped
            .Replace("\\*", ".*")
            .Replace("\\?", ".")
            + "$";
    }

    #region Helper Methods
    /// <summary>
    /// Нормализует путь: заменяет все обратные слэши на косые.
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        var normalized = path.Replace('\\', '/');
        while (normalized.Contains("//"))
            normalized = normalized.Replace("//", "/");
        if (normalized.StartsWith("/") && normalized.Length > 1)
            normalized = normalized.Substring(1);
        return normalized;
    }

    /// <summary>
    /// Получает имя директории с нормализованными путями.
    /// </summary>
    private static string? GetNormalizedDirectoryName(string relativePath)
    {
        var dirName = Path.GetDirectoryName(relativePath);
        return dirName != null ? NormalizePath(dirName) : null;
    }

    /// <summary>
    /// Проверяет наличие сегмента пути с границами.
    /// </summary>
    private static bool HasPathSegment(string normalizedPath, string segment)
    {
        if (string.IsNullOrEmpty(normalizedPath) || string.IsNullOrEmpty(segment))
            return false;
        return normalizedPath.Contains($"/{segment}/", StringComparison.Ordinal) ||
               normalizedPath.Contains($"{segment}/", StringComparison.Ordinal) ||
               normalizedPath.Contains($"/{segment}", StringComparison.Ordinal) ||
               normalizedPath.Equals(segment, StringComparison.Ordinal);
    }
    #endregion
}