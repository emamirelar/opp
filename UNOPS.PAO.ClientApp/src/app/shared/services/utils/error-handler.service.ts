import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';
import { FeedbackDialogService } from '../ui/feedback-dialog.service';
import { AppError, ErrorParser } from '../../models/error.model';
import { LoggerService } from './logger.service';

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService {
  private feedbackService = inject(FeedbackDialogService);
  private translateService = inject(TranslateService);
  private logger = inject(LoggerService);

  handleHttpError(error: HttpErrorResponse, context?: string): void {
    // Skip 401 errors (handled by auth interceptor)
    if (error.status === 401) {
      return;
    }

    const appError = ErrorParser.parse(error, context);

    // Log the error
    this.logger.error(
      `HTTP ${appError.status}: ${appError.title}`,
      appError.context || 'HTTP',
      { detail: appError.detail, url: appError.url, error }
    );

    // Network errors show blocking dialog with refresh button
    if (appError.status === 0) {
      this.showNetworkErrorDialog(appError);
      return;
    }

    // All other errors show toast
    this.showErrorToast(appError);
  }

  private showNetworkErrorDialog(error: AppError): void {
    this.feedbackService.showErrorDialog({
      closable: true,
      summary: this.translateService.instant(error.title),
      detail: this.translateService.instant(error.detail),
      showRefreshButton: true
    });
  }

  private showErrorToast(error: AppError): void {
    let detail = error.detail;

    // Append stack trace in development
    if (error.stackTrace) {
      detail += '\n\nStack Trace:\n' + error.stackTrace;
    }

    // Append missing fields if present
    if (error.missingFields && error.missingFields.length > 0) {
      detail += '\n\nMissing fields:\n' + error.missingFields.join('\n');
    }

    this.feedbackService.showErrorToast({
      summary: error.title,
      detail: detail,
      life: 5000
    });
  }

  handleError(error: Error, context?: string): void {
    this.logger.error(error.message || 'An unexpected error occurred.', context || 'Application', error);

    this.feedbackService.showErrorToast({
      summary: 'Application Error',
      detail: error.message || 'An unexpected error occurred.',
      life: 5000
    });
  }
}
