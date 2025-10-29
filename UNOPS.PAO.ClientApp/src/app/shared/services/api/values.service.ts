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
}


