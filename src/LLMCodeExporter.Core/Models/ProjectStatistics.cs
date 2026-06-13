namespace LLMCodeExporter.Core.Models;

/// <summary>
/// Статистика проекта
/// </summary>
public class ProjectStatistics
{
    /// <summary>
    /// Общее количество файлов
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Количество включенных файлов
    /// </summary>
    public int IncludedFiles { get; set; }

    /// <summary>
    /// Количество исключенных файлов
    /// </summary>
    public int ExcludedFiles => TotalFiles - IncludedFiles;

    /// <summary>
    /// Общий размер файлов в байтах
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Размер файлов в удобочитаемом формате
    /// </summary>
    public string TotalSizeFormatted
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = TotalSizeBytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    /// <summary>
    /// Оценка количества токенов
    /// </summary>
    public int EstimatedTokens { get; set; }

    /// <summary>
    /// Тип проекта
    /// </summary>
    public ProjectType ProjectType { get; set; }

    /// <summary>
    /// Список расширений файлов с количеством
    /// </summary>
    public Dictionary<string, int> FileExtensions { get; set; } = new();

    /// <summary>
    /// Список зависимостей
    /// </summary>
    public List<string> Dependencies { get; set; } = new();
}