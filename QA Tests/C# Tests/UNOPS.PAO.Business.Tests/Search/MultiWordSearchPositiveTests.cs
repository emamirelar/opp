/**
 * @fileoverview Positive tests for PNO-1211: Generic Search — Multi-word user search.
 * @author QA Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Search;

/// <summary>
/// Tests for PNO-1211: Generic Search Issue - User appears in search results with specific steps only
///
/// Requirements validated:
/// - REQ-1: First name alone search → Tests: SearchByFirstName_ReturnsMatchingUsers
/// - REQ-2: Last name alone search → Tests: SearchByLastName_ReturnsMatchingUsers
/// - REQ-3: Full name search (FirstName AND LastName) → Tests: SearchByFullName_ReturnsMatchingUsers
/// - REQ-4: Partial first name → Tests: SearchByPartialFirstName_ReturnsMatchingUsers
/// - REQ-5: Partial last name → Tests: SearchByPartialLastName_ReturnsMatchingUsers
/// - REQ-6: Email search → Tests: SearchByEmail_ReturnsMatchingUsers
/// - REQ-7: Multi-word AND logic → Tests: SearchByFullName_ReturnsMatchingUsers, MultiWordSearch_AllTermsMustMatch
/// - REQ-8: Case-insensitive → Tests: Search_CaseInsensitive_MatchesAllVariations
/// - REQ-9: Multiple spaces handled → Tests: Search_MultipleSpacesBetweenWords_HandledCorrectly
/// - REQ-10: Empty/whitespace no filter → Tests: Search_EmptyOrWhitespace_AddsNoFilter
/// </summary>
public class PNO1211PositiveTests
{
    #region REQ-1: First name alone

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-1")]
    public void SearchByFirstName_John_MatchesUserWithFirstNameJohn()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-1")]
    public void SearchByFirstName_Jane_MatchesUserWithFirstNameJane()
    {
        var spec = new MultiWordSearchSpec("Jane");
        spec.Matches("Jane", "Smith", "jane@unops.org").Should().BeTrue();
    }

    #endregion

    #region REQ-2: Last name alone

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-2")]
    public void SearchByLastName_Doe_MatchesUserWithLastNameDoe()
    {
        var spec = new MultiWordSearchSpec("Doe");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-2")]
    public void SearchByLastName_Smith_MatchesUserWithLastNameSmith()
    {
        var spec = new MultiWordSearchSpec("Smith");
        spec.Matches("Jane", "Smith", "jane@unops.org").Should().BeTrue();
    }

    #endregion

    #region REQ-3: Full name

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-3")]
    public void SearchByFullName_JohnDoe_MatchesUserJohnDoe()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-3")]
    public void SearchByFullName_JaneSmith_MatchesUserJaneSmith()
    {
        var spec = new MultiWordSearchSpec("Jane Smith");
        spec.Matches("Jane", "Smith", "jane.smith@unops.org").Should().BeTrue();
    }

    #endregion

    #region REQ-4: Partial first name

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-4")]
    public void SearchByPartialFirstName_Joh_MatchesJohn()
    {
        var spec = new MultiWordSearchSpec("Joh");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-4")]
    public void SearchByPartialFirstName_Jan_MatchesJane()
    {
        var spec = new MultiWordSearchSpec("Jan");
        spec.Matches("Jane", "Smith", "jane@unops.org").Should().BeTrue();
    }

    #endregion

    #region REQ-5: Partial last name

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-5")]
    public void SearchByPartialLastName_Do_MatchesDoe()
    {
        var spec = new MultiWordSearchSpec("Do");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-5")]
    public void SearchByPartialLastName_Smi_MatchesSmith()
    {
        var spec = new MultiWordSearchSpec("Smi");
        spec.Matches("Jane", "Smith", "jane@unops.org").Should().BeTrue();
    }

    #endregion

    #region REQ-6: Email search

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-6")]
    public void SearchByEmail_PartialEmail_MatchesUser()
    {
        var spec = new MultiWordSearchSpec("john.doe");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-6")]
    public void SearchByEmail_Domain_MatchesUser()
    {
        var spec = new MultiWordSearchSpec("unops.org");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    #endregion

    #region REQ-7: Multi-word AND logic

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-7")]
    public void MultiWordSearch_JohnDoe_BothTermsMustMatch()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
        spec.Matches("John", "X", "x@y.com").Should().BeFalse("Doe must match");
        spec.Matches("X", "Doe", "x@y.com").Should().BeFalse("John must match");
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-7")]
    public void MultiWordSearch_ThreeWords_AllMustMatch()
    {
        var spec = new MultiWordSearchSpec("John M Doe");
        spec.Matches("John", "Doe", "john.m.doe@unops.org").Should().BeTrue("M can match in email");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeFalse("M must match somewhere");
    }

    #endregion

    #region REQ-8: Case-insensitive

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-8")]
    public void Search_JOHN_MatchesJohn()
    {
        var spec = new MultiWordSearchSpec("JOHN");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-8")]
    public void Search_john_MatchesJohn()
    {
        var spec = new MultiWordSearchSpec("john");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-8")]
    public void Search_JohnDoe_MixedCase_Matches()
    {
        var spec = new MultiWordSearchSpec("JOHN doe");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    #endregion

    #region REQ-9: Multiple spaces

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-9")]
    public void Search_JohnDoubleSpaceDoe_HandledAsJohnDoe()
    {
        var spec = new MultiWordSearchSpec("John  Doe");
        spec.SplitTerms.Should().Equal("john", "doe");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-9")]
    public void Search_MultipleSpacesBetweenWords_RemoveEmptyEntries()
    {
        var spec = new MultiWordSearchSpec("John   M   Doe");
        spec.SplitTerms.Should().Equal("john", "m", "doe");
    }

    #endregion

    #region REQ-10: Empty/whitespace

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-10")]
    public void Search_EmptyString_AddsNoFilter()
    {
        var spec = new MultiWordSearchSpec("");
        spec.HasSearchFilter.Should().BeFalse();
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-10")]
    public void Search_WhitespaceOnly_AddsNoFilter()
    {
        var spec = new MultiWordSearchSpec("   ");
        spec.HasSearchFilter.Should().BeFalse();
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    [Trait("Requirement", "REQ-10")]
    public void Search_Null_AddsNoFilter()
    {
        var spec = new MultiWordSearchSpec(null);
        spec.HasSearchFilter.Should().BeFalse();
    }

    #endregion

    #region Additional positive scenarios

    [Fact]
    [Trait("Category", "Positive")]
    public void Search_SingleCharacter_Matches()
    {
        var spec = new MultiWordSearchSpec("J");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Search_TermInEmail_Matches()
    {
        var spec = new MultiWordSearchSpec("perminder");
        spec.Matches("Perminder", "Saluja", "perminder.saluja@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Search_SplitTerms_ProduceCorrectParameters()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Parameters.Should().Equal("%john%", "%doe%");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Search_WhereClauseFragment_ContainsAndLogic()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        var fragment = spec.BuildWhereClauseFragment(0);
        fragment.Should().Contain("AND");
        fragment.Should().Contain("@p0");
        fragment.Should().Contain("@p1");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Search_WhereClauseFragment_EachTermHasOrLogic()
    {
        var spec = new MultiWordSearchSpec("John");
        var fragment = spec.BuildWhereClauseFragment(0);
        fragment.Should().Contain("FirstName");
        fragment.Should().Contain("LastName");
        fragment.Should().Contain("UserEmail");
        fragment.Should().Contain("OR");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Search_LeadingTrailingSpaces_TrimmedBySplit()
    {
        var spec = new MultiWordSearchSpec("  John  ");
        spec.SplitTerms.Should().Equal("john");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void Search_WordMatchesInDifferentFields_StillMatches()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", "Doe", "other@unops.org").Should().BeTrue();
        spec.Matches("John", "Doe", "j.doe@unops.org").Should().BeTrue();
    }

    #endregion
}
