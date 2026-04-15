import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { inject } from '@angular/core';

export function authInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const router = inject(Router);

  const cookies = document.cookie.split(';').map(c => c.trim());
  const devCookie = cookies.find(c => c.startsWith('dev-user-email='));

  if (devCookie && request.url.startsWith('/api')) {
    request = request.clone({
      setHeaders: {
        'X-Using-Dev-Cookie': 'true',
      }
    });
  }

  return next(request).pipe(
    catchError(error => {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 401) {
          const innerCookies = document.cookie.split(';').map(c => c.trim());
          const innerDevCookie = innerCookies.find(c => c.startsWith('dev-user-email='));

          if (innerDevCookie) {
            console.warn('[AUTH-INTERCEPTOR] 401 error despite dev cookie authentication');
            if (!request.url.includes('/dev-login')) {
              setTimeout(() => window.location.reload(), 500);
            }
          } else if (!router.url.includes('/login')) {
            router.navigate(['login']);
          }
        } else if (error.status === 403) {
          console.error('[AUTH-INTERCEPTOR] Access forbidden. You do not have permission to access this resource.');
        }
      }

      // Always re-throw so downstream catchError handlers work correctly.
      // Using of(error) here would emit HttpErrorResponse as a value, which
      // Angular's HttpClient filters out, causing EmptyError in firstValueFrom().
      return throwError(() => error);
    })
  );
}
