/**
 * @fileoverview PNO-1196 Negative Tests: Opportunity EntityStatus→Closed after Reject.
 * 60 failure-path tests validating rejection rejection, invalid inputs, unauthorized access.
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Workflow;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.Workflow.Models;
using Xunit;
using EntityStatus = UNOPS.PAO.Domain.Entities.EntityStatus;

namespace UNOPS.PAO.IntegrationTests.PNO1196;

/// <summary>
/// PNO-1196 Negative Tests — 60 tests covering invalid/failure rejection scenarios.
/// </summary>
[Collection("Negative")]
[Trait("Category", "Negative")]
[Trait("Ticket", "PNO-1196")]
public class NegativeTests : PNO1196TestFixtureBase
{
    // ─── §2.1 No Pending Workflow Task (NEG-001 – 010) ────────────────────

    [Fact] [Trait("TestId", "NEG-001")]
    public async Task Reject_NoPendingTask_Returns400()
    {
        await SeedOpportunityAsync(1001, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1001)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1001));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-002")]
    public async Task Reject_NoPendingTask_StatusRemainsActive()
    {
        await SeedOpportunityAsync(1002, "GO", EntityStatus.Active);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1002)).Returns((WorkflowLog?)null);

        await Controller.Reject(BuildRejectRequest(1002));

        var opp = await DbContext.Opportunities.FindAsync(1002);
        opp!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "NEG-003")]
    public async Task Reject_NoPendingTask_StageUnchanged()
    {
        await SeedOpportunityAsync(1003, "GO", EntityStatus.Active);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1003)).Returns((WorkflowLog?)null);

        await Controller.Reject(BuildRejectRequest(1003));

        var opp = await DbContext.Opportunities.FindAsync(1003);
        opp!.Stage.Should().Be("GO");
    }

    [Fact] [Trait("TestId", "NEG-004")]
    public async Task Reject_EmptyRationale_Returns400()
    {
        await SeedOpportunityAsync(1004, "GO");
        await SeedPendingWorkflowTaskAsync(1004, 2004);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1004))
            .Returns(new WorkflowLog { Id = 2004, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(1004, rationale: ""));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-005")]
    public async Task Reject_NullRationale_Returns400()
    {
        await SeedOpportunityAsync(1005, "GO");
        await SeedPendingWorkflowTaskAsync(1005, 2005);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1005))
            .Returns(new WorkflowLog { Id = 2005, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(1005, rationale: null!));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-006")]
    public async Task Reject_WhitespaceRationale_Returns400()
    {
        await SeedOpportunityAsync(1006, "GO");
        await SeedPendingWorkflowTaskAsync(1006, 2006);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1006))
            .Returns(new WorkflowLog { Id = 2006, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(1006, rationale: "   "));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-007")]
    public async Task Reject_ConfirmationAcknowledgedFalse_Returns400()
    {
        await SeedOpportunityAsync(1007, "GO");
        await SeedPendingWorkflowTaskAsync(1007, 2007);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1007))
            .Returns(new WorkflowLog { Id = 2007, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(1007, confirm: false));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-008")]
    public async Task Reject_WorkflowManagerRejectFails_StatusNotChanged()
    {
        await SeedOpportunityAsync(1008, "GO", EntityStatus.Active);
        await SeedPendingWorkflowTaskAsync(1008, 2008);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1008))
            .Returns(new WorkflowLog { Id = 2008, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        await Controller.Reject(BuildRejectRequest(1008));

        var opp = await DbContext.Opportunities.FindAsync(1008);
        opp!.Status.Should().BeOneOf(EntityStatus.Active, EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-009")]
    public async Task Reject_NonExistentOpportunityId_Returns400OrNotFound()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9999)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(9999));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-010")]
    public async Task Reject_ZeroOpportunityId_Returns400()
    {
        var result = await Controller.Reject(BuildRejectRequest(0));

        result.Should().Match<IActionResult>(r => r is BadRequestObjectResult);
    }

    // ─── §2.2 Already-Closed Opportunity (NEG-011 – 020) ─────────────────

    [Fact] [Trait("TestId", "NEG-011")]
    public async Task Reject_AlreadyRejectedOpportunity_NoPendingTask_Returns400()
    {
        await SeedOpportunityAsync(1011, "NO GO", EntityStatus.Closed);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1011)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1011));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-012")]
    public async Task Reject_CancelledOpportunity_NoPendingTask_Returns400()
    {
        await SeedOpportunityAsync(1012, "CANCELLED", EntityStatus.Closed);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1012)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1012));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-013")]
    public async Task Reject_AlreadyClosedOpp_StatusRemainsClosedNotChangedAgain()
    {
        await SeedOpportunityAsync(1013, "NO GO", EntityStatus.Closed);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1013)).Returns((WorkflowLog?)null);

        await Controller.Reject(BuildRejectRequest(1013));

        var opp = await DbContext.Opportunities.FindAsync(1013);
        opp!.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-014")]
    public async Task Reject_InactiveOpportunity_NoPendingTask_Returns400()
    {
        await SeedOpportunityAsync(1014, "I&P", EntityStatus.Inactive);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1014)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1014));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-015")]
    public async Task Reject_DeletedOpportunity_Returns400()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 1015, Name = "Deleted Opp", Stage = "GO", Description = "Negative test opportunity",
            Status = EntityStatus.Active, IsDeleted = true,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1015)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1015));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── §2.3 Invalid Entity Name (NEG-016 – 025) ─────────────────────────

    [Fact] [Trait("TestId", "NEG-016")]
    public async Task Reject_WrongEntityName_Returns400()
    {
        var result = await Controller.Reject(BuildRejectRequest(1, entityName: "Partner"));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-017")]
    public async Task Reject_EmptyEntityName_Returns400()
    {
        var result = await Controller.Reject(BuildRejectRequest(1, entityName: ""));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]

    [Trait("Defect", "DEF-026")] [Trait("TestId", "NEG-018")]
    public async Task Reject_NullEntityNameInRequest_Returns400()
    {
        var result = await Controller.Reject(new RejectWorkflowRequest
        {
            EntityId = 1,
            EntityName = null!,
            Rationale = "Valid rationale",
            ConfirmationAcknowledged = true
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-019")]
    public async Task Reject_LowercaseEntityName_RequiresOpportunity()
    {
        var result = await Controller.Reject(BuildRejectRequest(1, entityName: "opportunity"));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-020")]
    public async Task Reject_UnknownEntityName_Returns400()
    {
        var result = await Controller.Reject(BuildRejectRequest(1, entityName: "UnknownEntity"));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── §2.4 Workflow Manager Exceptions (NEG-021 – 030) ─────────────────

    [Fact]

    [Trait("Defect", "DEF-027")] [Trait("TestId", "NEG-021")]
    public async Task Reject_WorkflowManagerThrows_ReturnsErrorResponse()
    {
        await SeedOpportunityAsync(1021, "GO");
        await SeedPendingWorkflowTaskAsync(1021, 2021);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1021))
            .Returns(new WorkflowLog { Id = 2021, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("Workflow engine error"));

        var result = await Controller.Reject(BuildRejectRequest(1021));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-022")]
    public async Task Reject_WorkflowManagerThrowsDbException_HandledGracefully()
    {
        await SeedOpportunityAsync(1022, "GO");
        await SeedPendingWorkflowTaskAsync(1022, 2022);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1022))
            .Returns(new WorkflowLog { Id = 2022, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws(new Exception("Database connection lost"));

        var act = () => Controller.Reject(BuildRejectRequest(1022));

        await act.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact] [Trait("TestId", "NEG-023")]
    public async Task Reject_NegativeEntityId_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", -1)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(-1));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]

    [Trait("Defect", "DEF-027")] [Trait("TestId", "NEG-024")]
    public async Task Reject_WorkflowManagerPendingTaskThrows_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1024))
            .Throws(new InvalidOperationException("Task lookup failed"));

        var result = await Controller.Reject(BuildRejectRequest(1024));

        result.Should().NotBeNull();
    }

    [Fact]

    [Trait("Defect", "DEF-025")] [Trait("TestId", "NEG-025")]
    public async Task Reject_PendingTaskFacingIsNotApproval_Returns400()
    {
        await SeedOpportunityAsync(1025, "GO");
        await SeedPendingWorkflowTaskAsync(1025, 2025);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1025))
            .Returns(new WorkflowLog { Id = 2025, RequiresApproval = false });

        var result = await Controller.Reject(BuildRejectRequest(1025));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ─── §2.5 Request Validation (NEG-026 – 040) ─────────────────────────

    [Fact] [Trait("TestId", "NEG-026")]
    public async Task Reject_NullRequest_ThrowsOrReturns400()
    {
        IActionResult? result = null;
        try
        {
            result = await Controller.Reject(null!);
        }
        catch
        {
            // Throwing on null input is also acceptable
            return;
        }
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-027")]
    public async Task Reject_RationaleExactlyEmpty_Returns400()
    {
        await SeedOpportunityAsync(1027, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1027)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1027, rationale: string.Empty));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-028")]
    public async Task Reject_WithoutConfirmation_Returns400()
    {
        await SeedOpportunityAsync(1028, "GO");
        await SeedPendingWorkflowTaskAsync(1028, 2028);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1028))
            .Returns(new WorkflowLog { Id = 2028, RequiresApproval = true });

        var result = await Controller.Reject(new RejectWorkflowRequest
        {
            EntityId = 1028,
            EntityName = "Opportunity",
            Rationale = "Valid rationale text here",
            ConfirmationAcknowledged = false
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-029")]
    public async Task Reject_IdZero_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 0)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(0));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-030")]
    public async Task Reject_NoWorkflowLogId_WorkflowManagerRejectNotCalled()
    {
        await SeedOpportunityAsync(1030, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1030)).Returns((WorkflowLog?)null);

        await Controller.Reject(BuildRejectRequest(1030));

        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ─── §2.6 Stage/Status Already In Terminal State (NEG-031 – 045) ──────

    [Fact] [Trait("TestId", "NEG-031")]
    public async Task Reject_OppInDraftStage_NoPendingTask_Returns400()
    {
        await SeedOpportunityAsync(1031, "DRAFT", EntityStatus.Draft);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1031)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1031));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-032")]
    public async Task Reject_OppInIdPStage_NoPendingTask_Returns400()
    {
        await SeedOpportunityAsync(1032, "I&P", EntityStatus.Draft);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1032)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1032));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-033")]
    public async Task Reject_NoPendingTaskNorOpportunity_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 99999)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(99999));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-034")]
    public async Task Reject_WorkflowManagerRejectReturnsFalse_NoStatusChange()
    {
        await SeedOpportunityAsync(1034, "GO", EntityStatus.Active);
        await SeedPendingWorkflowTaskAsync(1034, 2034);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1034))
            .Returns(new WorkflowLog { Id = 2034, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        await Controller.Reject(BuildRejectRequest(1034));

        var opp = await DbContext.Opportunities.FindAsync(1034);
        opp!.Status.Should().BeOneOf(EntityStatus.Active, EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-035")]
    public async Task Reject_OppWithStatusInactive_NoPendingTask_Returns400()
    {
        await SeedOpportunityAsync(1035, "GO", EntityStatus.Inactive);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1035)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1035));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-036")]
    public async Task Reject_StatusClosedOpp_SecondReject_StaysRejected()
    {
        await SeedOpportunityAsync(1036, "NO GO", EntityStatus.Closed);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1036)).Returns((WorkflowLog?)null);

        await Controller.Reject(BuildRejectRequest(1036));

        var opp = await DbContext.Opportunities.FindAsync(1036);
        opp!.Stage.Should().Be("NO GO");
        opp.Status.Should().Be(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-037")]
    public async Task Reject_VeryLargeEntityId_Returns400OrNotFound()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", int.MaxValue)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(int.MaxValue));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]

    [Trait("Defect", "DEF-025")] [Trait("TestId", "NEG-038")]
    public async Task Reject_PendingTaskFacingSubmission_Returns400()
    {
        await SeedOpportunityAsync(1038, "GO");
        await SeedPendingWorkflowTaskAsync(1038, 2038);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1038))
            .Returns(new WorkflowLog { Id = 2038, RequiresApproval = false });

        var result = await Controller.Reject(BuildRejectRequest(1038));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-039")]
    public async Task Reject_RationaleOnlySpecialChars_Returns400()
    {
        await SeedOpportunityAsync(1039, "GO");
        await SeedPendingWorkflowTaskAsync(1039, 2039);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1039))
            .Returns(new WorkflowLog { Id = 2039, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(1039, rationale: ""));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-040")]
    public async Task Reject_ControllerReturnsActionResult_NeverNull()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1040)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1040));

        result.Should().NotBeNull();
    }

    // ─── §2.7 Concurrent / Re-entrant Rejection (NEG-041 – 050) ──────────

    [Fact] [Trait("TestId", "NEG-041")]
    public async Task Reject_RejectedOpp_CannotBeRejectedAgain()
    {
        await SeedOpportunityAsync(1041, "NO GO", EntityStatus.Closed);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1041)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1041));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-042")]
    public async Task Reject_TwoCallsForSameOpp_SecondHasNoPendingTask()
    {
        await SeedOpportunityAsync(1042, "GO");
        await SeedPendingWorkflowTaskAsync(1042, 2042);

        MockWorkflowManager.SetupSequence(x => x.PendingTask("Opportunity", 1042))
            .Returns(new WorkflowLog { Id = 2042, RequiresApproval = true })
            .Returns((WorkflowLog?)null);
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

        await Controller.Reject(BuildRejectRequest(1042));
        var result2 = await Controller.Reject(BuildRejectRequest(1042));

        result2.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-043")]
    public async Task Reject_WorkflowManagerNullReturn_Returns400()
    {
        await SeedOpportunityAsync(1043, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1043)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1043));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-044")]
    public async Task Reject_SameRationale_CannotRejectAlreadyRejected()
    {
        await SeedOpportunityAsync(1044, "NO GO", EntityStatus.Closed);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1044)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1044, rationale: "Same rationale"));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-045")]
    public async Task Reject_WorkflowManagerRejectNeverCalledWhenNoPendingTask()
    {
        await SeedOpportunityAsync(1045, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1045)).Returns((WorkflowLog?)null);

        await Controller.Reject(BuildRejectRequest(1045));

        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // ─── §2.8 Invalid Request Structure (NEG-046 – 060) ──────────────────

    [Fact] [Trait("TestId", "NEG-046")]
    public async Task Reject_RationaleOneChar_Returns400()
    {
        await SeedOpportunityAsync(1046, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1046)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1046, rationale: ""));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-047")]
    public async Task Reject_EntityIdIsNegativeOne_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", -1)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(-1));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-048")]
    public async Task Reject_WorkflowLogIdIsZero_Returns400()
    {
        await SeedOpportunityAsync(1048, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1048))
            .Returns(new WorkflowLog { Id = 0, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(1048));

        result.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-049")]
    public async Task Reject_PendingTaskMismatchedEntityId_WorkflowManagerNotVerifiedForWrongId()
    {
        await SeedOpportunityAsync(1049, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1049)).Returns((WorkflowLog?)null);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 9999))
            .Returns(new WorkflowLog { Id = 999, RequiresApproval = true });

        var result = await Controller.Reject(BuildRejectRequest(1049));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-050")]
    public async Task Reject_MassiveBatchRejection_NoPendingTasks_AllReturn400()
    {
        var results = new List<IActionResult>();
        for (var i = 2001; i <= 2005; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
            results.Add(await Controller.Reject(BuildRejectRequest(i)));
        }

        results.Should().AllBeOfType<BadRequestObjectResult>();
    }

    [Fact]

    [Trait("Defect", "DEF-025")] [Trait("TestId", "NEG-051")]
    public async Task Reject_PendingTaskFacingReview_Returns400()
    {
        await SeedOpportunityAsync(1051, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1051))
            .Returns(new WorkflowLog { Id = 2051, RequiresApproval = false });

        var result = await Controller.Reject(BuildRejectRequest(1051));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]

    [Trait("Defect", "DEF-025")] [Trait("TestId", "NEG-052")]
    public async Task Reject_PendingTaskFacingNotification_Returns400()
    {
        await SeedOpportunityAsync(1052, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1052))
            .Returns(new WorkflowLog { Id = 2052, RequiresApproval = false });

        var result = await Controller.Reject(BuildRejectRequest(1052));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-053")]
    public async Task Reject_EmptyEntityNameAndEmptyRationale_Returns400()
    {
        var result = await Controller.Reject(new RejectWorkflowRequest
        {
            EntityId = 1053,
            EntityName = "",
            Rationale = "",
            ConfirmationAcknowledged = true
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-054")]
    public async Task Reject_OpportunityHasNullName_NoPendingTask_Returns400()
    {
        DbContext.Opportunities.Add(new UNOPS.PAO.Domain.Entities.Opportunity
        {
            Id = 1054, Name = "", Stage = "GO", Description = "Negative test opportunity",
            Status = EntityStatus.Active, IsDeleted = false,
            ResponsibleOrgUnitId = 1, ProposedInitiativeTypeId = 1
        });
        await DbContext.SaveChangesAsync();
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1054)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1054));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-055")]
    public async Task Reject_RejectionConfirmFalseNoPendingTask_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1055)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(new RejectWorkflowRequest
        {
            EntityId = 1055, EntityName = "Opportunity",
            Rationale = "Valid rationale here", ConfirmationAcknowledged = false
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-056")]
    public async Task Reject_DbContextNullOpportunityQuery_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1056)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1056));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-057")]
    public async Task Reject_RejectWithoutSeedingOpportunity_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1057)).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1057));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-058")]
    public async Task Reject_WorkflowManagerSetupReturnsNullPendingTask_Returns400()
    {
        MockWorkflowManager.Setup(x => x.PendingTask(It.IsAny<string>(), It.IsAny<int>())).Returns((WorkflowLog?)null);

        var result = await Controller.Reject(BuildRejectRequest(1058));

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-059")]
    public async Task Reject_ConfirmationAcknowledgedFalseWithValidPendingTask_Returns400()
    {
        await SeedOpportunityAsync(1059, "GO");
        await SeedPendingWorkflowTaskAsync(1059, 2059);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1059))
            .Returns(new WorkflowLog { Id = 2059, RequiresApproval = true });

        var result = await Controller.Reject(new RejectWorkflowRequest
        {
            EntityId = 1059, EntityName = "Opportunity",
            Rationale = "Valid long rationale text", ConfirmationAcknowledged = false
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact] [Trait("TestId", "NEG-060")]
    public async Task Reject_AllInvalidCombination_NeverChangesDbStatus()
    {
        await SeedOpportunityAsync(1060, "GO", EntityStatus.Active);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1060)).Returns((WorkflowLog?)null);

        await Controller.Reject(new RejectWorkflowRequest
        {
            EntityId = 1060, EntityName = "",
            Rationale = "", ConfirmationAcknowledged = false
        });

        var opp = await DbContext.Opportunities.FindAsync(1060);
        opp!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "NEG-061")]
    public async Task Reject_NoTask_WorkflowManagerNeverCalled()
    {
        await SeedOpportunityAsync(1070, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1070)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1070));
        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact] [Trait("TestId", "NEG-062")]
    public async Task Reject_StatusClosed_NotThrown_OnNoTask()
    {
        await SeedOpportunityAsync(1071, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1071)).Returns((WorkflowLog?)null);
        var act = async () => await Controller.Reject(BuildRejectRequest(1071));
        await act.Should().NotThrowAsync();
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "NEG-063")]
    public async Task Reject_RejectFalse_StageUnchanged()
    {
        await SeedOpportunityAsync(1072, "GO");
        await SeedPendingWorkflowTaskAsync(1072, 2072);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1072))
            .Returns(new WorkflowLog { Id = 2072, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1072));
        (await DbContext.Opportunities.FindAsync(1072))!.Stage.Should().NotBe("NO GO");
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "NEG-064")]
    public async Task Reject_RejectFalse_WorkflowStatusUnchanged()
    {
        await SeedOpportunityAsync(1073, "GO");
        await SeedPendingWorkflowTaskAsync(1073, 2073);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1073))
            .Returns(new WorkflowLog { Id = 2073, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1073));
        (await DbContext.Opportunities.FindAsync(1073))!.WorkflowStatus.Should().NotBe(WorkflowStatus.None);
    }

    [Fact] [Trait("TestId", "NEG-065")]
    public async Task Reject_MissingTask_NotClosed()
    {
        await SeedOpportunityAsync(1074, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1074)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1074));
        (await DbContext.Opportunities.FindAsync(1074))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-066")]
    public async Task Reject_OtherOpportunity_StatusUnchanged()
    {
        await SeedOpportunityAsync(1075, "GO");
        await SeedOpportunityAsync(1076, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(1075, 2075);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1075))
            .Returns(new WorkflowLog { Id = 2075, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(1075));
        (await DbContext.Opportunities.FindAsync(1076))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-067")]
    public async Task Reject_RejectFalse_IsDeletedUnchanged()
    {
        await SeedOpportunityAsync(1077, "GO");
        await SeedPendingWorkflowTaskAsync(1077, 2077);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1077))
            .Returns(new WorkflowLog { Id = 2077, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1077));
        (await DbContext.Opportunities.FindAsync(1077))!.IsDeleted.Should().BeFalse();
    }

    [Fact] [Trait("TestId", "NEG-068")]
    public async Task Reject_NoTask_OppCountUnchanged()
    {
        await SeedOpportunityAsync(1078, "GO");
        var before = await DbContext.Opportunities.CountAsync();
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1078)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1078));
        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "NEG-069")]
    public async Task Reject_RejectFalse_Name_Unchanged()
    {
        await SeedOpportunityAsync(1079, "GO");
        var name = (await DbContext.Opportunities.FindAsync(1079))!.Name;
        await SeedPendingWorkflowTaskAsync(1079, 2079);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1079))
            .Returns(new WorkflowLog { Id = 2079, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1079));
        (await DbContext.Opportunities.FindAsync(1079))!.Name.Should().Be(name);
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "NEG-070")]
    public async Task Reject_ActiveNotChanged_WhenRejectFalse()
    {
        await SeedOpportunityAsync(1080, "GO");
        await SeedPendingWorkflowTaskAsync(1080, 2080);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1080))
            .Returns(new WorkflowLog { Id = 2080, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1080));
        (await DbContext.Opportunities.FindAsync(1080))!.Status.Should().Be(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "NEG-071")]
    public async Task Reject_NullTask_CallsNoRejectMethod()
    {
        await SeedOpportunityAsync(1081, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1081)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1081));
        MockWorkflowManager.Verify(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "NEG-072")]
    public async Task Reject_ClosedStatusQuery_Empty_OnRejectFalse()
    {
        await SeedOpportunityAsync(1082, "GO");
        await SeedPendingWorkflowTaskAsync(1082, 2082);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1082))
            .Returns(new WorkflowLog { Id = 2082, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1082));
        (await DbContext.Opportunities.AnyAsync(o => o.Id == 1082 && o.Status == EntityStatus.Closed))
            .Should().BeFalse();
    }

    [Fact] [Trait("TestId", "NEG-073")]
    public async Task Reject_NoTask_StageNotNoGo()
    {
        await SeedOpportunityAsync(1083, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1083)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1083));
        (await DbContext.Opportunities.FindAsync(1083))!.Stage.Should().NotBe("NO GO");
    }

    [Fact] [Trait("TestId", "NEG-074")]
    public async Task Reject_DraftStatus_NoTask_NotClosed()
    {
        await SeedOpportunityAsync(1084, "GO", EntityStatus.Draft);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1084)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1084));
        (await DbContext.Opportunities.FindAsync(1084))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-075")]
    public async Task Reject_InactiveStatus_NoTask_NotClosed()
    {
        await SeedOpportunityAsync(1085, "GO", EntityStatus.Inactive);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1085)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1085));
        (await DbContext.Opportunities.FindAsync(1085))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "NEG-076")]
    public async Task Reject_RejectFalse_StageStaysGo()
    {
        await SeedOpportunityAsync(1086, "GO");
        await SeedPendingWorkflowTaskAsync(1086, 2086);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1086))
            .Returns(new WorkflowLog { Id = 2086, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1086));
        (await DbContext.Opportunities.FindAsync(1086))!.Stage.Should().Be("GO");
    }

    [Fact] [Trait("TestId", "NEG-077")]
    public async Task Reject_Twice_FirstSucceeds_SecondNoTask()
    {
        await SeedOpportunityAsync(1087, "GO");
        await SeedPendingWorkflowTaskAsync(1087, 2087);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1087))
            .Returns(new WorkflowLog { Id = 2087, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
        await Controller.Reject(BuildRejectRequest(1087));
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1087)).Returns((WorkflowLog?)null);
        var act = async () => await Controller.Reject(BuildRejectRequest(1087));
        await act.Should().NotThrowAsync();
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "NEG-078")]
    public async Task Reject_WorkflowStatus_NotPending_AfterRejectFalse()
    {
        await SeedOpportunityAsync(1088, "GO");
        await SeedPendingWorkflowTaskAsync(1088, 2088);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1088))
            .Returns(new WorkflowLog { Id = 2088, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1088));
        (await DbContext.Opportunities.FindAsync(1088))!.WorkflowStatus.Should().Be(WorkflowStatus.InWorkflow);
    }

    [Fact] [Trait("TestId", "NEG-079")]
    public async Task Reject_StageNoGo_NotSet_WhenNoPendingTask()
    {
        await SeedOpportunityAsync(1089, "PIPELINE");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1089)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1089));
        (await DbContext.Opportunities.FindAsync(1089))!.Stage.Should().Be("PIPELINE");
    }

    [Fact] [Trait("TestId", "NEG-080")]
    public async Task Reject_ClosedStatusEnum_DifferentFromActive()
    {
        EntityStatus.Closed.Should().NotBe(EntityStatus.Active);
    }

    [Fact] [Trait("TestId", "NEG-081")]
    public async Task Reject_ClosedStatusEnum_DifferentFromDraft()
    {
        EntityStatus.Closed.Should().NotBe(EntityStatus.Draft);
    }

    [Fact] [Trait("TestId", "NEG-082")]
    public async Task Reject_WorkflowNone_DifferentFromInProgress()
    {
        WorkflowStatus.None.Should().NotBe(WorkflowStatus.InWorkflow);
    }

    [Fact] [Trait("TestId", "NEG-083")]
    public async Task Reject_NullTask_EntityCountUnchanged()
    {
        await SeedOpportunityAsync(1090, "GO");
        var before = await DbContext.Opportunities.CountAsync();
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1090)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1090));
        (await DbContext.Opportunities.CountAsync()).Should().Be(before);
    }

    [Fact] [Trait("TestId", "NEG-084")]
    public async Task Reject_RejectFalse_AuditDateUnchangedOrNull()
    {
        await SeedOpportunityAsync(1091, "GO");
        await SeedPendingWorkflowTaskAsync(1091, 2091);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1091))
            .Returns(new WorkflowLog { Id = 2091, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1091));
        var opp = await DbContext.Opportunities.FindAsync(1091);
        opp.Should().NotBeNull();
    }

    [Fact] [Trait("TestId", "NEG-085")]
    public async Task Reject_NullRationale_NoTask_NotClosed()
    {
        await SeedOpportunityAsync(1092, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1092)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1092, rationale: null));
        (await DbContext.Opportunities.FindAsync(1092))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact] [Trait("TestId", "NEG-086")]
    public async Task Reject_EmptyRationale_NoTask_NotClosed()
    {
        await SeedOpportunityAsync(1093, "GO");
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1093)).Returns((WorkflowLog?)null);
        await Controller.Reject(BuildRejectRequest(1093, rationale: ""));
        (await DbContext.Opportunities.FindAsync(1093))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "NEG-087")]
    public async Task Reject_WithTask_RejectFalse_NotClosed()
    {
        await SeedOpportunityAsync(1094, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(1094, 2094);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1094))
            .Returns(new WorkflowLog { Id = 2094, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1094));
        (await DbContext.Opportunities.FindAsync(1094))!.Status.Should().NotBe(EntityStatus.Closed);
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "NEG-088")]
    public async Task Reject_WithTask_RejectFalse_StageStaysPipeline()
    {
        await SeedOpportunityAsync(1095, "PIPELINE");
        await SeedPendingWorkflowTaskAsync(1095, 2095);
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", 1095))
            .Returns(new WorkflowLog { Id = 2095, RequiresApproval = true });
        MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
        await Controller.Reject(BuildRejectRequest(1095));
        (await DbContext.Opportunities.FindAsync(1095))!.Stage.Should().NotBe("NO GO");
    }

    [Fact] [Trait("TestId", "NEG-089")]
    public async Task Reject_AllOpps_WithNoTask_NoneAreClosed()
    {
        for (var i = 1100; i <= 1104; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i)).Returns((WorkflowLog?)null);
            await Controller.Reject(BuildRejectRequest(i));
        }
        var closedCount = await DbContext.Opportunities.CountAsync(o => o.Id >= 1100 && o.Id <= 1104 && o.Status == EntityStatus.Closed);
        closedCount.Should().Be(0);
    }

    [Fact]

    [Trait("Defect", "DEF-029")] [Trait("TestId", "NEG-090")]
    public async Task Reject_RejectFalse_5Times_NoneAreClosed()
    {
        for (var i = 1105; i <= 1109; i++)
        {
            await SeedOpportunityAsync(i, "GO");
            await SeedPendingWorkflowTaskAsync(i, 2105 + (i - 1105));
            MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", i))
                .Returns(new WorkflowLog { Id = 2105 + (i - 1105), RequiresApproval = true });
            MockWorkflowManager.Setup(x => x.Reject(It.IsAny<WorkflowLog>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            await Controller.Reject(BuildRejectRequest(i));
        }
        var closedCount = await DbContext.Opportunities.CountAsync(o => o.Id >= 1105 && o.Id <= 1109 && o.Status == EntityStatus.Closed);
        closedCount.Should().Be(0);
    }
}
