/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
namespace LLMCodeExporter.Infrastructure.Utils;

using Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class DependencyAnalyzer
{
    /// <summary>
    /// Генерирует граф зависимостей в формате Mermaid
    /// </summary>
    public static string GenerateDependencyGraph(List<FileMetadata> files)
    {
        if (files == null || !files.Any())
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("## 🔗 Граф зависимостей");
        sb.AppendLine();
        sb.AppendLine("<details>");
        sb.AppendLine("<summary>📊 Показать граф</summary>");
        sb.AppendLine();
        sb.AppendLine("```mermaid");
        sb.AppendLine("graph TD");
        
        // Группировка по папкам
        var groups = files.GroupBy(f => Path.GetDirectoryName(f.RelativePath) ?? "Root")
                          .Select(g => new { Folder = g.Key, Files = g.ToList() })
                          .ToList();

        foreach (var group in groups)
        {
            string nodeId = "Folder_" + group.Folder.Replace("\\", "_").Replace("/", "_");
            sb.AppendLine($"    {nodeId}[\"{group.Folder}\"]");
        }

        foreach (var file in files)
        {
            string fileNode = "File_" + file.RelativePath.Replace("\\", "_").Replace("/", "_").Replace(".", "_");
            string folder = Path.GetDirectoryName(file.RelativePath) ?? "Root";
            string folderNode = "Folder_" + folder.Replace("\\", "_").Replace("/", "_");
            sb.AppendLine($"    {fileNode}[\"{Path.GetFileName(file.RelativePath)}\"]");
            sb.AppendLine($"    {folderNode} --> {fileNode}");
        }

        // Пример связей между файлами (для демонстрации)
        foreach (var group in groups)
        {
            var fileList = group.Files;
            for (int i = 0; i < fileList.Count - 1; i++)
            {
                string from = "File_" + fileList[i].RelativePath.Replace("\\", "_").Replace("/", "_").Replace(".", "_");
                string to = "File_" + fileList[i + 1].RelativePath.Replace("\\", "_").Replace("/", "_").Replace(".", "_");
                sb.AppendLine($"    {from} --> {to}");
            }
        }

        sb.AppendLine("```");
        sb.AppendLine("</details>");
        sb.AppendLine();
        sb.AppendLine("**Ключевые узлы:**");
        var entryPoints = files.Where(f => f.RelativePath.Contains("Program") || f.RelativePath.Contains("Startup"))
                               .Select(f => $"`{f.RelativePath}`")
                               .Take(5);
        if (entryPoints.Any())
        {
            sb.AppendLine($"- {string.Join(", ", entryPoints)}");
        }
        sb.AppendLine();

        sb.AppendLine("---");
        sb.AppendLine();

        return sb.ToString();
    }

    private static List<Dependency> ExtractDependencies(List<FileMetadata> files)
    {
        var dependencies = new List<Dependency>();
        var classNames = files
            .Select(f => Path.GetFileNameWithoutExtension(f.RelativePath))
            .Where(name => !string.IsNullOrEmpty(name))
            .ToHashSet();

        foreach (var file in files)
        {
            try
            {
                string content = File.ReadAllText(file.FullPath);
                string fromClass = Path.GetFileNameWithoutExtension(file.RelativePath);

                // Ищем использование других классов
                foreach (var className in classNames)
                {
                    if (className == fromClass)
                        continue;

                    // Паттерны использования: new ClassName(), ClassName variable, ClassName.Method()
                    var patterns = new[]
                    {
                        $@"\bnew\s+{className}\s*\(",           // new ClassName()
                        $@"\b{className}\s+\w+\s*[=;]",         // ClassName variable
                        $@"\b{className}\.",                     // ClassName.Method()
                        $@":\s*{className}\b",                   // : ClassName (наследование)
                        $@"<{className}>",                       // List<ClassName>
                        $@"\({className}\s+\w+\)"               // (ClassName param)
                    };

                    if (patterns.Any(pattern => Regex.IsMatch(content, pattern)))
                    {
                        dependencies.Add(new Dependency
                        {
                            From = fromClass,
                            To = className
                        });
                    }
                }
            }
          catch (Exception ex)
                {
                    Logger.LogError($"Ошибка извлечения зависимостей из файла {file.FullPath}", ex);
                }
        }

        return dependencies;
    }

    private static Dictionary<string, int> CalculateNodeImportance(List<Dependency> dependencies)
    {
        var importance = new Dictionary<string, int>();

        foreach (var dep in dependencies)
        {
            // Узел важен если он имеет много входящих И исходящих связей
            if (!importance.ContainsKey(dep.From))
                importance[dep.From] = 0;
            if (!importance.ContainsKey(dep.To))
                importance[dep.To] = 0;

            importance[dep.From] += 1;  // Исходящая связь
            importance[dep.To] += 2;    // Входящая связь (важнее)
        }

        return importance;
    }

    private static string GetNodeDescription(string nodeName, int inCount, int outCount)
    {
        if (inCount > 5 && outCount > 5)
            return "центральный координатор";
        if (inCount > 5)
            return "используется многими классами";
        if (outCount > 5)
            return "использует много зависимостей";
        if (nodeName.Contains("Service"))
            return "сервисный слой";
        if (nodeName.Contains("Repository"))
            return "доступ к данным";
        if (nodeName.Contains("Manager"))
            return "управляющий компонент";

        return "компонент системы";
    }

    private class Dependency
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}