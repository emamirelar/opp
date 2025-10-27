import { Component, EventEmitter, Input, ChangeDetectionStrategy, inject, OnInit, OnChanges, output, Output, signal } from '@angular/core';
import { PartnerTreeService } from '@partnerships/partners/services/partner-tree.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { PanelModule } from 'primeng/panel';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CachedDataService } from '@shared/services/utils';
import { FeedbackDialogService } from '@shared/services/ui';
import { CommonModule } from '@angular/common';
import { SelectModule } from 'primeng/select';
import { DialogModule } from 'primeng/dialog';
import { PartnerTree } from '@partnerships/partners/models/partner-tree.model';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';

import { PermissionUtilityService } from '@core/services/auth';
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
})
export class PartnerTreeItemComponent implements OnInit, OnChanges {
  @Input() record?: PartnerTree;
  private dialogRef = inject(DynamicDialogRef);
  private dialogConfig = inject(DynamicDialogConfig);
  isFormInvalid = signal<boolean>(false);
  parent?: PartnerTree;


  partnerTreeService = inject(PartnerTreeService);
  feedbackDialogService = inject(FeedbackDialogService);
  translateService = inject(TranslateService);

  // RBAC permissions
  permissionUtilityService = inject(PermissionUtilityService);
  recordPermissionsData = this.permissionUtilityService.createInstancePermissions('PartnerTree');
  recordPermissions = this.recordPermissionsData.recordPermissions;

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
  partnerCategoryOptions: PartnerTree[] = [];
  filteredPartnerGroupOptions: PartnerTree[] = [];
  allPartnerGroupOptions: PartnerTree[] = [];

  constructor() {
    this.record = this.dialogConfig.data?.record;
    this.parentOptions = this.partnerTreeService.parentOptions;

    // Setup footer template and bind actions
    // Note: Footer component removed as it was not used in template
    this.dialogConfig.data = {
      ...this.dialogConfig.data,
      handleDelete: () => this.handleDelete(),
      handleActivate: () => this.handleActivate(),
      handleSave: () => this.handleSave(),
      record: this.record,
      isFormInvalid: this.isFormInvalid,
      recordPermissions: this.recordPermissions,
      permissionUtilityService: this.permissionUtilityService
    };
  }

  ngOnInit() {
    if (this.record) {
      this.formGroup.patchValue(this.record);
      const parentCode = this.record?.parent;
      if (parentCode) {
        this.parent = this.parentOptions.find(p => p.code === parentCode);
      }

      // Extract permissions from record if available
      if (this.record.permissions) {
        this.recordPermissions.set({
          entity: 'PartnerTree',
          hasAccess: true,
          permissions: this.record.permissions
        });
      } else if (this.record.id) {
        // Load permissions for existing record
        this.recordPermissionsData.loadPermissions(this.record.id.toString());
      } else {
        // For new records without ID, set default permissions that allow creation
        // The list component already checked entity-level permissions before opening this modal
        this.recordPermissions.set({
          entity: 'PartnerTree',
          hasAccess: true,
          permissions: {
            canRead: true,
            canCreate: true,
            canUpdate: true,
            canDelete: false,
            canExport: false,
            canImport: false
          }
        });
      }
    } else {
      // For completely new records, set default permissions that allow creation
      // The list component already checked entity-level permissions before opening this modal
      this.recordPermissions.set({
        entity: 'PartnerTree',
        hasAccess: true,
        permissions: {
          canRead: true,
          canCreate: true,
          canUpdate: true,
          canDelete: false,
          canExport: false,
          canImport: false
        }
      });
    }

    // Initialize options from service (after patchValue so category filtering works correctly)
    this.loadPartnerCategoryAndGroupOptions();

    // Update control states based on backend properties

    this.updateFormValidity();

    // Subscribe to form status changes
    this.formGroup.statusChanges.subscribe(() => {
      this.updateFormValidity();
    });

    // Subscribe to partner category changes to update group options
    this.formGroup.get('partnerCategoryCode')?.valueChanges.subscribe((categoryCode) => {
      this.updatePartnerGroupOptions(categoryCode);
    });

    // Subscribe to form changes that affect editing permissions (type, code, parent)
    this.formGroup.get('type')?.valueChanges.subscribe(() => {
      // Trigger change detection for conditional rendering
    });
    this.formGroup.get('code')?.valueChanges.subscribe(() => {
      // Trigger change detection for conditional rendering
    });
    this.formGroup.get('parent')?.valueChanges.subscribe(() => {
      // Trigger change detection for conditional rendering
    });
  }

  ngOnChanges() {
    if (this.record) {
      this.formGroup.patchValue(this.record);
      this.updateFormValidity();
    }
  }

  handleDelete() {
    // Check permission before deleting
    if (!this.permissionUtilityService.canDelete(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('messages.error.noDeletePermission')
      });
      return;
    }

    if (!this.record?.id) return;

