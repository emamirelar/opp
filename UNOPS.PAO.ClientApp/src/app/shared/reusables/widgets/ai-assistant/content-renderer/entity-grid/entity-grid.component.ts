import { Component, Input, OnInit, inject, signal, Output, EventEmitter, ChangeDetectionStrategy, ViewChild, ElementRef, AfterViewInit, OnDestroy, ChangeDetectorRef, HostListener, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityConfigurationService } from '@features/shared/services/entity-configuration.service';
import { ListviewCardComponent } from '../../../../../pages/components/listview/card/listview-card.component';
import { ListViewColumn, ListViewConfig } from '../../../../../pages/components/listview/listview.model';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-entity-grid',
  standalone: true,
  imports: [CommonModule, ListviewCardComponent, TranslateModule],
  templateUrl: './entity-grid.component.html',
  styleUrls: ['./entity-grid.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EntityGridComponent implements OnInit, AfterViewInit, OnDestroy {
  @Input() entityType: string = 'partner';
  @Input() gridData!: any[];
  @Output() cardClicked = new EventEmitter<{ entityType: string, entityId: string, rowData: any }>();
  
  private entityConfigurationService = inject(EntityConfigurationService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  
  columns = signal<ListViewColumn[]>([]);
  isLoading = signal(false);
  
  // Width tracking for responsive layout
  componentWidth = signal<number>(0);
  private widthTrackingInterval?: ReturnType<typeof setInterval>;
  private resizeObserver?: ResizeObserver;
  
  // ViewChild reference for width tracking
  @ViewChild('widthTracker', { static: false }) widthTracker?: ElementRef;
  
  // Computed responsive columns based on component width
  responsiveColumns = computed(() => {
    const width = this.componentWidth();
    const allColumns = this.columns();
    
    if (!allColumns || allColumns.length === 0) {
      return [];
    }
    
    // Determine number of columns to show based on component width
    let columnsToShow: number;
    if (width >= 1200) {
      columnsToShow = Math.min(6, allColumns.length); // Show up to 6 columns on very wide screens
    } else if (width >= 900) {
      columnsToShow = Math.min(4, allColumns.length); // Show up to 4 columns on wide screens
    } else if (width >= 600) {
      columnsToShow = Math.min(3, allColumns.length); // Show up to 3 columns on medium screens
    } else if (width >= 400) {
      columnsToShow = Math.min(2, allColumns.length); // Show up to 2 columns on small screens
    } else {
      columnsToShow = 1; // Show only 1 column on very small screens
    }
    
    // Always include the first column (usually name/title) and then select the most important ones
    const prioritizedColumns = this.prioritizeColumns(allColumns);
    return prioritizedColumns.slice(0, columnsToShow);
  });
  
  // Card configuration for consistent display
  cardConfig = signal<ListViewConfig>({
    pageSize: 20,
    enablePagination: false,
    enableSorting: false,
    enableSearch: false,
    enableExport: false,
    defaultViewMode: 'card',
    showViewModeToggle: false,
    autoSwitchToCardView: false
  });

  ngOnInit() {
    if (this.entityType) {
      this.loadColumns();
    } else {
      console.warn('🏗️ EntityGrid - No entity type provided');
    }
  }

  private loadColumns() {
    this.isLoading.set(true);
    
    this.entityConfigurationService.getEntityListViewConfiguration(this.entityType)
      .pipe(
        map((response: any) => {
          // Extract columns from the response
          if (response?.body?.columns) {
            return response.body.columns;
          } else if (response?.columns) {
            return response.columns;
          } else if (Array.isArray(response)) {
            return response;
          }
          return [];
        }),
        catchError((error) => {
          console.error(`Failed to load columns for ${this.entityType}:`, error);
          // Fallback: create basic columns from data structure
          return of(this.createFallbackColumns());
        })
      )
              .subscribe((columns: ListViewColumn[]) => {
          // If no columns returned from API, use fallback columns from data structure
          if (!columns || columns.length === 0) {
            const fallbackColumns = this.createFallbackColumns();
            this.columns.set(fallbackColumns);
          } else {
            this.columns.set(columns);
          }
          this.isLoading.set(false);
        });
  }



  private createFallbackColumns(): ListViewColumn[] {
    // Use predefined fallback columns configuration
    const fallbackColumns: ListViewColumn[] = [
      {
        "field": "logourl",
        "label": "Logo",
        "type": "avatar",
        "sortable": false,
        "width": undefined,
        "ellipsis": false,
        "firstLetterFallbackField": "name",
        "helperText": undefined
      },
      {
        "field": "name",
        "label": "Name",
        "type": "text",
        "sortable": false,
        "width": undefined,
        "ellipsis": false,
        "firstLetterFallbackField": undefined,
        "helperText": "Partner organization name"
      },
      {
        "field": "partnercategoryname",
        "label": "Partner Category",
        "type": "template",
        "sortable": false,
        "width": "15%",
        "ellipsis": true,
        "firstLetterFallbackField": undefined,
        "helperText": "Partner category classification"
      },
      {
        "field": "partnergroupname",
        "label": "Partner Group",
        "type": "template",
        "sortable": false,
        "width": "15%",
        "ellipsis": true,
        "firstLetterFallbackField": undefined,
        "helperText": "Partner group classification"
      },
      {
        "field": "shortname",
        "label": "shortName",
        "type": "template",
        "sortable": true,
        "width": undefined,
        "ellipsis": false,
        "firstLetterFallbackField": undefined,
        "helperText": undefined
      },
      {
        "field": "address1city",
        "label": "Address1City",
        "type": "template",
        "sortable": true,
        "width": undefined,
        "ellipsis": false,
        "firstLetterFallbackField": undefined,
        "helperText": undefined
      }
    ];

    return fallbackColumns;

    /* Original dynamic fallback columns logic - commented out but preserved
    if (!this.gridData || this.gridData.length === 0) {
      console.warn('No grid data available for creating fallback columns');
      return [];
    }

    // Get all unique keys from all objects to handle cases where not all objects have the same properties
    const allKeys = new Set<string>();
    this.gridData.forEach(item => {
      if (item && typeof item === 'object') {
        Object.keys(item).forEach(key => allKeys.add(key));
      }
    });

    if (allKeys.size === 0) {
      console.warn('No properties found in grid data for creating fallback columns');
      return [];
    }

    const firstItem = this.gridData[0];
    const columns = Array.from(allKeys).map(key => ({
      field: key,
      label: this.formatHeader(key),
      type: this.inferColumnType(firstItem[key]) as 'text' | 'date' | 'number' | 'currency',
      sortable: true
    }));

    return columns;
    */
  }

  private formatHeader(key: string): string {
    return key
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase())
      .trim();
  }

  private inferColumnType(value: any): 'text' | 'date' | 'number' | 'currency' {
    if (typeof value === 'number') return 'number';
    if (value instanceof Date) return 'date';
    return 'text';
  }

  handleRowClick(rowData: any) {
    
    if (!rowData || !rowData.id) {
      return;
    }
    this.cardClicked.emit({ entityType: this.entityType, entityId: rowData.id, rowData });
  }
  
  private prioritizeColumns(columns: ListViewColumn[]): ListViewColumn[] {
    // Define priority order for different column types
    const priorityOrder = ['avatar', 'text', 'template', 'date', 'number', 'currency'];
    const importantFields = ['name', 'title', 'logourl', 'partnercategoryname', 'partnergroupname', 'shortname', 'address1city'];
    
    return columns.sort((a, b) => {
      // First priority: important fields
      const aImportant = importantFields.includes(a.field.toLowerCase());
      const bImportant = importantFields.includes(b.field.toLowerCase());
      if (aImportant && !bImportant) return -1;
      if (!aImportant && bImportant) return 1;
      
      // Second priority: column type
      const aTypeIndex = priorityOrder.indexOf(a.type);
      const bTypeIndex = priorityOrder.indexOf(b.type);
      if (aTypeIndex !== bTypeIndex) {
        return (aTypeIndex === -1 ? priorityOrder.length : aTypeIndex) - 
               (bTypeIndex === -1 ? priorityOrder.length : bTypeIndex);
      }
      
      // Third priority: alphabetical by label
      return a.label.localeCompare(b.label);
    });
  }
  
  ngAfterViewInit() {
    // Initial width measurement with multiple attempts
    this.attemptWidthMeasurement();

    // Use ResizeObserver for more efficient width tracking if available
    if (typeof ResizeObserver !== 'undefined' && this.widthTracker?.nativeElement) {
      this.resizeObserver = new ResizeObserver((entries) => {
        for (const entry of entries) {
          const width = entry.contentRect.width;
          if (width > 0) {
            const currentWidth = this.componentWidth();
            if (currentWidth !== width) {
              this.componentWidth.set(width);
              this.cdr.detectChanges();
            }
          }
        }
      });
      
      this.resizeObserver.observe(this.widthTracker.nativeElement);
    } else {
      // Fallback to polling for older browsers
      this.startWidthPolling();
    }
  }
  
  ngOnDestroy() {
    // Clean up ResizeObserver
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }
    
    // Clean up polling interval
    if (this.widthTrackingInterval) {
      clearInterval(this.widthTrackingInterval);
    }
  }
  
  @HostListener('window:resize', ['$event'])
  onResize() {
    setTimeout(() => {
      this.updateComponentWidth();
    }, 100);
  }
  
  private attemptWidthMeasurement() {
    // Try multiple times with increasing delays to ensure element is rendered
    const attempts = [0, 50, 100, 250, 500];
    
    attempts.forEach((delay, index) => {
      setTimeout(() => {
        this.updateComponentWidth();
        
        // If we got a width, stop trying
        if (this.componentWidth() > 0 && index < attempts.length - 1) {
          return;
        }
      }, delay);
    });
  }
  
  private startWidthPolling() {
    this.widthTrackingInterval = setInterval(() => {
      this.updateComponentWidth();
    }, 1000); // Check every second as fallback
  }
  
  private updateComponentWidth() {
    if (!this.widthTracker?.nativeElement) {
      return;
    }
    
    const element = this.widthTracker.nativeElement;
    const width = element.offsetWidth || element.clientWidth || 0;
    
    if (width > 0) {
      const currentWidth = this.componentWidth();
      if (currentWidth !== width) {
        this.componentWidth.set(width);
        // Trigger change detection
        this.cdr.detectChanges();
      }
    }
  }
  
  // Debug method to check current width and responsive columns (can be called from browser console)
  getCurrentWidth() {
    return {
      componentWidth: this.componentWidth(),
      totalColumns: this.columns().length,
      responsiveColumns: this.responsiveColumns().length,
      responsiveColumnFields: this.responsiveColumns().map(c => c.field)
    };
  }

  private buildEntityUrl(entityType: string, entityId: number | string, rowData: any): string | null {
    // Remove window.location.origin and hash logic, just return the route path
    switch (entityType?.toLowerCase()) {
      case 'partner':
        return `/partnerships/partners/${entityId}`;
      case 'contact':
        if (rowData.partnerId) {
          return `/partnerships/partners/${rowData.partnerId}/contacts/${entityId}`;
        }
        return `/contacts/${entityId}`;
      case 'interaction':
        return `/interactions/${entityId}`;
      case 'partneragreement':
      case 'partnership':
        return `/partnerships/agreements/${entityId}`;
      default:
        // TAD: Defaulting to partner for now
        // const routeSegment = entityType.toLowerCase().replace(/\s+/g, '-');
        // return `/${routeSegment}s/${entityId}`;
        return `/partnerships/partners/${entityId}`;
    }
  }
} 
