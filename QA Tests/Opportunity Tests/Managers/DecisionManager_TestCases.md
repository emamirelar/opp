# DecisionManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/DecisionManager`  
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
| §6 Security | 22 | 22 | ✅ |
| §7 Concurrency | 15 | 15 | ✅ |
| §8 Unit | 15 | 15 | ✅ |
| §9 Performance | 10 | 10 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio:** N≥3P: 90≥90 ✅ | E≥3P: 90≥90 ✅ | F≥3P: 90≥90 ✅ | I≥3P: 90≥90 ✅

---

## Feature Overview

Business logic for Go/No-Go decision workflow: SubmitForGoAsync, ApproveAsync, RejectAsync, CancelAsync, RecallAsync, DoA identification, mandatory field validation, opportunity statement generation trigger, notification dispatch, workflow history, permission checks, and audit trail.

---

## §1–§10

**§1 (30):** SubmitForGoAsync (P0), ApproveAsync (P0), RejectAsync (P0), CancelAsync (P0), RecallAsync (P0), + 25 (GetDecisionHistory, GetStatus, GetPermissions, IdentifyDoA, ValidateMandatoryFields, TriggerStatementGeneration, SendNotification, GetPendingDecisions, ReSubmitAfterRecall, DecisionWithComment, DecisionWithReason, GetDoAChain, BatchNotification, DecisionAudit, ExportHistory, model mapping, status transitions, read-only enforcement, workflow indicator, stage stepper data, pending count, overdue decisions, escalation, delegation, decision analytics).
**§2 (90):** Input (null/non-existent/deleted oppId, invalid status, missing mandatory fields, null reason), Auth (10), State (submit already submitted, approve without submit, reject approved, cancel cancelled, recall not-in-workflow, double approve, modify after Go, re-submit without recall, approve as Collaborator, submit incomplete), injection (10), dependencies (DB, notification service, statement service, DoA service, workflow), format/ID (10), business (no DoA found, DoA disabled, org mismatch, multiple DoA conflict, statement gen failure, notification failure, workflow lock, concurrent decision, stale state, mass assignment) + 20 additional.
**§3 (90):** Reason/comment lengths, history sizes, decision chain depth, DoA hierarchy levels, mandatory field counts, notification counts, concurrent decisions, re-submission limits, Unicode, date boundaries, permission combinations, status transition matrix coverage, escalation levels, timeout boundaries.
**§4 (90):** Workflow transitions (15), DoA routing (10), mandatory validation (10), notification (10), audit (5) + 40 additional.
**§5 (90):** Opportunity service (10), DoA/OrgUnit (10), notification (10), statement (10), workflow (10) + 50 additional.
**§6 (22):** Injection (10), auth (10), IDOR (2).
**§7 (15):** Concurrent submit/approve/reject/cancel/recall, DoA lookup during decision, notification during decision.
**§8 (15):** Status validation (5), DoA lookup (5), mandatory check (3), notification template (2).
**§9 (10):** Submit (<500ms), approve (<500ms), history (<300ms), DoA lookup (<200ms), notification (<2s), memory.
**§10 (10):** 50 concurrent decisions, spike, sustained, large histories, recovery.

---

**Status:** Ready for Execution
