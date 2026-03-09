import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { DialogService } from 'primeng/dynamicdialog';
import { LoginComponent } from './login.component';
import { AuthService } from '@core/services/auth';
import { of } from 'rxjs';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  beforeEach(async () => {
    const mockAuthService = jasmine.createSpyObj('AuthService', ['getAuthInfo', 'logIn', 'resetAuthenticationState']);
    mockAuthService.getAuthInfo.and.returnValue(of(null));
    mockAuthService.resetAuthenticationState = jasmine.createSpy('resetAuthenticationState');

    await TestBed.configureTestingModule({
      imports: [LoginComponent, HttpClientTestingModule],
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: DialogService, useValue: {} },
        { provide: 'SocialAuthServiceConfig', useValue: { autoLogin: false, providers: [] } }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});

