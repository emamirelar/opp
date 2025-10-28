import { Injectable, signal, computed } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GlobalFilterService {
  private readonly STORAGE_KEYS = {
    FILTER_ENABLED: 'globalFilter_enabled',
    SELECTED_ORG_UNIT: 'globalFilter_selectedOrgUnitId'
  };

  private filterEnabledSignal = signal<boolean>(this.loadFilterEnabled());
  private selectedOrgUnitIdSignal = signal<number | null>(this.loadSelectedOrgUnitId());
  private filtersChangedSignal = signal<number>(0);

  filterEnabled = this.filterEnabledSignal.asReadonly();
  selectedOrgUnitId = this.selectedOrgUnitIdSignal.asReadonly();
  filtersChanged = this.filtersChangedSignal.asReadonly();

  filterEnabled$ = toObservable(this.filterEnabled);
  selectedOrgUnitId$ = toObservable(this.selectedOrgUnitId);
  filtersChanged$ = toObservable(this.filtersChanged);

  activeOrgUnitId = computed(() =>
    this.filterEnabled() ? this.selectedOrgUnitId() : null
  );

  activeOrgUnitId$: Observable<number | null> = toObservable(this.activeOrgUnitId);

  constructor() {}

  setFilterEnabled(enabled: boolean): void {
    this.filterEnabledSignal.set(enabled);
    this.saveFilterEnabled(enabled);
    this.triggerFiltersChanged();
  }

  setSelectedOrgUnitId(orgUnitId: number | null): void {
    this.selectedOrgUnitIdSignal.set(orgUnitId);
    this.saveSelectedOrgUnitId(orgUnitId);
    this.triggerFiltersChanged();
  }

  triggerFiltersChanged(): void {
    this.filtersChangedSignal.set(this.filtersChangedSignal() + 1);
  }

  // Method to clear all filters (used during reset)
  clearAllFilters(): void {
    this.setFilterEnabled(false);
    this.setSelectedOrgUnitId(null);
    this.triggerFiltersChanged();
  }

  isFilterEnabled(): boolean {
    return this.filterEnabled();
  }

  getSelectedOrgUnitId(): number | null {
    return this.selectedOrgUnitId();
  }

  getActiveOrgUnitId(): number | null {
    return this.activeOrgUnitId();
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
