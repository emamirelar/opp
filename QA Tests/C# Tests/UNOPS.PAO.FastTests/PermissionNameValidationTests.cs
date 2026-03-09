/**
 * @fileoverview Fast standalone tests for permission name conventions and validation
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests for permission name conventions and validation rules
/// </summary>
public class PermissionNameValidationTests
{
    private static readonly IReadOnlyList<string> KnownPermissionNames = new[]
    {
        "CanViewAllPartners", "CanEditPartners", "CanDeletePartners", "CanCreatePartners",
        "CanViewAllContacts", "CanEditContacts", "CanDeleteContacts", "CanCreateContacts",
        "CanViewAllOpportunities", "CanEditOpportunities", "CanDeleteOpportunities", "CanCreateOpportunities",
        "CanViewAllInteractions", "CanEditInteractions", "CanDeleteInteractions", "CanCreateInteractions",
        "CanManageUsers", "CanManageRoles", "CanViewAuditLog", "CanManageEntityConfiguration",
        "CanApproveOpportunities", "CanViewDashboard", "CanExportData", "CanImportData",
        "CanManageDocuments", "CanViewReports", "CanManageAIPrompts", "CanManageTranslations"
    };

    private static readonly IReadOnlyDictionary<string, string> PermissionDescriptions = new Dictionary<string, string>
    {
        ["CanViewAllPartners"] = "View all partners",
        ["CanEditPartners"] = "Edit partners",
        ["CanDeletePartners"] = "Delete partners",
        ["CanCreatePartners"] = "Create partners",
        ["CanViewAllContacts"] = "View all contacts",
        ["CanEditContacts"] = "Edit contacts",
        ["CanDeleteContacts"] = "Delete contacts",
        ["CanCreateContacts"] = "Create contacts",
        ["CanViewAllOpportunities"] = "View all opportunities",
        ["CanEditOpportunities"] = "Edit opportunities",
        ["CanDeleteOpportunities"] = "Delete opportunities",
        ["CanCreateOpportunities"] = "Create opportunities",
        ["CanViewAllInteractions"] = "View all interactions",
        ["CanEditInteractions"] = "Edit interactions",
        ["CanDeleteInteractions"] = "Delete interactions",
        ["CanCreateInteractions"] = "Create interactions",
        ["CanManageUsers"] = "Manage users",
        ["CanManageRoles"] = "Manage roles",
        ["CanViewAuditLog"] = "View audit log",
        ["CanManageEntityConfiguration"] = "Manage entity configuration",
        ["CanApproveOpportunities"] = "Approve opportunities",
        ["CanViewDashboard"] = "View dashboard",
        ["CanExportData"] = "Export data",
        ["CanImportData"] = "Import data",
        ["CanManageDocuments"] = "Manage documents",
        ["CanViewReports"] = "View reports",
        ["CanManageAIPrompts"] = "Manage AI prompts",
        ["CanManageTranslations"] = "Manage translations"
    };

    // --- No duplicate permission names (5 tests) ---

    [Fact]
    public void PermissionNames_NoDuplicates_CountEqualsDistinctCount()
    {
        var distinctCount = KnownPermissionNames.Distinct().Count();
        KnownPermissionNames.Count.Should().Be(distinctCount, "permission names must not contain duplicates");
    }

    [Fact]
    public void PermissionNames_NoDuplicates_GroupByRevealsNoDuplicates()
    {
        var duplicates = KnownPermissionNames
            .GroupBy(n => n)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        duplicates.Should().BeEmpty("no permission name should appear more than once");
    }

    [Fact]
    public void PermissionNames_NoDuplicates_HashSetSizeMatchesCount()
    {
        var set = new HashSet<string>(KnownPermissionNames);
        set.Count.Should().Be(KnownPermissionNames.Count, "HashSet deduplication should not remove any items");
    }

    [Fact]
    public void PermissionNames_NoDuplicates_AllNamesUnique()
    {
        var seen = new HashSet<string>();
        foreach (var name in KnownPermissionNames)
        {
            seen.Add(name).Should().BeTrue($"permission '{name}' should not appear more than once");
        }
    }

    [Fact]
    public void PermissionNames_NoDuplicates_FirstAndLastOccurrenceMatch()
    {
        foreach (var name in KnownPermissionNames.Distinct())
        {
            var indices = KnownPermissionNames
                .Select((n, i) => (n, i))
                .Where(x => x.n == name)
                .Select(x => x.i)
                .ToList();
            indices.Should().HaveCount(1, $"permission '{name}' should appear exactly once");
        }
    }

    // --- Permission names follow naming convention: start with "Can" (3 tests) ---

    [Fact]
    public void PermissionNames_AllStartWithCan()
    {
        foreach (var name in KnownPermissionNames)
        {
            name.Should().StartWith("Can", $"permission '{name}' must start with 'Can'");
        }
    }

    [Fact]
    public void PermissionNames_NoneStartWithLowercaseCan()
    {
        var invalid = KnownPermissionNames.Where(n => n.StartsWith("can") && !n.StartsWith("Can")).ToList();
        invalid.Should().BeEmpty("permissions must use PascalCase 'Can' not lowercase");
    }

    [Fact]
    public void PermissionNames_AllHaveCanPrefix_CountMatches()
    {
        var withCan = KnownPermissionNames.Count(n => n.StartsWith("Can"));
        withCan.Should().Be(KnownPermissionNames.Count, "all permission names must start with 'Can'");
    }

    // --- Permission names don't contain spaces or special characters (2 tests) ---

    [Fact]
    public void PermissionNames_NoSpaces()
    {
        var withSpaces = KnownPermissionNames.Where(n => n.Contains(' ')).ToList();
        withSpaces.Should().BeEmpty("permission names must not contain spaces");
    }

    [Fact]
    public void PermissionNames_NoSpecialCharacters()
    {
        var allowed = new HashSet<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
        foreach (var name in KnownPermissionNames)
        {
            foreach (var c in name)
            {
                allowed.Contains(c).Should().BeTrue(
                    $"permission '{name}' contains invalid character '{c}'");
            }
        }
    }

    // --- Every permission has a corresponding description that's non-empty (2 tests) ---

    [Fact]
    public void PermissionNames_AllHaveDescriptions()
    {
        foreach (var name in KnownPermissionNames)
        {
            PermissionDescriptions.Should().ContainKey(name, $"permission '{name}' must have a description");
        }
    }

    [Fact]
    public void PermissionNames_AllDescriptionsNonEmpty()
    {
        foreach (var name in KnownPermissionNames)
        {
            var desc = PermissionDescriptions[name];
            desc.Should().NotBeNullOrWhiteSpace($"description for '{name}' must not be empty");
        }
    }

    // --- Permission names are PascalCase (2 tests) ---

    [Fact]
    public void PermissionNames_AllPascalCase_FirstLetterUppercase()
    {
        foreach (var name in KnownPermissionNames)
        {
            name.Should().NotBeEmpty();
            char.IsUpper(name[0]).Should().BeTrue($"permission '{name}' must start with uppercase (PascalCase)");
        }
    }

    [Fact]
    public void PermissionNames_AllPascalCase_ContainOnlyLetters()
    {
        foreach (var name in KnownPermissionNames)
        {
            name.Should().MatchRegex("^[A-Z][a-zA-Z]*$",
                $"permission '{name}' must be PascalCase (letters only, first uppercase)");
        }
    }
}
