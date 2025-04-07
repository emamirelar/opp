import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';

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
        label="{{'button.save' | translate}}"
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
