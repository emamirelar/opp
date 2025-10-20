import { ComponentFixture, TestBed } from '@angular/core/testing';
import { LoadingOverlayComponent, LoadingOverlayService } from './loading-overlay.component';

describe('LoadingOverlayComponent', () => {
  let component: LoadingOverlayComponent;
  let fixture: ComponentFixture<LoadingOverlayComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoadingOverlayComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(LoadingOverlayComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('show', () => {
    it('should show loading overlay with default message', (done) => {
      component.show();

      component.loading$.subscribe(loading => {
        expect(loading).toBeTrue();
        expect(component.message).toBe('Loading...');
        done();
      });
    });

    it('should show loading overlay with custom message', (done) => {
      const customMessage = 'Custom loading message';
      
      component.show(customMessage);

      component.loading$.subscribe(loading => {
        expect(loading).toBeTrue();
        expect(component.message).toBe(customMessage);
        done();
      });
    });
  });

  describe('hide', () => {
    it('should hide loading overlay', (done) => {
      component.show();
      component.hide();

      component.loading$.subscribe(loading => {
        expect(loading).toBeFalse();
        done();
      });
    });
  });

  describe('template rendering', () => {
    it('should not render overlay when not loading', () => {
      component.hide();
      fixture.detectChanges();

      const overlay = fixture.nativeElement.querySelector('.fixed');
      expect(overlay).toBeNull();
    });

    it('should render overlay when loading', (done) => {
      component.show('Test message');
      
      setTimeout(() => {
        fixture.detectChanges();
        const overlay = fixture.nativeElement.querySelector('.fixed');
        expect(overlay).toBeTruthy();
        done();
      }, 100);
    });
  });
});

describe('LoadingOverlayService', () => {
  let service: LoadingOverlayService;
  let mockComponent: jasmine.SpyObj<LoadingOverlayComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoadingOverlayService);
    
    mockComponent = jasmine.createSpyObj('LoadingOverlayComponent', ['show', 'hide']);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('registerComponent', () => {
    it('should register component', () => {
      service.registerComponent(mockComponent as any);
      
      service.show();
      
      expect(mockComponent.show).toHaveBeenCalled();
    });
  });

  describe('show', () => {
    it('should call component show with default message', () => {
      service.registerComponent(mockComponent as any);
      
      service.show();
      
      expect(mockComponent.show).toHaveBeenCalledWith('Loading...');
    });

    it('should call component show with custom message', () => {
      service.registerComponent(mockComponent as any);
      const message = 'Custom message';
      
      service.show(message);
      
      expect(mockComponent.show).toHaveBeenCalledWith(message);
    });

    it('should warn if component not registered', () => {
      spyOn(console, 'warn');
      
      service.show();
      
      expect(console.warn).toHaveBeenCalledWith('LoadingOverlay component not registered');
    });
  });

  describe('hide', () => {
    it('should call component hide', () => {
      service.registerComponent(mockComponent as any);
      
      service.hide();
      
      expect(mockComponent.hide).toHaveBeenCalled();
    });

    it('should not throw error if component not registered', () => {
      expect(() => service.hide()).not.toThrow();
    });
  });
});


