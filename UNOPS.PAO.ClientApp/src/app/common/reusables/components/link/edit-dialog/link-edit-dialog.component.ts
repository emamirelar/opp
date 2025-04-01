import { Component, EventEmitter, Input, Output, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TranslateModule } from '@ngx-translate/core';
import { EntityType, Link } from '../../../../models/link.model';
import LinkDataService from '../link-data.service';
import {Textarea} from 'primeng/textarea';

@Component({
  selector: 'app-link-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    TranslateModule,
    Textarea
  ],
  templateUrl: './link-edit-dialog.component.html',
  styleUrls: ['./link-edit-dialog.component.scss']
})
export class LinkEditDialogComponent {
  private linkDataService = inject(LinkDataService);

  @Input() visible = false;
  @Input() entityType!: EntityType;
  @Input() entityId!: number;
  @Input() link?: Link;
  @Output() visibleChange = new EventEmitter<boolean>();

  newLink: Link = this.linkDataService.createEmptyLink();

  ngOnChanges() {
    if (this.link) {
      this.newLink = { ...this.link };
    } else {
      this.newLink = this.linkDataService.createEmptyLink();
    }
  }

  saveLink() {
    if (!this.newLink.url) return;
    this.linkDataService.saveLink(this.newLink);
    this.visibleChange.emit(false);
  }

  deleteLink() {
    if (this.link?.id) {
      this.linkDataService.deleteLink(this.link.id);
      this.visibleChange.emit(false);
    }
  }

  close() {
    this.visibleChange.emit(false);
  }
}
