import { Component, inject, input, output, ViewChild, OnInit, effect } from '@angular/core';

//NGPrime
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { FileUploadModule } from 'primeng/fileupload';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { BlockUIModule } from 'primeng/blockui';

import { DocumentService } from '@shared/services/api/document.service';
import { Documentype } from '../../../../interfaces/document.interface';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-upload-document',
  imports: [TableModule, ButtonModule, FileUploadModule, SelectModule, FormsModule, BlockUIModule, TranslateModule],
  templateUrl: './upload-document.component.html',
  styleUrl: './upload-document.component.scss',
})
export class UploadDocumentComponent {
  entityName = input<string>('');
  entityId = input<string>('');
  multiple = input<boolean>(false);
  acceptedFormat = input<string>('*');
  onUploadSuccess = output();
  files: any = [];
  documentTypes: Documentype[] = [];
  @ViewChild('fileUploadComponent') fileUploadComponent: any;

  feedbackService = inject(FeedbackDialogService);
  documentService: DocumentService = inject(DocumentService);
  translateService = inject(TranslateService);

  get scrollHeightValue() {
    return this.files.length > 0 ? 'flex' : undefined;
  }

  constructor() {
    // Effect to watch for changes in entityName
    effect(() => {
      const entityName = this.entityName();
      
      // Only load document types if entityName is provided
      if (entityName) {
        this.loadDocumentTypes();
      }
    });
  }


  private loadDocumentTypes() {
    if (!this.entityName()) {
      this.documentTypes = [];
      return;
    }

    this.documentService.getDocumentTypesByEntityName(this.entityName()).subscribe({
      next: (data: any) => {
        this.documentTypes = data.records;
      },
    });
  }

  onSelectedFiles(event: any) {
    // for (let file of event.currentFiles) {
    //   this.files.push(file);
    // }
    this.files = event.currentFiles;
    this.fileUploadComponent.clear();
  }

  clear() {
    this.files = [];
  }

  handleOnDeleteFile(file: any) {
    let indexOfItemToBeRemoved = this.files.indexOf(file);
    if (indexOfItemToBeRemoved > -1) {
      this.files.splice(indexOfItemToBeRemoved, 1);
    }
  }

  uploadFiles() {
    //exit condition
    if (this.files.length <= 0) {
      return;
    }

    let canUploadFileds = this.validateFiles();
    if (canUploadFileds !== true) {
      this.feedbackService.showInfoToast({
        detail: this.translateService.instant('message.selectDocumentType'),
      });
      return;
    }
    const formData = new FormData();
    for (let file of this.files) {
      formData.append('file', file);
      formData.append('parentEntityName', this.entityName());
      formData.append('parentEntityId', this.entityId());
      formData.append('documentTypeId', this.getDocumentTypeIdByName(file.docType));
      formData.append('name', file['name']);
    }

    this.documentService.uploadFile(formData).subscribe({
      next: () => {
        this.feedbackService.showSuccessToast({
          detail: this.translateService.instant('message.documentUploadSuccess'),
        });
        this.onUploadSuccess.emit();
      },
    });
  }

  private validateFiles() {
    let result = true;

    for (let file of this.files) {
      if (file.hasOwnProperty('docType') === false || file['docType'] == '') {
        result = false;
        break;
      }
    }

    return result;
  }

  private getDocumentTypeIdByName(docTypeName: string) {
    let docTypeId: any;

    for (let docType of this.documentTypes) {
      if (docType.name == docTypeName) {
        docTypeId = docType.id;
        break;
      }
    }

    return docTypeId;
  }
}
