import { Component, Input, Output, EventEmitter, TemplateRef, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DashboardCardFilter, DashboardCardConfig, DashboardCardSize } from './dashboard-card.models';

/**
 * Common dashboard card component for consistent layout across all dashboard panels
 * Handles responsive design, zoom levels, and consistent styling
 */
@Component({
  selector: 'app-dashboard-card',
  standalone: true,
  imports: [CommonModule, ButtonModule, TooltipModule, TranslateModule],
  templateUrl: './dashboard-card.component.html',
  styleUrls: ['./dashboard-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardCardComponent {
  private translateService = inject(TranslateService);
  
  @Input() config!: DashboardCardConfig;
  @Input() filters?: DashboardCardFilter[];
  @Input() isExpanded: boolean = false;
  @Input() showViewAllButton: boolean = false;
  @Input() hasContent: boolean = true;
  @Input() contentTemplate?: TemplateRef<any>;
  @Input() remainingCount?: number;
  @Input() isMobile: boolean = false;

  @Output() filterClick = new EventEmitter<DashboardCardFilter>();
  @Output() clearFilter = new EventEmitter<void>();
  @Output() viewAllClick = new EventEmitter<void>();
  @Output() collapseClick = new EventEmitter<void>();
  @Output() emptyStateAction = new EventEmitter<void>();

  get cardSize(): DashboardCardSize {
    return this.config.size || 'auto';
  }

  get cardHeight(): string {
    if (this.isExpanded) {
      return 'auto';
    }
    
    // If a custom height is specified, use it
    if (this.config.height) {
      return this.config.height;
    }
    
    // Otherwise, let CSS classes handle the sizing
    return 'auto';
  }

  getIconTextColor(): string {
    // Extract text color class from icon color background class
    const colorMap: { [key: string]: string } = {
      'bg-unops-warning/10': 'text-unops-warning',
      'bg-unops-info/10': 'text-unops-info',
      'bg-unops-primary/10': 'text-unops-primary',
      'bg-unops-accent-orange/10': 'text-unops-accent-orange',
      'bg-unops-success/10': 'text-unops-success',
      'bg-unops-error/10': 'text-unops-error'
    };
    
    return colorMap[this.config.iconColor] || 'text-unops-primary';
  }

  hasActiveFilter(): boolean {
    return this.filters?.some(f => f.active) || false;
  }

  getViewAllText(): string {
    const baseText = this.config.viewAllText || this.translateService.instant('dashboard.card.viewAll');
    if (this.remainingCount && this.remainingCount > 0) {
      return `${baseText} (${this.remainingCount} ${this.translateService.instant('dashboard.card.more')})`;
    }
    return baseText;
  }

  onFilterClick(filter: DashboardCardFilter): void {
    this.filterClick.emit(filter);
  }

  onClearFilter(): void {
    this.clearFilter.emit();
  }

  onViewAll(): void {
    this.viewAllClick.emit();
  }

  onCollapse(): void {
    this.collapseClick.emit();
  }

  onEmptyStateAction(): void {
    this.emptyStateAction.emit();
  }
}
