import { ChangeDetectionStrategy, Component, effect, EventEmitter, inject, Input, Output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { ImportDialogService } from '../import-dialog.service';
import { ImportGoogleSheetService } from '../../import-google-sheet.service';
import { ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ImportService } from '../../import.service';

@Component({
  selector: 'app-import-footer',
  standalone: true,
  imports: [ButtonModule, TranslateModule, ConfirmDialogModule],
  template: `
    <div class="flex justify-content-end gap-4 w-full">
      <div class="flex gap-4 mr-auto items-center">
        @if (importDialogService.getFileUrl()()) {
          <div class="font-medium">Selected File: {{ importDialogService.getFileUrl()() }}</div>
        }
        @if (importService.isProcessingFile()) {
          <div class="text-orange-500 font-medium">
            <i class="pi pi-exclamation-circle mr-1"></i>
            File analysis in progress - please wait
          </div>
        }
      </div>

      <p-button
        [label]="'button.cancel' | translate"
        icon="pi pi-times"
        (onClick)="importDialogService.cancelImport()"
        [text]="true">
      </p-button>

      <p-button
        [label]="'button.import' | translate"
        icon="pi pi-file-import"
        [disabled]="importService.isProcessingFile() || importDialogService.isLoading()"
        (onClick)="importDialogService.triggerImport(importDialogService.getImportType())">
      </p-button>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: `:host { width: 100%; }`
})
export class ImportFooterComponent {
  importDialogService = inject(ImportDialogService);
  importService = inject(ImportService);
}
