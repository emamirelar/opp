import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { PermissionService, EntityPermissions } from '@core/services/permission.service';

export interface EntityDropdownModel {
  id: number;
  entityName: string;
}

export interface EntityFieldConfigurationDto {
  id?: number; // Optional for new fields
  fieldName: string;
  dataType: string;
  description?: string;
  isRequired: boolean;
  isActive: boolean;
  enableChangeLog: boolean; // Whether change logging is enabled for this field
  defaultValue?: string;
  maxLength?: number;
  displayOrder: number;
  showInListView: boolean;
  listViewOrder?: number;
  relatedDisplayProperty?: string; // For relationship fields, specifies which property of related entity to display
  displayTemplate?: string; // Template pattern for combining multiple fields and accessing field paths
  listViewLabel?: string; // Custom label for the list view column
  listViewType?: string; // Type of list view column: text, avatar, template, multiple-avatars
  listViewWidth?: string; // Column width in list view
  listViewEllipsis?: boolean; // Whether to show ellipsis for long text
  listViewSortable?: boolean; // Whether the column is sortable
  firstLetterFallbackField?: string; // Field for avatar initials fallback
  helperText?: string; // Helper text to assist users with field completion
}

export interface EntityConfigurationDetailsResponse {
  id: number;
  entityName: string;
  tableName: string;
  description?: string;
  isActive: boolean;
  enableChangeLog: boolean;
  fields: EntityFieldConfigurationDto[];
}

export interface UpdateEntityConfigurationRequest {
  id: number;
  entityName: string;
  tableName: string;
  description?: string;
  isActive: boolean;
  enableChangeLog: boolean;
}

export interface UpdateEntityFieldRequest {
  fieldName: string;
  dataType: string;
  description?: string;
  isRequired: boolean;
  isActive: boolean;
  defaultValue?: string;
  maxLength?: number;
  displayOrder: number;
  showInListView: boolean;
  listViewOrder?: number;
  relatedDisplayProperty?: string;
  displayTemplate?: string;
  listViewLabel?: string;
  listViewType?: string;
  listViewWidth?: string;
  listViewEllipsis?: boolean;
  listViewSortable?: boolean;
  helperText?: string;
}

export interface SaveEntityConfigurationRequest {
  entityName: string;
  description?: string;
  fields: EntityFieldConfigurationDto[];
}

export interface EntityPermissionsModel {
  entity: string;
  canCreate: boolean;
  canRead: boolean;
  canUpdate: boolean;
  canDelete: boolean;
  canExport: boolean;
  canImport: boolean;
}

export interface RelatedFieldOption {
  value: string;           // 'name', 'shortName', 'name,shortName'
  label: string;           // 'Name', 'Short Name', 'Name (Short Name)'
  isTemplate: boolean;     // true for combined fields
  templatePattern?: string; // '{name} ({shortName})'
  fieldPath?: string;      // 'partner.name', 'partner.shortName'
}

export interface ListViewColumn {
  field: string;           // Field name or path
  label: string;           // Display label
  type: string;            // 'text', 'avatar', 'template', 'multiple-avatars'
  sortable: boolean;       // Whether the column is sortable
  width?: string;          // Column width (e.g., '15%', '200px')
  ellipsis?: boolean;      // Whether to show ellipsis for long text
  templatePattern?: string; // Template pattern for 'template' type
  firstLetterFallbackField?: string; // Field for avatar initials fallback
  helperText?: string;     // Helper text to show in column header tooltip
}

