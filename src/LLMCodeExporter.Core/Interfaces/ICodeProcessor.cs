using LLMCodeExporter.Core.Models;

namespace LLMCodeExporter.Core.Interfaces;

public interface ICodeProcessor
{
    string ProcessCode(string code, ExportSettings settings);
}
