# WorkflowManager - Unit Test Cases

**Manager**: `WorkflowManager`  
**File**: `UNOPS.PAO.Business/Managers/WorkflowManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions  
**Test Project**: `UNOPS.PAO.Business.Tests`

---

## Test Suite Overview

This test suite covers the `WorkflowManager` with focus on:
- Workflow path generation
- Workflow state retrieval
- Internal vs External facing states
- Workflow logging
- State machine navigation

**Total Test Cases**: 18+

---

## 1. Get Workflow Path Tests

### TC-WM-001: Get Workflow Path for Two-Face
**Test**: `GetWorkflowPath_Should_ReturnAllStages_When_FacingIsTwoFace`

**Arrange**:
```csharp
var stateMachine = new StateMachine
{
    States = new List<State>
    {
        new() { StageCode = "DRAFT", Name = "Draft", Sequence = 0, Facing = Facing.TwoFace },
        new() { StageCode = "REVIEW", Name = "Review", Sequence = 1, Facing = Facing.TwoFace },
        new() { StageCode = "APPROVED", Name = "Approved", Sequence = 2, Facing = Facing.TwoFace }
    }
};
```

**Act**:
```csharp
var result = WorkflowManager.GetWorkflowPath(stateMachine);
```

**Assert**:
```csharp
result.Should().HaveCount(3);
result.Should().BeInAscendingOrder(s => s.Sequence);
result[0].Stage.Should().Be("DRAFT");
result[2].Stage.Should().Be("APPROVED");
```

---

### TC-WM-002: Get Workflow Path for Internal Facing
**Test**: `GetWorkflowPath_Should_ReturnInternalStates_When_FacingIsInternal`

**Arrange**:
```csharp
var stateMachine = new StateMachine
{
    States = new List<State>
    {
        new() { StageCode = "DRAFT", Name = "Draft", Sequence = 0, Facing = Facing.Internal },
        new() { StageCode = "EXTERNAL", Name = "External", Sequence = 1, Facing = Facing.External }, // Should be excluded
        new() { StageCode = "INTERNAL", Name = "Internal", Sequence = 2, Facing = Facing.Internal }
    }
};
```

**Act**:
```csharp
var result = WorkflowManager.GetWorkflowPath(stateMachine, Facing.Internal);
```

**Assert**:
```csharp
result.Should().HaveCount(2);
result.Should().OnlyContain(s => s.Facing == Facing.Internal || s.Facing == Facing.TwoFace);
result.Should().NotContain(s => s.Stage == "EXTERNAL");
```

---

### TC-WM-003: Get Workflow Path for External Facing
**Test**: `GetWorkflowPath_Should_ReturnExternalStates_When_FacingIsExternal`

**Arrange**:
```csharp
var stateMachine = new StateMachine
{
    States = new List<State>
    {
        new() { StageCode = "DRAFT", Name = "Draft", Sequence = 0, Facing = Facing.External },
        new() { StageCode = "INTERNAL", Name = "Internal", Sequence = 1, Facing = Facing.Internal }, // Should be excluded
        new() { StageCode = "SUBMIT", Name = "Submit", Sequence = 2, Facing = Facing.External }
    }
};
```

**Act**:
```csharp
var result = WorkflowManager.GetWorkflowPath(stateMachine, Facing.External);
```

**Assert**:
```csharp
result.Should().HaveCount(2);
result.Should().NotContain(s => s.Stage == "INTERNAL");
```

---

### TC-WM-004: Exclude Negative Sequence
**Test**: `GetWorkflowPath_Should_ExcludeNegativeSequence_When_SequenceLessThanZero`

**Arrange**:
```csharp
var stateMachine = new StateMachine
{
    States = new List<State>
    {
        new() { StageCode = "HIDDEN", Name = "Hidden", Sequence = -1, Facing = Facing.TwoFace }, // Should be excluded
        new() { StageCode = "DRAFT", Name = "Draft", Sequence = 0, Facing = Facing.TwoFace },
        new() { StageCode = "REVIEW", Name = "Review", Sequence = 1, Facing = Facing.TwoFace }
    }
};
```

**Act**:
```csharp
var result = WorkflowManager.GetWorkflowPath(stateMachine);
```

**Assert**:
```csharp
result.Should().HaveCount(2);
result.Should().NotContain(s => s.Stage == "HIDDEN");
result.Should().OnlyContain(s => s.Sequence >= 0);
```

---

### TC-WM-005: Order by Sequence
**Test**: `GetWorkflowPath_Should_OrderBySequence_When_StagesOutOfOrder`

**Arrange**:
```csharp
var stateMachine = new StateMachine
{
    States = new List<State>
    {
        new() { StageCode = "REVIEW", Name = "Review", Sequence = 2, Facing = Facing.TwoFace },
        new() { StageCode = "DRAFT", Name = "Draft", Sequence = 0, Facing = Facing.TwoFace },
        new() { StageCode = "APPROVED", Name = "Approved", Sequence = 1, Facing = Facing.TwoFace }
    }
};
```

**Act**:
```csharp
var result = WorkflowManager.GetWorkflowPath(stateMachine);
```

**Assert**:
```csharp
result.Should().HaveCount(3);
result.Should().BeInAscendingOrder(s => s.Sequence);
result[0].Stage.Should().Be("DRAFT");
result[1].Stage.Should().Be("APPROVED");
result[2].Stage.Should().Be("REVIEW");
```

---

## 2. Get Workflow State Tests

### TC-WM-006: Get Current Workflow State
**Test**: `GetWorkflowState_Should_ReturnState_When_ValidStageProvided`

**Arrange**:
```csharp
var state = new State
{
    StageCode = "DRAFT",
    Name = "Draft",
    Actions = new List<StateAction>
    {
        new() { Name = "Submit", Sequence = 0, Facing = Facing.TwoFace }
    }
};

