import { afterNextRender, ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, output, signal } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown"; 
import { DatePickerModule } from 'primeng/datepicker';


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
import { BlockUI } from 'primeng/blockui';
import { MessageModule } from 'primeng/message';
import { PartnerService } from '../../../services/partner.service';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';

@Component({
  selector: 'app-new-partner',
  imports: [
    TranslateModule,
    InputTextModule,
    DropdownModule,
    DatePickerModule,
    ButtonModule,
    TextareaModule,
    PanelModule,
    SelectModule,
    AutoFocusModule,
    BlockUI,
    MessageModule,
    DividerModule,
    CardModule,
    CheckboxModule,
    ReactiveFormsModule],
  templateUrl: './new-partner.component.html',
  styleUrl: './new-partner.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NewPartnerComponent implements OnInit, OnDestroy {
  formGroup = new FormGroup({
    name: new FormControl('', {
      validators:[Validators.required]
    }),
    status: new FormControl(null, {
      validators:[Validators.required]
    }),
    newEngagement: new FormControl(null, {
      validators:[Validators.required]
    }),
    phone: new FormControl(null, {
      validators:[Validators.required]
    }),
    website: new FormControl(null, {
      validators:[Validators.required]
    }),
    shortName: new FormControl(null, {
      validators:[Validators.required]
    }),
    internalReportingLevel: new FormControl(null, {
      validators:[Validators.required]
    }),
    externalReportingLevel: new FormControl(null, {
      validators:[Validators.required]
    }),
    pooledFund: new FormControl(null, {
      validators:[Validators.required]
    }),
    ddRequired: new FormControl(null, {
      validators:[Validators.required]
    }),
    ddeacDone: new FormControl(null, {
      validators:[Validators.required]
    }),
    eacReference: new FormControl(null, {
      validators:[Validators.required]
    }),
    globalKeyAccount: new FormControl(false),
    unSecretariatEntity: new FormControl(false),
    levyPotentiallyApplies: new FormControl(null, {
      validators:[Validators.required]
    }),
    reasonForLevyNotApplying: new FormControl(null, {
      validators:[Validators.required]
    }),
    levyTreatment: new FormControl(null, {
      validators:[Validators.required]
    }),
    scope: new FormControl(null, {
      validators:[Validators.required]
    }),
    address1Street: new FormControl(null, {
      validators:[Validators.required]
    }),
    address1Street2: new FormControl(null, {
      validators:[Validators.required]
    }),
    address1City: new FormControl(null, {
      validators:[Validators.required]
    }),
    address1StateProvince: new FormControl(null, {
      validators:[Validators.required]
    }),
    address1PostalCode: new FormControl(null, {
      validators:[Validators.required]
    }),
    address1Country: new FormControl(null, {
      validators:[Validators.required]
    }),
    address2Street: new FormControl(null, {
      validators:[Validators.required]
    }),
    address2Street2: new FormControl(null, {
      validators:[Validators.required]
    }),
    address2City: new FormControl(null, {
      validators: [Validators.required]
    }),
    address2StateProvince: new FormControl(null, {
      validators: [Validators.required]
    }),
    address2PostalCode: new FormControl(null, {
      validators: [Validators.required]
    }),
    address2Country: new FormControl(null, {
      validators: [Validators.required]
    }),
    discriminator: new FormControl(null, {
      validators: [Validators.required]
    }),
    createdBy: new FormControl(null, {
      validators:[Validators.required]
    }),
    createdDate: new FormControl(new Date(), {
      validators:[Validators.required]
    }),
    lastModifiedBy: new FormControl(null, {
      validators:[Validators.required]
    }),
    lastModifiedDate: new FormControl(new Date(), {
      validators:[Validators.required]
    }),
    isDeleted: new FormControl(null, {
      validators:[Validators.required]
    }),
    deletedBy: new FormControl(null, {
      validators:[Validators.required]
    }),
    deletedDate: new FormControl(null, {
      validators:[Validators.required]
    }),
  });

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  partnerService = inject(PartnerService);
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
  allPartnerReportingLevelData = this.cachedDataService.allPartnerReportingLevel;
  allYesNoData = this.cachedDataService.allYesNo;
  allPartnerLevyAppliesData = this.cachedDataService.allPartnerLevyApplies;
  allPartnerReasonForLevyNotData = this.cachedDataService.allPartnerReasonForLevyNot;
  allPartnerLevyTreatmentData = this.cachedDataService.allPartnerLevyTreatment;
  allPartnerScopesData = this.cachedDataService.allPartnerScope;

  constructor() {
    //load salutations
    //this.cachedDataService.loadSalutations();
    //this.cachedDataService.loadStatus();
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  ngOnInit() {
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  _handleOnSaveClick(){
    let canSave = this._validate();

    canSave = true;

    if( canSave === true )
    {
      this.partnerService.createPartner(this._getRequestPayload()).subscribe({
        next: (data: any) => {
          this.feedbackDialogService.showSuccessToast({ detail: 'Record created successfully!' });
          this.onRecordCreationSuccess.emit(data);
        }
      });
    }
  }

  _validate(){
    let result = true;

    if( this.formGroup.status == "INVALID" )
    {
      this.showValidationFailedError.set( false );

      if( this.formGroup.get("name")?.invalid )
      {
        this.formGroup.get("name")?.markAsDirty();
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

    return requestJsonObj;
  }
}
