# Opportunity Sections (WHY/WHERE/WHAT/WHO) — Comprehensive Test Cases

**Feature:** Opportunity data entry sections — WHY (strategic rationale), WHERE (geographic scope), WHAT (products/services/budget), WHO (stakeholders/team)  
**Created:** 2026-01-22  
**Restructured:** 2026-02-11 (10-category standard)  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 35 | 30-50 | ✅ |
| 2 | Negative Tests | §2 | 70 | Max(50, 2×35=70) | ✅ |
| 3 | Boundary Tests | §3 | 70 | Max(50, 2×35=70) | ✅ |
| 4 | Functional Tests | §4 | 50 | ≥50 | ✅ |
| 5 | Integration Tests | §5 | 50 | ≥50 | ✅ |
| 6 | Security Tests | §6 | 50 | ≥50 | ✅ |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **397** | **≥347** | ✅ |

**3:1 Ratio Check:** (70 + 70) = **140** ≥ 3 × 35 = **105** → ✅ PASS

---

## Sections Overview

| Section | Content | Key Fields |
|---------|---------|-----------|
| **WHY** | Strategic rationale | Context & Challenges, Strategic Missions, Expected Impact, Expected Outcomes, SDG Alignment |
| **WHERE** | Geographic scope | Countries of Implementation, Responsible Org Unit, UNCF alignment, Timeline |
| **WHAT** | Deliverables & budget | Products/Services, Budget, Proposed Initiative Type, Beneficiaries |
| **WHO** | Stakeholders | Opportunity Manager, Collaborators, Funding Partners, Client Partners, Stakeholders |

---

## §1 Positive Tests (Happy Path)

> **Count: 35** | **Minimum: 30-50** | ✅ COMPLIANT

### WHY Section (10)

| ID | Test Name | Steps | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| POS-001 | Enter Context & Challenges | Fill text area | Saved correctly | P0 |
| POS-002 | Select Strategic Mission(s) | Choose 1+ missions | Selections saved | P0 |
| POS-003 | Enter Expected Impact | Fill text area | Saved | P0 |
| POS-004 | Enter Expected Outcomes | Fill text area | Saved | P0 |
| POS-005 | Select SDG Alignment (single) | Choose 1 SDG | SDG linked | P0 |
| POS-006 | Select SDG Alignment (multiple) | Choose 3 SDGs | All linked | P1 |
| POS-007 | AI assists WHY section | Click AI Assist | Suggestions populated | P1 |
| POS-008 | Save WHY section partially | Fill some fields | Partial save works (Draft) | P1 |
| POS-009 | Edit existing WHY data | Modify text | Updated correctly | P1 |
| POS-010 | View WHY in read-only mode | After submission | Fields not editable | P1 |

### WHERE Section (9)

| ID | Test Name | Steps | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| POS-011 | Select country of implementation | Choose country | Country linked | P0 |
| POS-012 | Select multiple countries | Choose 3 countries | All linked | P0 |
| POS-013 | Select responsible org unit | Choose org unit | Org unit linked | P0 |
| POS-014 | Select UNCF alignment | Choose UNCF | UNCF saved | P1 |
| POS-015 | Enter timeline (start/end) | Set dates | Dates saved | P0 |
| POS-016 | Target signing date set | Enter date | Date saved | P1 |
| POS-017 | Country auto-suggests org unit | Select country → org unit suggested | Correct org unit | P1 |
| POS-018 | View WHERE in read-only | After submission | Not editable | P1 |
| POS-019 | Save WHERE partially | Some fields filled | Partial save works | P1 |

### WHAT Section (9)

| ID | Test Name | Steps | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| POS-020 | Select product/service | Choose from catalog | Product linked | P0 |
| POS-021 | Enter budget amount + currency | Set $100,000 USD | Budget saved | P0 |
| POS-022 | Select initiative type | Choose type | Type saved | P0 |
| POS-023 | Enter estimated beneficiaries | Set number | Saved | P1 |
| POS-024 | Beneficiary breakdown by gender | M/F/Other counts | Breakdown saved | P1 |
| POS-025 | Beneficiary breakdown calculation | Sum of parts | Matches total | P1 |
| POS-026 | Multiple products selected | 3+ products | All linked | P1 |
| POS-027 | View WHAT in read-only | After submission | Not editable | P1 |
| POS-028 | Budget with different currencies | EUR, GBP, CHF | Currency-specific | P1 |

### WHO Section (7)

| ID | Test Name | Steps | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| POS-029 | Assign OM | Select OM from users | OM linked | P0 |
| POS-030 | Add funding partner | Search + select partner | Partner linked with amount | P0 |
| POS-031 | Add client partner | Search + select partner | Client partner linked | P0 |
| POS-032 | Add stakeholder | Add stakeholder user | Stakeholder linked | P1 |
| POS-033 | Add collaborator | Assign collaborator role | Collaborator linked | P1 |
| POS-034 | Multiple funding partners | Add 3 partners | All saved with amounts | P1 |
| POS-035 | View WHO in read-only | After submission | Not editable | P1 |

