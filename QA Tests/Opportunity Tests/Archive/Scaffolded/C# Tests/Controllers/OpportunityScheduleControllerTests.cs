using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Controllers
{
    public class OpportunityScheduleControllerTests
    {
        private readonly Mock<IManagerWrapper> _mockManagerWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly OpportunityScheduleController _controller;

        public OpportunityScheduleControllerTests()
        {
            _mockManagerWrapper = new Mock<IManagerWrapper>();
            _mockMapper = new Mock<IMapper>();
            _controller = new OpportunityScheduleController(_mockManagerWrapper.Object, _mockMapper.Object);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-SCH-CTRL-F-001")]
        public async Task GenerateSchedule_ValidOpportunity_ReturnsOkWithSchedule()
        {
            // Arrange
            var opportunityId = 1;
            var schedule = new ScheduleModel
            {
                Id = 1,
                OpportunityId = opportunityId,
                TotalDuration = 24,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(24)
            };

            _mockManagerWrapper.Setup(m => m.ScheduleManager.GenerateScheduleAsync(opportunityId))
                .ReturnsAsync(schedule);

            // Act
            var result = await _controller.GenerateSchedule(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSchedule = Assert.IsType<ScheduleModel>(okResult.Value);
            Assert.Equal(24, returnedSchedule.TotalDuration);
            Assert.Equal(opportunityId, returnedSchedule.OpportunityId);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-SCHCTRL-002")]
        public async Task GetSchedule_ValidOpportunity_ReturnsOkWithPhasesAndMilestones()
        {
            // Arrange
            var opportunityId = 1;
            var schedule = new ScheduleModel
            {
                Id = 1,
                OpportunityId = opportunityId,
                Phases = new System.Collections.Generic.List<PhaseModel>
                {
                    new PhaseModel { Name = "Planning", Duration = 6 },
                    new PhaseModel { Name = "Implementation", Duration = 18 }
                },
                Milestones = new System.Collections.Generic.List<MilestoneModel>
                {
                    new MilestoneModel { Name = "Project Kickoff", Date = DateTime.UtcNow },
                    new MilestoneModel { Name = "Midpoint Review", Date = DateTime.UtcNow.AddMonths(12) }
                }
            };

            _mockManagerWrapper.Setup(m => m.ScheduleManager.GetScheduleAsync(opportunityId))
                .ReturnsAsync(schedule);

            // Act
            var result = await _controller.GetSchedule(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSchedule = Assert.IsType<ScheduleModel>(okResult.Value);
            Assert.Equal(2, returnedSchedule.Phases.Count);
            Assert.Equal(2, returnedSchedule.Milestones.Count);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-SCHCTRL-003")]
        public async Task UpdateSchedule_ValidRequest_ReturnsOkWithUpdatedSchedule()
        {
            // Arrange
            var scheduleId = 1;
            var updateRequest = new ScheduleUpdateRequest
            {
                TotalDuration = 30,
                Notes = "Extended timeline due to scope expansion"
            };

            var updatedSchedule = new ScheduleModel
            {
                Id = scheduleId,
                TotalDuration = 30,
                Notes = "Extended timeline due to scope expansion"
            };

            _mockManagerWrapper.Setup(m => m.ScheduleManager.UpdateScheduleAsync(scheduleId, updateRequest))
                .ReturnsAsync(updatedSchedule);

            // Act
            var result = await _controller.UpdateSchedule(scheduleId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSchedule = Assert.IsType<ScheduleModel>(okResult.Value);
            Assert.Equal(30, returnedSchedule.TotalDuration);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-SCHCTRL-004")]
        public async Task GetWorkBreakdownStructure_ValidSchedule_ReturnsOkWithWBS()
        {
            // Arrange
            var scheduleId = 1;
            var wbs = new WBSResponse
            {
                RootNode = new WBSNode
                {
                    Id = "1",
                    Name = "Project Root",
                    Children = new System.Collections.Generic.List<WBSNode>
                    {
                        new WBSNode { Id = "1.1", Name = "Phase 1: Planning" },
                        new WBSNode { Id = "1.2", Name = "Phase 2: Execution" }
                    }
                }
            };

            _mockManagerWrapper.Setup(m => m.ScheduleManager.GetWBSAsync(scheduleId))
                .ReturnsAsync(wbs);

            // Act
            var result = await _controller.GetWorkBreakdownStructure(scheduleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedWBS = Assert.IsType<WBSResponse>(okResult.Value);
            Assert.Equal("Project Root", returnedWBS.RootNode.Name);
            Assert.Equal(2, returnedWBS.RootNode.Children.Count);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-SCHCTRL-005")]
        public async Task GetMilestones_ValidSchedule_ReturnsOkWithMilestoneList()
        {
            // Arrange
            var scheduleId = 1;
            var milestones = new System.Collections.Generic.List<MilestoneModel>
            {
                new MilestoneModel { Id = 1, Name = "Kickoff", Date = DateTime.UtcNow, Status = "Completed" },
                new MilestoneModel { Id = 2, Name = "Midpoint", Date = DateTime.UtcNow.AddMonths(6), Status = "Pending" },
                new MilestoneModel { Id = 3, Name = "Completion", Date = DateTime.UtcNow.AddMonths(12), Status = "Pending" }
            };

            _mockManagerWrapper.Setup(m => m.ScheduleManager.GetMilestonesAsync(scheduleId))
                .ReturnsAsync(milestones);

            // Act
            var result = await _controller.GetMilestones(scheduleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMilestones = Assert.IsAssignableFrom<System.Collections.Generic.List<MilestoneModel>>(okResult.Value);
            Assert.Equal(3, returnedMilestones.Count);
            Assert.Equal("Completed", returnedMilestones[0].Status);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-SCHCTRL-006")]
        public async Task GetGanttChartData_ValidSchedule_ReturnsOkWithGanttData()
        {
            // Arrange
            var scheduleId = 1;
            var ganttData = new GanttChartResponse
            {
                Tasks = new System.Collections.Generic.List<GanttTask>
                {
                    new GanttTask { Id = 1, Name = "Task 1", Start = DateTime.UtcNow, End = DateTime.UtcNow.AddDays(30) },
                    new GanttTask { Id = 2, Name = "Task 2", Start = DateTime.UtcNow.AddDays(30), End = DateTime.UtcNow.AddDays(60), DependsOn = new[] { 1 } }
                }
            };

            _mockManagerWrapper.Setup(m => m.ScheduleManager.GetGanttChartDataAsync(scheduleId))
                .ReturnsAsync(ganttData);

            // Act
            var result = await _controller.GetGanttChartData(scheduleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedData = Assert.IsType<GanttChartResponse>(okResult.Value);
            Assert.Equal(2, returnedData.Tasks.Count);
            Assert.Contains(1, returnedData.Tasks[1].DependsOn);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-SCHCTRL-007")]
        public async Task GetCriticalPath_ValidSchedule_ReturnsOkWithCriticalPathAnalysis()
        {
            // Arrange
            var scheduleId = 1;
            var criticalPath = new CriticalPathResponse
            {
                CriticalTasks = new System.Collections.Generic.List<TaskModel>
                {
                    new TaskModel { Id = 1, Name = "Design", Duration = 30, Float = 0 },
                    new TaskModel { Id = 3, Name = "Development", Duration = 60, Float = 0 },
                    new TaskModel { Id = 5, Name = "Testing", Duration = 30, Float = 0 }
                },
                TotalCriticalPathDuration = 120,
                NonCriticalTasks = new System.Collections.Generic.List<TaskModel>
                {
                    new TaskModel { Id = 2, Name = "Documentation", Duration = 20, Float = 10 }
                }
            };

            _mockManagerWrapper.Setup(m => m.ScheduleManager.GetCriticalPathAsync(scheduleId))
                .ReturnsAsync(criticalPath);

            // Act
            var result = await _controller.GetCriticalPath(scheduleId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPath = Assert.IsType<CriticalPathResponse>(okResult.Value);
            Assert.Equal(3, returnedPath.CriticalTasks.Count);
            Assert.Equal(120, returnedPath.TotalCriticalPathDuration);
            Assert.All(returnedPath.CriticalTasks, task => Assert.Equal(0, task.Float));
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-SCHCTRL-008")]
        public async Task ExportToMSProject_ValidSchedule_ReturnsFileResult()
        {
            // Arrange
            var scheduleId = 1;
            var mppBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // ZIP signature (MPP files are ZIP-based)

            _mockManagerWrapper.Setup(m => m.ScheduleManager.ExportToMSProjectAsync(scheduleId))
                .ReturnsAsync(mppBytes);

            // Act
            var result = await _controller.ExportToMSProject(scheduleId);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.ms-project", fileResult.ContentType);
            Assert.NotEmpty(fileResult.FileContents);
        }

        public class ScheduleModel
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int TotalDuration { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Notes { get; set; }
            public System.Collections.Generic.List<PhaseModel> Phases { get; set; }
            public System.Collections.Generic.List<MilestoneModel> Milestones { get; set; }
        }

        public class PhaseModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Duration { get; set; }
        }

        public class MilestoneModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Date { get; set; }
            public string Status { get; set; }
        }

        public class ScheduleUpdateRequest
        {
            public int TotalDuration { get; set; }
            public string Notes { get; set; }
        }

        public class WBSResponse
        {
            public WBSNode RootNode { get; set; }
        }

        public class WBSNode
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public System.Collections.Generic.List<WBSNode> Children { get; set; }
        }

        public class GanttChartResponse
        {
            public System.Collections.Generic.List<GanttTask> Tasks { get; set; }
        }

        public class GanttTask
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public int[] DependsOn { get; set; }
        }

        public class CriticalPathResponse
        {
            public System.Collections.Generic.List<TaskModel> CriticalTasks { get; set; }
            public int TotalCriticalPathDuration { get; set; }
            public System.Collections.Generic.List<TaskModel> NonCriticalTasks { get; set; }
        }

        public class TaskModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Duration { get; set; }
            public int Float { get; set; }
        }
    }
}
