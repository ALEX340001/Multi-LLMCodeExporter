using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Core.Interfaces;

/// <summary>
/// Интерфейс для анализатора проектов
/// </summary>
public interface IProjectAnalyzer
{
    /// <summary>
    /// Анализирует проект и возвращает информацию о нем
    /// </summary>
    /// <param name="projectPath">Путь к проекту</param>
    /// <returns>Информация о проекте</returns>
    ProjectInfo AnalyzeProject(string projectPath);

    /// <summary>
    /// Определяет тип проекта
    /// </summary>
    /// <param name="projectPath">Путь к проекту</param>
    /// <returns>Тип проекта</returns>
    ProjectType DetectProjectType(string projectPath);

    /// <summary>
    /// Оценивает количество токенов в файле
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns>Примерное количество токенов</returns>
    int EstimateFileTokens(string filePath);

    /// <summary>
    /// Оценивает общее количество токенов в списке файлов
    /// </summary>
    /// <param name="files">Список файлов</param>
    /// <returns>Общее количество токенов</returns>
    int EstimateTokens(List<FileMetadata> files);

    /// <summary>
    /// Анализирует архитектуру проекта
    /// </summary>
    /// <param name="files">Список файлов проекта</param>
    /// <param name="projectType">Тип проекта</param>
    /// <returns>Словарь с информацией об архитектуре</returns>
    Dictionary<string, object> AnalyzeArchitecture(List<FileMetadata> files, ProjectType projectType);

    /// <summary>
    /// Анализирует зависимости проекта
    /// </summary>
    /// <param name="files">Список файлов проекта</param>
    /// <param name="projectType">Тип проекта</param>
    /// <returns>Список зависимостей проекта</returns>
    List<string> AnalyzeDependencies(List<FileMetadata> files, ProjectType projectType);
}