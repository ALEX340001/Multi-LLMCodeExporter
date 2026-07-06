/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Infrastructure.Utils;

using System.Text;
using System.Text.RegularExpressions;

public static class AnchorGenerator
{
    /// <summary>
    /// Генерирует якорь для заголовка Markdown (GitHub-style)
    /// </summary>
    public static string GenerateAnchor(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Убираем префикс "## 📄 " если есть
        text = Regex.Replace(text, @"^##\s*📄\s*", "");

        // Убираем обратные кавычки
        text = text.Replace("`", "");

        // Приводим к нижнему регистру
        text = text.ToLower();

        // Заменяем пробелы и слеши на дефисы
        text = text.Replace(" ", "-");
        text = text.Replace("\\", "");
        text = text.Replace("/", "");
        text = text.Replace(".", "");

        // Убираем специальные символы (оставляем только буквы, цифры, дефисы)
        text = Regex.Replace(text, @"[^a-z0-9\-_]", "");

        // Убираем множественные дефисы
        text = Regex.Replace(text, @"-+", "-");

        // Убираем дефисы в начале/конце
        text = text.Trim('-');

        return text;
    }

    /// <summary>
    /// Создаёт ссылку на файл в экспорте
    /// </summary>
  public static string CreateFileLink(string relativePath, string? displayText = null)
{
    string anchor = GenerateAnchor($"## 📄 {relativePath}");
    string text = displayText ?? $"`{relativePath}`";
    return $"[{text}](#{anchor})";
}

    /// <summary>
    /// Создаёт якорь для заголовка файла
    /// </summary>
    public static string CreateFileHeaderAnchor(string relativePath)
    {
        return GenerateAnchor($"## 📄 {relativePath}");
    }
}
