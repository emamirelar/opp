import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PanelModule } from 'primeng/panel';
import { FluidModule } from 'primeng/fluid';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { MultiSelectModule } from 'primeng/multiselect';
import { SelectModule } from 'primeng/select';
import { InputNumberModule } from 'primeng/inputnumber';
import { StepperModule } from 'primeng/stepper';
import { CardModule } from 'primeng/card';
import { StepsModule } from 'primeng/steps';
import { DatePicker, DatePickerModule } from 'primeng/datepicker';
import { DatePipe } from '@angular/common';


import { FormsModule } from '@angular/forms';
import { DocumentUploadComponent } from '../../../../../common/reusables/components/document-upload/document-upload.component';
import { MenuItem } from 'primeng/api';
import { CheckboxModule } from 'primeng/checkbox';
import { BlockUI } from 'primeng/blockui';
import { WorkflowComponent } from "../../../../../common/reusables/components/workflow/workflow.component";
import { WorkflowService } from '../../../../../common/services/workflow.service';
import { ProposalService } from '../../../services/proposal.service';
import { DocumentListComponent } from '../../../../../common/reusables/components/document-list/document-list.component';
import { TranslateModule } from '@ngx-translate/core';


@Component({
  selector: 'app-proposal',
  templateUrl: './proposalItem.component.html',
  styleUrl: './proposalItem.component.scss',
  imports: [PanelModule,
    FluidModule,
    InputTextModule,
    ButtonModule,
    TextareaModule,
    MultiSelectModule,
    SelectModule,
    InputNumberModule,
    StepperModule,
    CardModule,
    StepsModule,
    DatePickerModule,
    FormsModule,
    DatePipe,
    WorkflowComponent,
    DocumentListComponent,
    CheckboxModule,
    TranslateModule,
    BlockUI]
})
export class ProposalItemComponent {

  router = inject( Router );
  activatedRoute = inject( ActivatedRoute );
  proposalService = inject( ProposalService );
  workflowService = inject( WorkflowService );

  recordId : string = '';
  recordData = signal<any>(null);
  stages = signal<MenuItem[] | []>([]);

  entityName = 'Proposal';
  currentStageIndex : number = -1;

  constructor() {
    //load stages
    this.workflowService.getWorkFlowForEntity('internal-proposal').subscribe({
      next: ( data: any ) => {
        let stageArray : [] = data || [];
        if( stageArray )
        {
          this.stages.set( stageArray.map( (item) => {
            return {
              label : item["displayName"],
              name: item["stage"],
              value: item["sequence"]
            };
          } ) );
        }
      }
    });
  }

  ngOnInit() {

    this.activatedRoute.paramMap.subscribe({
      next: (paramMap) => {
        this.recordId = paramMap.get("recordId") || '';

        if( this.recordId != '' )
        {
          this._loadRecordDetails();
        }
      }
    });
  }

  _loadRecordDetails(){

    //fetch record details
    this.proposalService.getProposalById( this.recordId ).subscribe({
      next: (data: any) => {

        //set current stage index
        this.currentStageIndex = this.stages().findIndex(function( item ){
          return item["name"] == data["stage"];
        });

        this.recordData.set( data );
      }
    });

  }

  handleOnStageChange( data: any ){
    //exit condition
    if( data == null || data == '' || data == undefined )
    {
      return;
    }

    this.currentStageIndex = this.stages().findIndex(function( item ){
      return item["name"] == data["stage"];
    });
  }

}
