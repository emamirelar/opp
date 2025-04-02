import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../../common/services/language.service';
import { Subscription } from 'rxjs';
import { PartnerService } from '../../services/partner.service';
import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';
import { PartnerNewComponent } from './new/partner-new.component';
import { Partner } from '../../models/partner.model';
import { ListviewComponent } from '../../../../common/pages/components/listview/listview.component';
import { ListViewColumn } from '../../../../common/pages/components/listview/listview.model';

@Component({
  selector: 'app-partner',
  templateUrl: './partner.component.html',
  styleUrl: './partner.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    ButtonModule,
    PartnerNewComponent,
    TranslateModule,
    ListviewComponent,
  ]
})
export class PartnerComponent implements OnDestroy {
  private langChangeSubscription: Subscription = new Subscription;
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  partnerService = inject(PartnerService);
  feedbackDialogService = inject(FeedbackDialogService);

  newPartnerData = signal<Partner|null>(null);

  columns: ListViewColumn[] = [
    {
      field: 'id',
      label: 'label.partner.id',
      sortable: false,
      type: 'text'
    },
    {
      field: 'name',
      label: 'label.partner.name',
      sortable: true,
      type: 'text'
    },
    {
      field: 'status',
      label: 'label.partner.status',
      sortable: false,
      type: 'text'
    },
    {
      field: 'newEngagement',
      label: 'label.partner.newEngagement',
      sortable: false,
      type: 'text'
    }
  ];

  constructor(private languageService: LanguageService, private cdr: ChangeDetectorRef) {
    this.setNewPartnerFromAIAssistant();
  }

  private setNewPartnerFromAIAssistant() {
    this.activatedRoute.queryParams.subscribe(params => {
      if (params['openNewDialog'] === 'true') {
        const state = history.state;
        if (state?.data) {
          this.newPartnerData.set(state.data);
        }
        this.removeOpenNewDialogFromUrl();
      }
    });
  }

  private removeOpenNewDialogFromUrl() {
    this.router.navigate([], {
      relativeTo: this.activatedRoute,
      queryParams: {openNewDialog: null},
      queryParamsHandling: 'merge'
    });
  }

  handleOnOpenRecordDetails(record: any) {
    this.router.navigate(['partner', record.id]);
  }

  handleOnRecordDelete(record: any) {
    this.partnerService.deletePartnerById(record.id).subscribe({
      next: () => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Record deleted successfully!' });
        const listviewElement = document.querySelector('app-listview');
        if (listviewElement) {
          listviewElement.dispatchEvent(new CustomEvent('refresh-listview'));
        }
      }
    });
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  _handleOnRecordCreation(newRecordData: any) {
    if (newRecordData?.id) {
      this.router.navigate(['partner', newRecordData.id]);
    }
  }
}
