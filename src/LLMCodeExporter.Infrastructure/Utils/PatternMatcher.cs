namespace LLMCodeExporter.Infrastructure.Utils;

using System.Text.RegularExpressions;

/// <summary>
/// Утилита для сопоставления файлов с паттернами (glob patterns)
/// </summary>
public static class PatternMatcher
{
    /// <summary>
    /// Проверяет соответствует ли путь файла паттерну
    /// </summary>
   public static bool MatchesPattern(string filePath, string pattern)
{
    // Нормализуем пути к формату с '/' для упрощения
    string normalizedPath = filePath.Replace('\\', '/');
    string normalizedPattern = pattern.Replace('\\', '/');

    // Паттерн с расширением (например: *.Designer.cs)
    if (normalizedPattern.StartsWith("*"))
    {
        string extension = normalizedPattern.Substring(1);
        return normalizedPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase);
    }

    // Паттерн папки (например: Forms, bin/Debug)
    if (!normalizedPattern.Contains("*") && !normalizedPattern.Contains("."))
    {
        var pathParts = normalizedPath.Split('/');
        var patternParts = normalizedPattern.Split('/');
        // Проверяем, содержит ли путь все части паттерна (как вложенность)
        // Более гибко: ищем последовательное вхождение patternParts в pathParts
        for (int i = 0; i <= pathParts.Length - patternParts.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < patternParts.Length; j++)
            {
                if (!pathParts[i + j].Equals(patternParts[j], StringComparison.OrdinalIgnoreCase))
                {
                    match = false;
                    break;
                }
            }
            if (match) return true;
        }
        return false;
    }

    // Полный путь с wildcard (например: */Forms/*.cs)
    if (normalizedPattern.Contains("*"))
    {
        string regexPattern = "^" + Regex.Escape(normalizedPattern)
            .Replace("\\*\\*", ".*")  // ** = любая вложенность
            .Replace("\\*", "[^/]*")   // * = любые символы кроме /
            .Replace("\\?", "[^/]")    // ? = один символ
            + "$";
        return Regex.IsMatch(normalizedPath, regexPattern, RegexOptions.IgnoreCase);
    }

    // Точное совпадение (с учётом регистра? используем IgnoreCase)
    return normalizedPath.EndsWith(normalizedPattern, StringComparison.OrdinalIgnoreCase);
}

    /// <summary>
    /// Проверяет соответствует ли файл хотя бы одному из паттернов
    /// </summary>
    public static bool MatchesAnyPattern(string filePath, IEnumerable<string> patterns)
    {
        return patterns.Any(pattern => MatchesPattern(filePath, pattern));
    }

    /// <summary>
    /// Фильтрует список файлов по паттернам включения и исключения
    /// </summary>
    public static (List<T> included, List<T> excluded) FilterByPatterns<T>(
        IEnumerable<T> files,
        Func<T, string> pathSelector,
        List<string> includePatterns,
        List<string> excludePatterns)
    {
        var included = new List<T>();
        var excluded = new List<T>();

        foreach (var file in files)
        {
            string filePath = pathSelector(file);

            // Сначала проверяем исключения
            if (excludePatterns.Any() && MatchesAnyPattern(filePath, excludePatterns))
            {
                excluded.Add(file);
                continue;
            }

            // Если есть паттерны включения - проверяем их
            if (includePatterns.Any())
            {
                if (MatchesAnyPattern(filePath, includePatterns))
                {
                    included.Add(file);
                }
                else
                {
                    excluded.Add(file);
                }
            }
            else
            {
                // Нет паттернов включения - включаем всё что не исключено
                included.Add(file);
            }
        }

        return (included, excluded);
    }

    /// <summary>
    /// Получает человеко-читаемое описание паттерна
    /// </summary>
    public static string GetPatternDescription(string pattern)
    {
        if (pattern.StartsWith("*."))
        {
            return $"файлы {pattern}";
        }

        if (!pattern.Contains("*") && !pattern.Contains("."))
        {
            return $"папка '{pattern}'";
        }

        return $"паттерн '{pattern}'";
    }
}
