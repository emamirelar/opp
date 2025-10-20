import { TestBed } from '@angular/core/testing';
import { UserProfileService } from './user-profile.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('UserProfileService', () => {
  let service: UserProfileService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserProfileService]
    });
    service = TestBed.inject(UserProfileService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for user profile loading
  // TODO: Add tests for profile updates
});

