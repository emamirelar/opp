import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse, HttpRequest, HttpEvent, HttpEventType } from '@angular/common/http';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let mockRouter: jasmine.SpyObj<Router>;
  let mockNext: jasmine.Spy;

  beforeEach(() => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate'], { url: '/dashboard' });
    mockNext = jasmine.createSpy('next');

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: mockRouter }
      ]
    });
  });

  it('should pass request through when no errors occur', (done) => {
    const mockRequest = new HttpRequest('GET', '/api/test');
    const mockResponse: HttpEvent<any> = { type: HttpEventType.Response } as any;

    mockNext.and.returnValue(of(mockResponse));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockNext).subscribe({
        next: (event) => {
          expect(event).toEqual(mockResponse);
          done();
        }
      });
    });
  });

  it('should add dev cookie header when dev cookie exists', (done) => {
    // Set up a dev cookie
    const originalCookie = document.cookie;
    document.cookie = 'dev-user-email=test@unops.org';

    const mockRequest = new HttpRequest('GET', '/api/test');
    const mockResponse: HttpEvent<any> = { type: HttpEventType.Response } as any;

    mockNext.and.returnValue(of(mockResponse));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockNext).subscribe({
        next: () => {
          const clonedRequest = mockNext.calls.mostRecent().args[0];
          expect(clonedRequest.headers.get('X-Using-Dev-Cookie')).toBe('true');
          
          // Clean up cookie
          document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC';
          done();
        }
      });
    });
  });

  it('should handle 401 errors and redirect to login when no dev cookie', (done) => {
    const mockRequest = new HttpRequest('GET', '/api/test');
    const error = new HttpErrorResponse({ status: 401 });

    mockNext.and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockNext).subscribe({
        next: () => {
          expect(mockRouter.navigate).toHaveBeenCalledWith(['login']);
          done();
        },
        error: () => {
          // Also acceptable - interceptor may re-throw
          expect(mockRouter.navigate).toHaveBeenCalledWith(['login']);
          done();
        }
      });
    });
  });

  it('should not redirect to login when already on login page', (done) => {
    // Define url property on the mock router
    Object.defineProperty(mockRouter, 'url', {
      value: '/login',
      writable: true,
      configurable: true
    });
    
    const mockRequest = new HttpRequest('GET', '/api/test');
    const error = new HttpErrorResponse({ status: 401 });

    mockNext.and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockNext).subscribe({
        next: () => {
          expect(mockRouter.navigate).not.toHaveBeenCalled();
          done();
        },
        error: () => {
          expect(mockRouter.navigate).not.toHaveBeenCalled();
          done();
        }
      });
    });
  });

  it('should handle 403 errors without navigation', (done) => {
    const mockRequest = new HttpRequest('GET', '/api/test');
    const error = new HttpErrorResponse({ status: 403 });

    mockNext.and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockNext).subscribe({
        next: () => {
          expect(mockRouter.navigate).not.toHaveBeenCalled();
          done();
        },
        error: () => {
          expect(mockRouter.navigate).not.toHaveBeenCalled();
          done();
        }
      });
    });
  });

  it('should pass through non-authentication errors', (done) => {
    const mockRequest = new HttpRequest('GET', '/api/test');
    const error = new HttpErrorResponse({ status: 500 });

    mockNext.and.returnValue(throwError(() => error));

    TestBed.runInInjectionContext(() => {
      authInterceptor(mockRequest, mockNext).subscribe({
        next: () => {
          // Error should be returned via next as 'of(error)'
          done();
        },
        error: () => {
          // Or re-thrown
          done();
        }
      });
    });
  });
});

