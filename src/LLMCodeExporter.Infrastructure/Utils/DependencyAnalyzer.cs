namespace LLMCodeExporter.Infrastructure.Utils;

using Core.Models;
using System.Text;
using System.Text.RegularExpressions;

public static class DependencyAnalyzer
{
    /// <summary>
    /// Генерирует граф зависимостей в формате Mermaid
    /// </summary>
    public static string GenerateDependencyGraph(List<FileMetadata> files, int maxNodes = 20)
    {
        var dependencies = ExtractDependencies(files);

        if (!dependencies.Any())
            return string.Empty;

        var sb = new StringBuilder();

        sb.AppendLine("## 🔗 Граф зависимостей");
        sb.AppendLine();

        sb.AppendLine("<details>");
        sb.AppendLine("<summary>📊 Показать граф</summary>");
        sb.AppendLine();

        // ✅ ИСПОЛЬЗУЕМ ПЕРЕМЕННУЮ!
        string codeFence = "```";
        sb.AppendLine(codeFence + "mermaid");

        sb.AppendLine("graph TD");

        // Находим самые важные узлы (по количеству связей)
        var nodeImportance = CalculateNodeImportance(dependencies);
        var topNodes = nodeImportance
            .OrderByDescending(x => x.Value)
            .Take(maxNodes)
            .Select(x => x.Key)
            .ToHashSet();

        // Добавляем только важные связи
        var addedEdges = new HashSet<string>();
        foreach (var dep in dependencies)
        {
            if (!topNodes.Contains(dep.From) || !topNodes.Contains(dep.To))
                continue;

            string edge = $"{dep.From} --> {dep.To}";
            if (addedEdges.Contains(edge))
                continue;

            sb.AppendLine($"    {edge}");
            addedEdges.Add(edge);
        }

        sb.AppendLine(codeFence);  // ✅ Закрываем тоже через переменную!

        sb.AppendLine("</details>");
        sb.AppendLine();

        // Добавляем описание ключевых узлов
        var keyNodes = nodeImportance
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToList();

        if (keyNodes.Any())
        {
            sb.AppendLine("**Ключевые узлы:**");
            foreach (var node in keyNodes)
            {
                var inCount = dependencies.Count(d => d.To == node.Key);
                var outCount = dependencies.Count(d => d.From == node.Key);
                sb.AppendLine($"- `{node.Key}` - {GetNodeDescription(node.Key, inCount, outCount)}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();

        return sb.ToString();
    }



    private static List<Dependency> ExtractDependencies(List<FileMetadata> files)
    {
        var dependencies = new List<Dependency>();
        var classNames = files
            .Select(f => Path.GetFileNameWithoutExtension(f.RelativePath))
            .Where(name => !string.IsNullOrEmpty(name))
            .ToHashSet();

        foreach (var file in files)
        {
            try
            {
                string content = File.ReadAllText(file.FullPath);
                string fromClass = Path.GetFileNameWithoutExtension(file.RelativePath);

                // Ищем использование других классов
                foreach (var className in classNames)
                {
                    if (className == fromClass)
                        continue;

                    // Паттерны использования: new ClassName(), ClassName variable, ClassName.Method()
                    var patterns = new[]
                    {
                        $@"\bnew\s+{className}\s*\(",           // new ClassName()
                        $@"\b{className}\s+\w+\s*[=;]",         // ClassName variable
                        $@"\b{className}\.",                     // ClassName.Method()
                        $@":\s*{className}\b",                   // : ClassName (наследование)
                        $@"<{className}>",                       // List<ClassName>
                        $@"\({className}\s+\w+\)"               // (ClassName param)
                    };

                    if (patterns.Any(pattern => Regex.IsMatch(content, pattern)))
                    {
                        dependencies.Add(new Dependency
                        {
                            From = fromClass,
                            To = className
                        });
                    }
                }
            }
            catch
            {
                // Пропускаем файлы с ошибками чтения
            }
        }

        return dependencies;
    }

    private static Dictionary<string, int> CalculateNodeImportance(List<Dependency> dependencies)
    {
        var importance = new Dictionary<string, int>();

        foreach (var dep in dependencies)
        {
            // Узел важен если он имеет много входящих И исходящих связей
            if (!importance.ContainsKey(dep.From))
                importance[dep.From] = 0;
            if (!importance.ContainsKey(dep.To))
                importance[dep.To] = 0;

            importance[dep.From] += 1;  // Исходящая связь
            importance[dep.To] += 2;    // Входящая связь (важнее)
        }

        return importance;
    }

    private static string GetNodeDescription(string nodeName, int inCount, int outCount)
    {
        if (inCount > 5 && outCount > 5)
            return "центральный координатор";
        if (inCount > 5)
            return "используется многими классами";
        if (outCount > 5)
            return "использует много зависимостей";
        if (nodeName.Contains("Service"))
            return "сервисный слой";
        if (nodeName.Contains("Repository"))
            return "доступ к данным";
        if (nodeName.Contains("Manager"))
            return "управляющий компонент";

        return "компонент системы";
    }

    private class Dependency
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}
