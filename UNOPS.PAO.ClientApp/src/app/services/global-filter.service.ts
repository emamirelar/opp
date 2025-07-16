import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class GlobalFilterService {
  private readonly STORAGE_KEYS = {
    FILTER_ENABLED: 'globalFilter_enabled',
    SELECTED_ORG_UNIT: 'globalFilter_selectedOrgUnitId'
  };

  private filterEnabledSubject = new BehaviorSubject<boolean>(this.loadFilterEnabled());
  private selectedOrgUnitIdSubject = new BehaviorSubject<number | null>(this.loadSelectedOrgUnitId());
  private filtersChangedSubject = new BehaviorSubject<void>(undefined);

  filterEnabled$ = this.filterEnabledSubject.asObservable();
  selectedOrgUnitId$ = this.selectedOrgUnitIdSubject.asObservable();
  filtersChanged$ = this.filtersChangedSubject.asObservable();

  // Combined observable that emits the org unit ID only when filter is enabled
  activeOrgUnitId$: Observable<number | null> = combineLatest([
    this.filterEnabled$,
    this.selectedOrgUnitId$
  ]).pipe(
    map(([enabled, orgUnitId]) => enabled ? orgUnitId : null)
  );

  constructor() {}

  setFilterEnabled(enabled: boolean): void {
    this.filterEnabledSubject.next(enabled);
    this.saveFilterEnabled(enabled);
    this.filtersChangedSubject.next();
  }

  setSelectedOrgUnitId(orgUnitId: number | null): void {
    this.selectedOrgUnitIdSubject.next(orgUnitId);
    this.saveSelectedOrgUnitId(orgUnitId);
    this.filtersChangedSubject.next();
  }

  // Method to trigger a refresh when global filters are saved
  triggerFiltersChanged(): void {
    this.filtersChangedSubject.next();
  }

  isFilterEnabled(): boolean {
    return this.filterEnabledSubject.value;
  }

  getSelectedOrgUnitId(): number | null {
    return this.selectedOrgUnitIdSubject.value;
  }

  getActiveOrgUnitId(): number | null {
    return this.isFilterEnabled() ? this.getSelectedOrgUnitId() : null;
  }

  private loadFilterEnabled(): boolean {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEYS.FILTER_ENABLED);
      return stored !== null ? JSON.parse(stored) : true; // Default to true
    } catch {
      return true; // Default to true if parsing fails
    }
  }

  private saveFilterEnabled(enabled: boolean): void {
    try {
      localStorage.setItem(this.STORAGE_KEYS.FILTER_ENABLED, JSON.stringify(enabled));
    } catch (error) {
      console.warn('Failed to save filter enabled state to localStorage:', error);
    }
  }

  private loadSelectedOrgUnitId(): number | null {
    try {
      const stored = localStorage.getItem(this.STORAGE_KEYS.SELECTED_ORG_UNIT);
      return stored !== null ? JSON.parse(stored) : null; // Default to null
    } catch {
      return null; // Default to null if parsing fails
    }
  }

  private saveSelectedOrgUnitId(orgUnitId: number | null): void {
    try {
      if (orgUnitId === null) {
        localStorage.removeItem(this.STORAGE_KEYS.SELECTED_ORG_UNIT);
      } else {
        localStorage.setItem(this.STORAGE_KEYS.SELECTED_ORG_UNIT, JSON.stringify(orgUnitId));
      }
    } catch (error) {
      console.warn('Failed to save selected org unit ID to localStorage:', error);
    }
  }
}