# Go/No-Go Decision PRD — Test Cases

**Component:** Go/No-Go Decision per PRD Requirements  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio  
**Note:** This covers PRD-specific requirements. See `PNO-969_GoDecision_TestCases.md` for the authoritative JIRA-based test cases.

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:**
- N ≥ 3P: 90 ≥ 90 → ✅ PASS
- E ≥ 3P: 90 ≥ 90 → ✅ PASS
- F ≥ 3P: 90 ≥ 90 → ✅ PASS
- I ≥ 3P: 90 ≥ 90 → ✅ PASS

---

## Feature Overview

PRD-specific test scenarios for the Go/No-Go decision workflow. Covers: mandatory field validation before Go submission, opportunity statement auto-generation, DoA (Decision of Authority) identification and routing, email notifications for all workflow events, read-only enforcement during workflow, UI indicators ("In Workflow"), workflow history tracking, stage stepper visualization, recall functionality, and role-specific permissions.

---

## §1 Positive — 30

POS-001–005 (P0): Mandatory fields complete → Submit Go enabled, Opportunity Statement generated on Go, DoA correctly identified, email sent to DoA on submission, read-only enforced during workflow.
POS-006–030 (P1/P2): "In Workflow" indicator, workflow history entry, stage stepper updates, recall by OM, recall resets read-only, Go decision by DoA, No Go decision by DoA, Cancel by OM, email on Go decision, email on No Go, email on Cancel, email on Recall, status Active after Go, status Closed after No Go, status Closed after Cancel, stage history, multiple DoA levels, re-submit after recall, mandatory field re-validation, statement regeneration, DoA change notification, Collaborator view-only, OM permissions, field validation errors shown, partial completion warning.

---

## §2 Negative — 90

NEG-001–010: Mandatory field missing (each required field), submit with incomplete.
NEG-011–020: Auth (Collaborator submit, wrong OM, non-DoA approve, expired, tampered, disabled, etc.).
NEG-021–030: State (Go already Go'd, recall already recalled, cancel cancelled, approve draft, submit read-only, etc.).
NEG-031–040: SQL/XSS/injection in comments/rationale.
NEG-041–050: DoA errors (no DoA found, multiple DoA conflict, DoA disabled, DoA changed, org mismatch).
NEG-051–060: Notification errors (email service down, invalid email, template missing, rate limit, bounce).
NEG-061–070: Statement errors (generation fail, too long, template corrupt, missing data, concurrent generation, timeout, service unavailable, format error, encoding error, mass assignment).
NEG-071–080: Invalid IDs (non-existent opportunity, deleted opportunity, wrong entity type, malformed GUID, negative ID, zero ID, overflow ID, stale reference).
NEG-081–090: Permission edge cases (revoked mid-session, role downgrade during workflow, missing DoA permission, OM removed from opportunity, Collaborator attempts approve, expired session on submit, cross-tenant access, orphaned workflow, invalid token on recall, concurrent permission change).

---

## §3 Boundary — 90

BND-001–020: Mandatory fields (exactly all filled, all-1, optional fields empty), comment lengths (min, max, exactly at limit).
BND-021–040: Rationale lengths (min, max, exactly at limit), DoA hierarchy depth (1, max, empty).
BND-041–060: Notification count (0, 1, max recipients), concurrent submissions (1, 2, max), stage transition boundaries.
BND-061–070: Recall window (immediate, at limit, expired), statement length limits (min, max, exactly at limit).
BND-071–080: Unicode in all fields (ASCII, extended Unicode, emoji, RTL, mixed scripts).
BND-081–090: Date boundaries (epoch, max date, DST transition, timezone edge), workflow duration (min, max), re-submission limits (0, 1, max), empty vs null handling, whitespace-only fields, boundary stage combinations.

---

## §4 Functional — 90

**Mandatory validation (25):** All required fields present, each field individually validated, combined validation, submit blocked when incomplete, submit enabled when complete, partial completion warning, re-validation on recall, field-level error messages, cross-field validation, batch validation, validation order, async validation, sync validation, validation on load, validation on blur, validation on submit attempt, optional vs required distinction, conditional required fields, nested object validation, array validation, date range validation, numeric range validation, string format validation, custom business rules, validation summary display.

**Statement generation (20):** Auto-generation on Go, template application, data mapping, regeneration on recall, concurrent generation handling, timeout handling, error recovery, format validation, encoding handling, length truncation, placeholder substitution, missing data fallback, version compatibility, audit of generation, statement attachment, statement preview, statement history, multi-language support, special character handling, statement versioning.

**DoA routing (20):** Correct DoA identification, hierarchy traversal, org match, level-based routing, fallback when no DoA, multiple DoA conflict resolution, DoA change notification, disabled DoA handling, delegation chain, escalation path, DoA lookup performance, DoA cache invalidation, cross-region DoA, DoA permission check, DoA assignment audit, DoA override scenarios, interim DoA, acting DoA, DoA delegation, DoA expiry.

**Notification (15):** Email on submit, email on Go, email on No Go, email on Cancel, email on Recall, template selection, recipient resolution, batch sending, retry on failure, bounce handling, rate limit handling, audit trail, notification content validation, attachment inclusion, notification preferences.

**Read-only enforcement (10):** Fields locked during workflow, unlock on recall, unlock on cancel, unlock on decision, partial lock (some fields editable), Collaborator always read-only, OM edit before submit, DoA read-only except decision, audit of edit attempts, concurrent edit prevention.

---

## §5 Integration — 90

**Workflow service (18):** Submit flow, Go decision flow, No Go decision flow, Cancel flow, Recall flow, status transitions, stage persistence, history recording, concurrent workflow handling, workflow validation, workflow audit, workflow timeout, workflow recovery, workflow cancellation propagation, workflow state sync, workflow event emission, workflow permission check, workflow rollback.

**Email service (18):** Send on submit, send on Go, send on No Go, send on Cancel, send on Recall, template rendering, recipient resolution, SMTP integration, retry logic, bounce handling, queue integration, batch processing, rate limiting, logging, error propagation, async delivery, sync delivery fallback, email audit.

**DoA service (18):** Lookup by opportunity, hierarchy resolution, org matching, level determination, conflict resolution, cache integration, fallback logic, delegation chain, permission integration, audit logging, change notification, disabled user handling, interim assignment, cross-service call, timeout handling, retry logic, stale data handling, DoA persistence.

**Statement service (18):** Generation trigger, template fetch, data aggregation, format output, storage integration, version tracking, concurrent generation, error handling, retry logic, cache invalidation, audit trail, attachment creation, preview generation, history retrieval, multi-format support, encoding handling, length validation, placeholder resolution.

**Opportunity service (18):** Status update on Go, status update on No Go, status update on Cancel, field locking, field unlock on recall, history integration, audit integration, search integration, filter by workflow status, notification trigger, DoA association, statement attachment, concurrent update handling, validation integration, permission check, event emission, rollback on failure, cross-entity sync.

---

## §6–§10

**§6 Security (50):** Injection (10), access control (10), IDOR (10), workflow security (10), notification security (10).
**§7 Concurrency (25):** Concurrent submissions, approvals, recalls, notifications, statement generation.
**§8 Unit (21):** Validation (5), DoA lookup (5), statement format (3), status calculation (5), notification template (3).
**§9 Performance (16):** Submit (<500ms), approve (<500ms), statement gen (<3s), email (<2s), DoA lookup (<200ms), list (<300ms), memory.
**§10 Load (10):** 50 concurrent workflows, spike, sustained, recovery.

---

**Status:** Ready for Execution
