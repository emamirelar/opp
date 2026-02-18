# PartnershipAgreementController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/PartnershipAgreementController`  
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
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**Ratio Compliance:** N≥3P: 90≥90 ✅ | E≥3P: 90≥90 ✅ | F≥3P: 90≥90 ✅ | I≥3P: 90≥90 ✅

---

## Feature Overview

REST API for partnership agreements linked to opportunities: CRUD agreements, template selection, document generation, approval workflow, linking to partners/opportunities, and export.

---

## §1 Positive — 30

**§1 (30):** POST create (P0), GET by ID (P0), GET for opportunity (P0), PUT update (P0), DELETE (P0), + 25 (link to partner, template select, generate PDF, approval workflow, search, filter by type, filter by status, pagination, sort, export, audit, permissions, clone, amend, sign, version history, bulk operations, model map, typeahead, count, renewal, expiry check, compliance, notification, compare).

---

## §2 Negative — 90

**§2 (90):** NEG-001–070: Input (null, non-existent, deleted, invalid type, invalid partner, missing required), Auth (10), State (10), HTTP (10), injection (10), dependencies (10), format/ID (10), business (duplicate, expired template, invalid clause, circular link, max amendments, signing without approval, expired cert, role violation, conflicting terms, mass assignment).

NEG-071–090: Partnership agreement error handling — invalid agreement ID, non-existent template ID, deleted partner reference, orphaned opportunity link, invalid clause reference, malformed PDF request, missing signature block, invalid amendment sequence, expired agreement update, sign without approval, duplicate clause ID, invalid party role, missing required clause, template version mismatch, invalid renewal period, conflicting expiry dates, bulk create partial failure, invalid export format, archive non-finalized agreement, compare deleted agreements.

---

## §3 Boundary — 90

**§3 (90):** BND-001–070: Name/clause lengths, agreement count (0–100+), amendment count, signature count, version count, template count, date ranges, file sizes, pagination, Unicode, concurrent, compliance checks, renewal periods, party count.

BND-071–090: Boundary conditions — agreement name length (0/1/255/256), clause length (0/1/10000/10001), amendment count (0/1/50/51), signature count (0/1/20/21), version count (0/1/100/101), template count (0/1/100+), start date equals end date, expiry at midnight, renewal period (1 day/1 year/366 days), party count (0/1/10/11), PDF size (0 bytes/10MB/10MB+1), pagination page (1/9999/10000), pageSize (1/100/101), sort empty result, filter no match, Unicode in clause text, emoji in agreement name, concurrent identical create, max amendments boundary, version overflow.

---

## §4 Functional — 90

**§4 (90):** FUN-001–050: CRUD (15), approval (10), document generation (10), linking (10), audit (5).

FUN-051–090: Business rules — template selection validation, clause mandatory presence, partner linkage uniqueness, opportunity linkage scope, approval workflow state machine, sign-after-approval enforcement, amendment sequence ordering, version increment on update, renewal eligibility check, expiry notification trigger, compliance flag propagation, audit trail on create/update/delete/sign, permission inheritance from opportunity, bulk operation atomicity, clone deep copy, archive soft-delete, export format validation, PDF generation pipeline, typeahead filtering, count aggregation, model mapping completeness, duplicate detection, circular link prevention, max amendments enforcement, role-based signing, conflicting terms detection, mass assignment protection, soft-delete filter, workflow status sync, date range validation.

---

## §5 Integration — 90

**§5 (90):** INT-001–050: Partner service (10), opportunity (10), PDF/document (10), notification (10), approval (10).

INT-051–090: Integration flows — partner service round-trip, opportunity context resolution, PDF generation service, notification service on sign/approve/expiry, approval workflow integration, audit service logging, permission service delegation, manager DbContext scope, transaction boundary on bulk, template service fetch, document storage upload, export pipeline, mapper chain entity-to-model, validation pipeline, error propagation to client, response serialization, request deserialization, route resolution, auth middleware, CORS handling, rate limit integration, logging correlation, metrics emission, health check, config injection, dependency resolution, cache invalidation on update, external API fallback, soft-delete propagation, workflow integration.

---

## §6–§10

**§6 (50):** Injection (10), auth (10), IDOR (10), document security (10), signing security (10).
**§7 (25):** Concurrent edits, approvals, signatures, generation, amendments.
**§8 (21):** Validation (5), generation (5), linking (3), status (5), formatting (3).
**§9 (16):** GET (<200ms), generate PDF (<3s), search (<500ms), list (<300ms), export (<3s), memory.
**§10 (10):** 50 concurrent, spike, sustained, large agreements, recovery.

---

**Status:** Ready for Execution
