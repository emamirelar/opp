import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { TranslateService } from '@ngx-translate/core';
import { LanguageService } from './language.service';
import { AuthService } from '@core/services/auth';
import { of } from 'rxjs';

describe('LanguageService', () => {
  let service: LanguageService;
  let mockTranslateService: jasmine.SpyObj<TranslateService>;
  let mockAuthService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    localStorage.clear();

    mockTranslateService = jasmine.createSpyObj('TranslateService', ['use', 'setDefaultLang', 'addLangs', 'getBrowserLang']);
    mockTranslateService.use.and.returnValue(of({}));
    mockTranslateService.getBrowserLang.and.returnValue('en');

    mockAuthService = jasmine.createSpyObj('AuthService', ['user', 'isLogedIn']);
    mockAuthService.user.and.returnValue(of([{ type: 'email', value: 'test@unops.org' }]));
    mockAuthService.isLogedIn.and.returnValue(of(true));

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        LanguageService,
        { provide: TranslateService, useValue: mockTranslateService },
        { provide: AuthService, useValue: mockAuthService }
      ]
    });

    service = TestBed.inject(LanguageService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for language detection
  // TODO: Add tests for setting language
  // TODO: Add tests for language persistence
  // TODO: Add tests for supported languages
  // TODO: Add tests for fallback language handling
});

