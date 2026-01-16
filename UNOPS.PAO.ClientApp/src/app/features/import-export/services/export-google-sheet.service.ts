import { Injectable, inject } from '@angular/core';
import { ConfigurationService } from '@core/services/configuration';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { Observable, from, of, throwError } from 'rxjs';
import { switchMap, catchError, map } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';

declare const google: any;
declare const gapi: any;

@Injectable({
  providedIn: 'root',
})
export class ExportGoogleSheetService {
  private clientId: string;
  private apiKey: string;
  private scope = 'https://www.googleapis.com/auth/drive https://www.googleapis.com/auth/spreadsheets';
  private oauthToken?: string;
  private tokenExpirationTime?: number;
  private driveApiReady = false;
  private sheetsApiReady = false;
  private http = inject(HttpClient);
  private configService = inject(ConfigurationService);
  private feedbackDialogService = inject(FeedbackDialogService);
  private tokenClient: any;

  constructor() {
    this.clientId = this.configService.getConfig().googleClientId;
    this.apiKey = this.configService.getConfig().googleApiKey;
    this.checkExistingToken();
    this.loadApis();
  }

  private loadApis() {
    gapi.load('client', { callback: this.initSheetsAPI.bind(this) });
  }

  private initSheetsAPI() {
    setTimeout(() => {
      gapi.client.init({
        apiKey: this.apiKey,
        discoveryDocs: [
          'https://sheets.googleapis.com/$discovery/rest?version=v4',
          'https://www.googleapis.com/discovery/v1/apis/drive/v3/rest'
        ],
      }).then(() => {
        this.sheetsApiReady = true;
        this.driveApiReady = true;
        
        // Initialize the token client for OAuth
        this.initTokenClient();
      }).catch((error: any) => {
        console.error('Google API initialization error:', error);
        this.feedbackDialogService.showErrorToast({
          detail: 'Error initializing Google APIs: ' + error.message
        });
      });
    });
  }

  private initTokenClient() {
    if (typeof google !== 'undefined' && google?.accounts?.oauth2) {
      this.tokenClient = google.accounts.oauth2.initTokenClient({
        client_id: this.clientId,
        scope: this.scope,
        callback: (tokenResponse: any) => {
          if (tokenResponse && tokenResponse.access_token) {
            this.oauthToken = tokenResponse.access_token;
            this.tokenExpirationTime = Date.now() + (55 * 60 * 1000);
            
            // Store token and expiration in localStorage
            if (this.oauthToken) {
              try {
                localStorage.setItem('google_oauth_token_export', this.oauthToken as string);
                localStorage.setItem('google_oauth_token_export_expiration', this.tokenExpirationTime.toString());
              } catch (error) {
                console.warn('Unable to store Google OAuth export token in localStorage.', error);
              }
            }
            
            // Set the token for gapi client
            gapi.client.setToken({ access_token: this.oauthToken });
          }
        },
        error_callback: (error: any) => {
          this.feedbackDialogService.showErrorToast({
            detail: 'Error getting OAuth token: ' + error.message
          });
        }
      });
    }
  }

  private checkExistingToken(): void {
    try {
      // Check for Google OAuth token in localStorage
      const storedToken = localStorage.getItem('google_oauth_token_export');
      const storedExpiration = localStorage.getItem('google_oauth_token_export_expiration');

      if (storedToken && storedExpiration) {
        const expirationTime = parseInt(storedExpiration, 10);
        if (Date.now() < expirationTime) {
          this.oauthToken = storedToken;
          this.tokenExpirationTime = expirationTime;
        } else {
          // Clear expired token
          localStorage.removeItem('google_oauth_token_export');
          localStorage.removeItem('google_oauth_token_export_expiration');
        }
      }
    } catch (error) {
      console.warn('Unable to access localStorage for Google OAuth export token.', error);
    }
  }

  private isTokenValid(): boolean {
    if (!this.oauthToken || !this.tokenExpirationTime) return false;
    return Date.now() < this.tokenExpirationTime;
  }

