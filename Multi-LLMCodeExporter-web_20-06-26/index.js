// ===== Загрузка навигации =====
function getBasePath() {
    const path = window.location.pathname;
    if (
        path.includes('/documentation/') ||
        path.includes('/community/') ||
        path.includes('/dev/') ||
        path.includes('/about/')  // ← добавить эту строку
    ) {
        return '../';
    }
    return './';
}

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
            } else {
                console.error('❌ Контейнер .nav-wrapper не найден');
            }
        })
        .catch(error => console.error('❌ Ошибка загрузки навигации:', error));
}
// ===== Инициализация темы и звёзд =====
function initThemeAndStars() {
    console.log('🔍 initThemeAndStars вызвана, ищем кнопку...');
    const toggleBtn = document.getElementById('theme-toggle');
    if (!toggleBtn) {
        console.error('❌ Кнопка #theme-toggle не найдена');
        return;
    }

    // Восстановление темы из localStorage
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

    // Обработчик клика по кнопке
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

// ===== Загрузка футера =====
function loadFooter() {
    const base = getBasePath(); // определяем путь к корню
    return fetch(base + 'footer/footer.html') // теперь путь всегда правильный
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

// ===== Основной код при загрузке страницы =====
document.addEventListener('DOMContentLoaded', () => {
    loadNav().then(() => {
        // После загрузки навигации загружаем футер
        loadFooter();
        initClocks();
         // Инициализируем Google Translate после загрузки навигации
        initGoogleTranslate();
    });
});

// ===== Google Translate с улучшенной обработкой =====
let translateInitialized = false;

function initGoogleTranslate() {
    // Проверяем, что мы не на локальном файле (file://) и не на http (кроме localhost)
    const isLocal = window.location.protocol === 'file:' || 
                   (window.location.protocol === 'http:' && window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1');
    if (isLocal) {
        console.warn('⚠️ Google Translate не работает на file:// или обычном HTTP (кроме localhost). Используйте HTTPS.');
        // Показываем сообщение пользователю
        showTranslateUnavailable();
        return;
    }

    window.googleTranslateInit = function() {
        if (typeof google !== 'undefined' && google.translate) {
            new google.translate.TranslateElement({
                pageLanguage: 'ru',
                includedLanguages: 'en,es,fr,de,it,pt,zh,ja,ko,ar,tr,pl', // 👈 12 языков
                layout: google.translate.TranslateElement.InlineLayout.SIMPLE,
                autoDisplay: false
            }, 'google_translate_element');
            console.log('✅ Google Translate инициализирован (12 языков)');
            waitForTranslateSelect();
        } else {
            console.warn('⚠️ Google Translate API не загружен, повтор через 2 сек');
            setTimeout(initGoogleTranslate, 2000);
        }
    };

    // Загружаем скрипт Google Translate
    const script = document.createElement('script');
    script.src = '//translate.google.com/translate_a/element.js?cb=googleTranslateInit';
    script.async = true;
    script.onerror = function() {
        console.error('❌ Ошибка загрузки Google Translate скрипта');
        showTranslateUnavailable();
    };
    document.head.appendChild(script);
}

function waitForTranslateSelect() {
    let attempts = 0;
    const maxAttempts = 20; // 20 * 200 = 4 секунды
    const interval = setInterval(() => {
        attempts++;
        const select = document.querySelector('.goog-te-combo');
        if (select) {
            clearInterval(interval);
            console.log('✅ Элемент перевода найден');
            translateInitialized = true;
            hideGoogleBranding();
        } else if (attempts >= maxAttempts) {
            clearInterval(interval);
            console.warn('⚠️ Элемент перевода не найден за отведённое время');
            // Возможно, скрипт не загрузился полностью, попробуем переинициализировать
            // Но чтобы не зацикливаться, просто покажем сообщение
            showTranslateUnavailable();
        }
    }, 200);
}

function hideGoogleBranding() {
    const style = document.createElement('style');
    style.textContent = `
        .goog-te-banner-frame.skiptranslate { display: none !important; }
        body { top: 0px !important; }
        .goog-te-gadget { display: none !important; }
        .goog-te-menu-value { display: none !important; }
        .goog-te-combo { display: none !important; } /* скрываем оригинальный селект */
    `;
    document.head.appendChild(style);
}

function showTranslateUnavailable() {
    // Показываем уведомление в консоли и, возможно, на странице
    console.warn('🌐 Перевод временно недоступен. Попробуйте использовать встроенный переводчик браузера.');
    // Можно добавить всплывающее сообщение, но не обязательно
}

function triggerGoogleTranslate(langCode) {
    if (!translateInitialized) {
        console.warn('⚠️ Перевод ещё не инициализирован. Попробуйте позже.');
        // Если селект ещё не появился, попробуем подождать
        const checkExist = setInterval(() => {
            const select = document.querySelector('.goog-te-combo');
            if (select) {
                clearInterval(checkExist);
                translateInitialized = true;
                select.value = langCode;
                select.dispatchEvent(new Event('change'));
                console.log(`🌐 Язык переключен на: ${langCode}`);
            }
        }, 300);
        // Остановим проверку через 5 секунд
        setTimeout(() => clearInterval(checkExist), 5000);
        return;
    }
    const select = document.querySelector('.goog-te-combo');
    if (select) {
        select.value = langCode;
        select.dispatchEvent(new Event('change'));
        console.log(`🌐 Язык переключен на: ${langCode}`);
    } else {
        console.warn('⚠️ Селект не найден, пробуем переинициализировать');
        // Повторно инициализируем
        initGoogleTranslate();
    }
}

// ===== Скрываем лишние элементы Google (баннер, логотип) =====
function hideGoogleBranding() {
    const style = document.createElement('style');
    style.textContent = `
        .goog-te-banner-frame.skiptranslate { display: none !important; }
        body { top: 0px !important; }
        .goog-te-gadget { display: none !important; }
        .goog-te-menu-value { display: none !important; }
    `;
    document.head.appendChild(style);
}

// ===== Функция переключения языка (без бесконечного цикла) =====
function triggerGoogleTranslate(langCode) {
    const select = document.querySelector('.goog-te-combo');
    if (select) {
        select.value = langCode;
        select.dispatchEvent(new Event('change'));
        console.log(`🌐 Язык переключен на: ${langCode}`);
    } else {
        console.warn(`⚠️ Селект ещё не готов, повтор через 500 мс`);
        setTimeout(() => triggerGoogleTranslate(langCode), 500);
    }
}

// ===== Функция переключения языка =====
function triggerGoogleTranslate(langCode) {
    const select = document.querySelector('.goog-te-combo');
    if (select) {
        select.value = langCode;
        select.dispatchEvent(new Event('change'));
        console.log(`🌐 Язык переключен на: ${langCode}`);
    } else {
        console.warn('⚠️ Элемент выбора языка не найден. Возможно, Google Translate еще не загружен.');
        // Повторяем попытку через секунду
        setTimeout(() => triggerGoogleTranslate(langCode), 1000);
    }
}

// ===== Скрываем оригинальный виджет Google Translate =====
// Добавляем стиль для скрытия виджета, но оставляем его функциональным
const style = document.createElement('style');
style.textContent = `
    .goog-te-banner-frame.skiptranslate {
        display: none !important;
    }
    body {
        top: 0px !important;
    }
    .goog-te-gadget {
        display: none !important;
    }
`;
document.head.appendChild(style);

function animateOnScroll() {
    const cards = document.querySelectorAll('.project-card, .class-card');
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, { threshold: 0.1 });

    cards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(card);
    });
}
// ===== Арт-часы (дискретные тики) =====
let clockInterval = null;
let clocks = [];
let prevSecond = -1; // для отслеживания смены секунды

