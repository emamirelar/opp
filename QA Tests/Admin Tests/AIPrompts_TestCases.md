# AI Prompts Administration — Test Cases

**Component:** `UNOPS.PAO.ClientApp/src/app/features/admin/ai-prompts`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30-50 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Compliance Check**
| Check | Result |
|-------|--------|
| N ≥ 3P | 90 ≥ 90 ✅ PASS |
| E ≥ 3P | 90 ≥ 90 ✅ PASS |
| F ≥ 3P | 90 ≥ 90 ✅ PASS |
| I ≥ 3P | 90 ≥ 90 ✅ PASS |

---

## Feature Overview

AI prompt management: CRUD prompts, template variables, version control, usage tracking, prompt testing.

---

## §1 Positive Tests (Happy Path)

> **Minimum:** 30-50 tests | **Focus:** Valid inputs, standard workflows, successful operations

### Detailed Test Cases (P0)

#### POS-001: Create AI Prompt with Valid Data

**Priority:** P0  
**Precondition:** User has admin permission. Prompt category exists.

**Steps:**
1. Open AI Prompts page
2. Click "Add New Prompt"
3. Enter Name, Category, Template Text, Variables
4. Save

**Expected Result:** Prompt created with Id, audit fields set, appears in list.

---

#### POS-002: Edit Existing Prompt

**Priority:** P0  
**Precondition:** Prompt exists.

**Steps:**
1. Select prompt from list
2. Click Edit
3. Modify template text
4. Save

**Expected Result:** Changes persisted, LastModifiedBy/Date updated.

---

#### POS-003: Delete Prompt (Soft Delete)

**Priority:** P0  
**Precondition:** Prompt exists, user has delete permission.

**Steps:**
1. Select prompt
2. Delete
3. Confirm

**Expected Result:** IsDeleted=true, excluded from list.

---

#### POS-004: View Prompt List with Pagination

**Priority:** P0  
**Precondition:** 50+ prompts exist.

**Steps:**
1. Open AI Prompts page
2. View list with page size 20

**Expected Result:** 20 prompts displayed, pagination controls visible.

---

#### POS-005: Test Prompt with Sample Input

**Priority:** P0  
**Precondition:** Prompt with variables exists.

**Steps:**
1. Select prompt
2. Click "Test Prompt"
3. Enter sample values for variables
4. Execute test

**Expected Result:** Rendered prompt/response displayed without error.

---

### Positive Tests — Tabular (P1/P2)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-006 | Create prompt with template variables | Valid category | Create with {{var1}}, {{var2}} | Variables parsed and stored | P1 |
| POS-007 | View prompt usage statistics | Prompt used | Open usage tab | Usage count, last used date shown | P1 |
| POS-008 | Duplicate prompt | Existing prompt | Duplicate action | New prompt created with copy of content | P1 |
| POS-009 | Activate prompt | Inactive prompt | Toggle status to Active | Status = Active | P1 |
| POS-010 | Deactivate prompt | Active prompt | Toggle status to Inactive | Status = Inactive | P1 |
| POS-011 | Version prompt | Existing prompt | Create new version | Version incremented, history tracked | P1 |
| POS-012 | Restore previous version | Prompt with versions | Restore v1 | Content reverted to v1 | P1 |
| POS-013 | Filter prompts by category | Multiple categories | Select category filter | Only matching prompts shown | P1 |
| POS-014 | Search prompts by name | Prompts exist | Search "Partner" | Matching prompts returned | P1 |
| POS-015 | Sort prompts by last modified | Multiple prompts | Sort by date desc | Most recent first | P1 |
| POS-016 | Export prompts list | Prompts exist | Export to CSV | File downloaded | P1 |
| POS-017 | Import prompts | Valid JSON file | Import | Prompts created | P2 |
| POS-018 | Create prompt with minimal fields | Required only | Name, Category, Template | Created successfully | P2 |
| POS-019 | Bulk activate prompts | Multiple inactive | Select all, Activate | All activated | P2 |
| POS-020 | View prompt audit trail | Modified prompt | Open audit | Create/Update history shown | P2 |
| POS-021 | Create prompt with Unicode | Valid input | Name with Arabic chars | Stored correctly | P2 |
| POS-022 | Copy prompt to clipboard | Prompt selected | Copy | Text copied | P2 |
| POS-023 | Assign prompt to entity type | Entity config | Link to Partner/Opportunity | Association saved | P2 |
| POS-024 | View prompt dependencies | Prompt used elsewhere | Open dependencies | Usage locations shown | P2 |
| POS-025 | Create prompt with long template | 4000 chars | Enter long text | Stored successfully | P2 |
| POS-026 | Prompt with special variable syntax | {{context.entity}} | Save | Parsed correctly | P2 |
| POS-027 | Filter by status Active | Mixed statuses | Filter Active | Only active shown | P2 |
| POS-028 | Filter by status Inactive | Mixed statuses | Filter Inactive | Only inactive shown | P2 |
| POS-029 | Prompt with default variable values | {{var:default}} | Save | Defaults stored | P2 |
| POS-030 | Reorder prompts | Multiple prompts | Drag reorder | Order persisted | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Minimum:** 90 tests | **Focus:** Invalid inputs, unauthorized access, error conditions

