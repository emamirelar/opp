import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
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
import { ListViewColumn, ListViewConfig, SearchParams } from '../../../../common/pages/components/listview/listview.model';
import { PartnerEditDialogFooterComponent } from './edit-dialog/footer/partner-edit-dialog-footer.component';
import { PartnerEditDialogComponent } from './edit-dialog/partner-edit-dialog.component';
import { DialogService } from 'primeng/dynamicdialog';
import { ImportDialogService } from '../../../../common/reusables/components/import/dialog/import-dialog.service';
import { SearchField } from '../../../../common/services/search-parser.service';

@Component({
  selector: 'app-partner',
  templateUrl: './partner.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    ButtonModule,
    PartnerNewComponent,
    TranslateModule,
    ListviewComponent,
  ],
  providers: [DialogService]
})
export class PartnerComponent implements OnDestroy, OnInit {
  private langChangeSubscription: Subscription = new Subscription;
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  partnerService = inject(PartnerService);
  feedbackDialogService = inject(FeedbackDialogService);
  dialogService = inject(DialogService);
  importDialogService = inject(ImportDialogService);

  newPartnerData = signal<Partner|null>(null);

  // Listview configuration
  listviewConfig: ListViewConfig = {
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    selectionMode: 'single',
    enablePagination: true,
    enableSorting: true,
    enableSearch: true,
    enableExport: true,
    scrollable: true,
    scrollHeight: 'flex',
    entityName: 'Partner',
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: 'Search partners...',
      searchableFields: [
        { 
          field: 'name', 
          label: 'Name', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'shortName', 
          label: 'Short Name', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'status', 
          label: 'Status', 
          type: 'string',
          operators: ['is', 'is not']
        },
        { 
          field: 'website', 
          label: 'Website', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'street', 
          label: 'Street', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'city', 
          label: 'City', 
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        { 
          field: 'country', 
          label: 'Country', 
          type: 'string',
          operators: ['is', 'is not']
        }
      ] as SearchField[]
    }
  };

  columns: ListViewColumn[] = [
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
    console.log('Partner component constructor');
    console.log('Initial listview config:', this.listviewConfig);
    this.setNewPartnerFromAIAssistant();
  }

  ngOnInit() {
    console.log('Partner component ngOnInit');
    console.log('Searchable fields:', this.listviewConfig.searchConfig?.searchableFields);
    
    this.activatedRoute.queryParams
      .subscribe(params => {
        if (params['openNewDialog'] === 'true') {
          const state = history.state;
          const emptyPartner: Partner = {};
          this.openPartnerEditDialog(state?.data || emptyPartner);
        }
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

  openPartnerEditDialog(partnerData: Partner = {}) {
    const ref = this.dialogService.open(PartnerEditDialogComponent, {
      header: partnerData.id ? 'Edit Partner' : 'New Partner',
      width: '40vw',
      breakpoints: { '960px': '95vw' },
      closable: true,
      templates: {
        footer: PartnerEditDialogFooterComponent
      },
      data: {
        mode: partnerData.id ? 'edit' : 'new',
        record: partnerData,
        requestingSaveSignal: signal<boolean>(false)
      }
    });

    const refSub = ref.onClose.subscribe((result: any) => {
      if (result) {
        this._handleOnRecordCreation(result);
      }
      refSub.unsubscribe();
    });
  }

  openImportDialog() {
    // Use the Google Sheet picker directly which will show loading indicators
    this.importDialogService.openGoogleSheetPicker('partner');
  }

  onSearchChange(searchParams: SearchParams) {
    // Handle search parameters if needed
  }
}
