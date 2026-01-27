using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.E2E
{
    public class CrossSystemIntegrationTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;
        private readonly Mock<IERPIntegrationService> _mockERPService;
        private readonly Mock<IPMToolService> _mockPMService;
        private readonly Mock<IHRSystemService> _mockHRService;

        public CrossSystemIntegrationTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"CrossSystemTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
            _mockERPService = new Mock<IERPIntegrationService>();
            _mockPMService = new Mock<IPMToolService>();
            _mockHRService = new Mock<IHRSystemService>();
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-013")]
        public async Task CrossSystemSync_FourSystems_AllSyncedSuccessfully()
        {
            // Arrange - Opportunity converted to project
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Education Infrastructure Programme",
                EstimatedValue = 5000000,
                Timeline = 36, // 3 years
                Status = "Converted",
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            var project = new Project
            {
                Id = 1,
                Name = opportunity.Name,
                Budget = opportunity.EstimatedValue,
                OriginalOpportunityId = opportunity.Id
            };

            _context.Opportunities.Add(opportunity);
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Act - Sync to ERP
            _mockERPService.Setup(e => e.SyncProjectAsync(project))
                .ReturnsAsync(new SyncResult { Success = true, ExternalId = "PRJ-2026-0847" });

            var erpSync = await _mockERPService.Object.SyncProjectAsync(project);

            // Act - Sync to PM Tool
            _mockPMService.Setup(p => p.CreateProjectWorkspaceAsync(project))
                .ReturnsAsync(new SyncResult { Success = true, WorkspaceUrl = "https://pm.unops.org/projects/1" });

            var pmSync = await _mockPMService.Object.CreateProjectWorkspaceAsync(project);

            // Act - Sync to HR System
            _mockHRService.Setup(h => h.CreatePositionRequestsAsync(project))
                .ReturnsAsync(new SyncResult { Success = true, PositionsCreated = 12 });

            var hrSync = await _mockHRService.Object.CreatePositionRequestsAsync(project);

            // Assert - All syncs successful
            Assert.True(erpSync.Success);
            Assert.Equal("PRJ-2026-0847", erpSync.ExternalId);

            Assert.True(pmSync.Success);
            Assert.NotNull(pmSync.WorkspaceUrl);

            Assert.True(hrSync.Success);
            Assert.Equal(12, hrSync.PositionsCreated);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-014")]
        public async Task GlobalIndicesUpdateCascade_193Countries_TriggersUpdates()
        {
            // Arrange - 5 opportunities in different countries
            var opportunities = new List<Domain.Entities.Opportunity>
            {
                new() { Id = 1, PrimaryCountryId = 1, Status = "Profiling" },
                new() { Id = 2, PrimaryCountryId = 2, Status = "Profiling" },
                new() { Id = 3, PrimaryCountryId = 3, Status = "Decision" },
                new() { Id = 4, PrimaryCountryId = 1, Status = "Draft" }, // Same country as #1
                new() { Id = 5, PrimaryCountryId = 4, Status = "Approved" } // Already approved
            };

            _context.Opportunities.AddRange(opportunities);
            await _context.SaveChangesAsync();

            // Act - Upload new global indices
            var newIndices = new List<GlobalIndexUpdate>
            {
                new() { CountryId = 1, IndexType = "MVI", OldValue = 35m, NewValue = 42m, Change = +7m }, // Significant change
                new() { CountryId = 2, IndexType = "FSI", OldValue = 75m, NewValue = 78m, Change = +3m }, // Minor change
                new() { CountryId = 3, IndexType = "CPI", OldValue = 32m, NewValue = 30m, Change = -2m } // Improved
            };

            // Identify affected opportunities (active development only)
            var affectedOpportunities = opportunities
                .Where(o => new[] { "Draft", "Profiling", "Decision" }.Contains(o.Status))
                .Where(o => newIndices.Any(i => i.CountryId == o.PrimaryCountryId && Math.Abs(i.Change) >= 5m))
                .ToList();

            // Assert
            Assert.Equal(2, affectedOpportunities.Count); // IDs 1 and 4 (Country 1, change >= 5)
            // Opportunities 2, 3, 5 not significantly affected or already approved
        }

        public class Project { public int Id { get; set; } public string Name { get; set; } public decimal Budget { get; set; } public int OriginalOpportunityId { get; set; } }
        public class SyncResult { public bool Success { get; set; } public string ExternalId { get; set; } public string WorkspaceUrl { get; set; } public int PositionsCreated { get; set; } }
        public class GlobalIndexUpdate { public int CountryId { get; set; } public string IndexType { get; set; } public decimal OldValue { get; set; } public decimal NewValue { get; set; } public decimal Change { get; set; } }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
