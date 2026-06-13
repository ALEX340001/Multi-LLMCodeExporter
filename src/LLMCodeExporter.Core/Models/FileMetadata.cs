namespace LLMCodeExporter.Core.Models;

public class FileMetadata
{
    public string FullPath { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }

    // ИЗМЕНЕНИЕ: добавляем set для свойства EstimatedTokens
    public int EstimatedTokens { get; set; }
}