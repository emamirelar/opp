import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  input,
  OnInit,
  signal
} from '@angular/core';
import { DatePipe } from '@angular/common';

import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { PartnerService } from '../../../services/partner.service';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../../../common/services/language.service';


@Component({
  selector: 'app-partner-contacts',
  templateUrl: './partner-contacts.component.html',
  styleUrls: ['./partner-contacts.component.css'],
  imports: [ButtonModule, DialogModule, TableModule, DatePipe, TranslateModule],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerContactsComponent implements OnInit {
  partnerService = inject(PartnerService);

  partnerId = input.required<string>();
  partnerName = input<string>();

  contactData = signal<any>([]);
  isDataLoading = this.partnerService.isLoading;

  constructor(private languageService: LanguageService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.partnerService.getAllContactsById(this.partnerId()).subscribe((data: any) => {
        this.contactData.set(data);
    });
  }

  handleOnOpenRecordDetails(record: any) {
    if (record == null) {
      return;
    }
    let URL = window.location.origin + "/#/contact/" + record["id"];
    window.open(URL, "_blank");
  }
}
