/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
using System.Linq;
namespace LLMCodeExporter.Core.Models;
/// <summary>
/// Настройки языка программирования
/// </summary>
public class LanguageSettings
{
    public string[] FileExtensions { get; set; } = Array.Empty<string>();
    public string[] ExcludeFolders { get; set; } = Array.Empty<string>();
    public string[] EntryPoints { get; set; } = Array.Empty<string>();
    public string CommentSyntax { get; set; } = "//";
    public bool HasPackageManager { get; set; } = false;
    public string[] PackageFiles { get; set; } = Array.Empty<string>();
    // ================================================================
    // СТАТИЧЕСКИЕ НАСТРОЙКИ ДЛЯ КАЖДОГО ЯЗЫКА
    // ================================================================
    public static LanguageSettings ForCSharp => new()
    {
        FileExtensions = new[] { ".cs", ".csproj", ".sln", ".config", ".json", ".cshtml", ".razor" },
        ExcludeFolders = new[] { "bin", "obj", "packages", ".git", ".vs" },
        EntryPoints = new[] { "Program.cs", "Startup.cs", "Main.cs" },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { ".csproj", ".sln" }
    };
    public static LanguageSettings ForPython => new()
    {
        FileExtensions = new[] { ".py", ".pyw", ".txt", ".json", ".yaml", ".yml", ".toml", ".cfg", ".ini" },
        ExcludeFolders = new[] { "__pycache__", ".venv", "venv", "env", ".git", ".pytest_cache", ".mypy_cache" },
        EntryPoints = new[] { "main.py", "__main__.py", "app.py", "manage.py", "wsgi.py", "asgi.py" },
        CommentSyntax = "#",
        HasPackageManager = true,
        PackageFiles = new[] { "requirements.txt", "pyproject.toml", "setup.py", "setup.cfg", "Pipfile" }
    };
    public static LanguageSettings ForJavaScript => new()
    {
        FileExtensions = new[] { ".js", ".jsx", ".json", ".mjs", ".cjs" },
        ExcludeFolders = new[] { "node_modules", "dist", "build", ".git", ".cache", ".next" },
        EntryPoints = new[] { "index.js", "main.js", "app.js", "server.js", "package.json" },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { "package.json", "package-lock.json", "yarn.lock", "pnpm-lock.yaml" }
    };
    public static LanguageSettings ForTypeScript => new()
    {
        FileExtensions = new[] { ".ts", ".tsx", ".js", ".jsx", ".json", ".mjs", ".cjs" },
        ExcludeFolders = new[] { "node_modules", "dist", "build", ".git", ".cache", ".next" },
        EntryPoints = new[] { "index.ts", "main.ts", "app.ts", "server.ts", "package.json" },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { "package.json", "package-lock.json", "tsconfig.json", "yarn.lock" }
    };
    public static LanguageSettings ForJava => new()
    {
        FileExtensions = new[] { ".java", ".jsp", ".xml", ".properties", ".gradle", ".kt" },
        ExcludeFolders = new[] { "target", "build", ".git", ".idea", ".gradle", "out" },
        EntryPoints = new[] { "Main.java", "Application.java", "App.java" },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { "pom.xml", "build.gradle", "settings.gradle" }
    };
    public static LanguageSettings ForGo => new()
    {
        FileExtensions = new[] { ".go", ".mod", ".sum", ".tmpl" },
        ExcludeFolders = new[] { "bin", "pkg", ".git", "vendor" },
        EntryPoints = new[] { "main.go", "cmd/main.go" },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { "go.mod", "go.sum" }
    };
    public static LanguageSettings ForRuby => new()
    {
        FileExtensions = new[] { ".rb", ".erb", ".rake", ".gemspec", ".yml", ".yaml", ".gemfile" },
        ExcludeFolders = new[] { "vendor", ".git", ".bundle", "tmp", "log" },
        EntryPoints = new[] { "main.rb", "app.rb", "config.ru", "Rakefile", "Gemfile" },
        CommentSyntax = "#",
        HasPackageManager = true,
        PackageFiles = new[] { "Gemfile", "Gemfile.lock" }
    };
    public static LanguageSettings ForPHP => new()
    {
        FileExtensions = new[] { ".php", ".html", ".htm", ".css", ".js", ".json", ".xml" },
        ExcludeFolders = new[] { "vendor", "node_modules", ".git", "tmp", "cache", "logs" },
        EntryPoints = new[] { "index.php", "main.php", "app.php", "bootstrap.php" },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { "composer.json", "composer.lock" }
    };
    public static LanguageSettings ForWebApp => new()
    {
        FileExtensions = new[] { ".js", ".jsx", ".ts", ".tsx", ".css", ".scss", ".sass", ".less", ".html", ".htm", ".json", ".xml", ".svg" },
        ExcludeFolders = new[] { "node_modules", "dist", "build", ".git", ".next", ".nuxt", ".cache" },
        EntryPoints = new[] { "index.html", "main.js", "app.js", "styles.css", "package.json" },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { "package.json", "package-lock.json", "yarn.lock" }
    };
    public static LanguageSettings ForGeneric => new()
    {
        FileExtensions = new[] { ".txt", ".md", ".json", ".xml", ".yaml", ".yml", ".cfg", ".ini", ".csv", ".log" },
        ExcludeFolders = new[] { ".git", ".vs", "node_modules", "bin", "obj", "packages", "__pycache__" },
        EntryPoints = Array.Empty<string>(),
        CommentSyntax = "//",
        HasPackageManager = false,
        PackageFiles = Array.Empty<string>()
    };
    /// <summary>
    /// Возвращает настройки для указанного языка.
    /// </summary>
    public static LanguageSettings ForLanguage(Language lang)
    {
        return lang switch
        {
            Language.CSharp => ForCSharp,
            Language.Python => ForPython,
            Language.JavaScript => ForJavaScript,
            Language.TypeScript => ForTypeScript,
            Language.Java => ForJava,
            Language.Go => ForGo,
            Language.Ruby => ForRuby,
            Language.PHP => ForPHP,
            Language.Generic => ForGeneric,
            _ => new LanguageSettings
            {
                FileExtensions = new[] { ".*" },
                ExcludeFolders = new[] { ".git", ".vs", "node_modules", "bin", "obj" },
                EntryPoints = Array.Empty<string>(),
                CommentSyntax = "//"
            }
        };
    }

