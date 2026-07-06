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
/// Контекст для форматтеров архитектуры
/// </summary>
public class ArchitectureContext
{
    public ProjectInfo ProjectInfo { get; set; } = new();
    public ProjectType ProjectType { get; set; }
    public ExportMetadata Metadata => ProjectInfo.Metadata;
    public List<FileMetadata> Files => ProjectInfo.Files;
    public Dictionary<string, LayerInfo> Layers { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
}