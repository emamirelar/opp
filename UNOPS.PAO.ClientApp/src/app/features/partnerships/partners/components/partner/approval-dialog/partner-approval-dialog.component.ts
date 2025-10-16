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
import { PartnerService } from '@partnerships/partners/services/partner.service';
import { CachedDataService } from '@shared/services/utils';
import { FeedbackDialogService } from '@shared/services/ui';

// Models
import { Partner } from '@partnerships/partners/models/partner.model';

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
  dueDiligenceRequiredValue = signal<string>('');
  dueDiligenceApprovalValue = signal<string>('');
  
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
      
      const reasonForLevyControl = this.formGroup.get('reasonForLevy');
      
      // Clear and manage validators for reasonForLevy based on visibility
      if (value === 'DoesNotApply' || value === 'PotentiallyNotApplied') {
        // Make reasonForLevy required when visible
        reasonForLevyControl?.setValidators([Validators.required]);
      } else {
        // Clear value and validators when hidden
        reasonForLevyControl?.setValue('');
        reasonForLevyControl?.clearValidators();
      }
      
      reasonForLevyControl?.updateValueAndValidity();
    });
    
    // Subscribe to Due Diligence Required changes
    this.formGroup.get('dueDiligenceRequired')?.valueChanges.subscribe(value => {
      this.dueDiligenceRequiredValue.set(value || '');
      
      // Clear Due Diligence Approval fields when Due Diligence is not Required
      if (value !== 'Required') {
        this.formGroup.get('dueDiligenceApproval')?.setValue('');
        this.formGroup.get('dueDiligenceApprovalDate')?.setValue(null);
        this.formGroup.get('dueDiligenceExpiryDate')?.setValue(null);
        this.formGroup.get('dueDiligenceApproval')?.clearValidators();
        this.formGroup.get('dueDiligenceApprovalDate')?.clearValidators();
        this.formGroup.get('dueDiligenceExpiryDate')?.clearValidators();
      }
      this.formGroup.get('dueDiligenceApproval')?.updateValueAndValidity();
      this.formGroup.get('dueDiligenceApprovalDate')?.updateValueAndValidity();
      this.formGroup.get('dueDiligenceExpiryDate')?.updateValueAndValidity();
    });
    
    // Subscribe to Due Diligence Approval changes
    this.formGroup.get('dueDiligenceApproval')?.valueChanges.subscribe(value => {
      this.dueDiligenceApprovalValue.set(value || '');
      
      const approvalDateControl = this.formGroup.get('dueDiligenceApprovalDate');
      const expiryDateControl = this.formGroup.get('dueDiligenceExpiryDate');
      
      if (value === 'Approved') {
        // Make dates required when Approved
        approvalDateControl?.setValidators([Validators.required]);
        expiryDateControl?.setValidators([Validators.required]);
      } else {
        // Clear dates and validators when not Approved
        approvalDateControl?.setValue(null);
        expiryDateControl?.setValue(null);
        approvalDateControl?.clearValidators();
        expiryDateControl?.clearValidators();
      }
      
      approvalDateControl?.updateValueAndValidity();
      expiryDateControl?.updateValueAndValidity();
    });
    
    // Initialize the signals with the current form values
    this.partnerLevyStatusValue.set(this.formGroup.get('partnerLevyStatus')?.value || '');
    this.dueDiligenceRequiredValue.set(this.formGroup.get('dueDiligenceRequired')?.value || '');
    this.dueDiligenceApprovalValue.set(this.formGroup.get('dueDiligenceApproval')?.value || '');
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
      pooledFund: [this.partner.pooledFund || false]
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

  // Show "Due Diligence Approval" only when Due Diligence Required is "Required"
  shouldShowDueDiligenceApproval = computed(() => {
    return this.dueDiligenceRequiredValue() === 'Required';
  });

  // Show "Due Diligence Approval Date" and "Due Diligence Expiry Date" only when Due Diligence Approval is "Approved"
  shouldShowDueDiligenceDates = computed(() => {
    return this.dueDiligenceApprovalValue() === 'Approved';
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
