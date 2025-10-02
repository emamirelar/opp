import { Injectable, isDevMode } from '@angular/core';

export enum LogLevel {
  Debug = 0,
  Info = 1,
  Warn = 2,
  Error = 3,
  None = 4
}

export interface LogEntry {
  level: LogLevel;
  timestamp: Date;
  message: string;
  context?: string;
  data?: unknown;
}

@Injectable({
  providedIn: 'root'
})
export class LoggerService {
  private minLevel: LogLevel = isDevMode() ? LogLevel.Debug : LogLevel.Info;
  private logHistory: LogEntry[] = [];
  private maxHistorySize = 100;

  debug(message: string, context?: string, data?: unknown): void {
    this.log(LogLevel.Debug, message, context, data);
  }

  info(message: string, context?: string, data?: unknown): void {
    this.log(LogLevel.Info, message, context, data);
  }

  warn(message: string, context?: string, data?: unknown): void {
    this.log(LogLevel.Warn, message, context, data);
  }

  error(message: string, context?: string, data?: unknown): void {
    this.log(LogLevel.Error, message, context, data);
  }

  getHistory(): LogEntry[] {
    return [...this.logHistory];
  }

  clearHistory(): void {
    this.logHistory = [];
  }

  setMinLevel(level: LogLevel): void {
    this.minLevel = level;
  }

  private log(level: LogLevel, message: string, context?: string, data?: unknown): void {
    if (level < this.minLevel) {
      return;
    }

    const entry: LogEntry = {
      level,
      timestamp: new Date(),
      message,
      context,
      data
    };

    // Store in history
    this.logHistory.push(entry);
    if (this.logHistory.length > this.maxHistorySize) {
      this.logHistory.shift();
    }

    // Console output
    const prefix = context ? `[${context}]` : '';
    const formattedMessage = `${prefix} ${message}`;

    switch (level) {
      case LogLevel.Debug:
        console.debug(formattedMessage, data ?? '');
        break;
      case LogLevel.Info:
        console.info(formattedMessage, data ?? '');
        break;
      case LogLevel.Warn:
        console.warn(formattedMessage, data ?? '');
        break;
      case LogLevel.Error:
        console.error(formattedMessage, data ?? '');
        break;
    }
  }
}
