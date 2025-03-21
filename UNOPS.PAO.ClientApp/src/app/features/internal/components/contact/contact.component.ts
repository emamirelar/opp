import {ChangeDetectionStrategy, Component, inject, OnInit, signal, OnDestroy} from '@angular/core';
import {DatePipe, NgIf} from '@angular/common';

import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ContactService } from '../../services/contact.service';
import { DialogModule } from 'primeng/dialog';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';

import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';
import {ContactNewComponent} from './new/contact-new.component';

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
    DatePipe,
    ProgressSpinnerModule,
    TranslateModule,
    NgIf
  ]
})
export class ContactComponent implements OnInit {

  router = inject(Router);
  route = inject(ActivatedRoute);
  contactService = inject(ContactService);

  newContact = signal(false);
  newContactData = signal<any>(null);

  contactData = this.contactService.allContacts;
  isDataLoading = this.contactService.isLoading;
  feedbackDialogService = inject(FeedbackDialogService);

  ngOnInit() {
    this.contactService.getAllContacts();

    // Combine route parameters and navigation state
    this.route.queryParams
      .subscribe(params => {
        if (params['openNewDialog'] === 'true') {
          // Get contact data from history state
          const state = history.state;
          if (state?.contactData) {
            this.newContactData.set(state.contactData);
          }

          // Open the new contact dialog
          this.newContact.set(true);

          // Remove the query parameter to avoid reopening on page refresh
          this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { openNewDialog: null },
            queryParamsHandling: 'merge'
          });
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
}
