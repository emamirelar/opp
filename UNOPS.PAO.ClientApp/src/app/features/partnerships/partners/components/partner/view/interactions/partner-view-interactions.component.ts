import { ChangeDetectionStrategy, Component, inject, signal, computed, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { HttpClient } from '@angular/common/http';
import { ListviewComponent } from '@features/list-view/components/listview/listview.component';
import { ListViewColumn, ListViewConfig, SearchParams } from '@features/list-view/components/listview/listview.model';
import { PermissionUtilityService, PermissionService, EntityPermissions } from '@core/services/auth';
import { FeedbackDialogService } from '@shared/services/ui';
import { EntityConfigurationService } from '@shared/services/api/entity-configuration.service';
import { DialogService } from 'primeng/dynamicdialog';
import { InteractionModalComponent } from '@partnerships/interactions/components/interaction/modal/interaction-modal.component';
import { Router } from '@angular/router';
import { SearchField } from '@shared/services/utils';
import { InteractionIconService } from '@shared/services/domain';
import { TimelineComponent, TimelineConfig } from '@shared/components/data-display/timeline/timeline.component';
import { TabViewModule } from 'primeng/tabview';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { CreateOpportunityFromInteractionsDialogComponent } from '@partnerships/interactions/components/dialogs/create-opportunity-from-interactions-dialog.component';
import { CreateOpportunityFromInteractionsConfig } from '@partnerships/interactions/models/interaction-selection.model';

// Backend SearchFieldInfo interface to match the API response
interface SearchFieldInfo {
  field: string;
  displayName: string;
  fieldType: string;
  isNavigationProperty?: boolean;
  allowedOperators: string[];
  dropdownOptions?: DropdownOption[];
}

interface DropdownOption {
  value: string;
  label: string;
}

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
    ProgressSpinnerModule,
    CreateOpportunityFromInteractionsDialogComponent
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
        <div class="flex items-center justify-end gap-4 flex-wrap">
            <p-button
              [label]="'title.newInteraction' | translate"
              icon="pi pi-plus"
              rounded
              (click)="openNewInteractionModal()"
            />
        </div>
      }
      @if(!permissionsLoading() && permissionUtilityService.canCreate(opportunityEntityPermissions())) {
        <div class="flex items-center justify-end gap-4 flex-wrap">
            <p-button
              [label]="'button.newOpportunity' | translate"
              icon="pi pi-plus"
              severity="secondary"
              rounded
              (click)="openCreateOpportunityDialog()"
            />
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
      
      <!-- Create Opportunity from Interactions Dialog -->
      <app-create-opportunity-from-interactions-dialog
        [config]="dialogConfig()"
        (opportunityCreated)="handleOpportunityCreated($event)"
      />
    </div>
  `,
  styles: [``],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerViewInteractionsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private dialogService = inject(DialogService);
  private entityConfigurationService = inject(EntityConfigurationService);
  private feedbackDialogService = inject(FeedbackDialogService);
  public permissionUtilityService = inject(PermissionUtilityService);
  private permissionService = inject(PermissionService);
  private interactionIconService = inject(InteractionIconService);
  private translateService = inject(TranslateService);

  // ViewChild to access the dialog component
  @ViewChild(CreateOpportunityFromInteractionsDialogComponent)
  createOpportunityDialog?: CreateOpportunityFromInteractionsDialogComponent;

  // Get partner ID from route
  partnerId = signal<string>('');
  partnerName = signal<string>('');

  // Dialog configuration
  dialogConfig = computed<CreateOpportunityFromInteractionsConfig>(() => ({
    mode: 'list-view',
    partnerId: parseInt(this.partnerId()),
    partnerName: this.partnerName(),
    preSelectedInteractionIds: []
  }));

  // Permission handling for interactions
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Interaction');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  // Permission handling for Opportunity (needed for Create Opportunity action)
  opportunityEntityPermissions = signal<EntityPermissions>({
    entity: 'Opportunity',
    hasAccess: false,
    permissions: {
      canRead: false,
      canCreate: false,
      canUpdate: false,
      canDelete: false,
      canExport: false,
      canImport: false
    }
  });

  // Dynamic interaction columns loaded from API
  columns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(true);

  // Dynamic search fields from API
  searchFieldsFromAPI = signal<SearchFieldInfo[]>([]);
  isLoadingSearchFields = signal<boolean>(false);
  searchFieldsError = signal<string | null>(null);

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
      titleTemplate: this.translateService.instant('partner.interactions.timeline.clusterTitle', { count: '{count}' }),
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

  // Configure listview behavior with computed permissions and dynamic search fields
  listviewConfig = computed<ListViewConfig>(() => ({
    enableSelection: true,
    enablePagination: true,
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enableSorting: true,
    enableSearch: true,
    enableExport: this.entityPermissions().permissions.canCreate || this.entityPermissions().permissions.canUpdate,
    entityName: 'Interaction',
    scrollable: true,
    scrollHeight: 'flex',
    defaultSortField: 'subject',
    defaultSortOrder: 'asc',
    sortableFields: [
      { field: 'subject', label: 'Subject' },
      { field: 'createdDate', label: 'Created Date' },
      { field: 'lastModifiedDate', label: 'Last Updated Date' }
    ],
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: this.translateService.instant('partner.interactions.search.placeholder'),
      entityType: 'Interaction' as const,
      searchableFields: this.getSearchableFields()
    },
    searchMetadata: {
      enabled: true,
      defaultVisible: false
    }
  }));

  // Convert API search fields to SearchField format
  private getSearchableFields(): SearchField[] {
    const apiFields = this.searchFieldsFromAPI();
    
    if (apiFields.length > 0) {
      return apiFields.map(field => {
        // Get translation or fallback to the displayName itself
        const translatedLabel = this.translateService.instant(field.displayName);
        const label = translatedLabel !== field.displayName ? translatedLabel : field.displayName;
        
        return {
          field: field.field,
          label: label,
          type: this.mapFieldTypeToSearchFieldType(field.fieldType),
          operators: field.allowedOperators || ['like', 'eq', 'neq'],
          dropdownOptions: field.dropdownOptions
        };
      });
    }
    
    // Fallback to empty array if API hasn't loaded yet
    return [];
  }

  // Map backend field types to frontend search field types
  private mapFieldTypeToSearchFieldType(backendType: string): 'string' | 'number' | 'date' | 'boolean' {
    switch (backendType.toLowerCase()) {
      case 'text':
      case 'string':
      case 'enum':
      case 'dropdown':
        return 'string';
      case 'number':
      case 'int':
      case 'decimal':
        return 'number';
      case 'date':
      case 'datetime':
        return 'date';
      case 'bool':
      case 'boolean':
        return 'boolean';
      default:
        return 'string';
    }
  }

  ngOnInit() {
    // Get partner ID from route params
    this.route.parent?.paramMap.subscribe(params => {
      const id = params.get('recordId');
      if (id) {
        this.partnerId.set(id);
      }
    });

    // Get partner data from resolver
    this.route.parent?.data.subscribe(data => {
      if (data['partnerData']) {
        this.partnerName.set(data['partnerData'].name || '');
      }
    });

    // Load permissions for Interaction
    this.permissionUtils.loadPermissions(this.router);

    // Load permissions for Opportunity entity directly by entity name
    this.permissionService.getEntityPermissions('Opportunity').subscribe({
      next: (permissions) => {
        this.opportunityEntityPermissions.set(permissions);
      },
      error: (error) => {
        console.error('Error loading Opportunity permissions:', error);
      }
    });

    // Load dynamic columns from API
    this.loadInteractionColumns();
    
    // Load dynamic search fields from API
    this.loadSearchFields();
  }

  private loadSearchFields(): void {
    this.isLoadingSearchFields.set(true);
    this.searchFieldsError.set(null);

    const endpoint = '/api/interaction/search-fields';

    this.http.get<SearchFieldInfo[]>(endpoint).subscribe({
      next: (searchFields) => {
        // Transform API response to include translation keys for displayName
        const transformedFields = searchFields.map(field => ({
          ...field,
          displayName: this.translateService.instant(field.displayName) || field.displayName
        }));
        
        this.searchFieldsFromAPI.set(transformedFields);
        this.isLoadingSearchFields.set(false);
      },
      error: (error) => {
        console.error('Error loading interaction search fields:', error);
        this.searchFieldsError.set('Failed to load search fields');
        this.isLoadingSearchFields.set(false);
        
        // Fallback to empty array if API fails
        this.searchFieldsFromAPI.set([]);
      }
    });
  }

  private loadInteractionColumns() {
    this.columnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('Interaction')
      .subscribe({
        next: (columns) => {
          // Filter out redundant partner-related columns since we're already in partner context
          const filteredColumns = columns.filter(col => 
            !['partner.name', 'partnerName', 'partnerId', 'partner.id'].includes(col.field)
          );
          
          // Convert backend columns to frontend format and add template functions
          const processedColumns = filteredColumns.map(col => this.processColumn(col));
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
      helperText: column.helperText,
      thumbnailSize: column.thumbnailSize,
      thumbnailShape: column.thumbnailShape,
      thumbnailBorder: column.thumbnailBorder,
      thumbnailFallback: column.thumbnailFallback,
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

  /**
   * @uiButton create_interaction
   * @description Opens the interaction creation modal pre-filled with the current partner information
   * @label New Interaction
   * @icon pi pi-plus
   * @when_to_use When you want to record a new meeting, call, email, or other communication with this partner
   * @permissions INTERACTION_CREATE
   */
  openNewInteractionModal(): void {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('partner.interactions.error.createPermissionDenied'),
        summary: this.translateService.instant('common.error.permissionDenied')
      });
      return;
    }

    const ref = this.dialogService.open(InteractionModalComponent, {
      header: this.translateService.instant('partner.interactions.modal.newHeader'),
      width: '90%',
      height: '90%',
      modal: true,
      closable: true,
      data: {
        initialData: {
          partnerId: this.partnerId() // Pre-fill partner ID
        },
        partnerContext: {
          partnerId: this.partnerId(),
          lockPartner: false // Allow partner selection but require at least one contact from current partner
        }
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        // Refresh the listview and timeline
        window.dispatchEvent(new CustomEvent('refresh-listview'));

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

  /**
   * @uiButton view_interaction
   * @description Opens the interaction detail page in a new tab for viewing or editing
   * @label View Interaction
   * @icon pi pi-external-link
   * @when_to_use When you want to view full details of an interaction record
   * @permissions INTERACTION_READ
   */
  openEditInteractionModal(item: any): void {
    // Navigate to interaction detail page in new tab
    const interactionUrl = `#/partnerships/interactions/${item.id}`;
    window.open(interactionUrl, '_blank');
  }

  onSearchChange(searchParams: SearchParams) {
    // console.log('Partner interactions search changed:', searchParams);
  }

  onTimelineRangeChanged(range: {start: Date, end: Date}) {
  }

  /**
   * Open the Create Opportunity from Interactions dialog
   */
  openCreateOpportunityDialog(): void {
    // Check if user has permission to create opportunities
    if (!this.permissionUtilityService.canCreate(this.opportunityEntityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('message.noPermissionToCreate'),
        summary: this.translateService.instant('message.permissionDenied'),
      });
      return;
    }
    
    if (this.createOpportunityDialog) {
      this.createOpportunityDialog.visible.set(true);
    }
  }

  /**
   * Handle opportunity creation success
   */
  handleOpportunityCreated(opportunity: any): void {
    this.feedbackDialogService.showSuccessToast({
      summary: this.translateService.instant('common.success.title'),
      detail: this.translateService.instant('message.opportunityCreatedFromInteractions', { count: 1 })
    });

    // Open the new opportunity in a new tab
    if (opportunity && opportunity.id) {
      const url = this.router.serializeUrl(
        this.router.createUrlTree(['/partnerships/opportunities', opportunity.id])
      );
      window.open(url, '_blank');
    }
  }
}
