using LLMCodeExporter.CLI.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LLMCodeExporter.Tests.Unit.CLI.UI
{
    [TestClass]
    public class InputHelperTests
    {
        private StringWriter _consoleOutput;
        private StringReader _consoleInput;

        
        [TestInitialize]
        public void Setup() 
        {
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);
            // Убрать _consoleInput из Setup - создавать в каждом тесте
        }

        [TestMethod]
        public void ReadLine_ValidInput_ReturnsInput()
        {
            // Arrange
            _consoleInput = new StringReader("Test Input\n");
            Console.SetIn(_consoleInput);

            // Act
            string result = InputHelper.ReadLine("Введите что-нибудь:");

            // Assert
            Assert.AreEqual("Test Input", result);
        }

        [TestMethod]
        public void ReadYesNo_DefaultYes_ReturnsTrueOnEnter()
        {
            // Arrange
            _consoleInput = new StringReader("\n"); // Нажатие Enter
            Console.SetIn(_consoleInput);

            // Act
            bool result = InputHelper.ReadYesNo("Продолжить?", defaultValue: true);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ReadChoice_ValidChoice_ReturnsChoice()
        {
            // Arrange
            _consoleInput = new StringReader("2\n");
            Console.SetIn(_consoleInput);

            // Act
            int result = InputHelper.ReadChoice("Выберите:", 1, 5, 1);

            // Assert
            Assert.AreEqual(2, result);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Console.SetOut(Console.Out);
            Console.SetIn(Console.In);
            _consoleOutput?.Dispose();
        }
    }
}