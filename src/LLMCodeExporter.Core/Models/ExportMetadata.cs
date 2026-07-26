/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.Collections.Generic;
using System.Linq;
namespace LLMCodeExporter.Core.Models;
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
    public ProjectType ProjectType { get; set; } = ProjectType.AutoDetect;
    public Language BackendLanguage { get; set; } = Language.CSharp;
    public Language FrontendLanguage { get; set; } = Language.JavaScript;
    public Dictionary<string, object> ArchitectureLayers { get; set; } = new();
    public List<string> Dependencies { get; set; } = new();
    public int BackendFilesCount { get; set; }
    public int FrontendFilesCount { get; set; }
    public ArchitectureInfo Architecture { get; set; } = new();
    public List<IntegrationPoint> IntegrationPoints { get; set; } = new();
    // Новые свойства для правил и нарушений
    public List<DependencyViolation> DependencyViolations { get; set; } = new();
    public List<DependencyRule> AppliedRules { get; set; } = new();
    public double CompressionRatio => OriginalEstimatedTokens > 0
        ? (double)EstimatedTokens / OriginalEstimatedTokens
        : 1.0;
    public string RecommendedContextWindow { get; set; } = "128k";
    public string ReadTimeEstimate { get; set; } = "2-5 sec";
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public List<string> AppliedFilters { get; set; } = new();
    public string SemanticDescription { get; set; } = string.Empty;
    public CodeMetrics Metrics { get; set; } = new();
    public List<KeyComponent> KeyComponents { get; set; } = new();
    public string ToMarkdown()
    {
        var lines = new List<string> { "---" };
        if (AppliedFilters.Any())
            lines.Add($"**Фильтры:** {string.Join(", ", AppliedFilters)}");
        lines.Add($"**Режим:** {GetModeDescription()}");
        if (ProjectType == ProjectType.Hybrid)
        {
            lines.Add($"**Бекенд:** {BackendLanguage}");
            lines.Add($"**Фронтенд:** {FrontendLanguage}");
        }
        lines.Add($"**Дата:** {GeneratedAt:dd.MM.yyyy HH:mm}");
        lines.Add($"**Файлов:** {IncludedFiles}");
        if (ExcludedFiles > 0)
            lines.Add($"  _(исключено: {ExcludedFiles})_");
        lines.Add($"**Токенов:** ~{EstimatedTokens:N0}");
        if (Mode != ExportMode.Full && OriginalEstimatedTokens > 0 && EstimatedTokens > 0 && CompressionRatio < 1.0)
        {
            var savings = (1 - CompressionRatio) * 100;
            lines.Add($"  _(оригинал: ~{OriginalEstimatedTokens:N0}, сэкономлено: {savings:F0}%)_");
        }
        lines.Add($"**Рекомендуется:** {RecommendedContextWindow}");
        lines.Add("---");
        return string.Join("\n", lines);
    }

    private string GetModeDescription() => Mode switch
    {
        ExportMode.Compact => "🎯 Compact (структура + сигнатуры)",
        ExportMode.Balanced => "⚖️ Balanced (бизнес-логика без UI)",
        ExportMode.Full => "📚 Full (весь проект с UI)",
        _ => "Unknown"
    };
    public string GetLLMRecommendations()
    {
        if (EstimatedTokens <= 8000)
            return "✅ Подходит для всех LLM (включая ChatGPT бесплатный)";
        if (EstimatedTokens <= 32000)
            return "✅ GPT-4 Turbo, Claude 3, Gemini Pro, DeepSeek";
        if (EstimatedTokens <= 128000)
            return "✅ GPT-4 Turbo (128K), Claude 3.5 Sonnet, Gemini 1.5 Pro";
        if (EstimatedTokens <= 200000)
            return "✅ Claude 3.5 Sonnet (200K), Gemini 1.5 Pro (1M)";
        return "⚠️ Очень большой! Рекомендуется Gemini 1.5 Pro (1M) или разбить на части";
    }

}