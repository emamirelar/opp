/**
 * @fileoverview Service for fetching dropdown/lookup values from the backend
 * @author UNOPS Opportunity+ System Development Team
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * @interface SimpleValue
 * @description Simple value model for dropdowns
 */
export interface SimpleValue {
  id: number;
  name: string;
  code?: string;
  description?: string;
  continent?: string;  // For countries
  region?: string;  // For countries
  logoUrl?: string;  // For partners
  email?: string;  // For users
  pooledFund?: boolean;  // For partners - indicates if this is a pooled funding programme
  partnerId?: number;  // For contacts - the partner they belong to
  type?: string;  // For entity roles - role type classification
  subType?: string;  // For entity roles - role subtype classification
}

/**
 * @interface OrganizationUnit
 * @description Organization unit model
 */
export interface OrganizationUnit {
  id: number;
  name: string;
  code: string;
  type: string;
  status: string;
  description?: string;
}

/**
 * @interface SuggestedOrgUnitsResponse
 * @description Response model for suggested organization units based on countries
 */
export interface SuggestedOrgUnitsResponse {
  suggestedOrgUnitIds: number[];
  primarySuggestionId: number | null;
  suggestionReason: string | null;
}

/**
 * @interface UserBasicModel
 * @description Basic user information
 */
export interface UserBasicModel {
  userId: number;
  name: string | null;
  email: string | null;
}

/**
 * @interface EntityUserRoleGroupModel
 * @description Group of users assigned to a specific entity role
 */
export interface EntityUserRoleGroupModel {
  entityRoleId: number;
  entityRoleName: string | null;
  users: UserBasicModel[];
}

/**
 * @interface EntityUserRolesByOrgUnitResponse
 * @description Response model for entity user roles grouped by role for an organization hierarchy
 */
export interface EntityUserRolesByOrgUnitResponse {
  organizationHierarchyId: number;
  organizationHierarchyName: string | null;
  organizationHierarchyType: string | null;
  roleGroups: EntityUserRoleGroupModel[];
}

/**
 * @interface Output
 * @description Output model for deliverables with hierarchical UNOPS Products and Services List structure
 */
export interface Output {
  id: number;
  name?: string;
  
  // Hierarchical structure (Level 0-4)
  level0?: string;
  level1?: string;
  definitionLevel1?: string;
  level2?: string;
  definitionLevel2?: string;
  level3?: string;
  definitionLevel3?: string;
  level4?: string;
  definitionLevel4?: string;
  
  // Service Line
  serviceLine?: string;
  
  // Component flags for specialist requirement indicators
  grantSupportImplementingModality?: boolean | null;
  grantSupportComponent?: boolean | null;
  procurementComponent?: boolean | null;
  procurementInstallationComponent?: boolean | null;
  infrastructureComponent?: boolean | null;
}

/**
 * @interface SDG
 * @description Sustainable Development Goal model
 */
export interface SDG {
  id: number;
  name: string;
  sdgId?: string;
  sdgNumber?: string;
  sdgDescription?: string;
  sdgLogo?: string;
  sdgLongDescription?: string;
  status: string;
}

/**
 * @interface SDGTarget
 * @description SDG Target reference model
 */
export interface SDGTarget {
  id: number;
  name: string;
  sdgTargetId: string;  // e.g., "1.1", "3.3"
  sdgId: string;  // Parent SDG ID
  targetDescription: string | null;
  targetType: string | null;
}

/**
 * @interface SDGIndicator
 * @description SDG Indicator reference model
 */
export interface SDGIndicator {
  id: number;
  name: string;
  sdgIndicatorId: string;  // e.g., "1.1.1", "3.3.2"
  sdgTargetId: string;  // Parent Target ID
  sdgIndicatorLongDescription: string | null;
}

/**
 * @interface UNCFOutcome
 * @description UN Cooperation Framework (UNCF) Outcome reference model
 */
export interface UNCFOutcome {
  id: number;
  name: string;
  uncfOutcomeExternalId: string | null;  // External ID from source system
  versionNo: number | null;  // Version number
  country: string | null;  // ISO2 country code
}

/**
 * @interface UNCFIndicator
 * @description UNCF Indicator reference model
 */
