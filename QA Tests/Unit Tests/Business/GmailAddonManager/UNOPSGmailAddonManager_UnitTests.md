# UNOPSGmailAddonManager - Unit Test Cases

**Manager**: `UNOPSGmailAddonManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSGmailAddonManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Test Suite Overview
Focus: Gmail integration, Email parsing, Attachment handling, Context detection

**Total Test Cases**: 15+

---

## Test Categories

### 1. Email Parsing (5 tests)
- TC-GA-001: Parse email headers
- TC-GA-002: Extract email body
- TC-GA-003: Parse HTML content
- TC-GA-004: Extract plain text
- TC-GA-005: Parse email attachments

### 2. Context Detection (4 tests)
- TC-GA-006: Detect partner context
- TC-GA-007: Detect opportunity context
- TC-GA-008: Extract entity references
- TC-GA-009: Link email to entity

### 3. Attachment Handling (3 tests)
- TC-GA-010: Download attachment
- TC-GA-011: Save to document manager
- TC-GA-012: Handle large attachments

### 4. Integration Tests (3 tests)
- TC-GA-013: Create interaction from email
- TC-GA-014: Link email to contact
- TC-GA-015: Gmail API error handling

**Coverage**: 70%+

