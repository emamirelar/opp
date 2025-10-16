import { TestBed } from '@angular/core/testing';
import { LinkService } from './link.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('LinkService', () => {
  let service: LinkService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LinkService]
    });
    service = TestBed.inject(LinkService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for link creation
  // TODO: Add tests for link retrieval
  // TODO: Add tests for link deletion
});

