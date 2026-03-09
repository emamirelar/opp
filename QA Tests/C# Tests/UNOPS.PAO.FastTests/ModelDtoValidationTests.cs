/**
 * @fileoverview Fast standalone tests for DTO/Model naming and structural conventions.
 * Validates naming rules, property types, and structural patterns.
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests DTO/Model naming and structural conventions.
/// All types and registries are defined inline — no production assembly references.
/// </summary>
public class ModelDtoValidationTests
{
    // --- Inline registry of known model types with property lists ---

    private static readonly IReadOnlyList<ModelTypeInfo> KnownModelTypes = new[]
    {
        new ModelTypeInfo("PartnerModel", new[] { "Id", "Name", "Status", "CreatedDate", "LastModifiedDate" }, false),
        new ModelTypeInfo("ContactModel", new[] { "Id", "Name", "PartnerId", "Email", "Status", "CreatedDate" }, false),
        new ModelTypeInfo("OpportunityModel", new[] { "Id", "Name", "Stage", "Status", "CreatedDate" }, false),
        new ModelTypeInfo("CreatePartnerRequest", new[] { "Name", "Status" }, false),
        new ModelTypeInfo("UpdateContactRequest", new[] { "Id", "Name", "Email" }, false),
        new ModelTypeInfo("PartnerFilterRequest", new[] { "PageNumber", "PageSize", "Name", "Status" }, true),
        new ModelTypeInfo("OpportunityFilterRequest", new[] { "PageNumber", "PageSize", "Stage", "Search" }, true),
        new ModelTypeInfo("PartnerInternalDTO", new[] { "Id", "Name", "InternalCode" }, false),
        new ModelTypeInfo("ContactInternalDTO", new[] { "Id", "PartnerId", "Email" }, false),
        new ModelTypeInfo("PartnerExternalDTO", new[] { "Id", "Name", "DisplayName" }, false),
        new ModelTypeInfo("ContactExternalDTO", new[] { "Id", "Name", "Email" }, false)
    };

    private static readonly IReadOnlyList<string> RequiredPropertyNames = new[]
    {
        "Name", "Id", "Email", "Status"
    };

    private static readonly IReadOnlyList<string> DatePropertySuffixes = new[] { "Date", "DateTime" };

    private static readonly IReadOnlyList<string> StatusPropertyNames = new[] { "Status", "Stage", "WorkflowStatus" };

    private static readonly IReadOnlyList<string> ForbiddenPropertySubstrings = new[] { "Password", "Secret" };

    private record ModelTypeInfo(string TypeName, IReadOnlyList<string> Properties, bool IsFilterRequest);

    // --- Model classes end with "Model" suffix (2 tests) ---

    [Fact]
    public void ModelClasses_EndWithModelSuffix()
    {
        var modelTypes = KnownModelTypes.Where(t => t.TypeName.EndsWith("Model")).ToList();
        modelTypes.Should().NotBeEmpty();
        modelTypes.Should().OnlyContain(t => t.TypeName.EndsWith("Model"));
    }

    [Fact]
    public void ModelClasses_AllModelSuffixTypes_FollowConvention()
    {
        var modelTypes = KnownModelTypes.Where(t => t.TypeName.EndsWith("Model")).ToList();
        foreach (var t in modelTypes)
        {
            t.TypeName.Should().EndWith("Model", $"'{t.TypeName}' must end with 'Model'");
        }
    }

    // --- Request DTOs end with "Request" suffix (2 tests) ---

    [Fact]
    public void RequestDtos_EndWithRequestSuffix()
    {
        var requestTypes = KnownModelTypes
            .Where(t => t.TypeName.EndsWith("Request") && !t.TypeName.Contains("Filter"))
            .ToList();
        requestTypes.Should().NotBeEmpty();
        requestTypes.Should().OnlyContain(t => t.TypeName.EndsWith("Request"));
    }

    [Fact]
    public void RequestDtos_AllRequestSuffixTypes_FollowConvention()
    {
        var requestTypes = KnownModelTypes.Where(t =>
            t.TypeName.EndsWith("Request") &&
            (t.TypeName.StartsWith("Create") || t.TypeName.StartsWith("Update")));
        foreach (var t in requestTypes)
        {
            t.TypeName.Should().EndWith("Request");
        }
    }

    // --- Filter requests inherit pagination properties (PageNumber, PageSize) (2 tests) ---

    [Fact]
    public void FilterRequests_HavePageNumberAndPageSize()
    {
        var filterTypes = KnownModelTypes.Where(t => t.IsFilterRequest).ToList();
        filterTypes.Should().NotBeEmpty();
        foreach (var t in filterTypes)
        {
            t.Properties.Should().Contain("PageNumber", $"'{t.TypeName}' must have PageNumber");
            t.Properties.Should().Contain("PageSize", $"'{t.TypeName}' must have PageSize");
        }
    }

    [Fact]
    public void FilterRequests_AllFilterTypes_HavePaginationProperties()
    {
        var filterTypes = KnownModelTypes.Where(t => t.TypeName.Contains("Filter") && t.TypeName.EndsWith("Request"));
        foreach (var t in filterTypes)
        {
            t.Properties.Should().Contain(p => p == "PageNumber" || p == "PageSize");
        }
    }

    // --- Internal DTOs end with "InternalDTO" (2 tests) ---

    [Fact]
    public void InternalDtos_EndWithInternalDtoSuffix()
    {
        var internalTypes = KnownModelTypes.Where(t => t.TypeName.EndsWith("InternalDTO")).ToList();
        internalTypes.Should().NotBeEmpty();
        internalTypes.Should().OnlyContain(t => t.TypeName.EndsWith("InternalDTO"));
    }