  private authenticate(): Observable<void> {
    return new Observable<void>(observer => {
      if (!this.tokenClient) {
        this.initTokenClient();
      }

      if (this.tokenClient) {
        this.tokenClient.requestAccessToken({
          prompt: 'consent'  // Force the consent screen to ensure user permissions
        });

        // Create an interval to check if the token has been obtained
        const checkTokenInterval = setInterval(() => {
          if (this.oauthToken) {
            clearInterval(checkTokenInterval);
            observer.next();
            observer.complete();
          }
        }, 500);

        // Add a timeout to avoid infinite waiting
        setTimeout(() => {
          clearInterval(checkTokenInterval);
          if (!this.oauthToken) {
            observer.error(new Error('Authentication timed out'));
          }
        }, 60000); // 1 minute timeout
      } else {
        observer.error(new Error('Token client is not initialized'));
      }
    });
  }

  private ensureAuthenticated(): Observable<void> {
    if (!this.isTokenValid()) {
      return this.authenticate();
    }
    
    // Set both the token AND ensure API key is properly configured
    if (this.oauthToken) {
      console.log('Setting OAuth token for GAPI client');
      gapi.client.setToken({ 
        access_token: this.oauthToken
      });
      
      // Ensure API key is also set (redundant but ensures consistency)
      gapi.client.setApiKey(this.apiKey);
    }
    
    return of(undefined);
  }

  /**
   * Splits a large cell value into multiple parts if it exceeds Google Sheets' 50,000 character limit
   * @param value The cell value to split
   * @returns Array of split values (single item if no split needed)
   */
  private splitLargeCellValue(value: any): string[] {
    const MAX_CELL_LENGTH = 49000; // Use 49,000 to have a safety margin
    
    if (typeof value !== 'string' || value.length <= MAX_CELL_LENGTH) {
      return [value ?? ''];
    }
    
    // Split the large string into chunks
    const parts: string[] = [];
    let remainingText = value;
    
    while (remainingText.length > 0) {
      parts.push(remainingText.substring(0, MAX_CELL_LENGTH));
      remainingText = remainingText.substring(MAX_CELL_LENGTH);
    }
    
    console.warn(`Large cell split into ${parts.length} parts (original length: ${value.length} characters)`);
    return parts;
  }

  /**
   * Processes data to handle large cells by splitting them across multiple columns
   * @param data Original data array
   * @returns Processed data with expanded headers and split cell values
   */
  private processDataWithLargeCells<T extends object>(data: T[]): { headers: string[], values: any[][] } {
    if (data.length === 0) {
      return { headers: [], values: [] };
    }

    const originalHeaders = Object.keys(data[0]);
    const expandedHeaders: string[] = [];
    const headerSplitMap = new Map<string, number>(); // Track how many parts each header needs

    // First pass: determine which headers need splitting and how many parts
    data.forEach(item => {
      originalHeaders.forEach(header => {
        const cellValue = item[header as keyof T];
        const parts = this.splitLargeCellValue(cellValue);
        const currentMax = headerSplitMap.get(header) || 1;
        headerSplitMap.set(header, Math.max(currentMax, parts.length));
      });
    });

    // Build expanded headers
    originalHeaders.forEach(header => {
      const partCount = headerSplitMap.get(header) || 1;
      if (partCount === 1) {
        expandedHeaders.push(header);
      } else {
        for (let i = 1; i <= partCount; i++) {
          expandedHeaders.push(`${header}_Part${i}`);
        }
      }
    });

    // Second pass: build the data rows with split values
    const processedValues: any[][] = data.map(item => {
      const row: any[] = [];
      
      originalHeaders.forEach(header => {
        const cellValue = item[header as keyof T];
        const parts = this.splitLargeCellValue(cellValue);
        const partCount = headerSplitMap.get(header) || 1;
        
        // Add all parts, padding with empty strings if needed
        for (let i = 0; i < partCount; i++) {
          row.push(parts[i] || '');
        }
      });
      
      return row;
    });

    return { headers: expandedHeaders, values: processedValues };
  }

