# LinkManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/LinkManager`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

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

**LinkManager** manages CRUD for links, URL validation, entity association, categorization, and preview. Key responsibilities: link lifecycle, entity linking (Partner/Contact/PartnerTree), URL validation, categorization, name defaulting to URL, and orphan handling.

---

## §1 Positive Tests (35)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Create link — Partner | Partner exists | CreateLink(Partner, 123, url) | Link created | P0 |
| POS-002 | Create link — Contact | Contact exists | CreateLink(Contact, 456, url) | Link created | P0 |
| POS-003 | Create link — PartnerTree | PartnerTree exists | CreateLink(PartnerTree, 789, url) | Link created | P0 |
| POS-004 | Create — name defaults to URL | Name=null | CreateLink | Name=URL | P0 |
| POS-005 | Create — explicit name | Name provided | CreateLink | Name saved | P1 |
| POS-006 | Get link by ID | Link exists | GetLink(789) | Link returned | P0 |
| POS-007 | Update link | Link exists | UpdateLink | Updated | P0 |
| POS-008 | Delete link | Link exists | DeleteLink | IsDeleted=true | P0 |
| POS-009 | Get link — not found | ID 99999 | GetLink(99999) | Null | P1 |
| POS-010 | Update — not found | ID 99999 | UpdateLink(99999) | Null | P1 |
| POS-011 | Delete — not found | ID 99999 | DeleteLink(99999) | Graceful | P1 |
| POS-012 | Get entity links — Partner | Partner 123 | GetEntityLinks(Partner, 123) | Partner links | P0 |
| POS-013 | Get entity links — Contact | Contact 456 | GetEntityLinks(Contact, 456) | Contact links | P0 |
| POS-014 | Get entity links — PartnerTree | PartnerTree 789 | GetEntityLinks(PartnerTree, 789) | PartnerTree links | P0 |
| POS-015 | Get all links | Links exist | GetAllLinks | All non-deleted | P1 |
| POS-016 | URL validation — https | https://example.com | CreateLink | Accepted | P0 |
| POS-017 | URL validation — http | http://example.com | CreateLink | Accepted | P1 |
| POS-018 | Categorization | Category provided | CreateLink | Category saved | P1 |
| POS-019 | Preview generation | Link created | GetLinkPreview | Preview | P1 |
| POS-020 | Update URL | Link exists | UpdateLink new URL | URL updated | P1 |
| POS-021 | Update name | Link exists | UpdateLink new name | Name updated | P1 |
| POS-022 | Update category | Link exists | UpdateLink new category | Category updated | P1 |
| POS-023 | Full CRUD cycle | None | Create→Get→Update→Get→Delete | All succeed | P0 |
| POS-024 | Orphan handling | Entity deleted | GetLink | Null, link deleted | P1 |
| POS-025 | Pagination | 100 links | GetEntityLinks paginated | Paginated | P1 |
| POS-026 | Filter by category | Links with categories | GetEntityLinks filter | Filtered | P1 |
| POS-027 | Sort by name | Links exist | GetEntityLinks OrderBy=Name | Sorted | P1 |
| POS-028 | Sort by URL | Links exist | GetEntityLinks OrderBy=Url | Sorted | P1 |
| POS-029 | Multiple links per entity | Partner has 10 links | GetEntityLinks | 10 returned | P1 |
| POS-030 | Empty entity links | Entity has no links | GetEntityLinks | Empty list | P1 |
| POS-031 | URL with query string | https://example.com?q=1 | CreateLink | Accepted | P1 |
| POS-032 | URL with fragment | https://example.com#section | CreateLink | Accepted | P1 |
| POS-033 | Deleted link excluded | Link IsDeleted | GetEntityLinks | Excluded | P0 |
| POS-034 | Specification filter | Spec | GetLinksWithSpecification | Filtered | P1 |
| POS-035 | Audit trail | Create | CreateLink | Audit entry | P1 |

---

