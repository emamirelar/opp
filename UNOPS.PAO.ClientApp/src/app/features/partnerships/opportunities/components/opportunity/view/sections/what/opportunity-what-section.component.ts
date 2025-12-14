/**
 * @fileoverview Opportunity WHAT Section Component - Manages opportunity overview with edit capabilities
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, output, signal, computed, inject, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ChipModule } from 'primeng/chip';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';

// Services and Models
import { ValuesService, SimpleValue, OrganizationUnit, Output, OutputSemanticSearchMatch, OutputSemanticSearchResponse } from '@shared/services/api/values.service';
import { OpportunityService } from '../../../../../services/opportunity.service';
import { Opportunity, OpportunityDeliverable, FrameworkStatusResponse, ExtractedDeliverableInfo } from '@shared/models/opportunity.model';
import { FeedbackDialogService } from '@shared/services/ui';

/**
 * @interface TreeNode
 * @description Represents a node in the products/services tree hierarchy
 */
export interface TreeNode {
  /** Unique identifier for the node */
  id: string;
  /** Display label for the node */
  label: string;
  /** Hierarchy level (0-4) */
  level: number;
  /** Full path from root to this node */
  path: string[];
  /** Child nodes */
  children: TreeNode[];
  /** Whether this node has a selectable output */
  isSelectable: boolean;
  /** The output associated with this node (if selectable) */
  output?: Output;
  /** Number of total selectable items under this node */
  selectableCount: number;
  /** Definition/description for this level */
  definition?: string;
  /** Service line (if available) */
  serviceLine?: string;
  /** Whether node has procurement component */
  hasProcurementComponent?: boolean;
}

