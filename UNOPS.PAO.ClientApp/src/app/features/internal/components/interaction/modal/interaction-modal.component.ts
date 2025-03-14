import {ChangeDetectionStrategy, Component, EventEmitter, Input, Output, signal} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import { Interaction } from '../../../models/interaction.model';
import { InteractionService } from '../../../services/interaction.service';
import {Dialog} from 'primeng/dialog';
import {InputText} from 'primeng/inputtext';
import {Calendar} from 'primeng/calendar';
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

@Component({
  selector: 'app-interaction-modal',
  templateUrl: './interaction-modal.component.html',
  imports: [
    Dialog,
    InputText,
    ReactiveFormsModule,
    Calendar,
    Button,
    Textarea,
    SelectModule,
    TranslateModule,
    CommonModule,
    ConfirmDialog
  ],
  providers: [
    ConfirmationService,
    MessageService
  ],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionModalComponent {
  @Input() interaction?: Interaction;
  @Output() closeModal = new EventEmitter<void>();
  @Output() deleted = new EventEmitter<void>();

  isSaving = signal(false);

  interactionForm: FormGroup;
  display = true;

  typeOptions = Object.values(InteractionType).map(type => ({
    label: INTERACTION_TYPE_TRANSLATION_KEYS[type],
    value: type,
    translateKey: INTERACTION_TYPE_TRANSLATION_KEYS[type]
  }));

  contacts: Contact[] = [];

  constructor(
    private fb: FormBuilder,
    private interactionService: InteractionService,
    protected contactService: ContactService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private translateService: TranslateService,
  ) {
    this.interactionForm = this.fb.group({
      id: [''],
      type: ['', Validators.required],
      date: [new Date(), Validators.required],
      data: [''],
      contactId: ['', Validators.required]
    });
    this.contactService.getAllContacts();
  }

  ngOnInit() {
    if (this.interaction) {
      this.interactionForm.patchValue({
        id: this.interaction.id,
        type: this.interaction.type,
        date: new Date(this.interaction.date),
        data: this.interaction.data,
        contactId: this.interaction.contactId
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

  onSubmit(): void {
    if (this.interactionForm.valid) {
      const formValue = this.interactionForm.value;
      this.isSaving.set(true);

      if (formValue.id) {
        // Update existing interaction
        this.interactionService.update(formValue).subscribe({
          next: () => {
            this.showSuccessMessage('message.interactionUpdated');
            this.hide();
          },
          error: (error) => this.showErrorMessage('message.errorUpdatingInteraction', error)
        });
      } else {
        // Create new interaction
        this.interactionService.create(formValue).subscribe({
          next: () => {
            this.showSuccessMessage('message.interactionCreated');
            this.hide();
          },
          error: (error) => this.showErrorMessage('message.errorCreatingInteraction', error)
        });
      }
    }
  }

  hide(): void {
    this.display = false;
    this.closeModal.emit();
  }

  deleteInteraction(): void {
    this.confirmationService.confirm({
      message: this.translateService.instant('message.deleteInteractionConfirmation'),
      header: this.translateService.instant('title.confirmation'),
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        const interactionId = this.interactionForm.get('id')?.value;
        if (interactionId) {
          this.interactionService.delete(interactionId).subscribe({
            next: () => {
              this.showSuccessMessage('message.interactionDeleted');
              this.hide();
              this.deleted.emit();
            },
            error: (error) => this.showErrorMessage('message.errorDeletingInteraction', error)
          });
        }
      }
    });
  }
}
