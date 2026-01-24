import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FileUploadComponent } from './file-upload.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { createMockTranslateService, createMockDialogService, createMockMarkdownService } from '@shared/testing/test-utilities';
import { AiAssistantService } from '@ai/services/ai-assistant.service';

describe('FileUploadComponent', () => {
  let component: FileUploadComponent;
  let fixture: ComponentFixture<FileUploadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        FileUploadComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [AiAssistantService]
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

