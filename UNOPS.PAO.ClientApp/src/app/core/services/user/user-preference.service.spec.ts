import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UserPreferenceService, DefaultOrgUnitResponse, GlobalFilters, UserPreference } from './user-preference.service';

describe('UserPreferenceService', () => {
  let service: UserPreferenceService;
  let httpMock: HttpTestingController;
  const apiUrl = '/api/user-preferences';
  const globalApiUrl = '/api/global';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserPreferenceService]
    });

    service = TestBed.inject(UserPreferenceService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get default org unit', (done) => {
    const mockResponse: DefaultOrgUnitResponse = { defaultOrgUnitId: 123 };

    service.getDefaultOrgUnit().subscribe(response => {
      expect(response.defaultOrgUnitId).toBe(123);
      done();
    });

    const req = httpMock.expectOne(`${apiUrl}/default-org-unit`);
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should update BehaviorSubject when getting default org unit', (done) => {
    const mockResponse: DefaultOrgUnitResponse = { defaultOrgUnitId: 456 };

    service.defaultOrgUnit$.subscribe(orgUnitId => {
      if (orgUnitId === 456) {
        expect(orgUnitId).toBe(456);
        done();
      }
    });

    service.getDefaultOrgUnit().subscribe();

    const req = httpMock.expectOne(`${apiUrl}/default-org-unit`);
    req.flush(mockResponse);
  });

  it('should handle error when getting default org unit', (done) => {
    service.getDefaultOrgUnit().subscribe(response => {
      expect(response.defaultOrgUnitId).toBeNull();
      done();
    });

    const req = httpMock.expectOne(`${apiUrl}/default-org-unit`);
    req.error(new ProgressEvent('error'), { status: 500, statusText: 'Server Error' });
  });

  it('should set default org unit', (done) => {
    const orgUnitId = 789;
    const mockResponse = { success: true };

    service.setDefaultOrgUnit(orgUnitId).subscribe(response => {
      expect(response).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne(`${apiUrl}/default-org-unit`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ orgUnitId: 789 });
    req.flush(mockResponse);
  });

  it('should update BehaviorSubject when setting default org unit', (done) => {
    service.defaultOrgUnit$.subscribe(orgUnitId => {
      if (orgUnitId === 999) {
        expect(service.getCurrentDefaultOrgUnitId()).toBe(999);
        done();
      }
    });

    service.setDefaultOrgUnit(999).subscribe();

    const req = httpMock.expectOne(`${apiUrl}/default-org-unit`);
    req.flush({ success: true });
  });

  it('should get current default org unit ID', () => {
    expect(service.getCurrentDefaultOrgUnitId()).toBeNull();
  });

  it('should get global filters', (done) => {
    const userId = 'user123';
    const mockFilters: GlobalFilters = {
      orgUnitId: 100,
      relatedToMe: true,
      preferredLanguage: 'en'
    };

    service.getGlobalFilters(userId).subscribe(filters => {
      expect(filters).toEqual(mockFilters);
      expect(filters.orgUnitId).toBe(100);
      done();
    });

    const req = httpMock.expectOne(`${globalApiUrl}/filters?id=${userId}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockFilters);
  });

  it('should return default filters on error', (done) => {
    const userId = 'user123';

    service.getGlobalFilters(userId).subscribe(filters => {
      expect(filters.orgUnitId).toBeNull();
      expect(filters.relatedToMe).toBe(false);
      expect(filters.preferredLanguage).toBe('en');
      done();
    });

    const req = httpMock.expectOne(`${globalApiUrl}/filters?id=${userId}`);
    req.error(new ProgressEvent('error'));
  });

  it('should update global filters', (done) => {
    const userId = 'user123';
    const filters: GlobalFilters = {
      orgUnitId: 200,
      relatedToMe: false,
      theme: 'dark'
    };

    service.updateGlobalFilters(userId, filters).subscribe(response => {
      expect(response).toBeDefined();
      done();
    });

    const req = httpMock.expectOne(`${globalApiUrl}/filters?id=${userId}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(filters);
    req.flush({ success: true });
  });

  it('should reset global filters', (done) => {
    const userId = 'user123';

    service.resetGlobalFilters(userId).subscribe(response => {
      expect(response).toBeDefined();
      done();
    });

    const req = httpMock.expectOne(`${globalApiUrl}/filters/reset?id=${userId}`);
    expect(req.request.method).toBe('POST');
    req.flush({ success: true });
  });

  it('should get user preferences', (done) => {
    const userId = 'user123';
    const mockPreferences: UserPreference = {
      userId: 123,
      globalFilterJson: '{"orgUnitId":100}',
      additionalSettingsJson: '{}'
    };

    service.getUserPreferences(userId).subscribe(prefs => {
      expect(prefs).toEqual(mockPreferences);
      expect(prefs.userId).toBe(123);
      done();
    });

    const req = httpMock.expectOne(`${globalApiUrl}/user-preferences?id=${userId}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockPreferences);
  });

  it('should update user preferences', (done) => {
    const userId = 'user123';
    const preferences: UserPreference = {
      userId: 123,
      globalFilterJson: '{"orgUnitId":200}'
    };

    service.updateUserPreferences(userId, preferences).subscribe(response => {
      expect(response).toBeDefined();
      done();
    });

    const req = httpMock.expectOne(`${globalApiUrl}/user-preferences?id=${userId}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(preferences);
    req.flush({ success: true });
  });

  it('should handle errors when updating global filters', (done) => {
    const userId = 'user123';
    const filters: GlobalFilters = { orgUnitId: 300 };

    service.updateGlobalFilters(userId, filters).subscribe({
      next: () => fail('should have failed'),
      error: (error) => {
        expect(error).toBeDefined();
        done();
      }
    });

    const req = httpMock.expectOne(`${globalApiUrl}/filters?id=${userId}`);
    req.error(new ProgressEvent('error'), { status: 400, statusText: 'Bad Request' });
  });
});

