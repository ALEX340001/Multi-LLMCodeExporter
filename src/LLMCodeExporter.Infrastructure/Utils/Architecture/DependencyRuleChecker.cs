/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture;

public static class DependencyRuleChecker
{
    /// <summary>
    /// Проверяет все файлы на соответствие архитектурным правилам.
    /// </summary>
    public static List<DependencyViolation> CheckRules(
        List<FileMetadata> files,
        ArchitectureInfo architecture,
        List<DependencyRule> rules)
    {
        var violations = new List<DependencyViolation>();

        // Строим карту: относительный путь файла -> слой
        var fileToLayer = new Dictionary<string, string>();
        foreach (var layer in architecture.Layers)
        {
            foreach (var file in layer.FilePaths)
            {
                fileToLayer[file] = layer.Name;
            }
        }

        // Для каждого файла анализируем импорты
        foreach (var file in files)
        {
            if (!fileToLayer.TryGetValue(file.RelativePath, out var sourceLayer))
                continue;

            var imports = ExtractImports(file.FullPath);
            foreach (var import in imports)
            {
                var targetFile = ResolveImportToFile(import, file.RelativePath, files);
                if (targetFile == null) continue;

                if (!fileToLayer.TryGetValue(targetFile.RelativePath, out var targetLayer))
                    continue;

                // Проверяем правила
                foreach (var rule in rules)
                {
                    if (!rule.IsEnabled) continue;
                    if (sourceLayer == rule.SourceLayer && targetLayer == rule.ForbiddenTargetLayer)
                    {
                        violations.Add(new DependencyViolation
                        {
                            SourceFile = file.RelativePath,
                            SourceLayer = sourceLayer,
                            TargetFile = targetFile.RelativePath,
                            TargetLayer = targetLayer,
                            RuleDescription = rule.Description,
                            Severity = "Warning"
                        });
                    }
                }
            }
        }

        return violations;
    }

    /// <summary>
    /// Извлекает все импорты/using из файла.
    /// Поддерживаются: using (C#), import (JS/TS), require (JS).
    /// </summary>
    private static List<string> ExtractImports(string filePath)
    {
        var imports = new List<string>();
        try
        {
            var content = File.ReadAllText(filePath);
            var lines = content.Split('\n');

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // C# using
                if (trimmed.StartsWith("using "))
                {
                    var match = Regex.Match(trimmed, @"using\s+([\w\.]+);");
                    if (match.Success)
                        imports.Add(match.Groups[1].Value);
                }
                // JS/TS import
                else if (trimmed.StartsWith("import ") || trimmed.StartsWith("import type "))
                {
                    // Ищем строку в кавычках: import ... from '...'
                    var match = Regex.Match(trimmed, @"from\s+['""]([^'""]+)['""]");
                    if (match.Success)
                        imports.Add(match.Groups[1].Value);
                    else
                    {
                        // import 'module'
                        var match2 = Regex.Match(trimmed, @"import\s+['""]([^'""]+)['""]");
                        if (match2.Success)
                            imports.Add(match2.Groups[1].Value);
                    }
                }
                // JS require
                else if (trimmed.Contains("require("))
                {
                    var match = Regex.Match(trimmed, @"require\s*\(\s*['""]([^'""]+)['""]\s*\)");
                    if (match.Success)
                        imports.Add(match.Groups[1].Value);
                }
            }
        }
        catch (Exception ex)
                {
                    Logger.LogError($"Ошибка чтения файла {filePath} для извлечения импортов", ex);
                }
        return imports;
    }

    /// <summary>
    /// Пытается сопоставить импорт с файлом в проекте.
    /// </summary>
    private static FileMetadata? ResolveImportToFile(string import, string currentFile, List<FileMetadata> files)
    {
        // Если импорт выглядит как путь (содержит / или \), пробуем разрешить относительно текущего файла
        if (import.Contains('/') || import.Contains('\\'))
        {
            var currentDir = Path.GetDirectoryName(currentFile) ?? "";
            var importPath = import.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);

            // Убираем расширение, если оно есть, так как файлы могут быть без расширения в импорте
            var importWithoutExt = importPath;
            if (importPath.Contains('.'))
                importWithoutExt = Path.ChangeExtension(importPath, null);

            // Пробуем найти файл по относительному пути
            var relative = Path.GetFullPath(Path.Combine(currentDir, importWithoutExt));
            var candidates = files.Where(f => f.FullPath.EndsWith(importWithoutExt, StringComparison.OrdinalIgnoreCase) ||
                                               f.FullPath.Replace('\\', '/').Contains(importWithoutExt.Replace('\\', '/')));

            // Отфильтруем по точному совпадению с относительным путём
            var best = candidates.FirstOrDefault(f =>
            {
                var normalized = f.RelativePath.Replace('\\', '/');
                var importNorm = import.Replace('\\', '/');
                return normalized.Contains(importNorm) ||
                       importNorm.Contains(normalized) ||
                       f.FullPath.Replace('\\', '/').Contains(importNorm);
            });

            if (best != null)
                return best;

            // Если не нашли, пробуем по имени файла
            var fileName = Path.GetFileName(importWithoutExt);
            var byName = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.RelativePath).Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (byName != null)
                return byName;
        }
        else
        {
            // Простое имя модуля – ищем файл с таким именем
            var fileName = import;
            if (fileName.Contains('.'))
                fileName = Path.GetFileNameWithoutExtension(fileName);
            var byName = files.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f.RelativePath).Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (byName != null)
                return byName;
        }

        return null;
    }
}