/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Core.Models;

/// <summary>
/// Метаданные экспорта - информация о результате экспорта
/// </summary>
public class ExportMetadata
{
    public string ProjectName { get; set; } = string.Empty;
    public ExportMode Mode { get; set; }
    public int TotalFiles { get; set; }
    public int IncludedFiles { get; set; }
    public int ExcludedFiles => TotalFiles - IncludedFiles;
    public long TotalSize { get; set; }
    public long OptimizedSize { get; set; }
    public int EstimatedTokens { get; set; }
    public int OriginalEstimatedTokens { get; set; }

    // Добавляем новые свойства
    public ProjectType ProjectType { get; set; } = ProjectType.AutoDetect;
    public Dictionary<string, object> ArchitectureLayers { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();

    public double CompressionRatio => OriginalEstimatedTokens > 0
        ? (double)EstimatedTokens / OriginalEstimatedTokens
        : 1.0;
    public string RecommendedContextWindow { get; set; } = "128k";
    public string ReadTimeEstimate { get; set; } = "2-5 sec";
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public List<string> AppliedFilters { get; set; } = new();

    /// <summary>
    /// Форматирует метаданные в Markdown для заголовка экспорта
    /// </summary>
    public string ToMarkdown()
    {
        var lines = new List<string>
        {
            "---"
        };

        if (AppliedFilters.Any())
        {
            lines.Add($"**Фильтры:** {string.Join(", ", AppliedFilters)}");
        }

        lines.Add($"**Режим:** {GetModeDescription()}");
        lines.Add($"**Дата:** {GeneratedAt:dd.MM.yyyy HH:mm}");
        lines.Add($"**Файлов:** {IncludedFiles}");

        if (ExcludedFiles > 0)
        {
            lines.Add($"  _(исключено: {ExcludedFiles})_");
        }

        lines.Add($"**Токенов:** ~{EstimatedTokens:N0}");

        // Показываем сжатие только если есть реальные данные
        if (Mode != ExportMode.Full &&
            OriginalEstimatedTokens > 0 &&
            EstimatedTokens > 0 &&  // ← ДОБАВИЛИ ПРОВЕРКУ!
            CompressionRatio < 1.0)
        {
            var savings = (1 - CompressionRatio) * 100;
            lines.Add($"  _(оригинал: ~{OriginalEstimatedTokens:N0}, сэкономлено: {savings:F0}%)_");
        }

        lines.Add($"**Рекомендуется:** {RecommendedContextWindow}");
        lines.Add("---");

        return string.Join("\n", lines);
    }

    private string GetModeDescription()
    {
        return Mode switch
        {
            ExportMode.Compact => "🎯 Compact (структура + сигнатуры)",
            ExportMode.Balanced => "⚖️ Balanced (бизнес-логика без UI)",
            ExportMode.Full => "📚 Full (весь проект с UI)",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Получает рекомендации по LLM на основе размера
    /// </summary>
    public string GetLLMRecommendations()
    {
        if (EstimatedTokens <= 8000)
        {
            return "✅ Подходит для всех LLM (включая ChatGPT бесплатный)";
        }
        else if (EstimatedTokens <= 32000)
        {
            return "✅ GPT-4 Turbo, Claude 3, Gemini Pro, DeepSeek";
        }
        else if (EstimatedTokens <= 128000)
        {
            return "✅ GPT-4 Turbo (128K), Claude 3.5 Sonnet, Gemini 1.5 Pro";
        }
        else if (EstimatedTokens <= 200000)
        {
            return "✅ Claude 3.5 Sonnet (200K), Gemini 1.5 Pro (1M)";
        }
        else
        {
            return "⚠️ Очень большой! Рекомендуется Gemini 1.5 Pro (1M) или разбить на части";
        }
    }
}