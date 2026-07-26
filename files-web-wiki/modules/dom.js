// modules/dom.js

import { CLI_HELP } from './config.js';

// ------------------------------------------------------------------
//  Утилиты выбора
// ------------------------------------------------------------------

/** Выбор одного элемента по селектору */
export const $ = (sel) => document.querySelector(sel);

/** Выбор нескольких элементов по селектору */
export const $$ = (sel) => document.querySelectorAll(sel);

// ------------------------------------------------------------------
//  Основные DOM-ссылки
// ------------------------------------------------------------------
export const elRunSh = $('#runShPath');
export const elProject = $('#projectPath');
export const elMode = $('#mode');
export const elFormat = $('#format');
export const elPreset = $('#preset');
export const elBackend = $('#backendLang');
export const elFrontend = $('#frontendLang');
export const elExclude = $('#excludePatterns');
export const elInclude = $('#includePatterns');
export const elOutput = $('#outputDir');
export const elThreshold = $('#collapseThreshold');
export const elRemoveComments = $('#removeComments');
export const elKeepEmpty = $('#keepEmptyLines');
export const elConsolidate = $('#consolidateUsings');
export const elQuickLinks = $('#generateQuickLinks');
export const elCmdDisplay = $('#cmdDisplay');
export const elHistoryContainer = $('#historyContainer');
export const elHistoryCount = $('#historyCount');
export const elHistoryList = $('#historyList');
export const elHistoryToggle = $('#historyToggle');
export const elHistoryArrow = $('#historyArrow');
export const elToastContainer = $('#toastContainer');

// ------------------------------------------------------------------
//  DOM-ссылки на модальное окно настроек
// ------------------------------------------------------------------
export const elSettingsModal = $('#settingsModal');
export const elSettingsRunSh = $('#settingsRunSh');
export const elSettingsBtn = $('#settingsBtn');
export const elSettingsClose = $('#settingsModalClose');
export const elSettingsSave = $('#settingsSaveBtn');
export const elSettingsReset = $('#settingsResetBtn');

// Переменная для хранения элемента, на котором был фокус до открытия модалки
let lastFocusedElement = null;

// ------------------------------------------------------------------
//  Рендеринг справки
// ------------------------------------------------------------------

/**
 * Рендерит блок справки с CLI-ключами.
 */
export function renderHelp() {
    const container = document.getElementById('cliHelpContainer');
    container.innerHTML = CLI_HELP.map(item => `
        <div class="row">
            <span class="key">${item.key}</span>
            <span class="desc">${item.desc}</span>
        </div>
    `).join('');
}

// ------------------------------------------------------------------
//  Экранирование HTML
// ------------------------------------------------------------------

/**
 * Экранирует HTML-спецсимволы для безопасного вывода.
 * @param {string} str - Строка для экранирования.
 * @returns {string} Экранированная строка.
 */
export function escapeHtml(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

// ------------------------------------------------------------------
//  Тост-уведомления
// ------------------------------------------------------------------

/**
 * Показывает всплывающее уведомление.
 * @param {string} message - Текст сообщения.
 * @param {('success'|'error'|'info')} type - Тип уведомления.
 */
export function showToast(message, type = 'info') {
    const container = elToastContainer;
    const icons = { success: '✅', error: '❌', info: 'ℹ️' };
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <span class="toast-icon" aria-hidden="true">${icons[type] || 'ℹ️'}</span>
        <span>${message}</span>
        <button class="toast-close" aria-label="Закрыть уведомление">✕</button>
    `;
    container.appendChild(toast);

    const close = () => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(8px)';
        toast.style.transition = 'opacity 0.2s, transform 0.2s';
        setTimeout(() => toast.remove(), 250);
    };
    toast.querySelector('.toast-close').addEventListener('click', close);
    setTimeout(close, 4000);
}

// ------------------------------------------------------------------
//  Рендеринг истории
// ------------------------------------------------------------------

/**
 * Рендерит список сохранённых конфигураций.
 * @param {Array} history - Массив объектов конфигураций.
 */
export function renderHistory(history) {
    const container = elHistoryContainer;
    const count = history.length;
    elHistoryCount.textContent = count;

    if (count === 0) {
        container.innerHTML = `<div class="history-empty">Пока нет сохранённых конфигураций</div>`;
        return;
    }

    container.innerHTML = history.map((item, idx) => {
        const date = new Date(item.timestamp);
        const dateStr = date.toLocaleString('ru-RU', {
            day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
        const tags = [];
        if (item.mode) tags.push(`режим: ${item.mode}`);
        if (item.format) tags.push(`формат: ${item.format}`);
        if (item.preset) tags.push(`пресет: ${item.preset}`);
        if (item.backend) tags.push(`backend: ${item.backend}`);
        if (item.frontend) tags.push(`frontend: ${item.frontend}`);

        return `
            <div class="history-item" data-idx="${idx}">
                <div class="left">
                    <div class="cmd-line">${escapeHtml(item.command || './run.sh')}</div>
                    <div class="meta">
                        <span>🕒 ${dateStr}</span>
                        ${tags.map(t => `<span class="tag">${t}</span>`).join('')}
                        ${item.project ? `<span class="tag">📁 ${escapeHtml(item.project)}</span>` : ''}
                    </div>
                </div>
                <div class="actions">
                    <button class="btn btn-sm btn-primary" data-load-idx="${idx}">
                        <span aria-hidden="true">📂</span> Загрузить
                    </button>
                    <button class="btn btn-sm btn-danger" data-del-idx="${idx}" aria-label="Удалить конфигурацию">
                        ✕
                    </button>
                </div>
            </div>
        `;
    }).join('');
}

// ------------------------------------------------------------------
//  Управление модальным окном настроек
// ------------------------------------------------------------------

/**
 * Открывает модальное окно настроек.
 */
export function openSettingsModal() {
    lastFocusedElement = document.activeElement;
    elSettingsModal.classList.add('active');
    setTimeout(() => {
        elSettingsRunSh.focus();
    }, 100);
}

/**
 * Закрывает модальное окно настроек.
 */
export function closeSettingsModal() {
    elSettingsModal.classList.remove('active');
    if (lastFocusedElement) {
        lastFocusedElement.focus();
        lastFocusedElement = null;
    }
}

/**
 * Проверяет, открыта ли модалка.
 * @returns {boolean}
 */
export function isSettingsModalOpen() {
    return elSettingsModal.classList.contains('active');
}