# Opportunity Creation — Comprehensive Test Cases

**JIRA References:** [PNO-687](https://unops.atlassian.net/browse/PNO-687) (From Partners), [PNO-688](https://unops.atlassian.net/browse/PNO-688) (From Interactions), [PNO-689](https://unops.atlassian.net/browse/PNO-689) (From Opportunity Page)  
**Created:** 2026-01-20  
**Restructured:** 2026-02-11 (10-category standard)  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30-50 | ✅ |
| 2 | Negative Tests | §2 | 90 | Max(50, 3×30=90) | ✅ |
| 3 | Boundary Tests | §3 | 90 | Max(50, 3×30=90) | ✅ |
| 4 | Functional Tests | §4 | 90 | Max(50, 3×30=90) | ✅ |
| 5 | Integration Tests | §5 | 90 | Max(50, 3×30=90) | ✅ |
| 6 | Security Tests | §6 | 50 | ≥50 | ✅ |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

**Ratio Checks:**
- N ≥ 3P: 90 ≥ 90 → ✅ PASS
- E ≥ 3P: 90 ≥ 90 → ✅ PASS
- F ≥ 3P: 90 ≥ 90 → ✅ PASS
- I ≥ 3P: 90 ≥ 90 → ✅ PASS

---

## Feature Overview

Opportunities can be created from 3 entry points:
1. **From Partners Page (PNO-687):** Partner pre-populated, AI Assistant can help fill fields
2. **From Interactions (PNO-688):** Partner and interaction data pre-populated, AI transcription available
3. **From Opportunity Page (PNO-689):** Blank form, manual entry

---

## §1 Positive Tests (Happy Path)

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

### PNO-687: Create from Partners Page (12 tests)

| ID | Test Name | Steps (Brief) | Expected Result | Priority |
|----|-----------|---------------|-----------------|----------|
| POS-001 | Create opportunity from partner list | Select partner → Click "Create Opportunity" | Form opens with partner pre-populated | P0 |
| POS-002 | Partner name pre-populated | Open creation form from partner | Partner field filled and read-only | P0 |
| POS-003 | Partner ID linked correctly | Create from partner → Save | Opportunity linked to correct partner | P0 |
| POS-004 | Save as Draft from partner context | Fill required fields → Save | Status=Draft, linked to partner | P0 |
| POS-005 | AI Assistant pre-fills from partner data | Click "Use AI" → Accept suggestions | Fields populated from partner profile | P1 |
| POS-006 | Upload document during creation | Attach file → Save | Document associated with opportunity | P1 |
| POS-007 | Descriptive information filled | Enter title, description → Save | All text fields saved correctly | P0 |
| POS-008 | Multiple partners linkable | Add 2nd partner during creation | Both partners linked | P1 |
| POS-009 | Navigate back to partner after creation | Create opp → Click partner breadcrumb | Returns to partner detail page | P2 |
| POS-010 | Opportunity visible in partner's opp list | Create from partner → View partner | Opportunity appears in partner's list | P1 |
| POS-011 | Currency auto-populated from partner | Partner has default currency | Currency field pre-filled | P2 |
| POS-012 | Country auto-populated from partner | Partner has country | Country of implementation pre-filled | P1 |

### PNO-688: Create from Interactions (12 tests)

| ID | Test Name | Steps (Brief) | Expected Result | Priority |
|----|-----------|---------------|-----------------|----------|
| POS-013 | Create from interaction detail | Click "Create Opportunity" on interaction | Form opens with partner + interaction pre-populated | P0 |
| POS-014 | Interaction notes transcribed by AI | AI transcribes interaction → Create opp | Transcription used to fill description | P1 |
| POS-015 | Partner from interaction pre-populated | Create from interaction with partner | Partner field auto-filled | P0 |
| POS-016 | Interaction ID linked | Create and save | Opportunity linked to source interaction | P0 |
| POS-017 | Multiple opportunities from same interaction | Create 2 opps from same interaction | Both created, both linked | P1 |
| POS-018 | Meeting notes populate description | Interaction has meeting notes | Notes appear in opportunity description | P1 |
| POS-019 | Contact from interaction linked | Interaction has contact | Contact suggested during creation | P2 |
| POS-020 | Save interaction-sourced opp as Draft | Fill fields → Save | Draft with interaction linkage | P0 |
| POS-021 | AI generates title from transcription | AI processes meeting | Title suggestion provided | P1 |
| POS-022 | Date from interaction sets context | Recent interaction | Interaction date shown as reference | P2 |
| POS-023 | Interaction type visible during creation | Meeting/Call/Email | Source type shown for context | P2 |
| POS-024 | Navigate back to interaction after create | Create → Click breadcrumb | Returns to interaction detail | P2 |

### PNO-689: Create from Opportunity Page (6 tests)

| ID | Test Name | Steps (Brief) | Expected Result | Priority |
|----|-----------|---------------|-----------------|----------|
| POS-025 | Create blank opportunity | Click "New Opportunity" on list | Empty form opens | P0 |
| POS-026 | Fill all required fields and save | Complete all mandatory fields | Opportunity saved successfully | P0 |
| POS-027 | Fill only required fields (minimal) | Only mandatory fields | Saves with minimal data | P0 |
| POS-028 | Assign OM during creation | Select OM from dropdown | OM assigned to opportunity | P0 |
| POS-029 | Select responsible org unit | Choose org unit | Org unit linked | P0 |
| POS-030 | Add funding partner manually | Search and select partner | Partner linked with amount | P1 |

---

## §2 Negative Tests (Failure Scenarios)

> **Count: 90** | **Minimum: Max(50, 3×30=90)** | ✅ COMPLIANT

### 2.1 Missing Required Fields (15 tests)

| ID | Missing Field | Entry Point | Expected Error | Priority |
|----|--------------|-------------|---------------|----------|
| NEG-001 | Opportunity Name | All | "Name is required" | P0 |
| NEG-002 | Responsible Org Unit | All | "Organization unit is required" | P0 |
| NEG-003 | Opportunity Manager | Direct | "OM is required" | P0 |
| NEG-004 | Description | All | "Description is required" | P0 |
| NEG-005 | All fields empty | Direct | Multiple validation errors listed | P0 |
| NEG-006 | Partner (when creating from partner page) | Partners | Should not occur (pre-populated) | P1 |
| NEG-007 | Budget amount without currency | All | "Currency required when amount specified" | P1 |
| NEG-008 | Implementation end without start | All | "Start date required" | P1 |
| NEG-009 | Country without org unit | All | "Org unit required" | P1 |
| NEG-010 | Funding partner without amount | All | "Amount required" | P1 |
| NEG-011 | Multiple missing fields | All | All errors shown simultaneously | P0 |
| NEG-012 | SDG not selected | All | "At least one SDG required" (if mandatory) | P1 |
| NEG-013 | Product not selected | All | "At least one product required" (if mandatory) | P1 |
| NEG-014 | Target signing date missing | All | "Signing date required" (if mandatory) | P1 |
| NEG-015 | Initiative type not selected | All | "Initiative type required" | P1 |

### 2.2 Invalid Input Data (15 tests)

| ID | Field | Invalid Input | Expected Error | Priority |
|----|-------|--------------|---------------|----------|
| NEG-016 | Name | Only whitespace | "Name cannot be blank" | P0 |
| NEG-017 | Budget amount | Negative number (-1000) | "Amount must be positive" | P0 |
| NEG-018 | Budget amount | Non-numeric ("abc") | "Invalid number format" | P1 |
| NEG-019 | Implementation end date | Before start date | "End date must be after start date" | P0 |
| NEG-020 | Email in contact link | Invalid email format | "Invalid email format" | P1 |
| NEG-021 | Description | Only HTML tags | Tags stripped/escaped | P1 |
| NEG-022 | Budget amount | Extremely large (10^15) | "Amount exceeds maximum" | P1 |
| NEG-023 | Name | 256+ characters (over max) | "Name too long" | P1 |
| NEG-024 | Description | Over max length | "Description too long" | P1 |
| NEG-025 | Budget with 5+ decimals | 100.12345 | "Max 2 decimal places" | P2 |
| NEG-026 | Date in invalid format | "not-a-date" | "Invalid date format" | P1 |
| NEG-027 | Budget = NaN | NaN string | "Invalid number" | P2 |
| NEG-028 | Duplicate opportunity name | Same name as existing | Warning or accept (business rule) | P1 |
| NEG-029 | Partner ID = 0 | Zero ID | "Invalid partner" | P2 |
| NEG-030 | Org unit ID = -1 | Negative ID | "Invalid organization unit" | P2 |

### 2.3 Unauthorized Access (10 tests)

| ID | User Role | Action | Expected Result | Priority |
|----|-----------|--------|-----------------|----------|
| NEG-031 | Unauthenticated | Create opportunity | 401 Unauthorized | P0 |
| NEG-032 | User without create permission | Click "New Opportunity" | Button not visible / 403 | P0 |
| NEG-033 | Partner User (external) | Create from partner page | Action not available | P0 |
| NEG-034 | Read-only user | Submit creation form | 403 Forbidden | P1 |
| NEG-035 | User from different org unit | Create in restricted org unit | 403 Forbidden | P1 |
| NEG-036 | Expired session | Submit form | Redirect to login | P1 |
| NEG-037 | Token tampered | API create request | 401 Unauthorized | P1 |
| NEG-038 | User without partner view | Create from partner page | Partner page not accessible | P1 |
| NEG-039 | Collaborator without create | Direct API POST | 403 Forbidden | P1 |
| NEG-040 | Deactivated user account | Any action | Account locked/redirect | P1 |

### 2.4 Invalid State/Context (10 tests)

| ID | Scenario | Expected Result | Priority |
|----|----------|-----------------|----------|
| NEG-041 | Create from deleted partner | "Partner not found" or action hidden | P0 |
| NEG-042 | Create from archived interaction | "Interaction archived" warning | P1 |
| NEG-043 | Create from inactive partner | Warning: "Partner is inactive" | P1 |
| NEG-044 | Create from partner with no org unit | Error: "No org unit for partner" | P1 |
| NEG-045 | Select deactivated OM | OM not shown in dropdown | P1 |
| NEG-046 | Select deactivated org unit | Org unit not selectable | P1 |
| NEG-047 | Create from partner being edited by another user | Handle gracefully (no lock needed for read) | P2 |
| NEG-048 | Submit form after browser back button | Prevent duplicate submission | P1 |
| NEG-049 | API rate limit on rapid creation | 429 Too Many Requests | P2 |
| NEG-050 | Create during system maintenance | Graceful error message | P2 |

### 2.5 Dependency Failures (10 tests)

| ID | Failure | Expected Behavior | Priority |
|----|---------|-------------------|----------|
| NEG-051 | Database timeout during save | Error message, no partial save | P1 |
| NEG-052 | AI service unavailable | Manual entry still works, AI button disabled | P1 |
| NEG-053 | File storage unavailable | Document upload fails with message, opp saves | P1 |
| NEG-054 | Partner search service down | Typeahead shows error, manual ID entry | P2 |
| NEG-055 | Org unit hierarchy unavailable | Error: "Cannot load org units" | P2 |
| NEG-056 | Network disconnect during save | Error: "Connection lost" + retry | P1 |
| NEG-057 | Auth service timeout | Redirect to login | P1 |
| NEG-058 | SDG reference data unavailable | SDG dropdown shows error | P2 |
| NEG-059 | Country reference data unavailable | Country dropdown shows error | P2 |
| NEG-060 | Product catalog unavailable | Product dropdown shows error | P2 |

### 2.6 Form Behavior Failures (10 tests)

| ID | Scenario | Expected Behavior | Priority |
|----|----------|-------------------|----------|
| NEG-061 | Close form without saving (dirty) | Confirmation dialog: "Unsaved changes" | P0 |
| NEG-062 | Navigate away with unsaved data | Confirmation dialog | P0 |
| NEG-063 | Double-click Save button | Only one save executes | P0 |
| NEG-064 | Submit form via Enter key on text field | Does not submit (only Save button) | P2 |
| NEG-065 | Browser refresh during form fill | Data lost, form resets | P1 |
| NEG-066 | Paste very large text into description | Text truncated at max or error | P1 |
| NEG-067 | JavaScript disabled | Form gracefully degrades | P2 |
| NEG-068 | Clear pre-populated partner field | Warning: "Partner is required" | P1 |
| NEG-069 | Tab out of required field without filling | Inline validation shows error | P1 |
| NEG-070 | Invalid URL in reference field | "Invalid URL format" | P2 |

### 2.7 Opportunity Creation Failures (20 tests)

| ID | Scenario | Expected Result | Priority |
|----|----------|-----------------|----------|
| NEG-071 | Create with non-existent partner ID | 400 "Partner not found" | P0 |
| NEG-072 | Create with soft-deleted partner ID | 400 "Partner not available" | P1 |
| NEG-073 | Create with non-existent interaction ID | 400 "Interaction not found" | P1 |
| NEG-074 | Create with interaction from different partner | 400 "Interaction partner mismatch" | P1 |
| NEG-075 | Create with invalid currency code | 400 "Invalid currency" | P1 |
| NEG-076 | Create with future implementation start before signing | Warning or reject per business rule | P1 |
| NEG-077 | Create with funding amount exceeding total budget | 400 "Funding exceeds budget" | P1 |
| NEG-078 | Create with duplicate funding partner | 400 "Duplicate funding partner" | P1 |
| NEG-079 | Create with non-existent SDG code | 400 "Invalid SDG" | P1 |
| NEG-080 | Create with non-existent country code | 400 "Invalid country" | P1 |
| NEG-081 | Create with non-existent product ID | 400 "Product not found" | P1 |
| NEG-082 | Create with non-existent strategic mission | 400 "Invalid strategic mission" | P1 |
| NEG-083 | Create with OM from different org unit (restricted) | 403 or validation error | P1 |
| NEG-084 | Create with stakeholder who is not active user | 400 "Invalid stakeholder" | P2 |
| NEG-085 | Create with circular partner reference | 400 "Invalid partner linkage" | P2 |
| NEG-086 | Create with implementation dates in past (if blocked) | 400 "Dates must be in future" | P1 |
| NEG-087 | Create with budget in unsupported currency | 400 "Currency not supported" | P2 |
| NEG-088 | Create with malformed JSON in API request | 400 "Invalid request body" | P1 |
| NEG-089 | Create with Content-Type mismatch (e.g., form instead of JSON) | 400 or 415 Unsupported Media Type | P1 |
| NEG-090 | Create with null/undefined for required nested object | 400 "Required field missing" | P0 |

---

## §3 Boundary Tests (Edge Cases)

> **Count: 90** | **Minimum: Max(50, 3×30=90)** | ✅ COMPLIANT

### 3.1 String Length Boundaries (15 tests)

| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Opportunity Name | 1 | 255 | ✅ | ✅ | ❌ | P1 |
| BND-002 | Description | 1 | 10000 | ✅ | ✅ | ❌ | P1 |
| BND-003 | Short Description | 0 | 500 | ✅ | ✅ | ❌ | P1 |
| BND-004 | Context & Challenges | 1 | 10000 | ✅ | ✅ | ❌ | P2 |
| BND-005 | Expected Impact | 1 | 5000 | ✅ | ✅ | ❌ | P2 |
| BND-006 | Expected Outcomes | 1 | 5000 | ✅ | ✅ | ❌ | P2 |
| BND-007 | Name = exactly 1 char | — | — | ✅ | — | — | P2 |
| BND-008 | Name = exactly 255 chars | — | — | — | ✅ | — | P2 |
| BND-009 | Description = exactly 10000 chars | — | — | — | ✅ | — | P2 |
| BND-010 | AI-generated text at max length | — | — | — | ✅ (truncated) | — | P2 |
| BND-011 | Empty string (not null) for optional field | — | — | ✅ | — | — | P2 |
| BND-012 | Name with leading/trailing spaces | — | — | Trimmed | — | — | P1 |
| BND-013 | Description with only newlines | — | — | ✅ Accept | — | — | P2 |
| BND-014 | Name with consecutive spaces | — | — | Normalized | — | — | P2 |
| BND-015 | All text fields at maximum simultaneously | — | — | — | ✅ All saved | — | P1 |

### 3.2 Numeric Boundaries (10 tests)

| ID | Field | Zero | Negative | Very Large | Precision | Priority |
|----|-------|------|----------|-----------|-----------|----------|
| BND-016 | Budget amount | ✅ (0) | ❌ | ✅ (999,999,999.99) | 2 dec | P1 |
| BND-017 | Funding amount | ✅ (0) | ❌ | ✅ | 2 dec | P1 |
| BND-018 | Estimated beneficiaries | ✅ (0) | ❌ | ✅ (MAX_INT) | Integer | P1 |
| BND-019 | Budget = 0.01 (min positive) | ✅ | — | — | — | P2 |
| BND-020 | Budget = 999,999,999.99 (max) | — | — | ✅ | — | P2 |
| BND-021 | Budget = 1,000,000,000 (overflow) | — | — | ❌ | — | P2 |
| BND-022 | Beneficiaries = 1 | ✅ | — | — | — | P2 |
| BND-023 | Beneficiaries = 0 | ✅ | — | — | — | P2 |
| BND-024 | Multiple funding amounts sum to 0 | — | — | ❌ (total must be >0) | — | P1 |
| BND-025 | Budget with currency conversion | — | — | Precision preserved | — | P2 |

### 3.3 Date Boundaries (10 tests)

| ID | Date Input | Expected Result | Priority |
|----|-----------|-----------------|----------|
| BND-026 | Target signing = today | ✅ Accept | P1 |
| BND-027 | Target signing = yesterday | ⚠️ Warning | P1 |
| BND-028 | Implementation start = today | ✅ Accept | P1 |
| BND-029 | Implementation end = start date (0 duration) | ✅ Accept | P2 |
| BND-030 | Implementation span = 10 years | ✅ Accept | P2 |
| BND-031 | Feb 29 leap year | ✅ Accept | P2 |
| BND-032 | Year boundary (Dec 31 → Jan 1) | ✅ Correct fiscal year | P2 |
| BND-033 | Implementation end = 2099 | ✅ Accept or ⚠️ | P2 |
| BND-034 | All dates = same date | ✅ Accept | P2 |
| BND-035 | No dates set (all optional) | ✅ Saves without dates | P1 |

### 3.4 Collection Boundaries (15 tests)

| ID | Collection | State | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| BND-036 | Funding Partners | 0 (none) | ⚠️ or ❌ (depends on mandatory) | P1 |
| BND-037 | Funding Partners | 1 (minimum) | ✅ | P1 |
| BND-038 | Funding Partners | 20+ | ✅ | P1 |
| BND-039 | Countries | 1 | ✅ | P1 |
| BND-040 | Countries | All (~200) | ✅ or defined limit | P2 |
| BND-041 | SDGs | 1 | ✅ | P1 |
| BND-042 | SDGs | All 17 | ✅ | P1 |
| BND-043 | Products | 1 | ✅ | P1 |
| BND-044 | Products | 100+ | ✅ or defined limit | P2 |
| BND-045 | Documents attached | 0 | ✅ | P1 |
| BND-046 | Documents attached | 1 | ✅ | P1 |
| BND-047 | Documents attached | 50+ | ✅ or defined limit | P2 |
| BND-048 | Strategic Missions | 1 | ✅ | P1 |
| BND-049 | Strategic Missions | All | ✅ | P2 |
| BND-050 | Stakeholders | 0 | ✅ (optional) | P1 |

### 3.5 Unicode & Special Characters (10 tests)

| ID | Field | Input | Expected Result | Priority |
|----|-------|-------|-----------------|----------|
| BND-051 | Name | Arabic: "فرصة جديدة" | ✅ Stored correctly | P1 |
| BND-052 | Name | Chinese: "新机会" | ✅ Stored correctly | P1 |
| BND-053 | Description | French accents: "Opportunité créée" | ✅ | P1 |
| BND-054 | Name | Emoji: "New Opportunity 🌍" | ✅ or ❌ with clear msg | P2 |
| BND-055 | Description | HTML entities: &amp; &lt; | Escaped, not rendered | P1 |
| BND-056 | Description | Mixed RTL/LTR | Rendered correctly | P2 |
| BND-057 | Name | Diacritics: "São Paulo" | ✅ | P1 |
| BND-058 | Budget | Locale format: "1.234,56" (EU) | Parsed correctly | P1 |
| BND-059 | Description | Tab characters | Preserved | P2 |
| BND-060 | Name | SQL-like chars: O'Brien & Sons | ✅ Escaped in query | P1 |

### 3.6 Entry Point Boundaries (10 tests)

| ID | Boundary | Expected Result | Priority |
|----|----------|-----------------|----------|
| BND-061 | Create from partner with 0 existing opps | ✅ First opportunity created | P1 |
| BND-062 | Create from partner with 100+ existing opps | ✅ 101st created | P1 |
| BND-063 | Create from interaction with no notes | ✅ Description empty | P1 |
| BND-064 | Create from interaction with max-length notes | ✅ Truncated if needed | P2 |
| BND-065 | Rapidly create 10 opps from same partner | All 10 created independently | P1 |
| BND-066 | Create from partner with longest possible name | Partner name displayed correctly | P2 |
| BND-067 | Create from interaction linked to deleted partner | Handle gracefully | P1 |
| BND-068 | Open creation form, wait 30min, then save | Session handles timeout | P2 |
| BND-069 | Create with exactly 1 of every collection type | ✅ Minimum viable data | P1 |
| BND-070 | AI suggestion with 0 confidence | Suggestion shown with disclaimer | P2 |

### 3.7 Additional Boundary Cases (20 tests)

| ID | Boundary | Expected Result | Priority |
|----|----------|-----------------|----------|
| BND-071 | Name at 254 characters (one below max) | ✅ Accept | P1 |
| BND-072 | Description at 9999 characters (one below max) | ✅ Accept | P1 |
| BND-073 | Budget = 0.00 with currency | ✅ Accept | P1 |
| BND-074 | Funding amount = 0.01 (min positive) | ✅ Accept | P2 |
| BND-075 | Implementation start = end of year (Dec 31) | ✅ Accept | P2 |
| BND-076 | Implementation end = start of year (Jan 1) | ✅ Accept | P2 |
| BND-077 | Exactly 2 funding partners (min for split) | ✅ Accept | P1 |
| BND-078 | Exactly 17 SDGs (all selected) | ✅ Accept | P1 |
| BND-079 | Exactly 1 country of implementation | ✅ Accept | P1 |
| BND-080 | Exactly 1 product selected | ✅ Accept | P1 |
| BND-081 | Target signing = implementation start (same day) | ✅ Accept | P2 |
| BND-082 | Name with single space only | Trimmed or reject per rule | P2 |
| BND-083 | Description with exactly 1 character | ✅ Accept if min=1 | P2 |
| BND-084 | Beneficiaries = MAX_INT - 1 | ✅ Accept or defined limit | P2 |
| BND-085 | Create with 0 documents (optional) | ✅ Accept | P1 |
| BND-086 | Create with 1 stakeholder only | ✅ Accept | P2 |
| BND-087 | Currency with 3-letter ISO code boundary | ✅ Accept valid codes | P2 |
| BND-088 | Implementation span = 1 day | ✅ Accept | P2 |
| BND-089 | Name with Unicode combining characters | ✅ Normalized/stored | P2 |
| BND-090 | Create from partner at org unit boundary (root vs leaf) | ✅ Accept valid hierarchy | P2 |

---

## §4 Functional Tests (Business Rules)

> **Count: 90** | **Minimum: Max(50, 3×30=90)** | ✅ COMPLIANT

### 4.1 Workflow Rules (15 tests)

| ID | Business Rule | Test Scenario | Expected Outcome | Priority |
|----|--------------|--------------|-----------------|----------|
| FUN-001 | New opportunity defaults to I&P/Draft | Create any opp | Stage=I&P, Status=Draft | P0 |
| FUN-002 | OM auto-assigned if not specified | Create without OM | Current user = OM | P0 |
| FUN-003 | Partner linkage is immutable after creation | Try to change source partner | Partner field read-only after save | P1 |
| FUN-004 | Interaction linkage preserved | Create from interaction | Interaction ID stored | P1 |
| FUN-005 | Creation source tracked (Partner/Interaction/Direct) | View opp detail | Source type displayed | P1 |
| FUN-006 | Draft opp is editable | Edit any field after save | Changes persist | P0 |
| FUN-007 | Soft delete on creation | Accidentally create → delete | IsDeleted=true, not physically removed | P1 |
| FUN-008 | Audit trail on creation | Create opp | CreatedBy, CreatedDate set | P0 |
| FUN-009 | ModifiedBy updated on edit | Edit opp | ModifiedBy=current user | P1 |
| FUN-010 | Opportunity appears in list after creation | Create → navigate to list | New opp visible | P0 |
| FUN-011 | Opportunity searchable after creation | Create → search by name | Found in search results | P1 |
| FUN-012 | Notification sent on creation (if configured) | Create opp | Notification to OM/stakeholders | P2 |
| FUN-013 | Sequential ID assignment | Create 3 opps | IDs are sequential | P2 |
| FUN-014 | Creation timestamp in UTC | Create from any timezone | Stored as UTC | P1 |
| FUN-015 | OM name displayed correctly | Create with OM | OM full name shown, not ID | P1 |

### 4.2 Validation Rules (15 tests)

| ID | Validation Rule | Valid | Invalid | Priority |
|----|----------------|-------|---------|----------|
| FUN-016 | Name required and non-blank | "New Opp" | "" / null / whitespace | P0 |
| FUN-017 | Org unit must exist in system | Valid org unit | Deleted org unit | P0 |
| FUN-018 | OM must be active user | Active user | Deactivated user | P0 |
| FUN-019 | End date ≥ start date | End > Start | End < Start | P0 |
| FUN-020 | Budget ≥ 0 | 0 or positive | Negative | P0 |
| FUN-021 | Currency required with budget | Amount + currency | Amount without currency | P1 |
| FUN-022 | At least 1 funding partner (if mandatory) | 1+ partners | 0 partners | P1 |
| FUN-023 | Server-side re-validation | Pass client, fail server | Server rejects | P0 |
| FUN-024 | Name max length 255 | 255 chars | 256 chars | P1 |
| FUN-025 | Description max length enforced | At max | Over max | P1 |
| FUN-026 | Date format validation | ISO format | Free text | P1 |
| FUN-027 | Integer-only fields reject decimals | Beneficiaries = 5 | Beneficiaries = 5.5 | P2 |
| FUN-028 | Partner must not be soft-deleted | Active partner | IsDeleted=true partner | P1 |
| FUN-029 | Org unit must not be deactivated | Active org unit | Inactive org unit | P1 |
| FUN-030 | All validations shown at once (not one-at-a-time) | Multiple errors | All errors listed | P0 |

### 4.3 Constraint Rules (10 tests)

| ID | Constraint | Test | Expected | Priority |
|----|-----------|------|----------|----------|
| FUN-031 | Opportunity Name uniqueness (if enforced) | Duplicate name | Warning or accept | P1 |
| FUN-032 | OM field never blank | Remove OM | Prevented | P0 |
| FUN-033 | Stage cannot be set during creation | Try to set stage=GO | Field not available | P1 |
| FUN-034 | Status cannot be set during creation | Try to set status=Active | Field not available | P1 |
| FUN-035 | Deleted partners excluded from dropdown | Soft-deleted partner | Not in selection list | P0 |
| FUN-036 | Inactive org units excluded from dropdown | Inactive org unit | Not in selection list | P1 |
| FUN-037 | Document size limit enforced | Upload 100MB file | "File too large" | P1 |
| FUN-038 | Document type restriction | Upload .exe file | "File type not allowed" | P1 |
| FUN-039 | Max documents per opportunity | Upload beyond limit | "Maximum documents reached" | P2 |
| FUN-040 | Budget precision constraint | 2 decimal places | Values rounded | P2 |

### 4.4 Audit Rules (10 tests)

| ID | Action | Expected Audit Entry | Priority |
|----|--------|---------------------|----------|
| FUN-041 | Create opportunity | CreatedBy=[user], CreatedDate=[UTC], Action="Create" | P0 |
| FUN-042 | Edit opportunity | ModifiedBy=[user], ModifiedDate=[UTC] | P0 |
| FUN-043 | Delete opportunity (soft) | DeletedBy=[user], DeletedDate=[UTC], IsDeleted=true | P0 |
| FUN-044 | Add funding partner | Change logged with partner name + amount | P1 |
| FUN-045 | Remove funding partner | Change logged with partner name | P1 |
| FUN-046 | Upload document | Document name + upload timestamp | P1 |
| FUN-047 | Change OM | Old OM + New OM logged | P1 |
| FUN-048 | Change org unit | Old + New org unit logged | P1 |
| FUN-049 | AI-assisted field population | AI source noted in audit | P2 |
| FUN-050 | Bulk edit (if supported) | Each change individually audited | P2 |

### 4.5 Additional Functional Rules (40 tests)

| ID | Business Rule | Test Scenario | Expected Outcome | Priority |
|----|--------------|--------------|-----------------|----------|
| FUN-051 | Partner pre-population from partner page | Create from partner | Partner ID and name locked | P0 |
| FUN-052 | Interaction pre-population from interaction | Create from interaction | Partner + interaction linked | P0 |
| FUN-053 | Direct creation has no pre-population | Create from opportunity page | All fields empty | P0 |
| FUN-054 | Funding partner amount must be ≤ total budget | Add funding > budget | Validation error | P1 |
| FUN-055 | Sum of funding amounts can equal total budget | Funding sum = budget | ✅ Accept | P1 |
| FUN-056 | Country must be in allowed list | Select valid country | ✅ Accept | P1 |
| FUN-057 | SDG must be 1-17 | Select valid SDG | ✅ Accept | P1 |
| FUN-058 | Product must exist in catalog | Select valid product | ✅ Accept | P1 |
| FUN-059 | Strategic mission must exist | Select valid mission | ✅ Accept | P1 |
| FUN-060 | Stakeholder must be active user | Select active user | ✅ Accept | P1 |
| FUN-061 | Name trimming on save | Enter "  Name  " | Stored as "Name" | P1 |
| FUN-062 | Description preserves line breaks | Multi-line description | Line breaks preserved | P1 |
| FUN-063 | Currency code case-insensitive | "usd" or "USD" | Normalized to USD | P2 |
| FUN-064 | Duplicate country selection prevented | Select same country twice | Single selection or warning | P2 |
| FUN-065 | Duplicate SDG selection prevented | Select same SDG twice | Single selection or warning | P2 |
| FUN-066 | Opportunity inherits partner currency (from partner) | Create from partner with EUR | Currency = EUR | P1 |
| FUN-067 | Opportunity inherits partner country (from partner) | Create from partner with country | Country pre-filled | P1 |
| FUN-068 | WorkflowStatus set to Draft on creation | Create any opp | WorkflowStatus = Draft | P0 |
| FUN-069 | Status field not editable during creation | Try to set status | Field disabled/hidden | P1 |
| FUN-070 | Stage field not editable during creation | Try to set stage | Field disabled/hidden | P1 |
| FUN-071 | CreatedBy set to current user | Create opp | CreatedBy = current user ID | P0 |
| FUN-072 | CreatedDate set to server time | Create opp | CreatedDate = UTC timestamp | P0 |
| FUN-073 | LastModifiedBy null on creation | New opp | LastModifiedBy = null | P1 |
| FUN-074 | LastModifiedDate null on creation | New opp | LastModifiedDate = null | P1 |
| FUN-075 | IsDeleted = false on creation | New opp | IsDeleted = false | P0 |
| FUN-076 | Name required even when creating from interaction | Omit name | Validation error | P0 |
| FUN-077 | Org unit required from all entry points | Omit org unit | Validation error | P0 |
| FUN-078 | OM required when creating from opportunity page | Omit OM (direct) | Validation error | P0 |
| FUN-079 | OM optional when creating from partner | Create from partner without OM | Auto-assign or allow | P1 |
| FUN-080 | Interaction link optional when creating from interaction | Create without linking | Can save without interaction | P2 |
| FUN-081 | Document upload does not block save | Upload fails, other fields valid | Opportunity saves without doc | P1 |
| FUN-082 | AI suggestion can be partially accepted | Accept 2 of 5 suggestions | Only accepted fields updated | P2 |
| FUN-083 | AI suggestion can be rejected | Reject all suggestions | Form unchanged | P2 |
| FUN-084 | Breadcrumb reflects creation source | Create from partner | Breadcrumb shows Partner > New Opp | P2 |
| FUN-085 | Return URL after save (from partner) | Create from partner, save | Redirect to partner or opp detail | P1 |
| FUN-086 | Return URL after save (from interaction) | Create from interaction, save | Redirect to interaction or opp detail | P1 |
| FUN-087 | Return URL after save (direct) | Create from opportunity page, save | Redirect to opp detail or list | P1 |
| FUN-088 | Opportunity visible to OM immediately | Create, assign OM | OM sees in "My Opportunities" | P0 |
| FUN-089 | Opportunity visible to org unit members | Create in org unit | Org unit members can view | P1 |
| FUN-090 | Opportunity not visible to unauthorized users | Create, view as other user | 403 or not in list | P0 |

---

## §5 Integration Tests (End-to-End Flows)

> **Count: 90** | **Minimum: Max(50, 3×30=90)** | ✅ COMPLIANT

### 5.1 CRUD Workflow (10 tests)

| ID | Flow | Expected Result | Priority |
|----|------|-----------------|----------|
| INT-001 | Create → Read → Update → Delete (full CRUD) | All operations succeed, audit complete | P0 |
| INT-002 | Create from partner → View in partner detail | Opp visible in partner's opportunity list | P0 |
| INT-003 | Create from interaction → View in interaction | Opp linked in interaction detail | P0 |
| INT-004 | Create → Edit name → Verify search finds new name | Search returns updated name | P1 |
| INT-005 | Create → Add partner → Remove partner → Save | Partner removal persisted | P1 |
| INT-006 | Create → Upload doc → Download doc | Document round-trip works | P1 |
| INT-007 | Create → Edit → Navigate away → Return | Latest saved state displayed | P1 |
| INT-008 | Create → Soft delete → Verify not in list | Deleted opp hidden from list | P0 |
| INT-009 | Create minimal → Add more data later | Incremental updates work | P1 |
| INT-010 | Create → Assign new OM → Verify OM access | New OM can edit opportunity | P0 |

### 5.2 Search & Filter (10 tests)

| ID | Search/Filter | Expected Results | Priority |
|----|--------------|-----------------|----------|
| INT-011 | Search by opportunity name | Matching opps returned | P0 |
| INT-012 | Filter by stage (I&P) | Only I&P opps | P1 |
| INT-013 | Filter by OM | OM's opportunities | P1 |
| INT-014 | Filter by partner | Linked opps for partner | P1 |
| INT-015 | Filter by country | Country-filtered opps | P1 |
| INT-016 | Search by description keyword | Full-text search results | P1 |
| INT-017 | Combined filters (stage + OM + country) | Intersection of criteria | P1 |
| INT-018 | Sort by creation date | Chronological order | P2 |
| INT-019 | Sort by name (A-Z / Z-A) | Alphabetical order | P2 |
| INT-020 | Search with no results | "No opportunities found" message | P1 |

### 5.3 Pagination (5 tests)

| ID | Scenario | Expected Result | Priority |
|----|----------|-----------------|----------|
| INT-021 | First page (10 per page) | First 10 shown | P2 |
| INT-022 | Last page | Remaining items | P2 |
| INT-023 | Change page size | Items per page updates | P2 |
| INT-024 | 100+ opportunities in list | Paginated correctly | P2 |
| INT-025 | Empty list (no opps) | "No opportunities" message | P2 |

### 5.4 Relationships (10 tests)

| ID | Relationship | Test | Expected Result | Priority |
|----|-------------|------|-----------------|----------|
| INT-026 | Opportunity → Partner (N:M) | Create with 2 partners | Both linked | P0 |
| INT-027 | Opportunity → Org Unit (N:1) | Assign org unit | Correct linkage | P0 |
| INT-028 | Opportunity → OM User (N:1) | Assign OM | OM linked correctly | P0 |
| INT-029 | Opportunity → Documents (1:N) | Upload 3 docs | All linked | P1 |
| INT-030 | Opportunity → Countries (N:M) | Select 5 countries | All linked | P1 |
| INT-031 | Opportunity → SDGs (N:M) | Select 3 SDGs | All linked | P1 |
| INT-032 | Opportunity → Products (N:M) | Select 2 products | All linked | P1 |
| INT-033 | Opportunity → Interaction (N:1) | Create from interaction | Linked | P0 |
| INT-034 | Opportunity → Source Partner (N:1) | Create from partner | Linked | P0 |
| INT-035 | Cascade behavior on partner delete | Soft-delete partner | Opp still exists, partner link shows deleted | P1 |

### 5.5 Error Handling (15 tests)

| ID | Error Condition | Expected Response | Priority |
|----|----------------|------------------|----------|
| INT-036 | POST with missing required field | 400 + field errors | P0 |
| INT-037 | POST with invalid org unit | 400 "Invalid org unit" | P0 |
| INT-038 | POST with non-existent partner ID | 400 "Partner not found" | P1 |
| INT-039 | POST without auth token | 401 Unauthorized | P0 |
| INT-040 | POST with expired token | 401 Token expired | P1 |
| INT-041 | POST with malformed JSON | 400 "Invalid request" | P1 |
| INT-042 | PUT on non-existent opp | 404 Not Found | P1 |
| INT-043 | DELETE on non-existent opp | 404 Not Found | P1 |
| INT-044 | POST exceeding rate limit | 429 | P2 |
| INT-045 | POST during DB maintenance | 503 Service Unavailable | P2 |
| INT-046 | GET deleted opportunity | 404 (soft-deleted) | P1 |
| INT-047 | POST with concurrent creation (same name) | Both succeed or handled | P1 |
| INT-048 | Upload document exceeding size | 413 Payload Too Large | P1 |
| INT-049 | Upload unsupported file type | 400 "File type not allowed" | P1 |
| INT-050 | API call with extra unknown fields | Extra fields ignored (no error) | P2 |

### 5.6 Additional Integration Flows (40 tests)

| ID | Flow | Expected Result | Priority |
|----|------|-----------------|----------|
| INT-051 | Create from partner → Partner detail shows opp count +1 | Count incremented | P1 |
| INT-052 | Create from interaction → Interaction detail shows linked opp | Link visible | P1 |
| INT-053 | Create → Dashboard widget updates | New opp in dashboard count | P2 |
| INT-054 | Create → Notification service receives event | Notification created (if configured) | P2 |
| INT-055 | Create with funding partners → Budget total calculated | Sum displayed correctly | P1 |
| INT-056 | Create with multiple countries → Filter by any country finds opp | Filter works | P1 |
| INT-057 | Create with multiple SDGs → Filter by any SDG finds opp | Filter works | P1 |
| INT-058 | Create → Export to CSV includes new opp | Export contains new record | P2 |
| INT-059 | Create → Report includes new opp | Report reflects new data | P2 |
| INT-060 | Create from partner A → Create from partner B → Both in list | Both visible, correct linkage | P1 |
| INT-061 | Create → AI insights refresh (if enabled) | Insights updated | P2 |
| INT-062 | Create → Audit log query returns create event | Audit traceable | P1 |
| INT-063 | Create with document → Document service stores file | File in storage | P1 |
| INT-064 | Create → Permission endpoint returns correct flags | canEdit, canDelete etc. correct | P1 |
| INT-065 | Create from partner → Partner API returns opp in related | Related opportunities include new | P1 |
| INT-066 | Create from interaction → Interaction API returns opp in related | Related opportunities include new | P1 |
| INT-067 | Create → Workflow component shows correct stage | I&P/Draft displayed | P0 |
| INT-068 | Create → Breadcrumb navigation works | All breadcrumb links valid | P2 |
| INT-069 | Create → Browser back after save | No duplicate, correct state | P1 |
| INT-070 | Create → Session timeout → Retry save | Re-auth, then save succeeds | P2 |
| INT-071 | Create with all optional fields → Full detail view | All data displayed | P1 |
| INT-072 | Create minimal → Edit to add optional → Save | Incremental data persists | P1 |
| INT-073 | Create → Duplicate (if feature exists) | Copy created correctly | P2 |
| INT-074 | Create → Share link (if feature exists) | Link works for authorized user | P2 |
| INT-075 | Create → Print/PDF (if feature exists) | Renders correctly | P2 |
| INT-076 | Create from partner → Partner soft-deleted later | Opp still accessible, partner shows deleted | P1 |
| INT-077 | Create from interaction → Interaction archived later | Opp still accessible | P1 |
| INT-078 | Create → Org unit changed in system | Opp retains original org unit | P1 |
| INT-079 | Create → OM deactivated later | Opp retains OM, read-only or reassign | P1 |
| INT-080 | Create with 10 funding partners → All displayed in detail | UI shows all 10 | P1 |
| INT-081 | Create with 17 SDGs → All displayed | All SDGs shown | P1 |
| INT-082 | Create with 20 countries → All displayed | All countries shown | P2 |
| INT-083 | Create → Search by OM name | Found | P1 |
| INT-084 | Create → Search by org unit name | Found | P1 |
| INT-085 | Create → Search by partner name | Found | P1 |
| INT-086 | Create → Filter by date range (creation) | Found in range | P1 |
| INT-087 | Create → Bulk operations (if supported) | Selectable in bulk list | P2 |
| INT-088 | Create → oUP sync (if integrated) | Synced or queued | P2 |
| INT-089 | Create → External system webhook (if configured) | Webhook triggered | P2 |
| INT-090 | Create → Full E2E from login to opp detail | Complete flow succeeds | P0 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 6.1 Injection Prevention (10)

| ID | Vector | Target | Payload | Expected Block | Priority |
|----|--------|--------|---------|---------------|----------|
| SEC-001 | SQL Injection | Name | `'; DROP TABLE--` | Parameterized, no effect | P0 |
| SEC-002 | SQL Injection | Description | `' OR 1=1--` | Escaped | P0 |
| SEC-003 | XSS | Name | `<script>alert(1)</script>` | HTML escaped | P0 |
| SEC-004 | XSS | Description | `<img onerror=alert(1) src=x>` | Sanitized | P0 |
| SEC-005 | XSS | Partner name display | `<svg onload=alert(1)>` | Escaped on render | P0 |
| SEC-006 | Command injection | Name | `$(rm -rf /)` | Stored as text | P1 |
| SEC-007 | Path traversal | Document upload | `../../etc/passwd` | Rejected | P1 |
| SEC-008 | JSON injection | API body | `{"__proto__":{"admin":true}}` | Blocked | P1 |
| SEC-009 | LDAP injection | User search | `*)(objectClass=*)` | Rejected | P2 |
| SEC-010 | Header injection | API header | `\r\nBCC:evil@hack.com` | Blocked | P2 |

### 6.2 Broken Access Control (10)

| ID | Role | Action | Expected | Priority |
|----|------|--------|----------|----------|
| SEC-011 | Unauthenticated | POST /api/opportunity | 401 | P0 |
| SEC-012 | No create permission | POST /api/opportunity | 403 | P0 |
| SEC-013 | Partner User (external) | POST /api/opportunity | 403 | P0 |
| SEC-014 | Read-only user | POST /api/opportunity | 403 | P0 |
| SEC-015 | Wrong org unit user | POST with restricted org unit | 403 | P1 |
| SEC-016 | Expired session | POST /api/opportunity | 401 | P1 |
| SEC-017 | Revoked permissions mid-session | POST /api/opportunity | 403 | P1 |
| SEC-018 | User with view-only on partners | Create from partner | 403 | P1 |
| SEC-019 | Service account | POST /api/opportunity | Per config | P2 |
| SEC-020 | Admin bypassing normal flow | Direct API | Allowed if authorized | P2 |

### 6.3 IDOR (10)

| ID | Object | Manipulation | Expected | Priority |
|----|--------|-------------|----------|----------|
| SEC-021 | Partner ID | Change to another user's partner | 403 | P0 |
| SEC-022 | Org Unit ID | Change to restricted org unit | 403 | P0 |
| SEC-023 | Opportunity ID (after creation) | Access other user's opp | 403 | P0 |
| SEC-024 | Document ID | Access other opp's document | 403 | P1 |
| SEC-025 | Sequential ID enumeration | Try ID+1, ID+2 | 403 for unauthorized | P1 |
| SEC-026 | Negative ID | ID=-1 | 400 | P2 |
| SEC-027 | Very large ID | ID=9999999999 | 404 | P2 |
| SEC-028 | User ID in audit trail | Access other's audit | 403 | P1 |
| SEC-029 | Interaction ID | Change to other's interaction | 403 | P1 |
| SEC-030 | OM user ID | Forge OM assignment | Server validates | P1 |

### 6.4 Mass Assignment (5)

| ID | Protected Field | Expected | Priority |
|----|----------------|----------|----------|
| SEC-031 | Stage field | Ignored | P0 |
| SEC-032 | Status field | Ignored | P0 |
| SEC-033 | CreatedBy/ModifiedBy | Overwritten by server | P1 |
| SEC-034 | IsDeleted flag | Not modifiable | P1 |
| SEC-035 | Id field | Auto-generated, ignored | P1 |

### 6.5 Authentication & Session (10)

| ID | Attack | Expected | Priority |
|----|--------|----------|----------|
| SEC-036 | Replay captured POST | Anti-replay protection | P0 |
| SEC-037 | CSRF on creation endpoint | Token required | P0 |
| SEC-038 | JWT tampering | Rejected | P0 |
| SEC-039 | Session fixation | New session after auth | P1 |
| SEC-040 | Brute force creation | Rate limited | P1 |
| SEC-041 | Token refresh during form fill | Seamless refresh | P1 |
| SEC-042 | Access after logout | Redirect to login | P1 |
| SEC-043 | Cookie HttpOnly flag | Set | P1 |
| SEC-044 | Cookie Secure flag | Set | P1 |
| SEC-045 | HTTP access (not HTTPS) | Redirect or reject | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Expected | Priority |
|----|------|----------|----------|
| SEC-046 | Error response | No stack trace | P0 |
| SEC-047 | API response includes deleted opps | Excluded | P1 |
| SEC-048 | API response exposes internal IDs | Display-safe IDs | P2 |
| SEC-049 | Logs contain passwords/tokens | No sensitive data | P0 |
| SEC-050 | Document download without auth | 401 | P0 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

| ID | Scenario | Expected Behavior | Priority |
|----|----------|-------------------|----------|
| CON-001 | Two users create opp with same name simultaneously | Both succeed (names not unique) or conflict handled | P1 |
| CON-002 | Two users edit same opp simultaneously | Optimistic concurrency — last save wins or conflict | P0 |
| CON-003 | Double-click Save button | Only one save executes | P0 |
| CON-004 | Double-click "Create Opportunity" | Only one form opens or one save | P0 |
| CON-005 | User A edits while User B deletes | Edit fails: "Opportunity deleted" | P1 |
| CON-006 | Rapid sequential creation (10 opps in 10 seconds) | All 10 created with unique IDs | P1 |
| CON-007 | Upload document while another user edits | Both operations succeed independently | P1 |
| CON-008 | Change OM while OM is editing | Notification to old OM, session behavior | P1 |
| CON-009 | Parallel API POST calls (same user) | Both succeed or idempotency | P1 |
| CON-010 | Database transaction isolation | Create reads own writes | P1 |
| CON-011 | Cache invalidation after creation | Other users see new opp in list | P1 |
| CON-012 | Session timeout during form fill | Graceful redirect to login | P1 |
| CON-013 | Concurrent partner search + creation | Both operations independent | P2 |
| CON-014 | Load balancer routes to different server | Data consistent | P2 |
| CON-015 | Concurrent document upload for same opp | Both uploaded successfully | P1 |
| CON-016 | Parallel creation from same partner by 2 users | Both opps created independently | P1 |
| CON-017 | AI service concurrent requests | Both responses correct | P2 |
| CON-018 | Concurrent list view during mass creation | List shows consistent state | P2 |
| CON-019 | Form submission after network reconnect | Submission succeeds or clear error | P1 |
| CON-020 | Concurrent audit trail writes | All entries preserved, none lost | P1 |
| CON-021 | Parallel org unit lookup | Both return correct result | P2 |
| CON-022 | Concurrent creation + filter/search | Search returns consistent results | P2 |
| CON-023 | Stress: 100 parallel creations | All succeed, DB consistent | P2 |
| CON-024 | Concurrent partner deletion + opp creation from partner | Handle gracefully | P1 |
| CON-025 | Form auto-save + manual save race | One version saved, no corruption | P2 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

### Validation (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-001 | ValidateName — valid | "Test Opportunity" | Passes | P1 |
| UNT-002 | ValidateName — empty | "" | Error: "Name required" | P1 |
| UNT-003 | ValidateOrgUnit — exists | Valid ID | Passes | P1 |
| UNT-004 | ValidateOrgUnit — not found | Invalid ID | Error: "Not found" | P1 |
| UNT-005 | ValidateDateRange — valid | Start < End | Passes | P1 |

### Formatting (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-006 | FormatOpportunityTitle | Name + ID | "[Name] (OPP-[ID])" | P2 |
| UNT-007 | FormatBudgetDisplay | 1234567.89, USD | "$1,234,567.89" | P1 |
| UNT-008 | FormatDateDisplay | DateTime | "dd MMM yyyy" format | P2 |

### Calculations (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-009 | TotalBudget — single partner | 1 × $100K | $100,000 | P1 |
| UNT-010 | TotalBudget — multiple | 3 partners, various | Sum | P1 |
| UNT-011 | TotalBudget — zero | No partners | $0 | P1 |
| UNT-012 | ImplementationDuration | Start + End | Months between | P2 |
| UNT-013 | DocumentCount | 5 docs | 5 | P2 |

### Status Logic (5)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-014 | DefaultStage | New opportunity | I&P | P0 |
| UNT-015 | DefaultStatus | New opportunity | Draft | P0 |
| UNT-016 | IsEditable — Draft | Status=Draft | true | P1 |
| UNT-017 | IsEditable — Closed | Status=Closed | false | P1 |
| UNT-018 | CanDelete — Draft | Status=Draft | true | P1 |

### Collections (3)

| ID | Test | Input | Expected | Priority |
|----|------|-------|----------|----------|
| UNT-019 | FilterActivePartners | 5 total, 2 deleted | 3 returned | P1 |
| UNT-020 | SortByCreationDate | Unsorted list | Sorted chronologically | P2 |
| UNT-021 | GroupByCountry | 10 opps, 3 countries | 3 groups | P2 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

| ID | Operation | Threshold | Priority |
|----|----------|-----------|----------|
| PRF-001 | Create opportunity API response | < 2 seconds | P1 |
| PRF-002 | Update opportunity API response | < 2 seconds | P1 |
| PRF-003 | Document upload (10MB) | < 5 seconds | P1 |
| PRF-004 | Batch creation (10 opps) | < 20 seconds | P2 |
| PRF-005 | AI-assisted creation (with AI call) | < 10 seconds | P2 |
| PRF-006 | Opportunity list load (10K opps) | < 3 seconds | P1 |
| PRF-007 | Search by name (10K opps) | < 2 seconds | P1 |
| PRF-008 | Partner typeahead (5K partners) | < 1 second | P1 |
| PRF-009 | Org unit dropdown load | < 1 second | P1 |
| PRF-010 | Combined filter (3 criteria, 10K opps) | < 3 seconds | P2 |
| PRF-011 | 50 concurrent creations | All < 5 seconds | P2 |
| PRF-012 | 20 concurrent list views | Avg < 3 seconds | P2 |
| PRF-013 | 100 concurrent searches | All < 3 seconds | P2 |
| PRF-014 | Memory during creation (profile) | < 30MB increase | P2 |
| PRF-015 | Memory during list load (10K) | < 50MB | P2 |
| PRF-016 | Memory during document upload | No leak after upload | P2 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

| ID | Load Profile | Duration | Success Criteria | Priority |
|----|-------------|----------|-----------------|----------|
| LDT-001 | 100 users creating opps simultaneously | 30 min | Error rate < 1% | P2 |
| LDT-002 | 200 concurrent list/search operations | 1 hour | P99 < 5 seconds | P2 |
| LDT-003 | 50 users uploading documents | 30 min | All uploads succeed | P2 |
| LDT-004 | Spike: 500 creations in 1 minute | 1 min | All processed | P2 |
| LDT-005 | 1000 concurrent reads during bulk creation | 5 min | System responsive | P2 |
| LDT-006 | Increase load until 5% error rate | Until failure | Identify breaking point | P2 |
| LDT-007 | Sustained 100 ops/min for 2 hours | 2 hours | No degradation | P2 |
| LDT-008 | Fill storage to 90% then create | Until full | Graceful error | P2 |
| LDT-009 | Remove load after spike | 5 min recovery | Normal within 2 min | P2 |
| LDT-010 | Restart service during active creations | Recovery | No data loss | P2 |

---

## Traceability Matrix

| Requirement | Test Cases |
|------------|------------|
| **PNO-687:** Create from Partners | POS-001 to POS-012, INT-002, INT-034, BND-061-062 |
| **PNO-688:** Create from Interactions | POS-013 to POS-024, INT-003, INT-033, BND-063-064 |
| **PNO-689:** Create from Opportunity Page | POS-025 to POS-030, INT-001 |
| Partner pre-population | POS-001, POS-002, POS-003, NEG-006 |
| AI-assisted creation | POS-005, POS-014, POS-021, NEG-052, BND-070, PRF-005 |
| Mandatory field validation | NEG-001 to NEG-015, FUN-016 to FUN-030 |
| Draft default state | FUN-001, UNT-014, UNT-015 |
| Audit trail | FUN-041 to FUN-050 |

---

**Last Updated:** 2026-02-18  
**Supersedes:** Previous version (19 tests, 3 categories)  
**Status:** Ready for Execution  
**Compliance:** ✅ 10-Category Standard | N≥3P ✅ | E≥3P ✅ | F≥3P ✅ | I≥3P ✅ | Total: 462
