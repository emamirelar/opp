# DataImportFixes Business Logic — Test Cases

**Component:** `UNOPS.PAO.Business/Managers` (Data Import/Migration Utilities)  
**Created:** 2026-02-04  
**Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio (per `comprehensive-test-strategy.mdc`)

---

## Compliance Summary

| Category | File/Section | Count | Minimum Required | Status |
|----------|-------------|-------|-----------------|--------|
| Positive Tests | §1 | 30 | 30-50 | ✅ |
| Negative Tests | §2 | 90 | Max(50, 3×30)=90 | ✅ |
| Boundary Tests | §3 | 90 | Max(50, 3×30)=90 | ✅ |
| Functional Tests | §4 | 90 | ≥90 | ✅ |
| Integration Tests | §5 | 90 | ≥90 | ✅ |
| Security Tests | §6 | 50 | ≥50 | ✅ |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

Data import and migration utilities for bulk partner/contact/interaction/opportunity data. Key features: CSV/Excel file import, field mapping, data validation (pre-import), duplicate detection, conflict resolution (skip/overwrite/merge), dry-run mode, batch processing, error reporting, rollback on failure, progress tracking, import history/audit, scheduled imports, data transformation rules, encoding handling (UTF-8/Latin-1), date format normalization, and partial import (valid rows only).

---

## §1 Positive Tests — 35 tests

### P0 Detailed (5)

#### POS-001: Import Partners from Valid CSV
**Priority:** P0 | **Precondition:** CSV with 10 valid partner rows, correct headers.
**Steps:** ImportPartnersAsync(csvFile, mapping)
**Expected:** 10 partners created, all fields mapped correctly, import history logged

#### POS-002: Import with Dry-Run Mode
**Priority:** P0 | **Precondition:** CSV with 10 rows (8 valid, 2 invalid).
**Steps:** ImportAsync(csvFile, dryRun=true)
**Expected:** Validation report returned (8 pass, 2 fail), NO data written to DB

#### POS-003: Import with Duplicate Skip
**Priority:** P0 | **Precondition:** CSV has 5 rows, 2 already exist in DB.
**Steps:** ImportAsync(csvFile, conflictResolution=Skip)
**Expected:** 3 new records created, 2 skipped, report shows counts

#### POS-004: Import Contacts from Excel
**Priority:** P0 | **Precondition:** .xlsx file with 20 contact rows, correct sheet.
**Steps:** ImportContactsAsync(xlsxFile, mapping)
**Expected:** 20 contacts created, linked to correct partners

#### POS-005: Rollback on Critical Error
**Priority:** P0 | **Precondition:** CSV with 100 rows, DB constraint violation at row 50.
**Steps:** ImportAsync(csvFile, rollbackOnError=true)
**Expected:** All 100 rows rolled back, no partial data, error report generated

### P1/P2 Tabular (30)

