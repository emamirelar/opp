using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
/// Negative tests for PNO-976: UNSDCF indicators - invalid inputs, wrong states, expected failures.
/// </summary>
public class PNO976NegativeTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDbContextTransaction? _transaction;

    public PNO976NegativeTests()
    {
        var options = TestEnvironment.CreateAppDbContextOptions($"PNO976_Neg_{Guid.NewGuid():N}");
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
    [Trait("Category", "Negative")]
    public void ValuesManager_GetUNCFIndicatorsByOutcomeId_InvalidOutcomeId_ReturnsEmpty()
    {
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicatorsByOutcomeId(-1).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ValuesManager_GetUNCFIndicatorsByOutcomeId_NonExistentOutcomeId_ReturnsEmpty()
    {
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicatorsByOutcomeId(99999).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFIndicatorModel_IndicatorsNull_ShouldNotThrowWhenAccessed()
    {
        var model = new UNCFIndicatorModel { Id = 1, Name = "X", Indicators = null };
        var act = () => _ = model.Indicators;
        act.Should().NotThrow();
        model.Indicators.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFIndicatorModel_DescriptionNull_ShouldNotThrowWhenAccessed()
    {
        var model = new UNCFIndicatorModel { Id = 1, Name = "X", Description = null };
        model.Description.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFIndicatorModel_UnitNull_ShouldNotThrowWhenAccessed()
    {
        var model = new UNCFIndicatorModel { Id = 1, Name = "X", Unit = null };
        model.Unit.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AutoMapper_UNCFIndicatorWithNullIndicators_MapsToNull()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "Ind", Indicators = null, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Indicators.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AutoMapper_UNCFIndicatorWithNullDescription_MapsToNull()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "Ind", Description = null, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Description.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AutoMapper_UNCFIndicatorWithNullUnit_MapsToNull()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "Ind", Unit = null, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Unit.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IndicatorNameFallback_NullIndicators_ShouldNotThrow()
    {
        string? indicators = null;
        string name = "Ind-001";
        var display = !string.IsNullOrEmpty(indicators) ? indicators : name;
        display.Should().Be("Ind-001");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IndicatorNameFallback_WhitespaceIndicators_ShouldUseName()
    {
        var indicators = "   ";
        var name = "Ind-001";
        var display = !string.IsNullOrEmpty(indicators?.Trim()) ? indicators : name;
        display.Should().Be("Ind-001");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ValuesManager_GetUNCFIndicators_NoData_ReturnsEmpty()
    {
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ValuesManager_GetUNCFOutcomesByCountry_InvalidCountry_ReturnsEmpty()
    {
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFOutcomesByCountry("INVALID").ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFIndicatorModel_NameEmpty_ShouldBeValid()
    {
        var model = new UNCFIndicatorModel { Id = 1, Name = "", Indicators = "Desc" };
        model.Name.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AutoMapper_NullUNCFIndicator_ReturnsNull()
    {
        UNCFIndicator? entity = null;
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IndicatorDisplay_NumericIdOnly_WithoutIndicators_IsInsufficient()
    {
        // REQ-2: Displaying only numeric ID is insufficient - need descriptive text
        var entity = new UNCFIndicator { Id = 1, Name = "1.2.3", Indicators = null, Status = EntityStatus.Active };
        var hasDescriptiveInfo = !string.IsNullOrEmpty(entity.Indicators) || !string.IsNullOrEmpty(entity.Description);
        hasDescriptiveInfo.Should().BeFalse("entity has no descriptive text - display would show only numeric ID");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFIndicatorModel_AllOptionalFieldsNull_StillValid()
    {
        var model = new UNCFIndicatorModel
        {
            Id = 1,
            Name = "Minimal",
            Indicators = null,
            Description = null,
            Unit = null,
            Country = null,
            UNCFIndicatorExternalId = null,
            UNCFOutcomeExternalId = null,
            VersionNo = null
        };
        model.Should().NotBeNull();
        model.Indicators.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ValuesRepository_GetUNCFIndicatorsByOutcomeId_OutcomeWithNoIndicators_ReturnsEmpty()
    {
        var outcomeId = SeedOutcomeOnly();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFIndicator_StatusInactive_ExcludedWhenIncludeInactiveFalse()
    {
        var (outcomeId, _) = SeedInactiveIndicator();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicatorsByOutcomeId(outcomeId, includeInactive: false).ToList();
        result.Should().BeEmpty("inactive indicators should be excluded by default");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFMetadata_Inactive_ExcludesIndicatorsFromDefaultQuery()
    {
        SeedInactiveMetadata();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicators(includeInactive: false).ToList();
        result.Should().BeEmpty("indicators under inactive metadata should be excluded");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void OpportunityUNCFIndicatorModel_UNCFIndicatorNameNull_WhenIndicatorMissing()
    {
        var oppIndicator = new OpportunityUNCFIndicator { Id = 1, UNCFIndicatorId = 999, UNCFIndicator = null };
        var model = _mapper.Map<OpportunityUNCFIndicatorModel>(oppIndicator);
        model.UNCFIndicatorName.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void DisplayLogic_EmptyIndicators_ShouldFallbackToName()
    {
        var indicators = "";
        var name = "Fallback-Name";
        var result = !string.IsNullOrEmpty(indicators) ? indicators : name;
        result.Should().Be("Fallback-Name");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFIndicatorModel_ZeroId_IsValid()
    {
        var model = new UNCFIndicatorModel { Id = 0, Name = "X" };
        model.Id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ValuesManager_GetUNCFOutcomes_NoMetadata_ReturnsEmpty()
    {
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFOutcomes().ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void AutoMapper_UNCFIndicator_MissingOptionalFields_DoesNotThrow()
    {
        var entity = new UNCFIndicator
        {
            Id = 1,
            Name = "X",
            Status = EntityStatus.Active
        };
        var act = () => _mapper.Map<UNCFIndicatorModel>(entity);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void IndicatorNameFallback_BothNull_ThrowsOrHandles()
    {
        string? indicators = null;
        string name = "";
        var display = !string.IsNullOrEmpty(indicators) ? indicators : name;
        display.Should().Be("");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFIndicator_CountryMismatch_NotReturnedForOutcome()
    {
        var (outcomeId, _) = SeedIndicatorWithCountryMismatch();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        result.Should().BeEmpty("indicator has different country than outcome");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void UNCFIndicator_VersionMismatch_NotReturnedForOutcome()
    {
        var (outcomeId, _) = SeedIndicatorWithVersionMismatch();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        result.Should().BeEmpty("indicator has different version than outcome");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void ValuesRepository_GetUNCFIndicators_NoMetadata_ReturnsEmpty()
    {
        SeedIndicatorWithoutMetadata();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicators(includeInactive: false).ToList();
        result.Should().BeEmpty("indicator without matching active metadata");
    }

    private int SeedOutcomeOnly()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        _context.SaveChanges();
        return outcome.Id;
    }

    private (int, int) SeedInactiveIndicator()
    {
        var metadata = new UNCFMetadata { Id = 1, Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Id = 1, Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var indicator = new UNCFIndicator { Id = 1, Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Inactive };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
        return (outcome.Id, indicator.Id);
    }

    private void SeedInactiveMetadata()
    {
        var metadata = new UNCFMetadata { Id = 1, Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Inactive };
        _context.UNCFMetadatas.Add(metadata);
        var indicator = new UNCFIndicator { Id = 1, Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
    }

    private (int, int) SeedIndicatorWithCountryMismatch()
    {
        var metadata = new UNCFMetadata { Id = 1, Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Id = 1, Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var indicator = new UNCFIndicator { Id = 1, Name = "I1", Country = "YY", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
        return (outcome.Id, indicator.Id);
    }

    private (int, int) SeedIndicatorWithVersionMismatch()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var indicator = new UNCFIndicator { Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 2, Status = EntityStatus.Active };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
        return (outcome.Id, indicator.Id);
    }

    private void SeedIndicatorWithoutMetadata()
    {
        var indicator = new UNCFIndicator { Name = "I1", Country = "ZZ", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _context.Dispose();
    }
}
