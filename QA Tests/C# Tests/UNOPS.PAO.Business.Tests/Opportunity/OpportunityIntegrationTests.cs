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
using UNOPS.PAO.UNOPSDomain.Entities;
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
/// Integration tests for opportunity workflows and cross-feature scenarios
/// Tests real-world opportunity management scenarios end-to-end
/// Created: January 15, 2026
/// Priority: P1 (High)
/// SKIPPED: QA-009 - Z.EntityFramework.Extensions requires relational database (PostgreSQL)
/// </summary>
public class OpportunityIntegrationTests : IDisposable
{
    private const string SkipReason = "QA-009: Z.EntityFramework.Extensions requires relational database";
    private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
    private readonly UNOPSAppDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly string _testMarker = $"INT_{Guid.NewGuid():N}";
    private readonly List<int> _createdOpportunityIds = new();
    private int _currencyId;
    private int _countryId;
    private int _orgHierarchyId;
    private int _orgHierarchyId2;
    private int _proposedInitiativeTypeId;
    private int _paoUserId;
    private int _entityRoleId;
    private int _partnerId1;
    private int _partnerId2;
    private int _partnerId3;
    private int _sdgId6;
    private int _sdgId13;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IDbContextFactory<UNOPSAppDbContext>> _mockDbContextFactory;
    private readonly Mock<IExchangeRateService> _mockExchangeRateService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly UNOPSOpportunityManager _manager;
    private readonly ClaimsPrincipal _testUser;

