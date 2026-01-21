# WorkflowManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `WorkflowManager`  
**Location**: `UNOPS.PAO.Business/Managers/WorkflowManager.cs`  
**Purpose**: Manages workflow state machines, transitions, and audit logging.

---

## Functional Test Cases (25 Cases)

### TC-WM-F001-025: Core Operations
- F001: Get workflow path - Internal facing
- F002: Get workflow path - External facing
- F003: Get workflow path - TwoFace (both)
- F004: Get workflow path - filtered by sequence >= 0
- F005: Get workflow path - ordered by sequence
- F006: Get workflow path - distinct stages only
- F007: Get workflow state - valid stage
- F008: Get workflow state - invalid stage returns empty
- F009: Get workflow state - Internal facing transformation
- F010: Get workflow state - External facing transformation
- F011: Get workflow state - includes next actions
- F012: Get workflow state - with last log comment
- F013: Get workflow state - no log comment
- F014: Get workflow state - actions filtered by facing
- F015: Add workflow log - complete entry
- F016: Add workflow log - with comment
- F017: Add workflow log - without comment (null)
- F018: Add workflow log - stage transition
- F019: Get workflow path - empty state machine
- F020: Get workflow state - with multiple actions
- F021: Get workflow state - actions ordered by sequence
- F022: Workflow log - retrieve by entity
- F023: Workflow log - ordered by date
- F024: State machine with circular references
- F025: Workflow path - excluding negative sequences

---

## Performance Test Cases (10 Cases)

### TC-WM-P001: Get Workflow Path - Complex State Machine
**Performance Criteria**: < 100ms for 50-state workflow  
**Test Steps**: Get path for large state machine  
**Expected Result**: Path generation < 100ms  

### TC-WM-P002: Get Workflow State - Response Time
**Performance Criteria**: < 50ms per state lookup  
**Test Steps**: Lookup 100 different states  
**Expected Result**: Average < 50ms  

### TC-WM-P003: Add Workflow Log - Bulk Logging
**Performance Criteria**: < 1000ms for 100 log entries  
**Test Steps**: Add 100 workflow logs  
**Expected Result**: Bulk add < 1000ms  

### TC-WM-P004: Workflow Log Query - Large History
**Performance Criteria**: < 500ms for 10K log entries  
**Test Steps**: Query logs for entity with large history  
**Expected Result**: Query < 500ms  

### TC-WM-P005: Get Workflow Path - Multiple Facings
**Performance Criteria**: < 200ms for both facings  
**Test Steps**: Get Internal and External paths  
**Expected Result**: Both paths < 200ms total  

### TC-WM-P006: Workflow State Lookup - With Last Log
**Performance Criteria**: < 150ms including log lookup  
**Test Steps**: Get state with recent log entry  
**Expected Result**: Lookup < 150ms  

### TC-WM-P007: State Machine Traversal - Deep Hierarchy
**Performance Criteria**: < 300ms for 20 levels  
**Test Steps**: Traverse deeply nested state machine  
**Expected Result**: Traversal < 300ms  

### TC-WM-P008: Concurrent Workflow Path Requests
**Performance Criteria**: 50 concurrent requests < 200ms each  
**Test Steps**: 50 threads get workflow path  
**Expected Result**: All < 200ms  

### TC-WM-P009: Workflow Log - Date Range Query
**Performance Criteria**: < 800ms for year range  
**Test Steps**: Query logs across date range  
**Expected Result**: Query < 800ms  

### TC-WM-P010: Get Workflow State - Action Filtering
**Performance Criteria**: < 100ms with facing filter  
**Test Steps**: Get state with filtered actions  
**Expected Result**: Filter < 100ms  

---

## Concurrency Test Cases (10 Cases)

### TC-WM-C001: Concurrent Workflow Path Lookups
**Scenario**: 20 threads get workflow path simultaneously  
**Expected Result**: All return consistent path  

### TC-WM-C002: Concurrent Workflow State Lookups - Same Stage
**Scenario**: 15 threads lookup same workflow stage  
**Expected Result**: All return same state information  

### TC-WM-C003: Concurrent Log Additions - Different Entities
**Scenario**: 30 threads add workflow logs for different entities  
**Expected Result**: All logs created successfully  

### TC-WM-C004: Concurrent Log Additions - Same Entity
**Scenario**: 5 threads add logs for same entity simultaneously  
**Expected Result**: All logs persisted with correct timestamps  

### TC-WM-C005: Get State During Log Addition
**Scenario**: Thread 1 gets state, Thread 2 adds log  
**Expected Result**: State returns correct last log  

### TC-WM-C006: Concurrent Facing Filters
**Scenario**: Multiple threads request different facings  
**Expected Result**: Each returns correct facing-specific data  

### TC-WM-C007: Bulk Log Addition Concurrent
**Scenario**: 10 threads each add 50 logs  
**Expected Result**: All 500 logs created  

### TC-WM-C008: State Machine Modification During Query
**Scenario**: Modify state machine while querying path  
**Expected Result**: Query uses snapshot, no errors  

### TC-WM-C009: Concurrent Action Sequence Lookups
**Scenario**: 25 threads query next actions  
**Expected Result**: All return correct ordered actions  

### TC-WM-C010: Log Query During Log Creation
**Scenario**: Query logs while logs being added  
**Expected Result**: Consistent query results  

---

## Edge Cases (10 Cases)

### TC-WM-E001: State Machine With No States
### TC-WM-E002: Workflow Stage Not Found
### TC-WM-E003: Null Comment in Log Entry
### TC-WM-E004: State With No Next Actions
### TC-WM-E005: Internal/External State Mapping - Missing
### TC-WM-E006: Workflow Log - Empty Entity ID
### TC-WM-E007: State With Duplicate Actions
### TC-WM-E008: Facing Filter - All Actions Filtered Out
### TC-WM-E009: Very Long Comment in Workflow Log
### TC-WM-E010: State Machine With Circular Actions


