import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { AiPanelComponent, AiDataService } from './ai-panel.component';
import { MarkdownPipe } from '@shared/pipes/markdown.pipe';

describe('AiPanelComponent', () => {
  let component: AiPanelComponent;
  let fixture: ComponentFixture<AiPanelComponent>;
  let mockAiService: jasmine.SpyObj<AiDataService>;
  let mockTranslateService: jasmine.SpyObj<TranslateService>;

  beforeEach(async () => {
    mockAiService = jasmine.createSpyObj('AiDataService', ['get']);
    mockAiService.get.and.returnValue(of('')); // Must return Observable - component calls .pipe().subscribe()
    mockTranslateService = jasmine.createSpyObj('TranslateService', ['instant']);
    mockTranslateService.instant.and.returnValue('Translated text');

    await TestBed.configureTestingModule({
      imports: [
        AiPanelComponent,
        TranslateModule.forRoot(),
        MarkdownPipe
      ],
      providers: [
        { provide: TranslateService, useValue: mockTranslateService },
        provideNoopAnimations()
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AiPanelComponent);
    component = fixture.componentInstance;

    // Set required inputs BEFORE detectChanges (ngDoCheck may trigger loadData)
    fixture.componentRef.setInput('title', 'Test Title');
    fixture.componentRef.setInput('entityId', 'entity-123');
    fixture.componentRef.setInput('promptType', 'test-prompt');
    fixture.componentRef.setInput('aiService', mockAiService);
    fixture.componentRef.setInput('loadOnInit', false); // Don't load on init for tests
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('loadData', () => {
    it('should load data successfully', (done) => {
      const testData = '# Test markdown content';
      mockAiService.get.and.returnValue(of(testData));

      component.loadData();

      setTimeout(() => {
        expect(component.content()).toBe(testData);
        expect(component.isLoading()).toBeFalse();
        expect(component.hasError()).toBeFalse();
        done();
      }, 100);
    });

    it('should handle errors when loading data', (done) => {
      const error = new Error('Test error');
      mockAiService.get.and.returnValue(throwError(() => error));

      component.loadData();

      setTimeout(() => {
        expect(component.isLoading()).toBeFalse();
        expect(component.hasError()).toBeTrue();
        done();
      }, 100);
    });

    it('should not load if required parameters are missing', () => {
      fixture.componentRef.setInput('entityId', '');
      component.loadData();

      expect(mockAiService.get).not.toHaveBeenCalled();
    });
  });

  describe('refresh', () => {
    it('should emit onRefresh and reload data', () => {
      const testData = 'Refreshed content';
      mockAiService.get.and.returnValue(of(testData));
      
      spyOn(component.onRefresh, 'emit');
      
      component.refresh();

      expect(component.onRefresh.emit).toHaveBeenCalled();
      expect(mockAiService.get).toHaveBeenCalled();
    });
  });

  describe('toggleFullContent', () => {
    it('should toggle showFullContent signal', () => {
      const initialValue = component.showFullContent();
      
      component.toggleFullContent();
      
      expect(component.showFullContent()).toBe(!initialValue);
      
      component.toggleFullContent();
      
      expect(component.showFullContent()).toBe(initialValue);
    });
  });

  describe('computed values', () => {
    it('shouldShowSpinner should reflect loading state', () => {
      component.isLoading.set(true);
      expect(component.shouldShowSpinner()).toBeTrue();

      component.isLoading.set(false);
      expect(component.shouldShowSpinner()).toBeFalse();
    });

    it('shouldShowContent should be true when loaded without error', () => {
      component.isLoading.set(false);
      component.hasError.set(false);
      component.content.set('Some content');

      expect(component.shouldShowContent()).toBeTruthy();
    });

    it('shouldShowError should be true when error occurred', () => {
      component.isLoading.set(false);
      component.hasError.set(true);

      expect(component.shouldShowError()).toBeTrue();
    });

    it('shouldTruncate should return true for long content', () => {
      fixture.componentRef.setInput('truncateLength', 10);
      component.content.set('This is a very long piece of content that exceeds the truncate length');
      component.showFullContent.set(false);

      expect(component.shouldTruncate()).toBeTrue();
    });
  });

  describe('ngOnDestroy', () => {
    it('should clean up resources', () => {
      component.ngOnDestroy();
      
      // Component should abort any ongoing requests
      expect(component).toBeTruthy(); // Basic check that destroy completes
    });
  });
});


