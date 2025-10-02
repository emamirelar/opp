import { ChangeDetectionStrategy, Component, computed, effect, inject, OnInit, signal, viewChild } from '@angular/core';
import { CachedDataService } from '../../../../../../common/services/cached-data.service';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { combineLatest, Observable, of, delay } from 'rxjs';
import { EntityConfigurationService } from '../../../../services/entity-configuration.service';

import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown";
import { DatePickerModule } from 'primeng/datepicker';

import { FeedbackDialogService } from '../../../../../../common/services/feedback-dialog.service';
import { DocumentService } from '../../../../services/document.service';
import { DocumentComponent } from '../../../../../../common/reusables/components/document/document.component';
import { GDriveDocumentComponent } from '../../../../overrides/reusables/components/document/gdrive/document-gdrive.component';
import { AiPanelComponent, AiDataService } from '../../../../../../common/reusables/components/ai-panel/ai-panel.component';
import { GeminiService } from '../../../../services/gemini.service';

import { TranslateModule, TranslateService } from '@ngx-translate/core';

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
/**
 * @uiEntity PartnerTreeDetails
 * @route /admin/partner-tree/:recordId
 * @description Partner tree node detail view for administrative management of organizational hierarchies. Allows viewing and editing partner tree structure, relationships, and hierarchical data.
 * @capabilities view_partner_tree_node, edit_tree_structure, manage_hierarchies, configure_relationships, update_organizational_data
 * @synonyms organizational_structure, partner_hierarchy, tree_management, organizational_chart, hierarchy_admin
 * @mandatoryFields recordId
 * @help_when_stuck This shows detailed information about a specific node in the partner organizational tree. Use the form fields to edit organizational details, relationships, and hierarchical positioning. Changes affect how the partner appears in organizational charts and hierarchical views.
 * @common_tasks
 *   - Viewing tree node details: Review the organizational information and relationships
 *   - Editing organizational data: Modify fields related to hierarchy and structure
 *   - Managing relationships: Update parent-child relationships in the organization tree
 *   - Configuring hierarchy: Set up proper organizational positioning and reporting lines
 *   - Updating structure: Make changes to how the organization is represented in the system
 */

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
  entityConfigurationService = inject(EntityConfigurationService);

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  geminiService = inject(GeminiService);
  translateService = inject(TranslateService);

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

  // ViewChild references for AI panels to trigger refresh when data changes
  interactionsSummaryPanel = viewChild<AiPanelComponent>('interactionsSummaryPanel');
  newsPanel = viewChild<AiPanelComponent>('newsPanel');

  // Accepted MIME types for Google Drive documents
  acceptedMiMIETypesForgDrive = 'application/pdf,application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document,application/vnd.google-apps.document,application/vnd.google-apps.spreadsheet,application/vnd.google-apps.presentation';

  constructor() {
    // Effect to reload AI panels when partner tree changes
    effect(() => {
      const tree = this.partnerTree();
      const entityId = this.entityId();
      
      // Only trigger reload if we have valid data and the AI panels are available
      if (tree && entityId && this.interactionsSummaryPanel() && this.newsPanel()) {
        // Small delay to ensure the panels are fully initialized
        setTimeout(() => {
          this.interactionsSummaryPanel()?.loadData();
          this.newsPanel()?.loadData();
        }, 0);
      }
    });
  }

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
        console.error(this.translateService.instant('error.loading_partner_tree_data'), error);
        this.feedbackDialogService.showErrorToast({
          detail: this.translateService.instant('error.failed_to_load_partner_tree_data')
        });
        this.isLoading.set(false);
      }
    });

    // Load dynamic columns for partners list
    this.loadPartnerColumns();
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

  // Dynamic columns for partners list
  partnerColumns = signal<ListViewColumn[]>([]);
  partnerColumnsLoading = signal(true);

  // Fallback columns
  private fallbackPartnerColumns: ListViewColumn[] = [
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
        detail: this.translateService.instant('error.no_permission_edit_partner_tree')
      });
      return;
    }

    const ref = this.dialogService.open(PartnerTreeItemComponent, {
      header: this.translateService.instant('dialog.edit_partner_level'),
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
            this.feedbackDialogService.showSuccessToast({ 
              detail: this.translateService.instant('success.partner_tree_updated_successfully')
            });
            this.isLoading.set(false);
          },
          error: (error) => {
            this.feedbackDialogService.showErrorToast({ 
              detail: this.translateService.instant('error.failed_to_update_partner_tree')
            });
            this.isLoading.set(false);
          }
        });
      }
    });
  }

  private loadPartnerColumns() {
    this.partnerColumnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('Partner')
      .subscribe({
        next: (columns) => {
          // In partner tree context, we might want to filter out certain columns
          // Since this is showing partners within a tree structure, we keep most columns
          // but could filter based on specific needs
          const filteredColumns = columns.filter(col =>
            // Filter any columns that might be redundant in this context
            !['partnerTree.name', 'partnerTreeName'].includes(col.field)
          );

          // Process columns and handle nested fields
          const processedColumns = filteredColumns.map(col => this.processColumn(col));
          this.partnerColumns.set(processedColumns);
          this.partnerColumnsLoading.set(false);
        },
        error: (error) => {
          console.error(this.translateService.instant('error.failed_to_load_partner_columns'), error);
          // Use fallback columns if API fails
          this.setFallbackPartnerColumns();
          this.partnerColumnsLoading.set(false);
        }
      });
  }

  private processColumn(column: any): ListViewColumn {
    const processedColumn: ListViewColumn = {
      field: column.field,
      label: column.label,
      type: column.type,
      sortable: column.sortable,
      width: column.width,
      ellipsis: column.ellipsis,
      helperText: column.helperText
    };

    // Handle nested field paths (fields with dots) by adding a template function
    if (column.field && column.field.includes('.') && column.type !== 'template') {
      processedColumn.templateFn = (rowData: any) => {
        const value = this.getNestedProperty(rowData, column.field);
        return value !== undefined && value !== null ? String(value) : '';
      };
      // Change type to template since we're using a template function
      processedColumn.type = 'template';
    }

    // Add template function for template type columns
    const templatePattern = column.templatePattern || column.TemplatePattern;
    if (column.type === 'template' && templatePattern) {
      processedColumn.templateFn = this.createTemplateFunction(templatePattern);
    }

    return processedColumn;
  }

  private createTemplateFunction(templatePattern: string): (rowData: any) => string {
    return (rowData: any) => {
      return templatePattern.replace(/\{([^}]+)\}/g, (match, expression) => {
        try {
          const value = this.getNestedProperty(rowData, expression.trim());
          return value !== null && value !== undefined ? String(value) : '';
        } catch (error) {
          console.warn(`${this.translateService.instant('error.template_expression_error')} ${expression}`, error);
          return '';
        }
      });
    };
  }

  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((current, prop) => current?.[prop], obj);
  }

  private setFallbackPartnerColumns() {
    this.partnerColumns.set(this.fallbackPartnerColumns);
  }

}

