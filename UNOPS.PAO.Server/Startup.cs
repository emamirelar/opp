using UNOPS.PAO.Identity;
using UNOPS.PAO.DataAccess.Services;
using Lamar;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.GoogleServices;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Identity.Extensions;
using UNOPS.PAO.Server.Infrastructure;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSIdentity;
using UNOPS.PAO.UNOPSIdentity.Validators;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Utilities.Interfaces;
using UNOPS.PAO.Utilities.Registers;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.Server.Infrastructure.Security;
using UNOPS.PAO.UNOPSPresentation.ContextPermissionHandlers;
using UNOPS.PAO.Presentation.ContextPermissionHandlers;
using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSIdentity.Authentication;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSPresentation.Authorization;
using UNOPS.PAO.UNOPSDataAccess.Seed;
using UNOPS.PAO.UNOPSPresentation.Middleware;
using System.IO;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.UNOPSBusiness.Extensions;

namespace UNOPS.PAO.Server;

public class Startup
{
    public Startup(IWebHostEnvironment environment)
    {
        Configuration = new SystemConfigurationManager(environment).GetConfiguration();
        CurrentEnvironment = environment;
    }

    public IConfiguration Configuration { get; }
    private IWebHostEnvironment CurrentEnvironment { get; }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        var myAllowSpecificOrigins = "AllowOrigin";

        // Configure the HTTP request pipeline.
        if (!env.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
            
            // Development login page middleware
            app.UseWhen(
                context => context.Request.Path.StartsWithSegments("/dev-login"),
                appBuilder => appBuilder.UseMiddleware<DevelopmentLoginPageMiddleware>()
            );
        }

        app.UseStaticFiles();
        app.UseRouting();
        
        // Add diagnostic logging middleware to check headers FIRST
        app.UseMiddleware<AuthenticationLoggingMiddleware>();
        
        // Add IAP verification middleware second - will log headers in original form
        app.UseIAPVerification();
        
        // Add IAP simulation in development THIRD - it may modify the headers
        if (env.IsDevelopment())
        {
            // Development login page middleware
            app.UseWhen(
                context => context.Request.Path.StartsWithSegments("/dev-login"),
                appBuilder => appBuilder.UseMiddleware<DevelopmentLoginPageMiddleware>()
            );
            
            // Set IAP headers for development
            app.UseMiddleware<DevelopmentIAPAuthHandler>();
            
            // Add a second instance of logging AFTER development middleware to see modified headers
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    var logger = loggerFactory.CreateLogger("PostDevMiddlewareLogger");
                    logger.LogInformation("Headers AFTER dev middleware:");
                    
                    foreach (var header in context.Request.Headers)
                    {
                        if (header.Key.Contains("jwt", StringComparison.OrdinalIgnoreCase))
                        {
                            logger.LogInformation("  {Key}: [REDACTED - Length: {Length}]", 
                                header.Key, header.Value.ToString().Length);
                        }
                        else
                        {
                            logger.LogInformation("  {Key}: {Value}", header.Key, header.Value);
                        }
                    }
                }
                
