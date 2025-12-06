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

                _logger.Info("Initialize: Connecting to database 'etnopapers'...");
                _database = _client.GetDatabase("etnopapers");
                _logger.Info("Initialize: Database 'etnopapers' connected");

                _logger.Info("Initialize: Connecting to collection 'records'...");
                _collection = _database.GetCollection<BsonDocument>("records");
                _logger.Info("Initialize: Collection 'records' connected");

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

                _logger.Info($"UploadRecordAsync: Inserting document to collection 'records' - {record.Id}");
                await _collection.InsertOneAsync(doc);

                _logger.Info($"UploadRecordAsync: Successfully inserted document - {record.Id}");
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
                docs.Add(BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(record)));
            }

            try
            {
                await _collection.InsertManyAsync(docs);
                return docs.Count;
            }
            catch
            {
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
    }
}
