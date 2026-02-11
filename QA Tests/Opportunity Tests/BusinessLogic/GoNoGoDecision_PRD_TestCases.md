# Go/No-Go Decision PRD — Test Cases

**Component:** Go/No-Go Decision per PRD Requirements  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio  
**Note:** This covers PRD-specific requirements. See `PNO-969_GoDecision_TestCases.md` for the authoritative JIRA-based test cases.

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 35 | 30-50 | ✅ |
| §2 Negative | 70 | 70 | ✅ |
| §3 Boundary | 70 | 70 | ✅ |
| §4 Functional | 50 | 50 | ✅ |
| §5 Integration | 50 | 50 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **397** | **≥347** | ✅ |

**3:1 Ratio:** (70+70)=140 ≥ 3×35=105 → ✅ PASS

---

## Feature Overview

PRD-specific test scenarios for the Go/No-Go decision workflow. Covers: mandatory field validation before Go submission, opportunity statement auto-generation, DoA (Decision of Authority) identification and routing, email notifications for all workflow events, read-only enforcement during workflow, UI indicators ("In Workflow"), workflow history tracking, stage stepper visualization, recall functionality, and role-specific permissions.

---

## §1 Positive — 35

POS-001–005 (P0): Mandatory fields complete → Submit Go enabled, Opportunity Statement generated on Go, DoA correctly identified, email sent to DoA on submission, read-only enforced during workflow.
POS-006–035 (P1/P2): "In Workflow" indicator, workflow history entry, stage stepper updates, recall by OM, recall resets read-only, Go decision by DoA, No Go decision by DoA, Cancel by OM, email on Go decision, email on No Go, email on Cancel, email on Recall, status Active after Go, status Closed after No Go, status Closed after Cancel, stage history, multiple DoA levels, re-submit after recall, mandatory field re-validation, statement regeneration, DoA change notification, Collaborator view-only, OM permissions, field validation errors shown, partial completion warning, batch notification, audit per action, model mapping, search by workflow status, filter by stage.

## §2 Negative — 70

NEG-001–010: Mandatory field missing (each required field), submit with incomplete.
NEG-011–020: Auth (Collaborator submit, wrong OM, non-DoA approve, expired, tampered, disabled, etc.).
NEG-021–030: State (Go already Go'd, recall already recalled, cancel cancelled, approve draft, submit read-only, etc.).
NEG-031–040: SQL/XSS/injection in comments/rationale.
NEG-041–050: DoA errors (no DoA found, multiple DoA conflict, DoA disabled, DoA changed, org mismatch).
NEG-051–060: Notification errors (email service down, invalid email, template missing, rate limit, bounce).
NEG-061–070: Statement errors (generation fail, too long, template corrupt, missing data, concurrent generation, timeout, service unavailable, format error, encoding error, mass assignment).

## §3 Boundary — 70

BND-001–070: Mandatory fields (exactly all filled, all-1, optional fields empty), comment lengths, rationale lengths, DoA hierarchy depth, notification count, concurrent submissions, stage transition boundaries, recall window, statement length limits, Unicode in all fields, date boundaries, workflow duration, re-submission limits.

## §4–§10

**§4 (50):** Mandatory validation (15), statement generation (10), DoA routing (10), notification (10), read-only enforcement (5).
**§5 (50):** Workflow service (10), email service (10), DoA service (10), statement service (10), opportunity service (10).
**§6 (50):** Injection (10), access control (10), IDOR (10), workflow security (10), notification security (10).
**§7 (25):** Concurrent submissions, approvals, recalls, notifications, statement generation.
**§8 (21):** Validation (5), DoA lookup (5), statement format (3), status calculation (5), notification template (3).
**§9 (16):** Submit (<500ms), approve (<500ms), statement gen (<3s), email (<2s), DoA lookup (<200ms), list (<300ms), memory.
**§10 (10):** 50 concurrent workflows, spike, sustained, recovery.

---

**Status:** Ready for Execution
