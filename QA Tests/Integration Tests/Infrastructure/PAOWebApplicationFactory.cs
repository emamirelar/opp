using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using UNOPS.PAO.Identity.Security;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSIdentity.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Microsoft.AspNetCore.Builder;
using UNOPS.PAO.UNOPSBusiness.Services;
using System.Linq;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.Server;
using Lamar;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Identity.Entities;
using Google.Apis.Auth.OAuth2;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;
using UNOPS.PAO.DataAccess.Interfaces;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.MailSender.Interfaces;
#if WORKFLOW_AVAILABLE
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Models.Requirements;
#endif
using Microsoft.AspNetCore.Authorization;
using Lamar.Microsoft.DependencyInjection;

namespace UNOPS.PAO.IntegrationTests.Infrastructure;

public class PAOWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    /// <summary>
    /// True when the factory successfully connected to a real PostgreSQL database
    /// (with table-level permissions). False when falling back to InMemory.
    /// Tests that require pg_trgm or raw SQL should skip when this is false.
    /// </summary>
    public bool IsUsingPostgres { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set the environment first
        builder.UseEnvironment("Testing");
        
        // Add test configuration — use the test assembly directory (not content root) so we load
        // OUR appsettings.Testing.json, not the server project's version of the same file.
        var testAssemblyDir = Path.GetDirectoryName(typeof(PAOWebApplicationFactory<>).Assembly.Location)!;
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(Path.Combine(testAssemblyDir, "appsettings.Testing.json"), optional: false, reloadOnChange: false);
            
            // Only override settings that are not already provided by appsettings.Testing.json
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:UseIamAuthentication"] = "true",
                ["GOOGLE_CLOUD_PROJECT"] = "test-project",
                ["Vertex AI Model"] = "gemini-1.5-pro-002",
                ["AISettings:DisableExternalCalls"] = "true",
                ["IAP:RequireJwtVerification"] = "false"
            });
        });
        
        // Use the actual startup class
        builder.UseStartup<Startup>();
        
        builder.ConfigureTestServices(services =>
        {
            // ============================================================
            // AUTHENTICATION OVERRIDE
            // Must run in ConfigureTestServices (Phase 3, AFTER Startup)
            // so it overrides whatever Startup configured, including
            // AddIdentityCore().AddApiEndpoints() which re-registers
            // default authentication schemes.
            // ============================================================
            RemoveAuthenticationServices(services);
            
            // ================================================================
            // CRITICAL: Register TestAuthHandler for BOTH "IAP" AND
            // IdentityConstants.ApplicationScheme ("Identity.Application").
            //
            // The production PermissionPolicyProvider (the last-registered
            // IAuthorizationPolicyProvider in Lamar's DI chain after its scan)
            // hardcodes IdentityConstants.ApplicationScheme in its
            // GetDefaultPolicyAsync() return value.  When UseAuthorization
            // re-authenticates via that scheme and finds no cookie, it resets
            // context.User to an unauthenticated ClaimsPrincipal, which causes
            // PAOExecutionContext to cache empty UserPermissions → 403 on all
            // endpoints decorated with [AccessControlled].
            //
            // By also hooking TestAuthHandler onto the ApplicationScheme,
            // context.AuthenticateAsync("Identity.Application") succeeds and
            // context.User is set to the test principal (NameIdentifier=123),
            // which lets PAOExecutionContext find the seeded user and its role
            // claims (all permissions seeded in SeedIdentityUser).
            // ================================================================
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "IAP";
                options.DefaultChallengeScheme = "IAP";
                options.DefaultScheme = "IAP";
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("IAP", options => { })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(IdentityConstants.ApplicationScheme, options => { });
            
            // ============================================================
            // DATABASE: Prefer real PostgreSQL; fall back to InMemory.
            //
            // Startup.ConfigureContainer() SKIPS ConfigureDataAccess() when environment
            // is "Testing", so we must register DbContexts here.
            //
            // Authentication strategy (mirrors Startup.cs):
            //   1. If UseIamAuthentication=true in config → build NpgsqlDataSource with
            //      PeriodicPasswordProvider using CloudSqlIamAuthProvider (same as Startup.cs)
            //   2. If $env:PGPASSWORD is set → inject it into connection string (password auth)
            //   3. Otherwise → use connection string as-is (proxy with --auto-iam-authn)
            //
            // Fallback: InMemory database when PostgreSQL is unreachable.
            // ============================================================
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DbContext")
                ?? config.GetConnectionString("DefaultConnection");

            var configSection = config.GetSection("ConnectionStrings");
            var useIamAuth = configSection.GetValue<bool>("UseIamAuthentication", true);

            // Inject PGPASSWORD env var when password auth is used (non-IAM).
            var pgPassword = System.Environment.GetEnvironmentVariable("PGPASSWORD");
            if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(pgPassword))
            {
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
                builder.Password = pgPassword;
                connectionString = builder.ConnectionString;
            }

            // Build NpgsqlDataSource with IAM auth token.
            // Strategy: read a pre-generated gcloud token from %TEMP%\gcloud_token.txt
            // (fast, no async race) and inject it as the password. If not available,
            // fall back to ADC via PeriodicPasswordProvider.
            //
            // To generate the token before running tests:
            //   gcloud auth print-access-token > %TEMP%\gcloud_token.txt
            Npgsql.NpgsqlDataSource? dataSource = null;
            var usePostgres = false;
            var factoryLog = Path.Combine(Path.GetTempPath(), "factory_probe.log");
            void Log(string msg) { File.AppendAllText(factoryLog, $"{DateTime.Now:HH:mm:ss} {msg}\n"); }
            File.WriteAllText(factoryLog, $"--- Factory probe started {DateTime.Now} ---\n");
            Log($"ConnectionString: {connectionString?.Substring(0, Math.Min(80, connectionString?.Length ?? 0))}...");
            Log($"UseIamAuth: {useIamAuth}");

            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    var connBuilder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
                    {
                        Timeout = 15,
                        MinPoolSize = 2,
                        MaxPoolSize = 20
                    };

                    if (useIamAuth)
                    {
                        var tokenFile = Path.Combine(Path.GetTempPath(), "gcloud_token.txt");
                        Log($"Token file: {tokenFile}, exists: {File.Exists(tokenFile)}");
                        string? token = null;
                        if (File.Exists(tokenFile))
                        {
                            token = File.ReadAllText(tokenFile).Trim();
                            Log($"Token length: {token.Length}");
                            if (token.Length > 50)
                            {
                                connBuilder.Password = token;
                                Log("Injected file token as password");
                            }
                            else
                            {
                                token = null;
                            }
                        }

                        // Fall back to ADC token if no file token
                        if (string.IsNullOrEmpty(token))
                        {
                            try
                            {
                                UNOPS.PAO.DataAccess.Services.CloudSqlIamAuthProvider.IsEnabled = true;
                                var adcToken = UNOPS.PAO.DataAccess.Services.CloudSqlIamAuthProvider
                                    .ProvidePasswordAsync("127.0.0.1", 5432,
                                        connBuilder.Database ?? "", connBuilder.Username ?? "")
                                    .AsTask().GetAwaiter().GetResult();
                                if (!string.IsNullOrEmpty(adcToken))
                                {
                                    connBuilder.Password = adcToken;
                                    Console.WriteLine("[PAOWebApplicationFactory] Using ADC token");
                                }
                            }
                            catch (Exception adcEx)
                            {
                                Console.WriteLine($"[PAOWebApplicationFactory] ADC token failed: {adcEx.Message}");
                            }
                        }
                    }

                    var dsBuilder = new Npgsql.NpgsqlDataSourceBuilder(connBuilder.ConnectionString);

                    if (useIamAuth && string.IsNullOrEmpty(connBuilder.Password))
                    {
                        // Last resort: PeriodicPasswordProvider (may have timing issues)
                        UNOPS.PAO.DataAccess.Services.CloudSqlIamAuthProvider.IsEnabled = true;
                        dsBuilder.UsePeriodicPasswordProvider(
                            async (settings, ct) =>
                            {
                                return await UNOPS.PAO.DataAccess.Services.CloudSqlIamAuthProvider
                                    .ProvidePasswordAsync(
                                        settings.Host ?? "",
                                        settings.Port,
                                        settings.Database ?? "",
                                        settings.Username ?? "",
                                        ct) ?? "";
                            },
                            TimeSpan.FromMinutes(55),
                            TimeSpan.FromSeconds(10));
                    }

                    dataSource = dsBuilder.Build();

                    // Probe: verify we can actually connect and query
                    using var probe = dataSource.OpenConnection();
                    using var cmd = probe.CreateCommand();
                    cmd.CommandText = "SELECT 1 FROM public.\"UserProfile\" LIMIT 1";
                    cmd.ExecuteNonQuery();
                    probe.Close();
                    usePostgres = true;
                    Log("PROBE SUCCEEDED — using real PostgreSQL");
                }
                catch (Exception ex)
                {
                    Log($"PROBE FAILED: {ex.GetType().Name}: {ex.Message}");
                    if (ex.InnerException != null)
                        Log($"  Inner: {ex.InnerException.Message}");
                    dataSource?.Dispose();
                    dataSource = null;
                    usePostgres = false;
                }
            }

            IsUsingPostgres = usePostgres;

            // Remove any stale DbContext registrations
            services.RemoveAll<DbContextOptions<UNOPSAppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions<PAOIdentityDbContext>>();
            services.RemoveAll<IDbContextFactory<UNOPSAppDbContext>>();
            services.RemoveAll<IDbContextFactory<AppDbContext>>();
            services.RemoveAll<IDbContextFactory<PAOIdentityDbContext>>();

            if (usePostgres && dataSource != null)
            {
                // ✅ Real PostgreSQL via NpgsqlDataSource (with IAM auth when enabled)
                services.AddSingleton(dataSource);
                services.AddDbContext<UNOPSAppDbContext>(options =>
                {
                    options.UseNpgsql(dataSource);
                    options.EnableSensitiveDataLogging();
                });
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseNpgsql(dataSource);
                    options.EnableSensitiveDataLogging();
                });
                services.AddDbContextFactory<UNOPSAppDbContext>(options =>
                {
                    options.UseNpgsql(dataSource);
                    options.EnableSensitiveDataLogging();
                });
                services.AddDbContextFactory<AppDbContext>(options =>
                {
                    options.UseNpgsql(dataSource);
                    options.EnableSensitiveDataLogging();
                });
                services.AddDbContext<PAOIdentityDbContext>(options =>
                {
                    options.UseNpgsql(dataSource);
                    options.EnableSensitiveDataLogging();
                });
            }
            else
            {
                // ⚠️ InMemory fallback — PostgreSQL-specific features (pg_trgm, etc.) will
                // return 500. Start Cloud SQL proxy and ensure IAM credentials are valid
                // to enable the full test suite.
                var dbName = $"TestDb_{Guid.NewGuid()}";
                services.AddDbContext<UNOPSAppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                    options.EnableSensitiveDataLogging();
                });
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"{dbName}_Core");
                    options.EnableSensitiveDataLogging();
                });
                services.AddDbContextFactory<UNOPSAppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                    options.EnableSensitiveDataLogging();
                });
                services.AddDbContextFactory<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"{dbName}_Core");
                    options.EnableSensitiveDataLogging();
                });
                services.AddDbContext<PAOIdentityDbContext>(options =>
                {
                    options.UseInMemoryDatabase($"{dbName}_Identity");
                    options.EnableSensitiveDataLogging();
                });
            }

            // Add basic services for tests
            services.AddLogging();
            services.AddMemoryCache();
            
            // Ensure controllers are added from the correct assemblies
            services.AddControllers()
                .AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly)
                .AddApplicationPart(typeof(UNOPSPresentation.AssemblyReference).Assembly);
            
            // Add routing
            services.AddRouting();
            
            // Replace OrgUnitHierarchyService with test-friendly implementation
            services.RemoveAll<IOrgUnitHierarchyService>();
            services.AddScoped<IOrgUnitHierarchyService, TestOrgUnitHierarchyService>();
            
            // Replace PermissionService with test implementation
            services.RemoveAll<IPermissionService>();
            services.AddScoped<IPermissionService, TestPermissionService>();
            
            // Register mock Google Credential for AI services
            services.RemoveAll<GoogleCredential>();
            services.AddSingleton<GoogleCredential>(sp => MockGoogleCredential.Create());
            
            // Register mock cache services to avoid external dependencies
            services.RemoveAll<IUserProfileCacheService>();
            services.AddScoped<IUserProfileCacheService, MockUserProfileCacheService>();
            
            services.RemoveAll<IScreenContextCacheService>();
            services.AddScoped<IScreenContextCacheService, MockScreenContextCacheService>();
            
            services.RemoveAll<IGeoTimeCacheService>();
            services.AddScoped<IGeoTimeCacheService, MockGeoTimeCacheService>();
            
            // Register mock UserInfoService
            services.RemoveAll<IUserInfoService>();
            services.AddScoped<IUserInfoService, MockUserInfoService>();
            
            // Register mock UserPreferenceService (real service needs PostgreSQL; InMemory throws 500)
            services.RemoveAll<IUserPreferenceService>();
            services.AddScoped<IUserPreferenceService, MockUserPreferenceService>();
            
            // Register mock AiContextualService to avoid Vertex AI dependency
            services.RemoveAll<AiContextualService>();
            services.AddScoped<AiContextualService>(sp => 
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var context = sp.GetRequiredService<UNOPSAppDbContext>();
                var credential = sp.GetRequiredService<GoogleCredential>();
                var aiPromptCache = sp.GetService<IAiPromptCacheService>();
                return new AiContextualService(config, context, credential, aiPromptCache);
            });
            
            // ============================================================
            // FIX: Override authorization policy provider and execution context.
            //
            // In production, PermissionPolicyProvider creates authorization policies
            // requiring the "Identity.Application" (cookie) auth scheme. Since tests
            // authenticate via TestAuthHandler on the "IAP" scheme, the cookie scheme
            // has no valid ticket and the middleware returns 401 Unauthorized before
            // the permission handler is ever invoked.
            //
            // Additionally, PAOExecutionContext resolves permissions from Identity
            // role/claim mappings which are empty in the InMemory identity store,
            // so PermissionHandler always calls context.Fail() → 403 Forbidden.
            //
            // These two replacements fix 886+ test failures (572 × 401 + 314 × 403).
            // ============================================================
            services.RemoveAll<IAuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationPolicyProvider>(sp =>
                new MockServices.TestPermissionPolicyProvider());
            
            services.RemoveAll<IPAOExecutionContext>();
            services.AddScoped<IPAOExecutionContext>(sp =>
                new MockServices.TestPAOExecutionContext());
            
            // ============================================================
            // FIX: Override IAuthorizationService to bypass PAOAuthorizationService.
            //
            // PAOAuthorizationService manually iterates IAuthorizationHandler
            // instances but only the PermissionHandler and EntityPermissionHandler
            // are registered.  Standard requirements like
            // DenyAnonymousAuthorizationRequirement have no handler, so every
            // request gets 403 Forbidden even with a valid authenticated user.
            //
            // TestAuthorizationService simply succeeds for authenticated users
            // and fails for anonymous ones, which is appropriate for integration
            // tests where permission-level checks are handled by
            // TestPermissionService.
            // ============================================================
            services.RemoveAll<IAuthorizationService>();
            services.AddScoped<IAuthorizationService, MockServices.TestAuthorizationService>();
            
            // Ensure GlobalFilterService is registered
            services.RemoveAll<GlobalFilterService>();
            services.AddScoped<GlobalFilterService>();
            
            // Ensure AdvancedSearchService is registered  
            services.RemoveAll<AdvancedSearchService>();
            services.AddScoped<AdvancedSearchService>();
            
            // Add HttpClient for services that need it
            services.AddHttpClient();
            
