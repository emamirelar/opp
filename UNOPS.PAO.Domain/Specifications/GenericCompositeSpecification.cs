namespace UNOPS.PAO.Domain.Specifications;

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using System.Diagnostics;
using System.Text.Json;
using UNOPS.PAO.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// A generic composite specification that can handle any type of filter
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TFilter">The filter type</typeparam>
public abstract class GenericCompositeSpecification<TEntity, TFilter> : BaseCompositeSpecification<TEntity>
{
    private static readonly HashSet<string> IgnoredProperties = new() 
    { 
        "PageIndex", "PageSize", "OrderBy", "Ascending", "Direction" 
    };

    protected GenericCompositeSpecification(TFilter filter)
        : base(BuildExpression(filter))
    {
    }

    private static Expression<Func<TEntity, bool>> BuildExpression(TFilter filter)
    {
        if (filter == null)
        {
            Debug.WriteLine("Filter is null, returning default expression");
            return x => true;
        }

        Debug.WriteLine($"Filter type: {typeof(TFilter).Name}");
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var expressions = new List<Expression>();

        // Check if this is an advanced search
        bool isAdvancedSearch = filter.GetType().GetProperty("AdvancedSearch")?.GetValue(filter) as bool? ?? false;
        
        Debug.WriteLine($"AdvancedSearch: {isAdvancedSearch}");

        // Process advanced search criteria if available
        if (isAdvancedSearch)
        {
            Debug.WriteLine("Processing advanced search");
            var searchCriteria = GetSearchCriteria(filter);
            
            if (searchCriteria != null && searchCriteria.Any())
            {
                Debug.WriteLine($"Found {searchCriteria.Count()} search criteria");
                var criteriaExpressions = new List<Expression>();
                
                foreach (var criteria in searchCriteria)
                {
                    try
                    {
                        Debug.WriteLine($"Processing criteria: {criteria}");
                        
                        var criteriaDict = criteria as IDictionary<string, object>;
                        if (criteriaDict == null) 
                        {
                            Debug.WriteLine("Criteria is not a dictionary, skipping");
                            continue;
                        }

                        string field = criteriaDict.TryGetValue("field", out object fieldObj) ? fieldObj?.ToString() : null;
                        string value = criteriaDict.TryGetValue("value", out object valueObj) ? valueObj?.ToString() : null;
                        string comparisonOperator = criteriaDict.TryGetValue("operator", out object opObj) ? opObj?.ToString() ?? "like" : "like";
                        string secondValue = criteriaDict.TryGetValue("secondValue", out object secondValueObj) ? secondValueObj?.ToString() : null;

                        Debug.WriteLine($"Processing criteria - Field: {field}, Value: {value}, ComparisonOperator: {comparisonOperator}");

                        if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(value)) 
                        {
                            Debug.WriteLine("Field or value is empty, skipping");
                            continue;
                        }

                        // Build property access and null checks
                        var propertyAccess = BuildPropertyAccess(parameter, field);
                        if (propertyAccess == null) 
                        {
                            Debug.WriteLine($"Failed to build property access for {field}");
                            continue;
                        }

                        Debug.WriteLine($"Property access built for {field}, type: {propertyAccess.Type}");

                        // Create the comparison expression
                        Expression comparisonExpr = null;
                        
                        if (propertyAccess.Type == typeof(string))
                        {
                            var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));

                            // Handle different string operators
                            switch (comparisonOperator.ToLower())
                            {
                                case "like":
                                    // Try to use EF.Functions.Like for case-insensitive contains
                                    try
                                    {
                                        var efType = typeof(EF);
                                        var functionsProperty = efType.GetProperty("Functions");
                                        var functionsExpression = Expression.Property(null, functionsProperty);
                                        var likeMethod = functionsProperty.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
                                        
                                        if (likeMethod != null)
                                        {
                                            var pattern = $"%{value}%";
                                            var likeCall = Expression.Call(functionsExpression, likeMethod, propertyAccess, Expression.Constant(pattern));
                                            comparisonExpr = Expression.AndAlso(nullCheck, likeCall);
                                        }
                                        else
                                        {
                                            // Fallback to Contains
                                            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                            var containsCall = Expression.Call(propertyAccess, containsMethod, Expression.Constant(value));
                                            comparisonExpr = Expression.AndAlso(nullCheck, containsCall);
                                        }
                                    }
                                    catch
                                    {
                                        // Fallback to Contains if EF.Functions is not available
                                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                        var containsCall = Expression.Call(propertyAccess, containsMethod, Expression.Constant(value));
                                        comparisonExpr = Expression.AndAlso(nullCheck, containsCall);
                                    }
                                    break;
                                case "not like":
                                    try
                                    {
                                        var efType2 = typeof(EF);
                                        var functionsProperty2 = efType2.GetProperty("Functions");
                                        var functionsExpression2 = Expression.Property(null, functionsProperty2);
                                        var likeMethod2 = functionsProperty2.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
                                        
                                        if (likeMethod2 != null)
                                        {
                                            var pattern2 = $"%{value}%";
                                            var notLikeCall = Expression.Call(functionsExpression2, likeMethod2, propertyAccess, Expression.Constant(pattern2));
                                            comparisonExpr = Expression.AndAlso(nullCheck, Expression.Not(notLikeCall));
                                        }
                                        else
                                        {
                                            // Fallback to Not Contains
                                            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                            var containsCall = Expression.Call(propertyAccess, containsMethod, Expression.Constant(value));
                                            comparisonExpr = Expression.AndAlso(nullCheck, Expression.Not(containsCall));
                                        }
                                    }
                                    catch
                                    {
                                        // Fallback to Not Contains
                                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                        var containsCall = Expression.Call(propertyAccess, containsMethod, Expression.Constant(value));
                                        comparisonExpr = Expression.AndAlso(nullCheck, Expression.Not(containsCall));
                                    }
                                    break;
                                case "is":
                                    comparisonExpr = Expression.Equal(propertyAccess, Expression.Constant(value));
                                    break;
                                case "is not":
                                    comparisonExpr = Expression.NotEqual(propertyAccess, Expression.Constant(value));
                                    break;
                                default:
                                    // Default to "like" behavior with fallback
                                    try
                                    {
                                        var efTypeDefault = typeof(EF);
                                        var functionsPropertyDefault = efTypeDefault.GetProperty("Functions");
                                        var functionsExpressionDefault = Expression.Property(null, functionsPropertyDefault);
                                        var likeMethodDefault = functionsPropertyDefault.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
                                        
                                        if (likeMethodDefault != null)
                                        {
                                            var patternDefault = $"%{value}%";
                                            var likeCallDefault = Expression.Call(functionsExpressionDefault, likeMethodDefault, propertyAccess, Expression.Constant(patternDefault));
                                            comparisonExpr = Expression.AndAlso(nullCheck, likeCallDefault);
                                        }
                                        else
                                        {
                                            // Fallback to Contains
                                            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                            var containsCall = Expression.Call(propertyAccess, containsMethod, Expression.Constant(value));
                                            comparisonExpr = Expression.AndAlso(nullCheck, containsCall);
                                        }
                                    }
                                    catch
                                    {
                                        // Fallback to Contains
                                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                        var containsCall = Expression.Call(propertyAccess, containsMethod, Expression.Constant(value));
                                        comparisonExpr = Expression.AndAlso(nullCheck, containsCall);
                                    }
                                    break;
                            }
                        }
                        else if (propertyAccess.Type == typeof(DateTime) || propertyAccess.Type == typeof(DateTime?))
                        {
                            if (DateTime.TryParse(value, out var dateValue))
                            {
                                var constant = Expression.Constant(dateValue, propertyAccess.Type);

                                // Handle different date operators
                                switch (comparisonOperator.ToLower())
                                {
                                    case ">":
                                    case "after":
                                        comparisonExpr = Expression.GreaterThan(propertyAccess, constant);
                                        break;
                                    case ">=":
                                        comparisonExpr = Expression.GreaterThanOrEqual(propertyAccess, constant);
                                        break;
                                    case "<":
                                    case "before":
                                        comparisonExpr = Expression.LessThan(propertyAccess, constant);
                                        break;
                                    case "<=":
                                        comparisonExpr = Expression.LessThanOrEqual(propertyAccess, constant);
                                        break;
                                    case "between":
                                        if (!string.IsNullOrEmpty(secondValue) && DateTime.TryParse(secondValue, out var endDateValue))
                                        {
                                            var endConstant = Expression.Constant(endDateValue, propertyAccess.Type);
                                            var startComparison = Expression.GreaterThanOrEqual(propertyAccess, constant);
                                            var endComparison = Expression.LessThanOrEqual(propertyAccess, endConstant);
                                            comparisonExpr = Expression.AndAlso(startComparison, endComparison);
                                        }
                                        else
                                        {
                                            Debug.WriteLine($"Invalid second date value for 'between' operator: {secondValue}");
                                            continue;
                                        }
                                        break;
                                    case "is":
                                        comparisonExpr = Expression.Equal(propertyAccess, constant);
                                        break;
                                    case "is not":
                                        comparisonExpr = Expression.NotEqual(propertyAccess, constant);
                                        break;
                                    default:
                                        comparisonExpr = Expression.Equal(propertyAccess, constant);
                                        break;
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"Failed to parse date value: {value}");
                                continue;
                            }
                        }
                        else if (propertyAccess.Type == typeof(int) || propertyAccess.Type == typeof(int?) ||
                                 propertyAccess.Type == typeof(decimal) || propertyAccess.Type == typeof(decimal?) ||
                                 propertyAccess.Type == typeof(double) || propertyAccess.Type == typeof(double?))
                        {
                            try
                            {
                                var numericValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(propertyAccess.Type) ?? propertyAccess.Type);
                                var constant = Expression.Constant(numericValue, propertyAccess.Type);

                                // Handle different numeric operators
                                switch (comparisonOperator.ToLower())
                                {
                                    case ">":
                                        comparisonExpr = Expression.GreaterThan(propertyAccess, constant);
                                        break;
                                    case ">=":
                                        comparisonExpr = Expression.GreaterThanOrEqual(propertyAccess, constant);
                                        break;
                                    case "<":
                                        comparisonExpr = Expression.LessThan(propertyAccess, constant);
                                        break;
                                    case "<=":
                                        comparisonExpr = Expression.LessThanOrEqual(propertyAccess, constant);
                                        break;
                                    case "is":
                                        comparisonExpr = Expression.Equal(propertyAccess, constant);
                                        break;
                                    case "is not":
                                        comparisonExpr = Expression.NotEqual(propertyAccess, constant);
                                        break;
                                    default:
                                        comparisonExpr = Expression.Equal(propertyAccess, constant);
                                        break;
                                }
                            }
                            catch (Exception conversionEx)
                            {
                                Debug.WriteLine($"Failed to convert numeric value: {value}, Error: {conversionEx.Message}");
                                continue;
                            }
                        }
                        else if (propertyAccess.Type == typeof(bool) || propertyAccess.Type == typeof(bool?))
                        {
                            if (bool.TryParse(value, out var boolValue))
                            {
                                var constant = Expression.Constant(boolValue, propertyAccess.Type);
                                
                                // Handle boolean operators
                                switch (comparisonOperator.ToLower())
                                {
                                    case "is not":
                                        comparisonExpr = Expression.NotEqual(propertyAccess, constant);
                                        break;
                                    case "is":
                                    default:
                                        comparisonExpr = Expression.Equal(propertyAccess, constant);
                                        break;
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"Failed to parse boolean value: {value}");
                                continue;
                            }
                        }
                        else
                        {
                            comparisonExpr = Expression.Equal(propertyAccess, Expression.Constant(value));
                        }

                        if (comparisonExpr != null)
                        {
                            criteriaExpressions.Add(comparisonExpr);
                            Debug.WriteLine($"Successfully created comparison expression for {field}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing criteria: {ex.Message}");
                        continue;
                    }
                }

