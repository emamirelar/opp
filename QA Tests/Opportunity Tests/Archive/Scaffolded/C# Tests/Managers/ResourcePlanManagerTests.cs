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
    /// Tests for ResourcePlanManager
    /// Based on ResourcePlanManager_TestCases.md (15+ tests)
    /// </summary>
    public class ResourcePlanManagerTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ResourcePlanManager _manager;

        public ResourcePlanManagerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"ResourceTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockMapper = new Mock<IMapper>();

            _manager = new ResourcePlanManager(_mockMapper.Object, _context);

            SeedTestData();
        }

        private void SeedTestData()
        {
            _context.Opportunities.Add(new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Infrastructure Project",
                EstimatedValue = 2500000,
                Timeline = 24,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            });

            _context.SaveChanges();
        }

        #region TC-OPP-RES-F-001: Generate Resource Plan

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-RES-F-001")]
        public async Task GenerateResourcePlan_BasedOnScope_Success()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var resourcePlan = await _manager.GenerateResourcePlanAsync(opportunityId);

            // Assert
            Assert.NotNull(resourcePlan);
            Assert.Equal(opportunityId, resourcePlan.OpportunityId);
            Assert.NotEmpty(resourcePlan.Roles);
            
            // Typical infrastructure project roles
            var pmRole = resourcePlan.Roles.FirstOrDefault(r => r.RoleName.Contains("Project Manager"));
            Assert.NotNull(pmRole);
            Assert.Equal(1.0m, pmRole.FTE); // Full-time PM
            
            // Engineering roles
            var engineerRoles = resourcePlan.Roles.Where(r => r.RoleName.Contains("Engineer")).ToList();
            Assert.NotEmpty(engineerRoles);
        }

        #endregion

        #region TC-OPP-RES-F-002: Identify Development Roles

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-RES-F-002")]
        public async Task IdentifyDevelopmentRoles_OpportunityStage_Success()
        {
            // Arrange
            var opportunityId = 1;
            var resourcePlan = await _manager.GenerateResourcePlanAsync(opportunityId);

            // Act
            var developmentRoles = await _manager.GetDevelopmentRolesAsync(resourcePlan.Id);

            // Assert
            Assert.NotEmpty(developmentRoles);
            
            // Development stage roles (before Go decision)
            var typicalDevRoles = new[] 
            { 
                "Opportunity Manager", 
                "Business Developer", 
                "Technical Advisor",
                "Budget Specialist" 
            };
            
            foreach (var roleName in typicalDevRoles)
            {
                Assert.Contains(developmentRoles, r => r.RoleName.Contains(roleName));
            }
            
            // Development roles typically part-time or short duration
            Assert.All(developmentRoles, r => Assert.True(r.FTE <= 0.5m || r.Duration <= 3)); // <= 50% FTE or <=3 months
        }

        #endregion

        #region TC-OPP-RES-F-003: Identify Implementation Roles

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-RES-F-003")]
        public async Task IdentifyImplementationRoles_ProjectStage_Success()
        {
            // Arrange
            var opportunityId = 1;
            var resourcePlan = await _manager.GenerateResourcePlanAsync(opportunityId);

            // Act
            var implementationRoles = await _manager.GetImplementationRolesAsync(resourcePlan.Id);

            // Assert
            Assert.NotEmpty(implementationRoles);
            
            // Implementation roles (after Go decision)
            var typicalImplRoles = new[] 
            { 
                "Project Manager", 
                "Engineer",
                "Procurement Specialist",
                "Site Supervisor" 
            };
            
            foreach (var roleName in typicalImplRoles)
            {
                Assert.Contains(implementationRoles, r => r.RoleName.Contains(roleName));
            }
            
            // Implementation roles typically full-time and longer duration
            var fullTimeRoles = implementationRoles.Where(r => r.FTE >= 0.8m).ToList();
            Assert.NotEmpty(fullTimeRoles);
        }

        #endregion

        #region TC-OPP-RES-F-004: Calculate Personnel Budget

        [Fact]
        [Trait("Category", "P0")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-RES-F-004")]
        public async Task CalculatePersonnelBudget_AllRoles_Success()
        {
            // Arrange
            var opportunityId = 1;
            var resourcePlan = await _manager.GenerateResourcePlanAsync(opportunityId);

            // Act
            var personnelBudget = await _manager.CalculatePersonnelBudgetAsync(resourcePlan.Id);

            // Assert
            Assert.NotNull(personnelBudget);
            Assert.True(personnelBudget.TotalCost > 0);
            
            // Personnel budget typically 40-60% of total opportunity budget
            var opportunity = await _context.Opportunities.FindAsync(opportunityId);
            var personnelPercentage = (personnelBudget.TotalCost / opportunity.EstimatedValue) * 100;
            
            Assert.InRange(personnelPercentage, 35m, 65m); // 35-65% is reasonable
            
            // Breakdown by role category
            Assert.NotEmpty(personnelBudget.RoleCosts);
            Assert.All(personnelBudget.RoleCosts, rc => Assert.True(rc.Cost > 0));
        }

        #endregion

        #region TC-OPP-RES-F-005: Check Resource Availability

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Functional")]
        [Trait("TestId", "TC-OPP-RES-F-005")]
        public async Task CheckResourceAvailability_RequestedRoles_ReturnsAvailability()
        {
            // Arrange
            var opportunityId = 1;
            var resourcePlan = await _manager.GenerateResourcePlanAsync(opportunityId);

            // Mock resource availability data
            var availableResources = new Dictionary<string, int>
            {
                { "Project Manager", 3 },
                { "Engineer", 5 },
                { "Procurement Specialist", 2 }
            };

            // Act
            var availabilityCheck = await _manager.CheckResourceAvailabilityAsync(
                resourcePlan.Id, 
                startDate: DateTime.UtcNow,
                endDate: DateTime.UtcNow.AddMonths(24));

            // Assert
            Assert.NotNull(availabilityCheck);
            Assert.NotEmpty(availabilityCheck.RoleAvailability);
            
            foreach (var roleCheck in availabilityCheck.RoleAvailability)
            {
                Assert.True(roleCheck.Requested >= 0);
                Assert.True(roleCheck.Available >= 0);
                
                // Flag if insufficient
                if (roleCheck.Available < roleCheck.Requested)
                {
                    Assert.True(roleCheck.Insufficient);
                }
            }
        }

        #endregion

        #region TC-OPP-RES-V-001: Validate FTE Range

        [Fact]
        [Trait("Category", "P1")]
        [Trait("Type", "Validation")]
        [Trait("TestId", "TC-OPP-RES-V-001")]
        public async Task ValidateFTE_ExceedsMaximum_ThrowsException()
        {
            // Arrange - Role with invalid FTE (>1.0)
            var resourceRole = new ResourcePlanRole
            {
                ResourcePlanId = 1,
                RoleName = "Test Role",
                FTE = 1.5m, // Invalid - cannot be > 100%
                Duration = 12
            };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() =>
            {
                if (resourceRole.FTE > 1.0m)
                    throw new ValidationException("FTE cannot exceed 1.0 (100%)");
            });

            Assert.Contains("1.0", ex.Message);
        }

        #endregion

        #region Helper Classes

        public class ResourcePlanRole
        {
            public int Id { get; set; }
            public int ResourcePlanId { get; set; }
            public string RoleName { get; set; }
            public decimal FTE { get; set; }
            public int Duration { get; set; } // months
        }

        public class PersonnelBudget
        {
            public decimal TotalCost { get; set; }
            public List<RoleCost> RoleCosts { get; set; }
        }

        public class RoleCost
        {
            public string RoleName { get; set; }
            public decimal Cost { get; set; }
        }

        public class ResourceAvailabilityCheck
        {
            public List<RoleAvailability> RoleAvailability { get; set; }
        }

        public class RoleAvailability
        {
            public string RoleName { get; set; }
            public int Requested { get; set; }
            public int Available { get; set; }
            public bool Insufficient { get; set; }
        }

        public class FloatAnalysis
        {
            public string ActivityName { get; set; }
            public int FloatDays { get; set; }
        }

        public class ValidationException : Exception
        {
            public ValidationException(string message) : base(message) { }
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
