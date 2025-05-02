using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace UNOPS.PAO.Presentation.Helpers;

public class SearchCriterion
{
    [JsonPropertyName("field")]
    public string Field { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }
    
    [JsonPropertyName("operator")]
    public string Operator { get; set; }
}

public static class AdvancedSearchHelper
{
    public static T MapAdvancedSearchCriteria<T>(string searchCriteria) where T : class, new()
    {
        Debug.WriteLine($"Received search criteria: {searchCriteria}");

        if (string.IsNullOrEmpty(searchCriteria))
        {
            return new T();
        }

        var request = new T();
        try
        {
            var criteria = JsonSerializer.Deserialize<List<SearchCriterion>>(searchCriteria);
            Debug.WriteLine($"Deserialized {criteria?.Count ?? 0} search criteria");
            
            // Log all deserialized criteria
            if (criteria != null)
            {
                foreach (var criterion in criteria)
                {
                    Debug.WriteLine($"CRITERION: Field={criterion.Field}, Value={criterion.Value}, Operator={criterion.Operator ?? "null"}, Label={criterion.Label}");
                }
            }

            // Get all properties including nested ones
            var properties = GetAllProperties(typeof(T));
            Debug.WriteLine($"Available properties in {typeof(T).Name}: {string.Join(", ", properties.Keys)}");

            foreach (var criterion in criteria)
            {
                if (string.IsNullOrEmpty(criterion?.Field))
                {
                    Debug.WriteLine("Skipping criterion with null or empty field");
                    continue;
                }

                Debug.WriteLine($"Processing field: {criterion.Field}, value: {criterion.Value}");

                // Handle nested properties correctly
                if (criterion.Field.Contains("."))
                {
                    // Keep the SearchCriteria and AdvancedSearch flags
                    // to let GenericCompositeSpecification handle the nested properties
                    var searchCriteriaProperty = typeof(T).GetProperty("SearchCriteria");
                    var advancedSearchProperty = typeof(T).GetProperty("AdvancedSearch");
                    
                    if (searchCriteriaProperty != null && advancedSearchProperty != null)
                    {
                        // Add to SearchCriteria collection if it exists
                        List<SearchCriterion> criteriaList;
                        var existingCriteria = searchCriteriaProperty.GetValue(request) as string;
                        
                        if (string.IsNullOrEmpty(existingCriteria))
                        {
                            criteriaList = new List<SearchCriterion>();
                        }
                        else
                        {
                            criteriaList = JsonSerializer.Deserialize<List<SearchCriterion>>(existingCriteria);
                        }
                        
                        // Add criterion with the original operator value preserved
                        criteriaList.Add(criterion);
                        searchCriteriaProperty.SetValue(request, JsonSerializer.Serialize(criteriaList));
                        advancedSearchProperty.SetValue(request, true);
                        
                        Debug.WriteLine($"Added nested property {criterion.Field} to SearchCriteria with operator {criterion.Operator ?? "null"}");
                    }
                    else
                    {
                        Debug.WriteLine($"Unable to handle nested property: {criterion.Field}, missing required properties");
                    }
                    
                    continue; // Skip to next criterion
                }

                // Handle non-nested properties using the original logic
                var lowerField = criterion.Field.ToLower();
                if (properties.TryGetValue(lowerField, out var property))
                {
                    try
                    {
                        object convertedValue = null;
                        if (!string.IsNullOrEmpty(criterion.Value))
                        {
                            var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                            
                            // Use TypeConverter for proper type conversion
                            var converter = TypeDescriptor.GetConverter(propertyType);
                            if (converter != null && converter.CanConvertFrom(typeof(string)))
                            {
                                convertedValue = converter.ConvertFromString(criterion.Value);
                            }
                            else
                            {
                                // Fallback to basic conversion
                                convertedValue = Convert.ChangeType(criterion.Value, propertyType);
                            }
                        }

                        Debug.WriteLine($"Setting property {property.Name} to value: {convertedValue ?? "null"}");
                        property.SetValue(request, convertedValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error setting property {property.Name}: {ex.Message}");
                        throw new ArgumentException($"Error setting value for field '{criterion.Field}': {ex.Message}");
                    }
                }
                else
                {
                    Debug.WriteLine($"Property not found for field: {criterion.Field}");
                }
            }
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"JSON deserialization error: {ex.Message}");
            throw new ArgumentException($"Invalid search criteria format: {ex.Message}", nameof(searchCriteria));
        }

        return request;
    }

    private static Dictionary<string, PropertyInfo> GetAllProperties(Type type)
    {
        var properties = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var prop in type.GetProperties())
        {
            properties[prop.Name.ToLower()] = prop;
            
            // If this is a complex type (but not a string or primitive), get its properties too
            if (prop.PropertyType.IsClass && 
                prop.PropertyType != typeof(string) && 
                !prop.PropertyType.IsPrimitive)
            {
                foreach (var nestedProp in prop.PropertyType.GetProperties())
                {
                    var key = $"{prop.Name}{nestedProp.Name}".ToLower();
                    properties[key] = nestedProp;
                }
            }
        }
        
        return properties;
    }
} 