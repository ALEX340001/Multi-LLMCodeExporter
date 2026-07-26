/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
namespace LLMCodeExporter.Infrastructure.Services;
using Core.Interfaces;
using Core.Models;
using LLMCodeExporter.Infrastructure.Utils;
public class FileScanner : IFileScanner
{
    public ProjectInfo ScanProject(string projectPath, ExportSettings settings)
    {
        Logger.Log($"Начало сканирования: {projectPath}");
        if (!Directory.Exists(projectPath))
        {
            Logger.LogError($"Путь не найден: {projectPath}");
            throw new DirectoryNotFoundException($"Путь не найден: {projectPath}");
        }

        var projectInfo = new ProjectInfo
        {
            ProjectPath = projectPath,
            ProjectName = Path.GetFileName(projectPath)
        };
        Logger.Log($"Поиск файлов с расширениями: {string.Join(", ", settings.FileExtensions)}");
        var allFiles = settings.FileExtensions
            .SelectMany(ext => Directory.GetFiles(projectPath, ext, SearchOption.AllDirectories))
            .Distinct()
            .ToArray();
        Logger.Log($"Найдено файлов (до фильтрации): {allFiles.Length}");
        var filesAfterBuildFilter = settings.FilterBuildFolders
            ? allFiles.Where(f => !IsExcludedBuildPath(f, projectPath, settings.ExcludeFolders)).ToArray()
            : allFiles;
        if (settings.FilterBuildFolders && allFiles.Length != filesAfterBuildFilter.Length)
        {
            int excluded = allFiles.Length - filesAfterBuildFilter.Length;
            Logger.LogWarning($"Исключено файлов из папок {string.Join(", ", settings.ExcludeFolders)}: {excluded}");
        }

        // Исправленная деконструкция — явное указание типа
        var filterResult = PatternMatcher.FilterByPatterns(
            filesAfterBuildFilter,
            f => Path.GetRelativePath(projectPath, f),
            settings.IncludeOnlyPatterns,
            settings.ExcludePatterns
        );
        var includedFiles = filterResult.included;
        var excludedFiles = filterResult.excluded;
        if (settings.ExcludePatterns.Any())
        {
            Logger.Log($"Применены паттерны исключения: {string.Join(", ", settings.ExcludePatterns)}");
            Logger.Log($"Исключено по паттернам: {excludedFiles.Count} файлов");
        }

        if (settings.IncludeOnlyPatterns.Any())
        {
            Logger.Log($"Применены паттерны включения: {string.Join(", ", settings.IncludeOnlyPatterns)}");
            Logger.Log($"Включено по паттернам: {includedFiles.Count} файлов");
        }

        foreach (var filePath in includedFiles)
        {
            var fileInfo = new FileInfo(filePath);
            projectInfo.Files.Add(new FileMetadata
            {
                FullPath = filePath,
                RelativePath = Path.GetRelativePath(projectPath, filePath),
                SizeInBytes = fileInfo.Length
            });
        }

        foreach (var filePath in excludedFiles)
        {
            var fileInfo = new FileInfo(filePath);
            projectInfo.ExcludedFiles.Add(new FileMetadata
            {
                FullPath = filePath,
                RelativePath = Path.GetRelativePath(projectPath, filePath),
                SizeInBytes = fileInfo.Length
            });
        }

        projectInfo.TotalCharacters = projectInfo.Files.Sum(f => f.SizeInBytes);
        if (settings.ExcludePatterns.Any())
            projectInfo.Metadata.AppliedFilters.Add($"Исключено: {string.Join(", ", settings.ExcludePatterns)}");
        if (settings.IncludeOnlyPatterns.Any())
            projectInfo.Metadata.AppliedFilters.Add($"Только: {string.Join(", ", settings.IncludeOnlyPatterns)}");
        Logger.LogSuccess($"Сканирование завершено: {projectInfo.TotalFiles} файлов, ~{projectInfo.EstimatedTokens:N0} токенов");
        if (projectInfo.ExcludedFiles.Any())
            Logger.Log($"Исключено всего: {projectInfo.ExcludedFiles.Count} файлов");
        projectInfo.Metadata.TotalFiles = projectInfo.TotalScannedFiles;
        projectInfo.Metadata.IncludedFiles = projectInfo.TotalFiles;
        projectInfo.Metadata.ProjectName = projectInfo.ProjectName;
        return projectInfo;
    }

    private bool IsExcludedBuildPath(string filePath, string basePath, string[] excludeFolders)
    {
        var relativePath = Path.GetRelativePath(basePath, filePath);
        var normalizedPath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        var pathParts = normalizedPath.Split(Path.DirectorySeparatorChar);
        return pathParts.Any(part => excludeFolders.Contains(part, StringComparer.OrdinalIgnoreCase));
    }
}