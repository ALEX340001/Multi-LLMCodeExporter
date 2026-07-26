/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Linq;
using System.Text;
using LLMCodeExporter.Core.Models;
namespace LLMCodeExporter.Infrastructure.Utils.Architecture;
public static class ArchitectureDiagramGenerator
{
    public static string GenerateLayerDiagram(ArchitectureInfo architecture, ProjectType projectType)
    {
        if (!architecture.Layers.Any()) return string.Empty;
        var sb = new StringBuilder();
        sb.AppendLine("## 🧱 Архитектурная диаграмма");
        sb.AppendLine();
        sb.AppendLine("<details>");
        sb.AppendLine("<summary>📊 Показать диаграмму слоёв</summary>");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph TD");
        var orderedLayers = architecture.Layers
            .OrderBy(l => GetLayerOrder(l.Name))
            .ToList();
        foreach (var layer in orderedLayers)
        {
            string nodeId = Sanitize(layer.Name);
            string label = $"{layer.Name}\\n({layer.FileCount} файлов)";
            sb.AppendLine($"    {nodeId}[\"{label}\"]");
        }

        for (int i = 0; i < orderedLayers.Count - 1; i++)
        {
            string from = Sanitize(orderedLayers[i].Name);
            string to = Sanitize(orderedLayers[i + 1].Name);
            sb.AppendLine($"    {from} --> {to}");
        }

        sb.AppendLine("    classDef default fill:#f9f9f9,stroke:#333,stroke-width:2px;");
        sb.AppendLine("    classDef domain fill:#e1f5fe,stroke:#01579b;");
        sb.AppendLine("    classDef app fill:#e8f5e9,stroke:#2e7d32;");
        sb.AppendLine("    classDef infra fill:#fff3e0,stroke:#e65100;");
        sb.AppendLine("    classDef ui fill:#fce4ec,stroke:#c62828;");
        foreach (var layer in orderedLayers)
        {
            string className = layer.Name switch
            {
                "Domain" => "domain",
                "Application" => "app",
                "Infrastructure" => "infra",
                "UI" or "Frontend" or "Backend" => "ui",
                _ => "default"
            };
            sb.AppendLine($"    class {Sanitize(layer.Name)} {className};");
        }

        sb.AppendLine("```");
        sb.AppendLine("</details>");
        sb.AppendLine();
        return sb.ToString();
    }

    private static string Sanitize(string name) => name.Replace(" ", "_").Replace("-", "_");
    private static int GetLayerOrder(string name) => name switch
    {
        "Domain" => 1,
        "Application" => 2,
        "Infrastructure" => 3,
        "Controllers" => 4,
        "UI" => 5,
        "Frontend" => 5,
        "Tests" => 6,
        _ => 99
    };
}