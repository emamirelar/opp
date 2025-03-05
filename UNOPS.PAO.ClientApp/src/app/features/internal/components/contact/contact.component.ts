import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnDestroy, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../../common/services/language.service';
import { Subscription, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ContactService } from '../../services/contact.service';
import { DialogModule } from 'primeng/dialog';
import { NewContactComponent } from './new-contact/new-contact.component';
import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';

interface ColumnDefinition {
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
  private langChangeSubscription: Subscription = new Subscription();
  private destroy$ = new Subject<void>();

  newContact: boolean = false;
  columns = signal<ColumnDefinition[]>([]);
  contactData: any;
  isDataLoading: any;

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private contactService: ContactService,
    private feedbackDialogService: FeedbackDialogService,
    public translateService: TranslateService,
    private languageService: LanguageService,
    private cdr: ChangeDetectorRef
  ) {
    this.contactData = contactService.allContacts;
    this.isDataLoading = contactService.isLoading;
   }

  ngOnInit() {
    this.activatedRoute.paramMap.pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.columns.update(() => this.getColumns());
        this.contactService.getAllContacts();
      }
    });

    this.langChangeSubscription = this.languageService.translationService.onLangChange.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  getColumns(): ColumnDefinition[] {
    return [
      { label: 'label.contact.actions', id: '' },
      { label: 'label.contact.id', id: 'id' },
      { label: 'label.contact.salutation', id: 'salutation' },
      { label: 'label.contact.firstName', id: 'firstName' },
      { label: 'label.contact.lastName', id: 'lastName' },
      { label: 'label.contact.email', id: 'email' },
      { label: 'label.contact.mobile', id: 'mobile' }
    ];
  }

  handleOnOpenRecordDetails(record: { id: string }) {
    this.router.navigate(['contact', record.id]);
  }

  handleOnRecordDelete(record: { id: string }) {
    this.contactService.deleteContactById(record.id).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Record deleted successfully!' });
        this.contactService.getAllContacts();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  _handleOnRecordCreation(newRecordData: any) {
    this.newContact = false;
    if (newRecordData?.id) {
      this.router.navigate(['contact', newRecordData.id]);
    }
  }
}