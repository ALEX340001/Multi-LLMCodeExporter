// ===== Базовый путь =====
function getBasePath() {
    const path = window.location.pathname;
    if (
        path.includes('/documentation/') ||
        path.includes('/community/') ||
        path.includes('/dev/') ||
        path.includes('/about/')
    ) {
        return '../';
    }
    return './';
}

// ===== Загрузка навигации =====
function loadNav() {
    const base = getBasePath();
    return fetch(base + 'nav.html')
        .then(response => {
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return response.text();
        })
        .then(html => {
            console.log('📄 Загруженный HTML из nav.html:', html);
            const navWrapper = document.querySelector('.nav-wrapper');
            if (navWrapper) {
                navWrapper.innerHTML = html;
                console.log('✅ Навигация загружена');
                initThemeAndStars();
                initLanguageSwitcher();     // обработчики языков
            } else {
                console.error('❌ Контейнер .nav-wrapper не найден');
            }
        })
        .catch(error => console.error('❌ Ошибка загрузки навигации:', error));
}

// ===== Загрузка футера =====
function loadFooter() {
    const base = getBasePath();
    return fetch(base + 'footer/footer.html')
        .then(response => {
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return response.text();
        })
        .then(html => {
            document.body.insertAdjacentHTML('beforeend', html);
            console.log('Футер загружен');
        })
        .catch(error => console.error('Ошибка загрузки футера:', error));
}

// ===== Обработчик смены языка (делегирование) =====
function initLanguageSwitcher() {
    document.body.addEventListener('click', function (e) {
        const link = e.target.closest('a[data-lang]');
        if (!link) return;
        e.preventDefault();
        const lang = link.getAttribute('data-lang');
        if (lang) window.switchLang(lang);
    });
}

// ===== Переключение языка (куки) =====
window.switchLang = function (lang) {
    if (lang === 'ru') {
        // Удаляем куку – возврат к русскому
        document.cookie = 'googtrans=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC';
    } else {
        document.cookie = `googtrans=/ru/${lang}; path=/;`;
    }
    window.location.reload();
};

// ===== Автоопределение языка браузера =====
const supportedLangs = ['en', 'es', 'fr', 'de', 'it', 'pt', 'zh-CN', 'ja', 'ko', 'ar', 'tr', 'pl'];

function autoDetectLanguage() {
    // Если кука уже есть – не переопределяем
    if (document.cookie.includes('googtrans=')) return;

    const userLang = navigator.language || navigator.userLanguage;
    const langCode = userLang.split('-')[0];

    if (langCode === 'ru') return;

    if (supportedLangs.includes(langCode) || supportedLangs.includes(userLang)) {
        const targetLang = supportedLangs.includes(userLang) ? userLang : langCode;
        document.cookie = `googtrans=/ru/${targetLang}; path=/;`;
        location.reload();
    }
}

// ===== Тема и звёзды =====
function initThemeAndStars() {
    console.log('🔍 initThemeAndStars вызвана, ищем кнопку...');
    const toggleBtn = document.getElementById('theme-toggle');
    if (!toggleBtn) {
        console.error('❌ Кнопка #theme-toggle не найдена');
        return;
    }

    const saved = localStorage.getItem('theme');
    if (saved === 'dark') {
        document.body.classList.add('dark');
        toggleBtn.textContent = '☀️';
        initStars();
    } else {
        document.body.classList.remove('dark');
        toggleBtn.textContent = '🌙';
        removeStars();
    }

    toggleBtn.addEventListener('click', () => {
        const nowDark = document.body.classList.toggle('dark');
        localStorage.setItem('theme', nowDark ? 'dark' : 'light');
        toggleBtn.textContent = nowDark ? '☀️' : '🌙';
        nowDark ? initStars() : removeStars();
    });
}

// ===== Звёздный фон =====
let starCanvas = null;
let starCtx = null;
let resizeHandler = null;

