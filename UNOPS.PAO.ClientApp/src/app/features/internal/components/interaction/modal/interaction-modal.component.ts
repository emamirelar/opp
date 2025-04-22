import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output, signal, SimpleChanges, inject} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';
import {Button} from 'primeng/button';
import {Textarea} from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { InteractionType, INTERACTION_TYPE_TRANSLATION_KEYS } from '../../../models/interaction-type.enum';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ContactService } from '../../../services/contact.service';
import { CommonModule } from '@angular/common';
import { ConfirmationService } from 'primeng/api';
import { MessageService } from 'primeng/api';
import { ConfirmDialog } from 'primeng/confirmdialog';
import {Contact} from '../../../models/contact.model';
import { ActivatedRoute, Router } from '@angular/router';
import {Partner} from '../../../models/partner.model';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { CalendarModule } from 'primeng/calendar';
import { InteractionModalFooterComponent } from './footer/interaction-modal-footer.component';

@Component({
  selector: 'app-interaction-modal',
  templateUrl: './interaction-modal.component.html',
  imports: [
    ReactiveFormsModule,
    CalendarModule,
    Button,
    Textarea,
    SelectModule,
    TranslateModule,
    CommonModule,
    ConfirmDialog,
    HttpClientModule,
    AiTranscribeComponent
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
  
  record?: Interaction;
  isSaving = signal(false);

  formGroup: FormGroup;

  typeOptions = Object.values(InteractionType).map(type => ({
    label: INTERACTION_TYPE_TRANSLATION_KEYS[type],
    value: type,
    translateKey: INTERACTION_TYPE_TRANSLATION_KEYS[type]
  }));

  constructor(
    private fb: FormBuilder,
    private interactionService: InteractionService,
    protected contactService: ContactService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translateService: TranslateService,
  ) {
    this.formGroup = this.fb.group({
      id: [''],
      type: ['', Validators.required],
      date: [new Date(), Validators.required],
      data: [''],
      contactId: ['', Validators.required]
    });
    // Ensure contacts are loaded when component is created
    this.contactService.getAllContacts();
    
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
        contactId: this.record.contactId
      });
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

  // Handler for transcription completion
  onTranscriptionCompleted(data: any): void {
    if (data) {
      this.formGroup.patchValue({
        type: data.type || this.formGroup.get('type')?.value,
        date: data.date ? new Date(data.date) : this.formGroup.get('date')?.value,
        data: data.data || this.formGroup.get('data')?.value,
        contactId: data.contactId || this.formGroup.get('contactId')?.value
      });
    }
  }

  // Helper to get proper display name for a contact
  getContactDisplayName(contact: any): string {
    if (!contact) return '';
    
    // Log the contact object to see what properties are available
    console.log('Contact for display:', contact);
    
    const firstName = contact.firstName || contact.FirstName || '';
    const lastName = contact.lastName || contact.LastName || '';
    
    if (firstName || lastName) {
      return `${firstName} ${lastName}`.trim();
    }
    
    // Fallback to email if name components aren't available
    if (contact.email || contact.Email) {
      return contact.email || contact.Email;
    }
    
    // Last resort - return the ID or something to identify the contact
    return `Contact #${contact.id || contact.Id || 'Unknown'}`;
  }

  // Helper to extract the ID from a contact object
  getContactId(contact: any): string | number {
    if (!contact) return '';
    
    // Support both camelCase and PascalCase property naming
    return contact.id || contact.Id || '';
  }
}