var stateMachine = new StateMachine
{
    States = new List<State> { state }
};
```

**Act**:
```csharp
var result = manager.GetWorkflowState(stateMachine, "DRAFT", Facing.TwoFace);
```

**Assert**:
```csharp
result.Should().NotBeNull();
result.Stage.Should().Be("DRAFT");
result.DisplayName.Should().Be("Draft");
result.NextActions.Should().HaveCount(1);
```

---

### TC-WM-007: Get Internal State
**Test**: `GetWorkflowState_Should_ReturnInternalState_When_FacingIsInternal`

**Arrange**:
```csharp
var internalState = new State
{
    StageCode = "INTERNAL_DRAFT",
    Name = "Internal Draft"
};

var state = new State
{
    StageCode = "DRAFT",
    Name = "Draft",
    InternalState = internalState
};

var stateMachine = new StateMachine { States = new List<State> { state } };
```

**Act**:
```csharp
var result = manager.GetWorkflowState(stateMachine, "DRAFT", Facing.Internal);
```

**Assert**:
```csharp
result.Stage.Should().Be("INTERNAL_DRAFT");
result.DisplayName.Should().Be("Internal Draft");
```

---

### TC-WM-008: Get External State
**Test**: `GetWorkflowState_Should_ReturnExternalState_When_FacingIsExternal`

**Arrange**:
```csharp
var externalState = new State
{
    StageCode = "EXTERNAL_DRAFT",
    Name = "External Draft"
};

var state = new State
{
    StageCode = "DRAFT",
    Name = "Draft",
    ExternalState = externalState
};

