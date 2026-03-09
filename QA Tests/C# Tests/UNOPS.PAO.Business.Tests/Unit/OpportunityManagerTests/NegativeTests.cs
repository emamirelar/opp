using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Unit.OpportunityManagerTests;

/// <summary>
/// Negative tests for base OpportunityManager.
/// Invalid inputs, unauthorized states, expected failures, immutability violations.
/// </summary>
public class NegativeTests : OpportunityManagerTestFixtureBase
{
    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-058")]
    public async Task GetOpportunityAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await Manager.GetOpportunityAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-058")]
    public async Task GetOpportunityAsync_ZeroId_ReturnsNull()
    {
        // Act
        var result = await Manager.GetOpportunityAsync(0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-058")]
    public async Task GetOpportunityAsync_NegativeId_ReturnsNull()
    {
        // Act
        var result = await Manager.GetOpportunityAsync(-1);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    [Trait("Defect", "DEF-058")]
    public async Task GetOpportunityAsync_SoftDeletedId_ReturnsNull()
    {
        // Arrange - soft-deleted opportunities are filtered by repository
        var id = await SeedSoftDeletedOpportunityAsync();

        // Act
        var result = await Manager.GetOpportunityAsync(id);

        // Assert - DataRepository filters !IsDeleted, so soft-deleted returns null
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateOpportunityAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var request = new UpdateOpportunityRequest { Id = 99999, Name = "X", Description = "Y" };

        // Act
        var result = await Manager.UpdateOpportunityAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateOpportunityAsync_GOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("GO");
        var request = new UpdateOpportunityRequest { Id = id, Name = "Updated", Description = "Desc" };

        // Act
        var act = () => Manager.UpdateOpportunityAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateOpportunityAsync_NOGOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("NO GO");
        var request = new UpdateOpportunityRequest { Id = id, Name = "Updated", Description = "Desc" };

        // Act
        var act = () => Manager.UpdateOpportunityAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateOpportunityAsync_CANCELLEDStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("CANCELLED");
        var request = new UpdateOpportunityRequest { Id = id, Name = "Updated", Description = "Desc" };

        // Act
        var act = () => Manager.UpdateOpportunityAsync(request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*locked*cannot be modified*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateOverviewSectionAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new OverviewSectionRequest { Name = "X", Description = "Y" };

        // Act
        var act = () => Manager.UpdateOverviewSectionAsync(99999, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99999*not found*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateOverviewSectionAsync_GOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("GO");
        var request = new OverviewSectionRequest { Name = "X", Description = "Y" };

        // Act
        var act = () => Manager.UpdateOverviewSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhatSectionAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new WhatSectionRequest { Description = "X" };

        // Act
        var act = () => Manager.UpdateWhatSectionAsync(99999, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhatSectionAsync_GOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("GO");
        var request = new WhatSectionRequest { Description = "X" };

        // Act
        var act = () => Manager.UpdateWhatSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhySectionAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new WhySectionRequest { ExpectedImpact = "X" };

        // Act
        var act = () => Manager.UpdateWhySectionAsync(99999, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhySectionAsync_GOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("GO");
        var request = new WhySectionRequest { ExpectedImpact = "X" };

        // Act
        var act = () => Manager.UpdateWhySectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhoSectionAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new WhoSectionRequest();

        // Act
        var act = () => Manager.UpdateWhoSectionAsync(99999, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhoSectionAsync_GOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("GO");
        var request = new WhoSectionRequest { FundingPartners = new List<OpportunityFundingPartnerRequest>() };

        // Act
        var act = () => Manager.UpdateWhoSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhereSectionAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() };

        // Act
        var act = () => Manager.UpdateWhereSectionAsync(99999, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhereSectionAsync_GOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("GO");
        var request = new WhereSectionRequest { Countries = new List<OpportunityCountryRequest>() };

        // Act
        var act = () => Manager.UpdateWhereSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhenSectionAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new WhenSectionRequest { TargetSigningDate = DateTime.UtcNow };

        // Act
        var act = () => Manager.UpdateWhenSectionAsync(99999, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhenSectionAsync_ImplementationBeforeSigning_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new WhenSectionRequest
        {
            TargetSigningDate = new DateTime(2026, 7, 1),
            ImplementationStartDate = new DateTime(2026, 6, 1) // Before signing
        };

        // Act
        var act = () => Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Implementation Start Date*cannot be before*Target Signing*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhenSectionAsync_DeliveryBeforeImplementation_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var request = new WhenSectionRequest
        {
            ImplementationStartDate = new DateTime(2026, 7, 1),
            TargetDeliveryDate = new DateTime(2026, 6, 1) // Before implementation
        };

        // Act
        var act = () => Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Target Delivery Date*must be after*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhenSectionAsync_GOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("GO");
        var request = new WhenSectionRequest { TargetSigningDate = DateTime.UtcNow };

        // Act
        var act = () => Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetRelatedItemsAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Act
        var act = () => Manager.GetRelatedItemsAsync(99999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task DeleteOpportunityAsync_NonExistentId_ReturnsFalse()
    {
        // Act
        var result = await Manager.DeleteOpportunityAsync(99999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task DeleteOpportunityAsync_GOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("GO");

        // Act
        var act = () => Manager.DeleteOpportunityAsync(id);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task DeleteOpportunityAsync_NOGOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("NO GO");

        // Act
        var act = () => Manager.DeleteOpportunityAsync(id);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task AssignExecutiveAsync_NonExistentOpportunity_ThrowsKeyNotFoundException()
    {
        // Act
        var act = () => Manager.AssignExecutiveAsync(99999, 1);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*99999*not found*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetExecutivesForOpportunityAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Act
        var act = () => Manager.GetExecutivesForOpportunityAsync(99999);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunitiesByPartnerIdAsync_ThrowsNotImplementedException()
    {
        // Act
        var act = () => Manager.GetOpportunitiesByPartnerIdAsync(1);

        // Assert
        act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*UNOPS*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void GetOpportunitySearchFields_ThrowsNotImplementedException()
    {
        // Act
        var act = () => Manager.GetOpportunitySearchFields();

        // Assert
        act.Should().Throw<NotImplementedException>()
            .WithMessage("*UNOPS*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UpdateHighRiskAcknowledgementAsync_ThrowsNotImplementedException()
    {
        // Act
        var act = () => Manager.UpdateHighRiskAcknowledgementAsync(1, true);

        // Assert
        act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*UNOPS*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void CreateOpportunityFromProposalAsync_ThrowsNotImplementedException()
    {
        // Arrange
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            SourceInteractionIds = new List<int> { 1 }
        };

        // Act
        var act = () => Manager.CreateOpportunityFromProposalAsync(request, 1);

        // Assert
        act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*UNOPS*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task GetAllOpportunitiesAsync_ExcludesSoftDeleted()
    {
        // Arrange
        var activeId = await SeedOpportunityAsync("IDENTIFY & PROFILE", false);
        var deletedId = await SeedSoftDeletedOpportunityAsync();

        // Act
        var result = (await Manager.GetAllOpportunitiesAsync()).ToList();

        // Assert - GetAll uses .Where(o => !o.IsDeleted)
        result.Select(o => o.Id).Should().Contain(activeId);
        result.Select(o => o.Id).Should().NotContain(deletedId);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateTeamSectionAsync_GOStage_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedImmutableOpportunityAsync("GO");
        var request = new TeamSectionRequest { ResponsibleOrgUnitId = 1 };

        // Act
        var act = () => Manager.UpdateTeamSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateTeamSectionAsync_NonExistentId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new TeamSectionRequest();

        // Act
        var act = () => Manager.UpdateTeamSectionAsync(99999, request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhenSectionAsync_DeliverableStartBeforeImplementation_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var outputId = await SeedOutputAsync();
        var opp = await Context.Opportunities.FindAsync(id);
        var deliverable = new OpportunityDeliverable
        {
            OpportunityId = id,
            OutputId = outputId,
            Quantity = 1,
            Name = "D1"
        };
        Context.Set<OpportunityDeliverable>().Add(deliverable);
        await Context.SaveChangesAsync();

        var request = new WhenSectionRequest
        {
            ImplementationStartDate = new DateTime(2026, 7, 1),
            Deliverables = new List<DeliverableDateUpdate>
            {
                new() { Id = deliverable.Id, PlannedStartDate = new DateTime(2026, 6, 1) } // Before impl start
            }
        };

        // Act
        var act = () => Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Planned Start Date*cannot be before*");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task UpdateWhenSectionAsync_DeliverableEndBeforeStart_ThrowsBusinessException()
    {
        // Arrange
        var id = await SeedOpportunityAsync();
        var outputId = await SeedOutputAsync();
        var deliverable = new OpportunityDeliverable
        {
            OpportunityId = id,
            OutputId = outputId,
            Quantity = 1,
            Name = "D1"
        };
        Context.Set<OpportunityDeliverable>().Add(deliverable);
        await Context.SaveChangesAsync();

        var request = new WhenSectionRequest
        {
            ImplementationStartDate = new DateTime(2026, 6, 1),
            Deliverables = new List<DeliverableDateUpdate>
            {
                new()
                {
                    Id = deliverable.Id,
                    PlannedStartDate = new DateTime(2026, 7, 1),
                    PlannedEndDate = new DateTime(2026, 6, 15) // Before start
                }
            }
        };

        // Act
        var act = () => Manager.UpdateWhenSectionAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("*Planned End Date*cannot be before*Planned Start*");
    }
}
