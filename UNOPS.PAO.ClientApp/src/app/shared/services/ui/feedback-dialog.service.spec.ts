import { TestBed } from '@angular/core/testing';
import { MessageService } from 'primeng/api';
import { FeedbackDialogService } from './feedback-dialog.service';
import { FeedbackConfig } from '@shared/interfaces/feedback';

describe('FeedbackDialogService', () => {
  let service: FeedbackDialogService;
  let mockMessageService: jasmine.SpyObj<MessageService>;

  beforeEach(() => {
    mockMessageService = jasmine.createSpyObj('MessageService', ['add', 'clear']);

    TestBed.configureTestingModule({
      providers: [
        FeedbackDialogService,
        { provide: MessageService, useValue: mockMessageService }
      ]
    });

    service = TestBed.inject(FeedbackDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should show success toast', () => {
    const options: FeedbackConfig = {
      summary: 'Success',
      detail: 'Operation completed successfully'
    };

    service.showSuccessToast(options);

    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'success',
      summary: 'Success',
      detail: 'Operation completed successfully',
      life: 3000,
      closable: true,
      sticky: false
    });
  });

  it('should show info toast', () => {
    const options: FeedbackConfig = {
      summary: 'Info',
      detail: 'Information message'
    };

    service.showInfoToast(options);

    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'info',
      summary: 'Info',
      detail: 'Information message',
      life: 3000,
      closable: true,
      sticky: false
    });
  });

  it('should show warning toast', () => {
    const options: FeedbackConfig = {
      summary: 'Warning',
      detail: 'Warning message'
    };

    service.showWarningToast(options);

    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'warn',
      summary: 'Warning',
      detail: 'Warning message',
      life: 3000,
      closable: true,
      sticky: false
    });
  });

  it('should show error toast', () => {
    const options: FeedbackConfig = {
      summary: 'Error',
      detail: 'Error message'
    };

    service.showErrorToast(options);

    expect(mockMessageService.add).toHaveBeenCalledWith({
      severity: 'error',
      summary: 'Error',
      detail: 'Error message',
      life: 3000,
      closable: true,
      sticky: false
    });
  });

  it('should use default summary if not provided', () => {
    const options: FeedbackConfig = {
      detail: 'Test message'
    };

    service.showSuccessToast(options);

    expect(mockMessageService.add).toHaveBeenCalledWith(
      jasmine.objectContaining({
        summary: 'Success'
      })
    );
  });

  it('should use custom life time', () => {
    const options: FeedbackConfig = {
      summary: 'Test',
      detail: 'Test message',
      life: 5000
    };

    service.showInfoToast(options);

    expect(mockMessageService.add).toHaveBeenCalledWith(
      jasmine.objectContaining({
        life: 5000
      })
    );
  });

  it('should clear all messages', () => {
    service.clearAll();

    expect(mockMessageService.clear).toHaveBeenCalled();
  });

  it('should show error dialog', (done) => {
    const options: FeedbackConfig = {
      summary: 'Error',
      detail: 'Error details'
    };

    service.getErrorDialogState().subscribe(state => {
      if (state) {
        expect(state.summary).toBe('Error');
        expect(state.detail).toBe('Error details');
        done();
      }
    });

    service.showErrorDialog(options);
  });

  it('should hide error dialog', (done) => {
    let callCount = 0;
    service.getErrorDialogState().subscribe(state => {
      callCount++;
      // Skip initial null emission, wait for hideErrorDialog emission
      if (callCount === 2) {
        expect(state).toBeNull();
        done();
      }
    });

    service.hideErrorDialog();
  });

  it('should show confirm dialog with callback', (done) => {
    const options: FeedbackConfig = {
      summary: 'Confirm',
      detail: 'Are you sure?'
    };
    const callback = jasmine.createSpy('callback');

    service.getDialogState().subscribe(state => {
      if (state) {
        expect(state.summary).toBe('Confirm');
        expect(state.onConfirm).toBeDefined();
        done();
      }
    });

    service.showConfirmDialog(options, callback);
  });

  // TODO: Add tests for sticky messages
  // TODO: Add tests for non-closable messages
});