                // Combine criteria expressions based on logical operators
                if (criteriaExpressions.Any())
                {
                    Expression advancedSearchExpression = criteriaExpressions[0];
                    
                    // For now, use AND to combine multiple criteria
                    for (int i = 1; i < criteriaExpressions.Count; i++)
                    {
                        advancedSearchExpression = Expression.AndAlso(advancedSearchExpression, criteriaExpressions[i]);
                    }
                    
                    expressions.Add(advancedSearchExpression);
                    Debug.WriteLine("Added advanced search expression to final expressions");
                }
            }
            else
            {
                Debug.WriteLine("No search criteria found for advanced search");
            }
        }

        // Process regular properties (these should be combined with AND)
        var propertyExpressions = typeof(TFilter)
            .GetProperties()
            .Where(p => !ShouldIgnoreProperty(p) && p.Name != "SearchText" && 
                   p.Name != "AdvancedSearch" && p.Name != "SearchCriteria")
            .Select(p => new { Property = p, Value = p.GetValue(filter) })
            .Where(x => x.Value != null && !string.IsNullOrEmpty(x.Value.ToString()))
            .Select(x => 
            {
                Debug.WriteLine($"Processing filter property: {x.Property.Name} with value: {x.Value}");
                return CreatePropertyExpression(parameter, x.Property, x.Value);
            })
            .Where(expr => expr != null);

        foreach (var expr in propertyExpressions)
        {
            expressions.Add(expr.Body);
            Debug.WriteLine($"Added property expression to final expressions");
        }

        // Handle search text (this should be combined with AND)
        if (filter is ISearchFilter searchFilter && !string.IsNullOrWhiteSpace(searchFilter.SearchText))
        {
            Debug.WriteLine($"Processing search text: '{searchFilter.SearchText}'");
            var searchExpression = CreateSearchTextExpression(parameter, searchFilter.SearchText);
            if (searchExpression != null)
            {
                expressions.Add(searchExpression.Body);
                Debug.WriteLine("Added search text expression");
            }
        }

        if (!expressions.Any())
        {
            Debug.WriteLine("No valid expressions created, returning default expression");
            return x => true;
        }

        Debug.WriteLine($"Total expressions to combine: {expressions.Count}");

        // Combine all expressions with AND
        Expression finalExpression = expressions[0];
        for (int i = 1; i < expressions.Count; i++)
        {
            finalExpression = Expression.AndAlso(finalExpression, expressions[i]);
        }

        Debug.WriteLine($"Final expression created: {finalExpression}");
        return Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);
    }

    private static Expression<Func<TEntity, bool>> CreateSearchTextExpression(ParameterExpression parameter, string searchText)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                Debug.WriteLine("Search text is empty or whitespace");
                return null;
            }

            var searchTermValue = searchText.Trim();
            Debug.WriteLine($"Creating search text expression for: {searchTermValue}");

            var searchableProperties = GetSearchableProperties(typeof(TEntity));
            Debug.WriteLine($"Found {searchableProperties.Count} searchable properties: {string.Join(", ", searchableProperties)}");

            if (!searchableProperties.Any())
            {
                Debug.WriteLine("No searchable properties found");
                return null;
            }

            var propertyExpressions = new List<Expression>();

            foreach (var propertyPath in searchableProperties)
            {
                try
                {
                    Debug.WriteLine($"Processing property path: {propertyPath}");
                    var propertyAccess = BuildPropertyAccess(parameter, propertyPath);
                    if (propertyAccess == null)
                    {
                        Debug.WriteLine($"Failed to build property access for {propertyPath}");
                        continue;
                    }

                    var nullChecks = BuildNullChecks(parameter, propertyPath);
                    Debug.WriteLine($"Built {nullChecks.Count} null checks for {propertyPath}");

                    // Use EF.Functions.Like for string comparison, with fallback to Contains
                    Expression stringComparisonExpr;
                    try
                    {
                        var efType = typeof(EF);
                        var functionsProperty = efType.GetProperty("Functions");
                        var functionsExpression = Expression.Property(null, functionsProperty);
                        var likeMethod = functionsProperty.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
                        
                        if (likeMethod != null)
                        {
                            var pattern = $"%{searchTermValue}%";
                            stringComparisonExpr = Expression.Call(
                                functionsExpression,
                                likeMethod,
                                propertyAccess,
                                Expression.Constant(pattern)
                            );
                        }
                        else
                        {
                            // Fallback to Contains
                            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                            stringComparisonExpr = Expression.Call(propertyAccess, containsMethod, Expression.Constant(searchTermValue));
                        }
                    }
                    catch
                    {
                        // Fallback to Contains if EF.Functions is not available
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        stringComparisonExpr = Expression.Call(propertyAccess, containsMethod, Expression.Constant(searchTermValue));
                    }

                    // Combine null checks with string comparison
                    var finalExpression = nullChecks.Aggregate(
                        stringComparisonExpr,
                        (current, nullCheck) => Expression.AndAlso(nullCheck, current)
                    );

                    propertyExpressions.Add(finalExpression);
                    Debug.WriteLine($"Successfully created expression for {propertyPath}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error creating expression for {propertyPath}: {ex.Message}");
                }
            }

            if (!propertyExpressions.Any())
            {
                Debug.WriteLine("No valid property expressions created for search");
                return null;
            }

            // Combine all property expressions with OR
            Expression combinedExpression = propertyExpressions[0];
            for (int i = 1; i < propertyExpressions.Count; i++)
            {
                combinedExpression = Expression.OrElse(combinedExpression, propertyExpressions[i]);
            }

            return Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in CreateSearchTextExpression: {ex.Message}");
            return null;
        }
    }

    private static Expression BuildPropertyAccess(ParameterExpression parameter, string propertyPath)
    {
        try
        {
            Debug.WriteLine($"Building property access for: {propertyPath}");
            
            // Convert camelCase to PascalCase if needed
            var pascalCaseField = ConvertToPascalCase(propertyPath);
            Debug.WriteLine($"Converted to PascalCase: {pascalCaseField}");
            
            var parts = pascalCaseField.Split('.');
            Expression expression = parameter;
            
            foreach (var part in parts)
            {
                var currentType = expression.Type;
                Debug.WriteLine($"Looking for property '{part}' on type {currentType.Name}");
                
                var property = currentType.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property == null)
                {
                    Debug.WriteLine($"Property '{part}' not found on type {currentType.Name}");
                    return null;
                }
                
                expression = Expression.Property(expression, property);
                Debug.WriteLine($"Built property access for '{part}', new type: {expression.Type.Name}");
            }
            
            return expression;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error building property access for {propertyPath}: {ex.Message}");
            return null;
        }
    }

    private static string ConvertToPascalCase(string camelCaseField)
    {
        if (string.IsNullOrEmpty(camelCaseField))
            return camelCaseField;
            
        // If it contains dots, process each part
        if (camelCaseField.Contains("."))
        {
            var parts = camelCaseField.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
            }
            return string.Join(".", parts);
        }
        
        return char.ToUpper(camelCaseField[0]) + camelCaseField.Substring(1);
    }

    private static List<Expression> BuildNullChecks(ParameterExpression parameter, string propertyPath)
    {
        var nullChecks = new List<Expression>();
        var parts = propertyPath.Split('.');
        Expression currentExpression = parameter;
        
        // Build null checks for all intermediate navigation properties
        for (int i = 0; i < parts.Length - 1; i++)
        {
            var currentType = currentExpression.Type;
            var property = currentType.GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            
            if (property != null)
            {
                currentExpression = Expression.Property(currentExpression, property);
                
                // Add null check for reference types
                if (!property.PropertyType.IsValueType || 
                    (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    nullChecks.Add(Expression.NotEqual(currentExpression, Expression.Constant(null)));
                }
            }
        }
        
        return nullChecks;
    }

    private static List<string> GetSearchableProperties(Type type, string prefix = "")
    {
        var searchableProperties = new List<string>();
        Debug.WriteLine($"Getting searchable properties for type: {type.Name}");

        // Define core searchable properties for Contact
        if (type.Name == "Contact")
        {
            searchableProperties.AddRange(new[]
            {
                "FirstName",
                "LastName",
                "Email",
                "Title",
                "Department",
                "Description",
                "Phone",
                "Mobile",
                "Assistant",
                "AssistantEmail",
                "AssistantPhone",
                "MailingCity",
                "MailingStateProvince",
                "MailingPostalCode",
                "MailingCountry"
            });

            // Add Partner properties
            searchableProperties.AddRange(new[]
            {
                "Partner.Name",
                "Partner.ShortName",
                "Partner.Status"
            });

            Debug.WriteLine($"Added {searchableProperties.Count} searchable properties for Contact");
            return searchableProperties;
        }

        // For other types, use reflection to find string properties
        foreach (var property in type.GetProperties())
        {
            var propertyPath = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
            Debug.WriteLine($"Checking property: {propertyPath}");

            if (property.PropertyType == typeof(string))
            {
                searchableProperties.Add(propertyPath);
                Debug.WriteLine($"Added string property: {propertyPath}");
            }
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && prefix.Split('.').Length < 2)
            {
                // Limit nesting to avoid infinite recursion
                searchableProperties.AddRange(GetSearchableProperties(property.PropertyType, propertyPath));
            }
        }

        return searchableProperties;
    }

    private static Expression<Func<TEntity, bool>> CreatePropertyExpression(
        ParameterExpression parameter,
        PropertyInfo property,
        object value)
    {
        try
        {
            if (property.Name == "SearchText")
                return null;

            Debug.WriteLine($"Creating expression for property: {property.Name}");

            // Get property path (for nested properties)
            var propertyPath = GetPropertyPath(property.Name);
            Debug.WriteLine($"Property path parts: {string.Join(" -> ", propertyPath)}");

            Expression propertyAccess = parameter;

            // Build the property access chain
            foreach (var pathPart in propertyPath)
            {
                var currentType = propertyAccess.Type;
                var currentProperty = currentType.GetProperty(pathPart, 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                
                if (currentProperty == null)
                {
                    Debug.WriteLine($"Property {pathPart} not found on type {currentType.Name}");
                    return null;
                }

                propertyAccess = Expression.Property(propertyAccess, currentProperty);
            }

            // Create the comparison based on the property type
            Expression comparison;
            if (propertyAccess.Type == typeof(string))
            {
                var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));
                
                // Use EF.Functions.Like with fallback to Contains
                Expression stringComparisonExpr;
                try
                {
                    var efType = typeof(EF);
                    var functionsProperty = efType.GetProperty("Functions");
                    var functionsExpression = Expression.Property(null, functionsProperty);
                    var likeMethod = functionsProperty.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
                    
                    if (likeMethod != null)
                    {
                        var pattern = $"%{value}%";
                        stringComparisonExpr = Expression.Call(
                            functionsExpression,
                            likeMethod,
                            propertyAccess,
                            Expression.Constant(pattern)
                        );
                    }
                    else
                    {
                        // Fallback to Contains
                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        stringComparisonExpr = Expression.Call(propertyAccess, containsMethod, Expression.Constant(value));
                    }
                }
                catch
                {
                    // Fallback to Contains if EF.Functions is not available
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    stringComparisonExpr = Expression.Call(propertyAccess, containsMethod, Expression.Constant(value));
                }
                
                comparison = Expression.AndAlso(nullCheck, stringComparisonExpr);
            }
            else
            {
                // For non-string types, use equality comparison
                comparison = Expression.Equal(propertyAccess, Expression.Constant(value));
            }

            return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error creating property expression for {property.Name}: {ex.Message}");
            return null;
        }
    }

    private static List<string> GetPropertyPath(string propertyName)
    {
        // Only split on explicit dot notation (e.g., "Partner.Name")
        if (propertyName.Contains("."))
        {
            return propertyName.Split('.').ToList();
        }

        // Return the property name as is, don't split PascalCase
        return new List<string> { propertyName };
    }

    private static bool ShouldIgnoreProperty(PropertyInfo property)
    {
        return IgnoredProperties.Contains(property.Name);
    }

    private static IEnumerable<dynamic> GetSearchCriteria(TFilter filter)
    {
        var searchCriteriaProperty = filter.GetType().GetProperty("SearchCriteria");
        if (searchCriteriaProperty == null) 
        {
            Debug.WriteLine("SearchCriteria property not found");
            return null;
        }

        var searchCriteriaValue = searchCriteriaProperty.GetValue(filter);
        Debug.WriteLine($"SearchCriteria value: {searchCriteriaValue}");
        
        if (searchCriteriaValue is string searchCriteriaString && !string.IsNullOrEmpty(searchCriteriaString))
        {
            try
            {
                Debug.WriteLine($"Parsing JSON: {searchCriteriaString}");
                using var doc = JsonDocument.Parse(searchCriteriaString);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var criteriaList = new List<Dictionary<string, object>>();
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        var criterionDict = new Dictionary<string, object>();
                        foreach (var property in element.EnumerateObject())
                        {
                            criterionDict[property.Name] = property.Value.GetString();
                        }
                        criteriaList.Add(criterionDict);
                    }
                    
                    Debug.WriteLine($"Successfully parsed {criteriaList.Count} criteria");
                    return criteriaList.Cast<dynamic>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deserializing SearchCriteria: {ex.Message}");
            }
        }
        
        return null;
    }
} 