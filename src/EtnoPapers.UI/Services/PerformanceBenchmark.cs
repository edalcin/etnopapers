using System;
using System.Diagnostics;
using EtnoPapers.Core.Services;

namespace EtnoPapers.UI.Services
{
    /// <summary>
    /// Measures application startup performance metrics.
    /// Tracks time from app launch to main window visibility.
    /// Target: <2 seconds startup time
    /// </summary>
    public class PerformanceBenchmark
    {
        private static readonly Stopwatch StartupTimer = new Stopwatch();
        private readonly LoggerService _logger;

        public DateTime AppStartTime { get; private set; }
        public DateTime MainWindowVisibleTime { get; private set; }
        public TimeSpan TotalStartupTime { get; private set; }

        public PerformanceBenchmark()
        {
            _logger = new LoggerService();
        }

        /// <summary>
        /// Call this at the very beginning of application startup (App.xaml.cs constructor)
        /// </summary>
        public void RecordApplicationStart()
        {
            AppStartTime = DateTime.UtcNow;
            StartupTimer.Start();
            _logger.Info("Application startup initiated");
        }

        /// <summary>
        /// Call this when MainWindow becomes visible (MainWindow.xaml.cs Loaded event)
        /// </summary>
        public void RecordMainWindowVisible()
        {
            StartupTimer.Stop();
            MainWindowVisibleTime = DateTime.UtcNow;
            TotalStartupTime = StartupTimer.Elapsed;

            _logger.Info($"Main window visible in {TotalStartupTime.TotalMilliseconds:F2}ms");

            // Log warning if exceeds target
            if (TotalStartupTime.TotalSeconds > 2.0)
            {
                _logger.Warn($"Startup performance target missed: {TotalStartupTime.TotalSeconds:F2}s (target: <2s)");
            }
            else
            {
                _logger.Info($"Startup performance target achieved: {TotalStartupTime.TotalSeconds:F2}s (target: <2s)");
            }
        }

        /// <summary>
        /// Gets a human-readable startup time report.
        /// </summary>
        public string GetStartupReport()
        {
            return $@"
═══════════════════════════════════════════════════════════
Application Startup Performance Report
═══════════════════════════════════════════════════════════
Start Time:         {AppStartTime:HH:mm:ss.fff}
Window Visible:     {MainWindowVisibleTime:HH:mm:ss.fff}
Total Startup Time: {TotalStartupTime.TotalMilliseconds:F2} ms ({TotalStartupTime.TotalSeconds:F2} s)

Target Performance: < 2 seconds
Status:             {(TotalStartupTime.TotalSeconds < 2 ? "✓ PASS" : "✗ FAIL")}
═══════════════════════════════════════════════════════════
";
        }
    }
}
