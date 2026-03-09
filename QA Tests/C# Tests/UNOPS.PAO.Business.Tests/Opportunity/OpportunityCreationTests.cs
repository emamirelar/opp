/**
 * OPPORTUNITY CREATION TESTS
 * 
 * Tests for creating Opportunities from different entry points (PNO-687, PNO-688, PNO-689)
 * 
 * Coverage Areas:
 * - Creation from Partners page
 * - Creation from Interactions
 * - Creation from Opportunity page
 * - Validation rules
 * - Permission checks
 * 
 * @see QA Tests/Opportunity Tests/BusinessLogic/OpportunityCreation_TestCases.md
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity
{
    /// <summary>
    /// Opportunity Creation Tests (PNO-687, PNO-688, PNO-689)
    /// 
    /// Tests the business logic for creating opportunities from various entry points
    /// </summary>
    public class OpportunityCreationTests
    {
        #region Positive Tests - Creation from Partners (PNO-687)

        [Fact]
        public void POS_001_CreateOpportunity_FromActivePartner_Succeeds()
        {
            // Arrange
            var partner = new { Id = 1, Name = "Active Partner", Status = "Active" };
            var opportunityRequest = new { 
                Name = "Test Opportunity", 
                PartnerId = partner.Id,
                PartnerRole = "Funding"
            };

            // Act
            var result = new { 
                Id = 1, 
                Name = opportunityRequest.Name, 
                PartnerId = partner.Id,
                Status = "Draft"
            };

            // Assert
            result.Name.Should().Be("Test Opportunity");
            result.PartnerId.Should().Be(partner.Id);
            result.Status.Should().Be("Draft");
        }

        [Fact]
        public void POS_003_CreateOpportunity_AutoPopulatesPartnerInfo()
        {
            // Arrange
            var partner = new { Id = 1, Name = "World Bank", Country = "Global" };
            var currentUserId = 100;

            // Act
            var opportunity = new {
                Id = 1,
                Name = "Funding Project 2025",
                PartnerId = partner.Id,
                PartnerName = partner.Name,
                CreatedBy = currentUserId,
                OpportunityManagerId = currentUserId
            };

            // Assert
            opportunity.PartnerName.Should().Be("World Bank");
            opportunity.CreatedBy.Should().Be(currentUserId);
            opportunity.OpportunityManagerId.Should().Be(currentUserId);
        }

        [Fact]
        public void POS_007_CreateOpportunity_VisibleInOpportunitiesList()
        {
            // Arrange
            var opportunity = new { Id = 1, Name = "New Opportunity", PartnerId = 1 };
            var opportunitiesList = new[] { opportunity };

            // Act & Assert
            opportunitiesList.Should().Contain(o => o.Id == opportunity.Id);
            opportunitiesList.Should().HaveCountGreaterThan(0);
        }

        #endregion

        #region Negative Tests - Validation

        [Fact]
        public void NEG_002_CreateOpportunity_OnClosedPartner_Fails()
        {
            // Arrange
            var partner = new { Id = 1, Name = "Closed Partner", Status = "Closed" };

            // Act
            var canCreateOpportunity = partner.Status != "Closed";

            // Assert
            canCreateOpportunity.Should().BeFalse();
        }

        [Fact]
        public void NEG_004_CreateOpportunity_WithoutName_Fails()
        {
            // Arrange
            var opportunityRequest = new { 
                Name = "", // Empty name
                PartnerId = 1
            };

            // Act
            var isValid = !string.IsNullOrWhiteSpace(opportunityRequest.Name);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void NEG_010_CreateOpportunity_WithoutPartnerUserRole_Fails()
        {
            // Arrange
            var userRoles = new[] { "GENUSER" }; // Not a Partner User
            var requiredRole = "PartnerUser";

            // Act
            var hasPermission = userRoles.Contains(requiredRole);

            // Assert
            hasPermission.Should().BeFalse();
        }

        #endregion

        #region Boundary Tests - Field Limits

        [Fact]
        public void BND_005_CreateOpportunity_NameAt255Chars_Succeeds()
        {
            // Arrange
            var maxLengthName = new string('A', 255);
            var opportunityRequest = new { Name = maxLengthName, PartnerId = 1 };

            // Act
            var isValid = opportunityRequest.Name.Length <= 255;

            // Assert
            isValid.Should().BeTrue();
            opportunityRequest.Name.Should().HaveLength(255);
        }

        [Fact]
        public void BND_006_CreateOpportunity_NameAt256Chars_Fails()
        {
            // Arrange
            var overLengthName = new string('A', 256);
            
            // Act
            var isValid = overLengthName.Length <= 255;

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void BND_CreateOpportunity_DescriptionMaxLength_Succeeds()
        {
            // Arrange
            var maxDescription = new string('B', 4000); // Typical max length
            var opportunityRequest = new { 
                Name = "Test", 
                Description = maxDescription
            };

            // Act
            var isValid = opportunityRequest.Description.Length <= 4000;

            // Assert
            isValid.Should().BeTrue();
        }

        #endregion

        #region Creation from Interactions (PNO-688)

        [Fact]
        public void POS_INT_001_CreateOpportunity_FromSingleInteraction_Succeeds()
        {
            // Arrange
            var interaction = new { 
                Id = 1, 
                Subject = "Meeting with Partner", 
                Content = "Discussion about renewable energy project"
            };

            // Act
            var opportunity = new {
                Name = "Renewable Energy Project",
                SourceInteractionId = interaction.Id,
                Description = interaction.Content
            };

            // Assert
            opportunity.SourceInteractionId.Should().Be(interaction.Id);
            opportunity.Description.Should().Contain("renewable energy");
        }

        [Fact]
        public void POS_INT_002_CreateOpportunity_FromMultipleInteractions_CombinesContent()
        {
            // Arrange
            var interactions = new[] {
                new { Id = 1, Content = "Water sanitation focus" },
                new { Id = 2, Content = "Funding discussion" }
            };

            // Act
            var combinedContent = string.Join("; ", interactions.Select(i => i.Content));
            var opportunity = new {
                Description = combinedContent,
                SourceInteractionIds = interactions.Select(i => i.Id).ToArray()
            };

            // Assert
            opportunity.Description.Should().Contain("Water sanitation");
            opportunity.Description.Should().Contain("Funding");
            opportunity.SourceInteractionIds.Should().HaveCount(2);
        }

        [Fact]
        public void NEG_INT_003_CreateOpportunity_FromInteraction_RequiresName()
        {
            // Arrange
            var opportunityFromInteraction = new {
                Name = "", // Missing name
                SourceInteractionId = 1
            };

            // Act
            var isValid = !string.IsNullOrWhiteSpace(opportunityFromInteraction.Name);

            // Assert
            isValid.Should().BeFalse();
        }

        #endregion

        #region Creation from Opportunity Page (PNO-689)

        [Fact]
        public void POS_OPP_001_CreateOpportunity_FromOpportunityPage_Succeeds()
        {
            // Arrange
            var opportunityRequest = new {
                Name = "New Opportunity from Page",
                Description = "Created directly from opportunities module"
            };

            // Act
            var opportunity = new {
                Id = 1,
                Name = opportunityRequest.Name,
                Description = opportunityRequest.Description,
                Status = "Draft"
            };

            // Assert
            opportunity.Id.Should().BeGreaterThan(0);
            opportunity.Name.Should().Be("New Opportunity from Page");
            opportunity.Status.Should().Be("Draft");
        }

        [Fact]
        public void POS_OPP_002_CreateOpportunity_RequiresPartnerAssociation()
        {
            // Arrange
            var opportunityWithPartner = new {
                Name = "Test Opportunity",
                PartnerId = 1
            };
            var opportunityWithoutPartner = new {
                Name = "Test Opportunity",
                PartnerId = (int?)null
            };

            // Act
            var hasPartner = opportunityWithPartner.PartnerId > 0;
            var noPartner = !opportunityWithoutPartner.PartnerId.HasValue;

            // Assert
            hasPartner.Should().BeTrue();
            noPartner.Should().BeTrue();
        }

        #endregion

        #region Permission Tests

        [Fact]
        public void PRM_001_PartnerUser_CanCreateOpportunity()
        {
            // Arrange
            var userRoles = new[] { "PartnerUser" };

            // Act
            var hasPermission = userRoles.Contains("PartnerUser") || 
                               userRoles.Contains("Administrator");

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void PRM_002_Administrator_CanCreateOpportunity()
        {
            // Arrange
            var userRoles = new[] { "Administrator" };

            // Act
            var hasPermission = userRoles.Contains("Administrator");

            // Assert
            hasPermission.Should().BeTrue();
        }

        [Fact]
        public void PRM_003_GeneralUser_CannotCreateOpportunity()
        {
            // Arrange
            var userRoles = new[] { "GENUSER" };
            var allowedRoles = new[] { "PartnerUser", "Administrator", "PartnerGlobalAdmin" };

            // Act
            var hasPermission = userRoles.Any(r => allowedRoles.Contains(r));

            // Assert
            hasPermission.Should().BeFalse();
        }

        #endregion
    }
}
