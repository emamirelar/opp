using FluentAssertions;
using System.Text.Json;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.Domain.DTOs;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Comprehensive tests for search model validation, serialization/deserialization,
/// computed properties, and default values across SearchCriteria, SearchFilter,
/// SearchFieldInfo, GlobalSearchResponse, and AdvancedSearchDTO.
/// </summary>
public class SearchModelValidationTests
{
    #region Positive Tests

    [Fact]
    public void SearchCriteria_DefaultValues_AreCorrectlyInitialized()
    {
        var criteria = new SearchCriteria();

        criteria.Field.Should().Be(string.Empty);
        criteria.Value.Should().Be(string.Empty);
        criteria.Label.Should().Be(string.Empty);
        criteria.Operator.Should().Be("like");
        criteria.LogicalOperator.Should().Be("AND");
        criteria.SecondValue.Should().BeNull();
        criteria.FieldType.Should().BeNull();
    }

    [Fact]
    public void SearchFilter_DefaultValues_AreCorrectlyInitialized()
    {
        var filter = new SearchFilter();

        filter.Field.Should().Be(string.Empty);
        filter.Operator.Should().Be("eq");
        filter.Value.Should().BeNull();
        filter.LogicalOperator.Should().Be("AND");
        filter.FieldType.Should().Be("text");
    }

    [Fact]
    public void GlobalSearchResponse_TotalResults_CalculatesFromAllEntities()
    {
        var response = new GlobalSearchResponse
        {
            Partners = new List<GlobalSearchResult>
            {
                new() { EntityType = "Partner", EntityId = 1, Score = 0.9 },
                new() { EntityType = "Partner", EntityId = 2, Score = 0.8 }
            },
            Contacts = new List<GlobalSearchResult>
            {
                new() { EntityType = "Contact", EntityId = 10, Score = 0.7 }
            },
            Interactions = new List<GlobalSearchResult>
            {
                new() { EntityType = "Interaction", EntityId = 20, Score = 0.6 },
                new() { EntityType = "Interaction", EntityId = 21, Score = 0.5 },
                new() { EntityType = "Interaction", EntityId = 22, Score = 0.4 }
            },
            Opportunities = new List<GlobalSearchResult>
            {
                new() { EntityType = "Opportunity", EntityId = 30, Score = 0.3 }
            },
            SearchQuery = "test query"
        };

        response.TotalResults.Should().Be(7);
    }

    [Fact]
    public void SearchFieldInfo_DefaultValues_AreCorrectlyInitialized()
    {
        var fieldInfo = new SearchFieldInfo();

        fieldInfo.Field.Should().Be(string.Empty);
        fieldInfo.DisplayName.Should().Be(string.Empty);
        fieldInfo.FieldType.Should().Be("text");
        fieldInfo.IsNavigationProperty.Should().BeFalse();
        fieldInfo.NavigationEntity.Should().BeNull();
        fieldInfo.AllowedOperators.Should().BeEquivalentTo(new[] { "eq", "neq", "like" });
        fieldInfo.DropdownOptions.Should().BeNull();
    }

    #endregion

    #region Negative Tests

