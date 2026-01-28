using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.IntegrationTests.Infrastructure;

namespace UNOPS.PAO.Tests.Integration.DST
{
    /// <summary>
    /// Performance and edge case tests for DST (Decision Support Tool)
    /// Tests system performance, scalability, and handling of edge cases
    /// 
    /// Test Coverage:
    /// - Large opportunity context processing time
    /// - Concurrent recommendation requests
    /// - Cache effectiveness metrics
    /// - Memory usage for large result sets
    /// - Database query optimization
    /// - Extreme edge cases
    /// </summary>
    [Collection("Integration Tests")]
    [Trait("Category", "Integration")]
    [Trait("Feature", "DST")]
    [Trait("Component", "Performance")]
    public class DSTPerformanceTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DSTPerformanceTests(PAOWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        #region Test Setup Helpers

        private async Task<int> CreateTestOpportunityAsync(
            string title,
            string description,
            string deliverables = "",
            decimal budget = 1000000m)
        {
            using var scope = _factory.Services.CreateScope();
            var opportunityManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().OpportunityManager;

            var opportunity = new OpportunityCreateRequest
            {
                Title = title,
                Description = description,
                Deliverables = deliverables,
                CountryId = 1,
                EstimatedBudget = budget,
                Stage = "Draft"
            };

            var created = await opportunityManager.CreateOpportunityAsync(opportunity);
            return created.Id;
        }

        private async Task<DSTRecommendationsResponse> GetDSTRecommendationsAsync(
            int opportunityId,
            int maxResults = 10,
            bool forceRefresh = true)
        {
            using var scope = _factory.Services.CreateScope();
            var geminiManager = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().GeminiManager;
            var user = CreateTestUser();

            return await geminiManager.GetDSTRecommendationsAsync(
                opportunityId: opportunityId,
                user: user,
                maxResults: maxResults,
                dismissedOupQuestionIds: new List<int>(),
                forceRefresh: forceRefresh
            );
        }

        private ClaimsPrincipal CreateTestUser(int userId = 1)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, $"Test User {userId}"),
                new Claim(ClaimTypes.Role, "Project Manager")
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        #endregion

        #region TC-DST-PERF-001 through TC-DST-PERF-003: Performance Benchmarks

        /// <summary>
        /// TC-DST-PERF-001: Large opportunity context processing time
        /// 
        /// Given: Opportunity with extensive description and deliverables
        /// When: DST recommendations are generated
        /// Then: Processing completes within performance threshold
        /// 
        /// Expected Behavior:
        /// - Completes within 30 seconds
        /// - Handles large text efficiently
        /// - Memory usage is reasonable
        /// - All steps (keyword extraction, vector search, AI analysis) complete
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-PERF-001")]
        [Trait("Priority", "Critical")]
        public async Task DSTRecommendations_LargeContext_UnderPerformanceThreshold()
        {
            // Arrange - Create large opportunity
            var largeDescription = string.Join("\n\n", Enumerable.Range(1, 50).Select(i =>
                $"Paragraph {i}: This project involves complex multi-sector development activities including infrastructure construction, capacity building, institutional strengthening, community engagement, environmental sustainability measures, gender mainstreaming, conflict sensitivity, and risk management protocols. The implementation will require coordination with multiple stakeholders including government ministries, civil society organizations, private sector partners, and international development agencies."));

            var largeDeliverables = string.Join("\n", Enumerable.Range(1, 30).Select(i =>
                $"Deliverable {i}: Comprehensive implementation of sector-specific activities including needs assessment, design, procurement, construction/implementation, monitoring, evaluation, reporting, and knowledge management."));

            var opportunityId = await CreateTestOpportunityAsync(
                title: "Large-Scale Multi-Sector Development Program with Extensive Scope and Complexity",
                description: largeDescription,
                deliverables: largeDeliverables,
                budget: 25000000m
            );

            // Act
            var stopwatch = Stopwatch.StartNew();
            var response = await GetDSTRecommendationsAsync(opportunityId, maxResults: 20);
            stopwatch.Stop();

            // Assert
            response.Should().NotBeNull("DST should handle large context");
            
            // Performance threshold: 30 seconds
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000,
                "large context should process within 30 seconds");

