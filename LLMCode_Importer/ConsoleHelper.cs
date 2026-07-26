/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.IO;

namespace LLMCode_Importer;

public static class ConsoleHelper
{
    public static void PrintHeader()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║  📦 LLMCode Importer v1.0             ║");
        Console.WriteLine("║     Распаковка снапшотов кода         ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void PrintHelp()
    {
        Console.WriteLine("ИСПОЛЬЗОВАНИЕ:");
        Console.WriteLine("  LLMCode_Importer <snapshot.md> [опции]");
        Console.WriteLine();
        Console.WriteLine("ОПЦИИ:");
        Console.WriteLine("  --output, -o <папка>     Выходная папка (по умолчанию текущая)");
        Console.WriteLine("  --config, -c <файл>      Файл конфигурации (unpacker.settings.json)");
        Console.WriteLine("  --overwrite              Перезаписывать существующие файлы (по умолчанию)");
        Console.WriteLine("  --no-overwrite           Не перезаписывать существующие файлы");
        Console.WriteLine("  --help, -h, /?           Показать эту справку");
        Console.WriteLine();
        Console.WriteLine("ПРИМЕРЫ:");
        Console.WriteLine("  LLMCode_Importer snapshot.md --output ./restored");
        Console.WriteLine("  LLMCode_Importer snapshot.md --no-overwrite");
        Console.WriteLine();
    }

    public static void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"❌ {message}");
        Console.ResetColor();
    }

    public static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️ {message}");
        Console.ResetColor();
    }

    public static void PrintInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"ℹ️ {message}");
        Console.ResetColor();
    }

    public static void PrintSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"✅ {message}");
        Console.ResetColor();
    }

    public static string AskForSnapshotPath()
    {
        Console.WriteLine("📁 Укажите путь к файлу снапшота (Markdown):");
        string? path;
        do
        {
            Console.Write("📂 > ");
            path = Console.ReadLine()?.Trim().Trim('"');

            if (string.IsNullOrEmpty(path))
            {
                PrintWarning("Путь не может быть пустым. Попробуйте снова или нажмите Ctrl+C для выхода.");
            }
            else if (!File.Exists(path))
            {
                PrintError($"Файл не найден: {path}");
                path = null;
            }
        } while (string.IsNullOrEmpty(path));

        return path;
    }

    public static string AskForOutputRoot()
    {
        string defaultPath = Directory.GetCurrentDirectory();
        Console.WriteLine();
        Console.WriteLine($"📁 Выходная папка (Enter для '{defaultPath}'):");
        Console.Write("📁 > ");
        string? input = Console.ReadLine()?.Trim().Trim('"');

        if (string.IsNullOrEmpty(input))
            return defaultPath;

        try
        {
            Directory.CreateDirectory(input);
            return input;
        }
        catch (Exception ex)
        {
            PrintError($"Не удалось создать папку: {ex.Message}");
            Console.WriteLine($"Будет использована папка по умолчанию: {defaultPath}");
            return defaultPath;
        }
    }

    public static bool AskForOverwrite()
    {
        Console.WriteLine();
        Console.Write("🔄 Перезаписывать существующие файлы? [Y/n]: ");
        var key = Console.ReadKey();
        Console.WriteLine();
        return key.Key != ConsoleKey.N;
    }
}