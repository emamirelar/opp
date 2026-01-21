import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TourControlComponent } from './tour-control.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';
import { WelcomeTourService } from '@shared/services/ui/welcome-tour.service';

describe('TourControlComponent', () => {
  let component: TourControlComponent;
  let fixture: ComponentFixture<TourControlComponent>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockWelcomeTourService: jasmine.SpyObj<WelcomeTourService>;

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate'], { url: '/test' });
    mockWelcomeTourService = jasmine.createSpyObj('WelcomeTourService', [
      'resetWelcomeTourState',
      'markTourCompleted'
    ]);

    await TestBed.configureTestingModule({
      imports: [
        TourControlComponent,
        HttpClientTestingModule,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: WelcomeTourService, useValue: mockWelcomeTourService }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TourControlComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize with default values', () => {
    expect(component.hideNotificationDot).toBe(false);
    expect(component.customTourFile).toBeUndefined();
  });

  it('should show notification dot by default', () => {
    expect(component.showNotificationDot()).toBe(true);
  });

  it('should hide notification dot when hideNotificationDot is true', () => {
    component.hideNotificationDot = true;
    expect(component.showNotificationDot()).toBe(false);
  });

  it('should reset welcome tour when resetWelcomeTour is called', () => {
    component.resetWelcomeTour();
    expect(mockWelcomeTourService.resetWelcomeTourState).toHaveBeenCalled();
  });

  it('should handle custom tour file input', () => {
    component.customTourFile = 'custom-tour';
    expect(component.customTourFile).toBe('custom-tour');
  });
});

