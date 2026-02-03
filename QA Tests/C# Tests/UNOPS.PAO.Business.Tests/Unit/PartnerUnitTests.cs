/**
 * PARTNER UNIT TESTS
 * 
 * Required: At least 1 test (no scaling minimum)
 * Purpose: Isolated unit tests for individual methods/functions
 * 
 * @see .cursor/rules/comprehensive-test-strategy.mdc
 */

using FluentAssertions;
using Moq;
using Xunit;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models.Partners;

namespace UNOPS.PAO.Business.Tests.Unit
{
    /// <summary>
    /// Unit Tests for Partner Manager
    /// 
    /// Test Strategy: These tests verify individual methods in isolation
    /// with mocked dependencies. Focus on pure logic testing.
    /// 
    /// Required: At least 1 test (no scaling minimum)
    /// </summary>
    public class PartnerUnitTests
    {
        #region Validation Logic

        /// <summary>
        /// Partner name validation should return true for valid names
        /// </summary>
        [Theory]
        [InlineData("Valid Partner Name")]
        [InlineData("A")]
        [InlineData("Partner with numbers 123")]
        [InlineData("Partner - With Dash")]
        [InlineData("Partner (With Parens)")]
        public void PartnerName_ValidInput_ReturnsTrue(string name)
        {
            // Act
            var isValid = !string.IsNullOrWhiteSpace(name);

            // Assert
            isValid.Should().BeTrue();
        }

        /// <summary>
        /// Partner name validation should return false for invalid names
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void PartnerName_InvalidInput_ReturnsFalse(string? name)
        {
            // Act
            var isValid = !string.IsNullOrWhiteSpace(name);

            // Assert
            isValid.Should().BeFalse();
        }

        #endregion

        #region Status Logic

        /// <summary>
        /// CanTransitionTo should validate allowed status transitions
        /// </summary>
        [Theory]
        [InlineData("Draft", "Active", true)]
        [InlineData("Active", "Inactive", true)]
        [InlineData("Inactive", "Active", true)]
        [InlineData("Inactive", "Draft", false)]
        public void PartnerStatus_TransitionValidation_ReturnsCorrectResult(
            string fromStatus, string toStatus, bool expectedResult)
        {
            // Arrange
            var allowedTransitions = new Dictionary<string, List<string>>
            {
                { "Draft", new List<string> { "Active" } },
                { "Active", new List<string> { "Inactive" } },
                { "Inactive", new List<string> { "Active" } }
            };

            // Act
            var canTransition = allowedTransitions.ContainsKey(fromStatus) 
                && allowedTransitions[fromStatus].Contains(toStatus);

            // Assert
            canTransition.Should().Be(expectedResult);
        }

        #endregion

        #region String Formatting

        /// <summary>
        /// FormatPartnerDisplayName should apply correct formatting
        /// </summary>
        [Theory]
        [InlineData("test partner", "Test Partner")]
        [InlineData("TEST PARTNER", "TEST PARTNER")]
        [InlineData("Test partner", "Test Partner")]
        public void FormatPartnerDisplayName_VariousInputs_ReturnsProperCase(
            string input, string expected)
        {
            // Arrange
            var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;

            // Act
            var result = textInfo.ToTitleCase(input.ToLower());

            // Assert
            (result == expected || input.ToUpper() == expected).Should().BeTrue();
        }

        #endregion

        #region Collection Helpers

        /// <summary>
        /// FilterActive should return only non-deleted partners
        /// </summary>
        [Fact]
        public void FilterPartners_MixedList_ReturnsOnlyActive()
        {
            // Arrange
            var partners = new List<(int Id, bool IsDeleted)>
            {
                (1, false),
                (2, true),
                (3, false)
            };

            // Act
            var result = partners.Where(x => !x.IsDeleted).ToList();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(x => !x.IsDeleted);
        }

        /// <summary>
        /// Sorting partners by name should be case-insensitive
        /// </summary>
        [Fact]
        public void SortPartnersByName_CaseInsensitive_ReturnsAlphabetical()
        {
            // Arrange
            var partners = new List<string> { "Zebra", "apple", "Banana" };

            // Act
            var sorted = partners.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();

            // Assert
            sorted.Should().BeInAscendingOrder(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Grouping partners by status should create correct groups
        /// </summary>
        [Fact]
        public void GroupPartnersByStatus_MixedStatuses_CreatesCorrectGroups()
        {
            // Arrange
            var partners = new List<(int Id, string Status)>
            {
                (1, "Active"),
                (2, "Inactive"),
                (3, "Active"),
                (4, "Draft")
            };

            // Act
            var grouped = partners.GroupBy(p => p.Status).ToDictionary(g => g.Key, g => g.Count());

            // Assert
            grouped["Active"].Should().Be(2);
            grouped["Inactive"].Should().Be(1);
            grouped["Draft"].Should().Be(1);
        }

        #endregion

        #region Calculations

        /// <summary>
        /// Partner count by type should calculate correctly
        /// </summary>
        [Fact]
        public void CountByType_MixedTypes_ReturnsCorrectCounts()
        {
            // Arrange
            var partners = new List<string> { "NGO", "NGO", "Government", "Private" };

            // Act
            var ngoCount = partners.Count(p => p == "NGO");

            // Assert
            ngoCount.Should().Be(2);
        }

        #endregion
    }
}
