import {ChangeDetectionStrategy, Component, inject, signal, ViewChild, WritableSignal, ChangeDetectorRef, OnInit, OnDestroy, computed} from '@angular/core';
import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';

import {Button, ButtonDirective} from 'primeng/button';
import { Router, ActivatedRoute} from '@angular/router';
import { InteractionModalComponent } from '../modal/interaction-modal.component';
import { INTERACTION_TYPE_TRANSLATION_KEYS, InteractionType } from '../../../models/interaction-type.enum';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ListviewComponent } from '../../../../../common/pages/components/listview/listview.component';
import { ListViewColumn, ListViewConfig, SearchParams } from '../../../../../common/pages/components/listview/listview.model';
import { DialogService } from 'primeng/dynamicdialog';
import { PermissionUtilityService } from '../../../../../essentials/services/permission-utility.service';
import { FeedbackDialogService } from '../../../../../common/reusables/services/feedback-dialog.service';
import { SearchField } from '../../../../../common/services/search-parser.service';
import { EntityConfigurationService } from '../../../services/entity-configuration.service';
import { ImportDialogService } from '../../../../../common/reusables/components/import/dialog/import-dialog.service';
import { InteractionIconService } from '../../../../../common/services/interaction-icon.service';
import { InteractionPreviewComponent } from '../preview/interaction-preview.component';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { OverlayPanel } from 'primeng/overlaypanel';

/**
 * @uiEntity InteractionList
 * @route /partnerships/interactions
 * @description Browse and manage all interaction records across the organization. Central hub for viewing meetings, calls, emails, and other communications with comprehensive search and filtering capabilities.
 * @capabilities search_interactions, filter_interactions, create_interaction, edit_interaction, delete_interaction, export_interactions, import_interactions, view_timeline
 * @synonyms communications, meetings, activities, engagements, touchpoints, correspondence
 * @mandatoryFields type, date, subject, contactId
 * @help_when_stuck Use the search bar to find interactions by type, date, or participant. Click + to create new interactions if you have permissions. Use filters to narrow results by interaction type, date range, or participants.
 * @common_tasks
 *   - Finding interactions: Search by date, participant, subject, or interaction type
 *   - Creating interactions: Click 'New Interaction' button (requires INTERACTION_CREATE permission)
 *   - Editing interactions: Click on any interaction row to open details and modify
 *   - Filtering by type: Use interaction type filters to see specific communication types
 *   - Exporting data: Use Export button to download interaction lists for reporting
 *   - Importing interactions: Use Import button to bulk upload interaction data
 */

