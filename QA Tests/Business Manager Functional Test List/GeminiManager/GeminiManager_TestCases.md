# GeminiManager - Comprehensive Test Cases

## Manager Overview
**Manager**: `GeminiManager`  
**Location**: `UNOPS.PAO.Business/Managers/GeminiManager.cs`  
**Purpose**: Manages AI prompts, chat sessions, and Gemini AI integration for content generation.

---

## Functional Test Cases (20+ Cases)

### TC-GM-F001-020: Core Operations
- F001: Get prompt data by type - existing type
- F002: Get prompt data by type - non-existent type returns empty
- F003: Get prompt data by type - null type
- F004: Get prompt data by type - empty string type
- F005: Map model to entity - valid request
- F006: Map model to entity - null request
- F007: Get user sessions - existing user
- F008: Get user sessions - user with no sessions
- F009: Create new session - valid user
- F010: End session - existing session
- F011: End session - non-existent session
- F012: Update session star - set starred
- F013: Update session star - remove starred
- F014: Update session archive - archive session
- F015: Update session archive - unarchive session
- F016: Update session title - valid title
- F017: Update session title - empty title
- F018: Get session data with chats - existing session
- F019: Get session data with chats - non-existent session
- F020: Update current session if inactive - active session

---

## Performance Test Cases (10 Cases)

### TC-GM-P001: Get Prompt Data - Response Time
**Performance Criteria**: < 100ms for prompt retrieval

### TC-GM-P002: Create Session - Response Time
**Performance Criteria**: < 200ms for session creation

### TC-GM-P003: Get User Sessions - Large Session Count
**Performance Criteria**: < 500ms for 100 sessions

### TC-GM-P004: Session Update Operations
**Performance Criteria**: < 150ms per update

### TC-GM-P005: Concurrent Session Access
**Performance Criteria**: 20 concurrent session reads < 300ms each

### TC-GM-P006: Prompt Data Query Performance
**Performance Criteria**: < 50ms with type filter

### TC-GM-P007: Session Data with Chats - Large Chat History
**Performance Criteria**: < 1000ms for 500 messages

### TC-GM-P008: Bulk Session Operations
**Performance Criteria**: < 2000ms for 50 session updates

### TC-GM-P009: AutoMapper Performance
**Performance Criteria**: < 20ms for entity mapping

### TC-GM-P010: Database Round Trip
**Performance Criteria**: < 100ms total round trip

---

## Concurrency Test Cases (10 Cases)

### TC-GM-C001: Concurrent Session Creation
**Scenario**: 10 threads creating sessions simultaneously

### TC-GM-C002: Concurrent Session Updates - Same Session
**Scenario**: 5 threads updating same session

### TC-GM-C003: Concurrent Session Updates - Different Sessions
**Scenario**: 20 threads updating different sessions

### TC-GM-C004: Concurrent Get User Sessions
**Scenario**: Same user sessions queried concurrently

### TC-GM-C005: Concurrent Prompt Data Retrieval
**Scenario**: 15 threads fetching different prompt types

### TC-GM-C006: Session End During Access
**Scenario**: Ending session while chat being processed

### TC-GM-C007: Concurrent Star/Archive Operations
**Scenario**: Multiple threads toggling session flags

### TC-GM-C008: Chat History Access During Update
**Scenario**: Reading chat history while new messages added

### TC-GM-C009: Concurrent User Session Queries
**Scenario**: Multiple users querying sessions simultaneously

### TC-GM-C010: High Load Session Management
**Scenario**: 50 concurrent session operations

---

## Edge Cases (5 Cases)

### TC-GM-E001: Session ID Format Validation
### TC-GM-E002: User ID Zero or Negative
### TC-GM-E003: Prompt Type with Unicode Characters
### TC-GM-E004: Session Title Exceeds Maximum Length
### TC-GM-E005: Archived Session Modification Attempt

