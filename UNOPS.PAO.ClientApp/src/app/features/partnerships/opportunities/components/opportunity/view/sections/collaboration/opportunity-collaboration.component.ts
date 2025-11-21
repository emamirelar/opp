/**
 * @fileoverview Opportunity Collaboration Component - Wrapper for comments specific to opportunities
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CommentComponent } from '@shared/components/comments/comment.component';

/**
 * @class OpportunityCollaborationComponent
 * @description Wrapper component that integrates the reusable comment component 
 * specifically for opportunities. This allows for opportunity-specific customization
 * while keeping the core comment functionality generic.
 * 
 * @example
 * ```html
 * <app-opportunity-collaboration
 *   [opportunityId]="opportunity()!.id!"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-collaboration',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    CommentComponent
  ],
  templateUrl: './opportunity-collaboration.component.html',
  styleUrls: ['./opportunity-collaboration.component.scss']
})
export class OpportunityCollaborationComponent {
  /**
   * @description The opportunity ID
   * @type {Signal<number>}
   * @required
   */
  readonly opportunityId = input.required<number>();

  constructor(private translateService: TranslateService) {}

  /**
   * @description Get the panel header text
   * @returns {string} Translated header text
   */
  get panelHeader(): string {
    return this.translateService.instant('label.opportunity.collaborationComments');
  }
}

