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
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSIdentity.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.Server;
using Lamar;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Identity.Entities;
using Google.Apis.Auth.OAuth2;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.IntegrationTests.Infrastructure.MockServices;
using UNOPS.PAO.DataAccess.Interfaces;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Models.Requirements;
using Microsoft.AspNetCore.Authorization;

namespace UNOPS.PAO.IntegrationTests.Infrastructure;

public class PAOWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set the environment first
        builder.UseEnvironment("Testing");
        
        // Add test configuration
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Testing.json", optional: true, reloadOnChange: false);
            
            // Add in-memory configuration for tests
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
                ["ConnectionStrings:UseIamAuthentication"] = "false",
                ["GOOGLE_CLOUD_PROJECT"] = "test-project",
                ["Vertex AI Model"] = "gemini-1.5-pro-002",
                ["AISettings:DisableExternalCalls"] = "true"
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
            
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "IAP";
                options.DefaultChallengeScheme = "IAP";
                options.DefaultScheme = "IAP";
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("IAP", options => { })
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Events.OnRedirectToLogin = (context) =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });
            
            // Remove ALL existing DbContext and DbContextFactory registrations
            // (Startup.cs registers Npgsql versions; we replace with SQLite in-memory for tests)
            services.RemoveAll<DbContextOptions<UNOPSAppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<DbContextOptions<PAOIdentityDbContext>>();
            services.RemoveAll<IDbContextFactory<UNOPSAppDbContext>>();
            services.RemoveAll<IDbContextFactory<AppDbContext>>();
            services.RemoveAll<IDbContextFactory<PAOIdentityDbContext>>();
            
            // Remove ALL existing DbContext and DbContextFactory registrations
            // (Startup.cs registers Npgsql versions; we replace with InMemory for tests)
            //
            // NOTE: InMemory provider does NOT support relational features (raw SQL,
            // model finalization, GetRelationalModel). Services that use these features
            // (e.g. DuplicateDetectionService) will fail with 500 errors. A proper fix
            // requires either a test PostgreSQL instance or a carefully configured
            // SQLite database. See DEF-xxx in the defect list.
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
                return new PaoWorkflowNotificationService(
                    emailSender, dbContextFactory, logger, config, notifManager);
            });
            services.AddScoped<IWorkflowNotificationService>(sp =>
                sp.GetRequiredService<PaoWorkflowNotificationService>());
            
            services.RemoveAll<IStageRequirementsProvider>();
            services.AddScoped<IStageRequirementsProvider>(sp => new Mock<IStageRequirementsProvider>().Object);
            
            // Register in-memory WorkflowDbContext (skipped in Startup for Testing)
            services.RemoveAll<DbContextOptions<WorkflowDbContext>>();
            services.AddDbContext<WorkflowDbContext>(options =>
                options.UseInMemoryDatabase($"{Guid.NewGuid()}_Workflow"));
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
        
        // Initialize databases
        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var unopsDb = services.GetRequiredService<UNOPSAppDbContext>();
            var coreDb = services.GetRequiredService<AppDbContext>();
            var identityDb = services.GetRequiredService<PAOIdentityDbContext>();
            
            unopsDb.Database.EnsureCreated();
            coreDb.Database.EnsureCreated();
            identityDb.Database.EnsureCreated();
            
            // Seed basic data for tests
            SeedTestData(unopsDb, coreDb);
            
            // Seed identity user
            SeedIdentityUser(services).Wait();
        }
        
        return host;
    }
    
    private void SeedTestData(UNOPSAppDbContext unopsDb, AppDbContext coreDb)
    {
        // Ensure test user exists in UNOPS UserProfile
        var userInfos = unopsDb.UserProfile.FirstOrDefault(u => u.UserEmail == "testuser@unops.org");
        if (userInfos == null)
        {
            unopsDb.UserProfile.Add(new UserProfile
            {
                UserId = 123,
                UserEmail = "testuser@unops.org",
                FirstName = "Test User",
                OrgUnit = "HQ"
            });
            unopsDb.SaveChanges();
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
                UserProfile = new UserProfile
                {
                    UserId = 123,
                    UserEmail = "testuser@unops.org",
                    FirstName = "Test",
                    LastName = "User",
                    OrgUnit = "HQ"
                }
            });
            coreDb.SaveChanges();
        }
        
        // Use TestDataSeeder for consistent data
        TestDataSeeder.SeedBasicData(unopsDb);
    }
    
    private async Task SeedIdentityUser(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<PAOIdentityUser>>();
        var identityDb = services.GetRequiredService<PAOIdentityDbContext>();
        
        // Create test user if not exists
        var existingUser = await userManager.FindByIdAsync("123");
        if (existingUser == null)
        {
            var user = new PAOIdentityUser
            {
                Id = 123,
                UserName = "testuser@unops.org",
                Email = "testuser@unops.org",
                EmailConfirmed = true,
                NormalizedEmail = "TESTUSER@UNOPS.ORG",
                NormalizedUserName = "TESTUSER@UNOPS.ORG"
            };
            
            await userManager.CreateAsync(user);
        }
    }
    
}