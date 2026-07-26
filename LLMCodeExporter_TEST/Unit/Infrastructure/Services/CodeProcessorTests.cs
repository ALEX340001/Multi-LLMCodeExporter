/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using LLMCodeExporter.Infrastructure.Services;
using LLMCodeExporter.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace LLMCodeExporter.Tests.Unit.Infrastructure.Services
{
    [TestClass]
    public class CodeProcessorTests
    {
        private CodeProcessor? _processor;
        [TestInitialize]
        public void Setup()
        {
            _processor = new CodeProcessor();
        }

        [TestMethod]
        public void ProcessCode_RemoveComments_RemovesAllComments()
        {
            string code = @"
                // Single line comment
                public class Test {
                    /* Multi-line
                       comment */
                    public void Method() {
                        // Another comment
                        var x = 1;
                    }
                }";
            var settings = new ExportSettings
            {
                RemoveComments = true,
                RemoveEmptyLines = false
            };
            string result = _processor!.ProcessCode(code, settings);
            Assert.IsFalse(result.Contains("// Single line comment"));
            Assert.IsFalse(result.Contains("/* Multi-line"));
            Assert.IsFalse(result.Contains("// Another comment"));
            Assert.IsTrue(result.Contains("public class Test"));
            Assert.IsTrue(result.Contains("public void Method()"));
        }

        [TestMethod]
        public void ProcessCode_KeepComments_PreservesComments()
        {
            string code = "// This is a comment\npublic class Test {}";
            var settings = new ExportSettings
            {
                RemoveComments = false,
                RemoveEmptyLines = false
            };
            string result = _processor!.ProcessCode(code, settings);
            Assert.IsTrue(result.Contains("// This is a comment"));
        }

        [TestMethod]
        public void RemoveEmptyLines_RemovesExcessiveEmptyLines()
        {
            string code = "Line 1\n\n\nLine 2\n\nLine 3";
            string result = CodeProcessor.RemoveEmptyLines(code);
            var lines = result.Split('\n');
            int emptyLines = lines.Count(string.IsNullOrWhiteSpace);
            Assert.IsTrue(emptyLines <= 2);
        }

        [TestMethod]
        public void RemoveComments_PreservesXmlDocumentation()
        {
            string code = @"/// <summary>XML documentation</summary>
                    // Regular comment
                    public class Test { public void Method() { } }";
            var settings = new ExportSettings { RemoveComments = true };
            string result = _processor!.ProcessCode(code, settings);
            Assert.IsFalse(result.Contains("// Regular comment"));
            Assert.IsTrue(result.Contains("/// <summary>"));
            Assert.IsTrue(result.Contains("public class Test"));
        }
    }
}