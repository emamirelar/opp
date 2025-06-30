using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace UNOPS.PAO.IntegrationTests.Infrastructure;

public class PAOWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
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

            // Override authentication with test handlers
            services.RemoveAll<IAuthenticationSchemeProvider>();
            services.AddAuthentication("Test")
                .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { })
                .AddScheme<TestAuthenticationSchemeOptions, TestAuthenticationHandler>("IAP", options => { });
        });

        builder.UseEnvironment("Testing");
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
        }
        
        return host;
    }
}

public class TestAuthenticationSchemeOptions : AuthenticationSchemeOptions { }

public class TestAuthenticationHandler : AuthenticationHandler<TestAuthenticationSchemeOptions>
{
    public const string TestUserId = "test-user-123";
    public const string TestUserEmail = "testuser@unops.org";
    public const string TestUserName = "Test User";

    public TestAuthenticationHandler(IOptionsMonitor<TestAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId),
            new Claim(ClaimTypes.Email, TestUserEmail),
            new Claim(ClaimTypes.Name, TestUserName),
            new Claim("sub", TestUserId),
            new Claim("email", TestUserEmail),
            new Claim("name", TestUserName)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}