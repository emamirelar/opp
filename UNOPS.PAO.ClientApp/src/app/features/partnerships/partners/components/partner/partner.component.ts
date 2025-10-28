import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal, computed, ViewChild } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '@shared/services/utils';
import { Subscription } from 'rxjs';
import { PartnerService } from '../../services/partner.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { PartnerNewComponent } from './new/partner-new.component';
import { Partner } from '../../models/partner.model';
import { ListviewComponent } from '@features/list-view/components/listview/listview.component';
import { ListViewColumn, ListViewConfig, SearchParams } from '@features/list-view/components/listview/listview.model';
import { PartnerEditDialogFooterComponent } from './edit-dialog/footer/partner-edit-dialog-footer.component';
import { PartnerEditDialogComponent } from './edit-dialog/partner-edit-dialog.component';
import { DialogService } from 'primeng/dynamicdialog';
import { ImportDialogService } from '@features/import-export/components/import/dialog/import-dialog.service';
import { SearchField } from '@shared/services/utils';
import { PermissionUtilityService } from '@core/services/auth';
import { EntityPermissions } from '@core/services/auth';
import { EntityConfigurationService } from '@shared/services/api/entity-configuration.service';
import { CachedDataService } from '@shared/services/utils';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { PageContextService } from '@shared/services/utils';


/**
 * @uiEntity Partner
 * @route /partnerships/partners
 * @description Browse and manage partner organizations with comprehensive search, filtering, and CRUD operations. Central hub for all partner-related activities.
 * @capabilities search_partners, filter_partners, create_partner, edit_partner, delete_partner, export_partners, import_partners, bulk_operations
 * @synonyms organization, collaborator, entity, associate, vendor, supplier, contractor
 * @mandatoryFields name, partnerType, status, partnerOfficeId
 * @help_when_stuck Use the search bar to find specific partners by name, type, or location. Click the + button to create new partners if you have permissions. Use filters to narrow down results by partner type, status, or organizational unit.
 * @common_tasks
 *   - Finding a partner: Use the global search bar or entity-specific filters
 *   - Creating a partner: Click 'Create Partner' button (requires PARTNER_CREATE permission)
 *   - Editing a partner: Click on any partner row to open details, then click Edit
 *   - Filtering partners: Use the advanced search and filter options in the left panel
 *   - Exporting data: Use the Export button to download partner lists in Excel format
 *   - Importing partners: Use the Import button to bulk upload partner data
 * @tabs Details:/partnerships/partners/:id, Contacts:/partnerships/partners/:id/contacts, Interactions:/partnerships/partners/:id/interactions, Data:/partnerships/partners/:id/data
 */
@Component({
  selector: 'app-partner',
  templateUrl: './partner.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    ButtonModule,
    PartnerNewComponent,
    TranslateModule,
    ListviewComponent,
    MenuModule,
  ],
  providers: [DialogService]
})
export class PartnerComponent implements OnDestroy, OnInit {
  private langChangeSubscription: Subscription = new Subscription;
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  partnerService = inject(PartnerService);
  feedbackDialogService = inject(FeedbackDialogService);
  dialogService = inject(DialogService);
  importDialogService = inject(ImportDialogService);
  permissionUtilityService = inject(PermissionUtilityService);
  entityConfigurationService = inject(EntityConfigurationService);
  cachedDataService = inject(CachedDataService);
  translateService = inject(TranslateService);
  private pageContextService = inject(PageContextService);

  newPartnerData = signal<Partner|null>(null);

  // Permission management using utility service
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Partner');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  // Reference to listview component for export functionality
  @ViewChild(ListviewComponent) listviewComponent!: ListviewComponent;

  // Dynamic partner columns loaded from API
  columns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(true);

