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
import {Contact} from '../../models/contact.model';

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

  contactData = this.contactService.allContacts;
  isDataLoading = this.contactService.isLoading;

  ngOnInit() {
    this.contactService.getAllContacts();

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
        this.contactService.getAllContacts();
      }
    });
  }

  _handleOnRecordCreation(newRecordData: any) {
    if (newRecordData?.id) {
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
}
