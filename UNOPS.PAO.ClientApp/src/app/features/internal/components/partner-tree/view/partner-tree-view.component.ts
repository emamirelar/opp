import { ChangeDetectionStrategy, Component, OnInit, OnDestroy, ViewChild, inject } from '@angular/core';
import { Router, RouterModule, ActivatedRoute, NavigationEnd } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { Tab, TabList, Tabs } from 'primeng/tabs';
import { TooltipModule } from 'primeng/tooltip';
import { filter, Subscription } from 'rxjs';
import { PartnerTree } from '../../../models/partner-tree.model';
import { PartnerTreeViewNavigationComponent } from './navigation/partner-tree-view-navigation.component';
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerTreeItemComponent } from '../item/partner-tree-item.component';
import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { PermissionUtilityService } from '../../../../../essentials/services/permission-utility.service';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { GoBackComponent } from '../../../../../common/reusables/components/go-back/go-back.component';

interface TabItem {
  label: string;
  route: string;
}

@Component({
  selector: 'app-partner-tree-view',
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    Tabs,
    TabList,
    Tab,
    TooltipModule,
    PartnerTreeViewNavigationComponent,
    GoBackComponent
  ],
  providers: [DialogService],
  template: `
  <div class="flex flex-col gap-8">
    <!-- Back button -->
<!--    <app-go-back></app-go-back>-->

    <!-- Section Titre -->
    <div class="flex flex-col gap-4">
      <!-- Title and Name section -->
      <div class="flex items-center justify-between w-full">
        <div class="flex flex-col">
          <div class="text-lg font-medium text-gray-600">
            @if (recordData?.partnerGroupCode) {
              {{ 'label.partnerTree.partnerGroup' | translate }}
            } @else if (recordData?.partnerCategoryCode) {
              {{ 'label.partnerTree.partnerCategory' | translate }}
            } @else {
              {{ 'title.partnerTree' | translate }}
            }
          </div>
          <div class="text-3xl font-bold">
            {{ recordData?.name }}
          </div>
        </div>

        <!-- Edit button in top right -->
        <div class="flex items-start">
          @if (hasEditPermission) {
            <button
              type="button"
              class="p-2 rounded-full hover:bg-gray-100 transition-colors"
              (click)="handleEditClick()">
              <i class="pi pi-pencil text-gray-600 hover:text-gray-800"></i>
            </button>
          }
        </div>
      </div>

      <!-- Navigation breadcrumb -->
      <app-partner-tree-view-navigation></app-partner-tree-view-navigation>
    </div>

    <!-- Tabs -->
    <p-tabs [value]="activeRoute">
      <p-tablist>
        <p-tab *ngFor="let tab of tabs"
              [value]="tab.route"
              [routerLink]="tab.route"
              class="flex items-center !gap-2 text-inherit">
          <span>{{ tab.label }}</span>
        </p-tab>
      </p-tablist>
    </p-tabs>

    <!-- Router outlet for tab content -->
    <div>
      <router-outlet></router-outlet>
    </div>
  </div>
  `,
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: `
    :host ::ng-deep {
      --p-tabs-tablist-background: transparent;
    }
  `
})
export class PartnerTreeViewComponent implements OnInit, OnDestroy {
  recordId: string = '';
  activeRoute: string = '';

  tabs: TabItem[] = [];
  recordData: PartnerTree = {} as PartnerTree;
  private routerSubscription: Subscription | null = null;
  private paramSubscription: Subscription | null = null;

  // Inject services
  private dialogService = inject(DialogService);
  private feedbackDialogService = inject(FeedbackDialogService);
  private permissionUtilityService = inject(PermissionUtilityService);
  private cachedDataService = inject(CachedDataService);

  // RBAC permissions
  recordPermissionsData = this.permissionUtilityService.createInstancePermissions('PartnerTree');
  recordPermissions = this.recordPermissionsData.recordPermissions;

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Listen to parameter changes instead of using snapshot
    this.paramSubscription = this.activatedRoute.paramMap.subscribe(params => {
      const newRecordId = params.get('recordId') || '';
      if (newRecordId !== this.recordId) {
        this.recordId = newRecordId;
        this.updateTabs();
      }
    });

    // Get the resolved data from the route
    this.activatedRoute.data.subscribe(data => {
      this.recordData = data['partnerTreeData']?.data || {};

      // Extract permissions from response if available
      if (data['partnerTreeData']?.permissions) {
        this.recordPermissions.set({
          entity: 'PartnerTree',
          hasAccess: true,
          permissions: data['partnerTreeData'].permissions
        });
      } else if (this.recordData?.id) {
        // Load permissions for the partner tree
        this.recordPermissionsData.loadPermissions(this.recordData.id.toString());
      }
    });

    // Set initial active tab
    this.updateActiveTab();

    // Subscribe to router events to update active tab on navigation
    this.routerSubscription = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateActiveTab();
      });
  }

  ngOnDestroy(): void {
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
    if (this.paramSubscription) {
      this.paramSubscription.unsubscribe();
    }
  }

  private updateTabs(): void {
    // Create tabs based on recordId
    this.tabs = [
      {
        label: 'Details',
        route: `/admin/partner-tree/${this.recordId}`
      },
      {
        label: 'Dashboard',
        route: `/admin/partner-tree/${this.recordId}/data`
      }
    ];
  }

  private updateActiveTab(): void {
    const currentUrl = this.router.url;
    const activeTabIndex = currentUrl.includes('/data') ? 1 : 0;
    this.activeRoute = this.tabs[activeTabIndex]?.route || '';
  }

  get hasEditPermission(): boolean {
    return this.permissionUtilityService.canUpdate(this.recordPermissions());
  }

  handleEditClick(): void {
    // Check permission before opening modal
    if (!this.permissionUtilityService.canUpdate(this.recordPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to edit this partner tree'
      });
      return;
    }

    const ref = this.dialogService.open(PartnerTreeItemComponent, {
      header: 'Edit Partner Level',
      width: '50rem',
      closable: true,
      data: {
        record: this.recordData
      }
    });

    ref.onClose.subscribe((result: PartnerTree) => {
      if (result) {
        // Reload the tree data after successful edit
        this.cachedDataService.partnerTreeService.getPartnerTreeDataById(result.id!.toString()).subscribe({
          next: (data: any) => {
            this.recordData = data.data;
            this.feedbackDialogService.showSuccessToast({ detail: 'Partner tree updated successfully!' });

            // Reload the current route to refresh all child components
            this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
              this.router.navigate([this.router.url]);
            });
          },
          error: (error) => {
            this.feedbackDialogService.showErrorToast({ detail: 'Failed to update partner tree' });
          }
        });
      }
    });
  }

}
