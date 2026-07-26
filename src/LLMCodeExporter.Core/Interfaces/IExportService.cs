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
/// Интерфейс службы экспорта
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Экспортирует проект в заданный формат
    /// </summary>
    /// <param name="projectInfo">Информация о проекте</param>
    /// <param name="settings">Настройки экспорта</param>
    /// <returns>Результат экспорта</returns>
    ExportResult ExportProject(ProjectInfo projectInfo, ExportSettings settings);
}