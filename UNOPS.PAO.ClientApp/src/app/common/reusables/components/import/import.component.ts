import { ChangeDetectionStrategy, Component, effect, inject, OnInit,  signal } from '@angular/core';
import { DynamicDialogConfig,  } from 'primeng/dynamicdialog';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { ProgressBarModule } from 'primeng/progressbar';
import { CardModule } from 'primeng/card';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageModule } from 'primeng/message';
import { BlockUIModule } from 'primeng/blockui';
import { StepperModule } from 'primeng/stepper';
import { FeedbackDialogService } from '../../../../common/pages/services/feedback-dialog.service';
import {NgForOf, NgClass, JsonPipe} from '@angular/common';
import { ImportDialogService } from './import-dialog.service';


@Component({
  selector: 'app-import',
  standalone: true,
  imports: [
    TranslateModule,
    ButtonModule,
    TableModule,
    InputTextModule,
    DialogModule,
    ProgressBarModule,
    CardModule,
    FormsModule,
    ReactiveFormsModule,
    MessageModule,
    BlockUIModule,
    StepperModule,
    NgForOf,
    NgClass
  ],
  templateUrl: './import.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImportComponent {
  feedbackDialogService = inject(FeedbackDialogService);
  private fb = inject(FormBuilder);
  importDialogService = inject(ImportDialogService);

  // Form template for validation
  contactFormTemplate = this.fb.group({
    salutation: [''],
    firstName: [''],
    middleName: [''],
    lastName: ['', Validators.required],
    suffix: [''],
    title: [''],
    pronouns: [''],
    email: ['', [Validators.required, Validators.email]],
    phone: [''],
    mobile: [''],
    department: [''],
    contactNumber: ['']
  });

  currentStep = signal<number>(1);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string>('');

  // Sample data structure - replace with your actual data model
  importData = signal<any[]>([]);
  selectedRows = signal<any[]>([]);
  validationErrors = signal<Map<number, string[]>>(new Map());

  // Table columns configuration derived from form controls
  columns = [
    { field: 'salutation', header: 'Salutation', required: false },
    { field: 'firstName', header: 'First Name', required: false },
    { field: 'middleName', header: 'Middle Name', required: false },
    { field: 'lastName', header: 'Last Name', required: true },
    { field: 'suffix', header: 'Suffix', required: false },
    { field: 'title', header: 'Title', required: false },
    { field: 'pronouns', header: 'Pronouns', required: false },
    { field: 'email', header: 'Email', required: true },
    { field: 'phone', header: 'Phone', required: false },
    { field: 'mobile', header: 'Mobile', required: false },
    { field: 'department', header: 'Department', required: false },
    { field: 'contactNumber', header: 'Contact Number', required: false }
  ];

  getFieldHeader(fieldName: string): string {
    const column = this.columns.find(col => col.field === fieldName);
    return column ? column.header : fieldName;
  }

  hasErrors(rowIndex: number): boolean {
    return this.validationErrors().has(rowIndex);
  }

  getRowErrors(rowIndex: number): string[] {
    return this.validationErrors().get(rowIndex) || [];
  }
}
