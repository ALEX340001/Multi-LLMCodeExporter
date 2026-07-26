/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.Collections.Generic;
using System.Text;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Utils.Architecture.Formatters;
using LLMCodeExporter.Infrastructure.Utils.Architecture.Models;
namespace LLMCodeExporter.Infrastructure.Utils.Architecture;
/// <summary>
/// Главный анализатор архитектуры — orchestrator для всех форматтеров
/// </summary>
public static class ArchitectureAnalyzer
{
    private static readonly List<ILayerFormatter> _formatters = new()
    {
        new HybridLayerFormatter(),
        new WebAppLayerFormatter(),
        new DotNetLayerFormatter(),
        new PythonLayerFormatter()
    };
    /// <summary>
    /// Генерирует краткое описание архитектуры проекта на основе структуры файлов.
    /// </summary>
    public static string GenerateArchitectureOverview(ProjectInfo projectInfo, ProjectType projectType)
    {
        var sb = new StringBuilder();
        sb.AppendLine("## 🏗️ Architecture Overview");
        sb.AppendLine();
        var context = BuildContext(projectInfo, projectType);
        var formatter = FindFormatter(context);
        if (formatter != null)
        {
            sb.Append(formatter.Format(context));
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("Проект имеет **монолитную структуру** с разделением по папкам.");
            sb.AppendLine();
        }

        if (context.Patterns.Any())
        {
            sb.AppendLine($"**Используемые паттерны:** {string.Join(", ", context.Patterns)}");
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();
        return sb.ToString();
    }

    // 🔁 Сделали метод публичным, чтобы использовать в ArchitectureInfoBuilder
    public static ArchitectureContext BuildContext(ProjectInfo projectInfo, ProjectType projectType)
    {
        var context = new ArchitectureContext
        {
            ProjectInfo = projectInfo,
            ProjectType = projectType
        };
        if (projectType == ProjectType.Hybrid)
        {
            var metadata = projectInfo.Metadata;
            context.Layers = LayerDetector.DetectHybridLayers(
                projectInfo.Files,
                metadata.BackendLanguage,
                metadata.FrontendLanguage
            );
        }
        else
        {
            context.Layers = LayerDetector.DetectLayers(projectInfo.Files, projectType);
        }

        context.Patterns = PatternDetector.DetectPatterns(projectInfo.Files);
        return context;
    }

    private static ILayerFormatter? FindFormatter(ArchitectureContext context)
    {
        foreach (var formatter in _formatters)
        {
            if (formatter.CanHandle(context))
                return formatter;
        }
        return null;
    }
}