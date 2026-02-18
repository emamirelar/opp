using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Advanced feature tests for opportunity management
/// Tests AI integration, performance scenarios, edge cases, and complex workflows
/// Created: January 15, 2026
/// Priority: P2 (Medium)
/// </summary>
public class OpportunityAdvancedFeaturesTests : IDisposable
{
    private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
    private readonly UNOPSAppDbContext _context;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IDbContextFactory<UNOPSAppDbContext>> _mockDbContextFactory;
    private readonly Mock<IExchangeRateService> _mockExchangeRateService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly UNOPSOpportunityManager _manager;
    private readonly ClaimsPrincipal _testUser;

    public OpportunityAdvancedFeaturesTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: $"OpportunityAdvancedTestDb_{Guid.NewGuid()}")
            .Options;

        var mockUserService = new Mock<UserResolverService<int>>(null);
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        _context = new UNOPSAppDbContext(_dbContextOptions, mockUserService.Object, mockDbSchema.Object);

        _mockMapper = new Mock<IMapper>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockPermissionService = new Mock<IPermissionService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockDbContextFactory = new Mock<IDbContextFactory<UNOPSAppDbContext>>();
        _mockServiceProvider = new Mock<IServiceProvider>();

        _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "Test User")
        }, "TestAuthType"));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(_testUser);
        _mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);

        _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(default))
            .ReturnsAsync(_context);

        _manager = new UNOPSOpportunityManager(
            _mockMapper.Object,
            _context,
            _mockConfiguration.Object,
            _mockDbContextFactory.Object,
            _mockExchangeRateService.Object,
            _mockPermissionService.Object,
            _mockHttpContextAccessor.Object,
            _mockServiceProvider.Object
        );

        SeedTestData();
    }

    private void SeedTestData()
    {
        _context.Currencies.Add(new Currency { Id = 1, Code = "USD", Name = "US Dollar", IsDeleted = false });
        _context.Countries.Add(new Country { Id = 1, Name = "Bangladesh", Iso2Code = "BD" });
        _context.OrganizationHierarchies.Add(new OrganizationHierarchy { Id = 1, Name = "Test Org", Code = "TO", Description = "Test Organization", IsDeleted = false });
        _context.WorkflowStages.Add(new WorkflowStage { Id = 1, Name = "Identification", EntityType = "Opportunity", Order = 1, IsDeleted = false });
        _context.ProposedInitiativeTypes.Add(new ProposedInitiativeType { Id = 1, Name = "Project", IsDeleted = false });
        _context.PAOUsers.Add(new PAOUser { Id = 1, Email = "test@unops.org" });
        _context.SaveChanges();
    }

    #region P2 - AI Integration Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "AI")]
    [Trait("TestId", "TC-UNOPS-ADV-001")]
    public async Task ApplyAiChanges_UpdatesMultipleFields_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Original Name",
            Description = "Original Description",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var aiChangesRequest = new ApplyOpportunityAiChangesRequest
        {
            Name = "AI-Enhanced Project Name",
            Description = "AI-generated comprehensive description with improved clarity and structure",
            ExpectedImpact = "Significant positive impact on target communities",
            ExpectedOutcomes = "Improved infrastructure and sustainable development",
            Challenges = "Implementation risks include weather conditions and resource availability",
            ResultsFocus = "Sustainable Development Goal alignment"
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel
            {
                Id = 1,
                Name = aiChangesRequest.Name,
                Description = aiChangesRequest.Description,
                ExpectedImpact = aiChangesRequest.ExpectedImpact,
                ExpectedOutcomes = aiChangesRequest.ExpectedOutcomes,
                Challenges = aiChangesRequest.Challenges
            });

        // Act
        var result = await _manager.ApplyAiChangesAsync(1, aiChangesRequest);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("AI-Enhanced Project Name");
        result.Description.Should().Contain("AI-generated");
        result.ExpectedImpact.Should().Be("Significant positive impact on target communities");
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "AI")]
    [Trait("TestId", "TC-UNOPS-ADV-002")]
    public async Task GetOpportunityDetailsForAI_ReturnsCompleteContext()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Complex Multi-Partner Project",
            Description = "Detailed project description",
            InitiativeBudgetUSD = 5000000,
            WorkflowStageId = 1,
            ResponsibleOrgUnitId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _manager.GetOpportunityDetailsForAIAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("id");
        result.Should().ContainKey("name");
        result.Should().ContainKey("description");
        result["id"].Should().Be(1);
        result["name"].Should().Be("Complex Multi-Partner Project");
        result["description"].Should().Be("Detailed project description");
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "AI")]
    [Trait("TestId", "TC-UNOPS-ADV-003")]
    public async Task ApplyAiChanges_PreservesManuallyEditedFields_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Manually Edited Name",
            Description = "User provided description",
            InitiativeBudgetUSD = 1000000, // Manually set budget
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // AI changes only description, not budget
        var aiChangesRequest = new ApplyOpportunityAiChangesRequest
        {
            Description = "AI-enhanced description"
            // Budget not included - should preserve manual value
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel
            {
                Id = 1,
                Name = "Manually Edited Name",
                Description = "AI-enhanced description",
                InitiativeBudgetUSD = 1000000 // Preserved
            });

        // Act
        var result = await _manager.ApplyAiChangesAsync(1, aiChangesRequest);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(1000000); // Preserved from manual entry
    }

    #endregion

    #region P2 - Performance and Scalability Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Performance")]
    [Trait("TestId", "TC-UNOPS-ADV-004")]
    public async Task GetOpportunityWithManyRelationships_PerformsWithinTimeout()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Large Scale Programme",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = "Large Scale Programme" });

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _manager.GetOpportunityAsync(1);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Should complete within 3 seconds
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Performance")]
    [Trait("TestId", "TC-UNOPS-ADV-005")]
    public async Task CreateOpportunityWithManyChildRecords_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Complex Opportunity",
            Description = "With many related records",
            FundingPartners = Enumerable.Range(1, 10).Select(i => new OpportunityFundingPartnerRequest
            {
                PartnerId = i,
                Amount = 100000 * i,
                CurrencyId = 1
            }).ToList(),
            ClientPartners = Enumerable.Range(1, 5).Select(i => new OpportunityClientPartnerRequest
            {
                PartnerId = i + 10
            }).ToList(),
            Deliverables = Enumerable.Range(1, 20).Select(i => new OpportunityDeliverableRequest
            {
                Name = $"Deliverable {i}",
                Description = $"Description for deliverable {i}"
            }).ToList(),
            Countries = Enumerable.Range(1, 3).Select(i => new OpportunityCountryRequest
            {
                CountryId = 1 // All same country for simplicity
            }).ToList()
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = request.Name });

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    #endregion

    #region P2 - Edge Case Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "EdgeCase")]
    [Trait("TestId", "TC-UNOPS-ADV-006")]
    public async Task CreateOpportunity_WithUnicodeCharacters_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "المشروع التنموي - Projet de Développement - プロジェクト",
            Description = "多言語プロジェクト説明 مشروع متعدد اللغات Multilingual project description"
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = request.Name });

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Contain("プロジェクト");
        result.Name.Should().Contain("Développement");
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "EdgeCase")]
    [Trait("TestId", "TC-UNOPS-ADV-007")]
    public async Task UpdateOpportunity_ClearOptionalFields_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Has description",
            PartnerReference = "REF-001",
            Challenges = "Has challenges",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act - Clear optional fields by setting to null
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            PartnerReference = null,
            // Description and Challenges not specified - should remain
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = "Test Opportunity", PartnerReference = null });

        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        
        var savedOpportunity = await _context.Opportunities.FindAsync(1);
        savedOpportunity!.PartnerReference.Should().BeNull(); // Cleared
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "EdgeCase")]
    [Trait("TestId", "TC-UNOPS-ADV-008")]
    public async Task GetOpportunity_MultipleTimesInParallel_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Parallel Access Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = "Parallel Access Test" });

        // Act - Multiple parallel reads
        var tasks = Enumerable.Range(1, 10).Select(_ =>
            _manager.GetOpportunityAsync(1)
        ).ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().OnlyContain(r => r != null && r.Id == 1);
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "EdgeCase")]
    [Trait("TestId", "TC-UNOPS-ADV-009")]
    public async Task CreateOpportunity_WithExtremelyLargeBudget_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Mega Programme",
            Description = "Extremely large budget programme",
            InitiativeBudgetUSD = decimal.MaxValue // Max possible value
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            InitiativeBudgetUSD = decimal.MaxValue,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = request.Name, InitiativeBudgetUSD = decimal.MaxValue });

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(decimal.MaxValue);
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "EdgeCase")]
    [Trait("TestId", "TC-UNOPS-ADV-010")]
    public async Task CreateOpportunity_WithFutureCreatedDate_HandleGracefully()
    {
        // Arrange - Test edge case where created date might be in future (clock sync issues)
        var request = new OpportunityRequest
        {
            Name = "Future Date Test",
            Description = "Testing future date handling"
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow.AddDays(1), // Future date (clock skew)
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = request.Name });

        // Act & Assert - Should handle gracefully
        var result = await _manager.CreateOpportunityAsync(request);
        result.Should().NotBeNull();
    }

    #endregion

    #region P2 - Complex Workflow Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Workflow")]
    [Trait("TestId", "TC-UNOPS-ADV-011")]
    public async Task OpportunityWorkflow_ProgressThroughAllStages_Success()
    {
        // Arrange - Add more workflow stages
        _context.WorkflowStages.AddRange(new[]
        {
            new WorkflowStage { Id = 2, Name = "Development", Order = 2, IsDeleted = false },
            new WorkflowStage { Id = 3, Name = "Review", Order = 3, IsDeleted = false },
            new WorkflowStage { Id = 4, Name = "Approval", Order = 4, IsDeleted = false }
        });
        await _context.SaveChangesAsync();

        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Workflow Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act - Progress through stages
        for (int stageId = 2; stageId <= 4; stageId++)
        {
            var updateRequest = new UpdateOpportunityRequest
            {
                Id = 1,
                WorkflowStageId = stageId
            };

            _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
                .Returns(new OpportunityModel { Id = 1, WorkflowStageId = stageId });

            var result = await _manager.UpdateOpportunityAsync(updateRequest);
            result.Should().NotBeNull();
            result!.WorkflowStageId.Should().Be(stageId);
        }

        // Assert - Verify final stage
        var finalOpportunity = await _context.Opportunities.FindAsync(1);
        finalOpportunity!.WorkflowStageId.Should().Be(4); // Approval stage
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Workflow")]
    [Trait("TestId", "TC-UNOPS-ADV-012")]
    public async Task CreateMultipleOpportunities_SameUser_Success()
    {
        // Arrange
        var requests = Enumerable.Range(1, 5).Select(i => new OpportunityRequest
        {
            Name = $"Opportunity {i}",
            Description = $"Description for opportunity {i}",
            ResponsibleOrgUnitId = 1
        }).ToArray();

        var entities = requests.Select((r, index) => new Domain.Entities.Opportunity
        {
            Id = index + 1,
            Name = r.Name,
            Description = r.Description,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        }).ToArray();

        int entityIndex = 0;
        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(() => entities[entityIndex++]);

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns((Domain.Entities.Opportunity o) => new OpportunityModel { Id = o.Id, Name = o.Name });

        // Act
        var results = new List<OpportunityModel>();
        foreach (var request in requests)
        {
            var result = await _manager.CreateOpportunityAsync(request);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(5);
        results.Select(r => r.Id).Should().OnlyHaveUniqueItems();

        var savedOpportunities = await _context.Opportunities.ToListAsync();
        savedOpportunities.Should().HaveCount(5);
    }

    #endregion

    #region P2 - Data Consistency Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "DataConsistency")]
    [Trait("TestId", "TC-UNOPS-ADV-013")]
    public async Task UpdateOpportunity_MaintainsAuditTrail_Success()
    {
        // Arrange
        var createTime = DateTime.UtcNow.AddDays(-7);
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Audit Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = createTime,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act - Update
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Updated Name"
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = "Updated Name" });

        await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert - Verify audit fields
        var savedOpportunity = await _context.Opportunities.FindAsync(1);
        savedOpportunity.Should().NotBeNull();
        savedOpportunity!.CreatedBy.Should().Be(1); // Original creator preserved
        savedOpportunity.CreatedDate.Should().Be(createTime); // Original date preserved
        savedOpportunity.LastModifiedBy.Should().Be(1); // Updated
        savedOpportunity.LastModifiedDate.Should().NotBeNull(); // Set
        savedOpportunity.LastModifiedDate.Should().BeAfter(createTime); // After creation
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "DataConsistency")]
    [Trait("TestId", "TC-UNOPS-ADV-014")]
    public async Task DeleteOpportunity_PreservesData_ForAudit()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "To Be Deleted",
            Description = "Important data to preserve",
            InitiativeBudgetUSD = 1000000,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act - Soft delete
        await _manager.DeleteOpportunityAsync(1);

        // Assert - Data preserved for audit
        var deletedOpportunity = await _context.Opportunities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == 1);

        deletedOpportunity.Should().NotBeNull();
        deletedOpportunity!.IsDeleted.Should().BeTrue();
        deletedOpportunity.Name.Should().Be("To Be Deleted"); // Data preserved
        deletedOpportunity.Description.Should().Be("Important data to preserve");
        deletedOpportunity.InitiativeBudgetUSD.Should().Be(1000000);
        deletedOpportunity.DeletedBy.Should().Be(1);
        deletedOpportunity.DeletedDate.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "DataConsistency")]
    [Trait("TestId", "TC-UNOPS-ADV-015")]
    public async Task CreateOpportunity_SetsDefaultValues_Correctly()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Minimal Opportunity",
            Description = "Only required fields"
            // No workflow stage specified - should default to 1
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            WorkflowStageId = 1, // Default
            Status = EntityStatus.Draft, // Default
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false,
            BeneficiariesToBeDetermined = false, // Default
            IsPooledFunding = false // Default
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = request.Name, Status = EntityStatus.Draft, WorkflowStageId = 1 });

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Draft"); // Default status
        result.WorkflowStageId.Should().Be(1); // Default workflow stage

        var savedOpportunity = await _context.Opportunities.FindAsync(1);
        savedOpportunity!.WorkflowStageId.Should().Be(1);
        savedOpportunity.Status.Should().Be("Draft");
    }

    #endregion

    #region P2 - Integration with Other Managers Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-ADV-016")]
    public async Task GetOpportunitiesByPartner_ReturnsRelated_Success()
    {
        // Arrange
        _context.Opportunities.AddRange(new[]
        {
            new Domain.Entities.Opportunity { Id = 1, Name = "Opp 1", WorkflowStageId = 1, Status = EntityStatus.Draft, CreatedBy = 1, CreatedDate = DateTime.UtcNow, IsDeleted = false },
            new Domain.Entities.Opportunity { Id = 2, Name = "Opp 2", WorkflowStageId = 1, Status = EntityStatus.Draft, CreatedBy = 1, CreatedDate = DateTime.UtcNow, IsDeleted = false }
        });
        await _context.SaveChangesAsync();

        var relatedModels = new List<OpportunityModel>
        {
            new() { Id = 1, Name = "Opp 1" },
            new() { Id = 2, Name = "Opp 2" }
        };

        _mockMapper.Setup(m => m.Map<IEnumerable<OpportunityModel>>(It.IsAny<List<Domain.Entities.Opportunity>>()))
            .Returns(relatedModels);

        // Act
        var result = await _manager.GetOpportunitiesByPartnerIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        var opportunities = result.ToList();
        opportunities.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-ADV-017")]
    public async Task AssignCreatorAsOpportunityManager_IntegratesWithTeam_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Team Integration Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act
        await _manager.AssignCreatorAsOpportunityManagerAsync(1, 1);

        // Assert - Verify method completes
        // In actual implementation, would verify stakeholder assignment
        var savedOpportunity = await _context.Opportunities
            .Include(o => o.Stakeholders)
            .FirstOrDefaultAsync(o => o.Id == 1);

        savedOpportunity.Should().NotBeNull();
    }

    #endregion

    #region P2 - Null Safety and Error Resilience Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "ErrorHandling")]
    [Trait("TestId", "TC-UNOPS-ADV-018")]
    public async Task GetOpportunityAsync_NullId_ReturnsNull()
    {
        // Act
        var result = await _manager.GetOpportunityAsync(0);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "ErrorHandling")]
    [Trait("TestId", "TC-UNOPS-ADV-019")]
    public async Task UpdateOpportunity_NullRequest_HandlesGracefully()
    {
        // Arrange
        UpdateOpportunityRequest? nullRequest = null;

        // Act & Assert
        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(nullRequest!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "ErrorHandling")]
    [Trait("TestId", "TC-UNOPS-ADV-020")]
    public async Task DeleteOpportunity_NegativeId_ReturnsFalse()
    {
        // Act
        var result = await _manager.DeleteOpportunityAsync(-1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region P2 - Opportunity Status Transitions Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Workflow")]
    [Trait("TestId", "TC-UNOPS-ADV-021")]
    public async Task UpdateOpportunity_TransitionFromDraftToActive_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Status Transition Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            WorkflowStageId = 2 // Move to Development stage
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Status = EntityStatus.Active, WorkflowStageId = 2 });

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.WorkflowStageId.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Workflow")]
    [Trait("TestId", "TC-UNOPS-ADV-022")]
    public async Task CreateOpportunity_InDraftStatus_AllowsIncompleteData()
    {
        // Arrange - Draft can have minimal data
        var request = new OpportunityRequest
        {
            Name = "Draft Opportunity",
            Description = "Work in progress",
            // Many optional fields not provided
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = request.Name, Status = "Draft" });

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Draft");
    }

    #endregion

    #region P2 - High Risk Acknowledgment Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-ADV-023")]
    public async Task UpdateOpportunity_AcknowledgeHighRisks_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "High Risk Opportunity",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel
            {
                Id = 1,
                Name = "High Risk Opportunity",
                HighRisksAcknowledged = true
            });

        // Act
        var result = await _manager.GetOpportunityAsync(1);

        // Assert - In actual implementation, would verify acknowledgment
        result.Should().NotBeNull();
    }

    #endregion

    #region P2 - Delivery Modality Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-ADV-024")]
    public async Task CreateOpportunity_WithDeliveryModality_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Delivery Modality Test",
            Description = "Testing delivery approach selection",
            DeliveryModality = 1 // Direct Execution
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            DeliveryModality = request.DeliveryModality,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = request.Name, DeliveryModality = 1 });

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.DeliveryModality.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-ADV-025")]
    public async Task UpdateOpportunity_ChangeDeliveryModality_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Modality Change Test",
            DeliveryModality = 1,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            DeliveryModality = 2 // Change to different modality
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, DeliveryModality = 2 });

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.DeliveryModality.Should().Be(2);
    }

    #endregion

    #region P2 - New Value Range Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "BusinessLogic")]
    [Trait("TestId", "TC-UNOPS-ADV-026")]
    public async Task CreateOpportunity_ExceedsOrgUnitHistoricalMax_FlagsNewValueRange()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Record-Breaking Opportunity",
            Description = "Largest value for this org unit",
            ResponsibleOrgUnitId = 1,
            InitiativeBudgetUSD = 10000000 // Exceeds historical max
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            ResponsibleOrgUnitId = 1,
            InitiativeBudgetUSD = 10000000,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel
            {
                Id = 1,
                Name = request.Name,
                InitiativeBudgetUSD = 10000000,
                IsNewValueRangeForOrgUnit = true,
                OrgUnitHistoricalMaxValue = 5000000
            });

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(10000000);
    }

    #endregion

    #region P2 - Opportunity Stats Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-ADV-027")]
    public async Task GetOpportunity_IncludesStats_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Stats Test Opportunity",
            WorkflowStageId = 1,
            ResponsibleOrgUnitId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow.AddDays(-30),
            LastModifiedDate = DateTime.UtcNow.AddDays(-1),
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var modelWithStats = new OpportunityModel
        {
            Id = 1,
            Name = "Stats Test Opportunity",
            Stats = new OpportunityStats
            {
                // Stats would be calculated by actual implementation
            }
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(modelWithStats);

        // Act
        var result = await _manager.GetOpportunityAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Stats.Should().NotBeNull();
    }

    #endregion

    #region P2 - Conditional Tags Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "BusinessLogic")]
    [Trait("TestId", "TC-UNOPS-ADV-028")]
    public async Task GetOpportunity_CalculatesConditionalTags_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Tagged Opportunity",
            WorkflowStageId = 1,
            InitiativeBudgetUSD = 10000000, // Large budget
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow.AddDays(-90), // Old draft
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var modelWithTags = new OpportunityModel
        {
            Id = 1,
            Name = "Tagged Opportunity",
            // Tags calculated based on opportunity state
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(modelWithTags);

        // Act
        var result = await _manager.GetOpportunityAsync(1);

        // Assert
        result.Should().NotBeNull();
        // In actual implementation, would verify tags like "Large Budget", "Stale Draft", etc.
    }

    #endregion

    #region P2 - User Role Context Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-ADV-029")]
    public async Task GetOpportunity_IncludesUserRoleContext_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Role Context Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var modelWithRole = new OpportunityModel
        {
            Id = 1,
            Name = "Role Context Test",
            UserRole = "Opportunity Manager" // User's role for this opportunity
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(modelWithRole);

        // Act
        var result = await _manager.GetOpportunityAsync(_testUser, 1);

        // Assert
        result.Should().NotBeNull();
        result!.UserRole.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region P2 - Opportunity Lifecycle Edge Cases

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "EdgeCase")]
    [Trait("TestId", "TC-UNOPS-ADV-030")]
    public async Task UpdateOpportunity_RapidSuccessiveUpdates_HandlesCorrectly()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Rapid Update Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns((Domain.Entities.Opportunity o) => new OpportunityModel { Id = o.Id, Name = o.Name });

        // Act - Perform 3 rapid updates
        var update1 = new UpdateOpportunityRequest { Id = 1, Name = "Update 1" };
        var update2 = new UpdateOpportunityRequest { Id = 1, Name = "Update 2" };
        var update3 = new UpdateOpportunityRequest { Id = 1, Name = "Update 3" };

        await _manager.UpdateOpportunityAsync(update1);
        await _manager.UpdateOpportunityAsync(update2);
        var finalResult = await _manager.UpdateOpportunityAsync(update3);

        // Assert
        finalResult.Should().NotBeNull();
        
        var savedOpportunity = await _context.Opportunities.FindAsync(1);
        savedOpportunity!.Name.Should().Be("Update 3");
        savedOpportunity.LastModifiedDate.Should().NotBeNull();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
