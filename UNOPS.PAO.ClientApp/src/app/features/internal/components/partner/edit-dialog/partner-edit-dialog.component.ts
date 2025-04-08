import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, output, signal } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { DocumentUploadComponent } from '../../../../../common/reusables/components/document-upload/document-upload.component';
import { DocumentService } from '../../../services/document.service';
import { DriveDocumentUploadComponent } from '../../../overrides/reusables/components/document/drive/upload/document-drive-upload.component';
import { ParentEntityType } from '../../../overrides/interfaces/types';
import { DocumentLinkModel } from '../../../overrides/interfaces/types';
import { DocumentComponent } from '../../../../../common/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '../../../overrides/reusables/components/document/gdrive/document-gdrive.component';

//Language translation import
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../../../common/services/language.service';
import { Subscription } from 'rxjs/internal/Subscription';

//PrimeNG imports
import { InputTextModule } from 'primeng/inputtext';
import { DividerModule } from 'primeng/divider';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { AutoFocusModule } from 'primeng/autofocus';
import { DialogModule } from 'primeng/dialog';
import { MessageModule } from 'primeng/message';
import { PartnerService } from '../../../services/partner.service';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { ActivatedRoute, Router } from '@angular/router';
import {PartnerContactsComponent} from '../contacts/partner-contacts.component';
import { GeminiService } from '../../../services/gemini.service';
import {MarkdownPipe} from '../../../pipes/markdown.pipe';
import {LinkListComponent} from "../../../../../common/reusables/components/link/list/link-list.component";
import {EntityType} from '../../../../../common/models/link.model';

