using System;
using System.IO;
using Serilog;
using Serilog.Core;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Provides file-based logging with rotation and multiple log levels.
    /// </summary>
    public class LoggerService
    {
        private readonly ILogger _logger;
        private readonly string _logPath;
        public ILogger Logger => _logger;

        public LoggerService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EtnoPapers",
                "logs"
            );

            Directory.CreateDirectory(appDataPath);
            _logPath = appDataPath;

            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    Path.Combine(appDataPath, "app-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Info(string message)
        {
            _logger.Information(message);
        }

        public void Warn(string message)
        {
            _logger.Warning(message);
        }

        public void Error(string message, Exception ex = null)
        {
            if (ex != null)
                _logger.Error(ex, message);
            else
                _logger.Error(message);
        }

        public string GetLogFilePath()
        {
            return _logPath;
        }
    }
}
