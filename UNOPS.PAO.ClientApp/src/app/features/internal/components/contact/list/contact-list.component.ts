import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal, computed } from '@angular/core';
import { NgIf, AsyncPipe } from '@angular/common';

import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DialogService } from 'primeng/dynamicdialog';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';

import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import {DialogModule} from 'primeng/dialog';
import {ContactEditDialogComponent} from '../edit-dialog/contact-edit-dialog.component';
import {BusinessCardScannerComponent} from './business-card-scanner/business-card-scanner.component';
import {ListviewComponent} from '../../../../../common/pages/components/listview/listview.component';
import {ContactService} from '../../../services/contact.service';
import {FeedbackDialogService} from '../../../../../common/pages/services/feedback-dialog.service';
import {ListViewColumn, ListViewConfig, SearchParams} from '../../../../../common/pages/components/listview/listview.model';
import {Contact} from '../../../models/contact.model';
import { ImportDialogService } from '../../../../../common/reusables/components/import/dialog/import-dialog.service';
import { SearchField } from '../../../../../common/services/search-parser.service';
import { PermissionUtilityService } from '../../../../../essentials/services/permission-utility.service';
import { EntityConfigurationService } from '../../../services/entity-configuration.service';

@Component({
  selector: 'app-contact-list',
  templateUrl: './contact-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    PanelModule,
    ButtonModule,
    TableModule,
    DialogModule,
    ScrollPanelModule,
    ProgressSpinnerModule,
    TranslateModule,
    ListviewComponent,
    ConfirmDialog,
    NgIf,
    AsyncPipe
  ],
  providers: [DialogService, ConfirmationService]
})
export class ContactListComponent implements OnInit, OnDestroy {
  router = inject(Router);
  route = inject(ActivatedRoute);
  contactService = inject(ContactService);
  feedbackDialogService = inject(FeedbackDialogService);
  dialogService = inject(DialogService);
  importDialogService = inject(ImportDialogService);
  permissionUtilityService = inject(PermissionUtilityService);
  entityConfigurationService = inject(EntityConfigurationService);
  cdr = inject(ChangeDetectorRef);

