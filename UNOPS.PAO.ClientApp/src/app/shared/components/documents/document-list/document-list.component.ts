import { NgClass } from '@angular/common';
import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-document-list',
  imports: [NgClass, TableModule, ButtonModule, TranslateModule],
  templateUrl: './document-list.component.html',
  styleUrl: './document-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DocumentListComponent {
  @Input() documents: any[] = [];

  constructor(
    private messageService: FeedbackDialogService,
    private translate: TranslateService
  ) { }

  openDocument(documentLink: string) {
    if (!documentLink) {
      this.messageService.showErrorDialog({
        summary: this.translate.instant('documentList.error'),
        detail: this.translate.instant('documentList.documentLinkNotAvailable'),
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
