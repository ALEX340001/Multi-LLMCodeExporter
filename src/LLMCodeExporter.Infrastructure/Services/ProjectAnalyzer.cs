using LLMCodeExporter.Core.Interfaces;
using LLMCodeExporter.Core.Models;
using System.Text.RegularExpressions;

namespace LLMCodeExporter.Infrastructure.Services;

/// <summary>
/// Базовый анализатор проектов с поддержкой специализации для разных языков
/// </summary>
public class ProjectAnalyzer : IProjectAnalyzer
{
    protected readonly IExportSettings _settings;
    protected readonly Dictionary<ProjectType, LanguageSettings> _languageSettings;

    /// <summary>
    /// Конструктор анализатора проектов
    /// </summary>
    /// <param name="settings">Настройки экспорта</param>
    /// <param name="languageSettings">Настройки языков программирования</param>
    public ProjectAnalyzer(IExportSettings settings, Dictionary<ProjectType, LanguageSettings> languageSettings)
    {
        _settings = settings;
        _languageSettings = languageSettings;
    }

    /// <summary>
    /// Конструктор для Python-анализатора (совместимость с PythonProjectAnalyzer)
    /// </summary>
    /// <param name="settings">Настройки экспорта</param>
    public ProjectAnalyzer(IExportSettings settings) : this(settings, GetDefaultLanguageSettings())
    {
    }

    /// <inheritdoc/>
    public virtual ProjectInfo AnalyzeProject(string projectPath)
    {
        var projectInfo = new ProjectInfo
        {
            ProjectPath = projectPath,
            ProjectName = Path.GetFileName(projectPath)
        };

        // Автоопределение типа проекта
        var detectedType = DetectProjectType(projectPath);
        var languageSettings = _languageSettings[detectedType];

        // Специализированное сканирование файлов в зависимости от типа проекта
        var files = ScanProjectFiles(projectPath, languageSettings, detectedType);

        // Анализ зависимостей с учетом типа проекта
        var dependencies = AnalyzeDependencies(files, detectedType);

        // Анализ архитектуры
        var architecture = AnalyzeArchitecture(files, detectedType);

        // Сбор метаданных
        projectInfo.Files = files;
        projectInfo.Metadata = new ExportMetadata
        {
            ProjectType = detectedType,
            TotalFiles = files.Count,
            EstimatedTokens = EstimateTokens(files),
            ArchitectureLayers = architecture,
            Dependencies = dependencies
        };

        return projectInfo;
    }

    /// <inheritdoc/>
    public virtual ProjectType DetectProjectType(string projectPath)
    {
        if (_settings.ProjectType != ProjectType.AutoDetect)
            return _settings.ProjectType;

        var files = Directory.GetFiles(projectPath, "*", SearchOption.TopDirectoryOnly);

        // Определение по специфичным файлам
        if (files.Any(f => f.EndsWith(".csproj") || f.EndsWith(".sln")))
            return ProjectType.CSharp;

        if (files.Any(f => f.EndsWith(".py") || f.EndsWith("requirements.txt")))
            return ProjectType.Python;

        if (files.Any(f => f.EndsWith("package.json")))
            return files.Any(f => f.EndsWith(".ts"))
                ? ProjectType.TypeScript
                : ProjectType.JavaScript;

        return ProjectType.Generic;
    }

    /// <inheritdoc/>
    public virtual int EstimateFileTokens(string filePath)
    {
        // Базовая реализация: приблизительно 1 токен = 4 символа
        var content = File.ReadAllText(filePath);
        return (int)Math.Ceiling(content.Length / 4.0);
    }

    /// <inheritdoc/>
    public virtual int EstimateTokens(List<FileMetadata> files)
    {
        return files.Sum(f => f.EstimatedTokens);
    }

    /// <inheritdoc/>
    public virtual Dictionary<string, object> AnalyzeArchitecture(List<FileMetadata> files, ProjectType projectType)
    {
        var architecture = new Dictionary<string, object>();

        // Простая группировка по папкам для демонстрации
        var folders = files
            .Select(f => Path.GetDirectoryName(f.RelativePath))
            .Where(dir => !string.IsNullOrEmpty(dir))
            .Distinct()
            .ToList();

        architecture["Folders"] = folders;
        architecture["FileCount"] = files.Count;

        // Специализированная архитектурная информация для разных типов проектов
        switch (projectType)
        {
            case ProjectType.Python:
                var pythonModules = files
                    .Where(f => f.RelativePath.EndsWith(".py"))
                    .Select(f => Path.GetFileNameWithoutExtension(f.RelativePath))
                    .Distinct()
                    .ToList();
                architecture["PythonModules"] = pythonModules;
                break;
        }

        return architecture;
    }

