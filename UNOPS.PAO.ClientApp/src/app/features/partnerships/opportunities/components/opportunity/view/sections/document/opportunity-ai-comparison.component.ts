/**
 * @fileoverview Component for comparing current opportunity data with AI-extracted data
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, output, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

/**
 * @class OpportunityAiComparisonComponent
 * @description Component that displays a side-by-side comparison of current opportunity data
 * and AI-extracted data, highlighting differences and allowing selective application of changes.
 * Built with Angular 19 signals and Tailwind-first approach.
 * 
 * @example
 * ```html
 * <app-opportunity-ai-comparison
 *   [currentData]="currentOpportunityData()"
 *   [aiExtractedData]="aiExtractedData()"
 *   [visible]="showComparison()"
 *   (visibleChange)="showComparison.set($event)"
 *   (applyChanges)="handleApplyChanges($event)">
 * </app-opportunity-ai-comparison>
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-ai-comparison',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    DialogModule,
    CheckboxModule,
    FormsModule,
    TranslateModule
  ],
  templateUrl: './opportunity-ai-comparison.component.html',
  styleUrls: ['./opportunity-ai-comparison.component.scss']
})
export class OpportunityAiComparisonComponent {
  /**
   * @description Current saved opportunity data from audit log
   * @type {Signal<any>}
   * @default undefined
   */
  readonly currentData = input<any>();

  /**
   * @description AI-extracted opportunity data
   * @type {Signal<any>}
   * @default undefined
   */
  readonly aiExtractedData = input<any>();

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
   * @description Whether all differences are selected
   * @type {Signal<boolean>}
   */
  readonly allSelected = computed(() => {
    const total = this.differences().length;
    const selected = this.selectedCount();
    return total > 0 && total === selected;
  });

  constructor(private translateService: TranslateService) {}

  /**
   * @description Calculate differences between current and AI-extracted data
   * @param {any} current - Current opportunity data
   * @param {any} aiData - AI-extracted opportunity data
   * @returns {DiffItem[]} Array of difference items
   */
  private calculateDifferences(current: any, aiData: any, path: string = ''): DiffItem[] {
    const differences: DiffItem[] = [];

    // Skip internal fields
    const skipFields = ['id', 'createdBy', 'createdDate', 'lastModifiedBy', 'lastModifiedDate', '_confidence', 'dependents', 'stats'];

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
      if (Array.isArray(aiValue) && Array.isArray(currentValue)) {
        if (JSON.stringify(currentValue) !== JSON.stringify(aiValue)) {
          differences.push({
            field: fieldPath,
            label: this.formatFieldLabel(key),
            currentValue: currentValue,
            aiValue: aiValue,
            type: 'array',
            isDifferent: true
          });
        }
      }
      // Handle objects (but not dates)
      else if (typeof aiValue === 'object' && aiValue !== null && !(aiValue instanceof Date)) {
        // For nested objects, recurse
        const nestedDiffs = this.calculateDifferences(currentValue || {}, aiValue, fieldPath);
        differences.push(...nestedDiffs);
      }
      // Handle primitive values
      else {
        if (currentValue !== aiValue) {
          differences.push({
            field: fieldPath,
            label: this.formatFieldLabel(key),
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
   * @description Format field name to human-readable label
   * @param {string} fieldName - Field name in camelCase
   * @returns {string} Formatted label
   */
  private formatFieldLabel(fieldName: string): string {
    // Try to get translation first
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
  }

  /**
   * @description Format value for display
   * @param {any} value - Value to format
   * @returns {string} Formatted value
   */
  formatValue(value: any): string {
    if (value === null || value === undefined) {
      return '-';
    }

    if (Array.isArray(value)) {
      return value.length > 0 ? JSON.stringify(value, null, 2) : '[]';
    }

    if (typeof value === 'object') {
      return JSON.stringify(value, null, 2);
    }

    if (typeof value === 'boolean') {
      return value ? 'Yes' : 'No';
    }

    return String(value);
  }
}

/**
 * @interface DiffItem
 * @description Represents a single difference between current and AI data
 */
interface DiffItem {
  field: string;
  label: string;
  currentValue: any;
  aiValue: any;
  type: string;
  isDifferent: boolean;
}

