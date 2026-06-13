/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Core.Interfaces;

using Models;

public interface IOutputFormatter
{
    string FormatHeader(ProjectInfo projectInfo);
    string FormatFile(FileMetadata file, string content, string tag = "");
    string FormatFooter(ProjectInfo projectInfo);
}
