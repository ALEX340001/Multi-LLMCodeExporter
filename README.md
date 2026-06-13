# Multi‑LLMCodeExporter

Экспорт исходного кода в компактном виде для передачи в большие языковые модели (ChatGPT, Claude, Gemini и другие). Инструмент анализирует структуру проекта, удаляет лишние элементы, оценивает количество токенов и сохраняет результат в файл (Markdown или Plain Text).

## Возможности

- Поддержка C#, Python, JavaScript/TypeScript, веб-приложений (HTML, CSS).
- Три режима экспорта:
  - `Compact` – только сигнатуры классов и методов (примерно 30% исходного объёма).
  - `Balanced` – бизнес-логика, длинные методы сворачиваются (50–70% объёма).
  - `Full` – весь код без изменений.
- Гибкая фильтрация файлов и папок (исключение/включение по паттернам).
- Интерактивный режим настройки и CLI для автоматизации.
- Оценка количества токенов, степени сжатия, рекомендации по модели LLM.
- Генерация графа зависимостей (Mermaid) и навигационных ссылок.

## Требования

- .NET 8.0 SDK или новее (скачать с [dotnet.microsoft.com](https://dotnet.microsoft.com/download))

## Сборка и запуск

### Из исходного кода

git clone https://github.com/ALEX340001/Multi-LLMCodeExporter.git
cd Multi-LLMCodeExporter
dotnet build
Интерактивный режим
bash
dotnet run --project src/LLMCodeExporter.CLI
Приложение предложит выбрать тип проекта, режим экспорта, фильтры и формат вывода.

CLI режим (без интерактивного меню)
bash
dotnet run --project src/LLMCodeExporter.CLI -- /путь/к/проекту [опции]
Основные опции
Опция	Описание
--mode=compact	Только сигнатуры
--mode=balanced	Сворачивание длинных методов (по умолчанию)
--mode=full	Весь код без изменений
--format=markdown	Вывод в Markdown (по умолчанию)
--format=text	Вывод в Plain Text
--exclude=pattern	Исключить файлы по паттерну (можно несколько)
--include=pattern	Включить только файлы по паттерну
--backend-only	Исключить UI файлы (Forms, Views, Designer)
--domain-services	Только Domain + Services
--compact-aggressive	Максимальное сжатие (удаление комментариев и пустых строк)
--web-app	Пресет для веб-приложений (JS, CSS, HTML)
--no-comments	Удалить комментарии из кода
--keep-empty-lines	Сохранить пустые строки
--output=/path	Директория для сохранения результата
Примеры

# Экспорт C# проекта в компактном режиме
dotnet run --project src/LLMCodeExporter.CLI -- ~/MyProject --mode=compact

# Только бизнес-логика (исключая UI)
dotnet run --project src/LLMCodeExporter.CLI -- ~/MyProject --domain-services

# Веб-приложение с максимальным сжатием
dotnet run --project src/LLMCodeExporter.CLI -- ~/MyWebApp --web-app --compact-aggressive

# Исключить тесты и файлы дизайнера
dotnet run --project src/LLMCodeExporter.CLI -- ~/MyProject --exclude=*.Designer.cs --exclude=Tests
Результат экспорта
Сгенерированный файл содержит:

Заголовок с метаданными (режим, дата, количество файлов, оценка токенов).

Рекомендации по LLM в зависимости от размера.

Быстрые ссылки на ключевые файлы.

Архитектурный обзор и граф зависимостей (Mermaid).

Структуру проекта в виде дерева.

Исходный код выбранных файлов.

Итоговую статистику.

Лицензия
Проект распространяется под лицензией Mozilla Public License 2.0. Подробности в файле LICENSE.

Контакты и вклад
Сообщения об ошибках и предложения принимаются через Issues. Pull Requests приветствуются.

Сохраните файл, затем выполните:

git add README.md
git commit -m "Добавлен README с описанием и инструкцией"
git push origin main