---

## §2 Negative Tests

> **Count: 70** | **Minimum: Max(50, 2×35=70)** | ✅ COMPLIANT

### 2.1 Missing Required Fields (16)

| ID | Section | Missing Field | Expected Error | Priority |
|----|---------|--------------|---------------|----------|
| NEG-001 | WHY | Context & Challenges | "Context is required" | P0 |
| NEG-002 | WHY | Strategic Mission(s) | "At least one mission required" | P0 |
| NEG-003 | WHY | Expected Impact | "Impact is required" | P0 |
| NEG-004 | WHY | Expected Outcomes | "Outcomes is required" | P0 |
| NEG-005 | WHY | SDG Alignment (0 selected) | "At least one SDG required" | P0 |
| NEG-006 | WHERE | Country (0 selected) | "At least one country required" | P0 |
| NEG-007 | WHERE | Org Unit | "Org unit is required" | P0 |
| NEG-008 | WHERE | Implementation Start | "Start date required" | P1 |
| NEG-009 | WHERE | Implementation End | "End date required" | P1 |
| NEG-010 | WHAT | Products (0 selected) | "At least one product required" | P0 |
| NEG-011 | WHAT | Initiative Type | "Initiative type required" | P1 |
| NEG-012 | WHO | OM not assigned | "OM is required" | P0 |
| NEG-013 | WHO | Funding Partner (0) | "At least one funding partner required" | P0 |
| NEG-014 | WHO | Client Partner (0) | "At least one client partner required" | P0 |
| NEG-015 | ALL | All sections empty | All errors listed | P0 |
| NEG-016 | ALL | 5+ fields missing across sections | All shown simultaneously | P0 |

### 2.2 Invalid Data (14)

| ID | Field | Invalid Input | Expected Error | Priority |
|----|-------|--------------|---------------|----------|
| NEG-017 | Budget | Negative (-1000) | "Must be positive" | P0 |
| NEG-018 | Budget | Non-numeric ("abc") | "Invalid number" | P1 |
| NEG-019 | Implementation End | Before Start | "End must be after start" | P0 |
| NEG-020 | Beneficiaries | Negative (-5) | "Must be positive" | P1 |
| NEG-021 | Beneficiaries | Decimal (5.5) | "Must be integer" | P1 |
| NEG-022 | Beneficiary breakdown | Sum ≠ total | "Breakdown must match total" | P1 |
| NEG-023 | Budget | 0 with funding partner | Warning or accept | P1 |
| NEG-024 | Signing date | Invalid format | "Invalid date" | P1 |
| NEG-025 | Context | Only whitespace | "Cannot be blank" | P1 |
| NEG-026 | Impact | Only HTML tags | Escaped/stripped | P1 |
| NEG-027 | Outcomes | Script injection attempt | Escaped | P1 |
| NEG-028 | Budget with wrong currency code | "XYZ" | "Invalid currency" | P2 |
| NEG-029 | Budget = NaN | NaN | "Invalid number" | P2 |
| NEG-030 | Partner with inactive status | Inactive partner | Warning | P1 |

### 2.3 Unauthorized Access per Section (10)

| ID | Role | Section | Action | Expected | Priority |
|----|------|---------|--------|----------|----------|
| NEG-031 | Unauthenticated | WHY | Edit | 401 | P0 |
| NEG-032 | No edit permission | WHERE | Edit | 403 | P0 |
| NEG-033 | Collaborator (not implemented) | WHAT | Edit | Behavior TBD | P1 |
| NEG-034 | Partner User | WHO | View/Edit | Limited access | P1 |
| NEG-035 | Read-only mode (in workflow) | WHY | Edit | All fields disabled | P0 |
| NEG-036 | Read-only mode | WHERE | Edit | All fields disabled | P0 |
| NEG-037 | Read-only mode | WHAT | Edit | All fields disabled | P0 |
| NEG-038 | Read-only mode | WHO | Edit | All fields disabled | P0 |
| NEG-039 | After approval | Any | Edit | Permanently read-only | P0 |
| NEG-040 | Different org unit user | WHO | Change OM | 403 | P1 |

### 2.4 Invalid References (10)

| ID | Field | Invalid Reference | Expected | Priority |
|----|-------|--------------------|----------|----------|
| NEG-041 | Country | Deleted country | Not in dropdown | P1 |
| NEG-042 | Org Unit | Deleted org unit | Not in dropdown | P1 |
| NEG-043 | Product | Deprecated product | Warning or hidden | P1 |
| NEG-044 | Funding Partner | Soft-deleted partner | Not in search | P1 |
| NEG-045 | Client Partner | Soft-deleted partner | Not in search | P1 |
| NEG-046 | OM | Deactivated user | Not in dropdown | P1 |
| NEG-047 | SDG | Invalid SDG number | Rejected | P2 |
| NEG-048 | Currency | Unsupported currency | Rejected | P2 |
| NEG-049 | UNCF | Expired UNCF | Warning | P2 |
| NEG-050 | Strategic Mission | Deleted mission | Not in dropdown | P1 |

