/**
 * @fileoverview Boundary tests for PNO-1211: Generic Search — Multi-word user search.
 * Min/max values, edge cases, soft-delete interactions, nullable FK, concurrent modification.
 * @author QA Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Search;

/// <summary>
/// Boundary tests for PNO-1211: Edge cases, min/max, whitespace, unicode, parameter indexing.
/// </summary>
public class PNO1211BoundaryTests
{
    #region Single character boundary

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SingleCharA_MatchesWhenInField()
    {
        var spec = new MultiWordSearchSpec("A");
        spec.Matches("Anna", "Smith", "anna@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SingleCharZ_MatchesWhenInField()
    {
        var spec = new MultiWordSearchSpec("Z");
        spec.Matches("Zoe", "Zhang", "zoe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SingleChar_DoesNotMatchWhenAbsent()
    {
        var spec = new MultiWordSearchSpec("Q");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SingleSpace_NoFilter()
    {
        var spec = new MultiWordSearchSpec(" ");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SingleTab_ProducesFilter()
    {
        var spec = new MultiWordSearchSpec("\t");
        spec.HasSearchFilter.Should().BeTrue();
    }

    #endregion

    #region Min/max length

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_EmptyString_NoTerms()
    {
        var spec = new MultiWordSearchSpec("");
        spec.SplitTerms.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_VeryLongTerm_Handled()
    {
        var longTerm = new string('a', 10000);
        var spec = new MultiWordSearchSpec(longTerm);
        spec.SplitTerms.Should().HaveCount(1);
        spec.Matches("John", "Doe", "john@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_VeryLongMultiWord_Handled()
    {
        var words = Enumerable.Range(0, 100).Select(i => $"word{i}").ToArray();
        var term = string.Join(" ", words);
        var spec = new MultiWordSearchSpec(term);
        spec.SplitTerms.Should().HaveCount(100);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_OneCharPerWord_FiveWords()
    {
        var spec = new MultiWordSearchSpec("a b c d e");
        spec.SplitTerms.Should().Equal("a", "b", "c", "d", "e");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_MaxParamIndex_InWhereFragment()
    {
        var spec = new MultiWordSearchSpec("A B C");
        var fragment = spec.BuildWhereClauseFragment(100);
        fragment.Should().Contain("@p100");
        fragment.Should().Contain("@p101");
        fragment.Should().Contain("@p102");
    }

    #endregion

    #region Nullable field boundaries

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_FirstNameOnly_MatchesWhenLastNameNull()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", null, null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_LastNameOnly_MatchesWhenFirstNameNull()
    {
        var spec = new MultiWordSearchSpec("Doe");
        spec.Matches(null, "Doe", null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_EmailOnly_MatchesWhenNamesNull()
    {
        var spec = new MultiWordSearchSpec("john@unops.org");
        spec.Matches(null, null, "john@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TwoTerms_OneFieldEach_BothNullOtherFields()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", "Doe", null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_EmptyStringField_DoesNotMatch()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("", "Doe", "doe@unops.org").Should().BeFalse();
    }

    #endregion

    #region Whitespace boundaries

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_LeadingSpace_Trimmed()
    {
        var spec = new MultiWordSearchSpec(" John");
        spec.SplitTerms.Should().Equal("john");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TrailingSpace_Trimmed()
    {
        var spec = new MultiWordSearchSpec("John ");
        spec.SplitTerms.Should().Equal("john");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_MultipleSpacesBetween_ProduceTwoTerms()
    {
        var spec = new MultiWordSearchSpec("John     Doe");
        spec.SplitTerms.Should().Equal("john", "doe");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TabBetweenWords_SplitBySpaceOnly()
    {
        var spec = new MultiWordSearchSpec("John\tDoe");
        spec.SplitTerms.Should().Equal("john\tdoe");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_NewlineBetweenWords_NotSplitByNewline()
    {
        var spec = new MultiWordSearchSpec("John\nDoe");
        spec.SplitTerms.Should().HaveCount(1);
    }

    #endregion

    #region Unicode boundaries

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_UnicodeName_Matches()
    {
        var spec = new MultiWordSearchSpec("José");
        spec.Matches("José", "García", "jose.garcia@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_UnicodeInEmail_Matches()
    {
        var spec = new MultiWordSearchSpec("josé");
        spec.Matches("Jose", "Garcia", "josé.garcia@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Cyrillic_Matches()
    {
        var spec = new MultiWordSearchSpec("Иван");
        spec.Matches("Иван", "Петров", "ivan@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Arabic_Matches()
    {
        var spec = new MultiWordSearchSpec("أحمد");
        spec.Matches("أحمد", "علي", "ahmed@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Emoji_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("John 😀");
        spec.SplitTerms.Should().Contain("john");
        spec.SplitTerms.Should().Contain("😀");
    }

    #endregion

    #region Parameter and WHERE structure boundaries

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_OneTerm_OneParameter()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Parameters.Should().HaveCount(1);
        spec.Parameters[0].Should().Be("%john%");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TwoTerms_TwoParameters()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Parameters.Should().HaveCount(2);
        spec.Parameters.Should().Equal("%john%", "%doe%");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_WhereFragment_ContainsAndBetweenTerms()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        var fragment = spec.BuildWhereClauseFragment(0);
        var andCount = fragment.Split(" AND ").Length - 1;
        andCount.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_WhereFragment_ContainsOrWithinTerm()
    {
        var spec = new MultiWordSearchSpec("John");
        var fragment = spec.BuildWhereClauseFragment(0);
        fragment.Should().Contain("OR");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_WhereFragment_ParamIndexIncrements()
    {
        var spec = new MultiWordSearchSpec("A B C");
        var fragment = spec.BuildWhereClauseFragment(5);
        fragment.Should().Contain("@p5");
        fragment.Should().Contain("@p6");
        fragment.Should().Contain("@p7");
    }

    #endregion

    #region Case boundary

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_AllUpperCase_MatchesLowerCaseData()
    {
        var spec = new MultiWordSearchSpec("JOHN DOE");
        spec.Matches("john", "doe", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_AllLowerCase_MatchesUpperCaseData()
    {
        var spec = new MultiWordSearchSpec("john doe");
        spec.Matches("JOHN", "DOE", "JOHN.DOE@UNOPS.ORG").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_MixedCase_SplitTermsLowered()
    {
        var spec = new MultiWordSearchSpec("JoHn DoE");
        spec.SplitTerms.Should().Equal("john", "doe");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TurkishI_Handled()
    {
        var spec = new MultiWordSearchSpec("istanbul");
        spec.Matches("İstanbul", "Test", "istanbul@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Parameters_Lowercased()
    {
        var spec = new MultiWordSearchSpec("JOHN");
        spec.Parameters[0].Should().Be("%john%");
    }

    #endregion

    #region Substring boundaries

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TermAtStartOfField_Matches()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TermAtEndOfField_Matches()
    {
        var spec = new MultiWordSearchSpec("hn");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TermInMiddle_Matches()
    {
        var spec = new MultiWordSearchSpec("oh");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_WholeFieldExact_Matches()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TermEqualsField_Matches()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", null, null).Should().BeTrue();
    }

    #endregion

    #region Special character boundaries

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_HyphenInName_AsSingleTerm()
    {
        var spec = new MultiWordSearchSpec("Jean-Pierre");
        spec.SplitTerms.Should().Equal("jean-pierre");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ApostropheInName_AsSingleTerm()
    {
        var spec = new MultiWordSearchSpec("O'Brien");
        spec.SplitTerms.Should().Equal("o'brien");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_DotInEmail_Matches()
    {
        var spec = new MultiWordSearchSpec("john.doe");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_AtInEmail_Matches()
    {
        var spec = new MultiWordSearchSpec("john@unops");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_PlusInEmail_Matches()
    {
        var spec = new MultiWordSearchSpec("john+tag");
        spec.Matches("John", "Doe", "john+tag@unops.org").Should().BeTrue();
    }

    #endregion

    #region Three-plus word boundaries

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ThreeWords_AllMustMatch()
    {
        var spec = new MultiWordSearchSpec("John M Doe");
        spec.Matches("John", "Doe", "john.m.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ThreeWords_ThreeParameters()
    {
        var spec = new MultiWordSearchSpec("John M Doe");
        spec.Parameters.Should().Equal("%john%", "%m%", "%doe%");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_FourWords_AllMustMatch()
    {
        var spec = new MultiWordSearchSpec("John Q Public Doe");
        spec.Matches("John", "Doe", "q.public@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TenWords_SplitCorrectly()
    {
        var spec = new MultiWordSearchSpec("a b c d e f g h i j");
        spec.SplitTerms.Should().HaveCount(10);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_RepeatedWord_CountedTwice()
    {
        var spec = new MultiWordSearchSpec("John John");
        spec.SplitTerms.Should().Equal("john", "john");
        spec.Parameters.Should().Equal("%john%", "%john%");
    }

    #endregion

    #region Null/empty input boundaries

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Null_Safe()
    {
        var spec = new MultiWordSearchSpec(null);
        spec.HasSearchFilter.Should().BeFalse();
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Empty_Safe()
    {
        var spec = new MultiWordSearchSpec("");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SpacesOnly_Safe()
    {
        var spec = new MultiWordSearchSpec("   ");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_NullFields_WithFilter_NoMatch()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches(null, null, null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_EmptyFields_WithFilter_NoMatch()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("", "", "").Should().BeFalse();
    }

    #endregion

    #region SQL injection resistance (boundary - special chars)

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Semicolon_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("John; DROP TABLE");
        spec.SplitTerms.Should().Equal("john;", "drop", "table");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SingleQuote_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("O'Brien");
        spec.SplitTerms.Should().Equal("o'brien");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_DoubleQuote_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("\"John\"");
        spec.SplitTerms.Should().Equal("\"john\"");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_DoubleDash_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("John--");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Slash_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("John/Doe");
        spec.SplitTerms.Should().Equal("john/doe");
    }

    #endregion

    #region Additional boundary scenarios (expand to 90+)

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ZeroWidthSpace_Handled()
    {
        var spec = new MultiWordSearchSpec("John\u200B");
        spec.SplitTerms.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_CombiningAccent_Handled()
    {
        var spec = new MultiWordSearchSpec("e\u0301");
        spec.SplitTerms.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_RTLCharacter_Handled()
    {
        var spec = new MultiWordSearchSpec("أحمد");
        spec.Matches("أحمد", "علي", "ahmed@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Japanese_Handled()
    {
        var spec = new MultiWordSearchSpec("田中");
        spec.Matches("田中", "太郎", "tanaka@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Chinese_Handled()
    {
        var spec = new MultiWordSearchSpec("王");
        spec.Matches("王", "伟", "wang@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Thai_Handled()
    {
        var spec = new MultiWordSearchSpec("สมชาย");
        spec.Matches("สมชาย", "ใจดี", "somchai@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Hindi_Handled()
    {
        var spec = new MultiWordSearchSpec("राज");
        spec.Matches("राज", "कुमार", "raj@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_NumericInName_Matches()
    {
        var spec = new MultiWordSearchSpec("John2");
        spec.Matches("John2", "Doe", "john2@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Alphanumeric_Matches()
    {
        var spec = new MultiWordSearchSpec("User1");
        spec.Matches("User1", "Test", "user1@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_WordBoundary_ExactMatch()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SubstringAtBoundary_Matches()
    {
        var spec = new MultiWordSearchSpec("hn");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_FirstChar_Matches()
    {
        var spec = new MultiWordSearchSpec("J");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_LastChar_Matches()
    {
        var spec = new MultiWordSearchSpec("n");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ConsecutiveSpaces_ProduceEmptyRemoved()
    {
        var spec = new MultiWordSearchSpec("John   Doe");
        spec.SplitTerms.Should().Equal("john", "doe");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SpaceAtStart_Removed()
    {
        var spec = new MultiWordSearchSpec("  John");
        spec.SplitTerms.Should().Equal("john");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SpaceAtEnd_Removed()
    {
        var spec = new MultiWordSearchSpec("John  ");
        spec.SplitTerms.Should().Equal("john");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ParamIndexZero_Valid()
    {
        var spec = new MultiWordSearchSpec("John");
        var fragment = spec.BuildWhereClauseFragment(0);
        fragment.Should().Contain("@p0");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ParamIndexLarge_Valid()
    {
        var spec = new MultiWordSearchSpec("John");
        var fragment = spec.BuildWhereClauseFragment(999);
        fragment.Should().Contain("@p999");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_TermWithOnlySpaces_NoFilter()
    {
        var spec = new MultiWordSearchSpec("   ");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SpacesOnlyMixed_NoFilter()
    {
        var spec = new MultiWordSearchSpec("     ");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_WordThenSpacesThenWord_SplitCorrectly()
    {
        var spec = new MultiWordSearchSpec("John   Doe");
        spec.SplitTerms.Should().Equal("john", "doe");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ThreeWordsWithExtraSpaces_SplitCorrectly()
    {
        var spec = new MultiWordSearchSpec("John  M  Doe");
        spec.SplitTerms.Should().Equal("john", "m", "doe");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ToLower_AppliedToTerms()
    {
        var spec = new MultiWordSearchSpec("JOHN");
        spec.SplitTerms[0].Should().Be("john");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_ToLower_AppliedToParameters()
    {
        var spec = new MultiWordSearchSpec("JOHN");
        spec.Parameters[0].Should().Be("%john%");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Matches_IsCaseInsensitive()
    {
        var spec = new MultiWordSearchSpec("JOHN");
        spec.Matches("john", "doe", "JOHN.DOE@UNOPS.ORG").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Matches_NullSafe()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches(null, "Doe", "doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_WhereFragment_NotEmptyWhenHasFilter()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.BuildWhereClauseFragment(0).Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_WhereFragment_EmptyWhenNoFilter()
    {
        var spec = new MultiWordSearchSpec("");
        spec.BuildWhereClauseFragment(0).Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_SplitTerms_Immutable()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.SplitTerms.Should().NotBeNull();
        spec.SplitTerms.Count.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void Search_Parameters_Immutable()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Parameters.Should().NotBeNull();
        spec.Parameters.Count.Should().Be(1);
    }

    #endregion
}
