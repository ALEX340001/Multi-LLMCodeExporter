/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿using LLMCodeExporter.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
namespace LLMCodeExporter.Tests.Unit.Core.Models
{
    [TestClass]
    public class ExportMetadataTests
    {
        [TestMethod]
        public void ToMarkdown_IncludesAllMetadata()
        {
            // Arrange
            var metadata = new ExportMetadata
            {
                ProjectName = "TestProject",
                Mode = ExportMode.Balanced,
                IncludedFiles = 10,
                TotalFiles = 15,
                EstimatedTokens = 5000,
                OriginalEstimatedTokens = 10000,
                GeneratedAt = new DateTime(2024, 1, 1, 12, 0, 0)
            };
            // Act
            string markdown = metadata.ToMarkdown();
            // Assert
            Assert.IsTrue(markdown.Contains("TestProject"));
            Assert.IsTrue(markdown.Contains("Balanced"));
            Assert.IsTrue(markdown.Contains("10"));
            Assert.IsTrue(markdown.Contains("5,000"));
            Assert.IsTrue(markdown.Contains("01.01.2024"));
        }

        [TestMethod]
        public void GetLLMRecommendations_ReturnsCorrectRecommendation()
        {
            var testCases = new[]
            {
                new { Tokens = 5000, ExpectedContains = "всех LLM" },
                new { Tokens = 15000, ExpectedContains = "GPT-4" },
                new { Tokens = 50000, ExpectedContains = "GPT-4 Turbo" },
                new { Tokens = 150000, ExpectedContains = "Claude 3.5" },
                new { Tokens = 500000, ExpectedContains = "Gemini 1.5 Pro" }
            };
            foreach (var test in testCases)
            {
                var metadata = new ExportMetadata { EstimatedTokens = test.Tokens };
                string recommendation = metadata.GetLLMRecommendations();
                Assert.IsTrue(recommendation.Contains(test.ExpectedContains),
                    $"Tokens: {test.Tokens}, Recommendation: {recommendation}");
            }
        }

        [TestMethod]
        public void CompressionRatio_CalculatesCorrectly()
        {
            // Arrange
            var metadata = new ExportMetadata
            {
                EstimatedTokens = 5000,
                OriginalEstimatedTokens = 10000
            };
            // Act
            double ratio = metadata.CompressionRatio;
            // Assert
            Assert.AreEqual(0.5, ratio);
        }

        [TestMethod]
        public void CompressionRatio_ZeroOriginal_ReturnsOne()
        {
            // Arrange
            var metadata = new ExportMetadata
            {
                EstimatedTokens = 5000,
                OriginalEstimatedTokens = 0
            };
            // Act
            double ratio = metadata.CompressionRatio;
            // Assert
            Assert.AreEqual(1.0, ratio);
        }
    }
}