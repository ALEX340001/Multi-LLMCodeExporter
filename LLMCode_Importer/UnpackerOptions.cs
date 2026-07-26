/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.IO;

namespace LLMCode_Importer;

public record UnpackerOptions
{
    public string OutputRoot { get; init; } = Directory.GetCurrentDirectory();
    public bool Overwrite { get; init; } = true;
    public string? DefaultSnapshotPath { get; init; }
}