## §2 Negative Tests (70)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Create — entity not exist | EntityId=99999 | Error | P0 |
| NEG-002 | Create — invalid URL | URL="not-a-url" | Validation error | P0 |
| NEG-003 | Create — null URL | URL=null | ArgumentNullException | P0 |
| NEG-004 | Create — empty URL | URL="" | Validation error | P0 |
| NEG-005 | Create — invalid entity type | EntityType="Invalid" | Error | P0 |
| NEG-006 | Get — ID zero | GetLink(0) | Null | P1 |
| NEG-007 | Get — ID negative | GetLink(-1) | Null | P1 |
| NEG-008 | Update — non-existent | UpdateLink(99999) | Null | P1 |
| NEG-009 | Update — null model | UpdateLink(null) | ArgumentNullException | P0 |
| NEG-010 | Delete — already deleted | Link IsDeleted | Idempotent | P1 |
| NEG-011 | URL — javascript: | javascript:alert(1) | Rejected | P0 |
| NEG-012 | URL — data: | data:text/html | Rejected | P0 |
| NEG-013 | URL — file: | file:///etc/passwd | Rejected | P0 |
| NEG-014 | SQL injection in URL | ' OR 1=1-- | Sanitized | P0 |
| NEG-015 | XSS in name | <script>alert(1)</script> | Sanitized | P0 |
| NEG-016 | XSS in URL | javascript:... | Rejected | P0 |
| NEG-017 | Unauthorized create | User lacks permission | 403 | P0 |
| NEG-018 | Unauthorized update | User lacks permission | 403 | P0 |
| NEG-019 | Unauthorized delete | User lacks permission | 403 | P0 |
| NEG-020 | IDOR — access other org | GetLink(otherId) | 403 | P0 |
| NEG-021 | IDOR — update other org | UpdateLink(otherId) | 403 | P0 |
| NEG-022 | IDOR — delete other org | DeleteLink(otherId) | 403 | P0 |
| NEG-023 | Mass assignment | Include Id | CreateLink | Ignored | P0 |
| NEG-024 | Unauthenticated | No auth | Any op | 401 | P0 |
| NEG-025 | Expired token | Expired JWT | Any op | 401 | P0 |
| NEG-026 | Open redirect | URL to phishing | Validation | Rejected | P0 |
| NEG-027 | DNS rebinding | Malicious URL | Validation | Rejected | P0 |
| NEG-028 | SSRF | Internal URL | CreateLink | Rejected | P0 |
| NEG-029 | URL too long | URL 2000 chars | Validation error | P1 |
| NEG-030 | Name too long | Name 256 chars | Validation error | P1 |
| NEG-031 | Invalid entity type | EntityType="Xyz" | Error | P1 |
| NEG-032 | Entity soft-deleted | Entity deleted | CreateLink | Error | P1 |
| NEG-033 | Pagination — invalid | PageIndex=-1 | Error | P1 |
| NEG-034 | Specification — null | GetLinksWithSpecification(null) | ArgumentNullException | P1 |
| NEG-035 | Category invalid | Category="Invalid" | Error | P1 |
| NEG-036 | Database timeout | DB timeout | CreateLink | Exception | P1 |
| NEG-037 | Concurrent update conflict | 2 users update | Concurrency error | P1 |
| NEG-038 | Duplicate URL same entity | Same URL exists | CreateLink | Per rule | P1 |
| NEG-039 | Preview fetch fail | Unreachable URL | GetLinkPreview | Error/default | P1 |
| NEG-040 | Rate limit | Too many creates | CreateLink | 429 | P1 |
| NEG-041 | Org scope bypass | User OrgB | GetEntityLinks | 403 or filtered | P0 |
| NEG-042 | Null entity ID | EntityId=null | GetEntityLinks | Error | P1 |
| NEG-043 | Zero entity ID | EntityId=0 | GetEntityLinks | Empty or error | P1 |
| NEG-044 | JWT alg none | alg=none | Request | Rejected | P0 |
| NEG-045 | Brute force | Enumerate | GetLink | Rate limited | P1 |
| NEG-046 | CSRF create | Cross-site | CreateLink | Token validated | P0 |
| NEG-047 | CSRF update | Cross-site | UpdateLink | Token validated | P0 |
| NEG-048 | CSRF delete | Cross-site | DeleteLink | Token validated | P0 |
| NEG-049 | Log injection | Malicious log | Log | Sanitized | P1 |
| NEG-050 | Parameter pollution | id=1&id=2 | Get | Handled | P1 |
| NEG-051 | Sensitive data error | Stack trace | Exception | Not exposed | P0 |
| NEG-052 | URL encoding | %00 in URL | CreateLink | Rejected | P0 |
| NEG-053 | Unicode URL | IDN | CreateLink | Handled | P1 |
| NEG-054 | IPv6 URL | [::1] | CreateLink | Accepted or rejected | P1 |
| NEG-055 | Localhost URL | http://localhost | CreateLink | Per rule | P1 |
| NEG-056 | Private IP URL | http://192.168.1.1 | CreateLink | Per rule | P1 |
| NEG-057 | URL with credentials | http://user:pass@host | CreateLink | Sanitized | P0 |
| NEG-058 | Malformed URL | "htxp://" | CreateLink | Rejected | P0 |
| NEG-059 | Relative URL | /path | CreateLink | Rejected | P0 |
| NEG-060 | Protocol relative | //example.com | CreateLink | Per rule | P1 |
| NEG-061 | Update — entity deleted | Entity deleted | UpdateLink | Error | P1 |
| NEG-062 | Get — deleted link | Link IsDeleted | GetLink | Null | P1 |
| NEG-063 | Batch create — partial fail | One invalid | Per transaction | P2 |
| NEG-064 | Specification — invalid | Malformed spec | GetLinksWithSpecification | Error | P2 |
| NEG-065 | Preview timeout | Slow URL | GetLinkPreview | Timeout | P1 |
| NEG-066 | Preview size limit | Huge page | GetLinkPreview | Limited | P1 |
| NEG-067 | Category XSS | Category with script | CreateLink | Sanitized | P0 |
| NEG-068 | Name null | Name=null | CreateLink | Defaults to URL | P1 |
| NEG-069 | Update — invalid URL | Update to invalid | UpdateLink | Error | P1 |
| NEG-070 | Audit log failure | Audit down | Any op | Op succeeds | P2 |

