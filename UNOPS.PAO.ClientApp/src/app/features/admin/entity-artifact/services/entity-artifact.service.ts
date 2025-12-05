import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EntityTypeOption {
  entityType: string;
  displayName: string;
}

export interface ArtifactTypeResponse {
  id: number;
  name: string;
  artifactTypeCode: string;
  artifactDataTypeId: number;
  artifactDataTypeName: string | null;
  description: string | null;
  category: string | null;
  applicableEntityTypes: string | null;
  isUsedForCalculations: boolean;
  isUsedForAI: boolean;
  order: number;
  source: string | null;
  isSearchable: boolean;
  allowBulkUpdate: boolean;
}

export interface EntityRecordOption {
  id: number;
  name: string;
  description: string | null;
}

export interface EntityArtifactResponse {
  id: number;
  entityType: string;
  entityId: number;
  artifactTypeId: number;
  artifactTypeName: string | null;
  artifactTypeCode: string | null;
  dataTypeName: string | null;
  name: string | null;
  valueText: string | null;
  valueNumber: number | null;
  valueBoolean: boolean | null;
  valueDate: string | null;
  valueJson: string | null;
  documentId: number | null;
  documentName: string | null;
  effectiveDate: string | null;
  expiryDate: string | null;
  source: string | null;
  isExtracted: boolean;
  sourceArtifactId: number | null;
  metadata: string | null;
  confidenceScore: number | null;
  createdDate: string;
  createdBy: number;
  createdByName: string | null;
  lastModifiedDate: string | null;
  lastModifiedBy: number | null;
  lastModifiedByName: string | null;
}

export interface EntityArtifactRequest {
  entityType: string;
  entityId: number;
  artifactTypeId: number;
  name?: string | null;
  valueText?: string | null;
  valueNumber?: number | null;
  valueBoolean?: boolean | null;
  valueDate?: string | null;
  valueJson?: string | null;
  documentId?: number | null;
  effectiveDate?: string | null;
  expiryDate?: string | null;
  source?: string | null;
  metadata?: string | null;
}

// Bulk Entity Artifact Interfaces
export interface EntityUniqueIdExampleResponse {
  entityType: string;
  uniqueIdFieldName: string;
  uniqueIdFieldLabel: string;
  description: string;
  exampleValue: string;
  exampleEntityName: string;
  fullExplanation: string;
}

export interface BulkTemplateDownloadRequest {
  entityType: string;
  artifactTypeIds: number[];
}

export interface BulkEntityArtifactRowRequest {
  rowNumber: number;
  uniqueId: string;
  cellValues: { [columnIndex: number]: string };
}

export interface BulkEntityArtifactRequest {
  entityType: string;
  rows: BulkEntityArtifactRowRequest[];
  columnToArtifactTypeMapping: { [columnIndex: number]: number };
}

export interface BulkEntityArtifactCellResult {
  columnIndex: number;
  artifactTypeId: number;
  artifactTypeName: string | null;
  success: boolean;
  errorMessage: string | null;
  previousValue: string | null;
  currentValue: string | null;
  isNew: boolean;
  skipped: boolean;
}

export interface BulkEntityArtifactRowResult {
  rowNumber: number;
  uniqueId: string;
  entityId: number | null;
  entityName: string | null;
  success: boolean;
  errorMessage: string | null;
  cellResults: BulkEntityArtifactCellResult[];
}

export interface BulkEntityArtifactResponse {
  entityType: string;
  totalRows: number;
  successfulRows: number;
  failedRows: number;
  rowResults: BulkEntityArtifactRowResult[];
}

