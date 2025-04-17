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
    FormsModule
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
    createdBy: [''],
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
    this.formGroup.patchValue(this.record);

    // Exposer la fonction handleSave
    this.dialogConfig.data.handleSave = this.handleSave.bind(this);

    // Check if any assistant fields have values
    const hasAssistantInfo = this.record?.assistant || 
                           this.record?.assistantPhone || 
                           this.record?.assistantEmail;
    this.showAssistantFields.set(!!hasAssistantInfo);
  }

  handleSave() {
    if (!this.formGroup.invalid) {
      const payload = this._getRequestPayload();

      // Reset requesting save signal immediately
      this.requestingSaveSignal.set(false);

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
}
