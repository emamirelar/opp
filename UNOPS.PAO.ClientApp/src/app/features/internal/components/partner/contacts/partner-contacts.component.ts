import {
  ChangeDetectionStrategy,
  Component,
  input,
  OnInit,
  signal,
  inject,
  computed
} from '@angular/core';

import { ButtonModule } from 'primeng/button';
import { ActivatedRoute, Router } from '@angular/router';

import { TranslateModule } from '@ngx-translate/core';
import { ListviewComponent } from '../../../../../common/pages/components/listview/listview.component';
import { ListViewColumn, ListViewConfig } from '../../../../../common/pages/components/listview/listview.model';
import { ContactService } from '../../../services/contact.service';
import { EntityConfigurationService } from '../../../services/entity-configuration.service';

@Component({
  selector: 'app-partner-contacts',
  templateUrl: './partner-contacts.component.html',
  imports: [ButtonModule, TranslateModule, ListviewComponent],
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerContactsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private contactService = inject(ContactService);
  private entityConfigurationService = inject(EntityConfigurationService);

  partnerId = input<string>();
  partnerName = input<string>();
  dataUrl = signal<string>('');

  // Dynamic columns loaded from API
  columns = signal<ListViewColumn[]>([]);
  columnsLoading = signal(true);

  // Fallback columns definition
  private fallbackColumns: ListViewColumn[] = [
    {
      field: 'profilePictureUrl',
      label: 'Avatar',
      type: 'avatar',
      sortable: false
    },
    {
      field: 'firstName',
      label: 'contacts.firstName',
      type: 'text',
      sortable: true
    },
    {
      field: 'lastName',
      label: 'contacts.lastName',
      type: 'text',
      sortable: true
    },
    {
      field: 'title',
      label: 'contacts.title',
      type: 'text',
      sortable: true
    },
    {
      field: 'email',
      label: 'contacts.email',
      type: 'email',
      sortable: true
    },
    {
      field: 'phone',
      label: 'contacts.phone',
      type: 'text',
      sortable: false
    }
  ];

  config: ListViewConfig = {
    pageSize: 20,
    pageSizeOptions: [20, 50, 100],
    enablePagination: false, // Using infinite scroll
    enableSorting: true,
    enableExport: true,
    scrollable: true,
    scrollHeight: 'calc(100vh - 20rem)',
    autoSwitchToCardView: false,
    autoSwitchMinWidth: 768,
    defaultViewMode: 'card',
    entityName: 'Contact',
     
  };

  ngOnInit(): void {
    // If partnerId is provided as input, use it directly
    if (this.partnerId()) {
      this.dataUrl.set(`/api/contact?partnerId=${this.partnerId()}`);
    } else {
      // Otherwise, get partnerId from parent route params (when used as a child route)
      this.route.parent?.paramMap.subscribe(params => {
        const recordId = params.get('recordId');
        if (recordId) {
          this.dataUrl.set(`/api/contact?partnerId=${recordId}`);
        }
      });
    }

    // Load dynamic columns from API
    this.loadContactColumns();
  }

  private loadContactColumns() {
    this.columnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('Contact')
      .subscribe({
        next: (columns) => {
          // Filter out redundant partner-related columns since we're in partner context
          const filteredColumns = columns.filter(col => 
            !['partner.name', 'partnerName', 'partnerId', 'partner.id'].includes(col.field)
          );
          
          // Process columns and handle nested fields
          const processedColumns = filteredColumns.map(col => this.processColumn(col));
          this.columns.set(processedColumns);
          this.columnsLoading.set(false);
        },
        error: (error) => {
          console.error('Failed to load contact columns:', error);
          // Use fallback columns if API fails
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
    this.columns.set(this.fallbackColumns);
  }

  onContactClick(contact: any): void {
    if (contact?.id) {
      this.router.navigate(['/partnerships/contacts', contact.id]);
    }
  }

  onAddContact(): void {
    const currentPartnerId = this.partnerId() || this.getCurrentPartnerIdFromRoute();
    console.log('Add contact for partner:', currentPartnerId);
    // Open add contact dialog or navigate to create form
    // Implementation depends on your contact creation flow
  }

  private getCurrentPartnerIdFromRoute(): string {
    return this.route.parent?.snapshot.paramMap.get('recordId') || '';
  }

  handleOnOpenRecordDetails(record: any) {
    if (record == null) {
      return;
    }
    this.router.navigate(['/partnerships/contacts', record.id]);
  }
}
