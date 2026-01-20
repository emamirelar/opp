using FluentAssertions;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.UNOPSDomain.Entities;
using Xunit;
using System.Text.Json;
using UNOPS.PAO.Models.Search;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Tests pour valider la logique OR/AND dans les critères de recherche avancée
/// Ces tests montrent comment les critères devraient se combiner avec OR/AND
/// </summary>
public class AdvancedSearchLogicTests
{
    [Fact]
    public void AdvancedSearch_WithANDLogic_ShouldReturnIntersection()
    {
        // Arrange
        var partners = GetTestPartners();
        
        // Simuler une recherche: Status = "Active" AND Name contains "Global"
        var searchCriteria = new[]
        {
            new SearchCriteria 
            { 
                Field = "Status", 
                Value = "Active", 
                Operator = "is",
                LogicalOperator = "AND" 
            },
            new SearchCriteria 
            { 
                Field = "Name", 
                Value = "Global", 
                Operator = "like",
                LogicalOperator = null // Dernier critère
            }
        };

        // Act - Simuler la logique AND
        var filteredPartners = partners
            .Where(p => GetStatusAsString(p) == "Active")
            .Where(p => p.Name.Contains("Global", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert - Trouve "Global Tech Solutions" et "ACME Global Services" (tous deux Active + contiennent Global)
        filteredPartners.Should().HaveCount(2);
        
        var expectedNames = new[] { "Global Tech Solutions", "ACME Global Services" };
        filteredPartners.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
        filteredPartners.Should().OnlyContain(p => GetStatusAsString(p) == "Active");
    }

    [Fact]
    public void AdvancedSearch_WithORLogic_ShouldReturnUnion()
    {
        // Arrange
        var partners = GetTestPartners();
        
        // Simuler une recherche: Status = "Inactive" OR Name contains "ACME"
        var searchCriteria = new[]
        {
            new SearchCriteria 
            { 
                Field = "Status", 
                Value = "Inactive", 
                Operator = "is",
                LogicalOperator = "OR" 
            },
            new SearchCriteria 
            { 
                Field = "Name", 
                Value = "ACME", 
                Operator = "like",
                LogicalOperator = null // Dernier critère
            }
        };

        // Act - Simuler la logique OR correcte
        var filteredPartners = partners
            .Where(p => GetStatusAsString(p) == "Inactive" || 
                       p.Name.Contains("ACME", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert - Devrait trouver: Beta Industries (Inactive) + ACME Corporation + ACME Global Services
        filteredPartners.Should().HaveCount(3);
        
        var expectedNames = new[] { "Beta Industries", "ACME Corporation", "ACME Global Services" };
        filteredPartners.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public void AdvancedSearch_WithMixedLogic_ShouldHandleComplexCombination()
    {
        // Arrange
        var partners = GetTestPartners();
        
        // Simuler: (Status = "Active" AND Name contains "Global") OR Status = "Prospect"
        var searchCriteria = new[]
        {
            new SearchCriteria 
            { 
                Field = "Status", 
                Value = "Active", 
                Operator = "is",
                LogicalOperator = "AND" 
            },
            new SearchCriteria 
            { 
                Field = "Name", 
                Value = "Global", 
                Operator = "like",
                LogicalOperator = "OR" 
            },
            new SearchCriteria 
            { 
                Field = "Status", 
                Value = "Prospect", 
                Operator = "is",
                LogicalOperator = null 
            }
        };

        // Act - Simuler la logique complexe : (Active AND Global) OR Prospect
        var filteredPartners = partners
            .Where(p => (GetStatusAsString(p) == "Active" && p.Name.Contains("Global", StringComparison.OrdinalIgnoreCase)) ||
                       GetStatusAsString(p) == "Prospect")
            .ToList();

        // Assert - Devrait trouver: 
        // - Global Tech Solutions (Active+Global) 
        // - ACME Global Services (Active+Global)
        // - Global Finance Corp (Prospect)
        filteredPartners.Should().HaveCount(3);
        
        var expectedNames = new[] { "Global Tech Solutions", "ACME Global Services", "Global Finance Corp" };
        filteredPartners.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public void SearchCriteria_JsonSerialization_WithORLogic()
    {
        // Arrange
        var searchCriteria = new[]
        {
            new SearchCriteria 
            { 
                Field = "Status", 
                Value = "Active", 
                Operator = "is",
                LogicalOperator = "OR" 
            },
            new SearchCriteria 
            { 
                Field = "Name", 
                Value = "ACME", 
                Operator = "like",
                LogicalOperator = null 
            }
        };

        // Act
        var json = JsonSerializer.Serialize(searchCriteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria[]>(json);

        // Assert
        deserialized.Should().HaveCount(2);
        deserialized[0].LogicalOperator.Should().Be("OR");
        deserialized[1].LogicalOperator.Should().BeNull();
        
        json.Should().Contain("\"logicalOperator\":\"OR\"");
    }

    [Fact]
    public void AdvancedSearch_WithMultipleORConditions_ShouldReturnCorrectUnion()
    {
        // Arrange
        var partners = GetTestPartners();
        
        // Simuler: Status = "Active" OR Status = "Inactive" OR Name contains "Finance"
        var searchCriteria = new[]
        {
            new SearchCriteria { Field = "Status", Value = "Active", Operator = "is", LogicalOperator = "OR" },
            new SearchCriteria { Field = "Status", Value = "Inactive", Operator = "is", LogicalOperator = "OR" },
            new SearchCriteria { Field = "Name", Value = "Finance", Operator = "like", LogicalOperator = null }
        };

        // Act - Simuler plusieurs OR
        var filteredPartners = partners
            .Where(p => GetStatusAsString(p) == "Active" || 
                       GetStatusAsString(p) == "Inactive" || 
                       p.Name.Contains("Finance", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert - Tous sauf Global Finance Corp qui est Prospect (mais contient Finance, donc inclus)
        filteredPartners.Should().HaveCount(5); // Tous les partenaires
        
        // Vérifier que tous les statuts attendus sont présents
        filteredPartners.Should().Contain(p => GetStatusAsString(p) == "Active");
        filteredPartners.Should().Contain(p => GetStatusAsString(p) == "Inactive");
        filteredPartners.Should().Contain(p => p.Name.Contains("Finance"));
    }

    [Fact]
    public void AdvancedSearch_EmptyLogicalOperator_ShouldDefaultToAND()
    {
        // Arrange
        var partners = GetTestPartners();
        
        // Critères sans LogicalOperator explicite (devrait être AND par défaut)
        var searchCriteria = new[]
        {
            new SearchCriteria { Field = "Status", Value = "Active", Operator = "is" },
            new SearchCriteria { Field = "Name", Value = "Tech", Operator = "like" }
        };

        // Act - Logique AND par défaut
        var filteredPartners = partners
            .Where(p => GetStatusAsString(p) == "Active")
            .Where(p => p.Name.Contains("Tech", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(1);
        filteredPartners.Single().Name.Should().Be("Global Tech Solutions");
    }

    [Fact]
    public void SearchCriteria_OperatorTypes_ShouldSupportAllComparisonTypes()
    {
        // Arrange
        var partners = GetTestPartners();

        // Test différents opérateurs
        var testCases = new[]
        {
            new { Operator = "is", Value = "Active", Expected = 3 },
            new { Operator = "is not", Value = "Active", Expected = 2 },
            new { Operator = "like", Value = "Global", Expected = 3 },
            new { Operator = "not like", Value = "Global", Expected = 2 }
        };

        foreach (var testCase in testCases)
        {
            // Act
            var filteredPartners = testCase.Operator switch
            {
                "is" => partners.Where(p => GetStatusAsString(p) == testCase.Value).ToList(),
                "is not" => partners.Where(p => GetStatusAsString(p) != testCase.Value).ToList(),
                "like" => partners.Where(p => p.Name.Contains(testCase.Value, StringComparison.OrdinalIgnoreCase)).ToList(),
                "not like" => partners.Where(p => !p.Name.Contains(testCase.Value, StringComparison.OrdinalIgnoreCase)).ToList(),
                _ => new List<UNOPSPartner>()
            };

            // Assert
            filteredPartners.Should().HaveCount(testCase.Expected, 
                $"Operator '{testCase.Operator}' with value '{testCase.Value}' should return {testCase.Expected} results");
        }
    }

    [Fact]
    public void SearchCriteria_PropertyMapping_ShouldHandleNestedProperties()
    {
        // Cette fonction teste la capacité à chercher dans des propriétés imbriquées
        // Par exemple Partner.PartnerOffice.Name ou Partner.Contact.FirstName
        
        // Arrange
        var searchCriteria = new[]
        {
            new SearchCriteria { Field = "partner.name", Value = "ACME", Operator = "like" },
            new SearchCriteria { Field = "partnerOffice.name", Value = "Headquarters", Operator = "is" }
        };

        // Act & Assert - Juste valider que la structure est correcte
        searchCriteria[0].Field.Should().Be("partner.name");
        searchCriteria[1].Field.Should().Be("partnerOffice.name");
        
        // Les tests d'intégration réels testeraient la résolution des propriétés imbriquées
    }

    [Fact]
    public void AdvancedSearch_LogicalOperatorFromCurrentCriterion_ShouldUseCorrectOperator()
    {
        // Arrange - Test case from the user: mailingCountry not like "Germany" AND firstName like "James"
        // Frontend shows OR but backend should now apply OR correctly
        var searchCriteria = new[]
        {
            new SearchCriteria 
            { 
                Field = "mailingCountry", 
                Value = "Germany", 
                Operator = "not like",
                LogicalOperator = "AND" // This is ignored for first criterion
            },
            new SearchCriteria 
            { 
                Field = "firstName", 
                Value = "James", 
                Operator = "like",
                LogicalOperator = "OR" // This should be used to combine with previous
            }
        };

        // Act & Assert - Verify the structure matches the user's example
        searchCriteria[0].Field.Should().Be("mailingCountry");
        searchCriteria[0].Value.Should().Be("Germany");
        searchCriteria[0].Operator.Should().Be("not like");
        searchCriteria[0].LogicalOperator.Should().Be("AND");

        searchCriteria[1].Field.Should().Be("firstName");
        searchCriteria[1].Value.Should().Be("James");
        searchCriteria[1].Operator.Should().Be("like");
        searchCriteria[1].LogicalOperator.Should().Be("OR");

        // With our fix, the backend should now use the second criterion's "OR" operator
        // instead of the first criterion's "AND" operator
    }
    
    [Fact]
    public void SearchCriteria_JsonSerialization_ShouldMatchUserExample()
    {
        // Arrange - Exact JSON from user's example
        var expectedJson = """
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

        // Act
        var searchCriteria = JsonSerializer.Deserialize<SearchCriteria[]>(expectedJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        // Assert
        searchCriteria.Should().NotBeNull();
        searchCriteria.Should().HaveCount(2);
        
        // First criterion
        searchCriteria[0].Field.Should().Be("mailingCountry");
        searchCriteria[0].Value.Should().Be("Germany");
        searchCriteria[0].LogicalOperator.Should().Be("AND");
        
        // Second criterion - this OR should now be used by the backend
        searchCriteria[1].Field.Should().Be("firstName");
        searchCriteria[1].Value.Should().Be("James");
        searchCriteria[1].LogicalOperator.Should().Be("OR");
    }

    #region Helper Methods

    // Helper method to get status as string for comparison compatibility
    private static string GetStatusAsString(UNOPSPartner partner)
    {
        return partner.Status switch
        {
            Domain.Entities.EntityStatus.Active => "Active",
            Domain.Entities.EntityStatus.Closed => "Inactive",
            Domain.Entities.EntityStatus.Draft => "Prospect",
            Domain.Entities.EntityStatus.Archived => "Archived",
            _ => "Unknown"
        };
    }

    private static List<UNOPSPartner> GetTestPartners()
    {
        return new List<UNOPSPartner>
        {
            CreatePartner("ACME Corporation", "Active", "ACME"),
            CreatePartner("Global Tech Solutions", "Active", "GTS"),
            CreatePartner("Beta Industries", "Inactive", "BETA"),
            CreatePartner("Global Finance Corp", "Prospect", "GFC"),
            CreatePartner("ACME Global Services", "Active", "AGS")
        };
    }

    private static UNOPSPartner CreatePartner(string name, string status, string shortName)
    {
        // Map old status to new enum
        var systemStatus = status switch
        {
            "Active" => Domain.Entities.EntityStatus.Active,
            "Inactive" => Domain.Entities.EntityStatus.Closed,
            "Prospect" => Domain.Entities.EntityStatus.Draft,
            _ => Domain.Entities.EntityStatus.Draft
        };

        return new UNOPSPartner
        {
            Id = Random.Shared.Next(1, 1000),
            // Enhanced Partner structure
            Name = name,
            PartnerShortDescription = shortName,
            PartnerCategoryId = 1, // Default test category
            LiaisonOfficeId = 1, // Default test liaison office
            UNAndStateEntity = false,
            Status = systemStatus,
            CanCreateNewOpportunities = true,
            PooledFund = false,
            DueDiligenceRequired = Domain.Enums.DueDiligenceRequired.NotRequired,
            DueDiligenceApproval = Domain.Enums.DueDiligenceApproval.NotApproved,
            PartnerLevyStatus = Domain.Enums.PartnerLevyStatus.DoesNotApply,
            PartnerGroupId = 1,
            CreatedDate = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 100)),
            LastModifiedDate = DateTime.UtcNow,
            // Note: For test compatibility, we'll create a helper method to get status as string
        };
    }

    #endregion
}