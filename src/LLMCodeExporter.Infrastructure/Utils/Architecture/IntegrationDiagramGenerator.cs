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

namespace LLMCodeExporter.Infrastructure.Utils.Architecture;

public static class IntegrationDiagramGenerator
{
    public static string GenerateIntegrationDiagram(List<IntegrationPoint> points)
    {
        if (!points.Any()) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("## 🔌 Интеграционная диаграмма");
        sb.AppendLine();
        sb.AppendLine("<details>");
        sb.AppendLine("<summary>📊 Показать интеграционные связи</summary>");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph LR");

        bool hasBackend = points.Any(p => p.Direction.Contains("Backend"));
        bool hasFrontend = points.Any(p => p.Direction.Contains("Frontend"));

        if (hasBackend) sb.AppendLine("    Backend[\"Backend (C#)\"]");
        if (hasFrontend) sb.AppendLine("    Frontend[\"Frontend (JavaScript/TypeScript)\"]");

        foreach (var point in points)
        {
            string from, to;
            if (point.Direction.Contains("Backend → Frontend"))
            {
                from = "Backend";
                to = "Frontend";
            }
            else
            {
                from = "Frontend";
                to = "Backend";
            }
            // Экранируем кавычки в описании
            var label = point.TargetDescription.Replace("\"", "\\\"").Replace("\n", " ");
            sb.AppendLine($"    {from} -->|\"{label}\"| {to}");
        }

        sb.AppendLine("```");
        sb.AppendLine("</details>");
        sb.AppendLine();
        return sb.ToString();
    }
}