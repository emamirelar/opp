namespace UNOPS.PAO.Business.Workflow.StageRequirements;

/// <summary>
/// Defines field validation requirements for Opportunity stage transitions.
/// Used to ensure all required fields are populated before a stage change.
/// </summary>
public static class OpportunityStageRequirements
{
    /// <summary>
    /// Gets the required fields for transitioning to a specific stage.
    /// </summary>
    /// <param name="toStage">The target stage</param>
    /// <returns>List of field requirement definitions</returns>
    public static List<FieldRequirement> GetRequiredFieldsForStage(string toStage)
    {
        return toStage switch
        {
            OpportunityWorkflow.Stages.Go => GetGoStageRequirements(),
            OpportunityWorkflow.Stages.NoGo => GetNoGoStageRequirements(),
            OpportunityWorkflow.Stages.IdentifyAndProfile => GetIdentifyAndProfileRequirements(),
            _ => new List<FieldRequirement>()
        };
    }

    /// <summary>
    /// Gets the required fields for transitioning to GO stage.
    /// GO represents approval to proceed with the opportunity.
    /// </summary>
    private static List<FieldRequirement> GetGoStageRequirements()
    {
        return new List<FieldRequirement>
        {
            new FieldRequirement
            {
                FieldName = "Name",
                DisplayName = "Opportunity Name",
                IsRequired = true,
                ErrorMessage = "Opportunity name is required before moving to GO stage."
            },
            new FieldRequirement
            {
                FieldName = "Description",
                DisplayName = "Description",
                IsRequired = true,
                ErrorMessage = "Description is required before moving to GO stage."
            },
            new FieldRequirement
            {
                FieldName = "ResponsibleOrgUnitId",
                DisplayName = "Responsible Organization Unit",
                IsRequired = true,
                ErrorMessage = "Responsible organization unit must be assigned before moving to GO stage."
            },
            new FieldRequirement
            {
                FieldName = "InitiativeBudgetUSD",
                DisplayName = "Initiative Budget (USD)",
                IsRequired = false,
                ValidationRule = FieldValidationRule.PositiveNumber,
                ErrorMessage = "Initiative budget must be a positive number if provided."
            }
        };
    }

    /// <summary>
    /// Gets the required fields for transitioning to NO GO stage.
    /// NO GO represents decision not to proceed with the opportunity.
    /// </summary>
    private static List<FieldRequirement> GetNoGoStageRequirements()
    {
        // NO GO has fewer requirements as it's a rejection
        return new List<FieldRequirement>
        {
            new FieldRequirement
            {
                FieldName = "Name",
                DisplayName = "Opportunity Name",
                IsRequired = true,
                ErrorMessage = "Opportunity name is required."
            }
            // Comment is required by workflow transition config, not field validation
        };
    }

    /// <summary>
    /// Gets the required fields for transitioning back to IDENTIFY & PROFILE stage.
    /// This is the reopen scenario from NO GO.
    /// </summary>
    private static List<FieldRequirement> GetIdentifyAndProfileRequirements()
    {
        // Reopening has minimal requirements
        return new List<FieldRequirement>
        {
            new FieldRequirement
            {
                FieldName = "Name",
                DisplayName = "Opportunity Name",
                IsRequired = true,
                ErrorMessage = "Opportunity name is required."
            }
        };
    }

    /// <summary>
    /// Validates an opportunity entity against the stage requirements.
    /// </summary>
    /// <typeparam name="T">The opportunity entity type</typeparam>
    /// <param name="entity">The entity to validate</param>
    /// <param name="toStage">The target stage</param>
    /// <returns>Validation result with any errors</returns>
    public static StageValidationResult ValidateForStage<T>(T entity, string toStage) where T : class
    {
        var result = new StageValidationResult { IsValid = true };
        var requirements = GetRequiredFieldsForStage(toStage);

        foreach (var requirement in requirements)
        {
            var property = typeof(T).GetProperty(requirement.FieldName);
            if (property == null) continue;

            var value = property.GetValue(entity);
            var isValid = ValidateFieldValue(value, requirement);

            if (!isValid)
            {
                result.IsValid = false;
                result.Errors.Add(new StageValidationError
                {
                    FieldName = requirement.FieldName,
                    DisplayName = requirement.DisplayName,
                    ErrorMessage = requirement.ErrorMessage
                });
            }
        }

        return result;
    }

    private static bool ValidateFieldValue(object? value, FieldRequirement requirement)
    {
        // Check required fields
        if (requirement.IsRequired)
        {
            if (value == null) return false;
            if (value is string strValue && string.IsNullOrWhiteSpace(strValue)) return false;
            if (value is int intValue && intValue == 0) return false;
        }

        // Check validation rules
        if (value != null && requirement.ValidationRule != FieldValidationRule.None)
        {
            return requirement.ValidationRule switch
            {
                FieldValidationRule.PositiveNumber => IsPositiveNumber(value),
                FieldValidationRule.NotEmpty => !IsEmpty(value),
                _ => true
            };
        }

        return true;
    }

    private static bool IsPositiveNumber(object value)
    {
        return value switch
        {
            int intVal => intVal > 0,
            decimal decVal => decVal > 0,
            double dblVal => dblVal > 0,
            float fltVal => fltVal > 0,
            _ => false
        };
    }

    private static bool IsEmpty(object value)
    {
        return value switch
        {
            string strVal => string.IsNullOrWhiteSpace(strVal),
            System.Collections.ICollection collection => collection.Count == 0,
            _ => false
        };
    }
}

/// <summary>
/// Represents a field requirement for stage transition validation.
/// </summary>
public class FieldRequirement
{
    /// <summary>
    /// The property name on the entity.
    /// </summary>
    public required string FieldName { get; set; }

    /// <summary>
    /// The human-readable display name for the field.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Whether the field is required for the stage transition.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Optional validation rule to apply.
    /// </summary>
    public FieldValidationRule ValidationRule { get; set; } = FieldValidationRule.None;

    /// <summary>
    /// Error message to display if validation fails.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}

/// <summary>
/// Validation rules that can be applied to fields.
/// </summary>
public enum FieldValidationRule
{
    None,
    NotEmpty,
    PositiveNumber,
    ValidDate,
    ValidEmail
}

/// <summary>
/// Result of stage transition validation.
/// </summary>
public class StageValidationResult
{
    /// <summary>
    /// Whether all validations passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors if any.
    /// </summary>
    public List<StageValidationError> Errors { get; set; } = new();
}

/// <summary>
/// Represents a single validation error.
/// </summary>
public class StageValidationError
{
    /// <summary>
    /// The field that failed validation.
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the field.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
