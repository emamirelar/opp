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

    private static readonly Dictionary<Type, Func<Expression, object, Expression>> PropertyHandlers = new()
    {
        { typeof(string), HandleStringProperty },
        { typeof(int), HandleNumericProperty },
        { typeof(int?), HandleNumericProperty },
        { typeof(bool), HandleBooleanProperty },
        { typeof(bool?), HandleBooleanProperty },
        { typeof(DateTime), HandleDateTimeProperty },
        { typeof(DateTime?), HandleDateTimeProperty }
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
        
        // Process advanced search criteria if available
        if (isAdvancedSearch)
        {
            Debug.WriteLine("Processing advanced search");
            var searchCriteria = GetSearchCriteria(filter);
            
            if (searchCriteria != null)
            {
                var andCriteria = new List<Expression>();
                var orCriteria = new List<Expression>();
                
                foreach (var criteria in searchCriteria)
                {
                    try
                    {
                        var criteriaDict = criteria as IDictionary<string, object>;
                        if (criteriaDict == null) continue;

                        string field = criteriaDict.TryGetValue("field", out object fieldObj) ? fieldObj?.ToString() : null;
                        string value = criteriaDict.TryGetValue("value", out object valueObj) ? valueObj?.ToString() : null;
                        string op = criteriaDict.TryGetValue("operator", out object opObj) ? opObj?.ToString() ?? "AND" : "AND";

                        Debug.WriteLine($"Processing criteria - Field: {field}, Value: {value}, Operator: {op}");

                        if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(value)) continue;

                        // Build property access and null checks
                        var propertyAccess = BuildPropertyAccess(parameter, field);
                        if (propertyAccess == null) continue;

                        // Create the comparison expression
                        Expression comparisonExpr;
                        if (propertyAccess.Type == typeof(string))
                        {
                            var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));
                            
                            // Use EF.Functions.Like instead of Contains
                            var efType = typeof(EF);
                            var functionsProperty = efType.GetProperty("Functions");
                            var functionsExpression = Expression.Property(null, functionsProperty);
                            var likeMethod = functionsProperty.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
                            var pattern = $"%{value}%";
                            var likeCall = Expression.Call(
                                functionsExpression, 
                                likeMethod, 
                                propertyAccess, 
                                Expression.Constant(pattern)
                            );
                            
                            comparisonExpr = Expression.AndAlso(nullCheck, likeCall);
                        }
                        else
                        {
                            comparisonExpr = Expression.Equal(propertyAccess, Expression.Constant(value));
                        }

                        // Add null checks for nested properties
                        var parts = field.Split('.');
                        Expression currentExpr = parameter;
                        for (int i = 0; i < parts.Length - 1; i++)
                        {
                            currentExpr = Expression.PropertyOrField(currentExpr, parts[i]);
                            var nullCheck = Expression.NotEqual(currentExpr, Expression.Constant(null));
                            comparisonExpr = Expression.AndAlso(nullCheck, comparisonExpr);
                        }

                        // Add to appropriate list based on operator
                        if (op.Equals("OR", StringComparison.OrdinalIgnoreCase))
                        {
                            orCriteria.Add(comparisonExpr);
                        }
                        else
                        {
                            andCriteria.Add(comparisonExpr);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing criteria: {ex.Message}");
                        continue;
                    }
                }

                // Handle advanced search expressions
                Expression advancedSearchExpression = null;

                // Combine AND criteria if any exist
                if (andCriteria.Any())
                {
                    advancedSearchExpression = andCriteria.Aggregate(Expression.AndAlso);
                }

                // Combine OR criteria if any exist
                if (orCriteria.Any())
                {
                    var orExpression = orCriteria.Aggregate(Expression.OrElse);
                    // If we already have AND criteria, add the OR criteria with OR, otherwise just use OR criteria
                    advancedSearchExpression = advancedSearchExpression != null
                        ? Expression.OrElse(advancedSearchExpression, orExpression)
                        : orExpression;
                }

                if (advancedSearchExpression != null)
                {
                    expressions.Add(advancedSearchExpression);
                }
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
                var expr = CreatePropertyExpression(parameter, x.Property, x.Value);
                if (expr == null)
                {
                    Debug.WriteLine($"Failed to create expression for property {x.Property.Name}");
                    return null;
                }
                return expr.Body;
            })
            .Where(expr => expr != null);

        expressions.AddRange(propertyExpressions);

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

        // Combine all expressions with AND (advanced search is already properly combined internally)
        var combinedExpression = expressions.Aggregate(Expression.AndAlso);
        return Expression.Lambda<Func<TEntity, bool>>(combinedExpression, parameter);
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

            var propertyExpressions = new List<Expression<Func<TEntity, bool>>>();

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

                    // Use EF.Functions.Like for string comparison
                    var efType = typeof(EF);
                    var functionsProperty = efType.GetProperty("Functions");
                    var functionsExpression = Expression.Property(null, functionsProperty);
                    var likeMethod = functionsProperty.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
                    var pattern = $"%{searchTermValue}%";
                    
                    var likeCall = Expression.Call(
                        functionsExpression,
                        likeMethod,
                        propertyAccess,
                        Expression.Constant(pattern)
                    );

                    // Combine null checks with LIKE
                    var finalExpression = nullChecks.Aggregate(
                        (Expression)likeCall,
                        (current, nullCheck) => Expression.AndAlso(nullCheck, current)
                    );

                    var lambda = Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);
                    propertyExpressions.Add(lambda);
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
            var combinedExpression = CombineOrExpressions(propertyExpressions, parameter);
            
            Debug.WriteLine("Successfully created combined search expression");
            return combinedExpression;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in CreateSearchTextExpression: {ex.Message}");
            return null;
        }
    }

    // Add a new method to combine expressions with OR without using Expression.Invoke
    private static Expression<Func<TEntity, bool>> CombineOrExpressions(List<Expression<Func<TEntity, bool>>> expressions, ParameterExpression parameter)
    {
        if (expressions.Count == 0)
            return x => false;
            
        if (expressions.Count == 1)
            return expressions[0];
            
        // Start with the first expression's body
        var firstExpr = expressions[0];
        var body = firstExpr.Body;
            
        // Replace parameter references in other expressions and combine with OR
        var parameterReplacer = new ParameterReplacer(parameter);
            
        for (int i = 1; i < expressions.Count; i++)
        {
            var expr = expressions[i];
            var replacedBody = parameterReplacer.Replace(expr.Body);
            body = Expression.OrElse(body, replacedBody);
        }
            
        return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
    }
    
    // Add a parameter replacer class to help with expression combining
    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        
        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }
        
        public Expression Replace(Expression node)
        {
            return Visit(node);
        }
        
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }

    private static Expression BuildPropertyAccess(ParameterExpression parameter, string propertyPath)
    {
        try
        {
            Debug.WriteLine($"Building property access for path: {propertyPath}");
            var parts = propertyPath.Split('.');
            var currentExpression = (Expression)parameter;

            foreach (var part in parts)
            {
                Debug.WriteLine($"Processing property part: {part}");
                currentExpression = Expression.PropertyOrField(currentExpression, part);
            }

            Debug.WriteLine($"Successfully built property access for {propertyPath}");
            return currentExpression;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error building property access for {propertyPath}: {ex.Message}");
            return null;
        }
    }

    private static List<Expression> BuildNullChecks(ParameterExpression parameter, string propertyPath)
    {
        var nullChecks = new List<Expression>();
        Debug.WriteLine($"Building null checks for path: {propertyPath}");
        
        try
        {
            var parts = propertyPath.Split('.');
            var currentExpression = (Expression)parameter;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                currentExpression = Expression.PropertyOrField(currentExpression, parts[i]);
                var nullCheck = Expression.NotEqual(currentExpression, Expression.Constant(null));
                nullChecks.Add(nullCheck);
                Debug.WriteLine($"Added null check for part: {parts[i]}");
            }

            Debug.WriteLine($"Created {nullChecks.Count} null checks");
            return nullChecks;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error building null checks: {ex.Message}");
            return nullChecks;
        }
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
            else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
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
            var nullChecks = new List<Expression>();

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
                
                // Add null check for reference types
                if (currentProperty.PropertyType.IsClass)
                {
                    nullChecks.Add(Expression.NotEqual(propertyAccess, Expression.Constant(null)));
                }
            }

            // Create the comparison based on the property type
            Expression comparison;
            if (propertyAccess.Type == typeof(string))
            {
                var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));
                
                // Use EF.Functions.Like instead of Contains
                var efType = typeof(EF);
                var functionsProperty = efType.GetProperty("Functions");
                var functionsExpression = Expression.Property(null, functionsProperty);
                var likeMethod = functionsProperty.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
                var pattern = $"%{value}%";
                
                var likeCall = Expression.Call(
                    functionsExpression,
                    likeMethod,
                    propertyAccess,
                    Expression.Constant(pattern)
                );
                
                comparison = Expression.AndAlso(nullCheck, likeCall);
            }
            else
            {
                // For non-string types, use equality comparison
                comparison = Expression.Equal(propertyAccess, Expression.Constant(value));
            }

            // Combine null checks with the comparison
            Expression finalExpression = comparison;
            foreach (var nullCheck in nullChecks.AsEnumerable().Reverse())
            {
                finalExpression = Expression.AndAlso(nullCheck, finalExpression);
            }

            return Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);
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

    private static Expression CreateComparison(Expression propertyAccess, object value, Type propertyType)
    {
        if (propertyAccess == null || value == null)
            return null;

        if (PropertyHandlers.TryGetValue(propertyType, out var handler))
        {
            return handler(propertyAccess, value);
        }

        return Expression.Equal(propertyAccess, Expression.Constant(value));
    }

    private static Expression HandleStringProperty(Expression propertyAccess, object value)
    {
        try
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return null;

            var stringValue = value.ToString();
            
            // Use EF.Functions.Like for case-insensitive contains
            var efType = typeof(EF);
            var functionsProperty = efType.GetProperty("Functions");
            var functionsExpression = Expression.Property(null, functionsProperty);
            
            var likeMethod = functionsProperty.PropertyType.GetMethod("Like", 
                new[] { typeof(string), typeof(string) });
            
            var pattern = $"%{stringValue}%";
            
            var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));
            var likeCall = Expression.Call(
                functionsExpression,
                likeMethod,
                propertyAccess,
                Expression.Constant(pattern)
            );
            
            return Expression.AndAlso(nullCheck, likeCall);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error handling string property: {ex.Message}");
            return null;
        }
    }

    private static Expression HandleNumericProperty(Expression propertyAccess, object value)
    {
        return Expression.Equal(propertyAccess, Expression.Constant(value));
    }

    private static Expression HandleBooleanProperty(Expression propertyAccess, object value)
    {
        return Expression.Equal(propertyAccess, Expression.Constant(value));
    }

    private static Expression HandleDateTimeProperty(Expression propertyAccess, object value)
    {
        return Expression.Equal(propertyAccess, Expression.Constant(value));
    }

    private static MethodInfo GetStringContainsMethod()
    {
        try
        {
            // First try to get the method with StringComparison parameter (available in .NET Core)
            var method = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
            if (method != null)
            {
                return method;
            }
        }
        catch
        {
            // Method doesn't exist in this runtime
        }
        
        // Fall back to the method without StringComparison parameter
        return typeof(string).GetMethod("Contains", new[] { typeof(string) });
    }

    private static Expression CreateStringContainsExpression(Expression stringExpression, string value)
    {
        try
        {
            Debug.WriteLine($"Creating string contains expression for value: {value}");
            
            if (stringExpression == null)
            {
                Debug.WriteLine("String expression is null");
                return null;
            }
            
            // Use EF.Functions.Like for string comparison
            var efType = typeof(EF);
            var functionsProperty = efType.GetProperty("Functions");
            var functionsExpression = Expression.Property(null, functionsProperty);
            var likeMethod = functionsProperty.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
            var pattern = $"%{value}%";
            
            return Expression.Call(
                functionsExpression,
                likeMethod,
                stringExpression,
                Expression.Constant(pattern)
            );
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in CreateStringContainsExpression: {ex.Message}");
            return null;
        }
    }

    private static IEnumerable<dynamic> GetSearchCriteria(TFilter filter)
    {
        var searchCriteriaProperty = filter.GetType().GetProperty("SearchCriteria");
        if (searchCriteriaProperty == null) return null;

        var searchCriteriaValue = searchCriteriaProperty.GetValue(filter);
        if (searchCriteriaValue is IEnumerable<dynamic> enumerable)
        {
            return enumerable;
        }
        
        if (searchCriteriaValue is string searchCriteriaString && !string.IsNullOrEmpty(searchCriteriaString))
        {
            try
            {
                using var doc = JsonDocument.Parse(searchCriteriaString);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var criteriaList = new List<dynamic>();
                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        var criterionDict = new Dictionary<string, object>();
                        foreach (var property in element.EnumerateObject())
                        {
                            criterionDict[property.Name.ToLowerInvariant()] = property.Value.GetString();
                        }
                        
                        dynamic criterionObj = new System.Dynamic.ExpandoObject();
                        var criterionObjDict = (IDictionary<string, object>)criterionObj;
                        foreach (var kvp in criterionDict)
                        {
                            criterionObjDict[kvp.Key] = kvp.Value;
                        }
                        
                        criteriaList.Add(criterionObj);
                    }
                    return criteriaList;
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