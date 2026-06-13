using LLMCodeExporter.Infrastructure.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LLMCodeExporter.Tests.Unit.Infrastructure.Utils

{
    [TestClass]
    public class PatternMatcherTests
    {
        [TestMethod]
        public void MatchesPattern_WithStarExtension_ReturnsTrue()
        {
            // Arrange
            string filePath = @"src\Services\UserService.cs";
            string pattern = "*.cs";

            // Act
            bool result = PatternMatcher.MatchesPattern(filePath, pattern);

            // Assert
            Assert.IsTrue(result, $"Файл {filePath} должен соответствовать паттерну {pattern}");
        }

        [TestMethod]
        public void MatchesPattern_WithExcludePattern_ReturnsFalse()
        {
            // Arrange
            string filePath = @"src\UI\Form1.Designer.cs";
            string pattern = "*.Designer.cs";

            // Act
            bool result = PatternMatcher.MatchesPattern(filePath, pattern);

            // Assert
            Assert.IsTrue(result, $"Файл {filePath} должен соответствовать паттерну исключения {pattern}");
        }

        [TestMethod]
        public void FilterByPatterns_WithMultiplePatterns_CorrectlyFilters()
        {
            // Arrange
            var files = new List<string>
            {
                @"src\UI\Form1.cs",
                @"src\UI\Form1.Designer.cs",
                @"src\Domain\User.cs",
                @"src\Tests\UserTests.cs"
            };

            var excludePatterns = new List<string> { "*.Designer.cs", "Tests" };

            // Act
            (List<string> included, List<string> excluded) = PatternMatcher.FilterByPatterns(
                files,
                f => f,
                new List<string>(), // include all
                excludePatterns
            );

            // Assert
            Assert.AreEqual(2, included.Count, "Должно быть 2 включенных файла");
            Assert.AreEqual(2, excluded.Count, "Должно быть 2 исключенных файла");
            Assert.IsTrue(included.Contains(@"src\UI\Form1.cs"));
            Assert.IsTrue(included.Contains(@"src\Domain\User.cs"));
        }
    }
}