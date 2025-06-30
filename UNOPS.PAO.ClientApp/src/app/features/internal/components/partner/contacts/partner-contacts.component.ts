import {
  ChangeDetectionStrategy,
  Component,
  input,
  OnInit,
  signal,
  inject
} from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { ActivatedRoute, Router } from '@angular/router';

import { TranslateModule } from '@ngx-translate/core';
import { ListviewComponent } from '../../../../../common/pages/components/listview/listview.component';
import { ListViewColumn, ListViewConfig } from '../../../../../common/pages/components/listview/listview.model';
import { ContactService } from '../../../services/contact.service';

@Component({
  selector: 'app-partner-contacts',
  templateUrl: './partner-contacts.component.html',
  imports: [ButtonModule, TranslateModule, ListviewComponent],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerContactsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private contactService = inject(ContactService);

  partnerId = input<string>();
  partnerName = input<string>();
  dataUrl = signal<string>('');

  columns: ListViewColumn[] = [
    {
      field: 'profilePictureUrl',
      label: 'Avatar',
      type: 'avatar',
      sortable: false
    },
    {
      field: 'firstName',
      label: 'contacts.firstName',
      type: 'text',
      sortable: true
    },
    {
      field: 'lastName',
      label: 'contacts.lastName',
      type: 'text',
      sortable: true
    },
    {
      field: 'title',
      label: 'contacts.title',
      type: 'text',
      sortable: true
    },
    {
      field: 'email',
      label: 'contacts.email',
      type: 'email',
      sortable: true
    },
    {
      field: 'phone',
      label: 'contacts.phone',
      type: 'text',
      sortable: false
    }
  ];

  config: ListViewConfig = {
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enablePagination: false, // Using infinite scroll
    enableSorting: true,
    enableExport: true,
    scrollable: true,
    scrollHeight: 'calc(100vh - 20rem)',
    autoSwitchToCardView: false,
    autoSwitchMinWidth: 768,
    defaultViewMode: 'card',
    entityName: 'Contact',
    cardConfig: {
      titleField: 'firstName',
      contentFields: ['lastName', 'title', 'email', 'phone'],
      cardsPerRow: {
        xs: 1,
        sm: 2,
        md: 2,
        lg: 3,
        xl: 4
      }
    },
  };

  ngOnInit(): void {
    // If partnerId is provided as input, use it directly
    if (this.partnerId()) {
      this.dataUrl.set(`/api/contact?partnerId=${this.partnerId()}`);
    } else {
      // Otherwise, get partnerId from parent route params (when used as a child route)
      this.route.parent?.paramMap.subscribe(params => {
        const recordId = params.get('recordId');
        if (recordId) {
          this.dataUrl.set(`/api/contact?partnerId=${recordId}`);
        }
      });
    }
  }

  onContactClick(contact: any): void {
    if (contact?.id) {
      this.router.navigate(['/partnerships/contacts', contact.id]);
    }
  }

  onAddContact(): void {
    const currentPartnerId = this.partnerId() || this.getCurrentPartnerIdFromRoute();
    console.log('Add contact for partner:', currentPartnerId);
    // Open add contact dialog or navigate to create form
    // Implementation depends on your contact creation flow
  }

  private getCurrentPartnerIdFromRoute(): string {
    return this.route.parent?.snapshot.paramMap.get('recordId') || '';
  }

  handleOnOpenRecordDetails(record: any) {
    if (record == null) {
      return;
    }
    this.router.navigate(['/partnerships/contacts', record.id]);
  }
}
