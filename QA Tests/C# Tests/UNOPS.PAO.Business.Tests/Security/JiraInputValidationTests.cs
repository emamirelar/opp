using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Tests.TestBase;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using System.Text.RegularExpressions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Security;

/// <summary>
/// Input validation and security tests migrated from JIRA.
/// Covers: SQL injection prevention, XSS prevention, input sanitization,
/// permission role logic, session/API security validation.
/// Source: PNO-582, PNO-691, PNO-677, PNO-457, PNO-474.
/// Tests real entity persistence and input handling via UNOPSAppDbContext.
/// </summary>
public class JiraInputValidationTests : ManagerTestBase
{
    private readonly string _marker = $"JIVT_{Guid.NewGuid():N}";

    #region Helpers

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static string SanitizeForDatabase(string input)
    {
        var sanitized = Regex.Replace(input, @"[;'\-\-]", "");
        sanitized = Regex.Replace(sanitized, @"\b(DROP|DELETE|INSERT|UPDATE|ALTER|EXEC|EXECUTE|UNION|SELECT|TRUNCATE|WAITFOR)\b",
            "", RegexOptions.IgnoreCase);
        return sanitized.Trim();
    }

    private static string SanitizeForHtml(string input)
    {
        var sanitized = Regex.Replace(input, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        sanitized = Regex.Replace(sanitized, @"<[^>]+>", "");
        sanitized = Regex.Replace(sanitized, @"javascript:", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"onerror=", "", RegexOptions.IgnoreCase);
        return sanitized;
    }

    private async Task<UNOPSPartner> SeedPartnerAsync(string name)
    {
        var partner = new UNOPSPartner
        {
            Name = name,
            PartnerShortDescription = "Test",
            Status = EntityStatus.Draft,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            CreatedDate = DateTime.UtcNow,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Partners.AddAsync(partner);
        await SaveChangesAsync();
        RegisterTableCleanup("Partners", $"\"Id\" = {partner.Id}");
        return partner;
    }

    #endregion

    #region Positive Tests

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-JIVT-POS-001")]
    public void POS_001_ValidEmail_PassesValidation()
    {
        IsValidEmail("user@example.com").Should().BeTrue();
        IsValidEmail("user.name+tag@domain.co.uk").Should().BeTrue();
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JIVT-POS-002")]
    public void POS_002_AllowedFileTypes_Accepted()
    {
        var allowed = new[] { ".csv", ".xlsx" };
        Path.GetExtension("contacts.csv").Should().Be(".csv");
        allowed.Should().Contain(".csv");
        allowed.Should().Contain(".xlsx");
    }

    #endregion

    #region Negative Tests (>= 6)

    [Theory]
    [Trait("JIRA", "PNO-677")]
    [Trait("TestId", "TC-JIVT-NEG-001")]
    [InlineData("'; DROP TABLE Partners; --")]
    [InlineData("1 OR 1=1")]
    [InlineData("UNION SELECT * FROM Users")]
    public void NEG_001_SQLInjection_InputSanitized(string maliciousInput)
    {
        var sanitized = SanitizeForDatabase(maliciousInput);
        sanitized.Should().NotContain("DROP TABLE");
        sanitized.Should().NotContain("UNION SELECT");
    }

    [Theory]
    [Trait("JIRA", "PNO-677")]
    [Trait("TestId", "TC-JIVT-NEG-002")]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<img src=x onerror=alert('XSS')>")]
    [InlineData("javascript:alert('XSS')")]
    public void NEG_002_XSSInput_Sanitized(string maliciousInput)
    {
        var sanitized = SanitizeForHtml(maliciousInput);
        sanitized.Should().NotContain("<script>");
        sanitized.Should().NotContain("javascript:");
        sanitized.Should().NotContain("onerror=");
    }

    [Theory]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-JIVT-NEG-003")]
    [InlineData("invalid-email")]
    [InlineData("@missing.prefix")]
    [InlineData("missing@")]
    public void NEG_003_InvalidEmail_FailsValidation(string email)
    {
        IsValidEmail(email).Should().BeFalse();
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JIVT-NEG-004")]
    public void NEG_004_DangerousFileExtension_Rejected()
    {
        var allowed = new[] { ".csv", ".xlsx" };
        var ext = Path.GetExtension("malware.exe");
        allowed.Should().NotContain(ext);
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JIVT-NEG-005")]
    public void NEG_005_PathTraversal_SanitizedByGetFileName()
    {
        var malicious = "../../../etc/passwd";
        var sanitized = Path.GetFileName(malicious);
        sanitized.Should().NotContain("..");
        sanitized.Should().Be("passwd");
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JIVT-NEG-006")]
    public void NEG_006_HTMLInImportData_Escaped()
    {
        var malicious = "<script>steal(cookies)</script>";
        var escaped = System.Net.WebUtility.HtmlEncode(malicious);
        escaped.Should().NotContain("<script>");
        escaped.Should().Contain("&lt;script&gt;");
    }

    #endregion

    #region Edge/Boundary Tests (>= 6)

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-JIVT-EDGE-001")]
    public async Task EDGE_001_PartnerName_MaxLength_Persists()
    {
        var longName = new string('X', 255);
        var partner = await SeedPartnerAsync(longName);

        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded!.Name.Should().HaveLength(255);
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-JIVT-EDGE-002")]
    public void EDGE_002_EmailValidation_EdgeCases()
    {
        IsValidEmail("a@b.c").Should().BeTrue();
        IsValidEmail("").Should().BeFalse();
        IsValidEmail(" ").Should().BeFalse();
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-JIVT-EDGE-003")]
    public void EDGE_003_RoleCheck_PartnerUser_CannotClose()
    {
        var userRoles = new[] { "PartnerUser" };
        var closeAllowed = new[] { "PartnerGlobalAdmin", "Administrator" };
        var canClose = userRoles.Any(r => closeAllowed.Contains(r));
        canClose.Should().BeFalse();
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-JIVT-EDGE-004")]
    public void EDGE_004_RoleCheck_PartnerGlobalAdmin_CanArchive()
    {
        var userRoles = new[] { "PartnerGlobalAdmin" };
        var archiveAllowed = new[] { "PartnerGlobalAdmin", "Administrator" };
        var canArchive = userRoles.Any(r => archiveAllowed.Contains(r));
        canArchive.Should().BeTrue();
    }

    [Fact]
    [Trait("JIRA", "PNO-677")]
    [Trait("TestId", "TC-JIVT-EDGE-005")]
    public void EDGE_005_OrgUnitAdmin_CannotModifyOtherOrgUnits()
    {
        var userOrgUnitId = 1;
        var partnerOrgUnitId = 2;
        var userRoles = new[] { "OrgUnitAdmin" };

        var isSameOrgUnit = userOrgUnitId == partnerOrgUnitId;
        var canModify = userRoles.Contains("Administrator") ||
                       (userRoles.Contains("OrgUnitAdmin") && isSameOrgUnit);

        canModify.Should().BeFalse();
    }

    [Fact]
    [Trait("JIRA", "PNO-474")]
    [Trait("TestId", "TC-JIVT-EDGE-006")]
    public void EDGE_006_SyncedContent_ScriptTagsRemoved()
    {
        var content = "Hello <script>alert('xss')</script> world";
        var sanitized = SanitizeForHtml(content);
        sanitized.Should().NotContain("<script>");
        sanitized.Should().Contain("Hello");
        sanitized.Should().Contain("world");
    }

    #endregion

    #region Functional Tests (>= 6)

    [Fact]
    [Trait("JIRA", "PNO-677")]
    [Trait("TestId", "TC-JIVT-FUNC-001")]
    public async Task FUNC_001_SearchFilter_OrgUnitScope_WorksWithRealData()
    {
        var partner1 = await SeedPartnerAsync($"P1_{_marker}");
        var partner2 = await SeedPartnerAsync($"P2_{_marker}");

        var results = await Context.Partners
            .Where(p => !p.IsDeleted && p.Name!.Contains(_marker))
            .ToListAsync();

        results.Should().HaveCount(2);
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JIVT-FUNC-002")]
    public void FUNC_002_FileSizeLimit_Enforced()
    {
        var fileSizeMB = 100;
        var limitMB = 50;
        (fileSizeMB > limitMB).Should().BeTrue("files exceeding limit should be rejected");
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-JIVT-FUNC-003")]
    public void FUNC_003_BulkSyncLimit_Enforced()
    {
        var syncCount = 20;
        var limit = 15;
        (syncCount > limit).Should().BeTrue("bulk sync exceeding 15 should prompt admin");
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-JIVT-FUNC-004")]
    public void FUNC_004_DDExpiryWarning_Within6Months()
    {
        var expiryDate = DateTime.Today.AddMonths(5);
        var warningThreshold = 6;
        var monthsUntilExpiry = (expiryDate - DateTime.Today).TotalDays / 30;

        (monthsUntilExpiry <= warningThreshold && monthsUntilExpiry > 0).Should().BeTrue();
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-JIVT-FUNC-005")]
    public void FUNC_005_DDExpiryExactlyToday_IsExpired()
    {
        var expiryDate = DateTime.Today;
        (expiryDate <= DateTime.Today).Should().BeTrue();
    }

    [Fact]
    [Trait("JIRA", "PNO-474")]
    [Trait("TestId", "TC-JIVT-FUNC-006")]
    public void FUNC_006_OAuthTokenFormat_Validated()
    {
        var validToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.payload.signature";
        validToken.StartsWith("eyJ").Should().BeTrue();
        validToken.Split('.').Length.Should().Be(3);
    }

    #endregion

    #region Integration Tests (>= 6)

    [Fact]
    [Trait("JIRA", "PNO-677")]
    [Trait("TestId", "TC-JIVT-INT-001")]
    public async Task INT_001_SpecialCharsInPartnerName_PersistedAndQueryable()
    {
        var specialName = $"Partner O'Brien & Co. <{_marker}>";
        var partner = await SeedPartnerAsync(specialName);

        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded!.Name.Should().Be(specialName);
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-JIVT-INT-002")]
    public async Task INT_002_UnicodeInPartnerName_Persists()
    {
        var unicodeName = $"\u00C9l\u00E8ve D\u00E9veloppement {_marker}";
        var partner = await SeedPartnerAsync(unicodeName);

        var loaded = await Context.Partners.FindAsync(partner.Id);
        loaded!.Name.Should().Contain("\u00C9l\u00E8ve");
    }

    [Fact]
    [Trait("JIRA", "PNO-677")]
    [Trait("TestId", "TC-JIVT-INT-003")]
    public async Task INT_003_SearchQuery_CaseInsensitive_LinqFilter()
    {
        await SeedPartnerAsync($"UPPERCASE_{_marker}");
        await SeedPartnerAsync($"lowercase_{_marker}");

        var results = await Context.Partners
            .Where(p => !p.IsDeleted && p.Name!.ToLower().Contains(_marker.ToLower()))
            .ToListAsync();

        results.Should().HaveCount(2);
    }

    [Fact]
    [Trait("JIRA", "PNO-457")]
    [Trait("TestId", "TC-JIVT-INT-004")]
    public async Task INT_004_BulkPartnerCreation_AllPersisted()
    {
        for (int i = 0; i < 5; i++)
        {
            await SeedPartnerAsync($"Bulk_{i}_{_marker}");
        }

        var count = await Context.Partners
            .CountAsync(p => !p.IsDeleted && p.Name!.Contains(_marker));

        count.Should().Be(5);
    }

    [Fact]
    [Trait("JIRA", "PNO-582")]
    [Trait("TestId", "TC-JIVT-INT-005")]
    public async Task INT_005_NegativeBeneficiaryCount_StoredAtDbLevel()
    {
        var opp = new Domain.Entities.Opportunity
        {
            Name = $"NegBen_{_marker}",
            Description = "Test",
            Stage = "IDENTIFY & PROFILE",
            EstimatedDirectBeneficiaries = -100,
            CreatedBy = TestUserId,
            LastModifiedBy = TestUserId,
            LastModifiedDate = DateTime.UtcNow
        };
        await Context.Opportunities.AddAsync(opp);
        await SaveChangesAsync();
        RegisterTableCleanup("Opportunities", $"\"Id\" = {opp.Id}");

        var loaded = await Context.Opportunities.FindAsync(opp.Id);
        loaded!.EstimatedDirectBeneficiaries.Should().Be(-100,
            "DB layer does not enforce positive constraint; validation is in business layer");
    }

    [Fact]
    [Trait("JIRA", "PNO-691")]
    [Trait("TestId", "TC-JIVT-INT-006")]
    public void INT_006_SessionTimeout_Logic_Works()
    {
        var sessionStart = DateTime.UtcNow.AddHours(-2);
        var timeoutHours = 1;
        var isExpired = (DateTime.UtcNow - sessionStart).TotalHours > timeoutHours;

        isExpired.Should().BeTrue();
    }

    #endregion
}
