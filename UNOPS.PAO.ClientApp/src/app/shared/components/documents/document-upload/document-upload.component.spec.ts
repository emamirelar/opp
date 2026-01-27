import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { DocumentUploadComponent } from './document-upload.component';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';

describe('DocumentUploadComponent', () => {
  let component: DocumentUploadComponent;
  let fixture: ComponentFixture<DocumentUploadComponent>;
  let mockFeedbackService: jasmine.SpyObj<FeedbackDialogService>;

  beforeEach(async () => {
    mockFeedbackService = jasmine.createSpyObj('FeedbackDialogService', ['showInfoToast']);

    await TestBed.configureTestingModule({
      imports: [
        DocumentUploadComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: FeedbackDialogService, useValue: mockFeedbackService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('input properties', () => {
    it('should have default accept value', () => {
      expect(component.accept).toBe('image/*,application/pdf');
    });

    it('should have default maxFileSize value', () => {
      expect(component.maxFileSize).toBe(1000000);
    });

    it('should have default multiple value', () => {
      expect(component.multiple).toBeTrue();
    });

    it('should accept custom accept value', () => {
      component.accept = 'application/pdf';
      expect(component.accept).toBe('application/pdf');
    });

    it('should accept custom maxFileSize value', () => {
      component.maxFileSize = 5000000;
      expect(component.maxFileSize).toBe(5000000);
    });

    it('should accept custom multiple value', () => {
      component.multiple = false;
      expect(component.multiple).toBeFalse();
    });
  });

  describe('onUpload', () => {
    it('should add files to uploadedFiles array', () => {
      const mockFiles = [
        { name: 'file1.pdf', size: 1000 },
        { name: 'file2.pdf', size: 2000 }
      ];
      const mockEvent = { files: mockFiles };

      component.onUpload(mockEvent);

      expect(component.uploadedFiles.length).toBe(2);
      expect(component.uploadedFiles).toContain(mockFiles[0]);
      expect(component.uploadedFiles).toContain(mockFiles[1]);
    });

    it('should emit fileUploaded event', () => {
      const mockEvent = { files: [{ name: 'test.pdf' }] };
      spyOn(component.fileUploaded, 'emit');

      component.onUpload(mockEvent);

      expect(component.fileUploaded.emit).toHaveBeenCalledWith(mockEvent);
    });

    it('should handle empty files array', () => {
      const mockEvent = { files: [] };

      component.onUpload(mockEvent);

      expect(component.uploadedFiles.length).toBe(0);
    });
  });

  describe('onSelect', () => {
    it('should show info toast with file count', () => {
      const mockEvent = {
        currentFiles: [{ name: 'file1.pdf' }, { name: 'file2.pdf' }]
      };

      component.onSelect(mockEvent);

      expect(mockFeedbackService.showInfoToast).toHaveBeenCalledWith({
        detail: '2 file(s) ready for upload.'
      });
    });

    it('should emit fileSelected event', () => {
      const mockEvent = { currentFiles: [{ name: 'test.pdf' }] };
      spyOn(component.fileSelected, 'emit');

      component.onSelect(mockEvent);

      expect(component.fileSelected.emit).toHaveBeenCalledWith(mockEvent);
    });
  });

  describe('onRemove', () => {
    it('should remove file from uploadedFiles array', () => {
      const mockFile = { name: 'test.pdf', size: 1000 };
      component.uploadedFiles = [mockFile];

      component.onRemove({ file: mockFile });

      expect(component.uploadedFiles.length).toBe(0);
    });

    it('should show info toast when file is removed', () => {
      const mockFile = { name: 'test.pdf' };
      component.uploadedFiles = [mockFile];

      component.onRemove({ file: mockFile });

      expect(mockFeedbackService.showInfoToast).toHaveBeenCalledWith({
        detail: 'File removed successfully!'
      });
    });

    it('should emit fileRemoved event', () => {
      const mockFile = { name: 'test.pdf' };
      component.uploadedFiles = [mockFile];
      spyOn(component.fileRemoved, 'emit');

      component.onRemove({ file: mockFile });

      expect(component.fileRemoved.emit).toHaveBeenCalledWith({ file: mockFile });
    });

    it('should handle removal of non-existent file', () => {
      const mockFile1 = { name: 'test1.pdf' };
      const mockFile2 = { name: 'test2.pdf' };
      component.uploadedFiles = [mockFile1];

      component.onRemove({ file: mockFile2 });

      // Should not throw error
      expect(component.uploadedFiles.length).toBe(1);
      expect(component.uploadedFiles).toContain(mockFile1);
    });
  });

  describe('clearFiles', () => {
    it('should clear all uploaded files', () => {
      component.uploadedFiles = [
        { name: 'file1.pdf' },
        { name: 'file2.pdf' }
      ];

      component.clearFiles();

      expect(component.uploadedFiles.length).toBe(0);
    });

    it('should show info toast when files are cleared', () => {
      component.clearFiles();

      expect(mockFeedbackService.showInfoToast).toHaveBeenCalledWith({
        detail: 'All files have been cleared!'
      });
    });

    it('should emit filesCleared event', () => {
      spyOn(component.filesCleared, 'emit');

      component.clearFiles();

      expect(component.filesCleared.emit).toHaveBeenCalled();
    });

    it('should work when uploadedFiles is already empty', () => {
      component.uploadedFiles = [];

      component.clearFiles();

      expect(component.uploadedFiles.length).toBe(0);
      expect(mockFeedbackService.showInfoToast).toHaveBeenCalled();
    });
  });

  describe('event emitters', () => {
    it('should have fileUploaded output', () => {
      expect(component.fileUploaded).toBeDefined();
    });

    it('should have fileSelected output', () => {
      expect(component.fileSelected).toBeDefined();
    });

    it('should have fileRemoved output', () => {
      expect(component.fileRemoved).toBeDefined();
    });

    it('should have filesCleared output', () => {
      expect(component.filesCleared).toBeDefined();
    });
  });
});


