import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component, ElementRef,
  EventEmitter,
  inject,
  OnChanges,
  OnDestroy,
  OnInit, Output,
  output,
  signal, ViewChild,
  Input, SimpleChanges
} from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';

import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../../../common/services/language.service';
import { Subscription } from 'rxjs/internal/Subscription';
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
import { DialogModule } from 'primeng/dialog';
import { DialogService, DynamicDialogComponent, DynamicDialogRef } from 'primeng/dynamicdialog';
import {Partner} from '../../../models/partner.model';

@Component({
  selector: 'app-partner-new',
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
    ReactiveFormsModule,
    DialogModule
  ],
  templateUrl: './partner-new.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerNewComponent implements OnChanges, OnInit {
  @Input() partnerData: Partner | null = null;
  @Input() public record: any = {};
  @Output() closeModal = new EventEmitter<void>();
  display = true;

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
  languageService = inject(LanguageService);
  cdr = inject( ChangeDetectorRef);

  private langChangeSubscription: Subscription = new Subscription();
  @Output() onRecordCreationSuccess = new EventEmitter<any>();

  showValidationFailedError = signal<boolean>(false);
  allPartnerStatusData = this.cachedDataService.allPartnerStatus;
  allPartnerNewEngagementData = this.cachedDataService.allPartnerNewEngagement;
  allPartnerReportingLevelData = this.cachedDataService.allPartnerReportingLevel;
  allYesNoData = this.cachedDataService.allYesNo;
  allPartnerLevyAppliesData = this.cachedDataService.allPartnerLevyApplies;
  allPartnerReasonForLevyNotData = this.cachedDataService.allPartnerReasonForLevyNot;
  allPartnerLevyTreatmentData = this.cachedDataService.allPartnerLevyTreatment;
  allPartnerScopesData = this.cachedDataService.allPartnerScope;
  isSaving = signal(false);

  ngOnInit() {
    this.updateForm(this.partnerData)
  }

  ngOnChanges(changes: SimpleChanges) {
    this.display = true;

    if (changes['partnerData'] && this.partnerData) {
      this.updateForm(this.partnerData);
    } else if (Object.keys(this.record).length > 0) {
      this.updateForm(this.record);
    }

  }

  updateForm(data: Partner | null) {
    if (!data) return;
    this.formGroup.patchValue(data as any);
  }

  _handleOnSaveClick(){
    let canSave = this._validate();

    canSave = true;

    if( canSave === true )
    {
      this.isSaving.set(true)
      this.partnerService.createPartner(this._getRequestPayload()).subscribe({
        next: (data: any) => {
          this.isSaving.set(false);
          this.feedbackDialogService.showSuccessToast({ detail: 'Record created successfully!' });
          this.onRecordCreationSuccess.emit(data);
          this.hide();
        },
        error: () => this.isSaving.set(false)
      });
    }
  }

  hide(): void {
    this.display = false;
    this.closeModal.emit();
  }

  _validate(){
    let result = true;

    if( this.formGroup.invalid )
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