var stateMachine = new StateMachine { States = new List<State> { state } };
```

**Act**:
```csharp
var result = manager.GetWorkflowState(stateMachine, "DRAFT", Facing.External);
```

**Assert**:
```csharp
result.Stage.Should().Be("EXTERNAL_DRAFT");
result.DisplayName.Should().Be("External Draft");
```

---

### TC-WM-009: Invalid Stage Returns Empty State
**Test**: `GetWorkflowState_Should_ReturnEmptyState_When_StageDoesNotExist`

**Arrange**:
```csharp
var stateMachine = new StateMachine
{
    States = new List<State>
    {
        new() { StageCode = "DRAFT", Name = "Draft" }
    }
};
```

**Act**:
```csharp
var result = manager.GetWorkflowState(stateMachine, "INVALID_STAGE", Facing.TwoFace);
```

**Assert**:
```csharp
result.Stage.Should().BeEmpty();
result.DisplayName.Should().BeEmpty();
result.NextActions.Should().BeEmpty();
```

---

### TC-WM-010: Filter Actions by Facing
**Test**: `GetWorkflowState_Should_FilterActions_When_FacingSpecified`

**Arrange**:
```csharp
var state = new State
{
    StageCode = "DRAFT",
    Name = "Draft",
    Actions = new List<StateAction>
    {
        new() { Name = "Internal Action", Sequence = 0, Facing = Facing.Internal },
        new() { Name = "External Action", Sequence = 1, Facing = Facing.External },
        new() { Name = "TwoFace Action", Sequence = 2, Facing = Facing.TwoFace }
    }
};

var stateMachine = new StateMachine { States = new List<State> { state } };
```

**Act**:
```csharp
var resultInternal = manager.GetWorkflowState(stateMachine, "DRAFT", Facing.Internal);
var resultExternal = manager.GetWorkflowState(stateMachine, "DRAFT", Facing.External);
```

**Assert**:
```csharp
resultInternal.NextActions.Should().HaveCount(2); // Internal + TwoFace
resultExternal.NextActions.Should().HaveCount(2); // External + TwoFace
```

---

## 3. Add Workflow Log Tests

### TC-WM-011: Add Workflow Log
**Test**: `AddLog_Should_CreateLogEntry_When_ValidDataProvided`

**Arrange**:
```csharp
// No existing logs
```

**Act**:
```csharp
await manager.AddLog("FundingOpportunity", "123", "DRAFT", "REVIEW", "Submitting for review");
```

**Assert**:
```csharp
var log = await Context.WorkflowLogs.FirstOrDefaultAsync();
log.Should().NotBeNull();
log.EntityName.Should().Be("FundingOpportunity");
log.EntityId.Should().Be("123");
log.Stage.Should().Be("DRAFT");
log.NewStage.Should().Be("REVIEW");
log.Comment.Should().Be("Submitting for review");
```

---

### TC-WM-012: Add Log with Null Stage
**Test**: `AddLog_Should_AllowNullStage_When_InitialCreation`

**Arrange**:
```csharp
// Initial creation scenario
```

**Act**:
```csharp
await manager.AddLog("FundingOpportunity", "123", null, "DRAFT", "Created");
```

**Assert**:
```csharp
var log = await Context.WorkflowLogs.FirstAsync();
log.Stage.Should().BeNull();
log.NewStage.Should().Be("DRAFT");
```

---

### TC-WM-013: Multiple Log Entries
**Test**: `AddLog_Should_CreateMultipleEntries_When_CalledMultipleTimes`

**Arrange**:
```csharp
// Workflow progression
```

**Act**:
```csharp
await manager.AddLog("FO", "1", null, "DRAFT", "Created");
await manager.AddLog("FO", "1", "DRAFT", "REVIEW", "Submitted");
await manager.AddLog("FO", "1", "REVIEW", "APPROVED", "Approved");
```

**Assert**:
```csharp
var logs = await Context.WorkflowLogs.Where(l => l.EntityId == "1").ToListAsync();
logs.Should().HaveCount(3);
```

---

## 4. Workflow State with Comments Tests

### TC-WM-014: Get State with Last Comment
**Test**: `GetWorkflowState_Should_IncludeLastComment_When_LogEntryExists`

**Arrange**:
```csharp
var log = new WorkflowLog
{
    EntityName = "FO",
    EntityId = "1",
    NewStage = "REVIEW",
    Comment = "Submitted for review",
    CreatedDate = DateTime.UtcNow
};
await Context.WorkflowLogs.AddAsync(log);

