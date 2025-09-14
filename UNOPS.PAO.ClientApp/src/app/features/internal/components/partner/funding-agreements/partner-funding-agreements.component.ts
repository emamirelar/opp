import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  signal,
  inject
} from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Tab, TabList, Tabs, TabPanels, TabPanel } from 'primeng/tabs';
import { CardModule } from 'primeng/card';
import {Panel} from 'primeng/panel';

@Component({
  selector: 'app-partner-funding-agreements',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    TranslateModule,
    CardModule,
    Panel
  ],
  template: `
    <div class="flex flex-col gap-8">
      <div class="flex flex-col xl:flex-row gap-8">
        <div class="xl:w-1/2 flex flex-col gap-8">
          <p-panel header="Partner Funding Opportunities" styleClass="shadow-sm">
          </p-panel>


          <p-panel header="Partnership Agreements & Templates" styleClass="shadow-sm">
          </p-panel>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerFundingAgreementsComponent implements OnInit {
  private route = inject(ActivatedRoute);


  partnerId = signal<string>('');
  ngOnInit(): void {
    // Get partnerId from parent route params (since this is a child route)
    this.route.parent?.paramMap.subscribe(params => {
      const recordId = params.get('recordId');
      if (recordId) {
        this.partnerId.set(recordId);
      }
    });
  }
}
