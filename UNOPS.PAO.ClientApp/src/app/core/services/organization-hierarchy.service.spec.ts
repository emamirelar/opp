import { TestBed } from '@angular/core/testing';
import { OrganizationHierarchyService } from './organization-hierarchy.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('OrganizationHierarchyService', () => {
  let service: OrganizationHierarchyService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [OrganizationHierarchyService]
    });
    service = TestBed.inject(OrganizationHierarchyService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for organization hierarchy retrieval
  // TODO: Add tests for hierarchy caching
});

