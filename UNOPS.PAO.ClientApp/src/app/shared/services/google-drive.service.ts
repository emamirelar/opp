/**
 * @fileoverview Google Drive service for file conversion and management
 * @author UNOPS Opportunity+ System Development Team
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ConfigurationService } from '@core/services/configuration/configuration.service';

/**
 * @class GoogleDriveService
 * @description Service for interacting with Google Drive API, primarily for converting Office files to PDF
 * @since 1.0.0
 */
@Injectable({
  providedIn: 'root'
})
export class GoogleDriveService {
  private readonly http = inject(HttpClient);
  private readonly configService = inject(ConfigurationService);
  
  // Google API configuration (loaded from backend)
  private CLIENT_ID = '';
  private API_KEY = '';
  private readonly SCOPES = 'https://www.googleapis.com/auth/drive.file';
  
  // Google API loaded flag
  private gapiLoaded = false;
  private gisLoaded = false;
  private tokenClient: any = null;
  private accessToken: string | null = null;
  
  // Popup blocked detection
  private popupBlocked = false;
  
  // Config loaded flag
  private configLoaded = false;

  constructor() {
    // TEMPORARY: Hardcoded credentials for testing
    this.CLIENT_ID = '1069310298210-dprrg7ts1lk8td81t4oj9q8sje28sh0f.apps.googleusercontent.com';
    this.API_KEY = 'AIzaSyCgblVxknwJuc8FB9o4J_Bg5oQpMSFxnMI';
    this.configLoaded = true;
    console.log('🔧 TEMPORARY: Using hardcoded Google API credentials for testing');
    
    // Load configuration from backend (will override if available)
    this.loadConfiguration();
    
    // Load Google API scripts
    this.loadGoogleAPIs();
  }
  
  /**
   * @description Load Google API configuration from backend
   * @returns {void}
   * @private
   */
  private loadConfiguration(): void {
    const config = this.configService.getConfig();
    if (config && config.GoogleClientId && config.GoogleApiKey) {
      this.CLIENT_ID = config.GoogleClientId;
      this.API_KEY = config.GoogleApiKey;
      this.configLoaded = true;
    }
  }

  /**
   * @description Load Google API scripts dynamically
   * @returns {void}
   */
  private loadGoogleAPIs(): void {
    // Load GAPI
    const gapiScript = document.createElement('script');
    gapiScript.src = 'https://apis.google.com/js/api.js';
    gapiScript.async = true;
    gapiScript.defer = true;
    gapiScript.onload = () => {
      this.gapiLoaded = true;
    };
    document.body.appendChild(gapiScript);

    // Load GIS
    const gisScript = document.createElement('script');
    gisScript.src = 'https://accounts.google.com/gsi/client';
    gisScript.async = true;
    gisScript.defer = true;
    gisScript.onload = () => {
      this.gisLoaded = true;
    };
    document.body.appendChild(gisScript);
  }

  /**
   * @description Initialize Google Drive authentication
   * @returns {Observable<boolean>} Observable that emits true if auth is successful
   */
  public initializeAuth(): Observable<boolean> {
    return from(this.initializeAuthAsync());
  }

  /**
   * @description Initialize authentication asynchronously with retry logic for configuration loading
   * @returns {Promise<boolean>}
   */
  private async initializeAuthAsync(): Promise<boolean> {
    // Wait for configuration to be available (with retries)
    const maxRetries = 10; // 10 retries
    const retryDelay = 300; // 300ms between retries
    
    for (let attempt = 0; attempt < maxRetries; attempt++) {
      this.loadConfiguration();
      
      if (this.configLoaded && this.CLIENT_ID && this.API_KEY) {
        console.log(`✅ Google Drive API configuration loaded successfully (attempt ${attempt + 1})`);
        break;
      }
      
      if (attempt < maxRetries - 1) {
        console.log(`⏳ Waiting for configuration to load... (attempt ${attempt + 1}/${maxRetries})`);
        await new Promise(resolve => setTimeout(resolve, retryDelay));
      }
    }
    
    // Final check after retries
    if (!this.configLoaded) {
      console.error('❌ Google API configuration not available after retries');
      console.error('CLIENT_ID:', this.CLIENT_ID ? 'Present' : 'Missing');
      console.error('API_KEY:', this.API_KEY ? 'Present' : 'Missing');
      console.error('Make sure the backend configuration endpoint is working and returning Google API credentials');
      return false;
    }
    
    // Validate configuration
    if (!this.CLIENT_ID || !this.API_KEY) {
      console.error('❌ Google API credentials are missing or empty after retries');
      console.error('CLIENT_ID:', this.CLIENT_ID || '(empty)');
      console.error('API_KEY:', this.API_KEY ? '(present but check if valid)' : '(empty)');
      return false;
    }

    console.log('✅ Proceeding with Google Drive authentication initialization');

    // Wait for APIs to load
    await this.waitForAPIs();

    try {
      // Initialize GAPI client
      await this.initializeGapiClient();
      
      // Initialize GIS token client
      this.initializeGisClient();
      
      console.log('✅ Google Drive auth initialized successfully');
      return true;
    } catch (error) {
      console.error('❌ Failed to initialize Google Drive auth:', error);
      return false;
    }
  }

