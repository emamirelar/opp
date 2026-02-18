# Opportunity Statement — Test Cases

**Component:** Opportunity Statement Auto-Generation & Management  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30 | ✅ |
| 2 | Negative Tests | §2 | 90 | 3×30=90 | ✅ |
| 3 | Boundary Tests | §3 | 90 | 3×30=90 | ✅ |
| 4 | Functional Tests | §4 | 90 | 3×30=90 | ✅ |
| 5 | Integration Tests | §5 | 90 | 3×30=90 | ✅ |
| 6 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 7 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 8 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 9 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

### Mandatory Ratio Compliance Checks

| Check | Formula | Required | Actual | Status |
|-------|---------|----------|--------|--------|
| N ≥ 3P | Negative ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ PASS |
| E ≥ 3P | Edge/Boundary ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ PASS |
| F ≥ 3P | Functional ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ PASS |
| I ≥ 3P | Integration ≥ 3×Positive | 90 ≥ 90 | 90 ≥ 90 | ✅ PASS |

---

## Feature Overview

Auto-generates opportunity statement documents from opportunity data (WHY, WHAT, Team, Budget, Schedule sections). Features: template-based generation, section compilation, PDF/Word export, version control, AI-enhanced text, manual editing, approval workflow, statement comparison, data freshness indicator, missing data warnings, and multi-language support.

---

## §1 Positive Tests (Happy Path)

> **Count: 30** | **Minimum: 30** | ✅ COMPLIANT

### Generation & Export (12 tests)

| ID | Test Name | Steps (Brief) | Expected Result | Priority |
|----|-----------|---------------|-----------------|----------|
| POS-001 | Generate statement from valid opportunity | Select opp → Generate | Statement created with all sections | P0 |
| POS-002 | Statement includes WHY section | Generate | WHY section populated from opp data | P0 |
| POS-003 | Statement includes WHAT section | Generate | WHAT section populated | P0 |
| POS-004 | Statement includes Team section | Generate | Team section populated | P0 |
| POS-005 | Statement includes Budget section | Generate | Budget section populated | P0 |
| POS-006 | Statement includes Schedule section | Generate | Schedule section populated | P0 |
| POS-007 | PDF export succeeds | Generate → Export PDF | PDF file downloadable | P0 |
| POS-008 | Word export succeeds | Generate → Export Word | Word file downloadable | P0 |
| POS-009 | Version control creates new version | Edit → Save | New version created | P0 |
| POS-010 | AI-enhanced text applied | Click "Enhance with AI" | Text improved, marked as AI-enhanced | P1 |
| POS-011 | Manual edit saves correctly | Edit section → Save | Changes persisted | P1 |
| POS-012 | Regenerate after data change | Change opp data → Regenerate | Statement reflects new data | P1 |

### Template & Format (10 tests)

| ID | Test Name | Steps (Brief) | Expected Result | Priority |
|----|-----------|---------------|-----------------|----------|
| POS-013 | Template selection applied | Select template → Generate | Correct template used | P1 |
| POS-014 | Section ordering respected | Custom order → Generate | Sections in specified order | P1 |
| POS-015 | Data freshness indicator shown | View statement | Indicator shows last sync time | P1 |
| POS-016 | Approval submit from statement | Submit for approval | Workflow triggered | P1 |
| POS-017 | Comparison view between versions | Select 2 versions → Compare | Diff view displayed | P1 |
| POS-018 | Historical versions list | View versions | All versions listed | P1 |
| POS-019 | Pagination of version list | Many versions | Pagination works | P2 |
| POS-020 | Search within statement | Search text | Results highlighted | P2 |
| POS-021 | Sort versions by date | Sort descending | Newest first | P2 |
| POS-022 | Filter by approval status | Filter approved | Only approved shown | P2 |

### Multi-Language & Advanced (8 tests)

| ID | Test Name | Steps (Brief) | Expected Result | Priority |
|----|-----------|---------------|-----------------|----------|
| POS-023 | Multi-language generation | Select language → Generate | Statement in selected language | P1 |
| POS-024 | Format options (font, margins) | Set options → Export | Options applied | P2 |
| POS-025 | Watermark on draft | Generate draft | Watermark visible | P2 |
| POS-026 | Header/footer customization | Set header/footer | Applied to export | P2 |
| POS-027 | Table of contents generated | Generate long statement | TOC included | P2 |
| POS-028 | Appendix section included | Add appendix → Generate | Appendix in output | P2 |
| POS-029 | Signature block placeholder | Generate | Signature block present | P2 |
| POS-030 | Cover page with opp title | Generate | Cover page correct | P2 |

---

## §2 Negative Tests (Failure Scenarios)

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### 2.1 Invalid Input (15 tests)

