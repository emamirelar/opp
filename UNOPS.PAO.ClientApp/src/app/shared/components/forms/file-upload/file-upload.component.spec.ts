import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FileUploadComponent } from './file-upload.component';
import { TranslateModule } from '@ngx-translate/core';
import { AiAssistantService } from '@ai/services/ai-assistant.service';

const mockAiAssistantService = {
  supportedFileTypes: ['image/jpeg', 'image/png', 'application/pdf'],
  validateFiles: (files: File[]) => ({ valid: files, invalid: [] }),
  isImageFile: () => false,
  getFilePreview: () => Promise.resolve(null)
};

describe('FileUploadComponent', () => {
  let component: FileUploadComponent;
  let fixture: ComponentFixture<FileUploadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FileUploadComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: AiAssistantService, useValue: mockAiAssistantService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FileUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.maxSizeMB).toBe(10);
    expect(component.multiple).toBe(true);
    expect(component.selectedFiles).toEqual([]);
  });

  it('should handle file selection', () => {
    expect(component.hasFiles()).toBe(false);
  });
});