| ID | Test Name | Steps | Expected | Pr |
|----|-----------|-------|----------|----|
| POS-006 | Import with overwrite | conflictResolution=Overwrite | Existing updated | P1 |
| POS-007 | Import with merge | conflictResolution=Merge | Fields merged | P1 |
| POS-008 | Partial import (valid only) | rollbackOnError=false | Valid rows imported, invalid skipped | P1 |
| POS-009 | Field mapping custom | Map CSV "Organisation" → DB "Name" | Correct mapping | P1 |
| POS-010 | Date format normalization | "02/11/2026" → ISO | Correct date | P1 |
| POS-011 | Encoding UTF-8 | UTF-8 CSV | All chars correct | P1 |
| POS-012 | Encoding Latin-1 | Latin-1 CSV | Converted correctly | P1 |
| POS-013 | Import opportunities | CSV with opportunities | Created correctly | P1 |
| POS-014 | Import interactions | CSV with interactions | Created correctly | P1 |
| POS-015 | Import with categories | Map category column | Categories assigned | P1 |
| POS-016 | Progress tracking | Import 100 rows | Progress updates | P1 |
| POS-017 | Import history log | Complete import | History entry created | P1 |
| POS-018 | Import report | Complete import | Summary report | P1 |
| POS-019 | Trim whitespace | " Partner " → "Partner" | Trimmed | P1 |
| POS-020 | Transform rules | Upper case country codes | Transformed | P1 |
| POS-021 | Multiple sheets (Excel) | Sheet per entity | Correct mapping | P1 |
| POS-022 | Header row detection | First row = headers | Auto-detected | P1 |
| POS-023 | Auto-map by header | Headers match DB columns | Auto-mapped | P2 |
| POS-024 | Skip blank rows | CSV with empty rows | Skipped | P1 |
| POS-025 | Get import templates | GetTemplateAsync(Partner) | Template CSV | P2 |
| POS-026 | Get import history | GetImportHistoryAsync | Past imports | P2 |
| POS-027 | Scheduled import | Schedule weekly | Executes on schedule | P2 |
| POS-028 | Import status check | GetImportStatusAsync(jobId) | Running/Complete | P1 |
| POS-029 | Cancel running import | CancelImportAsync(jobId) | Cancelled, partial data handled | P1 |
| POS-030 | Duplicate detection | Same email/name | Flagged | P1 |

---

## §2 Negative Tests — 90 tests

