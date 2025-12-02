import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  OnDestroy,
  OnInit,
  signal,
  computed,
  ViewChild,
  DestroyRef,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NgIf } from '@angular/common';

import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { FloatLabelModule } from 'primeng/floatlabel';
import { MessageModule } from 'primeng/message';
import { FormsModule } from '@angular/forms';

import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ListviewComponent } from '@features/list-view/components/listview/listview.component';
import { OpportunityService } from '../../../services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui';
import {
  ListViewColumn,
  ListViewConfig,
  SearchParams,
} from '@features/list-view/components/listview/listview.model';
import { Opportunity } from '@shared/models/opportunity.model';
import { SearchField } from '@shared/services/utils';
import { PermissionUtilityService } from '@core/services/auth';
import { EntityConfigurationService } from '@shared/services/api/entity-configuration.service';
import { PageContextService } from '@shared/services/utils';
import { CreateOpportunityFromInteractionsDialogComponent } from '@partnerships/interactions/components/dialogs/create-opportunity-from-interactions-dialog.component';
import { CreateOpportunityFromInteractionsConfig } from '@partnerships/interactions/models/interaction-selection.model';

/**
 * @uiEntity Opportunity
 * @route /partnerships/opportunities
 * @description Browse and manage funding and partnership opportunities. Central hub for tracking opportunities from identification to delivery.
 * @capabilities search_opportunities, filter_opportunities, create_opportunity, edit_opportunity, delete_opportunity, export_opportunities
 * @synonyms funding_opportunity, partnership_opportunity, project_opportunity, initiative
 * @mandatoryFields name, description
 * @help_when_stuck Use the search bar to find opportunities by name or description. Click + to add new opportunities if you have permissions. Use filters to narrow results by workflow stage or budget.
 * @common_tasks
 *   - Finding an opportunity: Search by name, description, or workflow stage
 *   - Creating an opportunity: Click 'New' button (requires OPPORTUNITY_CREATE permission)
 *   - Editing an opportunity: Click on any opportunity row to open details, then click Edit
 *   - Filtering by stage: Use the workflow stage filter to see opportunities at specific stages
 *   - Exporting opportunities: Use Export button to download opportunity lists with details
 * @tabs Details:/partnerships/opportunities/:id
 */
@Component({
  selector: 'app-opportunity-list',
  templateUrl: './opportunity-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    PanelModule,
    ButtonModule,
    ListviewComponent,
    ConfirmDialog,
    NgIf,
    TranslateModule,
    RouterModule,
    CreateOpportunityFromInteractionsDialogComponent
  ],
  providers: [ConfirmationService],
})
export class OpportunityListComponent implements OnInit, OnDestroy {
  router = inject(Router);
  route = inject(ActivatedRoute);
  opportunityService = inject(OpportunityService);
  feedbackDialogService = inject(FeedbackDialogService);
  permissionUtilityService = inject(PermissionUtilityService);
  entityConfigurationService = inject(EntityConfigurationService);
  translateService = inject(TranslateService);
  cdr = inject(ChangeDetectorRef);
  private destroyRef = inject(DestroyRef);
  private pageContextService = inject(PageContextService);

  // Permission management using utility service
  private permissionUtils =
    this.permissionUtilityService.createEntityPermissions('Opportunity');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  // Reference to listview component for export functionality
  @ViewChild(ListviewComponent) listviewComponent!: ListviewComponent;

  // Dynamic opportunity columns loaded from API
  opportunityColumns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(true);

