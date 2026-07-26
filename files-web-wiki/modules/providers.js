// modules/providers.js

import {
    elRunSh, elProject, elMode, elFormat, elPreset,
    elBackend, elFrontend, elExclude, elInclude,
    elOutput, elThreshold, elRemoveComments,
    elKeepEmpty, elConsolidate, elQuickLinks,
    showToast, renderHistory, openSettingsModal, closeSettingsModal,
    elSettingsRunSh
} from './dom.js';
import { STORAGE_KEY, LAST_KEY, SETTINGS_KEY } from './config.js';
import { buildCommand, updateCommandPreview } from './generators.js';
import { validateConfig, safeJSONParse } from './utils.js';

// ------------------------------------------------------------------
//  Состояние
// ------------------------------------------------------------------
let history = [];

// ------------------------------------------------------------------
//  Управление историей
// ------------------------------------------------------------------

/**
 * Загружает историю конфигураций из localStorage.
 * @returns {Array} Массив сохранённых конфигураций.
 */
export function loadHistory() {
    const raw = localStorage.getItem(STORAGE_KEY);
    const parsed = safeJSONParse(raw, []);
    history = Array.isArray(parsed) ? parsed.filter(item => validateConfig(item) !== null) : [];
    renderHistory(history);
    return history;
}

/**
 * Сохраняет текущую историю в localStorage.
 */
export function saveHistory() {
    try {
        localStorage.setItem(STORAGE_KEY, JSON.stringify(history));
    } catch (e) {
        console.warn('Failed to save history:', e);
    }
    renderHistory(history);
}

/**
 * Возвращает текущий массив истории.
 * @returns {Array}
 */
export function getHistory() {
    return history;
}

/**
 * Удаляет элемент истории по индексу.
 * @param {number} idx - Индекс элемента.
 */
export function deleteHistoryItem(idx) {
    if (idx < 0 || idx >= history.length) return;
    history.splice(idx, 1);
    saveHistory();
    showToast('🗑️ Конфигурация удалена', 'info');
}

/**
 * Очищает всю историю.
 */
export function clearHistory() {
    if (history.length === 0) return;
    if (!confirm('Удалить всю историю?')) return;
    history = [];
    saveHistory();
    showToast('История очищена', 'info');
}

// ------------------------------------------------------------------
//  Захват / применение конфигурации
// ------------------------------------------------------------------

/**
 * Собирает текущие значения полей формы в объект конфигурации.
 * @returns {Object} Объект конфигурации.
 */
export function captureConfig() {
    return {
        runSh: elRunSh.value.trim(),
        project: elProject.value.trim(),
        mode: elMode.value,
        format: elFormat.value,
        preset: elPreset.value,
        backend: elBackend.value,
        frontend: elFrontend.value,
        exclude: elExclude.value.trim(),
        include: elInclude.value.trim(),
        output: elOutput.value.trim(),
        threshold: parseInt(elThreshold.value, 10) || 50,
        removeComments: elRemoveComments.checked,
        keepEmptyLines: elKeepEmpty.checked,
        consolidateUsings: elConsolidate.checked,
        quickLinks: elQuickLinks.checked,
        command: buildCommand(),
        timestamp: Date.now(),
    };
}

/**
 * Применяет переданную конфигурацию к полям формы.
 * @param {Object} cfg - Объект конфигурации.
 */
export function applyConfig(cfg) {
    const normalized = validateConfig(cfg);
    if (!normalized) {
        showToast('⚠️ Повреждённая конфигурация', 'error');
        return;
    }

    elRunSh.value = normalized.runSh || '';
    elProject.value = normalized.project || '';
    elMode.value = normalized.mode || 'balanced';
    elFormat.value = normalized.format || 'markdown';
    elPreset.value = normalized.preset || '';
    elBackend.value = normalized.backend || 'CSharp';
    elFrontend.value = normalized.frontend || 'JavaScript';
    elExclude.value = normalized.exclude || '';
    elInclude.value = normalized.include || '';
    elOutput.value = normalized.output || '';
    elThreshold.value = normalized.threshold || 50;
    elRemoveComments.checked = !!normalized.removeComments;
    elKeepEmpty.checked = normalized.keepEmptyLines !== undefined ? normalized.keepEmptyLines : true;
    elConsolidate.checked = normalized.consolidateUsings !== undefined ? normalized.consolidateUsings : true;
    elQuickLinks.checked = normalized.quickLinks !== undefined ? normalized.quickLinks : true;

    const isHybrid = (normalized.preset === 'hybrid') || 
                     (normalized.backend && normalized.frontend && normalized.backend !== normalized.frontend);
    document.getElementById('hybridLangRow').style.display = isHybrid ? 'grid' : 'none';

    updateCommandPreview();
}