@Component({
  selector: 'app-partner-edit-dialog',
  imports: [
    TranslateModule,
    InputTextModule,
    DropdownModule,
    DatePickerModule,
    DocumentUploadComponent,
    DriveDocumentUploadComponent,
    DocumentComponent,
    GDriveDocumentComponent,
    ButtonModule,
    TextareaModule,
    PanelModule,
    SelectModule,
    AutoFocusModule,
    DialogModule,
    MessageModule,
    DividerModule,
    CardModule,
    CheckboxModule,
    ReactiveFormsModule,
    PartnerContactsComponent,
    MarkdownPipe,
    LinkListComponent
  ],
  templateUrl: './partner-edit-dialog.component.html',
  styleUrl: './partner-edit-dialog.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerEditDialogComponent implements OnInit {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  recordPermissions = signal<any>({});
  documentService = inject(DocumentService);

  formGroup = new FormGroup({
      id: new FormControl('', {
        validators: [Validators.required]
      }),
      name: new FormControl('', {
        validators: [Validators.required]
      }),
      status: new FormControl(null, {
        validators: [Validators.required]
      }),
      newEngagement: new FormControl(null, {
        validators: [Validators.required]
      }),
      phone: new FormControl(null, {
        validators: [Validators.required]
      }),
      website: new FormControl(null, {
        validators: [Validators.required]
      }),
      shortName: new FormControl(null, {
        validators: [Validators.required]
      }),
      pooledFund: new FormControl(null, {
        validators: [Validators.required]
      }),
      ddRequired: new FormControl(null, {
        validators: [Validators.required]
      }),
      ddeacDone: new FormControl(null, {
        validators: [Validators.required]
      }),
      eacReference: new FormControl(null, {
        validators: [Validators.required]
      }),
      globalKeyAccount: new FormControl(false),
    unSecretariatEntity: new FormControl(false),
      levyPotentiallyApplies: new FormControl(null, {
        validators: [Validators.required]
      }),
      reasonForLevyNotApplying: new FormControl(null, {
        validators: [Validators.required]
      }),
      levyTreatment: new FormControl(null, {
        validators: [Validators.required]
      }),
      address1Street: new FormControl(null, {
        validators: [Validators.required]
      }),
      address1Street2: new FormControl(null, {
        validators: [Validators.required]
      }),
      address1City: new FormControl(null, {
        validators: [Validators.required]
      }),
      address1StateProvince: new FormControl(null, {
        validators: [Validators.required]
      }),
      address1PostalCode: new FormControl(null, {
        validators: [Validators.required]
      }),
      address1Country: new FormControl(null, {
        validators: [Validators.required]
      }),
      discriminator: new FormControl(null, {
        validators:[Validators.required]
      }),
      createdBy: new FormControl(null, {
        validators: [Validators.required]
      }),
      createdDate: new FormControl(new Date(), {
        validators: [Validators.required]
      }),
      lastModifiedBy: new FormControl(null, {
        validators: [Validators.required]
      }),
      lastModifiedDate: new FormControl(new Date(), {
        validators: [Validators.required]
      }),
      isDeleted: new FormControl(null, {
        validators: [Validators.required]
      }),
      deletedBy: new FormControl(null, {
        validators: [Validators.required]
      }),
      deletedDate: new FormControl(null, {
        validators: [Validators.required]
      }),
    });

    cachedDataService = inject(CachedDataService);
    feedbackDialogService = inject(FeedbackDialogService);
    partnerService = inject(PartnerService);
    geminiService = inject(GeminiService);
    translateService = inject(TranslateService);
    languageService = inject(LanguageService);
    cdr = inject( ChangeDetectorRef);

    private langChangeSubscription: Subscription = new Subscription();
    onRecordCreationSuccess = output();

    //allSalutationsData = this.cachedDataService.allSalutations;
    //allPronounsData = this.cachedDataService.allPronouns;
    showValidationFailedError = signal<boolean>(false);
    //maxDate = new Date();
    allPartnerStatusData = this.cachedDataService.allPartnerStatus;
    allPartnerNewEngagementData = this.cachedDataService.allPartnerNewEngagement;
    allYesNoData = this.cachedDataService.allYesNo;
    allPartnerLevyAppliesData = this.cachedDataService.allPartnerLevyApplies;
    allPartnerReasonForLevyNotData = this.cachedDataService.allPartnerReasonForLevyNot;
    allPartnerLevyTreatmentData = this.cachedDataService.allPartnerLevyTreatment;
    allPartnerScopesData = this.cachedDataService.allPartnerScope;
    recordId: string = '';
    recordData = signal<any>({});
    showCommentDialog = false;
    riskProfile = signal<string>('');
    riskIsLoading = signal<boolean>(true);
    summaryOfInteractionsIsLoading = signal<boolean>(true);
    summaryOfInteractions = signal<string>('');
    partnerNewsIsLoading = signal<boolean>(true);
    partnerNews = signal<string>('');
    entityTypePartner =  EntityType.Partner;

    ngOnInit() {
      this.activatedRoute.paramMap.subscribe({
        next: (paramMap) => {
          this.recordId = paramMap.get("recordId") || '';

          if (this.recordId != '') {
            this._loadRecordDetails();
            this._loadGeminiData();
          }
        }
      });

      this.activatedRoute.queryParamMap.subscribe({
        next: (paramMap) => {
          if (this.recordId != '' && paramMap.get('show-contacts')?.toLowerCase() == 'true') {
            this._handleOnViewContacts();
          } else {
            this.showCommentDialog = false;
          }
        },
      });
    }

    /*_loadPermissions() {
      //fetch permissions for record details
      this.partnerService.getRecordDetailPermissionsById(this.recordId).subscribe({
        next: (data: any) => {
          this.recordPermissions.set(data);
        },
      });
    }*/

    _loadRecordDetails() {
      //fetch record details
      this.partnerService.getPartnerById(this.recordId).subscribe({
        next: (data: any) => {
          this.recordData.set(data);
          this.formGroup.patchValue(data);

        }
      });
    }

    _loadGeminiData() {
      this.summaryOfInteractionsIsLoading.set(true);
      this.riskIsLoading.set(true);
      this.geminiService.get(this.recordId, 'partner_interactions_summary').subscribe({
        next: (summary: string) => {
          this.summaryOfInteractions.set(summary);
          this.summaryOfInteractionsIsLoading.set(false);
        },
        error: () => {
          this.summaryOfInteractions.set(this.translateService.instant('errors.failedToLoad'));
          this.summaryOfInteractionsIsLoading.set(false);
        }
      });

      this.geminiService.get(this.recordId, 'partner_risk_profile').subscribe({
        next: (risk: string) => {
          this.riskProfile.set(risk);
          this.riskIsLoading.set(false);
        },
        error: () => {
          this.riskProfile.set(this.translateService.instant('errors.failedToLoad'));
          this.riskIsLoading.set(false);
        }
      });

      this.geminiService.get(this.recordId, 'partner_news').subscribe({
        next: (news: string) => {
          this.partnerNews.set(news);
          this.partnerNewsIsLoading.set(false);
        },
        error: () => {
          this.partnerNews.set(this.translateService.instant('errors.failedToLoad'));
          this.partnerNewsIsLoading.set(false);
        }
      });
    }

    handleOnCancelClick(event: MouseEvent) {
      this.router.navigate(['partners']);
    }

    handleOnSaveClick(event: MouseEvent) {
      this._validate()

      this.partnerService.updatePartnerById(this._getRequestPayload()).subscribe({
        next: (data: any) => {
          this._loadRecordDetails();
          this.feedbackDialogService.showSuccessToast({ detail: 'Changes saved successfully!' });
        }
      });
    }

    _validate(){
      let result = true;

      if( this.formGroup.invalid )
      {
        this.showValidationFailedError.set( false );

        if( this.formGroup.get("firstName")?.invalid )
        {
          this.formGroup.get("firstName")?.markAsDirty();
        }
        result = false;
      }

      return result;
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

    _handleOnViewContacts() {
      this.showCommentDialog = true;

      this.router.navigate([], {
        relativeTo: this.activatedRoute,
        queryParams: {
          'show-contacts': true,
        },
      });
    }

    _handleOnViewContactsDaialogClose() {
      this.showCommentDialog = false;

      this.router.navigate([], {
        relativeTo: this.activatedRoute,
        queryParams: {},
      });
  }

  get acceptedMiMIETypesForgDrive() {
    return 'application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.google-apps.document,application/vnd.google-apps.spreadsheet';
  }

  onFileUploaded(response: any) {
    const formData = new FormData();
    for (let file of response.files) {
      formData.append('file', file);
      formData.append('parentEntityType', ParentEntityType.Partner.toString());
      formData.append('parentEntityId', this.recordId);
      formData.append('name', file.name);
      formData.append('documentTypeId', '1');
    }

    this.documentService.uploadUnopsFiles(formData).subscribe({
      next: (response: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: `File ${response.name} uploaded successfully!` });
      },
      error: (error) => {
        this.feedbackDialogService.showErrorDialog({ detail: 'Unable to upload file!' });
      },
    });
  }

  onDriveFileUploaded(response: any) {
    // TODO: allow more than one file to be uploaded if multiple is set to true
    const file = response[0];
    const req: DocumentLinkModel = {
      link: file.url,
      name: file.name,
      type: file.mimeType,
      parentEntityType: ParentEntityType.Partner,
      parentEntityId: parseInt(this.recordId),
    };

    this.documentService.linkUnopsFiles(req).subscribe({
      next: (response: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: `File ${response.name} uploaded successfully!` });
      },
      error: (error) => {
        this.feedbackDialogService.showErrorDialog({ detail: 'Unable to upload file!' });
      },
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
}
