import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, throwError, catchError } from 'rxjs';
import { inject } from '@angular/core';

export function authInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const cookies = document.cookie.split(';').map((c) => c.trim());
  const devCookie = cookies.find((c) => c.startsWith('dev-user-email='));

  if (devCookie && request.url.startsWith('/api')) {
    request = request.clone({
      setHeaders: { 'X-Using-Dev-Cookie': 'true' },
    });
  }

  return next(request).pipe(
    catchError(error => {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 401) {
          const cookiesNow = document.cookie.split(';').map(c => c.trim());
          const hasDevCookie = cookiesNow.some(c => c.startsWith('dev-user-email='));

          if (hasDevCookie && !request.url.includes('/dev-login')) {
            setTimeout(() => window.location.reload(), 500);
          }
        }
      }

      return throwError(() => error);
    })
  );
}