function initStars() {
    if (starCanvas) return;
    const container = document.createElement('div');
    container.className = 'stars-container';
    starCanvas = document.createElement('canvas');
    starCanvas.className = 'stars-canvas';
    container.appendChild(starCanvas);
    document.body.appendChild(container);
    starCtx = starCanvas.getContext('2d');

    function drawStars() {
        if (!starCtx || !starCanvas) return;
        const w = window.innerWidth;
        const h = window.innerHeight;
        starCanvas.width = w;
        starCanvas.height = h;
        starCtx.clearRect(0, 0, w, h);
        const count = Math.floor((w * h) / 4000) + 80;
        starCtx.fillStyle = 'white';
        for (let i = 0; i < count; i++) {
            starCtx.globalAlpha = Math.random() * 0.6 + 0.2;
            starCtx.beginPath();
            starCtx.arc(Math.random() * w, Math.random() * h, Math.random() * 2 + 0.5, 0, Math.PI * 2);
            starCtx.fill();
        }
    }

    resizeHandler = () => drawStars();
    window.addEventListener('resize', resizeHandler);
    drawStars();
}

function removeStars() {
    if (starCanvas) {
        starCanvas.parentElement?.remove();
        starCanvas = null;
        starCtx = null;
        if (resizeHandler) window.removeEventListener('resize', resizeHandler);
        resizeHandler = null;
    }
}

// ===== Часы (дискретные тики) =====
let clockInterval = null;
let clocks = [];
let prevSecond = -1;

function createClocks() {
    const container = document.getElementById('clocks-container');
    if (!container) return;
    container.innerHTML = '';
    clocks = [];
    const count = Math.floor((window.innerWidth * window.innerHeight) / 15000) + 20;
    const now = new Date();
    const realSeconds = now.getSeconds();
    for (let i = 0; i < count; i++) {
        const canvas = document.createElement('canvas');
        canvas.className = 'clock-canvas';
        const size = Math.floor(Math.random() * 40 + 30);
        canvas.width = size;
        canvas.height = size;
        const x = Math.random() * (window.innerWidth - size);
        const y = Math.random() * (window.innerHeight - size);
        canvas.style.left = x + 'px';
        canvas.style.top = y + 'px';
        canvas.style.width = size + 'px';
        canvas.style.height = size + 'px';
        const rotation = Math.random() * 360;
        canvas.style.transform = `rotate(${rotation}deg)`;
        container.appendChild(canvas);
        const hours = Math.floor(Math.random() * 12);
        const minutes = Math.floor(Math.random() * 60);
        const seconds = Math.floor(Math.random() * 60);
        clocks.push({ canvas, hours, minutes, seconds, size });
    }
    prevSecond = realSeconds;
    drawAllClocks();
    if (clockInterval) clearInterval(clockInterval);
    clockInterval = setInterval(() => {
        const now = new Date();
        const sec = now.getSeconds();
        if (sec !== prevSecond) {
            clocks.forEach(clock => {
                clock.seconds = (clock.seconds + 1) % 60;
                if (clock.seconds === 0) {
                    clock.minutes = (clock.minutes + 1) % 60;
                    if (clock.minutes === 0) {
                        clock.hours = (clock.hours + 1) % 12;
                    }
                }
            });
            drawAllClocks();
            prevSecond = sec;
        }
    }, 100);
}

function drawAllClocks() {
    clocks.forEach(clock => drawClock(clock.canvas, clock.hours, clock.minutes, clock.seconds, clock.size));
}

