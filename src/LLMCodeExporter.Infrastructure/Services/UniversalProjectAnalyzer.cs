using LLMCodeExporter.Core.Interfaces;
using LLMCodeExporter.Core.Models;
using System.Text.RegularExpressions;

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

        // Инициализация настроек для разных языков
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

    public ProjectInfo AnalyzeProject(string projectPath)
    {
        var projectInfo = new ProjectInfo
        {
            ProjectPath = projectPath,
            ProjectName = Path.GetFileName(projectPath)
        };

        // Автоопределение типа проекта
        var detectedType = DetectProjectType(projectPath);
        var languageSettings = _languageSettings[detectedType];

        // Сканирование файлов с учетом языка
        var files = ScanProjectFiles(projectPath, languageSettings);

        // Анализ зависимостей
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

    public ProjectType DetectProjectType(string projectPath)
    {
        if (_settings.ProjectType != ProjectType.AutoDetect)
            return _settings.ProjectType;

        var files = Directory.GetFiles(projectPath, "*", SearchOption.TopDirectoryOnly);

        // Определение по специфичным файлам
        foreach (var file in files)
        {
            var normalizedFile = NormalizePath(file).ToLowerInvariant();
            
            if (normalizedFile.EndsWith(".csproj") || normalizedFile.EndsWith(".sln"))
                return ProjectType.CSharp;

            if (normalizedFile.EndsWith(".py") || normalizedFile.Contains("requirements.txt".ToLower()))
                return ProjectType.Python;

            if (normalizedFile.EndsWith("package.json"))
                return files.Any(f => NormalizePath(f).ToLowerInvariant().EndsWith(".ts"))
                    ? ProjectType.TypeScript
                    : ProjectType.JavaScript;
        }

        return ProjectType.Generic;
    }

    public int EstimateFileTokens(string filePath)
    {
        if (!File.Exists(filePath))
            return 0;

        // Базовая реализация: приблизительно 1 токен = 4 символа
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var contentLength = stream.Length;
        return (int)Math.Ceiling(contentLength / 4.0);
    }

    public int EstimateTokens(List<FileMetadata> files)
    {
        return files.Sum(f => f.EstimatedTokens);
    }

    public Dictionary<string, object> AnalyzeArchitecture(List<FileMetadata> files, ProjectType projectType)
    {
        var architecture = new Dictionary<string, object>();

        // Простая группировка по папкам для демонстрации
        var folders = files
            .Select(f => GetNormalizedDirectoryName(f.RelativePath))
            .Where(dir => !string.IsNullOrEmpty(dir))
            .Distinct()
            .ToList();

        architecture["Folders"] = folders;
        architecture["FileCount"] = files.Count;

        // Группировка по расширениям
        var extensions = files
            .Select(f => Path.GetExtension(f.FullPath).ToLower())
            .GroupBy(ext => ext)
            .Select(g => new { Extension = g.Key, Count = g.Count() })
            .ToDictionary(x => x.Extension, x => x.Count);

        architecture["FileExtensions"] = extensions;

        return architecture;
    }

    public List<string> AnalyzeDependencies(List<FileMetadata> files, ProjectType projectType)
    {
        var dependencies = new List<string>();

        // Простой анализ зависимостей по типу проекта
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

            case ProjectType.Python:
                var reqFiles = files.Where(f => 
                    NormalizePath(f.FullPath).ToLowerInvariant().Contains("requirements.txt")).ToList();
                if (reqFiles.Any())
                {
                    dependencies.Add("Python requirements.txt detected");
                }
                break;

            case ProjectType.JavaScript:
            case ProjectType.TypeScript:
                var packageJsonFiles = files.Where(f => 
                    NormalizePath(f.FullPath).ToLowerInvariant().EndsWith("package.json")).ToList();
                if (packageJsonFiles.Any())
                {
                    dependencies.Add("Node.js package.json detected");
                }
                break;
        }

        return dependencies;
    }

    /// <summary>
    /// Сканирует файлы проекта с учётом исключений (переопределяемый метод для наследников).
    /// </summary>
    protected virtual List<FileMetadata> ScanProjectFiles(string projectPath, LanguageSettings settings)
    {
        var files = new List<FileMetadata>();

        foreach (var extension in settings.FileExtensions)
        {
            foreach (var file in Directory.GetFiles(projectPath, $"*{extension}", SearchOption.AllDirectories))
            {
                // Проверка исключений с нормализацией путей
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
    /// Проверяет, следует ли исключить файл из анализа (переопределяемый метод для наследников).
    /// </summary>
    protected virtual bool ShouldExclude(string filePath, string[] excludePatterns)
    {
        var fileName = Path.GetFileName(filePath);
        var directoryName = Path.GetDirectoryName(filePath);

        if (string.IsNullOrEmpty(directoryName))
            return false;

        // Нормализуем путь файла для согласования разделителей между ОС
        string normalizedDir = NormalizePath(directoryName);
        normalizedDir = normalizedDir.ToLowerInvariant();

        foreach (var pattern in excludePatterns)
        {
            // Нормализуем паттерн (заменяем \ на /)
            string normalizedPattern = NormalizePath(pattern);
            normalizedPattern = normalizedPattern.ToLowerInvariant();

            // Проверяем, содержит ли путь исключаемую папку (регистронезависимо)
            // Учитываем границы сегмента пути для точности
            if (HasPathSegment(normalizedDir, normalizedPattern))
                return true;

            // Проверка по имени файла с wildcard
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

        // Заменяем специальные символы, оставляя * и ? как есть для дальнейшей обработки
        var escaped = Regex.Escape(pattern);
        
        // Заменяем экранированные джокеры обратно на regex-метасимволы
        return "^" + escaped
            .Replace("\\*", ".*")   // Замена \* на .* (любой текст)
            .Replace("\\?", ".")    // Замена \? на . (любой один символ)
            + "$";
    }

    #region Helper Methods

    /// <summary>
    /// Нормализует путь: заменяет все обратные слэши на косые, устраняет дубли.
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        // Заменяем все разделители на единый формат (/)
        var normalized = path.Replace('\\', '/');
        
        // Устраняем двойные слэши
        while (normalized.Contains("//"))
            normalized = normalized.Replace("//", "/");
        
        // Убираем ведущий слэш если он появился при нормализации корня
        if (normalized.StartsWith("/") && normalized.Length > 1)
            normalized = normalized.Substring(1);

        return normalized;
    }

    /// <summary>
    /// Получает имя директории с нормализованными путями.
    /// </summary>
    private static string GetNormalizedDirectoryName(string relativePath)
    {
        var dirName = Path.GetDirectoryName(relativePath);
        return dirName != null ? NormalizePath(dirName) : null;
    }

    /// <summary>
    /// Проверяет наличие сегмента пути с границами (папка, а не часть имени).
    /// </summary>
    private static bool HasPathSegment(string normalizedPath, string segment)
    {
        if (string.IsNullOrEmpty(normalizedPath) || string.IsNullOrEmpty(segment))
            return false;

        // Добавляем слэши для проверки полных совпадений сегментов
        var fullPathMatch = $"/{segment}/";
        var startMatch = $"{segment}/";       // Начало пути
        var endMatch = $"/{segment}";         // Конец пути
        var exactMatch = segment;             // Точное совпадение всего пути
        
        // Проверяем разные варианты расположения сегмента
        return normalizedPath.Contains(fullPathMatch, StringComparison.Ordinal) ||
               normalizedPath.Contains(startMatch, StringComparison.Ordinal) ||
               normalizedPath.Contains(endMatch, StringComparison.Ordinal) ||
               normalizedPath.Equals(exactMatch, StringComparison.Ordinal);
    }

    #endregion
}