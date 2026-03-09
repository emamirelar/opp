import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { DialogService } from 'primeng/dynamicdialog';
import { ConfirmationService } from 'primeng/api';
import { ImportDialogService } from '@features/import-export/components/import/dialog/import-dialog.service';
import { ContactService, ContactsParams } from './contact.service';
import { Contact } from '../models/contact.model';

describe('ContactService', () => {
  let service: ContactService;
  let httpTestingController: HttpTestingController;
  
  const mockContact: Contact = {
    id: '1',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    phoneNumber: '+1234567890',
    salutation: 'Mr.',
    jobTitle: 'Manager',
    status: 'Active',
    partnerId: 'partner-1'
  } as Contact;

  const mockContactResponse = {
    records: [mockContact],
    totalCount: 1,
    pageIndex: 1,
    pageSize: 10,
    totalPages: 1
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        ContactService,
        { provide: DialogService, useValue: {} },
        { provide: ConfirmationService, useValue: { confirm: () => {} } },
        { provide: ImportDialogService, useValue: {} }
      ]
    });
    
    service = TestBed.inject(ContactService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('signal state management', () => {
    it('should initialize with empty contacts and loading false', () => {
      expect(service.allContacts()).toEqual([]);
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('getAll', () => {
    it('should make GET request with params and return response', () => {
      const params = { page: 1, pageSize: 10, searchText: 'test' };
      
      service.getAll(params).subscribe(response => {
        expect(response.body).toEqual(mockContactResponse);
        expect(service.isLoading()).toBe(false);
      });

      const req = httpTestingController.expectOne((request) => {
        return request.url === '/api/contact' && request.method === 'GET';
      });
      
      expect(req.request.params.get('page')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('10');
      expect(req.request.params.get('searchText')).toBe('test');
      expect(service.isLoading()).toBe(true);
      
      req.flush(mockContactResponse);
    });

    it('should set loading to false on error', () => {
      service.getAll({}).subscribe({
        error: () => {
          // Error callback executed
        }
      });

      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      req.error(new ProgressEvent('error'));
      
      // Check loading state after error
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('getUrl', () => {
    it('should return correct API URL', () => {
      expect(service.getUrl()).toBe('/api/contact');
    });
  });

  describe('getClassicSearchUrl', () => {
    it('should return correct search URL', () => {
      expect(service.getClassicSearchUrl()).toBe('/api/contact');
    });
  });

  describe('getUploadProfilePictureUrl', () => {
    it('should return correct upload URL for contact', () => {
      const contactId = '123';
      const expected = '/api/contact/123/profile-picture';
      
      expect(service.getUploadProfilePictureUrl(contactId)).toBe(expected);
    });
  });

  describe('getAllContacts', () => {
    it('should load all contacts and update signals', () => {
      service.getAllContacts();
      
      expect(service.isLoading()).toBe(true);
      
      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      req.flush(mockContactResponse);
      
      expect(service.allContacts()).toEqual([mockContact]);
      expect(service.isLoading()).toBe(false);
    });

    it('should handle error and update loading state', () => {
      spyOn(console, 'error');
      
      service.getAllContacts();
      
      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      req.error(new ProgressEvent('error'));
      
      expect(console.error).toHaveBeenCalledWith('Error loading contacts:', jasmine.any(Object));
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('getContacts', () => {
    it('should transform response correctly with all params', () => {
      const params: ContactsParams = {
        page: 2,
        pageSize: 25,
        searchText: 'john',
        sortField: 'lastName',
        sortOrder: 'desc'
      };
      
      service.getContacts(params).subscribe(result => {
        expect(result).toEqual({
          data: [mockContact],
          total: 1
        });
        expect(service.isLoading()).toBe(false);
      });

      const req = httpTestingController.expectOne((request) => {
        return request.url === '/api/contact' && request.method === 'GET';
      });
      
      expect(req.request.params.get('page')).toBe('2');
      expect(req.request.params.get('pageSize')).toBe('25');
      expect(req.request.params.get('searchText')).toBe('john');
      expect(req.request.params.get('sortField')).toBe('lastName');
      expect(req.request.params.get('sortOrder')).toBe('desc');
      
      req.flush(mockContactResponse);
    });

    it('should handle minimal params', () => {
      const params: ContactsParams = {
        page: 1,
        pageSize: 10
      };
      
      service.getContacts(params).subscribe();

      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      
      expect(req.request.params.get('page')).toBe('1');
      expect(req.request.params.get('pageSize')).toBe('10');
      expect(req.request.params.get('searchText')).toBeNull();
      expect(req.request.params.get('sortField')).toBeNull();
      expect(req.request.params.get('sortOrder')).toBeNull();
      
      req.flush(mockContactResponse);
    });

    it('should default sortOrder to asc when sortField provided', () => {
      const params: ContactsParams = {
        page: 1,
        pageSize: 10,
        sortField: 'name'
      };
      
      service.getContacts(params).subscribe();

      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      expect(req.request.params.get('sortOrder')).toBe('asc');
      
      req.flush(mockContactResponse);
    });

    it('should handle empty response data', () => {
      const emptyResponse = {};
      
      service.getContacts({ page: 1, pageSize: 10 }).subscribe(result => {
        expect(result).toEqual({
          data: [],
          total: 0
        });
      });

      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      req.flush(emptyResponse);
    });

    it('should set loading to false on error', () => {
      service.getContacts({ page: 1, pageSize: 10 }).subscribe({
        error: () => {
          expect(service.isLoading()).toBe(false);
        }
      });

      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      req.error(new ProgressEvent('error'));
    });
  });

  describe('getContactById', () => {
    it('should fetch contact by ID', () => {
      const contactId = '123';
      
      service.getContactById(contactId).subscribe(contact => {
        expect(contact).toEqual(mockContact);
        expect(service.isLoading()).toBe(false);
      });

      expect(service.isLoading()).toBe(true);
      
      const req = httpTestingController.expectOne(`/api/contact/${contactId}`);
      req.flush(mockContact);
    });

    it('should handle error and update loading state', () => {
      service.getContactById('123').subscribe({
        error: () => {
          // Error callback executed
        }
      });

      const req = httpTestingController.expectOne('/api/contact/123');
      req.error(new ProgressEvent('error'));
      
      // Check loading state after error
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('createContact', () => {
    it('should create contact and manage loading state', () => {
      service.createContact(mockContact).subscribe(contact => {
        expect(contact).toEqual(mockContact);
        expect(service.isLoading()).toBe(false);
      });

      expect(service.isLoading()).toBe(true);
      
      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockContact);
      
      req.flush(mockContact);
    });

    it('should handle creation error', () => {
      service.createContact(mockContact).subscribe({
        error: () => {
          expect(service.isLoading()).toBe(false);
        }
      });

      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      req.error(new ProgressEvent('error'));
    });
  });

  describe('updateContactById', () => {
    it('should update contact and manage loading state', () => {
      const updatedContact = { ...mockContact, firstName: 'Jane' };
      
      service.updateContactById(updatedContact).subscribe(contact => {
        expect(service.isLoading()).toBe(false);
      });

      expect(service.isLoading()).toBe(true);
      
      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updatedContact);
      
      req.flush(updatedContact);
    });

    it('should handle update error', () => {
      service.updateContactById(mockContact).subscribe({
        error: () => {
          expect(service.isLoading()).toBe(false);
        }
      });

      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      req.error(new ProgressEvent('error'));
    });
  });

  describe('deleteContactById', () => {
    it('should delete contact and manage loading state', () => {
      const contactId = '123';
      
      service.deleteContactById(contactId).subscribe(() => {
        expect(service.isLoading()).toBe(false);
      });

      expect(service.isLoading()).toBe(true);
      
      const req = httpTestingController.expectOne(`/api/contact/${contactId}`);
      expect(req.request.method).toBe('DELETE');
      
      req.flush({});
    });

    it('should handle delete error', () => {
      service.deleteContactById('123').subscribe({
        error: () => {
          expect(service.isLoading()).toBe(false);
        }
      });

      const req = httpTestingController.expectOne('/api/contact/123');
      req.error(new ProgressEvent('error'));
    });
  });

  describe('integration scenarios', () => {
    it('should handle rapid successive API calls', () => {
      // Multiple rapid calls
      service.getAllContacts();
      service.getContactById('1').subscribe();
      service.createContact(mockContact).subscribe();

      const requests = httpTestingController.match(() => true);
      expect(requests.length).toBe(3);
      
      // Complete all requests
      requests.forEach(req => req.flush(req.request.method === 'GET' ? mockContactResponse : mockContact));
      
      expect(service.isLoading()).toBe(false);
    });

    it('should properly construct complex query parameters', () => {
      const complexParams: ContactsParams = {
        page: 5,
        pageSize: 50,
        searchText: 'complex search with spaces',
        sortField: 'fullName',
        sortOrder: 'desc'
      };
      
      service.getContacts(complexParams).subscribe();
      
      const req = httpTestingController.expectOne(req => req.url === '/api/contact');
      
      expect(req.request.params.get('page')).toBe('5');
      expect(req.request.params.get('pageSize')).toBe('50');
      expect(req.request.params.get('searchText')).toBe('complex search with spaces');
      expect(req.request.params.get('sortField')).toBe('fullName');
      expect(req.request.params.get('sortOrder')).toBe('desc');
      
      req.flush(mockContactResponse);
    });
  });
});