    /// <inheritdoc/>
    public virtual List<string> AnalyzeDependencies(List<FileMetadata> files, ProjectType projectType)
    {
        // Используем специализированный анализ для Python
        if (projectType == ProjectType.Python)
        {
            return AnalyzePythonDependencies(files);
        }

        // Общий анализ для других типов проектов
        var dependencies = new List<string>();

        switch (projectType)
        {
            case ProjectType.CSharp:
                var csprojFiles = files.Where(f => f.FullPath.EndsWith(".csproj")).ToList();
                foreach (var file in csprojFiles)
                {
                    try
                    {
                        var content = File.ReadAllText(file.FullPath);
                        // Простой парсинг для демонстрации
                        if (content.Contains("PackageReference"))
                        {
                            dependencies.Add("NuGet packages detected");
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки чтения
                    }
                }
                break;

            case ProjectType.JavaScript:
            case ProjectType.TypeScript:
                var packageJsonFiles = files.Where(f => f.FullPath.EndsWith("package.json")).ToList();
                if (packageJsonFiles.Any())
                {
                    dependencies.Add("Node.js package.json detected");
                }
                break;
        }

        return dependencies;
    }

    /// <summary>
    /// Специализированный анализ зависимостей Python
    /// </summary>
    /// <param name="files">Список файлов проекта</param>
    /// <returns>Список зависимостей Python</returns>
    public virtual List<string> AnalyzePythonDependencies(List<FileMetadata> files)
    {
        var dependencies = new List<string>();

        // Ищем requirements.txt
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
            catch
            {
                dependencies.Add("requirements.txt (ошибка чтения)");
            }
        }

        // Ищем setup.py или pyproject.toml
        var setupFile = files.FirstOrDefault(f =>
            f.RelativePath.EndsWith("setup.py") ||
            f.FullPath.EndsWith("setup.py"));

        if (setupFile != null)
        {
            dependencies.Add("setup.py обнаружен");
        }

        var pyprojectFile = files.FirstOrDefault(f =>
            f.RelativePath.EndsWith("pyproject.toml") ||
            f.FullPath.EndsWith("pyproject.toml"));

        if (pyprojectFile != null)
        {
            dependencies.Add("pyproject.toml обнаружен");
        }

        // Если ничего не найдено, добавляем базовую информацию
        if (!dependencies.Any())
        {
            dependencies.Add("Python dependencies not explicitly defined");
        }

        return dependencies;
    }

    /// <summary>
    /// Сканирует файлы проекта с поддержкой специализации по типам проектов
    /// </summary>
    /// <param name="projectPath">Путь к проекту</param>
    /// <param name="settings">Настройки языка</param>
    /// <param name="projectType">Тип проекта</param>
    /// <returns>Список метаданных файлов</returns>
    protected virtual List<FileMetadata> ScanProjectFiles(string projectPath, LanguageSettings settings, ProjectType projectType)
    {
        var files = new List<FileMetadata>();

        foreach (var extension in settings.FileExtensions)
        {
            foreach (var file in Directory.GetFiles(projectPath, $"*{extension}", SearchOption.AllDirectories))
            {
                // Проверка исключений
                if (ShouldExclude(file, settings.ExcludeFolders))
                    continue;

                var fileInfo = new FileInfo(file);
                var estimatedTokens = EstimateFileTokens(file);

                // Специализированная обработка для разных типов файлов
                ProcessFileByType(file, files, projectPath, fileInfo, estimatedTokens, projectType);
            }
        }

        return files;
    }

    /// <summary>
    /// Обрабатывает файл с учетом его типа и типа проекта
    /// </summary>
    protected virtual void ProcessFileByType(string file, List<FileMetadata> files, string projectPath,
        FileInfo fileInfo, int estimatedTokens, ProjectType projectType)
    {
        // Специализированная логика для Python файлов
        if (projectType == ProjectType.Python && file.EndsWith(".py"))
        {
            // Можно добавить специальную обработку для .py файлов
            // Например, анализ импортов или docstring
        }

        files.Add(new FileMetadata
        {
            FullPath = file,
            RelativePath = Path.GetRelativePath(projectPath, file),
            SizeInBytes = fileInfo.Length,
            EstimatedTokens = estimatedTokens
        });
    }

    /// <summary>
    /// Проверяет, нужно ли исключить файл
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="excludePatterns">Паттерны исключения</param>
    /// <returns>True если файл нужно исключить</returns>
    protected virtual bool ShouldExclude(string filePath, string[] excludePatterns)
    {
        var relativePath = Path.GetFileName(filePath);
        return excludePatterns.Any(pattern =>
            pattern.Contains("*")
                ? Regex.IsMatch(relativePath, WildcardToRegex(pattern))
                : relativePath.Contains(pattern));
    }

    /// <summary>
    /// Преобразует wildcard паттерн в regex
    /// </summary>
    /// <param name="pattern">Wildcard паттерн</param>
    /// <returns>Regex строка</returns>
    protected virtual string WildcardToRegex(string pattern) =>
        "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";

    /// <summary>
    /// Получает настройки языков по умолчанию
    /// </summary>
    /// <returns>Словарь настроек языков</returns>
    private static Dictionary<ProjectType, LanguageSettings> GetDefaultLanguageSettings()
    {
        // Возвращаем базовые настройки языков
        return new Dictionary<ProjectType, LanguageSettings>
        {
            { ProjectType.CSharp, new LanguageSettings { FileExtensions = new[] { ".cs", ".csproj", ".sln" } } },
            { ProjectType.Python, new LanguageSettings { FileExtensions = new[] { ".py", ".txt", ".toml", ".cfg", ".ini" } } },
            { ProjectType.JavaScript, new LanguageSettings { FileExtensions = new[] { ".js", ".json", ".html", ".css" } } },
            { ProjectType.TypeScript, new LanguageSettings { FileExtensions = new[] { ".ts", ".tsx", ".js", ".json" } } },
            { ProjectType.Generic, new LanguageSettings { FileExtensions = new[] { ".*" } } }
        };
    }
}