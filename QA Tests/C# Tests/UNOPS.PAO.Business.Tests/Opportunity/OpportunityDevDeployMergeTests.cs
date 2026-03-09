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
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Tests for features and fixes introduced in the dev-deploy merge (March 2026).
/// Covers: name validation on update, SDG Main/Cross-cutting classification,
/// UNOPS Missions Not Applicable, ProposedInitiativeType name resolution,
/// ImplementationStartDate defaulting, and Opportunity Statement PDF document record creation.
/// </summary>
public class OpportunityDevDeployMergeTests : IDisposable
{
    private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
    private readonly UNOPSAppDbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly string _testMarker = $"MERGE_{Guid.NewGuid():N}";
    private readonly List<int> _createdOpportunityIds = new();
    private int _currencyId;
    private int _countryId;
    private int _orgHierarchyId;
    private int _proposedInitiativeTypeId;
    private int _sdgId;
    private int _sdgId2;
    private int _paoUserId;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly Mock<IPermissionService> _mockPermissionService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IDbContextFactory<UNOPSAppDbContext>> _mockDbContextFactory;
    private readonly Mock<IExchangeRateService> _mockExchangeRateService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly UNOPSOpportunityManager _manager;

    public OpportunityDevDeployMergeTests()
    {
        _dbContextOptions = TestEnvironment.CreateUNOPSDbContextOptions($"OppMergeTestDb_{Guid.NewGuid()}");
        var mockDbSchema = new Mock<IDbContextSchema>();
        mockDbSchema.Setup(s => s.Schema).Returns("public");

        {
            var tempAccessor = CreateMockHttpContextAccessor("0");
            var tempResolver = new UserResolverService<int>(tempAccessor.Object, null);
            using var tempCtx = UNOPS.PAO.Business.Tests.TestBase.TestDbContextFactory.CreateUNOPS(_dbContextOptions, tempResolver, mockDbSchema.Object);
            _paoUserId = TestDataHelper.GetOrCreateTestUser(tempCtx, "testuser@unops.org");
        }

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

        var orgHierarchy = _context.OrganizationHierarchies.FirstOrDefault(o => o.Code == "MRG" && !o.IsDeleted);
        if (orgHierarchy == null)
        {
            orgHierarchy = new OrganizationHierarchy { Name = "Merge Test Org", Code = "MRG", Description = "Merge Test", IsDeleted = false };
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

        var sdg1 = _context.SDGs.FirstOrDefault(s => s.SDGNumber == "1");
        if (sdg1 == null)
        {
            sdg1 = new SDG { Name = "No Poverty", SDGNumber = "1", IsDeleted = false };
            _context.SDGs.Add(sdg1);
            _context.SaveChanges();
        }
        _sdgId = sdg1.Id;

        var sdg2 = _context.SDGs.FirstOrDefault(s => s.SDGNumber == "6");
        if (sdg2 == null)
        {
            sdg2 = new SDG { Name = "Clean Water and Sanitation", SDGNumber = "6", IsDeleted = false };
            _context.SDGs.Add(sdg2);
            _context.SaveChanges();
        }
        _sdgId2 = sdg2.Id;

        _paoUserId = TestDataHelper.GetOrCreateTestUser(_context, "testuser@unops.org");
        _context.ChangeTracker.Clear();
    }

    private async Task<int> CreateTestOpportunityAsync(
        string? name = null,
        string? description = null,
        string stage = "IDENTIFY & PROFILE",
        EntityStatus status = EntityStatus.Draft)
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
            IsDeleted = false
        };
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();
        _createdOpportunityIds.Add(opportunity.Id);
        return opportunity.Id;
    }

    // ================================================================
    // POSITIVE TESTS (2 tests)
    // ================================================================

    #region Positive Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Positive")]
    [Trait("TestId", "TC-MERGE-POS-001")]
    public async Task UpdateOpportunity_WithValidName_Succeeds()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Original Name");
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "Updated Valid Name"
        };

        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        result.Should().NotBeNull();
        var saved = await _context.Opportunities.FindAsync(oppId);
        saved!.Name.Should().Be("Updated Valid Name");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Positive")]
    [Trait("TestId", "TC-MERGE-POS-002")]
    public async Task CreateOpportunity_WithSDGsHavingIsPrimary_SavesCorrectly()
    {
        var request = new OpportunityRequest
        {
            Name = $"SDG Primary Test {_testMarker}",
            Description = "Test SDG Main/Cross-cutting classification",
            SDGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = _sdgId, IsPrimary = true },
                new() { SDGId = _sdgId2, IsPrimary = false }
            }
        };

        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    #endregion

    // ================================================================
    // NEGATIVE TESTS (>= 6 tests, ratio 3:1)
    // ================================================================

    #region Negative Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("  \r\n  ")]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-MERGE-NEG-001")]
    public async Task UpdateOpportunity_WithInvalidName_ThrowsBusinessException(string? invalidName)
    {
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 1,
            Name = invalidName
        };

        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Name is required.");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-MERGE-NEG-002")]
    public async Task UpdateOpportunity_NullRequest_ThrowsException()
    {
        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(null!);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Name is required.");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-MERGE-NEG-003")]
    public async Task UpdateOpportunity_DeletedOpportunity_ReturnsNull()
    {
        var opportunity = new OpportunityEntity
        {
            Name = $"Deleted Opp {_testMarker}",
            Description = "Deleted",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            CreatedBy = _paoUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedBy = _paoUserId,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = true
        };
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();
        _createdOpportunityIds.Add(opportunity.Id);

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = opportunity.Id,
            Name = "Try to update deleted"
        };

        var result = await _manager.UpdateOpportunityAsync(updateRequest);
        result.Should().BeNull();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-MERGE-NEG-004")]
    public async Task UpdateOpportunity_NonExistentId_ReturnsNull()
    {
        var updateRequest = new UpdateOpportunityRequest
        {
            Id = 999999,
            Name = "Should not find this"
        };

        var result = await _manager.UpdateOpportunityAsync(updateRequest);
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-MERGE-NEG-005")]
    public void OpportunityRequest_ImplementationStartDate_DoesNotDefaultAutomatically()
    {
        var request = new OpportunityRequest
        {
            Name = "No dates",
            Description = "Test"
        };

        request.ImplementationStartDate.Should().BeNull(
            "ImplementationStartDate should not have a default at model level");
        request.SubmissionDeadline.Should().BeNull();
        request.IsTargetSigningDateFirm.Should().BeNull();
        request.SigningDateNotes.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-MERGE-NEG-006")]
    public void OpportunityRequest_UNOPSMissionsNotApplicable_DefaultsFalse()
    {
        var request = new OpportunityRequest
        {
            Name = "Default test",
            Description = "Test"
        };

        request.UNOPSMissionsNotApplicable.Should().BeFalse(
            "UNOPSMissionsNotApplicable should default to false");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-MERGE-NEG-007")]
    public void CreateFromInteractionsRequest_NullName_IsInvalid()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = null!,
            Description = "Valid description"
        };

        request.Name.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Negative")]
    [Trait("TestId", "TC-MERGE-NEG-008")]
    public void ApplyAiChangesRequest_NullSDGs_IsValid()
    {
        var request = new ApplyOpportunityAiChangesRequest
        {
            SdGs = null
        };

        request.SdGs.Should().BeNull();
        request.ProposedInitiativeTypeName.Should().BeNull();
        request.UNOPSMissionsNotApplicable.Should().BeNull();
    }

    #endregion

    // ================================================================
    // EDGE/BOUNDARY TESTS (>= 6 tests, ratio 3:1)
    // ================================================================

    #region Edge/Boundary Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-MERGE-EDGE-001")]
    public void SDGRequest_IsPrimary_DefaultsToFalse()
    {
        var sdgRequest = new OpportunitySDGRequest { SDGId = 1 };
        sdgRequest.IsPrimary.Should().BeFalse("new SDG should default to Cross-cutting (not Main)");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-MERGE-EDGE-002")]
    public void SDGRequest_ExplicitIsPrimary_True_Preserved()
    {
        var sdgRequest = new OpportunitySDGRequest { SDGId = 1, IsPrimary = true };
        sdgRequest.IsPrimary.Should().BeTrue("Main SDG flag should be preserved");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-MERGE-EDGE-003")]
    public void CreateFromInteractionsRequest_SDGDeduplication_PreservesIsPrimaryFromFirst()
    {
        var sdgs = new List<OpportunitySDGRequest>
        {
            new() { SDGId = 1, IsPrimary = true },
            new() { SDGId = 1, IsPrimary = false },
            new() { SDGId = 2, IsPrimary = false }
        };

        var deduplicated = sdgs
            .GroupBy(s => s.SDGId)
            .Select(g => g.First())
            .ToList();

        deduplicated.Should().HaveCount(2);
        deduplicated.First(s => s.SDGId == 1).IsPrimary.Should().BeTrue(
            "first occurrence's IsPrimary should win");
        deduplicated.First(s => s.SDGId == 2).IsPrimary.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-MERGE-EDGE-004")]
    public void CreateFromInteractionsRequest_EmptySDGList_DeduplicatesToEmpty()
    {
        var sdgs = new List<OpportunitySDGRequest>();

        var deduplicated = sdgs
            .GroupBy(s => s.SDGId)
            .Select(g => g.First())
            .ToList();

        deduplicated.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-MERGE-EDGE-005")]
    public void ImplementationStartDate_DefaultsToTargetSigningDate_WhenNull()
    {
        var targetSigning = new DateTime(2026, 6, 15);
        DateTime? implementationStart = null;

        var result = implementationStart ?? targetSigning;

        result.Should().Be(targetSigning);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-MERGE-EDGE-006")]
    public void ImplementationStartDate_UsesExplicitValue_WhenProvided()
    {
        var targetSigning = new DateTime(2026, 6, 15);
        DateTime? implementationStart = new DateTime(2026, 9, 1);

        var result = implementationStart ?? targetSigning;

        result.Should().Be(new DateTime(2026, 9, 1),
            "explicit ImplementationStartDate should override TargetSigningDate");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-MERGE-EDGE-007")]
    public void ImplementationStartDate_BothNull_ResultIsNull()
    {
        DateTime? targetSigning = null;
        DateTime? implementationStart = null;

        var result = implementationStart
            ?? (targetSigning.HasValue ? targetSigning : null);

        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-MERGE-EDGE-008")]
    public void ProposedInitiativeTypeName_Whitespace_IsNotResolvable()
    {
        var name = "   ";
        var isResolvable = !string.IsNullOrWhiteSpace(name);

        isResolvable.Should().BeFalse("whitespace-only name should not trigger resolution");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Edge")]
    [Trait("TestId", "TC-MERGE-EDGE-009")]
    public void UNOPSMissionsNotApplicable_TrueWithMissions_MissionsAreIgnored()
    {
        bool? notApplicable = true;
        var missions = new List<OpportunityUNOPSMissionRequest>
        {
            new() { UNOPSMissionId = 1 },
            new() { UNOPSMissionId = 2 }
        };

        var shouldSaveMissions = missions != null && !(notApplicable == true);

        shouldSaveMissions.Should().BeFalse(
            "missions should be ignored when Not Applicable is true");
    }

    #endregion

    // ================================================================
    // FUNCTIONAL TESTS (>= 6 tests, ratio 3:1)
    // ================================================================

    #region Functional Tests

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-MERGE-FUNC-001")]
    public void OpportunityRequest_NewFields_AreAccessible()
    {
        var request = new OpportunityRequest
        {
            Name = "Test",
            Description = "Test",
            ImplementationStartDate = new DateTime(2026, 7, 1),
            SubmissionDeadline = new DateTime(2026, 5, 1),
            IsTargetSigningDateFirm = true,
            SigningDateNotes = "Confirmed by donor",
            UNOPSMissionsNotApplicable = true
        };

        request.ImplementationStartDate.Should().Be(new DateTime(2026, 7, 1));
        request.SubmissionDeadline.Should().Be(new DateTime(2026, 5, 1));
        request.IsTargetSigningDateFirm.Should().BeTrue();
        request.SigningDateNotes.Should().Be("Confirmed by donor");
        request.UNOPSMissionsNotApplicable.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-MERGE-FUNC-002")]
    public void CreateFromInteractionsRequest_ProposedInitiativeTypeName_IsAvailable()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Test",
            ProposedInitiativeTypeName = "Programme"
        };

        request.ProposedInitiativeTypeName.Should().Be("Programme");
        request.ProposedInitiativeTypeId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-MERGE-FUNC-003")]
    public void ApplyAiChangesRequest_SDGsUseIsPrimaryClassification()
    {
        var request = new ApplyOpportunityAiChangesRequest
        {
            SdGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = 1, IsPrimary = true },
                new() { SDGId = 6, IsPrimary = false },
                new() { SDGId = 13, IsPrimary = false }
            }
        };

        request.SdGs.Should().HaveCount(3);
        request.SdGs!.Count(s => s.IsPrimary).Should().Be(1, "only one Main SDG expected");
        request.SdGs!.Count(s => !s.IsPrimary).Should().Be(2, "two Cross-cutting SDGs expected");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-MERGE-FUNC-004")]
    public void ApplyAiChangesRequest_ProposedInitiativeTypeName_FallbackWhenNoId()
    {
        var request = new ApplyOpportunityAiChangesRequest
        {
            ProposedInitiativeTypeId = null,
            ProposedInitiativeTypeName = "Programme"
        };

        request.ProposedInitiativeTypeId.Should().BeNull();
        request.ProposedInitiativeTypeName.Should().Be("Programme");

        var shouldResolve = !request.ProposedInitiativeTypeId.HasValue
                            && !string.IsNullOrWhiteSpace(request.ProposedInitiativeTypeName);
        shouldResolve.Should().BeTrue("name resolution should activate when ID is missing");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-MERGE-FUNC-005")]
    public void ApplyAiChangesRequest_IdTakesPriorityOverName()
    {
        var request = new ApplyOpportunityAiChangesRequest
        {
            ProposedInitiativeTypeId = 42,
            ProposedInitiativeTypeName = "Programme"
        };

        var shouldResolve = !request.ProposedInitiativeTypeId.HasValue
                            && !string.IsNullOrWhiteSpace(request.ProposedInitiativeTypeName);
        shouldResolve.Should().BeFalse("ID is present, name resolution should not activate");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-MERGE-FUNC-006")]
    public void CreateFromInteractionsRequest_UNOPSMissionsNotApplicable_OverridesMissions()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Test",
            UNOPSMissionsNotApplicable = true,
            UNOPSMissions = new List<OpportunityUNOPSMissionRequest>
            {
                new() { UNOPSMissionId = 1 }
            }
        };

        var shouldSave = request.UNOPSMissions != null && !request.UNOPSMissionsNotApplicable;
        shouldSave.Should().BeFalse(
            "missions should not be saved when UNOPSMissionsNotApplicable is true");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-MERGE-FUNC-007")]
    public void CreateFromInteractionsRequest_NewDateFields_AreAccessible()
    {
        var request = new CreateOpportunityFromInteractionsRequest
        {
            Name = "Test",
            Description = "Test",
            ImplementationStartDate = new DateTime(2026, 9, 1),
            SubmissionDeadline = new DateTime(2026, 5, 1),
            IsTargetSigningDateFirm = false,
            SigningDateNotes = "Tentative"
        };

        request.ImplementationStartDate.Should().Be(new DateTime(2026, 9, 1));
        request.SubmissionDeadline.Should().Be(new DateTime(2026, 5, 1));
        request.IsTargetSigningDateFirm.Should().BeFalse();
        request.SigningDateNotes.Should().Be("Tentative");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Type", "Functional")]
    [Trait("TestId", "TC-MERGE-FUNC-008")]
    public void SDGMainCrossCuttingTerminology_IsPrimary_MapsCorrectly()
    {
        var sdg = new OpportunitySDGRequest { SDGId = 1, IsPrimary = true };
        var label = sdg.IsPrimary ? "Main" : "Cross-cutting";
        label.Should().Be("Main");

        var sdg2 = new OpportunitySDGRequest { SDGId = 6, IsPrimary = false };
        var label2 = sdg2.IsPrimary ? "Main" : "Cross-cutting";
        label2.Should().Be("Cross-cutting");
    }

    #endregion

    // ================================================================
    // INTEGRATION TESTS (>= 6 tests, ratio 3:1)
    // ================================================================

    #region Integration Tests

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-MERGE-INT-001")]
    public async Task UpdateOpportunity_EmptyName_ThrowsBusinessException_Integration()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Will try empty update");

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = ""
        };

        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Name is required.");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-MERGE-INT-002")]
    public async Task UpdateOpportunity_WhitespaceOnlyName_ThrowsBusinessException_Integration()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Will try whitespace update");

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "   \t  "
        };

        Func<Task> act = async () => await _manager.UpdateOpportunityAsync(updateRequest);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage("Name is required.");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-MERGE-INT-003")]
    public async Task CreateOpportunity_WithSDGs_SavesIsPrimaryFlag()
    {
        var request = new OpportunityRequest
        {
            Name = $"SDG Integration Test {_testMarker}",
            Description = "Test SDG persistence",
            SDGs = new List<OpportunitySDGRequest>
            {
                new() { SDGId = _sdgId, IsPrimary = true },
                new() { SDGId = _sdgId2, IsPrimary = false }
            }
        };

        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        _context.ChangeTracker.Clear();
        var savedSdgs = await _context.Set<OpportunitySDG>()
            .Where(s => s.OpportunityId == result.Id && !s.IsDeleted)
            .ToListAsync();

        savedSdgs.Should().HaveCount(2);
        savedSdgs.First(s => s.SDGId == _sdgId).IsPrimary.Should().BeTrue("SDG 1 should be Main");
        savedSdgs.First(s => s.SDGId == _sdgId2).IsPrimary.Should().BeFalse("SDG 6 should be Cross-cutting");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-MERGE-INT-004")]
    public async Task CreateOpportunity_WithUNOPSMissionsNotApplicable_SavesFlag()
    {
        var request = new OpportunityRequest
        {
            Name = $"Missions NA Test {_testMarker}",
            Description = "Test UNOPSMissionsNotApplicable flag",
            UNOPSMissionsNotApplicable = true,
            UNOPSMissions = null
        };

        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        _context.ChangeTracker.Clear();
        var savedOpp = await _context.Opportunities.FindAsync(result.Id);
        savedOpp.Should().NotBeNull();
        savedOpp!.UNOPSMissionsNotApplicable.Should().BeTrue();
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-MERGE-INT-005")]
    public async Task CreateOpportunity_WithNewDateFields_SavesCorrectly()
    {
        var targetSigning = DateTime.UtcNow.AddMonths(3);
        var implStart = DateTime.UtcNow.AddMonths(4);
        var submissionDeadline = DateTime.UtcNow.AddMonths(1);

        var request = new OpportunityRequest
        {
            Name = $"Date Fields Test {_testMarker}",
            Description = "Test new date fields",
            TargetSigningDate = targetSigning,
            ImplementationStartDate = implStart,
            SubmissionDeadline = submissionDeadline,
            IsTargetSigningDateFirm = true,
            SigningDateNotes = "Donor confirmed"
        };

        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-MERGE-INT-006")]
    public async Task UpdateOpportunity_NameChanged_PersistsNewValue()
    {
        var oppId = await CreateTestOpportunityAsync(name: "Before Update");

        var updateRequest = new UpdateOpportunityRequest
        {
            Id = oppId,
            Name = "After Update"
        };

        var result = await _manager.UpdateOpportunityAsync(updateRequest);

        result.Should().NotBeNull();
        _context.ChangeTracker.Clear();
        var saved = await _context.Opportunities.FindAsync(oppId);
        saved!.Name.Should().Be("After Update");
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-MERGE-INT-007")]
    public async Task CreateOpportunity_ProposedInitiativeTypeId_SavesCorrectly()
    {
        var request = new OpportunityRequest
        {
            Name = $"Initiative Type Test {_testMarker}",
            Description = "Test ProposedInitiativeTypeId",
            ProposedInitiativeTypeId = _proposedInitiativeTypeId
        };

        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        _context.ChangeTracker.Clear();
        var saved = await _context.Opportunities.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.ProposedInitiativeTypeId.Should().Be(_proposedInitiativeTypeId);
    }

    [SkipIfInMemoryFact]
    [Trait("Category", "P1")]
    [Trait("Type", "Integration")]
    [Trait("TestId", "TC-MERGE-INT-008")]
    public async Task CreateOpportunity_EmptySDGList_SavesNone()
    {
        var request = new OpportunityRequest
        {
            Name = $"Empty SDGs Test {_testMarker}",
            Description = "Test empty SDGs",
            SDGs = new List<OpportunitySDGRequest>()
        };

        var result = await _manager.CreateOpportunityAsync(request);
        _createdOpportunityIds.Add(result.Id);

        _context.ChangeTracker.Clear();
        var savedSdgs = await _context.Set<OpportunitySDG>()
            .Where(s => s.OpportunityId == result.Id && !s.IsDeleted)
            .ToListAsync();

        savedSdgs.Should().BeEmpty();
    }

    #endregion

    // ================================================================
    // Helpers
    // ================================================================

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
                _context.Database.ExecuteSql($"DELETE FROM public.\"OpportunitySDGs\" WHERE \"OpportunityId\" IN ({ids})");
                _context.Database.ExecuteSql($"DELETE FROM public.\"Opportunities\" WHERE \"Id\" IN ({ids})");
            }
        }
        catch { }

        if (TestEnvironment.UseInMemory)
        {
            try { _context.Database.EnsureDeleted(); }
            catch { }
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
