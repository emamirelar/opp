import { afterNextRender, ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, output, signal } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { FundingOpportunityService } from '../../../services/fundingOpportunity.service';

//Language translation import
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../../../common/services/language.service';
import { Subscription } from 'rxjs/internal/Subscription';

//PrimeNG imports
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { AutoFocusModule } from 'primeng/autofocus';
import { BlockUI } from 'primeng/blockui';
import { MessageModule } from 'primeng/message';


@Component({
  selector: 'app-newFundingOpportunity',
  imports: [
    TranslateModule,
    InputTextModule,
    ButtonModule,
    TextareaModule,
    SelectModule,
    AutoFocusModule,
    BlockUI,
    MessageModule,
    ReactiveFormsModule],
  templateUrl: './newFundingOpportunity.component.html',
  styleUrl: './newFundingOpportunity.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NewFundingOpportunityComponent implements OnInit, OnDestroy {

  formGroup = new FormGroup({
      name: new FormControl('', {
        validators:[Validators.required]
      }),
      description: new FormControl(''),
      project: new FormControl(null, {
        validators:[Validators.required]
      })
    });

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  fundingOpportunityService = inject(FundingOpportunityService);
  translateService = inject(TranslateService);
  languageService = inject(LanguageService);
  cdr = inject( ChangeDetectorRef);

  private langChangeSubscription: Subscription = new Subscription();
  onRecordCreationSuccess = output();

  allProjects = this.cachedDataService.allProjects;
  showValidationFailedError = signal<boolean>(false);

  constructor() {
    //load projects
    this.cachedDataService.loadProjects();
  }

  ngOnDestroy(): void {
    this.langChangeSubscription?.unsubscribe();
  }

  ngOnInit() {
    this.langChangeSubscription = this.languageService.translationService.onLangChange.subscribe(() => {
      this.cdr.detectChanges();
    });
  }

  _handleOnSaveClick(){
    let canSave = this._validate();

    if( canSave === true )
    {
      this.fundingOpportunityService.createFundingOpportunity(this._getRequestPayload()).subscribe({
        next: (data: any) => {
          this.feedbackDialogService.showSuccessToast({ detail: 'Record created successfully!' });
          this.onRecordCreationSuccess.emit(data);
        }
      });
    }
  }

  _validate(){
    let result = true;

    if( this.formGroup.status == "INVALID" )
    {
      this.showValidationFailedError.set( true );

      if( this.formGroup.get("name")?.invalid )
      {
        this.formGroup.get("name")?.markAsDirty();
      }

      if( this.formGroup.get("project")?.invalid )
      {
        this.formGroup.get("project")?.markAsDirty();
      }
      result = false;
    }

    return result;
  }

  _getRequestPayload() {
    let valueObj = this.formGroup.value,
      requestJsonObj: any = {},
      projectNumber = "";

    for (let key in valueObj) {
      if (valueObj.hasOwnProperty(key)) {
        let indexValue = (valueObj as any)[key];
        switch (key) {
          case "project":
            if (valueObj["project"] != null && valueObj["project"] !== undefined) {
              projectNumber = valueObj["project"]["projectNumber"];
            }
            requestJsonObj["projectNumber"] = projectNumber;
            break;

          default:
            requestJsonObj[key] = indexValue;
            break;
        }
      }
    }

    return requestJsonObj;
  }

}