| ID | Category | Scenario | Expected | Pr |
|----|----------|---------|----------|----|
| NEG-001 | Input | Null file | BusinessException | P0 |
| NEG-002 | Input | Empty file | BusinessException | P0 |
| NEG-003 | Input | Wrong file type (.exe) | BusinessException | P0 |
| NEG-004 | Input | Corrupt CSV | ParseException | P0 |
| NEG-005 | Input | Corrupt Excel | ParseException | P0 |
| NEG-006 | Input | No header row | BusinessException | P0 |
| NEG-007 | Input | Missing required columns | BusinessException: field X missing | P0 |
| NEG-008 | Input | Null mapping | BusinessException | P0 |
| NEG-009 | Input | Invalid mapping (unknown col) | BusinessException | P0 |
| NEG-010 | Input | Null entity type | BusinessException | P0 |
| NEG-011 | Data | All rows invalid | 0 imported, error report | P0 |
| NEG-012 | Data | Invalid email format | Row rejected | P0 |
| NEG-013 | Data | Invalid date format | Row rejected | P0 |
| NEG-014 | Data | Missing required field | Row rejected | P0 |
| NEG-015 | Data | Null required field | Row rejected | P0 |
| NEG-016 | Data | Duplicate within file | Flagged/merged | P0 |
| NEG-017 | Data | FK violation (bad partnerId) | Row rejected | P0 |
| NEG-018 | Data | Numeric in text field | Accepted | P2 |
| NEG-019 | Data | Text in numeric field | Row rejected | P1 |
| NEG-020 | Data | Negative numeric | Depends on field | P1 |
| NEG-021 | Auth | No auth | Unauthorized | P0 |
| NEG-022 | Auth | No import perm | Unauthorized | P0 |
| NEG-023 | Auth | Expired token | Unauthorized | P0 |
| NEG-024 | Auth | Tampered JWT | Unauthorized | P0 |
| NEG-025 | Auth | Scoped user, wrong scope | Unauthorized | P0 |
| NEG-026 | Auth | Disabled account | Unauthorized | P1 |
| NEG-027 | Auth | Post-logout | Unauthorized | P1 |
| NEG-028 | Auth | Role escalation | Ignored | P0 |
| NEG-029 | Auth | Non-admin schedule | Unauthorized | P1 |
| NEG-030 | Auth | Cancel without perm | Unauthorized | P1 |
| NEG-031 | SQL | SQL in CSV data | Parameterized | P0 |
| NEG-032 | SQL | SQL in header | Sanitized | P0 |
| NEG-033 | SQL | SQL in mapping | Parameterized | P0 |
| NEG-034 | XSS | XSS in CSV data | Sanitized | P0 |
| NEG-035 | XSS | XSS in file name | Sanitized | P0 |
| NEG-036 | XSS | Script in CSV cell | Not executed | P0 |
| NEG-037 | Path | Path traversal in name | Sanitized | P0 |
| NEG-038 | Format | CSV with inconsistent columns | Error per row | P1 |
| NEG-039 | Format | CSV with extra columns | Ignored or mapped | P1 |
| NEG-040 | Format | Semicolon delimiter (European) | Auto-detect or config | P1 |
| NEG-041 | Format | Tab delimiter | Auto-detect or config | P1 |
| NEG-042 | Format | Quoted fields with commas | Correctly parsed | P1 |
| NEG-043 | Format | Quoted fields with newlines | Correctly parsed | P1 |
| NEG-044 | Format | BOM (byte order mark) | Handled | P1 |
| NEG-045 | Encoding | Invalid encoding | Error or fallback | P1 |
| NEG-046 | Encoding | Mixed encoding | Best effort | P1 |
| NEG-047 | Size | File > 100MB | Rejected or streamed | P1 |
| NEG-048 | Size | >100,000 rows | Processed in batches | P1 |
| NEG-049 | Dep | DB timeout | Rollback | P1 |
| NEG-050 | Dep | DB connection lost | Rollback | P1 |
| NEG-051 | Dep | Storage full | Error | P1 |
| NEG-052 | Dep | Constraint violation | Row or batch error | P1 |
| NEG-053 | State | Import while another running | Queued or rejected | P1 |
| NEG-054 | State | Cancel completed import | No-op | P2 |
| NEG-055 | State | Cancel non-existent | Error | P2 |
| NEG-056 | State | Get status non-existent | Not found | P2 |
| NEG-057 | Transform | Invalid transform rule | Error at validation | P1 |
| NEG-058 | Transform | Transform produces null | Row rejected | P1 |
| NEG-059 | Schedule | Invalid cron expression | Error | P1 |
| NEG-060 | Schedule | Schedule in past | Error | P2 |
| NEG-061 | Dup | Duplicate detection false positive | Review needed | P1 |
| NEG-062 | Dup | Same file imported twice | Duplicates handled per policy | P1 |
| NEG-063 | Virus | Malicious CSV (formula injection) | =CMD blocked | P0 |
| NEG-064 | Virus | Virus in Excel macros | Blocked | P0 |
| NEG-065 | Mass | Mass assign IsDeleted | Blocked | P1 |
| NEG-066 | Mass | Mass assign Id | Blocked | P1 |
| NEG-067 | Mass | Mass assign CreatedBy | Blocked | P1 |
| NEG-068 | Report | Error report for empty import | Empty report | P2 |
| NEG-069 | Report | Error report > max size | Truncated | P2 |
| NEG-070 | Rollback | Rollback after 50% | All rolled back | P1 |
| NEG-071 | Input | Null conflict resolution | BusinessException | P1 |
| NEG-072 | Input | Invalid batch size | BusinessException | P1 |
| NEG-073 | Input | Negative row limit | BusinessException | P1 |
| NEG-074 | Data | Null FK in required column | Row rejected | P1 |
| NEG-075 | Data | Empty string in required | Row rejected | P1 |
| NEG-076 | Auth | Concurrent import same user | Queued or rejected | P1 |
| NEG-077 | Format | Malformed Excel sheet name | Error | P2 |
| NEG-078 | Format | CSV with null bytes | Handled or error | P1 |
| NEG-079 | Dep | File locked by another process | Error | P1 |
| NEG-080 | State | Import after entity deleted | Error | P1 |
| NEG-081 | Transform | Transform throws | Row rejected | P1 |
| NEG-082 | Map | Map to deleted lookup | Row rejected | P1 |
| NEG-083 | Schedule | Schedule with invalid timezone | Error | P2 |
| NEG-084 | Report | Report generation timeout | Truncated or error | P2 |
| NEG-085 | Mass | Mass assign DeletedBy | Blocked | P1 |
| NEG-086 | Mass | Mass assign DeletedDate | Blocked | P1 |
| NEG-087 | Input | File path too long | Error | P2 |
| NEG-088 | Input | Invalid temp directory | Error | P1 |
| NEG-089 | Dep | Disk full during import | Rollback | P1 |
| NEG-090 | State | Cancel non-running import | No-op | P2 |