@Injectable({
  providedIn: 'root'
})
export class EntityArtifactService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/entity-artifacts';

  /**
   * Get all available entity types
   */
  getEntityTypes(): Observable<EntityTypeOption[]> {
    return this.http.get<EntityTypeOption[]>(`${this.baseUrl}/entity-types`);
  }

  /**
   * Get artifact types filtered by entity type
   */
  getArtifactTypesByEntityType(entityType: string): Observable<ArtifactTypeResponse[]> {
    const params = new HttpParams().set('entityType', entityType);
    return this.http.get<ArtifactTypeResponse[]>(`${this.baseUrl}/artifact-types`, { params });
  }

  /**
   * Get entity records for dropdown (e.g., list of countries, partners, etc.)
   */
  getEntityRecords(entityType: string, searchTerm?: string): Observable<EntityRecordOption[]> {
    let params = new HttpParams().set('entityType', entityType);
    if (searchTerm) {
      params = params.set('searchTerm', searchTerm);
    }
    return this.http.get<EntityRecordOption[]>(`${this.baseUrl}/entity-records`, { params });
  }

  /**
   * Get existing artifact value for a specific entity and artifact type
   */
  getEntityArtifact(entityType: string, entityId: number, artifactTypeId: number): Observable<EntityArtifactResponse> {
    const params = new HttpParams()
      .set('entityType', entityType)
      .set('entityId', entityId.toString())
      .set('artifactTypeId', artifactTypeId.toString());
    return this.http.get<EntityArtifactResponse>(`${this.baseUrl}/get`, { params });
  }

  /**
   * Upsert (create or update) an entity artifact
   */
  upsertEntityArtifact(request: EntityArtifactRequest): Observable<EntityArtifactResponse> {
    return this.http.post<EntityArtifactResponse>(`${this.baseUrl}/upsert`, request);
  }

  /**
   * Upload a document artifact to Google Cloud Storage
   * Documents are stored with folder path: EntityArtifacts/{ArtifactCode}/{Entity}/{EntityId}/
   * The GCS URL is stored in ValueText instead of base64 in ValueJson
   */
  uploadDocumentArtifact(
    entityType: string,
    entityId: number,
    artifactTypeId: number,
    artifactTypeCode: string,
    file: File,
    name?: string,
    source?: string
  ): Observable<EntityArtifactResponse> {
    const formData = new FormData();
    formData.append('EntityType', entityType);
    formData.append('EntityId', entityId.toString());
    formData.append('ArtifactTypeId', artifactTypeId.toString());
    formData.append('ArtifactTypeCode', artifactTypeCode);
    formData.append('File', file);
    if (name) {
      formData.append('Name', name);
    }
    if (source) {
      formData.append('Source', source);
    }
    return this.http.post<EntityArtifactResponse>(`${this.baseUrl}/upload-document`, formData);
  }

  /**
   * Get a signed URL for viewing/downloading a document artifact
   */
  getDocumentUrl(entityType: string, entityId: number, artifactTypeId: number): Observable<{ url: string; fileName: string }> {
    const params = new HttpParams()
      .set('entityType', entityType)
      .set('entityId', entityId.toString())
      .set('artifactTypeId', artifactTypeId.toString());
    return this.http.get<{ url: string; fileName: string }>(`${this.baseUrl}/document-url`, { params });
  }

  /**
   * Get all artifacts for a specific entity
   */
  getEntityArtifacts(entityType: string, entityId: number): Observable<EntityArtifactResponse[]> {
    const params = new HttpParams()
      .set('entityType', entityType)
      .set('entityId', entityId.toString());
    return this.http.get<EntityArtifactResponse[]>(`${this.baseUrl}/list`, { params });
  }

  // Bulk Entity Artifact Operations

  /**
   * Get artifact types for bulk operations (filtered by AllowBulkUpdate = true)
   */
  getBulkArtifactTypesByEntityType(entityType: string): Observable<ArtifactTypeResponse[]> {
    const params = new HttpParams().set('entityType', entityType);
    return this.http.get<ArtifactTypeResponse[]>(`${this.baseUrl}/bulk/artifact-types`, { params });
  }

  /**
   * Get unique identifier example for bulk import template
   */
  getBulkUniqueIdExample(entityType: string): Observable<EntityUniqueIdExampleResponse> {
    const params = new HttpParams().set('entityType', entityType);
    return this.http.get<EntityUniqueIdExampleResponse>(`${this.baseUrl}/bulk/unique-id-example`, { params });
  }

  /**
   * Download CSV template for bulk import
   */
  downloadBulkTemplate(request: BulkTemplateDownloadRequest): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/bulk/template-download`, request, {
      responseType: 'blob'
    });
  }

  /**
   * Bulk upsert entity artifacts from CSV data
   */
  bulkUpsertEntityArtifacts(request: BulkEntityArtifactRequest): Observable<BulkEntityArtifactResponse> {
    return this.http.post<BulkEntityArtifactResponse>(`${this.baseUrl}/bulk/upsert`, request);
  }
}

