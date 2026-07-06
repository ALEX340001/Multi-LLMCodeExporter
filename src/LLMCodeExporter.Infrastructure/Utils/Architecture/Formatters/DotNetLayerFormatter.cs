/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Utils.Architecture.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture.Formatters;

/// <summary>
/// Форматтер для .NET проектов (Clean Architecture, MVC)
/// </summary>
public class DotNetLayerFormatter : ILayerFormatter
{
    private readonly HashSet<ProjectType> _supportedTypes = new()
    {
        ProjectType.CSharp,
        ProjectType.Java,
        ProjectType.AutoDetect,
        ProjectType.Generic
    };

    public bool CanHandle(ArchitectureContext context)
    {
        if (context.ProjectType == ProjectType.Hybrid || context.ProjectType == ProjectType.WebApp)
            return false;

        return _supportedTypes.Contains(context.ProjectType) ||
               context.Files.Any(f => f.RelativePath.EndsWith(".cs") || f.RelativePath.EndsWith(".java")); // <-- исправлено
    }

    public string Format(ArchitectureContext context)
    {
        var sb = new StringBuilder();

        if (HasCleanArchitecture(context.Layers))
        {
            sb.AppendLine("Проект следует **Clean Architecture** (многослойная архитектура):");
            FormatLayer(sb, context.Layers, "Domain", "содержит бизнес-сущности", true);
            FormatLayer(sb, context.Layers, "Application", "реализуют бизнес-логику", false);
            FormatLayer(sb, context.Layers, "Infrastructure", "обрабатывает доступ к данным и внешние зависимости", false);
            FormatLayer(sb, context.Layers, "UI", "содержит пользовательский интерфейс", false);
            FormatLayer(sb, context.Layers, "Tests", "включают модульные и интеграционные тесты", false);
        }
        else if (context.Layers.ContainsKey("Controllers"))
        {
            sb.AppendLine("Проект использует **MVC/Web API** архитектуру:");
            sb.AppendLine("- Controllers обрабатывают HTTP запросы");
            sb.AppendLine("- Models представляют данные");
            sb.AppendLine("- Services содержат бизнес-логику");
        }
        else
        {
            sb.AppendLine("Проект имеет **модульную структуру** с разделением по функциональным слоям:");
            FormatAllLayers(sb, context.Layers);
        }

        return sb.ToString();
    }

    private static bool HasCleanArchitecture(Dictionary<string, LayerInfo> layers)
    {
        return layers.ContainsKey("Domain") &&
               layers.ContainsKey("Application") &&
               layers.ContainsKey("Infrastructure");
    }

    private static void FormatLayer(StringBuilder sb, Dictionary<string, LayerInfo> layers, string name, string description, bool showKeyFiles)
    {
        if (!layers.TryGetValue(name, out var layer)) return;

        var count = layer.FileCount;
        sb.Append($"- **{name}** ({count} файлов) – {description}");

        if (showKeyFiles && layer.KeyFiles.Any())
        {
            var files = layer.KeyFiles.Select(f => Path.GetFileNameWithoutExtension(f));
            sb.Append($": `{string.Join("`, `", files)}`");
        }
        sb.AppendLine();
    }

    private static void FormatAllLayers(StringBuilder sb, Dictionary<string, LayerInfo> layers)
    {
        var orderedLayers = layers.OrderBy(l => l.Key).ToList();
        foreach (var kvp in orderedLayers)
        {
            var count = kvp.Value.FileCount;
            var files = kvp.Value.KeyFiles.Take(3).Select(Path.GetFileName);
            var fileList = files.Any() ? $" (`{string.Join("`, `", files)}`)" : "";
            sb.AppendLine($"- **{kvp.Key}** ({count} файлов){fileList}");
        }
    }
}