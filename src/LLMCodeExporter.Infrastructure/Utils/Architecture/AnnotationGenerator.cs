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

public static class AnnotationGenerator
{
    /// <summary>
    /// Генерирует аннотации для всех файлов и собирает ключевые компоненты.
    /// </summary>
    public static List<KeyComponent> GenerateAnnotations(ProjectInfo projectInfo, ArchitectureInfo architecture, List<string> entryPointNames)
    {
        var keyComponents = new List<KeyComponent>();
        var fileToLayer = BuildFileToLayerMap(architecture);

        foreach (var file in projectInfo.Files)
        {
            // Определяем слой
            string layer = fileToLayer.TryGetValue(file.RelativePath, out var l) ? l : "Other";

            // Извлекаем комментарий (если есть)
            string annotation = ExtractComment(file.FullPath);

            // Если комментария нет – генерируем на основе имени и слоя
            if (string.IsNullOrWhiteSpace(annotation))
                annotation = GenerateFromNameAndLayer(Path.GetFileNameWithoutExtension(file.RelativePath), layer);

            // Сохраняем в FileMetadata
            file.Annotation = annotation;

            // Если файл является точкой входа или важным компонентом – добавляем в список ключевых
            if (IsKeyComponent(file, layer, entryPointNames))
            {
                keyComponents.Add(new KeyComponent
                {
                    FilePath = file.RelativePath,
                    Name = Path.GetFileNameWithoutExtension(file.RelativePath),
                    Layer = layer,
                    Annotation = annotation,
                    IsEntryPoint = entryPointNames.Contains(Path.GetFileName(file.RelativePath), StringComparer.OrdinalIgnoreCase),
                    FileType = GetFileType(file.RelativePath)
                });
            }
        }

        return keyComponents;
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

    private static string ExtractComment(string filePath)
    {
        try
        {
            var content = File.ReadAllText(filePath);
            // C# XML-комментарий /// <summary>...</summary>
            var summaryMatch = Regex.Match(content, @"///\s*<summary>\s*(.*?)\s*</summary>", RegexOptions.Singleline);
            if (summaryMatch.Success)
                return summaryMatch.Groups[1].Value.Trim();

            // Python docstring """..."""
            var docMatch = Regex.Match(content, @"""""([^""]+)""""", RegexOptions.Singleline);
            if (docMatch.Success)
                return docMatch.Groups[1].Value.Trim();

            // Альтернативный docstring с тройными кавычками
            var docMatch2 = Regex.Match(content, @"'''(.*?)'''", RegexOptions.Singleline);
            if (docMatch2.Success)
                return docMatch2.Groups[1].Value.Trim();

            return string.Empty;
        }
        catch (Exception ex)
            {
                Logger.LogError($"Ошибка извлечения комментария из файла {filePath}", ex);
                return string.Empty;
            }
    }

    private static string GenerateFromNameAndLayer(string className, string layer)
    {
        if (string.IsNullOrEmpty(className)) return string.Empty;

        // Убираем суффиксы
        string baseName = className;
        var suffixes = new[] { "Service", "Repository", "Controller", "Handler", "Manager", "Helper", "Factory", "Builder", "Provider", "Mapper" };
        foreach (var suffix in suffixes)
        {
            if (baseName.EndsWith(suffix))
            {
                baseName = baseName.Substring(0, baseName.Length - suffix.Length);
                break;
            }
        }

        // Генерируем описание на основе слоя
        string role = layer switch
        {
            "Domain" => "бизнес-сущность",
            "Application" => "сервис/обработчик",
            "Infrastructure" => "доступ к данным/внешние сервисы",
            "UI" => "пользовательский интерфейс",
            "Controllers" => "контроллер API",
            "Tests" => "тест",
            "Backend" => "бэкенд-компонент",
            "Frontend" => "фронтенд-компонент",
            _ => "компонент"
        };

        // Пытаемся сделать более осмысленно
        if (className.EndsWith("Service"))
            return $"Сервис для управления {baseName.ToLowerInvariant()}";
        if (className.EndsWith("Repository"))
            return $"Репозиторий для работы с {baseName.ToLowerInvariant()}";
        if (className.EndsWith("Controller"))
            return $"Контроллер для обработки запросов, связанных с {baseName.ToLowerInvariant()}";
        if (className.EndsWith("Handler"))
            return $"Обработчик команд/запросов для {baseName.ToLowerInvariant()}";
        if (className.EndsWith("Factory"))
            return $"Фабрика для создания {baseName.ToLowerInvariant()}";
        if (className.EndsWith("Manager"))
            return $"Менеджер для управления {baseName.ToLowerInvariant()}";

        return $"{role} {className}";
    }

    private static bool IsKeyComponent(FileMetadata file, string layer, List<string> entryPointNames)
    {
        // Точки входа
        if (entryPointNames.Contains(Path.GetFileName(file.RelativePath), StringComparer.OrdinalIgnoreCase))
            return true;

        // Файлы с большим размером или важные слои
        if (file.SizeInBytes > 20 * 1024) // >20KB
            return true;

        // Классы с характерными суффиксами
        var name = Path.GetFileNameWithoutExtension(file.RelativePath);
        var importantSuffixes = new[] { "Service", "Repository", "Controller", "Handler", "Manager", "Factory" };
        if (importantSuffixes.Any(s => name.EndsWith(s)))
            return true;

        // Если файл находится в корне слоя Application или Domain
        if (layer == "Application" || layer == "Domain")
            return true;

        return false;
    }

    private static string GetFileType(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".cs" => "C# Class",
            ".java" => "Java Class",
            ".py" => "Python Module",
            ".js" => "JavaScript",
            ".ts" => "TypeScript",
            ".jsx" => "React JSX",
            ".tsx" => "React TSX",
            ".html" => "HTML",
            ".css" => "CSS",
            ".json" => "JSON Configuration",
            ".xml" => "XML Configuration",
            _ => "Unknown"
        };
    }
}