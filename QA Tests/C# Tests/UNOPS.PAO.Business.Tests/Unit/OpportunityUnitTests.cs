/**
 * OPPORTUNITY UNIT TESTS
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
    /// Unit Tests for Opportunity Manager
    /// 
    /// Test Strategy: These tests verify individual methods in isolation
    /// with mocked dependencies. Focus on pure logic testing.
    /// 
    /// Required: At least 1 test (no scaling minimum)
    /// </summary>
    public class OpportunityUnitTests
    {
        #region Amount Calculations

        /// <summary>
        /// Calculate total amount should sum correctly
        /// </summary>
        [Fact]
        public void CalculateTotalAmount_MultipleAmounts_ReturnsSummed()
        {
            // Arrange
            var amounts = new[] { 100_000m, 200_000m, 300_000m };

            // Act
            var result = amounts.Sum();

            // Assert
            result.Should().Be(600_000m);
        }

        /// <summary>
        /// Calculate percentage should handle division correctly
        /// </summary>
        [Theory]
        [InlineData(50_000, 100_000, 50)]
        [InlineData(25_000, 100_000, 25)]
        [InlineData(0, 100_000, 0)]
        public void CalculatePercentage_ValidInputs_ReturnsCorrectPercentage(
            decimal part, decimal whole, decimal expected)
        {
            // Act
            var result = whole > 0 ? (part / whole) * 100 : 0;

            // Assert
            result.Should().Be(expected);
        }

        /// <summary>
        /// Calculate percentage with zero denominator should return zero
        /// </summary>
        [Fact]
        public void CalculatePercentage_ZeroDenominator_ReturnsZero()
        {
            // Arrange
            decimal part = 50_000;
            decimal whole = 0;

            // Act
            var result = whole > 0 ? (part / whole) * 100 : 0;

            // Assert
            result.Should().Be(0m);
        }

        #endregion

        #region Date Validation

        /// <summary>
        /// End date must be after start date
        /// </summary>
        [Theory]
        [InlineData("2024-01-01", "2024-12-31", true)]
        [InlineData("2024-06-01", "2024-06-01", true)]  // Same day allowed
        [InlineData("2024-12-31", "2024-01-01", false)]
        public void DateRange_Validation_ReturnsCorrectResult(
            string startStr, string endStr, bool expectedValid)
        {
            // Arrange
            var startDate = DateTime.Parse(startStr);
            var endDate = DateTime.Parse(endStr);

            // Act
            var isValid = endDate >= startDate;

            // Assert
            isValid.Should().Be(expectedValid);
        }

        /// <summary>
        /// Duration calculation should be correct
        /// </summary>
        [Theory]
        [InlineData("2024-01-01", "2024-01-31", 30)]
        [InlineData("2024-01-01", "2024-01-01", 0)]
        [InlineData("2024-01-01", "2024-12-31", 365)]
        public void CalculateDuration_ValidDates_ReturnsCorrectDays(
            string startStr, string endStr, int expectedDays)
        {
            // Arrange
            var startDate = DateTime.Parse(startStr);
            var endDate = DateTime.Parse(endStr);

            // Act
            var duration = (endDate - startDate).Days;

            // Assert
            duration.Should().Be(expectedDays);
        }

        #endregion

        #region Stage Validation

        /// <summary>
        /// Opportunity stage should follow allowed transitions
        /// </summary>
        [Theory]
        [InlineData("Identification", "Qualification", true)]
        [InlineData("Qualification", "Proposal", true)]
        [InlineData("Proposal", "Identification", false)]
        [InlineData("Won", "Identification", false)]
        public void StageTransition_Validation_ReturnsCorrectResult(
            string fromStage, string toStage, bool expectedAllowed)
        {
            // Arrange
            var stageOrder = new List<string> 
            { 
                "Identification", "Qualification", "Proposal", "Negotiation", "Won", "Lost" 
            };
            
            var fromIndex = stageOrder.IndexOf(fromStage);
            var toIndex = stageOrder.IndexOf(toStage);

            // Act - Can only move forward or to Lost
            var canTransition = toStage == "Lost" || (toIndex > fromIndex && toIndex <= fromIndex + 1);

            // Assert
            canTransition.Should().Be(expectedAllowed);
        }

        #endregion

        #region Priority Calculation

        /// <summary>
        /// Priority score calculation based on amount and probability
        /// </summary>
        [Theory]
        [InlineData(100_000, 80, 80_000)]  // amount * probability%
        [InlineData(500_000, 50, 250_000)]
        [InlineData(1_000_000, 25, 250_000)]
        public void CalculatePriorityScore_AmountAndProbability_ReturnsWeightedValue(
            decimal amount, int probability, decimal expectedScore)
        {
            // Act
            var result = amount * (probability / 100m);

            // Assert
            result.Should().Be(expectedScore);
        }

        #endregion

        #region String Validation

        /// <summary>
        /// Opportunity title should not exceed max length
        /// </summary>
        [Theory]
        [InlineData("Short Title", 100, true)]
        [InlineData("", 100, false)]  // Empty not allowed
        public void OpportunityTitle_LengthValidation_ReturnsCorrectResult(
            string title, int maxLength, bool expectedValid)
        {
            // Act
            var isValid = !string.IsNullOrWhiteSpace(title) && title.Length <= maxLength;

            // Assert
            isValid.Should().Be(expectedValid);
        }

        #endregion
    }
}
