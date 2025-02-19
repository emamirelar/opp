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
        //Redirect unauthorized requests to login page.
        if (err instanceof HttpErrorResponse && err.status === 401) {
          router.navigate(['login']);
        }
      },
    }),
  );
}
