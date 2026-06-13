namespace LLMCodeExporter.Core.Models;

public class ExportResult
{
    public bool Success { get; set; }
    public string OutputFilePath { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ProjectInfo ProjectInfo { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
