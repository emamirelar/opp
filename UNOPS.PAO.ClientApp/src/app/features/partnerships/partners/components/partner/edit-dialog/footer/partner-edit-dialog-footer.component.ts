import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';

@Component({
  selector: 'app-partner-edit-dialog-footer',
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
        [label]="isImportEdit ? ('button.updateImportData' | translate) : ('button.save' | translate)"
        [loading]="isSaving"
        [disabled]="isSaving"
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
export class PartnerEditDialogFooterComponent {
  private dialogRef = inject(DynamicDialogRef);
  private config = inject(DynamicDialogConfig);
  
  // Check if this is an edit for import data
  get isImportEdit(): boolean {
    const record = this.config.data?.record;
    return record?.isImportEdit || record?.skipServerSave || false;
  }
  
  // Get isSaving state from the main component
  get isSaving(): boolean {
    return this.config.data?.isSaving?.() || false;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    const requestingSaveSignal = this.config.data?.requestingSaveSignal;
    if (requestingSaveSignal) {
      requestingSaveSignal.set(true);
    }
  }
}