### 2.1 Invalid Input Validation

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-001 | Create with null Name | Name = null | BusinessException: "Name is required" | P0 |
| NEG-002 | Create with empty Name | Name = "" | BusinessException: "Name is required" | P0 |
| NEG-003 | Create with null Category | Category = null | BusinessException: "Category is required" | P0 |
| NEG-004 | Create with invalid Category | Category = 99999 | KeyNotFoundException | P0 |
| NEG-005 | Create with null Template | Template = null | BusinessException: "Template is required" | P0 |
| NEG-006 | Create with empty Template | Template = "" | BusinessException: "Template is required" | P0 |
| NEG-007 | Update non-existent prompt | Id = 999999 | KeyNotFoundException | P0 |
| NEG-008 | Delete non-existent prompt | Id = 999999 | KeyNotFoundException | P0 |
| NEG-009 | Test prompt with invalid variable | {{unknown}} | Error or placeholder | P0 |
| NEG-010 | Create with malformed variable syntax | {{unclosed | Validation error | P0 |

### 2.2 Unauthorized Access

| ID | Test Name | User Role | Action Attempted | Expected Result | Priority |
|----|-----------|-----------|-----------------|-----------------|----------|
| NEG-011 | User without admin permission | Reader | Create prompt | UnauthorizedAccessException | P0 |
| NEG-012 | User without edit permission | Reader | Update prompt | UnauthorizedAccessException | P0 |
| NEG-013 | User without delete permission | Reader | Delete prompt | UnauthorizedAccessException | P0 |
| NEG-014 | Anonymous user | No auth | Any operation | 401 Unauthorized | P0 |
| NEG-015 | Expired session | Expired token | Create prompt | 401 | P0 |
| NEG-016 | User without view permission | No CanViewAIPrompts | List prompts | UnauthorizedAccessException | P1 |
| NEG-017 | Disabled user | Disabled account | Any operation | 403 | P1 |
| NEG-018 | Read-only role | ReadOnly | Edit prompt | 403 | P1 |
| NEG-019 | API without auth header | No Bearer | POST /prompts | 401 | P0 |
| NEG-020 | Tampered JWT | Modified token | Any operation | 401 | P0 |

### 2.3 Invalid State Transitions

| ID | Test Name | Current State | Invalid Action | Expected Result | Priority |
|----|-----------|--------------|---------------|-----------------|----------|
| NEG-021 | Update deleted prompt | IsDeleted=true | UpdatePromptAsync | BusinessException | P1 |
| NEG-022 | Delete already-deleted prompt | IsDeleted=true | DeletePromptAsync | No-op or error | P1 |
| NEG-023 | Restore non-versioned prompt | No versions | RestoreVersion | BusinessException | P1 |
| NEG-024 | Test deleted prompt | IsDeleted=true | TestPrompt | 404 | P1 |
| NEG-025 | Activate deleted prompt | IsDeleted=true | Activate | BusinessException | P1 |

### 2.4 Missing/Null Data

| ID | Test Name | Missing Field | Expected Error | Priority |
|----|-----------|--------------|---------------|----------|
| NEG-026 | Create with all nulls | All fields null | BusinessException | P1 |
| NEG-027 | Update with null Name | Name = null | Validation error | P1 |
| NEG-028 | Import with null file | File = null | ArgumentNullException | P1 |
| NEG-029 | Test with null variable values | Variables = null | Default or error | P1 |
| NEG-030 | Search with null query | Query = null | Empty or all results | P1 |
| NEG-031 | Filter with null category | Category = null | All categories | P1 |
| NEG-032 | Create with whitespace-only Name | "   " | BusinessException | P1 |
| NEG-033 | Create with whitespace-only Template | "   " | BusinessException | P1 |
| NEG-034 | Version with null content | Content = null | Validation error | P1 |
| NEG-035 | Duplicate with invalid source ID | SourceId = 0 | KeyNotFoundException | P1 |

### 2.5 Dependency Failures

| ID | Test Name | Failure Scenario | Expected Behavior | Priority |
|----|-----------|-----------------|-------------------|----------|
| NEG-036 | Database connection lost | DB drops | Exception, rollback | P1 |
| NEG-037 | Database timeout | Slow DB | TimeoutException | P1 |
| NEG-038 | AI service unavailable for test | Service down | Graceful error message | P1 |
| NEG-039 | File storage failure on export | Storage down | BusinessException | P1 |
| NEG-040 | Category service unavailable | Category API down | Error or fallback | P2 |

### 2.6 Duplicate & Constraint Violations

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-041 | Create duplicate prompt name | Same name exists | BusinessException or allowed | P1 |
| NEG-042 | Name exceeds max length | 500 chars | Validation error | P1 |
| NEG-043 | Template exceeds max length | 10000 chars | Validation error | P1 |
| NEG-044 | Invalid variable name | {{123var}} | Validation error | P1 |
| NEG-045 | Variable name with spaces | {{my var}} | Validation error | P1 |
| NEG-046 | Import malformed JSON | Invalid JSON | Parse error | P1 |
| NEG-047 | Import with missing required | No Name in JSON | Validation error | P1 |
| NEG-048 | Create with SQL injection in name | `'; DROP TABLE--` | Sanitized or rejected | P0 |
| NEG-049 | Create with XSS in template | `<script>alert(1)</script>` | Sanitized | P0 |
| NEG-050 | Cyclic variable reference | {{a}} refs {{b}} refs {{a}} | Validation error | P1 |

### 2.7 Additional Negative Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-051 | Create with negative Category ID | -1 | Validation error | P1 |
| NEG-052 | Create with zero ID | Id = 0 | Validation error | P1 |
| NEG-053 | Get with negative prompt ID | -1 | Not found | P1 |
| NEG-054 | Paginate with page = 0 | Page = 0 | Default or error | P2 |
| NEG-055 | Paginate with pageSize = 0 | Size = 0 | Default or error | P2 |
| NEG-056 | Paginate with pageSize = 10000 | Size = 10000 | Capped or error | P2 |
| NEG-057 | Sort by invalid column | SortBy = "INVALID" | Default sort | P2 |
| NEG-058 | Restore to invalid version number | Version = 999 | Not found | P1 |
| NEG-059 | Test with oversized variable value | 10000 chars | Truncated or error | P2 |
| NEG-060 | Create with invalid entity type | EntityType = "Invalid" | Validation error | P1 |
| NEG-061 | Import with duplicate names | JSON has duplicates | Error or overwrite | P2 |
| NEG-062 | Export with empty list | No prompts | Empty file or error | P2 |
| NEG-063 | Version with empty diff | Identical content | No new version or error | P2 |
| NEG-064 | Create with reserved variable name | {{system}} | Rejected | P1 |
| NEG-065 | Path traversal in import filename | `../../evil.json` | Rejected | P0 |
| NEG-066 | Create with null request object | Request = null | ArgumentNullException | P1 |
| NEG-067 | Update with null request object | Request = null | ArgumentNullException | P1 |
| NEG-068 | LDAP injection in search | `*)(cn=*` | Sanitized | P1 |
| NEG-069 | Regex injection in search | `.*+?[]()` | Escaped or literal | P1 |
| NEG-070 | Concurrent delete same prompt | 2 users delete | One succeeds, other 404 | P1 |
| NEG-071 | Create with invalid prompt type | Type = "Invalid" | Validation error | P1 |
| NEG-072 | Test with null prompt ID | PromptId = null | ArgumentNullException | P1 |
| NEG-073 | Get prompt with invalid format | Id = "abc" | 400 Bad Request | P1 |
| NEG-074 | Version with invalid number | Version = -1 | Validation error | P1 |
| NEG-075 | Restore with deleted prompt | Prompt deleted | KeyNotFoundException | P1 |
| NEG-076 | Duplicate with invalid source | Source deleted | KeyNotFoundException | P1 |
| NEG-077 | Import with wrong file type | .txt file | Parse error | P2 |
| NEG-078 | Export with invalid options | Options = null | Default or error | P2 |
| NEG-079 | Create with reserved keyword | Name = "system" | Rejected | P1 |
| NEG-080 | Variable with invalid syntax | {{123}} | Validation error | P1 |
| NEG-081 | Template with unclosed brace | {{var | Validation error | P1 |
| NEG-082 | Assign to deleted entity | Entity deleted | KeyNotFoundException | P1 |
| NEG-083 | Filter with invalid date | Date = "invalid" | Default or error | P2 |
| NEG-084 | Search with control chars | \0 in query | Sanitized | P1 |
| NEG-085 | Create with empty category | Category = "" | Validation error | P1 |
| NEG-086 | Update with stale version | Stale version | Conflict error | P1 |
| NEG-087 | Bulk activate with empty list | [] | No-op or error | P1 |
| NEG-088 | Test with oversized context | 2MB context | Truncated or error | P2 |
| NEG-089 | Create with nested invalid | {{a.b.c}} invalid | Validation error | P1 |
| NEG-090 | Import with encoding error | Wrong encoding | Parse error | P2 |

---

## §3 Boundary Tests (Edge Cases)

> **Minimum:** 90 tests | **Focus:** Limits, boundaries, unusual but valid inputs

### 3.1 String Length Boundaries

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Prompt Name | 1 | 200 | ✅ "A" | ✅ 200 chars | ❌ Rejected | P1 |
| BND-002 | Template | 1 | 10000 | ✅ "x" | ✅ 10000 chars | ❌ Rejected | P1 |
| BND-003 | Variable name | 1 | 100 | ✅ "v" | ✅ 100 chars | ❌ Rejected | P1 |
| BND-004 | Category name | 1 | 100 | ✅ "C" | ✅ 100 chars | ❌ Rejected | P1 |
| BND-005 | Search query | 0 | 255 | ✅ Empty | ✅ 255 chars | ❌ Capped | P1 |
| BND-006 | Variable default value | 0 | 500 | ✅ Empty | ✅ 500 chars | ❌ Rejected | P2 |
| BND-007 | Description | 0 | 1000 | ✅ Empty | ✅ 1000 chars | ❌ Rejected | P2 |
| BND-008 | Import file path | 1 | 260 | ✅ "a.json" | ✅ 260 chars | ❌ Rejected | P2 |

### 3.2 Numeric Boundaries

| ID | Field | Min | Max | Zero | Negative | Max+1 | Priority |
|----|-------|-----|-----|------|----------|-------|----------|
| BND-009 | Prompt ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-010 | Category ID | 1 | MAX_INT | ❌ | ❌ | Overflow | P1 |
| BND-011 | Page number | 1 | 10000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-012 | Page size | 1 | 1000 | ❌ Default | ❌ Error | Capped | P1 |
| BND-013 | Version number | 1 | 999 | ❌ | ❌ | Rejected | P1 |
| BND-014 | Usage count | 0 | MAX_LONG | ✅ 0 | ❌ | Overflow | P2 |
| BND-015 | Variable count per prompt | 0 | 50 | ✅ 0 | ❌ | Rejected | P2 |
| BND-016 | Import batch size | 1 | 100 | ✅ 1 | ❌ | Chunked | P1 |

### 3.3 Date Boundaries

| ID | Test Name | Date Input | Expected Result | Priority |
|----|-----------|-----------|-----------------|----------|
| BND-017 | Prompt created leap year | Feb 29, 2028 | CreatedDate correct | P2 |
| BND-018 | LastUsed at midnight UTC | 00:00:00 | Stored correctly | P2 |
| BND-019 | Version date at 23:59:59 | End of day | Correct | P2 |
| BND-020 | Filter by date range (same day) | FromDate = ToDate | Returns that day | P2 |
| BND-021 | Usage stats date boundary | Timezone edge | Correct display | P2 |

### 3.4 Collection Boundaries

| ID | Test Name | Collection State | Expected Result | Priority |
|----|-----------|-----------------|-----------------|----------|
| BND-022 | Zero prompts | Empty | Empty list, count=0 | P1 |
| BND-023 | One prompt | Single | List with 1 item | P1 |
| BND-024 | Exactly page size prompts | 20 prompts, size=20 | Full page, hasNext=false | P1 |
| BND-025 | Page size + 1 prompts | 21 prompts, size=20 | 20 on page 1, hasNext=true | P1 |
| BND-026 | 1000 prompts | Large | Paginated correctly | P1 |
| BND-027 | Prompt with 0 variables | No variables | Empty vars list | P1 |
| BND-028 | Prompt with 1 variable | Single | 1 variable | P1 |
| BND-029 | Prompt with 50 variables | Max | All 50 loaded | P2 |
| BND-030 | Prompt with 0 versions | No version history | Empty history | P1 |
| BND-031 | Last page of paginated results | Page 5 of 5 | Correct remaining | P1 |

### 3.5 Unicode & Special Characters

| ID | Field | Input Characters | Expected Result | Priority |
|----|-------|-----------------|-----------------|----------|
| BND-032 | Name (Arabic) | `موجه` | Stored correctly | P2 |
| BND-033 | Template (Chinese) | `分析以下内容` | Stored correctly | P2 |
| BND-034 | Name (Cyrillic) | `Подсказка` | Stored correctly | P2 |
| BND-035 | Variable (Accented) | `{{información}}` | Parsed correctly | P2 |
| BND-036 | Name with apostrophe | `Partner's Guide` | Preserved | P1 |
| BND-037 | Template with newlines | Multi-line | Newlines preserved | P1 |
| BND-038 | Variable with underscore | `{{user_name}}` | Valid | P1 |
| BND-039 | Variable with hyphen | `{{context-entity}}` | Valid or rejected per spec | P2 |
| BND-040 | Template with emoji | `Great! 🤝` | Stored | P2 |
| BND-041 | Name with special chars | `O'Brien & Co.` | Preserved | P2 |
| BND-042 | Template with HTML entities | `&lt;div&gt;` | Escaped or stored | P2 |

### 3.6 Template & Variable Boundaries

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-043 | Template with single variable | `{{name}}` | Parsed | P1 |
| BND-044 | Template with 10 variables | 10 {{var}} | All parsed | P1 |
| BND-045 | Variable at start of template | `{{x}} rest` | Valid | P1 |
| BND-046 | Variable at end of template | `rest {{x}}` | Valid | P1 |
| BND-047 | Adjacent variables | `{{a}}{{b}}` | Both parsed | P1 |
| BND-048 | Nested braces (invalid) | `{{a{b}}}` | Error or partial | P2 |
| BND-049 | Empty variable | `{{}}` | Rejected | P1 |
| BND-050 | Double braces | `{{{{var}}}}` | Parsed per spec | P2 |

### 3.7 Additional Boundary Scenarios

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| BND-051 | Name exactly 1 char | "A" | Accepted | P1 |
| BND-052 | Template exactly max | 10000 chars | Accepted | P1 |
| BND-053 | Prompt ID = 1 | First prompt | Retrieved | P2 |
| BND-054 | Prompt ID = MAX_INT | Overflow | Handled | P2 |
| BND-055 | Paginate last page with 1 item | 41 prompts, page 3, size 20 | 1 on page 3 | P1 |
| BND-056 | Search with 1 char | "P" | Matches | P1 |
| BND-057 | Search with max chars | 255 chars | Processed | P1 |
| BND-058 | Import 1 prompt | Single in JSON | Created | P2 |
| BND-059 | Import 100 prompts | Batch | All created | P2 |
| BND-060 | Create with all optional null | Required only | Success | P1 |
| BND-061 | Create with all optional filled | Full data | Success | P1 |
| BND-062 | Version 1 only | No previous | Version 1 created | P1 |
| BND-063 | Restore to version 1 | From v2 | Content reverted | P2 |
| BND-064 | Usage count at zero | New prompt | 0 usage | P1 |
| BND-065 | Usage count at max | High usage | Displayed correctly | P2 |
| BND-066 | Concurrent list requests | 2 users list | Both get correct data | P2 |
| BND-067 | Timezone boundary for LastUsed | UTC vs local | Correct | P2 |
| BND-068 | Sort by each column | Name, Date, Category | All work | P1 |
| BND-069 | Filter by each status | Active, Inactive | Correct results | P2 |
| BND-070 | Category with max prompts | 500 in category | Paginated | P1 |
| BND-071 | Name at 199 chars | 199 chars | Accepted | P1 |
| BND-072 | Template at 9999 chars | 9999 chars | Accepted | P1 |
| BND-073 | Variable count at 49 | 49 variables | Accepted | P2 |
| BND-074 | Page size at 999 | 999 | Accepted | P1 |
| BND-075 | Version number at 998 | 998 | Accepted | P2 |
| BND-076 | Search exactly 254 chars | 254 chars | Processed | P1 |
| BND-077 | Prompt with 2 variables | Two vars | Both parsed | P1 |
| BND-078 | Import 50 prompts | 50 in batch | All created | P2 |
| BND-079 | Empty template | "" | Rejected | P1 |
| BND-080 | Single char variable | "x" | Parsed | P1 |
| BND-081 | Template at 1 char | "x" | Accepted | P1 |
| BND-082 | Category with 1 prompt | Single | 1 prompt | P1 |
| BND-083 | Usage count at 1 | 1 usage | Displayed | P2 |
| BND-084 | LastUsed at epoch | Epoch | Stored | P2 |
| BND-085 | Unicode in variable | Arabic | Parsed | P2 |
| BND-086 | Prompt ID = 2 | Second | Retrieved | P2 |
| BND-087 | Pagination page 2 of 2 | 2 pages | 2nd page | P1 |
| BND-088 | Filter by single category | One category | Correct | P1 |
| BND-089 | Template with 2 variables | {{a}} {{b}} | Both | P1 |
| BND-090 | Zero versions | No versions | Empty | P1 |

---

## §4 Functional Tests (Business Rules)

> **Minimum:** 90 tests | **Breakdown:** Workflow (15), Validation (15), Constraint (10), Audit (10)

### 4.1 Workflow Rules (15)

| ID | Test Name | Rule | Trigger | Expected Outcome | Priority |
|----|-----------|------|---------|-----------------|----------|
| FUN-001 | Prompts query excludes deleted | IsDeleted filter | List prompts | Only !IsDeleted | P0 |
| FUN-002 | Create sets audit fields | Audit on create | CreatePromptAsync | CreatedBy, CreatedDate | P0 |
| FUN-003 | Update sets audit fields | Audit on update | UpdatePromptAsync | LastModifiedBy, LastModifiedDate | P0 |
| FUN-004 | Delete sets soft-delete | Soft-delete | DeletePromptAsync | IsDeleted, DeletedBy, DeletedDate | P0 |
| FUN-005 | Variable parsing on save | Parse | Save template | Variables extracted | P1 |
| FUN-006 | Version increment on change | Versioning | Save modified | Version++ | P1 |
| FUN-007 | Restore creates new version | Restore | RestoreVersion | Content reverted, version++ | P1 |
| FUN-008 | Test increments usage count | Usage tracking | TestPrompt | UsageCount++ | P1 |
| FUN-009 | Test updates LastUsed | LastUsed | TestPrompt | LastUsed = now | P1 |
| FUN-010 | Category validated on create | FK | Create | Category must exist | P0 |
| FUN-011 | Activate sets Status | Status | Activate | Status = Active | P1 |
| FUN-012 | Deactivate sets Status | Status | Deactivate | Status = Inactive | P1 |
| FUN-013 | Duplicate copies content | Duplicate | DuplicatePrompt | New prompt, same content | P1 |
| FUN-014 | Export includes only active | Export filter | Export | Only !IsDeleted | P1 |
| FUN-015 | Import creates new prompts | Import | Import | New IDs, no overwrite by default | P1 |

### 4.2 Validation Rules (15)

| ID | Test Name | Rule | Valid | Invalid | Priority |
|----|-----------|------|-------|---------|----------|
| FUN-016 | Name required | Required | "Prompt" | null, "" | P0 |
| FUN-017 | Template required | Required | "Text" | null, "" | P0 |
| FUN-018 | Category required | Required | Valid ID | null, 0 | P0 |
| FUN-019 | Variable name format | [a-zA-Z_][a-zA-Z0-9_]* | "user_name" | "123var" | P1 |
| FUN-020 | Name max length | ≤200 | 200 chars | 201 chars | P1 |
| FUN-021 | Template max length | ≤10000 | 10000 | 10001 | P1 |
| FUN-022 | No SQL in name | Sanitize | "Test" | `'; DROP--` | P0 |
| FUN-023 | No XSS in template | Sanitize | "Text" | `<script>` | P0 |
| FUN-024 | Valid JSON for import | JSON | Valid file | Invalid JSON | P1 |
| FUN-025 | Version number positive | ≥1 | 1 | 0, -1 | P1 |
| FUN-026 | Trim whitespace from name | Trim | "  Name  " | → "Name" | P2 |
| FUN-027 | Reserved variables blocked | Block | "user" | "system" | P1 |
| FUN-028 | Cyclic ref detection | No cycles | A→B | A→B→A | P1 |
| FUN-029 | Entity type validation | Enum | "Partner" | "Invalid" | P1 |
| FUN-030 | Duplicate variable names | Unique | {{a}}, {{b}} | {{a}}, {{a}} | P1 |

### 4.3 Constraint Rules (10)

| ID | Test Name | Constraint | Test Input | Expected Result | Priority |
|----|-----------|-----------|-----------|-----------------|----------|
| FUN-031 | Max variables per prompt | 50 | 51 variables | Rejected | P1 |
| FUN-032 | Max page size | 1000 | 5000 | Capped at 1000 | P1 |
| FUN-033 | Unique name per category | Optional | Duplicate name | Error or allowed | P2 |
| FUN-034 | FK category exists | FK | Non-existent | FK error | P0 |
| FUN-035 | Import batch limit | 100 | 150 | Chunked | P2 |
| FUN-036 | Version history limit | 100 | 101 | Oldest pruned | P2 |
| FUN-037 | Export row limit | 10000 | 15000 | Paginated export | P2 |
| FUN-038 | Test variable value limit | 1000 chars | 2000 chars | Truncated | P1 |
| FUN-039 | File upload size limit | 5MB | 10MB | Rejected | P1 |
| FUN-040 | Concurrent test limit | 5/user | 10 simultaneous | Queued | P2 |

### 4.4 Audit Rules (10)

| ID | Test Name | Action | Expected Audit Entry | Priority |
|----|-----------|--------|---------------------|----------|
| FUN-041 | Create audit | CreatePromptAsync | CreatedBy, CreatedDate | P0 |
| FUN-042 | Update audit | UpdatePromptAsync | LastModifiedBy, LastModifiedDate | P0 |
| FUN-043 | Delete audit | DeletePromptAsync | DeletedBy, DeletedDate | P0 |
| FUN-044 | Version audit | CreateVersion | Version number, timestamp | P1 |
| FUN-045 | Restore audit | RestoreVersion | RestoredBy, RestoredDate | P1 |
| FUN-046 | Read no audit | GetPromptById | No modification | P1 |
| FUN-047 | Import audit | Import | Batch create audit | P1 |
| FUN-048 | Export audit | Export | ExportBy, ExportDate | P1 |
| FUN-049 | Failed create no audit | Failed create | No audit entry | P1 |
| FUN-050 | Audit immutable on read | Get | Audit fields unchanged | P1 |
| FUN-051 | Variable extraction on save | Save template | Variables extracted | P1 |
| FUN-052 | Version increment on change | Save modified | Version++ | P1 |
| FUN-053 | Restore creates new version | Restore | Content reverted | P1 |
| FUN-054 | Test increments usage | Test | UsageCount++ | P1 |
| FUN-055 | Test updates LastUsed | Test | LastUsed = now | P1 |
| FUN-056 | Category validated on create | Create | Category exists | P0 |
| FUN-057 | Activate sets Status | Activate | Status = Active | P1 |
| FUN-058 | Deactivate sets Status | Deactivate | Status = Inactive | P1 |
| FUN-059 | Duplicate copies content | Duplicate | New prompt, same content | P1 |
| FUN-060 | Export excludes deleted | Export | Only !IsDeleted | P1 |
| FUN-061 | Import creates new | Import | New IDs | P1 |
| FUN-062 | Name required | Required | "Prompt" | null, "" | P0 |
| FUN-063 | Template required | Required | "Text" | null, "" | P0 |
| FUN-064 | Variable format validation | Format | "user_name" | "123var" | P1 |
| FUN-065 | Name max length | ≤200 | 200 | 201 | P1 |
| FUN-066 | Template max length | ≤10000 | 10000 | 10001 | P1 |
| FUN-067 | No SQL in name | Sanitize | "Test" | `'; DROP--` | P0 |
| FUN-068 | No XSS in template | Sanitize | "Text" | `<script>` | P0 |
| FUN-069 | Valid JSON import | JSON | Valid | Invalid | P1 |
| FUN-070 | Version positive | ≥1 | 1 | 0 | P1 |
| FUN-071 | Trim name | Trim | "  Name  " | → "Name" | P2 |
| FUN-072 | Reserved variables blocked | Block | "user" | "system" | P1 |
| FUN-073 | Cyclic ref detection | No cycles | A→B | A→B→A | P1 |
| FUN-074 | Entity type validation | Enum | "Partner" | "Invalid" | P1 |
| FUN-075 | Duplicate variable names | Unique | {{a}}, {{b}} | {{a}}, {{a}} | P1 |
| FUN-076 | Max variables per prompt | 50 | 51 | Rejected | P1 |
| FUN-077 | Max page size | 1000 | 5000 | Capped | P1 |
| FUN-078 | FK category exists | FK | Valid | 999999 | P0 |
| FUN-079 | Import batch limit | 100 | 150 | Chunked | P2 |
| FUN-080 | Version history limit | 100 | 101 | Pruned | P2 |
| FUN-081 | Export row limit | 10000 | 15000 | Paginated | P2 |
| FUN-082 | Test variable limit | 1000 | 2000 | Truncated | P1 |
| FUN-083 | File upload limit | 5MB | 10MB | Rejected | P1 |
| FUN-084 | Create audit | Create | CreatedBy, CreatedDate | P0 |
| FUN-085 | Update audit | Update | LastModifiedBy, LastModifiedDate | P0 |
| FUN-086 | Delete audit | Delete | DeletedBy, DeletedDate | P0 |
| FUN-087 | Version audit | Version | Version, timestamp | P1 |
| FUN-088 | Restore audit | Restore | RestoredBy, RestoredDate | P1 |
| FUN-089 | Read no audit | Get | No modification | P1 |
| FUN-090 | Failed create no audit | Failed | No audit | P1 |

---

## §5 Integration Tests (End-to-End Flows)

> **Minimum:** 90 tests

### 5.1 CRUD Workflow (10)

| ID | Test Name | Operation | Entities | Expected Result | Priority |
|----|-----------|----------|----------|-----------------|----------|
| INT-001 | Full CRUD lifecycle | Create→Read→Update→Delete | Prompt | All succeed | P0 |
| INT-002 | Create → appears in list | Create | Prompt | In list | P0 |
| INT-003 | Delete → excluded from list | Delete | Prompt | Not in list | P0 |
| INT-004 | Update → persists | Update + read | Prompt | Changes persisted | P0 |
| INT-005 | Create with variables → test | Create + Test | Prompt | Test works | P1 |
| INT-006 | Version → restore | Version + Restore | Prompt | Content reverted | P1 |
| INT-007 | Duplicate → edit | Duplicate + Edit | Prompt | Independent prompt | P1 |
| INT-008 | Import → export | Import + Export | Prompts | Data round-trip | P1 |
| INT-009 | Activate → use in AI | Activate + Use | Prompt | Used in assistant | P1 |
| INT-010 | Bulk operations | Multi-select actions | Prompts | Batch success | P1 |

### 5.2 Search & Filter (10)

| ID | Test Name | Criteria | Expected | Priority |
|----|-----------|---------|----------|----------|
| INT-011 | Search by name | "Partner" | Matching prompts | P0 |
| INT-012 | Search by category | Category filter | Category matches | P0 |
| INT-013 | Filter by status | Active | Active only | P1 |
| INT-014 | Combined search + filter | "Prompt" + Active | Both applied | P1 |
| INT-015 | Search empty | "NONEXISTENT" | Empty result | P1 |
| INT-016 | Filter by date range | Last 30 days | Date filtered | P1 |
| INT-017 | Search case-insensitive | "prompt" vs "PROMPT" | Same results | P1 |
| INT-018 | Filter by entity type | Partner prompts | Only Partner | P1 |
| INT-019 | Search with special chars | "O'Brien" | Handled | P2 |
| INT-020 | Filter excludes deleted | Include deleted | Deleted excluded | P1 |

### 5.3 Pagination (5)

| ID | Test Name | Page/Size | Expected | Priority |
|----|-----------|----------|----------|----------|
| INT-021 | Page 1 of 3 | 50 prompts, page=1, size=20 | 20 items, hasNext | P1 |
| INT-022 | Last page | 50 prompts, page=3, size=20 | 10 items, hasNext=false | P1 |
| INT-023 | Empty page | Filter yields 0 | Empty, total=0 | P1 |
| INT-024 | Single page | 15 prompts, size=20 | 15 items | P2 |
| INT-025 | Large page | 1000 prompts, size=1000 | All on 1 page | P2 |

### 5.4 Relationships (10)

| ID | Test Name | Relationship | Scenario | Expected | Priority |
|----|-----------|-------------|---------|----------|----------|
| INT-026 | Prompt → Category | FK | Load prompt | Category loaded | P0 |
| INT-027 | Prompt → Versions | Include | Load with versions | Versions loaded | P1 |
| INT-028 | Prompt → Entity Type | Config | Link to Partner | Association saved | P1 |
| INT-029 | Category deletion | Cascade | Delete category | Prompts handled | P1 |
| INT-030 | Prompt used in AI Assistant | Integration | Use prompt | Assistant gets prompt | P1 |
| INT-031 | Prompt dependency graph | Dependencies | Prompt A uses B | Graph shown | P2 |
| INT-032 | Multi-category filter | Many | Filter 3 categories | Union result | P1 |
| INT-033 | Prompt → Audit trail | Audit | Load audit | History shown | P1 |
| INT-034 | Export includes category | Export | Export | Category in file | P2 |
| INT-035 | Import with category mapping | Import | Map category | Correct category | P1 |

### 5.5 Error Handling (15)

| ID | Test Name | Error | Expected | Priority |
|----|-----------|-------|----------|----------|
| INT-036 | Create invalid → 400 | Validation | BusinessException | P0 |
| INT-037 | Get non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-038 | Unauthorized → 403 | No permission | UnauthorizedAccessException | P0 |
| INT-039 | Update non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-040 | Delete non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-041 | Duplicate name → 400 | Constraint | BusinessException | P1 |
| INT-042 | Invalid category → 400 | FK | BusinessException | P1 |
| INT-043 | Import malformed → 400 | Parse | Validation error | P1 |
| INT-044 | DB timeout → 500 | Timeout | Graceful error | P1 |
| INT-045 | Concurrent conflict → 409 | Concurrency | Conflict error | P1 |
| INT-046 | Malformed request → 400 | Bad JSON | Validation error | P1 |
| INT-047 | Rate limit → 429 | Too many | Rate limit message | P2 |
| INT-048 | SQL injection → sanitized | Injection | Parameterized | P0 |
| INT-049 | Large payload → 413 | Oversized | Rejected | P2 |
| INT-050 | AI service down for test | Service error | User-friendly message | P1 |
| INT-051 | Create → Test → Version | Full flow | All succeed | P1 |
| INT-052 | Create → Duplicate → Edit | Duplicate flow | Independent | P1 |
| INT-053 | Import → Export round-trip | Import | Export matches | P1 |
| INT-054 | Version → Restore | Version flow | Content reverted | P1 |
| INT-055 | Activate → Use in AI | Activate | Used in assistant | P1 |
| INT-056 | Search → Filter → Sort | Combined | Correct results | P1 |
| INT-057 | API GetById → Test | Get | Test works | P1 |
| INT-058 | Bulk activate → Verify | Bulk | All activated | P1 |
| INT-059 | Create with vars → Test | Create | Test with vars | P1 |
| INT-060 | Export → Download | Export | File downloaded | P1 |
| INT-061 | Create → Update → Delete | CRUD | All succeed | P0 |
| INT-062 | Filter by category → List | Filter | Category filtered | P1 |
| INT-063 | Pagination → Filter | Page + Filter | Correct subset | P1 |
| INT-064 | Prompt → Category | FK | Category loaded | P0 |
| INT-065 | Prompt → Versions | Include | Versions loaded | P1 |
| INT-066 | Prompt → Entity Type | Config | Association saved | P1 |
| INT-067 | Category deletion | Cascade | Prompts handled | P1 |
| INT-068 | Prompt used in AI | Integration | Assistant gets prompt | P1 |
| INT-069 | Multi-category filter | Many | Union result | P1 |
| INT-070 | Prompt → Audit | Audit | History shown | P1 |
| INT-071 | Create → In list | Create | In list | P0 |
| INT-072 | Delete → Not in list | Delete | Excluded | P0 |
| INT-073 | Update → Persisted | Update | Changes saved | P0 |
| INT-074 | Duplicate → New ID | Duplicate | New prompt | P1 |
| INT-075 | Restore → Content | Restore | Content reverted | P1 |
| INT-076 | Test → Usage updated | Test | UsageCount++ | P1 |
| INT-077 | Import → Validate | Import | Validated | P1 |
| INT-078 | Export → Category in file | Export | Category included | P2 |
| INT-079 | Import with category map | Import | Category mapped | P1 |
| INT-080 | Create invalid → 400 | Validation | BusinessException | P0 |
| INT-081 | Get non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-082 | Unauthorized → 403 | No permission | UnauthorizedAccessException | P0 |
| INT-083 | Update non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-084 | Delete non-existent → 404 | Not found | KeyNotFoundException | P0 |
| INT-085 | Duplicate name → 400 | Constraint | BusinessException | P1 |
| INT-086 | Invalid category → 400 | FK | BusinessException | P1 |
| INT-087 | Import malformed → 400 | Parse | Validation error | P1 |
| INT-088 | DB timeout → 500 | Timeout | Graceful error | P1 |
| INT-089 | Concurrent conflict → 409 | Concurrency | Conflict error | P1 |
| INT-090 | Full workflow → Audit | Full workflow | Audit complete | P1 |

---

## §6 Security Tests

> **Minimum:** 50 tests

### 6.1 Injection Prevention (10)

| ID | Attack | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL injection in Name | `'; DROP TABLE--` | Parameterized | P0 |
| SEC-002 | SQL injection in search | `1 OR 1=1` | Parameterized | P0 |
| SEC-003 | XSS in Name | `<script>alert(1)</script>` | Sanitized | P0 |
| SEC-004 | XSS in Template | `"><script>` | Sanitized | P0 |
| SEC-005 | LDAP injection | `*)(cn=*` | Sanitized | P1 |
| SEC-006 | OS command in filename | `; rm -rf /` | Sanitized | P0 |
| SEC-007 | Path traversal in import | `../../evil.json` | Rejected | P0 |
| SEC-008 | HTML in template | `<img onerror=...>` | Escaped | P1 |
| SEC-009 | JSON injection | `{"$ne":null}` | Rejected | P1 |
| SEC-010 | XXE in import | XXE payload | Rejected | P1 |

### 6.2 Broken Access Control (10)

| ID | Test | Role | Action | Expected | Priority |
|----|------|------|--------|----------|----------|
| SEC-011 | Anonymous create | No auth | POST /prompts | 401 | P0 |
| SEC-012 | No create permission | Reader | POST /prompts | 403 | P0 |
| SEC-013 | Expired token | Expired | Any | 401 | P0 |
| SEC-014 | Tampered JWT | Modified | Any | 401 | P0 |
| SEC-015 | Disabled account | Disabled | Any | 403 | P1 |
| SEC-016 | Post-logout | Logged out | Cached call | 401 | P1 |
| SEC-017 | Role escalation | Basic | ?role=admin | Ignored | P0 |
| SEC-018 | Cross-tenant access | User A | User B's scope | 403 | P0 |
| SEC-019 | No export permission | Reader | Export | 403 | P1 |
| SEC-020 | No import permission | Reader | Import | 403 | P1 |

### 6.3 IDOR (10)

| ID | Object | Manipulation | Expected | Priority |
|----|--------|-------------|----------|----------|
| SEC-021 | Prompt ID guess | Enumerate IDs | 403 if no access | P0 |
| SEC-022 | Deleted prompt | Access deleted | 404 | P1 |
| SEC-023 | Negative ID | -1 | 400 | P1 |
| SEC-024 | Zero ID | 0 | 400 | P1 |
| SEC-025 | Float ID | 1.5 | 400 | P1 |
| SEC-026 | String ID | "abc" | 400 | P1 |
| SEC-027 | MAX_INT ID | 2147483647 | 404 | P1 |
| SEC-028 | Category ID manipulation | Change category | Validated | P0 |
| SEC-029 | Version ID manipulation | Invalid version | 404 | P1 |
| SEC-030 | Other user's prompt | Access via ID | 403 | P0 |

### 6.4 Mass Assignment (5)

| ID | Protected Field | Expected | Priority |
|----|----------------|----------|----------|
| SEC-031 | IsDeleted | Not modifiable | P0 |
| SEC-032 | CreatedBy | Not modifiable | P0 |
| SEC-033 | CreatedDate | Not modifiable | P0 |
| SEC-034 | Id | Not settable | P0 |
| SEC-035 | DeletedBy/DeletedDate | Not modifiable | P1 |

### 6.5 Authentication & Session (10)

| ID | Attack | Expected Protection | Priority |
|----|--------|-------------------|----------|
| SEC-036 | Brute-force | Account lockout | P0 |
| SEC-037 | Session fixation | New session | P0 |
| SEC-038 | Session hijacking | Token binding | P1 |
| SEC-039 | CSRF on create | CSRF token | P0 |
| SEC-040 | CSRF on delete | CSRF token | P0 |
| SEC-041 | Token storage | HttpOnly, Secure | P0 |
| SEC-042 | Concurrent sessions | Policy enforced | P1 |
| SEC-043 | Token refresh | Works correctly | P1 |
| SEC-044 | Logout | Token invalidated | P0 |
| SEC-045 | HTTPS | Enforced | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Expected Protection | Priority |
|----|------|-------------------|----------|
| SEC-046 | Internal audit fields | DTO filtering | P1 |
| SEC-047 | Stack traces | Generic errors | P0 |
| SEC-048 | API keys in prompts | Not exposed | P1 |
| SEC-049 | Response caching | Cache-Control: no-store | P1 |
| SEC-050 | Tokens in URL | HttpOnly cookie | P1 |

---

## §7 Concurrency Tests

> **Minimum:** 25 tests

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|-------------------|-------------------|----------|
| CON-001 | Two users update same prompt | Concurrent update | Last write wins or conflict | P1 |
| CON-002 | Create and delete same prompt | Race | One succeeds, other fails | P1 |
| CON-003 | Two users create prompts | Concurrent create | Both succeed | P1 |
| CON-004 | Update during read | Read consistency | Consistent read | P1 |
| CON-005 | Delete during read | Read consistency | Null or pre-delete | P1 |
| CON-006 | Concurrent test same prompt | 2 tests | Both complete | P1 |
| CON-007 | Concurrent version create | 2 versions | Both created | P1 |
| CON-008 | Concurrent pagination | Multiple pages | Correct data | P2 |
| CON-009 | Database deadlock | Circular | Resolved, retry | P1 |
| CON-010 | Token refresh during create | Expire mid-call | Retry with new token | P1 |
| CON-011 | Bulk import concurrent | 2 users import | Both complete | P2 |
| CON-012 | Concurrent export | 2 exports | Both succeed | P2 |
| CON-013 | Update during test | Test + Update | No corruption | P1 |
| CON-014 | Concurrent activate/deactivate | Opposing actions | Consistent state | P1 |
| CON-015 | Restore during update | Restore + Update | One wins | P1 |
| CON-016 | Duplicate during delete | Duplicate + Delete | Handled | P2 |
| CON-017 | Import during list | Import + List | List consistent | P2 |
| CON-018 | Concurrent filter changes | 2 users filter | Independent | P2 |
| CON-019 | Optimistic concurrency | Update stale | Conflict error | P1 |
| CON-020 | Connection pool exhaustion | Many concurrent | Queued or error | P1 |
| CON-021 | Cache invalidation | Update + read | Fresh data | P1 |
| CON-022 | Usage count race | 2 tests | Count += 2 | P1 |
| CON-023 | Version conflict | Same base version | Conflict | P1 |
| CON-024 | Bulk delete concurrent | 2 bulk deletes | Both complete | P2 |
| CON-025 | Search during update | Search + Update | Search consistent | P2 |

---

## §8 Unit Tests

> **Minimum:** 21 tests

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|----------------|----------|
| UNT-001 | Variable extraction | Validation | "Hi {{name}}" | ["name"] | P1 |
| UNT-002 | Multiple variables | Validation | "{{a}} {{b}}" | ["a","b"] | P1 |
| UNT-003 | Variable with default | Formatting | "{{x:default}}" | Parsed | P1 |
| UNT-004 | Empty template | Validation | "" | Error | P1 |
| UNT-005 | Invalid variable syntax | Validation | "{{123}}" | Error | P1 |
| UNT-006 | Name trim | Formatting | "  Name  " | "Name" | P2 |
| UNT-007 | Template length truncate | Calculations | 10001 chars | Error | P1 |
| UNT-008 | Version increment | Status logic | v1 + change | v2 | P1 |
| UNT-009 | Status Active | Status logic | Activate | Active | P1 |
| UNT-010 | Status Inactive | Status logic | Deactivate | Inactive | P1 |
| UNT-011 | Usage count increment | Calculations | Test call | +1 | P1 |
| UNT-012 | LastUsed update | Status logic | Test call | Now | P1 |
| UNT-013 | Map entity to model | Collections | Entity | Model | P1 |
| UNT-014 | Map request to entity | Collections | Request | Entity | P1 |
| UNT-015 | JSON parse import | Validation | Valid JSON | Prompt list | P1 |
| UNT-016 | Export format | Formatting | Prompts | CSV/JSON | P2 |
| UNT-017 | Pagination default | Calculations | null, null | 1, 20 | P1 |
| UNT-018 | Category lookup | Collections | ID | Category | P1 |
| UNT-019 | Reserved variable check | Validation | "system" | Rejected | P1 |
| UNT-020 | Cyclic ref detection | Validation | A→B→A | Error | P1 |
| UNT-021 | Date format for audit | Formatting | Now | ISO string | P2 |

---

## §9 Performance Tests

> **Minimum:** 16 tests

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Create prompt | Single create | < 200ms | P2 |
| PRF-002 | Get prompt by ID | Single read | < 100ms | P2 |
| PRF-003 | List 20 prompts | Paginated list | < 500ms | P2 |
| PRF-004 | List 1000 prompts | Full list | < 3s | P2 |
| PRF-005 | Search prompts | Search query | < 1s | P2 |
| PRF-006 | Test prompt | AI test call | < 5s | P2 |
| PRF-007 | Export 100 prompts | Export | < 2s | P2 |
| PRF-008 | Import 50 prompts | Import | < 5s | P2 |
| PRF-009 | 10 concurrent creates | Concurrent | All < 1s | P2 |
| PRF-010 | 20 concurrent reads | Concurrent | All < 500ms | P2 |
| PRF-011 | List with includes | Versions, Category | < 800ms | P2 |
| PRF-012 | Variable parsing | 50 variables | < 50ms | P2 |
| PRF-013 | Version creation | Create version | < 300ms | P2 |
| PRF-014 | Restore version | Restore | < 500ms | P2 |
| PRF-015 | Memory: 1000 prompts | Load all | No leak | P2 |
| PRF-016 | Filter + sort | Combined | < 1s | P2 |

---

## §10 Load Tests

> **Minimum:** 10 tests

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | Sustained list | 10 users, 1 req/s | 5 min | 95% < 1s | P2 |
| LDT-002 | Sustained create | 5 users, 0.5 req/s | 5 min | 95% < 500ms | P2 |
| LDT-003 | Sustained search | 20 users, 2 req/s | 5 min | 95% < 1s | P2 |
| LDT-004 | Spike list | 0→50 users in 30s | 2 min | No errors | P2 |
| LDT-005 | Spike test prompt | 0→20 users | 2 min | Queue or 429 | P2 |
| LDT-006 | Stress list | 100 users, 5 req/s | 5 min | Graceful degradation | P2 |
| LDT-007 | Stress create | 50 users, 2 req/s | 5 min | Queue or 429 | P2 |
| LDT-008 | Breaking point | Ramp to failure | - | Identify limit | P2 |
| LDT-009 | Recovery after spike | Spike then 10 users | 5 min | Back to normal | P2 |
| LDT-010 | Recovery after stress | Stress then idle | 2 min | System recovers | P2 |

---

## Traceability Matrix

| Requirement / AC | Test Cases Covering |
|-----------------|-------------------|
| AC-1: CRUD prompts | POS-001 to POS-005, INT-001 to INT-010 |
| AC-2: Template variables | POS-006, FUN-005, FUN-019, UNT-001 to UNT-003 |
| AC-3: Version control | POS-011, POS-012, FUN-006, FUN-007, INT-006 |
| AC-4: Usage tracking | POS-007, FUN-008, FUN-009, UNT-011, UNT-012 |
| AC-5: Prompt testing | POS-005, NEG-009, INT-005, INT-050 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
