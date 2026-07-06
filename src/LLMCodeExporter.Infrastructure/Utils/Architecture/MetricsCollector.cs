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

public static class MetricsCollector
{
    /// <summary>
    /// Собирает метрики качества для проекта и по слоям.
    /// </summary>
    public static CodeMetrics Collect(ProjectInfo projectInfo, ArchitectureInfo architecture)
    {
        var globalMetrics = new CodeMetrics();
        var fileToLayer = BuildFileToLayerMap(architecture);

        // Группируем файлы по слоям
        var filesByLayer = projectInfo.Files
            .GroupBy(f => fileToLayer.TryGetValue(f.RelativePath, out var layer) ? layer : "Other")
            .ToDictionary(g => g.Key, g => g.ToList());

        var layerMetrics = new Dictionary<string, CodeMetrics>();

        foreach (var kvp in filesByLayer)
        {
            var layer = kvp.Key;
            var files = kvp.Value;
            var metrics = CalculateMetrics(files);
            layerMetrics[layer] = metrics;
            // Добавляем к глобальным
            globalMetrics.TotalLinesOfCode += metrics.TotalLinesOfCode;
            globalMetrics.ClassCount += metrics.ClassCount;
            globalMetrics.MethodCount += metrics.MethodCount;
            globalMetrics.DocumentedFilesCount += metrics.DocumentedFilesCount;
        }

        // Пересчитываем средние для глобальных
        globalMetrics.AverageMethodLength = globalMetrics.MethodCount > 0
            ? (double)globalMetrics.TotalLinesOfCode / globalMetrics.MethodCount
            : 0;
        globalMetrics.MaxMethodLength = filesByLayer.Values.SelectMany(f => f)
            .Select(f => CalculateMethodLengths(f.FullPath).MaxOrDefault(0))
            .DefaultIfEmpty(0)
            .Max();

        // Приблизительный индекс поддерживаемости (упрощённо)
        globalMetrics.MaintainabilityIndex = CalculateMaintainabilityIndex(globalMetrics);

        globalMetrics.ByLayer = layerMetrics;

        return globalMetrics;
    }

    private static Dictionary<string, string> BuildFileToLayerMap(ArchitectureInfo architecture)
    {
        var map = new Dictionary<string, string>();
        foreach (var layer in architecture.Layers)
        {
            foreach (var path in layer.FilePaths)
            {
                map[path] = layer.Name;
            }
        }
        return map;
    }

    private static CodeMetrics CalculateMetrics(List<FileMetadata> files)
    {
        var metrics = new CodeMetrics();
        int totalLines = 0;
        int classCount = 0;
        int methodCount = 0;
        int documentedFiles = 0;
        var methodLengths = new List<int>();

        foreach (var file in files)
        {
            try
            {
                var content = File.ReadAllText(file.FullPath);
                var lines = content.Split('\n');
                totalLines += lines.Length;

                // Классы (C#: class, interface, struct, enum; Python: class)
                var classMatches = Regex.Matches(content, @"\b(class|interface|struct|enum)\s+\w+", RegexOptions.IgnoreCase);
                classCount += classMatches.Count;

                // Методы (C#: public/private/protected/internal ... methodName(...); Python: def methodName(...))
                var methodMatches = Regex.Matches(content, @"\b(def\s+\w+\s*\(|(public|private|protected|internal)\s+\w+\s+\w+\s*\()", RegexOptions.IgnoreCase);
                methodCount += methodMatches.Count;

                // Документированные файлы (содержат /// или """)
                if (content.Contains("///") || content.Contains("\"\"\""))
                    documentedFiles++;

                // Длины методов (упрощённо: считаем строки между фигурными скобками)
                var methodLengthsForFile = CalculateMethodLengths(file.FullPath);
                methodLengths.AddRange(methodLengthsForFile);
            }
           catch (Exception ex) 
                    { 
                        Logger.LogError($"Ошибка чтения файла {file.FullPath}", ex); 
                    }
        }

        metrics.TotalLinesOfCode = totalLines;
        metrics.ClassCount = classCount;
        metrics.MethodCount = methodCount;
        metrics.DocumentedFilesCount = documentedFiles;
        metrics.AverageMethodLength = methodLengths.Any() ? methodLengths.Average() : 0;
        metrics.MaxMethodLength = methodLengths.Any() ? methodLengths.Max() : 0;
        metrics.MaintainabilityIndex = CalculateMaintainabilityIndex(metrics);

        return metrics;
    }

    private static List<int> CalculateMethodLengths(string filePath)
    {
        var lengths = new List<int>();
        try
        {
            var content = File.ReadAllText(filePath);
            var lines = content.Split('\n');
            // Ищем строки с методами (упрощённо)
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (Regex.IsMatch(line, @"\b(def\s+\w+\s*\(|(public|private|protected|internal)\s+\w+\s+\w+\s*\()", RegexOptions.IgnoreCase))
                {
                    int start = i;
                    int braceCount = 0;
                    bool foundOpen = false;
                    for (int j = i; j < lines.Length; j++)
                    {
                        var current = lines[j];
                        if (current.Contains("{"))
                        {
                            braceCount++;
                            foundOpen = true;
                        }
                        if (current.Contains("}"))
                            braceCount--;
                        if (foundOpen && braceCount == 0)
                        {
                            int length = j - start + 1;
                            lengths.Add(length);
                            i = j; // продолжим после конца метода
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
            {
                Logger.LogError($"Ошибка при анализе длин методов в файле {filePath}", ex);
            }
        return lengths;
    }

    private static double CalculateMaintainabilityIndex(CodeMetrics metrics)
    {
        // Упрощённая формула: MI = 171 - 5.2 * ln(avg_lines_per_method) - 0.23 * (class_count / method_count) - 16.2 * ln(loc)
        double avgMethodLength = metrics.AverageMethodLength > 0 ? metrics.AverageMethodLength : 1;
        double loc = metrics.TotalLinesOfCode > 0 ? metrics.TotalLinesOfCode : 1;
        double methodDensity = metrics.MethodCount > 0 ? (double)metrics.ClassCount / metrics.MethodCount : 0;

        double mi = 171 - 5.2 * Math.Log(avgMethodLength) - 0.23 * methodDensity - 16.2 * Math.Log(loc);
        return Math.Max(0, mi);
    }

    private static int MaxOrDefault(this IEnumerable<int> source, int defaultValue)
    {
        if (source == null || !source.Any())
            return defaultValue;
        return source.Max();
    }
}