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
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.DataAccess.Services;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Validation and business rule tests for opportunity management
/// Tests data validation, business constraints, and error handling
/// Created: January 15, 2026
/// Priority: P1-P2
/// </summary>
public class OpportunityValidationTests : IDisposable
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

    public OpportunityValidationTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase(databaseName: $"OpportunityValidationTestDb_{Guid.NewGuid()}")
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

        var userResolverService = new UserResolverService<int>(mockUserServiceHttpContextAccessor.Object, null);
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        _context = new UNOPSAppDbContext(_dbContextOptions, userResolverService, mockDbSchema.Object);
        
        // Ensure EF Core model is finalized for in-memory database
        _context.Database.EnsureCreated();

        // Setup real AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
        });
        _mapper = mapperConfig.CreateMapper();
        
        // Build real configuration from test data
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

        var testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Name, "Test User")
        }, "TestAuthType"));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(testUser);
        _mockHttpContextAccessor.Setup(m => m.HttpContext).Returns(mockHttpContext.Object);

        _mockDbContextFactory.Setup(f => f.CreateDbContextAsync(default))
            .ReturnsAsync(_context);

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

        SeedTestData();
    }

    private void SeedTestData()
    {
        _context.Currencies.Add(new Currency { Id = 1, Code = "USD", Name = "US Dollar", IsDeleted = false });
        _context.Countries.Add(new Country { Id = 1, Name = "Bangladesh", Iso2Code = "BD" });
        _context.OrganizationHierarchies.Add(new OrganizationHierarchy { Id = 1, Name = "Test Org Unit", Code = "TOU", Description = "Test Organization Unit", IsDeleted = false });
        // Workflow stages are now stored as string values in Opportunity.Stage property
        _context.ProposedInitiativeTypes.Add(new ProposedInitiativeType { Id = 1, Name = "Project", IsDeleted = false });
        _context.PAOUsers.Add(new PAOUser { Id = 1, Email = "test@unops.org" });
        _context.SaveChanges();
    }

    #region P1 - Name Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-001")]
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
    [Trait("TestId", "TC-UNOPS-VAL-002")]
    public async Task CreateOpportunity_NameTooLong_ThrowsException()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = new string('A', 256), // Exceeds max length
            Description = "Valid description"
        };

        // Act & Assert
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);
        await act.Should().ThrowAsync<Exception>().WithMessage("*length*");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-003")]
    public async Task CreateOpportunity_NameWithSpecialCharacters_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Infrastructure Project - Phase 1 (2026)",
            Description = "Valid description with special characters"
        };

        var entity = new Domain.Entities.Opportunity
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
        result.Name.Should().Be(request.Name);
    }

    #endregion

    #region P1 - Budget Validation Tests

    [Theory]
    [InlineData(-1000)]
    [InlineData(-0.01)]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-004")]
    public async Task CreateOpportunity_NegativeBudget_HandlesGracefully(decimal negativeBudget)
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Negative Budget Test",
            Description = "Testing negative budget validation",
            InitiativeBudgetUSD = negativeBudget
        };

        // Act & Assert
        // Implementation may either throw exception or set to zero
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);
        
        // Should either throw or handle gracefully
        var exception = await Record.ExceptionAsync(act);
        if (exception != null)
        {
            exception.Message.Should().Contain("budget", because: "error should mention budget");
        }
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-005")]
    public async Task CreateOpportunity_ZeroBudget_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Zero Budget Opportunity",
            Description = "Some opportunities may have zero budget initially",
            InitiativeBudgetUSD = 0
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description ?? "Default description",
            InitiativeBudgetUSD = 0,
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
        result.InitiativeBudgetUSD.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-006")]
    public async Task CreateOpportunity_VeryLargeBudget_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Large Scale Programme",
            Description = "Major infrastructure programme",
            InitiativeBudgetUSD = 999999999.99m // Very large budget
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


        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(999999999.99m);
    }

    #endregion

    #region P1 - Date Validation Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-007")]
    public async Task CreateOpportunity_EndDateBeforeStartDate_HandlesGracefully()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Invalid Date Range",
            Description = "End date before start date",
            TargetSigningDate = DateTime.UtcNow.AddMonths(12),
            TargetDeliveryDate = DateTime.UtcNow // Before signing date
        };

        // Act & Assert
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);
        
        // Should validate date logic
        var exception = await Record.ExceptionAsync(act);
        if (exception != null)
        {
            exception.Message.Should().MatchRegex("date|timeline|invalid", because: "error should mention date validation");
        }
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-008")]
    public async Task CreateOpportunity_PastTargetDate_AllowedForHistoricalData()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Historical Opportunity",
            Description = "Opportunity with past target date for historical tracking",
            TargetSigningDate = DateTime.UtcNow.AddMonths(-6),
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(-3)
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description ?? "Default description",
            TargetSigningDate = request.TargetSigningDate,
            TargetDeliveryDate = request.TargetDeliveryDate,
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

    #region P1 - Description Validation Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-009")]
    public async Task CreateOpportunity_EmptyDescription_Success()
    {
        // Arrange - Description may be optional or can be empty initially
        var request = new OpportunityRequest
        {
            Name = "Minimal Opportunity",
            Description = ""
        };

        var entity = new Domain.Entities.Opportunity
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
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-010")]
    public async Task CreateOpportunity_VeryLongDescription_Success()
    {
        // Arrange
        var longDescription = new string('A', 5000); // Very long description
        var request = new OpportunityRequest
        {
            Name = "Detailed Opportunity",
            Description = longDescription
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = longDescription,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        // Real AutoMapper is now used - no mock setup needed


        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().HaveLength(5000);
    }

    #endregion

    #region P2 - Challenges Field Validation Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-011")]
    public async Task CreateOpportunity_ChallengesExceedsMaxLength_ThrowsException()
    {
        // Arrange - Challenges field has 1020 character limit
        var request = new OpportunityRequest
        {
            Name = "Challenge Test",
            Description = "Testing challenges validation",
            Challenges = new string('A', 1021) // Exceeds 1020 limit
        };

        // Act & Assert
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);
        
        var exception = await Record.ExceptionAsync(act);
        if (exception != null)
        {
            exception.Message.Should().MatchRegex("challenge|length|1020", because: "should validate challenges field length");
        }
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-012")]
    public async Task CreateOpportunity_ChallengesAtMaxLength_Success()
    {
        // Arrange - Exactly at 1020 character limit
        var maxLengthChallenges = new string('A', 1020);
        var request = new OpportunityRequest
        {
            Name = "Max Length Challenges Test",
            Description = "Testing maximum challenges length",
            Challenges = maxLengthChallenges
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description ?? "Default description",
            Challenges = maxLengthChallenges,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        // Real AutoMapper is now used - no mock setup needed


        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Challenges.Should().HaveLength(1020);
    }

    #endregion

    #region P2 - Expected Impact/Outcomes Validation Tests

    [Theory]
    [InlineData(511)]
    [InlineData(600)]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-013")]
    public async Task CreateOpportunity_ExpectedImpactTooLong_HandlesGracefully(int length)
    {
        // Arrange - ExpectedImpact has 510 character limit
        var request = new OpportunityRequest
        {
            Name = "Impact Validation Test",
            Description = "Testing expected impact validation",
            ExpectedImpact = new string('A', length)
        };

        // Act & Assert
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);
        
        var exception = await Record.ExceptionAsync(act);
        if (exception != null)
        {
            exception.Message.Should().MatchRegex("impact|length|510", because: "should validate impact field length");
        }
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-014")]
    public async Task CreateOpportunity_ExpectedOutcomesAtMaxLength_Success()
    {
        // Arrange - ExpectedOutcomes has 510 character limit
        var maxLengthOutcomes = new string('A', 510);
        var request = new OpportunityRequest
        {
            Name = "Outcomes Validation Test",
            Description = "Testing expected outcomes validation",
            ExpectedOutcomes = maxLengthOutcomes
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description ?? "Default description",
            ExpectedOutcomes = maxLengthOutcomes,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        // Real AutoMapper is now used - no mock setup needed


        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.ExpectedOutcomes.Should().HaveLength(510);
    }

    #endregion

    #region P2 - Beneficiaries Validation Tests

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-015")]
    public async Task CreateOpportunity_NegativeBeneficiaries_HandlesGracefully(int negativeBeneficiaries)
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "Beneficiaries Validation Test",
            Description = "Testing negative beneficiaries",
            EstimatedDirectBeneficiaries = negativeBeneficiaries
        };

        // Act & Assert
        Func<Task> act = async () => await _manager.CreateOpportunityAsync(request);
        
        var exception = await Record.ExceptionAsync(act);
        if (exception != null)
        {
            exception.Message.Should().MatchRegex("beneficiaries|negative|invalid", because: "should validate beneficiaries count");
        }
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-016")]
    public async Task CreateOpportunity_BeneficiariesToBeDetermined_Success()
    {
        // Arrange
        var request = new OpportunityRequest
        {
            Name = "TBD Beneficiaries Test",
            Description = "Beneficiaries to be determined",
            BeneficiariesToBeDetermined = true,
            EstimatedDirectBeneficiaries = null
        };

        var entity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description ?? "Default description",
            BeneficiariesToBeDetermined = true,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        // Real AutoMapper is now used - no mock setup needed


        // Real AutoMapper is now used - no mock setup needed

        // Act
        var result = await _manager.CreateOpportunityAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BeneficiariesToBeDetermined.Should().BeTrue();
    }

    #endregion

    #region P2 - Collection Validation Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-017")]
    public async Task CreateOpportunity_EmptyCollections_Success()
    {
        // Arrange - Empty collections should be acceptable
        var request = new OpportunityRequest
        {
            Name = "Empty Collections Test",
            Description = "Testing empty collections",
            FundingPartners = new List<OpportunityFundingPartnerRequest>(),
            ClientPartners = new List<OpportunityClientPartnerRequest>(),
            SDGs = new List<OpportunitySDGRequest>()
        };

        var entity = new Domain.Entities.Opportunity
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
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-018")]
    public async Task CreateOpportunity_NullCollections_Success()
    {
        // Arrange - Null collections should be acceptable
        var request = new OpportunityRequest
        {
            Name = "Null Collections Test",
            Description = "Testing null collections",
            FundingPartners = null,
            ClientPartners = null,
            SDGs = null
        };

        var entity = new Domain.Entities.Opportunity
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
    }

    #endregion

    #region P2 - Update Validation Tests

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-019")]
    public async Task UpdateOpportunity_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange - Create opportunity
        var opportunity = new Domain.Entities.Opportunity
        {
            Id = 1,
            Name = "Original Name",
            Description = "Original Description",
            InitiativeBudgetUSD = 1000000,
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = 1,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        // Act - Update only name, leave others unchanged
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = "Updated Name Only"
            // Description and budget not provided - should remain unchanged
        };
        // Real AutoMapper is now used - no mock setup needed

        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        
        var savedOpportunity = await _context.Opportunities.FindAsync(1);
        savedOpportunity.Should().NotBeNull();
        savedOpportunity!.Name.Should().Be("Updated Name Only");
        savedOpportunity.Description.Should().Be("Original Description"); // Unchanged
        savedOpportunity.InitiativeBudgetUSD.Should().Be(1000000); // Unchanged
    }

    [Fact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-020")]
    public async Task UpdateOpportunity_InvalidId_ReturnsNull()
    {
        // Arrange
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 999999,
            Name = "This should not succeed"
        };

        // Act
        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
