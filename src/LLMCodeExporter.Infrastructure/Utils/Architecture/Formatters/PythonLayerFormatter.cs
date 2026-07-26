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
using LLMCodeExporter.Infrastructure.Utils.Architecture.Models;
namespace LLMCodeExporter.Infrastructure.Utils.Architecture.Formatters;
/// <summary>
/// Форматтер для Python проектов
/// </summary>
public class PythonLayerFormatter : ILayerFormatter
{
    public bool CanHandle(ArchitectureContext context) => context.ProjectType == ProjectType.Python;
    public string Format(ArchitectureContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Проект имеет **модульную структуру** с разделением по функциональным слоям:");
        var orderedLayers = context.Layers.OrderBy(l => l.Key).ToList();
        foreach (var kvp in orderedLayers)
        {
            var count = kvp.Value.FileCount;
            var files = kvp.Value.KeyFiles.Take(3).Select(Path.GetFileName);
            var fileList = files.Any() ? $" (`{string.Join("`, `", files)}`)" : "";
            sb.AppendLine($"- **{kvp.Key}** ({count} файлов){fileList}");
        }

        return sb.ToString();
    }
}