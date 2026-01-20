import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, NavigationEnd } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ResponsiveTabsComponent } from './responsive-tabs.component';
import { ResponsiveTabItem } from './responsive-tabs.model';
import { Subject } from 'rxjs';

describe('ResponsiveTabsComponent', () => {
  let component: ResponsiveTabsComponent;
  let fixture: ComponentFixture<ResponsiveTabsComponent>;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockTranslateService: jasmine.SpyObj<TranslateService>;
  let routerEventsSubject: Subject<any>;
  let langChangeSubject: Subject<any>;
  const setRouterUrl = (url: string) => {
    Object.defineProperty(mockRouter, 'url', {
      get: () => url,
      configurable: true
    });
  };

  const mockTabs: ResponsiveTabItem[] = [
    { route: '/tab1', label: 'tab.one', icon: 'home', disabled: false },
    { route: '/tab2', label: 'tab.two', icon: 'settings', disabled: false },
    { route: '/tab3', label: 'tab.three', icon: 'info', disabled: true }
  ];

  beforeEach(async () => {
    routerEventsSubject = new Subject();
    langChangeSubject = new Subject();

    mockRouter = jasmine.createSpyObj('Router', ['navigate'], {
      events: routerEventsSubject.asObservable()
    });
    setRouterUrl('/tab1');

    mockTranslateService = jasmine.createSpyObj('TranslateService', ['instant'], {
      onLangChange: langChangeSubject.asObservable()
    });

    mockTranslateService.instant.and.callFake((key: string) => `Translated ${key}`);

    await TestBed.configureTestingModule({
      imports: [ResponsiveTabsComponent, TranslateModule.forRoot()],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: TranslateService, useValue: mockTranslateService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ResponsiveTabsComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('initialization', () => {
    it('should have default values', () => {
      expect(component.tabs).toEqual([]);
      expect(component.disabled).toBeFalse();
      expect(component.dropdownPlaceholder).toBe('Select tab');
      expect(component.breakpoint).toBe(768);
    });

    it('should initialize with provided tabs', () => {
      component.tabs = mockTabs;
      fixture.detectChanges();

      expect(component.tabs.length).toBe(3);
      expect(component.tabs[0].route).toBe('/tab1');
    });

    it('should translate tab labels on init', () => {
      component.tabs = mockTabs;
      fixture.detectChanges();

      expect(mockTranslateService.instant).toHaveBeenCalledWith('tab.one');
      expect(component.tabs[0].translatedLabel).toBe('Translated tab.one');
    });

    it('should set active tab based on current route', () => {
      setRouterUrl('/tab2');
      component.tabs = mockTabs;
      fixture.detectChanges();

      expect(component.activeRoute).toBe('/tab2');
    });

    it('should default to first non-disabled tab if no match', () => {
      setRouterUrl('/unknown');
      component.tabs = mockTabs;
      fixture.detectChanges();

      expect(component.activeRoute).toBe('/tab1');
    });

    it('should handle tabs with all disabled', () => {
      const allDisabledTabs = mockTabs.map(t => ({ ...t, disabled: true }));
      component.tabs = allDisabledTabs;
      fixture.detectChanges();

      expect(component.activeRoute).toBe('');
    });
  });

  describe('mobile/desktop view detection', () => {
    it('should detect mobile view on small screens', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(500);
      component.ngOnInit();

      expect(component.isMobileView).toBeTrue();
    });

    it('should detect desktop view on large screens', () => {
      spyOnProperty(window, 'innerWidth').and.returnValue(1024);
      component.ngOnInit();

      expect(component.isMobileView).toBeFalse();
    });

    it('should update view mode on window resize', () => {
      component.breakpoint = 768;
      spyOnProperty(window, 'innerWidth').and.returnValue(500);
      
      component.onResize({});

      expect(component.isMobileView).toBeTrue();

      spyOnProperty(window, 'innerWidth').and.returnValue(1024);
      component.onResize({});

      expect(component.isMobileView).toBeFalse();
    });

    it('should respect custom breakpoint', () => {
      component.breakpoint = 1024;
      spyOnProperty(window, 'innerWidth').and.returnValue(800);
      
      component.onResize({});

      expect(component.isMobileView).toBeTrue();
    });
  });

  describe('router navigation handling', () => {
    it('should update active tab on navigation', () => {
      component.tabs = mockTabs;
      fixture.detectChanges();

      routerEventsSubject.next(new NavigationEnd(1, '/tab2', '/tab2'));

      expect(component.activeRoute).toBe('/tab2');
    });

    it('should handle navigation to child routes', () => {
      component.tabs = mockTabs;
      setRouterUrl('/tab1/details');
      fixture.detectChanges();

      routerEventsSubject.next(new NavigationEnd(1, '/tab1/details', '/tab1/details'));

      expect(component.activeRoute).toBe('/tab1');
    });

    it('should ignore query parameters in route matching', () => {
      component.tabs = mockTabs;
      setRouterUrl('/tab1?param=value');
      fixture.detectChanges();

      expect(component.activeRoute).toBe('/tab1');
    });

    it('should match most specific route first', () => {
      const specificTabs: ResponsiveTabItem[] = [
        { route: '/tab', label: 'General', disabled: false },
        { route: '/tab/specific', label: 'Specific', disabled: false }
      ];
      component.tabs = specificTabs;
      setRouterUrl('/tab/specific');
      fixture.detectChanges();

      expect(component.activeRoute).toBe('/tab/specific');
    });
  });

  describe('language change handling', () => {
    it('should update translations on language change', () => {
      component.tabs = mockTabs;
      fixture.detectChanges();

      mockTranslateService.instant.and.callFake((key: string) => `French ${key}`);
      langChangeSubject.next({ lang: 'fr' });

      expect(component.tabs[0].translatedLabel).toBe('French tab.one');
    });
  });

  describe('getActiveTab', () => {
    it('should return current active tab', () => {
      component.tabs = mockTabs;
      component.activeRoute = '/tab2';

      const activeTab = component.getActiveTab();

      expect(activeTab).toBeTruthy();
      expect(activeTab?.route).toBe('/tab2');
    });

    it('should return null if no active tab', () => {
      component.tabs = mockTabs;
      component.activeRoute = '/nonexistent';

      const activeTab = component.getActiveTab();

      expect(activeTab).toBeNull();
    });
  });

  describe('onTabChange', () => {
    it('should navigate to selected tab', () => {
      component.tabs = mockTabs;
      
      component.onTabChange({ value: mockTabs[1] });

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/tab2']);
    });

    it('should not navigate if tab is disabled', () => {
      component.tabs = mockTabs;
      
      component.onTabChange({ value: mockTabs[2] });

      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should not navigate if tab is null', () => {
      component.onTabChange({ value: null });

      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  describe('getTabClass', () => {
    beforeEach(() => {
      component.tabs = mockTabs;
      component.activeTabClass = 'active-custom';
      component.inactiveTabClass = 'inactive-custom';
    });

    it('should return active class for active tab', () => {
      component.activeRoute = '/tab1';
      
      const tabClass = component.getTabClass(mockTabs[0]);

      expect(tabClass).toContain('active-custom');
    });

    it('should return inactive class for inactive tab', () => {
      component.activeRoute = '/tab1';
      
      const tabClass = component.getTabClass(mockTabs[1]);

      expect(tabClass).toContain('inactive-custom');
    });

    it('should add disabled class for disabled tab', () => {
      const tabClass = component.getTabClass(mockTabs[2]);

      expect(tabClass).toContain('p-tab-disabled');
    });

    it('should always include base classes', () => {
      const tabClass = component.getTabClass(mockTabs[0]);

      expect(tabClass).toContain('flex items-center !gap-2 text-inherit');
    });
  });

  describe('public API methods', () => {
    describe('setActiveTab', () => {
      it('should navigate to specified tab', () => {
        component.tabs = mockTabs;
        
        component.setActiveTab('/tab2');

        expect(mockRouter.navigate).toHaveBeenCalledWith(['/tab2']);
      });

      it('should not navigate if tab does not exist', () => {
        component.tabs = mockTabs;
        
        component.setActiveTab('/nonexistent');

        expect(mockRouter.navigate).not.toHaveBeenCalled();
      });

      it('should not navigate if tab is disabled', () => {
        component.tabs = mockTabs;
        
        component.setActiveTab('/tab3');

        expect(mockRouter.navigate).not.toHaveBeenCalled();
      });
    });

    describe('addTab', () => {
      it('should add new tab to list', () => {
        component.tabs = [...mockTabs];
        const newTab: ResponsiveTabItem = { route: '/tab4', label: 'tab.four', disabled: false };
        
        component.addTab(newTab);

        expect(component.tabs.length).toBe(4);
        expect(component.tabs[3]).toBe(newTab);
      });

      it('should translate new tab label', () => {
        component.tabs = [...mockTabs];
        const newTab: ResponsiveTabItem = { route: '/tab4', label: 'tab.four', disabled: false };
        
        component.addTab(newTab);

        expect(mockTranslateService.instant).toHaveBeenCalledWith('tab.four');
      });
    });

    describe('removeTab', () => {
      it('should remove tab from list', () => {
        component.tabs = [...mockTabs];
        
        component.removeTab('/tab2');

        expect(component.tabs.length).toBe(2);
        expect(component.tabs.find(t => t.route === '/tab2')).toBeUndefined();
      });

      it('should not fail if tab does not exist', () => {
        component.tabs = [...mockTabs];
        
        expect(() => component.removeTab('/nonexistent')).not.toThrow();
        expect(component.tabs.length).toBe(3);
      });
    });

    describe('setTabDisabled', () => {
      it('should enable/disable specified tab', () => {
        component.tabs = [...mockTabs];
        
        component.setTabDisabled('/tab1', true);

        expect(component.tabs[0].disabled).toBeTrue();
        
        component.setTabDisabled('/tab1', false);

        expect(component.tabs[0].disabled).toBeFalse();
      });

      it('should not fail if tab does not exist', () => {
        component.tabs = [...mockTabs];
        
        expect(() => component.setTabDisabled('/nonexistent', true)).not.toThrow();
      });
    });
  });

  describe('ngOnDestroy', () => {
    it('should unsubscribe from router events', () => {
      component.tabs = mockTabs;
      component.ngOnInit();
      
      spyOn(component['routerSubscription']!, 'unsubscribe');
      
      component.ngOnDestroy();

      expect(component['routerSubscription']!.unsubscribe).toHaveBeenCalled();
    });

    it('should not fail if subscription is null', () => {
      component['routerSubscription'] = null;
      
      expect(() => component.ngOnDestroy()).not.toThrow();
    });
  });

  describe('input properties', () => {
    it('should accept custom dropdown placeholder', () => {
      component.dropdownPlaceholder = 'Choose a tab';
      fixture.detectChanges();

      expect(component.dropdownPlaceholder).toBe('Choose a tab');
    });

    it('should accept custom CSS classes', () => {
      component.tabsClass = 'custom-tabs';
      component.tabListClass = 'custom-tablist';
      component.activeTabClass = 'custom-active';
      component.inactiveTabClass = 'custom-inactive';
      fixture.detectChanges();

      expect(component.tabsClass).toBe('custom-tabs');
      expect(component.tabListClass).toBe('custom-tablist');
      expect(component.activeTabClass).toBe('custom-active');
      expect(component.inactiveTabClass).toBe('custom-inactive');
    });

    it('should accept disabled state', () => {
      component.disabled = true;
      fixture.detectChanges();

      expect(component.disabled).toBeTrue();
    });
  });

  describe('edge cases', () => {
    it('should handle empty tabs array', () => {
      component.tabs = [];
      fixture.detectChanges();

      expect(component.getActiveTab()).toBeNull();
      expect(component.activeRoute).toBe('');
    });

    it('should handle tabs without icons', () => {
      const tabsWithoutIcons: ResponsiveTabItem[] = [
        { route: '/tab1', label: 'tab.one', disabled: false }
      ];
      component.tabs = tabsWithoutIcons;
      fixture.detectChanges();

      expect(component.tabs[0].icon).toBeUndefined();
    });

    it('should handle very long route paths', () => {
      const longRoute = '/very/long/route/path/with/many/segments';
      const longTab: ResponsiveTabItem = { route: longRoute, label: 'Long', disabled: false };
      component.tabs = [longTab];
      setRouterUrl(longRoute);
      fixture.detectChanges();

      expect(component.activeRoute).toBe(longRoute);
    });

    it('should handle special characters in routes', () => {
      const specialTab: ResponsiveTabItem = { 
        route: '/tab-1_special%20char', 
        label: 'Special', 
        disabled: false 
      };
      component.tabs = [specialTab];
      setRouterUrl('/tab-1_special%20char');
      fixture.detectChanges();

      expect(component.activeRoute).toBe('/tab-1_special%20char');
    });
  });
});


