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
 * @interface Output
 * @description Output model for deliverables
 */
export interface Output {
  id: number;
  name?: string;
  outputGroup?: string;
  outputSubGroup?: string;
  outputName?: string;
  description?: string;
  unitId?: number;
  unitName?: string;
  projectCategoryId?: number;
  projectCategoryName?: string;
  outputServiceLine?: string;
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
   * @description Get all organization units
   * @returns {Observable<OrganizationUnit[]>}
   * @since 1.0.0
   */
  getOrganizationUnits(): Observable<OrganizationUnit[]> {
    return this.http.get<OrganizationUnit[]>(`${this.baseUrl}/organization-units`);
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
   * @returns {string[]} Array of distinct output group names
   * @since 1.0.0
   */
  getDistinctOutputGroups(outputs: Output[]): string[] {
    const groups = outputs
      .map(o => o.outputGroup)
      .filter((group, index, self) => group && self.indexOf(group) === index) as string[];
    return groups.sort();
  }

  /**
   * @description Get distinct output sub-groups for a specific output group, or all sub-groups if no group specified
   * @param {Output[]} outputs - Array of all outputs
   * @param {string} outputGroup - Selected output group (empty string returns all sub-groups)
   * @returns {string[]} Array of distinct output sub-group names
   * @since 1.0.0
   */
  getDistinctOutputSubGroups(outputs: Output[], outputGroup: string): string[] {
    const filtered = outputGroup ? outputs.filter(o => o.outputGroup === outputGroup) : outputs;
    const subGroups = filtered
      .map(o => o.outputSubGroup)
      .filter((subGroup, index, self) => subGroup && self.indexOf(subGroup) === index) as string[];
    return subGroups.sort();
  }

  /**
   * @description Get outputs filtered by group and/or sub-group (both optional)
   * @param {Output[]} outputs - Array of all outputs
   * @param {string} [outputGroup] - Optional selected output group
   * @param {string} [outputSubGroup] - Optional selected output sub-group
   * @returns {Output[]} Filtered array of outputs
   * @since 1.0.0
   */
  getFilteredOutputs(outputs: Output[], outputGroup?: string, outputSubGroup?: string): Output[] {
    return outputs.filter(o => {
      const matchesGroup = !outputGroup || o.outputGroup === outputGroup;
      const matchesSubGroup = !outputSubGroup || o.outputSubGroup === outputSubGroup;
      return matchesGroup && matchesSubGroup;
    }).sort((a, b) => (a.outputName || '').localeCompare(b.outputName || ''));
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
}


