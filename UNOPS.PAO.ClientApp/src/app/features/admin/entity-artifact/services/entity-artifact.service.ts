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
  valueDate?: string | null;
  valueJson?: string | null;
  documentId?: number | null;
  effectiveDate?: string | null;
  expiryDate?: string | null;
  source?: string | null;
  metadata?: string | null;
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
   * Get all artifacts for a specific entity
   */
  getEntityArtifacts(entityType: string, entityId: number): Observable<EntityArtifactResponse[]> {
    const params = new HttpParams()
      .set('entityType', entityType)
      .set('entityId', entityId.toString());
    return this.http.get<EntityArtifactResponse[]>(`${this.baseUrl}/list`, { params });
  }
}