---

## §3 Boundary Tests — 90 tests

| ID | Category | Scenario | Expected | Pr |
|----|----------|---------|----------|----|
| BND-001 | Rows | 1 row | Imported | P1 |
| BND-002 | Rows | 100 rows | Imported <5s | P1 |
| BND-003 | Rows | 1,000 rows | Imported <30s | P1 |
| BND-004 | Rows | 10,000 rows | Imported <5min | P1 |
| BND-005 | Rows | 100,000 rows | Batched, imported | P1 |
| BND-006 | Rows | 0 data rows (header only) | No-op, report | P1 |
| BND-007 | Cols | 1 column | Minimal import | P1 |
| BND-008 | Cols | 50 columns | All mapped | P1 |
| BND-009 | Cols | 100 columns | Handled | P2 |
| BND-010 | Cols | 0 columns | Error | P1 |
| BND-011 | Size | 1 KB file | Fast import | P2 |
| BND-012 | Size | 1 MB file | Acceptable | P1 |
| BND-013 | Size | 10 MB file | Acceptable | P1 |
| BND-014 | Size | 50 MB file | Acceptable | P1 |
| BND-015 | Size | 100 MB file | Max or rejected | P1 |
| BND-016 | Size | 101 MB file | Rejected | P1 |
| BND-017 | Field | Name 1 char | Accepted | P1 |
| BND-018 | Field | Name 200 chars | Accepted | P1 |
| BND-019 | Field | Name 201 chars | Rejected | P1 |
| BND-020 | Field | Email valid (a@b.c) | Accepted | P1 |
| BND-021 | Field | Email max length | Accepted | P1 |
| BND-022 | Field | Email 1 char too long | Rejected | P1 |
| BND-023 | Field | Phone 1 digit | Accepted/rejected | P2 |
| BND-024 | Field | Phone 20 digits | Accepted | P2 |
| BND-025 | Field | Description 0 | Accepted (nullable) | P2 |
| BND-026 | Field | Description 4000 | Accepted | P2 |
| BND-027 | Field | Description 4001 | Rejected | P2 |
| BND-028 | Date | "2026-02-11" (ISO) | Parsed | P1 |
| BND-029 | Date | "02/11/2026" (US) | Parsed | P1 |
| BND-030 | Date | "11/02/2026" (EU) | Parsed with config | P1 |
| BND-031 | Date | "Feb 11, 2026" | Parsed | P2 |
| BND-032 | Date | "" (empty) | Null | P1 |
| BND-033 | Date | "9999-12-31" | Edge | P2 |
| BND-034 | Date | "1900-01-01" | Edge | P2 |
| BND-035 | Numeric | 0 | Accepted | P1 |
| BND-036 | Numeric | -1 | Depends on field | P1 |
| BND-037 | Numeric | MAX_INT | Handled | P2 |
| BND-038 | Numeric | Decimal | Accepted if float field | P2 |
| BND-039 | Unicode | Arabic data | Stored | P2 |
| BND-040 | Unicode | Chinese data | Stored | P2 |
| BND-041 | Unicode | Cyrillic data | Stored | P2 |
| BND-042 | Unicode | French accents | Stored | P2 |
| BND-043 | Unicode | Emoji data | Handled | P2 |
| BND-044 | Batch | Batch size 1 | Single batch | P1 |
| BND-045 | Batch | Batch size 100 | 1 batch (if < limit) | P1 |
| BND-046 | Batch | Batch size 1000 | Multiple batches | P1 |
| BND-047 | Batch | Last batch partial | Handled | P1 |
| BND-048 | Dup | 0 duplicates | All imported | P1 |
| BND-049 | Dup | 1 duplicate | 1 handled per policy | P1 |
| BND-050 | Dup | All duplicates | All skipped/merged | P1 |
| BND-051 | Dup | 50% duplicates | Mixed result | P1 |
| BND-052 | Error | 0 errors | Clean import | P1 |
| BND-053 | Error | 1 error in 100 | 99 imported, 1 skipped | P1 |
| BND-054 | Error | 50% errors | Partial import | P1 |
| BND-055 | Error | 100% errors | 0 imported, full report | P1 |
| BND-056 | Progress | 1 row, 100% at once | Instant complete | P2 |
| BND-057 | Progress | 10,000 rows | Incremental updates | P2 |
| BND-058 | History | 0 past imports | Empty history | P2 |
| BND-059 | History | 100 past imports | All listed | P2 |
| BND-060 | History | 1000 past imports | Paginated | P2 |
| BND-061 | Transform | No transforms | Raw data | P2 |
| BND-062 | Transform | 1 transform | Applied | P2 |
| BND-063 | Transform | 10 transforms | All applied | P2 |
| BND-064 | Map | Auto-map 100% match | All mapped | P1 |
| BND-065 | Map | Auto-map 0% match | Manual required | P1 |
| BND-066 | Map | Auto-map 50% match | Partial + manual | P1 |
| BND-067 | Delimiter | Comma | Standard CSV | P1 |
| BND-068 | Delimiter | Semicolon | European CSV | P1 |
| BND-069 | Delimiter | Tab | TSV format | P2 |
| BND-070 | Delimiter | Pipe | Custom delimiter | P2 |
| BND-071 | Rows | 999 rows | Imported <30s | P1 |
| BND-072 | Rows | 1001 rows | Batched | P1 |
| BND-073 | Cols | 25 columns | All mapped | P1 |
| BND-074 | Cols | 75 columns | Handled | P2 |
| BND-075 | Size | 99 MB file | Max or accepted | P1 |
| BND-076 | Field | Name 100 chars | Accepted | P1 |
| BND-077 | Field | Email 319 chars | Accepted | P1 |
| BND-078 | Batch | Batch size 50 | 1 batch (if < limit) | P1 |
| BND-079 | Batch | Batch size 500 | Multiple batches | P1 |
| BND-080 | Dup | 2 duplicates | Both handled | P1 |
| BND-081 | Error | 99% errors | 1 imported, report | P1 |
| BND-082 | Map | Auto-map 75% match | Partial + manual | P1 |
| BND-083 | Date | "2026-01-01" (start of year) | Parsed | P2 |
| BND-084 | Date | "2026-12-31" (end of year) | Parsed | P2 |
| BND-085 | Numeric | 1 | Accepted | P1 |
| BND-086 | Unicode | Japanese data | Stored | P2 |
| BND-087 | History | 50 past imports | All listed | P2 |
| BND-088 | Transform | 5 transforms | All applied | P2 |
| BND-089 | Progress | 100 rows | Incremental updates | P2 |
| BND-090 | Delimiter | Colon | Custom delimiter | P2 |

