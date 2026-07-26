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
/// Информация о слое архитектуры для экспорта
/// </summary>
public class ArchitectureLayerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public List<string> KeyFiles { get; set; } = new();
    public List<string> FilePaths { get; set; } = new();
}

/// <summary>
/// Контейнер для всех архитектурных данных
/// </summary>
public class ArchitectureInfo
{
    public List<ArchitectureLayerInfo> Layers { get; set; } = new();
    public List<string> DetectedPatterns { get; set; } = new();
    public string ArchitectureStyle { get; set; } = string.Empty;
}

/// <summary>
/// Точка интеграции между слоями (Backend ↔ Frontend)
/// </summary>
public class IntegrationPoint
{
    public string Direction { get; set; } = string.Empty; // "Backend → Frontend" или "Frontend → Backend"
    public string SourceFile { get; set; } = string.Empty;
    public string TargetDescription { get; set; } = string.Empty;
    public string CodeSnippet { get; set; } = string.Empty; // опционально
}