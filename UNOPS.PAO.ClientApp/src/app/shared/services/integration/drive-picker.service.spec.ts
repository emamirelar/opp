import { TestBed } from '@angular/core/testing';
import { DrivePickerService } from './drive-picker.service';

describe('DrivePickerService', () => {
  let service: DrivePickerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DrivePickerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for Google Drive picker initialization
  // TODO: Add tests for file selection
});

