import { Component, EventEmitter, Input, ChangeDetectionStrategy, inject, OnInit, OnChanges, output, Output, signal } from '@angular/core';
import { PartnerTreeService } from '../../../services/partner-tree.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { PanelModule } from 'primeng/panel';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { CommonModule } from '@angular/common';
import { SelectModule } from 'primeng/select';
import { DialogModule } from 'primeng/dialog';
import { PartnerTree } from '../../../models/partner-tree.model';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { PartnerTreeItemFooterComponent } from './partner-tree-item-footer.component';

interface PartnerTreeFormControls {
  id: AbstractControl<number | null>;
  name: AbstractControl<string | null>;
  description: AbstractControl<string | null>;
  code: AbstractControl<string | null>;
  type: AbstractControl<string | null>;
  parent: AbstractControl<string | null>;
  partnerCategoryCode: AbstractControl<string | null>;
  partnerGroupCode: AbstractControl<string | null>;
  status: AbstractControl<string | null>;
}

@Component({
  selector: 'app-partner-tree-item',
  imports: [
    ReactiveFormsModule, 
    SelectModule, 
    TranslateModule, 
    ButtonModule, 
    PanelModule, 
    InputTextModule, 
    CommonModule, 
    TextareaModule, 
    DialogModule,
    PartnerTreeItemFooterComponent
  ],
  templateUrl: './partner-tree-item.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerTreeItemComponent implements OnInit, OnChanges {
  @Input() record?: PartnerTree;
  private dialogRef = inject(DynamicDialogRef);
  private dialogConfig = inject(DynamicDialogConfig);
  isFormInvalid = signal<boolean>(false);
  parent?: PartnerTree;

  partnerTreeService = inject(PartnerTreeService);
  feedbackDialogService = inject(FeedbackDialogService);
  formGroup = new FormGroup<PartnerTreeFormControls>({
    id: new FormControl<number | null>(null),
    name: new FormControl<string | null>('', {
      validators: [Validators.required]
    }),
    description: new FormControl<string | null>('', {
      validators: [Validators.required]
    }),
    code: new FormControl<string | null>('', {
      validators: [Validators.required]
    }),
    type: new FormControl<string | null>('', {
      validators: [Validators.required]
    }),
    parent: new FormControl<string | null>(null),
    partnerCategoryCode: new FormControl<string | null>(null),
    partnerGroupCode: new FormControl<string | null>(null),
    status: new FormControl<string | null>('', {
      validators: [Validators.required]
    }),
  });

  cachedDataService = inject(CachedDataService);
  allTypeData = this.cachedDataService.allPartnerLevelTypes;
  allStatusData = this.cachedDataService.allStatus;
  parentOptions: PartnerTree[] = [];
  filteredPartnerGroupOptions: PartnerTree[] = [];

  constructor() {
    this.record = this.dialogConfig.data?.record;
    this.parentOptions = this.partnerTreeService.parentOptions;
    
    // Setup footer template and bind actions
    this.dialogConfig.templates = {
      footer: PartnerTreeItemFooterComponent
    };
    this.dialogConfig.data = {
      ...this.dialogConfig.data,
      handleDelete: () => this.handleDelete(),
      handleActivate: () => this.handleActivate(),
      handleSave: () => this.handleSave(),
      record: this.record,
      isFormInvalid: this.isFormInvalid
    };
  }

  ngOnInit() {
    if (this.record) {
      this.formGroup.patchValue(this.record);
      const parentCode = this.record?.parent;
      if (parentCode) {
        this.parent = this.parentOptions.find(p => p.code === parentCode);
      }
    }
    this.updateFormValidity();
    
    // Subscribe to form status changes
    this.formGroup.statusChanges.subscribe(() => {
      this.updateFormValidity();
    });
  }

  ngOnChanges() {
    if (this.record) {
      this.formGroup.patchValue(this.record);
      this.updateFormValidity();
    }
  }

  handleDelete() {
    if (!this.record?.id) return;
    
    const recordToDelete = { ...this.record, status: '0' };
    this.partnerTreeService.updatePartnerTreeLevel([recordToDelete]).subscribe({
      next: (data: any) => {
        if (this.record?.id) {
          this.partnerTreeService.deletePartnerLevel(this.record.id.toString()).subscribe({
            next: (data: any) => {
              this.feedbackDialogService.showSuccessToast({ detail: 'Record deleted successfully!' });
              this.dialogRef.close(data);
            }
          });
        }
      }
    });
  }

  handleActivate() {
    if (this.formGroup.valid) {
      const formValue = this.formGroup.value;
      // Find parent object from parentOptions using the parent code
      if (formValue.parent) {
        this.parent = this.parentOptions.find(p => p.code === formValue.parent);
      }
      
      const payload: PartnerTree = {
        id: formValue.id || undefined,
        name: formValue.name || undefined,
        description: formValue.description || undefined,
        code: formValue.code || undefined,
        type: formValue.type || undefined,
        parent: formValue.parent || undefined,
        partnerCategoryCode: formValue.partnerCategoryCode || undefined,
        partnerGroupCode: formValue.partnerGroupCode || undefined,
        status: '1' // Active
      };

      this.partnerTreeService.updatePartnerTreeLevel([payload]).subscribe({
        next: (results: PartnerTree[]) => {
          this.feedbackDialogService.showSuccessToast({ detail: 'Record activated successfully!' });
          this.dialogRef.close(results[0]);
        },
        error: (error: any) => {
          this.feedbackDialogService.showErrorToast({ detail: 'Failed to activate record' });
        }
      });
    }
  }

  handleSave() {
    if (this.formGroup.valid) {
      const formValue = this.formGroup.value;
      // Find parent object from parentOptions using the parent code
      if (formValue.parent) {
        this.parent = this.parentOptions.find(p => p.code === formValue.parent);
      }
      
      const payload: PartnerTree = {
        id: formValue.id || undefined,
        name: formValue.name || undefined,
        description: formValue.description || undefined,
        code: formValue.code || undefined,
        type: formValue.type || undefined,
        parent: formValue.parent || undefined,
        partnerCategoryCode: formValue.partnerCategoryCode || undefined,
        partnerGroupCode: formValue.partnerGroupCode || undefined,
        status: formValue.status || undefined
      };

      if (this.record?.id) {
        // Update existing record
        this.partnerTreeService.updatePartnerTreeLevel([payload]).subscribe({
          next: (results: PartnerTree[]) => {
            this.feedbackDialogService.showSuccessToast({ detail: 'Record updated successfully!' });
            this.dialogRef.close(results[0]);
          },
          error: (error: any) => {
            this.feedbackDialogService.showErrorToast({ detail: 'Failed to update record' });
          }
        });
      } else {
        // Create new record
        this.partnerTreeService.createPartnerTreeLevel(payload).subscribe({
          next: (result: PartnerTree) => {
            this.feedbackDialogService.showSuccessToast({ detail: 'Record created successfully!' });
            this.dialogRef.close(result);
          },
          error: (error: any) => {
            this.feedbackDialogService.showErrorToast({ detail: 'Failed to create record' });
          }
        });
      }
    }
  }

  onNameBlur() {
    const nameValue = this.formGroup.get('name')?.value;
    const codeValue = this.formGroup.get('code')?.value;
    
    // Only update code if it's empty and name has a value
    if (!codeValue && nameValue) {
      const formattedCode = nameValue.toUpperCase().replace(/\s+/g, '_');
      this.formGroup.patchValue({ code: formattedCode });
    }
  }

  hide() {
    this.dialogRef.close();
  }

  private updateFormValidity() {
    this.isFormInvalid.set(this.formGroup.invalid);
  }

  canEditPartnerCategory() {
    return this.formGroup.get('type')?.value === 'Level_1' || (this.formGroup.get('type')?.value === 'Level_2' && !this.parent?.partnerCategoryEditable);
  }

  canEditPartnerGroup() {
    if (!this.parent) return false;

    return this.parent?.partnerGroupEditable || this.parent?.partnerCategoryEditable;
  }


}
