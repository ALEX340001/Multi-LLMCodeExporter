using System.Reflection;

namespace LLMCodeExporter.Core.Interfaces;

using Models;

public interface IFileScanner
{
    ProjectInfo ScanProject(string projectPath, ExportSettings settings);
}
