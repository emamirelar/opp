import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { DocumentService } from './document.service';
import { DocumentLinkModel } from '@app/shared/interfaces';

describe('DocumentService', () => {
  let service: DocumentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [DocumentService]
    });

    service = TestBed.inject(DocumentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get document types by entity name', (done) => {
    const entityName = 'Contact';
    const mockResponse = [{ id: 1, name: 'Document Type 1' }];

    expect(service.isLoading()).toBe(false);

    service.getDocumentTypesByEntityName(entityName).subscribe(response => {
      expect(response).toEqual(mockResponse);
      expect(service.isLoading()).toBe(false);
      done();
    });

    expect(service.isLoading()).toBe(true);

    const req = httpMock.expectOne('/api/document-type/' + entityName);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get documents by entity name and ID', (done) => {
    const entityName = 'Partner';
    const entityId = '123';
    const mockResponse = [{ id: 1, name: 'Document 1' }];

    service.getDocuments(entityName, entityId).subscribe(response => {
      expect(response).toEqual(mockResponse);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/document/' + entityName + '/' + entityId);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should upload file', (done) => {
    const formData = new FormData();
    formData.append('file', new Blob(['test']), 'test.pdf');
    const mockResponse = { success: true, id: 1 };

    service.uploadFile(formData).subscribe(response => {
      expect(response).toEqual(mockResponse);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/document/upload');
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);
  });

  it('should link file', (done) => {
    const linkModel: DocumentLinkModel = {
      link: 'https://drive.google.com/file/123',
      googleId: '123',
      name: 'Test Document',
      type: 'application/pdf',
      documentTypeId: 1,
      parentEntityName: 'Contact',
      parentEntityId: 456
    };
    const mockResponse = { success: true };

    service.linkFile(linkModel).subscribe(response => {
      expect(response).toEqual(mockResponse);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/document/link');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(linkModel);
    req.flush(mockResponse);
  });

  it('should delete document', (done) => {
    const documentId = 123;
    const mockResponse = { success: true };

    service.delete(documentId).subscribe(response => {
      expect(response).toEqual(mockResponse);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/document/' + documentId);
    expect(req.request.method).toBe('DELETE');
    req.flush(mockResponse);
  });

  it('should download document', (done) => {
    const documentId = 123;
    const mockBlob = new Blob(['test content'], { type: 'application/pdf' });

    service.download(documentId).subscribe(response => {
      expect(response).toEqual(mockBlob);
      expect(service.isLoading()).toBe(false);
      done();
    });

    const req = httpMock.expectOne('/api/document/download/' + documentId);
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(mockBlob);
  });

  it('should set isLoading to false on error', (done) => {
    const entityName = 'Contact';

    service.getDocumentTypesByEntityName(entityName).subscribe({
      next: () => fail('should have errored'),
      error: () => {
        expect(service.isLoading()).toBe(false);
        done();
      }
    });

    const req = httpMock.expectOne('/api/document-type/' + entityName);
    req.error(new ProgressEvent('error'));
  });

  describe('uploadUnopsFiles', () => {
    it('should upload UNOPS files', (done) => {
      const formData = new FormData();
      formData.append('file', new Blob(['test']), 'test.pdf');
      formData.append('entityName', 'Project');
      formData.append('entityId', '123');
      const mockResponse = { success: true, id: 1, message: 'File uploaded successfully' };

      expect(service.isLoading()).toBe(false);

      service.uploadUnopsFiles(formData).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(service.isLoading()).toBe(false);
        done();
      });

      expect(service.isLoading()).toBe(true);

      const req = httpMock.expectOne('/api/unops/document/upload');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(formData);
      req.flush(mockResponse);
    });

    it('should set isLoading to false on error', (done) => {
      const formData = new FormData();

      service.uploadUnopsFiles(formData).subscribe({
        next: () => fail('should have errored'),
        error: () => {
          expect(service.isLoading()).toBe(false);
          done();
        }
      });

      const req = httpMock.expectOne('/api/unops/document/upload');
      req.error(new ProgressEvent('error'));
    });

    it('should handle large file uploads', (done) => {
      const formData = new FormData();
      const largeBlob = new Blob(['x'.repeat(1024 * 1024)], { type: 'application/pdf' }); // 1MB
      formData.append('file', largeBlob, 'large-file.pdf');
      const mockResponse = { success: true, id: 2 };

      service.uploadUnopsFiles(formData).subscribe(response => {
        expect(response).toEqual(mockResponse);
        done();
      });

      const req = httpMock.expectOne('/api/unops/document/upload');
      req.flush(mockResponse);
    });
  });

  describe('linkUnopsFiles', () => {
    it('should link UNOPS files', (done) => {
      const linkModel: DocumentLinkModel = {
        link: 'https://drive.google.com/file/unops123',
        googleId: 'unops123',
        name: 'UNOPS Document',
        type: 'application/pdf',
        documentTypeId: 2,
        parentEntityName: 'Project',
        parentEntityId: 789
      };
      const mockResponse = { success: true, message: 'File linked successfully' };

      expect(service.isLoading()).toBe(false);

      service.linkUnopsFiles(linkModel).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(service.isLoading()).toBe(false);
        done();
      });

      expect(service.isLoading()).toBe(true);

      const req = httpMock.expectOne('/api/unops/document/link');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(linkModel);
      req.flush(mockResponse);
    });

    it('should set isLoading to false on error', (done) => {
      const linkModel: DocumentLinkModel = {
        link: 'https://drive.google.com/file/test',
        googleId: 'test',
        name: 'Test',
        type: 'application/pdf',
        documentTypeId: 1,
        parentEntityName: 'Project',
        parentEntityId: 1
      };

      service.linkUnopsFiles(linkModel).subscribe({
        next: () => fail('should have errored'),
        error: () => {
          expect(service.isLoading()).toBe(false);
          done();
        }
      });

      const req = httpMock.expectOne('/api/unops/document/link');
      req.error(new ProgressEvent('error'));
    });

    it('should handle different document types', (done) => {
      const linkModel: DocumentLinkModel = {
        link: 'https://docs.google.com/spreadsheets/123',
        googleId: '123',
        name: 'Spreadsheet',
        type: 'application/vnd.google-apps.spreadsheet',
        documentTypeId: 3,
        parentEntityName: 'Partner',
        parentEntityId: 456
      };
      const mockResponse = { success: true };

      service.linkUnopsFiles(linkModel).subscribe(response => {
        expect(response).toEqual(mockResponse);
        done();
      });

      const req = httpMock.expectOne('/api/unops/document/link');
      req.flush(mockResponse);
    });
  });

  describe('uploadFiles', () => {
    it('should upload files', (done) => {
      const formData = new FormData();
      formData.append('file', new Blob(['content']), 'document.docx');
      formData.append('entityName', 'Contact');
      formData.append('entityId', '456');
      const mockResponse = { success: true, id: 5, url: '/documents/5' };

      expect(service.isLoading()).toBe(false);

      service.uploadFiles(formData).subscribe(response => {
        expect(response).toEqual(mockResponse);
        expect(service.isLoading()).toBe(false);
        done();
      });

      expect(service.isLoading()).toBe(true);

      const req = httpMock.expectOne('/api/document');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(formData);
      req.flush(mockResponse);
    });

    it('should set isLoading to false on error', (done) => {
      const formData = new FormData();

      service.uploadFiles(formData).subscribe({
        next: () => fail('should have errored'),
        error: () => {
          expect(service.isLoading()).toBe(false);
          done();
        }
      });

      const req = httpMock.expectOne('/api/document');
      req.error(new ProgressEvent('error'));
    });

    it('should handle multiple file uploads', (done) => {
      const formData = new FormData();
      formData.append('file1', new Blob(['file1']), 'doc1.pdf');
      formData.append('file2', new Blob(['file2']), 'doc2.pdf');
      const mockResponse = { success: true, ids: [10, 11] };

      service.uploadFiles(formData).subscribe(response => {
        expect(response).toEqual(mockResponse);
        done();
      });

      const req = httpMock.expectOne('/api/document');
      req.flush(mockResponse);
    });

    it('should handle different file formats', (done) => {
      const formData = new FormData();
      formData.append('imageFile', new Blob(['img'], { type: 'image/png' }), 'image.png');
      formData.append('textFile', new Blob(['txt'], { type: 'text/plain' }), 'text.txt');
      const mockResponse = { success: true };

      service.uploadFiles(formData).subscribe(response => {
        expect(response).toEqual(mockResponse);
        done();
      });

      const req = httpMock.expectOne('/api/document');
      req.flush(mockResponse);
    });
  });

  describe('integration scenarios', () => {
    it('should handle sequential uploads', (done) => {
      const formData1 = new FormData();
      const formData2 = new FormData();
      
      service.uploadFile(formData1).subscribe(() => {
        service.uploadFile(formData2).subscribe(() => {
          expect(service.isLoading()).toBe(false);
          done();
        });

        const req2 = httpMock.expectOne('/api/document/upload');
        req2.flush({ success: true });
      });

      const req1 = httpMock.expectOne('/api/document/upload');
      req1.flush({ success: true });
    });

    it('should handle upload then link workflow', (done) => {
      const formData = new FormData();
      const linkModel: DocumentLinkModel = {
        link: 'https://drive.google.com/file/123',
        googleId: '123',
        name: 'Doc',
        type: 'application/pdf',
        documentTypeId: 1,
        parentEntityName: 'Contact',
        parentEntityId: 1
      };

      service.uploadFile(formData).subscribe(() => {
        service.linkFile(linkModel).subscribe(() => {
          expect(service.isLoading()).toBe(false);
          done();
        });

        const req2 = httpMock.expectOne('/api/document/link');
        req2.flush({ success: true });
      });

      const req1 = httpMock.expectOne('/api/document/upload');
      req1.flush({ success: true, id: 1 });
    });

    it('should handle download after upload', (done) => {
      const formData = new FormData();
      
      service.uploadFile(formData).subscribe(response => {
        const documentId = (response as any).id;
        
        service.download(documentId).subscribe(blob => {
          expect(blob).toBeDefined();
          expect(service.isLoading()).toBe(false);
          done();
        });

        const req2 = httpMock.expectOne('/api/document/download/1');
        req2.flush(new Blob(['content']));
      });

      const req1 = httpMock.expectOne('/api/document/upload');
      req1.flush({ success: true, id: 1 });
    });
  });
});

