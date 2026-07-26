/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Core.Models;
public class ExportResult
{
    public bool Success { get; set; }
    public string OutputFilePath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ProjectInfo ProjectInfo { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}