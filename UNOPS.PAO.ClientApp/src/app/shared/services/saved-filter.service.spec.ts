import { TestBed } from '@angular/core/testing';
import { SavedFilterService } from './saved-filter.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('SavedFilterService', () => {
  let service: SavedFilterService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [SavedFilterService]
    });
    service = TestBed.inject(SavedFilterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for filter saving
  // TODO: Add tests for filter loading
  // TODO: Add tests for filter deletion
});

