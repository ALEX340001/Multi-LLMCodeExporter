/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Infrastructure.Formatters;

using Core.Interfaces;
using Core.Models;
using LLMCodeExporter.Infrastructure.Utils;
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

        // Заголовок
        sb.AppendLine("# 🚀 Code Export для LLM");
        sb.AppendLine();

        // Основная информация
        sb.AppendLine($"**Проект:** `{projectInfo.ProjectName}`");
        sb.AppendLine($"**Путь:** `{projectInfo.ProjectPath}`");
        sb.AppendLine();

        // Метаданные экспорта (режим, дата, файлы, токены)
        sb.Append(projectInfo.Metadata.ToMarkdown());
        sb.AppendLine();

        // Рекомендации по LLM
        sb.AppendLine($"💡 {projectInfo.Metadata.GetLLMRecommendations()}");
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine();

        // Quick Links навигация
        var quickLinksSettings = new ExportSettings
        {
            GenerateQuickLinks = true,
            EntryPoints = new List<string> { "Program.cs", "Startup.cs", "ApplicationRunner.cs", "Main.cs" }
        };

        // ✅ Quick Links
        sb.Append(NavigationGenerator.GenerateQuickLinks(projectInfo.Files, quickLinksSettings));
        sb.AppendLine();

        // ✅ Architecture Overview (передаём тип проекта)
        sb.Append(ArchitectureAnalyzer.GenerateArchitectureOverview(projectInfo, _settings.ProjectType));
        sb.AppendLine();

        // ✅ Граф зависимостей
        sb.Append(DependencyAnalyzer.GenerateDependencyGraph(projectInfo.Files));
        sb.AppendLine();

        // Структура проекта
        sb.AppendLine("## 📁 Структура проекта");
        sb.AppendLine();
        sb.AppendLine("```csharp");

        var filePaths = projectInfo.Files.Select(f => f.FullPath).ToArray();
        sb.Append(ProjectStructureBuilder.BuildHierarchicalTree(projectInfo.ProjectPath, filePaths));

        sb.AppendLine("```");
        sb.AppendLine();

        // Статистика по папкам
        sb.Append(ProjectStructureBuilder.BuildStatisticsByFolder(projectInfo.ProjectPath, projectInfo.Files));

        sb.AppendLine("---");
        sb.AppendLine();

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
        {
            sb.AppendLine($"- **Файлов исключено:** {metadata.ExcludedFiles}");
        }

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
            {
                sb.AppendLine($"  - {filter}");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"_Сгенерировано: {metadata.GeneratedAt:dd.MM.yyyy HH:mm:ss}_");
        sb.AppendLine();

        return sb.ToString();
    }
}
