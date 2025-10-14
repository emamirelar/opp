import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { of, throwError } from 'rxjs';
import { DocumentComponent } from './document.component';
import { DocumentService } from './../../../services/document.service';
import { FeedbackDialogService } from '../../../services/feedback-dialog.service';
import { AuthService } from '@core/services/auth.service';

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
      'getDocumentTypes',
      'deleteDocument',
      'downloadDocument'
    ], {
      isLoading: jasmine.createSpy('isLoading')
    });
    mockFeedbackService = jasmine.createSpyObj('FeedbackDialogService', ['showSuccess', 'showError']);
    mockTranslateService = jasmine.createSpyObj('TranslateService', ['instant']);
    mockAuthService = jasmine.createSpyObj('AuthService', ['isAuthenticated']);

    mockTranslateService.instant.and.returnValue('Translated text');
    mockDocumentService.getDocuments.and.returnValue(of([]));
    mockDocumentService.getDocumentTypes.and.returnValue(of([]));

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
    it('should load documents successfully', (done) => {
      const mockDocuments = [
        { id: '1', name: 'Document 1' },
        { id: '2', name: 'Document 2' }
      ];
      mockDocumentService.getDocuments.and.returnValue(of(mockDocuments));

      component.load();

      setTimeout(() => {
        expect(mockDocumentService.getDocuments).toHaveBeenCalledWith('Partner', '123');
        expect(component.documents()).toEqual(mockDocuments);
        done();
      }, 100);
    });

    it('should not load if entityName or entityId is missing', () => {
      fixture.componentRef.setInput('entityId', '');
      
      component.load();

      expect(mockDocumentService.getDocuments).not.toHaveBeenCalled();
      expect(component.documents()).toEqual([]);
    });

    it('should handle errors when loading documents', (done) => {
      mockDocumentService.getDocuments.and.returnValue(
        throwError(() => new Error('Load error'))
      );

      component.load();

      setTimeout(() => {
        expect(mockFeedbackService.showError).toHaveBeenCalled();
        done();
      }, 100);
    });
  });

  describe('loadDocumentTypes', () => {
    it('should load document types successfully', (done) => {
      const mockTypes = [
        { id: '1', name: 'Type 1' },
        { id: '2', name: 'Type 2' }
      ];
      mockDocumentService.getDocumentTypes.and.returnValue(of(mockTypes));

      component.loadDocumentTypes();

      setTimeout(() => {
        expect(mockDocumentService.getDocumentTypes).toHaveBeenCalledWith('Partner');
        expect(component.documentTypes()).toEqual(mockTypes);
        done();
      }, 100);
    });
  });

  describe('deleteDocument', () => {
    it('should delete document successfully', (done) => {
      const mockDocument = { id: '1', name: 'Document 1' };
      mockDocumentService.deleteDocument.and.returnValue(of(null));
      mockDocumentService.getDocuments.and.returnValue(of([]));

      component.deleteDocument(mockDocument);

      setTimeout(() => {
        expect(mockDocumentService.deleteDocument).toHaveBeenCalledWith('1');
        expect(mockFeedbackService.showSuccess).toHaveBeenCalled();
        expect(mockDocumentService.getDocuments).toHaveBeenCalled(); // Reload after delete
        done();
      }, 100);
    });

    it('should handle errors when deleting document', (done) => {
      const mockDocument = { id: '1', name: 'Document 1' };
      mockDocumentService.deleteDocument.and.returnValue(
        throwError(() => new Error('Delete error'))
      );

      component.deleteDocument(mockDocument);

      setTimeout(() => {
        expect(mockFeedbackService.showError).toHaveBeenCalled();
        done();
      }, 100);
    });
  });

  describe('file upload', () => {
    it('should open upload dialog', () => {
      expect(component.showUploadFile).toBeFalse();
      
      component.openUploadDialog();
      
      expect(component.showUploadFile).toBeTrue();
    });

    it('should close upload dialog', () => {
      component.showUploadFile = true;
      
      component.closeUploadDialog();
      
      expect(component.showUploadFile).toBeFalse();
    });

    it('should handle successful file upload', () => {
      spyOn(component, 'load');
      
      component.onUploadSuccess();

      expect(component.showUploadFile).toBeFalse();
      expect(component.load).toHaveBeenCalled();
      expect(mockFeedbackService.showSuccess).toHaveBeenCalled();
    });
  });

  describe('allDocuments getter', () => {
    it('should combine documents and pending files', () => {
      const docs = [{ id: '1', name: 'Doc 1' }];
      const pending = [{ id: 'pending', name: 'Pending Doc' }];
      
      component.documents.set(docs);
      component.pendingFiles.set(pending);

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


