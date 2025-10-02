import { ChangeDetectionStrategy, Component, OnInit, signal, computed, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Button } from 'primeng/button';
import { DialogService } from 'primeng/dynamicdialog';
import { InteractionModalComponent } from '../../../interaction/modal/interaction-modal.component';
import { Router, ActivatedRoute } from '@angular/router';
import {ListViewColumn, ListViewConfig, SearchParams} from '@common/pages/components/listview/listview.model';
import {ListviewComponent} from '@common/pages/components/listview/listview.component';
import {FeedbackDialogService} from '../../../../../../common/services/feedback-dialog.service';
import {PermissionUtilityService} from '@essentials/services/permission-utility.service';
import {SearchField} from '@common/services/search-parser.service';
import {EntityConfigurationService} from '@features/internal/services/entity-configuration.service';
import {InteractionIconService} from '@common/services/interaction-icon.service';

@Component({
  selector: 'app-contact-view-interactions',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    Button,
    ListviewComponent
  ],
  providers: [DialogService],
  template: `
    <div class="flex flex-col gap-8 w-full">
      @if(!permissionsLoading() && permissionUtilityService.canCreate(entityPermissions())) {
        <div class="flex items-center gap-4 flex-wrap">
            <p-button class="ml-auto"
                      [label]="'title.newInteraction' | translate"
                      icon="pi pi-plus"
                      rounded
                      (click)="openNewInteractionModal()"></p-button>
        </div>
      }

      @if(interactionsApiUrl()) {
        <app-listview
          [dataUrl]="interactionsApiUrl()!"
          [columns]="columns()"
          [entityType]="'Interaction'"
          [config]="listviewConfig()"
          (rowClick)="navigateToInteractionDetail($event)"
          (searchChange)="onSearchChange($event)"
        >
        </app-listview>
      } @else {
        <div class="flex items-center justify-center p-8">
          <span class="text-gray-500">No contact selected</span>
        </div>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ContactViewInteractionsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private dialogService = inject(DialogService);
  private entityConfigurationService = inject(EntityConfigurationService);
  private feedbackDialogService = inject(FeedbackDialogService);
  public permissionUtilityService = inject(PermissionUtilityService);
  private interactionIconService = inject(InteractionIconService);
  private translateService = inject(TranslateService);

  // Permission handling for interactions
  private permissionUtils = this.permissionUtilityService.createEntityPermissions('Interaction');
  entityPermissions = this.permissionUtils.entityPermissions;
  permissionsLoading = this.permissionUtils.permissionsLoading;

  // Dynamic interaction columns loaded from API
  columns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(true);

  // Signal for contactId from query params
  contactIdSignal = signal<string>('');

  // Computed API URL with contact filter
  interactionsApiUrl = computed<string | null>(() => {
    const id = this.contactIdSignal();
    if (!id) {
      return null; // Return null when no contactId to prevent API calls
    }
    return `/api/interactions?contactId=${id}`;
  });

  constructor() {
    // Effect to watch for route changes and extract contactId from parent route params
    effect(() => {
      // Get the recordId from parent route parameters
      this.route.parent?.params.subscribe(params => {
        const contactId = params['recordId'];
        if (contactId) {
          this.contactIdSignal.set(contactId);
        }
      });
    });
  }

  // Configure listview behavior with computed permissions
  listviewConfig = computed<ListViewConfig>(() => ({
    enableSelection: true,
    enablePagination: true,
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enableSorting: true,
    enableSearch: true,
    enableExport: this.entityPermissions().permissions.canCreate || this.entityPermissions().permissions.canUpdate,
    entityName: 'Interaction',
    scrollable: true,
    scrollHeight: 'flex',
    defaultSortField: 'subject',
    defaultSortOrder: 'asc',
    sortableFields: [
      { field: 'subject', label: 'Subject' },
      { field: 'createdDate', label: 'Created Date' },
      { field: 'lastModifiedDate', label: 'Last Updated Date' }
    ],
    searchConfig: {
      useAdvancedSearch: true,
      placeholder: this.translateService.instant('search.interactionsPlaceholder'),
      entityType: 'Interaction' as const,
      searchableFields: [
        {
          field: 'type',
          label: 'Type',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        {
          field: 'subject',
          label: 'Subject',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        {
          field: 'description',
          label: 'Description',
          type: 'string',
          operators: ['is', 'is not', 'like', 'not like']
        },
        {
          field: 'date',
          label: 'Date',
          type: 'date',
          operators: ['is', 'is not', 'after', 'before', 'between', '>', '<', '>=', '<=']
        }
      ] as SearchField[]
    }
  }));

  ngOnInit() {
    // Load permissions
    this.permissionUtils.loadPermissions(this.router);

    // Load dynamic columns from API
    this.loadInteractionColumns();
  }

  private loadInteractionColumns() {
    this.columnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('Interaction')
      .subscribe({
        next: (columns: any) => {
          // Filter out redundant contact-related columns since we're already in contact context
          const filteredColumns = columns.filter((col: any) =>
            !['contact.name', 'contactName', 'contact.firstName', 'contact.lastName',
              'contactId', 'contact.id', 'contact.fullName'].includes(col.field)
          );

          // Convert backend columns to frontend format and add template functions
          const processedColumns = filteredColumns.map((col: any) => this.processColumn(col));
          this.columns.set(processedColumns);
          this.columnsLoading.set(false);
        },
        error: (error: any) => {
          console.error('Failed to load interaction columns:', error);
          // Fallback to default columns if API fails
          this.setFallbackColumns();
          this.columnsLoading.set(false);
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

    // Detect interaction type columns and convert them to interactionIcon type
    if (column.field === 'type' && column.type === 'text') {
      processedColumn.type = 'interactionIcon';
    }

    // Handle nested field paths (fields with dots) by adding a template function
    if (column.field && column.field.includes('.') && column.type !== 'template' && column.type !== 'interactionIcon') {
      // Keep the original field for identification but add a template function to access nested data
      processedColumn.templateFn = (rowData: any) => {
        const value = this.getNestedProperty(rowData, column.field);
        return value !== undefined && value !== null ? String(value) : '';
      };
      // Change type to template since we're now using a template function
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
          console.warn(`Template expression error: ${expression}`, error);
          return '';
        }
      });
    };
  }

  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((current, prop) => current?.[prop], obj);
  }

  private setFallbackColumns() {
    const fallbackColumns: ListViewColumn[] = [
      {
        field: 'type',
        label: 'label.interaction.type',
        sortable: true,
        type: 'interactionIcon'
      },
      {
        field: 'date',
        label: 'label.interaction.date',
        sortable: true,
        type: 'date'
      },
      {
        field: 'subject',
        label: 'label.interaction.subject',
        sortable: false,
        type: 'text'
      },
      {
        field: 'description',
        label: 'label.interaction.description',
        sortable: false,
        type: 'text'
      }
    ];
    this.columns.set(fallbackColumns);
  }

  openNewInteractionModal(): void {
    // Check if user has create permission
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to create interactions',
        summary: 'Permission Denied'
      });
      return;
    }

    const ref = this.dialogService.open(InteractionModalComponent, {
      header: 'New Interaction',
      width: '90%',
      height: '90%',
      modal: true,
      closable: true,
      data: {
        initialData: {
          contactId: this.contactIdSignal(), // Pre-fill contact ID
          contactIds: [parseInt(this.contactIdSignal())] // Pre-fill contact IDs array for "Related To" field
        }
      }
    });

    ref.onClose.subscribe((result) => {
      if (result) {
        // Refresh the listview
        window.dispatchEvent(new CustomEvent('refresh-listview'));
      }
    });
  }

  navigateToInteractionDetail(item: any): void {
    // Check if user has read permission
    if (!this.permissionUtilityService.canRead(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({
        detail: 'You do not have permission to view interactions',
        summary: 'Permission Denied'
      });
      return;
    }

    // Navigate to the interaction detail page
    if (item && item.id) {
      this.router.navigate(['/partnerships/interactions', item.id]);
    } else {
      this.feedbackDialogService.showErrorToast({
        detail: 'Invalid interaction data',
        summary: 'Navigation Error'
      });
    }
  }

  onSearchChange(searchParams: SearchParams) {
  }
}

