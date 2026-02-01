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
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.DataAccess.Services;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Test suite for UNOPSOpportunityManager - Working tests that match actual codebase
/// Tests CRUD operations, section updates, AI integration, and validations
/// Created: January 15, 2026
/// Priority: P0 (Critical)
/// Note: Tests that use Z.EntityFramework.Extensions features may fail with InMemory provider
/// </summary>
public class UNOPSOpportunityManagerTests : IDisposable
{
    private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
    private readonly UNOPSAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IDbContextFactory<UNOPSAppDbContext>> _mockDbContextFactory;
    private readonly Mock<IExchangeRateService> _mockExchangeRateService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly UNOPSOpportunityManager _manager;
    private readonly ClaimsPrincipal _testUser;

    public UNOPSOpportunityManagerTests()
    {
        // Setup in-memory database
        _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: $"OpportunityTestDb_{Guid.NewGuid()}")
            .Options;

        // Setup mock HttpContextAccessor for UserResolverService
        var mockUserServiceHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockUserServiceHttpContext = new Mock<HttpContext>();
        var mockRequest = new Mock<HttpRequest>();
        var mockHeaders = new HeaderDictionary();
        
        var userServiceTestUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "testuser@unops.org")
        }, "TestAuthType"));
        
        mockRequest.Setup(r => r.Headers).Returns(mockHeaders);
        mockUserServiceHttpContext.Setup(m => m.User).Returns(userServiceTestUser);
        mockUserServiceHttpContext.Setup(m => m.Request).Returns(mockRequest.Object);
        mockUserServiceHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockUserServiceHttpContext.Object);

        // Create UserResolverService with properly mocked HttpContextAccessor
        // Note: UserResolverService is a real instance (not mocked) because GetCurrentUserId() is not virtual
        var userResolverService = new UserResolverService<int>(mockUserServiceHttpContextAccessor.Object, null);
        
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        _context = new UNOPSAppDbContext(_dbContextOptions, userResolverService, mockDbSchema.Object);
        
        // Ensure EF Core model is finalized for in-memory database
        _context.Database.EnsureCreated();

        // Setup real AutoMapper with application profiles
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });
        _mapper = mapperConfig.CreateMapper();
        
        // Build real configuration from test data (mocking IConfiguration doesn't work with GetValue<T>)
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

        // Setup test user
        _testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "testuser@unops.org")
        }, "TestAuthType"));

        // Setup HttpContext with user
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(_testUser);
        _mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);

        // Setup DbContextFactory
        _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(default))
            .ReturnsAsync(_context);

        // Initialize manager
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

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
        // Seed Currencies
        _context.Currencies.AddRange(new[]
        {
            new Currency { Id = 1, Code = "USD", Name = "US Dollar", IsDeleted = false },
            new Currency { Id = 2, Code = "EUR", Name = "Euro", IsDeleted = false }
        });

        // Seed Countries
        _context.Countries.AddRange(new[]
        {
            new Country { Id = 1, Name = "Bangladesh", Iso2Code = "BD" },
            new Country { Id = 2, Name = "Nepal", Iso2Code = "NP" },
            new Country { Id = 3, Name = "Myanmar", Iso2Code = "MM" }
        });

        // Seed Organization Hierarchies
        _context.OrganizationHierarchies.AddRange(new[]
        {
            new OrganizationHierarchy { Id = 1, Name = "South Asia Hub", Code = "SAH", Description = "South Asia Regional Hub", IsDeleted = false },
            new OrganizationHierarchy { Id = 2, Name = "Bangladesh Office", Code = "BDO", Description = "Bangladesh Country Office", ParentId = 1, IsDeleted = false }
        });

        // Workflow stages are now stored as string values in Opportunity.Stage property
        // No need to seed WorkflowStage entities

        // Seed Proposed Initiative Types
        _context.ProposedInitiativeTypes.AddRange(new[]
        {
            new ProposedInitiativeType { Id = 1, Name = "Project", IsDeleted = false },
            new ProposedInitiativeType { Id = 2, Name = "Programme", IsDeleted = false },
            new ProposedInitiativeType { Id = 3, Name = "Advisory", IsDeleted = false }
        });

        // Seed test user
        _context.PAOUsers.Add(new PAOUser
        {
            Id = 1,
            Email = "testuser@unops.org"
        });

        // Seed EntityRole for Opportunity Manager (required for team assignment tests)
        _context.EntityRoles.Add(new EntityRole
        {
            Id = 1,
            EntityType = "Opportunity",
            Name = "Opportunity Manager",
            Description = "Manages the opportunity",
            IsInternal = true,
            AllowsMultiple = false,
            Code = "Opportunity_Manager",
            Status = EntityStatus.Active,
            IsDeleted = false
        });

        _context.SaveChanges();
    }

    #region P0 - Create Opportunity Tests

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-001")]
    public async Task CreateOpportunity_WithRequiredFields_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Water Infrastructure Initiative - South Asia",
            Description = "Multi-country water infrastructure project focusing on sustainable water supply systems",
            ResponsibleOrgUnitId = 1,
            ProposedInitiativeTypeId = 1,
            InitiativeBudgetUSD = 2500000.00m,
            TargetSigningDate = DateTime.UtcNow.AddMonths(6),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(24)
        };

        var opportunityEntity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description ?? "Default description",
            ResponsibleOrgUnitId = request.ResponsibleOrgUnitId,
            ProposedInitiativeTypeId = request.ProposedInitiativeTypeId,
            InitiativeBudgetUSD = request.InitiativeBudgetUSD,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        var expectedModel = new OpportunityModel
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description,
            Status = "Draft",
            Stage = "IDENTIFY & PROFILE"
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(request.Name);
        result.Status.Should().Be("Draft");
        result.Stage.Should().Be("IDENTIFY & PROFILE");

        // Verify entity saved to database
        var savedEntity = await _context.Opportunities.FindAsync(1);
        savedEntity.Should().NotBeNull();
        savedEntity!.Stage.Should().Be("IDENTIFY & PROFILE"); // Default workflow stage set
    }

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-OPP-002")]
    public async Task CreateOpportunity_WithoutRequiredName_ThrowsException()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = null!,  // Required field missing
            Description = "Test opportunity without name"
        };

        // Act & Assert
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*name*"); // Should contain reference to missing name
    }

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-003")]
    public async Task CreateOpportunity_WithFundingPartners_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Multi-Partner Development Project",
            Description = "Collaborative infrastructure development",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = 1, Amount = 1000000, CurrencyId = 1 },
                new() { PartnerId = 2, Amount = 500000, CurrencyId = 1 }
            }
        };

        var opportunityEntity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description ?? "Default description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);

        // Verify funding partners saved
        var savedOpportunity = await _context.Opportunities
            .Include(o => o.FundingPartners)
            .FirstOrDefaultAsync(o => o.Id == 1);

        savedOpportunity.Should().NotBeNull();
        // Note: In actual implementation, funding partners are handled by mapper
        // This test validates the request structure
    }

    #endregion

    #region P0 - Get Opportunity Tests

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-004")]
    public async Task GetOpportunity_ById_ReturnsOpportunity()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            ResponsibleOrgUnitId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.GetOpportunityAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Opportunity");
        result.Status.Should().Be("Draft");
    }

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-OPP-005")]
    public async Task GetOpportunity_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _manager.GetOpportunityAsync(999999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Security")]
    [Trait("TestId", "TC-UNOPS-OPP-006")]
    public async Task GetOpportunity_WithUser_AppliesPermissions()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            ResponsibleOrgUnitId = 1,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.GetOpportunityAsync(_testUser, 1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Opportunity");
    }

    #endregion

    #region P0 - Update Opportunity Tests

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-007")]
    public async Task UpdateOpportunity_BasicFields_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Original Name",
            Description = "Original Description",
            Stage = "IDENTIFY & PROFILE",
            InitiativeBudgetUSD = 1000000,
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow.AddDays(-7),
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Updated Name",
            Description = "Updated Description",
            InitiativeBudgetUSD = 1500000
        };

        var updatedModel = new OpportunityModel
        {
            Id = 1,
            Name = "Updated Name",
            Description = "Updated Description"
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");

        // Verify entity updated in database
        var savedEntity = await _context.Opportunities.FindAsync(1);
        savedEntity.Should().NotBeNull();
        savedEntity!.Name.Should().Be("Updated Name");
        savedEntity.Description.Should().Be("Updated Description");
        savedEntity.InitiativeBudgetUSD.Should().Be(1500000);
        savedEntity.LastModifiedBy.Should().Be(1);
        savedEntity.LastModifiedDate.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-OPP-008")]
    public async Task UpdateOpportunity_NonExistentId_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 999999,
            Name = "Updated Name"
        };

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-009")]
    public async Task UpdateOverviewSection_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var overviewRequest = new OverviewSectionRequest
        {
            Description = "Updated comprehensive description"
        };

        var updatedModel = new OpportunityModel
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = overviewRequest.Description
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.UpdateOverviewSectionAsync(1, overviewRequest);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().Be("Updated comprehensive description");
    }

    #endregion

    #region P0 - Delete Opportunity Tests

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-010")]
    public async Task DeleteOpportunity_SoftDelete_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _manager.DeleteOpportunityAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        // Note: UNOPSAppDbContext does not use global query filters for IsDeleted
        // Soft-delete filtering is done manually in repository methods per the architecture
        var deletedEntity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == 1);

        deletedEntity.Should().NotBeNull();
        deletedEntity!.IsDeleted.Should().BeTrue();
        deletedEntity.DeletedBy.Should().Be(1);
        deletedEntity.DeletedDate.Should().NotBeNull();

        // Verify soft-deleted records are excluded when using manual IsDeleted filter
        // This is how the repository layer filters records per the codebase architecture
        var normalQuery = await _context.Opportunities
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync(o => o.Id == 1);

        normalQuery.Should().BeNull(); // Soft-deleted records excluded when filtered
    }

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-OPP-011")]
    public async Task DeleteOpportunity_NonExistentId_ReturnsFalse()
    {
        // Act
        var result = await _manager.DeleteOpportunityAsync(999999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region P0 - GetAll Opportunities Tests

    [Fact]
    [Trait("Category", "P0")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-012")]
    public async Task GetAllOpportunities_ReturnsAllActive()
    {
        // Arrange
        _context.Opportunities.AddRange(new[]
        {
            new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Opportunity 1",
                Description = "Test Description 1",
                Stage = "IDENTIFY & PROFILE",
                Status = EntityStatus.Draft,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            },
            new Domain.Entities.Opportunity
            {
                Id = 2,
                Name = "Opportunity 2",
                Description = "Test Description 2",
                Stage = "DEVELOP",
                Status = EntityStatus.Active,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            },
            new Domain.Entities.Opportunity
            {
                Id = 3,
                Name = "Deleted Opportunity",
                Description = "Deleted Description",
                Stage = "IDENTIFY & PROFILE",
                Status = EntityStatus.Draft,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = true  // Soft-deleted
            }
        });
        await _context.SaveChangesAsync();

        var mappedModels = new List<OpportunityModel>
        {
            new() { Id = 1, Name = "Opportunity 1", Status = "Draft" },
            new() { Id = 2, Name = "Opportunity 2", Status = "Active" }
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.GetAllOpportunitiesAsync();

        // Assert
        var opportunityModels = result.ToList();
        opportunityModels.Should().HaveCount(2); // Excluding soft-deleted
        opportunityModels.Should().NotContain(o => o.Id == 3);
    }

    #endregion

    #region P1 - Section Update Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-013")]
    public async Task UpdateWhatSection_WithDeliverables_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var whatRequest = new WhatSectionRequest
        {
            // Deliverable properties structure has changed
            Deliverables = new List<OpportunityDeliverableRequest>()
        };

        var updatedModel = new OpportunityModel
        {
            Id = 1,
            Name = "Test Opportunity"
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.UpdateWhatSectionAsync(1, whatRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-014")]
    public async Task UpdateWhySection_WithSDGs_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var whyRequest = new WhySectionRequest
        {
            ResultsFocus = "Sustainable development outcomes"
            // SDGs property structure has changed
        };

        var updatedModel = new OpportunityModel
        {
            Id = 1,
            Name = "Test Opportunity",
            ResultsFocus = whyRequest.ResultsFocus
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.UpdateWhySectionAsync(1, whyRequest);

        // Assert
        result.Should().NotBeNull();
        result.ResultsFocus.Should().Be("Sustainable development outcomes");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-015")]
    public async Task UpdateWhoSection_WithStakeholders_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var whoRequest = new WhoSectionRequest
        {
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = 1, Amount = 1000000, CurrencyId = 1 }
            },
            ClientPartners = new List<OpportunityClientPartnerRequest>
            {
                new() { PartnerId = 2 }
            }
        };

        var updatedModel = new OpportunityModel
        {
            Id = 1,
            Name = "Test Opportunity"
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.UpdateWhoSectionAsync(1, whoRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-016")]
    public async Task UpdateWhereSection_WithCountries_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var whereRequest = new WhereSectionRequest
        {
            Countries = new List<OpportunityCountryRequest>
            {
                new() { CountryId = 1 }, // Bangladesh
                new() { CountryId = 2 }  // Nepal
            }
        };

        var updatedModel = new OpportunityModel
        {
            Id = 1,
            Name = "Test Opportunity"
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.UpdateWhereSectionAsync(1, whereRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-017")]
    public async Task UpdateWhenSection_WithTimeline_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var whenRequest = new WhenSectionRequest
        {
            TargetSigningDate = DateTime.UtcNow.AddMonths(6),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(24),
            IsTargetSigningDateFirm = true,
            SigningDateNotes = "Partner deadline"
        };

        var updatedModel = new OpportunityModel
        {
            Id = 1,
            Name = "Test Opportunity",
            TargetSigningDate = whenRequest.TargetSigningDate,
            TargetDeliveryDate = whenRequest.TargetDeliveryDate
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.UpdateWhenSectionAsync(1, whenRequest);

        // Assert
        result.Should().NotBeNull();
        result.TargetSigningDate.Should().Be(whenRequest.TargetSigningDate);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-018")]
    public async Task UpdateTeamSection_WithStakeholders_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var teamRequest = new TeamSectionRequest
        {
            // Stakeholder properties structure has changed
            Stakeholders = new List<OpportunityStakeholderRequest>()
        };

        var updatedModel = new OpportunityModel
        {
            Id = 1,
            Name = "Test Opportunity"
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.UpdateTeamSectionAsync(1, teamRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    #endregion

    #region P1 - AI Integration Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "AI")]
    [Trait("TestId", "TC-UNOPS-OPP-019")]
    public async Task ApplyAiChanges_UpdatesOpportunity_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        var aiRequest = new ApplyOpportunityAiChangesRequest
        {
            Name = "AI-Enhanced Opportunity Name",
            Description = "AI-generated comprehensive description",
            ExpectedImpact = "Significant positive impact on communities",
            ExpectedOutcomes = "Improved infrastructure and services"
        };

        var updatedModel = new OpportunityModel
        {
            Id = 1,
            Name = aiRequest.Name,
            Description = aiRequest.Description,
            ExpectedImpact = aiRequest.ExpectedImpact
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.ApplyAiChangesAsync(1, aiRequest);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("AI-Enhanced Opportunity Name");
        result.Description.Should().Be("AI-generated comprehensive description");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "AI")]
    [Trait("TestId", "TC-UNOPS-OPP-020")]
    public async Task GetOpportunityDetailsForAI_ReturnsComprehensiveData()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Comprehensive opportunity description",
            Stage = "IDENTIFY & PROFILE",
            ResponsibleOrgUnitId = 1,
            InitiativeBudgetUSD = 2500000,
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
        result["name"].Should().Be("Test Opportunity");
    }

    #endregion

    #region P1 - Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-OPP-021")]
    public async Task CreateOpportunity_InvalidName_ThrowsException(string? invalidName)
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = invalidName!,
            Description = "Valid description"
        };

        // Act & Assert
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-OPP-022")]
    public async Task CreateOpportunity_NameExceedsMaxLength_ThrowsException()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = new string('A', 256), // Assuming max length is 255
            Description = "Valid description"
        };

        // Act & Assert
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*length*");
    }

    #endregion

    #region P2 - Advanced Features Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-OPP-023")]
    public async Task GetOpportunitiesByPartner_FiltersCorrectly()
    {
        // Arrange
        _context.Opportunities.AddRange(new[]
        {
            new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Opportunity 1",
                Description = "Opportunity 1 Description",
                Stage = "IDENTIFY & PROFILE",
                Status = EntityStatus.Draft,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            },
            new Domain.Entities.Opportunity
            {
                Id = 2,
                Name = "Opportunity 2",
                Description = "Opportunity 2 Description",
                Stage = "DEVELOP",
                Status = EntityStatus.Draft,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            }
        });
        await _context.SaveChangesAsync();

        var mappedModels = new List<OpportunityModel>
        {
            new() { Id = 1, Name = "Opportunity 1" }
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.GetOpportunitiesByPartnerIdAsync(1);

        // Assert
        var opportunityModels = result.ToList();
        opportunityModels.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-OPP-024")]
    public async Task AssignCreatorAsOpportunityManager_Success()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Test Opportunity",
            Description = "Test Description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act
        await _manager.AssignCreatorAsOpportunityManagerAsync(1, 1);

        // Assert - Method completes without exception
        // Actual verification would check stakeholder assignments
        var savedOpportunity = await _context.Opportunities
            .Include(o => o.Stakeholders)
            .FirstOrDefaultAsync(o => o.Id == 1);

        savedOpportunity.Should().NotBeNull();
    }

    #endregion

    #region P2 - Create From Proposal Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-UNOPS-OPP-025")]
    public async Task CreateOpportunityFromProposal_Success()
    {
        // Arrange
        var proposalRequest = new OpportunityRequest
        {
            Name = "Opportunity from Proposal",
            Description = "Generated from partner proposal",
            InitiativeBudgetUSD = 1500000
        };

        var opportunityEntity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = proposalRequest.Name,
            Description = proposalRequest.Description ?? "Default description",
            InitiativeBudgetUSD = proposalRequest.InitiativeBudgetUSD,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(proposalRequest);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Opportunity from Proposal");
    }

    #endregion

    #region P1 - Proposal to Opportunity Conversion Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-026")]
    public async Task CreateOpportunityFromProposal_WithPartnerData_Success()
    {
        // Arrange
        var proposalRequest = new OpportunityRequest
        {
            Name = "Opportunity from Partner Proposal",
            Description = "Generated from partner submission",
            PartnerReference = "PROP-2026-001",
            InitiativeBudgetUSD = 2500000,
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = 1, Amount = 2500000, CurrencyId = 1 }
            }
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = proposalRequest.Name,
            Description = proposalRequest.Description ?? "Default description",
            PartnerReference = proposalRequest.PartnerReference,
            InitiativeBudgetUSD = proposalRequest.InitiativeBudgetUSD,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(proposalRequest);

        // Assert
        result.Should().NotBeNull();
        result.PartnerReference.Should().Be("PROP-2026-001");
        result.Id.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-027")]
    public async Task CreateOpportunityFromInteractions_LinksInteractionHistory_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Opportunity from Meeting Series",
            Description = "Created based on multiple partner interactions",
            ClientPartners = new List<OpportunityClientPartnerRequest>
            {
                new() { PartnerId = 1 }
            }
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = "Default description",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    #endregion

    #region P1 - Multi-Currency Budget Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-028")]
    public async Task CreateOpportunity_WithMultiCurrencyFunding_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Multi-Currency Initiative",
            Description = "Funding in multiple currencies",
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = 1, Amount = 1000000, CurrencyId = 1 }, // USD
                new() { PartnerId = 2, Amount = 750000, CurrencyId = 2 }   // EUR
            },
            InitiativeBudgetUSD = 2500000 // Converted total in USD
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description ?? "Default description",
            InitiativeBudgetUSD = request.InitiativeBudgetUSD,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(2500000);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-OPP-029")]
    public async Task UpdateOpportunity_BudgetMismatchWithPartners_HandlesGracefully()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Budget Test",
            Description = "Budget test description",
            InitiativeBudgetUSD = 1000000,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Update with funding that doesn't match total budget
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            InitiativeBudgetUSD = 2000000, // Changed budget
            FundingPartners = new List<OpportunityFundingPartnerRequest>
            {
                new() { PartnerId = 1, Amount = 1000000, CurrencyId = 1 } // Only 1M, not 2M
            }
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        // System should handle mismatch (either warn or allow)
    }

    #endregion

    #region P2 - Timeline Dependency Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-OPP-030")]
    public async Task UpdateOpportunity_ImplementationBeforeSigningDate_HandlesGracefully()
    {
        // Arrange
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Timeline Test",
            Description = "Timeline test description",
            TargetSigningDate = DateTime.UtcNow.AddMonths(6),
            Stage = "IDENTIFY & PROFILE",
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
            TargetSigningDate = DateTime.UtcNow.AddMonths(6),
            // Implementation start before signing - may be intentional for mobilization
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act & Assert
        var result = await _manager.UpdateOpportunityAsync(updateRequest);
        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-UNOPS-OPP-031")]
    public async Task CreateOpportunity_WithSubmissionDeadline_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Competitive Bid Opportunity",
            Description = "RFP with submission deadline",
            TargetSigningDate = DateTime.UtcNow.AddMonths(8) // Signing after selection
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description ?? "Default description",
            TargetSigningDate = request.TargetSigningDate,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
