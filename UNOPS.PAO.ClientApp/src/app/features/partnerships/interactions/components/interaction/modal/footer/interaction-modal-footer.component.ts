import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { NgIf } from '@angular/common';
import { PermissionUtilityService } from '@core/services/auth';

@Component({
  selector: 'app-interaction-modal-footer',
  template: `
    <div class="flex justify-end flex-wrap w-full gap-4 pt-2">
      <p-button
        *ngIf="config.data?.record?.id && canDelete()"
        type="button"
        [label]="'button.delete' | translate"
        class="p-button-text mr-auto"
        variant="outlined"
        severity="danger"
        (click)="onDelete()"
      ></p-button>
      <p-button
        class="ml-auto"
        [label]="'button.cancel' | translate"
        severity="secondary"
        (click)="onCancel()"
      ></p-button>
      <p-button
        *ngIf="canSave()"
        [loading]="getSavingState()"
        icon="pi pi-check"
        [label]="isImportEdit ? 'Update Import Data' : ('button.save' | translate)"
        (click)="onSave()"
      ></p-button>
    </div>
  `,
  standalone: true,
  imports: [
    TranslateModule,
    ButtonModule,
    NgIf
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InteractionModalFooterComponent {
  private dialogRef = inject(DynamicDialogRef);
  protected config = inject(DynamicDialogConfig);
  private permissionUtilityService = inject(PermissionUtilityService);

  // Check if this is an edit for import data
  get isImportEdit(): boolean {
    const record = this.config.data?.record;
    return record?.isImportEdit || record?.skipServerSave || false;
  }

  /**
   * @uiButton cancel_interaction_dialog
   * @description Closes the interaction dialog without saving any changes
   * @label Cancel
   * @icon pi pi-times
   * @when_to_use When you want to discard changes and close the interaction dialog
   * @permissions None required
   */
  onCancel(): void {
    this.dialogRef.close();
  }

  /**
   * @uiButton save_interaction_footer
   * @description Triggers the save action for the interaction form from the footer
   * @label Save
   * @icon pi pi-check
   * @when_to_use When all interaction details are filled and you want to save the record
   * @permissions INTERACTION_CREATE, INTERACTION_UPDATE
   */
  onSave(): void {
    if (this.config.data?.handleSave) {
      this.config.data.handleSave();
    }
  }

  /**
   * @uiButton delete_interaction_footer
   * @description Triggers the delete action for the interaction from the footer
   * @label Delete
   * @icon pi pi-trash
   * @when_to_use When you want to permanently remove an existing interaction record
   * @permissions INTERACTION_DELETE
   */
  onDelete(): void {
    if (this.config.data?.handleDelete) {
      this.config.data.handleDelete();
    }
  }

  canSave(): boolean {
    // For import edits, always allow saving since it's just updating local data
    if (this.isImportEdit) {
      return true;
    }

    const recordPermissions = this.config.data?.recordPermissions;
    if (!recordPermissions) return true; // Default to allow if no permissions data

    const isEdit = !!this.config.data?.record?.id;
    return isEdit
      ? this.permissionUtilityService.canUpdate(recordPermissions())
      : this.permissionUtilityService.canCreate(recordPermissions());
  }

  canDelete(): boolean {
    const recordPermissions = this.config.data?.recordPermissions;
    if (!recordPermissions) return true; // Default to allow if no permissions data

    return this.permissionUtilityService.canDelete(recordPermissions());
  }

  getSavingState(): boolean {
    // For import edits, don't show loading state
    if (this.isImportEdit) {
      return false;
    }

    // Check if isSaving exists and is a function in config.data
    const isSaving = this.config.data?.isSaving;
    return isSaving && typeof isSaving === 'function' ? isSaving() : false;
  }
}
