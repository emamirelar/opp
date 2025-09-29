import { Component, Input, OnInit, inject, signal, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntityConfigurationService } from '../../../../../../features/internal/services/entity-configuration.service';
import { ListviewCardComponent } from '../../../../../pages/components/listview/card/listview-card.component';
import { ListViewColumn, ListViewConfig } from '../../../../../pages/components/listview/listview.model';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';

@Component({
  selector: 'app-entity-grid',
  standalone: true,
  imports: [CommonModule, ListviewCardComponent],
  templateUrl: './entity-grid.component.html',
  styleUrls: ['./entity-grid.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EntityGridComponent implements OnInit {
  @Input() entityType: string = 'partner';
  @Input() gridData!: any[];
  @Output() cardClicked = new EventEmitter<{ entityType: string, entityId: string, rowData: any }>();
  
  private entityConfigurationService = inject(EntityConfigurationService);
  private router = inject(Router);
  
  columns = signal<ListViewColumn[]>([]);
  isLoading = signal(false);
  
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
    console.log('🏗️ EntityGrid - INITIALIZING NEW COMPONENT:', {
      entityType: this.entityType,
      itemCount: this.gridData?.length || 0,
      componentId: Math.random().toString(36).substr(2, 9), // Add random ID to track instances
      gridDataReference: this.gridData
    });
    
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
            console.log(`🔧 EntityGrid - No columns configuration found for ${this.entityType}, using fallback columns from data structure`);
            const fallbackColumns = this.createFallbackColumns();
            console.log(`🔧 EntityGrid - Created ${fallbackColumns.length} fallback columns:`, fallbackColumns);
            this.columns.set(fallbackColumns);
          } else {
            console.log(`✅ EntityGrid - Loaded ${columns.length} columns for ${this.entityType} from entity configuration:`, columns);
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

    console.log(`Created ${fallbackColumns.length} predefined fallback columns:`, fallbackColumns.map(c => c.label));
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

    console.log(`Created ${columns.length} fallback columns:`, columns.map(c => c.label));
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
    console.log('🔗 EntityGrid - Row clicked:', rowData);
    console.log('🔗 EntityGrid - Entity type:', this.entityType);
    
    if (!rowData || !rowData.id) {
      console.warn('🔗 EntityGrid - No ID found in row data, cannot navigate');
      return;
    }
    this.cardClicked.emit({ entityType: this.entityType, entityId: rowData.id, rowData });
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