  /**
   * @description Wait for Google APIs to load
   * @returns {Promise<void>}
   */
  private waitForAPIs(): Promise<void> {
    return new Promise((resolve) => {
      const checkInterval = setInterval(() => {
        if (this.gapiLoaded && this.gisLoaded) {
          clearInterval(checkInterval);
          resolve();
        }
      }, 100);
    });
  }

  /**
   * @description Initialize GAPI client
   * @returns {Promise<void>}
   */
  private async initializeGapiClient(): Promise<void> {
    return new Promise((resolve, reject) => {
      (window as any).gapi.load('client', async () => {
        try {
          await (window as any).gapi.client.init({
            apiKey: this.API_KEY,
            discoveryDocs: ['https://www.googleapis.com/discovery/v1/apis/drive/v3/rest'],
          });
          resolve();
        } catch (error) {
          reject(error);
        }
      });
    });
  }

  /**
   * @description Initialize GIS client
   * @returns {void}
   */
  private initializeGisClient(): void {
    this.tokenClient = (window as any).google.accounts.oauth2.initTokenClient({
      client_id: this.CLIENT_ID,
      scope: this.SCOPES,
      callback: (response: any) => {
        if (response.access_token) {
          this.accessToken = response.access_token;
          (window as any).gapi.client.setToken({ access_token: this.accessToken });
        }
      },
    });
  }

  /**
   * @description Request access token
   * @returns {Promise<boolean>}
   */
  private async requestAccessToken(): Promise<boolean> {
    return new Promise((resolve) => {
      if (!this.tokenClient) {
        resolve(false);
        return;
      }

      this.tokenClient.callback = (response: any) => {
        if (response.error !== undefined) {
          console.error('Token request error:', response);
          resolve(false);
          return;
        }
        
        this.accessToken = response.access_token;
        (window as any).gapi.client.setToken({ access_token: this.accessToken });
        resolve(true);
      };

      // Check if we already have a token
      if (this.accessToken) {
        resolve(true);
        return;
      }

      // Request token
      try {
        this.tokenClient.requestAccessToken({ prompt: 'consent' });
      } catch (error) {
        console.error('Failed to request access token:', error);
        this.popupBlocked = true;
        resolve(false);
      }
    });
  }

  /**
   * @description Check if file is Microsoft Office file
   * @param {string} mimeType - File MIME type
   * @returns {boolean}
   */
  public isMicrosoftOfficeFile(mimeType: string): boolean {
    const officeMimeTypes = [
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document', // .docx
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',       // .xlsx
      'application/vnd.openxmlformats-officedocument.presentationml.presentation', // .pptx
      'application/msword',                                                       // .doc
      'application/vnd.ms-excel',                                                 // .xls
      'application/vnd.ms-powerpoint',                                            // .ppt
    ];
    
    return officeMimeTypes.includes(mimeType);
  }
  
  /**
   * @description Check if file is Google Workspace file
   * @param {string} mimeType - File MIME type
   * @returns {boolean}
   */
  public isGoogleWorkspaceFile(mimeType: string): boolean {
    const workspaceMimeTypes = [
      'application/vnd.google-apps.document',      // Google Docs
      'application/vnd.google-apps.spreadsheet',   // Google Sheets
      'application/vnd.google-apps.presentation',  // Google Slides
    ];
    
    return workspaceMimeTypes.includes(mimeType);
  }
  
  /**
   * @description Check if file needs conversion to PDF
   * @param {string} mimeType - File MIME type
   * @returns {boolean}
   */
  public needsPdfConversion(mimeType: string): boolean {
    return this.isMicrosoftOfficeFile(mimeType) || this.isGoogleWorkspaceFile(mimeType);
  }

  /**
   * @description Convert local Office file to PDF using Google Drive
   * @param {File} file - Office file to convert
   * @returns {Observable<{name: string, data: string, mimeType: string}>}
   */
  public convertLocalOfficeFileToPdf(file: File): Observable<{name: string, data: string, mimeType: string}> {
    return from(this.convertLocalOfficeFileToPdfAsync(file));
  }
  
  /**
   * @description Export Google Drive file as PDF
   * @param {string} fileId - Google Drive file ID
   * @param {string} fileName - Original file name
   * @returns {Observable<{name: string, data: string, mimeType: string}>}
   */
  public exportDriveFileAsPdf(fileId: string, fileName: string): Observable<{name: string, data: string, mimeType: string}> {
    return from(this.exportDriveFileAsPdfAsync(fileId, fileName));
  }
  
