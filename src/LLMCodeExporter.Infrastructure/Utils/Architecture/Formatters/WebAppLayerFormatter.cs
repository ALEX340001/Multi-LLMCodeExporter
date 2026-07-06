/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Linq;
using System.Text;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Utils.Architecture.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture.Formatters;

/// <summary>
/// Форматтер для веб-приложений (JS, CSS, HTML)
/// </summary>
public class WebAppLayerFormatter : ILayerFormatter
{
    public bool CanHandle(ArchitectureContext context) => context.ProjectType == ProjectType.WebApp;

    public string Format(ArchitectureContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Проект представляет собой **веб-приложение** (JS, CSS, HTML):");

        // HTML Templates
        if (context.Layers.TryGetValue("HTML", out var htmlLayer) && htmlLayer.Files.Any())
        {
            var files = htmlLayer.KeyFiles.Select(f => Path.GetFileNameWithoutExtension(f));
            sb.AppendLine($"- **HTML Templates**: `{string.Join("`, `", files)}`");
        }

        // JavaScript
        if (context.Layers.TryGetValue("JavaScript", out var jsLayer))
            sb.AppendLine($"- **JavaScript/TypeScript** ({jsLayer.FileCount} файлов) - клиентская логика");

        // CSS
        if (context.Layers.TryGetValue("CSS", out var cssLayer))
            sb.AppendLine($"- **CSS/Styles** ({cssLayer.FileCount} файлов) - стилизация");

        // Assets
        if (context.Layers.TryGetValue("Assets", out var assetsLayer))
            sb.AppendLine($"- **Assets/Images** ({assetsLayer.FileCount} файлов) - изображения и ресурсы");

        // Configuration
        if (context.Layers.TryGetValue("Configuration", out var configLayer))
            sb.AppendLine($"- **Configuration** - настройки проекта (package.json, configs)");

        sb.AppendLine($"\n**Сборка расширений:** JS, CSS, HTML");

        return sb.ToString();
    }
}