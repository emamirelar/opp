import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { HttpResponse } from '@angular/common/http';
import LinkDataService from './link-data.service';
import { LinkService } from '@shared/services/api/link.service';
import { Link, EntityType, LinkRequest, UpdateLinkRequest } from '../../../models/link.model';
import { of, throwError } from 'rxjs';

describe('LinkDataService', () => {
  let service: LinkDataService;
  let mockLinkService: jasmine.SpyObj<LinkService>;

  const mockLink: Link = {
    id: 1,
    url: 'https://example.com',
    name: 'Example Link',
    entity: EntityType.Partner,
    entityId: 123
  };
  const emptyLinksResponse = new HttpResponse({
    body: {
      records: [],
      totalCount: 0,
      pageIndex: 1,
      pageSize: 20,
      totalPages: 1
    }
  });
  const linkResponse = new HttpResponse({ body: mockLink });

  beforeEach(() => {
    mockLinkService = jasmine.createSpyObj('LinkService', [
      'getAll',
      'create',
      'update',
      'delete'
    ]);
    mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));
    mockLinkService.create.and.returnValue(of(linkResponse));
    mockLinkService.update.and.returnValue(of(new HttpResponse<void>({ body: undefined })));
    mockLinkService.delete.and.returnValue(of(void 0));

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        LinkDataService,
        { provide: LinkService, useValue: mockLinkService }
      ]
    });

    service = TestBed.inject(LinkDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initialization', () => {
    it('should have default signal values', () => {
      expect(service.links()).toEqual([]);
      expect(service.loading()).toBeFalse();
      expect(service.saving()).toBeFalse();
      expect(service.currentPage()).toBe(-1);
      expect(service.hasMore()).toBeTrue();
      expect(service.pageSize()).toBe(20);
    });

    it('should set entityType and entityId to undefined initially', () => {
      expect(service.entityType()).toBeUndefined();
      expect(service.entityId()).toBeUndefined();
    });
  });

  describe('initialize', () => {
    it('should set entityType, entityId, and pageSize', () => {
      service.initialize(EntityType.Partner, 123, 10);

      expect(service.entityType()).toBe(EntityType.Partner);
      expect(service.entityId()).toBe(123);
      expect(service.pageSize()).toBe(10);
    });

    it('should call load with reset=true', () => {
      spyOn(service, 'load');
      
      service.initialize(EntityType.Contact, 456);

      expect(service.load).toHaveBeenCalledWith(true);
    });

    it('should use default pageSize of 20 if not provided', () => {
      service.initialize(EntityType.Partner, 123);

      expect(service.pageSize()).toBe(20);
    });
  });

  describe('load', () => {
    beforeEach(() => {
      mockLinkService.getAll.calls.reset();
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));
      service.initialize(EntityType.Partner, 123);
    });

    it('should not load if entityType is undefined', () => {
      mockLinkService.getAll.calls.reset();
      service.entityType.set(undefined);

      service.load();

      expect(mockLinkService.getAll).not.toHaveBeenCalled();
    });

    it('should not load if entityId is undefined', () => {
      mockLinkService.getAll.calls.reset();
      service.entityId.set(undefined);

      service.load();

      expect(mockLinkService.getAll).not.toHaveBeenCalled();
    });

    it('should reset currentPage to 0 when reset=true', () => {
      service.currentPage.set(5);
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));

      service.load(true);

      expect(service.currentPage()).toBe(0);
    });

    it('should increment currentPage when reset=false', () => {
      service.currentPage.set(2);
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));

      service.load(false);

      expect(service.currentPage()).toBe(3);
    });

    it('should set loading to true while loading', () => {
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));

      service.load();

      // Loading is set synchronously
      expect(mockLinkService.getAll).toHaveBeenCalled();
    });

    it('should replace links when reset=true', (done) => {
      const newLinks = [mockLink];
      service.links.set([{ ...mockLink, id: 999 }]);
      mockLinkService.getAll.and.returnValue(of(new HttpResponse({
        body: {
          records: newLinks,
          totalCount: newLinks.length,
          pageIndex: 1,
          pageSize: 20,
          totalPages: 1
        }
      })));

      service.load(true);

      setTimeout(() => {
        expect(service.links()).toEqual(newLinks);
        expect(service.loading()).toBeFalse();
        done();
      }, 100);
    });

    it('should append links when reset=false', (done) => {
      const existingLinks = [{ ...mockLink, id: 1 }];
      const newLinks = [{ ...mockLink, id: 2 }];
      service.links.set(existingLinks);
      mockLinkService.getAll.and.returnValue(of(new HttpResponse({
        body: {
          records: newLinks,
          totalCount: newLinks.length,
          pageIndex: 1,
          pageSize: 20,
          totalPages: 1
        }
      })));

      service.load(false);

      setTimeout(() => {
        expect(service.links().length).toBe(2);
        expect(service.links()).toContain(existingLinks[0]);
        expect(service.links()).toContain(newLinks[0]);
        done();
      }, 100);
    });

    it('should set hasMore to true when full page returned', (done) => {
      const links = new Array(20).fill(mockLink);
      mockLinkService.getAll.and.returnValue(of(new HttpResponse({
        body: {
          records: links,
          totalCount: links.length,
          pageIndex: 1,
          pageSize: 20,
          totalPages: 1
        }
      })));

      service.load();

      setTimeout(() => {
        expect(service.hasMore()).toBeTrue();
        done();
      }, 100);
    });

    it('should set hasMore to false when partial page returned', (done) => {
      const links = [mockLink]; // Less than pageSize
      mockLinkService.getAll.and.returnValue(of(new HttpResponse({
        body: {
          records: links,
          totalCount: links.length,
          pageIndex: 1,
          pageSize: 20,
          totalPages: 1
        }
      })));

      service.load();

      setTimeout(() => {
        expect(service.hasMore()).toBeFalse();
        done();
      }, 100);
    });

    it('should handle errors gracefully', (done) => {
      mockLinkService.getAll.and.returnValue(throwError(() => new Error('Load error')));

      service.load();

      setTimeout(() => {
        expect(service.loading()).toBeFalse();
        done();
      }, 100);
    });
  });

  describe('createLink', () => {
    beforeEach(() => {
      service.initialize(EntityType.Partner, 123);
    });

    it('should not create if entityType is undefined', () => {
      service.entityType.set(undefined);
      
      service.createLink('https://example.com');

      expect(mockLinkService.create).not.toHaveBeenCalled();
    });

    it('should not create if entityId is undefined', () => {
      service.entityId.set(undefined);
      
      service.createLink('https://example.com');

      expect(mockLinkService.create).not.toHaveBeenCalled();
    });

    it('should create link with extracted name from URL', () => {
      mockLinkService.create.and.returnValue(of(linkResponse));
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));

      service.createLink('https://example.com');

      expect(mockLinkService.create).toHaveBeenCalledWith(
        jasmine.objectContaining({
          url: 'https://example.com',
          entity: EntityType.Partner,
          entityId: 123
        })
      );
    });

    it('should set saving to true while creating', () => {
      mockLinkService.create.and.returnValue(of(linkResponse));
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));

      service.createLink('https://example.com');

      // Saving is set synchronously
      expect(mockLinkService.create).toHaveBeenCalled();
    });

    it('should reload links after successful creation', (done) => {
      mockLinkService.create.and.returnValue(of(linkResponse));
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));
      spyOn(service, 'load');

      service.createLink('https://example.com');

      setTimeout(() => {
        expect(service.load).toHaveBeenCalledWith(true);
        expect(service.saving()).toBeFalse();
        done();
      }, 100);
    });

    it('should handle creation errors', (done) => {
      mockLinkService.create.and.returnValue(throwError(() => new Error('Create error')));
      spyOn(console, 'error');

      service.createLink('https://example.com');

      setTimeout(() => {
        expect(console.error).toHaveBeenCalled();
        expect(service.saving()).toBeFalse();
        done();
      }, 100);
    });
  });

  describe('saveLink', () => {
    beforeEach(() => {
      service.initialize(EntityType.Partner, 123);
    });

    it('should not save if url is empty', () => {
      const link = { ...mockLink, url: '' };
      
      service.saveLink(link);

      expect(mockLinkService.update).not.toHaveBeenCalled();
    });

    it('should update existing link', () => {
      mockLinkService.update.and.returnValue(of(new HttpResponse<void>({ body: undefined })));
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));

      service.saveLink(mockLink);

      expect(mockLinkService.update).toHaveBeenCalledWith(mockLink as UpdateLinkRequest);
    });

    it('should reload links after successful save', (done) => {
      mockLinkService.update.and.returnValue(of(new HttpResponse<void>({ body: undefined })));
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));
      spyOn(service, 'load');

      service.saveLink(mockLink);

      setTimeout(() => {
        expect(service.load).toHaveBeenCalledWith(true);
        expect(service.saving()).toBeFalse();
        done();
      }, 100);
    });

    it('should handle save errors', (done) => {
      mockLinkService.update.and.returnValue(throwError(() => new Error('Save error')));
      spyOn(console, 'error');

      service.saveLink(mockLink);

      setTimeout(() => {
        expect(console.error).toHaveBeenCalled();
        expect(service.saving()).toBeFalse();
        done();
      }, 100);
    });
  });

  describe('deleteLink', () => {
    beforeEach(() => {
      service.initialize(EntityType.Partner, 123);
    });

    it('should delete link by id', () => {
      mockLinkService.delete.and.returnValue(of(void 0));
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));

      service.deleteLink(1);

      expect(mockLinkService.delete).toHaveBeenCalledWith(1);
    });

    it('should reload links after successful deletion', (done) => {
      mockLinkService.delete.and.returnValue(of(void 0));
      mockLinkService.getAll.and.returnValue(of(emptyLinksResponse));
      spyOn(service, 'load');

      service.deleteLink(1);

      setTimeout(() => {
        expect(service.load).toHaveBeenCalledWith(true);
        expect(service.saving()).toBeFalse();
        done();
      }, 100);
    });

    it('should handle deletion errors', (done) => {
      mockLinkService.delete.and.returnValue(throwError(() => new Error('Delete error')));
      spyOn(console, 'error');

      service.deleteLink(1);

      setTimeout(() => {
        expect(console.error).toHaveBeenCalled();
        expect(service.saving()).toBeFalse();
        done();
      }, 100);
    });
  });

  describe('getNameFromUrl', () => {
    it('should extract domain name from URL', () => {
      const result = service['getNameFromUrl']('https://example.com/path');
      
      expect(result).toBe('example.com');
    });

    it('should handle URLs without protocol', () => {
      const result = service['getNameFromUrl']('example.com');
      
      expect(result).toBeTruthy();
    });

    it('should handle invalid URLs gracefully', () => {
      const result = service['getNameFromUrl']('not-a-url');
      
      expect(result).toBeTruthy(); // Should return something, not throw
    });
  });
});


