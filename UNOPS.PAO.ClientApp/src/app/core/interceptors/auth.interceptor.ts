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
            console.warn('[AUTH-INTERCEPTOR] 401 error despite dev cookie authentication');
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

          // Skip refresh attempts from the refresh verification itself to avoid loops
          if (request.url.includes('/user/claims') || request.url.includes('favicon.ico')) {
            return throwError(() => error);
          }

          if (iapSessionRefresh.shouldRun()) {
            console.log('[AUTH-INTERCEPTOR] 401 received - attempting IAP session refresh', {
              failedRequestUrl: request.url,
              method: request.method,
            });

            // All concurrent 401s share the same refresh promise (no race condition)
            return from(iapSessionRefresh.refreshSession()).pipe(
              switchMap((refreshed) => {
                if (refreshed) {
                  console.log('[AUTH-INTERCEPTOR] Session refresh succeeded - retrying request', {
                    url: request.url,
                  });
                  return next(request);
                }
                // Only redirect for user-initiated requests, not background polls
                if (isBackgroundRequest(request)) {
                  console.warn('[AUTH-INTERCEPTOR] Session refresh failed for background request - suppressing redirect', {
                    url: request.url,
                  });
                  return throwError(() => error);
                }
                console.warn('[AUTH-INTERCEPTOR] Session refresh failed - redirecting to login');
                router.navigate(['login']);
                return throwError(() => error);
              }),
              catchError((refreshErr) => {
                if (isBackgroundRequest(request)) {
                  console.warn('[AUTH-INTERCEPTOR] Session refresh threw for background request - suppressing redirect', {
                    url: request.url,
                    error: refreshErr,
                  });
                  return throwError(() => error);
                }
                console.warn('[AUTH-INTERCEPTOR] Session refresh threw - redirecting to login', {
                  error: refreshErr,
                });
                router.navigate(['login']);
                return throwError(() => error);
              })
            );
          }

          if (isBackgroundRequest(request)) {
            return throwError(() => error);
          }

          console.log('[AUTH-INTERCEPTOR] 401 - shouldRun=false, redirecting to login');
          router.navigate(['login']);
          return throwError(() => error);
        }

        if (error.status === 403) {
          console.error(
            '[AUTH-INTERCEPTOR] Access forbidden. You do not have permission to access this resource.'
          );
        }
      }

      return throwError(() => error);
    })
  );
}

/**
 * Background polling requests should not trigger login redirects on 401.
 * They should fail silently and let the next user-initiated request handle auth.
 */
function isBackgroundRequest(request: HttpRequest<unknown>): boolean {
  return request.url.includes('/notifications') ||
    request.url.includes('/api/notifications');
}
