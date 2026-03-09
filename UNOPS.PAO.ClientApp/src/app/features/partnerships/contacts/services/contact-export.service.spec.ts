import { TestBed } from '@angular/core/testing';
import { ContactExportService } from './contact-export.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { ContactService } from './contact.service';
import { ExportGoogleSheetService } from '@features/import-export/services/export-google-sheet.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { ConfirmationService } from 'primeng/api';

const mockContactService = jasmine.createSpyObj('ContactService', ['getContacts']);
const mockExportGoogleSheetService = jasmine.createSpyObj('ExportGoogleSheetService', ['exportToGoogleSheet']);
const mockFeedbackDialogService = jasmine.createSpyObj('FeedbackDialogService', ['showInfoToast', 'clearAll']);
const mockConfirmationService = jasmine.createSpyObj('ConfirmationService', ['confirm']);

describe('ContactExportService', () => {
  let service: ContactExportService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        ContactExportService,
        { provide: ContactService, useValue: mockContactService },
        { provide: ExportGoogleSheetService, useValue: mockExportGoogleSheetService },
        { provide: FeedbackDialogService, useValue: mockFeedbackDialogService },
        { provide: ConfirmationService, useValue: mockConfirmationService }
      ]
    });
    service = TestBed.inject(ContactExportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for contact export
  // TODO: Add tests for export formats
});

