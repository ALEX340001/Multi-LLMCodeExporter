namespace LLMCodeExporter.Core.Models;

public class ProjectInfo
{
    public string ProjectPath { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public List<FileMetadata> Files { get; set; } = new();
    public long TotalCharacters { get; set; }
    public long EstimatedTokens => TotalCharacters / 4;
    public int TotalFiles => Files.Count;

    // Новые свойства для v2.0
    public List<FileMetadata> ExcludedFiles { get; set; } = new();
    public int TotalScannedFiles => Files.Count + ExcludedFiles.Count;
    public ExportMetadata Metadata { get; set; } = new();
}
