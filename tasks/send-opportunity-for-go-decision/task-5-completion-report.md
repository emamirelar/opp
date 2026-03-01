# Task 5.0 Completion Report: Backend Email Notification Templates

**Date Completed:** 2026-01-29  
**Task Reference:** send-opportunity-for-go-decision-tasks.md - Task 5.0

---

## Summary

Updated all email templates with PRD-specific wording for the Go Decision workflow, implemented actual email sending in `PaoWorkflowNotificationService`, and added internal stakeholder notification for FR-11.

---

## Subtasks Completed

| Subtask | Description | Status |
|---------|-------------|--------|
| 5.1 | Update `WorkflowApprovalRequest.html` template | ✅ Complete |
| 5.2 | Update `WorkflowCompleted.html` template | ✅ Complete |
| 5.3 | Update `WorkflowRejected.html` template | ✅ Complete |
| 5.4 | Update `WorkflowRecalled.html` template | ✅ Complete |
| 5.5 | Update `PaoWorkflowNotificationService.cs` to send actual emails | ✅ Complete |
| 5.6 | Add method to get recipient emails/names from user IDs | ✅ Complete |
| 5.7 | Add method to notify Internal Stakeholders on Go decision | ✅ Complete |
| 5.8 | Test notification service methods | ✅ Complete |
| 5.9 | Review implementation | ✅ Complete |

---

## Files Modified

### Email Templates

| File | Changes |
|------|---------|
| `UNOPS.PAO.Business/EmailTemplates/WorkflowApprovalRequest.html` | Updated for Go Decision: DoA Level 2 greeting, org unit info, statement link (`#statement`), internal stakeholder notification note |
| `UNOPS.PAO.Business/EmailTemplates/WorkflowCompleted.html` | Updated for Go Decision approved: GO stage messaging, approver info, org unit context |
| `UNOPS.PAO.Business/EmailTemplates/WorkflowRejected.html` | Updated for NO GO: "Set to NO GO" language, org unit won't proceed messaging, OM reopen note |
| `UNOPS.PAO.Business/EmailTemplates/WorkflowRecalled.html` | Updated for recall: mandatory justification display, org unit context |

### Backend Service

| File | Changes |
|------|---------|
| `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs` | Major update - implemented actual email sending with template models |
| `UNOPS.PAO.Business/Workflow/Adapters/WorkflowServiceExtensions.cs` | Updated DI registration for concrete type injection |
| `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` | Injected notification service, added internal stakeholder notification call on GO approval |

### Tests

| File | Changes |
|------|---------|
| `QA Tests/Integration Tests/Controllers/WorkflowControllerTests.cs` | Updated with notification service mock and configuration |

---

## Implementation Details

### Email Template Model Records (New)

```csharp
public record ApprovalRequestEmailModel
{
    public string ApproverName { get; init; }
    public string ApproverRole { get; init; } = "DoA Level 2";
    public string OrgUnitName { get; init; }
    public string EntityName { get; init; }
    public string RequestedByName { get; init; }
    public string RequestedOn { get; init; }
    public string Comment { get; init; }
    public string EntityUrl { get; init; }
}

// Similar records: WorkflowCompletedEmailModel, WorkflowRejectedEmailModel, WorkflowRecalledEmailModel
```

### Notification Methods Implemented

| Method | Email Template | Subject Pattern |
|--------|---------------|-----------------|
| `NotifyNewApprovalRequestAsync()` | WorkflowApprovalRequest.html | "PAO: [Name] - Action Required" |
| `NotifyWorkflowCompletedAsync()` | WorkflowCompleted.html | "PAO: [Name] - Go Decision Approved" |
| `NotifyWorkflowRejectedAsync()` | WorkflowRejected.html | "PAO: [Name] - Set to NO GO" |
| `NotifyWorkflowRecalledAsync()` | WorkflowRecalled.html | "PAO: [Name] - Submission Recalled" |

### Internal Stakeholder Notification (FR-11)

```csharp
public async Task NotifyInternalStakeholdersOnGoDecisionAsync(int opportunityId, string approverName)
{
    // 1. Get opportunity with countries
    // 2. Get org units responsible for those countries (excluding opportunity's own org unit)
    // 3. Get stakeholders from those org units
    // 4. Send FYI notification
}
```