  // Computed listview configuration that respects permissions
  listviewConfig = computed<ListViewConfig>(() => ({
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enablePagination: true,
    enableSorting: true,
    enableSearch: true,
    enableExport: this.entityPermissions().permissions.canCreate || this.entityPermissions().permissions.canUpdate,
    scrollable: true,
    scrollHeight: 'flex',
    entityName: 'Partner',
    defaultSortField: 'name',
    defaultSortOrder: 'asc',
    sortableFields: [
      { field: 'name', label: this.translateService.instant('label.partner.name') },
      { field: 'createdDate', label: this.translateService.instant('label.column.createdDate') },
      { field: 'lastModifiedDate', label: this.translateService.instant('label.column.lastUpdatedDate') }
    ],
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: this.translateService.instant('placeholder.searchPartners'),
      searchableFields: [
        { 
          field: 'name', 
          label: 'label.partner.name', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'partnerShortDescription', 
          label: 'label.partner.partnerShortDescription', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'partnerLongDescription', 
          label: 'label.partner.partnerLongDescription', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'status', 
          label: 'label.partner.status', 
          type: 'string',
          operators: ['is', 'is not']
        },
        { 
          field: 'partnerGroupId', 
          label: 'label.partner.partnerGroup', 
          type: 'number',
          operators: ['is', 'is not']
        },
        { 
          field: 'partnerCategoryId', 
          label: 'label.partner.partnerCategory', 
          type: 'number',
          operators: ['is', 'is not', '>', '<', '>=', '<=']
        },
        { 
          field: 'liaisonOfficeId', 
          label: 'label.partner.liaisonOffice', 
          type: 'number',
          operators: ['is', 'is not', '>', '<', '>=', '<=']
        },
        { 
          field: 'keyGlobalPartner', 
          label: 'label.partner.keyGlobalPartner', 
          type: 'string',
          operators: ['is', 'is not']
        },
        { 
          field: 'unSecretariatPartner', 
          label: 'label.partner.unSecretariatPartner', 
          type: 'string',
          operators: ['is', 'is not']
        },
        { 
          field: 'partnerApprovalStatus', 
          label: 'label.partner.partnerApprovalStatus', 
          type: 'string',
          operators: ['is', 'is not']
        },
        { 
          field: 'pooledFund', 
          label: 'label.partner.pooledFund', 
          type: 'string',
          operators: ['is', 'is not']
        },
        { 
          field: 'canCreateNewOpportunities', 
          label: 'label.partner.canCreateNewOpportunities', 
          type: 'string',
          operators: ['is', 'is not']
        },
        { 
          field: 'createdDate', 
          label: 'label.audit.createdDate', 
          type: 'date',
          operators: ['after', 'before', 'between']
        },
        { 
          field: 'lastModifiedDate', 
          label: 'label.audit.lastModifiedDate', 
          type: 'date',
          operators: ['after', 'before', 'between']
        }
      ] as SearchField[]
    },
    // Enable search metadata display
    searchMetadata: {
      enabled: true,
      defaultVisible: false, // Hidden by default, user can toggle
      searchQuery: '', // Will be populated automatically
      extractMetadata: (item: any) => {
        // Extract search metadata from the item
        return item._searchMetadata || null;
      }
    }
  }));

  constructor(private languageService: LanguageService, private cdr: ChangeDetectorRef) {
    this.setNewPartnerFromAIAssistant();
    
    // Subscribe to language changes to update dynamic translations
    this.langChangeSubscription = this.translateService.onLangChange.subscribe(() => {
      this.updateDynamicTranslations();
    });
  }

  ngOnInit() {
    // Register component data for AI Assistant
    this.pageContextService.setComponentData(this);
    
    
    // Load permissions using utility service
    this.permissionUtils.loadPermissions(this.router, this.cdr);
    
    // Load dynamic columns from API
    this.loadPartnerColumns();
    
    // Listen for refresh events (e.g., from imports) to refresh partner cache
    window.addEventListener('refresh-listview', this.refreshPartnerCacheHandler);
    
    this.activatedRoute.queryParams
      .subscribe(params => {
        if (params['openNewDialog'] === 'true') {
          const state = history.state;
          const emptyPartner: Partner = {};
          this.openPartnerEditDialog(state?.data || emptyPartner);
        }
      });
  }

  private setNewPartnerFromAIAssistant() {
    this.activatedRoute.queryParams.subscribe(params => {
      if (params['openNewDialog'] === 'true') {
        const state = history.state;
        if (state?.data) {
          this.newPartnerData.set(state.data);
        }
        this.removeOpenNewDialogFromUrl();
      }
    });
  }

  private removeOpenNewDialogFromUrl() {
    this.router.navigate([], {
      relativeTo: this.activatedRoute,
      queryParams: {openNewDialog: null},
      queryParamsHandling: 'merge'
    });
  }

  handleOnOpenRecordDetails(record: any) {
    if (record && record.id !== undefined && record.id !== null) {
      
      this.router.navigate(['partnerships/partners', record.id.toString()]);
    } else {
      console.error(this.translateService.instant('error.cannotNavigateRecordUndefined'), record);
    }
  }

  handleOnRecordDelete(record: any) {
    this.partnerService.deletePartnerById(record.id).subscribe({
      next: () => {
        this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('message.partnerDeleteSuccess') });
        const listviewElement = document.querySelector('app-listview');
        if (listviewElement) {
          listviewElement.dispatchEvent(new CustomEvent('refresh-listview'));
        }
      },
      error: (error: any) => {
        this.feedbackDialogService.showErrorToast({
          detail: this.translateService.instant('message.failedToDeleteRecord'),
          summary: error.message || this.translateService.instant('message.anErrorOccurred')
        });
      }
    });
  }

  // Handler for refresh-listview events to refresh partner cache
  private refreshPartnerCacheHandler = () => {
    this.cachedDataService.refreshPartners();
  };

  private updateDynamicTranslations() {
    // Trigger change detection to update computed values that use translations
    this.cdr.detectChanges();
  }

  ngOnDestroy(): void {
    // Clear component data for AI Assistant
    this.pageContextService.clearComponentData();
    
    this.langChangeSubscription?.unsubscribe();
    // Clean up event listener
    window.removeEventListener('refresh-listview', this.refreshPartnerCacheHandler);
  }

  _handleOnRecordCreation(newRecordData: any) {
    if (newRecordData && newRecordData.id !== undefined && newRecordData.id !== null) {
      // Refresh partner cache to include the newly created partner
      this.cachedDataService.refreshPartners();
      
      // Trigger refresh of the list view to show the new partner
      window.dispatchEvent(new CustomEvent('refresh-listview'));
      
      this.router.navigate(['partnerships/partners', newRecordData.id.toString()]);
    } else {
      console.error(this.translateService.instant('error.cannotNavigateCreatedRecordUndefined'), newRecordData);
    }
  }

  /**
   * @uiButton create_partner,edit_partner
   * @description Opens the partner creation or editing dialog with comprehensive form fields for managing partner organization information
   * @label New Partner | Edit Partner
   * @icon pi pi-plus | pi pi-pencil
   * @when_to_use When creating a new partner organization or editing existing partner details, including organizational information, contacts, and business relationships
   * @permissions PARTNER_CREATE, PARTNER_UPDATE
   */
  openPartnerEditDialog(partnerData: Partner = {}) {
    const ref = this.dialogService.open(PartnerEditDialogComponent, {
      header: partnerData.id ? 
        this.translateService.instant('dialog.header.editPartner') : 
        this.translateService.instant('dialog.header.newPartner'),
      width: '40vw',
      breakpoints: { '960px': '95vw' },
      closable: true,
      templates: {
        footer: PartnerEditDialogFooterComponent
      },
      data: {
        mode: partnerData.id ? 'edit' : 'new',
        record: partnerData,
        requestingSaveSignal: signal<boolean>(false)
      }
    });

    const refSub = ref.onClose.subscribe((result: any) => {
      if (result) {
        this._handleOnRecordCreation(result);
      }
      refSub.unsubscribe();
    });
  }

  // Import menu items - computed to support language changes
  importMenuItems = computed<MenuItem[]>(() => [
    {
      label: this.translateService.instant('importMenu.selectFromGoogleDrive'),
      icon: 'pi pi-google',
      command: () => this.openGooglePickerImport(),
      title: this.translateService.instant('importMenu.googleDriveTooltip')
    },
    {
      label: this.translateService.instant('importMenu.manualEntry'),
      icon: 'pi pi-link',
      command: () => this.openManualEntryImport(),
      title: this.translateService.instant('importMenu.manualEntryTooltip')
    }
  ]);

  /**
   * @uiButton import_partners
   * @description Opens the import dialog to bulk import partner organizations from Google Sheets or CSV files
   * @label Import Partners
   * @icon pi pi-file-import
   * @when_to_use When you need to add multiple partner organizations at once from external sources, ideal for bulk data migration or initial system setup
   * @permissions PARTNER_CREATE
   */
  openImportDialog() {
    // This method now shows the import menu instead of directly opening the picker
    // The actual menu is handled in the template via p-menu
  }

  /**
   * Open Google Picker for import (original flow)
   */
  openGooglePickerImport() {
    // Use the Google Sheet picker directly which will show loading indicators
    this.importDialogService.openGoogleSheetPicker('partner');
  }

  /**
   * Open manual entry dialog for import
   */
  openManualEntryImport() {
    this.importDialogService.openManualEntryDialog('partner');
  }

  /**
   * @uiButton export_partners
   * @description Exports partner data to Google Sheets respecting current search and filter criteria
   * @label Export Partners
   * @icon pi pi-file-export
   * @when_to_use When you need to export partner data with current filters applied for external analysis or reporting
   * @permissions PARTNER_GLOB_ADMIN
   */
  exportData() {
    if (this.listviewComponent) {
      this.listviewComponent.exportData();
    }
  }

  onSearchChange(searchParams: SearchParams) {
    // Handle search parameters if needed
  }

  private loadPartnerColumns() {
    this.columnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('Partner')
      .subscribe({
        next: (columns) => {
          // Convert backend columns to frontend format and add template functions
          const processedColumns = columns.map(col => this.processColumn(col));
          
          // Always add tags column if not already present
          const hasTagsColumn = processedColumns.some(col => col.field === 'tags');
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
          
          this.columns.set(processedColumns);
          this.columnsLoading.set(false);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Failed to load partner columns:', error);
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

    // Handle nested field paths (fields with dots) by adding a template function
    if (column.field && column.field.includes('.') && column.type !== 'template') {
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
      processedColumn.templateFn = this.createTemplateFunction(column.templatePattern);
    }

    // Add special handling for multiple-avatars
    if (column.type === 'multiple-avatars') {
      // Use the configured fallback field from the API
      processedColumn.firstLetterFallbackField = column.firstLetterFallbackField || 'first5ContactsByDate.firstName';
    }

    // Add special handling for tags field
    if (column.field === 'tags' || column.type === 'tags') {
      processedColumn.type = 'template';
      processedColumn.templateFn = (rowData: any) => {
        const tags = rowData?.tags;
        if (!tags || !Array.isArray(tags) || tags.length === 0) {
          return '';
        }
        
        return tags.map((tag: any) => 
          `<span class="px-2 py-1 text-xs rounded-full ${tag.color} whitespace-nowrap">${tag.tag}</span>`
        ).join(' ');
      };
    }

    return processedColumn;
  }

  private createTemplateFunction(templatePattern: string): (rowData: any) => string {
    return (rowData: any) => {
      let result = templatePattern;
      
      // Replace field placeholders like {name}, {shortName} with actual values
      const fieldMatches = templatePattern.match(/\{([^}]+)\}/g);
      if (fieldMatches) {
        fieldMatches.forEach(match => {
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
    // Fallback to original hardcoded columns if API fails
    const fallbackColumns: ListViewColumn[] = [
      {
        field: 'partnerCategoryName',
        label: 'label.partnerTree.partnerCategory',
        sortable: false,
        type: 'text',
        width: '15%',
        ellipsis: true
      },
      {
        field: 'partnerGroupName',
        label: 'label.partnerTree.partnerGroup',
        sortable: false,
        type: 'text',
        width: '15%',
        ellipsis: true
      },
      {
        field: 'logoUrl',
        label: '',
        sortable: false,
        type: 'avatar'
      },
      {
        field: 'name',
        label: 'label.partner.name',
        sortable: false,
        type: 'text'
      },
      { 
        field: 'organizationUnitName',
        label: 'label.partner.organizationUnit',
        sortable: false,
        width: '20%',
        type: 'template',
        ellipsis: true,
        templateFn: (partner: any) => {
          const primaryOrgUnit = partner.getPrimaryOrganizationUnit?.() || 
                                (partner.organizationUnitRelationships && partner.organizationUnitRelationships.length > 0 
                                 ? partner.organizationUnitRelationships[0].organizationHierarchy 
                                 : null);
          return primaryOrgUnit?.name || '';
        }
      },
      {
        field: 'first5ContactsByDate.profilePictureUrl',
        firstLetterFallbackField: 'first5ContactsByDate.firstName',
        label: 'label.partner.partnerTeam',
        sortable: false,
        type: 'multiple-avatars',
        width: '10%',
      },
      {
        field: 'tags',
        label: this.translateService.instant('label.column.status'),
        sortable: false,
        type: 'template',
        width: '15%',
        templateFn: (rowData: any) => {
          const tags = rowData?.tags;
          if (!tags || !Array.isArray(tags) || tags.length === 0) {
            return '';
          }
          
          return tags.map((tag: any) => 
            `<span class="px-2 py-1 text-xs rounded-full ${tag.color} whitespace-nowrap">${tag.tag}</span>`
          ).join(' ');
        }
      }
    ];
    
    this.columns.set(fallbackColumns);
  }
}
