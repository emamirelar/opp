import { TestBed } from '@angular/core/testing';
import { DocumentService } from './document.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('DocumentService', () => {
  let service: DocumentService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [DocumentService]
    });
    service = TestBed.inject(DocumentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for document upload
  // TODO: Add tests for document retrieval
  // TODO: Add tests for document deletion
});

