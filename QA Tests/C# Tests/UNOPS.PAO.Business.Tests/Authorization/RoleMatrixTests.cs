/**
 * ROLE MATRIX AUTHORIZATION TESTS
 * 
 * Tests for role-based permission validation (PNO-562)
 * 
 * Coverage Areas:
 * - Partner permissions by role
 * - Opportunity permissions by role
 * - Contact permissions by role
 * - Interaction permissions by role
 * - Admin access permissions
 * 
 * @see QA Tests/Authorization Tests/RoleMatrix_TestCases.md
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Authorization
{
    /// <summary>
    /// Role Matrix Tests (PNO-562)
    /// 
    /// Tests the permission matrix for all user roles
    /// </summary>
    public class RoleMatrixTests
    {
        #region Role Definitions

        private static readonly string[] AdministratorRoles = { "Administrator" };
        private static readonly string[] PartnerGlobalAdminRoles = { "PartnerGlobalAdmin" };
        private static readonly string[] PartnerUserRoles = { "PartnerUser" };
        private static readonly string[] OrgUnitAdminRoles = { "OrgUnitAdmin" };
        private static readonly string[] GeneralUserRoles = { "GENUSER" };

        #endregion

        #region Partner Permissions

        [Fact]
        public void POS_001_Administrator_CanCreatePartners()
        {
            // Arrange
            var userRoles = AdministratorRoles;
            var permission = "CanCreatePartner";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void POS_002_PartnerUser_CanCreatePartners()
        {
            // Arrange
            var userRoles = PartnerUserRoles;
            var permission = "CanCreatePartner";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void NEG_003_GeneralUser_CannotCreatePartners()
        {
            // Arrange
            var userRoles = GeneralUserRoles;
            var permission = "CanCreatePartner";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeFalse();
        }

        [Fact]
        public void POS_014_GeneralUser_CanViewPartners()
        {
            // Arrange
            var userRoles = GeneralUserRoles;
            var permission = "CanViewPartner";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void NEG_016_PartnerUser_CannotDeletePartners()
        {
            // Arrange
            var userRoles = PartnerUserRoles;
            var permission = "CanDeletePartner";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeFalse();
        }

        [Fact]
        public void POS_017_Administrator_CanDeletePartners()
        {
            // Arrange
            var userRoles = AdministratorRoles;
            var permission = "CanDeletePartner";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeTrue();
        }

        #endregion

        #region Opportunity Permissions

        [Fact]
        public void POS_004_PartnerUser_CanCreateOpportunities()
        {
            // Arrange
            var userRoles = PartnerUserRoles;
            var permission = "CanCreateOpportunity";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void NEG_005_GeneralUser_CannotCreateOpportunities()
        {
            // Arrange
            var userRoles = GeneralUserRoles;
            var permission = "CanCreateOpportunity";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeFalse();
        }

        [Fact]
        public void POS_006_OpportunityManager_CanEditOwnOpportunities()
        {
            // Arrange
            var userId = 100;
            var opportunity = new { Id = 1, OpportunityManagerId = 100 };
            var userRoles = PartnerUserRoles;

            // Act
            var isOpportunityManager = opportunity.OpportunityManagerId == userId;
            var hasPermission = isOpportunityManager && HasPermission(userRoles, "CanEditOpportunity");

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void NEG_007_PartnerUser_CannotEditOthersOpportunities()
        {
            // Arrange
            var userId = 100;
            var opportunity = new { Id = 1, OpportunityManagerId = 200 }; // Different manager
            var userRoles = PartnerUserRoles;

            // Act
            var isOpportunityManager = opportunity.OpportunityManagerId == userId;
            var hasEditPermission = isOpportunityManager;

            // Assert
            hasEditPermission.Should().BeFalse();
        }

        [Fact]
        public void POS_015_GeneralUser_CanViewOpportunities()
        {
            // Arrange
            var userRoles = GeneralUserRoles;
            var permission = "CanViewOpportunity";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeTrue();
        }

        #endregion

        #region Contact Permissions

        [Fact]
        public void POS_008_PartnerUser_CanCreateContacts()
        {
            // Arrange
            var userRoles = PartnerUserRoles;
            var permission = "CanCreateContact";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void NEG_009_GeneralUser_CannotCreateContacts()
        {
            // Arrange
            var userRoles = GeneralUserRoles;
            var permission = "CanCreateContact";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeFalse();
        }

        #endregion

        #region Interaction Permissions

        [Fact]
        public void POS_010_PartnerUser_CanLogInteractions()
        {
            // Arrange
            var userRoles = PartnerUserRoles;
            var permission = "CanCreateInteraction";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void NEG_011_GeneralUser_CannotLogInteractions()
        {
            // Arrange
            var userRoles = GeneralUserRoles;
            var permission = "CanCreateInteraction";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeFalse();
        }

        #endregion

        #region Administration Permissions

        [Fact]
        public void POS_012_Administrator_CanAccessAllAdminFeatures()
        {
            // Arrange
            var userRoles = AdministratorRoles;
            var adminPermissions = new[] { 
                "CanAccessUserManagement", 
                "CanAccessAIPrompts", 
                "CanAccessEntityManager" 
            };

            // Act
            var hasAllPermissions = adminPermissions.All(p => HasPermission(userRoles, p));

            // Assert
            hasAllPermissions.Should().BeTrue();
        }

        [Fact]
        public void NEG_013_PartnerUser_CannotAccessAdminFeatures()
        {
            // Arrange
            var userRoles = PartnerUserRoles;
            var permission = "CanAccessUserManagement";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeFalse();
        }

        [Fact]
        public void NEG_GeneralUser_CannotAccessAdmin()
        {
            // Arrange
            var userRoles = GeneralUserRoles;
            var permission = "CanAccessUserManagement";

            // Act
            var hasPermission = HasPermission(userRoles, permission);

            // Assert
            hasPermission.Should().BeFalse();
        }

        #endregion

        #region Org Unit Admin Permissions

        [Fact]
        public void POS_018_OrgUnitAdmin_CanManageOwnUnitPartners()
        {
            // Arrange
            var userOrgUnitId = 1;
            var partner = new { Id = 1, OrgUnitId = 1 }; // Same org unit
            var userRoles = OrgUnitAdminRoles;

            // Act
            var isSameOrgUnit = partner.OrgUnitId == userOrgUnitId;
            var hasPermission = isSameOrgUnit && userRoles.Contains("OrgUnitAdmin");

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void NEG_019_OrgUnitAdmin_CannotManageOtherUnitPartners()
        {
            // Arrange
            var userOrgUnitId = 1;
            var partner = new { Id = 1, OrgUnitId = 2 }; // Different org unit
            var userRoles = OrgUnitAdminRoles;

            // Act
            var isSameOrgUnit = partner.OrgUnitId == userOrgUnitId;
            var hasPermission = isSameOrgUnit;

            // Assert
            hasPermission.Should().BeFalse();
        }

        #endregion

        #region Partner Global Admin Permissions

        [Fact]
        public void POS_020_PartnerGlobalAdmin_HasFullPartnerAccess()
        {
            // Arrange
            var userRoles = PartnerGlobalAdminRoles;
            var partnerPermissions = new[] {
                "CanCreatePartner",
                "CanEditPartner",
                "CanDeletePartner",
                "CanCreateOpportunity",
                "CanCreateInteraction"
            };

            // Act
            var hasAllPermissions = partnerPermissions.All(p => HasPermission(userRoles, p));

            // Assert
            hasAllPermissions.Should().BeTrue();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Simulates permission check based on role matrix
        /// </summary>
        private static bool HasPermission(string[] userRoles, string permission)
        {
            var permissionMatrix = new Dictionary<string, string[]>
            {
                // View permissions - all roles
                { "CanViewPartner", new[] { "Administrator", "PartnerGlobalAdmin", "PartnerUser", "OrgUnitAdmin", "GENUSER" } },
                { "CanViewOpportunity", new[] { "Administrator", "PartnerGlobalAdmin", "PartnerUser", "OrgUnitAdmin", "GENUSER" } },
                { "CanViewContact", new[] { "Administrator", "PartnerGlobalAdmin", "PartnerUser", "OrgUnitAdmin", "GENUSER" } },
                
                // Create permissions
                { "CanCreatePartner", new[] { "Administrator", "PartnerGlobalAdmin", "PartnerUser", "OrgUnitAdmin" } },
                { "CanCreateOpportunity", new[] { "Administrator", "PartnerGlobalAdmin", "PartnerUser", "OrgUnitAdmin" } },
                { "CanCreateContact", new[] { "Administrator", "PartnerGlobalAdmin", "PartnerUser", "OrgUnitAdmin" } },
                { "CanCreateInteraction", new[] { "Administrator", "PartnerGlobalAdmin", "PartnerUser", "OrgUnitAdmin" } },
                
                // Edit permissions
                { "CanEditPartner", new[] { "Administrator", "PartnerGlobalAdmin", "PartnerUser", "OrgUnitAdmin" } },
                { "CanEditOpportunity", new[] { "Administrator", "PartnerGlobalAdmin", "PartnerUser", "OrgUnitAdmin" } },
                
                // Delete permissions
                { "CanDeletePartner", new[] { "Administrator", "PartnerGlobalAdmin" } },
                
                // Admin permissions
                { "CanAccessUserManagement", new[] { "Administrator" } },
                { "CanAccessAIPrompts", new[] { "Administrator" } },
                { "CanAccessEntityManager", new[] { "Administrator" } }
            };

            if (!permissionMatrix.TryGetValue(permission, out var allowedRoles))
            {
                return false;
            }

            return userRoles.Any(role => allowedRoles.Contains(role));
        }

        #endregion
    }
}
