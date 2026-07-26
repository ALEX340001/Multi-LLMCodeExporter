/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Infrastructure.Services;
using System.Text;
using System.Text.RegularExpressions;
using Core.Interfaces;
using Core.Models;
using LLMCodeExporter.Infrastructure.Utils;
public class CodeProcessor : ICodeProcessor
{
    public string ProcessCode(string code, ExportSettings settings)
    {
        if (settings.RemoveComments)
            code = RemoveComments(code);
        if (settings.RemoveEmptyLines)
            code = RemoveEmptyLines(code);
        return code.Trim();
    }

    public static string RemoveComments(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;
        var result = new StringBuilder();
        var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        bool inMultiLineComment = false;
        foreach (var line in lines)
        {
            var trimmedLine = line.TrimStart();
            var processedLine = line;
            // Пропускаем XML-документацию (///, но НЕ обычные комментарии //)
            if (trimmedLine.StartsWith("///"))
            {
                continue; // Удаляем XML-док
            }

            // Обработка многострочных комментариев /* */
            if (inMultiLineComment)
            {
                var endIndex = processedLine.IndexOf("*/");
                if (endIndex >= 0)
                {
                    processedLine = processedLine.Substring(endIndex + 2);
                    inMultiLineComment = false;
                }
                else
                {
                    continue; // Пропускаем всю строку
                }
            }

            // Начало многострочного комментария
            var startIndex = processedLine.IndexOf("/*");
            if (startIndex >= 0)
            {
                var endIndex = processedLine.IndexOf("*/", startIndex + 2);
                if (endIndex >= 0)
                {
                    // Однострочный /* comment */
                    processedLine = processedLine.Substring(0, startIndex) +
                                   processedLine.Substring(endIndex + 2);
                }
                else
                {
                    // Начало многострочного
                    processedLine = processedLine.Substring(0, startIndex);
                    inMultiLineComment = true;
                }
            }

            // Удаляем однострочные комментарии //
            // НО только если они не внутри строк!
            var commentIndex = processedLine.IndexOf("//");
            if (commentIndex >= 0)
            {
                // Проверяем что // не внутри строки "..."
                var beforeComment = processedLine.Substring(0, commentIndex);
                var quoteCount = beforeComment.Count(c => c == '"' &&
                    (beforeComment.IndexOf(c) == 0 || beforeComment[beforeComment.IndexOf(c) - 1] != '\\'));
                // Если чётное количество кавычек - комментарий снаружи строки
                if (quoteCount % 2 == 0)
                {
                    processedLine = processedLine.Substring(0, commentIndex).TrimEnd();
                }
            }

            // ИСПРАВЛЕНО: Сохраняем строку только если она не стала пустой после удаления комментариев
            if (!string.IsNullOrWhiteSpace(processedLine))
            {
                result.AppendLine(processedLine);
            }
        }

        return result.ToString().TrimEnd();
    }

    public static string RemoveEmptyLines(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;
        var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var nonEmptyLines = new List<string>();
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();
            // ИСПРАВЛЕНО: Сохраняем строки которые не полностью пустые
            // НЕ трогаем синтаксис - сохраняем скобки, точки с запятой, пробелы
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                nonEmptyLines.Add(line); // Сохраняем оригинальную строку с отступами!
            }
            // Сохраняем одну пустую строку между логическими блоками
            else if (i > 0 && i < lines.Length - 1 &&
                     !string.IsNullOrWhiteSpace(lines[i - 1]) &&
                     !string.IsNullOrWhiteSpace(lines[i + 1]))
            {
                // Если предыдущая строка была закрывающей скобкой "}"
                if (lines[i - 1].Trim() == "}")
                {
                    nonEmptyLines.Add(""); // Оставляем одну пустую строку
                }
            }
        }

        return string.Join(Environment.NewLine, nonEmptyLines);
    }

}