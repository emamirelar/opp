import { ChangeDetectionStrategy, ChangeDetectorRef, Component, computed, effect, inject, OnDestroy, OnInit, output, signal } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { DocumentService } from '../../../services/document.service';
import { DocumentComponent } from '../../../../../common/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '../../../overrides/reusables/components/document/gdrive/document-gdrive.component';
import { PictureComponent } from "../../../../../common/reusables/components/picture/picture.component";


import { TranslateModule } from '@ngx-translate/core';


//PrimeNG imports
import { InputTextModule } from 'primeng/inputtext';
import { DividerModule } from 'primeng/divider';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { AutoFocusModule } from 'primeng/autofocus';
import { DialogModule } from 'primeng/dialog';
import { MessageModule } from 'primeng/message';
import { CardModule } from 'primeng/card';
import { CheckboxModule } from 'primeng/checkbox';
import { ActivatedRoute, Router } from '@angular/router';
import { MarkdownPipe } from '../../../pipes/markdown.pipe';
import { LinkListComponent } from "../../../../../common/reusables/components/link/list/link-list.component";
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerViewContactsComponent } from './contacts/partner-view-contacts.component';
import { PartnerTreeViewNavigationComponent } from './navigation/partner-tree-view-navigation.component';
import {PartnerContactsComponent} from "../../partner/contacts/partner-contacts.component";
import { PartnerTree } from '../../../models/partner-tree.model';
import { PartnerCategoryGroup, PartnerGroup } from '../../../models/partner-category-group.model';    
import { JsonPipe } from '@angular/common';
import { PartnerTreeService } from '../../../services/partner-tree.service';
import { ListViewColumn } from '../../../../../common/pages/components/listview/listview.model';
import { ListviewComponent } from '../../../../../common/pages/components/listview/listview.component';
import { PartnerTreeItemComponent } from '../item/partner-tree-item.component';
import { PartnerTreeItemFooterComponent } from '../item/partner-tree-item-footer.component';
@Component({
  selector: 'app-partner-tree-view',
  imports: [
    TranslateModule,
    InputTextModule,
    DropdownModule,
    DatePickerModule,
    DocumentComponent,
    GDriveDocumentComponent,
    ButtonModule,
    TextareaModule,
    PanelModule,
    SelectModule,
    AutoFocusModule,
    DialogModule,
    MessageModule,
    DividerModule,
    CardModule,
    CheckboxModule,
    ReactiveFormsModule,
    MarkdownPipe,
    LinkListComponent,
    PictureComponent,
    PartnerTreeViewNavigationComponent,
    PartnerContactsComponent,
    PartnerViewContactsComponent,
    ListviewComponent,
    RouterModule,
    JsonPipe
  ],
  templateUrl: './partner-tree-view.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DialogService],
})
export class PartnerTreeViewComponent implements OnInit {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  documentService = inject(DocumentService);
  dialogService = inject(DialogService);

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  partnerTreeService = inject(PartnerTreeService);

  partnerTreeId = signal<number>(0);
  partnerTree = signal<PartnerTree | null>(null);
  partnerTreeChildren = signal<PartnerTree[]>([]);

  childrenPartnerGroups = signal<PartnerGroup[]>([]);

  isPartnerCategory = computed(() => this.partnerTree()?.partnerCategoryCode !== null);

  ngOnInit() {
    this.activatedRoute.data.subscribe((data: {[key: string]: any}) => {
      if (data['partnerTreeData']) {
        this.partnerTree.set(data['partnerTreeData'].data);
        this.childrenPartnerGroups.set(this.cachedDataService.getParterGroupByCategoryCode(this.partnerTree()?.partnerCategoryCode));
      }
    });
  }

  partnerColumns: ListViewColumn[] = [
    {
      field: 'logoUrl',
      label: '',
      sortable: false,
      type: 'avatar',
      width: '50px'
    },
    {
      field: 'name',
      label: 'label.partner.name',
      sortable: false,
      type: 'text'
    }
  ];

  handleEditClick() {
    const ref = this.dialogService.open(PartnerTreeItemComponent, {
      header: 'Edit Partner Level',
      width: '50rem',
      closable: true,
      data: {
        record: this.partnerTree()
      }
    });

    ref.onClose.subscribe((result: PartnerTree) => {
      if (result) {
        // Reload the tree data after successful edit
        this.partnerTreeService.getPartnerTreeDataById(result.id!.toString()).subscribe({
          next: (data: any) => {
            this.partnerTree.set(data.data);
            this.childrenPartnerGroups.set(this.cachedDataService.getParterGroupByCategoryCode(this.partnerTree()?.partnerCategoryCode));
            this.feedbackDialogService.showSuccessToast({ detail: 'Partner tree updated successfully!' });
          }
        });
      }
    });
  }

  navigateToPartner($event: any) {
    this.router.navigate(['/partnerships/partners/' + $event.id]);
  }

  getPartnersUrl() : string {
    if (this.isPartnerCategory()) {
      return 'api/partner/by-partner-category-code/' + this.partnerTree()?.partnerCategoryCode;
    } else {
      return 'api/partner/by-partner-group-code/' + this.partnerTree()?.partnerGroupCode;
    }
  }

}
