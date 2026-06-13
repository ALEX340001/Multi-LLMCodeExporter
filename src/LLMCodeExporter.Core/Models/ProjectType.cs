using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LLMCodeExporter.Core.Models
{
    public enum ProjectType
    {
        AutoDetect,
        CSharp,
        Python,
        JavaScript,
        TypeScript,
        Java,
        Go,
        Generic,
        WebApp
    }
}