export interface UNCFIndicator {
  id: number;
  name: string;
  uncfIndicatorExternalId: string | null;  // External ID from source system
  uncfOutcomeExternalId: string | null;  // Parent Outcome External ID
  versionNo: number | null;  // Version number
  country: string | null;  // ISO2 country code
}

/**
 * @interface CountrySearchResult
 * @description Country search result with match context
 */
export interface CountrySearchResult {
  country: {
    id: number;
    name: string;
    iso2Code: string;
    continent?: string;
    region?: string;
  };
  matchReasons: SearchMatchReason[];
  relevanceScore: number;
}

/**
 * @interface SearchMatchReason
 * @description Describes why a country matched the search
 */
export interface SearchMatchReason {
  matchType: 'CountryName' | 'ArtifactValue';
  artifactTypeCode?: string;
  artifactTypeName?: string;
  category?: string;
  matchedValue: string;
  highlightedValue?: string;
}

/**
 * @interface CountrySearchGroups
 * @description Grouped country search results
 */
export interface CountrySearchGroups {
  nameMatches: CountrySearchResult[];
  regionMatches: CountrySearchResult[];
  continentMatches: CountrySearchResult[];
  artifactMatches: { [artifactType: string]: CountrySearchResult[] };
}

/**
 * @interface CountryDynamicSearchResponse
 * @description Dynamic search response with grouped results
 */
export interface CountryDynamicSearchResponse {
  totalMatches: number;
  groups: CountrySearchGroups;
  allResults: CountrySearchResult[];
  metadata: {
    searchTerm: string;
    artifactTypesSearched: number;
    executionTimeMs: number;
    fromCache: boolean;
  };
}

/**
 * @interface CountryDynamicSearchRequest
 * @description Dynamic search request parameters
 */
export interface CountryDynamicSearchRequest {
  searchTerm: string;
  includeArtifacts?: boolean;
  artifactTypeCodes?: string[];
  caseSensitive?: boolean;
  exactMatch?: boolean;
  maxResults?: number;
  highlightMatches?: boolean;
}

/**
 * @class ValuesService
 * @description Service for fetching dropdown/lookup values from the API
 * 
 * @example
 * ```typescript
 * constructor(private valuesService: ValuesService) {}
 * 
 * ngOnInit() {
 *   this.valuesService.getOrganizationUnits().subscribe(units => {
 *     this.organizationUnits = units;
 *   });
 * }
 * ```
 * 
 * @since 1.0.0
 */
