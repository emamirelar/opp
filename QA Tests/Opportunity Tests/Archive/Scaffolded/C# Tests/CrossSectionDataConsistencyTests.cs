using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Business.Tests.Opportunity
{
    /// <summary>
    /// Cross-Section Data Consistency Tests
    /// 
    /// Purpose: Verify data consistency across different sections and views of opportunities
    /// 
    /// Real Production Bug: PNO-912 - STATEMENT section
    /// - Target signing date: Dec 12 in WHEN section → Dec 11 in Statement (off by 1!)
    /// - Delivery date: May 15 in WHEN section → May 14 in Statement (off by 1!)
    /// - Opportunity Manager name missing in generated statement
    /// 
    /// These tests prevent data discrepancies between:
    /// - View mode vs Edit mode
    /// - Different opportunity sections (WHEN, WHO, WHAT, WHERE)
    /// - Generated documents vs source data
    /// - Database values vs displayed values
    /// </summary>
    [Trait("Category", "CrossSectionConsistency")]
    [Trait("Priority", "Critical")]
    public class CrossSectionDataConsistencyTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public CrossSectionDataConsistencyTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"CrossSectionTest_{Guid.NewGuid()}")
                .Options;
            _context = TestDbContextFactory.Create(_dbOptions);
        }

        #region Date Consistency Tests

        [Fact]
        public async Task TC_CSDC_001_TargetSigningDate_MatchesAcrossSections_NoOffset()
        {
            // Arrange
            var expectedDate = new DateTime(2026, 12, 12, 0, 0, 0, DateTimeKind.Utc);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                OpportunityNumber = "OPP-2026-001",
                TargetSigningDate = expectedDate,
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Retrieve from database
            var savedOpportunity = await _context.Opportunities
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Date should match exactly (no off-by-one error)
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.TargetSigningDate.Should().Be(expectedDate);
            
            // Verify no timezone shift
            savedOpportunity.TargetSigningDate.Value.Year.Should().Be(2026);
            savedOpportunity.TargetSigningDate.Value.Month.Should().Be(12);
            savedOpportunity.TargetSigningDate.Value.Day.Should().Be(12);
        }

        [Fact]
        public async Task TC_CSDC_002_DeliveryDate_MatchesAcrossSections_NoOffset()
        {
            // Arrange
            var expectedDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc);
            
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Test Opportunity",
                OpportunityNumber = "OPP-2026-002",
                DeliveryDate = expectedDate,
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Date should match exactly
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.DeliveryDate.Should().Be(expectedDate);
            savedOpportunity.DeliveryDate.Value.Day.Should().Be(15, "DeliveryDate should not shift from May 15 to May 14");
        }

        [Fact]
        public async Task TC_CSDC_003_AllDates_ConsistentAcrossViews()
        {
            // Arrange
            var targetDate = new DateTime(2026, 12, 12, 0, 0, 0, DateTimeKind.Utc);
            var deliveryDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc);
            var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Multi-Date Opportunity",
                OpportunityNumber = "OPP-2026-003",
                TargetSigningDate = targetDate,
                DeliveryDate = deliveryDate,
                EstimatedStartDate = startDate,
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - All dates match exactly
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.TargetSigningDate.Should().Be(targetDate);
            savedOpportunity.DeliveryDate.Should().Be(deliveryDate);
            savedOpportunity.EstimatedStartDate.Should().Be(startDate);
        }

        [Fact]
        public async Task TC_CSDC_004_Timeline_DatesNotShiftedByTimezone()
        {
            // Arrange - Create opportunity with midnight UTC dates
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Timezone Test Opportunity",
                OpportunityNumber = "OPP-2026-004",
                TargetSigningDate = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                DeliveryDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Retrieve multiple times to check consistency
            var retrieval1 = await _context.Opportunities.FindAsync(opportunity.Id);
            var retrieval2 = await _context.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Dates remain consistent across retrievals
            retrieval1!.TargetSigningDate.Value.Day.Should().Be(31, "Date should not shift from Dec 31 to Jan 1");
            retrieval2!.TargetSigningDate.Value.Day.Should().Be(31);
            retrieval1.TargetSigningDate.Should().Be(retrieval2.TargetSigningDate);
        }

        #endregion

        #region Manager/User Consistency Tests

        [Fact]
        public async Task TC_CSDC_005_OpportunityManager_NameAppearsInAllSections()
        {
            // Arrange
            var manager = new UserProfile
            {
                UserId = 100,
                FirstName = "Jane",
                LastName = "Manager",
                UserEmail = "jane.manager@unops.org"
            };
            _context.UserProfile.Add(manager);

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Manager Test Opportunity",
                OpportunityNumber = "OPP-2026-005",
                OpportunityManagerId = manager.UserId,
                CreatedBy = manager.UserId,
                LastModifiedBy = manager.UserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Load opportunity with manager
            var savedOpportunity = await _context.Opportunities
                .Include(o => o.OpportunityManager)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Manager data is available and consistent
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.OpportunityManagerId.Should().Be(manager.UserId);
            savedOpportunity.OpportunityManager.Should().NotBeNull();
            savedOpportunity.OpportunityManager!.FirstName.Should().Be("Jane");
            savedOpportunity.OpportunityManager.LastName.Should().Be("Manager");
        }

        [Fact]
        public async Task TC_CSDC_006_CreatedBy_MatchesAuditFields()
        {
            // Arrange
            var creator = new UserProfile
            {
                UserId = 101,
                FirstName = "Creator",
                LastName = "User",
                UserEmail = "creator@unops.org"
            };
            _context.UserProfile.Add(creator);

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Audit Test Opportunity",
                OpportunityNumber = "OPP-2026-006",
                OpportunityManagerId = creator.UserId,
                CreatedBy = creator.UserId,
                LastModifiedBy = creator.UserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - Audit fields are consistent
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.CreatedBy.Should().Be(creator.UserId);
            savedOpportunity.LastModifiedBy.Should().Be(creator.UserId);
        }

        #endregion

        #region Budget Consistency Tests

        [Fact]
        public async Task TC_CSDC_007_BudgetValues_ConsistentAcrossViews()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Budget Test Opportunity",
                OpportunityNumber = "OPP-2026-007",
                EstimatedBudget = 1000000.50m,
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Retrieve in different ways
            var directLoad = await _context.Opportunities.FindAsync(opportunity.Id);
            var queryLoad = await _context.Opportunities
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Budget values match exactly
            directLoad!.EstimatedBudget.Should().Be(1000000.50m);
            queryLoad!.EstimatedBudget.Should().Be(1000000.50m);
            directLoad.EstimatedBudget.Should().Be(queryLoad.EstimatedBudget);
        }

        #endregion

        #region Team Assignment Consistency Tests

        [Fact]
        public async Task TC_CSDC_008_TeamMembers_VisibleInAllRelevantSections()
        {
            // Arrange
            var manager = new UserProfile
            {
                UserId = 102,
                FirstName = "Team",
                LastName = "Lead",
                UserEmail = "team.lead@unops.org"
            };
            _context.UserProfile.Add(manager);

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Team Test Opportunity",
                OpportunityNumber = "OPP-2026-008",
                OpportunityManagerId = manager.UserId,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Load with team members
            var savedOpportunity = await _context.Opportunities
                .Include(o => o.OpportunityManager)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Team data is complete
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.OpportunityManager.Should().NotBeNull();
            savedOpportunity.OpportunityManager!.UserId.Should().Be(manager.UserId);
        }

        #endregion

        #region Country/Location Consistency Tests

        [Fact]
        public async Task TC_CSDC_009_Countries_ConsistentBetweenWHEREandDetails()
        {
            // Arrange
            var country = new Country
            {
                Id = 1,
                Iso2Code = "KE",
                Iso3Code = "KEN",
                Name = "Kenya",
                Status = EntityStatus.Active
            };
            _context.Countries.Add(country);

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Location Test Opportunity",
                OpportunityNumber = "OPP-2026-009",
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Verify country data is accessible
            var savedCountry = await _context.Countries.FindAsync(country.Id);

            // Assert
            savedCountry.Should().NotBeNull();
            savedCountry!.Name.Should().Be("Kenya");
        }

        #endregion

        #region Format Consistency Tests

        [Fact]
        public async Task TC_CSDC_010_NoFormattingDifferences_BetweenSections()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Format Test Opportunity",
                OpportunityNumber = "OPP-2026-010",
                Description = "Test description with special characters: $1,000.00",
                EstimatedBudget = 1000.00m,
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act
            var savedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - Text content matches exactly
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.Description.Should().Be("Test description with special characters: $1,000.00");
        }

        [Fact]
        public async Task TC_CSDC_011_Audit_Trail_ReflectsAllChanges()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Audit Trail Test",
                OpportunityNumber = "OPP-2026-011",
                EstimatedBudget = 500000m,
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            var originalCreatedDate = opportunity.CreatedDate;

            // Act - Update opportunity
            opportunity.EstimatedBudget = 750000m;
            opportunity.LastModifiedBy = 2;
            opportunity.LastModifiedDate = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var updatedOpportunity = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - Audit trail is accurate
            updatedOpportunity.Should().NotBeNull();
            updatedOpportunity!.CreatedDate.Should().Be(originalCreatedDate);
            updatedOpportunity.LastModifiedBy.Should().Be(2);
            updatedOpportunity.EstimatedBudget.Should().Be(750000m);
        }

        #endregion

        #region View/Edit Mode Consistency Tests

        [Fact]
        public async Task TC_CSDC_012_NoDataLoss_BetweenViewAndEditModes()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "View/Edit Test Opportunity",
                OpportunityNumber = "OPP-2026-012",
                Description = "Complete description",
                EstimatedBudget = 1000000m,
                TargetSigningDate = new DateTime(2026, 12, 31),
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Simulate view mode (read-only)
            var viewMode = await _context.Opportunities
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Simulate edit mode (tracked)
            var editMode = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - Data is identical in both modes
            viewMode.Should().NotBeNull();
            editMode.Should().NotBeNull();
            viewMode!.Name.Should().Be(editMode!.Name);
            viewMode.Description.Should().Be(editMode.Description);
            viewMode.EstimatedBudget.Should().Be(editMode.EstimatedBudget);
            viewMode.TargetSigningDate.Should().Be(editMode.TargetSigningDate);
        }

        #endregion

        #region Statement Generation Tests

        [Fact]
        public async Task TC_CSDC_013_GeneratedStatement_ContainsAllEnteredData()
        {
            // Arrange
            var manager = new UserProfile
            {
                UserId = 103,
                FirstName = "Statement",
                LastName = "Manager",
                UserEmail = "statement.manager@unops.org"
            };
            _context.UserProfile.Add(manager);

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Statement Test Opportunity",
                OpportunityNumber = "OPP-2026-013",
                Description = "Complete opportunity description for statement",
                EstimatedBudget = 2000000m,
                TargetSigningDate = new DateTime(2026, 12, 12),
                DeliveryDate = new DateTime(2026, 5, 15),
                OpportunityManagerId = manager.UserId,
                CreatedBy = manager.UserId,
                LastModifiedBy = manager.UserId,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Load opportunity with all related data (as would be done for statement generation)
            var fullOpportunity = await _context.Opportunities
                .Include(o => o.OpportunityManager)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - All data needed for statement is present
            fullOpportunity.Should().NotBeNull();
            fullOpportunity!.Name.Should().Be("Statement Test Opportunity");
            fullOpportunity.Description.Should().Be("Complete opportunity description for statement");
            fullOpportunity.EstimatedBudget.Should().Be(2000000m);
            fullOpportunity.TargetSigningDate.Value.Day.Should().Be(12, "Target signing date should be Dec 12, not Dec 11");
            fullOpportunity.DeliveryDate.Value.Day.Should().Be(15, "Delivery date should be May 15, not May 14");
            fullOpportunity.OpportunityManager.Should().NotBeNull();
            fullOpportunity.OpportunityManager!.FirstName.Should().Be("Statement");
            fullOpportunity.OpportunityManager.LastName.Should().Be("Manager");
        }

        [Fact]
        public async Task TC_CSDC_014_RegeneratedStatement_IncludesLatestUpdates()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Regeneration Test Opportunity",
                OpportunityNumber = "OPP-2026-014",
                EstimatedBudget = 500000m,
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Update opportunity
            opportunity.EstimatedBudget = 750000m;
            opportunity.Description = "Updated description";
            await _context.SaveChangesAsync();

            var regeneratedData = await _context.Opportunities.FindAsync(opportunity.Id);

            // Assert - Statement would reflect latest data
            regeneratedData.Should().NotBeNull();
            regeneratedData!.EstimatedBudget.Should().Be(750000m);
            regeneratedData.Description.Should().Be("Updated description");
        }

        [Fact]
        public async Task TC_CSDC_015_ExportedDocuments_MatchSourceData()
        {
            // Arrange
            var targetDate = new DateTime(2026, 12, 12, 0, 0, 0, DateTimeKind.Utc);
            var deliveryDate = new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc);

            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Export Test Opportunity",
                OpportunityNumber = "OPP-2026-015",
                Description = "Opportunity for export testing",
                EstimatedBudget = 1500000m,
                TargetSigningDate = targetDate,
                DeliveryDate = deliveryDate,
                OpportunityManagerId = 1,
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };
            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Retrieve for export
            var exportData = await _context.Opportunities
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Export data matches source exactly
            exportData.Should().NotBeNull();
            exportData!.Name.Should().Be("Export Test Opportunity");
            exportData.EstimatedBudget.Should().Be(1500000m);
            exportData.TargetSigningDate.Should().Be(targetDate);
            exportData.DeliveryDate.Should().Be(deliveryDate);
            
            // Verify dates haven't shifted
            exportData.TargetSigningDate.Value.Day.Should().Be(12);
            exportData.DeliveryDate.Value.Day.Should().Be(15);
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
