// js/modules/config.js

export const CLI_HELP = [
    { key: '--mode=compact|balanced|full', desc: 'Режим экспорта: структура (30%), сбалансированный (60%), полный (100%)' },
    { key: '--format=markdown|plain|json|md+json', desc: 'Формат выходного файла' },
    { key: '--exclude=pattern', desc: 'Исключить файлы/папки по паттерну (можно повторять)' },
    { key: '--include-only=pattern', desc: 'Включить только файлы, соответствующие паттерну' },
    { key: '--backend-only', desc: 'Пресет: исключить UI-файлы (Forms, Views, UI)' },
    { key: '--domain-services', desc: 'Пресет: только бизнес-логика (Domain, Services)' },
    { key: '--compact-aggressive', desc: 'Пресет: максимальное сжатие (удаление комментариев и пустых строк)' },
    { key: '--web-app', desc: 'Пресет: веб-приложение (JS, CSS, HTML)' },
    { key: '--hybrid', desc: 'Гибридный режим (Backend + Frontend)' },
    { key: '--backend=Language', desc: 'Язык бекенда: CSharp, Python, JavaScript, ...' },
    { key: '--frontend=Language', desc: 'Язык фронтенда: JavaScript, TypeScript, ...' },
    { key: '--output=path  / -o=path', desc: 'Папка для сохранения результата' },
    { key: '--collapse-threshold=N', desc: 'Порог свёртывания методов (по умолчанию 50)' },
    { key: '--no-comments', desc: 'Удалить комментарии из кода' },
    { key: '--keep-empty-lines', desc: 'Сохранять пустые строки' },
    { key: '--no-consolidate-usings', desc: 'Не консолидировать using-директивы' },
    { key: '--exclude-file=path', desc: 'Загрузить паттерны исключений из файла' },
    { key: '--help', desc: 'Показать эту справку' },
    { key: '<project-path>', desc: 'Путь к проекту (обязательный параметр)' },
];

export const STORAGE_KEY = 'llmcodeexporter_configs';
export const LAST_KEY = 'llmcodeexporter_last';
export const SETTINGS_KEY = 'llmcodeexporter_settings';