### 2.5 Dependency & Save Failures (10)

| ID | Failure | Expected | Priority |
|----|---------|----------|----------|
| NEG-051 | DB timeout during save | Error, no partial save | P1 |
| NEG-052 | Partner search timeout | Typeahead error | P1 |
| NEG-053 | Country reference data unavailable | Dropdown error | P1 |
| NEG-054 | Product catalog unavailable | Dropdown error | P1 |
| NEG-055 | SDG data unavailable | Dropdown error | P2 |
| NEG-056 | AI service timeout | Manual entry still works | P1 |
| NEG-057 | Network disconnect during save | "Connection lost" | P1 |
| NEG-058 | Auth token expired during editing | Redirect on save | P1 |
| NEG-059 | File upload service down | Document section shows error | P2 |
| NEG-060 | Currency exchange service down | Budget saved in original currency | P2 |

### 2.6 Form Interaction Failures (10)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-061 | Navigate away with unsaved changes | Confirmation dialog | P0 |
| NEG-062 | Double-click Save in any section | Single save | P0 |
| NEG-063 | Browser refresh during edit | Data lost (if no auto-save) | P1 |
| NEG-064 | Clear required field after filling | Inline validation error | P1 |
| NEG-065 | Paste very large text in Context | Truncated or error | P1 |
| NEG-066 | Select then deselect all SDGs | "At least one required" | P1 |
| NEG-067 | Remove all funding partners | "At least one required" | P1 |
| NEG-068 | Remove all countries | "At least one required" | P1 |
| NEG-069 | Tab navigation order correct | Fields focused in logical order | P2 |
| NEG-070 | Screen reader reads all labels | All fields accessible | P2 |

---

## §3 Boundary Tests

> **Count: 70** | **Minimum: Max(50, 2×35=70)** | ✅ COMPLIANT

### 3.1 Text Field Boundaries (20)

| ID | Field | Min | Max | At Min | At Max | Over | Priority |
|----|-------|-----|-----|--------|--------|------|----------|
| BND-001 | Context & Challenges | 1 | 10000 | ✅ | ✅ | ❌ | P1 |
| BND-002 | Expected Impact | 1 | 5000 | ✅ | ✅ | ❌ | P1 |
| BND-003 | Expected Outcomes | 1 | 5000 | ✅ | ✅ | ❌ | P1 |
| BND-004 | Budget Notes | 0 | 2000 | ✅ | ✅ | ❌ | P2 |
| BND-005 | Context = 1 char | — | — | ✅ | — | — | P2 |
| BND-006 | Context = 10000 chars | — | — | — | ✅ | — | P2 |
| BND-007 | Context = 10001 chars | — | — | — | — | ❌ | P2 |
| BND-008 | Impact = 1 char | — | — | ✅ | — | — | P2 |
| BND-009 | Impact = 5000 chars | — | — | — | ✅ | — | P2 |
| BND-010 | Outcomes = exactly max | — | — | — | ✅ | — | P2 |
| BND-011 | All text fields at max simultaneously | All at max | — | ✅ | ✅ | — | P1 |
| BND-012 | Text with only newlines | Newlines | — | ✅ | — | — | P2 |
| BND-013 | Text with tabs | Tab chars | — | ✅ | — | — | P2 |
| BND-014 | Text with leading/trailing spaces | Whitespace | — | Trimmed | — | — | P1 |
| BND-015 | Text with consecutive spaces | Multiple spaces | — | Preserved/normalized | — | — | P2 |
| BND-016 | AI-generated text at field max | AI output | — | Truncated if needed | — | — | P1 |
| BND-017 | Copy/paste large text from Word | Formatted text | — | Formatting handled | — | — | P1 |
| BND-018 | Text with URL links | URLs in text | — | URLs preserved | — | — | P2 |
| BND-019 | Text with bullet point characters | •, -, * | — | ✅ Preserved | — | — | P2 |
| BND-020 | Text with mathematical symbols | ±, ÷, ×, √ | — | ✅ Stored | — | — | P2 |

### 3.2 Numeric Boundaries (10)

| ID | Field | Test | Expected | Priority |
|----|-------|------|----------|----------|
| BND-021 | Budget = 0 | Zero | ✅ Accept | P1 |
| BND-022 | Budget = 0.01 | Min positive | ✅ | P1 |
| BND-023 | Budget = 999,999,999.99 | Max | ✅ | P1 |
| BND-024 | Budget = 1,000,000,000 | Over max | ❌ | P2 |
| BND-025 | Beneficiaries = 0 | Zero | ✅ | P1 |
| BND-026 | Beneficiaries = 1 | Min positive | ✅ | P2 |
| BND-027 | Beneficiaries = MAX_INT | Max | Handle gracefully | P2 |
| BND-028 | Budget with 3+ decimals | 100.123 | Rounded or rejected | P2 |
| BND-029 | Funding amount sum overflow | Very large sums | Handle gracefully | P2 |
| BND-030 | Beneficiary breakdown: all same gender | 100% M or F | ✅ | P2 |