| ID | Test Name | Invalid Input | Expected Error | Priority |
|----|-----------|---------------|----------------|----------|
| NEG-001 | Null opportunity ID | oppId=null | 400 Bad Request | P0 |
| NEG-002 | Non-existent opportunity ID | oppId=999999 | 404 Not Found | P0 |
| NEG-003 | Deleted opportunity | oppId of soft-deleted | 404 Not Found | P0 |
| NEG-004 | Incomplete opportunity data | Missing WHY data | Warning + partial generation | P0 |
| NEG-005 | Missing WHY section data | No rationale | Missing data warning | P0 |
| NEG-006 | Missing WHAT section data | No scope | Missing data warning | P0 |
| NEG-007 | Missing Team section data | No team members | Missing data warning | P0 |
| NEG-008 | Missing Budget data | No budget | Missing data warning | P0 |
| NEG-009 | Missing Schedule data | No dates | Missing data warning | P0 |
| NEG-010 | Negative opportunity ID | oppId=-1 | 400 Bad Request | P1 |
| NEG-011 | Zero opportunity ID | oppId=0 | 400 Bad Request | P1 |
| NEG-012 | Non-numeric opportunity ID | oppId="abc" | 400 Bad Request | P1 |
| NEG-013 | Invalid template ID | templateId=999999 | 404 / fallback to default | P1 |
| NEG-014 | Invalid version ID for comparison | versionId=-1 | 400 Bad Request | P1 |
| NEG-015 | Invalid language code | lang="xx" | Fallback to default / 400 | P1 |

### 2.2 Authorization Failures (10 tests)

| ID | Test Name | User Context | Expected Result | Priority |
|----|-----------|--------------|-----------------|----------|
| NEG-016 | Unauthenticated generate | No token | 401 Unauthorized | P0 |
| NEG-017 | Unauthenticated export | No token | 401 Unauthorized | P0 |
| NEG-018 | User without opp view permission | Generate | 403 Forbidden | P0 |
| NEG-019 | User without edit permission | Edit statement | 403 Forbidden | P0 |
| NEG-020 | User without export permission | Export PDF | 403 Forbidden | P0 |
| NEG-021 | User from different org unit | Generate for restricted opp | 403 Forbidden | P1 |
| NEG-022 | Read-only user edit attempt | Edit | 403 Forbidden | P1 |
| NEG-023 | Expired session | Any action | 401 / redirect to login | P1 |
| NEG-024 | Token tampered | API request | 401 Unauthorized | P1 |
| NEG-025 | User without approval permission | Submit for approval | 403 Forbidden | P1 |

### 2.3 Invalid State (10 tests)

| ID | Test Name | State | Expected Result | Priority |
|----|-----------|-------|-----------------|----------|
| NEG-026 | Generate for closed opportunity | Status=Closed | Blocked or read-only | P0 |
| NEG-027 | Edit locked statement | Locked by approval | Edit disabled | P0 |
| NEG-028 | Approve draft (no submit) | Draft | Approve not available | P0 |
| NEG-029 | Edit approved statement | Approved | Edit blocked | P0 |
| NEG-030 | Delete final version | Final version | Delete blocked | P0 |
| NEG-031 | Regenerate during approval | Pending approval | Regenerate blocked | P1 |
| NEG-032 | Export during generation | Generation in progress | Wait or queue | P1 |
| NEG-033 | Compare with deleted version | Deleted version ID | 404 Not Found | P1 |
| NEG-034 | Submit already-submitted | Already pending | Duplicate submit blocked | P1 |
| NEG-035 | Edit during concurrent edit | Another user editing | Conflict handling | P1 |

### 2.4 Injection & Security (10 tests)

| ID | Test Name | Malicious Input | Expected Result | Priority |
|----|-----------|-----------------|----------------|----------|
| NEG-036 | SQL injection in text field | `'; DROP TABLE--` | Escaped/sanitized | P0 |
| NEG-037 | XSS in statement text | `<script>alert(1)</script>` | Escaped/sanitized | P0 |
| NEG-038 | Template injection | `{{constructor}}` | Sanitized | P0 |
| NEG-039 | Path traversal in template | `../../../etc/passwd` | Rejected | P0 |
| NEG-040 | HTML injection in WHY | `<img src=x>` | Escaped | P1 |
| NEG-041 | JavaScript in description | `javascript:alert(1)` | Sanitized | P1 |
| NEG-042 | LDAP injection | `*)(uid=*` | Escaped | P1 |
| NEG-043 | Command injection in filename | `; rm -rf /` | Sanitized | P1 |
| NEG-044 | Null byte in path | `file.pdf%00.txt` | Rejected | P1 |
| NEG-045 | Oversized payload | 10MB text | 413 or truncated | P1 |

### 2.5 AI Service Errors (10 tests)

| ID | Test Name | AI Failure | Expected Result | Priority |
|----|-----------|------------|-----------------|----------|
| NEG-046 | AI timeout | 30s timeout | Graceful fallback, no crash | P0 |
| NEG-047 | AI quota exceeded | Rate limit | User message, retry later | P0 |
| NEG-048 | Inappropriate content flagged | AI returns block | Content rejected | P0 |
| NEG-049 | AI hallucination detected | Off-topic output | Validation fails | P1 |
| NEG-050 | AI model unavailable | 503 from service | Fallback to manual | P1 |
| NEG-051 | AI returns empty | Empty response | Fallback or error | P1 |
| NEG-052 | AI returns malformed JSON | Invalid structure | Parse error handled | P1 |
| NEG-053 | AI returns wrong language | Requested en, got fr | Validation / retry | P1 |
| NEG-054 | AI connection refused | Network error | Error message | P1 |
| NEG-055 | AI authentication failed | 401 from AI | Logged, user message | P1 |

### 2.6 Dependency Failures (10 tests)

