/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
﻿using LLMCodeExporter.Infrastructure.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace LLMCodeExporter.Tests.Unit.Infrastructure.Utils
{
    [TestClass]
    public class AnchorGeneratorTests
    {
        [TestMethod]
        public void GenerateAnchor_WithSimpleText_ReturnsValidAnchor()
        {
            // Arrange
            string text = "Test File.cs";
            // Act
            string anchor = AnchorGenerator.GenerateAnchor(text);
            // Assert
            Assert.AreEqual("test-filecs", anchor);
            Assert.IsFalse(anchor.Contains(" "));
            Assert.IsFalse(anchor.Contains("."));
        }

        [TestMethod]
        public void GenerateAnchor_WithSpecialCharacters_RemovesThem()
        {
            // Arrange
            string text = "## 📄 `src\\Models\\User.cs`";
            // Act
            string anchor = AnchorGenerator.GenerateAnchor(text);
            // Assert
            Assert.AreEqual("srcmodelsusercs", anchor);
        }

        [TestMethod]
        public void CreateFileLink_CreatesValidMarkdownLink()
        {
            // Arrange
            string relativePath = @"src\Models\User.cs";
            // Act
            string link = AnchorGenerator.CreateFileLink(relativePath, "User Model");
            // Assert
            Assert.IsTrue(link.Contains("[User Model]"));
            Assert.IsTrue(link.Contains("(#"));
        }
    }
}