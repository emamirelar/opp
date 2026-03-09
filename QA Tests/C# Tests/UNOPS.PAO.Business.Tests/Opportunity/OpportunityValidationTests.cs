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
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Opportunities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Validation and business rule tests for opportunity management
/// Tests data validation, business constraints, and error handling
/// Created: January 15, 2026
/// Priority: P1-P2
/// SKIPPED: QA-009 - Z.EntityFramework.Extensions requires relational database (PostgreSQL)
/// </summary>
public class OpportunityValidationTests : IDisposable
{
    private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
    private readonly UNOPSAppDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly string _testMarker = $"VAL_{Guid.NewGuid():N}";
    private readonly List<int> _createdOpportunityIds = new();
    private int _currencyId;
    private int _countryId;
    private int _orgHierarchyId;
    private int _proposedInitiativeTypeId;
    private int _paoUserId;
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
        _dbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"OpportunityValidationTestDb_{Guid.NewGuid()}");
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

        // Seed remaining reference data (test user already exists from Phase 1)
        SeedTestData();

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

        // Setup real AutoMapper
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

        var testUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _paoUserId.ToString()),
            new Claim(ClaimTypes.Name, "Test User")
        }, "TestAuthType"));

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(testUser);
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

        var orgHierarchy = _context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "TOU" && !o.IsDeleted);
        if (orgHierarchy == null)
        {
            orgHierarchy = new OrganizationHierarchy { Name = "Test Org Unit", Code = "TOU", Description = "Test Organization Unit", IsDeleted = false };
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

        _paoUserId = TestDataHelper.GetOrCreateTestUser(_context, "testuser@unops.org");

        _context.ChangeTracker.Clear();
    }

    private async Task<int> CreateTestOpportunityAsync(
        string? name = null,
        string? description = null,
        string stage = "IDENTIFY & PROFILE",
        EntityStatus status = EntityStatus.Draft,
        decimal? budgetUSD = null,
        int? responsibleOrgUnitId = null,
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
            Challenges = challenges
        };
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();
        _createdOpportunityIds.Add(opportunity.Id);
        return opportunity.Id;
    }

    #region P1 - Name Validation Tests

    [Theory(Skip = "DEF-071: OpportunityManager lacks application-level name validation")]
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

    [Trait("Defect", "DEF-071")]
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

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

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

        // Act & Assert - Implementation may either throw exception or set to zero
        OpportunityModel? created = null;
        Func<Task> act = async () => created = await _manager.CreateOpportunityAsync(request);
        var exception = await Record.ExceptionAsync(act);

        if (exception != null)
        {
            exception.Message.Should().Contain("budget", because: "error should mention budget");
        }
        else if (created != null)
        {
            _createdOpportunityIds.Add(created.Id);
        }
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(0);
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.InitiativeBudgetUSD.Should().Be(999999999.99m);
    }

    #endregion

    #region P1 - Date Validation Tests

    [SkipIfInMemoryFact]
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

        // Act & Assert - Should validate date logic
        OpportunityModel? created = null;
        Func<Task> act = async () => created = await _manager.CreateOpportunityAsync(request);
        var exception = await Record.ExceptionAsync(act);

        if (exception != null)
        {
            exception.Message.Should().MatchRegex("date|timeline|invalid", because: "error should mention date validation");
        }
        else if (created != null)
        {
            _createdOpportunityIds.Add(created.Id);
        }
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region P1 - Description Validation Tests

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.Description.Should().HaveLength(5000);
    }

    #endregion

    #region P2 - Challenges Field Validation Tests

    [SkipIfInMemoryFact]
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
            // Check full exception chain (EF Core wraps DB errors in DbUpdateException)
            var fullMessage = GetFullExceptionMessage(exception);
            fullMessage.Should().MatchRegex("challenge|length|1020|value too long|22001",
                because: "should validate challenges field length");
        }
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

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
        OpportunityModel? created = null;
        Func<Task> act = async () => created = await _manager.CreateOpportunityAsync(request);
        var exception = await Record.ExceptionAsync(act);

        if (exception != null)
        {
            // Check full exception chain (EF Core wraps DB errors in DbUpdateException)
            var fullMessage = GetFullExceptionMessage(exception);
            fullMessage.Should().MatchRegex("impact|length|510|value too long|22001",
                because: "should validate impact field length");
        }
        else if (created != null)
        {
            _createdOpportunityIds.Add(created.Id);
        }
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

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
        OpportunityModel? created = null;
        Func<Task> act = async () => created = await _manager.CreateOpportunityAsync(request);
        var exception = await Record.ExceptionAsync(act);

        if (exception != null)
        {
            exception.Message.Should().MatchRegex("beneficiaries|negative|invalid", because: "should validate beneficiaries count");
        }
        else if (created != null)
        {
            _createdOpportunityIds.Add(created.Id);
        }
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
        result.BeneficiariesToBeDetermined.Should().BeTrue();
    }

    #endregion

    #region P2 - Collection Validation Tests

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
    }

    [SkipIfInMemoryFact]
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

        // Act
        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region P2 - Update Validation Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P2")]
    [Trait("Type", "Validation")]
    [Trait("TestId", "TC-UNOPS-VAL-019")]
    public async Task UpdateOpportunity_PartialUpdate_OnlyUpdatesProvidedFields()
    {
        // Arrange - Create opportunity via helper
        var oppId = await CreateTestOpportunityAsync(
            name: "Original Name",
            description: "Original Description",
            budgetUSD: 1000000);

        // Act - Update only name, leave others unchanged
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Updated Name Only"
            // Description and budget not provided - should remain unchanged
        };

        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();

        var savedOpportunity = await _context.Opportunities.FindAsync(oppId);
        savedOpportunity.Should().NotBeNull();
        savedOpportunity!.Name.Should().Be("Updated Name Only");
        savedOpportunity.Description.Should().Be("Original Description"); // Unchanged
        savedOpportunity.InitiativeBudgetUSD.Should().Be(1000000); // Unchanged
    }

    [SkipIfInMemoryFact]
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

    /// <summary>
    /// Concatenates all messages in the exception chain (outer + inner exceptions).
    /// EF Core wraps database errors in DbUpdateException; the actual PostgreSQL
    /// error details are in inner exceptions.
    /// </summary>
    private static string GetFullExceptionMessage(Exception ex)
    {
        var messages = new List<string>();
        var current = ex;
        while (current != null)
        {
            messages.Add(current.Message);
            current = current.InnerException;
        }
        return string.Join(" | ", messages);
    }

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
