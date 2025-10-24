/**
 * @fileoverview Option 3: Wizard-Guided Workflow - Step-by-step opportunity creation
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

// PrimeNG
import { StepperModule } from 'primeng/stepper';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { AvatarModule } from 'primeng/avatar';
import { MessageModule } from 'primeng/message';
import { ProgressBarModule } from 'primeng/progressbar';

// Services
import {
  OpportunityDemoService,
  DemoOpportunity,
} from '@shared/services/api/opportunity-demo.service';

/**
 * @class OpportunityOption3Component
 * @description Wizard-Guided Workflow - step-by-step interface that guides users
 * through opportunity creation with progressive disclosure and AI assistance.
 *
 * @example
 * ```html
 * <app-opportunity-option3></app-opportunity-option3>
 * ```
 *
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-option3',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    StepperModule,
    ButtonModule,
    CardModule,
    BadgeModule,
    TagModule,
    ChipModule,
    DividerModule,
    AvatarModule,
    MessageModule,
    ProgressBarModule,
  ],
  templateUrl: './opportunity-option3.component.html',
  styleUrls: ['./opportunity-option3.component.scss'],
})
export class OpportunityOption3Component implements OnInit {
  private demoService = inject(OpportunityDemoService);

  opportunity = signal<DemoOpportunity | null>(null);
  loading = signal(true);
  currentStepIndex = signal(0);

  // Step definitions
  steps = [
    { label: 'Getting Started', icon: 'pi-play', status: 'complete' },
    { label: "What We'll Deliver", icon: 'pi-box', status: 'complete' },
    { label: "Who's Involved", icon: 'pi-users', status: 'current' },
    { label: 'Why This Matters', icon: 'pi-star', status: 'pending' },
    { label: 'When & Where', icon: 'pi-calendar', status: 'pending' },
    { label: 'Review & Submit', icon: 'pi-check-circle', status: 'pending' },
  ];

  // Step-specific AI guidance
  stepGuidance = computed(() => {
    const stepIndex = this.currentStepIndex();
    const guidance: {
      [key: number]: { title: string; suggestions: string[] };
    } = {
      0: {
        title: 'Upload Documents for AI Extraction',
        suggestions: [
          'Upload concept notes, proposals, or partner correspondence',
          'AI will extract key information to save you time',
          'You can also enter information manually',
        ],
      },
      1: {
        title: 'Define Core Elements',
        suggestions: [
          'Provide clear, concise deliverable descriptions',
          'Link each deliverable to a service line',
          'Consider breaking complex work into multiple deliverables',
        ],
      },
      2: {
        title: 'Identify Partners & Team',
        suggestions: [
          'I found 2 funding partners in your uploaded documents',
          'Based on similar projects, consider adding a gender advisor',
          'Client contact information will help with communication',
        ],
      },
      3: {
        title: 'Strategic Alignment & Impact',
        suggestions: [
          'Based on your deliverables, SDG 6 (Clean Water) is highly relevant',
          'Consider adding SDG 13 (Climate Action) as secondary',
          'Define measurable outcomes for better impact tracking',
        ],
      },
      4: {
        title: 'Timeline & Geographic Scope',
        suggestions: [
          'Myanmar flagged as high-risk context - review carefully',
          'Monsoon season may affect timeline - plan accordingly',
          'Typical development timeline for similar projects: 16-20 weeks',
        ],
      },
      5: {
        title: 'Final Review & Submission',
        suggestions: [
          'Completeness: 78% - all required fields provided',
          'Complexity score: 7.2/10 (Medium-High)',
          '4 risks identified - recommend review before submission',
        ],
      },
    };
    return guidance[stepIndex] || { title: '', suggestions: [] };
  });

  // Step completeness
  stepCompleteness = computed(() => {
    const completeness: { [key: number]: number } = {
      0: 100,
      1: 100,
      2: 75,
      3: 100,
      4: 85,
      5: 78,
    };
    return completeness[this.currentStepIndex()] || 0;
  });

  ngOnInit(): void {
    this.loadOpportunity();
  }

  private loadOpportunity(): void {
    this.loading.set(true);
    this.demoService.getDemoOpportunity().subscribe({
      next: (opp) => {
        this.opportunity.set(opp);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading demo opportunity:', error);
        this.loading.set(false);
      },
    });
  }

  nextStep(): void {
    if (this.currentStepIndex() < this.steps.length - 1) {
      this.currentStepIndex.update((i) => i + 1);
    }
  }

  prevStep(): void {
    if (this.currentStepIndex() > 0) {
      this.currentStepIndex.update((i) => i - 1);
    }
  }

  goToStep(index: number): void {
    this.currentStepIndex.set(index);
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  getSeverityClass(severity: string): string {
    const severityMap: { [key: string]: string } = {
      High: 'danger',
      Medium: 'warning',
      Low: 'info',
    };
    return severityMap[severity] || 'info';
  }
}
