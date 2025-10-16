import { ComponentFixture, TestBed } from '@angular/core/testing';
import { IapStatusComponent } from './iap-status.component';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '@core/services/auth';
import { of } from 'rxjs';

describe('IapStatusComponent', () => {
  let component: IapStatusComponent;
  let fixture: ComponentFixture<IapStatusComponent>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    mockAuthService = jasmine.createSpyObj('AuthService', [
      'isIapAuthenticated',
      'isLogedIn',
      'getAuthInfo'
    ]);
    
    mockAuthService.isIapAuthenticated.and.returnValue(of(false));
    mockAuthService.isLogedIn.and.returnValue(of(false));
    mockAuthService.getAuthInfo.and.returnValue(of({}));

    await TestBed.configureTestingModule({
      imports: [
        IapStatusComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: AuthService, useValue: mockAuthService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(IapStatusComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should check authentication status on init', () => {
    expect(mockAuthService.isIapAuthenticated).toHaveBeenCalled();
    expect(mockAuthService.isLogedIn).toHaveBeenCalled();
  });
});

