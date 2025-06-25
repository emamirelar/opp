import { ChangeDetectionStrategy, Component, inject, signal, computed, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { ListviewComponent } from '../../../../../../common/pages/components/listview/listview.component';
import { ListViewColumn, ListViewConfig, SearchParams } from '../../../../../../common/pages/components/listview/listview.model';
import { PermissionUtilityService } from '../../../../../../essentials/services/permission-utility.service';
import { FeedbackDialogService } from '../../../../../../common/reusables/services/feedback-dialog.service';
import { EntityConfigurationService } from '../../../../services/entity-configuration.service';
import { DialogService } from 'primeng/dynamicdialog';
import { InteractionModalComponent } from '../../../interaction/modal/interaction-modal.component';
import { Router } from '@angular/router';
import { SearchField } from '../../../../../../common/services/search-parser.service';
import { InteractionIconService } from '../../../../../../common/services/interaction-icon.service';
import { TimelineComponent, TimelineConfig } from '../../../../../../common/reusables/components/timeline/timeline.component';
import { TabViewModule } from 'primeng/tabview';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-partner-view-interactions',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    Button,
    ListviewComponent,
    TimelineComponent,
    TabViewModule,
    ProgressSpinnerModule
  ],
  providers: [DialogService],
  template: `
    <div class="flex flex-col gap-8 w-full">
      <app-timeline
        [dataUrl]="interactionsApiUrl()"
        [config]="timelineConfig()"
        [partnerId]="partnerId() ? +partnerId() : undefined"
        (itemSelect)="openEditInteractionModal($event)"
        (rangeChanged)="onTimelineRangeChanged($event)">
      </app-timeline>

      @if(!permissionsLoading() && permissionUtilityService.canCreate(entityPermissions())) {
        <div class="flex items-center gap-4 flex-wrap">
            <p-button class="ml-auto"
                      [label]="'title.newInteraction' | translate"
                      icon="pi pi-plus"
                      rounded
                      (click)="openNewInteractionModal()"></p-button>
        </div>
      }

      <app-listview
        [dataUrl]="interactionsApiUrl()"
        [columns]="columns()"
        [entityType]="'Interaction'"
        [config]="listviewConfig()"
        (rowClick)="openEditInteractionModal($event)"
        (searchChange)="onSearchChange($event)"
      >
      </app-listview>
    </div>
  `,
  styles: [``],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerViewInteractionsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private dialogService = inject(DialogService);
  private entityConfigurationService = inject(EntityConfigurationService);
  private feedbackDialogService = inject(FeedbackDialogService);
  public permissionUtilityService = inject(PermissionUtilityService);
  private interactionIconService = inject(InteractionIconService);

  // Get partner ID from route
  partnerId = signal<string>('');

  // Permission handling for interactions
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Interaction');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  // Dynamic interaction columns loaded from API
  columns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(true);

  // Timeline configuration with navigator, clustering, and lazy loading
  timelineConfig = computed<TimelineConfig>(() => ({
    showNavigator: true,
    navigatorHeight: '60px',
    aggregateByDay: true,
    height: '200px',
    zoomable: true,
    moveable: true,
    selectable: true,
    enableClustering: true,
    dataLoadingStrategy:  'navigator-full',
    cluster: {
      maxItems: 3,
      titleTemplate: 'Groupe de {count} interactions',
      showStipes: true,
      fitOnDoubleClick: true
    },
    enableLazyLoading: true,
    lazyLoading: {
      bufferDays: 60,
      maxItemsPerLoad: 500,
      preloadOnZoom: true,
      cacheStrategy: 'session',
      maxCacheSize: 15,
      cacheTTL: 120,
      enablePartialLoading: true
    },
    rangeConstraints: {
      minRangeDuration: 24 * 60 * 60 * 1000,
      maxRangeDuration: 365 * 24 * 60 * 60 * 1000,
      enforceMinimum: true
    }
  }));

  // Computed API URL with partner filter
  interactionsApiUrl = computed(() => {
    const id = this.partnerId();
    return `/api/interactions?partnerId=${id}`;
  });

  // Configure listview behavior with computed permissions
  listviewConfig = computed<ListViewConfig>(() => ({
    enableSelection: true,
    enablePagination: true,
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enableSorting: true,
    enableSearch: false,
    enableExport: this.entityPermissions().permissions.canCreate || this.entityPermissions().permissions.canUpdate,
    entityName: 'Interaction',
    scrollable: true,
    scrollHeight: 'flex',
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: 'Search partner interactions...',
      searchableFields: [
        {
          field: 'type',
          label: 'Type',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        {
          field: 'subject',
          label: 'Subject',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        {
          field: 'description',
          label: 'Description',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        {
          field: 'date',
          label: 'Date',
          type: 'date',
          operators: ['is', 'is not', 'after', 'before', 'between', '>', '<', '>=', '<=']
        },
        {
          field: 'contactName',
          label: 'Contact Name',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        }
      ] as SearchField[]
    }
  }));

  ngOnInit() {
    // Get partner ID from route params
    this.route.parent?.paramMap.subscribe(params => {
      const id = params.get('recordId');
      if (id) {
        this.partnerId.set(id);
      }
    });

    // Load permissions
    this.permissionUtils.loadPermissions(this.router);

    // Load dynamic columns from API
    this.loadInteractionColumns();
  }

  private loadInteractionColumns() {
    this.columnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('Interaction')
      .subscribe({
        next: (columns) => {
          // Convert backend columns to frontend format and add template functions
          const processedColumns = columns.map(col => this.processColumn(col));
          this.columns.set(processedColumns);
          this.columnsLoading.set(false);
        },
        error: (error) => {
          console.error('Failed to load interaction columns:', error);
          // Fallback to default columns if API fails
          this.setFallbackColumns();
          this.columnsLoading.set(false);
        }
      });
  }

  private processColumn(column: any): ListViewColumn {
    const processedColumn: ListViewColumn = {
      field: column.field,
      label: column.label,
      type: column.type,
      sortable: column.sortable,
      width: column.width,
      ellipsis: column.ellipsis,
      helperText: column.helperText
    };

    // Detect interaction type columns and convert them to interactionIcon type
    if (column.field === 'type' && column.type === 'text') {
      processedColumn.type = 'interactionIcon';
    }

    // Handle nested field paths (fields with dots) by adding a template function
    if (column.field && column.field.includes('.') && column.type !== 'template' && column.type !== 'interactionIcon') {
      // Keep the original field for identification but add a template function to access nested data
      processedColumn.templateFn = (rowData: any) => {
        const value = this.getNestedProperty(rowData, column.field);
        return value !== undefined && value !== null ? String(value) : '';
      };
      // Change type to template since we're now using a template function
      processedColumn.type = 'template';
    }

    // Add template function for template type columns
    const templatePattern = column.templatePattern || column.TemplatePattern;
    if (column.type === 'template' && templatePattern) {
      processedColumn.templateFn = this.createTemplateFunction(templatePattern);
    }

    return processedColumn;
  }

  private createTemplateFunction(templatePattern: string): (rowData: any) => string {
    return (rowData: any) => {
      return templatePattern.replace(/\{([^}]+)\}/g, (match, expression) => {
        try {
          const value = this.getNestedProperty(rowData, expression.trim());
          return value !== null && value !== undefined ? String(value) : '';
        } catch (error) {
          console.warn(`Template expression error: ${expression}`, error);
          return '';
        }
      });
    };
  }

  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((current, prop) => current?.[prop], obj);
  }

  private setFallbackColumns() {
    const fallbackColumns: ListViewColumn[] = [
      {
        field: 'type',
        label: 'label.interaction.type',
        sortable: true,
        type: 'interactionIcon'
      },
      {
        field: 'date',
        label: 'label.interaction.date',
        sortable: true,
        type: 'date'
      },
      {
        field: 'subject',
        label: 'label.interaction.subject',
        sortable: false,
        type: 'text'
      },
      {
        field: 'description',
        label: 'label.interaction.description',
        sortable: false,
        type: 'text'
      }
    ];
    this.columns.set(fallbackColumns);
  }

  openNewInteractionModal(): void {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to create interactions',
        summary: 'Permission Denied'
      });
      return;
    }

    const ref = this.dialogService.open(InteractionModalComponent, {
      header: 'New Interaction',
      width: '90%',
      height: '90%',
      modal: true,
      closable: true,
      data: {
        initialData: {
          partnerId: this.partnerId() // Pre-fill partner ID
        }
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        console.log('Interaction created:', result);
        // Refresh the listview and timeline
        const listviewElement = document.querySelector('app-listview');
        if (listviewElement) {
          listviewElement.dispatchEvent(new CustomEvent('refresh-listview'));
        }

        const timelineElement = document.querySelector('app-timeline') as any;
        if (timelineElement) {
          if (result.date && timelineElement.invalidateCache) {
            const interactionDate = new Date(result.date);
            const bufferDays = 7;
            const start = new Date(interactionDate.getTime() - (bufferDays * 24 * 60 * 60 * 1000));
            const end = new Date(interactionDate.getTime() + (bufferDays * 24 * 60 * 60 * 1000));
            timelineElement.invalidateCache(start, end);
          }

          if (timelineElement.refreshTimeline) {
            timelineElement.refreshTimeline();
          }
        }
      }
    });
  }

  openEditInteractionModal(item: any): void {
    // Check if user has update permission
    if (!this.permissionUtilityService.canUpdate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to edit interactions',
        summary: 'Permission Denied'
      });
      return;
    }

    const ref = this.dialogService.open(InteractionModalComponent, {
      header: 'Edit Interaction',
      width: '90%',
      height: '90%',
      modal: true,
      data: {
        id: item.id,
        initialData: item
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        console.log('Interaction updated:', result);
        // Refresh the listview and timeline
        const listviewElement = document.querySelector('app-listview');
        if (listviewElement) {
          listviewElement.dispatchEvent(new CustomEvent('refresh-listview'));
        }

        const timelineElement = document.querySelector('app-timeline') as any;
        if (timelineElement) {
          if (result.date && timelineElement.invalidateCache) {
            const interactionDate = new Date(result.date);
            const bufferDays = 7;
            const start = new Date(interactionDate.getTime() - (bufferDays * 24 * 60 * 60 * 1000));
            const end = new Date(interactionDate.getTime() + (bufferDays * 24 * 60 * 60 * 1000));
            timelineElement.invalidateCache(start, end);
          }

          if (timelineElement.refreshTimeline) {
            timelineElement.refreshTimeline();
          }
        }
      }
    });
  }

  onSearchChange(searchParams: SearchParams) {
    console.log('Partner interactions search changed:', searchParams);
  }

  onTabChange(event: any) {
    console.log('Tab changed:', event);
  }

  onTimelineRangeChanged(range: {start: Date, end: Date}) {
    console.log('Timeline range changed:', range);
  }
}
