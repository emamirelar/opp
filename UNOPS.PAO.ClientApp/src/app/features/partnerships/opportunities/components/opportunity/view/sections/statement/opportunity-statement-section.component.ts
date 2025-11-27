/**
 * @fileoverview Opportunity Statement Section Component - Manages AI-generated opportunity statement
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, output, signal, inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { MarkdownModule } from 'ngx-markdown';

// Services and Models
import { OpportunityService } from '../../../../../services/opportunity.service';
import { Opportunity } from '@shared/models/opportunity.model';
import { FeedbackDialogService } from '@shared/services/ui';
import { GoogleOAuthService } from '@core/services/auth/google-oauth.service';

/**
 * @class OpportunityStatementSectionComponent
 * @description Manages the AI-generated opportunity statement section with generate/regenerate functionality.
 * Displays markdown-formatted statement content.
 * 
 * @example
 * ```html
 * <app-opportunity-statement-section
 *   [opportunity]="opportunity()"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-statement-section',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    DialogModule,
    MarkdownModule
  ],
  templateUrl: './opportunity-statement-section.component.html',
  styleUrls: ['./opportunity-statement-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityStatementSectionComponent {
  // Services
  private readonly opportunityService = inject(OpportunityService);
  private readonly translateService = inject(TranslateService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly googleOAuthService = inject(GoogleOAuthService);
  private readonly cdr = inject(ChangeDetectorRef);

  /**
   * @description Input signal for opportunity data from parent
   * @type {Signal<Opportunity>}
   * @since 1.0.0
   */
  readonly opportunity = input.required<Opportunity>();

  /**
   * @description Input signal for update permission - controls visibility of generate/export buttons
   */
  readonly canUpdate = input<boolean>(false);

  /**
   * @description Output event when opportunity statement is generated/regenerated
   * @type {OutputEmitterRef<Opportunity>}
   * @param {Opportunity} opportunity - The updated opportunity with new statement
   * @example
   * ```html
   * <app-opportunity-statement-section
   *   [opportunity]="opportunity()"
   *   (opportunityUpdated)="handleOpportunityUpdate($event)"
   * />
   * ```
   * @since 1.0.0
   */
  readonly opportunityUpdated = output<Opportunity>();

  /**
   * @description Signal indicating if statement generation is in progress
   * @type {Signal<boolean>}
   * @default false
   * @since 1.0.0
   */
  readonly generatingStatement = signal<boolean>(false);

  /**
   * @description Signal indicating if export to Google Docs is in progress
   * @type {Signal<boolean>}
   * @default false
   * @since 1.0.0
   */
  readonly isExporting = signal<boolean>(false);

  /**
   * @description Signal to control visibility of export success dialog
   * @type {Signal<boolean>}
   * @default false
   * @since 1.0.0
   */
  showExportSuccessDialog = false;

  /**
   * @description URL of the exported Google Doc
   * @type {string | null}
   * @default null
   * @since 1.0.0
   */
  exportedDocUrl: string | null = null;

  /**
   * @description Generate or regenerate opportunity statement using AI
   * @returns {void}
   * @example
   * ```typescript
   * this.generateOpportunityStatement();
   * ```
   * @since 1.0.0
   */
  generateOpportunityStatement(): void {
    const opportunityId = this.opportunity()?.id;
    if (!opportunityId) return;

    this.generatingStatement.set(true);

    this.opportunityService.generateOpportunityStatement(opportunityId).subscribe({
      next: (response) => {
        this.generatingStatement.set(false);
        
        // Update the opportunity with the generated statement
        const currentOpportunity = this.opportunity();
        if (currentOpportunity) {
          const updatedOpportunity: Opportunity = {
            ...currentOpportunity,
            opportunityStatementMarkdown: response.statementMarkdown
          };
          
          // Emit updated opportunity to parent
          this.opportunityUpdated.emit(updatedOpportunity);
        }

        this.feedbackService.showSuccessToast({
          detail: this.translateService.instant('message.opportunity.statementGenerated'),
          summary: this.translateService.instant('message.success')
        });
        
        this.cdr.detectChanges();
      },
      error: () => {
        this.generatingStatement.set(false);
        this.cdr.detectChanges();
        // Error handled by global interceptor
      }
    });
  }

  /**
   * @description Export opportunity statement to Google Docs
   * @returns {Promise<void>}
   * @example
   * ```typescript
   * await this.exportToGoogleDoc();
   * ```
   * @since 1.0.0
   */
  async exportToGoogleDoc(): Promise<void> {
    const markdown = this.opportunity()?.opportunityStatementMarkdown;
    if (!markdown || this.isExporting()) {
      return;
    }

    this.isExporting.set(true);
    this.cdr.detectChanges();

    try {
      // Get valid Google OAuth ID token (will trigger auth popup if needed)
      let idToken: string;
      
      try {
        idToken = await this.googleOAuthService.getValidIdToken();
      } catch (authError) {
        console.error('❌ Google authentication failed:', authError);
        
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: this.translateService.instant('message.opportunity.exportAuthRequired')
        });
        
        this.isExporting.set(false);
        this.cdr.detectChanges();
        return;
      }

      // Prepare the request
      const formData = new FormData();
      const markdownBlob = new Blob([markdown], { type: 'text/markdown' });
      const filename = `${this.opportunity().name || 'Opportunity Statement'}`;
      formData.append('file', markdownBlob, filename);
      formData.append('data', JSON.stringify({ name: filename }));

      console.log('🌐 Exporting to Google Doc...');

      // Make API call with OAuth ID token
      const response = await fetch('https://api.ai.unops.org/v1/convert/markdown-to-google-doc', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${idToken}`
        },
        body: formData
      });

      if (!response.ok) {
        const errorText = await response.text();
        console.error('API Error:', errorText);
        
        // Handle authentication errors - refresh token and retry
        if (response.status === 401 || response.status === 403) {
          console.log('🔄 Token expired, refreshing and retrying...');
          
          try {
            // Refresh token and retry
            await this.googleOAuthService.refreshToken();
            
            // Retry the export with fresh token
            this.isExporting.set(false);
            this.cdr.detectChanges();
            return this.exportToGoogleDoc();
          } catch (refreshError) {
            console.error('❌ Token refresh failed:', refreshError);
            
            this.feedbackService.showErrorToast({
              summary: this.translateService.instant('message.error'),
              detail: this.translateService.instant('message.opportunity.exportAuthExpired')
            });
          }
        } else {
          this.feedbackService.showErrorToast({
            summary: this.translateService.instant('message.error'),
            detail: this.translateService.instant('message.opportunity.exportFailed')
          });
        }
        
        this.isExporting.set(false);
        this.cdr.detectChanges();
        return;
      }

      const result = await response.json();
      console.log('✅ Export successful:', result);
      
      // Extract the Google Doc URL from the response
      const docUrl = result.data?.data?.url || result.data?.url || result.url;
      
      if (docUrl) {
        this.exportedDocUrl = docUrl;
        this.showExportSuccessDialog = true;
        
        this.feedbackService.showSuccessToast({
          summary: this.translateService.instant('message.success'),
          detail: this.translateService.instant('message.opportunity.exportSuccess')
        });
      } else {
        console.error('No URL found in response:', result);
        this.feedbackService.showWarningToast({
          summary: this.translateService.instant('message.warning'),
          detail: this.translateService.instant('message.opportunity.exportNoUrl')
        });
      }

      this.cdr.detectChanges();
    } catch (error: any) {
      console.error('Error exporting to Google Doc:', error);
      
      let errorMessage = this.translateService.instant('message.opportunity.exportError');
      if (error.message) {
        errorMessage = error.message;
      }
      
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: errorMessage
      });
    } finally {
      this.isExporting.set(false);
      this.cdr.detectChanges();
    }
  }

  /**
   * @description Open the exported Google Doc in a new tab
   * @returns {void}
   * @example
   * ```typescript
   * this.openExportedDoc();
   * ```
   * @since 1.0.0
   */
  openExportedDoc(): void {
    if (this.exportedDocUrl) {
      window.open(this.exportedDocUrl, '_blank');
    }
  }

  /**
   * @description Close the export success dialog
   * @returns {void}
   * @since 1.0.0
   */
  closeExportDialog(): void {
    this.showExportSuccessDialog = false;
    this.exportedDocUrl = null;
    this.cdr.detectChanges();
  }

  /**
   * @description Handle markdown content ready event - configure link behavior
   * Prevents hash links from triggering Angular routing and opens external links in new tab
   * @returns {void}
   * @since 1.0.0
   */
  onMarkdownReady(): void {
    // Find all links in the markdown content and configure them
    setTimeout(() => {
      const markdownContainer = document.querySelector('.markdown-content');
      if (!markdownContainer) return;

      const links = markdownContainer.querySelectorAll('a');
      links.forEach((link: HTMLAnchorElement) => {
        const href = link.getAttribute('href');
        
        if (!href) return;

        // Handle hash links (internal anchors)
        if (href.startsWith('#')) {
          // Prevent default Angular routing for hash links
          link.addEventListener('click', (event: Event) => {
            event.preventDefault();
            event.stopPropagation();
            
            // Extract the anchor target (remove the # symbol)
            const targetId = href.substring(1);
            const targetElement = document.getElementById(targetId);
            
            if (targetElement) {
              // Smooth scroll to the target element
              targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
            }
          });
        } else {
          // For all other links (external or internal paths), open in new tab
          link.setAttribute('target', '_blank');
          link.setAttribute('rel', 'noopener noreferrer');
          
          // Add external link icon for visual indication
          if (!link.querySelector('.external-link-icon')) {
            const icon = document.createElement('i');
            icon.className = 'pi pi-external-link external-link-icon ml-1 text-xs';
            link.appendChild(icon);
          }
        }
      });
    }, 100);
  }
}

