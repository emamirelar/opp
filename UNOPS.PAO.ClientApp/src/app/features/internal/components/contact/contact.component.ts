import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DatePipe } from '@angular/common';

import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ContactService } from '../../services/contact.service';
import { DialogModule } from 'primeng/dialog';

import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';
import {ContactNewComponent} from './new/contact-new.component';

@Component({
  selector: 'app-contact',
  templateUrl: './contact.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    PanelModule,
    ButtonModule,
    TableModule,
    DialogModule,
    ScrollPanelModule,
    ContactNewComponent,
    DatePipe,
    ProgressSpinnerModule,
    TranslateModule
  ]
})
export class ContactComponent {
  router = inject(Router);
  contactService = inject(ContactService);

  newContact = false;

  contactData = this.contactService.allContacts;
  isDataLoading = this.contactService.isLoading;
  feedbackDialogService = inject(FeedbackDialogService);

  ngOnInit() {
    this.contactService.getAllContacts();
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

  _handleOnRecordCreation( newRecordData: any ){
    //hides record creation dialog.
    this.newContact = false;
    //navigate to the newly created record.
    if( newRecordData !== null && ( newRecordData["id"] !== undefined && newRecordData["id"] !== null ) )
    {
      this.router.navigate([ 'contact', newRecordData["id"] ]);
    }
  }
}
