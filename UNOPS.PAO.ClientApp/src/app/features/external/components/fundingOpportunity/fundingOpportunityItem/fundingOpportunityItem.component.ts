import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { FundingOpportunityOverviewComponent } from './fundingOpportunityOverview.component';

import { MenuItem } from 'primeng/api';

import { ProposalService } from '../../../services/proposal.service';
import { WorkflowService } from './../../../../../common/services/workflow.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-fundingOpportunityItem',
  templateUrl: './fundingOpportunityItem.component.html',
  styleUrls: ['./fundingOpportunityItem.component.css'],
  imports: [
    PanelModule,
    ButtonModule,
    FundingOpportunityOverviewComponent,
    TranslateModule,
  ],
})
export class FundingOpportunityItemComponent {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  proposalService = inject(ProposalService);
  workflowService = inject(WorkflowService);

  recordId: string = '';
  recordData = signal<any>({});
  stages = signal<MenuItem[] | []>([]);

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        this.recordId = paramMap.get('recordId') || '';
      },
    });
  }

  handleOnInitiateProposalClick(event: MouseEvent) {
    this.proposalService
      .createRecordByFundingOpportunityId(this.recordId)
      .subscribe({
        next: (data: any) => {
          this.router.navigate(['external/proposal/' + data.id]);
        },
      });
  }
}
