/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿using LLMCodeExporter.Core.Interfaces;
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Services;

/// <summary>
/// Анализатор проектов Python
/// </summary>
public class PythonProjectAnalyzer : UniversalProjectAnalyzer
{
    // ИЗМЕНЕНИЕ: конструктор принимает IExportSettings, а не ExportSettings
    public PythonProjectAnalyzer(IExportSettings settings) : base(settings)
    {
    }

    // ИЗМЕНЕНИЕ: убираем override, если нет базового метода для переопределения
    // Вместо этого переопределяем ScanProjectFiles, если нужно изменить логику для Python
    protected override List<FileMetadata> ScanProjectFiles(string projectPath, LanguageSettings settings)
    {
        // Специализированная реализация для Python
        var files = new List<FileMetadata>();

        foreach (var extension in settings.FileExtensions)
        {
            foreach (var file in Directory.GetFiles(projectPath, $"*{extension}", SearchOption.AllDirectories))
            {
                // Проверка исключений
                if (ShouldExclude(file, settings.ExcludeFolders))
                    continue;

                var fileInfo = new FileInfo(file);
                var estimatedTokens = EstimateFileTokens(file);

                // Дополнительная логика для Python файлов
                if (file.EndsWith(".py"))
                {
                    // Можно добавить специальную обработку для .py файлов
                }

                files.Add(new FileMetadata
                {
                    FullPath = file,
                    RelativePath = Path.GetRelativePath(projectPath, file),
                    SizeInBytes = fileInfo.Length,
                    EstimatedTokens = estimatedTokens
                });
            }
        }

        return files;
    }

    // Дополнительные специализированные методы для Python
    public List<string> AnalyzePythonDependencies(List<FileMetadata> files)
    {
        var dependencies = new List<string>();

        // Ищем requirements.txt
        var requirementsFile = files.FirstOrDefault(f =>
            f.RelativePath.EndsWith("requirements.txt") ||
            f.FullPath.EndsWith("requirements.txt"));

        if (requirementsFile != null)
        {
            try
            {
                var content = File.ReadAllText(requirementsFile.FullPath);
                var lines = content.Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.Trim().StartsWith("#"))
                    .Select(line => line.Trim())
                    .ToList();

                dependencies.AddRange(lines);
            }
            catch
            {
                dependencies.Add("requirements.txt (ошибка чтения)");
            }
        }

        // Ищем setup.py или pyproject.toml
        var setupFile = files.FirstOrDefault(f =>
            f.RelativePath.EndsWith("setup.py") ||
            f.FullPath.EndsWith("setup.py"));

        if (setupFile != null)
        {
            dependencies.Add("setup.py обнаружен");
        }

        var pyprojectFile = files.FirstOrDefault(f =>
            f.RelativePath.EndsWith("pyproject.toml") ||
            f.FullPath.EndsWith("pyproject.toml"));

        if (pyprojectFile != null)
        {
            dependencies.Add("pyproject.toml обнаружен");
        }

        return dependencies;
    }
}