import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ConfirmationService } from 'primeng/api';
import { of, throwError } from 'rxjs';

import { ListviewExportService } from './listview-export.service';
import { ExportGoogleSheetService } from '@features/import-export/services/export-google-sheet.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { SearchParams } from './listview.model';

describe('ListviewExportService', () => {
  let service: ListviewExportService;
  let httpMock: HttpTestingController;
  let exportGoogleSheetService: jasmine.SpyObj<ExportGoogleSheetService>;
  let feedbackDialogService: jasmine.SpyObj<FeedbackDialogService>;
  let confirmationService: jasmine.SpyObj<ConfirmationService>;

  const mockData = [
    { id: 1, name: 'John Doe', email: 'john@example.com' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com' }
  ];

  const mockExportResult = {
    id: 'sheet123',
    url: 'https://sheets.google.com/sheet123'
  };

  beforeEach(() => {
    const exportSpy = jasmine.createSpyObj('ExportGoogleSheetService', ['exportToSheet']);
    const feedbackSpy = jasmine.createSpyObj('FeedbackDialogService', [
      'showInfoToast', 'showWarningToast', 'showErrorToast', 'clearAll'
    ]);
    const confirmationSpy = jasmine.createSpyObj('ConfirmationService', ['confirm']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        ListviewExportService,
        { provide: ExportGoogleSheetService, useValue: exportSpy },
        { provide: FeedbackDialogService, useValue: feedbackSpy },
        { provide: ConfirmationService, useValue: confirmationSpy }
      ]
    });

    service = TestBed.inject(ListviewExportService);
    httpMock = TestBed.inject(HttpTestingController);
    exportGoogleSheetService = TestBed.inject(ExportGoogleSheetService) as jasmine.SpyObj<ExportGoogleSheetService>;
    feedbackDialogService = TestBed.inject(FeedbackDialogService) as jasmine.SpyObj<FeedbackDialogService>;
    confirmationService = TestBed.inject(ConfirmationService) as jasmine.SpyObj<ConfirmationService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Basic Export Functionality', () => {
    it('should export data to Google Sheets successfully', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: (result) => {
          expect(result).toEqual(jasmine.objectContaining(mockExportResult));
          expect((result as { recordCount?: number }).recordCount).toBe(mockData.length);
          expect(feedbackDialogService.showInfoToast).toHaveBeenCalledWith({
            detail: 'Preparing contacts for export...',
            sticky: true
          });
          expect(feedbackDialogService.clearAll).toHaveBeenCalled();
          expect(confirmationService.confirm).toHaveBeenCalled();
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req =>
        req.url === '/api/contacts' && req.params.get('export') === 'true'
      );
      req.flush(mockData);
    });

    it('should handle empty data response', (done) => {
      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: () => done.fail('Expected error but got success'),
        error: (error) => {
          expect(error.message).toBe('No contacts found to export');
          expect(feedbackDialogService.showWarningToast).toHaveBeenCalledWith({
            detail: 'No contacts found to export'
          });
          done();
        }
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush([]);
    });

    it('should handle HTTP errors', (done) => {
      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: () => done.fail('Expected error but got success'),
        error: () => {
          expect(feedbackDialogService.showErrorToast).toHaveBeenCalledWith({
            detail: jasmine.stringContaining('Failed to export contacts')
          });
          done();
        }
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.error(new ErrorEvent('Network error'));
    });
  });

  describe('Search Parameters', () => {
    it('should include simple search parameters', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts', 
        'test search'
      );

      result$.subscribe({
        next: (result) => {
          expect(result).toEqual(jasmine.objectContaining(mockExportResult));
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('query') === 'test search'
      );
      req.flush(mockData);
    });

    it('should include advanced search parameters', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const searchParams: SearchParams = {
        fieldSearches: [
          { field: 'name', label: 'Name', value: 'test', operator: 'like' }
        ]
      };

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts', 
        searchParams
      );

      result$.subscribe({
        next: (result) => {
          expect(result).toEqual(jasmine.objectContaining(mockExportResult));
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('filters') === JSON.stringify(searchParams.fieldSearches)
      );
      req.flush(mockData);
    });

    it('should include general search from SearchParams object', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const searchParams: SearchParams = {
        generalSearch: 'general search term'
      };

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts', 
        searchParams
      );

      result$.subscribe({
        next: (result) => {
          expect(result).toEqual(jasmine.objectContaining(mockExportResult));
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('query') === 'general search term'
      );
      req.flush(mockData);
    });
  });

  describe('Sorting Parameters', () => {
    it('should include sorting parameters', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts',
        undefined,
        'name',
        'desc'
      );

      result$.subscribe({
        next: (result) => {
          expect(result).toEqual(jasmine.objectContaining(mockExportResult));
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('orderBy') === 'name' &&
        req.params.get('ascending') === 'false'
      );
      req.flush(mockData);
    });

    it('should handle ascending sort', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts',
        undefined,
        'email',
        'asc'
      );

      result$.subscribe({
        next: (result) => {
          expect(result).toEqual(jasmine.objectContaining(mockExportResult));
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('orderBy') === 'email' &&
        req.params.get('ascending') === 'true'
      );
      req.flush(mockData);
    });
  });

  describe('Response Format Handling', () => {
    it('should handle response with records property', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const response = {
        records: mockData,
        totalCount: 2
      };

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: (result) => {
          expect(result).toEqual(jasmine.objectContaining(mockExportResult));
          expect(exportGoogleSheetService.exportToSheet).toHaveBeenCalledWith(
            jasmine.any(Array),
            jasmine.stringMatching(/Contacts Export \d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}/)
          );
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(response);
    });

    it('should handle response with data property', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const response = {
        data: mockData,
        total: 2
      };

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: (result) => {
          expect(result).toEqual(jasmine.objectContaining(mockExportResult));
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(response);
    });

    it('should handle direct array response', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: (result) => {
          expect(result).toEqual(jasmine.objectContaining(mockExportResult));
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);
    });
  });

  describe('Entity-Specific Transforms', () => {
    it('should use contact transform for contact entities', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const contactData = [
        {
          id: 1,
          firstName: 'John',
          lastName: 'Doe',
          email: 'john@example.com',
          partner: { name: 'Test Partner' }
        }
      ];

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: () => {
          expect(exportGoogleSheetService.exportToSheet).toHaveBeenCalledWith(
            jasmine.arrayContaining([
              jasmine.objectContaining({
                ID: 1,
                FirstName: 'John',
                LastName: 'Doe',
                Email: 'john@example.com',
                Partner: 'Test Partner'
              })
            ]),
            jasmine.any(String)
          );
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(contactData);
    });

    it('should use partner transform for partner entities', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const partnerData = [
        {
          id: 1,
          name: 'Test Partner',
          partnerShortDescription: 'TP',
          status: 'Active',
          phone: '123-456-7890'
        }
      ];

      const result$ = service.exportToGoogleSheet('Partner', '/api/partners');

      result$.subscribe({
        next: () => {
          expect(exportGoogleSheetService.exportToSheet).toHaveBeenCalledWith(
            jasmine.arrayContaining([
              jasmine.objectContaining({
                ID: 1,
                Name: 'Test Partner',
                ShortDescription: 'TP',
                Status: 'Active'
              })
            ]),
            jasmine.any(String)
          );
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/partners');
      req.flush(partnerData);
    });

    it('should use interaction transform for interaction entities', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const interactionData = [
        {
          id: 1,
          type: 'Meeting',
          date: '2023-01-01',
          subject: 'Test Meeting',
          contactId: 123
        }
      ];

      const result$ = service.exportToGoogleSheet('Interaction', '/api/interactions');

      result$.subscribe({
        next: () => {
          expect(exportGoogleSheetService.exportToSheet).toHaveBeenCalledWith(
            jasmine.arrayContaining([
              jasmine.objectContaining({
                ID: 1,
                Type: 'Meeting',
                Date: '2023-01-01',
                Subject: 'Test Meeting',
                ContactId: 123
              })
            ]),
            jasmine.any(String)
          );
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/interactions');
      req.flush(interactionData);
    });

    it('should use custom transform function when provided', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const customTransform = (data: any[]) => data.map(item => ({
        CustomField: item.name,
        CustomEmail: item.email
      }));

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts',
        undefined,
        undefined,
        undefined,
        customTransform
      );

      result$.subscribe({
        next: () => {
          expect(exportGoogleSheetService.exportToSheet).toHaveBeenCalledWith(
            jasmine.arrayContaining([
              jasmine.objectContaining({
                CustomField: 'John Doe',
                CustomEmail: 'john@example.com'
              })
            ]),
            jasmine.any(String)
          );
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);
    });
  });

  describe('Default Transform', () => {
    it('should use default transform for unknown entity types', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const unknownData = [
        {
          id: 1,
          someField: 'value',
          camelCaseField: 'test',
          permissions: { read: true, write: false }
        }
      ];

      const result$ = service.exportToGoogleSheet('Unknown', '/api/unknown');

      result$.subscribe({
        next: () => {
          expect(exportGoogleSheetService.exportToSheet).toHaveBeenCalledWith(
            jasmine.arrayContaining([
              jasmine.objectContaining({
                Id: 1,
                'Some Field': 'value',
                'Camel Case Field': 'test'
              })
            ]),
            jasmine.any(String)
          );

          // Should not include permissions field
          const callArgs = exportGoogleSheetService.exportToSheet.calls.mostRecent().args[0];
          expect(callArgs[0].hasOwnProperty('permissions')).toBe(false);
          expect(callArgs[0].hasOwnProperty('Permissions')).toBe(false);
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/unknown');
      req.flush(unknownData);
    });

    it('should handle null and undefined values in default transform', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const dataWithNulls = [
        {
          id: 1,
          nullField: null,
          undefinedField: undefined,
          validField: 'value'
        }
      ];

      const result$ = service.exportToGoogleSheet('Test', '/api/test');

      result$.subscribe({
        next: () => {
          const callArgs = exportGoogleSheetService.exportToSheet.calls.mostRecent().args[0];
          expect(callArgs[0]).toEqual(jasmine.objectContaining({
            Id: 1,
            'Null Field': '',
            'Undefined Field': '',
            'Valid Field': 'value'
          }));
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/test');
      req.flush(dataWithNulls);
    });

    it('should skip object properties in default transform', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const dataWithObjects = [
        {
          id: 1,
          stringField: 'value',
          objectField: { nested: 'object' },
          arrayField: ['item1', 'item2']
        }
      ];

      const result$ = service.exportToGoogleSheet('Test', '/api/test');

      result$.subscribe({
        next: () => {
          const callArgs = exportGoogleSheetService.exportToSheet.calls.mostRecent().args[0];
          expect(callArgs[0]).toEqual({
            Id: 1,
            'String Field': 'value'
          });
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/test');
      req.flush(dataWithObjects);
    });
  });

  describe('Filename Generation', () => {
    it('should generate filename with timestamp', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: () => {
          const callArgs = exportGoogleSheetService.exportToSheet.calls.mostRecent().args;
          const filename = callArgs[1];
          expect(filename).toMatch(/Contacts Export \d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}/);
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);
    });
  });

  describe('Success and Error Handling', () => {
    it('should show success confirmation with clickable link', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: () => {
          expect(confirmationService.confirm).toHaveBeenCalledWith(
            jasmine.objectContaining({
              message: jasmine.stringContaining(mockExportResult.url),
              header: 'Export Complete',
              icon: 'pi pi-check-circle'
            })
          );
          done();
        },
        error: done.fail
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);
    });

    it('should handle export service errors', (done) => {
      exportGoogleSheetService.exportToSheet.and.returnValue(
        throwError(() => new Error('Export failed'))
      );

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      result$.subscribe({
        next: () => done.fail('Expected error but got success'),
        error: () => {
          expect(feedbackDialogService.showErrorToast).toHaveBeenCalledWith({
            detail: 'Failed to export contacts: Export failed'
          });
          done();
        }
      });

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);
    });
  });
});
