import { Component, inject, signal, input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PanelModule } from 'primeng/panel';
import { FluidModule } from 'primeng/fluid';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { StepperModule } from 'primeng/stepper';
import { CardModule } from 'primeng/card';
import { StepsModule } from 'primeng/steps';
import { CheckboxModule } from 'primeng/checkbox';
import { DatePicker, DatePickerModule } from 'primeng/datepicker';
import { MenuItem } from 'primeng/api';

import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { WorkflowService } from '../../../../../common/services/workflow.service';
import { FundingOpportunityService } from '../../../services/fundingOpportunity.service';

import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-fundingOpportunityOverview',
  templateUrl: './fundingOpportunityOverview.component.html',
  styleUrls: ['./fundingOpportunityOverview.component.css'],
  imports: [
    PanelModule,
    FluidModule,
    InputTextModule,
    ButtonModule,
    TextareaModule,
    MultiSelectModule,
    SelectModule,
    InputNumberModule,
    StepperModule,
    CardModule,
    StepsModule,
    CheckboxModule,
    ReactiveFormsModule,
    DatePickerModule,
    DatePicker,
    TranslateModule,
  ],
})
export class FundingOpportunityOverviewComponent {
  showHeader = input<boolean>(false);
  fundingOpportunityId = input.required<string>();

  formGroup = new FormGroup({
    name: new FormControl({ value: '', disabled: true }),
    description: new FormControl({ value: '', disabled: true }),
    sdGs: new FormControl({ value: [], disabled: true }),
    eligibleEntities: new FormControl({ value: [], disabled: true }),
    eligibilityCriteria: new FormControl({ value: '', disabled: true }),
    currency: new FormControl({ value: null, disabled: true }),
    fundingAvailable: new FormControl({ value: 0, disabled: true }),
    applicationType: new FormControl({ value: null, disabled: true }),
    postingDate: new FormControl({ value: null, disabled: true }),
    submissionDueDate: new FormControl({ value: null, disabled: true }),
    informationSessionDate: new FormControl({ value: null, disabled: true }),
    clarificationDeadline: new FormControl({ value: null, disabled: true }),
  });

  fundingOpportunityService = inject(FundingOpportunityService);

  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  cachedDataService = inject(CachedDataService);
  workflowService = inject(WorkflowService);

  recordData = signal<any>({});
  stages = signal<MenuItem[] | []>([]);
  currentStageIndex: number = -1;

  allSDGs = this.cachedDataService.allSDGs;
  allEligibleEntities = this.cachedDataService.allEligibleEntities;
  allApplicationTypes = this.cachedDataService.allApplicationTypes;
  allCurrencies = this.cachedDataService.allCurrencies;

  constructor() {
    //fetch SDG list
    this.cachedDataService.loadSDGs();
    //fetch Eligible Entities
    this.cachedDataService.loadEligibleEntities();
    //load application types
    this.cachedDataService.loadApplicationTypes();
    //load Currencies
    this.cachedDataService.loadCurrencies();

    //load stages
    this.workflowService
      .getWorkFlowForEntity('external-funding-opportunity')
      .subscribe({
        next: (data: any) => {
          let stageArray: [] = data || [];
          if (stageArray) {
            this.stages.set(
              stageArray.map((item) => {
                return {
                  label: item['displayName'],
                  name: item['stage'],
                  value: item['sequence'],
                };
              }),
            );
          }
        },
      });
  }

  ngOnInit() {
    this._loadRecordDetails();
  }

  ngOnChanges() {
    this._loadRecordDetails();
  }

  _loadRecordDetails() {
    //fetch record details
    if (this.fundingOpportunityId() != '') {
      this.fundingOpportunityService
        .getRecordDetailsById(this.fundingOpportunityId())
        .subscribe({
          next: (data: any) => {
            this.recordData.set(data);

            this.formGroup.patchValue(data);

            this._loadWorkflowStages();
          },
        });
    }
  }

  _loadWorkflowStages() {
    this.workflowService
      .getWorkFlowForEntity('external-funding-opportunity')
      .subscribe({
        next: (data: any) => {
          let curStage = this.recordData().stage;
          let stageArray: [] = data || [];

          if (stageArray) {
            this.stages.set(
              stageArray.map((item) => {
                return {
                  label: item['displayName'],
                  name: item['stage'],
                  value: item['sequence'],
                };
              }),
            );

            this.currentStageIndex = this.stages().findIndex(function (item) {
              return item['name'] == curStage;
            });
          }
        },
      });
  }
}
