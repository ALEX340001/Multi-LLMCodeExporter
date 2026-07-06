/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Collections.Generic;
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture.Models;

/// <summary>
/// Информация о слое архитектуры
/// </summary>
public class LayerInfo
{
    public string Name { get; set; } = string.Empty;
    public List<FileMetadata> Files { get; set; } = new();
    public string? Description { get; set; }
    public int FileCount => Files.Count;
    public List<string> KeyFiles => GetKeyFiles(3);

    private List<string> GetKeyFiles(int count)
    {
        var result = new List<string>();
        for (int i = 0; i < System.Math.Min(count, Files.Count); i++)
        {
            var relativePath = Files[i].RelativePath;
            if (!string.IsNullOrEmpty(relativePath))
                result.Add(relativePath);
        }
        return result;
    }
}