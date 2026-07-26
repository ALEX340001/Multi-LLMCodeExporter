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
/// Интерфейс оптимизатора кода
/// </summary>
public interface ICodeOptimizer
{
    /// <summary>
    /// Оптимизирует код
    /// </summary>
    /// <param name="content">Исходный код</param>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="settings">Настройки</param>
    /// <returns>Оптимизированный код</returns>
    string Optimize(string content, string filePath, IExportSettings settings);
    /// <summary>
    /// Оценивает количество токенов после оптимизации
    /// </summary>
    /// <param name="content">Исходный код</param>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="settings">Настройки</param>
    /// <returns>Количество токенов после оптимизации</returns>
    int EstimateTokensAfterOptimization(string content, string filePath, IExportSettings settings);
}