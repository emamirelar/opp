using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Authorization;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.UNOPSBusiness.Services;

public interface IGenericRowFilterService
{
    Task<IQueryable<T>> ApplyRowFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action = "read") where T : class;
    Task<bool> CanUserAccessEntityAsync<T>(T entity, ClaimsPrincipal user, string action = "read") where T : class;
}

public class GenericRowFilterService : IGenericRowFilterService
{
    private readonly IDbContextFactory<UNOPSAppDbContext> _contextFactory;
    private readonly ILogger<GenericRowFilterService> _logger;
    
    // Security configuration constants
    private static readonly ParsingConfig SecureParsingConfig = new ParsingConfig
    {
        IsCaseSensitive = true,
        UseParameterizedNamesInDynamicQuery = false,
        AllowNewToEvaluateAnyType = false,
        // Additional security settings
        ResolveTypesBySimpleName = false,
        EvaluateGroupByAtDatabase = true
    };
    
    // Maximum expression length to prevent DoS attacks
    private const int MAX_EXPRESSION_LENGTH = 1000;
    
    // Maximum number of nested parentheses to prevent stack overflow
    private const int MAX_NESTING_DEPTH = 10;

    public GenericRowFilterService(IDbContextFactory<UNOPSAppDbContext> contextFactory, ILogger<GenericRowFilterService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IQueryable<T>> ApplyRowFiltersAsync<T>(IQueryable<T> query, ClaimsPrincipal user, string action = "read") where T : class
    {
        _logger.LogDebug("Applying row filters for entity {EntityName}, action {Action}", typeof(T).Name, action);
        
        // Get entity name
        var entityName = GetEntityName<T>();
        
        // Get user roles
        var userRoles = GetUserRoles(user);
        
        if (!userRoles.Any())
        {
            _logger.LogWarning("No roles found for user, denying access");
            return query.Where(x => false); // Return empty query
        }

        // Get permissions for this entity and user's roles using a new context
        using var context = await _contextFactory.CreateDbContextAsync();
        var permissions = await context.EntityPermissions
            .Where(ep => ep.Entity == entityName && userRoles.Contains(ep.Role))
            .ToListAsync();

        if (!permissions.Any())
        {
            _logger.LogDebug("No permissions found for entity {EntityName} and roles {Roles}", entityName, string.Join(", ", userRoles));
            return query.Where(x => false); // Return empty query if no permissions defined
        }

        // Check if user has boolean permission for this action first
        var hasBasicPermission = permissions.Any(p => action.ToLower() switch
        {
            "read" => p.CanRead,
            "create" => p.CanCreate,
            "update" => p.CanUpdate,
            "delete" => p.CanDelete,
            _ => false
        });

        if (!hasBasicPermission)
        {
            _logger.LogDebug("User does not have basic {Action} permission for entity {EntityName}", action, entityName);
            return query.Where(x => false); // Return empty query if no basic permission
        }

        // Get permissions that have basic permission for this action
        var relevantPermissions = permissions.Where(p => action.ToLower() switch
        {
            "read" => p.CanRead,
            "create" => p.CanCreate,
            "update" => p.CanUpdate,
            "delete" => p.CanDelete,
            _ => false
        }).ToList();

        // Check if any relevant permissions have specific action filters
        var permissionsWithFilters = new List<EntityPermission>();
        
        foreach (var permission in relevantPermissions)
        {
            if (string.IsNullOrEmpty(permission.RowFilter)) continue;
            
            try
            {
                var rowFilterConditions = JsonSerializer.Deserialize<RowFilterConditions>(permission.RowFilter);
                if (rowFilterConditions == null) continue;

                var actionFilter = action.ToLower() switch
                {
                    "read" => rowFilterConditions.CanRead,
                    "create" => rowFilterConditions.CanCreate,
                    "update" => rowFilterConditions.CanUpdate,
                    "delete" => rowFilterConditions.CanDelete,
                    _ => null
                };

                // Only include permissions where the specific action has a non-empty filter
                if (!string.IsNullOrWhiteSpace(actionFilter))
                {
                    permissionsWithFilters.Add(permission);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in RowFilter for permission {PermissionId}: {RowFilter}", permission.Id, permission.RowFilter);
            }
        }

        if (!permissionsWithFilters.Any())
        {
            _logger.LogDebug("No row filters defined for entity {EntityName} and action {Action}, allowing full access", entityName, action);
            return query; // No filters = no restrictions (full access)
        }

        // Build combined filter expression
        var combinedFilter = await BuildCombinedFilterExpression<T>(permissionsWithFilters, action, user);
        
        if (!string.IsNullOrWhiteSpace(combinedFilter))
        {
            try
            {
                _logger.LogDebug("Applying filter: {Filter}", combinedFilter);
                
                // Apply the filter with secure configuration
                return query.Where(SecureParsingConfig, combinedFilter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying row filter: {Filter}", combinedFilter);
                return query.Where(x => false); // Return empty query on error
            }
        }

        return query;
    }

    public async Task<bool> CanUserAccessEntityAsync<T>(T entity, ClaimsPrincipal user, string action = "read") where T : class
    {
        if (entity == null) return false;

        // Get entity name and user roles
        var entityName = GetEntityName<T>();
        var userRoles = GetUserRoles(user);
        
        if (!userRoles.Any())
        {
            return false;
        }

        // Get permissions for this entity and user's roles using a new context
        using var context = await _contextFactory.CreateDbContextAsync();
        var permissions = await context.EntityPermissions
            .Where(ep => ep.Entity == entityName && userRoles.Contains(ep.Role))
            .ToListAsync();

        if (!permissions.Any())
        {
            return false; // No permissions defined
        }

        // Check if user has boolean permission for this action first
        var hasBasicPermission = permissions.Any(p => action.ToLower() switch
        {
            "read" => p.CanRead,
            "create" => p.CanCreate,
            "update" => p.CanUpdate,
            "delete" => p.CanDelete,
            _ => false
        });

        if (!hasBasicPermission)
        {
            return false; // No basic permission
        }

        // Get permissions that have basic permission for this action
        var relevantPermissions = permissions.Where(p => action.ToLower() switch
        {
            "read" => p.CanRead,
            "create" => p.CanCreate,
            "update" => p.CanUpdate,
            "delete" => p.CanDelete,
            _ => false
        }).ToList();

        // Check if any relevant permissions have row filters
        var permissionsWithFilters = relevantPermissions.Where(p => !string.IsNullOrEmpty(p.RowFilter)).ToList();
        
        if (!permissionsWithFilters.Any())
        {
            return true; // Basic permission exists and no row filters = full access
        }

        // Apply row filters to single entity
        var singleItemQuery = new[] { entity }.AsQueryable();
        var filteredQuery = await ApplyRowFiltersAsync(singleItemQuery, user, action);
        
        return filteredQuery.Any();
    }

    private async Task<string> BuildCombinedFilterExpression<T>(List<EntityPermission> permissions, string action, ClaimsPrincipal user)
    {
        var validFilters = new List<string>();
        var userContext = await BuildUserContext(user);

        foreach (var permission in permissions)
        {
            try
            {
                var rowFilterConditions = JsonSerializer.Deserialize<RowFilterConditions>(permission.RowFilter!);
                if (rowFilterConditions == null) continue;

                var filterExpression = action.ToLower() switch
                {
                    "read" => rowFilterConditions.CanRead,
                    "create" => rowFilterConditions.CanCreate,
                    "update" => rowFilterConditions.CanUpdate,
                    "delete" => rowFilterConditions.CanDelete,
                    _ => null
                };

                if (!string.IsNullOrWhiteSpace(filterExpression))
                {
                    // Replace parameters in the expression
                    var processedExpression = ProcessExpression(filterExpression, userContext);
                    if (!string.IsNullOrWhiteSpace(processedExpression))
                    {
                        validFilters.Add($"({processedExpression})");
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in RowFilter for permission {PermissionId}: {RowFilter}", permission.Id, permission.RowFilter);
            }
        }

        // Combine with OR logic (user can access if ANY role allows)
        return validFilters.Any() ? string.Join(" || ", validFilters) : string.Empty;
    }

    private async Task<Dictionary<string, object>> BuildUserContext(ClaimsPrincipal user)
    {
        var context = new Dictionary<string, object>();

        // Add common user context variables
        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        
        if (int.TryParse(userIdClaim, out var userId))
        {
            context["@currentUserId"] = userId;
            context["@userId"] = userId;
        }

        if (!string.IsNullOrEmpty(userEmail))
        {
            context["@userEmail"] = userEmail;
        }

        // Get user's org unit
        var userOrgUnit = await GetUserOrgUnitAsync(user);
        if (!string.IsNullOrEmpty(userOrgUnit))
        {
            context["@userOrgUnit"] = userOrgUnit;
            context["@orgUnit"] = userOrgUnit;
            
            // Also get the org unit ID if needed using a new context
            using var dbContext = await _contextFactory.CreateDbContextAsync();
            var orgUnitEntity = await dbContext.OrganizationHierarchies
                .FirstOrDefaultAsync(o => o.Code == userOrgUnit && o.Type == OrganizationUnitType.OrgUnit);
            
            if (orgUnitEntity != null)
            {
                context["@orgUnitId"] = orgUnitEntity.Id;
                context["@userOrgUnitId"] = orgUnitEntity.Id;
            }
        }

        return context;
    }

    private string ProcessExpression(string expression, Dictionary<string, object> userContext)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return string.Empty;

        // First validate the raw expression before processing
        if (!IsExpressionStructurallyValid(expression))
        {
            _logger.LogError("Expression failed structural validation: {Expression}", expression);
            return string.Empty;
        }

        var processedExpression = expression;

        // Replace parameters with actual values using secure replacement
        foreach (var kvp in userContext)
        {
            if (kvp.Value is string stringValue)
            {
                // Escape quotes and validate string values
                var escapedValue = EscapeStringValue(stringValue);
                processedExpression = processedExpression.Replace(kvp.Key, $"\"{escapedValue}\"");
            }
            else if (kvp.Value is int || kvp.Value is long || kvp.Value is decimal || kvp.Value is double || kvp.Value is float)
            {
                // Only allow numeric types for non-string parameters
                processedExpression = processedExpression.Replace(kvp.Key, kvp.Value.ToString());
            }
            else if (kvp.Value is bool boolValue)
            {
                processedExpression = processedExpression.Replace(kvp.Key, boolValue.ToString().ToLower());
            }
            else
            {
                _logger.LogWarning("Unsupported parameter type {Type} for key {Key}", kvp.Value?.GetType(), kvp.Key);
                return string.Empty;
            }
        }

        // Comprehensive security validation after parameter replacement
        if (!IsExpressionSafe(processedExpression))
        {
            _logger.LogWarning("Potentially unsafe expression rejected - Original: {OriginalExpression}, Processed: {ProcessedExpression}", 
                expression, processedExpression);
            return string.Empty;
        }

        // Final syntax validation
        if (!IsValidLinqExpression(processedExpression))
        {
            _logger.LogWarning("Invalid LINQ expression syntax: {Expression}", processedExpression);
            return string.Empty;
        }

        return processedExpression;
    }

    private bool IsExpressionStructurallyValid(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        // Check expression length to prevent DoS attacks
        if (expression.Length > MAX_EXPRESSION_LENGTH)
        {
            _logger.LogWarning("Expression exceeds maximum length of {MaxLength}: {ActualLength}", 
                MAX_EXPRESSION_LENGTH, expression.Length);
            return false;
        }

        // Check for balanced parentheses and nesting depth
        var openParens = 0;
        var maxDepth = 0;
        var currentDepth = 0;
        
        foreach (var c in expression)
        {
            if (c == '(')
            {
                openParens++;
                currentDepth++;
                maxDepth = Math.Max(maxDepth, currentDepth);
            }
            else if (c == ')')
            {
                currentDepth--;
                if (currentDepth < 0)
                {
                    _logger.LogWarning("Unbalanced parentheses - closing parenthesis without opening");
                    return false;
                }
            }
        }
        
        if (openParens != currentDepth)
        {
            _logger.LogWarning("Unbalanced parentheses - {OpenCount} opening, {CloseCount} closing", 
                openParens, openParens - currentDepth);
            return false;
        }
        
        if (maxDepth > MAX_NESTING_DEPTH)
        {
            _logger.LogWarning("Expression exceeds maximum nesting depth of {MaxDepth}: {ActualDepth}", 
                MAX_NESTING_DEPTH, maxDepth);
            return false;
        }

        // Check for balanced quotes
        var quoteCount = expression.Count(c => c == '"');
        if (quoteCount % 2 != 0)
        {
            _logger.LogWarning("Unbalanced quotes in expression");
            return false;
        }

        // Check for forbidden characters that could indicate injection attempts
        var forbiddenStrings = new[] { ";", "--", "/*", "*/", "\0", "\r", "\n" };
        if (forbiddenStrings.Any(expression.Contains))
        {
            _logger.LogWarning("Expression contains forbidden characters");
            return false;
        }

        return true;
    }

    private string EscapeStringValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape quotes and other special characters
        return value.Replace("\"", "\\\"")
                   .Replace("'", "\\'")
                   .Replace("\\", "\\\\")
                   .Replace("\0", "")
                   .Replace("\r", "")
                   .Replace("\n", "");
    }

    private bool IsExpressionSafe(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        var upperExpression = expression.ToUpperInvariant();
        _logger.LogDebug("Validating expression: {Expression} -> {UpperExpression}", expression, upperExpression);

        // Dangerous .NET types and namespaces
        var dangerousPatterns = new[]
        {
            "SYSTEM.", "PROCESS.", "FILE.", "DIRECTORY.", "ASSEMBLY.", "REFLECTION.",
            "TYPE.", "METHOD.", "PROPERTY.", "FIELD.", "EXECUTE", "INVOKE", "CALL",
            "RUN", "START", "THREAD.", "TASK.", "PARALLEL.", "UNSAFE.", "MARSHAL.",
            "POINTER", "INTPTR", "ACTIVATOR.", "APPDOMAINDOMAIN.", "ENVIRONMENT.",
            "CONVERT.", "ENCODING.", "REGEX.", "SCRIPT.", "EVAL", "COMPILE",
            "DYNAMIC", "EXPRESSION.", "LAMBDA", "FUNC<", "ACTION<", "DELEGATE",
            "GETTYPE", "TYPEOF", "NAMEOF", "SIZEOF", "STACKALLOC", "FIXED",
            "HTTPCONTEXT", "REQUEST.", "RESPONSE.", "SESSION.", "CACHE.",
            "CONFIGURATION.", "CONNECTIONSTRING", "SQLCOMMAND", "SQLCONNECTION",
            "DBCOMMAND", "DBCONNECTION", "OLEDB", "ODBC", "ORACLE",
            // SQL injection patterns
            "DROP ", "DELETE ", "INSERT ", "UPDATE ", "CREATE ", "ALTER ", 
            "TRUNCATE ", "EXEC ", "EXECUTE ", "UNION ", "SELECT ", "SCRIPT",
            "JAVASCRIPT:", "VBSCRIPT:", "DATA:", "JAVASCRIPT", "VBSCRIPT",
            // XSS patterns
            "<SCRIPT", "</SCRIPT", "ONLOAD", "ONERROR", "ONCLICK", "ONMOUSEOVER",
            // File system access
            "..\\", "../", "C:\\", "D:\\", "/ETC/", "/VAR/", "/USR/", "/BIN/",
            // Network access
            "HTTP://", "HTTPS://", "FTP://", "FILE://", "LDAP://", "TCP://",
            // Other dangerous patterns
            "WHILE(", "FOR(", "FOREACH(", "GOTO ", "THROW ", "CATCH(", "FINALLY(",
            "__", "GLOBAL::", "EXTERN ", "UNMANAGED", "PINVOKE"
        };

        // Check for dangerous patterns
        foreach (var pattern in dangerousPatterns)
        {
            if (upperExpression.Contains(pattern))
            {
                _logger.LogWarning("Expression contains dangerous pattern '{Pattern}': {Expression}", pattern, expression);
                return false;
            }
        }

        // Whitelist allowed property names and methods for entity filtering
        var allowedPatterns = new[]
        {
            // Entity properties
            "ID", "CODE", "NAME", "EMAIL", "PHONE", "ADDRESS", "DESCRIPTION",
            "CREATEDAT", "UPDATEDAT", "CREATEDBY", "UPDATEDBY", "ISDELETED", "ISACTIVE",
            "PARTNERID", "CONTACTID", "INTERACTIONID", "ORGUNITID", "USERID",
            // Navigation properties
            "PARTNER", "CONTACT", "INTERACTION", "ORGUNIT", "PARTNEROFFICE", "INTERACTIONUSERS",
            // Standard LINQ methods
            "ANY(", "ALL(", "COUNT(", "FIRST(", "FIRSTORDEFAULT(", "LAST(", "LASTORDEFAULT(",
            "SINGLE(", "SINGLEORDEFAULT(", "WHERE(", "SELECT(", "CONTAINS(",
            // Comparison operators
            "==", "!=", ">=", "<=", ">", "<", "&&", "||", "!",
            // Null checks
            "NULL", "ISNULL", "HASVALUE",
            // String methods (limited set)
            "TOLOWER(", "TOUPPER(", "TRIM(", "STARTSWITH(", "ENDSWITH(",
            // Numeric comparisons
            "TRUE", "FALSE"
        };

        // Remove allowed patterns from the expression for further validation
        var expressionForValidation = upperExpression;
        foreach (var allowedPattern in allowedPatterns)
        {
            var beforeLength = expressionForValidation.Length;
            expressionForValidation = expressionForValidation.Replace(allowedPattern, "");
            if (beforeLength != expressionForValidation.Length)
            {
                _logger.LogDebug("Removed pattern '{Pattern}' from validation", allowedPattern);
            }
        }

        _logger.LogDebug("After removing allowed patterns: '{RemainingExpression}'", expressionForValidation);

        // Remove safe characters (letters, numbers, basic punctuation, spaces, parameter placeholders)
        // Updated regex to include more safe characters that might appear in string literals
        var beforeRegex = expressionForValidation;
        expressionForValidation = System.Text.RegularExpressions.Regex.Replace(
            expressionForValidation, @"[A-Z0-9\s\(\)\[\]\{\}\.,=!<>&|@_\-\+\*\/\%\?:'""]", "");

        _logger.LogDebug("After regex cleanup - Before: '{Before}', After: '{After}'", beforeRegex, expressionForValidation);

        // If anything remains, it's potentially dangerous
        if (!string.IsNullOrEmpty(expressionForValidation))
        {
            _logger.LogWarning("Expression contains non-whitelisted characters: '{RemainingChars}' (Length: {Length})", 
                expressionForValidation, expressionForValidation.Length);
                
            // Log each remaining character for debugging
            for (int i = 0; i < expressionForValidation.Length; i++)
            {
                var ch = expressionForValidation[i];
                _logger.LogWarning("Remaining char at position {Position}: '{Char}' (Unicode: {Unicode})", 
                    i, ch, (int)ch);
            }
            
            return false;
        }

        _logger.LogDebug("Expression passed security validation: {Expression}", expression);
        return true;
    }

    private bool IsValidLinqExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return false;

        // Skip the dummy validation since it causes false positives when the dummy object
        // doesn't have the properties referenced in the expression.
        // The expression will be properly validated when applied to actual entity types.
        // 
        // Basic syntax checks can still be done here if needed, but for now we'll rely on
        // the security validation and let EF handle the actual LINQ validation.
        
        _logger.LogDebug("LINQ expression syntax validation skipped for: {Expression}", expression);
        return true;

        /* Original validation code that causes false positives:
        try
        {
            // Try to parse the expression using System.Linq.Dynamic.Core's parser
            // This is a dry run to validate syntax without executing
            // Create a dummy queryable to test the expression
            var dummyQuery = new[] { new { Id = 1, Name = "Test" } }.AsQueryable();
            
            // Try to build the expression - this will throw if syntax is invalid
            _ = DynamicQueryableExtensions.Where(dummyQuery, SecureParsingConfig, expression);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Expression failed LINQ syntax validation: {Expression}", expression);
            return false;
        }
        */
    }

    private async Task<string?> GetUserOrgUnitAsync(ClaimsPrincipal user)
    {
        var userEmail = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
        
        if (string.IsNullOrEmpty(userEmail))
            return null;

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var userInfo = await context.UserInfos
                .Where(u => u.UserEmail.ToLower() == userEmail.ToLower() && !u.IsDeleted)
                .Select(u => u.OrgUnit)
                .FirstOrDefaultAsync();
                
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user org unit for email: {Email}", userEmail);
            return null;
        }
    }

    private List<string> GetUserRoles(ClaimsPrincipal user)
    {
        return user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    private string GetEntityName<T>()
    {
        string entityName = typeof(T).Name;
        entityName = entityName.Replace("UNOPS", "");
        return entityName;
    }
} 