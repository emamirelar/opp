using UNOPS.PAO.Identity;
using UNOPS.PAO.DataAccess.Services;
using Lamar;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
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
using UNOPS.PAO.Business;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.MailSender;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Google.Api.Gax;
using Google.Cloud.SecretManager.V1;
using UNOPS.PAO.UNOPSIdentity.Authentication;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSPresentation.Authorization;
using UNOPS.PAO.UNOPSDataAccess.Seed;
using System.IO;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.Business.Services;
using Google.Apis.Auth.OAuth2;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.Workflow.DataAccess;

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
        var myAllowSpecificOrigins = "AllowAll";

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
        
        // Add IAP simulation in development BEFORE verification - must add headers first
        if (env.IsDevelopment())
        {
            // Development login page middleware
            app.UseWhen(
                context => context.Request.Path.StartsWithSegments("/dev-login"),
                appBuilder => appBuilder.UseMiddleware<DevelopmentLoginPageMiddleware>()
            );
            
            // Set IAP headers for development BEFORE verification
            app.UseMiddleware<DevelopmentIAPAuthHandler>();
        }
        
        // Add IAP verification middleware AFTER development headers are set
        app.UseIAPVerification();
        
        // Add a second instance of logging AFTER development middleware to see modified headers in development
        if (env.IsDevelopment())
        {
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
        
        if (!env.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        
        app.UseCors(myAllowSpecificOrigins);
        
        // Standard authentication processing
        app.UseAuthentication();
        
        app.UseAuthorization();
        
        app.UseExceptionHandler();

        // Configure Strict-Transport-Security header
        app.Use(async (context, next) =>
        {
            if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
            {
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains;";
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
        if (!CurrentEnvironment.IsEnvironment("Testing"))
        {
            ConfigureDataAccess(services);
        }

        services.Scan(x =>
        {
            x.AddAllTypesOf(typeof(Register<>));
            x.WithDefaultConventions();
        });

        services.AddScoped(GetDbSchema);
        services.AddHttpContextAccessor();
        
        // Register dev middleware
        services.AddScoped<DevelopmentIAPAuthHandler>();

        // Register the EntityArtifactValueResolver and EntityDocumentValueResolver for automatic artifact and document loading
        services.AddScoped<UNOPS.PAO.Business.Mapping.EntityArtifactValueResolver>();
        services.AddScoped<UNOPS.PAO.Business.Mapping.EntityDocumentValueResolver>();

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
        
        // Add HttpClient for external service calls
        services.AddHttpClient();
        
        // Register AI prompt cache service
        services.AddScoped<IAiPromptCacheService, AiPromptCacheService>();
        
        // Register EntityPermissionHelper
        services.AddScoped<EntityPermissionHelper>();
        
        // Register authorization handlers
        ConfigureAuthorization(services);

        // Get JWT secret from Secret Manager (skip in Testing environment)
        string jwtSecret;
        if (CurrentEnvironment.IsEnvironment("Testing"))
        {
            // Use a test JWT secret for testing environment
            jwtSecret = "test-jwt-secret-key-for-integration-tests-minimum-32-characters-long";
        }
        else
        {
            var projectId = Configuration["AppConfig:ProjectId"];
            var secretManager = SecretManagerServiceClient.Create();
            var secretName = $"projects/{projectId}/secrets/Bearer_Auth_Secret/versions/latest";
            var secret = secretManager.AccessSecretVersion(secretName);
            jwtSecret = secret.Payload.Data.ToStringUtf8();
        }

        // Configure authentication with support for both IAP and cookies
        // Skip IAP configuration in Testing environment - tests will configure their own
        if (!CurrentEnvironment.IsEnvironment("Testing"))
        {
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
            //Add JWT Bearer authentication for API requests
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["JWTSettings:validIssuer"],
                    ValidAudience = Configuration["JWTSettings:validAudienceDev"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret))
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
                
                // Configure user impersonation settings
                options.EnableImpersonation = iapConfig.GetValue<bool>("EnableImpersonation", false);
                options.ImpersonationHeaderName = iapConfig.GetValue<string>("ImpersonationHeaderName", "x-unops-impersonated-user");
                
                // Load trusted service accounts list
                options.TrustedServiceAccounts = new List<string>();
                var trustedAccounts = iapConfig.GetSection("TrustedServiceAccounts");
                if (trustedAccounts.Exists())
                {
                    foreach (var child in trustedAccounts.GetChildren())
                    {
                        var account = child.Value;
                        if (!string.IsNullOrEmpty(account))
                        {
                            options.TrustedServiceAccounts.Add(account);
                        }
                    }
                }
                
                // Also add DefaultServiceAccount if specified
                var defaultServiceAccount = iapConfig.GetValue<string>("DefaultServiceAccount", "");
                if (!string.IsNullOrEmpty(defaultServiceAccount) && 
                    !options.TrustedServiceAccounts.Contains(defaultServiceAccount))
                {
                    options.TrustedServiceAccounts.Add(defaultServiceAccount);
                }
            });
        }

        services.AddAuthorization();

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

        // Add SavedFilter Service
        services.AddScoped<ISavedFilterService, SavedFilterService>();
        
        // Configure authorization
        services.AddAuthorization(options =>
        {
            // Set default policy to accept IAP authentication
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes("IAP", "Bearer")
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
        
        // Register OrgUnit filtering services
        services.AddScoped<IUserPreferenceService, UserPreferenceService>();
        services.AddScoped<IOrgUnitHierarchyService, OrgUnitHierarchyService>();
        services.AddScoped<IOrgUnitFilterService, OrgUnitFilterService>();
        
        // Register User Profile Cache service for optimizing ChatWithGemini performance
        services.AddScoped<IUserProfileCacheService, UserProfileCacheService>();
        
        // Register Screen Context Cache service for optimizing ChatWithGemini performance
        services.AddScoped<IScreenContextCacheService, ScreenContextCacheService>();
        
        // Register Geo Time Cache service for optimizing ChatWithGemini performance
        services.AddScoped<IGeoTimeCacheService, GeoTimeCacheService>();
        
        // Register Dashboard service for user-specific filtering
        // Uses lightweight projection models and optimized queries for high performance
        services.AddScoped<IDashboardService, DashboardService>();

        // Register OrganizationHierarchy manager
        services.AddScoped<IOrganizationHierarchyManager, OrganizationHierarchyManager>();
        
        // Register Permission service for access control
        services.AddScoped<IPermissionService, PermissionService>();
        
        // Register Secure Specification Factory for RBAC-aware database filtering
        services.AddScoped<ISecureSpecificationFactory, SecureSpecificationFactory>();
        
        // Register Exchange Rate Service for currency conversion
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        
        // Register Google Credential for AI services
        services.AddSingleton<GoogleCredential>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var credentialParams = configuration.GetSection("AISettings")
                .Get<JsonCredentialParameters>();
            if (credentialParams == null)
                throw new Exception("AISettings configuration is missing.");
        
            var secretName = configuration.GetValue<string>("AISettings:AIServiceAccountJSONSecretName");
            
            var basicProvider = new GoogleSecretManagerConfigurationProvider(credentialParams.ProjectId);
            var secretValue = basicProvider.GetSecretVersion(secretName, "latest");
            return GoogleCredential.FromJson(secretValue);
        });
        
        // Register AI Contextual Service for similarity search and embeddings
        services.AddScoped<AiContextualService>();
        
        // Register Advanced Search Service for enhanced search capabilities
        services.AddScoped<AdvancedSearchService>();
        
        // Register Global Filter Service for centralized global filter logic
        services.AddScoped<GlobalFilterService>();
        
        // Register External API configuration for AI retriever services
        services.Configure<UNOPS.PAO.Models.Configuration.ExternalApiSettings>(
            Configuration.GetSection("ExternalApiSettings"));
        
        // Register IAP authentication helper for service account impersonation (singleton for token caching)
        services.AddSingleton<IAPAuthHelper>();
        
        // Register AI Retriever Manager for external API calls with shared authentication
        services.AddScoped<IAiRetrieverManager, AiRetrieverManager>();
        
        // Data seeding is now triggered manually via API endpoint: POST /api/system-admin/seeding/run
        // services.AddDataSeeding(); // REMOVED - no longer runs on startup

        // Register HttpContextAccessor for accessing request context in managers
        services.AddHttpContextAccessor();
        
        
        //services.AddScoped<IManagerWrapper, ManagerWrapper>();
        services.AddScoped<IManagerWrapper, UNOPSManagerWrapper>();



        AddServices(services);
        services.AddScoped<IGoogleDriveDocumentManager, GoogleDriveDocumentManager>();
        services.AddScoped<GoogleCloudStorageService>();
        
        if (!CurrentEnvironment.IsEnvironment("Testing"))
        {
            ApplyMigrations(services);
        }
        
        services.SeedAsync();
        ConfigureRegisters(services);
        
        if (!CurrentEnvironment.IsEnvironment("Testing"))
        {
            services.AddHostedService<PubSubPullService>(); // Register your background service
            services.AddHostedService<DueDiligenceNotificationService>(); // Register due diligence notification service
        }
        
        // Register URL service for building entity URLs
        services.AddScoped<IUrlService, UrlService>();
        
        // Register PAO email services (MailKit-based)
        services.AddEmailServices(Configuration);
        
        // Register PAO email sender
        services.AddScoped<PAOEmailSender>();
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
        string? connectionString = !CurrentEnvironment.IsDevelopment() && !CurrentEnvironment.IsEnvironment("Testing") ? GetConnectionStringFromSecretManager() : Configuration.GetConnectionString("DbContext");

        if (connectionString == null)
            throw new Exception("Connection string cannot be null. " +
                                $"Please set it up under in appsettings.{CurrentEnvironment.EnvironmentName}.json under ConnectionStrings. " +
                                $"Current environment: {CurrentEnvironment.EnvironmentName}.");

        // Check if IAM authentication is enabled (ONLY for local development)
        // In Dev/QA/Prod, connection strings from Secret Manager already have proper credentials
        var connectionStringsSection = Configuration.GetSection("ConnectionStrings");
        var useIamAuth = CurrentEnvironment.IsDevelopment() && 
                         connectionStringsSection.GetValue<bool>("UseIamAuthentication", false);
        DataAccess.Services.CloudSqlIamAuthProvider.IsEnabled = useIamAuth;

        // OPTIMIZE: Configure connection pool for better concurrency
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            MinPoolSize = 10,              // Keep connections warm
            MaxPoolSize = 100,             // Allow more concurrent connections
            ConnectionLifetime = 300,       // 5 minutes
            ConnectionIdleLifetime = 60,    // 1 minute idle timeout
            CommandTimeout = 60,            // Increase timeout for complex queries
            Timeout = 30,                   // Connection timeout
            ReadBufferSize = 16384,        // 16KB read buffer
            WriteBufferSize = 16384        // 16KB write buffer
            // Note: Multiplexing and KeepAlive are incompatible, disabled for IAM auth compatibility
        };
        
        // CRITICAL: Remove password from connection string when using IAM authentication
        // Npgsql requires no password set when using periodic password provider
        // Setting Password to null ensures it's not included in the connection string
        // Also handle case where Password might be empty string in original connection string
        if (useIamAuth)
        {
            // Explicitly set to null to ensure it's removed from connection string
            connectionStringBuilder.Password = null;
        }
        else if (string.IsNullOrEmpty(connectionStringBuilder.Password))
        {
            // If IAM auth is not enabled but no password is provided, this is an error
            throw new InvalidOperationException(
                "No password provided in connection string and IAM authentication is disabled. " +
                "Either provide a password in the connection string or enable IAM authentication " +
                "by setting ConnectionStrings:UseIamAuthentication to true in appsettings.json");
        }
        
        var optimizedConnectionString = connectionStringBuilder.ToString();

        // Configure Npgsql data source with optional IAM authentication
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(optimizedConnectionString);
        if (useIamAuth)
        {
            // Use IAM authentication - password is generated dynamically via OAuth2 token
            // The callback receives connection parameters but we use the static provider
            dataSourceBuilder.UsePeriodicPasswordProvider(
                async (connStringBuilder, ct) =>
                {
                    var password = await DataAccess.Services.CloudSqlIamAuthProvider.ProvidePasswordAsync(
                        connStringBuilder.Host ?? "",
                        connStringBuilder.Port,
                        connStringBuilder.Database ?? "",
                        connStringBuilder.Username ?? "",
                        ct);
                    
                    if (string.IsNullOrEmpty(password))
                    {
                        throw new InvalidOperationException(
                            "IAM authentication is enabled but failed to obtain access token. " +
                            "Ensure Application Default Credentials are configured (run 'gcloud auth application-default login').");
                    }
                    
                    return password;
                },
                TimeSpan.FromMinutes(55),  // Refresh token every 55 minutes (tokens expire in 60)
                TimeSpan.FromSeconds(10)   // Retry interval on failure
            );
        }
        var dataSource = dataSourceBuilder.Build();
        
        // Core DB context
        services.AddDbContext<DataAccess.Context.AppDbContext>(options =>
            options
                .UseNpgsql(dataSource)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

        // Override / UNOPS DB context
        services.AddDbContext<UNOPSAppDbContext>(options =>
            options
                .UseNpgsql(dataSource)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

        // Register IDbContextFactory for UNOPSAppDbContext
        // Used for parallel query execution (thread-safe DbContext instances)
        services.AddDbContextFactory<UNOPSAppDbContext>(options =>
            options
                .UseNpgsql(dataSource)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

        // Register IDbContextFactory for AppDbContext (base context)
        // Used by workflow adapters to avoid DbContext concurrency issues
        services.AddDbContextFactory<DataAccess.Context.AppDbContext>(options =>
            options
                .UseNpgsql(dataSource)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

        services.AddDbContext<PAOIdentityDbContext>(options =>
            options
                .UseNpgsql(dataSource)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

        // PERFORMANCE: Add DbContextFactory for PAOIdentityDbContext to support thread-safe parallel operations
        // This allows code that needs to run identity queries in parallel to create separate context instances
        services.AddDbContextFactory<PAOIdentityDbContext>(options =>
            options
                .UseNpgsql(dataSource)
                .ReplaceService<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>());

        // ==========================================
        // Workflow Submodule - DbContext and Services
        // ==========================================
        // Registers WorkflowDbContext with a separate "workflow" schema.
        // Auto-creates schema and applies migrations on startup (like Hangfire).
        // Uses the same connection string as the main AppDbContext.
        // Also registers PAO-specific implementations:
        // - PaoWorkflowUserContext (IWorkflowUserContext)
        // - PaoEntityStageProvider (IEntityStageProvider)
        // - PaoWorkflowApproverProvider (IWorkflowApproverProvider)
        // - PaoWorkflowNotificationService (IWorkflowNotificationService)
        services.AddPaoWorkflowServices(options =>
        {
            options.UsePostgreSqlStorage(optimizedConnectionString, "workflow");
        });

        // Override WorkflowDbContext registration to use dataSource (with IAM auth support)
        // This ensures WorkflowDbContext uses the same IAM authentication as other DbContexts
        services.AddDbContext<UNOPS.Workflow.DataAccess.WorkflowDbContext>(options =>
            options
                .UseNpgsql(dataSource, npgsql =>
                {
                    npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "workflow");
                }));

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