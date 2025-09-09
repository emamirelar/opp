import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, effect, inject, OnDestroy, Input, OnInit, Output, signal, computed } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { DialogService } from 'primeng/dynamicdialog';
import { DuplicateConfirmationDialogComponent } from '../../../components/contact/duplicate-confirmation-dialog/duplicate-confirmation-dialog.component';

//Language translation import
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { UserSearchService } from '../../../../../common/services/user-search.service';
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
import { ENTITY_STATUS_OPTIONS } from '../../../models/entity-status.enum';

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
    BlockUI,
    DialogModule,
    MessageModule,
    DividerModule,
    CardModule,
    CheckboxModule,
    ReactiveFormsModule,
    AiTranscribeComponent,
    ProgressSpinnerModule,
    DuplicateConfirmationDialogComponent
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
      partnerGroupCode: new FormControl(null),
      
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
      canCreateNewOpportunities: new FormControl(false),
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
  isLoading = signal<boolean>(false);
  isAdmin = signal<boolean>(false);
  partnerLevyStatusValue = signal<string>('');
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
  allPartnerCategoriesData = this.cachedDataService.getPartnerCategoriesForSelect;
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

  showApprovalFields = computed(() => {
    return this.recordData()?.partnerApprovalStatus === 'Approved';
  });

  approvalFieldsEnabled = computed(() => {
    return this.isAdmin();
  });

  // Check if reason field should be required (when approval fields are visible and enabled)
  reasonFieldRequired = computed(() => {
    return this.showApprovalFields() && this.approvalFieldsEnabled();
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

  /**
   * Check if status field should be visible
   */
  showStatusField = computed(() => {
    return !!(this.recordData().permissions?.canClose || this.recordData().permissions?.canArchive);
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

  @Output() closeModal = new EventEmitter<void>();

  constructor() {
    effect(() => {
      if (this.dialogConfig.data?.requestingSaveSignal?.()) {
        this.handleSave();
      }
    });

    // Effect to handle conditional validation for reason field
    effect(() => {
      const shouldRequireReason = this.showApprovalFields() && this.approvalFieldsEnabled();
      const reasonControl = this.formGroup?.get('reasonForNoNewOpportunity');
      
      if (reasonControl) {
        if (shouldRequireReason) {
          reasonControl.setValidators([Validators.required]);
        } else {
          reasonControl.clearValidators();
        }
        reasonControl.updateValueAndValidity();
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
    
    // Subscribe to partnerLevyStatus changes to update the signal for reactive computed properties
    this.formGroup.get('partnerLevyStatus')?.valueChanges.subscribe(value => {
      this.partnerLevyStatusValue.set(value || '');
      
      // Clear reasonForLevy when it should be hidden
      if (value !== 'DoesNotApply' && value !== 'PotentiallyNotApplied') {
        this.formGroup.get('reasonForLevy')?.setValue(null);
      }
    });
    
    // Initialize the signal with the current form value
    this.partnerLevyStatusValue.set(this.formGroup.get('partnerLevyStatus')?.value || '');

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
        // Mark as updated so the import dialog knows to apply the changes
        updatedRecord._updated = true;
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
        this.createPartnerWithDuplicateDetection(payload);
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
        partnerGroupCode: data.partnerGroupCode || this.formGroup.get('partnerGroupCode')?.value,
        
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
        pooledFund: data.pooledFund ?? this.formGroup.get('pooledFund')?.value,
        canCreateNewOpportunities: data.canCreateNewOpportunities ?? this.formGroup.get('canCreateNewOpportunities')?.value,
        reasonForNoNewOpportunity: data.reasonForNoNewOpportunity || this.formGroup.get('reasonForNoNewOpportunity')?.value
      });

      // Handle organization unit relationships from AI transcription
      if (data.organizationUnitRelationships && Array.isArray(data.organizationUnitRelationships)) {
        this.setOrganizationHierarchyIds(data.organizationUnitRelationships);
      }
      // Fallback for legacy organizationHierarchyIds
      else if (data.organizationHierarchyIds && Array.isArray(data.organizationHierarchyIds)) {
        this.setOrganizationHierarchyIds(data.organizationHierarchyIds);
      }

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
          this.feedbackDialogService.showSuccessToast({ 
            detail: response.message || 'Partner created successfully!' 
          });
          setTimeout(() => this.dialogRef.close(response.data || response));
        } else {
          // Fallback for successful creation (old format)
          this.feedbackDialogService.showSuccessToast({ 
            detail: 'Partner created successfully!' 
          });
          setTimeout(() => this.dialogRef.close(response));
        }
      },
      error: (error: any) => {
        this.feedbackDialogService.showErrorToast({ 
          detail: 'Failed to create partner. Please try again.' 
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
      header: 'Duplicate Partner Detected',
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
              this.feedbackDialogService.showSuccessToast({ 
                detail: 'Partner created successfully (duplicate confirmation acknowledged)!' 
              });
              setTimeout(() => this.dialogRef.close(response.data));
            } else {
              // Fallback for successful creation
              this.feedbackDialogService.showSuccessToast({ 
                detail: 'Partner created successfully!' 
              });
              setTimeout(() => this.dialogRef.close(response));
            }
          },
          error: (error: any) => {
            this.feedbackDialogService.showErrorToast({ 
              detail: 'Failed to create partner. Please try again.' 
            });
            console.error('Confirmed partner creation error:', error);
          }
        });
      } else {
        // User cancelled - do nothing, stay on the form
        this.feedbackDialogService.showInfoToast({ 
          detail: 'Partner creation cancelled.' 
        });
      }
    });
  }
}
