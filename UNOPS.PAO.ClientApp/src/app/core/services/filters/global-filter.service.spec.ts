import { TestBed } from '@angular/core/testing';
import { GlobalFilterService } from './global-filter.service';

describe('GlobalFilterService', () => {
  let service: GlobalFilterService;

  beforeEach(() => {
    localStorage.clear();
    
    TestBed.configureTestingModule({
      providers: [GlobalFilterService]
    });

    service = TestBed.inject(GlobalFilterService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialize with default filter enabled state (true)', () => {
    expect(service.filterEnabled()).toBe(true);
  });

  it('should initialize with null selected org unit', () => {
    expect(service.selectedOrgUnitId()).toBeNull();
  });

  it('should set filter enabled state', () => {
    service.setFilterEnabled(false);
    expect(service.filterEnabled()).toBe(false);
    expect(service.isFilterEnabled()).toBe(false);
  });

  it('should set selected org unit ID', () => {
    service.setSelectedOrgUnitId(123);
    expect(service.selectedOrgUnitId()).toBe(123);
    expect(service.getSelectedOrgUnitId()).toBe(123);
  });

  it('should persist filter enabled state to localStorage', () => {
    service.setFilterEnabled(false);
    
    const stored = localStorage.getItem('globalFilter_enabled');
    expect(stored).toBe('false');
  });

  it('should persist selected org unit to localStorage', () => {
    service.setSelectedOrgUnitId(456);
    
    const stored = localStorage.getItem('globalFilter_selectedOrgUnitId');
    expect(stored).toBe('456');
  });

  it('should load filter enabled state from localStorage', () => {
    localStorage.setItem('globalFilter_enabled', 'false');
    
    TestBed.runInInjectionContext(() => {
      const newService = new GlobalFilterService();
      expect(newService.filterEnabled()).toBe(false);
    });
  });

  it('should load selected org unit from localStorage', () => {
    localStorage.setItem('globalFilter_selectedOrgUnitId', '789');
    
    TestBed.runInInjectionContext(() => {
      const newService = new GlobalFilterService();
      expect(newService.selectedOrgUnitId()).toBe(789);
    });
  });

  it('should compute active org unit ID when filter is enabled', () => {
    service.setFilterEnabled(true);
    service.setSelectedOrgUnitId(100);
    
    expect(service.activeOrgUnitId()).toBe(100);
    expect(service.getActiveOrgUnitId()).toBe(100);
  });

  it('should return null active org unit when filter is disabled', () => {
    service.setFilterEnabled(false);
    service.setSelectedOrgUnitId(100);
    
    expect(service.activeOrgUnitId()).toBeNull();
    expect(service.getActiveOrgUnitId()).toBeNull();
  });

  it('should trigger filtersChanged signal when setting enabled state', (done) => {
    const initialValue = service.filtersChanged();
    
    service.filtersChanged$.subscribe(value => {
      if (value > initialValue) {
        expect(value).toBe(initialValue + 1);
        done();
      }
    });

    service.setFilterEnabled(false);
  });

  it('should trigger filtersChanged signal when setting org unit', (done) => {
    const initialValue = service.filtersChanged();
    
    service.filtersChanged$.subscribe(value => {
      if (value > initialValue) {
        expect(value).toBe(initialValue + 1);
        done();
      }
    });

    service.setSelectedOrgUnitId(200);
  });

  it('should clear all filters', () => {
    service.setFilterEnabled(true);
    service.setSelectedOrgUnitId(300);
    
    service.clearAllFilters();
    
    expect(service.filterEnabled()).toBe(false);
    expect(service.selectedOrgUnitId()).toBeNull();
  });

  it('should remove org unit from localStorage when set to null', () => {
    service.setSelectedOrgUnitId(400);
    expect(localStorage.getItem('globalFilter_selectedOrgUnitId')).toBeTruthy();
    
    service.setSelectedOrgUnitId(null);
    expect(localStorage.getItem('globalFilter_selectedOrgUnitId')).toBeNull();
  });

  it('should handle invalid localStorage data gracefully', () => {
    localStorage.setItem('globalFilter_enabled', 'invalid-json');
    localStorage.setItem('globalFilter_selectedOrgUnitId', 'not-a-number');
    
    TestBed.runInInjectionContext(() => {
      const newService = new GlobalFilterService();
      expect(newService.filterEnabled()).toBe(true); // Default fallback
      expect(newService.selectedOrgUnitId()).toBeNull(); // Default fallback
    });
  });

  it('should expose observables for reactive updates', (done) => {
    let emissions: boolean[] = [];
    
    service.filterEnabled$.subscribe(enabled => {
      emissions.push(enabled);
      // Wait for 2 emissions: initial true, then false
      if (emissions.length === 2) {
        expect(emissions[0]).toBe(true); // Initial value
        expect(emissions[1]).toBe(false); // After setFilterEnabled
        done();
      }
    });

    // Give the initial emission time to happen
    setTimeout(() => {
      service.setFilterEnabled(false);
    }, 0);
  });
});