---

## §3 Boundary Tests (70)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | URL length | 10 | 2048 | "https://a.b" | 2048 chars | 2049 chars | P1 |
| BND-002 | Name length | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-003 | Category length | 0 | 100 | "" | 100 chars | 101 chars | P1 |
| BND-004 | LinkId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-005 | EntityId | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-006 | PageIndex | 0 | Max | 0 | Valid | -1 | P1 |
| BND-007 | PageSize | 1 | 1000 | 1 | 1000 | 1001 | P1 |
| BND-008 | Links per entity | 0 | 100 | 0 | 100 | 101 | P1 |
| BND-009 | Empty result | — | — | 0 links | — | — | P1 |
| BND-010 | Single result | — | — | 1 link | — | — | P1 |
| BND-011 | Unicode in URL | — | — | IDN | — | — | P1 |
| BND-012 | Unicode in name | — | — | "リンク" | — | — | P1 |
| BND-013 | Special chars URL | — | — | ? & = | — | — | P1 |
| BND-014 | Pagination last partial | — | — | 95 total, Size=20 | — | — | P1 |
| BND-015 | Pagination beyond last | — | — | Page 100 | — | — | P1 |
| BND-016 | Zero LinkId | — | — | GetLink(0) | — | — | P1 |
| BND-017 | Null optional | — | — | Name=null | — | — | P1 |
| BND-018 | Control chars URL | — | — | \x00 in URL | — | — | P1 |
| BND-019 | Emoji in name | — | — | "🔗 Link" | — | — | P2 |
| BND-020 | RTL in name | — | — | Arabic | — | — | P2 |
| BND-021 | Newline in name | — | — | "Line1\nLine2" | — | — | P1 |
| BND-022 | Tab in URL | — | — | Tab in URL | — | — | P1 |
| BND-023 | Whitespace URL | — | — | "  https://example.com  " | — | — | P1 |
| BND-024 | Leading/trailing name | — | — | "  Name  " | — | — | P1 |
| BND-025 | Multiple spaces | — | — | "Name   here" | — | — | P2 |
| BND-026 | Date boundaries | — | — | Min/Max DateTime | — | — | P2 |
| BND-027 | Timestamp precision | — | — | Millisecond | — | — | P2 |
| BND-028 | Entity type enum | — | — | Partner, Contact, PartnerTree | — | — | P1 |
| BND-029 | URL scheme | — | — | https, http | — | — | P1 |
| BND-030 | URL port | — | — | :443, :8080 | — | — | P1 |
| BND-031 | URL path | — | — | /path/to/page | — | — | P1 |
| BND-032 | URL query | — | — | ?a=1&b=2 | — | — | P1 |
| BND-033 | URL fragment | — | — | #section | — | — | P1 |
| BND-034 | Negative EntityId | — | — | EntityId=-1 | — | — | P1 |
| BND-035 | Float EntityId | — | — | EntityId=1.5 | — | — | P2 |
| BND-036 | Null EntityId | — | — | EntityId=null | — | — | P1 |
| BND-037 | Empty category | — | — | Category="" | — | — | P1 |
| BND-038 | Null category | — | — | Category=null | — | — | P1 |
| BND-039 | Long category | — | — | 100 chars | — | — | P1 |
| BND-040 | Preview content size | — | — | Max preview | — | — | P2 |
| BND-041 | Preview image size | — | — | OG image | — | — | P2 |
| BND-042 | Preview title length | — | — | OG title | — | — | P2 |
| BND-043 | Preview description length | — | — | OG description | — | — | P2 |
| BND-044 | Collection empty | — | — | [] | — | — | P1 |
| BND-045 | Collection null | — | — | null | — | — | P1 |
| BND-046 | Sort empty | — | — | OrderBy on empty | — | — | P1 |
| BND-047 | Filter empty | — | — | No filter | — | — | P1 |
| BND-048 | Concurrent create | — | — | 2 threads same entity | — | — | P1 |
| BND-049 | Same URL different entity | — | — | Same URL, diff entity | — | — | P1 |
| BND-050 | URL case | — | — | HTTPS vs https | — | — | P1 |
| BND-051 | Duplicate URL same entity | — | — | Same URL twice | — | — | P1 |
| BND-052 | Empty string name | — | — | Name="" | — | — | P1 |
| BND-053 | Empty string URL | — | — | URL="" | — | — | P1 |
| BND-054 | HTML in name | — | — | <b>bold</b> | — | — | P1 |
| BND-055 | URL with brackets | — | — | [IPv6] | — | — | P1 |
| BND-056 | URL with space | — | — | %20 | — | — | P1 |
| BND-057 | URL encoded | — | — | %2F etc | — | — | P1 |
| BND-058 | Very long path | — | — | /a/b/... | — | — | P2 |
| BND-059 | Very long query | — | — | ?a=... | — | — | P2 |
| BND-060 | Multiple entities | — | — | 100 entities | — | — | P2 |
| BND-061 | Category hierarchy | — | — | Parent/child | — | — | P2 |
| BND-062 | Preview fetch timeout | — | — | Slow server | — | — | P2 |
| BND-063 | Preview redirect | — | — | 301/302 | — | — | P2 |
| BND-064 | Preview 404 | — | — | 404 URL | — | — | P1 |
| BND-065 | Preview 500 | — | — | 500 URL | — | — | P1 |
| BND-066 | Preview SSL error | — | — | Invalid cert | — | — | P2 |
| BND-067 | Preview content type | — | — | text/html | — | — | P2 |
| BND-068 | Preview charset | — | — | UTF-8 | — | — | P2 |
| BND-069 | Preview meta tags | — | — | OG tags | — | — | P2 |
| BND-070 | Nested includes | — | — | Link→Entity | — | — | P2 |

