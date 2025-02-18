import { NgClass } from '@angular/common';
import { Component, Input } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { FeedbackDialogService } from '../../../pages/services/feedback-dialog.service';
import { HttpClient } from '@angular/common/http';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-document-list',
  imports: [NgClass, TableModule, ButtonModule, TranslateModule],
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.scss'
})
export class DocumentListComponent {
  @Input() documents: any[] = [];

  constructor(private http: HttpClient,
    private messageService: FeedbackDialogService) { }

  openDocument(documentLink: string) {
    if (!documentLink) {
      this.messageService.showErrorToast({
        summary: 'Error',
        detail: 'Document link is not available'
      });
      return;
    }

    const newWindow = window.open(documentLink, '_blank');
    if (newWindow) {
      newWindow.opener = null;
      newWindow.focus();
    }
  }

}