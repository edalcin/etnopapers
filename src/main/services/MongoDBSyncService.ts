/**
 * MongoDB Sync Service
 * Handles synchronization of records to MongoDB Atlas or local server
 */

import { MongoClient, Db, Collection } from 'mongodb';
import { IMongoDBSyncService, SyncStatus, SyncResult } from '@shared/types/services';
import { ArticleRecord } from '@shared/types/article';
import { MongoDBConfig } from '@shared/types/config';
import { MongoDBConnectionError } from '@shared/types/errors';

export class MongoDBSyncService implements IMongoDBSyncService {
  private client: MongoClient | null = null;
  private db: Db | null = null;
  private collection: Collection<ArticleRecord> | null = null;
  private config: MongoDBConfig;
  private isConnected = false;
  private lastSyncAt: Date | null = null;
  private totalSynced = 0;

  constructor(config: MongoDBConfig) {
    this.config = config;
  }

  /**
   * Test MongoDB connection
   */
  async testConnection(): Promise<boolean> {
    try {
      const client = new MongoClient(this.config.uri, {
        serverSelectionTimeoutMS: 5000,
        connectTimeoutMS: 5000,
      });

      await client.connect();
      await client.db('admin').command({ ping: 1 });
      await client.close();

      return true;
    } catch (error) {
      throw new MongoDBConnectionError(this.config.uri);
    }
  }

  /**
   * Connect to MongoDB
   */
  private async connect(): Promise<void> {
    try {
      if (this.isConnected && this.client) return;

      this.client = new MongoClient(this.config.uri);
      await this.client.connect();

      this.db = this.client.db(this.config.database);
      this.collection = this.db.collection<ArticleRecord>(this.config.collection);

      // Create indexes
      await this.collection.createIndex({ _id: 1 });
      await this.collection.createIndex({ createdAt: -1 });
      await this.collection.createIndex({ syncStatus: 1 });

      this.isConnected = true;
    } catch (error) {
      this.isConnected = false;
      throw new MongoDBConnectionError(this.config.uri);
    }
  }

  /**
   * Upload single record to MongoDB
   */
  async uploadRecord(record: ArticleRecord): Promise<void> {
    try {
      await this.connect();

      if (!this.collection) {
        throw new Error('Collection not initialized');
      }

      await this.collection.updateOne(
        { _id: record._id },
        {
          $set: {
            ...record,
            syncStatus: 'synced' as const,
          },
        },
        { upsert: true }
      );

      this.totalSynced++;
      this.lastSyncAt = new Date();
    } catch (error) {
      throw new Error(`Failed to upload record: ${error instanceof Error ? error.message : String(error)}`);
    }
  }

  /**
   * Upload batch of records to MongoDB
   */
  async uploadBatch(records: ArticleRecord[]): Promise<SyncResult> {
    const result: SyncResult = {
      success: 0,
      failed: 0,
      errors: {},
    };

    try {
      await this.connect();

      if (!this.collection) {
        throw new Error('Collection not initialized');
      }

      for (const record of records) {
        try {
          await this.collection.updateOne(
            { _id: record._id },
            {
              $set: {
                ...record,
                syncStatus: 'synced' as const,
              },
            },
            { upsert: true }
          );

          result.success++;
          this.totalSynced++;
        } catch (error) {
          result.failed++;
          result.errors[record._id] = error instanceof Error ? error.message : String(error);
        }
      }

      this.lastSyncAt = new Date();
    } catch (error) {
      // Batch-level error
      result.failed = records.length;
      throw error;
    }

    return result;
  }

  /**
   * Get sync status
   */
  async getStatus(): Promise<SyncStatus> {
    return {
      totalRecords: this.totalSynced,
      syncedRecords: this.totalSynced,
      lastSyncAt: this.lastSyncAt || undefined,
      isConnected: this.isConnected,
      error: undefined,
    };
  }

  /**
   * Delete record from MongoDB after successful sync
   */
  async deleteAfterSync(id: string): Promise<void> {
    try {
      await this.connect();

      if (!this.collection) {
        throw new Error('Collection not initialized');
      }

      await this.collection.deleteOne({ _id: id });
    } catch (error) {
      throw new Error(`Failed to delete record: ${error instanceof Error ? error.message : String(error)}`);
    }
  }

  /**
   * Disconnect from MongoDB
   */
  async disconnect(): Promise<void> {
    if (this.client) {
      await this.client.close();
      this.isConnected = false;
      this.client = null;
      this.db = null;
      this.collection = null;
    }
  }

  /**
   * Update configuration
   */
  updateConfig(config: MongoDBConfig): void {
    this.config = config;
    this.isConnected = false;
  }
}
