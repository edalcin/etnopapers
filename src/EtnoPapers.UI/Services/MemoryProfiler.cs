using System;
using System.Diagnostics;
using EtnoPapers.Core.Services;

namespace EtnoPapers.UI.Services
{
    /// <summary>
    /// Profiles memory usage of the application.
    /// Target: Idle memory < 150 MB after loading 1000 records
    /// </summary>
    public class MemoryProfiler
    {
        private readonly LoggerService _logger;
        private long _initialMemory;
        private long _peakMemory;

        public MemoryProfiler()
        {
            _logger = new LoggerService();
        }

        /// <summary>
        /// Records the initial memory state at app startup.
        /// Call this during application initialization.
        /// </summary>
        public void RecordInitialMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            _initialMemory = GC.GetTotalMemory(true);
            _peakMemory = _initialMemory;

            _logger.Info($"Initial memory: {FormatBytes(_initialMemory)}");
        }

        /// <summary>
        /// Records the current memory usage (typically after loading records).
        /// </summary>
        public long RecordCurrentMemory()
        {
            var currentMemory = GC.GetTotalMemory(false);
            if (currentMemory > _peakMemory)
            {
                _peakMemory = currentMemory;
            }

            return currentMemory;
        }

        /// <summary>
        /// Gets a memory usage report with metrics.
        /// </summary>
        public string GetMemoryReport()
        {
            var currentMemory = RecordCurrentMemory();
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64;

            var report = $@"
═══════════════════════════════════════════════════════════
Memory Usage Report
═══════════════════════════════════════════════════════════
Initial Memory:     {FormatBytes(_initialMemory)}
Current Memory:     {FormatBytes(currentMemory)}
Peak Memory:        {FormatBytes(_peakMemory)}
Working Set:        {FormatBytes(workingSet)}
Memory Increase:    {FormatBytes(currentMemory - _initialMemory)}

Target (Idle):      < 150 MB
Current Status:     {(workingSet < 150 * 1024 * 1024 ? "✓ PASS" : "✗ FAIL")}
═══════════════════════════════════════════════════════════
";
            return report;
        }

        /// <summary>
        /// Performs garbage collection and returns freed memory.
        /// </summary>
        public long ForceGarbageCollection()
        {
            var beforeGC = GC.GetTotalMemory(false);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var afterGC = GC.GetTotalMemory(true);

            var freedMemory = beforeGC - afterGC;
            _logger.Info($"Garbage collection freed {FormatBytes(freedMemory)}");

            return freedMemory;
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:F2} {sizes[order]}";
        }
    }
}
