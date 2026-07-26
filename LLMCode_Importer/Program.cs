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
using System.Text;
using System.Threading.Tasks;

namespace LLMCode_Importer;

public static class Program
{
    private const string DefaultConfigFileName = "unpacker.settings.json";

    public static async Task<int> Main(string[] args)
    {
        if (args.Length == 1 && (args[0] == "--help" || args[0] == "-h" || args[0] == "/?"))
        {
            ConsoleHelper.PrintHelp();
            return 0;
        }

        return args.Length == 0
            ? await RunInteractiveAsync()
            : await RunCliAsync(args);
    }

    private static async Task<int> RunCliAsync(string[] args)
    {
        string outputRoot = null!;
        string configPath = DefaultConfigFileName;
        bool overwrite = true;
        var positionalArgs = new List<string>();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            switch (arg)
            {
                case "--output":
                case "-o":
                    if (i + 1 < args.Length)
                        outputRoot = args[++i];
                    else
                    {
                        ConsoleHelper.PrintError("не указан путь после --output");
                        return 1;
                    }
                    break;

                case "--config":
                case "-c":
                    if (i + 1 < args.Length)
                        configPath = args[++i];
                    else
                    {
                        ConsoleHelper.PrintError("не указан путь к конфигу");
                        return 1;
                    }
                    break;

                case "--overwrite":
                    overwrite = true;
                    break;

                case "--no-overwrite":
                    overwrite = false;
                    break;

                default:
                    if (!arg.StartsWith("--") && !arg.StartsWith("-"))
                        positionalArgs.Add(arg);
                    else
                    {
                        ConsoleHelper.PrintError($"Неизвестный аргумент: {arg}");
                        ConsoleHelper.PrintHelp();
                        return 1;
                    }
                    break;
            }
        }

        string snapshotPath = string.Join(" ", positionalArgs);

        UnpackerOptions? configOptions = null;
        if (File.Exists(configPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(configPath, Encoding.UTF8);
                configOptions = System.Text.Json.JsonSerializer.Deserialize<UnpackerOptions>(json);
                ConsoleHelper.PrintInfo($"Загружен конфиг: {configPath}");
            }
            catch (Exception ex)
            {
                ConsoleHelper.PrintWarning($"Ошибка чтения конфига: {ex.Message}");
            }
        }

        outputRoot ??= configOptions?.OutputRoot ?? Directory.GetCurrentDirectory();

        if (string.IsNullOrEmpty(snapshotPath))
        {
            snapshotPath = configOptions?.DefaultSnapshotPath ?? string.Empty;
            if (string.IsNullOrEmpty(snapshotPath))
            {
                ConsoleHelper.PrintError("не указан путь к снапшоту ни в аргументах, ни в конфиге.");
                ConsoleHelper.PrintHelp();
                return 1;
            }
        }

        if (!File.Exists(snapshotPath))
        {
            ConsoleHelper.PrintError($"Файл не найден: {snapshotPath}");
            return 1;
        }

        if (configOptions is not null && !HasOverwriteArgument(args))
            overwrite = configOptions.Overwrite;

        return await UnpackCoreAsync(snapshotPath, outputRoot, overwrite);
    }

    private static async Task<int> RunInteractiveAsync()
    {
        ConsoleHelper.PrintHeader();

        string snapshotPath = ConsoleHelper.AskForSnapshotPath();
        if (string.IsNullOrEmpty(snapshotPath))
            return 0;

        string outputRoot = ConsoleHelper.AskForOutputRoot();
        if (string.IsNullOrEmpty(outputRoot))
            return 0;

        bool overwrite = ConsoleHelper.AskForOverwrite();

        return await UnpackCoreAsync(snapshotPath, outputRoot, overwrite);
    }

    private static async Task<int> UnpackCoreAsync(string snapshotPath, string outputRoot, bool overwrite)
    {
        try
        {
            var options = new UnpackerOptions
            {
                OutputRoot = outputRoot,
                Overwrite = overwrite
            };
            var unpacker = new SnapshotUnpacker(options);
            var result = await unpacker.UnpackAsync(snapshotPath);

            Console.WriteLine();
            ConsoleHelper.PrintSuccess($"Распаковано файлов: {result.ExtractedCount}");
            ConsoleHelper.PrintInfo($"Корневая папка: {result.OutputRoot}");
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelper.PrintError($"Критическая ошибка: {ex.Message}");
            return 1;
        }
    }

    private static bool HasOverwriteArgument(string[] args)
    {
        foreach (var arg in args)
            if (arg == "--overwrite" || arg == "--no-overwrite")
                return true;
        return false;
    }
}