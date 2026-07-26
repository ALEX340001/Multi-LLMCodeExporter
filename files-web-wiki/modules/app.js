// modules/app.js

import {
    renderHelp, renderHistory, showToast,
    elRunSh, elProject, elMode, elFormat, elPreset,
    elBackend, elFrontend, elExclude, elInclude,
    elOutput, elThreshold, elRemoveComments,
    elKeepEmpty, elConsolidate, elQuickLinks,
    elHistoryToggle, elHistoryList, elHistoryArrow,
    elSettingsBtn, elSettingsClose, elSettingsModal,
    elSettingsSave, elSettingsReset,
    openSettingsModal, closeSettingsModal,
    isSettingsModalOpen
} from './dom.js';

import {
    loadHistory, saveHistory, getHistory,
    deleteHistoryItem, clearHistory,
    captureConfig, applyConfig,
    saveCurrentConfig, loadLastConfig,
    loadConfigFromHistory, applyPreset,
    loadSettings, saveSettings, resetSettings,
    openSettingsWithCurrent
} from './providers.js';

import {
    updateCommandPreview,
    buildCommand
} from './generators.js';

import {
    copyCommand,
    exportHistoryJSON
} from './clipboard.js';

import { debounce } from './utils.js';

// ------------------------------------------------------------------
//  Состояние UI
// ------------------------------------------------------------------
let historyOpen = false;

// ------------------------------------------------------------------
//  Инициализация
// ------------------------------------------------------------------
function init() {
    // 1. Рендерим справку
    renderHelp();

    // 2. Загружаем настройки
    loadSettings();

    // 3. Загружаем историю
    loadHistory();

    // 4. Загружаем последнюю конфигурацию
    loadLastConfig();

    // 5. Обновляем превью команды
    updateCommandPreview();

    // 6. Управление видимостью истории
    const historyData = getHistory();
    if (historyData.length > 0) {
        elHistoryList.classList.add('open');
        elHistoryArrow.classList.add('open');
        historyOpen = true;
        elHistoryToggle.setAttribute('aria-expanded', 'true');
    } else {
        elHistoryList.classList.remove('open');
        elHistoryArrow.classList.remove('open');
        historyOpen = false;
        elHistoryToggle.setAttribute('aria-expanded', 'false');
    }

    // 7. Debounced функция для обновления команды
    const debouncedUpdate = debounce(updateCommandPreview, 300);

    // 8. Подписки на изменения полей формы (без elPreset)
    const formInputs = [
        elRunSh, elProject, elMode, elFormat,
        elBackend, elFrontend, elExclude, elInclude, elOutput, elThreshold,
        elRemoveComments, elKeepEmpty, elConsolidate, elQuickLinks
    ];

    formInputs.forEach(el => {
        el.addEventListener('change', updateCommandPreview);
        if (el.tagName === 'INPUT' && (el.type === 'text' || el.type === 'number')) {
            el.addEventListener('input', debouncedUpdate);
        }
    });

    // 9. Отдельный обработчик для пресета
    elPreset.addEventListener('change', (e) => {
        applyPreset(e.target.value);
    });

    // 10. Основные кнопки
    document.getElementById('buildCmdBtn').addEventListener('click', () => {
        updateCommandPreview();
        showToast('✅ Команда обновлена', 'success');
    });

    document.getElementById('saveConfigBtn').addEventListener('click', saveCurrentConfig);
    document.getElementById('loadLastBtn').addEventListener('click', loadLastConfig);
    document.getElementById('copyCmdBtn').addEventListener('click', copyCommand);

    document.getElementById('clearHistoryBtn').addEventListener('click', () => {
        clearHistory();
        if (getHistory().length === 0) {
            elHistoryList.classList.remove('open');
            elHistoryArrow.classList.remove('open');
            historyOpen = false;
            elHistoryToggle.setAttribute('aria-expanded', 'false');
        }
    });

    document.getElementById('exportHistoryBtn').addEventListener('click', exportHistoryJSON);
    document.getElementById('resetDefaultsBtn').addEventListener('click', resetDefaults);

    // 11. Переключение панели истории
    elHistoryToggle.addEventListener('click', toggleHistory);
    elHistoryToggle.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            toggleHistory();
        }
    });

    // 12. Делегирование событий для истории
    const historyContainer = document.getElementById('historyContainer');
    historyContainer.addEventListener('click', (e) => {
        const target = e.target.closest('button');
        if (!target) return;
        if (target.dataset.loadIdx !== undefined) {
            const idx = parseInt(target.dataset.loadIdx, 10);
            loadConfigFromHistory(idx);
        } else if (target.dataset.delIdx !== undefined) {
            const idx = parseInt(target.dataset.delIdx, 10);
            deleteHistoryItem(idx);
            if (getHistory().length === 0) {
                elHistoryList.classList.remove('open');
                elHistoryArrow.classList.remove('open');
                historyOpen = false;
                elHistoryToggle.setAttribute('aria-expanded', 'false');
            }
        }
    });

    // 13. Настройки (модальное окно)
    elSettingsBtn.addEventListener('click', openSettingsWithCurrent);
    elSettingsClose.addEventListener('click', closeSettingsModal);
    elSettingsModal.addEventListener('click', (e) => {
        if (e.target === elSettingsModal) closeSettingsModal();
    });
    elSettingsSave.addEventListener('click', saveSettings);
    elSettingsReset.addEventListener('click', resetSettings);

    // 14. Обработка Escape для закрытия модалки
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && isSettingsModalOpen()) {
            closeSettingsModal();
        }
    });

    // 15. Инициализация видимости блока выбора языков
    const currentPreset = elPreset.value;
    document.getElementById('hybridLangRow').style.display = currentPreset === 'hybrid' ? 'grid' : 'none';

    // 16. Приветственное сообщение
    setTimeout(() => {
        showToast('🚀 Готово! Настройте параметры и сохраните конфигурацию.', 'info');
    }, 400);

    console.log('🧠 LLMCodeExporter Configurator v2 (модульная версия)');
    console.log(`📦 ${getHistory().length} сохранённых конфигураций`);
}

// ------------------------------------------------------------------
//  Вспомогательные функции для UI
// ------------------------------------------------------------------

/**
 * Переключает видимость панели истории.
 */
function toggleHistory() {
    historyOpen = !historyOpen;
    elHistoryList.classList.toggle('open', historyOpen);
    elHistoryArrow.classList.toggle('open', historyOpen);
    elHistoryToggle.setAttribute('aria-expanded', historyOpen);
}

/**
 * Сбрасывает все настройки формы к значениям по умолчанию.
 */
function resetDefaults() {
    if (!confirm('Сбросить все настройки к значениям по умолчанию?')) return;
    elRunSh.value = '';
    elProject.value = '';
    elMode.value = 'balanced';
    elFormat.value = 'markdown';
    elPreset.value = '';
    elBackend.value = 'CSharp';
    elFrontend.value = 'JavaScript';
    elExclude.value = '';
    elInclude.value = '';
    elOutput.value = '';
    elThreshold.value = '50';
    elRemoveComments.checked = false;
    elKeepEmpty.checked = true;
    elConsolidate.checked = true;
    elQuickLinks.checked = true;
    document.getElementById('hybridLangRow').style.display = 'none';
    updateCommandPreview();
    showToast('↺ Настройки сброшены', 'info');
}

// ------------------------------------------------------------------
//  Запуск приложения
// ------------------------------------------------------------------
document.addEventListener('DOMContentLoaded', init);