/**
 * @class OpportunityWhatSectionComponent
 * @description Manages the WHAT section of opportunity with independent edit/save/cancel functionality.
 * Communicates with parent via input signals and output events.
 * 
 * @example
 * ```html
 * <app-opportunity-what-section
 *   [opportunity]="opportunity()"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 *   (sectionSaved)="handleSectionSaved()"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-what-section',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    SelectModule,
    ChipModule,
    TooltipModule,
    DialogModule,
    InputTextModule,
  ],
  templateUrl: './opportunity-what-section.component.html',
  styleUrls: ['./opportunity-what-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityWhatSectionComponent implements OnInit {
  // Services
  private readonly valuesService = inject(ValuesService);
  private readonly opportunityService = inject(OpportunityService);
  private readonly translateService = inject(TranslateService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly cdr = inject(ChangeDetectorRef);

  // State tracking properties to prevent duplicate processing in effects
  private lastProcessedOpportunityId: number | undefined;
  private lastDocumentUploadTrigger = 0;
  private lastSectionSaveTrigger = 0;

  /**
   * @description Input signal for opportunity data from parent
   */
  readonly opportunity = input.required<Opportunity>();
  readonly suggestions = input<any[]>([]);
  
  /**
   * @description Input signal for update permission - controls visibility of edit button
   */
  readonly canUpdate = input<boolean>(false);

  /**
   * @description Input signal to trigger AI recommendations refresh when documents are uploaded
   * Parent should increment this value when documents are uploaded/linked/deleted
   */
  readonly documentUploadTrigger = input<number>(0);

  /**
   * @description Input signal to trigger framework status refresh when any section saves
   * Parent should increment this value after any section saves successfully
   */
  readonly sectionSaveTrigger = input<number>(0);

  /**
   * @description Output event when opportunity is updated - signals parent to refresh
   */
  readonly opportunityUpdated = output<Opportunity>();

  /**
   * @description Output event when section is saved - for cross-section refresh triggers
   */
  readonly sectionSaved = output<void>();

  /**
   * @description Output event when changes are detected (for unsaved changes tracking)
   */
  readonly changesDetected = output<void>();

  /**
   * @description Output event when changes are saved or discarded (clear unsaved state)
   */
  readonly changesSavedOrDiscarded = output<void>();

  // Edit mode state
  readonly isEditing = signal<boolean>(false);
  readonly isSaving = signal<boolean>(false);
  readonly hasUnsavedChangesSignal = signal<boolean>(false);
  private originalData: {
    responsibleOrgUnitId?: number;
    proposedInitiativeTypeId?: number;
    deliveryModality?: number | null;
    deliverables?: any[];
  } | null = null;

  // Form controls for WHAT section
  orgUnitControl = new FormControl<number | null>(null);
  initiativeTypeControl = new FormControl<number | null>(null);
  deliveryModalityControl = new FormControl<number | null>(null);
  
  // Delivery Modality options (values match backend enum: 1=NotYetKnown, 2=AllDirect, 3=AllGrantSupport, 4=Mixed)
  readonly deliveryModalityOptions = signal([
    { value: 1, label: 'label.deliveryModality.notYetKnown' },
    { value: 2, label: 'label.deliveryModality.allDirect' },
    { value: 3, label: 'label.deliveryModality.allGrantSupport' },
    { value: 4, label: 'label.deliveryModality.mixed' }
  ]);

  // Dropdown data
  organizationUnits = signal<OrganizationUnit[]>([]);
  initiativeTypes = signal<SimpleValue[]>([]);
  outputs = signal<Output[]>([]);

  // Framework status and extraction
  frameworkStatus = signal<FrameworkStatusResponse | null>(null);
  isCheckingFramework = signal<boolean>(false);
  isExtracting = signal<boolean>(false);
  hasRunExtraction = signal<boolean>(false); // Track if extraction has been triggered at least once
  extractedDeliverables = signal<ExtractedDeliverableInfo[]>([]);
  acceptedDeliverables = signal<ExtractedDeliverableInfo[]>([]);
  showFrameworkWarning = computed(() => {
    const status = this.frameworkStatus();
    return status && !status.hasTaggedFrameworks;
  });
  showFrameworkInfo = computed(() => {
    const status = this.frameworkStatus();
    return status && status.hasTaggedFrameworks;
  });
  // Computed signal for remaining extracted items (not yet accepted/dismissed)
  visibleExtractedDeliverables = computed(() => {
    const extracted = this.extractedDeliverables();
    const accepted = this.acceptedDeliverables();
    return extracted.filter(e => !accepted.some(a => a.partnerLanguage === e.partnerLanguage));
  });

  // Deliverables dialog
  showDeliverablesDialog = signal<boolean>(false);
  selectedOutput = signal<Output | null>(null);
  isEditingDeliverable = signal<boolean>(false);
  editingDeliverableIndex = signal<number | null>(null);

  // Search mode toggle (search-first vs browse vs AI-assisted mode)
  searchMode = signal<'search' | 'browse' | 'ai'>('search');
  
  // Search functionality
  searchQuery = signal<string>('');
  searchResults = signal<Output[]>([]);
  
  // Multi-selection support
  selectedOutputsForDialog = signal<Output[]>([]);
  
  // Context from rejected AI recommendation
  rejectedItemContext = signal<string | null>(null);
  
  // Tree view state
  treeData = signal<TreeNode[]>([]);
  expandedNodes = signal<Set<string>>(new Set());
  treeSearchQuery = signal<string>('');
  
  // AI Semantic Search state
  aiSearchQuery = signal<string>('');
  aiSearchResults = signal<OutputSemanticSearchMatch[]>([]);
  isAiSearching = signal<boolean>(false);
  aiSearchError = signal<string | null>(null);
  
  // Filtered tree based on search
  filteredTreeData = computed(() => {
    const query = this.treeSearchQuery().toLowerCase().trim();
    const tree = this.treeData();
    
    if (!query || query.length < 2) {
      return tree;
    }
    
    return this.filterTreeBySearch(tree, query);
  });

  /**
   * Computed signal to detect if procurement expert is required
   * @description Checks if any deliverables have ProcurementComponent flag
   * and ServiceLine is NOT "Procurement"
   */
  readonly requiresProcurementExpert = computed(() => {
    const deliverables = this.opportunity()?.deliverables;
    if (!deliverables || deliverables.length === 0) {
      return false;
    }
    
    return deliverables.some(d => 
      d.procurementComponent === true && 
      d.serviceLine !== 'Procurement'
    );
  });

  /**
   * Get list of deliverables that require procurement expert
   */
  readonly deliverablesRequiringProcurement = computed(() => {
    const deliverables = this.opportunity()?.deliverables;
    if (!deliverables) {
      return [];
    }
    
    return deliverables.filter(d => 
      d.procurementComponent === true && 
      d.serviceLine !== 'Procurement'
    );
  });

  // Dynamic cascading dropdown data (Level 0-4)
  level0Options = signal<string[]>([]);
  level1Options = signal<string[]>([]);
  level2Options = signal<string[]>([]);
  level3Options = signal<string[]>([]);
  level4Options = signal<string[]>([]);
  filteredOutputs = signal<Output[]>([]);

  // Dynamic cascading dropdown form controls
  level0Control = new FormControl<string | null>(null);
  level1Control = new FormControl<string | null>(null);
  level2Control = new FormControl<string | null>(null);
  level3Control = new FormControl<string | null>(null);
  level4Control = new FormControl<string | null>(null);
  outputControl = new FormControl<Output | null>(null);

  // Computed properties
  readonly deliverableCount = computed(() => this.opportunity().deliverables?.length || 0);
  readonly selectedOutputDetails = computed(() => {
    const output = this.outputControl.value;
    if (!output) return null;
    return {
      level0: output.level0,
      level1: output.level1,
      definitionLevel1: output.definitionLevel1,
      level2: output.level2,
      definitionLevel2: output.definitionLevel2,
      level3: output.level3,
      definitionLevel3: output.definitionLevel3,
      level4: output.level4,
      definitionLevel4: output.definitionLevel4,
      serviceLine: output.serviceLine,
      // Include flags
      procurementComponent: output.procurementComponent,
      grantSupportComponent: output.grantSupportComponent,
      infrastructureComponent: output.infrastructureComponent
    };
  });
  
  /**
   * Check if selected output requires procurement expert
   * @description Returns true if output has Procurement Component flag AND service line is NOT "Procurement"
   */
  readonly selectedOutputRequiresProcurement = computed(() => {
    const output = this.outputControl.value;
    if (!output) return false;
    
    // Check if procurement component is flagged
    const hasProcurementFlag = output.procurementComponent === true;
    
    // Check if service line is NOT "Procurement"
    const isNotProcurementService = output.serviceLine?.toLowerCase() !== 'procurement';
    
    return hasProcurementFlag && isNotProcurementService;
  });

  // Check if any level is selected (for breadcrumb display)
  readonly hasAnyLevelSelected = computed(() => {
    return !!(
      this.level0Control.value ||
      this.level1Control.value ||
      this.level2Control.value ||
      this.level3Control.value ||
      this.level4Control.value
    );
  });

  constructor() {
    // Effect must be in constructor (injection context)
    // IMPORTANT: Only trigger initial load when opportunity ID changes
    // to prevent infinite loops caused by signal updates
    effect(() => {
      const opp = this.opportunity();
      if (opp && opp.id) {
        // Only process if this is a NEW opportunity ID (prevents infinite loops)
        if (this.lastProcessedOpportunityId !== opp.id) {
          this.lastProcessedOpportunityId = opp.id;
          
          // Use setTimeout to avoid calling during signal computation
          setTimeout(() => {
            // Initial load: check framework status for the new opportunity
            this.checkFrameworkStatus();
            
            // Auto-load AI recommendations (only on first load)
            if (!this.hasRunExtraction()) {
              this.extractProductsAndServices();
            }
          }, 0);
        }
      }
    });
    
    // Effect to refresh framework status and AI recommendations when documents are uploaded
    effect(() => {
      const trigger = this.documentUploadTrigger();
      // Only refresh if trigger actually increased (prevents initial/redundant runs)
      if (trigger > this.lastDocumentUploadTrigger) {
        this.lastDocumentUploadTrigger = trigger;
        // Use setTimeout to avoid calling during signal computation
        setTimeout(() => {
          // Refresh framework status (document might be tagged as framework)
          this.checkFrameworkStatus();
          // Refresh AI recommendations from documents
          if (this.hasRunExtraction()) {
            this.refreshAiRecommendations();
          }
        }, 0);
      }
    });
    
    // Effect to refresh framework status when any section saves
    // This handles cases like WHO section tagging documents to partners
    effect(() => {
      const trigger = this.sectionSaveTrigger();
      // Only refresh if trigger actually increased (prevents initial/redundant runs)
      if (trigger > this.lastSectionSaveTrigger) {
        this.lastSectionSaveTrigger = trigger;
        // Use setTimeout to avoid calling during signal computation
        setTimeout(() => {
          // Refresh framework status after section saves
          this.checkFrameworkStatus();
        }, 0);
      }
    });
    
    // Set up change detection on form controls
    // Only mark as changed if we're in edit mode (to avoid triggering on initial setValue)
    this.orgUnitControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.initiativeTypeControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
    this.deliveryModalityControl.valueChanges.subscribe(() => {
      if (this.isEditing()) {
        this.markAsChanged();
      }
    });
  }

  ngOnInit(): void {
    // Load dropdown data on initialization
    this.loadDropdownData();
  }

  /**
   * @description Load dropdown data for form fields
   */
  private loadDropdownData(): void {
    this.valuesService.getOrganizationUnits().subscribe({
      next: (data) => {
        this.organizationUnits.set(data);
        this.cdr.detectChanges();
      }
    });

    this.valuesService.getProposedInitiativeTypes().subscribe({
      next: (data) => {
        this.initiativeTypes.set(data);
        this.cdr.detectChanges();
      }
    });

    this.valuesService.getOutputs().subscribe({
      next: (data) => {
        this.outputs.set(data);
        
        // Initialize Level0 dropdown with all distinct values
        const level0Values = this.valuesService.getDistinctLevel0(data);
        this.level0Options.set(level0Values);
        
        // Initialize all level options (to show all by default)
        this.level1Options.set(this.valuesService.getDistinctLevel1(data, ''));
        this.level2Options.set(this.valuesService.getDistinctLevel2(data));
        this.level3Options.set(this.valuesService.getDistinctLevel3(data));
        this.level4Options.set(this.valuesService.getDistinctLevel4(data));
        
        // Show all outputs by default
        this.filteredOutputs.set(data);
        
        // Build tree structure for browse mode
        this.buildTreeFromOutputs(data);
        
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * @description Build hierarchical tree structure from flat outputs
   */
  private buildTreeFromOutputs(outputs: Output[]): void {
    const tree: TreeNode[] = [];
    const nodeMap = new Map<string, TreeNode>();

    // Sort outputs for consistent ordering
    const sortedOutputs = [...outputs].sort((a, b) => {
      const pathA = [a.level0, a.level1, a.level2, a.level3, a.level4].filter(Boolean).join(' > ');
      const pathB = [b.level0, b.level1, b.level2, b.level3, b.level4].filter(Boolean).join(' > ');
      return pathA.localeCompare(pathB);
    });

    for (const output of sortedOutputs) {
      const levels = [output.level0, output.level1, output.level2, output.level3, output.level4];
      const definitions = [null, output.definitionLevel1, output.definitionLevel2, output.definitionLevel3, output.definitionLevel4];
      
      let parentNode: TreeNode | null = null;
      const currentPath: string[] = [];

      for (let i = 0; i < levels.length; i++) {
        const levelValue = levels[i];
        if (!levelValue) break;

        currentPath.push(levelValue);
        const nodeKey = currentPath.join('|||');
        
        // Check if this exact path represents a terminal node (selectable output)
        const isTerminalAtThisLevel = this.isOutputTerminalAtLevel(output, i);

        if (!nodeMap.has(nodeKey)) {
          const newNode: TreeNode = {
            id: nodeKey,
            label: levelValue,
            level: i,
            path: [...currentPath],
            children: [],
            isSelectable: false,
            selectableCount: 0,
            definition: definitions[i] || undefined,
            serviceLine: output.serviceLine
          };
          nodeMap.set(nodeKey, newNode);

          if (parentNode) {
            parentNode.children.push(newNode);
          } else {
            tree.push(newNode);
          }
        }

        const currentNode = nodeMap.get(nodeKey)!;
        
        // Mark as selectable if this output terminates at this level
        if (isTerminalAtThisLevel) {
          currentNode.isSelectable = true;
          currentNode.output = output;
          currentNode.serviceLine = output.serviceLine;
          currentNode.hasProcurementComponent = output.procurementComponent === true;
        }

        parentNode = currentNode;
      }
    }

    // Calculate selectable counts for each node (recursive)
    this.calculateSelectableCounts(tree);
    
    this.treeData.set(tree);
  }

  /**
   * @description Check if an output terminates at a specific level
   */
  private isOutputTerminalAtLevel(output: Output, level: number): boolean {
    const levels = [output.level0, output.level1, output.level2, output.level3, output.level4];
    
    // Terminal if current level has a value and next level is empty/undefined
    if (!levels[level]) return false;
    if (level === 4) return true; // Level 4 is always terminal if it exists
    return !levels[level + 1];
  }

  /**
   * @description Calculate total selectable items under each node
   */
  private calculateSelectableCounts(nodes: TreeNode[]): number {
    let total = 0;
    for (const node of nodes) {
      const childCount = this.calculateSelectableCounts(node.children);
      node.selectableCount = (node.isSelectable ? 1 : 0) + childCount;
      total += node.selectableCount;
    }
    return total;
  }

  /**
   * @description Filter tree nodes by search query
   */
  private filterTreeBySearch(nodes: TreeNode[], query: string): TreeNode[] {
    const result: TreeNode[] = [];

    for (const node of nodes) {
      const labelMatches = node.label.toLowerCase().includes(query);
      const definitionMatches = node.definition?.toLowerCase().includes(query);
      const serviceLineMatches = node.serviceLine?.toLowerCase().includes(query);
      const nodeMatches = labelMatches || definitionMatches || serviceLineMatches;

      // Recursively filter children
      const filteredChildren = this.filterTreeBySearch(node.children, query);

      // Include node if it matches or has matching children
      if (nodeMatches || filteredChildren.length > 0) {
        result.push({
          ...node,
          children: filteredChildren.length > 0 ? filteredChildren : node.children
        });
      }
    }

    return result;
  }

  /**
   * @description Toggle node expansion state
   */
  toggleNodeExpansion(nodeId: string): void {
    const expanded = new Set(this.expandedNodes());
    if (expanded.has(nodeId)) {
      expanded.delete(nodeId);
    } else {
      expanded.add(nodeId);
    }
    this.expandedNodes.set(expanded);
    this.cdr.detectChanges();
  }

  /**
   * @description Check if a node is expanded
   */
  isNodeExpanded(nodeId: string): boolean {
    return this.expandedNodes().has(nodeId);
  }

  /**
   * @description Expand all nodes in tree
   */
  expandAllNodes(): void {
    const allNodeIds = new Set<string>();
    const collectNodeIds = (nodes: TreeNode[]) => {
      for (const node of nodes) {
        if (node.children.length > 0) {
          allNodeIds.add(node.id);
          collectNodeIds(node.children);
        }
      }
    };
    collectNodeIds(this.treeData());
    this.expandedNodes.set(allNodeIds);
    this.cdr.detectChanges();
  }

  /**
   * @description Collapse all nodes in tree
   */
  collapseAllNodes(): void {
    this.expandedNodes.set(new Set());
    this.cdr.detectChanges();
  }

  /**
   * @description Select a node from the tree (add to selection)
   */
  selectTreeNode(node: TreeNode): void {
    if (!node.isSelectable || !node.output) {
      return;
    }

    // Check if already selected
    if (this.selectedOutputsForDialog().some(o => o.id === node.output!.id)) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.outputAlreadySelected')
      });
      return;
    }

    // Add to selection
    this.selectedOutputsForDialog.set([...this.selectedOutputsForDialog(), node.output]);
    this.cdr.detectChanges();
  }

  /**
   * @description Check if a tree node's output is already selected
   */
  isTreeNodeSelected(node: TreeNode): boolean {
    if (!node.output) return false;
    return this.selectedOutputsForDialog().some(o => o.id === node.output!.id);
  }

  /**
   * @description Get the parent path string for display
   */
  getParentPath(node: TreeNode): string {
    if (node.path.length <= 1) return '';
    return node.path.slice(0, -1).join(' > ');
  }

  /**
   * @description Clear tree search
   */
  clearTreeSearch(): void {
    this.treeSearchQuery.set('');
    this.cdr.detectChanges();
  }

  /**
   * @description Handle tree search input
   */
  onTreeSearchInput(query: string): void {
    this.treeSearchQuery.set(query);
    
    // Auto-expand matching nodes when searching
    if (query.length >= 2) {
      this.expandMatchingNodes(query.toLowerCase());
    }
    
    this.cdr.detectChanges();
  }

  /**
   * @description Expand nodes that match the search query
   */
  private expandMatchingNodes(query: string): void {
    const nodesToExpand = new Set<string>();
    
    const findMatchingPaths = (nodes: TreeNode[], parentIds: string[] = []) => {
      for (const node of nodes) {
        const currentPath = [...parentIds, node.id];
        const matches = node.label.toLowerCase().includes(query) ||
                       node.definition?.toLowerCase().includes(query) ||
                       node.serviceLine?.toLowerCase().includes(query);
        
        if (matches) {
          // Expand all parent nodes
          parentIds.forEach(id => nodesToExpand.add(id));
        }
        
        if (node.children.length > 0) {
          findMatchingPaths(node.children, currentPath);
        }
      }
    };
    
    findMatchingPaths(this.treeData());
    
    // Merge with existing expanded nodes
    const expanded = new Set(this.expandedNodes());
    nodesToExpand.forEach(id => expanded.add(id));
    this.expandedNodes.set(expanded);
  }

  /**
   * @description Get level color class for visual distinction
   */
  getLevelColorClass(level: number): string {
    const colors = [
      'bg-blue-100 text-blue-700 border-blue-200',      // Level 0
      'bg-purple-100 text-purple-700 border-purple-200', // Level 1
      'bg-pink-100 text-pink-700 border-pink-200',       // Level 2
      'bg-orange-100 text-orange-700 border-orange-200', // Level 3
      'bg-green-100 text-green-700 border-green-200'     // Level 4
    ];
    return colors[level] || colors[0];
  }

  /**
   * @description Get level badge color for chips
   */
  getLevelBadgeStyle(level: number): { [key: string]: string } {
    const styles = [
      { 'background-color': '#dbeafe', 'color': '#1e40af' },  // Level 0 - Blue
      { 'background-color': '#ede9fe', 'color': '#5b21b6' },  // Level 1 - Purple
      { 'background-color': '#fce7f3', 'color': '#9d174d' },  // Level 2 - Pink
      { 'background-color': '#ffedd5', 'color': '#9a3412' },  // Level 3 - Orange
      { 'background-color': '#d1fae5', 'color': '#065f46' }   // Level 4 - Green
    ];
    return styles[level] || styles[0];
  }

  /**
   * @description Handle Level0 selection - filters Level1 and subsequent levels
   */
  onLevel0Change(level0: string | null): void {
    if (!level0) {
      // Reset to show all
      const allOutputs = this.outputs();
      this.level1Options.set(this.valuesService.getDistinctLevel1(allOutputs, ''));
      this.level2Options.set(this.valuesService.getDistinctLevel2(allOutputs));
      this.level3Options.set(this.valuesService.getDistinctLevel3(allOutputs));
      this.level4Options.set(this.valuesService.getDistinctLevel4(allOutputs));
      this.filteredOutputs.set(allOutputs);
      this.cdr.detectChanges();
      return;
    }

    const allOutputs = this.outputs();
    
    // Filter Level1 options
    const level1Values = this.valuesService.getDistinctLevel1(allOutputs, level0);
    this.level1Options.set(level1Values);
    
    // Clear subsequent levels if they're no longer valid
    const currentLevel1 = this.level1Control.value;
    if (currentLevel1 && !level1Values.includes(currentLevel1)) {
      this.level1Control.setValue(null);
      this.level2Control.setValue(null);
      this.level3Control.setValue(null);
      this.level4Control.setValue(null);
    }
    
    // Update filtered outputs
    this.updateFilteredOutputs();
    this.cdr.detectChanges();
  }

  /**
   * @description Handle Level1 selection - filters Level2 and subsequent levels
   */
  onLevel1Change(level1: string | null): void {
    const level0 = this.level0Control.value || undefined;
    const allOutputs = this.outputs();
    
    if (!level1) {
      // Reset Level2+ based on Level0 only
      this.level2Options.set(this.valuesService.getDistinctLevel2(allOutputs, level0));
      this.level3Options.set(this.valuesService.getDistinctLevel3(allOutputs, level0));
      this.level4Options.set(this.valuesService.getDistinctLevel4(allOutputs, level0));
      this.updateFilteredOutputs();
      this.cdr.detectChanges();
      return;
    }
    
    // Filter Level2 options
    const level2Values = this.valuesService.getDistinctLevel2(allOutputs, level0, level1);
    this.level2Options.set(level2Values);
    
    // Clear subsequent levels if they're no longer valid
    const currentLevel2 = this.level2Control.value;
    if (currentLevel2 && !level2Values.includes(currentLevel2)) {
      this.level2Control.setValue(null);
      this.level3Control.setValue(null);
      this.level4Control.setValue(null);
    }
    
    // Update filtered outputs
    this.updateFilteredOutputs();
    this.cdr.detectChanges();
  }

  /**
   * @description Handle Level2 selection - filters Level3 and Level4
   */
  onLevel2Change(level2: string | null): void {
    const level0 = this.level0Control.value || undefined;
    const level1 = this.level1Control.value || undefined;
    const allOutputs = this.outputs();
    
    if (!level2) {
      // Reset Level3+ based on Level0 and Level1
      this.level3Options.set(this.valuesService.getDistinctLevel3(allOutputs, level0, level1));
      this.level4Options.set(this.valuesService.getDistinctLevel4(allOutputs, level0, level1));
      this.updateFilteredOutputs();
      this.cdr.detectChanges();
      return;
    }
    
    // Filter Level3 options
    const level3Values = this.valuesService.getDistinctLevel3(allOutputs, level0, level1, level2);
    this.level3Options.set(level3Values);
    
    // Clear subsequent levels if they're no longer valid
    const currentLevel3 = this.level3Control.value;
    if (currentLevel3 && !level3Values.includes(currentLevel3)) {
      this.level3Control.setValue(null);
      this.level4Control.setValue(null);
    }
    
    // Update filtered outputs
    this.updateFilteredOutputs();
    this.cdr.detectChanges();
  }

  /**
   * @description Handle Level3 selection - filters Level4
   */
  onLevel3Change(level3: string | null): void {
    const level0 = this.level0Control.value || undefined;
    const level1 = this.level1Control.value || undefined;
    const level2 = this.level2Control.value || undefined;
    const allOutputs = this.outputs();
    
    if (!level3) {
      // Reset Level4 based on previous levels
      this.level4Options.set(this.valuesService.getDistinctLevel4(allOutputs, level0, level1, level2));
      this.updateFilteredOutputs();
      this.cdr.detectChanges();
      return;
    }
    
    // Filter Level4 options
    const level4Values = this.valuesService.getDistinctLevel4(allOutputs, level0, level1, level2, level3);
    this.level4Options.set(level4Values);
    
    // Clear Level4 if it's no longer valid
    const currentLevel4 = this.level4Control.value;
    if (currentLevel4 && !level4Values.includes(currentLevel4)) {
      this.level4Control.setValue(null);
    }
    
    // Update filtered outputs
    this.updateFilteredOutputs();
    this.cdr.detectChanges();
  }

  /**
   * @description Handle Level4 selection
   */
  onLevel4Change(level4: string | null): void {
    this.updateFilteredOutputs();
    this.cdr.detectChanges();
  }

  /**
   * @description Update filtered outputs based on all selected levels
   */
  private updateFilteredOutputs(): void {
    const allOutputs = this.outputs();
    const filtered = this.valuesService.getFilteredOutputsByLevels(
      allOutputs,
      this.level0Control.value || undefined,
      this.level1Control.value || undefined,
      this.level2Control.value || undefined,
      this.level3Control.value || undefined,
      this.level4Control.value || undefined
    );
    this.filteredOutputs.set(filtered);
    
    // Clear output selection if it's no longer in filtered list
    const currentOutput = this.outputControl.value;
    if (currentOutput && !filtered.some(o => o.id === currentOutput.id)) {
      this.outputControl.setValue(null);
    }
  }

  /**
   * @description Handle output selection - auto-fills levels
   */
  onOutputChange(output: Output | null): void {
    if (!output) {
      this.outputControl.setValue(null);
      this.cdr.detectChanges();
      return;
    }
    
    // Explicitly set the control value to ensure it's updated
    this.outputControl.setValue(output, { emitEvent: false });
    
    // Auto-fill all levels based on the selected output
    if (output.level0 && this.level0Control.value !== output.level0) {
      this.level0Control.setValue(output.level0, { emitEvent: false });
    }
    if (output.level1 && this.level1Control.value !== output.level1) {
      this.level1Control.setValue(output.level1, { emitEvent: false });
    }
    if (output.level2 && this.level2Control.value !== output.level2) {
      this.level2Control.setValue(output.level2, { emitEvent: false });
    }
    if (output.level3 && this.level3Control.value !== output.level3) {
      this.level3Control.setValue(output.level3, { emitEvent: false });
    }
    if (output.level4 && this.level4Control.value !== output.level4) {
      this.level4Control.setValue(output.level4, { emitEvent: false });
    }
    
    // Force change detection to update the template
    this.cdr.detectChanges();
  }

  /**
   * @description Check if user can select at Level 0 (is it a leaf node?)
   */
  canSelectAtLevel0(): boolean {
    const level0 = this.level0Control.value;
    if (!level0) return false;

    // Check if there's an output with ONLY Level0 (no Level1)
    return this.outputs().some(
      (o) => o.level0 === level0 && !o.level1 && !o.level2 && !o.level3 && !o.level4
    );
  }

  /**
   * @description Check if user can select at Level 1
   */
  canSelectAtLevel1(): boolean {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    if (!level0 || !level1) return false;

    // Check if there's an output with Level0+Level1 but no Level2
    return this.outputs().some(
      (o) => o.level0 === level0 && o.level1 === level1 && !o.level2 && !o.level3 && !o.level4
    );
  }

  /**
   * @description Check if user can select at Level 2
   */
  canSelectAtLevel2(): boolean {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    if (!level0 || !level1 || !level2) return false;

    return this.outputs().some(
      (o) =>
        o.level0 === level0 && o.level1 === level1 && o.level2 === level2 && !o.level3 && !o.level4
    );
  }

  /**
   * @description Check if user can select at Level 3
   */
  canSelectAtLevel3(): boolean {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    const level3 = this.level3Control.value;
    if (!level0 || !level1 || !level2 || !level3) return false;

    return this.outputs().some(
      (o) =>
        o.level0 === level0 &&
        o.level1 === level1 &&
        o.level2 === level2 &&
        o.level3 === level3 &&
        !o.level4
    );
  }

  /**
   * @description Check if user can select at Level 4
   */
  canSelectAtLevel4(): boolean {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    const level3 = this.level3Control.value;
    const level4 = this.level4Control.value;
    if (!level0 || !level1 || !level2 || !level3 || !level4) return false;

    return this.outputs().some(
      (o) =>
        o.level0 === level0 &&
        o.level1 === level1 &&
        o.level2 === level2 &&
        o.level3 === level3 &&
        o.level4 === level4
    );
  }

  /**
   * Check if a level0 value has child levels
   * @description Determines if there are more specific levels below Level 0
   */
  hasChildLevelsForLevel0(level0: string): boolean {
    return this.outputs().some(
      (o) => o.level0 === level0 && !!o.level1
    );
  }

  /**
   * Check if a level1 value has child levels
   * @description Determines if there are more specific levels below Level 1
   */
  hasChildLevelsForLevel1(level1: string): boolean {
    const level0 = this.level0Control.value;
    if (!level0) return false;
    return this.outputs().some(
      (o) => o.level0 === level0 && o.level1 === level1 && !!o.level2
    );
  }

  /**
   * Check if a level2 value has child levels
   * @description Determines if there are more specific levels below Level 2
   */
  hasChildLevelsForLevel2(level2: string): boolean {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    if (!level0 || !level1) return false;
    return this.outputs().some(
      (o) => o.level0 === level0 && o.level1 === level1 && o.level2 === level2 && !!o.level3
    );
  }

  /**
   * Check if a level3 value has child levels
   * @description Determines if there are more specific levels below Level 3
   */
  hasChildLevelsForLevel3(level3: string): boolean {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    if (!level0 || !level1 || !level2) return false;
    return this.outputs().some(
      (o) => o.level0 === level0 && o.level1 === level1 && o.level2 === level2 && o.level3 === level3 && !!o.level4
    );
  }

  /**
   * @description Select output at Level 0
   */
  selectAtLevel0(): void {
    const level0 = this.level0Control.value;
    if (!level0) return;

    const output = this.outputs().find(
      (o) => o.level0 === level0 && !o.level1 && !o.level2 && !o.level3 && !o.level4
    );

    if (output) {
      // Check if already selected
      if (this.isOutputSelected(output)) {
        this.feedbackService.showWarningToast({
          summary: this.translateService.instant('message.warning'),
          detail: this.translateService.instant('message.validation.outputAlreadySelected')
        });
        return;
      }

      // Add to selection (same as search mode)
      this.selectedOutputsForDialog.set([...this.selectedOutputsForDialog(), output]);
      
      // Clear the form for next selection
      this.level0Control.setValue(null);
      this.level1Control.setValue(null);
      this.level2Control.setValue(null);
      this.level3Control.setValue(null);
      this.level4Control.setValue(null);
      this.outputControl.setValue(null);
      
      this.cdr.detectChanges();
    }
  }

  /**
   * @description Select output at Level 1
   */
  selectAtLevel1(): void {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    if (!level0 || !level1) return;

    const output = this.outputs().find(
      (o) => o.level0 === level0 && o.level1 === level1 && !o.level2 && !o.level3 && !o.level4
    );

    if (output) {
      // Check if already selected
      if (this.isOutputSelected(output)) {
        this.feedbackService.showWarningToast({
          summary: this.translateService.instant('message.warning'),
          detail: this.translateService.instant('message.validation.outputAlreadySelected')
        });
        return;
      }

      // Add to selection (same as search mode)
      this.selectedOutputsForDialog.set([...this.selectedOutputsForDialog(), output]);
      
      // Clear the form for next selection
      this.level0Control.setValue(null);
      this.level1Control.setValue(null);
      this.level2Control.setValue(null);
      this.level3Control.setValue(null);
      this.level4Control.setValue(null);
      this.outputControl.setValue(null);
      
      this.cdr.detectChanges();
    }
  }

  /**
   * @description Select output at Level 2
   */
  selectAtLevel2(): void {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    if (!level0 || !level1 || !level2) return;

    const output = this.outputs().find(
      (o) =>
        o.level0 === level0 && o.level1 === level1 && o.level2 === level2 && !o.level3 && !o.level4
    );

    if (output) {
      // Check if already selected
      if (this.isOutputSelected(output)) {
        this.feedbackService.showWarningToast({
          summary: this.translateService.instant('message.warning'),
          detail: this.translateService.instant('message.validation.outputAlreadySelected')
        });
        return;
      }

      // Add to selection (same as search mode)
      this.selectedOutputsForDialog.set([...this.selectedOutputsForDialog(), output]);
      
      // Clear the form for next selection
      this.level0Control.setValue(null);
      this.level1Control.setValue(null);
      this.level2Control.setValue(null);
      this.level3Control.setValue(null);
      this.level4Control.setValue(null);
      this.outputControl.setValue(null);
      
      this.cdr.detectChanges();
    }
  }

  /**
   * @description Select output at Level 3
   */
  selectAtLevel3(): void {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    const level3 = this.level3Control.value;
    if (!level0 || !level1 || !level2 || !level3) return;

    const output = this.outputs().find(
      (o) =>
        o.level0 === level0 &&
        o.level1 === level1 &&
        o.level2 === level2 &&
        o.level3 === level3 &&
        !o.level4
    );

    if (output) {
      // Check if already selected
      if (this.isOutputSelected(output)) {
        this.feedbackService.showWarningToast({
          summary: this.translateService.instant('message.warning'),
          detail: this.translateService.instant('message.validation.outputAlreadySelected')
        });
        return;
      }

      // Add to selection (same as search mode)
      this.selectedOutputsForDialog.set([...this.selectedOutputsForDialog(), output]);
      
      // Clear the form for next selection
      this.level0Control.setValue(null);
      this.level1Control.setValue(null);
      this.level2Control.setValue(null);
      this.level3Control.setValue(null);
      this.level4Control.setValue(null);
      this.outputControl.setValue(null);
      
      this.cdr.detectChanges();
    }
  }

  /**
   * @description Select output at Level 4
   */
  selectAtLevel4(): void {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    const level3 = this.level3Control.value;
    const level4 = this.level4Control.value;
    if (!level0 || !level1 || !level2 || !level3 || !level4) return;

    const output = this.outputs().find(
      (o) =>
        o.level0 === level0 &&
        o.level1 === level1 &&
        o.level2 === level2 &&
        o.level3 === level3 &&
        o.level4 === level4
    );

    if (output) {
      // Check if already selected
      if (this.isOutputSelected(output)) {
        this.feedbackService.showWarningToast({
          summary: this.translateService.instant('message.warning'),
          detail: this.translateService.instant('message.validation.outputAlreadySelected')
        });
        return;
      }

      // Add to selection (same as search mode)
      this.selectedOutputsForDialog.set([...this.selectedOutputsForDialog(), output]);
      
      // Clear the form for next selection
      this.level0Control.setValue(null);
      this.level1Control.setValue(null);
      this.level2Control.setValue(null);
      this.level3Control.setValue(null);
      this.level4Control.setValue(null);
      this.outputControl.setValue(null);
      
      this.cdr.detectChanges();
    }
  }

  /**
   * @description Get definition for Level 1
   */
  getDefinitionForLevel1(): string | null {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    if (!level0 || !level1) return null;

    const output = this.outputs().find((o) => o.level0 === level0 && o.level1 === level1);
    return output?.definitionLevel1 || null;
  }

  /**
   * @description Get definition for Level 2
   */
  getDefinitionForLevel2(): string | null {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    if (!level0 || !level1 || !level2) return null;

    const output = this.outputs().find(
      (o) => o.level0 === level0 && o.level1 === level1 && o.level2 === level2
    );
    return output?.definitionLevel2 || null;
  }

  /**
   * @description Get definition for Level 3
   */
  getDefinitionForLevel3(): string | null {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    const level3 = this.level3Control.value;
    if (!level0 || !level1 || !level2 || !level3) return null;

    const output = this.outputs().find(
      (o) =>
        o.level0 === level0 && o.level1 === level1 && o.level2 === level2 && o.level3 === level3
    );
    return output?.definitionLevel3 || null;
  }

  /**
   * @description Get definition for Level 4
   */
  getDefinitionForLevel4(): string | null {
    const level0 = this.level0Control.value;
    const level1 = this.level1Control.value;
    const level2 = this.level2Control.value;
    const level3 = this.level3Control.value;
    const level4 = this.level4Control.value;
    if (!level0 || !level1 || !level2 || !level3 || !level4) return null;

    const output = this.outputs().find(
      (o) =>
        o.level0 === level0 &&
        o.level1 === level1 &&
        o.level2 === level2 &&
        o.level3 === level3 &&
        o.level4 === level4
    );
    return output?.definitionLevel4 || null;
  }

  /**
   * @description Get hierarchical path for display
   */
  getHierarchicalPath(output: Output): string {
    const parts: string[] = [];
    if (output.level0) parts.push(output.level0);
    if (output.level1) parts.push(output.level1);
    if (output.level2) parts.push(output.level2);
    if (output.level3) parts.push(output.level3);
    if (output.level4) parts.push(output.level4);
    return parts.join(' > ');
  }

  /**
   * @description Get the deepest level name for an output
   */
  getDeepestLevel(output: Output): string {
    if (output.level4) return output.level4;
    if (output.level3) return output.level3;
    if (output.level2) return output.level2;
    if (output.level1) return output.level1;
    if (output.level0) return output.level0;
    return output.name || '';
  }

  /**
   * @description Get level depth (0-4) for an output
   */
  getLevelDepth(output: Output): number {
    if (output.level4) return 4;
    if (output.level3) return 3;
    if (output.level2) return 2;
    if (output.level1) return 1;
    if (output.level0) return 0;
    return 0;
  }

  /**
   * @description Get level label for display
   */
  getLevelLabel(depth: number): string {
    const labels = [
      'label.serviceCategory',
      'label.primaryService',
      'label.specificService',
      'label.detailedService',
      'label.mostSpecific'
    ];
    return this.translateService.instant(labels[depth] || labels[0]);
  }

  /**
   * @description Perform unified search across all levels
   */
  performUnifiedSearch(query: string): void {
    this.searchQuery.set(query);
    
    if (!query || query.trim().length < 2) {
      this.searchResults.set([]);
      return;
    }

    const lowerQuery = query.toLowerCase().trim();
    const allOutputs = this.outputs();
    
    // Search across all fields
    const results = allOutputs.filter(output => {
      const searchableText = [
        output.name,
        output.level0,
        output.level1,
        output.level2,
        output.level3,
        output.level4,
        output.serviceLine,
        output.definitionLevel1,
        output.definitionLevel2,
        output.definitionLevel3,
        output.definitionLevel4
      ]
        .filter(field => field)
        .map(field => field!.toLowerCase())
        .join(' ');
      
      return searchableText.includes(lowerQuery);
    });

    // Sort by relevance (exact matches first, then by level depth)
    results.sort((a, b) => {
      const aDeepest = this.getDeepestLevel(a).toLowerCase();
      const bDeepest = this.getDeepestLevel(b).toLowerCase();
      const aExact = aDeepest === lowerQuery ? 1 : 0;
      const bExact = bDeepest === lowerQuery ? 1 : 0;
      
      if (aExact !== bExact) return bExact - aExact;
      
      // Prefer deeper (more specific) levels
      return this.getLevelDepth(b) - this.getLevelDepth(a);
    });

    this.searchResults.set(results);
    this.cdr.detectChanges();
  }

  /**
   * @description Group search results by level depth
   */
  getGroupedSearchResults(): Map<number, Output[]> {
    const grouped = new Map<number, Output[]>();
    
    this.searchResults().forEach(output => {
      const depth = this.getLevelDepth(output);
      if (!grouped.has(depth)) {
        grouped.set(depth, []);
      }
      grouped.get(depth)!.push(output);
    });
    
    return grouped;
  }

  /**
   * @description Toggle output selection from unified search (multi-select)
   */
  toggleOutputSelection(output: Output): void {
    const currentSelections = this.selectedOutputsForDialog();
    const index = currentSelections.findIndex(o => o.id === output.id);
    
    if (index >= 0) {
      // Already selected, remove it
      const updated = currentSelections.filter(o => o.id !== output.id);
      this.selectedOutputsForDialog.set(updated);
    } else {
      // Not selected, add it
      this.selectedOutputsForDialog.set([...currentSelections, output]);
    }
    
    this.cdr.detectChanges();
  }

  /**
   * @description Check if output is selected
   */
  isOutputSelected(output: Output): boolean {
    return this.selectedOutputsForDialog().some(o => o.id === output.id);
  }

  /**
   * @description Clear all selected outputs
   */
  clearSelectedOutputs(): void {
    this.selectedOutputsForDialog.set([]);
    this.cdr.detectChanges();
  }

  /**
   * @description Select output from unified search (deprecated - use toggleOutputSelection)
   */
  selectFromUnifiedSearch(output: Output): void {
    // Toggle selection instead of single select
    this.toggleOutputSelection(output);
  }

  /**
   * @description Toggle between search modes (search -> ai -> browse -> search)
   */
  toggleSearchMode(): void {
    const currentMode = this.searchMode();
    let newMode: 'search' | 'browse' | 'ai';
    
    if (currentMode === 'search') {
      newMode = 'ai';
    } else if (currentMode === 'ai') {
      newMode = 'browse';
    } else {
      newMode = 'search';
    }
    
    this.searchMode.set(newMode);
    
    // Clear search state when switching modes
    if (newMode === 'browse') {
      this.searchQuery.set('');
      this.searchResults.set([]);
    } else if (newMode === 'search') {
      this.aiSearchQuery.set('');
      this.aiSearchResults.set([]);
      this.aiSearchError.set(null);
    } else if (newMode === 'ai') {
      this.searchQuery.set('');
      this.searchResults.set([]);
    }
    
    this.cdr.detectChanges();
  }

  /**
   * @description Set search mode directly
   */
  setSearchMode(mode: 'search' | 'browse' | 'ai'): void {
    this.searchMode.set(mode);
    this.cdr.detectChanges();
  }

  /**
   * @description Perform AI semantic search for Products & Services
   * User enters text in their own words and gets AI-matched results
   */
  performAiSemanticSearch(): void {
    const query = this.aiSearchQuery().trim();
    
    if (!query || query.length < 3) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.aiSearchMinimum3Chars')
      });
      return;
    }

    this.isAiSearching.set(true);
    this.aiSearchError.set(null);
    this.aiSearchResults.set([]);
    
    this.valuesService.semanticSearchOutputs({
      searchText: query,
      maxResults: 10,
      minSimilarity: 0.3
    }).subscribe({
      next: (response: OutputSemanticSearchResponse) => {
        this.isAiSearching.set(false);
        this.aiSearchResults.set(response.matches);
        
        if (response.matches.length === 0) {
          this.feedbackService.showInfoToast({
            summary: this.translateService.instant('message.info'),
            detail: this.translateService.instant('message.noAiMatchesFound'),
            life: 5000
          });
        }
        
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isAiSearching.set(false);
        this.aiSearchError.set(error?.message || this.translateService.instant('message.error.aiSearchFailed'));
        console.error('AI semantic search error:', error);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * @description Select an output from AI search results
   */
  selectFromAiSearch(match: OutputSemanticSearchMatch): void {
    const output = match.output;
    
    // Check if already selected
    if (this.selectedOutputsForDialog().some(o => o.id === output.id)) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.outputAlreadySelected')
      });
      return;
    }

    // Add to selection
    this.selectedOutputsForDialog.set([...this.selectedOutputsForDialog(), output]);
    this.cdr.detectChanges();
  }

  /**
   * @description Check if an AI search result is already selected
   */
  isAiResultSelected(match: OutputSemanticSearchMatch): boolean {
    return this.selectedOutputsForDialog().some(o => o.id === match.output.id);
  }

  /**
   * @description Get confidence level label based on score
   */
  getConfidenceLevel(score: number): string {
    if (score >= 0.8) return this.translateService.instant('label.confidence.high');
    if (score >= 0.5) return this.translateService.instant('label.confidence.medium');
    return this.translateService.instant('label.confidence.low');
  }

  /**
   * @description Get confidence level CSS class based on score
   */
  getConfidenceClass(score: number): string {
    if (score >= 0.8) return 'bg-green-100 text-green-700';
    if (score >= 0.5) return 'bg-yellow-100 text-yellow-700';
    return 'bg-orange-100 text-orange-700';
  }

  /**
   * @description Clear AI search results and query
   */
  clearAiSearch(): void {
    this.aiSearchQuery.set('');
    this.aiSearchResults.set([]);
    this.aiSearchError.set(null);
    this.cdr.detectChanges();
  }

  /**
   * @description Check if output has child levels
   */
  hasChildLevels(output: Output): boolean {
    const depth = this.getLevelDepth(output);
    
    // Check if there are more specific outputs with the same parent path
    const allOutputs = this.outputs();
    
    return allOutputs.some(o => {
      if (depth === 0 && output.level0) {
        return o.level0 === output.level0 && !!o.level1;
      } else if (depth === 1 && output.level0 && output.level1) {
        return o.level0 === output.level0 && o.level1 === output.level1 && !!o.level2;
      } else if (depth === 2 && output.level0 && output.level1 && output.level2) {
        return o.level0 === output.level0 && o.level1 === output.level1 && o.level2 === output.level2 && !!o.level3;
      } else if (depth === 3 && output.level0 && output.level1 && output.level2 && output.level3) {
        return o.level0 === output.level0 && o.level1 === output.level1 && o.level2 === output.level2 && o.level3 === output.level3 && !!o.level4;
      }
      return false;
    });
  }

  /**
   * @description Handle quick search selection
   */
  selectFromQuickSearch(output: Output | null): void {
    if (!output) return;

    // Auto-populate all levels
    if (output.level0) {
      this.level0Control.setValue(output.level0, { emitEvent: false });
      this.level1Options.set(this.valuesService.getDistinctLevel1(this.outputs(), output.level0));
    }
    if (output.level1) {
      this.level1Control.setValue(output.level1, { emitEvent: false });
      this.level2Options.set(
        this.valuesService.getDistinctLevel2(this.outputs(), output.level0, output.level1)
      );
    }
    if (output.level2) {
      this.level2Control.setValue(output.level2, { emitEvent: false });
      this.level3Options.set(
        this.valuesService.getDistinctLevel3(
          this.outputs(),
          output.level0,
          output.level1,
          output.level2
        )
      );
    }
    if (output.level3) {
      this.level3Control.setValue(output.level3, { emitEvent: false });
      this.level4Options.set(
        this.valuesService.getDistinctLevel4(
          this.outputs(),
          output.level0,
          output.level1,
          output.level2,
          output.level3
        )
      );
    }
    if (output.level4) {
      this.level4Control.setValue(output.level4, { emitEvent: false });
    }

    // Set the final output
    this.outputControl.setValue(output);
    this.cdr.detectChanges();
  }

  /**
   * Check if Partner Results Framework documents are tagged
   * @description Checks the status of Partner Results Framework documents for this opportunity
   */
  checkFrameworkStatus(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    this.isCheckingFramework.set(true);
    this.opportunityService.getFrameworkStatus(opp.id).subscribe({
      next: (status) => {
        this.frameworkStatus.set(status);
        this.isCheckingFramework.set(false);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error checking framework status:', error);
        this.isCheckingFramework.set(false);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * Extract products and services from documents
   * @description Triggers AI extraction of deliverables from documents, prioritizing tagged frameworks
   */
  extractProductsAndServices(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    this.isExtracting.set(true);

    this.opportunityService.extractProductsAndServices(opp.id).subscribe({
      next: (extracted) => {
        this.extractedDeliverables.set(extracted);
        this.acceptedDeliverables.set([]); // Reset accepted list
        this.isExtracting.set(false);
        this.hasRunExtraction.set(true); // Mark that extraction has been run
        
        // Note: No toast notifications - recommendations load silently in the background
        
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error extracting products and services:', error);
        this.isExtracting.set(false);
        
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error.extractionFailed'),
          detail: error?.error?.detail || error?.message || this.translateService.instant('message.error.extractionFailed'),
          life: 5000
        });
        
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * Refresh AI recommendations
   * @description Re-triggers AI extraction of deliverables from documents (user-initiated refresh)
   */
  refreshAiRecommendations(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    this.isExtracting.set(true);
    this.hasRunExtraction.set(false); // Show loading state

    this.opportunityService.extractProductsAndServices(opp.id).subscribe({
      next: (extracted) => {
        this.extractedDeliverables.set(extracted);
        this.acceptedDeliverables.set([]); // Reset accepted list
        this.isExtracting.set(false);
        this.hasRunExtraction.set(true);
        
        this.feedbackService.showSuccessToast({
          summary: this.translateService.instant('message.success'),
          detail: this.translateService.instant('message.recommendationsRefreshed'),
          life: 3000
        });
        
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error refreshing AI recommendations:', error);
        this.isExtracting.set(false);
        this.hasRunExtraction.set(true); // Restore state to allow retry
        
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error.extractionFailed'),
          detail: error?.error?.detail || error?.message || this.translateService.instant('message.error.extractionFailed'),
          life: 5000
        });
        
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * @description Enter edit mode for this section
   */
  startEditing(): void {
    const opp = this.opportunity();
    
    // Backup original data for cancel
    this.originalData = {
      responsibleOrgUnitId: opp.responsibleOrgUnitId ?? undefined,
      proposedInitiativeTypeId: opp.proposedInitiativeTypeId ?? undefined,
      deliveryModality: opp.deliveryModality ?? null,
      deliverables: opp.deliverables ? [...opp.deliverables] : []
    };

    // Set form controls
    this.orgUnitControl.setValue(opp.responsibleOrgUnitId ?? null);
    this.initiativeTypeControl.setValue(opp.proposedInitiativeTypeId ?? null);
    this.deliveryModalityControl.setValue(opp.deliveryModality ?? null);

    this.isEditing.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Mark section as having unsaved changes
   * @private
   */
  private markAsChanged(): void {
    if (!this.hasUnsavedChangesSignal()) {
      this.hasUnsavedChangesSignal.set(true);
      this.changesDetected.emit();
    }
  }

  /**
   * @description Save section changes
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    const whatData = {
      responsibleOrgUnitId: this.orgUnitControl.value ?? undefined,
      proposedInitiativeTypeId: this.initiativeTypeControl.value ?? undefined,
      deliveryModality: this.deliveryModalityControl.value ?? undefined,
      deliverables: opp.deliverables // Include modified deliverables
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityWhat(opp.id, whatData).subscribe({
      next: (fullUpdatedOpportunity) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.originalData = null;
        this.hasUnsavedChangesSignal.set(false);
        
        // Emit full updated opportunity to parent
        this.opportunityUpdated.emit(fullUpdatedOpportunity);
        
        // Emit that section was saved (for potential cross-section updates)
        this.sectionSaved.emit();
        
        // Clear unsaved changes tracking
        this.changesSavedOrDiscarded.emit();
        
        this.feedbackService.showSuccessToast({
          detail: this.translateService.instant('message.opportunity.updatedSuccessfully'),
          summary: this.translateService.instant('message.success')
        });
        this.cdr.detectChanges();
      },
      error: () => {
        this.isSaving.set(false);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * @description Cancel editing and revert changes
   */
  cancelEditing(): void {
    const opp = this.opportunity();
    
    // Restore original data if available
    if (this.originalData) {
      // Reset form controls to original values
      this.orgUnitControl.setValue(this.originalData.responsibleOrgUnitId ?? null);
      this.initiativeTypeControl.setValue(this.originalData.proposedInitiativeTypeId ?? null);
      this.deliveryModalityControl.setValue(this.originalData.deliveryModality ?? null);
      
      // Restore original deliverables (reverts any AI recommendations that were accepted)
      const updatedOpportunity = {
        ...opp,
        responsibleOrgUnitId: this.originalData.responsibleOrgUnitId ?? null,
        proposedInitiativeTypeId: this.originalData.proposedInitiativeTypeId ?? null,
        deliveryModality: this.originalData.deliveryModality ?? null,
        deliverables: this.originalData.deliverables ? [...this.originalData.deliverables] : []
      };
      
      // Emit the reverted opportunity to parent
      this.opportunityUpdated.emit(updatedOpportunity);
    } else {
      // Fallback: just reset form controls to current opportunity values
      this.orgUnitControl.setValue(opp.responsibleOrgUnitId ?? null);
      this.initiativeTypeControl.setValue(opp.proposedInitiativeTypeId ?? null);
      this.deliveryModalityControl.setValue(opp.deliveryModality ?? null);
    }
    
    // Clear accepted recommendations tracking (they're being discarded)
    this.acceptedDeliverables.set([]);
    
    this.isEditing.set(false);
    this.originalData = null;
    this.hasUnsavedChangesSignal.set(false);
    
    // Clear unsaved changes tracking
    this.changesSavedOrDiscarded.emit();
    
    this.cdr.detectChanges();
  }
  
  /**
   * Get translated label for delivery modality value
   * @description Returns the translated label for a delivery modality option
   */
  getDeliveryModalityLabel(value: number): string {
    const option = this.deliveryModalityOptions().find(o => o.value === value);
    if (option) {
      return this.translateService.instant(option.label);
    }
    return this.translateService.instant('label.deliveryModality.notYetKnown');
  }

  /**
   * @description Open deliverables dialog for adding new deliverable
   */
  openDeliverablesDialog(): void {
    // Reset edit mode
    this.isEditingDeliverable.set(false);
    this.editingDeliverableIndex.set(null);
    
    // Clear multi-selection
    this.selectedOutputsForDialog.set([]);
    
    // Reset form controls
    this.level0Control.setValue(null);
    this.level1Control.setValue(null);
    this.level2Control.setValue(null);
    this.level3Control.setValue(null);
    this.level4Control.setValue(null);
    this.outputControl.setValue(null);
    
    // Initialize all dropdowns with all data
    const allOutputs = this.outputs();
    this.level0Options.set(this.valuesService.getDistinctLevel0(allOutputs));
    this.level1Options.set(this.valuesService.getDistinctLevel1(allOutputs, ''));
    this.level2Options.set(this.valuesService.getDistinctLevel2(allOutputs));
    this.level3Options.set(this.valuesService.getDistinctLevel3(allOutputs));
    this.level4Options.set(this.valuesService.getDistinctLevel4(allOutputs));
    this.filteredOutputs.set(allOutputs);
    
    this.showDeliverablesDialog.set(true);
  }
  
  /**
   * @description Close deliverables dialog and clear rejected context
   */
  closeDeliverablesDialog(): void {
    this.showDeliverablesDialog.set(false);
    this.rejectedItemContext.set(null);
    this.searchQuery.set('');
    this.searchResults.set([]);
    this.selectedOutputsForDialog.set([]);
  }

  /**
   * @description Add to selection from browse mode
   */
  addOutputToSelection(): void {
    const output = this.outputControl.value;
    if (!output) return;

    // Check if already selected
    if (this.isOutputSelected(output)) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.outputAlreadySelected')
      });
      return;
    }

    // Add to selection
    this.selectedOutputsForDialog.set([...this.selectedOutputsForDialog(), output]);
    
    // Clear the form for next selection
    this.level0Control.setValue(null);
    this.level1Control.setValue(null);
    this.level2Control.setValue(null);
    this.level3Control.setValue(null);
    this.level4Control.setValue(null);
    this.outputControl.setValue(null);
    
    this.cdr.detectChanges();
  }

  /**
   * @description Remove output from selection
   */
  removeFromSelection(output: Output): void {
    const updated = this.selectedOutputsForDialog().filter(o => o.id !== output.id);
    this.selectedOutputsForDialog.set(updated);
    this.cdr.detectChanges();
  }

  /**
   * @description Add all selected deliverables
   */
  addDeliverable(): void {
    const opp = this.opportunity();
    if (!opp) return;

    // Use selected outputs from dialog
    const outputsToAdd = this.selectedOutputsForDialog();
    
    if (outputsToAdd.length === 0) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.noOutputsSelected')
      });
      return;
    }

    // Check for duplicates
    const duplicates = outputsToAdd.filter(output => 
      opp.deliverables?.some(d => d.outputId === output.id)
    );

    if (duplicates.length > 0) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.someOutputsAlreadyAdded', { count: duplicates.length })
      });
      return;
    }

    // Create deliverable objects for all selected outputs
    const newDeliverables: OpportunityDeliverable[] = outputsToAdd.map(output => ({
      id: 0,
      opportunityId: opp.id,
      outputId: output.id ?? null,
      outputName: output.name ?? null,
      level0: output.level0 ?? null,
      level1: output.level1 ?? null,
      definitionLevel1: output.definitionLevel1 ?? null,
      level2: output.level2 ?? null,
      definitionLevel2: output.definitionLevel2 ?? null,
      level3: output.level3 ?? null,
      definitionLevel3: output.definitionLevel3 ?? null,
      level4: output.level4 ?? null,
      definitionLevel4: output.definitionLevel4 ?? null,
      serviceLine: output.serviceLine ?? null,
      grantSupportImplementingModality: output.grantSupportImplementingModality ?? null,
      grantSupportComponent: output.grantSupportComponent ?? null,
      procurementComponent: output.procurementComponent ?? null,
      procurementInstallationComponent: output.procurementInstallationComponent ?? null,
      infrastructureComponent: output.infrastructureComponent ?? null,
      sequenceOrder: null,
      plannedStartDate: null,
      plannedEndDate: null,
      quantity: null,
      notes: null
    }));

    // Add all new deliverables
    const currentDeliverables = opp.deliverables || [];
    const updatedDeliverables = [...currentDeliverables, ...newDeliverables];

    // Update opportunity with modified deliverables
    const updatedOpportunity: Opportunity = {
      ...opp,
      deliverables: updatedDeliverables
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Mark as changed (deliverables added)
    this.markAsChanged();

    // Show success message
    this.feedbackService.showSuccessToast({
      summary: this.translateService.instant('message.success'),
      detail: this.translateService.instant('message.productsServicesAdded', { count: outputsToAdd.length })
    });

    // Reset dialog state
    this.showDeliverablesDialog.set(false);
    this.selectedOutputsForDialog.set([]);
    this.level0Control.setValue(null);
    this.level1Control.setValue(null);
    this.level2Control.setValue(null);
    this.level3Control.setValue(null);
    this.level4Control.setValue(null);
    this.outputControl.setValue(null);
    this.cdr.detectChanges();
  }

  /**
   * @description Edit existing deliverable
   */
  editDeliverable(index: number): void {
    const opp = this.opportunity();
    if (!opp || !opp.deliverables || !opp.deliverables[index]) return;

    const deliverable = opp.deliverables[index];
    const allOutputs = this.outputs();

    // Set edit mode
    this.isEditingDeliverable.set(true);
    this.editingDeliverableIndex.set(index);

    // Find the output to pre-fill the form
    const matchedOutput = allOutputs.find(o => o.id === deliverable.outputId);
    
    if (matchedOutput) {
      // Pre-fill cascading dropdowns with all level values
      if (matchedOutput.level0) {
        this.level0Control.setValue(matchedOutput.level0);
        this.level1Options.set(this.valuesService.getDistinctLevel1(allOutputs, matchedOutput.level0));
      }

      if (matchedOutput.level1) {
        this.level1Control.setValue(matchedOutput.level1);
        this.level2Options.set(this.valuesService.getDistinctLevel2(allOutputs, matchedOutput.level0, matchedOutput.level1));
      }

      if (matchedOutput.level2) {
        this.level2Control.setValue(matchedOutput.level2);
        this.level3Options.set(this.valuesService.getDistinctLevel3(allOutputs, matchedOutput.level0, matchedOutput.level1, matchedOutput.level2));
      }

      if (matchedOutput.level3) {
        this.level3Control.setValue(matchedOutput.level3);
        this.level4Options.set(this.valuesService.getDistinctLevel4(allOutputs, matchedOutput.level0, matchedOutput.level1, matchedOutput.level2, matchedOutput.level3));
      }

      if (matchedOutput.level4) {
        this.level4Control.setValue(matchedOutput.level4);
      }

      // Update filtered outputs
      const filtered = this.valuesService.getFilteredOutputsByLevels(
        allOutputs,
        matchedOutput.level0,
        matchedOutput.level1,
        matchedOutput.level2,
        matchedOutput.level3,
        matchedOutput.level4
      );
      this.filteredOutputs.set(filtered);

      this.outputControl.setValue(matchedOutput);
    }

    this.showDeliverablesDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Remove deliverable at index
   */
  removeDeliverable(index: number): void {
    const opp = this.opportunity();
    if (!opp || !opp.deliverables) return;

    const updatedDeliverables = opp.deliverables.filter((_, i) => i !== index);
    const updatedOpportunity = {
      ...opp,
      deliverables: updatedDeliverables
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);
    
    // Mark as changed (deliverable removed)
    this.markAsChanged();
    
    this.cdr.detectChanges();
  }

  /**
   * Reject AI match and open manual search for alternative
   * @description Allows user to reject the AI-suggested match and search using AI semantic search
   * @note Option 2: Can find different match WITHOUT edit mode (auto-enters edit mode)
   */
  findDifferentMatch(item: ExtractedDeliverableInfo): void {
    // If not in edit mode, enter it first (Option 2: seamless acceptance)
    if (!this.isEditing()) {
      this.startEditing();
    }
    
    // Store partner language for context display
    this.rejectedItemContext.set(item.partnerLanguage);
    
    // Remove from extracted list (user is rejecting this recommendation)
    const currentExtracted = this.extractedDeliverables();
    const filtered = currentExtracted.filter(e => e.partnerLanguage !== item.partnerLanguage);
    this.extractedDeliverables.set(filtered);
    
    // Open deliverables dialog
    this.openDeliverablesDialog();
    
    // Put the partner language text into the AI search input and trigger search
    this.aiSearchQuery.set(item.partnerLanguage);
    
    // Trigger AI semantic search after a short delay to allow dialog to open
    setTimeout(() => {
      this.performAiSemanticSearch();
    }, 100);
    
    this.cdr.detectChanges();
  }

  /**
   * @description Accept extracted deliverable and add it to the opportunity
   * @description Moves an extracted item from recommendations to accepted list
   * @note Option 2: Can accept recommendations WITHOUT edit mode
   */
  acceptExtractedDeliverable(item: ExtractedDeliverableInfo, index: number): void {
    if (!item.matchedOutputId) {
      return;
    }
    
    // If not in edit mode, enter it first (Option 2: seamless acceptance)
    if (!this.isEditing()) {
      this.startEditing();
    }

    const opp = this.opportunity();
    if (!opp) return;

    // Check for duplicate
    const isDuplicate = opp.deliverables?.some(d => d.outputId === item.matchedOutputId);
    if (isDuplicate) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.outputAlreadyAdded')
      });
      return;
    }

    // Find the full Output details from the outputs list
    const matchedOutput = this.outputs().find(o => o.id === item.matchedOutputId);
    if (!matchedOutput) {
      this.feedbackService.showErrorToast({
        summary: this.translateService.instant('message.error'),
        detail: this.translateService.instant('message.outputNotFound')
      });
      return;
    }

    // Create deliverable object with all Output fields (same as addDeliverable method)
    const newDeliverable: OpportunityDeliverable = {
      id: 0,
      opportunityId: opp.id,
      outputId: matchedOutput.id ?? null,
      outputName: matchedOutput.name ?? null,
      level0: matchedOutput.level0 ?? null,
      level1: matchedOutput.level1 ?? null,
      definitionLevel1: matchedOutput.definitionLevel1 ?? null,
      level2: matchedOutput.level2 ?? null,
      definitionLevel2: matchedOutput.definitionLevel2 ?? null,
      level3: matchedOutput.level3 ?? null,
      definitionLevel3: matchedOutput.definitionLevel3 ?? null,
      level4: matchedOutput.level4 ?? null,
      definitionLevel4: matchedOutput.definitionLevel4 ?? null,
      serviceLine: matchedOutput.serviceLine ?? null,
      grantSupportImplementingModality: matchedOutput.grantSupportImplementingModality ?? null,
      grantSupportComponent: matchedOutput.grantSupportComponent ?? null,
      procurementComponent: matchedOutput.procurementComponent ?? null,
      procurementInstallationComponent: matchedOutput.procurementInstallationComponent ?? null,
      infrastructureComponent: matchedOutput.infrastructureComponent ?? null,
      sequenceOrder: null,
      plannedStartDate: null,
      plannedEndDate: null,
      quantity: null,
      notes: null
    };

    // Add to deliverables array
    const currentDeliverables = opp.deliverables || [];
    const updatedOpportunity = {
      ...opp,
      deliverables: [...currentDeliverables, newDeliverable]
    };
    
    // Update opportunity signal
    this.opportunityUpdated.emit(updatedOpportunity);

    // Move item from recommendations to accepted list (for tracking)
    const currentAccepted = this.acceptedDeliverables();
    this.acceptedDeliverables.set([...currentAccepted, item]);
    
    // Mark as changed (AI recommendation accepted)
    this.markAsChanged();

    // Show success message
    this.feedbackService.showSuccessToast({
      summary: this.translateService.instant('message.success'),
      detail: this.translateService.instant('message.recommendationAccepted'),
      life: 3000
    });

    this.cdr.detectChanges();
  }

  /**
   * Dismiss extracted deliverable (remove from recommendations)
   * @description Removes an extracted item from the visible list without adding it
   */

  /**
   * Add accepted deliverables to opportunity
   * @description Converts accepted extracted items to OpportunityDeliverable and saves them
   */
  addAcceptedDeliverablesToOpportunity(): void {
    const accepted = this.acceptedDeliverables();
    if (accepted.length === 0) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.noAcceptedDeliverables'),
        life: 3000
      });
      return;
    }

    const opp = this.opportunity();
    if (!opp) return;

    // Convert accepted items to OpportunityDeliverable format
    const newDeliverables: OpportunityDeliverable[] = accepted
      .filter(item => item.matchedOutputId) // Only add items with matched outputs
      .map(item => ({
        id: 0,
        opportunityId: opp.id,
        outputId: item.matchedOutputId!,
        outputName: item.matchedOutputName || item.partnerLanguage,
        level0: null,
        level1: null,
        definitionLevel1: null,
        level2: null,
        definitionLevel2: null,
        level3: null,
        definitionLevel3: null,
        level4: null,
        definitionLevel4: null,
        serviceLine: null,
        grantSupportImplementingModality: null,
        grantSupportComponent: null,
        procurementComponent: null,
        procurementInstallationComponent: null,
        infrastructureComponent: null,
        sequenceOrder: null,
        plannedStartDate: null,
        plannedEndDate: null,
        quantity: null,
        notes: `Extracted from: ${item.sourceDocumentName}\nContext: ${item.context}\nConfidence: ${(item.confidence * 100).toFixed(0)}%`
      }));

    if (newDeliverables.length === 0) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.noMatchedOutputs'),
        life: 3000
      });
      return;
    }

    // Add to existing deliverables
    const updatedDeliverables = [...(opp.deliverables || []), ...newDeliverables];
    const updatedOpportunity = {
      ...opp,
      deliverables: updatedDeliverables
    };

    // Emit updated opportunity to parent
    this.opportunityUpdated.emit(updatedOpportunity);

    // Clear accepted list
    this.acceptedDeliverables.set([]);
    this.extractedDeliverables.set([]);

    this.feedbackService.showSuccessToast({
      summary: this.translateService.instant('message.success'),
      detail: this.translateService.instant('message.deliverablesAdded', { count: newDeliverables.length }),
      life: 5000
    });

    this.cdr.detectChanges();
  }
}