---

## §4-§10 (Functional through Load Tests)

### §4 Functional Tests — 90 tests
**4.1 Import Pipeline (15):** File parse, header detect, field mapping, validation pass, duplicate check, conflict resolution, batch insert, progress update, error collection, report generation, audit log, notification, rollback, partial commit, import complete.

**4.2 Validation (15):** Required fields, email format, date format, numeric format, FK reference, unique constraint, string length, type validation, encoding validation, header validation, row count validation, delimiter detection, BOM handling, file type validation, virus scan.

**4.3 Conflict Resolution (10):** Skip duplicates, overwrite duplicates, merge duplicates, within-file duplicates, cross-file duplicates, custom resolution rule, manual review queue, auto-resolve by date, auto-resolve by source, conflict report.

**4.4 Audit & History (10):** Import start, import complete, import failed, rows imported count, rows skipped count, errors count, user tracked, duration tracked, file stored, re-import tracking.

**4.5 Extended Functional (40):** FUN-051: Trim applied before validation; FUN-052: Case normalization for codes; FUN-053: Null coalesce for optional fields; FUN-054: Default value injection; FUN-055: FK lookup cache; FUN-056: Batch commit atomicity; FUN-057: Partial batch rollback; FUN-058: Progress granularity; FUN-059: Error row index tracking; FUN-060: Duplicate key handling; FUN-061: Encoding fallback chain; FUN-062: Date format priority; FUN-063: Column order independence; FUN-064: Header case insensitivity; FUN-065: Empty cell handling; FUN-066: Whitespace-only cell; FUN-067: Formula evaluation disabled; FUN-068: Hyperlink extraction; FUN-069: Comment row exclusion; FUN-070: Multi-header detection; FUN-071: Sheet name validation; FUN-072: Row limit enforcement; FUN-073: File size pre-check; FUN-074: MIME validation; FUN-075: Checksum verification; FUN-076: Temp file naming; FUN-077: Cleanup on cancel; FUN-078: Notification on partial; FUN-079: Retry failed rows option; FUN-080: Idempotent re-import; FUN-081: Delta import detection; FUN-082: Incremental import; FUN-083: Import versioning; FUN-084: Schema validation; FUN-085: Cross-entity FK; FUN-086: Hierarchical import order; FUN-087: Lookup table pre-load; FUN-088: Transform dependency order; FUN-089: Validation dependency; FUN-090: Report aggregation.

