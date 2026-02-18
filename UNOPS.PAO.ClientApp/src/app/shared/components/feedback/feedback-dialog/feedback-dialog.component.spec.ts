import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FeedbackDialogComponent } from './feedback-dialog.component';
import { MessageService } from 'primeng/api';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { TranslateModule } from '@ngx-translate/core';
import { of } from 'rxjs';

describe('FeedbackDialogComponent', () => {
  let component: FeedbackDialogComponent;
  let fixture: ComponentFixture<FeedbackDialogComponent>;
  let mockFeedbackService: jasmine.SpyObj<FeedbackDialogService>;

  beforeEach(async () => {
    mockFeedbackService = jasmine.createSpyObj('FeedbackDialogService', [
      'getErrorDialogState',
      'hideErrorDialog'
    ]);
    
    mockFeedbackService.getErrorDialogState.and.returnValue(of(null));

    await TestBed.configureTestingModule({
      imports: [
        FeedbackDialogComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        MessageService,
        { provide: FeedbackDialogService, useValue: mockFeedbackService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FeedbackDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize dialog state on init', () => {
    expect(mockFeedbackService.getErrorDialogState).toHaveBeenCalled();
  });

  it('should call hideErrorDialog on dialog close', () => {
    component.onDialogClose();
    expect(mockFeedbackService.hideErrorDialog).toHaveBeenCalled();
  });
});

