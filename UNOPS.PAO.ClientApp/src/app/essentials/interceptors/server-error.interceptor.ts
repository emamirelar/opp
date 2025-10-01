import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
} from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { inject } from '@angular/core';
import { FeedbackDialogService } from '../../common/pages/services/feedback-dialog.service';
import { TranslateService } from '@ngx-translate/core';

export function serverErrorInterceptor(
  request: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  let feedbackService = inject(FeedbackDialogService);
  let translateService = inject(TranslateService);

  return next(request).pipe(
    tap({
      next: (event) => {},
      error: (err) => {
        if (err instanceof HttpErrorResponse && err.status !== 401) {
          let title: string;
          let detail: string;

          // Handle network errors
          if (err.status === 0) {
            // Show blocking dialog with refresh button
            feedbackService.showErrorDialog({
              closable: true,
              summary: translateService.instant('error.networkError.title'),
              detail: translateService.instant('error.networkError.detail'),
              showRefreshButton: true
            });
            return; // Exit early, don't show toast
          }
          // Handle 500 errors with ProblemDetails format
          else if (err.status >= 500) {
            title = err.error?.title || 'Server Error';
            detail = err.error?.detail || 'An unexpected server error occurred. Please try again later.';

            // In development, show stack trace if available
            if (err.error?.stackTrace) {
              detail += '\n\nStack Trace:\n' + err.error.stackTrace;
            }
          }
          // Handle 400-499 errors with ProblemDetails format
          else if (err.error && typeof err.error === 'object') {
            // Check for validation errors format (errors property)
            if (err.error.errors) {
              title = err.error.title || 'Validation Error';
              detail = typeof err.error.errors === 'object'
                ? Object.entries(err.error.errors).map(([key, value]) => `${key}: ${value}`).join('\n')
                : JSON.stringify(err.error.errors);
            }
            // Check for ProblemDetails format
            else if (err.error.title) {
              title = err.error.title;
              detail = err.error.detail || 'An error occurred while processing your request.';
            }
            // Check for simple error object format { error: "message" }
            else if (err.error.error && typeof err.error.error === 'string') {
              title = `Error ${err.status}`;
              detail = err.error.error;

              // Check for additional fields like missingFields
              if (err.error.missingFields && Array.isArray(err.error.missingFields)) {
                detail += '\n\nMissing fields:\n' + err.error.missingFields.join('\n');
              }
            }
            // Fallback for other error objects
            else {
              title = `Error ${err.status}`;
              detail = err.error.message || err.message || 'An unexpected error occurred.';
            }
          }
          // Fallback for non-object errors
          else {
            title = `Error ${err.status}`;
            detail = err.message || 'An unexpected error occurred.';
          }

          feedbackService.showErrorToast({
            summary: title,
            detail: detail,
            life: 5000 // 5 seconds for all errors
          });
        }
      },
    }),
  );
}
