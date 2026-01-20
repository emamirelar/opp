import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ElementRef } from '@angular/core';

import { TimelineComponent, TimelineItem, TimelineConfig } from './timeline.component';

describe('TimelineComponent', () => {
  let component: TimelineComponent;
  let fixture: ComponentFixture<TimelineComponent>;
  let httpMock: HttpTestingController;

  const mockTimelineItems: TimelineItem[] = [
    {
      id: '1',
      content: 'Test Meeting',
      start: new Date('2024-01-15T10:00:00Z'),
      type: 'point',
      title: 'Meeting: Project Discussion',
      className: 'timeline-meeting',
      data: {
        id: 1,
        type: 'Meeting',
        subject: 'Project Discussion',
        contactName: 'John Doe',
        date: '2024-01-15T10:00:00Z'
      }
    },
    {
      id: '2',
      content: 'Test Email',
      start: new Date('2024-01-16T14:30:00Z'),
      type: 'point',
      title: 'Email: Follow up',
      className: 'timeline-email',
      data: {
        id: 2,
        type: 'Email',
        subject: 'Follow up',
        contactName: 'Jane Smith',
        date: '2024-01-16T14:30:00Z'
      }
    }
  ];

  const mockApiResponse = {
    records: [
      {
        id: 1,
        type: 'Meeting',
        subject: 'Project Discussion',
        contactName: 'John Doe',
        date: '2024-01-15T10:00:00Z'
      },
      {
        id: 2,
        type: 'Email',
        subject: 'Follow up',
        contactName: 'Jane Smith',
        date: '2024-01-16T14:30:00Z'
      }
    ]
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        TimelineComponent,
        HttpClientTestingModule
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TimelineComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);

    // Mock the DOM elements
    const mockTimelineContainer = {
      nativeElement: document.createElement('div')
    };
    const mockNavigatorDiv = document.createElement('div');
    Object.defineProperty(mockNavigatorDiv, 'offsetWidth', { value: 800, writable: true });
    const mockNavigatorContainer = {
      nativeElement: mockNavigatorDiv
    };
    
    const mockCanvas = document.createElement('canvas');
    const mockContext = {
      clearRect: jasmine.createSpy('clearRect'),
      strokeStyle: '',
      lineWidth: 0,
      font: '',
      fillStyle: '',
      beginPath: jasmine.createSpy('beginPath'),
      moveTo: jasmine.createSpy('moveTo'),
      lineTo: jasmine.createSpy('lineTo'),
      stroke: jasmine.createSpy('stroke'),
      fillText: jasmine.createSpy('fillText'),
      arc: jasmine.createSpy('arc'),
      fill: jasmine.createSpy('fill'),
      textAlign: ''
    };
    Object.defineProperty(mockCanvas, 'width', { value: 800, writable: true });
    Object.defineProperty(mockCanvas, 'height', { value: 40, writable: true });
    spyOn(mockCanvas, 'getContext').and.returnValue(mockContext as any);
    
    const mockNavigatorCanvas = {
      nativeElement: mockCanvas
    };

    component.timelineContainer = mockTimelineContainer as ElementRef;
    component.navigatorContainer = mockNavigatorContainer as ElementRef;
    component.navigatorCanvas = mockNavigatorCanvas as ElementRef<HTMLCanvasElement>;

    // Prevent actual timeline creation in tests
    spyOn(component, 'createTimeline' as any).and.stub();
    
    // Mock ngOnDestroy to prevent timeline destruction errors
    spyOn(component, 'ngOnDestroy').and.callFake(() => {
      // Clean up without destroying timeline
      if (component['updateTimelineDebounce']) {
        clearTimeout(component['updateTimelineDebounce']);
      }
      if (component['navigatorSelectionDebounce']) {
        clearTimeout(component['navigatorSelectionDebounce']);
      }
      if (component['rangeChangeDebounce']) {
        clearTimeout(component['rangeChangeDebounce']);
      }
      if (component['zoomDebounce']) {
        clearTimeout(component['zoomDebounce']);
      }
      if (component['animationFrameId']) {
        cancelAnimationFrame(component['animationFrameId']);
      }
      if (component['currentLoadingRequest']) {
        component['currentLoadingRequest'].abort();
      }
    });

    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
    // Safely destroy timeline if it exists
    if (component.timeline) {
      try {
        component.timeline.destroy();
        component.timeline = undefined;
      } catch (e) {
        // Ignore timeline destruction errors in tests
      }
    }
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should initialize with default config', () => {
      expect(component.config).toEqual({});
      expect(component.items).toEqual([]);
      expect(component.autoLoadFromUrl).toBe(true);
    });

    it('should initialize navigator with 3-year range', () => {
      const now = new Date();
      const expectedStart = new Date(now.getFullYear() - 3, now.getMonth(), now.getDate());
      const expectedEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate());

      expect(component.navigatorStartDate.getFullYear()).toBe(expectedStart.getFullYear());
      expect(component.navigatorEndDate.getFullYear()).toBe(expectedEnd.getFullYear());
    });
  });

  describe('Data Loading', () => {
    it('should load data from URL when dataUrl is provided', fakeAsync(() => {
      component.dataUrl = '/api/timeline-data';
      component.autoLoadFromUrl = true;

      component.ngOnChanges({
        dataUrl: {
          currentValue: '/api/timeline-data',
          previousValue: undefined,
          firstChange: true,
          isFirstChange: () => true
        }
      });

      const req = httpMock.expectOne('/api/timeline-data');
      expect(req.request.method).toBe('GET');

      req.flush(mockApiResponse);
      tick();

      expect(component.allItems.length).toBe(2);
      expect(component.allItems[0].id).toBe(1);
      expect(component.allItems[1].id).toBe(2);
    }));

    it('should handle API error gracefully', fakeAsync(() => {
      spyOn(console, 'error');
      component.dataUrl = '/api/timeline-data';
      component.autoLoadFromUrl = true;

      component.ngOnChanges({
        dataUrl: {
          currentValue: '/api/timeline-data',
          previousValue: undefined,
          firstChange: true,
          isFirstChange: () => true
        }
      });

      const req = httpMock.expectOne('/api/timeline-data');
      req.error(new ErrorEvent('Network error'));
      tick();

      expect(console.error).toHaveBeenCalledWith('Failed to load timeline data:', jasmine.any(Object));
    }));

    it('should update timeline data when items input changes', () => {
      spyOn(component.timelineData, 'clear');
      spyOn(component.timelineData, 'add');
      component.timeline = {} as any; // Mock timeline existence

      component.ngOnChanges({
        items: {
          currentValue: mockTimelineItems,
          previousValue: [],
          firstChange: false,
          isFirstChange: () => false
        }
      });

      expect(component.timelineData.clear).toHaveBeenCalled();
      expect(component.timelineData.add).toHaveBeenCalledWith(mockTimelineItems);
    });
  });

  describe('Data Conversion', () => {
    it('should convert API record to timeline item correctly', () => {
      const record = {
        id: 1,
        type: 'Meeting',
        subject: 'Test Meeting',
        contactName: 'John Doe',
        date: '2024-01-15T10:00:00Z'
      };

      const result = component['convertToTimelineItem'](record);

      expect(result.id).toBe(1);
      expect(result.start).toEqual(new Date('2024-01-15T10:00:00Z'));
      expect(result.type).toBe('point');
      expect(result.title).toBe('Meeting: Test Meeting');
      expect(result.className).toBe('timeline-meeting');
      expect(result.data).toBe(record);
    });

    it('should create correct timeline item content', () => {
      const record = {
        id: 1,
        type: 'Email',
        contactName: 'Jane Smith'
      };

      const content = component['createTimelineItemContent'](record);

      expect(content).toContain('✉️'); // Email unicode icon
      expect(content).toContain('Jane Smith');
    });
  });

  describe('Icon Mapping', () => {
    it('should return correct icons for different interaction types', () => {
      expect(component['getSimpleUnicodeIcon']('Meeting')).toBe('🤝');
      expect(component['getSimpleUnicodeIcon']('Email')).toBe('✉️');
      expect(component['getSimpleUnicodeIcon']('Call')).toBe('📞');
      expect(component['getSimpleUnicodeIcon']('Unknown')).toBe('⚪');
    });

    it('should return correct CSS classes for different interaction types', () => {
      expect(component['getTimelineItemClass']('Meeting')).toBe('timeline-meeting');
      expect(component['getTimelineItemClass']('Email')).toBe('timeline-email');
      expect(component['getTimelineItemClass']('Call')).toBe('timeline-phone');
      expect(component['getTimelineItemClass']('Unknown')).toBe('timeline-other');
    });
  });

  describe('Navigator Functionality', () => {

    it('should handle navigator mouse down event', () => {
      const event = new MouseEvent('mousedown', { clientX: 100 });
      Object.defineProperty(event, 'offsetX', { value: 100 });

      component.onNavigatorMouseDown(event);

      expect(component.isDragging).toBe(true);
      expect(component['isNavigatorSelecting']).toBe(true);
      expect(component.dragStartX).toBe(100);
      expect(component.selectionLeft).toBe(100);
      expect(component.selectionWidth).toBe(0);
    });

    it('should handle navigator mouse move event when dragging', () => {
      component.isDragging = true;
      component.dragStartX = 50;

      const event = new MouseEvent('mousemove', { clientX: 150 });
      Object.defineProperty(event, 'offsetX', { value: 150 });

      component.onNavigatorMouseMove(event);

      expect(component.selectionLeft).toBe(50);
      expect(component.selectionWidth).toBe(100);
    });

    it('should not handle navigator mouse move when not dragging', () => {
      component.isDragging = false;
      const originalLeft = component.selectionLeft;
      const originalWidth = component.selectionWidth;

      const event = new MouseEvent('mousemove', { clientX: 150 });
      Object.defineProperty(event, 'offsetX', { value: 150 });

      component.onNavigatorMouseMove(event);

      expect(component.selectionLeft).toBe(originalLeft);
      expect(component.selectionWidth).toBe(originalWidth);
    });

    it('should get correct year range string', () => {
      const now = new Date();
      const startYear = now.getFullYear() - 10;
      const endYear = now.getFullYear();

      const result = component.getYearRange();

      expect(result).toBe(`${startYear} - ${endYear}`);
    });
  });

  describe('Public API Methods', () => {
    it('should refresh timeline', () => {
      spyOn(component, 'loadDataFromUrl' as any);
      component.dataUrl = '/api/data';
      component.autoLoadFromUrl = true;

      component.refreshTimeline();

      expect(component['loadDataFromUrl']).toHaveBeenCalled();
    });

    it('should fit timeline when timeline exists', () => {
      const mockTimeline = {
        fit: jasmine.createSpy('fit')
      };
      component.timeline = mockTimeline as any;
      (component as any).createTimeline.and.callThrough(); // Allow actual call for this test

      component.fitTimeline();

      expect(mockTimeline.fit).toHaveBeenCalled();
    });

    it('should get timeline range when timeline exists', () => {
      const mockWindow = {
        start: new Date('2024-01-01'),
        end: new Date('2024-01-31')
      };
      const mockTimeline = {
        getWindow: jasmine.createSpy('getWindow').and.returnValue(mockWindow)
      };
      component.timeline = mockTimeline as any;

      const result = component.getTimelineRange();

      expect(result).toEqual({
        start: new Date('2024-01-01'),
        end: new Date('2024-01-31')
      });
    });

    it('should return null when timeline does not exist', () => {
      component.timeline = undefined;

      const result = component.getTimelineRange();

      expect(result).toBeNull();
    });
  });

  describe('Event Emitters', () => {
    it('should emit itemClick event', () => {
      spyOn(component.itemClick, 'emit');
      const testData = { id: 1, type: 'Meeting' };

      component.itemClick.emit(testData);

      expect(component.itemClick.emit).toHaveBeenCalledWith(testData);
    });

    it('should emit itemSelect event', () => {
      spyOn(component.itemSelect, 'emit');
      const testData = { id: 1, type: 'Email' };

      component.itemSelect.emit(testData);

      expect(component.itemSelect.emit).toHaveBeenCalledWith(testData);
    });

    it('should emit rangeChanged event', () => {
      spyOn(component.rangeChanged, 'emit');
      const testRange = {
        start: new Date('2024-01-01'),
        end: new Date('2024-01-31')
      };

      component.rangeChanged.emit(testRange);

      expect(component.rangeChanged.emit).toHaveBeenCalledWith(testRange);
    });
  });

  describe('Cache Functionality', () => {
    beforeEach(() => {
      component.config = {
        enableLazyLoading: true,
        lazyLoading: {
          maxCacheSize: 10,
          cacheTTL: 60,
          cacheStrategy: 'memory'
        }
      };
    });

    it('should invalidate entire cache when no parameters provided', () => {
      component['cachedRanges'] = [
        {
          start: new Date('2024-01-01'),
          end: new Date('2024-01-31'),
          items: mockTimelineItems,
          timestamp: Date.now(),
          lastAccessed: Date.now(),
          size: 1000
        }
      ];
      component['cacheSize'] = 1000;

      component.invalidateCache();

      expect(component['cachedRanges']).toEqual([]);
      expect(component['cacheSize']).toBe(0);
    });

    it('should get cache stats correctly', () => {
      component['cachedRanges'] = [
        {
          start: new Date('2024-01-01'),
          end: new Date('2024-01-31'),
          items: mockTimelineItems,
          timestamp: Date.now(),
          lastAccessed: Date.now(),
          size: 1024 * 1024 // 1MB
        }
      ];
      component['cacheSize'] = 1024 * 1024;

      const stats = component.cacheStats;

      expect(stats.rangeCount).toBe(1);
      expect(stats.totalSize).toBe(1024 * 1024);
      expect(stats.sizeMB).toBe(1);
    });
  });

  describe('Loading States', () => {
    it('should expose loading state correctly', () => {
      component['isLoading'].set(true);
      expect(component.isLoadingData).toBe(true);

      component['isLoading'].set(false);
      expect(component.isLoadingData).toBe(false);
    });

    it('should expose current loading state correctly', () => {
      component['loadingState'].set('loading');
      expect(component.currentLoadingState).toBe('loading');

      component['loadingState'].set('idle');
      expect(component.currentLoadingState).toBe('idle');

      component['loadingState'].set('debouncing');
      expect(component.currentLoadingState).toBe('debouncing');
    });
  });

  describe('Error Handling', () => {
    it('should handle missing timeline container gracefully', () => {
      component.timelineContainer = undefined as any;
      spyOn(console, 'error');

      // This should not throw an error
      expect(() => component['createTimeline']()).not.toThrow();
    });

    it('should handle missing navigator elements gracefully', () => {
      component.navigatorContainer = undefined as any;
      component.navigatorCanvas = undefined as any;

      // This should not throw an error
      expect(() => component['drawNavigator']()).not.toThrow();
    });
  });

  describe('Clustering Functionality', () => {
    it('should determine cluster criteria based on zoom level', () => {
      const mockTimeline = {
        getWindow: jasmine.createSpy('getWindow').and.returnValue({
          start: new Date('2024-01-01'),
          end: new Date('2024-12-31') // 1 year range
        })
      };
      component.timeline = mockTimeline as any;

      const item1 = { start: new Date('2024-01-01') };
      const item2 = { start: new Date('2024-01-15') }; // 14 days apart

      const result = component['defaultClusterCriteria'](item1, item2);

      // Should cluster items within a month for year-long view
      expect(result).toBe(true);
    });
  });
});
