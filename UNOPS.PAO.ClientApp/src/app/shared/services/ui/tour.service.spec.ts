import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router, NavigationEnd } from '@angular/router';
import { TourService } from './tour.service';
import { Subject } from 'rxjs';

describe('TourService', () => {
  let service: TourService;
  let mockRouter: jasmine.SpyObj<Router>;
  let routerEventsSubject: Subject<any>;

  beforeEach(() => {
    routerEventsSubject = new Subject();
    
    mockRouter = jasmine.createSpyObj('Router', ['navigate'], {
      events: routerEventsSubject.asObservable(),
      url: '/'
    });

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        TourService,
        { provide: Router, useValue: mockRouter }
      ]
    });

    service = TestBed.inject(TourService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for starting tours
  // TODO: Add tests for tour step navigation
  // TODO: Add tests for tour completion tracking
  // TODO: Add tests for tour state persistence
});