function drawClock(canvas, hours, minutes, seconds, size) {
    const ctx = canvas.getContext('2d');
    const cx = size / 2;
    const cy = size / 2;
    const radius = size * 0.45;
    ctx.clearRect(0, 0, size, size);
    if (document.body.classList.contains('dark')) return;
    const faceColor = 'rgba(0,0,0,0.04)';
    const strokeColor = 'rgba(0,0,0,0.15)';
    const hourColor = 'rgba(0,0,0,0.3)';
    const minuteColor = 'rgba(0,0,0,0.25)';
    const secondColor = 'rgba(200,50,50,0.4)';
    ctx.beginPath();
    ctx.arc(cx, cy, radius, 0, Math.PI * 2);
    ctx.fillStyle = faceColor;
    ctx.fill();
    ctx.strokeStyle = strokeColor;
    ctx.lineWidth = 1;
    ctx.stroke();
    for (let i = 0; i < 12; i++) {
        const angle = (i / 12) * Math.PI * 2 - Math.PI / 2;
        const len = (i % 3 === 0) ? radius * 0.2 : radius * 0.1;
        const x1 = cx + Math.cos(angle) * (radius - len);
        const y1 = cy + Math.sin(angle) * (radius - len);
        const x2 = cx + Math.cos(angle) * radius;
        const y2 = cy + Math.sin(angle) * radius;
        ctx.beginPath();
        ctx.moveTo(x1, y1);
        ctx.lineTo(x2, y2);
        ctx.strokeStyle = strokeColor;
        ctx.lineWidth = (i % 3 === 0) ? 1.5 : 1;
        ctx.stroke();
    }
    const hourAngle = ((hours % 12) / 12) * Math.PI * 2 + (minutes / 60) * (Math.PI / 6) - Math.PI / 2;
    const minuteAngle = (minutes / 60) * Math.PI * 2 - Math.PI / 2;
    const secondAngle = (seconds / 60) * Math.PI * 2 - Math.PI / 2;
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(cx + Math.cos(hourAngle) * radius * 0.5, cy + Math.sin(hourAngle) * radius * 0.5);
    ctx.strokeStyle = hourColor;
    ctx.lineWidth = 2;
    ctx.stroke();
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(cx + Math.cos(minuteAngle) * radius * 0.7, cy + Math.sin(minuteAngle) * radius * 0.7);
    ctx.strokeStyle = minuteColor;
    ctx.lineWidth = 1.5;
    ctx.stroke();
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(cx + Math.cos(secondAngle) * radius * 0.8, cy + Math.sin(secondAngle) * radius * 0.8);
    ctx.strokeStyle = secondColor;
    ctx.lineWidth = 1;
    ctx.stroke();
    ctx.beginPath();
    ctx.arc(cx, cy, 2, 0, Math.PI * 2);
    ctx.fillStyle = strokeColor;
    ctx.fill();
}

function initClocks() {
    if (clockInterval) clearInterval(clockInterval);
    const container = document.getElementById('clocks-container');
    if (!container) return;
    if (document.body.classList.contains('dark')) {
        container.innerHTML = '';
        clocks = [];
        return;
    }
    createClocks();
}

// ===== Единый запуск после загрузки страницы =====
document.addEventListener('DOMContentLoaded', () => {
    loadNav().then(() => {
        loadFooter();
        initClocks();
        autoDetectLanguage();   // авто-перевод, если нужно
        // initGoogleTranslate() больше нет – перевод работает через куки
    });
});// ===== Базовый путь =====
function getBasePath() {
    const path = window.location.pathname;
    if (
        path.includes('/documentation/') ||
        path.includes('/community/') ||
        path.includes('/dev/') ||
        path.includes('/about/')
    ) {
        return '../';
    }
    return './';
}

// ===== Загрузка навигации =====
function loadNav() {
    const base = getBasePath();
    return fetch(base + 'nav.html')
        .then(response => {
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return response.text();
        })
        .then(html => {
            console.log('📄 Загруженный HTML из nav.html:', html);
            const navWrapper = document.querySelector('.nav-wrapper');
            if (navWrapper) {
                navWrapper.innerHTML = html;
                console.log('✅ Навигация загружена');
                initThemeAndStars();
                initLanguageSwitcher();     // обработчики для языков
            } else {
                console.error('❌ Контейнер .nav-wrapper не найден');
            }
        })
        .catch(error => console.error('❌ Ошибка загрузки навигации:', error));
}

