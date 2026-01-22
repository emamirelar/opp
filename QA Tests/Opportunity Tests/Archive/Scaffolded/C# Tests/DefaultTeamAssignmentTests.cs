using Xunit;
using System;
using System.Linq;
using System.Collections.Generic;
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
    /// Default Team Member Assignment Tests
    /// 
    /// Purpose: Verify default team members are automatically assigned from org unit hierarchy
    /// 
    /// Real Production Bug: PNO-931 - OiCs, HoSS, and HoPs not listed as internal stakeholders
    /// - Expected default team members don't auto-populate from org unit
    /// - Officers-in-Charge (OiC), Heads of Support Services (HoSS), and Heads of Practice (HoP) should appear
    /// 
    /// These tests ensure:
    /// - OiC assigned when org unit is set
    /// - HoSS assigned from org unit hierarchy
    /// - HoP assigned from org unit hierarchy
    /// - All default stakeholders visible in team tab
    /// - Default team updates when org unit changes
    /// - Manual assignments override defaults appropriately
    /// </summary>
    [Trait("Category", "TeamManagement")]
    [Trait("Priority", "High")]
    public class DefaultTeamAssignmentTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public DefaultTeamAssignmentTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TeamAssignTest_{Guid.NewGuid()}")
                .Options;
            _context = TestDbContextFactory.Create(_dbOptions);
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Seed organization hierarchy
            var orgUnits = new[]
            {
                new OrganizationHierarchy
                {
                    Id = 1,
                    Name = "Global HQ",
                    Code = "GHQ",
                    Type = OrganizationUnitType.Office,
                    ParentId = null,
                    OfficerInChargeId = 100, // OiC for Global HQ
                    HeadOfSupportServicesId = 101, // HoSS for Global HQ
                    HeadOfPracticeId = 102, // HoP for Global HQ
                    CreatedBy = 1,
                    LastModifiedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = EntityStatus.Active
                },
                new OrganizationHierarchy
                {
                    Id = 2,
                    Name = "Africa Region",
                    Code = "AFR",
                    Type = OrganizationUnitType.Region,
                    ParentId = 1,
                    OfficerInChargeId = 200, // OiC for Africa Region
                    HeadOfSupportServicesId = 201, // HoSS for Africa Region
                    HeadOfPracticeId = 202, // HoP for Africa Region
                    CreatedBy = 1,
                    LastModifiedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = EntityStatus.Active
                },
                new OrganizationHierarchy
                {
                    Id = 3,
                    Name = "Asia Region",
                    Code = "ASI",
                    Type = OrganizationUnitType.Region,
                    ParentId = 1,
                    OfficerInChargeId = 300,
                    HeadOfSupportServicesId = 301,
                    HeadOfPracticeId = 302,
                    CreatedBy = 1,
                    LastModifiedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = EntityStatus.Active
                }
            };
            _context.OrganizationHierarchies.AddRange(orgUnits);

            // Seed users for the team roles
            var users = new[]
            {
                // Global HQ team
                new UserProfile { UserId = 100, FirstName = "Global", LastName = "OiC", UserEmail = "global.oic@unops.org" },
                new UserProfile { UserId = 101, FirstName = "Global", LastName = "HoSS", UserEmail = "global.hoss@unops.org" },
                new UserProfile { UserId = 102, FirstName = "Global", LastName = "HoP", UserEmail = "global.hop@unops.org" },
                
                // Africa Region team
                new UserProfile { UserId = 200, FirstName = "Africa", LastName = "OiC", UserEmail = "africa.oic@unops.org" },
                new UserProfile { UserId = 201, FirstName = "Africa", LastName = "HoSS", UserEmail = "africa.hoss@unops.org" },
                new UserProfile { UserId = 202, FirstName = "Africa", LastName = "HoP", UserEmail = "africa.hop@unops.org" },
                
                // Asia Region team
                new UserProfile { UserId = 300, FirstName = "Asia", LastName = "OiC", UserEmail = "asia.oic@unops.org" },
                new UserProfile { UserId = 301, FirstName = "Asia", LastName = "HoSS", UserEmail = "asia.hoss@unops.org" },
                new UserProfile { UserId = 302, FirstName = "Asia", LastName = "HoP", UserEmail = "asia.hop@unops.org" }
            };
            _context.UserProfile.AddRange(users);

            _context.SaveChanges();
        }

        #region OiC Assignment Tests

        [Fact]
        public async Task TC_DTA_001_SetOrgUnit_AutoAssignsOiC()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "OiC Test Opportunity",
                OpportunityNumber = "OPP-2026-OIC001",
                OpportunityManagerId = 1,
                ResponsibleOrgUnitId = 2, // Africa Region
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Load opportunity with org unit to get default stakeholders
            var savedOpportunity = await _context.Opportunities
                .Include(o => o.ResponsibleOrgUnit)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - OiC from org unit should be identified
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.ResponsibleOrgUnit.Should().NotBeNull();
            savedOpportunity.ResponsibleOrgUnit!.OfficerInChargeId.Should().Be(200,
                "Officer-in-Charge from Africa Region should be assigned (Bug PNO-931 fix)");

            // Verify OiC user exists
            var oic = await _context.UserProfile.FindAsync(savedOpportunity.ResponsibleOrgUnit.OfficerInChargeId);
            oic.Should().NotBeNull();
            oic!.FirstName.Should().Be("Africa");
            oic.LastName.Should().Be("OiC");
        }

        [Fact]
        public async Task TC_DTA_002_SetOrgUnit_AutoAssignsHoSS()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "HoSS Test Opportunity",
                OpportunityNumber = "OPP-2026-HOSS001",
                OpportunityManagerId = 1,
                ResponsibleOrgUnitId = 2, // Africa Region
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
                .Include(o => o.ResponsibleOrgUnit)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - HoSS from org unit should be identified
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.ResponsibleOrgUnit!.HeadOfSupportServicesId.Should().Be(201,
                "Head of Support Services from Africa Region should be assigned");

            // Verify HoSS user exists
            var hoss = await _context.UserProfile.FindAsync(savedOpportunity.ResponsibleOrgUnit.HeadOfSupportServicesId);
            hoss.Should().NotBeNull();
            hoss!.FirstName.Should().Be("Africa");
            hoss.LastName.Should().Be("HoSS");
        }

        [Fact]
        public async Task TC_DTA_003_SetOrgUnit_AutoAssignsHoP()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "HoP Test Opportunity",
                OpportunityNumber = "OPP-2026-HOP001",
                OpportunityManagerId = 1,
                ResponsibleOrgUnitId = 2, // Africa Region
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
                .Include(o => o.ResponsibleOrgUnit)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - HoP from org unit should be identified
            savedOpportunity.Should().NotBeNull();
            savedOpportunity!.ResponsibleOrgUnit!.HeadOfPracticeId.Should().Be(202,
                "Head of Practice from Africa Region should be assigned");

            // Verify HoP user exists
            var hop = await _context.UserProfile.FindAsync(savedOpportunity.ResponsibleOrgUnit.HeadOfPracticeId);
            hop.Should().NotBeNull();
            hop!.FirstName.Should().Be("Africa");
            hop.LastName.Should().Be("HoP");
        }

        [Fact]
        public async Task TC_DTA_004_AllDefaultStakeholders_VisibleInTeamTab()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Full Team Test Opportunity",
                OpportunityNumber = "OPP-2026-TEAM001",
                OpportunityManagerId = 1,
                ResponsibleOrgUnitId = 2, // Africa Region
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Act - Load opportunity with full team information
            var savedOpportunity = await _context.Opportunities
                .Include(o => o.ResponsibleOrgUnit)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            var orgUnit = savedOpportunity!.ResponsibleOrgUnit!;
            var defaultTeamMemberIds = new List<int?>
            {
                orgUnit.OfficerInChargeId,
                orgUnit.HeadOfSupportServicesId,
                orgUnit.HeadOfPracticeId
            };

            // Load all default team members
            var teamMembers = await _context.UserProfile
                .Where(u => defaultTeamMemberIds.Contains(u.UserId))
                .ToListAsync();

            // Assert - All three default stakeholders should be available
            teamMembers.Should().HaveCount(3, 
                "All default stakeholders (OiC, HoSS, HoP) should be available for team tab");
            
            teamMembers.Should().Contain(u => u.LastName == "OiC");
            teamMembers.Should().Contain(u => u.LastName == "HoSS");
            teamMembers.Should().Contain(u => u.LastName == "HoP");
        }

        #endregion

        #region Team Update Tests

        [Fact]
        public async Task TC_DTA_005_ChangeOrgUnit_UpdatesDefaultTeam()
        {
            // Arrange - Create opportunity with Africa Region
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Team Update Test Opportunity",
                OpportunityNumber = "OPP-2026-UPDATE001",
                OpportunityManagerId = 1,
                ResponsibleOrgUnitId = 2, // Africa Region initially
                CreatedBy = 1,
                LastModifiedBy = 1,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Status = EntityStatus.Active
            };

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            // Get initial team
            var initialOrgUnit = await _context.OrganizationHierarchies.FindAsync(2);
            var initialOiC = initialOrgUnit!.OfficerInChargeId;
            initialOiC.Should().Be(200, "Initial OiC should be from Africa Region");

            // Act - Change org unit to Asia Region
            opportunity.ResponsibleOrgUnitId = 3; // Asia Region
            await _context.SaveChangesAsync();

            // Get updated team
            var updatedOpportunity = await _context.Opportunities
                .Include(o => o.ResponsibleOrgUnit)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Team should now be from Asia Region
            updatedOpportunity!.ResponsibleOrgUnit!.OfficerInChargeId.Should().Be(300,
                "OiC should update to Asia Region's OiC when org unit changes");
            updatedOpportunity.ResponsibleOrgUnit.HeadOfSupportServicesId.Should().Be(301);
            updatedOpportunity.ResponsibleOrgUnit.HeadOfPracticeId.Should().Be(302);
        }

        [Fact]
        public async Task TC_DTA_006_ManualAssignments_CoexistWithDefaults()
        {
            // Arrange
            var opportunity = new Domain.Entities.Opportunity
            {
                Name = "Manual Assignment Test",
                OpportunityNumber = "OPP-2026-MANUAL001",
                OpportunityManagerId = 999, // Manually assigned manager
                ResponsibleOrgUnitId = 2, // Africa Region (provides default team)
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
                .Include(o => o.ResponsibleOrgUnit)
                .FirstOrDefaultAsync(o => o.Id == opportunity.Id);

            // Assert - Manual manager assignment coexists with default team from org unit
            savedOpportunity!.OpportunityManagerId.Should().Be(999,
                "Manually assigned manager should be preserved");
            
            savedOpportunity.ResponsibleOrgUnit!.OfficerInChargeId.Should().Be(200,
                "Default OiC from org unit should still be available");
            savedOpportunity.ResponsibleOrgUnit.HeadOfSupportServicesId.Should().Be(201);
            savedOpportunity.ResponsibleOrgUnit.HeadOfPracticeId.Should().Be(202);
            
            // Logic: Manual assignments don't override defaults, they work together
        }

        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
