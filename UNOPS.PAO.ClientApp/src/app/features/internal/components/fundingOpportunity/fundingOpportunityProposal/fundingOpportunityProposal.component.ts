import { afterNextRender, ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, input, OnDestroy, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { FundingOpportunityService } from '../../../services/fundingOpportunity.service';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { LanguageService } from '../../../../../common/services/language.service';

interface columnDefination {
  label: string,
  id: string
}

@Component({
  selector: 'app-fundingOpportunityProposal',
  templateUrl: './fundingOpportunityProposal.component.html',
  styleUrls: ['./fundingOpportunityProposal.component.css'],
  imports: [ ButtonModule, DialogModule, TableModule, DatePipe, TranslateModule ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FundingOpportunityProposalComponent implements OnInit, OnDestroy {
  private langChangeSubscription: Subscription = new Subscription;
  fundingOpportunityService = inject(FundingOpportunityService);

  fundingOpportunityId = input.required<string>();
  fundingOpportunityName = input<string>();
  columns = signal<columnDefination[]>([]);

  opportunityId: string = '';
  proposalData = signal<any>([]);
  isDataLoading = this.fundingOpportunityService.isLoading;

  constructor(private languageService: LanguageService, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    //initialize columns
    this.columns.update(() => {
      return this.getColumns();
    });
    //fetch data
    this.fundingOpportunityService.getAllProposalsById(this.fundingOpportunityId()).subscribe({
      next: ( data: any ) => {
        this.proposalData.set( data );
      }
    });
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  getColumns() {
    return [{
      label: 'Proposal Id',
      id: 'id'
    }, {
      label: 'Funding Opportunity Name',
      id: 'fundingOpportunityName'
    }, {
      label: 'Applicant Name',
      id: 'applicantName'
    }, {
      label: 'Submission Date',
      id: 'submissionDate'
    }, {
      label: 'Status',
      id: 'status'
    }];
  }

  handleOnOpenRecordDetails(record: any) {
    if( record == null || record == undefined )
    {
      return;
    }
    let URL = window.location.origin + "/#/proposal/"+record["id"];
    window.open( URL, "_blank" );
  }

  _handleOnViewProposalDaialogClose(){}

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  } 
}
