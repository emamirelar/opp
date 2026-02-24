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

  const headers: Record<string, string> = {};

  // Google IAP: X-Requested-With tells IAP the request is from JavaScript (AJAX).
  // Without it, IAP may return 302 redirect instead of 401, causing CORS errors.
  // https://cloud.google.com/iap/docs/external-identity-sessions
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
            return of(error);
          }

          if (router.url.includes('/login')) {
            return of(error);
          }

          // Skip refresh for login page auth check - it determines if we have a session
          if (request.url.includes('check-iap-simulation')) {
            return of(error);
          }

          // Try IAP session refresh before redirecting to login
          if (iapSessionRefresh.shouldRun()) {
            console.log('[AUTH-INTERCEPTOR] 401 received - attempting IAP session refresh', {
              failedRequestUrl: request.url,
              method: request.method,
            });
            return from(iapSessionRefresh.refreshSession()).pipe(
              switchMap((refreshed) => {
                if (refreshed) {
                  console.log('[AUTH-INTERCEPTOR] Session refresh succeeded - retrying request', {
                    url: request.url,
                  });
                  return next(request);
                }
                console.warn('[AUTH-INTERCEPTOR] Session refresh failed - redirecting to login');
                router.navigate(['login']);
                return of(error);
              }),
              catchError((refreshErr) => {
                console.warn('[AUTH-INTERCEPTOR] Session refresh threw - redirecting to login', {
                  error: refreshErr,
                });
                router.navigate(['login']);
                return of(error);
              })
            );
          }

          console.log('[AUTH-INTERCEPTOR] 401 - shouldRun=false, redirecting to login');
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
