import { TestBed } from '@angular/core/testing';
import { UserSearchService } from './user-search.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('UserSearchService', () => {
  let service: UserSearchService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserSearchService]
    });
    service = TestBed.inject(UserSearchService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for user search
  // TODO: Add tests for search filtering
});