### 3.3 Collection Boundaries (15)

| ID | Collection | State | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-031 | SDGs | 1 (minimum) | ✅ | P1 |
| BND-032 | SDGs | All 17 | ✅ | P1 |
| BND-033 | Countries | 1 | ✅ | P1 |
| BND-034 | Countries | 50+ | ✅ or limit | P2 |
| BND-035 | Countries | All (~200) | ✅ or limit | P2 |
| BND-036 | Products | 1 | ✅ | P1 |
| BND-037 | Products | 100+ | ✅ or limit | P2 |
| BND-038 | Funding Partners | 1 | ✅ | P1 |
| BND-039 | Funding Partners | 20+ | ✅ | P1 |
| BND-040 | Client Partners | 1 | ✅ | P1 |
| BND-041 | Client Partners | 50+ | ✅ or limit | P2 |
| BND-042 | Stakeholders | 0 | ✅ (optional) | P1 |
| BND-043 | Stakeholders | 100+ | ✅ or limit | P2 |
| BND-044 | Strategic Missions | 1 | ✅ | P1 |
| BND-045 | Strategic Missions | All available | ✅ | P2 |

### 3.4 Unicode & Special Characters (10)

| ID | Field | Input | Expected | Priority |
|----|-------|-------|----------|----------|
| BND-046 | Context | Arabic: "السياق والتحديات" | ✅ Stored | P1 |
| BND-047 | Impact | Chinese: "预期影响" | ✅ Stored | P1 |
| BND-048 | Outcomes | French: "Résultats attendus" | ✅ | P1 |
| BND-049 | Context | Emoji: "🌍 Global context" | ✅ or ❌ | P2 |
| BND-050 | Impact | HTML entities: `&amp;` | Escaped | P1 |
| BND-051 | Outcomes | Mixed RTL/LTR | Rendered correctly | P2 |
| BND-052 | Budget notes | Diacritics: "Orçamento" | ✅ | P1 |
| BND-053 | Budget | EU locale: "1.234,56" | Parsed | P1 |
| BND-054 | Partner name | O'Brien & Sons "Ltd" | ✅ Escaped | P1 |
| BND-055 | Context | SQL chars: '; -- | Parameterized | P1 |

### 3.5 Date Boundaries (8)

| ID | Date Test | Expected | Priority |
|----|----------|----------|----------|
| BND-056 | Start = today | ✅ | P1 |
| BND-057 | Start = yesterday | ⚠️ Warning | P1 |
| BND-058 | End = start (0 duration) | ✅ | P2 |
| BND-059 | Span = 10 years | ✅ | P2 |
| BND-060 | Feb 29 leap year | ✅ | P2 |
| BND-061 | Year boundary | Correct year | P2 |
| BND-062 | All dates same | ✅ | P2 |
| BND-063 | No dates (all optional) | ✅ if optional | P1 |

### 3.6 Section Navigation Boundaries (7)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-064 | Fill WHY → skip to WHAT (skip WHERE) | Allowed for Draft | P1 |
| BND-065 | Fill all sections in reverse order | ✅ No required order | P1 |
| BND-066 | Navigate between sections rapidly | No data loss | P1 |
| BND-067 | Open all sections simultaneously (accordion) | All render correctly | P2 |
| BND-068 | Collapse section with unsaved data | Data preserved or warning | P1 |
| BND-069 | Section visible on smallest viewport | Responsive layout | P2 |
| BND-070 | Section with screen reader | All labels announced | P2 |

---

## §4 Functional Tests

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 4.1 Workflow Rules (15)

| ID | Rule | Scenario | Expected | Priority |
|----|------|----------|----------|----------|
| FUN-001 | Sections editable in Draft | I&P/Draft | All sections editable | P0 |
| FUN-002 | Sections read-only in workflow | GO/Active | All disabled | P0 |
| FUN-003 | Sections read-only after approval | GO/Closed | Permanently disabled | P0 |
| FUN-004 | Sections editable after reopen | Reopen from NO GO | Editable again | P0 |
| FUN-005 | No required section completion order | Draft | Any order works | P1 |
| FUN-006 | Auto-save within sections | Edit + wait | Changes saved | P1 |
| FUN-007 | Section completeness indicator | All fields filled | Section shows ✅ | P1 |
| FUN-008 | Section incomplete indicator | Missing fields | Section shows ⚠️ | P1 |
| FUN-009 | Section validation on submit | Submit with gaps | All gaps listed | P0 |
| FUN-010 | AI assistance available per section | Each section | AI button present | P1 |
| FUN-011 | Section data preserved across edits | Edit + navigate + return | Data intact | P0 |
| FUN-012 | Partial save allowed | Only WHY filled | Save as Draft | P0 |
| FUN-013 | Full validation only on submit | Draft with gaps | Save works, submit blocked | P0 |
| FUN-014 | Section tabs indicate progress | Some complete | Visual progress shown | P1 |
| FUN-015 | Print/export includes all sections | Export | All data present | P2 |

