import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { WelcomeTourService } from './welcome-tour.service';
import { of } from 'rxjs';

describe('WelcomeTourService', () => {
  let service: WelcomeTourService;
  let mockRouter: jasmine.SpyObj<Router>;
  let mockTranslateService: jasmine.SpyObj<TranslateService>;

  beforeEach(() => {
    mockRouter = jasmine.createSpyObj('Router', ['navigate'], {
      events: of(),
      url: '/'
    });

    mockTranslateService = jasmine.createSpyObj('TranslateService', ['instant', 'get']);
    mockTranslateService.instant.and.returnValue('Translated Text');
    mockTranslateService.get.and.returnValue(of('Translated Text'));

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        WelcomeTourService,
        { provide: Router, useValue: mockRouter },
        { provide: TranslateService, useValue: mockTranslateService }
      ]
    });

    service = TestBed.inject(WelcomeTourService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for welcome tour initialization
  // TODO: Add tests for first-time user detection
  // TODO: Add tests for tour completion tracking
  // TODO: Add tests for skipping welcome tour
});

