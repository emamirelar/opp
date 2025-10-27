import { TestBed } from '@angular/core/testing';
import { ContactExportService } from './contact-export.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('ContactExportService', () => {
  let service: ContactExportService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ContactExportService]
    });
    service = TestBed.inject(ContactExportService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for contact export
  // TODO: Add tests for export formats
});

