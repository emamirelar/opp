import {
  ChangeDetectionStrategy,
  Component,
  input,
  OnInit,
  signal,
  inject,
  computed,
  ChangeDetectorRef,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

import { ButtonModule } from 'primeng/button';
import { ActivatedRoute, Router } from '@angular/router';
import { DialogService } from 'primeng/dynamicdialog';

import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ListviewComponent } from '@features/list-view/components/listview/listview.component';
import { ListViewColumn, ListViewConfig } from '@features/list-view/components/listview/listview.model';
import { SearchField } from '@shared/services/utils';
import { EntityConfigurationService } from '@shared/services/api/entity-configuration.service';
import { PermissionUtilityService } from '@core/services/auth';
import { PermissionService, EntityPermissions } from '@core/services/auth';
import { FeedbackDialogService } from '@shared/services/ui';
import { PartnerService } from '@partnerships/partners/services/partner.service';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { FloatLabelModule } from 'primeng/floatlabel';
import { MessageModule } from 'primeng/message';
import { FormsModule } from '@angular/forms';
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
  selector: 'app-partner-view-opportunities',
  templateUrl: './partner-view-opportunities.component.html',
  imports: [
    CommonModule,
    ButtonModule,
    TranslateModule,
    ListviewComponent,
    CreateOpportunityFromInteractionsDialogComponent
  ],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DialogService]
})
export class PartnerViewOpportunitiesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private partnerService = inject(PartnerService);
  private entityConfigurationService = inject(EntityConfigurationService);
  private permissionUtilityService = inject(PermissionUtilityService);
  private permissionService = inject(PermissionService);
  private feedbackDialogService = inject(FeedbackDialogService);
  private dialogService = inject(DialogService);
  private translateService = inject(TranslateService);
  private cdr = inject(ChangeDetectorRef);

  partnerId = input<string>();
  partnerName = input<string>();
  partnerStatus = signal<string | undefined>(undefined);
  dataUrl = signal<string>('');
  
  // Internal partner name signal for when loaded from route data
  internalPartnerName = signal<string>('');

  // Permission management for opportunities within partner context
  entityPermissions = signal<EntityPermissions>({
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
  permissionsLoading = signal<boolean>(true);

  // Reference to listview component for export functionality
  @ViewChild(ListviewComponent) listviewComponent!: ListviewComponent;

  // Dynamic columns loaded from API
  columns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(true);

  // Dynamic search fields from API
  searchFieldsFromAPI = signal<SearchFieldInfo[]>([]);
  isLoadingSearchFields = signal<boolean>(false);
  searchFieldsError = signal<string | null>(null);

  // Unified opportunity creation dialog
  showCreateOpportunityDialog = signal<boolean>(false);
  
  // Computed property to check if user can create opportunities
  canCreateOpportunity = computed(() => {
    // Check if user has permission to create opportunities
    return this.permissionUtilityService.canCreate(this.entityPermissions());
  });

  // Dialog configuration for unified dialog
  dialogConfig = computed<CreateOpportunityFromInteractionsConfig>(() => {
    const partnerId = this.partnerId() || this.getCurrentPartnerIdFromRoute();
    // Use input partnerName if available, otherwise use internal signal from route data
    const partnerName = this.partnerName() || this.internalPartnerName() || '';
    
    return {
      partnerId: partnerId ? +partnerId : 0,
      partnerName: partnerName,
      mode: 'list-view', // From partner opportunities tab
      preSelectedInteractionIds: [] // No interactions pre-selected
    };
  });

  // Fallback columns definition
  private fallbackColumns: ListViewColumn[] = [
    {
      field: 'name',
      label: 'opportunity.name',
      type: 'text',
      sortable: true
    },
    {
      field: 'workflowStage.name',
      label: 'opportunity.workflowStage',
      type: 'text',
      sortable: true
    },
    {
      field: 'initiativeBudgetUSD',
      label: 'opportunity.budget',
      type: 'currency',
      sortable: true
    },
    {
      field: 'targetSigningDate',
      label: 'opportunity.targetSigningDate',
      type: 'date',
      sortable: true
    }
  ];

  // Configure listview behavior with computed permissions and dynamic search fields
  config = computed<ListViewConfig>(() => ({
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enablePagination: false, // Using infinite scroll
    enableSorting: true,
    enableExport: this.entityPermissions().permissions.canCreate || this.entityPermissions().permissions.canUpdate,
    scrollable: true,
    scrollHeight: 'calc(100vh - 20rem)',
    autoSwitchToCardView: false,
    autoSwitchMinWidth: 768,
    defaultViewMode: 'card',
    enableSearch: true,
    entityName: 'Opportunity',
    defaultSortField: 'name',
    defaultSortOrder: 'asc',
    sortableFields: [
      { field: 'name', label: 'Name' },
      { field: 'createdDate', label: 'Created Date' },
      { field: 'lastModifiedDate', label: 'Last Updated Date' }
    ],
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: this.translateService.instant('search.opportunitiesPlaceholder'),
      entityType: 'Opportunity' as const,
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

  ngOnInit(): void {
    // If partnerId is provided as input, use it directly
    if (this.partnerId()) {
      this.setupDataUrlWithPartnerFilter(this.partnerId()!);
      this.loadPermissions();
      // Fetch partner status if not available from route data
      this.loadPartnerStatus(this.partnerId()!);
    } else {
      // Otherwise, get partnerId from parent route params (when used as a child route)
      this.route.parent?.paramMap.subscribe(params => {
        const recordId = params.get('recordId');
        if (recordId) {
          this.setupDataUrlWithPartnerFilter(recordId);
          // Load permissions once we have the partner ID
          this.loadPermissions();
          // Fetch partner status if not available from route data
          this.loadPartnerStatus(recordId);
        }
      });

      // Get partner data from resolved route data
      this.route.parent?.data.subscribe(data => {
        const partnerData = data['partnerData'];
        if (partnerData) {
          this.partnerStatus.set(partnerData.status);
          // Store partner name in internal signal for dialog config
          if (partnerData.name) {
            this.internalPartnerName.set(partnerData.name);
          }
        }
      });
    }

    // Load dynamic columns from API
    this.loadOpportunityColumns();
    
    // Load dynamic search fields from API
    this.loadSearchFields();
  }

  /**
   * Loads partner status from API if not already available
   */
  private loadPartnerStatus(partnerId: string): void {
    // Only fetch if status is not already set
    if (!this.partnerStatus()) {
      this.partnerService.getPartnerById(partnerId).subscribe({
        next: (partner) => {
          this.partnerStatus.set(partner.status || undefined);
        },
        error: (error) => {
          console.warn('Could not load partner status:', error);
          // Don't set error state - allow dialog to open and let backend validate
        }
      });
    }
  }

  private setupDataUrlWithPartnerFilter(partnerId: string): void {
    // Set the base URL with partnerId parameter for filtering
    this.dataUrl.set(`/api/partner/${partnerId}/opportunities`);
  }

  private loadSearchFields(): void {
    this.isLoadingSearchFields.set(true);
    this.searchFieldsError.set(null);

    const endpoint = '/api/opportunity/search-fields';

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
        console.error('Error loading opportunity search fields:', error);
        this.searchFieldsError.set('Failed to load search fields');
        this.isLoadingSearchFields.set(false);
        
        // Fallback to empty array if API fails
        this.searchFieldsFromAPI.set([]);
      }
    });
  }

  private loadPermissions(): void {
    this.permissionsLoading.set(true);
    
    // Clear cache before loading to ensure fresh permissions
    this.permissionService.clearPermissionCaches();
    
    // Construct the correct route path for opportunities within partner context
    const currentPartnerId = this.partnerId() || this.getCurrentPartnerIdFromRoute();
    const opportunitiesRoutePath = currentPartnerId ? 
      `partnerships/partners/${currentPartnerId}/opportunities` : 
      'partnerships/opportunities';
    
    // Load from server using the specific opportunities route
    this.permissionService.getEntityPermissions(opportunitiesRoutePath)
      .subscribe({
        next: (permissions) => {
          if (!permissions.hasAccess) {
            console.warn('No access to partner opportunities');
            this.router.navigate(['/access-denied']);
            return;
          }
          
          this.entityPermissions.set(permissions);
          this.permissionsLoading.set(false);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading opportunity permissions in partner context:', error);
          this.permissionsLoading.set(false);
          this.cdr.detectChanges();
        }
      });
  }

  private loadOpportunityColumns() {
    this.columnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('Opportunity')
      .subscribe({
        next: (columns) => {
          // Filter out redundant partner-related columns since we're in partner context
          const filteredColumns = columns.filter(col => 
            !['partner.name', 'partnerName', 'partnerId', 'partner.id'].includes(col.field)
          );
          
          // Process columns and handle nested fields
          const processedColumns = filteredColumns.map(col => this.processColumn(col));
          this.columns.set(processedColumns);
          this.columnsLoading.set(false);
        },
        error: (error) => {
          console.error('Failed to load opportunity columns:', error);
          // Use fallback columns if API fails
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

    // Handle nested field paths (fields with dots) by adding a template function
    if (column.field && column.field.includes('.') && column.type !== 'template') {
      processedColumn.templateFn = (rowData: any) => {
        const value = this.getNestedProperty(rowData, column.field);
        return value !== undefined && value !== null ? String(value) : '';
      };
      // Change type to template since we're using a template function
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
    this.columns.set(this.fallbackColumns);
  }

  onAddOpportunity(): void {
    this.openCreateOpportunityDialog();
  }

  private getCurrentPartnerIdFromRoute(): string {
    return this.route.parent?.snapshot.paramMap.get('recordId') || '';
  }

  handleOnOpenRecordDetails(record: any) {
    if (record == null) {
      return;
    }
    // Open opportunity in new tab
    const url = this.router.serializeUrl(
      this.router.createUrlTree(['/partnerships/opportunities', record.id])
    );
    window.open(url, '_blank');
  }

  /**
   * Opens the unified create opportunity dialog
   */
  openCreateOpportunityDialog(): void {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'message.noPermissionToCreate',
        summary: 'message.permissionDenied',
      });
      return;
    }

    // Check partner status - allow Active and Draft partners
    const currentStatus = this.partnerStatus();
    if (!currentStatus) {
      // If status is not available, fetch it from the API
      const partnerId = this.partnerId() || this.getCurrentPartnerIdFromRoute();
      if (partnerId) {
        this.partnerService.getPartnerById(partnerId).subscribe({
          next: (partner) => {
            const status = partner.status || undefined;
            this.partnerStatus.set(status);
            this.checkAndOpenDialog(status);
          },
          error: () => {
            // If we can't fetch the status, allow the dialog to open
            // The backend will validate the status
            this.showCreateOpportunityDialog.set(true);
          }
        });
        return;
      } else {
        // If we can't determine partner ID, allow the dialog to open
        // The backend will validate the status
        this.showCreateOpportunityDialog.set(true);
        return;
      }
    }

    this.checkAndOpenDialog(currentStatus);
  }

  /**
   * Checks partner status and opens dialog if allowed
   * Allows Active and Draft statuses
   */
  private checkAndOpenDialog(status: string | undefined): void {
    const allowedStatuses = ['Active', 'Draft'];
    
    if (status && !allowedStatuses.includes(status)) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('message.partner.mustBeActiveOrDraftToCreateOpportunity'),
        summary: this.translateService.instant('common.error.title')
      });
      return;
    }

    this.showCreateOpportunityDialog.set(true);
  }

  /**
   * Handle successful opportunity creation from unified dialog
   */
  handleOpportunityCreated(opportunity: any): void {
    this.showCreateOpportunityDialog.set(false);
    
    // Refresh the listview
    window.dispatchEvent(new CustomEvent('refresh-listview'));

    // Open the new opportunity in a new tab if we have an ID
    if (opportunity && opportunity.id) {
      const url = this.router.serializeUrl(
        this.router.createUrlTree(
          ['/partnerships/opportunities', opportunity.id],
          { queryParams: { fromCreate: 'true' } }
        )
      );
      window.open(url, '_blank');
    }
  }
}
