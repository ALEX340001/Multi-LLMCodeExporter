
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
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Utils.Architecture;

/// <summary>
/// Детектор паттернов проектирования
/// </summary>
public static class PatternDetector
{
    private static readonly List<PatternDefinition> _patterns = new()
    {
        new PatternDefinition("Repository", new[] { "REPOSITORY" }),
        new PatternDefinition("Service Layer", new[] { "SERVICE" }),
        new PatternDefinition("Factory", new[] { "FACTORY" }),
        new PatternDefinition("Singleton", new[] { "SINGLETON" }),
        new PatternDefinition("Observer", new[] { "OBSERVER" }),
        new PatternDefinition("Strategy", new[] { "STRATEGY" }),
        new PatternDefinition("Decorator", new[] { "DECORATOR" }),
        new PatternDefinition("Dependency Injection", new[] { "I[A-Z]" }, (files) =>
        {
            return files.Any(f => Path.GetFileName(f.RelativePath).StartsWith("I") &&
                                  files.Any(f2 => Path.GetFileNameWithoutExtension(f2.RelativePath) ==
                                                  Path.GetFileNameWithoutExtension(f.RelativePath).Substring(1)));
        }),
        new PatternDefinition("MVC", new[] { "MODEL", "VIEW", "CONTROLLER" }, (files) =>
        {
            return files.Any(f => f.RelativePath.IndexOf("MODEL", StringComparison.OrdinalIgnoreCase) >= 0) &&
                   files.Any(f => f.RelativePath.IndexOf("VIEW", StringComparison.OrdinalIgnoreCase) >= 0) &&
                   files.Any(f => f.RelativePath.IndexOf("CONTROLLER", StringComparison.OrdinalIgnoreCase) >= 0);
        })
    };

    public static List<string> DetectPatterns(List<FileMetadata> files)
    {
        var detected = new List<string>();

        foreach (var pattern in _patterns)
        {
            if (pattern.Detect(files))
                detected.Add(pattern.Name);
        }

        return detected;
    }

    private class PatternDefinition
    {
        public string Name { get; }
        private readonly string[] _keywords;
        private readonly Func<List<FileMetadata>, bool>? _customDetector;

        public PatternDefinition(string name, string[] keywords, Func<List<FileMetadata>, bool>? customDetector = null)
        {
            Name = name;
            _keywords = keywords;
            _customDetector = customDetector;
        }

        public bool Detect(List<FileMetadata> files)
        {
            if (_customDetector != null)
                return _customDetector(files);

            var normalizedPaths = files.Select(f => f.RelativePath.Replace('\\', '/').ToUpperInvariant()).ToList();

            if (_keywords.Length == 1)
                return normalizedPaths.Any(p => p.Contains(_keywords[0]));

            // Для MVC — все ключевые слова должны присутствовать
            return _keywords.All(keyword => normalizedPaths.Any(p => p.Contains(keyword)));
        }
    }
}