import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';

import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ScrollPanelModule } from 'primeng/scrollpanel';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { LanguageService } from '../../../../common/services/language.service';
import { Subscription } from 'rxjs';
import { PartnerService } from '../../services/partner.service';
import { DialogModule } from 'primeng/dialog';
import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';
import {PartnerNewComponent} from './new/partner-new.component';
import {Partner} from '../../models/partner.model';

interface columnDefinition {
  label: string,
  id: string
}

@Component({
  selector: 'app-partner',
  templateUrl: './partner.component.html',
  styleUrl: './partner.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [PanelModule, ButtonModule, TableModule, DialogModule, ScrollPanelModule, PartnerNewComponent, ProgressSpinnerModule, TranslateModule]
})
export class PartnerComponent implements OnInit, OnDestroy {
  private langChangeSubscription: Subscription = new Subscription;
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);

  partnerService = inject(PartnerService);

  newPartnerData = signal<Partner|null>(null);

  columns = signal<columnDefinition[]>([]);

  partnerData = this.partnerService.allPartners;
  isDataLoading = this.partnerService.isLoading;
  feedbackDialogService = inject(FeedbackDialogService);

  constructor(private languageService: LanguageService, private cdr: ChangeDetectorRef) { }

  ngOnInit() {
    this.setNewPartnerFromAIAssistant();
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        //initialize columns
        this.columns.update(() => {
          return this.getColumns();
        });
        //make server call to get all proposals.
        this.partnerService.getAllPartners();
      }
    });
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
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

  getColumns() {
    return [{
      label: 'label.partner.id',
      id: 'id'
    }, {
      label: 'label.partner.name',
      id: 'name'
    }, {
      label: 'label.partner.status',
      id: 'status'
    }, {
      label: 'label.partner.phone',
      id: 'phone'
    }, {
      label: 'label.partner.website',
      id: 'website'
    }, {
      label: 'label.partner.newEngagement',
      id: 'website'
    },{
      label: 'label.partner.actions',
      id: ''
    }];
  }

  handleOnOpenRecordDetails(record: any) {
    this.router.navigate(['partner', record.id]);
  }

  handleOnRecordDelete(record: any) {
    console.log(record.id);
    this.partnerService.deletePartnerById(record.id).subscribe({
      next: (data: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Record deleted successfully!' });
        this.partnerService.getAllPartners();
      }
    });
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  _handleOnRecordCreation( newRecordData: any ){
    if( newRecordData !== null && ( newRecordData["id"] !== undefined && newRecordData["id"] !== null ) )
    {
      this.router.navigate([ 'partner', newRecordData["id"] ]);
    }
  }
}