| ID | Test Name | Failure | Expected Result | Priority |
|----|-----------|---------|-----------------|----------|
| NEG-056 | PDF service unavailable | PDF service down | Error message, retry option | P0 |
| NEG-057 | Template engine error | Template parse fail | Error + fallback | P0 |
| NEG-058 | Storage write failure | Disk full | Error, no partial save | P0 |
| NEG-059 | Database connection lost | DB timeout | Retry or error | P0 |
| NEG-060 | Word generation service down | Word service 503 | Error message | P1 |
| NEG-061 | Template file missing | File not found | Fallback template | P1 |
| NEG-062 | Storage read failure | Corrupt file | Error, no crash | P1 |
| NEG-063 | Cache failure | Redis down | Degraded, no cache | P1 |
| NEG-064 | Notification service down | Email fail | Statement saved, notify later | P1 |
| NEG-065 | Audit log failure | Audit service error | Statement saved, audit retried | P1 |

### 2.7 Format & Validation (10 tests)

| ID | Test Name | Invalid Case | Expected Result | Priority |
|----|-----------|--------------|-----------------|----------|
| NEG-066 | Invalid export format | format="xyz" | 400 Bad Request | P1 |
| NEG-067 | Invalid page range | pageStart=-1 | 400 Bad Request | P1 |
| NEG-068 | Invalid version ID format | versionId="x" | 400 Bad Request | P1 |
| NEG-069 | Mass assignment (extra fields) | JSON with extra props | Ignored | P1 |
| NEG-070 | Empty template content | Template body empty | Error or default | P1 |
| NEG-071 | Invalid font name | font="InvalidFont" | Fallback font | P2 |
| NEG-072 | Invalid margin values | margins=-10 | Validation error | P2 |
| NEG-073 | Invalid watermark format | watermark=object | Rejected | P2 |
| NEG-074 | Duplicate version number | Manual version conflict | Auto-renumber | P2 |
| NEG-075 | Invalid date in schedule | date="invalid" | Validation error | P2 |

### 2.8 Additional Negative (15 tests)

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| NEG-076 | Generate with no sections enabled | All sections disabled | Error or minimal output | P1 |
| NEG-077 | Export before first generation | No statement exists | Prompt to generate first | P0 |
| NEG-078 | Compare single version | Only 1 version | Error message | P1 |
| NEG-079 | Regenerate with no changes | No data change | Idempotent or skip | P2 |
| NEG-080 | Submit empty statement | No content | Validation error | P0 |
| NEG-081 | Delete only version | Last version | Blocked | P1 |
| NEG-082 | Restore deleted version | Version soft-deleted | Restore or 404 | P1 |
| NEG-083 | Generate for cancelled opportunity | Status=Cancelled | Blocked | P1 |
| NEG-084 | Bulk export with invalid IDs | Mix valid/invalid | Partial success + errors | P2 |
| NEG-085 | Concurrent generate same opp | 2 users generate | One succeeds, conflict or queue | P1 |
| NEG-086 | Template with circular refs | Template A refs B refs A | Parse error | P1 |
| NEG-087 | Unicode in invalid encoding | Wrong encoding | Handled or error | P2 |
| NEG-088 | Statement exceeds max size | Very large content | Truncated or error | P1 |
| NEG-089 | Missing data indicator for all sections | All empty | Clear warning | P1 |
| NEG-090 | API version mismatch | Old client | 400 or compatibility | P2 |

---

## §3 Boundary Tests

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### 3.1 Section Length Boundaries (15 tests)

| ID | Test Name | Boundary Value | Expected Result | Priority |
|----|-----------|----------------|-----------------|----------|
| BND-001 | WHY section length = 0 | Empty string | Handled, warning or skip | P0 |
| BND-002 | WHY section length = 100 | Min meaningful | Renders correctly | P0 |
| BND-003 | WHY section length = 1000 | Typical | Renders correctly | P0 |
| BND-004 | WHY section length = 10000 | Long | Renders, pagination | P1 |
| BND-005 | WHY section length = max | At limit | Truncated or error | P1 |
| BND-006 | WHAT section length = 0 | Empty | Handled | P0 |
| BND-007 | WHAT section = 100/1000/10000 | Various | All render | P1 |
| BND-008 | Team section = 0 members | Empty team | Warning | P0 |
| BND-009 | Team section = 1 member | Min | Renders | P0 |
| BND-010 | Team section = 50 members | Many | Pagination/scroll | P1 |
| BND-011 | Budget = 0 | Zero budget | Handled | P0 |
| BND-012 | Budget = 1 (min) | Min value | Renders | P1 |
| BND-013 | Budget = max decimal | 999999999.99 | Renders | P1 |
| BND-014 | Schedule start = end | Same date | Handled | P1 |
| BND-015 | Schedule range = 1 day | Min range | Renders | P1 |

### 3.2 Document Size Boundaries (10 tests)

| ID | Test Name | Boundary Value | Expected Result | Priority |
|----|-----------|----------------|-----------------|----------|
| BND-016 | Total length = 1 page | ~500 words | Single page PDF | P0 |
| BND-017 | Total length = 10 pages | ~5000 words | 10 pages | P1 |
| BND-018 | Total length = 50 pages | ~25000 words | 50 pages | P1 |
| BND-019 | Total length = 100 pages | ~50000 words | 100 pages or split | P1 |
| BND-020 | File size = 0 bytes (empty) | Empty export | Error or minimal | P1 |
| BND-021 | File size = 1 KB | Tiny | Valid file | P2 |
| BND-022 | File size = 10 MB | Large | Success or limit | P1 |
| BND-023 | File size at limit | Max allowed | Success | P1 |
| BND-024 | PDF page count = 1 | Min | Valid | P0 |
| BND-025 | PDF page count = 500 | Very large | Success or limit | P1 |

### 3.3 Template & Version Boundaries (10 tests)

