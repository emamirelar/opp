import { Component, input } from '@angular/core';

//Prime NG
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { GDriveAddLinkComponent } from './addlink/document-gdrive-addlink.component';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-document-gdrive',
  imports: [ButtonModule, DialogModule, GDriveAddLinkComponent, TranslateModule],
  templateUrl: './document-gdrive.component.html',
  styleUrl: './document-gdrive.component.scss',
})
export class GDriveDocumentComponent {
  entityName = input<string>('');
  entityId = input<string>('');
  appDocumentRef = input<any>(null);
  acceptedMIMETypes = input<string>('');
  showAddLinkDialog: boolean = false;

  handleOnSelectDriveBtnClick() {
    this.showAddLinkDialog = true;
  }

  handleOnAddLinkBtnClick(addLinkComponent: any) {
    addLinkComponent.addLinks();
  }

  handleOnAddLinkDialogClose(addLinkComponent: any) {
    addLinkComponent.clear();
  }

  handleOnAddLinkSuccess() {
    this.showAddLinkDialog = false;
    if (this.appDocumentRef() !== null) {
      this.appDocumentRef().load();
    }
  }
}
