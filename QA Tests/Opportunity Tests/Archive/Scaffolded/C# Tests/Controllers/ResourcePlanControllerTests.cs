using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Controllers
{
    public class ResourcePlanControllerTests
    {
        private readonly Mock<IManagerWrapper> _mockManagerWrapper;
        private readonly ResourcePlanController _controller;

        public ResourcePlanControllerTests()
        {
            _mockManagerWrapper = new Mock<IManagerWrapper>();
            _controller = new ResourcePlanController(_mockManagerWrapper.Object);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-RES-CTRL-F-001")]
        public async Task GenerateResourcePlan_ValidOpportunity_ReturnsOk()
        {
            var opportunityId = 1;
            var plan = new ResourcePlanModel { Id = 1, OpportunityId = opportunityId, TotalFTEs = 8 };
            _mockManagerWrapper.Setup(m => m.ResourcePlanManager.GenerateResourcePlanAsync(opportunityId)).ReturnsAsync(plan);

            var result = await _controller.GeneratePlan(opportunityId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-RES-CTRL-F-002")]
        public async Task CheckAvailability_RequestedRoles_ReturnsAvailability()
        {
            // Arrange
            var opportunityId = 1;
            var availability = new AvailabilityModel
            {
                TotalRequested = 8,
                TotalAvailable = 6,
                Insufficient = true,
                Details = new System.Collections.Generic.List<RoleAvailability>
                {
                    new RoleAvailability { Role = "Project Manager", Requested = 1, Available = 1, Sufficient = true },
                    new RoleAvailability { Role = "Engineer", Requested = 5, Available = 3, Sufficient = false },
                    new RoleAvailability { Role = "Admin", Requested = 2, Available = 2, Sufficient = true }
                }
            };

            _mockManagerWrapper.Setup(m => m.ResourcePlanManager.CheckResourceAvailabilityAsync(opportunityId))
                .ReturnsAsync(availability);

            // Act
            var result = await _controller.CheckAvailability(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAvailability = Assert.IsType<AvailabilityModel>(okResult.Value);
            Assert.True(returnedAvailability.Insufficient);
            Assert.Equal(3, returnedAvailability.Details.Count);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-RES-CTRL-F-003")]
        public async Task GetResourcePlan_ByOpportunityId_ReturnsOkWithDetailedPlan()
        {
            // Arrange
            var opportunityId = 1;
            var plan = new ResourcePlanModel
            {
                Id = 1,
                OpportunityId = opportunityId,
                TotalFTEs = 8,
                DevelopmentPhase = new PhaseResources { FTEs = 2, Duration = 6 },
                ImplementationPhase = new PhaseResources { FTEs = 6, Duration = 18 },
                Roles = new System.Collections.Generic.List<RoleRequirement>
                {
                    new RoleRequirement { Role = "Project Manager", FTEs = 1, Level = "Senior" },
                    new RoleRequirement { Role = "Engineers", FTEs = 5, Level = "Mid-Level" },
                    new RoleRequirement { Role = "Admin Support", FTEs = 2, Level = "Junior" }
                }
            };

            _mockManagerWrapper.Setup(m => m.ResourcePlanManager.GetResourcePlanAsync(opportunityId))
                .ReturnsAsync(plan);

            // Act
            var result = await _controller.GetPlan(opportunityId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlan = Assert.IsType<ResourcePlanModel>(okResult.Value);
            Assert.Equal(8, returnedPlan.TotalFTEs);
            Assert.Equal(3, returnedPlan.Roles.Count);
        }

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-RES-CTRL-F-004")]
        public async Task UpdateResourcePlan_ValidRequest_ReturnsOkWithUpdatedPlan()
        {
            // Arrange
            var planId = 1;
            var updateRequest = new ResourcePlanUpdateRequest
            {
                TotalFTEs = 10,
                Notes = "Increased staffing for accelerated timeline"
            };

            var updatedPlan = new ResourcePlanModel
            {
                Id = planId,
                TotalFTEs = 10,
                Notes = "Increased staffing for accelerated timeline"
            };

            _mockManagerWrapper.Setup(m => m.ResourcePlanManager.UpdateResourcePlanAsync(planId, updateRequest))
                .ReturnsAsync(updatedPlan);

            // Act
            var result = await _controller.UpdatePlan(planId, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPlan = Assert.IsType<ResourcePlanModel>(okResult.Value);
            Assert.Equal(10, returnedPlan.TotalFTEs);
        }

        [Fact]
        [Trait("Category", "P2")]
        [Trait("Type", "API")]
        [Trait("TestId", "TC-OPP-RES-CTRL-F-005")]
        public async Task GetPersonnelBudget_ByResourcePlan_ReturnsOkWithBudgetBreakdown()
        {
            // Arrange
            var planId = 1;
            var budgetBreakdown = new PersonnelBudgetResponse
            {
                TotalPersonnelCost = 960000m, // 8 FTEs * 120K/year
                DevelopmentCost = 240000m,
                ImplementationCost = 720000m,
                RoleBreakdown = new System.Collections.Generic.Dictionary<string, decimal>
                {
                    { "Project Manager", 180000m },
                    { "Engineers", 600000m },
                    { "Admin Support", 180000m }
                }
            };

            _mockManagerWrapper.Setup(m => m.ResourcePlanManager.GetPersonnelBudgetAsync(planId))
                .ReturnsAsync(budgetBreakdown);

            // Act
            var result = await _controller.GetPersonnelBudget(planId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedBudget = Assert.IsType<PersonnelBudgetResponse>(okResult.Value);
            Assert.Equal(960000m, returnedBudget.TotalPersonnelCost);
            Assert.Equal(3, returnedBudget.RoleBreakdown.Count);
        }

        public class ResourcePlanModel
        {
            public int Id { get; set; }
            public int OpportunityId { get; set; }
            public int TotalFTEs { get; set; }
            public string Notes { get; set; }
            public PhaseResources DevelopmentPhase { get; set; }
            public PhaseResources ImplementationPhase { get; set; }
            public System.Collections.Generic.List<RoleRequirement> Roles { get; set; }
        }

        public class PhaseResources
        {
            public int FTEs { get; set; }
            public int Duration { get; set; }
        }

        public class RoleRequirement
        {
            public string Role { get; set; }
            public int FTEs { get; set; }
            public string Level { get; set; }
        }

        public class AvailabilityModel
        {
            public int TotalRequested { get; set; }
            public int TotalAvailable { get; set; }
            public bool Insufficient { get; set; }
            public System.Collections.Generic.List<RoleAvailability> Details { get; set; }
        }

        public class RoleAvailability
        {
            public string Role { get; set; }
            public int Requested { get; set; }
            public int Available { get; set; }
            public bool Sufficient { get; set; }
        }

        public class ResourcePlanUpdateRequest
        {
            public int TotalFTEs { get; set; }
            public string Notes { get; set; }
        }

        public class PersonnelBudgetResponse
        {
            public decimal TotalPersonnelCost { get; set; }
            public decimal DevelopmentCost { get; set; }
            public decimal ImplementationCost { get; set; }
            public System.Collections.Generic.Dictionary<string, decimal> RoleBreakdown { get; set; }
        }
    }
}
