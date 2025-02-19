import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';

import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { FundingOpportunityService } from '../../services/fundingOpportunity.service';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../../common/services/language.service';
import { Subscription } from 'rxjs/internal/Subscription';

interface columnDefination{
  label: string,
  id: string
}

@Component({
  selector: 'app-fundingOpportunity',
  templateUrl: './fundingOpportunity.component.html',
  styleUrls: ['./fundingOpportunity.component.css'],
  imports: [PanelModule, ButtonModule, TableModule, ScrollPanelModule, DatePipe, DecimalPipe, ProgressSpinnerModule, TranslateModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FundingOpportunityComponent implements OnInit, OnDestroy {
  private langChangeSubscription: Subscription = new Subscription;
  router = inject( Router );
  fundingOpportunityService = inject( FundingOpportunityService );

  columns = signal<columnDefination[]>([]);

  fundingOpportunityData = this.fundingOpportunityService.allFundingOpportunities;
  isDataLoading = this.fundingOpportunityService.isLoading;

  constructor(public translationService: TranslateService, private languageService: LanguageService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    //initialize columns
    this.columns.update( () => {
      return this.getColumns();
    } );

    //make server call to get all funding opportunities.
    this.fundingOpportunityService.getAllFundingOppotunities();
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  getColumns() {
    return [{
      label: 'label.fundingOpportunity.name',
      id: 'name'
    },{
      label: 'label.fundingOpportunity.description',
      id: 'description'
    },{
      label: 'label.fundingOpportunity.currency',
      id: 'currency'
    },{
      label: 'label.fundingOpportunity.availableFunding',
      id: 'fundingAvailable'
    },{
      label: 'label.fundingOpportunity.applicationType',
      id: 'applicationType'
    },{
      label: 'label.fundingOpportunity.submissionDueDate',
      id: 'submissionDueDate'
    }];
  }

  handleOnOpenRecordDetails(record: any) {
    this.router.navigate(['external', 'funding-opportunity', record.id]);
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  } 
}
