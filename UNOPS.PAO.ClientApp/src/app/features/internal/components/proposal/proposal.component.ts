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
import { LanguageService } from '../../../../common/services/language.service';
import { Subscription } from 'rxjs';

interface columnDefination {
  label: string,
  id: string
}

@Component({
  selector: 'app-proposal',
  templateUrl: './proposal.component.html',
  styleUrls: ['./proposal.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [PanelModule, ButtonModule, TableModule, ScrollPanelModule, DatePipe, ProgressSpinnerModule, TranslateModule]
})
export class ProposalComponent implements OnInit, OnDestroy {
  private langChangeSubscription: Subscription = new Subscription;
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);

  proposalService = inject(ProposalService);

  columns = signal<columnDefination[]>([]);

  opportunityId: string = '';
  proposalData = this.proposalService.allProposals;
  isDataLoading = this.proposalService.isLoading;

  constructor(public translateService: TranslateService, private languageService: LanguageService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        //initialize columns
        this.columns.update(() => {
          return this.getColumns();
        });
        //make server call to get all proposals.
        this.proposalService.getAllProposals();
      }
    });
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  getColumns() {
    return [{
      label: 'label.proposal.id',
      id: 'id'
    }, {
      label: 'label.proposal.fundingOpportunityName',
      id: 'fundingOpportunityName'
    }, {
      label: 'label.proposal.applicantName',
      id: 'applicantName'
    }, {
      label: 'label.proposal.applicantId',
      id: 'applicantId'
    }, {
      label: 'label.proposal.eligibilityCriteriaMet',
      id: 'eligibilityCriteriaMet'
    }, {
      label: 'label.proposal.eligibilityEntityMet',
      id: 'eligibilityEntityMet'
    }, {
      label: 'label.proposal.submissionDate',
      id: 'submissionDate'
    }, {
      label: 'label.stage',
      id: 'stage'
    }];
  }

  handleOnOpenRecordDetails(record: any) {
    this.router.navigate(['proposal', record.id]);
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }
}
