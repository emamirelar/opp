import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output, signal, SimpleChanges, inject} from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';
import { Button } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import {Textarea} from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { MultiSelectModule } from 'primeng/multiselect';
import { InteractionType, INTERACTION_TYPE_TRANSLATION_KEYS } from '../../../models/interaction-type.enum';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ContactService } from '../../../services/contact.service';
import { PartnerService } from '../../../services/partner.service';
//import { UserService } from '../../../services/user.service';
import { CommonModule } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
import { MessageService } from 'primeng/api';
import { MessageModule } from 'primeng/message';
import { ConfirmDialog } from 'primeng/confirmdialog';
import {Contact} from '../../../models/contact.model';
import { ActivatedRoute, Router } from '@angular/router';
import {Partner} from '../../../models/partner.model';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { CalendarModule } from 'primeng/calendar';
import { InteractionModalFooterComponent } from './footer/interaction-modal-footer.component';
import { NgIf } from '@angular/common';
import { ChipModule, Chip } from 'primeng/chip';
import { AutoCompleteModule } from 'primeng/autocomplete';

@Component({
  selector: 'app-interaction-modal',
  templateUrl: './interaction-modal.component.html',
  styleUrl: './interaction-modal.component.scss',
  imports: [
    ReactiveFormsModule,
    FormsModule,
    CalendarModule,
    Button,
    InputTextModule,
    Textarea,
    SelectModule,
    MultiSelectModule,
    TranslateModule,
    CommonModule,
    MessageModule,
    ConfirmDialog,
    NgIf,
    ChipModule,
    AutoCompleteModule
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

  formGroup: FormGroup;

  typeOptions = Object.values(InteractionType).map(type => ({
    label: INTERACTION_TYPE_TRANSLATION_KEYS[type],
    value: type,
    translateKey: INTERACTION_TYPE_TRANSLATION_KEYS[type]
  }));

  contacts: Contact[] = [];
  partners: Partner[] = [];
  emailOptions: string[] = [];
  invalidEmails: string[] = [];
  showValidationFailedError = signal<boolean>(false);

  constructor(
    private fb: FormBuilder,
    private interactionService: InteractionService,
    protected contactService: ContactService,
    protected partnerService: PartnerService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translateService: TranslateService,
  ) {
    this.formGroup = this.fb.group({
      id: [''],
      type: ['', Validators.required],
      date: [new Date(), Validators.required],
      data: [''],
      contactId: ['', Validators.required],
      contactIds: [[]],
      partnerIds: [[]],
      emailAddresses: [[]],
      phoneNumbers: [[]],
      location: [''],
      subject: ['', Validators.required],
      orgUnitId: [null]
    });
    this.partnerService.getAllPartners();
    this.contactService.getAllContacts();
    //this.userService.getAllContacts();
    
    // Set up the footer template
    this.dialogConfig.templates = {
      footer: InteractionModalFooterComponent
    };
  }

  ngOnInit() {
    this.record = this.dialogConfig.data?.record;
    if (this.record) {
      this.formGroup.patchValue({
        id: this.record.id,
        type: this.record.type,
        date: new Date(this.record.date),
        data: this.record.data,
        contactId: this.record.contactId,
        contactIds: this.record.contactIds,
        partnerIds: this.record.partnerIds,
        emailAddresses: this.record.emailAddresses,
        phoneNumbers: this.record.phoneNumbers,
        location: this.record.location,
        subject: this.record.subject,
        orgUnitId: this.record.orgUnitId
      });
      this.emailOptions = this.record.emailAddresses || [];
    }
    
    // Expose the handleSave function to be called from footer
    if (this.dialogConfig.data) {
      this.dialogConfig.data.handleSave = this.onSubmit.bind(this);
      this.dialogConfig.data.handleDelete = this.deleteInteraction.bind(this);
      this.dialogConfig.data.isSaving = this.isSaving;
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

  onSubmit(): void {
    if (this.formGroup.valid) {
      const formValue = this.formGroup.value;
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
          next: (data) => {
            this.showSuccessMessage('message.interactionCreated');
            this.dialogRef.close(data);
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

  deleteInteraction(): void {
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
}
