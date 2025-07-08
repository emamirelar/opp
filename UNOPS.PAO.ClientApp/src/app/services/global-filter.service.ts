import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class GlobalFilterService {
  private filterEnabledSubject = new BehaviorSubject<boolean>(true);
  private selectedOrgUnitIdSubject = new BehaviorSubject<number | null>(null);

  filterEnabled$ = this.filterEnabledSubject.asObservable();
  selectedOrgUnitId$ = this.selectedOrgUnitIdSubject.asObservable();

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
  }

  setSelectedOrgUnitId(orgUnitId: number | null): void {
    this.selectedOrgUnitIdSubject.next(orgUnitId);
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
}