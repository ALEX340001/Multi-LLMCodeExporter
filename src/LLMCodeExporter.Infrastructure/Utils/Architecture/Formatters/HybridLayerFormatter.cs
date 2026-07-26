/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Text;
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Utils.Architecture.Models;
namespace LLMCodeExporter.Infrastructure.Utils.Architecture.Formatters;
/// <summary>
/// Форматтер для гибридных проектов
/// </summary>
public class HybridLayerFormatter : ILayerFormatter
{
    public bool CanHandle(ArchitectureContext context) => context.ProjectType == ProjectType.Hybrid;
    public string Format(ArchitectureContext context)
    {
        var sb = new StringBuilder();
        var metadata = context.Metadata;
        sb.AppendLine($"Проект является **гибридным** с разделением на бекенд и фронтенд:");
        sb.AppendLine($"- **Backend** ({metadata.BackendLanguage}) – серверная логика, API, бизнес-слой");
        sb.AppendLine($"- **Frontend** ({metadata.FrontendLanguage}) – пользовательский интерфейс, клиентские скрипты");
        sb.AppendLine($"- **Configuration** – общие настройки, конфигурационные файлы");
        sb.AppendLine();
        // Статистика по слоям
        if (metadata.BackendFilesCount > 0 || metadata.FrontendFilesCount > 0)
        {
            sb.AppendLine("📊 **Распределение файлов по слоям:**");
            sb.AppendLine($"   • Backend: {metadata.BackendFilesCount} файлов");
            sb.AppendLine($"   • Frontend: {metadata.FrontendFilesCount} файлов");
            var other = metadata.TotalFiles - metadata.BackendFilesCount - metadata.FrontendFilesCount;
            if (other > 0)
                sb.AppendLine($"   • Конфигурация и прочее: {other} файлов");
            sb.AppendLine();
        }

        // Ключевые файлы
        if (context.Layers.TryGetValue("Backend", out var backendLayer))
            AppendKeyFiles(sb, "бекенда", backendLayer.KeyFiles, backendLayer.FileCount);
        if (context.Layers.TryGetValue("Frontend", out var frontendLayer))
            AppendKeyFiles(sb, "фронтенда", frontendLayer.KeyFiles, frontendLayer.FileCount);
        return sb.ToString();
    }

    private static void AppendKeyFiles(StringBuilder sb, string layerName, List<string> keyFiles, int totalCount)
    {
        if (!keyFiles.Any()) return;
        var files = keyFiles.Select(f => $"`{f}`");
        sb.AppendLine($"**Ключевые файлы {layerName}:** {string.Join(", ", files)}");
        if (totalCount > 3)
            sb.AppendLine($"   _(и ещё {totalCount - 3} файлов)_");
    }
}