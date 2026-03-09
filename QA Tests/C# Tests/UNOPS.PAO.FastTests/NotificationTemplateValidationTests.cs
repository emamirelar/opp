/**
 * @fileoverview Fast standalone tests for notification template conventions and validation
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests for notification template conventions: workflow event coverage,
/// template naming, placeholder patterns, event mapping, and subject line presence.
/// </summary>
public class NotificationTemplateValidationTests
{
    // --- Inline template definitions ---

    private static readonly IReadOnlyList<string> KnownTemplateNames =
    [
        "OpportunityWorkflowApprovalRequest",
        "OpportunityWorkflowCompleted",
        "OpportunityWorkflowRejected",
        "OpportunityWorkflowRecalled",
        "DueDiligenceExpiryNotification"
    ];

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> TemplatePlaceholders =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["OpportunityWorkflowApprovalRequest"] = ["EntityName", "EntityUrl", "ApproverName"],
            ["OpportunityWorkflowCompleted"] = ["EntityName", "EntityUrl", "InitiatorName"],
            ["OpportunityWorkflowRejected"] = ["EntityName", "EntityUrl", "InitiatorName"],
            ["OpportunityWorkflowRecalled"] = ["EntityName", "EntityUrl"],
            ["DueDiligenceExpiryNotification"] = ["EntityName", "EntityUrl", "ExpiryDate"]
        };

    private static readonly IReadOnlyDictionary<string, string> TemplateToEvent =
        new Dictionary<string, string>
        {
            ["OpportunityWorkflowApprovalRequest"] = "ApprovalRequest",
            ["OpportunityWorkflowCompleted"] = "Completed",
            ["OpportunityWorkflowRejected"] = "Rejected",
            ["OpportunityWorkflowRecalled"] = "Recalled",
            ["DueDiligenceExpiryNotification"] = "DueDiligenceExpiry"
        };

    private static readonly IReadOnlyDictionary<string, string> TemplateSubjects =
        new Dictionary<string, string>
        {
            ["OpportunityWorkflowApprovalRequest"] = "Approval required: {{ EntityName }}",
            ["OpportunityWorkflowCompleted"] = "Opportunity completed: {{ EntityName }}",
            ["OpportunityWorkflowRejected"] = "Opportunity rejected: {{ EntityName }}",
            ["OpportunityWorkflowRecalled"] = "Opportunity recalled: {{ EntityName }}",
            ["DueDiligenceExpiryNotification"] = "Due diligence expiry: {{ EntityName }}"
        };

    private static readonly IReadOnlyList<string> WorkflowEvents =
    [
        "ApprovalRequest",
        "Completed",
        "Rejected",
        "Recalled",
        "DueDiligenceExpiry"
    ];

    private static bool IsPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        return value[0] == char.ToUpperInvariant(value[0]) &&
               !value.Contains(" ") &&
               value.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    // --- All workflow events have corresponding templates (3 tests) ---

    [Fact]
    public void WorkflowEvents_AllHaveCorrespondingTemplates()
    {
        foreach (var evt in WorkflowEvents)
        {
            var hasTemplate = TemplateToEvent.Values.Contains(evt);
            hasTemplate.Should().BeTrue($"workflow event '{evt}' must have a corresponding template");
        }
    }

    [Fact]
    public void WorkflowEvents_CountMatchesTemplateCount()
    {
        var eventCount = WorkflowEvents.Distinct().Count();
        var templateCount = TemplateToEvent.Count;
        templateCount.Should().BeGreaterOrEqualTo(eventCount);
    }

    [Fact]
    public void WorkflowEvents_ApprovalRequest_HasTemplate()
    {
        var template = TemplateToEvent.FirstOrDefault(kv => kv.Value == "ApprovalRequest");
        template.Key.Should().NotBeNullOrEmpty();
        template.Key.Should().Be("OpportunityWorkflowApprovalRequest");
    }

    // --- Template names follow naming convention (2 tests) ---

    [Fact]
    public void TemplateNames_AllUsePascalCase()
    {
        foreach (var name in KnownTemplateNames)
        {
            name.Should().MatchRegex("^[A-Z][a-zA-Z0-9]*", $"template '{name}' must follow PascalCase");
        }
    }

    [Fact]
    public void TemplateNames_WorkflowTemplates_ContainWorkflow()
    {
        var workflowTemplates = KnownTemplateNames.Where(n => n.Contains("Workflow")).ToList();
        workflowTemplates.Should().HaveCount(4, "ApprovalRequest, Completed, Rejected, Recalled are workflow templates");
    }

    // --- Required placeholders are present in template definitions (3 tests) ---

    [Fact]
    public void Placeholders_AllTemplates_HaveEntityName()
    {
        foreach (var (template, placeholders) in TemplatePlaceholders)
        {
            placeholders.Should().Contain("EntityName", $"template '{template}' must have EntityName placeholder");
        }
    }

    [Fact]
    public void Placeholders_AllTemplates_HaveEntityUrl()
    {
        foreach (var (template, placeholders) in TemplatePlaceholders)
        {
            placeholders.Should().Contain("EntityUrl", $"template '{template}' must have EntityUrl placeholder");
        }
    }

    [Fact]
    public void Placeholders_AllTemplates_HaveAtLeastTwoPlaceholders()
    {
        foreach (var (template, placeholders) in TemplatePlaceholders)
        {
            placeholders.Count.Should().BeGreaterOrEqualTo(2, $"template '{template}' must have at least 2 placeholders");
        }
    }

    // --- No duplicate template names (2 tests) ---

    [Fact]
    public void TemplateNames_NoDuplicates()
    {
        var distinct = KnownTemplateNames.Distinct().ToList();
        KnownTemplateNames.Count.Should().Be(distinct.Count);
    }

    [Fact]
    public void TemplateNames_AllUnique()
    {
        var duplicates = KnownTemplateNames.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        duplicates.Should().BeEmpty();
    }

    // --- Template event mapping is complete (2 tests) ---

    [Fact]
    public void TemplateEventMapping_AllTemplates_HaveEvent()
    {
        foreach (var template in KnownTemplateNames)
        {
            TemplateToEvent.Should().ContainKey(template);
        }
    }

    [Fact]
    public void TemplateEventMapping_AllEvents_HaveTemplate()
    {
        var mappedEvents = TemplateToEvent.Values.Distinct().ToList();
        foreach (var evt in WorkflowEvents)
        {
            mappedEvents.Should().Contain(evt);
        }
    }

    // --- Placeholder names use PascalCase (2 tests) ---

    [Fact]
    public void PlaceholderNames_AllUsePascalCase()
    {
        foreach (var (template, placeholders) in TemplatePlaceholders)
        {
            foreach (var ph in placeholders)
            {
                IsPascalCase(ph).Should().BeTrue($"placeholder '{ph}' in template '{template}' must use PascalCase");
            }
        }
    }

    [Fact]
    public void PlaceholderNames_EntityNameAndEntityUrl_ArePascalCase()
    {
        var commonPlaceholders = new[] { "EntityName", "EntityUrl", "ApproverName", "InitiatorName", "ExpiryDate" };
        foreach (var ph in commonPlaceholders)
        {
            ph.Should().MatchRegex("^[A-Z][a-zA-Z0-9]*");
        }
    }

    // --- All templates have a subject line pattern (2 tests) ---

    [Fact]
    public void SubjectLine_AllTemplates_HaveSubject()
    {
        foreach (var template in KnownTemplateNames)
        {
            TemplateSubjects.Should().ContainKey(template);
            TemplateSubjects[template].Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void SubjectLine_AllSubjects_ContainEntityNamePlaceholder()
    {
        foreach (var (template, subject) in TemplateSubjects)
        {
            subject.Should().Contain("{{ EntityName }}", $"template '{template}' subject must include {{ EntityName }}");
        }
    }
}
