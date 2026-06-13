/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿using System;
using System.IO;
using System.Linq;
using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Infrastructure.Utils
{
    public static class ProjectTypeDetector
    {
        public static ProjectType Detect(string projectPath)
        {
            if (!Directory.Exists(projectPath))
                return ProjectType.Generic;

            // Получаем только имена файлов для ускорения проверки условий
            var topFiles = Directory.GetFiles(projectPath, "*", SearchOption.TopDirectoryOnly)
                                    .Select(Path.GetFileName)
                                    .ToArray();

            var allFiles = Directory.GetFiles(projectPath, "*", SearchOption.AllDirectories)
                                   .Select(Path.GetFileName)
                                   .ToArray();

            // 1. Проверка Web-приложений (JS, CSS, HTML)
            var webExtensions = new[] { ".html", ".htm", ".css", ".scss", ".sass", ".less", ".js", ".jsx", ".ts", ".tsx" };
            
            bool hasWebFiles = topFiles.Any(f => webExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)) ||
                               allFiles.Any(f => webExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));

            if (hasWebFiles)
            {
                if (topFiles.Any(f => f.Equals("package.json", StringComparison.OrdinalIgnoreCase)) ||
                    allFiles.Any(f => f.Equals("package.json", StringComparison.OrdinalIgnoreCase)))
                {
                    return ProjectType.WebApp;
                }

                if (topFiles.Any(f => f.Equals("index.html", StringComparison.OrdinalIgnoreCase) || 
                                      f.Equals("index.htm", StringComparison.OrdinalIgnoreCase)))
                {
                    return ProjectType.WebApp;
                }
            }

            // 2. Python проекты
            if (topFiles.Any(f => f.Equals("requirements.txt", StringComparison.OrdinalIgnoreCase) ||
                                  f.Equals("setup.py", StringComparison.OrdinalIgnoreCase) ||
                                  f.Equals("pyproject.toml", StringComparison.OrdinalIgnoreCase)) ||
                topFiles.Any(f => Path.GetExtension(f).Equals(".py", StringComparison.OrdinalIgnoreCase)) ||
                allFiles.Any(f => Path.GetExtension(f).Equals(".py", StringComparison.OrdinalIgnoreCase)))
            {
                return ProjectType.Python;
            }

            // 3. C# проекты
            if (topFiles.Any(f => Path.GetExtension(f).Equals(".csproj", StringComparison.OrdinalIgnoreCase) ||
                                  Path.GetExtension(f).Equals(".sln", StringComparison.OrdinalIgnoreCase)) ||
                allFiles.Any(f => Path.GetExtension(f).Equals(".cs", StringComparison.OrdinalIgnoreCase)))
            {
                return ProjectType.CSharp;
            }

            // 4. JavaScript/TypeScript NodeJS проекты
            if (topFiles.Any(f => f.Equals("package.json", StringComparison.OrdinalIgnoreCase)))
            {
                if (topFiles.Any(f => Path.GetExtension(f).Equals(".ts", StringComparison.OrdinalIgnoreCase) || 
                                      Path.GetExtension(f).Equals(".tsx", StringComparison.OrdinalIgnoreCase)))
                {
                    return ProjectType.TypeScript;
                }
                return ProjectType.JavaScript;
            }

            return ProjectType.Generic;
        }
    }
}
