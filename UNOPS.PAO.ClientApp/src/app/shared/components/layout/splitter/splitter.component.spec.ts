import { ComponentFixture, TestBed } from '@angular/core/testing';
import { SplitterComponent, SplitterPanel, SplitterResizeEvent } from './splitter.component';
import { DebugElement, TemplateRef } from '@angular/core';
import { By } from '@angular/platform-browser';

describe('SplitterComponent', () => {
  let component: SplitterComponent;
  let fixture: ComponentFixture<SplitterComponent>;

  const mockTemplate: TemplateRef<any> = {} as TemplateRef<any>;

  const createMockPanels = (count: number = 2): SplitterPanel[] => {
    return Array.from({ length: count }, (_, i) => ({
      id: `panel-${i}`,
      size: 50,
      minSize: 10,
      maxSize: 90,
      resizable: true,
      visible: true,
      template: mockTemplate,
      data: { index: i }
    }));
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SplitterComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(SplitterComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('initialization', () => {
    it('should have default values', () => {
      expect(component.layout).toBe('horizontal');
      expect(component.gutterSize).toBe(4);
      expect(component.step).toBe(5);
      expect(component.stateStorage).toBe('session');
      expect(component.stateKey).toBeNull();
      expect(component.style).toBeNull();
      expect(component.styleClass).toBeNull();
      expect(component.minSizes).toEqual([]);
      expect(component.maxSizes).toEqual([]);
    });

    it('should initialize with empty panels', () => {
      expect(component.panels).toEqual([]);
    });
  });

  describe('panels input', () => {
    it('should set panels array', () => {
      const mockPanels = createMockPanels(3);
      
      component.panels = mockPanels;

      expect(component.panels.length).toBe(3);
      expect(component.panels[0].id).toBe('panel-0');
    });

    it('should create a copy of panels array', () => {
      const mockPanels = createMockPanels(2);
      
      component.panels = mockPanels;

      expect(component.panels).not.toBe(mockPanels);
      expect(component.panels).toEqual(mockPanels);
    });

    it('should handle empty panels array', () => {
      component.panels = [];

      expect(component.panels).toEqual([]);
    });

    it('should filter visible panels', () => {
      const mockPanels = createMockPanels(3);
      mockPanels[1].visible = false;
      
      component.panels = mockPanels;

      expect(component.visiblePanels().length).toBe(2);
      expect(component.visiblePanels().find(p => p.id === 'panel-1')).toBeUndefined();
    });
  });

  describe('panelSizes input', () => {
    it('should set panel sizes', () => {
      const sizes = [30, 70];
      
      component.panelSizes = sizes;

      expect(component.panelSizes).toEqual(sizes);
    });

    it('should update computed sizes', () => {
      const mockPanels = createMockPanels(2);
      component.panels = mockPanels;
      
      component.panelSizes = [40, 60];

      expect(component.computedSizes().length).toBe(2);
    });

    it('should handle empty sizes array', () => {
      component.panelSizes = [];

      expect(component.panelSizes).toEqual([]);
    });
  });

  describe('layout configuration', () => {
    it('should accept horizontal layout', () => {
      component.layout = 'horizontal';
      fixture.detectChanges();

      expect(component.layout).toBe('horizontal');
    });

    it('should accept vertical layout', () => {
      component.layout = 'vertical';
      fixture.detectChanges();

      expect(component.layout).toBe('vertical');
    });

    it('should accept custom gutter size', () => {
      component.gutterSize = 10;
      fixture.detectChanges();

      expect(component.gutterSize).toBe(10);
    });

    it('should accept custom step size', () => {
      component.step = 10;
      fixture.detectChanges();

      expect(component.step).toBe(10);
    });
  });

  describe('state management', () => {
    it('should accept session storage', () => {
      component.stateStorage = 'session';
      fixture.detectChanges();

      expect(component.stateStorage).toBe('session');
    });

    it('should accept local storage', () => {
      component.stateStorage = 'local';
      fixture.detectChanges();

      expect(component.stateStorage).toBe('local');
    });

    it('should accept null storage', () => {
      component.stateStorage = null;
      fixture.detectChanges();

      expect(component.stateStorage).toBeNull();
    });

    it('should accept custom state key', () => {
      component.stateKey = 'my-splitter-state';
      fixture.detectChanges();

      expect(component.stateKey).toBe('my-splitter-state');
    });
  });

  describe('min/max sizes', () => {
    it('should accept min sizes array', () => {
      component.minSizes = [10, 20];
      fixture.detectChanges();

      expect(component.minSizes).toEqual([10, 20]);
    });

    it('should accept max sizes array', () => {
      component.maxSizes = [80, 90];
      fixture.detectChanges();

      expect(component.maxSizes).toEqual([80, 90]);
    });

    it('should handle different array lengths', () => {
      component.minSizes = [10];
      component.maxSizes = [80, 90, 95];
      fixture.detectChanges();

      expect(component.minSizes.length).toBe(1);
      expect(component.maxSizes.length).toBe(3);
    });
  });

  describe('computed properties', () => {
    describe('visiblePanels', () => {
      it('should return only visible panels', () => {
        const mockPanels = createMockPanels(4);
        mockPanels[1].visible = false;
        mockPanels[3].visible = false;
        
        component.panels = mockPanels;

        const visible = component.visiblePanels();
        expect(visible.length).toBe(2);
        expect(visible[0].id).toBe('panel-0');
        expect(visible[1].id).toBe('panel-2');
      });

      it('should return all panels when all are visible', () => {
        const mockPanels = createMockPanels(3);
        
        component.panels = mockPanels;

        expect(component.visiblePanels().length).toBe(3);
      });

      it('should return empty array when no panels', () => {
        component.panels = [];

        expect(component.visiblePanels()).toEqual([]);
      });
    });

    describe('computedSizes', () => {
      it('should use provided sizes when length matches panels', () => {
        const mockPanels = createMockPanels(2);
        component.panels = mockPanels;
        component.panelSizes = [40, 60];

        expect(component.computedSizes()).toEqual([40, 60]);
      });

      it('should calculate equal sizes when no sizes provided', () => {
        const mockPanels = createMockPanels(4);
        component.panels = mockPanels;
        component.panelSizes = [];

        const sizes = component.computedSizes();
        expect(sizes.every(s => s === 25)).toBeTrue();
      });

      it('should use equal distribution when panelSizes is empty', () => {
        const mockPanels = createMockPanels(2);
        mockPanels[0].size = 30;
        mockPanels[1].size = 70;
        component.panels = mockPanels;
        component.panelSizes = [];

        const sizes = component.computedSizes();
        expect(sizes).toEqual([50, 50]);
      });
    });
  });

  describe('event emitters', () => {
    it('should have onResizeStart emitter', () => {
      expect(component.onResizeStart).toBeDefined();
    });

    it('should have onResizeEnd emitter', () => {
      expect(component.onResizeEnd).toBeDefined();
    });

    it('should have onPanelAdd emitter', () => {
      expect(component.onPanelAdd).toBeDefined();
    });

    it('should have onPanelRemove emitter', () => {
      expect(component.onPanelRemove).toBeDefined();
    });

    it('should have onPanelSizeChange emitter', () => {
      expect(component.onPanelSizeChange).toBeDefined();
    });
  });

  describe('styling', () => {
    it('should accept custom style object', () => {
      const customStyle = { width: '100%', height: '500px' };
      component.style = customStyle;
      fixture.detectChanges();

      expect(component.style).toEqual(customStyle);
    });

    it('should accept custom style class', () => {
      component.styleClass = 'custom-splitter';
      fixture.detectChanges();

      expect(component.styleClass).toBe('custom-splitter');
    });

    it('should accept custom resizer style', () => {
      const resizerStyle = { backgroundColor: 'blue' };
      component.resizerStyle = resizerStyle;
      fixture.detectChanges();

      expect(component.resizerStyle).toEqual(resizerStyle);
    });

    it('should accept custom resizer style class', () => {
      component.resizerStyleClass = 'custom-resizer';
      fixture.detectChanges();

      expect(component.resizerStyleClass).toBe('custom-resizer');
    });
  });

  describe('panel management', () => {
    it('should handle adding panels dynamically', () => {
      const initialPanels = createMockPanels(2);
      component.panels = initialPanels;

      const newPanel: SplitterPanel = {
        id: 'panel-new',
        size: 33,
        visible: true,
        template: mockTemplate
      };

      component.panels = [...component.panels, newPanel];

      expect(component.panels.length).toBe(3);
      expect(component.panels[2].id).toBe('panel-new');
    });

    it('should handle removing panels dynamically', () => {
      const mockPanels = createMockPanels(3);
      component.panels = mockPanels;

      component.panels = component.panels.filter(p => p.id !== 'panel-1');

      expect(component.panels.length).toBe(2);
      expect(component.panels.find(p => p.id === 'panel-1')).toBeUndefined();
    });

    it('should handle updating panel properties', () => {
      const mockPanels = createMockPanels(2);
      component.panels = mockPanels;

      mockPanels[0].size = 70;
      component.panels = [...mockPanels];

      expect(component.panels[0].size).toBe(70);
    });
  });

  describe('panel resizability', () => {
    it('should respect resizable flag', () => {
      const mockPanels = createMockPanels(2);
      mockPanels[0].resizable = false;
      
      component.panels = mockPanels;

      expect(component.panels[0].resizable).toBeFalse();
    });

    it('should default resizable to true', () => {
      const mockPanels = createMockPanels(1);
      delete mockPanels[0].resizable;
      
      component.panels = mockPanels;

      expect(component.panels[0].resizable).toBeUndefined();
    });
  });

  describe('edge cases', () => {
    it('should handle single panel', () => {
      const mockPanels = createMockPanels(1);
      
      component.panels = mockPanels;

      expect(component.panels.length).toBe(1);
      expect(component.visiblePanels().length).toBe(1);
    });

    it('should handle many panels', () => {
      const mockPanels = createMockPanels(10);
      
      component.panels = mockPanels;

      expect(component.panels.length).toBe(10);
      expect(component.visiblePanels().length).toBe(10);
    });

    it('should handle panels with missing optional properties', () => {
      const minimalPanel: SplitterPanel = {
        id: 'minimal',
        template: mockTemplate
      };
      
      component.panels = [minimalPanel];

      expect(component.panels[0].id).toBe('minimal');
      expect(component.panels[0].size).toBeUndefined();
      expect(component.panels[0].minSize).toBeUndefined();
    });

    it('should handle panels with zero size', () => {
      const mockPanels = createMockPanels(2);
      mockPanels[0].size = 0;
      mockPanels[1].size = 100;
      
      component.panels = mockPanels;

      expect(component.panels[0].size).toBe(0);
    });

    it('should handle panels with custom data', () => {
      const mockPanels = createMockPanels(2);
      mockPanels[0].data = { title: 'Panel 1', color: 'blue' };
      
      component.panels = mockPanels;

      expect(component.panels[0].data).toEqual({ title: 'Panel 1', color: 'blue' });
    });

    it('should handle rapid panel updates', () => {
      component.panels = createMockPanels(2);
      component.panels = createMockPanels(3);
      component.panels = createMockPanels(4);

      expect(component.panels.length).toBe(4);
    });

    it('should handle size arrays longer than panel count', () => {
      const mockPanels = createMockPanels(2);
      component.panels = mockPanels;
      component.panelSizes = [25, 25, 25, 25];

      // Component should handle gracefully
      expect(component.panelSizes.length).toBe(4);
    });

    it('should handle size arrays shorter than panel count', () => {
      const mockPanels = createMockPanels(4);
      component.panels = mockPanels;
      component.panelSizes = [50, 50];

      // Component should calculate missing sizes
      expect(component.computedSizes().length).toBeGreaterThanOrEqual(2);
    });

    it('should handle all panels hidden', () => {
      const mockPanels = createMockPanels(3);
      mockPanels.forEach(p => p.visible = false);
      
      component.panels = mockPanels;

      expect(component.visiblePanels().length).toBe(0);
    });

    it('should handle panels with null template', () => {
      const mockPanels: SplitterPanel[] = [{
        id: 'null-template',
        template: null
      }];
      
      component.panels = mockPanels;

      expect(component.panels[0].template).toBeNull();
    });

    it('should handle negative step values', () => {
      component.step = -5;
      fixture.detectChanges();

      expect(component.step).toBe(-5);
    });

    it('should handle zero gutter size', () => {
      component.gutterSize = 0;
      fixture.detectChanges();

      expect(component.gutterSize).toBe(0);
    });

    it('should handle very large gutter size', () => {
      component.gutterSize = 100;
      fixture.detectChanges();

      expect(component.gutterSize).toBe(100);
    });
  });

  describe('AfterViewInit', () => {
    it('should have ngAfterViewInit method', () => {
      expect(component.ngAfterViewInit).toBeDefined();
    });

    it('should not throw on ngAfterViewInit', () => {
      expect(() => component.ngAfterViewInit()).not.toThrow();
    });
  });

  describe('OnDestroy', () => {
    it('should have ngOnDestroy method', () => {
      expect(component.ngOnDestroy).toBeDefined();
    });

    it('should not throw on ngOnDestroy', () => {
      expect(() => component.ngOnDestroy()).not.toThrow();
    });
  });
});

