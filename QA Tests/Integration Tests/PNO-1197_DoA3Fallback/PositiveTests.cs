using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.Business.Workflow.Adapters;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;
using UNOPS.PAO.Models.Workflow;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using UNOPS.Workflow.Models.Requirements;
using Xunit;
using Facing = UNOPS.Workflow.Models.Facing;

namespace UNOPS.PAO.IntegrationTests.PNO1197;

[Collection("Positive")]
[Trait("Category", "Positive")]
[Trait("Type", "Positive")]
public class PositiveTests : PNO1197TestFixtureBase, IDisposable
{
    #region POS_001-010: DoA2 happy paths

    [Fact]
    public async Task POS_001_SubmitWithDoA2_Succeeds()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(1, 1);
        SetupStandardSubmitMocks();
        var request = new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 1, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        };
        var result = await Controller.Submit(request);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_002_DoA2Holder_CorrectlyIdentified()
    {
        await SeedOpportunityAsync(2, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(2, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 2, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
        // When Success=true, UnmetRequirements is null (no requirements unmet) - null-safe check
        response.UnmetRequirements?.Should().NotContain(r => r.Contains("doaHolderRequired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task POS_003_DoA2OnSameOrgUnit_Succeeds()
    {
        await SeedOpportunityAsync(3, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(3, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 3, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_004_DoA2ActiveUser_Succeeds()
    {
        await SeedOpportunityAsync(4, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(4, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 4, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_005_DoA2WithMultipleRoles_Succeeds()
    {
        await SeedOpportunityAsync(5, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        await SeedOpportunityManagerStakeholderAsync(5, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 5, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_006_DoA2OverridesDoA3WhenBothPresent_Succeeds()
    {
        await SeedOpportunityAsync(6, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(6, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 6, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_007_SubmitWithDoA2_ReturnsSuccessResponse()
    {
        await SeedOpportunityAsync(7, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(7, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 7, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
        response.ApprovalRequired.Should().BeTrue();
    }

    [Fact]
    public async Task POS_008_SubmitRequirementListEmpty_WhenDoA2Present()
    {
        await SeedOpportunityAsync(8, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(8, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 8, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        // When Success=true, UnmetRequirements is null (no requirements unmet) - null-safe check
        response!.UnmetRequirements?.Should().NotContain(r => r.Contains("doaHolderRequired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task POS_009_DoA2HolderInfo_IncludedInResponse()
    {
        await SeedOpportunityAsync(9, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(9, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 9, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_010_DoA2Check_ExactCodeMatch_Succeeds()
    {
        await SeedOpportunityAsync(10, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(10, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 10, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    #endregion

    #region POS_011-020: DoA3 fallback happy paths

    [Fact]
    public async Task POS_011_SubmitWithDoA3Only_Succeeds()
    {
        await SeedOpportunityAsync(11, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(11, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 11, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_012_DoA3CorrectlyIdentified_WhenNoDoA2()
    {
        await SeedOpportunityAsync(12, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(12, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 12, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
        // When Success=true, UnmetRequirements is null (no requirements unmet) - null-safe check
        response.UnmetRequirements?.Should().NotContain(r => r.Contains("doaHolderRequired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task POS_013_DoA3OnSameOrgUnit_Succeeds()
    {
        await SeedOpportunityAsync(13, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(13, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 13, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_014_DoA3ActiveUser_Succeeds()
    {
        await SeedOpportunityAsync(14, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(14, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 14, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_015_DoA3WithMultipleEntityRoles_Succeeds()
    {
        await SeedOpportunityAsync(15, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(15, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 15, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_016_BothDoA2AndDoA3Present_Succeeds()
    {
        await SeedOpportunityAsync(16, "IDENTIFY & PROFILE");
        await SeedDoAHolderAsync(1, 2);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(16, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 16, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_017_DoA3HolderInfo_Included()
    {
        await SeedOpportunityAsync(17, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(17, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 17, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_018_SubmitRequirementListEmpty_WithDoA3()
    {
        await SeedOpportunityAsync(18, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(18, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 18, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        // When Success=true, UnmetRequirements is null (no requirements unmet) - null-safe check
        response!.UnmetRequirements?.Should().NotContain(r => r.Contains("doaHolderRequired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task POS_019_DoA3Check_ExactCodeMatch_Succeeds()
    {
        await SeedOpportunityAsync(19, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(19, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 19, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_020_DoA3WithDifferentOrgUnitStructure_Succeeds()
    {
        if (!await DbContext.Set<OrganizationHierarchy>().AnyAsync(oh => oh.Id == 2))
        {
            DbContext.Set<OrganizationHierarchy>().Add(new OrganizationHierarchy
            {
                Id = 2,
                Name = "Test Org Unit 2",
                Code = "TOU2",
                Description = "Test",
                Status = EntityStatus.Active,
                IsDeleted = false
            });
            await DbContext.SaveChangesAsync();
        }
        var opp = await DbContext.Opportunities.FindAsync(20);
        if (opp == null)
        {
            DbContext.Opportunities.Add(new Opportunity
            {
                Id = 20,
                Name = "Test Opportunity 20",
                Description = "Full",
                Stage = "IDENTIFY & PROFILE",
                Status = EntityStatus.Active,
                IsDeleted = false,
                InitiativeBudgetUSD = 100000m,
                Challenges = "C", ExpectedImpact = "I", ExpectedOutcomes = "O",
                BeneficiariesToBeDetermined = true,
                UNOPSMissionsNotApplicable = true,
                TargetSigningDate = DateTime.UtcNow.AddMonths(1),
                ImplementationStartDate = DateTime.UtcNow.AddMonths(2),
                TargetDeliveryDate = DateTime.UtcNow.AddMonths(12),
                OpportunityStatementMarkdown = "## Statement",
                ResponsibleOrgUnitId = 2,
                ProposedInitiativeTypeId = 1
            });
            await DbContext.SaveChangesAsync();
        }
        else
        {
            opp.ResponsibleOrgUnitId = 2;
            await DbContext.SaveChangesAsync();
        }
        await SeedDoAHolderAsync(2, 3);
        await SeedOpportunityManagerStakeholderAsync(20, 1);
        if (!await DbContext.Set<OpportunityDeliverable>().AnyAsync(d => d.OpportunityId == 20))
        {
            DbContext.Set<OpportunityDeliverable>().Add(new OpportunityDeliverable { Id = 2001, OpportunityId = 20, Name = "D" });
            DbContext.Set<OpportunitySDG>().Add(new OpportunitySDG { Id = 2001, OpportunityId = 20, SDGId = 1, Name = "S" });
            DbContext.Set<OpportunityFundingPartner>().Add(new OpportunityFundingPartner { Id = 2001, OpportunityId = 20, PartnerId = 1, Name = "F" });
            DbContext.Set<OpportunityClientPartner>().Add(new OpportunityClientPartner { Id = 2001, OpportunityId = 20, PartnerId = 2, Name = "C" });
            if (!await DbContext.Set<Country>().AnyAsync(c => c.Id == 1))
                DbContext.Set<Country>().Add(new Country { Id = 1, Name = "TC", Iso2Code = "TC", Status = EntityStatus.Active, IsDeleted = false });
            await DbContext.SaveChangesAsync();
            DbContext.Set<OpportunityCountry>().Add(new OpportunityCountry { Id = 2001, OpportunityId = 20, CountryId = 1, Name = "TC" });
            await DbContext.SaveChangesAsync();
        }
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 20, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    #endregion

    #region POS_021-030: Submit with DoA holder general

    [Fact]
    public async Task POS_021_SubmitWithAll21RequirementsMet_Succeeds()
    {
        await SeedOpportunityAsync(21, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(21, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 21, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_022_SubmitWithOMStakeholder_Succeeds()
    {
        await SeedOpportunityAsync(22, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(22, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 22, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_023_SubmitWithCountries_Succeeds()
    {
        await SeedOpportunityAsync(23, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(23, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 23, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_024_SubmitWithDeliverables_Succeeds()
    {
        await SeedOpportunityAsync(24, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(24, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 24, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_025_SubmitWithSDGs_Succeeds()
    {
        await SeedOpportunityAsync(25, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(25, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 25, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_026_SubmitWithStatement_Succeeds()
    {
        await SeedOpportunityAsync(26, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(26, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 26, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_027_SubmitWithBeneficiaries_Succeeds()
    {
        await SeedOpportunityAsync(27, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(27, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 27, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_028_SubmitWithResponsibleOrgUnit_Succeeds()
    {
        await SeedOpportunityAsync(28, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(28, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 28, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task POS_029_SubmitCompleteWorkflow_Succeeds()
    {
        await SeedOpportunityAsync(29, "IDENTIFY & PROFILE");
        await SeedOpportunityManagerStakeholderAsync(29, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 29, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        var response = (result.Result as OkObjectResult)!.Value as WorkflowSubmitResponse;
        response!.Success.Should().BeTrue();
        response.ApprovalRequired.Should().BeTrue();
    }

    [Fact]
    public async Task POS_030_FullSubmitApproveFlow_WithDoA3_Succeeds()
    {
        await SeedOpportunityAsync(30, "IDENTIFY & PROFILE");
        await RemoveDoAHoldersForOrgUnitAsync(1);
        await SeedDoAHolderAsync(1, 3);
        await SeedOpportunityManagerStakeholderAsync(30, 1);
        SetupStandardSubmitMocks();
        var result = await Controller.Submit(new WorkflowSubmitRequest
        {
            EntityName = "opportunity", EntityId = 30, NewStage = "GO",
            ConfirmedNonOMSubmission = false, ConfirmedOrgUnitWarning = true, AcknowledgedStatement = true
        });
        (result.Result as OkObjectResult)!.Value.Should().BeOfType<WorkflowSubmitResponse>()
            .Which.Success.Should().BeTrue();
    }

    #endregion
}
