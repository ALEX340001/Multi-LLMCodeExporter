/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using LLMCodeExporter.CLI.UI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
namespace LLMCodeExporter.Tests.Unit.CLI.UI
{
    [TestClass]
    public class InputHelperTests
    {
        private StringWriter? _consoleOutput;
        private StringReader? _consoleInput;
        [TestInitialize]
        public void Setup()
        {
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);
            // _consoleInput будет создаваться в каждом тесте
            Console.WriteLine("=== Начало теста ===");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Console.WriteLine("=== Окончание теста ===");
            Console.SetOut(Console.Out);
            Console.SetIn(Console.In);
            _consoleOutput?.Dispose();
            _consoleInput?.Dispose();
        }

        [TestMethod]
        public void ReadLine_ValidInput_ReturnsInput()
        {
            // Arrange
            _consoleInput = new StringReader("Test Input\n");
            Console.SetIn(_consoleInput);
            Console.WriteLine("Тест: ReadLine_ValidInput - ожидается ввод 'Test Input'");
            // Act
            string result = InputHelper.ReadLine("Введите что-нибудь:")!;
            // Assert
            Assert.AreEqual("Test Input", result);
            Console.WriteLine($"Результат: '{result}' — успешно");
        }

        [TestMethod]
        public void ReadYesNo_DefaultYes_ReturnsTrueOnEnter()
        {
            // Arrange
            _consoleInput = new StringReader("\n");
            Console.SetIn(_consoleInput);
            Console.WriteLine("Тест: ReadYesNo_DefaultYes - ожидается Enter (значение по умолчанию true)");
            // Act
            bool result = InputHelper.ReadYesNo("Продолжить?", defaultValue: true);
            // Assert
            Assert.IsTrue(result);
            Console.WriteLine($"Результат: {result} — успешно");
        }

        [TestMethod]
        public void ReadChoice_ValidChoice_ReturnsChoice()
        {
            // Arrange
            _consoleInput = new StringReader("2\n");
            Console.SetIn(_consoleInput);
            Console.WriteLine("Тест: ReadChoice_ValidChoice - ожидается выбор '2'");
            // Act
            int result = InputHelper.ReadChoice("Выберите:", 1, 5, 1);
            // Assert
            Assert.AreEqual(2, result);
            Console.WriteLine($"Результат: {result} — успешно");
        }

        [TestMethod]
        public void ReadLine_EmptyInput_WithAllowEmpty_ReturnsEmpty()
        {
            // Arrange
            _consoleInput = new StringReader("\n");
            Console.SetIn(_consoleInput);
            Console.WriteLine("Тест: ReadLine_EmptyInput_WithAllowEmpty - ожидается пустая строка (разрешено)");
            // Act
            string result = InputHelper.ReadLine("Введите что-нибудь (может быть пустым):", maxAttempts: 1, allowEmpty: true)!;
            // Assert
            Assert.AreEqual(string.Empty, result);
            Console.WriteLine($"Результат: '{result}' — успешно");
        }

        [TestMethod]
        public void ReadLine_EmptyInput_WithoutAllowEmpty_ReturnsNullAfterAttempts()
        {
            // Arrange
            _consoleInput = new StringReader("\n");
            Console.SetIn(_consoleInput);
            Console.WriteLine("Тест: ReadLine_EmptyInput_WithoutAllowEmpty - ожидается null после превышения попыток");
            // Act
            string? result = InputHelper.ReadLine("Введите что-нибудь (не может быть пустым):", maxAttempts: 2, allowEmpty: false);
            // Assert
            Assert.IsNull(result);
            Console.WriteLine($"Результат: null — успешно");
        }
    }
}