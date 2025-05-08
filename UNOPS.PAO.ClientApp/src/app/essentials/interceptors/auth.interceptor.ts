import {
  HttpErrorResponse,
  HttpEvent,
  HttpEventType,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { inject } from '@angular/core';

export function authInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  let router = inject(Router);

  return next(request).pipe(
    tap({
      next: (event) => {},
      error: (err) => {
        // Only redirect to login if:
        // 1. It's a 401 error
        // 2. We're not already on the login page
        // 3. The request is not for the login or Google sign-in endpoints
        if (err instanceof HttpErrorResponse && 
            err.status === 401 && 
            !router.url.includes('/login') &&
            !request.url.includes('/user/login') &&
            !request.url.includes('/user/googleSignIn')) {
          router.navigate(['login']);
        }
      },
    }),
  );
}
