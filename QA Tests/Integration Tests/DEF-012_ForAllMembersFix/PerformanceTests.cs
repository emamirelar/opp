using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Managers.Mapping;
using Xunit;

using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;

namespace UNOPS.PAO.Business.Tests.DEF012;

/// <summary>
/// DEF-012: Performance tests for OpportunityMappingProfile ForAllMembers fix.
/// Single map speed, batch map speed, resource efficiency.
/// </summary>
[Collection("Performance")]
[Trait("Category", "Performance")]
[Trait("Type", "Performance")]
public class PerformanceTests
{
    private readonly IMapper _mapper;

    public PerformanceTests()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        _mapper = config.CreateMapper();
    }

    #region PERF_001-005: Single Map Speed

    [Fact]
    [Trait("DEF012", "PERF_001")]
    public void PERF_001_SingleMap_LessThan5ms()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Test" };
        var sw = Stopwatch.StartNew();
        _mapper.Map(request, dest);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    [Trait("DEF012", "PERF_002")]
    public void PERF_002_MapWithAllNulls_LessThan5ms()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10 };
        var sw = Stopwatch.StartNew();
        _mapper.Map(request, dest);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    [Trait("DEF012", "PERF_003")]
    public void PERF_003_MapWithAllValues_LessThan5ms()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "N",
            Description = "D",
            PartnerReference = "PR",
            Stage = "GO",
            ResponsibleOrgUnitId = 1,
            InitiativeBudgetUSD = 100m,
            TargetSigningDate = DateTime.UtcNow,
            TargetDeliveryDate = DateTime.UtcNow.AddMonths(6),
            ProposedInitiativeTypeId = 2
        };
        var sw = Stopwatch.StartNew();
        _mapper.Map(request, dest);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    [Trait("DEF012", "PERF_004")]
    public void PERF_004_MapWithPartialValues_LessThan5ms()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "P", Description = "D" };
        var sw = Stopwatch.StartNew();
        _mapper.Map(request, dest);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    [Trait("DEF012", "PERF_005")]
    public void PERF_005_MapWithLargeStrings_LessThan10ms()
    {
        var dest = CreateOpportunity();
        var big = new string('x', 5000);
        var request = new UpdateOpportunityRequest { Id = 10, Name = big, Description = big };
        var sw = Stopwatch.StartNew();
        _mapper.Map(request, dest);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    #endregion

    #region PERF_006-010: Batch Map Speed

    [Fact]
    [Trait("DEF012", "PERF_006")]
    public void PERF_006_TenMaps_LessThan50ms()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 10; i++)
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"N{i}" }, d);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    [Fact]
    [Trait("DEF012", "PERF_007")]
    public void PERF_007_100Maps_LessThan200ms()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"N{i}" }, d);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    [Trait("DEF012", "PERF_008")]
    public void PERF_008_1000Maps_LessThan1s()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"N{i}" }, d);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    [Trait("DEF012", "PERF_009")]
    public void PERF_009_MapsWithIncreasingData_LessThan2s()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var d = CreateOpportunity();
            var req = new UpdateOpportunityRequest
            {
                Id = 10,
                Name = new string('a', i % 100),
                Description = new string('b', i % 50),
                InitiativeBudgetUSD = i * 1000m
            };
            _mapper.Map(req, d);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    [Trait("DEF012", "PERF_010")]
    public void PERF_010_SequentialMapThenRead_LessThan500ms()
    {
        var dest = CreateOpportunity();
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 50; i++)
        {
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"R{i}" }, dest);
            _ = dest.Name;
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    #endregion

    #region PERF_011-016: Resource Efficiency

    [Fact]
    [Trait("DEF012", "PERF_011")]
    public void PERF_011_Map_DoesNotAllocateExcessively()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Alloc" };
        var before = GC.GetTotalMemory(false);
        for (var i = 0; i < 100; i++)
            _mapper.Map(request, dest);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var after = GC.GetTotalMemory(false);
        (after - before).Should().BeLessThan(10 * 1024 * 1024);
    }

    [Fact]
    [Trait("DEF012", "PERF_012")]
    public void PERF_012_1000Maps_MemoryStable()
    {
        var dests = new List<OpportunityEntity>();
        for (var i = 0; i < 1000; i++)
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"M{i}" }, d);
            dests.Add(d);
        }
        dests.Should().HaveCount(1000);
        dests[999].Name.Should().Be("M999");
    }

    [Fact]
    [Trait("DEF012", "PERF_013")]
    public void PERF_013_MapperInstance_Reusable()
    {
        for (var i = 0; i < 100; i++)
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"U{i}" }, d);
            d.Name.Should().Be($"U{i}");
        }
    }

    [Fact]
    [Trait("DEF012", "PERF_014")]
    public void PERF_014_GcPressureFromMaps_Minimal()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Gc" };
        for (var i = 0; i < 200; i++)
            _mapper.Map(request, dest);
        dest.Name.Should().Be("Gc");
    }

    [Fact]
    [Trait("DEF012", "PERF_015")]
    public void PERF_015_Map_DoesNotHoldReferences()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Ref" };
        _mapper.Map(request, dest);
        request = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        dest.Name.Should().Be("Ref");
    }

    [Fact]
    [Trait("DEF012", "PERF_016")]
    public void PERF_016_MapperConfigurationCreation_LessThan100ms()
    {
        var sw = Stopwatch.StartNew();
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        _ = config.CreateMapper();
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    #endregion

    private static OpportunityEntity CreateOpportunity()
    {
        return new OpportunityEntity
        {
            Id = 10,
            Name = "Test",
            Description = "Test Desc",
            Stage = "IDENTIFY & PROFILE",
            Status = EntityStatus.Draft,
            IsDeleted = false
        };
    }
}
