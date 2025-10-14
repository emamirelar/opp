import { TestBed } from '@angular/core/testing';
import { ImportDialogService } from './import-dialog.service';

describe('ImportDialogService', () => {
  let service: ImportDialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ImportDialogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for dialog opening
  // TODO: Add tests for dialog data management
});

