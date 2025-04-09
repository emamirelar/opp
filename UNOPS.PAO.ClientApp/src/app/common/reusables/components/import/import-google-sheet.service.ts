import {EventEmitter, inject, Injectable, Output} from '@angular/core';
import { ConfigurationService } from '../../../../essentials/services/configuration.service';
import { ImportService } from './import.service';
import {ImportDialogService} from './import-dialog.service';
import {Contact} from '../../../../features/internal/models/contact.model';

declare const google: any;
declare const gapi: any;

@Injectable({
  providedIn: 'root',
})
export class ImportGoogleSheetService {
  private clientId;
  private scope = 'https://www.googleapis.com/auth/drive.readonly https://www.googleapis.com/auth/spreadsheets.readonly';
  private oauthToken?: string;
  private pickerReady = false;
  private sheetsApiReady = false;

  private data : any[] = [];
  private importDialogService = inject(ImportDialogService);


  constructor(
    configService: ConfigurationService,
    private importService: ImportService
  ) {
    this.clientId = configService.getConfig().googleClientId;
    gapi.load('picker', { callback: this.onPickerApiLoad.bind(this) });
    gapi.load('client', { callback: this.initSheetsAPI.bind(this) });
  }

  private onPickerApiLoad() {
    this.pickerReady = true;
  }

  private initSheetsAPI() {
    gapi.client.init({
      apiKey: this.clientId,
      discoveryDocs: ['https://sheets.googleapis.com/$discovery/rest?version=v4'],
      scope: this.scope
    }).then(() => {
      this.sheetsApiReady = true;
    });
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

      // Only show Google Sheets
      const sheetsView = new google.picker.DocsView(google.picker.ViewId.SPREADSHEETS);
      pickerBuilder.addView(sheetsView);

      // My Drive sheets (with explicit My Drive view)
      const myDriveView = new google.picker.DocsView(google.picker.ViewId.SPREADSHEETS);
      myDriveView.setLabel('My Drive');
      myDriveView.setOwnedByMe(true);
      pickerBuilder.addView(myDriveView);

      // Shared sheets
      const sharedSheetsView = new google.picker.DocsView(google.picker.ViewId.SPREADSHEETS);
      sharedSheetsView.setLabel('Shared with me');
      sharedSheetsView.setOwnedByMe(false);
      pickerBuilder.addView(sharedSheetsView);

      // Team Drive sheets
      const teamDriveView = new google.picker.DocsView(google.picker.ViewId.SPREADSHEETS);
      teamDriveView.setIncludeFolders(true);
      teamDriveView.setEnableTeamDrives(true);
      teamDriveView.setLabel('Team Drives');
      pickerBuilder.addView(teamDriveView);

      const picker = pickerBuilder.build();
      picker.setVisible(true);

      // Fix z-index issue
      const elements = document.getElementsByClassName('picker-dialog');
      for (let i = 0; i < elements.length; i++) {
        (elements[i] as HTMLElement).style.zIndex = '99999999999999';
      }
    }
  }

  private pickerCallback(data: any) {
    if (data.action === google.picker.Action.PICKED) {
      const selectedSheet = data[google.picker.Response.DOCUMENTS][0];
      this.loadSheetData(selectedSheet.id);
    }
  }

  private loadSheetData(sheetId: string) {
    this.importService.analyzeFile(sheetId, 'bulk_contact_action')
      .subscribe({
        next: (response: any) => {
          const parsedRecords = JSON.parse(response.records);
          this.importDialogService.setData(parsedRecords);
        },
        error: (error) => {
          console.error('Error analyzing Google Sheet:', error);
          // TODO : tooltip
        }
      });
  }

  public openPicker() {
    if (!this.oauthToken) {
      gapi.load('auth', { callback: this.authenticate.bind(this) });
    } else {
      this.createPicker();
    }
  }
}
