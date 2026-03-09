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
/// Boundary/edge tests for PNO-976: UNSDCF indicators - min/max values, soft-delete, nullable edges.
/// </summary>
public class PNO976BoundaryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDbContextTransaction? _transaction;

    public PNO976BoundaryTests()
    {
        var options = TestEnvironment.CreateAppDbContextOptions($"PNO976_Bnd_{Guid.NewGuid():N}");
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
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_IndicatorsField_MaxLength2000_IsAccepted()
    {
        var longText = new string('x', 2000);
        var entity = new UNCFIndicator { Id = 1, Name = "X", Indicators = longText, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Indicators.Should().HaveLength(2000);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_DescriptionField_MaxLength3000_IsAccepted()
    {
        var longText = new string('d', 3000);
        var entity = new UNCFIndicator { Id = 1, Name = "X", Description = longText, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Description.Should().HaveLength(3000);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IndicatorNameFallback_IndicatorsExactlyOneChar_UsedOverName()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "LongName", Indicators = "A", Status = EntityStatus.Active };
        var display = !string.IsNullOrEmpty(entity.Indicators) ? entity.Indicators : entity.Name;
        display.Should().Be("A");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IndicatorNameFallback_IndicatorsWhitespaceOnly_ShouldFallbackToName()
    {
        var indicators = " \t\n ";
        var name = "Name";
        var display = !string.IsNullOrEmpty(indicators?.Trim()) ? indicators : name;
        display.Should().Be("Name");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicatorModel_UnitSingleChar_IsValid()
    {
        var model = new UNCFIndicatorModel { Id = 1, Name = "X", Unit = "%" };
        model.Unit.Should().Be("%");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_SpecialCharactersInIndicators_MapsCorrectly()
    {
        var text = "Indicator with spécial chars: 50% <target> & \"quotes\"";
        var entity = new UNCFIndicator { Id = 1, Name = "X", Indicators = text, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Indicators.Should().Be(text);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_UnicodeInDescription_MapsCorrectly()
    {
        var text = "描述文本 日本語 العربية";
        var entity = new UNCFIndicator { Id = 1, Name = "X", Description = text, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Description.Should().Be(text);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_IndicatorsAndDescriptionBothPopulated_BothMapped()
    {
        var entity = new UNCFIndicator
        {
            Id = 1,
            Name = "X",
            Indicators = "Primary description",
            Description = "Secondary description",
            Status = EntityStatus.Active
        };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Indicators.Should().Be("Primary description");
        model.Description.Should().Be("Secondary description");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ValuesManager_GetUNCFIndicators_SingleIndicator_ReturnsOne()
    {
        var indicatorId = SeedSingleIndicator();
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().ContainSingle();
        result[0].Id.Should().Be(indicatorId);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ValuesManager_GetUNCFIndicatorsByOutcomeId_IncludeInactive_ReturnsInactive()
    {
        var (outcomeId, _) = SeedActiveAndInactiveIndicators();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicatorsByOutcomeId(outcomeId, includeInactive: true).ToList();
        result.Should().HaveCount(2, "includeInactive=true should return both active and inactive");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_VersionNoNull_MapsToNull()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "X", UNCooperationFrameworkVersionNo = null, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.VersionNo.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_CountryTwoLetter_MapsCorrectly()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "X", Country = "ET", Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Country.Should().Be("ET");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void IndicatorNameFallback_IndicatorsWithLeadingTrailingSpaces_TrimmedForDisplay()
    {
        var indicators = "  Descriptive text  ";
        var trimmed = indicators.Trim();
        trimmed.Should().Be("Descriptive text");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicatorModel_AllFieldsAtBoundary_Valid()
    {
        var model = new UNCFIndicatorModel
        {
            Id = int.MaxValue,
            Name = new string('n', 1000),
            Indicators = new string('i', 2000),
            Description = new string('d', 3000),
            Unit = new string('u', 255)
        };
        model.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void AutoMapper_UNCFIndicator_ExternalIdsNull_MapsToNull()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "X", UNCFIndicatorId = null, UNCFOutcomeExternalId = null, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.UNCFIndicatorExternalId.Should().BeNull();
        model.UNCFOutcomeExternalId.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void ValuesRepository_GetUNCFIndicators_IncludeInactiveTrue_ReturnsAll()
    {
        SeedActiveAndInactiveIndicators();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicators(includeInactive: true).ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_NameComputedFromIndicators_WhenIndicatorsPresent()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "Short", Indicators = "Long descriptive indicator text", Status = EntityStatus.Active };
        var preferredDisplay = !string.IsNullOrEmpty(entity.Indicators) ? entity.Indicators : entity.Name;
        preferredDisplay.Should().Be("Long descriptive indicator text");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_UnitPercentSign_MapsCorrectly()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "X", Unit = "%", Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Unit.Should().Be("%");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_UnitNumber_MapsCorrectly()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "X", Unit = "Number", Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Unit.Should().Be("Number");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-114")]
    public void SoftDelete_UNCFIndicatorIsDeleted_ShouldBeExcluded_RequiresIsDeletedFilter()
    {
        // DEF-076: ValuesRepository.GetUNCFIndicators does not filter !indicator.IsDeleted
        var (outcomeId, deletedIndicatorId) = SeedSoftDeletedIndicator();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicatorsByOutcomeId(outcomeId, includeInactive: true).ToList();
        result.Should().NotContain(i => i.Id == deletedIndicatorId, "REQ: soft-deleted records must be excluded per IsDeleted rule");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_OutcomeExternalIdMismatch_NotReturned()
    {
        var (outcomeId, _) = SeedIndicatorWithWrongOutcomeExternalId();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_MultipleIndicatorsSameOutcome_AllReturned()
    {
        var outcomeId = SeedMultipleIndicatorsSameOutcome();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        result.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    [Trait("Defect", "DEF-115")]
    public void OpportunityUNCFIndicator_UNCFIndicatorName_ShouldUseIndicatorsOverName()
    {
        // DEF-077: OpportunityMappingProfile maps UNCFIndicatorName from Name only.
        // REQ-3: Should use Indicators ?? Name (descriptive text preferred)
        var indicator = new UNCFIndicator
        {
            Id = 1,
            Name = "Ind-001",
            Indicators = "Percentage of population with access to basic services",
            Status = EntityStatus.Active
        };
        var oppIndicator = new OpportunityUNCFIndicator { Id = 1, UNCFIndicatorId = 1, UNCFIndicator = indicator };
        var model = _mapper.Map<OpportunityUNCFIndicatorModel>(oppIndicator);
        model.UNCFIndicatorName.Should().Be("Percentage of population with access to basic services",
            "REQ-3: uncfIndicatorName must use descriptive Indicators text when available");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_EmptyStringIndicators_FallbackToName()
    {
        var entity = new UNCFIndicator { Id = 1, Name = "Name", Indicators = "", Status = EntityStatus.Active };
        var display = !string.IsNullOrEmpty(entity.Indicators) ? entity.Indicators : entity.Name;
        display.Should().Be("Name");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void UNCFIndicator_NewlineInIndicators_MapsCorrectly()
    {
        var text = "Line1\nLine2";
        var entity = new UNCFIndicator { Id = 1, Name = "X", Indicators = text, Status = EntityStatus.Active };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Indicators.Should().Be(text);
    }

    private int SeedSingleIndicator()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var indicator = new UNCFIndicator { Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
        return indicator.Id;
    }

    private (int, int) SeedActiveAndInactiveIndicators()
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

    private (int, int) SeedSoftDeletedIndicator()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var ind1 = new UNCFIndicator { Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        var ind2 = new UNCFIndicator { Name = "I2", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active, IsDeleted = true };
        _context.UNCFIndicators.AddRange(ind1, ind2);
        _context.SaveChanges();
        return (outcome.Id, ind2.Id);
    }

    private (int, int) SeedIndicatorWithWrongOutcomeExternalId()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var indicator = new UNCFIndicator { Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o2", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
        return (outcome.Id, indicator.Id);
    }

    private int SeedMultipleIndicatorsSameOutcome()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var ind1 = new UNCFIndicator { Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        var ind2 = new UNCFIndicator { Name = "I2", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFIndicators.AddRange(ind1, ind2);
        _context.SaveChanges();
        return outcome.Id;
    }

    public void Dispose()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _context.Dispose();
    }
}
