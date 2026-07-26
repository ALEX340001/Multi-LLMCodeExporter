// js/modules/clipboard.js

import { elCmdDisplay, showToast } from './dom.js';
import { getHistory } from './providers.js';

export function copyCommand() {
    const cmd = elCmdDisplay.textContent;
    if (!cmd) return;
    navigator.clipboard.writeText(cmd).then(() => {
        showToast('📋 Команда скопирована в буфер!', 'success');
    }).catch(() => {
        const ta = document.createElement('textarea');
        ta.value = cmd;
        document.body.appendChild(ta);
        ta.select();
        document.execCommand('copy');
        ta.remove();
        showToast('📋 Команда скопирована!', 'success');
    });
}

export function exportHistoryJSON() {
    const history = getHistory();
    if (history.length === 0) {
        showToast('Нет данных для экспорта', 'info');
        return;
    }
    const data = JSON.stringify(history, null, 2);
    const blob = new Blob([data], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `llmcodeexporter_history_${new Date().toISOString().slice(0,10)}.json`;
    a.click();
    URL.revokeObjectURL(url);
    showToast('📤 История экспортирована', 'success');
}