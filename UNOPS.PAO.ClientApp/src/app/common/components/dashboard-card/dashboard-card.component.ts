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
  template: `
    <div class="dashboard-card bg-unops-surface-primary rounded-unops-lg shadow-unops-md border border-unops-neutral-200 overflow-hidden"
         [style.height]="cardHeight"
         [class.card-size-auto]="cardSize === 'auto'"
         [class.card-size-fixed]="cardSize === 'fixed'"
         [class.card-size-compact]="cardSize === 'compact'"
         [class.card-size-tall]="cardSize === 'tall'"
         [class.mobile-mode]="isMobile">
      
      <!-- Header -->
      <div class="card-header p-unops-lg border-b border-unops-neutral-200">
        <div class="flex items-center justify-between">
          <div class="flex items-center space-x-unops-md">
            <div class="flex items-center justify-center w-10 h-10 rounded-unops-lg"
                 [ngClass]="config.iconColor">
              <i class="material-symbols-outlined text-unops-xl" 
                 [ngClass]="getIconTextColor()">{{ config.icon }}</i>
            </div>
            <div>
              <h3 class="font-unops-display text-unops-headline-small font-unops-semibold text-unops-neutral-900">
                {{ config.title }}
              </h3>
              <p class="font-unops-body text-unops-body-small text-unops-neutral-600">
                {{ config.subtitle }}
              </p>
            </div>
          </div>
          
          <!-- Collapse button for expanded view -->
          @if (isExpanded) {
            <button 
              pButton 
              type="button" 
              icon="pi pi-times" 
              class="p-button-outlined p-button-sm"
              (click)="onCollapse()"
              pTooltip="{{ 'dashboard.card.collapse' | translate }}"
              tooltipPosition="bottom">
            </button>
          }
        </div>
      </div>

      <!-- Content Area -->
      <div class="card-content flex flex-col" 
           [class.expanded-content]="isExpanded"
           [class.normal-content]="!isExpanded">
        
        <!-- Filters (if enabled and provided) -->
        @if (config.showFilters && filters && filters.length > 0) {
          <div class="filter-section p-unops-lg pb-0">
            <div class="flex flex-wrap gap-unops-sm">
              @for (filter of filters; track filter.id) {
                <button 
                  class="px-unops-md py-unops-sm rounded-unops-md font-unops-body text-unops-body-small font-unops-medium transition-all duration-unops-short border"
                  [class.bg-unops-primary]="filter.active"
                  [class.text-unops-white]="filter.active"
                  [class.border-unops-primary]="filter.active"
                  [class.bg-unops-surface-cool]="!filter.active"
                  [class.text-unops-neutral-700]="!filter.active"
                  [class.border-unops-neutral-300]="!filter.active"
                  (click)="onFilterClick(filter)">
                  {{ filter.label }}
                  @if (filter.count !== undefined) {
                    ({{ filter.count }})
                  }
                </button>
              }
              @if (hasActiveFilter()) {
                <button 
                  class="px-unops-sm py-unops-sm rounded-unops-md bg-unops-neutral-200 text-unops-neutral-600 hover:bg-unops-neutral-300 transition-colors"
                  (click)="onClearFilter()">
                  <i class="material-symbols-outlined text-unops-sm">close</i>
                </button>
              }
            </div>
          </div>
        }

        <!-- Main Content Area -->
        <div class="main-content flex-1 p-unops-lg" 
             [class.pt-unops-sm]="config.showFilters && filters && filters.length > 0">
          
          <!-- Content Template -->
          @if (contentTemplate) {
            <ng-container *ngTemplateOutlet="contentTemplate"></ng-container>
          } @else if (hasContent) {
            <ng-content></ng-content>
          } @else {
            <!-- Empty State -->
            <div class="empty-state flex flex-col items-center justify-center h-full text-center py-unops-lg">
              <div class="flex items-center justify-center w-12 h-12 bg-unops-neutral-200 rounded-unops-full mb-unops-md">
                <i class="material-symbols-outlined text-unops-xl text-unops-neutral-400">
                  {{ config.emptyStateIcon || 'inbox' }}
                </i>
              </div>
              <h4 class="font-unops-body text-unops-body-large font-unops-medium text-unops-neutral-900 mb-unops-sm">
                {{ config.emptyStateTitle || ('dashboard.card.noItemsFound' | translate) }}
              </h4>
              <p class="font-unops-body text-unops-body-medium text-unops-neutral-500 mb-unops-sm">
                {{ config.emptyStateMessage || ('dashboard.card.noDataMessage' | translate) }}
              </p>
              @if (config.emptyStateActionLabel) {
                <button 
                  pButton 
                  type="button" 
                  icon="pi pi-plus" 
                  [label]="config.emptyStateActionLabel"
                  class="p-button-sm unops-button-primary"
                  (click)="onEmptyStateAction()">
                </button>
              }
            </div>
          }
        </div>

        <!-- Footer with View All (hidden on mobile) -->
        @if (config.showViewAll !== false && !isExpanded && !isMobile) {
          <div class="card-footer border-t border-unops-neutral-200 p-unops-md">
            @if (showViewAllButton) {
              <div class="text-center">
                <button 
                  class="text-unops-body-small text-unops-primary hover:text-unops-primary-dark font-unops-medium transition-colors"
                  (click)="onViewAll()">
                  {{ getViewAllText() }}
                </button>
              </div>
            } @else {
              <div class="text-center text-unops-body-small text-unops-neutral-400">
                &nbsp;
              </div>
            }
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .dashboard-card {
      display: flex;
      flex-direction: column;
    }

    .card-header {
      flex-shrink: 0;
    }

    .card-content {
      flex: 1;
      min-height: 0; /* Important for flex children */
    }

    .normal-content {
      height: calc(100% - 80px); /* Subtract header height */
    }

    .expanded-content {
      min-height: 400px;
    }

    .main-content {
      overflow-y: auto;
      min-height: 0;
    }

    .card-footer {
      flex-shrink: 0;
      margin-top: auto;
    }

    .filter-section {
      flex-shrink: 0;
    }

    .empty-state {
      min-height: 200px;
    }

    /* Size-based styling */
    .card-size-fixed {
      height: 420px !important;
    }

    .card-size-compact {
      height: 280px !important;
    }

    .card-size-tall {
      height: 520px !important;
    }

    .card-size-auto {
      height: auto !important;
      min-height: 300px;
    }

    /* Responsive height management for different zoom levels */
    @media screen and (min-width: 1024px) {
      .dashboard-card.card-size-fixed:not(.expanded) {
        height: 420px !important;
      }
      
      .dashboard-card:not(.expanded):not(.card-size-fixed):not(.card-size-compact):not(.card-size-tall):not(.card-size-auto) {
        height: 420px;
      }
    }

    @media screen and (max-width: 1023px) {
      .dashboard-card.card-size-fixed:not(.expanded) {
        height: auto !important;
        min-height: 350px;
      }
      
      .dashboard-card:not(.expanded):not(.card-size-fixed):not(.card-size-compact):not(.card-size-tall):not(.card-size-auto) {
        height: auto;
        min-height: 350px;
      }
    }

    /* Handle high zoom levels (when viewport becomes smaller) */
    @media screen and (max-width: 768px) {
      .dashboard-card.card-size-fixed:not(.expanded),
      .dashboard-card.card-size-compact:not(.expanded) {
        height: auto !important;
        min-height: 300px;
      }
      
      .dashboard-card:not(.expanded):not(.card-size-fixed):not(.card-size-compact):not(.card-size-tall):not(.card-size-auto) {
        height: auto;
        min-height: 300px;
      }
      
      .main-content {
        max-height: 250px;
      }
    }

    /* Very high zoom or small screens */
    @media screen and (max-width: 480px) {
      .dashboard-card.card-size-fixed:not(.expanded),
      .dashboard-card.card-size-compact:not(.expanded) {
        height: auto !important;
        min-height: 280px;
      }
      
      .dashboard-card:not(.expanded):not(.card-size-fixed):not(.card-size-compact):not(.card-size-tall):not(.card-size-auto) {
        height: auto;
        min-height: 280px;
      }
      
      .main-content {
        max-height: 200px;
      }
    }

    /* Ensure footer is always visible by using CSS Grid when needed */
    @supports (display: grid) {
      .card-content.normal-content {
        display: grid;
        grid-template-rows: auto 1fr auto;
        height: 100%;
      }
      
      .filter-section {
        grid-row: 1;
      }
      
      .main-content {
        grid-row: 2;
        overflow-y: auto;
      }
      
      .card-footer {
        grid-row: 3;
        margin-top: 0;
      }
    }

    /* Fallback for browsers without grid support */
    @supports not (display: grid) {
      .card-content.normal-content {
        display: flex;
        flex-direction: column;
        height: 100%;
      }
      
      .main-content {
        flex: 1;
        overflow-y: auto;
        min-height: 0;
      }
      
      .card-footer {
        flex-shrink: 0;
        margin-top: auto;
      }
    }

    /* Mobile mode - natural height, no restrictions */
    .dashboard-card.mobile-mode {
      height: auto !important;
      min-height: 0 !important;
    }

    .dashboard-card.mobile-mode .card-content {
      height: auto !important;
    }

    .dashboard-card.mobile-mode .main-content {
      overflow-y: visible !important;
      max-height: none !important;
      min-height: 0 !important;
    }

    .dashboard-card.mobile-mode .card-content.normal-content {
      display: flex;
      flex-direction: column;
      height: auto !important;
    }
  `],
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
