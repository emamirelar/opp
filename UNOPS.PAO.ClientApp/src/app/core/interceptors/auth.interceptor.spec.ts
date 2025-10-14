import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { Router } from '@angular/router';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(() => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate'], { url: '/dashboard' });

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: mockRouter },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);

    // Clear cookies before each test
    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  });

  afterEach(() => {
    httpMock.verify();
    // Clean up cookies
    document.cookie = 'dev-user-email=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
  });

  it('should add X-Using-Dev-Cookie header when dev cookie is present', () => {
    // Set dev cookie
    document.cookie = 'dev-user-email=test@example.com';

    httpClient.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('X-Using-Dev-Cookie')).toBeTrue();
    expect(req.request.headers.get('X-Using-Dev-Cookie')).toBe('true');

    req.flush({ success: true });
  });

  it('should not add X-Using-Dev-Cookie header when dev cookie is absent', () => {
    httpClient.get('/api/test').subscribe();

    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.has('X-Using-Dev-Cookie')).toBeFalse();

    req.flush({ success: true });
  });

  it('should not modify non-API requests', () => {
    httpClient.get('/external/api').subscribe();

    const req = httpMock.expectOne('/external/api');
    expect(req.request.headers.has('X-Using-Dev-Cookie')).toBeFalse();

    req.flush({ success: true });
  });

  it('should redirect to login on 401 error when not authenticated', (done) => {
    httpClient.get('/api/test').subscribe({
      next: () => fail('Should have failed with 401'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(401);
        expect(mockRouter.navigate).toHaveBeenCalledWith(['login']);
        done();
      },
    });

    const req = httpMock.expectOne('/api/test');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
  });

  it('should not redirect to login on 401 when already on login page', (done) => {
    mockRouter.url = '/login';

    httpClient.get('/api/test').subscribe({
      next: () => fail('Should have failed with 401'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(401);
        expect(mockRouter.navigate).not.toHaveBeenCalled();
        done();
      },
    });

    const req = httpMock.expectOne('/api/test');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
  });

  it('should handle 401 error with dev cookie present', (done) => {
    document.cookie = 'dev-user-email=test@example.com';
    
    // Mock setTimeout to avoid actual delays in tests
    jasmine.clock().install();

    httpClient.get('/api/test').subscribe({
      next: () => fail('Should have failed with 401'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(401);
        jasmine.clock().uninstall();
        done();
      },
    });

    const req = httpMock.expectOne('/api/test');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
  });

  it('should not reload page on 401 for dev-login endpoint', (done) => {
    document.cookie = 'dev-user-email=test@example.com';

    httpClient.get('/api/dev-login').subscribe({
      next: () => fail('Should have failed with 401'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(401);
        done();
      },
    });

    const req = httpMock.expectOne('/api/dev-login');
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
  });

  it('should log 403 errors without redirecting', (done) => {
    spyOn(console, 'error');

    httpClient.get('/api/test').subscribe({
      next: () => fail('Should have failed with 403'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(403);
        expect(console.error).toHaveBeenCalledWith(
          '[AUTH-INTERCEPTOR] Access forbidden. You do not have permission to access this resource.'
        );
        expect(mockRouter.navigate).not.toHaveBeenCalled();
        done();
      },
    });

    const req = httpMock.expectOne('/api/test');
    req.flush('Forbidden', { status: 403, statusText: 'Forbidden' });
  });

  it('should pass through other HTTP errors', (done) => {
    httpClient.get('/api/test').subscribe({
      next: () => fail('Should have failed with 500'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(500);
        expect(mockRouter.navigate).not.toHaveBeenCalled();
        done();
      },
    });

    const req = httpMock.expectOne('/api/test');
    req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
  });

  it('should allow successful requests to pass through', (done) => {
    httpClient.get('/api/test').subscribe({
      next: (response) => {
        expect(response).toEqual({ data: 'test' });
        done();
      },
      error: () => fail('Should not have failed'),
    });

    const req = httpMock.expectOne('/api/test');
    req.flush({ data: 'test' });
  });
});