### 4.2 Validation Rules (15)

| ID | Rule | Valid | Invalid | Priority |
|----|------|-------|---------|----------|
| FUN-016 | All WHY fields required for submit | All filled | Context missing | P0 |
| FUN-017 | All WHERE fields required for submit | All filled | Country missing | P0 |
| FUN-018 | All WHAT fields required for submit | All filled | Product missing | P0 |
| FUN-019 | OM required (WHO) | OM assigned | No OM | P0 |
| FUN-020 | At least 1 SDG | 1+ selected | 0 selected | P0 |
| FUN-021 | At least 1 country | 1+ selected | 0 selected | P0 |
| FUN-022 | At least 1 product | 1+ selected | 0 selected | P0 |
| FUN-023 | At least 1 funding partner | 1+ selected | 0 selected | P0 |
| FUN-024 | Budget ≥ 0 | 0 or positive | Negative | P0 |
| FUN-025 | End date ≥ start date | End after start | End before start | P0 |
| FUN-026 | Beneficiary breakdown = total | Sum matches | Mismatch | P1 |
| FUN-027 | Server re-validates all sections | Client passes, server checks | Server catches errors | P0 |
| FUN-028 | Cross-section validation (country → org unit) | Matching | Mismatch warning | P1 |
| FUN-029 | Funding amount ≥ 0 per partner | Positive | Negative | P1 |
| FUN-030 | Currency required with amount | Amount + currency | Amount only | P1 |

### 4.3 Constraint Rules (10)

| ID | Constraint | Expected | Priority |
|----|-----------|----------|----------|
| FUN-031 | Deleted partners not in dropdowns | Excluded | P0 |
| FUN-032 | Inactive users not in OM dropdown | Excluded | P0 |
| FUN-033 | Deleted countries not selectable | Excluded | P1 |
| FUN-034 | Deprecated products filtered | Hidden or marked | P1 |
| FUN-035 | Budget precision: 2 decimal places | Enforced | P1 |
| FUN-036 | Document upload limits per section | Enforced | P2 |
| FUN-037 | Max stakeholders (if limited) | Enforced | P2 |
| FUN-038 | Unique partners (no duplicates) | Duplicate blocked | P1 |
| FUN-039 | Unique countries (no duplicates) | Duplicate blocked | P1 |
| FUN-040 | Unique SDGs (no duplicates) | Duplicate blocked | P1 |

### 4.4 Audit Rules (10)

| ID | Action | Expected Audit | Priority |
|----|--------|---------------|----------|
| FUN-041 | Edit WHY section | Fields changed + values | P0 |
| FUN-042 | Edit WHERE section | Fields changed + values | P0 |
| FUN-043 | Edit WHAT section | Fields changed + values | P0 |
| FUN-044 | Edit WHO section | Fields changed + values | P0 |
| FUN-045 | Add funding partner | Partner name + amount | P1 |
| FUN-046 | Remove funding partner | Partner name removed | P1 |
| FUN-047 | Change OM | Old + New OM logged | P1 |
| FUN-048 | Add/remove country | Country change logged | P1 |
| FUN-049 | AI-assisted edit | AI source noted | P2 |
| FUN-050 | Bulk section update | Each field change logged | P2 |

---

## §5 Integration Tests

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 5.1 CRUD (10)

| ID | Flow | Expected | Priority |
|----|------|----------|----------|
| INT-001 | Fill all 4 sections → Save | All data persisted | P0 |
| INT-002 | Fill sections → Submit for Go | Pre-submission validation passes | P0 |
| INT-003 | Edit section → Verify in detail view | Updated data shown | P0 |
| INT-004 | Add partner in WHO → Verify in partner detail | Opp linked to partner | P0 |
| INT-005 | Select country → Verify org unit suggestions | Org unit matches | P1 |
| INT-006 | Fill WHAT budget → Verify in opportunity summary | Amount displayed | P1 |
| INT-007 | Add SDGs → Verify in opportunity card | SDGs shown | P1 |
| INT-008 | Delete all sections data → Revert to empty | Clean state | P1 |
| INT-009 | Fill via AI → Verify AI data accepted | AI suggestions saved | P1 |
| INT-010 | Change OM in WHO → New OM can edit | Access transferred | P0 |

### 5.2 Search & Filter (10)

