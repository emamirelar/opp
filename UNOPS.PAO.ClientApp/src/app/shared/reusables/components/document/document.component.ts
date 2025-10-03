import { Component, inject, input, OnInit, signal, effect, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { FormsModule } from '@angular/forms';

//NGPrime
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { PaginatorModule } from 'primeng/paginator';
import { DialogModule } from 'primeng/dialog';
import { Menu } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';

import { UploadDocumentComponent } from './upload/upload-document.component';
import { DocumentService } from './../../../services/document.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FeedbackDialogService } from '../../../services/feedback-dialog.service';
import { AuthService } from '@core/services/auth.service';
import { DocumentLinkModel } from '../../../interfaces/document.interface';

@Component({
  selector: 'app-document',
  standalone: true,
  imports: [
    TableModule,
    ButtonModule,
    PaginatorModule,
    DialogModule,
    UploadDocumentComponent,
    TranslateModule,
    Menu,
    SelectModule,
    FormsModule,
    TooltipModule,
  ],
  templateUrl: './document.component.html',
  styleUrl: './document.component.scss',
})
export class DocumentComponent implements OnInit {
  documentService = inject(DocumentService);
  feedbackService = inject(FeedbackDialogService);
  translateService = inject(TranslateService);
  private destroyRef = inject(DestroyRef);

  isReadOnly = input<boolean>(false);
  entityName = input<string>('');
  entityId = input<string>('');
  acceptedFormat = input<string>('*');
  documents = signal([]);
  canPreview = input<boolean>(true);
  canDownload = input<boolean>(true);
  canDelete = input<boolean>(true);
  disabled = input<boolean>(false);
  showUploadButton = input<boolean>(true);
  isLoading = this.documentService.isLoading;

  showUploadFile: boolean = false;
  items: MenuItem[] = [];
  selectedDocument: any = null;
  pendingFiles = signal<any[]>([]);
  documentTypes = signal<any[]>([]);

  get scrollHeightValue() {
    return this.documents().length > 0 || this.pendingFiles().length > 0 ? 'flex' : undefined;
  }

  get allDocuments() {
    return [...this.documents(), ...this.pendingFiles()];
  }

  constructor(private authService: AuthService) {
    // Effect to watch for changes in entityName and entityId
    effect(() => {
      const entityName = this.entityName();
      const entityId = this.entityId();
      
      // Load document types when entityName changes
      if (entityName) {
        this.loadDocumentTypes();
      }
      
      // Only load documents if both entityName and entityId are provided
      if (entityName && entityId) {
        this.load();
      }
    });
  }

  ngOnInit(): void {
    // Initial load will be handled by the effect
    // this.load(); - removed as it's now handled by the effect
  }

  load() {
    // Only proceed if we have the required parameters
    if (!this.entityName() || !this.entityId()) {
      this.documents.set([]);
      return;
    }

    this.documentService.getDocuments(this.entityName(), this.entityId()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data: any) => {
        this.documents.set(data);
      },
    });
  }

  loadDocumentTypes() {
    if (this.entityName()) {
      this.documentService.getDocumentTypesByEntityName(this.entityName()).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
        next: (data: any) => {
          this.documentTypes.set(data.records || []);
        },
      });
    }
  }

  addPendingFiles(files: any[]) {
    // Add files to pending list with default properties
    const newPendingFiles = files.map(file => ({
      ...file,
      isPending: true,
      selectedDocumentType: null,
      isSaving: false
    }));
    this.pendingFiles.set([...this.pendingFiles(), ...newPendingFiles]);
  }

  removePendingFile(fileToRemove: any) {
    const updatedFiles = this.pendingFiles().filter(file => file.id !== fileToRemove.id);
    this.pendingFiles.set(updatedFiles);
  }

  savePendingFile(file: any) {
    if (!file.selectedDocumentType) {
      this.feedbackService.showInfoToast({
        detail: this.translateService.instant('message.selectDocumentTypeRequired'),
      });
      return;
    }

    // Set saving state
    file.isSaving = true;
    
    const documentLinkModel: DocumentLinkModel = {
      link: file.url,
      name: file.name,
      type: file.mimeType,
      googleId: file.id,
      documentTypeId: file.selectedDocumentType.id,
      parentEntityName: this.entityName(),
      parentEntityId: parseInt(this.entityId()),
    };

    this.documentService.linkFile(documentLinkModel).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (response: any) => {
        this.feedbackService.showSuccessToast({ 
          detail: this.translateService.instant('message.documentLinkedSuccessfully', { fileName: response.name })
        });
        
        // Remove from pending files
        this.removePendingFile(file);
        
        // Reload document list
        this.load();
      },
      error: () => {
        file.isSaving = false;
      }
    });
  }

  handleOnUploadDocumentDialogClose(uploadDocument: any) {
    uploadDocument.clear();
  }

  handleOnClickUploadDocumentDialogUploadBtn(uploadDocument: any) {
    uploadDocument.uploadFiles();
  }

  handleOnUploadDocumentSuccess() {
    this.showUploadFile = false;
    this.load();
  }

  /**
   * Opens the upload document dialog
   */
  openUploadDialog() {
    this.showUploadFile = true;
  }

  handleOnMenuButtonClick(event: any, document: any, menu: any) {
    this.selectedDocument = document;
    this.configureMenuAsPerDocument(document);
    menu.show(event);
  }

  getDocumentIconCls(document: any) {
    let iconCls = 'pi pi-file';
    switch (document.type) {
      case 'application/vnd.google-apps.spreadsheet':
      case 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet':
        iconCls = 'pi pi-file-excel file-sheet';
        break;

      case 'application/pdf':
        iconCls = 'pi pi-file-pdf file-pdf';
        break;

      case 'application/vnd.openxmlformats-officedocument.wordprocessingml.document':
      case 'text/plain':
        iconCls = 'pi pi-file-word file-doc';
        break;

      default:
        iconCls = 'pi pi-file file-doc';
        break;
    }
    return iconCls;
  }

  /**
   * Preview a document by opening it in a new tab
   * @param document The document to preview
   */
  previewDocument(document: any) {
    if (!document || !document.link) {
      return;
    }
    window.open(document.link, '_blank');
  }

  private handleOnDocumentPreview() {
    //exit
    if (this.selectedDocument == null) {
      return;
    }

    window.open(this.selectedDocument.link, '_blank');
  }

  private handleOnDocumentDelete() {
    //exit
    if (this.selectedDocument == null) {
      return;
    }

    this.documentService.delete(this.selectedDocument.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.feedbackService.showSuccessToast({
          detail: this.translateService.instant('message.documentDeleteSuccess'),
        });
        this.load();
      },
    });
  }

  private getDocumentDownloadType(document: any) {
    let downloadType = '';
    switch (document.type) {
      case 'application/vnd.google-apps.spreadsheet':
        downloadType = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
        break;

      case 'application/vnd.google-apps.document':
        downloadType = 'application/vnd.openxmlformats-officedocument.wordprocessingml.document';
        break;

      default:
        downloadType = document.type;
        break;
    }
    return downloadType;
  }

  private handleOnDocumentDownload() {
    //exit
    if (this.selectedDocument == null) {
      return;
    }

    this.documentService.download(this.selectedDocument.id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data: any) => {
        const downloadedFile = new Blob([data], { type: this.getDocumentDownloadType(this.selectedDocument) });
        const a = document.createElement('a');
        a.setAttribute('style', 'display:none;');
        document.body.appendChild(a);
        a.download = this.selectedDocument.name;
        a.href = URL.createObjectURL(downloadedFile);
        a.target = '_blank';
        a.click();
        document.body.removeChild(a);

        this.feedbackService.showSuccessToast({
          detail: this.translateService.instant('message.documentDownloadSuccess'),
        });
        this.load();
      },
    });
  }

  private configureMenuAsPerDocument(document: any) {
    let menuItem: MenuItem[] = [];

    if (document.link && this.canPreview()) {
      menuItem.push({
        label: this.translateService.instant('button.preview'),
        icon: 'pi pi-eye',
        command: () => {
          this.handleOnDocumentPreview();
        },
      });
    }

    if (this.canDownload()) {
      menuItem.push({
        label: this.translateService.instant('button.download'),
        icon: 'pi pi-download',
        command: () => {
          this.handleOnDocumentDownload();
        },
      });
    }

    if (this.isReadOnly() !== true && this.canDelete()) {
      menuItem.push({
        label: this.translateService.instant('button.delete'),
        icon: 'pi pi-trash',
        command: () => {
          this.handleOnDocumentDelete();
        },
      });
    }

    this.items = menuItem;
  }
}
