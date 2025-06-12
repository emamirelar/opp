import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ConfirmationService } from 'primeng/api';
import { of, throwError } from 'rxjs';

import { ListviewExportService } from './listview-export.service';
import { ExportGoogleSheetService } from '../../../reusables/components/export/export-google-sheet.service';
import { FeedbackDialogService } from '../../../reusables/services/feedback-dialog.service';
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
    it('should export data to Google Sheets successfully', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && req.params.get('export') === 'true'
      );
      req.flush(mockData);

      result$.subscribe(result => {
        expect(result).toEqual(mockExportResult);
        expect(feedbackDialogService.showInfoToast).toHaveBeenCalledWith({
          detail: 'Preparing contacts for export...',
          sticky: true
        });
        expect(feedbackDialogService.clearAll).toHaveBeenCalled();
        expect(confirmationService.confirm).toHaveBeenCalled();
      });
    });

    it('should handle empty data response', () => {
      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush([]);

      result$.subscribe({
        error: (error) => {
          expect(error.message).toBe('No contacts found to export');
          expect(feedbackDialogService.showWarningToast).toHaveBeenCalledWith({
            detail: 'No contacts found to export'
          });
        }
      });
    });

    it('should handle HTTP errors', () => {
      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.error(new ErrorEvent('Network error'));

      result$.subscribe({
        error: () => {
          expect(feedbackDialogService.showErrorToast).toHaveBeenCalledWith({
            detail: jasmine.stringContaining('Failed to export contacts')
          });
        }
      });
    });
  });

  describe('Search Parameters', () => {
    it('should include simple search parameters', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts', 
        'test search'
      );

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('searchText') === 'test search'
      );
      req.flush(mockData);

      result$.subscribe();
    });

    it('should include advanced search parameters', () => {
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

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('advancedSearch') === 'true' &&
        req.params.get('searchCriteria') === JSON.stringify(searchParams.fieldSearches)
      );
      req.flush(mockData);

      result$.subscribe();
    });

    it('should include general search from SearchParams object', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const searchParams: SearchParams = {
        generalSearch: 'general search term'
      };

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts', 
        searchParams
      );

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('searchText') === 'general search term'
      );
      req.flush(mockData);

      result$.subscribe();
    });
  });

  describe('Sorting Parameters', () => {
    it('should include sorting parameters', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts',
        undefined,
        'name',
        'desc'
      );

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('orderBy') === 'name' &&
        req.params.get('ascending') === 'false'
      );
      req.flush(mockData);

      result$.subscribe();
    });

    it('should handle ascending sort', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet(
        'Contact', 
        '/api/contacts',
        undefined,
        'email',
        'asc'
      );

      const req = httpMock.expectOne(req => 
        req.url === '/api/contacts' && 
        req.params.get('orderBy') === 'email' &&
        req.params.get('ascending') === 'true'
      );
      req.flush(mockData);

      result$.subscribe();
    });
  });

  describe('Response Format Handling', () => {
    it('should handle response with records property', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const response = {
        records: mockData,
        totalCount: 2
      };

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(response);

      result$.subscribe(result => {
        expect(result).toEqual(mockExportResult);
        expect(exportGoogleSheetService.exportToSheet).toHaveBeenCalledWith(
          jasmine.any(Array),
          jasmine.stringMatching(/Contacts Export \d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}/)
        );
      });
    });

    it('should handle response with data property', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const response = {
        data: mockData,
        total: 2
      };

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(response);

      result$.subscribe();
    });

    it('should handle direct array response', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);

      result$.subscribe();
    });
  });

  describe('Entity-Specific Transforms', () => {
    it('should use contact transform for contact entities', () => {
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

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(contactData);

      result$.subscribe(() => {
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
      });
    });

    it('should use partner transform for partner entities', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const partnerData = [
        {
          id: 1,
          name: 'Test Partner',
          shortName: 'TP',
          status: 'Active',
          phone: '123-456-7890'
        }
      ];

      const result$ = service.exportToGoogleSheet('Partner', '/api/partners');

      const req = httpMock.expectOne(req => req.url === '/api/partners');
      req.flush(partnerData);

      result$.subscribe(() => {
        expect(exportGoogleSheetService.exportToSheet).toHaveBeenCalledWith(
          jasmine.arrayContaining([
            jasmine.objectContaining({
              ID: 1,
              Name: 'Test Partner',
              ShortName: 'TP',
              Status: 'Active',
              Phone: '123-456-7890'
            })
          ]),
          jasmine.any(String)
        );
      });
    });

    it('should use interaction transform for interaction entities', () => {
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

      const req = httpMock.expectOne(req => req.url === '/api/interactions');
      req.flush(interactionData);

      result$.subscribe(() => {
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
      });
    });

    it('should use custom transform function when provided', () => {
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

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);

      result$.subscribe(() => {
        expect(exportGoogleSheetService.exportToSheet).toHaveBeenCalledWith(
          jasmine.arrayContaining([
            jasmine.objectContaining({
              CustomField: 'John Doe',
              CustomEmail: 'john@example.com'
            })
          ]),
          jasmine.any(String)
        );
      });
    });
  });

  describe('Default Transform', () => {
    it('should use default transform for unknown entity types', () => {
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

      const req = httpMock.expectOne(req => req.url === '/api/unknown');
      req.flush(unknownData);

      result$.subscribe(() => {
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
      });
    });

    it('should handle null and undefined values in default transform', () => {
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

      const req = httpMock.expectOne(req => req.url === '/api/test');
      req.flush(dataWithNulls);

      result$.subscribe(() => {
        const callArgs = exportGoogleSheetService.exportToSheet.calls.mostRecent().args[0];
        expect(callArgs[0]).toEqual(jasmine.objectContaining({
          Id: 1,
          'Null Field': '',
          'Undefined Field': '',
          'Valid Field': 'value'
        }));
      });
    });

    it('should skip object properties in default transform', () => {
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

      const req = httpMock.expectOne(req => req.url === '/api/test');
      req.flush(dataWithObjects);

      result$.subscribe(() => {
        const callArgs = exportGoogleSheetService.exportToSheet.calls.mostRecent().args[0];
        expect(callArgs[0]).toEqual({
          Id: 1,
          'String Field': 'value'
        });
      });
    });
  });

  describe('Filename Generation', () => {
    it('should generate filename with timestamp', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);

      result$.subscribe(() => {
        const callArgs = exportGoogleSheetService.exportToSheet.calls.mostRecent().args;
        const filename = callArgs[1];
        expect(filename).toMatch(/Contacts Export \d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}/);
      });
    });
  });

  describe('Success and Error Handling', () => {
    it('should show success confirmation with clickable link', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(of(mockExportResult));

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);

      result$.subscribe(() => {
        expect(confirmationService.confirm).toHaveBeenCalledWith(
          jasmine.objectContaining({
            message: jasmine.stringContaining(mockExportResult.url),
            header: 'Export Complete',
            icon: 'pi pi-check-circle'
          })
        );
      });
    });

    it('should handle export service errors', () => {
      exportGoogleSheetService.exportToSheet.and.returnValue(
        throwError(() => new Error('Export failed'))
      );

      const result$ = service.exportToGoogleSheet('Contact', '/api/contacts');

      const req = httpMock.expectOne(req => req.url === '/api/contacts');
      req.flush(mockData);

      result$.subscribe({
        error: () => {
          expect(feedbackDialogService.showErrorToast).toHaveBeenCalledWith({
            detail: 'Failed to export contacts: Export failed'
          });
        }
      });
    });
  });
});