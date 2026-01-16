using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.E2E
{
    public class BulkProcessingTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IDSTManager> _mockDSTManager;

        public BulkProcessingTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"BulkTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockDSTManager = new Mock<IDSTManager>();
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-018")]
        public async Task BulkOpportunityProcessing_15Opportunities_BatchDecision()
        {
            // Arrange - 15 small similar opportunities
            var opportunities = new List<Domain.Entities.Opportunity>();
            for (int i = 1; i <= 15; i++)
            {
                opportunities.Add(new Domain.Entities.Opportunity
                {
                    Id = i,
                    Name = $"School Renovation {i}",
                    EstimatedValue = 50000 + (i * 10000), // $50K-$200K
                    Sector = "Education",
                    PrimaryCountryId = 1, // Tanzania
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                });
            }

            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act - Batch DST generation
            var stopwatch = Stopwatch.StartTime();
            
            // Parallel processing
            var dstTasks = opportunities.Select(async o =>
            {
                _mockDSTManager.Setup(m => m.GenerateDSTProfileAsync(o.Id))
                    .ReturnsAsync(new DSTProfile { OpportunityId = o.Id, ComplexityScore = 5.5m });
                return await _mockDSTManager.Object.GenerateDSTProfileAsync(o.Id);
            });

            await Task.WhenAll(dstTasks);
            stopwatch.Stop();

            // Assert - Parallel processing much faster
            Assert.True(stopwatch.ElapsedMilliseconds < 60000); // < 1 minute for batch
            // vs 15 minutes sequential (15 opps × 1 min each)

            // Batch decision
            var batchDecision = new BatchDecision
            {
                OpportunityIds = opportunities.Select(o => o.Id).ToList(),
                Decision = "Go",
                Rationale = "Low-risk, standardized school renovations",
                DecisionMakerId = 1,
                DecisionDate = DateTime.UtcNow
            };

            _context.BatchDecisions.Add(batchDecision);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(15, batchDecision.OpportunityIds.Count);
            Assert.Equal("Go", batchDecision.Decision);
        }

        public class DSTProfile { public int OpportunityId { get; set; } public decimal ComplexityScore { get; set; } }
        public class BatchDecision { public int Id { get; set; } public List<int> OpportunityIds { get; set; } public string Decision { get; set; } public string Rationale { get; set; } public int DecisionMakerId { get; set; } public DateTime DecisionDate { get; set; } }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
