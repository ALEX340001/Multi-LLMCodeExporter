/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿using LLMCodeExporter.Core.Interfaces;
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Services;

/// <summary>
/// Универсальный оптимизатор кода
/// </summary>
public class UniversalCodeOptimizer : ICodeOptimizer
{
    public string Optimize(string content, string filePath, IExportSettings settings)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        var extension = Path.GetExtension(filePath).ToLower();

        // Базовая оптимизация - удаление лишних пробелов и пустых строк
        var lines = content.Split('\n')
            .Select(line => line.TrimEnd())
            .Where(line => !string.IsNullOrWhiteSpace(line) || settings.IncludeComments)
            .ToArray();

        // Дополнительная обработка в зависимости от типа файла
        if (settings.Minify)
        {
            switch (extension)
            {
                case ".cs":
                case ".js":
                case ".ts":
                    // Минификация для C#/JavaScript/TypeScript
                    return MinifyCode(lines);
                default:
                    return string.Join("\n", lines);
            }
        }

        return string.Join("\n", lines);
    }

    public int EstimateTokensAfterOptimization(string content, string filePath, IExportSettings settings)
    {
        var optimized = Optimize(content, filePath, settings);
        // Приблизительная оценка: 1 токен = 4 символа
        return (int)Math.Ceiling(optimized.Length / 4.0);
    }

    private string MinifyCode(string[] lines)
    {
        // Простая минификация - удаление комментариев и лишних пробелов
        var result = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Пропускаем однострочные комментарии (кроме важных директив)
            if (trimmedLine.StartsWith("//") &&
                !trimmedLine.StartsWith("///") &&
                !trimmedLine.StartsWith("// <"))
            {
                continue;
            }

            // Пропускаем пустые строки
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            result.Add(trimmedLine);
        }

        return string.Join(" ", result);
    }
}