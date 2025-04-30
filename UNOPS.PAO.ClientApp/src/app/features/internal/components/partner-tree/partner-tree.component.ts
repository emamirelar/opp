import { ChangeDetectionStrategy, Component, OnInit, inject, effect, ChangeDetectorRef, signal, ViewChild } from '@angular/core';
import { TreeTableModule } from 'primeng/treetable';
import { TreeNode } from "primeng/api";
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { TranslateModule } from '@ngx-translate/core';
import { ColumnDefinition, FeatureBaseComponent } from '../../../../common/reusables/feature-base/feature-base.component';
import { PartnerTreeService } from '../../services/partner-tree.service';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { SelectModule } from 'primeng/select';
import { DialogModule } from 'primeng/dialog';
import { TooltipModule } from 'primeng/tooltip';
import { CachedDataService } from '../../../../common/services/cached-data.service';
import { Router } from '@angular/router';
import { PartnerTree } from '../../models/partner-tree.model';
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerTreeItemComponent } from './item/partner-tree-item.component';

@Component({
  selector: 'app-partner-tree',
  imports: [DialogModule, ProgressSpinnerModule, TreeTableModule, ButtonModule, CommonModule, FormsModule, TableModule, TranslateModule, ToggleSwitchModule, SelectModule, TooltipModule],
  templateUrl: './partner-tree.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './partner-tree.component.scss'
})
export class PartnerTreeComponent extends FeatureBaseComponent implements OnInit {
  @ViewChild('partnerTreeTable') partnerTreeTable: any;
  
  // State management
  expandedNodes: Map<string, boolean> = new Map();
  override data: TreeNode<PartnerTree>[] = [];
  updatedRecords: any[] = [];
  parentOptions: any[] = [];
  override service = inject(PartnerTreeService);
  cachedDataService = inject(CachedDataService);
  originalData: any[] = [];
  override isDataLoading = this.service.isLoading();
  
  // Dialog state
  parentUpdated: boolean = false;
  updatePartnerLevel: boolean = false;
  createPartnerLevel: boolean = false;
  changeRecord: any = null;
  
  // Data options
  partnerGroupOptions: any[] = [];
  partnerTree: TreeNode<PartnerTree>[] = [];
  selectedNode: TreeNode<PartnerTree> | null = null;
  loading = false;
  private dialogService = inject(DialogService);

  constructor(public override router: Router) {
    super();
  }

