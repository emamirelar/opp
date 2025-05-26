import { ChangeDetectionStrategy, ChangeDetectorRef, Component, effect, EventEmitter, inject, Input, OnInit, Output, signal } from '@angular/core';
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
import { CardModule } from 'primeng/card';
import { DialogModule } from 'primeng/dialog';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Router } from '@angular/router';
import { Contact } from '../../../models/contact.model';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { ContactEditDialogFooterComponent } from './footer/contact-edit-dialog-footer.component';
import { AiTranscribeComponent } from '../../../../../common/reusables/components/ai-transcribe/ai-transcribe.component';

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
    AiTranscribeComponent
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
    title: [''],

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
    deletedDate: [null]
  });

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  contactService = inject(ContactService);
  languageService = inject(LanguageService);
  private dialogRef = inject(DynamicDialogRef);
  private dialogConfig = inject(DynamicDialogConfig);

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
  }

  ngOnInit() {
    this.record = this.dialogConfig.data?.record;
    
    // Set initial loading state
    this.isLoading.set(true);
    
    // Check if we have the record data
    if (this.record) {
      this.formGroup.patchValue(this.record);
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
      this.dialogConfig.data.requestingSaveSignal.set(false);

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
          error: (error) => {
            this.feedbackDialogService.showErrorToast({ detail: 'Failed to update record' });
          }
        });
      } else {
        // Create new contact
        this.contactService.createContact(payload).subscribe({
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
}
