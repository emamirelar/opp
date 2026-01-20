import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { of } from 'rxjs';
import { DocumentComponent } from './document.component';
import { DocumentService } from '@shared/services/api/document.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { AuthService } from '@core/services/auth';

describe('DocumentComponent', () => {
  let component: DocumentComponent;
  let fixture: ComponentFixture<DocumentComponent>;
  let mockDocumentService: jasmine.SpyObj<DocumentService>;
  let mockFeedbackService: jasmine.SpyObj<FeedbackDialogService>;
  let mockTranslateService: jasmine.SpyObj<TranslateService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    mockDocumentService = jasmine.createSpyObj('DocumentService', [
      'getDocuments',
      'getDocumentTypesByEntityName',
      'delete',
      'download',
      'linkFile'
    ], {
      isLoading: signal(false)
    });
    mockFeedbackService = jasmine.createSpyObj('FeedbackDialogService', ['showSuccessToast', 'showInfoToast', 'showErrorToast']);
    mockTranslateService = jasmine.createSpyObj('TranslateService', ['instant', 'get'], {
      onLangChange: of({ lang: 'en' }),
      onTranslationChange: of({ lang: 'en', translations: {} }),
      onDefaultLangChange: of({ lang: 'en', translations: {} })
    });
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAuthenticated']);

    mockTranslateService.instant.and.returnValue('Translated text');
    mockDocumentService.getDocuments.and.returnValue(of([]));
    mockDocumentService.getDocumentTypesByEntityName.and.returnValue(of({ records: [] }));
    mockTranslateService.get.and.returnValue(of('Translated text'));

    await TestBed.configureTestingModule({
      imports: [
        DocumentComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: DocumentService, useValue: mockDocumentService },
        { provide: FeedbackDialogService, useValue: mockFeedbackService },
        { provide: TranslateService, useValue: mockTranslateService },
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentComponent);
    component = fixture.componentInstance;
    
    // Set required inputs
    fixture.componentRef.setInput('entityName', 'Partner');
    fixture.componentRef.setInput('entityId', '123');
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('load', () => {
    it('should load documents successfully', () => {
      const mockDocuments = [
        { id: '1', name: 'Document 1' },
        { id: '2', name: 'Document 2' }
      ];
      mockDocumentService.getDocuments.and.returnValue(of(mockDocuments));

      component.load();

      expect(mockDocumentService.getDocuments).toHaveBeenCalledWith('Partner', '123');
      expect(component.documents() as any).toEqual(mockDocuments);
    });

    it('should not load if entityName or entityId is missing', () => {
      fixture.componentRef.setInput('entityId', '');
      
      component.load();

      expect(mockDocumentService.getDocuments).not.toHaveBeenCalled();
      expect(component.documents()).toEqual([]);
    });

  });

  describe('loadDocumentTypes', () => {
    it('should load document types successfully', () => {
      const mockTypes = [
        { id: '1', name: 'Type 1' },
        { id: '2', name: 'Type 2' }
      ];
      mockDocumentService.getDocumentTypesByEntityName.and.returnValue(of({ records: mockTypes }));

      component.loadDocumentTypes();

      expect(mockDocumentService.getDocumentTypesByEntityName).toHaveBeenCalledWith('Partner');
      expect(component.documentTypes()).toEqual(mockTypes);
    });
  });

  describe('deleteDocument', () => {
    it('should delete document successfully', () => {
      const mockDocument = { id: 1, name: 'Document 1' };
      mockDocumentService.delete.and.returnValue(of({}));
      mockDocumentService.getDocuments.and.returnValue(of([]));
      spyOn(component, 'load');
      component.selectedDocument = mockDocument;

      component['handleOnDocumentDelete']();

      expect(mockDocumentService.delete).toHaveBeenCalledWith(1);
      expect(mockFeedbackService.showSuccessToast).toHaveBeenCalled();
      expect(component.load).toHaveBeenCalled();
    });
  });

  describe('file upload', () => {
    it('should open upload dialog', () => {
      expect(component.showUploadFile).toBeFalse();
      
      component.openUploadDialog();
      
      expect(component.showUploadFile).toBeTrue();
    });

    it('should handle successful file upload', () => {
      spyOn(component, 'load');
      
      component.handleOnUploadDocumentSuccess();

      expect(component.showUploadFile).toBeFalse();
      expect(component.load).toHaveBeenCalled();
    });

    it('should clear upload dialog on close', () => {
      const uploadDialog = { clear: jasmine.createSpy('clear') };

      component.handleOnUploadDocumentDialogClose(uploadDialog);

      expect(uploadDialog.clear).toHaveBeenCalled();
    });
  });

  describe('allDocuments getter', () => {
    it('should combine documents and pending files', () => {
      const docs = [{ id: '1', name: 'Doc 1' }];
      const pending = [{ id: 'pending', name: 'Pending Doc' }];
      
      component.documents.set(docs as unknown as never[]);
      component.pendingFiles.set(pending as unknown as never[]);

      const all = component.allDocuments;

      expect(all.length).toBe(2);
      expect(all).toContain(docs[0]);
      expect(all).toContain(pending[0]);
    });
  });

  describe('input validations', () => {
    it('should respect isReadOnly input', () => {
      fixture.componentRef.setInput('isReadOnly', true);
      fixture.detectChanges();

      expect(component.isReadOnly()).toBeTrue();
    });

    it('should respect canDownload input', () => {
      fixture.componentRef.setInput('canDownload', false);
      fixture.detectChanges();

      expect(component.canDownload()).toBeFalse();
    });

    it('should respect canDelete input', () => {
      fixture.componentRef.setInput('canDelete', false);
      fixture.detectChanges();

      expect(component.canDelete()).toBeFalse();
    });
  });
});


