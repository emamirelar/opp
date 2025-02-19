import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { inject } from '@angular/core';
import { FeedbackDialogService } from '../../common/pages/services/feedback-dialog.service';

export function serverErrorInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  let feedbackService = inject(FeedbackDialogService);

  return next(request).pipe(
    tap({
      next: (event) => {},
      error: (err) => {
        if (err instanceof HttpErrorResponse && err.status !== 401) {
          if (
            err.hasOwnProperty('error') &&
            err['error'].hasOwnProperty('errors')
          ) {
            let title = err.error.title;
            let errorDetails = JSON.stringify(err.error.errors);
            feedbackService
              .showErrorToast({summary: title, detail: errorDetails})
          }
        }
      },
    }),
  );
}