    const recordToDelete = { ...this.record, status: '0' };
    this.partnerTreeService.updatePartnerTreeLevel([recordToDelete]).subscribe({
      next: (data: any) => {
        if (this.record?.id) {
          this.partnerTreeService.deletePartnerLevel(this.record.id.toString()).subscribe({
            next: (data: any) => {
              this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('messages.success.recordDeleted') });

              // Force refresh of cached partner data
              this.cachedDataService.refreshPartners();

              // Close with success indicator to trigger list refresh
              this.dialogRef.close({ success: true, deleted: true, data: data });
            },
            error: (error: any) => {
              this.feedbackDialogService.showErrorToast({ detail: this.translateService.instant('messages.error.failedToDelete') });
            }
          });
        }
      },
      error: (error: any) => {
        this.feedbackDialogService.showErrorToast({ detail: this.translateService.instant('messages.error.failedToUpdateStatus') });
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
          this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('messages.success.recordActivated') });
          this.dialogRef.close(results[0]);
        },
        error: (error: any) => {
          this.feedbackDialogService.showErrorToast({ detail: this.translateService.instant('messages.error.failedToActivate') });
        }
      });
    }
  }

  handleSave() {
    // Check permission before saving
    const isNewRecord = !this.record?.id;
    const requiredPermission = isNewRecord ? 'canCreate' : 'canUpdate';

    if (isNewRecord && !this.permissionUtilityService.canCreate(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('messages.error.noCreatePermission')
      });
      return;
    }

    if (!isNewRecord && !this.permissionUtilityService.canUpdate(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: this.translateService.instant('messages.error.noUpdatePermission')
      });
      return;
    }

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
            this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('messages.success.recordUpdated') });
            this.dialogRef.close(results[0]);
          },
          error: (error: any) => {
            this.feedbackDialogService.showErrorToast({ detail: this.translateService.instant('messages.error.failedToUpdate') });
          }
        });
      } else {
        // Create new record
        this.partnerTreeService.createPartnerTreeLevel(payload).subscribe({
          next: (result: PartnerTree) => {
            this.feedbackDialogService.showSuccessToast({ detail: this.translateService.instant('messages.success.recordCreated') });
            this.dialogRef.close(result);
          },
          error: (error: any) => {
            this.feedbackDialogService.showErrorToast({ detail: this.translateService.instant('messages.error.failedToCreate') });
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
      const formattedCode = nameValue.toUpperCase().replace(/\s+/g, '_').substring(0, 25);
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
    // Business Rule: GOVERNMENT or MULTILATERAL cannot change Partner Category or Partner Group
    if (this.isGovernmentOrMultilateral()) {
      return false;
    }

    // Business Rule: Level 1 OR Level 2 with GOVERNMENT/MULTILATERAL parent can change Partner Category
    if (this.isLevel1OrLevel2WithRestrictedParent()) {
      return true;
    }

    // All other Partner Trees cannot change Partner Category
    return false;
  }

  canEditPartnerGroup() {
    // Business Rule: GOVERNMENT or MULTILATERAL cannot change Partner Category or Partner Group
    if (this.isGovernmentOrMultilateral()) {
      return false;
    }

    // Business Rule: Level 1 OR Level 2 with GOVERNMENT/MULTILATERAL parent cannot change Partner Group
    if (this.isLevel1OrLevel2WithRestrictedParent()) {
      return false;
    }

    // All other Partner Trees can change Partner Group
    return true;
  }

  private isGovernmentOrMultilateral(): boolean {
    const code = this.record?.code || this.formGroup.get('code')?.value;
    return code === 'GOVERNMENT' || code === 'MULTILATERAL';
  }

  private isLevel1OrLevel2WithRestrictedParent(): boolean {
    const type = this.record?.type || this.formGroup.get('type')?.value;
    const parent = this.record?.parent || this.formGroup.get('parent')?.value;

    // Level 1 can change Partner Category
    if (type === 'Level_1') {
      return true;
    }

    // Level 2 with GOVERNMENT or MULTILATERAL parent can change Partner Category
    if (type === 'Level_2' && (parent === 'GOVERNMENT' || parent === 'MULTILATERAL')) {
      return true;
    }

    return false;
  }

  private loadPartnerCategoryAndGroupOptions() {
    // Filter parent options to get only categories (Level_1 items typically)
    this.partnerCategoryOptions = this.parentOptions.filter(item =>
      item.type === 'Level_1' || item.partnerCategoryEditable === true
    );

    // Get all partner group options
    this.allPartnerGroupOptions = this.parentOptions.filter(item =>
      item.type === 'Level_2' || item.partnerGroupEditable === true
    );

    // Initialize filtered options based on current category selection
    const currentCategoryCode = this.formGroup.get('partnerCategoryCode')?.value ?? null;
    this.updatePartnerGroupOptions(currentCategoryCode);
  }

  private updatePartnerGroupOptions(categoryCode: string | null) {
    if (!categoryCode) {
      this.filteredPartnerGroupOptions = [];
      return;
    }

    // Filter groups based on selected category
    this.filteredPartnerGroupOptions = this.allPartnerGroupOptions.filter(group =>
      group.parent === categoryCode
    );
  }


}
