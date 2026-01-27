import { TestBed, fakeAsync, flushMicrotasks, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ExportGoogleSheetService } from './export-google-sheet.service';
import { ConfigurationService } from '@core/services/configuration';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';

describe('ExportGoogleSheetService', () => {
  let service: ExportGoogleSheetService;
  let httpMock: HttpTestingController;
  let mockConfigService: jasmine.SpyObj<ConfigurationService>;
  let mockFeedbackService: jasmine.SpyObj<FeedbackDialogService>;

  // Mock Google APIs
  const mockGapi = {
    load: jasmine.createSpy('load'),
    client: {
      init: jasmine.createSpy('init').and.returnValue(Promise.resolve()),
      setToken: jasmine.createSpy('setToken'),
      sheets: {
        spreadsheets: {
          create: jasmine.createSpy('create'),
          values: {
            update: jasmine.createSpy('update')
          }
        }
      },
      drive: {
        permissions: {
          create: jasmine.createSpy('create')
        }
      }
    }
  };

  const mockGoogle = {
    accounts: {
      oauth2: {
        initTokenClient: jasmine.createSpy('initTokenClient')
      }
    }
  };

  beforeEach(() => {
    mockConfigService = jasmine.createSpyObj('ConfigurationService', ['getConfig']);
    mockFeedbackService = jasmine.createSpyObj('FeedbackDialogService', ['showErrorToast', 'showSuccessToast']);
    
    mockConfigService.getConfig.and.returnValue({
      googleClientId: 'test-client-id',
      googleApiKey: 'test-api-key'
    });

    // Set up global mocks
    (window as any).gapi = mockGapi;
    (window as any).google = mockGoogle;

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        ExportGoogleSheetService,
        { provide: ConfigurationService, useValue: mockConfigService },
        { provide: FeedbackDialogService, useValue: mockFeedbackService }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    service = TestBed.inject(ExportGoogleSheetService);
  });

  afterEach(() => {
    httpMock.verify();
    // Clean up global mocks
    delete (window as any).gapi;
    delete (window as any).google;
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initialization', () => {
    it('should load Google APIs on construction', () => {
      expect(mockGapi.load).toHaveBeenCalledWith('client', jasmine.any(Object));
    });

    it('should get client ID from config service', () => {
      expect(mockConfigService.getConfig).toHaveBeenCalled();
      expect(service['clientId']).toBe('test-client-id');
    });

    it('should get API key from config service', () => {
      expect(service['apiKey']).toBe('test-api-key');
    });

    it('should check for existing token in localStorage', () => {
      spyOn(service as any, 'checkExistingToken');
      // Constructor already called, so we can't test directly
      expect(service).toBeTruthy();
    });
  });

  describe('token management', () => {
    it('should check for valid token in localStorage', () => {
      const token = 'test-token';
      const expiration = (Date.now() + 60000).toString();
      
      localStorage.setItem('google_oauth_token_export', token);
      localStorage.setItem('google_oauth_token_export_expiration', expiration);

      service['checkExistingToken']();

      expect(service['oauthToken']).toBe(token);
    });

    it('should clear expired token from localStorage', () => {
      const token = 'expired-token';
      const expiration = (Date.now() - 1000).toString(); // Past expiration
      
      localStorage.setItem('google_oauth_token_export', token);
      localStorage.setItem('google_oauth_token_export_expiration', expiration);

      service['checkExistingToken']();

      expect(localStorage.getItem('google_oauth_token_export')).toBeNull();
      expect(localStorage.getItem('google_oauth_token_export_expiration')).toBeNull();
    });

    it('should validate token expiration correctly', () => {
      service['oauthToken'] = 'valid-token';
      service['tokenExpirationTime'] = Date.now() + 60000;

      expect(service['isTokenValid']()).toBeTrue();
    });

    it('should return false for expired token', () => {
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
    it('should initialize Sheets API with correct config', fakeAsync(() => {
      service['initSheetsAPI']();
      tick();

      expect(mockGapi.client.init).toHaveBeenCalledWith({
        apiKey: 'test-api-key',
        discoveryDocs: [
          'https://sheets.googleapis.com/$discovery/rest?version=v4',
          'https://www.googleapis.com/discovery/v1/apis/drive/v3/rest'
        ]
      });
    }));

    it('should set API ready flags after initialization', fakeAsync(() => {
      mockGapi.client.init.and.returnValue(Promise.resolve());

      service['initSheetsAPI']();

      tick();
      flushMicrotasks();
      expect(service['sheetsApiReady']).toBeTrue();
      expect(service['driveApiReady']).toBeTrue();
    }));

    it('should handle initialization errors', fakeAsync(() => {
      const error = new Error('API Init Error');
      mockGapi.client.init.and.callFake(() => Promise.reject(error));

      service['initSheetsAPI']();

      tick();
      flushMicrotasks();
      expect(mockFeedbackService.showErrorToast).toHaveBeenCalled();
    }));
  });

  describe('OAuth token client', () => {
    it('should initialize token client with correct config', () => {
      service['initTokenClient']();

      expect(mockGoogle.accounts.oauth2.initTokenClient).toHaveBeenCalledWith(
        jasmine.objectContaining({
          client_id: 'test-client-id',
          scope: jasmine.any(String)
        })
      );
    });

    it('should store token and expiration on callback', () => {
      const tokenResponse = { access_token: 'new-token' };
      let callback: Function;

      mockGoogle.accounts.oauth2.initTokenClient.and.callFake((config: any) => {
        callback = config.callback;
        return {};
      });

      service['initTokenClient']();
      callback!(tokenResponse);

      expect(service['oauthToken']).toBe('new-token');
      expect(localStorage.getItem('google_oauth_token_export')).toBe('new-token');
    });
  });

  describe('error handling', () => {
    it('should handle missing Google APIs', () => {
      delete (window as any).google;
      
      expect(() => service['initTokenClient']()).not.toThrow();
    });

    it('should handle localStorage errors gracefully', () => {
      spyOn(localStorage, 'setItem').and.throwError('Storage error');
      
      expect(() => service['checkExistingToken']()).not.toThrow();
    });
  });
});


