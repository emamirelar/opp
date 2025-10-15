import { Component, EventEmitter, Input, Output, inject, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TranslateModule } from '@ngx-translate/core';
import { EntityType, Link } from '../../../../models/link.model';
import LinkDataService from '../link-data.service';
import { Textarea } from 'primeng/textarea';

@Component({
  selector: 'app-link-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    TranslateModule,
    Textarea
  ],
  templateUrl: './link-edit-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LinkEditDialogComponent implements OnInit {
  private linkDataService = inject(LinkDataService);
  private fb = inject(FormBuilder);

  @Input() visible = false;
  @Input() entityType!: EntityType;
  @Input() entityId!: number;
  @Input() link?: Link;
  @Output() visibleChange = new EventEmitter<boolean>();

  form!: FormGroup;

  ngOnInit() {
    this.initForm();
  }

  ngOnChanges() {
    if (this.form) {
      if (this.link) {
        this.form.patchValue({
          url: this.link.url,
          name: this.link.name
        });
      } else {
        this.form.reset();
      }
    }
  }

  private initForm() {
    this.form = this.fb.group({
      url: ['', [Validators.required, Validators.maxLength(2000)]],
      name: ['', [Validators.maxLength(2000)]]
    });
  }

  saveLink() {
    if (this.form.invalid) return;

    const formValue = this.form.value;
    const linkToSave: Link = {
      ...this.link,
      entity: this.entityType,
      entityId: this.entityId,
      url: formValue.url,
      name: formValue.name
    };

    this.linkDataService.saveLink(linkToSave);
    this.close();
  }

  deleteLink() {
    if (this.link?.id) {
      this.linkDataService.deleteLink(this.link.id);
      this.close();
    }
  }

  close() {
    this.form.reset();
    this.visibleChange.emit(false);
  }
}
