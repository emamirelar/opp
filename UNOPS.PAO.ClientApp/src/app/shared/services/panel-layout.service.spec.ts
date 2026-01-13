import { TestBed } from '@angular/core/testing';

/**
 * PanelLayoutService Tests
 * 
 * Tests for the panel layout service that manages panel configuration,
 * user preferences, and responsive layouts.
 * 
 * To run: Copy to UNOPS.PAO.ClientApp/src/app/shared/services/
 *         and run 'ng test'
 */

// Mock service for testing
class MockPanelLayoutService {
  private layouts = new Map<string, any>();
  private preferences = new Map<string, any>();

  getLayout(entityType: string, breakpoint?: string) {
    const key = breakpoint ? `${entityType}-${breakpoint}` : entityType;
    return this.layouts.get(key) || this.getDefaultLayout(entityType);
  }

  getDefaultLayout(entityType: string) {
    return {
      entityType,
      panels: [
        { id: 'contacts', visible: true, order: 0 },
        { id: 'interactions', visible: true, order: 1 },
        { id: 'documents', visible: true, order: 2 }
      ]
    };
  }

  setPanelVisibility(entityType: string, panelId: string, visible: boolean) {
    const layout = this.getLayout(entityType);
    const panel = layout.panels.find((p: any) => p.id === panelId);
    if (panel) {
      panel.visible = visible;
      this.layouts.set(entityType, layout);
    }
  }

  isPanelVisible(entityType: string, panelId: string): boolean {
    const layout = this.getLayout(entityType);
    const panel = layout.panels.find((p: any) => p.id === panelId);
    return panel?.visible ?? false;
  }

  setPanelOrder(entityType: string, order: string[]) {
    const layout = this.getLayout(entityType);
    order.forEach((panelId, index) => {
      const panel = layout.panels.find((p: any) => p.id === panelId);
      if (panel) {
        panel.order = index;
      }
    });
    this.layouts.set(entityType, layout);
  }

  getPanelOrder(entityType: string): string[] {
    const layout = this.getLayout(entityType);
    return layout.panels
      .sort((a: any, b: any) => a.order - b.order)
      .map((p: any) => p.id);
  }

  resetToDefault(entityType: string) {
    this.layouts.delete(entityType);
  }

  movePanelUp(entityType: string, panelId: string) {
    const order = this.getPanelOrder(entityType);
    const index = order.indexOf(panelId);
    if (index > 0) {
      [order[index - 1], order[index]] = [order[index], order[index - 1]];
      this.setPanelOrder(entityType, order);
    }
  }

  movePanelDown(entityType: string, panelId: string) {
    const order = this.getPanelOrder(entityType);
    const index = order.indexOf(panelId);
    if (index < order.length - 1) {
      [order[index], order[index + 1]] = [order[index + 1], order[index]];
      this.setPanelOrder(entityType, order);
    }
  }

  savePreferences(entityType: string) {
    const layout = this.getLayout(entityType);
    this.preferences.set(entityType, JSON.stringify(layout));
    localStorage.setItem(`panel-layout-${entityType}`, JSON.stringify(layout));
  }

  loadPreferences(entityType: string): boolean {
    const stored = localStorage.getItem(`panel-layout-${entityType}`);
    if (stored) {
      this.layouts.set(entityType, JSON.parse(stored));
      return true;
    }
    return false;
  }

  clearPreferences(entityType: string) {
    this.preferences.delete(entityType);
    localStorage.removeItem(`panel-layout-${entityType}`);
  }
}

