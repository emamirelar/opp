/**
 * @fileoverview Reusable requirements validation component for workflow stage transitions
 * @author UNOPS Opportunity+ Development Team
 */

import {
  Component,
  input,
  output,
  signal,
  computed,
  inject,
  OnInit,
  OnDestroy,
  OnChanges,
  SimpleChanges,
  ChangeDetectionStrategy,
  InjectionToken,
  effect,
} from '@angular/core';
import { FormGroup, AbstractControl } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Subject, takeUntil, Subscription } from 'rxjs';
import { WorkflowService } from '../../services/workflow.service';
import { StageRequirement, isBuiltInFieldType, getSectionForField } from '../../models/requirement.models';
import { WorkflowStageModel } from '../../models/workflow.models';

/**
 * Event payload emitted when a requirement is clicked
 */
export interface RequirementClickEvent {
  /** The clicked requirement */
  requirement: StageRequirement;
  /** The section ID to navigate to (if determined) */
  section?: string;
  /** The field name associated with the requirement */
  fieldName?: string;
}

/**
 * Custom field validator service interface.
 * Implement this in consuming app for custom field type validation.
 */
export interface ICustomFieldValidatorService {
  /**
   * Validate a requirement
   * @param requirement The requirement to validate
   * @param formGroup The form group containing the field
   * @param context Additional context (entity data, nested forms, etc.)
   * @returns Promise resolving to true if valid
   */
  validate(requirement: StageRequirement, formGroup: FormGroup, context?: unknown): Promise<boolean>;
}

/**
 * Injection token for custom validators map
 */
export const CUSTOM_FIELD_VALIDATORS = new InjectionToken<Map<string, ICustomFieldValidatorService>>(
  'CUSTOM_FIELD_VALIDATORS'
);

/**
 * @class RequirementsValidationComponent
 * @description Reusable component for displaying and validating workflow stage requirements.
 * Follows the GMS pattern: blue collapsible info panel showing only unmet requirements.
 * Supports both built-in field type validation and custom validators for complex business logic.
 *
 * @example
 * ```html
 * <app-requirements-validation
 *   [entityName]="'opportunity'"
 *   [entityId]="opportunityId"
 *   [formGroup]="form"
 *   [currentStage]="opportunity.stage"
 *   (validationChanged)="onValidationChanged($event)"
 *   (requirementsLoaded)="onRequirementsLoaded($event)"
 * />
 * ```
 * @since 1.0.0
 */
