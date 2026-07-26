/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
namespace LLMCodeExporter.CLI.UI;
using Infrastructure.Utils;
public static class InputHelper
{
    /// <summary>
    /// Безопасное чтение строки с повторными попытками при пустом вводе.
    /// Возвращает null, если превышено количество попыток и пустой ввод не разрешён.
    /// </summary>
    public static string? ReadLine(string message, int maxAttempts = 3, bool allowEmpty = false)
    {
        Logger.Log($"Запрос ввода: {message}");
        string input = string.Empty;
        int attemptCount = 0;
        while (attemptCount < maxAttempts)
        {
            try
            {
                attemptCount++;
                Console.WriteLine(message);
                Console.Write("📝 > ");
                input = Console.ReadLine() ?? string.Empty;
                Logger.Log($"[Попытка {attemptCount}] Получен ввод: {(string.IsNullOrWhiteSpace(input) ? "<пустой>" : input)}");
                if (string.IsNullOrWhiteSpace(input) && !allowEmpty)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ Ввод не может быть пустым. Попробуйте снова.");
                    Console.ResetColor();
                    Logger.LogWarning($"[Попытка {attemptCount}] Пустой ввод");
                    if (attemptCount < maxAttempts)
                    {
                        Console.WriteLine();
                    }
                }
                else
                {
                    Logger.Log($"[Попытка {attemptCount}] Корректный ввод получен");
                    return input?.Trim() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Произошла ошибка при вводе: {ex.Message}");
                Console.WriteLine("Попробуйте еще раз.");
                Console.ResetColor();
                Logger.LogError($"Ошибка при вводе (попытка {attemptCount})", ex);
            }
        }

        Logger.LogWarning($"Превышено количество попыток ввода ({maxAttempts})");
        return allowEmpty ? string.Empty : null;
    }

    /// <summary>
    /// Чтение пути к директории с валидацией. Возвращает null, если путь не указан или не существует.
    /// </summary>
    public static string? ReadDirectoryPath(string message, int maxAttempts = 3)
    {
        Logger.Log($"Запрос пути к папке: {message}");
        int attemptCount = 0;
        while (attemptCount < maxAttempts)
        {
            attemptCount++;
            Console.WriteLine(message);
            Console.Write("📁 > ");
            string input = Console.ReadLine()?.Trim().Trim('"', '\'', '`') ?? string.Empty;
            Logger.Log($"[Попытка {attemptCount}] Получен путь: {(string.IsNullOrWhiteSpace(input) ? "<пустой>" : input)}");
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠ Путь не может быть пустым!");
                Console.ResetColor();
                Logger.LogWarning($"[Попытка {attemptCount}] Пустой путь");
                if (attemptCount < maxAttempts)
                {
                    Console.WriteLine($"Осталось попыток: {maxAttempts - attemptCount}\n");
                }
                continue;
            }

            if (!Directory.Exists(input))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Папка не найдена: {input}");
                Console.ResetColor();
                Logger.LogWarning($"[Попытка {attemptCount}] Папка не существует: {input}");
                if (File.Exists(input))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("  ℹ️  Вы указали путь к файлу, но нужна папка!");
                    string? suggestedPath = Path.GetDirectoryName(input);
                    if (!string.IsNullOrEmpty(suggestedPath))
                    {
                        Console.WriteLine($"  Попробуйте: {suggestedPath}");
                    }
                    Console.ResetColor();
                }
                if (attemptCount < maxAttempts)
                {
                    Console.WriteLine($"Осталось попыток: {maxAttempts - attemptCount}\n");
                }
                continue;
            }

            try
            {
                Directory.GetFiles(input);
                Logger.Log($"[Попытка {attemptCount}] Корректный путь получен: {input}");
                return input;
            }
            catch (UnauthorizedAccessException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"✗ Нет доступа к папке: {input}");
                Console.WriteLine("  Возможно, требуются права администратора.");
                Console.ResetColor();
                Logger.LogError($"[Попытка {attemptCount}] Нет доступа к папке: {input}");
                if (attemptCount < maxAttempts)
                {
                    Console.WriteLine($"Осталось попыток: {maxAttempts - attemptCount}\n");
                }
            }
        }

        Logger.LogWarning($"Превышено количество попыток ввода пути ({maxAttempts})");
        return null;
    }

    /// <summary>
    /// Безопасное чтение Yes/No с валидацией
    /// </summary>
    public static bool ReadYesNo(string message, bool defaultValue = true)
    {
        Console.Write($"{message} [{(defaultValue ? "Y/n" : "y/N")}]: ");
        try
        {
            var key = Console.ReadKey();
            Console.WriteLine();
            if (key.Key == ConsoleKey.Enter)
            {
                Logger.Log($"Y/N вопрос: '{message}' - использовано значение по умолчанию: {defaultValue}");
                return defaultValue;
            }

            char c = char.ToLower(key.KeyChar);
            bool result = (c == 'y' || c == 'н');
            Logger.Log($"Y/N вопрос: '{message}' - ответ: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка при чтении Y/N: {message}", ex);
            return defaultValue;
        }
    }

    /// <summary>
    /// Чтение числового выбора из диапазона
    /// </summary>
    public static int ReadChoice(string message, int minValue, int maxValue, int defaultValue)
    {
        Console.Write($"{message} [{minValue}-{maxValue}, Enter = {defaultValue}]: ");
        try
        {
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                Logger.Log($"Выбор: '{message}' - использовано значение по умолчанию: {defaultValue}");
                return defaultValue;
            }

            if (int.TryParse(input, out int choice) && choice >= minValue && choice <= maxValue)
            {
                Logger.Log($"Выбор: '{message}' - выбрано: {choice}");
                return choice;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ Некорректный выбор. Использовано значение по умолчанию: {defaultValue}");
            Console.ResetColor();
            Logger.LogWarning($"Выбор: '{message}' - некорректное значение '{input}', использовано {defaultValue}");
            return defaultValue;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Ошибка при чтении выбора: {message}", ex);
            return defaultValue;
        }
    }
}