using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Utils;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Manages local JSON file storage for article records.
    /// Data stored in Documents/EtnoPapers/data.json
    /// Maintains limit of 1000 records per file.
    /// </summary>
    public class DataStorageService
    {
        private readonly string _dataPath;
        private readonly int _recordLimit = 1000;
        private List<ArticleRecord> _records;
        private readonly object _lockObject = new();

        public DataStorageService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EtnoPapers"
            );

            Directory.CreateDirectory(appDataPath);
            _dataPath = Path.Combine(appDataPath, "data.json");
        }

        /// <summary>
        /// Initializes the storage service by loading existing data.
        /// </summary>
        public void Initialize()
        {
            lock (_lockObject)
            {
                _records = LoadAll();
            }
        }

        /// <summary>
        /// Loads all records from storage.
        /// </summary>
        public List<ArticleRecord> LoadAll()
        {
            lock (_lockObject)
            {
                try
                {
                    if (!File.Exists(_dataPath))
                        return new List<ArticleRecord>();

                    var json = File.ReadAllText(_dataPath);
                    if (string.IsNullOrWhiteSpace(json))
                        return new List<ArticleRecord>();

                    return JsonSerializationHelper.DeserializeList(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading records: {ex.Message}");
                    return new List<ArticleRecord>();
                }
            }
        }

        /// <summary>
        /// Gets a record by ID.
        /// </summary>
        public ArticleRecord GetById(string id)
        {
            lock (_lockObject)
            {
                if (_records == null)
                    _records = LoadAll();

                return _records?.FirstOrDefault(r => r.Id == id);
            }
        }

        /// <summary>
        /// Creates a new record and persists to storage.
        /// </summary>
        public bool Create(ArticleRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            lock (_lockObject)
            {
                if (_records == null)
                    _records = LoadAll();

                // Check record limit
                if (_records.Count >= _recordLimit)
                    return false;

                // Generate ID if not provided
                if (string.IsNullOrEmpty(record.Id))
                    record.Id = Guid.NewGuid().ToString();

                record.DataCriacao = DateTime.UtcNow;
                record.DataUltimaAtualizacao = DateTime.UtcNow;

                _records.Add(record);
                SaveToFile();
                return true;
            }
        }

        /// <summary>
        /// Updates an existing record.
        /// </summary>
        public bool Update(ArticleRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            lock (_lockObject)
            {
                if (_records == null)
                    _records = LoadAll();

                var existing = _records.FirstOrDefault(r => r.Id == record.Id);
                if (existing == null)
                    return false;

                var index = _records.IndexOf(existing);
                record.DataCriacao = existing.DataCriacao;
                record.DataUltimaAtualizacao = DateTime.UtcNow;
                _records[index] = record;
                SaveToFile();
                return true;
            }
        }

        /// <summary>
        /// Deletes a record by ID.
        /// </summary>
        public bool Delete(string id)
        {
            lock (_lockObject)
            {
                if (_records == null)
                    _records = LoadAll();

                var record = _records.FirstOrDefault(r => r.Id == id);
                if (record == null)
                    return false;

                _records.Remove(record);
                SaveToFile();
                return true;
            }
        }

        /// <summary>
        /// Deletes multiple records by IDs.
        /// </summary>
        public int DeleteMultiple(List<string> ids)
        {
            if (ids == null || ids.Count == 0)
                return 0;

            lock (_lockObject)
            {
                if (_records == null)
                    _records = LoadAll();

                var recordsToRemove = _records.Where(r => ids.Contains(r.Id)).ToList();
                var count = recordsToRemove.Count;

                foreach (var record in recordsToRemove)
                    _records.Remove(record);

                if (count > 0)
                    SaveToFile();

                return count;
            }
        }

        /// <summary>
        /// Gets total record count.
        /// </summary>
        public int Count()
        {
            lock (_lockObject)
            {
                if (_records == null)
                    _records = LoadAll();

                return _records.Count;
            }
        }

        /// <summary>
        /// Checks if storage is at limit.
        /// </summary>
        public bool IsAtLimit()
        {
            return Count() >= _recordLimit;
        }

        /// <summary>
        /// Gets remaining capacity.
        /// </summary>
        public int GetRemainingCapacity()
        {
            return Math.Max(0, _recordLimit - Count());
        }

        /// <summary>
        /// Persists all records to file.
        /// </summary>
        private void SaveToFile()
        {
            try
            {
                if (_records == null || _records.Count == 0)
                {
                    if (File.Exists(_dataPath))
                        File.Delete(_dataPath);
                    return;
                }

                var json = JsonSerializationHelper.SerializeList(_records);
                File.WriteAllText(_dataPath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save records: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the data file path.
        /// </summary>
        public string GetDataFilePath()
        {
            return _dataPath;
        }
    }
}
