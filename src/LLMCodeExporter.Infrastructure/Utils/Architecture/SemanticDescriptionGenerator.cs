/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture;

public static class SemanticDescriptionGenerator
{
    public static string Generate(ArchitectureInfo architecture, List<IntegrationPoint> integrations, ProjectType projectType)
    {
        var sb = new StringBuilder();

        // Тип проекта
        sb.Append("Проект представляет собой ");
        if (projectType == ProjectType.WebApp)
            sb.Append("веб-приложение");
        else if (projectType == ProjectType.Hybrid)
            sb.Append("гибридное приложение (Backend + Frontend)");
        else if (projectType == ProjectType.Python)
            sb.Append("Python-проект");
        else if (projectType == ProjectType.CSharp)
            sb.Append(".NET-приложение");
        else if (projectType == ProjectType.JavaScript || projectType == ProjectType.TypeScript)
            sb.Append("JavaScript/TypeScript-приложение");
        else
            sb.Append("программный проект");

        // Архитектурный стиль
        if (!string.IsNullOrEmpty(architecture.ArchitectureStyle))
            sb.Append($" с архитектурой {architecture.ArchitectureStyle}.");

        sb.AppendLine();
        sb.AppendLine();

        // Слои
        if (architecture.Layers.Any())
        {
            sb.AppendLine("**Архитектурные слои:**");
            foreach (var layer in architecture.Layers)
            {
                sb.Append($"- **{layer.Name}** ({layer.FileCount} файлов)");
                if (!string.IsNullOrEmpty(layer.Description))
                    sb.Append($" – {layer.Description}");

                // Покажем несколько ключевых файлов для понимания
                if (layer.KeyFiles.Any())
                {
                    var sampleFiles = layer.KeyFiles.Take(3).Select(f => $"`{f}`");
                    sb.Append($" (например, {string.Join(", ", sampleFiles)})");
                }
                sb.AppendLine();
            }
            sb.AppendLine();
        }

        // Паттерны
        if (architecture.DetectedPatterns.Any())
        {
            sb.Append($"**Обнаружены паттерны:** {string.Join(", ", architecture.DetectedPatterns)}.");
            sb.AppendLine();
            sb.AppendLine();
        }

        // Интеграции (для гибридных)
        if (integrations.Any())
        {
            sb.AppendLine("**Взаимодействие между слоями:**");
            var grouped = integrations.GroupBy(i => i.Direction);
            foreach (var group in grouped)
            {
                sb.Append($"- {group.Key}: ");
                var descriptions = group.Select(i => i.TargetDescription).Distinct().Take(3);
                sb.Append(string.Join("; ", descriptions));
                if (group.Count() > 3)
                    sb.Append($" и ещё {group.Count() - 3} вызовов");
                sb.AppendLine();
            }
            sb.AppendLine();
        }

        // Контекстная подсказка для LLM
        sb.AppendLine("**Рекомендация для анализа:**");
        sb.Append("При изучении кода учитывай следующие архитектурные ограничения: ");
        var hints = new List<string>();
        if (architecture.Layers.Any(l => l.Name == "Application"))
            hints.Add("бизнес-логика находится в слое Application");
        if (architecture.Layers.Any(l => l.Name == "Infrastructure"))
            hints.Add("доступ к данным осуществляется через Infrastructure");
        if (architecture.Layers.Any(l => l.Name == "Domain"))
            hints.Add("бизнес-сущности описаны в Domain");
        if (architecture.Layers.Any(l => l.Name == "UI"))
            hints.Add("пользовательский интерфейс реализован в UI");
        if (architecture.Layers.Any(l => l.Name == "Controllers"))
            hints.Add("API-контроллеры находятся в Controllers");

        if (!hints.Any())
            sb.Append("слои чётко разделены по функциональному признаку.");
        else
            sb.Append(string.Join(", ", hints));

        sb.AppendLine();
        sb.AppendLine("При ответах учитывай эти зависимости и предлагай решения, соответствующие архитектуре.");

        return sb.ToString();
    }
}