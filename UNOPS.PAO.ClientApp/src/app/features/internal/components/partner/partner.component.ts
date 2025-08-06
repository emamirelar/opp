import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal, computed } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../../common/services/language.service';
import { Subscription } from 'rxjs';
import { PartnerService } from '../../services/partner.service';
import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';
import { PartnerNewComponent } from './new/partner-new.component';
import { Partner } from '../../models/partner.model';
import { ListviewComponent } from '../../../../common/pages/components/listview/listview.component';
import { ListViewColumn, ListViewConfig, SearchParams } from '../../../../common/pages/components/listview/listview.model';
import { PartnerEditDialogFooterComponent } from './edit-dialog/footer/partner-edit-dialog-footer.component';
import { PartnerEditDialogComponent } from './edit-dialog/partner-edit-dialog.component';
import { DialogService } from 'primeng/dynamicdialog';
import { ImportDialogService } from '../../../../common/reusables/components/import/dialog/import-dialog.service';
import { SearchField } from '../../../../common/services/search-parser.service';
import { PermissionUtilityService } from '../../../../essentials/services/permission-utility.service';
import { EntityPermissions } from '../../../../essentials/services/permission.service';
import { EntityConfigurationService } from '../../services/entity-configuration.service';

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

  newPartnerData = signal<Partner|null>(null);

  // Permission management using utility service
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Partner');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

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
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: 'Search partners...',
      searchableFields: [
        { 
          field: 'name', 
          label: 'Name', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'shortName', 
          label: 'Short Name', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'status', 
          label: 'Status', 
          type: 'string',
          operators: ['is', 'is not']
        },
        { 
          field: 'website', 
          label: 'Website', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'street', 
          label: 'Street', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'city', 
          label: 'City', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'country', 
          label: 'Country', 
          type: 'string',
          operators: ['is', 'is not']
        }
      ] as SearchField[]
    }
  }));

  constructor(private languageService: LanguageService, private cdr: ChangeDetectorRef) {
    
    
    this.setNewPartnerFromAIAssistant();
  }

  ngOnInit() {
    
    
    
    // Load permissions using utility service
    this.permissionUtils.loadPermissions(this.router, this.cdr);
    
    // Load dynamic columns from API
    this.loadPartnerColumns();
    
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
      console.error('Cannot navigate: record or record.id is undefined', record);
    }
  }

  handleOnRecordDelete(record: any) {
    this.partnerService.deletePartnerById(record.id).subscribe({
      next: () => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Record deleted successfully!' });
        const listviewElement = document.querySelector('app-listview');
        if (listviewElement) {
          listviewElement.dispatchEvent(new CustomEvent('refresh-listview'));
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  _handleOnRecordCreation(newRecordData: any) {
    if (newRecordData && newRecordData.id !== undefined && newRecordData.id !== null) {
      
      this.router.navigate(['partnerships/partners', newRecordData.id.toString()]);
    } else {
      console.error('Cannot navigate to created record: id is undefined', newRecordData);
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
      header: partnerData.id ? 'Edit Partner' : 'New Partner',
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

  /**
   * @uiButton import_partners
   * @description Opens the import dialog to bulk import partner organizations from Google Sheets or CSV files
   * @label Import Partners
   * @icon pi pi-file-import
   * @when_to_use When you need to add multiple partner organizations at once from external sources, ideal for bulk data migration or initial system setup
   * @permissions PARTNER_CREATE
   */
  openImportDialog() {
    // Use the Google Sheet picker directly which will show loading indicators
    this.importDialogService.openGoogleSheetPicker('partner');
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
      }
    ];
    
    this.columns.set(fallbackColumns);
  }
}
