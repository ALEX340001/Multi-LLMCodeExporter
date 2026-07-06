/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Collections.Generic;

namespace LLMCodeExporter.Core.Models;

/// <summary>
/// Метрики качества кода для проекта или слоя
/// </summary>
public class CodeMetrics
{
    /// <summary>
    /// Общее количество строк кода (SLOC)
    /// </summary>
    public int TotalLinesOfCode { get; set; }

    /// <summary>
    /// Количество классов (или типов верхнего уровня)
    /// </summary>
    public int ClassCount { get; set; }

    /// <summary>
    /// Количество методов (функций)
    /// </summary>
    public int MethodCount { get; set; }

    /// <summary>
    /// Количество файлов, содержащих XML-комментарии / docstring
    /// </summary>
    public int DocumentedFilesCount { get; set; }

    /// <summary>
    /// Средняя длина метода в строках
    /// </summary>
    public double AverageMethodLength { get; set; }

    /// <summary>
    /// Максимальная длина метода в строках
    /// </summary>
    public int MaxMethodLength { get; set; }

    /// <summary>
    /// Приблизительный индекс поддерживаемости (упрощённый)
    /// </summary>
    public double MaintainabilityIndex { get; set; }

    /// <summary>
    /// Метрики по слоям (ключ – имя слоя)
    /// </summary>
    public Dictionary<string, CodeMetrics> ByLayer { get; set; } = new();
}