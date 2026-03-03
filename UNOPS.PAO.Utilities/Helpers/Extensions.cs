using Microsoft.AspNetCore.Http;

namespace UNOPS.PAO.Utilities.Helpers;

using System.Reflection;
using System.Security.Claims;
using UNOPS.PAO.Domain.Entities;

public static class Extensions
{
    //public static void AddUserAuthentication(this IServiceCollection services, IConfiguration configuration)
    //{
    //    services.AddIdentity<ApplicationUser, ApplicationRole>()
    //        .AddEntityFrameworkStores<ApplicationDbContext>();

    //    SecretManagerConfigurationProvider secretManager = new SecretManagerConfigurationProvider();
    //    IConfigurationSection jwtSettings = configuration.GetSection("JwtSettings");
    //    string? jwtSecurityKey = Environment.GetEnvironmentVariable("JWT_SECRET") ??
    //                         secretManager.GetSecret("JWT_SECRET") ??
    //                         jwtSettings.GetSection("securityKey").Value;

    //    services.AddAuthentication(opt =>
    //    {
    //        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //    }).AddJwtBearer(options =>
    //    {
    //        options.TokenValidationParameters = new TokenValidationParameters
    //        {
    //            ValidateIssuer = true,
    //            ValidateAudience = true,
    //            ValidateLifetime = true,
    //            ValidateIssuerSigningKey = true,
    //            ValidIssuer = jwtSettings.GetSection("validIssuer").Value,
    //            ValidAudience = jwtSettings.GetSection("validAudience").Value,
    //            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecurityKey)),
    //            ClockSkew = TimeSpan.FromHours(8)
    //        };
    //    });
    //    services.AddScoped<JwtHandler>();
    //    services.AddScoped<UserServices>();
			
    //    services.AddAuthorization(options => { options.AddAuthorizationPolicies(configuration); });
    //    services.AddTransient<IAuthorizationHandler, RolesInDbAuthorizationHandler>();
    //}

    public static string GetFileType(this IFormFile file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));
        return file.ContentType;
    }
    
    public static bool HasRoles(this ClaimsPrincipal user, params string [] roles)
    {
        return user.Claims
            .Where(a => a.Type == ClaimTypes.Role)
            .Any(a => roles.Contains(a.Value));
    }
    
    public static DateTime ToUserUTCDate(this DateTime date)
    {
        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
    }

    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null).Cast<Type>();
        }
    }

    /// <summary>
    ///     Checks wheather the given type implements a generic type. The check is done
    ///     on the immediate base class and all subsequent/nested base classes.
    /// </summary>
    /// <param name="me">Type which potentially implements the generic class.</param>
    /// <param name="genericType">Generic type which should be implemented.</param>
    /// <returns>True or false.</returns>
    public static bool ImplementsGenericType(this Type me, Type genericType)
    {
        if (!genericType.IsGenericType)
        {
            throw new ArgumentException("Supplied argument is not a generic type", nameof(genericType));
        }

        return me.GetBaseClasses().Any(b => b.IsConstructedGenericType && b.GetGenericTypeDefinition() == genericType);
    }

    public static IEnumerable<Type> GetBaseClasses(this Type type)
    {
        var current = type;
        while (current.BaseType != typeof(object) && current.BaseType != null)
        {
            yield return current.BaseType;
            current = current.BaseType;
        }
    }

    /// <summary>
    ///     Checks whether this class inherits another class.
    /// </summary>
    /// <param name="type">Type which might be inheriting from the other class.</param>
    /// <param name="baseClass">Base class which should be implemented by <paramref name="type" />.</param>
    /// <returns>True or false.</returns>
    public static bool ImplementsClass(this Type type, Type baseClass)
    {
        return type.GetBaseClassOfType(baseClass) != null;
    }

    /// <summary>
    ///     Checks whether this class inherits another class.
    /// </summary>
    /// <param name="type">Type which might be inheriting from the other class.</param>
    /// <param name="baseClass">Base class which should be implemented by <paramref name="type" />.</param>
    /// <returns>Type implementing the <paramref name="baseClass" />, or null if not found.</returns>
    public static Type? GetBaseClassOfType(this Type type, Type baseClass)
    {
        if (type == baseClass)
        {
            return baseClass;
        }

        var baseType = type.BaseType;
        if (baseType is null)
        {
            return null;
        }

        if (baseClass.IsGenericType)
        {
            // T1 : T2<int>
            if (baseType.IsConstructedGenericType)
            {
                var genericType = baseClass.IsConstructedGenericType
                    ? baseType
                    : baseType.GetGenericTypeDefinition();

                if (genericType == baseClass)
                {
                    return baseType.ContainsGenericParameters
                        ? baseType.GetGenericTypeDefinition()
                        : baseType;
                }
            }
        }

#pragma warning disable CS8602 // baseType is non-null after check above
#pragma warning disable CS8603 // Method returns Type? - null is valid
        return baseType.GetBaseClassOfType(baseClass);
#pragma warning restore CS8603
#pragma warning restore CS8602
    }

    public static object GetPropertyValue(this object src, string propName)
    {
        return src.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)?.GetValue(src, null)!;
    }
}