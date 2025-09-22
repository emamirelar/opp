using System.ComponentModel.DataAnnotations;
using UNOPS.PAO.ExternalDataService.Models.Configuration;
using Cronos;

namespace UNOPS.PAO.ExternalDataService.Services.Configuration;

public class ConfigurationValidator
{
    private readonly ILogger<ConfigurationValidator> _logger;

    public ConfigurationValidator(ILogger<ConfigurationValidator> logger)
    {
        _logger = logger;
    }

    public Task<bool> ValidateAsync(SyncConfiguration configuration)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(configuration);
        
        if (!ValidationExtensions.TryValidateObjectRecursively(configuration, validationContext, validationResults, true))
        {
            foreach (var result in validationResults)
            {
                _logger.LogError("Configuration validation error: {ErrorMessage} for {MemberNames}", 
                    result.ErrorMessage, string.Join(", ", result.MemberNames ?? new string[0]));
            }
            return Task.FromResult(false);
        }

        // Custom validation logic
        var isValid = true;

        // Validate cron expression if provided
        if (!string.IsNullOrEmpty(configuration.Metadata.ScheduleCron))
        {
            try
            {
                CronExpression.Parse(configuration.Metadata.ScheduleCron);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid cron expression: {CronExpression}", configuration.Metadata.ScheduleCron);
                isValid = false;
            }
        }

        // Validate source query contains required elements
        if (string.IsNullOrEmpty(configuration.Source.Query))
        {
            _logger.LogError("Source query cannot be empty");
            isValid = false;
        }
        else if (!configuration.Source.Query.ToUpper().Contains("SELECT"))
        {
            _logger.LogError("Source query must contain SELECT statement");
            isValid = false;
        }

        // Validate field mappings
        if (!configuration.Destination.FieldMappings.Any())
        {
            _logger.LogError("Destination must have at least one field mapping");
            isValid = false;
        }

        // Check for unique destination field names
        var duplicateFields = configuration.Destination.FieldMappings
            .GroupBy(f => f.DestinationField)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        if (duplicateFields.Any())
        {
            _logger.LogError("Duplicate destination fields found: {DuplicateFields}", string.Join(", ", duplicateFields));
            isValid = false;
        }

        // Validate foreign key mappings
        foreach (var fkMapping in configuration.Destination.ForeignKeyMappings)
        {
            if (string.IsNullOrEmpty(fkMapping.LookupTable) || 
                string.IsNullOrEmpty(fkMapping.LookupField) ||
                string.IsNullOrEmpty(fkMapping.DestinationField))
            {
                _logger.LogError("Foreign key mapping incomplete: {SourceField}", fkMapping.SourceField);
                isValid = false;
            }
        }

        // Validate data type mappings
        foreach (var fieldMapping in configuration.Destination.FieldMappings)
        {
            if (!IsValidDataType(fieldMapping.DataType))
            {
                _logger.LogError("Invalid data type: {DataType} for field {DestinationField}", 
                    fieldMapping.DataType, fieldMapping.DestinationField);
                isValid = false;
            }
        }

        if (isValid)
        {
            _logger.LogInformation("Configuration validation passed for {ConfigName}", configuration.Metadata.Name);
        }

        return Task.FromResult(isValid);
    }

    private bool IsValidDataType(string dataType)
    {
        var validTypes = new[]
        {
            "varchar", "text", "char", "integer", "bigint", "smallint",
            "decimal", "numeric", "real", "double precision", "money",
            "boolean", "date", "time", "timestamp", "interval",
            "uuid", "json", "jsonb", "xml", "bytea"
        };

        var normalizedType = dataType.ToLower();
        
        // Handle parameterized types like varchar(255)
        if (normalizedType.Contains('('))
        {
            normalizedType = normalizedType.Substring(0, normalizedType.IndexOf('('));
        }

        return validTypes.Contains(normalizedType);
    }
}

// Extension method for recursive validation
public static class ValidationExtensions
{
    public static bool TryValidateObjectRecursively<T>(T obj, ValidationContext validationContext, ICollection<ValidationResult> results, bool validateAllProperties)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }
        
        return Validator.TryValidateObject(obj, validationContext, results, validateAllProperties);
    }
}
