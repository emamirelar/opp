import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { of } from 'rxjs';
import { DocumentListComponent } from './document-list.component';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';

describe('DocumentListComponent', () => {
  let component: DocumentListComponent;
  let fixture: ComponentFixture<DocumentListComponent>;
  let mockMessageService: jasmine.SpyObj<FeedbackDialogService>;
  let mockTranslateService: jasmine.SpyObj<TranslateService>;

  beforeEach(async () => {
    mockMessageService = jasmine.createSpyObj('FeedbackDialogService', ['showErrorDialog']);
    mockTranslateService = jasmine.createSpyObj('TranslateService', ['instant', 'get'], {
      onLangChange: of({ lang: 'en' }),
      onTranslationChange: of({ lang: 'en', translations: {} }),
      onDefaultLangChange: of({ lang: 'en', translations: {} })
    });
    mockTranslateService.instant.and.returnValue('Translated text');
    mockTranslateService.get.and.returnValue(of('Translated text'));

    await TestBed.configureTestingModule({
      imports: [
        DocumentListComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: FeedbackDialogService, useValue: mockMessageService },
        { provide: TranslateService, useValue: mockTranslateService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('openDocument', () => {
    it('should open document in new window when link is provided', () => {
      const mockWindow = {
        opener: {},
        focus: jasmine.createSpy('focus')
      };
      spyOn(window, 'open').and.returnValue(mockWindow as any);

      component.openDocument('https://example.com/document.pdf');

      expect(window.open).toHaveBeenCalledWith('https://example.com/document.pdf', '_blank');
      expect(mockWindow.opener).toBeNull();
      expect(mockWindow.focus).toHaveBeenCalled();
    });

    it('should show error when document link is not provided', () => {
      component.openDocument('');

      expect(mockMessageService.showErrorDialog).toHaveBeenCalledWith({
        summary: 'Translated text',
        detail: 'Translated text'
      });
    });

    it('should show error when document link is null', () => {
      component.openDocument(null as any);

      expect(mockMessageService.showErrorDialog).toHaveBeenCalled();
    });

    it('should handle when window.open returns null', () => {
      spyOn(window, 'open').and.returnValue(null);

      component.openDocument('https://example.com/document.pdf');

      // Should not throw error, just handle gracefully
      expect(window.open).toHaveBeenCalled();
      expect(mockMessageService.showErrorDialog).not.toHaveBeenCalled();
    });
  });

  describe('documents input', () => {
    it('should accept documents array', () => {
      const mockDocuments = [
        { id: '1', name: 'Document 1', link: 'https://example.com/doc1.pdf' },
        { id: '2', name: 'Document 2', link: 'https://example.com/doc2.pdf' }
      ];

      component.documents = mockDocuments;
      fixture.detectChanges();

      expect(component.documents).toEqual(mockDocuments);
      expect(component.documents.length).toBe(2);
    });

    it('should have empty array as default', () => {
      expect(component.documents).toEqual([]);
    });
  });

  describe('template rendering', () => {
    it('should render document list when documents are provided', () => {
      component.documents = [
        { id: '1', name: 'Document 1', link: 'https://example.com/doc1.pdf' }
      ];
      fixture.detectChanges();

      const compiled = fixture.nativeElement;
      expect(compiled).toBeTruthy();
    });

    it('should handle empty documents array', () => {
      component.documents = [];
      fixture.detectChanges();

      const compiled = fixture.nativeElement;
      expect(compiled).toBeTruthy();
    });
  });
});


