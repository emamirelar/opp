import { TestBed } from '@angular/core/testing';
import { InteractionIconService } from './interaction-icon.service';

describe('InteractionIconService', () => {
  let service: InteractionIconService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(InteractionIconService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for icon retrieval
  // TODO: Add tests for icon mapping
});

