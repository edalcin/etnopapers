/**
 * Data Storage Service
 * Handles local storage of article records using lowdb (JSON database)
 */

import path from 'path';
import { app } from 'electron';
import { JSONFile, Low } from 'lowdb';
import { v4 as uuidv4 } from 'uuid';
import { IDataStorageService } from '@shared/types/services';
import { ArticleRecord } from '@shared/types/article';
import { StorageLimitError } from '@shared/types/errors';
import { getConfigService } from '../ipc/configHandlers';

interface Database {
  records: ArticleRecord[];
}

export class DataStorageService implements IDataStorageService {
  private db: Low<Database> | null = null;
  private readonly dbPath: string;
  private maxRecords: number = 1000;

  constructor() {
    const dataDir = path.join(app.getPath('userData'), 'data');
    this.dbPath = path.join(dataDir, 'articles.json');
  }

  /**
   * Initialize database
   */
  async initialize(): Promise<void> {
    try {
      // Load max records from config
      const configService = getConfigService();
      const config = await configService.load();
      this.maxRecords = config.storage.maxRecords;

      // Setup lowdb
      const file = new JSONFile<Database>(this.dbPath);
      this.db = new Low<Database>(file, { records: [] });

      await this.db.read();

      // Initialize if empty
      if (!this.db.data) {
        this.db.data = { records: [] };
        await this.db.write();
      }
    } catch (error) {
      throw new Error(`Failed to initialize database: ${error instanceof Error ? error.message : String(error)}`);
    }
  }

  /**
   * Get all records
   */
  async getAll(): Promise<ArticleRecord[]> {
    this.ensureInitialized();
    await this.db!.read();
    return this.db!.data?.records || [];
  }

  /**
   * Get record by ID
   */
  async getById(id: string): Promise<ArticleRecord | null> {
    this.ensureInitialized();
    await this.db!.read();
    const record = this.db!.data?.records.find((r) => r._id === id);
    return record || null;
  }

  /**
   * Create new record
   */
  async create(
    data: Omit<ArticleRecord, '_id' | 'createdAt' | 'updatedAt'>
  ): Promise<ArticleRecord> {
    this.ensureInitialized();

    // Check limit
    if (!(await this.checkLimit())) {
      throw new StorageLimitError(
        this.db!.data?.records.length || 0,
        this.maxRecords
      );
    }

    const now = new Date();
    const record: ArticleRecord = {
      ...data,
      _id: uuidv4(),
      createdAt: now,
      updatedAt: now,
      syncStatus: 'local',
    };

    await this.db!.read();
    if (this.db!.data) {
      this.db!.data.records.push(record);
      await this.db!.write();
    }

    return record;
  }

  /**
   * Update record
   */
  async update(id: string, data: Partial<ArticleRecord>): Promise<ArticleRecord> {
    this.ensureInitialized();
    await this.db!.read();

    const recordIndex = this.db!.data?.records.findIndex((r) => r._id === id);
    if (recordIndex === undefined || recordIndex === -1) {
      throw new Error(`Record not found: ${id}`);
    }

    const updated: ArticleRecord = {
      ...this.db!.data!.records[recordIndex],
      ...data,
      _id: id, // Never change ID
      createdAt: this.db!.data!.records[recordIndex].createdAt, // Never change created date
      updatedAt: new Date(),
    };

    this.db!.data!.records[recordIndex] = updated;
    await this.db!.write();

    return updated;
  }

  /**
   * Delete record by ID
   */
  async delete(id: string): Promise<void> {
    this.ensureInitialized();
    await this.db!.read();

    if (this.db!.data) {
      const initialLength = this.db!.data.records.length;
      this.db!.data.records = this.db!.data.records.filter((r) => r._id !== id);

      if (this.db!.data.records.length === initialLength) {
        throw new Error(`Record not found: ${id}`);
      }

      await this.db!.write();
    }
  }

  /**
   * Delete multiple records by IDs
   */
  async deleteMany(ids: string[]): Promise<void> {
    this.ensureInitialized();
    await this.db!.read();

    if (this.db!.data) {
      const idSet = new Set(ids);
      this.db!.data.records = this.db!.data.records.filter((r) => !idSet.has(r._id));
      await this.db!.write();
    }
  }

  /**
   * Get total record count
   */
  async count(): Promise<number> {
    this.ensureInitialized();
    await this.db!.read();
    return this.db!.data?.records.length || 0;
  }

  /**
   * Check if storage limit is not exceeded
   */
  async checkLimit(): Promise<boolean> {
    this.ensureInitialized();
    await this.db!.read();
    const count = this.db!.data?.records.length || 0;
    return count < this.maxRecords;
  }

  /**
   * Get storage usage percentage
   */
  async getUsagePercentage(): Promise<number> {
    const count = await this.count();
    return (count / this.maxRecords) * 100;
  }

  /**
   * Update max records from config
   */
  async updateMaxRecords(newMax: number): Promise<void> {
    this.maxRecords = newMax;
  }

  /**
   * Ensure database is initialized
   */
  private ensureInitialized(): void {
    if (!this.db) {
      throw new Error('Database not initialized. Call initialize() first.');
    }
  }
}
