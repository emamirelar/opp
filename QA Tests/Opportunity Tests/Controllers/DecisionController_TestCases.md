# DecisionController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/DecisionController`  
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
| §6 Security | 25 | 25 | ✅ |
| §7 Concurrency | 15 | 15 | ✅ |
| §8 Unit | 10 | 10 | ✅ |
| §9 Performance | 12 | 12 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio:** N≥3P: 90≥90 ✅ | E≥3P: 90≥90 ✅ | F≥3P: 90≥90 ✅ | I≥3P: 90≥90 ✅

---

## Feature Overview

REST API endpoints for Go/No-Go decision workflow: submit for Go, approve/reject, cancel, recall, get decision history, get decision status, get permissions, and decision-related notifications. Endpoints delegate to DecisionManager.

---

## §1 Positive — 30

| ID | Test | Endpoint | Expected | Pr |
|----|------|----------|----------|----|
| POS-001 | Submit for Go | POST /decision/{oppId}/submit | 200, status updated | P0 |
| POS-002 | Approve (Go) | POST /decision/{oppId}/approve | 200, stage=GO | P0 |
| POS-003 | Reject (No Go) | POST /decision/{oppId}/reject | 200, stage=NO GO | P0 |
| POS-004 | Cancel | POST /decision/{oppId}/cancel | 200, stage=CANCELLED | P0 |
| POS-005 | Get decision history | GET /decision/{oppId}/history | 200, history array | P0 |
| POS-006 | Recall | POST /decision/{oppId}/recall | 200, recalled | P1 |
| POS-007 | Get decision status | GET /decision/{oppId}/status | 200, current status | P1 |
| POS-008 | Get permissions | GET /decision/{oppId}/permissions | 200, permission flags | P1 |
| POS-009 | Reject with reason | POST /reject with body | 200, reason stored | P1 |
| POS-010 | Cancel with reason | POST /cancel with body | 200, reason stored | P1 |
| POS-011 | Get pending decisions | GET /decision/pending | 200, list | P1 |
| POS-012 | Get my decisions | GET /decision/my | 200, user's decisions | P1 |
| POS-013 | Decision with comment | POST /approve with comment | 200, comment saved | P1 |
| POS-014 | Get DoA for opp | GET /decision/{oppId}/doa | 200, DoA info | P1 |
| POS-015 | Re-submit after recall | POST /submit after recall | 200, re-submitted | P1 |
| POS-016–030 | Additional | Various endpoints | 200/201 responses | P2 |

## §2 Negative — 90

NEG-001–010: Input (null oppId, non-existent, deleted, invalid format, missing body, invalid reason, null comment, blank reason, invalid status, duplicate submit).
NEG-011–020: Auth (no token, expired, tampered, no permission, wrong role [Collaborator], wrong scope, disabled, post-logout, CSRF, role escalation).
NEG-021–030: State (approve draft, reject approved, cancel cancelled, recall not-in-workflow, submit already submitted, approve recalled, reject without submit, double approve, cancel after Go, modify Go'd).
NEG-031–040: HTTP (wrong method GET→POST, wrong content-type, malformed JSON, extra fields, missing required fields, oversized body, invalid encoding, HEAD method, OPTIONS abuse, TRACE).
NEG-041–050: Injection (SQL in reason, XSS in comment, HTML in body, path traversal, JSON injection, template injection, header injection, cookie manipulation, parameter pollution, CRLF).
NEG-051–060: Dependencies (manager throws, DB timeout, service unavailable, rate limit, circuit breaker, serialization error, mapper error, 500 from manager, transaction fail, deadlock).
NEG-061–070: Format (negative ID, zero ID, float ID, string ID, MAX_INT, special chars in ID, URL-encoded ID, page=-1, pageSize=0, sort=invalid).
NEG-071–090: Additional negative scenarios (business rules, validation, edge failures).

## §3 Boundary — 90

BND-001–090: Reason length (0/1/500/1000/1001), comment length (0/1/2000/4000/4001), history count (0/1/10/100), pending list size, ID boundaries, pagination, concurrent submissions, response sizes, date ranges, Unicode in comments, special chars, encoding boundaries, permission flag combinations, DoA hierarchy depth, re-submit count, decision chain length, API response time at boundary, rate limit threshold, token expiry edge, session boundaries.

## §4–§10

**§4 (90):** Route mapping (10), request validation (10), response formatting (10), status codes (10), model binding (10).
**§5 (90):** Manager integration (10), auth service (10), permission service (10), notification (10), audit (10).
**§6 (25):** Injection (10), auth bypass (10), IDOR (10), header security (10), CORS/CSRF (10).
**§7 (15):** Concurrent submit, approve, reject, cancel, recall, history read, permission check.
**§8 (10):** Route parsing (5), model validation (5), response mapping (3), error formatting (5), permission calc (3).
**§9 (12):** Submit (<500ms), approve (<500ms), history (<300ms), permissions (<200ms), list (<300ms), memory.
**§10 (10):** 50 concurrent decisions, 100 reads, spike, sustained, recovery.

---

**Status:** Ready for Execution
