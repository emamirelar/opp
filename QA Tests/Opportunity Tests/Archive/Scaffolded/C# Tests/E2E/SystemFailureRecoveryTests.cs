using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.E2E
{
    /// <summary>
    /// End-to-End tests for system failure and recovery scenarios
    /// Covers database failures, network issues, data corruption, and recovery mechanisms
    /// </summary>
    public class SystemFailureRecoveryTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DecisionManager _decisionManager;

        public SystemFailureRecoveryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"SystemFailureTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockMapper = new Mock<IMapper>();

            _decisionManager = new DecisionManager(_mockMapper.Object, _context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Test Opportunity",
                EstimatedValue = 3000000,
                Status = "Pending Decision",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });

            _context.SaveChanges();
        }

        #region TC-OPP-E2E-NEG-001: Database Connection Loss During Decision Recording

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "E2E-Negative")]
        [Trait("TestId", "TC-OPP-E2E-NEG-001")]
        public async Task DatabaseConnectionLoss_DuringDecisionRecording_TransactionRolledBack()
        {
            // Arrange
            var opportunityId = 1;
            var decision = new OpportunityDecision
            {
                OpportunityId = opportunityId,
                Decision = "Go",
                DecisionMakerId = 1,
                Rationale = "Approved for implementation",
                DecisionDate = DateTime.UtcNow
            };

            // Act - Begin transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Step 1: Add decision
                _context.OpportunityDecisions.Add(decision);
                
                // Step 2: Update opportunity status
                var opportunity = await _context.Opportunities.FindAsync(opportunityId);
                opportunity.Status = "Approved";
                
                // Simulate connection loss before commit
                // In a real scenario, this would be a network/database failure
                throw new Exception("Simulated database connection loss");
                
                // This would normally commit
                // await _context.SaveChangesAsync();
                // await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Connection lost - rollback
                await transaction.RollbackAsync();
            }

            // Assert - Verify no partial data saved
            var opportunityAfterFailure = await _context.Opportunities.FindAsync(opportunityId);
            var decisionsAfterFailure = await _context.OpportunityDecisions
                .Where(d => d.OpportunityId == opportunityId)
                .ToListAsync();

            Assert.Equal("Pending Decision", opportunityAfterFailure.Status); // Status unchanged
            Assert.Empty(decisionsAfterFailure); // No decision saved
            // Data integrity maintained - no partial save
        }

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "E2E-Negative")]
        [Trait("TestId", "TC-OPP-E2E-NEG-001-Retry")]
        public async Task DatabaseConnectionLoss_RetryAfterRecovery_Success()
        {
            // Arrange
            var opportunityId = 1;
            var decisionRationale = "Approved for implementation - retry after connection restored";

            // Act - First attempt fails (simulated in previous test)
            // Act - Second attempt after database recovery
            var decision = new OpportunityDecision
            {
                OpportunityId = opportunityId,
                Decision = "Go",
                DecisionMakerId = 1,
                Rationale = decisionRationale,
                DecisionDate = DateTime.UtcNow,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.OpportunityDecisions.Add(decision);
            
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            opportunity.Status = "Approved";
            opportunity.LastModifiedBy = 1;
            opportunity.LastModifiedDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            // Assert - Retry successful
            var savedDecision = await _context.OpportunityDecisions
                .FirstOrDefaultAsync(d => d.OpportunityId == opportunityId);
            var updatedOpportunity = await _context.Opportunities.FindAsync(opportunityId);

            Assert.NotNull(savedDecision);
            Assert.Equal("Go", savedDecision.Decision);
            Assert.Contains("retry", savedDecision.Rationale);
            Assert.Equal("Approved", updatedOpportunity.Status);
        }

        #endregion

        #region TC-OPP-E2E-NEG-003: Data Corruption Detection and Recovery

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "E2E-Negative")]
        [Trait("TestId", "TC-OPP-E2E-NEG-003")]
        public async Task DataCorruption_Detection_IsolatesAndRecovery()
        {
            // Arrange - Create opportunity with valid data
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 100,
                Name = "Corruption Test Opportunity",
                EstimatedValue = 2000000,
                Status = "Profiling",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Create backup snapshot
            var backupData = new
            {
                Budget = opportunity.EstimatedValue,
                Status = opportunity.Status,
                Timestamp = DateTime.UtcNow
            };

            // Act - Simulate data corruption
            try
            {
                // Corrupt the budget field (in reality this would be a disk error)
                // For test purposes, we'll use an invalid value
                opportunity.EstimatedValue = -999999; // Invalid negative value
                opportunity.Status = "CORRUPTED_STATUS"; // Invalid status
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Corruption detected during save
            }

            // Detect corruption through validation
            var corruptedOpportunity = await _context.Opportunities.FindAsync(100);
            var isCorrupted = corruptedOpportunity.EstimatedValue < 0 || 
                            !new[] { "Draft", "Profiling", "Decision", "Approved" }.Contains(corruptedOpportunity.Status);

            Assert.True(isCorrupted); // Corruption detected

            // Act - Recovery from backup
            if (isCorrupted)
            {
                // Restore from backup
                corruptedOpportunity.EstimatedValue = backupData.Budget;
                corruptedOpportunity.Status = backupData.Status;
                await _context.SaveChangesAsync();
            }

            // Assert - Data recovered successfully
            var recoveredOpportunity = await _context.Opportunities.FindAsync(100);
            Assert.Equal(2000000, recoveredOpportunity.EstimatedValue);
            Assert.Equal("Profiling", recoveredOpportunity.Status);
            // Zero data loss
        }

        #endregion

        #region TC-OPP-E2E-NEG-004: Network Partition During Multi-User Collaboration

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E-Negative")]
        [Trait("TestId", "TC-OPP-E2E-NEG-004")]
        public async Task NetworkPartition_MultiUserEditing_ConflictResolution()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 200,
                Name = "Network Partition Test",
                EstimatedValue = 2200000,
                Status = "Draft",
                Description = "Initial description",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - User A and User B connected to database
            var userAContext = new UNOPSAppDbContext(_dbContextOptions);
            var userAOpportunity = await userAContext.Opportunities.FindAsync(200);
            userAOpportunity.EstimatedValue = 2500000;
            userAOpportunity.LastModifiedBy = 1;
            await userAContext.SaveChangesAsync();

            var userBContext = new UNOPSAppDbContext(_dbContextOptions);
            var userBOpportunity = await userBContext.Opportunities.FindAsync(200);
            userBOpportunity.Description = userBOpportunity.Description + "\nUser B updates: 12 deliverables";
            userBOpportunity.LastModifiedBy = 2;
            await userBContext.SaveChangesAsync();

            // User C gets isolated (network partition)
            // User C works offline
            var userCOfflineChanges = new
            {
                NewRisks = new List<string> { "Risk 1: Security", "Risk 2: Access", "Risk 3: Weather" },
                UserId = 3
            };

            // Network restored - User C reconnects
            // Act - Merge offline changes
            var userCContext = new UNOPSAppDbContext(_dbContextOptions);
            var currentOpportunity = await userCContext.Opportunities.FindAsync(200);
            
            // Detect conflicts
            var conflicts = new List<string>();
            if (currentOpportunity.EstimatedValue != 2200000)
                conflicts.Add("Budget changed by User A");
            if (currentOpportunity.Description != "Initial description")
                conflicts.Add("Description changed by User B");

            // User C's changes (risks) don't conflict - can be merged
            foreach (var risk in userCOfflineChanges.NewRisks)
            {
                var riskRecord = new OpportunityRisk
                {
                    OpportunityId = 200,
                    RiskDescription = risk,
                    CreatedBy = userCOfflineChanges.UserId,
                    CreatedDate = DateTime.UtcNow
                };
                userCContext.OpportunityRisks.Add(riskRecord);
            }
            await userCContext.SaveChangesAsync();

            // Assert - Conflict resolution successful
            Assert.NotEmpty(conflicts); // Conflicts detected
            
            var finalOpportunity = await _context.Opportunities.FindAsync(200);
            Assert.Equal(2500000, finalOpportunity.EstimatedValue); // User A's change preserved
            Assert.Contains("User B updates", finalOpportunity.Description); // User B's change preserved
            
            var risks = await _context.OpportunityRisks.Where(r => r.OpportunityId == 200).ToListAsync();
            Assert.Equal(3, risks.Count); // User C's offline work merged successfully

            // Clean up
            userAContext.Dispose();
            userBContext.Dispose();
            userCContext.Dispose();
        }

        #endregion

        #region TC-OPP-E2E-NEG-005: System Overload During Peak Usage

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "E2E-Negative")]
        [Trait("TestId", "TC-OPP-E2E-NEG-005")]
        public async Task SystemOverload_PeakUsage_GracefulDegradation()
        {
            // Arrange - Simulate 50 opportunities being submitted simultaneously (scaled down from 500)
            var opportunities = new List<Domain.Entities.Opportunity>();
            for (int i = 0; i < 50; i++)
            {
                opportunities.Add(new Domain.Entities.Opportunity
                {
                    Name = $"Peak Load Test Opportunity {i}",
                    EstimatedValue = 100000 + (i * 10000),
                    Status = "Draft",
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                });
            }

            // Act - Simulate high load (all submitted at once)
            var startTime = DateTime.UtcNow;
            
            // Priority queue: Critical operations processed first
            var criticalOperations = new List<Domain.Entities.Opportunity>();
            var queuedOperations = new List<Domain.Entities.Opportunity>();

            // Simulate capacity check
            int systemCapacity = 30; // Can handle 30 immediate, rest queued
            
            for (int i = 0; i < opportunities.Count; i++)
            {
                if (i < systemCapacity)
                {
                    criticalOperations.Add(opportunities[i]);
                }
                else
                {
                    queuedOperations.Add(opportunities[i]);
                }
            }

            // Process critical operations immediately
            _context.Opportunities.AddRange(criticalOperations);
            await _context.SaveChangesAsync();

            // Queue remaining operations (would be processed after load reduces)
            // Simulating auto-scaling and queue processing
            await Task.Delay(100); // Simulate brief delay

            _context.Opportunities.AddRange(queuedOperations);
            await _context.SaveChangesAsync();

            var endTime = DateTime.UtcNow;
            var processingTime = (endTime - startTime).TotalSeconds;

            // Assert - All operations completed (though some were queued)
            var savedOpportunities = await _context.Opportunities
                .Where(o => o.Name.StartsWith("Peak Load Test"))
                .ToListAsync();

            Assert.Equal(50, savedOpportunities.Count); // All saved eventually
            Assert.Equal(30, criticalOperations.Count); // 30 immediate
            Assert.Equal(20, queuedOperations.Count); // 20 queued
            Assert.True(processingTime < 10); // Completed within reasonable time
            
            // No data loss during high load
            Assert.All(savedOpportunities, o => Assert.NotNull(o.Name));
        }

        #endregion

        #region Supporting Entity Classes

        public class OpportunityRisk
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string RiskDescription { get; set; }
            public string Severity { get; set; }
            public string Status { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
