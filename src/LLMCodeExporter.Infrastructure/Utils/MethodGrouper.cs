namespace LLMCodeExporter.Infrastructure.Utils;

using System.Text.RegularExpressions;

public static class MethodGrouper
{
    /// <summary>
    /// Группирует методы по семантическому назначению
    /// </summary>
    public static Dictionary<string, List<string>> GroupMethods(string[] methodSignatures)
    {
        var groups = new Dictionary<string, List<string>>
        {
            ["📖 CRUD Operations"] = new List<string>(),
            ["✅ Validation & Checks"] = new List<string>(),
            ["🔍 Query & Search"] = new List<string>(),
            ["⚙️ Business Logic"] = new List<string>(),
            ["🛠️ Utility & Helpers"] = new List<string>(),
            ["🎯 Event Handlers"] = new List<string>(),
            ["📊 Data Transformation"] = new List<string>(),
            ["🔒 Security & Auth"] = new List<string>(),
            ["📝 Logging & Diagnostics"] = new List<string>(),
            ["🗑️ Other"] = new List<string>()
        };

        foreach (var method in methodSignatures)
        {
            string group = ClassifyMethod(method);
            if (groups.ContainsKey(group))
            {
                groups[group].Add(method);
            }
            else
            {
                groups["🗑️ Other"].Add(method);
            }
        }

        // Удаляем пустые группы
        var result = groups
            .Where(g => g.Value.Any())
            .ToDictionary(g => g.Key, g => g.Value);

        return result;
    }

    private static string ClassifyMethod(string methodSignature)
    {
        string methodName = ExtractMethodName(methodSignature);
        string lowerName = methodName.ToLower();

        // CRUD Operations
        if (Regex.IsMatch(lowerName, @"\b(get|find|fetch|load|read|select|retrieve)\b"))
            return "📖 CRUD Operations";
        if (Regex.IsMatch(lowerName, @"\b(add|create|insert|save|store|post)\b"))
            return "📖 CRUD Operations";
        if (Regex.IsMatch(lowerName, @"\b(update|modify|edit|change|put|patch)\b"))
            return "📖 CRUD Operations";
        if (Regex.IsMatch(lowerName, @"\b(delete|remove|destroy|clear)\b"))
            return "📖 CRUD Operations";

        // Validation
        if (Regex.IsMatch(lowerName, @"\b(validate|check|verify|is|has|can|ensure)\b"))
            return "✅ Validation & Checks";
        if (Regex.IsMatch(lowerName, @"\b(exists|contains|isvalid)\b"))
            return "✅ Validation & Checks";

        // Query & Search
        if (Regex.IsMatch(lowerName, @"\b(search|query|filter|find|where|lookup)\b"))
            return "🔍 Query & Search";
        if (Regex.IsMatch(lowerName, @"\b(getall|getby|findby|list)\b"))
            return "🔍 Query & Search";

        // Business Logic
        if (Regex.IsMatch(lowerName, @"\b(calculate|compute|process|execute|apply|perform)\b"))
            return "⚙️ Business Logic";
        if (Regex.IsMatch(lowerName, @"\b(generate|build|create|prepare)\b"))
            return "⚙️ Business Logic";

        // Security & Auth
        if (Regex.IsMatch(lowerName, @"\b(auth|login|logout|authorize|authenticate|permission)\b"))
            return "🔒 Security & Auth";
        if (Regex.IsMatch(lowerName, @"\b(encrypt|decrypt|hash|secure)\b"))
            return "🔒 Security & Auth";

        // Event Handlers
        if (Regex.IsMatch(lowerName, @"^on[A-Z]|_click|_load|_changed|eventhandler"))
            return "🎯 Event Handlers";
        if (methodSignature.Contains("EventHandler") || methodSignature.Contains("EventArgs"))
            return "🎯 Event Handlers";

        // Data Transformation
        if (Regex.IsMatch(lowerName, @"\b(convert|transform|map|to|from|parse|format)\b"))
            return "📊 Data Transformation";
        if (Regex.IsMatch(lowerName, @"\b(serialize|deserialize|encode|decode)\b"))
            return "📊 Data Transformation";

        // Logging
        if (Regex.IsMatch(lowerName, @"\b(log|trace|debug|warn|error|info)\b"))
            return "📝 Logging & Diagnostics";

        // Utility
        if (Regex.IsMatch(lowerName, @"\b(helper|util|tool|format|parse)\b"))
            return "🛠️ Utility & Helpers";

        return "🗑️ Other";
    }

    private static string ExtractMethodName(string methodSignature)
    {
        // Извлекаем имя метода из сигнатуры: "public void MyMethod(...)" -> "MyMethod"
        var match = Regex.Match(methodSignature, @"\b([A-Z_][a-zA-Z0-9_]*)\s*\(");
        return match.Success ? match.Groups[1].Value : methodSignature;
    }

    /// <summary>
    /// Форматирует сгруппированные методы в строку
    /// </summary>
    public static string FormatGroupedMethods(Dictionary<string, List<string>> groups)
    {
        var sb = new System.Text.StringBuilder();

        foreach (var group in groups)
        {
            sb.AppendLine($"    // {group.Key} ({group.Value.Count} methods)");
            foreach (var method in group.Value)
            {
                string methodLine = method.TrimStart();
                sb.AppendLine($"    {methodLine}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

}
