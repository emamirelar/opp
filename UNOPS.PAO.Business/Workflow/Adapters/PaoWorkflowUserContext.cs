using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.Workflow.Business.Interfaces;

namespace UNOPS.PAO.Business.Workflow.Adapters;

/// <summary>
/// PAO implementation of IWorkflowUserContext.
/// Provides current user information from the HTTP context for workflow operations.
/// </summary>
public class PaoWorkflowUserContext : IWorkflowUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public PaoWorkflowUserContext(
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        AppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _context = context;
    }

    /// <summary>
    /// Gets the current user's ID from the NameIdentifier claim.
    /// </summary>
    public int CurrentUserId => int.TryParse(
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        out var id) ? id : 0;

    /// <summary>
    /// Gets the current user's display name.
    /// Queries the user profile from the database if available.
    /// </summary>
    public string CurrentUserName
    {
        get
        {
            var userId = CurrentUserId;
            if (userId == 0) 
                return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            
            // Query user profile from database
            var userProfile = _context.UserProfile
                .FirstOrDefault(up => up.UserId == userId);
            
            if (userProfile != null && !string.IsNullOrEmpty(userProfile.Name))
                return userProfile.Name;
            
            // Fallback to email or identity name
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
        }
    }

    /// <summary>
    /// Gets the current user's email from the Email claim.
    /// </summary>
    public string CurrentUserEmail
    {
        get
        {
            var emailClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(emailClaim))
                return emailClaim;
            
            // Fallback: try to get from user table
            var userId = CurrentUserId;
            if (userId > 0)
            {
                var user = _context.PAOUsers.FirstOrDefault(u => u.Id == userId);
                return user?.Email ?? string.Empty;
            }
            
            return string.Empty;
        }
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
