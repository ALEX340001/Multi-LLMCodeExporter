/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * SPDX-License-Identifier: MPL-2.0
 */
using System;
namespace LLMCodeExporter.CLI.UI;
/// <summary>
/// Вывод справки по использованию приложения
/// </summary>
public static class HelpPrinter
{
    public static void Print()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  📦 LLM Code Exporter vBeta release v0.4                       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("ИСПОЛЬЗОВАНИЕ:");
        Console.WriteLine("  LLMCodeExporter.exe <путь-к-проекту> [опции]");
        Console.WriteLine();
        Console.WriteLine("РЕЖИМЫ ЭКСПОРТА:");
        Console.WriteLine("  --mode=compact      Только структура и сигнатуры (~30% размера, ~30K токенов)");
        Console.WriteLine("  --mode=balanced     Оптимизация больших методов (~50-70%, ~50-70K токенов) [по умолчанию]");
        Console.WriteLine("  --mode=full         Весь код без изменений (100%, может быть 100K+ токенов)");
        Console.WriteLine();
        Console.WriteLine("ФИЛЬТРАЦИЯ:");
        Console.WriteLine("  --exclude=pattern   Исключить файлы по паттерну (можно указать несколько раз)");
        Console.WriteLine("                      Примеры: *.Designer.cs, Forms, *.g.cs, UI");
        Console.WriteLine("  --include=pattern   Включить ТОЛЬКО файлы по паттерну");
        Console.WriteLine("                      Примеры: Domain, Services, Core");
        Console.WriteLine();
        Console.WriteLine("PRESET'Ы:");
        Console.WriteLine("  --backend-only      Исключить UI файлы (Forms, Views, Designer, xaml, razor)");
        Console.WriteLine("  --domain-services   Только бизнес-логика (Domain, Services, Application, Core)");
        Console.WriteLine("  --compact-aggressive Максимальное сжатие (compact + удаление комментариев)");
        Console.WriteLine("  --web-app           Веб-приложение (JS, CSS, HTML файлы)");
        Console.WriteLine("  --hybrid            Гибридный режим (требует --backend и --frontend или автоопределение)");
        Console.WriteLine();
        Console.WriteLine("ГИБРИДНЫЙ РЕЖИМ:");
        Console.WriteLine("  --backend=<lang>    Язык бекенда (CSharp, Python, JavaScript, TypeScript, Java, Go, Ruby, PHP)");
        Console.WriteLine("  --frontend=<lang>   Язык фронтенда (те же варианты)");
        Console.WriteLine("  Пример: --hybrid --backend=CSharp --frontend=JavaScript");
        Console.WriteLine("  Пример: --backend=Python --frontend=TypeScript (автоматически включает --hybrid)");
        Console.WriteLine();
        Console.WriteLine("ДОПОЛНИТЕЛЬНЫЕ ОПЦИИ:");
        Console.WriteLine("  --format=markdown   Формат вывода: markdown [по умолчанию] или text");
        Console.WriteLine("  --no-comments       Удалить комментарии из кода");
        Console.WriteLine("  --keep-empty-lines  Сохранить пустые строки (по умолчанию удаляются)");
        Console.WriteLine("  --collapse-threshold N  Порог строк для сворачивания методов [50]");
        Console.WriteLine("  --output=path       Путь для сохранения результата");
        Console.WriteLine("  --format=md+json    Markdown с встроенным JSON-блоком (полный отчёт)");
        Console.WriteLine("  --help, -h, /?      Показать эту справку");
        Console.WriteLine();
        Console.WriteLine("ПРИМЕРЫ:");
        Console.WriteLine();
        Console.WriteLine("  1. Интерактивный режим (по умолчанию):");
        Console.WriteLine("     LLMCodeExporter.exe");
        Console.WriteLine();
        Console.WriteLine("  2. Drag & Drop:");
        Console.WriteLine("     Перетащите папку с проектом на .exe файл");
        Console.WriteLine();
        Console.WriteLine("  3. Compact режим для быстрого обзора:");
        Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyApp\" --mode=compact");
        Console.WriteLine();
        Console.WriteLine("  4. Backend анализ (без UI):");
        Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyApp\" --backend-only");
        Console.WriteLine();
        Console.WriteLine("  5. Только Domain и Services:");
        Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyApp\" --include=Domain --include=Services");
        Console.WriteLine();
        Console.WriteLine("  6. Исключить Designer файлы и Forms:");
        Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyApp\" --exclude=*.Designer.cs --exclude=Forms");
        Console.WriteLine();
        Console.WriteLine("  7. Максимальное сжатие для очень большого проекта:");
        Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\LargeApp\" --compact-aggressive");
        Console.WriteLine();
        Console.WriteLine("  8. Веб-приложение (JS, CSS, HTML):");
        Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyWebApp\" --web-app");
        Console.WriteLine();
        Console.WriteLine("  9. Гибридный проект с выбором языков:");
        Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyHybrid\" --hybrid --backend=Python --frontend=TypeScript");
        Console.WriteLine();
        Console.WriteLine(" 10. Гибридный проект с автоопределением языков:");
        Console.WriteLine("     LLMCodeExporter.exe \"C:\\Projects\\MyHybrid\" --hybrid");
        Console.WriteLine();
        Console.WriteLine("РЕКОМЕНДАЦИИ ПО РАЗМЕРУ:");
        Console.WriteLine("  • < 8K токенов    - Full режим, подходит для всех LLM");
        Console.WriteLine("  • 8-32K токенов   - Balanced, подходит для GPT-4, Claude 3");
        Console.WriteLine("  • 32-100K токенов - Balanced + фильтрация, GPT-4 Turbo, Claude 3.5");
        Console.WriteLine("  • > 100K токенов  - Compact или разбиение на модули");
        Console.WriteLine();
    }
}