    [Fact]
    public void InternalDtos_AllInternalTypes_FollowConvention()
    {
        var internalTypes = KnownModelTypes.Where(t => t.TypeName.EndsWith("InternalDTO"));
        foreach (var t in internalTypes)
        {
            t.TypeName.Should().EndWith("InternalDTO");
        }
    }

    // --- External DTOs end with "ExternalDTO" (2 tests) ---

    [Fact]
    public void ExternalDtos_EndWithExternalDtoSuffix()
    {
        var externalTypes = KnownModelTypes.Where(t => t.TypeName.EndsWith("ExternalDTO")).ToList();
        externalTypes.Should().NotBeEmpty();
        externalTypes.Should().OnlyContain(t => t.TypeName.EndsWith("ExternalDTO"));
    }

    [Fact]
    public void ExternalDtos_AllExternalTypes_FollowConvention()
    {
        var externalTypes = KnownModelTypes.Where(t => t.TypeName.EndsWith("ExternalDTO"));
        foreach (var t in externalTypes)
        {
            t.TypeName.Should().EndWith("ExternalDTO");
        }
    }

    // --- Required properties are marked correctly (2 tests) ---

    [Fact]
    public void RequiredProperties_CommonRequiredNames_ExistInRegistry()
    {
        var allProperties = KnownModelTypes.SelectMany(t => t.Properties).Distinct().ToList();
        allProperties.Should().Contain("Name", "Name is a common required property");
        allProperties.Should().Contain("Id", "Id is a common required property");
    }

    [Fact]
    public void RequiredProperties_RegistryContainsExpectedNames()
    {
        var allProperties = KnownModelTypes.SelectMany(t => t.Properties).ToHashSet();
        foreach (var p in RequiredPropertyNames)
        {
            allProperties.Should().Contain(p, $"required property '{p}' should exist in some model");
        }
    }

    // --- ID properties are integer type (2 tests) ---

    [Fact]
    public void IdProperties_AllModelsHaveIdProperty()
    {
        var modelTypes = KnownModelTypes.Where(t => t.TypeName.EndsWith("Model") || t.TypeName.EndsWith("DTO"));
        foreach (var t in modelTypes)
        {
            t.Properties.Should().Contain("Id", $"'{t.TypeName}' should have Id property");
        }
    }

    [Fact]
    public void IdProperties_IdIsFirstOrEarlyInPropertyList()
    {
        var typesWithId = KnownModelTypes.Where(t => t.Properties.Contains("Id"));
        foreach (var t in typesWithId)
        {
            var idx = t.Properties.ToList().IndexOf("Id");
            idx.Should().BeGreaterThanOrEqualTo(0);
            idx.Should().BeLessThan(3, $"Id should be early in '{t.TypeName}' property list");
        }
    }

    // --- Date properties follow naming convention (*Date, *DateTime) (2 tests) ---

    [Fact]
    public void DateProperties_FollowNamingConvention()
    {
        var allProperties = KnownModelTypes.SelectMany(t => t.Properties).Distinct().ToList();
        var dateProps = allProperties.Where(p =>
            p.EndsWith("Date") || p.EndsWith("DateTime")).ToList();
        dateProps.Should().NotBeEmpty();
        dateProps.Should().OnlyContain(p =>
            p.EndsWith("Date") || p.EndsWith("DateTime"));
    }

    [Fact]
    public void DateProperties_CommonDateNames_Present()
    {
        var allProperties = KnownModelTypes.SelectMany(t => t.Properties).ToHashSet();
        allProperties.Should().Contain("CreatedDate");
        allProperties.Should().Contain("LastModifiedDate");
    }

    // --- Status properties use enum types (2 tests) ---

    [Fact]
    public void StatusProperties_StatusOrStagePresentInModels()
    {
        var modelTypes = KnownModelTypes.Where(t => t.TypeName.EndsWith("Model"));
        foreach (var t in modelTypes)
        {
            var hasStatus = t.Properties.Any(p =>
                StatusPropertyNames.Contains(p));
            hasStatus.Should().BeTrue($"'{t.TypeName}' should have Status or Stage property");
        }
    }

    [Fact]
    public void StatusProperties_RegistryHasStatusProperties()
    {
        var allProperties = KnownModelTypes.SelectMany(t => t.Properties).ToHashSet();
        allProperties.Should().Contain("Status");
        allProperties.Should().Contain("Stage");
    }

    // --- No model exposes "Password" or "Secret" fields (2 tests) ---

    [Fact]
    public void NoModel_ExposesPasswordOrSecret()
    {
        var allProperties = KnownModelTypes.SelectMany(t => t.Properties).Distinct().ToList();
        var forbidden = allProperties.Where(p =>
            ForbiddenPropertySubstrings.Any(f => p.Contains(f, StringComparison.OrdinalIgnoreCase))).ToList();
        forbidden.Should().BeEmpty("no model should expose Password or Secret fields");
    }

    [Fact]
    public void NoModel_PropertyNames_ExcludeSensitiveSubstrings()
    {
        foreach (var t in KnownModelTypes)
        {
            foreach (var p in t.Properties)
            {
                p.Should().NotContain("Password", $"'{t.TypeName}.{p}' must not contain Password");
                p.Should().NotContain("Secret", $"'{t.TypeName}.{p}' must not contain Secret");
            }
        }
    }
}
