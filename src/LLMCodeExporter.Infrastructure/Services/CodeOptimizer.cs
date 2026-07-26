/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿namespace LLMCodeExporter.Infrastructure.Services;
using System.Text;
using System.Text.RegularExpressions;
using Core.Models;
using LLMCodeExporter.Infrastructure.Utils;  
/// <summary>
/// Оптимизатор кода для разных режимов экспорта
/// </summary>
public class CodeOptimizer
{
    private readonly ExportSettings _settings;
    public CodeOptimizer(ExportSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Оптимизирует содержимое файла в зависимости от режима
    /// </summary>
    public string OptimizeContent(string content, string filePath)
    {
        // В Full режиме не оптимизируем
        if (_settings.Mode == ExportMode.Full)
        {
            return content;
        }

        var lines = content.Split('\n');
        var result = new StringBuilder();
        // Консолидация using директив
        if (_settings.ConsolidateUsings)
        {
            var (usings, codeWithoutUsings) = ExtractUsings(lines);
            if (usings.Any())
            {
                result.AppendLine("// Consolidated usings");
                foreach (var u in usings.Distinct().OrderBy(x => x))
                {
                    result.AppendLine(u);
                }
                result.AppendLine();
            }

            lines = codeWithoutUsings.Split('\n');
        }

        // Применяем оптимизацию в зависимости от режима
        if (_settings.Mode == ExportMode.Compact)
        {
            result.Append(ExtractSignatures(lines, filePath));
        }
        else // Balanced
        {
            result.Append(CollapseLongMethods(lines));
        }

        return result.ToString();
    }

    /// <summary>
    /// Извлекает using директивы и возвращает код без них
    /// </summary>
    private (List<string> usings, string code) ExtractUsings(string[] lines)
    {
        var usings = new List<string>();
        var codeLines = new List<string>();
        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("using ") && trimmed.EndsWith(";"))
            {
                usings.Add(line.Trim());
            }
            else
            {
                codeLines.Add(line);
            }
        }

        return (usings, string.Join("\n", codeLines));
    }

   /// <summary>
