import {
  ChangeDetectionStrategy,
  Component,
  input
} from '@angular/core';

import { ButtonModule } from 'primeng/button';

import { TranslateModule } from '@ngx-translate/core';
import { ListviewComponent } from '../../../../../common/pages/components/listview/listview.component';
import { ListViewColumn } from '../../../../../common/pages/components/listview/listview.model';

@Component({
  selector: 'app-partner-contacts',
  templateUrl: './partner-contacts.component.html',
  styleUrls: ['./partner-contacts.component.css'],
  imports: [ButtonModule, TranslateModule, ListviewComponent],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerContactsComponent {
  partnerId = input.required<string>();
  partnerName = input<string>();

  columns: ListViewColumn[] = [
    {
      field: 'id',
      label: 'Contact Id',
      sortable: false,
      type: 'text'
    },
    {
      field: 'partnerName',
      label: 'Partner Name',
      sortable: true,
      type: 'text'
    },
    {
      field: 'salutation',
      label: 'Salutation',
      sortable: true,
      type: 'text'
    },
    {
      field: 'firstName',
      label: 'First Name',
      sortable: true,
      type: 'text'
    },
    {
      field: 'lastName',
      label: 'Last Name',
      sortable: true,
      type: 'text'
    },
    {
      field: 'email',
      label: 'Email',
      sortable: true,
      type: 'text'
    },
    {
      field: 'mobile',
      label: 'Mobile',
      sortable: true,
      type: 'text'
    }
  ];

  handleOnOpenRecordDetails(record: any) {
    if (record == null) {
      return;
    }
    const URL = window.location.origin + "/#/contact/" + record["id"];
    window.open(URL, "_blank");
  }
}
