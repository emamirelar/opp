import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EntityConfigurationService, EntityDropdownModel, EntityConfigurationDetailsResponse, UpdateEntityConfigurationRequest } from './entity-configuration.service';
import { PermissionService, EntityPermissions } from '@core/services/auth';
import { of } from 'rxjs';

describe('EntityConfigurationService', () => {
  let service: EntityConfigurationService;
  let httpMock: HttpTestingController;
  let mockPermissionService: jasmine.SpyObj<PermissionService>;

  beforeEach(() => {
    mockPermissionService = jasmine.createSpyObj('PermissionService', ['getEntityPermissions']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        EntityConfigurationService,
        { provide: PermissionService, useValue: mockPermissionService }
      ]
    });

    service = TestBed.inject(EntityConfigurationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get all entities', (done) => {
    const mockEntities: EntityDropdownModel[] = [
      { id: 1, entityName: 'Contact' },
      { id: 2, entityName: 'Partner' }
    ];

    service.getEntities().subscribe(entities => {
      expect(entities).toEqual(mockEntities);
      done();
    });

    const req = httpMock.expectOne('/api/entities');
    expect(req.request.method).toBe('GET');
    expect(req.request.headers.get('Content-Type')).toBe('application/json');
    req.flush(mockEntities);
  });

  it('should get entity configuration', (done) => {
    const entityName = 'Contact';
    const mockConfig: EntityConfigurationDetailsResponse = {
      id: 1,
      entityName: 'Contact',
      tableName: 'contact',
      description: 'Contact entity',
      isActive: true,
      enableChangeLog: true,
      fields: []
    };

    service.getEntityConfiguration(entityName).subscribe(config => {
      expect(config).toEqual(mockConfig);
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/Contact');
    expect(req.request.method).toBe('GET');
    req.flush(mockConfig);
  });

  it('should handle entity names with special characters', (done) => {
    const entityName = 'Organization/Partner';
    const mockConfig: EntityConfigurationDetailsResponse = {
      id: 1,
      entityName: entityName,
      tableName: 'org_partner',
      isActive: true,
      enableChangeLog: true,
      fields: []
    };

    service.getEntityConfiguration(entityName).subscribe(config => {
      expect(config).toEqual(mockConfig);
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/Organization%2FPartner');
    expect(req.request.method).toBe('GET');
    req.flush(mockConfig);
  });

  it('should update entity configuration', (done) => {
    const id = 1;
    const request: UpdateEntityConfigurationRequest = {
      id: 1,
      entityName: 'Contact',
      tableName: 'contact',
      description: 'Updated description',
      isActive: true,
      enableChangeLog: true
    };

    service.updateEntityConfiguration(id, request).subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/1');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(request);
    req.flush({ success: true });
  });

  it('should save entity configuration with fields', (done) => {
    const entityName = 'Contact';
    const request = {
      entityName: 'Contact',
      description: 'Contact entity',
      fields: []
    };

    service.saveEntityConfiguration(entityName, request).subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/Contact/save');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush({ success: true });
  });

  it('should update entity field', (done) => {
    const entityName = 'Contact';
    const fieldId = 123;
    const request = {
      fieldName: 'email',
      dataType: 'string',
      isRequired: true,
      isActive: true,
      displayOrder: 1,
      showInListView: true
    };

    service.updateEntityField(entityName, fieldId, request as any).subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/Contact/fields/123');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(request);
    req.flush({ success: true });
  });

  it('should get entity permissions', (done) => {
    const mockPermissions: EntityPermissions = {
      entity: 'EntityManager',
      hasAccess: true,
      permissions: {
        canCreate: true,
        canRead: true,
        canUpdate: true,
        canDelete: false,
        canExport: true,
        canImport: false
      }
    };

    mockPermissionService.getEntityPermissions.and.returnValue(of(mockPermissions));

    service.getEntityPermissions().subscribe(permissions => {
      expect(permissions.entity).toBe('EntityManager');
      expect(permissions.canCreate).toBe(true);
      expect(permissions.canRead).toBe(true);
      expect(permissions.canUpdate).toBe(true);
      expect(permissions.canDelete).toBe(false);
      done();
    });
  });

  it('should update list view fields', (done) => {
    const entityName = 'Contact';
    const fieldIds = [1, 2, 3];

    service.updateListViewFields(entityName, fieldIds).subscribe(response => {
      expect(response).toBeTruthy();
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/Contact/listview');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ fieldIds });
    req.flush({ success: true });
  });

  it('should get related entity fields', (done) => {
    const entityType = 'Partner';
    const mockFields = [
      { value: 'name', label: 'Name', isTemplate: false },
      { value: 'shortName', label: 'Short Name', isTemplate: false }
    ];

    service.getRelatedEntityFields(entityType).subscribe(fields => {
      expect(fields).toEqual(mockFields);
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/related-fields/Partner');
    expect(req.request.method).toBe('GET');
    req.flush(mockFields);
  });

  it('should get field options for data type', (done) => {
    const dataType = 'relationship';
    const contextEntityName = 'Contact';
    const mockOptions = [
      { value: 'partner.name', label: 'Partner Name', isTemplate: false }
    ];

    service.getFieldOptionsForDataType(dataType, contextEntityName).subscribe(options => {
      expect(options).toEqual(mockOptions);
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/field-options/relationship/Contact');
    expect(req.request.method).toBe('GET');
    req.flush(mockOptions);
  });

  it('should get entity list view configuration', (done) => {
    const entityName = 'Contact';
    const mockColumns = [
      { field: 'name', label: 'Name', type: 'text', sortable: true }
    ];

    service.getEntityListViewConfiguration(entityName).subscribe(columns => {
      expect(columns).toEqual(mockColumns);
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/Contact/list-view');
    expect(req.request.method).toBe('GET');
    req.flush(mockColumns);
  });

  it('should get sample data for entity', (done) => {
    const entityName = 'Partner';
    const mockResponse = {
      data: [{ id: 1, name: 'Sample Partner' }]
    };

    service.getSampleData(entityName).subscribe(data => {
      expect(data).toEqual({ id: 1, name: 'Sample Partner' });
      done();
    });

    const req = httpMock.expectOne('/api/partner?page=1&pageSize=1');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get sample data for contact entity', (done) => {
    const entityName = 'Contact';
    const mockResponse = {
      data: [{ id: 1, name: 'Sample Contact' }]
    };

    service.getSampleData(entityName).subscribe(data => {
      expect(data).toEqual({ id: 1, name: 'Sample Contact' });
      done();
    });

    const req = httpMock.expectOne('/api/contact?page=1&pageSize=1');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should get sample data for interaction entity', (done) => {
    const entityName = 'Interaction';
    const mockResponse = {
      data: [{ id: 1, type: 'Meeting' }]
    };

    service.getSampleData(entityName).subscribe(data => {
      expect(data).toEqual({ id: 1, type: 'Meeting' });
      done();
    });

    const req = httpMock.expectOne('/api/interactions?page=1&pageSize=1');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);
  });

  it('should handle sample data with records array', (done) => {
    const entityName = 'Partner';
    const mockResponse = {
      records: [{ id: 1, name: 'Sample Partner' }]
    };

    service.getSampleData(entityName).subscribe(data => {
      expect(data).toEqual({ id: 1, name: 'Sample Partner' });
      done();
    });

    const req = httpMock.expectOne('/api/partner?page=1&pageSize=1');
    req.flush(mockResponse);
  });

  it('should handle sample data with array response', (done) => {
    const entityName = 'Partner';
    const mockResponse = [{ id: 1, name: 'Sample Partner' }];

    service.getSampleData(entityName).subscribe(data => {
      expect(data).toEqual({ id: 1, name: 'Sample Partner' });
      done();
    });

    const req = httpMock.expectOne('/api/partner?page=1&pageSize=1');
    req.flush(mockResponse);
  });

  it('should return null for empty sample data', (done) => {
    const entityName = 'Partner';
    const mockResponse = { data: [] };

    service.getSampleData(entityName).subscribe(data => {
      expect(data).toBeNull();
      done();
    });

    const req = httpMock.expectOne('/api/partner?page=1&pageSize=1');
    req.flush(mockResponse);
  });

  it('should export entity configuration as SQL', (done) => {
    const mockBlob = new Blob(['SQL content'], { type: 'text/plain' });

    service.exportEntityConfigurationAsSql().subscribe(blob => {
      expect(blob).toEqual(mockBlob);
      done();
    });

    const req = httpMock.expectOne('/api/entity-configuration/export-sql');
    expect(req.request.method).toBe('GET');
    expect(req.request.responseType).toBe('blob');
    req.flush(mockBlob);
  });
});
