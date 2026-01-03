using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using EtnoPapers.Core.Models;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Manages MongoDB synchronization for article records.
    /// </summary>
    public class MongoDBSyncService
    {
        private IMongoClient _client;
        private IMongoDatabase _database;
        private IMongoCollection<BsonDocument> _collection;
        private readonly LoggerService _logger = new LoggerService();

        /// <summary>
        /// Tests MongoDB connection.
        /// </summary>
        public async Task<bool> TestConnectionAsync(string connectionUri)
        {
            if (string.IsNullOrWhiteSpace(connectionUri))
                return false;

            try
            {
                var client = new MongoClient(connectionUri);
                await client.ListDatabaseNamesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes connection to MongoDB.
        /// </summary>
        public void Initialize(string connectionUri)
        {
            try
            {
                _logger.Info($"Initialize: Starting MongoDB initialization with URI: {connectionUri}");

                _logger.Info("Initialize: Creating MongoClient...");
                _client = new MongoClient(connectionUri);
                _logger.Info("Initialize: MongoClient created successfully");

                // Extract database name from connection URI
                var mongoUrl = MongoUrl.Create(connectionUri);
                string databaseName = mongoUrl.DatabaseName ?? "etnodb";
                string collectionName = "etnodb";

                _logger.Info($"Initialize: Connecting to database '{databaseName}'...");
                _database = _client.GetDatabase(databaseName);
                _logger.Info($"Initialize: Database '{databaseName}' connected");

                _logger.Info($"Initialize: Connecting to collection '{collectionName}'...");
                _collection = _database.GetCollection<BsonDocument>(collectionName);
                _logger.Info($"Initialize: Collection '{collectionName}' connected");

                _logger.Info("Initialize: MongoDB initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Initialize: Failed to initialize MongoDB: {ex.GetType().Name}: {ex.Message}", ex);
                _logger.Error($"Initialize: Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Failed to initialize MongoDB: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Uploads a single record to MongoDB.
        /// </summary>
        public async Task<bool> UploadRecordAsync(ArticleRecord record)
        {
            if (record == null)
            {
                _logger.Warn("UploadRecordAsync: record is null");
                return false;
            }

            if (_collection == null)
            {
                _logger.Error("UploadRecordAsync: MongoDB collection is not initialized (_collection is null)");
                return false;
            }

            if (_database == null)
            {
                _logger.Error("UploadRecordAsync: MongoDB database is not initialized (_database is null)");
                return false;
            }

            if (_client == null)
            {
                _logger.Error("UploadRecordAsync: MongoDB client is not initialized (_client is null)");
                return false;
            }

            try
            {
                _logger.Info($"UploadRecordAsync: Converting record to BsonDocument - {record.Id}");
                var doc = BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(record));

                // Remove the 'id' field - MongoDB will generate its own '_id'
                if (doc.Contains("id"))
                {
                    doc.Remove("id");
                    _logger.Info($"UploadRecordAsync: Removed 'id' field - MongoDB will generate '_id' automatically");
                }

                _logger.Info($"UploadRecordAsync: Inserting document to collection 'records'");
                await _collection.InsertOneAsync(doc);

                _logger.Info($"UploadRecordAsync: Successfully inserted document");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"UploadRecordAsync: Failed to upload record {record?.Id}: {ex.GetType().Name}: {ex.Message}", ex);
                _logger.Error($"UploadRecordAsync: Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Uploads multiple records in batch.
        /// </summary>
        public async Task<int> UploadBatchAsync(List<ArticleRecord> records)
        {
            if (records == null || records.Count == 0 || _collection == null)
                return 0;

            var docs = new List<BsonDocument>();
            foreach (var record in records)
            {
                var doc = BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(record));

                // Remove the 'id' field - MongoDB will generate its own '_id'
                if (doc.Contains("id"))
                {
                    doc.Remove("id");
                }

                docs.Add(doc);
            }

            try
            {
                await _collection.InsertManyAsync(docs);
                _logger.Info($"UploadBatchAsync: Successfully inserted {docs.Count} documents (without 'id' field)");
                return docs.Count;
            }
            catch (Exception ex)
            {
                _logger.Error($"UploadBatchAsync: Failed to upload batch: {ex.Message}", ex);
                return 0;
            }
        }

        /// <summary>
        /// Gets sync status.
        /// </summary>
        public string GetStatus()
        {
            return _collection != null ? "connected" : "disconnected";
        }

        /// <summary>
        /// Checks if a record with the same title and year already exists in MongoDB.
        /// </summary>
        /// <param name="titulo">The title to check</param>
        /// <param name="ano">The year to check</param>
        /// <returns>True if a duplicate exists, false otherwise</returns>
        public async Task<bool> CheckDuplicateExistsAsync(string titulo, int? ano)
        {
            if (_collection == null)
            {
                _logger.Error("CheckDuplicateExistsAsync: MongoDB collection is not initialized");
                return false;
            }

            if (string.IsNullOrWhiteSpace(titulo))
            {
                _logger.Warn("CheckDuplicateExistsAsync: titulo is null or empty");
                return false;
            }

            try
            {
                var normalizedTitulo = titulo.Trim().ToLowerInvariant();

                FilterDefinition<BsonDocument> filter;

                if (ano.HasValue)
                {
                    // Match both title (case-insensitive) and year
                    filter = Builders<BsonDocument>.Filter.And(
                        Builders<BsonDocument>.Filter.Regex("titulo", new BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(normalizedTitulo)}$", "i")),
                        Builders<BsonDocument>.Filter.Eq("ano", ano.Value)
                    );
                }
                else
                {
                    // Match only title (case-insensitive) if year is not available
                    filter = Builders<BsonDocument>.Filter.Regex("titulo", new BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(normalizedTitulo)}$", "i"));
                }

                var count = await _collection.CountDocumentsAsync(filter);
                var exists = count > 0;

                if (exists)
                {
                    _logger.Info($"CheckDuplicateExistsAsync: Found duplicate for titulo='{titulo}', ano={ano}");
                }

                return exists;
            }
            catch (Exception ex)
            {
                _logger.Error($"CheckDuplicateExistsAsync: Failed to check duplicate: {ex.Message}", ex);
                return false;
            }
        }
    }
}
