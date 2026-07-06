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
using LLMCodeExporter.Infrastructure.Utils;
using LLMCodeExporter.Infrastructure.Utils.Architecture;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MarkdownFormatter : IOutputFormatter
{
    private readonly ExportSettings _settings;

    public MarkdownFormatter(ExportSettings settings)
    {
        _settings = settings;
    }

    public string FormatHeader(ProjectInfo projectInfo)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# 🚀 Code Export для LLM");
        sb.AppendLine();
        sb.AppendLine($"**Проект:** `{projectInfo.ProjectName}`");
        sb.AppendLine($"**Путь:** `{projectInfo.ProjectPath}`");
        sb.AppendLine();
        sb.Append(projectInfo.Metadata.ToMarkdown());
        sb.AppendLine();
        sb.AppendLine($"💡 {projectInfo.Metadata.GetLLMRecommendations()}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();

        // Quick Links
        var quickLinksSettings = new ExportSettings
        {
            GenerateQuickLinks = true,
            EntryPoints = new List<string> { "Program.cs", "Startup.cs", "ApplicationRunner.cs", "Main.cs" }
        };
        sb.Append(NavigationGenerator.GenerateQuickLinks(projectInfo.Files, quickLinksSettings));
        sb.AppendLine();

        // Architecture Overview
        sb.Append(ArchitectureAnalyzer.GenerateArchitectureOverview(projectInfo, _settings.ProjectType));
        sb.AppendLine();

        // Dependency Graph
        sb.Append(DependencyAnalyzer.GenerateDependencyGraph(projectInfo.Files));
        sb.AppendLine();

        // Structure
        sb.AppendLine("## 📁 Структура проекта");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        var filePaths = projectInfo.Files.Select(f => f.FullPath).ToArray();
        sb.Append(ProjectStructureBuilder.BuildHierarchicalTree(projectInfo.ProjectPath, filePaths));
        sb.AppendLine("```");
        sb.AppendLine();
        sb.Append(ProjectStructureBuilder.BuildStatisticsByFolder(projectInfo.ProjectPath, projectInfo.Files));
        sb.AppendLine("---");
        sb.AppendLine();

        // === НОВЫЕ СЕКЦИИ (ВСЁ ВНУТРИ МЕТОДА) ===

        // 1. Архитектурная диаграмма слоёв
        var archDiagram = ArchitectureDiagramGenerator.GenerateLayerDiagram(
            projectInfo.Metadata.Architecture,
            _settings.ProjectType
        );
        if (!string.IsNullOrEmpty(archDiagram))
            sb.Append(archDiagram);

        // 2. Интеграция между слоями (таблица)
        if (projectInfo.Metadata.IntegrationPoints.Any())
        {
            sb.AppendLine("## 🔌 Интеграция между слоями");
            sb.AppendLine();
            sb.AppendLine("| Направление | Исходный файл | Описание |");
            sb.AppendLine("|-------------|---------------|----------|");
            foreach (var point in projectInfo.Metadata.IntegrationPoints)
            {
                sb.AppendLine($"| {point.Direction} | `{point.SourceFile}` | {point.TargetDescription} |");
            }
            sb.AppendLine();
        }

        // 3. Интеграционная диаграмма (Mermaid)
        var integrationDiagram = IntegrationDiagramGenerator.GenerateIntegrationDiagram(
            projectInfo.Metadata.IntegrationPoints
        );
        if (!string.IsNullOrEmpty(integrationDiagram))
            sb.Append(integrationDiagram);

        // 4. Нарушения архитектурных правил
        if (projectInfo.Metadata.DependencyViolations.Any())
        {
            sb.AppendLine("## 🚨 Нарушения архитектурных правил");
            sb.AppendLine();
            sb.AppendLine("| Исходный файл | Слой | Целевой файл | Слой | Описание |");
            sb.AppendLine("|---------------|------|--------------|------|----------|");
            foreach (var violation in projectInfo.Metadata.DependencyViolations)
            {
                sb.AppendLine($"| `{violation.SourceFile}` | {violation.SourceLayer} | `{violation.TargetFile}` | {violation.TargetLayer} | {violation.RuleDescription} |");
            }
            sb.AppendLine();
        }

        // 5. Семантическое описание архитектуры
        if (!string.IsNullOrEmpty(projectInfo.Metadata.SemanticDescription))
        {
            sb.AppendLine("## 📖 Семантическое описание архитектуры");
            sb.AppendLine();
            sb.AppendLine(projectInfo.Metadata.SemanticDescription);
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        // 6. Метрики качества кода
        if (projectInfo.Metadata.Metrics != null && projectInfo.Metadata.Metrics.TotalLinesOfCode > 0)
        {
            sb.AppendLine("## 📊 Метрики качества кода");
            sb.AppendLine();
            var metrics = projectInfo.Metadata.Metrics;
            sb.AppendLine("| Метрика | Значение |");
            sb.AppendLine("|---------|----------|");
            sb.AppendLine($"| Строк кода (SLOC) | {metrics.TotalLinesOfCode:N0} |");
            sb.AppendLine($"| Количество классов | {metrics.ClassCount} |");
            sb.AppendLine($"| Количество методов | {metrics.MethodCount} |");
            sb.AppendLine($"| Документированных файлов | {metrics.DocumentedFilesCount} |");
            sb.AppendLine($"| Средняя длина метода | {metrics.AverageMethodLength:F1} строк |");
            sb.AppendLine($"| Максимальная длина метода | {metrics.MaxMethodLength} строк |");
            sb.AppendLine($"| Индекс поддерживаемости | {metrics.MaintainabilityIndex:F1} |");
            sb.AppendLine();

            if (metrics.ByLayer.Any())
            {
                sb.AppendLine("### По слоям");
                sb.AppendLine();
                sb.AppendLine("| Слой | SLOC | Классы | Методы |");
                sb.AppendLine("|------|------|--------|--------|");
                foreach (var kvp in metrics.ByLayer.OrderBy(k => k.Key))
                {
                    var m = kvp.Value;
                    sb.AppendLine($"| {kvp.Key} | {m.TotalLinesOfCode:N0} | {m.ClassCount} | {m.MethodCount} |");
                }
                sb.AppendLine();
            }

            sb.AppendLine("---");
            sb.AppendLine();
        }

        // 7. Ключевые компоненты
        if (projectInfo.Metadata.KeyComponents.Any())
        {
            sb.AppendLine("## 🧩 Ключевые компоненты");
            sb.AppendLine();
            sb.AppendLine("| Компонент | Слой | Роль |");
            sb.AppendLine("|-----------|------|------|");
            foreach (var comp in projectInfo.Metadata.KeyComponents.Take(15))
            {
                var entryMarker = comp.IsEntryPoint ? " 🚀" : "";
                var typeMarker = comp.FileType != "Unknown" ? $" ({comp.FileType})" : "";
                sb.AppendLine($"| `{comp.Name}{entryMarker}`{typeMarker} | {comp.Layer} | {comp.Annotation} |");
            }
            if (projectInfo.Metadata.KeyComponents.Count > 15)
            {
                sb.AppendLine($"| ... | ... | _(и ещё {projectInfo.Metadata.KeyComponents.Count - 15} компонентов)_ |");
            }
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public string FormatFile(FileMetadata file, string content, string tag = "")
    {
        var sb = new StringBuilder();
        string tagDisplay = string.IsNullOrEmpty(tag) ? "" : $" {tag}";
        sb.AppendLine($"## 📄 `{file.RelativePath}`{tagDisplay}");
        sb.AppendLine();
        sb.AppendLine("```csharp");
        sb.AppendLine(content);
        sb.AppendLine("```");
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
        sb.AppendLine("## 📊 Итоговая статистика");
        sb.AppendLine();
        sb.AppendLine($"- **Файлов обработано:** {metadata.IncludedFiles}");
        if (metadata.ExcludedFiles > 0)
            sb.AppendLine($"- **Файлов исключено:** {metadata.ExcludedFiles}");
        sb.AppendLine($"- **Символов:** {metadata.OptimizedSize:N0}");
        sb.AppendLine($"- **Токенов:** ~{metadata.EstimatedTokens:N0}");
        if (metadata.Mode != ExportMode.Full)
        {
            sb.AppendLine($"- **Оригинальный размер:** ~{metadata.OriginalEstimatedTokens:N0} токенов");
            sb.AppendLine($"- **Степень сжатия:** {metadata.CompressionRatio:P0}");
        }
        sb.AppendLine($"- **Режим экспорта:** {metadata.Mode}");
        if (metadata.AppliedFilters.Any())
        {
            sb.AppendLine($"- **Применённые фильтры:**");
            foreach (var filter in metadata.AppliedFilters)
                sb.AppendLine($"  - {filter}");
        }
        sb.AppendLine();
        sb.AppendLine($"_Сгенерировано: {metadata.GeneratedAt:dd.MM.yyyy HH:mm:ss}_");
        sb.AppendLine();
        return sb.ToString();
    }
}