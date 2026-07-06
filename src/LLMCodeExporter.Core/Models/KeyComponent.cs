/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
namespace LLMCodeExporter.Core.Models;

/// <summary>
/// Ключевой компонент системы (файл или класс) с аннотацией
/// </summary>
public class KeyComponent
{
    public string FilePath { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty; // имя класса/файла
    public string Layer { get; set; } = string.Empty;
    public string Annotation { get; set; } = string.Empty; // описание роли
    public bool IsEntryPoint { get; set; }
    public string FileType { get; set; } = string.Empty; // "Class", "Interface", "Controller", etc.
}