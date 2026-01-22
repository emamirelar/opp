# Migration Guide: WorkflowStage to UNOPS.Workflow Submodule

## Overview

This guide outlines the steps needed to migrate PAO from the database-driven `WorkflowStage` approach to the **UNOPS.Workflow submodule** - the same production-proven workflow library used by GMS.

**Estimated Time:** 9-13 hours
**Complexity:** Medium
**Risk Level:** Medium (requires entity changes and data migration)

---

## What is UNOPS.Workflow?

UNOPS.Workflow is a **reusable Git submodule** that provides:

- ✅ **Complete Workflow Engine** - State machines, transitions, approvals
- ✅ **Separate Database Schema** - `workflow` schema auto-created with migrations
- ✅ **Pre-built Entities** - `WorkflowLog`, `StateMachineStageChange`, `StateMachineStageChangeRole`
- ✅ **Role-Based Permissions** - Fine-grained control over who can trigger/approve
- ✅ **Approval Workflows** - Multi-user approval system out of the box
- ✅ **Angular Components** - Ready-to-use UI components
- ✅ **Battle-Tested** - Used in GMS with 15+ entity types in production

**Repository:** `https://github.com/UNOPS-ITG/unops-workflow.git`

---

## How GMS Uses It

GMS integrates UNOPS.Workflow as a submodule:

```
business-gms-plus/
├── UNOPS.Grants.Business/       # References submodule projects
├── UNOPS.Grants.DataAccess/     # Uses AppDbContext for grants data
├── UNOPS.Workflow/              # Git submodule
│   ├── UNOPS.Workflow.Business/
│   ├── UNOPS.Workflow.DataAccess/  # WorkflowDbContext (auto-created)
│   ├── UNOPS.Workflow.Domain/
│   ├── UNOPS.Workflow.Models/
│   └── unops-workflow-angular/
```

**Key Point:** GMS does NOT create `WorkflowDbContext` or workflow entities - they come from the submodule!

---

## Architecture: Current vs Target

### Current PAO Architecture
```
Opportunities Table
├── WorkflowStageId (FK to WorkflowStages)
└── WorkflowStage Navigation Property

WorkflowStages Table
├── Id
├── EntityType
├── Name
└── Order
```

### Target Architecture (with UNOPS.Workflow)
```
public.Opportunities Table
└── Stage (string property, no FK)

workflow.StateMachineStageChanges     ← Provided by submodule
├── EntityName (e.g., "Opportunity")
├── FromStage → ToStage
├── ActionName
├── ApprovalRequired
└── CommentRequired/Optional

workflow.StateMachineStageChangeRoles ← Provided by submodule
├── EntityName
├── FromStage → ToStage
├── RoleId
└── CanTrigger / CanApprove

workflow.WorkflowLogs                 ← Provided by submodule
├── EntityName
├── EntityId
├── Stage → NewStage
├── Action
└── UserId / Timestamp
```

**Everything in `workflow` schema is auto-created by the submodule!**

---

## Benefits of UNOPS.Workflow Approach

| Feature | Current (WorkflowStage) | UNOPS.Workflow |
|---------|------------------------|----------------|
| **State Definitions** | Database (WorkflowStages table) | Code (StateMachine class) |
| **Transition Rules** | None (manual checks) | Database (StateMachineStageChanges) |
| **Role Permissions** | Manual in code | Database (StateMachineStageChangeRoles) |
| **Approval Workflows** | Not implemented | Built-in (Initiate/Approve/Reject/Recall) |
| **Audit Trail** | None | Complete (WorkflowLogs) |
| **Version Control** | ❌ Database seeds only | ✅ Git-tracked code |
| **Runtime Config** | ❌ Requires deployment | ✅ Database-driven transitions |
| **Reusability** | ❌ PAO-specific | ✅ Used across UNOPS projects |
| **Frontend Components** | Custom | Pre-built Angular components |
| **Maintenance** | PAO team | Workflow submodule team |

---

## PAO's Existing Entity Role System (Advantage!)

**Great news!** PAO already has a sophisticated entity-level role system that integrates perfectly with workflow approvals:

### EntityRole - Role Definitions
Defines available roles per entity type (e.g., "Opportunity Manager", "Senior Manager")
- `EntityType` - Which entity (Opportunity, Partner, etc.)
- `Code` - Unique identifier (e.g., "Opportunity_Manager")
- `IsInternal` - Internal UNOPS users vs External contacts
- `AllowsMultiple` - Can multiple people have this role?

### EntityRolePerson - Role Assignments
Assigns specific users/contacts to roles for specific entity instances:
- `EntityId` + `EntityType` - The specific Opportunity/Partner
- `UserId` - For internal UNOPS users
- `ContactId` - For external contacts
- `EffectiveDate` / `EndDate` - Time-bound role assignments!

### EntityUserRole - Organization-Level Roles
Maps users to organizational units with delegation authority:
- Used for organizational hierarchy approvals
- Director, DoA1, DoA2 roles for approval escalation

**This means:** The workflow migration can leverage PAO's existing role assignments for approval workflows without rebuilding a permission system!

---

## Prerequisites

Before starting:
- [ ] **CRITICAL:** Backup your database
- [ ] Ensure you have access to `https://github.com/UNOPS-ITG/unops-workflow.git`
- [ ] Review current WorkflowStage usage in PAO
- [ ] Review EntityRole and EntityRolePerson tables - these will be used for approvals
- [ ] Test in development environment first
- [ ] Allocate 1-2 days for this migration

---

## Migration Phases

### Phase 1: Add UNOPS.Workflow Submodule
**Estimated Time:** 15 minutes

Add the workflow submodule to your repository:

```bash
cd business-partners-and-opportunities
git submodule add https://github.com/UNOPS-ITG/unops-workflow.git UNOPS.Workflow
git submodule update --init --recursive
```

**Verify the submodule structure:**
```bash
ls UNOPS.Workflow/
# Should see:
# - UNOPS.Workflow.Business/
# - UNOPS.Workflow.DataAccess/
# - UNOPS.Workflow.Domain/
# - UNOPS.Workflow.Models/
# - unops-workflow-angular/
# - README.md
```

**Commit the submodule:**
```bash
git add .gitmodules UNOPS.Workflow
git commit -m "Add UNOPS.Workflow submodule"
```

---

### Phase 2: Add Project References
**Estimated Time:** 15 minutes

#### 2.1 Update UNOPS.PAO.Business.csproj

Add references to the workflow submodule projects:

```xml
<ItemGroup>
  <!-- Existing references -->
  
  <!-- Workflow submodule projects -->
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Business\UNOPS.Workflow.Business.csproj" />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.DataAccess\UNOPS.Workflow.DataAccess.csproj" />
  <ProjectReference Include="..\UNOPS.Workflow\UNOPS.Workflow.Models\UNOPS.Workflow.Models.csproj" />
</ItemGroup>
```

#### 2.2 Restore packages

```bash
dotnet restore
```

#### 2.3 Verify build

```bash
dotnet build
```

---

### Phase 3: Implement Required Interfaces
**Estimated Time:** 3-4 hours

The workflow submodule requires your application to implement 4 interfaces and create email templates. These allow the generic workflow engine to interact with PAO-specific logic.

#### 3.1 Create PaoWorkflowUserContext

**File:** `UNOPS.PAO.Business/Workflow/PaoWorkflowUserContext.cs`

```csharp
using UNOPS.Workflow.Business.Interfaces;

namespace UNOPS.PAO.Business.Workflow;

/// <summary>
/// Provides current user context for workflow operations.
/// Adapts PAO's identity system to the workflow submodule interface.
/// </summary>
public sealed class PaoWorkflowUserContext : IWorkflowUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _context;

    public PaoWorkflowUserContext(
        IHttpContextAccessor httpContextAccessor,
        AppDbContext context)
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
    }

    public int CurrentUserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return 0;

            var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
        }
    }

    public string CurrentUserName
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Identity?.Name ?? "Unknown";
        }
    }

    public string CurrentUserEmail
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var emailClaim = user?.FindFirst(System.Security.Claims.ClaimTypes.Email);
            return emailClaim?.Value ?? string.Empty;
        }
    }

    public IEnumerable<string> CurrentUserRoles
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value) ?? Enumerable.Empty<string>();
        }
    }

    public bool HasRole(string roleName)
    {
        return CurrentUserRoles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public string Environment
    {
        get
        {
            return System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                ?? "Production";
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }
    }
}
```

#### 3.2 Create PaoEntityStageProvider

