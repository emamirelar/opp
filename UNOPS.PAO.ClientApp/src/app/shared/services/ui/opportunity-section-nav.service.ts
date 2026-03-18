import { computed, Injectable, signal } from '@angular/core';
import { Subject } from 'rxjs';

export interface SectionDefinition {
  id: string;
  label: string;
  icon: string;
}

@Injectable({
  providedIn: 'root'
})
export class OpportunitySectionNavService {
  readonly sections = signal<SectionDefinition[]>([]);
  readonly activeSection = signal<string>('');
  readonly isViewingOpportunity = computed(() => this.sections().length > 0);

  private scrollRequest$ = new Subject<string>();
  readonly onScrollRequest = this.scrollRequest$.asObservable();

  registerSections(sections: SectionDefinition[]): void {
    this.sections.set(sections);
  }

  unregisterSections(): void {
    this.sections.set([]);
    this.activeSection.set('');
  }

  setActiveSection(id: string): void {
    this.activeSection.set(id);
  }

  requestScrollToSection(id: string): void {
    this.scrollRequest$.next(id);
  }
}
