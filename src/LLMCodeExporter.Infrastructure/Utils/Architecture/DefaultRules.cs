/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Collections.Generic;
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture;

public static class DefaultRules
{
    public static List<DependencyRule> GetCleanArchitectureRules()
    {
        return new List<DependencyRule>
        {
            new DependencyRule
            {
                SourceLayer = "UI",
                ForbiddenTargetLayer = "Infrastructure",
                Description = "UI не должен напрямую зависеть от Infrastructure (нарушает Clean Architecture)"
            },
            new DependencyRule
            {
                SourceLayer = "UI",
                ForbiddenTargetLayer = "Data",
                Description = "UI не должен напрямую зависеть от Data (нарушает Clean Architecture)"
            },
            new DependencyRule
            {
                SourceLayer = "Application",
                ForbiddenTargetLayer = "Infrastructure",
                Description = "Application не должен напрямую зависеть от Infrastructure (только через абстракции)"
            },
            new DependencyRule
            {
                SourceLayer = "Application",
                ForbiddenTargetLayer = "Data",
                Description = "Application не должен напрямую зависеть от Data (только через абстракции)"
            },
            new DependencyRule
            {
                SourceLayer = "Domain",
                ForbiddenTargetLayer = "Application",
                Description = "Domain не должен зависеть от Application (Domain — ядро)"
            },
            new DependencyRule
            {
                SourceLayer = "Domain",
                ForbiddenTargetLayer = "Infrastructure",
                Description = "Domain не должен зависеть от Infrastructure (Domain — ядро)"
            },
            // Для гибридных проектов можно добавить правила взаимодействия
            new DependencyRule
            {
                SourceLayer = "Frontend",
                ForbiddenTargetLayer = "Backend",
                Description = "Frontend не должен напрямую импортировать backend-код (разделение слоёв)"
            }
        };
    }
}