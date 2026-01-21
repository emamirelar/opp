# UNOPSAiPromptManager - Unit Test Cases

**Manager**: `UNOPSAiPromptManager`  
**File**: `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSAiPromptManager.cs`  
**Test Framework**: xUnit + Moq + FluentAssertions

---

## Test Suite Overview
Focus: AI prompt management, Template management, Variable substitution, Prompt versioning

**Total Test Cases**: 18+

---

## Test Categories

### 1. Prompt CRUD (5 tests)
- TC-AP-001: Create prompt template
- TC-AP-002: Get prompt template
- TC-AP-003: Update prompt template
- TC-AP-004: Delete prompt template
- TC-AP-005: List all prompts

### 2. Template Processing (5 tests)
- TC-AP-006: Replace variables
- TC-AP-007: Validate template syntax
- TC-AP-008: Handle missing variables
- TC-AP-009: Escape special characters
- TC-AP-010: Apply default values

### 3. Versioning (3 tests)
- TC-AP-011: Create new version
- TC-AP-012: Get version history
- TC-AP-013: Revert to previous version

### 4. Prompt Categories (2 tests)
- TC-AP-014: Categorize prompts
- TC-AP-015: Filter by category

### 5. Usage Tracking (3 tests)
- TC-AP-016: Log prompt usage
- TC-AP-017: Get usage statistics
- TC-AP-018: Track popular prompts

**Coverage**: 70%+