**File:** `UNOPS.PAO.Business/Workflow/PaoEntityStageProvider.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using UNOPS.Workflow.Business.Interfaces;

namespace UNOPS.PAO.Business.Workflow;

/// <summary>
/// Provides entity stage information and updates for PAO entities.
/// Maps workflow operations to specific entity types (Opportunity, etc.).
/// </summary>
public sealed class PaoEntityStageProvider : IEntityStageProvider
{
    private readonly AppDbContext _context;

    public PaoEntityStageProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetCurrentStageAsync(string entityName, string entityId)
    {
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await GetOpportunityStageAsync(entityId),
            // Add other entity types here as needed
            _ => null
        };
    }

    public async Task<bool> UpdateStageAsync(
        string entityName, 
        string entityId, 
        string newStage, 
        int userId)
    {
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await UpdateOpportunityStageAsync(entityId, newStage, userId),
            // Add other entity types here as needed
            _ => false
        };
    }

    public async Task<bool> IsEntityValidAsync(string entityName, string entityId)
    {
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await IsOpportunityValidAsync(entityId),
            // Add other entity types here as needed
            _ => false
        };
    }

    public async Task<string> GetEntityDisplayNameAsync(string entityName, string entityId)
    {
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await GetOpportunityDisplayNameAsync(entityId),
            // Add other entity types here as needed
            _ => $"{entityName} {entityId}"
        };
    }

    // Private helper methods for Opportunity entity
    private async Task<string?> GetOpportunityStageAsync(string entityId)
    {
        if (!int.TryParse(entityId, out var id))
            return null;

        var opportunity = await _context.Opportunities
            .Where(o => o.Id == id && !o.IsDeleted)
            .Select(o => o.Stage)
            .FirstOrDefaultAsync();

        return opportunity;
    }

    private async Task<bool> UpdateOpportunityStageAsync(
        string entityId, 
        string newStage, 
        int userId)
    {
        if (!int.TryParse(entityId, out var id))
            return false;

        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (opportunity is null)
            return false;

        opportunity.Stage = newStage;
        opportunity.ModifiedBy = userId;
        opportunity.ModifiedOn = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> IsOpportunityValidAsync(string entityId)
    {
        if (!int.TryParse(entityId, out var id))
            return false;

        return await _context.Opportunities
            .AnyAsync(o => o.Id == id && !o.IsDeleted);
    }

    private async Task<string> GetOpportunityDisplayNameAsync(string entityId)
    {
        if (!int.TryParse(entityId, out var id))
            return $"Opportunity {entityId}";

        var name = await _context.Opportunities
            .Where(o => o.Id == id)
            .Select(o => o.Title)
            .FirstOrDefaultAsync();

        return name ?? $"Opportunity {entityId}";
    }
}
```

#### 3.3 Create PaoWorkflowNotificationService

**File:** `UNOPS.PAO.Business/Workflow/PaoWorkflowNotificationService.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.PAO.MailSender;
using UNOPS.PAO.MailSender.Interfaces;

namespace UNOPS.PAO.Business.Workflow;

/// <summary>
/// Sends workflow-related notifications using PAO's email infrastructure.
/// Uses PAO's existing UNOPS.PAO.MailSender template-based email system.
/// </summary>
public sealed class PaoWorkflowNotificationService : IWorkflowNotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<PaoWorkflowNotificationService> _logger;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public PaoWorkflowNotificationService(
        IEmailSender emailSender,
        ILogger<PaoWorkflowNotificationService> logger,
        AppDbContext context,
        IConfiguration configuration)
    {
        _emailSender = emailSender;
        _logger = logger;
        _context = context;
        _configuration = configuration;
    }

    public async Task NotifyNewApprovalRequestAsync(WorkflowNotification notification)
    {
        try
        {
            var recipientEmails = await GetUserEmailsAsync(notification.RecipientUserIds);
            
            if (!recipientEmails.Any())
                return;

            var emailMessage = new EmailMessage
            {
                TemplateName = "UNOPS.PAO.Business.EmailTemplates.WorkflowApprovalRequest.html",
                Title = $"Approval Required: {notification.EntityDisplayName}",
                EmailReceivers = recipientEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, new
            {
                EntityDisplayName = notification.EntityDisplayName,
                EntityUrl = notification.EntityUrl,
                PerformedByUserName = notification.PerformedByUserName,
                Action = notification.Action,
                FromStage = notification.FromStage,
                ToStage = notification.ToStage,
                Comment = notification.Comment,
                Timestamp = notification.Timestamp,
                CurrentYear = DateTime.UtcNow.Year
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval notification for {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    public async Task NotifyWorkflowCompletedAsync(WorkflowNotification notification)
    {
        try
        {
            var recipientEmails = await GetUserEmailsAsync(notification.RecipientUserIds);
            
            if (!recipientEmails.Any())
                return;

            var emailMessage = new EmailMessage
            {
                TemplateName = "UNOPS.PAO.Business.EmailTemplates.WorkflowCompleted.html",
                Title = $"Workflow Completed: {notification.EntityDisplayName}",
                EmailReceivers = recipientEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, new
            {
                EntityDisplayName = notification.EntityDisplayName,
                EntityUrl = notification.EntityUrl,
                Action = notification.Action,
                NewStage = notification.ToStage,
                Timestamp = notification.Timestamp,
                CurrentYear = DateTime.UtcNow.Year
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send completion notification for {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    public async Task NotifyWorkflowRejectedAsync(WorkflowNotification notification)
    {
        try
        {
            var recipientEmails = await GetUserEmailsAsync(notification.RecipientUserIds);
            
            if (!recipientEmails.Any())
                return;

            var emailMessage = new EmailMessage
            {
                TemplateName = "UNOPS.PAO.Business.EmailTemplates.WorkflowRejected.html",
                Title = $"Workflow Rejected: {notification.EntityDisplayName}",
                EmailReceivers = recipientEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, new
            {
                EntityDisplayName = notification.EntityDisplayName,
                EntityUrl = notification.EntityUrl,
                RejectedByUserName = notification.PerformedByUserName,
                Action = notification.Action,
                Reason = notification.Comment,
                Timestamp = notification.Timestamp,
                CurrentYear = DateTime.UtcNow.Year
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send rejection notification for {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    public async Task NotifyWorkflowRecalledAsync(WorkflowNotification notification)
    {
        try
        {
            var recipientEmails = await GetUserEmailsAsync(notification.RecipientUserIds);
            
            if (!recipientEmails.Any())
                return;

            var emailMessage = new EmailMessage
            {
                TemplateName = "UNOPS.PAO.Business.EmailTemplates.WorkflowRecalled.html",
                Title = $"Workflow Recalled: {notification.EntityDisplayName}",
                EmailReceivers = recipientEmails.ToArray()
            };

            await _emailSender.SendEmailAsync(emailMessage, new
            {
                EntityDisplayName = notification.EntityDisplayName,
                EntityUrl = notification.EntityUrl,
                RecalledByUserName = notification.PerformedByUserName,
                Action = notification.Action,
                Timestamp = notification.Timestamp,
                CurrentYear = DateTime.UtcNow.Year
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send recall notification for {EntityName} {EntityId}",
                notification.EntityName, notification.EntityId);
        }
    }

    private async Task<List<string>> GetUserEmailsAsync(List<int> userIds)
    {
        return await _context.PAOUsers
            .Where(u => userIds.Contains(u.Id))
            .Select(u => u.Email ?? string.Empty)
            .Where(e => !string.IsNullOrEmpty(e))
            .ToListAsync();
    }
}
```

#### 3.4 Create PaoWorkflowApproverProvider

