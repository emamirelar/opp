import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LinkService } from './link.service';
import { Link, LinkRequest, UpdateLinkRequest, EntityType } from '../../models/link.model';
import { PaginationResponse } from '../../models/pagination-response.model';

describe('LinkService', () => {
  let service: LinkService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LinkService]
    });

    service = TestBed.inject(LinkService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get all links without filters', (done) => {
    const mockResponse: PaginationResponse<Link> = {
      records: [{ id: 1, url: 'https://example.com', name: 'Example', entity: EntityType.Contact, entityId: 1 } as Link],
      totalCount: 1,
      pageIndex: 1,
      pageSize: 10,
      totalPages: 1
    };

    service.getAll(undefined, undefined, 1, 10).subscribe(response => {
      expect(response.body).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne('/api/links?pageIndex=1&pageSize=10');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get links filtered by entity', (done) => {
    const entity: EntityType = EntityType.Contact;
    const entityId = 123;
    const mockResponse: PaginationResponse<Link> = {
      records: [{ id: 1, url: 'https://example.com', name: 'Example', entity: EntityType.Contact, entityId: 123 } as Link],
      totalCount: 1,
      pageIndex: 1,
      pageSize: 10,
      totalPages: 1
    };

    service.getAll(entity, entityId, 1, 10).subscribe(response => {
      expect(response.body).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne('/api/links?pageIndex=1&pageSize=10&entity=Contact&entityId=123');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get links with sorting', (done) => {
    const mockResponse: PaginationResponse<Link> = {
      records: [],
      totalCount: 0,
      pageIndex: 1,
      pageSize: 10,
      totalPages: 0
    };

    service.getAll(undefined, undefined, 1, 10, 'name', true).subscribe(response => {
      expect(response.body).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne('/api/links?pageIndex=1&pageSize=10&orderBy=name&ascending=true');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should create a link', (done) => {
    const newLink: LinkRequest = {
      url: 'https://example.com',
      name: 'New Link',
      entity: EntityType.Contact,
      entityId: 123
    };
    const createdLink: Link = { id: 1, ...newLink };

    service.create(newLink).subscribe(response => {
      expect(response.body).toEqual(createdLink);
      done();
    });

    const req = httpMock.expectOne('/api/links');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newLink);
    req.flush(createdLink);
  });

  it('should update a link', (done) => {
    const updateLink: UpdateLinkRequest = {
      id: 1,
      url: 'https://updated.com',
      name: 'Updated Link'
    };

    service.update(updateLink).subscribe(response => {
      expect(response.status).toBeGreaterThanOrEqual(200);
      expect(response.status).toBeLessThan(300);
      done();
    });

    const req = httpMock.expectOne('/api/links');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updateLink);
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('should delete a link', (done) => {
    const linkId = 123;

    service.delete(linkId).subscribe(() => {
      expect(true).toBe(true);
      done();
    });

    const req = httpMock.expectOne('/api/links?id=123');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should handle pagination parameters', (done) => {
    const mockResponse: PaginationResponse<Link> = {
      records: [],
      totalCount: 0,
      pageIndex: 2,
      pageSize: 20,
      totalPages: 0
    };

    service.getAll(undefined, undefined, 2, 20).subscribe(response => {
      expect(response.body).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne('/api/links?pageIndex=2&pageSize=20');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should handle descending sort order', (done) => {
    const mockResponse: PaginationResponse<Link> = {
      records: [],
      totalCount: 0,
      pageIndex: 1,
      pageSize: 10,
      totalPages: 0
    };

    service.getAll(undefined, undefined, 1, 10, 'createdAt', false).subscribe(response => {
      expect(response.body).toEqual(mockResponse);
      done();
    });

    const req = httpMock.expectOne('/api/links?pageIndex=1&pageSize=10&orderBy=createdAt&ascending=false');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });
});

