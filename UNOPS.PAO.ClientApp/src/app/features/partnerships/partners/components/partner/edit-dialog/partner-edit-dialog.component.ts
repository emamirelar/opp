import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, effect, inject, OnDestroy, Input, OnInit, Output, signal, computed } from '@angular/core';
import { CachedDataService } from '@shared/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';

import { FeedbackDialogService } from '@shared/services/feedback-dialog.service';
import { DialogService } from 'primeng/dynamicdialog';
import { DuplicateConfirmationDialogComponent } from '@partnerships/contacts/components/contact/duplicate-confirmation-dialog/duplicate-confirmation-dialog.component';

//Language translation import
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { UserSearchService } from '@shared/services/user-search.service';
import { LanguageService } from '@shared/services/language.service';
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
import { PartnerService } from '@partnerships/partners/services/partner.service';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { ActivatedRoute, Router } from '@angular/router';
import {MarkdownPipe} from '@features/shared/pipes/markdown.pipe';
import {LinkListComponent} from '@shared/reusables/components/link/list/link-list.component';
import {EntityType} from '@shared/models/link.model';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Partner } from '@partnerships/partners/models/partner.model';
import { AiTranscribeComponent } from '@shared/reusables/components/ai-transcribe/ai-transcribe.component';
import { JsonPipe } from '@angular/common';
import { PartnerTreeService } from '@partnerships/partners/services/partner-tree.service';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SkeletonModule } from 'primeng/skeleton';
import { AuthService } from '@core/services/auth.service';
import { ENTITY_STATUS_OPTIONS } from '@features/shared/models/entity-status.enum';
import { TooltipModule } from 'primeng/tooltip';

