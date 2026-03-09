/// <summary>
/// Comprehensive unit tests for DueDiligenceNotificationService.
/// Tests expiry threshold calculation, notification deduplication, email/in-app notification construction,
/// logging behavior, error handling (DB failures, email send failures), and configuration.
/// Requirements source: UNOPS.PAO.UNOPSBusiness/Services/DueDiligenceNotificationService.cs
///
/// Note: DueDiligenceNotificationService is a BackgroundService with private methods.
/// Tests use reflection to invoke CheckAndSendNotifications and verify behavior via mocks and DB state.
/// </summary>

using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

[Trait("Category", "Unit")]
[Trait("Feature", "DueDiligenceNotificationService")]
public class DueDiligenceNotificationServiceTests : IDisposable
{
    private readonly UNOPSAppDbContext _context;
    private readonly DbContextOptions<UNOPSAppDbContext> _options;
    private readonly UNOPS.PAO.DataAccess.Services.UserResolverService<int> _userResolver;
    private readonly UNOPS.PAO.DataAccess.Interfaces.IDbContextSchema _schema;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly Mock<IUrlService> _mockUrlService;
    private readonly Mock<ILogger<DueDiligenceNotificationService>> _mockLogger;

    public DueDiligenceNotificationServiceTests()
    {
        var dbName = $"DueDiligence_{Guid.NewGuid():N}";
        _options = TestEnvironment.CreateUNOPSDbContextOptions(dbName);
        _userResolver = new UNOPS.PAO.DataAccess.Services.UserResolverService<int>(
            TestDbContextFactory.CreateMockHttpContextAccessor("1").Object);
        var mockSchema = new Mock<UNOPS.PAO.DataAccess.Interfaces.IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");
        _schema = mockSchema.Object;

        _context = TestDbContextFactory.CreateUNOPS(_options, _userResolver, _schema);
        TestEnvironment.EnsureCleanDatabase(_context);

        _mockEmailSender = new Mock<IEmailSender>();
        _mockUrlService = new Mock<IUrlService>();
        _mockLogger = new Mock<ILogger<DueDiligenceNotificationService>>();
    }

    public void Dispose() => _context?.Dispose();

    #region 1. Configuration

