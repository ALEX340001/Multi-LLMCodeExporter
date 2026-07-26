// modules/utils.js

/**
 * Debounce — ограничивает частоту вызова функции.
 * @param {Function} fn - Функция для вызова.
 * @param {number} delay - Задержка в миллисекундах.
 * @returns {Function} Обёрнутая функция с debounce.
 */
export function debounce(fn, delay) {
    let timer = null;
    return (...args) => {
        clearTimeout(timer);
        timer = setTimeout(() => fn(...args), delay);
    };
}

/**
 * Проверяет и нормализует объект конфигурации.
 * @param {any} data - Данные из localStorage.
 * @returns {Object|null} Нормализованная конфигурация или null, если данные некорректны.
 */
export function validateConfig(data) {
    if (!data || typeof data !== 'object') return null;
    
    const required = ['project', 'mode', 'format', 'timestamp'];
    for (const field of required) {
        if (!(field in data)) return null;
    }
    
    return {
        runSh: data.runSh || '',
        project: data.project || '',
        mode: data.mode || 'balanced',
        format: data.format || 'markdown',
        preset: data.preset || '',
        backend: data.backend || 'CSharp',
        frontend: data.frontend || 'JavaScript',
        exclude: data.exclude || '',
        include: data.include || '',
        output: data.output || '',
        threshold: data.threshold || 50,
        removeComments: !!data.removeComments,
        keepEmptyLines: data.keepEmptyLines !== undefined ? data.keepEmptyLines : true,
        consolidateUsings: data.consolidateUsings !== undefined ? data.consolidateUsings : true,
        quickLinks: data.quickLinks !== undefined ? data.quickLinks : true,
        command: data.command || '',
        timestamp: data.timestamp || Date.now(),
    };
}

/**
 * Безопасный парсинг JSON с возвратом значения по умолчанию.
 * @param {string} raw - Строка JSON.
 * @param {*} defaultValue - Значение по умолчанию при ошибке.
 * @returns {*} Распарсенный объект или значение по умолчанию.
 */
export function safeJSONParse(raw, defaultValue = null) {
    try {
        return JSON.parse(raw);
    } catch {
        return defaultValue;
    }
}