| ID | Test Name | Boundary Value | Expected Result | Priority |
|----|-----------|----------------|-----------------|----------|
| BND-026 | Template count = 1 | Only default | Works | P0 |
| BND-027 | Template count = 10 | Several | All selectable | P1 |
| BND-028 | Template count = 50 | Many | Pagination | P1 |
| BND-029 | Version count = 1 | First version | Works | P0 |
| BND-030 | Version count = 10 | Several | All listed | P1 |
| BND-031 | Version count = 100 | Many | Pagination | P1 |
| BND-032 | Version number = 1 | Initial | Correct | P0 |
| BND-033 | Version number = 999 | High | Correct | P1 |
| BND-034 | Template name length = max | At limit | Truncated or error | P2 |
| BND-035 | Version comment = max length | At limit | Truncated or error | P2 |

### 3.4 Concurrency & Race Boundaries (10 tests)

| ID | Test Name | Scenario | Expected Result | Priority |
|----|-----------|----------|-----------------|----------|
| BND-036 | Concurrent generate same opp | 2 requests | One wins, no corruption | P0 |
| BND-037 | Concurrent edit different sections | 2 users | Merge or conflict | P1 |
| BND-038 | Generate while editing | Overlap | Queue or block | P1 |
| BND-039 | Export during regenerate | Overlap | Queue or block | P1 |
| BND-040 | Version create during export | Overlap | Both succeed | P1 |
| BND-041 | Approval submit during edit | Race | One blocked | P1 |
| BND-042 | Delete version during compare | Race | Error or stale | P2 |
| BND-043 | Template change during generate | Mid-generation | Old or new consistent | P1 |
| BND-044 | Data update during generate | Opp changed | Use consistent snapshot | P1 |
| BND-045 | Multiple exports same version | 5 concurrent | All succeed | P1 |

### 3.5 Unicode & Locale Boundaries (10 tests)

| ID | Test Name | Boundary Value | Expected Result | Priority |
|----|-----------|----------------|-----------------|----------|
| BND-046 | Unicode in WHY (CJK) | 中文日本語한글 | Renders correctly | P0 |
| BND-047 | Unicode in WHAT (Arabic) | العربية | RTL if supported | P1 |
| BND-048 | Unicode in Team names | Accented chars | Renders | P0 |
| BND-049 | Emoji in text | 😀📄 | Escaped or rendered | P1 |
| BND-050 | Mixed scripts | Latin + Cyrillic | Renders | P1 |
| BND-051 | Zero-width chars | ​ | Stripped or handled | P2 |
| BND-052 | Date format en-US | MM/DD/YYYY | Correct | P0 |
| BND-053 | Date format fr-FR | DD/MM/YYYY | Correct | P1 |
| BND-054 | Currency format multiple | USD, EUR, etc. | Correct symbols | P1 |
| BND-055 | Number format locale | 1,000.00 vs 1.000,00 | Correct | P1 |

### 3.6 Data Field Count Boundaries (10 tests)

| ID | Test Name | Boundary Value | Expected Result | Priority |
|----|-----------|----------------|-----------------|----------|
| BND-056 | Funding partners = 0 | None | Section empty | P0 |
| BND-057 | Funding partners = 1 | Single | Renders | P0 |
| BND-058 | Funding partners = 20 | Many | All listed | P1 |
| BND-059 | Stakeholders = 0 | None | Section empty | P0 |
| BND-060 | Stakeholders = 50 | Many | Pagination | P1 |
| BND-061 | SDGs = 0 | None | Section empty | P0 |
| BND-062 | SDGs = 17 | All | All shown | P1 |
| BND-063 | Documents = 0 | None | No attachments | P0 |
| BND-064 | Documents = 100 | Many | All linked | P1 |
| BND-065 | Geography entries = 0 | None | Section empty | P0 |

### 3.7 Comparison & Approval Boundaries (10 tests)

| ID | Test Name | Boundary Value | Expected Result | Priority |
|----|-----------|----------------|-----------------|----------|
| BND-066 | Compare versions 1 vs 2 | Adjacent | Diff shown | P0 |
| BND-067 | Compare versions 1 vs 50 | Far apart | Diff shown | P1 |
| BND-068 | Compare identical versions | Same content | No diff | P1 |
| BND-069 | Compare max diff size | Very different | Handled | P1 |
| BND-070 | Approval chain depth = 1 | Single approver | Works | P0 |
| BND-071 | Approval chain depth = 5 | Long chain | All notified | P1 |
| BND-072 | Approval chain depth = 10 | Very long | Works or limit | P1 |
| BND-073 | Review comments = 0 | None | Works | P0 |
| BND-074 | Review comments = 50 | Many | All shown | P1 |
| BND-075 | Track changes count = max | Many edits | All visible | P1 |

### 3.8 Language & Format Boundaries (15 tests)

| ID | Test Name | Boundary Value | Expected Result | Priority |
|----|-----------|----------------|-----------------|----------|
| BND-076 | Language count = 1 | Only en | Works | P0 |
| BND-077 | Language count = 4 | en, fr, es, pt | All work | P1 |
| BND-078 | Language count = 10 | Many | All selectable | P1 |
| BND-079 | Font size = min | 8pt | Renders | P2 |
| BND-080 | Font size = max | 72pt | Renders | P2 |
| BND-081 | Margin = 0 | No margin | Renders | P2 |
| BND-082 | Margin = max | 2 inch | Renders | P2 |
| BND-083 | Line spacing = 1.0 | Single | Renders | P2 |
| BND-084 | Line spacing = 2.0 | Double | Renders | P2 |
| BND-085 | Header height = 0 | No header | Works | P2 |
| BND-086 | Footer height = max | Large footer | Renders | P2 |
| BND-087 | Watermark opacity = 0 | Invisible | Or skip | P2 |
| BND-088 | Watermark opacity = 1 | Opaque | Renders | P2 |
| BND-089 | Page size = A4 | Default | Works | P0 |
| BND-090 | Page size = Letter | US | Works | P1 |

