/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.IO;
using LLMCodeExporter.Infrastructure.Utils;
namespace LLMCodeExporter.CLI.UI;
/// <summary>
/// Нормализация путей к проектам
/// </summary>
public static class PathNormalizer
{
    /// <summary>
    /// Нормализует путь к проекту
    /// </summary>
    public static string? Normalize(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return path;
        path = path.Trim().Trim('"');
        Logger.Log($"Нормализация пути: {path}");
        // Если это файл .csproj - берём его директорию
        if (File.Exists(path) && path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                Logger.Log($"Путь к файлу .csproj нормализован: {path} -> {directory}");
                return directory;
            }
        }

        // Если это любой другой файл - берём его директорию
        if (File.Exists(path))
        {
            string? directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                Logger.Log($"Путь к файлу нормализован: {path} -> {directory}");
                return directory;
            }
        }

        // Если путь не существует, пробуем преобразовать в абсолютный
        if (!Directory.Exists(path) && !File.Exists(path))
        {
            string fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath))
            {
                Logger.Log($"Относительный путь нормализован: {path} -> {fullPath}");
                return fullPath;
            }

            // Пробуем взять родительскую директорию
            string? parentDir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
            {
                Logger.Log($"Путь нормализован к родительской директории: {path} -> {parentDir}");
                return parentDir;
            }
        }

        return path;
    }

    /// <summary>
    /// Проверяет существование директории и выводит сообщение об ошибке
    /// </summary>
    public static bool ValidateAndPrintError(string? projectPath)
    {
        if (string.IsNullOrEmpty(projectPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Не удалось получить путь к проекту!");
            Console.ResetColor();
            return false;
        }

        if (!Directory.Exists(projectPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ Ошибка: Указанная папка не существует!");
            Console.WriteLine($"  Путь: {projectPath}");
            Console.ResetColor();
            Logger.LogError("Папка не найдена", new DirectoryNotFoundException(projectPath));
            return false;
        }

        return true;
    }
}