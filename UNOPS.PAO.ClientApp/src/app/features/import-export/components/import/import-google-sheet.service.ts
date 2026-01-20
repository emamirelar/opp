import {EventEmitter, inject, Injectable, Output} from '@angular/core';
import { ConfigurationService } from '@core/services/configuration';
import { ImportService } from './import.service';
import {ImportDialogService} from './dialog/import-dialog.service';
import {Contact} from '@partnerships/contacts/models/contact.model';
import { Observable, Subject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

declare const google: any;
declare const gapi: any;

@Injectable({
  providedIn: 'root',
})
export class ImportGoogleSheetService {
  private clientId;
  private apiKey;
  private scope = 'https://www.googleapis.com/auth/drive.readonly https://www.googleapis.com/auth/spreadsheets.readonly';
  private oauthToken?: string;
  private tokenExpirationTime?: number;
  private pickerReady = false;
  private sheetsApiReady = false;

  private checkExistingToken(): void {
    try {
      // Check for Google OAuth token in localStorage
      const storedToken = localStorage.getItem('google_oauth_token');
      const storedExpiration = localStorage.getItem('google_oauth_token_expiration');

      if (storedToken && storedExpiration) {
        const expirationTime = parseInt(storedExpiration, 10);
        if (Date.now() < expirationTime) {
          this.oauthToken = storedToken;
          this.tokenExpirationTime = expirationTime;
        } else {
          // Clear expired token
          localStorage.removeItem('google_oauth_token');
          localStorage.removeItem('google_oauth_token_expiration');
        }
      }
    } catch (error) {
      console.warn('Unable to access localStorage for Google OAuth token.', error);
    }
  }

  private isTokenValid(): boolean {
    if (!this.oauthToken || !this.tokenExpirationTime) return false;
    return Date.now() < this.tokenExpirationTime;
  }

  constructor(configService: ConfigurationService) {
    this.clientId = configService.getConfig().googleClientId;
    this.apiKey = configService.getConfig().googleApiKey;
    this.checkExistingToken();
    gapi.load('picker', { callback: this.onPickerApiLoad.bind(this) });
    gapi.load('client', { callback: this.initSheetsAPI.bind(this) });
  }

  private onPickerApiLoad() {
    this.pickerReady = true;
  }

  private initSheetsAPI() {
    gapi.client.init({
      apiKey: this.apiKey,
      discoveryDocs: ['https://sheets.googleapis.com/$discovery/rest?version=v4'],
      scope: this.scope
    }).then(() => {
      this.sheetsApiReady = true;
    }).catch((error: any) => {
      console.error('Google Sheets API initialization error:', error);
    });
  }

  private authenticate(): Observable<void> {
    return new Observable<void>(observer => {
      google.accounts.oauth2
        .initTokenClient({
          client_id: this.clientId,
          scope: this.scope,
          callback: (response: any) => {
            this.oauthToken = response.access_token;
            // Set expiration time to 55 minutes from now (Google tokens typically expire after 1 hour)
            this.tokenExpirationTime = Date.now() + (55 * 60 * 1000);

            // Store token and expiration in localStorage
            if (this.oauthToken) {
              try {
                localStorage.setItem('google_oauth_token', this.oauthToken);
                localStorage.setItem('google_oauth_token_expiration', this.tokenExpirationTime.toString());
              } catch (error) {
                console.warn('Unable to store Google OAuth token in localStorage.', error);
              }
            }
            observer.next();
            observer.complete();
          },
        })
        .requestAccessToken();
    });
  }

  /**
   * Get the Google Client ID used for authentication
   * @returns The Google Client ID
   */
  getClientId(): string {
    return this.clientId;
  }

  private createPicker(): Observable<string> {
    const sheetIdSubject = new Subject<string>();
    
    if (this.pickerReady && this.oauthToken) {
      const pickerBuilder = new google.picker.PickerBuilder();
      pickerBuilder.setOAuthToken(this.oauthToken);
      pickerBuilder.enableFeature(google.picker.Feature.SUPPORT_DRIVES);
      pickerBuilder.setCallback((data: any) => {
        if (data.action === google.picker.Action.PICKED) {
          const selectedSheet = data[google.picker.Response.DOCUMENTS][0];
          sheetIdSubject.next(selectedSheet.id);
          sheetIdSubject.complete();
        } else if (data.action === google.picker.Action.CANCEL) {
          // Handle cancel action by emitting a special value
          sheetIdSubject.next('CANCELED');
          sheetIdSubject.complete();
        }
      });

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

    return sheetIdSubject.asObservable();
  }

  public openPicker(): Observable<string> {
    if (!this.oauthToken || !this.isTokenValid()) {
      return new Observable<string>(subscriber => {
        gapi.load('auth', () => {
          this.authenticate().pipe(
            switchMap(() => this.createPicker())
          ).subscribe({
            next: (sheetId) => subscriber.next(sheetId),
            complete: () => subscriber.complete(),
            error: (error) => subscriber.error(error)
          });
        });
      });
    } else {
      return this.createPicker();
    }
  }
}
