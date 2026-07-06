/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Collections.Generic;

namespace LLMCodeExporter.Core.Models;

/// <summary>
/// Определение архитектурного правила: какой слой не должен зависеть от какого
/// </summary>
public class DependencyRule
{
    public string SourceLayer { get; set; } = string.Empty;
    public string ForbiddenTargetLayer { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Нарушение архитектурного правила
/// </summary>
public class DependencyViolation
{
    public string SourceFile { get; set; } = string.Empty;
    public string SourceLayer { get; set; } = string.Empty;
    public string TargetFile { get; set; } = string.Empty;
    public string TargetLayer { get; set; } = string.Empty;
    public string RuleDescription { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning";
}