**File:** `UNOPS.PAO.Business/Workflow/PaoWorkflowApproverProvider.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Models;

namespace UNOPS.PAO.Business.Workflow;

/// <summary>
/// Provides entity-specific workflow approvers and approval configuration.
/// Leverages PAO's existing EntityRole and EntityRolePerson system.
/// </summary>
public sealed class PaoWorkflowApproverProvider : IWorkflowApproverProvider
{
    private readonly AppDbContext _context;

    public PaoWorkflowApproverProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<WorkflowApproverModel>> GetApproversAsync(
        string entityName, 
        int entityId, 
        string fromStage, 
        string toStage)
    {
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await GetOpportunityApproversAsync(entityId, fromStage, toStage),
            _ => new List<WorkflowApproverModel>()
        };
    }

    public async Task<(List<WorkflowTaskModel> approvals, string[] roles)?> GetApprovalConfigurationAsync(
        string entityName, 
        int entityId, 
        string fromStage, 
        string toStage)
    {
        // PAO-specific approval configuration logic
        // For now, return null to use default workflow behavior
        return null;
    }

    public async Task<(List<WorkflowTaskModel> triggers, string[] roles)?> GetTriggerConfigurationAsync(
        string entityName, 
        int entityId, 
        string fromStage, 
        string toStage)
    {
        // PAO-specific trigger configuration logic
        // For now, return null to use default workflow behavior
        return null;
    }

    public async Task<bool> CanUserApproveAsync(
        string entityName, 
        int entityId, 
        int userId, 
        string fromStage, 
        string toStage)
    {
        return entityName.ToLowerInvariant() switch
        {
            "opportunity" => await CanUserApproveOpportunityAsync(entityId, userId, fromStage, toStage),
            _ => false
        };
    }

    // Private helper methods for Opportunity entity
    private async Task<List<WorkflowApproverModel>> GetOpportunityApproversAsync(
        int opportunityId, 
        string fromStage, 
        string toStage)
    {
        // Use PAO's EntityRolePerson to find users assigned to this opportunity
        // with roles that can approve (e.g., "Opportunity Manager", "Senior Manager")
        
        var approverRoleCodes = new[] 
        { 
            "Opportunity_Manager",      // Adjust based on actual EntityRole.Code values
            "Senior_Manager",
            "Director"
        };

        var approvers = await _context.EntityRolePersons
            .Where(erp => 
                erp.EntityType == "Opportunity" &&
                erp.EntityId == opportunityId &&
                !erp.IsDeleted &&
                erp.UserId != null &&  // Only internal users can approve
                (erp.EndDate == null || erp.EndDate > DateTime.UtcNow) && // Still active
                erp.EntityRole != null &&
                approverRoleCodes.Contains(erp.EntityRole.Code ?? ""))
            .Include(erp => erp.User)
            .Include(erp => erp.EntityRole)
            .Select(erp => new WorkflowApproverModel
            {
                UserId = erp.UserId ?? 0,
                UserName = erp.User!.Name ?? "Unknown",
                Email = erp.User.Email ?? string.Empty,
                CanApprove = true
            })
            .Distinct()
            .ToListAsync();

        // If no specific approvers found, fall back to organizational hierarchy
        if (!approvers.Any())
        {
            approvers = await GetApproversFromOrgHierarchy(opportunityId);
        }

        return approvers;
    }

    private async Task<List<WorkflowApproverModel>> GetApproversFromOrgHierarchy(int opportunityId)
    {
        // Fallback: Get approvers based on the opportunity's organizational unit
        // Using PAO's existing organizational hierarchy
        
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId);

        if (opportunity?.OrganizationUnitId == null)
            return new List<WorkflowApproverModel>();

        // Find users with approval roles in the org unit hierarchy
        var orgUnitApprovers = await _context.EntityUserRoles
            .Where(eur => 
                eur.EntityType == "OrganizationUnit" &&
                eur.EntityId == opportunity.OrganizationUnitId &&
                !eur.IsDeleted &&
                eur.EntityRole != null &&
                (eur.EntityRole.Code == "Director" || 
                 eur.EntityRole.Code == "DoA1" ||
                 eur.EntityRole.Code == "DoA2"))
            .Include(eur => eur.User)
            .Select(eur => new WorkflowApproverModel
            {
                UserId = eur.UserId,
                UserName = eur.User!.Name ?? "Unknown",
                Email = eur.User.Email ?? string.Empty,
                CanApprove = true
            })
            .ToListAsync();

        return orgUnitApprovers;
    }

    private async Task<bool> CanUserApproveOpportunityAsync(
        int opportunityId, 
        int userId, 
        string fromStage, 
        string toStage)
    {
        // Check if user has an approval role for this specific opportunity
        var hasApprovalRole = await _context.EntityRolePersons
            .AnyAsync(erp => 
                erp.EntityType == "Opportunity" &&
                erp.EntityId == opportunityId &&
                erp.UserId == userId &&
                !erp.IsDeleted &&
                (erp.EndDate == null || erp.EndDate > DateTime.UtcNow) &&
                erp.EntityRole != null &&
                (erp.EntityRole.Code == "Opportunity_Manager" ||
                 erp.EntityRole.Code == "Senior_Manager" ||
                 erp.EntityRole.Code == "Director"));

        if (hasApprovalRole)
            return true;

        // Check organizational hierarchy permissions
        var opportunity = await _context.Opportunities
            .FirstOrDefaultAsync(o => o.Id == opportunityId);

        if (opportunity?.OrganizationUnitId == null)
            return false;

        var hasOrgUnitApprovalRole = await _context.EntityUserRoles
            .AnyAsync(eur => 
                eur.EntityType == "OrganizationUnit" &&
                eur.EntityId == opportunity.OrganizationUnitId &&
                eur.UserId == userId &&
                !eur.IsDeleted &&
                eur.EntityRole != null &&
                (eur.EntityRole.Code == "Director" || 
                 eur.EntityRole.Code == "DoA1" ||
                 eur.EntityRole.Code == "DoA2"));

        return hasOrgUnitApprovalRole;
    }
}
```

#### 3.5 Create Email Templates for Workflow Notifications

PAO uses template-based emails. Create HTML email templates for workflow notifications.

**Directory:** `UNOPS.PAO.Business/EmailTemplates/`

Create these template files:

