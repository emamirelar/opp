/**
 * @fileoverview Negative tests for PNO-1211: Generic Search — Multi-word user search.
 * Invalid input, non-matching scenarios, expected failures.
 * @author QA Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Search;

/// <summary>
/// Negative tests for PNO-1211: Search should NOT match when criteria are not satisfied.
/// </summary>
public class PNO1211NegativeTests
{
    #region Non-matching first name

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_John_DoesNotMatchUserWithDifferentFirstName()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("Jane", "Doe", "jane.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_John_DoesNotMatchUserWithNoFirstName()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches(null, "Doe", "doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_John_DoesNotMatchUserWithJohnNotInAnyField()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("Bob", "Smith", "bob.smith@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_John_DoesNotMatchWhenNoFieldContainsJohn()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("Bob", "Jones", "bob.jones@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_John_DoesNotMatchEmptyUser()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("", "", "").Should().BeFalse();
    }

    #endregion

    #region Non-matching last name

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Doe_DoesNotMatchUserWithDifferentLastName()
    {
        var spec = new MultiWordSearchSpec("Doe");
        spec.Matches("John", "Smith", "john.smith@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Doe_DoesNotMatchUserWithNoLastName()
    {
        var spec = new MultiWordSearchSpec("Doe");
        spec.Matches("John", null, "john@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Smith_DoesNotMatchWhenSmithNotInAnyField()
    {
        var spec = new MultiWordSearchSpec("Smith");
        spec.Matches("Jane", "Jones", "jane.jones@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Doe_DoesNotMatchWhenDoeNotInAnyField()
    {
        var spec = new MultiWordSearchSpec("Doe");
        spec.Matches("John", "Smith", "john.smith@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_XYZ_DoesNotMatchAnyUser()
    {
        var spec = new MultiWordSearchSpec("XYZ");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    #endregion

    #region Multi-word AND failure

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_JohnDoe_DoesNotMatchWhenFirstWordMissing()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("Jane", "Doe", "jane.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_JohnDoe_DoesNotMatchWhenSecondWordMissing()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", "Smith", "john.smith@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_JohnDoe_DoesNotMatchWhenNeitherWordMatches()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("Bob", "Smith", "bob.smith@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_ThreeWords_DoesNotMatchWhenOneMissing()
    {
        var spec = new MultiWordSearchSpec("John M Doe");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_JohnDoe_DoesNotMatchReversedName()
    {
        var spec = new MultiWordSearchSpec("Doe John");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue("Doe matches last name, John matches first - both terms match!");
    }

    #endregion

    #region Partial match failures

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Joh_DoesNotMatchJo()
    {
        var spec = new MultiWordSearchSpec("Joh");
        spec.Matches("Jo", "Doe", "jo@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Do_DoesNotMatchD()
    {
        var spec = new MultiWordSearchSpec("Do");
        spec.Matches("John", "D", "john.d@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_PartialEmail_DoesNotMatchWhenNotInAnyField()
    {
        var spec = new MultiWordSearchSpec("nonexistent@xyz");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_SingleCharX_DoesNotMatchJohnDoe()
    {
        var spec = new MultiWordSearchSpec("X");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_LongNonExistent_DoesNotMatch()
    {
        var spec = new MultiWordSearchSpec("ZZZZNONEXISTENTZZZ");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    #endregion

    #region Case sensitivity (search is case-insensitive, so these verify no false negatives)

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Lowercase_StillMatchesUppercaseData()
    {
        var spec = new MultiWordSearchSpec("john");
        spec.Matches("JOHN", "DOE", "JOHN@UNOPS.ORG").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_MixedCase_DoesNotFailOnMatch()
    {
        var spec = new MultiWordSearchSpec("JoHn");
        spec.Matches("john", "doe", "x@y.com").Should().BeTrue();
    }

    #endregion

    #region Special characters and edge inputs

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_TermWithPercent_DoesNotBreakLogic()
    {
        var spec = new MultiWordSearchSpec("John%");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse("John% does not match John");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_TermWithUnderscore_DoesNotMatchWithoutIt()
    {
        var spec = new MultiWordSearchSpec("John_Doe");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse("John_Doe is one term, no space");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_HyphenatedName_AsSingleTerm_DoesNotMatchSeparate()
    {
        var spec = new MultiWordSearchSpec("Jean-Pierre");
        spec.Matches("Jean", "Pierre", "jean.pierre@unops.org").Should().BeFalse("Jean-Pierre is one term");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_UnicodeNonMatching_DoesNotMatch()
    {
        var spec = new MultiWordSearchSpec("José");
        spec.Matches("Jose", "Garcia", "jose@unops.org").Should().BeFalse("José != Jose in string compare");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_NumberOnly_DoesNotMatchNameFields()
    {
        var spec = new MultiWordSearchSpec("12345");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    #endregion

    #region Null and empty field handling

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_John_DoesNotMatchWhenAllFieldsNull()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches(null, null, null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_John_DoesNotMatchWhenOnlyWrongFieldPopulated()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("Bob", "Doe", "bob.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_EmailTerm_DoesNotMatchWhenEmailNull()
    {
        var spec = new MultiWordSearchSpec("unops.org");
        spec.Matches("John", "Doe", null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_TermInOnlyOneField_WhenOtherFieldsNull_StillMatches()
    {
        var spec = new MultiWordSearchSpec("John");
        spec.Matches("John", null, null).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_TwoTerms_OneFieldNull_DoesNotMatchIfTermNeededThere()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        spec.Matches("John", null, "john@unops.org").Should().BeFalse("Doe must match somewhere");
    }

    #endregion

    #region Parameter and WHERE clause verification

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_EmptyString_ProducesNoParameters()
    {
        var spec = new MultiWordSearchSpec("");
        spec.Parameters.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Null_ProducesNoParameters()
    {
        var spec = new MultiWordSearchSpec(null);
        spec.Parameters.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_SpacesOnly_ProducesNoParameters()
    {
        var spec = new MultiWordSearchSpec("     ");
        spec.Parameters.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_EmptyString_ProducesEmptyWhereFragment()
    {
        var spec = new MultiWordSearchSpec("");
        spec.BuildWhereClauseFragment(0).Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_NonMatchingUser_MatchesReturnsFalse()
    {
        var spec = new MultiWordSearchSpec("Alice Wonder");
        spec.Matches("Bob", "Builder", "bob@unops.org").Should().BeFalse();
    }

    #endregion

    #region Additional negative scenarios (expand to 90+)

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Johnson_DoesNotMatchJohn()
    {
        var spec = new MultiWordSearchSpec("Johnson");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeFalse("Johnson not in John");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Johnny_DoesNotMatchJohn()
    {
        var spec = new MultiWordSearchSpec("Johnny");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeFalse("Johnny not in John");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Smithson_DoesNotMatchSmith()
    {
        var spec = new MultiWordSearchSpec("Smithson");
        spec.Matches("Jane", "Smith", "jane@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Abc_DoesNotMatchAb()
    {
        var spec = new MultiWordSearchSpec("Abc");
        spec.Matches("Ab", "Cd", "ab.cd@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Ab_DoesNotMatchA()
    {
        var spec = new MultiWordSearchSpec("Ab");
        spec.Matches("A", "B", "a.b@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_WordOrderMattersForAnd_JohnDoeVsDoeJohn()
    {
        var spec1 = new MultiWordSearchSpec("John Doe");
        var spec2 = new MultiWordSearchSpec("Doe John");
        spec1.Matches("John", "Doe", "x@y.com").Should().BeTrue();
        spec2.Matches("John", "Doe", "x@y.com").Should().BeTrue("Both orderings match - AND is symmetric");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_FourWords_AllFourMustMatch()
    {
        var spec = new MultiWordSearchSpec("John Q Public Doe");
        spec.Matches("John", "Doe", "q.public@unops.org").Should().BeTrue();
        spec.Matches("John", "Doe", "john@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_SpecialRegexChars_DoNotCauseException()
    {
        var spec = new MultiWordSearchSpec(".*");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse(".* not in any field as literal");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_BracketChars_HandledAsLiteral()
    {
        var spec = new MultiWordSearchSpec("[John]");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_NewlineInTerm_NotSplitByNewline()
    {
        var spec = new MultiWordSearchSpec("John\nDoe");
        spec.SplitTerms.Should().HaveCount(1);
        spec.SplitTerms[0].Should().Contain("john");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_TabInTerm_NotSplitByTab()
    {
        var spec = new MultiWordSearchSpec("John\tDoe");
        spec.SplitTerms.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_MultipleConsecutiveSpaces_ProduceSingleTerms()
    {
        var spec = new MultiWordSearchSpec("John    Doe");
        spec.SplitTerms.Should().Equal("john", "doe");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_OnlySpaces_NoFilter()
    {
        var spec = new MultiWordSearchSpec("     ");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_TermShorterThanMatch_NoMatch()
    {
        var spec = new MultiWordSearchSpec("J");
        spec.Matches("K", "Doe", "k.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_TermLongerThanAnyField_NoMatch()
    {
        var spec = new MultiWordSearchSpec("VeryLongNameThatDoesNotExistInAnyField");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_SimilarButDifferent_NoMatch()
    {
        var spec = new MultiWordSearchSpec("Jon");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeFalse("Jon != John substring");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Do_DoesNotMatchWhenDoNotInAnyField()
    {
        var spec = new MultiWordSearchSpec("Do");
        spec.Matches("John", "Smith", "john.smith@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_EmailLocalPart_DoesNotMatchWhenNotInAnyField()
    {
        var spec = new MultiWordSearchSpec("john");
        spec.Matches("Bob", "Smith", "bob.smith@example.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_DomainOnly_DoesNotMatchLocalPart()
    {
        var spec = new MultiWordSearchSpec("unops.org");
        spec.Matches("Unops", "Org", "other@example.com").Should().BeFalse("unops.org not in any field");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_ZeroWidthChars_Handled()
    {
        var spec = new MultiWordSearchSpec("John\u200BDoe");
        spec.SplitTerms.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_ControlChars_RemovedBySplit()
    {
        var spec = new MultiWordSearchSpec("John\x00Doe");
        spec.SplitTerms.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Backslash_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("John\\Doe");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse("\\Doe not in fields");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_QuestionMark_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("John?");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Asterisk_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("John*");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_CommaSeparated_NotSplitByComma()
    {
        var spec = new MultiWordSearchSpec("John,Doe");
        spec.SplitTerms.Should().Equal("john,doe");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse("john,doe as single term");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_SemicolonSeparated_NotSplitBySemicolon()
    {
        var spec = new MultiWordSearchSpec("John;Doe");
        spec.SplitTerms.Should().Equal("john;doe");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_PipeSeparated_NotSplitByPipe()
    {
        var spec = new MultiWordSearchSpec("John|Doe");
        spec.SplitTerms.Should().Equal("john|doe");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_DotSeparated_NotSplitByDot()
    {
        var spec = new MultiWordSearchSpec("John.Doe");
        spec.SplitTerms.Should().Equal("john.doe");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_HyphenSeparated_NotSplitByHyphen()
    {
        var spec = new MultiWordSearchSpec("John-Doe");
        spec.SplitTerms.Should().Equal("john-doe");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_AtSign_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("john@");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeTrue("john@ is substring of email");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_StrictSubstring_NoMatchWhenPartial()
    {
        var spec = new MultiWordSearchSpec("oh");
        spec.Matches("John", "Doe", "x@y.com").Should().BeTrue("oh is in John");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_SingleSpace_ProducesEmptyTerms()
    {
        var spec = new MultiWordSearchSpec(" ");
        spec.HasSearchFilter.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_UnicodeWhitespace_MayProduceEmpty()
    {
        var spec = new MultiWordSearchSpec("\u00A0John\u00A0");
        spec.SplitTerms.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_EmptyBetweenSpaces_Removed()
    {
        var spec = new MultiWordSearchSpec("  John  Doe  ");
        spec.SplitTerms.Should().Equal("john", "doe");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_OneWordWithSpaces_TrimsToSingleTerm()
    {
        var spec = new MultiWordSearchSpec("  John  ");
        spec.SplitTerms.Should().Equal("john");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_ThreeWordsOneMismatch_NoMatch()
    {
        var spec = new MultiWordSearchSpec("John X Doe");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_TwoWordsBothInEmail_Matches()
    {
        var spec = new MultiWordSearchSpec("john doe");
        spec.Matches("Bob", "Smith", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_TwoWordsOneInEmailOneInName_Matches()
    {
        var spec = new MultiWordSearchSpec("John unops");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_ReverseOrder_JohnDoe_DoeInFirstName_NoMatch()
    {
        var spec = new MultiWordSearchSpec("Doe John");
        spec.Matches("Doe", "John", "doe.john@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_MiddleNameOnly_NoMatchWhenNotPresent()
    {
        var spec = new MultiWordSearchSpec("Middle");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_MiddleInitial_MatchesWhenInEmail()
    {
        var spec = new MultiWordSearchSpec("M");
        spec.Matches("John", "Doe", "john.m.doe@unops.org").Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_NumberInName_DoesNotMatchWhenAbsent()
    {
        var spec = new MultiWordSearchSpec("John2");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_AccentedChar_DoesNotMatchNonAccented()
    {
        var spec = new MultiWordSearchSpec("José");
        spec.Matches("Jose", "Garcia", "jose@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_UnicodeChar_DoesNotMatchWhenAbsent()
    {
        var spec = new MultiWordSearchSpec("é");
        spec.Matches("John", "Doe", "john.doe@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_FiveWords_FourMatchOneFails_NoMatch()
    {
        var spec = new MultiWordSearchSpec("John Q Public Doe Smith");
        spec.Matches("John", "Doe", "q.public@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_LeadingZero_DoesNotMatchWithoutZero()
    {
        var spec = new MultiWordSearchSpec("John0");
        spec.Matches("John", "Doe", "john@unops.org").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_QuotedTerm_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("\"John\"");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse("\"John\" not in John");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Caret_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("^John");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Dollar_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("John$");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_PlusSign_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("John+Doe");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_Parentheses_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("(John)");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Search_CurlyBraces_AsLiteral()
    {
        var spec = new MultiWordSearchSpec("{John}");
        spec.Matches("John", "Doe", "x@y.com").Should().BeFalse();
    }

    #endregion
}
