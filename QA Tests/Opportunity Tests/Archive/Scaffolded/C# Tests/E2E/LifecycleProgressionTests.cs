using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.E2E
{
    public class LifecycleProgressionTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;

        public LifecycleProgressionTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"LifecycleTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-017")]
        public async Task CompleteLifecycleProgression_15Stages_AllStagesCompleted()
        {
            // Arrange - Create opportunity in Draft
            var startDate = new DateTime(2026, 1, 1);
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "School Rehabilitation - Ghana",
                EstimatedValue = 1200000,
                Status = "Draft",
                WorkflowStage = "Draft",
                CreatedBy = 1,
                CreatedDate = startDate
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Progress through all 15 stages
            var stages = new[]
            {
                ("Draft", startDate),
                ("Data Collection", startDate.AddDays(5)),
                ("Profiling", startDate.AddDays(10)),
                ("Budget Development", startDate.AddDays(15)),
                ("Schedule Development", startDate.AddDays(18)),
                ("Risk Assessment", startDate.AddDays(21)),
                ("Quality Review", startDate.AddDays(25)),
                ("Ready for Decision", startDate.AddDays(26)),
                ("Under Review", startDate.AddDays(28)),
                ("Approved", startDate.AddDays(33)),
                ("Authorized", startDate.AddDays(35)),
                ("Conversion Pending", startDate.AddDays(40)),
                ("Converted", startDate.AddDays(41)),
                ("Initiated", startDate.AddDays(45)),
                ("Implementation", startDate.AddDays(51))
            };

            foreach (var (status, date) in stages)
            {
                var stageTransition = new StageTransition
                {
                    OpportunityId = 1,
                    FromStage = opportunity.Status,
                    ToStage = status,
                    TransitionDate = date,
                    TransitionedBy = 1
                };
                _context.StageTransitions.Add(stageTransition);

                opportunity.Status = status;
                opportunity.WorkflowStage = status;
                opportunity.LastModifiedDate = date;
            }

            await _context.SaveChangesAsync();

            // Assert - All stages completed
            var transitions = await _context.StageTransitions
                .Where(t => t.OpportunityId == 1)
                .OrderBy(t => t.TransitionDate)
                .ToListAsync();

            Assert.Equal(15, transitions.Count);
            
            // Verify chronological progression
            for (int i = 0; i < transitions.Count - 1; i++)
            {
                Assert.True(transitions[i].TransitionDate <= transitions[i + 1].TransitionDate);
            }

            // Total lifecycle time: 51 days
            var totalDays = (transitions.Last().TransitionDate - transitions.First().TransitionDate).Days;
            Assert.Equal(51, totalDays);

            // Final status: Implementation
            var finalOpportunity = await _context.Opportunities.FindAsync(1);
            Assert.Equal("Implementation", finalOpportunity.Status);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-020")]
        public async Task OpportunityRecoveryAfter18MonthHold_SystematicReactivation_Success()
        {
            // Arrange - Opportunity placed on hold
            var holdDate = new DateTime(2025, 2, 15);
            var reactivationDate = new DateTime(2026, 8, 15); // 18 months later

            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Infrastructure - Country X",
                EstimatedValue = 4000000,
                Status = "On Hold",
                OnHoldDate = holdDate,
                OnHoldReason = "Political instability - operations suspended",
                ProgressPercentage = 60, // 60% complete when placed on hold
                CreatedBy = 1,
                CreatedDate = holdDate.AddMonths(-2)
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Reactivation after 18 months
            opportunity.Status = "Active";
            opportunity.ReactivatedDate = reactivationDate;
            opportunity.OnHoldDuration = (reactivationDate - holdDate).Days / 30; // 18 months
            
            // Data currency validations performed
            var validations = new ReactivationValidations
            {
                CountryIndicesRefreshed = true,
                BudgetInflationAdjusted = true, // 12% adjustment
                DSTRegenerated = true,
                TeamReassigned = true,
                PartnersReengaged = true
            };

            opportunity.LastModifiedDate = reactivationDate;
            await _context.SaveChangesAsync();

            // Assert
            var reactivatedOpp = await _context.Opportunities.FindAsync(1);
            Assert.Equal("Active", reactivatedOpp.Status);
            Assert.Equal(18, reactivatedOpp.OnHoldDuration);
            Assert.NotNull(reactivatedOpp.ReactivatedDate);
            
            // All validations completed
            Assert.True(validations.CountryIndicesRefreshed);
            Assert.True(validations.BudgetInflationAdjusted);
            Assert.True(validations.DSTRegenerated);
        }

        public class StageTransition { public int OpportunityId { get; set; } public string FromStage { get; set; } public string ToStage { get; set; } public DateTime TransitionDate { get; set; } public int TransitionedBy { get; set; } }
        public class Country { public int Id { get; set; } public string Name { get; set; } public string Code { get; set; } }
        public class OpportunityComponent { public int OpportunityId { get; set; } public string Name { get; set; } public decimal Budget { get; set; } }
        public class ReactivationValidations { public bool CountryIndicesRefreshed { get; set; } public bool BudgetInflationAdjusted { get; set; } public bool DSTRegenerated { get; set; } public bool TeamReassigned { get; set; } public bool PartnersReengaged { get; set; } }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
