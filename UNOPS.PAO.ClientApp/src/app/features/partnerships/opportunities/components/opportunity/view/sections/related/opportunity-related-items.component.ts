/**
 * @fileoverview Related Items component for displaying contacts, partners, and interactions
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
  input,
  signal,
  inject,
  OnInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { AvatarModule } from 'primeng/avatar';
import { DividerModule } from 'primeng/divider';
import { BadgeModule } from 'primeng/badge';
import { RelatedItems } from '@shared/models/opportunity.model';
import { OpportunityService } from '@features/partnerships/opportunities/services/opportunity.service';

/**
 * @class OpportunityRelatedItemsComponent
 * @description Component for displaying related contacts, partners, and interactions for an opportunity
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
    AvatarModule,
    DividerModule,
    BadgeModule
  ],
  templateUrl: './opportunity-related-items.component.html',
  styleUrls: ['./opportunity-related-items.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityRelatedItemsComponent implements OnInit {
  private readonly opportunityService = inject(OpportunityService);
  private readonly cdr = inject(ChangeDetectorRef);

  // Inputs
  readonly opportunityId = input.required<number>();

  // State
  readonly relatedItems = signal<RelatedItems>({
    contacts: [],
    partners: [],
    interactions: []
  });
  readonly isLoading = signal(false);
  readonly isCollapsed = signal(false);

  ngOnInit(): void {
    this.loadRelatedItems();
  }

  /**
   * @description Load related items for the opportunity
   */
  loadRelatedItems(): void {
    const id = this.opportunityId();
    if (!id) return;

    this.isLoading.set(true);
    this.opportunityService.getRelatedItems(id).subscribe({
      next: (data) => {
        this.relatedItems.set(data);
        this.isLoading.set(false);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading related items:', error);
        this.isLoading.set(false);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * @description Get avatar initials from name
   */
  getInitials(name: string): string {
    const parts = name.split(' ');
    if (parts.length >= 2) {
      return (parts[0].charAt(0) + parts[parts.length - 1].charAt(0)).toUpperCase();
    }
    return name.charAt(0).toUpperCase();
  }

  /**
   * @description Navigate to contact detail page
   */
  navigateToContact(id: number): void {
    window.open(`/#/partnerships/contacts/${id}`, '_blank');
  }

  /**
   * @description Navigate to partner detail page
   */
  navigateToPartner(id: number): void {
    window.open(`/#/partnerships/partners/${id}`, '_blank');
  }

  /**
   * @description Navigate to interaction detail page
   */
  navigateToInteraction(id: number): void {
    window.open(`/#/partnerships/interactions/${id}`, '_blank');
  }

  /**
   * @description Format date for display
   */
  formatDate(dateString?: string): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}