// ===== Загрузка футера (единая версия) =====
function loadFooter() {
    const base = getBasePath();
    return fetch(base + 'footer/footer.html')
        .then(response => {
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            return response.text();
        })
        .then(html => {
            document.body.insertAdjacentHTML('beforeend', html);
            console.log('Футер загружен');
        })
        .catch(error => console.error('Ошибка загрузки футера:', error));
}

// ===== Обработчик кликов по языкам (делегирование) =====
function initLanguageSwitcher() {
    document.body.addEventListener('click', function (e) {
        const link = e.target.closest('a[data-lang]');
        if (!link) return;
        e.preventDefault();
        const lang = link.getAttribute('data-lang');
        if (lang) window.switchLang(lang);
    });
}

// ===== Переключение языка через куки (надёжный способ) =====
window.switchLang = function (lang) {
    if (lang === 'ru') {
        // Сброс перевода
        document.cookie = 'googtrans=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC';
    } else {
        document.cookie = `googtrans=/ru/${lang}; path=/;`;
    }
    window.location.reload();
};

// ===== Автоопределение языка пользователя =====
const supportedLangs = ['en', 'es', 'fr', 'de', 'it', 'pt', 'zh-CN', 'ja', 'ko', 'ar', 'tr', 'pl'];

function autoDetectLanguage() {
    // Если кука уже есть — пользователь уже выбирал язык, не перебиваем
    if (document.cookie.includes('googtrans=')) return;

    const userLang = navigator.language || navigator.userLanguage;
    const langCode = userLang.split('-')[0];

    if (langCode === 'ru') return;

    if (supportedLangs.includes(langCode) || supportedLangs.includes(userLang)) {
        const targetLang = supportedLangs.includes(userLang) ? userLang : langCode;
        document.cookie = `googtrans=/ru/${targetLang}; path=/;`;
        location.reload();
    }
}

// ===== Тема и звёзды =====
function initThemeAndStars() {
    console.log('🔍 initThemeAndStars вызвана, ищем кнопку...');
    const toggleBtn = document.getElementById('theme-toggle');
    if (!toggleBtn) {
        console.error('❌ Кнопка #theme-toggle не найдена');
        return;
    }

    const saved = localStorage.getItem('theme');
    if (saved === 'dark') {
        document.body.classList.add('dark');
        toggleBtn.textContent = '☀️';
        initStars();
    } else {
        document.body.classList.remove('dark');
        toggleBtn.textContent = '🌙';
        removeStars();
    }

    toggleBtn.addEventListener('click', () => {
        const nowDark = document.body.classList.toggle('dark');
        localStorage.setItem('theme', nowDark ? 'dark' : 'light');
        toggleBtn.textContent = nowDark ? '☀️' : '🌙';
        nowDark ? initStars() : removeStars();
    });
}

// ===== Звёздный фон =====
let starCanvas = null;
let starCtx = null;
let resizeHandler = null;

function initStars() {
    if (starCanvas) return;
    const container = document.createElement('div');
    container.className = 'stars-container';
    starCanvas = document.createElement('canvas');
    starCanvas.className = 'stars-canvas';
    container.appendChild(starCanvas);
    document.body.appendChild(container);
    starCtx = starCanvas.getContext('2d');

    function drawStars() {
        if (!starCtx || !starCanvas) return;
        const w = window.innerWidth;
        const h = window.innerHeight;
        starCanvas.width = w;
        starCanvas.height = h;
        starCtx.clearRect(0, 0, w, h);
        const count = Math.floor((w * h) / 4000) + 80;
        starCtx.fillStyle = 'white';
        for (let i = 0; i < count; i++) {
            starCtx.globalAlpha = Math.random() * 0.6 + 0.2;
            starCtx.beginPath();
            starCtx.arc(Math.random() * w, Math.random() * h, Math.random() * 2 + 0.5, 0, Math.PI * 2);
            starCtx.fill();
        }
    }

    resizeHandler = () => drawStars();
    window.addEventListener('resize', resizeHandler);
    drawStars();
}

function removeStars() {
    if (starCanvas) {
        starCanvas.parentElement?.remove();
        starCanvas = null;
        starCtx = null;
        if (resizeHandler) window.removeEventListener('resize', resizeHandler);
        resizeHandler = null;
    }
}

