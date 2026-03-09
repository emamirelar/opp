/**
 * @fileoverview Fast standalone tests for API route conventions and validation
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests for API route conventions and validation rules
/// </summary>
public class ApiRouteValidationTests
{
    private static readonly IReadOnlyDictionary<string, string> KnownRoutes = new Dictionary<string, string>
    {
        ["Partner"] = "/api/partner",
        ["Contact"] = "/api/contact",
        ["Interaction"] = "/api/interaction",
        ["Opportunity"] = "/api/opportunity",
        ["Document"] = "/api/document",
        ["Comment"] = "/api/comment",
        ["Link"] = "/api/link",
        ["User"] = "/api/user",
        ["Role"] = "/api/role",
        ["AuditLog"] = "/api/auditlog",
        ["EntityConfiguration"] = "/api/entityconfiguration",
        ["Values"] = "/api/values",
        ["Notification"] = "/api/notifications",
        ["Dashboard"] = "/api/dashboard",
        ["AIPrompt"] = "/api/aiprompt",
        ["AIRetriever"] = "/api/airetriever",
        ["Search"] = "/api/search",
        ["Profile"] = "/api/profile",
        ["Import"] = "/api/import",
        ["SeedScript"] = "/api/seedscript",
        ["PartnerTree"] = "/api/partnertree"
    };

    private static IReadOnlyList<string> RouteValues => KnownRoutes.Values.ToList();

    // --- No duplicate routes (2 tests) ---

    [Fact]
    public void Routes_NoDuplicates_CountEqualsDistinctCount()
    {
        var distinctCount = RouteValues.Distinct().Count();
        RouteValues.Count.Should().Be(distinctCount, "route values must not contain duplicates");
    }

    [Fact]
    public void Routes_NoDuplicates_AllValuesUnique()
    {
        var duplicates = RouteValues
            .GroupBy(r => r)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();
        duplicates.Should().BeEmpty("no route should appear more than once");
    }

    // --- All routes start with expected prefix pattern (2 tests) ---

    [Fact]
    public void Routes_AllStartWithApiPrefix()
    {
        foreach (var route in RouteValues)
        {
            route.Should().StartWith("/api/", $"route '{route}' must start with /api/");
        }
    }

    [Fact]
    public void Routes_AllHaveValidPrefix()
    {
        foreach (var route in RouteValues)
        {
            route.Should().MatchRegex("^/api/[a-z0-9]+", $"route '{route}' must follow /api/{{segment}} pattern");
        }
    }

    // --- Routes use kebab-case or lowercase (3 tests) ---

    [Fact]
    public void Routes_NoUppercaseLetters()
    {
        foreach (var route in RouteValues)
        {
            var pathPart = route.TrimStart('/').Split('/')[0];
            pathPart.Should().Be(pathPart.ToLowerInvariant(),
                $"route segment '{pathPart}' must be lowercase or kebab-case");
        }
    }

    [Fact]
    public void Routes_BaseSegmentsAreLowercase()
    {
        foreach (var (_, route) in KnownRoutes)
        {
            var segments = route.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var seg in segments)
            {
                seg.Should().Be(seg.ToLowerInvariant(),
                    $"segment '{seg}' in route '{route}' must be lowercase");
            }
        }
    }

    [Fact]
    public void Routes_NoInvalidCharacters()
    {
        foreach (var route in RouteValues)
        {
            route.Should().MatchRegex("^/api/[a-z0-9/{}_-]+$",
                $"route '{route}' must use only valid path characters (lowercase, digits, slashes, braces)");
        }
    }

    // --- No trailing slashes (2 tests) ---

    [Fact]
    public void Routes_NoTrailingSlash_BaseRoutes()
    {
        var baseRoutes = KnownRoutes.Values.Where(r => !r.Contains("{")).ToList();
        foreach (var route in baseRoutes)
        {
            route.Should().NotEndWith("/", $"route '{route}' must not have trailing slash");
        }
    }

    [Fact]
    public void Routes_NoTrailingSlash_AllRoutes()
    {
        foreach (var route in RouteValues)
        {
            route.TrimEnd('/').Should().Be(route, $"route '{route}' must not have trailing slash");
        }
    }

    // --- Route parameters use consistent format {id} (2 tests) ---

    [Fact]
    public void Routes_WithParameters_UseIdFormat()
    {
        var routesWithParams = KnownRoutes.Values
            .Where(r => r.Contains("{"))
            .ToList();
        foreach (var route in routesWithParams)
        {
            route.Should().MatchRegex(@"\{id\}", $"route '{route}' should use {{id}} for parameter");
        }
    }

    [Fact]
    public void Routes_ParameterFormat_Consistent()
    {
        var paramPattern = new System.Text.RegularExpressions.Regex(@"\{[^}]+\}");
        foreach (var route in RouteValues)
        {
            var matches = paramPattern.Matches(route);
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                m.Value.Should().Be("{id}", $"route '{route}' should use {{id}} not '{m.Value}'");
            }
        }
    }

    // --- No empty route strings (1 test) ---

    [Fact]
    public void Routes_NoEmptyStrings()
    {
        foreach (var (name, route) in KnownRoutes)
        {
            route.Should().NotBeNullOrWhiteSpace($"route for '{name}' must not be empty");
        }
    }

    // --- Entity routes follow RESTful conventions (2 tests) ---

    [Fact]
    public void Routes_EntityRoutes_UseSingularNoun()
    {
        var entityRoutes = new[] { "partner", "contact", "interaction", "opportunity", "document", "comment", "link", "user", "role" };
        foreach (var entity in entityRoutes)
        {
            var expectedPath = $"/api/{entity}";
            KnownRoutes.Values.Should().Contain(expectedPath,
                $"entity route for '{entity}' should follow /api/{{singular}} pattern");
        }
    }

    [Fact]
    public void Routes_EntityRoutes_ConsistentStructure()
    {
        foreach (var route in RouteValues)
        {
            route.Should().MatchRegex("^/api/[a-z]+", $"route '{route}' should follow RESTful /api/resource pattern");
        }
    }
}
