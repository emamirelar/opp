import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { take } from 'rxjs/operators';
import { GlobalFilterService } from './global-filter.service';

describe('GlobalFilterService', () => {
  let service: GlobalFilterService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GlobalFilterService]
    });
    service = TestBed.inject(GlobalFilterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('Filter Enabled State', () => {
    it('should have filter enabled by default', () => {
      expect(service.isFilterEnabled()).toBe(true);
    });

    it('should emit filter enabled state through observable', (done) => {
      service.filterEnabled$.pipe(take(1)).subscribe(enabled => {
        expect(enabled).toBe(true);
        done();
      });
    });

    it('should update filter enabled state', () => {
      service.setFilterEnabled(false);
      expect(service.isFilterEnabled()).toBe(false);

      service.setFilterEnabled(true);
      expect(service.isFilterEnabled()).toBe(true);
    });

    it('should emit updated filter enabled state', fakeAsync(() => {
      const emittedValues: boolean[] = [];
      service.filterEnabled$.subscribe(value => emittedValues.push(value));

      tick(); // Initial value
      service.setFilterEnabled(false);
      tick();
      service.setFilterEnabled(true);
      tick();

      expect(emittedValues).toEqual([true, false, true]);
    }));
  });

  describe('Selected Org Unit ID', () => {
    it('should have null org unit ID by default', () => {
      expect(service.getSelectedOrgUnitId()).toBeNull();
    });

    it('should emit null org unit ID through observable by default', (done) => {
      service.selectedOrgUnitId$.pipe(take(1)).subscribe(orgUnitId => {
        expect(orgUnitId).toBeNull();
        done();
      });
    });

    it('should update selected org unit ID', () => {
      service.setSelectedOrgUnitId(123);
      expect(service.getSelectedOrgUnitId()).toBe(123);

      service.setSelectedOrgUnitId(456);
      expect(service.getSelectedOrgUnitId()).toBe(456);

      service.setSelectedOrgUnitId(null);
      expect(service.getSelectedOrgUnitId()).toBeNull();
    });

    it('should emit updated selected org unit ID', fakeAsync(() => {
      const emittedValues: (number | null)[] = [];
      service.selectedOrgUnitId$.subscribe(value => emittedValues.push(value));

      tick(); // Initial value
      service.setSelectedOrgUnitId(123);
      tick();
      service.setSelectedOrgUnitId(456);
      tick();
      service.setSelectedOrgUnitId(null);
      tick();

      expect(emittedValues).toEqual([null, 123, 456, null]);
    }));
  });

  describe('Active Org Unit ID', () => {
    it('should return null when filter is disabled', () => {
      service.setFilterEnabled(false);
      service.setSelectedOrgUnitId(123);
      
      expect(service.getActiveOrgUnitId()).toBeNull();
    });

    it('should return selected org unit ID when filter is enabled', () => {
      service.setFilterEnabled(true);
      service.setSelectedOrgUnitId(123);
      
      expect(service.getActiveOrgUnitId()).toBe(123);
    });

    it('should emit null through activeOrgUnitId$ when filter is disabled', (done) => {
      service.setFilterEnabled(false);
      service.setSelectedOrgUnitId(123);

      service.activeOrgUnitId$.pipe(take(1)).subscribe(activeId => {
        expect(activeId).toBeNull();
        done();
      });
    });

    it('should emit selected org unit ID through activeOrgUnitId$ when filter is enabled', (done) => {
      service.setFilterEnabled(true);
      service.setSelectedOrgUnitId(123);

      service.activeOrgUnitId$.pipe(take(1)).subscribe(activeId => {
        expect(activeId).toBe(123);
        done();
      });
    });

    it('should emit correct values when toggling filter enabled state', fakeAsync(() => {
      const emittedValues: (number | null)[] = [];
      service.activeOrgUnitId$.subscribe(value => emittedValues.push(value));

      tick(); // Initial value (filter enabled, no org unit)
      
      service.setSelectedOrgUnitId(123);
      tick(); // Filter enabled, org unit 123
      
      service.setFilterEnabled(false);
      tick(); // Filter disabled, should emit null
      
      service.setFilterEnabled(true);
      tick(); // Filter enabled again, should emit 123
      
      service.setSelectedOrgUnitId(456);
      tick(); // Filter enabled, org unit 456

      expect(emittedValues).toEqual([null, 123, null, 123, 456]);
    }));

    it('should emit correct values when changing org unit while filter is disabled', fakeAsync(() => {
      const emittedValues: (number | null)[] = [];
      service.activeOrgUnitId$.subscribe(value => emittedValues.push(value));

      tick(); // Initial value
      
      service.setFilterEnabled(false);
      tick(); // Filter disabled
      
      service.setSelectedOrgUnitId(123);
      tick(); // Still null because filter is disabled
      
      service.setSelectedOrgUnitId(456);
      tick(); // Still null
      
      service.setFilterEnabled(true);
      tick(); // Now should emit 456

      expect(emittedValues).toEqual([null, null, null, null, 456]);
    }));
  });

  describe('Edge Cases', () => {
    it('should handle multiple rapid state changes', fakeAsync(() => {
      const emittedValues: (number | null)[] = [];
      service.activeOrgUnitId$.subscribe(value => emittedValues.push(value));
      tick();

      // Rapid changes
      service.setFilterEnabled(true);
      service.setSelectedOrgUnitId(1);
      service.setFilterEnabled(false);
      service.setSelectedOrgUnitId(2);
      service.setFilterEnabled(true);
      service.setSelectedOrgUnitId(3);
      tick();

      // Should only emit the final state
      expect(emittedValues[emittedValues.length - 1]).toBe(3);
    }));

    it('should handle setting the same value multiple times', () => {
      service.setFilterEnabled(true);
      service.setSelectedOrgUnitId(123);

      const initialEnabled = service.isFilterEnabled();
      const initialOrgUnitId = service.getSelectedOrgUnitId();

      // Set same values again
      service.setFilterEnabled(true);
      service.setSelectedOrgUnitId(123);

      expect(service.isFilterEnabled()).toBe(initialEnabled);
      expect(service.getSelectedOrgUnitId()).toBe(initialOrgUnitId);
    });

    it('should handle null and undefined appropriately', () => {
      service.setSelectedOrgUnitId(null);
      expect(service.getSelectedOrgUnitId()).toBeNull();
      expect(service.getActiveOrgUnitId()).toBeNull();

      // TypeScript should prevent undefined, but test runtime behavior
      service.setSelectedOrgUnitId(undefined as any);
      expect(service.getSelectedOrgUnitId()).toBeUndefined();
    });
  });

  describe('Observable Subscriptions', () => {
    it('should support multiple subscribers', fakeAsync(() => {
      const subscriber1Values: boolean[] = [];
      const subscriber2Values: boolean[] = [];

      service.filterEnabled$.subscribe(value => subscriber1Values.push(value));
      service.filterEnabled$.subscribe(value => subscriber2Values.push(value));

      tick();
      service.setFilterEnabled(false);
      tick();

      expect(subscriber1Values).toEqual([true, false]);
      expect(subscriber2Values).toEqual([true, false]);
    }));

    it('should handle late subscribers correctly', fakeAsync(() => {
      service.setFilterEnabled(false);
      service.setSelectedOrgUnitId(123);
      tick();

      // Late subscriber should get current values
      const lateValues: (number | null)[] = [];
      service.activeOrgUnitId$.subscribe(value => lateValues.push(value));
      tick();

      expect(lateValues).toEqual([null]); // Filter is disabled, so null
    }));
  });
});
