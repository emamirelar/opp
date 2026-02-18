import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';
import { DrivePickerService } from '@shared/services/integration/drive-picker.service';
import { DocumentService } from '@shared/services/api/document.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { GDriveDocumentComponent } from './document-gdrive.component';

describe('GDriveDocumentComponent', () => {
  let component: GDriveDocumentComponent;
  let fixture: ComponentFixture<GDriveDocumentComponent>;

  beforeEach(async () => {
    const drivePickerService = {
      onFilesSelectedEmitter: of({ files: [] }),
      setAcceptedMIMETypes: jasmine.createSpy('setAcceptedMIMETypes'),
      openPicker: jasmine.createSpy('openPicker')
    };
    const documentService = jasmine.createSpyObj('DocumentService', ['getDocumentTypesByEntityName', 'linkFile'], {
      isLoading: signal(false)
    });
    const feedbackDialogService = jasmine.createSpyObj('FeedbackDialogService', ['showInfoToast', 'showSuccessToast']);

    documentService.getDocumentTypesByEntityName.and.returnValue(of({ records: [] }));
    documentService.linkFile.and.returnValue(of({ name: 'Mock File' }));

    await TestBed.configureTestingModule({
      imports: [GDriveDocumentComponent, TranslateModule.forRoot()],
      providers: [
        { provide: DrivePickerService, useValue: drivePickerService },
        { provide: DocumentService, useValue: documentService },
        { provide: FeedbackDialogService, useValue: feedbackDialogService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GDriveDocumentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

