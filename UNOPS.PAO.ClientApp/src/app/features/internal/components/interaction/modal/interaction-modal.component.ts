import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, signal, SimpleChanges, inject, effect, computed } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';
import { Button } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import {Textarea} from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { InteractionType, INTERACTION_TYPE_TRANSLATION_KEYS } from '../../../models/interaction-type.enum';
import { DocumentComponent } from '../../../../../common/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '../../../overrides/reusables/components/document/gdrive/document-gdrive.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ContactService } from '../../../services/contact.service';
import { PartnerService } from '../../../services/partner.service';
import { CommonModule } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
import { MessageService } from 'primeng/api';
import { MessageModule } from 'primeng/message';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { CalendarModule } from 'primeng/calendar';
import { InteractionModalFooterComponent } from './footer/interaction-modal-footer.component';
import { NgIf } from '@angular/common';
import { ChipModule, Chip } from 'primeng/chip';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { AiTranscribeComponent } from '../../../../../common/reusables/components/ai-transcribe/ai-transcribe.component';
import { HttpClientModule } from '@angular/common/http';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { PanelModule } from 'primeng/panel';
import { PermissionUtilityService } from '../../../../../essentials/services/permission-utility.service';
import { FeedbackDialogService } from '../../../../../common/reusables/services/feedback-dialog.service';
import {Divider} from 'primeng/divider';

/**
 * @uiEntity Interaction
 * @route Modal dialog (no direct route)
 * @description Create and edit interaction records including meetings, calls, emails, and other communications with partners and contacts. Supports AI transcription and file attachments.
 * @capabilities create_interaction, edit_interaction, add_participants, upload_documents, ai_transcription, schedule_followup, set_interaction_type
 * @synonyms meeting, communication, event, activity, engagement, touchpoint
 * @mandatoryFields type, date, subject, contactId
 * @help_when_stuck Fill in the interaction type, date, and subject. Add participants using email addresses or selecting contacts. Use the AI transcription feature to quickly populate interaction details from audio or images.
 * @common_tasks
 *   - Recording a meeting: Select 'Meeting' type, add date/time, participants, and notes
 *   - Logging a phone call: Choose 'Phone Call' type, add contact, and conversation summary
 *   - Adding participants: Use email addresses or select from contact list
 *   - Using AI transcription: Click the transcribe button to process audio/image files
 *   - Attaching documents: Use the document section to upload relevant files
 *   - Setting follow-up: Add future interaction reminders or next steps
 */
