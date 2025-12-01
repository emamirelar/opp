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
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { DialogService } from 'primeng/dynamicdialog';

import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ListviewComponent } from '@features/list-view/components/listview/listview.component';
import { ListViewColumn, ListViewConfig } from '@features/list-view/components/listview/listview.model';
import { SearchField } from '@shared/services/utils';
import { ContactService } from '@partnerships/contacts/services/contact.service';
import { EntityConfigurationService } from '@shared/services/api/entity-configuration.service';
import { PermissionUtilityService } from '@core/services/auth';
import { PermissionService, EntityPermissions } from '@core/services/auth';
import { FeedbackDialogService } from '@shared/services/ui';
import { ContactEditDialogComponent } from '@partnerships/contacts/components/contact/edit-dialog/contact-edit-dialog.component';
import { Contact } from '@partnerships/contacts/models/contact.model';

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
  selector: 'app-partner-contacts',
  templateUrl: './partner-contacts.component.html',
  imports: [CommonModule, ButtonModule, TranslateModule, ListviewComponent, ConfirmDialog],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DialogService, ConfirmationService]
})
export class PartnerContactsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private contactService = inject(ContactService);
  private entityConfigurationService = inject(EntityConfigurationService);
  private permissionUtilityService = inject(PermissionUtilityService);
  private permissionService = inject(PermissionService);
  private feedbackDialogService = inject(FeedbackDialogService);
  private dialogService = inject(DialogService);
  private translateService = inject(TranslateService);
  private cdr = inject(ChangeDetectorRef);

  partnerId = input<string>();
  partnerName = input<string>();
  dataUrl = signal<string>('');

  // Permission management for contacts within partner context
  entityPermissions = signal<EntityPermissions>({
    entity: 'Contact',
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

  // Fallback columns definition
  private fallbackColumns: ListViewColumn[] = [
    {
      field: 'profilePictureUrl',
      label: 'Avatar',
      type: 'avatar',
      sortable: false
    },
    {
      field: 'firstName',
      label: 'contacts.firstName',
      type: 'text',
      sortable: true
    },
    {
      field: 'lastName',
      label: 'contacts.lastName',
      type: 'text',
      sortable: true
    },
    {
      field: 'title',
      label: 'contacts.title',
      type: 'text',
      sortable: true
    },
    {
      field: 'email',
      label: 'contacts.email',
      type: 'email',
      sortable: true
    },
    {
      field: 'phone',
      label: 'contacts.phone',
      type: 'text',
      sortable: false
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
    entityName: 'Contact',
    defaultSortField: 'firstName',
    defaultSortOrder: 'asc',
    sortableFields: [
      { field: 'firstName', label: 'First Name' },
      { field: 'createdDate', label: 'Created Date' },
      { field: 'lastModifiedDate', label: 'Last Updated Date' }
    ],
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: this.translateService.instant('search.contactsPlaceholder'),
      entityType: 'Contact' as const,
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
    } else {
      // Otherwise, get partnerId from parent route params (when used as a child route)
      this.route.parent?.paramMap.subscribe(params => {
        const recordId = params.get('recordId');
        if (recordId) {
          this.setupDataUrlWithPartnerFilter(recordId);
          // Load permissions once we have the partner ID
          this.loadPermissions();
        }
      });
    }

    // Load dynamic columns from API
    this.loadContactColumns();
    
    // Load dynamic search fields from API
    this.loadSearchFields();
  }

  private setupDataUrlWithPartnerFilter(partnerId: string): void {
    // Set the base URL with partnerId parameter for filtering
    this.dataUrl.set(`/api/contact?partnerId=${partnerId}`);
  }

  private loadSearchFields(): void {
    this.isLoadingSearchFields.set(true);
    this.searchFieldsError.set(null);

    const endpoint = '/api/contact/search-fields';

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
        console.error('Error loading contact search fields:', error);
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
    
    // Construct the correct route path for contacts within partner context
    const currentPartnerId = this.partnerId() || this.getCurrentPartnerIdFromRoute();
    const contactsRoutePath = currentPartnerId ? 
      `partnerships/partners/${currentPartnerId}/contacts` : 
      'partnerships/contacts';
    
    // Load from server using the specific contacts route
    this.permissionService.getEntityPermissions(contactsRoutePath)
      .subscribe({
        next: (permissions) => {
          if (!permissions.hasAccess) {
            console.warn('No access to partner contacts');
            this.router.navigate(['/access-denied']);
            return;
          }
          
          this.entityPermissions.set(permissions);
          this.permissionsLoading.set(false);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Error loading contact permissions in partner context:', error);
          this.permissionsLoading.set(false);
          this.cdr.detectChanges();
        }
      });
  }

  private loadContactColumns() {
    this.columnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('Contact')
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
          console.error('Failed to load contact columns:', error);
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

  onContactClick(contact: any): void {
    if (contact?.id) {
      this.router.navigate(['/partnerships/contacts', contact.id]);
    }
  }

  onAddContact(): void {
    this.openContactEditDialog();
  }

  private getCurrentPartnerIdFromRoute(): string {
    return this.route.parent?.snapshot.paramMap.get('recordId') || '';
  }

  handleOnOpenRecordDetails(record: any) {
    if (record == null) {
      return;
    }
    this.router.navigate(['/partnerships/contacts', record.id]);
  }

  /**
   * Opens the contact creation or editing dialog with form fields for managing contact information
   */
  openContactEditDialog(contactData: Contact = {}) {
    // Check if user has appropriate permission
    if (contactData.id && !this.permissionUtilityService.canUpdate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'message.noPermissionToEdit',
        summary: 'message.permissionDenied'
      });
      return;
    } else if (!contactData.id && !this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'message.noPermissionToCreate',
        summary: 'message.permissionDenied'
      });
      return;
    }

    // Set the partner for the new contact if creating
    const currentPartnerId = this.partnerId() || this.getCurrentPartnerIdFromRoute();
    
    if (!contactData.id && currentPartnerId) {
      contactData.partner = { id: currentPartnerId };
    }

    const ref = this.dialogService.open(ContactEditDialogComponent, {
      header: contactData.id ? this.translateService.instant('title.editContact') : this.translateService.instant('title.newContact'),
      width: '40vw',
      breakpoints: { '960px': '95vw' },
      closable: true,
      data: {
        mode: contactData.id ? 'edit' : 'new',
        record: contactData,
        partnerContext: {
          partnerId: currentPartnerId,
          lockPartner: true // Lock partner field when opened from partner context
        }
      }
    });

    const refSub = ref.onClose.subscribe((result) => {
      if (result) {
        this._handleOnRecordCreation(result);
      }
      refSub.unsubscribe();
    });
  }


  private _handleOnRecordCreation(newRecordData: Contact) {
    if (newRecordData && newRecordData.id !== undefined && newRecordData.id !== null) {
      // Refresh the list before navigating to show the new contact
      window.dispatchEvent(new CustomEvent('refresh-listview'));
      // Navigate to the new contact details
      this.router.navigate(['partnerships/contacts', newRecordData.id.toString()]);
    } else {
      console.error('Cannot navigate to created contact: id is undefined', newRecordData);
    }
  }
}