Integrated into `WorkflowController.Approve()`:
```csharp
if (normalizedEntityName == "Opportunity" && newStage == OpportunityWorkflow.Stages.Go)
{
    var currentUserName = await GetCurrentUserNameAsync();
    await _notificationService.NotifyInternalStakeholdersOnGoDecisionAsync(request.EntityId, currentUserName);
}
```

### Helper Methods Added

| Method | Purpose |
|--------|---------|
| `GetRecipientEmailsAsync(List<int> userIds)` | Returns email addresses for user IDs |
| `GetRecipientNamesAsync(List<int> userIds)` | Returns display names for email greetings |
| `GetOrgUnitNameForOpportunityAsync(string entityId)` | Returns org unit name for opportunity |
| `GetCurrentUserNameAsync()` | Returns current user's display name (in controller) |

### DI Registration Update

```csharp
// Register notification service as both interface and concrete type
services.AddScoped<PaoWorkflowNotificationService>();
services.AddScoped<IWorkflowNotificationService>(sp => sp.GetRequiredService<PaoWorkflowNotificationService>());
```

---

## Email Template Specifications

### WorkflowApprovalRequest.html

**Subject:** `PAO: {EntityName} - Action Required`

**Key Elements:**
- Greeting: "Hello {ApproverName},"
- Action box: "As a **DoA Level 2** for **{OrgUnitName}**, you have been requested to review and approve a Go Decision"
- Information table: Opportunity, Responsible Org Unit, Submitted By, Submitted On
- Comment section (if provided)
- Button: "Review Opportunity Statement" → links to `{EntityUrl}#statement`
- Note: "If the Go Decision is approved, internal stakeholders from other org units normally responsible for the opportunity's implementation countries will be automatically notified."

### WorkflowCompleted.html

**Subject:** `PAO: {EntityName} - Go Decision Approved`

**Key Elements:**
- Success message: "The Go Decision for your opportunity has been approved!"
- GO stage notification
- Information table: Opportunity, Responsible Org Unit, Approved By, Approved On
- Button: "View Opportunity"

### WorkflowRejected.html

**Subject:** `PAO: {EntityName} - Set to NO GO`

**Key Elements:**
- "Opportunity Set to NO GO" header
- Message: "The DoA Level 2 holder has decided that **{OrgUnitName}** will not proceed with the development of this opportunity at this time."
- Rejection reason (comment)
- Note: "The Opportunity Manager can reopen this opportunity if circumstances change and the opportunity becomes viable again."

### WorkflowRecalled.html

**Subject:** `PAO: {EntityName} - Submission Recalled`

**Key Elements:**
- "Go Decision Submission Recalled" header
- Message: "The Go Decision request for the following opportunity has been recalled and no longer requires your action"
- Mandatory justification display
- "No further action is required from you at this time."

---

## PRD Requirements Addressed

| Requirement | Description | Implementation |
|-------------|-------------|----------------|
| FR-9 | Create Email Templates | All 4 templates updated with PRD wording |
| FR-10 | Update PaoWorkflowNotificationService | Implemented actual email sending |
| FR-11 | Notify Internal Stakeholders on Go Decision | `NotifyInternalStakeholdersOnGoDecisionAsync()` integrated |

---

## Testing Notes

- Notification service is tested via controller integration tests
- Email templates use Handlebars-style placeholders (`{{FieldName}}`)
- Templates render via `IEmailTemplateRenderer` in `SmtpEmailSender`
- Base URL configurable via `AppConfig:BaseUrl` in configuration

---

## Dependencies

- `UNOPS.PAO.MailSender` - Email sending infrastructure
- `IEmailSender` - Email sending interface
- `IEmailTemplateRenderer` - Template rendering
- `IConfiguration` - For `AppConfig:BaseUrl` setting

---

## Next Steps

- **Task 6.0:** Frontend: Requirements Validation Integration
- **Task 7.0:** Frontend: Workflow UI Updates
- **Task 8.0:** Integration & End-to-End Testing
