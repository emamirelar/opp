import { Component, inject, input, OnDestroy, OnInit, output } from '@angular/core';

//NGPrime
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { FormsModule } from '@angular/forms';
import { BlockUIModule } from 'primeng/blockui';

import { DrivePickerService } from '@shared/services/integration/drive-picker.service';
import { DocumentService } from '@shared/services/api/document.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { DocumentLinkModel, Documentype } from '@shared/interfaces/document.interface';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-document-gdrive-addlink',
  imports: [TableModule, ButtonModule, SelectModule, FormsModule, BlockUIModule, TranslateModule],
  templateUrl: './document-gdrive-addlink.component.html',
  styleUrl: './document-gdrive-addlink.component.scss',
})
export class GDriveAddLinkComponent implements OnInit, OnDestroy {
  entityName = input<string>('');
  entityId = input<string>('');
  acceptedMIMETypes = input<string>('');
  onAddLinkSuccess = output();
  files: any = [];
  documentTypes: Documentype[] = [];

  feedbackService = inject(FeedbackDialogService);
  documentService: DocumentService = inject(DocumentService);
  drivePickerService = inject(DrivePickerService);

  get scrollHeightValue() {
    return this.files.length > 0 ? 'flex' : undefined;
  }

  constructor() {}

  ngOnInit(): void {
    this.documentService.getDocumentTypesByEntityName(this.entityName()).subscribe({
      next: (data: any) => {
        this.documentTypes = data.records;
      },
    });

    //set MimeTypes
    this.drivePickerService.setAcceptedMIMETypes(this.acceptedMIMETypes());

    this.drivePickerService.onFilesSelectedEmitter.subscribe({
      next: (event: any) => {
        this.onSelectedFiles(event);
      },
    });
  }

  ngOnDestroy(): void {
    //reset MimeTypes
    this.drivePickerService.setAcceptedMIMETypes('');
  }

  onSelectedFiles(event: any) {
    this.files.push(...event.files);
  }

  onValueChange(event: any, file: any) {
    
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

  addLinks() {
    //exit condition
    if (this.files.length <= 0) {
      return;
    }

    let canAddLinks = this.validateFiles();
    if (canAddLinks !== true) {
      this.feedbackService.showInfoToast({
        detail: 'Please select a document type.',
      });
      return;
    }

    const file = this.files[0];
    const req: DocumentLinkModel = {
      link: file.url,
      name: file.name,
      type: file.mimeType,
      googleId: file.id,
      documentTypeId: this.getDocumentTypeIdByName(file.docType),
      parentEntityName: this.entityName(),
      parentEntityId: parseInt(this.entityId()),
    };

    this.documentService.linkFile(req).subscribe({
      next: (response: any) => {
        this.feedbackService.showSuccessToast({ detail: `File ${response.name} linked successfully!` });
        this.onAddLinkSuccess.emit();
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