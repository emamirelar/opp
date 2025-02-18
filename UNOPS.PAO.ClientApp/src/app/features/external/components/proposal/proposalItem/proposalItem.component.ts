import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { ProposalService } from '../../../services/proposal.service';
import { WorkflowService } from './../../../../../common/services/workflow.service';
import { Component, inject, signal } from '@angular/core';
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
import { MenuItem } from 'primeng/api';
import { BlockUI } from 'primeng/blockui';
import { NgIf } from '@angular/common';

import { DocumentUploadComponent } from '../../../../../common/reusables/components/document-upload/document-upload.component';
import { FundingOpportunityOverviewComponent } from '../../fundingOpportunity/fundingOpportunityItem/fundingOpportunityOverview.component';
import { WorkflowComponent } from '../../../../../common/reusables/components/workflow/workflow.component';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { DocumentService } from '../../../../internal/services/document.service';
import { DocumentType } from '../../../../internal/overrides/interfaces/types';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-proposalItem',
  templateUrl: './proposalItem.component.html',
  styleUrls: ['./proposalItem.component.css'],
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
    FundingOpportunityOverviewComponent,
    DocumentUploadComponent,
    WorkflowComponent,
    BlockUI,
    NgIf,
    TranslateModule,
  ],
})
export class ProposalItemComponent {
  formGroup = new FormGroup({
    name: new FormControl(''),
    eligibilityEntityMet: new FormControl(false),
    eligibilityCriteriaMet: new FormControl(false),
    stage: new FormControl(''),
  });

  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  proposalService = inject(ProposalService);
  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  workflowService = inject(WorkflowService);
  documentService = inject(DocumentService);

  recordId: string = '';
  recordData = signal<any>({});
  stages = signal<MenuItem[] | []>([]);
  currentStageIndex: number = -1;
  fundingOpportunityId = signal<string>('');

  constructor(private feedback: FeedbackDialogService) {
    //load stages
    this.workflowService.getWorkFlowForEntity('proposal').subscribe({
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
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        this.recordId = paramMap.get('recordId') || '';

        if (this.recordId != '') {
          this._loadRecordDetails();
        }
      },
    });

    this.formGroup.get('stage')?.valueChanges.subscribe((value) => {
      const eligibilityEntityMetControl = this.formGroup.get(
        'eligibilityEntityMet',
      );
      const eligibilityCriteriaMetControl = this.formGroup.get(
        'eligibilityCriteriaMet',
      );
      if (value === 'Submitted') {
        eligibilityEntityMetControl?.disable();
        eligibilityCriteriaMetControl?.disable();
      } else {
        eligibilityEntityMetControl?.enable();
        eligibilityCriteriaMetControl?.enable();
      }
    });
  }

  _loadRecordDetails() {
    //fetch record details
    this.proposalService.getRecordDetailsById(this.recordId).subscribe({
      next: (data: any) => {
        this.recordData.set(data);

        this.formGroup.patchValue(data);

        //set current stage index
        this.currentStageIndex = this.stages().findIndex(function (item) {
          return item['name'] == data['stage'];
        });

        this.fundingOpportunityId.set(this.recordData().fundingOpportunityId);
      },
    });
  }

  handleOnCancelClick(event: MouseEvent) {
    this.router.navigate([
      'external/funding-opportunity/' + this.recordData().fundingOpportunityId,
    ]);
  }

  handleOnSaveClick(event: MouseEvent) {
    this.proposalService
      .updateRecordDetailsById(this._getRequestPayload())
      .subscribe({
        next: (data: any) => {
          this._loadRecordDetails();
          this.feedbackDialogService.showSuccessToast({
            detail: 'Changes saved successfully!',
          });
        },
      });
  }

  _getRequestPayload() {
    let valueObj = this.formGroup.value,
      requestJsonObj: any = {};

    for (let key in valueObj) {
      if (valueObj.hasOwnProperty(key)) {
        let indexValue = (valueObj as any)[key];

        switch (key) {
          default:
            requestJsonObj[key] = indexValue;
            break;
        }
      }
    }

    requestJsonObj['id'] = this.recordId;

    return requestJsonObj;
  }

  validateAndSaveBeforeStageChange(
    validationFormGroup: FormGroup,
  ): Promise<boolean> {
    return new Promise((resolve) => {
      if (
        validationFormGroup.get('eligibilityEntityMet')?.value &&
        validationFormGroup.get('eligibilityCriteriaMet')?.value
      ) {
        this.proposalService
          .updateRecordDetailsById(this._getRequestPayload())
          .subscribe({
            next: (data: any) => {
              resolve(true);
            },
            error: (error: any) => {
              this.feedbackDialogService.showErrorToast({
                detail: 'Failed to save changes. Please try again.',
              });

              resolve(false);
            },
          });
      } else {
        this.feedbackDialogService.showErrorToast({
          detail:
            'Meet eligible entities and Meet eligible criteria are required to submit the Proposal',
        });

        resolve(false);
      }
    });
  }

  onFileUploaded(response: any) {
    const formData = new FormData();
    for (let file of response.files) {
      formData.append('file', file);
      formData.append('parentEntityType', DocumentType.Proposal.toString());
      formData.append('parentEntityId', this.recordId);
      formData.append('name', file.name);
    }

    this.documentService.uploadFiles(formData).subscribe({
      next: (response: any) => {
        this.feedback.showSuccessToast({
          detail: `File ${response.name} uploaded successfully!`,
        });
      },
      error: (error) => {
        this.feedback.showErrorToast({ detail: 'Unable to upload file!' });
      },
    });
  }

  handleOnStageChange(data: any) {
    //exit condition
    if (data == null || data == '' || data == undefined) {
      return;
    }

    this.formGroup.get('stage')?.setValue(data['stage']);
    this.currentStageIndex = this.stages().findIndex(function (item) {
      return item['name'] == data['stage'];
    });
  }
}
