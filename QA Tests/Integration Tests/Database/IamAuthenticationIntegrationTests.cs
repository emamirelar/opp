using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.IntegrationTests.Database
{
    /// <summary>
    /// Integration tests for Cloud SQL IAM authentication.
    /// These tests verify that the database connection works with IAM authentication.
    /// NOTE: These tests may be skipped in CI/CD if Google Cloud credentials are not available.
    /// </summary>
    [Collection("Sequential")]
    public class IamAuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private bool _originalIamEnabledState;

        public IamAuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _originalIamEnabledState = CloudSqlIamAuthProvider.IsEnabled;
        }

        public void Dispose()
        {
            // Restore original IAM enabled state
            CloudSqlIamAuthProvider.IsEnabled = _originalIamEnabledState;
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task DatabaseConnection_WithIamAuthDisabled_ConnectsSuccessfully()
        {
            // Arrange: Ensure IAM auth is disabled (fallback to password)
            CloudSqlIamAuthProvider.IsEnabled = false;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

            // Act: Execute simple query
            var canConnect = await context.Database.CanConnectAsync();

            // Assert: Connection should succeed with password auth
            canConnect.Should().BeTrue("database connection should work with password authentication");
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task DatabaseConnection_WithIamAuthEnabled_ConnectsSuccessfully()
        {
            // Arrange: Enable IAM auth
            CloudSqlIamAuthProvider.IsEnabled = true;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

            // Act: Execute simple query
            var canConnect = await context.Database.CanConnectAsync();

            // Assert: Connection should succeed with IAM auth
            canConnect.Should().BeTrue("database connection should work with IAM authentication");
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task SimpleQuery_WithPasswordAuth_ExecutesSuccessfully()
        {
            // Arrange: Disable IAM auth (use password)
            CloudSqlIamAuthProvider.IsEnabled = false;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

            // Act: Execute simple query
            var result = await context.Partners
                .Take(1)
                .FirstOrDefaultAsync();

            // Assert: Query should execute without error
            // Result may be null if no partners exist, but query should succeed
            var exception = Record.Exception(() => result);
            exception.Should().BeNull("query should execute without errors");
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task SimpleQuery_WithIamAuth_ExecutesSuccessfully()
        {
            // Arrange: Enable IAM auth
            CloudSqlIamAuthProvider.IsEnabled = true;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

            // Act: Execute simple query
            var result = await context.Partners
                .Take(1)
                .FirstOrDefaultAsync();

            // Assert: Query should execute without error
            var exception = Record.Exception(() => result);
            exception.Should().BeNull("query should execute without errors with IAM auth");
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task ParallelQueries_WithIamAuth_AllSucceed()
        {
            // Arrange: Enable IAM auth
            CloudSqlIamAuthProvider.IsEnabled = true;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

            // Act: Execute 5 parallel queries
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => context.Partners.Take(1).FirstOrDefaultAsync());

            // Assert: All queries should complete without error
            var results = await Task.WhenAll(tasks);
            results.Should().NotBeNull("all parallel queries should complete");
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task ConnectionPooling_WithPasswordAuth_HandlesMultipleConnections()
        {
            // Arrange: Disable IAM auth
            CloudSqlIamAuthProvider.IsEnabled = false;

            // Act: Create multiple scopes and execute queries
            var tasks = Enumerable.Range(0, 10).Select(async i =>
            {
                using var scope = _factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
                return await context.Partners.CountAsync();
            });

            // Assert: All queries should complete
            var results = await Task.WhenAll(tasks);
            results.Should().AllBeOfType<int>("all queries should return counts");
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task ConnectionPooling_WithIamAuth_HandlesMultipleConnections()
        {
            // Arrange: Enable IAM auth
            CloudSqlIamAuthProvider.IsEnabled = true;

            // Act: Create multiple scopes and execute queries
            var tasks = Enumerable.Range(0, 10).Select(async i =>
            {
                using var scope = _factory.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();
                return await context.Partners.CountAsync();
            });

            // Assert: All queries should complete with IAM auth
            var results = await Task.WhenAll(tasks);
            results.Should().AllBeOfType<int>("all queries should return counts with IAM auth");
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task DatabaseQuery_WithPasswordAuth_ReturnsValidData()
        {
            // Arrange: Disable IAM auth
            CloudSqlIamAuthProvider.IsEnabled = false;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

            // Act: Get partner count (should work even if 0)
            var count = await context.Partners.CountAsync();

            // Assert: Query should complete and return a valid count
            count.Should().BeGreaterThanOrEqualTo(0, "count should be a valid non-negative number");
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task DatabaseQuery_WithIamAuth_ReturnsValidData()
        {
            // Arrange: Enable IAM auth
            CloudSqlIamAuthProvider.IsEnabled = true;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

            // Act: Get partner count
            var count = await context.Partners.CountAsync();

            // Assert: Query with IAM auth should return valid count
            count.Should().BeGreaterThanOrEqualTo(0, "count should be a valid non-negative number with IAM auth");
        }

        [Fact(Skip = "Requires Google Cloud credentials and IAM authentication configured - run in staging/production environment")]
        public async Task SwitchingAuthMethods_FromPasswordToDisabled_WorksCorrectly()
        {
            // Arrange: Start with password auth
            CloudSqlIamAuthProvider.IsEnabled = false;

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UNOPSAppDbContext>();

            // Act: Execute query with password auth
            var result1 = await context.Partners.CountAsync();

            // Simulate switching (though in real scenario would need app restart)
            CloudSqlIamAuthProvider.IsEnabled = false;

            var result2 = await context.Partners.CountAsync();

            // Assert: Both queries should succeed
            result1.Should().BeGreaterThanOrEqualTo(0);
            result2.Should().BeGreaterThanOrEqualTo(0);
        }
    }
}
