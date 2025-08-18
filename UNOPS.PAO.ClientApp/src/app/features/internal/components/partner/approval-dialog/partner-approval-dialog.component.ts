import { Component, OnInit, signal, inject, ChangeDetectionStrategy } from '@angular/core';
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

// Services
import { PartnerService } from '../../../services/partner.service';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';

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
    DialogModule
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
  }

  initializeForm(): void {
    this.formGroup = this.fb.group({
      ddRequired: [this.partner.ddRequired || '', Validators.required],
      ddeacDone: [this.partner.ddeacDone || '', Validators.required],
      eacReference: [this.partner.eacReference || ''],
      levyPotentiallyApplies: [this.partner.levyPotentiallyApplies || '', Validators.required],
      reasonForLevyNotApplying: [this.partner.reasonForLevyNotApplying || ''],
      levyTreatment: [this.partner.levyTreatment || ''],
      pooledFund: [this.partner.pooledFund || '', Validators.required],
      globalKeyAccount: [this.partner.globalKeyAccount || false],
      unSecretariatEntity: [this.partner.unSecretariatEntity || false]
    });
  }

  // Cached data - these match the edit dialog
  allYesNoData = this.cachedDataService.allYesNo;
  allPartnerLevyAppliesData = this.cachedDataService.allPartnerLevyApplies;
  allPartnerReasonForLevyNotData = this.cachedDataService.allPartnerReasonForLevyNot;
  allPartnerLevyTreatmentData = this.cachedDataService.allPartnerLevyTreatment;

  handleApprove(): void {
    if (this.formGroup.invalid) {
      this.showValidationFailedError.set(true);
      return;
    }

    this.isLoading.set(true);
    
    const approvalData = {
      id: this.partner.id,
      ...this.formGroup.value,
      approvalStatus: 'Approved'
    };

    this.partnerService.approvePartner(approvalData).subscribe({
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

  handleCancel(): void {
    this.dialogRef.close();
  }
}
