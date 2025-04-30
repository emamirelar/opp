import { ChangeDetectionStrategy, Component, inject, OnInit, Signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';

@Component({
  selector: 'app-partner-tree-item-footer',
  standalone: true,
  imports: [ButtonModule, TranslateModule, CommonModule],
  template: `
    <div class="flex gap-2 mt-4 ">
      <p-button 
        icon="pi pi-trash" 
        outlined="" 
        *ngIf="record?.status === 'Active' && record?.id !== null" 
        severity="warn" 
        [label]="'button.inactive' | translate" 
        (onClick)="onDelete()"
      />
      <p-button 
        icon="pi pi-trash" 
        outlined="" 
        *ngIf="record?.status === 'Inactive' && record?.id !== null" 
        severity="warn" 
        [label]="'button.active' | translate" 
        (onClick)="onActivate()"
      />
      <div class="grow"></div>
      <p-button 
        icon="pi pi-times" 
        [label]="'button.cancel' | translate" 
        (onClick)="onCancel()" 
        severity="secondary"
      />
      <p-button 
        icon="pi pi-check" 
        severity="success" 
        [label]="'button.save' | translate" 
        (onClick)="onSave()" 
        [disabled]="isFormInvalid()"
      />
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: `
    :host {
      display: block;
      width: 100%;
    }
  `
})
export class PartnerTreeItemFooterComponent implements OnInit {
  private dialogRef = inject(DynamicDialogRef);
  private config = inject(DynamicDialogConfig);

  record: any;
  isFormInvalid!: Signal<boolean>;

  ngOnInit() {
    this.record = this.config.data?.record;
    this.isFormInvalid = this.config.data?.isFormInvalid;
  }

  onDelete(): void {
    if (this.config.data?.handleDelete) {
      this.config.data.handleDelete();
    }
  }

  onActivate(): void {
    if (this.config.data?.handleActivate) {
      this.config.data.handleActivate();
    }
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
