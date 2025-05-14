import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { NgIf } from '@angular/common';

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
import {FeedbackDialogService} from '../../../../../common/reusables/services/feedback-dialog.service';
import {ListViewColumn, ListViewConfig, SearchParams} from '../../../../../common/pages/components/listview/listview.model';
import {Contact} from '../../../models/contact.model';
import { ImportDialogService } from '../../../../../common/reusables/components/import/dialog/import-dialog.service';
import { SearchField } from '../../../../../common/services/search-parser.service';

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
    ConfirmDialog
  ],
  providers: [DialogService, ConfirmationService]
})
export class ContactListComponent implements OnInit {
  router = inject(Router);
  route = inject(ActivatedRoute);
  contactService = inject(ContactService);
  feedbackDialogService = inject(FeedbackDialogService);
  dialogService = inject(DialogService);
  importDialogService = inject(ImportDialogService);

  // Define contact columns for the listview
  contactColumns: ListViewColumn[] = [
    { field: 'profilePictureUrl', label: '', type: 'avatar', sortable: false, width: '5%' },
    { field: 'firstName', label: 'label.contact.firstName', type: 'text', sortable: true, width: '15%' },
    { field: 'lastName', label: 'label.contact.lastName', type: 'text', sortable: true, width: '15%' },
    { field: 'email', label: 'label.contact.email', type: 'email', sortable: true, width: '25%' },
    { field: 'mobile', label: 'label.contact.mobile', type: 'text', sortable: true, width: '15%' }
  ];

  // Configure listview behavior
  listviewConfig: ListViewConfig = {
    enableSelection: true,
    selectionMode: 'single',
    enablePagination: true,
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enableSorting: true,
    enableSearch: true,
    enableExport: true,
    entityName: 'Contact',
    scrollable: true,
    scrollHeight: 'flex',
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: 'Search contacts...',
      searchableFields: [
        { 
          field: 'firstName', 
          label: 'First Name', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'lastName', 
          label: 'Last Name', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'email', 
          label: 'Email', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'mobile', 
          label: 'Mobile', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'phone', 
          label: 'Phone', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'title', 
          label: 'Title', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'mailingCity', 
          label: 'City', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'mailingCountry', 
          label: 'Country', 
          type: 'string',
          operators: ['is', 'is not']
        },
        { 
          field: 'partner.name', 
          label: 'Partner', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        }
      ] as SearchField[]
    }
  };

  // Track current search term
  currentSearchText = '';

  ngOnInit() {
    console.log('Contact list config:', this.listviewConfig);
    
    this.route.queryParams
      .subscribe(params => {
        if (params['openNewDialog'] === 'true') {
          const state = history.state;
          const emptyContact: Contact = {};
          this.openContactEditDialog(state?.data || emptyContact);
        }
      });
  }

  handleOnOpenRecordDetails(record: Contact) {
    this.router.navigate(['contact', record.id]);
  }

  handleOnRecordDelete(record: Contact) {
    this.contactService.deleteContactById(record.id).subscribe({
      next: () => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Record deleted successfully!' });
        // Trigger a refresh for the listview
        window.dispatchEvent(new CustomEvent('refresh-listview'));
      },
      error: (error: any) => {
        this.feedbackDialogService.showErrorToast({
          detail: 'Failed to delete record',
          summary: error.message || 'An error occurred'
        });
      }
    });
  }

  _handleOnRecordCreation(newRecordData: Contact) {
    if (newRecordData?.id) {
      // Refresh the list before navigating to show the new contact
      window.dispatchEvent(new CustomEvent('refresh-listview'));
      // Navigate to the new contact details
      this.router.navigate(['contact', newRecordData.id]);
    }
  }

  openContactEditDialog(contactData: Contact = {}) {
    const ref = this.dialogService.open(ContactEditDialogComponent, {
      header: contactData.id ? 'Edit Contact' : 'New Contact',
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
    const ref = this.dialogService.open(BusinessCardScannerComponent, {
      header: 'Scan Business Card',
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
