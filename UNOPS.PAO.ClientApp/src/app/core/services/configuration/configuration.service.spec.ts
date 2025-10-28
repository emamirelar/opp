import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ConfigurationService } from './configuration.service';

describe('ConfigurationService', () => {
  let service: ConfigurationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ConfigurationService]
    });

    service = TestBed.inject(ConfigurationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should load configuration successfully', async () => {
    const mockConfig = {
      apiUrl: 'https://api.example.com',
      environment: 'production',
      features: {
        aiEnabled: true,
        searchEnabled: true
      }
    };

    const loadPromise = service.loadConfig();
    
    const req = httpMock.expectOne('/api/configuration');
    expect(req.request.method).toBe('GET');
    req.flush(mockConfig);

    await loadPromise;

    const config = service.getConfig();
    expect(config).toEqual(mockConfig);
    expect(config.environment).toBe('production');
  });

  it('should handle configuration load error gracefully', async () => {
    const loadPromise = service.loadConfig();

    const req = httpMock.expectOne('/api/configuration');
    req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });

    await loadPromise;

    const config = service.getConfig();
    expect(config).toBeUndefined();
  });

  it('should return undefined config before loading', () => {
    const config = service.getConfig();
    expect(config).toBeUndefined();
  });

  it('should store configuration after successful load', async () => {
    const mockConfig = {
      apiUrl: 'https://api.example.com',
      timeout: 5000
    };

    const loadPromise = service.loadConfig();
    
    const req = httpMock.expectOne('/api/configuration');
    req.flush(mockConfig);

    await loadPromise;

    expect(service.getConfig()).toBeDefined();
    expect(service.getConfig().apiUrl).toBe('https://api.example.com');
    expect(service.getConfig().timeout).toBe(5000);
  });

  it('should handle empty configuration response', async () => {
    const loadPromise = service.loadConfig();
    
    const req = httpMock.expectOne('/api/configuration');
    req.flush({});

    await loadPromise;

    const config = service.getConfig();
    expect(config).toEqual({});
  });
});

