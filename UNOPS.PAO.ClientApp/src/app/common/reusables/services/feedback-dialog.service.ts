import { Injectable } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FeedbackConfig } from '../interfaces/feedback';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';

@Injectable({
  providedIn: 'root',
})
export class FeedbackDialogService {
  private dialogState = new BehaviorSubject<FeedbackConfig | null>(null);

  constructor(private primeNgMessageService: MessageService) {}

  /**
   * Shows a success toast message
   * @param options Message options
   */
  showSuccessToast(options: FeedbackConfig) {
    this.primeNgMessageService.add({
      severity: 'success',
      summary: options.summary || 'Success',
      detail: options.detail,
      life: options.life || 3000,
      closable: options.closable !== undefined ? options.closable : true,
      sticky: options.sticky || false,
    });
  }

  /**
   * Shows an info toast message
   * @param options Message options
   */
  showInfoToast(options: FeedbackConfig) {
    this.primeNgMessageService.add({
      severity: 'info',
      summary: options.summary || 'Information',
      detail: options.detail,
      life: options.life || 3000,
      closable: options.closable !== undefined ? options.closable : true,
      sticky: options.sticky || false,
    });
  }

  /**
   * Shows a warning toast message
   * @param options Message options
   */
  showWarningToast(options: FeedbackConfig) {
    this.primeNgMessageService.add({
      severity: 'warn',
      summary: options.summary || 'Warning',
      detail: options.detail,
      life: options.life || 3000,
      closable: options.closable !== undefined ? options.closable : true,
      sticky: options.sticky || false,
    });
  }

  /**
   * Shows an error toast message
   * @param options Message options
   */
  showErrorToast(options: FeedbackConfig) {
    this.primeNgMessageService.add({
      severity: 'error',
      summary: options.summary || 'Error',
      detail: options.detail,
      life: options.life || 3000,
      closable: options.closable !== undefined ? options.closable : true,
      sticky: options.sticky || false,
    });
  }

  /**
   * Clears all currently displayed messages
   */
  clearAll() {
    this.primeNgMessageService.clear();
  }

  showErrorDialog(options: FeedbackConfig) {
    this.dialogState.next({ summary: options.summary || 'Error', ...options });
  }

  showConfirmDialog(options: FeedbackConfig, callback: () => void) {
    this.dialogState.next({ onConfirm: callback, ...options });
  }

  hideDialog() {
    this.dialogState.next(null);
  }

  getDialogState() {
    return this.dialogState.asObservable();
  }
}
