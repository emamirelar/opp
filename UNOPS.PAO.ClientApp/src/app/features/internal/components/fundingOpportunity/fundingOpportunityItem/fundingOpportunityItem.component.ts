import { WorkflowService } from './../../../../../common/services/workflow.service';
import { ChangeDetectorRef, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PanelModule } from 'primeng/panel';
import { FluidModule } from 'primeng/fluid';
import { InputTextModule } from 'primeng/inputtext';
import { FloatLabelModule } from 'primeng/floatlabel';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { StepperModule } from 'primeng/stepper';
import { CardModule } from 'primeng/card';
import { StepsModule } from 'primeng/steps';
import { DatePicker, DatePickerModule } from 'primeng/datepicker';
import { DatePipe } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { DocumentUploadComponent } from '../../../../../common/reusables/components/document-upload/document-upload.component';
import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { DocumentService } from '../../../services/document.service';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FundingOpportunityService } from '../../../services/fundingOpportunity.service';
import { MenuItem } from 'primeng/api';
import { BlockUI } from 'primeng/blockui';
import { DialogModule } from 'primeng/dialog';
import { WorkflowComponent } from "../../../../../common/reusables/components/workflow/workflow.component";
import { FundingOpportunityProposalComponent } from "../fundingOpportunityProposal/fundingOpportunityProposal.component";
import { DriveDocumentUploadComponent } from '../../../overrides/reusables/drive-document-upload/drive-document-upload.component';
import { DocumentLinkModel } from '../../../overrides/interfaces/types';
import { DocumentType } from '../../../overrides/interfaces/types';
import { DocumentListComponent } from '../../../../../common/reusables/components/document-list/document-list.component';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { LanguageService } from '../../../../../common/services/language.service';
@Component({
  selector: 'app-fundingOpportunityItem',
  templateUrl: './fundingOpportunityItem.component.html',
  styleUrls: ['./fundingOpportunityItem.component.css'],
  imports: [PanelModule,
    FluidModule,
    InputTextModule,
    FloatLabelModule,
    ButtonModule,
    TextareaModule,
    DocumentUploadComponent,
    MultiSelectModule,
    SelectModule,
    InputNumberModule,
    StepperModule,
    CardModule,
    StepsModule,
    DatePickerModule,
    DatePicker,
    ReactiveFormsModule,
    DatePipe,
    WorkflowComponent,
    BlockUI,
    DriveDocumentUploadComponent,
    DialogModule,
    DocumentListComponent,
    TranslateModule,
    FundingOpportunityProposalComponent]
})
export class FundingOpportunityItemComponent implements OnInit {
  formGroup = new FormGroup({
    name: new FormControl(''),
    description: new FormControl(''),
    sdGs: new FormControl([]),
    eligibleEntities: new FormControl([]),
    eligibilityCriteria: new FormControl(''),
    currency: new FormControl(null),
    fundingAvailable: new FormControl(0),
    applicationType: new FormControl(null),
    selectionMethodology: new FormControl(null),
    justification: new FormControl(''),
    project: new FormControl(null),
    countries: new FormControl([]),
    submissionDueDate: new FormControl<Date | null>(null),
    informationSessionDate: new FormControl<Date | null>(null),
    decisionDate: new FormControl<Date | null>(null),
    clarificationDeadline: new FormControl<Date | null>(null)
  });

  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  fundingOpportunityService = inject(FundingOpportunityService);
  documentService = inject(DocumentService);
  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  workflowService = inject(WorkflowService);

  recordId: string = '';
  recordData = signal<any>({});
  minSubmissionDate: Date | undefined;
  stages = signal<MenuItem[] | []>([]);
  currentStageIndex: number = -1;
  showCommentDialog = false;

  allSDGs = this.cachedDataService.allSDGs;
  allEligibleEntities = this.cachedDataService.allEligibleEntities;
  allApplicationTypes = this.cachedDataService.allApplicationTypes;
  allProjects = this.cachedDataService.allProjects;
  allCountries = this.cachedDataService.allCountries;
  allCurrencies = this.cachedDataService.allCurrencies;
  allSelectionMethodologies = this.cachedDataService.allSelectionMethodologies;


