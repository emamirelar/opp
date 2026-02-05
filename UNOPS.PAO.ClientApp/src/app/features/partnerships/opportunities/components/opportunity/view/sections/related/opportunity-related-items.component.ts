/**
 * @fileoverview Related Items component for displaying source interactions
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
  input,
  signal,
  inject,
  OnInit,
  ChangeDetectionStrategy,
  computed
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { OpportunityService } from '@features/partnerships/opportunities/services/opportunity.service';
import { ListviewCardComponent } from '@features/list-view/components/listview/card/listview-card.component';
import { ListViewColumn, ListViewConfig } from '@features/list-view/components/listview/listview.model';

/**
 * @class OpportunityRelatedItemsComponent
 * @description Component for displaying source interactions that led to opportunity creation
 * 
 * @example
 * ```html
 * <app-opportunity-related-items
 *   [opportunityId]="opportunity().id!"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-related-items',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    PanelModule,
    ListviewCardComponent
  ],
  templateUrl: './opportunity-related-items.component.html',
  styleUrls: ['./opportunity-related-items.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityRelatedItemsComponent implements OnInit {
  private readonly opportunityService = inject(OpportunityService);

  // Inputs
  readonly opportunityId = input.required<number>();

  // State
  readonly sourceInteractions = signal<any[]>([]);
  readonly isLoading = signal(false);
  readonly isCollapsed = signal(false);

  // List view configuration
  readonly interactionColumns = signal<ListViewColumn[]>([
    {
      field: 'subject',
      label: 'Subject',
      type: 'text',
      sortable: false,
      ellipsis: true
    },
    {
      field: 'interactionType',
      label: 'Type',
      type: 'badge',
      sortable: false
    },
    {
      field: 'interactionDate',
      label: 'Date',
      type: 'date',
      sortable: false
    }
  ]);

  readonly listViewConfig = computed<ListViewConfig>(() => ({
    pageSize: 20,
    pageSizeOptions: [10, 20, 50],
    selectable: false,
    multiSelect: false,
    showPaginator: false,
    forceMobileMode: false
  }));

  ngOnInit(): void {
    this.loadSourceInteractions();
  }

  /**
   * @description Load source interactions for the opportunity
   */
  loadSourceInteractions(): void {
    const id = this.opportunityId();
    if (!id) return;

    this.isLoading.set(true);
    this.opportunityService.getSourceInteractions(id).subscribe({
      next: (data) => {
        this.sourceInteractions.set(data);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading source interactions:', error);
        this.isLoading.set(false);
        this.sourceInteractions.set([]);
      }
    });
  }

  /**
   * @description Navigate to interaction detail page
   */
  navigateToInteraction(id: number): void {
    window.open(`/partnerships/interactions/${id}`, '_blank');
  }
}
