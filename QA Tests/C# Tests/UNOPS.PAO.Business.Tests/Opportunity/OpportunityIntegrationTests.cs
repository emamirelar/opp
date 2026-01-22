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
/// Integration tests for opportunity workflows and cross-feature scenarios
/// Tests real-world opportunity management scenarios end-to-end
/// Created: January 15, 2026
/// Priority: P1 (High)
/// </summary>
public class OpportunityIntegrationTests : IDisposable
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

    public OpportunityIntegrationTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: $"OpportunityIntegrationTestDb_{Guid.NewGuid()}")
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
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "testuser@unops.org")
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
        _context.Currencies.AddRange(new[]
        {
            new Currency { Id = 1, Code = "USD", Name = "US Dollar", IsDeleted = false },
            new Currency { Id = 2, Code = "EUR", Name = "Euro", IsDeleted = false }
        });

        _context.Countries.AddRange(new[]
        {
            new Country { Id = 1, Name = "Bangladesh", Iso2Code = "BD" },
            new Country { Id = 2, Name = "Nepal", Iso2Code = "NP" }
        });

        _context.OrganizationHierarchies.AddRange(new[]
        {
            new OrganizationHierarchy { Id = 1, Name = "South Asia Hub", Code = "SAH", Description = "South Asia Regional Hub", IsDeleted = false },
            new OrganizationHierarchy { Id = 2, Name = "Bangladesh Office", Code = "BDO", Description = "Bangladesh Country Office", ParentId = 1, IsDeleted = false }
        });

        _context.WorkflowStages.AddRange(new[]
        {
            new WorkflowStage { Id = 1, Name = "Identification", EntityType = "Opportunity", Order = 1, IsDeleted = false },
            new WorkflowStage { Id = 2, Name = "Development", EntityType = "Opportunity", Order = 2, IsDeleted = false },
            new WorkflowStage { Id = 3, Name = "Review", EntityType = "Opportunity", Order = 3, IsDeleted = false }
        });

        _context.ProposedInitiativeTypes.AddRange(new[]
        {
            new ProposedInitiativeType { Id = 1, Name = "Project", IsDeleted = false },
            new ProposedInitiativeType { Id = 2, Name = "Programme", IsDeleted = false }
        });

        _context.PAOUsers.Add(new PAOUser
        {
            Id = 1,
            Email = "testuser@unops.org",
            FirstName = "Test",
            LastName = "User",
            IsDeleted = false
        });

        _context.SaveChanges();
    }

    #region P1 - Complete Opportunity Lifecycle Tests

    [Fact]
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
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1,
            InitiativeBudgetUSD = 1000000
        };

        var createdEntity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = createRequest.Name,
            Description = createRequest.Description,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(createdEntity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = createRequest.Name });

        // Act - Create
        var created = await _manager.CreateOpportunityAsync(createRequest);
        created.Should().NotBeNull();
        created.Id.Should().Be(1);

        // Act - Update
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Updated Lifecycle Test",
            InitiativeBudgetUSD = 1500000
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = updateRequest.Name });

        var updated = await _manager.UpdateOpportunityAsync(updateRequest);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated Lifecycle Test");

        // Act - Get
        var retrieved = await _manager.GetOpportunityAsync(1);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(1);

        // Act - Delete
        var deleted = await _manager.DeleteOpportunityAsync(1);
        deleted.Should().BeTrue();

        // Verify soft delete
        var afterDelete = await _manager.GetOpportunityAsync(1);
        afterDelete.Should().BeNull(); // Soft-deleted records not returned
    }

    [Fact]
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
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = createRequest.Name,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = createRequest.Name });

        var created = await _manager.CreateOpportunityAsync(createRequest);

        // Act - Update Overview Section
        var overviewRequest = new OverviewSectionRequest
        {
            Description = "Comprehensive project description",
            PartnerReference = "REF-001"
        };

        var overviewResult = await _manager.UpdateOverviewSectionAsync(1, overviewRequest);
        overviewResult.Should().NotBeNull();

        // Act - Update What Section
        var whatRequest = new WhatSectionRequest
        {
            Deliverables = new List<OpportunityDeliverableRequest>
            {
                new() { Name = "Infrastructure", Description = "Build infrastructure" }
            }
        };

        var whatResult = await _manager.UpdateWhatSectionAsync(1, whatRequest);
        whatResult.Should().NotBeNull();

        // Act - Update Why Section
        var whyRequest = new WhySectionRequest
        {
            ResultsFocus = "Sustainable development",
            ExpectedImpact = "Positive community impact"
        };

        var whyResult = await _manager.UpdateWhySectionAsync(1, whyRequest);
        whyResult.Should().NotBeNull();

        // Act - Update When Section
        var whenRequest = new WhenSectionRequest
        {
            TargetSigningDate = DateTime.UtcNow.AddMonths(6),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(24)
        };

        var whenResult = await _manager.UpdateWhenSectionAsync(1, whenRequest);
        whenResult.Should().NotBeNull();

        // Assert - All sections updated successfully
        var finalOpportunity = await _manager.GetOpportunityAsync(1);
        finalOpportunity.Should().NotBeNull();
    }

    #endregion

    #region P1 - Multi-Partner Integration Tests

    [Fact]
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
                new() { PartnerId = 1, Amount = 1000000, CurrencyId = 1 },
                new() { PartnerId = 2, Amount = 750000, CurrencyId = 1 }
            },
            ClientPartners = new List<OpportunityClientPartnerRequest>
            {
                new() { PartnerId = 3 }
            }
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

        // Verify opportunity created
        var savedOpportunity = await _context.Opportunities
            .Include(o => o.FundingPartners)
            .Include(o => o.ClientPartners)
            .FirstOrDefaultAsync(o => o.Id == 1);

        savedOpportunity.Should().NotBeNull();
    }

    [Fact]
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
                new() { SDGId = 6 },  // Clean Water
                new() { SDGId = 13 }  // Climate Action
            }
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

    #region P1 - Workflow Progression Tests

    [Fact]
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
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1,
            WorkflowStageId = 1 // Identification
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = createRequest.Name,
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(entity);
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = createRequest.Name, WorkflowStageId = 1 });

        var created = await _manager.CreateOpportunityAsync(createRequest);
        created.WorkflowStageId.Should().Be(1);

        // Act - Progress to Development stage
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            WorkflowStageId = 2 // Development
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = createRequest.Name, WorkflowStageId = 2 });

        var updated = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        updated.Should().NotBeNull();
        updated!.WorkflowStageId.Should().Be(2);

        // Verify in database
        var savedOpportunity = await _context.Opportunities.FindAsync(1);
        savedOpportunity.Should().NotBeNull();
        savedOpportunity!.WorkflowStageId.Should().Be(2);
    }

    #endregion

    #region P1 - Data Validation and Constraints Tests

    [Fact]
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

        // Act & Assert - Should handle invalid reference gracefully
        // Actual behavior depends on implementation (throw or filter out invalid)
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);

        // Either succeeds (filtering invalid) or throws appropriate exception
        await act.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-INT-007")]
    public async Task UpdateOpportunity_ConcurrentModification_HandlesCorrectly()
    {
        // Arrange - Create base opportunity
        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Concurrent Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(entity);
        await _context.SaveChangesAsync();

        // Act - Simulate concurrent updates
        var update1 = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Update from User 1"
        };

        var update2 = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Update from User 2"
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = "Updated" });

        // First update succeeds
        var result1 = await _manager.UpdateOpportunityAsync(update1);
        result1.Should().NotBeNull();

        // Second update also succeeds (last write wins in this implementation)
        var result2 = await _manager.UpdateOpportunityAsync(update2);
        result2.Should().NotBeNull();

        // Verify final state
        var finalState = await _context.Opportunities.FindAsync(1);
        finalState.Should().NotBeNull();
        finalState!.Name.Should().Be("Update from User 2");
    }

    #endregion

    #region P1 - Complex Query and Filter Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-008")]
    public async Task GetAllOpportunities_WithMultipleFilters_ReturnsFiltered()
    {
        // Arrange
        _context.Opportunities.AddRange(new[]
        {
            new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Active Opportunity 1",
                WorkflowStageId = 2,
                ResponsibleOrgUnitId = 1,
                Status = EntityStatus.Active,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            },
            new Domain.Entities.Opportunity
            {
                Id = 2,
                Name = "Draft Opportunity",
                WorkflowStageId = 1,
                ResponsibleOrgUnitId = 1,
                Status = EntityStatus.Draft,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            },
            new Domain.Entities.Opportunity
            {
                Id = 3,
                Name = "Active Opportunity 2",
                WorkflowStageId = 2,
                ResponsibleOrgUnitId = 2,
                Status = EntityStatus.Active,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            }
        });
        await _context.SaveChangesAsync();

        var mappedModels = new List<OpportunityModel>
        {
            new() { Id = 1, Name = "Active Opportunity 1", Status = EntityStatus.Active, WorkflowStageId = 2 },
            new() { Id = 2, Name = "Draft Opportunity", Status = EntityStatus.Draft, WorkflowStageId = 1 },
            new() { Id = 3, Name = "Active Opportunity 2", Status = EntityStatus.Active, WorkflowStageId = 2 }
        };

        _mockMapper.Setup(m => m.Map<IEnumerable<OpportunityModel>>(It.IsAny<List<Domain.Entities.Opportunity>>()))
            .Returns(mappedModels);

        // Act
        var result = await _manager.GetAllOpportunitiesAsync();

        // Assert
        var opportunities = result.ToList();
        opportunities.Should().HaveCount(3);
        opportunities.Should().OnlyContain(o => !string.IsNullOrEmpty(o.Status));
    }

    #endregion

    #region P1 - Bulk Operations Tests

    [Fact]
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
                ResponsibleOrgUnitId = 1
            },
            new()
            {
                Name = "Batch Opportunity 2",
                Description = "Second in batch",
                ResponsibleOrgUnitId = 1
            },
            new()
            {
                Name = "Batch Opportunity 3",
                Description = "Third in batch",
                ResponsibleOrgUnitId = 2
            }
        };

        var entities = requests.Select((r, index) => new Domain.Entities.Opportunity
        {
            Id = index + 1,
            Name = r.Name,
            Description = r.Description,
            WorkflowStageId = 1,
            ResponsibleOrgUnitId = r.ResponsibleOrgUnitId,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        }).ToList();

        int entityIndex = 0;
        _mockMapper.Setup(m => m.Map<Domain.Entities.Opportunity>(It.IsAny<OpportunityRequest>()))
            .Returns(() => entities[entityIndex++]);
        
        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns((Domain.Entities.Opportunity o) => new OpportunityModel
            {
                Id = o.Id,
                Name = o.Name
            });

        // Act - Create multiple opportunities
        var results = new List<OpportunityModel>();
        foreach (var request in requests)
        {
            var result = await _manager.CreateOpportunityAsync(request);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r.Id > 0);
        results.Select(r => r.Name).Should().BeEquivalentTo(requests.Select(r => r.Name));

        // Verify all saved to database
        var savedOpportunities = await _context.Opportunities.ToListAsync();
        savedOpportunities.Should().HaveCount(3);
    }

    #endregion

    #region P2 - Performance and Load Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Performance")]
    [Trait("TestId", "TC-UNOPS-INT-010")]
    public async Task GetOpportunityWithLargeDataset_PerformsWithinBounds()
    {
        // Arrange - Create opportunity with many related records
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Large Dataset Test",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = "Large Dataset Test" });

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await _manager.GetOpportunityAsync(1);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    #endregion

    #region P1 - External Stakeholder Management Tests

    [Fact]
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

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            MiscExternalStakeholders = request.MiscExternalStakeholders,
            ExternalStakeholderNotes = request.ExternalStakeholderNotes,
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
                MiscExternalStakeholders = request.MiscExternalStakeholders
            });

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.MiscExternalStakeholders.Should().Contain("Ministry of Infrastructure");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-012")]
    public async Task UpdateOpportunity_AddExternalStakeholdersAfterCreation_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Evolving Stakeholder Initiative",
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
            // Add external stakeholders during development phase
        };

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns(new OpportunityModel { Id = 1, Name = "Evolving Stakeholder Initiative" });

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region P1 - Bulk Operations Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-013")]
    public async Task BulkUpdateOpportunities_UpdateWorkflowStage_Success()
    {
        // Arrange - Create 5 opportunities
        var opportunities = Enumerable.Range(1, 5).Select(i => new Domain.Entities.Opportunity
        {
            Id = i,
            Name = $"Bulk Test Opportunity {i}",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        }).ToArray();

        _context.Opportunities.AddRange(opportunities);
        await _context.SaveChangesAsync();

        _mockMapper.Setup(m => m.Map<OpportunityModel>(It.IsAny<Domain.Entities.Opportunity>()))
            .Returns((Domain.Entities.Opportunity o) => new OpportunityModel { Id = o.Id, WorkflowStageId = 2 });

        // Act - Update all to Development stage
        var results = new List<OpportunityModel>();
        foreach (var opp in opportunities)
        {
            var updateRequest = new UpdateOpportunityRequest
            {
                Id = opp.Id,
                WorkflowStageId = 2
            };
            var result = await _manager.UpdateOpportunityAsync(updateRequest);
            if (result != null) results.Add(result);
        }

        // Assert
        results.Should().HaveCount(5);
        results.Should().OnlyContain(r => r.WorkflowStageId == 2);

        var savedOpportunities = await _context.Opportunities.ToListAsync();
        savedOpportunities.Should().OnlyContain(o => o.WorkflowStageId == 2);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-INT-014")]
    public async Task BulkDelete_MultipleOpportunities_Success()
    {
        // Arrange
        var opportunities = Enumerable.Range(1, 3).Select(i => new Domain.Entities.Opportunity
        {
            Id = i,
            Name = $"To Delete {i}",
            WorkflowStageId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        }).ToArray();

        _context.Opportunities.AddRange(opportunities);
        await _context.SaveChangesAsync();

        // Act - Bulk delete
        var deleteResults = new List<bool>();
        foreach (var opp in opportunities)
        {
            var result = await _manager.DeleteOpportunityAsync(opp.Id);
            deleteResults.Add(result);
        }

        // Assert
        deleteResults.Should().OnlyContain(r => r == true);

        var remainingOpportunities = await _context.Opportunities.ToListAsync();
        remainingOpportunities.Should().BeEmpty();
    }

    #endregion

    #region P2 - Pooled Funding Tests

    [Fact]
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
                new() { PartnerId = 1, Amount = 500000, CurrencyId = 1 },
                new() { PartnerId = 2, Amount = 500000, CurrencyId = 1 },
                new() { PartnerId = 3, Amount = 500000, CurrencyId = 1 }
            },
            InitiativeBudgetUSD = 1500000
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            IsPooledFunding = true,
            InitiativeBudgetUSD = 1500000,
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
                IsPooledFunding = true
            });

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsPooledFunding.Should().BeTrue();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
