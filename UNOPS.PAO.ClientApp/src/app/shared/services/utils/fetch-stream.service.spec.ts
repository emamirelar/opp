import { TestBed } from '@angular/core/testing';
import { FetchStreamService } from './fetch-stream.service';

describe('FetchStreamService', () => {
  let service: FetchStreamService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(FetchStreamService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for streaming data fetch
  // TODO: Add tests for stream parsing
});