@Component({
  selector: 'app-requirements-validation',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  templateUrl: './requirements-validation.component.html',
  styleUrls: ['./requirements-validation.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RequirementsValidationComponent implements OnInit, OnDestroy, OnChanges {
  // Required inputs
  readonly entityName = input.required<string>();
  readonly entityId = input.required<string>();
  readonly formGroup = input.required<FormGroup>();

  // Optional inputs
  readonly currentStage = input<string>('');
  readonly nestedForms = input<Record<string, FormGroup>>({});
  readonly entityData = input<unknown>();
  readonly customValidators = input<Map<string, ICustomFieldValidatorService>>(new Map());
  readonly showTitle = input<boolean>(true);
  readonly title = input<string>('Requirements');

  // Outputs
  readonly requirementsLoaded = output<StageRequirement[]>();
  readonly validationChanged = output<boolean>();
  /** Emitted when a requirement item is clicked (for navigation) */
  readonly requirementClick = output<RequirementClickEvent>();

  // Services
  private readonly workflowService = inject(WorkflowService);
  private readonly translateService = inject(TranslateService);

  // State
  readonly requirements = signal<StageRequirement[]>([]);
  readonly isLoading = signal(false);
  readonly nextStage = signal<string>('');
  readonly nextStageDisplayName = signal<string>('');
  readonly error = signal<string | null>(null);
  readonly isCollapsed = signal<boolean>(true);

  private destroy$ = new Subject<void>();
  private nestedFormSubscriptions: Subscription[] = [];
  private lastDocumentComponentRef: unknown = null;

  // Effect for document changes - must be in injection context (field initializer)
  private readonly documentChangesEffect = effect(() => {
    const entityData = this.entityData() as Record<string, unknown>;
    const documentComponent = entityData?.['documentComponent'];

    // Only trigger if document component reference actually changed and requirements are loaded
    if (documentComponent !== this.lastDocumentComponentRef && documentComponent && this.requirements().length > 0) {
      this.lastDocumentComponentRef = documentComponent;
      // Small delay to ensure document component is fully initialized
      setTimeout(() => {
        this.validateAllRequirements();
      }, 300);
    }
  });

  /**
   * Public method to check if all requirements are met (for compatibility with ViewChild access)
   * @returns {boolean} True if all requirements are met, false otherwise
   */
  allRequirementsMet(): boolean {
    return this.requirements().every((requirement) => {
      // Server-side only requirements should be considered as met (they need server validation)
      if (requirement.onlyServerSideEvaluation) return true;

      // If isMet is explicitly set, use that value
      if (requirement.isMet !== undefined) {
        return requirement.isMet;
      }

      // If isMet is undefined, consider it as not met (validation hasn't run yet or requirement is invalid)
      return false;
    });
  }

  readonly metCount = computed(() => {
    return this.requirements().filter((req) => req.isMet === true).length;
  });

  readonly totalCount = computed(() => {
    return this.requirements().length;
  });

  // Filter to show only failed requirements
  readonly failedRequirements = computed(() => {
    return this.requirements().filter((req) => req.isMet === false);
  });

  /**
   * Converts kebab-case to camelCase
   */
  private toCamelCase(input: string): string {
    return input.replace(/-([a-z])/g, (g) => g[1].toUpperCase());
  }

  /**
   * Gets the validation message for the header
   */
  getValidationMessage(): string {
    const entityKey = this.toCamelCase(this.entityName());
    let entityDisplayName = this.translateService.instant(`title.${entityKey}`);

    // Handle cases where the translation key returns an object instead of a string
    if (typeof entityDisplayName === 'object' && entityDisplayName !== null) {
      entityDisplayName = (entityDisplayName as Record<string, string>)['title'] || entityDisplayName;
    }

    return this.translateService.instant('message.requirements.title', {
      entity: entityDisplayName,
      nextStage: this.nextStageDisplayName(),
    });
  }

  ngOnInit(): void {
    // Load next stage first, then load requirements (which uses currentStage, not nextStage)
    this.loadNextStage();
    this.loadRequirements();
    this.subscribeToFormValueChanges();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['entityId'] || changes['entityName'] || changes['currentStage']) {
      if (!changes['entityId']?.firstChange) {
        this.loadNextStage();
        this.loadRequirements();
      }
    }

    // Re-subscribe when formGroup changes
    if (changes['formGroup']) {
      this.subscribeToFormValueChanges();
    }

    // Re-subscribe when nestedForms change
    if (changes['nestedForms']) {
      this.subscribeToNestedForms();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.nestedFormSubscriptions.forEach((sub) => sub.unsubscribe());
    this.nestedFormSubscriptions = [];
  }

  /**
   * Loads requirements from the server
   */
  loadRequirements(): void {
    const entityName = this.entityName();
    const entityId = this.entityId();
    const currentStage = this.currentStage();

    if (!entityName || !entityId || !currentStage) {
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);

    this.workflowService
      .getRequirementsForStageChange(entityName, entityId, currentStage)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (requirements) => {
          // Normalize documentTypes - ensure it's accessible as a direct property
          const normalizedRequirements = requirements.map((req: StageRequirement) => {
            const customConfig = req.customValidatorConfig as Record<string, unknown> | undefined;
            if (!req.documentTypes && customConfig?.['documentTypes']) {
              req.documentTypes = customConfig['documentTypes'] as Array<{
                name: string;
                code: string;
                description?: string;
              }>;
            }
            return req;
          });
          this.requirements.set(normalizedRequirements);
          this.requirementsLoaded.emit(normalizedRequirements);
          // Validate immediately after loading requirements
          this.validateAllRequirements();
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Failed to load requirements:', err);
          this.error.set('Failed to load requirements');
          this.isLoading.set(false);
        },
      });
  }

  /**
   * Loads the next stage by calculating it from workflow stages
   */
  private loadNextStage(): void {
    const entityName = this.entityName();
    const currentStage = this.currentStage();

    if (!entityName || !currentStage) {
      return;
    }

    this.workflowService
      .getWorkflowStages(entityName)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stages: WorkflowStageModel[]) => {
          const currentIndex = stages.findIndex((s) => s.stage === currentStage);
          if (currentIndex >= 0 && currentIndex < stages.length - 1) {
            const nextStageData = stages[currentIndex + 1];
            this.nextStage.set(nextStageData.stage);
            this.nextStageDisplayName.set(nextStageData.displayName);
          }
        },
        error: (err) => {
          console.error('Failed to load workflow stages:', err);
        },
      });
  }

  /**
   * Subscribes to form value changes for automatic re-validation
   */
  private subscribeToFormValueChanges(): void {
    const form = this.formGroup();
    if (form) {
      form.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
        this.validateAllRequirements();
      });
    }

    // Subscribe to nested forms
    this.subscribeToNestedForms();
  }

  /**
   * Subscribes to nested forms value changes
   */
  private subscribeToNestedForms(): void {
    // Clean up existing subscriptions
    this.nestedFormSubscriptions.forEach((sub) => sub.unsubscribe());
    this.nestedFormSubscriptions = [];

    // Subscribe to each nested form's value changes
    const nested = this.nestedForms();
    Object.entries(nested).forEach(([, nestedForm]) => {
      if (nestedForm) {
        const subscription = nestedForm.valueChanges.pipe(takeUntil(this.destroy$)).subscribe(() => {
          if (nestedForm.valid || Object.keys(nestedForm.controls).length > 0) {
            this.validateAllRequirements();
          }
        });
        this.nestedFormSubscriptions.push(subscription);
      }
    });
  }

  /**
   * Validates all requirements and updates their isMet status
   */
  async validateAllRequirements(): Promise<void> {
    const reqs = this.requirements();
    const form = this.formGroup();
    const nested = this.nestedForms();
    const entity = this.entityData();
    const validators = this.customValidators();
    const entityId = this.entityId();
    const entityName = this.entityName();

    const updatedReqs = await Promise.all(
      reqs.map(async (req) => {
        // Skip server-side only requirements
        if (req.onlyServerSideEvaluation) {
          return { ...req, isMet: undefined };
        }

        const isMet = await this.validateRequirement(req, form, nested, entity, validators, entityId, entityName);
        return { ...req, isMet };
      })
    );

    this.requirements.set(updatedReqs);
    this.validationChanged.emit(this.allRequirementsMet());
  }

  /**
   * Validates a single requirement
   */
  private async validateRequirement(
    requirement: StageRequirement,
    mainForm: FormGroup,
    nestedForms: Record<string, FormGroup>,
    entityData: unknown,
    validators: Map<string, ICustomFieldValidatorService>,
    entityId?: string,
    entityName?: string
  ): Promise<boolean> {
    const fieldType = requirement.fieldType?.toLowerCase();

    // Check for custom validator first
    if (fieldType) {
      let validator = validators.get(fieldType);

      // If not found with exact match, try converting to camelCase
      if (!validator && fieldType.includes('validation')) {
        const camelCaseVariants = [
          fieldType.charAt(0).toUpperCase() + fieldType.slice(1),
          fieldType.replace(/([a-z])([a-z]*)/, (match, p1: string, p2: string) => p1 + p2.charAt(0).toUpperCase() + p2.slice(1)),
        ];

        for (const variant of camelCaseVariants) {
          if (validators.has(variant)) {
            validator = validators.get(variant);
            break;
          }
        }
      }

      if (validator) {
        try {
          return await validator.validate(requirement, mainForm, {
            nestedForms,
            entityData,
            entityId,
            entityName,
          });
        } catch (error) {
          console.error(`Custom validator error for ${fieldType}:`, error);
          return false;
        }
      }
    }

    // Use built-in validation for standard field types
    if (isBuiltInFieldType(fieldType)) {
      return this.validateBuiltInFieldType(requirement, mainForm, nestedForms);
    }

    // Unknown field type without custom validator - skip validation
    return true;
  }

  /**
   * Validates a built-in field type
   */
  private validateBuiltInFieldType(
    requirement: StageRequirement,
    mainForm: FormGroup,
    nestedForms: Record<string, FormGroup>
  ): boolean {
    const fieldName = requirement.fieldName;
    if (!fieldName) {
      return true;
    }

    if (!mainForm) {
      return true;
    }

    // Get the form to use (main form or nested)
    const formName = requirement.form;
    const form = formName ? nestedForms[formName] || mainForm : mainForm;

    if (!form) {
      return true;
    }

    // Get the field control
    const control = this.getFormControl(form, fieldName);
    if (!control) {
      if (requirement.validation?.required) {
        return false;
      }
      return true;
    }

    const value = control.value;
    const validation = requirement.validation;
    const fieldType = requirement.fieldType?.toLowerCase();

    // Check conditional validation
    if (validation?.conditional) {
      const conditionMet = this.checkCondition(form, nestedForms, validation.conditional);
      if (!conditionMet) {
        return true;
      }
    }

    // Handle required validation based on field type
    if (validation?.required) {
      if (fieldType === 'boolean') {
        if (value !== true) {
          return false;
        }
      } else {
        if (!this.hasValue(value, fieldName)) {
          return false;
        }
      }
    }

    if (!this.hasValue(value, fieldName)) {
      return true;
    }

    // Type-specific validation rules
    switch (fieldType) {
      case 'string':
      case 'text':
        return this.validateString(value, validation);
      case 'number':
      case 'decimal':
        return this.validateNumber(value, validation);
      case 'boolean':
        return this.validateBoolean(value, validation);
      case 'date':
        return this.validateDate(value, validation);
      case 'array':
      case 'multiselect':
        return this.validateArray(value, validation);
      default:
        return true;
    }
  }

  /**
   * Gets a form control by field name (supports nested paths)
   */
  private getFormControl(form: FormGroup, fieldName: string): AbstractControl | null {
    const parts = fieldName.split('.');
    let control: AbstractControl | null = form;
    let currentForm: FormGroup = form;

    for (let i = 0; i < parts.length; i++) {
      const part = parts[i];
      if (!control) {
        return null;
      }
      if (i < parts.length - 1) {
        currentForm = control as FormGroup;
        control = currentForm.get(part);
        if (!control) {
          return null;
        }
      } else {
        control = currentForm.get(part);
      }
    }

    return control;
  }

  /**
   * Checks if a conditional validation condition is met
   */
  private checkCondition(
    mainForm: FormGroup,
    nestedForms: Record<string, FormGroup>,
    conditional: { field: string; value: unknown }
  ): boolean {
    let control = mainForm.get(conditional.field);

    if (!control) {
      for (const nestedForm of Object.values(nestedForms)) {
        control = nestedForm?.get(conditional.field);
        if (control) break;
      }
    }

    if (!control) return false;

    const actualValue = control.value;
    const expectedValue = conditional.value;

    if (typeof expectedValue === 'boolean') {
      return actualValue === expectedValue;
    }

    return String(actualValue).toLowerCase() === String(expectedValue).toLowerCase();
  }

  /**
   * Checks if a value is considered to have a value
   */
  private hasValue(value: unknown, fieldName?: string): boolean {
    if (value === null || value === undefined) {
      return false;
    }
    if (typeof value === 'string') {
      return value.trim().length > 0;
    }
    if (Array.isArray(value)) {
      return value.length > 0;
    }
    if (fieldName === 'currency' && typeof value === 'object' && value !== null) {
      const currencyObj = value as Record<string, unknown>;
      return !!(currencyObj['code'] || currencyObj['id']);
    }
    return true;
  }

  /**
   * Validates a string value
   */
  private validateString(
    value: unknown,
    validation?: { minLength?: number; maxLength?: number; pattern?: string; required?: boolean }
  ): boolean {
    if (!validation) {
      return true;
    }

    const hasOnlyRequired = Object.keys(validation).length === 1 && validation.required !== undefined;
    const hasOtherRules = validation.minLength != null || validation.maxLength != null || !!validation.pattern;

    if (hasOnlyRequired && !hasOtherRules) {
      return true;
    }

    const strValue = String(value || '');

    if (validation.minLength != null && typeof validation.minLength === 'number' && !isNaN(validation.minLength)) {
      if (strValue.length < validation.minLength) {
        return false;
      }
    }

    if (validation.maxLength != null && typeof validation.maxLength === 'number' && !isNaN(validation.maxLength)) {
      if (strValue.length > validation.maxLength) {
        return false;
      }
    }

    if (validation.pattern && validation.pattern.trim().length > 0) {
      try {
        const regex = new RegExp(validation.pattern);
        if (!regex.test(strValue)) {
          return false;
        }
      } catch {
        // Invalid regex, skip validation
      }
    }

    return true;
  }

  /**
   * Validates a number value
   * Note: Use != null to check for both null and undefined, since JSON may serialize
   * missing values as null rather than undefined
   */
  private validateNumber(
    value: unknown,
    validation?: { greaterThan?: number; lessThan?: number; min?: number; max?: number; equalTo?: number }
  ): boolean {
    if (!validation) return true;
    const numValue = Number(value);
    if (isNaN(numValue)) return false;

    // Use != null to check for both null and undefined
    if (validation.greaterThan != null && numValue <= validation.greaterThan) return false;
    if (validation.lessThan != null && numValue >= validation.lessThan) return false;
    if (validation.min != null && numValue < validation.min) return false;
    if (validation.max != null && numValue > validation.max) return false;
    if (validation.equalTo != null && numValue !== validation.equalTo) return false;

    return true;
  }

  /**
   * Validates a boolean value
   */
  private validateBoolean(value: unknown, validation?: { required?: boolean; value?: unknown }): boolean {
    if (!validation) {
      return true;
    }

    const hasOnlyRequired = Object.keys(validation).length === 1 && validation.required !== undefined;
    const hasValueRule = validation.value !== undefined && validation.value !== null;

    if (hasOnlyRequired && !hasValueRule) {
      return true;
    }

    if (hasValueRule) {
      return value === validation.value;
    }

    return true;
  }

  /**
   * Validates a date value
   */
  private validateDate(value: unknown, validation?: { isPast?: boolean }): boolean {
    if (!validation) return true;

    const dateValue = value instanceof Date ? value : new Date(String(value));
    if (isNaN(dateValue.getTime())) return false;

    if (validation.isPast === true) {
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      if (dateValue >= today) return false;
    }

    return true;
  }

  /**
   * Validates an array value
   */
  private validateArray(
    value: unknown,
    validation?: { minLength?: number; maxLength?: number; required?: boolean }
  ): boolean {
    if (!validation) {
      return true;
    }

    const hasOnlyRequired = Object.keys(validation).length === 1 && validation.required !== undefined;
    const hasOtherRules = validation.minLength != null || validation.maxLength != null;

    if (hasOnlyRequired && !hasOtherRules) {
      return true;
    }

    const arr = Array.isArray(value) ? value : [];

    if (validation.minLength != null && typeof validation.minLength === 'number' && !isNaN(validation.minLength)) {
      if (arr.length < validation.minLength) {
        return false;
      }
    }

    if (validation.maxLength != null && typeof validation.maxLength === 'number' && !isNaN(validation.maxLength)) {
      if (arr.length > validation.maxLength) {
        return false;
      }
    }

    return true;
  }

  /**
   * Revalidates all requirements (async to ensure validation completes)
   * @returns {Promise<void>} Promise that resolves when validation is complete
   */
  async revalidate(): Promise<void> {
    await this.validateAllRequirements();
  }

  /**
   * Refresh requirements from server
   */
  refresh(): void {
    this.loadNextStage();
    this.loadRequirements();
  }

  /**
   * Toggle collapsed state
   */
  toggleCollapsed(): void {
    this.isCollapsed.set(!this.isCollapsed());
  }

  /**
   * Handles click on a requirement item.
   * Emits an event with the requirement and its associated section for navigation.
   * Collapses the panel after navigation to reduce visual clutter.
   * @param requirement - The clicked requirement
   */
  onRequirementClick(requirement: StageRequirement): void {
    // Determine the section to navigate to
    const section = requirement.section || getSectionForField(requirement.fieldName, this.entityName());

    // Only emit and collapse if we have a navigable section
    if (section || requirement.fieldName) {
      this.requirementClick.emit({
        requirement,
        section,
        fieldName: requirement.fieldName,
      });

      // Collapse the panel after clicking to navigate
      // Small delay to ensure the click event is fully processed
      setTimeout(() => {
        this.isCollapsed.set(true);
      }, 100);
    }
  }

  /**
   * Checks if a requirement is clickable (has a navigable section)
   * @param requirement - The requirement to check
   * @returns True if the requirement can be navigated to
   */
  isRequirementClickable(requirement: StageRequirement): boolean {
    return !!(requirement.section || getSectionForField(requirement.fieldName, this.entityName()));
  }
}
