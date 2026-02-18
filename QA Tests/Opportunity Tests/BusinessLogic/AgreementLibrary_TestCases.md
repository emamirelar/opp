# Agreement Library — Test Cases

**Component:** Opportunity Agreement Library  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive (P) | 30 | 30 | ✅ |
| §2 Negative (N) | 90 | 90 | ✅ |
| §3 Boundary (E) | 90 | 90 | ✅ |
| §4 Functional (F) | 90 | 90 | ✅ |
| §5 Integration (I) | 90 | 90 | ✅ |
| §7 Concurrency (CON) | 25 | 25 | ✅ |
| §8 Unit (UNT) | 21 | 21 | ✅ |
| §9 Performance (PRF) | 16 | 16 | ✅ |
| §10 Load (LDT) | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**Ratio Checks:**
- N≥3P: 90≥90 ✅ PASS
- E≥3P: 90≥90 ✅ PASS
- F≥3P: 90≥90 ✅ PASS
- I≥3P: 90≥90 ✅ PASS

---

## Feature Overview

Partnership agreement templates, versioning, linking to opportunities, clause management, approval workflows, document generation, PDF export, digital signatures, compliance tracking, agreement types (MOU, LOA, Framework), status lifecycle, amendment tracking, expiry notifications, and audit trail.

---

## §1 Positive Tests — 30 tests

| ID | Test Name | Steps | Expected | Pr |
|----|-----------|-------|----------|----|
| POS-001 | Create agreement from template | Select template → CreateAgreement | Agreement created with template fields | P0 |
| POS-002 | Link agreement to opportunity | LinkAgreement(oppId, agrId) | Linked, visible on opp | P0 |
| POS-003 | Generate PDF from agreement | GeneratePDF(agrId) | Valid PDF with all clauses | P0 |
| POS-004 | Update agreement clauses | UpdateClauses(agrId, clauses) | Clauses saved, version incremented | P0 |
| POS-005 | Submit agreement for approval | SubmitForApproval(agrId) | Status=PendingApproval | P0 |
| POS-006 | Approve agreement | Approve(agrId) | Status=Approved, audit logged | P1 |
| POS-007 | Reject agreement | Reject(agrId, reason) | Status=Rejected, reason stored | P1 |
| POS-008 | Create MOU type | Create(type=MOU) | MOU agreement created | P1 |
| POS-009 | Create LOA type | Create(type=LOA) | LOA agreement created | P1 |
| POS-010 | Create Framework type | Create(type=Framework) | Framework created | P1 |
| POS-011 | Add amendment | AddAmendment(agrId, text) | Amendment linked | P1 |
| POS-012 | Version history | GetVersions(agrId) | All versions listed | P1 |
| POS-013 | Search agreements | Search("partnership") | Matching found | P1 |
| POS-014 | Filter by type | Filter(type=MOU) | Only MOUs | P1 |
| POS-015 | Filter by status | Filter(status=Active) | Only active | P1 |
| POS-016 | Paginate agreements | GetPaginated(page=1) | Paginated results | P1 |
| POS-017 | Sort by date | Sort(date, desc) | Newest first | P2 |
| POS-018 | Sort by name | Sort(name, asc) | Alphabetical | P2 |
| POS-019 | Get agreement detail | GetById(agrId) | All fields returned | P1 |
| POS-020 | Soft delete | Delete(agrId) | IsDeleted=true | P1 |
| POS-021 | Expiry notification | Agreement near expiry | Notification sent | P1 |
| POS-022 | Clone agreement | Clone(agrId) | New agreement, same content | P1 |
| POS-023 | Add digital signature | Sign(agrId, userId) | Signature recorded | P1 |
| POS-024 | Multiple signatories | Sign by 3 users | All signatures | P1 |
| POS-025 | Agreement timeline | GetTimeline(agrId) | Events in order | P2 |
| POS-026 | Export to Word | ExportWord(agrId) | Valid .docx | P2 |
| POS-027 | Audit trail | GetAudit(agrId) | Full history | P2 |
| POS-028 | Map to model | mapper.Map | All fields | P2 |
| POS-029 | Get templates list | GetTemplates | Available templates | P2 |
| POS-030 | Template preview | PreviewTemplate(tplId) | Preview rendered | P2 |

---

## §2 Negative Tests — 90 tests

NEG-001–010: Input validation (null name, null template, non-existent oppId, deleted opp, invalid type, null clauses, blank name, duplicate name, missing required clause, invalid date range).

NEG-011–020: Auth (no auth, no create, no approve, no delete, wrong scope, expired token, tampered JWT, disabled, post-logout, escalation).

NEG-021–030: State (approve draft, reject approved, amend rejected, sign unapproved, delete signed, modify locked, submit incomplete, expire active, renew cancelled, clone deleted).

NEG-031–040: SQL/XSS (SQL name, SQL search, XSS clause, XSS name, path traversal, HTML injection, JSON injection, template injection, LDAP, command).

NEG-041–050: Dependencies (DB timeout, connection lost, PDF service down, email service down, storage failure, constraint violation, mapper missing, concurrent lock, pool exhausted, service unavailable).

NEG-051–060: Format (ID negative, ID zero, ID float, ID string, page=0, pageSize=-1, pageSize>1000, invalid sort, empty search, regex chars).

NEG-061–070: Business (link to wrong entity type, exceed max amendments, invalid signature, expired certificate, invalid template format, circular reference, max file size, invalid PDF, empty export, mass assignment).

NEG-071–080: Extended validation (null oppId on link, negative page number, null clause text, whitespace-only name, invalid MOU subtype, missing signatory role, orphaned amendment, template not found, agreement already linked, duplicate amendment version).

