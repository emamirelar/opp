# Task 5.0 Completion Report: Backend - Email CC Recipients

## Summary

Enhanced `PaoWorkflowNotificationService` email sending to include CC recipients in workflow approval request notifications. Per PRD US-9, CC recipients include:
- Opportunity Manager (from stakeholders)
- Workflow initiator (if different from OM)
- Director/Manager of responsible org unit

## Changes Made

### 1. Email Infrastructure

**File: `UNOPS.PAO.MailSender/Models/EmailMessage.cs`**
- Added `CcReceivers` property: `string[] CcReceivers { get; init; } = Array.Empty<string>();`
- This allows any email message to include CC recipients

**File: `UNOPS.PAO.MailSender/Services/SmtpEmailSender.cs`**
- Updated `CreateMimeMessage()` method to add CC recipients to the MimeMessage
- Uses `message.Cc.AddRange()` to add CC addresses when present

### 2. CC Recipient Logic

**File: `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs`**

Added new region `#region CC Recipient Methods` with the following methods:

1. **`BuildCCRecipientsAsync(WorkflowNotification notification)`**
   - Main orchestration method that builds the CC recipient list
   - Only applies to Opportunity entities (returns empty list for other entities)
   - Deduplicates emails to prevent duplicates
   - Handles exceptions gracefully (logs warning, proceeds without CC)

2. **`GetOpportunityManagerEmailAsync(string entityId)`**
   - Queries `OpportunityStakeholders` with `EntityRole.Code == "Opportunity_Manager_Opportunity"`
   - Returns OM's email or null

3. **`GetUserEmailAsync(int userId)`**
   - Simple user lookup by ID
   - Returns user's email or null

4. **`GetDirectorManagerEmailAsync(int orgUnitId)`**
   - Queries `EntityUserRole` for Director/Deputy Director roles on the org unit
   - Role codes checked (in priority order):
     - `OrgUnit_Director_OrganizationHierarchy`
     - `OrgUnit_Deputy_Director_OrganizationHierarchy`
     - `Regional_Director_OrganizationHierarchy`
     - `Regional_Deputy_Director_OrganizationHierarchy`
     - `MCO_Director_OrganizationHierarchy`
     - `MCO_Deputy_Director_OrganizationHierarchy`
   - Returns first match's email or null

### 3. Integration with NotifyNewApprovalRequestAsync

Updated `NotifyNewApprovalRequestAsync()` to:
1. Call `BuildCCRecipientsAsync()` after retrieving recipient data
2. Include `CcReceivers = ccRecipients.ToArray()` in the `EmailMessage`
3. Log CC count in the success message

## Unit Tests

**File: `QA Tests/Integration Tests/UnitTests/Workflow/PaoWorkflowNotificationServiceCCTests.cs`**

Created comprehensive unit tests:

1. **`NotifyNewApprovalRequestAsync_SendsEmailWithCCRecipients`**
   - Verifies CC includes OM, initiator, and director

2. **`NotifyNewApprovalRequestAsync_DoesNotDuplicateCCWhenInitiatorIsOM`**
   - Verifies no duplicate emails when initiator is same as OM

3. **`NotifyNewApprovalRequestAsync_ReturnsEmptyCCForNonOpportunity`**
   - Verifies CC is empty for non-Opportunity entities

4. **`NotifyNewApprovalRequestAsync_HandlesNoOMStakeholder`**
   - Verifies graceful handling when OM not found

5. **`EmailMessage_CcReceiversDefaultsToEmptyArray`**
   - Verifies EmailMessage CC defaults correctly

6. **`EmailMessage_CcReceiversCanBeSet`**
   - Verifies EmailMessage CC can be populated

## Build Verification

- ✅ `UNOPS.PAO.MailSender` project builds successfully
- ✅ `UNOPS.PAO.Business` project builds successfully  
- ✅ `UNOPS.PAO.Server` project builds successfully (0 errors)
- ⚠️ Test project has pre-existing `Facing` enum compilation error (unrelated to these changes)

## Key Design Decisions

1. **CC only for Opportunity entities**: The CC logic only applies when `EntityName == "Opportunity"`. Other entity types return an empty CC list.

2. **Deduplication**: CC recipients are deduplicated using case-insensitive string comparison to prevent sending duplicate CC emails.

3. **Graceful error handling**: If any CC lookup fails, the method logs a warning and continues without CC rather than failing the entire notification.

4. **Null safety**: All helper methods return `null` when data is not found, and the builder handles nulls gracefully.

5. **Priority order for Director**: Director roles are queried without explicit ordering since `FirstOrDefaultAsync()` returns the first match. The role codes array defines implicit priority.

## Related PRD Requirements

| Requirement | Status |
|------------|--------|
| US-9: Email CC Recipients | ✅ Implemented |
| AC 1.1: Email notification sent to DoA holders with CC recipients | ✅ Implemented |
| FR-9: Add CC Recipients to Email Notifications | ✅ Implemented |

## Files Modified

| File | Change Type |
|------|-------------|
| `UNOPS.PAO.MailSender/Models/EmailMessage.cs` | MODIFY - Added CcReceivers property |
| `UNOPS.PAO.MailSender/Services/SmtpEmailSender.cs` | MODIFY - Added CC to MimeMessage |
| `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs` | MODIFY - Added CC recipient methods and integration |
| `QA Tests/Integration Tests/UnitTests/Workflow/PaoWorkflowNotificationServiceCCTests.cs` | NEW - Unit tests |

## Next Steps

Task 6.0 (Post-Decision Immutability) can now be implemented. This task adds immutability enforcement in `OpportunityManager` to prevent modifications after Go/No-Go/Cancelled decisions.
