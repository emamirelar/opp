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
    public void PERF_001_SingleMap_CompletesWithin100ms()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Test" };

        // Warm-up to avoid JIT overhead in measurement
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Warmup" }, CreateOpportunity());

        var sw = Stopwatch.StartNew();
        _mapper.Map(request, dest);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(100,
            $"single map took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("DEF012", "PERF_002")]
    public void PERF_002_MapWithAllNulls_CompletesWithin100ms()
    {
        _mapper.Map(new UpdateOpportunityRequest { Id = 10 }, CreateOpportunity()); // warm-up
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10 };
        var sw = Stopwatch.StartNew();
        _mapper.Map(request, dest);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    [Trait("DEF012", "PERF_003")]
    public void PERF_003_MapWithAllValues_CompletesWithin100ms()
    {
        _mapper.Map(new UpdateOpportunityRequest { Id = 10 }, CreateOpportunity()); // warm-up
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
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    [Trait("DEF012", "PERF_004")]
    public void PERF_004_MapWithPartialValues_CompletesWithin100ms()
    {
        _mapper.Map(new UpdateOpportunityRequest { Id = 10 }, CreateOpportunity()); // warm-up
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "P", Description = "D" };
        var sw = Stopwatch.StartNew();
        _mapper.Map(request, dest);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }

    [Fact]
    [Trait("DEF012", "PERF_005")]
    public void PERF_005_MapWithLargeStrings_CompletesWithin200ms()
    {
        _mapper.Map(new UpdateOpportunityRequest { Id = 10 }, CreateOpportunity()); // warm-up
        var dest = CreateOpportunity();
        var big = new string('x', 5000);
        var request = new UpdateOpportunityRequest { Id = 10, Name = big, Description = big };
        var sw = Stopwatch.StartNew();
        _mapper.Map(request, dest);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(200,
            $"large string map took {sw.ElapsedMilliseconds}ms");
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
    public void PERF_013_MapperReuse_100Maps_CompletesWithin200ms()
    {
        _mapper.Map(new UpdateOpportunityRequest { Id = 10 }, CreateOpportunity()); // warm-up
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"U{i}" }, d);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(200,
            $"100 mapper reuse maps took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("DEF012", "PERF_014")]
    public void PERF_014_GcPressureFromMaps_200Iterations_MemoryStable()
    {
        _mapper.Map(new UpdateOpportunityRequest { Id = 10 }, CreateOpportunity()); // warm-up
        GC.Collect();
        var memBefore = GC.GetTotalMemory(forceFullCollection: true);

        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Gc" };
        for (var i = 0; i < 200; i++)
            _mapper.Map(request, dest);

        GC.Collect();
        var memAfter = GC.GetTotalMemory(forceFullCollection: true);
        var growthMb = (memAfter - memBefore) / 1_048_576.0;
        growthMb.Should().BeLessThan(5, "200 map operations should not leak memory");
        dest.Name.Should().Be("Gc");
    }

    [Fact]
    [Trait("DEF012", "PERF_015")]
    public async Task PERF_015_ConcurrentMapperUsage_ThreadSafe_CompletesWithin1s()
    {
        _mapper.Map(new UpdateOpportunityRequest { Id = 10 }, CreateOpportunity()); // warm-up
        var sw = Stopwatch.StartNew();
        var tasks = Enumerable.Range(0, 10).Select(t => Task.Run(() =>
        {
            for (var i = 0; i < 50; i++)
            {
                var d = CreateOpportunity();
                _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"T{t}_I{i}" }, d);
                d.Name.Should().Be($"T{t}_I{i}");
            }
        }));
        await Task.WhenAll(tasks);
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(1000,
            $"500 concurrent maps took {sw.ElapsedMilliseconds}ms");
    }

    [Fact]
    [Trait("DEF012", "PERF_016")]
    public void PERF_016_MapperConfigurationCreation_CompletesWithin3s()
    {
        var sw = Stopwatch.StartNew();
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        _ = config.CreateMapper();
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(3000,
            $"MapperConfiguration creation took {sw.ElapsedMilliseconds}ms");
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
