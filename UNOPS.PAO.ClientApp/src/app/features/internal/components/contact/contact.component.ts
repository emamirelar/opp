import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../../common/services/language.service';
import { Subscription } from 'rxjs';
import { ContactService } from '../../services/contact.service';
import { DialogModule } from 'primeng/dialog';
import { NewContactComponent } from './new-contact/new-contact.component';
import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';

interface columnDefination {
  label: string,
  id: string
}

@Component({
  selector: 'app-contact',
  templateUrl: './contact.component.html',
  styleUrl: './contact.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PanelModule, ButtonModule, TableModule, DialogModule, ScrollPanelModule, NewContactComponent, DatePipe, ProgressSpinnerModule, TranslateModule]
})
export class ContactComponent implements OnInit, OnDestroy {
  private langChangeSubscription: Subscription = new Subscription;
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);

  contactService = inject(ContactService);

  newContact : boolean = false;
  
  columns = signal<columnDefination[]>([]);

  contactData = this.contactService.allContacts;
  isDataLoading = this.contactService.isLoading;
  feedbackDialogService = inject(FeedbackDialogService);

  constructor(public translateService: TranslateService, private languageService: LanguageService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        //initialize columns
        this.columns.update(() => {
          return this.getColumns();
        });
        //make server call to get all proposals.
        this.contactService.getAllContacts();
      }
    });
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  getColumns() {
    return [{
      label: 'label.contact.actions',
      id: ''
    }, {
      label: 'label.contact.id',
      id: 'id'
    }, {
      label: 'label.contact.salutation',
      id: 'salutation'
    }, {
      label: 'label.contact.firstName',
      id: 'firstName'
    }, {
      label: 'label.contact.lastName',
      id: 'lastName'
    }, {
      label: 'label.contact.email',
      id: 'email'
    }, {
      label: 'label.contact.mobile',
      id: 'mobile'
    }];
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
    });;
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
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
