# WorkflowManager - Test Execution Report

**Manager**: `WorkflowManager`  
**Location**: `UNOPS.PAO.Business/Managers/WorkflowManager.cs`  
**Test Specification**: `Test Cases/Business/WorkflowManager/WorkflowManager_TestCases.md`  
**Execution Date**: November 11, 2025  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Executive Summary

**Total Test Cases**: 55  
**Test Categories**: Functional (25), Performance (10), Concurrency (10), Edge Cases (10)  
**Implementation Status**: ⚠️ Awaiting Implementation  
**Priority**: 🔴 **HIGH** (Core Workflow Engine)

---

## Test Categories Breakdown

### 1. Functional Tests (25 cases)

#### F001-F006: Workflow Path Generation
- **F001**: Get workflow path - Internal facing ✅ Spec Complete
- **F002**: Get workflow path - External facing ✅ Spec Complete
- **F003**: Get workflow path - TwoFace (both) ✅ Spec Complete
- **F004**: Get workflow path - filtered by sequence >= 0 ✅ Spec Complete
- **F005**: Get workflow path - ordered by sequence ✅ Spec Complete
- **F006**: Get workflow path - distinct stages only ✅ Spec Complete

**Purpose**: Validate workflow path generation for different user-facing contexts (Internal/External/TwoFace). This is critical for showing correct workflow stages to different user types.

**Implementation Notes**:
```csharp
// Mock state machine with various facings
// Test sequence filtering and ordering
// Verify distinct stage selection
// Validate Internal vs External vs TwoFace logic
```

#### F007-F014: Workflow State Retrieval
- **F007**: Get workflow state - valid stage ✅ Spec Complete
- **F008**: Get workflow state - invalid stage returns empty ✅ Spec Complete
- **F009**: Get workflow state - Internal facing transformation ✅ Spec Complete
- **F010**: Get workflow state - External facing transformation ✅ Spec Complete
- **F011**: Get workflow state - includes next actions ✅ Spec Complete
- **F012**: Get workflow state - with last log comment ✅ Spec Complete
- **F013**: Get workflow state - no log comment ✅ Spec Complete
- **F014**: Get workflow state - actions filtered by facing ✅ Spec Complete

**Purpose**: Ensure workflow state retrieval returns correct stage information with appropriate next actions based on user context.

#### F015-F025: Workflow Logging and Edge Cases
- **F015**: Add workflow log - complete entry ✅ Spec Complete
- **F016**: Add workflow log - with comment ✅ Spec Complete
- **F017**: Add workflow log - without comment (null) ✅ Spec Complete
- **F018**: Add workflow log - stage transition ✅ Spec Complete
- **F019**: Get workflow path - empty state machine ✅ Spec Complete
- **F020**: Get workflow state - with multiple actions ✅ Spec Complete
- **F021**: Get workflow state - actions ordered by sequence ✅ Spec Complete
- **F022**: Workflow log - retrieve by entity ✅ Spec Complete
- **F023**: Workflow log - ordered by date ✅ Spec Complete
- **F024**: State machine with circular references ✅ Spec Complete
- **F025**: Workflow path - excluding negative sequences ✅ Spec Complete

---

### 2. Performance Tests (10 cases)

| Test ID | Scenario | Target | Priority | Status |
|---------|----------|--------|----------|--------|
| **P001** | Get Workflow Path - 50-state workflow | < 100ms | 🔴 Critical | ⏳ Pending |
| **P002** | Get Workflow State - 100 lookups | < 50ms avg | 🔴 Critical | ⏳ Pending |
| **P003** | Add Workflow Log - 100 entries | < 1000ms | 🟡 High | ⏳ Pending |
| **P004** | Workflow Log Query - 10K history | < 500ms | 🟡 High | ⏳ Pending |
| **P005** | Get Workflow Path - Both facings | < 200ms total | 🟡 High | ⏳ Pending |
| **P006** | Workflow State + Last Log | < 150ms | 🟡 High | ⏳ Pending |
| **P007** | State Machine Traversal - 20 levels | < 300ms | 🟢 Medium | ⏳ Pending |
| **P008** | Concurrent Path Requests - 50 threads | < 200ms each | 🔴 Critical | ⏳ Pending |
| **P009** | Workflow Log - Year Range Query | < 800ms | 🟢 Medium | ⏳ Pending |
| **P010** | Get State - Action Filtering | < 100ms | 🟡 High | ⏳ Pending |

**Performance Baseline**: WorkflowManager is invoked for every workflow state change, status check, and audit requirement. Must maintain sub-second response times even with complex state machines.

**Critical Performance Paths**:
1. Get workflow state (called on every workflow view)
2. Get workflow path (used for workflow visualization)
3. Add workflow log (executed on state transitions)

