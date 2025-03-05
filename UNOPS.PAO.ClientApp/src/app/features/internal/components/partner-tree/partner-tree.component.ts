import { ChangeDetectionStrategy, Component, OnInit, inject, effect, ChangeDetectorRef } from '@angular/core';
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
import { PartnerTreeItemComponent } from './partner-tree-item/partner-tree-item.component';
import { CachedDataService } from '../../../../common/services/cached-data.service';
import { InputIcon } from 'primeng/inputicon';
import { IconField } from 'primeng/iconfield';

@Component({
  selector: 'app-partner-tree',
  imports: [DialogModule, PartnerTreeItemComponent, InputIcon, IconField, ProgressSpinnerModule, TreeTableModule, ButtonModule, CommonModule, FormsModule, TableModule, TranslateModule, ToggleSwitchModule, SelectModule],
  templateUrl: './partner-tree.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './partner-tree.component.scss'
})
export class PartnerTreeComponent extends FeatureBaseComponent implements OnInit {
  override data: TreeNode[] = [];
  cols: ColumnDefinition[] = [];
  updatedRecords: any[] = [];
  parentOptions: any[] = [];
  override service = inject(PartnerTreeService);
  cachedDataService = inject(CachedDataService);
  originalData: any[] = [];
  override isDataLoading = this.service.isLoading();
  levelOneOptions: any[] = [];
  levelTwoOptions: any[] = [];
  levelThreeOptions: any[] = [];
  parentUpdated: boolean = false;
  updatePartnerLevel: boolean = false;
  createPartnerLevel: boolean = false;
  changeRecord: any = null;
  allStatusData = this.cachedDataService.allStatus;


  constructor() {
    super();
  }

  override getColumns(): ColumnDefinition[] {
    return [
      { id: 'action', label: 'label.partnerTree.actions', editable: false }, 
      { id: "name", label: "label.partnerTree.name", editable: true }, 
      { id: "description", label: "label.partnerTree.description", editable: true },
      { id: "type", label: "label.partnerTree.type", editable: false },
      { id: "parent", label: "label.partnerTree.parent", editable: true },
      { id: "status", label: "label.partnerTree.status", editable: false },
      { id: 'action', label: 'label.partnerTree.actions', editable: false }, 
    ];
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
    var columns = this.getColumns();
    var originalData = this.parentOptions.find(option => option.id === event.field?.id);
    let valueChanged = false;
    if (originalData) {
       for (let index = 0; index < columns.length; index++) {
          if (columns[index].id !== 'action' && originalData[columns[index].id] !== event.field[columns[index].id]) {
            valueChanged = true;
            break;
          }
       }
    } else {
        valueChanged = true;
    }
    //event.field.status = (event.field.status === 'Active') ? '1' : '0';
    if (valueChanged) {
      this.updatedRecords.push(event.field);
      if (event.data === 'parent') {
        this.parentUpdated = true;
      }
    } else {
      this.updatedRecords = this.updatedRecords.filter(record => record.id !== event.field.id);
    }
  }

  isRecordUpdated(node: any): boolean {
    return this.updatedRecords.some(record => record.id === node?.node?.data?.id);
  }

  override ngOnInit() {
    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        // Initialize columns
        this.cols = this.getColumns();
        this.loadPartnerTreeData();
      }
    });

    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  loadPartnerTreeData() {
      // Make server call to get all partner tree data
      this.service.getAllPartnerTree().subscribe({
        next: (data: any) => {
          this.updatedRecords = [];
          this.data = data;
          this.cdr.detectChanges(); // Trigger change detection
          this.originalData = this.service.originalData;
          this.parentOptions = this.service.parentOptions;
          this.parentUpdated = false;
          this.changeRecord = null;
        },
        error: (err: any) => {
          console.error('Error loading partner tree data:', err);
        }
      });

  }

  onCreateNewPartnerLevel() {
    this.createPartnerLevel = true;
    let level = 'Level_1';
    this.changeRecord = {
      type: level,
      parent: null,
      id: null,
      status: 'Active'
    };

  }

  onAddPartnerLevel(rowData: any) {
    this.createPartnerLevel = true;
    let level = rowData.type.split('_')[0] + '_' + (parseInt(rowData.type.split('_')[1]) + 1);
    this.changeRecord = {
      type: level,
      parent: rowData.code,
      id: null,
      status: 'Active'
    };
  }

  handleOnRevertClick() {
    this.loadPartnerTreeData();
  }

  handleOnSaveClick() {
    this.updatedRecords.forEach(record => {
      record.status = (record.status === 'Active') ? '1' : '0';
    });
    this.service.updatePartnerTreeLevel(this.updatedRecords).subscribe({
      next: (data: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Updated successfully!' });
        this.loadPartnerTreeData();
      }
    });
  }

  handleOnDeleteClick() {
    this.service.deletePartnerLevel(this.updatedRecords).subscribe({
      next: (data: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Record deleted successfully!' });
        this.loadPartnerTreeData();
      }
    });
  }
}
