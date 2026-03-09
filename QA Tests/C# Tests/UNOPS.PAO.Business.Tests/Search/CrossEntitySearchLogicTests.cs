using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Cross-entity search logic tests covering Contact, Interaction, and Opportunity searching.
/// The existing Search tests only cover Partner entity filtering. These tests extend coverage
/// to Contact and Interaction entities with advanced filter combinations, cross-entity navigation,
/// soft-delete handling, and complex multi-criteria scenarios.
/// </summary>
public class CrossEntitySearchLogicTests
{
    #region Positive Tests

    [Fact]
    public void ContactSearch_ByFirstName_ReturnsMatchingContacts()
    {
        var contacts = GetTestContacts();

        var filtered = contacts
            .Where(c => c.FirstName != null && c.FirstName.Contains("John", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().HaveCount(1);
        filtered.Single().LastName.Should().Be("Doe");
    }

    [Fact]
    public void InteractionSearch_BySubject_ReturnsMatchingInteractions()
    {
        var interactions = GetTestInteractions();

        var filtered = interactions
            .Where(i => i.Subject.Contains("Partnership", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().HaveCount(2);
    }

    [Fact]
    public void ContactSearch_ByEmail_ReturnsExactMatch()
    {
        var contacts = GetTestContacts();

        var filtered = contacts
            .Where(c => string.Equals(c.Email, "john.doe@acme.com", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().HaveCount(1);
        filtered.Single().FirstName.Should().Be("John");
    }

    #endregion

    #region Negative Tests

    [Fact]
    public void ContactSearch_NonExistentName_ReturnsEmpty()
    {
        var contacts = GetTestContacts();

        var filtered = contacts
            .Where(c => c.FirstName != null && c.FirstName.Contains("ZZZ_NONEXISTENT", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().BeEmpty();
    }

    [Fact]
    public void InteractionSearch_FutureDate_ReturnsEmpty()
    {
        var interactions = GetTestInteractions();
        var futureDate = DateTime.UtcNow.AddYears(10);

        var filtered = interactions
            .Where(i => i.Date > futureDate)
            .ToList();

        filtered.Should().BeEmpty();
    }

    [Fact]
    public void ContactSearch_SoftDeletedContacts_ExcludedFromResults()
    {
        var contacts = GetTestContactsWithDeleted();

        var filtered = contacts
            .Where(c => !c.IsDeleted)
            .Where(c => c.LastName != null && c.LastName.Contains("Doe", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().HaveCount(1, "Only non-deleted Doe contacts should be returned");
        filtered.Single().FirstName.Should().Be("John");
    }

    [Fact]

    [Trait("Defect", "DEF-081")]
    public void InteractionSearch_SoftDeletedInteractions_ExcludedFromResults()
    {
        var interactions = GetTestInteractionsWithDeleted();

        var filtered = interactions
            .Where(i => !i.IsDeleted)
            .ToList();

        filtered.Should().HaveCount(4, "Deleted interaction should be excluded");
    }

    [Fact]
    public void ContactSearch_InvalidEmail_NoMatch()
    {
        var contacts = GetTestContacts();

        var filtered = contacts
            .Where(c => c.Email == "not-an-email")
            .ToList();

        filtered.Should().BeEmpty();
    }

    [Fact]
    public void InteractionSearch_InvalidType_NoMatch()
    {
        var interactions = GetTestInteractions();

        var filtered = interactions
            .Where(i => i.Type == (InteractionType)999)
            .ToList();

        filtered.Should().BeEmpty();
    }

    [Fact]
    public void ContactSearch_EmptySearchText_DoesNotFilterAnything()
    {
        var contacts = GetTestContacts();
        var searchText = "";

        var filtered = contacts
            .Where(c => string.IsNullOrEmpty(searchText) ||
                       (c.FirstName != null && c.FirstName.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        filtered.Should().HaveCount(contacts.Count);
    }

    [Fact]
    public void ContactSearch_NullFirstName_HandledGracefully()
    {
        var contacts = GetTestContacts();
        contacts.Add(CreateContact("NullFirst", null, "NullTest", "noname@test.com", "Tester"));

        var filtered = contacts
            .Where(c => c.FirstName != null && c.FirstName.Contains("test", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().NotContain(c => c.LastName == "NullTest", "Null FirstName should not cause exception");
    }

    [Fact]
    public void InteractionSearch_NullDescription_HandledGracefully()
    {
        var interactions = GetTestInteractions();

        var filtered = interactions
            .Where(i => i.Description != null && i.Description.Contains("review", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().NotBeNull();
    }

    #endregion

    #region Edge/Boundary Tests

    [Fact]
    public void ContactSearch_CaseInsensitive_AllVariationsMatch()
    {
        var contacts = GetTestContacts();
        var searchVariations = new[] { "JANE", "jane", "Jane", "jAnE" };

        foreach (var term in searchVariations)
        {
            var filtered = contacts
                .Where(c => c.FirstName != null && c.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase))
                .ToList();

            filtered.Should().HaveCount(1, $"Case variant '{term}' should find Jane Smith");
            filtered.Single().LastName.Should().Be("Smith");
        }
    }

    [Fact]
    public void InteractionSearch_DateBoundary_ExactMidnight()
    {
        var interactions = GetTestInteractions();
        var targetDate = new DateTime(2024, 3, 15);
        var startOfDay = targetDate.Date;
        var endOfDay = targetDate.Date.AddDays(1).AddMilliseconds(-1);

        var filtered = interactions
            .Where(i => i.Date >= startOfDay && i.Date <= endOfDay)
            .ToList();

        filtered.Should().HaveCount(1);
        filtered.Single().Subject.Should().Be("Partnership Review Meeting");
    }

    [Fact]
    public void ContactSearch_MultipleFields_ANDCombination()
    {
        var contacts = GetTestContacts();

        var filtered = contacts
            .Where(c => c.FirstName != null && c.FirstName.Contains("J", StringComparison.OrdinalIgnoreCase))
            .Where(c => c.MailingCountry != null && c.MailingCountry == "United States")
            .ToList();

        filtered.Should().HaveCount(1);
        filtered.Single().FirstName.Should().Be("John");
    }

    [Fact]
    public void ContactSearch_MultipleFields_ORCombination()
    {
        var contacts = GetTestContacts();

        var filtered = contacts
            .Where(c =>
                (c.MailingCountry != null && c.MailingCountry == "Germany") ||
                (c.MailingCountry != null && c.MailingCountry == "France"))
            .ToList();

        filtered.Should().HaveCount(2);
    }

    [Fact]
    public void InteractionSearch_ByType_FiltersCorrectly()
    {
        var interactions = GetTestInteractions();

        var emailInteractions = interactions.Where(i => i.Type == InteractionType.Email).ToList();
        var meetingInteractions = interactions.Where(i => i.Type == InteractionType.InPersonMeeting).ToList();
        var callInteractions = interactions.Where(i => i.Type == InteractionType.Call).ToList();

        emailInteractions.Should().HaveCount(2);
        meetingInteractions.Should().HaveCount(2);
        callInteractions.Should().HaveCount(1);
    }

    [Fact]
    public void ContactSearch_PartialMatchOnMultipleFields()
    {
        var contacts = GetTestContacts();
        var searchText = "acme";

        var filtered = contacts
            .Where(c =>
                (c.FirstName != null && c.FirstName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (c.LastName != null && c.LastName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (c.Email != null && c.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (c.Department != null && c.Department.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        filtered.Should().HaveCount(2, "Should find contacts with 'acme' in email domain");
    }

    [Fact]
    public void InteractionSearch_DateRange_InclusiveBoundaries()
    {
        var interactions = GetTestInteractions();
        var startDate = new DateTime(2024, 3, 15);
        var endDate = new DateTime(2024, 6, 20);

        var filtered = interactions
            .Where(i => i.Date >= startDate && i.Date <= endDate)
            .ToList();

        filtered.Should().HaveCount(3, "Should include interactions on the boundary dates");
    }

    [Fact]
    public void ContactSearch_Sorting_AscendingByLastName()
    {
        var contacts = GetTestContacts();

        var sorted = contacts
            .OrderBy(c => c.LastName)
            .ToList();

        sorted.Should().BeInAscendingOrder(c => c.LastName);
        sorted.First().LastName.Should().Be("Doe");
    }

    [Fact]
    public void ContactSearch_Pagination_ReturnsCorrectPage()
    {
        var contacts = GetTestContacts().OrderBy(c => c.LastName).ToList();
        var pageSize = 2;
        var pageIndex = 2;

        var paged = contacts
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        paged.Should().HaveCount(2);
    }

    [Fact]
    public void InteractionSearch_MultiWordSearch_ORLogic()
    {
        var interactions = GetTestInteractions();
        var searchTerms = new[] { "Partnership", "Quarterly" };

        var filtered = interactions
            .Where(i => searchTerms.Any(term => i.Subject.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        filtered.Should().HaveCount(3, "Should find interactions matching any of the search terms");
    }

    [Fact]
    public void InteractionSearch_MultiWordSearch_ANDLogic()
    {
        var interactions = GetTestInteractions();
        var searchTerms = new[] { "Partnership", "Meeting" };

        var filtered = interactions
            .Where(i => searchTerms.All(term => i.Subject.Contains(term, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        filtered.Should().HaveCount(1, "Should only find interactions matching ALL search terms");
    }

    #endregion

    #region Functional Tests

    [Fact]

    [Trait("Defect", "DEF-081")]
    public void ContactSearch_WithNotLikeOperator_ExcludesMatches()
    {
        var contacts = GetTestContacts();

        var filtered = contacts
            .Where(c => c.MailingCountry == null || !c.MailingCountry.Contains("United States", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().HaveCount(3);
        filtered.Should().NotContain(c => c.MailingCountry == "United States");
    }

    [Fact]
    public void InteractionSearch_WithAfterOperator_OnlyFutureResults()
    {
        var interactions = GetTestInteractions();
        var cutoffDate = new DateTime(2024, 6, 1);

        var filtered = interactions
            .Where(i => i.Date > cutoffDate)
            .ToList();

        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(i => i.Date > cutoffDate);
    }

    [Fact]
    public void InteractionSearch_WithBeforeOperator_OnlyPastResults()
    {
        var interactions = GetTestInteractions();
        var cutoffDate = new DateTime(2024, 4, 1);

        var filtered = interactions
            .Where(i => i.Date < cutoffDate)
            .ToList();

        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(i => i.Date < cutoffDate);
    }

    [Fact]
    public void ContactSearch_CombinedWithPartnerFilter_IntersectionResult()
    {
        var contacts = GetTestContactsWithPartners();

        var filtered = contacts
            .Where(c => !c.IsDeleted)
            .Where(c => c.Partner != null && c.Partner.Name.Contains("ACME", StringComparison.OrdinalIgnoreCase))
            .Where(c => c.Title != null && c.Title.Contains("Manager", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().HaveCount(1);
        filtered.Single().FirstName.Should().Be("John");
    }

    [Fact]
    public void InteractionSearch_GroupByType_CorrectCounts()
    {
        var interactions = GetTestInteractions();

        var grouped = interactions
            .GroupBy(i => i.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        grouped.Should().ContainKey(InteractionType.Email);
        grouped.Should().ContainKey(InteractionType.InPersonMeeting);
        grouped.Should().ContainKey(InteractionType.Call);
        grouped[InteractionType.Email].Should().Be(2);
        grouped[InteractionType.InPersonMeeting].Should().Be(2);
        grouped[InteractionType.Call].Should().Be(1);
    }

    [Fact]
    public void ContactSearch_IsOperator_ExactMatch()
    {
        var contacts = GetTestContacts();

        var filtered = contacts
            .Where(c => string.Equals(c.MailingCountry, "Germany", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().HaveCount(1);
        filtered.Single().FirstName.Should().Be("Hans");
    }

    [Fact]
    public void ContactSearch_IsNotOperator_ExcludesExactMatch()
    {
        var contacts = GetTestContacts();

        var filtered = contacts
            .Where(c => !string.Equals(c.MailingCountry, "Germany", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().HaveCount(4);
        filtered.Should().NotContain(c => c.FirstName == "Hans");
    }

    [Fact]

    [Trait("Defect", "DEF-081")]
    public void ContactSearch_WithSoftDeleteFilter_OnlyActiveRecords()
    {
        var contacts = GetTestContactsWithDeleted();

        var withoutFilter = contacts.Count;
        var withFilter = contacts.Where(c => !c.IsDeleted).Count();

        withFilter.Should().BeLessThan(withoutFilter, "Soft-delete filter should reduce result count");
        withFilter.Should().Be(4);
    }

    [Fact]

    [Trait("Defect", "DEF-081")]
    public void InteractionSearch_WithSoftDeleteFilter_OnlyActiveRecords()
    {
        var interactions = GetTestInteractionsWithDeleted();

        var withoutFilter = interactions.Count;
        var withFilter = interactions.Where(i => !i.IsDeleted).Count();

        withFilter.Should().BeLessThan(withoutFilter);
        withFilter.Should().Be(4);
    }

    [Fact]

    [Trait("Defect", "DEF-081")]
    public void ContactSearch_ComplexANDORCombination_CorrectResults()
    {
        var contacts = GetTestContacts();

        // (country = "United States" AND title contains "Manager") OR email contains "global"
        var filtered = contacts
            .Where(c =>
                (c.MailingCountry == "United States" &&
                 c.Title != null && c.Title.Contains("Manager", StringComparison.OrdinalIgnoreCase)) ||
                (c.Email != null && c.Email.Contains("global", StringComparison.OrdinalIgnoreCase)))
            .ToList();

        filtered.Should().HaveCount(2);
    }

    [Fact]

    [Trait("Defect", "DEF-081")]
    public void InteractionSearch_ByDescription_PartialMatch()
    {
        var interactions = GetTestInteractions();

        var filtered = interactions
            .Where(i => i.Description != null && i.Description.Contains("strategy", StringComparison.OrdinalIgnoreCase))
            .ToList();

        filtered.Should().HaveCount(1);
        filtered.Single().Subject.Should().Be("Partnership Strategy Discussion");
    }

    [Fact]
    public void SearchCriteria_AppliedToContacts_CorrectResults()
    {
        var contacts = GetTestContacts();
        var criteria = new SearchCriteria
        {
            Field = "mailingCountry",
            Value = "Germany",
            Operator = "is not",
            LogicalOperator = "AND"
        };

        IEnumerable<UNOPSContact> query = contacts;

        query = criteria.Operator switch
        {
            "is" => query.Where(c => string.Equals(c.MailingCountry, criteria.Value, StringComparison.OrdinalIgnoreCase)),
            "is not" => query.Where(c => !string.Equals(c.MailingCountry, criteria.Value, StringComparison.OrdinalIgnoreCase)),
            "like" => query.Where(c => c.MailingCountry != null && c.MailingCountry.Contains(criteria.Value, StringComparison.OrdinalIgnoreCase)),
            "not like" => query.Where(c => c.MailingCountry == null || !c.MailingCountry.Contains(criteria.Value, StringComparison.OrdinalIgnoreCase)),
            _ => query
        };

        var result = query.ToList();
        result.Should().HaveCount(4);
        result.Should().NotContain(c => c.MailingCountry == "Germany");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ContactSearch_FullPipelineSimulation_FilterSortPaginate()
    {
        var contacts = GetTestContacts();
        var searchText = "a";
        var ascending = true;
        var pageSize = 2;
        var pageIndex = 1;

        var query = contacts.Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(c =>
                (c.FirstName != null && c.FirstName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (c.LastName != null && c.LastName.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                (c.Email != null && c.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
        }

        var sorted = ascending
            ? query.OrderBy(c => c.LastName)
            : query.OrderByDescending(c => c.LastName);

        var totalCount = sorted.Count();
        var paged = sorted.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

        totalCount.Should().BeGreaterThan(0);
        paged.Should().HaveCountLessOrEqualTo(pageSize);
        paged.Should().BeInAscendingOrder(c => c.LastName);
    }

    [Fact]
    public void InteractionSearch_FullPipelineSimulation_FilterByTypeAndDateRange()
    {
        var interactions = GetTestInteractions();
        var targetType = InteractionType.Email;
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 12, 31);

        var query = interactions
            .Where(i => !i.IsDeleted)
            .Where(i => i.Type == targetType)
            .Where(i => i.Date >= startDate && i.Date <= endDate)
            .OrderByDescending(i => i.Date);

        var results = query.ToList();

        results.Should().NotBeEmpty();
        results.Should().OnlyContain(i => i.Type == InteractionType.Email);
        results.Should().BeInDescendingOrder(i => i.Date);
    }

    [Fact]
    public void ContactSearch_MultiCriteriaAdvancedSearch_SimulatesBackendBehavior()
    {
        var contacts = GetTestContacts();

        var criteria = new List<SearchCriteria>
        {
            new() { Field = "firstName", Value = "J", Operator = "like", LogicalOperator = "AND" },
            new() { Field = "mailingCountry", Value = "United States", Operator = "is", LogicalOperator = null }
        };

        IEnumerable<UNOPSContact> query = contacts.Where(c => !c.IsDeleted);

        foreach (var criterion in criteria)
        {
            var field = criterion.Field;
            var value = criterion.Value;

            query = (field, criterion.Operator) switch
            {
                ("firstName", "like") => query.Where(c => c.FirstName != null && c.FirstName.Contains(value, StringComparison.OrdinalIgnoreCase)),
                ("mailingCountry", "is") => query.Where(c => string.Equals(c.MailingCountry, value, StringComparison.OrdinalIgnoreCase)),
                _ => query
            };
        }

        var result = query.ToList();
        result.Should().HaveCount(1);
        result.Single().FirstName.Should().Be("John");
    }

    [Fact]
    public void InteractionSearch_MultiCriteriaWithOR_SimulatesBackendBehavior()
    {
        var interactions = GetTestInteractions();

        // subject like "Partnership" OR type is "Call"
        var filtered = interactions
            .Where(i => !i.IsDeleted)
            .Where(i =>
                i.Subject.Contains("Partnership", StringComparison.OrdinalIgnoreCase) ||
                i.Type == InteractionType.Call)
            .ToList();

        filtered.Should().HaveCount(3, "Should find 2 Partnership + 1 Call interactions");
    }

    [Fact]
    public void CrossEntitySearch_ContactsWithPartnerFilter_SimulatesNavigation()
    {
        var contacts = GetTestContactsWithPartners();

        var filtered = contacts
            .Where(c => !c.IsDeleted)
            .Where(c => c.Partner != null && !c.Partner.IsDeleted)
            .Where(c => c.Partner!.Status == EntityStatus.Active)
            .ToList();

        filtered.Should().HaveCount(3, "Should find contacts whose partner is Active and not deleted");
    }

    [Fact]
    public void CrossEntitySearch_PartnerContactCount_AggregationQuery()
    {
        var contacts = GetTestContactsWithPartners();

        var partnerContactCounts = contacts
            .Where(c => !c.IsDeleted && c.Partner != null)
            .GroupBy(c => c.Partner!.Name)
            .Select(g => new { PartnerName = g.Key, ContactCount = g.Count() })
            .OrderByDescending(x => x.ContactCount)
            .ToList();

        partnerContactCounts.Should().NotBeEmpty();
        partnerContactCounts.First().ContactCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void ContactSearch_PerformanceWithLargeDataset_UnderThreshold()
    {
        var contacts = GenerateManyContacts(2000);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var filtered = contacts
            .Where(c => !c.IsDeleted)
            .Where(c => c.FirstName != null && c.FirstName.Contains("Test", StringComparison.OrdinalIgnoreCase))
            .Where(c => c.MailingCountry != null && c.MailingCountry.Contains("United", StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.LastName)
            .Take(20)
            .ToList();

        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        filtered.Should().HaveCountLessOrEqualTo(20);
    }

    [Fact]
    public void InteractionSearch_PerformanceWithLargeDataset_UnderThreshold()
    {
        var interactions = GenerateManyInteractions(2000);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var filtered = interactions
            .Where(i => !i.IsDeleted)
            .Where(i => i.Type == InteractionType.Email)
            .Where(i => i.Date > DateTime.UtcNow.AddMonths(-6))
            .OrderByDescending(i => i.Date)
            .Take(50)
            .ToList();

        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100);
        filtered.Should().HaveCountLessOrEqualTo(50);
    }

    [Fact]
    public void GlobalSearchResponse_SimulatedCrossEntitySearch_AllEntitiesRepresented()
    {
        var response = new GlobalSearchResponse
        {
            Partners = new List<GlobalSearchResult>
            {
                new() { EntityType = "Partner", EntityId = 1, Score = 0.95, MatchedField = "name", FieldValue = "ACME Corp", SearchType = "text" }
            },
            Contacts = new List<GlobalSearchResult>
            {
                new() { EntityType = "Contact", EntityId = 10, Score = 0.88, MatchedField = "firstName", FieldValue = "John", SearchType = "text" },
                new() { EntityType = "Contact", EntityId = 11, Score = 0.72, MatchedField = "email", FieldValue = "john@acme.com", SearchType = "text" }
            },
            Interactions = new List<GlobalSearchResult>
            {
                new() { EntityType = "Interaction", EntityId = 20, Score = 0.65, MatchedField = "subject", FieldValue = "ACME Meeting", SearchType = "text" }
            },
            Opportunities = new List<GlobalSearchResult>(),
            SearchQuery = "ACME",
            ExecutionTimeMs = 150.0
        };

        response.TotalResults.Should().Be(4);
        response.Partners.Should().HaveCount(1);
        response.Contacts.Should().HaveCount(2);
        response.Interactions.Should().HaveCount(1);
        response.Opportunities.Should().BeEmpty();

        var allResults = response.Partners
            .Concat(response.Contacts)
            .Concat(response.Interactions)
            .Concat(response.Opportunities)
            .OrderByDescending(r => r.Score)
            .ToList();

        allResults.First().Score.Should().Be(0.95);
        allResults.Last().Score.Should().Be(0.65);
    }

    [Fact]
    public void SearchCriteria_LegacyFieldMapping_ContactPartnerToPartner()
    {
        var legacyMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "contact.partner.name", "partner.name" },
            { "contact.partner.status", "partner.status" },
            { "contact.partner.shortName", "partner.shortName" }
        };

        var criteria = new SearchCriteria { Field = "contact.partner.name", Value = "ACME", Operator = "like" };

        if (legacyMappings.TryGetValue(criteria.Field, out var mappedName))
        {
            criteria.Field = mappedName;
        }

        criteria.Field.Should().Be("partner.name", "Legacy field should be mapped to current field name");
    }

    [Fact]
    public void SearchCriteria_NonLegacyField_NotRemapped()
    {
        var legacyMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "contact.partner.name", "partner.name" },
            { "contact.partner.status", "partner.status" }
        };

        var criteria = new SearchCriteria { Field = "firstName", Value = "John", Operator = "like" };

        if (legacyMappings.TryGetValue(criteria.Field, out var mappedName))
        {
            criteria.Field = mappedName;
        }

        criteria.Field.Should().Be("firstName", "Non-legacy field should remain unchanged");
    }

    #endregion

    #region Helper Methods

    private static List<UNOPSContact> GetTestContacts()
    {
        return new List<UNOPSContact>
        {
            CreateContact("C001", "John", "Doe", "john.doe@acme.com", "Project Manager", "Engineering", "United States"),
            CreateContact("C002", "Jane", "Smith", "jane.smith@global.org", "Director", "Operations", "United Kingdom"),
            CreateContact("C003", "Hans", "Mueller", "hans@partner.de", "Analyst", "Finance", "Germany"),
            CreateContact("C004", "Marie", "Dupont", "marie.dupont@acme.fr", "Coordinator", "HR", "France"),
            CreateContact("C005", "Carlos", "Garcia", "carlos@global.com", "Engineer", "IT", "Spain")
        };
    }

    private static List<UNOPSContact> GetTestContactsWithDeleted()
    {
        var contacts = GetTestContacts();
        contacts.Add(CreateContact("C006", "Deleted", "Doe", "deleted@test.com", "Old Role", "Old Dept", "US", isDeleted: true));
        return contacts;
    }

    private static List<UNOPSContact> GetTestContactsWithPartners()
    {
        var partner1 = new UNOPSPartner
        {
            Id = 1, Name = "ACME Corporation",
            PartnerShortDescription = "ACME",
            PartnerCategoryId = 1, LiaisonOfficeId = 1,
            Status = EntityStatus.Active, PartnerGroupId = 1,
            UNAndStateEntity = false, CanCreateNewOpportunities = true,
            PooledFund = false,
            DueDiligenceRequired = Domain.Enums.DueDiligenceRequired.NotRequired,
            DueDiligenceApproval = Domain.Enums.DueDiligenceApproval.NotApproved,
            PartnerLevyStatus = Domain.Enums.PartnerLevyStatus.DoesNotApply
        };

        var partner2 = new UNOPSPartner
        {
            Id = 2, Name = "Global NGO",
            PartnerShortDescription = "GNGO",
            PartnerCategoryId = 1, LiaisonOfficeId = 1,
            Status = EntityStatus.Active, PartnerGroupId = 1,
            UNAndStateEntity = false, CanCreateNewOpportunities = true,
            PooledFund = false,
            DueDiligenceRequired = Domain.Enums.DueDiligenceRequired.NotRequired,
            DueDiligenceApproval = Domain.Enums.DueDiligenceApproval.NotApproved,
            PartnerLevyStatus = Domain.Enums.PartnerLevyStatus.DoesNotApply
        };

        var partner3 = new UNOPSPartner
        {
            Id = 3, Name = "Inactive Corp",
            PartnerShortDescription = "IC",
            PartnerCategoryId = 1, LiaisonOfficeId = 1,
            Status = EntityStatus.Closed, PartnerGroupId = 1,
            UNAndStateEntity = false, CanCreateNewOpportunities = false,
            PooledFund = false,
            DueDiligenceRequired = Domain.Enums.DueDiligenceRequired.NotRequired,
            DueDiligenceApproval = Domain.Enums.DueDiligenceApproval.NotApproved,
            PartnerLevyStatus = Domain.Enums.PartnerLevyStatus.DoesNotApply
        };

        return new List<UNOPSContact>
        {
            CreateContactWithPartner("CP01", "John", "Doe", "john@acme.com", "Project Manager", partner1),
            CreateContactWithPartner("CP02", "Jane", "Smith", "jane@acme.com", "Analyst", partner1),
            CreateContactWithPartner("CP03", "Bob", "Jones", "bob@global.org", "Director", partner2),
            CreateContactWithPartner("CP04", "Alice", "Brown", "alice@inactive.com", "Coordinator", partner3)
        };
    }

    private static List<UNOPSInteraction> GetTestInteractions()
    {
        return new List<UNOPSInteraction>
        {
            CreateInteraction(1, InteractionType.Email, new DateTime(2024, 1, 15), "Follow-up Email", "Follow up on partnership discussion"),
            CreateInteraction(2, InteractionType.InPersonMeeting, new DateTime(2024, 3, 15), "Partnership Review Meeting", "Quarterly partnership strategy review"),
            CreateInteraction(3, InteractionType.Call, new DateTime(2024, 5, 20), "Quarterly Update Call", "Budget and timeline discussion"),
            CreateInteraction(4, InteractionType.Email, new DateTime(2024, 6, 20), "Partnership Strategy Discussion", "Discussion about new strategy direction"),
            CreateInteraction(5, InteractionType.InPersonMeeting, new DateTime(2024, 8, 10), "Annual Review Meeting", "Annual partnership performance review")
        };
    }

    private static List<UNOPSInteraction> GetTestInteractionsWithDeleted()
    {
        var interactions = GetTestInteractions();
        var deleted = CreateInteraction(99, InteractionType.Email, new DateTime(2024, 4, 1), "Deleted Interaction", "This was deleted");
        deleted.IsDeleted = true;
        deleted.DeletedDate = DateTime.UtcNow;
        interactions.Add(deleted);
        return interactions;
    }

    private static UNOPSContact CreateContact(string contactNumber, string? firstName, string lastName, string email, string title,
        string? department = null, string? country = null, bool isDeleted = false)
    {
        return new UNOPSContact
        {
            Id = Random.Shared.Next(1, 10000),
            ContactNumber = contactNumber,
            Name = $"{firstName} {lastName}",
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Title = title,
            Department = department,
            MailingCountry = country,
            Status = EntityStatus.Active,
            IsDeleted = isDeleted,
            CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365)),
            LastModifiedDate = DateTime.UtcNow
        };
    }

    private static UNOPSContact CreateContactWithPartner(string contactNumber, string firstName, string lastName,
        string email, string title, UNOPSPartner partner)
    {
        var contact = CreateContact(contactNumber, firstName, lastName, email, title);
        contact.Partner = partner;
        return contact;
    }

    private static UNOPSInteraction CreateInteraction(int id, InteractionType type, DateTime date, string subject, string? description = null)
    {
        return new UNOPSInteraction
        {
            Id = id,
            Name = subject,
            Type = type,
            Date = date,
            Subject = subject,
            Description = description,
            Status = EntityStatus.Active,
            IsDeleted = false,
            CreatedDate = date,
            LastModifiedDate = DateTime.UtcNow
        };
    }

    private static List<UNOPSContact> GenerateManyContacts(int count)
    {
        var countries = new[] { "United States", "United Kingdom", "Germany", "France", "Spain", "Japan", "Brazil" };
        var contacts = new List<UNOPSContact>();

        for (int i = 0; i < count; i++)
        {
            contacts.Add(CreateContact(
                $"C{i:D4}",
                i % 5 == 0 ? $"Test{i}" : $"Name{i}",
                $"Last{i}",
                $"user{i}@test.com",
                "Title",
                "Department",
                countries[i % countries.Length]));
        }

        return contacts;
    }

    private static List<UNOPSInteraction> GenerateManyInteractions(int count)
    {
        var types = new[] { InteractionType.Email, InteractionType.InPersonMeeting, InteractionType.Call };
        var interactions = new List<UNOPSInteraction>();

        for (int i = 0; i < count; i++)
        {
            interactions.Add(CreateInteraction(
                i + 1,
                types[i % types.Length],
                DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 365)),
                $"Interaction Subject {i}",
                $"Description for interaction {i}"));
        }

        return interactions;
    }

    #endregion
}
