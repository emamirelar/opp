import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ButtonModule } from 'primeng/button';
import { PictureEditorComponent } from './picture-editor/picture-editor.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-picture',
  standalone: true,
  imports: [CommonModule, ButtonModule],
  templateUrl: './picture.component.html',
  providers: [DialogService]
})
export class PictureComponent {
  @Input() imageUrl: string | null = null;
  @Input() altText: string = 'Profile picture';
  @Input() size: 'extra-small' | 'small' | 'medium' | 'large' = 'medium';
  @Input() uploadUrl: string | null = null;
  @Input() disabled: boolean = false;
  @Output() imageChanged = new EventEmitter<string>();

  private dialogRef: DynamicDialogRef | null = null;
  private translateService = inject(TranslateService);

  constructor(private dialogService: DialogService) {}

  getSizeClass(): string {
    switch(this.size) {
      case 'extra-small': return 'w-10 h-10';
      case 'small': return 'w-16 h-16';
      case 'medium': return 'w-24 h-24';
      case 'large': return 'w-32 h-32';
      default: return 'w-24 h-24';
    }
  }

  openPictureEditor(): void {
    this.dialogRef = this.dialogService.open(PictureEditorComponent, {
      header: this.translateService.instant('title.editPicture'),
      width: '40vw',
      breakpoints: { '960px': '95vw' },
      closable: true,
      data: {
        uploadUrl: this.uploadUrl
      }
    });

    this.dialogRef.onClose.subscribe((result: string | undefined) => {
      if (result) {
        this.imageUrl = result;
      }
      this.imageChanged.emit(result);
    });
  }
}
