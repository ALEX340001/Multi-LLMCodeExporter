// ===== Базовый путь (для загрузки nav/footer) =====
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
                initLanguageSwitcher();
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

// ===== Переключение языка (куки + перезагрузка) =====
window.switchLang = function (lang) {
    if (lang === 'ru') {
        // Сброс перевода – удаляем куку
        document.cookie = 'googtrans=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC';
    } else {
        document.cookie = `googtrans=/ru/${lang}; path=/;`;
    }
    window.location.reload();
};

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

// ===== Запуск всего (без виджета Google Translate) =====
document.addEventListener('DOMContentLoaded', () => {
    loadNav().then(() => {
        loadFooter();
        initClocks();
        // перевод работает через куки, виджет не нужен
    });
});
