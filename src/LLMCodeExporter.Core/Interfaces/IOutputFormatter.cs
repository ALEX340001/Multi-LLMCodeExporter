namespace LLMCodeExporter.Core.Interfaces;

using Models;

public interface IOutputFormatter
{
    string FormatHeader(ProjectInfo projectInfo);
    string FormatFile(FileMetadata file, string content, string tag = "");
    string FormatFooter(ProjectInfo projectInfo);
}