NEG-081–090: Extended failures (PDF generation timeout, Word export failure, signature service unreachable, template engine error, search index corruption, audit write failure, notification delivery failure, version conflict on save, soft-deleted template reference, cross-tenant access attempt).

---

## §3 Boundary Tests — 90 tests

BND-001–010: String lengths (name 1/200/201, clause 1/10000/10001, description 0/4000/4001, template name 1).

BND-011–020: Counts (0/1/10/100/1000 agreements, 0/1/50 clauses per agreement, 0/1/10 amendments, 0/1/5 signatures).

BND-021–030: Pagination (page 1, last, pageSize 1/1000, exactly page size, +1, total items).

BND-031–040: Dates (today, past, far future, leap year, midnight, year boundary, expiry=today, expiry=tomorrow, expiry yesterday, duration 1 day).

BND-041–050: Unicode (Arabic, Chinese, Cyrillic, French, emoji, mixed script, RTL, long Unicode, special chars, apostrophe).

BND-051–060: File (PDF 1KB, PDF 10MB, PDF 50MB, Word 1KB, template minimal, template maximal, zero attachments, max attachments, image in template, formula in template).

BND-061–070: Version (v1, v2, v100, amendment v1, amendment v50, concurrent version, rollback, restore, export specific version, compare versions).

BND-071–080: Extended boundaries (name exactly 199 chars, clause exactly 9999 chars, pageSize=999, page=MAX_INT-1, expiry 1 second before midnight, amendment count at limit, signature count at limit, clause count at limit, description 3999 chars, template name 255 chars).

BND-081–090: Edge values (empty clause array, single-char search, zero-duration agreement, agreement with 0 amendments, agreement with max amendments, first page empty result, last page partial, sort by null field, filter with empty criteria, bulk export with 0 agreements).

---

## §4 Functional Tests — 90 tests

**Template & Clause (20):** Template selection, template validation, clause add, clause remove, clause reorder, clause replace, clause merge, clause split, template variable substitution, clause placeholder resolution, template version compatibility, clause type validation, required clause presence, optional clause handling, clause formatting preservation, template inheritance, clause conflict detection, template fallback, clause default values, clause conditional display.

**Approval & Workflow (20):** Submit for approval, approve transition, reject transition, approval delegation, multi-level approval, approval timeout, approval reminder, reject reason required, approval audit, approval rollback, draft→pending, pending→approved, pending→rejected, approved→amended, amendment approval, bulk approval, approval notification, approval expiry, approval override, approval chain validation.

**PDF & Export (15):** PDF generation, PDF layout, PDF clause order, PDF signature placement, Word export, Word formatting, export encoding, export filename, export metadata, multi-format export, export with amendments, export version selection, export watermark, export pagination, export error handling.

**Signature & Compliance (15):** Single signature, multi-signature, signature order, signature verification, signature timestamp, compliance check, compliance rules, compliance report, compliance gap, compliance override, signature expiry, certificate validation, compliance audit, compliance notification, compliance remediation.

**Version & Audit (20):** Version create, version list, version diff, version rollback, version restore, amendment tracking, amendment link, amendment version, audit create, audit read, audit filter, audit export, version compare, amendment history, audit trail integrity, version conflict, amendment cascade, audit retention, version purge, audit search.

---

## §5 Integration Tests — 90 tests

**CRUD & Opportunity (20):** Create agreement, read agreement, update agreement, soft delete, restore, link to opportunity, unlink, link validation, opportunity agreement count, opportunity agreement list, bulk link, bulk unlink, create from opportunity context, update from opportunity, delete cascade check, link to deleted opportunity (negative), link to non-existent opportunity (negative), duplicate link (negative), cross-opportunity link (negative), agreement orphan handling.

**Partner & Document (20):** Partner association, partner validation, partner agreement list, document attach, document detach, document storage, document retrieval, document version, document metadata, document type validation, storage service integration, storage quota, document encryption, document access control, partner permission check, multi-partner agreement, partner change, document migration, document cleanup, document audit.

**Services (25):** PDF service call, PDF service timeout, PDF service error, email notification send, email template, email delivery, signature service sign, signature service verify, signature service error, template engine render, template engine cache, template engine error, search index update, search index query, search index rebuild, export service invoke, export service format, export service batch, notification service trigger, notification service retry, audit service write, audit service query, compliance service check, compliance service report, service health check.

**Data & Concurrency (25):** DbContext save, DbContext transaction, DbContext rollback, mapper entity→model, mapper model→entity, mapper collection, soft delete filter, IsDeleted query, concurrent create, concurrent update, concurrent delete, optimistic concurrency, connection pool, transaction isolation, foreign key cascade, unique constraint, index usage, query performance, N+1 avoidance, batch operations, cache invalidation, event publishing, event handling, saga compensation, eventual consistency.

---

## §7 Concurrency — 25 | §8 Unit — 21 | §9 Performance — 16 | §10 Load — 10

**§7:** Concurrent approval, sign, edit, version, delete, clone, export, amend, search, PDF generation (25 scenarios).

**§8:** Validation (5), formatting (3), calculations (5), state (5), collections (3).

**§9:** Create (<200ms), PDF gen (<2s), search (<500ms), list 100 (<300ms), export (<5s), concurrent 10 (<1s), memory (6 tests).

**§10:** 50 concurrent ops (30min), 100 reads, spike, sustained, recovery (10 tests).

---

## Traceability Matrix

| Rule | Tests |
|------|-------|
| Agreement types | POS-008–010 |
| Approval workflow | POS-005–007, NEG-021–022 |
| PDF generation | POS-003, BND-051–054 |
| Signatures | POS-023–024, NEG-031 |
| Expiry | POS-021, BND-037–039 |

**Status:** Ready for Execution
