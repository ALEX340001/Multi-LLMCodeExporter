/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Infrastructure.Services;

using Core.Interfaces;
using Core.Models;
using Utils;
using System.Text;

public class ExportService : IExportService
{
    private readonly ICodeProcessor _codeProcessor;

    public ExportService(ICodeProcessor codeProcessor)
    {
        _codeProcessor = codeProcessor;
    }

    public ExportResult ExportProject(ProjectInfo projectInfo, ExportSettings settings)
    {
        return ExportProjectAsync(projectInfo, settings).GetAwaiter().GetResult();
    }

    private async Task<ExportResult> ExportProjectAsync(ProjectInfo projectInfo, ExportSettings settings)
    {
        var result = new ExportResult { ProjectInfo = projectInfo };

        Logger.Log($"Начало экспорта проекта '{projectInfo.ProjectName}' в режиме {settings.Mode}");
        Logger.Log($"Файлов для обработки: {projectInfo.TotalFiles}");

        try
        {
            var optimizer = new CodeOptimizer(settings);

            // 🔧 Исправлено: передаём settings в MarkdownFormatter
            IOutputFormatter formatter = settings.Format == ExportFormat.Markdown
                ? new Formatters.MarkdownFormatter(settings)
                : new Formatters.PlainTextFormatter();

            Logger.Log($"Выбран формат: {settings.Format}");

            string extension = settings.Format == ExportFormat.Markdown ? "md" : "txt";
            string modePrefix = settings.Mode != ExportMode.Full ? $"_{settings.Mode.ToString().ToLower()}" : "";
            string fileName = $"code_export_{projectInfo.ProjectName}{modePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";
            string outputPath = Path.Combine(settings.OutputDirectory, fileName);

            Logger.Log($"Целевой файл: {outputPath}");

            long originalSize = projectInfo.TotalCharacters;
            long optimizedSize = 0;

            projectInfo.Metadata.ProjectName = projectInfo.ProjectName;
            projectInfo.Metadata.Mode = settings.Mode;
            projectInfo.Metadata.TotalFiles = projectInfo.TotalScannedFiles;
            projectInfo.Metadata.IncludedFiles = projectInfo.TotalFiles;
            projectInfo.Metadata.TotalSize = originalSize;
            projectInfo.Metadata.OriginalEstimatedTokens = (int)(originalSize / 4);

            if (settings.ExcludePatterns.Any())
            {
                var uniqueExcludes = settings.ExcludePatterns.Distinct().ToList();
                projectInfo.Metadata.AppliedFilters.Add($"Исключено: {string.Join(", ", uniqueExcludes)}");
            }

            if (settings.IncludeOnlyPatterns.Any())
            {
                var uniqueIncludes = settings.IncludeOnlyPatterns.Distinct().ToList();
                projectInfo.Metadata.AppliedFilters.Add($"Только: {string.Join(", ", uniqueIncludes)}");
            }

            projectInfo.Metadata.EstimatedTokens = settings.Mode switch
            {
                ExportMode.Compact => (int)(originalSize * 0.15 / 4),
                ExportMode.Balanced => (int)(originalSize * 0.5 / 4),
                ExportMode.Full => (int)(originalSize * 0.9 / 4),
                _ => (int)(originalSize / 4)
            };
            projectInfo.Metadata.RecommendedContextWindow = GetRecommendedWindow(projectInfo.Metadata.EstimatedTokens);

            using (var writer = new StreamWriter(outputPath, false, new UTF8Encoding(true)))
            {
                await writer.WriteAsync(formatter.FormatHeader(projectInfo));

                int processedFiles = 0;

                foreach (var file in projectInfo.Files)
                {
                    try
                    {
                        string content = await ReadFileContentAsync(file.FullPath);

                        if (settings.Mode != ExportMode.Full)
                        {
                            content = optimizer.OptimizeContent(content, file.RelativePath);
                        }

                        content = _codeProcessor.ProcessCode(content, settings);

                        optimizedSize += content.Length;

                        string fileTag = optimizer.GetFileTag(file.RelativePath);

                        await writer.WriteAsync(formatter.FormatFile(file, content, fileTag));
                        processedFiles++;

                        Logger.Log($"Обработан [{processedFiles}/{projectInfo.TotalFiles}]: {file.RelativePath}");
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Ошибка при чтении {file.RelativePath}: {ex.Message}";
                        result.Errors.Add(errorMsg);
                        Logger.LogError(errorMsg, ex);
                        await writer.WriteLineAsync($"\n⚠ ОШИБКА: {file.RelativePath}\n{ex.Message}\n");
                    }
                }

                await writer.WriteAsync(formatter.FormatFooter(projectInfo));
            }

            projectInfo.Metadata.OptimizedSize = optimizedSize;
            projectInfo.Metadata.EstimatedTokens = (int)(optimizedSize / 4);
            projectInfo.Metadata.RecommendedContextWindow = GetRecommendedWindow(optimizedSize / 4);

            Logger.LogSuccess($"Экспорт завершен! Обработано файлов: {projectInfo.TotalFiles}");
            Logger.Log($"Оригинальный размер: {originalSize:N0} символов (~{originalSize / 4:N0} токенов)");
            Logger.Log($"Оптимизированный размер: {optimizedSize:N0} символов (~{optimizedSize / 4:N0} токенов)");

            if (originalSize > 0)
            {
                double compressionPercent = (1 - (double)optimizedSize / originalSize) * 100;
                Logger.Log($"Степень сжатия: {compressionPercent:F0} %");
            }

            result.Success = true;
            result.OutputFilePath = outputPath;
            result.Message = $"Экспорт завершен успешно: {outputPath}";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Ошибка экспорта: {ex.Message}";
            result.Errors.Add(ex.Message);

            Logger.LogError("Критическая ошибка экспорта", ex);
        }

        return result;
    }

    private async Task<string> ReadFileContentAsync(string filePath)
    {
        try
        {
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка чтения файла {filePath}", ex);
            return $"// Ошибка чтения файла: {ex.Message}";
        }
    }

    private string GetRecommendedWindow(long tokens)
    {
        if (tokens <= 8000) return "8K (все LLM)";
        if (tokens <= 32000) return "32K (GPT-4, Claude)";
        if (tokens <= 128000) return "128K (GPT-4 Turbo, Claude 3.5)";
        if (tokens <= 200000) return "200K (Claude 3.5 Sonnet)";
        return "1M (Gemini 1.5 Pro)";
    }
}
