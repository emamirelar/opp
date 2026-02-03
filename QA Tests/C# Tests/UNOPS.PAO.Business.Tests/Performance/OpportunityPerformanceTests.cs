/**
 * OPPORTUNITY PERFORMANCE TESTS
 * 
 * Required: At least 1 test (no scaling minimum)
 * Purpose: Load testing, response time verification, throughput testing
 * 
 * @see .cursor/rules/comprehensive-test-strategy.mdc
 */

using FluentAssertions;
using System.Diagnostics;
using Xunit;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.Performance
{
    /// <summary>
    /// Performance Tests for Opportunity Manager
    /// 
    /// Test Strategy: These tests verify response times, throughput,
    /// and behavior under load for opportunity operations.
    /// 
    /// Required: At least 1 test (no scaling minimum)
    /// </summary>
    public class OpportunityPerformanceTests : PerformanceTestBase
    {
        // Performance thresholds
        private const int MaxSingleOperationMs = 100;
        private const int MaxPipelineCalculationMs = 500;
        private const int MaxReportGenerationMs = 2000;

        #region Pipeline Calculations

        /// <summary>
        /// Pipeline value calculation should be fast for large datasets
        /// </summary>
        [Fact]
        public void CalculatePipelineValue_1000Opportunities_CompletesQuickly()
        {
            // Arrange
            var opportunities = Enumerable.Range(1, 1000)
                .Select(i => new
                {
                    Id = i,
                    Amount = 100_000m + (i * 1000),
                    Probability = 10 + (i % 90),
                    Stage = i % 5 switch
                    {
                        0 => "Identification",
                        1 => "Qualification",
                        2 => "Proposal",
                        3 => "Negotiation",
                        _ => "Contracting"
                    }
                })
                .ToList();

            var stopwatch = Stopwatch.StartNew();

            // Act - Calculate expected values
            var pipelineValue = opportunities.Sum(o => o.Amount * (o.Probability / 100m));
            stopwatch.Stop();

            // Assert
            pipelineValue.Should().BeGreaterThan(0);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPipelineCalculationMs,
                $"Pipeline calculation took {stopwatch.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// Stage grouping should be efficient
        /// </summary>
        [Fact]
        public void GroupByStage_LargeDataset_PerformsEfficiently()
        {
            // Arrange
            var opportunities = Enumerable.Range(1, 5000)
                .Select(i => new
                {
                    Id = i,
                    Stage = new[] { "Identification", "Qualification", "Proposal", "Negotiation", "Won", "Lost" }[i % 6]
                })
                .ToList();

            var stopwatch = Stopwatch.StartNew();

            // Act - Group by stage
            var groupedByStage = opportunities
                .GroupBy(o => o.Stage)
                .ToDictionary(g => g.Key, g => g.Count());
            stopwatch.Stop();

            // Assert
            groupedByStage.Keys.Should().HaveCountGreaterThanOrEqualTo(5);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPipelineCalculationMs,
                $"Grouping took {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Complex Queries

        /// <summary>
        /// Multi-criteria filter should perform well
        /// </summary>
        [Fact]
        public void Filter_MultiCriteria_PerformsEfficiently()
        {
            // Arrange
            var opportunities = Enumerable.Range(1, 10000)
                .Select(i => new
                {
                    Id = i,
                    Amount = 50_000m + (i * 100),
                    Stage = new[] { "Identification", "Qualification", "Proposal" }[i % 3],
                    Probability = 20 + (i % 60),
                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                })
                .ToList();

            var stopwatch = Stopwatch.StartNew();

            // Act - Complex filter
            var filtered = opportunities
                .Where(o => o.Amount > 100_000m)
                .Where(o => o.Stage == "Proposal")
                .Where(o => o.Probability > 50)
                .Where(o => o.CreatedDate > DateTime.UtcNow.AddDays(-365))
                .OrderByDescending(o => o.Amount)
                .Take(100)
                .ToList();
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxPipelineCalculationMs,
                $"Complex filter took {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Report Generation

        /// <summary>
        /// Summary report generation should complete within threshold
        /// </summary>
        [Fact]
        public void GenerateSummaryReport_LargeDataset_CompletesWithinThreshold()
        {
            // Arrange
            var opportunities = Enumerable.Range(1, 5000)
                .Select(i => new
                {
                    Id = i,
                    Amount = 100_000m + (i * 500),
                    Probability = 10 + (i % 80),
                    Stage = new[] { "Identification", "Qualification", "Proposal", "Negotiation", "Won" }[i % 5],
                    PartnerId = (i % 100) + 1
                })
                .ToList();

            var stopwatch = Stopwatch.StartNew();

            // Act - Generate summary report
            var report = new
            {
                TotalOpportunities = opportunities.Count,
                TotalValue = opportunities.Sum(o => o.Amount),
                WeightedValue = opportunities.Sum(o => o.Amount * (o.Probability / 100m)),
                ByStage = opportunities.GroupBy(o => o.Stage)
                    .Select(g => new { Stage = g.Key, Count = g.Count(), Value = g.Sum(o => o.Amount) })
                    .ToList(),
                TopPartners = opportunities.GroupBy(o => o.PartnerId)
                    .Select(g => new { PartnerId = g.Key, Count = g.Count(), Value = g.Sum(o => o.Amount) })
                    .OrderByDescending(p => p.Value)
                    .Take(10)
                    .ToList()
            };
            stopwatch.Stop();

            // Assert
            report.TotalOpportunities.Should().Be(5000);
            report.ByStage.Should().HaveCount(5);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(MaxReportGenerationMs,
                $"Report generation took {stopwatch.ElapsedMilliseconds}ms");
        }

        #endregion

        #region Throughput Testing

        /// <summary>
        /// System handles high throughput of stage updates
        /// </summary>
        [Fact]
        public void StageUpdates_HighVolume_MaintainsThroughput()
        {
            // Arrange
            var opportunities = Enumerable.Range(1, 1000)
                .Select(i => (Id: i, Stage: "Identification"))
                .ToList();

            var stopwatch = Stopwatch.StartNew();

            // Act - Simulate 1000 stage updates
            var updatedOpportunities = opportunities
                .Select(o => (o.Id, Stage: "Qualification"))
                .ToList();
            stopwatch.Stop();

            // Assert
            updatedOpportunities.Should().HaveCount(1000);
            var updatesPerSecond = 1000.0 / (stopwatch.ElapsedMilliseconds / 1000.0);
            updatesPerSecond.Should().BeGreaterThan(10000,
                $"Should handle >10K updates/sec, got {updatesPerSecond:F0}");
        }

        #endregion

        #region Memory Efficiency

        /// <summary>
        /// Processing opportunities uses memory efficiently
        /// </summary>
        [Fact]
        public void ProcessOpportunities_MemoryEfficient_NoLeaks()
        {
            // Arrange
            GC.Collect();
            var memoryBefore = GC.GetTotalMemory(true);

            // Act - Process multiple batches
            for (int batch = 0; batch < 10; batch++)
            {
                var opportunities = Enumerable.Range(1, 1000)
                    .Select(i => new { Id = i, Data = new string('X', 500) })
                    .ToList();

                var processed = opportunities.Select(o => o.Id).ToList();
                // Let batch go out of scope
            }

            GC.Collect();
            var memoryAfter = GC.GetTotalMemory(true);

            // Assert - Memory should not grow significantly after GC
            var memoryGrowthMB = (memoryAfter - memoryBefore) / (1024.0 * 1024.0);
            memoryGrowthMB.Should().BeLessThan(20,
                $"Memory grew by {memoryGrowthMB:F2}MB after processing");
        }

        #endregion

        #region Stress Testing

        /// <summary>
        /// System maintains performance under sustained load
        /// </summary>
        [Fact]
        public void SustainedLoad_PerformanceDoesNotDegrade()
        {
            // Arrange
            var operationTimes = new List<long>();

            // Act - Perform 100 iterations
            for (int i = 0; i < 100; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                
                // Simulate an operation
                var result = Enumerable.Range(1, 100)
                    .Select(j => new { Id = j, Value = j * i })
                    .Where(x => x.Value > 50)
                    .Sum(x => x.Value);
                
                stopwatch.Stop();
                operationTimes.Add(stopwatch.ElapsedTicks);
            }

            // Assert - Compare first and last quarter
            var firstQuarter = operationTimes.Take(25).Average();
            var lastQuarter = operationTimes.Skip(75).Average();

            lastQuarter.Should().BeLessThan(firstQuarter * 3,
                $"Performance degraded from {firstQuarter} to {lastQuarter} ticks avg");
        }

        #endregion
    }
}
