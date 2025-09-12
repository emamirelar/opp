import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { CachedDataService } from '../../../../../../common/services/cached-data.service';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { combineLatest, of } from 'rxjs';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';

import { FeedbackDialogService } from '../../../../../../common/pages/services/feedback-dialog.service';

import { TranslateModule } from '@ngx-translate/core';
import { LookerstudioComponent } from '../../../../../../common/reusables/components/lookerstudio/lookerstudio.component';

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
import { ProgressSpinnerModule } from 'primeng/progressspinner';

import { PartnerTree } from '../../../../models/partner-tree.model';
import { ListViewColumn } from '../../../../../../common/pages/components/listview/listview.model';
import { PermissionUtilityService } from '../../../../../../essentials/services/permission-utility.service';

@Component({
  selector: 'app-partner-tree-data',
  imports: [
    TranslateModule,
    InputTextModule,
    DropdownModule,
    DatePickerModule,
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
    RouterModule,
    ProgressSpinnerModule,
    LookerstudioComponent
  ],
  templateUrl: './partner-tree-data.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PartnerTreeDataComponent implements OnInit {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);

  // RBAC permissions
  permissionUtilityService = inject(PermissionUtilityService);
  recordPermissionsData = this.permissionUtilityService.createInstancePermissions('PartnerTree');
  recordPermissions = this.recordPermissionsData.recordPermissions;

  partnerTree = signal<PartnerTree | null>(null);

  // Computed children partner groups that updates automatically when partnerTree changes
  childrenPartnerGroups = computed(() => {
    const tree = this.partnerTree();
    if (!tree?.partnerCategoryCode) return [];
    return this.cachedDataService.getParterGroupByCategoryCode(tree.partnerCategoryCode);
  });

  // Loading state
  isLoading = signal<boolean>(false);

  // Lookerstudio properties
  dashboardId: string = 'dcf96b62-ae61-4d6c-8614-34b9faf91cd8';
  partnerCode = computed(() => this.partnerTree()?.code || '');
  minHeight: string = 'calc(100vh - 350px)'; // Customizable min-height for the dashboard

  // partnersUrl = computed(() => {
  //   if (!this.partnerTree()?.partnerGroupCode) {
  //     return 'api/partner/by-partner-category-code/' + this.partnerTree()?.partnerCategoryCode;
  //   } else {
  //     return 'api/partner/by-partner-group-code/' + this.partnerTree()?.partnerGroupCode;
  //   }
  // });

  ngOnInit() {
    // Combine both parent route data and parameter changes for reactive updates
    combineLatest([
      this.activatedRoute.parent?.data || of({}),
      this.activatedRoute.parent?.paramMap || of(null)
    ]).subscribe({
      next: ([data, params]) => {
        const recordId = params?.get('recordId');

        if (data && (data as any)['partnerTreeData']) {
          const newPartnerTree = (data as any)['partnerTreeData'].data;

          // Only update if it's actually a different record or if partnerTree is null
          if (!this.partnerTree() || newPartnerTree?.id?.toString() !== this.partnerTree()?.id?.toString()) {
            this.updatePartnerTreeData((data as any)['partnerTreeData']);
          }
        } else if (recordId && (!this.partnerTree() || recordId !== this.partnerTree()?.id?.toString())) {
          // Handle case where we have a recordId but no data yet (loading state)
          this.isLoading.set(true);
        }
      },
      error: (error) => {
        console.error('Error loading partner tree data:', error);
        this.feedbackDialogService.showErrorToast({
          detail: 'Failed to load partner tree data'
        });
        this.isLoading.set(false);
      }
    });
  }

  private updatePartnerTreeData(partnerTreeData: any): void {
    this.partnerTree.set(partnerTreeData.data);

    // Extract permissions from response if available
    if (partnerTreeData.permissions) {
      this.recordPermissions.set({
        entity: 'PartnerTree',
        hasAccess: true,
        permissions: partnerTreeData.permissions
      });
    } else if (this.partnerTree()?.id) {
      // Load permissions for the partner tree without ChangeDetectorRef
      this.recordPermissionsData.loadPermissions(this.partnerTree()!.id!.toString());
    }

    this.isLoading.set(false);
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

  navigateToPartner($event: any) {
    if ($event?.id) {
      this.router.navigate(['/partnerships/partners/' + $event.id]);
    }
  }
}
