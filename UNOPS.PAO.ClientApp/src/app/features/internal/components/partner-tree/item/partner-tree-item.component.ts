import { Component, EventEmitter, Input, ChangeDetectionStrategy, inject, OnInit, OnChanges, output, Output } from '@angular/core';
import { BlockUI } from 'primeng/blockui';
import { PartnerTreeService } from '../../../services/partner-tree.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { PanelModule } from 'primeng/panel';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { CommonModule } from '@angular/common';
import { SelectModule } from 'primeng/select';
import { DialogModule } from 'primeng/dialog';

@Component({
  selector: 'app-partner-tree-item',
  imports: [BlockUI, ReactiveFormsModule, SelectModule, TranslateModule, ButtonModule, PanelModule, InputTextModule, CommonModule, TextareaModule, DialogModule],
  templateUrl: './partner-tree-item.component.html',
  styleUrl: './partner-tree-item.component.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PartnerTreeItemComponent implements OnInit, OnChanges {
  @Input() record: any = null;
  @Output() closeModal = new EventEmitter<void>();
  display = true;
  
  partnerTreeService = inject(PartnerTreeService);
  feedbackDialogService = inject(FeedbackDialogService);
  formGroup = new FormGroup({
      id: new FormControl(null, {
        validators: [Validators.required]
      }),
      name: new FormControl('', {
        validators:[Validators.required]
      }),
      description: new FormControl('', {
        validators:[Validators.required]
      }),
      code: new FormControl('', {
        validators:[Validators.required]
      }),
      type: new FormControl('', {
        validators:[Validators.required]
      }),
      parent: new FormControl('', {
        validators:[Validators.required]
      }),
      status: new FormControl('', {
        validators:[Validators.required]
      }),
    });
  cachedDataService = inject(CachedDataService);
  allTypeData = this.cachedDataService.allPartnerLevelTypes;
  allStatusData = this.cachedDataService.allStatus;
  onRecordUpdateSuccess = output();
  parentOptions = this.partnerTreeService.parentOptions;
  constructor() { }

  ngOnChanges() {
    this.display = true;
    if (this.record) {
      this.formGroup.patchValue(this.record);
    }
  }

  filterParentOptions() {
    let rowData = this.record;
    return this.parentOptions.filter(option => option.value !== rowData.parent && option.value !== rowData.code);
  }

  _handleOnSaveClick(activate: boolean = true) {
    let payload = this._getRequestPayload();
    if (payload.status === 'Active') {
      payload.status = '1';
    }
    if (activate === true && payload.status === 'Inactive') {
      payload.status = '1';
    }
    this.partnerTreeService.updatePartnerTreeLevel([payload]).subscribe({
      next: (data: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Changes saved successfully!' });
        this.onRecordUpdateSuccess.emit(data);
        this.hide();
      }
    });
  }

  _handleOnCreateClick() {
    let payload = this._getRequestPayload();
    delete payload.id;
    if (payload.status === 'Active') {
      payload.status = '1';
    }
    this.partnerTreeService.createPartnerTreeLevel(payload).subscribe({
      next: (data: any) => {
        this.feedbackDialogService.showSuccessToast({ detail: 'Record created successfully!' });
        this.onRecordUpdateSuccess.emit(data);
        this.hide();
      }
    });
  }

  _handleOnDeleteClick() {
    this.record.status = '0';
    this.partnerTreeService.updatePartnerTreeLevel([this.record]).subscribe({
      next: (data: any) => {
        this.partnerTreeService.deletePartnerLevel(this.record.id).subscribe({
          next: (data: any) => {
            this.feedbackDialogService.showSuccessToast({ detail: 'Record deleted successfully!' });
            this.onRecordUpdateSuccess.emit(data);
            this.hide();
          }
        });
      }
    });
  }

  _getRequestPayload() {
    let valueObj = this.formGroup.value,
      requestJsonObj: any = {};

    for (let key in valueObj) {
      if (valueObj.hasOwnProperty(key)) {
        let indexValue = (valueObj as any)[key];
        requestJsonObj[key] = indexValue || '';
      }
    }

    return requestJsonObj;
  }

  hide(): void {
    this.display = false;
    this.closeModal.emit();
  }

  ngOnInit() {
    this.formGroup.patchValue(this.record);
    this.parentOptions = this.filterParentOptions();
  }
}
