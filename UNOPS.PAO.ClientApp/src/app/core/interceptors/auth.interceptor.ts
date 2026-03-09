import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, throwError, catchError } from 'rxjs';
import { Router } from '@angular/router';
import { inject } from '@angular/core';

export function authInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const router = inject(Router);

  const cookies = document.cookie.split(';').map((c) => c.trim());
  const devCookie = cookies.find((c) => c.startsWith('dev-user-email='));

  const setHeaders: Record<string, string> = {
    // Google IAP: X-Requested-With tells IAP the request is from JavaScript (AJAX).
    // Without it, IAP may return 302 redirect instead of 401, causing CORS errors.
    'X-Requested-With': 'XMLHttpRequest',
  };

  if (devCookie && request.url.startsWith('/api')) {
    setHeaders['X-Using-Dev-Cookie'] = 'true';
  }

  request = request.clone({ setHeaders });

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
            return throwError(() => error);
          } else if (!router.url.includes('/login')) {
            router.navigate(['login']);
            return throwError(() => error);
          }
          return throwError(() => error);
        }
      }
      
      return throwError(() => error);
    })
  );
}
