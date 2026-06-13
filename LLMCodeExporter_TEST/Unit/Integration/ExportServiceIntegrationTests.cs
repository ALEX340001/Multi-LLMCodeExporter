/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿using LLMCodeExporter.Core.Models;
using LLMCodeExporter.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System;

namespace LLMCodeExporter.Tests.Integration
{
    [TestClass]
    public class ExportServiceIntegrationTests
    {
        private string _testProjectPath;
        private string _outputDir;

        [TestInitialize]
        public void Setup()
        {
            _testProjectPath = CreateTestProject();
            _outputDir = Path.Combine(Path.GetTempPath(), $"ExportTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_outputDir);
        }

        [TestMethod]
        public void ExportProject_SimpleProject_CreatesOutputFile()
        {
            // Arrange
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

            // Act
            var projectInfo = scanner.ScanProject(_testProjectPath, settings);
            var result = service.ExportProject(projectInfo, settings);

            // Assert
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
            // Arrange
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

                // Act
                var projectInfo = scanner.ScanProject(_testProjectPath, settings);
                var result = service.ExportProject(projectInfo, settings);

                // Assert
                Assert.IsTrue(result.Success);
                fileSizes[mode] = new FileInfo(result.OutputFilePath).Length;
            }

            // Размеры должны отличаться
            Assert.IsTrue(fileSizes[ExportMode.Compact] < fileSizes[ExportMode.Balanced]);
            Assert.IsTrue(fileSizes[ExportMode.Balanced] < fileSizes[ExportMode.Full]);

            Console.WriteLine($"Compact: {fileSizes[ExportMode.Compact]} bytes");
            Console.WriteLine($"Balanced: {fileSizes[ExportMode.Balanced]} bytes");
            Console.WriteLine($"Full: {fileSizes[ExportMode.Full]} bytes");
        }

        [TestMethod]
        public void ExportProject_WithFilters_RespectsPatterns()
        {
            // Arrange
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

            // Act
            var projectInfo = scanner.ScanProject(_testProjectPath, settings);
            var result = service.ExportProject(projectInfo, settings);

            // Assert
            Assert.IsTrue(result.Success);

            string content = File.ReadAllText(result.OutputFilePath);

            // Проверяем наличие файла из Domain папки (путь может содержать \ или / в зависимости от ОС)
            Assert.IsTrue(
                content.Contains("Domain") && 
                content.Contains("User.cs"), 
                "Должен содержать файл Domain/User.cs");
            
            // Проверяем отсутствие файлов из других папок
            Assert.IsFalse(
                content.Contains("UserService.cs"), 
                "Не должен содержать файл UserService.cs при фильтрации только Domain");
        }

        private string CreateTestProject()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"TestProject_{Guid.NewGuid()}");

            // Структура тестового проекта – используем Path.Combine для кроссплатформенности
            var files = new Dictionary<string, string>
            {
                { Path.Combine("Program.cs"), "namespace TestProject;\npublic class Program {\n    static void Main() { }\n}" },
                { Path.Combine("Domain", "User.cs"), "namespace TestProject.Domain;\npublic class User {\n    public string Name { get; set; }\n    public int Age { get; set; }\n}" },
                { Path.Combine("Services", "UserService.cs"), "namespace TestProject.Services;\npublic class UserService {\n    public User GetUser(int id) { return new User(); }\n    public void SaveUser(User user) { }\n}" },
                { Path.Combine("UI", "Form1.cs"), "namespace TestProject.UI;\npublic partial class Form1 { }" },
                { Path.Combine("UI", "Form1.Designer.cs"), "namespace TestProject.UI;\npublic partial class Form1 { }" },
                { Path.Combine("Tests", "UserTests.cs"), "using Microsoft.VisualStudio.TestTools.UnitTesting;\n[TestClass]\npublic class UserTests {\n    [TestMethod]\n    public void Test1() { }\n}" },
                { Path.Combine("bin", "Debug", "TestProject.dll"), "binary content" },
                { Path.Combine("obj", "Debug", "TestProject.csproj"), "project file" }
            };

            foreach (var file in files)
            {
                string fullPath = Path.Combine(tempPath, file.Key);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllText(fullPath, file.Value);
            }

            return tempPath;
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (Directory.Exists(_testProjectPath))
                    Directory.Delete(_testProjectPath, true);

                if (Directory.Exists(_outputDir))
                    Directory.Delete(_outputDir, true);
            }
            catch { /* Игнорируем ошибки очистки */ }
        }
    }
}