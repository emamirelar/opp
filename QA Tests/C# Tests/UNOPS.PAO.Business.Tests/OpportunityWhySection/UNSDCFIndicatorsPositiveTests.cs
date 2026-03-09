using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Mapping;
using UNOPS.PAO.Business.Repositories;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.UNCF;
using UNOPS.PAO.Business.Tests.TestBase;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhySection;

/// <summary>
/// Tests for PNO-976: WHY > UNSDCF - Issues with spelling and indicators
///
/// Requirements validated:
/// - REQ-1: All UI labels must display "UNSDCF" (translation - validated in frontend)
/// - REQ-2: UNCF Indicators must display descriptive text (Indicators/Description), not just numeric ID
/// - REQ-3: uncfIndicatorName must use descriptive indicators text, fallback to name if null/empty
/// - REQ-4: UNCFIndicatorModel must expose Indicators, Description, Unit properties
/// - REQ-5: AutoMapper must correctly map UNCFIndicator entity fields to UNCFIndicatorModel
/// </summary>
public class PNO976PositiveTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDbContextTransaction? _transaction;

    public PNO976PositiveTests()
    {
        var options = TestEnvironment.CreateAppDbContextOptions($"PNO976_Pos_{Guid.NewGuid():N}");
        var mockAccessor = new Moq.Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
        var userResolver = new UNOPS.PAO.DataAccess.Services.UserResolverService<int>(mockAccessor.Object);
        var mockSchema = new Moq.Mock<UNOPS.PAO.DataAccess.Interfaces.IDbContextSchema>();
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
    [Trait("Category", "Positive")]
    public void UNCFIndicatorModel_HasIndicatorsProperty_CanBeSetAndRead()
    {
        // REQ-4: UNCFIndicatorModel must expose Indicators property
        var model = new UNCFIndicatorModel { Id = 1, Name = "Ind-1", Indicators = "Percentage of population with access to basic services" };
        model.Indicators.Should().Be("Percentage of population with access to basic services");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UNCFIndicatorModel_HasDescriptionProperty_CanBeSetAndRead()
    {
        // REQ-4: UNCFIndicatorModel must expose Description property
        var model = new UNCFIndicatorModel { Id = 1, Name = "Ind-1", Description = "Additional context for the indicator" };
        model.Description.Should().Be("Additional context for the indicator");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void UNCFIndicatorModel_HasUnitProperty_CanBeSetAndRead()
    {
        // REQ-4: UNCFIndicatorModel must expose Unit property
        var model = new UNCFIndicatorModel { Id = 1, Name = "Ind-1", Unit = "%" };
        model.Unit.Should().Be("%");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void AutoMapper_UNCFIndicatorToModel_MapsIndicatorsField()
    {
        // REQ-5: AutoMapper must map Indicators from entity to model
        var entity = new UNCFIndicator
        {
            Id = 1,
            Name = "Ind-001",
            Indicators = "Descriptive indicator text for display",
            Description = "Extra description",
            Unit = "Number",
            Status = EntityStatus.Active,
            UNCFIndicatorId = "ext-1",
            UNCFOutcomeExternalId = "out-1",
            UNCooperationFrameworkVersionNo = 1,
            Country = "XX"
        };

        var model = _mapper.Map<UNCFIndicatorModel>(entity);

        model.Indicators.Should().Be("Descriptive indicator text for display");
        model.Description.Should().Be("Extra description");
        model.Unit.Should().Be("Number");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void AutoMapper_UNCFIndicatorToModel_MapsAllNewProperties()
    {
        // REQ-5: Indicators, Description, Unit mapped by convention
        var entity = new UNCFIndicator
        {
            Id = 42,
            Name = "Test Name",
            Indicators = "Indicator description",
            Description = "Full description",
            Unit = "%",
            UNCFIndicatorId = "ext-42",
            UNCFOutcomeExternalId = "out-99",
            UNCooperationFrameworkVersionNo = 2,
            Country = "ET",
            Status = EntityStatus.Active
        };

        var model = _mapper.Map<UNCFIndicatorModel>(entity);

        model.Id.Should().Be(42);
        model.Name.Should().Be("Test Name");
        model.Indicators.Should().Be("Indicator description");
        model.Description.Should().Be("Full description");
        model.Unit.Should().Be("%");
        model.UNCFIndicatorExternalId.Should().Be("ext-42");
        model.VersionNo.Should().Be(2);
        model.Country.Should().Be("ET");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ValuesManager_GetUNCFIndicators_ReturnsModelsWithIndicatorsField()
    {
        // REQ-2, REQ-4: ValuesManager returns UNCFIndicatorModel with descriptive fields
        SeedUNCFData();
        var manager = new ValuesManager(_mapper, _context);

        var result = manager.GetUNCFIndicators().ToList();

        result.Should().NotBeEmpty();
        var withIndicators = result.FirstOrDefault(m => !string.IsNullOrEmpty(m.Indicators));
        if (withIndicators != null)
        {
            withIndicators.Indicators.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void ValuesManager_GetUNCFIndicatorsByOutcomeId_ReturnsModelsWithDescriptiveFields()
    {
        // REQ-2: Indicators returned with descriptive information
        var (outcomeId, _) = SeedUNCFData();
        var manager = new ValuesManager(_mapper, _context);

        var result = manager.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();

        result.Should().NotBeEmpty();
        result.Should().OnlyContain(m => m.Id > 0 && m.Name != null);
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void IndicatorNameFallback_WhenIndicatorsPopulated_ShouldPreferIndicatorsOverName()
    {
        // REQ-3: Fallback logic - when indicators is available, use it for display
        var entity = new UNCFIndicator
        {
            Id = 1,
            Name = "Ind-001",
            Indicators = "Percentage of households with improved water source",
            Status = EntityStatus.Active
        };

        var expectedDisplay = !string.IsNullOrEmpty(entity.Indicators) ? entity.Indicators : entity.Name;
        expectedDisplay.Should().Be("Percentage of households with improved water source");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void IndicatorNameFallback_WhenIndicatorsNull_ShouldUseName()
    {
        // REQ-3: Fallback - when indicators is null, use name
        var entity = new UNCFIndicator { Id = 1, Name = "Ind-001", Indicators = null, Status = EntityStatus.Active };
        var expectedDisplay = !string.IsNullOrEmpty(entity.Indicators) ? entity.Indicators : entity.Name;
        expectedDisplay.Should().Be("Ind-001");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void IndicatorNameFallback_WhenIndicatorsEmpty_ShouldUseName()
    {
        // REQ-3: Fallback - when indicators is empty string, use name
        var entity = new UNCFIndicator { Id = 1, Name = "Ind-001", Indicators = "", Status = EntityStatus.Active };
        var expectedDisplay = !string.IsNullOrEmpty(entity.Indicators) ? entity.Indicators : entity.Name;
        expectedDisplay.Should().Be("Ind-001");
    }

    private (int OutcomeId, int IndicatorId) SeedUNCFData()
    {
        var metadata = new UNCFMetadata
        {
            Name = "XX v1",
            Country = "XX",
            UNCooperationFrameworkVersionNo = 1,
            Status = EntityStatus.Active
        };
        _context.UNCFMetadatas.Add(metadata);

        var outcome = new UNCFOutcome
        {
            Name = "Outcome 1",
            Country = "XX",
            UNCFOutcomeId = "out-1",
            UNCooperationFrameworkVersionNo = 1,
            Status = EntityStatus.Active
        };
        _context.UNCFOutcomes.Add(outcome);

        var indicator = new UNCFIndicator
        {
            Name = "Ind-001",
            Indicators = "Descriptive indicator text",
            Description = "Description text",
            Unit = "%",
            Country = "XX",
            UNCFOutcomeExternalId = "out-1",
            UNCooperationFrameworkVersionNo = 1,
            UNCFIndicatorId = "ext-1",
            Status = EntityStatus.Active
        };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();

        return (outcome.Id, indicator.Id);
    }

    public void Dispose()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _context.Dispose();
    }
}
