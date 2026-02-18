/**
 * @fileoverview PNO-1197 Load Tests: DoA Level 3 Fallback in Submit Validation.
 * Tests sustained load, spike load, and stress scenarios for submit with DoA validation.
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using System.Diagnostics;
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

/// <summary>
/// PNO-1197 Load tests: DoA Level 3 Fallback in Submit Validation.
/// Tests sustained load, spike load, and stress scenarios.
/// </summary>
[Collection("Load")]
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class LoadTests : PNO1197TestFixtureBase
{
    private static WorkflowSubmitRequest CreateValidSubmitRequest(int entityId = 1) => new()
    {
        EntityName = "opportunity",
        EntityId = entityId,
        NewStage = "GO",
        ConfirmedNonOMSubmission = false,
        ConfirmedOrgUnitWarning = true,
        AcknowledgedStatement = true
    };

    #region LOAD_001-003: Sustained Load

    [Fact]
    public async Task LOAD_001_TwentySequentialSubmitsWithDoAValidation_LessThan30s()
    {
        for (var i = 1; i <= 20; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();

        var sw = Stopwatch.StartNew();
        for (var i = 1; i <= 20; i++)
            await Controller.Submit(CreateValidSubmitRequest(i));
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(30000);
    }

    [Fact]
    public async Task LOAD_002_FiftyDoAValidations_LessThan20s()
    {
        for (var i = 1; i <= 50; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();

        var sw = Stopwatch.StartNew();
        for (var i = 1; i <= 50; i++)
            await Controller.Submit(CreateValidSubmitRequest(i));
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(20000);
    }

    [Fact]
    public async Task LOAD_003_TenCompleteSubmitFlows_LessThan15s()
    {
        for (var i = 1; i <= 10; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();

        var sw = Stopwatch.StartNew();
        for (var i = 1; i <= 10; i++)
            await Controller.Submit(CreateValidSubmitRequest(i));
        sw.Stop();

        sw.ElapsedMilliseconds.Should().BeLessThan(15000);
    }

    #endregion

    #region LOAD_004-006: Spike Load

    [Fact]
    public async Task LOAD_004_FiveSimultaneousSubmits()
    {
        for (var i = 1; i <= 5; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();

        var tasks = Enumerable.Range(1, 5)
            .Select(i => Controller.Submit(CreateValidSubmitRequest(i)))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task LOAD_005_TenSimultaneousDoAValidations()
    {
        for (var i = 1; i <= 10; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();

        var tasks = Enumerable.Range(1, 10)
            .Select(i => Controller.Submit(CreateValidSubmitRequest(i)))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(10);
    }

    [Fact]
    public async Task LOAD_006_BurstOfSubmitApproveFlows()
    {
        for (var i = 1; i <= 5; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();
        MockWorkflowManager.Setup(x => x.PendingTask("Opportunity", It.IsAny<int>())).Returns((WorkflowLog?)null);

        var tasks = Enumerable.Range(1, 5)
            .Select(i => Controller.Submit(CreateValidSubmitRequest(i)))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(5);
    }

    #endregion

    #region LOAD_007-010: Stress

    [Fact]
    public async Task LOAD_007_HundredSequentialDoAValidations()
    {
        for (var i = 1; i <= 100; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();

        var successCount = 0;
        for (var i = 1; i <= 100; i++)
        {
            var result = await Controller.Submit(CreateValidSubmitRequest(i));
            if (result.Result is OkObjectResult ok && (ok.Value as WorkflowSubmitResponse)?.Success == true)
                successCount++;
        }

        successCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task LOAD_008_SubmitWithIncreasingEntityUserRoles()
    {
        await SeedOpportunityAsync(1, "IDENTIFY & PROFILE");
        var entityRole = await DbContext.EntityRoles.FirstOrDefaultAsync(r => r.Code == "DoA2_OrganizationHierarchy");
        if (entityRole != null)
        {
            var maxId = await DbContext.EntityUserRoles.AnyAsync() ? await DbContext.EntityUserRoles.MaxAsync(e => e.Id) : 0;
            for (var i = 1; i <= 50; i++)
            {
                DbContext.EntityUserRoles.Add(new EntityUserRole
                {
                    Id = maxId + i,
                    UserId = 1,
                    EntityRoleId = entityRole.Id,
                    EntityRole = entityRole,
                    EntityId = 1,
                    EntityType = "OrganizationHierarchy",
                    Name = $"DoA2_{i}",
                    IsDeleted = false
                });
            }
            await DbContext.SaveChangesAsync();
        }
        SetupStandardSubmitMocks();

        var result = await Controller.Submit(CreateValidSubmitRequest());
        result.Result.Should().NotBeNull();
    }

    [Fact]
    public async Task LOAD_009_SystemRecoveryAfterHeavyDoALoad()
    {
        for (var i = 1; i <= 30; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();

        for (var round = 0; round < 3; round++)
        {
            var tasks = Enumerable.Range(1, 10)
                .Select(j => Controller.Submit(CreateValidSubmitRequest(j)))
                .ToArray();
            await Task.WhenAll(tasks);
        }

        var finalResult = await Controller.Submit(CreateValidSubmitRequest(1));
        finalResult.Result.Should().NotBeNull();
    }

    [Fact]
    public async Task LOAD_010_DoAValidationUnderConnectionPoolPressure()
    {
        for (var i = 1; i <= 25; i++)
        {
            await SeedOpportunityAsync(i, "IDENTIFY & PROFILE");
            await SeedOpportunityManagerStakeholderAsync(i, 1);
        }
        SetupStandardSubmitMocks();

        var tasks = Enumerable.Range(1, 25)
            .Select(i => Controller.Submit(CreateValidSubmitRequest(i)))
            .ToArray();
        var results = await Task.WhenAll(tasks);

        results.Should().HaveCount(25);
        results.Count(r => r.Result != null).Should().Be(25);
    }

    #endregion
}