@Component({
  selector: 'app-interaction-modal',
  templateUrl: './interaction-modal.component.html',
  styleUrl: './interaction-modal.component.scss',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    CalendarModule,
    InputTextModule,
    Textarea,
    SelectModule,
    MultiSelectModule,
    DocumentComponent,
    GDriveDocumentComponent,
    TranslateModule,
    CommonModule,
    MessageModule,
    ConfirmDialog,
    ChipModule,
    AutoCompleteModule,
    HttpClientModule,
    AiTranscribeComponent,
    PanelModule,
    Divider
  ],
  providers: [
    ConfirmationService,
    MessageService
  ],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionModalComponent {
  private dialogRef = inject(DynamicDialogRef);
  private dialogConfig = inject(DynamicDialogConfig);

  onChange: any = () => { };
  onTouched: any = () => { };

  record?: Interaction;
  isSaving = signal(false);

  // Input property for recordId when used in AI layout
  @Input() recordId: string = '';

  formGroup: FormGroup;

  typeOptions = Object.values(InteractionType).map(type => ({
    label: INTERACTION_TYPE_TRANSLATION_KEYS[type],
    value: type,
    translateKey: INTERACTION_TYPE_TRANSLATION_KEYS[type]
  }));

  cachedDataService = inject(CachedDataService);

  //contacts: Contact[] = [];
  //partners: Partner[] = [];
  invalidEmails: string[] = [];
  invalidPhones: string[] = [];
  showValidationFailedError = signal<boolean>(false);

  allContacts = this.cachedDataService.allContacts;
  allPartners = this.cachedDataService.allPartners;
  allUsers = this.cachedDataService.allUsers;
  // Backend already filters for active organization units
  allOrgUnits = this.cachedDataService.allOrganizationUnits;
  currentUser = this.cachedDataService.currentUser;

  // Signal to track form control changes
  private selectedOrgUnitsSignal = signal<number[]>([]);

  // Custom counter for selected organization units
  getSelectedActiveOrgUnitsLabel = computed(() => {
    const selectedIds = this.selectedOrgUnitsSignal();
    if (!selectedIds.length) return this.translateService.instant('label.interaction.selectOrganizationUnits');

    // The backend already filters for active records, so we just count selected items
    const count = selectedIds.length;

    return count === 1
      ? this.translateService.instant('label.interaction.oneOrganizationUnitSelected')
      : this.translateService.instant('label.interaction.organizationUnitsSelected', { count });
  });

  // Check if this is an import edit
  get isImportEdit(): boolean {
    const record = this.dialogConfig.data?.record;
    return record?.isImportEdit || record?.skipServerSave || this.dialogConfig.data?.isImportEdit || false;
  }

  // Permission management using utility service
  private permissionUtils: any;
  recordPermissions: any;

  constructor(
    private fb: FormBuilder,
    private interactionService: InteractionService,
    protected contactService: ContactService,
    protected partnerService: PartnerService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translateService: TranslateService,
    private permissionUtilityService: PermissionUtilityService,
    private feedbackDialogService: FeedbackDialogService
  ) {
    this.formGroup = this.fb.group({
      id: [''],
      type: ['', Validators.required],
      date: [new Date(), Validators.required],
      description: [''],
      contactId: ['', Validators.required],
      contactIds: [[]],
      partnerIds: [[]],
      userIds: [[]],
      emailAddresses: [[]],
      phoneNumbers: [[]],
      location: [''],
      subject: ['', Validators.required],
      createdBy: [null],

      previousContactIds: [[]],
      previousEmails: [[]],
      previousPhones: [[]],
      previousUserIds: [[]],
      organizationHierarchyIds: [[]]
    });

    this.setupContactIdsChangeListener();
    this.setupEmailChangeListener();
    this.setupPhoneNumberChangeListener();
    this.setupUserIdsChangeListener();

    effect(() => {
      const userId = this.currentUser()?.id;
      const currentFormUserId = this.formGroup.get('createdBy')?.value;

      if (userId && !currentFormUserId) { // Only set if not already set
        this.formGroup.patchValue({ createdBy: userId });
      }
    });

    // Set up the footer template
    this.dialogConfig.templates = {
      footer: InteractionModalFooterComponent
    };

    // Initialize permission management
    this.permissionUtils = this.permissionUtilityService.createInstancePermissions('Interaction');
    this.recordPermissions = this.permissionUtils.recordPermissions;
  }

  // Helper methods for organization hierarchy FormControl
  setOrganizationHierarchyIds(ids: number[]): void {
    this.formGroup.get('organizationHierarchyIds')?.setValue(ids || []);
  }

  getSelectedOrganizationHierarchyIds(): number[] {
    return this.formGroup.get('organizationHierarchyIds')?.value || [];
  }

  ngOnInit() {
    // If recordId is provided via Input (AI layout), load data directly
    if (this.recordId && this.recordId !== '') {
      this.loadInteractionById(Number(this.recordId));
      return;
    }

    // Otherwise, use the dialog-based logic (normal modal usage)
    // Get the record ID from dialog data
    const recordId = this.dialogConfig.data?.id;
    const initialData = this.dialogConfig.data?.initialData;
    const recordData = this.dialogConfig.data?.record; // Data for import edits
    // Check if this is an import edit to adjust validation
    const isImportEdit = this.dialogConfig.data?.isImportEdit ||
                        this.dialogConfig.data?.record?.isImportEdit ||
                        this.dialogConfig.data?.record?.skipServerSave;

    if (recordId) {
      // Existing record - fetch full details from API
      this.recordId = recordId.toString();
      this.loadInteractionById(Number(recordId));
    } else if (initialData && initialData.id) {
      // Existing record passed as initial data (fallback)
      this.record = initialData;
      if (this.record) {
        this.recordId = this.record.id + '';
        this.populateForm(this.record);
      }
    } else if (recordData && Object.keys(recordData).length > 0) {
      // Import edit data - use record data directly
      this.record = recordData;
      if (this.record) {
        this.recordId = this.record.id ? this.record.id + '' : '';
        this.populateForm(this.record);
      }
    } else {
      // New interaction - set default permissions that allow creation
      this.recordPermissions.set({
        entity: 'Interaction',
        hasAccess: true,
        permissions: {
          canRead: true,
          canCreate: true, // Allow creation for new records
          canUpdate: true, // Allow editing form fields for new records
          canDelete: false // New records can't be deleted
        }
      });

      // Pre-populate form with initial data for new records (e.g., partnerId)
      if (initialData && Object.keys(initialData).length > 0) {
        this.formGroup.patchValue(initialData);

        // If partnerId is provided, also set it in partnerIds array
        if (initialData.partnerId) {
          this.formGroup.patchValue({
            partnerIds: [parseInt(initialData.partnerId)]
          });
        }
      }
    }

    // For import edits, adjust form validation to be more lenient
    if (isImportEdit && this.record) {
      // Remove contactId required validation for import edits since it might be empty
      this.formGroup.get('contactId')?.clearValidators();
      this.formGroup.get('contactId')?.updateValueAndValidity();
    }

    // Expose the handleSave function to be called from footer
    if (this.dialogConfig.data) {
      this.dialogConfig.data.handleSave = this.onSubmit.bind(this);
      this.dialogConfig.data.handleDelete = this.deleteInteraction.bind(this);
      this.dialogConfig.data.isSaving = this.isSaving;
      this.dialogConfig.data.recordPermissions = this.recordPermissions;
    }

    // Track form control changes for organization units counter
    this.formGroup.get('organizationHierarchyIds')?.valueChanges.subscribe(value => {
      this.selectedOrgUnitsSignal.set(value || []);
    });

    // Initialize the signal with current form value
    const currentValue = this.formGroup.get('organizationHierarchyIds')?.value || [];
    this.selectedOrgUnitsSignal.set(currentValue);
  }

  private loadInteractionById(id: number) {
    this.interactionService.getById(id).subscribe({
      next: (response) => {
        if (response.body) {
          this.record = response.body;
          this.populateForm(this.record);
        }
      },
      error: (error) => {
        console.error('Failed to load interaction:', error);
        this.feedbackDialogService.showErrorToast({
          detail: 'Failed to load interaction details',
          summary: 'Error'
        });
      }
    });
  }

  private populateForm(record: Interaction) {
    // Handle organization unit relationships - convert to organizationHierarchyIds for form
    let organizationHierarchyIds: number[] = [];
    if (record.organizationUnitRelationships) {
      organizationHierarchyIds = record.organizationUnitRelationships.map(rel => rel.organizationHierarchyId);
    }

    this.formGroup.patchValue({
      id: record.id,
      type: record.type,
      date: new Date(record.date),
      description: record.description,
      contactId: record.contactId,
      contactIds: record.contactIds || [],
      partnerIds: record.partnerIds || [],
      userIds: record.userIds || [],
      emailAddresses: record.emailAddresses || [],
      phoneNumbers: record.phoneNumbers || [],
      location: record.location,
      subject: record.subject,
      createdBy: record.createdBy,
      previousContactIds: record.contactIds || [],
      previousEmails: record.emailAddresses || [],
      previousPhones: record.phoneNumbers || [],
      previousUserIds: record.userIds || []
    });

    // Set organization hierarchy IDs using helper method
    this.setOrganizationHierarchyIds(organizationHierarchyIds);

    // Extract permissions from the interaction response if they exist
    if (record.permissions) {
      this.recordPermissions.set({
        entity: 'Interaction',
        hasAccess: true,
        permissions: record.permissions
      });
    }
  }

  private showSuccessMessage(messageKey: string): void {
    this.isSaving.set(false);
    this.messageService.add({
      severity: 'success',
      summary: this.translateService.instant('message.success'),
      detail: this.translateService.instant(messageKey)
    });
  }

  private showErrorMessage(messageKey: string, error?: any): void {
    this.isSaving.set(false);
    this.messageService.add({
      severity: 'error',
      summary: this.translateService.instant('message.error'),
      detail: this.translateService.instant(messageKey)
    });
    if (error) {
      console.error(error);
    }
  }

  /**
   * @uiButton save_interaction,create_interaction
   * @description Saves or creates an interaction record with all form data, including participants, documents, and interaction details
   * @label Save | Create Interaction
   * @icon pi pi-check
   * @when_to_use When all required fields are filled and you want to save the interaction to the system
   * @permissions INTERACTION_CREATE, INTERACTION_UPDATE
   */
  onSubmit(): void {
    const formValue = this.formGroup.value;

    // Set contactId to first contact from contactIds for backward compatibility
    if (formValue.contactIds && formValue.contactIds.length > 0) {
      formValue.contactId = formValue.contactIds[0];
      this.formGroup.patchValue({ contactId: formValue.contactId });
    }

    // Keep organizationHierarchyIds as is - no conversion needed
    // The backend now expects organizationHierarchyIds directly

    if (this.formGroup.valid) {
      // Clear validation error if form is now valid
      this.showValidationFailedError.set(false);

      // Check if this is an import edit (we're only updating local data, not saving to server)
      const isImportEdit = this.dialogConfig.data?.isImportEdit ||
                          this.dialogConfig.data?.record?.isImportEdit ||
                          this.dialogConfig.data?.record?.skipServerSave;

      if (isImportEdit) {
        // This is an import edit, skipping server save
        // Just update the record with the form values and mark it as updated
        if (this.record) {
          Object.assign(this.record, formValue);
          this.record._updated = true;

          // Close the dialog with the updated record
          this.dialogRef.close(this.record);
          return;
        }
      }

      // Only set loading state for actual server saves

      // Check permissions before saving
      if (formValue.id) {
        // For updates, check if user has update permission
        if (!this.permissionUtilityService.canUpdate(this.recordPermissions())) {
          this.feedbackDialogService.showErrorToast({
            detail: 'You do not have permission to update this interaction',
            summary: 'Permission Denied'
          });
          return;
        }
      } else {
        // For creates, check if user has create permission
        if (!this.permissionUtilityService.canCreate(this.recordPermissions())) {
          this.feedbackDialogService.showErrorToast({
            detail: 'You do not have permission to create interactions',
            summary: 'Permission Denied'
          });
          return;
        }
      }

      this.isSaving.set(true);

      if (formValue.id) {
        // Update existing interaction
        this.interactionService.update(formValue).subscribe({
          next: () => {
            this.showSuccessMessage('message.interactionUpdated');
            this.dialogRef.close('saved');
          },
          error: (error) => {
            this.showErrorMessage('message.errorUpdatingInteraction', error);
            this.isSaving.set(false);
          }
        });
      } else {
        // Create new interaction
        this.interactionService.create(formValue).subscribe({
          next: (response) => {
            // Check if response indicates duplicate detection
            if (response.body?.isDuplicate && response.body?.requiresConfirmation) {
              // Show duplicate confirmation dialog
              this.confirmationService.confirm({
                message: response.body.message,
                header: 'Duplicate Interaction Detected',
                acceptLabel: 'Create Anyway',
                rejectLabel: 'Cancel',
                accept: () => {
                  // User confirmed, create with duplicate confirmation
                  const confirmFormValue = { ...formValue, confirmDuplicateCreation: true };
                  this.interactionService.create(confirmFormValue).subscribe({
                    next: (data) => {
                      this.showSuccessMessage('message.interactionCreated');
                      this.dialogRef.close(data.body?.data || data.body || data);
                    },
                    error: (error) => {
                      this.showErrorMessage('message.errorCreatingInteraction', error);
                      this.isSaving.set(false);
                    }
                  });
                }
              });
            } else {
              // Normal creation success
              this.showSuccessMessage('message.interactionCreated');
              this.dialogRef.close(response.body?.data || response.body || response);
            }
          },
          error: (error) => {
            this.showErrorMessage('message.errorCreatingInteraction', error);
            this.isSaving.set(false);
          }
        });
      }
    }
    else {
      this.isSaving.set(false);
      this.showValidationFailedError.set(true);
    }
  }

  /**
   * @uiButton delete_interaction
   * @description Permanently deletes an interaction record after confirmation dialog
   * @label Delete
   * @icon pi pi-trash
   * @when_to_use When an interaction was recorded incorrectly or is no longer relevant (use with caution)
   * @permissions INTERACTION_DELETE
   */
  deleteInteraction(): void {
    // Check if user has delete permission
    if (!this.permissionUtilityService.canDelete(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to delete this interaction',
        summary: 'Permission Denied'
      });
      return;
    }

    this.confirmationService.confirm({
      message: this.translateService.instant('message.deleteInteractionConfirmation'),
      header: this.translateService.instant('title.confirmation'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        const interactionId = this.formGroup.get('id')?.value;
        if (interactionId) {
          this.interactionService.delete(interactionId).subscribe({
            next: () => {
              this.showSuccessMessage('message.interactionDeleted');
              this.dialogRef.close('deleted');
            },
            error: (error) => this.showErrorMessage('message.errorDeletingInteraction', error)
          });
        }
      }
    });
  }

  isValidEmail(email: any): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  validateEmail(email: any) {
    if (!this.isValidEmail(email)) {
      this.invalidEmails = [...this.invalidEmails, email];
      this.messageService.add({
        severity: 'warn',
        summary: 'Invalid Email',
        detail: `"${email}" is not valid`,
        life: 3000
      });
    }
  }

  isValidPhone(phone: string): boolean {
    // Basic international phone validation pattern
    const phonePattern = /^\+?[\d\s\-\(\)]{8,}$/;
    return phonePattern.test(phone);
  }

  validatePhone(phone: string) {
    if (!this.isValidPhone(phone)) {
      this.invalidPhones = [...(this.invalidPhones || []), phone];
      this.messageService.add({
        severity: 'warn',
        summary: 'Invalid Phone Number',
        detail: `"${phone}" is not a valid phone number`,
        life: 3000
      });
    }
  }

  // Helper: Get emails for contact IDs (only valid matches)
  private getEmailsForContactIds(contactIds: number[]): string[] {
    return contactIds
      .map(id => this.allContacts().find(c => c.id === id)?.email)
      .filter((email): email is string => email !== undefined);
  }

  // Helper: Get contact IDs for emails (only valid matches)
  private getContactIdsForEmails(emails: string[]): number[] {
    return emails
      .map(email => this.allContacts().find(c => c.email === email)?.id)
      .filter((id): id is number => id !== undefined);
  }

  // Helper: Get emails for user IDs (only valid matches)
  private getEmailsForUserIds(userIds: number[]): string[] {
    return userIds
      .map(id => this.allUsers().find(c => c.id === id)?.email)
      .filter((email): email is string => email !== undefined);
  }

  // Helper: Get user IDs for emails (only valid matches)
  private getUserIdsForEmails(emails: string[]): number[] {
    return emails
      .map(email => this.allUsers().find(c => c.email === email)?.id)
      .filter((id): id is number => id !== undefined);
  }

  // Sync when contactIds change (add/remove ONLY matched emails)
  private setupContactIdsChangeListener() {
    this.formGroup.get('contactIds')?.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b))
      )
      .subscribe((newContactIds: number[]) => {
        this.updatePartnerIdsBasedOnContacts();
        const currentEmails = this.formGroup.get('emailAddresses')?.value as string[];
        const validEmailsForNewContactIds = this.getEmailsForContactIds(newContactIds);

        // Step 1: Add new emails for newly added contact IDs (if valid)
        const emailsToAdd = validEmailsForNewContactIds.filter(
          email => !currentEmails.includes(email)
        );

        // Step 2: Remove emails for newly removed contact IDs (if valid)
        const previousContactIds = this.formGroup.get('previousContactIds')?.value as number[];
        const removedContactIds = previousContactIds.filter(id => !newContactIds.includes(id));
        const emailsToRemove = this.getEmailsForContactIds(removedContactIds);

        const updatedEmails = [
          ...currentEmails.filter(email => !emailsToRemove.includes(email)),
          ...emailsToAdd
        ];

        this.formGroup.get('previousContactIds')?.setValue(newContactIds);
        if (JSON.stringify(currentEmails) !== JSON.stringify(updatedEmails)) {
          this.formGroup.get('emailAddresses')?.setValue(updatedEmails);
        }
      });
  }

  // Sync when userIds change (add/remove ONLY matched emails)
  private setupUserIdsChangeListener() {
    this.formGroup.get('userIds')?.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b))
      )
      .subscribe((newUserIds: number[]) => {
        const currentEmails = this.formGroup.get('emailAddresses')?.value as string[];
        const validEmailsForNewUserIds = this.getEmailsForUserIds(newUserIds);

        // Step 1: Add new emails for newly added user IDs (if valid)
        const emailsToAdd = validEmailsForNewUserIds.filter(
          email => !currentEmails.includes(email)
        );

        // Step 2: Remove emails for newly removed user IDs (if valid)
        const previousUserIds = this.formGroup.get('previousUserIds')?.value as number[];
        const removedUserIds = previousUserIds.filter(id => !newUserIds.includes(id));
        const emailsToRemove = this.getEmailsForUserIds(removedUserIds);

        const updatedEmails = [
          ...currentEmails.filter(email => !emailsToRemove.includes(email)),
          ...emailsToAdd
        ];

        this.formGroup.get('previousUserIds')?.setValue(newUserIds);
        if (JSON.stringify(currentEmails) !== JSON.stringify(updatedEmails)) {
          this.formGroup.get('emailAddresses')?.setValue(updatedEmails);
        }
      });
  }

  // Sync when emailAddresses change (add/remove ONLY matched contact IDs)
  private setupEmailChangeListener() {
    this.formGroup.get('emailAddresses')?.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b))
      )
      .subscribe((newEmails: string[]) => {

        const previousEmails = this.formGroup.get('previousEmails')?.value as string[] || [];
        const addedEmails = newEmails.filter(email => !previousEmails.includes(email));

        // Validate all added emails
        const invalidAddedEmails = addedEmails.filter(email => !this.isValidEmail(email));

        if (invalidAddedEmails.length > 0) {
          // Handle invalid emails
          invalidAddedEmails.forEach(email => this.validateEmail(email));

          // Revert to previous valid state
          this.formGroup.get('emailAddresses')?.setValue(previousEmails, { emitEvent: false });

          return; // Abort the sync operation
        }

        const removedEmails = previousEmails.filter(email => !newEmails.includes(email));
        this.formGroup.get('previousEmails')?.setValue(newEmails);

        const currentContactIds = this.formGroup.get('contactIds')?.value as number[];
        const validContactIdsForNewEmails = this.getContactIdsForEmails(newEmails);

        // Step 1: Add new contact IDs for newly added emails (if valid)
        const contactIdsToAdd = validContactIdsForNewEmails.filter(
          id => !currentContactIds.includes(id)
        );

        // Step 2: Remove contact IDs for newly removed emails (if valid)
        const contactIdsToRemove = this.getContactIdsForEmails(removedEmails);

        const updatedContactIds = [
          ...currentContactIds.filter(id => !contactIdsToRemove.includes(id)),
          ...contactIdsToAdd
        ];

        if (JSON.stringify(currentContactIds) !== JSON.stringify(updatedContactIds)) {
          this.formGroup.get('previousContactIds')?.setValue(updatedContactIds);
          this.formGroup.get('contactIds')?.setValue(updatedContactIds);
          this.updatePartnerIdsBasedOnContacts();
        }

        const currentUserIds = this.formGroup.get('userIds')?.value as number[];
        const validUserIdsForNewEmails = this.getUserIdsForEmails(newEmails);

        // Step 1: Add new user IDs for newly added emails (if valid)
        const userIdsToAdd = validUserIdsForNewEmails.filter(
          id => !currentUserIds.includes(id)
        );

        // Step 2: Remove user IDs for newly removed emails (if valid)
        const userIdsToRemove = this.getUserIdsForEmails(removedEmails);

        const updatedUserIds = [
          ...currentUserIds.filter(id => !userIdsToRemove.includes(id)),
          ...userIdsToAdd
        ];

        if (JSON.stringify(currentUserIds) !== JSON.stringify(updatedUserIds)) {
          this.formGroup.get('previousUserIds')?.setValue(updatedUserIds);
          this.formGroup.get('userIds')?.setValue(updatedUserIds);
        }
      });
  }

  private setupPhoneNumberChangeListener() {
    this.formGroup.get('phoneNumbers')?.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b))
      )
      .subscribe((newPhones: string[]) => {

        const previousPhones = this.formGroup.get('previousPhones')?.value as string[] || [];
        const addedPhones = newPhones.filter(phone => !previousPhones.includes(phone));

        // Validate all added phones
        const invalidAddedPhones = addedPhones.filter(phone => !this.isValidPhone(phone));

        if (invalidAddedPhones.length > 0) {
          // Handle invalid phones
          this.invalidPhones.forEach(phone => this.validatePhone(phone));

          // Revert to previous valid state
          this.formGroup.get('phoneNumbers')?.setValue(previousPhones, { emitEvent: false });

          return;
        }
        this.formGroup.get('previousPhones')?.setValue(newPhones);
      });
  }

  private updatePartnerIdsBasedOnContacts() {
    const selectedContactIds = this.formGroup.get('contactIds')?.value as number[];

    // Get unique partnerIds from the selected contacts
    const relatedPartnerIds = this.allContacts()
      .filter(contact => selectedContactIds.includes(contact.id))
      .map(contact => contact.partnerId)
      .filter((partnerId, index, self) => self.indexOf(partnerId) === index); // Remove duplicates

    // Update partnerIds without triggering valueChanges
    this.formGroup.get('partnerIds')?.setValue(relatedPartnerIds, { emitEvent: false });
  }

  getSelectedPartners() {
    const selectedIds = this.formGroup.get('partnerIds')?.value || [];
    return this.allPartners().filter(p => selectedIds.includes(p.id));
  }

  get acceptedMiMIETypesForgDrive() {
    return 'application/pdf,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.google-apps.document,application/vnd.google-apps.spreadsheet';
  }

  /**
   * @uiButton process_transcription
   * @description Processes AI transcription results and automatically fills form fields with extracted interaction data
   * @label Process Transcription
   * @icon pi pi-microphone
   * @when_to_use After uploading audio, image, or text to extract interaction details automatically using AI
   * @permissions INTERACTION_UPDATE, AI_SERVICE_ACCESS
   */
  onTranscriptionCompleted(data: any): void {
    if (data) {
      this.formGroup.patchValue({
        type: data.type || this.formGroup.get('type')?.value,
        date: data.date ? new Date(data.date) : this.formGroup.get('date')?.value,
        description: data.description || this.formGroup.get('description')?.value,
        contactId: data.contactId || this.formGroup.get('contactId')?.value
      });

      // Handle organization hierarchy IDs from AI transcription
      if (data.organizationHierarchyIds && Array.isArray(data.organizationHierarchyIds)) {
        this.setOrganizationHierarchyIds(data.organizationHierarchyIds);
      }
    }
  }

  get showPhoneNumbers() {
    return this.formGroup.get('type')?.value != 'Email';
  }
}