// Interface for duplicate detection response
interface DuplicateDetectionResponse {
  success: boolean;
  action: 'duplicateConfirmation' | 'created';
  message: string;
  entityType?: string;
  duplicateInfo?: {
    totalDuplicates: number;
    highConfidence: number;
    mediumConfidence: number;
    lowConfidence: number;
    topDuplicate?: {
      entityId: number;
      score: number;
      matchReason: string;
      matchedData: any;
    };
  };
  confirmationRequired?: boolean;
  originalData?: any;
}

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
    DialogModule,
    MessageModule,
    DividerModule,
    CardModule,
    CheckboxModule,
    ReactiveFormsModule,
    AiTranscribeComponent,
    ProgressSpinnerModule,
    SkeletonModule,
    TooltipModule
  ],
  providers: [DialogService],
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
      partnerGroupId: new FormControl(null),

      // ========== GENERAL FIELDS ==========
      name: new FormControl('', {
        validators: [Validators.required]
      }),
      partnerShortDescription: new FormControl(null),
      partnerLongDescription: new FormControl(null),
      partnerCategoryId: new FormControl(null),
      liaisonOfficeId: new FormControl(null),
      partnerFocalPointUserId: new FormControl(null),
      status: new FormControl('Draft'),

      pooledFund: new FormControl(false),

      // ========== APPROVAL FIELDS ==========
      keyGlobalPartner: new FormControl(false),
      unAndStateEntity: new FormControl(false),
      unSecretariatPartner: new FormControl(false),
      dueDiligenceRequired: new FormControl(null),
      dueDiligenceApproval: new FormControl(null),
      dueDiligenceApprovalDate: new FormControl(null),
      dueDiligenceExpiryDate: new FormControl(null),
      partnerApprovalDate: new FormControl(null),
      partnerApprovalReference: new FormControl(null),
      partnerLevyStatus: new FormControl(null),
      reasonForLevy: new FormControl(null),
      levyTreatment: new FormControl(null),

      // System fields
      discriminator: new FormControl(null),
      id: new FormControl(null),
      createdBy: new FormControl(null),
      createdDate: new FormControl(new Date()),
      lastModifiedBy: new FormControl(null),
      lastModifiedDate: new FormControl(new Date()),
      isDeleted: new FormControl(null),
      deletedBy: new FormControl(null),
      deletedDate: new FormControl(null),
      // Bulk Import display fields
      liaisonOfficeName: new FormControl(null),
      partnerFocalPointUserName: new FormControl(null),
      partnerGroupName: new FormControl(null),
      organizationHierarchyNames: new FormControl(null),
  });

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  dialogService = inject(DialogService);
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
  isAdmin = signal<boolean>(false);
  validationMode = signal<'save' | 'activate'>('save');
  partnerLevyStatusValue = signal<string>('');
  dueDiligenceRequiredValue = signal<string>('');
  dueDiligenceApprovalValue = signal<string>('');
  allPartnerStatusData = this.cachedDataService.allPartnerStatus;
  allPartnerNewEngagementData = this.cachedDataService.allPartnerNewEngagement;
  allDueDiligenceRequiredData = this.cachedDataService.allDueDiligenceRequired;
  allDueDiligenceApprovalData = this.cachedDataService.allDueDiligenceApproval;
  allPartnerLevyAppliesData = this.cachedDataService.allPartnerLevyApplies;
  allPartnerReasonForLevyNotData = this.cachedDataService.allPartnerReasonForLevyNot;
  allPartnerLevyTreatmentData = this.cachedDataService.allPartnerLevyTreatment;
  allPartnerScopesData = this.cachedDataService.allPartnerScope;
  // Backend already filters for active organization units
  allOrganizationUnitsData = this.cachedDataService.allOrganizationUnits;
  allLiaisonOfficesData = this.cachedDataService.allLiaisonOffices;
  allUsersData = this.cachedDataService.allUsers;
  userSearchService = inject(UserSearchService);

  // User management signals for focal point selection
  userSearchResults = signal<any[]>([]);
  isSearchingUsers = this.userSearchService.isSearching;

  // Combined users for dropdown options - backend handles selected user persistence
  availableUsers = computed(() => {
    const searchResults = this.userSearchResults() || [];

    // When search results exist, use them (backend includes selected user automatically)
    if (searchResults.length > 0) {
      return searchResults;
    }

    // Otherwise use cached users for initial display
    return this.allUsersData() || [];
  });

  // Computed properties for approval section
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

  showApprovalFields = computed(() => {
    return this.recordData()?.partnerApprovalStatus === 'Approved';
  });

  approvalFieldsEnabled = computed(() => {
    return this.isAdmin();
  });

  // Check which fields should show asterisks based on validation mode
  requiredFieldsForActivate = computed(() => {
    const mode = this.validationMode();
    return mode === 'activate' ? {
      name: true,
      partnerShortDescription: true,
      partnerGroupId: true,
      liaisonOfficeId: true
    } : {
      name: true,
      partnerShortDescription: false,
      partnerGroupId: false,
      liaisonOfficeId: false
    };
  });

  // Status management constants and computed properties
  private readonly STATUS_OPTIONS = ENTITY_STATUS_OPTIONS;

  /**
   * Get available status options with translated labels (Active, Closed, Archived only)
   */
  statusOptions = computed(() => {
    const allowedStatuses = ['Active', 'Closed', 'Archived'];
    return this.STATUS_OPTIONS
      .filter(option => allowedStatuses.includes(option.value))
      .map(option => ({
        value: option.value,
        label: this.translateService.instant(option.labelKey)
      }));
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

  /**
   * Handles server-side user search triggered by select filter
   */
  onFocalPointUserSearch(event: any): void {
    // Handle both direct string and event object with filter property
    const searchTerm = typeof event === 'string' ? event : event?.filter || '';

    // Get currently selected focal point user ID to ensure it remains visible
    const selectedFocalPointUserId = this.formGroup.get('partnerFocalPointUserId')?.value;
    const selectedUserIds = selectedFocalPointUserId ? [selectedFocalPointUserId] : [];

    // If no search term and no selected user, clear results
    if ((!searchTerm || searchTerm.length < 2) && selectedUserIds.length === 0) {
      this.userSearchResults.set([]);
      return;
    }

    this.userSearchService.searchUsers(searchTerm, 50, selectedUserIds).subscribe({
      next: (users) => {
        this.userSearchResults.set(users);
      },
      error: (error) => {
        console.warn('Focal point user search failed:', error);
        this.userSearchResults.set([]);
      }
    });
  }

  recordId: string = '';
  recordData = signal<any>({});
  showCommentDialog = false;
  entityTypePartner = EntityType.Partner;

  // Get isSaving signal from dialog data
  isSaving = computed(() => {
    return this.dialogConfig.data?.isSaving?.() || false;
  });

  // Get isLoading signal from dialog data
  isLoading = computed(() => {
    return this.dialogConfig.data?.isLoading?.() || false;
  });

  @Output() closeModal = new EventEmitter<void>();

  constructor() {
    effect(() => {
      if (this.dialogConfig.data?.requestingSaveSignal?.()) {
        this.handleSave();
      }
    });

    // Set validation mode based on dialog config
    effect(() => {
      const mode = this.dialogConfig.data?.validationMode || 'save';
      this.validationMode.set(mode);
    });

    // Effect to handle conditional validation for reasonForLevy field
    effect(() => {
      const shouldRequireReasonForLevy = this.shouldShowReasonForLevy();
      const reasonForLevyControl = this.formGroup?.get('reasonForLevy');

      if (reasonForLevyControl) {
        if (shouldRequireReasonForLevy) {
          reasonForLevyControl.setValidators([Validators.required]);
        } else {
          reasonForLevyControl.clearValidators();
        }
        reasonForLevyControl.updateValueAndValidity();
      }
    });


    // Effect to handle disabled state for all approval fields
    effect(() => {
      const shouldEnableFields = this.approvalFieldsEnabled();
      const approvalFieldNames = [
        'dueDiligenceRequired',
        'dueDiligenceApproval',
        'dueDiligenceApprovalDate',
        'dueDiligenceExpiryDate',
        'partnerApprovalDate',
        'partnerApprovalReference',
        'partnerLevyStatus',
        'reasonForLevy',
        'levyTreatment',
        'keyGlobalPartner',
        'unAndStateEntity',
        'unSecretariatPartner',
        'pooledFund'
      ];

      approvalFieldNames.forEach(fieldName => {
        const control = this.formGroup?.get(fieldName);
        if (control) {
          if (shouldEnableFields) {
            control.enable({ emitEvent: false });
          } else {
            control.disable({ emitEvent: false });
          }
        }
      });
    });

    // Effect to handle validation mode changes
    effect(() => {
      const mode = this.validationMode();

      if (mode === 'activate') {
        // Set validators for activate mode
        const shortDescControl = this.formGroup?.get('partnerShortDescription');
        const groupControl = this.formGroup?.get('partnerGroupId');
        const liaisonControl = this.formGroup?.get('liaisonOfficeId');

        if (shortDescControl) {
          shortDescControl.setValidators([Validators.required]);
          shortDescControl.updateValueAndValidity();
        }
        if (groupControl) {
          groupControl.setValidators([Validators.required]);
          groupControl.updateValueAndValidity();
        }
        if (liaisonControl) {
          liaisonControl.setValidators([Validators.required]);
          liaisonControl.updateValueAndValidity();
        }
      } else if (mode === 'save') {
        // Clear validators for save mode (except name which is always required)
        const shortDescControl = this.formGroup?.get('partnerShortDescription');
        const groupControl = this.formGroup?.get('partnerGroupId');
        const liaisonControl = this.formGroup?.get('liaisonOfficeId');

        if (shortDescControl) {
          shortDescControl.clearValidators();
          shortDescControl.updateValueAndValidity();
        }
        if (groupControl) {
          groupControl.clearValidators();
          groupControl.updateValueAndValidity();
        }
        if (liaisonControl) {
          liaisonControl.clearValidators();
          liaisonControl.updateValueAndValidity();
        }
      }
    });
  }

  // Helper methods for organization hierarchy FormControl (single select managing array)
  setOrganizationHierarchyIds(ids: number[]): void {
    // Set the full array from backend
    const idsArray = ids || [];
    this.formGroup.get('organizationHierarchyIds')?.setValue(idsArray);

    // Manually sync the UI control to ensure it updates (for AI transcription)
    const firstElement = idsArray.length > 0 ? idsArray[0] : null;
    this.formGroup.get('selectedOrgUnitId')?.setValue(firstElement);
    this.selectedOrgUnitSignal.set(firstElement);
  }

  /**
   * Initialize display name fields based on currently selected IDs
   */
  private initializeDisplayNames(): void {
    // Initialize liaison office name
    const liaisonOfficeId = this.formGroup.get('liaisonOfficeId')?.value;
    if (liaisonOfficeId) {
      const liaisonOffices = this.allLiaisonOfficesData() as any[];
      const selectedOffice = liaisonOffices.find((office: any) => office.id === liaisonOfficeId);
      if (selectedOffice) {
        this.formGroup.get('liaisonOfficeName')?.setValue(selectedOffice.name);
      }
    }

    // Initialize partner group name
    const partnerGroupId = this.formGroup.get('partnerGroupId')?.value;
    if (partnerGroupId) {
      const partnerGroups = this.allPartnerGroupsForSelect() as any[];
      // Search through categories and their child groups to find the matching partnerGroupId
      let selectedGroup = null;
      for (const category of partnerGroups) {
        if (category.items) {
          selectedGroup = category.items.find((group: any) => group.value === partnerGroupId);
          if (selectedGroup) break;
        }
      }
      if (selectedGroup) {
        this.formGroup.get('partnerGroupName')?.setValue(selectedGroup.name);
      }
    }

    // Initialize focal point user name
    const partnerFocalPointUserId = this.formGroup.get('partnerFocalPointUserId')?.value;
    if (partnerFocalPointUserId) {
      const users = this.availableUsers();
      const selectedUser = users.find((user: any) => user.id === partnerFocalPointUserId);
      if (selectedUser) {
        this.formGroup.get('partnerFocalPointUserName')?.setValue(selectedUser.name);
      }
    }

    // Initialize organization hierarchy name
    const selectedOrgUnitId = this.formGroup.get('selectedOrgUnitId')?.value;
    if (selectedOrgUnitId) {
      const orgUnits = this.allOrganizationUnitsData() as any[];
      const selectedUnit = orgUnits.find((unit: any) => unit.id === selectedOrgUnitId);
      if (selectedUnit) {
        this.formGroup.get('organizationHierarchyNames')?.setValue(selectedUnit.name);
      }
    }
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

        // Check if recordId is available from dialog data (import edit scenario)
        if (!this.recordId && this.dialogConfig.data?.record?.recordId) {
          this.recordId = this.dialogConfig.data.record.recordId;
        }
        // Also check for id field in the record data
        if (!this.recordId && this.dialogConfig.data?.record?.id) {
          this.recordId = String(this.dialogConfig.data.record.id);
        }

        if (this.recordId != '') {
          this.dialogConfig.data?.isLoading?.set(true);
          this._loadRecordDetails();
        } else {
          // Data is passed directly via dialog config
          this.dialogConfig.data?.isLoading?.set(true);
          this.record = this.dialogConfig.data?.record;
          this.recordData.set(this.dialogConfig.data.record);

          // Status is already a string, no conversion needed
          const formData = { ...this.dialogConfig.data.record };

          // Handle organization unit relationships
          if (formData.organizationUnitRelationships) {
            const orgIds = formData.organizationUnitRelationships.map((rel: any) => rel.organizationHierarchyId);
            this.setOrganizationHierarchyIds(orgIds);
            delete formData.organizationUnitRelationships; // Remove from formData to avoid patch conflict
          }

          // Convert ISO date strings to Date objects for DatePicker components (dialog path)
          if (formData.dueDiligenceApprovalDate && typeof formData.dueDiligenceApprovalDate === 'string') {
            formData.dueDiligenceApprovalDate = new Date(formData.dueDiligenceApprovalDate);
          }
          if (formData.dueDiligenceExpiryDate && typeof formData.dueDiligenceExpiryDate === 'string') {
            formData.dueDiligenceExpiryDate = new Date(formData.dueDiligenceExpiryDate);
          }
          if (formData.partnerApprovalDate && typeof formData.partnerApprovalDate === 'string') {
            formData.partnerApprovalDate = new Date(formData.partnerApprovalDate);
          }

          this.formGroup.patchValue(formData);

          // Ensure focal point user is available in dropdown if selected
          const focalPointUserId = this.formGroup.get('partnerFocalPointUserId')?.value;
          if (focalPointUserId) {
            this.userSearchService.searchUsers('', 50, [focalPointUserId]).subscribe({
              next: (users) => {
                this.userSearchResults.set(users);
              },
              error: (error) => {
                console.warn('Failed to load focal point user for editing:', error);
              }
            });
          }

          // Initialize the partnerLevyStatus signal after patching form data
          this.partnerLevyStatusValue.set(this.formGroup.get('partnerLevyStatus')?.value || '');

          // Initialize display name fields
          this.initializeDisplayNames();

          // Set loading to false after a short delay to ensure form is properly initialized
          setTimeout(() => {
            this.dialogConfig.data?.isLoading?.set(false);
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

    // Subscribe to partnerLevyStatus changes to update the signal for reactive computed properties
    this.formGroup.get('partnerLevyStatus')?.valueChanges.subscribe(value => {
      this.partnerLevyStatusValue.set(value || '');

      // Clear reasonForLevy when it should be hidden
      if (value !== 'DoesNotApply' && value !== 'PotentiallyNotApplied') {
        this.formGroup.get('reasonForLevy')?.setValue(null);
      }
    });

    // Subscribe to Due Diligence Required changes
    this.formGroup.get('dueDiligenceRequired')?.valueChanges.subscribe(value => {
      this.dueDiligenceRequiredValue.set(value || '');
      
      // Clear Due Diligence Approval fields when Due Diligence is not Required
      if (value !== 'Required') {
        this.formGroup.get('dueDiligenceApproval')?.setValue(null);
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

    // Subscribe to liaisonOfficeId changes to set the display name
    this.formGroup.get('liaisonOfficeId')?.valueChanges.subscribe(value => {
      if (value) {
        const liaisonOffices = this.allLiaisonOfficesData() as any[];
        const selectedOffice = liaisonOffices.find((office: any) => office.id === value);
        if (selectedOffice) {
          this.formGroup.get('liaisonOfficeName')?.setValue(selectedOffice.name);
        }
      } else {
        this.formGroup.get('liaisonOfficeName')?.setValue(null);
      }
    });

    // Subscribe to partnerGroupId changes to set the display name
    this.formGroup.get('partnerGroupId')?.valueChanges.subscribe(value => {
      if (value) {
        const partnerGroups = this.allPartnerGroupsForSelect() as any[];
        // Search through categories and their child groups to find the matching partnerGroupId
        let selectedGroup = null;
        for (const category of partnerGroups) {
          if (category.items) {
            selectedGroup = category.items.find((group: any) => group.value === value);
            if (selectedGroup) break;
          }
        }
        if (selectedGroup) {
          this.formGroup.get('partnerGroupName')?.setValue(selectedGroup.name);
        }
      } else {
        this.formGroup.get('partnerGroupName')?.setValue(null);
      }
    });

    // Subscribe to partnerFocalPointUserId changes to set the display name
    this.formGroup.get('partnerFocalPointUserId')?.valueChanges.subscribe(value => {
      if (value) {
        const users = this.availableUsers();
        const selectedUser = users.find((user: any) => user.id === value);
        if (selectedUser) {
          this.formGroup.get('partnerFocalPointUserName')?.setValue(selectedUser.name);
        }
      } else {
        this.formGroup.get('partnerFocalPointUserName')?.setValue(null);
      }
    });

    // Subscribe to selectedOrgUnitId changes to set the display names
    this.formGroup.get('selectedOrgUnitId')?.valueChanges.subscribe(value => {
      if (value) {
        const orgUnits = this.allOrganizationUnitsData() as any[];
        const selectedUnit = orgUnits.find((unit: any) => unit.id === value);
        if (selectedUnit) {
          // Set single organization hierarchy name
          this.formGroup.get('organizationHierarchyNames')?.setValue(selectedUnit.name);
        }
      } else {
        this.formGroup.get('organizationHierarchyNames')?.setValue(null);
      }
    });

    // Initialize the signal with the current form value
    this.partnerLevyStatusValue.set(this.formGroup.get('partnerLevyStatus')?.value || '');

  }


  handleSave() {
    // Validate based on current mode
    const isValid = this.isFormValid();

    if (isValid) {
      this.dialogConfig.data?.isSaving?.set(true);
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
        // Mark as updated so the import dialog knows to apply the changes
        updatedRecord._updated = true;

        // Preserve existing duplicate info if available
        if (this.dialogConfig.data.record?.duplicateInfo) {
          updatedRecord.duplicateInfo = this.dialogConfig.data.record.duplicateInfo;
        }

        this.dialogConfig.data?.isSaving?.set(false);
        this.dialogRef.close(updatedRecord);

        // Trigger duplicate detection after closing to update duplicate indicators
        // This will update the record in the import dialog asynchronously
        setTimeout(() => {
          this.triggerDuplicateDetectionAfterSave(payload, updatedRecord);
        }, 100);
        return;
      }

      if (this.recordId) {
        // Update existing partner
        payload['id'] = this.recordId;
        this.partnerService.updatePartnerById(payload).subscribe({
          next: (data: any) => {
            this.dialogConfig.data?.isSaving?.set(false);
            this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('partner.edit.success.updated') });
            // Ensure we're not closing the dialog until the operation completes
            setTimeout(() => this.dialogRef.close("saved"));
          },
          error: (error) => {
            this.dialogConfig.data?.isSaving?.set(false);
            this.feedbackDialogService.showErrorToast({ detail: this.translateService.instant('partner.edit.error.updateFailed') });
          }
        });
      } else {
        // Create new partner
        this.createPartnerWithDuplicateDetection(payload);
      }
    } else {
      this.dialogConfig.data?.isSaving?.set(false);
      this.dialogConfig.data.requestingSaveSignal.set(false);
      this.showValidationFailedError.set(true);

      Object.keys(this.formGroup.controls).forEach(key => {
        const control = this.formGroup.get(key);
        if (control && control.invalid) {
          console.log(`- ${key}:`, control.errors);
        }
      });
    }
  }

  /**
   * Validates form based on current validation mode
   */
  private isFormValid(): boolean {
    const mode = this.validationMode();

    // Always check reasonForLevy if it should be required
    const reasonForLevyControl = this.formGroup.get('reasonForLevy');
    const isReasonForLevyValid = this.shouldShowReasonForLevy()
      ? (reasonForLevyControl && !reasonForLevyControl.invalid)
      : true;

    if (mode === 'save') {
      // For save, check name and reasonForLevy (if applicable)
      const nameControl = this.formGroup.get('name');
      return Boolean(nameControl && !nameControl.invalid && isReasonForLevyValid);
    } else if (mode === 'activate') {
      // For activate, check all required fields including reasonForLevy
      const nameControl = this.formGroup.get('name');
      const shortDescControl = this.formGroup.get('partnerShortDescription');
      const groupControl = this.formGroup.get('partnerGroupId');
      const liaisonControl = this.formGroup.get('liaisonOfficeId');

      return Boolean(nameControl && !nameControl.invalid &&
                    shortDescControl && !shortDescControl.invalid &&
                    groupControl && !groupControl.invalid &&
                    liaisonControl && !liaisonControl.invalid &&
                    isReasonForLevyValid);
    }

    return true;
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

        const formData = { ...data };

        // Status is already a string, no conversion needed

        // Handle organization unit relationships
        if (formData.organizationUnitRelationships) {
          const orgIds = formData.organizationUnitRelationships.map((rel: any) => rel.organizationHierarchyId);
          this.setOrganizationHierarchyIds(orgIds);
          delete formData.organizationUnitRelationships; // Remove from formData to avoid patch conflict
        }

        // Convert ISO date strings to Date objects for DatePicker components
        if (formData.dueDiligenceApprovalDate && typeof formData.dueDiligenceApprovalDate === 'string') {
          formData.dueDiligenceApprovalDate = new Date(formData.dueDiligenceApprovalDate);
        }
        if (formData.dueDiligenceExpiryDate && typeof formData.dueDiligenceExpiryDate === 'string') {
          formData.dueDiligenceExpiryDate = new Date(formData.dueDiligenceExpiryDate);
        }
        if (formData.partnerApprovalDate && typeof formData.partnerApprovalDate === 'string') {
          formData.partnerApprovalDate = new Date(formData.partnerApprovalDate);
        }

        this.formGroup.patchValue(formData);

        // Ensure focal point user is available in dropdown if selected
        const focalPointUserId = this.formGroup.get('partnerFocalPointUserId')?.value;
        if (focalPointUserId) {
          this.userSearchService.searchUsers('', 50, [focalPointUserId]).subscribe({
            next: (users) => {
              this.userSearchResults.set(users);
            },
            error: (error) => {
              console.warn('Failed to load focal point user for editing:', error);
            }
          });
        }

        // Initialize the partnerLevyStatus signal after patching form data
        this.partnerLevyStatusValue.set(this.formGroup.get('partnerLevyStatus')?.value || '');

        // Initialize display name fields
        this.initializeDisplayNames();

        this.dialogConfig.data?.isLoading?.set(false);
      },
      error: (error) => {
        console.error('Error loading partner details:', error);
        this.dialogConfig.data?.isLoading?.set(false);
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
          case 'partnerFocalPointUserId':
            // Ensure ID fields are sent as integers (not strings)
            requestJsonObj[key] = indexValue ? parseInt(indexValue, 10) : null;
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
        // Primary fields
        name: data.name || this.formGroup.get('name')?.value,
        partnerShortDescription: data.partnerShortDescription || this.formGroup.get('partnerShortDescription')?.value,
        partnerLongDescription: data.partnerLongDescription || this.formGroup.get('partnerLongDescription')?.value,
        partnerGroupId: data.partnerGroupId || this.formGroup.get('partnerGroupId')?.value,

        // Category and liaison office
        partnerCategoryId: data.partnerCategoryId || this.formGroup.get('partnerCategoryId')?.value,
        liaisonOfficeId: data.liaisonOfficeId || this.formGroup.get('liaisonOfficeId')?.value,

        // Focal point
        partnerFocalPointUserId: data.partnerFocalPointUserId || this.formGroup.get('partnerFocalPointUserId')?.value,

        // Status fields
        status: data.status || this.formGroup.get('status')?.value,
        partnerApprovalDate: data.partnerApprovalDate || this.formGroup.get('partnerApprovalDate')?.value,

        // Due diligence fields
        dueDiligenceRequired: data.dueDiligenceRequired || this.formGroup.get('dueDiligenceRequired')?.value,
        dueDiligenceApproval: data.dueDiligenceApproval || this.formGroup.get('dueDiligenceApproval')?.value,
        dueDiligenceApprovalDate: data.dueDiligenceApprovalDate || this.formGroup.get('dueDiligenceApprovalDate')?.value,
        dueDiligenceExpiryDate: data.dueDiligenceExpiryDate || this.formGroup.get('dueDiligenceExpiryDate')?.value,

        // Partner types
        keyGlobalPartner: data.keyGlobalPartner ?? this.formGroup.get('keyGlobalPartner')?.value,
        unAndStateEntity: data.unAndStateEntity ?? this.formGroup.get('unAndStateEntity')?.value,
        unSecretariatPartner: data.unSecretariatPartner ?? this.formGroup.get('unSecretariatPartner')?.value,

        // Levy fields
        partnerLevyStatus: data.partnerLevyStatus || this.formGroup.get('partnerLevyStatus')?.value,
        reasonForLevy: data.reasonForLevy || this.formGroup.get('reasonForLevy')?.value,
        levyTreatment: data.levyTreatment || this.formGroup.get('levyTreatment')?.value,

        // Additional fields
        pooledFund: data.pooledFund ?? this.formGroup.get('pooledFund')?.value
      });

      // Handle organization unit relationships from AI transcription
      if (data.organizationUnitRelationships && Array.isArray(data.organizationUnitRelationships)) {
        this.setOrganizationHierarchyIds(data.organizationUnitRelationships);
      }
      // Fallback for legacy organizationHierarchyIds
      else if (data.organizationHierarchyIds && Array.isArray(data.organizationHierarchyIds)) {
        this.setOrganizationHierarchyIds(data.organizationHierarchyIds);
      }

      // Update display names after AI transcription
      setTimeout(() => {
        this.initializeDisplayNames();
      }, 100);

      this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('message.preFillSuccess') });
    }
  }

  /**
   * Creates a partner with duplicate detection workflow
   */
  private createPartnerWithDuplicateDetection(payload: any): void {
    this.partnerService.createPartner(payload).subscribe({
      next: (response: any) => {
        // Check if response indicates duplicate detection
        if (response.confirmationRequired && response.action === "duplicateConfirmation") {
          // Show duplicate confirmation dialog
          this.showDuplicateConfirmationDialog(response, payload);
        } else if (response.action === 'created' || response.success) {
          // Partner created successfully
          this.dialogConfig.data?.isSaving?.set(false);
          this.cachedDataService.refreshPartners();
          this.feedbackDialogService.showSuccessToast({
            detail: response.message || this.translateService.instant('partner.edit.success.created')
          });
          setTimeout(() => this.dialogRef.close(response.data || response));
        } else {
          // Fallback for successful creation (old format)
          this.dialogConfig.data?.isSaving?.set(false);
          this.cachedDataService.refreshPartners();
          this.feedbackDialogService.showSuccessToast({
            detail: this.translateService.instant('partner.edit.success.created')
          });
          setTimeout(() => this.dialogRef.close(response));
        }
      },
      error: (error: any) => {
        this.dialogConfig.data?.isSaving?.set(false);
        this.feedbackDialogService.showErrorToast({
          detail: this.translateService.instant('partner.edit.error.createFailed')
        });
        console.error('Partner creation error:', error);
      }
    });
  }

  /**
   * Shows the duplicate confirmation dialog
   */
  private showDuplicateConfirmationDialog(duplicateResponse: DuplicateDetectionResponse, originalPayload: any): void {
    // Add entityType to the response
    const responseWithEntityType = {
      ...duplicateResponse,
      entityType: 'partner'
    };

    const dialogRef = this.dialogService.open(DuplicateConfirmationDialogComponent, {
      data: responseWithEntityType,
      header: this.translateService.instant('partner.edit.modal.duplicateDetectedHeader'),
      width: '500px',
      modal: true,
      breakpoints: {
        '960px': '450px',
        '640px': '90vw'
      }
    });

    dialogRef.onClose.subscribe((confirmed: boolean) => {
      if (confirmed) {
        // User confirmed - create partner anyway
        const confirmedPayload = {
          ...originalPayload,
          confirmDuplicateCreation: true
        };

        this.partnerService.createPartner(confirmedPayload).subscribe({
          next: (response: any) => {
            if (response.action === 'created') {
              this.dialogConfig.data?.isSaving?.set(false);
              this.cachedDataService.refreshPartners();
              this.feedbackDialogService.showSuccessToast({
                detail: this.translateService.instant('partner.edit.success.createdWithDuplicateConfirmation')
              });
              setTimeout(() => this.dialogRef.close(response.data));
            } else {
              // Fallback for successful creation
              this.dialogConfig.data?.isSaving?.set(false);
              this.cachedDataService.refreshPartners();
              this.feedbackDialogService.showSuccessToast({
                detail: this.translateService.instant('partner.edit.success.created')
              });
              setTimeout(() => this.dialogRef.close(response));
            }
          },
          error: (error: any) => {
            this.dialogConfig.data?.isSaving?.set(false);
            this.feedbackDialogService.showErrorToast({
              detail: this.translateService.instant('partner.edit.error.createFailed')
            });
            console.error('Confirmed partner creation error:', error);
          }
        });
      } else {
        // User cancelled - do nothing, stay on the form
        this.dialogConfig.data?.isSaving?.set(false);
        this.feedbackDialogService.showInfoToast({
          detail: this.translateService.instant('partner.edit.info.creationCancelled')
        });
      }
    });
  }

  /**
   * Triggers duplicate detection for a saved record to update duplicate information
   */
  private triggerDuplicateDetectionAfterSave(payload: any, updatedRecord?: any): void {
    // Skip if no payload
    if (!payload) {
      return;
    }

    // Create a copy of payload for duplicate detection
    const duplicateCheckPayload = { ...payload };

    // If there's an ID (edit scenario), ensure it's properly formatted as a number
    // The backend SQL will use this ID to exclude the record from duplicate detection
    if (payload.id) {
      const numericId = parseInt(payload.id.toString(), 10);
      if (isNaN(numericId)) {
        console.warn('Invalid ID format, proceeding without ID exclusion:', payload.id);
        delete duplicateCheckPayload.id;
      } else {
        duplicateCheckPayload.id = numericId;
      }
    } else {
    }

    // Call the partner service to detect duplicates (uses the updated SQL with ID exclusion)
    this.partnerService.detectDuplicates(duplicateCheckPayload).subscribe({
      next: (response: any) => {

        // If this is an import edit, update the duplicate information
        if (this.dialogConfig.data.isImportEdit) {
          this.updateDuplicateInfoAfterDetection(response, payload, updatedRecord);
        }
      },
      error: (error: any) => {
        // Silent failure - don't interrupt the user's workflow
        console.warn('Post-save duplicate detection failed:', error);
      }
    });
  }

  /**
   * Update the duplicate information in the record for import dialog refresh
   */
  private updateDuplicateInfoAfterDetection(response: any, payload: any, updatedRecord?: any): void {
    if (!response) {
      return;
    }

    // Extract duplicate information from the response
    const duplicateInfo = response.duplicateInfo;

    if (duplicateInfo) {
      // Parse the stringified JSON fields
      let parsedTopDuplicate = null;
      if (duplicateInfo.topDuplicate) {
        parsedTopDuplicate = { ...duplicateInfo.topDuplicate };

        // Parse matchedData if it's a string
        if (typeof duplicateInfo.topDuplicate.matchedData === 'string') {
          try {
            parsedTopDuplicate.matchedData = JSON.parse(duplicateInfo.topDuplicate.matchedData);
          } catch (e) {
            console.warn('Failed to parse matchedData:', e);
            parsedTopDuplicate.matchedData = duplicateInfo.topDuplicate.matchedData;
          }
        }
      }

      // Parse duplicates if it's a string
      let parsedDuplicates = null;
      if (typeof duplicateInfo.duplicates === 'string') {
        try {
          parsedDuplicates = JSON.parse(duplicateInfo.duplicates);
        } catch (e) {
          console.warn('Failed to parse duplicates:', e);
          parsedDuplicates = duplicateInfo.duplicates;
        }
      } else {
        parsedDuplicates = duplicateInfo.duplicates;
      }

      // Update the record with new duplicate information
      const updatedDuplicateInfo = {
        isDuplicate: duplicateInfo.totalDuplicates > 0,
        hasDuplicates: duplicateInfo.totalDuplicates > 0,
        totalDuplicates: duplicateInfo.totalDuplicates || 0,
        highConfidence: duplicateInfo.highConfidence || 0,
        mediumConfidence: duplicateInfo.mediumConfidence || 0,
        lowConfidence: duplicateInfo.lowConfidence || 0,
        topDuplicate: parsedTopDuplicate,
        duplicates: parsedDuplicates,
        tooltip: duplicateInfo.totalDuplicates > 0
          ? this.translateService.instant('partner.edit.duplicate.foundTooltip', { count: duplicateInfo.totalDuplicates })
          : this.translateService.instant('partner.edit.duplicate.uniqueRecord')
      };

      // Update the record's duplicate info
      this.updateRecordInImportDialog(updatedDuplicateInfo, updatedRecord);

    } else {
      // No duplicates found
      const noDuplicateInfo = {
        isDuplicate: false,
        hasDuplicates: false,
        totalDuplicates: 0,
        highConfidence: 0,
        mediumConfidence: 0,
        lowConfidence: 0,
        topDuplicate: null,
        duplicates: null,
        tooltip: this.translateService.instant('partner.edit.duplicate.uniqueRecord')
      };

      this.updateRecordInImportDialog(noDuplicateInfo, updatedRecord);
    }
  }

  /**
   * Update the record in the import dialog with new duplicate information
   */
  private updateRecordInImportDialog(duplicateInfo: any, updatedRecord?: any): void {
    // Try to find the import dialog service in the global scope
    try {
      // Use a custom event to communicate with the import dialog
      const importRowId = updatedRecord?._importRowId || this.dialogConfig.data.record?._importRowId;

      if (importRowId) {
        const updateEvent = new CustomEvent('update-duplicate-info', {
          detail: {
            importRowId: importRowId,
            duplicateInfo: duplicateInfo
          }
        });

        window.dispatchEvent(updateEvent);
      }
    } catch (error) {
      console.error('Error updating duplicate info in import dialog:', error);
    }
  }
}
