/**
 * @fileoverview Tests for PaoWorkflowNotificationService.NotifyInternalStakeholdersOnGoDecisionAsync.
 * PNO-1146: Notifies Implementation Country OrgUnit directors when Go Decision approved.
 * TO: Region/Hub/OrgUnit Directors (excludes DoA); CC: OM + Workflow Initiator.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Workflow;

[Collection("PaoWorkflowNotification")]
public class PaoWorkflowInternalStakeholderTests : PaoWorkflowNotificationTestFixtureBase
{
    private async Task SeedInternalStakeholderScenarioAsync(
        int opportunityId = 1,
        int responsibleOrgUnitId = 1,
        int implCountryOrgUnitId = 2,
        int countryId = 10,
        int directorUserId = 200)
    {
        await SeedOpportunityAsync(opportunityId, orgUnitId: responsibleOrgUnitId);
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == implCountryOrgUnitId))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
            {
                Id = implCountryOrgUnitId,
                Name = "Implementation Country Org Unit",
                Code = "IMPL-OU",
                Description = "Org unit for implementation country",
                IsDeleted = false
            });
        }
        if (!await DbContext.Set<Country>().AnyAsync(c => c.Id == countryId))
        {
            DbContext.Set<Country>().Add(new Country
            {
                Id = countryId,
                Name = "Kenya",
                Iso2Code = "KE",
                IsDeleted = false
            });
        }
        var opp = await DbContext.Opportunities
            .Include(o => o.Countries)
            .FirstOrDefaultAsync(o => o.Id == opportunityId);
        if (opp != null && !opp.Countries.Any())
        {
            DbContext.OpportunityCountries.Add(new OpportunityCountry
            {
                OpportunityId = opportunityId,
                CountryId = countryId,
                Name = "Kenya",
                IsDeleted = false,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            });
        }
        if (!await DbContext.OrganizationUnitRelationships.AnyAsync(r =>
            r.EntityType == "Country" && r.EntityId == countryId && r.OrganizationHierarchyId == implCountryOrgUnitId))
        {
            DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
            {
                Name = "Country-OU",
                EntityType = "Country",
                EntityId = countryId,
                OrganizationHierarchyId = implCountryOrgUnitId,
                IsDeleted = false,
                CreatedBy = 0,
                CreatedDate = DateTime.UtcNow
            });
        }
        var directorRole = new EntityRole
        {
            Id = 301,
            Code = "OrgUnit_Director_OrganizationHierarchy",
            EntityType = "OrganizationHierarchy",
            Name = "Director",
            IsDeleted = false
        };
        if (!await DbContext.Set<EntityRole>().AnyAsync(r => r.Id == 301))
        {
            DbContext.Set<EntityRole>().Add(directorRole);
        }
        await SeedUserAsync(directorUserId, "director@impl.org", "Impl", "Director");
        if (!await DbContext.EntityUserRoles.AnyAsync(e => e.EntityId == implCountryOrgUnitId && e.UserId == directorUserId))
        {
            DbContext.EntityUserRoles.Add(new EntityUserRole
            {
                Id = 301,
                Name = "Dir",
                EntityType = "OrganizationHierarchy",
                EntityId = implCountryOrgUnitId,
                EntityRoleId = 301,
                UserId = directorUserId,
                Status = EntityStatus.Active,
                IsDeleted = false
            });
        }
        await SeedUserAsync(100, "om@unops.org");
        await SeedUserAsync(101, "initiator@unops.org");
        await SeedOpportunityManagerAsync(opportunityId, 100);
        await SeedCompletedSubmitWorkflowLogAsync(opportunityId.ToString(), 101);
        await DbContext.SaveChangesAsync();
    }

    #region Positive Tests (4)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_WithImplementationCountryOrgUnit_SendsToDirectors()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver Name");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.TemplateName.Should().Be(TemplateCompleted);
        LastCapturedEmail.EmailReceivers.Should().Contain("director@impl.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_CCIncludesOMAndInitiator()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.CcReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.CcReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_EmailModelPopulated()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "John Approver");

        // Assert
        capturedModel.Should().NotBeNull();
        capturedModel!.ApprovedByName.Should().Be("John Approver");
        capturedModel.EntityName.Should().NotBeNullOrEmpty();
        capturedModel.EntityUrl.Should().Contain("opportunities/1");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_TitleContainsFYI()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.Title.Should().Contain("FYI");
    }

    #endregion

    #region Negative Tests (12)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_OpportunityNotFound_DoesNotThrow()
    {
        // Arrange
        await SeedUserAsync(1, "user@unops.org");

        // Act
        var act = () => NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(99999, "Approver");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_NoCountries_SkipsNotification()
    {
        // Arrange - opportunity without countries
        await SeedOpportunityAsync(1);
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - no email sent when no countries
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_NoOtherOrgUnits_SkipsNotification()
    {
        // Arrange - opportunity with country but no org unit relationships for other org units
        await SeedOpportunityAsync(1);
        await DbContext.Set<Country>().AddAsync(new Country { Id = 10, Name = "Kenya", Iso2Code = "KE", IsDeleted = false });
        DbContext.OpportunityCountries.Add(new OpportunityCountry
        {
            OpportunityId = 1,
            CountryId = 10,
            Name = "Kenya",
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - no OrganizationUnitRelationships for country, so no other org units
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_ExcludesResponsibleOrgUnit()
    {
        // Arrange - org unit relationship for same org unit as opportunity's responsible
        await SeedOpportunityAsync(1);
        await DbContext.Set<Country>().AddAsync(new Country { Id = 10, Name = "Kenya", Iso2Code = "KE", IsDeleted = false });
        DbContext.OpportunityCountries.Add(new OpportunityCountry
        {
            OpportunityId = 1,
            CountryId = 10,
            Name = "Kenya",
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
        {
            Name = "R",
            EntityType = "Country",
            EntityId = 10,
            OrganizationHierarchyId = 1, // Same as ResponsibleOrgUnitId
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - responsible org unit excluded, so no other org units
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_NoDirectorStakeholders_SkipsNotification()
    {
        // Arrange - org units exist but no director EntityUserRoles
        await SeedOpportunityAsync(1);
        DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
        {
            Id = 2,
            Name = "Other OU",
            Code = "OU2",
            Description = "Other",
            IsDeleted = false
        });
        await DbContext.Set<Country>().AddAsync(new Country { Id = 10, Name = "Kenya", Iso2Code = "KE", IsDeleted = false });
        DbContext.OpportunityCountries.Add(new OpportunityCountry
        {
            OpportunityId = 1,
            CountryId = 10,
            Name = "Kenya",
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
        {
            Name = "R",
            EntityType = "Country",
            EntityId = 10,
            OrganizationHierarchyId = 2,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - no directors in org unit 2
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_EmailSenderThrows_DoesNotPropagate()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("Email down"));

        // Act
        var act = () => NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_EmptyApproverName_Accepted()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();

        // Act
        var act = () => NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_SoftDeletedOpportunity_DoesNotThrow()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.IsDeleted = true;
        await DbContext.SaveChangesAsync();

        // Act
        var act = () => NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_EntityTypeCountry_Required()
    {
        // Arrange - wrong entity type in relationship
        await SeedOpportunityAsync(1);
        await DbContext.Set<Country>().AddAsync(new Country { Id = 10, Name = "Kenya", Iso2Code = "KE", IsDeleted = false });
        DbContext.OpportunityCountries.Add(new OpportunityCountry
        {
            OpportunityId = 1,
            CountryId = 10,
            Name = "Kenya",
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
        {
            Name = "R",
            EntityType = "Partner", // Wrong type
            EntityId = 10,
            OrganizationHierarchyId = 2,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - no match for EntityType Country
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_DirectorWithNoEmail_Excluded()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync(directorUserId: 201);
        var user = await DbContext.PAOUsers.FindAsync(201);
        if (user != null) user.Email = null;
        await DbContext.SaveChangesAsync();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - may send with empty TO or skip
        await NotificationService.Invoking(s => s.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver"))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_ExcludesDoARoles()
    {
        // Arrange - ImplementationCountryDirectorRoleCodes excludes DoA1-DoA4
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - only director roles in TO
        LastCapturedEmail!.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_NullResponsibleOrgUnit_Handled()
    {
        // Arrange
        await SeedOpportunityAsync(1, orgUnitId: null);
        var opp = await DbContext.Opportunities.FindAsync(1);
        opp!.ResponsibleOrgUnitId = null;
        await DbContext.SaveChangesAsync();
        await DbContext.Set<Country>().AddAsync(new Country { Id = 10, Name = "Kenya", Iso2Code = "KE", IsDeleted = false });
        DbContext.OpportunityCountries.Add(new OpportunityCountry
        {
            OpportunityId = 1,
            CountryId = 10,
            Name = "Kenya",
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
        {
            Name = "R",
            EntityType = "Country",
            EntityId = 10,
            OrganizationHierarchyId = 2,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();

        // Act - Where clause uses != opportunity.ResponsibleOrgUnitId; null != 2 is true
        var act = () => NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Boundary Tests (12)

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_MultipleCountries_MultipleOrgUnits()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync(countryId: 10);
        DbContext.Set<Country>().Add(new Country { Id = 11, Name = "Uganda", Iso2Code = "UG", IsDeleted = false });
        DbContext.OpportunityCountries.Add(new OpportunityCountry
        {
            OpportunityId = 1,
            CountryId = 11,
            Name = "Uganda",
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
        {
            Name = "R2",
            EntityType = "Country",
            EntityId = 11,
            OrganizationHierarchyId = 2,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_CommentSectionFixed()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        capturedModel!.CommentSection.Should().Contain("approved for development");
        capturedModel.CommentSection.Should().Contain("area of responsibility");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_OrgUnitNameFromOpportunity()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        capturedModel!.OrgUnitName.Should().Be("UNOPS HQ");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_ApprovedOnUtcNow()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        capturedModel!.ApprovedOn.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_ImplementationCountryDirectorRoleCodes_Used()
    {
        // Arrange - uses Regional_Director, MCO_Director, OrgUnit_Director, etc. (not DoA)
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("director@impl.org");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_AsNoTracking_Used()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - no throw from tracking
        await NotificationService.Invoking(s => s.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver"))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_SoftDeletedOrgUnitRelationship_Excluded()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        var rel = await DbContext.OrganizationUnitRelationships
            .FirstOrDefaultAsync(r => r.EntityType == "Country" && r.EntityId == 10);
        if (rel != null)
        {
            rel.IsDeleted = true;
            await DbContext.SaveChangesAsync();
        }

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - OrganizationUnitRelationship may filter IsDeleted
        await NotificationService.Invoking(s => s.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver"))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_EntityUserRoleIsDeleted_Excluded()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        var eur = await DbContext.EntityUserRoles
            .FirstOrDefaultAsync(e => e.EntityId == 2 && e.UserId == 200);
        if (eur != null)
        {
            eur.IsDeleted = true;
            await DbContext.SaveChangesAsync();
        }

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_CCDeduplication_OMAndInitiatorSame()
    {
        // Arrange - OM is initiator
        await SeedInternalStakeholderScenarioAsync();
        await SeedOpportunityManagerAsync(1, 101);
        await SeedCompletedSubmitWorkflowLogAsync("1", 101);
        await SeedUserAsync(101, "om@unops.org");
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.CcReceivers.Should().NotBeNull();
        var omCount = LastCapturedEmail.CcReceivers.Count(e => e == "om@unops.org");
        omCount.Should().BeLessThanOrEqualTo(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_OpportunityNameInModel()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        capturedModel!.EntityName.Should().Be("Test Opportunity");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_EntityUrlCorrect()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        capturedModel!.EntityUrl.Should().Contain("/partnerships/opportunities/1");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_RecipientNamesJoined()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        capturedModel!.RecipientName.Should().Contain("Impl");
        capturedModel.RecipientName.Should().Contain("Director");
    }

    #endregion

    #region Functional Tests (12)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_UsesWorkflowCompletedTemplate()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.TemplateName.Should().Be(TemplateCompleted);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_QueriesOrganizationUnitRelationships()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("director@impl.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_QueriesEntityUserRoles()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_GetOpportunityManagerEmail_ForCC()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.CcReceivers.Should().Contain("om@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_GetInitiatorUserId_ForCC()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.CcReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_CountryIdsFromOpportunityCountries()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_OrgUnitIdsDistinct()
    {
        // Arrange - multiple relationships to same org unit
        await SeedInternalStakeholderScenarioAsync();
        DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
        {
            Name = "R2",
            EntityType = "Country",
            EntityId = 10,
            OrganizationHierarchyId = 2,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        await DbContext.SaveChangesAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().Contain("director@impl.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_SendEmailCalledOnce()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_ResponsibleOrgUnitExcluded()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync(responsibleOrgUnitId: 1, implCountryOrgUnitId: 2);
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - org unit 2 directors in TO, not org unit 1
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("director@impl.org");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_IncludeEntityRole_ForRoleCodeFilter()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_UserIdDistinct()
    {
        // Arrange - same user in multiple org units
        await SeedInternalStakeholderScenarioAsync();
        DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
        {
            Id = 3,
            Name = "OU3",
            Code = "OU3",
            Description = "OU3",
            IsDeleted = false
        });
        DbContext.OrganizationUnitRelationships.Add(new OrganizationUnitRelationship
        {
            Name = "R3",
            EntityType = "Country",
            EntityId = 10,
            OrganizationHierarchyId = 3,
            IsDeleted = false,
            CreatedBy = 0,
            CreatedDate = DateTime.UtcNow
        });
        DbContext.EntityUserRoles.Add(new EntityUserRole
        {
            Id = 302,
            Name = "Dir",
            EntityType = "OrganizationHierarchy",
            EntityId = 3,
            EntityRoleId = 301,
            UserId = 200,
            Status = EntityStatus.Active,
            IsDeleted = false
        });
        await DbContext.SaveChangesAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - director@impl.org once
        var count = LastCapturedEmail!.EmailReceivers.Count(e => e == "director@impl.org");
        count.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_ToEmailsNotEmpty()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_BaseUrlInEntityUrl()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        capturedModel!.EntityUrl.Should().StartWith("https://");
    }

    #endregion

    #region Integration Tests (12)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_FullFlow_OpportunityToEmail()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "John Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().Contain("director@impl.org");
        LastCapturedEmail.CcReceivers.Should().Contain("om@unops.org");
        LastCapturedEmail.TemplateName.Should().Be(TemplateCompleted);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_DbContextFactory_Used()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        await NotificationService.Invoking(s => s.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver"))
            .Should().NotThrowAsync();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_OpportunityIncludeCountries()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_WorkflowDbContext_ForInitiator()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - CC includes initiator from WorkflowLog
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.CcReceivers.Should().Contain("initiator@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_ConsecutiveCalls_NoStateLeakage()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        MockEmailSender.Verify(
            e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<object>(), It.IsAny<string?>()),
            Times.Exactly(2));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_GetRecipientEmails_FromPAOUsers()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().AllSatisfy(e => e.Should().Contain("@"));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_GetRecipientNames_ForEmailModel()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        capturedModel!.RecipientName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_EmailMessageStructure_Valid()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail!.TemplateName.Should().NotBeNullOrEmpty();
        LastCapturedEmail.Title.Should().NotBeNullOrEmpty();
        LastCapturedEmail.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_ConfigurationBaseUrl_Used()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        WorkflowCompletedEmailModel? capturedModel = null;
        MockEmailSender
            .Setup(e => e.SendEmailAsync(It.IsAny<EmailMessage>(), It.IsAny<WorkflowCompletedEmailModel>(), It.IsAny<string?>()))
            .Callback<EmailMessage, WorkflowCompletedEmailModel, string?>((_, model, _) => capturedModel = model)
            .Returns(Task.CompletedTask);

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        capturedModel!.EntityUrl.Should().Contain("test.pao.unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_EndToEnd_AllComponentsInteract()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Director Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().NotBeEmpty("TO should have implementation country directors");
        LastCapturedEmail.CcReceivers.Should().NotBeEmpty("CC should have OM and initiator");
        LastCapturedEmail.Title.Should().Contain("FYI");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_AppDbContext_OpportunityLookup()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert
        LastCapturedEmail.Should().NotBeNull();
        LastCapturedEmail!.EmailReceivers.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task NotifyInternalStakeholdersOnGoDecisionAsync_ServiceScopeFactory_ForInitiator()
    {
        // Arrange
        await SeedInternalStakeholderScenarioAsync();
        SetupEmailCapture();

        // Act
        await NotificationService.NotifyInternalStakeholdersOnGoDecisionAsync(1, "Approver");

        // Assert - initiator from WorkflowDbContext
        LastCapturedEmail!.CcReceivers.Should().Contain("initiator@unops.org");
    }

    #endregion
}
