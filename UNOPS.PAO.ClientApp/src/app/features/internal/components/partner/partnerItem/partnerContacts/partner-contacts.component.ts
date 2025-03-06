import { afterNextRender, ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, input, OnDestroy, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { PartnerService } from '../../../../services/partner.service';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { LanguageService } from '../../../../../../common/services/language.service';

interface columnDefination {
  label: string,
  id: string
}

@Component({
  selector: 'app-partnerContacts',
  templateUrl: './partner-contacts.component.html',
  styleUrls: ['./partner-contacts.component.css'],
  imports: [ButtonModule, DialogModule, TableModule, DatePipe, TranslateModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerContactsComponent implements OnInit, OnDestroy {
  private langChangeSubscription: Subscription = new Subscription;
  partnerService = inject(PartnerService);

  partnerId = input.required<string>();
  partnerName = input<string>();
  columns = signal<columnDefination[]>([]);

  //opportunityId: string = '';
  contactData = signal<any>([]);
  isDataLoading = this.partnerService.isLoading;

  constructor(private languageService: LanguageService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    //initialize columns
    this.columns.update(() => {
      return this.getColumns();
    });
    //fetch data
    this.partnerService.getAllContactsById(this.partnerId()).subscribe({
      next: (data: any) => {
        this.contactData.set(data);
      }
    });
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  getColumns() {
    return [{
      label: 'Contact Id',
      id: 'id'
    }, {
      label: 'Partner Name',
      id: 'partnerName'
    }, {
      label: 'Salutation',
      id: 'salutation'
    }, {
      label: 'First Name',
      id: 'firstName'
    }, {
      label: 'Last Name',
      id: 'lastName'
    }, {
      label: 'Email',
      id: 'email'
    }, {
      label: 'Mobile',
      id: 'mobile'
    }];
  }

  handleOnOpenRecordDetails(record: any) {
    if (record == null || record == undefined) {
      return;
    }
    let URL = window.location.origin + "/#/contact/" + record["id"];
    window.open(URL, "_blank");
  }

  _handleOnViewContactDaialogClose() { }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }
}
