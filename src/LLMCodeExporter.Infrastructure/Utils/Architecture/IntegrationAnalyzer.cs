/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture;

public static class IntegrationAnalyzer
{
    public static List<IntegrationPoint> Analyze(List<FileMetadata> files, ExportSettings settings)
    {
        var points = new List<IntegrationPoint>();

        if (settings.ProjectType != ProjectType.Hybrid)
            return points;

        // 1. Backend → Frontend: IJSRuntime в C#
        foreach (var file in files.Where(f => f.RelativePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var content = File.ReadAllText(file.FullPath);
                if (content.Contains("IJSRuntime") || content.Contains("JSRuntime"))
                {
                    var matches = Regex.Matches(content, @"(?:IJSRuntime|JSRuntime)\s*\.\s*(?:Invoke(?:Async|VoidAsync)?)\s*\(\s*""([^""]+)""");
                    foreach (Match m in matches)
                    {
                        points.Add(new IntegrationPoint
                        {
                            Direction = "Backend → Frontend",
                            SourceFile = file.RelativePath,
                            TargetDescription = $"Вызов JS-функции: {m.Groups[1].Value}"
                        });
                    }
                    if (!matches.Any())
                    {
                        points.Add(new IntegrationPoint
                        {
                            Direction = "Backend → Frontend",
                            SourceFile = file.RelativePath,
                            TargetDescription = "Использование IJSRuntime (общий вызов)"
                        });
                    }
                }
            }
            catch { }
        }

        // 2. Frontend → Backend: fetch/axios в JS/TS
        var frontendExtensions = new[] { ".js", ".jsx", ".ts", ".tsx" };
        foreach (var file in files.Where(f => frontendExtensions.Contains(Path.GetExtension(f.RelativePath).ToLowerInvariant())))
        {
            try
            {
                var content = File.ReadAllText(file.FullPath);
                // fetch
                var fetchMatches = Regex.Matches(content, @"fetch\s*\(\s*['""`]([^'""`]+)['""`]");
                foreach (Match m in fetchMatches)
                {
                    var url = m.Groups[1].Value;
                    if (url.Contains("/api") || url.Contains("api.") || url.Contains("api/"))
                    {
                        points.Add(new IntegrationPoint
                        {
                            Direction = "Frontend → Backend",
                            SourceFile = file.RelativePath,
                            TargetDescription = $"HTTP запрос: {url}"
                        });
                    }
                }
                // axios
                var axiosMatches = Regex.Matches(content, @"axios\s*\.\s*(?:get|post|put|delete|patch)\s*\(\s*['""`]([^'""`]+)['""`]");
                foreach (Match m in axiosMatches)
                {
                    var url = m.Groups[1].Value;
                    if (url.Contains("/api") || url.Contains("api.") || url.Contains("api/"))
                    {
                        points.Add(new IntegrationPoint
                        {
                            Direction = "Frontend → Backend",
                            SourceFile = file.RelativePath,
                            TargetDescription = $"Axios запрос: {url}"
                        });
                    }
                }
            }
            catch { }
        }

        return points;
    }
}