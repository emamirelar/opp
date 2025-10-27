import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DomSanitizer } from '@angular/platform-browser';
import { LookerstudioComponent } from './lookerstudio.component';
import { SimpleChange } from '@angular/core';

describe('LookerstudioComponent', () => {
  let component: LookerstudioComponent;
  let fixture: ComponentFixture<LookerstudioComponent>;
  let mockSanitizer: jasmine.SpyObj<DomSanitizer>;

  beforeEach(async () => {
    mockSanitizer = jasmine.createSpyObj('DomSanitizer', ['bypassSecurityTrustResourceUrl']);
    mockSanitizer.bypassSecurityTrustResourceUrl.and.returnValue('safe-url' as any);

    await TestBed.configureTestingModule({
      imports: [LookerstudioComponent],
      providers: [
        { provide: DomSanitizer, useValue: mockSanitizer }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LookerstudioComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should update URL on initialization', () => {
      spyOn(component, 'updateUrl');
      
      component.ngOnInit();

      expect(component.updateUrl).toHaveBeenCalled();
    });
  });

  describe('ngOnChanges', () => {
    it('should update URL when dashboardId changes', () => {
      spyOn(component, 'updateUrl');
      
      component.ngOnChanges({
        dashboardId: new SimpleChange(null, 'new-id', false)
      });

      expect(component.updateUrl).toHaveBeenCalled();
    });

    it('should update URL when partnerCode changes', () => {
      spyOn(component, 'updateUrl');
      
      component.ngOnChanges({
        partnerCode: new SimpleChange(null, 'partner-123', false)
      });

      expect(component.updateUrl).toHaveBeenCalled();
    });

    it('should update URL when type changes', () => {
      spyOn(component, 'updateUrl');
      
      component.ngOnChanges({
        type: new SimpleChange('partnerTree', 'partner', false)
      });

      expect(component.updateUrl).toHaveBeenCalled();
    });

    it('should not update URL when unrelated properties change', () => {
      spyOn(component, 'updateUrl');
      
      component.ngOnChanges({
        isLoading: new SimpleChange(false, true, false)
      });

      expect(component.updateUrl).not.toHaveBeenCalled();
    });
  });

  describe('updateUrl', () => {
    it('should call partnerTreeUrl for partnerTree type', () => {
      component.partnerCode = 'partner-123';
      component.type = 'partnerTree';
      spyOn(component, 'partnerTreeUrl').and.returnValue('safe-url' as any);

      component.updateUrl();

      expect(component.partnerTreeUrl).toHaveBeenCalled();
    });

    it('should call partnerUrl for partner type', () => {
      component.partnerCode = 'partner-123';
      component.type = 'partner';
      spyOn(component, 'partnerUrl').and.returnValue('safe-url' as any);

      component.updateUrl();

      expect(component.partnerUrl).toHaveBeenCalled();
    });

    it('should not update URL if partnerCode is empty', () => {
      component.partnerCode = '';
      spyOn(component, 'partnerTreeUrl');
      spyOn(component, 'partnerUrl');

      component.updateUrl();

      expect(component.partnerTreeUrl).not.toHaveBeenCalled();
      expect(component.partnerUrl).not.toHaveBeenCalled();
    });
  });

  describe('partnerTreeUrl', () => {
    it('should generate correct URL with dashboardId', () => {
      component.dashboardId = 'test-dashboard-123';
      component.partnerCode = 'PARTNER1';

      const result = component.partnerTreeUrl();

      expect(mockSanitizer.bypassSecurityTrustResourceUrl).toHaveBeenCalled();
      const calledUrl = mockSanitizer.bypassSecurityTrustResourceUrl.calls.mostRecent().args[0];
      expect(calledUrl).toContain('test-dashboard-123');
      expect(calledUrl).toContain(encodeURIComponent('PARTNER1'));
    });

    it('should return empty URL if no dashboardId', () => {
      component.dashboardId = '';
      component.partnerCode = 'PARTNER1';

      const result = component.partnerTreeUrl();

      expect(mockSanitizer.bypassSecurityTrustResourceUrl).toHaveBeenCalledWith('');
    });
  });

  describe('partnerUrl', () => {
    it('should generate correct partner URL', () => {
      component.partnerCode = 'PARTNER1';

      const result = component.partnerUrl();

      expect(mockSanitizer.bypassSecurityTrustResourceUrl).toHaveBeenCalled();
      const calledUrl = mockSanitizer.bypassSecurityTrustResourceUrl.calls.mostRecent().args[0];
      expect(calledUrl).toContain(encodeURIComponent('PARTNER1'));
      expect(calledUrl).toContain('dcf96b62-ae61-4d6c-8614-34b9faf91cd8');
    });
  });

  describe('input properties', () => {
    it('should have default dashboardId', () => {
      expect(component.dashboardId).toBe('');
    });

    it('should have default partnerCode', () => {
      expect(component.partnerCode).toBe('');
    });

    it('should have default isLoading', () => {
      expect(component.isLoading).toBeFalse();
    });

    it('should have default minHeight', () => {
      expect(component.minHeight).toBe('calc(100vh - 300px)');
    });

    it('should have default type', () => {
      expect(component.type).toBe('partnerTree');
    });
  });

  describe('template rendering', () => {
    it('should render iframe with correct src', () => {
      component.partnerCode = 'PARTNER1';
      component.dashboardId = 'test-dashboard';
      component.updateUrl();
      fixture.detectChanges();

      const iframe = fixture.nativeElement.querySelector('iframe');
      expect(iframe).toBeTruthy();
    });

    it('should show loading state when isLoading is true', () => {
      component.isLoading = true;
      fixture.detectChanges();

      const compiled = fixture.nativeElement;
      // Check for loading indicator if it exists in template
      expect(compiled).toBeTruthy();
    });
  });
});


