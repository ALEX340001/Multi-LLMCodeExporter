/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Core.Models;
public class FileMetadata
{
    public string FullPath { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }
    // ИЗМЕНЕНИЕ: добавляем set для свойства EstimatedTokens
    public int EstimatedTokens { get; set; }
    public string Annotation { get; set; } = string.Empty;
}