/// Извлекает только сигнатуры классов, методов, свойств (Compact режим)
/// </summary>
private string ExtractSignatures(string[] lines, string filePath)
{
    var result = new StringBuilder();
    var inClass = false;
    var className = "";
    var braceStack = new Stack<char>(); // стек для отслеживания вложенности {}
    var signatures = new List<string>();
    foreach (var line in lines)
    {
        var trimmed = line.Trim();
        // Определяем начало класса/интерфейса/структуры
        if (!inClass && Regex.IsMatch(trimmed, @"^\s*(public|internal|private)?\s*(abstract|sealed|static)?\s*(class|interface|struct|enum)\s+\w+"))
        {
            var match = Regex.Match(trimmed, @"(class|interface|struct|enum)\s+(\w+)");
            className = match.Groups[2].Value;
            result.AppendLine(line);
            result.AppendLine("{");
            inClass = true;
            braceStack.Clear();
            signatures.Clear();
            // Добавляем первую открывающую скобку в стек
            if (trimmed.Contains("{"))
                braceStack.Push('{');
            continue;
        }

        if (inClass)
        {
            // Обрабатываем скобки в строке
            foreach (char ch in line)
            {
                if (ch == '{') braceStack.Push('{');
                else if (ch == '}') 
                {
                    if (braceStack.Count > 0)
                        braceStack.Pop();
                }
            }

            // Проверяем, не закончился ли класс (стек пуст, и текущая строка содержит только "}")
            if (braceStack.Count == 0 && trimmed == "}")
            {
                // Выводим сгруппированные методы
                if (signatures.Count > 5)
                {
                    var groups = MethodGrouper.GroupMethods(signatures.ToArray());
                    result.Append(MethodGrouper.FormatGroupedMethods(groups));
                }
                else
                {
                    foreach (var sig in signatures)
                        result.AppendLine($"    {sig}");
                }
                result.AppendLine("}");
                result.AppendLine();
                inClass = false;
                signatures.Clear();
                continue;
            }

            // Если внутри класса и стек не пуст (мы внутри класса)
            // Сохраняем сигнатуры методов и свойств
            if (Regex.IsMatch(trimmed, @"^\s*(public|private|protected|internal)\s+(static\s+)?(async\s+)?[\w<>\[\],\s]+\s+\w+\s*\("))
            {
                string signature = trimmed.Split('{')[0].Trim();
                if (!signature.EndsWith(";"))
                    signature += ";";
                signatures.Add(signature);
            }
            else if (Regex.IsMatch(trimmed, @"^\s*(public|private|protected|internal)\s+[\w<>\[\]]+\s+\w+\s*{\s*(get|set)"))
            {
                signatures.Add(line.Trim());
            }
            else if (!trimmed.StartsWith("//") && !string.IsNullOrWhiteSpace(trimmed))
            {
                // Поля класса
                if (Regex.IsMatch(trimmed, @"^\s*(public|private|protected|internal)\s+[\w<>\[\]]+\s+\w+\s*;"))
                {
                    result.AppendLine(line);
                }
            }
        }
        else
        {
            // Вне класса
            result.AppendLine(line);
        }
    }

    return result.ToString();
}
    /// <summary>
    /// Сворачивает длинные методы (Balanced режим)
    /// </summary>
    private string CollapseLongMethods(string[] lines)
    {
        var result = new StringBuilder();
        var methodStartLine = -1;
        var methodLines = new List<string>();
        var braceCount = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();
            // Определяем начало метода
            if (methodStartLine == -1 &&
                Regex.IsMatch(trimmed, @"^\s*(public|private|protected|internal)\s+(static\s+)?(async\s+)?[\w<>\[\],\s]+\s+\w+\s*\("))
            {
                methodStartLine = i;
                methodLines.Clear();
                braceCount = 0;
                methodLines.Add(line);
                continue;
            }

            if (methodStartLine >= 0)
            {
                methodLines.Add(line);
                if (trimmed.Contains("{")) braceCount++;
                if (trimmed.Contains("}")) braceCount--;
                // Конец метода
                if (braceCount == 0 && trimmed == "}")
                {
                    // Проверяем длину метода
                    if (methodLines.Count > _settings.MethodCollapseThreshold)
                    {
                        // Сворачиваем длинный метод
                        result.AppendLine(methodLines[0]); // Сигнатура
                        if (!methodLines[0].Trim().EndsWith("{"))
                        {
                            result.AppendLine("    {");
                        }

                        result.AppendLine($"        // ... method body collapsed ({methodLines.Count} lines)");
                        // Показываем первые 3 строки для контекста
                        for (int j = 1; j < Math.Min(4, methodLines.Count - 1); j++)
                        {
                            result.AppendLine(methodLines[j]);
                        }

                        result.AppendLine("        // ...");
                        result.AppendLine("    }");
                    }
                    else
                    {
                        // Метод короткий - оставляем как есть
                        foreach (var ml in methodLines)
                        {
                            result.AppendLine(ml);
                        }
                    }

                    result.AppendLine();
                    methodStartLine = -1;
                    methodLines.Clear();
                }
            }
            else
            {
                // Не внутри метода
                result.AppendLine(line);
            }
        }

        return result.ToString();
    }

    /// <summary>
    /// Проверяет является ли файл авто-сгенерированным
    /// </summary>
    public bool IsAutoGenerated(string filePath)
    {
        return filePath.Contains(".Designer.") ||
               filePath.Contains(".g.cs") ||
               filePath.Contains(".g.i.cs") ||
               filePath.EndsWith(".AssemblyInfo.cs");
    }

    /// <summary>
    /// Получает тег для файла (для метаинформации в экспорте)
    /// </summary>
    public string GetFileTag(string filePath)
    {
        if (IsAutoGenerated(filePath))
        {
            return "[auto-generated]";
        }

      var normalized = filePath.Replace('\\', '/');
if (normalized.Contains("/Test") || normalized.Contains("/Tests"))
    return "[test]";
        return "";
    }
}