  // Configure listview behavior with computed permissions
  listviewConfig = computed<ListViewConfig>(() => ({
    enableSelection: true,
    enablePagination: true,
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enableSorting: true,
    enableSearch: true,
    enableExport:
      this.entityPermissions().permissions.canCreate ||
      this.entityPermissions().permissions.canUpdate,
    entityName: 'Opportunity',
    scrollable: true,
    scrollHeight: 'flex',
    defaultSortField: 'lastModifiedDate',
    defaultSortOrder: 'desc',
    sortableFields: [
      { field: 'name', label: 'Name' },
      { field: 'createdDate', label: 'Created Date' },
      { field: 'lastModifiedDate', label: 'Last Updated Date' },
    ],
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: 'search.opportunitiesPlaceholder',
      searchableFields: [
        {
          field: 'name',
          label: 'label.opportunity.name',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like'],
        },
        {
          field: 'description',
          label: 'label.opportunity.description',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like'],
        },
        {
          field: 'status',
          label: 'label.opportunity.status',
          type: 'string',
          operators: ['is', 'is not'],
        },
        {
          field: 'partnerReference',
          label: 'label.opportunity.partnerReference',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like'],
        },
        {
          field: 'workflowStage.name',
          label: 'label.opportunity.workflowStage',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like'],
        },
        {
          field: 'initiativeBudgetUSD',
          label: 'label.opportunity.budgetUSD',
          type: 'number',
          operators: [
            'equals',
            'not equals',
            'greater than',
            'less than',
            'between',
          ],
        },
        {
          field: 'targetSigningDate',
          label: 'label.opportunity.targetSigningDate',
          type: 'date',
          operators: ['after', 'before', 'between'],
        },
        {
          field: 'createdDate',
          label: 'Created Date',
          type: 'date',
          operators: ['after', 'before', 'between'],
        },
        {
          field: 'lastModifiedDate',
          label: 'Last Modified Date',
          type: 'date',
          operators: ['after', 'before', 'between'],
        },
      ] as SearchField[],
    },
    // Enable search metadata display
    searchMetadata: {
      enabled: true,
      defaultVisible: false,
      searchQuery: '',
      extractMetadata: (item: any) => {
        return item._searchMetadata || null;
      },
    },
  }));

  // Track current search term
  currentSearchText = '';

  // Unified opportunity creation dialog
  showCreateDialog = signal(false);
  
  // Dialog configuration for unified dialog
  dialogConfig = computed<CreateOpportunityFromInteractionsConfig>(() => {
    return {
      partnerId: 0, // No specific partner - user can select
      partnerName: '',
      mode: 'list-view', // From opportunity list
      preSelectedInteractionIds: [] // No interactions pre-selected
    };
  });

  ngOnInit() {
    // Register component data for AI Assistant
    this.pageContextService.setComponentData(this);

    // Load permissions using utility service
    this.permissionUtils.loadPermissions(this.router, this.cdr);

    // Load dynamic columns from API
    this.loadOpportunityColumns();
  }

  private loadOpportunityColumns() {
    this.columnsLoading.set(true);
    this.entityConfigurationService
      .getEntityListViewConfiguration('Opportunity')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (columns: any) => {
          // Convert backend columns to frontend format and add template functions
          const processedColumns = columns.map((col: any) =>
            this.processColumn(col),
          );
          
          // Always add tags column if not already present
          const hasTagsColumn = processedColumns.some((col: ListViewColumn) => col.field === 'tags');
          if (!hasTagsColumn) {
            processedColumns.push({
              field: 'tags',
              label: this.translateService.instant('label.column.status'),
              sortable: false,
              type: 'template',
              width: '15%',
              templateFn: (rowData: any) => {
                if (!rowData?.tags || !Array.isArray(rowData.tags) || rowData.tags.length === 0) {
                  return '';
                }
                
                return rowData.tags.map((tag: any) => 
                  `<span class="px-2 py-1 text-xs rounded-full ${tag.color} whitespace-nowrap">${tag.tag}</span>`
                ).join(' ');
              }
            });
          }
          
          this.opportunityColumns.set(processedColumns);
          this.columnsLoading.set(false);
          this.cdr.detectChanges();
        },
        error: (error: any) => {
          console.error('Failed to load opportunity columns:', error);
          // Fallback to default columns if API fails
          this.setFallbackColumns();
          this.columnsLoading.set(false);
          this.cdr.detectChanges();
        },
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
    if (
      column.field &&
      column.field.includes('.') &&
      column.type !== 'template'
    ) {
      // Keep the original field for identification but add a template function to access nested data
      processedColumn.templateFn = (rowData: any) => {
        const value = this.getNestedProperty(rowData, column.field);
        return value !== undefined && value !== null ? String(value) : '';
      };
      // Change type to template since we're now using a template function
      processedColumn.type = 'template';
    }

    // Add template function for template type columns
    if (column.type === 'template' && column.templatePattern) {
      processedColumn.templateFn = this.createTemplateFunction(
        column.templatePattern,
      );
    }

    return processedColumn;
  }

  private createTemplateFunction(
    templatePattern: string,
  ): (rowData: any) => string {
    return (rowData: any) => {
      let result = templatePattern;

      // Replace field placeholders like {name}, {description} with actual values
      const fieldMatches = templatePattern.match(/\{([^}]+)\}/g);
      if (fieldMatches) {
        fieldMatches.forEach((match) => {
          const fieldName = match.replace(/[{}]/g, '');
          const fieldValue = this.getNestedProperty(rowData, fieldName) || '';
          result = result.replace(match, fieldValue);
        });
      }

      return result.trim();
    };
  }

  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((o, p) => o?.[p], obj);
  }

  private setFallbackColumns() {
    // Fallback to default columns if API fails
    const fallbackColumns: ListViewColumn[] = [
      {
        field: 'name',
        label: 'label.opportunity.name',
        type: 'text',
        sortable: true,
        width: '25%',
        ellipsis: true,
      },
      {
        field: 'description',
        label: 'label.opportunity.description',
        type: 'text',
        sortable: false,
        width: '30%',
        ellipsis: true,
      },
      {
        field: 'status',
        label: 'label.opportunity.status',
        type: 'badge',
        sortable: true,
        width: '10%',
      },
      {
        field: 'workflowStageName',
        label: 'label.opportunity.workflowStage',
        type: 'text',
        sortable: false,
        width: '15%',
        ellipsis: true,
      },
      {
        field: 'initiativeBudgetUSD',
        label: 'label.opportunity.budgetUSD',
        type: 'currency',
        sortable: true,
        width: '12%',
      },
    ];

    this.opportunityColumns.set(fallbackColumns);
  }

  ngOnDestroy() {
    // Clear component data for AI Assistant
    this.pageContextService.clearComponentData();
  }

  handleOnOpenRecordDetails(record: any) {
    if (record?.data) {
      record = record.data;
    }
    if (record && record.id !== undefined && record.id !== null) {
      this.router.navigate([
        'partnerships/opportunities',
        record.id.toString(),
      ]);
    } else {
      console.error(
        'Cannot navigate: record or record.id is undefined',
        record,
      );
    }
  }

  handleOnRecordDelete(record: Opportunity) {
    // Check if user has delete permission
    if (!this.permissionUtilityService.canDelete(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'message.noPermissionToDelete',
        summary: 'message.permissionDenied',
      });
      return;
    }

    if (!record.id) {
      console.error('Cannot delete opportunity: ID is missing');
      return;
    }

    this.opportunityService
      .deleteOpportunityById(record.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.feedbackDialogService.showSuccessToast({
            detail: 'message.recordDeletedSuccessfully',
          });
          // Trigger a refresh for the listview
          window.dispatchEvent(new CustomEvent('refresh-listview'));
        },
        error: (error: any) => {
          this.feedbackDialogService.showErrorToast({
            detail: 'message.failedToDeleteRecord',
            summary: error.message || 'message.anErrorOccurred',
          });
        },
      });
  }

  /**
   * @uiButton create_opportunity
   * @description Opens the opportunity creation dialog with form fields for managing opportunity information
   * @label New Opportunity
   * @icon pi pi-plus
   * @when_to_use When creating a new funding or partnership opportunity
   * @permissions OPPORTUNITY_CREATE
   */
  openOpportunityEditDialog() {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'message.noPermissionToCreate',
        summary: 'message.permissionDenied',
      });
      return;
    }

    this.showCreateDialog.set(true);
  }

  /**
   * Handle successful opportunity creation from unified dialog
   */
  handleOpportunityCreated(opportunity: any): void {
    this.showCreateDialog.set(false);
    
    // Refresh the listview
    window.dispatchEvent(new CustomEvent('refresh-listview'));
    
    // Navigate to the new opportunity detail page
    if (opportunity && opportunity.id) {
      this.router.navigate(['/partnerships/opportunities', opportunity.id]);
    }
  }

  onRowClick(opportunity: Opportunity) {
    this.handleOnOpenRecordDetails(opportunity);
  }

  /**
   * @uiButton export_opportunities
   * @description Exports opportunity data to Google Sheets respecting current search and filter criteria
   * @label Export Opportunities
   * @icon pi pi-file-export
   * @when_to_use When you need to export opportunity data with current filters applied for external analysis or reporting
   * @permissions PARTNER_GLOB_ADMIN
   */
  exportData() {
    if (this.listviewComponent) {
      this.listviewComponent.exportData();
    }
  }

  /**
   * Store the current search text when search is performed
   * @param searchParams Current search parameters
   */
  onSearchChange(searchParams: SearchParams) {
    this.currentSearchText = searchParams.generalSearch || '';
  }
}
