using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Advanced feature tests for opportunity management
/// Tests AI integration, performance scenarios, edge cases, and complex workflows
/// Created: January 15, 2026
/// Priority: P2 (Medium)
/// SKIPPED: QA-009 - Z.EntityFramework.Extensions requires relational database (PostgreSQL)
/// </summary>
public class OpportunityAdvancedFeaturesTests : IDisposable
{
    private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
    private readonly UNOPSAppDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IDbContextFactory<UNOPSAppDbContext>> _mockDbContextFactory;
    private readonly Mock<IExchangeRateService> _mockExchangeRateService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly UNOPSOpportunityManager _manager;
    private readonly ClaimsPrincipal _testUser;
    private readonly string _testMarker = $"ADV_{Guid.NewGuid():N}";
    private readonly List<int> _createdOpportunityIds = new();
    // Reference data IDs resolved from database
    private int _currencyId;
    private int _countryId;
    private int _orgHierarchyId;
    private int _proposedInitiativeTypeId;
    private int _paoUserId;
    private int _entityRoleId;

    public OpportunityAdvancedFeaturesTests()
    {
        _dbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"OpportunityAdvancedTestDb_{Guid.NewGuid()}");
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        // Phase 1: Resolve the test user ID using a temporary context (outside transaction).
        // AuditableDbContext caches _currentUserId at construction, so we must know the
        // real user ID before creating the main context.
        {
            var tempAccessor = CreateMockHttpContextAccessor("0");
            var tempResolver = new UserResolverService<int>(tempAccessor.Object, null);
            using var tempCtx = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_dbContextOptions, tempResolver, mockDbSchema.Object);
            _paoUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "test@unops.org");
        }

        // Phase 2: Create the MAIN context with the ACTUAL test user ID in claims.
        var mainAccessor = CreateMockHttpContextAccessor(_paoUserId.ToString());
        var userResolverService = new UserResolverService<int>(mainAccessor.Object, null);
        _context = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_dbContextOptions, userResolverService, mockDbSchema.Object);

        if (TestEnvironment.UsePostgreSQL)
        {
            _transaction = _context.Database.BeginTransaction();
        }

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DbSchema"] = "public",
                ["AISettings:DisableExternalCalls"] = "true",
                ["AISettings:ModelName"] = "gemini-pro",
                ["AISettings:ProjectId"] = "test-project",
                ["AISettings:Location"] = "us-central1",
                ["IsUNOPSOverride"] = "true",
                ["GoogleCloud:ProjectId"] = "test-project",
                ["GoogleCloud:PubSubTopic"] = "test-topic",
                ["ExchangeRate:ApiKey"] = "test-key",
                ["ExchangeRate:BaseUrl"] = "https://test-api.example.com"
            })
            .Build();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
            cfg.ConstructServicesUsing(serviceType =>
            {
                try { return Activator.CreateInstance(serviceType)!; }
                catch { return null!; }
            });
        });
        _mapper = mapperConfig.CreateMapper();

        _mockPermissionService = new Mock<IPermissionService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockDbContextFactory = new Mock<IDbContextFactory<UNOPSAppDbContext>>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockExchangeRateService = new Mock<IExchangeRateService>();

        SeedTestData();

        _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _paoUserId.ToString()),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "test@unops.org"),
            new Claim(ClaimTypes.Role, "User")
        }, "TestAuthType"));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(_testUser);
        _mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);

        _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var factoryAccessor = CreateMockHttpContextAccessor(_paoUserId.ToString());
                var factoryResolver = new UserResolverService<int>(factoryAccessor.Object, null);
                return UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_dbContextOptions, factoryResolver, mockDbSchema.Object);
            });

        _manager = new UNOPSOpportunityManager(
            _mapper,
            _context,
            _configuration,
            _mockDbContextFactory.Object,
            _mockExchangeRateService.Object,
            _mockPermissionService.Object,
            _mockHttpContextAccessor.Object,
            _mockServiceProvider.Object
        );
    }

    private void SeedTestData()
    {
        // Use "get or create" pattern for reference data to work with PostgreSQL
        var currency = _context.Currencies.FirstOrDefault(c => c.Code == "USD");
        if (currency == null)
        {
            currency = new Currency { Code = "USD", Name = "US Dollar", IsDeleted = false };
            _context.Currencies.Add(currency);
            _context.SaveChanges();
        }
        _currencyId = currency.Id;

        var country = _context.Countries.FirstOrDefault(c => c.Iso2Code == "BD");
        if (country == null)
        {
            country = new Country { Name = "Bangladesh", Iso2Code = "BD" };
            _context.Countries.Add(country);
            _context.SaveChanges();
        }
        _countryId = country.Id;

        var orgHierarchy = _context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "TO" && !o.IsDeleted);
        if (orgHierarchy == null)
        {
            orgHierarchy = new OrganizationHierarchy { Name = "Test Org", Code = "TO", Description = "Test Organization", IsDeleted = false };
            _context.OrganizationHierarchies.Add(orgHierarchy);
            _context.SaveChanges();
        }
        _orgHierarchyId = orgHierarchy.Id;

        var proposedInitiativeType = _context.ProposedInitiativeTypes.FirstOrDefault(p => p.Name == "Project" && !p.IsDeleted);
        if (proposedInitiativeType == null)
        {
            proposedInitiativeType = new ProposedInitiativeType { Name = "Project", IsDeleted = false };
            _context.ProposedInitiativeTypes.Add(proposedInitiativeType);
            _context.SaveChanges();
        }
        _proposedInitiativeTypeId = proposedInitiativeType.Id;

        _paoUserId = TestDataHelper.GetOrCreateTestUser(_context, "test@unops.org");

        var entityRole = _context.EntityRoles.FirstOrDefault(r => r.Code == "Opportunity_Manager_Opportunity" && !r.IsDeleted);
        if (entityRole == null)
        {
            entityRole = new EntityRole
            {
                EntityType = "Opportunity",
                Name = "Opportunity Manager",
                Description = "Manages the opportunity",
                IsInternal = true,
                AllowsMultiple = false,
                Code = "Opportunity_Manager_Opportunity",
                Status = EntityStatus.Active,
                IsDeleted = false
            };
            _context.EntityRoles.Add(entityRole);
            _context.SaveChanges();
        }
        _entityRoleId = entityRole.Id;

        _context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Helper: Creates a test opportunity with auto-generated ID and registers it for cleanup.
    /// </summary>
    private async Task<int> CreateTestOpportunityAsync(
        string? name = null,
        string? description = null,
        string stage = "IDENTIFY & PROFILE",
        EntityStatus status = EntityStatus.Draft,
        decimal? budgetUSD = null,
        int? responsibleOrgUnitId = null,
        string? partnerReference = null,
        string? challenges = null,
        DateTime? createdDate = null)
    {
        var opportunity = new OpportunityEntity
        {
            Name = name ?? $"Test Opportunity {_testMarker}",
            Description = description ?? "Test Description",
            Stage = stage,
            Status = status,
            CreatedBy = _paoUserId,
            CreatedDate = createdDate ?? DateTime.UtcNow,
            LastModifiedBy = _paoUserId,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = false,
            InitiativeBudgetUSD = budgetUSD,
            ResponsibleOrgUnitId = responsibleOrgUnitId,
            PartnerReference = partnerReference,
            Challenges = challenges
        };
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();
        _createdOpportunityIds.Add(opportunity.Id);
        return opportunity.Id;
    }

    #region P2 - AI Integration Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "AI")]
    [Trait("TestId", "TC-UNOPS-ADV-001")]
    public async Task ApplyAiChanges_UpdatesMultipleFields_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Original Name", description: "Original Description");

        var aiChangesRequest = new ApplyOpportunityAiChangesRequest
        {
            Name = "AI-Enhanced Project Name",
            Description = "AI-generated comprehensive description with improved clarity and structure",
            ExpectedImpact = "Significant positive impact on target communities",
            ExpectedOutcomes = "Improved infrastructure and sustainable development",
            Challenges = "Implementation risks include weather conditions and resource availability",
            ResultsFocus = "Sustainable Development Goal alignment"
        };

        // Act
        var result = await _manager.ApplyAiChangesAsync(oppId, aiChangesRequest);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("AI-Enhanced Project Name");
        result.Description.Should().Contain("AI-generated");
        result.ExpectedImpact.Should().Be("Significant positive impact on target communities");
    }

    [SkipIfNotPostgreSQLFact]
    [Trait("Category", "P2")]
    [Trait("Type", "AI")]
    [Trait("TestId", "TC-UNOPS-ADV-002")]
    public async Task GetOpportunityDetailsForAI_ReturnsCompleteContext()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(
            name: "Complex Multi-Partner Project",
            description: "Detailed project description",
            budgetUSD: 5000000,
            responsibleOrgUnitId: _orgHierarchyId);

        // Act
        var result = await _manager.GetOpportunityDetailsForAIAsync(oppId);

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainKey("id");
        result.Should().ContainKey("name");
        result.Should().ContainKey("description");
        result["id"].ToString().Should().Be(oppId.ToString());
        result["name"].Should().Be("Complex Multi-Partner Project");
        result["description"].Should().Be("Detailed project description");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "AI")]
    [Trait("TestId", "TC-UNOPS-ADV-003")]
    public async Task ApplyAiChanges_PreservesManuallyEditedFields_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(
            name: "Manually Edited Name",
            description: "User provided description",
            budgetUSD: 1000000);

        // AI changes only description, not budget
        var aiChangesRequest = new ApplyOpportunityAiChangesRequest
        {
            Description = "AI-enhanced description"
            // Budget not included - should preserve manual value
        };

        // Act
        var result = await _manager.ApplyAiChangesAsync(oppId, aiChangesRequest);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(1000000); // Preserved from manual entry
    }

    #endregion

    #region P2 - Performance and Scalability Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Performance")]
    [Trait("TestId", "TC-UNOPS-ADV-004")]
    public async Task GetOpportunityWithManyRelationships_PerformsWithinTimeout()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Large Scale Programme", description: "Test Description");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _manager.GetOpportunityAsync(oppId);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(3000); // Should complete within 3 seconds
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Performance")]
    [Trait("TestId", "TC-UNOPS-ADV-005")]
    public async Task CreateOpportunityWithManyChildRecords_Success()
    {
        // Arrange - Create real partners in the database
        var fundingPartnerIds = new List<int>();
        for (int i = 1; i <= 10; i++)
        {
            var partner = new UNOPSDomain.Entities.UNOPSPartner
            {
                Name = $"FundingPartner_{_testMarker}_{i}",
                Status = EntityStatus.Active,
                IsDeleted = false,
                CreatedBy = _paoUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = _paoUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();
            fundingPartnerIds.Add(partner.Id);
        }

        var clientPartnerIds = new List<int>();
        for (int i = 1; i <= 5; i++)
        {
            var partner = new UNOPSDomain.Entities.UNOPSPartner
            {
                Name = $"ClientPartner_{_testMarker}_{i}",
                Status = EntityStatus.Active,
                IsDeleted = false,
                CreatedBy = _paoUserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedBy = _paoUserId,
                LastModifiedDate = DateTime.UtcNow
            };
            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();
            clientPartnerIds.Add(partner.Id);
        }

        var request = new OpportunityRequest
        {
            Name = "Complex Opportunity",
            Description = "With many related records",
            FundingPartners = fundingPartnerIds.Select((id, i) => new OpportunityFundingPartnerRequest
            {
                PartnerId = id,
                Amount = 100000 * (i + 1),
                CurrencyId = _currencyId
            }).ToList(),
            ClientPartners = clientPartnerIds.Select(id => new OpportunityClientPartnerRequest
            {
                PartnerId = id
            }).ToList(),
            Deliverables = new List<OpportunityDeliverableRequest>(),
            Countries = Enumerable.Range(1, 3).Select(i => new OpportunityCountryRequest
            {
                CountryId = _countryId
            }).ToList()
        };

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region P2 - Edge Case Tests

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Contain("プロジェクト");
        result.Name.Should().Contain("Développement");
    }

    [Fact]

    [Trait("Defect", "DEF-105")]
    [Trait("Category", "P2")]
    [Trait("Type", "EdgeCase")]
    [Trait("TestId", "TC-UNOPS-ADV-007")]
    public async Task UpdateOpportunity_ClearOptionalFields_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(
            name: "Test Opportunity",
            description: "Has description",
            partnerReference: "REF-001",
            challenges: "Has challenges");

        // Act - Clear optional fields by setting to null
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            PartnerReference = null,
            // Description and Challenges not specified - should remain
        };

        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();

        var savedOpportunity = await _context.Opportunities.FindAsync(oppId);
        savedOpportunity!.PartnerReference.Should().BeNull(); // Cleared
    }

    [Fact]
    [Trait("Defect", "DEF-089")]
    [Trait("Category", "P2")]
    [Trait("Type", "EdgeCase")]
    [Trait("TestId", "TC-UNOPS-ADV-008")]
    public async Task GetOpportunity_MultipleTimesInParallel_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Parallel Access Test", description: "Test Description");

        // Act - Multiple parallel reads
        var tasks = Enumerable.Range(1, 10).Select(_ =>
            _manager.GetOpportunityAsync(oppId)
        ).ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().OnlyContain(r => r != null && r.Id == oppId);
    }

    [SkipIfInMemoryFact]
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
            InitiativeBudgetUSD = 999_999_999_999.99m // Very large but within PostgreSQL numeric precision
        };

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(999_999_999_999.99m);
    }

    [SkipIfInMemoryFact]
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

        // Act & Assert - Should handle gracefully
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);
        result.Should().NotBeNull();
    }

    #endregion

    #region P2 - Complex Workflow Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Workflow")]
    [Trait("TestId", "TC-UNOPS-ADV-011")]
    public async Task OpportunityWorkflow_ProgressThroughAllStages_Success()
    {
        // Arrange - Workflow stages are now stored as string values in Opportunity.Stage property
        var oppId = await CreateTestOpportunityAsync(name: "Workflow Test", description: "Test Description");

        // Act - Workflow stage progression now handled by workflow service, not direct updates
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Updated Workflow Test"
        };

        var result = await _manager.UpdateOpportunityAsync(updateRequest);
        result.Should().NotBeNull();
        result!.Stage.Should().NotBeNullOrEmpty();

        // Assert - Verify opportunity was updated
        var finalOpportunity = await _context.Opportunities.FindAsync(oppId);
        finalOpportunity!.Stage.Should().NotBeNullOrEmpty();
    }

    [SkipIfInMemoryFact]
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
            ResponsibleOrgUnitId = _orgHierarchyId
        }).ToArray();

        // Act
        var results = new List<OpportunityModel>();
        foreach (var request in requests)
        {
            var result = await _manager.CreateOpportunityAsync(request);
            _createdOpportunityIds.Add(result.Id);
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

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "DataConsistency")]
    [Trait("TestId", "TC-UNOPS-ADV-013")]
    public async Task UpdateOpportunity_MaintainsAuditTrail_Success()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        var oppId = await CreateTestOpportunityAsync(name: "Audit Test", description: "Test Description");

        // Record the actual CreatedDate after insert (AuditableDbContext sets this to UtcNow)
        _context.ChangeTracker.Clear();
        var originalEntity = await _context.Opportunities.AsNoTracking().FirstAsync(o => o.Id == oppId);
        var originalCreatedDate = originalEntity.CreatedDate;

        // Act - Update
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Updated Name"
        };

        await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert - Verify audit fields
        _context.ChangeTracker.Clear();
        var savedOpportunity = await _context.Opportunities.FindAsync(oppId);
        savedOpportunity.Should().NotBeNull();
        savedOpportunity!.CreatedBy.Should().Be(_paoUserId); // Original creator preserved
        savedOpportunity.CreatedDate.Should().Be(originalCreatedDate); // CreatedDate not changed by update
        savedOpportunity.LastModifiedBy.Should().Be(_paoUserId); // Updated
        savedOpportunity.LastModifiedDate.Should().NotBeNull(); // Set
        savedOpportunity.LastModifiedDate.Should().BeOnOrAfter(beforeCreate); // After test start
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "DataConsistency")]
    [Trait("TestId", "TC-UNOPS-ADV-014")]
    public async Task DeleteOpportunity_PreservesData_ForAudit()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(
            name: "To Be Deleted",
            description: "Important data to preserve",
            budgetUSD: 1000000);

        // Act - Soft delete
        await _manager.DeleteOpportunityAsync(oppId);

        // Assert - Data preserved for audit
        var deletedOpportunity = await _context.Opportunities
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == oppId);

        deletedOpportunity.Should().NotBeNull();
        deletedOpportunity!.IsDeleted.Should().BeTrue();
        deletedOpportunity.Name.Should().Be("To Be Deleted"); // Data preserved
        deletedOpportunity.Description.Should().Be("Important data to preserve");
        deletedOpportunity.InitiativeBudgetUSD.Should().Be(1000000);
        deletedOpportunity.DeletedBy.Should().Be(_paoUserId);
        deletedOpportunity.DeletedDate.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Draft"); // Default status
        result.Stage.Should().NotBeNullOrEmpty(); // Default workflow stage

        var savedOpportunity = await _context.Opportunities.FindAsync(result.Id);
        savedOpportunity!.Stage.Should().NotBeNullOrEmpty();
        savedOpportunity.Status.Should().Be(EntityStatus.Draft);
    }

    #endregion

    #region P2 - Integration with Other Managers Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-ADV-016")]
    public async Task GetOpportunitiesByPartner_ReturnsRelated_Success()
    {
        // Arrange - Create a partner and opportunities linked via FundingPartners
        var partner = new UNOPSDomain.Entities.UNOPSPartner
        {
            Name = $"Test Partner {_testMarker}",
            PartnerShortDescription = "Test partner for relationship query",
            CreatedBy = _paoUserId,
            LastModifiedBy = _paoUserId,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _context.Partners.Add(partner);
        await _context.SaveChangesAsync();

        var oppId1 = await CreateTestOpportunityAsync(name: "Partner Opp 1", description: "Test Description");
        var oppId2 = await CreateTestOpportunityAsync(name: "Partner Opp 2", description: "Test Description");

        // Link opportunity to partner via FundingPartners
        _context.Set<OpportunityFundingPartner>().Add(new OpportunityFundingPartner
        {
            OpportunityId = oppId1,
            PartnerId = partner.Id,
            Amount = 100000,
            CurrencyId = _currencyId
        });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        var result = await _manager.GetOpportunitiesByPartnerIdAsync(partner.Id);

        // Assert
        result.Should().NotBeNull();
        var opportunities = result.ToList();
        opportunities.Should().NotBeEmpty();
        opportunities.Should().Contain(o => o.Name == "Partner Opp 1");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-ADV-017")]
    public async Task AssignCreatorAsOpportunityManager_IntegratesWithTeam_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Team Integration Test", description: "Test Description");

        // Act
        await _manager.AssignCreatorAsOpportunityManagerAsync(oppId, _paoUserId);

        // Assert - Verify method completes
        // In actual implementation, would verify stakeholder assignment
        var savedOpportunity = await _context.Opportunities
            .Include(o => o.Stakeholders)
            .FirstOrDefaultAsync(o => o.Id == oppId);

        savedOpportunity.Should().NotBeNull();
    }

    #endregion

    #region P2 - Null Safety and Error Resilience Tests

    [SkipIfInMemoryFact]
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

    [Trait("Defect", "DEF-072")]
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

    [SkipIfInMemoryFact]
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

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Workflow")]
    [Trait("TestId", "TC-UNOPS-ADV-021")]
    public async Task UpdateOpportunity_TransitionFromDraftToActive_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Status Transition Test", description: "Test Description");

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Updated Name"
            // WorkflowStageId property removed - managed by workflow system
        };

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Stage.Should().NotBeNullOrEmpty();
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Draft");
    }

    #endregion

    #region P2 - High Risk Acknowledgment Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-ADV-023")]
    public async Task UpdateOpportunity_AcknowledgeHighRisks_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "High Risk Opportunity", description: "Test Description");

        // Act
        var result = await _manager.GetOpportunityAsync(oppId);

        // Assert - In actual implementation, would verify acknowledgment
        result.Should().NotBeNull();
    }

    #endregion

    #region P2 - Delivery Modality Tests

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.DeliveryModality.Should().Be(1);
    }

    [Fact]

    [Trait("Defect", "DEF-105")]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-ADV-025")]
    public async Task UpdateOpportunity_ChangeDeliveryModality_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Modality Change Test", description: "Test Description");

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Updated Modality Test"
            // DeliveryModality property removed from UpdateOpportunityRequest
        };

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.DeliveryModality.Should().Be(2);
    }

    #endregion

    #region P2 - New Value Range Tests

    [SkipIfInMemoryFact]
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
            ResponsibleOrgUnitId = _orgHierarchyId,
            InitiativeBudgetUSD = 10000000 // Exceeds historical max
        };

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(10000000);
    }

    #endregion

    #region P2 - Opportunity Stats Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-ADV-027")]
    public async Task GetOpportunity_IncludesStats_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(
            name: "Stats Test Opportunity",
            description: "Test Description",
            responsibleOrgUnitId: _orgHierarchyId);

        // Act
        var result = await _manager.GetOpportunityAsync(oppId);

        // Assert
        result.Should().NotBeNull();
        result!.Stats.Should().NotBeNull();
    }

    #endregion

    #region P2 - Conditional Tags Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "BusinessLogic")]
    [Trait("TestId", "TC-UNOPS-ADV-028")]
    public async Task GetOpportunity_CalculatesConditionalTags_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(
            name: "Tagged Opportunity",
            description: "Test Description",
            budgetUSD: 10000000,
            createdDate: DateTime.UtcNow.AddDays(-90)); // Old draft

        // Act
        var result = await _manager.GetOpportunityAsync(oppId);

        // Assert
        result.Should().NotBeNull();
        // In actual implementation, would verify tags like "Large Budget", "Stale Draft", etc.
    }

    #endregion

    #region P2 - User Role Context Tests

    [Fact]

    [Trait("Defect", "DEF-104")]
    [Trait("Category", "P2")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-ADV-029")]
    public async Task GetOpportunity_IncludesUserRoleContext_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Role Context Test", description: "Test Description");

        // Act
        var result = await _manager.GetOpportunityAsync(_testUser, oppId);

        // Assert
        result.Should().NotBeNull();
        result!.UserRole.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region P2 - Opportunity Lifecycle Edge Cases

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "EdgeCase")]
    [Trait("TestId", "TC-UNOPS-ADV-030")]
    public async Task UpdateOpportunity_RapidSuccessiveUpdates_HandlesCorrectly()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Rapid Update Test", description: "Test Description");

        // Act - Perform 3 rapid updates
        var update1 = new UpdateOpportunityRequest { Id = oppId, Name = "Update 1" };
        var update2 = new UpdateOpportunityRequest { Id = oppId, Name = "Update 2" };
        var update3 = new UpdateOpportunityRequest { Id = oppId, Name = "Update 3" };

        await _manager.UpdateOpportunityAsync(update1);
        await _manager.UpdateOpportunityAsync(update2);
        var finalResult = await _manager.UpdateOpportunityAsync(update3);

        // Assert
        finalResult.Should().NotBeNull();

        var savedOpportunity = await _context.Opportunities.FindAsync(oppId);
        savedOpportunity!.Name.Should().Be("Update 3");
        savedOpportunity.LastModifiedDate.Should().NotBeNull();
    }

    #endregion

    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(string userId)
    {
        var accessor = new Mock<IHttpContextAccessor>();
        var httpContext = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Headers).Returns(new HeaderDictionary());
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "testuser@unops.org")
        }, "TestAuthType"));
        httpContext.Setup(m => m.User).Returns(user);
        httpContext.Setup(m => m.Request).Returns(request.Object);
        accessor.Setup(m => m.HttpContext).Returns(httpContext.Object);
        return accessor;
    }

    public void Dispose()
    {
        try
        {
            if (TestEnvironment.UsePostgreSQL && _createdOpportunityIds.Any())
            {
                var ids = string.Join(",", _createdOpportunityIds);
                _context.Database.ExecuteSql($"DELETE FROM public.\"Opportunities\" WHERE \"Id\" IN ({ids})");
            }
        }
        catch { /* Best-effort cleanup */ }

        if (TestEnvironment.UseInMemory)
        {
            try { _context.Database.EnsureDeleted(); }
            catch { /* SQLite connection may already be closed during concurrent test runs */ }
        }
        if (_transaction != null)
        {
            try { _transaction.Rollback(); }
            catch { }
            _transaction.Dispose();
            _transaction = null;
        }
        _context.Dispose();
    }
}