---

## §4 Functional Tests

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### 4.1 Generation Pipeline (15 tests)

| ID | Test Name | Steps | Expected | Priority |
|----|-----------|-------|----------|----------|
| FUN-001 | Full pipeline: fetch → compile → render | End-to-end | Statement generated | P0 |
| FUN-002 | Pipeline: fetch opportunity data | API call | Data retrieved | P0 |
| FUN-003 | Pipeline: validate data completeness | Validation step | Warnings if incomplete | P0 |
| FUN-004 | Pipeline: compile WHY section | Compile | WHY populated | P0 |
| FUN-005 | Pipeline: compile WHAT section | Compile | WHAT populated | P0 |
| FUN-006 | Pipeline: compile Team section | Compile | Team populated | P0 |
| FUN-007 | Pipeline: compile Budget section | Compile | Budget populated | P0 |
| FUN-008 | Pipeline: compile Schedule section | Compile | Schedule populated | P0 |
| FUN-009 | Pipeline: apply template | Apply | Template applied | P0 |
| FUN-010 | Pipeline: merge sections | Merge | Single document | P0 |
| FUN-011 | Pipeline: add metadata | Add | Version, date in doc | P1 |
| FUN-012 | Pipeline: handle missing optional | Skip | Optional skipped | P1 |
| FUN-013 | Pipeline: retry on transient | Retry | Retries, then fail | P1 |
| FUN-014 | Pipeline: cache intermediate | Cache | Faster on repeat | P2 |
| FUN-015 | Pipeline: audit trail | Log | Steps logged | P1 |

### 4.2 Section Compilation (15 tests)

| ID | Test Name | Section | Expected | Priority |
|----|-----------|---------|----------|----------|
| FUN-016 | Compile WHY from rationale | WHY | Rationale text | P0 |
| FUN-017 | Compile WHY from objectives | WHY | Objectives included | P0 |
| FUN-018 | Compile WHAT from scope | WHAT | Scope text | P0 |
| FUN-019 | Compile WHAT from deliverables | WHAT | Deliverables | P0 |
| FUN-020 | Compile Team from stakeholders | Team | Names, roles | P0 |
| FUN-021 | Compile Team from OM | Team | OM included | P0 |
| FUN-022 | Compile Budget from funding | Budget | Amounts, partners | P0 |
| FUN-023 | Compile Budget from breakdown | Budget | Line items | P1 |
| FUN-024 | Compile Schedule from dates | Schedule | Start, end | P0 |
| FUN-025 | Compile Schedule from milestones | Schedule | Milestones | P1 |
| FUN-026 | Compile with placeholders | Missing | Placeholder text | P1 |
| FUN-027 | Compile with formatting | Format | Bold, lists | P1 |
| FUN-028 | Compile cross-references | Refs | Section refs correct | P1 |
| FUN-029 | Compile conditional sections | Condition | Show/hide by rule | P1 |
| FUN-030 | Compile order override | Order | Custom order | P1 |

### 4.3 Template Processing (15 tests)

