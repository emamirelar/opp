import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RoleService, Role, UserRoles } from './role.service';

describe('RoleService', () => {
  let service: RoleService;
  let httpMock: HttpTestingController;
  const baseUrl = 'api/role';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [RoleService]
    });

    service = TestBed.inject(RoleService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get all roles', (done) => {
    const mockRoles: Role[] = [
      { id: 1, name: 'Administrator' },
      { id: 2, name: 'User' },
      { id: 3, name: 'Internal' }
    ];

    service.getAllRoles().subscribe(roles => {
      expect(roles).toEqual(mockRoles);
      expect(roles.length).toBe(3);
      done();
    });

    const req = httpMock.expectOne(`${baseUrl}/all`);
    expect(req.request.method).toBe('GET');
    req.flush(mockRoles);
  });

  it('should get user roles', (done) => {
    const mockUserRoles: UserRoles = {
      email: 'test@unops.org',
      roles: ['Administrator', 'Internal']
    };

    service.getUserRoles().subscribe(userRoles => {
      expect(userRoles).toEqual(mockUserRoles);
      expect(userRoles.email).toBe('test@unops.org');
      expect(userRoles.roles.length).toBe(2);
      done();
    });

    const req = httpMock.expectOne(`${baseUrl}/user`);
    expect(req.request.method).toBe('GET');
    req.flush(mockUserRoles);
  });

  it('should update user roles', (done) => {
    const newRoles = ['User', 'Partner'];
    const mockResponse = { success: true };

    service.updateUserRoles(newRoles).subscribe(response => {
      expect(response).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne(`${baseUrl}/update`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(newRoles);
    req.flush(mockResponse);
  });

  it('should handle error when getting all roles', (done) => {
    service.getAllRoles().subscribe({
      next: () => fail('should have failed'),
      error: (error) => {
        expect(error.status).toBe(500);
        done();
      }
    });

    const req = httpMock.expectOne(`${baseUrl}/all`);
    req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
  });

  it('should handle error when getting user roles', (done) => {
    service.getUserRoles().subscribe({
      next: () => fail('should have failed'),
      error: (error) => {
        expect(error.status).toBe(404);
        done();
      }
    });

    const req = httpMock.expectOne(`${baseUrl}/user`);
    req.flush('Not found', { status: 404, statusText: 'Not Found' });
  });

  it('should handle error when updating user roles', (done) => {
    const newRoles = ['InvalidRole'];

    service.updateUserRoles(newRoles).subscribe({
      next: () => fail('should have failed'),
      error: (error) => {
        expect(error.status).toBe(400);
        done();
      }
    });

    const req = httpMock.expectOne(`${baseUrl}/update`);
    req.flush('Invalid roles', { status: 400, statusText: 'Bad Request' });
  });
});

