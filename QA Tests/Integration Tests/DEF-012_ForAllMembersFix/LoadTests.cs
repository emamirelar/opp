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
/// DEF-012: Load tests for OpportunityMappingProfile ForAllMembers fix.
/// Sustained load, spike load, stress.
/// </summary>
[Collection("Load")]
[Trait("Category", "Load")]
[Trait("Type", "Load")]
public class LoadTests
{
    private readonly IMapper _mapper;

    public LoadTests()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies()); });
        _mapper = config.CreateMapper();
    }

    #region LOAD_001-003: Sustained Load

    [Fact]
    [Trait("DEF012", "LOAD_001")]
    public void LOAD_001_100SequentialMaps_LessThan2s()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 100; i++)
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"L{i}" }, d);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(2000);
    }

    [Fact]
    [Trait("DEF012", "LOAD_002")]
    public void LOAD_002_500SequentialMaps_LessThan10s()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 500; i++)
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"L{i}" }, d);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(10000);
    }

    [Fact]
    [Trait("DEF012", "LOAD_003")]
    public void LOAD_003_1000MapsWithVariedData_LessThan20s()
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 1000; i++)
        {
            var d = CreateOpportunity();
            var req = new UpdateOpportunityRequest
            {
                Id = 10,
                Name = $"V{i}",
                Description = i % 2 == 0 ? $"Desc{i}" : null,
                InitiativeBudgetUSD = i % 3 == 0 ? i * 100m : null,
                Stage = i % 5 == 0 ? "GO" : null
            };
            _mapper.Map(req, d);
        }
        sw.Stop();
        sw.ElapsedMilliseconds.Should().BeLessThan(20000);
    }

    #endregion

    #region LOAD_004-006: Spike Load

    [Fact]
    [Trait("DEF012", "LOAD_004")]
    public void LOAD_004_50SimultaneousMaps()
    {
        var results = new string[50];
        Parallel.For(0, 50, i =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"S{i}" }, d);
            results[i] = d.Name;
        });
        results.Should().HaveCount(50);
        for (var i = 0; i < 50; i++)
            results[i].Should().Be($"S{i}");
    }

    [Fact]
    [Trait("DEF012", "LOAD_005")]
    public void LOAD_005_100SimultaneousMaps()
    {
        var results = new string[100];
        Parallel.For(0, 100, i =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"S{i}" }, d);
            results[i] = d.Name;
        });
        results.Should().HaveCount(100);
        for (var i = 0; i < 100; i++)
            results[i].Should().Be($"S{i}");
    }

    [Fact]
    [Trait("DEF012", "LOAD_006")]
    public void LOAD_006_BurstMapValidateCycles()
    {
        for (var burst = 0; burst < 5; burst++)
        {
            Parallel.For(0, 20, i =>
            {
                var d = CreateOpportunity();
                _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"B{burst}N{i}" }, d);
                d.Name.Should().Be($"B{burst}N{i}");
            });
        }
    }

    #endregion

    #region LOAD_007-010: Stress

    [Fact]
    [Trait("DEF012", "LOAD_007")]
    public void LOAD_007_10000Maps_Stability()
    {
        var errors = 0;
        Parallel.For(0, 10000, i =>
        {
            try
            {
                var d = CreateOpportunity();
                _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"X{i}" }, d);
                if (d.Name != $"X{i}") Interlocked.Increment(ref errors);
            }
            catch
            {
                Interlocked.Increment(ref errors);
            }
        });
        errors.Should().Be(0);
    }

    [Fact]
    [Trait("DEF012", "LOAD_008")]
    public void LOAD_008_MapsWithIncreasingStringSizes()
    {
        for (var size = 100; size <= 1000; size += 100)
        {
            var d = CreateOpportunity();
            var str = new string('x', size);
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = str }, d);
            d.Name.Should().Be(str);
        }
    }

    [Fact]
    [Trait("DEF012", "LOAD_009")]
    public void LOAD_009_SystemRecoveryAfterMapSpike()
    {
        Parallel.For(0, 200, i =>
        {
            var d = CreateOpportunity();
            _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"Spike{i}" }, d);
        });
        var d2 = CreateOpportunity();
        _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = "Recovery" }, d2);
        d2.Name.Should().Be("Recovery");
    }

    [Fact]
    [Trait("DEF012", "LOAD_010")]
    public void LOAD_010_MapPerformanceDegradationOverTime()
    {
        var times = new List<long>();
        for (var round = 0; round < 10; round++)
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 100; i++)
            {
                var d = CreateOpportunity();
                _mapper.Map(new UpdateOpportunityRequest { Id = 10, Name = $"D{i}" }, d);
            }
            sw.Stop();
            times.Add(sw.ElapsedMilliseconds);
        }
        times.Should().HaveCount(10);
        var avg = times.Average();
        times.Last().Should().BeLessThan((long)(avg * 3));
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
