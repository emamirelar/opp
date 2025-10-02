import { HttpErrorResponse } from '@angular/common/http';

export interface AppError {
  status: number;
  title: string;
  detail: string;
  timestamp: Date;
  url?: string;
  context?: string;
  stackTrace?: string;
  validationErrors?: Record<string, string[]>;
  missingFields?: string[];
}

export interface ErrorDialogConfig {
  closable?: boolean;
  showRefreshButton?: boolean;
}

export class ErrorParser {
  static parse(err: HttpErrorResponse, context?: string): AppError {
    const baseError: AppError = {
      status: err.status,
      title: '',
      detail: '',
      timestamp: new Date(),
      url: err.url || undefined,
      context,
    };

    // Network errors (no connection)
    if (err.status === 0) {
      return {
        ...baseError,
        title: 'error.networkError.title',
        detail: 'error.networkError.detail',
      };
    }

    // Server errors (500+)
    if (err.status >= 500) {
      return {
        ...baseError,
        title: err.error?.title || 'Server Error',
        detail: err.error?.detail || 'An unexpected server error occurred. Please try again later.',
        stackTrace: err.error?.stackTrace,
      };
    }

    // Client errors (400-499) with ProblemDetails format
    if (err.error && typeof err.error === 'object') {
      // Validation errors format
      if (err.error.errors) {
        return {
          ...baseError,
          title: err.error.title || 'Validation Error',
          detail: typeof err.error.errors === 'object'
            ? Object.entries(err.error.errors).map(([key, value]) => `${key}: ${value}`).join('\n')
            : JSON.stringify(err.error.errors),
          validationErrors: typeof err.error.errors === 'object' ? err.error.errors : undefined,
        };
      }

      // ProblemDetails format
      if (err.error.title) {
        return {
          ...baseError,
          title: err.error.title,
          detail: err.error.detail || 'An error occurred while processing your request.',
        };
      }

      // Simple error object format { error: "message" }
      if (err.error.error && typeof err.error.error === 'string') {
        return {
          ...baseError,
          title: `Error ${err.status}`,
          detail: err.error.error,
          missingFields: Array.isArray(err.error.missingFields) ? err.error.missingFields : undefined,
        };
      }

      // Fallback for other error objects
      return {
        ...baseError,
        title: `Error ${err.status}`,
        detail: err.error.message || err.message || 'An unexpected error occurred.',
      };
    }

    // Fallback for non-object errors
    return {
      ...baseError,
      title: `Error ${err.status}`,
      detail: err.message || 'An unexpected error occurred.',
    };
  }
}