---

## §4 Functional Tests (50)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Soft delete | Delete | DeleteLink | IsDeleted=true | P0 |
| FUN-002 | Deleted excluded | List | GetEntityLinks | Deleted excluded | P0 |
| FUN-003 | CreatedBy/CreatedDate | Create | CreateLink | Audit set | P0 |
| FUN-004 | LastModified on update | Update | UpdateLink | Updated | P0 |
| FUN-005 | Name default to URL | Name=null | CreateLink | Name=URL | P0 |
| FUN-006 | Entity validation | Create | CreateLink | Entity exists | P0 |
| FUN-007 | URL validation | Create | CreateLink | Valid URL | P0 |
| FUN-008 | Entity type filter | Get | GetEntityLinks | By type | P0 |
| FUN-009 | Pagination TotalCount | List | GetEntityLinks | Accurate | P0 |
| FUN-010 | Sort applied | Sort | GetEntityLinks | Sorted | P1 |
| FUN-011 | Org scope | User OrgA | GetEntityLinks | OrgA only | P0 |
| FUN-012 | Permission create | User lacks | CreateLink | 403 | P0 |
| FUN-013 | Permission update | User lacks | UpdateLink | 403 | P0 |
| FUN-014 | Permission delete | User lacks | DeleteLink | 403 | P0 |
| FUN-015 | Orphan deletion | Entity deleted | GetLink | Link deleted | P1 |
| FUN-016 | Update entity check | Update | UpdateLink | Entity exists | P1 |
| FUN-017 | Update URL validation | Update | UpdateLink | Valid URL | P1 |
| FUN-018 | Category filter | Filter | GetEntityLinks | Category filtered | P1 |
| FUN-019 | Specification filter | Spec | GetLinksWithSpecification | Filtered | P1 |
| FUN-020 | Get all non-deleted | GetAllLinks | GetAllLinks | Deleted excluded | P0 |
| FUN-021 | Audit trail create | Create | CreateLink | Audit entry | P1 |
| FUN-022 | Audit trail update | Update | UpdateLink | Audit entry | P1 |
| FUN-023 | Audit trail delete | Delete | DeleteLink | Audit entry | P1 |
| FUN-024 | Idempotent delete | Delete twice | DeleteLink | Graceful | P1 |
| FUN-025 | Update non-existent | Update | UpdateLink(99999) | Null | P1 |
| FUN-026 | Get non-existent | Get | GetLink(99999) | Null | P1 |
| FUN-027 | Preview cache | GetLinkPreview | GetLinkPreview | Cached | P2 |
| FUN-028 | Preview refresh | Force refresh | GetLinkPreview | Fresh | P2 |
| FUN-029 | URL normalization | Create | CreateLink | Normalized | P1 |
| FUN-030 | Duplicate URL | Same URL | CreateLink | Per rule | P1 |
| FUN-031 | Entity type validation | Invalid type | CreateLink | Error | P1 |
| FUN-032 | Required fields | Missing URL | CreateLink | Validation error | P0 |
| FUN-033 | Optional name | Name=null | CreateLink | Defaults | P1 |
| FUN-034 | Optional category | Category=null | CreateLink | Accepted | P1 |
| FUN-035 | Optimistic concurrency | Concurrent update | UpdateLink | Conflict | P1 |
| FUN-036 | Categorization | Category | CreateLink | Category saved | P1 |
| FUN-037 | Multi-entity links | Entity has many | GetEntityLinks | All returned | P1 |
| FUN-038 | Empty entity | No links | GetEntityLinks | Empty | P1 |
| FUN-039 | URL scheme validation | https only | CreateLink | Enforced | P1 |
| FUN-040 | Blocked domains | Blocked | CreateLink | Rejected | P1 |
| FUN-041 | Allowed domains | Whitelist | CreateLink | Checked | P1 |
| FUN-042 | Preview optional | No preview | GetLink | Handled | P1 |
| FUN-043 | Status filter | Active only | GetEntityLinks | Active only | P1 |
| FUN-044 | Date filter | Created date | GetEntityLinks | Filtered | P1 |
| FUN-045 | Name required per entity | ModifiableDeletableEntity | Create | Name set | P0 |
| FUN-046 | WorkflowStatus | If applicable | Check | Per design | P2 |
| FUN-047 | Status transitions | Status | Update | Valid only | P1 |
| FUN-048 | Restore | Restore | DeleteLink | Restored | P2 |
| FUN-049 | Archive | Archive | UpdateLink | Archived | P2 |
| FUN-050 | Export | Export | GetEntityLinks | Exported | P2 |

