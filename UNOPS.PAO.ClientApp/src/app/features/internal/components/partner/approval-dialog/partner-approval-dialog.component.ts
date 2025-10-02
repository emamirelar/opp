import { Component, OnInit, signal, inject, ChangeDetectionStrategy, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';

// PrimeNG imports
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DialogModule } from 'primeng/dialog';
import { DatePickerModule } from 'primeng/datepicker';
import { TextareaModule } from 'primeng/textarea';

// Services
import { PartnerService } from '../../../services/partner.service';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FeedbackDialogService } from '../../../../../common/services/feedback-dialog.service';

// Models
import { Partner } from '../../../models/partner.model';

/**
 * Partner Approval Dialog Component
 * Allows admin users to approve partners and fill approval-related fields
 */
@Component({
  selector: 'app-partner-approval-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    InputTextModule,
    SelectModule,
    CheckboxModule,
    ButtonModule,
    MessageModule,
    ProgressSpinnerModule,
    DialogModule,
    DatePickerModule,
    TextareaModule
  ],
  templateUrl: './partner-approval-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerApprovalDialogComponent implements OnInit {
  partnerService = inject(PartnerService);
  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  
  formGroup!: FormGroup;
  isLoading = signal(false);
  showValidationFailedError = signal(false);
  partnerLevyStatusValue = signal<string>('');
  
  partner: Partner;

  constructor(
    private fb: FormBuilder,
    public dialogRef: DynamicDialogRef,
    public dialogConfig: DynamicDialogConfig
  ) {
    this.partner = this.dialogConfig.data.partner;
  }

  ngOnInit(): void {
    this.initializeForm();
    
    // Subscribe to form changes to update the signal for reactive computed properties
    this.formGroup.get('partnerLevyStatus')?.valueChanges.subscribe(value => {
      this.partnerLevyStatusValue.set(value || '');
      
      // Clear reasonForLevy when it should be hidden
      if (value !== 'DoesNotApply' && value !== 'PotentiallyNotApplied') {
        this.formGroup.get('reasonForLevy')?.setValue('');
      }
    });
    
    // Initialize the signal with the current form value
    this.partnerLevyStatusValue.set(this.formGroup.get('partnerLevyStatus')?.value || '');
  }

  initializeForm(): void {
    this.formGroup = this.fb.group({
      // Exact order as specified with backend field names
      keyGlobalPartner: [this.partner.keyGlobalPartner || false],
      unAndStateEntity: [this.partner.unAndStateEntity || false],
      unSecretariatPartner: [this.partner.unSecretariatPartner || false],
      dueDiligenceRequired: [this.partner.dueDiligenceRequired || ''],
      dueDiligenceApproval: [this.partner.dueDiligenceApproval || ''],
      dueDiligenceApprovalDate: [this.partner.dueDiligenceApprovalDate || null],
      dueDiligenceExpiryDate: [this.partner.dueDiligenceExpiryDate || null],
      partnerApprovalDate: [this.partner.partnerApprovalDate || null],
      partnerApprovalReference: [this.partner.partnerApprovalReference || ''],
      partnerLevyStatus: [this.partner.partnerLevyStatus || ''],
      reasonForLevy: [this.partner.reasonForLevy || ''],
      levyTreatment: [this.partner.levyTreatment || ''],
      pooledFund: [this.partner.pooledFund || false],
      canCreateNewOpportunities: [this.partner.canCreateNewOpportunities || false],
      reasonForNoNewOpportunity: [this.partner.reasonForNoNewOpportunity || '', Validators.required]
    });
  }

  // Cached data - these match the edit dialog
  allDueDiligenceRequiredData = this.cachedDataService.allDueDiligenceRequired;
  allDueDiligenceApprovalData = this.cachedDataService.allDueDiligenceApproval;
  allPartnerLevyAppliesData = this.cachedDataService.allPartnerLevyApplies;
  allPartnerReasonForLevyNotData = this.cachedDataService.allPartnerReasonForLevyNot;
  allPartnerLevyTreatmentData = this.cachedDataService.allPartnerLevyTreatment;

  // Show "Reason for Levy" only when Partner Levy is "DoesNotApply" or "PotentiallyNotApplied"
  shouldShowReasonForLevy = computed(() => {
    const partnerLevyStatus = this.partnerLevyStatusValue();
    return (partnerLevyStatus === 'DoesNotApply' || partnerLevyStatus === 'PotentiallyNotApplied');
  });

  handleApprove(): void {
    if (this.formGroup.invalid) {
      this.showValidationFailedError.set(true);
      return;
    }

    this.isLoading.set(true);
    
    const payload = this._getRequestPayload();

    this.partnerService.approvePartner(payload).subscribe({
      next: (data: any) => {
        this.isLoading.set(false);
        this.feedbackDialogService.showSuccessToast({ detail: 'Partner approved successfully!' });
        this.dialogRef.close(data);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.feedbackDialogService.showErrorToast({ detail: 'Failed to approve partner' });
      }
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

    // Add required fields for approval
    requestJsonObj['id'] = this.partner.id;

    return requestJsonObj;
  }

  handleCancel(): void {
    this.dialogRef.close();
  }
}