| ID | Test Name | Template | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-031 | Load default template | Default | Loaded | P0 |
| FUN-032 | Load custom template | Custom | Loaded | P0 |
| FUN-033 | Parse template variables | {{var}} | Placeholders found | P0 |
| FUN-034 | Substitute variables | Substitute | Values replaced | P0 |
| FUN-035 | Handle conditional blocks | {{#if}} | Conditional rendered | P1 |
| FUN-036 | Handle loops in template | {{#each}} | Loop expanded | P1 |
| FUN-037 | Template inheritance | Extends | Base + override | P1 |
| FUN-038 | Partial templates | Include | Partial included | P1 |
| FUN-039 | Template validation | Invalid | Error | P1 |
| FUN-040 | Template caching | Repeat load | Cached | P2 |
| FUN-041 | Template hot reload | Update file | Reloaded | P2 |
| FUN-042 | Escape HTML in template | <script> | Escaped | P0 |
| FUN-043 | Template locale | fr | French template | P1 |
| FUN-044 | Template fallback | Missing | Default used | P1 |
| FUN-045 | Template versioning | v1, v2 | Correct version | P2 |

### 4.4 Version Management (15 tests)

| ID | Test Name | Action | Expected | Priority |
|----|-----------|--------|----------|----------|
| FUN-046 | Create first version | Generate | v1 created | P0 |
| FUN-047 | Create subsequent version | Edit + save | v2 created | P0 |
| FUN-048 | Version numbering | Sequential | v1, v2, v3 | P0 |
| FUN-049 | List versions | List | All versions | P0 |
| FUN-050 | Get version by ID | Get | Correct version | P0 |
| FUN-051 | Version metadata | Metadata | Author, date | P1 |
| FUN-052 | Version diff | Diff | Changes shown | P1 |
| FUN-053 | Restore version | Restore | Content restored | P1 |
| FUN-054 | Soft delete version | Delete | Soft deleted | P1 |
| FUN-055 | Version approval link | Link | Approval status | P1 |
| FUN-056 | Version export | Export | Exported | P1 |
| FUN-057 | Version pagination | Many | Paginated | P1 |
| FUN-058 | Version filter by date | Filter | Filtered | P2 |
| FUN-059 | Version filter by author | Filter | Filtered | P2 |
| FUN-060 | Version comment | Comment | Comment saved | P1 |

### 4.5 Approval Workflow (15 tests)

| ID | Test Name | Action | Expected | Priority |
|----|-----------|--------|----------|----------|
| FUN-061 | Submit for approval | Submit | Pending | P0 |
| FUN-062 | Approve statement | Approve | Approved | P0 |
| FUN-063 | Reject statement | Reject | Rejected | P0 |
| FUN-064 | Recall submission | Recall | Back to draft | P1 |
| FUN-065 | Approval notification | Notify | Approver notified | P0 |
| FUN-066 | Rejection notification | Notify | Author notified | P0 |
| FUN-067 | Approval history | History | All recorded | P1 |
| FUN-068 | Approval comments | Comments | Saved | P1 |
| FUN-069 | Multi-level approval | Chain | Routes correctly | P1 |
| FUN-070 | Delegation | Delegate | Delegate approves | P1 |
| FUN-071 | Approval SLA | SLA | Tracked | P2 |
| FUN-072 | Approval lock | Lock | Edit blocked | P0 |
| FUN-073 | Approval unlock on reject | Unlock | Editable | P1 |
| FUN-074 | Approval final state | Final | Read-only | P0 |
| FUN-075 | Approval audit | Audit | Logged | P1 |

### 4.6 Additional Functional (15 tests)

| ID | Test Name | Action | Expected | Priority |
|----|-----------|--------|----------|----------|
| FUN-076 | Data freshness check | Check | Indicator updated | P1 |
| FUN-077 | Missing data detection | Detect | Warnings shown | P0 |
| FUN-078 | Regenerate trigger | Trigger | Regeneration | P1 |
| FUN-079 | Export format selection | Select | Correct format | P0 |
| FUN-080 | Export options | Options | Applied | P1 |
| FUN-081 | Comparison algorithm | Compare | Diffs correct | P1 |
| FUN-082 | Search in statement | Search | Results | P1 |
| FUN-083 | Filter versions | Filter | Filtered list | P1 |
| FUN-084 | Sort versions | Sort | Sorted | P1 |
| FUN-085 | Bulk export | Bulk | Multiple exported | P2 |
| FUN-086 | Template CRUD | CRUD | Create, read, update, delete | P2 |
| FUN-087 | Watermark application | Watermark | Applied | P2 |
| FUN-088 | Header/footer application | Header/footer | Applied | P2 |
| FUN-089 | Table of contents | TOC | Generated | P2 |
| FUN-090 | Appendix handling | Appendix | Included | P2 |

---

## §5 Integration Tests

> **Count: 90** | **Minimum: 3×30=90** | ✅ COMPLIANT

### 5.1 Opportunity Data Integration (15 tests)

| ID | Test Name | Integration | Expected | Priority |
|----|-----------|-------------|----------|----------|
| INT-001 | Fetch opportunity by ID | Opportunity API | Data retrieved | P0 |
| INT-002 | Fetch related partner | Partner API | Partner data | P0 |
| INT-003 | Fetch stakeholders | User/Contact API | Stakeholders | P0 |
| INT-004 | Fetch funding partners | Partner API | Funding data | P0 |
| INT-005 | Fetch documents | Document API | Documents | P0 |
| INT-006 | Fetch geography | Geography API | Geography | P1 |
| INT-007 | Fetch SDGs | Reference API | SDGs | P1 |
| INT-008 | Fetch org unit | Org API | Org unit | P1 |
| INT-009 | Opportunity data consistency | Snapshot | Consistent | P0 |
| INT-010 | Opportunity soft delete | Filter | Excluded | P0 |
| INT-011 | Opportunity permission check | Auth | Authorized | P0 |
| INT-012 | Opportunity cache invalidation | Change | Cache cleared | P1 |
| INT-013 | Opportunity pagination | Pagination | Works | P1 |
| INT-014 | Opportunity include related | Include | Related loaded | P1 |
| INT-015 | Opportunity bulk fetch | Bulk | Multiple | P2 |

### 5.2 AI Service Integration (15 tests)

| ID | Test Name | Integration | Expected | Priority |
|----|-----------|-------------|----------|----------|
| INT-016 | AI enhance text | AI API | Enhanced text | P1 |
| INT-017 | AI generate summary | AI API | Summary | P1 |
| INT-018 | AI translate | AI API | Translation | P1 |
| INT-019 | AI timeout handling | Timeout | Graceful | P0 |
| INT-020 | AI retry logic | Retry | Retries | P1 |
| INT-021 | AI fallback | Fallback | Manual | P1 |
| INT-022 | AI rate limit | Rate limit | Queued/error | P1 |
| INT-023 | AI auth | Auth | Token | P1 |
| INT-024 | AI model selection | Model | Correct model | P2 |
| INT-025 | AI prompt template | Prompt | Correct prompt | P1 |
| INT-026 | AI response parsing | Parse | Parsed | P1 |
| INT-027 | AI content filter | Filter | Filtered | P1 |
| INT-028 | AI audit log | Log | Logged | P1 |
| INT-029 | AI cost tracking | Cost | Tracked | P2 |
| INT-030 | AI multi-language | Multi-lang | Correct lang | P1 |

### 5.3 PDF/Word Generation Integration (15 tests)

| ID | Test Name | Integration | Expected | Priority |
|----|-----------|-------------|----------|----------|
| INT-031 | PDF generation service | PDF service | PDF created | P0 |
| INT-032 | Word generation service | Word service | Word created | P0 |
| INT-033 | PDF options | Options | Applied | P1 |
| INT-034 | Word options | Options | Applied | P1 |
| INT-035 | PDF template | Template | Applied | P0 |
| INT-036 | Word template | Template | Applied | P0 |
| INT-037 | PDF fonts | Fonts | Embedded | P1 |
| INT-038 | PDF images | Images | Included | P1 |
| INT-039 | PDF watermark | Watermark | Applied | P2 |
| INT-040 | Word watermark | Watermark | Applied | P2 |
| INT-041 | PDF header/footer | Header/footer | Applied | P1 |
| INT-042 | Word header/footer | Header/footer | Applied | P1 |
| INT-043 | PDF page size | Page size | Applied | P1 |
| INT-044 | PDF margins | Margins | Applied | P1 |
| INT-045 | Export async | Async | Non-blocking | P1 |

### 5.4 Storage Integration (15 tests)

| ID | Test Name | Integration | Expected | Priority |
|----|-----------|-------------|----------|----------|
| INT-046 | Save statement to storage | Storage | Saved | P0 |
| INT-047 | Load statement from storage | Storage | Loaded | P0 |
| INT-048 | Save version to storage | Storage | Saved | P0 |
| INT-049 | Load version from storage | Storage | Loaded | P0 |
| INT-050 | Storage path structure | Path | Correct path | P1 |
| INT-051 | Storage permissions | Permissions | Correct | P1 |
| INT-052 | Storage quota | Quota | Enforced | P1 |
| INT-053 | Storage encryption | Encryption | Encrypted | P1 |
| INT-054 | Storage retention | Retention | Policy applied | P2 |
| INT-055 | Storage cleanup | Cleanup | Old deleted | P2 |
| INT-056 | Storage backup | Backup | Backed up | P2 |
| INT-057 | Storage restore | Restore | Restored | P2 |
| INT-058 | Storage migration | Migration | Migrated | P2 |
| INT-059 | Storage multipart | Large file | Multipart | P1 |
| INT-060 | Storage metadata | Metadata | Stored | P1 |

### 5.5 Notification Integration (15 tests)

| ID | Test Name | Integration | Expected | Priority |
|----|-----------|-------------|----------|----------|
| INT-061 | Email on approval request | Email | Sent | P0 |
| INT-062 | Email on approval | Email | Sent | P0 |
| INT-063 | Email on rejection | Email | Sent | P0 |
| INT-064 | In-app notification | In-app | Shown | P1 |
| INT-065 | Notification template | Template | Applied | P1 |
| INT-066 | Notification recipient | Recipient | Correct | P0 |
| INT-067 | Notification link | Link | Works | P1 |
| INT-068 | Notification retry | Retry | Retried | P1 |
| INT-069 | Notification queue | Queue | Queued | P1 |
| INT-070 | Notification audit | Audit | Logged | P1 |
| INT-071 | Notification preferences | Prefs | Respected | P2 |
| INT-072 | Notification batching | Batch | Batched | P2 |
| INT-073 | Notification dedup | Dedup | No duplicate | P2 |
| INT-074 | Notification locale | Locale | Correct lang | P1 |
| INT-075 | Notification failure handling | Fail | Logged, retry | P1 |

### 5.6 Additional Integration (15 tests)

| ID | Test Name | Integration | Expected | Priority |
|----|-----------|-------------|----------|----------|
| INT-076 | Database transaction | DB | Atomic | P0 |
| INT-077 | Database connection pool | Pool | Reused | P1 |
| INT-078 | Cache integration | Cache | Cached | P1 |
| INT-079 | Audit service | Audit | Logged | P1 |
| INT-080 | Permission service | Permission | Checked | P0 |
| INT-081 | User service | User | Resolved | P1 |
| INT-082 | Org unit service | Org | Resolved | P1 |
| INT-083 | Reference data service | Ref | Loaded | P1 |
| INT-084 | Search service | Search | Indexed | P2 |
| INT-085 | Export service | Export | Exported | P0 |
| INT-086 | Version control service | Version | Versioned | P0 |
| INT-087 | Template service | Template | Loaded | P0 |
| INT-088 | Workflow service | Workflow | Triggered | P0 |
| INT-089 | Logging service | Log | Logged | P1 |
| INT-090 | Metrics service | Metrics | Recorded | P2 |

---

## §7 Concurrency Tests (25)

> **Count: 25** | **Minimum: 25** | ✅ COMPLIANT

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| CON-001 | Concurrent generation same opp | 2 users generate | One succeeds, no corruption | P0 |
| CON-002 | Concurrent edit different sections | 2 users edit | Merge or conflict | P0 |
| CON-003 | Concurrent approval | 2 approvers | One wins | P0 |
| CON-004 | Concurrent version creation | 2 users save | Both versions created | P0 |
| CON-005 | Concurrent export | 2 users export | Both succeed | P0 |
| CON-006 | Generate during edit | Overlap | Queue or block | P1 |
| CON-007 | Export during regenerate | Overlap | Queue or block | P1 |
| CON-008 | Submit during edit | Race | One blocked | P1 |
| CON-009 | Delete version during compare | Race | Handled | P1 |
| CON-010 | Template change during generate | Mid-gen | Consistent | P1 |
| CON-011 | 5 concurrent generations | 5 users | All complete | P1 |
| CON-012 | 10 concurrent exports | 10 users | All succeed | P1 |
| CON-013 | Optimistic locking | Edit conflict | Conflict detected | P0 |
| CON-014 | Pessimistic lock on approval | Lock | Edit blocked | P0 |
| CON-015 | Version merge | Merge | Merged correctly | P1 |
| CON-016 | Cache invalidation | Concurrent update | Cache cleared | P1 |
| CON-017 | DB deadlock | Rare | Retry or error | P1 |
| CON-018 | Connection pool exhaustion | Many concurrent | Queue or limit | P1 |
| CON-019 | File lock | Concurrent write | One wins | P1 |
| CON-020 | Session conflict | Same user 2 tabs | Handled | P1 |
| CON-021 | Bulk export concurrent | Multiple | All complete | P2 |
| CON-022 | Regenerate race | Regenerate + edit | Consistent | P1 |
| CON-023 | Approval chain concurrent | Multiple approvers | Ordered | P1 |
| CON-024 | Storage concurrent write | Same file | One wins | P1 |
| CON-025 | AI request concurrent | Multiple enhance | All complete | P1 |

---

## §8 Unit Tests (21)

> **Count: 21** | **Minimum: 21** | ✅ COMPLIANT

| ID | Test Name | Unit | Expected | Priority |
|----|-----------|------|----------|----------|
| UNT-001 | Template rendering - variable | Render | Substituted | P0 |
| UNT-002 | Template rendering - conditional | Render | Conditional | P0 |
| UNT-003 | Template rendering - loop | Render | Loop | P0 |
| UNT-004 | Template rendering - partial | Render | Included | P1 |
| UNT-005 | Template rendering - escape | Render | Escaped | P0 |
| UNT-006 | Section compilation - WHY | Compile | WHY | P0 |
| UNT-007 | Section compilation - WHAT | Compile | WHAT | P0 |
| UNT-008 | Section compilation - Team | Compile | Team | P0 |
| UNT-009 | Section compilation - Budget | Compile | Budget | P0 |
| UNT-010 | Section compilation - Schedule | Compile | Schedule | P0 |
| UNT-011 | Formatting - PDF | Format | PDF | P0 |
| UNT-012 | Formatting - Word | Format | Word | P0 |
| UNT-013 | Formatting - HTML | Format | HTML | P1 |
| UNT-014 | Validation - required fields | Validate | Error | P0 |
| UNT-015 | Validation - max length | Validate | Error | P0 |
| UNT-016 | Validation - format | Validate | Error | P0 |
| UNT-017 | Validation - sanitization | Validate | Sanitized | P0 |
| UNT-018 | Validation - permission | Validate | Error | P0 |
| UNT-019 | Version numbering - sequential | Number | v1, v2 | P0 |
| UNT-020 | Version numbering - gap | Number | Handled | P1 |
| UNT-021 | Version numbering - reset | Number | Correct | P1 |

---

## §9 Performance Tests (16)

> **Count: 16** | **Minimum: 16** | ✅ COMPLIANT

| ID | Test Name | Metric | Target | Priority |
|----|-----------|--------|--------|----------|
| PRF-001 | Generate statement | Time | < 5s | P0 |
| PRF-002 | PDF export | Time | < 3s | P0 |
| PRF-003 | Word export | Time | < 3s | P0 |
| PRF-004 | Comparison view | Time | < 2s | P1 |
| PRF-005 | Version list | Time | < 300ms | P0 |
| PRF-006 | Generate - memory | Memory | No leak | P1 |
| PRF-007 | PDF - memory | Memory | No leak | P1 |
| PRF-008 | Large document generate | Time | < 15s | P1 |
| PRF-009 | 100 versions list | Time | < 1s | P1 |
| PRF-010 | AI enhance | Time | < 10s | P1 |
| PRF-011 | Template load | Time | < 100ms | P1 |
| PRF-012 | Section compile | Time | < 500ms | P1 |
| PRF-013 | Search in statement | Time | < 500ms | P1 |
| PRF-014 | Bulk export 10 | Time | < 30s | P2 |
| PRF-015 | Cache hit | Time | < 50ms | P2 |
| PRF-016 | Cold start | Time | < 2s | P2 |

---

## §10 Load Tests (10)

> **Count: 10** | **Minimum: 10** | ✅ COMPLIANT

| ID | Test Name | Scenario | Target | Priority |
|----|-----------|----------|--------|----------|
| LDT-001 | 20 concurrent generations | Load | All complete | P0 |
| LDT-002 | Spike - 50 users | Spike | Graceful | P0 |
| LDT-003 | Sustained 10 users 10 min | Sustained | Stable | P0 |
| LDT-004 | Large documents (50 pages) | Size | Complete | P1 |
| LDT-005 | Recovery after load | Recovery | Recovers | P1 |
| LDT-006 | 100 concurrent exports | Load | Queue/complete | P1 |
| LDT-007 | Mixed workload | Mixed | Stable | P1 |
| LDT-008 | Database under load | DB | No timeout | P1 |
| LDT-009 | Storage under load | Storage | No failure | P1 |
| LDT-010 | AI service under load | AI | Rate limit/queue | P1 |

---

**Status:** Ready for Execution
