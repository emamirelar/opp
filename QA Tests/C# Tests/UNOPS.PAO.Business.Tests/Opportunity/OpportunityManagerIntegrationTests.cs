using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.Models;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Integration tests for UNOPSOpportunityManager
/// Uses real services with in-memory database (no mocks)
/// Tests end-to-end behavior as users would experience it
/// Created: January 16, 2026
/// SKIPPED: QA-009 - Z.EntityFramework.Extensions requires relational database (PostgreSQL)
/// </summary>
public class OpportunityManagerIntegrationTests : IntegrationTestBase
{
    private const string SkipReason = "QA-009: Z.EntityFramework.Extensions requires relational database";

    #region P0 - Create Opportunity Integration Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-CREATE-001")]
    public async Task CreateOpportunity_WithMinimalRequiredFields_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Integration Test Opportunity",
            Description = "Testing with real services and database",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1
        };

        // Act - Real manager call with real services
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);

        // Verify in database
        var savedOpportunity = await Context.Opportunities.FindAsync(result.Id);
        savedOpportunity.Should().NotBeNull();
        savedOpportunity!.Name.Should().Be(request.Name);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-CREATE-002")]
    public async Task CreateOpportunity_WithBudget_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Budgeted Initiative",
            Description = "Initiative with budget allocation",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1,
            InitiativeBudgetUSD = 2500000.00m
        };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(2500000.00m);

        var savedOpportunity = await Context.Opportunities.FindAsync(result.Id);
        savedOpportunity!.InitiativeBudgetUSD.Should().Be(2500000.00m);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-CREATE-003")]
    public async Task CreateOpportunity_WithTargetDates_Success()
    {
        // Arrange
        var targetSigningDate = DateTime.UtcNow.AddMonths(6);
        var targetDeliveryDate = DateTime.UtcNow.AddMonths(24);

        var request = new OpportunityRequest
        {
            Name = "Time-Bound Initiative",
            Description = "Initiative with target dates",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1,
            TargetSigningDate = targetSigningDate,
            TargetDeliveryDate = targetDeliveryDate
        };

        // Act
        var result = await Manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TargetSigningDate.Should().BeCloseTo(targetSigningDate, TimeSpan.FromSeconds(1));
        result.TargetDeliveryDate.Should().BeCloseTo(targetDeliveryDate, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region P0 - Read Opportunity Integration Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-READ-001")]
    public async Task GetOpportunity_WithValidId_ReturnsData()
    {
        // Arrange - Create an opportunity first
        var createRequest = new OpportunityRequest
        {
            Name = "Get Test Opportunity",
            Description = "Testing GET operation",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1
        };

        var created = await Manager.CreateOpportunityAsync(createRequest);

        // Act - Retrieve the opportunity
        var result = await Manager.GetOpportunityAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Name.Should().Be(createRequest.Name);
        result.Description.Should().Be(createRequest.Description);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P0")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-READ-002")]
    public async Task GetOpportunity_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await Manager.GetOpportunityAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region P1 - Update Opportunity Integration Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-UPDATE-001")]
    public async Task UpdateOpportunity_ChangeName_Success()
    {
        // Arrange - Create an opportunity
        var createRequest = new OpportunityRequest
        {
            Name = "Original Name",
            Description = "Original Description",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1
        };

        var created = await Manager.CreateOpportunityAsync(createRequest);

        // Act - Update the name
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = created.Id,
            Name = "Updated Name"
        };

        var result = await Manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Name.Should().Be("Updated Name");

        // Verify in database
        var savedOpportunity = await Context.Opportunities.FindAsync(created.Id);
        savedOpportunity!.Name.Should().Be("Updated Name");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-UPDATE-002")]
    public async Task UpdateOpportunity_ChangeBudget_Success()
    {
        // Arrange
        var createRequest = new OpportunityRequest
        {
            Name = "Budget Update Test",
            Description = "Testing budget updates",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1,
            InitiativeBudgetUSD = 1000000m
        };

        var created = await Manager.CreateOpportunityAsync(createRequest);

        // Act
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = created.Id,
            Name = "Budget Update Test",
            InitiativeBudgetUSD = 2000000m
        };

        var result = await Manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.InitiativeBudgetUSD.Should().Be(2000000m);

        var savedOpportunity = await Context.Opportunities.FindAsync(created.Id);
        savedOpportunity!.InitiativeBudgetUSD.Should().Be(2000000m);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-UPDATE-003")]
    public async Task UpdateOpportunity_ChangeWorkflowStage_Success()
    {
        // Arrange
        var createRequest = new OpportunityRequest
        {
            Name = "Workflow Test",
            Description = "Testing workflow progression",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1
            // WorkflowStageId property removed - stage managed by workflow system
        };

        var created = await Manager.CreateOpportunityAsync(createRequest);

        // Act - Workflow stage progression now handled by workflow service, not direct update
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = created.Id,
            Name = "Updated Name"
            // WorkflowStageId property removed - stage managed by workflow system
        };

        var result = await Manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Stage.Should().NotBeNullOrEmpty();

        var savedOpportunity = await Context.Opportunities.FindAsync(created.Id);
        savedOpportunity!.Stage.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region P1 - Delete Opportunity Integration Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-DELETE-001")]
    public async Task DeleteOpportunity_ValidId_Success()
    {
        // Arrange
        var createRequest = new OpportunityRequest
        {
            Name = "Delete Test",
            Description = "Will be deleted",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1
        };

        var created = await Manager.CreateOpportunityAsync(createRequest);

        // Act
        var result = await Manager.DeleteOpportunityAsync(created.Id);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete in database
        var deletedOpportunity = await Context.Opportunities.FindAsync(created.Id);
        deletedOpportunity.Should().NotBeNull();
        deletedOpportunity!.IsDeleted.Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-DELETE-002")]
    public async Task DeleteOpportunity_InvalidId_ReturnsFalse()
    {
        // Act
        var result = await Manager.DeleteOpportunityAsync(99999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region P1 - Complete Lifecycle Integration Test

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-LIFECYCLE-001")]
    public async Task OpportunityLifecycle_CreateReadUpdateDelete_Success()
    {
        // Step 1: Create
        var createRequest = new OpportunityRequest
        {
            Name = "Lifecycle Test Opportunity",
            Description = "Testing complete lifecycle",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1,
            InitiativeBudgetUSD = 1500000m
        };

        var created = await Manager.CreateOpportunityAsync(createRequest);
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);

        // Step 2: Read
        var retrieved = await Manager.GetOpportunityAsync(created.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(createRequest.Name);

        // Step 3: Update
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = created.Id,
            Name = "Updated Lifecycle Test",
            InitiativeBudgetUSD = 2500000m
        };

        var updated = await Manager.UpdateOpportunityAsync(updateRequest);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Lifecycle Test");
        updated.InitiativeBudgetUSD.Should().Be(2500000m);

        // Step 4: Delete
        var deleted = await Manager.DeleteOpportunityAsync(created.Id);
        deleted.Should().BeTrue();

        // Step 5: Verify soft delete
        var deletedOpportunity = await Context.Opportunities.FindAsync(created.Id);
        deletedOpportunity.Should().NotBeNull();
        deletedOpportunity!.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region P2 - List Operations Integration Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-LIST-001")]
    public async Task GetAllOpportunities_ReturnsMultiple()
    {
        // Arrange - Create multiple opportunities
        for (int i = 1; i <= 5; i++)
        {
            var request = new OpportunityRequest
            {
                Name = $"List Test Opportunity {i}",
                Description = $"Description {i}",
                ResponsibleOrgUnitId = 1,
                ProposedInitiativeTypeId = 1
            };

            await Manager.CreateOpportunityAsync(request);
        }

        // Act
        var opportunities = Context.Opportunities.Where(o => !o.IsDeleted).ToList();

        // Assert
        opportunities.Should().HaveCountGreaterThanOrEqualTo(5);
        opportunities.Should().OnlyContain(o => !o.IsDeleted);
    }

    #endregion
}
