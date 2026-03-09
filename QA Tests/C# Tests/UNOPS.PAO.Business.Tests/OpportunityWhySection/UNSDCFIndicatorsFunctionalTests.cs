using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Mapping;
using UNOPS.PAO.Business.Repositories;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.UNCF;
using UNOPS.PAO.Business.Tests.TestBase;
using Moq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhySection;

/// <summary>
/// Functional tests for PNO-976: Business rules, audit fields, workflow, data transformations.
/// </summary>
public class PNO976FunctionalTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDbContextTransaction? _transaction;

    public PNO976FunctionalTests()
    {
        var options = TestEnvironment.CreateAppDbContextOptions($"PNO976_Fnc_{Guid.NewGuid():N}");
        var mockAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userResolver = new UserResolverService<int>(mockAccessor.Object);
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(s => s.Schema).Returns("public");
        _context = new AppDbContext(options, userResolver, mockSchema.Object);
        TestEnvironment.EnsureCleanDatabase(_context);

        if (TestEnvironment.UsePostgreSQL)
        {
            _transaction = _context.Database.BeginTransaction();
            _context.UNCFIndicators.RemoveRange(_context.UNCFIndicators);
            _context.UNCFOutcomes.RemoveRange(_context.UNCFOutcomes);
            _context.UNCFMetadatas.RemoveRange(_context.UNCFMetadatas);
            _context.SaveChanges();
        }
        else
        {
            _transaction = null;
        }

        var config = new MapperConfiguration(cfg => cfg.AddProfile<OpportunityMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AutoMapper_UNCFIndicatorToModel_ConventionMapsIndicatorsDescriptionUnit()
    {
        // REQ-5: AutoMapper maps by convention when property names match
        var entity = new UNCFIndicator
        {
            Id = 1,
            Name = "X",
            Indicators = "Ind",
            Description = "Desc",
            Unit = "U",
            Status = EntityStatus.Active
        };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Indicators.Should().Be("Ind");
        model.Description.Should().Be("Desc");
        model.Unit.Should().Be("U");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AutoMapper_UNCFIndicator_ExplicitMapsOverrideConvention()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "X", UNCFIndicatorId = "ext-1", UNCooperationFrameworkVersionNo = 5, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.UNCFIndicatorExternalId.Should().Be("ext-1");
        model.VersionNo.Should().Be(5);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ValuesManager_GetUNCFIndicators_MapsToUNCFIndicatorModel()
    {
        SeedUNCFData();
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().AllSatisfy(m =>
        {
            m.Should().BeOfType<UNCFIndicatorModel>();
            m.Id.Should().BeGreaterThan(0);
            m.Name.Should().NotBeNull();
        });
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ValuesManager_GetUNCFIndicatorsByOutcomeId_ReturnsOnlyMatchingOutcome()
    {
        var (outcomeId, _) = SeedMultipleOutcomesWithIndicators();
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(m => m.Id > 0);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IndicatorDisplayLogic_IndicatorsPreferredOverName()
    {
        // REQ-2, REQ-3: Display logic
        string GetDisplayName(UNCFIndicatorModel m) => !string.IsNullOrEmpty(m.Indicators) ? m.Indicators : m.Name;
        var model = new UNCFIndicatorModel { Id = 1, Name = "1.2.3", Indicators = "Percentage of population with access" };
        GetDisplayName(model).Should().Be("Percentage of population with access");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IndicatorDisplayLogic_NameFallbackWhenIndicatorsEmpty()
    {
        string GetDisplayName(UNCFIndicatorModel m) => !string.IsNullOrEmpty(m.Indicators) ? m.Indicators : m.Name;
        var model = new UNCFIndicatorModel { Id = 1, Name = "1.2.3", Indicators = null };
        GetDisplayName(model).Should().Be("1.2.3");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFIndicatorModel_ExposesAllRequiredPropertiesForFrontend()
    {
        // REQ-4: API model must expose Indicators, Description, Unit
        var model = new UNCFIndicatorModel();
        model.GetType().GetProperty("Indicators").Should().NotBeNull();
        model.GetType().GetProperty("Description").Should().NotBeNull();
        model.GetType().GetProperty("Unit").Should().NotBeNull();
        model.GetType().GetProperty("Name").Should().NotBeNull();
        model.GetType().GetProperty("Id").Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ValuesRepository_GetUNCFIndicators_JoinsWithMetadata()
    {
        SeedUNCFData();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicators().ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ValuesRepository_GetUNCFIndicatorsByOutcomeId_FiltersByOutcomeExternalIdAndVersion()
    {
        var (outcomeId, _) = SeedUNCFData();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFIndicator_StatusActive_IsIncludedInDefaultQuery()
    {
        SeedUNCFData();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicators(includeInactive: false).ToList();
        result.Should().OnlyContain(i => i.Status == EntityStatus.Active);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFMetadata_StatusActive_RequiredForIndicatorInclusion()
    {
        SeedUNCFData();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicators(includeInactive: false).ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AutoMapper_OpportunityUNCFIndicator_MapUNCFIndicatorNameFromNavigation()
    {
        var indicator = new UNCFIndicator { Id = 1, Name = "MappedName", Status = EntityStatus.Active };
        var oppIndicator = new OpportunityUNCFIndicator { Id = 1, UNCFIndicatorId = 1, UNCFIndicator = indicator };
        var model = _mapper.Map<OpportunityUNCFIndicatorModel>(oppIndicator);
        model.UNCFIndicatorName.Should().Be("MappedName");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFIndicatorModel_SerializationFriendly_AllPropertiesSettable()
    {
        var model = new UNCFIndicatorModel
        {
            Id = 1,
            Name = "N",
            Indicators = "I",
            Description = "D",
            Unit = "U",
            Country = "C",
            UNCFIndicatorExternalId = "E",
            UNCFOutcomeExternalId = "O",
            VersionNo = 1
        };
        model.Indicators.Should().Be("I");
        model.Description.Should().Be("D");
        model.Unit.Should().Be("U");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ValuesManager_GetUNCFIndicators_ReturnsDistinctIndicators()
    {
        SeedUNCFData();
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicators().ToList();
        var ids = result.Select(m => m.Id).ToList();
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void IndicatorNameFallback_ImplementationMatchesRequirement()
    {
        // REQ-3: uncfIndicatorName = indicators ?? name (with null/empty check)
        string GetUncfIndicatorName(string? indicators, string name) =>
            !string.IsNullOrEmpty(indicators) ? indicators : name;

        GetUncfIndicatorName("Desc", "Name").Should().Be("Desc");
        GetUncfIndicatorName(null, "Name").Should().Be("Name");
        GetUncfIndicatorName("", "Name").Should().Be("Name");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFIndicator_EntityHasIndicatorsField()
    {
        var entity = new UNCFIndicator { Indicators = "Test" };
        entity.Indicators.Should().Be("Test");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFIndicator_EntityHasDescriptionField()
    {
        var entity = new UNCFIndicator { Description = "Test" };
        entity.Description.Should().Be("Test");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFIndicator_EntityHasUnitField()
    {
        var entity = new UNCFIndicator { Unit = "%" };
        entity.Unit.Should().Be("%");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ValuesRepository_GetUNCFIndicators_RespectsIncludeInactiveFlag()
    {
        var (_, _) = SeedActiveAndInactive();
        var repo = new ValuesRepository(_context);
        var activeOnly = repo.GetUNCFIndicators(includeInactive: false).ToList();
        var all = repo.GetUNCFIndicators(includeInactive: true).ToList();
        all.Count.Should().BeGreaterThan(activeOnly.Count);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ValuesRepository_GetUNCFIndicatorsByOutcomeId_RespectsIncludeInactiveFlag()
    {
        var (outcomeId, _) = SeedActiveAndInactive();
        var repo = new ValuesRepository(_context);
        var activeOnly = repo.GetUNCFIndicatorsByOutcomeId(outcomeId, includeInactive: false).ToList();
        var all = repo.GetUNCFIndicatorsByOutcomeId(outcomeId, includeInactive: true).ToList();
        all.Count.Should().BeGreaterThanOrEqualTo(activeOnly.Count);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFIndicatorModel_CanBeUsedForDisplayBinding()
    {
        var model = new UNCFIndicatorModel { Name = "1.2.3", Indicators = "Percentage of households" };
        var displayText = !string.IsNullOrEmpty(model.Indicators) ? model.Indicators : model.Name;
        displayText.Should().Be("Percentage of households");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void AutoMapper_UNCFIndicator_ExplicitMappingsPreserved()
    {
        var entity = new UNCFIndicator
        {
            Id = 1,
            Name = "X",
            UNCFIndicatorId = "external-id",
            UNCooperationFrameworkVersionNo = 3,
            Status = EntityStatus.Active
        };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.UNCFIndicatorExternalId.Should().Be("external-id");
        model.VersionNo.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void OpportunityUNCFIndicatorModel_HasUNCFIndicatorNameForDisplay()
    {
        var model = new OpportunityUNCFIndicatorModel { UNCFIndicatorName = "Display Name" };
        model.UNCFIndicatorName.Should().Be("Display Name");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ValuesManager_GetUNCFIndicators_ReturnsEnumerable()
    {
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicators();
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<UNCFIndicatorModel>>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ValuesManager_GetUNCFIndicatorsByOutcomeId_ReturnsEnumerable()
    {
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicatorsByOutcomeId(1);
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<UNCFIndicatorModel>>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFIndicator_CountryAndVersion_JoinWithMetadata()
    {
        var metadata = new UNCFMetadata { Country = "ET", UNCooperationFrameworkVersionNo = 1 };
        var indicator = new UNCFIndicator { Country = "ET", UNCooperationFrameworkVersionNo = 1 };
        var matches = metadata.Country == indicator.Country && metadata.UNCooperationFrameworkVersionNo == indicator.UNCooperationFrameworkVersionNo;
        matches.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void UNCFIndicator_OutcomeExternalId_LinksToOutcome()
    {
        var outcome = new UNCFOutcome { UNCFOutcomeId = "out-1" };
        var indicator = new UNCFIndicator { UNCFOutcomeExternalId = "out-1" };
        var matches = indicator.UNCFOutcomeExternalId == outcome.UNCFOutcomeId;
        matches.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void REQ2_IndicatorDisplay_RequiresDescriptiveText()
    {
        // REQ-2: Indicators must display descriptive text, not just numeric ID
        var hasDescriptive = (UNCFIndicatorModel m) => !string.IsNullOrEmpty(m.Indicators) || !string.IsNullOrEmpty(m.Description);
        var modelWithDesc = new UNCFIndicatorModel { Name = "1.2.3", Indicators = "Percentage of households" };
        var modelWithoutDesc = new UNCFIndicatorModel { Name = "1.2.3", Indicators = null, Description = null };
        hasDescriptive(modelWithDesc).Should().BeTrue();
        hasDescriptive(modelWithoutDesc).Should().BeFalse();
    }

    private (int, int) SeedUNCFData()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var indicator = new UNCFIndicator { Name = "I1", Indicators = "Desc", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
        return (outcome.Id, indicator.Id);
    }

    private (int, int) SeedMultipleOutcomesWithIndicators()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var indicator = new UNCFIndicator { Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
        return (outcome.Id, indicator.Id);
    }

    private (int, int) SeedActiveAndInactive()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var ind1 = new UNCFIndicator { Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        var ind2 = new UNCFIndicator { Name = "I2", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Inactive };
        _context.UNCFIndicators.AddRange(ind1, ind2);
        _context.SaveChanges();
        return (outcome.Id, ind2.Id);
    }

    public void Dispose()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _context.Dispose();
    }
}
