namespace UNOPS.PAO.UNOPSIdentity.Validators;

using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;

public class UNOPSUserValidator<TUser> : IUserValidator<TUser> where TUser : PAOIdentityUser
{
    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
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
