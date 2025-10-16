import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { inject } from '@angular/core';
import { ErrorHandlerService } from '@shared/services/utils';

export function serverErrorInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const errorHandler = inject(ErrorHandlerService);

  return next(request).pipe(
    tap({
      next: (event) => {},
      error: (err) => {
        if (err instanceof HttpErrorResponse) {
          errorHandler.handleHttpError(err);
        }
      },
    }),
  );
}
