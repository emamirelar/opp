import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DynamicDialogRef, DynamicDialogConfig } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-manual-entry-dialog',
  standalone: true,
  imports: [
    ButtonModule,
    InputTextModule,
    MessageModule,
    ReactiveFormsModule,
    TranslateModule
  ],
  template: `
    <div class="p-6">
      <form [formGroup]="manualEntryForm" (ngSubmit)="onSubmit()">
        
        <!-- Information banner -->
        <div class="mb-6 p-4 border-l-4 border-blue-500 bg-blue-50 text-blue-700">
          <div class="flex">
            <div class="flex-shrink-0">
              <i class="pi pi-info-circle text-blue-400"></i>
            </div>
            <div class="ml-3">
              <p class="text-sm font-medium">
                {{ 'manualEntryDialog.banner.title' | translate }}
              </p>
              <p class="mt-1 text-sm">
                {{ 'manualEntryDialog.banner.message' | translate }}
              </p>
            </div>
          </div>
        </div>

        <!-- Google Sheet URL -->
        <div class="mb-4">
          <label for="sheetUrl" class="block text-sm font-medium text-gray-700 mb-2">
            {{ 'manualEntryDialog.form.googleSheetUrl' | translate }} <span class="text-red-500">*</span>
          </label>
          <input
            pInputText
            id="sheetUrl"
            formControlName="url"
            placeholder="{{ 'manualEntryDialog.form.urlPlaceholder' | translate }}"
            class="w-full"
            [class.p-invalid]="manualEntryForm.get('url')?.invalid && manualEntryForm.get('url')?.touched"
          />
          <small class="text-gray-500 mt-1 block">
            {{ 'manualEntryDialog.form.urlHelp' | translate }}
          </small>
          @if (manualEntryForm.get('url')?.invalid && manualEntryForm.get('url')?.touched) {
            <small class="text-red-500 mt-1 block">
              {{ 'manualEntryDialog.form.urlValidation' | translate }}
            </small>
          }
        </div>

        <!-- Sheet Name -->
        <div class="mb-6">
          <label for="sheetName" class="block text-sm font-medium text-gray-700 mb-2">
            {{ 'manualEntryDialog.form.sheetName' | translate }} <span class="text-red-500">*</span>
          </label>
          <input
            pInputText
            id="sheetName"
            formControlName="sheetName"
            placeholder="{{ 'manualEntryDialog.form.sheetNamePlaceholder' | translate }}"
            class="w-full"
            [class.p-invalid]="manualEntryForm.get('sheetName')?.invalid && manualEntryForm.get('sheetName')?.touched"
          />
          <small class="text-gray-500 mt-1 block">
            {{ 'manualEntryDialog.form.sheetNameHelp' | translate }}
          </small>
          @if (manualEntryForm.get('sheetName')?.invalid && manualEntryForm.get('sheetName')?.touched) {
            <small class="text-red-500 mt-1 block">
              {{ 'manualEntryDialog.form.sheetNameValidation' | translate }}
            </small>
          }
        </div>

        <!-- Action buttons -->
        <div class="flex justify-end gap-3">
          <p-button
            type="button"
            label="{{ 'button.cancel' | translate }}"
            severity="secondary"
            [outlined]="true"
            (click)="onCancel()"
          />
          <p-button
            type="submit"
            label="{{ 'button.import' | translate }}"
            icon="pi pi-file-import"
            [disabled]="manualEntryForm.invalid"
            [loading]="isSubmitting()"
          />
        </div>
      </form>
    </div>
  `
})
export class ManualEntryDialogComponent {
  private fb = inject(FormBuilder);
  private dialogRef = inject(DynamicDialogRef);
  private config = inject(DynamicDialogConfig);

  isSubmitting = signal(false);

  manualEntryForm: FormGroup = this.fb.group({
    url: ['', [Validators.required, this.googleSheetUrlValidator]],
    sheetName: ['', [Validators.required, Validators.minLength(1)]]
  });

  /**
   * Custom validator for Google Sheet URLs
   */
  private googleSheetUrlValidator(control: any) {
    if (!control.value) {
      return null; // Let required validator handle empty values
    }
    
    const url = control.value;
    const googleSheetPattern = /^https:\/\/docs\.google\.com\/spreadsheets\/d\/[a-zA-Z0-9-_]+/;
    
    if (!googleSheetPattern.test(url)) {
      return { invalidGoogleSheetUrl: true };
    }
    
    return null;
  }

  onSubmit(): void {
    if (this.manualEntryForm.valid) {
      this.isSubmitting.set(true);
      
      const formValue = this.manualEntryForm.value;
      
      // Close dialog with form data
      this.dialogRef.close({
        url: formValue.url,
        sheetName: formValue.sheetName
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