---

### 3. Concurrency Tests (10 cases)

#### C001-C002: Path and State Lookup Concurrency
- **C001**: Concurrent Workflow Path Lookups ⏳
  - **Scenario**: 20 threads get workflow path simultaneously
  - **Expected**: All return consistent path
  - **Risk**: Caching issues or inconsistent state machine reads
  
- **C002**: Concurrent Workflow State Lookups - Same Stage ⏳
  - **Scenario**: 15 threads lookup same workflow stage
  - **Expected**: All return same state information
  - **Risk**: Race conditions in state retrieval

#### C003-C005: Workflow Log Concurrency
- **C003**: Concurrent Log Additions - Different Entities ⏳
  - **Scenario**: 30 threads add workflow logs for different entities
  - **Expected**: All logs created successfully
  - **Risk**: Database deadlocks or transaction conflicts
  
- **C004**: Concurrent Log Additions - Same Entity ⏳
  - **Scenario**: 5 threads add logs for same entity simultaneously
  - **Expected**: All logs persisted with correct timestamps
  - **Risk**: Lost updates or timestamp collisions
  
- **C005**: Get State During Log Addition ⏳
  - **Scenario**: Thread 1 gets state, Thread 2 adds log
  - **Expected**: State returns correct last log
  - **Risk**: Stale data or dirty reads

#### C006-C010: Complex Concurrency Scenarios
- **C006**: Concurrent Facing Filters ⏳
  - **Scenario**: Multiple threads request different facings
  - **Expected**: Each returns correct facing-specific data
  
- **C007**: Bulk Log Addition Concurrent ⏳
  - **Scenario**: 10 threads each add 50 logs
  - **Expected**: All 500 logs created
  
- **C008**: State Machine Modification During Query ⏳
  - **Scenario**: Modify state machine while querying path
  - **Expected**: Query uses snapshot, no errors
  
- **C009**: Concurrent Action Sequence Lookups ⏳
  - **Scenario**: 25 threads query next actions
  - **Expected**: All return correct ordered actions
  
- **C010**: Log Query During Log Creation ⏳
  - **Scenario**: Query logs while logs being added
  - **Expected**: Consistent query results

---

### 4. Edge Cases (10 cases)

| Test ID | Scenario | Expected Behavior | Risk Level | Status |
|---------|----------|-------------------|------------|--------|
| **E001** | State Machine With No States | Return empty path/state | 🟡 Medium | ⏳ Pending |
| **E002** | Workflow Stage Not Found | Return empty state gracefully | 🟡 Medium | ⏳ Pending |
| **E003** | Null Comment in Log Entry | Accept null, store as empty | 🟢 Low | ⏳ Pending |
| **E004** | State With No Next Actions | Return state with empty actions list | 🟡 Medium | ⏳ Pending |
| **E005** | Internal/External State Mapping - Missing | Handle missing mapping | 🔴 High | ⏳ Pending |
| **E006** | Workflow Log - Empty Entity ID | Reject or handle gracefully | 🔴 High | ⏳ Pending |
| **E007** | State With Duplicate Actions | Filter to unique actions | 🟡 Medium | ⏳ Pending |
| **E008** | Facing Filter - All Actions Filtered Out | Return empty actions list | 🟡 Medium | ⏳ Pending |
| **E009** | Very Long Comment in Workflow Log | Truncate or accept large text | 🟢 Low | ⏳ Pending |
| **E010** | State Machine With Circular Actions | Detect and handle circular references | 🔴 High | ⏳ Pending |

---

## Risk Assessment

### Critical Risks 🔴

1. **State Machine Corruption**
   - Impact: Incorrect workflow paths shown to users, wrong actions available
   - Test Coverage: F001-F006, F019, F024-F025, E001, E005, E010
   - Mitigation: Comprehensive path generation and circular reference tests

2. **Workflow Log Data Loss**
   - Impact: Audit trail gaps, compliance issues
   - Test Coverage: F015-F018, F022-F023, C003-C005, E003, E006, E009
   - Mitigation: Transaction and concurrency tests

3. **Performance Degradation with Complex Workflows**
   - Impact: Slow workflow visualizations, timeouts
   - Test Coverage: P001, P007, P008
   - Mitigation: Performance benchmarks with large state machines

### High Risks 🟡

1. **Facing Logic Errors**
   - Impact: Wrong stages/actions shown to Internal vs External users
   - Test Coverage: F001-F003, F009-F010, F014, C006
   - Mitigation: Facing-specific transformation tests

2. **Last Log Comment Retrieval Issues**
   - Impact: Missing or incorrect workflow history displayed
   - Test Coverage: F012-F013, P006, C005, C010
   - Mitigation: Log retrieval and timing tests

---

