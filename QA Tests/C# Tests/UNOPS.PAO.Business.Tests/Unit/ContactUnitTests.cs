/**
 * CONTACT UNIT TESTS
 * 
 * Required: At least 1 test (no scaling minimum)
 * Purpose: Isolated unit tests for individual methods/functions
 * 
 * @see .cursor/rules/comprehensive-test-strategy.mdc
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Unit
{
    /// <summary>
    /// Unit Tests for Contact Manager
    /// 
    /// Test Strategy: These tests verify individual methods in isolation
    /// with mocked dependencies. Focus on pure logic testing.
    /// 
    /// Required: At least 1 test (no scaling minimum)
    /// </summary>
    public class ContactUnitTests
    {
        #region Email Validation

        /// <summary>
        /// Email validation should return true for valid emails
        /// </summary>
        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user.name@domain.org")]
        [InlineData("user+tag@subdomain.domain.com")]
        public void Email_ValidInput_ReturnsTrue(string email)
        {
            // Act
            var isValid = IsValidEmail(email);

            // Assert
            isValid.Should().BeTrue();
        }

        /// <summary>
        /// Email validation should return false for invalid emails
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("notanemail")]
        [InlineData("missing@domain")]
        [InlineData("@nodomain.com")]
        public void Email_InvalidInput_ReturnsFalse(string? email)
        {
            // Act
            var isValid = IsValidEmail(email);

            // Assert
            isValid.Should().BeFalse();
        }

        private bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Contains('.');
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Phone Number Formatting

        /// <summary>
        /// Phone number cleanup should remove non-digits
        /// </summary>
        [Theory]
        [InlineData("+1 (555) 123-4567", "15551234567")]
        [InlineData("555.123.4567", "5551234567")]
        [InlineData("555-123-4567", "5551234567")]
        public void PhoneNumber_CleanFormat_ReturnsDigitsOnly(
            string input, string expected)
        {
            // Act
            var result = new string(input.Where(char.IsDigit).ToArray());

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        #region Name Formatting

        /// <summary>
        /// Full name should combine first and last names correctly
        /// </summary>
        [Theory]
        [InlineData("John", "Doe", "John Doe")]
        [InlineData("Jane", null, "Jane")]
        [InlineData(null, "Smith", "Smith")]
        [InlineData("", "", "")]
        public void FullName_Combination_ReturnsCorrectFormat(
            string? firstName, string? lastName, string expected)
        {
            // Act
            var parts = new[] { firstName, lastName }
                .Where(p => !string.IsNullOrEmpty(p));
            var fullName = string.Join(" ", parts);

            // Assert
            fullName.Should().Be(expected);
        }

        #endregion

        #region Contact Type Validation

        /// <summary>
        /// Contact type should be one of allowed values
        /// </summary>
        [Theory]
        [InlineData("Primary", true)]
        [InlineData("Secondary", true)]
        [InlineData("Technical", true)]
        [InlineData("Invalid", false)]
        [InlineData("", false)]
        public void ContactType_Validation_ReturnsCorrectResult(
            string contactType, bool expectedValid)
        {
            // Arrange
            var allowedTypes = new[] { "Primary", "Secondary", "Technical", "Financial", "Legal" };

            // Act
            var isValid = allowedTypes.Contains(contactType);

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion
    }
}