  /**
   * @description Export Google Drive file as PDF asynchronously
   * @param {string} fileId - Google Drive file ID
   * @param {string} fileName - Original file name
   * @returns {Promise<{name: string, data: string, mimeType: string}>}
   */
  private async exportDriveFileAsPdfAsync(fileId: string, fileName: string): Promise<{name: string, data: string, mimeType: string}> {
    try {
      // Ensure we have access token
      if (!this.accessToken) {
        const hasToken = await this.requestAccessToken();
        if (!hasToken) {
          throw new Error('Failed to get Google Drive access token. Please authorize the application.');
        }
      }

      // Export the Drive file as PDF
      const pdfData = await this.exportFileAsPdf(fileId);
      
      // Generate PDF filename
      const pdfFileName = fileName.replace(/\.[^/.]+$/, '') + '.pdf';
      
      return {
        name: pdfFileName,
        data: pdfData,
        mimeType: 'application/pdf'
      };
    } catch (error: any) {
      console.error('Error exporting Drive file to PDF:', error);
      throw new Error(`Failed to export "${fileName}" to PDF: ${error.message || 'Unknown error'}`);
    }
  }

  /**
   * @description Convert local Office file to PDF asynchronously
   * @param {File} file - Office file to convert
   * @returns {Promise<{name: string, data: string, mimeType: string}>}
   */
  private async convertLocalOfficeFileToPdfAsync(file: File): Promise<{name: string, data: string, mimeType: string}> {
    try {
      // Ensure we have access token
      if (!this.accessToken) {
        const hasToken = await this.requestAccessToken();
        if (!hasToken) {
          throw new Error('Failed to get Google Drive access token. Please authorize the application.');
        }
      }

      // Step 1: Upload file to Google Drive
      const uploadedFile = await this.uploadFileToDrive(file);
      
      // Step 2: Export as PDF
      const pdfData = await this.exportFileAsPdf(uploadedFile.id);
      
      // Step 3: Delete the temporary file from Google Drive
      await this.deleteFileFromDrive(uploadedFile.id);
      
      // Generate PDF filename
      const pdfFileName = file.name.replace(/\.[^/.]+$/, '') + '.pdf';
      
      return {
        name: pdfFileName,
        data: pdfData,
        mimeType: 'application/pdf'
      };
    } catch (error: any) {
      console.error('Error converting Office file to PDF:', error);
      throw new Error(`Failed to convert "${file.name}" to PDF: ${error.message || 'Unknown error'}`);
    }
  }

  /**
   * @description Upload file to Google Drive
   * @param {File} file - File to upload
   * @returns {Promise<{id: string, name: string}>}
   */
  private async uploadFileToDrive(file: File): Promise<{id: string, name: string}> {
    const metadata = {
      name: file.name,
      mimeType: file.type
    };

    const form = new FormData();
    form.append('metadata', new Blob([JSON.stringify(metadata)], { type: 'application/json' }));
    form.append('file', file);

    const response = await fetch('https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.accessToken}`
      },
      body: form
    });

    if (!response.ok) {
      throw new Error(`Failed to upload file to Google Drive: ${response.statusText}`);
    }

    const result = await response.json();
    return {
      id: result.id,
      name: result.name
    };
  }

  /**
   * @description Export file as PDF
   * @param {string} fileId - Google Drive file ID
   * @returns {Promise<string>} Base64 encoded PDF data
   */
  private async exportFileAsPdf(fileId: string): Promise<string> {
    const response = await fetch(
      `https://www.googleapis.com/drive/v3/files/${fileId}/export?mimeType=application/pdf`,
      {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${this.accessToken}`
        }
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to export file as PDF: ${response.statusText}`);
    }

    const blob = await response.blob();
    return await this.blobToBase64(blob);
  }

  /**
   * @description Delete file from Google Drive
   * @param {string} fileId - Google Drive file ID
   * @returns {Promise<void>}
   */
  private async deleteFileFromDrive(fileId: string): Promise<void> {
    try {
      const response = await fetch(
        `https://www.googleapis.com/drive/v3/files/${fileId}`,
        {
          method: 'DELETE',
          headers: {
            'Authorization': `Bearer ${this.accessToken}`
          }
        }
      );

      if (!response.ok && response.status !== 404) {
        console.warn(`Failed to delete temporary file from Google Drive: ${response.statusText}`);
      }
    } catch (error) {
      console.warn('Failed to delete temporary file:', error);
      // Don't throw error - this is cleanup
    }
  }

  /**
   * @description Convert Blob to base64
   * @param {Blob} blob - Blob to convert
   * @returns {Promise<string>}
   */
  private blobToBase64(blob: Blob): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        const result = reader.result as string;
        // Remove data URL prefix
        const base64 = result.split(',')[1];
        resolve(base64);
      };
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  }

  /**
   * @description Check if popup was blocked
   * @returns {boolean}
   */
  public isPopupBlocked(): boolean {
    return this.popupBlocked;
  }

  /**
   * @description Reset popup blocked flag
   * @returns {void}
   */
  public resetPopupBlockedFlag(): void {
    this.popupBlocked = false;
  }
}

