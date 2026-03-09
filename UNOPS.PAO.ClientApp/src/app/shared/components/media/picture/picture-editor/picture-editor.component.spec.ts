import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule, TranslateService, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { PictureEditorComponent } from './picture-editor.component';
import { PictureEditorDataLoaderService } from './picture-editor-data-loader.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { of, throwError } from 'rxjs';
import { DomSanitizer } from '@angular/platform-browser';
import { ImageCroppedEvent } from 'ngx-image-cropper';

describe('PictureEditorComponent', () => {
  let component: PictureEditorComponent;
  let fixture: ComponentFixture<PictureEditorComponent>;
  let mockDialogRef: jasmine.SpyObj<DynamicDialogRef>;
  let mockDialogConfig: jasmine.SpyObj<DynamicDialogConfig>;
  let mockDataLoader: jasmine.SpyObj<PictureEditorDataLoaderService>;
  let translateService: TranslateService;
  let mockFeedbackService: jasmine.SpyObj<FeedbackDialogService>;
  let mockSanitizer: jasmine.SpyObj<DomSanitizer>;

  beforeEach(async () => {
    mockDialogRef = jasmine.createSpyObj('DynamicDialogRef', ['close']);
    mockDialogConfig = jasmine.createSpyObj('DynamicDialogConfig', [], {
      data: { uploadUrl: '/api/upload' }
    });
    mockDataLoader = jasmine.createSpyObj('PictureEditorDataLoaderService', 
      ['setUploadUrl', 'uploadImage'],
      {
        isLoading: jasmine.createSpy('isLoading').and.returnValue(false),
        uploadProgress: jasmine.createSpy('uploadProgress').and.returnValue(0)
      }
    );
    mockFeedbackService = jasmine.createSpyObj('FeedbackDialogService', ['showErrorToast', 'showSuccessToast']);
    mockSanitizer = jasmine.createSpyObj('DomSanitizer', ['bypassSecurityTrustUrl']);
    mockSanitizer.bypassSecurityTrustUrl.and.returnValue('safe-url' as any);

    await TestBed.configureTestingModule({
      imports: [
        PictureEditorComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader }
        })
      ],
      providers: [
        { provide: DynamicDialogRef, useValue: mockDialogRef },
        { provide: DynamicDialogConfig, useValue: mockDialogConfig },
        { provide: PictureEditorDataLoaderService, useValue: mockDataLoader },
        { provide: FeedbackDialogService, useValue: mockFeedbackService },
        { provide: DomSanitizer, useValue: mockSanitizer }
      ]
    })
      .overrideComponent(PictureEditorComponent, {
        set: {
          providers: [
            { provide: PictureEditorDataLoaderService, useValue: mockDataLoader }
          ]
        }
      })
      .compileComponents();

    fixture = TestBed.createComponent(PictureEditorComponent);
    component = fixture.componentInstance;
    translateService = TestBed.inject(TranslateService);
    spyOn(translateService, 'instant').and.returnValue('Translated message');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('initialization', () => {
    it('should have default values', () => {
      expect(component.imageChangedEvent).toBe('');
      expect(component.imageBase64).toBeNull();
      expect(component.croppedImage).toBeUndefined();
      expect(component.croppedBlob).toBeNull();
      expect(component.isProcessing).toBeFalse();
      expect(component.isCropperReady).toBeFalse();
      expect(component.isDraggingOver).toBeFalse();
    });

    it('should set upload URL from dialog config on init', () => {
      component.ngOnInit();

      expect(mockDataLoader.setUploadUrl).toHaveBeenCalledWith('/api/upload');
    });

    it('should not fail if dialog config is null', () => {
      mockDialogConfig.data = null;

      expect(() => component.ngOnInit()).not.toThrow();
    });

    it('should compute isUploading from data loader', () => {
      expect(component.isUploading()).toBe(false);
    });

    it('should compute uploadProgress from data loader', () => {
      expect(component.uploadProgress()).toBe(0);
    });
  });

  describe('uploadUrl input', () => {
    it('should set upload URL when provided', () => {
      component.uploadUrl = '/api/test/upload';

      expect(mockDataLoader.setUploadUrl).toHaveBeenCalledWith('/api/test/upload');
    });

    it('should not set upload URL when value is empty', () => {
      mockDataLoader.setUploadUrl.calls.reset();
      
      component.uploadUrl = '';

      expect(mockDataLoader.setUploadUrl).not.toHaveBeenCalled();
    });

    it('should not set upload URL when value is undefined', () => {
      mockDataLoader.setUploadUrl.calls.reset();
      
      component.uploadUrl = undefined as any;

      expect(mockDataLoader.setUploadUrl).not.toHaveBeenCalled();
    });
  });

  describe('hide', () => {
    it('should close the dialog', () => {
      component.hide();

      expect(mockDialogRef.close).toHaveBeenCalled();
    });
  });

  describe('handleFileInput', () => {
    let mockFile: File;

    beforeEach(() => {
      mockFile = new File(['test content'], 'test.jpg', { type: 'image/jpeg' });
    });

    it('should not process if file is null', () => {
      component.handleFileInput(null as any);

      expect(component.isProcessing).toBeFalse();
    });

    it('should reset state before processing', () => {
      component.isCropperReady = true;
      component.croppedBlob = new Blob();
      component.croppedImage = 'test' as any;

      component.handleFileInput(mockFile);

      expect(component.isCropperReady).toBeFalse();
      expect(component.croppedBlob).toBeNull();
      expect(component.croppedImage).toBeUndefined();
    });

    it('should set processing state while reading file', () => {
      component.handleFileInput(mockFile);

      expect(component.isProcessing).toBeTrue();
    });

    it('should read file as base64', (done) => {
      component.handleFileInput(mockFile);

      setTimeout(() => {
        expect(component.imageBase64).toBeTruthy();
        expect(component.imageChangedEvent).toBeTruthy();
        done();
      }, 100);
    });

    it('should show error if file reading fails', (done) => {
      spyOn(component as any, 'readFileAsBase64').and.returnValue(Promise.reject('Error'));

      component.handleFileInput(mockFile);

      setTimeout(() => {
        expect(mockFeedbackService.showErrorToast).toHaveBeenCalled();
        expect(component.isProcessing).toBeFalse();
        done();
      }, 100);
    });
  });

  describe('handleFileUpload', () => {
    it('should handle file from upload event', () => {
      const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
      spyOn(component, 'handleFileInput');

      component.handleFileUpload({ files: [mockFile] });

      expect(component.handleFileInput).toHaveBeenCalledWith(mockFile);
    });

    it('should not fail if event has no files', () => {
      expect(() => component.handleFileUpload({})).not.toThrow();
    });

    it('should not fail if files array is empty', () => {
      expect(() => component.handleFileUpload({ files: [] })).not.toThrow();
    });
  });

  describe('drag and drop', () => {
    describe('handleDragEnter', () => {
      it('should set dragging state to true', () => {
        const event = new DragEvent('dragenter');
        spyOn(event, 'preventDefault');

        component.handleDragEnter(event);

        expect(event.preventDefault).toHaveBeenCalled();
        expect(component.isDraggingOver).toBeTrue();
      });
    });

    describe('handleDragLeave', () => {
      it('should set dragging state to false', () => {
        component.isDraggingOver = true;
        const event = new DragEvent('dragleave');
        spyOn(event, 'preventDefault');

        component.handleDragLeave(event);

        expect(event.preventDefault).toHaveBeenCalled();
        expect(component.isDraggingOver).toBeFalse();
      });
    });

    describe('handleDrop', () => {
      it('should handle dropped file', () => {
        const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
        const event = new DragEvent('drop');
        Object.defineProperty(event, 'dataTransfer', {
          value: { files: [mockFile] }
        });
        spyOn(event, 'preventDefault');
        spyOn(component, 'handleFileInput');

        component.handleDrop(event);

        expect(event.preventDefault).toHaveBeenCalled();
        expect(component.isDraggingOver).toBeFalse();
        expect(component.handleFileInput).toHaveBeenCalledWith(mockFile);
      });

      it('should not process if no files dropped', () => {
        const event = new DragEvent('drop');
        spyOn(component, 'handleFileInput');

        component.handleDrop(event);

        expect(component.handleFileInput).not.toHaveBeenCalled();
      });
    });
  });

  describe('image cropper events', () => {
    describe('imageCropped', () => {
      it('should update cropped image and blob', () => {
        const mockBlob = new Blob(['test'], { type: 'image/jpeg' });
        const event: ImageCroppedEvent = {
          blob: mockBlob,
          objectUrl: 'blob:test-url',
          base64: 'base64-string',
          width: 100,
          height: 100,
          cropperPosition: { x1: 0, y1: 0, x2: 100, y2: 100 },
          imagePosition: { x1: 0, y1: 0, x2: 100, y2: 100 }
        };

        component.imageCropped(event);

        expect(component.croppedBlob).toBe(mockBlob);
        expect(mockSanitizer.bypassSecurityTrustUrl).toHaveBeenCalledWith('blob:test-url');
      });

      it('should not update if blob is missing', () => {
        const event: ImageCroppedEvent = {
          blob: null,
          objectUrl: 'blob:test-url',
          base64: 'base64-string',
          width: 100,
          height: 100,
          cropperPosition: { x1: 0, y1: 0, x2: 100, y2: 100 },
          imagePosition: { x1: 0, y1: 0, x2: 100, y2: 100 }
        };

        component.imageCropped(event);

        expect(component.croppedBlob).toBeNull();
      });

      it('should not update if objectUrl is missing', () => {
        const mockBlob = new Blob(['test'], { type: 'image/jpeg' });
        const event: ImageCroppedEvent = {
          blob: mockBlob,
          objectUrl: undefined,
          base64: 'base64-string',
          width: 100,
          height: 100,
          cropperPosition: { x1: 0, y1: 0, x2: 100, y2: 100 },
          imagePosition: { x1: 0, y1: 0, x2: 100, y2: 100 }
        };

        component.imageCropped(event);

        expect(component.croppedBlob).toBeNull();
      });
    });

    describe('imageLoaded', () => {
      it('should clear processing state', () => {
        component.isProcessing = true;

        component.imageLoaded();

        expect(component.isProcessing).toBeFalse();
      });
    });

    describe('cropperReady', () => {
      it('should set cropper ready state', () => {
        component.isProcessing = true;

        component.cropperReady();

        expect(component.isProcessing).toBeFalse();
        expect(component.isCropperReady).toBeTrue();
      });
    });

    describe('loadImageFailed', () => {
      it('should show error and reset states', () => {
        component.isProcessing = true;
        component.isCropperReady = true;

        component.loadImageFailed();

        expect(mockFeedbackService.showErrorToast).toHaveBeenCalled();
        expect(component.isProcessing).toBeFalse();
        expect(component.isCropperReady).toBeFalse();
      });
    });
  });

  describe('applyChanges', () => {
    beforeEach(() => {
      const mockBlob = new Blob(['test'], { type: 'image/jpeg' });
      component.croppedBlob = mockBlob;
      component.isCropperReady = true;
    });

    it('should upload cropped image', () => {
      mockDataLoader.uploadImage.and.returnValue(of('https://example.com/image.jpg'));

      component.applyChanges();

      expect(mockDataLoader.uploadImage).toHaveBeenCalled();
    });

    it('should emit onSaveImage event on success', (done) => {
      const imageUrl = 'https://example.com/image.jpg';
      mockDataLoader.uploadImage.and.returnValue(of(imageUrl));

      component.onSaveImage.subscribe(url => {
        expect(url).toBe(imageUrl);
        done();
      });

      component.applyChanges();
    });

    it('should close dialog with image URL on success', () => {
      const imageUrl = 'https://example.com/image.jpg';
      mockDataLoader.uploadImage.and.returnValue(of(imageUrl));

      component.applyChanges();

      expect(mockDialogRef.close).toHaveBeenCalledWith(imageUrl);
    });

    it('should show error if upload fails', () => {
      mockDataLoader.uploadImage.and.returnValue(throwError(() => new Error('Upload failed')));

      component.applyChanges();

      expect(mockFeedbackService.showErrorToast).toHaveBeenCalled();
    });

    it('should not upload if cropped blob is null', () => {
      component.croppedBlob = null;

      component.applyChanges();

      expect(mockDataLoader.uploadImage).not.toHaveBeenCalled();
      expect(mockFeedbackService.showErrorToast).toHaveBeenCalled();
    });

    it('should not upload if cropper is not ready', () => {
      component.isCropperReady = false;

      component.applyChanges();

      expect(mockDataLoader.uploadImage).not.toHaveBeenCalled();
      expect(mockFeedbackService.showErrorToast).toHaveBeenCalled();
    });

    it('should create file with correct name and type', () => {
      mockDataLoader.uploadImage.and.returnValue(of('image.jpg'));

      component.applyChanges();

      const uploadCall = mockDataLoader.uploadImage.calls.mostRecent();
      const uploadedFile = uploadCall.args[0] as File;

      expect(uploadedFile.name).toBe('profile-picture.jpg');
      expect(uploadedFile.type).toBe('image/jpeg');
    });
  });

  describe('error handling', () => {
    it('should translate error messages', () => {
      (translateService.instant as jasmine.Spy).and.returnValue('Erreur traduite');

      component.loadImageFailed();

      expect(translateService.instant).toHaveBeenCalledWith('message.failedToLoadImage');
      expect(mockFeedbackService.showErrorToast).toHaveBeenCalledWith(
        jasmine.objectContaining({ detail: 'Erreur traduite' })
      );
    });
  });

  describe('readFileAsBase64', () => {
    it('should resolve with base64 data', (done) => {
      const mockFile = new File(['test content'], 'test.txt', { type: 'text/plain' });

      component['readFileAsBase64'](mockFile).then(result => {
        expect(result).toBeTruthy();
        expect(result).toContain('data:');
        done();
      });
    });

    it('should reject on read error', (done) => {
      const mockFile = new File(['test'], 'test.txt', { type: 'text/plain' });
      
      spyOn(FileReader.prototype, 'readAsDataURL').and.callFake(function(this: FileReader) {
        setTimeout(() => {
          if (this.onerror) {
            this.onerror(new ProgressEvent('error') as ProgressEvent<FileReader>);
          }
        }, 0);
      });

      component['readFileAsBase64'](mockFile).catch(error => {
        expect(error).toBeTruthy();
        done();
      });
    });
  });

  describe('edge cases', () => {
    it('should handle multiple file inputs in sequence', () => {
      const file1 = new File(['content1'], 'test1.jpg', { type: 'image/jpeg' });
      const file2 = new File(['content2'], 'test2.jpg', { type: 'image/jpeg' });

      component.handleFileInput(file1);
      expect(component.isProcessing).toBeTrue();

      component.handleFileInput(file2);
      expect(component.isCropperReady).toBeFalse();
    });

    it('should handle very large files', () => {
      const largeContent = new Array(1024 * 1024).fill('x').join(''); // 1MB
      const largeFile = new File([largeContent], 'large.jpg', { type: 'image/jpeg' });

      expect(() => component.handleFileInput(largeFile)).not.toThrow();
    });

    it('should handle files with special characters in name', () => {
      const specialFile = new File(['test'], 'test file (1) [copy].jpg', { type: 'image/jpeg' });

      expect(() => component.handleFileInput(specialFile)).not.toThrow();
    });
  });
});