### §5 Integration Tests — 90 tests
**5.1 Entity Import (10):** Partners CSV, Partners Excel, Contacts CSV, Contacts Excel, Opportunities CSV, Interactions CSV, mixed entity file, linked entities, hierarchical import, import with documents.

**5.2 Database (10):** Transaction commit, transaction rollback, constraint enforcement, FK validation, unique enforcement, batch insert, partial batch, connection recovery, timeout handling, deadlock.

**5.3 File Processing (10):** CSV parse, Excel parse, encoding detection, large file stream, multipart upload, temp file cleanup, concurrent file access, file validation, MIME validation, checksum.

**5.4 Error Handling (10):** Parse error report, validation report, DB error report, partial import report, complete failure report, error CSV export, row-level errors, column-level errors, retry failed rows, error notification.

**5.5 Cross-Feature (10):** Imported partners appear in list, imported contacts linked, search finds imported, export includes imported, AI processes imported, dashboard updated, audit trail, notifications, permissions on imported, workflow status.

**5.6 Extended Integration (40):** INT-051: Partner import → Contact import; INT-052: Contact import → Interaction import; INT-053: Full entity chain import; INT-054: Import → API read round-trip; INT-055: Import → Export round-trip; INT-056: Import → Search round-trip; INT-057: Import → Filter round-trip; INT-058: Multi-entity single file; INT-059: Cross-entity FK validation; INT-060: Import with existing references; INT-061: Import with new references; INT-062: Batch import → Batch read; INT-063: Dry-run → Real import; INT-064: Cancel → Retry; INT-065: Partial → Re-import failed; INT-066: Error report → Fix → Re-import; INT-067: Template download → Populate → Import; INT-068: History → Re-run; INT-069: Schedule → Manual trigger; INT-070: Concurrent entity imports; INT-071: Import → Dashboard refresh; INT-072: Import → Notification; INT-073: Import → Audit query; INT-074: Import → Permission check; INT-075: Import → Workflow trigger; INT-076: Large file → Streaming; INT-077: Multipart upload → Import; INT-078: Storage → DB transaction; INT-079: DB → Storage cleanup; INT-080: Retry → Idempotent; INT-081: Timeout → Partial commit; INT-082: Connection pool → Import; INT-083: Transaction scope; INT-084: Savepoint on error; INT-085: Import → Cache invalidation; INT-086: Import → Search index; INT-087: Import → Report generation; INT-088: Import → Analytics; INT-089: Import → API contract; INT-090: End-to-end validation.

