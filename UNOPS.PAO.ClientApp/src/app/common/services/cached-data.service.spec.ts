import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CachedDataService } from './cached-data.service';
import { PartnerTreeService } from '../../features/internal/services/partner-tree.service';
import { PartnerCategoryGroup } from '../../features/internal/models/partner-category-group.model';
import { of, throwError } from 'rxjs';

describe('CachedDataService', () => {
  let service: CachedDataService;
  let httpTestingController: HttpTestingController;
  let mockPartnerTreeService: jasmine.SpyObj<PartnerTreeService>;

  const mockPartnerCategoryGroups: PartnerCategoryGroup[] = [
    {
      partnerCategoryId: 1,
      partnerCategoryCode: 'CAT1',
      partnerCategoryName: 'Category 1',
      children: [
        { partnerGroupId: 1, partnerGroupCode: 'GRP1', partnerGroupName: 'Group 1' },
        { partnerGroupId: 2, partnerGroupCode: 'GRP2', partnerGroupName: 'Group 2' }
      ]
    },
    {
      partnerCategoryId: 2,
      partnerCategoryCode: 'CAT2',
      partnerCategoryName: 'Category 2',
      children: [
        { partnerGroupId: 3, partnerGroupCode: 'GRP3', partnerGroupName: 'Group 3' }
      ]
    }
  ];

  const mockProjects = [
    { id: '1', name: 'Project 1' },
    { id: '2', name: 'Project 2' }
  ];

  const mockContacts = [
    { id: '1', name: 'Contact 1', email: 'contact1@test.com' },
    { id: '2', name: 'Contact 2', email: 'contact2@test.com' }
  ];

  beforeEach(() => {
    const partnerTreeServiceSpy = jasmine.createSpyObj('PartnerTreeService', ['getCategoryAndGroupStructure']);
    // Set up the spy before injecting the service
    partnerTreeServiceSpy.getCategoryAndGroupStructure.and.returnValue(of(mockPartnerCategoryGroups));

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        CachedDataService,
        { provide: PartnerTreeService, useValue: partnerTreeServiceSpy }
      ]
    });

    httpTestingController = TestBed.inject(HttpTestingController);
    mockPartnerTreeService = TestBed.inject(PartnerTreeService) as jasmine.SpyObj<PartnerTreeService>;
    
    // Create the service which will make constructor requests
    service = TestBed.inject(CachedDataService);
    
    // Handle all constructor HTTP requests
    const constructorRequests = httpTestingController.match(() => true);
    constructorRequests.forEach(req => {
      // Flush with appropriate response based on URL
      if (req.request.url.includes('/api/values/')) {
        req.flush([]);
      } else if (req.request.url.includes('/api/current-user-data')) {
        req.flush({});
      } else {
        req.flush([]);
      }
    });
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initial state', () => {
    it('should initialize with empty signals', () => {
      // Note: Constructor calls load methods, so we need to handle the HTTP requests
      const requests = httpTestingController.match(() => true);
      requests.forEach(req => req.flush([]));

      expect(service.allProjects()).toEqual([]);
      expect(service.allSDGs()).toEqual([]);
      expect(service.allCountries()).toEqual([]);
      expect(service.allCurrencies()).toEqual([]);
      expect(service.isLoading()).toBe(false);
    });

    it('should load static data on construction', () => {
      // Constructor requests were already handled in beforeEach
      // Just verify that the service was created and has data
      expect(service).toBeTruthy();
      // Verify that partner tree service was called
      expect(mockPartnerTreeService.getCategoryAndGroupStructure).toHaveBeenCalled();
      
      expect(service.allSalutations().length).toBeGreaterThan(0);
      expect(service.allStatus().length).toBeGreaterThan(0);
      expect(service.allPronouns().length).toBeGreaterThan(0);
    });
  });

  describe('clearCachedData', () => {
    it('should reset all cached data signals', () => {
      // Load some data first
      service.loadProjects();
      const projectReq = httpTestingController.expectOne('/api/unops/project');
      projectReq.flush(mockProjects);
      
      expect(service.allProjects()).toEqual(mockProjects as any);
      
      // Clear cache
      service.clearCachedData();
      
      expect(service.allProjects()).toEqual([]);
      expect(service.allSDGs()).toEqual([]);
      expect(service.allCountries()).toEqual([]);
      expect(service.partnerCategoryGroups()).toEqual([]);
    });
  });

  describe('loadProjects', () => {
    it('should load projects when cache is empty', () => {
      service.clearCachedData();
      service.loadProjects();
      
      expect(service.isLoading()).toBe(true);
      
      const req = httpTestingController.expectOne('/api/unops/project');
      req.flush(mockProjects);
      
      expect(service.allProjects()).toEqual(mockProjects as any);
      expect(service.isLoading()).toBe(false);
    });

    it('should not reload projects when cache is populated', () => {
      // First load
      service.loadProjects();
      const req1 = httpTestingController.expectOne('/api/unops/project');
      req1.flush(mockProjects);
      
      // Second load should not make HTTP request
      service.loadProjects();
      httpTestingController.expectNone('/api/unops/project');
      
      expect(service.allProjects()).toEqual(mockProjects as any);
    });

    it('should handle load error', () => {
      service.clearCachedData();
      service.loadProjects();
      
      const req = httpTestingController.expectOne('/api/unops/project');
      req.error(new ProgressEvent('error'));
      
      expect(service.isLoading()).toBe(false);
      expect(service.allProjects()).toEqual([]);
    });
  });

  describe('loadSDGs', () => {
    it('should load SDGs when cache is empty', () => {
      service.clearCachedData();
      service.loadSDGs();
      
      expect(service.isLoading()).toBe(true);
      
      const req = httpTestingController.expectOne('/api/values/sdg');
      req.flush([{ id: '1', name: 'SDG 1' }]);
      
      expect(service.allSDGs()).toEqual([{ id: '1', name: 'SDG 1' }] as any);
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('loadPartnerCategoryGroups', () => {
    it('should load partner category groups through PartnerTreeService', () => {
      service.clearCachedData();
      service.loadPartnerCategoryGroups();
      
      // Since the observable is synchronous in tests, loading completes immediately
      expect(mockPartnerTreeService.getCategoryAndGroupStructure).toHaveBeenCalled();
      expect(service.partnerCategoryGroups()).toEqual(mockPartnerCategoryGroups);
      expect(service.isLoading()).toBe(false);
    });

    it('should handle partner category groups load error', () => {
      mockPartnerTreeService.getCategoryAndGroupStructure.and.returnValue(throwError(() => new Error('Load error')));
      spyOn(console, 'error');
      
      service.clearCachedData();
      service.loadPartnerCategoryGroups();
      
      expect(console.error).toHaveBeenCalledWith('Error fetching partner category and group structure:', jasmine.any(Error));
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('getParterGroupByCategoryCode', () => {
    beforeEach(() => {
      // Ensure partner category groups are loaded
      service.clearCachedData();
      service.loadPartnerCategoryGroups();
    });

    it('should return groups for valid category code', () => {
      const groups = service.getParterGroupByCategoryCode('CAT1');
      
      expect(groups).toEqual([
        { partnerGroupId: 1, partnerGroupCode: 'GRP1', partnerGroupName: 'Group 1' },
        { partnerGroupId: 2, partnerGroupCode: 'GRP2', partnerGroupName: 'Group 2' }
      ]);
    });

    it('should return empty array for invalid category code', () => {
      const groups = service.getParterGroupByCategoryCode('INVALID');
      
      expect(groups).toEqual([]);
    });

    it('should return empty array for undefined category code', () => {
      const groups = service.getParterGroupByCategoryCode(undefined);
      
      expect(groups).toEqual([]);
    });
  });

  describe('getPartnerGroupsForSelect computed', () => {
    beforeEach(() => {
      service.clearCachedData();
      service.loadPartnerCategoryGroups();
    });

    it('should transform partner category groups for select dropdown', () => {
      const selectData = service.getPartnerGroupsForSelect();

      expect(selectData).toEqual([
        {
          name: 'Category 1',
          value: 'CAT1',
          items: [
            { name: 'Group 1', value: 'GRP1', searchText: 'category 1 group 1' },
            { name: 'Group 2', value: 'GRP2', searchText: 'category 1 group 2' }
          ]
        },
        {
          name: 'Category 2',
          value: 'CAT2',
          items: [
            { name: 'Group 3', value: 'GRP3', searchText: 'category 2 group 3' }
          ]
        }
      ]);
    });

    it('should return empty array when no category groups loaded', () => {
      service.clearCachedData();
      
      const selectData = service.getPartnerGroupsForSelect();
      
      expect(selectData).toEqual([]);
    });
  });

  describe('loadContacts', () => {
    it('should load contacts and ensure array format', () => {
      service.clearCachedData();
      service.loadContacts();
      
      const req = httpTestingController.expectOne('/api/values/contacts');
      req.flush(mockContacts);
      
      expect(service.allContacts()).toEqual(mockContacts);
      expect(service.isLoading()).toBe(false);
    });

    it('should handle non-array response for contacts', () => {
      service.clearCachedData();
      service.loadContacts();
      
      const req = httpTestingController.expectOne('/api/values/contacts');
      req.flush({ invalid: 'response' });
      
      expect(service.allContacts()).toEqual([]);
    });

    it('should handle contacts load error', () => {
      service.clearCachedData();
      service.loadContacts();
      
      const req = httpTestingController.expectOne('/api/values/contacts');
      req.error(new ProgressEvent('error'));
      
      expect(service.allContacts()).toEqual([]);
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('loadPartners', () => {
    it('should load partners and ensure array format', () => {
      const mockPartners = [
        { id: '1', name: 'Partner 1' },
        { id: '2', name: 'Partner 2' }
      ];
      
      service.clearCachedData();
      service.loadPartners();
      
      const req = httpTestingController.expectOne('/api/values/partners');
      req.flush(mockPartners);
      
      expect(service.allPartners()).toEqual(mockPartners);
    });

    it('should handle non-array response for partners', () => {
      service.clearCachedData();
      service.loadPartners();
      
      const req = httpTestingController.expectOne('/api/values/partners');
      req.flush('invalid response');
      
      expect(service.allPartners()).toEqual([]);
    });
  });

  describe('loadCurrentUserData', () => {
    it('should load current user data', () => {
      const mockUser = { id: '1', name: 'Test User', email: 'test@example.com' };
      
      service.loadCurrentUserData();
      
      const req = httpTestingController.expectOne('/api/current-user-data');
      req.flush(mockUser);
      
      expect(service.currentUser()).toEqual(mockUser);
      expect(service.isLoading()).toBe(false);
    });

    it('should not reload user data if already loaded', () => {
      const mockUser = { id: '1', name: 'Test User' };
      
      // First load
      service.loadCurrentUserData();
      const req1 = httpTestingController.expectOne('/api/current-user-data');
      req1.flush(mockUser);
      
      expect(service.currentUser()).toEqual(mockUser);
      
      // Second load should not make HTTP request
      service.loadCurrentUserData();
      httpTestingController.expectNone('/api/current-user-data');
      
      // User data should still be the same
      expect(service.currentUser()).toEqual(mockUser);
    });

    it('should handle user data load error', () => {
      service.loadCurrentUserData();
      
      const req = httpTestingController.expectOne('/api/current-user-data');
      req.error(new ProgressEvent('error'));
      
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('static data loaders', () => {
    it('should load salutations correctly', () => {
      const salutations = service.allSalutations();
      
      expect(salutations).toContain(jasmine.objectContaining({ id: 'Mr.', name: 'Mr.' }));
      expect(salutations).toContain(jasmine.objectContaining({ id: 'Ms.', name: 'Ms.' }));
      expect(salutations).toContain(jasmine.objectContaining({ id: 'Dr.', name: 'Dr.' }));
    });

    it('should load pronouns correctly', () => {
      const pronouns = service.allPronouns();
      
      expect(pronouns).toContain(jasmine.objectContaining({ id: 'He/Him', name: 'He/Him' }));
      expect(pronouns).toContain(jasmine.objectContaining({ id: 'She/Her', name: 'She/Her' }));
      expect(pronouns).toContain(jasmine.objectContaining({ id: 'They/Them', name: 'They/Them' }));
    });

    it('should load partner level types correctly', () => {
      const levelTypes = service.allPartnerLevelTypes();
      
      expect(levelTypes).toContain(jasmine.objectContaining({ id: 'Level_1', name: 'Level 1' }));
      expect(levelTypes).toContain(jasmine.objectContaining({ id: 'Level_4', name: 'Level 4' }));
    });
  });

  describe('integration scenarios', () => {
    it('should handle multiple concurrent loads', () => {
      service.clearCachedData();
      
      // Start multiple loads
      service.loadProjects();
      service.loadSDGs();
      service.loadContacts();
      
      expect(service.isLoading()).toBe(true);
      
      // Complete all requests
      const requests = httpTestingController.match(() => true);
      requests.forEach(req => {
        if (req.request.url.includes('project')) {
          req.flush(mockProjects);
        } else if (req.request.url.includes('contacts')) {
          req.flush(mockContacts);
        } else {
          req.flush([]);
        }
      });
      
      expect(service.allProjects()).toEqual(mockProjects as any);
      expect(service.allContacts()).toEqual(mockContacts);
      expect(service.isLoading()).toBe(false);
    });

    it('should maintain cache across operations', () => {
      service.clearCachedData();
      
      // Load data
      service.loadProjects();
      const req = httpTestingController.expectOne('/api/unops/project');
      req.flush(mockProjects);
      
      // Data should persist
      expect(service.allProjects()).toEqual(mockProjects as any);
      
      // Load other data shouldn't affect projects
      service.loadSDGs();
      const sdgReq = httpTestingController.expectOne('/api/values/sdg');
      sdgReq.flush([]);
      
      expect(service.allProjects()).toEqual(mockProjects as any);
    });
  });
});