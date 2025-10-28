import { Component, OnInit, OnDestroy, OnChanges, SimpleChanges, Input, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import {MessageService, TreeNode} from 'primeng/api';
import { IconField } from 'primeng/iconfield';
import { InputIcon } from 'primeng/inputicon';
import { Skeleton } from 'primeng/skeleton';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OrganizationHierarchyService } from '@core/services/organization';
import { UserPreferenceService } from '@core/services/user';
import { GlobalFilterService } from '@core/services/filters';
import { Subject, takeUntil, forkJoin } from 'rxjs';

interface OrgUnitOption {
  id: number;
  name: string;
  code: string;
  type: number;
  level: number;
  parentId?: number;
  isDefault: boolean;
  icon: string;
  expanded?: boolean;
  hasChildren?: boolean;
  visible?: boolean;
}

@Component({
  selector: 'app-org-unit-selector',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    ToastModule,
    IconField,
    InputIcon,
    Skeleton,
    TranslateModule,
  ],
  templateUrl: './org-unit-selector.component.html',
  styleUrls: ['./org-unit-selector.component.scss'],
  providers: [MessageService]
})
export class OrgUnitSelectorComponent implements OnInit, OnDestroy, OnChanges {
  @Input() preselectedOrgUnitId: number | null = null;
  @Output() orgUnitSelected = new EventEmitter<OrgUnitOption | null>();
  
  selectedOrgUnit: OrgUnitOption | null = null;
  tempSelectedOrgUnit: OrgUnitOption | null = null;
  orgUnitOptions: OrgUnitOption[] = [];
  filteredOrgUnits: OrgUnitOption[] = [];
  loading = true;
  visible = false;
  searchText = '';
  defaultOrgUnitId: number | null = null;
  expandedNodeIds = new Set<number>();

  private destroy$ = new Subject<void>();

  constructor(
    private organizationHierarchyService: OrganizationHierarchyService,
    private userPreferenceService: UserPreferenceService,
    private globalFilterService: GlobalFilterService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef,
    private translateService: TranslateService
  ) {}

  ngOnInit() {
    this.loadData();
  }

