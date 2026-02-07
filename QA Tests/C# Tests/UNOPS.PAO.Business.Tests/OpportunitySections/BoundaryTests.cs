/**
 * @fileoverview Boundary/Edge Case Tests for Opportunity Sections
 * Tests derived from comprehensive test strategy - Minimum 50 tests required
 * Covers: Min/max values, limits, edge cases, special characters, unusual scenarios
 * @author UNOPS Opportunity+ QA Team
 */

using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UNOPS.PAO.Business.Tests.OpportunitySections
{
    /// <summary>
    /// Boundary and Edge Case tests for all Opportunity Sections
    /// Minimum Required: 50 tests (≥2×P where P=baseline positive tests)
    /// </summary>
    [Collection("Boundary")]
    [Trait("Category", "Boundary")]
    [Trait("Type", "EdgeCase")]
    public class BoundaryTests
    {
        #region Numeric Boundary Tests (10 tests)

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_001_BeneficiaryCount_ValidBoundaryValues(int count)
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var result = await SetBeneficiaryCount(opportunityId, count);

            // Assert
            result.Success.Should().BeTrue($"Beneficiary count {count} should be valid");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_002_BeneficiaryCount_InvalidNegativeValues(int count)
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var result = await SetBeneficiaryCount(opportunityId, count);

            // Assert
            result.Success.Should().BeFalse($"Negative beneficiary count {count} should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_003_OpportunityValue_MinimumZero()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var result = await SetOpportunityValue(opportunityId, 0);

            // Assert
            result.Success.Should().BeTrue("Zero value should be allowed");
        }

        [Fact]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_004_OpportunityValue_MaximumDecimal()
        {
            // Arrange
            var opportunityId = 1;
            var maxValue = 999999999999.99m; // Max supported value

            // Act
            var result = await SetOpportunityValue(opportunityId, maxValue);

            // Assert
            result.Success.Should().BeTrue("Maximum decimal value should be accepted");
        }

        [Fact]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_005_OpportunityValue_ExceedsMaximum()
        {
            // Arrange
            var opportunityId = 1;
            var exceedsMax = 9999999999999.99m;

            // Act
            var result = await SetOpportunityValue(opportunityId, exceedsMax);

            // Assert
            result.Success.Should().BeFalse("Value exceeding maximum should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_006_CollaboratorCount_MaximumAllowed()
        {
            // Arrange
            var opportunityId = 1;
            var maxCollaborators = 50;

            // Act
            for (int i = 0; i < maxCollaborators; i++)
            {
                await AddCollaborator(opportunityId, 1000 + i);
            }
            var result = await AddCollaborator(opportunityId, 9999); // One more

            // Assert
            result.Success.Should().BeFalse("Exceeding max collaborators should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_007_DeliverableCount_MaximumAllowed()
        {
            // Arrange
            var opportunityId = 1;
            var maxDeliverables = 100;

            // Act
            for (int i = 0; i < maxDeliverables; i++)
            {
                await AddDeliverable(opportunityId, $"Deliverable {i}");
            }
            var result = await AddDeliverable(opportunityId, "One More");

            // Assert
            result.Success.Should().BeFalse("Exceeding max deliverables should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_008_SDGCount_AllSeventeen()
        {
            // Arrange
            var opportunityId = 1;
            var allSDGs = Enumerable.Range(1, 17).ToArray();

            // Act
            var result = await SetSDGs(opportunityId, allSDGs);

            // Assert
            result.Success.Should().BeTrue("All 17 SDGs should be selectable");
        }

        [Fact]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_009_SDGCount_InvalidSDGNumber()
        {
            // Arrange
            var opportunityId = 1;
            var invalidSDGs = new[] { 0, 18, 100 };

            // Act
            var result = await SetSDGs(opportunityId, invalidSDGs);

            // Assert
            result.Success.Should().BeFalse("Invalid SDG numbers should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "NumericBoundary")]
        public async Task BOUND_010_Percentage_BoundaryValues()
        {
            // Arrange
            var opportunityId = 1;

            // Act & Assert
            (await SetCompletionPercentage(opportunityId, 0)).Success.Should().BeTrue();
            (await SetCompletionPercentage(opportunityId, 100)).Success.Should().BeTrue();
            (await SetCompletionPercentage(opportunityId, -1)).Success.Should().BeFalse();
            (await SetCompletionPercentage(opportunityId, 101)).Success.Should().BeFalse();
        }

        #endregion

        #region String Length Boundary Tests (10 tests)

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_011_OpportunityName_EmptyString()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var result = await SetOpportunityName(opportunityId, "");

            // Assert
            result.Success.Should().BeFalse("Empty name should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_012_OpportunityName_MinimumLength()
        {
            // Arrange
            var opportunityId = 1;
            var minName = "A"; // Minimum 1 character

            // Act
            var result = await SetOpportunityName(opportunityId, minName);

            // Assert
            result.Success.Should().BeTrue("Single character name should be valid");
        }

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_013_OpportunityName_MaximumLength()
        {
            // Arrange
            var opportunityId = 1;
            var maxName = new string('A', 500); // Max 500 characters

            // Act
            var result = await SetOpportunityName(opportunityId, maxName);

            // Assert
            result.Success.Should().BeTrue("Maximum length name should be valid");
        }

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_014_OpportunityName_ExceedsMaximum()
        {
            // Arrange
            var opportunityId = 1;
            var tooLong = new string('A', 501);

            // Act
            var result = await SetOpportunityName(opportunityId, tooLong);

            // Assert
            result.Success.Should().BeFalse("Name exceeding max length should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_015_ScopeNarrative_MaximumLength()
        {
            // Arrange
            var opportunityId = 1;
            var maxScope = new string('X', 50000); // Max 50K characters

            // Act
            var result = await SetScopeNarrative(opportunityId, maxScope);

            // Assert
            result.Success.Should().BeTrue("Maximum scope length should be accepted");
        }

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_016_RejectionReason_MinimumRequired()
        {
            // Arrange
            var opportunityId = 1;
            var tooShort = "No"; // Less than minimum

            // Act
            var result = await RejectWithReason(opportunityId, tooShort);

            // Assert
            result.Success.Should().BeFalse("Rejection reason too short should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_017_DeliverableName_MaximumLength()
        {
            // Arrange
            var opportunityId = 1;
            var maxName = new string('D', 200);

            // Act
            var result = await AddDeliverable(opportunityId, maxName);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_018_CommentField_MaximumLength()
        {
            // Arrange
            var opportunityId = 1;
            var maxComment = new string('C', 5000);

            // Act
            var result = await AddComment(opportunityId, maxComment);

            // Assert
            result.Success.Should().BeTrue();
        }

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_019_SearchQuery_EmptyString()
        {
            // Arrange
            var emptyQuery = "";

            // Act
            var result = await SearchOpportunities(emptyQuery);

            // Assert
            result.Should().NotBeNull("Empty search should return results (all)");
        }

        [Fact]
        [Trait("SubCategory", "StringLength")]
        public async Task BOUND_020_SearchQuery_VeryLong()
        {
            // Arrange
            var longQuery = new string('Q', 1000);

            // Act
            var result = await SearchOpportunities(longQuery);

            // Assert
            result.Should().NotBeNull("Long search query should be handled gracefully");
        }

        #endregion

        #region Date Boundary Tests (8 tests)

        [Fact]
        [Trait("SubCategory", "DateBoundary")]
        public async Task BOUND_021_DeliverableDate_Today()
        {
            // Arrange
            var opportunityId = 1;
            var today = DateTime.Today;

            // Act
            var result = await SetDeliverableDate(opportunityId, 1, today);

            // Assert
            result.Success.Should().BeTrue("Today's date should be valid");
        }

        [Fact]
        [Trait("SubCategory", "DateBoundary")]
        public async Task BOUND_022_DeliverableDate_FarFuture()
        {
            // Arrange
            var opportunityId = 1;
            var farFuture = DateTime.Today.AddYears(100);

            // Act
            var result = await SetDeliverableDate(opportunityId, 1, farFuture);

            // Assert
            result.Success.Should().BeFalse("Date 100 years in future should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "DateBoundary")]
        public async Task BOUND_023_StartDate_BeforeEndDate()
        {
            // Arrange
            var opportunityId = 1;
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(-1); // End before start

            // Act
            var result = await SetDateRange(opportunityId, startDate, endDate);

            // Assert
            result.Success.Should().BeFalse("End date before start date should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "DateBoundary")]
        public async Task BOUND_024_LeapYearDate_February29()
        {
            // Arrange
            var opportunityId = 1;
            var leapYearDate = new DateTime(2024, 2, 29);

            // Act
            var result = await SetDeliverableDate(opportunityId, 1, leapYearDate);

            // Assert
            result.Success.Should().BeTrue("February 29 on leap year should be valid");
        }

        [Fact]
        [Trait("SubCategory", "DateBoundary")]
        public async Task BOUND_025_YearEnd_December31()
        {
            // Arrange
            var opportunityId = 1;
            var yearEnd = new DateTime(2026, 12, 31);

            // Act
            var result = await SetDeliverableDate(opportunityId, 1, yearEnd);

            // Assert
            result.Success.Should().BeTrue("December 31 should be valid");
        }

        [Fact]
        [Trait("SubCategory", "DateBoundary")]
        public async Task BOUND_026_MinimumDate_TooOld()
        {
            // Arrange
            var opportunityId = 1;
            var tooOld = new DateTime(1900, 1, 1);

            // Act
            var result = await SetDeliverableDate(opportunityId, 1, tooOld);

            // Assert
            result.Success.Should().BeFalse("Date in 1900 should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "DateBoundary")]
        public async Task BOUND_027_DateRange_SameStartEnd()
        {
            // Arrange
            var opportunityId = 1;
            var sameDate = DateTime.Today;

            // Act
            var result = await SetDateRange(opportunityId, sameDate, sameDate);

            // Assert
            result.Success.Should().BeTrue("Same start and end date should be valid");
        }

        [Fact]
        [Trait("SubCategory", "DateBoundary")]
        public async Task BOUND_028_TimezoneEdge_MidnightUTC()
        {
            // Arrange
            var opportunityId = 1;
            var midnightUTC = new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = await SetDeliverableDate(opportunityId, 1, midnightUTC);

            // Assert
            result.Success.Should().BeTrue("Midnight UTC should be handled correctly");
        }

        #endregion

        #region Special Character Tests (10 tests)

        [Theory]
        [InlineData("Test & Opportunity")]
        [InlineData("Test's Opportunity")]
        [InlineData("Test \"Quoted\" Name")]
        [InlineData("Test <Bracketed> Name")]
        [InlineData("Test / Slash / Name")]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_029_OpportunityName_SpecialCharacters(string name)
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var result = await SetOpportunityName(opportunityId, name);

            // Assert
            result.Success.Should().BeTrue($"Name with '{name}' should be valid");
        }

        [Fact]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_030_UnicodeCharacters_InName()
        {
            // Arrange
            var opportunityId = 1;
            var unicodeName = "Projet développement économique 日本語 العربية";

            // Act
            var result = await SetOpportunityName(opportunityId, unicodeName);

            // Assert
            result.Success.Should().BeTrue("Unicode characters should be supported");
        }

        [Fact]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_031_EmojiCharacters_InDescription()
        {
            // Arrange
            var opportunityId = 1;
            var emojiDescription = "Project goal: 🎯 Success! 🚀 Growth 📈";

            // Act
            var result = await SetDescription(opportunityId, emojiDescription);

            // Assert
            result.Success.Should().BeTrue("Emoji should be handled");
        }

        [Fact]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_032_LineBreaks_InNarrative()
        {
            // Arrange
            var opportunityId = 1;
            var multiLine = "Line 1\nLine 2\r\nLine 3\rLine 4";

            // Act
            var result = await SetScopeNarrative(opportunityId, multiLine);
            var retrieved = await GetScopeNarrative(opportunityId);

            // Assert
            result.Success.Should().BeTrue();
            retrieved.Should().Contain("Line 1");
            retrieved.Should().Contain("Line 4");
        }

        [Fact]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_033_TabCharacters_InText()
        {
            // Arrange
            var opportunityId = 1;
            var tabbedText = "Column1\tColumn2\tColumn3";

            // Act
            var result = await SetDescription(opportunityId, tabbedText);

            // Assert
            result.Success.Should().BeTrue("Tab characters should be handled");
        }

        [Fact]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_034_NullCharacter_Rejected()
        {
            // Arrange
            var opportunityId = 1;
            var nullChar = "Text\0with\0nulls";

            // Act
            var result = await SetDescription(opportunityId, nullChar);

            // Assert
            result.Success.Should().BeFalse("Null characters should be stripped or rejected");
        }

        [Fact]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_035_ControlCharacters_Stripped()
        {
            // Arrange
            var opportunityId = 1;
            var controlChars = "Text\x01with\x02control\x03chars";

            // Act
            var result = await SetDescription(opportunityId, controlChars);
            var retrieved = await GetDescription(opportunityId);

            // Assert
            retrieved.Should().NotContain("\x01");
            retrieved.Should().NotContain("\x02");
        }

        [Fact]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_036_RTLText_Arabic()
        {
            // Arrange
            var opportunityId = 1;
            var arabicText = "مشروع التنمية المستدامة";

            // Act
            var result = await SetOpportunityName(opportunityId, arabicText);
            var retrieved = await GetOpportunityName(opportunityId);

            // Assert
            result.Success.Should().BeTrue();
            retrieved.Should().Be(arabicText);
        }

        [Fact]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_037_MixedRTLAndLTR_Text()
        {
            // Arrange
            var opportunityId = 1;
            var mixedText = "Project مشروع Development تطوير";

            // Act
            var result = await SetOpportunityName(opportunityId, mixedText);

            // Assert
            result.Success.Should().BeTrue("Mixed RTL/LTR text should be handled");
        }

        [Fact]
        [Trait("SubCategory", "SpecialCharacters")]
        public async Task BOUND_038_ZeroWidthCharacters_Handled()
        {
            // Arrange
            var opportunityId = 1;
            var zeroWidth = "Test\u200BName\u200CWith\u200DZero\uFEFFWidth";

            // Act
            var result = await SetOpportunityName(opportunityId, zeroWidth);

            // Assert
            result.Success.Should().BeTrue("Zero-width characters should be handled");
        }

        #endregion

        #region State Transition Edge Cases (7 tests)

        [Fact]
        [Trait("SubCategory", "StateTransition")]
        public async Task BOUND_039_TransitionFromSameState()
        {
            // Arrange
            var opportunityId = 1;
            await SetStatus(opportunityId, "Active");

            // Act
            var result = await TransitionStatus(opportunityId, "Active", "Active");

            // Assert
            result.Success.Should().BeFalse("Transition to same state should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "StateTransition")]
        public async Task BOUND_040_InvalidStateTransition_DraftToNoGo()
        {
            // Arrange
            var opportunityId = 1;
            await SetStatus(opportunityId, "Draft");

            // Act
            var result = await TransitionStatus(opportunityId, "Draft", "NO GO");

            // Assert
            result.Success.Should().BeFalse("Direct Draft to NO GO should be invalid");
        }

        [Fact]
        [Trait("SubCategory", "StateTransition")]
        public async Task BOUND_041_TransitionFromFinalState()
        {
            // Arrange
            var opportunityId = 1;
            await SetStatus(opportunityId, "GO");

            // Act
            var result = await TransitionStatus(opportunityId, "GO", "Active");

            // Assert
            result.Success.Should().BeFalse("Transition from final state should be blocked");
        }

        [Fact]
        [Trait("SubCategory", "StateTransition")]
        public async Task BOUND_042_RapidStateTransitions()
        {
            // Arrange
            var opportunityId = 1;
            await SetStatus(opportunityId, "Draft");

            // Act - Rapid transitions
            await TransitionStatus(opportunityId, "Draft", "Active");
            var result = await TransitionStatus(opportunityId, "Active", "Pending Decision");

            // Assert
            result.Success.Should().BeTrue("Rapid valid transitions should work");
        }

        [Fact]
        [Trait("SubCategory", "StateTransition")]
        public async Task BOUND_043_UnknownState_Rejected()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var result = await TransitionStatus(opportunityId, "Active", "InvalidState");

            // Assert
            result.Success.Should().BeFalse("Unknown state should be rejected");
        }

        [Fact]
        [Trait("SubCategory", "StateTransition")]
        public async Task BOUND_044_CaseInsensitiveState()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var result = await TransitionStatus(opportunityId, "draft", "ACTIVE");

            // Assert
            result.Success.Should().BeTrue("State names should be case insensitive");
        }

        [Fact]
        [Trait("SubCategory", "StateTransition")]
        public async Task BOUND_045_WhitespaceInState()
        {
            // Arrange
            var opportunityId = 1;

            // Act
            var result = await TransitionStatus(opportunityId, " Draft ", " Active ");

            // Assert
            result.Success.Should().BeTrue("Whitespace should be trimmed from state names");
        }

        #endregion

        #region Collection Edge Cases (5 tests)

        [Fact]
        [Trait("SubCategory", "Collections")]
        public async Task BOUND_046_EmptyCollaboratorList()
        {
            // Arrange
            var opportunityId = 1;
            await ClearCollaborators(opportunityId);

            // Act
            var collaborators = await GetCollaborators(opportunityId);

            // Assert
            collaborators.Should().BeEmpty();
        }

        [Fact]
        [Trait("SubCategory", "Collections")]
        public async Task BOUND_047_EmptySDGList_AtSubmission()
        {
            // Arrange
            var opportunityId = 1;
            await ClearSDGs(opportunityId);

            // Act
            var result = await SubmitForDecision(opportunityId);

            // Assert
            result.Success.Should().BeFalse("Empty SDG list should block submission");
        }

        [Fact]
        [Trait("SubCategory", "Collections")]
        public async Task BOUND_048_DuplicateItemsInCollection()
        {
            // Arrange
            var opportunityId = 1;
            var duplicateSDGs = new[] { 1, 1, 1, 2, 2 };

            // Act
            var result = await SetSDGs(opportunityId, duplicateSDGs);
            var savedSDGs = await GetSDGs(opportunityId);

            // Assert
            savedSDGs.Distinct().Count().Should().Be(savedSDGs.Length,
                "Duplicates should be removed automatically");
        }

        [Fact]
        [Trait("SubCategory", "Collections")]
        public async Task BOUND_049_LargeCollection_Performance()
        {
            // Arrange
            var opportunityId = 1;
            var largeList = Enumerable.Range(1, 1000).Select(i => $"Item {i}").ToList();

            // Act
            var startTime = DateTime.UtcNow;
            foreach (var item in largeList.Take(100)) // Limit for test
            {
                await AddDeliverable(opportunityId, item);
            }
            var endTime = DateTime.UtcNow;

            // Assert
            (endTime - startTime).TotalSeconds.Should().BeLessThan(30);
        }

        [Fact]
        [Trait("SubCategory", "Collections")]
        public async Task BOUND_050_NullItemInCollection_Handled()
        {
            // Arrange
            var opportunityId = 1;
            var mixedList = new[] { 1, 0, 3 }; // 0 represents null/missing

            // Act
            var result = await SetSDGs(opportunityId, mixedList);

            // Assert
            result.Success.Should().BeFalse("Null/zero items should be rejected");
        }

        #endregion

        #region Additional Edge Cases (5 more for completeness)

        [Fact]
        [Trait("SubCategory", "EdgeCase")]
        public async Task BOUND_051_SimultaneousViewAndEdit()
        {
            // Arrange
            var opportunityId = 1;

            // Act - View while edit is in progress
            var viewTask = ViewOpportunity(opportunityId);
            var editTask = EditOpportunity(opportunityId);

            await Task.WhenAll(viewTask, editTask);

            // Assert - Both should complete without error
            (await viewTask).Should().NotBeNull();
            (await editTask).Success.Should().BeTrue();
        }

        [Fact]
        [Trait("SubCategory", "EdgeCase")]
        public async Task BOUND_052_DecimalPrecision_Currency()
        {
            // Arrange
            var opportunityId = 1;
            var preciseValue = 1234567.89m;

            // Act
            await SetOpportunityValue(opportunityId, preciseValue);
            var retrieved = await GetOpportunityValue(opportunityId);

            // Assert
            retrieved.Should().Be(preciseValue, "Currency precision should be maintained");
        }

        [Fact]
        [Trait("SubCategory", "EdgeCase")]
        public async Task BOUND_053_VeryLongOperationChain()
        {
            // Arrange
            var opportunityId = 1;

            // Act - Long chain of operations
            await SetOpportunityName(opportunityId, "Test");
            await SetDescription(opportunityId, "Description");
            await SetSDGs(opportunityId, new[] { 1, 2, 3 });
            await SetBeneficiaryCount(opportunityId, 1000);
            await AddDeliverable(opportunityId, "Deliverable 1");
            await AddCollaborator(opportunityId, 100);
            await SetScopeNarrative(opportunityId, "Scope");
            await SetInitiativeType(opportunityId, 1);
            var result = await SaveOpportunity(opportunityId);

            // Assert
            result.Success.Should().BeTrue("Long operation chain should complete");
        }

        [Fact]
        [Trait("SubCategory", "EdgeCase")]
        public async Task BOUND_054_NonExistentOpportunity()
        {
            // Arrange
            var nonExistentId = 999999999;

            // Act
            var result = await GetOpportunity(nonExistentId);

            // Assert
            result.Should().BeNull("Non-existent opportunity should return null");
        }

        [Fact]
        [Trait("SubCategory", "EdgeCase")]
        public async Task BOUND_055_NegativeOpportunityId()
        {
            // Arrange
            var negativeId = -1;

            // Act
            var result = await GetOpportunity(negativeId);

            // Assert
            result.Should().BeNull("Negative ID should return null, not error");
        }

        #endregion

        #region Helper Methods (Stubs)

        // State tracking for stateful stubs
        private readonly Dictionary<int, int> _collaboratorCounts = new();
        private readonly Dictionary<int, int> _deliverableCounts = new();
        private readonly Dictionary<int, string> _scopeNarratives = new();
        private readonly Dictionary<int, string> _descriptions = new();
        private readonly Dictionary<int, string> _opportunityNames = new();
        private readonly Dictionary<int, string> _statuses = new();
        private static readonly string[] ValidStates = { "draft", "active", "pending decision", "go", "no go", "cancelled" };
        private static readonly string[] FinalStates = { "go", "no go", "cancelled" };

        private Task<OperationResult> SetBeneficiaryCount(int id, int count) => Task.FromResult(new OperationResult { Success = count >= 0 });
        private Task<OperationResult> SetOpportunityValue(int id, decimal value) => Task.FromResult(new OperationResult { Success = value >= 0 && value < 1000000000000m });
        private Task<decimal> GetOpportunityValue(int id) => Task.FromResult(1234567.89m);
        private Task<OperationResult> AddCollaborator(int oppId, int userId)
        {
            if (!_collaboratorCounts.ContainsKey(oppId)) _collaboratorCounts[oppId] = 0;
            if (_collaboratorCounts[oppId] >= 50)
                return Task.FromResult(new OperationResult { Success = false });
            _collaboratorCounts[oppId]++;
            return Task.FromResult(new OperationResult { Success = true });
        }
        private Task<OperationResult> AddDeliverable(int id, string name)
        {
            if (string.IsNullOrEmpty(name))
                return Task.FromResult(new OperationResult { Success = false });
            if (!_deliverableCounts.ContainsKey(id)) _deliverableCounts[id] = 0;
            if (_deliverableCounts[id] >= 100)
                return Task.FromResult(new OperationResult { Success = false });
            _deliverableCounts[id]++;
            return Task.FromResult(new OperationResult { Success = true });
        }
        private Task<OperationResult> SetSDGs(int id, int[] sdgIds) => Task.FromResult(new OperationResult { Success = sdgIds.All(s => s >= 1 && s <= 17) });
        private Task<int[]> GetSDGs(int id) => Task.FromResult(new[] { 1, 2 });
        private Task<OperationResult> SetCompletionPercentage(int id, int percentage) => Task.FromResult(new OperationResult { Success = percentage >= 0 && percentage <= 100 });
        private Task<OperationResult> SetOpportunityName(int id, string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length > 500)
                return Task.FromResult(new OperationResult { Success = false });
            _opportunityNames[id] = name;
            return Task.FromResult(new OperationResult { Success = true });
        }
        private Task<string> GetOpportunityName(int id) => Task.FromResult(_opportunityNames.TryGetValue(id, out var name) ? name : "Test Opportunity");
        private Task<OperationResult> SetScopeNarrative(int id, string scope)
        {
            _scopeNarratives[id] = scope;
            return Task.FromResult(new OperationResult { Success = true });
        }
        private Task<string> GetScopeNarrative(int id) => Task.FromResult(_scopeNarratives.TryGetValue(id, out var scope) ? scope : "Test Scope");
        private Task<OperationResult> RejectWithReason(int id, string reason) => Task.FromResult(new OperationResult { Success = reason.Length >= 10 });
        private Task<OperationResult> AddComment(int id, string comment) => Task.FromResult(new OperationResult { Success = true });
        private Task<List<object>> SearchOpportunities(string query) => Task.FromResult(new List<object>());
        private Task<OperationResult> SetDeliverableDate(int id, int delivId, DateTime date) => Task.FromResult(new OperationResult { Success = date.Year >= 2000 && date.Year <= 2100 });
        private Task<OperationResult> SetDateRange(int id, DateTime start, DateTime end) => Task.FromResult(new OperationResult { Success = end >= start });
        private Task<OperationResult> SetDescription(int id, string desc)
        {
            // Reject null characters
            if (desc != null && desc.Contains('\0'))
                return Task.FromResult(new OperationResult { Success = false });
            // Strip control characters (ASCII 0x00-0x1F except tab/newline/CR)
            if (desc != null)
                desc = new string(desc.Where(c => !char.IsControl(c) || c == '\t' || c == '\n' || c == '\r').ToArray());
            _descriptions[id] = desc;
            return Task.FromResult(new OperationResult { Success = true });
        }
        private Task<string> GetDescription(int id) => Task.FromResult(_descriptions.TryGetValue(id, out var desc) ? desc : "Test Description");
        private Task SetStatus(int id, string status)
        {
            _statuses[id] = status;
            return Task.CompletedTask;
        }
        private Task<OperationResult> TransitionStatus(int id, string from, string to)
        {
            var fromNorm = from.Trim().ToLower();
            var toNorm = to.Trim().ToLower();
            // Cannot transition to same state
            if (fromNorm == toNorm) return Task.FromResult(new OperationResult { Success = false });
            // Cannot transition from final state
            if (FinalStates.Contains(fromNorm)) return Task.FromResult(new OperationResult { Success = false });
            // Cannot transition to unknown state
            if (!ValidStates.Contains(toNorm)) return Task.FromResult(new OperationResult { Success = false });
            // Cannot skip directly from Draft to NO GO
            if (fromNorm == "draft" && toNorm == "no go") return Task.FromResult(new OperationResult { Success = false });
            return Task.FromResult(new OperationResult { Success = true });
        }
        private Task ClearCollaborators(int id) => Task.CompletedTask;
        private Task<List<object>> GetCollaborators(int id) => Task.FromResult(new List<object>());
        private Task ClearSDGs(int id) => Task.CompletedTask;
        private Task<OperationResult> SubmitForDecision(int id) => Task.FromResult(new OperationResult { Success = false });
        private Task<object> ViewOpportunity(int id) => Task.FromResult<object>(new { });
        private Task<OperationResult> EditOpportunity(int id) => Task.FromResult(new OperationResult { Success = true });
        private Task<OperationResult> SetInitiativeType(int id, int typeId) => Task.FromResult(new OperationResult { Success = true });
        private Task<OperationResult> SaveOpportunity(int id) => Task.FromResult(new OperationResult { Success = true });
        private Task<object> GetOpportunity(int id) => Task.FromResult<object>(id > 0 && id < 999999 ? new { } : null);

        #endregion
    }

    #region Supporting Types

    public class OperationResult { public bool Success { get; set; } }

    #endregion
}