## Implementation Checklist

### Setup (30 minutes)
- [ ] Create `WorkflowManagerTests.cs` in unit test project
- [ ] Set up test fixtures for state machines
- [ ] Mock `AppDbContext`, `WorkflowLog` DbSet
- [ ] Create sample state machine builders (Internal/External/TwoFace)

### Functional Tests (2.5 hours)
- [ ] Implement F001-F006: Workflow path generation
- [ ] Implement F007-F014: Workflow state retrieval
- [ ] Implement F015-F018: Workflow logging
- [ ] Implement F019-F025: Edge cases and advanced scenarios

### Performance Tests (1 hour)
- [ ] Implement P001-P003: Core operation benchmarks
- [ ] Implement P004-P007: Complex scenario benchmarks
- [ ] Implement P008-P010: Concurrent and filtered benchmarks

### Concurrency Tests (1 hour)
- [ ] Implement C001-C005: Path, state, and log concurrency
- [ ] Implement C006-C010: Advanced concurrency scenarios

### Edge Cases (30 minutes)
- [ ] Implement E001-E010: All edge case scenarios

---

## Code Coverage Goals

**Target Coverage**: 85%+

### Critical Paths (Must Cover)
✅ GetWorkflowPath(facing)  
✅ GetWorkflowState(stage, facing)  
✅ AddWorkflowLog(entity, stage, comment)  
✅ GetWorkflowLogsForEntity(entityId)

### Secondary Paths (Should Cover)
✅ Facing transformation logic  
✅ Action filtering by facing  
✅ Sequence ordering  
✅ Circular reference detection  
✅ Last log comment retrieval

---

## Sample Test Implementation

```csharp
using FluentAssertions;
using Moq;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Managers;

public class WorkflowManagerTests
{
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<IStateMachine> _mockStateMachine;
    private readonly WorkflowManager _sut;

    public WorkflowManagerTests()
    {
        _mockDbContext = new Mock<AppDbContext>();
        _mockStateMachine = new Mock<IStateMachine>();
        
        _sut = new WorkflowManager(_mockDbContext.Object, _mockStateMachine.Object);
    }

    [Fact]
    public void GetWorkflowPath_Should_Return_Internal_Stages_Only()
    {
        // Arrange
        var stateMachine = new StateMachine
        {
            Stages = new List<WorkflowStage>
            {
                new() { Id = 1, Facing = Facing.Internal, Sequence = 1 },
                new() { Id = 2, Facing = Facing.External, Sequence = 2 },
                new() { Id = 3, Facing = Facing.TwoFace, Sequence = 3 }
            }
        };

        _mockStateMachine.Setup(x => x.GetStateMachine())
            .Returns(stateMachine);

        // Act
        var result = _sut.GetWorkflowPath(Facing.Internal);

        // Assert
        result.Should().HaveCount(2); // Internal + TwoFace only
        result.Should().Contain(s => s.Id == 1);
        result.Should().Contain(s => s.Id == 3);
        result.Should().NotContain(s => s.Id == 2);
    }

    [Fact]
    public void GetWorkflowState_Should_Include_Next_Actions()
    {
        // Arrange
        var stage = new WorkflowStage
        {
            Id = 1,
            Name = "Draft",
            Actions = new List<WorkflowAction>
            {
                new() { Id = 1, Name = "Submit", Sequence = 1 },
                new() { Id = 2, Name = "Save", Sequence = 2 }
            }
        };

        _mockStateMachine.Setup(x => x.GetStage(1))
            .Returns(stage);

        // Act
        var result = _sut.GetWorkflowState(1, Facing.Internal);

        // Assert
        result.Should().NotBeNull();
        result.Actions.Should().HaveCount(2);
        result.Actions.Should().BeInAscendingOrder(a => a.Sequence);
    }

    [Fact]
    public async Task AddWorkflowLog_Should_Create_Log_Entry()
    {
        // Arrange
        var entityId = 100;
        var stage = "Draft";
        var comment = "Initial submission";

        var mockLogSet = new Mock<DbSet<WorkflowLog>>();
        _mockDbContext.Setup(x => x.WorkflowLogs).Returns(mockLogSet.Object);

        // Act
        await _sut.AddWorkflowLogAsync(entityId, stage, comment);

        // Assert
        mockLogSet.Verify(
            x => x.AddAsync(
                It.Is<WorkflowLog>(log => 
                    log.EntityId == entityId && 
                    log.Stage == stage && 
                    log.Comment == comment),
                default),
            Times.Once
        );

        _mockDbContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public void GetWorkflowPath_Should_Filter_Negative_Sequences()
    {
        // Arrange
        var stateMachine = new StateMachine
        {
            Stages = new List<WorkflowStage>
            {
                new() { Id = 1, Sequence = 1 },
                new() { Id = 2, Sequence = -1 }, // Archived/hidden stage
                new() { Id = 3, Sequence = 2 }
            }
        };

        _mockStateMachine.Setup(x => x.GetStateMachine())
            .Returns(stateMachine);

        // Act
        var result = _sut.GetWorkflowPath(Facing.Internal);

        // Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(s => s.Sequence < 0);
    }

    [Fact]
    public async Task GetWorkflowState_Should_Include_Last_Log_Comment()
    {
        // Arrange
        var entityId = 100;
        var stage = "Review";

        var logs = new List<WorkflowLog>
        {
            new() { EntityId = entityId, Stage = stage, Comment = "First comment", CreatedDate = DateTime.UtcNow.AddHours(-2) },
            new() { EntityId = entityId, Stage = stage, Comment = "Last comment", CreatedDate = DateTime.UtcNow }
        }.AsQueryable();

        var mockLogSet = new Mock<DbSet<WorkflowLog>>();
        mockLogSet.As<IQueryable<WorkflowLog>>()
            .Setup(m => m.Provider).Returns(logs.Provider);
        mockLogSet.As<IQueryable<WorkflowLog>>()
            .Setup(m => m.Expression).Returns(logs.Expression);

        _mockDbContext.Setup(x => x.WorkflowLogs).Returns(mockLogSet.Object);

        // Act
        var result = await _sut.GetWorkflowStateAsync(entityId, stage);

        // Assert
        result.LastComment.Should().Be("Last comment");
    }

    [Theory]
    [InlineData(Facing.Internal, 2)] // Internal + TwoFace
    [InlineData(Facing.External, 2)] // External + TwoFace
    public void GetWorkflowPath_Should_Respect_Facing_Filter(Facing facing, int expectedCount)
    {
        // Arrange
        var stateMachine = new StateMachine
        {
            Stages = new List<WorkflowStage>
            {
                new() { Facing = Facing.Internal, Sequence = 1 },
                new() { Facing = Facing.External, Sequence = 2 },
                new() { Facing = Facing.TwoFace, Sequence = 3 }
            }
        };

        _mockStateMachine.Setup(x => x.GetStateMachine())
            .Returns(stateMachine);

        // Act
        var result = _sut.GetWorkflowPath(facing);

        // Assert
        result.Should().HaveCount(expectedCount);
    }
}
```

