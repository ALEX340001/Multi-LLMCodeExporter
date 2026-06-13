using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LLMCodeExporter.Tests.Unit.Infrastructure.Services
{
    [TestClass]
    public class FileScannerTests
    {
        private string _tempProjectDir;

        [TestInitialize]
        public void TestSetup()
        {
            _tempProjectDir = CreateSimpleTestProject();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                if (Directory.Exists(_tempProjectDir))
                    Directory.Delete(_tempProjectDir, true);
            }
            catch { }
        }

        [TestMethod]
        public void ScanProject_SimpleTest()
        {
            // Arrange
            var scanner = new FileScanner();
            var settings = new ExportSettings();

            // Act
            var result = scanner.ScanProject(_tempProjectDir, settings);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Files.Count >= 1);
            Assert.IsFalse(string.IsNullOrEmpty(result.ProjectName));
        }

        private string CreateSimpleTestProject()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"ScanTest_{Guid.NewGuid():N8}");
            Directory.CreateDirectory(tempPath);

            string programPath = Path.Combine(tempPath, "Program.cs");
            File.WriteAllText(programPath, @"
namespace TestProject 
{
    public class Program 
    {
        public static void Main() { }
    }
}");

            return tempPath;
        }
    }
}
