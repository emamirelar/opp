using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.Workflow.Business.Interfaces;

namespace UNOPS.PAO.Business.Workflow.Adapters;

/// <summary>
/// PAO implementation of IWorkflowUserContext.
/// Provides current user information from the HTTP context for workflow operations.
/// Uses DbContextFactory to create separate context instances for user lookups,
/// avoiding DbContext concurrency issues with async workflow operations.
/// </summary>
public class PaoWorkflowUserContext : IWorkflowUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    // Cached user information to avoid repeated database queries
    private string? _cachedUserName;
    private string? _cachedUserEmail;
    private int _cachedUserId = -1; // -1 indicates not yet cached

    public PaoWorkflowUserContext(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Gets the current user's ID from the NameIdentifier claim.
    /// </summary>
    public int CurrentUserId => int.TryParse(
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        out var id) ? id : 0;

    /// <summary>
    /// Gets the current user's display name asynchronously.
    /// Queries the user profile from the database if available.
    /// Uses a separate DbContext instance to avoid concurrency issues.
    /// Results are cached per request to avoid repeated queries.
    /// </summary>
    public async Task<string> GetCurrentUserNameAsync()
    {
        var userId = CurrentUserId;
        if (userId == 0) 
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
        
        // Return cached value if available for the same user
        if (_cachedUserId == userId && _cachedUserName != null)
            return _cachedUserName;
        
        // Query user profile from database using a separate context
        await using var context = await _contextFactory.CreateDbContextAsync();
        var userProfile = await context.UserProfile
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId);
        
        if (userProfile != null && !string.IsNullOrEmpty(userProfile.Name))
        {
            _cachedUserName = userProfile.Name;
            _cachedUserId = userId;
            return _cachedUserName;
        }
        
        // Fallback to email or identity name
        _cachedUserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
        _cachedUserId = userId;
        return _cachedUserName;
    }

    /// <summary>
    /// Gets the current user's email asynchronously.
    /// Uses a separate DbContext instance to avoid concurrency issues.
    /// Results are cached per request to avoid repeated queries.
    /// </summary>
    public async Task<string> GetCurrentUserEmailAsync()
    {
        var emailClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(emailClaim))
            return emailClaim;
        
        // Return cached value if available for the same user
        var userId = CurrentUserId;
        if (_cachedUserId == userId && _cachedUserEmail != null)
            return _cachedUserEmail;
        
        // Fallback: try to get from user table using a separate context
        if (userId > 0)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var user = await context.PAOUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);
            
            _cachedUserEmail = user?.Email ?? string.Empty;
            _cachedUserId = userId;
            return _cachedUserEmail;
        }
        
        return string.Empty;
    }

    /// <summary>
    /// Gets the role names assigned to the current user from Role claims.
    /// </summary>
    public IEnumerable<string> CurrentUserRoles
    {
        get
        {
            var roles = _httpContextAccessor.HttpContext?.User
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList() ?? new List<string>();
            
            return roles;
        }
    }

    /// <summary>
    /// Checks if the current user has a specific role.
    /// </summary>
    public bool HasRole(string roleName)
    {
        return CurrentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the current environment name from configuration.
    /// </summary>
    public string Environment => _configuration.GetValue<string>("AppConfig:Environment") ?? "Unknown";

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