  private createAndPopulateSheet<T extends object>(data: T[], fileName: string): Observable<{ id: string, url: string }> {
    if (!this.sheetsApiReady || !this.driveApiReady) {
      return throwError(() => new Error('Google APIs not loaded. Please try again.'));
    }

    console.log('Creating spreadsheet with name:', fileName);
    console.log('Data rows count:', data.length);

    // Create a new spreadsheet
    return from(gapi.client.sheets.spreadsheets.create({
      "resource": {
        "properties": {
          "title": fileName
        },
        "sheets": [
          {
            "data": []
          }
        ]
      }
    })).pipe(
      catchError(error => {
        console.error('Error creating spreadsheet:', error);
        
        // Handle 401 Unauthorized errors by attempting to re-authenticate
        if (error && error.status === 401) {
          console.log('Got 401 error, attempting re-authentication');
          // Clear existing token and force re-authentication
          this.oauthToken = undefined;
          localStorage.removeItem('google_oauth_token_export');
          localStorage.removeItem('google_oauth_token_export_expiration');
          
          return this.authenticate().pipe(
            switchMap(() => this.createAndPopulateSheet(data, fileName))
          );
        }
        return throwError(() => error);
      }),
      switchMap((response: any) => {
        const spreadsheetId = response.result.spreadsheetId;
        const spreadsheetUrl = response.result.spreadsheetUrl;

        console.log('Spreadsheet created successfully:', spreadsheetId);

        // Process data to handle large cells
        const { headers, values: dataRows } = this.processDataWithLargeCells(data);
        
        // Track if any cells were split
        const originalHeaders = data.length > 0 ? Object.keys(data[0]) : [];
        const hasSplitCells = headers.length > originalHeaders.length;

        // Prepare values array with headers and data
        const values = [headers, ...dataRows];

        // Show info if any cells were split
        if (hasSplitCells) {
          this.feedbackDialogService.showInfoToast({
            summary: 'Large Content Split',
            detail: 'Some cells exceeded 49,000 characters and were split across multiple columns to preserve all data.',
            life: 8000
          });
        }

        console.log('Updating spreadsheet with data, headers:', headers);
        console.log('Total columns (including split):', headers.length);

        // Update the spreadsheet with data
        return from(gapi.client.sheets.spreadsheets.values.update({
          spreadsheetId: spreadsheetId,
          range: 'Sheet1!A1',
          valueInputOption: 'RAW',
          resource: {
            values: values
          }
        })).pipe(
          map(() => ({ id: spreadsheetId, url: spreadsheetUrl })),
          catchError(error => {
            console.error('Error updating spreadsheet data:', error);
            
            // If we get an error here, try to handle 401 errors by re-authenticating
            if (error && error.status === 401) {
              console.log('Got 401 error during data update, attempting re-authentication');
              // Clear existing token and force re-authentication
              this.oauthToken = undefined;
              localStorage.removeItem('google_oauth_token_export');
              localStorage.removeItem('google_oauth_token_export_expiration');
              
              return this.authenticate().pipe(
                switchMap(() => {
                  // After re-authentication, try the entire operation again
                  return this.createAndPopulateSheet(data, fileName);
                })
              );
            }
            return throwError(() => error);
          })
        );
      })
    );
  }

  /**
   * Creates a new Google Sheet with the provided data
   * @param data Array of data objects to export
   * @param fileName Name of the sheet to create
   * @returns Observable with sheet ID and URL
   */
  public exportToSheet<T extends object>(data: T[], fileName: string): Observable<{ id: string, url: string }> {
    // Always force authentication first before attempting to create a sheet
    return this.ensureAuthenticated().pipe(
      switchMap(() => this.createAndPopulateSheet(data, fileName)),
      catchError(error => {
        this.feedbackDialogService.showErrorToast({
          detail: 'Error exporting to Google Sheet: ' + (error.message || 'Authentication failed')
        });
        return throwError(() => error);
      })
    );
  }
} 
