// js/modules/generators.js

import {
    elRunSh, elProject, elMode, elFormat,
    elPreset, elBackend, elFrontend,
    elExclude, elInclude, elOutput,
    elThreshold, elRemoveComments,
    elKeepEmpty, elConsolidate,
    elCmdDisplay
} from './dom.js';

// ------------------------------------------------------------------
//  Квотирование строк с пробелами
// ------------------------------------------------------------------
export function quoteIfNeeded(str) {
    if (!str) return str;
    if (/\s/.test(str) || str.includes('"') || str.includes("'")) {
        const escaped = str.replace(/"/g, '\\"');
        return `"${escaped}"`;
    }
    return str;
}

// ------------------------------------------------------------------
//  Построение команды
// ------------------------------------------------------------------
export function buildCommand() {
    const parts = [];
    let runSh = elRunSh.value.trim();
    if (!runSh) runSh = './run.sh';
    parts.push(quoteIfNeeded(runSh));

    const proj = elProject.value.trim();
    if (proj) parts.push(quoteIfNeeded(proj));

    const mode = elMode.value;
    if (mode !== 'balanced') parts.push(`--mode=${mode}`);

    const format = elFormat.value;
    if (format !== 'markdown') parts.push(`--format=${format}`);

    const exclude = elExclude.value.trim();
    if (exclude) {
        exclude.split(',').map(s => s.trim()).filter(Boolean).forEach(p => {
            parts.push(`--exclude=${p}`);
        });
    }

    const include = elInclude.value.trim();
    if (include) {
        include.split(',').map(s => s.trim()).filter(Boolean).forEach(p => {
            parts.push(`--include-only=${p}`);
        });
    }

    const out = elOutput.value.trim();
    if (out) parts.push(`--output=${quoteIfNeeded(out)}`);

    const th = parseInt(elThreshold.value, 10);
    if (th && th !== 50) parts.push(`--collapse-threshold=${th}`);

    if (elRemoveComments.checked) parts.push('--no-comments');
    if (!elKeepEmpty.checked) parts.push('--keep-empty-lines');
    if (!elConsolidate.checked) parts.push('--no-consolidate-usings');

    const preset = elPreset.value;
    if (preset === 'hybrid') {
        const backend = elBackend.value;
        const frontend = elFrontend.value;
        if (backend) parts.push(`--backend=${backend}`);
        if (frontend) parts.push(`--frontend=${frontend}`);
        if (!parts.some(p => p === '--hybrid')) parts.push('--hybrid');
    } else if (preset === 'backend-only') {
        parts.push('--backend-only');
    } else if (preset === 'domain-services') {
        parts.push('--domain-services');
    } else if (preset === 'compact-aggressive') {
        parts.push('--compact-aggressive');
    } else if (preset === 'web-app') {
        parts.push('--web-app');
    }

    // Удаляем дубликаты, сохраняя порядок
    const unique = [];
    const seen = new Set();
    for (const p of parts) {
        if (!seen.has(p)) { seen.add(p); unique.push(p); }
    }
    return unique.join(' ');
}

// ------------------------------------------------------------------
//  Обновление превью команды
// ------------------------------------------------------------------
export function updateCommandPreview() {
    const cmd = buildCommand();
    elCmdDisplay.textContent = cmd || './run.sh --help';
}