#if WORKFLOW_AVAILABLE
            // Register workflow services (skipped in Startup.cs for Testing environment
            // because AddPaoWorkflowServices eagerly connects to PostgreSQL for migrations).
            // Register mock/in-memory implementations instead.
            services.RemoveAll<IWorkflowManager>();
            services.AddScoped<IWorkflowManager>(sp => new Mock<IWorkflowManager>().Object);
            
            services.RemoveAll<IWorkflowRepository>();
            services.AddScoped<IWorkflowRepository>(sp => new Mock<IWorkflowRepository>().Object);
            
            services.RemoveAll<IWorkflowUserContext>();
            services.AddScoped<IWorkflowUserContext>(sp => new Mock<IWorkflowUserContext>().Object);
            
            services.RemoveAll<IEntityStageProvider>();
            services.AddScoped<IEntityStageProvider>(sp => new Mock<IEntityStageProvider>().Object);
            
            services.RemoveAll<IWorkflowApproverProvider>();
            services.AddScoped<IWorkflowApproverProvider>(sp => new Mock<IWorkflowApproverProvider>().Object);
            
            services.RemoveAll<IPaoWorkflowApproverProvider>();
            services.AddScoped<IPaoWorkflowApproverProvider>(sp => new Mock<IPaoWorkflowApproverProvider>().Object);
            
            services.RemoveAll<IWorkflowNotificationService>();
            services.RemoveAll<PaoWorkflowNotificationService>();
            services.AddScoped<PaoWorkflowNotificationService>(sp =>
            {
                var emailSender = new Mock<IEmailSender>().Object;
                var dbContextFactory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
                var config = sp.GetRequiredService<IConfiguration>();
                var logger = sp.GetRequiredService<ILogger<PaoWorkflowNotificationService>>();
                var appContext = sp.GetRequiredService<AppDbContext>();
                var userResolver = sp.GetRequiredService<UserResolverService<int>>();
                var notifManager = new NotificationManager(appContext, userResolver);
                var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                return new PaoWorkflowNotificationService(
                    emailSender, dbContextFactory, serviceScopeFactory, logger, config, notifManager);
            });
            services.AddScoped<IWorkflowNotificationService>(sp =>
                sp.GetRequiredService<PaoWorkflowNotificationService>());
            
            services.RemoveAll<IStageRequirementsProvider>();
            services.AddScoped<IStageRequirementsProvider>(sp => new Mock<IStageRequirementsProvider>().Object);
            
            // Register in-memory WorkflowDbContext (skipped in Startup for Testing)
            services.RemoveAll<DbContextOptions<WorkflowDbContext>>();
            services.AddDbContext<WorkflowDbContext>(options =>
                options.UseInMemoryDatabase($"{Guid.NewGuid()}_Workflow"));
