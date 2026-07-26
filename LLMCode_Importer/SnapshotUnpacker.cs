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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LLMCode_Importer;

public class SnapshotUnpacker
{
    private readonly UnpackerOptions _options;
    private static readonly Regex FileHeaderRegex = new(
        @"## 📄 `([^`\n]+)`\s*```(?:\w+)?\s*\n(.*?)\n```",
        RegexOptions.Singleline | RegexOptions.Compiled
    );

    public SnapshotUnpacker(UnpackerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<UnpackResult> UnpackAsync(string snapshotPath)
    {
        if (!File.Exists(snapshotPath))
            throw new FileNotFoundException($"Снапшот не найден: {snapshotPath}");

        var content = await File.ReadAllTextAsync(snapshotPath, Encoding.UTF8);
        var matches = FileHeaderRegex.Matches(content);

        var extracted = new List<string>(matches.Count);
        var outputRoot = _options.OutputRoot;

        if (!Directory.Exists(outputRoot))
            Directory.CreateDirectory(outputRoot);

        foreach (Match match in matches)
        {
            var relativePath = match.Groups[1].Value;
            var code = match.Groups[2].Value;

            var normalizedPath = relativePath.Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(outputRoot, normalizedPath);

            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!_options.Overwrite && File.Exists(fullPath))
            {
                Console.WriteLine($"⏭️  Пропущен (уже существует): {relativePath}");
                continue;
            }

            await File.WriteAllTextAsync(fullPath, code, Encoding.UTF8);
            extracted.Add(relativePath);
            Console.WriteLine($"📄 {relativePath}");
        }

        return new UnpackResult
        {
            ExtractedCount = extracted.Count,
            OutputRoot = outputRoot,
            ExtractedFiles = extracted.AsReadOnly()
        };
    }
}