import {
  HttpErrorResponse,
  HttpEvent,
  HttpEventType,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, tap, switchMap, of, catchError } from 'rxjs';
import { Router } from '@angular/router';
import { inject } from '@angular/core';

export function authInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const router = inject(Router);
  
  // Check for dev cookie to add a custom header
  const cookies = document.cookie.split(';').map(c => c.trim());
  const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
  
  // Clone the request if we have a dev cookie to explicitly mark it
  if (devCookie && request.url.startsWith('/api')) {
    // This is optional but helpful for debugging
    request = request.clone({
      setHeaders: {
        'X-Using-Dev-Cookie': 'true',
      }
    });
  }
  
  return next(request).pipe(
    catchError(error => {
      // Handle authentication errors
      if (error instanceof HttpErrorResponse) {
        if (error.status === 401) {
          // Check for dev cookie directly to avoid circular dependency
          const cookies = document.cookie.split(';').map(c => c.trim());
          const devCookie = cookies.find(c => c.startsWith('dev-user-email='));
          
          if (devCookie) {
            console.warn('[AUTH-INTERCEPTOR] 401 error despite dev cookie authentication');
            
            // If the URL includes specific endpoints that should work with dev auth,
            // we can attempt to reload the page to fix authentication
            if (!request.url.includes('/dev-login')) {
              // In a real implementation, consider using retry logic instead of a full page reload
              setTimeout(() => window.location.reload(), 500);
            }
            
            // Return the error for dev cookie case
            return of(error);
          } else if (!router.url.includes('/login')) {
            // Not using dev authentication, navigate to login
            router.navigate(['login']);
            return of(error);
          }
          return of(error);
        } else if (error.status === 403) {
          console.error('[AUTH-INTERCEPTOR] Access forbidden. You do not have permission to access this resource.');
          // Optional: Navigate to a custom forbidden page
          // router.navigate(['forbidden']);
        }
      }
      
      // Re-throw the error for other interceptors
      return of(error);
    })
  );
}
