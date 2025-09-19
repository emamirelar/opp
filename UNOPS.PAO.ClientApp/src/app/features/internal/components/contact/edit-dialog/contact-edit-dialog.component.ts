import { ChangeDetectionStrategy, ChangeDetectorRef, Component, effect, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError, map, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../../../common/services/language.service';
import { InputTextModule } from 'primeng/inputtext';
import { FloatLabelModule } from 'primeng/floatlabel';
import { DividerModule } from 'primeng/divider';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { AutoFocusModule } from 'primeng/autofocus';
import { BlockUI } from 'primeng/blockui';
import { MessageModule } from 'primeng/message';
import { ContactService } from '../../../services/contact.service';
import { PartnerService } from '../../../services/partner.service';
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Router } from '@angular/router';
import { Contact } from '../../../models/contact.model';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { ContactEditDialogFooterComponent } from './footer/contact-edit-dialog-footer.component';
import { AiTranscribeComponent } from '../../../../../common/reusables/components/ai-transcribe/ai-transcribe.component';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DialogService } from 'primeng/dynamicdialog';
import { DuplicateConfirmationDialogComponent, DuplicateDetectionResponse } from '../duplicate-confirmation-dialog/duplicate-confirmation-dialog.component';

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
    ProgressSpinnerModule
  ],
  templateUrl: './contact-edit-dialog.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactEditDialogComponent implements OnInit {
  router = inject(Router);
  private fb = inject(FormBuilder);

  showAssistantFields = signal<boolean>(false);

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
  showValidationFailedError = signal<boolean>(false);
  isLoading = signal<boolean>(false);
  maxDate = new Date();

  @Output() closeModal = new EventEmitter<void>();
  display = true;

  requestingSaveSignal = signal<boolean>(false);

  constructor() {
    this.dialogConfig.templates = {
      footer: ContactEditDialogFooterComponent
    };

    // Set up partner ID change listener to update partner name
    this.setupPartnerIdChangeListener();
  }

  ngOnInit() {
    this.record = this.dialogConfig.data?.record;
    
    // Set initial loading state
    this.isLoading.set(true);
    
    // Check if we have the record data
    if (this.record) {
      // Extract partnerId from partner object if it exists
      const formData: any = { ...this.record };
      if (this.record.partner && this.record.partner.id) {
        formData.partnerId = this.record.partner.id;
      }
      
      this.formGroup.patchValue(formData);
      
      // Update partner name if partnerId is set
      if (formData.partnerId) {
        this.updatePartnerName(formData.partnerId);
      }
    }
    
    // Check if any assistant fields have values
    const hasAssistantInfo = this.record?.assistant || 
                           this.record?.assistantPhone || 
                           this.record?.assistantEmail;
    this.showAssistantFields.set(!!hasAssistantInfo);

    // Exposer la fonction handleSave
    this.dialogConfig.data.handleSave = this.handleSave.bind(this);

    // Set loading to false after a short delay to ensure form is properly initialized
    setTimeout(() => {
      this.isLoading.set(false);
    }, 100);
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
        
        // Close the dialog with the updated record
        this.dialogRef.close(this.record);
        return;
      }

      if (this.record && this.record['id']) {
        // Update existing contact
        payload['id'] = this.record['id'];
        this.contactService.updateContactById(payload).subscribe({
          next: (data: any) => {
            this.feedbackDialogService.showSuccessToast({ detail: 'Record updated successfully!' });
            // Ensure we're not closing the dialog until the operation completes
            setTimeout(() => this.dialogRef.close("saved"));
          },
          error: (error: any) => {
            this.feedbackDialogService.showErrorToast({ detail: 'Failed to update record' });
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

    return requestJsonObj;
  }

  toggleAssistantFields() {
    this.showAssistantFields.update(value => !value);
  }

  // Handle AI Transcribe completion
  onTranscriptionCompleted(data: any): void {
    if (data) {
      // Pre-fill the contact form with AI-extracted data
      this.formGroup.patchValue({
        salutation: data.salutation || this.formGroup.get('salutation')?.value,
        firstName: data.firstName || this.formGroup.get('firstName')?.value,
        middleName: data.middleName || this.formGroup.get('middleName')?.value,
        lastName: data.lastName || this.formGroup.get('lastName')?.value,
        suffix: data.suffix || this.formGroup.get('suffix')?.value,
        title: data.title || this.formGroup.get('title')?.value,
        email: data.email || this.formGroup.get('email')?.value,
        phone: data.phone || this.formGroup.get('phone')?.value,
        mobile: data.mobile || this.formGroup.get('mobile')?.value,
        department: data.department || this.formGroup.get('department')?.value,
        mailingStreet: data.mailingStreet || this.formGroup.get('mailingStreet')?.value,
        mailingCity: data.mailingCity || this.formGroup.get('mailingCity')?.value,
        mailingStateProvince: data.mailingStateProvince || this.formGroup.get('mailingStateProvince')?.value,
        mailingPostalCode: data.mailingPostalCode || this.formGroup.get('mailingPostalCode')?.value,
        mailingCountry: data.mailingCountry || this.formGroup.get('mailingCountry')?.value
      });
      
      this.feedbackDialogService.showSuccessToast({ detail: 'Contact data transcribed successfully!' });
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
            detail: response.message || 'Contact created successfully!' 
          });
          setTimeout(() => this.dialogRef.close(response.data || response));
        } else {
          // Fallback for successful creation (old format)
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
      header: 'Duplicate Contact Detected',
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
          detail: 'Contact creation cancelled.' 
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
        console.log('🔧 Partner ID changed to:', newPartnerId);
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

    console.log('🔧 updatePartnerName called with partnerId:', partnerId);
    const allPartners = this.cachedDataService.allPartners();
    console.log('🔧 allPartners cache contains:', allPartners?.length || 0, 'partners');

    // First, try to find partner in the cache
    const partner = allPartners.find((p: any) => p.id === partnerId);
    if (partner) {
      console.log('🔧 Partner found in cache:', partner.name);
      this.formGroup.get('partnerName')?.setValue(partner.name);
      return;
    }

    // If partner is missing from cache, load it from API
    console.log('🔧 Partner not found in cache, loading from API:', partnerId);
    this.partnerService.getPartnerById(partnerId.toString()).pipe(
      map(partner => partner ? partner.name : null),
      catchError(error => {
        console.warn(`🔧 Failed to load partner ${partnerId}:`, error);
        return of(null);
      })
    ).subscribe({
      next: (partnerName) => {
        if (partnerName) {
          console.log('🔧 Partner loaded from API:', partnerName);
          this.formGroup.get('partnerName')?.setValue(partnerName);
        } else {
          console.log('🔧 Partner not found, clearing partner name');
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
}
