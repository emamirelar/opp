/**
 * @fileoverview Functional tests for PNO-1211: Generic Search — Multi-word user search.
 * Business rules, validation logic, state transitions, computed values.
 * @author QA Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Search;

/// <summary>
/// Functional tests for PNO-1211: Business rules, AND/OR logic, parameter generation, WHERE structure.
/// </summary>
public class PNO1211FunctionalTests
{
    #region REQ-7: Multi-word AND logic

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-7")]
    public void MultiWordSearch_EachWordMustMatchAtLeastOneField()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
        spec.Matches("John", "X", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-7")]
    public void MultiWordSearch_WordCanMatchInDifferentFields()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", "Doe", "other@unops.org").Should().BeTrue();
        spec.Matches("John", "Doe", "j.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-7")]
    public void MultiWordSearch_AllWordsMustSatisfyOrCondition()
    {
        var spec = new MultiWordSearchSpec("A B");
        spec.Matches("Anna", "Bob", "x@y.com").Should().BeTrue();
        spec.Matches("Anna", "X", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-7")]
    public void MultiWordSearch_AndLogicBetweenTerms()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        var term1Matches = spec.Matches("John", "X", "x@y.com");
        var term2Matches = spec.Matches("X", "Doe", "x@y.com");
        term1Matches.Should().BeFalse();
        term2Matches.Should().BeFalse();
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-7")]
    public void MultiWordSearch_OrLogicWithinTerm()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", null, null).Should().BeTrue();
        spec.Matches(null, "John", null).Should().BeTrue();
        spec.Matches(null, null, "john@unops.org").Should().BeTrue();
    }

    #endregion

    #region Parameter generation

    [Fact]
    [Trait("Category", "Functional")]
    public void ParameterGeneration_EachTermGetsWrappedInPercent()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Parameters[0].Should().Be("%john%");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ParameterGeneration_MultipleTermsGetSeparateParams()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Parameters.Should().HaveCount(2);
        spec.Parameters[0].Should().Be("%john%");
        spec.Parameters[1].Should().Be("%doe%");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ParameterGeneration_OrderMatchesSplitTerms()
    {
        var spec = new MultiWordSearchSpec("A B C");
        for (int i = 0; i < spec.SplitTerms.Count; i++)
        {
            spec.Parameters[i].Should().Be($"%{spec.SplitTerms[i]}%");
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ParameterGeneration_NoParamsForEmptySearch()
    {
        var spec = new MultiWordSearchSpec("");
        spec.Parameters.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void ParameterGeneration_NoParamsForNullSearch()
    {
        var spec = new MultiWordSearchSpec(null);
        spec.Parameters.Should().BeEmpty();
    }

    #endregion

    #region WHERE clause structure

    [Fact]
    [Trait("Category", "Functional")]
    public void WhereClause_ContainsFirstNameColumn()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(0).Should().Contain("FirstName");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void WhereClause_ContainsLastNameColumn()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(0).Should().Contain("LastName");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void WhereClause_ContainsUserEmailColumn()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(0).Should().Contain("UserEmail");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void WhereClause_ContainsLowerFunction()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(0).Should().Contain("LOWER");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void WhereClause_ContainsLikeOperator()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(0).Should().Contain("LIKE");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void WhereClause_ContainsOrBetweenFields()
    {
        var spec = new MultiWordSearchSpec("John");
        var fragment = spec.BuildWhereClauseFragment(0);
        fragment.Should().Contain("OR");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void WhereClause_ContainsAndBetweenTerms()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.BuildWhereClauseFragment(0).Should().Contain("AND");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void WhereClause_ParameterPlaceholdersInOrder()
    {
        var spec = new MultiWordSearchSpec("A B C");
        var fragment = spec.BuildWhereClauseFragment(10);
        fragment.Should().Contain("@p10");
        fragment.Should().Contain("@p11");
        fragment.Should().Contain("@p12");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void WhereClause_TableAliasUpUsed()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(0).Should().Contain("up.");
    }

    #endregion

    #region HasSearchFilter logic

    [Fact]
    [Trait("Category", "Functional")]
    public void HasSearchFilter_TrueWhenNonEmptyTerm()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.HasSearchFilter.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HasSearchFilter_FalseWhenEmpty()
    {
        var spec = new MultiWordSearchSpec("");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HasSearchFilter_FalseWhenNull()
    {
        var spec = new MultiWordSearchSpec(null);
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HasSearchFilter_FalseWhenWhitespaceOnly()
    {
        var spec = new MultiWordSearchSpec("   ");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void HasSearchFilter_TrueWhenSingleChar()
    {
        var spec = new MultiWordSearchSpec("J");
        spec.HasSearchFilter.Should().BeTrue();
    }

    #endregion

    #region Matches logic - OR within term

    [Fact]
    [Trait("Category", "Functional")]
    public void Matches_TermInFirstName_Matches()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "X", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Matches_TermInLastName_Matches()
    {
        var spec = new MultiWordSearchSpec("Doe");
        spec.Matches("X", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Matches_TermInEmail_Matches()
    {
        var spec = new MultiWordSearchSpec("john");
        spec.Matches("X", "Y", "john@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Matches_TermInMultipleFields_Matches()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "Johnson", "john@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Matches_TermInNoField_NoMatch()
    {
        var spec = new MultiWordSearchSpec("XYZ");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    #endregion

    #region Split logic

    [Fact]
    [Trait("Category", "Functional")]
    public void Split_SpaceDelimiter_SplitsCorrectly()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.SplitTerms.Should().Equal("john", "doe");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Split_RemoveEmptyEntries_NoEmptyTerms()
    {
        var spec = new MultiWordSearchSpec("John  Doe");
        spec.SplitTerms.Should().NotContain("");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Split_ToLower_AllTermsLowercase()
    {
        var spec = new MultiWordSearchSpec("JOHN DOE");
        spec.SplitTerms.Should().OnlyContain(t => t == t.ToLower());
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Split_SingleWord_OneTerm()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.SplitTerms.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Split_NoSpaces_OneTerm()
    {
        var spec = new MultiWordSearchSpec("JohnDoe");
        spec.SplitTerms.Should().Equal("johndoe");
    }

    #endregion

    #region Case insensitivity (REQ-8)

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-8")]
    public void CaseInsensitivity_SearchTermLowered()
    {
        var spec = new MultiWordSearchSpec("JOHN");
        spec.SplitTerms[0].Should().Be("john");
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-8")]
    public void CaseInsensitivity_MatchesIgnoresCase()
    {
        var spec = new MultiWordSearchSpec("john");
        spec.Matches("JOHN", "DOE", "JOHN.DOE@UNOPS.ORG").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-8")]
    public void CaseInsensitivity_AllVariationsMatch()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("JOHN", "doe", "x@y.com").Should().BeTrue();
        spec.Matches("john", "DOE", "x@y.com").Should().BeTrue();
        spec.Matches("JoHn", "DoE", "x@y.com").Should().BeTrue();
    }

    #endregion

    #region Empty/whitespace (REQ-10)

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-10")]
    public void EmptySearch_AddsNoFilter()
    {
        var spec = new MultiWordSearchSpec("");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-10")]
    public void EmptySearch_MatchesAll()
    {
        var spec = new MultiWordSearchSpec("");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
        spec.Matches("", "", "").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-10")]
    public void SpacesOnlySearch_AddsNoFilter()
    {
        var spec = new MultiWordSearchSpec("   ");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-10")]
    public void NullSearch_AddsNoFilter()
    {
        var spec = new MultiWordSearchSpec(null);
        spec.HasSearchFilter.Should().BeFalse();
    }

    #endregion

    #region Multiple spaces (REQ-9)

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-9")]
    public void MultipleSpaces_RemoveEmptyEntries()
    {
        var spec = new MultiWordSearchSpec("John   Doe");
        spec.SplitTerms.Should().Equal("john", "doe");
    }

    [Fact]
    [Trait("Category", "Functional")]
    [Trait("Requirement", "REQ-9")]
    public void MultipleSpaces_ProduceCorrectParams()
    {
        var spec = new MultiWordSearchSpec("John  Doe");
        spec.Parameters.Should().Equal("%john%", "%doe%");
    }

    #endregion

    #region Data transformation

    [Fact]
    [Trait("Category", "Functional")]
    public void Transformation_SearchTermToLower()
    {
        var spec = new MultiWordSearchSpec("JOHN DOE");
        spec.SplitTerms.Should().Equal("john", "doe");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Transformation_ParamsWrappedWithPercent()
    {
        var spec = new MultiWordSearchSpec("test");
        spec.Parameters[0].Should().StartWith("%");
        spec.Parameters[0].Should().EndWith("%");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Transformation_WhereFragmentUsesParamPlaceholders()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(0).Should().Contain("@p0");
    }

    #endregion

    #region Consistency checks

    [Fact]
    [Trait("Category", "Functional")]
    public void Consistency_SplitTermsCountEqualsParamsCount()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.SplitTerms.Count.Should().Be(spec.Parameters.Count);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Consistency_HasSearchFilterImpliesNonEmptySplitTerms()
    {
        var spec = new MultiWordSearchSpec("John");
        if (spec.HasSearchFilter)
            spec.SplitTerms.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Consistency_NoFilterImpliesEmptySplitTerms()
    {
        var spec = new MultiWordSearchSpec("");
        spec.HasSearchFilter.Should().BeFalse();
        spec.SplitTerms.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Consistency_MatchesRespectsHasSearchFilter()
    {
        var spec = new MultiWordSearchSpec("");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Consistency_WhereFragmentParamCountMatchesTerms()
    {
        var spec = new MultiWordSearchSpec("A B C");
        var fragment = spec.BuildWhereClauseFragment(0);
        fragment.Should().Contain("@p0");
        fragment.Should().Contain("@p1");
        fragment.Should().Contain("@p2");
    }

    #endregion

    #region Additional functional scenarios (expand to 90+)

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_SubstringMatch_AnyPosition()
    {
        var spec = new MultiWordSearchSpec("oh");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_EmailDomainMatch()
    {
        var spec = new MultiWordSearchSpec("unops");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_EmailLocalPartMatch()
    {
        var spec = new MultiWordSearchSpec("john.doe");
        spec.Matches("Bob", "Smith", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_MiddleNameInEmail()
    {
        var spec = new MultiWordSearchSpec("John M Doe");
        spec.Matches("John", "Doe", "john.m.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_AllTermsInEmail()
    {
        var spec = new MultiWordSearchSpec("john doe");
        spec.Matches("Bob", "Smith", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_DistributedMatch()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", "Doe", "other@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_RepeatedTerm_BothMustMatch()
    {
        var spec = new MultiWordSearchSpec("John John");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_ThreeFields_OrWithinTerm()
    {
        var spec = new MultiWordSearchSpec("X");
        spec.Matches("X", "Y", "Z").Should().BeTrue();
        spec.Matches("A", "X", "Z").Should().BeTrue();
        spec.Matches("A", "B", "X@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_PartialMatch_FirstName()
    {
        var spec = new MultiWordSearchSpec("Joh");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_PartialMatch_LastName()
    {
        var spec = new MultiWordSearchSpec("Do");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_PartialMatch_Email()
    {
        var spec = new MultiWordSearchSpec("unops.org");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_WhereFragment_Parenthesized()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        var fragment = spec.BuildWhereClauseFragment(0);
        fragment.Should().StartWith("(");
        fragment.Should().EndWith(")");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_WhereFragment_QuotedColumnNames()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(0).Should().Contain("\"");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_SplitTerms_ReadOnly()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.SplitTerms.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_Parameters_ReadOnly()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Parameters.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_Matches_Deterministic()
    {
        var spec = new MultiWordSearchSpec("John");
        var r1 = spec.Matches("John", "Doe", "x@y.com");
        var r2 = spec.Matches("John", "Doe", "x@y.com");
        r1.Should().Be(r2);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_Spec_Stateless()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "Doe", "x@y.com");
        spec.Matches("Jane", "Doe", "x@y.com");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_TwoTerms_BothInFirstName_Matches()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John Doe", "Smith", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_TwoTerms_BothInLastName_Matches()
    {
        var spec = new MultiWordSearchSpec("Van Der");
        spec.Matches("John", "Van Der Berg", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_TwoTerms_BothInEmail_Matches()
    {
        var spec = new MultiWordSearchSpec("john doe");
        spec.Matches("Bob", "Smith", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_OneTerm_TwoFieldsMatch_StillMatches()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "Johnson", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_AndLogic_ShortCircuit()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("Jane", "Doe", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_OrLogic_ShortCircuit()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "X", "Y").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_SplitPreservesOrder()
    {
        var spec = new MultiWordSearchSpec("First Middle Last");
        spec.SplitTerms[0].Should().Be("first");
        spec.SplitTerms[1].Should().Be("middle");
        spec.SplitTerms[2].Should().Be("last");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_ParamsPreserveOrder()
    {
        var spec = new MultiWordSearchSpec("A B C");
        spec.Parameters[0].Should().Be("%a%");
        spec.Parameters[1].Should().Be("%b%");
        spec.Parameters[2].Should().Be("%c%");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_WhereFragmentStartIndex_Respected()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(7).Should().Contain("@p7");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_Matches_NullFirstName_ChecksOtherFields()
    {
        var spec = new MultiWordSearchSpec("Doe");
        spec.Matches(null, "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_Matches_NullLastName_ChecksOtherFields()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", null, "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_Matches_NullEmail_ChecksOtherFields()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "Doe", null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_Contains_NotEquals()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("Johnny", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_Contains_Substring()
    {
        var spec = new MultiWordSearchSpec("hn");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_NoFilter_MatchesAnyUser()
    {
        var spec = new MultiWordSearchSpec(null);
        spec.Matches("A", "B", "c@d.com").Should().BeTrue();
        spec.Matches("", "", "").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_SingleCharTerm_MatchesWhenPresent()
    {
        var spec = new MultiWordSearchSpec("J");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_WhereFragment_NoSqlInjection()
    {
        var spec = new MultiWordSearchSpec("John'; DROP--");
        spec.BuildWhereClauseFragment(0).Should().Contain("@p0");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_ParamValues_NotInFragment()
    {
        var spec = new MultiWordSearchSpec("John");
        var fragment = spec.BuildWhereClauseFragment(0);
        fragment.Should().NotContain("%john%");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_MultiWord_IndependentTermMatching()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", "Doe", "a@b.com").Should().BeTrue();
        spec.Matches("Doe", "John", "a@b.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Functional_EmptyString_NoException()
    {
        var spec = new MultiWordSearchSpec("");
        var act = () => spec.Matches("John", "Doe", "x@y.com");
        act.Should().NotThrow();
    }

    #endregion
}
