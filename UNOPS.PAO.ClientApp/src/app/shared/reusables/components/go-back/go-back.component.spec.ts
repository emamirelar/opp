import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { GoBackComponent } from './go-back.component';

describe('GoBackComponent', () => {
  let component: GoBackComponent;
  let fixture: ComponentFixture<GoBackComponent>;
  let mockRouter: jasmine.SpyObj<Router>;

  beforeEach(async () => {
    mockRouter = jasmine.createSpyObj('Router', ['navigateByUrl']);

    await TestBed.configureTestingModule({
      imports: [
        GoBackComponent,
        TranslateModule.forRoot()
      ],
      providers: [
        { provide: Router, useValue: mockRouter }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(GoBackComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should set previousUrl from history state', () => {
      const testUrl = '/previous-page';
      spyOnProperty(history, 'state', 'get').and.returnValue({ previousUrl: testUrl });

      component.ngOnInit();

      expect(component['previousUrl']).toBe(testUrl);
    });

    it('should set previousUrl to undefined when no history state', () => {
      spyOnProperty(history, 'state', 'get').and.returnValue({});

      component.ngOnInit();

      expect(component['previousUrl']).toBeUndefined();
    });
  });

  describe('goBack', () => {
    it('should navigate to previousUrl if it exists', () => {
      const testUrl = '/previous-page';
      component['previousUrl'] = testUrl;

      component.goBack();

      expect(mockRouter.navigateByUrl).toHaveBeenCalledWith(testUrl);
    });

    it('should call window.history.back() if no previousUrl', () => {
      spyOn(window.history, 'back');
      component['previousUrl'] = undefined;

      component.goBack();

      expect(window.history.back).toHaveBeenCalled();
      expect(mockRouter.navigateByUrl).not.toHaveBeenCalled();
    });
  });

  describe('template rendering', () => {
    it('should render back button', () => {
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement;
      const button = compiled.querySelector('p-button');
      
      expect(button).toBeTruthy();
    });

    it('should call goBack when button is clicked', () => {
      spyOn(component, 'goBack');
      fixture.detectChanges();

      const button = fixture.nativeElement.querySelector('button');
      button.click();

      expect(component.goBack).toHaveBeenCalled();
    });
  });
});