@Injectable({
  providedIn: 'root'
})
export class EntityConfigurationService {
  private http = inject(HttpClient);
  private permissionService = inject(PermissionService);
  private apiUrl = '/api/entity-configuration';

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json'
    });
  }

  /**
   * Get all entities for dropdown
   */
  getEntities(): Observable<EntityDropdownModel[]> {
    return this.http.get<EntityDropdownModel[]>(`/api/entities`, {
      headers: this.getHeaders()
    });
  }

  /**
   * Get entity configuration details with fields
   */
  getEntityConfiguration(entityName: string): Observable<EntityConfigurationDetailsResponse> {
    return this.http.get<EntityConfigurationDetailsResponse>(`${this.apiUrl}/${encodeURIComponent(entityName)}`, {
      headers: this.getHeaders()
    });
  }

  /**
   * Update entity configuration
   */
  updateEntityConfiguration(id: number, request: UpdateEntityConfigurationRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, request, {
      headers: this.getHeaders()
    });
  }

  /**
   * Save entity configuration with fields (bulk update)
   */
  saveEntityConfiguration(entityName: string, request: SaveEntityConfigurationRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/${encodeURIComponent(entityName)}/save`, request, {
      headers: this.getHeaders()
    });
  }

  /**
   * Update entity field
   */
  updateEntityField(entityName: string, fieldId: number, request: UpdateEntityFieldRequest): Observable<any> {
    return this.http.put(`${this.apiUrl}/${encodeURIComponent(entityName)}/fields/${fieldId}`, request, {
      headers: this.getHeaders()
    });
  }

  /**
   * Get entity permissions for current user
   */
  getEntityPermissions(): Observable<EntityPermissionsModel> {
    return this.permissionService.getEntityPermissions('EntityManager').pipe(
      map((permissions: EntityPermissions) => ({
        entity: permissions.entity,
        canCreate: permissions.permissions.canCreate,
        canRead: permissions.permissions.canRead,
        canUpdate: permissions.permissions.canUpdate,
        canDelete: permissions.permissions.canDelete,
        canExport: permissions.permissions.canExport,
        canImport: permissions.permissions.canImport
      }))
    );
  }

  /**
   * Update list view fields for an entity
   */
  updateListViewFields(entityName: string, fieldIds: number[]): Observable<any> {
    return this.http.put(`${this.apiUrl}/${encodeURIComponent(entityName)}/listview`, { fieldIds }, {
      headers: this.getHeaders()
    });
  }

  /**
   * Get available fields for a related entity type (for relationship field configuration)
   */
  getRelatedEntityFields(entityType: string): Observable<RelatedFieldOption[]> {
    return this.http.get<RelatedFieldOption[]>(`${this.apiUrl}/related-fields/${encodeURIComponent(entityType)}`, {
      headers: this.getHeaders()
    });
  }

  /**
   * Get field options for a specific data type in the context of an entity
   */
  getFieldOptionsForDataType(dataType: string, contextEntityName: string): Observable<RelatedFieldOption[]> {
    return this.http.get<RelatedFieldOption[]>(`${this.apiUrl}/field-options/${encodeURIComponent(dataType)}/${encodeURIComponent(contextEntityName)}`, {
      headers: this.getHeaders()
    });
  }

  /**
   * Get list view configuration for an entity (for dynamic column generation)
   */
  getEntityListViewConfiguration(entityName: string): Observable<ListViewColumn[]> {
    return this.http.get<ListViewColumn[]>(`${this.apiUrl}/${encodeURIComponent(entityName)}/list-view`, {
      headers: this.getHeaders()
    });
  }

  /**
   * Get sample data for template preview (using existing APIs)
   */
  getSampleData(entityName: string): Observable<any> {
    const entityLower = entityName.toLowerCase();
    let apiUrl = '';
    
    switch (entityLower) {
      case 'partner':
        apiUrl = '/api/partner?page=1&pageSize=1';
        break;
      case 'contact':
        apiUrl = '/api/contact?page=1&pageSize=1';
        break;
      case 'interaction':
        apiUrl = '/api/interactions?page=1&pageSize=1';
        break;
      default:
        apiUrl = `/api/${entityLower}?page=1&pageSize=1`;
    }

    return this.http.get<any>(apiUrl, {
      headers: this.getHeaders()
    }).pipe(
      map((response: any) => {
        // Extract first record based on response structure
        if (response.data && response.data.length > 0) {
          return response.data[0];
        } else if (response.records && response.records.length > 0) {
          return response.records[0];
        } else if (Array.isArray(response) && response.length > 0) {
          return response[0];
        }
        return null;
      })
    );
  }

  /**
   * Exports all entity configurations as a single SQL script file
   */
  exportEntityConfigurationAsSql(): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export-sql`, {
      headers: this.getHeaders(),
      responseType: 'blob'
    });
  }
} 