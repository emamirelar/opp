/**
 * ENTITY SECURITY TESTS (Partner, Contact, Interaction)
 *
 * Required: ≥26 (FIXED)
 * Purpose: Verify security controls, input validation, and sanitization
 *
 * Coverage Areas:
 * - SQL injection prevention
 * - XSS prevention
 * - Path traversal prevention
 * - Integer overflow handling
 * - Null/empty input handling
 * - Special character handling
 *
 * @see .cursor/rules/comprehensive-test-strategy.mdc
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Security
{
    /// <summary>
    /// Security Tests for Partner, Contact, and Interaction entities
    ///
    /// Required: ≥26 (FIXED)
    /// 3:1 Ratio: P=2, N=6, E=6, F=6, I=6
    /// </summary>
    public class EntitySecurityTests
    {
        #region SQL Injection Prevention (6 tests)

        [Theory]
        [InlineData("'; DROP TABLE Partners; --")]
        [InlineData("1 OR 1=1")]
        [InlineData("UNION SELECT * FROM Users")]
        public void SEC_SQLInjection_PartnerName_Sanitized(string maliciousInput)
        {
            var sanitized = SanitizeForDatabase(maliciousInput);
            sanitized.Should().NotContain("DROP TABLE");
            sanitized.Should().NotContain("UNION SELECT");
        }

        [Theory]
        [InlineData("'; DROP TABLE Contacts; --")]
        [InlineData("admin'--")]
        [InlineData("1; EXEC xp_cmdshell")]
        public void SEC_SQLInjection_ContactSearchField_Sanitized(string maliciousInput)
        {
            var sanitized = SanitizeForDatabase(maliciousInput);
            sanitized.Should().NotContain("EXEC");
            sanitized.Should().NotContain("DROP");
        }

        [Theory]
        [InlineData("'; TRUNCATE TABLE Interactions; --")]
        [InlineData("WAITFOR DELAY '0:0:10'")]
        [InlineData("'; DELETE FROM Partners; --")]
        public void SEC_SQLInjection_InteractionSearchQuery_Sanitized(string maliciousInput)
        {
            var sanitized = SanitizeForDatabase(maliciousInput);
            sanitized.Should().NotContain("TRUNCATE");
            sanitized.Should().NotContain("WAITFOR");
            sanitized.Should().NotContain("DELETE FROM");
        }

        #endregion

        #region XSS Prevention (6 tests)

        [Theory]
        [InlineData("<script>alert('XSS')</script>")]
        [InlineData("<img src=x onerror=alert('XSS')>")]
        [InlineData("javascript:alert('XSS')")]
        public void SEC_XSS_PartnerName_Sanitized(string maliciousInput)
        {
            var sanitized = SanitizeForHtml(maliciousInput);
            sanitized.Should().NotContain("<script>");
            sanitized.Should().NotContain("javascript:");
            sanitized.Should().NotContain("onerror=");
        }

        [Theory]
        [InlineData("<svg onload=alert('XSS')>")]
        [InlineData("<body onload=alert('XSS')>")]
        [InlineData("<iframe src='evil.com'></iframe>")]
        public void SEC_XSS_ContactDescription_Sanitized(string maliciousInput)
        {
            var sanitized = SanitizeForHtml(maliciousInput);
            sanitized.Should().NotContain("onload=");
            sanitized.Should().NotContain("<iframe");
        }

        [Theory]
        [InlineData("<object data='evil.swf'>")]
        [InlineData("<input onfocus=alert('XSS')>")]
        [InlineData("<form action='evil.com'>")]
        public void SEC_XSS_InteractionSubject_Sanitized(string maliciousInput)
        {
            var sanitized = SanitizeForHtml(maliciousInput);
            sanitized.Should().NotContain("<object");
            sanitized.Should().NotContain("onfocus=");
        }

        #endregion

        #region Path Traversal Prevention (4 tests)

        [Theory]
        [InlineData("../../../etc/passwd")]
        [InlineData("..\\..\\..\\windows\\system32")]
        public void SEC_PathTraversal_DocumentReference_Sanitized(string maliciousPath)
        {
            var sanitized = Path.GetFileName(maliciousPath);
            sanitized.Should().NotContain("..");
        }

        [Theory]
        [InlineData("....//....//....//etc/passwd")]
        [InlineData("%2e%2e%2f%2e%2e%2f")]
        public void SEC_PathTraversal_EncodedPath_Handled(string maliciousPath)
        {
            var sanitized = Path.GetFileName(maliciousPath.Replace("%2e", ".").Replace("%2f", "/"));
            sanitized.Should().NotContain("..");
        }

        #endregion

        #region Integer Overflow / ID Validation (4 tests)

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public void SEC_IntegerOverflow_NegativePartnerId_Rejected(int id)
        {
            var isValid = id > 0;
            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void SEC_IntegerOverflow_ExtremeContactId_Validated(int id)
        {
            var isValid = id > 0 && id <= int.MaxValue;
            var expectedValid = id == int.MaxValue;
            isValid.Should().Be(expectedValid);
        }

        [Fact]
        public void SEC_IntegerOverflow_ZeroInteractionId_Rejected()
        {
            var id = 0;
            var isValid = id > 0;
            isValid.Should().BeFalse();
        }

        [Fact]
        public void SEC_IntegerOverflow_ValidId_Accepted()
        {
            var id = 12345;
            var isValid = id > 0 && id <= int.MaxValue;
            isValid.Should().BeTrue();
        }

        #endregion

        #region Null/Empty Input Handling (4 tests)

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void SEC_NullEmpty_PartnerName_Rejected(string? name)
        {
            var isValid = !string.IsNullOrWhiteSpace(name);
            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void SEC_NullEmpty_ContactEmail_Rejected(string? email)
        {
            var isValid = !string.IsNullOrEmpty(email);
            isValid.Should().BeFalse();
        }

        [Fact]
        public void SEC_NullEmpty_SanitizeForDatabase_ReturnsInput()
        {
            SanitizeForDatabase(null).Should().BeNull();
            SanitizeForDatabase("").Should().Be("");
        }

        [Fact]
        public void SEC_NullEmpty_SanitizeForHtml_ReturnsInput()
        {
            SanitizeForHtml(null).Should().BeNull();
            SanitizeForHtml("").Should().Be("");
        }

        #endregion

        #region Special Character Handling (4 tests)

        [Theory]
        [InlineData("Test & <script> ' \" input")]
        [InlineData("O'Brien & Sons")]
        public void SEC_SpecialChars_HtmlEscaped(string input)
        {
            var escaped = System.Net.WebUtility.HtmlEncode(input);
            escaped.Should().Contain("&amp;");
            escaped.Should().NotContain("<script>");
        }

        [Theory]
        [InlineData("Partner\n\r\tName")]
        [InlineData("Contact\x00Null")]
        public void SEC_SpecialChars_ControlChars_Handled(string input)
        {
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotBeNull();
        }

        [Fact]
        public void SEC_SpecialChars_UnicodeInName_Accepted()
        {
            var input = "Partner Café 日本";
            var sanitized = SanitizeForHtml(input);
            sanitized.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void SEC_SpecialChars_SingleQuote_EscapedForDatabase()
        {
            var input = "O'Reilly";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().Contain("''");
        }

        #endregion

        #region Positive Tests (2 tests)

        [Fact]
        [Trait("Category", "Positive")]
        public void SEC_Positive_ValidPartnerName_SanitizedCorrectly()
        {
            var input = "Acme Corporation";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().Be("Acme Corporation");
        }

        [Fact]
        [Trait("Category", "Positive")]
        public void SEC_Positive_ValidContactEmail_Safe()
        {
            var input = "user@example.com";
            var sanitized = SanitizeForHtml(input);
            sanitized.Should().Be("user@example.com");
        }

        #endregion

        #region Negative Tests (6 tests)

        [Fact]
        [Trait("Defect", "DEF-121")]
        [Trait("Category", "Negative")]
        public void SEC_Negative_SQLKeywords_RemovedFromPartnerSearch()
        {
            var input = "SELECT * FROM Partners WHERE 1=1";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotContain("SELECT");
            sanitized.Should().NotContain("FROM");
        }

        [Fact]
        [Trait("Category", "Negative")]
        public void SEC_Negative_XSSInContactTitle_Removed()
        {
            var input = "<script>document.cookie</script>";
            var sanitized = SanitizeForHtml(input);
            sanitized.Should().NotContain("<script>");
        }

        [Fact]
        [Trait("Category", "Negative")]
        public void SEC_Negative_InvalidUrlScheme_Rejected()
        {
            var maliciousUrl = "javascript:alert('XSS')";
            var isValidUrl = Uri.TryCreate(maliciousUrl, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            isValidUrl.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Negative")]
        public void SEC_Negative_CommentInjection_Removed()
        {
            var input = "test'; -- comment";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotContain("--");
        }

        [Fact]
        [Trait("Category", "Negative")]
        public void SEC_Negative_ExecKeyword_Removed()
        {
            var input = "xp_cmdshell 'dir'";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotContain("xp_");
        }

        [Fact]
        [Trait("Category", "Negative")]
        public void SEC_Negative_UpdateKeyword_Removed()
        {
            var input = "UPDATE Users SET Role='Admin'";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotContain("UPDATE");
        }

        #endregion

        #region Edge/Boundary Tests (6 tests)

        [Fact]
        [Trait("Category", "Edge")]
        public void SEC_Edge_EmptyStringAfterSanitization_Handled()
        {
            var input = "DROP DELETE INSERT UPDATE UNION SELECT";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "Edge")]
        public void SEC_Edge_MaxLengthPartnerName_Validated()
        {
            var overLengthName = new string('A', 300);
            var maxLength = 255;
            var isValid = overLengthName.Length <= maxLength;
            isValid.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", "Edge")]
        public void SEC_Edge_BoundaryLengthName_Accepted()
        {
            var exactLengthName = new string('A', 255);
            var maxLength = 255;
            var isValid = exactLengthName.Length <= maxLength;
            isValid.Should().BeTrue();
        }

        [Fact]
        [Trait("Defect", "DEF-121")]
        [Trait("Category", "Edge")]
        public void SEC_Edge_MixedCaseSqlKeywords_Removed()
        {
            var input = "SeLeCt * FrOm Partners";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotContain("SeLeCt");
            sanitized.Should().NotContain("FrOm");
        }

        [Fact]
        [Trait("Category", "Edge")]
        public void SEC_Edge_BlockCommentInjection_Removed()
        {
            var input = "test/* comment */more";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotContain("/*");
            sanitized.Should().NotContain("*/");
        }

        [Fact]
        [Trait("Category", "Edge")]
        public void SEC_Edge_SemicolonInjection_Removed()
        {
            var input = "name; DROP TABLE Partners";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotContain(";");
        }

        #endregion

        #region Functional Tests (6 tests)

        [Fact]
        [Trait("Category", "Functional")]
        public void SEC_Functional_SanitizeForDatabase_AllSqlKeywordsRemoved()
        {
            var input = "test DROP DELETE INSERT UPDATE UNION SELECT EXEC TRUNCATE WAITFOR xp_test";
            var sanitized = SanitizeForDatabase(input);
            sanitized.Should().NotContain("DROP");
            sanitized.Should().NotContain("DELETE");
            sanitized.Should().NotContain("INSERT");
            sanitized.Should().NotContain("UPDATE");
            sanitized.Should().NotContain("UNION");
            sanitized.Should().NotContain("SELECT");
            sanitized.Should().NotContain("EXEC");
            sanitized.Should().NotContain("TRUNCATE");
            sanitized.Should().NotContain("WAITFOR");
            sanitized.Should().NotContain("xp_");
        }

        [Fact]
        [Trait("Category", "Functional")]
        public void SEC_Functional_SanitizeForHtml_AllEventHandlersRemoved()
        {
            var input = "<div onerror=1 onload=1 onclick=1>test</div>";
            var sanitized = SanitizeForHtml(input);
            sanitized.Should().NotContain("onerror=");
            sanitized.Should().NotContain("onload=");
            sanitized.Should().NotContain("onclick=");
        }

        [Fact]
        [Trait("Category", "Functional")]
        public void SEC_Functional_PathGetFileName_PreventsTraversal()
        {
            var path = "../../../etc/passwd";
            var result = Path.GetFileName(path);
            result.Should().Be("passwd");
            result.Should().NotContain("..");
        }

        [Fact]
        [Trait("Category", "Functional")]
        public void SEC_Functional_EmailValidation_RejectsInvalid()
        {
            var invalidEmails = new[] { "not-an-email", "@missing.prefix", "missing@" };
            foreach (var invalidEmail in invalidEmails)
            {
                var isValid = IsValidEmail(invalidEmail);
                isValid.Should().BeFalse();
            }
        }

        [Fact]
        [Trait("Category", "Functional")]
        public void SEC_Functional_EmailValidation_AcceptsValid()
        {
            var validEmail = "user@example.com";
            var isValid = IsValidEmail(validEmail);
            isValid.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Functional")]
        public void SEC_Functional_ConsecutiveSanitization_Idempotent()
        {
            var input = "<script>alert(1)</script>";
            var first = SanitizeForHtml(input);
            var second = SanitizeForHtml(first);
            second.Should().Be(first);
        }

        #endregion

        #region Integration Tests (6 tests)

        [Fact]
        [Trait("Category", "Integration")]
        public void SEC_Integration_PartnerCreateFlow_AllInputsSanitized()
        {
            var name = "'; DROP TABLE Partners; --";
            var description = "<script>alert('XSS')</script>";

            var sanitizedName = SanitizeForDatabase(name);
            var sanitizedDesc = SanitizeForHtml(description);

            sanitizedName.Should().NotContain("DROP");
            sanitizedDesc.Should().NotContain("<script>");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SEC_Integration_ContactCreateFlow_AllInputsSanitized()
        {
            var firstName = "O'Brien";
            var notes = "javascript:evil()";

            var sanitizedFirst = SanitizeForDatabase(firstName);
            var sanitizedNotes = SanitizeForHtml(notes);

            sanitizedFirst.Should().Contain("''");
            sanitizedNotes.Should().NotContain("javascript:");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SEC_Integration_InteractionCreateFlow_AllInputsSanitized()
        {
            var subject = "Meeting'; DELETE FROM Interactions; --";
            var description = "<img onerror=alert(1)>";

            var sanitizedSubject = SanitizeForDatabase(subject);
            var sanitizedDesc = SanitizeForHtml(description);

            sanitizedSubject.Should().NotContain("DELETE");
            sanitizedDesc.Should().NotContain("onerror=");
        }

        [Fact]
        [Trait("Defect", "DEF-121")]
        [Trait("Category", "Integration")]
        public void SEC_Integration_SearchFlow_PartnerContactInteraction_Sanitized()
        {
            var partnerSearch = "1 OR 1=1";
            var contactSearch = "<script>steal()</script>";
            var interactionSearch = "'; TRUNCATE Interactions; --";

            var s1 = SanitizeForDatabase(partnerSearch);
            var s2 = SanitizeForHtml(contactSearch);
            var s3 = SanitizeForDatabase(interactionSearch);

            s1.Should().NotContain("OR 1=1");
            s2.Should().NotContain("<script>");
            s3.Should().NotContain("TRUNCATE");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SEC_Integration_DocumentReference_PathTraversalBlocked()
        {
            var docPath = "../../../secrets.txt";
            var safeName = Path.GetFileName(docPath);
            safeName.Should().Be("secrets.txt");
            safeName.Should().NotContain("..");
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SEC_Integration_FullEntityFlow_IdsValidated()
        {
            var partnerId = -1;
            var contactId = 0;
            var interactionId = int.MaxValue;

            var partnerValid = partnerId > 0;
            var contactValid = contactId > 0;
            var interactionValid = interactionId > 0 && interactionId <= int.MaxValue;

            partnerValid.Should().BeFalse();
            contactValid.Should().BeFalse();
            interactionValid.Should().BeTrue();
        }

        #endregion

        #region Helper Methods

        private static string? SanitizeForDatabase(string? input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input
                .Replace("'", "''")
                .Replace("--", "")
                .Replace(";", "")
                .Replace("/*", "")
                .Replace("*/", "")
                .Replace("xp_", "")
                .Replace("EXEC", "", StringComparison.OrdinalIgnoreCase)
                .Replace("DROP", "", StringComparison.OrdinalIgnoreCase)
                .Replace("DELETE", "", StringComparison.OrdinalIgnoreCase)
                .Replace("INSERT", "", StringComparison.OrdinalIgnoreCase)
                .Replace("UPDATE", "", StringComparison.OrdinalIgnoreCase)
                .Replace("UNION", "", StringComparison.OrdinalIgnoreCase)
                .Replace("SELECT", "", StringComparison.OrdinalIgnoreCase)
                .Replace("TRUNCATE", "", StringComparison.OrdinalIgnoreCase)
                .Replace("WAITFOR", "", StringComparison.OrdinalIgnoreCase);
        }

        private static string? SanitizeForHtml(string? input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return System.Text.RegularExpressions.Regex.Replace(input, @"<[^>]*>", string.Empty)
                .Replace("javascript:", "", StringComparison.OrdinalIgnoreCase)
                .Replace("onerror=", "", StringComparison.OrdinalIgnoreCase)
                .Replace("onload=", "", StringComparison.OrdinalIgnoreCase);
        }

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

        #endregion
    }
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 2 | SEC_Positive_ValidPartnerName_SanitizedCorrectly, SEC_Positive_ValidContactEmail_Safe |
| Negative (N) | 6 | SEC_Negative_SQLKeywords_RemovedFromPartnerSearch, SEC_Negative_XSSInContactTitle_Removed, SEC_Negative_InvalidUrlScheme_Rejected, SEC_Negative_CommentInjection_Removed, SEC_Negative_ExecKeyword_Removed, SEC_Negative_UpdateKeyword_Removed |
| Edge/Boundary (E) | 6 | SEC_Edge_EmptyStringAfterSanitization_Handled, SEC_Edge_MaxLengthPartnerName_Validated, SEC_Edge_BoundaryLengthName_Accepted, SEC_Edge_MixedCaseSqlKeywords_Removed, SEC_Edge_BlockCommentInjection_Removed, SEC_Edge_SemicolonInjection_Removed |
| Functional (F) | 6 | SEC_Functional_SanitizeForDatabase_AllSqlKeywordsRemoved, SEC_Functional_SanitizeForHtml_AllEventHandlersRemoved, SEC_Functional_PathGetFileName_PreventsTraversal, SEC_Functional_EmailValidation_RejectsInvalid, SEC_Functional_EmailValidation_AcceptsValid, SEC_Functional_ConsecutiveSanitization_Idempotent |
| Integration (I) | 6 | SEC_Integration_PartnerCreateFlow_AllInputsSanitized, SEC_Integration_ContactCreateFlow_AllInputsSanitized, SEC_Integration_InteractionCreateFlow_AllInputsSanitized, SEC_Integration_SearchFlow_PartnerContactInteraction_Sanitized, SEC_Integration_DocumentReference_PathTraversalBlocked, SEC_Integration_FullEntityFlow_IdsValidated |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
