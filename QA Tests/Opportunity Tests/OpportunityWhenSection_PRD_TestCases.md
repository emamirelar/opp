# Opportunity WHEN Section — Test Cases

**Component:** Opportunity WHEN - Timeline & Key Dates  
**Created:** 2026-03-09 | **Last Updated:** 2026-03-09  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio  
**Jira Tickets:** PNO-699, PNO-811, PNO-859

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive   | 30 | 30  | ✅ |
| §2 Negative   | 90 | 90  | ✅ |
| §3 Boundary   | 90 | 90  | ✅ |
| §4 Functional | 90 | 90  | ✅ |
| §5 Integration| 90 | 90  | ✅ |
| **TOTAL**     | **390** | **≥390** | ✅ |

**3:1 Ratio Checks:**
- N ≥ 3P: 90 ≥ 90 → ✅ PASS
- B ≥ 3P: 90 ≥ 90 → ✅ PASS
- F ≥ 3P: 90 ≥ 90 → ✅ PASS
- I ≥ 3P: 90 ≥ 90 → ✅ PASS

---

## Feature Overview

The WHEN section provides timeline and key dates for opportunities: Target Signing Date, Implementation Start Date, Target Delivery Date, Submission Deadline. It includes a duration calculator (3, 6, 12, 18, 24, 36 months + Custom), date validation (start before end, submission ≤ signing), and signing date details (firm deadline, notes). Tests validate PNO-699 (core feature), PNO-811 (6-month option), and PNO-859 (date validation, calculator optional, manual date clears duration).

---

## Requirement Traceability

| Requirement | Source | Test(s) | Validated? | Defect? |
|-------------|--------|---------|------------|---------|
| AC1: WHEN section exists with timeline and key dates | PNO-699 | HtmlTemplate_WhenSectionExists_ContainsTimelineKeyDates, Workflow_PNO699_AC1_SectionTitle | ✅ | — |
| AC2: Target Signing, Implementation Start, Target Delivery; duration derived | PNO-699 | Spec_ValidDates_NoValidationErrors, HtmlTemplate_*_HasFormControl, Spec_EffectiveImplementationStart_* | ✅ | — |
| AC3: Days until signing, timeline Gantt | PNO-699 | HtmlTemplate_TimelineOverview_Exists, Spec_DaysUntilImplementationStart_Computed | ✅ | — |
| AC5: Submission deadline before signing, firm deadline notes | PNO-699 | Spec_SubmissionDeadlineBeforeSigning_Valid, HtmlTemplate_SigningDateDetails_SectionExists | ✅ | — |
| 6-month duration option | PNO-811 | DurationOptions_IncludeSixMonths_PNO811, Workflow_PNO811_SixMonthsOption | ✅ | — |
| Date validation: end before start greyed out | PNO-859 | HtmlTemplate_*_HasMinDateBinding, Spec_*_ValidationRule_* | ✅ | — |
| Calculator optional | PNO-859 | HtmlTemplate_DurationCalculator_HasOptionalHint_PNO859 | ✅ | — |
| Manual date change clears duration | PNO-859 | Spec_OnDeliveryDateManualChange_ClearsDuration_PNO859 | ✅ | — |
| Server-side date validation | PNO-699/PNO-859 | — | ❌ Not implemented | DEF-146 |

---

## §1 Positive — 30

POS-001–030: WHEN section exists; date fields (Target Signing, Implementation Start, Target Delivery, Submission Deadline) with form controls; appendTo body; min/max date bindings; duration options including 6 months; valid date chronology; effective implementation start defaults to signing; normalization to UTC midnight; float labels; timeline overview; work breakdown structure.

---

## §2 Negative — 90

NEG-001–090: Implementation start before signing; delivery before implementation start; submission after signing; null handling; validation error combinations; duration options validation; template contract checks; save blocked when invalid; manual change handlers.

---

## §3 Boundary — 90

BND-001–090: Same-day dates; exact equality; one-day boundaries; null effective start; duration calculations (3, 6, 12, 18, 24, 36 months); leap year; year/month boundaries; custom duration min/max (1–120); time component ignored; explicitly set vs defaulted.

---

## §4 Functional — 90

FNC-001–090: Validation rules (impl ≥ signing, delivery ≥ start, submission ≤ signing); effective start logic; min date bindings; normalize to UTC; simulate start editing; date-only comparison; duration options; HTML contract; PNO-699/811/859 traceability.

---

## §5 Integration — 90

INT-001–090: Full template+spec contract; workflow start editing; valid dates normalize; invalid dates block save; datepicker appendTo body; effective impl start for min delivery; valid chronology; template IDs; normalize produces ISO; duration options; edit/save/cancel flow; PNO-699/811/859 requirements.
