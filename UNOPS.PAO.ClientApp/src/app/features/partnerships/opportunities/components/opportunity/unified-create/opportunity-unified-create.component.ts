/**
 * @fileoverview Unified Creation Dashboard - All creation methods in one interface
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

// PrimeNG
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { FileUploadModule } from 'primeng/fileupload';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';
import { ChipModule } from 'primeng/chip';
import { BadgeModule } from 'primeng/badge';
import { DividerModule } from 'primeng/divider';
import { FloatLabelModule } from 'primeng/floatlabel';

// Services
import { OpportunityDemoService } from '@shared/services/api/opportunity-demo.service';
import { FeedbackDialogService } from '@shared/services/ui';

interface AISuggestion {
  id: string;
  type: string;
  label: string;
  value: any;
  applied: boolean;
}

interface InteractionItem {
  id: number;
  type: string;
  description: string;
  date: string;
  selected: boolean;
}

/**
 * @class OpportunityUnifiedCreateComponent
 * @description Unified creation interface supporting multiple input methods simultaneously:
 * - Document upload with AI extraction
 * - Structured form input
 * - Selection from logged interactions
 * - Free-form notebook with AI assistance
 * 
 * All methods work together and are not mutually exclusive.
 * 
 * @example
 * ```html
 * <app-opportunity-unified-create></app-opportunity-unified-create>
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-unified-create',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    CardModule,
    FileUploadModule,
    InputTextModule,
    DropdownModule,
    CheckboxModule,
    ChipModule,
    BadgeModule,
    DividerModule,
    FloatLabelModule
  ],
  templateUrl: './opportunity-unified-create.component.html',
  styleUrls: ['./opportunity-unified-create.component.scss']
})
export class OpportunityUnifiedCreateComponent implements OnInit {
  private router = inject(Router);
  private demoService = inject(OpportunityDemoService);
  private feedbackService = inject(FeedbackDialogService);

  // UI State
  notebookExpanded = signal(true);
  interactionsPanelExpanded = signal(false);
  isCreating = signal(false);

  // Form data (minimal for demo)
  opportunityName = signal('');
  opportunityDescription = signal('');
  selectedOrgUnit = signal<any>(null);

  // Notebook
  notebookText = signal('');
  aiSuggestions = signal<AISuggestion[]>([]);

  // Document upload
  uploadedFiles = signal<any[]>([]);
  documentAISuggestions = signal<AISuggestion[]>([]);

  // Interactions
  recentInteractions = signal<InteractionItem[]>([
    {
      id: 1,
      type: 'Meeting',
      description: 'Meeting with World Bank - Water Infrastructure Discussion',
      date: '2025-10-20',
      selected: false
    },
    {
      id: 2,
      type: 'Email',
      description: 'Email thread with Ministry of Water Resources',
      date: '2025-10-22',
      selected: false
    },
    {
      id: 3,
      type: 'Call',
      description: 'Call with EU Commission - Funding Commitment',
      date: '2025-10-23',
      selected: false
    },
    {
      id: 4,
      type: 'Visit',
      description: 'Site visit to Bangladesh - Rural Water Assessment',
      date: '2025-10-18',
      selected: false
    }
  ]);

  // Mock org units
  orgUnits = signal([
    { id: 1, name: 'Regional Office - Asia Pacific' },
    { id: 2, name: 'Regional Office - Africa' },
    { id: 3, name: 'Regional Office - Latin America' },
    { id: 4, name: 'HQ - Infrastructure Practice' }
  ]);

  // Computed
  selectedInteractionsCount = computed(() => {
    return this.recentInteractions().filter(i => i.selected).length;
  });

  hasAISuggestions = computed(() => {
    return this.aiSuggestions().length > 0 || this.documentAISuggestions().length > 0;
  });

  ngOnInit(): void {
    // Initialize
  }

  /**
   * Handle file upload event
   */
  onFileUpload(event: any): void {
    const files = event.files || event.currentFiles;
    this.uploadedFiles.update(existing => [...existing, ...files]);

    // Simulate AI extraction from document
    setTimeout(() => {
      this.simulateDocumentAIExtraction(files[0]?.name || 'Document');
    }, 1000);
  }

  /**
   * Simulate AI extraction from uploaded document
   */
  private simulateDocumentAIExtraction(filename: string): void {
    const suggestions: AISuggestion[] = [
      {
        id: 'doc-1',
        type: 'name',
        label: 'Name: Water Infrastructure Initiative - South Asia',
        value: 'Water Infrastructure Initiative - South Asia',
        applied: false
      },
      {
        id: 'doc-2',
        type: 'budget',
        label: 'Budget: $2,500,000 USD',
        value: 2500000,
        applied: false
      },
      {
        id: 'doc-3',
        type: 'description',
        label: 'Comprehensive water infrastructure development program...',
        value: 'Comprehensive water infrastructure development program targeting rural communities',
        applied: false
      }
    ];

    this.documentAISuggestions.set(suggestions);

    this.feedbackService.showInfoToast({
      summary: 'AI Extraction Complete',
      detail: `Extracted ${suggestions.length} fields from ${filename}. Review and apply suggestions below.`
    });
  }

  /**
   * Handle notebook text changes with debouncing
   */
  onNotebookChange(): void {
    const text = this.notebookText();
    if (!text || text.length < 20) {
      this.aiSuggestions.set([]);
      return;
    }

    // Simple pattern matching to simulate AI
    this.detectKeywordsInNotebook(text);
  }

  /**
   * Detect keywords in notebook text and create suggestions
   */
  private detectKeywordsInNotebook(text: string): void {
    const suggestions: AISuggestion[] = [];

    // Budget detection
    const budgetMatch = text.match(/\$?\s*(\d+(?:\.\d+)?)\s*(?:M|million|USD)/i);
    if (budgetMatch) {
      const amount = parseFloat(budgetMatch[1]) * 1000000;
      suggestions.push({
        id: 'nb-budget',
        type: 'budget',
        label: `Budget: $${amount.toLocaleString()} USD`,
        value: amount,
        applied: false
      });
    }

    // Country detection
    const countries = ['Bangladesh', 'Nepal', 'Myanmar', 'India', 'Pakistan'];
    countries.forEach(country => {
      if (text.includes(country)) {
        suggestions.push({
          id: `nb-country-${country}`,
          type: 'country',
          label: `Country: ${country}`,
          value: country,
          applied: false
        });
      }
    });

    // Partner detection
    const partners = ['World Bank', 'EU Commission', 'UNDP', 'UNICEF'];
    partners.forEach(partner => {
      if (text.includes(partner)) {
        suggestions.push({
          id: `nb-partner-${partner}`,
          type: 'partner',
          label: `Partner: ${partner}`,
          value: partner,
          applied: false
        });
      }
    });

    // Date detection
    const dateMatch = text.match(/Q[1-4]\s+20\d{2}/i);
    if (dateMatch) {
      suggestions.push({
        id: 'nb-date',
        type: 'date',
        label: `Target Date: ${dateMatch[0]}`,
        value: dateMatch[0],
        applied: false
      });
    }

    this.aiSuggestions.set(suggestions);
  }

  /**
   * Apply a single AI suggestion
   */
  applySuggestion(suggestion: AISuggestion): void {
    // Apply to form based on type
    switch (suggestion.type) {
      case 'name':
        this.opportunityName.set(suggestion.value);
        break;
      case 'description':
        this.opportunityDescription.set(suggestion.value);
        break;
      case 'budget':
        // Would set budget field if we had it
        break;
    }

    // Mark as applied
    suggestion.applied = true;

    this.feedbackService.showSuccessToast({
      summary: 'Applied',
      detail: `${suggestion.label} applied to form`
    });
  }

  /**
   * Apply all AI suggestions
   */
  applyAllSuggestions(): void {
    const allSuggestions = [...this.aiSuggestions(), ...this.documentAISuggestions()];
    allSuggestions.forEach(suggestion => {
      if (!suggestion.applied) {
        this.applySuggestion(suggestion);
      }
    });
  }

  /**
   * Dismiss a suggestion
   */
  dismissSuggestion(suggestion: AISuggestion): void {
    this.aiSuggestions.update(sug => sug.filter(s => s.id !== suggestion.id));
    this.documentAISuggestions.update(sug => sug.filter(s => s.id !== suggestion.id));
  }

  /**
   * Toggle interaction selection
   */
  toggleInteraction(interaction: InteractionItem): void {
    this.recentInteractions.update(interactions =>
      interactions.map(i =>
        i.id === interaction.id ? { ...i, selected: !i.selected } : i
      )
    );
  }

  /**
   * Pull data from selected interactions
   */
  pullFromInteractions(): void {
    const selectedCount = this.selectedInteractionsCount();
    if (selectedCount === 0) {
      this.feedbackService.showWarningToast({
        summary: 'No Selection',
        detail: 'Please select at least one interaction'
      });
      return;
    }

    // Simulate AI structuring data from interactions
    this.feedbackService.showInfoToast({
      summary: 'Processing',
      detail: `Processing ${selectedCount} interaction(s)...`
    });

    setTimeout(() => {
      // Add some suggestions based on interactions
      const interactionSuggestions: AISuggestion[] = [
        {
          id: 'int-1',
          type: 'name',
          label: 'Name: Water Infrastructure Project (from interactions)',
          value: 'Water Infrastructure Project',
          applied: false
        },
        {
          id: 'int-2',
          type: 'partner',
          label: 'Partner: World Bank (mentioned in meeting)',
          value: 'World Bank',
          applied: false
        }
      ];

      this.documentAISuggestions.update(existing => [...existing, ...interactionSuggestions]);

      this.feedbackService.showSuccessToast({
        summary: 'Data Extracted',
        detail: `Extracted information from ${selectedCount} interaction(s). Review suggestions below.`
      });
    }, 1500);
  }

  /**
   * Toggle notebook expansion
   */
  toggleNotebook(): void {
    this.notebookExpanded.update(v => !v);
  }

  /**
   * Toggle interactions panel
   */
  toggleInteractionsPanel(): void {
    this.interactionsPanelExpanded.update(v => !v);
  }

  /**
   * Cancel creation and go back
   */
  cancel(): void {
    this.router.navigate(['/partnerships/opportunities']);
  }

  /**
   * Save as draft (future feature)
   */
  saveDraft(): void {
    this.feedbackService.showInfoToast({
      summary: 'Draft Saved',
      detail: 'Opportunity saved as draft (feature coming soon)'
    });
  }

  /**
   * Create opportunity - uses mock data and navigates to Option 1
   */
  createOpportunity(): void {
    this.isCreating.set(true);

    // Simulate API call delay
    setTimeout(() => {
      // Use the demo service's mock opportunity
      const mockId = 12345;

      this.feedbackService.showSuccessToast({
        summary: 'Success',
        detail: 'Opportunity created successfully!'
      });

      // Navigate to Option 1 with the mock opportunity ID
      this.router.navigate(['/partnerships/opportunities', mockId, 'view-option1']);

      this.isCreating.set(false);
    }, 1000);
  }
}

