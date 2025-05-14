namespace UNOPS.PAO.UNOPSIdentity.Validators;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Identity.Entities;

public class UNOPSUserValidator<TUser> : IUserValidator<TUser> where TUser : PAOIdentityUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UNOPSUserValidator(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
        // Skip validation if the user is being created through IAP authentication
        // Check for our simulated IAP headers in development
        if (_httpContextAccessor.HttpContext?.Request.Headers.ContainsKey("X-Goog-Authenticated-User-Email") == true ||
            _httpContextAccessor.HttpContext?.Request.Headers.ContainsKey("X-Dev-IAP-Simulation") == true)
        {
            // If IAP headers present, allow UNOPS users to be created without Google authentication
            return Task.FromResult(IdentityResult.Success);
        }

        // Continue with the normal validation for non-IAP authentication
        if ((user.UserName ?? string.Empty).Trim().EndsWith("@unops.org") && !user.GoogleSignIn)
        {
            return Task.FromResult(IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidEmail",
                Description = "UNOPS user must use Google authentication"
            }));
        }
        return Task.FromResult(IdentityResult.Success);
    }
}
