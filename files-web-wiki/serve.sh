#!/bin/bash

# Переходим в папку скрипта (чтобы сервер запускался в нужном месте)
cd "$(dirname "$0")"

echo "Запуск локального сервера для сайта..."

# Проверяем Python 3
if command -v python3 &> /dev/null; then
    echo "✅ Найден Python3. Запускаю сервер на порту 8000..."
    echo "🌐 Откройте в браузере: http://localhost:8000"
    echo "Для остановки нажмите Ctrl+C"
    python3 -m http.server 8000
    exit 0
fi

# Проверяем Python 2 (старые системы)
if command -v python &> /dev/null; then
    echo "✅ Найден Python2. Запускаю сервер на порту 8000..."
    python -m SimpleHTTPServer 8000
    exit 0
fi

# Проверяем Node.js и npx
if command -v npx &> /dev/null; then
    echo "✅ Найден Node.js. Запускаю http-server через npx..."
    echo "🌐 Откройте в браузере: http://localhost:8080"
    npx http-server -p 8080
    exit 0
fi

# Если ничего не подошло
echo "❌ Ошибка: не найден ни Python, ни Node.js"
echo "Установите Python (рекомендуется) или Node.js"
read -p "Нажмите Enter для выхода..."
exit 1