  ngOnChanges(changes: SimpleChanges) {
    // React to changes in preselectedOrgUnitId
    if (changes['preselectedOrgUnitId'] && this.orgUnitOptions.length > 0) {
      // Force update when preselectedOrgUnitId changes
      this.forceUpdateSelectedOrgUnit();
    }
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  getDisplayName(): string {
    if (!this.selectedOrgUnit) {
      return this.translateService.instant('orgUnitSelector.allOrganizationalUnits');
    }
    
    // If this is the root node (level 0), always show "All organizational units"
    if (this.selectedOrgUnit.level === 0) {
      return this.translateService.instant('orgUnitSelector.allOrganizationalUnits');
    }
    
    return this.selectedOrgUnit.name;
  }

  showDialog() {
    this.visible = true;
    this.tempSelectedOrgUnit = this.selectedOrgUnit;
    this.searchText = '';
    this.filterOrgUnits();

    // Scroll to selected item after dialog opens
    setTimeout(() => {
      const selectedElement = document.querySelector('.org-unit-selected');
      if (selectedElement) {
        selectedElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    }, 100);
  }

  hideDialog() {
    this.visible = false;
    this.tempSelectedOrgUnit = null;
    this.searchText = '';
  }

  confirmSelection() {
    if (this.tempSelectedOrgUnit) {
      this.selectedOrgUnit = this.tempSelectedOrgUnit;
      this.onOrgUnitChange();
      this.hideDialog();
    }
  }

  selectOrgUnit(orgUnit: OrgUnitOption) {
    this.tempSelectedOrgUnit = orgUnit;
    this.selectedOrgUnit = orgUnit;
    this.onOrgUnitChange();
    this.hideDialog();
  }

  filterOrgUnits() {
    if (!this.searchText.trim()) {
      // Show all items with proper visibility based on expansion
      this.filteredOrgUnits = this.orgUnitOptions.filter(unit => {
        // Set visibility based on expansion state
        unit.visible = this.isUnitVisible(unit);
        return unit.visible;
      });
      return;
    }

    const searchLower = this.searchText.toLowerCase();
    const matchingIds = new Set<number>();
    const parentIds = new Set<number>();

    // Find all matching units and their parents
    this.orgUnitOptions.forEach(unit => {
      if (unit.name.toLowerCase().includes(searchLower) ||
          unit.code.toLowerCase().includes(searchLower)) {
        matchingIds.add(unit.id);
        // Add all parent IDs to show the path
        let currentUnit = unit;
        while (currentUnit.parentId) {
          parentIds.add(currentUnit.parentId);
          currentUnit = this.orgUnitOptions.find(u => u.id === currentUnit.parentId)!;
        }
      }
    });

    // Expand all parents of matching items when searching
    if (this.searchText.trim()) {
      parentIds.forEach(id => this.expandedNodeIds.add(id));
    }

    // Filter to show only matching items and their parents, maintaining original order
    this.filteredOrgUnits = this.orgUnitOptions.filter(unit => {
      const shouldShow = matchingIds.has(unit.id) || parentIds.has(unit.id);
      // Update visibility flag
      unit.visible = shouldShow;
      return shouldShow;
    });
  }

  onSearchChange() {
    this.filterOrgUnits();
  }

  toggleNodeExpansion(orgUnit: OrgUnitOption, event: Event) {
    event.stopPropagation();
    if (this.expandedNodeIds.has(orgUnit.id)) {
      this.expandedNodeIds.delete(orgUnit.id);
    } else {
      this.expandedNodeIds.add(orgUnit.id);
    }
    this.filterOrgUnits();
  }

  isUnitVisible(unit: OrgUnitOption): boolean {
    // Root level items are always visible
    if (!unit.parentId) {
      return true;
    }

    // Check if all parents are expanded
    let currentUnit = unit;
    while (currentUnit.parentId) {
      if (!this.expandedNodeIds.has(currentUnit.parentId)) {
        return false;
      }
      const parent = this.orgUnitOptions.find(u => u.id === currentUnit.parentId);
      if (!parent) {
        return false;
      }
      currentUnit = parent;
    }

    return true;
  }

  isNodeExpanded(orgUnit: OrgUnitOption): boolean {
    return this.expandedNodeIds.has(orgUnit.id);
  }

  isNodeSelected(orgUnit: OrgUnitOption): boolean {
    return this.selectedOrgUnit?.id === orgUnit.id;
  }

  private loadData() {
    this.loading = true;

    // Load both org hierarchy and user preference in parallel
    forkJoin({
      hierarchy: this.organizationHierarchyService.getOrganizationHierarchy(),
      preference: this.userPreferenceService.getDefaultOrgUnit()
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: ({ hierarchy, preference }) => {
        this.defaultOrgUnitId = preference.defaultOrgUnitId;
        this.processHierarchyData(hierarchy);
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading data:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load organization units'
        });
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  private processHierarchyData(treeNodes: TreeNode[]) {
    // Process for flat list
    this.orgUnitOptions = [];
    this.flattenTreeNodes(treeNodes, 0);

    // Mark units that have children
    const parentIds = new Set(this.orgUnitOptions.filter(u => u.parentId).map(u => u.parentId!));
    this.orgUnitOptions.forEach(unit => {
      unit.hasChildren = parentIds.has(unit.id);
    });

    // Expand first level by default
    this.orgUnitOptions.forEach(unit => {
      if (unit.level === 0) {
        this.expandedNodeIds.add(unit.id);
      }
    });

    // No sorting needed - flattenTreeNodes already provides correct hierarchical order

    this.filterOrgUnits();
    
    this.updateSelectedOrgUnit();
  }


  private flattenTreeNodes(nodes: TreeNode[], level: number) {
    nodes.forEach(node => {
      if (node.data) {
        const orgUnit: OrgUnitOption = {
          id: node.data.id,
          name: node.data.name,
          code: node.data.code,
          type: node.data.type,
          level: level,
          parentId: node.data.parentId,
          isDefault: node.data.id === this.defaultOrgUnitId,
          icon: this.getIconForType(node.data.type),
          hasChildren: node.children && node.children.length > 0,
          visible: true
        };
        this.orgUnitOptions.push(orgUnit);

        if (node.children && node.children.length > 0) {
          this.flattenTreeNodes(node.children, level + 1);
        }
      }
    });
  }

  private getIconForType(type: number): string {
    switch (type) {
      case 0: return 'pi-building'; // Organization
      case 1: return 'pi-sitemap'; // Business Group
      case 2: return 'pi-map-marker'; // Country Office
      case 3: return 'pi-folder'; // Unit
      default: return 'pi-folder';
    }
  }

  setAsDefault(orgUnit: OrgUnitOption, event: Event) {
    event.stopPropagation();
    event.preventDefault();

    if (orgUnit.id === this.defaultOrgUnitId) {
      // Already default, do nothing
      return;
    }

    this.userPreferenceService.setDefaultOrgUnit(orgUnit.id).subscribe({
      next: () => {
        // Update the default status
        this.orgUnitOptions.forEach(ou => ou.isDefault = false);
        this.filteredOrgUnits.forEach(ou => ou.isDefault = false);
        orgUnit.isDefault = true;
        this.defaultOrgUnitId = orgUnit.id;

        // Find the org unit in the main array and update it too
        const mainOrgUnit = this.orgUnitOptions.find(ou => ou.id === orgUnit.id);
        if (mainOrgUnit) {
          mainOrgUnit.isDefault = true;
        }

        // Keep original hierarchical order instead of sorting by default status
        // Re-apply filter
        this.filterOrgUnits();

        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `${orgUnit.name} set as default organization unit`
        });
      },
      error: (error) => {
        console.error('Error setting default org unit:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to set default organization unit'
        });
      }
    });
  }

  private onOrgUnitChange() {
    if (this.selectedOrgUnit) {
      // Only emit the selected org unit for parent components listening
      // Don't automatically update global filter service or trigger page refresh
      this.orgUnitSelected.emit(this.selectedOrgUnit);
    } else {
      this.orgUnitSelected.emit(null);
    }
  }

  private updateSelectedOrgUnit() {
    // Only set selectedOrgUnit if no explicit selection has been made
    // This prevents overriding user's choice from global filters dialog
    if (!this.selectedOrgUnit) {
      this.forceUpdateSelectedOrgUnit();
    }
  }

  private forceUpdateSelectedOrgUnit() {
    // Always update selectedOrgUnit based on current preselectedOrgUnitId
    // Priority 1: Use preselected org unit ID from global filters dialog
    if (this.preselectedOrgUnitId) {
      this.selectedOrgUnit = this.orgUnitOptions.find(ou => ou.id === this.preselectedOrgUnitId) || null;
    }
    // Priority 2: If preselectedOrgUnitId is null, clear selection (show all)
    else if (this.preselectedOrgUnitId === null) {
      this.selectedOrgUnit = null;
    }
    // Priority 3: Use user's default org unit (only if no preselected value is set)
    else if (!this.selectedOrgUnit && this.defaultOrgUnitId) {
      this.selectedOrgUnit = this.orgUnitOptions.find(ou => ou.id === this.defaultOrgUnitId) || null;
    }
    
    // Trigger change detection to update the UI
    this.cdr.detectChanges();
  }

  // Tree helper methods


}
