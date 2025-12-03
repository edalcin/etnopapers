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
                _client = new MongoClient(connectionUri);
                _database = _client.GetDatabase("etnopapers");
                _collection = _database.GetCollection<BsonDocument>("records");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize MongoDB: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Uploads a single record to MongoDB.
        /// </summary>
        public async Task<bool> UploadRecordAsync(ArticleRecord record)
        {
            if (record == null || _collection == null)
                return false;

            try
            {
                var doc = BsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(record));
                await _collection.InsertOneAsync(doc);
                return true;
            }
            catch
            {
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