  constructor(private feedback: FeedbackDialogService) {
    //fetch SDG list
    this.cachedDataService.loadSDGs();
    //fetch Eligible Entities
    this.cachedDataService.loadEligibleEntities();
    //load application types
    this.cachedDataService.loadApplicationTypes();
    //load projects
    this.cachedDataService.loadProjects();
    //load countries
    this.cachedDataService.loadCountries();
    //load Currencies
    this.cachedDataService.loadCurrencies();
    //load selection methodologies
    this.cachedDataService.loadSelectionMethodologies();

    //load stages
    this.workflowService.getWorkFlowForEntity('funding-opportunity').subscribe({
      next: (data: any) => {
        let stageArray: [] = data || [];
        if (stageArray) {
          this.stages.set(stageArray.map((item) => {
            return {
              label: item["displayName"],
              name: item["stage"],
              value: item["sequence"]
            };
          }));
        }
      }
    });
  }

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        this.recordId = paramMap.get("recordId") || '';

        if (this.recordId != '') {
          this._loadRecordDetails();
        }
      }
    });

    this.activatedRoute.queryParamMap.subscribe({
      next: (paramMap) => {
        if ((this.recordId != '') && (paramMap.get("show-proposals")?.toLowerCase() == "true")) {
          this._handleOnViewProposals();
        }
        else {
          this.showCommentDialog = false;
        }
      }
    });
  }

  _loadRecordDetails() {

    //fetch record details
    this.fundingOpportunityService.getRecordDetailsById(this.recordId).subscribe({
      next: (data: any) => {

        if (data["postingDate"] !== null && data["postingDate"] !== '') {
          this.minSubmissionDate = new Date(data["postingDate"]);
        }
        if (data["submissionDueDate"] !== null && data["submissionDueDate"] !== '') {
          data["submissionDueDate"] = new Date(data["submissionDueDate"]);
        }
        if (data["informationSessionDate"] !== null && data["informationSessionDate"] !== '') {
          data["informationSessionDate"] = new Date(data["informationSessionDate"]);
        }
        if (data["decisionDate"] !== null && data["decisionDate"] !== '') {
          data["decisionDate"] = new Date(data["decisionDate"]);
        }
        if (data["clarificationDeadline"] !== null && data["clarificationDeadline"] !== '') {
          data["clarificationDeadline"] = new Date(data["clarificationDeadline"]);
        }

        //set current stage index
        this.currentStageIndex = this.stages().findIndex(function (item) {
          return item["name"] == data["stage"];
        });

        this.recordData.set(data);

        this.formGroup.patchValue(data);
      }
    });

  }

  handleOnCancelClick(event: MouseEvent) {
    this.router.navigate(['funding-opportunity']);
  }

  handleOnSaveClick(event: MouseEvent) {

    this.fundingOpportunityService.updateRecordDetailsById(this._getRequestPayload()).subscribe({
      next: (data: any) => {
        this._loadRecordDetails();
        this.feedbackDialogService.showSuccessToast({ detail: 'Changes saved successfully!' });
      }
    });
  }

  onFileUploaded(response: any) {
    const formData = new FormData();
    for (let file of response.files) {
      formData.append('file', file);
      formData.append('parentEntityType', DocumentType.FundingOpportunity.toString());
      formData.append('parentEntityId', this.recordId);
      formData.append('name', file.name);
    }

    this.documentService.uploadUnopsFiles(formData).subscribe({
      next: (response: any) => {
        this.feedback.showSuccessToast({ detail: `File ${response.name} uploaded successfully!` });
      },
      error: (error) => {
        this.feedback.showErrorToast({ detail: 'Unable to upload file!' });
      }
    });
  }

  onDriveFileUploaded(response: any) {
    // TODO: allow more than one file to be uploaded if multiple is set to true
    const file = response[0]
    const req: DocumentLinkModel = {
      link: file.url,
      name: file.name,
      type: file.mimeType,
      parentEntityType: DocumentType.FundingOpportunity,
      parentEntityId: parseInt(this.recordId)
    };

    this.documentService.linkUnopsFiles(req).subscribe({
      next: (response: any) => {
        this.feedback.showSuccessToast({ detail: `File ${response.name} uploaded successfully!` });
      },
      error: (error) => {
        this.feedback.showErrorToast({ detail: 'Unable to upload file!' });
      }
    });
  }

  onFileSelected(event: any) {
    console.log('Files selected:', event);
  }

  onFileRemoved(event: any) {
    console.log('File removed:', event);
  }

  onFilesCleared() {
    console.log('All files cleared');
  }

  _getRequestPayload() {
    let valueObj = this.formGroup.value,
      requestJsonObj: any = {},
      sdgIds: never[] = [],
      countryIds: never[] = [],
      projectNumber = "",
      eligibleEntityIds: never[] = [],
      selectionMethodologyId = "";

    for (let key in valueObj) {
      if (valueObj.hasOwnProperty(key)) {
        let indexValue = (valueObj as any)[key];
        switch (key) {
          case "sdGs":
            if (valueObj["sdGs"] != null && valueObj["sdGs"].length > 0) {
              sdgIds = valueObj["sdGs"].map((item) => { return item["id"] });
            }
            requestJsonObj["SDGIds"] = sdgIds;
            break;

          case "countries":
            if (valueObj["countries"] != null && valueObj["countries"].length > 0) {
              countryIds = valueObj["countries"].map((item) => { return item["id"] });
            }
            requestJsonObj["countryIds"] = countryIds;
            break;

          case "eligibleEntities":
            if (valueObj["eligibleEntities"] != null && valueObj["eligibleEntities"].length > 0) {
              eligibleEntityIds = valueObj["eligibleEntities"].map((item) => { return item["id"] });
            }
            requestJsonObj["eligibleEntityIds"] = eligibleEntityIds;
            break;

          case "project":
            if (valueObj["project"] != null && valueObj["project"] !== undefined) {
              projectNumber = valueObj["project"]["projectNumber"];
            }
            requestJsonObj["projectNumber"] = projectNumber;
            break;

          case "selectionMethodology":
            if (valueObj["selectionMethodology"] != null && valueObj["selectionMethodology"] !== undefined) {
              selectionMethodologyId = valueObj["selectionMethodology"]["id"];
            }
            requestJsonObj["selectionMethodologyId"] = selectionMethodologyId;
            break;

          case "applicationType":
            if (valueObj["applicationType"] != null && valueObj["applicationType"] !== undefined) {
              requestJsonObj["ApplicationTypeCode"] = valueObj["applicationType"]["id"];
            }
            break;

          case "currency":
            if (valueObj["currency"] != null && valueObj["currency"] !== undefined) {
              requestJsonObj["currencyCode"] = valueObj["currency"]["code"];
            }
            break;

          default:
            requestJsonObj[key] = indexValue;
            break;
        }
      }
    }

    requestJsonObj["id"] = this.recordId;

    return requestJsonObj;
  }

  handleOnStageChange(data: any) {
    //exit condition
    if (data == null || data == '' || data == undefined) {
      return;
    }

    this.currentStageIndex = this.stages().findIndex(function (item) {
      return item["name"] == data["stage"];
    });
  }

  _handleOnViewProposals() {
    this.showCommentDialog = true;

    this.router.navigate(
      [],
      {
        relativeTo: this.activatedRoute,
        queryParams: {
          "show-proposals": true
        }
      }
    );
  }

  _handleOnViewProposalDaialogClose() {
    this.showCommentDialog = false;

    this.router.navigate(
      [],
      {
        relativeTo: this.activatedRoute,
        queryParams: {}
      }
    );
  }
}
