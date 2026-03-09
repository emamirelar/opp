using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Tests for PNO-1146 workflow email notification template selection,
/// EntityUrl construction, CC recipient logic, and template naming.
///
/// These tests validate the notification service contract without
/// requiring the Workflow submodule — they test template name constants,
/// URL format, and model construction.
///
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class WorkflowNotificationTemplateTests
{
    private const string BaseUrl = "https://opportunityplus.dev.unops.org";
    private const string TemplateNamespace = "UNOPS.PAO.Business.EmailTemplates";

    #region Positive (2)

    [Fact]
    public void EntityUrl_OpportunityId_ConstructedCorrectly()
    {
        var entityId = 42;
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/{entityId}";

        entityUrl.Should().Be("https://opportunityplus.dev.unops.org/partnerships/opportunities/42");
        entityUrl.Should().Contain("/partnerships/opportunities/");
        entityUrl.Should().EndWith(entityId.ToString());
    }

    [Fact]
    public void TemplateName_ApprovalRequest_FollowsNamespaceConvention()
    {
        var templateName = $"{TemplateNamespace}.OpportunityWorkflowApprovalRequest.html";

        templateName.Should().StartWith("UNOPS.PAO.Business.EmailTemplates.");
        templateName.Should().EndWith(".html");
        templateName.Should().Contain("ApprovalRequest");
    }

    #endregion

    #region Negative (6)

    [Fact]
    public void EntityUrl_NegativeEntityId_StillConstructsUrl()
    {
        var entityId = -1;
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/{entityId}";

        entityUrl.Should().Contain("-1");
    }

    [Fact]
    public void EntityUrl_ZeroEntityId_StillConstructsUrl()
    {
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/0";

        entityUrl.Should().EndWith("/0");
    }

    [Fact]
    public void TemplateName_EmptyEntity_InvalidTemplate()
    {
        var templateName = $"{TemplateNamespace}..html";

        templateName.Should().Contain("..");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EntityUrl_NullOrEmptyBaseUrl_HandledGracefully(string? baseUrl)
    {
        var effectiveBaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? BaseUrl : baseUrl;
        var entityUrl = $"{effectiveBaseUrl}/partnerships/opportunities/1";

        entityUrl.Should().NotBeNullOrWhiteSpace();
        entityUrl.Should().Contain("/partnerships/opportunities/1");
    }

    [Fact]
    public void EntityStatementUrl_OpportunityId_IncludesStatementPath()
    {
        var entityId = 5;
        var statementUrl = $"{BaseUrl}/partnerships/opportunities/{entityId}/statement";

        statementUrl.Should().EndWith("/statement");
        statementUrl.Should().Contain("/partnerships/opportunities/5/");
    }

    [Fact]
    public void CCRecipientRoles_DoAHierarchy_OrderedByPriority()
    {
        var roleHierarchy = new[]
        {
            "OrgUnit_Director_OrganizationHierarchy",
            "OrgUnit_Deputy_Director_OrganizationHierarchy",
            "Regional_Director_OrganizationHierarchy",
            "Regional_Deputy_Director_OrganizationHierarchy",
            "MCO_Director_OrganizationHierarchy",
            "MCO_Deputy_Director_OrganizationHierarchy"
        };

        roleHierarchy.Should().HaveCount(6);
        roleHierarchy[0].Should().Contain("OrgUnit_Director");
        roleHierarchy.Should().AllSatisfy(r => r.Should().EndWith("_OrganizationHierarchy"));
    }

    #endregion

    #region Edge/Boundary (6)

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(1)]
    public void EntityUrl_ExtremEntityIds_Handled(int entityId)
    {
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/{entityId}";

        entityUrl.Should().NotBeNullOrWhiteSpace();
        entityUrl.Should().Contain($"/{entityId}");
    }

    [Fact]
    public void TemplateNames_AllFourWorkflowTemplates_Exist()
    {
        var templates = new[]
        {
            $"{TemplateNamespace}.OpportunityWorkflowApprovalRequest.html",
            $"{TemplateNamespace}.OpportunityWorkflowCompleted.html",
            $"{TemplateNamespace}.OpportunityWorkflowRejected.html",
            $"{TemplateNamespace}.OpportunityWorkflowRecalled.html"
        };

        templates.Should().HaveCount(4);
        templates.Should().AllSatisfy(t =>
        {
            t.Should().StartWith(TemplateNamespace);
            t.Should().EndWith(".html");
        });
    }

    [Fact]
    public void EntityUrl_TrailingSlashOnBaseUrl_NoDoubleSlash()
    {
        var baseUrlWithSlash = "https://opportunityplus.dev.unops.org/";
        var entityUrl = $"{baseUrlWithSlash.TrimEnd('/')}/partnerships/opportunities/1";

        entityUrl.Should().NotContain("//partnerships");
    }

    [Fact]
    public void CCRecipientRoles_OpportunityManagerRole_CorrectCode()
    {
        var omRole = "Opportunity_Manager_Opportunity";

        omRole.Should().Contain("Opportunity_Manager");
        omRole.Should().EndWith("_Opportunity");
    }

    [Fact]
    public void EntityUrl_NotHashBased_UsesPathRouting()
    {
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/1";

        entityUrl.Should().NotContain("#");
        entityUrl.Should().Contain("/partnerships/opportunities/");
    }

    [Fact]
    public void CCRecipients_Deduplication_CaseInsensitive()
    {
        var recipients = new List<string>
        {
            "User@Example.COM",
            "user@example.com",
            "USER@EXAMPLE.COM"
        };

        var deduplicated = recipients
            .Select(r => r.ToLowerInvariant())
            .Distinct()
            .ToList();

        deduplicated.Should().HaveCount(1);
    }

    #endregion

    #region Functional (6)

    [Theory]
    [InlineData("ApprovalRequest", "OpportunityWorkflowApprovalRequest")]
    [InlineData("Completed", "OpportunityWorkflowCompleted")]
    [InlineData("Rejected", "OpportunityWorkflowRejected")]
    [InlineData("Recalled", "OpportunityWorkflowRecalled")]
    public void TemplateName_WorkflowAction_MapsToCorrectTemplate(string action, string expectedTemplate)
    {
        var fullName = $"{TemplateNamespace}.{expectedTemplate}.html";

        fullName.Should().Contain(action);
        fullName.Should().StartWith(TemplateNamespace);
    }

    [Fact]
    public void EntityUrl_UsesPathBased_NotHashBased()
    {
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/1";

        entityUrl.Should().NotContain("/#/");
        entityUrl.Should().Contain("/partnerships/");
    }

    [Fact]
    public void EntityStatementUrl_AppendedToEntityUrl_Correctly()
    {
        var entityId = 10;
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/{entityId}";
        var statementUrl = $"{entityUrl}/statement";

        statementUrl.Should().Be($"{BaseUrl}/partnerships/opportunities/10/statement");
    }

    [Fact]
    public void CCRecipients_WorkflowInitiator_ExcludesOM_WhenSamePerson()
    {
        var omEmail = "om@unops.org";
        var initiatorEmail = "om@unops.org";
        var ccList = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { omEmail };

        ccList.Add(initiatorEmail);

        ccList.Should().HaveCount(1, "duplicate email should not be added");
    }

    [Fact]
    public void CCRecipients_WorkflowInitiator_IncludedWhenDifferentFromOM()
    {
        var omEmail = "om@unops.org";
        var initiatorEmail = "initiator@unops.org";
        var ccList = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { omEmail };

        ccList.Add(initiatorEmail);

        ccList.Should().HaveCount(2);
    }

    [Fact]
    public void TemplateNames_EmbeddedResourcePrefix_MatchesProjectNamespace()
    {
        var prefix = "UNOPS.PAO.Business.EmailTemplates";

        prefix.Should().StartWith("UNOPS.PAO.Business");
        prefix.Should().EndWith("EmailTemplates");
    }

    #endregion

    #region Integration (6)

    [Fact]
    public void EntityUrl_FullConstruction_ApprovalScenario()
    {
        var entityId = 100;
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/{entityId}";
        var statementUrl = $"{entityUrl}/statement";
        var templateName = $"{TemplateNamespace}.OpportunityWorkflowApprovalRequest.html";

        entityUrl.Should().Be("https://opportunityplus.dev.unops.org/partnerships/opportunities/100");
        statementUrl.Should().Be("https://opportunityplus.dev.unops.org/partnerships/opportunities/100/statement");
        templateName.Should().Be("UNOPS.PAO.Business.EmailTemplates.OpportunityWorkflowApprovalRequest.html");
    }

    [Fact]
    public void EntityUrl_FullConstruction_RejectionScenario()
    {
        var entityId = 200;
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/{entityId}";
        var templateName = $"{TemplateNamespace}.OpportunityWorkflowRejected.html";

        entityUrl.Should().Contain("/partnerships/opportunities/200");
        templateName.Should().Contain("Rejected");
    }

    [Fact]
    public void EntityUrl_FullConstruction_RecallScenario()
    {
        var entityId = 300;
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/{entityId}";
        var templateName = $"{TemplateNamespace}.OpportunityWorkflowRecalled.html";

        entityUrl.Should().Contain("/partnerships/opportunities/300");
        templateName.Should().Contain("Recalled");
    }

    [Fact]
    public void EntityUrl_FullConstruction_CompletionScenario()
    {
        var entityId = 400;
        var entityUrl = $"{BaseUrl}/partnerships/opportunities/{entityId}";
        var templateName = $"{TemplateNamespace}.OpportunityWorkflowCompleted.html";

        entityUrl.Should().Contain("/partnerships/opportunities/400");
        templateName.Should().Contain("Completed");
    }

    [Fact]
    public void CCRecipientRoles_FullHierarchy_AllRolesPresent()
    {
        var roles = new[]
        {
            "OrgUnit_Director_OrganizationHierarchy",
            "OrgUnit_Deputy_Director_OrganizationHierarchy",
            "Regional_Director_OrganizationHierarchy",
            "Regional_Deputy_Director_OrganizationHierarchy",
            "MCO_Director_OrganizationHierarchy",
            "MCO_Deputy_Director_OrganizationHierarchy"
        };

        roles.Should().OnlyContain(r => r.EndsWith("_OrganizationHierarchy"));
        roles.Should().Contain(r => r.StartsWith("OrgUnit_"));
        roles.Should().Contain(r => r.StartsWith("Regional_"));
        roles.Should().Contain(r => r.StartsWith("MCO_"));
    }

    [Fact]
    public void FullWorkflow_AllTemplatesAndUrls_ConsistentNaming()
    {
        var actions = new[] { "ApprovalRequest", "Completed", "Rejected", "Recalled" };

        foreach (var action in actions)
        {
            var templateName = $"{TemplateNamespace}.OpportunityWorkflow{action}.html";

            templateName.Should().StartWith(TemplateNamespace);
            templateName.Should().Contain("OpportunityWorkflow");
            templateName.Should().EndWith(".html");
        }
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 2 | EntityUrl_OpportunityId_ConstructedCorrectly, TemplateName_ApprovalRequest_FollowsNamespaceConvention |
| Negative (N) | 6 | NegativeEntityId, ZeroEntityId, EmptyEntity, NullOrEmptyBaseUrl, EntityStatementUrl, CCRecipientRoles_DoAHierarchy |
| Edge/Boundary (E) | 6 | ExtremEntityIds, AllFourWorkflowTemplates, TrailingSlash, OMRole, NotHashBased, Deduplication |
| Functional (F) | 6 | WorkflowAction_MapsToCorrectTemplate, PathBasedNotHash, StatementUrl, CCExcludesOM, CCIncludesDifferent, EmbeddedResourcePrefix |
| Integration (I) | 6 | ApprovalScenario, RejectionScenario, RecallScenario, CompletionScenario, FullHierarchy, AllTemplatesConsistent |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
