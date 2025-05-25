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
import { PermissionUtilityService } from '../../../../essentials/services/permission-utility.service';
import { EntityPermissions } from '../../../../essentials/services/permission.service';

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
  permissionUtilityService = inject(PermissionUtilityService);

  newPartnerData = signal<Partner|null>(null);

  // Permission management using utility service
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Partner');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  // Listview configuration
  listviewConfig: ListViewConfig = {
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
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
      field: 'logoUrl',
      label: '',
      sortable: false,
      type: 'avatar'
    },
    {
      field: 'name',
      label: 'label.partner.name',
      sortable: false,
      type: 'text'
    },
    {
      field: 'status',
      label: 'label.partner.status',
      sortable: false,
      type: 'conditionalIcon',
      conditionFn: (rowData: any) => rowData.status === 'Active'
    },
    {
      field: 'newEngagement',
      label: 'label.partner.newEngagement',
      sortable: false,
      type: 'conditionalIcon',
      conditionFn: (rowData: any) => rowData.newEngagement === 'Allowed'
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
    
    // Load permissions using utility service
    this.permissionUtils.loadPermissions(this.router, this.cdr);
    
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
    if (record && record.id !== undefined && record.id !== null) {
      console.log('Navigating to partner:', record.id);
      this.router.navigate(['partnerships/partners', record.id.toString()]);
    } else {
      console.error('Cannot navigate: record or record.id is undefined', record);
    }
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
    if (newRecordData && newRecordData.id !== undefined && newRecordData.id !== null) {
      console.log('Navigating to newly created partner:', newRecordData.id);
      this.router.navigate(['partnerships/partners', newRecordData.id.toString()]);
    } else {
      console.error('Cannot navigate to created record: id is undefined', newRecordData);
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
