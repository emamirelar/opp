/**
 * @fileoverview PNO-1146 shared test fixture base for workflow email notification tests.
 * PaoWorkflowNotificationService sends emails for submission, recall, approval, and rejection.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.Workflow.Business.Interfaces;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.PNO1146;

/// <summary>
/// Shared fixture base for PNO-1146: Email Notification Changes on Opportunity Submission,
/// Recall, Approval and Rejection. Provides in-memory DB, mocked IEmailSender, and seed helpers.
/// </summary>
public abstract class PNO1146TestFixtureBase : IDisposable
{
    protected readonly AppDbContext DbContext;
    protected readonly Mock<IEmailSender> MockEmailSender;
    protected readonly Mock<IDbContextFactory<AppDbContext>> MockContextFactory;
    protected readonly Mock<ILogger<PaoWorkflowNotificationService>> MockLogger;
    protected readonly Mock<IConfiguration> MockConfiguration;
    protected readonly PaoWorkflowNotificationService NotificationService;
    protected readonly DbContextOptions<AppDbContext> DbOptions;
    protected readonly UserResolverService<int> UserResolverService;
    private readonly Mock<IDbContextSchema> _mockDbContextSchema;

    protected PNO1146TestFixtureBase()
    {
        DbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.Email, "test@unops.org")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mockDbContextSchema = new Mock<IDbContextSchema>();
        _mockDbContextSchema.Setup(x => x.Schema).Returns("public");

        UserResolverService = new UserResolverService<int>(mockHttpContextAccessor.Object);
        DbContext = new AppDbContext(DbOptions, UserResolverService, _mockDbContextSchema.Object);

        MockEmailSender = new Mock<IEmailSender>();
        MockLogger = new Mock<ILogger<PaoWorkflowNotificationService>>();
        MockConfiguration = new Mock<IConfiguration>();
        MockConfiguration.Setup(c => c["AppBaseUrl"]).Returns("https://test.pao.unops.org");

        MockContextFactory = new Mock<IDbContextFactory<AppDbContext>>();
        MockContextFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContext(DbOptions, UserResolverService, _mockDbContextSchema.Object));
        MockContextFactory
            .Setup(f => f.CreateDbContext())
            .Returns(() => new AppDbContext(DbOptions, UserResolverService, _mockDbContextSchema.Object));

        var notificationManager = new NotificationManager(DbContext, UserResolverService);

        var mockServiceScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockServiceProvider.Object);
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        mockServiceScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        NotificationService = new PaoWorkflowNotificationService(
            MockEmailSender.Object,
            MockContextFactory.Object,
            mockServiceScopeFactory.Object,
            MockLogger.Object,
            MockConfiguration.Object,
            notificationManager);
    }

    /// <summary>Builds a WorkflowNotification for approval request.</summary>
    protected static WorkflowNotification BuildWorkflowNotification(
        string entityName = "Opportunity",
        string entityId = "1",
        string entityDisplayName = "Test Opportunity",
        List<int>? recipientUserIds = null,
        int performedByUserId = 1,
        string performedByUserName = "Test User",
        string comment = "Please review",
        DateTime? timestamp = null) => new()
    {
        EntityName = entityName,
        EntityId = entityId,
        EntityDisplayName = entityDisplayName,
        RecipientUserIds = recipientUserIds ?? new List<int> { 1 },
        PerformedByUserId = performedByUserId,
        PerformedByUserName = performedByUserName,
        Comment = comment,
        Timestamp = timestamp ?? DateTime.UtcNow
    };

    /// <summary>Seeds an opportunity with org unit.</summary>
    protected async Task SeedOpportunityAsync(int id = 1, string name = "Test Opportunity", int? orgUnitId = 1)
    {
        int resolvedOrgUnitId = orgUnitId ?? 1;
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == resolvedOrgUnitId))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
            {
                Id = resolvedOrgUnitId,
                Name = "UNOPS HQ",
                Code = "HQ",
                Description = "UNOPS Headquarters",
                IsDeleted = false
            });
        }

        var existing = await DbContext.Opportunities.FindAsync(id);
        if (existing != null)
        {
            existing.Name = name;
            existing.ResponsibleOrgUnitId = orgUnitId;
        }
        else
        {
            DbContext.Opportunities.Add(new Opportunity
            {
                Id = id,
                Name = name,
                Description = "Test opportunity",
                ResponsibleOrgUnitId = orgUnitId,
                Stage = "IDENTIFY & PROFILE",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        await DbContext.SaveChangesAsync();
    }

    /// <summary>Seeds a PAOUser with email and optional profile.</summary>
    protected async Task SeedUserAsync(int id, string email, string? firstName = null, string? lastName = null)
    {
        var existing = await DbContext.PAOUsers.FindAsync(id);
        if (existing != null)
        {
            existing.Email = email;
        }
        else
        {
            DbContext.PAOUsers.Add(new PAOUser { Id = id, Email = email });
        }

        if (firstName != null || lastName != null)
        {
            var profile = await DbContext.UserProfile.FirstOrDefaultAsync(p => p.UserId == id);
            if (profile == null)
            {
                DbContext.UserProfile.Add(new UserProfile
                {
                    UserId = id,
                    FirstName = firstName ?? "",
                    LastName = lastName ?? "",
                    Status = EntityStatus.Active,
                    IsDeleted = false,
                    CreatedBy = 0,
                    CreatedDate = DateTime.UtcNow
                });
            }
            else
            {
                profile.FirstName = firstName ?? profile.FirstName;
                profile.LastName = lastName ?? profile.LastName;
            }
        }
        await DbContext.SaveChangesAsync();
    }

    /// <summary>Seeds an OpportunityStakeholder (e.g. Opportunity Manager).</summary>
    protected async Task SeedStakeholderAsync(int opportunityId, int userId, int entityRoleId = 1)
    {
        var existing = await DbContext.OpportunityStakeholders
            .FirstOrDefaultAsync(s => s.OpportunityId == opportunityId && s.UserId == userId);
        if (existing == null)
        {
            DbContext.OpportunityStakeholders.Add(new OpportunityStakeholder
            {
                OpportunityId = opportunityId,
                UserId = userId,
                EntityRoleId = entityRoleId,
                IsInternal = true
            });
            await DbContext.SaveChangesAsync();
        }
    }

    /// <summary>Verifies that SendEmailAsync was called with the expected template name.</summary>
    protected void VerifyEmailSent(string templateName, int times = 1)
    {
        MockEmailSender.Verify(
            e => e.SendEmailAsync(
                It.Is<EmailMessage>(m => m.TemplateName == templateName),
                It.IsAny<object>(),
                It.IsAny<string?>()),
            Times.Exactly(times));
    }

    /// <summary>Last captured EmailMessage from SendEmailAsync callback. Set by SetupEmailCapture().</summary>
    protected EmailMessage? LastCapturedEmail { get; set; }

    /// <summary>Sets up the mock to capture the next EmailMessage sent. Call before invoking the service.</summary>
    protected void SetupEmailCapture()
    {
        LastCapturedEmail = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Callback<EmailMessage, object, string?>((msg, _, _) => LastCapturedEmail = msg)
            .Returns(Task.CompletedTask);
    }

    public virtual void Dispose()
    {
        DbContext.Dispose();
    }
}
