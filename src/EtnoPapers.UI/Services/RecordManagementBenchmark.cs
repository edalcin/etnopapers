using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Services;

namespace EtnoPapers.UI.Services
{
    /// <summary>
    /// Benchmarks record management operations (sorting, filtering, searching, pagination).
    /// Target: All operations complete in <200ms with 1000+ records
    /// </summary>
    public class RecordManagementBenchmark
    {
        private readonly LoggerService _logger;
        private readonly DataStorageService _storageService;
        private List<ArticleRecord> _testRecords;

        public Dictionary<string, double> Results { get; } = new();

        public RecordManagementBenchmark()
        {
            _logger = new LoggerService();
            _storageService = new DataStorageService();
        }

        /// <summary>
        /// Loads test data and runs all benchmarks.
        /// </summary>
        public void RunAllBenchmarks()
        {
            try
            {
                _logger.Info("Starting record management benchmarks");

                // Load test records
                _testRecords = _storageService.LoadAll();
                if (_testRecords.Count == 0)
                {
                    _logger.Warn("No records in storage for benchmarking");
                    return;
                }

                _logger.Info($"Loaded {_testRecords.Count} records for benchmarking");

                // Run benchmarks
                BenchmarkSortByTitle();
                BenchmarkSortByYear();
                BenchmarkFilterByYear();
                BenchmarkSearchByText();
                BenchmarkPagination();

                PrintResults();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error running benchmarks: {ex.Message}", ex);
            }
        }

        private void BenchmarkSortByTitle()
        {
            var sw = Stopwatch.StartNew();
            var sorted = _testRecords.OrderBy(r => r.Titulo).ToList();
            sw.Stop();

            Results["Sort by Title"] = sw.Elapsed.TotalMilliseconds;
            _logger.Info($"Sort by Title: {sw.Elapsed.TotalMilliseconds:F2}ms ({sorted.Count} records)");
        }

        private void BenchmarkSortByYear()
        {
            var sw = Stopwatch.StartNew();
            var sorted = _testRecords.OrderByDescending(r => r.Ano).ToList();
            sw.Stop();

            Results["Sort by Year"] = sw.Elapsed.TotalMilliseconds;
            _logger.Info($"Sort by Year: {sw.Elapsed.TotalMilliseconds:F2}ms ({sorted.Count} records)");
        }

        private void BenchmarkFilterByYear()
        {
            if (_testRecords.Count == 0) return;

            var minYear = _testRecords.Min(r => r.Ano) ?? 2000;
            var maxYear = _testRecords.Max(r => r.Ano) ?? DateTime.Now.Year;

            var sw = Stopwatch.StartNew();
            var filtered = _testRecords.Where(r => r.Ano >= minYear && r.Ano <= maxYear).ToList();
            sw.Stop();

            Results["Filter by Year"] = sw.Elapsed.TotalMilliseconds;
            _logger.Info($"Filter by Year ({minYear}-{maxYear}): {sw.Elapsed.TotalMilliseconds:F2}ms ({filtered.Count} records)");
        }

        private void BenchmarkSearchByText()
        {
            var searchTerm = "de"; // Common search term

            var sw = Stopwatch.StartNew();
            var results = _testRecords.Where(r =>
                (!string.IsNullOrEmpty(r.Titulo) && r.Titulo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (r.Autores != null && r.Autores.Any(a => a.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            ).ToList();
            sw.Stop();

            Results["Search by Text"] = sw.Elapsed.TotalMilliseconds;
            _logger.Info($"Search for '{searchTerm}': {sw.Elapsed.TotalMilliseconds:F2}ms ({results.Count} records)");
        }

        private void BenchmarkPagination()
        {
            const int pageSize = 10;
            var pageCount = (_testRecords.Count + pageSize - 1) / pageSize;

            var sw = Stopwatch.StartNew();
            var pages = new List<List<ArticleRecord>>();
            for (int i = 0; i < pageCount; i++)
            {
                var page = _testRecords.Skip(i * pageSize).Take(pageSize).ToList();
                pages.Add(page);
            }
            sw.Stop();

            Results["Pagination (10 per page)"] = sw.Elapsed.TotalMilliseconds;
            _logger.Info($"Pagination ({pageCount} pages): {sw.Elapsed.TotalMilliseconds:F2}ms");
        }

        private void PrintResults()
        {
            var report = @"
═══════════════════════════════════════════════════════════
Record Management Performance Report
═══════════════════════════════════════════════════════════
";
            report += $"Total Records: {_testRecords.Count}\n";
            report += $"Target Performance: < 200ms per operation\n";
            report += "\nOperation Results:\n";

            var allPass = true;
            foreach (var result in Results.OrderBy(r => r.Value))
            {
                var status = result.Value < 200 ? "✓ PASS" : "✗ FAIL";
                if (result.Value >= 200) allPass = false;
                report += $"  {result.Key,-25} {result.Value,8:F2}ms {status}\n";
            }

            report += "\nOverall Status: " + (allPass ? "✓ ALL TESTS PASSED" : "✗ SOME TESTS FAILED");
            report += "\n═══════════════════════════════════════════════════════════\n";

            _logger.Info(report);
        }
    }
}