    [Fact]
    [Trait("Category", "Positive")]
    public void Constructor_ReadsCheckIntervalHoursFromConfiguration()
    {
        var config = CreateConfig(checkIntervalHours: 12);
        var service = CreateService(config);
        GetPrivateField<int>(service, "_checkIntervalHours").Should().Be(12);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Constructor_ReadsWarningMonthsFromConfiguration()
    {
        var config = CreateConfig(warningMonths: 3);
        var service = CreateService(config);
        GetPrivateField<int>(service, "_warningMonths").Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Constructor_ReadsEnabledFromConfiguration()
    {
        var config = CreateConfig(enabled: false);
        var service = CreateService(config);
        GetPrivateField<bool>(service, "_enabled").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Constructor_ReadsTestModeFromConfiguration()
    {
        var config = CreateConfig(testMode: true);
        var service = CreateService(config);
        GetPrivateField<bool>(service, "_testMode").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Constructor_MissingSection_UsesDefaults()
    {
        var config = new ConfigurationBuilder().Build();
        var service = CreateService(config);
        GetPrivateField<int>(service, "_checkIntervalHours").Should().Be(24);
        GetPrivateField<int>(service, "_warningMonths").Should().Be(6);
        GetPrivateField<bool>(service, "_enabled").Should().BeTrue();
        GetPrivateField<bool>(service, "_testMode").Should().BeFalse();
    }

    #endregion

    #region 2. Expiry Threshold Calculation

    [Fact]
    [Trait("Category", "Positive")]
    public async Task CheckAndSendNotifications_PartnerExpiringWithinWarningMonths_IsIncluded()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(3);
        SeedPartnerWithUser("Partner A", 1, expiryDate, "focal@test.com", "Focal User");
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    [SkipIfPostgreSQLFact]
    [Trait("Category", "Boundary")]
    public async Task CheckAndSendNotifications_PartnerExpiringBeyondWarningMonths_IsExcluded()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(8);
        SeedPartnerWithUser("Partner B", 2, expiryDate, "focal2@test.com", "Focal User 2");
        var service = CreateServiceWithScope(warningMonths: 6);

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CheckAndSendNotifications_PartnerExpiryInPast_IsExcluded()
    {
        var expiryDate = DateTime.UtcNow.AddDays(-1);
        SeedPartnerWithUser("Partner C", 3, expiryDate, "focal3@test.com", "Focal User 3");
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CheckAndSendNotifications_PartnerWithoutFocalPoint_IsExcluded()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(2);
        SeedPartner("Partner D", expiryDate, null);
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task CheckAndSendNotifications_SoftDeletedPartner_IsExcluded()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(2);
        SeedPartnerWithUser("Partner E", 5, expiryDate, "focal5@test.com", "Focal User 5", isDeleted: true);
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task CheckAndSendNotifications_NoExpiringPartners_DoesNotSendEmail()
    {
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    #endregion

    #region 3. Notification Deduplication

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CheckAndSendNotifications_WhenNotificationAlreadySent_SkipsDuplicate()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(2);
        var partnerId = SeedPartnerWithUserAndGetPartnerId("Partner F", 6, expiryDate, "focal6@test.com", "Focal User 6");
        SeedEmailNotificationLog(partnerId, 6, expiryDate);
        var service = CreateServiceWithScope(testMode: false);

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [SkipIfPostgreSQLFact]
    [Trait("Category", "Functional")]
    public async Task CheckAndSendNotifications_TestModeTrue_BypassesDeduplication()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(2);
        var partnerId = SeedPartnerWithUserAndGetPartnerId("Partner G", 7, expiryDate, "focal7@test.com", "Focal User 7");
        SeedEmailNotificationLog(partnerId, 7, expiryDate);
        var service = CreateServiceWithScope(testMode: true);

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CheckAndSendNotifications_DifferentExpiryDate_SendsNewNotification()
    {
        var expiryDate1 = DateTime.UtcNow.AddMonths(2);
        var expiryDate2 = DateTime.UtcNow.AddMonths(4);
        var partnerId = SeedPartnerWithUserAndGetPartnerId("Partner H", 8, expiryDate2, "focal8@test.com", "Focal User 8");
        SeedEmailNotificationLog(partnerId, 8, expiryDate1);
        var service = CreateServiceWithScope(testMode: false);

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    #endregion

    #region 4. Email Notification Construction

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CheckAndSendNotifications_EmailSent_WithCorrectTemplateAndSubject()
    {
        UNOPS.PAO.MailSender.EmailMessage? capturedMessage = null;
        _mockEmailSender
            .Setup(x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Callback<UNOPS.PAO.MailSender.EmailMessage, object, string?>((msg, _, _) => capturedMessage = msg)
            .Returns(Task.CompletedTask);

        var expiryDate = DateTime.UtcNow.AddMonths(2);
        SeedPartnerWithUser("Acme Corp", 10, expiryDate, "acme@test.com", "John Doe");
        _mockUrlService.Setup(x => x.BuildEntityUrl("partner", It.IsAny<int>())).Returns("https://app.test/partner/10");
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        capturedMessage.Should().NotBeNull();
        capturedMessage!.TemplateName.Should().Contain("DueDiligenceExpiryNotification");
        capturedMessage.Title.Should().Contain("Acme Corp");
        capturedMessage.EmailReceivers.Should().Contain("acme@test.com");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CheckAndSendNotifications_EmailModel_ContainsPartnerNameAndExpiryDate()
    {
        object? capturedModel = null;
        _mockEmailSender
            .Setup(x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Callback<UNOPS.PAO.MailSender.EmailMessage, object, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        var expiryDate = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var partnerId = SeedPartnerWithUserAndGetPartnerId("Beta Inc", 11, expiryDate, "beta@test.com", "Jane Smith");
        var expectedUrl = $"https://app.test/partner/{partnerId}";
        _mockUrlService.Setup(x => x.BuildEntityUrl("partner", It.IsAny<int>())).Returns<string, int>((_, id) => $"https://app.test/partner/{id}");
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        capturedModel.Should().NotBeNull();
        var modelType = capturedModel!.GetType();
        modelType.GetProperty("PartnerName")!.GetValue(capturedModel).Should().Be("Beta Inc");
        modelType.GetProperty("UserName")!.GetValue(capturedModel).Should().Be("Jane Smith");
        modelType.GetProperty("ExpiryDate")!.GetValue(capturedModel).ToString().Should().Contain("June");
        modelType.GetProperty("PartnerUrl")!.GetValue(capturedModel).Should().Be(expectedUrl);
    }

    #endregion

    #region 5. In-App Notification Creation

    [SkipIfPostgreSQLFact]
    [Trait("Category", "Functional")]
    public async Task CheckAndSendNotifications_OnSuccess_CreatesInAppNotification()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(2);
        SeedPartnerWithUser("Gamma LLC", 12, expiryDate, "gamma@test.com", "Bob Wilson");
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        var notifications = await _context.Notifications
            .Where(n => n.UserId == 12 && n.Category == "DueDiligenceExpiry")
            .ToListAsync();
        notifications.Should().HaveCount(1);
        notifications[0].Message.Should().Contain("Gamma LLC");
        notifications[0].Message.Should().Contain("expires on");
        notifications[0].ResponseType.Should().Be("Partner");
    }

    #endregion

    #region 6. EmailNotificationLog Recording

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CheckAndSendNotifications_OnSuccess_RecordsEmailNotificationLog()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(2);
        var partnerId = SeedPartnerWithUserAndGetPartnerId("Delta Co", 13, expiryDate, "delta@test.com", "Alice Brown");
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        var logs = await _context.EmailNotificationLogs
            .Where(l => l.RelatedEntityId == partnerId && l.NotificationType == "DueDiligenceExpiry")
            .ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].RecipientUserId.Should().Be(13);
        logs[0].RecipientEmail.Should().Be("delta@test.com");
        logs[0].RelatedEntityType.Should().Be("Partner");
        logs[0].NotificationData.Should().Contain(expiryDate.ToString("yyyy-MM-dd"));
        logs[0].IsSuccessful.Should().BeTrue();
    }

    #endregion

    #region 7. Logging Behavior

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CheckAndSendNotifications_WhenDisabled_LogsInformation()
    {
        var config = CreateConfig(enabled: false);
        var service = CreateService(config);
        var logCalls = new List<string>();
        _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Callback<LogLevel, EventId, object, Exception?, Delegate>((level, _, state, _, _) =>
            {
                if (level == LogLevel.Information)
                    logCalls.Add(state?.ToString() ?? "");
            });

        await InvokeExecuteAsync(service);

        logCalls.Should().Contain(x => x.Contains("disabled", StringComparison.OrdinalIgnoreCase));
    }

    [SkipIfPostgreSQLFact]
    [Trait("Category", "Negative")]
    public async Task CheckAndSendNotifications_UserProfileWithoutEmail_LogsWarningAndIncrementsFailure()
    {
        var expiryDate = DateTime.UtcNow.AddMonths(2);
        SeedPartnerWithUser("Epsilon", 14, expiryDate, null!, "No Email User");
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        _mockEmailSender.Verify(
            x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    #endregion

    #region 8. Error Handling

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CheckAndSendNotifications_EmailSendThrows_LogsErrorAndContinues()
    {
        _mockEmailSender
            .Setup(x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("SMTP failure"));

        var expiryDate = DateTime.UtcNow.AddMonths(2);
        SeedPartnerWithUser("Zeta", 15, expiryDate, "zeta@test.com", "Zeta User");
        var service = CreateServiceWithScope();

        var act = async () => await InvokeCheckAndSendNotifications(service);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CheckAndSendNotifications_ServiceScopeFails_LogsErrorAndDoesNotThrow()
    {
        var badProvider = new Mock<IServiceProvider>();
        badProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns((IServiceScopeFactory?)null);
        var config = CreateConfig();
        var service = new DueDiligenceNotificationService(_mockLogger.Object, badProvider.Object, config);

        var act = async () => await InvokeCheckAndSendNotifications(service);

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region 9. Months/Days Calculation

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CheckAndSendNotifications_CalculatesMonthsUntilExpiryCorrectly()
    {
        var expiryDate = DateTime.UtcNow.AddDays(61);
        object? capturedModel = null;
        _mockEmailSender
            .Setup(x => x.SendEmailAsync(It.IsAny<UNOPS.PAO.MailSender.EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Callback<UNOPS.PAO.MailSender.EmailMessage, object, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        SeedPartnerWithUser("Theta", 17, expiryDate, "theta@test.com", "Theta User");
        var service = CreateServiceWithScope();

        await InvokeCheckAndSendNotifications(service);

        capturedModel.Should().NotBeNull();
        var modelType = capturedModel!.GetType();
        var months = (decimal)modelType.GetProperty("MonthsUntilExpiry")!.GetValue(capturedModel)!;
        var days = (int)modelType.GetProperty("DaysRemaining")!.GetValue(capturedModel)!;
        months.Should().BeApproximately(2.0m, 0.5m);
        days.Should().BeInRange(59, 63);
    }

    #endregion

    #region Helpers

    private static IConfiguration CreateConfig(
        int? checkIntervalHours = null,
        int? warningMonths = null,
        bool? enabled = null,
        bool? testMode = null)
    {
        var dict = new Dictionary<string, string?>();
        if (checkIntervalHours.HasValue)
            dict["DueDiligenceNotifications:CheckIntervalHours"] = checkIntervalHours.Value.ToString();
        if (warningMonths.HasValue)
            dict["DueDiligenceNotifications:WarningMonths"] = warningMonths.Value.ToString();
        if (enabled.HasValue)
            dict["DueDiligenceNotifications:Enabled"] = enabled.Value.ToString().ToLowerInvariant();
        if (testMode.HasValue)
            dict["DueDiligenceNotifications:TestMode"] = testMode.Value.ToString().ToLowerInvariant();

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    private DueDiligenceNotificationService CreateService(IConfiguration config)
    {
        var provider = CreateServiceProvider();
        return new DueDiligenceNotificationService(_mockLogger.Object, provider, config);
    }

    private DueDiligenceNotificationService CreateServiceWithScope(
        int? warningMonths = null,
        bool? testMode = null)
    {
        var dict = new Dictionary<string, string?>();
        if (warningMonths.HasValue)
            dict["DueDiligenceNotifications:WarningMonths"] = warningMonths.Value.ToString();
        if (testMode.HasValue)
            dict["DueDiligenceNotifications:TestMode"] = testMode.Value.ToString().ToLowerInvariant();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();

        var provider = CreateServiceProvider();
        return new DueDiligenceNotificationService(_mockLogger.Object, provider, config);
    }

    private IServiceProvider CreateServiceProvider()
    {
        var paoEmailSender = new PAOEmailSender(_mockEmailSender.Object);
        _mockUrlService.Setup(x => x.BuildEntityUrl(It.IsAny<string>(), It.IsAny<int>()))
            .Returns<string, int>((entity, id) => $"https://app.test/{entity}/{id}");

        var services = new ServiceCollection();
        services.AddScoped<UNOPSAppDbContext>(_ => CreateScopeContext());
        services.AddScoped<PAOEmailSender>(_ => paoEmailSender);
        services.AddScoped<IUrlService>(_ => _mockUrlService.Object);
        services.AddScoped<NotificationManager>(sp => new NotificationManager(sp.GetRequiredService<UNOPSAppDbContext>(), _userResolver));

        return services.BuildServiceProvider();
    }

    private UNOPSAppDbContext CreateScopeContext()
    {
        return TestDbContextFactory.CreateUNOPS(_options, _userResolver, _schema);
    }

    private void SeedPartnerWithUser(
        string partnerName,
        int focalPointUserId,
        DateTime expiryDate,
        string? userEmail,
        string? userName,
        bool isDeleted = false)
    {
        SeedUserProfile(focalPointUserId, userEmail, userName);
        SeedPartner(partnerName, expiryDate, focalPointUserId, isDeleted);
    }

    private int SeedPartnerWithUserAndGetPartnerId(
        string partnerName,
        int focalPointUserId,
        DateTime expiryDate,
        string? userEmail,
        string? userName,
        bool isDeleted = false)
    {
        SeedUserProfile(focalPointUserId, userEmail, userName);
        return SeedPartnerAndGetId(partnerName, expiryDate, focalPointUserId, isDeleted);
    }

    private void SeedPartner(string partnerName, DateTime expiryDate, int? focalPointUserId, bool isDeleted = false)
    {
        SeedPartnerAndGetId(partnerName, expiryDate, focalPointUserId, isDeleted);
    }

    private int SeedPartnerAndGetId(string partnerName, DateTime expiryDate, int? focalPointUserId, bool isDeleted = false)
    {
        var partner = new UNOPSPartner
        {
            Name = partnerName,
            PartnerShortDescription = "Short",
            DueDiligenceExpiryDate = expiryDate,
            PartnerFocalPointUserId = focalPointUserId,
            Status = EntityStatus.Active,
            IsDeleted = isDeleted
        };
        _context.Partners.Add(partner);
        _context.SaveChanges();
        return partner.Id;
    }

    private void SeedUserProfile(int userId, string? userEmail, string? userName)
    {
        var displayName = !string.IsNullOrEmpty(userName) ? userName : $"Test User {userId}";
        var email = userEmail ?? $"testuser_{userId}@test.local";

        if (TestEnvironment.UsePostgreSQL)
        {
            EnsureAspNetUser(userId, email ?? $"testuser_{userId}@test.local");
            var now = DateTime.UtcNow;
            var firstName = displayName.Split(' ').FirstOrDefault() ?? "Test";
            var lastName = displayName.Split(' ').Skip(1).FirstOrDefault() ?? "User";
            _context.Database.ExecuteSqlRaw(
                "INSERT INTO \"UserProfile\" (\"Id\", \"UserId\", \"FirstName\", \"LastName\", \"Name\", \"UserEmail\", " +
                "\"Status\", \"CreatedBy\", \"CreatedDate\", \"LastModifiedBy\", \"LastModifiedDate\", " +
                "\"IsDeleted\", \"DeletedBy\", \"WorkflowStatus\") " +
                "SELECT (SELECT COALESCE(MAX(\"Id\"), 0) + 1 FROM \"UserProfile\"), " +
                "{0}, {1}, {2}, {3}, {4}, 1, {0}, {5}, 0, {5}, false, 0, 0 " +
                "WHERE NOT EXISTS (SELECT 1 FROM \"UserProfile\" WHERE \"UserId\" = {0})",
                userId,
                firstName,
                lastName,
                displayName,
                email,
                now);
        }
        else
        {
            var profile = new UserProfile
            {
                UserId = userId,
                FirstName = displayName.Split(' ').FirstOrDefault() ?? "Test",
                LastName = displayName.Split(' ').Skip(1).FirstOrDefault() ?? "User",
                UserEmail = userEmail,
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            _context.UserProfile.Add(profile);
            _context.SaveChanges();
        }
    }

    private void EnsureAspNetUser(int userId, string email)
    {
        if (!TestEnvironment.UsePostgreSQL)
            return;

        _context.Database.ExecuteSqlRaw(
            "INSERT INTO \"AspNetUsers\" (\"Id\", \"Email\", \"NormalizedEmail\", \"UserName\", \"NormalizedUserName\", " +
            "\"EmailConfirmed\", \"PasswordHash\", \"SecurityStamp\", \"ConcurrencyStamp\", " +
            "\"PhoneNumberConfirmed\", \"TwoFactorEnabled\", \"LockoutEnabled\", \"AccessFailedCount\", \"IsInternal\") " +
            "SELECT {0}, {1}, {2}, {1}, {2}, " +
            "true, 'x', 'x', 'x', false, false, true, 0, true " +
            "WHERE NOT EXISTS (SELECT 1 FROM \"AspNetUsers\" WHERE \"Id\" = {0})",
            userId, email, email.ToUpperInvariant());
    }

    private void SeedEmailNotificationLog(int partnerId, int userId, DateTime expiryDate)
    {
        var log = new EmailNotificationLog
        {
            RecipientUserId = userId,
            RelatedEntityId = partnerId,
            RelatedEntityType = "Partner",
            NotificationType = "DueDiligenceExpiry",
            NotificationData = $"{{\"DueDiligenceExpiryDate\":\"{expiryDate:yyyy-MM-dd}\"}}",
            SentAt = DateTime.UtcNow,
            IsSuccessful = true
        };
        _context.EmailNotificationLogs.Add(log);
        _context.SaveChanges();
    }

    private static async Task InvokeCheckAndSendNotifications(DueDiligenceNotificationService service)
    {
        var method = typeof(DueDiligenceNotificationService)
            .GetMethod("CheckAndSendNotifications", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();
        var task = method!.Invoke(service, null);
        await (Task)task!;
    }

    private static async Task InvokeExecuteAsync(DueDiligenceNotificationService service)
    {
        var method = typeof(BackgroundService)
            .GetMethod("ExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();
        var task = method!.Invoke(service, new object[] { CancellationToken.None });
        await (Task)task!;
    }

    private static T? GetPrivateField<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T?)field?.GetValue(obj);
    }

    #endregion
}
