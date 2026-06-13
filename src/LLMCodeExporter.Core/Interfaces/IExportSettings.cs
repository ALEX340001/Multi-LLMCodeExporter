/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Core.Interfaces;

/// <summary>
/// Интерфейс настроек экспорта
/// </summary>
public interface IExportSettings
{
    /// <summary>
    /// Режим экспорта
    /// </summary>
    ExportMode Mode { get; set; }

    /// <summary>
    /// Тип проекта
    /// </summary>
    ProjectType ProjectType { get; set; }

    /// <summary>
    /// Включить комментарии
    /// </summary>
    bool IncludeComments { get; set; }

    /// <summary>
    /// Включить минификацию
    /// </summary>
    bool Minify { get; set; }

    /// <summary>
    /// Исключаемые папки
    /// </summary>
    string[] ExcludeFolders { get; set; }

    /// <summary>
    /// Исключаемые файлы
    /// </summary>
    string[] ExcludeFiles { get; set; }

    /// <summary>
    /// Максимальный размер файла (в байтах)
    /// </summary>
    long MaxFileSize { get; set; }

    /// <summary>
    /// Максимальное количество токенов
    /// </summary>
    int MaxTokens { get; set; }
}