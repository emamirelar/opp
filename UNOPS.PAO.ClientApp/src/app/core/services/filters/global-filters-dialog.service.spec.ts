import { TestBed } from '@angular/core/testing';
import { GlobalFiltersDialogService } from './global-filters-dialog.service';

describe('GlobalFiltersDialogService', () => {
  let service: GlobalFiltersDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GlobalFiltersDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for dialog open/close
  // TODO: Add tests for filter management
});

