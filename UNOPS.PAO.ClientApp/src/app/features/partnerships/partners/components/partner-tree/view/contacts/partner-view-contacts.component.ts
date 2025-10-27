import { Component, Input, OnInit, signal } from '@angular/core';
import { Panel } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { DynamicDialogModule } from 'primeng/dynamicdialog';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { PartnerViewContactsItemComponent } from './item/partner-view-contacts-item.component';
import { Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { ContactViewModel, GroupedContact } from '@partnerships/contacts/models/contact-view.model';
import { ContactService } from '@partnerships/contacts/services/contact.service';
import { PartnerViewContactsDialogComponent } from './dialog/partner-view-contacts-dialog.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-partner-view-contacts',
  standalone: true,
  imports: [
    Panel,
    ButtonModule,
    TooltipModule,
    DynamicDialogModule,
    PartnerViewContactsItemComponent,
    PartnerViewContactsDialogComponent,
    TranslateModule
  ],
  templateUrl: './partner-view-contacts.component.html',
  providers: [DialogService]
})
export class PartnerViewContactsComponent implements OnInit {
  @Input()
  partnerId?: string;

  dialogRef: DynamicDialogRef | undefined;
  isLoading = signal<boolean>(false);
  contacts: ContactViewModel[] = [];

  constructor(
    private dialogService: DialogService,
    private contactService: ContactService,
    private router: Router,
    private translateService: TranslateService
  ) {}

  ngOnInit() {
    if (this.partnerId) {
      this.loadContacts();
    }
  }

  get groupedContacts(): GroupedContact[] {
    const contacts = [...this.contacts];

    const grouped = contacts.reduce((acc, contact) => {
      const letter = (contact.lastName || '').charAt(0).toUpperCase();
      const key = letter || '#';

      if (!acc[key]) {
        acc[key] = {
          letter: key,
          contacts: []
        };
      }
      acc[key].contacts.push(contact);
      return acc;
    }, {} as Record<string, GroupedContact>);

    return Object.values(grouped).sort((a, b) => a.letter.localeCompare(b.letter));
  }

  openContactView(contact: ContactViewModel): void {
    this.router.navigate(['/contact', contact.id]);
  }

  openFullScreenContacts(): void {
    this.dialogRef = this.dialogService.open(PartnerViewContactsDialogComponent, {
      header: this.translateService.instant('title.contacts'),
      width: '90vw',
      height: '90vh',
      closable: true,
      style: { maxWidth: '800px' },
      data: {
        partnerId: this.partnerId
      }
    });

    this.dialogRef.onClose.subscribe(result => {
      if (result) {
        this.loadContacts();
      }
    });
  }

  private mapToViewModel(contact: any): ContactViewModel {
    return {
      id: contact.id,
      firstName: contact.firstName || '',
      lastName: contact.lastName || '',
      title: contact.title,
      email: contact.email,
      phone: contact.phone,
      profilePictureUrl: contact.profilePictureUrl
    };
  }

  loadContacts(): void {
    if (!this.partnerId) {
      return;
    }

    this.isLoading.set(true);

    this.contactService.getAll({
      partnerId: Number(this.partnerId),
      pageSize: 5
    })
      .pipe(
        map(response => response.body?.records.map((c: any) => this.mapToViewModel(c)) || []),
      )
      .subscribe(data => {
        this.contacts = data;
        this.isLoading.set(false);
      });
  }
} 