    [Fact]
    public void SearchCriteria_DeserializeInvalidJson_ThrowsJsonException()
    {
        var invalidJson = "{ this is not valid json }";

        var act = () => JsonSerializer.Deserialize<SearchCriteria>(invalidJson);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void SearchCriteria_DeserializeEmptyString_ThrowsJsonException()
    {
        var act = () => JsonSerializer.Deserialize<SearchCriteria>("");

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void SearchCriteria_DeserializeNull_ThrowsArgumentNullException()
    {
        var act = () => JsonSerializer.Deserialize<SearchCriteria>((string)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SearchCriteria_DeserializeEmptyArray_ReturnsEmptyList()
    {
        var json = "[]";
        var result = JsonSerializer.Deserialize<List<SearchCriteria>>(json);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GlobalSearchResponse_NullPartners_TotalResultsTreatsAsZero()
    {
        var response = new GlobalSearchResponse
        {
            Partners = null!,
            Contacts = new List<GlobalSearchResult> { new() { EntityId = 1 } },
            Interactions = null!,
            Opportunities = null!
        };

        response.TotalResults.Should().Be(1);
    }

    [Fact]
    public void GlobalSearchResponse_AllNullCollections_TotalResultsIsZero()
    {
        var response = new GlobalSearchResponse
        {
            Partners = null!,
            Contacts = null!,
            Interactions = null!,
            Opportunities = null!
        };

        response.TotalResults.Should().Be(0);
    }

    [Fact]
    public void SearchCriteria_DeserializeMissingFields_DefaultsApplied()
    {
        var json = """{"field":"name"}""";
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var result = JsonSerializer.Deserialize<SearchCriteria>(json, options);

        result.Should().NotBeNull();
        result!.Field.Should().Be("name");
        result.Value.Should().Be(string.Empty);
        result.Operator.Should().Be("like");
    }

    [Fact]
    public void SearchCriteria_DeserializeUnknownOperator_StoredWithoutValidation()
    {
        var json = """{"field":"name","value":"test","operator":"INVALID_OP"}""";
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var result = JsonSerializer.Deserialize<SearchCriteria>(json, options);

        result.Should().NotBeNull();
        result!.Operator.Should().Be("INVALID_OP");
    }

    [Fact]
    public void SearchCriteria_DeserializeNullLogicalOperator_AcceptsNull()
    {
        var json = """{"field":"name","value":"test","operator":"is","logicalOperator":null}""";

        var result = JsonSerializer.Deserialize<SearchCriteria>(json);

        result.Should().NotBeNull();
        result!.LogicalOperator.Should().BeNull();
    }

    [Fact]
    public void SearchFilter_NullValue_StoredWithoutError()
    {
        var filter = new SearchFilter
        {
            Field = "name",
            Operator = "eq",
            Value = null
        };

        filter.Value.Should().BeNull();
    }

    [Fact]
    public void GlobalSearchResult_EmptyEntityType_DefaultsToEmptyString()
    {
        var result = new GlobalSearchResult();

        result.EntityType.Should().Be(string.Empty);
        result.MatchedField.Should().Be(string.Empty);
        result.FieldValue.Should().Be(string.Empty);
        result.SearchType.Should().Be(string.Empty);
        result.MatchCriteria.Should().Be(string.Empty);
        result.Snippet.Should().Be(string.Empty);
        result.EntityId.Should().Be(0);
        result.Score.Should().Be(0.0);
    }

    [Fact]
    public void AdvancedSearchDTO_NullCriteria_IsValid()
    {
        var dto = new AdvancedSearchDTO
        {
            GeneralSearch = "test",
            Criteria = null,
            PageIndex = 1,
            PageSize = 10
        };

        dto.Criteria.Should().BeNull();
        dto.GeneralSearch.Should().Be("test");
    }

    #endregion

    #region Edge/Boundary Tests

    [Fact]
    public void SearchCriteria_UnicodeInField_PreservedThroughSerialization()
    {
        var criteria = new SearchCriteria
        {
            Field = "名前",
            Value = "テスト値",
            Operator = "like",
            Label = "名前フィールド"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Field.Should().Be("名前");
        deserialized.Value.Should().Be("テスト値");
        deserialized.Label.Should().Be("名前フィールド");
    }

    [Fact]
    public void SearchCriteria_SpecialJsonCharacters_SerializedCorrectly()
    {
        var criteria = new SearchCriteria
        {
            Field = "description",
            Value = "He said \"hello\" & she said 'goodbye' \\ newline\nnewline",
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().Be(criteria.Value);
    }

    [Fact]
    public void SearchCriteria_VeryLongFieldValue_PreservedThroughSerialization()
    {
        var longValue = new string('A', 10000);
        var criteria = new SearchCriteria
        {
            Field = "description",
            Value = longValue,
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().HaveLength(10000);
        deserialized.Value.Should().Be(longValue);
    }

    [Fact]
    public void SearchCriteria_EmptyStringValue_DiffersFromNull()
    {
        var withEmpty = new SearchCriteria { Value = "" };
        var withNull = new SearchCriteria { SecondValue = null };

        withEmpty.Value.Should().BeEmpty();
        withNull.SecondValue.Should().BeNull();
        withEmpty.Value.Should().NotBeNull();
    }

    [Fact]
    public void GlobalSearchResponse_LargeResultSets_TotalResultsAccurate()
    {
        var response = new GlobalSearchResponse
        {
            Partners = Enumerable.Range(1, 500).Select(i => new GlobalSearchResult { EntityId = i }).ToList(),
            Contacts = Enumerable.Range(1, 300).Select(i => new GlobalSearchResult { EntityId = i }).ToList(),
            Interactions = Enumerable.Range(1, 200).Select(i => new GlobalSearchResult { EntityId = i }).ToList(),
            Opportunities = Enumerable.Range(1, 1000).Select(i => new GlobalSearchResult { EntityId = i }).ToList()
        };

        response.TotalResults.Should().Be(2000);
    }

    [Fact]
    public void GlobalSearchResult_NegativeScore_IsStorable()
    {
        var result = new GlobalSearchResult { Score = -1.5 };
        result.Score.Should().Be(-1.5);
    }

    [Fact]
    public void GlobalSearchResult_ZeroScore_IsValid()
    {
        var result = new GlobalSearchResult { Score = 0.0 };
        result.Score.Should().Be(0.0);
    }

    [Fact]
    public void GlobalSearchResult_MaxScore_IsStorable()
    {
        var result = new GlobalSearchResult { Score = double.MaxValue };
        result.Score.Should().Be(double.MaxValue);
    }

    [Fact]
    public void SearchCriteria_AllOperatorTypes_StoredCorrectly()
    {
        var operators = new[] { "is", "is not", "like", "not like", ">", "<", ">=", "<=", "after", "before", "between" };

        foreach (var op in operators)
        {
            var criteria = new SearchCriteria { Operator = op };
            criteria.Operator.Should().Be(op, $"Operator '{op}' should be stored as-is");
        }
    }

    [Fact]
    public void SearchFieldInfo_NavigationProperty_FlagAndEntityStored()
    {
        var field = new SearchFieldInfo
        {
            Field = "partner.name",
            DisplayName = "Partner Name",
            IsNavigationProperty = true,
            NavigationEntity = "Partner"
        };

        field.IsNavigationProperty.Should().BeTrue();
        field.NavigationEntity.Should().Be("Partner");
    }

    [Fact]
    public void SearchCriteria_BetweenOperator_WithSecondValue_BothValuesPreserved()
    {
        var criteria = new SearchCriteria
        {
            Field = "createdDate",
            Value = "2024-01-01",
            SecondValue = "2024-12-31",
            Operator = "between",
            FieldType = "date"
        };

        criteria.Value.Should().Be("2024-01-01");
        criteria.SecondValue.Should().Be("2024-12-31");
        criteria.Operator.Should().Be("between");
    }

    [Fact]
    public void SearchFieldInfo_EmptyAllowedOperators_AcceptedButEmptyList()
    {
        var field = new SearchFieldInfo
        {
            AllowedOperators = new List<string>()
        };

        field.AllowedOperators.Should().BeEmpty();
    }

    #endregion

    #region Functional Tests

    [Fact]
    public void SearchCriteria_JsonRoundTrip_PreservesAllProperties()
    {
        var original = new SearchCriteria
        {
            Field = "mailingCountry",
            Value = "Germany",
            Label = "Country",
            Operator = "not like",
            LogicalOperator = "OR",
            SecondValue = "France",
            FieldType = "text"
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Field.Should().Be("mailingCountry");
        deserialized.Value.Should().Be("Germany");
        deserialized.Label.Should().Be("Country");
        deserialized.Operator.Should().Be("not like");
        deserialized.LogicalOperator.Should().Be("OR");
        deserialized.SecondValue.Should().Be("France");
        deserialized.FieldType.Should().Be("text");
    }

    [Fact]
    public void SearchFilter_AllProperties_SetAndRetrievedCorrectly()
    {
        var filter = new SearchFilter
        {
            Field = "contacts.firstName",
            Operator = "like",
            Value = "John",
            LogicalOperator = "OR",
            FieldType = "text"
        };

        filter.Field.Should().Be("contacts.firstName");
        filter.Operator.Should().Be("like");
        filter.Value.Should().Be("John");
        filter.LogicalOperator.Should().Be("OR");
        filter.FieldType.Should().Be("text");
    }

    [Fact]
    public void SearchFieldInfo_WithDropdownOptions_PopulatedCorrectly()
    {
        var field = new SearchFieldInfo
        {
            Field = "status",
            DisplayName = "Status",
            FieldType = "dropdown",
            AllowedOperators = new List<string> { "eq", "neq" },
            DropdownOptions = new List<DropdownOption>
            {
                new() { Value = "Active", Label = "Active" },
                new() { Value = "Inactive", Label = "Inactive" },
                new() { Value = "Draft", Label = "Draft" }
            }
        };

        field.DropdownOptions.Should().HaveCount(3);
        field.DropdownOptions![0].Value.Should().Be("Active");
        field.DropdownOptions[0].Label.Should().Be("Active");
        field.DropdownOptions[2].Value.Should().Be("Draft");
    }

    [Fact]
    public void GlobalSearchResponse_MultiEntityResults_ExecutionTimeTracked()
    {
        var response = new GlobalSearchResponse
        {
            Partners = new List<GlobalSearchResult> { new() { EntityId = 1, Score = 0.95 } },
            Contacts = new List<GlobalSearchResult> { new() { EntityId = 2, Score = 0.88 } },
            Interactions = new List<GlobalSearchResult>(),
            Opportunities = new List<GlobalSearchResult>(),
            SearchQuery = "test partner",
            ExecutionTimeMs = 145.7
        };

        response.TotalResults.Should().Be(2);
        response.ExecutionTimeMs.Should().Be(145.7);
        response.SearchQuery.Should().Be("test partner");
    }

    [Fact]
    public void SearchCriteria_LogicalOperators_ANDandOR_CorrectlyCombine()
    {
        var criteria = new[]
        {
            new SearchCriteria { Field = "status", Value = "Active", Operator = "is", LogicalOperator = "AND" },
            new SearchCriteria { Field = "name", Value = "ACME", Operator = "like", LogicalOperator = "OR" },
            new SearchCriteria { Field = "country", Value = "US", Operator = "is", LogicalOperator = null }
        };

        criteria[0].LogicalOperator.Should().Be("AND");
        criteria[1].LogicalOperator.Should().Be("OR");
        criteria[2].LogicalOperator.Should().BeNull();
    }

    [Fact]
    public void SearchFieldInfo_TextFieldType_HasCorrectDefaultOperators()
    {
        var textField = new SearchFieldInfo
        {
            Field = "name",
            FieldType = "text",
            AllowedOperators = new List<string> { "eq", "neq", "like", "not like" }
        };

        textField.AllowedOperators.Should().Contain("like");
        textField.AllowedOperators.Should().Contain("not like");
        textField.AllowedOperators.Should().Contain("eq");
    }

    [Fact]
    public void SearchFieldInfo_DateFieldType_HasCorrectOperators()
    {
        var dateField = new SearchFieldInfo
        {
            Field = "createdDate",
            FieldType = "date",
            AllowedOperators = new List<string> { "eq", "neq", "gt", "lt", "gte", "lte" }
        };

        dateField.AllowedOperators.Should().Contain("gt");
        dateField.AllowedOperators.Should().Contain("lt");
        dateField.FieldType.Should().Be("date");
    }

    [Fact]
    public void AdvancedSearchDTO_AllProperties_SetCorrectly()
    {
        var dto = new AdvancedSearchDTO
        {
            GeneralSearch = "global query",
            Criteria = new List<SearchCriterionDTO>
            {
                new() { Field = "name", Value = "test", Operator = "like", LogicalOperator = "AND" },
                new() { Field = "status", Value = "Active", Operator = "is", LogicalOperator = null }
            },
            PageIndex = 2,
            PageSize = 25,
            SortField = "name",
            SortOrder = "asc"
        };

        dto.GeneralSearch.Should().Be("global query");
        dto.Criteria.Should().HaveCount(2);
        dto.PageIndex.Should().Be(2);
        dto.PageSize.Should().Be(25);
        dto.SortField.Should().Be("name");
        dto.SortOrder.Should().Be("asc");
    }

    [Fact]
    public void GlobalSearchResponse_EmptyCollections_TotalResultsIsZero()
    {
        var response = new GlobalSearchResponse();

        response.TotalResults.Should().Be(0);
        response.Partners.Should().BeEmpty();
        response.Contacts.Should().BeEmpty();
        response.Interactions.Should().BeEmpty();
        response.Opportunities.Should().BeEmpty();
        response.SearchQuery.Should().Be("");
    }

    [Fact]
    public void SearchCriterionDTO_RequiredFieldsSet_WorksCorrectly()
    {
        var criterion = new SearchCriterionDTO
        {
            Field = "firstName",
            Value = "John",
            Operator = "like",
            LogicalOperator = "AND"
        };

        criterion.Field.Should().Be("firstName");
        criterion.Value.Should().Be("John");
        criterion.Operator.Should().Be("like");
        criterion.LogicalOperator.Should().Be("AND");
    }

    [Fact]
    public void DropdownOption_DefaultValues_AreEmptyStrings()
    {
        var option = new DropdownOption();

        option.Value.Should().Be(string.Empty);
        option.Label.Should().Be(string.Empty);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void SearchCriteria_FrontendJsonFormat_DeserializesCorrectly()
    {
        var frontendJson = """
        [
            {
                "field":"mailingCountry",
                "value":"Germany",
                "label":"Country",
                "operator":"not like",
                "logicalOperator":"AND",
                "fieldType":"text"
            },
            {
                "field":"firstName",
                "value":"James",
                "label":"First Name",
                "operator":"like",
                "logicalOperator":"OR",
                "fieldType":"text"
            }
        ]
        """;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var criteria = JsonSerializer.Deserialize<List<SearchCriteria>>(frontendJson, options);

        criteria.Should().NotBeNull();
        criteria.Should().HaveCount(2);

        criteria![0].Field.Should().Be("mailingCountry");
        criteria[0].Value.Should().Be("Germany");
        criteria[0].Label.Should().Be("Country");
        criteria[0].Operator.Should().Be("not like");
        criteria[0].LogicalOperator.Should().Be("AND");
        criteria[0].FieldType.Should().Be("text");

        criteria[1].Field.Should().Be("firstName");
        criteria[1].Value.Should().Be("James");
        criteria[1].Operator.Should().Be("like");
        criteria[1].LogicalOperator.Should().Be("OR");
    }

    [Fact]
    public void SearchCriteria_DateRangeQuery_DeserializesCorrectly()
    {
        var dateJson = """
        [
            {
                "field":"createdDate",
                "value":"2024-01-01",
                "secondValue":"2024-12-31",
                "label":"Created Date",
                "operator":"between",
                "logicalOperator":"AND",
                "fieldType":"date"
            }
        ]
        """;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var criteria = JsonSerializer.Deserialize<List<SearchCriteria>>(dateJson, options);

        criteria.Should().HaveCount(1);
        criteria![0].Operator.Should().Be("between");
        criteria[0].Value.Should().Be("2024-01-01");
        criteria[0].SecondValue.Should().Be("2024-12-31");
        criteria[0].FieldType.Should().Be("date");
    }

    [Fact]
    public void SearchCriteria_ComplexMultiCriteriaQuery_DeserializesCorrectly()
    {
        var complexJson = """
        [
            {"field":"status","value":"Active","operator":"is","logicalOperator":"AND","fieldType":"text"},
            {"field":"name","value":"Global","operator":"like","logicalOperator":"OR","fieldType":"text"},
            {"field":"createdDate","value":"2024-01-01","operator":"after","logicalOperator":"AND","fieldType":"date"},
            {"field":"partnerGroupId","value":"5","operator":"is","logicalOperator":null,"fieldType":"number"}
        ]
        """;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var criteria = JsonSerializer.Deserialize<List<SearchCriteria>>(complexJson, options);

        criteria.Should().HaveCount(4);
        criteria![0].LogicalOperator.Should().Be("AND");
        criteria[1].LogicalOperator.Should().Be("OR");
        criteria[2].LogicalOperator.Should().Be("AND");
        criteria[3].LogicalOperator.Should().BeNull();
        criteria[2].FieldType.Should().Be("date");
        criteria[3].FieldType.Should().Be("number");
    }

    [Fact]
    public void GlobalSearchResponse_FullResponse_SerializesAndDeserializesCorrectly()
    {
        var original = new GlobalSearchResponse
        {
            Partners = new List<GlobalSearchResult>
            {
                new() { EntityType = "Partner", EntityId = 1, Score = 0.95, MatchedField = "name", FieldValue = "ACME Corp", SearchType = "text", MatchCriteria = "exact", Snippet = "...ACME Corp..." }
            },
            Contacts = new List<GlobalSearchResult>
            {
                new() { EntityType = "Contact", EntityId = 10, Score = 0.85, MatchedField = "firstName", FieldValue = "John", SearchType = "similarity", MatchCriteria = "fuzzy", Snippet = "John Doe" }
            },
            Interactions = new List<GlobalSearchResult>(),
            Opportunities = new List<GlobalSearchResult>(),
            SearchQuery = "ACME",
            ExecutionTimeMs = 42.5
        };

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<GlobalSearchResponse>(json);

        deserialized.Should().NotBeNull();
        deserialized!.TotalResults.Should().Be(2);
        deserialized.Partners.Should().HaveCount(1);
        deserialized.Contacts.Should().HaveCount(1);
        deserialized.Partners[0].Score.Should().Be(0.95);
        deserialized.Partners[0].Snippet.Should().Be("...ACME Corp...");
        deserialized.Contacts[0].SearchType.Should().Be("similarity");
    }

    [Fact]
    public void SearchCriteria_NavigationFieldPaths_DeserializedCorrectly()
    {
        var navJson = """
        [
            {"field":"partner.name","value":"ACME","operator":"like","fieldType":"text"},
            {"field":"contacts.firstName","value":"John","operator":"is","fieldType":"text"},
            {"field":"partner.partnerGroup.name","value":"NGO","operator":"is","fieldType":"text"},
            {"field":"organizationUnitRelationships.organizationHierarchy.name","value":"HQ","operator":"like","fieldType":"text"}
        ]
        """;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var criteria = JsonSerializer.Deserialize<List<SearchCriteria>>(navJson, options);

        criteria.Should().HaveCount(4);
        criteria![0].Field.Should().Be("partner.name");
        criteria[1].Field.Should().Be("contacts.firstName");
        criteria[2].Field.Should().Be("partner.partnerGroup.name");
        criteria[3].Field.Should().Be("organizationUnitRelationships.organizationHierarchy.name");
    }

    [Fact]
    public void SearchCriteria_RealWorldPartnerSearchScenario_FullRoundTrip()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "ACME", Operator = "like", LogicalOperator = "AND", FieldType = "text" },
            new() { Field = "status", Value = "Active", Operator = "is", LogicalOperator = "AND", FieldType = "text" },
            new() { Field = "partnerCategoryId", Value = "3", Operator = "is", LogicalOperator = null, FieldType = "number" }
        };

        var json = JsonSerializer.Serialize(criteria);
        var roundTripped = JsonSerializer.Deserialize<List<SearchCriteria>>(json);

        roundTripped.Should().HaveCount(3);
        roundTripped![0].Field.Should().Be("name");
        roundTripped[1].Value.Should().Be("Active");
        roundTripped[2].FieldType.Should().Be("number");
    }

    [Fact]
    public void SearchCriteria_RealWorldContactSearchScenario_FullRoundTrip()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "firstName", Value = "John", Operator = "like", LogicalOperator = "OR", FieldType = "text" },
            new() { Field = "lastName", Value = "Doe", Operator = "like", LogicalOperator = "AND", FieldType = "text" },
            new() { Field = "partner.name", Value = "ACME", Operator = "is", LogicalOperator = null, FieldType = "text" }
        };

        var json = JsonSerializer.Serialize(criteria);
        var roundTripped = JsonSerializer.Deserialize<List<SearchCriteria>>(json);

        roundTripped.Should().HaveCount(3);
        roundTripped![0].LogicalOperator.Should().Be("OR");
        roundTripped[2].Field.Should().Be("partner.name");
    }

    [Fact]
    public void SearchCriteria_RealWorldInteractionSearchScenario_FullRoundTrip()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "subject", Value = "Meeting", Operator = "like", LogicalOperator = "AND", FieldType = "text" },
            new() { Field = "date", Value = "2024-06-01", SecondValue = "2024-06-30", Operator = "between", LogicalOperator = "AND", FieldType = "date" },
            new() { Field = "contact.firstName", Value = "James", Operator = "like", LogicalOperator = null, FieldType = "text" }
        };

        var json = JsonSerializer.Serialize(criteria);
        var roundTripped = JsonSerializer.Deserialize<List<SearchCriteria>>(json);

        roundTripped.Should().HaveCount(3);
        roundTripped![1].Operator.Should().Be("between");
        roundTripped[1].SecondValue.Should().Be("2024-06-30");
        roundTripped[2].Field.Should().Be("contact.firstName");
    }

    [Fact]
    public void AdvancedSearchDTO_CompleteRequest_SerializesCorrectly()
    {
        var dto = new AdvancedSearchDTO
        {
            GeneralSearch = "global search text",
            Criteria = new List<SearchCriterionDTO>
            {
                new() { Field = "name", Value = "Test", Operator = "like", LogicalOperator = "AND" },
                new() { Field = "status", Value = "Active", Operator = "is" }
            },
            PageIndex = 1,
            PageSize = 20,
            SortField = "createdDate",
            SortOrder = "desc"
        };

        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<AdvancedSearchDTO>(json);

        deserialized.Should().NotBeNull();
        deserialized!.GeneralSearch.Should().Be("global search text");
        deserialized.Criteria.Should().HaveCount(2);
        deserialized.PageIndex.Should().Be(1);
        deserialized.PageSize.Should().Be(20);
        deserialized.SortField.Should().Be("createdDate");
        deserialized.SortOrder.Should().Be("desc");
    }

    [Fact]
    public void SearchCriteria_URLEncodedJson_CanBeDecoded()
    {
        var originalJson = """[{"field":"name","value":"ACME Corp","operator":"like"}]""";
        var encoded = System.Net.WebUtility.UrlEncode(originalJson);
        var decoded = System.Net.WebUtility.UrlDecode(encoded);

        decoded.Should().Be(originalJson);

        var criteria = JsonSerializer.Deserialize<List<SearchCriteria>>(decoded);
        criteria.Should().HaveCount(1);
        criteria![0].Value.Should().Be("ACME Corp");
    }

    [Fact]
    public void SearchFieldInfo_CompleteConfiguration_AllFieldsPopulated()
    {
        var field = new SearchFieldInfo
        {
            Field = "status",
            DisplayName = "Partner Status",
            FieldType = "dropdown",
            IsNavigationProperty = false,
            NavigationEntity = null,
            AllowedOperators = new List<string> { "eq", "neq", "in" },
            DropdownOptions = new List<DropdownOption>
            {
                new() { Value = "Active", Label = "status.active" },
                new() { Value = "Inactive", Label = "status.inactive" },
                new() { Value = "Draft", Label = "status.draft" },
                new() { Value = "Archived", Label = "status.archived" }
            }
        };

        var json = JsonSerializer.Serialize(field);
        var deserialized = JsonSerializer.Deserialize<SearchFieldInfo>(json);

        deserialized.Should().NotBeNull();
        deserialized!.DropdownOptions.Should().HaveCount(4);
        deserialized.AllowedOperators.Should().HaveCount(3);
        deserialized.FieldType.Should().Be("dropdown");
    }

    #endregion
}