### §6 Security Tests — 50 tests
**6.1 Injection (10):** SQL in CSV data, SQL in headers, XSS in data, XSS in filename, formula injection (=CMD), DDE injection, macro injection, command injection, template injection, path traversal.

**6.2 Access Control (10):** Anonymous, no permission, wrong scope, expired token, tampered JWT, vertical escalation, horizontal access, disabled account, post-logout, role escalation.

**6.3 File Security (10):** .exe disguised, virus in CSV, malicious macros, zip bomb (compressed CSV), polyglot file, oversized file DoS, encoding attack, null byte in file, content-type mismatch, path traversal in archive.

**6.4 Data Security (10):** PII in error logs, PII in reports, sensitive data in temp files, temp file cleanup, import file retention, audit data security, GDPR compliance, data masking, encryption at rest, encryption in transit.

**6.5 Operational (10):** Rate limiting, concurrent import limit, resource exhaustion, memory limit, CPU limit, disk limit, connection pool, session fixation, CSRF, import token security.

### §7 Concurrency Tests — 25 tests
Two concurrent imports, import + normal CRUD, import + export same entity, concurrent cancel, concurrent status check, import + scheduled import, large import + small import, concurrent validation, DB deadlock during import, connection pool under import load, batch commit concurrent, rollback concurrent, progress update concurrent, file upload concurrent, duplicate detection concurrent, FK validation concurrent, audit log ordering, notification concurrent, temp file concurrent, cache invalidation, parallel batch processing, import + search, import + AI processing, session timeout during import, real-time progress.

### §8 Unit Tests — 21 tests
**Parse (5):** CSV parse, Excel parse, header detection, delimiter detection, encoding detection.
**Validation (5):** Email, date, required field, numeric, string length.
**Transform (3):** Trim, uppercase, date normalize.
**Mapping (3):** Auto-map, custom map, map validation.
**Duplicate (5):** Exact match, fuzzy match, within-file, cross-entity, threshold config.

### §9 Performance Tests — 16 tests
Import 100 rows (<5s), 1000 rows (<30s), 10,000 rows (<5min), 100,000 rows (<30min), CSV parse 10MB (<5s), Excel parse 10MB (<10s), validation 10,000 rows (<10s), duplicate check 10,000 (<10s), memory 10,000 rows (<200MB), memory 100,000 (<500MB), batch insert 1000 (<10s), concurrent 5 imports (stable), progress update overhead (<5%), error report generation (<5s), dry-run 10,000 (<1min), rollback 10,000 (<10s).

### §10 Load Tests — 10 tests
3 concurrent imports (30min, all complete), 10 concurrent status checks (30min, <200ms), spike 1→10 imports (5min, queued), sustained import (100K rows, 30min, stable), large file (50MB, 15min, complete), recovery DB crash (<60s), recovery service restart (<30s), mixed ops (import+CRUD+search, 30min), weekend bulk (500K rows), disk usage monitoring.

---

## Traceability Matrix

| Business Rule | Test Cases |
|--------------|-----------|
| CSV/Excel import | POS-001–005, INT-5.1 |
| Field mapping | POS-009, POS-022–023, BND-064–066 |
| Duplicate detection | POS-003, POS-030, NEG-016, BND-048–051 |
| Conflict resolution | FUN-4.3, POS-006–008 |
| Validation | NEG-011–020, FUN-4.2 |
| Rollback | POS-005, NEG-070, BND-052–055 |
| Security | SEC-001–050, NEG-031–037 |
| Performance | PRF-001–016, LDT-001–010 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
