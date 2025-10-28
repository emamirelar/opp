import { TestBed } from '@angular/core/testing';
import { LoggerService, LogLevel } from './logger.service';

describe('LoggerService', () => {
  let service: LoggerService;
  let consoleDebugSpy: jasmine.Spy;
  let consoleInfoSpy: jasmine.Spy;
  let consoleWarnSpy: jasmine.Spy;
  let consoleErrorSpy: jasmine.Spy;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [LoggerService]
    });

    service = TestBed.inject(LoggerService);
    
    // Spy on console methods
    consoleDebugSpy = spyOn(console, 'debug');
    consoleInfoSpy = spyOn(console, 'info');
    consoleWarnSpy = spyOn(console, 'warn');
    consoleErrorSpy = spyOn(console, 'error');
    
    // Clear history before each test
    service.clearHistory();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('debug logging', () => {
    it('should log debug messages', () => {
      service.debug('Debug message');
      
      expect(consoleDebugSpy).toHaveBeenCalled();
      expect(service.getHistory().length).toBe(1);
      expect(service.getHistory()[0].level).toBe(LogLevel.Debug);
      expect(service.getHistory()[0].message).toBe('Debug message');
    });

    it('should log debug with context', () => {
      service.debug('Debug message', 'TestContext');
      
      expect(consoleDebugSpy).toHaveBeenCalledWith('[TestContext] Debug message', '');
    });

    it('should log debug with data', () => {
      const testData = { key: 'value' };
      service.debug('Debug message', 'TestContext', testData);
      
      expect(consoleDebugSpy).toHaveBeenCalledWith('[TestContext] Debug message', testData);
      expect(service.getHistory()[0].data).toEqual(testData);
    });
  });

  describe('info logging', () => {
    it('should log info messages', () => {
      service.info('Info message');
      
      expect(consoleInfoSpy).toHaveBeenCalled();
      expect(service.getHistory().length).toBe(1);
      expect(service.getHistory()[0].level).toBe(LogLevel.Info);
    });

    it('should log info with context and data', () => {
      const data = { info: 'data' };
      service.info('Info message', 'InfoContext', data);
      
      expect(consoleInfoSpy).toHaveBeenCalledWith('[InfoContext] Info message', data);
    });
  });

  describe('warn logging', () => {
    it('should log warning messages', () => {
      service.warn('Warning message');
      
      expect(consoleWarnSpy).toHaveBeenCalled();
      expect(service.getHistory().length).toBe(1);
      expect(service.getHistory()[0].level).toBe(LogLevel.Warn);
    });

    it('should log warn with context', () => {
      service.warn('Warning', 'WarnContext');
      
      expect(consoleWarnSpy).toHaveBeenCalledWith('[WarnContext] Warning', '');
    });
  });

  describe('error logging', () => {
    it('should log error messages', () => {
      service.error('Error message');
      
      expect(consoleErrorSpy).toHaveBeenCalled();
      expect(service.getHistory().length).toBe(1);
      expect(service.getHistory()[0].level).toBe(LogLevel.Error);
    });

    it('should log error with context and data', () => {
      const errorData = { error: 'details' };
      service.error('Error occurred', 'ErrorContext', errorData);
      
      expect(consoleErrorSpy).toHaveBeenCalledWith('[ErrorContext] Error occurred', errorData);
      expect(service.getHistory()[0].data).toEqual(errorData);
    });
  });

  describe('log history', () => {
    it('should maintain log history', () => {
      service.info('Message 1');
      service.warn('Message 2');
      service.error('Message 3');
      
      const history = service.getHistory();
      expect(history.length).toBe(3);
      expect(history[0].message).toBe('Message 1');
      expect(history[1].message).toBe('Message 2');
      expect(history[2].message).toBe('Message 3');
    });

    it('should include timestamps in history', () => {
      const before = new Date();
      service.info('Test message');
      const after = new Date();
      
      const history = service.getHistory();
      expect(history[0].timestamp).toBeInstanceOf(Date);
      expect(history[0].timestamp.getTime()).toBeGreaterThanOrEqual(before.getTime());
      expect(history[0].timestamp.getTime()).toBeLessThanOrEqual(after.getTime());
    });

    it('should clear log history', () => {
      service.info('Message 1');
      service.warn('Message 2');
      
      expect(service.getHistory().length).toBe(2);
      
      service.clearHistory();
      
      expect(service.getHistory().length).toBe(0);
    });

    it('should limit history size to 100 entries', () => {
      // Add 105 log entries
      for (let i = 0; i < 105; i++) {
        service.info(`Message ${i}`);
      }
      
      const history = service.getHistory();
      expect(history.length).toBe(100);
      // Should have removed oldest entries
      expect(history[0].message).toBe('Message 5');
      expect(history[99].message).toBe('Message 104');
    });

    it('should return a copy of history, not reference', () => {
      service.info('Message 1');
      
      const history1 = service.getHistory();
      const history2 = service.getHistory();
      
      expect(history1).not.toBe(history2);
      expect(history1).toEqual(history2);
    });
  });

  describe('log level filtering', () => {
    it('should respect minimum log level', () => {
      service.setMinLevel(LogLevel.Warn);
      
      service.debug('Debug message');
      service.info('Info message');
      service.warn('Warn message');
      service.error('Error message');
      
      const history = service.getHistory();
      expect(history.length).toBe(2);
      expect(history[0].level).toBe(LogLevel.Warn);
      expect(history[1].level).toBe(LogLevel.Error);
    });

    it('should not log anything when level is None', () => {
      service.setMinLevel(LogLevel.None);
      
      service.debug('Debug');
      service.info('Info');
      service.warn('Warn');
      service.error('Error');
      
      expect(service.getHistory().length).toBe(0);
    });

    it('should log all levels when set to Debug', () => {
      service.setMinLevel(LogLevel.Debug);
      
      service.debug('Debug');
      service.info('Info');
      service.warn('Warn');
      service.error('Error');
      
      expect(service.getHistory().length).toBe(4);
    });

    it('should filter out debug when level is Info', () => {
      service.setMinLevel(LogLevel.Info);
      
      service.debug('Debug message');
      service.info('Info message');
      
      const history = service.getHistory();
      expect(history.length).toBe(1);
      expect(history[0].level).toBe(LogLevel.Info);
    });

    it('should filter out info and debug when level is Warn', () => {
      service.setMinLevel(LogLevel.Warn);
      
      service.debug('Debug');
      service.info('Info');
      service.warn('Warn');
      
      const history = service.getHistory();
      expect(history.length).toBe(1);
      expect(history[0].level).toBe(LogLevel.Warn);
    });
  });

  describe('console output formatting', () => {
    it('should format messages without context', () => {
      service.info('Simple message');
      
      expect(consoleInfoSpy).toHaveBeenCalledWith(' Simple message', '');
    });

    it('should format messages with context', () => {
      service.warn('Warning', 'MyContext');
      
      expect(consoleWarnSpy).toHaveBeenCalledWith('[MyContext] Warning', '');
    });

    it('should handle empty context', () => {
      service.error('Error', '');
      
      expect(consoleErrorSpy).toHaveBeenCalledWith(' Error', '');
    });

    it('should include data in console output', () => {
      const data = { test: 'value' };
      service.info('Message', undefined, data);
      
      expect(consoleInfoSpy).toHaveBeenCalledWith(' Message', data);
    });
  });

  describe('edge cases', () => {
    it('should handle undefined context', () => {
      service.info('Message', undefined);
      
      expect(service.getHistory()[0].context).toBeUndefined();
    });

    it('should handle undefined data', () => {
      service.info('Message', 'Context', undefined);
      
      expect(service.getHistory()[0].data).toBeUndefined();
    });

    it('should handle complex data objects', () => {
      const complexData = {
        nested: {
          property: 'value',
          array: [1, 2, 3]
        },
        date: new Date()
      };
      
      service.debug('Complex', 'Test', complexData);
      
      expect(service.getHistory()[0].data).toEqual(complexData);
    });
  });
});

