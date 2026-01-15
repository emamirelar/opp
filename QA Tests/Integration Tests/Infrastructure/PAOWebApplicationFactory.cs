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
                ["Vertex AI Model"] = "gemini-1.5-pro-002"
            });
        });
        
        // Configure services BEFORE Startup to prevent authentication conflicts
        builder.ConfigureServices(services =>
        {
            // Remove all authentication-related services that might have been added
            RemoveAuthenticationServices(services);
            
            // Add our test authentication scheme
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
        });
        
        // Use the actual startup class but override services
        builder.UseStartup<Startup>();
        
        builder.ConfigureTestServices(services =>
        {
            // Remove existing DbContext registrations
            RemoveService<DbContextOptions<UNOPSAppDbContext>>(services);
            RemoveService<DbContextOptions<AppDbContext>>(services);
            RemoveService<DbContextOptions<PAOIdentityDbContext>>(services);
            
            // Add in-memory database for testing
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
            
            // Ensure GlobalFilterService is registered
            services.RemoveAll<GlobalFilterService>();
            services.AddScoped<GlobalFilterService>();
            
            // Ensure AdvancedSearchService is registered  
            services.RemoveAll<AdvancedSearchService>();
            services.AddScoped<AdvancedSearchService>();
            
            // Add HttpClient for services that need it
            services.AddHttpClient();
        });
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
        // Ensure test user exists
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