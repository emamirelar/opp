import { EventEmitter, Injectable, Output } from '@angular/core';
import { ConfigurationService } from '../../../../essentials/services/configuration.service';

declare const google: any;
declare const gapi: any;

@Injectable({
  providedIn: 'root',
})
export class DrivePickerService {
  private clientId;
  private scope = 'https://www.googleapis.com/auth/drive.readonly';
  private oauthToken?: string;
  private googleDriveDefaultFolder: string = '';
  private pickerReady = false;
  private acceptedMIMETypes = '';

  @Output() onFilesSelectedEmitter = new EventEmitter<any>();

  constructor(configService: ConfigurationService) {
    this.clientId = configService.getConfig().googleClientId;
    gapi.load('picker', { callback: this.onPickerApiLoad.bind(this) });
  }

  isPickerReady(): boolean {
    return this.pickerReady;
  }

  setAcceptedMIMETypes(acceptedMIMETypes: string) {
    this.acceptedMIMETypes = acceptedMIMETypes;
  }

  private onPickerApiLoad() {
    this.pickerReady = true;
  }

  private authenticate() {
    google.accounts.oauth2
      .initTokenClient({
        client_id: this.clientId,
        scope: this.scope,
        callback: (response: any) => {
          this.oauthToken = response.access_token;
          this.openPicker();
        },
      })
      .requestAccessToken();
  }

  private createPicker() {
    if (this.pickerReady && this.oauthToken) {
      const pickerBuilder = new google.picker.PickerBuilder();
      pickerBuilder.setOAuthToken(this.oauthToken);
      pickerBuilder.enableFeature(google.picker.Feature.SUPPORT_DRIVES);
      pickerBuilder.setCallback(this.pickerCallback.bind(this));
      // pickerBuilder.enableFeature(google.picker.Feature.MULTISELECT_ENABLED);

      //Team Drive
      var multiTeamDrive = new google.picker.DocsView(google.picker.ViewId.DOCS);
      if (multiTeamDrive) {
        multiTeamDrive.setIncludeFolders(true);
        multiTeamDrive.setEnableTeamDrives(true);
        multiTeamDrive.setLabel('Team Drives');
        if (this.acceptedMIMETypes !== '') {
          multiTeamDrive.setMimeTypes(this.acceptedMIMETypes);
        }

        pickerBuilder.addView(multiTeamDrive);
      }

      //Starred
      var starredDOCView = new google.picker.DocsView(google.picker.ViewId.DOCS);
      if (starredDOCView) {
        starredDOCView.setLabel('Starred');
        starredDOCView.setIncludeFolders(false);
        starredDOCView.setOwnedByMe(false);
        starredDOCView.setStarred(true);
        if (this.acceptedMIMETypes !== '') {
          starredDOCView.setMimeTypes(this.acceptedMIMETypes);
        }

        pickerBuilder.addView(starredDOCView);
      }

      //Shared with me
      var sharedWithMeDOCView = new google.picker.DocsView(google.picker.ViewId.DOCS);
      if (sharedWithMeDOCView) {
        sharedWithMeDOCView.setLabel('Shared with me');
        sharedWithMeDOCView.setEnableDrives(false);
        sharedWithMeDOCView.setIncludeFolders(false);
        sharedWithMeDOCView.setOwnedByMe(false);
        if (this.acceptedMIMETypes !== '') {
          sharedWithMeDOCView.setMimeTypes(this.acceptedMIMETypes);
        }

        pickerBuilder.addView(sharedWithMeDOCView);
      }

      var picker = pickerBuilder.build();
      picker.setVisible(true);

      //This is to fix z-index issue on gDrive picker.
      var elements = document.getElementsByClassName('picker-dialog');
      for (var i = 0; i < elements.length; i++) {
        (elements[i] as HTMLElement).style.zIndex = '99999999999999';
      }
    }
  }

  private pickerCallback(data: any) {
    if (data.action === google.picker.Action.PICKED) {
      var selectedDocuments = data[google.picker.Response.DOCUMENTS];

      this.onFilesSelectedEmitter.emit({ processed: false, files: selectedDocuments });
    }
  }

  public openPicker() {
    if (!this.oauthToken) {
      gapi.load('auth', { callback: this.authenticate.bind(this) });
    } else {
      this.createPicker();
    }
  }
}