                await next();
            });
        }
        
        app.UseCors(myAllowSpecificOrigins);
        
        // Standard authentication processing
        app.UseAuthentication();
        
        app.UseAuthorization();
        
        app.UseHttpsRedirection();
        app.UseExceptionHandler();

        // Configure Strict-Transport-Security header
        app.Use(async (context, next) =>
        {
            if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
            {
                context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains;");
            }

            await next();
        });

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            //endpoints.MapControllerRoute(
            //    "default",
            //    "{controller}/{action=Index}/{id?}");

            endpoints.MapGroup("/user").MapIdentityApi<PAOIdentityUser>();

            // Register google signin controller from Identity project
            endpoints.MapGroup("/user").MapPAOIdentityApi<PAOIdentityUser>();

            endpoints.MapFallbackToFile("index.html");
        });
    }

    public void ConfigureContainer(ServiceRegistry services)
    {
        ConfigureDataAccess(services);

        services.Scan(x =>
        {
            x.AddAllTypesOf(typeof(Register<>));
            x.WithDefaultConventions();
        });

        services.AddScoped(GetDbSchema);
        services.AddHttpContextAccessor();
        
        // Register dev middleware
        services.AddScoped<DevelopmentIAPAuthHandler>();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // Register the mapping profile
        services.AddAutoMapper(cfg => cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Mapping.MappingProfile>());

        services.AddScoped<IPAOExecutionContext, PAOExecutionContext>();
        services.AddScoped<SystemConfigurationManager>();
        
        // Add user resolver service with correct registration order
        services.AddScoped<IUserLookupService, UserLookupService>();
        services.AddScoped<IEmailToUserIdResolver>(sp => sp.GetRequiredService<IUserLookupService>());
        services.AddScoped(typeof(UserResolverService<int>));
        
        // Add services for IAP verification
        services.AddIAPVerification();
        
        // Add memory cache for permission caching
        services.AddMemoryCache();
        
        // Register EntityPermissionHelper
        services.AddScoped<EntityPermissionHelper>();
        
        // Register authorization handlers
        ConfigureAuthorization(services);
        
        // Configure authentication with support for both IAP and cookies
        services.AddAuthentication(options =>
            {
                // Always use IAP as the default authentication scheme for all requests
                options.DefaultAuthenticateScheme = "IAP"; 
                options.DefaultChallengeScheme = "IAP";
                options.DefaultScheme = "IAP";
                
                // Keep cookie as the sign-in scheme for interactive login
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme,
                opt => {
                    opt.Events.OnRedirectToLogin = (context) =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    };
                    opt.Events.OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    };
                })
            // Add IAP authentication handler
            .AddScheme<IAPAuthenticationOptions, IAPAuthenticationHandler>("IAP", options => 
            {
                // Load IAP settings from configuration
                var iapConfig = Configuration.GetSection("IAP");
                
                options.AutoProvisionUsers = iapConfig.GetValue<bool>("AutoProvisionUsers", true);
                options.DefaultRole = iapConfig.GetValue<string>("DefaultRole", "User");
                options.RequireJwtVerification = iapConfig.GetValue<bool>("RequireJwtVerification", true);
                options.AllowHeaderFallback = iapConfig.GetValue<bool>("AllowHeaderFallback", false);
                options.ProjectNumber = iapConfig.GetValue<string>("ProjectNumber", "");
                options.ProjectId = iapConfig.GetValue<string>("ProjectId", "");
                options.BackendServiceId = iapConfig.GetValue<string>("BackendServiceId", "");
                options.HealthCheckPath = iapConfig.GetValue<string>("HealthCheckPath", "/health");
                
                // Add Cloud Run-specific settings
                options.Region = iapConfig.GetValue<string>("Region", "");
                options.ServiceName = iapConfig.GetValue<string>("ServiceName", "");
                
                // Configure domain role mappings
                options.DomainRoles = new Dictionary<string, string>();
                var domainMappings = iapConfig.GetSection("DomainRoles");
                if (domainMappings.Exists())
                {
                    foreach (var child in domainMappings.GetChildren())
                    {
                        options.DomainRoles[child.Key] = child.Value;
                    }
                }
                
                // Configure external role mappings
                options.ExternalRoleMappings = new Dictionary<string, string>();
                var roleMappings = iapConfig.GetSection("ExternalRoleMappings");
                if (roleMappings.Exists())
                {
                    foreach (var child in roleMappings.GetChildren())
                    {
                        options.ExternalRoleMappings[child.Key] = child.Value;
                    }
                }
                
                // Configure group role mappings
                options.ExternalGroupMappings = new Dictionary<string, string>();
                var groupMappings = iapConfig.GetSection("ExternalGroupMappings");
                if (groupMappings.Exists())
                {
                    foreach (var child in groupMappings.GetChildren())
                    {
                        options.ExternalGroupMappings[child.Key] = child.Value;
                    }
                }
            });

        services.AddIdentityCore<PAOIdentityUser>()
            .AddRoles<PAOIdentityRole>()
            .AddEntityFrameworkStores<PAOIdentityDbContext>()
            // Adding default identity api end point.
            .AddApiEndpoints()
            // Custom UNOPS user manager for IsInternal flag logic, remove or replace with custom manager
            .AddUserManager<UNOPSUserManager>()
            // Custom UNOPS user validator, remove or replace with custom validator
            .AddUserValidator<UNOPSUserValidator<PAOIdentityUser>>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // RBAC Services
        services.AddScoped<IPermissionService, PermissionService>();
        
        // Add Business Security Service for row-level filtering
        services.AddScoped<IBusinessSecurityService, BusinessSecurityService>();
        
        // Add SavedFilter Service
        services.AddScoped<ISavedFilterService, SavedFilterService>();
        
        // Add Generic Row Filter Service
        services.AddScoped<IGenericRowFilterService, GenericRowFilterService>();
        
        // Add Generic Column Filter Service
        services.AddScoped<IGenericColumnFilterService, GenericColumnFilterService>();
        
        // Add RBAC Infrastructure (interceptors, proxy generator, etc.)
        services.AddRBACInfrastructure();
        
        // Add Secure Specification Factory for RBAC-integrated pagination
        services.AddScoped<ISecureSpecificationFactory, SecureSpecificationFactory>();
        
        // Configure authorization
        services.AddAuthorization(options =>
        {
            // Set default policy to accept IAP authentication
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("IAP")
                .RequireAuthenticatedUser()
                .Build();
        });

        // Register authorization handlers and policy providers
        services.AddSingleton<IAuthorizationPolicyProvider, EntityPermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, EntityPermissionHandler>();

        // Keep existing authorization services
        services.AddScoped<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddScoped<IAuthorizationService, PAOAuthorizationService>();
        services.AddScoped<IAuthorizationHandlerWrapper, UNOPSAuthorizationHandlerWrapper>();

        // Register UserInfo service
        services.AddScoped<IUserInfoService, UserInfoService>();

        // Register OrganizationHierarchy manager
        services.AddScoped<IOrganizationHierarchyManager, OrganizationHierarchyManager>();
        
        // Add data seeding services
        services.AddDataSeeding();

        //services.AddScoped<IManagerWrapper, ManagerWrapper>();
        services.AddScoped<IManagerWrapper, UNOPSManagerWrapper>();

        AddServices(services);
        services.AddScoped<IGoogleDriveDocumentManager, GoogleDriveDocumentManager>();
        services.AddScoped<GoogleCloudStorageService>();
        ApplyMigrations(services);
        services.SeedAsync();
        ConfigureRegisters(services);
        services.AddHostedService<PubSubPullService>(); // Register your background service
        
        // Automatically discover and register all managers with RBAC attributes
        services.AddRBACManagersAutomatically();
    }

    private void AddServices(ServiceRegistry services)
    {
        var serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetLoadableTypes())
            .Where(i => i.GetInterfaces().Any(i => i == typeof(IApplicationService)))
            .ToList();

        foreach (var type in serviceTypes)
        {
            services.AddScoped(type);
        }
    }

    private void ConfigureDataAccess(ServiceRegistry services)
    {
        string? connectionString = !CurrentEnvironment.IsDevelopment() ? GetConnectionStringFromSecretManager() : Configuration.GetConnectionString("DbContext");

        if (connectionString == null)
            throw new Exception("Connection string cannot be null. " +
                                $"Please set it up under in appsettings.{CurrentEnvironment.EnvironmentName}.json under ConnectionStrings. " +
                                $"Current environment: {CurrentEnvironment.EnvironmentName}.");

        // Core DB context
        services.AddDbContext<DataAccess.Context.AppDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

        // Override / UNOPS DB context
        services.AddDbContext<UNOPSAppDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

        // Register IDbContextFactory for UNOPSAppDbContext
        services.AddDbContextFactory<UNOPSAppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddDbContext<PAOIdentityDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

    }
    private string? GetConnectionStringFromSecretManager()
    {
        var envDbConSecretName = Configuration.GetConnectionString("ConnectionSecretName");
        var projectId = Configuration.GetSection("AppConfig")["ProjectId"];
        var secretManager = new GoogleSecretManagerConfigurationProvider(projectId);
    
        return secretManager.GetSecretVersion(envDbConSecretName, "latest");
    }

    private void ApplyMigrations(IServiceCollection services)
    {
        var sp = services.BuildServiceProvider();

        ApplyContextMigrations<PAOIdentityDbContext>(sp);
        //ApplyContextMigrations<DataAccess.Context.AppDbContext>(sp);
        ApplyContextMigrations<UNOPSDataAccess.Context.UNOPSAppDbContext>(sp);
    }

    private void ApplyContextMigrations<T>(ServiceProvider sp) where T : DbContext
    {
        var dbContext = sp.GetRequiredService<T>();

        if (dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }
    }

    private IDbContextSchema GetDbSchema(IServiceProvider ctx)
    {
        // This was the implementation of schema per project solution, moved to another solution for now
        //  var user = ctx.GetRequiredService<UserResolverService>();
        //  var schema = user.GetProjectSchemaName() ?? "public";
        //  var schema = "public";

        string schema = Configuration.GetConnectionString("DbSchema");
        return new DbContextSchema(schema);
    }

    public void ConfigureRegisters(ServiceRegistry services)
    {
        var container = new Container(services);

        var registerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetLoadableTypes())
            .Where(p => p.ImplementsGenericType(typeof(Register<>)) && p != typeof(Register<>))
            .ToList();

        var assembliesToRegister = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic)
            .ToList();

        foreach (var registerType in registerTypes)
        {
            var register = container.GetInstance(registerType);
            var registerAssemblyMethod = registerType.GetMethod(nameof(Register<int>.RegisterAssembly));

            foreach (var assembly in assembliesToRegister)
            {
                // ReSharper disable once PossibleNullReferenceException
                registerAssemblyMethod.Invoke(register, new object[] { assembly });
            }

            services.AddSingleton(registerType, register);
        }
    }

    private void ConfigureAuthorization(ServiceRegistry services)
    {
        // Add the entity permission authorization handler
        services.AddScoped<IAuthorizationHandler, EntityPermissionHandler>();
        
        // Add all your entity-specific authorization handlers
        services.AddScoped<IAuthorizationHandler, ContactAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, ProfileAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, PartnerTreeAuthorizationHandler>();
        
        // Add the wrapping authorization handler
        services.AddScoped<IAuthorizationHandlerWrapper, AuthorizationHandlerWrapper>();
    }
}