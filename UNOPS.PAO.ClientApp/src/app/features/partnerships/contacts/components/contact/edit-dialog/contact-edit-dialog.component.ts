import { ChangeDetectionStrategy, ChangeDetectorRef, Component, effect, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError, map, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CachedDataService } from '@shared/services/utils';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FeedbackDialogService } from '@shared/services/ui';
import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '@shared/services/utils';
import { InputTextModule } from 'primeng/inputtext';
import { FloatLabelModule } from 'primeng/floatlabel';
import { DividerModule } from 'primeng/divider';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { AutoFocusModule } from 'primeng/autofocus';
import { BlockUI } from 'primeng/blockui';
import { MessageModule } from 'primeng/message';
import { ContactService } from '@partnerships/contacts/services/contact.service';
import { PartnerService } from '@partnerships/partners/services/partner.service';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Router } from '@angular/router';
import { Contact } from '@partnerships/contacts/models/contact.model';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { ContactEditDialogFooterComponent } from './footer/contact-edit-dialog-footer.component';
import { AiTranscribeComponent } from '@features/ai/components/ai-transcribe/ai-transcribe.component';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DialogService } from 'primeng/dynamicdialog';
import { DuplicateConfirmationDialogComponent, DuplicateDetectionResponse } from '../duplicate-confirmation-dialog/duplicate-confirmation-dialog.component';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-contact-edit-dialog',
  imports: [
    TranslateModule,
    InputTextModule,
    FloatLabelModule,
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
    ReactiveFormsModule,
    DialogModule,
    CheckboxModule,
    FormsModule,
    AiTranscribeComponent,
    ProgressSpinnerModule,
    TooltipModule
  ],
  templateUrl: './contact-edit-dialog.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactEditDialogComponent implements OnInit {
  router = inject(Router);
  private fb = inject(FormBuilder);

  showAssistantFields = signal<boolean>(false);
  private partnerContextApplied = false;

  public formGroup: FormGroup = this.fb.group({
    // Basic contact information
    salutation: [''],
    firstName: [''],
    middleName: [''],
    lastName: ['', [Validators.required]],
    suffix: [''],
    title: ['', [Validators.required]],

    // Contact details
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    mobile: [''],

    // Professional information
    partnerId: ['', [Validators.required]],
    department: [''],
    description: [''],
    contactNumber: [''],

    // Organization Unit fields - Array for backend compatibility
    organizationHierarchyIds: [[]],
    // UI FormControl for single select (synced with array)
    selectedOrgUnitId: [null],
    organizationHierarchyNames: [''],

    // Assistant information
    assistant: [''],
    assistantPhone: [''],
    assistantEmail: [''],

    // Mailing address
    mailingStreet: [''],
    mailingStreet2: [''],
    mailingCity: [''],
    mailingStateProvince: [''],
    mailingPostalCode: [''],
    mailingCountry: [''],

    // System fields
    discriminator: [''],
    createdBy: [null],
    createdDate: [new Date()],
    lastModifiedBy: [''],
    lastModifiedDate: [new Date()],
    isDeleted: [false],
    deletedBy: [''],
    deletedDate: [null],
    partnerName: [''],
    
    // Duplicate detection field
    confirmDuplicateCreation: [false]
  });

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  contactService = inject(ContactService);
  partnerService = inject(PartnerService);
  languageService = inject(LanguageService);
  translateService = inject(TranslateService);
  private dialogRef = inject(DynamicDialogRef);
  private dialogConfig = inject(DynamicDialogConfig);
  private dialogService = inject(DialogService);
  private cdr = inject(ChangeDetectorRef);

  @Input() public record: Contact = {};
  @Output() onRecordCreationSuccess = new EventEmitter<any>();

  allSalutationsData = this.cachedDataService.allSalutations;
  allStatusData = this.cachedDataService.allStatus;
  allPronounsData = this.cachedDataService.allPronouns;
  allPartners = this.cachedDataService.allPartners;
  allOrganizationUnitsData = this.cachedDataService.allOrganizationUnits;
  showValidationFailedError = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  maxDate = new Date();

  // Signal for tracking selected organization unit
  private selectedOrgUnitSignal = signal<number | null>(null);

  @Output() closeModal = new EventEmitter<void>();
  display = true;

  requestingSaveSignal = signal<boolean>(false);

  constructor() {
    this.dialogConfig.templates = {
      footer: ContactEditDialogFooterComponent
    };

    // Set up partner ID change listener to update partner name
    this.setupPartnerIdChangeListener();

    // Set up conditional validation for assistant email
    effect(() => {
      const assistantEmailControl = this.formGroup.get('assistantEmail');
      if (this.showAssistantFields()) {
        assistantEmailControl?.setValidators([Validators.required, Validators.email]);
      } else {
        assistantEmailControl?.clearValidators();
      }
      assistantEmailControl?.updateValueAndValidity();
    });
  }

  ngOnInit() {
    // Always refresh partners cache to ensure dropdowns have latest data
    // This is especially important after creating new contacts/partners
    this.cachedDataService.refreshPartners();

    this.record = this.dialogConfig.data?.record;
    const partnerContext = this.dialogConfig.data?.partnerContext;
    
    // Set initial loading state
    this.isLoading.set(true);
    
    // Check if we have the record data
    if (this.record) {
      // Extract partnerId from partner object if it exists
      const formData: any = { ...this.record };
      if (this.record.partner && this.record.partner.id) {
        formData.partnerId = this.record.partner.id;
      }

      // Handle organization unit relationships
      if (formData.organizationUnitRelationships) {
        const orgIds = formData.organizationUnitRelationships.map((rel: any) => rel.organizationHierarchyId);
        this.setOrganizationHierarchyIds(orgIds);
        delete formData.organizationUnitRelationships; // Remove from formData to avoid patch conflict
      }
      
      this.formGroup.patchValue(formData);
      
      // Update partner name if partnerId is set
      if (formData.partnerId) {
        this.updatePartnerName(formData.partnerId);
      }
    }
    
    // Handle partner context (when opened from partner page)
    if (partnerContext?.partnerId && partnerContext?.lockPartner) {
      // Convert partner ID to number if it's a string
      const partnerIdNum = typeof partnerContext.partnerId === 'string' 
        ? parseInt(partnerContext.partnerId) 
        : partnerContext.partnerId;
      
      // Apply partner context immediately and also after data loads
      this.applyPartnerContext(partnerIdNum);
      
      // Also try after a delay to ensure data is loaded
      setTimeout(() => {
        if (!this.partnerContextApplied) {
          this.applyPartnerContext(partnerIdNum);
        }
      }, 500);
    }
    
    // Check if any assistant fields have values
    const hasAssistantInfo = this.record?.assistant || 
                           this.record?.assistantPhone || 
                           this.record?.assistantEmail;
    this.showAssistantFields.set(!!hasAssistantInfo);

    // Exposer la fonction handleSave
    this.dialogConfig.data.handleSave = this.handleSave.bind(this);

    // Set up Organization Unit form control synchronization  
    this.setupOrganizationUnitSync();

    // Set loading to false after a short delay to ensure form is properly initialized
    setTimeout(() => {
      this.isLoading.set(false);
    }, 100);
  }

  // Check if partner field is locked due to partner context
  isPartnerLocked(): boolean {
    const partnerContext = this.dialogConfig.data?.partnerContext;
    return partnerContext?.lockPartner === true;
  }

  // Apply partner context with proper timing
  private applyPartnerContext(partnerIdNum: number): void {
    // Set the partner ID and disable the field
    this.formGroup.patchValue({ partnerId: partnerIdNum });
    this.formGroup.get('partnerId')?.disable();
    
    // Update partner name
    this.updatePartnerName(partnerIdNum);
    
    // Mark as applied
    this.partnerContextApplied = true;
    
    // Trigger change detection
    this.cdr.detectChanges();
  }

  handleSave() {
    if (!this.formGroup.invalid) {
      const payload = this._getRequestPayload();

      // Reset requesting save signal immediately
      this.requestingSaveSignal.set(false);

      // Check if this is an import edit (we're only updating local data, not saving to server)
      if (this.record && this.record.isImportEdit) {
        // Just update the record with the form values and mark it as updated
        Object.assign(this.record, payload);
        this.record._updated = true;
        
        // Preserve existing duplicate info if available
        if (this.dialogConfig.data.record?.duplicateInfo) {
          (this.record as any).duplicateInfo = this.dialogConfig.data.record.duplicateInfo;
        }
        
        // Trigger duplicate detection after closing to update duplicate indicators
        // This will update the record in the import dialog asynchronously
        setTimeout(() => {
          this.triggerDuplicateDetectionAfterSave(payload, this.record);
        }, 100);
        
        // Close the dialog with the updated record
        this.dialogRef.close(this.record);
        return;
      }

      if (this.record && this.record['id']) {
        // Update existing contact
        payload['id'] = this.record['id'];
        this.contactService.updateContactById(payload).subscribe({
          next: (data: any) => {
            this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('message.recordUpdatedSuccessfully') });
            
            // Trigger duplicate detection for the updated record
            this.triggerDuplicateDetectionAfterSave(payload);
            
            // Ensure we're not closing the dialog until the operation completes
            setTimeout(() => this.dialogRef.close("saved"));
          },
          error: (error: any) => {
            this.feedbackDialogService.showErrorToast({ detail: this.translateService.instant('message.failedToUpdateRecord') });
          }
        });
      } else {
        // Create new contact with duplicate detection
        this.createContactWithDuplicateDetection(payload);
      }
    } else {
      this.requestingSaveSignal.set(false);
      this.showValidationFailedError.set(true);
    }
  }

  _getRequestPayload() {
    const formValue = this.formGroup.value;
    const requestJsonObj: Record<string, any> = { ...formValue };

    // Include disabled fields (like locked partner field)
    const rawFormValue = this.formGroup.getRawValue();
    if (this.isPartnerLocked() && rawFormValue.partnerId) {
      requestJsonObj['partnerId'] = rawFormValue.partnerId;
    }

    // Clear assistant fields if the section is not shown
    if (!this.showAssistantFields()) {
      requestJsonObj['assistant'] = null;
      requestJsonObj['assistantPhone'] = null;
      requestJsonObj['assistantEmail'] = null;
    }

    // Handle partner specially
    if (formValue['partner'] && typeof formValue['partner'] === 'object' && 'id' in formValue['partner']) {
      requestJsonObj['partnerId'] = formValue['partner']['id'];
      delete requestJsonObj['partner'];
    }

    // Handle Organization Unit relationships
    requestJsonObj['organizationHierarchyIds'] = formValue['organizationHierarchyIds'] || [];

    return requestJsonObj;
  }

  toggleAssistantFields() {
    this.showAssistantFields.update(value => !value);
  }

  // Handle AI Transcribe completion
  onTranscriptionCompleted(data: any): void {
    if (data) {
      // Handle both flat structure (legacy) and nested structure (new format)
      let contactData = data;
      
      // Check if data has the new nested structure with data array
      if (data.data && Array.isArray(data.data) && data.data.length > 0) {
        // Use the first item from the data array
        contactData = data.data[0];
        
        // Show success message from the response
        if (data.Message) {
          this.feedbackDialogService.showSuccessToast({ detail: data.Message });
        }
      }
      
      // Pre-fill the contact form with AI-extracted data
      this.formGroup.patchValue({
        salutation: contactData.salutation || this.formGroup.get('salutation')?.value,
        firstName: contactData.firstName || this.formGroup.get('firstName')?.value,
        middleName: contactData.middleName || this.formGroup.get('middleName')?.value,
        lastName: contactData.lastName || this.formGroup.get('lastName')?.value,
        suffix: contactData.suffix || this.formGroup.get('suffix')?.value,
        title: contactData.title || this.formGroup.get('title')?.value,
        email: contactData.email || this.formGroup.get('email')?.value,
        phone: contactData.phone || this.formGroup.get('phone')?.value,
        mobile: contactData.mobile || this.formGroup.get('mobile')?.value,
        department: contactData.department || this.formGroup.get('department')?.value,
        mailingStreet: contactData.mailingStreet || this.formGroup.get('mailingStreet')?.value,
        mailingCity: contactData.mailingCity || this.formGroup.get('mailingCity')?.value,
        mailingStateProvince: contactData.mailingStateProvince || this.formGroup.get('mailingStateProvince')?.value,
        mailingPostalCode: contactData.mailingPostalCode || this.formGroup.get('mailingPostalCode')?.value,
        mailingCountry: contactData.mailingCountry || this.formGroup.get('mailingCountry')?.value
      });
      
      // Handle partner selection if partnerId was processed correctly
      if (contactData.partnerId && typeof contactData.partnerId === 'number') {
        // partnerId is now a proper ID, set it in the form
        this.formGroup.patchValue({
          partnerId: contactData.partnerId
        });
      }

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
      
      // Show success message for legacy format
      if (!data.data) {
        this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('message.contactDataTranscribedSuccessfully') });
      }
    }
  }

  /**
   * Creates a contact with duplicate detection workflow
   */
  private createContactWithDuplicateDetection(payload: any): void {
    this.contactService.createContact(payload).subscribe({
      next: (response: any) => {
        // Check if response indicates duplicate detection
        if (response.confirmationRequired && response.action === "duplicateConfirmation") {
          // Show duplicate confirmation dialog
          this.showDuplicateConfirmationDialog(response, payload);
        } else if (response.action === 'created' || response.success) {
          // Contact created successfully
          this.feedbackDialogService.showSuccessToast({ 
            detail: response.message || this.translateService.instant('message.contactCreatedSuccessfully') 
          });
          setTimeout(() => this.dialogRef.close(response.data || response));
        } else {
          // Fallback for successful creation (old format)
          this.feedbackDialogService.showSuccessToast({ 
            detail: this.translateService.instant('message.contactCreatedSuccessfully') 
          });
          setTimeout(() => this.dialogRef.close(response));
        }
      },
      error: (error: any) => {
        this.feedbackDialogService.showErrorToast({ 
          detail: this.translateService.instant('message.failedToCreateContact') 
        });
        console.error('Contact creation error:', error);
      }
    });
  }

  /**
   * Shows the duplicate confirmation dialog
   */
  private showDuplicateConfirmationDialog(duplicateResponse: DuplicateDetectionResponse, originalPayload: any): void {
    const dialogRef = this.dialogService.open(DuplicateConfirmationDialogComponent, {
      data: duplicateResponse,
      header: this.translateService.instant('title.duplicateContactDetected'),
      width: '500px',
      modal: true,
      breakpoints: {
        '960px': '450px',
        '640px': '90vw'
      }
    });

    dialogRef.onClose.subscribe((confirmed: boolean) => {
      if (confirmed) {
        // User confirmed - create contact anyway
        const confirmedPayload = {
          ...originalPayload,
          confirmDuplicateCreation: true
        };
        
        this.contactService.createContact(confirmedPayload).subscribe({
          next: (response: any) => {
            if (response.action === 'created') {
              this.feedbackDialogService.showSuccessToast({ 
                detail: 'Contact created successfully (duplicate confirmation acknowledged)!' 
              });
              setTimeout(() => this.dialogRef.close(response.data));
            } else {
              // Fallback for successful creation
              this.feedbackDialogService.showSuccessToast({ 
                detail: 'Contact created successfully!' 
              });
              setTimeout(() => this.dialogRef.close(response));
            }
          },
          error: (error: any) => {
            this.feedbackDialogService.showErrorToast({ 
              detail: 'Failed to create contact. Please try again.' 
            });
            console.error('Confirmed contact creation error:', error);
          }
        });
      } else {
        // User cancelled - do nothing, stay on the form
        this.feedbackDialogService.showInfoToast({ 
          detail: this.translateService.instant('message.contactCreationCancelled') 
        });
      }
    });
  }

  /**
   * Sets up the partner ID change listener to automatically update partner name
   */
  private setupPartnerIdChangeListener() {
    this.formGroup.get('partnerId')?.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe((newPartnerId: number) => {
        this.updatePartnerName(newPartnerId);
      });
  }

  /**
   * Updates the partner name based on partner ID
   */
  private updatePartnerName(partnerId: number) {
    if (!partnerId) {
      this.formGroup.get('partnerName')?.setValue('');
      return;
    }

    const allPartners = this.cachedDataService.allPartners();

    // First, try to find partner in the cache
    const partner = allPartners.find((p: any) => p.id === partnerId);
    if (partner) {
      this.formGroup.get('partnerName')?.setValue(partner.name);
      return;
    }

    // If partner is missing from cache, load it from API
    this.partnerService.getPartnerById(partnerId.toString()).pipe(
      map(partner => partner ? partner.name : null),
      catchError(error => {
        console.warn(`Failed to load partner ${partnerId}:`, error);
        return of(null);
      })
    ).subscribe({
      next: (partnerName) => {
        if (partnerName) {
          this.formGroup.get('partnerName')?.setValue(partnerName);
        } else {
          this.formGroup.get('partnerName')?.setValue('');
        }
        
        // Trigger change detection
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.warn('🔧 Error loading partner name:', error);
        this.formGroup.get('partnerName')?.setValue('');
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
    }
    
    // Call the contact service to detect duplicates (uses the updated SQL with ID exclusion)
    this.contactService.detectDuplicates(duplicateCheckPayload).subscribe({
      next: (response: any) => {
        const recordType = payload.id ? `existing Contact ID ${payload.id}` : 'new Contact';
        
        // If this is an import edit, update the duplicate information
        if (this.dialogConfig.data.isImportEdit) {
          this.updateDuplicateInfoAfterDetection(response, payload, updatedRecord);
        }
      },
      error: (error: any) => {
        // Silent failure - don't interrupt the user's workflow
        const recordType = payload.id ? `Contact ID ${payload.id}` : 'new Contact';
        console.warn('Post-save duplicate detection failed for', recordType, ':', error);
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
          ? this.translateService.instant('message.duplicatesFound', { count: duplicateInfo.totalDuplicates })
          : this.translateService.instant('message.uniqueRecord')
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
        tooltip: this.translateService.instant('message.uniqueRecord')
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
      } else {
        console.warn('No importRowId found to update duplicate info');
      }
    } catch (error) {
      console.error('Error updating duplicate info in import dialog:', error);
    }
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

  /**
   * Set up organization unit form control synchronization
   */
  private setupOrganizationUnitSync(): void {
    // When UI FormControl changes, update the array FormControl
    this.formGroup.get('selectedOrgUnitId')?.valueChanges.subscribe(value => {
      const newArray = value ? [value] : [];
      this.formGroup.get('organizationHierarchyIds')?.setValue(newArray, { emitEvent: false });
      this.selectedOrgUnitSignal.set(value);
      this.updateOrgUnitDisplayName(value);
    });

    // When array FormControl changes (from backend data), update UI FormControl
    this.formGroup.get('organizationHierarchyIds')?.valueChanges.subscribe(value => {
      const array = value || [];
      const firstElement = array.length > 0 ? array[0] : null;
      this.formGroup.get('selectedOrgUnitId')?.setValue(firstElement, { emitEvent: false });
      this.selectedOrgUnitSignal.set(firstElement);
      this.updateOrgUnitDisplayName(firstElement);
    });

    // Initialize both controls
    const currentArray = this.formGroup.get('organizationHierarchyIds')?.value || [];
    const firstElement = currentArray.length > 0 ? currentArray[0] : null;
    this.formGroup.get('selectedOrgUnitId')?.setValue(firstElement, { emitEvent: false });
    this.selectedOrgUnitSignal.set(firstElement);
  }

  /**
   * Update organization unit display name
   */
  private updateOrgUnitDisplayName(selectedOrgUnitId: number | null): void {
    if (selectedOrgUnitId) {
      const orgUnits = this.allOrganizationUnitsData() as any[];
      const selectedUnit = orgUnits.find((unit: any) => unit.id === selectedOrgUnitId);
      if (selectedUnit) {
        this.formGroup.get('organizationHierarchyNames')?.setValue(selectedUnit.name);
      }
    } else {
      this.formGroup.get('organizationHierarchyNames')?.setValue(null);
    }
  }

  /**
   * Initialize display name fields based on currently selected IDs
   */
  private initializeDisplayNames(): void {
    // Initialize organization hierarchy name
    const selectedOrgUnitId = this.formGroup.get('selectedOrgUnitId')?.value;
    if (selectedOrgUnitId) {
      this.updateOrgUnitDisplayName(selectedOrgUnitId);
    }
  }
}
