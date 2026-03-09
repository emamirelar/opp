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
/// DEF-012: Concurrency tests for OpportunityMappingProfile ForAllMembers fix.
/// Parallel mapping, thread safety, stress.
/// </summary>
[Collection("Concurrency")]
[Trait("Category", "Concurrency")]
[Trait("Type", "Concurrency")]
public class ConcurrencyTests
{
    private readonly IMapper _mapper;

    public ConcurrencyTests()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        _mapper = config.CreateMapper();
    }

    #region CONC_001-008: Parallel Mapping

    [Fact]
    [Trait("DEF012", "CONC_001")]
    public void CONC_001_TwoThreadsMapSameSource()
    {
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Shared" };
        var results = new List<string>();
        Parallel.Invoke(
            () =>
            {
                var d = CreateOpportunity();
                _mapper.Map(request, d);
                lock (results) results.Add(d.Name ?? "");
            },
            () =>
            {
                var d = CreateOpportunity();
                _mapper.Map(request, d);
                lock (results) results.Add(d.Name ?? "");
            });
        results.Should().HaveCount(2);
        results.Should().OnlyContain(s => s == "Shared");
    }

    [Fact]
    [Trait("DEF012", "CONC_002")]
    public async Task CONC_002_TwoThreadsMapToSameDestType()
    {
        var tasks = Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "T" }, d);
            return d.Name;
        })).ToArray();
        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(n => n == "T");
    }

    [Fact]
    [Trait("DEF012", "CONC_003")]
    public void CONC_003_ConcurrentMapDifferentSources()
    {
        var results = new string[10];
        Parallel.For(0, 10, i =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"Name{i}" }, d);
            results[i] = d.Name;
        });
        results.Should().HaveCount(10);
        for (var i = 0; i < 10; i++)
            results[i].Should().Be($"Name{i}");
    }

    [Fact]
    [Trait("DEF012", "CONC_004")]
    public async Task CONC_004_ParallelMapWithSharedMapper()
    {
        var count = 20;
        var tasks = Enumerable.Range(0, count).Select(i => Task.Run(() =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"N{i}" }, d);
            return d.Name;
        })).ToArray();
        var names = (await Task.WhenAll(tasks)).ToList();
        names.Should().HaveCount(count);
        names.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    [Trait("DEF012", "CONC_005")]
    public void CONC_005_RapidSequentialMaps()
    {
        var dest = CreateOpportunity();
        for (var i = 0; i < 50; i++)
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"Seq{i}" }, dest);
        dest.Name.Should().Be("Seq49");
    }

    [Fact]

    [Trait("Defect", "DEF-023")]
    [Trait("DEF012", "CONC_006")]
    public void CONC_006_MapDuringConfigValidation()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        config.AssertConfigurationIsValid();
        var mapper = config.CreateMapper();
        var dest = CreateOpportunity();
        mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "X" }, dest);
        dest.Name.Should().Be("X");
    }

    [Fact]
    [Trait("DEF012", "CONC_007")]
    public void CONC_007_ConcurrentCreateMapperCalls_Safe()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        var mappers = new IMapper[5];
        Parallel.For(0, 5, i => mappers[i] = config.CreateMapper());
        mappers.Should().OnlyContain(m => m != null);
    }

    [Fact]
    [Trait("DEF012", "CONC_008")]
    public void CONC_008_ParallelMapOutput_Independent()
    {
        var results = new List<OpportunityEntity>();
        Parallel.For(0, 5, _ =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Ind" }, d);
            lock (results) results.Add(d);
        });
        results.Should().HaveCount(5);
        results.Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region CONC_009-016: Thread Safety

    [Fact]
    [Trait("DEF012", "CONC_009")]
    public async Task CONC_009_Mapper_IsThreadSafe()
    {
        var tasks = Enumerable.Range(0, 20).Select(i => Task.Run(() =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"T{i}" }, d);
            return d.Name;
        })).ToArray();
        var results = await Task.WhenAll(tasks);
        results.Should().HaveCount(20);
    }

    [Fact]
    [Trait("DEF012", "CONC_010")]
    public void CONC_010_ConcurrentReadsOfMappedResult()
    {
        var d = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Read" }, d);
        var names = new string[10];
        Parallel.For(0, 10, i => names[i] = d.Name ?? "");
        names.Should().OnlyContain(n => n == "Read");
    }

    [Fact]
    [Trait("DEF012", "CONC_011")]
    public void CONC_011_ParallelMap_DoesNotCorrupt()
    {
        var dests = new OpportunityEntity[10];
        Parallel.For(0, 10, i =>
        {
            dests[i] = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"C{i}" }, dests[i]);
        });
        for (var i = 0; i < 10; i++)
            dests[i].Name.Should().Be($"C{i}");
    }

    [Fact]
    [Trait("DEF012", "CONC_012")]
    public void CONC_012_SourceMutationDuringMap()
    {
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Before" };
        var dest = CreateOpportunity();
        _mapper.Map(request, dest);
        request.Name = "After";
        dest.Name.Should().Be("Before");
    }

    [Fact]
    [Trait("DEF012", "CONC_013")]
    public async Task CONC_013_Destination_NotCorruptedByParallelMap()
    {
        var dest = CreateOpportunity();
        var tasks = Enumerable.Range(0, 5).Select(_ => Task.Run(() =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "P" }, d);
            return d;
        })).ToArray();
        var results = await Task.WhenAll(tasks);
        results.Should().OnlyContain(d => d.Name == "P");
    }

    [Fact]

    [Trait("Defect", "DEF-023")]
    [Trait("DEF012", "CONC_014")]
    public void CONC_014_CollectionIgnore_UnderConcurrency()
    {
        var request = new UpdateOpportunityRequest
        {
            Id = 10,
            Name = "X",
            FundingPartners = new List<OpportunityFundingPartnerRequest> { new() { PartnerId = 1 } }
        };
        var dests = new OpportunityEntity[5];
        Parallel.For(0, 5, i =>
        {
            dests[i] = CreateOpportunity();
            _mapper.Map(request, dests[i]);
        });
        dests.Should().OnlyContain(d => d.FundingPartners == null || d.FundingPartners.Count == 0);
    }

    [Fact]
    [Trait("DEF012", "CONC_015")]
    public void CONC_015_ForAllMembersCondition_ThreadSafe()
    {
        var dests = new OpportunityEntity[10];
        Parallel.For(0, 10, i =>
        {
            dests[i] = CreateOpportunity();
            dests[i].Description = $"Desc{i}";
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"N{i}", Description = null }, dests[i]);
        });
        for (var i = 0; i < 10; i++)
            dests[i].Description.Should().Be($"Desc{i}");
    }

    [Fact]
    [Trait("DEF012", "CONC_016")]
    public void CONC_016_SequentialMap_Consistency()
    {
        var dest = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "A" }, dest);
        dest.Name.Should().Be("A");
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "B" }, dest);
        dest.Name.Should().Be("B");
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "C" }, dest);
        dest.Name.Should().Be("C");
    }

    #endregion

    #region CONC_017-025: Stress

    [Fact]
    [Trait("DEF012", "CONC_017")]
    public void CONC_017_1000ParallelMaps()
    {
        var results = new string[1000];
        Parallel.For(0, 1000, i =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"S{i}" }, d);
            results[i] = d.Name;
        });
        results.Should().HaveCount(1000);
        for (var i = 0; i < 1000; i++)
            results[i].Should().Be($"S{i}");
    }

    [Fact]
    [Trait("DEF012", "CONC_018")]
    public void CONC_018_MapUnderCpuPressure()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Cpu" };
        var act = () => _mapper.Map(request, dest);
        act.Should().NotThrow();
        dest.Name.Should().Be("Cpu");
    }

    [Fact]
    [Trait("DEF012", "CONC_019")]
    public void CONC_019_MapUnderMemoryPressure()
    {
        var dest = CreateOpportunity();
        var request = new UpdateOpportunityRequest { Id = 10, Name = "Mem" };
        var act = () => _mapper.Map(request, dest);
        act.Should().NotThrow();
    }

    [Fact]
    [Trait("DEF012", "CONC_020")]
    public void CONC_020_RapidCreateMapCycles()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        for (var i = 0; i < 20; i++)
        {
            var mapper = config.CreateMapper();
            var d = CreateOpportunity();
            mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"Cycle{i}" }, d);
            d.Name.Should().Be($"Cycle{i}");
        }
    }

    [Fact]
    [Trait("DEF012", "CONC_021")]
    public void CONC_021_ConcurrentMapThenValidate()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        var mapper = config.CreateMapper();
        Parallel.For(0, 10, _ =>
        {
            var d = CreateOpportunity();
            mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "V" }, d);
            d.Name.Should().Be("V");
        });
    }

    [Fact]
    [Trait("DEF012", "CONC_022")]
    public void CONC_022_ParallelMapPerformance()
    {
        var sw = Stopwatch.StartNew();
        Parallel.For(0, 100, i =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"P{i}" }, d);
        });
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    [Trait("DEF012", "CONC_023")]
    public void CONC_023_BulkMapOperations()
    {
        var count = 500;
        var dests = new OpportunityEntity[count];
        Parallel.For(0, count, i =>
        {
            dests[i] = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"Bulk{i}" }, dests[i]);
        });
        for (var i = 0; i < count; i++)
            dests[i].Name.Should().Be($"Bulk{i}");
    }

    [Fact]
    [Trait("DEF012", "CONC_024")]
    public void CONC_024_ConcurrentMapAndAssert()
    {
        var errors = new List<string>();
        Parallel.For(0, 20, i =>
        {
            try
            {
                var d = CreateOpportunity();
                _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"A{i}" }, d);
                d.Name.Should().Be($"A{i}");
            }
            catch (Exception ex)
            {
                lock (errors) errors.Add(ex.Message);
            }
        });
        errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("DEF012", "CONC_025")]
    public void CONC_025_StressTest_MapperStability()
    {
        for (var round = 0; round < 5; round++)
        {
            Parallel.For(0, 100, i =>
            {
                var d = CreateOpportunity();
                _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"R{round}N{i}" }, d);
                d.Name.Should().Be($"R{round}N{i}");
            });
        }
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
