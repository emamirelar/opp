import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { Contact } from '../../../../models/contact.model';

@Component({
  selector: 'app-contact-edit-dialog-footer',
  template: `
    <div class="flex justify-end gap-2 pt-4">
      <p-button
        icon="pi pi-times"
        label="{{'button.cancel' | translate}}"
        (click)="onCancel()"
        severity="secondary">
      </p-button>
      <p-button
        icon="pi pi-check"
        [label]="isImportEdit ? 'Update Import Data' : ('button.save' | translate)"
        (click)="onSave()">
      </p-button>
    </div>
  `,
  standalone: true,
  imports: [
    TranslateModule,
    ButtonModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactEditDialogFooterComponent {
  private dialogRef = inject(DynamicDialogRef);
  private config = inject(DynamicDialogConfig);

  // Check if this is an edit for import data
  get isImportEdit(): boolean {
    const record = this.config.data?.record as Contact | undefined;
    return !!record?.isImportEdit;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.config.data?.handleSave) {
      this.config.data.handleSave();
    }
  }
}
