import { Injectable, inject, signal } from '@angular/core';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ImportComponent } from './import.component';
import { ImportFooterComponent } from './footer/import-footer.component';
import { Observable, Subject } from 'rxjs';
import { WritableSignal } from '@angular/core';
import { ImportService } from './import.service';
import { FeedbackDialogService } from '../../../pages/services/feedback-dialog.service';

@Injectable({
  providedIn: 'root'
})
export class ImportDialogService {
  private dialogService = inject(DialogService);
  private dialogRef: DynamicDialogRef | null = null;
  private _onClose = new Subject<any>();

  private data = signal<Array<any>>([])


  private _fileUrl = signal<string>('');
  importService = inject(ImportService);
  feedbackDialogService = inject(FeedbackDialogService);

  getData() {
    return this.data();
  }

  openImportDialog(header: string = 'Import'): Observable<any> {
    this.dialogRef = this.dialogService.open(ImportComponent, {
      header,
      width: '90vw',
      height: '100vh',
      closable: true,
      templates: {
        footer: ImportFooterComponent
      }
    });

    // Clear previous subscribers
    this._onClose = new Subject<any>();

    // Subscribe to dialog close and forward the result
    this.dialogRef.onClose.subscribe(result => {
      this._onClose.next(result);
      this._onClose.complete();
      this.dialogRef = null;
    });

    return this._onClose.asObservable();
  }

  /**
   * Close the dialog with an optional result
   */
  closeDialog(result?: any): void {
    if (this.dialogRef) {
      this.dialogRef.close(result);
    }
  }

  /**
   * Get the file URL signal
   */
  getFileUrl(): WritableSignal<string> {
    return this._fileUrl;
  }


  /**
   * Trigger import process
   */
  triggerImport(type: string) {
    this.importService.bulkUpload(this.data(), type).subscribe(() => {
      this.feedbackDialogService.showSuccessToast({ detail: 'Import successful'} );
      this.closeDialog();
    });
  }

  setData(data: any[]) {
    this.data.set(data);
  }
}
