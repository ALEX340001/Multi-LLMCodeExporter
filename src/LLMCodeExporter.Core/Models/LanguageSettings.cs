namespace LLMCodeExporter.Core.Models;
/// <summary>
/// Настройки языка программирования
/// </summary>
public class LanguageSettings
{
    /// <summary>
    /// Настройки для C#
    /// </summary>
    public static LanguageSettings ForCSharp => new()
    {
        FileExtensions = new[] { ".cs", ".csproj", ".sln", ".config", ".json", ".cshtml", ".razor" },
        ExcludeFolders = new[] { "bin", "obj", "packages", ".git", ".vs" },
        EntryPoints = new[] { "Program.cs", "Startup.cs", "Main.cs" },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { ".csproj", ".sln" }
    };

    /// <summary>
    /// Настройки для TypeScript
    /// </summary>
    public static LanguageSettings ForTypeScript => new()
    {
        FileExtensions = new[] { ".ts", ".tsx", ".js", ".jsx", ".json" },
        ExcludeFolders = new[] { "node_modules", "dist", "build", ".git" }
    };

    /// <summary>
    /// Настройки для JavaScript
    /// </summary>
    public static LanguageSettings ForJavaScript => new()
    {
        FileExtensions = new[] { ".js", ".jsx", ".json" },
        ExcludeFolders = new[] { "node_modules", "dist", "build", ".git" }
    };

    /// <summary>
    /// Настройки для Python
    /// </summary>
    public static LanguageSettings ForPython => new()
    {
        FileExtensions = new[] { ".py", ".pyw", ".txt", ".json", ".yaml", ".yml" },
        ExcludeFolders = new[] { "__pycache__", ".venv", "venv", "env", ".git" },
        EntryPoints = new[] { "main.py", "__main__.py", "app.py", "manage.py" },
        CommentSyntax = "#",
        HasPackageManager = true,
        PackageFiles = new[] { "requirements.txt", "pyproject.toml", "setup.py" }
    };

    /// <summary>
    /// Настройки для Web-приложений (JS, CSS, HTML) - НОВЫЙ
    /// </summary>
    public static LanguageSettings ForWebApp => new()
    {
        FileExtensions = new[] { ".js", ".jsx", ".ts", ".tsx", ".css", ".scss", ".sass", ".less", ".html", ".htm", ".json", ".xml", ".svg" },
        ExcludeFolders = new[] { "node_modules", "dist", "build", ".git", ".next", ".nuxt", ".cache" },
        EntryPoints = new[] { "index.html", "main.js", "app.js", "styles.css", "package.json", "vite.config.js", "webpack.config.js" },
        CommentSyntax = "//",
        HasPackageManager = true,
        PackageFiles = new[] { "package.json", "package-lock.json", "yarn.lock", "pnpm-lock.yaml" }
    };

    /// <summary>
    /// Расширения файлов
    /// </summary>
    public string[] FileExtensions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Папки для исключения
    /// </summary>
    public string[] ExcludeFolders { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Точки входа (имена файлов)
    /// </summary>
    public string[] EntryPoints { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Синтаксис комментариев
    /// </summary>
    public string CommentSyntax { get; set; } = "//";

    /// <summary>
    /// Имеет менеджер пакетов
    /// </summary>
    public bool HasPackageManager { get; set; } = false;

    /// <summary>
    /// Файлы пакетов (например, .csproj, package.json)
    /// </summary>
    public string[] PackageFiles { get; set; } = Array.Empty<string>();
}