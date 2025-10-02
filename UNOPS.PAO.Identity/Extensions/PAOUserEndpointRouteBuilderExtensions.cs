namespace UNOPS.PAO.Identity.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UNOPS.PAO.Identity.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;
using UNOPS.PAO.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using UNOPS.PAO.Identity.Context;
using System.Security.Claims;

public static class PAOUserEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapPAOIdentityApi<TUser>(this IEndpointRouteBuilder endpoints)
        where TUser : PAOIdentityUser, new()
    {
        var routeGroup = endpoints.MapGroup("");

        routeGroup.MapPost("/googleSignIn", async Task<Results<Ok, BadRequest>>
            ([FromBody] GoogleSignInRequest req, HttpContext context, IConfiguration configuration, [FromServices] IServiceProvider sp) =>
        {
            var userManager = sp.GetRequiredService<UserManager<TUser>>();
            var signInManager = sp.GetRequiredService<SignInManager<TUser>>();

            var googleSettings = configuration.GetSection("GoogleAuthSettings");
            var appConfig = configuration.GetSection("AppConfig");

            signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;

            // Get Google Client ID from Secret Manager
            string? googleClientId = null;
            try
            {
                var clientIdSecretName = googleSettings.GetSection("ClientIdSecretName").Value;
                var projectId = appConfig.GetSection("ProjectId").Value;
                
                // Fallback to direct configuration if secret retrieval fails
                if (string.IsNullOrEmpty(googleClientId))
                {
                    googleClientId = googleSettings.GetSection("clientId").Value;
                }
            }
            catch
            {
                // Fallback to direct configuration on any error
                googleClientId = googleSettings.GetSection("clientId").Value;
            }

            var payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { googleClientId ?? string.Empty }
                });

            if (payload == null)
            {
                return TypedResults.BadRequest();
            }

            var user = await userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new TUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    GoogleSignIn = true
                };

                var result = await userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    return TypedResults.BadRequest();
                }

            }

            await signInManager.SignInAsync(user, false);

            return TypedResults.Ok();

        });

        routeGroup.MapGet("/isInternal", async Task<Ok<bool>>
            (HttpContext context, [FromServices] UserManager<TUser> userManager) =>
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return TypedResults.Ok(false);
            }

            var userName = context.User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return TypedResults.Ok(false);
            }

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return TypedResults.Ok(false);
            }

            return TypedResults.Ok(user.IsInternal);
        });

        routeGroup.MapGet("/claims", async Task<dynamic>
            (HttpContext context, [FromServices] UserManager<TUser> userManager) =>
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var userName = context.User.Identity.Name;
            if (string.IsNullOrEmpty(userName))
            {
                return Results.Unauthorized();
            }

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return Results.Unauthorized();
            }

            // Get all claims from the user manager
            var userClaims = await userManager.GetClaimsAsync(user);
            
            // Add role claims
            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            userClaims.Add(new Claim("userId", user.Id.ToString()));

            // Add essential claims
            if (!string.IsNullOrEmpty(user.UserName))
            {
                userClaims.Add(new Claim(ClaimTypes.Name, user.UserName));
            }
            if (!string.IsNullOrEmpty(user.Email))
            {
                userClaims.Add(new Claim(ClaimTypes.Email, user.Email));
            }
            userClaims.Add(new Claim("IsInternal", user.IsInternal.ToString()));

            return userClaims.Select(x => new { x.Type, x.Value }).ToList();
        });

        routeGroup.MapGet("/permissions", Task<object>
            (HttpContext context, [FromServices] IPAOExecutionContext executionContext) =>
        {
            var userPermissions = executionContext.UserPermissions.Select(p => p.Name).ToList();
            return Task.FromResult<object>(userPermissions.Distinct().ToList());
        }).RequireAuthorization();

        return new PAOUserEndpointConventionBuilder(routeGroup);
    }
}

// Wrap RouteGroupBuilder with a non-public type to avoid a potential future behavioral breaking change.
internal sealed class PAOUserEndpointConventionBuilder(RouteGroupBuilder inner) : IEndpointConventionBuilder
{
    private IEndpointConventionBuilder InnerAsConventionBuilder => inner;

    public void Add(Action<EndpointBuilder> convention) => InnerAsConventionBuilder.Add(convention);
    public void Finally(Action<EndpointBuilder> finallyConvention) => InnerAsConventionBuilder.Finally(finallyConvention);
}
