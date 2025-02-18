import { MenuItem } from 'primeng/api';
import { WorkflowService } from './../../../services/workflow.service';
import { Component, inject, input, Input, OnInit, output, signal } from '@angular/core';

import { SplitButton } from 'primeng/splitbutton';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';

import { FeedbackDialogService } from '../../../pages/services/feedback-dialog.service';

@Component({
  selector: 'app-workflow',
  templateUrl: './workflow.component.html',
  styleUrl: './workflow.component.scss',
  imports: [SplitButton, DialogModule, ButtonModule, TextareaModule],
})
export class WorkflowComponent implements OnInit {
  feedbackDialogService = inject(FeedbackDialogService);

  entityName = input.required<string>();
  entityId = input.required<string>();

  @Input() disabled!: boolean
  @Input() beforeStageChange!: () => Promise<boolean>;

  stageChangeSuccess = output();

  primaryeStageLabel = '';
  primaryeStageName = '';
  primaryStageChangeCommentRequired = false;
  showCommentDialog = false;

  workflowService = inject(WorkflowService);

  items: MenuItem[] = [];

  nextStage = signal('');

  ngOnInit(): void {
    //loads worflows for current record.
    this.workflowService
      .getNextWorkFlowAtionsForARecordById(this.entityName(), this.entityId())
      .subscribe({
        next: (data: any) => {
          this._initialiseUI(data['nextActions']);
        },
      });
  }

  _initialiseUI(actions: any) {
    let itemsArray: any = [];
    actions.forEach((item: any, index: number) => {
      if (index === 0) {
        this.primaryeStageName = item['newStage'];
        this.primaryeStageLabel = item['actionName'];
        this.primaryStageChangeCommentRequired = item['commentRequired'];
      } else {
        itemsArray.push({
          label: item['actionName'],
          command: () => {
            this._executeStageChange(item['newStage'], item['commentRequired']);
          },
        });
      }
    });

    this.items = [...itemsArray];
  }

  _executeStageChange(nextStage: string, commentRequired: boolean) {

    this.nextStage.set( nextStage );

    if (!this.beforeStageChange) {
      if( commentRequired )
      {
        this.showCommentDialog = true;
      }
      else{
        this._performStageChange();
      }
    } else {
      this.beforeStageChange().then((result: boolean) => {
        if (result) {
          if( commentRequired )
          {
            this.showCommentDialog = true;
          }
          else{
            this._performStageChange();
          }
        }
      });
    }
  }

  _performStageChange( comment ?: string) {
    let requestJson = {
      entityName: this.entityName(),
      id: this.entityId(),
      newStage: this.nextStage(),
      comment: ( comment ? comment : '' ),
    };


    this.workflowService.changeWorkflow(requestJson).subscribe({
      next: (data: any) => {
        this._initialiseUI(data['nextActions']);
        this.feedbackDialogService.showSuccessToast({
          detail: 'Stage changed successfully!',
        });
        this.stageChangeSuccess.emit(data);
      },
    });
  }

  _handleOnPrimaryStageClick() {
    this._executeStageChange(this.primaryeStageName, this.primaryStageChangeCommentRequired);
  }

  handleOnCommentSave( comment: string ){
    this._performStageChange( comment );
    this.showCommentDialog = false;
  }
}
