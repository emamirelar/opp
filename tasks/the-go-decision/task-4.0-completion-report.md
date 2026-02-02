# Task 4.0 Completion Report: In-System Notifications Integration

## Summary

Successfully enhanced `PaoWorkflowNotificationService` to create in-system notifications for the notification bell when workflow approval is requested, and mark them as done when a decision is made (approve/reject/recall).

## Completed Subtasks

### 4.1 Add `NotificationManager` dependency ✅

**File:** `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs`

Added `NotificationManager` as a constructor dependency:

```csharp
private readonly NotificationManager _notificationManager;

public const string WorkflowApprovalCategory = "workflow_approval";

public PaoWorkflowNotificationService(
    IEmailSender emailSender,
    AppDbContext context,
    ILogger<PaoWorkflowNotificationService> logger,
    IConfiguration configuration,
    NotificationManager notificationManager)
{
    // ... existing assignments ...
    _notificationManager = notificationManager;
}
```

### 4.2 DI Registration ✅

No changes needed - `NotificationManager` is already registered via assembly scanning in the DI container.

### 4.3 Enhanced `NotifyNewApprovalRequestAsync()` ✅

After sending email, now creates in-system notifications for each approver:

```csharp
// Create in-system notifications for each approver
await CreateInSystemNotificationsAsync(notification, orgUnitName);
```

Private helper method added:

```csharp
private async Task CreateInSystemNotificationsAsync(WorkflowNotification notification, string orgUnitName)
{
    var notificationMessage = $"Go Decision approval required for \"{notification.EntityDisplayName}\" ({orgUnitName})";

    var notificationData = new
    {
        entityName = notification.EntityName,
        entityId = entityId,
        entityDisplayName = notification.EntityDisplayName,
        orgUnitName = orgUnitName,
        requestedBy = notification.PerformedByUserName,
        requestedOn = notification.Timestamp.ToString("o"),
        pendingStage = "GO"
    };

    foreach (var userId in notification.RecipientUserIds)
    {
        await CreateWorkflowNotificationAsync(userId, notificationMessage, ...);
    }
}
```

### 4.4 Entity and EntityId for Navigation ✅

Each notification includes `Entity` and `EntityId` properties for navigation:

```csharp
private async Task CreateWorkflowNotificationAsync(int userId, string message, string entityName, int entityId, object recordData)
{
    var notification = new Notification
    {
        UserId = userId,
        Message = message,
        Category = WorkflowApprovalCategory,
        ResponseType = "action_required",
        Entity = entityName,           // For navigation
        EntityId = entityId,           // For navigation
        RecordData = JsonSerializer.Serialize(recordData),
        IsRead = false,
        Status = NotificationStatus.Pending,
        CreatedAt = DateTime.UtcNow
    };

    await _context.Notifications.AddAsync(notification);
    await _context.SaveChangesAsync();
}
```

### 4.5-4.6 Mark Notifications as Done Methods ✅

Added methods to mark notifications as done when a decision is made:

```csharp
/// <summary>
/// Marks workflow approval notifications as done when a decision is made.
/// </summary>
public async Task MarkWorkflowNotificationsAsDoneAsync(string entityName, int entityId, string? decisionMessage = null)
{
    var notifications = await _context.Notifications
        .Where(n => n.Category == WorkflowApprovalCategory 
                 && n.Entity == entityName 
                 && n.EntityId == entityId
                 && n.Status == NotificationStatus.Pending)
        .ToListAsync();

    foreach (var notification in notifications)
    {
        notification.Status = NotificationStatus.Done;
        notification.IsRead = true;
        
        if (!string.IsNullOrEmpty(decisionMessage))
        {
            notification.Message = $"{notification.Message} - {decisionMessage}";
        }
    }

    await _context.SaveChangesAsync();
}

// Convenience methods
public Task MarkWorkflowNotificationsAsApprovedAsync(...) => MarkWorkflowNotificationsAsDoneAsync(..., "Approved");
public Task MarkWorkflowNotificationsAsRejectedAsync(...) => MarkWorkflowNotificationsAsDoneAsync(..., "Set to NO GO");
public Task MarkWorkflowNotificationsAsRecalledAsync(...) => MarkWorkflowNotificationsAsDoneAsync(..., "Recalled");
```

### 4.6 WorkflowController Integration ✅

**File:** `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs`

Added notification marking calls to:

1. **Approve endpoint** - after successful approval:
```csharp
await _notificationService.MarkWorkflowNotificationsAsApprovedAsync(normalizedEntityName, request.EntityId);
```

2. **Reject endpoint** (Opportunity NO GO):
```csharp
await _notificationService.MarkWorkflowNotificationsAsRejectedAsync(normalizedEntityName, request.EntityId);
```

3. **Reject endpoint** (standard rejection):
```csharp
await _notificationService.MarkWorkflowNotificationsAsRejectedAsync(normalizedEntityName, request.EntityId);
```

4. **Recall endpoint**:
```csharp
await _notificationService.MarkWorkflowNotificationsAsRecalledAsync(normalizedEntityName, request.EntityId);
```

## Files Modified

| File | Action | Description |
|------|--------|-------------|
| `UNOPS.PAO.Business/Workflow/Adapters/PaoWorkflowNotificationService.cs` | MODIFIED | Added NotificationManager dependency, CreateInSystemNotificationsAsync, MarkWorkflowNotificationsAsDoneAsync and convenience methods |
| `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs` | MODIFIED | Added calls to mark notifications as done in Approve, Reject, and Recall endpoints |

## Key Features

### Notification Creation
- Creates in-system notifications for each approver when approval is requested
- Notification message: "Go Decision approval required for {EntityDisplayName} ({OrgUnitName})"
- Category: `workflow_approval`
- ResponseType: `action_required`
- Includes entity reference for navigation

### Notification Completion
- Marks all pending workflow_approval notifications as done when decision is made
- Appends decision message (Approved/Set to NO GO/Recalled) to notification
- Sets `Status = NotificationStatus.Done` and `IsRead = true`

### Notification Data Structure
```json
{
  "entityName": "Opportunity",
  "entityId": 123,
  "entityDisplayName": "New Partnership Initiative",
  "orgUnitName": "Europe Regional Office",
  "requestedBy": "John Smith",
  "requestedOn": "2026-02-02T12:00:00Z",
  "pendingStage": "GO"
}
```

## Integration Points

1. **Notification Bell** - New workflow_approval notifications appear in the notification bell
2. **Actions Required Card** - Can filter notifications by category for dashboard display
3. **Navigation** - `Entity` and `EntityId` enable clicking notification to navigate to opportunity

## Dependencies

- Depends on Task 2.0 (enhanced approve/reject endpoints)
- Depends on Task 3.0 (pending approvals API for dashboard)
- Uses existing `NotificationManager` for creating notifications
- Uses existing `Notification` entity with `Entity`, `EntityId`, and `Status` properties

## Next Steps

Task 5.0: Backend - Email CC Recipients
- Add CC recipients to workflow emails
- Include Opportunity Manager, workflow initiator, and Director/Manager of org unit
