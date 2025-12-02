/**
 * @fileoverview Dialog component for creating opportunities from selected interactions
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
  computed,
  effect,
  untracked,
  OnInit,
  input,
  output,
  model
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { HttpClient } from '@angular/common/http';

// PrimeNG imports
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { DividerModule } from 'primeng/divider';
import { TagModule } from 'primeng/tag';
import { MessageModule } from 'primeng/message';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { FloatLabelModule } from 'primeng/floatlabel';
import { TooltipModule } from 'primeng/tooltip';
import { SelectModule } from 'primeng/select';
import { ChipModule } from 'primeng/chip';
import { AvatarModule } from 'primeng/avatar';

// Models
import {
  InteractionSummary,
  CreateOpportunityFromInteractionsConfig,
  DialogState,
  ProposedOpportunityResponse,
  ProposeOpportunityRequest
} from '../../models/interaction-selection.model';

// Services
import { FeedbackDialogService } from '@shared/services/ui';
import { InteractionService } from '../../services/interaction.service';
import { GoogleDriveService } from '@shared/services/google-drive.service';
import { DocumentService } from '@shared/services/api/document.service';
import { DrivePickerService } from '@shared/services/integration/drive-picker.service';
import { firstValueFrom } from 'rxjs';

/**
 * @class CreateOpportunityFromInteractionsDialogComponent
 * @description Dialog for creating opportunities from one or more interactions.
 * Supports two modes:
 * - List view: Multiple interactions pre-selected
 * - Detail view: Single interaction with option to add more
 * 
 * @example
 * ```html
 * <app-create-opportunity-from-interactions-dialog
 *   [(visible)]="showDialog"
 *   [config]="dialogConfig"
 *   (opportunityCreated)="handleOpportunityCreated($event)">
 * </app-create-opportunity-from-interactions-dialog>
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-create-opportunity-from-interactions-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    CheckboxModule,
    DividerModule,
    TagModule,
    MessageModule,
    ProgressSpinnerModule,
    FloatLabelModule,
    TooltipModule,
    SelectModule,
    ChipModule,
    AvatarModule
  ],
  templateUrl: './create-opportunity-from-interactions-dialog.component.html',
  styleUrls: ['./create-opportunity-from-interactions-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateOpportunityFromInteractionsDialogComponent implements OnInit {
  private http = inject(HttpClient);
  private translateService = inject(TranslateService);
  private feedbackDialogService = inject(FeedbackDialogService);
  private interactionService = inject(InteractionService);
  private googleDriveService = inject(GoogleDriveService);
  private documentService = inject(DocumentService);
  private drivePickerService = inject(DrivePickerService);
  
  // Google Drive auth for Office file conversion
  private googleDriveAuthAvailable = false;

  // Input for visibility - using model for two-way binding
  readonly visible = model<boolean>(false);
  readonly config = input.required<CreateOpportunityFromInteractionsConfig>();

  // Outputs
  readonly opportunityCreated = output<any>();

  // Dialog state
  currentStep = signal<'select' | 'review' | 'creating'>('select');
  selectedInteractions = signal<InteractionSummary[]>([]);
  availableInteractions = signal<InteractionSummary[]>([]);
  showAdditionalSelection = signal(false);
  searchQuery = signal('');
  generating = signal(false);
  loadingInteractions = signal(false);
  
  // Infinite scrolling state
  private currentPage = 1;
  private readonly pageSize = 50;
  private totalInteractions = 0;
  private hasMoreInteractions = true;
  loadingMoreInteractions = signal<boolean>(false);
  
  // Debounce timer for search
  private searchDebounceTimer: any = null;

  // Form fields
  opportunityName = signal('');
  opportunityDescription = signal('');
  isFundingPartner = signal(false);
  isClientPartner = signal(false);
  showValidationError = signal(false);
  
  // Document upload state
  selectedFiles = signal<{file: File, documentTypeId: number | null}[]>([]);
  uploadedDocuments = signal<{gcsPath: string, mimeType: string, name: string, documentTypeId: number | null}[]>([]);
  isUploadingToGCS = signal(false);
  uploadProgress = signal<string>('');
  selectedExistingDocumentIds = signal<number[]>([]);
  availablePartnerDocuments = signal<any[]>([]);
  showExistingDocuments = signal(false);
  selectedGoogleDriveFiles = signal<{id: string, name: string, mimeType: string, documentTypeId: number | null}[]>([]); // Files from Google Drive picker
  
  // Document types for Opportunity entity
  documentTypes = signal<any[]>([]);
  loadingDocumentTypes = signal(false);
  
  // Document type selection dialog
  showDocumentTypeDialog = signal(false);
  pendingFiles = signal<File[]>([]);
  pendingGoogleDriveFiles = signal<{id: string, name: string, mimeType: string}[]>([]);
  selectedDocumentTypeForDialog = signal<number | null>(null);

  // Proposed opportunity data (Step 2)
  proposedOpportunity = signal<ProposedOpportunityResponse | null>(null);
  
  // Field selection for proposal review (tracks which AI-proposed fields to accept)
  selectedFields = signal<Map<string, boolean>>(new Map());

  // Computed properties
  readonly mode = computed(() => this.config().mode);
  readonly partnerId = computed(() => this.config().partnerId);
  readonly partnerName = computed(() => this.config().partnerName);
  readonly currentInteractionId = computed(() => this.config().currentInteractionId);

  readonly selectedCount = computed(() => 
    this.selectedInteractions().filter(i => i.selected !== false).length
  );

  readonly canGenerate = computed(() => {
    const hasInteractions = this.selectedCount() > 0;
    const hasNewDocs = this.selectedFiles().length > 0 || this.selectedGoogleDriveFiles().length > 0;
    const hasExistingDocs = this.selectedExistingDocumentIds().length > 0;
    const hasAnySources = hasInteractions || hasNewDocs || hasExistingDocs;
    
    // Only require partner role selection when partner fields are shown (list-view mode from partner context)
    const needsPartnerRole = this.showPartnerFields();
    const hasRoleIfNeeded = !needsPartnerRole || (this.isFundingPartner() || this.isClientPartner());
    
    // Name is required (max 255 chars), description is optional
    const nameValue = this.opportunityName().trim();
    const hasValidName = nameValue.length > 0 && nameValue.length <= 255;
    
    return hasAnySources && hasValidName && hasRoleIfNeeded;
  });
  
  readonly canCreate = computed(() => {
    // Name is required (max 255 chars), description is optional
    const nameValue = this.opportunityName().trim();
    const hasValidName = nameValue.length > 0 && nameValue.length <= 255;
    
    // If in partner context (showPartnerFields), need role selection
    const needsPartnerRole = this.showPartnerFields();
    const hasRoleIfNeeded = !needsPartnerRole || (this.isFundingPartner() || this.isClientPartner());
    
    return hasValidName && hasRoleIfNeeded;
  });
  
  readonly showPartnerFields = computed(() => {
    // Only show partner role selection when:
    // 1. We have a partner ID (partner context)
    // 2. Mode is 'list-view' (from partner opportunities tab, not from interaction detail)
    const cfg = this.config();
    return this.partnerId() && this.partnerId() > 0 && cfg.mode === 'list-view';
  });
  
  readonly selectedFieldsCount = computed(() => {
    return Array.from(this.selectedFields().values()).filter(v => v).length;
  });
  
  readonly totalProposalFields = computed(() => {
    const proposal = this.proposedOpportunity();
    if (!proposal || !proposal.opportunity) return 0;
    
    let count = 0;
    const opp = proposal.opportunity;
    
    // Count non-empty fields
    if (opp.name) count++;
    if (opp.description) count++;
    if (opp.responsibleOrgUnitName) count++;
    if (opp.proposedInitiativeTypeName) count++;
    if (opp.initiativeBudgetUSD) count++;
    if (opp.strategicAlignment) count++;
    if (opp.resultsFocus) count++;
    if (opp.expectedBeneficiaries) count++;
    if (opp.intendedImpactOutcomes) count++;
    if (opp.targetSigningDate) count++;
    if (opp.targetDeliveryDate) count++;
    if (opp.deliverables && opp.deliverables.length > 0) count++;
    if (opp.fundingPartners && opp.fundingPartners.length > 0) count++;
    if (opp.clientPartners && opp.clientPartners.length > 0) count++;
    if (opp.stakeholders && opp.stakeholders.length > 0) count++;
    if (opp.countries && opp.countries.length > 0) count++;
    if (opp.sdGs && opp.sdGs.length > 0) count++;
    
    return count;
  });
  
  readonly allFieldsSelected = computed(() => {
    const total = this.totalProposalFields();
    const selected = this.selectedFieldsCount();
    return total > 0 && total === selected;
  });
  
  // Partner role selection management
  // Map<partnerId, {isFunding: boolean, isClient: boolean, selected: boolean}>
  readonly partnerRoleSelections = signal<Map<number, { isFunding: boolean, isClient: boolean, selected: boolean }>>(new Map());
  
  // Unified partner list merging funding and client partners
  readonly allProposedPartners = computed(() => {
    const proposal = this.proposedOpportunity();
    if (!proposal || !proposal.opportunity) return [];
    
    const partnerMap = new Map<number, any>();
    
    // Add funding partners
    if (proposal.opportunity.fundingPartners) {
      for (const fp of proposal.opportunity.fundingPartners) {
        if (!fp.partnerId) continue; // Skip if partnerId is undefined
        partnerMap.set(fp.partnerId, { 
          ...fp, 
          roles: { isFunding: true, isClient: false }
        });
      }
    }
    
    // Add/merge client partners
    if (proposal.opportunity.clientPartners) {
      for (const cp of proposal.opportunity.clientPartners) {
        if (!cp.partnerId) continue; // Skip if partnerId is undefined
        const existing = partnerMap.get(cp.partnerId);
        if (existing) {
          existing.roles.isClient = true;
        } else {
          partnerMap.set(cp.partnerId, { 
            ...cp, 
            partnerName: cp.partnerName,
            partnerLogoUrl: cp.partnerLogoUrl,
            partnerId: cp.partnerId,
            roles: { isFunding: false, isClient: true }
          });
        }
      }
    }
    
    return Array.from(partnerMap.values());
  });

  readonly filteredAvailableInteractions = computed(() => {
    // Don't filter here - filtering is done server-side via search
    return this.availableInteractions();
  });

  // Effect to watch for dialog visibility and load interactions
  private loadInteractionsEffect = effect(() => {
    const isVisible = this.visible();
    const cfg = this.config();
    
    if (isVisible && cfg) {
      // Use untracked to prevent infinite loops and queueMicrotask for timing
      untracked(() => {
        queueMicrotask(() => {
          this.loadInitialInteractions();
        });
      });
    } else if (!isVisible) {
      // Reset form when dialog is closed
      untracked(() => {
        queueMicrotask(() => {
          this.reset();
        });
      });
    }
  });

  ngOnInit(): void {
    // Effect is now defined as a class field
    
    // Load document types for Opportunity entity
    this.loadDocumentTypes();
    
    // Initialize Google Drive auth for Office file conversion
    this.googleDriveService.initializeAuth().subscribe({
      next: (authAvailable) => {
        this.googleDriveAuthAvailable = authAvailable;
        if (authAvailable) {
          console.log('✅ Google Drive auth initialized for document conversion');
        } else {
          console.warn('⚠️ Google Drive auth not available - Office file conversion will not be possible');
        }
      },
      error: (error) => {
        console.error('❌ Failed to initialize Google Drive auth:', error);
        this.googleDriveAuthAvailable = false;
      }
    });
  }

  /**
   * Load initial interactions based on config
   */
  private async loadInitialInteractions(): Promise<void> {
    try {
      const cfg = this.config();
      
      // Clear existing selections first
      this.selectedInteractions.set([]);
      this.availableInteractions.set([]);
      
      if (cfg.mode === 'detail-view' && cfg.currentInteractionId) {
        // Load the current interaction from detail view
        console.log('📝 Loading interaction for detail view:', cfg.currentInteractionId);
        const interaction = await this.loadInteractionSummary(cfg.currentInteractionId);
        if (interaction) {
          interaction.selected = true;
          this.selectedInteractions.set([interaction]);
          console.log('✅ Interaction loaded and selected:', interaction);
        } else {
          console.warn('⚠️ Failed to load interaction:', cfg.currentInteractionId);
        }
      } else if (cfg.preSelectedInteractionIds && cfg.preSelectedInteractionIds.length > 0) {
        // Load pre-selected interactions from list view
        console.log('📝 Loading pre-selected interactions:', cfg.preSelectedInteractionIds);
        const interactions = await this.loadMultipleInteractionSummaries(cfg.preSelectedInteractionIds);
        interactions.forEach(i => i.selected = true);
        this.selectedInteractions.set(interactions);
        console.log('✅ Interactions loaded:', interactions.length);
      }
    } catch (error) {
      console.error('❌ Error loading initial interactions:', error);
    }
  }

  /**
   * Load a single interaction summary by ID
   */
  private async loadInteractionSummary(interactionId: number): Promise<InteractionSummary | null> {
    try {
      const response = await this.http.get<any>(`/api/interactions/${interactionId}`, { observe: 'response' }).toPromise();
      if (response && response.body) {
        return this.mapToInteractionSummary(response.body);
      }
      return null;
    } catch (error) {
      console.error('Error loading interaction:', error);
      return null;
    }
  }

  /**
   * Load multiple interaction summaries by IDs
   */
  private async loadMultipleInteractionSummaries(ids: number[]): Promise<InteractionSummary[]> {
    try {
      const promises = ids.map(id => this.loadInteractionSummary(id));
      const results = await Promise.all(promises);
      return results.filter(i => i !== null) as InteractionSummary[];
    } catch (error) {
      console.error('Error loading interactions:', error);
      return [];
    }
  }

  /**
   * Map backend interaction to summary model
   */
  private mapToInteractionSummary(interaction: any): InteractionSummary {
    return {
      id: interaction.id,
      subject: interaction.subject,
      type: interaction.type,
      date: interaction.date,
      description: interaction.description,
      partnerNames: interaction.partners?.map((p: any) => p.name) || [],
      contactNames: interaction.contacts?.map((c: any) => c.name) || [],
      selected: false
    };
  }

  /**
   * Check if interaction is the current one (in detail view mode)
   */
  isCurrentInteraction(id: number): boolean {
    return id === this.currentInteractionId();
  }

  /**
   * Toggle additional interaction selection panel
   */
  toggleAdditionalSelection(): void {
    this.showAdditionalSelection.update(v => !v);
    if (this.showAdditionalSelection() && this.availableInteractions().length === 0) {
      this.loadAvailableInteractions();
    }
  }

  /**
   * Load available interactions based on context (partner-specific or all)
   */
  private async loadAvailableInteractions(): Promise<void> {
    // Reset pagination for fresh load
    this.currentPage = 1;
    this.hasMoreInteractions = true;
    
    try {
      this.loadingInteractions.set(true);
      const partnerIdValue = this.partnerId();
      let interactions: any[] = [];
      
      // Decide which endpoint to use based on partner context
      if (partnerIdValue && partnerIdValue > 0) {
        // Partner-specific interactions
        console.log('📝 Loading interactions for partner:', partnerIdValue);
        const response = await this.http
          .get<any[]>(`/api/partner/${partnerIdValue}/interactions`)
          .toPromise();
        interactions = response || [];
        this.totalInteractions = interactions.length;
        this.hasMoreInteractions = false; // Partner endpoint doesn't support pagination
      } else {
        // All interactions - uses paginated brief endpoint
        console.log('📝 Loading all interactions (page 1)');
        const response = await this.http
          .get<any>(`/api/interactions-brief?pageSize=${this.pageSize}&pageIndex=1`)
          .toPromise();
        
        // The endpoint returns paginated data with structure: { records: [...], totalCount: number }
        if (response) {
          if (Array.isArray(response)) {
            interactions = response;
            this.totalInteractions = response.length;
            this.hasMoreInteractions = false;
          } else if (response.records && Array.isArray(response.records)) {
            interactions = response.records;
            this.totalInteractions = response.totalCount || 0;
            this.hasMoreInteractions = interactions.length < this.totalInteractions;
          } else if (response.items && Array.isArray(response.items)) {
            interactions = response.items;
            this.totalInteractions = response.totalCount || 0;
            this.hasMoreInteractions = interactions.length < this.totalInteractions;
          } else {
            console.warn('⚠️ Unexpected response format:', response);
            console.log('Response keys:', Object.keys(response));
            interactions = [];
            this.totalInteractions = 0;
            this.hasMoreInteractions = false;
          }
        }
      }

      if (!interactions || interactions.length === 0) {
        this.availableInteractions.set([]);
        this.loadingInteractions.set(false);
        return;
      }

      // Map to summary - mark selected ones but DON'T filter them out
      const selectedIds = this.selectedInteractions().map(i => i.id);
      const available = interactions.map(i => {
        const summary = this.mapToInteractionSummary(i);
        summary.selected = selectedIds.includes(summary.id);
        return summary;
      });

      this.availableInteractions.set(available);
      console.log(`✅ Loaded ${available.length} of ${this.totalInteractions} interactions`);
    } catch (error) {
      console.error('❌ Error loading available interactions:', error);
      this.feedbackDialogService.showErrorToast({
        summary: this.translateService.instant('common.error.title'),
        detail: this.translateService.instant('message.error.loadingInteractions')
      });
      this.availableInteractions.set([]);
    } finally {
      this.loadingInteractions.set(false);
    }
  }
  
  /**
   * Load more interactions for infinite scrolling
   */
  async loadMoreInteractions(): Promise<void> {
    // Don't load if already loading or no more data
    if (this.loadingMoreInteractions() || !this.hasMoreInteractions) {
      return;
    }
    
    // Only works for paginated endpoint (when no partnerId)
    const partnerIdValue = this.partnerId();
    if (partnerIdValue && partnerIdValue > 0) {
      return; // Partner-specific endpoint doesn't support pagination
    }
    
    try {
      this.loadingMoreInteractions.set(true);
      this.currentPage++;
      
      console.log(`📝 Loading more interactions (page ${this.currentPage})...`);
      const response = await this.http
        .get<any>(`/api/interactions-brief?pageSize=${this.pageSize}&pageIndex=${this.currentPage}`)
        .toPromise();
      
      let interactions: any[] = [];
      if (response && response.records && Array.isArray(response.records)) {
        interactions = response.records;
      }
      
      if (interactions.length === 0) {
        this.hasMoreInteractions = false;
        console.log('✅ No more interactions to load');
        return;
      }
      
      // Map to summary and append to existing list
      const selectedIds = this.selectedInteractions().map(i => i.id);
      const newInteractions = interactions.map(i => {
        const summary = this.mapToInteractionSummary(i);
        summary.selected = selectedIds.includes(summary.id);
        return summary;
      });
      
      // Append to existing interactions
      this.availableInteractions.update(existing => [...existing, ...newInteractions]);
      
      // Check if we have more data
      const totalLoaded = this.availableInteractions().length;
      this.hasMoreInteractions = totalLoaded < this.totalInteractions;
      
      console.log(`✅ Loaded ${newInteractions.length} more interactions (${totalLoaded} of ${this.totalInteractions} total)`);
    } catch (error) {
      console.error('❌ Error loading more interactions:', error);
      // Don't show error toast for "load more" failures to avoid annoying users
    } finally {
      this.loadingMoreInteractions.set(false);
    }
  }
  
  /**
   * Handle scroll event for infinite scrolling
   */
  onInteractionListScroll(event: Event): void {
    const target = event.target as HTMLElement;
    const scrollPosition = target.scrollTop + target.clientHeight;
    const scrollHeight = target.scrollHeight;
    
    // Load more when user scrolls to within 50px of bottom
    if (scrollPosition >= scrollHeight - 50) {
      this.loadMoreInteractions();
    }
  }
  
  /**
   * Search interactions using the search endpoint with debounce
   */
  async searchInteractions(query: string): Promise<void> {
    // Clear existing timer
    if (this.searchDebounceTimer) {
      clearTimeout(this.searchDebounceTimer);
    }
    
    // If search cleared, reload available interactions immediately
    if (!query || query.trim().length === 0) {
      await this.loadAvailableInteractions();
      return;
    }
    
    // Debounce: wait 500ms after user stops typing
    this.searchDebounceTimer = setTimeout(async () => {
      try {
        this.loadingInteractions.set(true);
        console.log('🔍 Searching interactions:', query);
        
        const partnerIdValue = this.partnerId();
        let url = `/api/interactions/search?query=${encodeURIComponent(query)}`;
        
        // Add partner filter if available
        if (partnerIdValue && partnerIdValue > 0) {
          url += `&partnerId=${partnerIdValue}`;
        }
        
        const response = await this.http
          .get<any>(url)
          .toPromise();
        
        let interactions: any[] = [];
        if (response) {
          // Handle both array and paginated response
          if (Array.isArray(response)) {
            interactions = response;
          } else if (response.records && Array.isArray(response.records)) {
            interactions = response.records;  // ✅ Search endpoint uses 'records'
          } else if (response.items && Array.isArray(response.items)) {
            interactions = response.items;  // Fallback
          } else {
            console.warn('⚠️ Unexpected search response format:', response);
            console.log('Response keys:', Object.keys(response));
            interactions = [];
          }
        }
        
        // Map to summary - mark selected ones but don't filter them out
        const selectedIds = this.selectedInteractions().map(i => i.id);
        const available = interactions.map(i => {
          const summary = this.mapToInteractionSummary(i);
          summary.selected = selectedIds.includes(summary.id);
          return summary;
        });
        
        this.availableInteractions.set(available);
        this.hasMoreInteractions = false; // Search results don't support pagination
        console.log('✅ Search results:', available.length);
      } catch (error) {
        console.error('❌ Error searching interactions:', error);
        this.feedbackDialogService.showErrorToast({
          summary: this.translateService.instant('common.error.title'),
          detail: this.translateService.instant('message.error.searchingInteractions')
        });
        this.availableInteractions.set([]);
      } finally {
        this.loadingInteractions.set(false);
      }
    }, 500); // 500ms debounce delay
  }

  /**
   * Toggle interaction selection (add or remove from selected list)
   */
  addInteraction(interaction: InteractionSummary): void {
    if (this.isSelected(interaction.id)) {
      // Already selected, deselect it (but can't deselect current interaction)
      if (this.isCurrentInteraction(interaction.id)) {
        return; // Can't deselect the current interaction in detail view
      }
      
      // Remove from selected list
      this.selectedInteractions.update(list =>
        list.filter(i => i.id !== interaction.id)
      );
      
      // Update the selected flag in available list
      this.availableInteractions.update(list =>
        list.map(i => i.id === interaction.id ? { ...i, selected: false } : i)
      );
    } else {
      // Not selected, add it to selected list
      const interactionToAdd = { ...interaction, selected: true };
      this.selectedInteractions.update(list => [...list, interactionToAdd]);
      
      // Update the selected flag in available list
      this.availableInteractions.update(list =>
        list.map(i => i.id === interaction.id ? { ...i, selected: true } : i)
      );
    }
  }

  /**
   * Remove interaction from selected list
   */
  removeInteraction(interactionId: number): void {
    // Can't remove the current interaction in detail view
    if (this.isCurrentInteraction(interactionId)) {
      return;
    }

    // Remove from selected list
    this.selectedInteractions.update(list =>
      list.filter(i => i.id !== interactionId)
    );

    // Update the selected flag in available list
    this.availableInteractions.update(list =>
      list.map(i => i.id === interactionId ? { ...i, selected: false } : i)
    );
  }

  /**
   * Check if interaction is selected
   */
  isSelected(interactionId: number): boolean {
    return this.selectedInteractions().some(i => i.id === interactionId);
  }
  
  /**
   * Handle file selection for upload - opens document type dialog
   */
  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.pendingFiles.set(Array.from(input.files));
      this.selectedDocumentTypeForDialog.set(null);
      this.showDocumentTypeDialog.set(true);
    }
    // Reset input
    input.value = '';
  }
  
  /**
   * Confirm document type selection and add files
   */
  confirmDocumentType(): void {
    const documentTypeId = this.selectedDocumentTypeForDialog();
    
    if (!documentTypeId) {
      this.feedbackDialogService.showWarningToast({
        summary: this.translateService.instant('common.warning.title'),
        detail: this.translateService.instant('message.validation.selectDocumentType')
      });
      return;
    }
    
    // Add pending local files
    if (this.pendingFiles().length > 0) {
      const filesWithType = this.pendingFiles().map(file => ({
        file: file,
        documentTypeId: documentTypeId
      }));
      this.selectedFiles.update(existing => [...existing, ...filesWithType]);
    }
    
    // Add pending Google Drive files
    if (this.pendingGoogleDriveFiles().length > 0) {
      const driveFilesWithType = this.pendingGoogleDriveFiles().map(file => ({
        ...file,
        documentTypeId: documentTypeId
      }));
      this.selectedGoogleDriveFiles.update(existing => [...existing, ...driveFilesWithType]);
    }
    
    // Close dialog and reset
    this.showDocumentTypeDialog.set(false);
    this.pendingFiles.set([]);
    this.pendingGoogleDriveFiles.set([]);
    this.selectedDocumentTypeForDialog.set(null);
  }
  
  /**
   * Cancel document type selection
   */
  cancelDocumentTypeSelection(): void {
    this.showDocumentTypeDialog.set(false);
    this.pendingFiles.set([]);
    this.pendingGoogleDriveFiles.set([]);
    this.selectedDocumentTypeForDialog.set(null);
  }
  
  /**
   * Remove selected file from list
   */
  removeSelectedFile(index: number): void {
    this.selectedFiles.update(files => files.filter((_, i) => i !== index));
  }
  
  /**
   * Toggle existing documents panel and load available documents
   */
  async toggleExistingDocuments(): Promise<void> {
    this.showExistingDocuments.update(v => !v);
    
    // Load documents if opening panel for the first time
    if (this.showExistingDocuments() && this.availablePartnerDocuments().length === 0) {
      await this.loadPartnerDocuments();
    }
  }
  
  /**
   * Load documents from partner for selection
   */
  private async loadPartnerDocuments(): Promise<void> {
    try {
      const partnerIdValue = this.partnerId();
      
      if (!partnerIdValue || partnerIdValue === 0) {
        this.feedbackDialogService.showWarningToast({
          summary: this.translateService.instant('common.warning.title'),
          detail: this.translateService.instant('message.partnerRequiredToLoadDocuments')
        });
        return;
      }
      
      // Call API to get partner documents
      const documents = await this.http
        .get<any[]>(`/api/partner/${partnerIdValue}/documents`)
        .toPromise();
      
      if (documents) {
        this.availablePartnerDocuments.set(documents);
      }
    } catch (error) {
      console.error('Error loading partner documents:', error);
      this.feedbackDialogService.showErrorToast({
        summary: this.translateService.instant('common.error.title'),
        detail: this.translateService.instant('message.error.loadingDocuments')
      });
    }
  }
  
  /**
   * Toggle existing document selection
   */
  toggleExistingDocument(documentId: number): void {
    const ids = this.selectedExistingDocumentIds();
    if (ids.includes(documentId)) {
      this.selectedExistingDocumentIds.set(ids.filter(id => id !== documentId));
    } else {
      this.selectedExistingDocumentIds.set([...ids, documentId]);
    }
  }
  
  /**
   * Check if existing document is selected
   */
  isExistingDocumentSelected(documentId: number): boolean {
    return this.selectedExistingDocumentIds().includes(documentId);
  }
  
  /**
   * Load document types for Opportunity entity
   */
  private async loadDocumentTypes(): Promise<void> {
    try {
      this.loadingDocumentTypes.set(true);
      console.log('📝 Loading document types for Opportunity entity...');
      
      const response = await this.http
        .get<any>('/api/document-type/Opportunity')
        .toPromise();
      
      // Handle paginated response structure
      let documentTypes: any[] = [];
      if (response) {
        if (Array.isArray(response)) {
          documentTypes = response;
        } else if (response.records && Array.isArray(response.records)) {
          documentTypes = response.records; // ✅ Paginated response
        } else {
          console.warn('⚠️ Unexpected response format:', response);
        }
      }
      
      if (documentTypes.length > 0) {
        this.documentTypes.set(documentTypes);
        console.log('✅ Loaded document types:', documentTypes.length, documentTypes);
      } else {
        console.warn('⚠️ No document types found for Opportunity entity');
        this.documentTypes.set([]);
      }
    } catch (error) {
      console.error('❌ Error loading document types:', error);
      this.feedbackDialogService.showErrorToast({
        summary: this.translateService.instant('common.error.title'),
        detail: this.translateService.instant('message.error.loadingDocumentTypes')
      });
      this.documentTypes.set([]);
    } finally {
      this.loadingDocumentTypes.set(false);
    }
  }
  
  /**
   * Open Google Drive picker to select files
   */
  openGoogleDrivePicker(): void {
    // Set accepted MIME types for documents (PDF, Word, Excel, PowerPoint)
    const acceptedMIMETypes = [
      'application/pdf',
      'application/vnd.google-apps.document',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/msword',
      'application/vnd.google-apps.spreadsheet',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'application/vnd.ms-excel',
      'application/vnd.google-apps.presentation',
      'application/vnd.openxmlformats-officedocument.presentationml.presentation',
      'application/vnd.ms-powerpoint'
    ].join(',');
    
    this.drivePickerService.setAcceptedMIMETypes(acceptedMIMETypes);
    
    // Subscribe to file selection events
    const subscription = this.drivePickerService.onFilesSelectedEmitter.subscribe({
      next: (event: any) => {
        this.handleGoogleDriveFilesSelected(event);
        subscription.unsubscribe(); // Clean up subscription
      }
    });
    
    // Open the Google Drive picker
    this.drivePickerService.openPicker();
  }
  
  /**
   * Handle files selected from Google Drive picker - opens document type dialog
   */
  private handleGoogleDriveFilesSelected(event: any): void {
    if (event.files && event.files.length > 0) {
      const newFiles = event.files.map((file: any) => ({
        id: file.id,
        name: file.name,
        mimeType: file.mimeType
      }));
      this.pendingGoogleDriveFiles.set(newFiles);
      this.selectedDocumentTypeForDialog.set(null);
      this.showDocumentTypeDialog.set(true);
      
      console.log('✅ Selected Google Drive files for type selection:', newFiles);
    }
  }
  
  /**
   * Remove selected Google Drive file from list
   */
  removeGoogleDriveFile(index: number): void {
    this.selectedGoogleDriveFiles.update(files => files.filter((_, i) => i !== index));
  }
  
  /**
   * Upload selected files (local + Google Drive) to GCS and get GCS URIs
   */
  private async uploadFilesToGCS(): Promise<{gcsPath: string, mimeType: string, name: string, documentTypeId: number | null}[]> {
    const localFiles = this.selectedFiles();
    const driveFiles = this.selectedGoogleDriveFiles();
    const totalFiles = localFiles.length + driveFiles.length;
    
    if (totalFiles === 0) {
      return [];
    }
    
    this.isUploadingToGCS.set(true);
    const uploadedDocs: {gcsPath: string, mimeType: string, name: string, documentTypeId: number | null}[] = [];
    
    try {
      let fileIndex = 0;
      
      // Process local files
      for (const fileWithType of localFiles) {
        fileIndex++;
        const file = fileWithType.file;
        this.uploadProgress.set(`Processing file ${fileIndex} of ${totalFiles}: ${file.name}...`);
        
        // Check if Office file needs conversion to PDF (backend only accepts PDF/images)
        let fileToUpload = file;
        if (this.googleDriveService.isMicrosoftOfficeFile(file.type)) {
          if (!this.googleDriveAuthAvailable) {
            throw new Error('Google Drive auth not available for Office file conversion');
          }
          
          try {
            this.uploadProgress.set(`Converting ${file.name} to PDF...`);
            const result = await firstValueFrom(this.googleDriveService.convertLocalOfficeFileToPdf(file));
            
            // Convert base64 to File
            const blob = this.base64ToBlob(result.data, result.mimeType);
            fileToUpload = new File([blob], result.name, { type: result.mimeType });
          } catch (conversionError: any) {
            console.error('Error converting Office file to PDF:', conversionError);
            throw new Error(`Failed to convert "${file.name}" to PDF. ${conversionError.message || 'Conversion failed'}`);
          }
        }
        
        // Upload to GCS via backend (only PDF/images accepted)
        this.uploadProgress.set(`Uploading ${fileToUpload.name} to cloud storage...`);
        const formData = new FormData();
        formData.append('File', fileToUpload);
        formData.append('Name', fileToUpload.name);
        formData.append('UploadToGCS', 'true');
        formData.append('SkipDatabaseSave', 'true'); // Don't save to database yet
        
        const response = await this.http
          .post<any>('/api/document/upload', formData)
          .toPromise();
        
        if (response && response.storagePath) {
          uploadedDocs.push({
            gcsPath: response.storagePath,
            mimeType: fileToUpload.type,
            name: fileToUpload.name,
            documentTypeId: fileWithType.documentTypeId
          });
        }
      }
      
      // Process Google Drive files
      for (const driveFileWithType of driveFiles) {
        fileIndex++;
        const driveFile = driveFileWithType;
        this.uploadProgress.set(`Processing file ${fileIndex} of ${totalFiles}: ${driveFile.name}...`);
        
        // Check if Drive file needs PDF conversion
        const needsConversion = this.googleDriveService.needsPdfConversion(driveFile.mimeType || '');
        
        if (needsConversion) {
          // Export Drive file as PDF
          if (!this.googleDriveAuthAvailable) {
            throw new Error('Google Drive auth not available for file conversion');
          }
          
          this.uploadProgress.set(`Exporting ${driveFile.name} from Drive as PDF...`);
          const result = await firstValueFrom(
            this.googleDriveService.exportDriveFileAsPdf(driveFile.id, driveFile.name || '')
          );
          
          // Convert base64 to File object
          const blob = this.base64ToBlob(result.data, result.mimeType);
          const pdfFile = new File([blob], result.name, { type: result.mimeType });
          
          // Upload PDF to GCS
          this.uploadProgress.set(`Uploading ${result.name} to cloud storage...`);
          const formData = new FormData();
          formData.append('File', pdfFile);
          formData.append('Name', result.name);
          formData.append('UploadToGCS', 'true');
          formData.append('SkipDatabaseSave', 'true'); // Don't save to database yet
          formData.append('GoogleId', driveFile.id); // Keep Google Drive ID
          
          const response = await this.http
            .post<any>('/api/document/upload', formData)
            .toPromise();
          
          if (response && response.storagePath) {
            uploadedDocs.push({
              gcsPath: response.storagePath,
              mimeType: pdfFile.type,
              name: result.name,
              documentTypeId: driveFileWithType.documentTypeId
            });
          }
        } else {
          // File is already PDF - download and upload to GCS to get storagePath
          this.uploadProgress.set(`Downloading ${driveFile.name} from Drive...`);
          
          try {
            // Download the PDF from Google Drive
            const downloadResult = await firstValueFrom(
              this.googleDriveService.downloadDriveFile(driveFile.id, driveFile.name, driveFile.mimeType)
            );
            
            // Convert base64 to File object
            const blob = this.base64ToBlob(downloadResult.data, downloadResult.mimeType);
            const pdfFile = new File([blob], downloadResult.name, { type: downloadResult.mimeType });
            
            // Upload to GCS via backend
            this.uploadProgress.set(`Uploading ${pdfFile.name} to cloud storage...`);
            const formData = new FormData();
            formData.append('File', pdfFile);
            formData.append('Name', pdfFile.name);
            formData.append('UploadToGCS', 'true');
            formData.append('SkipDatabaseSave', 'true'); // Don't save to database yet
            formData.append('GoogleId', driveFile.id); // Keep Google Drive ID
            
            const response = await this.http
              .post<any>('/api/document/upload', formData)
              
              .toPromise();
            
            if (response && response.storagePath) {
              uploadedDocs.push({
                gcsPath: response.storagePath,
                mimeType: pdfFile.type,
                name: pdfFile.name,
                documentTypeId: driveFileWithType.documentTypeId
              });
            }
          } catch (error: any) {
            console.error('Error uploading PDF from Drive:', error);
            throw new Error(`Failed to upload "${driveFile.name}": ${error.message || 'Unknown error'}`);
          }
        }
      }
      
      this.uploadProgress.set('');
      return uploadedDocs;
      
    } catch (error) {
      console.error('Error uploading files to GCS:', error);
      this.uploadProgress.set('');
      throw error;
    } finally {
      this.isUploadingToGCS.set(false);
    }
  }
  
  /**
   * Convert base64 to Blob
   */
  private base64ToBlob(base64: string, mimeType: string): Blob {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    return new Blob([byteArray], { type: mimeType });
  }

  /**
   * Get document type name by ID
   */
  getDocumentTypeName(documentTypeId: number | null): string {
    if (!documentTypeId) return 'Unknown';
    const docType = this.documentTypes().find(dt => dt.id === documentTypeId);
    return docType ? docType.name : 'Unknown';
  }

  /**
   * Generate AI proposal (Step 1 -> Step 2)
   */
  async generateProposal(): Promise<void> {
    // Validate
    if (!this.canGenerate()) {
      this.showValidationError.set(true);
      return;
    }

    this.showValidationError.set(false);
    this.generating.set(true);

    try {
      // Step 1: Upload files to GCS if any selected (local or Google Drive)
      let uploadedDocs: {gcsPath: string, mimeType: string, name: string, documentTypeId: number | null}[] = [];
      const localFilesCount = this.selectedFiles().length;
      const driveFilesCount = this.selectedGoogleDriveFiles().length;
      
      console.log('📝 [GenerateProposal] Document state before upload:', {
        localFiles: localFilesCount,
        driveFiles: driveFilesCount,
        existingDocumentIds: this.selectedExistingDocumentIds().length
      });
      
      if (localFilesCount > 0 || driveFilesCount > 0) {
        console.log('📤 [GenerateProposal] Starting document upload to GCS...');
        try {
          uploadedDocs = await this.uploadFilesToGCS();
          console.log('✅ [GenerateProposal] Upload complete:', {
            uploadedCount: uploadedDocs.length,
            documents: uploadedDocs.map(d => ({ name: d.name, gcsPath: d.gcsPath, documentTypeId: d.documentTypeId }))
          });
        } catch (uploadError: any) {
          console.error('❌ [GenerateProposal] Error uploading files:', uploadError);
          this.feedbackDialogService.showErrorToast({
            summary: this.translateService.instant('common.error.title'),
            detail: uploadError?.message || this.translateService.instant('message.error.uploadingDocuments')
          });
          this.generating.set(false);
          return;
        }
      } else {
        console.log('ℹ️ [GenerateProposal] No local or Drive files to upload');
      }
      
      // Step 2: Get selected interaction IDs
      const interactionIds = this.selectedInteractions()
        .filter(i => i.selected !== false)
        .map(i => i.id);
      
      console.log('📋 [GenerateProposal] Selected interactions:', interactionIds);
      
      // Step 3: Prepare request with all sources
      const request: ProposeOpportunityRequest = {
        opportunityName: this.opportunityName(),
        opportunityDescription: this.opportunityDescription(),
        partnerId: this.partnerId(),
        isFundingPartner: this.isFundingPartner(),
        isClientPartner: this.isClientPartner(),
        interactionIds: interactionIds.length > 0 ? interactionIds : undefined,
        newDocumentStoragePaths: uploadedDocs.length > 0 ? uploadedDocs.map(d => d.gcsPath) : undefined,
        newDocumentMimeTypes: uploadedDocs.length > 0 ? uploadedDocs.map(d => d.mimeType) : undefined,
        existingDocumentIds: this.selectedExistingDocumentIds().length > 0 ? this.selectedExistingDocumentIds() : undefined
      };
      
      console.log('📤 [GenerateProposal] Sending proposal request:', {
        opportunityName: request.opportunityName,
        interactions: request.interactionIds?.length || 0,
        newDocPaths: request.newDocumentStoragePaths?.length || 0,
        newDocPathsList: request.newDocumentStoragePaths || [],
        newDocMimeTypes: request.newDocumentMimeTypes || [],
        existingDocs: request.existingDocumentIds?.length || 0,
        partnerId: request.partnerId
      });

      // Step 4: Call backend API to generate AI proposal
      const rawResponse = await this.http
        .post<any>( // Receive as 'any' first to parse stringified fields
          `/api/opportunity/generate-proposal`,
          request
        )
        .toPromise();

      if (!rawResponse) {
        throw new Error('No response from server');
      }
      
      // Step 5: Parse stringified JSON fields from backend
      console.log('📥 Raw response from backend:', rawResponse);
      
      // Helper function to safely parse JSON strings
      const safeJsonParse = (value: any, fieldName: string): any => {
        if (!value) {
          return null;
        }
        
        // If it's already an object/array, return as-is
        if (typeof value === 'object') {
          return value;
        }
        
        // If it's a string, try to parse it
        if (typeof value === 'string') {
          const trimmed = value.trim();
          if (trimmed === '' || trimmed === '[]') {
            return null;
          }
          
          try {
            return JSON.parse(trimmed);
          } catch (error) {
            console.error(`❌ Error parsing ${fieldName}:`, error, 'Value:', value);
            return null;
          }
        }
        
        return null;
      };
      
      const parsedResponse: ProposedOpportunityResponse = {
        ...rawResponse,
        opportunity: {
          ...rawResponse.opportunity,
          fundingPartners: safeJsonParse(rawResponse.opportunity.fundingPartners, 'fundingPartners'),
          clientPartners: safeJsonParse(rawResponse.opportunity.clientPartners, 'clientPartners'),
          stakeholders: safeJsonParse(rawResponse.opportunity.stakeholders, 'stakeholders'),
          deliverables: safeJsonParse(rawResponse.opportunity.deliverables, 'deliverables'),
          countries: safeJsonParse(rawResponse.opportunity.countries, 'countries'),
          sdGs: safeJsonParse(rawResponse.opportunity.sdGs, 'sdGs'),
          dependents: safeJsonParse(rawResponse.opportunity.dependents, 'dependents')
        }
      };
      
      console.log('✅ Parsed response with typed collections:', parsedResponse);
      this.proposedOpportunity.set(parsedResponse);
      
      // Store uploaded documents for later use when creating opportunity
      this.uploadedDocuments.set(uploadedDocs);
      console.log('💾 [GenerateProposal] Stored uploaded documents for opportunity creation:', {
        count: uploadedDocs.length,
        documents: uploadedDocs.map(d => ({ name: d.name, hasGcsPath: !!d.gcsPath, documentTypeId: d.documentTypeId }))
      });
      
      // Initialize field selection - auto-select all fields
      this.initializeFieldSelection();
      
      this.currentStep.set('review');
      
      this.feedbackDialogService.showSuccessToast({
        summary: this.translateService.instant('common.success.title'),
        detail: this.translateService.instant('message.proposalGenerated')
      });

    } catch (error) {
      console.error('Error generating proposal:', error);
      this.feedbackDialogService.showErrorToast({
        summary: this.translateService.instant('common.error.title'),
        detail: this.translateService.instant('message.error.generatingProposal')
      });
    } finally {
      this.generating.set(false);
    }
  }

  /**
   * Cancel and close dialog
   */
  cancel(): void {
    this.reset();
    this.visible.set(false);
  }
  
  /**
   * Go back from review step to edit step
   */
  backToEdit(): void {
    this.currentStep.set('select');
  }
  
  /**
   * Create opportunity directly without AI generation
   * Used when user just wants to create with name and description
   * Documents will be uploaded and attached, but interactions will be ignored
   */
  async createDirectly(): Promise<void> {
    if (!this.canCreate()) {
      return;
    }
    
    // Check if user has selected interactions (warn they will be ignored)
    const hasInteractions = this.selectedInteractions().length > 0;
    
    // If interactions are selected, warn user they will be ignored (but documents will be included)
    if (hasInteractions) {
      this.feedbackDialogService.showConfirmDialog(
        {
          summary: this.translateService.instant('common.confirmation.title'),
          detail: this.translateService.instant('message.confirmation.createWithoutInteractions')
        },
        () => {
          // User confirmed - proceed with direct creation (documents will be uploaded and attached)
          this.performDirectCreation();
        }
      );
    } else {
      // No interactions selected - proceed directly (documents will be uploaded if selected)
      this.performDirectCreation();
    }
  }
  
  /**
   * Perform the actual direct creation (called after confirmation if needed)
   */
  private async performDirectCreation(): Promise<void> {
    this.generating.set(true);
    
    try {
      console.log('📤 Creating opportunity directly (without AI)');
      
      // Step 1: Upload files to GCS if any selected (local or Google Drive)
      let uploadedDocs: {gcsPath: string, mimeType: string, name: string, documentTypeId: number | null}[] = [];
      if (this.selectedFiles().length > 0 || this.selectedGoogleDriveFiles().length > 0) {
        try {
          uploadedDocs = await this.uploadFilesToGCS();
        } catch (uploadError) {
          console.error('Error uploading files:', uploadError);
          this.feedbackDialogService.showErrorToast({
            summary: this.translateService.instant('common.error.title'),
            detail: this.translateService.instant('message.error.uploadingDocuments')
          });
          this.generating.set(false);
          return;
        }
      }
      
      // Build create request with basic info and documents
      // Note: For direct creation from interaction detail view (mode: 'detail-view'),
      // we don't send partnerId as the user hasn't explicitly selected partner roles
      const isFromInteractionDetail = this.mode() === 'detail-view';
      
      const createRequest: any = {
        name: this.opportunityName(),
        description: this.opportunityDescription(),
        // Only include partnerId when in list-view mode (from partner context with role selection)
        partnerId: isFromInteractionDetail ? 0 : (this.partnerId() || 0),
        isFundingPartner: isFromInteractionDetail ? false : this.isFundingPartner(),
        isClientPartner: isFromInteractionDetail ? false : this.isClientPartner(),
        
        // Include uploaded documents as structured array
        documents: uploadedDocs.map(d => ({
          gcsPath: d.gcsPath,
          mimeType: d.mimeType,
          documentTypeId: d.documentTypeId
        }))
      };
      
      console.log('📤 Sending direct create request with {0} documents:', uploadedDocs.length, createRequest);
      
      // Call backend API to create opportunity
      const response = await firstValueFrom(
        this.http.post<any>('/api/opportunity/create-from-proposal', createRequest)
      );
      
      console.log('✅ Opportunity created:', response);
      
      this.feedbackDialogService.showSuccessToast({
        summary: this.translateService.instant('common.success.title'),
        detail: this.translateService.instant('message.opportunityCreated')
      });
      
      // Emit the created opportunity
      this.opportunityCreated.emit(response);
      
      // Close dialog and reset
      this.reset();
      this.visible.set(false);
      
    } catch (error: any) {
      console.error('❌ Error creating opportunity:', error);
      this.feedbackDialogService.showErrorToast({
        summary: this.translateService.instant('common.error.title'),
        detail: error?.error?.detail || this.translateService.instant('message.error.creatingOpportunity')
      });
    } finally {
      this.generating.set(false);
    }
  }

  /**
   * Create opportunity from reviewed proposal
   */
  async createOpportunity(): Promise<void> {
    const proposal = this.proposedOpportunity();
    if (!proposal || !proposal.opportunity) {
      return;
    }
    
    this.generating.set(true);
    
    try {
      console.log('📤 Creating opportunity from proposal:', proposal);
      console.log('📝 Selected fields:', Array.from(this.selectedFields().entries()).filter(([_, selected]) => selected).map(([field]) => field));
      
      // Check if uploaded documents are still available
      const storedDocs = this.uploadedDocuments();
      console.log('💾 [CreateOpportunity] Checking stored documents:', {
        count: storedDocs.length,
        documents: storedDocs.map(d => ({ name: d.name, hasGcsPath: !!d.gcsPath, documentTypeId: d.documentTypeId }))
      });
      
      if (storedDocs.length === 0) {
        console.warn('⚠️ [CreateOpportunity] No uploaded documents found in signal - documents may have been lost!');
      }
      
      // Build create request with only user-selected fields
      const createRequest: any = {
        // Required fields (always included)
        name: proposal.opportunity.name,
        description: proposal.opportunity.description,
        partnerId: this.partnerId() || 0,
        isFundingPartner: this.isFundingPartner(),
        isClientPartner: this.isClientPartner(),
        sourceInteractionIds: proposal.sourceInteractionIds || [],
        
        // Include uploaded documents as structured array
        documents: storedDocs.map(d => ({
          gcsPath: d.gcsPath,
          mimeType: d.mimeType,
          documentTypeId: d.documentTypeId
        }))
      };
      
      console.log('📤 [CreateOpportunity] Documents in create request:', {
        count: createRequest.documents.length,
        documents: createRequest.documents
      });
      
      // Add optional fields only if selected
      if (this.isFieldSelected('partnerReference') && proposal.opportunity.partnerReference) {
        createRequest.partnerReference = proposal.opportunity.partnerReference;
      }
      
      if (this.isFieldSelected('responsibleOrgUnitName') && proposal.opportunity.responsibleOrgUnitId) {
        createRequest.responsibleOrgUnitId = proposal.opportunity.responsibleOrgUnitId;
      }
      
      if (this.isFieldSelected('proposedInitiativeTypeName') && proposal.opportunity.proposedInitiativeTypeId) {
        createRequest.proposedInitiativeTypeId = proposal.opportunity.proposedInitiativeTypeId;
      }
      
      if (this.isFieldSelected('initiativeBudgetUSD') && proposal.opportunity.initiativeBudgetUSD) {
        createRequest.initiativeBudgetUSD = proposal.opportunity.initiativeBudgetUSD;
      }
      
      if (this.isFieldSelected('strategicAlignment') && proposal.opportunity.strategicAlignment) {
        createRequest.strategicAlignment = proposal.opportunity.strategicAlignment;
      }
      
      if (this.isFieldSelected('resultsFocus') && proposal.opportunity.resultsFocus) {
        createRequest.resultsFocus = proposal.opportunity.resultsFocus;
      }
      
      if (this.isFieldSelected('intendedImpactOutcomes') && proposal.opportunity.intendedImpactOutcomes) {
        createRequest.intendedImpactOutcomes = proposal.opportunity.intendedImpactOutcomes;
      }
      
      if (this.isFieldSelected('expectedBeneficiaries') && proposal.opportunity.expectedBeneficiaries) {
        createRequest.expectedBeneficiaries = proposal.opportunity.expectedBeneficiaries;
      }
      
      if (this.isFieldSelected('targetSigningDate') && proposal.opportunity.targetSigningDate) {
        createRequest.targetSigningDate = proposal.opportunity.targetSigningDate;
      }
      
      if (this.isFieldSelected('targetDeliveryDate') && proposal.opportunity.targetDeliveryDate) {
        createRequest.targetDeliveryDate = proposal.opportunity.targetDeliveryDate;
      }
      
      if (this.isFieldSelected('partnershipAgreementReference') && proposal.opportunity.partnershipAgreementReference) {
        createRequest.partnershipAgreementReference = proposal.opportunity.partnershipAgreementReference;
      }
      
      // Collection fields
      if (this.isFieldSelected('deliverables') && proposal.opportunity.deliverables && proposal.opportunity.deliverables.length > 0) {
        createRequest.deliverables = proposal.opportunity.deliverables;
      }
      
      if (this.isFieldSelected('sdGs') && proposal.opportunity.sdGs && proposal.opportunity.sdGs.length > 0) {
        // Map SDGs to just IDs (backend expects List<int>)
        createRequest.sdGs = proposal.opportunity.sdGs
          .map((sdg: any) => sdg.sdgId || sdg.id)
          .filter((id: number) => id != null);
      }
      
      // Handle partners based on user's role selections
      const fundingPartners: any[] = [];
      const clientPartners: any[] = [];
      
      for (const partner of this.allProposedPartners()) {
        const roleSelection = this.partnerRoleSelections().get(partner.partnerId);
        if (roleSelection && roleSelection.selected) {
          if (roleSelection.isFunding) {
            // Add to funding partners with full structure
            fundingPartners.push({
              partnerId: partner.partnerId,
              amount: partner.amount || null,
              percentage: partner.percentage || null,
              feePercentage: partner.feePercentage || null,
              feeAmount: partner.feeAmount || null,
              feeAmountUSD: partner.feeAmountUSD || null,
              isAmountBasedFee: partner.isAmountBasedFee || false,
              partnershipAgreementReference: partner.partnershipAgreementReference || null
            });
          }
          if (roleSelection.isClient) {
            // Add to client partners with proper structure
            clientPartners.push({
              partnerId: partner.partnerId
            });
          }
        }
      }
      
      if (fundingPartners.length > 0) {
        createRequest.fundingPartners = fundingPartners;
      }
      
      if (clientPartners.length > 0) {
        createRequest.clientPartners = clientPartners;
      }
      
      if (this.isFieldSelected('stakeholders') && proposal.opportunity.stakeholders && proposal.opportunity.stakeholders.length > 0) {
        createRequest.stakeholders = proposal.opportunity.stakeholders;
      }
      
      if (this.isFieldSelected('countries') && proposal.opportunity.countries && proposal.opportunity.countries.length > 0) {
        // Map countries to just IDs (backend expects List<int>)
        createRequest.countries = proposal.opportunity.countries
          .map((c: any) => c.country?.id || c.countryId || c.id)
          .filter((id: number) => id != null);
      }
      
      console.log('📤 Sending create request:', createRequest);
      
      // Call backend API to create opportunity
      const response = await firstValueFrom(
        this.http.post<any>('/api/opportunity/create-from-proposal', createRequest)
      );
      
      console.log('✅ Opportunity created:', response);
      
      this.feedbackDialogService.showSuccessToast({
        summary: this.translateService.instant('common.success.title'),
        detail: this.translateService.instant('message.opportunityCreated')
      });
      
      // Emit the created opportunity
      this.opportunityCreated.emit(response);
      
      // Reset and close
      this.reset();
      this.visible.set(false);
      
    } catch (error) {
      console.error('❌ Error creating opportunity:', error);
      this.feedbackDialogService.showErrorToast({
        summary: this.translateService.instant('common.error.title'),
        detail: this.translateService.instant('message.error.creatingOpportunity')
      });
    } finally {
      this.generating.set(false);
    }
  }

  /**
   * Toggle selection of a field in the proposal
   */
  toggleField(fieldPath: string): void {
    const current = this.selectedFields().get(fieldPath) || false;
    const updated = new Map(this.selectedFields());
    updated.set(fieldPath, !current);
    this.selectedFields.set(updated);
  }

  /**
   * Check if a field is selected
   */
  isFieldSelected(fieldPath: string): boolean {
    return this.selectedFields().get(fieldPath) || false;
  }

  /**
   * Select or deselect all proposal fields
   */
  toggleAllFields(): void {
    const selectAll = !this.allFieldsSelected();
    const updated = new Map(this.selectedFields());
    
    const proposal = this.proposedOpportunity();
    if (!proposal || !proposal.opportunity) return;
    
    const opp = proposal.opportunity;
    
    // Toggle all non-empty fields
    if (opp.name) updated.set('name', selectAll);
    if (opp.description) updated.set('description', selectAll);
    if (opp.responsibleOrgUnitName) updated.set('responsibleOrgUnitName', selectAll);
    if (opp.proposedInitiativeTypeName) updated.set('proposedInitiativeTypeName', selectAll);
    if (opp.initiativeBudgetUSD) updated.set('initiativeBudgetUSD', selectAll);
    if (opp.strategicAlignment) updated.set('strategicAlignment', selectAll);
    if (opp.resultsFocus) updated.set('resultsFocus', selectAll);
    if (opp.expectedBeneficiaries) updated.set('expectedBeneficiaries', selectAll);
    if (opp.intendedImpactOutcomes) updated.set('intendedImpactOutcomes', selectAll);
    if (opp.targetSigningDate) updated.set('targetSigningDate', selectAll);
    if (opp.targetDeliveryDate) updated.set('targetDeliveryDate', selectAll);
    if (opp.deliverables && opp.deliverables.length > 0) updated.set('deliverables', selectAll);
    if (opp.fundingPartners && opp.fundingPartners.length > 0) updated.set('fundingPartners', selectAll);
    if (opp.clientPartners && opp.clientPartners.length > 0) updated.set('clientPartners', selectAll);
    if (opp.stakeholders && opp.stakeholders.length > 0) updated.set('stakeholders', selectAll);
    if (opp.countries && opp.countries.length > 0) updated.set('countries', selectAll);
    if (opp.sdGs && opp.sdGs.length > 0) updated.set('sdGs', selectAll);
    
    this.selectedFields.set(updated);
  }
  
  /**
   * Initialize field selection when proposal is loaded - auto-select all fields
   */
  private initializeFieldSelection(): void {
    const proposal = this.proposedOpportunity();
    if (!proposal || !proposal.opportunity) return;
    
    const selected = new Map<string, boolean>();
    const opp = proposal.opportunity;
    
    // Auto-select all non-empty fields
    if (opp.name) selected.set('name', true);
    if (opp.description) selected.set('description', true);
    if (opp.responsibleOrgUnitName) selected.set('responsibleOrgUnitName', true);
    if (opp.proposedInitiativeTypeName) selected.set('proposedInitiativeTypeName', true);
    if (opp.initiativeBudgetUSD) selected.set('initiativeBudgetUSD', true);
    if (opp.strategicAlignment) selected.set('strategicAlignment', true);
    if (opp.resultsFocus) selected.set('resultsFocus', true);
    if (opp.expectedBeneficiaries) selected.set('expectedBeneficiaries', true);
    if (opp.intendedImpactOutcomes) selected.set('intendedImpactOutcomes', true);
    if (opp.targetSigningDate) selected.set('targetSigningDate', true);
    if (opp.targetDeliveryDate) selected.set('targetDeliveryDate', true);
    if (opp.deliverables && opp.deliverables.length > 0) selected.set('deliverables', true);
    if (opp.fundingPartners && opp.fundingPartners.length > 0) selected.set('fundingPartners', true);
    if (opp.clientPartners && opp.clientPartners.length > 0) selected.set('clientPartners', true);
    if (opp.stakeholders && opp.stakeholders.length > 0) selected.set('stakeholders', true);
    if (opp.countries && opp.countries.length > 0) selected.set('countries', true);
    if (opp.sdGs && opp.sdGs.length > 0) selected.set('sdGs', true);
    
    this.selectedFields.set(selected);
    
    // Initialize partner role selections
    this.initializePartnerRoleSelections();
  }
  
  /**
   * Initialize partner role selections from AI proposal
   */
  private initializePartnerRoleSelections(): void {
    const proposal = this.proposedOpportunity();
    if (!proposal || !proposal.opportunity) return;
    
    const roleMap = new Map<number, { isFunding: boolean, isClient: boolean, selected: boolean }>();
    
    // Mark funding partners
    if (proposal.opportunity.fundingPartners) {
      for (const fp of proposal.opportunity.fundingPartners) {
        if (!fp.partnerId) continue; // Skip if partnerId is undefined
        const existing = roleMap.get(fp.partnerId);
        roleMap.set(fp.partnerId, { 
          isFunding: true, 
          isClient: existing?.isClient || false,
          selected: true // Auto-select all partners by default
        });
      }
    }
    
    // Mark client partners
    if (proposal.opportunity.clientPartners) {
      for (const cp of proposal.opportunity.clientPartners) {
        if (!cp.partnerId) continue; // Skip if partnerId is undefined
        const existing = roleMap.get(cp.partnerId);
        roleMap.set(cp.partnerId, { 
          isFunding: existing?.isFunding || false, 
          isClient: true,
          selected: existing?.selected !== undefined ? existing.selected : true // Auto-select by default
        });
      }
    }
    
    this.partnerRoleSelections.set(roleMap);
  }
  
  /**
   * Toggle partner role (funding or client)
   */
  togglePartnerRole(partnerId: number, role: 'funding' | 'client'): void {
    const current = this.partnerRoleSelections().get(partnerId);
    if (!current) return;
    
    const updated = new Map(this.partnerRoleSelections());
    
    if (role === 'funding') {
      current.isFunding = !current.isFunding;
    } else {
      current.isClient = !current.isClient;
    }
    
    updated.set(partnerId, current);
    this.partnerRoleSelections.set(updated);
  }
  
  /**
   * Toggle partner selection checkbox
   */
  togglePartnerSelection(partnerId: number): void {
    const current = this.partnerRoleSelections().get(partnerId);
    if (!current) return;
    
    const updated = new Map(this.partnerRoleSelections());
    
    // Toggle selection
    current.selected = !current.selected;
    
    // If selecting, enable default roles based on what the partner was proposed as
    if (current.selected) {
      const partner = this.allProposedPartners().find(p => p.partnerId === partnerId);
      if (partner) {
        current.isFunding = partner.roles.includes('funding');
        current.isClient = partner.roles.includes('client');
      }
    }
    
    updated.set(partnerId, current);
    this.partnerRoleSelections.set(updated);
  }
  
  /**
   * Check if partner is selected
   */
  isPartnerSelected(partnerId: number): boolean {
    return this.partnerRoleSelections().get(partnerId)?.selected || false;
  }
  
  /**
   * Toggle all partners selection
   */
  toggleAllPartners(): void {
    const allPartners = this.allProposedPartners();
    const allSelected = allPartners.every(p => this.isPartnerSelected(p.partnerId));
    
    const updated = new Map(this.partnerRoleSelections());
    for (const partner of allPartners) {
      const current = updated.get(partner.partnerId);
      if (current) {
        // Toggle selection
        current.selected = !allSelected;
        
        // If selecting all, also enable their default roles
        if (!allSelected) {
          // Set funding/client roles based on what the partner was proposed as
          current.isFunding = partner.roles.includes('funding');
          current.isClient = partner.roles.includes('client');
        }
        
        updated.set(partner.partnerId, current);
      }
    }
    
    this.partnerRoleSelections.set(updated);
  }
  
  /**
   * Check if all partners are selected
   */
  areAllPartnersSelected(): boolean {
    const allPartners = this.allProposedPartners();
    if (allPartners.length === 0) return false;
    return allPartners.every(p => this.isPartnerSelected(p.partnerId));
  }

  /**
   * Reset dialog state
   */
  private reset(): void {
    this.currentStep.set('select');
    this.selectedInteractions.set([]);
    this.availableInteractions.set([]);
    this.showAdditionalSelection.set(false);
    this.searchQuery.set('');
    this.opportunityName.set('');
    this.opportunityDescription.set('');
    this.isFundingPartner.set(false);
    this.isClientPartner.set(false);
    this.showValidationError.set(false);
    this.generating.set(false);
    this.proposedOpportunity.set(null);
    this.selectedFields.set(new Map());
    
    // Clear document state
    this.selectedFiles.set([]);
    this.selectedGoogleDriveFiles.set([]);
    this.uploadedDocuments.set([]);
    this.isUploadingToGCS.set(false);
    this.uploadProgress.set('');
    this.selectedExistingDocumentIds.set([]);
    this.availablePartnerDocuments.set([]);
    this.showExistingDocuments.set(false);
  }

  /**
   * Get confidence level label
   */
  getConfidenceLevel(confidence: number): string {
    if (confidence >= 75) return 'high';
    if (confidence >= 50) return 'medium';
    return 'low';
  }

  /**
   * Get confidence severity for PrimeNG tag
   */
  getConfidenceSeverity(confidence: number): 'success' | 'warning' | 'danger' {
    if (confidence >= 75) return 'success';
    if (confidence >= 50) return 'warning';
    return 'danger';
  }
}

