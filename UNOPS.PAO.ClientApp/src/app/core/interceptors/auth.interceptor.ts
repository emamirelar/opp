import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, catchError, from, switchMap, throwError } from 'rxjs';
import { Router } from '@angular/router';
import { inject } from '@angular/core';
import { IapSessionRefreshService } from '@core/services/auth';

export function authInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const router = inject(Router);
  const iapSessionRefresh = inject(IapSessionRefreshService);

  const cookies = document.cookie.split(';').map((c) => c.trim());
  const devCookie = cookies.find((c) => c.startsWith('dev-user-email='));

  const headers: Record<string, string> = {};

  // Google IAP: X-Requested-With tells IAP the request is from JavaScript (AJAX).
  // Without it, IAP may return 302 redirect instead of 401, causing CORS errors.
  headers['X-Requested-With'] = 'XMLHttpRequest';

  // Ensure UTF-8 encoding for API responses (fixes accented characters like Ángel María displaying as ??)
  if (request.url.startsWith('/api') || request.url.includes('/api/')) {
    headers['Accept-Charset'] = 'utf-8';
  }

  if (devCookie && request.url.startsWith('/api')) {
    headers['X-Using-Dev-Cookie'] = 'true';
  }

  request = request.clone({ setHeaders: headers });

  return next(request).pipe(
    catchError((error) => {
      if (error instanceof HttpErrorResponse) {
        if (error.status === 401) {
          const cookiesNow = document.cookie.split(';').map((c) => c.trim());
          const hasDevCookie = cookiesNow.some((c) => c.startsWith('dev-user-email='));

          if (hasDevCookie) {
            if (!request.url.includes('/dev-login')) {
              setTimeout(() => window.location.reload(), 500);
            }
            return throwError(() => error);
          }

          if (router.url.includes('/login')) {
            return throwError(() => error);
          }

          if (request.url.includes('check-iap-simulation')) {
            return throwError(() => error);
          }

          // Skip refresh for verification requests to avoid loops
          if (request.url.includes('/user/claims') || request.url.includes('favicon.ico')) {
            return throwError(() => error);
          }

          // Background requests fail silently
          if (isBackgroundRequest(request)) {
            return throwError(() => error);
          }

          if (iapSessionRefresh.shouldRun()) {
            return from(iapSessionRefresh.refreshSession()).pipe(
              switchMap((refreshed) => {
                if (refreshed) {
                  return next(request);
                }
                router.navigate(['login']);
                return throwError(() => error);
              }),
              catchError(() => {
                router.navigate(['login']);
                return throwError(() => error);
              })
            );
          }

          router.navigate(['login']);
          return throwError(() => error);
        }
      }

      return throwError(() => error);
    })
  );
}

/**
 * Background polling requests should not trigger redirects on 401.
 * They fail silently and the next user-initiated request handles auth.
 */
function isBackgroundRequest(request: HttpRequest<unknown>): boolean {
  return request.url.includes('/notifications') ||
    request.url.includes('/api/notifications');
}
