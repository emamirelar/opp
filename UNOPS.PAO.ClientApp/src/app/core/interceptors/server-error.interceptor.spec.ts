import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse, HttpRequest, HttpEvent, HttpEventType } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { serverErrorInterceptor } from './server-error.interceptor';
import { ErrorHandlerService } from '@shared/services/utils/error-handler.service';

describe('serverErrorInterceptor', () => {
  let mockErrorHandler: jasmine.SpyObj<ErrorHandlerService>;
  let mockNext: jasmine.Spy;

  beforeEach(() => {
    mockErrorHandler = jasmine.createSpyObj('ErrorHandlerService', ['handleHttpError']);
    mockNext = jasmine.createSpy('next');

    TestBed.configureTestingModule({
      providers: [
        { provide: ErrorHandlerService, useValue: mockErrorHandler }
      ]
    });
  });

  it('should pass request through when no errors occur', (done) => {
    const mockRequest = new HttpRequest('GET', '/api/test');
    const mockResponse: HttpEvent<any> = { type: HttpEventType.Response } as any;

    mockNext.and.returnValue(of(mockResponse));

    TestBed.runInInjectionContext(() => {
      serverErrorInterceptor(mockRequest, mockNext).subscribe({
        next: (event) => {
          expect(event).toEqual(mockResponse);
          expect(mockErrorHandler.handleHttpError).not.toHaveBeenCalled();
          done();
        }
      });
    });
  });

  it('should call error handler service on HTTP errors', (done) => {
    const mockRequest = new HttpRequest('GET', '/api/test');
    const error = new HttpErrorResponse({ 
      status: 500, 
      statusText: 'Internal Server Error' 
    });

    mockNext.and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      serverErrorInterceptor(mockRequest, mockNext).subscribe({
        error: (err) => {
          expect(mockErrorHandler.handleHttpError).toHaveBeenCalledWith(error);
          done();
        }
      });
    });
  });

  it('should handle 400 Bad Request errors', (done) => {
    const mockRequest = new HttpRequest('POST', '/api/test', { data: 'test' });
    const error = new HttpErrorResponse({ 
      status: 400, 
      statusText: 'Bad Request',
      error: { message: 'Invalid data' }
    });

    mockNext.and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      serverErrorInterceptor(mockRequest, mockNext).subscribe({
        error: (err) => {
          expect(mockErrorHandler.handleHttpError).toHaveBeenCalledWith(error);
          expect(err.status).toBe(400);
          done();
        }
      });
    });
  });

  it('should handle 404 Not Found errors', (done) => {
    const mockRequest = new HttpRequest('GET', '/api/test/123');
    const error = new HttpErrorResponse({ 
      status: 404, 
      statusText: 'Not Found' 
    });

    mockNext.and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      serverErrorInterceptor(mockRequest, mockNext).subscribe({
        error: (err) => {
          expect(mockErrorHandler.handleHttpError).toHaveBeenCalledWith(error);
          expect(err.status).toBe(404);
          done();
        }
      });
    });
  });

  it('should handle 500 Internal Server errors', (done) => {
    const mockRequest = new HttpRequest('POST', '/api/test', { data: 'test' });
    const error = new HttpErrorResponse({ 
      status: 500, 
      statusText: 'Internal Server Error',
      error: { message: 'Database connection failed' }
    });

    mockNext.and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      serverErrorInterceptor(mockRequest, mockNext).subscribe({
        error: (err) => {
          expect(mockErrorHandler.handleHttpError).toHaveBeenCalledWith(error);
          expect(err.status).toBe(500);
          done();
        }
      });
    });
  });

  it('should not call error handler for non-HTTP errors', (done) => {
    const mockRequest = new HttpRequest('GET', '/api/test');
    const error = new Error('Network error');

    mockNext.and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      serverErrorInterceptor(mockRequest, mockNext).subscribe({
        error: (err) => {
          expect(mockErrorHandler.handleHttpError).not.toHaveBeenCalled();
          done();
        }
      });
    });
  });
});

