import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { serverErrorInterceptor } from './server-error.interceptor';
import { ErrorHandlerService } from '@shared/services/utils';

describe('serverErrorInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let mockErrorHandler: jasmine.SpyObj<ErrorHandlerService>;

  beforeEach(() => {
    mockErrorHandler = jasmine.createSpyObj('ErrorHandlerService', ['handleHttpError']);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([serverErrorInterceptor])),
        provideHttpClientTesting(),
        { provide: ErrorHandlerService, useValue: mockErrorHandler },
      ],
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should call error handler on HTTP error', (done) => {
    httpClient.get('/api/test').subscribe({
      next: () => fail('Should have failed'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(500);
        expect(mockErrorHandler.handleHttpError).toHaveBeenCalledWith(error);
        done();
      },
    });

    const req = httpMock.expectOne('/api/test');
    req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });
  });

  it('should call error handler on 400 errors', (done) => {
    httpClient.get('/api/test').subscribe({
      next: () => fail('Should have failed'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(400);
        expect(mockErrorHandler.handleHttpError).toHaveBeenCalledWith(error);
        done();
      },
    });

    const req = httpMock.expectOne('/api/test');
    req.flush('Bad Request', { status: 400, statusText: 'Bad Request' });
  });

  it('should call error handler on 404 errors', (done) => {
    httpClient.get('/api/test').subscribe({
      next: () => fail('Should have failed'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(404);
        expect(mockErrorHandler.handleHttpError).toHaveBeenCalledWith(error);
        done();
      },
    });

    const req = httpMock.expectOne('/api/test');
    req.flush('Not Found', { status: 404, statusText: 'Not Found' });
  });

  it('should call error handler on network errors', (done) => {
    httpClient.get('/api/test').subscribe({
      next: () => fail('Should have failed'),
      error: (error: HttpErrorResponse) => {
        expect(error.status).toBe(0);
        expect(mockErrorHandler.handleHttpError).toHaveBeenCalledWith(error);
        done();
      },
    });

    const req = httpMock.expectOne('/api/test');
    req.error(new ProgressEvent('error'), { status: 0, statusText: 'Unknown Error' });
  });

  it('should allow successful requests to pass through without calling error handler', (done) => {
    httpClient.get('/api/test').subscribe({
      next: (response) => {
        expect(response).toEqual({ data: 'success' });
        expect(mockErrorHandler.handleHttpError).not.toHaveBeenCalled();
        done();
      },
      error: () => fail('Should not have failed'),
    });

    const req = httpMock.expectOne('/api/test');
    req.flush({ data: 'success' });
  });

  it('should handle multiple concurrent errors', (done) => {
    let errorCount = 0;
    const totalRequests = 3;

    for (let i = 0; i < totalRequests; i++) {
      httpClient.get(`/api/test${i}`).subscribe({
        next: () => fail('Should have failed'),
        error: () => {
          errorCount++;
          if (errorCount === totalRequests) {
            expect(mockErrorHandler.handleHttpError).toHaveBeenCalledTimes(totalRequests);
            done();
          }
        },
      });
    }

    for (let i = 0; i < totalRequests; i++) {
      const req = httpMock.expectOne(`/api/test${i}`);
      req.flush('Error', { status: 500, statusText: 'Internal Server Error' });
    }
  });
});

