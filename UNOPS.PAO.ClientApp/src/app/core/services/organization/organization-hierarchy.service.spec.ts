import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { OrganizationHierarchyService } from './organization-hierarchy.service';
import { TreeNode } from 'primeng/api';

describe('OrganizationHierarchyService', () => {
  let service: OrganizationHierarchyService;
  let httpMock: HttpTestingController;
  const apiUrl = '/api/organization-hierarchy';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [OrganizationHierarchyService]
    });

    service = TestBed.inject(OrganizationHierarchyService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get organization hierarchy in PrimeNG format', (done) => {
    const mockData: TreeNode[] = [
      {
        expanded: true,
        type: 'person',
        data: {
          id: 1,
          name: 'UNOPS',
          code: 'OPS',
          type: 0,
          description: 'Main organization'
        },
        children: []
      }
    ];

    service.getOrganizationHierarchy().subscribe(hierarchy => {
      expect(hierarchy).toEqual(mockData);
      expect(hierarchy.length).toBe(1);
      expect(hierarchy[0].data.name).toBe('UNOPS');
      done();
    });

    const req = httpMock.expectOne(apiUrl);
    expect(req.request.method).toBe('GET');
    req.flush(mockData);
  });

  it('should handle API error and fallback to legacy endpoint', (done) => {
    const legacyData = [
      {
        data: {
          id: 1,
          name: 'Legacy Org',
          code: 'LEG',
          type: 0,
          description: 'From legacy endpoint'
        }
      }
    ];

    service.getOrganizationHierarchy().subscribe(hierarchy => {
      expect(hierarchy.length).toBeGreaterThan(0);
      expect(hierarchy[0].data.name).toBe('Legacy Org');
      done();
    });

    const req1 = httpMock.expectOne(apiUrl);
    req1.error(new ProgressEvent('error'), { status: 500, statusText: 'Server Error' });

    const req2 = httpMock.expectOne(`${apiUrl}/legacy`);
    req2.flush(legacyData);
  });

  it('should use test data as fallback when both API calls fail', (done) => {
    service.getOrganizationHierarchy().subscribe(hierarchy => {
      expect(hierarchy.length).toBeGreaterThan(0);
      expect(hierarchy[0].data).toBeDefined();
      done();
    });

    const req1 = httpMock.expectOne(apiUrl);
    req1.error(new ProgressEvent('error'), { status: 500, statusText: 'Server Error' });

    const req2 = httpMock.expectOne(`${apiUrl}/legacy`);
    req2.error(new ProgressEvent('error'), { status: 500, statusText: 'Server Error' });
  });

  it('should transform legacy API response to PrimeNG format', (done) => {
    const legacyResponse = [
      {
        data: {
          id: 2,
          name: 'Business Group',
          code: 'BG1',
          type: 1,
          description: 'Business group description',
          children: [
            {
              id: 3,
              name: 'Country Office',
              code: 'CO1',
              type: 2,
              description: 'Country office',
              children: []
            }
          ]
        }
      }
    ];

    service.getOrganizationHierarchy().subscribe(hierarchy => {
      expect(hierarchy[0].data.name).toBe('Business Group');
      expect(hierarchy[0].children).toBeDefined();
      expect(hierarchy[0].children!.length).toBe(1);
      expect(hierarchy[0].children![0].data.name).toBe('Country Office');
      done();
    });

    const req1 = httpMock.expectOne(apiUrl);
    req1.error(new ProgressEvent('error'));

    const req2 = httpMock.expectOne(`${apiUrl}/legacy`);
    req2.flush(legacyResponse);
  });

  it('should handle already-formatted PrimeNG response', (done) => {
    const primeNgData: TreeNode[] = [
      {
        expanded: true,
        type: 'person',
        data: {
          id: 1,
          name: 'Already Formatted',
          code: 'AF',
          type: 0,
          description: 'Pre-formatted data'
        },
        children: []
      }
    ];

    service.getOrganizationHierarchy().subscribe(hierarchy => {
      expect(hierarchy).toEqual(primeNgData);
      expect(hierarchy[0].expanded).toBe(true);
      done();
    });

    const req = httpMock.expectOne(apiUrl);
    req.flush(primeNgData);
  });

  it('should handle empty API response', (done) => {
    service.getOrganizationHierarchy().subscribe(hierarchy => {
      expect(hierarchy).toBeDefined();
      expect(Array.isArray(hierarchy)).toBe(true);
      done();
    });

    const req1 = httpMock.expectOne(apiUrl);
    req1.flush([]);

    // No second request should be made for empty array
  });

  it('should set default values for missing properties', (done) => {
    const incompleteData = [
      {
        data: {
          id: 1,
          // Missing name, code, description
          type: 0
        }
      }
    ];

    service.getOrganizationHierarchy().subscribe(hierarchy => {
      expect(hierarchy[0].data.name).toBe('Unnamed');
      expect(hierarchy[0].data.code).toBe('No Code');
      expect(hierarchy[0].data.description).toBe('No description provided');
      done();
    });

    const req1 = httpMock.expectOne(apiUrl);
    req1.error(new ProgressEvent('error'));

    const req2 = httpMock.expectOne(`${apiUrl}/legacy`);
    req2.flush(incompleteData);
  });

  it('should process nested children recursively', (done) => {
    const nestedData = [
      {
        data: {
          id: 1,
          name: 'Level 1',
          code: 'L1',
          type: 0,
          description: 'First level',
          children: [
            {
              id: 2,
              name: 'Level 2',
              code: 'L2',
              type: 1,
              description: 'Second level',
              children: [
                {
                  id: 3,
                  name: 'Level 3',
                  code: 'L3',
                  type: 2,
                  description: 'Third level',
                  children: []
                }
              ]
            }
          ]
        }
      }
    ];

    service.getOrganizationHierarchy().subscribe(hierarchy => {
      expect(hierarchy[0].children!.length).toBe(1);
      expect(hierarchy[0].children![0].children!.length).toBe(1);
      expect(hierarchy[0].children![0].children![0].data.name).toBe('Level 3');
      done();
    });

    const req1 = httpMock.expectOne(apiUrl);
    req1.error(new ProgressEvent('error'));

    const req2 = httpMock.expectOne(`${apiUrl}/legacy`);
    req2.flush(nestedData);
  });
});

