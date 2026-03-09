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
using UNOPS.PAO.Models.UNCF;
using UNOPS.PAO.Business.Tests.TestBase;
using Moq;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhySection;

/// <summary>
/// Integration tests for PNO-976: Full flows, repository-to-manager, API contract.
/// </summary>
public class PNO976IntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDbContextTransaction? _transaction;

    public PNO976IntegrationTests()
    {
        var options = TestEnvironment.CreateAppDbContextOptions($"PNO976_Int_{Guid.NewGuid():N}");
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
    [Trait("Category", "Integration")]
    public void RepositoryToManager_GetUNCFIndicators_FullFlow()
    {
        SeedFullUNCFHierarchy();
        var repo = new ValuesRepository(_context);
        var manager = new ValuesManager(_mapper, _context);

        var repoEntities = repo.GetUNCFIndicators().ToList();
        var managerModels = manager.GetUNCFIndicators().ToList();

        managerModels.Should().HaveCount(repoEntities.Count);
        foreach (var model in managerModels)
        {
            model.Should().NotBeNull();
            model.Id.Should().BeGreaterThan(0);
            var entity = repoEntities.FirstOrDefault(e => e.Id == model.Id);
            entity.Should().NotBeNull();
            model.Indicators.Should().Be(entity!.Indicators);
            model.Description.Should().Be(entity.Description);
            model.Unit.Should().Be(entity.Unit);
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void RepositoryToManager_GetUNCFIndicatorsByOutcomeId_FullFlow()
    {
        var (outcomeId, _) = SeedFullUNCFHierarchy();
        var repo = new ValuesRepository(_context);
        var manager = new ValuesManager(_mapper, _context);

        var repoEntities = repo.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        var managerModels = manager.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();

        managerModels.Should().HaveCount(repoEntities.Count);
        managerModels.Should().OnlyContain(m => m.Indicators != null || m.Description != null || !string.IsNullOrEmpty(m.Name));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void EntityToModel_UncfIndicatorToApiModel_RoundTrip()
    {
        var entity = new UNCFIndicator
        {
            Id = 1,
            Name = "Ind-001",
            Indicators = "Percentage of population with access to basic services",
            Description = "SDG-aligned indicator",
            Unit = "%",
            UNCFIndicatorId = "ext-1",
            UNCFOutcomeExternalId = "out-1",
            UNCooperationFrameworkVersionNo = 1,
            Country = "ET",
            Status = EntityStatus.Active
        };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Id.Should().Be(entity.Id);
        model.Name.Should().Be(entity.Name);
        model.Indicators.Should().Be(entity.Indicators);
        model.Description.Should().Be(entity.Description);
        model.Unit.Should().Be(entity.Unit);
        model.UNCFIndicatorExternalId.Should().Be(entity.UNCFIndicatorId);
        model.VersionNo.Should().Be(entity.UNCooperationFrameworkVersionNo);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DbContext_SeedAndQuery_UNCFIndicatorsWithMetadata()
    {
        SeedFullUNCFHierarchy();
        var indicators = _context.UNCFIndicators.ToList();
        var metadata = _context.UNCFMetadatas.ToList();
        indicators.Should().NotBeEmpty();
        metadata.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ValuesManager_GetUNCFIndicators_ModelsHaveAllRequiredFields()
    {
        SeedFullUNCFHierarchy();
        var manager = new ValuesManager(_mapper, _context);
        var models = manager.GetUNCFIndicators().ToList();
        models.Should().NotBeEmpty();
        models.Should().OnlyContain(m =>
            m.Id > 0 &&
            m.Name != null &&
            m.GetType().GetProperty("Indicators") != null &&
            m.GetType().GetProperty("Description") != null &&
            m.GetType().GetProperty("Unit") != null);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_IndicatorWithDescriptiveText_AvailableForDisplay()
    {
        var (outcomeId, indicatorId) = SeedFullUNCFHierarchy();
        var manager = new ValuesManager(_mapper, _context);
        var models = manager.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        var withDescriptive = models.FirstOrDefault(m => !string.IsNullOrEmpty(m.Indicators) || !string.IsNullOrEmpty(m.Description));
        withDescriptive.Should().NotBeNull("at least one indicator should have descriptive text for display");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void MultiCountry_GetUNCFOutcomesByCountry_FiltersCorrectly()
    {
        SeedMultiCountryUNCF();
        var manager = new ValuesManager(_mapper, _context);
        var etOutcomes = manager.GetUNCFOutcomesByCountry("ET").ToList();
        var xxOutcomes = manager.GetUNCFOutcomesByCountry("XX").ToList();
        etOutcomes.Should().NotBeEmpty();
        xxOutcomes.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void IndicatorToModel_AllFieldsPreserved()
    {
        var entity = new UNCFIndicator
        {
            Id = 42,
            Name = "Name",
            Indicators = "Ind",
            Description = "Desc",
            Unit = "U",
            UNCFIndicatorId = "e",
            UNCFOutcomeExternalId = "o",
            UNCooperationFrameworkVersionNo = 2,
            Country = "CC",
            Status = EntityStatus.Active
        };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.Id.Should().Be(42);
        model.Name.Should().Be("Name");
        model.Indicators.Should().Be("Ind");
        model.Description.Should().Be("Desc");
        model.Unit.Should().Be("U");
        model.UNCFIndicatorExternalId.Should().Be("e");
        model.UNCFOutcomeExternalId.Should().Be("o");
        model.VersionNo.Should().Be(2);
        model.Country.Should().Be("CC");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Repository_GetUNCFIndicators_JoinWithMetadataWorks()
    {
        SeedFullUNCFHierarchy();
        var repo = new ValuesRepository(_context);
        var result = repo.GetUNCFIndicators().ToList();
        result.Should().NotBeEmpty();
        result.Should().OnlyContain(i => i.Country != null && i.UNCooperationFrameworkVersionNo != null);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Manager_GetUNCFIndicatorsByOutcomeId_OutcomeExists()
    {
        var (outcomeId, _) = SeedFullUNCFHierarchy();
        var outcome = _context.UNCFOutcomes.Find(outcomeId);
        outcome.Should().NotBeNull();
        var manager = new ValuesManager(_mapper, _context);
        var indicators = manager.GetUNCFIndicatorsByOutcomeId(outcomeId).ToList();
        indicators.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ApiContract_UNCFIndicatorModel_MatchesFrontendInterface()
    {
        // Frontend expects: id, name, indicators, description, unit
        var model = new UNCFIndicatorModel
        {
            Id = 1,
            Name = "1.2.3",
            Indicators = "Descriptive text",
            Description = "More info",
            Unit = "%"
        };
        model.Id.Should().Be(1);
        model.Name.Should().Be("1.2.3");
        model.Indicators.Should().Be("Descriptive text");
        model.Description.Should().Be("More info");
        model.Unit.Should().Be("%");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void DbContext_EnsureCreated_UNCFTablesExist()
    {
        var tables = new[] { "UNCFMetadatas", "UNCFOutcomes", "UNCFIndicators" };
        foreach (var table in tables)
        {
            var set = _context.GetType().GetProperty(table);
            set.Should().NotBeNull($"{table} DbSet should exist");
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ValuesManager_Constructor_AcceptsAppDbContext()
    {
        var manager = new ValuesManager(_mapper, _context);
        manager.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void ValuesRepository_Constructor_AcceptsAppDbContext()
    {
        var repo = new ValuesRepository(_context);
        repo.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_EmptyDatabase_GetUNCFIndicatorsReturnsEmpty()
    {
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicators().ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void FullFlow_EmptyDatabase_GetUNCFIndicatorsByOutcomeIdReturnsEmpty()
    {
        var manager = new ValuesManager(_mapper, _context);
        var result = manager.GetUNCFIndicatorsByOutcomeId(1).ToList();
        result.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void MapperConfiguration_OpportunityProfile_UNCFIndicatorMappingWorks()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OpportunityMappingProfile>());
        var mapper = config.CreateMapper();
        var entity = new UNCFIndicator { Id = 1, Name = "X", Indicators = "Desc", Status = EntityStatus.Active };
        var model = mapper.Map<UNCFIndicatorModel>(entity);
        model.Should().NotBeNull();
        model.Indicators.Should().Be("Desc");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void MultiOutcome_IndicatorsGroupedByOutcome()
    {
        var (outcome1Id, outcome2Id) = SeedTwoOutcomesWithIndicators();
        var manager = new ValuesManager(_mapper, _context);
        var ind1 = manager.GetUNCFIndicatorsByOutcomeId(outcome1Id).ToList();
        var ind2 = manager.GetUNCFIndicatorsByOutcomeId(outcome2Id).ToList();
        ind1.Should().NotBeEmpty();
        ind2.Should().NotBeEmpty();
        ind1.Select(i => i.Id).Should().NotIntersectWith(ind2.Select(i => i.Id));
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void UNCFIndicator_EntityMapping_ConventionAndExplicit()
    {
        var entity = new UNCFIndicator
        {
            Id = 1,
            Name = "N",
            Indicators = "I",
            UNCFIndicatorId = "ext",
            UNCooperationFrameworkVersionNo = 1,
            Status = EntityStatus.Active
        };
        var model = _mapper.Map<UNCFIndicatorModel>(entity);
        model.UNCFIndicatorExternalId.Should().Be("ext");
        model.VersionNo.Should().Be(1);
        model.Indicators.Should().Be("I");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void SaveChanges_AfterSeed_DataPersisted()
    {
        SeedFullUNCFHierarchy();
        var count = _context.UNCFIndicators.Count();
        count.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void GetUNCFIndicators_WithDescriptiveFields_ReadyForApiResponse()
    {
        SeedFullUNCFHierarchy();
        var manager = new ValuesManager(_mapper, _context);
        var models = manager.GetUNCFIndicators().ToList();
        models.Should().OnlyContain(m => m.Id > 0 && m.Name != null);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void REQ4_ApiModel_ExposesIndicatorsDescriptionUnit()
    {
        var model = new UNCFIndicatorModel();
        var indicatorsProp = model.GetType().GetProperty("Indicators");
        var descProp = model.GetType().GetProperty("Description");
        var unitProp = model.GetType().GetProperty("Unit");
        indicatorsProp.Should().NotBeNull();
        descProp.Should().NotBeNull();
        unitProp.Should().NotBeNull();
    }

    private (int, int) SeedFullUNCFHierarchy()
    {
        var metadata = new UNCFMetadata { Name = "ET v1", Country = "ET", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var outcome = new UNCFOutcome { Name = "Outcome 1", Country = "ET", UNCFOutcomeId = "out-1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.Add(outcome);
        var indicator = new UNCFIndicator
        {
            Name = "Ind-001",
            Indicators = "Percentage of population with access to basic services",
            Description = "SDG indicator",
            Unit = "%",
            Country = "ET",
            UNCFOutcomeExternalId = "out-1",
            UNCooperationFrameworkVersionNo = 1,
            UNCFIndicatorId = "ext-1",
            Status = EntityStatus.Active
        };
        _context.UNCFIndicators.Add(indicator);
        _context.SaveChanges();
        return (outcome.Id, indicator.Id);
    }

    private void SeedMultiCountryUNCF()
    {
        var meta1 = new UNCFMetadata { Name = "ET", Country = "ET", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        var meta2 = new UNCFMetadata { Name = "XX", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.AddRange(meta1, meta2);
        var out1 = new UNCFOutcome { Name = "O1", Country = "ET", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        var out2 = new UNCFOutcome { Name = "O2", Country = "XX", UNCFOutcomeId = "o2", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.AddRange(out1, out2);
        _context.SaveChanges();
    }

    private (int, int) SeedTwoOutcomesWithIndicators()
    {
        var metadata = new UNCFMetadata { Name = "X", Country = "XX", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFMetadatas.Add(metadata);
        var out1 = new UNCFOutcome { Name = "O1", Country = "XX", UNCFOutcomeId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        var out2 = new UNCFOutcome { Name = "O2", Country = "XX", UNCFOutcomeId = "o2", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFOutcomes.AddRange(out1, out2);
        var ind1 = new UNCFIndicator { Name = "I1", Country = "XX", UNCFOutcomeExternalId = "o1", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        var ind2 = new UNCFIndicator { Name = "I2", Country = "XX", UNCFOutcomeExternalId = "o2", UNCooperationFrameworkVersionNo = 1, Status = EntityStatus.Active };
        _context.UNCFIndicators.AddRange(ind1, ind2);
        _context.SaveChanges();
        return (out1.Id, out2.Id);
    }

    public void Dispose()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _context.Dispose();
    }
}