| ID | Filter | Expected | Priority |
|----|--------|----------|----------|
| INT-011 | Search by country | Opps in that country | P1 |
| INT-012 | Filter by SDG | Opps with that SDG | P1 |
| INT-013 | Filter by product | Opps with that product | P1 |
| INT-014 | Filter by OM | OM's opportunities | P1 |
| INT-015 | Filter by partner | Linked opps | P1 |
| INT-016 | Filter by initiative type | Matching opps | P1 |
| INT-017 | Search by context keyword | Full-text match | P1 |
| INT-018 | Combined filter (SDG + country) | Intersection | P1 |
| INT-019 | Sort by budget amount | Ordered by amount | P2 |
| INT-020 | Search with no results | "No results" | P1 |

### 5.3 Pagination (5)

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| INT-021 | Partners list in WHO (100+ partners) | Paginated/scrollable | P2 |
| INT-022 | SDG dropdown (17 items) | All visible | P2 |
| INT-023 | Country dropdown (200 items) | Filterable + scrollable | P2 |
| INT-024 | Products catalog (500 items) | Filterable + paginated | P2 |
| INT-025 | Stakeholder list (100+) | Scrollable | P2 |

### 5.4 Relationships (10)

| ID | Relationship | Test | Expected | Priority |
|----|-------------|------|----------|----------|
| INT-026 | Opp → SDGs (N:M) | Select 5 SDGs | All linked | P0 |
| INT-027 | Opp → Countries (N:M) | Select 3 countries | All linked | P0 |
| INT-028 | Opp → Products (N:M) | Select 4 products | All linked | P0 |
| INT-029 | Opp → Funding Partners (N:M) | 2 partners + amounts | All saved | P0 |
| INT-030 | Opp → Client Partners (N:M) | 1 client | Linked | P0 |
| INT-031 | Opp → OM (N:1) | Assign OM | Linked | P0 |
| INT-032 | Opp → Org Unit (N:1) | Select org unit | Linked | P0 |
| INT-033 | Opp → Strategic Missions (N:M) | 2 missions | Linked | P1 |
| INT-034 | Partner → Opp (reverse) | View partner | Opp in partner's list | P1 |
| INT-035 | Cascade: delete partner → opp section | Soft-delete partner | Partner shows deleted in WHO | P1 |

### 5.5 Error Handling (15)

| ID | Error | Expected | Priority |
|----|-------|----------|----------|
| INT-036 | Save with missing required field | 400 + field errors | P0 |
| INT-037 | Save with invalid budget | 400 | P0 |
| INT-038 | Save with non-existent partner ID | 400 | P1 |
| INT-039 | Save without auth | 401 | P0 |
| INT-040 | Save with expired token | 401 | P1 |
| INT-041 | Malformed JSON | 400 | P1 |
| INT-042 | Save during read-only (in workflow) | 403 | P1 |
| INT-043 | Save with DB timeout | 503 with retry | P1 |
| INT-044 | Partial section save failure | Atomic — all or nothing | P1 |
| INT-045 | Reference data unavailable | Graceful error per dropdown | P1 |
| INT-046 | Upload document > size limit | 413 | P1 |
| INT-047 | AI service error during assist | Manual entry still works | P1 |
| INT-048 | Concurrent edit conflict | 409 | P1 |
| INT-049 | Rate limited section saves | 429 | P2 |
| INT-050 | Section save with extra fields | Ignored | P2 |

---

## §6 Security Tests

> **Count: 50** | **Minimum: ≥50** | ✅ COMPLIANT

### 6.1 Injection (10)

| ID | Vector | Target | Expected | Priority |
|----|--------|--------|----------|----------|
| SEC-001 | SQL | Context field | Parameterized | P0 |
| SEC-002 | SQL | Impact field | Escaped | P0 |
| SEC-003 | XSS | Outcomes field | HTML escaped | P0 |
| SEC-004 | XSS | Budget notes | Sanitized | P0 |
| SEC-005 | XSS | Partner search | Escaped | P0 |
| SEC-006 | Command | Context | Stored as text | P1 |
| SEC-007 | Path traversal | Document | Rejected | P1 |
| SEC-008 | JSON | API body | Blocked | P1 |
| SEC-009 | Template | Impact | Not evaluated | P2 |
| SEC-010 | Header | API | Blocked | P2 |

### 6.2 Access Control (10)

| ID | Role | Section | Expected | Priority |
|----|------|---------|----------|----------|
| SEC-011 | Unauthenticated | Any section | 401 | P0 |
| SEC-012 | No permission | Edit WHY | 403 | P0 |
| SEC-013 | Read-only (workflow) | Edit WHERE | 403 | P0 |
| SEC-014 | After approval | Edit WHAT | 403 | P0 |
| SEC-015 | Wrong org unit | Edit WHO | 403 | P1 |
| SEC-016 | Expired session | Save | 401 | P1 |
| SEC-017 | Revoked permission | Save | 403 | P1 |
| SEC-018 | Horizontal: other user's opp | Edit sections | 403 | P0 |
| SEC-019 | Vertical: Collaborator as OM | OM actions | 403 | P1 |
| SEC-020 | Admin bypass | Direct API | Per config | P2 |

