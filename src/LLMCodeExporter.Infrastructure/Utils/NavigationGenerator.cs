namespace LLMCodeExporter.Infrastructure.Utils;

using Core.Models;
using System.Text;

/// <summary>
/// Генератор навигационных ссылок для быстрого перемещения по коду
/// </summary>
public static class NavigationGenerator
{
    /// <summary>
    /// Генерирует Quick Links секцию для навигации
    /// </summary>
    public static string GenerateQuickLinks(List<FileMetadata> files, ExportSettings settings)
    {
        if (!settings.GenerateQuickLinks || !files.Any())
            return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine("## 🧭 Quick Links");
        sb.AppendLine();

        // Entry Points с якорями
        var entryPoints = FindEntryPoints(files, settings.EntryPoints);
        if (entryPoints.Any())
        {
            sb.AppendLine("**Entry Points:**");
            foreach (var file in entryPoints)
            {
                // ✅ Генерируем якорь прямо здесь
                string anchor = GenerateFileAnchor(file.RelativePath);
                sb.AppendLine($"- [`{file.RelativePath}`](#{anchor})");
            }
            sb.AppendLine();
        }

        // Архитектурные слои с якорями
        var layerGroups = GroupByLayer(files);
        if (layerGroups.Any())
        {
            sb.AppendLine("**Архитектурные слои:**");

            foreach (var layer in layerGroups.OrderBy(g => GetLayerPriority(g.Key)))
            {
                var layerFiles = layer.Value.Take(5).ToList();
                var fileLinks = layerFiles.Select(f =>
                {
                    string fileName = Path.GetFileNameWithoutExtension(f.RelativePath);
                    string anchor = GenerateFileAnchor(f.RelativePath);
                    return $"[`{fileName}`](#{anchor})";
                });

                sb.Append($"- **{layer.Key}:** {string.Join(", ", fileLinks)}");

                if (layer.Value.Count > 5)
                {
                    sb.AppendLine($"  _(и ещё {layer.Value.Count - 5})_");
                }
                else
                {
                    sb.AppendLine();
                }
            }
            sb.AppendLine();
        }

        // Самые большие файлы с якорями
        var largestFiles = files
            .OrderByDescending(f => f.SizeInBytes)
            .Take(3)
            .ToList();

        if (largestFiles.Any())
        {
            sb.AppendLine("**Самые большие файлы:**");
            foreach (var file in largestFiles)
            {
                long sizeKB = file.SizeInBytes / 1024;
                string anchor = GenerateFileAnchor(file.RelativePath);
                sb.AppendLine($"- [`{file.RelativePath}`](#{anchor}) (~{sizeKB} KB)");
            }
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine();

        return sb.ToString();
    }


   private static string GenerateFileAnchor(string relativePath)
{
    string text = relativePath;
    // Заменяем оба разделителя на пустоту
    text = text.Replace("\\", "");
    text = text.Replace("/", "");
    text = text.Replace(".", "");
    text = text.ToLowerInvariant();
    // Префикс от "## 📄 `" – GitHub генерирует якорь как "-" + lowercased text без спецсимволов
    return "-" + text;
}




    private static List<FileMetadata> FindEntryPoints(List<FileMetadata> files, List<string> entryPointNames)
    {
        var result = new List<FileMetadata>();

        foreach (var entryPointName in entryPointNames)
        {
            var file = files.FirstOrDefault(f =>
                Path.GetFileName(f.FullPath).Equals(entryPointName, StringComparison.OrdinalIgnoreCase));

            if (file != null)
            {
                result.Add(file);
            }
        }

        return result;
    }

    private static Dictionary<string, List<FileMetadata>> GroupByLayer(List<FileMetadata> files)
    {
        var layers = new Dictionary<string, List<FileMetadata>>();

        foreach (var file in files)
        {
            string layer = GetLayer(file.RelativePath);

            if (!layers.ContainsKey(layer))
            {
                layers[layer] = new List<FileMetadata>();
            }

            layers[layer].Add(file);
        }

        return layers;
    }

    private static string GetLayer(string relativePath)
    {
        var pathUpper = relativePath.ToUpper();

        if (pathUpper.Contains("DOMAIN") || pathUpper.Contains("MODELS\\") || pathUpper.Contains("ENTITIES"))
            return "Domain Layer";

        if (pathUpper.Contains("APPLICATION") || pathUpper.Contains("SERVICES"))
            return "Application/Services";

        if (pathUpper.Contains("INFRASTRUCTURE") || pathUpper.Contains("REPOSITORIES") || pathUpper.Contains("DATA"))
            return "Infrastructure";

        if (pathUpper.Contains("FORMS") || pathUpper.Contains("VIEWS") || pathUpper.Contains("UI") || pathUpper.Contains("PAGES"))
            return "Presentation/UI";

        if (pathUpper.Contains("CONTROLLERS") || pathUpper.Contains("API"))
            return "API/Controllers";

        if (pathUpper.Contains("TEST"))
            return "Tests";

        return "Other";
    }

    private static int GetLayerPriority(string layer)
    {
        return layer switch
        {
            "Domain Layer" => 1,
            "Application/Services" => 2,
            "Infrastructure" => 3,
            "API/Controllers" => 4,
            "Presentation/UI" => 5,
            "Tests" => 6,
            _ => 99
        };
    }
}
