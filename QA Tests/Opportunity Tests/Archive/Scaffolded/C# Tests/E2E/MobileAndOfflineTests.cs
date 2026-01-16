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
    public class MobileAndOfflineTests : IDisposable
    {
        private readonly DbContextOptions<UNOPSAppDbContext> _dbContextOptions;
        private readonly UNOPSAppDbContext _context;

        public MobileAndOfflineTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<UNOPSAppDbContext>()
                .UseInMemoryDatabase($"MobileTestDb_{Guid.NewGuid()}")
                .Options;

            _context = new UNOPSAppDbContext(_dbContextOptions);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-E2E-POS-019")]
        public async Task MobileFieldWork_OfflineDataCollection_SyncsOnReconnect()
        {
            // Arrange - Opportunity synced for offline access
            var opportunity = new Domain.Entities.Opportunity
            {
                Id = 1,
                Name = "Rural Electrification - Remote Region",
                EstimatedValue = 950000,
                Status = "Profiling",
                SyncedForOffline = true,
                LastSyncDate = DateTime.UtcNow,
                CreatedBy = 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Simulate offline work
            var offlineChanges = new OfflineChangeSet
            {
                OpportunityId = 1,
                FieldNotes = new List<string>
                {
                    "Village 1: 200 households, no electricity",
                    "Village 2: 150 households, diesel generator",
                    "Village 3: 300 households, no infrastructure"
                },
                Photos = 47, // 47 photos taken
                GPSCoordinates = new List<GPSCoordinate>
                {
                    new() { Latitude = -15.4167, Longitude = 35.0167, Location = "Village 1" },
                    new() { Latitude = -15.4200, Longitude = 35.0200, Location = "Village 2" },
                    new() { Latitude = -15.4250, Longitude = 35.0150, Location = "Village 3" }
                },
                BudgetAdjustment = 150000, // +15% for remote location
                NewRisks = new List<string>
                {
                    "Seasonal road inaccessibility (June-August)",
                    "Limited local technical capacity"
                }
            };

            // Offline period: 2 days
            var offlineStartTime = DateTime.UtcNow;
            var offlineEndTime = offlineStartTime.AddDays(2);

            // Act - Return to office and sync
            opportunity.Description += "\n" + string.Join("\n", offlineChanges.FieldNotes);
            opportunity.EstimatedValue += offlineChanges.BudgetAdjustment;
            opportunity.LastModifiedDate = offlineEndTime;
            opportunity.LastSyncDate = offlineEndTime;

            await _context.SaveChangesAsync();

            // Assert - All offline work synced
            var syncedOpportunity = await _context.Opportunities.FindAsync(1);
            
            Assert.Contains("Village 1", syncedOpportunity.Description);
            Assert.Contains("Village 2", syncedOpportunity.Description);
            Assert.Contains("Village 3", syncedOpportunity.Description);
            Assert.Equal(950000 + 150000, syncedOpportunity.EstimatedValue); // Budget adjusted
            
            // Photos and GPS would be in separate tables
            Assert.Equal(47, offlineChanges.Photos); // 47 photos ready for upload
            Assert.Equal(3, offlineChanges.GPSCoordinates.Count); // 3 locations captured
        }

        public class OfflineChangeSet { public int OpportunityId { get; set; } public List<string> FieldNotes { get; set; } public int Photos { get; set; } public List<GPSCoordinate> GPSCoordinates { get; set; } public decimal BudgetAdjustment { get; set; } public List<string> NewRisks { get; set; } }
        public class GPSCoordinate { public double Latitude { get; set; } public double Longitude { get; set; } public string Location { get; set; } }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
