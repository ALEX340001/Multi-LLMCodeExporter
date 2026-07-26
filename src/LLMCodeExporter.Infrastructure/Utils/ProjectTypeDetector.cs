/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using LLMCodeExporter.Core.Models;
namespace LLMCodeExporter.Infrastructure.Utils
{
    public static class ProjectTypeDetector
    {
        public static ProjectType Detect(string projectPath)
        {
            if (!Directory.Exists(projectPath))
                return ProjectType.Generic;
            var topFiles = Directory.GetFiles(projectPath, "*", SearchOption.TopDirectoryOnly)
                                    .Select(Path.GetFileName)
                                    .ToArray();
            var allFiles = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                                    .Select(Path.GetFileName)
                                    .ToArray();
            bool hasWebFolder = Directory.Exists(Path.Combine(projectPath, "wwwroot")) ||
                                Directory.Exists(Path.Combine(projectPath, "Client")) ||
                                Directory.Exists(Path.Combine(projectPath, "UI"));
            var csharpExtensions = new[] { ".cs", ".csproj", ".sln" };
            bool hasCSharpFiles = topFiles.Any(f => csharpExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                               || allFiles.Any(f => csharpExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
            var webExtensions = new[] { ".html", ".htm", ".css", ".scss", ".sass", ".less", ".js", ".jsx", ".ts", ".tsx" };
            bool hasWebFiles = topFiles.Any(f => webExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                            || allFiles.Any(f => webExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
            if (hasCSharpFiles && (hasWebFiles || hasWebFolder))
                return ProjectType.Hybrid;
            if (hasWebFiles && !hasCSharpFiles)
            {
                if (topFiles.Any(f => f != null && f.Equals("package.json", StringComparison.OrdinalIgnoreCase)) ||
                    allFiles.Any(f => f != null && f.Equals("package.json", StringComparison.OrdinalIgnoreCase)))
                    return ProjectType.WebApp;
                if (topFiles.Any(f => f != null && (f.Equals("index.html", StringComparison.OrdinalIgnoreCase) ||
                                                    f.Equals("index.htm", StringComparison.OrdinalIgnoreCase))))
                    return ProjectType.WebApp;
            }

            if (topFiles.Any(f => f != null && (f.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase) ||
                                                f.Equals("setup.py", StringComparison.OrdinalIgnoreCase) ||
                                                f.Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase))) ||
                topFiles.Any(f => f != null && Path.GetExtension(f).Equals(".py", StringComparison.OrdinalIgnoreCase)) ||
                allFiles.Any(f => f != null && Path.GetExtension(f).Equals(".py", StringComparison.OrdinalIgnoreCase)))
            {
                return ProjectType.Python;
            }

            if (hasCSharpFiles && !hasWebFiles && !hasWebFolder)
                return ProjectType.CSharp;
            if (topFiles.Any(f => f != null && f.Equals("package.json", StringComparison.OrdinalIgnoreCase)))
            {
                if (topFiles.Any(f => f != null && (Path.GetExtension(f).Equals(".ts", StringComparison.OrdinalIgnoreCase) ||
                                                    Path.GetExtension(f).Equals(".tsx", StringComparison.OrdinalIgnoreCase))))
                    return ProjectType.TypeScript;
                return ProjectType.JavaScript;
            }

            return ProjectType.Generic;
        }

        /// <summary>
        /// Определяет список языков, присутствующих в проекте, с учётом приоритетов.
        /// </summary>
        public static List<Language> DetectLanguages(string projectPath)
        {
            var detected = new HashSet<Language>();
            if (!Directory.Exists(projectPath))
                return detected.ToList();
            var allFiles = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories);
            var fileNames = allFiles.Select(Path.GetFileName).ToArray();
            // C#
            if (allFiles.Any(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) ||
                allFiles.Any(f => f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)) ||
                allFiles.Any(f => f.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)))
                detected.Add(Language.CSharp);
            // Python
            if (allFiles.Any(f => f.EndsWith(".py", StringComparison.OrdinalIgnoreCase)) ||
                fileNames.Any(f => f != null && f.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase)) ||
                fileNames.Any(f => f != null && f.Equals("setup.py", StringComparison.OrdinalIgnoreCase)) ||
                fileNames.Any(f => f != null && f.Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase)))
                detected.Add(Language.Python);
            // TypeScript (имеет приоритет над JavaScript)
            bool hasTypeScript = allFiles.Any(f => f.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                                                   f.EndsWith(".tsx", StringComparison.OrdinalIgnoreCase));
            if (hasTypeScript)
            {
                detected.Add(Language.TypeScript);
            }
            else
            {
                // JavaScript только если нет TypeScript
                if (allFiles.Any(f => f.EndsWith(".js", StringComparison.OrdinalIgnoreCase)) ||
                    fileNames.Any(f => f != null && f.Equals("package.json", StringComparison.OrdinalIgnoreCase)))
                    detected.Add(Language.JavaScript);
            }

            // Java
            if (allFiles.Any(f => f.EndsWith(".java", StringComparison.OrdinalIgnoreCase)) ||
                fileNames.Any(f => f != null && f.Equals("pom.xml", StringComparison.OrdinalIgnoreCase)) ||
                fileNames.Any(f => f != null && f.Equals("build.gradle", StringComparison.OrdinalIgnoreCase)))
                detected.Add(Language.Java);
            // Go
            if (allFiles.Any(f => f.EndsWith(".go", StringComparison.OrdinalIgnoreCase)) ||
                fileNames.Any(f => f != null && f.Equals("go.mod", StringComparison.OrdinalIgnoreCase)))
                detected.Add(Language.Go);
            // Ruby
            if (allFiles.Any(f => f.EndsWith(".rb", StringComparison.OrdinalIgnoreCase)) ||
                fileNames.Any(f => f != null && f.Equals("Gemfile", StringComparison.OrdinalIgnoreCase)))
                detected.Add(Language.Ruby);
            // PHP
            if (allFiles.Any(f => f.EndsWith(".php", StringComparison.OrdinalIgnoreCase)) ||
                fileNames.Any(f => f != null && f.Equals("composer.json", StringComparison.OrdinalIgnoreCase)))
                detected.Add(Language.PHP);
            return detected.ToList();
        }
    }
}