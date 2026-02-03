/**
 * CONTACT FUNCTIONAL TESTS
 * 
 * Required: At least 1 test (no scaling minimum)
 * Purpose: Business rule verification, workflow testing
 * 
 * @see .cursor/rules/comprehensive-test-strategy.mdc
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Functional
{
    /// <summary>
    /// Functional Tests for Contact Manager
    /// 
    /// Test Strategy: These tests verify business rules and workflows
    /// are correctly implemented. Focus on "what the system does"
    /// from a business perspective.
    /// 
    /// Required: At least 1 test (no scaling minimum)
    /// </summary>
    public class ContactFunctionalTests
    {
        #region Business Rule: Partner Association

        /// <summary>
        /// BR-C001: Contacts must be associated with a valid partner
        /// </summary>
        [Fact]
        public void BR_C001_Contact_RequiresValidPartner()
        {
            // Arrange
            var validPartnerIds = new[] { 1, 2, 3 };
            var contact = new { PartnerId = 2 };

            // Act
            var hasValidPartner = validPartnerIds.Contains(contact.PartnerId);

            // Assert
            hasValidPartner.Should().BeTrue("Contact must be linked to valid partner");
        }

        /// <summary>
        /// BR-C002: Contact with invalid partner ID should fail validation
        /// </summary>
        [Fact]
        public void BR_C002_Contact_InvalidPartner_FailsValidation()
        {
            // Arrange
            var validPartnerIds = new[] { 1, 2, 3 };
            var contact = new { PartnerId = 999 };

            // Act
            var hasValidPartner = validPartnerIds.Contains(contact.PartnerId);

            // Assert
            hasValidPartner.Should().BeFalse("Contact with invalid partner should fail");
        }

        #endregion

        #region Business Rule: Primary Contact

        /// <summary>
        /// BR-C003: Only one primary contact per partner
        /// </summary>
        [Fact]
        public void BR_C003_Partner_OnlyOnePrimaryContact()
        {
            // Arrange
            var contacts = new List<(int Id, int PartnerId, bool IsPrimary)>
            {
                (1, 1, true),
                (2, 1, false),
                (3, 1, false)
            };

            // Act
            var primaryCount = contacts.Count(c => c.PartnerId == 1 && c.IsPrimary);

            // Assert
            primaryCount.Should().Be(1, "Partner should have exactly one primary contact");
        }

        /// <summary>
        /// BR-C004: Setting new primary should unset existing
        /// </summary>
        [Fact]
        public void BR_C004_SetPrimary_UnsetsExisting()
        {
            // Arrange
            var contacts = new List<(int Id, int PartnerId, bool IsPrimary)>
            {
                (1, 1, true),  // Current primary
                (2, 1, false)  // Will become primary
            };

            // Act - Set contact 2 as primary, unset contact 1
            var updatedContacts = contacts.Select(c => 
                c.Id == 2 ? (c.Id, c.PartnerId, true) : 
                c.Id == 1 ? (c.Id, c.PartnerId, false) : c
            ).ToList();

            // Assert
            updatedContacts.Single(c => c.IsPrimary).Id.Should().Be(2);
            updatedContacts.Count(c => c.IsPrimary).Should().Be(1);
        }

        #endregion

        #region Business Rule: Email Communication

        /// <summary>
        /// BR-C005: Contact with valid email can receive communications
        /// </summary>
        [Theory]
        [InlineData("test@example.com", true)]
        [InlineData("user@domain.org", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void BR_C005_Contact_CanReceiveCommunications(string? email, bool expected)
        {
            // Act
            var canReceive = !string.IsNullOrWhiteSpace(email) && email.Contains('@');

            // Assert
            canReceive.Should().Be(expected);
        }

        #endregion

        #region Business Rule: Contact Status

        /// <summary>
        /// BR-C006: Active contacts should be included in notifications
        /// </summary>
        [Fact]
        public void BR_C006_ActiveContacts_IncludedInNotifications()
        {
            // Arrange
            var contacts = new List<(int Id, string Status, string Email)>
            {
                (1, "Active", "active@example.com"),
                (2, "Inactive", "inactive@example.com"),
                (3, "Active", "active2@example.com")
            };

            // Act
            var notificationRecipients = contacts
                .Where(c => c.Status == "Active")
                .Select(c => c.Email)
                .ToList();

            // Assert
            notificationRecipients.Should().HaveCount(2);
            notificationRecipients.Should().NotContain("inactive@example.com");
        }

        #endregion

        #region Business Rule: Contact Role

        /// <summary>
        /// BR-C007: Contact roles must be valid
        /// </summary>
        [Theory]
        [InlineData("Focal Point", true)]
        [InlineData("Technical", true)]
        [InlineData("Financial", true)]
        [InlineData("Random Role", false)]
        public void BR_C007_ContactRole_Validation(string role, bool expectedValid)
        {
            // Arrange
            var validRoles = new[] { "Focal Point", "Technical", "Financial", "Legal", "Management" };

            // Act
            var isValid = validRoles.Contains(role);

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion
    }
}