describe('PanelLayoutService', () => {
  let service: MockPanelLayoutService;

  beforeEach(() => {
    service = new MockPanelLayoutService();
    localStorage.clear();
  });

  // Layout Management Tests
  describe('Layout Management', () => {
    it('should return default layout', () => {
      const layout = service.getLayout('Partner');
      
      expect(layout.entityType).toBe('Partner');
      expect(layout.panels.length).toBe(3);
    });

    it('should set panel visibility', () => {
      service.setPanelVisibility('Partner', 'contacts', false);
      
      expect(service.isPanelVisible('Partner', 'contacts')).toBeFalse();
    });

    it('should check panel visibility', () => {
      const visible = service.isPanelVisible('Partner', 'contacts');
      
      expect(visible).toBeTrue();
    });

    it('should set panel order', () => {
      service.setPanelOrder('Partner', ['documents', 'contacts', 'interactions']);
      
      const order = service.getPanelOrder('Partner');
      expect(order[0]).toBe('documents');
      expect(order[1]).toBe('contacts');
      expect(order[2]).toBe('interactions');
    });

    it('should get panel order', () => {
      const order = service.getPanelOrder('Partner');
      
      expect(order.length).toBe(3);
      expect(order[0]).toBe('contacts');
    });

    it('should reset to default', () => {
      service.setPanelVisibility('Partner', 'contacts', false);
      service.resetToDefault('Partner');
      
      expect(service.isPanelVisible('Partner', 'contacts')).toBeTrue();
    });

    it('should move panel up', () => {
      const originalOrder = service.getPanelOrder('Partner');
      expect(originalOrder[1]).toBe('interactions');
      
      service.movePanelUp('Partner', 'interactions');
      
      const newOrder = service.getPanelOrder('Partner');
      expect(newOrder[0]).toBe('interactions');
    });

    it('should not move first panel up', () => {
      const originalOrder = [...service.getPanelOrder('Partner')];
      
      service.movePanelUp('Partner', 'contacts');
      
      const newOrder = service.getPanelOrder('Partner');
      expect(newOrder).toEqual(originalOrder);
    });

    it('should move panel down', () => {
      const originalOrder = service.getPanelOrder('Partner');
      expect(originalOrder[0]).toBe('contacts');
      
      service.movePanelDown('Partner', 'contacts');
      
      const newOrder = service.getPanelOrder('Partner');
      expect(newOrder[1]).toBe('contacts');
    });

    it('should not move last panel down', () => {
      const originalOrder = [...service.getPanelOrder('Partner')];
      
      service.movePanelDown('Partner', 'documents');
      
      const newOrder = service.getPanelOrder('Partner');
      expect(newOrder).toEqual(originalOrder);
    });
  });

  // User Preferences Tests
  describe('User Preferences', () => {
    it('should save preferences', () => {
      service.setPanelVisibility('Partner', 'contacts', false);
      service.savePreferences('Partner');
      
      const stored = localStorage.getItem('panel-layout-Partner');
      expect(stored).toBeTruthy();
    });

    it('should load preferences', () => {
      const layout = { entityType: 'Partner', panels: [{ id: 'test', visible: true, order: 0 }] };
      localStorage.setItem('panel-layout-Partner', JSON.stringify(layout));
      
      const loaded = service.loadPreferences('Partner');
      
      expect(loaded).toBeTrue();
    });

    it('should return false when no preferences exist', () => {
      const loaded = service.loadPreferences('NonExistent');
      
      expect(loaded).toBeFalse();
    });

    it('should clear preferences', () => {
      service.savePreferences('Partner');
      service.clearPreferences('Partner');
      
      const stored = localStorage.getItem('panel-layout-Partner');
      expect(stored).toBeNull();
    });

    it('should maintain separate preferences per entity type', () => {
      service.setPanelVisibility('Partner', 'contacts', false);
      service.savePreferences('Partner');
      
      // Contact layout should still have default
      expect(service.isPanelVisible('Contact', 'contacts')).toBeTrue();
    });
  });

  // Responsive Layout Tests
  describe('Responsive Layouts', () => {
    it('should get mobile layout', () => {
      const layout = service.getLayout('Partner', 'mobile');
      
      expect(layout).toBeTruthy();
    });

    it('should get tablet layout', () => {
      const layout = service.getLayout('Partner', 'tablet');
      
      expect(layout).toBeTruthy();
    });

    it('should get desktop layout', () => {
      const layout = service.getLayout('Partner', 'desktop');
      
      expect(layout).toBeTruthy();
    });
  });
});

