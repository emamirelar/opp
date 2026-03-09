/**
 * @fileoverview Integration tests for PNO-1211: Generic Search — Multi-word user search.
 * Full pipeline simulation: user list + spec filter = filtered results.
 * @author QA Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Search;

/// <summary>
/// Integration tests: Full pipeline simulation — apply spec matching to user list, verify filtered results.
/// Models the end-to-end flow from UserManagementRequest.SearchTerm to filtered UserIds.
/// </summary>
public class PNO1211IntegrationTests
{
    private static List<(string? FirstName, string? LastName, string? Email)> GetTestUserList()
    {
        return new List<(string?, string?, string?)>
        {
            ("John", "Doe", "john.doe@unops.org"),
            ("Jane", "Smith", "jane.smith@unops.org"),
            ("Bob", "Johnson", "bob.johnson@unops.org"),
            ("Alice", "Williams", "alice.williams@unops.org"),
            ("John", "Smith", "john.smith@unops.org"),
            ("Jane", "Doe", "jane.doe@unops.org"),
            ("Perminder", "Saluja", "perminder.saluja@unops.org"),
        };
    }

    private static List<(string? FirstName, string? LastName, string? Email)> FilterUsers(
        List<(string? FirstName, string? LastName, string? Email)> users, string? searchTerm)
    {
        var spec = new MultiWordSearchSpec(searchTerm);
        return users.Where(u => spec.Matches(u.FirstName, u.LastName, u.Email)).ToList();
    }

    #region Full pipeline - First name search

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchJohn_ReturnsJohnDoeJohnSmithAndBobJohnson()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John");
        filtered.Should().HaveCount(3);
        filtered.Should().Contain(u => u.FirstName == "John" && u.LastName == "Doe");
        filtered.Should().Contain(u => u.FirstName == "John" && u.LastName == "Smith");
        filtered.Should().Contain(u => u.LastName == "Johnson");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchJane_ReturnsJaneSmithAndJaneDoe()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Jane");
        filtered.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchPerminder_ReturnsPerminderSaluja()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Perminder");
        filtered.Should().HaveCount(1);
        filtered[0].LastName.Should().Be("Saluja");
    }

    #endregion

    #region Full pipeline - Last name search

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchDoe_ReturnsJohnDoeAndJaneDoe()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Doe");
        filtered.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchSmith_ReturnsJaneSmithAndJohnSmith()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Smith");
        filtered.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchSaluja_ReturnsPerminderSaluja()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Saluja");
        filtered.Should().HaveCount(1);
    }

    #endregion

    #region Full pipeline - Full name search

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchJohnDoe_ReturnsJohnDoeOnly()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John Doe");
        filtered.Should().HaveCount(1);
        filtered[0].FirstName.Should().Be("John");
        filtered[0].LastName.Should().Be("Doe");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchJaneSmith_ReturnsJaneSmithOnly()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Jane Smith");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchPerminderSaluja_ReturnsOneUser()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Perminder Saluja");
        filtered.Should().HaveCount(1);
        filtered[0].Email.Should().Be("perminder.saluja@unops.org");
    }

    #endregion

    #region Full pipeline - Email search

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchByEmail_ReturnsMatchingUser()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "perminder.saluja");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchByDomain_ReturnsAllUnopsUsers()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "unops.org");
        filtered.Should().HaveCount(7);
    }

    #endregion

    #region Full pipeline - Empty/whitespace

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_EmptySearch_ReturnsAllUsers()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "");
        filtered.Should().HaveCount(7);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_NullSearch_ReturnsAllUsers()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, null);
        filtered.Should().HaveCount(7);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_WhitespaceSearch_ReturnsAllUsers()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "   ");
        filtered.Should().HaveCount(7);
    }

    #endregion

    #region Full pipeline - No matches

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_NonExistentName_ReturnsEmpty()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "ZZZNonexistent");
        filtered.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_PartialNonMatch_ReturnsEmpty()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John X");
        filtered.Should().BeEmpty();
    }

    #endregion

    #region Full pipeline - Case insensitivity

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_UpperCaseSearch_ReturnsMatches()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "JOHN DOE");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_LowerCaseSearch_ReturnsMatches()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "john doe");
        filtered.Should().HaveCount(1);
    }

    #endregion

    #region Full pipeline - Multiple spaces

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_MultipleSpaces_ReturnsCorrectMatch()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John   Doe");
        filtered.Should().HaveCount(1);
    }

    #endregion

    #region Full pipeline - Partial match

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_PartialFirstName_ReturnsMatches()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Joh");
        filtered.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_PartialLastName_ReturnsMatches()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Smi");
        filtered.Should().HaveCount(2);
    }

    #endregion

    #region Full pipeline - Combined with sort simulation

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_FilterThenSort_OrderPreserved()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John");
        var sorted = filtered.OrderBy(u => u.LastName).ToList();
        sorted.Should().HaveCount(3);
        sorted[0].LastName.Should().Be("Doe");
        sorted[1].LastName.Should().Be("Johnson");
        sorted[2].LastName.Should().Be("Smith");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_FilterThenCount_MatchesFilteredCount()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Doe");
        filtered.Count.Should().Be(2);
    }

    #endregion

    #region Full pipeline - Pagination simulation

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_FilterThenPaginate_ReturnsCorrectPage()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John");
        var page = filtered.Skip(0).Take(1).ToList();
        page.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_TotalCount_MatchesFilteredCount()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Smith");
        var totalCount = filtered.Count;
        totalCount.Should().Be(2);
    }

    #endregion

    #region Full pipeline - Multi-word AND

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_JohnDoe_ExcludesJohnSmith()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John Doe");
        filtered.Should().NotContain(u => u.LastName == "Smith");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_JohnDoe_ExcludesJaneDoe()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John Doe");
        filtered.Should().NotContain(u => u.FirstName == "Jane");
    }

    #endregion

    #region Full pipeline - Large dataset simulation

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_LargeUserList_FilterCorrectly()
    {
        var users = Enumerable.Range(0, 1000)
            .Select(i => ($"User{i}", $"Last{i}", $"user{i}@unops.org"))
            .ToList();
        users.Add(("John", "Doe", "john.doe@unops.org"));
        var filtered = FilterUsers(users, "John Doe");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_ManyMatchingFirstNames_AllReturned()
    {
        var users = Enumerable.Range(0, 50)
            .Select(i => ("John", $"Last{i}", $"john{i}@unops.org"))
            .ToList();
        var filtered = FilterUsers(users, "John");
        filtered.Should().HaveCount(50);
    }

    #endregion

    #region Full pipeline - Edge cases

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SingleUser_SearchMatches_ReturnsOne()
    {
        var users = new List<(string?, string?, string?)> { ("John", "Doe", "john@unops.org") };
        var filtered = FilterUsers(users, "John");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SingleUser_SearchNoMatch_ReturnsEmpty()
    {
        var users = new List<(string?, string?, string?)> { ("John", "Doe", "john@unops.org") };
        var filtered = FilterUsers(users, "Jane");
        filtered.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_EmptyUserList_ReturnsEmpty()
    {
        var users = new List<(string?, string?, string?)>();
        var filtered = FilterUsers(users, "John");
        filtered.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_NullFieldsInUser_Handled()
    {
        var users = new List<(string?, string?, string?)> { (null, "Doe", "doe@unops.org") };
        var filtered = FilterUsers(users, "Doe");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_AllNullFields_NoMatch()
    {
        var users = new List<(string?, string?, string?)> { (null, null, null) };
        var filtered = FilterUsers(users, "John");
        filtered.Should().BeEmpty();
    }

    #endregion

    #region Full pipeline - Parameter and WHERE verification

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SpecAndFilter_Consistent()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        var users = GetTestUserList();
        var filtered = users.Where(u => spec.Matches(u.FirstName, u.LastName, u.Email)).ToList();
        filtered.Should().HaveCount(1);
        filtered[0].FirstName.Should().Be("John");
        filtered[0].LastName.Should().Be("Doe");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_WhereFragment_IntegratesWithBaseConditions()
    {
        var spec = new MultiWordSearchSpec("John");
        var fragment = spec.BuildWhereClauseFragment(0);
        var fullWhere = $"up.\"IsDeleted\" = false AND {fragment}";
        fullWhere.Should().Contain("IsDeleted");
        fullWhere.Should().Contain("FirstName");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_ParamIndexOffset_WhenOtherFiltersPresent()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        var fragment = spec.BuildWhereClauseFragment(2);
        fragment.Should().Contain("@p2");
        fragment.Should().Contain("@p3");
    }

    #endregion

    #region Additional integration scenarios (expand to 90+)

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_ThreeWordSearch_AllMatch()
    {
        var users = new List<(string?, string?, string?)>
        {
            ("John", "Doe", "john.doe@unops.org"),
            ("John", "Doe", "john.m.doe@unops.org"),
        };
        var filtered = FilterUsers(users, "John M Doe");
        filtered.Should().HaveCount(1);
        filtered[0].Email.Should().Contain("m");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_ReverseNameOrder_Matches()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Doe John");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_EmailPart_Matches()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "perminder");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SubstringInMultipleUsers_AllReturned()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "oh");
        filtered.Should().Contain(u => u.FirstName == "John");
        filtered.Should().Contain(u => u.LastName == "Johnson");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_RepeatedFilter_SameResult()
    {
        var users = GetTestUserList();
        var r1 = FilterUsers(users, "John Doe");
        var r2 = FilterUsers(users, "John Doe");
        r1.Should().BeEquivalentTo(r2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_DifferentSearches_DifferentResults()
    {
        var users = GetTestUserList();
        var john = FilterUsers(users, "John");
        var jane = FilterUsers(users, "Jane");
        john.Should().NotBeEquivalentTo(jane);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_UnicodeNames_Matches()
    {
        var users = new List<(string?, string?, string?)> { ("José", "García", "jose.garcia@unops.org") };
        var filtered = FilterUsers(users, "José");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_HyphenatedName_AsSingleTerm()
    {
        var users = new List<(string?, string?, string?)> { ("Jean-Pierre", "Dupont", "jp@unops.org") };
        var filtered = FilterUsers(users, "Jean-Pierre");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_LeadingTrailingSpaces_Handled()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "  John Doe  ");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SpecReuse_MultipleSearches()
    {
        var users = GetTestUserList();
        var spec1 = new MultiWordSearchSpec("John");
        var spec2 = new MultiWordSearchSpec("Jane");
        var johnCount = users.Count(u => spec1.Matches(u.FirstName, u.LastName, u.Email));
        var janeCount = users.Count(u => spec2.Matches(u.FirstName, u.LastName, u.Email));
        johnCount.Should().Be(3);
        janeCount.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_AllUsersMatchUnops_SevenResults()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "unops");
        filtered.Should().HaveCount(7);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_NoUserMatchesXyz_ZeroResults()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "xyz");
        filtered.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_Johnson_MatchesBobJohnson()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Johnson");
        filtered.Should().HaveCount(1);
        filtered[0].FirstName.Should().Be("Bob");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_Williams_MatchesAliceWilliams()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Williams");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_Bob_ReturnsBobJohnson()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Bob");
        filtered.Should().HaveCount(1);
        filtered[0].LastName.Should().Be("Johnson");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_Alice_ReturnsAliceWilliams()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Alice");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_AliceWilliams_ReturnsOne()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Alice Williams");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_BobJohnson_ReturnsOne()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Bob Johnson");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SingleCharJ_ReturnsJohnsAndJanesAndSaluja()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "J");
        filtered.Should().HaveCount(6);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchSmith_ReturnsSmiths()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Smith");
        filtered.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_JohnSmith_ReturnsOne()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John Smith");
        filtered.Should().HaveCount(1);
        filtered[0].Email.Should().Be("john.smith@unops.org");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_JaneDoe_ReturnsOne()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Jane Doe");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SimulateManageUserPermissions_Scenario()
    {
        var users = GetTestUserList();
        var searchTerm = "Perminder Saluja";
        var filtered = FilterUsers(users, searchTerm);
        filtered.Should().HaveCount(1);
        filtered[0].Email.Should().Contain("perminder");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SimulateNewInteractionPersonnel_Scenario()
    {
        var users = GetTestUserList();
        var searchTerm = "John";
        var filtered = FilterUsers(users, searchTerm);
        filtered.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_FullNameVsPartial_FullNameNarrower()
    {
        var users = GetTestUserList();
        var fullName = FilterUsers(users, "John Doe");
        var partial = FilterUsers(users, "John");
        fullName.Should().HaveCount(1);
        partial.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_LastThenFirst_Matches()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Saluja Perminder");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_EmailThenName_Matches()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "perminder Saluja");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_ThreeTerms_AllInDifferentFields()
    {
        var users = new List<(string?, string?, string?)>
        {
            ("John", "Doe", "middle@unops.org"),
        };
        var filtered = FilterUsers(users, "John middle Doe");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_EmptyFilter_ThenPaginate()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "");
        var page = filtered.Skip(2).Take(2).ToList();
        page.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_FilterThenSkipTake_Consistent()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "John");
        var first = filtered.Take(1).ToList();
        var second = filtered.Skip(1).Take(1).ToList();
        first.Should().HaveCount(1);
        second.Should().HaveCount(1);
        first[0].Should().NotBe(second[0]);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_TotalCount_ForPagination()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Doe");
        var totalCount = filtered.Count;
        var pageSize = 10;
        var pageIndex = 0;
        var page = filtered.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        page.Should().HaveCount(2);
        totalCount.Should().Be(2);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_RequestResponse_Contract()
    {
        var request = new { SearchTerm = "John Doe" };
        var users = GetTestUserList();
        var filtered = FilterUsers(users, request.SearchTerm);
        var response = new { TotalCount = filtered.Count, Records = filtered };
        response.TotalCount.Should().Be(1);
        response.Records.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_MultipleFilters_Sequential()
    {
        var users = GetTestUserList();
        var step1 = FilterUsers(users, "John");
        var step2 = FilterUsers(step1, "Doe");
        step2.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SpecBuildWhere_IntegratesInSql()
    {
        var spec = new MultiWordSearchSpec("John Doe");
        var fragment = spec.BuildWhereClauseFragment(0);
        var sql = $@"SELECT up.""UserId"" FROM public.""UserProfile"" up WHERE up.""IsDeleted"" = false AND {fragment}";
        sql.Should().Contain("IsDeleted");
        sql.Should().Contain("@p0");
        sql.Should().Contain("@p1");
        sql.Should().Contain("FirstName");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchWilliams_ReturnsAliceWilliams()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Williams");
        filtered.Should().HaveCount(1);
        filtered[0].FirstName.Should().Be("Alice");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchAlice_ReturnsAliceWilliams()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Alice");
        filtered.Should().HaveCount(1);
        filtered[0].LastName.Should().Be("Williams");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchBob_ReturnsBobJohnson()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Bob");
        filtered.Should().HaveCount(1);
        filtered[0].LastName.Should().Be("Johnson");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchJohnson_ReturnsBobJohnson()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Johnson");
        filtered.Should().HaveCount(1);
        filtered[0].FirstName.Should().Be("Bob");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchUnops_ReturnsAll()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "unops");
        filtered.Should().HaveCount(7);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchDot_ReturnsEmailMatches()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, ".");
        filtered.Should().HaveCount(7);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchCaseInsensitive_JohnVsJOHN()
    {
        var users = GetTestUserList();
        var lower = FilterUsers(users, "john");
        var upper = FilterUsers(users, "JOHN");
        lower.Should().HaveCount(3);
        upper.Should().HaveCount(3);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchMultipleSpaces_ReturnsSameAsSingle()
    {
        var users = GetTestUserList();
        var single = FilterUsers(users, "John Doe");
        var multiple = FilterUsers(users, "John   Doe");
        single.Should().HaveCount(1);
        multiple.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchSingleCharA_ReturnsAliceAndJane()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "a");
        filtered.Should().HaveCount(4);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchSingleCharB_ReturnsBob()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "b");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchNonExistent_ReturnsEmpty()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "XyzNonexistent");
        filtered.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchAliceWilliams_ReturnsOne()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Alice Williams");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchBobJohnson_ReturnsOne()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Bob Johnson");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchPartialPerminder_ReturnsPerminderSaluja()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Perm");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchPartialSaluja_ReturnsPerminderSaluja()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "Salu");
        filtered.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void Pipeline_SearchEmailLocalPart_ReturnsMatch()
    {
        var users = GetTestUserList();
        var filtered = FilterUsers(users, "perminder.saluja");
        filtered.Should().HaveCount(1);
    }

    #endregion
}
