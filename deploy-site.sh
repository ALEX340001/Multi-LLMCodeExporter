#!/bin/bash

# Если нет .git – инициализируем репозиторий и привязываем к удалённому
if [ ! -d ".git" ]; then
    echo "🔧 .git не найден. Инициализируем репозиторий..."
    git init
    git remote add origin https://github.com/ALEX340001/Multi-LLMCodeExporter.git
    echo "✅ Репозиторий инициализирован и привязан к origin"
fi

# Переключаемся на ветку gh-pages (если ещё не там)
git checkout gh-pages 2>/dev/null || git checkout --orphan gh-pages

echo "📦 Добавляем изменения..."
git add .

echo "💬 Введите сообщение коммита (или нажмите Enter для автоматического):"
read commit_msg
if [ -z "$commit_msg" ]; then
    commit_msg="Deploy website $(date '+%Y-%m-%d %H:%M:%S')"
fi

git commit -m "$commit_msg"

echo "🚀 Отправляем на GitHub (принудительно, чтобы перезаписать ветку gh-pages)..."
git push -f origin gh-pages

echo "✅ Готово! Сайт обновлён."