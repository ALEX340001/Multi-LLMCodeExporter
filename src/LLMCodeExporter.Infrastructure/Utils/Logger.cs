
// версия 03-01-26


// версия 14-01-26
namespace LLMCodeExporter.Infrastructure.Utils;

public static class Logger
{
    private static readonly string AppFolderName = "LLMCodeExporter";
    private static readonly string LogSubFolder = "Logs";
    private static string _appFolderPath;
    private static string _currentLogFile;
    private static bool _isInitialized = false;

    // Инициализация лог-файла при первом запуске
    private static void Initialize()
    {
        if (_isInitialized) return;

        try
        {
            // Создание структуры папок
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _appFolderPath = Path.Combine(documentsPath, AppFolderName);
            string logFolderPath = Path.Combine(_appFolderPath, LogSubFolder);

            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            // Формирование уникального имени файла с датой И временем
            string dateTimeString = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            _currentLogFile = Path.Combine(logFolderPath, $"log_{dateTimeString}.txt");

            // Записываем заголовок в новый лог
            string header = new string('=', 80) + "\n";
            header += $"LLM Code Exporter - Лог сессии\n";
            header += $"Запуск: {DateTime.Now:dd.MM.yyyy HH:mm:ss}\n";
            header += new string('=', 80) + "\n\n";

            File.WriteAllText(_currentLogFile, header);

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Ошибка инициализации логгера: {ex.Message}");
        }
    }

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        Initialize();

        try
        {
            // Формирование сообщения с временем и уровнем
            string levelStr = level switch
            {
                LogLevel.Info => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "ERROR",
                LogLevel.Success => "SUCCESS",
                _ => "INFO"
            };

            string logMessage = $"[{DateTime.Now:HH:mm:ss}] [{levelStr}] {message}\n";

            // Запись в файл
            File.AppendAllText(_currentLogFile, logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Ошибка записи лога: {ex.Message}");
        }
    }

    public static void LogError(string message, Exception ex = null)
    {
        string fullMessage = ex != null
            ? $"{message} | Exception: {ex.Message}\n  StackTrace: {ex.StackTrace}"
            : message;
        Log(fullMessage, LogLevel.Error);
    }

    public static void LogSuccess(string message)
    {
        Log(message, LogLevel.Success);
    }

    public static void LogWarning(string message)
    {
        Log(message, LogLevel.Warning);
    }

    /// <summary>
    /// Получает путь к папке приложения
    /// </summary>
    public static string GetAppFolderPath()
    {
        Initialize();
        return _appFolderPath;
    }

    /// <summary>
    /// Получает путь к текущему лог-файлу
    /// </summary>
    public static string GetCurrentLogFile()
    {
        Initialize();
        return _currentLogFile;
    }

    /// <summary>
    /// Опциональный метод для очистки старых логов (более N дней)
    /// </summary>
    public static void CleanOldLogs(int daysToKeep = 30)
    {
        try
        {
            string logFolderPath = Path.Combine(GetAppFolderPath(), LogSubFolder);

            if (!Directory.Exists(logFolderPath)) return;

            var logFiles = Directory.GetFiles(logFolderPath, "log_*.txt");
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            int deletedCount = 0;

            foreach (var logFile in logFiles)
            {
                var fileInfo = new FileInfo(logFile);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(logFile);
                    deletedCount++;
                }
            }

            if (deletedCount > 0)
            {
                Log($"Очищено старых логов: {deletedCount}");
            }
        }
        catch (Exception ex)
        {
            Log($"Ошибка очистки старых логов: {ex.Message}", LogLevel.Warning);
        }
    }
}

public enum LogLevel
{
    Info,
    Warning,
    Error,
    Success
}



