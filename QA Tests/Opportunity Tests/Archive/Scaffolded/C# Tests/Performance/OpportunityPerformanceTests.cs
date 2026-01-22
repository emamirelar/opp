using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Performance
{
    /// <summary>
    /// Performance and scalability tests for Opportunity features
    /// Tests system behavior under load and with large datasets
    /// </summary>
    public class OpportunityPerformanceTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly OpportunityManager _manager;

        public OpportunityPerformanceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"PerfTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _manager = new OpportunityManager(_context);
        }

        #region TC-OPP-PERF-001: Bulk Opportunity Creation

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-PERF-001")]
        public async Task CreateOpportunities_1000Records_CompletesUnder10Seconds()
        {
            // Arrange
            var opportunities = new List<Domain.Entities.Opportunity>();
            for (int i = 1; i <= 1000; i++)
            {
                opportunities.Add(new Domain.Entities.Opportunity
                {
                    Name = $"Performance Test Opportunity {i}",
                    EstimatedValue = 1000000 + (i * 1000),
                    Timeline = 12 + (i % 48),
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
                $"Creation took {stopwatch.ElapsedMilliseconds}ms, expected < 10000ms");
            
            var count = await _context.Opportunities.CountAsync();
            Assert.Equal(1000, count);
        }

        #endregion

        #region TC-OPP-PERF-002: Large Dataset Search Performance

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-PERF-002")]
        public async Task SearchOpportunities_In10000Records_CompletesUnder2Seconds()
        {
            // Arrange - Create 10,000 opportunities
            var opportunities = new List<Domain.Entities.Opportunity>();
            for (int i = 1; i <= 10000; i++)
            {
                opportunities.Add(new Domain.Entities.Opportunity
                {
                    Name = $"Opportunity {i}",
                    Description = i % 100 == 0 ? "Infrastructure Project" : "Regular Project",
                    EstimatedValue = 500000 + (i * 100),
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                });
            }
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act - Search with filter
            var stopwatch = Stopwatch.StartNew();
            var results = await _context.Opportunities
                .Where(o => o.Description.Contains("Infrastructure"))
                .ToListAsync();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 2000,
                $"Search took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
            
            Assert.Equal(100, results.Count); // 100 infrastructure projects
        }

        #endregion

        #region TC-OPP-PERF-003: Pagination Performance

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-PERF-003")]
        public async Task GetOpportunities_PaginatedLargeDataset_EfficientRetrieval()
        {
            // Arrange - Create 5,000 opportunities
            var opportunities = Enumerable.Range(1, 5000).Select(i => new Domain.Entities.Opportunity
            {
                Name = $"Opportunity {i}",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow.AddDays(-i)
            }).ToList();
            
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act - Retrieve page 50 (records 2450-2500)
            var stopwatch = Stopwatch.StartNew();
            var page50 = await _context.Opportunities
                .OrderByDescending(o => o.CreatedDate)
                .Skip(2450)
                .Take(50)
                .ToListAsync();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 500,
                $"Pagination took {stopwatch.ElapsedMilliseconds}ms, expected < 500ms");
            
            Assert.Equal(50, page50.Count);
        }

        #endregion

        #region TC-OPP-PERF-004: Bulk Update Performance

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-PERF-004")]
        public async Task UpdateOpportunities_500Records_CompletesUnder5Seconds()
        {
            // Arrange - Create 500 opportunities
            var opportunities = Enumerable.Range(1, 500).Select(i => new Domain.Entities.Opportunity
            {
                Name = $"Opportunity {i}",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            }).ToList();
            
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act - Bulk update all to "Under Review"
            var stopwatch = Stopwatch.StartNew();
            var allOpportunities = await _context.Opportunities.ToListAsync();
            foreach (var opp in allOpportunities)
            {
                opp.Status = "Under Review";
                opp.LastModifiedDate = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 5000,
                $"Bulk update took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
            
            var updatedCount = await _context.Opportunities
                .CountAsync(o => o.Status == "Under Review");
            Assert.Equal(500, updatedCount);
        }

        #endregion

        #region TC-OPP-PERF-005: Complex Query Performance

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-PERF-005")]
        public async Task ComplexQuery_WithMultipleJoins_CompletesUnder3Seconds()
        {
            // Arrange - Create opportunities with related data
            for (int i = 1; i <= 1000; i++)
            {
                var opportunity = new Domain.Entities.Opportunity
                {
                    Name = $"Opportunity {i}",
                    EstimatedValue = 1000000 + (i * 1000),
                    Status = i % 2 == 0 ? "Approved" : "Draft",
                    PrimaryCountryId = (i % 10) + 1,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                };
                _context.Opportunities.Add(opportunity);
            }
            await _context.SaveChangesAsync();

            // Act - Complex query with grouping and aggregation
            var stopwatch = Stopwatch.StartNew();
            var statistics = await _context.Opportunities
                .Where(o => o.Status == "Approved")
                .GroupBy(o => o.PrimaryCountryId)
                .Select(g => new
                {
                    CountryId = g.Key,
                    Count = g.Count(),
                    TotalValue = g.Sum(o => o.EstimatedValue),
                    AverageValue = g.Average(o => o.EstimatedValue)
                })
                .ToListAsync();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 3000,
                $"Complex query took {stopwatch.ElapsedMilliseconds}ms, expected < 3000ms");
            
            Assert.NotEmpty(statistics);
        }

        #endregion

        #region TC-OPP-PERF-006: Memory Usage - Large Result Sets

        [Fact]
        [Trait("Category", "P3")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-PERF-006")]
        public async Task LoadOpportunities_2000Records_AcceptableMemoryUsage()
        {
            // Arrange - Create 2,000 opportunities
            var opportunities = Enumerable.Range(1, 2000).Select(i => new Domain.Entities.Opportunity
            {
                Name = $"Opportunity {i}",
                Description = $"Description for opportunity {i} with some additional text to increase size",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            }).ToList();
            
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act
            var beforeMemory = GC.GetTotalMemory(true);
            var allOpportunities = await _context.Opportunities.ToListAsync();
            var afterMemory = GC.GetTotalMemory(false);
            var memoryUsed = (afterMemory - beforeMemory) / 1024 / 1024; // MB

            // Assert
            Assert.Equal(2000, allOpportunities.Count);
            Assert.True(memoryUsed < 100, 
                $"Memory usage was {memoryUsed}MB, expected < 100MB");
        }

        #endregion

        #region TC-OPP-PERF-007: Concurrent Access Performance

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-PERF-007")]
        public async Task ConcurrentAccess_10SimultaneousReads_CompletesEfficiently()
        {
            // Arrange - Create 100 opportunities
            var opportunities = Enumerable.Range(1, 100).Select(i => new Domain.Entities.Opportunity
            {
                Name = $"Opportunity {i}",
                EstimatedValue = 1000000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            }).ToList();
            
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act - 10 concurrent read operations
            var stopwatch = Stopwatch.StartNew();
            var tasks = new List<Task<List<Domain.Entities.Opportunity>>>();
            
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(_context.Opportunities
                    .Where(o => o.Status == "Draft")
                    .ToListAsync());
            }

            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 2000,
                $"Concurrent access took {stopwatch.ElapsedMilliseconds}ms, expected < 2000ms");
            
            Assert.Equal(10, results.Length);
            Assert.All(results, r => Assert.Equal(100, r.Count));
        }

        #endregion

        #region TC-OPP-PERF-008: Index Performance Validation

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Performance")]
        [Trait("TestId", "TC-OPP-PERF-008")]
        public async Task QueryByIndexedColumns_FastRetrieval_Under1Second()
        {
            // Arrange - Create 5,000 opportunities
            var opportunities = Enumerable.Range(1, 5000).Select(i => new Domain.Entities.Opportunity
            {
                Name = $"Opportunity {i}",
                EstimatedValue = 1000000,
                Status = i % 5 == 0 ? "Approved" : "Draft", // 20% approved
                PrimaryCountryId = (i % 50) + 1,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            }).ToList();
            
            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act - Query by indexed Status column
            var stopwatch = Stopwatch.StartNew();
            var approvedOpportunities = await _context.Opportunities
                .Where(o => o.Status == "Approved")
                .ToListAsync();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 1000,
                $"Indexed query took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
            
            Assert.Equal(1000, approvedOpportunities.Count); // 20% of 5000
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
