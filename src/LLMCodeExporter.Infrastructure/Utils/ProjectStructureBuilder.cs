namespace LLMCodeExporter.Infrastructure.Utils;

using System.Text;

public static class ProjectStructureBuilder
{
    /// <summary>
    /// Генерирует древовидную структуру проекта
    /// </summary>
    public static string BuildTree(string projectPath, string[] filePaths)
    {
        var tree = new StringBuilder();

        // Группируем файлы по папкам
        var filesByDirectory = filePaths
            .Select(f => new
            {
                FullPath = f,
                RelativePath = Path.GetRelativePath(projectPath, f),
                Directory = Path.GetDirectoryName(Path.GetRelativePath(projectPath, f)) ?? string.Empty,
                FileName = Path.GetFileName(f)
            })
            .GroupBy(f => f.Directory)
            .OrderBy(g => g.Key)
            .ToList();

        tree.AppendLine($"📁 {Path.GetFileName(projectPath)}/");

        // Сначала файлы в корне
        var rootFiles = filesByDirectory.FirstOrDefault(g => string.IsNullOrEmpty(g.Key));
        if (rootFiles != null)
        {
            foreach (var file in rootFiles.OrderBy(f => f.FileName))
            {
                tree.AppendLine($"├── 📄 {file.FileName}");
            }
        }

        // Затем папки с файлами
        var directories = filesByDirectory
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .ToList();

        foreach (var dir in directories)
        {
            string[] pathParts = dir.Key.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string indent = new string('│', pathParts.Length - 1);
            string connector = "├──";

            // Показываем папку
            tree.AppendLine($"{indent}{connector} 📂 {pathParts.Last()}/");

            // Показываем файлы в папке
            foreach (var file in dir.OrderBy(f => f.FileName))
            {
                string fileIndent = new string('│', pathParts.Length);
                tree.AppendLine($"{fileIndent}├── 📄 {file.FileName}");
            }
        }

        return tree.ToString();
    }

    /// <summary>
    /// Генерирует компактную структуру (только пути)
    /// </summary>
    public static string BuildCompactTree(string projectPath, string[] filePaths)
    {
        var tree = new StringBuilder();
        var relativePaths = filePaths
            .Select(f => Path.GetRelativePath(projectPath, f))
            .OrderBy(p => p)
            .ToList();

        tree.AppendLine($"📁 {Path.GetFileName(projectPath)}/");

        foreach (var path in relativePaths)
        {
            int depth = path.Count(c => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar);
            string indent = new string(' ', depth * 2);
            string fileName = Path.GetFileName(path);
            tree.AppendLine($"{indent}└── {fileName}");
        }

        return tree.ToString();
    }

    /// <summary>
    /// Генерирует детальную статистику по папкам
    /// </summary>
    public static string BuildStatisticsByFolder(string projectPath, IEnumerable<Core.Models.FileMetadata> files)
    {
        var sb = new StringBuilder();

        var folderStats = files
            .GroupBy(f => Path.GetDirectoryName(f.RelativePath) ?? "Root")
            .Select(g => new
            {
                Folder = g.Key,
                FileCount = g.Count(),
                TotalSize = g.Sum(f => f.SizeInBytes),
                EstimatedTokens = g.Sum(f => f.EstimatedTokens)
            })
            .OrderByDescending(s => s.EstimatedTokens)
            .ToList();

        sb.AppendLine("## 📊 Статистика по папкам\n");
        sb.AppendLine("| Папка | Файлов | Размер | ~Токенов |");
        sb.AppendLine("|-------|--------|--------|----------|");

        foreach (var stat in folderStats)
        {
            string folderName = stat.Folder == "Root" ? "(корень)" : stat.Folder;
            sb.AppendLine($"| {folderName} | {stat.FileCount} | {FormatSize(stat.TotalSize)} | ~{stat.EstimatedTokens:N0} |");
        }

        sb.AppendLine();
        return sb.ToString();
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
        return $"{bytes / (1024 * 1024)} MB";
    }

    /// <summary>
    /// Генерирует полную иерархическую структуру с вложенностью
    /// </summary>
    public static string BuildHierarchicalTree(string projectPath, string[] filePaths)
    {
        var tree = new StringBuilder();
        var rootNode = BuildTreeNode(projectPath, filePaths);

        AppendNode(tree, rootNode, "", true);

        return tree.ToString();
    }

    private static TreeNode BuildTreeNode(string projectPath, string[] filePaths)
    {
        var root = new TreeNode
        {
            Name = Path.GetFileName(projectPath),
            IsDirectory = true,
            Children = new List<TreeNode>()
        };

        var filesByPath = filePaths
            .Select(f => Path.GetRelativePath(projectPath, f))
            .OrderBy(p => p)
            .ToList();

        var directoryNodes = new Dictionary<string, TreeNode>();

        foreach (var relativePath in filesByPath)
        {
            var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            TreeNode currentNode = root;
            string currentPath = "";

            // Создаём промежуточные папки
            for (int i = 0; i < parts.Length - 1; i++)
            {
                currentPath = string.IsNullOrEmpty(currentPath)
                    ? parts[i]
                    : Path.Combine(currentPath, parts[i]);

                if (!directoryNodes.ContainsKey(currentPath))
                {
                    var dirNode = new TreeNode
                    {
                        Name = parts[i],
                        IsDirectory = true,
                        Children = new List<TreeNode>()
                    };
                    currentNode.Children.Add(dirNode);
                    directoryNodes[currentPath] = dirNode;
                    currentNode = dirNode;
                }
                else
                {
                    currentNode = directoryNodes[currentPath];
                }
            }

            // Добавляем файл
            currentNode.Children.Add(new TreeNode
            {
                Name = parts.Last(),
                IsDirectory = false
            });
        }

        return root;
    }

    private static void AppendNode(StringBuilder sb, TreeNode node, string indent, bool isLast)
    {
        string icon = node.IsDirectory ? "📂" : "📄";
        string connector = isLast ? "└──" : "├──";

        if (indent == "")
        {
            sb.AppendLine($"📁 {node.Name}/");
        }
        else
        {
            sb.AppendLine($"{indent}{connector} {icon} {node.Name}{(node.IsDirectory ? "/" : "")}");
        }

        if (node.Children != null && node.Children.Any())
        {
            string childIndent = indent + (isLast ? "    " : "│   ");

            for (int i = 0; i < node.Children.Count; i++)
            {
                bool isLastChild = i == node.Children.Count - 1;
                AppendNode(sb, node.Children[i], childIndent, isLastChild);
            }
        }
    }

    private class TreeNode
    {
        public string Name { get; set; }
        public bool IsDirectory { get; set; }
        public List<TreeNode> Children { get; set; }
    }
}
