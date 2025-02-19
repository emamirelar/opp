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

            signInManager.AuthenticationScheme = IdentityConstants.ApplicationScheme;

            var payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { googleSettings.GetSection("clientId").Value ?? string.Empty }
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
            if (!context.User.Identity.IsAuthenticated)
            {
                return TypedResults.Ok(false);
            }

            var user = await userManager.FindByNameAsync(context.User.Identity.Name);
            if (user == null)
            {
                return TypedResults.Ok(false);
            }

            return TypedResults.Ok(user.IsInternal);
        });

        routeGroup.MapGet("/claims", object
            (HttpContext context, [FromServices] IServiceProvider sp) =>
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var userClaims = context.User.Claims.Select(x => new { x.Type, x.Value }).ToList();

            return userClaims;

        }).RequireAuthorization();

        routeGroup.MapGet("/permissions", async Task<object>
            (HttpContext context, [FromServices] IPAOExecutionContext executionContext) =>
        {
            var userPermissions = executionContext.UserPermissions.Select(p => p.Name).ToList();
            return userPermissions.Distinct().ToList();
        }).RequireAuthorization();

        routeGroup.MapPost("/logout", async Task<Results<Ok, BadRequest>>
            (HttpContext context) =>
        {
            await context.SignOutAsync();
            return TypedResults.Ok();
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