@Injectable({
  providedIn: 'root'
})
export class ValuesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/values';

  /**
   * @description Get all organization units (OrgUnit type only)
   * @returns {Observable<OrganizationUnit[]>}
   * @since 1.0.0
   */
  getOrganizationUnits(): Observable<OrganizationUnit[]> {
    return this.http.get<OrganizationUnit[]>(`${this.baseUrl}/organization-units`);
  }

  /**
   * @description Get organization units for Opportunity dropdown (includes OrgUnit, Hub, and Region types)
   * @returns {Observable<OrganizationUnit[]>}
   * @since 1.0.0
   */
  getOpportunityOrganizationUnits(): Observable<OrganizationUnit[]> {
    return this.http.get<OrganizationUnit[]>(`${this.baseUrl}/opportunity-organization-units`);
  }

  /**
   * @description Get all proposed initiative types
   * @returns {Observable<SimpleValue[]>}
   * @since 1.0.0
   */
  getProposedInitiativeTypes(): Observable<SimpleValue[]> {
    return this.http.get<SimpleValue[]>(`${this.baseUrl}/proposed-initiative-types`);
  }

  /**
   * @description Get all outputs
   * @returns {Observable<Output[]>}
   * @since 1.0.0
   */
  getOutputs(): Observable<Output[]> {
    return this.http.get<Output[]>(`${this.baseUrl}/outputs`);
  }

  /**
   * @description Get all SDGs
   * @returns {Observable<SDG[]>}
   * @since 1.0.0
   */
  getSDGs(): Observable<SDG[]> {
    return this.http.get<SDG[]>(`${this.baseUrl}/sdgs`);
  }
  
  /**
   * @description Get all UNOPS Strategic Missions
   * @returns Observable<UNOPSMission[]>
   */
  getUNOPSMissions(): Observable<import('../../models/opportunity.model').UNOPSMission[]> {
    return this.http.get<import('../../models/opportunity.model').UNOPSMission[]>(`${this.baseUrl}/unops-missions`);
  }

  /**
   * @description Get all SDG Targets, optionally filtered by SDG ID
   * @param {string} sdgId - Optional SDG ID to filter targets
   * @returns {Observable<SDGTarget[]>}
   * @since 1.0.0
   */
  getSDGTargets(sdgId?: string): Observable<SDGTarget[]> {
    const url = sdgId
      ? `${this.baseUrl}/sdg-targets?sdgId=${sdgId}`
      : `${this.baseUrl}/sdg-targets`;
    return this.http.get<SDGTarget[]>(url);
  }

  /**
   * @description Get all SDG Indicators, optionally filtered by Target ID
   * @param {string} targetId - Optional Target ID to filter indicators
   * @returns {Observable<SDGIndicator[]>}
   * @since 1.0.0
   */
  getSDGIndicators(targetId?: string): Observable<SDGIndicator[]> {
    const url = targetId
      ? `${this.baseUrl}/sdg-indicators?targetId=${targetId}`
      : `${this.baseUrl}/sdg-indicators`;
    return this.http.get<SDGIndicator[]>(url);
  }

  /**
   * @description Get distinct Level0 values for cascading dropdown
   * @param {Output[]} outputs - Array of all outputs
   * @returns {string[]} Array of distinct Level0 values
   * @since 1.0.0
   */
  getDistinctLevel0(outputs: Output[]): string[] {
    const level0Values = outputs
      .map(o => o.level0)
      .filter((value, index, self) => value && self.indexOf(value) === index) as string[];
    return level0Values.sort();
  }

  /**
   * @description Get distinct Level1 values for a specific Level0
   * @param {Output[]} outputs - Array of all outputs
   * @param {string} level0 - Selected Level0 value (empty returns all Level1)
   * @returns {string[]} Array of distinct Level1 values
   * @since 1.0.0
   */
  getDistinctLevel1(outputs: Output[], level0: string): string[] {
    const filtered = level0 ? outputs.filter(o => o.level0 === level0) : outputs;
    const level1Values = filtered
      .map(o => o.level1)
      .filter((value, index, self) => value && self.indexOf(value) === index) as string[];
    return level1Values.sort();
  }

  /**
   * @description Get distinct Level2 values for a specific Level0 and/or Level1
   * @param {Output[]} outputs - Array of all outputs
   * @param {string} [level0] - Selected Level0 value
   * @param {string} [level1] - Selected Level1 value
   * @returns {string[]} Array of distinct Level2 values
   * @since 1.0.0
   */
  getDistinctLevel2(outputs: Output[], level0?: string, level1?: string): string[] {
    let filtered = outputs;
    if (level0) filtered = filtered.filter(o => o.level0 === level0);
    if (level1) filtered = filtered.filter(o => o.level1 === level1);
    
    const level2Values = filtered
      .map(o => o.level2)
      .filter((value, index, self) => value && self.indexOf(value) === index) as string[];
    return level2Values.sort();
  }

  /**
   * @description Get distinct Level3 values for a specific Level0, Level1, and/or Level2
   * @description Get all UNCF Outcomes (latest version only), optionally filtered by country
   * @param {string} countryCode - Optional ISO2 country code to filter outcomes
   * @returns {Observable<UNCFOutcome[]>}
   * @since 1.0.0
   */
  getUNCFOutcomes(countryCode?: string): Observable<UNCFOutcome[]> {
    const url = countryCode
      ? `${this.baseUrl}/uncf-outcomes?countryCode=${countryCode}`
      : `${this.baseUrl}/uncf-outcomes`;
    return this.http.get<UNCFOutcome[]>(url);
  }

  /**
   * @description Get all UNCF Indicators, optionally filtered by Outcome ID
   * @param {number} outcomeId - Optional Outcome ID (database ID) to filter indicators
   * @returns {Observable<UNCFIndicator[]>}
   * @since 1.0.0
   */
  getUNCFIndicators(outcomeId?: number): Observable<UNCFIndicator[]> {
    const url = outcomeId
      ? `${this.baseUrl}/uncf-indicators?outcomeId=${outcomeId}`
      : `${this.baseUrl}/uncf-indicators`;
    return this.http.get<UNCFIndicator[]>(url);
  }

  /**
   * @description Get distinct output groups for cascading dropdown
   * @param {Output[]} outputs - Array of all outputs
   * @param {string} [level0] - Selected Level0 value
   * @param {string} [level1] - Selected Level1 value
   * @param {string} [level2] - Selected Level2 value
   * @returns {string[]} Array of distinct Level3 values
   * @since 1.0.0
   */
  getDistinctLevel3(outputs: Output[], level0?: string, level1?: string, level2?: string): string[] {
    let filtered = outputs;
    if (level0) filtered = filtered.filter(o => o.level0 === level0);
    if (level1) filtered = filtered.filter(o => o.level1 === level1);
    if (level2) filtered = filtered.filter(o => o.level2 === level2);
    
    const level3Values = filtered
      .map(o => o.level3)
      .filter((value, index, self) => value && self.indexOf(value) === index) as string[];
    return level3Values.sort();
  }

  /**
   * @description Get distinct Level4 values for a specific Level0, Level1, Level2, and/or Level3
   * @param {Output[]} outputs - Array of all outputs
   * @param {string} [level0] - Selected Level0 value
   * @param {string} [level1] - Selected Level1 value
   * @param {string} [level2] - Selected Level2 value
   * @param {string} [level3] - Selected Level3 value
   * @returns {string[]} Array of distinct Level4 values
   * @since 1.0.0
   */
  getDistinctLevel4(outputs: Output[], level0?: string, level1?: string, level2?: string, level3?: string): string[] {
    let filtered = outputs;
    if (level0) filtered = filtered.filter(o => o.level0 === level0);
    if (level1) filtered = filtered.filter(o => o.level1 === level1);
    if (level2) filtered = filtered.filter(o => o.level2 === level2);
    if (level3) filtered = filtered.filter(o => o.level3 === level3);
    
    const level4Values = filtered
      .map(o => o.level4)
      .filter((value, index, self) => value && self.indexOf(value) === index) as string[];
    return level4Values.sort();
  }

  /**
   * @description Get outputs filtered by any combination of levels
   * @param {Output[]} outputs - Array of all outputs
   * @param {string} [level0] - Selected Level0 value
   * @param {string} [level1] - Selected Level1 value
   * @param {string} [level2] - Selected Level2 value
   * @param {string} [level3] - Selected Level3 value
   * @param {string} [level4] - Selected Level4 value
   * @returns {Output[]} Filtered array of outputs
   * @since 1.0.0
   */
  getFilteredOutputsByLevels(
    outputs: Output[], 
    level0?: string, 
    level1?: string, 
    level2?: string, 
    level3?: string, 
    level4?: string
  ): Output[] {
    return outputs.filter(o => {
      const matchesLevel0 = !level0 || o.level0 === level0;
      const matchesLevel1 = !level1 || o.level1 === level1;
      const matchesLevel2 = !level2 || o.level2 === level2;
      const matchesLevel3 = !level3 || o.level3 === level3;
      const matchesLevel4 = !level4 || o.level4 === level4;
      return matchesLevel0 && matchesLevel1 && matchesLevel2 && matchesLevel3 && matchesLevel4;
    }).sort((a, b) => (a.name || '').localeCompare(b.name || ''));
  }

  /**
   * @description Get all countries
   * @returns {Observable<SimpleValue[]>}
   * @since 1.0.0
   */
  getCountries(): Observable<SimpleValue[]> {
    return this.http.get<SimpleValue[]>(`${this.baseUrl}/country`);
  }

  /**
   * @description Get all currencies
   * @returns {Observable<SimpleValue[]>}
   * @since 1.0.0
   */
  getCurrencies(): Observable<SimpleValue[]> {
    return this.http.get<SimpleValue[]>(`${this.baseUrl}/currency`);
  }

  /**
   * @description Get all contacts
   * @returns {Observable<SimpleValue[]>}
   * @since 1.0.0
   */
  getContacts(): Observable<SimpleValue[]> {
    return this.http.get<SimpleValue[]>(`${this.baseUrl}/contacts`);
  }

  /**
   * @description Get all partners
   * @returns {Observable<SimpleValue[]>}
   * @since 1.0.0
   */
  getPartners(): Observable<SimpleValue[]> {
    return this.http.get<SimpleValue[]>(`${this.baseUrl}/partners`);
  }

  /**
   * @description Get entity roles for a specific entity type
   * @param {string} entityType - Entity type (e.g., "Opportunity")
   * @returns {Observable<SimpleValue[]>}
   * @since 1.0.0
   */
  getEntityRoles(entityType: string): Observable<SimpleValue[]> {
    return this.http.get<SimpleValue[]>(`${this.baseUrl}/entity-roles/${entityType}`);
  }

  /**
   * @description Get all internal users (UNOPS users)
   * @returns {Observable<SimpleValue[]>}
   * @since 1.0.0
   */
  getInternalUsers(): Observable<SimpleValue[]> {
    return this.http.get<SimpleValue[]>(`${this.baseUrl}/internal-users`);
  }

  /**
   * @description Performs dynamic search across country names and artifact values
   * @param {CountryDynamicSearchRequest} request - Search request parameters
   * @returns {Observable<CountryDynamicSearchResponse>} Observable of grouped search results
   * @example
   * ```typescript
   * const request: CountryDynamicSearchRequest = {
   *   searchTerm: 'development',
   *   includeArtifacts: true,
   *   maxResults: 50,
   *   highlightMatches: true
   * };
   * 
   * this.valuesService.dynamicSearchCountries(request).subscribe(results => {
   *   console.log('Found', results.totalMatches, 'countries');
   *   console.log('Name matches:', results.groups.nameMatches);
   *   console.log('Artifact matches:', results.groups.artifactMatches);
   * });
   * ```
   * @since 1.0.0
   */
  dynamicSearchCountries(request: CountryDynamicSearchRequest): Observable<CountryDynamicSearchResponse> {
    return this.http.post<CountryDynamicSearchResponse>(
      '/api/country/dynamic-search',
      request
    );
  }

  /**
   * @description Get suggested organization units based on countries of implementation
   * @param {number[]} countryIds - Array of country IDs
   * @returns {Observable<SuggestedOrgUnitsResponse>}
   * @since 1.0.0
   */
  getSuggestedOrgUnits(countryIds: number[]): Observable<SuggestedOrgUnitsResponse> {
    const params = countryIds.map(id => `countryIds=${id}`).join('&');
    return this.http.get<SuggestedOrgUnitsResponse>(`${this.baseUrl}/suggested-org-units?${params}`);
  }

  /**
   * @description Get entity user roles for multiple organization hierarchies.
   * Used to auto-populate internal stakeholders when selecting OrgUnits.
   * @param {number[]} organizationHierarchyIds - Array of organization hierarchy IDs
   * @returns {Observable<EntityUserRolesByOrgUnitResponse[]>}
   * @since 1.0.0
   */
  getEntityUserRolesByOrgUnits(organizationHierarchyIds: number[]): Observable<EntityUserRolesByOrgUnitResponse[]> {
    return this.http.post<EntityUserRolesByOrgUnitResponse[]>(
      `${this.baseUrl}/entity-user-roles-by-org-unit`,
      organizationHierarchyIds
    );
  }

  /**
   * @description Get org unit IDs for countries including their parent and grandparent org units.
   * Used when a GPO is selected to auto-populate stakeholders from country-responsible org units.
   * @param {number[]} countryIds - Array of country IDs
   * @returns {Observable<number[]>}
   * @since 1.0.0
   */
  getOrgUnitIdsForCountries(countryIds: number[]): Observable<number[]> {
    return this.http.post<number[]>(`${this.baseUrl}/org-unit-ids-for-countries`, countryIds);
  }

  /**
   * @description Get child org unit IDs under a Hub/Region that relate to the given country IDs.
   * Used when a Hub or Region is selected to auto-populate stakeholders from child org units.
   * @param {number} parentOrgUnitId - The Hub/Region org unit ID
   * @param {number[]} countryIds - Array of country IDs
   * @returns {Observable<number[]>}
   * @since 1.0.0
   */
  getChildOrgUnitIdsForHubRegion(parentOrgUnitId: number, countryIds: number[]): Observable<number[]> {
    return this.http.post<number[]>(
      `${this.baseUrl}/child-org-unit-ids-for-hub-region/${parentOrgUnitId}`,
      countryIds
    );
  }
}


