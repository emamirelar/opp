import { EventEmitter, Injectable, Output } from '@angular/core';
import { ConfigurationService } from '../../../../essentials/services/configuration.service';

declare const google: any;
declare const gapi: any 

@Injectable({
  providedIn: 'root'
})
export class DrivePickerService {
  private clientId;
  private scope = 'https://www.googleapis.com/auth/drive.readonly';
  private oauthToken?: string;
  private googleDriveDefaultFolder: string = "";
  private pickerReady = false;

  @Output() onFilesSelectedEmitter = new EventEmitter<any>();

  constructor(configService: ConfigurationService) {
    this.clientId = configService.getConfig().googleClientId;
    gapi.load('picker', { 'callback': this.onPickerApiLoad.bind(this) });
  }

  isPickerReady(): boolean {
    return this.pickerReady;
  }

  private onPickerApiLoad() {
    this.pickerReady = true;
  }

  private authenticate() {
    google.accounts.oauth2.initTokenClient({
      client_id: this.clientId,
      scope: this.scope,
      callback: (response: any) => {
        this.oauthToken = response.access_token;
        this.openPicker();
      }
    }).requestAccessToken();
  }

  private createPicker() {
    if (this.pickerReady && this.oauthToken) {
      const view = new google.picker.DocsView(google.picker.ViewId.DOCS)
      .setIncludeFolders(true)
      .setOwnedByMe(false)
      .setMode(google.picker.DocsViewMode.LIST)
      .setSelectFolderEnabled(false)
      
      if (this.googleDriveDefaultFolder)
        view.setParent(this.googleDriveDefaultFolder);

      const picker = new google.picker.PickerBuilder()
        .enableFeature(google.picker.Feature.NAV_HIDDEN)
        .enableFeature(google.picker.Feature.MULTISELECT_ENABLED)
        .setOAuthToken(this.oauthToken)
        .addView(view)
        .addView(google.picker.ViewId.DOCUMENTS)
        .addView(google.picker.ViewId.PRESENTATIONS)
        .addView(new google.picker.DocsUploadView())
        .setCallback(this.pickerCallback.bind(this))
        .build();
      picker.setVisible(true);
    }
  }

  private pickerCallback(data: any) {
    if (data.action === google.picker.Action.PICKED) {
      var selectedDocuments = data[google.picker.Response.DOCUMENTS];

      this.onFilesSelectedEmitter.emit({processed: false, files: selectedDocuments});
    }
  }

  public openPicker() {
    if (!this.oauthToken)
      gapi.load('auth', { 'callback': this.authenticate.bind(this) });
    else 
      this.createPicker();
  }
}