var state = new State { StageCode = "REVIEW", Name = "Review" };
var stateMachine = new StateMachine { States = new List<State> { state } };
```

**Act**:
```csharp
var result = manager.GetWorkflowState(stateMachine, "REVIEW", Facing.TwoFace);
```

**Assert**:
```csharp
result.Comment.Should().Be("Submitted for review");
```

---

### TC-WM-015: Get Most Recent Comment
**Test**: `GetWorkflowState_Should_ReturnMostRecentComment_When_MultipleLogsExist`

**Arrange**:
```csharp
var baseDate = DateTime.UtcNow;
var logs = new List<WorkflowLog>
{
    new() { NewStage = "REVIEW", Comment = "Old comment", CreatedDate = baseDate.AddDays(-2) },
    new() { NewStage = "REVIEW", Comment = "Recent comment", CreatedDate = baseDate }
};
await Context.WorkflowLogs.AddRangeAsync(logs);

var state = new State { StageCode = "REVIEW", Name = "Review" };
var stateMachine = new StateMachine { States = new List<State> { state } };
```

**Act**:
```csharp
var result = manager.GetWorkflowState(stateMachine, "REVIEW", Facing.TwoFace);
```

**Assert**:
```csharp
result.Comment.Should().Be("Recent comment");
```

---

## 5. Edge Case Tests

### TC-WM-016: Empty State Machine
**Test**: `GetWorkflowPath_Should_ReturnEmpty_When_NoStatesExist`

**Arrange**:
```csharp
var stateMachine = new StateMachine
{
    States = new List<State>() // Empty
};
```

**Act**:
```csharp
var result = WorkflowManager.GetWorkflowPath(stateMachine);
```

**Assert**:
```csharp
result.Should().BeEmpty();
```

---

### TC-WM-017: No Comment in Log
**Test**: `GetWorkflowState_Should_ReturnEmptyComment_When_NoLogExists`

**Arrange**:
```csharp
var state = new State { StageCode = "DRAFT", Name = "Draft" };
var stateMachine = new StateMachine { States = new List<State> { state } };
```

**Act**:
```csharp
var result = manager.GetWorkflowState(stateMachine, "DRAFT", Facing.TwoFace);
```

**Assert**:
```csharp
result.Comment.Should().BeEmpty();
```

---

### TC-WM-018: Duplicate Stage Codes
**Test**: `GetWorkflowPath_Should_Distinct_When_DuplicateStageCodesExist`

**Arrange**:
```csharp
var stateMachine = new StateMachine
{
    States = new List<State>
    {
        new() { StageCode = "DRAFT", Name = "Draft", Sequence = 0, Facing = Facing.TwoFace },
        new() { StageCode = "DRAFT", Name = "Draft Duplicate", Sequence = 0, Facing = Facing.TwoFace }
    }
};
```

**Act**:
```csharp
var result = WorkflowManager.GetWorkflowPath(stateMachine);
```

**Assert**:
```csharp
result.Should().HaveCount(1); // Should deduplicate
```

---

## Test Data Factory

```csharp
public class WorkflowTestDataFactory
{
    public StateMachine CreateStateMachine(params State[] states)
    {
        return new StateMachine
        {
            States = states.ToList()
        };
    }

    public State CreateState(string stageCode, string name, int sequence, Facing facing = Facing.TwoFace)
    {
        return new State
        {
            StageCode = stageCode,
            Name = name,
            Sequence = sequence,
            Facing = facing,
            Actions = new List<StateAction>()
        };
    }

    public WorkflowLog CreateLog(string entityName, string entityId, string? stage, string newStage, string? comment = null)
    {
        return new WorkflowLog
        {
            EntityName = entityName,
            EntityId = entityId,
            Stage = stage,
            NewStage = newStage,
            Comment = comment,
            CreatedDate = DateTime.UtcNow
        };
    }
}
```

---

## Coverage Goals

| Category | Tests | Target Coverage |
|----------|-------|-----------------|
| **Workflow Path** | 5 tests | 90% |
| **Workflow State** | 5 tests | 90% |
| **Workflow Logging** | 3 tests | 85% |
| **State with Comments** | 2 tests | 85% |
| **Edge Cases** | 3 tests | 80% |
| **Overall** | **18+ tests** | **85%+** |

---

**End of WorkflowManager Unit Test Cases**