// ===== Google Translate (инициализация, скрытие) =====
function initGoogleTranslate() {
    const isLocal = window.location.protocol === 'file:' ||
        (window.location.protocol === 'http:' && window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1');
    if (isLocal) {
        console.warn('⚠️ Google Translate не работает в этом окружении.');
        return;
    }

    const container = document.createElement('div');
    container.id = 'google_translate_element';
    container.style.display = 'none';
    document.body.appendChild(container);

    window.googleTranslateInit = function () {
        if (window.google && google.translate) {
            new google.translate.TranslateElement({
                pageLanguage: 'ru',
                includedLanguages: supportedLangs.join(','),
                layout: google.translate.TranslateElement.InlineLayout.SIMPLE,
                autoDisplay: false
            }, 'google_translate_element');
            console.log('✅ Google Translate виджет инициализирован');
            hideGoogleBranding();
        } else {
            console.warn('⚠️ Google API ещё не готов, повтор через 2 с');
            setTimeout(initGoogleTranslate, 2000);
        }
    };

    const script = document.createElement('script');
    script.src = '//translate.google.com/translate_a/element.js?cb=googleTranslateInit';
    script.async = true;
    script.onerror = () => console.error('❌ Не удалось загрузить скрипт Google Translate');
    document.head.appendChild(script);
}

function hideGoogleBranding() {
    const style = document.createElement('style');
    style.textContent = `
        .goog-te-banner-frame.skiptranslate, iframe.goog-te-banner-frame, iframe[id*="goog-te-banner"],
        #goog-gt-tt, .goog-tooltip, .goog-tooltip:hover, .goog-te-balloon-frame, iframe.goog-te-balloon-frame,
        .goog-te-gadget, .goog-te-gadget-simple, #google_translate_element, .goog-te-menu-frame,
        iframe.goog-te-menu-frame, .goog-te-combo, select.goog-te-combo, .goog-te-menu-value, .goog-te-menu-value span {
            display: none !important;
        }
        iframe[src*="translate.google.com"], iframe[src*="translate.googleapis.com"] {
            display: none !important;
            height: 0 !important;
            width: 0 !important;
            border: none !important;
        }
        body { top: 0 !important; position: static !important; }
    `;
    document.head.appendChild(style);

    const container = document.getElementById('google_translate_element');
    if (container) container.remove();
}

// ===== Часы (всё как у вас, без изменений) =====
let clockInterval = null;
let clocks = [];
let prevSecond = -1;

function createClocks() {
    const container = document.getElementById('clocks-container');
    if (!container) return;
    container.innerHTML = '';
    clocks = [];
    const count = Math.floor((window.innerWidth * window.innerHeight) / 15000) + 20;
    const now = new Date();
    const realSeconds = now.getSeconds();
    for (let i = 0; i < count; i++) {
        const canvas = document.createElement('canvas');
        canvas.className = 'clock-canvas';
        const size = Math.floor(Math.random() * 40 + 30);
        canvas.width = size;
        canvas.height = size;
        const x = Math.random() * (window.innerWidth - size);
        const y = Math.random() * (window.innerHeight - size);
        canvas.style.left = x + 'px';
        canvas.style.top = y + 'px';
        canvas.style.width = size + 'px';
        canvas.style.height = size + 'px';
        const rotation = Math.random() * 360;
        canvas.style.transform = `rotate(${rotation}deg)`;
        container.appendChild(canvas);
        const hours = Math.floor(Math.random() * 12);
        const minutes = Math.floor(Math.random() * 60);
        const seconds = Math.floor(Math.random() * 60);
        clocks.push({ canvas, hours, minutes, seconds, size });
    }
    prevSecond = realSeconds;
    drawAllClocks();
    if (clockInterval) clearInterval(clockInterval);
    clockInterval = setInterval(() => {
        const now = new Date();
        const sec = now.getSeconds();
        if (sec !== prevSecond) {
            clocks.forEach(clock => {
                clock.seconds = (clock.seconds + 1) % 60;
                if (clock.seconds === 0) {
                    clock.minutes = (clock.minutes + 1) % 60;
                    if (clock.minutes === 0) {
                        clock.hours = (clock.hours + 1) % 12;
                    }
                }
            });
            drawAllClocks();
            prevSecond = sec;
        }
    }, 100);
}

function drawAllClocks() {
    clocks.forEach(clock => drawClock(clock.canvas, clock.hours, clock.minutes, clock.seconds, clock.size));
}

function drawClock(canvas, hours, minutes, seconds, size) {
    const ctx = canvas.getContext('2d');
    const cx = size / 2;
    const cy = size / 2;
    const radius = size * 0.45;
    ctx.clearRect(0, 0, size, size);
    if (document.body.classList.contains('dark')) return;
    const faceColor = 'rgba(0,0,0,0.04)';
    const strokeColor = 'rgba(0,0,0,0.15)';
    const hourColor = 'rgba(0,0,0,0.3)';
    const minuteColor = 'rgba(0,0,0,0.25)';
    const secondColor = 'rgba(200,50,50,0.4)';
    ctx.beginPath();
    ctx.arc(cx, cy, radius, 0, Math.PI * 2);
    ctx.fillStyle = faceColor;
    ctx.fill();
    ctx.strokeStyle = strokeColor;
    ctx.lineWidth = 1;
    ctx.stroke();
    for (let i = 0; i < 12; i++) {
        const angle = (i / 12) * Math.PI * 2 - Math.PI / 2;
        const len = (i % 3 === 0) ? radius * 0.2 : radius * 0.1;
        const x1 = cx + Math.cos(angle) * (radius - len);
        const y1 = cy + Math.sin(angle) * (radius - len);
        const x2 = cx + Math.cos(angle) * radius;
        const y2 = cy + Math.sin(angle) * radius;
        ctx.beginPath();
        ctx.moveTo(x1, y1);
        ctx.lineTo(x2, y2);
        ctx.strokeStyle = strokeColor;
        ctx.lineWidth = (i % 3 === 0) ? 1.5 : 1;
        ctx.stroke();
    }
    const hourAngle = ((hours % 12) / 12) * Math.PI * 2 + (minutes / 60) * (Math.PI / 6) - Math.PI / 2;
    const minuteAngle = (minutes / 60) * Math.PI * 2 - Math.PI / 2;
    const secondAngle = (seconds / 60) * Math.PI * 2 - Math.PI / 2;
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(cx + Math.cos(hourAngle) * radius * 0.5, cy + Math.sin(hourAngle) * radius * 0.5);
    ctx.strokeStyle = hourColor;
    ctx.lineWidth = 2;
    ctx.stroke();
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(cx + Math.cos(minuteAngle) * radius * 0.7, cy + Math.sin(minuteAngle) * radius * 0.7);
    ctx.strokeStyle = minuteColor;
    ctx.lineWidth = 1.5;
    ctx.stroke();
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(cx + Math.cos(secondAngle) * radius * 0.8, cy + Math.sin(secondAngle) * radius * 0.8);
    ctx.strokeStyle = secondColor;
    ctx.lineWidth = 1;
    ctx.stroke();
    ctx.beginPath();
    ctx.arc(cx, cy, 2, 0, Math.PI * 2);
    ctx.fillStyle = strokeColor;
    ctx.fill();
}

function initClocks() {
    if (clockInterval) clearInterval(clockInterval);
    const container = document.getElementById('clocks-container');
    if (!container) return;
    if (document.body.classList.contains('dark')) {
        container.innerHTML = '';
        clocks = [];
        return;
    }
    createClocks();
}

// ===== Единый запуск после загрузки страницы =====
document.addEventListener('DOMContentLoaded', () => {
    loadNav().then(() => {
        loadFooter();          // только один раз
        initClocks();
        autoDetectLanguage();  // перед виджетом
        initGoogleTranslate();
    });
});
