/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System.Text;
using LLMCodeExporter.Infrastructure.Utils.Architecture.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture.Formatters;

/// <summary>
/// Интерфейс для форматтеров архитектуры
/// </summary>
public interface ILayerFormatter
{
    bool CanHandle(ArchitectureContext context);
    string Format(ArchitectureContext context);
}