/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;
using System.Collections.Generic;
namespace LLMCodeExporter.Tests.Integration
{
    [TestClass]
    public class ExportServiceIntegrationTests
    {
        private string _testProjectPath = string.Empty;
        private string _outputDir = string.Empty;
        [TestInitialize]
        public void Setup()
        {
            _testProjectPath = CreateTempTestProject();
            _outputDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_outputDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            CleanupTempDirectory(_testProjectPath);
            CleanupTempDirectory(_outputDir);
        }

        [TestMethod]
        public void ExportProject_SimpleProject_CreatesOutputFile()
        {
            var scanner = new FileScanner();
            var processor = new CodeProcessor();
            var service = new ExportService(processor);
            var settings = new ExportSettings
            {
                Mode = ExportMode.Balanced,
                Format = ExportFormat.Markdown,
                OutputDirectory = _outputDir,
                FilterBuildFolders = true
            };
            var projectInfo = scanner.ScanProject(_testProjectPath, settings);
            var result = service.ExportProject(projectInfo, settings);
            Assert.IsTrue(result.Success);
            Assert.IsTrue(File.Exists(result.OutputFilePath));
            Assert.IsTrue(new FileInfo(result.OutputFilePath).Length > 0);
            string content = File.ReadAllText(result.OutputFilePath);
            Assert.IsTrue(content.Contains("# 🚀 Code Export для LLM"));
            Assert.IsTrue(content.Contains("TestProject"));
        }

        [TestMethod]
        public void ExportProject_DifferentModes_ProducesDifferentSizes()
        {
            var scanner = new FileScanner();
            var processor = new CodeProcessor();
            var service = new ExportService(processor);
            var modes = new[] { ExportMode.Compact, ExportMode.Balanced, ExportMode.Full };
            var fileSizes = new Dictionary<ExportMode, long>();
            foreach (var mode in modes)
            {
                var settings = new ExportSettings
                {
                    Mode = mode,
                    Format = ExportFormat.Markdown,
                    OutputDirectory = _outputDir
                };
                var projectInfo = scanner.ScanProject(_testProjectPath, settings);
                var result = service.ExportProject(projectInfo, settings);
                Assert.IsTrue(result.Success);
                fileSizes[mode] = new FileInfo(result.OutputFilePath).Length;
            }
            Assert.IsTrue(fileSizes[ExportMode.Compact] < fileSizes[ExportMode.Balanced]);
            Assert.IsTrue(fileSizes[ExportMode.Balanced] < fileSizes[ExportMode.Full]);
        }

        [TestMethod]
        public void ExportProject_WithFilters_RespectsPatterns()
        {
            var scanner = new FileScanner();
            var processor = new CodeProcessor();
            var service = new ExportService(processor);
            var settings = new ExportSettings
            {
                Mode = ExportMode.Balanced,
                Format = ExportFormat.Markdown,
                OutputDirectory = _outputDir,
                IncludeOnlyPatterns = new List<string> { "Domain" }
            };
            var projectInfo = scanner.ScanProject(_testProjectPath, settings);
            var result = service.ExportProject(projectInfo, settings);
            Assert.IsTrue(result.Success);
            string content = File.ReadAllText(result.OutputFilePath);
            Assert.IsTrue(content.Contains("Domain") && content.Contains("User.cs"),
                "Должен содержать файл Domain/User.cs");
            Assert.IsFalse(content.Contains("UserService.cs"),
                "Не должен содержать файл UserService.cs при фильтрации только Domain");
        }

        [TestMethod]
        public void ExportProject_HybridProject_ExportsCorrectly()
        {
            string hybridProjectPath = CreateTempHybridProject();
            try
            {
                var scanner = new FileScanner();
                var processor = new CodeProcessor();
                var service = new ExportService(processor);
                var settings = new ExportSettings();
                settings.ApplyHybridPreset();
                settings.OutputDirectory = _outputDir;
                var projectInfo = scanner.ScanProject(hybridProjectPath, settings);
                var result = service.ExportProject(projectInfo, settings);
                Assert.IsTrue(result.Success, "Экспорт должен завершиться успешно");
                Assert.IsTrue(File.Exists(result.OutputFilePath), "Выходной файл должен быть создан");
                string content = File.ReadAllText(result.OutputFilePath);
                Assert.IsTrue(content.Contains("Startup.cs"), "Должен присутствовать Startup.cs");
                Assert.IsTrue(content.Contains("HomeController.cs"), "Должен присутствовать HomeController.cs");
                Assert.IsTrue(content.Contains("index.html"), "Должен присутствовать index.html");
                Assert.IsTrue(content.Contains("app.js"), "Должен присутствовать app.js");
                Assert.IsFalse(content.Contains("app.dll"), "Бинарный файл не должен попасть в экспорт");
                Assert.IsTrue(content.Contains("package.json"), "Должен быть включён package.json");
            }
            finally
            {
                CleanupTempDirectory(hybridProjectPath);
            }
        }

        // ===== Вспомогательные методы (перенесены из TestHelpers) =====
        private static string CreateTempTestProject(string projectName = "TestProject")
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"{projectName}_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            string programPath = Path.Combine(tempDir, "Program.cs");
            File.WriteAllText(programPath, "namespace TestProject { public class Program { public static void Main() { } } }");
            return tempDir;
        }

        private static string CreateTempHybridProject()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempPath);
            File.WriteAllText(Path.Combine(tempPath, "Startup.cs"), "public class Startup { }");
            return tempPath;
        }

        private static void CleanupTempDirectory(string path)
        {
            try { if (Directory.Exists(path)) Directory.Delete(path, true); } catch { }
        }
    }
}