/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Linq;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Utils.Architecture.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture;

public static class ArchitectureInfoBuilder
{
    public static ArchitectureInfo Build(ProjectInfo projectInfo, ProjectType projectType)
    {
        var context = ArchitectureAnalyzer.BuildContext(projectInfo, projectType);
        var info = new ArchitectureInfo();

        info.ArchitectureStyle = DetermineStyle(context);

        foreach (var kvp in context.Layers)
        {
            var layer = kvp.Value;
            info.Layers.Add(new ArchitectureLayerInfo
            {
                Name = layer.Name,
                Description = layer.Description ?? GetDefaultDescription(layer.Name),
                FileCount = layer.FileCount,
                KeyFiles = layer.KeyFiles.Take(5).ToList(),
                FilePaths = layer.Files.Select(f => f.RelativePath).ToList()
            });
        }

        info.DetectedPatterns = context.Patterns;
        return info;
    }

    private static string DetermineStyle(ArchitectureContext context)
    {
        if (context.ProjectType == ProjectType.Hybrid) return "Hybrid (Backend + Frontend)";
        if (context.Layers.ContainsKey("Domain") && context.Layers.ContainsKey("Application") && context.Layers.ContainsKey("Infrastructure"))
            return "Clean Architecture";
        if (context.Layers.ContainsKey("Controllers")) return "MVC / Web API";
        if (context.ProjectType == ProjectType.WebApp) return "Web Application (SPA / Static)";
        if (context.ProjectType == ProjectType.Python) return "Python Modular";
        return "Monolithic / Layered";
    }

    private static string GetDefaultDescription(string layerName) => layerName switch
    {
        "Domain" => "Бизнес-сущности и правила",
        "Application" => "Бизнес-логика и сценарии использования",
        "Infrastructure" => "Доступ к данным и внешние сервисы",
        "UI" => "Пользовательский интерфейс",
        "Controllers" => "Контроллеры API",
        "Tests" => "Модульные и интеграционные тесты",
        "Backend" => "Серверная логика",
        "Frontend" => "Клиентская часть",
        _ => ""
    };
}