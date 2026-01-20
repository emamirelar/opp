import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { AuthService } from '@core/services/auth';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { SignUpComponent } from './sign-up.component';

describe('SignUpComponent', () => {
  let component: SignUpComponent;
  let fixture: ComponentFixture<SignUpComponent>;

  beforeEach(async () => {
    const authService = jasmine.createSpyObj('AuthService', ['signUp']);
    const feedbackDialogService = jasmine.createSpyObj('FeedbackDialogService', ['showSuccessToast', 'showErrorToast']);

    authService.signUp.and.returnValue(of({}));

    await TestBed.configureTestingModule({
      imports: [SignUpComponent, NoopAnimationsModule],
      providers: [
        { provide: AuthService, useValue: authService },
        { provide: FeedbackDialogService, useValue: feedbackDialogService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SignUpComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