**File:** `WorkflowApprovalRequest.html`
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #0072C6; color: white; padding: 20px; text-align: center; }
        .content { background-color: #f9f9f9; padding: 20px; margin-top: 20px; }
        .button { display: inline-block; padding: 12px 24px; background-color: #0072C6; color: white; text-decoration: none; border-radius: 4px; margin-top: 20px; }
        .footer { margin-top: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Approval Required</h1>
        </div>
        <div class="content">
            <p><strong>{{PerformedByUserName}}</strong> has requested approval for:</p>
            <ul>
                <li><strong>Entity:</strong> {{EntityDisplayName}}</li>
                <li><strong>Action:</strong> {{Action}}</li>
                <li><strong>From Stage:</strong> {{FromStage}}</li>
                <li><strong>To Stage:</strong> {{ToStage}}</li>
            </ul>
            {{#if Comment}}
            <p><strong>Comment:</strong> {{Comment}}</p>
            {{/if}}
            <a href="{{EntityUrl}}" class="button">View and Approve</a>
        </div>
        <div class="footer">
            <p>&copy; {{CurrentYear}} UNOPS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
```

**File:** `WorkflowCompleted.html`
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #28a745; color: white; padding: 20px; text-align: center; }
        .content { background-color: #f9f9f9; padding: 20px; margin-top: 20px; }
        .button { display: inline-block; padding: 12px 24px; background-color: #28a745; color: white; text-decoration: none; border-radius: 4px; margin-top: 20px; }
        .footer { margin-top: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Workflow Completed</h1>
        </div>
        <div class="content">
            <p>Your workflow request has been completed:</p>
            <ul>
                <li><strong>Entity:</strong> {{EntityDisplayName}}</li>
                <li><strong>Action:</strong> {{Action}}</li>
                <li><strong>New Stage:</strong> {{NewStage}}</li>
            </ul>
            <a href="{{EntityUrl}}" class="button">View Details</a>
        </div>
        <div class="footer">
            <p>&copy; {{CurrentYear}} UNOPS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
```

**File:** `WorkflowRejected.html`
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #dc3545; color: white; padding: 20px; text-align: center; }
        .content { background-color: #f9f9f9; padding: 20px; margin-top: 20px; }
        .button { display: inline-block; padding: 12px 24px; background-color: #dc3545; color: white; text-decoration: none; border-radius: 4px; margin-top: 20px; }
        .footer { margin-top: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Workflow Rejected</h1>
        </div>
        <div class="content">
            <p>Your workflow request has been rejected:</p>
            <ul>
                <li><strong>Entity:</strong> {{EntityDisplayName}}</li>
                <li><strong>Action:</strong> {{Action}}</li>
                <li><strong>Rejected By:</strong> {{RejectedByUserName}}</li>
            </ul>
            {{#if Reason}}
            <p><strong>Reason:</strong> {{Reason}}</p>
            {{/if}}
            <a href="{{EntityUrl}}" class="button">View Details</a>
        </div>
        <div class="footer">
            <p>&copy; {{CurrentYear}} UNOPS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
```

**File:** `WorkflowRecalled.html`
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #ffc107; color: #333; padding: 20px; text-align: center; }
        .content { background-color: #f9f9f9; padding: 20px; margin-top: 20px; }
        .button { display: inline-block; padding: 12px 24px; background-color: #ffc107; color: #333; text-decoration: none; border-radius: 4px; margin-top: 20px; }
        .footer { margin-top: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>Workflow Recalled</h1>
        </div>
        <div class="content">
            <p>A workflow request has been recalled by <strong>{{RecalledByUserName}}</strong>:</p>
            <ul>
                <li><strong>Entity:</strong> {{EntityDisplayName}}</li>
                <li><strong>Action:</strong> {{Action}}</li>
            </ul>
            <a href="{{EntityUrl}}" class="button">View Details</a>
        </div>
        <div class="footer">
            <p>&copy; {{CurrentYear}} UNOPS. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
```

**Important:** Mark these files as **Embedded Resources** in the `.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="EmailTemplates\*.html" />
</ItemGroup>
```

This allows PAO's `IEmailTemplateRenderer` to load the templates at runtime.

---

### Phase 4: Register Workflow Services
**Estimated Time:** 30 minutes

#### 4.1 Create WorkflowServiceExtensions

**File:** `UNOPS.PAO.Business/Workflow/WorkflowServiceExtensions.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.DataAccess;

namespace UNOPS.PAO.Business.Workflow;

/// <summary>
/// Extension methods for registering PAO workflow services.
/// Follows the same pattern as GMS.
/// </summary>
public static class WorkflowServiceExtensions
{
    public static IServiceCollection AddPaoWorkflowServices(
        this IServiceCollection services,
        string connectionString)
    {
        // Register the submodule's core services (WorkflowDbContext, IWorkflowManager, IWorkflowRepository)
        // This automatically creates the workflow schema and applies migrations
        services.AddWorkflowServices(options =>
        {
            options.UsePostgreSqlStorage(connectionString, "workflow");
        });

        // Register PAO-specific implementations
        services.AddScoped<IWorkflowUserContext, PaoWorkflowUserContext>();
        services.AddScoped<IWorkflowNotificationService, PaoWorkflowNotificationService>();
        services.AddScoped<IEntityStageProvider, PaoEntityStageProvider>();
        services.AddScoped<IWorkflowApproverProvider, PaoWorkflowApproverProvider>();

        return services;
    }
}
```

#### 4.2 Update Program.cs (or Startup.cs)

Add the workflow services registration:

```csharp
// After existing service registrations
builder.Services.AddPaoWorkflowServices(
    builder.Configuration.GetConnectionString("DefaultConnection")
);
```

**Important:** The submodule will automatically:
1. Create the `workflow` schema in PostgreSQL
2. Apply EF Core migrations to create tables
3. Register `WorkflowDbContext`, `IWorkflowManager`, and `IWorkflowRepository`

---

### Phase 5: Update Opportunity Entity
**Estimated Time:** 30 minutes

#### 5.1 Modify Opportunity.cs

Replace `WorkflowStageId` foreign key with a `Stage` string property:

```csharp
public class Opportunity : ModifiableDeletableEntity
{
    // ... other properties ...

    /// <summary>
    /// Current workflow stage of the opportunity.
    /// Valid values defined in OpportunityWorkflow.StateMachine.
    /// </summary>
    [MaxLength(100)]
    public string Stage { get; set; } = "IdentifyAndProfile";

    // REMOVE these properties:
    // public int? WorkflowStageId { get; set; }
    // public WorkflowStage? WorkflowStage { get; set; }

    // ... rest of properties ...
}
```

---

### Phase 6: Define OpportunityWorkflow StateMachine
**Estimated Time:** 1 hour

Create a code-based workflow definition for Opportunity.

**File:** `UNOPS.PAO.Business/Workflow/OpportunityWorkflow.cs`

```csharp
using UNOPS.Workflow.Models;

namespace UNOPS.PAO.Business.Workflow;

/// <summary>
/// Defines the workflow state machine for Opportunity entities.
/// This is the "code-driven" part of the hybrid workflow approach.
/// </summary>
public static class OpportunityWorkflow
{
    public const string EntityName = "Opportunity";

    /// <summary>
    /// Stage codes (used in database)
    /// </summary>
    public static class Stages
    {
        public const string IdentifyAndProfile = "IdentifyAndProfile";
        public const string QualifyAndValidate = "QualifyAndValidate";
        public const string NegotiateAndClose = "NegotiateAndClose";
        public const string Complete = "Complete";
        public const string Abandoned = "Abandoned";
    }

    /// <summary>
    /// Action names (displayed on buttons)
    /// </summary>
    public static class Actions
    {
        public const string Submit = "Submit";
        public const string Qualify = "Qualify";
        public const string Negotiate = "Negotiate";
        public const string Close = "Close";
        public const string Abandon = "Abandon";
        public const string ReOpen = "Re-Open";
    }

    /// <summary>
    /// The StateMachine definition - used by the workflow engine
    /// </summary>
    public static readonly StateMachine StateMachine = new()
    {
        EntityType = EntityName,
        Stage = Stages.IdentifyAndProfile, // Default stage
        States = new[]
        {
            new State
            {
                StageCode = Stages.IdentifyAndProfile,
                Name = "Identify and Profile",
                Sequence = 1,
                Facing = Facing.Internal,
                Actions = new[]
                {
                    new StateAction
                    {
                        ActionName = Actions.Submit,
                        NewStage = Stages.QualifyAndValidate,
                        Sequence = 1,
                        CommentRequired = false,
                        Facing = Facing.Internal
                    },
                    new StateAction
                    {
                        ActionName = Actions.Abandon,
                        NewStage = Stages.Abandoned,
                        Sequence = 2,
                        CommentRequired = true,
                        Facing = Facing.Internal
                    }
                }
            },
            new State
            {
                StageCode = Stages.QualifyAndValidate,
                Name = "Qualify and Validate",
                Sequence = 2,
                Facing = Facing.TwoFace,
                Actions = new[]
                {
                    new StateAction
                    {
                        ActionName = Actions.Qualify,
                        NewStage = Stages.NegotiateAndClose,
                        Sequence = 1,
                        CommentRequired = false,
                        Facing = Facing.Internal
                    },
                    new StateAction
                    {
                        ActionName = Actions.Abandon,
                        NewStage = Stages.Abandoned,
                        Sequence = 2,
                        CommentRequired = true,
                        Facing = Facing.Internal
                    }
                }
            },
            new State
            {
                StageCode = Stages.NegotiateAndClose,
                Name = "Negotiate and Close",
                Sequence = 3,
                Facing = Facing.TwoFace,
                Actions = new[]
                {
                    new StateAction
                    {
                        ActionName = Actions.Close,
                        NewStage = Stages.Complete,
                        Sequence = 1,
                        CommentRequired = false,
                        Facing = Facing.Internal
                    },
                    new StateAction
                    {
                        ActionName = Actions.Abandon,
                        NewStage = Stages.Abandoned,
                        Sequence = 2,
                        CommentRequired = true,
                        Facing = Facing.Internal
                    }
                }
            },
            new State
            {
                StageCode = Stages.Complete,
                Name = "Complete",
                Sequence = 4,
                Facing = Facing.TwoFace,
                Actions = Array.Empty<StateAction>() // Terminal state
            },
            new State
            {
                StageCode = Stages.Abandoned,
                Name = "Abandoned",
                Sequence = 5,
                Facing = Facing.Internal,
                Actions = new[]
                {
                    new StateAction
                    {
                        ActionName = Actions.ReOpen,
                        NewStage = Stages.IdentifyAndProfile,
                        Sequence = 1,
                        CommentRequired = false,
                        Facing = Facing.Internal
                    }
                }
            }
        }
    };
}
```

---

### Phase 7: Create Database Migration for Opportunity Entity
**Estimated Time:** 30 minutes

Create a migration to add the `Stage` column and migrate data from `WorkflowStage`.

```bash
cd UNOPS.PAO.DataAccess
dotnet ef migrations add AddStageToOpportunity
```

#### 7.1 Edit the generated migration

Modify the migration to:
1. Add `Stage` column
2. Migrate existing data from `WorkflowStage` to `Stage`
3. Make `WorkflowStageId` nullable (don't drop yet)

```csharp
public partial class AddStageToOpportunity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Add Stage column (nullable initially)
        migrationBuilder.AddColumn<string>(
            name: "Stage",
            table: "Opportunities",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        // 2. Migrate data from WorkflowStage to Stage
        migrationBuilder.Sql(@"
            UPDATE ""Opportunities"" o
            SET ""Stage"" = 
                CASE ws.""Name""
                    WHEN 'Identify and Profile' THEN 'IdentifyAndProfile'
                    WHEN 'Qualify and Validate' THEN 'QualifyAndValidate'
                    WHEN 'Negotiate and Close' THEN 'NegotiateAndClose'
                    WHEN 'Complete' THEN 'Complete'
                    WHEN 'Abandoned' THEN 'Abandoned'
                    ELSE 'IdentifyAndProfile'
                END
            FROM ""WorkflowStages"" ws
            WHERE o.""WorkflowStageId"" = ws.""Id""
        ");

        // 3. Set default for records without WorkflowStageId
        migrationBuilder.Sql(@"
            UPDATE ""Opportunities""
            SET ""Stage"" = 'IdentifyAndProfile'
            WHERE ""Stage"" IS NULL
        ");

        // 4. Make Stage non-nullable
        migrationBuilder.AlterColumn<string>(
            name: "Stage",
            table: "Opportunities",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false);

        // 5. Make WorkflowStageId nullable (prepare for removal)
        migrationBuilder.AlterColumn<int>(
            name: "WorkflowStageId",
            table: "Opportunities",
            nullable: true,
            oldClrType: typeof(int),
            oldNullable: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse migration
        migrationBuilder.DropColumn(
            name: "Stage",
            table: "Opportunities");

        migrationBuilder.AlterColumn<int>(
            name: "WorkflowStageId",
            table: "Opportunities",
            nullable: false,
            oldClrType: typeof(int),
            oldNullable: true);
    }
}
```

#### 7.2 Apply the migration

```bash
dotnet ef database update
```

**Verify the data migration:**
```sql
SELECT "Stage", "WorkflowStageId", COUNT(*)
FROM "Opportunities"
GROUP BY "Stage", "WorkflowStageId";
```

---

### Phase 8: Seed Workflow Transition Data
**Estimated Time:** 1 hour

**First, identify your actual EntityRole codes:**

Before creating the seeder, check what role codes PAO uses:

```sql
-- Query PAO's EntityRole table
SELECT "Id", "Code", "Name", "EntityType", "IsInternal", "AllowsMultiple"
FROM "EntityRoles"
WHERE "EntityType" = 'Opportunity'
  AND "IsDeleted" = false
ORDER BY "Code";

-- Check which roles are actually assigned to opportunities
SELECT DISTINCT er."Code", er."Name", COUNT(*) as "AssignmentCount"
FROM "EntityRolePersons" erp
JOIN "EntityRoles" er ON er."Id" = erp."EntityRoleId"
WHERE erp."EntityType" = 'Opportunity'
  AND erp."IsDeleted" = false
  AND (erp."EndDate" IS NULL OR erp."EndDate" > CURRENT_TIMESTAMP)
GROUP BY er."Code", er."Name"
ORDER BY "AssignmentCount" DESC;
```

**Then create the seeder using those actual role codes:**

The workflow submodule provides a base seeder class. Create a seeder to populate the transition rules.

**File:** `UNOPS.PAO.DataAccess/Seeders/OpportunityWorkflowSeeder.cs`

```csharp
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.DataAccess.Seeders;
using UNOPS.Workflow.Domain.Entities;
using UNOPS.PAO.Business.Workflow;

namespace UNOPS.PAO.DataAccess.Seeders;

/// <summary>
/// Seeds workflow transition data for Opportunity workflows.
/// Run this once after adding the submodule.
/// </summary>
public sealed class OpportunityWorkflowSeeder : WorkflowSeederBase
{
    public OpportunityWorkflowSeeder(WorkflowDbContext context) : base(context)
    {
    }

    public void Seed()
    {
        var entityName = OpportunityWorkflow.EntityName;

        // Define all valid transitions for Opportunity
        var transitions = new[]
        {
            // From IdentifyAndProfile
            new StateMachineStageChange
            {
                EntityName = entityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.QualifyAndValidate,
                Name = OpportunityWorkflow.Actions.Submit,
                CommentRequired = false,
                CommentOptional = true,
                ApprovalRequired = false,
                Internal = true,
                External = false
            },
            new StateMachineStageChange
            {
                EntityName = entityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.Abandoned,
                Name = OpportunityWorkflow.Actions.Abandon,
                CommentRequired = true,
                CommentOptional = false,
                ApprovalRequired = false,
                Internal = true,
                External = false
            },

            // From QualifyAndValidate
            new StateMachineStageChange
            {
                EntityName = entityName,
                FromStage = OpportunityWorkflow.Stages.QualifyAndValidate,
                ToStage = OpportunityWorkflow.Stages.NegotiateAndClose,
                Name = OpportunityWorkflow.Actions.Qualify,
                CommentRequired = false,
                CommentOptional = true,
                ApprovalRequired = false,
                Internal = true,
                External = true
            },
            new StateMachineStageChange
            {
                EntityName = entityName,
                FromStage = OpportunityWorkflow.Stages.QualifyAndValidate,
                ToStage = OpportunityWorkflow.Stages.Abandoned,
                Name = OpportunityWorkflow.Actions.Abandon,
                CommentRequired = true,
                CommentOptional = false,
                ApprovalRequired = false,
                Internal = true,
                External = false
            },

            // From NegotiateAndClose
            new StateMachineStageChange
            {
                EntityName = entityName,
                FromStage = OpportunityWorkflow.Stages.NegotiateAndClose,
                ToStage = OpportunityWorkflow.Stages.Complete,
                Name = OpportunityWorkflow.Actions.Close,
                CommentRequired = false,
                CommentOptional = true,
                ApprovalRequired = true, // Requires approval!
                Internal = true,
                External = true
            },
            new StateMachineStageChange
            {
                EntityName = entityName,
                FromStage = OpportunityWorkflow.Stages.NegotiateAndClose,
                ToStage = OpportunityWorkflow.Stages.Abandoned,
                Name = OpportunityWorkflow.Actions.Abandon,
                CommentRequired = true,
                CommentOptional = false,
                ApprovalRequired = false,
                Internal = true,
                External = false
            },

            // From Abandoned
            new StateMachineStageChange
            {
                EntityName = entityName,
                FromStage = OpportunityWorkflow.Stages.Abandoned,
                ToStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                Name = OpportunityWorkflow.Actions.ReOpen,
                CommentRequired = false,
                CommentOptional = true,
                ApprovalRequired = false,
                Internal = true,
                External = false
            }
        };

        SeedTransitions(transitions);

        // Seed role permissions using PAO's actual EntityRole codes
        // IMPORTANT: Replace these with the actual role codes from your EntityRoles table!
        var rolePermissions = new[]
        {
            // Example: Opportunity_Manager can trigger transitions
            new StateMachineStageChangeRole
            {
                EntityType = entityName,
                FromStage = OpportunityWorkflow.Stages.IdentifyAndProfile,
                ToStage = OpportunityWorkflow.Stages.QualifyAndValidate,
                RoleName = "Opportunity_Manager",  // ← Use actual EntityRole.Code
                CanTrigger = true,
                CanApprove = false
            },
            
            // Example: Senior_Manager can approve closures
            new StateMachineStageChangeRole
            {
                EntityType = entityName,
                FromStage = OpportunityWorkflow.Stages.NegotiateAndClose,
                ToStage = OpportunityWorkflow.Stages.Complete,
                RoleName = "Senior_Manager",  // ← Use actual EntityRole.Code
                CanTrigger = false,
                CanApprove = true
            },
            
            // Example: Director can approve closures
            new StateMachineStageChangeRole
            {
                EntityType = entityName,
                FromStage = OpportunityWorkflow.Stages.NegotiateAndClose,
                ToStage = OpportunityWorkflow.Stages.Complete,
                RoleName = "Director",  // ← Use actual EntityRole.Code
                CanTrigger = false,
                CanApprove = true
            },
            
            // Add more role permissions based on your actual EntityRoles
            // Query: SELECT "Code", "Name" FROM "EntityRoles" WHERE "EntityType" = 'Opportunity'
        };

        SeedRolePermissions(rolePermissions);
    }
}
```

#### 8.1 Run the seeder

Add to your database initialization/seeding code:

```csharp
// In your database seeder or startup
public static void SeedWorkflowData(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
    
    var seeder = new OpportunityWorkflowSeeder(workflowContext);
    seeder.Seed();
}
```

**Important:** Adjust the `RoleName` values in `StateMachineStageChangeRole` to match your actual `EntityRole.Code` values from PAO's EntityRole table:

```sql
-- Check your actual entity role codes
SELECT "Code", "Name", "EntityType" 
FROM "EntityRoles" 
WHERE "EntityType" = 'Opportunity' 
  AND "IsInternal" = true
  AND "IsDeleted" = false;
```

Common PAO role codes:
- `Opportunity_Manager`
- `Senior_Manager`
- `Director`
- `DoA1`, `DoA2` (Delegation of Authority levels)

---

### Phase 9: Update WorkflowController
**Estimated Time:** 1 hour

Update your workflow API controller to use the workflow submodule's `IWorkflowManager`.

**File:** `UNOPS.PAO.Presentation/Controllers/WorkflowController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.PAO.Business.Workflow;

namespace UNOPS.PAO.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowManager _workflowManager;

    public WorkflowController(IWorkflowManager workflowManager)
    {
        _workflowManager = workflowManager;
    }

    /// <summary>
    /// Gets the workflow state machine for an entity type
    /// </summary>
    [HttpGet("{entityName}/state-machine")]
    public IActionResult GetStateMachine(string entityName)
    {
        var stateMachine = entityName.ToLowerInvariant() switch
        {
            "opportunity" => OpportunityWorkflow.StateMachine,
            _ => null
        };

        if (stateMachine is null)
            return NotFound($"No workflow found for entity: {entityName}");

        return Ok(stateMachine);
    }

    /// <summary>
    /// Gets available workflow actions for a specific entity instance
    /// </summary>
    [HttpGet("{entityName}/{entityId}/actions")]
    public async Task<IActionResult> GetAvailableActions(string entityName, string entityId)
    {
        try
        {
            var stateMachine = entityName.ToLowerInvariant() switch
            {
                "opportunity" => OpportunityWorkflow.StateMachine,
                _ => null
            };

            if (stateMachine is null)
                return NotFound($"No workflow found for entity: {entityName}");

            // Get available actions from the workflow manager
            // This queries the database for allowed transitions
            var actions = await _workflowManager.NextActions(
                entityName, 
                entityId, 
                stateMachine);

            return Ok(actions);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Executes a workflow action
    /// </summary>
    [HttpPost("{entityName}/{entityId}/execute")]
    public async Task<IActionResult> ExecuteAction(
        string entityName, 
        string entityId,
        [FromBody] WorkflowActionRequest request)
    {
        try
        {
            var stateMachine = entityName.ToLowerInvariant() switch
            {
                "opportunity" => OpportunityWorkflow.StateMachine,
                _ => null
            };

            if (stateMachine is null)
                return NotFound($"No workflow found for entity: {entityName}");

            var result = await _workflowManager.ChangeStateAsync(
                entityName,
                entityId,
                request.ActionName,
                stateMachine,
                request.Comment);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets workflow history for an entity
    /// </summary>
    [HttpGet("{entityName}/{entityId}/history")]
    public async Task<IActionResult> GetHistory(string entityName, string entityId)
    {
        try
        {
            var history = await _workflowManager.GetWorkflowHistoryAsync(
                entityName, 
                entityId);

            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets pending workflow approvals for an entity
    /// </summary>
    [HttpGet("{entityName}/{entityId}/pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals(string entityName, string entityId)
    {
        try
        {
            var details = await _workflowManager.WorkflowDetailsAsync(
                entityName, 
                entityId);

            return Ok(details);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Approves a pending workflow action
    /// </summary>
    [HttpPost("{entityName}/{entityId}/approve")]
    public async Task<IActionResult> Approve(
        string entityName, 
        string entityId,
        [FromBody] WorkflowApprovalRequest request)
    {
        try
        {
            var result = await _workflowManager.Approve(
                entityName,
                entityId,
                request.ActionName,
                request.Comment);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Rejects a pending workflow action
    /// </summary>
    [HttpPost("{entityName}/{entityId}/reject")]
    public async Task<IActionResult> Reject(
        string entityName, 
        string entityId,
        [FromBody] WorkflowApprovalRequest request)
    {
        try
        {
            var result = await _workflowManager.Reject(
                entityName,
                entityId,
                request.ActionName,
                request.Comment);

            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class WorkflowActionRequest
{
    public required string ActionName { get; set; }
    public string? Comment { get; set; }
}

public class WorkflowApprovalRequest
{
    public required string ActionName { get; set; }
    public required string Comment { get; set; }
}
```

---

### Phase 10: Update Angular Frontend
**Estimated Time:** 2-3 hours

The workflow submodule includes Angular components! You can use them directly or customize them.

#### 10.1 Install the Angular library

```bash
cd client-app
npm install ../UNOPS.Workflow/unops-workflow-angular
```

#### 10.2 Use the workflow component

Import and use in your Opportunity component:

```typescript
import { Component, OnInit, input } from '@angular/core';
import { WorkflowService, StageWorkflowComponent } from 'unops-workflow-angular';

@Component({
  selector: 'app-opportunity-detail',
  standalone: true,
  imports: [
    StageWorkflowComponent,  // Import the workflow component
    // ... other imports
  ],
  template: `
    <div class="opportunity-detail">
      <h2>{{ opportunity().title }}</h2>
      
      <!-- Use the pre-built workflow component -->
      <app-stage-workflow
        [entityName]="'Opportunity'"
        [entityId]="opportunity().id.toString()"
        [currentStage]="opportunity().stage"
        [canEdit]="canEditOpportunity()"
        (workflowChanged)="onWorkflowChanged($event)">
      </app-stage-workflow>
      
      <!-- Rest of your opportunity details -->
    </div>
  `
})
export class OpportunityDetailComponent implements OnInit {
  opportunityId = input.required<number>();
  
  // ... component logic
  
  onWorkflowChanged(event: any) {
    // Refresh opportunity data
    this.loadOpportunity();
  }
}
```

The `StageWorkflowComponent` provides:
- ✅ Display of current stage
- ✅ Available action buttons
- ✅ Comment input for actions requiring comments
- ✅ Approval workflow UI
- ✅ Workflow history timeline

#### 10.3 Or use the workflow service directly

```typescript
import { Component, OnInit } from '@angular/core';
import { WorkflowService } from 'unops-workflow-angular';

@Component({
  // ...
})
export class OpportunityDetailComponent implements OnInit {
  constructor(private workflowService: WorkflowService) {}

  async loadWorkflowActions() {
    const actions = await this.workflowService.getAvailableActions(
      'Opportunity',
      this.opportunityId().toString()
    );
    
    // Use actions in your custom UI
  }

  async executeWorkflowAction(actionName: string, comment?: string) {
    const result = await this.workflowService.executeAction(
      'Opportunity',
      this.opportunityId().toString(),
      actionName,
      comment
    );
    
    if (result.success) {
      // Handle success
    }
  }
}
```

---

### Phase 11: Remove Old WorkflowStage Code
**Estimated Time:** 1 hour

After verifying everything works, clean up the old WorkflowStage implementation.

#### 11.1 Create migration to drop WorkflowStage

```bash
cd UNOPS.PAO.DataAccess
dotnet ef migrations add RemoveWorkflowStage
```

Edit the migration:

```csharp
public partial class RemoveWorkflowStage : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Drop foreign key
        migrationBuilder.DropForeignKey(
            name: "FK_Opportunities_WorkflowStages_WorkflowStageId",
            table: "Opportunities");

        // 2. Drop index
        migrationBuilder.DropIndex(
            name: "IX_Opportunities_WorkflowStageId",
            table: "Opportunities");

        // 3. Drop column
        migrationBuilder.DropColumn(
            name: "WorkflowStageId",
            table: "Opportunities");

        // 4. Drop WorkflowStages table
        migrationBuilder.DropTable(
            name: "WorkflowStages");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse migration
        // ... (EF Core generates this)
    }
}
```

Apply the migration:

```bash
dotnet ef database update
```

#### 11.2 Delete old files

Remove these files (they're no longer needed):
- Old `WorkflowManager.cs` (if it existed)
- Old `WorkflowStage.cs` entity
- Old workflow-related services/interfaces

#### 11.3 Update AppDbContext

Remove `WorkflowStages` DbSet from `AppDbContext.cs`:

```csharp
// REMOVE:
// public DbSet<WorkflowStage> WorkflowStages { get; set; }
```

---

### Phase 12: Testing
**Estimated Time:** 2-3 hours

#### 12.1 Test workflow transitions

```bash
# Test getting available actions
curl http://localhost:5000/api/workflow/opportunity/1/actions

# Test executing an action
curl -X POST http://localhost:5000/api/workflow/opportunity/1/execute \
  -H "Content-Type: application/json" \
  -d '{"actionName": "Submit", "comment": "Ready for review"}'

# Test workflow history
curl http://localhost:5000/api/workflow/opportunity/1/history
```

#### 12.2 Test approval workflows

```bash
# Execute action requiring approval
curl -X POST http://localhost:5000/api/workflow/opportunity/1/execute \
  -H "Content-Type: application/json" \
  -d '{"actionName": "Close", "comment": "Opportunity completed successfully"}'

# Check pending approvals
curl http://localhost:5000/api/workflow/opportunity/1/pending-approvals

# Approve the action (as approver)
curl -X POST http://localhost:5000/api/workflow/opportunity/1/approve \
  -H "Content-Type: application/json" \
  -d '{"actionName": "Close", "comment": "Approved"}'
```

#### 12.3 Verify workflow logs

Check the `workflow.WorkflowLogs` table:

```sql
SELECT * FROM workflow."WorkflowLogs"
WHERE "EntityName" = 'Opportunity'
ORDER BY "CreatedOn" DESC;
```

#### 12.4 Test frontend workflow components

- [ ] Verify workflow actions display correctly
- [ ] Test executing actions with/without comments
- [ ] Verify workflow history displays
- [ ] Test approval workflows in UI
- [ ] Verify role-based action visibility

---

## Rollback Plan

If you need to rollback:

1. **Revert database migrations:**
   ```bash
   dotnet ef database update <previous-migration-name>
   ```

2. **Remove submodule:**
   ```bash
   git submodule deinit -f UNOPS.Workflow
   git rm -f UNOPS.Workflow
   rm -rf .git/modules/UNOPS.Workflow
   ```

3. **Revert code changes:**
   ```bash
   git revert <commit-hash>
   ```

---

## PAO-Specific Configuration Notes

### Email System Integration

PAO has a production-ready email infrastructure (`UNOPS.PAO.MailSender`) that uses:

**Components:**
- `IEmailSender` - Main email sending interface
- `EmailMessage` - Email definition with template name and recipients
- `IEmailTemplateRenderer` - Renders Handlebars templates with model data
- `SmtpEmailSender` - SMTP implementation with Google Cloud Secret Manager support

**Template System:**
PAO uses Handlebars (`.html` files) as embedded resources:
```csharp
var emailMessage = new EmailMessage
{
    TemplateName = "UNOPS.PAO.Business.EmailTemplates.WorkflowApprovalRequest.html",
    Title = "Approval Required",
    EmailReceivers = ["user@unops.org"]
};

await _emailSender.SendEmailAsync(emailMessage, new 
{ 
    UserName = "John Doe",
    EntityName = "Opportunity #123",
    // ... template model properties
});
```

**Configuration:**
Email settings in `appsettings.json`:
```json
{
  "EmailConfiguration": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "...",
    "EnableSsl": true
  }
}
```

**Already Registered:** PAO's email services are already registered in `Program.cs`, so you don't need to register them again - just inject `IEmailSender` in your workflow notification service!

### EntityRole Codes vs Names

**Critical:** The workflow system uses `EntityRole.Code` (not `Name`) for role matching!

```csharp
// ✅ CORRECT - Uses Code
var approverRoleCodes = new[] { "Opportunity_Manager", "Senior_Manager" };

// ❌ WRONG - Uses Name with spaces
var approverRoleCodes = new[] { "Opportunity Manager", "Senior Manager" };
```

**Verify your codes:**
```sql
SELECT "Code", "Name" FROM "EntityRoles" 
WHERE "EntityType" = 'Opportunity' AND "IsDeleted" = false;
```

### Time-Bound Role Assignments

PAO's `EntityRolePerson` supports time-bound roles via `EffectiveDate` and `EndDate`. The workflow integration automatically respects these:

```csharp
// Automatically filters for currently active roles
(erp.EndDate == null || erp.EndDate > DateTime.UtcNow)
```

This means:
- ✅ Users automatically gain approval rights when their role becomes active
- ✅ Users automatically lose approval rights when their role expires
- ✅ No manual permission management needed!

### Organizational Hierarchy Fallback

If no entity-specific approvers are found via `EntityRolePerson`, the system falls back to organizational hierarchy via `EntityUserRole`:

```csharp
// Looks for Director, DoA1, DoA2 in the opportunity's org unit
var orgUnitApprovers = await _context.EntityUserRoles
    .Where(eur => 
        eur.EntityType == "OrganizationUnit" &&
        eur.EntityId == opportunity.OrganizationUnitId &&
        (eur.EntityRole.Code == "Director" || ...))
```

This provides automatic escalation through your organizational structure!

---

## Troubleshooting

### Workflow schema not created

**Issue:** Tables not appearing in `workflow` schema

**Solution:**
```csharp
// Manually trigger schema creation
using var scope = app.Services.CreateScope();
var workflowContext = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();
workflowContext.EnsureWorkflowSchemaCreated();
```

### Transitions not appearing

**Issue:** `NextActions` returns empty

**Causes:**
1. Transition data not seeded
2. Current stage doesn't match any `FromStage`
3. User doesn't have permission

**Solution:**
- Re-run seeder
- Check `workflow.StateMachineStageChanges` table
- Verify role permissions in `workflow.StateMachineStageChangeRoles`

### Approval not working

**Issue:** Approval actions not triggering workflow completion

**Causes:**
1. `IWorkflowApproverProvider` not implemented correctly
2. User doesn't have `CanApprove` permission
3. EntityRole codes don't match

**Solution:**
- Check `GetApproversAsync` implementation
- Verify role permissions in database
- **Check EntityRole.Code values:**
  ```sql
  -- Verify the user has an active role assignment
  SELECT erp.*, er."Code", er."Name", u."Name" as "UserName"
  FROM "EntityRolePersons" erp
  JOIN "EntityRoles" er ON er."Id" = erp."EntityRoleId"
  JOIN "PAOUsers" u ON u."Id" = erp."UserId"
  WHERE erp."EntityType" = 'Opportunity'
    AND erp."EntityId" = [your_opportunity_id]
    AND erp."IsDeleted" = false
    AND (erp."EndDate" IS NULL OR erp."EndDate" > CURRENT_TIMESTAMP);
  ```

### No approvers found

**Issue:** `GetApproversAsync` returns empty list

**Causes:**
1. No EntityRolePerson records exist for the opportunity
2. All role assignments have expired (EndDate passed)
3. Role codes in code don't match database

**Solution:**
```sql
-- Check if ANY role assignments exist for this opportunity
SELECT COUNT(*) FROM "EntityRolePersons"
WHERE "EntityType" = 'Opportunity'
  AND "EntityId" = [your_opportunity_id]
  AND "IsDeleted" = false;

-- If zero, you need to assign users to the opportunity first!

-- Check what role codes exist vs what you're looking for
SELECT DISTINCT er."Code"
FROM "EntityRolePersons" erp
JOIN "EntityRoles" er ON er."Id" = erp."EntityRoleId"
WHERE erp."EntityType" = 'Opportunity'
  AND erp."IsDeleted" = false;
```

### Role code mismatch

**Issue:** Approvers not found even though roles are assigned

**Root Cause:** Your code is looking for `"Opportunity_Manager"` but the database has `"OpportunityManager"` or `"Opportunity Manager"`

**Solution:**
```csharp
// Update PaoWorkflowApproverProvider to use your actual codes
var approverRoleCodes = new[] 
{ 
    "Opportunity_Manager",  // ← Change to match your EntityRole.Code!
    "Senior_Manager",
    "Director"
};
```

**Verify:**
```sql
SELECT "Code", "Name" FROM "EntityRoles" 
WHERE "EntityType" = 'Opportunity' AND "IsDeleted" = false;
-- Copy the actual Code values and use them in your C# code!

---

## Performance Considerations

### Workflow Query Performance

The workflow submodule includes optimized indexes:

```sql
-- Automatically created by WorkflowDbContext
CREATE INDEX "IX_WorkflowLogs_Entity" 
  ON workflow."WorkflowLogs" ("EntityName", "EntityId");

CREATE INDEX "IX_WorkflowLogs_Pending" 
  ON workflow."WorkflowLogs" ("EntityName", "CompletedOn")
  WHERE "CompletedOn" IS NULL;

CREATE INDEX "IX_StateMachineStageChanges_Lookup" 
  ON workflow."StateMachineStageChanges" ("EntityName", "FromStage", "ToStage");
```

### Caching Recommendations

Consider caching:
- StateMachine definitions (code-based, rarely change)
- Available transitions (`StateMachineStageChanges` per entity type)
- User permissions (`StateMachineStageChangeRoles`)

**Example caching:**
```csharp
services.AddMemoryCache();

// In WorkflowController
private readonly IMemoryCache _cache;

public async Task<IActionResult> GetAvailableActions(string entityName, string entityId)
{
    var cacheKey = $"workflow-actions-{entityName}-{entityId}";
    
    if (!_cache.TryGetValue(cacheKey, out var actions))
    {
        actions = await _workflowManager.NextActions(entityName, entityId, stateMachine);
        _cache.Set(cacheKey, actions, TimeSpan.FromMinutes(5));
    }
    
    return Ok(actions);
}
```

---

## Security Considerations

### Authorization

The workflow submodule respects your ASP.NET Core authorization policies. Configure in `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageOpportunities", policy =>
        policy.RequireRole("Opportunity Manager", "Admin"));
    
    options.AddPolicy("CanApproveOpportunities", policy =>
        policy.RequireRole("Senior Manager", "Admin"));
});
```

Use in controller:

```csharp
[Authorize(Policy = "CanManageOpportunities")]
[HttpPost("{entityName}/{entityId}/execute")]
public async Task<IActionResult> ExecuteAction(...)
{
    // ...
}

[Authorize(Policy = "CanApproveOpportunities")]
[HttpPost("{entityName}/{entityId}/approve")]
public async Task<IActionResult> Approve(...)
{
    // ...
}
```

---

## Documentation Updates

After migration, update:

1. **README.md** - Document workflow integration
2. **API Documentation** - Update workflow endpoints
3. **User Guide** - Explain new workflow UI
4. **Developer Guide** - How to add workflows for new entities

---

## Estimated Timeline Summary

| Phase | Description | Time |
|-------|-------------|------|
| 1 | Add submodule | 15 min |
| 2 | Add project references | 15 min |
| 3 | Implement 4 interfaces + email templates | 3-4 hours |
| 4 | Register services | 30 min |
| 5 | Update Opportunity entity | 30 min |
| 6 | Define OpportunityWorkflow | 1 hour |
| 7 | Database migration | 30 min |
| 8 | Seed workflow data | 1 hour |
| 9 | Update controller | 1 hour |
| 10 | Update Angular frontend | 2-3 hours |
| 11 | Remove old code | 1 hour |
| 12 | Testing | 2-3 hours |
| **TOTAL** | **9-13 hours** | |

---

## Benefits Realized

After migration, PAO will have:

✅ **Production-Proven Workflow Engine** - Same system used by GMS
✅ **Zero Maintenance** - Workflow bugs fixed in submodule
✅ **Full Approval Workflows** - Multi-user approval out of the box
✅ **Complete Audit Trail** - Every workflow action logged
✅ **Role-Based Permissions** - Fine-grained access control using **existing EntityRole system**
✅ **Runtime Configuration** - Change transitions without deployment
✅ **Pre-Built UI Components** - Angular components included
✅ **Extensible** - Easy to add workflows for new entities
✅ **Leverages Existing Roles** - No need to rebuild permission system, uses EntityRolePerson!
✅ **Time-Bound Approvals** - Respects EntityRolePerson EffectiveDate/EndDate
✅ **Org Hierarchy Fallback** - Can escalate approvals through organizational structure
✅ **Production-Ready Email System** - PAO's existing `UNOPS.PAO.MailSender` handles all notifications
✅ **Template-Based Notifications** - Professional email templates with consistent branding

---

## Assigning Users to Opportunities for Workflow Approvals

Before workflow approvals work, users must be assigned to opportunities with appropriate roles via `EntityRolePerson`:

### Manual Assignment (SQL)
```sql
-- Assign a user as Opportunity Manager for a specific opportunity
INSERT INTO "EntityRolePersons" 
(
    "EntityType", 
    "EntityId", 
    "EntityRoleId", 
    "UserId", 
    "EffectiveDate",
    "CreatedOn",
    "IsDeleted"
)
SELECT 
    'Opportunity',
    123,  -- Opportunity ID
    er."Id",
    456,  -- User ID
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP,
    false
FROM "EntityRoles" er
WHERE er."Code" = 'Opportunity_Manager'  -- Use actual role code
  AND er."EntityType" = 'Opportunity';
```

### Programmatic Assignment (C#)
```csharp
public async Task AssignOpportunityManagerAsync(int opportunityId, int userId)
{
    var managerRole = await _context.EntityRoles
        .FirstOrDefaultAsync(er => 
            er.EntityType == "Opportunity" && 
            er.Code == "Opportunity_Manager");

    if (managerRole == null)
        throw new Exception("Opportunity_Manager role not found");

    var assignment = new EntityRolePerson
    {
        EntityType = "Opportunity",
        EntityId = opportunityId,
        EntityRoleId = managerRole.Id,
        UserId = userId,
        EffectiveDate = DateTime.UtcNow,
        CreatedOn = DateTime.UtcNow,
        IsDeleted = false
    };

    _context.EntityRolePersons.Add(assignment);
    await _context.SaveChangesAsync();
}
```

### Time-Bound Assignment
```csharp
// Assign temporary approver (e.g., while regular manager on leave)
var tempAssignment = new EntityRolePerson
{
    EntityType = "Opportunity",
    EntityId = opportunityId,
    EntityRoleId = seniorManagerRoleId,
    UserId = temporaryApproverId,
    EffectiveDate = DateTime.UtcNow,
    EndDate = DateTime.UtcNow.AddDays(14),  // Active for 2 weeks
    CreatedOn = DateTime.UtcNow,
    IsDeleted = false
};
```

---

## Next Steps After Migration

1. **Assign users to opportunities** - Use EntityRolePerson for approval permissions
2. **Add workflows for other entities** (Business Partners, Projects, etc.)
3. **Configure email notifications** - Customize notification templates
4. **Add more granular role permissions** - Per-entity-type permissions
5. **Implement workflow analytics** - Dashboard for workflow metrics
6. **Configure approval hierarchies** - Multi-level approvals
7. **Add workflow automation** - Auto-transition based on conditions

---

## Support

For issues with:
- **Workflow submodule** - Check `UNOPS.Workflow/README.md` or contact workflow team
- **PAO integration** - This guide or PAO team
- **GMS examples** - Review `business-gms-plus` for reference implementations

---

## Best Practices: EntityRole with Workflows

### 1. Define Clear Role Codes
Use consistent, descriptive codes for entity roles:
```sql
-- Good: Clear, consistent naming
Code = 'Opportunity_Manager'
Code = 'Senior_Manager'  
Code = 'Director'

-- Bad: Inconsistent, unclear
Code = 'OppMgr'
Code = 'seniormanager'
Code = 'Dir'
```

### 2. Separate Trigger vs Approve Permissions
Use `StateMachineStageChangeRole` to distinguish who can initiate vs approve:
```csharp
// Opportunity Managers can trigger (initiate) the close action
new StateMachineStageChangeRole
{
    RoleName = "Opportunity_Manager",
    CanTrigger = true,
    CanApprove = false
}

// Directors can approve the close action
new StateMachineStageChangeRole
{
    RoleName = "Director",
    CanTrigger = false,
    CanApprove = true
}
```

### 3. Leverage Time-Bound Roles
Use `EntityRolePerson.EndDate` for temporary delegation:
- Acting managers while regular manager on leave
- Temporary project approvers
- Fixed-term assignments

### 4. Combine Entity-Specific and Org Hierarchy
Strategy: Try entity-specific approvers first, fall back to org hierarchy:
```csharp
// 1. Check if specific users assigned to THIS opportunity
var specificApprovers = await GetEntityRolePersonApprovers(opportunityId);

// 2. Fall back to organizational hierarchy if none found
if (!specificApprovers.Any())
{
    specificApprovers = await GetOrgHierarchyApprovers(opportunity.OrganizationUnitId);
}
```

This provides flexibility: explicit assignment when needed, automatic hierarchy fallback otherwise.

### 5. Audit Role Assignments Regularly
Monitor which users have approval permissions:
```sql
-- Find all active approvers across all opportunities
SELECT 
    o."Id", 
    o."Title",
    u."Name" as "ApproverName",
    er."Name" as "RoleName",
    erp."EffectiveDate",
    erp."EndDate"
FROM "EntityRolePersons" erp
JOIN "EntityRoles" er ON er."Id" = erp."EntityRoleId"
JOIN "PAOUsers" u ON u."Id" = erp."UserId"
JOIN "Opportunities" o ON o."Id" = erp."EntityId"
WHERE erp."EntityType" = 'Opportunity'
  AND erp."IsDeleted" = false
  AND (erp."EndDate" IS NULL OR erp."EndDate" > CURRENT_TIMESTAMP)
  AND er."Code" IN ('Director', 'Senior_Manager', 'DoA1')
ORDER BY o."Id", er."Name";
```

---

## Why PAO's EntityRole System is Perfect for Workflows

### Comparison: Before vs After

**Before (Generic Approach):**
- Hardcoded role checks in code
- Manual permission management
- No time-bound assignments
- Difficult to audit who can approve what

**After (PAO EntityRole Integration):**
- ✅ Declarative role definitions in database
- ✅ Automatic permission resolution from EntityRolePerson
- ✅ Time-bound assignments with automatic expiry
- ✅ Clear audit trail via EntityRolePersons table
- ✅ Organizational hierarchy fallback
- ✅ Supports internal users AND external contacts
- ✅ Reusable across all entity types (Partner, Interaction, etc.)

**Key Insight:** PAO already solved the hardest part of workflow approvals (role management). The migration just connects the dots!

---

## Conclusion

Migrating to UNOPS.Workflow gives PAO a enterprise-grade workflow system without building it from scratch. The 8-12 hour investment provides:

- Immediate access to production-proven workflow engine
- Zero ongoing maintenance burden
- Full feature parity with GMS workflows
- Foundation for future workflow enhancements
- **Seamless integration with PAO's existing EntityRole system**

The hybrid approach (code-based states + database-driven transitions) provides the best of both worlds: type safety and version control for states, runtime configurability for transitions and permissions.

**Bonus:** PAO's sophisticated EntityRole/EntityRolePerson system provides production-ready approval infrastructure that most systems have to build from scratch!
