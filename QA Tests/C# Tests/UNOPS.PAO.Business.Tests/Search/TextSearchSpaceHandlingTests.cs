using FluentAssertions;
using UNOPS.PAO.IntegrationTests.TestData;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Models;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Tests pour valider la gestion améliorée des espaces dans la recherche textuelle
/// Ces tests montrent les problèmes actuels et les solutions attendues
/// </summary>
public class TextSearchSpaceHandlingTests
{
    [Theory]
    [InlineData("  Global  ", "Global")] // Espaces au début et à la fin
    [InlineData("Global Tech", "Global Tech")] // Espaces normaux
    [InlineData("  Global   Tech  ", "Global Tech")] // Espaces multiples
    [InlineData("Global\t\tTech", "Global Tech")] // Tabulations
    [InlineData("Global\n\rTech", "Global Tech")] // Caractères de nouvelle ligne
    [InlineData("  \t  ", "")] // Seulement des espaces
    public void SearchText_Normalization_ShouldCleanupSpaces(string input, string expected)
    {
        // Act
        var normalized = NormalizeSearchText(input);

        // Assert
        normalized.Should().Be(expected);
    }

    [Fact]
    public void PartnerSearch_WithExtraSpaces_ShouldFindCorrectResults()
    {
        // Arrange
        var partners = GetTestPartners();
        var searchTextWithSpaces = "  Global   Tech  "; // Espaces en trop

        // Act - Simuler une recherche avec normalisation
        var normalizedSearch = NormalizeSearchText(searchTextWithSpaces);
        var filteredPartners = partners
            .Where(p => p.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(1);
        filteredPartners.Single().Name.Should().Be("Global Tech Solutions");
    }

    [Fact]
    public void PartnerSearch_WithMultipleWords_ShouldFindPartialMatches()
    {
        // Arrange
        var partners = GetTestPartners();
        var searchText = "ACME Global"; // Deux mots qui peuvent apparaître dans des noms différents

        // Act - Recherche actuelle (un seul terme)
        var currentSearch = partners
            .Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Act - Recherche améliorée (mots séparés avec OR)
        var words = SplitSearchTerms(searchText);
        var improvedSearch = partners
            .Where(p => words.Any(word => p.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Assert
        currentSearch.Should().HaveCount(1, "Recherche actuelle trouve seulement 'ACME Global Services'");
        improvedSearch.Should().HaveCount(4, "Recherche améliorée trouve tous les partenaires contenant 'ACME' ou 'Global'");
        
        var expectedNames = new[] { "ACME Corporation", "ACME Global Services", "Global Tech Solutions", "Global Finance Corp" };
        improvedSearch.Select(p => p.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public void PartnerSearch_WithMultipleWordsAND_ShouldFindExactMatches()
    {
        // Arrange
        var partners = GetTestPartners();
        var searchText = "ACME Global"; // Recherche AND : doit contenir ACME ET Global

        // Act - Recherche AND (tous les mots doivent être présents)
        var words = SplitSearchTerms(searchText);
        var andSearch = partners
            .Where(p => words.All(word => p.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Assert
        andSearch.Should().HaveCount(1);
        andSearch.Single().Name.Should().Be("ACME Global Services");
    }

    [Theory]
    [InlineData("")] // Chaîne vide
    [InlineData("   ")] // Seulement des espaces
    [InlineData("\t\n\r")] // Seulement des caractères blancs
    [InlineData(null)] // Null
    public void PartnerSearch_WithEmptyOrWhitespace_ShouldReturnAll(string? searchText)
    {
        // Arrange
        var partners = GetTestPartners();

        // Act - Une recherche vide devrait retourner tous les résultats
        var filteredPartners = partners
            .Where(p => string.IsNullOrWhiteSpace(searchText) || 
                       p.Name.Contains(NormalizeSearchText(searchText), StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(5, "Une recherche vide doit retourner tous les partenaires");
    }

    [Fact]
    public void PartnerSearch_WithSpecialCharacters_ShouldHandleGracefully()
    {
        // Arrange
        var partners = GetTestPartners();
        var searchTexts = new[]
        {
            "Global & Tech", // Avec &
            "ACME (Corporation)", // Avec parenthèses
            "Beta-Industries", // Avec tiret
            "Global.Tech", // Avec point
            "Tech@Solutions" // Avec @
        };

        foreach (var searchText in searchTexts)
        {
            // Act - Ne devrait pas lever d'exception
            var normalized = NormalizeSearchText(searchText);
            
            var filteredPartners = partners
                .Where(p => p.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Assert - Devrait fonctionner sans erreur
            filteredPartners.Should().NotBeNull();
        }
    }

    [Fact]
    public void PartnerSearch_CaseInsensitive_ShouldWork()
    {
        // Arrange
        var partners = GetTestPartners();
        var searchVariations = new[]
        {
            "GLOBAL",
            "global", 
            "Global",
            "gLoBaL"
        };

        foreach (var searchText in searchVariations)
        {
            // Act
            var filteredPartners = partners
                .Where(p => p.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Assert - Toutes les variations devraient donner le même résultat
            filteredPartners.Should().HaveCount(3, $"Search '{searchText}' should find 3 partners");
        }
    }

    [Fact]
    public void PartnerSearch_WithQuotes_ShouldSearchExactPhrase()
    {
        // Arrange
        var partners = GetTestPartners();
        var exactPhrase = "\"Tech Solutions\""; // Recherche de phrase exacte

        // Act - Simuler recherche de phrase exacte
        var cleanPhrase = exactPhrase.Trim('"');
        var filteredPartners = partners
            .Where(p => p.Name.Contains(cleanPhrase, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert
        filteredPartners.Should().HaveCount(1);
        filteredPartners.Single().Name.Should().Be("Global Tech Solutions");
    }

    [Fact]
    public void PartnerSearch_WithMinimumLength_ShouldIgnoreShortTerms()
    {
        // Arrange
        var partners = GetTestPartners();
        var searchText = "A B Global"; // 'A' et 'B' sont trop courts

        // Act - Filtrer les termes trop courts (< 2 caractères)
        var words = SplitSearchTerms(searchText)
            .Where(word => word.Length >= 2)
            .ToArray();

        var filteredPartners = partners
            .Where(p => words.Any(word => p.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        // Assert
        words.Should().ContainSingle("Global", "Seul 'Global' devrait rester après filtrage");
        filteredPartners.Should().HaveCount(3, "Devrait trouver tous les partenaires contenant 'Global'");
    }

    [Fact]
    public void PartnerSearch_Performance_WithManySpaces()
    {
        // Arrange
        var partners = GenerateManyPartners(1000);
        var searchTextWithManySpaces = "    Global    Tech    Solutions    ";

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var normalizedSearch = NormalizeSearchText(searchTextWithManySpaces);
        var words = SplitSearchTerms(normalizedSearch);
        
        var filteredPartners = partners
            .Where(p => words.Any(word => p.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
            .Take(10)
            .ToList();
            
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100, "La normalisation ne devrait pas impacter significativement les performances");
        filteredPartners.Should().HaveCountLessOrEqualTo(10);
    }

    [Theory]
    [InlineData("global", "Global Tech Solutions")]
    [InlineData("GLOBAL", "Global Tech Solutions")]
    [InlineData("GlObAl", "Global Tech Solutions")]
    [InlineData("acme", "ACME Corporation")]
    [InlineData("ACME", "ACME Corporation")]
    [InlineData("AcMe", "ACME Corporation")]
    [InlineData("tech", "Global Tech Solutions")]
    [InlineData("TECH", "Global Tech Solutions")]
    [InlineData("TeCh", "Global Tech Solutions")]
    public void PartnerSearch_CaseInsensitive_ShouldFindMatches(string searchTerm, string expectedPartnerName)
    {
        // Arrange
        var partners = GetTestPartners();

        // Act - Recherche insensible à la casse
        var filteredPartners = partners
            .Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert
        filteredPartners.Should().NotBeEmpty($"Search for '{searchTerm}' should find at least one partner");
        filteredPartners.Should().Contain(p => p.Name == expectedPartnerName, 
            $"Search for '{searchTerm}' should find '{expectedPartnerName}'");
    }

    [Fact]
    public void PartnerSearch_CaseInsensitive_MultipleWords_ShouldWork()
    {
        // Arrange
        var partners = GetTestPartners();
        var searchVariations = new[]
        {
            "global tech",
            "GLOBAL TECH", 
            "Global Tech",
            "gLoBaL tEcH"
        };

        foreach (var searchText in searchVariations)
        {
            // Act - Recherche par mots multiples, insensible à la casse
            var words = SplitSearchTerms(searchText);
            var filteredPartners = partners
                .Where(p => words.Any(word => p.Name.Contains(word, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Assert
            filteredPartners.Should().Contain(p => p.Name == "Global Tech Solutions", 
                $"Search '{searchText}' should find 'Global Tech Solutions'");
        }
    }

    [Theory]
    [InlineData("Active", "is", 3)] // Toutes les casses de "Active"
    [InlineData("active", "is", 3)]
    [InlineData("ACTIVE", "is", 3)]
    [InlineData("AcTiVe", "is", 3)]
    [InlineData("Inactive", "is not", 4)] // Tout sauf "Inactive"
    [InlineData("inactive", "is not", 4)]
    [InlineData("INACTIVE", "is not", 4)]
    public void PartnerSearch_CaseInsensitive_ExactMatch_ShouldWork(string searchValue, string operatorType, int expectedCount)
    {
        // Arrange
        var partners = GetTestPartners();

        // Act - Simuler la recherche exacte insensible à la casse
        var filteredPartners = operatorType switch
        {
            "is" => partners.Where(p => string.Equals(GetStatusAsString(p), searchValue, StringComparison.OrdinalIgnoreCase)).ToList(),
            "is not" => partners.Where(p => !string.Equals(GetStatusAsString(p), searchValue, StringComparison.OrdinalIgnoreCase)).ToList(),
            _ => new List<UNOPSPartner>()
        };

        // Assert
        filteredPartners.Should().HaveCount(expectedCount, 
            $"Search for '{searchValue}' with operator '{operatorType}' should return {expectedCount} results");
    }

    [Fact]
    public void PartnerSearch_CaseInsensitive_LikeOperator_ShouldWork()
    {
        // Arrange
        var partners = GetTestPartners();
        var searchVariations = new[]
        {
            ("global", 3), // Global Tech Solutions, Global Finance Corp, ACME Global Services
            ("GLOBAL", 3),
            ("GlObAl", 3),
            ("acme", 2), // ACME Corporation, ACME Global Services
            ("ACME", 2),
            ("AcMe", 2),
            ("corp", 2), // ACME Corporation, Global Finance Corp
            ("CORP", 2),
            ("CoRp", 2)
        };

        foreach (var (searchTerm, expectedCount) in searchVariations)
        {
            // Act - Simuler l'opérateur "like" insensible à la casse
            var filteredPartners = partners
                .Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Assert
            filteredPartners.Should().HaveCount(expectedCount, 
                $"Like search for '{searchTerm}' should return {expectedCount} results");
        }
    }

    [Fact]
    public void PartnerSearch_CaseInsensitive_NotLikeOperator_ShouldWork()
    {
        // Arrange
        var partners = GetTestPartners();
        var totalPartners = partners.Count;

        var searchVariations = new[]
        {
            ("global", 2), // Tous sauf les 3 qui contiennent "Global"
            ("GLOBAL", 2),
            ("GlObAl", 2),
            ("acme", 3), // Tous sauf les 2 qui contiennent "ACME"
            ("ACME", 3),
            ("AcMe", 3)
        };

        foreach (var (searchTerm, expectedCount) in searchVariations)
        {
            // Act - Simuler l'opérateur "not like" insensible à la casse
            var filteredPartners = partners
                .Where(p => !p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Assert
            filteredPartners.Should().HaveCount(expectedCount, 
                $"Not like search for '{searchTerm}' should return {expectedCount} results");
        }
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

    /// <summary>
    /// Normalise le texte de recherche en supprimant les espaces en trop et les caractères de contrôle
    /// </summary>
    private static string NormalizeSearchText(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return string.Empty;

        // Remplacer tous les caractères blancs par des espaces normaux
        var normalized = System.Text.RegularExpressions.Regex.Replace(searchText, @"\s+", " ");
        
        // Trim les espaces en début et fin
        return normalized.Trim();
    }

    /// <summary>
    /// Divise le texte de recherche en mots individuels
    /// </summary>
    private static string[] SplitSearchTerms(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return Array.Empty<string>();

        var normalized = NormalizeSearchText(searchText);
        
        return normalized
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length >= 1) // Peut être ajusté selon les besoins
            .ToArray();
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

    private static List<UNOPSPartner> GenerateManyPartners(int count)
    {
        var faker = TestDataBuilder.GetPartnerFaker();
        var partners = faker.Generate(count);
        
        // Ajouter quelques partenaires avec des mots spécifiques pour les tests
        for (int i = 0; i < count; i += 100)
        {
            if (i < partners.Count)
            {
                partners[i].Name = $"Global Tech Solutions {i}";
            }
        }
        
        return partners;
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
            LastModifiedDate = DateTime.UtcNow
        };
    }

    #endregion
}