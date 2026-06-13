namespace LLMCodeExporter.Infrastructure.Formatters;

using Core.Interfaces;
using Core.Models;
using Utils;
using System.Text;

public class PlainTextFormatter : IOutputFormatter
{
    public string FormatHeader(ProjectInfo projectInfo)
    {
        var sb = new StringBuilder();
        var metadata = projectInfo.Metadata;

        sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║            CODE EXPORT ДЛЯ НЕЙРОСЕТИ v2.0                     ║");
        sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"Проект:          {projectInfo.ProjectName}");
        sb.AppendLine($"Режим:           {metadata.Mode}");
        sb.AppendLine($"Дата:            {DateTime.Now:dd.MM.yyyy HH:mm}");
        sb.AppendLine($"Файлов:          {metadata.IncludedFiles}");

        if (metadata.ExcludedFiles > 0)
        {
            sb.AppendLine($"Исключено:       {metadata.ExcludedFiles}");
        }

        sb.AppendLine($"Токенов:         ~{metadata.EstimatedTokens:N0}");

        if (metadata.Mode != ExportMode.Full)
        {
            sb.AppendLine($"Оригинал:        ~{metadata.OriginalEstimatedTokens:N0}");
            sb.AppendLine($"Сжатие:          {metadata.CompressionRatio:P0}");
        }

        sb.AppendLine($"Рекомендуется:   {metadata.RecommendedContextWindow}");
        sb.AppendLine();

        if (metadata.AppliedFilters.Any())
        {
            sb.AppendLine("Применённые фильтры:");
            foreach (var filter in metadata.AppliedFilters)
            {
                sb.AppendLine($"  • {filter}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine();

        // Структура проекта
        sb.AppendLine("═══ СТРУКТУРА ПРОЕКТА ═══");
        sb.AppendLine();

        var filePaths = projectInfo.Files.Select(f => f.FullPath).ToArray();
        sb.Append(ProjectStructureBuilder.BuildHierarchicalTree(projectInfo.ProjectPath, filePaths));

        sb.AppendLine();
        sb.AppendLine("═════════════════════════════════════════════════════════════════");
        sb.AppendLine();

        return sb.ToString();
    }

    public string FormatFile(FileMetadata file, string content, string tag = "")
    {
        var sb = new StringBuilder();
        string tagDisplay = string.IsNullOrEmpty(tag) ? "" : $" {tag}";
        sb.AppendLine(new string('=', 80));
        sb.AppendLine($"FILE: {file.RelativePath}{tagDisplay}");
        sb.AppendLine(new string('=', 80));
        sb.AppendLine();
        sb.AppendLine(content);
        sb.AppendLine();
        return sb.ToString();
    }

    public string FormatFooter(ProjectInfo projectInfo)
    {
        var sb = new StringBuilder();
        var metadata = projectInfo.Metadata;

        sb.AppendLine();
        sb.AppendLine(new string('─', 80));
        sb.AppendLine();
        sb.AppendLine("ИТОГОВАЯ СТАТИСТИКА:");
        sb.AppendLine($"  Файлов обработано: {metadata.IncludedFiles}");

        if (metadata.ExcludedFiles > 0)
        {
            sb.AppendLine($"  Файлов исключено:  {metadata.ExcludedFiles}");
        }

        sb.AppendLine($"  Символов:          {metadata.OptimizedSize:N0}");
        sb.AppendLine($"  Токенов:           ~{metadata.EstimatedTokens:N0}");

        if (metadata.Mode != ExportMode.Full)
        {
            sb.AppendLine($"  Оригинал:          ~{metadata.OriginalEstimatedTokens:N0} токенов");
            sb.AppendLine($"  Сжатие:            {metadata.CompressionRatio:P0}");
        }

        sb.AppendLine();
        sb.AppendLine($"Сгенерировано: {metadata.GeneratedAt:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine();

        return sb.ToString();
    }
}
