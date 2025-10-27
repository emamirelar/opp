import { Component, Input, OnInit, signal, inject } from '@angular/core';
import { Panel } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { DynamicDialogModule } from 'primeng/dynamicdialog';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { PartnerViewContactsItemComponent } from './item/partner-view-contacts-item.component';
import { Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { ContactViewModel, GroupedContact } from './contact-view.model';
import { ContactService } from '@partnerships/contacts/services/contact.service';
import { PartnerViewContactsDialogComponent } from './dialog/partner-view-contacts-dialog.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ContactEditDialogComponent } from '@partnerships/contacts/components/contact/edit-dialog/contact-edit-dialog.component';
import { PermissionUtilityService } from '@core/services/auth';
import { FeedbackDialogService } from '@shared/services/ui';

@Component({
  selector: 'app-partner-view-contacts',
  standalone: true,
  imports: [
    Panel,
    ButtonModule,
    TooltipModule,
    DynamicDialogModule,
    PartnerViewContactsItemComponent,
    TranslateModule,
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

  // Inject services
  public permissionUtilityService = inject(PermissionUtilityService);
  private feedbackDialogService = inject(FeedbackDialogService);

  // Permission handling for contacts
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Contact');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  constructor(
    private dialogService: DialogService,
    private contactService: ContactService,
    private router: Router,
    private translateService: TranslateService
  ) {}

  ngOnInit() {
    // Load permissions
    this.permissionUtils.loadPermissions(this.router);

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

  /**
   * Opens the contact creation dialog pre-filled with the current partner information
   */
  openNewContactDialog(): void {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('message.noPermissionToCreate'),
        summary: this.translateService.instant('message.permissionDenied')
      });
      return;
    }

    const ref = this.dialogService.open(ContactEditDialogComponent, {
      header: this.translateService.instant('title.newContact'),
      width: '40vw',
      breakpoints: { '960px': '95vw' },
      closable: true,
      data: {
        mode: 'new',
        record: {},
        partnerContext: {
          partnerId: this.partnerId,
          lockPartner: true
        }
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        this.loadContacts();
      }
    });
  }
}
