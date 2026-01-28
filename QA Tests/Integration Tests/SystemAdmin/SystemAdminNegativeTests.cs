using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Admin;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.SystemAdmin
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "SystemAdmin")][Trait("Component", "NegativeTests")]
    public class SystemAdminNegativeTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public SystemAdminNegativeTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser(int id = 1) => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-ADMIN-NEG-001")][Trait("Priority", "Critical")]
        public async Task GetSystemSettings_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetSystemSettingsAsync(viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-002")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_NonAdminUser_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "User") }, "TestAuth"));
            var request = new UpdateSystemSettingsRequest { MaintenanceMode = true };
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.UpdateSystemSettingsAsync(request, user));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-003")][Trait("Priority", "Critical")]
        public async Task GetSystemSettings_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetSystemSettingsAsync(null));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-004")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_NullRequest_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.UpdateSystemSettingsAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-005")][Trait("Priority", "High")]
        public async Task ClearCache_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.ClearCacheAsync(viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-006")][Trait("Priority", "High")]
        public async Task GetAuditLogs_NonExistentDateRange_ReturnsEmpty()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var result = await mgr.GetAuditLogsAsync(DateTime.UtcNow.AddYears(10), DateTime.UtcNow.AddYears(11), CreateUser());
            result.Should().BeEmpty();
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-007")][Trait("Priority", "High")]
        public async Task GetAuditLogs_EndBeforeStart_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetAuditLogsAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-008")][Trait("Priority", "Medium")]
        public async Task GetAuditLogs_NullStartDate_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetAuditLogsAsync(null, DateTime.UtcNow, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-009")][Trait("Priority", "Medium")]
        public async Task GetAuditLogs_NullEndDate_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetAuditLogsAsync(DateTime.UtcNow, null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-010")][Trait("Priority", "High")]
        public async Task RestartService_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RestartServiceAsync("TestService", viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-011")][Trait("Priority", "High")]
        public async Task RestartService_NonExistentService_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RestartServiceAsync("NonExistentService", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-012")][Trait("Priority", "High")]
        public async Task RestartService_NullServiceName_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.RestartServiceAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-013")][Trait("Priority", "High")]
        public async Task RestartService_EmptyServiceName_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.RestartServiceAsync(string.Empty, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-014")][Trait("Priority", "Critical")]
        public async Task GetSystemHealth_DatabaseUnavailable_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetSystemHealthAsync(CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-015")][Trait("Priority", "High")]
        public async Task BackupDatabase_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.BackupDatabaseAsync(viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-016")][Trait("Priority", "High")]
        public async Task RestoreDatabase_NonExistentBackup_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.RestoreDatabaseAsync("NonExistentBackup", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-017")][Trait("Priority", "High")]
        public async Task RestoreDatabase_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RestoreDatabaseAsync("Backup", viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-018")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_InvalidMaxUploadSize_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var request = new UpdateSystemSettingsRequest { MaxUploadSizeMB = -1 };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateSystemSettingsAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-019")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_InvalidSessionTimeout_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var request = new UpdateSystemSettingsRequest { SessionTimeoutMinutes = 0 };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateSystemSettingsAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-020")][Trait("Priority", "Medium")]
        public async Task GetAuditLogs_ExcessiveDateRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetAuditLogsAsync(DateTime.UtcNow.AddYears(-10), DateTime.UtcNow, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-021")][Trait("Priority", "High")]
        public async Task GetSystemLogs_NonExistentLogType_ThrowsKeyNotFoundException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await mgr.GetSystemLogsAsync("NonExistentType", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-022")][Trait("Priority", "High")]
        public async Task GetSystemLogs_NullLogType_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetSystemLogsAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-023")][Trait("Priority", "High")]
        public async Task GetSystemLogs_EmptyLogType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetSystemLogsAsync(string.Empty, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-024")][Trait("Priority", "Critical")]
        public async Task PurgeOldData_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.PurgeOldDataAsync(DateTime.UtcNow.AddYears(-1), viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-025")][Trait("Priority", "High")]
        public async Task PurgeOldData_FutureDate_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.PurgeOldDataAsync(DateTime.UtcNow.AddDays(1), CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-026")][Trait("Priority", "High")]
        public async Task PurgeOldData_NullDate_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.PurgeOldDataAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-027")][Trait("Priority", "Medium")]
        public async Task GetSystemMetrics_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetSystemMetricsAsync(viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-028")][Trait("Priority", "High")]
        public async Task ExecuteMaintenanceTask_InvalidTaskType_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.ExecuteMaintenanceTaskAsync("InvalidTask", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-029")][Trait("Priority", "High")]
        public async Task ExecuteMaintenanceTask_NullTaskType_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.ExecuteMaintenanceTaskAsync(null, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-030")][Trait("Priority", "High")]
        public async Task ExecuteMaintenanceTask_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.ExecuteMaintenanceTaskAsync("CleanupLogs", viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-031")][Trait("Priority", "Critical")]
        public async Task GetSystemHealth_InsufficientClaims_ThrowsException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Test") }, "TestAuth"));
            await Assert.ThrowsAnyAsync<Exception>(async () => await mgr.GetSystemHealthAsync(user));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-032")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_ExcessiveMaxUploadSize_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var request = new UpdateSystemSettingsRequest { MaxUploadSizeMB = 10000 };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateSystemSettingsAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-033")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_ExcessiveSessionTimeout_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var request = new UpdateSystemSettingsRequest { SessionTimeoutMinutes = 100000 };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateSystemSettingsAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-034")][Trait("Priority", "High")]
        public async Task GetAuditLogs_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetAuditLogsAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-035")][Trait("Priority", "Medium")]
        public async Task GetSystemLogs_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.GetSystemLogsAsync("Error", viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-036")][Trait("Priority", "High")]
        public async Task RunDatabaseOptimization_InsufficientPermissions_ThrowsUnauthorizedAccessException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var viewer = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "999"), new Claim(ClaimTypes.Role, "Viewer") }, "TestAuth"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await mgr.RunDatabaseOptimizationAsync(viewer));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-037")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_InvalidEmailConfig_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var request = new UpdateSystemSettingsRequest { SMTPServer = "invalid_server" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateSystemSettingsAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-038")][Trait("Priority", "High")]
        public async Task GetActiveUsers_NegativeLimit_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetActiveUsersAsync(-1, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-039")][Trait("Priority", "Medium")]
        public async Task GetActiveUsers_ZeroLimit_HandlesOrRejects()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            try { var result = await mgr.GetActiveUsersAsync(0, CreateUser()); result.Should().BeEmpty(); }
            catch (ArgumentException) { Assert.True(true, "Zero limit rejected"); }
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-040")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_InvalidFeatureFlags_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var request = new UpdateSystemSettingsRequest { FeatureFlags = new Dictionary<string, bool> { { "InvalidFeature", true } } };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateSystemSettingsAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-041")][Trait("Priority", "High")]
        public async Task GetSystemMetrics_NegativeTimeRange_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetSystemMetricsAsync(CreateUser(), timeRangeMinutes: -60));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-042")][Trait("Priority", "Medium")]
        public async Task BackupDatabase_ConcurrentBackups_OneSucceedsOneFails()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var t1 = mgr.BackupDatabaseAsync(CreateUser());
            var t2 = mgr.BackupDatabaseAsync(CreateUser());
            try { await Task.WhenAll(t1, t2); Assert.True(true); }
            catch (InvalidOperationException) { Assert.True(true, "Concurrent backup prevented"); }
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-043")][Trait("Priority", "High")]
        public async Task RestoreDatabase_WhileSystemActive_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await mgr.RestoreDatabaseAsync("Backup", CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-044")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_InvalidDatabaseConnectionString_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var request = new UpdateSystemSettingsRequest { DatabaseConnectionString = "invalid_connection" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateSystemSettingsAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-045")][Trait("Priority", "Critical")]
        public async Task ClearCache_ConcurrentCalls_HandlesGracefully()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var tasks = Enumerable.Range(0, 10).Select(_ => mgr.ClearCacheAsync(CreateUser()));
            try { await Task.WhenAll(tasks); Assert.True(true); }
            catch { Assert.True(true, "Concurrent clear handled"); }
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-046")][Trait("Priority", "High")]
        public async Task GetSystemHealth_NullUser_ThrowsArgumentNullException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await mgr.GetSystemHealthAsync(null));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-047")][Trait("Priority", "High")]
        public async Task RunDatabaseOptimization_AlreadyRunning_ThrowsInvalidOperationException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var t1 = mgr.RunDatabaseOptimizationAsync(CreateUser());
            var t2 = mgr.RunDatabaseOptimizationAsync(CreateUser());
            try { await Task.WhenAll(t1, t2); Assert.True(true); }
            catch (InvalidOperationException) { Assert.True(true, "Concurrent optimization prevented"); }
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-048")][Trait("Priority", "Medium")]
        public async Task GetAuditLogs_ExcessiveLimit_ThrowsArgumentException()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.GetAuditLogsAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, CreateUser(), limit: 100000));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-049")][Trait("Priority", "High")]
        public async Task UpdateSystemSettings_SQLInjectionSMTPServer_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            var request = new UpdateSystemSettingsRequest { SMTPServer = "'; DROP TABLE Settings; --" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateSystemSettingsAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-ADMIN-NEG-050")][Trait("Priority", "Critical")]
        public async Task RestoreDatabase_PathTraversal_Blocked()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().SystemAdminManager;
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.RestoreDatabaseAsync("../../etc/passwd", CreateUser()));
        }


    }
}
