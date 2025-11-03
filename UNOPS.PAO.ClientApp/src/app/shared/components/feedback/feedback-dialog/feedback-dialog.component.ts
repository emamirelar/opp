import { Component, inject } from '@angular/core';
import { MessageService } from 'primeng/api';
import { Dialog } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { FeedbackConfig } from '../../../interfaces/feedback';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-feedback-dialog',
  templateUrl: './feedback-dialog.component.html',
  imports: [ToastModule, Dialog, TranslateModule, ButtonModule],
  styleUrls: ['./feedback-dialog.component.scss'],
})
export class FeedbackDialogComponent {
  dialogConfig: FeedbackConfig | null = null;
  visible = false;
  translateService = inject(TranslateService);

  constructor(private messageService: MessageService, private feedbackService: FeedbackDialogService) {}

  ngOnInit() {
    this.feedbackService.getErrorDialogState().subscribe((config) => {
      this.dialogConfig = config;
      this.visible = !!config;
    });
  }

  onDialogClose() {
    this.feedbackService.hideErrorDialog();
  }

  onConfirm() {
    if (this.dialogConfig?.onConfirm) {
      this.dialogConfig.onConfirm();
    }
    this.feedbackService.hideErrorDialog();
  }

  onCancel() {
    this.feedbackService.hideErrorDialog();
  }

  refreshPage() {
    window.location.reload();
  }
}
