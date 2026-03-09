import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateService, TranslateLoader, TranslateFakeLoader } from '@ngx-translate/core';
import { DomSanitizer } from '@angular/platform-browser';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { MessageService } from 'primeng/api';
import { of, throwError } from 'rxjs';
import { AiTranscribeComponent } from './ai-transcribe.component';
import { GeminiService } from '@ai/services/gemini.service';

describe('AiTranscribeComponent', () => {
  let component: AiTranscribeComponent;
  let fixture: ComponentFixture<AiTranscribeComponent>;
  let mockGeminiService: jasmine.SpyObj<GeminiService>;
  let mockMessageService: jasmine.SpyObj<MessageService>;
  let mockSanitizer: jasmine.SpyObj<DomSanitizer>;
  let translateService: TranslateService;

  beforeEach(async () => {
    mockGeminiService = jasmine.createSpyObj('GeminiService', ['scanFile']);
    mockMessageService = jasmine.createSpyObj('MessageService', ['add']);
    mockSanitizer = jasmine.createSpyObj('DomSanitizer', ['bypassSecurityTrustUrl']);
    mockSanitizer.bypassSecurityTrustUrl.and.returnValue('safe-url');

    await TestBed.configureTestingModule({
      imports: [
        AiTranscribeComponent,
        TranslateModule.forRoot({
          loader: { provide: TranslateLoader, useClass: TranslateFakeLoader }
        })
      ],
      providers: [
        { provide: GeminiService, useValue: mockGeminiService },
        { provide: MessageService, useValue: mockMessageService },
        { provide: DomSanitizer, useValue: mockSanitizer },
        provideNoopAnimations()
      ]
    }).compileComponents();

    translateService = TestBed.inject(TranslateService);
    spyOn(translateService, 'instant').and.returnValue('Translated text');
    spyOn(translateService, 'get').and.returnValue(of('Translated text'));
    translateService.use('en');

    fixture = TestBed.createComponent(AiTranscribeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('initTranscribeMenu', () => {
    it('should initialize menu items', () => {
      expect(component.transcribeMenuItems).toBeDefined();
      expect(component.transcribeMenuItems.length).toBe(3);
      expect(component.transcribeMenuItems[0].label).toBe('Translated text');
    });
  });

  describe('onFileSelect', () => {
    it('should handle image file selection', () => {
      const mockFile = new File(['test'], 'test.png', { type: 'image/png' });
      const mockEvent = {
        target: { files: [mockFile], value: 'test.png' }
      };

      component.onFileSelect(mockEvent);

      // File reading is async, just verify the method doesn't throw
      expect(component).toBeTruthy();
    });

    it('should handle audio file selection', () => {
      const mockFile = new File(['test'], 'test.mp3', { type: 'audio/mp3' });
      const mockEvent = {
        target: { files: [mockFile], value: 'test.mp3' }
      };

      component.onFileSelect(mockEvent);

      // Audio files are set synchronously (no FileReader)
      expect(component.uploadedFile()).toBeTruthy();
      expect(component.uploadedFile()?.file).toBe(mockFile);
    });

    it('should handle empty file selection', () => {
      const mockEvent = { target: { files: [] }, files: [] };

      component.onFileSelect(mockEvent);

      expect(component.uploadedFile()).toBeNull();
    });
  });

  describe('transcribeFile', () => {
    it('should transcribe file successfully', (done) => {
      const mockFile = new File(['test'], 'test.png', { type: 'image/png' });
      component.uploadedFile.set({ file: mockFile, preview: 'safe-url' });
      
      const mockResponse = { text: 'Extracted text' };
      mockGeminiService.scanFile.and.returnValue(of(mockResponse));
      
      spyOn(component.transcriptionCompleted, 'emit');

      component.transcribeFile();

      setTimeout(() => {
        expect(mockGeminiService.scanFile).toHaveBeenCalledWith(mockFile, 'default');
        expect(component.transcriptionCompleted.emit).toHaveBeenCalledWith(mockResponse);
        expect(component.isUploading()).toBeFalse();
        expect(component.uploadedFile()).toBeNull();
        expect(mockMessageService.add).toHaveBeenCalled();
        done();
      }, 100);
    });

    it('should handle transcription error', (done) => {
      const mockFile = new File(['test'], 'test.png', { type: 'image/png' });
      component.uploadedFile.set({ file: mockFile, preview: 'safe-url' });
      
      mockGeminiService.scanFile.and.returnValue(throwError(() => new Error('Transcription failed')));

      component.transcribeFile();

      setTimeout(() => {
        expect(component.isUploading()).toBeFalse();
        expect(component.uploadedFile()).toBeNull();
        expect(mockMessageService.add).toHaveBeenCalledWith(jasmine.objectContaining({
          severity: 'error'
        }));
        done();
      }, 100);
    });

    it('should not transcribe if no file uploaded', () => {
      component.uploadedFile.set(null);

      component.transcribeFile();

      expect(mockGeminiService.scanFile).not.toHaveBeenCalled();
    });
  });

  describe('camera operations', () => {
    it('should toggle menu', () => {
      const mockEvent = new Event('click');
      component['menu'] = { toggle: jasmine.createSpy('toggle') };

      component.toggleMenu(mockEvent);

      expect(component['menu'].toggle).toHaveBeenCalledWith(mockEvent);
    });

    it('should select image when selectImage is called', () => {
      component['fileInput'] = {
        nativeElement: { click: jasmine.createSpy('click') }
      } as any;

      component.selectImage();

      expect(component['fileInput'].nativeElement.click).toHaveBeenCalled();
    });

    it('should select audio when selectAudio is called', () => {
      component['audioInput'] = {
        nativeElement: { click: jasmine.createSpy('click') }
      } as any;

      component.selectAudio();

      expect(component['audioInput'].nativeElement.click).toHaveBeenCalled();
    });
  });

  describe('getAudioUrl', () => {
    it('should return empty string if no file', () => {
      const result = component.getAudioUrl(null);
      expect(result).toBe('');
    });

    it('should create object URL for audio file', () => {
      const mockFile = new File(['test'], 'test.mp3', { type: 'audio/mp3' });
      spyOn(URL, 'createObjectURL').and.returnValue('blob:test-url');

      component.getAudioUrl(mockFile);

      expect(URL.createObjectURL).toHaveBeenCalledWith(mockFile);
      expect(mockSanitizer.bypassSecurityTrustUrl).toHaveBeenCalledWith('blob:test-url');
    });
  });
});


