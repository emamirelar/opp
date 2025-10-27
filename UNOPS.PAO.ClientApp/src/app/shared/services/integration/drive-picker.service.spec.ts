import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { DrivePickerService } from './drive-picker.service';
import { ConfigurationService } from '@core/services/configuration';

describe('DrivePickerService', () => {
  let service: DrivePickerService;
  let mockConfigService: jasmine.SpyObj<ConfigurationService>;

  beforeEach(() => {
    mockConfigService = jasmine.createSpyObj('ConfigurationService', ['getConfig']);
    mockConfigService.getConfig.and.returnValue({ googleClientId: 'test-client-id' } as any);

    // Mock global gapi object
    (window as any).gapi = {
      load: jasmine.createSpy('load').and.callFake((api: string, options: any) => {
        if (options && options.callback) {
          options.callback();
        }
      })
    };

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        DrivePickerService,
        { provide: ConfigurationService, useValue: mockConfigService }
      ]
    });

    service = TestBed.inject(DrivePickerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for Google Drive picker initialization
  // TODO: Add tests for file selection handling
  // TODO: Add tests for authentication with Google Drive
  // TODO: Add tests for picker configuration options
});

