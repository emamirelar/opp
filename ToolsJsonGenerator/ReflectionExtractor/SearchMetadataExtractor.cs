using System.Reflection;
using ReflectionExtractor.Models;

namespace ReflectionExtractor;

public class SearchMetadataExtractor
{
    public List<EntitySearchMetadata> ExtractSearchMetadata(Assembly assembly)
    {
        var searchMetadata = new List<EntitySearchMetadata>();
        
        // Find the AdvancedSearchHelper class
        var helperType = assembly.GetTypes()
            .FirstOrDefault(t => t.Name == "AdvancedSearchHelper");
            
        if (helperType == null)
        {
            Console.WriteLine("[WARNING] AdvancedSearchHelper not found in assembly");
            return searchMetadata;
        }

        // Extract metadata for each entity
        var entities = new[] { "Contact", "Partner", "Interaction" };
        
        foreach (var entity in entities)
        {
            var metadata = ExtractEntityMetadata(helperType, entity);
            if (metadata != null)
            {
                searchMetadata.Add(metadata);
                Console.WriteLine($"[OK] Extracted search metadata for {entity}: {metadata.DirectFields.Count + metadata.NestedFields.Values.Sum(v => v.Count)} fields");
            }
        }
        
        return searchMetadata;
    }
    
    private EntitySearchMetadata? ExtractEntityMetadata(Type helperType, string entityName)
    {
        // Get the method that returns allowed fields for this entity
        var methodName = $"Get{entityName}AllowedFields";
        var method = helperType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        
        if (method == null)
        {
            Console.WriteLine($"[WARNING] Method {methodName} not found");
            return null;
        }
        
        try
        {
            // Invoke the method to get the allowed fields
            var result = method.Invoke(null, null);
            if (result is not HashSet<string> allowedFields)
            {
                Console.WriteLine($"[ERROR] Method {methodName} did not return HashSet<string>");
                return null;
            }
            
            var metadata = new EntitySearchMetadata
            {
                Entity = entityName,
                Operators = GetSupportedOperators(),
                DateFields = GetDateFields(entityName),
                ExampleCriteria = GenerateExampleCriteria(entityName)
            };
            
            // Separate direct fields from nested fields
            foreach (var field in allowedFields)
            {
                if (field.Contains('.'))
                {
                    // Nested field like "partner.name"
                    var parts = field.Split('.');
                    if (parts.Length == 2)
                    {
                        var relationEntity = parts[0];
                        var relationField = parts[1];
                        
                        if (!metadata.NestedFields.ContainsKey(relationEntity))
                        {
                            metadata.NestedFields[relationEntity] = new List<string>();
                        }
                        
                        if (!metadata.NestedFields[relationEntity].Contains(relationField))
                        {
                            metadata.NestedFields[relationEntity].Add(relationField);
                        }
                    }
                }
                else
                {
                    // Direct field
                    metadata.DirectFields.Add(field);
                }
            }
            
            return metadata;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to extract metadata for {entityName}: {ex.Message}");
            return null;
        }
    }
    
    private List<string> GetSupportedOperators()
    {
        return new List<string>
        {
            "like", "is", "not", "contains", "startsWith", "endsWith",
            "between", "greaterThan", "lessThan", "greaterThanOrEqual", 
            "lessThanOrEqual", "on", "not on", "this week", "this month", "this year"
        };
    }
    
    private List<string> GetDateFields(string entityName)
    {
        return entityName.ToLower() switch
        {
            "interaction" => new List<string> { "date", "fromDate", "toDate", "createdDate", "modifiedDate" },
            "partner" => new List<string> { "createdDate", "modifiedDate" },
            "contact" => new List<string> { "createdDate", "modifiedDate" },
            _ => new List<string> { "createdDate", "modifiedDate" }
        };
    }
    
    private List<SearchCriteriaExample> GenerateExampleCriteria(string entityName)
    {
        return entityName.ToLower() switch
        {
            "interaction" => new List<SearchCriteriaExample>
            {
                new() { Field = "partner.name", Operator = "like", Value = "UNICEF", Description = "Find interactions with UNICEF partners" },
                new() { Field = "contact.firstName", Operator = "like", Value = "John", LogicalOperator = "AND", Description = "Find interactions with contacts named John" },
                new() { Field = "type", Operator = "is", Value = "Meeting", Description = "Find only meeting interactions" },
                new() { Field = "date", Operator = "between", Value = "2024-01-01,2024-12-31", Description = "Find interactions in 2024" }
            },
            "partner" => new List<SearchCriteriaExample>
            {
                new() { Field = "name", Operator = "like", Value = "Foundation", Description = "Find partners with 'Foundation' in name" },
                new() { Field = "status", Operator = "is", Value = "Active", Description = "Find only active partners" },
                new() { Field = "contact.firstName", Operator = "like", Value = "Sarah", Description = "Find partners with contacts named Sarah" },
                new() { Field = "addressCountry", Operator = "is", Value = "Bangladesh", Description = "Find partners in Bangladesh" }
            },
            "contact" => new List<SearchCriteriaExample>
            {
                new() { Field = "firstName", Operator = "like", Value = "Maria", Description = "Find contacts named Maria" },
                new() { Field = "partner.name", Operator = "like", Value = "UNICEF", Description = "Find contacts from UNICEF partners" },
                new() { Field = "email", Operator = "contains", Value = "@unops.org", Description = "Find UNOPS email contacts" },
                new() { Field = "department", Operator = "is", Value = "Finance", Description = "Find contacts in Finance department" }
            },
            _ => new List<SearchCriteriaExample>()
        };
    }
} 