### 6.3 IDOR (10)

| ID | Object | Expected | Priority |
|----|--------|----------|----------|
| SEC-021 | Opportunity ID | 403 | P0 |
| SEC-022 | Partner ID | 403 | P0 |
| SEC-023 | Document ID | 403 | P1 |
| SEC-024 | SDG mapping ID | 403 | P1 |
| SEC-025 | Country mapping ID | 403 | P1 |
| SEC-026 | Negative ID | 400 | P2 |
| SEC-027 | Large ID | 404 | P2 |
| SEC-028 | UUID format | 400 | P2 |
| SEC-029 | Sequential scan | Rate limited | P1 |
| SEC-030 | Predictable ID | Auth gated | P1 |

### 6.4 Mass Assignment (5)

| ID | Field | Expected | Priority |
|----|-------|----------|----------|
| SEC-031 | Stage | Ignored | P0 |
| SEC-032 | Status | Ignored | P0 |
| SEC-033 | CreatedBy | Server-controlled | P1 |
| SEC-034 | IsDeleted | Not modifiable | P1 |
| SEC-035 | Id | Auto-generated | P1 |

### 6.5 Auth & Session (10)

| ID | Attack | Expected | Priority |
|----|--------|----------|----------|
| SEC-036 | Replay | Anti-replay | P0 |
| SEC-037 | CSRF | Token required | P0 |
| SEC-038 | JWT tamper | Rejected | P0 |
| SEC-039 | Session fixation | New session | P1 |
| SEC-040 | Brute force | Rate limited | P1 |
| SEC-041 | Token refresh | Seamless | P1 |
| SEC-042 | After logout | Redirect | P1 |
| SEC-043 | HttpOnly cookie | Set | P1 |
| SEC-044 | Secure cookie | Set | P1 |
| SEC-045 | HTTPS only | Enforced | P0 |

### 6.6 Data Exposure (5)

| ID | Data | Expected | Priority |
|----|------|----------|----------|
| SEC-046 | Error response | No stack trace | P0 |
| SEC-047 | Deleted records | Excluded from queries | P1 |
| SEC-048 | Internal IDs | Display-safe | P2 |
| SEC-049 | Sensitive logs | No passwords/tokens | P0 |
| SEC-050 | Document download | Auth required | P0 |

---

## §7 Concurrency Tests

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | Two users edit same section | Optimistic concurrency | P0 |
| CON-002 | User A edits WHY, User B edits WHERE | Both succeed independently | P1 |
| CON-003 | Double-click Save | Single save | P0 |
| CON-004 | Edit while opp enters workflow | Edit blocked, read-only | P1 |
| CON-005 | Concurrent partner addition | Both added | P1 |
| CON-006 | Concurrent SDG selection | Both saved | P1 |
| CON-007 | Save during DB transaction lock | Retry or error | P1 |
| CON-008 | AI assist while manual edit | Both resolved | P1 |
| CON-009 | Concurrent country selection | Both saved | P2 |
| CON-010 | Cache invalidation after edit | Others see update | P1 |
| CON-011 | Parallel section saves (API) | All succeed | P1 |
| CON-012 | Edit + delete concurrent | One wins | P1 |
| CON-013 | Session timeout during save | Redirect, no partial | P1 |
| CON-014 | Multiple tabs editing same opp | Conflict on save | P1 |
| CON-015 | Bulk edit multiple sections | Atomic or section-level | P1 |
| CON-016 | Concurrent reference data update | Consistent view | P2 |
| CON-017 | Parallel document uploads | Both complete | P1 |
| CON-018 | Load balancer + section edits | Data consistent | P2 |
| CON-019 | Concurrent audit writes | All preserved | P1 |
| CON-020 | Rapid field changes (debounce) | Final value saved | P1 |
| CON-021 | Concurrent partner search | Independent results | P2 |
| CON-022 | Parallel validation checks | All return correct | P2 |
| CON-023 | Concurrent OM change + section edit | OM change respected | P1 |
| CON-024 | Stress: 50 concurrent section saves | All succeed or graceful error | P2 |
| CON-025 | Race: section save + workflow transition | Transition wins | P1 |

---

## §8 Unit Tests

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

