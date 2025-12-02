/**
 * Logger Service for main process
 * Manages application logging using Winston with file rotation
 */

import path from 'path';
import { app } from 'electron';
import winston from 'winston';
import { ILoggerService } from '@shared/types/services';

export class LoggerService implements ILoggerService {
  private logger: winston.Logger;
  private logFilePath: string;

  constructor() {
    // Determine log directory
    const logDir = path.join(app.getPath('userData'), 'logs');
    this.logFilePath = path.join(logDir, 'etnopapers.log');

    // Create Winston logger instance
    this.logger = winston.createLogger({
      level: process.env.LOG_LEVEL || 'info',
      format: winston.format.combine(
        winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
        winston.format.errors({ stack: true }),
        winston.format.printf(({ timestamp, level, message, ...meta }) => {
          let metaStr = '';
          if (Object.keys(meta).length > 0) {
            metaStr = ` ${JSON.stringify(meta)}`;
          }
          return `[${timestamp}] ${level.toUpperCase()}: ${message}${metaStr}`;
        })
      ),
      transports: [
        // File transport with rotation
        new winston.transports.File({
          filename: this.logFilePath,
          maxsize: 10485760, // 10 MB
          maxFiles: 5,
        }),
        // Error file transport
        new winston.transports.File({
          filename: path.join(logDir, 'error.log'),
          level: 'error',
          maxsize: 10485760, // 10 MB
          maxFiles: 5,
        }),
      ],
    });

    // Add console transport in development
    if (process.env.NODE_ENV === 'development') {
      this.logger.add(
        new winston.transports.Console({
          format: winston.format.combine(
            winston.format.colorize(),
            winston.format.simple()
          ),
        })
      );
    }
  }

  /**
   * Log debug level message
   */
  debug(message: string, ...args: any[]): void {
    this.logger.debug(message, ...args);
  }

  /**
   * Log info level message
   */
  info(message: string, ...args: any[]): void {
    this.logger.info(message, ...args);
  }

  /**
   * Log warning level message
   */
  warn(message: string, ...args: any[]): void {
    this.logger.warn(message, ...args);
  }

  /**
   * Log error level message
   */
  error(message: string, ...args: any[]): void {
    this.logger.error(message, ...args);
  }

  /**
   * Get path to log file
   */
  getLogFilePath(): string {
    return this.logFilePath;
  }
}