---

## Execution Timeline

### Week 1: Implementation
- **Day 1**: Functional tests F001-F010
- **Day 2**: Functional tests F011-F025
- **Day 3**: Performance tests P001-P010
- **Day 4**: Concurrency tests C001-C010
- **Day 5**: Edge cases E001-E010 + Review

### Week 2: Validation
- **Day 1**: Execute all tests, fix failures
- **Day 2**: Performance validation and tuning
- **Day 3**: Code coverage analysis (target: 85%+)
- **Day 4**: Concurrency stress testing
- **Day 5**: Documentation and CI/CD integration

---

## Success Criteria

✅ All 55 test cases implemented  
✅ All tests passing (100% pass rate)  
✅ Code coverage ≥ 85%  
✅ Performance targets met:
  - Path generation < 100ms (50-state workflow)
  - State retrieval < 50ms average
  - Log creation < 1000ms (100 entries)  
✅ No concurrency issues in stress tests  
✅ All edge cases handled gracefully  
✅ CI/CD integration complete  

---

## Dependencies

### Required Packages
- xUnit (test framework)
- Moq (mocking)
- FluentAssertions (readable assertions)
- Microsoft.EntityFrameworkCore.InMemory (EF Core testing)
- BenchmarkDotNet (optional, for detailed performance profiling)

### Test Data Requirements
- Sample state machines (simple, complex, circular)
- Workflow stages with various facings (Internal, External, TwoFace)
- Workflow actions with sequencing
- Historical workflow logs for query testing

---

## Related Components

This test suite validates:
- State machine logic
- Workflow path generation
- Workflow stage transitions
- Audit trail (workflow logs)
- User-facing filtering (Internal vs External)

Impacts:
- Funding Opportunity workflows
- Proposal workflows
- Capacity Assessment workflows
- Any entity with workflow states

---

## Next Steps

1. **Create test project infrastructure** (if not exists)
2. **Implement Priority 1: Functional tests** (F001-F025)
3. **Run initial validation**
4. **Add performance benchmarks**
5. **Stress test concurrency**
6. **Generate coverage reports**
7. **Document findings and optimization opportunities**

---

**Report Status**: Specification Complete ✅ | Implementation Pending ⚠️  
**Estimated Implementation Time**: 3 hours  
**Priority**: 🔴 HIGH (Core workflow engine for all workflow-based entities)


