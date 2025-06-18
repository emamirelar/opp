import { ChangeDetectionStrategy, Component, computed, inject, OnInit, signal } from '@angular/core';
import { CachedDataService } from '../../../../../../common/services/cached-data.service';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { combineLatest, Observable, of, delay } from 'rxjs';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';

import { FeedbackDialogService } from '../../../../../../common/pages/services/feedback-dialog.service';
import { DocumentService } from '../../../../services/document.service';
import { DocumentComponent } from '../../../../../../common/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '../../../../overrides/reusables/components/document/gdrive/document-gdrive.component';
import { AiPanelComponent, AiDataService } from '../../../../../../common/reusables/components/ai-panel/ai-panel.component';

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
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { LinkListComponent } from "../../../../../../common/reusables/components/link/list/link-list.component";
import { EntityType } from "../../../../../../common/models/link.model";
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerTree } from '../../../../models/partner-tree.model';
import { PartnerTreeItemComponent } from '../../item/partner-tree-item.component';
import { PermissionUtilityService } from '../../../../../../essentials/services/permission-utility.service';
import { ListViewColumn } from '../../../../../../common/pages/components/listview/listview.model';
import { ListviewComponent } from '../../../../../../common/pages/components/listview/listview.component';
import { GoBackComponent } from '../../../../../../common/reusables/components/go-back/go-back.component';

// Mock AI service for partner tree AI panels
class MockPartnerTreeAiService implements AiDataService {
  get(entityId: string, promptType: string): Observable<string> {
    const mockResponses: { [key: string]: string } = {
      'summary-interactions': `

**This is mocked data.**

Recent activities with partners in this category show strong engagement across multiple projects. Key highlights include:

- **Collaborative Projects:** 15 active partnerships in development phase
- **Communication Frequency:** Average 2.3 touchpoints per week
- **Success Rate:** 87% of initiated partnerships reaching implementation
- **Geographic Distribution:** Partners across 23 countries

`,

      'category-news': `

**This is mocked data.**


Latest developments in this partner category:

- **Industry Trends:** Increased focus on sustainable development initiatives
- **New Opportunities:** 8 upcoming tender opportunities identified
- **Market Analysis:** Growing demand for technical expertise partnerships
- **Regulatory Updates:** New compliance requirements effective Q2 2024

`,

      'partner-news': `

**This is mocked data.**


Recent news and updates from partners in this network:

- **Awards & Recognition:** 3 partners received international sustainability awards
- **Expansion Updates:** 2 partners opened new regional offices
- **Innovation Focus:** Increased investment in digital transformation initiatives
- **Partnership Opportunities:** 12 new collaboration proposals under review

`,

      'category-summary': `

**This is mocked data.**


This partner category demonstrates strong performance metrics:

- **Total Partners:** 45 active organizations
- **Engagement Score:** 8.2/10 average rating
- **Project Success Rate:** 91% completion rate
- **Innovation Index:** High adoption of emerging technologies
- **Growth Potential:** Projected 25% expansion in next fiscal year

`,

      'group-summary': `

**This is mocked data.**


This partner group shows excellent collaboration outcomes:

- **Active Members:** 12 organizations within this group
- **Collaboration Frequency:** Weekly coordination meetings
- **Joint Projects:** 6 active multi-partner initiatives
- **Resource Sharing:** 78% efficiency in shared resource utilization
- **Knowledge Exchange:** Regular best practices sharing sessions
- **Success Stories:** 4 major breakthrough projects this quarter

`
    };

    const response = mockResponses[promptType] || `## AI Analysis

Mock response for entity ${entityId} with prompt type ${promptType}.

This is a placeholder response while the AI service is being implemented.

*Generated by Mock AI Service*`;

    // Simulate network delay
    return of(response).pipe(delay(1500));
  }
}

@Component({
  selector: 'app-partner-tree-details',
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
    LinkListComponent,
    RouterModule,
    ProgressSpinnerModule,
    AiPanelComponent,
    ListviewComponent,
    GoBackComponent
  ],
  templateUrl: './partner-tree-details.component.html',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [DialogService],
})
export class PartnerTreeDetailsComponent implements OnInit {
  router = inject(Router);
  activatedRoute = inject(ActivatedRoute);
  documentService = inject(DocumentService);
  dialogService = inject(DialogService);

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);

  // RBAC permissions
  permissionUtilityService = inject(PermissionUtilityService);
  recordPermissionsData = this.permissionUtilityService.createInstancePermissions('PartnerTree');
  recordPermissions = this.recordPermissionsData.recordPermissions;

  partnerTreeId = signal<number>(0);
  partnerTree = signal<PartnerTree | null>(null);
  partnerTreeChildren = signal<PartnerTree[]>([]);

  // Computed children partner groups that updates automatically when partnerTree changes
  childrenPartnerGroups = computed(() => {
    const tree = this.partnerTree();
    if (!tree?.partnerCategoryCode) return [];
    return this.cachedDataService.getParterGroupByCategoryCode(tree.partnerCategoryCode);
  });

  // Loading state
  isLoading = signal<boolean>(false);

  // Mock AI service instance
  mockAiService = new MockPartnerTreeAiService();

  isPartnerCategory = computed(() => this.partnerTree()?.partnerCategoryCode !== null);

  // Computed entity ID for AI panels
  entityId = computed(() => {
    return this.partnerTree()?.id?.toString() || '';
  });

  // Properties for app-link-list component
  entityTypePartner = EntityType.PartnerTree; // Entity type for link list component

  // Properties for app-document component
  recordId = computed(() => {
    return this.partnerTree()?.id?.toString() || '';
  });

  // Accepted MIME types for Google Drive documents
  acceptedMiMIETypesForgDrive = 'application/pdf,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.google-apps.document,application/vnd.google-apps.spreadsheet,application/vnd.google-apps.presentation';

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
      field: 'name',
      label: 'label.name',
      sortable: false,
      type: 'text'
    }
  ];

  navigateToPartner($event: any) {
    if ($event?.id) {
      this.router.navigate(['/partnerships/partners/' + $event.id]);
    }
  }

  handleEditClick() {
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
        record: this.partnerTree()
      }
    });

    ref.onClose.subscribe((result: PartnerTree) => {
      if (result) {
        this.isLoading.set(true);
        // Reload the tree data after successful edit
        this.cachedDataService.partnerTreeService.getPartnerTreeDataById(result.id!.toString()).subscribe({
          next: (data: any) => {
            this.partnerTree.set(data.data);
            this.feedbackDialogService.showSuccessToast({ detail: 'Partner tree updated successfully!' });
            this.isLoading.set(false);
          },
          error: (error) => {
            this.feedbackDialogService.showErrorToast({ detail: 'Failed to update partner tree' });
            this.isLoading.set(false);
          }
        });
      }
    });
  }

}

