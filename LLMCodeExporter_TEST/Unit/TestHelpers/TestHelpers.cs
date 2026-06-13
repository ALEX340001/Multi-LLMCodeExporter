using System;
using System.IO;

namespace LLMCodeExporter.Tests.TestHelpers
{
    public static class TestHelpers
    {
        /// <summary>
        /// Создаёт временный веб-проект для тестов.
        /// </summary>
        public static string CreateTempWebProject(string projectName = "WebProject")
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"{projectName}_{Guid.NewGuid()}");

            // Базовая структура веб-проекта – Path.Combine для кроссплатформенности
            var files = new[]
            {
                new { Path = Path.Combine("", "index.html"), Content = CreateIndexHtml() },
                new { Path = Path.Combine("styles", "main.css"), Content = CreateMainCss() },
                new { Path = Path.Combine("scripts", "app.js"), Content = CreateAppJs() },
                new { Path = Path.Combine("scripts", "utils.js"), Content = CreateUtilsJs() },
                new { Path = Path.Combine("assets", "logo.svg"), Content = "<svg>logo</svg>" },
                new { Path = Path.Combine("", "package.json"), Content = CreatePackageJson() },
                new { Path = Path.Combine("", "README.md"), Content = "# Web Project\n\nВеб-приложение" }
            };

            foreach (var file in files)
            {
                string fullPath = Path.Combine(tempDir, file.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllText(fullPath, file.Content);
            }

            return tempDir;
        }

        private static string CreateIndexHtml() => @"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Web Application</title>
    <link rel='stylesheet' href='styles/main.css'>
</head>
<body>
    <div id='app'>
        <h1>Web Application</h1>
        <button onclick='greet()'>Click me</button>
    </div>
    <script src='scripts/utils.js'></script>
    <script src='scripts/app.js'></script>
</body>
</html>";

        private static string CreateMainCss() => @"/* Основные стили */
body {
    font-family: Arial, sans-serif;
    margin: 0;
    padding: 20px;
    background-color: #f0f0f0;
}

#app {
    max-width: 800px;
    margin: 0 auto;
    background: white;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}

button {
    padding: 10px 20px;
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
}

button:hover {
    background-color: #0056b3;
}";

        private static string CreateAppJs() => @"// Основное приложение
function greet() {
    alert('Hello from Web Application!');
    console.log('Button clicked');
}

// Инициализация приложения
document.addEventListener('DOMContentLoaded', function() {
    console.log('Web application loaded');
    
    // Пример использования утилит
    const result = addNumbers(5, 10);
    console.log('Result:', result);
});";

        private static string CreateUtilsJs() => @"// Утилитные функции
function addNumbers(a, b) {
    return a + b;
}

function formatDate(date) {
    return date.toLocaleDateString();
}

// Экспорт функций для использования в других модулях
window.utils = {
    addNumbers,
    formatDate
};";

        private static string CreatePackageJson() => @"{
    ""name"": ""web-application"",
    ""version"": ""1.0.0"",
    ""description"": ""Sample web application"",
    ""main"": ""scripts/app.js"",
    ""scripts"": {
        ""start"": ""live-server"",
        ""build"": ""webpack --mode production"",
        ""dev"": ""webpack --mode development""
    },
    ""dependencies"": {
        ""lodash"": ""^4.17.21""
    },
    ""devDependencies"": {
        ""webpack"": ""^5.75.0"",
        ""webpack-cli"": ""^5.0.1"",
        ""live-server"": ""^1.2.2""
    }
}";

        /// <summary>
        /// Создаёт временный C# проект для тестов.
        /// </summary>
        public static string CreateTempTestProject(string projectName = "TestProject")
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"{projectName}_{Guid.NewGuid()}");

            // Базовая структура C# проекта – Path.Combine для кроссплатформенности
            var files = new[]
            {
                new { Path = Path.Combine("", "Program.cs"), Content = CreateProgramCs() },
                new { Path = Path.Combine("Models", "User.cs"), Content = CreateUserModel() },
                new { Path = Path.Combine("Services", "UserService.cs"), Content = CreateUserService() },
                new { Path = Path.Combine("Tests", "UserTests.cs"), Content = CreateTestClass() },
                new { Path = Path.Combine("UI", "Form1.Designer.cs"), Content = CreateDesignerFile() }
            };

            foreach (var file in files)
            {
                string fullPath = Path.Combine(tempDir, file.Path);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllText(fullPath, file.Content);
            }

            return tempDir;
        }

        private static string CreateProgramCs() => @"
using System;

namespace TestProject
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
        
        public static int Add(int a, int b) => a + b;
    }
}";

        private static string CreateUserModel() => @"
using System;

namespace TestProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        
        public bool IsValid() => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Email);
    }
}";

        private static string CreateUserService() => @"
using TestProject.Models;
using System.Collections.Generic;

namespace TestProject.Services
{
    public interface IUserService
    {
        User GetUser(int id);
        List<User> GetAllUsers();
        void SaveUser(User user);
        void DeleteUser(int id);
    }
    
    public class UserService : IUserService
    {
        private readonly List<User> _users = new();
        
        public User GetUser(int id) => _users.Find(u => u.Id == id);
        
        public List<User> GetAllUsers() => new List<User>(_users);
        
        public void SaveUser(User user)
        {
            var existing = _users.FindIndex(u => u.Id == user.Id);
            if (existing >= 0)
                _users[existing] = user;
            else
                _users.Add(user);
        }
        
        public void DeleteUser(int id) => _users.RemoveAll(u => u.Id == id);
        
        public bool ValidateUser(User user) => user?.IsValid() ?? false;
    }
}";

        private static string CreateTestClass() => @"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestProject.Models;
using TestProject.Services;

namespace TestProject.Tests
{
    [TestClass]
    public class UserTests
    {
        [TestMethod]
        public void User_IsValid_ReturnsTrueForValidUser()
        {
            var user = new User { Name = ""Test"", Email = ""test@example.com"" };
            Assert.IsTrue(user.IsValid());
        }
    }
}";

        private static string CreateDesignerFile() => @"
// This is an auto-generated file
// Manual changes will be overwritten

namespace TestProject.UI
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }
    }
}";

        public static void CleanupTempDirectory(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, true);
            }
            catch { /* Игнорируем ошибки */ }
        }
    }
}