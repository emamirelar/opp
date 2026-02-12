namespace UNOPS.PAO.UNOPSIdentity;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UNOPS.PAO.Identity.Entities;


public class UNOPSUserManager : UserManager<PAOIdentityUser>
{
    public UNOPSUserManager(IUserStore<PAOIdentityUser> store, IOptions<IdentityOptions> optionsAccessor,
                            IPasswordHasher<PAOIdentityUser> passwordHasher,
                            IEnumerable<IUserValidator<PAOIdentityUser>> userValidators,
                            IEnumerable<IPasswordValidator<PAOIdentityUser>> passwordValidators,
                            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
                            IServiceProvider services, ILogger<UserManager<PAOIdentityUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    public override Task<IdentityResult> CreateAsync(PAOIdentityUser user)
    {
        user.IsInternal = (user.Email ?? string.Empty).EndsWith("@unops.org");

        return base.CreateAsync(user);
    }
}