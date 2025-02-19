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
import { ContactService } from '../../../services/contact.service';

@Component({
  selector: 'app-new-contact',
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
  templateUrl: './new-contact.component.html',
  styleUrl: './new-contact.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NewContactComponent implements OnInit, OnDestroy {
  formGroup = new FormGroup({
    salutation: new FormControl('', {
      validators:[Validators.required]
    }),
    firstName: new FormControl(''),
    lastName: new FormControl(null, {
      validators:[Validators.required]
    }),
    email: new FormControl(null, {
      validators:[Validators.required]
    }),
    mobile: new FormControl(null, {
      validators:[Validators.required]
    }),
    middleName: new FormControl(null, {
      validators:[Validators.required]
    }),
    suffix: new FormControl(null, {
      validators:[Validators.required]
    }),
    title: new FormControl(null, {
      validators:[Validators.required]
    }),
    pronouns: new FormControl(null, {
      validators:[Validators.required]
    }),
    department: new FormControl(null, {
      validators:[Validators.required]
    }),
    birthDate: new FormControl(new Date(), {
      validators:[Validators.required]
    }),
    fax: new FormControl(null, {
      validators:[Validators.required]
    }),
    description: new FormControl(null, {
      validators:[Validators.required]
    }),
    phone: new FormControl(null, {
      validators:[Validators.required]
    }),
    otherPhone: new FormControl(null, {
      validators:[Validators.required]
    }),
    assistant: new FormControl(null, {
      validators:[Validators.required]
    }),
    assistantPhone: new FormControl(null, {
      validators:[Validators.required]
    }),
    assistantEmail: new FormControl(null, {
      validators:[Validators.required]
    }),
    status: new FormControl(null, {
      validators:[Validators.required]
    }),
    mailingStreet: new FormControl(null, {
      validators:[Validators.required]
    }),
    mailingStreet2: new FormControl(null, {
      validators:[Validators.required]
    }),
    mailingCity: new FormControl(null, {
      validators:[Validators.required]
    }),
    mailingStateProvince: new FormControl(null, {
      validators:[Validators.required]
    }),
    mailingPostalCode: new FormControl(null, {
      validators:[Validators.required]
    }),
    mailingCountry: new FormControl(null, {
      validators:[Validators.required]
    }),
    discriminator: new FormControl(null, {
      validators:[Validators.required]
    }),
    contactNumber: new FormControl(null, {
      validators:[Validators.required]
    }),
    createdBy: new FormControl(null, {
      validators:[Validators.required]
    }),
    createdDate: new FormControl(new Date(), {
      validators:[Validators.required]
    }),
    lastModifiedBy: new FormControl(null, {
      validators:[Validators.required]
    }),
    lastModifiedDate: new FormControl(new Date(), {
      validators:[Validators.required]
    }),
    isDeleted: new FormControl(null, {
      validators:[Validators.required]
    }),
    deletedBy: new FormControl(null, {
      validators:[Validators.required]
    }),
    deletedDate: new FormControl(null, {
      validators:[Validators.required]
    }),
  });

  cachedDataService = inject(CachedDataService);
  feedbackDialogService = inject(FeedbackDialogService);
  contactService = inject(ContactService);
  translateService = inject(TranslateService);
  languageService = inject(LanguageService);
  cdr = inject( ChangeDetectorRef);

  private langChangeSubscription: Subscription = new Subscription();
  onRecordCreationSuccess = output();

  allContacts = this.cachedDataService.allContacts;
  showValidationFailedError = signal<boolean>(false);

  constructor() {
    //load projects
    //this.cachedDataService.loadContacts();
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

    canSave = true;

    if( canSave === true )
    {
      this.contactService.createContact(this._getRequestPayload()).subscribe({
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
      this.showValidationFailedError.set( false );

      if( this.formGroup.get("firstName")?.invalid )
      {
        this.formGroup.get("firstName")?.markAsDirty();
      }
      result = false;
    }

    return result;
  }

  _getRequestPayload() {
    let valueObj = this.formGroup.value,
      requestJsonObj: any = {},
      projectNumber = "";

    console.log(valueObj);

    for (let key in valueObj) {
      if (valueObj.hasOwnProperty(key)) {
        let indexValue = (valueObj as any)[key];
        requestJsonObj[key] = indexValue || '';
      }
    }

    return requestJsonObj;
  }
}