#endif
        });
    }
    
    /// <summary>
    /// Creates an HttpClient with IAP authentication headers pre-configured.
    /// Many test classes call Factory.CreateClient() instead of using the base
    /// class Client property, so this ensures all clients are authenticated by
    /// default. Tests that need unauthenticated access should call
    /// CreateUnauthenticatedClient() or clear the headers explicitly.
    /// </summary>
    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-Email", "accounts.google.com:testuser@unops.org");
        client.DefaultRequestHeaders.Add("X-Goog-Authenticated-User-ID", "accounts.google.com:123");
        client.DefaultRequestHeaders.Add("Cookie", "DevIAPAuth=testuser@unops.org; dev-user-email=testuser@unops.org");
        return client;
    }

    /// <summary>
    /// Creates an HttpClient with PARTNER_GLOB_ADMIN role for admin endpoints.
    /// Use for entities that require CheckRoleAuthorizationAsync(BaseRole.PARTNER_GLOB_ADMIN).
    /// </summary>
    public HttpClient CreateAuthenticatedAdminClient()
    {
        var client = CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Add("Test-Role", "PARTNER_GLOB_ADMIN");
        return client;
    }
    
    private void RemoveAuthenticationServices(IServiceCollection services)
    {
        // Remove all authentication-related services
        var authenticationServiceDescriptors = services
            .Where(d => d.ServiceType.Namespace != null && 
                       (d.ServiceType.Namespace.Contains("Authentication") ||
                        d.ServiceType.Name.Contains("Authentication") ||
                        d.ServiceType.Name.Contains("AuthenticationScheme")))
            .ToList();

        foreach (var descriptor in authenticationServiceDescriptors)
        {
            services.Remove(descriptor);
        }
        
        // Also remove specific authentication services
        services.RemoveAll<IAuthenticationService>();
        services.RemoveAll<IAuthenticationHandlerProvider>();
        services.RemoveAll<IAuthenticationSchemeProvider>();
        services.RemoveAll<IAuthenticationHandlerProvider>();
        services.RemoveAll<IOptionsMonitor<AuthenticationSchemeOptions>>();
    }

    private void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        
        // ====================================================================
        // POST-BUILD DI OVERRIDE via Lamar container reconfiguration.
        //
        // Problem: Startup.ConfigureContainer runs WithDefaultConventions()
        // scan + explicit registrations AFTER ConfigureTestServices completes.
        // Lamar resolves the LAST registered implementation, so the scan and
        // explicit entries always override our ConfigureTestServices mocks for
        // these services:
        //
        //   • IAuthorizationPolicyProvider → PermissionPolicyProvider
        //       PermissionPolicyProvider.GetDefaultPolicyAsync() hardcodes
        //       IdentityConstants.ApplicationScheme (cookie). UseAuthorization
        //       re-authenticates via cookie → no cookie present → FAILS →
        //       context.User set to unauthenticated ClaimsPrincipal.
        //
        //   • IPAOExecutionContext → PAOExecutionContext
        //       Reads IHttpContextAccessor.HttpContext.User.FindFirst(
        //       NameIdentifier). When context.User is unauthenticated (above),
        //       NameIdentifier is null → UserPermissions = empty list (cached).
        //
        //   • IAuthorizationService → PAOAuthorizationService
        //       Uses PAOExecutionContext (empty permissions) and only iterates
        //       custom handlers (PermissionHandler, EntityPermissionHandler),
        //       missing DenyAnonymousAuthorizationRequirement handling.
        //
        //   • IPermissionService → PermissionService
        //       AccessControlledAttribute calls CanPerformActionAsync →
        //       PAOExecutionContext.UserPermissions (cached empty) → false → 403.
        //
        // Fix: after the Lamar container is fully built (scan already ran),
        // call container.Configure() to add our test registrations as the
        // NEWEST entries.  Lamar's "last wins" rule then picks our mocks for
        // ALL subsequent resolutions in this test run.
        // ====================================================================
        if (host.Services is Lamar.IContainer lamarContainer)
        {
            lamarContainer.Configure(registry =>
            {
                // Use standard IServiceCollection methods which work on Lamar's ServiceRegistry.
                // Adding registrations AFTER the container has been built makes them the
                // "last" entries for each interface.  Lamar resolves the last registration
                // when GetService<T>() is called, so these test overrides reliably win.

                // Authorization overrides (fixes 403 Forbidden failures caused by
                // PermissionPolicyProvider hardcoding IdentityConstants.ApplicationScheme)
                registry.AddSingleton<IAuthorizationPolicyProvider>(
                    _ => new TestPermissionPolicyProvider());
                registry.AddScoped<IPAOExecutionContext>(
                    _ => new TestPAOExecutionContext());
                registry.AddScoped<IAuthorizationService, TestAuthorizationService>();
                registry.AddScoped<IPermissionService, TestPermissionService>();

                // GoogleCredential override: Startup.ConfigureContainer line ~457 registers
                // a factory that calls Google Secret Manager (GoogleCredential.FromJson).
                // This causes 500 for all controllers that transitively depend on
                // GoogleCredential (e.g. PartnerController → AiContextualService → GoogleCredential).
                registry.AddSingleton<GoogleCredential>(_ => MockGoogleCredential.Create());

                // AiContextualService override: Startup.ConfigureContainer also registers
                // AiContextualService explicitly.  Override it here to use MockGoogleCredential.
                registry.AddScoped<AiContextualService>(sp =>
                {
                    var cfg = sp.GetRequiredService<IConfiguration>();
                    var ctx = sp.GetRequiredService<UNOPSAppDbContext>();
                    var cred = sp.GetRequiredService<GoogleCredential>(); // resolves MockGoogleCredential
                    var cache = sp.GetService<IAiPromptCacheService>();
                    return new AiContextualService(cfg, ctx, cred, cache);
                });
            });
        }
        
        // Initialize databases.  EnsureCreated() is a no-op when schema already exists
        // (PostgreSQL) or creates the schema from the model (InMemory).  Seeding is
        // idempotent (checks before inserting) so repeated runs are safe.
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var unopsDb = services.GetRequiredService<UNOPSAppDbContext>();
            var coreDb = services.GetRequiredService<AppDbContext>();
            var identityDb = services.GetRequiredService<PAOIdentityDbContext>();
            
            try
            {
                // For PostgreSQL use EnsureCreated (migrations already applied by DBA / CI).
                // For InMemory this creates the full schema from the model.
                unopsDb.Database.EnsureCreated();
                coreDb.Database.EnsureCreated();
                identityDb.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                // EnsureCreated may fail on PostgreSQL if tables already exist with
                // different schema constraints. Safe to ignore — migrations handle schema.
                Console.WriteLine($"[PAOWebApplicationFactory] EnsureCreated warning: {ex.Message}");
            }
            
            try
            {
                SeedTestData(unopsDb, coreDb);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PAOWebApplicationFactory] SeedTestData warning: {ex.Message}");
            }
            
            try
            {
                SeedIdentityUser(services).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PAOWebApplicationFactory] SeedIdentityUser warning: {ex.Message}");
            }
        }
        
        return host;
    }
    
    private void SeedTestData(UNOPSAppDbContext unopsDb, AppDbContext coreDb)
    {
        // Ensure test user exists in UNOPS UserProfile.
        // UserProfile.Name is a read-only computed property (hides base via 'new'),
        // so EF Core excludes it from INSERTs. Use raw SQL to satisfy the NOT NULL constraint.
        var userInfos = unopsDb.UserProfile.FirstOrDefault(u => u.UserEmail == "testuser@unops.org");
        if (userInfos == null)
        {
            unopsDb.Database.ExecuteSqlRaw(@"
                INSERT INTO public.""UserProfile""
                    (""Id"", ""UserId"", ""UserEmail"", ""FirstName"", ""LastName"", ""Name"",
                     ""OrgUnit"", ""IsDeleted"", ""CreatedBy"", ""CreatedDate"",
                     ""LastModifiedBy"", ""LastModifiedDate"", ""Status"", ""WorkflowStatus"")
                VALUES
                    (0, 123, 'testuser@unops.org', 'Test', 'User', 'Test User',
                     'HQ', false, 0, NOW(), 0, NOW(), 0, 0)
                ON CONFLICT DO NOTHING");
        }

        // Ensure PAOUser exists in AppDbContext for ProfileManager (POST /api/profile)
        var paoUser = coreDb.PAOUsers.FirstOrDefault(u => u.Email == "testuser@unops.org");
        if (paoUser == null)
        {
            coreDb.PAOUsers.Add(new PAOUser
            {
                Id = 123,
                Email = "testuser@unops.org",
                IsInternal = true,
                ActiveUser = true,
            });
            coreDb.SaveChanges();
        }
        
        // Use TestDataSeeder for consistent data
        TestDataSeeder.SeedBasicData(unopsDb);
    }
    
    private async Task SeedIdentityUser(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<PAOIdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<PAOIdentityRole>>();

        // ---------------------------------------------------------------
        // STEP 1: Ensure UNOPS_GEN_USER role exists with ALL permissions.
        //
        // The real PAOExecutionContext reads permission claims from the
        // role. Seeding all Permission fields here ensures that even when
        // Lamar bypasses the TestPAOExecutionContext mock and resolves the
        // production PAOExecutionContext, every permission check passes.
        // ---------------------------------------------------------------
        if (!await roleManager.RoleExistsAsync("UNOPS_GEN_USER"))
        {
            await roleManager.CreateAsync(new PAOIdentityRole { Name = "UNOPS_GEN_USER" });
        }

        var role = await roleManager.FindByNameAsync("UNOPS_GEN_USER");
        if (role != null)
        {
            var existingClaims = await roleManager.GetClaimsAsync(role);
            var seededPermissions = existingClaims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToHashSet();

            var allPermissionNames = typeof(Permission)
                .GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .Where(f => f.FieldType == typeof(Permission))
                .Select(f => ((Permission)f.GetValue(null)!).Name)
                .ToList();

            foreach (var name in allPermissionNames)
            {
                if (!seededPermissions.Contains(name))
                {
                    await roleManager.AddClaimAsync(role, new Claim("permission", name));
                }
            }
        }

        // ---------------------------------------------------------------
        // STEP 2: Resolve PostgreSQL identity-user conflicts.
        //
        // On the very first request of a test run, IAPVerificationMiddleware
        // creates "testuser@unops.org" with an auto-generated Id (e.g. 1)
        // BEFORE SeedIdentityUser runs for the first time.  On subsequent
        // factory initializations SeedIdentityUser's FindByIdAsync("123")
        // returns null → CreateAsync(Id=123) fails with a unique-email
        // constraint → error silently swallowed → the identity store has
        // the user at Id=1 but TestAuthHandler returns NameIdentifier="123"
        // → PAOExecutionContext.FindByIdAsync("123") → null → no perms → 403.
        //
        // Fix: check by EMAIL first; delete any conflicting entry; then
        // create (or confirm) the canonical Id=123 test user.
        // ---------------------------------------------------------------
        var userByEmail = await userManager.FindByEmailAsync("testuser@unops.org");
        var userById    = await userManager.FindByIdAsync("123");

        // If another user already owns the email, remove that conflicting entry.
        if (userByEmail != null && userByEmail.Id != 123)
        {
            await userManager.DeleteAsync(userByEmail);
            userByEmail = null;
        }

        // If another user already owns Id=123 but with wrong email, remove it.
        if (userById != null && !string.Equals(userById.Email, "testuser@unops.org",
                StringComparison.OrdinalIgnoreCase))
        {
            await userManager.DeleteAsync(userById);
            userById = null;
        }

        // Create the canonical test user when it does not yet exist.
        if (userByEmail == null && userById == null)
        {
            var user = new PAOIdentityUser
            {
                Id = 123,
                UserName = "testuser@unops.org",
                Email = "testuser@unops.org",
                EmailConfirmed = true,
                NormalizedEmail = "TESTUSER@UNOPS.ORG",
                NormalizedUserName = "TESTUSER@UNOPS.ORG",
                // UNOPSUserValidator rejects @unops.org users unless GoogleSignIn=true
                // or the request has an IAP header. SeedIdentityUser runs outside of
                // any HTTP request context so there is no IAP header — set this flag
                // to satisfy the validator. Note: GoogleSignIn is not persisted to
                // the database (it is Ignored in PAOIdentityDbContext) so it only
                // needs to be set for the duration of the CreateAsync() call.
                GoogleSignIn = true
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                Console.WriteLine($"[SeedIdentityUser] Warning: could not create test user: " +
                    $"{string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        // Ensure the test user has the UNOPS_GEN_USER role.
        var testUser = await userManager.FindByEmailAsync("testuser@unops.org");
        if (testUser != null)
        {
            var roles = await userManager.GetRolesAsync(testUser);
            if (!roles.Contains("UNOPS_GEN_USER"))
            {
                await userManager.AddToRoleAsync(testUser, "UNOPS_GEN_USER");
            }
        }
    }
    
}