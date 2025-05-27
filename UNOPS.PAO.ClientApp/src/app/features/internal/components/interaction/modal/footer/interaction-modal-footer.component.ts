import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { NgIf } from '@angular/common';
import { PermissionUtilityService } from '../../../../../../essentials/services/permission-utility.service';

@Component({
  selector: 'app-interaction-modal-footer',
  template: `
    <div class="flex justify-end flex-wrap w-full gap-4">
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
        [loading]="config.data?.isSaving()" 
        icon="pi pi-check" 
        [label]="'button.save' | translate" 
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

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.config.data?.handleSave) {
      this.config.data.handleSave();
    }
  }
  
  onDelete(): void {
    if (this.config.data?.handleDelete) {
      this.config.data.handleDelete();
    }
  }

  canSave(): boolean {
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
} 
