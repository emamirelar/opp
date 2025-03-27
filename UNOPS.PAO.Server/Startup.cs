using UNOPS.PAO.Identity;
using UNOPS.PAO.DataAccess.Services;
using Lamar;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UNOPS.PAO.Business.Interfaces;
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
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors(myAllowSpecificOrigins);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();
        app.UseExceptionHandler();

        app.Use(async (context, next) =>
        {
            //context.Response.Headers.Add("content-security-policy", "font-src 'self' https://fonts.gstatic.com data:; img-src 'self' https://lh3.googleusercontent.com/a/ALm5wu1Dqwlmxtyx5gOEQ2wss0UQc8sW6uFz3qiy4g_GZw=s96-c https://i.ibb.co/r771gnJ/Logistica.png data:; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://accounts.google.com/gsi/style; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net/ https://www.googletagmanager.com/gtag/js https://accounts.google.com/ https://region1.google-analytics.com/g/collect https://accounts.google.com/gsi/client https://lh3.googleusercontent.com https://fonts.gstatic.com/ https://play.google.com; default-src 'self' https://lh3.googleusercontent.com https://fonts.gstatic.com/ https://play.google.com https://accounts.google.com https://region1.google-analytics.com/g/collect ;");
            //context.Response.Headers.Add("x-content-security-policy", "font-src 'self' https://fonts.gstatic.com data:; img-src 'self' https://lh3.googleusercontent.com/a/ALm5wu1Dqwlmxtyx5gOEQ2wss0UQc8sW6uFz3qiy4g_GZw=s96-c https://i.ibb.co/r771gnJ/Logistica.png data:; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://accounts.google.com/gsi/style; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net/ https://www.googletagmanager.com/gtag/js https://accounts.google.com/ https://region1.google-analytics.com/g/collect https://accounts.google.com/gsi/client https://lh3.googleusercontent.com https://fonts.gstatic.com/ https://play.google.com; default-src 'self' https://lh3.googleusercontent.com https://fonts.gstatic.com/ https://play.google.com https://accounts.google.com https://region1.google-analytics.com/g/collect ;");
            context.Response.Headers.Add("x-content-type-options", "nosniff");
            if (!env.IsDevelopment())
            {
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

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // Register the mapping profile
        services.AddAutoMapper(cfg => cfg.AddProfile<UNOPS.PAO.UNOPSBusiness.Mapping.MappingProfile>());

        services.AddScoped<IPAOExecutionContext, PAOExecutionContext>();
        services.AddScoped<SystemConfigurationManager>();
        services.AddScoped(typeof(UserResolverService<int>));

        services.AddAuthentication(IdentityConstants.ApplicationScheme)
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

        //services.AddScoped<IManagerWrapper, ManagerWrapper>();
        services.AddScoped<IManagerWrapper, UNOPSManagerWrapper>();

        services.AddScoped<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        services.AddScoped<IAuthorizationService, PAOAuthorizationService>();

        services.AddScoped<IAuthorizationHandlerWrapper, UNOPSAuthorizationHandlerWrapper>();

        AddServices(services);
        ApplyMigrations(services);
        services.SeedAsync();
        ConfigureRegisters(services);
    }

    private void AddServices(ServiceRegistry services)
    {
        var serviceTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetLoadableTypes())
            .Where(t => t.GetInterfaces().Any(i => i == typeof(IApplicationService)))
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

        //Override / UNOPS DB context
        services.AddDbContext<UNOPSAppDbContext>(options =>
            options
                .UseNpgsql(connectionString)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

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
}