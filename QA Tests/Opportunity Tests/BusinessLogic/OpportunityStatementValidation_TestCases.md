# Opportunity Statement Validation — Comprehensive Test Cases

**Component:** `OpportunityStatementMarkdown` — AI-generated statement with alignment validation  
**Entity:** `Opportunity.OpportunityStatementMarkdown` (text, nullable)  
**Stage Requirement:** Required for GO submission (requirement #17)  
**Frontend:** `opportunity-statement-section.component.ts/html`  
**Backend:** `UNOPSGeminiManager.GenerateOpportunityStatementAsync`, `ValidateOpportunityStatementAsync`  
**API:** `POST /api/opportunity/{id}/generate-statement`, `POST /api/opportunity/{id}/validate-statement`  
**Created:** 2026-02-17  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| # | Category | Section | Count | Minimum Required | Status |
|---|----------|---------|-------|-----------------|--------|
| 1 | Positive Tests | §1 | 30 | 30-50 | ✅ |
| 2 | Negative Tests | §2 | 90 | Max(50, 2×30=60) | ✅ |
| 3 | Boundary Tests | §3 | 90 | Max(50, 2×30=60) | ✅ |
| 4 | Functional Tests | §4 | 90 | ≥50 | ✅ |
| 5 | Integration Tests | §5 | 90 | ≥50 | ✅ |
| 6 | Security Tests | §6 | — | OUT OF SCOPE | N/A |
| 7 | Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| 8 | Unit Tests | §8 | 21 | ≥21 | ✅ |
| 9 | Performance Tests | §9 | 16 | ≥16 | ✅ |
| 10 | Load Tests | §10 | 10 | ≥10 | ✅ |
| | **TOTAL** | | **462** | **≥462** | ✅ |

**Ratio Checks:**
- **N ≥ 3P:** 90 ≥ 90 → ✅ PASS  
- **E ≥ 3P:** 90 ≥ 90 → ✅ PASS  
- **F ≥ 3P:** 90 ≥ 90 → ✅ PASS  
- **I ≥ 3P:** 90 ≥ 90 → ✅ PASS  

---

## Feature Overview

### Statement Lifecycle

1. **Generate** — AI creates markdown statement from structured opportunity data
2. **Display** — Rendered markdown on opportunity detail page
3. **Edit** — User can manually edit generated statement
4. **Validate** — AI checks statement alignment with structured data
5. **Submit** — Statement required for GO submission (stage requirement #17)

### Validation Rules

| Rule | Source | Details |
|------|--------|---------|
| **Required for GO** | `OpportunityStageRequirementsProvider` | Requirement #17, `FieldTypes.Text`, `Required = true` |
| **Non-empty check** | `WorkflowController.cs` | `string.IsNullOrWhiteSpace(opportunity.OpportunityStatementMarkdown)` |
| **No max length** | Database column | `text` type (unlimited) |
| **No character restrictions** | None | Any characters allowed |
| **AI alignment** | `ValidateOpportunityStatementAsync` | Optional; checks statement aligns with structured data |
| **Auto-generate on submit** | `WorkflowController.cs` | Generates statement if missing before submit |

### API Endpoints

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/api/opportunity/{id}/generate-statement` | POST | Generate/regenerate AI statement |
| `/api/opportunity/{id}/validate-statement` | POST | Validate statement alignment |

---

## §1 Positive Tests — 30

> **Count: 30** | **Minimum: 30-50** | ✅ COMPLIANT

### Statement Generation (POS-001–010)

POS-001: Generate statement for opportunity with complete data → Markdown returned.  
POS-002: Generated statement saved to `OpportunityStatementMarkdown` field.  
POS-003: Generated statement contains opportunity name.  
POS-004: Generated statement is valid markdown.  
POS-005: Regenerate statement → Replaces previous statement.  
POS-006: Generate statement on opportunity with minimal data → Statement still generated.  
POS-007: Generate button visible on statement section.  
POS-008: Regenerate button visible after initial generation.  
POS-009: Loading indicator shown during generation.  
POS-010: Success notification after generation.

### Statement Display (POS-011–018)

POS-011: Statement markdown rendered as formatted HTML on detail page.  
POS-012: Statement section shows markdown preview.  
POS-013: Statement section shows raw markdown in editor.  
POS-014: Statement with headers renders correctly.  
POS-015: Statement with lists renders correctly.  
POS-016: Statement with bold/italic renders correctly.  
POS-017: Statement with links renders with `pi pi-external-link` icon.  
POS-018: Statement section toggle between preview and edit mode.

### Statement Validation (POS-019–028)

POS-019: Validate aligned statement → Returns "aligned" status.  
POS-020: Validate misaligned statement → Returns misalignment items.  
POS-021: Validation results displayed in UI.  
POS-022: Validation loading indicator shown during check.  
POS-023: Validation button visible on statement section.  
POS-024: Validation uses current structured opportunity data.  
POS-025: Validation response includes alignment percentage or score.  
POS-026: Misalignment items listed with specific fields.  
POS-027: Re-validate after statement edit → Updated results.  
POS-028: Re-validate after data change → Updated results.

### Submission Requirement (POS-029–030)

POS-029: Submit with valid statement → Passes requirement #17.  
POS-030: Statement auto-generated if missing during submit.

---

## §2 Negative Tests — 90

> **Count: 90** | **Minimum: Max(50, 2×30=60)** | ✅ COMPLIANT

### Generation Failures (NEG-001–020)

NEG-001: Generate statement for non-existent opportunity → 404.  
NEG-002: Generate statement for soft-deleted opportunity → 404.  
NEG-003: Generate statement without authentication → 401.  
NEG-004: Generate statement without permission → 403.  
NEG-005: Generate statement on immutable opportunity (NO GO) → Blocked.  
NEG-006: Generate statement on immutable opportunity (GO) → Blocked.  
NEG-007: Generate statement on immutable opportunity (CANCELLED) → Blocked.  
NEG-008: Generate statement when AI service unavailable → Error message.  
NEG-009: Generate statement when AI service times out → Timeout error.  
NEG-010: Generate statement when AI returns empty → Error or retry.  
NEG-011: Generate statement when AI returns invalid markdown → Stored as-is.  
NEG-012: Generate statement with opportunity ID=0 → Validation error.  
NEG-013: Generate statement with negative opportunity ID → Validation error.  
NEG-014: Generate statement with malformed request → 400.  
NEG-015: Generate statement during database outage → Error message.  
NEG-016: Generate statement with very large opportunity data → AI handles or truncates.  
NEG-017: Generate statement when previous generation is still running → Queued or rejected.  
NEG-018: Generate statement API called with GET instead of POST → 405.  
NEG-019: Generate statement with tampered JWT → 401.  
NEG-020: Generate statement on opportunity with no data at all → Minimal statement or error.

### Validation Failures (NEG-021–040)

NEG-021: Validate statement for non-existent opportunity → 404.  
NEG-022: Validate statement when no statement exists → Error (nothing to validate).  
NEG-023: Validate empty statement → Error.  
NEG-024: Validate whitespace-only statement → Error.  
NEG-025: Validate statement without authentication → 401.  
NEG-026: Validate statement without permission → 403.  
NEG-027: Validate statement when AI service unavailable → Error.  
NEG-028: Validate statement when AI service times out → Timeout.  
NEG-029: Validate statement with completely unrelated content → Fully misaligned.  
NEG-030: Validate statement with AI returning invalid response → Error handling.  
NEG-031: Validate statement with opportunity ID=0 → Validation error.  
NEG-032: Validate statement with negative opportunity ID → Validation error.  
NEG-033: Validate statement with malformed request → 400.  
NEG-034: Validate statement after opportunity data changed → Misalignments detected.  
NEG-035: Validate statement with SQL injection in content → Sanitized.  
NEG-036: Validate statement with XSS in content → HTML escaped.  
NEG-037: Validate statement with null bytes → Sanitized.  
NEG-038: Validate statement with CRLF injection → Sanitized.  
NEG-039: Validate statement API called with GET → 405.  
NEG-040: Validate statement with concurrent data modification → Handles gracefully.

### Submission Failures (NEG-041–055)

NEG-041: Submit without statement (null) → Unmet requirement.  
NEG-042: Submit without statement (empty string) → Unmet requirement.  
NEG-043: Submit without statement (whitespace only) → Unmet requirement.  
NEG-044: Submit with statement but other requirements unmet → Other requirements listed.  
NEG-045: Submit without `AcknowledgedStatement` → Blocked.  
NEG-046: Auto-generate statement during submit fails → Submit blocked with error.  
NEG-047: Auto-generate statement during submit timeout → Submit blocked.  
NEG-048: Submit with statement on immutable opportunity → Invalid state.  
NEG-049: Submit with statement on already-submitted opportunity → Duplicate submit.  
NEG-050: Submit unauthenticated → 401.  
NEG-051: Submit without permission → 403.  
NEG-052: Submit with forged request → Authorization check.  
NEG-053: Submit during AI service outage → Statement auto-gen fails.  
NEG-054: Submit with database error → Transaction rolled back.  
NEG-055: Submit with concurrent statement modification → Conflict or last-write-wins.

### Edit/Save Failures (NEG-056–070)

NEG-056: Edit statement on immutable opportunity → Blocked.  
NEG-057: Edit statement during approval workflow → Blocked.  
NEG-058: Edit statement without permission → Blocked.  
NEG-059: Save empty statement (clearing content) → Saved as null or empty.  
NEG-060: Save statement with only whitespace → Treated as empty.  
NEG-061: Save statement API with malformed JSON → 400.  
NEG-062: Save statement with database error → Error, no partial save.  
NEG-063: Save statement with very long content → Accepted (text type, no limit).  
NEG-064: Save statement with binary content → Stored but renders poorly.  
NEG-065: Save statement from unauthorized session → 401.  
NEG-066: Save statement with CSRF attack → Protected.  
NEG-067: Save statement and immediately navigate away → Save completes or warns.  
NEG-068: Save statement with network error → Retry possible.  
NEG-069: Save statement with concurrent edit from another user → Last-write-wins.  
NEG-070: Save statement with stale optimistic concurrency token → Conflict.

### Statement Validation Failures (NEG-071–090)

NEG-071: Validate statement with mismatched opportunity name → Misalignment flagged.  
NEG-072: Validate statement with mismatched budget amount → Misalignment flagged.  
NEG-073: Validate statement with mismatched partner names → Misalignment flagged.  
NEG-074: Validate statement with mismatched country list → Misalignment flagged.  
NEG-075: Validate statement with mismatched stakeholder names → Misalignment flagged.  
NEG-076: Validate statement with mismatched dates → Misalignment flagged.  
NEG-077: Validate statement with mismatched SDG codes → Misalignment flagged.  
NEG-078: Validate statement with truncated content (missing sections) → Misalignment flagged.  
NEG-079: Validate statement with fabricated data not in opportunity → May flag or ignore.  
NEG-080: Validate statement with wrong numeric precision → Misalignment flagged.  
NEG-081: Validate statement with wrong currency format → Misalignment flagged.  
NEG-082: Validate statement with wrong date format → Misalignment flagged.  
NEG-083: Validate statement with outdated partner roles → Misalignment flagged.  
NEG-084: Validate statement with wrong geographic scope → Misalignment flagged.  
NEG-085: Validate statement with missing required sections → Misalignment flagged.  
NEG-086: Validate statement with wrong entity type (partner vs contact) → Misalignment flagged.  
NEG-087: Validate statement when validation service returns 500 → Error shown.  
NEG-088: Validate statement when validation service returns malformed JSON → Error handling.  
NEG-089: Validate statement with null opportunity reference in request → 400.  
NEG-090: Validate statement with wrong content-type header → 400 or 415.

---

## §3 Boundary Tests — 90

> **Count: 90** | **Minimum: Max(50, 2×30=60)** | ✅ COMPLIANT

### Statement Content Boundaries (BND-001–025)

BND-001: Statement = 1 character → Accepted, passes requirement.  
BND-002: Statement = 10 characters → Accepted.  
BND-003: Statement = 100 characters → Accepted.  
BND-004: Statement = 1000 characters → Accepted.  
BND-005: Statement = 10,000 characters → Accepted.  
BND-006: Statement = 100,000 characters → Accepted (text type).  
BND-007: Statement = 1,000,000 characters → Accepted but may affect performance.  
BND-008: Statement with 0 lines → Empty (fails requirement).  
BND-009: Statement with 1 line → Valid.  
BND-010: Statement with 100 lines → Valid.  
BND-011: Statement with 1000 lines → Valid, scrollable.  
BND-012: Statement with only markdown headers → Valid.  
BND-013: Statement with only bullet points → Valid.  
BND-014: Statement with only links → Valid.  
BND-015: Statement with nested markdown (lists in lists) → Valid.  
BND-016: Statement with markdown tables → Rendered correctly.  
BND-017: Statement with code blocks → Rendered correctly.  
BND-018: Statement with Unicode (Arabic, Chinese, Cyrillic) → Stored and rendered.  
BND-019: Statement with emoji → Stored and rendered.  
BND-020: Statement with special characters (`<>{}[]()`) → Escaped in HTML.  
BND-021: Statement with HTML tags → Escaped or sanitized.  
BND-022: Statement with script tags → XSS prevention.  
BND-023: Statement with markdown images → Rendered.  
BND-024: Statement with markdown horizontal rules → Rendered.  
BND-025: Statement with mixed formatting → Rendered correctly.

### AI Generation Boundaries (BND-026–045)

BND-026: Generate from opportunity with 1 field populated → Minimal statement.  
BND-027: Generate from opportunity with all fields populated → Full statement.  
BND-028: Generate from opportunity with very long field values → AI handles.  
BND-029: Generate from opportunity with Unicode field values → Included.  
BND-030: Generate from opportunity with special characters → Escaped.  
BND-031: Generate from opportunity with 0 stakeholders → Statement omits section.  
BND-032: Generate from opportunity with 100 stakeholders → AI summarizes.  
BND-033: Generate from opportunity with 0 partners → Statement omits section.  
BND-034: Generate from opportunity with 50 partners → AI summarizes.  
BND-035: Generate from opportunity with 0 countries → Statement omits section.  
BND-036: Generate from opportunity with 30 countries → AI summarizes.  
BND-037: Generate from opportunity with budget = 0 → Included as zero.  
BND-038: Generate from opportunity with budget = MAX_DECIMAL → Formatted.  
BND-039: Generated statement markdown format consistent.  
BND-040: Generated statement length proportional to input data.  
BND-041: AI response time proportional to data complexity.  
BND-042: AI retry on transient failure → Second attempt succeeds.  
BND-043: AI rate limit hit → Queued or error.  
BND-044: AI token limit exceeded → Truncated or error.  
BND-045: AI model version change → Statement quality may differ.

### Validation Alignment Boundaries (BND-046–060)

BND-046: 100% aligned statement → No misalignment items.  
BND-047: 0% aligned statement → All items misaligned.  
BND-048: 50% aligned → Half items misaligned.  
BND-049: Statement matches data exactly → Fully aligned.  
BND-050: Statement slightly outdated (1 field changed) → 1 misalignment.  
BND-051: Statement fully outdated (all fields changed) → Fully misaligned.  
BND-052: Statement with extra information not in data → May not flag.  
BND-053: Statement missing critical data points → Flagged.  
BND-054: Statement with incorrect numbers → Flagged.  
BND-055: Statement with incorrect dates → Flagged.  
BND-056: Statement with incorrect partner names → Flagged.  
BND-057: Validation with concurrent data modification → Validates against current data.  
BND-058: Validation immediately after generation → Fully aligned.  
BND-059: Validation after manual edit → May have misalignments.  
BND-060: Validation response size proportional to misalignments.

### Submission Requirement Boundaries (BND-061–070)

BND-061: Statement = null → Fails requirement.  
BND-062: Statement = "" → Fails requirement.  
BND-063: Statement = " " → Fails requirement (whitespace only).  
BND-064: Statement = "\t\n" → Fails requirement (whitespace only).  
BND-065: Statement = "a" → Passes requirement (non-empty).  
BND-066: Statement auto-generated during submit → Passes requirement.  
BND-067: Statement auto-generation fails → Submit blocked.  
BND-068: Statement requirement is #17 of 17+ requirements → Checked last.  
BND-069: All requirements met including statement → Submit succeeds.  
BND-070: Statement met but other requirements not → Submit blocked with other reasons.

### Statement Validation Boundaries (BND-071–090)

BND-071: Validation alignment score exactly 0% → All misalignments.  
BND-072: Validation alignment score exactly 100% → No misalignments.  
BND-073: Validation alignment score 99% → One minor misalignment.  
BND-074: Validation with 1 misalignment item → Single item in response.  
BND-075: Validation with 50 misalignment items → All listed.  
BND-076: Validation with statement length = 0 → Error.  
BND-077: Validation with statement length = 1 character → Validates.  
BND-078: Validation with statement length = 1M characters → May timeout or truncate.  
BND-079: Validation with opportunity having 0 related entities → Aligned if statement minimal.  
BND-080: Validation with opportunity having max related entities → Completes.  
BND-081: Validation immediately after data save → Uses latest data.  
BND-082: Validation with statement containing only numbers → Validates.  
BND-083: Validation with statement containing only dates → Validates.  
BND-084: Validation with statement containing only names → Validates.  
BND-085: Validation with statement in different language than data → May misalign.  
BND-086: Validation with statement using abbreviations vs full names → May flag.  
BND-087: Validation with statement using different number formats → May flag.  
BND-088: Validation with statement using different date formats → May flag.  
BND-089: Validation response with empty misalignment list → Aligned.  
BND-090: Validation response with max misalignment list size → All returned.

---

## §4 Functional Tests — 90

> **Count: 90** | **Minimum: ≥50** | ✅ COMPLIANT

### Generation Rules (FUN-001–015)

FUN-001: Generate API creates markdown from `GetOpportunityDetailsForStatementValidationAsync`.  
FUN-002: Generated statement saved to `OpportunityStatementMarkdown` field.  
FUN-003: Generate updates `LastModifiedBy` and `LastModifiedDate`.  
FUN-004: Regenerate replaces existing statement entirely.  
FUN-005: Generate uses current structured data (not cached).  
FUN-006: Generate works for opportunities in Draft status.  
FUN-007: Generate works for opportunities in Active status.  
FUN-008: Generate blocked for immutable stages.  
FUN-009: Generate UI shows loading spinner.  
FUN-010: Generate UI shows success/error toast.  
FUN-011: Generate button text changes to "Regenerate" after first generation.  
FUN-012: Generated markdown includes section headers.  
FUN-013: Generated markdown includes opportunity context.  
FUN-014: Generated markdown includes partner information.  
FUN-015: Generated markdown includes budget information.

### Validation Rules (FUN-016–030)

FUN-016: Validate compares statement against `GetOpportunityDetailsForStatementValidationAsync`.  
FUN-017: Validate returns alignment status (aligned/misaligned).  
FUN-018: Validate returns list of misalignment items.  
FUN-019: Validation results displayed below statement.  
FUN-020: Aligned status shows success indicator.  
FUN-021: Misaligned status shows warning indicator.  
FUN-022: Misalignment items clickable for details.  
FUN-023: Validation is optional (not required for submission).  
FUN-024: Validation can be repeated after edits.  
FUN-025: Validation uses latest opportunity data.  
FUN-026: Validation results cleared on new generation.  
FUN-027: Validation results persist across page navigation (within session).  
FUN-028: Validation button disabled during validation.  
FUN-029: Validation button disabled during generation.  
FUN-030: Validation results not persisted to database.

### Submission Rules (FUN-031–040)

FUN-031: Stage requirement #17 checks `opportunityStatementMarkdown`.  
FUN-032: `string.IsNullOrWhiteSpace()` check on server side.  
FUN-033: Auto-generate called if statement missing during submit.  
FUN-034: `AcknowledgedStatement` flag in `WorkflowSubmitRequest`.  
FUN-035: Unmet requirements dialog includes statement description.  
FUN-036: Statement requirement description: `message.requirements.opportunity.statementRequired`.  
FUN-037: Statement FieldType = `Text`.  
FUN-038: Statement FieldName = `opportunityStatementMarkdown`.  
FUN-039: Statement visible on submitted opportunity (read-only).  
FUN-040: Statement visible on approved opportunity (read-only).

### Display Rules (FUN-041–050)

FUN-041: Markdown rendered as HTML in preview mode.  
FUN-042: Raw markdown shown in edit mode.  
FUN-043: Edit/preview toggle works correctly.  
FUN-044: Statement section has correct heading.  
FUN-045: Statement section conditional visibility (only if data exists or in edit mode).  
FUN-046: Statement FormControl synced with opportunity data.  
FUN-047: Statement changes tracked for unsaved changes warning.  
FUN-048: Statement saved via opportunity save (not separate endpoint).  
FUN-049: Statement markdown external links render with icon.  
FUN-050: Statement section responsive on mobile.

### Statement Validation Rules (FUN-051–090)

FUN-051: Validation compares statement opportunity name to structured data.  
FUN-052: Validation compares statement budget to structured data.  
FUN-053: Validation compares statement partner list to structured data.  
FUN-054: Validation compares statement country list to structured data.  
FUN-055: Validation compares statement stakeholder list to structured data.  
FUN-056: Validation compares statement dates to structured data.  
FUN-057: Validation compares statement SDG references to structured data.  
FUN-058: Validation misalignment item includes field name.  
FUN-059: Validation misalignment item includes expected vs actual.  
FUN-060: Validation misalignment item includes severity or type.  
FUN-061: Validation handles numeric comparison (tolerance for rounding).  
FUN-062: Validation handles date comparison (format-agnostic).  
FUN-063: Validation handles name comparison (case-insensitive or normalized).  
FUN-064: Validation ignores extra content in statement not conflicting.  
FUN-065: Validation flags missing required data in statement.  
FUN-066: Validation flags incorrect data in statement.  
FUN-067: Validation response includes overall alignment percentage.  
FUN-068: Validation response includes misalignment count.  
FUN-069: Validation API accepts opportunity ID in path.  
FUN-070: Validation API uses statement from opportunity (no body).  
FUN-071: Validation clears previous results on new validation run.  
FUN-072: Validation error state shows retry option.  
FUN-073: Validation timeout shows user-friendly message.  
FUN-074: Validation with AI unavailable shows fallback message.  
FUN-075: Validation results sortable by field or severity.  
FUN-076: Validation results filterable (if multiple).  
FUN-077: Validation triggers on explicit user action only.  
FUN-078: Validation does not auto-run on statement load.  
FUN-079: Validation does not auto-run on statement save.  
FUN-080: Validation uses same data source as generation.  
FUN-081: Validation respects user permission to validate.  
FUN-082: Validation blocked for immutable opportunities.  
FUN-083: Validation works for Draft opportunity.  
FUN-084: Validation works for Active opportunity.  
FUN-085: Validation result format consistent across runs.  
FUN-086: Validation misalignment descriptions human-readable.  
FUN-087: Validation supports i18n for result messages.  
FUN-088: Validation accessibility: results announced to screen reader.  
FUN-089: Validation loading state blocks duplicate requests.  
FUN-090: Validation completes even with partial AI response (graceful degradation).

---

## §5 Integration Tests — 90

> **Count: 90** | **Minimum: ≥50** | ✅ COMPLIANT

### Generation End-to-End (INT-001–015)

INT-001: Generate statement → Verify saved in DB.  
INT-002: Generate statement → Verify displayed on UI.  
INT-003: Generate statement → Verify in API GET response.  
INT-004: Regenerate statement → Verify old statement replaced.  
INT-005: Generate statement with complete opportunity → Long statement.  
INT-006: Generate statement with minimal opportunity → Short statement.  
INT-007: Generate statement → Page refresh → Statement persists.  
INT-008: Generate statement → Navigate away → Statement persists.  
INT-009: Generate statement on opportunity with partners → Partners referenced.  
INT-010: Generate statement on opportunity with budget → Budget referenced.  
INT-011: Generate statement on opportunity with countries → Countries referenced.  
INT-012: Generate statement on opportunity with stakeholders → Stakeholders referenced.  
INT-013: Generate statement on opportunity with SDGs → SDGs referenced.  
INT-014: Generate statement API returns 200 on success.  
INT-015: Generate statement API returns statement in response body.

### Validation End-to-End (INT-016–025)

INT-016: Generate → Validate → Aligned result.  
INT-017: Generate → Edit statement → Validate → May show misalignments.  
INT-018: Generate → Change opportunity data → Validate → Misalignments shown.  
INT-019: Validate → Results displayed in UI correctly.  
INT-020: Validate → Misalignment items reference specific fields.  
INT-021: Validate API returns 200 on success.  
INT-022: Validate API returns alignment status and items.  
INT-023: Validate with AI service error → Error shown in UI.  
INT-024: Validate after reopen from NO GO → Works on Draft.  
INT-025: Validate on opportunity with maximum data → Completes.

### Submission End-to-End (INT-026–040)

INT-026: Submit with valid statement → Requirement passed.  
INT-027: Submit without statement → Auto-generated → Passes.  
INT-028: Submit without statement + AI failure → Submit blocked.  
INT-029: Submit → Statement visible on submitted opportunity.  
INT-030: Submit → Approve → Statement visible on GO opportunity.  
INT-031: Submit → Reject → Reopen → Statement preserved.  
INT-032: Submit → Statement field read-only during approval.  
INT-033: Submit → Recall → Statement editable again.  
INT-034: Unmet requirements check → Statement missing → Listed.  
INT-035: Unmet requirements check → Statement present → Not listed.  
INT-036: All 17 requirements met → Submit proceeds.  
INT-037: Only statement missing → Only statement in unmet list.  
INT-038: Statement + other requirements missing → All in unmet list.  
INT-039: Statement auto-generated on submit → Visible after submit.  
INT-040: Submit API includes AcknowledgedStatement flag.

### Cross-Component (INT-041–050)

INT-041: Statement section in opportunity view → Correct placement.  
INT-042: Statement section with AI panel → Both functional.  
INT-043: Statement section with workflow component → Interaction correct.  
INT-044: Statement in AI summary → Included as data source.  
INT-045: Statement in opportunity export → Included.  
INT-046: Statement in opportunity print → Rendered.  
INT-047: Statement changes trigger unsaved changes warning.  
INT-048: Statement save failure → Error toast shown.  
INT-049: Statement section accessibility → Keyboard navigable.  
INT-050: Statement section i18n → Labels translated.

### Statement Validation Integration (INT-051–090)

INT-051: Generate → Validate (no edit) → Aligned → DB and UI consistent.  
INT-052: Generate → Edit name in statement → Validate → Misalignment on name.  
INT-053: Generate → Edit budget in statement → Validate → Misalignment on budget.  
INT-054: Change opportunity data in another tab → Validate → Detects changes.  
INT-055: Validate → Edit statement to fix misalignment → Re-validate → Aligned.  
INT-056: Validate → Change opportunity data to match statement → Re-validate → Aligned.  
INT-057: Validate with multiple misalignments → All displayed in UI.  
INT-058: Validate → Click misalignment item → Highlights or navigates to field.  
INT-059: Validate during generation → Validation queued or blocked.  
INT-060: Validate after generation completes → Immediate validation works.  
INT-061: Validate → Navigate to another section → Return → Results persist.  
INT-062: Validate → Regenerate statement → Validation results cleared.  
INT-063: Validate → Save opportunity (no statement change) → Results persist.  
INT-064: Validate API + Generate API same opportunity → No conflict.  
INT-065: Validate with statement containing markdown → Parsed correctly.  
INT-066: Validate with statement containing links → Links not validated as data.  
INT-067: Validate with statement in different locale → Comparison locale-agnostic.  
INT-068: Validate → Export opportunity → Statement and validation state in export.  
INT-069: Validate → Print opportunity → Validation results in print view.  
INT-070: Validate with soft-deleted related entity in statement → Handles gracefully.  
INT-071: Validate after partner removed from opportunity → Misalignment.  
INT-072: Validate after country added to opportunity → Misalignment.  
INT-073: Validate after stakeholder role changed → Misalignment.  
INT-074: Validate after budget updated → Misalignment.  
INT-075: Validate after date range changed → Misalignment.  
INT-076: Validate with AI panel open → Both functional.  
INT-077: Validate with workflow component showing → No UI conflict.  
INT-078: Validate from opportunity list (if available) → Works.  
INT-079: Validate in read-only view → Validation button available.  
INT-080: Validate in edit view → Validation button available.  
INT-081: Validate result → Copy to clipboard (if supported) → Correct format.  
INT-082: Validate → Refresh page → Results cleared (not persisted).  
INT-083: Validate with network interruption → Error or retry.  
INT-084: Validate with slow AI response → Loading state throughout.  
INT-085: Validate with AI returning partial response → Graceful handling.  
INT-086: Validate + concurrent opportunity save → Validates latest.  
INT-087: Validate + concurrent statement edit → Validates current statement.  
INT-088: Validate on opportunity with 0 related data → Minimal alignment check.  
INT-089: Validate on opportunity with max related data → Completes.  
INT-090: Full flow: Create opportunity → Generate → Validate → Edit → Re-validate → Submit → Success.

---

## §6 Security Tests — OUT OF SCOPE

---

## §7 Concurrency Tests — 25

> **Count: 25** | **Minimum: ≥25** | ✅ COMPLIANT

CON-001: Two users generating statement simultaneously → Both get result.  
CON-002: Generate + validate simultaneously → Both complete.  
CON-003: Generate + save opportunity simultaneously → Last write wins.  
CON-004: Generate during submit → Generate completes, submit uses result.  
CON-005: Two users editing statement simultaneously → Last save wins.  
CON-006: Generate + page refresh → Generation completes, data refreshes.  
CON-007: Validate during data modification → Validates against current data.  
CON-008: Concurrent generate requests for same opportunity → Both return.  
CON-009: AI service under load → Queued, eventually completes.  
CON-010: AI service timeout during concurrent requests → Individual timeouts.  
CON-011: Statement save + concurrent generate → One overwrites other.  
CON-012: Submit + concurrent statement edit → Submit uses committed version.  
CON-013: Concurrent auto-generate during submit → Single generation.  
CON-014: Generate button rapid double-click → Debounced.  
CON-015: Validate button rapid double-click → Debounced.  
CON-016: Statement section reactive update → Signal recomputes correctly.  
CON-017: Concurrent statement reads → All return same data.  
CON-018: Cache invalidation after statement save → Next read sees latest.  
CON-019: AI response caching → Same request returns cached result.  
CON-020: Statement save during network instability → Retry or error.  
CON-021: Concurrent opportunity saves including statement → Last wins.  
CON-022: Generate followed immediately by validate → Aligned.  
CON-023: Concurrent PDF generation with statement → Statement included.  
CON-024: Statement auto-gen race with manual save → One wins.  
CON-025: Concurrent access from two browser tabs → Consistent state.

---

## §8 Unit Tests — 21

> **Count: 21** | **Minimum: ≥21** | ✅ COMPLIANT

UNT-001: `string.IsNullOrWhiteSpace(null)` returns true → Fails requirement.  
UNT-002: `string.IsNullOrWhiteSpace("")` returns true → Fails requirement.  
UNT-003: `string.IsNullOrWhiteSpace(" ")` returns true → Fails requirement.  
UNT-004: `string.IsNullOrWhiteSpace("content")` returns false → Passes requirement.  
UNT-005: Stage requirement #17 has Name = `opportunityStatementMarkdown`.  
UNT-006: Stage requirement #17 has FieldType = Text.  
UNT-007: Stage requirement #17 has Required = true.  
UNT-008: Statement FormControl initialization → Correct default value.  
UNT-009: Statement FormControl dirty tracking → Correct.  
UNT-010: Markdown rendering function → Converts markdown to HTML.  
UNT-011: Markdown sanitization → Strips dangerous HTML.  
UNT-012: Markdown link rendering → Adds external-link icon.  
UNT-013: Validate alignment response parsing → Correct model.  
UNT-014: Validate misalignment items parsing → Correct list.  
UNT-015: Generate button disabled when loading.  
UNT-016: Validate button disabled when loading.  
UNT-017: Generate button text changes after first generation.  
UNT-018: AcknowledgedStatement flag default → false.  
UNT-019: AcknowledgedStatement flag set → true after acknowledgment.  
UNT-020: Unmet requirements list includes statement description.  
UNT-021: Unmet requirements list excludes statement when present.

---

## §9 Performance Tests — 16

> **Count: 16** | **Minimum: ≥16** | ✅ COMPLIANT

PRF-001: Generate statement API < 30s (AI processing).  
PRF-002: Validate statement API < 15s (AI processing).  
PRF-003: Statement save (as part of opportunity save) < 500ms.  
PRF-004: Statement section render < 200ms.  
PRF-005: Markdown rendering < 100ms for 10K character statement.  
PRF-006: Markdown rendering < 500ms for 100K character statement.  
PRF-007: Statement display on page load < 200ms.  
PRF-008: Edit/preview toggle < 50ms.  
PRF-009: Unmet requirements check < 100ms.  
PRF-010: Auto-generate during submit < 30s.  
PRF-011: Generate memory usage stable (no leak over 100 generations).  
PRF-012: Validate memory usage stable.  
PRF-013: AI service response time for 10 concurrent requests < 60s each.  
PRF-014: Statement FormControl sync < 10ms.  
PRF-015: Statement search indexing (if applicable) < 500ms.  
PRF-016: Statement in opportunity export < 1s additional.

---

## §10 Load Tests — 10

> **Count: 10** | **Minimum: ≥10** | ✅ COMPLIANT

LDT-001: 10 concurrent statement generations → All complete < 60s.  
LDT-002: 20 concurrent statement validations → All complete < 30s.  
LDT-003: 50 concurrent opportunity saves with statements → All succeed.  
LDT-004: 10 concurrent submits with auto-generate → All complete.  
LDT-005: AI service under heavy load → Graceful degradation.  
LDT-006: Spike: 20 generations in 10 seconds → Queued, all complete.  
LDT-007: Sustained generation load (50/hour) → AI service stable.  
LDT-008: Statement display for 100 concurrent users → Page loads < 2s.  
LDT-009: Recovery after AI service outage → Generations resume.  
LDT-010: Recovery after database outage → Statement saves resume.

---

## Status: Ready for Implementation
