import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, of, catchError, from, switchMap } from 'rxjs';
import { Router } from '@angular/router';
import { inject } from '@angular/core';
import { IapSessionRefreshService } from '@core/services/auth';

export function authInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const router = inject(Router);
  const iapSessionRefresh = inject(IapSessionRefreshService);

  // Check for dev cookie to add a custom header
  const cookies = document.cookie.split(';').map((c) => c.trim());
  const devCookie = cookies.find((c) => c.startsWith('dev-user-email='));

  // Clone the request if we have a dev cookie to explicitly mark it
  if (devCookie && request.url.startsWith('/api')) {
    request = request.clone({
      setHeaders: {
        'X-Using-Dev-Cookie': 'true',
      },
    });
  }

  return next(request).pipe(
    catchError((error) => {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 401) {
          const cookiesNow = document.cookie.split(';').map((c) => c.trim());
          const hasDevCookie = cookiesNow.some((c) => c.startsWith('dev-user-email='));

          if (hasDevCookie) {
            console.warn('[AUTH-INTERCEPTOR] 401 error despite dev cookie authentication');
            if (!request.url.includes('/dev-login')) {
              setTimeout(() => window.location.reload(), 500);
            }
            return of(error);
          }

          if (router.url.includes('/login')) {
            return of(error);
          }

          // Try IAP session refresh before redirecting to login
          if (iapSessionRefresh.shouldRun()) {
            return from(iapSessionRefresh.refreshSession()).pipe(
              switchMap((refreshed) => {
                if (refreshed) {
                  return next(request);
                }
                router.navigate(['login']);
                return of(error);
              }),
              catchError(() => {
                router.navigate(['login']);
                return of(error);
              })
            );
          }

          router.navigate(['login']);
          return of(error);
        }

        if (error.status === 403) {
          console.error(
            '[AUTH-INTERCEPTOR] Access forbidden. You do not have permission to access this resource.'
          );
        }
      }

      return of(error);
    })
  );
}