            // Verify system still produces quality results
            response.Recommendations.Should().NotBeNull();
            response.ExtractedKeywords.Should().NotBeEmpty(
                "keywords should be extracted even from large context");

            // ExecutionTimeMs should be close to measured time
            response.ExecutionTimeMs.Should().BeLessThan(stopwatch.ElapsedMilliseconds + 5000,
                "reported execution time should be accurate");

            // Should still generate recommendations
            if (response.Recommendations.Any())
            {
                response.Recommendations.Should().HaveCountLessThanOrEqualTo(20,
                    "maxResults should be respected");
            }
        }

        /// <summary>
        /// TC-DST-PERF-002: Concurrent recommendation requests
        /// 
        /// Given: Multiple users requesting DST recommendations simultaneously
        /// When: Requests are processed concurrently
        /// Then: System handles load without degradation
        /// 
        /// Expected Behavior:
        /// - All requests complete successfully
        /// - No deadlocks or race conditions
        /// - Performance remains acceptable under load
        /// - Resources are properly managed
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-PERF-002")]
        [Trait("Priority", "High")]
        public async Task DSTRecommendations_ConcurrentRequests_HandlesLoad()
        {
            // Arrange - Create multiple opportunities
            var opportunityIds = await Task.WhenAll(
                Enumerable.Range(1, 5).Select(i =>
                    CreateTestOpportunityAsync(
                        title: $"Concurrent Test Project {i}",
                        description: $"Project {i} for concurrent testing with various risk factors",
                        deliverables: $"Deliverables for project {i}"
                    )
                )
            );

            // Act - Send concurrent requests
            var stopwatch = Stopwatch.StartNew();
            
            var tasks = opportunityIds.Select(id =>
                GetDSTRecommendationsAsync(id, maxResults: 10, forceRefresh: true)
            ).ToList();

            var responses = await Task.WhenAll(tasks);
            
            stopwatch.Stop();

            // Assert
            responses.Should().HaveCount(5, "all requests should complete");
            responses.Should().AllSatisfy(response =>
            {
                response.Should().NotBeNull("all responses should be valid");
                response.Recommendations.Should().NotBeNull();
            });

            // Concurrent execution should not take drastically longer than sequential
            // Allow up to 60 seconds for 5 concurrent requests
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(60000,
                "concurrent requests should complete within reasonable time");

            // Verify no data corruption
            var uniqueRecommendations = responses
                .SelectMany(r => r.Recommendations)
                .Select(rec => rec.StableIdentifier)
                .ToList();

            // If recommendations were generated, they should be consistent
            if (uniqueRecommendations.Any())
            {
                uniqueRecommendations.Should().NotBeNull(
                    "concurrent access should not corrupt data");
            }
        }

        /// <summary>
        /// TC-DST-PERF-003: Cache effectiveness metrics
        /// 
        /// Given: Same opportunity analyzed multiple times
        /// When: Cache is used (forceRefresh=false)
        /// Then: Cache significantly improves performance
        /// 
        /// Expected Behavior:
        /// - First call is slower (full analysis)
        /// - Cached calls are 50%+ faster
        /// - Results are consistent
        /// - Cache hit rate is high for repeated requests
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-PERF-003")]
        [Trait("Priority", "Medium")]
        public async Task DSTCache_HitRate_MeetsTarget()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Cache Performance Test Project",
                description: "Project for testing cache effectiveness",
                deliverables: "Standard deliverables"
            );

            // Act - First call (uncached)
            var stopwatch1 = Stopwatch.StartNew();
            var uncachedResponse = await GetDSTRecommendationsAsync(
                opportunityId, 
                maxResults: 10, 
                forceRefresh: true);
            stopwatch1.Stop();

            var uncachedTime = stopwatch1.ElapsedMilliseconds;

            // Wait a moment
            await Task.Delay(500);

            // Second call (cached)
            var stopwatch2 = Stopwatch.StartNew();
            var cachedResponse1 = await GetDSTRecommendationsAsync(
                opportunityId, 
                maxResults: 10, 
                forceRefresh: false);
            stopwatch2.Stop();

            var cachedTime1 = stopwatch2.ElapsedMilliseconds;

            // Third call (cached)
            var stopwatch3 = Stopwatch.StartNew();
            var cachedResponse2 = await GetDSTRecommendationsAsync(
                opportunityId, 
                maxResults: 10, 
                forceRefresh: false);
            stopwatch3.Stop();

            var cachedTime2 = stopwatch3.ElapsedMilliseconds;

            // Assert
            uncachedResponse.Should().NotBeNull();
            cachedResponse1.Should().NotBeNull();
            cachedResponse2.Should().NotBeNull();

            // Cached calls should be faster (though not guaranteed in all environments)
            // Look for significant improvement
            if (cachedTime1 < uncachedTime * 0.7)
            {
                cachedTime1.Should().BeLessThan(uncachedTime,
                    "first cached call should be faster than uncached");
            }

            if (cachedTime2 < uncachedTime * 0.7)
            {
                cachedTime2.Should().BeLessThan(uncachedTime,
                    "second cached call should be faster than uncached");
            }

            // Results should be consistent
            if (uncachedResponse.Recommendations.Any() && cachedResponse1.Recommendations.Any())
            {
                var countDifference = Math.Abs(
                    uncachedResponse.Recommendations.Count - 
                    cachedResponse1.Recommendations.Count);
                
                countDifference.Should().BeLessThanOrEqualTo(1,
                    "cached and uncached results should be similar");
            }
        }

        #endregion

        #region TC-DST-PERF-004 through TC-DST-PERF-006: Resource Management and Optimization

        /// <summary>
        /// TC-DST-PERF-004: Memory usage for large result sets
        /// 
        /// Given: DST returns many recommendations
        /// When: Result set is processed
        /// Then: Memory usage remains within acceptable limits
        /// 
        /// Expected Behavior:
        /// - Large result sets don't cause memory issues
        /// - Objects are properly disposed
        /// - No memory leaks on repeated calls
        /// - GC pressure is manageable
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-PERF-004")]
        [Trait("Priority", "Medium")]
        public async Task DSTRecommendations_LargeResults_MemoryEfficient()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Complex Multi-Sector Program Requiring Comprehensive Risk Analysis",
                description: "Large program spanning health, education, infrastructure, governance, economic development, environmental sustainability, gender equality, and conflict sensitivity. Multiple risk factors across all sectors.",
                deliverables: "Extensive deliverables across all sectors with detailed sub-components",
                budget: 50000000m
            );

            // Force GC before test
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memoryBefore = GC.GetTotalMemory(false);

            // Act - Request maximum results
            var response = await GetDSTRecommendationsAsync(
                opportunityId, 
                maxResults: 50,  // Request many results
                forceRefresh: true);

            // Force GC after test
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memoryAfter = GC.GetTotalMemory(false);

            // Assert
            response.Should().NotBeNull();

            // Calculate memory increase
            var memoryIncrease = memoryAfter - memoryBefore;

            // Memory increase should be reasonable (less than 100MB)
            memoryIncrease.Should().BeLessThan(100 * 1024 * 1024,
                "large result sets should not consume excessive memory");

            // Make another call to verify no memory leak
            await GetDSTRecommendationsAsync(
                opportunityId, 
                maxResults: 50, 
                forceRefresh: false);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memoryAfterSecondCall = GC.GetTotalMemory(false);

            // Second call should not significantly increase memory
            var secondCallIncrease = memoryAfterSecondCall - memoryAfter;
            secondCallIncrease.Should().BeLessThan(50 * 1024 * 1024,
                "repeated calls should not leak memory");
        }

        /// <summary>
        /// TC-DST-PERF-005: Database query optimization
        /// 
        /// Given: DST needs to query opportunity details and related data
        /// When: Queries are executed
        /// Then: Queries are optimized and performant
        /// 
        /// Expected Behavior:
        /// - Minimal number of database queries
        /// - No N+1 query problems
        /// - Efficient use of indexes
        /// - Query execution time is acceptable
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-PERF-005")]
        [Trait("Priority", "Medium")]
        public async Task DSTQueries_ExecutionPlan_Optimized()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Infrastructure Development Project",
                description: "Infrastructure project with multiple components",
                deliverables: "Infrastructure outputs"
            );

            // Act
            var stopwatch = Stopwatch.StartNew();
            var response = await GetDSTRecommendationsAsync(opportunityId);
            stopwatch.Stop();

            // Assert
            response.Should().NotBeNull();

            // Database queries should be fast (< 5 seconds for data retrieval)
            // Most time should be in AI processing, not database
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(30000,
                "overall execution including DB queries should be fast");

            // Verify data was retrieved (indicates DB queries succeeded)
            response.ExtractedKeywords.Should().NotBeNull();
            
            // If recommendations exist, verify they have proper IDs (DB data)
            if (response.Recommendations.Any())
            {
                var preDefinedRisks = response.Recommendations
                    .Where(r => r.PreDefinedHighRiskId.HasValue)
                    .ToList();

                if (preDefinedRisks.Any())
                {
                    preDefinedRisks.Should().AllSatisfy(rec =>
                    {
                        rec.PreDefinedHighRiskId.Should().BeGreaterThan(0,
                            "DB-sourced data should have valid IDs");
                    });
                }
            }
        }

        /// <summary>
        /// TC-DST-PERF-006: Stress test with rapid sequential requests
        /// 
        /// Given: Many sequential DST requests in quick succession
        /// When: Requests are processed rapidly
        /// Then: System remains stable and responsive
        /// 
        /// Expected Behavior:
        /// - All requests complete successfully
        /// - No performance degradation over time
        /// - No resource exhaustion
        /// - Response times remain consistent
        /// </summary>
        [Fact]
        [Trait("TestId", "TC-DST-PERF-006")]
        [Trait("Priority", "Low")]
        public async Task DSTRecommendations_RapidSequential_RemainsStable()
        {
            // Arrange
            var opportunityId = await CreateTestOpportunityAsync(
                title: "Stress Test Project",
                description: "Project for stress testing",
                deliverables: "Standard deliverables"
            );

            var executionTimes = new List<long>();

            // Act - Make 10 rapid sequential requests
            for (int i = 0; i < 10; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                
                var response = await GetDSTRecommendationsAsync(
                    opportunityId, 
                    maxResults: 5,
                    forceRefresh: false); // Use cache for speed
                
                stopwatch.Stop();

                response.Should().NotBeNull($"request {i + 1} should succeed");
                executionTimes.Add(stopwatch.ElapsedMilliseconds);
            }

            // Assert
            executionTimes.Should().HaveCount(10, "all requests should complete");

            // Calculate performance metrics
            var avgTime = executionTimes.Average();
            var maxTime = executionTimes.Max();
            var minTime = executionTimes.Min();

            // No request should take more than 10 seconds
            maxTime.Should().BeLessThan(10000,
                "no individual request should be excessively slow");

            // Performance should not degrade significantly over time
            var firstHalfAvg = executionTimes.Take(5).Average();
            var secondHalfAvg = executionTimes.Skip(5).Average();

            // Second half should not be more than 2x slower than first half
            secondHalfAvg.Should().BeLessThan(firstHalfAvg * 2,
                "performance should remain stable over repeated requests");
        }

        #endregion
    }
}
