import { TestBed } from '@angular/core/testing';
import { PermissionService } from './permission.service';
import { HttpClientTestingModule } from '@angular/common/http/testing';

describe('PermissionService', () => {
  let service: PermissionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PermissionService]
    });
    service = TestBed.inject(PermissionService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // TODO: Add tests for canAccessRoute()
  // TODO: Add tests for permission checking
  // TODO: Add tests for entity permissions
});

