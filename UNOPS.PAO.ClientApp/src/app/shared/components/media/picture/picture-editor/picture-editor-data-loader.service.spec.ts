import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { HttpEvent, HttpEventType, HttpResponse } from '@angular/common/http';
import { PictureEditorDataLoaderService } from './picture-editor-data-loader.service';

describe('PictureEditorDataLoaderService', () => {
  let service: PictureEditorDataLoaderService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PictureEditorDataLoaderService]
    });

    service = TestBed.inject(PictureEditorDataLoaderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initialization', () => {
    it('should have default signal values', () => {
      expect(service.isLoading()).toBeFalse();
      expect(service.hasError()).toBeFalse();
      expect(service.uploadProgress()).toBe(0);
    });
  });

  describe('setUploadUrl', () => {
    it('should set the upload URL', () => {
      const url = '/api/contact/123/profile-picture';
      service.setUploadUrl(url);

      // Verify by uploading (which will use the URL)
      const file = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      service.uploadImage(file).subscribe();

      const req = httpMock.expectOne(url);
      expect(req.request.url).toBe(url);
      req.flush({ imageUrl: 'test.jpg' });
    });

    it('should update the upload URL when called multiple times', () => {
      service.setUploadUrl('/api/url1');
      service.setUploadUrl('/api/url2');

      const file = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      service.uploadImage(file).subscribe();

      const req = httpMock.expectOne('/api/url2');
      expect(req.request.url).toBe('/api/url2');
      req.flush({ imageUrl: 'test.jpg' });
    });
  });

  describe('uploadImage', () => {
    const mockFile = new File(['test content'], 'test.jpg', { type: 'image/jpeg' });
    const uploadUrl = '/api/contact/123/profile-picture';

    beforeEach(() => {
      service.setUploadUrl(uploadUrl);
    });

    it('should return empty string if upload URL is not set', (done) => {
      service.setUploadUrl('');
      
      service.uploadImage(mockFile).subscribe(result => {
        expect(result).toBe('');
        done();
      });

      httpMock.expectNone(uploadUrl);
    });

    it('should set loading state when upload starts', () => {
      service.uploadImage(mockFile).subscribe();

      expect(service.isLoading()).toBeTrue();
      expect(service.hasError()).toBeFalse();
      expect(service.uploadProgress()).toBe(0);

      const req = httpMock.expectOne(uploadUrl);
      req.flush({ imageUrl: 'uploaded.jpg' });
    });

    it('should send file as FormData', () => {
      service.uploadImage(mockFile).subscribe();

      const req = httpMock.expectOne(uploadUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body instanceof FormData).toBeTrue();
      
      const formData = req.request.body as FormData;
      expect(formData.get('file')).toBe(mockFile);

      req.flush({ imageUrl: 'uploaded.jpg' });
    });

    it('should update progress during upload', () => {
      service.uploadImage(mockFile).subscribe();

      const req = httpMock.expectOne(uploadUrl);

      // Simulate upload progress events
      const progressEvent: HttpEvent<any> = {
        type: HttpEventType.UploadProgress,
        loaded: 50,
        total: 100
      };

      req.event(progressEvent);
      expect(service.uploadProgress()).toBe(50);

      const progressEvent2: HttpEvent<any> = {
        type: HttpEventType.UploadProgress,
        loaded: 100,
        total: 100
      };

      req.event(progressEvent2);
      expect(service.uploadProgress()).toBe(100);

      req.flush({ imageUrl: 'uploaded.jpg' });
    });

    it('should handle upload progress without total', () => {
      service.uploadImage(mockFile).subscribe();

      const req = httpMock.expectOne(uploadUrl);

      const progressEvent: HttpEvent<any> = {
        type: HttpEventType.UploadProgress,
        loaded: 50,
        total: undefined
      };

      req.event(progressEvent);
      expect(service.uploadProgress()).toBe(0);

      req.flush({ imageUrl: 'uploaded.jpg' });
    });

    it('should return image URL on successful upload', (done) => {
      const expectedUrl = 'https://example.com/uploaded-image.jpg';

      service.uploadImage(mockFile).subscribe(imageUrl => {
        expect(imageUrl).toBe(expectedUrl);
        expect(service.isLoading()).toBeFalse();
        expect(service.hasError()).toBeFalse();
        expect(service.uploadProgress()).toBe(100);
        done();
      });

      const req = httpMock.expectOne(uploadUrl);
      req.flush({ imageUrl: expectedUrl });
    });

    it('should handle missing imageUrl in response', (done) => {
      service.uploadImage(mockFile).subscribe(imageUrl => {
        expect(imageUrl).toBe('');
        done();
      });

      const req = httpMock.expectOne(uploadUrl);
      req.flush({});
    });

    it('should handle response without body', (done) => {
      service.uploadImage(mockFile).subscribe(imageUrl => {
        expect(imageUrl).toBe('');
        done();
      });

      const req = httpMock.expectOne(uploadUrl);
      req.flush(null);
    });

    it('should set error state on upload failure', (done) => {
      const errorMessage = 'Upload failed';

      service.uploadImage(mockFile).subscribe({
        next: (result) => {
          expect(result).toBe('');
          expect(service.isLoading()).toBeFalse();
          expect(service.hasError()).toBeTrue();
          done();
        }
      });

      const req = httpMock.expectOne(uploadUrl);
      req.error(new ProgressEvent('error'), { status: 500, statusText: errorMessage });
    });

    it('should clear error state when starting new upload', () => {
      // First upload fails
      service.uploadImage(mockFile).subscribe();
      let req = httpMock.expectOne(uploadUrl);
      req.error(new ProgressEvent('error'));

      expect(service.hasError()).toBeTrue();

      // Second upload starts
      service.uploadImage(mockFile).subscribe();
      
      expect(service.hasError()).toBeFalse();
      expect(service.isLoading()).toBeTrue();

      req = httpMock.expectOne(uploadUrl);
      req.flush({ imageUrl: 'success.jpg' });
    });

    it('should reset progress when starting new upload', () => {
      // First upload
      service.uploadImage(mockFile).subscribe();
      let req = httpMock.expectOne(uploadUrl);
      req.flush({ imageUrl: 'first.jpg' });

      expect(service.uploadProgress()).toBe(100);

      // Second upload starts
      service.uploadImage(mockFile).subscribe();
      
      expect(service.uploadProgress()).toBe(0);

      req = httpMock.expectOne(uploadUrl);
      req.flush({ imageUrl: 'second.jpg' });
    });

    it('should handle multiple simultaneous uploads', () => {
      const file1 = new File(['content1'], 'file1.jpg', { type: 'image/jpeg' });
      const file2 = new File(['content2'], 'file2.jpg', { type: 'image/jpeg' });

      let result1: string = '';
      let result2: string = '';

      service.uploadImage(file1).subscribe(url => result1 = url);
      service.uploadImage(file2).subscribe(url => result2 = url);

      const requests = httpMock.match(uploadUrl);
      expect(requests.length).toBe(2);

      requests[0].flush({ imageUrl: 'url1.jpg' });
      requests[1].flush({ imageUrl: 'url2.jpg' });

      // Note: Due to signal updates, the second upload will overwrite the first's state
      expect(result1).toBe('url1.jpg');
      expect(result2).toBe('url2.jpg');
    });

    it('should handle very large progress values', () => {
      service.uploadImage(mockFile).subscribe();

      const req = httpMock.expectOne(uploadUrl);

      const progressEvent: HttpEvent<any> = {
        type: HttpEventType.UploadProgress,
        loaded: 999999999,
        total: 1000000000
      };

      req.event(progressEvent);
      expect(service.uploadProgress()).toBeGreaterThan(99);
      expect(service.uploadProgress()).toBeLessThanOrEqual(100);

      req.flush({ imageUrl: 'uploaded.jpg' });
    });

    it('should request with correct options', () => {
      service.uploadImage(mockFile).subscribe();

      const req = httpMock.expectOne(uploadUrl);
      expect(req.request.reportProgress).toBeTrue();
      expect(req.request.params.keys().length).toBe(0);

      req.flush({ imageUrl: 'uploaded.jpg' });
    });
  });

  describe('computed signals', () => {
    it('should reactively update isLoading', () => {
      expect(service.isLoading()).toBeFalse();

      service.setUploadUrl('/api/test');
      const file = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      service.uploadImage(file).subscribe();

      expect(service.isLoading()).toBeTrue();

      const req = httpMock.expectOne('/api/test');
      req.flush({ imageUrl: 'test.jpg' });

      expect(service.isLoading()).toBeFalse();
    });

    it('should reactively update hasError', () => {
      expect(service.hasError()).toBeFalse();

      service.setUploadUrl('/api/test');
      const file = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      service.uploadImage(file).subscribe();

      const req = httpMock.expectOne('/api/test');
      req.error(new ProgressEvent('error'));

      expect(service.hasError()).toBeTrue();
    });

    it('should reactively update uploadProgress', () => {
      expect(service.uploadProgress()).toBe(0);

      service.setUploadUrl('/api/test');
      const file = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      service.uploadImage(file).subscribe();

      const req = httpMock.expectOne('/api/test');

      const progressEvent: HttpEvent<any> = {
        type: HttpEventType.UploadProgress,
        loaded: 75,
        total: 100
      };

      req.event(progressEvent);
      expect(service.uploadProgress()).toBe(75);

      req.flush({ imageUrl: 'test.jpg' });
      expect(service.uploadProgress()).toBe(100);
    });
  });
});


