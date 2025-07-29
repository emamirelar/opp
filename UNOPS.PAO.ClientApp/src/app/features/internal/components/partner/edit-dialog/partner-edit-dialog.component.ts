import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, effect, inject, OnDestroy, Input, OnInit, Output, signal, computed } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';

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
import { MultiSelectModule } from 'primeng/multiselect';
import { AutoFocusModule } from 'primeng/autofocus';
import { DialogModule } from 'primeng/dialog';
import { MessageModule } from 'primeng/message';
import { PartnerService } from '../../../services/partner.service';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { ActivatedRoute, Router } from '@angular/router';
import {MarkdownPipe} from '../../../pipes/markdown.pipe';
import {LinkListComponent} from "../../../../../common/reusables/components/link/list/link-list.component";
import {EntityType} from '../../../../../common/models/link.model';
import { BlockUI } from 'primeng/blockui';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Partner } from '../../../models/partner.model';
import { AiTranscribeComponent } from '../../../../../common/reusables/components/ai-transcribe/ai-transcribe.component';
import { JsonPipe } from '@angular/common';
import { PartnerTreeService } from '../../../services/partner-tree.service';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
  selector: 'app-partner-edit-dialog',
  imports: [
    TranslateModule,
    InputTextModule,
    DropdownModule,
    DatePickerModule,
    ButtonModule,
    TextareaModule,
    PanelModule,
    SelectModule,
    MultiSelectModule,
    AutoFocusModule,
    BlockUI,
    DialogModule,
    MessageModule,
    DividerModule,
    CardModule,
    CheckboxModule,
    ReactiveFormsModule,
    AiTranscribeComponent,
    ProgressSpinnerModule
  ],
  templateUrl: './partner-edit-dialog.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerEditDialogComponent implements OnInit {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  recordPermissions = signal<any>({});

  public formGroup = new FormGroup({
      organizationHierarchyIds: new FormControl<number[]>([], {
        validators: [Validators.required]
      }),
      partnerGroupCode: new FormControl(null, {
        validators: [Validators.required]
      }),
      name: new FormControl('', {
        validators: [Validators.required]
      }),
      status: new FormControl('Active'),
      newEngagement: new FormControl(null, {
        validators: [Validators.required]
      }),
      phone: new FormControl(null),
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
      eacReference: new FormControl(null),
      globalKeyAccount: new FormControl(false),
      unSecretariatEntity: new FormControl(false),
      levyPotentiallyApplies: new FormControl(null, {
        validators: [Validators.required]
      }),
      reasonForLevyNotApplying: new FormControl(null),
      levyTreatment: new FormControl(null),
      address1Street: new FormControl(null),
      address1Street2: new FormControl(null),
      address1City: new FormControl(null),
      address1StateProvince: new FormControl(null),
      address1PostalCode: new FormControl(null),
      address1Country: new FormControl(null),
      discriminator: new FormControl(null),
      id: new FormControl(null),
      createdBy: new FormControl(null),
      createdDate: new FormControl(new Date()),
      lastModifiedBy: new FormControl(null),
      lastModifiedDate: new FormControl(new Date()),
      isDeleted: new FormControl(null),
      deletedBy: new FormControl(null),
      deletedDate: new FormControl(null)
  });

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  partnerService = inject(PartnerService);
  translateService = inject(TranslateService);
  languageService = inject(LanguageService);
  cdr = inject(ChangeDetectorRef);
  private dialogRef = inject(DynamicDialogRef);
  private dialogConfig = inject(DynamicDialogConfig);

  private langChangeSubscription: Subscription = new Subscription();
  @Input() public record: Partner = {};
  @Output() onRecordCreationSuccess = new EventEmitter<any>();

  partnerTreeService = inject(PartnerTreeService);

  showValidationFailedError = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  allPartnerStatusData = this.cachedDataService.allPartnerStatus;
  allPartnerNewEngagementData = this.cachedDataService.allPartnerNewEngagement;
  allYesNoData = this.cachedDataService.allYesNo;
  allPartnerLevyAppliesData = this.cachedDataService.allPartnerLevyApplies;
  allPartnerReasonForLevyNotData = this.cachedDataService.allPartnerReasonForLevyNot;
  allPartnerLevyTreatmentData = this.cachedDataService.allPartnerLevyTreatment;
  allPartnerScopesData = this.cachedDataService.allPartnerScope;
  // Backend already filters for active organization units
  allOrganizationUnitsData = this.cachedDataService.allOrganizationUnits;

  // Signal to track form control changes
  private selectedOrgUnitsSignal = signal<number[]>([]);

  // Custom counter for selected organization units 
  getSelectedActiveOrgUnitsLabel = computed(() => {
    const selectedIds = this.selectedOrgUnitsSignal();
    if (!selectedIds.length) return 'Select organization units';
    
    // The backend already filters for active records, so we just count selected items
    const count = selectedIds.length;
    
    return count === 1 
      ? '1 organization unit selected' 
      : `${count} organization units selected`;
  });
  allPartnerCategoriesData = this.cachedDataService.partnerCategoryGroups;
  allPartnerGroupsForSelect = this.cachedDataService.getPartnerGroupsForSelect;
  recordId: string = '';
  recordData = signal<any>({});
  showCommentDialog = false;
  entityTypePartner = EntityType.Partner;

  @Output() closeModal = new EventEmitter<void>();

  constructor() {
    effect(() => {
      if (this.dialogConfig.data?.requestingSaveSignal?.()) {
        this.handleSave();
      }
    });
  }

  // Helper methods for organization hierarchy FormControl
  setOrganizationHierarchyIds(ids: number[]): void {
    this.formGroup.get('organizationHierarchyIds')?.setValue(ids || []);
  }

  getSelectedOrganizationHierarchyIds(): number[] {
    return this.formGroup.get('organizationHierarchyIds')?.value || [];
  }

  ngOnInit() {
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        this.recordId = paramMap.get("recordId") || '';
        if (this.recordId != '') {
          this.isLoading.set(true);
          this._loadRecordDetails();
        } else {
          // Data is passed directly via dialog config
          this.isLoading.set(true);
          this.record = this.dialogConfig.data?.record;
          this.recordData.set(this.dialogConfig.data.record);
          
          // Preserve the "Active" default if status is null or undefined
          const formData = { ...this.dialogConfig.data.record };
          if (!formData.status) {
            formData.status = 'Active';
          }
          
          // Handle organization unit relationships
          if (formData.organizationUnitRelationships) {
            const orgIds = formData.organizationUnitRelationships.map((rel: any) => rel.organizationHierarchyId);
            this.setOrganizationHierarchyIds(orgIds);
            delete formData.organizationUnitRelationships; // Remove from formData to avoid patch conflict
          }
          
          this.formGroup.patchValue(formData);
          
          // Set loading to false after a short delay to ensure form is properly initialized
          setTimeout(() => {
            this.isLoading.set(false);
          }, 100);
        }
      }
    });
    
    // Track form control changes for organization units counter
    this.formGroup.get('organizationHierarchyIds')?.valueChanges.subscribe(value => {
      this.selectedOrgUnitsSignal.set(value || []);
    });
    
    // Initialize the signal with current form value
    const currentValue = this.formGroup.get('organizationHierarchyIds')?.value || [];
    this.selectedOrgUnitsSignal.set(currentValue);
  }


  handleSave() {
    if (!this.formGroup.invalid) {
      const payload = this._getRequestPayload();

      // Reset requesting save signal immediately
      this.dialogConfig.data.requestingSaveSignal.set(false);

      // Check if this is an import edit
      const isImportEdit = this.dialogConfig.data?.isImportEdit ||
                          this.dialogConfig.data?.record?.isImportEdit ||
                          this.dialogConfig.data?.record?.skipServerSave;

      if (isImportEdit) {
        // This is an import edit, skipping server save
        // Create a copy of the payload with the _updated flag
        const updatedRecord = {
          ...payload,
          _updated: true,
          isImportEdit: true,
          skipServerSave: true
        };

        // For import edits, just return the updated record without saving to server
        this.dialogRef.close(updatedRecord);
        return;
      }

      if (this.recordId) {
        // Update existing partner
        payload['id'] = this.recordId;
        this.partnerService.updatePartnerById(payload).subscribe({
          next: (data: any) => {
            this.feedbackDialogService.showSuccessToast({ detail: 'Record updated successfully!' });
            // Ensure we're not closing the dialog until the operation completes
            setTimeout(() => this.dialogRef.close("saved"));
          },
          error: (error) => {
            this.feedbackDialogService.showErrorToast({ detail: 'Failed to update record' });
          }
        });
      } else {
        // Create new partner
        this.partnerService.createPartner(payload).subscribe({
          next: (data: any) => {
            this.feedbackDialogService.showSuccessToast({ detail: 'Record created successfully!' });
            // Ensure we're not closing the dialog until the operation completes
            setTimeout(() => this.dialogRef.close(data));
          },
          error: (error) => {
            this.feedbackDialogService.showErrorToast({ detail: 'Failed to create record' });
          }
        });
      }
    } else {
      this.dialogConfig.data.requestingSaveSignal.set(false);
      this.showValidationFailedError.set(true);
    }
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
        
        // Preserve the "Active" default if status is null or undefined
        const formData = { ...data };
        if (!formData.status) {
          formData.status = 'Active';
        }
        
        // Handle organization unit relationships
        if (formData.organizationUnitRelationships) {
          const orgIds = formData.organizationUnitRelationships.map((rel: any) => rel.organizationHierarchyId);
          this.setOrganizationHierarchyIds(orgIds);
          delete formData.organizationUnitRelationships; // Remove from formData to avoid patch conflict
        }
        
        this.formGroup.patchValue(formData);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading partner details:', error);
        this.isLoading.set(false);
      }
    });
  }

  handleOnCancelClick(event: MouseEvent) {
    // Check if this is an import edit
    const isImportEdit = this.dialogConfig.data?.isImportEdit ||
                        this.dialogConfig.data?.record?.isImportEdit ||
                        this.dialogConfig.data?.record?.skipServerSave;

    if (isImportEdit) {
      // Just close the dialog for import edits
      this.dialogRef.close();
      return;
    }

    // Standard behavior - navigate to partners page
    this.router.navigate(['partners']);
  }

  _getRequestPayload() {
    let valueObj = this.formGroup.value,
    requestJsonObj: any = {};

    for (let key in valueObj) {
      if (valueObj.hasOwnProperty(key)) {
        let indexValue = (valueObj as any)[key];

        switch (key) {
          case 'organizationHierarchyIds':
            // Convert FormArray values to organizationUnitRelationships
            if (indexValue && Array.isArray(indexValue)) {
              requestJsonObj['organizationUnitRelationships'] = indexValue.map((id: number) => ({
                organizationHierarchyId: id,
                entityType: 'Partner'
              }));
            }
            break;

          default:
            requestJsonObj[key] = indexValue;
            break;
        }
      }
    }

    requestJsonObj['id'] = this.recordId;

    return requestJsonObj;
  }

  // Handler for AI transcription completion
  onTranscriptionCompleted(data: any): void {
    if (data) {
      this.formGroup.patchValue({
        name: data.name || this.formGroup.get('name')?.value,
        shortName: data.shortName || this.formGroup.get('shortName')?.value,
        phone: data.phone || this.formGroup.get('phone')?.value,
        address1Street: data.address1Street || this.formGroup.get('address1Street')?.value,
        address1Street2: data.address1Street2 || this.formGroup.get('address1Street2')?.value,
        address1City: data.address1City || this.formGroup.get('address1City')?.value,
        address1StateProvince: data.address1StateProvince || this.formGroup.get('address1StateProvince')?.value,
        address1PostalCode: data.address1PostalCode || this.formGroup.get('address1PostalCode')?.value,
        address1Country: data.address1Country || this.formGroup.get('address1Country')?.value,
        partnerGroupCode: data.partnerGroupCode || this.formGroup.get('partnerGroupCode')?.value,
      });

      // Handle organization hierarchy IDs from AI transcription
      if (data.organizationHierarchyIds && Array.isArray(data.organizationHierarchyIds)) {
        this.setOrganizationHierarchyIds(data.organizationHierarchyIds);
      }

      this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('message.preFillSuccess') });
    }
  }
}
