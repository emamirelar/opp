import { afterNextRender, ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, OnDestroy, OnInit, output, signal } from '@angular/core';
import { CachedDataService } from '../../../../../common/services/cached-data.service';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { FeedbackDialogService } from '../../../../../common/pages/services/feedback-dialog.service';
import { PanelModule } from 'primeng/panel';
import { DropdownModule } from "primeng/dropdown"; 
import { DatePickerModule } from 'primeng/datepicker';


//Language translation import
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { LanguageService } from '../../../../../common/services/language.service';
import { Subscription } from 'rxjs/internal/Subscription';

//PrimeNG imports
import { InputTextModule } from 'primeng/inputtext';
import { FloatLabelModule } from 'primeng/floatlabel';
import { DividerModule } from 'primeng/divider';
import { ButtonModule } from 'primeng/button';
import { TextareaModule } from 'primeng/textarea';
import { SelectModule } from 'primeng/select';
import { AutoFocusModule } from 'primeng/autofocus';
import { BlockUI } from 'primeng/blockui';
import { MessageModule } from 'primeng/message';
import { ContactService } from '../../../services/contact.service';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-new-contact',
  imports: [
    TranslateModule,
    InputTextModule,
    FloatLabelModule,
    DropdownModule,
    DatePickerModule,
    ButtonModule,
    TextareaModule,
    PanelModule,
    SelectModule,
    AutoFocusModule,
    BlockUI,
    MessageModule,
    DividerModule,
    CardModule,
    ReactiveFormsModule],
  templateUrl: './new-contact.component.html',
  styleUrl: './new-contact.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NewContactComponent implements OnInit, OnDestroy {
  formGroup = new FormGroup({
    partner: new FormControl(null),
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

  allSalutationsData = this.cachedDataService.allSalutations;
  allStatusData = this.cachedDataService.allStatus;
  allPronounsData = this.cachedDataService.allPronouns;
  allPartners = this.cachedDataService.allPartners;
  showValidationFailedError = signal<boolean>(false);
  maxDate = new Date();

  constructor() {
    //load salutations
    //this.cachedDataService.loadSalutations();
    //this.cachedDataService.loadStatus();
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
      partnerId = "",
      projectNumber = "";

    for (let key in valueObj) {
      if (valueObj.hasOwnProperty(key)) {
        let indexValue = (valueObj as any)[key];
        switch (key) {
          case "partner":
            if (valueObj["partner"] != null && valueObj["partner"] !== undefined) {
              partnerId = valueObj["partner"]["id"];
            }
            requestJsonObj["partnerId"] = partnerId;
            break;

          default:
            requestJsonObj[key] = indexValue || '';
            break;
        }
      }
    }

    return requestJsonObj;
  }
}
