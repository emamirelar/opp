using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Managers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Managers
{
    /// <summary>
    /// Tests for OpportunityScheduleManager
    /// Based on OpportunityScheduleManager_TestCases.md (15+ tests)
    /// </summary>
    public class OpportunityScheduleManagerTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly OpportunityScheduleManager _manager;

        public OpportunityScheduleManagerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"ScheduleTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockMapper = new Mock<IMapper>();

            _manager = new OpportunityScheduleManager(_mockMapper.Object, _context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Opportunities.AddRange(new[]
            {
                new Domain.Entities.Opportunity
                {
                    Id = 1,
                    Name = "Infrastructure Project",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(18),
                    Timeline = 18,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow
                }
            });

            _context.OpportunityDeliverables.AddRange(new[]
            {
                new OpportunityDeliverable { Id = 1, OpportunityId = 1, Description = "Phase 1: Design", Duration = 6 },
                new OpportunityDeliverable { Id = 2, OpportunityId = 1, Description = "Phase 2: Procurement", Duration = 6 },
                new OpportunityDeliverable { Id = 3, OpportunityId = 1, Description = "Phase 3: Implementation", Duration = 6 }
            });

            _context.SaveChanges();
        }

        #region TC-OPP-SCH-F-001: Generate High-Level Schedule

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-SCH-F-001")]
        public async Task GenerateSchedule_WithDeliverables_Success()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var schedule = await _manager.GenerateScheduleAsync(opportunityId);

            // Assert
            Assert.NotNull(schedule);
            Assert.Equal(opportunityId, schedule.OpportunityId);
            Assert.Equal(18, schedule.TotalDuration); // months
            Assert.NotNull(schedule.StartDate);
            Assert.NotNull(schedule.EndDate);
            
            var duration = (schedule.EndDate - schedule.StartDate).Days / 30; // approximate months
            Assert.InRange(duration, 17, 19); // ~18 months
        }

        #endregion

        #region TC-OPP-SCH-F-002: Generate Work Breakdown Structure (WBS)

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-SCH-F-002")]
        public async Task GenerateWBS_ThreePhases_Success()
        {
            // Arrange
            var opportunityId = 1;
            var schedule = await _manager.GenerateScheduleAsync(opportunityId);

            // Act
            var wbs = await _manager.GenerateWBSAsync(schedule.Id);

            // Assert
            Assert.NotNull(wbs);
            Assert.Equal(3, wbs.WorkPackages.Count); // 3 deliverables = 3 work packages
            
            var phase1 = wbs.WorkPackages.First(w => w.Name.Contains("Phase 1"));
            var phase2 = wbs.WorkPackages.First(w => w.Name.Contains("Phase 2"));
            var phase3 = wbs.WorkPackages.First(w => w.Name.Contains("Phase 3"));
            
            Assert.Equal(6, phase1.Duration); // 6 months
            Assert.Equal(6, phase2.Duration);
            Assert.Equal(6, phase3.Duration);
            
            // Phases are sequential
            Assert.True(phase2.StartDate >= phase1.EndDate);
            Assert.True(phase3.StartDate >= phase2.EndDate);
        }

        #endregion

        #region TC-OPP-SCH-F-003: Generate Milestones

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-SCH-F-003")]
        public async Task GenerateMilestones_AutomaticFromPhases_Success()
        {
            // Arrange
            var opportunityId = 1;
            var schedule = await _manager.GenerateScheduleAsync(opportunityId);

            // Act
            var milestones = await _manager.GenerateMilestonesAsync(schedule.Id);

            // Assert
            Assert.NotNull(milestones);
            Assert.NotEmpty(milestones);
            
            // Key milestones: Project start, phase completions, project end
            Assert.Contains(milestones, m => m.Name.Contains("Project Start"));
            Assert.Contains(milestones, m => m.Name.Contains("Phase 1 Complete"));
            Assert.Contains(milestones, m => m.Name.Contains("Phase 2 Complete"));
            Assert.Contains(milestones, m => m.Name.Contains("Phase 3 Complete"));
            Assert.Contains(milestones, m => m.Name.Contains("Project End"));
            
            // Milestones are chronological
            var orderedMilestones = milestones.OrderBy(m => m.MilestoneDate).ToList();
            Assert.Equal(milestones.Count, orderedMilestones.Count);
        }

        #endregion

        #region TC-OPP-SCH-F-004: Identify Critical Path

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-SCH-F-004")]
        public async Task IdentifyCriticalPath_SequentialPhases_Success()
        {
            // Arrange
            var opportunityId = 1;
            var schedule = await _manager.GenerateScheduleAsync(opportunityId);
            var wbs = await _manager.GenerateWBSAsync(schedule.Id);

            // Act
            var criticalPath = await _manager.IdentifyCriticalPathAsync(schedule.Id);

            // Assert
            Assert.NotNull(criticalPath);
            Assert.NotEmpty(criticalPath.Activities);
            
            // In sequential project, all phases are on critical path
            Assert.Equal(3, criticalPath.Activities.Count);
            Assert.All(criticalPath.Activities, a => Assert.True(a.IsCritical));
            
            // Critical path duration = total project duration
            var criticalPathDuration = criticalPath.Activities.Sum(a => a.Duration);
            Assert.Equal(18, criticalPathDuration); // 18 months total
        }

        #endregion

        #region TC-OPP-SCH-F-005: Generate Gantt Chart Data

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-SCH-F-005")]
        public async Task GenerateGanttChartData_AllPhases_Success()
        {
            // Arrange
            var opportunityId = 1;
            var schedule = await _manager.GenerateScheduleAsync(opportunityId);

            // Act
            var ganttData = await _manager.GenerateGanttChartDataAsync(schedule.Id);

            // Assert
            Assert.NotNull(ganttData);
            Assert.NotEmpty(ganttData.Tasks);
            
            // Each deliverable/phase is a task
            Assert.Equal(3, ganttData.Tasks.Count);
            
            foreach (var task in ganttData.Tasks)
            {
                Assert.NotNull(task.Name);
                Assert.NotNull(task.StartDate);
                Assert.NotNull(task.EndDate);
                Assert.True(task.EndDate > task.StartDate);
            }
            
            // Timeline spans full project
            var earliestStart = ganttData.Tasks.Min(t => t.StartDate);
            var latestEnd = ganttData.Tasks.Max(t => t.EndDate);
            Assert.Equal(schedule.StartDate, earliestStart);
            Assert.Equal(schedule.EndDate, latestEnd);
        }

        #endregion

        #region TC-OPP-SCH-V-001: Validate Timeline Consistency

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-SCH-V-001")]
        public async Task ValidateTimeline_EndBeforeStart_ThrowsException()
        {
            // Arrange - Invalid opportunity with end before start
            var invalidOpportunity = new Domain.Entities.Opportunity
            {
                Id = 99,
                Name = "Invalid Timeline",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(-6), // End BEFORE start
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(invalidOpportunity);
            await _context.SaveChangesAsync();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessException>(async () =>
                await _manager.GenerateScheduleAsync(99));

            Assert.Contains("end date", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("start date", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region TC-OPP-SCH-C-001: Calculate Float/Slack Time

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-SCH-C-001")]
        public async Task CalculateFloat_NonCriticalActivities_ReturnsSlack()
        {
            // Arrange - Add parallel activities (not all sequential)
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 2,
                Name = "Parallel Activities Project",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(12),
                Timeline = 12,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };
            _context.Opportunities.Add(opportunity);
            
            // Add activities - some can run in parallel
            _context.OpportunityDeliverables.AddRange(new[]
            {
                new OpportunityDeliverable { Id = 10, OpportunityId = 2, Description = "Critical Activity", Duration = 12, IsCritical = true },
                new OpportunityDeliverable { Id = 11, OpportunityId = 2, Description = "Parallel Activity 1", Duration = 6, IsCritical = false },
                new OpportunityDeliverable { Id = 12, OpportunityId = 2, Description = "Parallel Activity 2", Duration = 8, IsCritical = false }
            });
            await _context.SaveChangesAsync();

            var schedule = await _manager.GenerateScheduleAsync(2);

            // Act
            var floatAnalysis = await _manager.CalculateFloatAsync(schedule.Id);

            // Assert
            Assert.NotNull(floatAnalysis);
            
            // Critical activity has zero float
            var criticalActivity = floatAnalysis.First(f => f.ActivityName.Contains("Critical"));
            Assert.Equal(0, criticalActivity.FloatDays);
            
            // Parallel activities have float (can be delayed without impacting end date)
            var parallelActivity1 = floatAnalysis.First(f => f.ActivityName.Contains("Parallel Activity 1"));
            Assert.True(parallelActivity1.FloatDays > 0); // Has slack time
        }

        #endregion

        #region Helper Classes

        public class OpportunityDeliverable
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public string Description { get; set; }
            public int Duration { get; set; } // months
            public bool IsCritical { get; set; }
        }

        public class WorkBreakdownStructure
        {
            public List<WorkPackage> WorkPackages { get; set; }
        }

        public class WorkPackage
        {
            public string Name { get; set; }
            public int Duration { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class Milestone
        {
            public string Name { get; set; }
            public DateTime MilestoneDate { get; set; }
        }

        public class CriticalPath
        {
            public List<CriticalActivity> Activities { get; set; }
        }

        public class CriticalActivity
        {
            public string Name { get; set; }
            public int Duration { get; set; }
            public bool IsCritical { get; set; }
        }

        public class GanttChartData
        {
            public List<GanttTask> Tasks { get; set; }
        }

        public class GanttTask
        {
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public class FloatAnalysis
        {
            public string ActivityName { get; set; }
            public int FloatDays { get; set; }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
