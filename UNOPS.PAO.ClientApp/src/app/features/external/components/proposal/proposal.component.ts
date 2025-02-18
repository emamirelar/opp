import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';

import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { ProposalService } from '../../services/proposal.service';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { LanguageService } from '../../../../common/services/language.service';

interface columnDefination {
  label: string,
  id: string
}

@Component({
  selector: 'app-proposal',
  templateUrl: './proposal.component.html',
  styleUrls: ['./proposal.component.css'],
  imports: [PanelModule, ButtonModule, TableModule, ScrollPanelModule, DatePipe, ProgressSpinnerModule, TranslateModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProposalComponent implements OnInit, OnDestroy {
  private langChangeSubscription: Subscription = new Subscription;
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);

  proposalService = inject(ProposalService);

  columns = signal<columnDefination[]>([]);

  opportunityId: string = '';
  proposalData = this.proposalService.applicantProposals;
  isDataLoading = this.proposalService.isLoading;

  constructor(public translationService : TranslateService, private languageService: LanguageService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        //initialize columns
        this.columns.update(() => {
          return this.getColumns();
        });
        //make server call to get all proposals.
        this.proposalService.getApplicantProposals();
      }
    });
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  getColumns() {
    return [{
      label: this.translationService.instant('label.proposal.id'),
      id: 'id'
    }, {
      label: this.translationService.instant('label.proposal.fundingOpportunityName'),
      id: 'fundingOpportunityName'
    }, {
      label: this.translationService.instant('label.proposal.fundingOpportunitySubmissionDueDate'),
      id: 'fundingOpportunitySubmissionDueDate'
    }, {
      label: this.translationService.instant('label.proposal.eligibilityCriteriaMet'),
      id: 'eligibilityCriteriaMet'
    }, {
      label: this.translationService.instant('label.proposal.submissionDate'),
      id: 'submissionDate'
    }, {
      label: this.translationService.instant('label.stage'),
      id: 'stage'
    }];
  }

  handleOnOpenRecordDetails(record: any) {
    this.router.navigate(['external/proposal', record.id]);
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  } 
}