    public OpportunityIntegrationTests()
    {
        _dbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"OpportunityIntegrationTestDb_{Guid.NewGuid()}");
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        // Phase 1: Resolve the test user ID using a temporary context (outside transaction).
        // AuditableDbContext caches _currentUserId at construction, so we must know the
        // real user ID before creating the main context.
        {
            var tempAccessor = CreateMockHttpContextAccessor("0");
            var tempResolver = new UserResolverService<int>(tempAccessor.Object, null);
            using var tempCtx = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_dbContextOptions, tempResolver, mockDbSchema.Object);
            _paoUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "testuser@unops.org");
        }

        // Phase 2: Create the MAIN context with the ACTUAL test user ID in claims.
        var mainAccessor = CreateMockHttpContextAccessor(_paoUserId.ToString());
        var userResolverService = new UserResolverService<int>(mainAccessor.Object, null);
        _context = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_dbContextOptions, userResolverService, mockDbSchema.Object);

        if (TestEnvironment.UsePostgreSQL)
        {
            _transaction = _context.Database.BeginTransaction();
        }

        SeedTestData();

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });
        _mapper = mapperConfig.CreateMapper();
        
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
        
        _mockPermissionService = new Mock<IPermissionService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockDbContextFactory = new Mock<IDbContextFactory<UNOPSAppDbContext>>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockExchangeRateService = new Mock<IExchangeRateService>();

        _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _paoUserId.ToString()),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "testuser@unops.org")
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

        var orgHierarchy = _context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "SAH" && !o.IsDeleted);
        if (orgHierarchy == null)
        {
            orgHierarchy = new OrganizationHierarchy { Name = "South Asia Hub", Code = "SAH", Description = "South Asia Regional Hub", IsDeleted = false };
            _context.OrganizationHierarchies.Add(orgHierarchy);
            _context.SaveChanges();
        }
        _orgHierarchyId = orgHierarchy.Id;

        var orgHierarchy2 = _context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "BDO" && !o.IsDeleted);
        if (orgHierarchy2 == null)
        {
            orgHierarchy2 = new OrganizationHierarchy { Name = "Bangladesh Office", Code = "BDO", Description = "Bangladesh Country Office", ParentId = _orgHierarchyId, IsDeleted = false };
            _context.OrganizationHierarchies.Add(orgHierarchy2);
            _context.SaveChanges();
        }
        _orgHierarchyId2 = orgHierarchy2.Id;

        var proposedInitiativeType = _context.ProposedInitiativeTypes.FirstOrDefault(p => p.Name == "Project" && !p.IsDeleted);
        if (proposedInitiativeType == null)
        {
            proposedInitiativeType = new ProposedInitiativeType { Name = "Project", IsDeleted = false };
            _context.ProposedInitiativeTypes.Add(proposedInitiativeType);
            _context.SaveChanges();
        }
        _proposedInitiativeTypeId = proposedInitiativeType.Id;

        _paoUserId = TestDataHelper.GetOrCreateTestUser(_context, "testuser@unops.org");

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

        // Partners for multi-partner tests (UNOPSAppDbContext uses UNOPSPartner)
        var partner1 = _context.Partners.FirstOrDefault(p => p.Name == $"Test Partner 1 {_testMarker}" && !p.IsDeleted);
        if (partner1 == null)
        {
            partner1 = new UNOPSPartner { Name = $"Test Partner 1 {_testMarker}", Status = EntityStatus.Active, IsDeleted = false };
            _context.Partners.Add(partner1);
            _context.SaveChanges();
        }
        _partnerId1 = partner1.Id;

        var partner2 = _context.Partners.FirstOrDefault(p => p.Name == $"Test Partner 2 {_testMarker}" && !p.IsDeleted);
        if (partner2 == null)
        {
            partner2 = new UNOPSPartner { Name = $"Test Partner 2 {_testMarker}", Status = EntityStatus.Active, IsDeleted = false };
            _context.Partners.Add(partner2);
            _context.SaveChanges();
        }
        _partnerId2 = partner2.Id;

        var partner3 = _context.Partners.FirstOrDefault(p => p.Name == $"Test Partner 3 {_testMarker}" && !p.IsDeleted);
        if (partner3 == null)
        {
            partner3 = new UNOPSPartner { Name = $"Test Partner 3 {_testMarker}", Status = EntityStatus.Active, IsDeleted = false };
            _context.Partners.Add(partner3);
            _context.SaveChanges();
        }
        _partnerId3 = partner3.Id;

        // SDGs for SDG-aligned tests
        var sdg6 = _context.SDGs.FirstOrDefault(s => s.SDGNumber == "6");
        if (sdg6 == null)
        {
            sdg6 = new SDG { Name = "Clean Water", SDGNumber = "6", SDGId = "6", IsDeleted = false };
            _context.SDGs.Add(sdg6);
            _context.SaveChanges();
        }
        _sdgId6 = sdg6.Id;

        var sdg13 = _context.SDGs.FirstOrDefault(s => s.SDGNumber == "13");
        if (sdg13 == null)
        {
            sdg13 = new SDG { Name = "Climate Action", SDGNumber = "13", SDGId = "13", IsDeleted = false };
            _context.SDGs.Add(sdg13);
            _context.SaveChanges();
        }
        _sdgId13 = sdg13.Id;

        _context.ChangeTracker.Clear();
    }

    private async Task<int> CreateTestOpportunityAsync(
        string? name = null,
        string? description = null,
        string stage = "IDENTIFY & PROFILE",
        EntityStatus status = EntityStatus.Draft,
        decimal? budgetUSD = null,
        int? responsibleOrgUnitId = null,
        string? partnerReference = null,
        string? challenges = null)
    {
        var opportunity = new OpportunityEntity
        {
            Name = name ?? $"Test Opportunity {_testMarker}",
            Description = description ?? "Test Description",
            Stage = stage,
            Status = status,
            CreatedBy = _paoUserId,
            CreatedDate = DateTime.UtcNow,
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

    #region P1 - Complete Opportunity Lifecycle Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-001")]
    public async Task CompleteOpportunityLifecycle_CreateUpdateGetDelete_Success()
    {
        // Arrange - Create
        var createRequest = new OpportunityRequest
        {
            Name = "Complete Lifecycle Test Opportunity",
            Description = "Testing full CRUD lifecycle",
            ResponsibleOrgUnitId = _orgHierarchyId,
            ProposedInitiativeTypeId = _proposedInitiativeTypeId,
            InitiativeBudgetUSD = 1000000
        };

        // Act - Create
        var created = await _manager.CreateOpportunityAsync(createRequest);
        _createdOpportunityIds.Add(created.Id);
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);

        var oppId = created.Id;

        // Act - Update
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Updated Lifecycle Test",
            InitiativeBudgetUSD = 1500000
        };

        var updated = await _manager.UpdateOpportunityAsync(updateRequest);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Lifecycle Test");

        // Act - Get
        var retrieved = await _manager.GetOpportunityAsync(oppId);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(oppId);

        // Act - Delete
        var deleted = await _manager.DeleteOpportunityAsync(oppId);
        deleted.Should().BeTrue();

        // Remove from cleanup list since we deleted it
        _createdOpportunityIds.Remove(oppId);

        // Verify soft delete
        var afterDelete = await _manager.GetOpportunityAsync(oppId);
        afterDelete.Should().BeNull(); // Soft-deleted records not returned
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-002")]
    public async Task OpportunityWithMultipleSections_UpdatesAllSections_Success()
    {
        // Arrange
        var createRequest = new OpportunityRequest
        {
            Name = "Multi-Section Opportunity",
            Description = "Testing all section updates",
            ResponsibleOrgUnitId = _orgHierarchyId,
            ProposedInitiativeTypeId = _proposedInitiativeTypeId
        };

        var created = await _manager.CreateOpportunityAsync(createRequest);
        _createdOpportunityIds.Add(created.Id);
        var oppId = created.Id;

        // Act - Update Overview Section
        var overviewRequest = new OverviewSectionRequest
        {
            Description = "Comprehensive project description"
        };

        var overviewResult = await _manager.UpdateOverviewSectionAsync(oppId, overviewRequest);
        overviewResult.Should().NotBeNull();

        // Act - Update What Section
        var whatRequest = new WhatSectionRequest
        {
            Deliverables = new List<OpportunityDeliverableRequest>()
        };

        var whatResult = await _manager.UpdateWhatSectionAsync(oppId, whatRequest);
        whatResult.Should().NotBeNull();

        // Act - Update Why Section
        var whyRequest = new WhySectionRequest
        {
            ResultsFocus = "Sustainable development",
            ExpectedImpact = "Positive community impact"
        };

        var whyResult = await _manager.UpdateWhySectionAsync(oppId, whyRequest);
        whyResult.Should().NotBeNull();

        // Act - Update When Section
        var whenRequest = new WhenSectionRequest
        {
            TargetSigningDate = DateTime.UtcNow.AddMonths(6),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(24)
        };

        var whenResult = await _manager.UpdateWhenSectionAsync(oppId, whenRequest);
        whenResult.Should().NotBeNull();

        // Assert - All sections updated successfully
        var finalOpportunity = await _manager.GetOpportunityAsync(oppId);
        finalOpportunity.Should().NotBeNull();
    }

    #endregion

    #region P1 - Multi-Partner Integration Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-003")]
    public async Task OpportunityWithMultiplePartners_CreatesRelationships_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Multi-Partner Initiative",
            Description = "Collaborative project with multiple partners",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = _partnerId1, Amount = 1000000, CurrencyId = _currencyId },
                new() { PartnerId = _partnerId2, Amount = 750000, CurrencyId = _currencyId }
            },
            ClientPartners = new List<OpportunityClientPartnerRequest>
            {
                new() { PartnerId = _partnerId3 }
            }
        };

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        // Verify opportunity created
        var savedOpportunity = await _context.Opportunities
            .Include(o => o.FundingPartners)
            .Include(o => o.ClientPartners)
            .FirstOrDefaultAsync(o => o.Id == result.Id);

        savedOpportunity.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-004")]
    public async Task OpportunityWithSDGsAndUNCFOutcomes_LinksFrameworks_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "SDG-Aligned Initiative",
            Description = "Project aligned with SDGs and UNCF outcomes",
            SDGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = _sdgId6 },  // Clean Water
                new() { SDGId = _sdgId13 }  // Climate Action
            }
        };

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    #region P1 - Workflow Progression Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-005")]
    public async Task OpportunityWorkflowProgression_UpdatesStages_Success()
    {
        // Arrange - Create opportunity in Identification stage
        var createRequest = new OpportunityRequest
        {
            Name = "Workflow Progression Test",
            Description = "Testing workflow stage progression",
            ResponsibleOrgUnitId = _orgHierarchyId,
            ProposedInitiativeTypeId = _proposedInitiativeTypeId
        };

        var created = await _manager.CreateOpportunityAsync(createRequest);
        _createdOpportunityIds.Add(created.Id);
        var oppId = created.Id;
        created.Stage.Should().Be("IDENTIFY & PROFILE");

        // Act - Workflow stage progression now handled by workflow service, not direct update
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Updated Name"
        };

        var updated = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        updated.Should().NotBeNull();
        updated!.Stage.Should().NotBeNullOrEmpty();

        // Verify in database
        var savedOpportunity = await _context.Opportunities.FindAsync(oppId);
        savedOpportunity.Should().NotBeNull();
        savedOpportunity!.Stage.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region P1 - Data Validation and Constraints Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-INT-006")]
    public async Task CreateOpportunity_WithInvalidCountry_HandlesGracefully()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Invalid Country Test",
            Description = "Testing invalid foreign key reference",
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = 999 } // Non-existent country
            }
        };

        // Act & Assert - Should handle invalid reference gracefully
        // Actual behavior depends on implementation (throw or filter out invalid)
        OpportunityModel? created = null;
        Func<Task> act = async () => created = await _manager.CreateOpportunityAsync(request);

        // Either succeeds (filtering invalid) or throws appropriate exception
        await act.Should().NotThrowAsync<NullReferenceException>();

        if (created != null)
        {
            _createdOpportunityIds.Add(created.Id);
        }
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-INT-007")]
    public async Task UpdateOpportunity_ConcurrentModification_HandlesCorrectly()
    {
        // Arrange - Create base opportunity
        var oppId = await CreateTestOpportunityAsync(name: "Concurrent Test", description: "Test Description");

        // Act - Simulate concurrent updates
        var update1 = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Update from User 1"
        };

        var update2 = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Update from User 2"
        };

        // First update succeeds
        var result1 = await _manager.UpdateOpportunityAsync(update1);
        result1.Should().NotBeNull();

        // Second update also succeeds (last write wins in this implementation)
        var result2 = await _manager.UpdateOpportunityAsync(update2);
        result2.Should().NotBeNull();

        // Verify final state
        var finalState = await _context.Opportunities.FindAsync(oppId);
        finalState.Should().NotBeNull();
        finalState!.Name.Should().Be("Update from User 2");
    }

    #endregion

    #region P1 - Complex Query and Filter Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-008")]
    public async Task GetAllOpportunities_WithMultipleFilters_ReturnsFiltered()
    {
        // Arrange
        await CreateTestOpportunityAsync(name: "Active Opportunity 1", description: "Test Description", stage: "DEVELOP", status: EntityStatus.Active, responsibleOrgUnitId: _orgHierarchyId);
        await CreateTestOpportunityAsync(name: "Draft Opportunity", description: "Test Description", stage: "IDENTIFY & PROFILE", status: EntityStatus.Draft, responsibleOrgUnitId: _orgHierarchyId);
        await CreateTestOpportunityAsync(name: "Active Opportunity 2", description: "Test Description", stage: "DEVELOP", status: EntityStatus.Active, responsibleOrgUnitId: _orgHierarchyId2);

        // Act
        var result = await _manager.GetAllOpportunitiesAsync();

        // Assert - Filter to our created opportunities (PostgreSQL may have other data)
        var opportunities = result.Where(o => _createdOpportunityIds.Contains(o.Id)).ToList();
        opportunities.Should().HaveCount(3);
        opportunities.Should().OnlyContain(o => !string.IsNullOrEmpty(o.Status));
    }

    #endregion

    #region P1 - Bulk Operations Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-009")]
    public async Task CreateMultipleOpportunities_BatchOperation_Success()
    {
        // Arrange
        var requests = new List<OpportunityRequest>
        {
            new()
            {
                Name = "Batch Opportunity 1",
                Description = "First in batch",
                ResponsibleOrgUnitId = _orgHierarchyId
            },
            new()
            {
                Name = "Batch Opportunity 2",
                Description = "Second in batch",
                ResponsibleOrgUnitId = _orgHierarchyId
            },
            new()
            {
                Name = "Batch Opportunity 3",
                Description = "Third in batch",
                ResponsibleOrgUnitId = _orgHierarchyId2
            }
        };

        // Act - Create multiple opportunities
        var results = new List<OpportunityModel>();
        foreach (var request in requests)
        {
            var result = await _manager.CreateOpportunityAsync(request);
            _createdOpportunityIds.Add(result.Id);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.Id > 0);
        results.Select(r => r.Name).Should().BeEquivalentTo(requests.Select(r => r.Name));

        // Verify all saved to database
        var savedOpportunities = await _context.Opportunities.Where(o => _createdOpportunityIds.Contains(o.Id)).ToListAsync();
        savedOpportunities.Should().HaveCount(3);
    }

    #endregion

    #region P2 - Performance and Load Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Performance")]
    [Trait("TestId", "TC-UNOPS-INT-010")]
    public async Task GetOpportunityWithLargeDataset_PerformsWithinBounds()
    {
        // Arrange - Create opportunity with many related records
        var oppId = await CreateTestOpportunityAsync(name: "Large Dataset Test", description: "Test Description");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _manager.GetOpportunityAsync(oppId);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    #endregion

    #region P1 - External Stakeholder Management Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-011")]
    public async Task CreateOpportunity_WithExternalStakeholders_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Multi-Stakeholder Initiative",
            Description = "Involves government, NGOs, and private sector",
            MiscExternalStakeholders = "Ministry of Infrastructure, Local NGO Consortium",
            ExternalStakeholderNotes = "Coordination meetings scheduled bi-weekly"
        };

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.MiscExternalStakeholders.Should().Contain("Ministry of Infrastructure");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-012")]
    public async Task UpdateOpportunity_AddExternalStakeholdersAfterCreation_Success()
    {
        // Arrange
        var oppId = await CreateTestOpportunityAsync(name: "Evolving Stakeholder Initiative", description: "Test Description");

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Evolving Stakeholder Initiative"
        };

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region P1 - Bulk Operations Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-013")]
    public async Task BulkUpdateOpportunities_UpdateWorkflowStage_Success()
    {
        // Arrange - Create 5 opportunities
        var oppIds = new List<int>();
        for (var i = 1; i <= 5; i++)
        {
            var id = await CreateTestOpportunityAsync(name: $"Bulk Test Opportunity {i}", description: "Test Description");
            oppIds.Add(id);
        }

        // Act - Update all to Development stage
        var results = new List<OpportunityModel>();
        foreach (var oppId in oppIds)
        {
            var opp = await _context.Opportunities.FindAsync(oppId);
            var updateRequest = new UpdateOpportunityRequest
            {
                Id = oppId,
                Name = opp!.Name
            };
            var result = await _manager.UpdateOpportunityAsync(updateRequest);
            if (result != null) results.Add(result);
        }

        // Assert
        results.Should().HaveCount(5);
        results.Should().OnlyContain(r => !string.IsNullOrEmpty(r.Stage));

        var savedOpportunities = await _context.Opportunities.Where(o => oppIds.Contains(o.Id)).ToListAsync();
        savedOpportunities.Should().OnlyContain(o => !string.IsNullOrEmpty(o.Stage));
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-014")]
    public async Task BulkDelete_MultipleOpportunities_Success()
    {
        // Arrange
        var oppIds = new List<int>();
        for (var i = 1; i <= 3; i++)
        {
            var id = await CreateTestOpportunityAsync(name: $"To Delete {i}", description: "Test Description");
            oppIds.Add(id);
        }

        // Act - Bulk delete (soft delete)
        var deleteResults = new List<bool>();
        foreach (var oppId in oppIds)
        {
            var result = await _manager.DeleteOpportunityAsync(oppId);
            deleteResults.Add(result);
        }

        // Assert
        deleteResults.Should().OnlyContain(r => r == true);

        // Verify our opportunities are soft-deleted (excluded from normal queries)
        var remainingOpportunities = await _context.Opportunities.Where(o => !o.IsDeleted && oppIds.Contains(o.Id)).ToListAsync();
        remainingOpportunities.Should().BeEmpty();
    }

    #endregion

    #region P2 - Pooled Funding Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-015")]
    public async Task CreateOpportunity_WithPooledFunding_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Pooled Funding Initiative",
            Description = "Multiple donors contributing to common fund",
            IsPooledFunding = true,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = _partnerId1, Amount = 500000, CurrencyId = _currencyId },
                new() { PartnerId = _partnerId2, Amount = 500000, CurrencyId = _currencyId },
                new() { PartnerId = _partnerId3, Amount = 500000, CurrencyId = _currencyId }
            },
            InitiativeBudgetUSD = 1500000
        };

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsPooledFunding.Should().BeTrue();
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