    /// <summary>
    /// Объединяет настройки двух языков (для гибридного режима).
    /// </summary>
    public static LanguageSettings Combine(LanguageSettings first, LanguageSettings second)
    {
        return new LanguageSettings
        {
            FileExtensions = first.FileExtensions.Union(second.FileExtensions).ToArray(),
            ExcludeFolders = first.ExcludeFolders.Union(second.ExcludeFolders).ToArray(),
            EntryPoints = first.EntryPoints.Union(second.EntryPoints).ToArray(),
            CommentSyntax = first.CommentSyntax,
            HasPackageManager = first.HasPackageManager || second.HasPackageManager,
            PackageFiles = first.PackageFiles.Union(second.PackageFiles).ToArray()
        };
    }

    // ================================================================
    // СТАРАЯ ВЕРСИЯ ForHybrid (для обратной совместимости)
    // ================================================================
    public static LanguageSettings ForHybrid => new()
    {
        FileExtensions = new[]
        {
            ".cs", ".cshtml", ".razor",
            ".js", ".jsx", ".ts", ".tsx",
            ".html", ".htm", ".css", ".scss", ".sass", ".less",
            ".json", ".xml", ".svg"
        },
        ExcludeFolders = new[]
        {
            "bin", "obj", "node_modules", "dist", "build",
            ".git", ".vs", ".cache", ".next", ".nuxt"
        },
        EntryPoints = new[]
        {
            "Program.cs", "Startup.cs",
            "index.html", "main.js", "app.js", "package.json"
        },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { ".csproj", ".sln", "package.json", "package-lock.json" }
    };
}