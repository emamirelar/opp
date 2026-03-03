using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace UNOPS.PAO.DataAccess.Services;

// Define a local interface that mirrors the functionality needed from IUserLookupService
public interface IEmailToUserIdResolver
{
    Task<int> GetUserIdByEmailAsync(string email);
}

public class UserResolverService<TUserId>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private readonly string? _userEmail;
    private readonly IEmailToUserIdResolver? _userLookupService;

    public UserResolverService(IHttpContextAccessor context, IEmailToUserIdResolver? userLookupService = null)
    {
        _httpContextAccessor = context;
        _userLookupService = userLookupService;
    }

    public UserResolverService(string? userEmail)
    {
        _httpContextAccessor = null;
        _userEmail = userEmail;
    }

    public string? GetUserEmail()
    {
        // First try to get email from the email claim (this contains the actual email)
        // Identity.Name might contain the Firebase UID (sub claim) which is not what we want
        var emailClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(emailClaim))
        {
            return emailClaim;
        }
        
        // Fallback to Identity.Name only if no email claim exists
        return _userEmail ?? _httpContextAccessor?.HttpContext?.User?.Identity?.Name;
    }

    public string? GetUserName()
    {
        return _userEmail ?? _httpContextAccessor?.HttpContext?.User?.Identity?.Name;
    }

    public bool IsImpersonator()
    {
        var isImpersonation = _httpContextAccessor?.HttpContext?.User.HasClaim(a => a.Type == "Impersonator");
        return isImpersonation != null && isImpersonation.Value;
    }

    public TUserId GetCurrentUserId()
    {
        var context = _httpContextAccessor?.HttpContext;
        if (context == null)
        {
            return default!;
        }
            
        var user = context.User;
        
        // PRIORITY 1: Check for impersonation header from AI Assistant or other services
        // This header contains the actual user email who initiated the request via AI Assistant
        if (context.Request.Headers.TryGetValue("x-unops-impersonated-user", out var impersonatedUserEmail))
        {
            var email = impersonatedUserEmail.ToString();
            if (!string.IsNullOrEmpty(email) && _userLookupService != null)
            {
                try
                {
                    // Console.WriteLine($"[UserResolverService] Found impersonated user email: {email}");
                    var userId = _userLookupService.GetUserIdByEmailAsync(email).GetAwaiter().GetResult();
                    if (userId > 0)
                    {
                        // Console.WriteLine($"[UserResolverService] Resolved impersonated user to ID: {userId}");
                        return (TUserId)Convert.ChangeType(userId, typeof(TUserId));
                    }
                }
                catch (Exception)
                {
                    // Log but continue to other authentication methods
                }
            }
        }
        
        // PRIORITY 2: Check if we have IAP headers, regardless of authentication state
        // This is critical for development mode where the authentication might not be fully processed
        if (context.Request.Headers.TryGetValue("X-Goog-Authenticated-User-Email", out var headerValue) ||
            context.Request.Headers.TryGetValue("X-Dev-IAP-Simulation", out _))
        {
            string? email = null;
            
            // Try to get email from the header first
            if (!string.IsNullOrEmpty(headerValue))
            {
                // IAP email header format: "accounts.google.com:user@example.com"
                email = headerValue.ToString().Split(':').Last();
                // Console.WriteLine($"[UserResolverService] Found email in IAP headers: {email}");
            }
            else if (user != null)
            {
                // Try to get email from claims as a fallback
                email = user.FindFirst(ClaimTypes.Email)?.Value;
                // Console.WriteLine($"[UserResolverService] Found email in user claims: {email}");
            }
            
            // If we have an email, try to resolve it to a user ID
            if (!string.IsNullOrEmpty(email))
            {
                // If we have the lookup service, try to get the user ID only once to avoid loops
                if (_userLookupService != null)
                {
                    try
                    {
                        var userId = _userLookupService.GetUserIdByEmailAsync(email).GetAwaiter().GetResult();
                        if (userId > 0)
                        {
                            // Console.WriteLine($"[UserResolverService] Resolved email to user ID: {userId}");
                            return (TUserId)Convert.ChangeType(userId, typeof(TUserId));
                        }
                        // Console.WriteLine($"[UserResolverService] Failed to resolve email to user ID, got: {userId}");
                    }
                    catch (Exception)
                    {
                        // Log but don't throw to avoid breaking the app
                    }
                }
                
                // If no lookup service or lookup failed, check if the user is already authenticated
                if (user?.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (userIdClaim != null)
                    {
                        // Console.WriteLine($"[UserResolverService] Using authenticated user ID: {userIdClaim}");
                        return (TUserId)Convert.ChangeType(userIdClaim, typeof(TUserId));
                    }
                }
                
                // Return a default value as fallback only if we truly can't resolve the ID
                // Console.WriteLine("[UserResolverService] Using default user ID fallback");
                return (TUserId)Convert.ChangeType(1, typeof(TUserId));
            }
        }
        
        // PRIORITY 3: If there are no IAP headers, try standard claims-based authentication
        if (user?.Identity?.IsAuthenticated == true)
        {
            // Try to get user ID from NameIdentifier claim
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim != null)
            {
                // Console.WriteLine($"[UserResolverService] Using authenticated user ID from claims: {userIdClaim}");
                return (TUserId)Convert.ChangeType(userIdClaim, typeof(TUserId));
            }
        }

        // If all else fails, return default
        return default!;
    }
}