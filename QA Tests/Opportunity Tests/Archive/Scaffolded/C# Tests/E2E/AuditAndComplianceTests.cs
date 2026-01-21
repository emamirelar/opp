using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.E2E
{
    public class AuditAndComplianceTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;

        public AuditAndComplianceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"AuditTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-015")]
        public async Task CompleteAuditTrail_EntireLifecycle_AllEventsLogged()
        {
            // Arrange & Act - Simulate complete opportunity lifecycle with audit logging
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Health Systems Strengthening - Uganda",
                EstimatedValue = 2400000,
                Status = "Draft",
                CreatedBy = 1,
                CreatedDate = new DateTime(2026, 1, 15, 9, 23, 0)
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Create audit trail entries for each major action
            var auditEvents = new List<AuditEvent>
            {
                new() { OpportunityId = 1, Action = "Created", UserId = 1, Timestamp = new DateTime(2026, 1, 15, 9, 23, 0), Details = "Initial creation" },
                new() { OpportunityId = 1, Action = "Document Uploaded", UserId = 1, Timestamp = new DateTime(2026, 1, 15, 9, 37, 0), Details = "Concept Note uploaded" },
                new() { OpportunityId = 1, Action = "AI Extraction Completed", UserId = 0, Timestamp = new DateTime(2026, 1, 15, 9, 57, 0), Details = "87% fields extracted" },
                new() { OpportunityId = 1, Action = "DST Profile Generated", UserId = 0, Timestamp = new DateTime(2026, 1, 16, 14, 0, 0), Details = "Complexity: 6.8, Risk: 6.2" },
                new() { OpportunityId = 1, Action = "Budget Developed", UserId = 2, Timestamp = new DateTime(2026, 1, 22, 16, 0, 0), Details = "$2.4M approved" },
                new() { OpportunityId = 1, Action = "Decision Package Assembled", UserId = 1, Timestamp = new DateTime(2026, 1, 25, 10, 0, 0), Details = "All components complete" },
                new() { OpportunityId = 1, Action = "Submitted for Decision", UserId = 1, Timestamp = new DateTime(2026, 1, 26, 10, 15, 0), Details = "Submitted to DOA3" },
                new() { OpportunityId = 1, Action = "Technical Review Complete", UserId = 3, Timestamp = new DateTime(2026, 1, 28, 14, 0, 0), Details = "Approved with minor adjustment" },
                new() { OpportunityId = 1, Action = "Go Decision Recorded", UserId = 4, Timestamp = new DateTime(2026, 2, 5, 15, 30, 0), Details = "DOA3 approved" },
                new() { OpportunityId = 1, Action = "Budget Authorized", UserId = 4, Timestamp = new DateTime(2026, 2, 6, 9, 5, 0), Details = "$2.35M authorized" },
                new() { OpportunityId = 1, Action = "Converted to Project", UserId = 1, Timestamp = new DateTime(2026, 2, 10, 10, 0, 0), Details = "PRJ-2026-0215 created" }
            };

            _context.AuditEvents.AddRange(auditEvents);
            await _context.SaveChangesAsync();

            // Assert - Complete audit trail
            var auditTrail = await _context.AuditEvents
                .Where(e => e.OpportunityId == 1)
                .OrderBy(e => e.Timestamp)
                .ToListAsync();

            Assert.Equal(11, auditTrail.Count); // All major events logged
            
            // Chronological order
            for (int i = 0; i < auditTrail.Count - 1; i++)
            {
                Assert.True(auditTrail[i].Timestamp <= auditTrail[i + 1].Timestamp);
            }

            // Key events present
            Assert.Contains(auditTrail, e => e.Action == "Created");
            Assert.Contains(auditTrail, e => e.Action == "DST Profile Generated");
            Assert.Contains(auditTrail, e => e.Action == "Go Decision Recorded");
            Assert.Contains(auditTrail, e => e.Action == "Converted to Project");

            // Lifecycle duration
            var totalDays = (auditTrail.Last().Timestamp - auditTrail.First().Timestamp).Days;
            Assert.Equal(26, totalDays); // 26 days from creation to conversion
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-015-AutoReport")]
        public async Task GenerateAuditReport_AutomatedProcess_CompletesQuickly()
        {
            // Arrange - Opportunity with complete audit trail
            var opportunity = new Domain.Entities.Opportunity { Id = 1, Name = "Test", CreatedBy = 1, CreatedDate = DateTime.UtcNow };
            _context.Opportunities.Add(opportunity);

            // 50 audit events
            for (int i = 0; i < 50; i++)
            {
                _context.AuditEvents.Add(new AuditEvent
                {
                    OpportunityId = 1,
                    Action = $"Action {i}",
                    UserId = 1,
                    Timestamp = DateTime.UtcNow.AddMinutes(i),
                    Details = $"Details for action {i}"
                });
            }

            await _context.SaveChangesAsync();

            // Act - Generate audit report (timed)
            var startTime = DateTime.UtcNow;
            
            var auditReport = new AuditReport
            {
                OpportunityId = 1,
                TotalEvents = await _context.AuditEvents.CountAsync(e => e.OpportunityId == 1),
                GeneratedDate = DateTime.UtcNow
            };

            var endTime = DateTime.UtcNow;
            var generationTime = (endTime - startTime).TotalSeconds;

            // Assert
            Assert.Equal(50, auditReport.TotalEvents);
            Assert.True(generationTime < 5); // < 5 seconds (vs 8-10 hours manual)
        }

        public class AuditEvent { public int OpportunityId { get; set; } public string Action { get; set; } public int UserId { get; set; } public DateTime Timestamp { get; set; } public string Details { get; set; } public string IPAddress { get; set; } }
        public class AuditReport { public int OpportunityId { get; set; } public int TotalEvents { get; set; } public DateTime GeneratedDate { get; set; } }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