| ID | Category | Test | Input | Expected | Priority |
|----|----------|------|-------|----------|----------|
| UNT-001 | Validation | ValidateWHYSection — complete | All fields | Passes | P1 |
| UNT-002 | Validation | ValidateWHYSection — missing context | Context=null | Error | P1 |
| UNT-003 | Validation | ValidateWHERE — no country | 0 countries | Error | P1 |
| UNT-004 | Validation | ValidateWHAT — no product | 0 products | Error | P1 |
| UNT-005 | Validation | ValidateWHO — no OM | OM=null | Error | P1 |
| UNT-006 | Formatting | FormatBudget | 1234567.89, USD | "$1,234,567.89" | P1 |
| UNT-007 | Formatting | FormatSDGDisplay | SDG 1 | "SDG 1: No Poverty" | P2 |
| UNT-008 | Formatting | FormatCountryList | 3 countries | Comma-separated | P2 |
| UNT-009 | Calc | TotalBudget | 3 partners | Sum of amounts | P1 |
| UNT-010 | Calc | BeneficiaryTotal | M+F+Other | Correct sum | P1 |
| UNT-011 | Calc | BeneficiaryBreakdown % | Breakdown | Percentages | P2 |
| UNT-012 | Calc | ImplementationDuration | Start→End | Months | P1 |
| UNT-013 | Calc | SectionCompleteness | Filled fields | Percentage | P1 |
| UNT-014 | Status | IsSectionEditable — Draft | Draft | true | P0 |
| UNT-015 | Status | IsSectionEditable — InWorkflow | Active | false | P0 |
| UNT-016 | Status | IsSectionEditable — Closed | Closed | false | P1 |
| UNT-017 | Status | GetRequiredFields — WHY | Section | Field list | P1 |
| UNT-018 | Status | GetRequiredFields — WHERE | Section | Field list | P1 |
| UNT-019 | Collection | FilterActivePartners | Mixed active/deleted | Active only | P1 |
| UNT-020 | Collection | SortSDGs | Unsorted | Numeric order | P2 |
| UNT-021 | Collection | GroupBySection | All fields | 4 groups | P2 |

---

## §9 Performance Tests

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

| ID | Operation | Threshold | Priority |
|----|----------|-----------|----------|
| PRF-001 | Save WHY section | < 2s | P1 |
| PRF-002 | Save WHERE section | < 2s | P1 |
| PRF-003 | Save WHAT section (with budget) | < 2s | P1 |
| PRF-004 | Save WHO section (with partners) | < 3s | P1 |
| PRF-005 | Load all sections (full opp) | < 3s | P1 |
| PRF-006 | Partner typeahead (5K partners) | < 1s | P1 |
| PRF-007 | Country dropdown (200 items) | < 500ms | P1 |
| PRF-008 | Product catalog (500 items) | < 1s | P1 |
| PRF-009 | SDG list (17 items) | < 200ms | P2 |
| PRF-010 | AI assist response | < 10s | P2 |
| PRF-011 | 50 concurrent section saves | All < 5s | P2 |
| PRF-012 | 100 concurrent reads | Avg < 3s | P2 |
| PRF-013 | Full opp with all fields at max | < 5s load | P2 |
| PRF-014 | Memory: 4 sections loaded | < 30MB | P2 |
| PRF-015 | Memory: partner search | < 10MB | P2 |
| PRF-016 | Memory: no leaks after section nav | Stable | P2 |

---

## §10 Load Tests

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

| ID | Profile | Duration | Criteria | Priority |
|----|---------|----------|----------|----------|
| LDT-001 | 100 users editing sections | 1 hour | Error < 1% | P2 |
| LDT-002 | 200 concurrent reads | 1 hour | P99 < 5s | P2 |
| LDT-003 | 50 users saving simultaneously | 30 min | All succeed | P2 |
| LDT-004 | 500 section saves in 1 min | 1 min | All processed | P2 |
| LDT-005 | 1000 concurrent list views | 5 min | Responsive | P2 |
| LDT-006 | Increase until 5% errors | Until failure | Find limits | P2 |
| LDT-007 | Partner search under load | 30 min | < 2s response | P2 |
| LDT-008 | Reference data requests | 1 hour | Stable | P2 |
| LDT-009 | Recovery after spike | 5 min | Normal in 2 min | P2 |
| LDT-010 | Service restart during edits | Recovery | No data loss | P2 |

---

## Traceability Matrix

| Feature Area | Test Cases |
|-------------|------------|
| **WHY Section** | POS-001 to POS-010, NEG-001 to NEG-005, BND-001 to BND-009, FUN-016 |
| **WHERE Section** | POS-011 to POS-019, NEG-006 to NEG-009, BND-056 to BND-063, FUN-017 |
| **WHAT Section** | POS-020 to POS-028, NEG-010 to NEG-011, BND-021 to BND-030, FUN-018 |
| **WHO Section** | POS-029 to POS-035, NEG-012 to NEG-014, FUN-019 |
| **Cross-Section Validation** | FUN-027, FUN-028, INT-001 to INT-002 |
| **AI Assistance** | POS-007, POS-009, INT-009, NEG-056 |
| **Read-Only Enforcement** | FUN-002, FUN-003, NEG-035 to NEG-039 |
| **Partner Dropdowns** | FUN-031, FUN-038, NEG-044, NEG-045, SEC-022 |

---

**Last Updated:** 2026-02-11  
**Supersedes:** Previous version (43 tests, feature sections only)  
**Status:** Ready for Execution  
**Compliance:** ✅ 10-Category Standard, ✅ 3:1 Ratio (140 ≥ 105)