@Component({
  selector: 'app-interaction-list',
  standalone: true,
  imports: [
    Button,
    TranslateModule,
    ListviewComponent,
    InteractionPreviewComponent,
    OverlayPanelModule,
  ],
  providers: [
    DialogService
  ],
  templateUrl: './interaction-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionListComponent implements OnInit, OnDestroy {
  selectedInteraction: WritableSignal<Interaction | undefined> = signal(undefined);

  @ViewChild("listviewComponent")
  listviewComponent?: ListviewComponent;

  @ViewChild("previewPanel")
  previewPanel?: OverlayPanel;

  previewInteraction = signal<Interaction | null>(null);

  // Inject services
  router = inject(Router);
  route = inject(ActivatedRoute);
  permissionUtilityService = inject(PermissionUtilityService);
  feedbackDialogService = inject(FeedbackDialogService);
  entityConfigurationService = inject(EntityConfigurationService);
  importDialogService = inject(ImportDialogService);
  cdr = inject(ChangeDetectorRef);
  interactionIconService = inject(InteractionIconService);

  // Permission handling
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Interaction');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  // Dynamic interaction columns loaded from API
  columns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(true);

  // Configure listview behavior with computed permissions
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
    sortableFields: [
      { field: 'createdBy', label: 'Created By' },
      { field: 'lastModifiedBy', label: 'Last Updated By' }
    ],
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: 'Search interactions...',
      searchableFields: [
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
        },
        {
          field: 'partner.name',
          label: 'Partner',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        {
          field: 'createdDate',
          label: 'Created Date',
          type: 'date',
          operators: ['after', 'before', 'between']
        },
        {
          field: 'lastModifiedDate',
          label: 'Last Modified Date',
          type: 'date',
          operators: ['after', 'before', 'between']
        }
      ] as SearchField[]
    }
  }));

  // Track current search term
  currentSearchText = '';

  private dialogService = inject(DialogService);
  private translateService = inject(TranslateService);

  constructor(
    private interactionService: InteractionService,
  ) {
    this.openModalFromRoute();
    this.setInteractionFromHistoryState();
  }

  ngOnInit() {


    // Load permissions using utility service
    this.permissionUtils.loadPermissions(this.router, this.cdr);

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
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Failed to load interaction columns:', error);
          // Fallback to default columns if API fails
          this.setFallbackColumns();
          this.columnsLoading.set(false);
          this.cdr.detectChanges();
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
    // Check for both camelCase (templatePattern) and PascalCase (TemplatePattern) from backend
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

  ngOnDestroy() {
    // No need to clear caches manually - utility service handles this
  }

  private setInteractionFromHistoryState() {
    this.route.queryParams.subscribe(params => {
      if (params['openNewDialog'] === 'true') {
        const state = history.state;
        if (state?.data) {
          this.selectedInteraction.set(state.data);
          this.openInteractionModal(state.data);
        }
      }
    });
  }

  openModalFromRoute(): void {
    this.route.params.subscribe(params => {
      const interactionId = params['id'];
      if (interactionId) {
        this.interactionService.getById(interactionId).subscribe(
          (response) => {
            if (response.body) {
              this.openEditInteractionModal(response.body);
            }
          }
        );
      }
    });
  }

  /**
   * @uiButton create_interaction
   * @description Opens the interaction creation modal to record new meetings, calls, emails, or other communications
   * @label New Interaction
   * @icon pi pi-plus
   * @when_to_use When you want to record a new communication, meeting, or activity with partners or contacts
   * @permissions INTERACTION_CREATE
   */
  openNewInteractionModal(): void {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to create interactions',
        summary: 'Permission Denied'
      });
      return;
    }

    this.openInteractionModal();
  }

  handleOnOpenRecordDetails(record: any) {
    if (record && record.id !== undefined && record.id !== null) {
      console.log('Navigating to interaction:', record.id);
      this.router.navigate(['partnerships/interactions', record.id.toString()]);
    } else {
      console.error('Cannot navigate: record or record.id is undefined', record);
    }
  }

  handleOnRecordDelete(record: Interaction) {
    // Check if user has delete permission
    if (!this.permissionUtilityService.canDelete(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to delete interactions',
        summary: 'Permission Denied'
      });
      return;
    }

    this.interactionService.delete(record.id!).subscribe({
      next: () => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Interaction deleted successfully!' });
        // Refresh the listview
        const listviewElement = document.querySelector('app-listview');
        if (listviewElement) {
          listviewElement.dispatchEvent(new CustomEvent('refresh-listview'));
        }
      },
      error: (error: any) => {
        this.feedbackDialogService.showErrorToast({
          detail: 'Failed to delete interaction',
          summary: 'Error'
        });
        console.error('Error deleting interaction:', error);
      }
    });
  }

  _handleOnRecordCreation(newRecordData: Interaction) {
    if (newRecordData && newRecordData.id !== undefined && newRecordData.id !== null) {
      console.log('Navigating to newly created interaction:', newRecordData.id);
      this.router.navigate(['partnerships/interactions', newRecordData.id.toString()]);
    } else {
      console.error('Cannot navigate to created record: id is undefined', newRecordData);
    }
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
        // Refresh the listview
        const listviewElement = document.querySelector('app-listview');
        if (listviewElement) {
          listviewElement.dispatchEvent(new CustomEvent('refresh-listview'));
        }
      }
    });
  }

  private openInteractionModal(record?: Interaction): void {
    const ref = this.dialogService.open(InteractionModalComponent, {
      header: record ? 'Edit Interaction' : 'New Interaction',
      width: '90%',
      height: '90%',
      modal: true,
      closable: true,
      data: {
        id: record?.id,
        initialData: record || {}
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        console.log('Interaction saved:', result);
        if (record) {
          // Update existing interaction
          const listviewElement = document.querySelector('app-listview');
          if (listviewElement) {
            listviewElement.dispatchEvent(new CustomEvent('refresh-listview'));
          }
        } else {
          // New interaction created
          this._handleOnRecordCreation(result);
        }
      }
    });
  }

  onSearchChange(searchParams: SearchParams) {
    this.currentSearchText = searchParams.generalSearch || '';
  }

  /**
   * @uiButton import_interactions
   * @description Opens the import dialog to bulk import interaction records from Google Sheets or CSV files
   * @label Import Interactions
   * @icon pi pi-file-import
   * @when_to_use When you need to add multiple interaction records at once from external sources or data migration
   * @permissions INTERACTION_CREATE
   */
  openImportDialog() {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to import interactions',
        summary: 'Permission Denied'
      });
      return;
    }
    
    // Use the Google Sheet picker directly which will show loading indicators
    this.importDialogService.openGoogleSheetPicker('interaction');
  }

  showInteractionPreview(event: MouseEvent, interaction: Interaction) {
    this.previewInteraction.set(interaction);
    this.previewPanel?.show(event, event.target as HTMLElement);
  }

  hideInteractionPreview() {
    this.previewPanel?.hide();
    this.previewInteraction.set(null);
  }
}
