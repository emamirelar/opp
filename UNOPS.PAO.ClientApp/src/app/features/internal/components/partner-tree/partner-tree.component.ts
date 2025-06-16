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
import { Router, RouterModule } from '@angular/router';
import { PartnerTree } from '../../models/partner-tree.model';
import { DialogService } from 'primeng/dynamicdialog';
import { PartnerTreeItemComponent } from './item/partner-tree-item.component';
import { PermissionUtilityService } from '../../../../essentials/services/permission-utility.service';
import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';
import { EntityConfigurationService } from '../../services/entity-configuration.service';
import { ListViewColumn } from '../../../../common/pages/components/listview/listview.model';

@Component({
  selector: 'app-partner-tree',
  imports: [DialogModule, ProgressSpinnerModule, TreeTableModule, ButtonModule, CommonModule, FormsModule, TableModule, TranslateModule, ToggleSwitchModule, SelectModule, TooltipModule, RouterModule],
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
  entityConfigurationService = inject(EntityConfigurationService);
  originalData: any[] = [];
  override isDataLoading = this.service.isLoading();

  // Dynamic partner tree columns loaded from API  
  treeColumns = signal<ListViewColumn[]>([]);
  treeColumnsLoading = signal(true);

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

  // RBAC permissions
  permissionUtilityService = inject(PermissionUtilityService);
  override feedbackDialogService = inject(FeedbackDialogService);
  entityPermissionsData = this.permissionUtilityService.createEntityPermissions('PartnerTree');
  entityPermissions = this.entityPermissionsData.entityPermissions;
  permissionsLoading = this.entityPermissionsData.permissionsLoading;

  constructor(public override router: Router) {
    super();
  }

  override ngOnInit() {
    super.ngOnInit();
    // Load entity permissions
    this.entityPermissionsData.loadPermissions(this.router, this.cdr);
    
    // Load dynamic columns from API
    this.loadPartnerTreeColumns();
    
    this.setNewPartnerFromAIAssistant();
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        this.loadPartnerTreeData();
      }
    });

    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });

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
    // Check permission before opening modal
    if (!this.permissionUtilityService.canCreate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({ 
        detail: 'You do not have permission to create partner trees' 
      });
      return;
    }

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
    // Check permission before opening modal
    if (!this.permissionUtilityService.canUpdate(this.entityPermissions())) {
      this.feedbackDialogService.showErrorToast({ 
        detail: 'You do not have permission to edit partner trees' 
      });
      return;
    }

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

  private loadPartnerTreeColumns() {
    this.treeColumnsLoading.set(true);
    this.entityConfigurationService.getEntityListViewConfiguration('PartnerTree')
      .subscribe({
        next: (columns) => {
          // Convert backend columns to frontend format and add template functions
          const processedColumns = columns.map(col => this.processColumn(col));
          this.treeColumns.set(processedColumns);
          this.treeColumnsLoading.set(false);
          this.cdr.detectChanges();
        },
        error: (error) => {
          console.error('Failed to load partner tree columns:', error);
          // Fallback to default columns if API fails
          this.setFallbackTreeColumns();
          this.treeColumnsLoading.set(false);
          this.cdr.detectChanges();
        }
      });
  }

  private processColumn(column: any): ListViewColumn {
    const processedColumn: ListViewColumn = {
      field: column.field,
      label: column.label,
      type: column.type,
      sortable: column.sortable,
      width: column.width,
      ellipsis: column.ellipsis,
      helperText: column.helperText
    };

    // Handle nested field paths (fields with dots) by adding a template function
    if (column.field && column.field.includes('.') && column.type !== 'template') {
      // Keep the original field for identification but add a template function to access nested data
      processedColumn.templateFn = (rowData: any) => {
        const value = this.getNestedProperty(rowData, column.field);
        return value !== undefined && value !== null ? String(value) : '';
      };
      // Change type to template since we're now using a template function
      processedColumn.type = 'template';
    }

    // Add template function for template type columns
    if (column.type === 'template' && column.templatePattern) {
      processedColumn.templateFn = this.createTemplateFunction(column.templatePattern);
    }

    return processedColumn;
  }

  private createTemplateFunction(templatePattern: string): (rowData: any) => string {
    return (rowData: any) => {
      let result = templatePattern;
      
      // Replace field placeholders like {name}, {description} with actual values
      const fieldMatches = templatePattern.match(/\{([^}]+)\}/g);
      if (fieldMatches) {
        fieldMatches.forEach(match => {
          const fieldName = match.replace(/[{}]/g, '');
          const fieldValue = this.getNestedProperty(rowData, fieldName) || '';
          result = result.replace(match, fieldValue);
        });
      }
      
      return result.trim();
    };
  }

  private getNestedProperty(obj: any, path: string): any {
    return path.split('.').reduce((o, p) => o?.[p], obj);
  }

  private setFallbackTreeColumns() {
    // Fallback to original hardcoded columns if API fails
    // Note: Actions are always hardcoded in HTML template, not included here
    const fallbackColumns: ListViewColumn[] = [
      {
        field: 'name',
        label: 'label.partnerTree.name',
        sortable: false,
        type: 'text'
      },
      {
        field: 'description',
        label: 'label.partnerTree.description',
        sortable: false,
        type: 'text'
      },
      {
        field: 'type',
        label: 'label.partnerTree.type',
        sortable: false,
        type: 'text',
        width: '80px'
      },
      {
        field: 'partnerCategoryName',
        label: 'label.partnerTree.partnerCategory',
        sortable: false,
        type: 'text'
      },
      {
        field: 'partnerGroupName',
        label: 'label.partnerTree.partnerGroup',
        sortable: false,
        type: 'text'
      }
    ];
    
    this.treeColumns.set(fallbackColumns);
  }
}
