import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { DialogService } from 'primeng/dynamicdialog';
import { ConfirmationService } from 'primeng/api';
import { ImportDialogService } from './import-dialog.service';
import { NotificationService } from '@shared/services/ui/notification.service';
import { LoadingOverlayService } from '@shared/components/layout/loading-overlay/loading-overlay.component';
import { ImportService } from '../import.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { ImportGoogleSheetService } from '../import-google-sheet.service';
import { of } from 'rxjs';

describe('ImportDialogService', () => {
  let service: ImportDialogService;

  beforeEach(() => {
    const dialogService = jasmine.createSpyObj('DialogService', ['open']);
    const notificationService = jasmine.createSpyObj('NotificationService', ['markAsRead']);
    const confirmationService = jasmine.createSpyObj('ConfirmationService', ['confirm']);
    const loadingOverlayService = jasmine.createSpyObj('LoadingOverlayService', ['show', 'hide']);
    const importService = jasmine.createSpyObj('ImportService', ['bulkUpload', 'analyzeFile', 'cancelAnalysis', 'getActiveJobId']);
    const feedbackDialogService = jasmine.createSpyObj('FeedbackDialogService', ['showWarningToast', 'showErrorToast', 'showInfoToast']);
    const importGoogleSheetService = jasmine.createSpyObj('ImportGoogleSheetService', ['openPicker']);

    notificationService.markAsRead.and.returnValue(of(undefined));
    importService.bulkUpload.and.returnValue(of({}));
    importService.analyzeFile.and.returnValue(of({}));
    importService.cancelAnalysis.and.returnValue(of({}));
    importService.getActiveJobId.and.returnValue(null);
    importGoogleSheetService.openPicker.and.returnValue(of('CANCELED'));

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: DialogService, useValue: dialogService },
        { provide: NotificationService, useValue: notificationService },
        { provide: ConfirmationService, useValue: confirmationService },
        { provide: LoadingOverlayService, useValue: loadingOverlayService },
        { provide: ImportService, useValue: importService },
        { provide: FeedbackDialogService, useValue: feedbackDialogService },
        { provide: ImportGoogleSheetService, useValue: importGoogleSheetService },
      ]
    });
    service = TestBed.inject(ImportDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for dialog opening
  // TODO: Add tests for dialog data management
});

