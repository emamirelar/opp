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
import { AuthService } from '../../../../../essentials/services/auth.service';

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
      // Partner Org Unit - Array for backend compatibility (optional)
      organizationHierarchyIds: new FormControl<number[]>([]),
      // UI FormControl for single select (synced with array)
      selectedOrgUnitId: new FormControl<number | null>(null),
      partnerGroupCode: new FormControl(null),
      
      // ========== GENERAL FIELDS ==========
      partnerDescription: new FormControl('', {
        validators: [Validators.required]
      }),
      partnerShortDescription: new FormControl(null, {
        validators: [Validators.required]
      }),
      partnerLongDescription: new FormControl(null),
      partnerCategoryId: new FormControl(null, {
        validators: [Validators.required]
      }),
      liaisonOfficeId: new FormControl(null, {
        validators: [Validators.required]
      }),
      
      // Backward compatibility (auto-synced with visible fields, no validators needed)
      name: new FormControl(''),
      shortName: new FormControl(null),
      
      pooledFund: new FormControl(false),
      
      // ========== APPROVAL FIELDS ==========
      keyGlobalPartner: new FormControl(false),
      unAndStateEntity: new FormControl(false),
      unSecretariatPartner: new FormControl(false),
      dueDiligenceRequired: new FormControl(null),
      dueDiligenceApproval: new FormControl(null),
      dueDiligenceApprovalDate: new FormControl(null),
      dueDiligenceExpiryDate: new FormControl(null),
      partnerApprovalStatus: new FormControl('NotApproved'),
      partnerApprovalDate: new FormControl(null),
      partnerApprovalReference: new FormControl(null),
      partnerLevyStatus: new FormControl(null),
      reasonForLevy: new FormControl(null),
      levyTreatment: new FormControl(null),
      canCreateNewOpportunities: new FormControl(true),
      reasonForNoNewOpportunity: new FormControl(null),
      
      // System fields
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
  authService = inject(AuthService);

  showValidationFailedError = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  isAdmin = signal<boolean>(false);
  allPartnerStatusData = this.cachedDataService.allPartnerStatus;
  allPartnerNewEngagementData = this.cachedDataService.allPartnerNewEngagement;
  allYesNoData = this.cachedDataService.allYesNo;
  allPartnerLevyAppliesData = this.cachedDataService.allPartnerLevyApplies;
  allPartnerReasonForLevyNotData = this.cachedDataService.allPartnerReasonForLevyNot;
  allPartnerLevyTreatmentData = this.cachedDataService.allPartnerLevyTreatment;
  allPartnerScopesData = this.cachedDataService.allPartnerScope;
  // Backend already filters for active organization units
  allOrganizationUnitsData = this.cachedDataService.allOrganizationUnits;
  allPartnerCategoriesData = this.cachedDataService.getPartnerCategoriesForSelect;
  allLiaisonOfficesData = this.cachedDataService.allLiaisonOffices;

  // Computed properties for approval section
  isPartnerApproved = computed(() => {
    return this.formGroup.get('partnerApprovalStatus')?.value === 'Approved';
  });

  showApprovalFields = computed(() => {
    return this.isPartnerApproved();
  });

  approvalFieldsEnabled = computed(() => {
    return this.isAdmin();
  });

  // Signal to track form control changes (first element of array for single org unit)
  private selectedOrgUnitSignal = signal<number | null>(null);

  // Get selected organization unit name for display
  getSelectedOrgUnitLabel = computed(() => {
    const selectedId = this.selectedOrgUnitSignal();
    if (!selectedId) return this.translateService.instant('label.partner.selectPartnerOrgUnit');
    
    // Find the selected organization unit name
    const orgUnits = this.allOrganizationUnitsData() as any[];
    const selectedUnit = orgUnits.find((unit: any) => unit.id === selectedId);
    
    return selectedUnit ? selectedUnit.name : this.translateService.instant('label.partner.selectPartnerOrgUnit');
  });
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

  // Helper methods for organization hierarchy FormControl (single select managing array)
  setOrganizationHierarchyIds(ids: number[]): void {
    // Set the full array from backend, UI control will sync automatically
    this.formGroup.get('organizationHierarchyIds')?.setValue(ids || []);
  }

  getSelectedOrganizationHierarchyIds(): number[] {
    // Return the full array for backend compatibility
    return this.formGroup.get('organizationHierarchyIds')?.value || [];
  }



  ngOnInit() {
    // Check admin role
    this.authService.isAdmin().subscribe({
      next: (isAdmin) => {
        this.isAdmin.set(isAdmin);
      },
      error: (error) => {
        console.error('Error checking admin role:', error);
        this.isAdmin.set(false);
      }
    });

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
    
    // Sync between selectedOrgUnitId (UI) and organizationHierarchyIds (backend array)
    
    // When UI FormControl changes, update the array FormControl
    this.formGroup.get('selectedOrgUnitId')?.valueChanges.subscribe(value => {
      const newArray = value ? [value] : [];
      this.formGroup.get('organizationHierarchyIds')?.setValue(newArray, { emitEvent: false });
      this.selectedOrgUnitSignal.set(value);
    });
    
    // When array FormControl changes (from backend data), update UI FormControl
    this.formGroup.get('organizationHierarchyIds')?.valueChanges.subscribe(value => {
      const array = value || [];
      const firstElement = array.length > 0 ? array[0] : null;
      this.formGroup.get('selectedOrgUnitId')?.setValue(firstElement, { emitEvent: false });
      this.selectedOrgUnitSignal.set(firstElement);
    });
    
    // Initialize both controls
    const currentArray = this.formGroup.get('organizationHierarchyIds')?.value || [];
    const firstElement = currentArray.length > 0 ? currentArray[0] : null;
    this.formGroup.get('selectedOrgUnitId')?.setValue(firstElement, { emitEvent: false });
    this.selectedOrgUnitSignal.set(firstElement);
    
    // Sync backward compatibility fields with visible fields
    this.formGroup.get('partnerDescription')?.valueChanges.subscribe(value => {
      this.formGroup.get('name')?.setValue(value, { emitEvent: false });
    });
    
    this.formGroup.get('partnerShortDescription')?.valueChanges.subscribe(value => {
      this.formGroup.get('shortName')?.setValue(value, { emitEvent: false });
    });
    
    // Initialize backward compatibility fields with current values
    const currentDescription = this.formGroup.get('partnerDescription')?.value || '';
    const currentShortDescription = this.formGroup.get('partnerShortDescription')?.value || null;
    this.formGroup.get('name')?.setValue(currentDescription, { emitEvent: false });
    this.formGroup.get('shortName')?.setValue(currentShortDescription, { emitEvent: false });
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
      
      // Debug: Log which fields are invalid
      console.log('Form validation failed. Invalid fields:');
      Object.keys(this.formGroup.controls).forEach(key => {
        const control = this.formGroup.get(key);
        if (control && control.invalid) {
          console.log(`- ${key}:`, control.errors);
        }
      });
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
            // Already an array, pass directly to backend
            requestJsonObj['organizationHierarchyIds'] = indexValue || [];
            break;
          
          case 'partnerCategoryId':
          case 'liaisonOfficeId':
            // Ensure ID fields are sent as integers (not strings)
            requestJsonObj[key] = indexValue ? parseInt(indexValue, 10) : null;
            break;
          
          case 'dueDiligenceApproval':
          case 'dueDiligenceRequired':
          case 'partnerLevyStatus':
            // Populate empty string for these fields if value is empty
            requestJsonObj[key] = indexValue || '';
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
        // New field names
        partnerDescription: data.partnerDescription || data.name || this.formGroup.get('partnerDescription')?.value,
        partnerShortDescription: data.partnerShortDescription || data.shortName || this.formGroup.get('partnerShortDescription')?.value,
        partnerLongDescription: data.partnerLongDescription || this.formGroup.get('partnerLongDescription')?.value,
        
        // Backward compatibility
        name: data.partnerDescription || data.name || this.formGroup.get('name')?.value,
        shortName: data.partnerShortDescription || data.shortName || this.formGroup.get('shortName')?.value,
        
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