---

## §5 Integration Tests (50)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Full CRUD | Create→Get→Update→Delete | Link | All succeed | P0 |
| INT-002 | PartnerManager | Get partner | Link, Partner | Partner loaded | P0 |
| INT-003 | ContactManager | Get contact | Link, Contact | Contact loaded | P0 |
| INT-004 | PartnerTreeManager | Get tree | Link, PartnerTree | Tree loaded | P0 |
| INT-005 | Permission | Authorize | Link, PermissionService | Correct | P0 |
| INT-006 | Audit | Audit | Link, AuditLog | Entries | P1 |
| INT-007 | UserContext | Current user | Link, UserResolver | UserId | P0 |
| INT-008 | DbContext | Persist | Link, DbContext | Saved | P0 |
| INT-009 | AutoMapper | Entity to Model | Link, AutoMapper | Mapped | P1 |
| INT-010 | Controller | API | Link, Controller | 200/201/204 | P0 |
| INT-011 | Error handling | Exception | Link, Handler | Consistent | P1 |
| INT-012 | Logging | Log | Link, ILogger | Logs | P2 |
| INT-013 | Configuration | Config | Link | Applied | P2 |
| INT-014 | ManagerWrapper | Resolution | ManagerWrapper | Correct | P1 |
| INT-015 | Multi-tenant | Org scope | Link | Isolated | P0 |
| INT-016 | API 404 | Get invalid | Controller | 404 | P0 |
| INT-017 | API 400 | Invalid request | Controller | 400 | P0 |
| INT-018 | API 403 | Unauthorized | Controller | 403 | P0 |
| INT-019 | List view | Links in list | Link | Displayed | P1 |
| INT-020 | Detail view | Link detail | Link | All sections | P0 |
| INT-021 | Preview service | GetLinkPreview | Link, PreviewService | Preview | P1 |
| INT-022 | URL fetcher | Fetch URL | Link | Fetched | P1 |
| INT-023 | Repository | CRUD | Link, Repository | Works | P1 |
| INT-024 | Validation | Validate | Link, Validator | Errors | P1 |
| INT-025 | Entity resolution | Resolve entity | Link | Resolved | P0 |
| INT-026 | Orphan cleanup | Cleanup job | Link | Orphans removed | P1 |
| INT-027 | Report | Report | Link | In report | P2 |
| INT-028 | Export | Export | Link | Exported | P2 |
| INT-029 | Import | Import | Link | Imported | P2 |
| INT-030 | Migration | Add field | Link | Migrated | P2 |
| INT-031 | Seed data | Seed | Link | Seeded | P2 |
| INT-032 | Feature flag | Feature | Link | Respected | P2 |
| INT-033 | Notification | Notify | Link, NotificationManager | Sent | P2 |
| INT-034 | Search | Search | Link | In results | P1 |
| INT-035 | Dashboard | Dashboard | Link | Count/List | P1 |
| INT-036 | Analytics | Analytics | Link | Metrics | P2 |
| INT-037 | Bulk operations | Bulk | Link | Batch | P2 |
| INT-038 | Cache | Cache | Link | Cached | P2 |
| INT-039 | Rate limit | Rate limit | Link | Enforced | P1 |
| INT-040 | Health check | Health | Link | Status | P2 |
| INT-041 | Metrics | Metrics | Link | Recorded | P2 |
| INT-042 | URL blocklist | Blocklist | Link | Checked | P1 |
| INT-043 | URL allowlist | Allowlist | Link | Checked | P1 |
| INT-044 | Preview timeout | Timeout | Link | Config | P1 |
| INT-045 | Preview user agent | User-Agent | Link | Set | P2 |
| INT-046 | Preview proxy | Proxy | Link | Used | P2 |
| INT-047 | Category service | Category | Link | Resolved | P1 |
| INT-048 | Entity lookup | Entity | Link | Lookup | P0 |
| INT-049 | Soft delete cascade | Delete entity | Link | Per rule | P1 |
| INT-050 | Restore cascade | Restore entity | Link | Per rule | P2 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | SQL injection URL | ' OR 1=1-- | CreateLink | Sanitized | P0 |
| SEC-002 | SQL injection name | '; DROP TABLE-- | CreateLink | Sanitized | P0 |
| SEC-003 | XSS in name | <script>alert(1)</script> | CreateLink | Sanitized | P0 |
| SEC-004 | XSS in URL | javascript:alert(1) | CreateLink | Rejected | P0 |
| SEC-005 | Open redirect | phishing URL | CreateLink | Rejected | P0 |
| SEC-006 | SSRF | http://internal | CreateLink | Rejected | P0 |
| SEC-007 | IDOR get | GetLink(otherId) | GetLink | 403 | P0 |
| SEC-008 | IDOR update | UpdateLink(otherId) | Update | 403 | P0 |
| SEC-009 | IDOR delete | DeleteLink(otherId) | Delete | 403 | P0 |
| SEC-010 | Mass assignment | Include Id | CreateLink | Ignored | P0 |
| SEC-011 | Unauthenticated | No auth | Any op | 401 | P0 |
| SEC-012 | Expired token | Expired JWT | Any | 401 | P0 |
| SEC-013 | Wrong role create | No permission | CreateLink | 403 | P0 |
| SEC-014 | Wrong role update | No permission | UpdateLink | 403 | P0 |
| SEC-015 | Wrong role delete | No permission | DeleteLink | 403 | P0 |
| SEC-016 | Org scope bypass | Cross-org | GetLink | 403 | P0 |
| SEC-017 | URL scheme bypass | data: | CreateLink | Rejected | P0 |
| SEC-018 | DNS rebinding | Malicious | CreateLink | Rejected | P0 |
| SEC-019 | LDAP injection | *)(uid=* | Filter | Sanitized | P0 |
| SEC-020 | Sensitive data error | Stack trace | Exception | Not exposed | P0 |
| SEC-021 | Rate limit | Too many | CreateLink | 429 | P1 |
| SEC-022 | CSRF create | Cross-site | CreateLink | Token validated | P0 |
| SEC-023 | CSRF update | Cross-site | UpdateLink | Token validated | P0 |
| SEC-024 | CSRF delete | Cross-site | DeleteLink | Token validated | P0 |
| SEC-025 | Parameter pollution | id=1&id=2 | Get | Handled | P1 |
| SEC-026 | Header injection | Malicious header | Request | Sanitized | P1 |
| SEC-027 | Brute force | Enumerate | GetLink | Rate limited | P1 |
| SEC-028 | JWT alg none | alg=none | Request | Rejected | P0 |
| SEC-029 | Log injection | Malicious log | Log | Sanitized | P1 |
| SEC-030 | Info disclosure | Probe | Invalid | Generic | P1 |
| SEC-031 | Cookie manipulation | Modify auth | Request | Rejected | P0 |
| SEC-032 | Substitution attack | Replace JWT | Request | 403 | P0 |
| SEC-033 | Timing attack | Response time | GetLink | Constant | P2 |
| SEC-034 | Cache poisoning | Malicious cache | Cache | Sanitized | P1 |
| SEC-035 | Excessive data | Huge PageSize | GetEntityLinks | Capped | P1 |
| SEC-036 | DoS create | Many creates | CreateLink | 429 | P0 |
| SEC-037 | DoS preview | Many previews | GetLinkPreview | Rate limited | P0 |
| SEC-038 | Privilege escalation | Admin action | User | 403 | P0 |
| SEC-039 | Entity IDOR | EntityId other org | CreateLink | Error | P0 |
| SEC-040 | Token expiry | Expired | Request | 401 | P0 |
| SEC-041 | Session fixation | Fixate | Login | New session | P1 |
| SEC-042 | Path traversal | ../../../ | URL | Rejected | P0 |
| SEC-043 | Null byte | %00 | URL | Rejected | P0 |
| SEC-044 | URL credentials | user:pass@ | CreateLink | Sanitized | P0 |
| SEC-045 | Audit bypass | Skip audit | Any op | Audit required | P0 |
| SEC-046 | PII in log | PII | Log | Not logged | P0 |
| SEC-047 | HTTP verb tampering | PUT vs POST | Create | 405 | P1 |
| SEC-048 | Replay attack | Replay | CreateLink | Rejected | P0 |
| SEC-049 | Preview SSRF | Preview internal | GetLinkPreview | Rejected | P0 |
| SEC-050 | Category injection | Malicious category | CreateLink | Sanitized | P0 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Concurrent create same entity | 10 threads CreateLink(Partner, 123) | All created | P0 |
| CON-002 | Concurrent update same link | 5 threads UpdateLink(789) | No corruption | P0 |
| CON-003 | Concurrent delete same link | 2 threads DeleteLink(789) | One succeeds | P0 |
| CON-004 | Create and get | Thread1 create, Thread2 get | Consistent | P1 |
| CON-005 | Update and get | Thread1 update, Thread2 get | Consistent | P1 |
| CON-006 | Delete and get | Thread1 delete, Thread2 get | Null | P0 |
| CON-007 | Optimistic concurrency | 2 users update same | Conflict | P0 |
| CON-008 | Connection pool | 100 concurrent | No exhaustion | P1 |
| CON-009 | Deadlock | Circular | No deadlock | P1 |
| CON-010 | Double submit | User double-clicks | One created | P0 |
| CON-011 | Race on entity | 2 threads same entity | Handled | P1 |
| CON-012 | List during create | Thread1 create, Thread2 list | Consistent | P1 |
| CON-013 | Filter concurrent | 10 threads different filters | All correct | P1 |
| CON-014 | Pagination concurrent | 20 threads different pages | Correct pages | P1 |
| CON-015 | Preview concurrent | 10 threads GetLinkPreview | All succeed | P1 |
| CON-016 | Transaction isolation | Read uncommitted | Per level | P1 |
| CON-017 | Lost update | 2 users different fields | Per design | P1 |
| CON-018 | Phantom read | Insert during list | Per isolation | P2 |
| CON-019 | Non-repeatable read | Update between reads | Per isolation | P2 |
| CON-020 | Cache consistency | Concurrent cache | Consistent | P1 |
| CON-021 | Bulk create | 2 threads bulk | Consistent | P1 |
| CON-022 | Orphan concurrent | Delete entity during get | Handled | P1 |
| CON-023 | Entity links concurrent | 10 threads GetEntityLinks | All correct | P1 |
| CON-024 | Specification concurrent | 15 threads different specs | All correct | P1 |
| CON-025 | GetAllLinks concurrent | 20 threads GetAllLinks | All correct | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | URL validation | Validation | "https://example.com" | Valid | P0 |
| UNT-002 | URL invalid | Validation | "not-a-url" | Invalid | P0 |
| UNT-003 | URL empty | Validation | "" | Invalid | P0 |
| UNT-004 | Name validation | Validation | "Link" | Valid | P0 |
| UNT-005 | Entity type validation | Validation | "Partner" | Valid | P0 |
| UNT-006 | URL trim | Formatting | "  https://example.com  " | Trimmed | P1 |
| UNT-007 | Name trim | Formatting | "  Link  " | "Link" | P1 |
| UNT-008 | URL normalization | Formatting | URL | Normalized | P1 |
| UNT-009 | Name default | Calculation | URL, Name=null | Name=URL | P1 |
| UNT-010 | Status Active | Status logic | IsDeleted=false | Active | P1 |
| UNT-011 | Status Deleted | Status logic | IsDeleted=true | Excluded | P0 |
| UNT-012 | Collection filter | Collections | List with deleted | Excluded | P1 |
| UNT-013 | Empty collection | Collections | No links | Count=0 | P1 |
| UNT-014 | Null to empty | Collections | Null list | [] | P1 |
| UNT-015 | Map to Model | Mapping | Link entity | LinkModel | P0 |
| UNT-016 | Map Request | Mapping | CreateRequest | Entity | P0 |
| UNT-017 | Pagination slice | Calculation | Page 1, Size 10 | Skip 10, Take 10 | P1 |
| UNT-018 | Entity type parse | Validation | "partner" | Partner | P1 |
| UNT-019 | URL scheme check | Validation | https | Valid | P1 |
| UNT-020 | Audit fields | Status logic | New link | CreatedBy set | P1 |
| UNT-021 | Orphan check | Calculation | Entity deleted | IsOrphan | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | Create link | CreateLink | < 200ms | P0 |
| PRF-002 | Get by ID | GetLink | < 100ms | P0 |
| PRF-003 | Get entity links | GetEntityLinks (100) | < 300ms | P0 |
| PRF-004 | Update link | UpdateLink | < 200ms | P0 |
| PRF-005 | Delete link | DeleteLink | < 100ms | P0 |
| PRF-006 | Get all links | GetAllLinks (1000) | < 1000ms | P0 |
| PRF-007 | Get link preview | GetLinkPreview | < 2000ms | P0 |
| PRF-008 | Specification query | GetLinksWithSpecification | < 300ms | P1 |
| PRF-009 | Pagination | GetEntityLinks page 1 | < 200ms | P1 |
| PRF-010 | Entity validation | Validate entity | < 50ms | P1 |
| PRF-011 | URL validation | Validate URL | < 10ms | P1 |
| PRF-012 | Memory 100 | GetEntityLinks PageSize=100 | < 50MB | P1 |
| PRF-013 | Concurrent 20 | 20 CreateLink | < 300ms each | P1 |
| PRF-014 | AutoMapper | Mapping | < 10ms | P1 |
| PRF-015 | Cold start | First query | < 300ms | P2 |
| PRF-016 | Cached preview | Second GetLinkPreview | < 50ms | P2 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | Sustained 20 req/s create | 20 CreateLink/sec | 5 min | 95% < 200ms | P0 |
| LDT-002 | Sustained 50 req/s get | 50 GetLink/sec | 5 min | 95% < 100ms | P0 |
| LDT-003 | Sustained 30 req/s list | 30 GetEntityLinks/sec | 5 min | 95% < 300ms | P0 |
| LDT-004 | Spike 100 req/s | 100 req/s burst | 1 min | No crash | P0 |
| LDT-005 | Spike 200 req/s | 200 req/s | 30 sec | Graceful degrade | P1 |
| LDT-006 | Stress ramp | 1→500 req/s | Until fail | Find limit | P1 |
| LDT-007 | Connection pool | 200 concurrent | 2 min | No exhaustion | P1 |
| LDT-008 | Memory | 10K links | 5 min | No leak | P1 |
| LDT-009 | Recovery spike | Spike then normal | 5 min | Baseline | P0 |
| LDT-010 | Recovery stress | Stress then restart | Post-restart | Full recovery | P1 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