  override ngOnInit() {
    this.setNewPartnerFromAIAssistant();
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        this.loadPartnerTreeData();
      }
    });

    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });

    this.loadPartnerTree();
    
    // Initialize expandedNodes map
    this.expandedNodes = new Map();
  }

  // Get filtered partner group options based on parent
  getFilteredPartnerGroupOptions(rowData: any): any[] {
    if (rowData && rowData.code) {
      return this.service.getChildrenByParentCode(rowData.code);
    }
    return [];
  }

  handleOnRecordUpdation(event: any) {
    this.updatePartnerLevel = false;
    this.createPartnerLevel = false;
    this.loadPartnerTreeData();
  }

  onEditComplete(event: any) {
    if (event.data === 'action') {
      return;
    }

    // Validate required fields
    if (event.field) {
      if (event.field.name === '' && event.column.field === 'name') {
        this.feedbackDialogService.showErrorToast({ detail: 'Name is required' });
        return;
      }

      if (this.isPartnerCategoryEditable(event.field) && event.field.partnerCategory === '' && event.column.field === 'partnerCategory') {
        this.feedbackDialogService.showErrorToast({ detail: 'Partner Category is required' });
        return;
      }

      if (event.field.partnerGroup === '' && event.column.field === 'partnerGroup') {
        this.feedbackDialogService.showErrorToast({ detail: 'Partner Group is required' });
        return;
      }

      if (event.field.code === '' && event.column.field === 'code') {
        this.feedbackDialogService.showErrorToast({ detail: 'Code is required' });
        return;
      }
    }

    // Handle object-to-string conversions for dropdown selections
    this.convertObjectSelectionsToValues(event.field);

    // Check if the record actually changed
    var originalData = this.parentOptions.find(option => option.id === event.field?.id);
    let valueChanged = this.hasRecordChanged(originalData, event.field);
    
    if (valueChanged) {
      this.updatedRecords.push(event.field);
      if (event.data === 'parent') {
        this.parentUpdated = true;
      }
    } else {
      this.updatedRecords = this.updatedRecords.filter(record => record.id !== event.field.id);
    }
  }

  // Convert object selections to string values
  private convertObjectSelectionsToValues(field: any) {
    if (!field) return;
    
    // Handle partnerCategory selection, converting object to code if needed
    if (field.partnerCategory && typeof field.partnerCategory === 'object') {
      field.partnerCategoryName = field.partnerCategory.name;
      field.partnerCategory = field.partnerCategory.code;
    }

    // Handle partnerGroup selection, converting object to code if needed
    if (field.partnerGroup && typeof field.partnerGroup === 'object') {
      field.partnerGroupName = field.partnerGroup.name;
      field.partnerGroup = field.partnerGroup.code;
    }
  }

  // Check if a record has changed compared to original
  private hasRecordChanged(originalData: any, currentData: any): boolean {
    if (!originalData) return true;
    
    const columnIds = ['name', 'description', 'type', 'partnerCategory', 'partnerGroup', 'code'];
    for (const columnId of columnIds) {
      if (originalData[columnId] !== currentData[columnId]) {
        return true;
      }
    }
    
    return false;
  }

  isRecordUpdated(node: any): boolean {
    return this.updatedRecords.some(record => record.id === node?.node?.data?.id);
  }

  private setNewPartnerFromAIAssistant() {
    this.activatedRoute.queryParams.subscribe(params => {
      if (params['openNewDialog'] === 'true') {
        const state = history.state;
        if (state?.data) {
          this.changeRecord = state.data;
          this.createPartnerLevel = true;
        }
      }
    });
  }

  loadPartnerTree() {
    this.loading = true;
    this.service.getAllPartnerTree().subscribe({
      next: (data) => {
        this.partnerTree = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading partner tree:', error);
        this.loading = false;
      }
    });
  }

  onNodeSelect(event: { node: TreeNode<PartnerTree> }) {
    this.selectedNode = event.node;
  }

  loadPartnerTreeData() {
    // Make server call to get all partner tree data
    this.service.getAllPartnerTree().subscribe({
      next: (data: any) => {
        this.updatedRecords = [];
        this.data = data;
        
        // Restore expanded state after loading data
        this.restoreExpansionState();
        this.cdr.detectChanges(); // Trigger change detection
        this.originalData = this.service.originalData;
        this.parentOptions = this.service.parentOptions;
        // Initialize partnerGroupOptions
        this.partnerGroupOptions = this.service.partnerGroupOptions || [];
        this.parentUpdated = false;
        this.changeRecord = null;
      },
      error: (err: any) => {
        console.error('Error loading partner tree data:', err);
      }
    });
  }

  // Check if Partner Category is editable for a node
  isPartnerCategoryEditable(rowData: any): boolean {
    if (!rowData) return false;
    return rowData.partnerCategoryEditable === true;
  }

  // Check if Partner Group is editable for a node
  isPartnerGroupEditable(rowData: any): boolean {
    if (!rowData) return false;
    return rowData.partnerGroupEditable === true;
  }

  onCreateNewPartnerLevel() {
    let level = 'Level_1';
    this.changeRecord = {
      type: level,
      parent: '',
      id: null,
      status: 'Active'
    };
    
    const ref = this.dialogService.open(PartnerTreeItemComponent, {
      header: 'New Partner Level',
      width: '50rem',
      closable: true,
      data: {
        record: this.changeRecord
      }
    });

    ref.onClose.subscribe((result: PartnerTree) => {
      if (result) {
        this.handleOnRecordUpdation(result);
      }
    });
  }

  onAddPartnerLevel(rowData: any) {
    let level = rowData.type.split('_')[0] + '_' + (parseInt(rowData.type.split('_')[1]) + 1);
    this.changeRecord = {
      type: level,
      parent: rowData.code,
      id: null,
      status: 'Active'
    };

    const ref = this.dialogService.open(PartnerTreeItemComponent, {
      header: 'New Partner Level',
      width: '50rem',
      closable: true,
      data: {
        record: this.changeRecord
      }
    });

    ref.onClose.subscribe((result: PartnerTree) => {
      if (result) {
        this.handleOnRecordUpdation(result);
      }
    });
  }

  handleOnRevertClick() {
    // Save the current expansion state before reloading
    this.saveExpansionState();
    this.loadPartnerTreeData();
  }

  handleOnSaveClick() {
    // Validate required fields
    const invalidRecords = this.updatedRecords.filter(record =>
      !record.name || record.name.trim() === '' ||
      !record.code || record.code.trim() === '');

    if (invalidRecords.length > 0) {
      this.feedbackDialogService.showErrorToast({ detail: 'Name, and Code are required for all records' });
      return;
    }

    // Save the current expansion state before making the API call
    this.saveExpansionState();

    // Process records before saving
    const recordsToSave = this.updatedRecords.map(record => {
      // Create a copy to avoid modifying the original
      const processedRecord = {...record};

      // Ensure partnerCategory is stored as a code value
      if (processedRecord.partnerCategory && typeof processedRecord.partnerCategory === 'object') {
        processedRecord.partnerCategory = processedRecord.partnerCategory.code;
      }

      // Ensure partnerGroup is stored as a code value
      if (processedRecord.partnerGroup && typeof processedRecord.partnerGroup === 'object') {
        processedRecord.partnerGroup = processedRecord.partnerGroup.code;
      }

      // Convert status
      processedRecord.status = (processedRecord.status === 'Active') ? '1' : '0';

      return processedRecord;
    });

    this.service.updatePartnerTreeLevel(recordsToSave).subscribe({
      next: (data: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Updated successfully!' });
        this.loadPartnerTreeData();
      }
    });
  }

  hasInvalidRecords(): boolean {
    return this.updatedRecords.some(record =>
      !record.name || record.name.trim() === '' ||
      !record.code || record.code.trim() === '');
  }

  openPartnerDialog(rowData: any) {
    const ref = this.dialogService.open(PartnerTreeItemComponent, {
      header: 'View Partner Level',
      width: '50rem',
      closable: true,
      data: {
        record: rowData
      }
    });

    ref.onClose.subscribe((result: PartnerTree) => {
      if (result) {
        this.handleOnRecordUpdation(result);
      }
    });
  }

  // Track expanded nodes
  onNodeExpand(event: any) {
    if (event.node && event.node.data && event.node.data.id) {
      this.expandedNodes.set(String(event.node.data.id), true);
    }
  }

  // Track collapsed nodes
  onNodeCollapse(event: any) {
    if (event.node && event.node.data && event.node.data.id) {
      this.expandedNodes.delete(String(event.node.data.id));
    }
  }

  // Save expansion state of all currently expanded nodes
  saveExpansionState() {
    this.expandedNodes.clear();
    this.captureExpandedNodes(this.data);
  }

  // Recursive function to capture all expanded nodes
  private captureExpandedNodes(nodes: TreeNode<PartnerTree>[]) {
    if (!nodes) return;
    
    nodes.forEach(node => {
      if (node.expanded) {
        if (node.data && node.data.id) {
          this.expandedNodes.set(String(node.data.id), true);
        }
      }
      
      if (node.children && node.children.length > 0) {
        this.captureExpandedNodes(node.children);
      }
    });
  }

  // Restore expansion state
  restoreExpansionState() {
    this.expandedNodes.size > 0 && this.applyExpansionState(this.data);
    this.cdr.detectChanges();
  }

  // Recursive function to restore expanded nodes
  private applyExpansionState(nodes: TreeNode<PartnerTree>[]) {
    if (!nodes) return;
    
    nodes.forEach(node => {
      if (node.data && node.data.id && this.expandedNodes.has(String(node.data.id))) {
        node.expanded = true;
      }
      
      if (node.children && node.children.length > 0) {
        this.applyExpansionState(node.children);
      }
    });
  }
}
