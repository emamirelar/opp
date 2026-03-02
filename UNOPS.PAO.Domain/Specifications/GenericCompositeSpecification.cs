namespace UNOPS.PAO.Domain.Specifications;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type
#pragma warning disable CS8601 // Possible null reference assignment
#pragma warning disable CS8602 // Dereference of a possibly null reference
#pragma warning disable CS8603 // Possible null reference return
#pragma warning disable CS8604 // Possible null reference argument

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
        "PageIndex", "PageSize", "OrderBy", "Ascending", "Direction", "OrgUnitId" 
    };

    protected GenericCompositeSpecification(TFilter filter)
        : base(BuildExpression(filter))
    {
    }

    private static Expression<Func<TEntity, bool>> BuildExpression(TFilter filter)
    {
        if (filter == null)
        {
                return x => true;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var expressions = new List<Expression>();

        // Check if this is an advanced search
        bool isAdvancedSearch = filter.GetType().GetProperty("AdvancedSearch")?.GetValue(filter) as bool? ?? false;
        

        // Process advanced search criteria if available
        if (isAdvancedSearch)
        {
            var searchCriteria = GetSearchCriteria(filter);
            
            if (searchCriteria != null && searchCriteria.Any())
            {
                var criteriaWithOperators = new List<(Expression Expression, string LogicalOperator)>();
                
                foreach (var criteria in searchCriteria)
                {
                    try
                    {
                        
                        var criteriaDict = criteria as IDictionary<string, object>;
                        if (criteriaDict == null) 
                        {
                            continue;
                        }

                        string? field = criteriaDict.TryGetValue("field", out object? fieldObj) ? fieldObj?.ToString() : null;
                        string? value = criteriaDict.TryGetValue("value", out object? valueObj) ? valueObj?.ToString() : null;
                        string comparisonOperator = criteriaDict.TryGetValue("operator", out object? opObj) ? opObj?.ToString() ?? "like" : "like";
                        string? secondValue = criteriaDict.TryGetValue("secondValue", out object? secondValueObj) ? secondValueObj?.ToString() : null;
                        string logicalOperator = criteriaDict.TryGetValue("logicalOperator", out object? logicalOpObj) ? logicalOpObj?.ToString() ?? "AND" : "AND";


                        if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(value)) 
                        {
                            continue;
                        }

                        // Build property access and null checks
                        var propertyAccess = BuildPropertyAccess(parameter, field);
                        if (propertyAccess == null) 
                        {
                            continue;
                        }


                        // Create the comparison expression
                        Expression? comparisonExpr = null;
                        
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
                                            // Fallback to case-insensitive Contains using ToLower()
                                            var toLowerMethodFallback = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                                            var propertyToLowerFallback = Expression.Call(propertyAccess, toLowerMethodFallback);
                                            var valueToLowerFallback = value.ToLower();
                                            var containsMethodFallback = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                            var containsCallFallback = Expression.Call(propertyToLowerFallback, containsMethodFallback, Expression.Constant(valueToLowerFallback));
                                            comparisonExpr = Expression.AndAlso(nullCheck, containsCallFallback);
                                        }
                                    }
                                    catch
                                    {
                                        // Fallback to case-insensitive Contains using ToLower() if EF.Functions is not available
                                        var toLowerMethodLikeCatch = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                                        var propertyToLowerLikeCatch = Expression.Call(propertyAccess, toLowerMethodLikeCatch);
                                        var valueToLowerLikeCatch = value.ToLower();
                                        var containsMethodLikeCatch = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                        var containsCallLikeCatch = Expression.Call(propertyToLowerLikeCatch, containsMethodLikeCatch, Expression.Constant(valueToLowerLikeCatch));
                                        comparisonExpr = Expression.AndAlso(nullCheck, containsCallLikeCatch);
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
                                            // Fallback to case-insensitive Not Contains using ToLower()
                                            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                                            var propertyToLower = Expression.Call(propertyAccess, toLowerMethod);
                                            var valueToLower = value.ToLower();
                                            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                            var containsCall = Expression.Call(propertyToLower, containsMethod, Expression.Constant(valueToLower));
                                            comparisonExpr = Expression.AndAlso(nullCheck, Expression.Not(containsCall));
                                        }
                                    }
                                    catch
                                    {
                                        // Fallback to case-insensitive Not Contains using ToLower()
                                        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                                        var propertyToLower = Expression.Call(propertyAccess, toLowerMethod);
                                        var valueToLower = value.ToLower();
                                        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                        var containsCall = Expression.Call(propertyToLower, containsMethod, Expression.Constant(valueToLower));
                                        comparisonExpr = Expression.AndAlso(nullCheck, Expression.Not(containsCall));
                                    }
                                    break;
                                case "is":
                                    // Case-insensitive string equality using ToLower()
                                    var toLowerMethodIs = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                                    var propertyToLowerIs = Expression.Call(propertyAccess, toLowerMethodIs);
                                    var valueToLowerIs = value.ToLower();
                                    var equalsMethodIs = typeof(string).GetMethod("Equals", new[] { typeof(string) });
                                    comparisonExpr = Expression.Call(propertyToLowerIs, equalsMethodIs, Expression.Constant(valueToLowerIs));
                                    break;
                                case "is not":
                                    // Case-insensitive string inequality using ToLower()
                                    var toLowerMethodIsNot = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                                    var propertyToLowerIsNot = Expression.Call(propertyAccess, toLowerMethodIsNot);
                                    var valueToLowerIsNot = value.ToLower();
                                    var equalsMethodIsNot = typeof(string).GetMethod("Equals", new[] { typeof(string) });
                                    var equalsCallIsNot = Expression.Call(propertyToLowerIsNot, equalsMethodIsNot, Expression.Constant(valueToLowerIsNot));
                                    comparisonExpr = Expression.Not(equalsCallIsNot);
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
                                            // Fallback to case-insensitive Contains using ToLower()
                                            var toLowerMethodFallback = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                                            var propertyToLowerFallback = Expression.Call(propertyAccess, toLowerMethodFallback);
                                            var valueToLowerFallback = value.ToLower();
                                            var containsMethodFallback = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                            var containsCallFallback = Expression.Call(propertyToLowerFallback, containsMethodFallback, Expression.Constant(valueToLowerFallback));
                                            comparisonExpr = Expression.AndAlso(nullCheck, containsCallFallback);
                                        }
                                    }
                                    catch
                                    {
                                        // Fallback to case-insensitive Contains using ToLower()
                                        var toLowerMethodDefaultCatch = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                                        var propertyToLowerDefaultCatch = Expression.Call(propertyAccess, toLowerMethodDefaultCatch);
                                        var valueToLowerDefaultCatch = value.ToLower();
                                        var containsMethodDefaultCatch = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                        var containsCallDefaultCatch = Expression.Call(propertyToLowerDefaultCatch, containsMethodDefaultCatch, Expression.Constant(valueToLowerDefaultCatch));
                                        comparisonExpr = Expression.AndAlso(nullCheck, containsCallDefaultCatch);
                                    }
                                    break;
                            }
                        }
                        else if (propertyAccess.Type == typeof(DateTime) || propertyAccess.Type == typeof(DateTime?))
                        {
                            var parsedDate = ParseDateValue(value);
                            if (parsedDate.HasValue)
                            {
                                var dateValue = parsedDate.Value;
                                var constant = Expression.Constant(dateValue, propertyAccess.Type);

                                // Handle different date operators with enhanced logic
                                switch (comparisonOperator.ToLower())
                                {
                                    case ">":
                                    case "after":
                                        // For "after" dates, we typically want end of day comparison
                                        var afterDate = GetEndOfDay(dateValue);
                                        var afterConstant = Expression.Constant(afterDate, propertyAccess.Type);
                                        comparisonExpr = Expression.GreaterThan(propertyAccess, afterConstant);
                                        break;
                                    case ">=":
                                    case "after or equal":
                                        var afterEqualDate = GetStartOfDay(dateValue);
                                        var afterEqualConstant = Expression.Constant(afterEqualDate, propertyAccess.Type);
                                        comparisonExpr = Expression.GreaterThanOrEqual(propertyAccess, afterEqualConstant);
                                        break;
                                    case "<":
                                    case "before":
                                        // For "before" dates, we typically want start of day comparison
                                        var beforeDate = GetStartOfDay(dateValue);
                                        var beforeConstant = Expression.Constant(beforeDate, propertyAccess.Type);
                                        comparisonExpr = Expression.LessThan(propertyAccess, beforeConstant);
                                        break;
                                    case "<=":
                                    case "before or equal":
                                        var beforeEqualDate = GetEndOfDay(dateValue);
                                        var beforeEqualConstant = Expression.Constant(beforeEqualDate, propertyAccess.Type);
                                        comparisonExpr = Expression.LessThanOrEqual(propertyAccess, beforeEqualConstant);
                                        break;
                                    case "between":
                                        var secondParsedDate = ParseDateValue(secondValue ?? string.Empty);
                                        if (secondParsedDate.HasValue)
                                        {
                                            var startDate = GetStartOfDay(dateValue);
                                            var endDate = GetEndOfDay(secondParsedDate.Value);
                                            
                                            var startConstant = Expression.Constant(startDate, propertyAccess.Type);
                                            var endConstant = Expression.Constant(endDate, propertyAccess.Type);
                                            
                                            var startComparison = Expression.GreaterThanOrEqual(propertyAccess, startConstant);
                                            var endComparison = Expression.LessThanOrEqual(propertyAccess, endConstant);
                                            comparisonExpr = Expression.AndAlso(startComparison, endComparison);
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        break;
                                    case "is":
                                    case "on":
                                        // For exact date matches, compare the entire day
                                        var onStartDate = GetStartOfDay(dateValue);
                                        var onEndDate = GetEndOfDay(dateValue);
                                        
                                        var onStartConstant = Expression.Constant(onStartDate, propertyAccess.Type);
                                        var onEndConstant = Expression.Constant(onEndDate, propertyAccess.Type);
                                        
                                        var onStartComparison = Expression.GreaterThanOrEqual(propertyAccess, onStartConstant);
                                        var onEndComparison = Expression.LessThanOrEqual(propertyAccess, onEndConstant);
                                        comparisonExpr = Expression.AndAlso(onStartComparison, onEndComparison);
                                        break;
                                    case "is not":
                                    case "not on":
                                        // For "not on" date, exclude the entire day
                                        var notOnStartDate = GetStartOfDay(dateValue);
                                        var notOnEndDate = GetEndOfDay(dateValue);
                                        
                                        var notOnStartConstant = Expression.Constant(notOnStartDate, propertyAccess.Type);
                                        var notOnEndConstant = Expression.Constant(notOnEndDate, propertyAccess.Type);
                                        
                                        var notOnStartComparison = Expression.LessThan(propertyAccess, notOnStartConstant);
                                        var notOnEndComparison = Expression.GreaterThan(propertyAccess, notOnEndConstant);
                                        comparisonExpr = Expression.OrElse(notOnStartComparison, notOnEndComparison);
                                        break;
                                    case "this week":
                                        var weekDates = GetThisWeekDates();
                                        var weekStartConstant = Expression.Constant(weekDates.Start, propertyAccess.Type);
                                        var weekEndConstant = Expression.Constant(weekDates.End, propertyAccess.Type);
                                        var weekStartComparison = Expression.GreaterThanOrEqual(propertyAccess, weekStartConstant);
                                        var weekEndComparison = Expression.LessThanOrEqual(propertyAccess, weekEndConstant);
                                        comparisonExpr = Expression.AndAlso(weekStartComparison, weekEndComparison);
                                        break;
                                    case "this month":
                                        var monthDates = GetThisMonthDates();
                                        var monthStartConstant = Expression.Constant(monthDates.Start, propertyAccess.Type);
                                        var monthEndConstant = Expression.Constant(monthDates.End, propertyAccess.Type);
                                        var monthStartComparison = Expression.GreaterThanOrEqual(propertyAccess, monthStartConstant);
                                        var monthEndComparison = Expression.LessThanOrEqual(propertyAccess, monthEndConstant);
                                        comparisonExpr = Expression.AndAlso(monthStartComparison, monthEndComparison);
                                        break;
                                    case "this year":
                                        var yearDates = GetThisYearDates();
                                        var yearStartConstant = Expression.Constant(yearDates.Start, propertyAccess.Type);
                                        var yearEndConstant = Expression.Constant(yearDates.End, propertyAccess.Type);
                                        var yearStartComparison = Expression.GreaterThanOrEqual(propertyAccess, yearStartConstant);
                                        var yearEndComparison = Expression.LessThanOrEqual(propertyAccess, yearEndConstant);
                                        comparisonExpr = Expression.AndAlso(yearStartComparison, yearEndComparison);
                                        break;
                                    default:
                                        // Default to exact day match
                                        var defaultStartDate = GetStartOfDay(dateValue);
                                        var defaultEndDate = GetEndOfDay(dateValue);
                                        var defaultStartConstant = Expression.Constant(defaultStartDate, propertyAccess.Type);
                                        var defaultEndConstant = Expression.Constant(defaultEndDate, propertyAccess.Type);
                                        var defaultStartComparison = Expression.GreaterThanOrEqual(propertyAccess, defaultStartConstant);
                                        var defaultEndComparison = Expression.LessThanOrEqual(propertyAccess, defaultEndConstant);
                                        comparisonExpr = Expression.AndAlso(defaultStartComparison, defaultEndComparison);
                                        break;
                                }
                            }
                            else
                            {
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
                            catch (Exception)
                            {
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
                                continue;
                            }
                        }
                        else
                        {
                            comparisonExpr = Expression.Equal(propertyAccess, Expression.Constant(value));
                        }

                        if (comparisonExpr != null)
                        {
                            // Store both the expression and its logical operator for later combination
                            criteriaWithOperators.Add((comparisonExpr, logicalOperator));
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                // Combine criteria expressions based on logical operators
                if (criteriaWithOperators.Any())
                {
                    Expression advancedSearchExpression = CombineExpressionsWithLogicalOperators(criteriaWithOperators);
                    expressions.Add(advancedSearchExpression);
                }
            }
            else
            {
            }
        }

        // Process regular properties (these should be combined with AND)
        var allProperties = typeof(TFilter).GetProperties();
        
        var validProperties = allProperties
            .Where(p => !ShouldIgnoreProperty(p) && p.Name != "SearchText" && 
                   p.Name != "AdvancedSearch" && p.Name != "SearchCriteria");
        
        var propertiesWithValues = validProperties
            .Select(p => new { Property = p, Value = p.GetValue(filter) })
            .ToList();
        
        var nonEmptyProperties = propertiesWithValues
            .Where(x => x.Value != null && !string.IsNullOrEmpty(x.Value.ToString()));
            
        foreach (var x in nonEmptyProperties)
        {
            var expr = CreatePropertyExpression(parameter, x.Property, x.Value);
            if (expr != null)
            {
                expressions.Add(expr.Body);
            }
        }

        // Handle search text (this should be combined with AND)
        if (filter is ISearchFilter searchFilter && !string.IsNullOrWhiteSpace(searchFilter.SearchText))
        {
            var searchExpression = CreateSearchTextExpression(parameter, searchFilter.SearchText);
            if (searchExpression != null)
            {
                expressions.Add(searchExpression.Body);
            }
        }

        // Handle MyOfficeOnly filter - but cannot implement here due to lack of user context
        // This needs to be handled in concrete specifications or at a higher level

        if (!expressions.Any())
        {
            return x => true;
        }


        // Combine all expressions with AND
        Expression finalExpression = expressions[0];
        for (int i = 1; i < expressions.Count; i++)
        {
            finalExpression = Expression.AndAlso(finalExpression, expressions[i]);
        }

        return Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);
    }

    private static Expression<Func<TEntity, bool>>? CreateSearchTextExpression(ParameterExpression parameter, string searchText)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return null;
            }

            var searchTermValue = NormalizeSearchText(searchText);
            if (string.IsNullOrEmpty(searchTermValue))
            {
                return null;
            }
            

            var searchableProperties = GetSearchableProperties(typeof(TEntity));

            if (!searchableProperties.Any())
            {
                return null;
            }

            // Déterminer le mode de recherche (phrase exacte ou mots multiples)
            var searchMode = DetermineSearchMode(searchTermValue);
            var searchTerms = GetSearchTerms(searchTermValue, searchMode);
            

            var propertyExpressions = new List<Expression>();

            foreach (var propertyPath in searchableProperties)
            {
                try
                {
                    var propertyAccess = BuildPropertyAccess(parameter, propertyPath);
                    if (propertyAccess == null)
                    {
                        continue;
                    }

                    var nullChecks = BuildNullChecks(parameter, propertyPath);

                    // Créer l'expression de recherche selon le mode
                    Expression searchExpression = CreateSearchExpression(propertyAccess, searchTerms, searchMode);

                    if (searchExpression != null)
                    {
                        // Combine null checks with search expression
                        var finalExpression = nullChecks.Aggregate(
                            searchExpression,
                            (current, nullCheck) => Expression.AndAlso(nullCheck, current)
                        );

                        propertyExpressions.Add(finalExpression);
                    }
                }
                catch (Exception)
                {
                }
            }

            if (!propertyExpressions.Any())
            {
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
        catch (Exception)
        {
            return null;
        }
    }

    private static Expression? BuildPropertyAccess(ParameterExpression parameter, string propertyPath)
    {
        try
        {
            
            // Convert camelCase to PascalCase if needed
            var pascalCaseField = ConvertToPascalCase(propertyPath);
            
            var parts = pascalCaseField.Split('.');
            Expression expression = parameter;
            
            foreach (var part in parts)
            {
                var currentType = expression.Type;
                
                // Try declared only first to avoid ambiguous matches
                var property = currentType.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly);
                
                // If not found in declared members, try base type
                if (property == null)
                {
                    property = currentType.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                }
                
                if (property == null)
                {
                    return null;
                }
                
                expression = Expression.Property(expression, property);
            }
            
            return expression;
        }
        catch (Exception)
        {
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
            
            // Try declared only first to avoid ambiguous matches
            var property = currentType.GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly);
            
            // If not found in declared members, try base type
            if (property == null)
            {
                property = currentType.GetProperty(parts[i], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            }
            
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

            return searchableProperties;
        }

        // Define core searchable properties for Partner - don't include navigation properties
        if (type.Name == "Partner" || type.Name == "UNOPSPartner")
        {
            searchableProperties.AddRange(new[]
            {
                "Name",
                "PartnerShortDescription", // Updated from ShortName
                "PartnerLongDescription",  // Added for long description
                "Status",
                "PartnerGroupCode"         // Updated from PartnerCode
            });

            return searchableProperties;
        }

        // Define core searchable properties for Interaction - exclude NotMapped computed properties
        if (type.Name == "Interaction" || type.Name == "UNOPSInteraction")
        {
            searchableProperties.AddRange(new[]
            {
                "Description",
                "Location", 
                "Subject",
                "GmailThreadId",
                "GmailMessageId",
                "Name"
                // Explicitly exclude: InteractionContactsList, InteractionPartnersList, 
                // InteractionUsersList, InteractionOrgUnits (these are [NotMapped] computed properties)
            });

            return searchableProperties;
        }

        // For other types, use reflection to find string properties - but don't traverse navigation properties
        foreach (var property in type.GetProperties())
        {
            var propertyPath = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            if (property.PropertyType == typeof(string))
            {
                searchableProperties.Add(propertyPath);
            }
            // Removed the traversal into navigation properties to avoid null reference issues
        }

        return searchableProperties;
    }

    private static Expression<Func<TEntity, bool>>? CreatePropertyExpression(
        ParameterExpression parameter,
        PropertyInfo property,
        object? value)
    {
        try
        {
            if (property.Name == "SearchText")
                return null;
            
            // Special handling for PartnerId in Interaction entities
            if (property.Name == "PartnerId" && typeof(TEntity).Name == "Interaction" && value != null)
            {
                return CreatePartnerIdExpression(parameter, value);
            }

            // Special handling for ContactId in Interaction entities
            if (property.Name == "ContactId" && typeof(TEntity).Name == "Interaction" && value != null)
            {
                return CreateContactIdExpression(parameter, value);
            }

            // Get property path (for nested properties)
            var propertyPath = GetPropertyPath(property.Name);

            Expression propertyAccess = parameter;

            // Build the property access chain
            foreach (var pathPart in propertyPath)
            {
                var currentType = propertyAccess.Type;
                var currentProperty = currentType.GetProperty(pathPart, 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly);
                
                // If not found in declared members, try base type
                if (currentProperty == null)
                {
                    currentProperty = currentType.GetProperty(pathPart, 
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                }
                
                if (currentProperty == null)
                {
                    return null;
                }

                propertyAccess = Expression.Property(propertyAccess, currentProperty);
            }

            // Create the comparison based on the property type
            Expression comparison;
            if (propertyAccess.Type == typeof(string))
            {
                var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));
                
                // Properties that should use exact matching
                var exactMatchProperties = new[] { "Status", "NewEngagement", "PooledFund", "DDRequired", 
                                                   "DDEACDone", "LevyPotentiallyApplies", "PartnerGroupCode" };
                
                Expression stringComparisonExpr;
                
                if (exactMatchProperties.Contains(property.Name))
                {
                    // Use exact matching for certain properties
                    stringComparisonExpr = Expression.Equal(propertyAccess, Expression.Constant(value.ToString()));
                }
                else
                {
                    // Use Contains/LIKE pattern matching for other properties
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
                            // Fallback to case-insensitive Contains using ToLower()
                            var toLowerMethodProp = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                            var propertyToLowerProp = Expression.Call(propertyAccess, toLowerMethodProp);
                            var valueToLowerProp = value.ToString().ToLower();
                            var containsMethodProp = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                            stringComparisonExpr = Expression.Call(propertyToLowerProp, containsMethodProp, Expression.Constant(valueToLowerProp));
                        }
                    }
                    catch
                    {
                        // Fallback to case-insensitive Contains using ToLower() if EF.Functions is not available
                        var toLowerMethodCatch = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                        var propertyToLowerCatch = Expression.Call(propertyAccess, toLowerMethodCatch);
                        var valueToLowerCatch = value.ToString().ToLower();
                        var containsMethodCatch = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        stringComparisonExpr = Expression.Call(propertyToLowerCatch, containsMethodCatch, Expression.Constant(valueToLowerCatch));
                    }
                }
                
                comparison = Expression.AndAlso(nullCheck, stringComparisonExpr);
            }
            else
            {
                // For non-string types, use equality comparison with type conversion
                object convertedValue = value;
                
                // Convert the value to the target type if needed
                if (value != null && propertyAccess.Type != value.GetType())
                {
                    try
                    {
                        convertedValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(propertyAccess.Type) ?? propertyAccess.Type);
                    }
                    catch
                    {
                        // If conversion fails, use the original value
                    }
                }
                
                comparison = Expression.Equal(propertyAccess, Expression.Constant(convertedValue, propertyAccess.Type));
            }

            return Expression.Lambda<Func<TEntity, bool>>(comparison, parameter);
        }
        catch (Exception)
        {
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
            return null;
        }

        var searchCriteriaValue = searchCriteriaProperty.GetValue(filter);
        
        if (searchCriteriaValue is string searchCriteriaString && !string.IsNullOrEmpty(searchCriteriaString))
        {
            try
            {
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
                    
                    return criteriaList.Cast<dynamic>();
                }
            }
            catch (Exception)
            {
            }
        }
        
        return null;
    }

    private static Expression<Func<TEntity, bool>> CreatePartnerIdExpression(
        ParameterExpression parameter,
        object value)
    {
        try
        {
            if (!int.TryParse(value.ToString(), out int partnerId))
            {
                Debug.WriteLine($"Failed to parse PartnerId value: {value}");
                return null;
            }

            Debug.WriteLine($"Creating PartnerId expression for value: {partnerId}");

            // Build expression: x => x.InteractionPartners.Any(ip => ip.PartnerId == partnerId)
            var interactionPartnersProperty = typeof(TEntity).GetProperty("InteractionPartners");
            if (interactionPartnersProperty == null)
            {
                Debug.WriteLine("InteractionPartners property not found on entity");
                return null;
            }

            var interactionPartnersAccess = Expression.Property(parameter, interactionPartnersProperty);
            
            // Create parameter for the Any() lambda: ip => ip.PartnerId == partnerId
            var junctionParameter = Expression.Parameter(interactionPartnersProperty.PropertyType.GetGenericArguments()[0], "ip");
            var partnerIdProperty = junctionParameter.Type.GetProperty("PartnerId");
            
            if (partnerIdProperty == null)
            {
                Debug.WriteLine("PartnerId property not found on junction table");
                return null;
            }

            var partnerIdAccess = Expression.Property(junctionParameter, partnerIdProperty);
            var partnerIdConstant = Expression.Constant(partnerId);
            var partnerIdEquality = Expression.Equal(partnerIdAccess, partnerIdConstant);
            
            var lambdaExpression = Expression.Lambda(partnerIdEquality, junctionParameter);
            
            // Get the Any method for ICollection<InteractionPartner>
            var enumerableType = typeof(System.Linq.Enumerable);
            var anyMethod = enumerableType.GetMethods()
                .Where(m => m.Name == "Any" && m.GetParameters().Length == 2)
                .First()
                .MakeGenericMethod(junctionParameter.Type);
            
            // Create the Any() call
            var anyCall = Expression.Call(anyMethod, interactionPartnersAccess, lambdaExpression);
            
            Debug.WriteLine("Successfully created PartnerId expression using Any()");
            return Expression.Lambda<Func<TEntity, bool>>(anyCall, parameter);
        }
        catch (Exception)
        {
            Debug.WriteLine("Error creating PartnerId expression");
            return null;
        }
    }

    private static Expression<Func<TEntity, bool>> CreateContactIdExpression(
        ParameterExpression parameter,
        object value)
    {
        try
        {
            if (!int.TryParse(value.ToString(), out int contactId))
            {
                Debug.WriteLine($"Failed to parse ContactId value: {value}");
                return null;
            }

            Debug.WriteLine($"Creating ContactId expression for value: {contactId}");

            // Build expression: x => x.InteractionContacts.Any(ic => ic.ContactId == contactId)
            var interactionContactsProperty = typeof(TEntity).GetProperty("InteractionContacts");
            if (interactionContactsProperty == null)
            {
                Debug.WriteLine("InteractionContacts property not found on entity");
                return null;
            }

            var interactionContactsAccess = Expression.Property(parameter, interactionContactsProperty);
            
            // Create parameter for the Any() lambda: ic => ic.ContactId == contactId
            var junctionParameter = Expression.Parameter(interactionContactsProperty.PropertyType.GetGenericArguments()[0], "ic");
            var contactIdProperty = junctionParameter.Type.GetProperty("ContactId");
            
            if (contactIdProperty == null)
            {
                Debug.WriteLine("ContactId property not found on junction table");
                return null;
            }

            var contactIdAccess = Expression.Property(junctionParameter, contactIdProperty);
            var contactIdConstant = Expression.Constant(contactId);
            var contactIdEquality = Expression.Equal(contactIdAccess, contactIdConstant);
            
            var lambdaExpression = Expression.Lambda(contactIdEquality, junctionParameter);
            
            // Get the Any method for ICollection<InteractionContact>
            var enumerableType = typeof(System.Linq.Enumerable);
            var anyMethod = enumerableType.GetMethods()
                .Where(m => m.Name == "Any" && m.GetParameters().Length == 2)
                .First()
                .MakeGenericMethod(junctionParameter.Type);
            
            // Create the Any() call
            var anyCall = Expression.Call(anyMethod, interactionContactsAccess, lambdaExpression);
            
            Debug.WriteLine("Successfully created ContactId expression using Any()");
            return Expression.Lambda<Func<TEntity, bool>>(anyCall, parameter);
        }
        catch (Exception)
        {
            Debug.WriteLine("Error creating ContactId expression");
            return null;
        }
    }

    /// <summary>
    /// Combines expressions with their logical operators (OR/AND) respecting precedence and grouping
    /// </summary>
    private static Expression? CombineExpressionsWithLogicalOperators(List<(Expression Expression, string LogicalOperator)> criteriaWithOperators)
    {
        if (!criteriaWithOperators.Any())
            return null;

        if (criteriaWithOperators.Count == 1)
            return criteriaWithOperators[0].Expression;


        // Start with the first expression
        Expression result = criteriaWithOperators[0].Expression;
        
        // Process each subsequent expression with its logical operator
        for (int i = 1; i < criteriaWithOperators.Count; i++)
        {
            var currentExpression = criteriaWithOperators[i].Expression;
            var currentLogicalOperator = criteriaWithOperators[i].LogicalOperator;
            
            
            if (string.Equals(currentLogicalOperator, "OR", StringComparison.OrdinalIgnoreCase))
            {
                result = Expression.OrElse(result, currentExpression);
            }
            else // Default to AND
            {
                result = Expression.AndAlso(result, currentExpression);
            }
        }

        return result;
    }

    /// <summary>
    /// Normalise le texte de recherche en supprimant les espaces en trop et les caractères de contrôle
    /// </summary>
    private static string NormalizeSearchText(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return string.Empty;

        // Remplacer tous les caractères blancs (espaces, tabs, nouvelle ligne) par des espaces normaux
        var normalized = System.Text.RegularExpressions.Regex.Replace(searchText, @"\s+", " ");
        
        // Trim les espaces en début et fin
        return normalized.Trim();
    }

    /// <summary>
    /// Détermine le mode de recherche basé sur le contenu du texte
    /// </summary>
    private static SearchTextMode DetermineSearchMode(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
            return SearchTextMode.ExactPhrase;

        // Si le texte est entouré de guillemets, c'est une recherche de phrase exacte
        if (searchText.StartsWith("\"") && searchText.EndsWith("\"") && searchText.Length > 2)
            return SearchTextMode.ExactPhrase;

        // Si le texte contient plusieurs mots, utiliser la recherche par mots multiples
        if (searchText.Contains('|'))
            return SearchTextMode.MultipleWords;

        // Sinon, recherche de phrase exacte (mot unique)
        return SearchTextMode.ExactPhrase;
    }

    /// <summary>
    /// Extrait les termes de recherche selon le mode spécifié
    /// </summary>
    private static string[] GetSearchTerms(string searchText, SearchTextMode mode)
    {
        if (string.IsNullOrEmpty(searchText))
            return Array.Empty<string>();

        switch (mode)
        {
            case SearchTextMode.ExactPhrase:
                // Pour une phrase exacte, retourner le texte tel quel (sans guillemets si présents)
                var cleanText = searchText.Trim('"');
                return new[] { cleanText };

            case SearchTextMode.MultipleWords:
                // Diviser en mots individuels et filtrer les termes trop courts
                return searchText
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(word => word.Length >= 2) // Ignorer les mots d'une lettre
                    .ToArray();

            default:
                return new[] { searchText };
        }
    }

    /// <summary>
    /// Crée l'expression de recherche pour une propriété selon le mode spécifié
    /// </summary>
    private static Expression CreateSearchExpression(Expression propertyAccess, string[] searchTerms, SearchTextMode mode)
    {
        if (!searchTerms.Any())
            return null;

        switch (mode)
        {
            case SearchTextMode.ExactPhrase:
                // Recherche de phrase exacte
                return CreateStringContainsExpression(propertyAccess, searchTerms[0]);

            case SearchTextMode.MultipleWords:
                // Recherche par mots multiples (OR)
                var wordExpressions = searchTerms
                    .Select(term => CreateStringContainsExpression(propertyAccess, term))
                    .Where(expr => expr != null)
                    .ToArray();

                if (!wordExpressions.Any())
                    return null;

                // Combiner avec OR
                Expression result = wordExpressions[0];
                for (int i = 1; i < wordExpressions.Length; i++)
                {
                    result = Expression.OrElse(result, wordExpressions[i]);
                }
                return result;

            default:
                return CreateStringContainsExpression(propertyAccess, searchTerms[0]);
        }
    }

    /// <summary>
    /// Crée une expression Contains pour une propriété string
    /// </summary>
    private static Expression CreateStringContainsExpression(Expression propertyAccess, string searchTerm)
    {
        // Add null check for the property
        var nullCheck = Expression.NotEqual(propertyAccess, Expression.Constant(null));
        
        try
        {
            // Try to use EF.Functions.Like for case-insensitive search
            var efType = typeof(EF);
            var functionsProperty = efType.GetProperty("Functions");
            var functionsExpression = Expression.Property(null, functionsProperty);
            var likeMethod = functionsProperty.PropertyType.GetMethod("Like", new[] { typeof(string), typeof(string) });
            
            if (likeMethod != null)
            {
                var pattern = $"%{searchTerm}%";
                var likeExpression = Expression.Call(functionsExpression, likeMethod, propertyAccess, Expression.Constant(pattern));
                // Combine null check with the LIKE expression
                return Expression.AndAlso(nullCheck, likeExpression);
            }
            else
            {
                // Fallback to case-insensitive Contains using ToLower()
                var toLowerMethodString = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
                var propertyToLowerString = Expression.Call(propertyAccess, toLowerMethodString);
                var searchTermToLower = searchTerm.ToLower();
                var containsMethodString = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                var containsExpression = Expression.Call(propertyToLowerString, containsMethodString, Expression.Constant(searchTermToLower));
                // Combine null check with the contains expression
                return Expression.AndAlso(nullCheck, containsExpression);
            }
        }
        catch
        {
            // Fallback to case-insensitive Contains using ToLower() if EF.Functions is not available
            var toLowerMethodFinalCatch = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var propertyToLowerFinalCatch = Expression.Call(propertyAccess, toLowerMethodFinalCatch);
            var searchTermToLowerFinalCatch = searchTerm.ToLower();
            var containsMethodFinalCatch = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var containsExpressionFallback = Expression.Call(propertyToLowerFinalCatch, containsMethodFinalCatch, Expression.Constant(searchTermToLowerFinalCatch));
            // Combine null check with the contains expression
            return Expression.AndAlso(nullCheck, containsExpressionFallback);
        }
    }

    /// <summary>
    /// Modes de recherche textuelle
    /// </summary>
    private enum SearchTextMode
    {
        /// <summary>
        /// Recherche de phrase exacte (comportement par défaut)
        /// </summary>
        ExactPhrase,
        
        /// <summary>
        /// Recherche par mots multiples (OR entre les mots)
        /// </summary>
        MultipleWords
    }

    #region Date Helper Methods

    /// <summary>
    /// Parse une valeur de date avec support de formats multiples et dates relatives
    /// </summary>
    private static DateTime? ParseDateValue(string dateValue)
    {
        if (string.IsNullOrWhiteSpace(dateValue))
            return null;

        var normalizedValue = dateValue.Trim().ToLower();

        // Gestion des dates relatives en UTC
        switch (normalizedValue)
        {
            case "today":
            case "aujourd'hui":
                return DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            case "yesterday":
            case "hier":
                return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-1), DateTimeKind.Utc);
            case "tomorrow":
            case "demain":
                return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(1), DateTimeKind.Utc);
            case "last week":
            case "semaine dernière":
                return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-7), DateTimeKind.Utc);
            case "next week":
            case "semaine prochaine":
                return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(7), DateTimeKind.Utc);
            case "last month":
            case "mois dernier":
                return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddMonths(-1), DateTimeKind.Utc);
            case "next month":
            case "mois prochain":
                return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddMonths(1), DateTimeKind.Utc);
            case "last year":
            case "année dernière":
                return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddYears(-1), DateTimeKind.Utc);
            case "next year":
            case "année prochaine":
                return DateTime.SpecifyKind(DateTime.UtcNow.Date.AddYears(1), DateTimeKind.Utc);
        }

        // Gestion des formats de dates standards
        var formats = new[]
        {
            "yyyy-MM-dd",           // ISO 8601
            "yyyy/MM/dd",           // Alternative slash
            "dd/MM/yyyy",           // European format
            "MM/dd/yyyy",           // American format
            "dd-MM-yyyy",           // European dash
            "MM-dd-yyyy",           // American dash
            "yyyy-MM-dd HH:mm:ss",  // ISO with time
            "yyyy-MM-dd HH:mm",     // ISO with time (no seconds)
            "dd/MM/yyyy HH:mm:ss",  // European with time
            "dd/MM/yyyy HH:mm",     // European with time (no seconds)
            "yyyy-MM-ddTHH:mm:ss",  // ISO 8601 full
            "yyyy-MM-ddTHH:mm:ssZ", // ISO 8601 with Z
        };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateValue, format, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, out var result))
            {
                    return DateTime.SpecifyKind(result, DateTimeKind.Utc);
            }
        }

        // Fallback vers DateTime.TryParse pour formats automatiques
        if (DateTime.TryParse(dateValue, out var fallbackResult))
        {
            return DateTime.SpecifyKind(fallbackResult, DateTimeKind.Utc);
        }

        return null;
    }

    /// <summary>
    /// Obtient le début de la journée (00:00:00) en UTC
    /// </summary>
    private static DateTime GetStartOfDay(DateTime date)
    {
        var startOfDay = date.Date;
        return DateTime.SpecifyKind(startOfDay, DateTimeKind.Utc);
    }

    /// <summary>
    /// Obtient la fin de la journée (23:59:59.999) en UTC
    /// </summary>
    private static DateTime GetEndOfDay(DateTime date)
    {
        var endOfDay = date.Date.AddDays(1).AddMilliseconds(-1);
        return DateTime.SpecifyKind(endOfDay, DateTimeKind.Utc);
    }

    /// <summary>
    /// Obtient les dates de début et fin de la semaine actuelle (Lundi à Dimanche) en UTC
    /// </summary>
    private static (DateTime Start, DateTime End) GetThisWeekDates()
    {
        var today = DateTime.UtcNow.Date;
        var dayOfWeek = (int)today.DayOfWeek;
        
        // Considère Lundi comme le premier jour de la semaine
        var mondayOffset = dayOfWeek == 0 ? -6 : -(dayOfWeek - 1);
        var monday = today.AddDays(mondayOffset);
        var sunday = monday.AddDays(6);
        
        return (GetStartOfDay(monday), GetEndOfDay(sunday));
    }

    /// <summary>
    /// Obtient les dates de début et fin du mois actuel en UTC
    /// </summary>
    private static (DateTime Start, DateTime End) GetThisMonthDates()
    {
        var today = DateTime.UtcNow.Date;
        var firstDay = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        
        return (GetStartOfDay(firstDay), GetEndOfDay(lastDay));
    }

    /// <summary>
    /// Obtient les dates de début et fin de l'année actuelle en UTC
    /// </summary>
    private static (DateTime Start, DateTime End) GetThisYearDates()
    {
        var today = DateTime.UtcNow.Date;
        var firstDay = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastDay = new DateTime(today.Year, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        
        return (GetStartOfDay(firstDay), GetEndOfDay(lastDay));
    }

    #endregion
}

#pragma warning restore CS8600
#pragma warning restore CS8601
#pragma warning restore CS8602
#pragma warning restore CS8603
#pragma warning restore CS8604 