function createClocks() {
    const container = document.getElementById('clocks-container');
    if (!container) return;
    container.innerHTML = '';
    clocks = [];
    const count = Math.floor((window.innerWidth * window.innerHeight) / 15000) + 20;
    const now = new Date();
    const realSeconds = now.getSeconds();
    // Для каждого циферблата задаём случайное время
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
        // Случайные часы и минуты
        const hours = Math.floor(Math.random() * 12);
        const minutes = Math.floor(Math.random() * 60);
        // Секунды задаём случайно, чтобы стрелки были разбросаны
        const seconds = Math.floor(Math.random() * 60);
        clocks.push({
            canvas,
            hours,
            minutes,
            seconds,   // начальное значение секунд
            size
        });
    }
    // Запоминаем текущую секунду для синхронизации
    prevSecond = realSeconds;
    drawAllClocks();
    // Запускаем интервал проверки (каждые 100 мс для точности)
    if (clockInterval) clearInterval(clockInterval);
    clockInterval = setInterval(() => {
        const now = new Date();
        const sec = now.getSeconds();
        if (sec !== prevSecond) {
            // Произошёл тик — обновляем все часы
            clocks.forEach(clock => {
                clock.seconds = (clock.seconds + 1) % 60;
                if (clock.seconds === 0) {
                    // При переходе через 60 увеличиваем минуты
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
    clocks.forEach(clock => {
        drawClock(clock.canvas, clock.hours, clock.minutes, clock.seconds, clock.size);
    });
}

function drawClock(canvas, hours, minutes, seconds, size) {
    const ctx = canvas.getContext('2d');
    const cx = size / 2;
    const cy = size / 2;
    const radius = size * 0.45;
    ctx.clearRect(0, 0, size, size);
    // Если тёмная тема — не рисуем (или рисуем очень бледно)
    if (document.body.classList.contains('dark')) return;
    const faceColor = 'rgba(0,0,0,0.04)';
    const strokeColor = 'rgba(0,0,0,0.15)';
    const hourColor = 'rgba(0,0,0,0.3)';
    const minuteColor = 'rgba(0,0,0,0.25)';
    const secondColor = 'rgba(200,50,50,0.4)';
    // Циферблат
    ctx.beginPath();
    ctx.arc(cx, cy, radius, 0, Math.PI * 2);
    ctx.fillStyle = faceColor;
    ctx.fill();
    ctx.strokeStyle = strokeColor;
    ctx.lineWidth = 1;
    ctx.stroke();
    // Деления
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
    // Углы стрелок (дискретные значения)
    const hourAngle = ((hours % 12) / 12) * Math.PI * 2 + (minutes / 60) * (Math.PI / 6) - Math.PI / 2;
    const minuteAngle = (minutes / 60) * Math.PI * 2 - Math.PI / 2;
    const secondAngle = (seconds / 60) * Math.PI * 2 - Math.PI / 2;
    // Часовая
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(cx + Math.cos(hourAngle) * radius * 0.5, cy + Math.sin(hourAngle) * radius * 0.5);
    ctx.strokeStyle = hourColor;
    ctx.lineWidth = 2;
    ctx.stroke();
    // Минутная
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(cx + Math.cos(minuteAngle) * radius * 0.7, cy + Math.sin(minuteAngle) * radius * 0.7);
    ctx.strokeStyle = minuteColor;
    ctx.lineWidth = 1.5;
    ctx.stroke();
    // Секундная
    ctx.beginPath();
    ctx.moveTo(cx, cy);
    ctx.lineTo(cx + Math.cos(secondAngle) * radius * 0.8, cy + Math.sin(secondAngle) * radius * 0.8);
    ctx.strokeStyle = secondColor;
    ctx.lineWidth = 1;
    ctx.stroke();
    // Центр
    ctx.beginPath();
    ctx.arc(cx, cy, 2, 0, Math.PI * 2);
    ctx.fillStyle = strokeColor;
    ctx.fill();
}

// Инициализация часов
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

// Переключение темы
function handleThemeChange() {
    const container = document.getElementById('clocks-container');
    if (!container) return;
    if (document.body.classList.contains('dark')) {
        container.innerHTML = '';
        if (clockInterval) clearInterval(clockInterval);
        clocks = [];
    } else {
        createClocks();
    }
}