import { ChangeDetectionStrategy, Component, effect, EventEmitter, inject, Input, Output, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TranslateModule } from '@ngx-translate/core';
import { ImportDialogService } from '../import-dialog.service';
import {ImportGoogleSheetService} from '../import-google-sheet.service';

@Component({
  selector: 'app-import-footer',
  standalone: true,
  imports: [ButtonModule, TranslateModule],
  template: `
    <div class="flex justify-content-end gap-4 w-full">
      <div class="flex gap-4 mr-auto items-center">
        <p-button
          label="Select from Google Drive"
          icon="pi pi-google"
          (onClick)="importGoogleSheetService.openPicker()">
        </p-button>

        @if (importDialogService.getFileUrl()()) {
          <div class="font-medium">Selected File: {{ importDialogService.getFileUrl()() }}</div>
        }
      </div>


      <p-button
        [label]="'button.cancel' | translate"
        icon="pi pi-times"
        (onClick)="importDialogService.closeDialog()"
        [text]="true">
      </p-button>

      <p-button
        [label]="'button.import' | translate"
        icon="pi pi-file-import"
        (onClick)="importDialogService.triggerImport()">
      </p-button>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: `:host { width: 100%; }`
})
export class ImportFooterComponent {
  importDialogService = inject(ImportDialogService);
  importGoogleSheetService = inject(ImportGoogleSheetService);
}
