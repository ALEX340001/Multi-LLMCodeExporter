using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Core.Interfaces;

/// <summary>
/// Интерфейс настроек экспорта
/// </summary>
public interface IExportSettings
{
    /// <summary>
    /// Режим экспорта
    /// </summary>
    ExportMode Mode { get; set; }

    /// <summary>
    /// Тип проекта
    /// </summary>
    ProjectType ProjectType { get; set; }

    /// <summary>
    /// Включить комментарии
    /// </summary>
    bool IncludeComments { get; set; }

    /// <summary>
    /// Включить минификацию
    /// </summary>
    bool Minify { get; set; }

    /// <summary>
    /// Исключаемые папки
    /// </summary>
    string[] ExcludeFolders { get; set; }

    /// <summary>
    /// Исключаемые файлы
    /// </summary>
    string[] ExcludeFiles { get; set; }

    /// <summary>
    /// Максимальный размер файла (в байтах)
    /// </summary>
    long MaxFileSize { get; set; }

    /// <summary>
    /// Максимальное количество токенов
    /// </summary>
    int MaxTokens { get; set; }
}