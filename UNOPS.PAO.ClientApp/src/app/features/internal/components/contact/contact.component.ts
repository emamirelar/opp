import { ChangeDetectionStrategy, Component, inject, OnInit, signal, OnDestroy} from '@angular/core';
import { NgIf } from '@angular/common';

import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { Router, ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ContactService } from '../../services/contact.service';
import { DialogModule } from 'primeng/dialog';

import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';
import { ContactNewComponent } from './new/contact-new.component';
import { BusinessCardScannerComponent } from './business-card-scanner/business-card-scanner.component';
import { Contact } from '../../models/contact.model';
import { ListviewComponent } from '../../../../common/pages/components/listview/listview.component';
import { ListViewColumn, ListViewConfig } from '../../../../common/pages/components/listview/listview.model';

@Component({
  selector: 'app-contact',
  templateUrl: './contact.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    PanelModule,
    ButtonModule,
    TableModule,
    DialogModule,
    ScrollPanelModule,
    ContactNewComponent,
    BusinessCardScannerComponent,
    ProgressSpinnerModule,
    TranslateModule,
    ListviewComponent,
    NgIf
  ]
})
export class ContactComponent implements OnInit {
  router = inject(Router);
  route = inject(ActivatedRoute);
  contactService = inject(ContactService);
  feedbackDialogService = inject(FeedbackDialogService);

  newContact = signal(false);
  newContactData = signal<any>(null);

  // Define contact columns for the listview
  contactColumns: ListViewColumn[] = [
    { field: 'id', label: 'label.contact.id', type: 'text', sortable: false, width: '10%' },
    { field: 'salutation', label: 'label.contact.salutation', type: 'text', sortable: true, width: '10%' },
    { field: 'firstName', label: 'label.contact.firstName', type: 'text', sortable: true, width: '15%' },
    { field: 'lastName', label: 'label.contact.lastName', type: 'text', sortable: true, width: '15%' },
    { field: 'email', label: 'label.contact.email', type: 'text', sortable: true, width: '25%' },
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
    scrollable: true,
    scrollHeight: 'flex'
  };

  ngOnInit() {
    this.route.queryParams
      .subscribe(params => {
        if (params['openNewDialog'] === 'true') {
          const state = history.state;
          if (state?.data) {
            this.newContactData.set(state.data);
          }

          this.newContact.set(true);
        }
      });
  }

  handleOnOpenRecordDetails(record: any) {
    this.router.navigate(['contact', record.id]);
  }

  handleOnRecordDelete(record: any) {
    this.contactService.deleteContactById(record.id).subscribe({
      next: (data: any) => {
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

  _handleOnRecordCreation(newRecordData: any) {
    if (newRecordData?.id) {
      // Refresh the list before navigating to show the new contact
      window.dispatchEvent(new CustomEvent('refresh-listview'));
      // Navigate to the new contact details
      this.router.navigate(['contact', newRecordData.id]);
    }
  }

  closeNewContactDialog() {
    this.newContact.set(false);
    this.newContactData.set(null);
  }

  onScannedContact(scannedContact: Contact) {
    this.newContact.set(true);
    this.newContactData.set(scannedContact);
  }

  // Handle row selection from listview
  onRowSelected(contact: Contact) {
    // You can implement custom behavior here if needed
  }

  // Handle row double-click from listview
  onRowDoubleClicked(contact: Contact) {
    this.handleOnOpenRecordDetails(contact);
  }
}
