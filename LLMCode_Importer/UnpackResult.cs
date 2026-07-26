/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.Collections.Generic;

namespace LLMCode_Importer;

public record UnpackResult
{
    public required int ExtractedCount { get; init; }
    public required string OutputRoot { get; init; }
    public IReadOnlyList<string> ExtractedFiles { get; init; } = Array.Empty<string>();
}