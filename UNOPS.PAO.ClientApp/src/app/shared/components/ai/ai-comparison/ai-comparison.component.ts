/**
 * @fileoverview Reusable AI Comparison Component for any entity type
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, inject, input, output, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { BadgeModule } from 'primeng/badge';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

/**
 * @interface FieldMapping
 * @description Custom field display configuration
 */
export interface FieldMapping {
  fieldPath: string;
  displayName: string;
  formatFn?: (value: any) => string;
}

/**
 * @interface DiffItem
 * @description Represents a single difference between current and AI data
 */
export interface DiffItem {
  field: string;
  label: string;
  currentValue: any;
  aiValue: any;
  type: string;
  isDifferent: boolean;
}

/**
 * @class AiComparisonComponent
 * @description Reusable component that displays a side-by-side comparison of current entity data
 * and AI-extracted data, highlighting differences and allowing selective application of changes.
 * Built with Angular 19 signals and Tailwind-first approach.
 * 
 * @example
 * ```html
 * <app-ai-comparison
 *   entityType="Opportunity"
 *   [entityId]="2"
 *   [aiExtractedData]="aiData()"
 *   [fieldMappings]="customFieldMappings"
 *   [visible]="showDialog()"
 *   (visibleChange)="showDialog.set($event)"
 *   (applyChanges)="handleApplyChanges($event)">
 * </app-ai-comparison>
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-ai-comparison',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    DialogModule,
    CheckboxModule,
    BadgeModule,
    FormsModule,
    TranslateModule
  ],
  templateUrl: './ai-comparison.component.html',
  styleUrls: ['./ai-comparison.component.scss']
})
export class AiComparisonComponent {
  private readonly http = inject(HttpClient);
  private readonly translateService = inject(TranslateService);

  /**
   * @description Type of entity being compared (e.g., 'Opportunity', 'Partner', 'Contact')
   * @type {Signal<string>}
   */
  readonly entityType = input.required<string>();

  /**
   * @description ID of the entity being compared
   * @type {Signal<number>}
   */
  readonly entityId = input.required<number>();

  /**
   * @description AI-extracted data
   * @type {Signal<any>}
   */
  readonly aiExtractedData = input.required<any>();

  /**
   * @description Optional custom field mappings for display names and formatting
   * @type {Signal<FieldMapping[] | undefined>}
   */
  readonly fieldMappings = input<FieldMapping[]>();

  /**
   * @description Whether the comparison dialog is visible
   * @type {Signal<boolean>}
   * @default false
   */
  readonly visible = input<boolean>(false);

  /**
   * @description Event emitted when dialog visibility changes
   * @type {OutputEmitterRef<boolean>}
   */
  readonly visibleChange = output<boolean>();

  /**
   * @description Event emitted when user applies selected changes
   * @type {OutputEmitterRef<any>}
   * @param {any} changes - Object containing selected fields and their new values
   */
  readonly applyChanges = output<any>();

  /**
   * @description Current entity data fetched from audit log
   * @type {WritableSignal<any>}
   */
  readonly currentData = signal<any>(null);

  /**
   * @description Loading state for audit log fetch
   * @type {WritableSignal<boolean>}
   */
  readonly loading = signal<boolean>(false);

  /**
   * @description Error message if audit log fetch fails
   * @type {WritableSignal<string | null>}
   */
  readonly error = signal<string | null>(null);

  /**
   * @description Map of field paths to their selection state
   * @type {WritableSignal<Map<string, boolean>>}
   */
  readonly selectedFields = signal<Map<string, boolean>>(new Map());

  /**
   * @description Whether currently applying changes
   * @type {WritableSignal<boolean>}
   */
  readonly applying = signal<boolean>(false);

  /**
   * @description Computed list of differences between current and AI data
   * @type {Signal<DiffItem[]>}
   */
  readonly differences = computed(() => {
    const current = this.currentData();
    const aiData = this.aiExtractedData();
    
    if (!current || !aiData) {
      return [];
    }

    return this.calculateDifferences(current, aiData);
  });

  /**
   * @description Computed count of selected fields
   * @type {Signal<number>}
   */
  readonly selectedCount = computed(() => {
    return Array.from(this.selectedFields().values()).filter(v => v).length;
  });

  /**
   * @description Total count of differences
   * @type {Signal<number>}
   */
  readonly totalCount = computed(() => this.differences().length);

  /**
   * @description Whether all differences are selected
   * @type {Signal<boolean>}
   */
  readonly allSelected = computed(() => {
    const total = this.totalCount();
    const selected = this.selectedCount();
    return total > 0 && total === selected;
  });

  constructor() {
    // Effect to fetch audit log when entity changes or dialog opens
    effect(() => {
      const type = this.entityType();
      const id = this.entityId();
      const isVisible = this.visible();
      
      if (isVisible && type && id) {
        this.fetchAuditLog(type, id);
      } else if (!isVisible) {
        // Reset state when dialog closes
        this.currentData.set(null);
        this.error.set(null);
        this.selectedFields.set(new Map());
        this.applying.set(false); // Reset applying state when dialog closes
      }
    });

    // Effect to auto-select all differences when they are loaded
    effect(() => {
      const diffs = this.differences();
      const isVisible = this.visible();
      const currentSelectionSize = this.selectedFields().size;
      
      // Auto-select all differences when dialog opens and no selections exist
      if (isVisible && diffs.length > 0 && currentSelectionSize === 0) {
        const selectedMap = new Map<string, boolean>();
        diffs.forEach(diff => selectedMap.set(diff.field, true));
        this.selectedFields.set(selectedMap);
      }
    }, { allowSignalWrites: true });
  }

  /**
   * @description Fetch latest audit log for the entity
   * @param {string} entityType - Type of entity
   * @param {number} entityId - ID of entity
   * @returns {void}
   */
  private fetchAuditLog(entityType: string, entityId: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.http.get<any>(`/api/auditlog/latest`, {
      params: {
        entityType: entityType,
        entityId: entityId.toString()
      }
    }).subscribe({
      next: (auditLog) => {
        console.log(`📋 Latest Audit Log for ${entityType} ${entityId}:`, auditLog);
        
        if (auditLog && auditLog.jsonData) {
          try {
            const data = JSON.parse(auditLog.jsonData);
            console.log(`📋 Current ${entityType} Data (from Audit Log):`, data);
            this.currentData.set(data);
          } catch (e) {
            console.error('Failed to parse audit log JSON:', e);
            this.error.set('Failed to parse audit log data');
          }
        } else {
          this.error.set('No audit log data found');
        }
        
        this.loading.set(false);
      },
      error: (error) => {
        console.error(`Error fetching audit log for ${entityType} ${entityId}:`, error);
        this.error.set('Failed to fetch audit log');
        this.loading.set(false);
      }
    });
  }

  /**
   * @description Calculate differences between current and AI-extracted data
   * @param {any} current - Current entity data
   * @param {any} aiData - AI-extracted entity data
   * @param {string} path - Current path in object hierarchy
   * @returns {DiffItem[]} Array of difference items
   */
  private calculateDifferences(current: any, aiData: any, path: string = ''): DiffItem[] {
    const differences: DiffItem[] = [];

    // Skip internal fields
    const skipFields = ['id', 'createdBy', 'createdDate', 'lastModifiedBy', 'lastModifiedDate', 
                       '_confidence', 'dependents', 'stats', 'permissions', 'createdByName', 'lastModifiedByName'];

    for (const key in aiData) {
      if (skipFields.includes(key)) {
        continue;
      }

      const currentValue = current[key];
      const aiValue = aiData[key];
      const fieldPath = path ? `${path}.${key}` : key;

      // Handle null/undefined values
      if (aiValue === null || aiValue === undefined) {
        continue;
      }

      // Handle arrays
      if (Array.isArray(aiValue)) {
        const currentArray = Array.isArray(currentValue) ? currentValue : [];
        if (JSON.stringify(currentArray) !== JSON.stringify(aiValue)) {
          differences.push({
            field: fieldPath,
            label: this.getFieldLabel(fieldPath),
            currentValue: currentArray,
            aiValue: aiValue,
            type: 'array',
            isDifferent: true
          });
        }
      }
      // Handle objects (but not dates)
      else if (typeof aiValue === 'object' && aiValue !== null && !(aiValue instanceof Date)) {
        // For nested objects, we can recurse or treat as a single field
        // For simplicity, treating as single field
        if (JSON.stringify(currentValue) !== JSON.stringify(aiValue)) {
          differences.push({
            field: fieldPath,
            label: this.getFieldLabel(fieldPath),
            currentValue: currentValue,
            aiValue: aiValue,
            type: 'object',
            isDifferent: true
          });
        }
      }
      // Handle primitive values
      else {
        if (currentValue !== aiValue) {
          differences.push({
            field: fieldPath,
            label: this.getFieldLabel(fieldPath),
            currentValue: currentValue,
            aiValue: aiValue,
            type: typeof aiValue,
            isDifferent: true
          });
        }
      }
    }

    return differences;
  }

  /**
   * @description Get display label for a field
   * @param {string} fieldPath - Field path
   * @returns {string} Display label
   */
  private getFieldLabel(fieldPath: string): string {
    // Check custom field mappings first
    const customMapping = this.fieldMappings()?.find(m => m.fieldPath === fieldPath);
    if (customMapping) {
      return customMapping.displayName;
    }

    // Try translation
    const fieldName = fieldPath.split('.').pop() || fieldPath;
    const translationKey = `label.${fieldName}`;
    const translated = this.translateService.instant(translationKey);
    
    if (translated !== translationKey) {
      return translated;
    }

    // Fallback to formatting the field name
    return fieldName
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase())
      .trim();
  }

  /**
   * @description Toggle selection of a field
   * @param {string} fieldPath - Path to the field
   * @returns {void}
   */
  toggleField(fieldPath: string): void {
    const current = this.selectedFields().get(fieldPath) || false;
    const updated = new Map(this.selectedFields());
    updated.set(fieldPath, !current);
    this.selectedFields.set(updated);
  }

  /**
   * @description Check if a field is selected
   * @param {string} fieldPath - Path to the field
   * @returns {boolean} Whether field is selected
   */
  isSelected(fieldPath: string): boolean {
    return this.selectedFields().get(fieldPath) || false;
  }

  /**
   * @description Select or deselect all fields
   * @returns {void}
   */
  toggleAll(): void {
    const selectAll = !this.allSelected();
    const updated = new Map(this.selectedFields());
    
    this.differences().forEach(diff => {
      updated.set(diff.field, selectAll);
    });
    
    this.selectedFields.set(updated);
  }

  /**
   * @description Apply selected changes
   * @returns {void}
   */
  handleApplyChanges(): void {
    const selectedChanges: any = {};
    
    this.differences().forEach(diff => {
      if (this.selectedFields().get(diff.field)) {
        // Build nested object structure
        this.setNestedValue(selectedChanges, diff.field, diff.aiValue);
      }
    });

    if (Object.keys(selectedChanges).length > 0) {
      this.applying.set(true);
      this.applyChanges.emit(selectedChanges);
    }
  }

  /**
   * @description Set nested value in object using dot-notation path
   * @param {any} obj - Target object
   * @param {string} path - Dot-notation path
   * @param {any} value - Value to set
   * @returns {void}
   */
  private setNestedValue(obj: any, path: string, value: any): void {
    const keys = path.split('.');
    let current = obj;

    for (let i = 0; i < keys.length - 1; i++) {
      const key = keys[i];
      if (!current[key]) {
        current[key] = {};
      }
      current = current[key];
    }

    current[keys[keys.length - 1]] = value;
  }

  /**
   * @description Close the dialog
   * @returns {void}
   */
  handleClose(): void {
    this.visibleChange.emit(false);
    this.selectedFields.set(new Map());
    this.applying.set(false);
    this.currentData.set(null);
    this.error.set(null);
  }

  /**
   * @description Format value for display
   * @param {any} value - Value to format
   * @param {string} fieldPath - Field path for custom formatting
   * @returns {string} Formatted value
   */
  formatValue(value: any, fieldPath?: string): string {
    // Check custom field mappings for formatting function
    if (fieldPath) {
      const customMapping = this.fieldMappings()?.find(m => m.fieldPath === fieldPath);
      if (customMapping?.formatFn) {
        return customMapping.formatFn(value);
      }
    }

    if (value === null || value === undefined) {
      return '-';
    }

    if (Array.isArray(value)) {
      if (value.length === 0) return '[]';
      // Format array items
      return value.map(item => {
        if (typeof item === 'object') {
          return item.name || item.partnerName || item.countryName || JSON.stringify(item);
        }
        return String(item);
      }).join(', ');
    }

    if (typeof value === 'object') {
      // Try to extract a meaningful display value
      if (value.name) return value.name;
      if (value.partnerName) return value.partnerName;
      if (value.countryName) return value.countryName;
      return JSON.stringify(value, null, 2);
    }

    if (typeof value === 'boolean') {
      return value ? 'Yes' : 'No';
    }

    return String(value);
  }

  /**
   * @description Check if field is a deliverable field
   * @param {string} field - Field path
   * @returns {boolean} Whether field is deliverables
   */
  isDeliverableField(field: string): boolean {
    return field === 'deliverables';
  }

  /**
   * @description Check if field is a funding partner field
   * @param {string} field - Field path
   * @returns {boolean} Whether field is funding partners
   */
  isFundingPartnerField(field: string): boolean {
    return field === 'fundingPartners';
  }

  /**
   * @description Check if field is a client partner field
   * @param {string} field - Field path
   * @returns {boolean} Whether field is client partners
   */
  isClientPartnerField(field: string): boolean {
    return field === 'clientPartners';
  }

  /**
   * @description Check if field is an SDG field
   * @param {string} field - Field path
   * @returns {boolean} Whether field is SDGs
   */
  isSDGField(field: string): boolean {
    return field === 'sdGs';
  }

  /**
   * @description Check if field is a country field
   * @param {string} field - Field path
   * @returns {boolean} Whether field is countries
   */
  isCountryField(field: string): boolean {
    return field === 'countries';
  }

  /**
   * @description Check if field is a stakeholder field
   * @param {string} field - Field path
   * @returns {boolean} Whether field is stakeholders
   */
  isStakeholderField(field: string): boolean {
    return field === 'stakeholders';
  }

  /**
   * @description Format currency amount
   * @param {number} amount - Amount to format
   * @returns {string} Formatted currency string
   */
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }

  /**
   * @description Get SDG logo URL
   * @param {string} sdgId - SDG ID
   * @returns {string} SDG logo URL
   */
  getSDGLogo(sdgId: string): string {
    const sdgNumber = parseInt(sdgId);
    return `https://sdgs.un.org/sites/default/files/goals/E_SDG_Icons-${String(sdgNumber).padStart(2, '0')}.jpg`;
  }

  /**
   * @description Get country flag emoji
   * @param {string} countryCode - ISO 2-letter country code
   * @returns {string} Flag emoji
   */
  getCountryFlag(countryCode: string): string {
    if (!countryCode || countryCode.length !== 2) return '🏳️';
    
    const codePoints = countryCode
      .toUpperCase()
      .split('')
      .map(char => 127397 + char.charCodeAt(0));
    return String.fromCodePoint(...codePoints);
  }

  /**
   * @description Get initials from a name
   * @param {string} name - Full name
   * @returns {string} Initials
   */
  getInitials(name: string): string {
    if (!name) return '??';
    const parts = name.split(' ').filter(p => p.length > 0);
    if (parts.length === 0) return '??';
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  /**
   * @description Cast value to any type for template access (type-safe workaround)
   * @param {any} value - Value to cast
   * @returns {any} Casted value
   */
  asAny(value: any): any {
    return value;
  }

  /**
   * @description Check if value is an array (template helper)
   * @param {any} value - Value to check
   * @returns {boolean} Whether value is an array
   */
  isArray(value: any): boolean {
    return Array.isArray(value);
  }
}