  // Permission management using utility service
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Contact');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  // Dynamic contact columns loaded from API
  contactColumns = signal<ListViewColumn[]>([]);
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
    entityName: 'Contact',
    scrollable: true,
    scrollHeight: 'flex',
          searchConfig: {
        useAdvancedSearch: true,
        placeholder: 'search.contactsPlaceholder',
                searchableFields: [
          {
            field: 'firstName',
            label: 'label.contact.firstName',
            type: 'string',
            operators: ['is', 'is not', 'like', 'not like']
          },
          {
            field: 'lastName',
            label: 'label.contact.lastName',
            type: 'string',
            operators: ['is', 'is not', 'like', 'not like']
          },
          {
            field: 'email',
            label: 'label.contact.email',
            type: 'string',
            operators: ['is', 'is not', 'like', 'not like']
          },
          {
            field: 'mobile',
            label: 'label.contact.mobile',
            type: 'string',
            operators: ['is', 'is not', 'like', 'not like']
          },
          {
            field: 'phone',
            label: 'label.contact.phone',
            type: 'string',
            operators: ['is', 'is not', 'like', 'not like']
          },
          {
            field: 'title',
            label: 'label.contact.title',
            type: 'string',
            operators: ['is', 'is not', 'like', 'not like']
          },
          {
            field: 'mailingCity',
            label: 'label.contact.mailingCity',
            type: 'string',
            operators: ['is', 'is not', 'like', 'not like']
          },
          {
            field: 'mailingCountry',
            label: 'label.contact.mailingCountry',
            type: 'string',
            operators: ['is', 'is not']
          },
          {
            field: 'partner.name',
            label: 'label.partner.partner',
            type: 'string',
            operators: ['is', 'is not', 'like', 'not like']
          }
      ] as SearchField[]
    }
  }));

  // Track current search term
  currentSearchText = '';

  ngOnInit() {


    // Load permissions using utility service
    this.permissionUtils.loadPermissions(this.router, this.cdr);

    // Load dynamic columns from API
    this.loadContactColumns();

    this.route.queryParams
      .subscribe(params => {
        if (params['openNewDialog'] === 'true') {
          const state = history.state;
          const emptyContact: Contact = {};
          this.openContactEditDialog(state?.data || emptyContact);
        }
      });
  }

  private loadContactColumns() {
    this.columnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('Contact')
      .subscribe({
        next: (columns) => {
          // Convert backend columns to frontend format and add template functions
          const processedColumns = columns.map(col => this.processColumn(col));
          this.contactColumns.set(processedColumns);
          this.columnsLoading.set(false);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Failed to load contact columns:', error);
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

    return processedColumn;
  }

  private createTemplateFunction(templatePattern: string): (rowData: any) => string {
    return (rowData: any) => {
      let result = templatePattern;

      // Replace field placeholders like {firstName}, {lastName} with actual values
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
      { field: 'profilePictureUrl', label: '', type: 'avatar', sortable: false, width: '5%' },
      {
        field: 'partnerName',
        label: 'label.partner.partner',
        type: 'text',
        sortable: false,
        width: '15%',
        ellipsis: true
      },
      {
        field: 'fullName',
        label: 'label.contact.fullName',
        type: 'template',
        sortable: false,
        width: '20%',
        templateFn: (contact: any) => {
          const firstName = contact.firstName || '';
          const lastName = contact.lastName || '';
          const middleName = contact.middleName || '';
          return `${firstName} ${middleName} ${lastName}`.trim();
        }
      },
      { field: 'title', label: 'label.contact.title', type: 'text', sortable: true, width: '15%' },
      {
        field: 'createdByName',
        label: 'label.audit.createdBy',
        type: 'text',
        sortable: false,
        width: '15%',
      },
      { field: 'createdByOfficeName', label: 'label.contact.createdByOffice', type: 'text', sortable: false, width: '15%' },
    ];

    this.contactColumns.set(fallbackColumns);
  }

  ngOnDestroy() {
    // No need to clear caches manually - utility service handles this
  }

  handleOnOpenRecordDetails(record: any) {
    if (record?.data) {
      record = record.data;
    }
    if (record && record.id !== undefined && record.id !== null) {

      this.router.navigate(['partnerships/contacts', record.id.toString()]);
    } else {
      console.error('Cannot navigate: record or record.id is undefined', record);
    }
  }

  handleOnRecordDelete(record: Contact) {
    // Check if user has delete permission
    if (!this.permissionUtilityService.canDelete(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'message.noPermissionToDelete',
        summary: 'message.permissionDenied'
      });
      return;
    }

    this.contactService.deleteContactById(record.id).subscribe({
      next: () => {
        this.feedbackDialogService.showSuccessToast({ detail: 'message.recordDeletedSuccessfully' });
        // Trigger a refresh for the listview
        window.dispatchEvent(new CustomEvent('refresh-listview'));
      },
      error: (error: any) => {
        this.feedbackDialogService.showErrorToast({
          detail: 'message.failedToDeleteRecord',
          summary: error.message || 'message.anErrorOccurred'
        });
      }
    });
  }

  _handleOnRecordCreation(newRecordData: Contact) {
    if (newRecordData && newRecordData.id !== undefined && newRecordData.id !== null) {
      // Refresh the list before navigating to show the new contact
      window.dispatchEvent(new CustomEvent('refresh-listview'));
      // Navigate to the new contact details

      this.router.navigate(['partnerships/contacts', newRecordData.id.toString()]);
    } else {
      console.error('Cannot navigate to created contact: id is undefined', newRecordData);
    }
  }

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

    const ref = this.dialogService.open(ContactEditDialogComponent, {
      header: contactData.id ? 'title.editContact' : 'title.newContact',
      width: '40vw',
      breakpoints: { '960px': '95vw' },
      closable: true,
      data: {
        mode: contactData.id ? 'edit' : 'new',
        record: contactData,
      }
    });

    const refSub = ref.onClose.subscribe((result) => {
      if (result) {
        this._handleOnRecordCreation(result);
      }
      refSub.unsubscribe();
    });
  }

  onRowClick(contact: Contact) {
    this.handleOnOpenRecordDetails(contact);
  }

  openBusinessCardScanner() {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'message.noPermissionToCreate',
        summary: 'message.permissionDenied'
      });
      return;
    }

    const ref = this.dialogService.open(BusinessCardScannerComponent, {
      header: 'title.scanBusinessCard',
      width: '95vw',
      style: { maxWidth: '800px' },
      closable: true
    });

    const refSub = ref.onClose.subscribe((result) => {
      if (result) {
        this.openContactEditDialog(result);
      }
    });
  }

  openImportDialog() {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'message.noPermissionToImport',
        summary: 'message.permissionDenied'
      });
      return;
    }

    // Use the Google Sheet picker directly which will show loading indicators
    this.importDialogService.openGoogleSheetPicker('contact');
  }

  /**
   * Store the current search text when search is performed
   * @param searchParams Current search parameters
   */
  onSearchChange(searchParams: SearchParams) {
    this.currentSearchText = searchParams.generalSearch || '';
  }
}
