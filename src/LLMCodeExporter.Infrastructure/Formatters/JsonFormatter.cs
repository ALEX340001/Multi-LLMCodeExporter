/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Text.Json;
using System.Text.Json.Serialization;
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Formatters;

public class JsonFormatter
{
    public string Format(ProjectInfo projectInfo)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        var exportData = new
        {
            projectInfo.ProjectName,
            projectInfo.ProjectPath,
            projectInfo.Metadata,
            Files = projectInfo.Files.Select(f => new
            {
                f.RelativePath,
                f.SizeInBytes,
                f.EstimatedTokens,
                f.Annotation
            }),
            ExcludedFiles = projectInfo.ExcludedFiles.Select(f => new
            {
                f.RelativePath,
                f.SizeInBytes
            }),
            TotalFiles = projectInfo.TotalFiles,
            TotalScannedFiles = projectInfo.TotalScannedFiles,
            EstimatedTokens = projectInfo.EstimatedTokens
        };

        return JsonSerializer.Serialize(exportData, options);
    }
}