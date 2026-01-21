/**
 * @fileoverview Fast standalone tests for ERP Dim Value business logic
 * Tests critical defect prevention for PNO-686 without full project dependencies
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests for ERP Dimension Value assignment logic
/// Critical defect prevention for PNO-686: Skipping 8000-9999 range
/// </summary>
public class ErpDimValueLogicTests
{
    /// <summary>
    /// Simulates the GetNextErpDimValue logic from UNOPSPartnerManager
    /// This tests the pure business logic without database dependencies
    /// </summary>
    private static int GetNextErpDimValue(int? currentMaxValue)
    {
        // If no existing value, start at 1000
        if (!currentMaxValue.HasValue || currentMaxValue.Value < 1000)
        {
            return 1000;
        }

        int nextValue = currentMaxValue.Value + 1;

        // CRITICAL: Skip the reserved range 8000-9999
        // This is the fix for PNO-686
        if (nextValue >= 8000 && nextValue <= 9999)
        {
            return 10000;
        }

        return nextValue;
    }

    [Fact]
    public void GetNextErpDimValue_WhenNoExistingValues_ShouldReturn1000()
    {
        // Arrange
        int? currentMax = null;

        // Act
        var result = GetNextErpDimValue(currentMax);

        // Assert
        result.Should().Be(1000, "first ERP dim value should start at 1000");
    }

    [Fact]
    public void GetNextErpDimValue_WhenCurrentMaxIs1000_ShouldReturn1001()
    {
        // Arrange
        int? currentMax = 1000;

        // Act
        var result = GetNextErpDimValue(currentMax);

        // Assert
        result.Should().Be(1001);
    }

    [Fact]
    public void GetNextErpDimValue_WhenCurrentMaxIs7999_ShouldSkipTo10000()
    {
        // Arrange - Critical boundary test for PNO-686
        int? currentMax = 7999;

        // Act
        var result = GetNextErpDimValue(currentMax);

        // Assert
        result.Should().Be(10000, 
            "should skip the reserved range 8000-9999 and jump to 10000");
    }

    [Theory]
    [InlineData(8000)]
    [InlineData(8500)]
    [InlineData(9000)]
    [InlineData(9999)]
    public void GetNextErpDimValue_WhenCurrentMaxInReservedRange_ShouldReturn10000(int currentMax)
    {
        // Arrange - Any value in reserved range should skip to 10000
        
        // Act
        var result = GetNextErpDimValue(currentMax);

        // Assert
        result.Should().Be(10000, 
            $"when current max is {currentMax} (in reserved range), next should be 10000");
    }

    [Fact]
    public void GetNextErpDimValue_WhenCurrentMaxIs10000_ShouldReturn10001()
    {
        // Arrange - After skipping reserved range, normal increment resumes
        int? currentMax = 10000;

        // Act
        var result = GetNextErpDimValue(currentMax);

        // Assert
        result.Should().Be(10001);
    }

    [Theory]
    [InlineData(10000, 10001)]
    [InlineData(15000, 15001)]
    [InlineData(20000, 20001)]
    public void GetNextErpDimValue_AfterReservedRange_ShouldIncrementNormally(int currentMax, int expected)
    {
        // Act
        var result = GetNextErpDimValue(currentMax);

        // Assert
        result.Should().Be(expected, "normal increment should continue after reserved range");
    }

    [Fact]
    public void GetNextErpDimValue_BoundaryTest_7998To7999()
    {
        // Arrange
        int? currentMax = 7998;

        // Act
        var result = GetNextErpDimValue(currentMax);

        // Assert
        result.Should().Be(7999, "7999 is still valid (last before reserved range)");
    }

    [Fact]
    public void GetNextErpDimValue_NeverReturnsValueInReservedRange()
    {
        // Arrange - Test a range of inputs
        var testInputs = Enumerable.Range(7990, 30).Select(x => (int?)x).ToList();
        testInputs.Add(null);

        // Act & Assert
        foreach (var input in testInputs)
        {
            var result = GetNextErpDimValue(input);
            var isValidRange = result < 8000 || result >= 10000;
            isValidRange.Should().BeTrue(
                $"result {result} from input {input} should never be in range 8000-9999");
        }
    }
}

/// <summary>
/// Tests for Contact Duplicate Detection logic
/// Critical defect prevention for PNO-676
/// </summary>
public class DuplicateDetectionLogicTests
{
    /// <summary>
    /// Simulates duplicate detection triggering logic
    /// </summary>
    private static bool ShouldTriggerDuplicateDetection(bool isSaved, string? firstName, string? lastName, string? email)
    {
        // Must have saved first
        if (!isSaved) return false;

        // Need at least first name and last name OR email
        bool hasName = !string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName);
        bool hasEmail = !string.IsNullOrWhiteSpace(email);

        return hasName || hasEmail;
    }

    [Fact]
    public void ShouldTriggerDuplicateDetection_AfterSaveWithValidData_ReturnsTrue()
    {
        // Arrange
        bool isSaved = true;
        string firstName = "John";
        string lastName = "Doe";
        string email = "john.doe@test.com";

        // Act
        var result = ShouldTriggerDuplicateDetection(isSaved, firstName, lastName, email);

        // Assert
        result.Should().BeTrue("duplicate detection should trigger after save with valid data");
    }

    [Fact]
    public void ShouldTriggerDuplicateDetection_BeforeSave_ReturnsFalse()
    {
        // Arrange - PNO-676 fix: must wait for save
        bool isSaved = false;
        string firstName = "John";
        string lastName = "Doe";
        string email = "john.doe@test.com";

        // Act
        var result = ShouldTriggerDuplicateDetection(isSaved, firstName, lastName, email);

        // Assert
        result.Should().BeFalse("duplicate detection should NOT trigger before save");
    }

    [Fact]
    public void ShouldTriggerDuplicateDetection_WithOnlyEmail_ReturnsTrue()
    {
        // Arrange
        bool isSaved = true;
        string? firstName = null;
        string? lastName = null;
        string email = "test@example.com";

        // Act
        var result = ShouldTriggerDuplicateDetection(isSaved, firstName, lastName, email);

        // Assert
        result.Should().BeTrue("email alone should be sufficient for duplicate detection");
    }

    [Fact]
    public void ShouldTriggerDuplicateDetection_WithOnlyName_ReturnsTrue()
    {
        // Arrange
        bool isSaved = true;
        string firstName = "John";
        string lastName = "Doe";
        string? email = null;

        // Act
        var result = ShouldTriggerDuplicateDetection(isSaved, firstName, lastName, email);

        // Assert
        result.Should().BeTrue("name alone should be sufficient for duplicate detection");
    }

    [Fact]
    public void ShouldTriggerDuplicateDetection_WithNoData_ReturnsFalse()
    {
        // Arrange
        bool isSaved = true;
        string? firstName = null;
        string? lastName = null;
        string? email = null;

        // Act
        var result = ShouldTriggerDuplicateDetection(isSaved, firstName, lastName, email);

        // Assert
        result.Should().BeFalse("cannot detect duplicates without any identifying data");
    }

    [Theory]
    [InlineData("", "Doe", null)]
    [InlineData("John", "", null)]
    [InlineData("", "", null)]
    [InlineData(null, null, "")]
    [InlineData("  ", "  ", "  ")]
    public void ShouldTriggerDuplicateDetection_WithEmptyOrWhitespaceData_ReturnsFalse(
        string? firstName, string? lastName, string? email)
    {
        // Arrange
        bool isSaved = true;

        // Act
        var result = ShouldTriggerDuplicateDetection(isSaved, firstName, lastName, email);

        // Assert
        result.Should().BeFalse("empty or whitespace data should not trigger duplicate detection");
    }
}

/// <summary>
/// Tests for Advanced Search field mapping logic
/// Critical defect prevention for PNO-677
/// </summary>
public class AdvancedSearchFieldMappingTests
{
    /// <summary>
    /// Simulates the allowed fields list for Partner search
    /// </summary>
    private static readonly HashSet<string> PartnerAllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "name",
        "referenceNumber",
        "email",
        "partnerType",
        "country",
        "city",
        "status",
        "createdDate",
        "lastModifiedDate",
        "organizationType",
        // PNO-677 fix: Added missing fields
        "phoneNumber",
        "website",
        "address",
        "region",
        "partnerApprovalStatus"
    };

    [Theory]
    [InlineData("name")]
    [InlineData("referenceNumber")]
    [InlineData("email")]
    [InlineData("partnerType")]
    [InlineData("country")]
    [InlineData("phoneNumber")]
    [InlineData("website")]
    public void PartnerAllowedFields_ContainsCommonSearchFields(string fieldName)
    {
        // Act & Assert
        PartnerAllowedFields.Should().Contain(fieldName,
            $"'{fieldName}' should be an allowed search field for Partners");
    }

    [Fact]
    public void PartnerAllowedFields_IsCaseInsensitive()
    {
        // Act & Assert
        PartnerAllowedFields.Contains("NAME").Should().BeTrue();
        PartnerAllowedFields.Contains("Name").Should().BeTrue();
        PartnerAllowedFields.Contains("name").Should().BeTrue();
    }

    [Fact]
    public void PartnerAllowedFields_DoesNotContainSensitiveFields()
    {
        // Arrange - Fields that should NOT be searchable
        var sensitiveFields = new[] { "password", "secretKey", "internalNotes", "deletedBy" };

        // Act & Assert
        foreach (var field in sensitiveFields)
        {
            PartnerAllowedFields.Should().NotContain(field,
                $"'{field}' is a sensitive field and should not be searchable");
        }
    }

    [Fact]
    public void PartnerAllowedFields_HasMinimumRequiredFields()
    {
        // Assert - Should have at least core fields
        PartnerAllowedFields.Count.Should().BeGreaterOrEqualTo(10,
            "Partner search should support at least 10 searchable fields");
    }
}

/// <summary>
/// Tests for Export functionality logic
/// Critical defect prevention for PNO-680
/// </summary>
public class ExportLogicTests
{
    /// <summary>
    /// Simulates export field configuration
    /// </summary>
    private static Dictionary<string, Func<TestContact, object?>> GetExportFieldMappings()
    {
        return new Dictionary<string, Func<TestContact, object?>>
        {
            { "FirstName", c => c.FirstName },
            { "LastName", c => c.LastName },
            { "Email", c => c.Email },
            { "Phone", c => c.Phone },
            { "JobTitle", c => c.JobTitle },
            { "Organization", c => c.Organization?.Name },
            { "Country", c => c.Country },
            { "CreatedDate", c => c.CreatedDate?.ToString("yyyy-MM-dd") }
        };
    }

    private record TestContact(
        string? FirstName,
        string? LastName,
        string? Email,
        string? Phone,
        string? JobTitle,
        TestOrganization? Organization,
        string? Country,
        DateTime? CreatedDate);

    private record TestOrganization(string Name);

    [Fact]
    public void ExportFieldMappings_HandlesNullValues_WithoutException()
    {
        // Arrange - Contact with many null values
        var contact = new TestContact(null, null, null, null, null, null, null, null);
        var mappings = GetExportFieldMappings();

        // Act & Assert - Should not throw
        var act = () =>
        {
            foreach (var mapping in mappings)
            {
                var value = mapping.Value(contact);
                // Value can be null, but shouldn't throw
            }
        };

        act.Should().NotThrow("export should handle null values gracefully");
    }

    [Fact]
    public void ExportFieldMappings_HandlesNullNestedObject_WithoutException()
    {
        // Arrange - PNO-680 fix: null Organization should be handled
        var contact = new TestContact("John", "Doe", "john@test.com", null, null, null, null, null);
        var mappings = GetExportFieldMappings();

        // Act
        var orgValue = mappings["Organization"](contact);

        // Assert
        orgValue.Should().BeNull("null organization should return null, not throw");
    }

    [Fact]
    public void ExportFieldMappings_ExtractsNestedValue_WhenPresent()
    {
        // Arrange
        var org = new TestOrganization("Test Corp");
        var contact = new TestContact("John", "Doe", "john@test.com", null, null, org, null, null);
        var mappings = GetExportFieldMappings();

        // Act
        var orgValue = mappings["Organization"](contact);

        // Assert
        orgValue.Should().Be("Test Corp");
    }

    [Fact]
    public void ExportFieldMappings_FormatsDateCorrectly()
    {
        // Arrange
        var date = new DateTime(2024, 6, 15);
        var contact = new TestContact(null, null, null, null, null, null, null, date);
        var mappings = GetExportFieldMappings();

        // Act
        var dateValue = mappings["CreatedDate"](contact);

        // Assert
        dateValue.Should().Be("2024-06-15");
    }

    [Fact]
    public void ExportFieldMappings_ContainsAllRequiredFields()
    {
        // Arrange
        var requiredFields = new[] { "FirstName", "LastName", "Email", "Organization" };
        var mappings = GetExportFieldMappings();

        // Assert
        foreach (var field in requiredFields)
        {
            mappings.Should().ContainKey(field,
                $"'{field}' is a required export field");
        }
    }
}

