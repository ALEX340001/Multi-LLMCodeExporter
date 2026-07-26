
#!/bin/bash
# run.sh - Скрипт для сборки и запуска LLMCodeExporter

set -e  # Остановка при ошибке

# Определяем цвета для вывода
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Определяем пути
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="${SCRIPT_DIR}/src/LLMCodeExporter.CLI/LLMCodeExporter.CLI.csproj"

# Функция вывода справки
show_help() {
    echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${BLUE}║  📦 LLMCodeExporter - Сборка и запуск                         ║${NC}"
    echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
    echo ""
    echo -e "${YELLOW}Использование:${NC}"
    echo "  ./run.sh           - Собрать и запустить в интерактивном режиме"
    echo "  ./run.sh build     - Только собрать проект"
    echo "  ./run.sh clean     - Очистить сборку"
    echo "  ./run.sh help      - Показать эту справку"
    echo "  ./run.sh --help    - Показать справку приложения"
    echo ""
    echo -e "${YELLOW}Примеры:${NC}"
    echo "  ./run.sh                    # Запуск в интерактивном режиме"
    echo "  ./run.sh \"путь/к/проекту\"   # Запуск с указанием пути"
    echo "  ./run.sh --mode=compact     # Запуск в компактном режиме"
    echo ""
}

# Функция проверки .NET
check_dotnet() {
    if ! command -v dotnet &> /dev/null; then
        echo -e "${RED}❌ .NET SDK не найден!${NC}"
        echo "Установите .NET SDK: https://dotnet.microsoft.com/download"
        exit 1
    fi
    
    DOTNET_VERSION=$(dotnet --version)
    echo -e "${GREEN}✅ .NET SDK версия: ${DOTNET_VERSION}${NC}"
}

# Функция проверки проекта
check_project() {
    if [ ! -f "$PROJECT_PATH" ]; then
        echo -e "${RED}❌ Проект не найден: ${PROJECT_PATH}${NC}"
        echo "Проверьте структуру директорий."
        exit 1
    fi
    echo -e "${GREEN}✅ Проект найден: ${PROJECT_PATH}${NC}"
}

# Функция сборки
build_project() {
    echo -e "${YELLOW}🔨 Сборка проекта...${NC}"
    dotnet build "$PROJECT_PATH" --configuration Release
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Сборка успешно завершена!${NC}"
    else
        echo -e "${RED}❌ Ошибка сборки!${NC}"
        exit 1
    fi
}

# Функция очистки
clean_project() {
    echo -e "${YELLOW}🧹 Очистка проекта...${NC}"
    dotnet clean "$PROJECT_PATH"
    
    # Удаляем папки bin/obj
    find "${SCRIPT_DIR}/src" -type d -name "bin" -exec rm -rf {} \; 2>/dev/null || true
    find "${SCRIPT_DIR}/src" -type d -name "obj" -exec rm -rf {} \; 2>/dev/null || true
    
    echo -e "${GREEN}✅ Очистка завершена!${NC}"
}

# Функция запуска
run_project() {
    echo -e "${YELLOW}🚀 Запуск LLMCodeExporter...${NC}"
    echo -e "${BLUE}────────────────────────────────────────────────────────────────────${NC}"
    echo ""
    
    # Передаём все аргументы в dotnet run
    dotnet run --project "$PROJECT_PATH" -- "$@"
    
    EXIT_CODE=$?
    echo ""
    echo -e "${BLUE}────────────────────────────────────────────────────────────────────${NC}"
    
    if [ $EXIT_CODE -eq 0 ]; then
        echo -e "${GREEN}✅ Программа завершила работу успешно${NC}"
    else
        echo -e "${RED}❌ Программа завершилась с кодом: ${EXIT_CODE}${NC}"
    fi
    
    return $EXIT_CODE
}

# Функция установки как исполняемого
make_executable() {
    if [ ! -x "$0" ]; then
        chmod +x "$0"
        echo -e "${GREEN}✅ Скрипт сделан исполняемым${NC}"
    fi
}

# ============ ОСНОВНАЯ ЛОГИКА ============

# Делаем скрипт исполняемым (если не был)
make_executable

# Проверяем аргументы
case "$1" in
    help|--help|-h)
        show_help
        exit 0
        ;;
    build)
        check_dotnet
        check_project
        build_project
        ;;
    clean)
        check_dotnet
        clean_project
        ;;
    *)
        # Для всех остальных аргументов - сборка и запуск
        check_dotnet
        check_project
        build_project
        run_project "$@"
        ;;
esac