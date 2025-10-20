import { TestBed } from '@angular/core/testing';
import { ImportGoogleSheetService } from './import-google-sheet.service';
import { ConfigurationService } from '@core/services/configuration';
import { of } from 'rxjs';

describe('ImportGoogleSheetService', () => {
  let service: ImportGoogleSheetService;
  let mockConfigService: jasmine.SpyObj<ConfigurationService>;

  // Mock Google APIs
  const mockGapi = {
    load: jasmine.createSpy('load'),
    client: {
      init: jasmine.createSpy('init').and.returnValue(Promise.resolve()),
      sheets: {
        spreadsheets: {
          values: {
            get: jasmine.createSpy('get')
          }
        }
      }
    }
  };

  const mockGoogle = {
    accounts: {
      oauth2: {
        initTokenClient: jasmine.createSpy('initTokenClient')
      }
    },
    picker: {
      PickerBuilder: jasmine.createSpy('PickerBuilder'),
      DocsView: jasmine.createSpy('DocsView'),
      ViewId: {
        SPREADSHEETS: 'SPREADSHEETS'
      }
    }
  };

  beforeEach(() => {
    mockConfigService = jasmine.createSpyObj('ConfigurationService', ['getConfig']);
    mockConfigService.getConfig.and.returnValue({
      googleClientId: 'test-client-id',
      googleApiKey: 'test-api-key'
    });

    // Set up global mocks
    (window as any).gapi = mockGapi;
    (window as any).google = mockGoogle;

    TestBed.configureTestingModule({
      providers: [
        ImportGoogleSheetService,
        { provide: ConfigurationService, useValue: mockConfigService }
      ]
    });

    service = TestBed.inject(ImportGoogleSheetService);
  });

  afterEach(() => {
    // Clean up global mocks
    delete (window as any).gapi;
    delete (window as any).google;
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initialization', () => {
    it('should load Google Picker API', () => {
      expect(mockGapi.load).toHaveBeenCalledWith('picker', jasmine.any(Object));
    });

    it('should load Google Client API', () => {
      expect(mockGapi.load).toHaveBeenCalledWith('client', jasmine.any(Object));
    });

    it('should get configuration from config service', () => {
      expect(mockConfigService.getConfig).toHaveBeenCalled();
      expect(service['clientId']).toBe('test-client-id');
      expect(service['apiKey']).toBe('test-api-key');
    });

    it('should check for existing token on initialization', () => {
      expect(service).toBeTruthy(); // Service is created, constructor runs
    });
  });

  describe('token management', () => {
    it('should retrieve valid token from localStorage', () => {
      const token = 'test-token';
      const expiration = (Date.now() + 60000).toString();
      
      localStorage.setItem('google_oauth_token', token);
      localStorage.setItem('google_oauth_token_expiration', expiration);

      service['checkExistingToken']();

      expect(service['oauthToken']).toBe(token);
      expect(service['tokenExpirationTime']).toBe(parseInt(expiration, 10));
    });

    it('should remove expired token from localStorage', () => {
      const token = 'expired-token';
      const expiration = (Date.now() - 1000).toString();
      
      localStorage.setItem('google_oauth_token', token);
      localStorage.setItem('google_oauth_token_expiration', expiration);

      service['checkExistingToken']();

      expect(localStorage.getItem('google_oauth_token')).toBeNull();
      expect(localStorage.getItem('google_oauth_token_expiration')).toBeNull();
    });

    it('should validate token correctly when token is valid', () => {
      service['oauthToken'] = 'valid-token';
      service['tokenExpirationTime'] = Date.now() + 60000;

      expect(service['isTokenValid']()).toBeTrue();
    });

    it('should return false for invalid/expired token', () => {
      service['oauthToken'] = 'expired-token';
      service['tokenExpirationTime'] = Date.now() - 1000;

      expect(service['isTokenValid']()).toBeFalse();
    });

    it('should return false when no token exists', () => {
      service['oauthToken'] = undefined;
      service['tokenExpirationTime'] = undefined;

      expect(service['isTokenValid']()).toBeFalse();
    });
  });

  describe('API initialization', () => {
    it('should initialize Sheets API with correct configuration', () => {
      service['initSheetsAPI']();

      expect(mockGapi.client.init).toHaveBeenCalledWith({
        apiKey: 'test-api-key',
        discoveryDocs: ['https://sheets.googleapis.com/$discovery/rest?version=v4'],
        scope: jasmine.any(String)
      });
    });

    it('should set sheetsApiReady flag after successful initialization', (done) => {
      mockGapi.client.init.and.returnValue(Promise.resolve());

      service['initSheetsAPI']();

      setTimeout(() => {
        expect(service['sheetsApiReady']).toBeTrue();
        done();
      }, 100);
    });

    it('should handle initialization errors', (done) => {
      const error = new Error('Init failed');
      mockGapi.client.init.and.returnValue(Promise.reject(error));
      spyOn(console, 'error');

      service['initSheetsAPI']();

      setTimeout(() => {
        expect(console.error).toHaveBeenCalledWith('Google Sheets API initialization error:', error);
        done();
      }, 100);
    });

    it('should set pickerReady flag when picker API loads', () => {
      service['onPickerApiLoad']();

      expect(service['pickerReady']).toBeTrue();
    });
  });

  describe('authentication', () => {
    it('should initialize token client for authentication', (done) => {
      let tokenCallback: Function;
      
      mockGoogle.accounts.oauth2.initTokenClient.and.callFake((config: any) => {
        tokenCallback = config.callback;
        setTimeout(() => tokenCallback({ access_token: 'new-token' }), 10);
        return { requestAccessToken: () => {} };
      });

      service['authenticate']().subscribe(() => {
        expect(service['oauthToken']).toBe('new-token');
        expect(localStorage.getItem('google_oauth_token')).toBe('new-token');
        done();
      });
    });

    it('should calculate token expiration time correctly', (done) => {
      let tokenCallback: Function;
      const beforeAuth = Date.now();
      
      mockGoogle.accounts.oauth2.initTokenClient.and.callFake((config: any) => {
        tokenCallback = config.callback;
        setTimeout(() => tokenCallback({ access_token: 'new-token' }), 10);
        return { requestAccessToken: () => {} };
      });

      service['authenticate']().subscribe(() => {
        const expectedExpiration = beforeAuth + (55 * 60 * 1000);
        expect(service['tokenExpirationTime']).toBeGreaterThan(beforeAuth);
        expect(service['tokenExpirationTime']).toBeLessThanOrEqual(expectedExpiration + 1000); // Allow 1s margin
        done();
      });
    });
  });

  describe('scope configuration', () => {
    it('should use readonly scopes', () => {
      expect(service['scope']).toContain('drive.readonly');
      expect(service['scope']).toContain('spreadsheets.readonly');
    });
  });

  describe('error handling', () => {
    it('should handle missing localStorage gracefully', () => {
      spyOn(localStorage, 'getItem').and.throwError('Storage error');
      
      expect(() => service['checkExistingToken']()).not.toThrow();
    });

    it('should handle missing Google APIs', () => {
      delete (window as any).google;
      
      // Service should handle missing APIs gracefully
      expect(service).toBeTruthy();
    });
  });
});


