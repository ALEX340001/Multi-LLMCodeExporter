/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
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
        sb.AppendLine("║            CODE EXPORT ДЛЯ НЕЙРОСЕТИ v0.3                      ║");
        sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"Проект:          {projectInfo.ProjectName}");
        sb.AppendLine($"Режим:           {metadata.Mode}");
        sb.AppendLine($"Дата:            {DateTime.Now:dd.MM.yyyy HH:mm}");
        sb.AppendLine($"Файлов:          {metadata.IncludedFiles}");
        if (metadata.ExcludedFiles > 0)
            sb.AppendLine($"Исключено:       {metadata.ExcludedFiles}");
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
                sb.AppendLine($"  • {filter}");
            sb.AppendLine();
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("═══ СТРУКТУРА ПРОЕКТА ═══");
        sb.AppendLine();
        var filePaths = projectInfo.Files.Select(f => f.FullPath).ToArray();
        sb.Append(ProjectStructureBuilder.BuildHierarchicalTree(projectInfo.ProjectPath, filePaths));
        sb.AppendLine();
        sb.AppendLine("═════════════════════════════════════════════════════════════════");
        sb.AppendLine();
        // ===== ДОБАВЛЯЕМ АРХИТЕКТУРНЫЕ СЛОИ =====
        var arch = projectInfo.Metadata.Architecture;
        if (arch.Layers.Any())
        {
            sb.AppendLine("═══ АРХИТЕКТУРНЫЕ СЛОИ ═══");
            sb.AppendLine();
            sb.AppendLine($"Стиль: {arch.ArchitectureStyle}");
            foreach (var layer in arch.Layers)
            {
                sb.AppendLine($"  • {layer.Name} ({layer.FileCount} файлов)");
                if (layer.KeyFiles.Any())
                    sb.AppendLine($"      Ключевые файлы: {string.Join(", ", layer.KeyFiles)}");
            }
            sb.AppendLine();
        }
        // Семантическое описание
if (!string.IsNullOrEmpty(projectInfo.Metadata.SemanticDescription))
{
    sb.AppendLine("═══ СЕМАНТИЧЕСКОЕ ОПИСАНИЕ АРХИТЕКТУРЫ ═══");
    sb.AppendLine();
    sb.AppendLine(projectInfo.Metadata.SemanticDescription);
    sb.AppendLine();
}

// Метрики качества
if (projectInfo.Metadata.Metrics != null && projectInfo.Metadata.Metrics.TotalLinesOfCode > 0)
{
    sb.AppendLine("═══ МЕТРИКИ КАЧЕСТВА КОДА ═══");
    sb.AppendLine();
    var metrics = projectInfo.Metadata.Metrics;
    sb.AppendLine($"  Строк кода (SLOC):     {metrics.TotalLinesOfCode:N0}");
    sb.AppendLine($"  Количество классов:    {metrics.ClassCount}");
    sb.AppendLine($"  Количество методов:    {metrics.MethodCount}");
    sb.AppendLine($"  Документированных файлов: {metrics.DocumentedFilesCount}");
    sb.AppendLine($"  Средняя длина метода:  {metrics.AverageMethodLength:F1} строк");
    sb.AppendLine($"  Максимальная длина метода: {metrics.MaxMethodLength} строк");
    sb.AppendLine($"  Индекс поддерживаемости: {metrics.MaintainabilityIndex:F1}");
    sb.AppendLine();
    if (metrics.ByLayer.Any())
    {
        sb.AppendLine("  По слоям:");
        foreach (var kvp in metrics.ByLayer.OrderBy(k => k.Key))
        {
            var m = kvp.Value;
            sb.AppendLine($"    {kvp.Key,-15} SLOC: {m.TotalLinesOfCode,6:N0}  Классы: {m.ClassCount,3}  Методы: {m.MethodCount,3}");
        }
        sb.AppendLine();
    }
}

// Ключевые компоненты
if (projectInfo.Metadata.KeyComponents.Any())
{
    sb.AppendLine("═══ КЛЮЧЕВЫЕ КОМПОНЕНТЫ ═══");
    sb.AppendLine();
    sb.AppendLine("  Компонент                 | Слой     | Роль");
    sb.AppendLine("  --------------------------|----------|-----");
    foreach (var comp in projectInfo.Metadata.KeyComponents.Take(15))
    {
        var entryMarker = comp.IsEntryPoint ? " 🚀" : "";
        var typeMarker = comp.FileType != "Unknown" ? $" ({comp.FileType})" : "";
        sb.AppendLine($"  {comp.Name,-25}{entryMarker} {comp.Layer,-10} {comp.Annotation}");
    }
    if (projectInfo.Metadata.KeyComponents.Count > 15)
    {
        sb.AppendLine($"  ... и ещё {projectInfo.Metadata.KeyComponents.Count - 15} компонентов");
    }
    sb.AppendLine();
}

        // ===== НАРУШЕНИЯ ПРАВИЛ =====
        if (projectInfo.Metadata.DependencyViolations.Any())
        {
            sb.AppendLine("═══ НАРУШЕНИЯ АРХИТЕКТУРНЫХ ПРАВИЛ ═══");
            sb.AppendLine();
            foreach (var violation in projectInfo.Metadata.DependencyViolations)
            {
                sb.AppendLine($"  • {violation.SourceFile} ({violation.SourceLayer}) -> {violation.TargetFile} ({violation.TargetLayer})");
                sb.AppendLine($"    Правило: {violation.RuleDescription}");
            }
            sb.AppendLine();
        }

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
            sb.AppendLine($"  Файлов исключено:  {metadata.ExcludedFiles}");
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