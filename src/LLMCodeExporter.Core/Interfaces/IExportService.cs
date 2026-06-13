using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Core.Interfaces;

/// <summary>
/// Интерфейс службы экспорта
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Экспортирует проект в заданный формат
    /// </summary>
    /// <param name="projectInfo">Информация о проекте</param>
    /// <param name="settings">Настройки экспорта</param>
    /// <returns>Результат экспорта</returns>
    ExportResult ExportProject(ProjectInfo projectInfo, ExportSettings settings);
}