/**
 * Сохраняет текущую конфигурацию в историю.
 */
export function saveCurrentConfig() {
    const cfg = captureConfig();
    if (!cfg.project) {
        showToast('⚠️ Укажите путь к проекту перед сохранением', 'error');
        return;
    }
    history.push(cfg);
    if (history.length > 50) history = history.slice(-50);
    saveHistory();
    try {
        localStorage.setItem(LAST_KEY, JSON.stringify(cfg));
    } catch {}
    showToast('✅ Конфигурация сохранена!', 'success');
}

/**
 * Загружает последнюю использованную конфигурацию.
 */
export function loadLastConfig() {
    const raw = localStorage.getItem(LAST_KEY);
    if (!raw) {
        showToast('Нет сохранённой конфигурации', 'info');
        return;
    }
    const parsed = safeJSONParse(raw);
    if (parsed && validateConfig(parsed)) {
        applyConfig(parsed);
        showToast('📂 Загружена последняя конфигурация', 'success');
    } else {
        showToast('⚠️ Повреждённая конфигурация', 'error');
    }
}

/**
 * Загружает конфигурацию из истории по индексу.
 * @param {number} idx - Индекс в массиве истории.
 */
export function loadConfigFromHistory(idx) {
    if (idx < 0 || idx >= history.length) return;
    const cfg = history[idx];
    if (validateConfig(cfg)) {
        applyConfig(cfg);
        showToast(`📂 Загружена конфигурация #${idx + 1}`, 'success');
    } else {
        showToast('⚠️ Повреждённая конфигурация', 'error');
    }
}

// ------------------------------------------------------------------
//  Применение пресета
// ------------------------------------------------------------------

/**
 * Применяет выбранный пресет к полям формы.
 * @param {string} preset - Название пресета.
 */
export function applyPreset(preset) {
    if (!preset) return;
    elExclude.value = '';
    elInclude.value = '';

    switch (preset) {
        case 'backend-only':
            elExclude.value = '*.Designer.cs, Forms, Views, UI, *.xaml.cs, *.razor';
            elMode.value = 'balanced';
            break;
        case 'domain-services':
            elInclude.value = 'Domain, Services, Application, Core';
            elMode.value = 'balanced';
            break;
        case 'compact-aggressive':
            elMode.value = 'compact';
            elRemoveComments.checked = true;
            elKeepEmpty.checked = false;
            elConsolidate.checked = true;
            elThreshold.value = '30';
            break;
        case 'web-app':
            elMode.value = 'full';
            elExclude.value = 'node_modules, dist, build, .git, .next, *.min.js, *.min.css, *.bundle.js';
            elFormat.value = 'markdown';
            break;
        case 'hybrid':
            elMode.value = 'balanced';
            elBackend.value = 'CSharp';
            elFrontend.value = 'JavaScript';
            document.getElementById('hybridLangRow').style.display = 'grid';
            break;
        default: break;
    }

    if (preset !== 'hybrid') {
        document.getElementById('hybridLangRow').style.display = 'none';
    }

    updateCommandPreview();
}

// ------------------------------------------------------------------
//  Управление настройками (Settings)
// ------------------------------------------------------------------

/**
 * Загружает настройки из localStorage.
 */
export function loadSettings() {
    const raw = localStorage.getItem(SETTINGS_KEY);
    if (raw) {
        const settings = safeJSONParse(raw);
        if (settings && settings.runShPath) {
            elSettingsRunSh.value = settings.runShPath;
            elRunSh.value = settings.runShPath;
            updateCommandPreview();
        } else {
            elSettingsRunSh.value = './run.sh';
        }
    } else {
        elSettingsRunSh.value = './run.sh';
    }
}

/**
 * Сохраняет настройки в localStorage.
 */
export function saveSettings() {
    const runShPath = elSettingsRunSh.value.trim() || './run.sh';
    const settings = { runShPath };
    try {
        localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings));
        elRunSh.value = runShPath;
        updateCommandPreview();
        showToast('✅ Настройки сохранены', 'success');
        closeSettingsModal();
    } catch (e) {
        showToast('❌ Ошибка сохранения настроек', 'error');
        console.error(e);
    }
}

/**
 * Сбрасывает настройки к значениям по умолчанию.
 */
export function resetSettings() {
    elSettingsRunSh.value = './run.sh';
}

/**
 * Открывает модалку настроек с подгрузкой текущих значений.
 */
export function openSettingsWithCurrent() {
    const raw = localStorage.getItem(SETTINGS_KEY);
    if (raw) {
        const settings = safeJSONParse(raw);
        elSettingsRunSh.value = (settings && settings.runShPath) || './run.sh';
    } else {
        elSettingsRunSh.value = './run.sh';
    }
    openSettingsModal();
}