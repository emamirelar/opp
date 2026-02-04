/**
 * @fileoverview Stage requirements validation models for Angular frontend
 * @author UNOPS Opportunity+ Development Team
 */

/**
 * Stage requirement model - represents a validation requirement for a workflow stage transition
 */
export interface StageRequirement {
  /**
   * Unique identifier for the requirement
   */
  name: string;

  /**
   * Translation key or description for the requirement message
   */
  description: string;

  /**
   * Field name to validate on the entity (for form-based validation)
   */
  fieldName?: string;

  /**
   * Type of field determining which validator to use.
   * Built-in types: string, text, boolean, number, decimal, date, array, multiselect, select
   * Custom types defined by consuming application
   */
  fieldType?: string;

  /**
   * Name of the form containing the field (for nested/tabbed forms)
   */
  form?: string;

  /**
   * Section ID for navigation (maps to UI section where this field is located)
   * Used by click-to-navigate functionality
   */
  section?: string;

  /**
   * Validation rules for the requirement
   */
  validation?: RequirementValidation;

  /**
   * Reference to a related entity for cross-entity validation
   */
  entityReference?: string;

  /**
   * Name of the step for process-based validation
   */
  stepName?: string;

  /**
   * If true, requirement is only evaluated server-side
   */
  onlyServerSideEvaluation?: boolean;

  /**
   * Additional configuration for custom validators
   */
  customValidatorConfig?: Record<string, unknown>;

  /**
   * Document types required (for documents field type)
   * This is a direct property from backend conversion for backward compatibility
   */
  documentTypes?: Array<{ name: string; code: string; description?: string }>;

  /**
   * Runtime state - indicates if the requirement is currently satisfied
   */
  isMet?: boolean;

  /**
   * Runtime state - error message if requirement is not met
   */
  errorMessage?: string;
}

/**
 * Validation rules for a stage requirement
 */
export interface RequirementValidation {
  /**
   * Whether the field is required (non-null, non-empty)
   */
  required?: boolean;

  /**
   * Minimum length for strings or minimum count for arrays
   */
  minLength?: number;

  /**
   * Maximum length for strings or maximum count for arrays
   */
  maxLength?: number;

  /**
   * Value must be greater than this number (exclusive)
   */
  greaterThan?: number;

  /**
   * Value must be less than this number (exclusive)
   */
  lessThan?: number;

  /**
   * Minimum numeric value (inclusive)
   */
  min?: number;

  /**
   * Maximum numeric value (inclusive)
   */
  max?: number;

  /**
   * Value must be equal to this number
   */
  equalTo?: number;

  /**
   * For date fields: if true, date must be in the past
   */
  isPast?: boolean;

  /**
   * Exact value that the field must have
   */
  value?: unknown;

  /**
   * Regular expression pattern for string validation
   */
  pattern?: string;

  /**
   * For multi-field validation: list of field names to check
   */
  fields?: string[];

  /**
   * For multi-field validation: "OR" (at least one) or "AND" (all required)
   */
  operator?: 'OR' | 'AND';

  /**
   * Conditional validation: requirement only applies when condition is met
   */
  conditional?: ConditionalValidation;

  /**
   * Custom validation message
   */
  message?: string;

  /**
   * Additional properties for custom validators
   */
  [key: string]: unknown;
}

/**
 * Conditional validation configuration
 */
export interface ConditionalValidation {
  /**
   * Field name to check for the condition
   */
  field: string;

  /**
   * Value that the field must have for the requirement to apply
   */
  value: unknown;

  /**
   * Whether the requirement is required when condition is met (default: true)
   */
  required?: boolean;
}

/**
 * Validation result from the server
 */
export interface ValidationResult {
  /**
   * True if all requirements passed validation
   */
  isValid: boolean;

  /**
   * Collection of validation errors
   */
  errors: ValidationError[];

  /**
   * Number of validation errors
   */
  errorCount: number;
}

/**
 * Single validation error
 */
export interface ValidationError {
  /**
   * The field name that failed validation
   */
  field: string;

  /**
   * The error message (may be a translation key)
   */
  message: string;

  /**
   * Optional error code for programmatic handling
   */
  code?: string;

  /**
   * Optional additional context data
   */
  context?: Record<string, unknown>;
}

/**
 * Built-in field types supported by the base validation manager
 */
export const FieldTypes = {
  String: 'string',
  Text: 'text',
  Boolean: 'boolean',
  Number: 'number',
  Decimal: 'decimal',
  Date: 'date',
  Array: 'array',
  MultiSelect: 'multiselect',
  Select: 'select',
} as const;

/**
 * Type helper for built-in field types
 */
export type BuiltInFieldType = (typeof FieldTypes)[keyof typeof FieldTypes];

/**
 * Checks if a field type is a built-in type
 */
export function isBuiltInFieldType(fieldType: string | undefined): boolean {
  if (!fieldType) return false;
  const builtInTypes = Object.values(FieldTypes);
  return builtInTypes.includes(fieldType.toLowerCase() as BuiltInFieldType);
}

/**
 * Field-to-section mapping for Opportunity entity.
 * Maps field names from requirements to their corresponding UI section IDs.
 * Used by click-to-navigate functionality in requirements validation component.
 */
export const OpportunityFieldSectionMapping: Record<string, string> = {
  // Overview section fields
  name: 'overview',
  description: 'overview',
  initiativeBudgetUSD: 'overview',

  // Why section fields
  challenges: 'why',
  expectedImpact: 'why',
  expectedOutcomes: 'why',
  unopsMissions: 'why',
  sdgs: 'why',
  beneficiaries: 'why',
  beneficiariesToBeDetermined: 'why',
  estimatedDirectBeneficiaries: 'why',
  estimatedIndirectBeneficiaries: 'why',

  // Who section fields
  fundingPartners: 'who',
  clientPartners: 'who',
  stakeholders: 'who',

  // What section fields
  deliverables: 'what',

  // Where section fields
  countries: 'where',

  // When section fields
  targetSigningDate: 'when',
  implementationStartDate: 'when',
  targetDeliveryDate: 'when',

  // Statement section fields
  opportunityStatementMarkdown: 'statement',

  // Team section fields
  responsibleOrgUnitId: 'team',
  proposedInitiativeTypeId: 'team',
  doaHolders: 'team',
};

/**
 * Gets the section ID for a given field name.
 * @param fieldName - The field name from a requirement
 * @param entityName - The entity type (defaults to 'opportunity')
 * @returns The section ID or undefined if no mapping exists
 */
export function getSectionForField(fieldName: string | undefined, entityName?: string): string | undefined {
  if (!fieldName) return undefined;

  // Currently only Opportunity entity has field-section mappings
  // Can be extended to support other entities in the future
  if (!entityName || entityName.toLowerCase() === 'opportunity') {
    return OpportunityFieldSectionMapping[fieldName];
  }

  return undefined;
}
