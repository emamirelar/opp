# PartnerManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/PartnerManager`  
**Interface:** `UNOPS.PAO.Business/Interfaces/IPartnerManager.cs`  
**Base:** `UNOPS.PAO.Business/Managers/PartnerManager.cs`  
**Override:** `UNOPS.PAO.UNOPSBusiness/Managers/UNOPSPartnerManager.cs`  
**Controller:** `UNOPS.PAO.Presentation/Controllers/Partners/PartnerController.cs`  
**Entity:** `UNOPS.PAO.Domain/Entities/Partner.cs` (inherits ModifiableDeletableEntity)  
**Created:** 2026-02-18 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive (P) | 30 | 30-50 | ✅ |
| §2 Negative (N) | 90 | 90 | ✅ |
| §3 Boundary (E) | 90 | 90 | ✅ |
| §4 Functional (F) | 90 | 90 | ✅ |
| §5 Integration (I) | 90 | 90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Compliance Check**

| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 30 | POS-001 through POS-030 |
| Negative (N) | 90 | NEG-001 through NEG-090 |
| Edge/Boundary (E) | 90 | BND-001 through BND-090 |
| Functional (F) | 90 | FUN-001 through FUN-090 |
| Integration (I) | 90 | INT-001 through INT-090 |
| **N ≥ 3P?** | ✅ | 90 ≥ 90 |
| **E ≥ 3P?** | ✅ | 90 ≥ 90 |
| **F ≥ 3P?** | ✅ | 90 ≥ 90 |
| **I ≥ 3P?** | ✅ | 90 ≥ 90 |

---

## Feature Overview

**PartnerManager** manages partner organizations including CRUD, approval workflow, ERP dimension value assignment, status lifecycle (Draft→Active→Closed→Archived), partner groups/categories, organization unit relationships, logo upload, duplicate detection, smart search, bulk upload, Gmail addon integration, and opportunity linkage. Key responsibilities: partner lifecycle, soft-delete (IsDeleted), audit trail, permission-based access (HasPermissionAsync), and UNOPS-specific overrides via UNOPSPartner.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Create partner with valid minimal data | User has CanCreatePartners, LiaisonOffice exists | POST /api/partner with Name, PartnerShortDescription, PartnerCategoryId, PartnerGroupId, LiaisonOfficeId | 201 Created, partner returned with Id, Status=Draft | P0 |
| POS-002 | Create partner with all optional fields | User has permissions | POST /api/partner with full PartnerRequest (website, phone, address, OrgUnitRelationships, etc.) | Partner created with all fields persisted | P0 |
| POS-003 | Get partner by ID — exists | Partner 123 exists, not deleted | GET /api/partner/123 | PartnerModel returned with Id, Name, Status | P0 |
| POS-004 | Get partner with contacts and interactions | Partner 123 has 5 contacts, 20 interactions | GetPartnerWithContactsAndInteractionsAsync(123) | Partner with Contacts and Interactions loaded | P0 |
| POS-005 | Update partner basic fields | Partner 123 exists, user has edit permission | PUT /api/partner with Id=123, new Name, PartnerShortDescription | Changes persisted, LastModifiedDate updated | P0 |
| POS-006 | Soft delete partner | Partner 123 exists, user has delete permission | DELETE /api/partner/123 | IsDeleted=true, DeletedBy/DeletedDate set, 200 OK | P0 |
| POS-007 | List partners — paginated | 100 partners exist | GET /api/partner?pageIndex=1&pageSize=20 | PaginationResponse with 20 items, TotalCount=100 | P0 |
| POS-008 | Search partners — smart search | Partners "UNICEF", "UNHCR" exist | GET /api/partner/search?q=UN | Partners matching "UN" returned, ranked by relevance | P1 |
| POS-009 | Advanced search with filters | Partners in various groups | GET /api/partner/advanced-search with PartnerGroupId, Status filters | Filtered results returned | P1 |
| POS-010 | Get partners by partner group | PartnerGroup 5 has 12 partners | GET /api/partner/by-partner-group-id/5 | 12 partners returned | P1 |
| POS-011 | Get partners by category code | Category "GOV" has 8 partners | GET /api/partner/by-partner-category-code/GOV | 8 partners returned | P1 |
| POS-012 | Activate partner — Draft to Active | Partner in Draft with all mandatory fields | POST /api/partner/123/activate | Status=Active, 200 OK | P0 |
| POS-013 | Close partner — Active to Closed | Partner in Active status | POST /api/partner/123/close with StatusChangeRequest | Status=Closed | P1 |
| POS-014 | Archive partner — Closed to Archived | Partner in Closed status | POST /api/partner/123/archive | Status=Archived | P1 |
| POS-015 | Approve partner — admin workflow | Partner Active, user has admin permission | POST /api/partner/123/approve with UpdatePartnerRequest | PartnerApprovalStatus=Approved, ErpDimValue assigned, CanCreateNewOpportunities=true | P0 |
| POS-016 | Unapprove partner | Partner Approved | POST /api/partner/123/unapprove | PartnerApprovalStatus=NotApproved, CanCreateNewOpportunities=false | P1 |
| POS-017 | Get partner permissions | Partner exists, user authenticated | GET /api/partner/123/permissions | canView, canEdit, canDelete returned | P0 |
| POS-018 | Upload partner logo | Partner exists, valid JPG file | POST /api/partner/123/logo with IFormFile | LogoUrl updated, 200 OK | P1 |
| POS-019 | Get partner interactions | Partner has contacts with interactions | GET /api/partner/123/interactions | Interactions list returned | P1 |
| POS-020 | Get categories summary | Partners in multiple categories | GET /api/partner/categories-summary | Summary with counts per category | P1 |
| POS-021 | Get groups summary | Partners in multiple groups | GET /api/partner/groups-summary | Summary with counts per group | P1 |
| POS-022 | Get categorization overview | Partners exist | GET /api/partner/categorization-overview | Overview data returned | P1 |
| POS-023 | Detect duplicates — no duplicates | New partner "Acme Corp" | POST /api/partner/detect-duplicates with PartnerRequest | No duplicates found, create allowed | P1 |
| POS-024 | Create opportunity from partner | Partner 123 exists | POST /api/partner/123/create-opportunity | Opportunity created, linked to partner | P1 |
| POS-025 | Get partner opportunities | Partner 123 has 3 opportunities | GET /api/partner/123/opportunities | 3 opportunities returned | P1 |
| POS-026 | Get partner opportunities search | Partner has opportunities | GET /api/partner/123/opportunities/search?q=project | Filtered opportunities | P1 |
| POS-027 | Get partner by name | Partner "UNICEF" exists | GetPartnerByNameAsync(user, "UNICEF") | PartnerModel returned | P1 |
| POS-028 | Get partners for Gmail addon | Gmail request with partner-related emails | GetPartnersForGmailAddon(request, user) | Matching partners returned | P1 |
| POS-029 | Get total partner count | 50 partners exist | GetTotalPartnerCountAsync(user) | 50 returned | P1 |
| POS-030 | Get sample partner names | Partners exist | GetSamplePartnerNamesAsync(user, 5) | 5 partner names returned | P1 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Create — missing Name | PartnerRequest with Name=null or empty | 400 Bad Request, "Name is required" | P0 |
| NEG-002 | Create — PartnerLevyStatus DoesNotApply without ReasonForLevy | PartnerLevyStatus="DoesNotApply", ReasonForLevy=null | 400 Bad Request, validation error | P0 |
| NEG-003 | Create — PartnerLevyStatus PotentiallyNotApplied without ReasonForLevy | PartnerLevyStatus="PotentiallyNotApplied", ReasonForLevy=null | 400 Bad Request | P0 |
| NEG-004 | Create — invalid PartnerGroupId | PartnerGroupId=99999 (non-existent) | BusinessException or 400 | P0 |
| NEG-005 | Create — invalid PartnerCategoryId | PartnerCategoryId=99999 (non-existent) | FK violation or BusinessException | P0 |
| NEG-006 | Create — invalid LiaisonOfficeId | LiaisonOfficeId=99999 (non-existent) | FK violation or BusinessException | P0 |
| NEG-007 | Create — user without create permission | User lacks CanCreatePartners | 403 Forbidden | P0 |
| NEG-008 | Get partner — ID zero | GET /api/partner/0 | 404 or 400 | P1 |
| NEG-009 | Get partner — ID negative | GET /api/partner/-1 | 404 or 400 | P1 |
| NEG-010 | Get partner — non-existent ID | GET /api/partner/99999 | 404 Not Found | P0 |
| NEG-011 | Get partner — soft-deleted partner | Partner IsDeleted=true | 404 or not returned | P0 |
| NEG-012 | Update — non-existent ID | PUT /api/partner with Id=99999 | 404 or null | P0 |
| NEG-013 | Update — user without edit permission | User lacks CanEditPartners | 403 Forbidden | P0 |
| NEG-014 | Update — missing Id in UpdatePartnerRequest | UpdatePartnerRequest with Id=0 | Validation error | P0 |
| NEG-015 | Delete — non-existent ID | DELETE /api/partner/99999 | 404 or graceful handling | P1 |
| NEG-016 | Delete — already soft-deleted partner | Partner IsDeleted=true | Idempotent or 404 | P1 |
| NEG-017 | Delete — user without delete permission | User lacks CanDeletePartners | 403 Forbidden | P0 |
| NEG-018 | Activate — partner not in Draft | Partner Status=Active | 400, "Only Draft partners can be activated" | P0 |
| NEG-019 | Activate — missing mandatory fields | Partner missing PartnerShortDescription | 400, GetMissingMandatoryFieldsForActivation | P0 |
| NEG-020 | Activate — missing PartnerGroupId | Partner has no PartnerGroupId | Activation fails with missing fields | P0 |
| NEG-021 | Activate — missing LiaisonOfficeId | Partner has no LiaisonOfficeId | Activation fails | P0 |
| NEG-022 | Activate — user without permission | User lacks update permission | 403 Forbidden | P0 |
| NEG-023 | Close — partner not Active | Partner Status=Draft | 400, "Only Active partners can be closed" | P0 |
| NEG-024 | Archive — partner in Draft | Partner Status=Draft | 400, "Only Active or Closed can be archived" | P0 |
| NEG-025 | Approve — partner not Active | Partner Status=Draft | 400, "Only Active partners can be approved" | P0 |
| NEG-026 | Unapprove — partner not Approved | Partner PartnerApprovalStatus=NotApproved | 400, "Only approved partners can be unapproved" | P0 |
| NEG-027 | Unapprove — partner not Active | Partner Status=Closed | 400 | P0 |
| NEG-028 | Logo upload — non-existent partner | POST /api/partner/99999/logo | 404 | P1 |
| NEG-029 | Logo upload — invalid file type | IFormFile with .exe extension | 400, file type rejected | P0 |
| NEG-030 | Logo upload — file too large | IFormFile > 5MB | 400, size limit exceeded | P1 |
| NEG-031 | Pagination — PageIndex negative | GET /api/partner?pageIndex=-1 | 400 or default to 1 | P1 |
| NEG-032 | Pagination — PageSize zero | GET /api/partner?pageSize=0 | 400 or default | P1 |
| NEG-033 | Pagination — PageSize excessive | GET /api/partner?pageSize=100000 | Capped or 400 | P1 |
| NEG-034 | Get by partner group — invalid group ID | GET /api/partner/by-partner-group-id/99999 | Empty list or 404 | P1 |
| NEG-035 | Get by category — invalid code | GET /api/partner/by-partner-category-code/INVALID | Empty list or 404 | P1 |
| NEG-036 | Smart search — empty search text | PerformSmartSearchAsync(user, "") | Empty or validation error | P1 |
| NEG-037 | Smart search — null search text | PerformSmartSearchAsync(user, null) | ArgumentNullException or handled | P1 |
| NEG-038 | GetPartnerByName — non-existent name | GetPartnerByNameAsync(user, "NonExistentPartnerXYZ") | null returned | P1 |
| NEG-039 | GetPartnerByName — empty string | GetPartnerByNameAsync(user, "") | null or validation error | P1 |
| NEG-040 | Create — null PartnerRequest | CreatePartnerAsync(user, null) | ArgumentNullException | P0 |
| NEG-041 | Update — null UpdatePartnerRequest | UpdatePartnerAsync(user, null) | ArgumentNullException | P0 |
| NEG-042 | HasPermissionAsync — invalid userId | HasPermissionAsync(-1, 123, "Read") | false or error | P1 |
| NEG-043 | HasPermissionAsync — invalid partnerId | HasPermissionAsync(userId, 99999, "Read") | false | P1 |
| NEG-044 | HasPermissionAsync — invalid operation | HasPermissionAsync(userId, 123, "InvalidOp") | false | P1 |
| NEG-045 | GetPartnersForGmailAddon — null request | GetPartnersForGmailAddon(null, user) | ArgumentNullException | P1 |
| NEG-046 | Bulk upload — invalid file format | POST /api/partner/bulk-upload with .txt file | 400, invalid format | P1 |
| NEG-047 | Bulk upload — malformed CSV | CSV with wrong columns | 400, validation error | P1 |
| NEG-048 | Detect duplicates — null request | POST /api/partner/detect-duplicates with null body | 400 | P1 |
| NEG-049 | Create opportunity — non-existent partner | POST /api/partner/99999/create-opportunity | 404 | P1 |
| NEG-050 | Get opportunities — non-existent partner | GET /api/partner/99999/opportunities | 404 or empty | P1 |
| NEG-051 | Create — SQL injection in Name | Name="'; DROP TABLE Partner--" | Sanitized or rejected | P0 |
| NEG-052 | Create — XSS in PartnerLongDescription | PartnerLongDescription="<script>alert(1)</script>" | Sanitized or rejected | P0 |
| NEG-053 | Update — FK to deleted PartnerGroup | PartnerGroupId=5 (IsDeleted=true) | Error or handled | P1 |
| NEG-054 | Update — FK to deleted LiaisonOffice | LiaisonOfficeId=3 (deleted) | Error | P1 |
| NEG-055 | Get — expired auth token | Request with expired JWT | 401 Unauthorized | P0 |
| NEG-056 | Get — no auth token | Request without Authorization header | 401 Unauthorized | P0 |
| NEG-057 | GetPartnersWithSpecification — null specification | GetPartnersWithSpecificationAsync(user, null, request) | ArgumentNullException | P1 |
| NEG-058 | GetPartnersWithSpecification — null pagination | GetPartnersWithSpecificationAsync(user, spec, null) | ArgumentNullException or default | P1 |
| NEG-059 | Create — duplicate detection blocks (no confirm) | Partner "UNICEF" exists, create same without ConfirmDuplicateCreation | 200 with action=duplicateConfirmation | P1 |
| NEG-060 | Analyse file — invalid file | POST /api/partner/analyse-file with corrupt file | 400 | P1 |
| NEG-061 | Scan data — invalid input | POST /api/partner/scan-data with malformed data | 400 | P1 |
| NEG-062 | Get metadata info — unauthorized | User without read permission | 403 | P1 |
| NEG-063 | Create — ErpDimValue manually set (should be auto) | PartnerRequest with ErpDimValue=12345 | May be overwritten on approve | P2 |
| NEG-064 | Update — change Status directly (bypass workflow) | UpdatePartnerRequest with Status=Archived | Workflow validation or allowed per design | P1 |
| NEG-065 | Get partner — user from different org unit | User from OrgUnit B, partner in OrgUnit A | 403 or filtered out | P0 |
| NEG-066 | Activate — invalid ActivatePartnerRequest | ActivatePartnerAsync with null request | Handled (Notes optional) | P2 |
| NEG-067 | Close — invalid StatusChangeRequest | StatusChangeRequest with Status=null | Validation error | P1 |
| NEG-068 | Get sample names — count negative | GetSamplePartnerNamesAsync(user, -1) | 0 or default count | P1 |
| NEG-069 | Get sample names — count zero | GetSamplePartnerNamesAsync(user, 0) | Empty list | P1 |
| NEG-070 | PerformSmartSearch — maxResults zero | maxResults=0 | Empty or default 50 | P1 |
| NEG-071 | PerformSmartSearch — maxResults negative | maxResults=-1 | Default or error | P1 |
| NEG-072 | Create — PartnerFocalPointUserId invalid | PartnerFocalPointUserId=99999 (non-existent user) | FK error or handled | P1 |
| NEG-073 | Update — DueDiligenceExpiryDate in past | DueDiligenceExpiryDate=2020-01-01 | Accepted (data only) or validation | P2 |
| NEG-074 | Get partners — filterActive=false with no inactive | All partners active | Same as filterActive=true | P2 |
| NEG-075 | Create — OrganizationHierarchyIds invalid | List contains 99999 (non-existent) | Error or partial success | P1 |
| NEG-076 | Update — remove required Name | UpdatePartnerRequest with Name=null | Validation error | P0 |
| NEG-077 | Create — whitespace-only Name | Name="   " | Validation error | P1 |
| NEG-078 | Create — Name exceeds max length | Name with 500+ chars | Validation error (MaxLength) | P1 |
| NEG-079 | Update — concurrent modification | Two users update same partner | Optimistic concurrency or last-write-wins | P1 |
| NEG-080 | Delete — partner with active opportunities | Partner has 3 active opportunities | Soft delete succeeds, opportunities preserved | P1 |
| NEG-081 | Delete — partner with contacts | Partner has 10 contacts | Soft delete, contacts handling per design | P1 |
| NEG-082 | Get — ID max int | GET /api/partner/2147483647 | 404 or handled | P2 |
| NEG-083 | Create — malformed JSON | POST with invalid JSON body | 400 Bad Request | P1 |
| NEG-084 | Update — malformed JSON | PUT with invalid JSON | 400 Bad Request | P1 |
| NEG-085 | Get partners by group — user no permission | User lacks read for that group | 403 or filtered | P0 |
| NEG-086 | Get categorization overview — no partners | Empty database | Empty overview | P1 |
| NEG-087 | Get total count — user restricted scope | User has org filter | Count reflects scope | P1 |
| NEG-088 | Create — duplicate UniqueKey (system generated) | N/A — system assigns | No user input | P2 |
| NEG-089 | Logo upload — path traversal filename | Filename="../../../etc/passwd" | Rejected | P0 |
| NEG-090 | Create — rate limit exceeded | Too many create requests | 429 Too Many Requests | P2 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|---------------|-----|-----|--------|--------|----------|----------|
| BND-001 | Partner Name | 1 | 255 | "A" | 255 chars | 256 chars rejected | P1 |
| BND-002 | PartnerShortDescription | 0 | 100 | "" | 100 chars | 101 chars rejected | P1 |
| BND-003 | PartnerLongDescription | 0 | 4000 | "" | 4000 chars | 4001 rejected | P1 |
| BND-004 | PartnerApprovalReference | 0 | 500 | "" | 500 chars | 501 rejected | P1 |
| BND-005 | ReasonForLevy | 0 | 500 | "" | 500 chars | 501 rejected | P1 |
| BND-006 | LevyTreatment | 0 | 500 | "" | 500 chars | 501 rejected | P1 |
| BND-007 | ReasonForNoNewOpportunity | 0 | 500 | "" | 500 chars | 501 rejected | P1 |
| BND-008 | Partner Id | 1 | 2147483647 | 1 | Max int | Overflow handled | P1 |
| BND-009 | PartnerGroupId | 1 | 2147483647 | 1 | Valid FK | 0 = null | P1 |
| BND-010 | PartnerCategoryId | 1 | 2147483647 | 1 | Valid FK | 0 = null | P1 |
| BND-011 | LiaisonOfficeId | 1 | 2147483647 | 1 | Valid FK | 0 = null | P1 |
| BND-012 | ErpDimValue | 0 | 2147483647 | 0 | Max | Negative rejected | P1 |
| BND-013 | PartnerFocalPointUserId | 0 | 2147483647 | 0 (null) | Valid | Invalid FK | P1 |
| BND-014 | PageIndex | 1 | Max | 1 | Valid | 0 or negative | P1 |
| BND-015 | PageSize | 1 | 1000 | 1 | 1000 | 1001 capped/rejected | P1 |
| BND-016 | Pagination TotalCount | 0 | 2147483647 | 0 | Large | — | P2 |
| BND-017 | Smart search maxResults | 1 | 100 | 1 | 100 | 101 capped | P1 |
| BND-018 | Logo file size | 0 | 5MB | 0 bytes | 5MB | 5MB+1 rejected | P1 |
| BND-019 | Contacts per partner | 0 | 10000 | 0 | 10000 | — | P2 |
| BND-020 | Interactions per partner | 0 | 100000 | 0 | Large | — | P2 |
| BND-021 | OrganizationUnitRelationships | 0 | 100 | 0 | 100 | 101 | P1 |
| BND-022 | Documents per partner | 0 | 1000 | 0 | 1000 | — | P2 |
| BND-023 | Opportunities per partner | 0 | 10000 | 0 | Large | — | P2 |
| BND-024 | Partner list — empty DB | 0 | 0 | — | Empty list | — | P1 |
| BND-025 | Partner list — single record | 1 | 1 | 1 item | 1 item | — | P1 |
| BND-026 | Last page partial | Page 5 of 103 items, PageSize=20 | 3 items | 3 items | — | P1 |
| BND-027 | DueDiligenceExpiryDate | MinValue | MaxValue | Past | Future | — | P2 |
| BND-028 | DueDiligenceApprovalDate | MinValue | MaxValue | Past | Now | — | P2 |
| BND-029 | PartnerApprovalDate | — | — | Set on approve | — | — | P1 |
| BND-030 | Unicode in Name | — | — | "UNICEF" | "日本国連" | Emoji | P2 |
| BND-031 | Unicode in PartnerShortDescription | — | — | "UN" | "联合国" | — | P2 |
| BND-032 | Special chars in Name | — | — | "O'Brien" | "Smith & Co." | — | P2 |
| BND-033 | Empty search result | PerformSmartSearch "zzznonexistent" | 0 results | Empty list | — | P1 |
| BND-034 | Search — single match | One partner "UNICEF" | 1 result | 1 item | — | P1 |
| BND-035 | Filter by group — no partners | PartnerGroupId with 0 partners | Empty list | [] | — | P1 |
| BND-036 | Filter by category — no partners | Category with 0 partners | Empty list | [] | — | P1 |
| BND-037 | GetPartnerByName — case insensitive | "unicef" vs "UNICEF" | Match | Same partner | — | P1 |
| BND-038 | GetPartnerByName — exact match | "UNICEF" | 1 | Partner | — | P1 |
| BND-039 | Status Draft | — | — | New partner | Status=Draft | — | P0 |
| BND-040 | Status Active | — | — | After activate | Status=Active | — | P0 |
| BND-041 | Status Closed | — | — | After close | Status=Closed | — | P1 |
| BND-042 | Status Archived | — | — | After archive | Status=Archived | — | P1 |
| BND-043 | PartnerApprovalStatus NotApproved | — | — | Default | NotApproved | — | P1 |
| BND-044 | PartnerApprovalStatus Approved | — | — | After approve | Approved | — | P1 |
| BND-045 | Soft delete — IsDeleted boundary | false | true | false | true | — | P0 |
| BND-046 | CreatedDate | — | — | On create | UtcNow | — | P1 |
| BND-047 | LastModifiedDate | — | — | On create | On update | — | P1 |
| BND-048 | DeletedDate | — | — | null when active | Set on delete | — | P1 |
| BND-049 | UniqueKey Guid | — | — | NewGuid on create | Valid | — | P2 |
| BND-050 | PartnerKey Guid | — | — | NewGuid | Valid | — | P2 |
| BND-051 | Bulk upload — empty file | 0 rows | — | Empty CSV | 0 created | — | P1 |
| BND-052 | Bulk upload — single row | 1 row | — | 1 partner | 1 created | — | P1 |
| BND-053 | Bulk upload — max rows | 100 | 1000 | 100 | 1000 | Config limit | P1 |
| BND-054 | Gmail addon — empty email list | 0 emails | — | [] | Empty result | — | P1 |
| BND-055 | Gmail addon — single email | 1 email | — | 1 match | 1 partner | — | P1 |
| BND-056 | GetSamplePartnerNames — count 1 | 1 | — | 1 | 1 name | — | P1 |
| BND-057 | GetSamplePartnerNames — count 5 | 5 | — | 5 | 5 names | — | P1 |
| BND-058 | GetSamplePartnerNames — more than exist | 10 requested, 3 exist | 3 | 3 returned | — | P1 |
| BND-059 | OrderBy — Name ascending | — | — | A-Z | Correct order | — | P1 |
| BND-060 | OrderBy — Name descending | — | — | Z-A | Correct order | — | P1 |
| BND-061 | OrderBy — CreatedDate | — | — | Oldest first | Newest first | — | P1 |
| BND-062 | Export — pageSize max | export=true | int.MaxValue | All records | — | P1 |
| BND-063 | filterActive=false | Include inactive | — | Inactive included | — | P1 |
| BND-064 | filterActive=true (default) | Exclude inactive | — | Active only | — | P1 |
| BND-065 | PartnerGroupId filter in list | null | Valid ID | All groups | Filtered | — | P1 |
| BND-066 | CanCreateNewOpportunities — false | Unapproved | — | false | — | P1 |
| BND-067 | CanCreateNewOpportunities — true | Approved | — | true | — | P1 |
| BND-068 | UNAndStateEntity — false | Default | — | false | — | P2 |
| BND-069 | UNAndStateEntity — true | UN entity | — | true | — | P2 |
| BND-070 | KeyGlobalPartner — false/true | — | — | false | true | — | P2 |
| BND-071 | PooledFund — false/true | — | — | false | true | — | P2 |
| BND-072 | DueDiligenceRequired — null/Required | — | — | null | Required | — | P2 |
| BND-073 | PartnerLevyStatus — DoesNotApply | — | — | — | With ReasonForLevy | — | P1 |
| BND-074 | PartnerLevyStatus — PotentiallyApplied | — | — | — | No ReasonForLevy required | — | P1 |
| BND-075 | OrganizationHierarchyIds — empty list | [] | — | No org units | — | — | P1 |
| BND-076 | OrganizationHierarchyIds — single | [5] | — | 1 org unit | — | — | P1 |
| BND-077 | OrganizationHierarchyIds — multiple | [5,6,7] | — | 3 org units | — | — | P1 |
| BND-078 | GetPartnerWithContactsAndInteractions — no contacts | 0 contacts | — | Empty collections | — | — | P1 |
| BND-079 | GetPartnerWithContactsAndInteractions — no interactions | Contacts, 0 interactions | — | Empty interactions | — | — | P1 |
| BND-080 | IncludeInactive in smart search — false | Default | — | Active only | — | — | P1 |
| BND-081 | IncludeInactive in smart search — true | includeInactive=true | — | All partners | — | — | P1 |
| BND-082 | Create opportunity — partner with 0 opps | 0 existing | — | 1 created | — | — | P1 |
| BND-083 | Create opportunity — partner with many opps | 50 existing | — | 51 total | — | — | P1 |
| BND-084 | Logo — PNG format | — | — | image/png | Accepted | — | P1 |
| BND-085 | Logo — JPEG format | — | — | image/jpeg | Accepted | — | P1 |
| BND-086 | Logo — WebP format | — | — | image/webp | Accepted or rejected per config | P2 |
| BND-087 | Duplicate detection — threshold 0.5 | — | — | 0.5 | Low confidence | — | P2 |
| BND-088 | Duplicate detection — threshold 1.0 | — | — | 1.0 | Exact match | — | P2 |
| BND-089 | ConfirmDuplicateCreation — true | Bypass duplicate check | — | Create proceeds | — | — | P1 |
| BND-090 | GetTotalPartnerCount — 0 partners | Empty DB | — | 0 | — | — | P1 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Create sets Status=Draft | New partners must start as Draft | CreatePartnerAsync | Status=Draft | P0 |
| FUN-002 | Create sets CreatedBy | Audit trail | CreatePartnerAsync | CreatedBy=currentUserId | P0 |
| FUN-003 | Create sets CreatedDate | Audit trail | CreatePartnerAsync | CreatedDate=UtcNow | P0 |
| FUN-004 | Update sets LastModifiedBy | Audit trail | UpdatePartnerAsync | LastModifiedBy=currentUserId | P0 |
| FUN-005 | Update sets LastModifiedDate | Audit trail | UpdatePartnerAsync | LastModifiedDate=UtcNow | P0 |
| FUN-006 | Soft delete sets IsDeleted | Soft delete pattern | DeletePartnerAsync | IsDeleted=true | P0 |
| FUN-007 | Soft delete sets DeletedBy | Audit trail | DeletePartnerAsync | DeletedBy=currentUserId | P0 |
| FUN-008 | Soft delete sets DeletedDate | Audit trail | DeletePartnerAsync | DeletedDate=UtcNow | P0 |
| FUN-009 | Queries filter IsDeleted | Deleted partners excluded | GetPartners, GetPartner | !IsDeleted in filter | P0 |
| FUN-010 | Activate requires Name | HasMandatoryFieldsForActivation | Activate with Name=null | Fails | P0 |
| FUN-011 | Activate requires PartnerShortDescription | HasMandatoryFieldsForActivation | Activate without short desc | Fails | P0 |
| FUN-012 | Activate requires PartnerGroupId | HasMandatoryFieldsForActivation | Activate without group | Fails | P0 |
| FUN-013 | Activate requires LiaisonOfficeId | HasMandatoryFieldsForActivation | Activate without liaison | Fails | P0 |
| FUN-014 | Activate requires PartnerCategoryId | HasMandatoryFieldsForActivation | Activate without category | Fails | P0 |
| FUN-015 | Activate sets Status=Active | Workflow | ActivatePartnerAsync | Status=Active | P0 |
| FUN-016 | Close requires Status=Active | Only Active can close | Close from Draft | Fails | P0 |
| FUN-017 | Close sets Status=Closed | Workflow | ClosePartnerAsync | Status=Closed | P0 |
| FUN-018 | Archive requires Active or Closed | Workflow | Archive from Draft | Fails | P0 |
| FUN-019 | Archive sets Status=Archived | Workflow | ArchivePartnerAsync | Status=Archived | P0 |
| FUN-020 | Approve requires Status=Active | Only Active can approve | Approve from Draft | Fails | P0 |
| FUN-021 | Approve sets PartnerApprovalStatus | Workflow | ApprovePartnerAsync | PartnerApprovalStatus=Approved | P0 |
| FUN-022 | Approve assigns ErpDimValue | Auto-assign if null | ApprovePartnerAsync | ErpDimValue set | P0 |
| FUN-023 | Approve sets CanCreateNewOpportunities=true | Business rule | ApprovePartnerAsync | CanCreateNewOpportunities=true | P0 |
| FUN-024 | Approve sets PartnerApprovalDate | Audit | ApprovePartnerAsync | PartnerApprovalDate=UtcNow | P0 |
| FUN-025 | Approve sets PartnerApprovedBy | Audit trail | ApprovePartnerAsync | PartnerApprovedBy contains approver | P0 |
| FUN-026 | Unapprove requires Approved status | Workflow | Unapprove from NotApproved | Fails | P0 |
| FUN-027 | Unapprove sets CanCreateNewOpportunities=false | Business rule | UnapprovePartnerAsync | CanCreateNewOpportunities=false | P0 |
| FUN-028 | Unapprove clears PartnerApprovalStatus | Workflow | UnapprovePartnerAsync | PartnerApprovalStatus=NotApproved | P0 |
| FUN-029 | Name required for creation | Validation | Create with Name=null | Fails | P0 |
| FUN-030 | PartnerLevy ReasonForLevy when DoesNotApply | Validation | PartnerLevyStatus=DoesNotApply, no ReasonForLevy | Fails | P0 |
| FUN-031 | PartnerLevy ReasonForLevy when PotentiallyNotApplied | Validation | Same as above | Fails | P0 |
| FUN-032 | HasPermissionAsync checks Read | Permission | HasPermissionAsync(userId, id, "Read") | true/false per role | P0 |
| FUN-033 | HasPermissionAsync checks Update | Permission | HasPermissionAsync(userId, id, "Update") | true/false per role | P0 |
| FUN-034 | HasPermissionAsync checks Delete | Permission | HasPermissionAsync(userId, id, "Delete") | true/false per role | P0 |
| FUN-035 | GetPartners respects user scope | RBAC | GetPartners(userId) | Filtered by org/role | P0 |
| FUN-036 | GetPartner respects user scope | RBAC | GetPartner(userId, id) | 403 if no access | P0 |
| FUN-037 | Specification filters applied | Filtering | GetPartnersWithSpecificationAsync | Specification filters results | P0 |
| FUN-038 | Pagination returns correct page | Pagination | Page 2, PageSize 20 | Items 21-40 | P0 |
| FUN-039 | Pagination TotalCount accurate | Pagination | 103 total | TotalCount=103 | P0 |
| FUN-040 | Smart search ranks by relevance | Search | PerformSmartSearchAsync | Ranked results | P1 |
| FUN-041 | Smart search includes partner name | Search | Search "UN" | Partners with "UN" in name | P1 |
| FUN-042 | Smart search includes contacts | Search | Search contact name | Partner via contact | P1 |
| FUN-043 | Smart search includeInactive | Search | includeInactive=true | Deleted/inactive included | P1 |
| FUN-044 | GetPartnersByPartnerGroup filters | Filtering | GetPartnersByPartnerGroupAsync | Only partners in group | P1 |
| FUN-045 | GetPartnersByCategory filters | Filtering | GetPartnersByCategoryAsync | Only partners in category | P1 |
| FUN-046 | UpdatePartnerLogoAsync updates LogoUrl | Logo | Upload valid image | LogoUrl set | P1 |
| FUN-047 | GetPartnerWithContactsAndInteractions loads Contacts | Eager load | GetPartnerWithContactsAndInteractionsAsync | Contacts populated | P1 |
| FUN-048 | GetPartnerWithContactsAndInteractions loads Interactions | Eager load | Same | Interactions via contacts | P1 |
| FUN-049 | GetPartnerByName case-insensitive | Search | GetPartnerByNameAsync("unicef") | Matches "UNICEF" | P1 |
| FUN-050 | GetPartnersForGmailAddon filters by email | Gmail | Emails in request | Partners with matching contacts | P1 |
| FUN-051 | Create assigns UniqueKey | System | CreatePartnerAsync | UniqueKey=NewGuid | P1 |
| FUN-052 | Create assigns PartnerKey | System | CreatePartnerAsync | PartnerKey=NewGuid | P1 |
| FUN-053 | OrganizationHierarchyIds applied | Org units | Create/Update with OrganizationHierarchyIds | OrganizationUnitRelationships created | P1 |
| FUN-054 | Duplicate detection on create | Duplicate | Create similar partner | duplicateConfirmation or block | P1 |
| FUN-055 | ConfirmDuplicateCreation bypasses | Duplicate | Create with ConfirmDuplicateCreation=true | Creation proceeds | P1 |
| FUN-056 | Create opportunity links partner | Integration | create-opportunity | Opportunity.PartnerId=partnerId | P1 |
| FUN-057 | Get opportunities filters by partner | Integration | GET opportunities | Only partner's opportunities | P1 |
| FUN-058 | Categories summary aggregates | Summary | GET categories-summary | Count per category | P1 |
| FUN-059 | Groups summary aggregates | Summary | GET groups-summary | Count per group | P1 |
| FUN-060 | Categorization overview combines | Summary | GET categorization-overview | Combined view | P1 |
| FUN-061 | Metadata info returns schema | Metadata | GET metadata-info | Field info | P1 |
| FUN-062 | Detect duplicates returns matches | Duplicate | POST detect-duplicates | Duplicate candidates | P1 |
| FUN-063 | Bulk upload creates multiple | Bulk | POST bulk-upload with CSV | All valid rows created | P1 |
| FUN-064 | Scan data parses input | Scan | POST scan-data | Parsed data | P1 |
| FUN-065 | Analyse file extracts data | Analyse | POST analyse-file | Extracted partner data | P1 |
| FUN-066 | GetTotalPartnerCount excludes deleted | Count | GetTotalPartnerCountAsync | Only !IsDeleted | P1 |
| FUN-067 | GetSamplePartnerNames excludes deleted | Sample | GetSamplePartnerNamesAsync | Only active partners | P1 |
| FUN-068 | OrderBy applied | Sorting | orderBy=Name, ascending=true | Sorted results | P1 |
| FUN-069 | filterActive excludes inactive | Filter | filterActive=true | Only active | P1 |
| FUN-070 | Export removes pagination limit | Export | export=true | PageSize=int.MaxValue | P1 |
| FUN-071 | Permissions endpoint returns flags | Permissions | GET permissions | canView, canEdit, canDelete | P0 |
| FUN-072 | Permissions respect entity state | Permissions | Partner Closed | canEdit may be false | P1 |
| FUN-073 | Permissions respect user role | Permissions | Different roles | Different flags | P0 |
| FUN-074 | ErpDimValue not overwritten if set | Approve | Partner has ErpDimValue, Approve | Keeps existing value | P1 |
| FUN-075 | Approve overwrites ErpDimValue if null | Approve | Partner ErpDimValue=null, Approve | Assigns next value | P1 |
| FUN-076 | CanBeActivated checks fields | Validation | Partner.CanBeActivated() | true if all mandatory | P1 |
| FUN-077 | GetMissingMandatoryFieldsForActivation | Validation | Partner missing fields | List of missing | P1 |
| FUN-078 | HasMinimumFieldsForCreation | Validation | Name only | true if Name set | P1 |
| FUN-079 | IsApproved property | Computed | PartnerApprovalStatus=Approved | IsApproved=true | P1 |
| FUN-080 | IsDueDiligenceExpiring | Computed | Expiry within 6 months | true | P1 |
| FUN-081 | GetPrimaryOrgUnitId | Org units | First active relationship | OrganizationHierarchyId | P1 |
| FUN-082 | GetAllOrgUnitIds | Org units | All active relationships | List of IDs | P1 |
| FUN-083 | AddOrganizationUnitRelationship | Org units | AddOrganizationUnitRelationship(org) | Relationship added | P1 |
| FUN-084 | RemoveOrganizationUnitRelationship | Org units | RemoveOrganizationUnitRelationship(id) | Relationship removed | P1 |
| FUN-085 | GetRecentInteractions | Interactions | GetRecentInteractions(10) | Last 10 | P1 |
| FUN-086 | GetInteractionsByContact | Interactions | GetInteractionsByContact() | Grouped by contact | P1 |
| FUN-087 | GetTotalInteractionsCount | Interactions | Partner with 15 interactions | 15 | P1 |
| FUN-088 | GetLastInteractionDate | Interactions | Partner with interactions | Most recent date | P1 |
| FUN-089 | First5ContactsByDate | Contacts | Partner with 10 contacts | First 5 by date | P1 |
| FUN-090 | PartnerOrgUnit computed | Org units | Partner with org relationships | Comma-separated names | P1 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Full CRUD lifecycle | Create→Get→Update→Delete | Partner | All operations succeed | P0 |
| INT-002 | Create then Get by ID | Create, GetPartner | Partner | Created partner returned | P0 |
| INT-003 | Create then Update | Create, Update | Partner | Updated fields persisted | P0 |
| INT-004 | Create then Soft Delete | Create, Delete | Partner | IsDeleted=true, Get returns 404 | P0 |
| INT-005 | List with pagination through pages | GetPartners pages 1,2,3 | Partner, Pagination | Correct items per page | P0 |
| INT-006 | Search then Get detail | PerformSmartSearch, GetPartner | Partner | Detail matches search hit | P0 |
| INT-007 | Filter by group then update | GetPartnersByPartnerGroup, Update | Partner, PartnerTree | Update succeeds | P1 |
| INT-008 | Filter by category then activate | GetPartnersByCategory, Activate | Partner, PartnerCategory | Activation succeeds | P1 |
| INT-009 | Activate then Approve | ActivatePartner, ApprovePartner | Partner | Status=Active, Approved | P0 |
| INT-010 | Approve then Create Opportunity | Approve, create-opportunity | Partner, Opportunity | Opportunity created | P0 |
| INT-011 | Partner with Contacts and Interactions | Create partner, add contacts, add interactions, GetPartnerWithContactsAndInteractions | Partner, Contact, Interaction | Full graph loaded | P0 |
| INT-012 | Partner with OrganizationUnitRelationships | Create with OrganizationHierarchyIds | Partner, OrganizationUnitRelationship, OrganizationHierarchy | Relationships created | P0 |
| INT-013 | Partner with LiaisonOffice | Create with LiaisonOfficeId | Partner, LiaisonOffice | LiaisonOffice loaded | P1 |
| INT-014 | Partner with PartnerGroup | Create with PartnerGroupId | Partner, PartnerTree | PartnerGroup loaded | P1 |
| INT-015 | Partner with PartnerCategory | Create with PartnerCategoryId | Partner, PartnerCategory | Category loaded | P1 |
| INT-016 | Partner with Documents | Create partner, add documents | Partner, Document | Documents associated | P1 |
| INT-017 | Partner with PartnerFocalPointUser | Create with PartnerFocalPointUserId | Partner, PAOUser | User loaded | P1 |
| INT-018 | Upload logo then Get | Upload logo, GetPartner | Partner, Storage | LogoUrl in response | P1 |
| INT-019 | Create opportunity from partner | create-opportunity | Partner, Opportunity | Opportunity.PartnerId set | P1 |
| INT-020 | Get partner opportunities | GET opportunities | Partner, Opportunity | Partner's opportunities | P1 |
| INT-021 | Get partner opportunities search | GET opportunities/search | Partner, Opportunity | Filtered opportunities | P1 |
| INT-022 | Duplicate detection then create with confirm | detect-duplicates, create with ConfirmDuplicateCreation | Partner, AiContextualService | Second partner created | P1 |
| INT-023 | Bulk upload then list | bulk-upload, GetPartners | Partner | All created visible | P1 |
| INT-024 | Scan data then create | scan-data, create | Partner | Data from scan used | P1 |
| INT-025 | Analyse file then create | analyse-file, create | Partner, File | Extracted data used | P1 |
| INT-026 | Get permissions then conditional update | GET permissions, if canEdit then PUT | Partner, Authorization | Update only if permitted | P0 |
| INT-027 | Get interactions via partner | GET partner/123/interactions | Partner, Contact, Interaction | All interactions | P1 |
| INT-028 | Categories summary with real data | Create partners in categories, GET summary | Partner, PartnerCategory | Correct counts | P1 |
| INT-029 | Groups summary with real data | Create partners in groups, GET summary | Partner, PartnerTree | Correct counts | P1 |
| INT-030 | Categorization overview with data | Create partners, GET overview | Partner, Category, Group | Overview correct | P1 |
| INT-031 | Gmail addon flow | GetPartnersForGmailAddon with emails | Partner, Contact, GmailRelatedRecordsRequest | Matching partners | P1 |
| INT-032 | GetPartnerByName then Update | GetPartnerByNameAsync, UpdatePartnerAsync | Partner | Update by name lookup | P1 |
| INT-033 | GetTotalPartnerCount after create | Create, GetTotalPartnerCount | Partner | Count incremented | P1 |
| INT-034 | GetSamplePartnerNames after create | Create "TestPartner", GetSamplePartnerNames | Partner | "TestPartner" in sample | P1 |
| INT-035 | Advanced search with multiple filters | new-advanced-search with filters | Partner, Specification | Filtered results | P1 |
| INT-036 | Search fields endpoint | GET search-fields | Partner, EntityConfiguration | Search fields returned | P1 |
| INT-037 | Metadata info for create form | GET metadata-info | Partner, EntityConfiguration | Schema for UI | P1 |
| INT-038 | Detect duplicates with existing | Create "UNICEF", detect with similar | Partner, AiContextualService | Duplicates found | P1 |
| INT-039 | Close then Archive | Close, Archive | Partner | Status=Archived | P1 |
| INT-040 | Unapprove then Approve again | Unapprove, Approve | Partner | Re-approved | P1 |
| INT-041 | Partner with multiple contacts | Create, add 5 contacts | Partner, Contact | 5 contacts | P1 |
| INT-042 | Partner with multiple org units | Create with [1,2,3] | Partner, OrganizationUnitRelationship | 3 relationships | P1 |
| INT-043 | Soft delete partner with contacts | Delete partner, contacts exist | Partner, Contact | Partner deleted, contacts handling per design | P1 |
| INT-044 | Soft delete partner with opportunities | Delete partner, opportunities exist | Partner, Opportunity | Partner deleted, opportunities per design | P1 |
| INT-045 | Update org unit relationships | Update with new OrganizationHierarchyIds | Partner, OrganizationUnitRelationship | Relationships updated | P1 |
| INT-046 | Update PartnerGroup | Update PartnerGroupId | Partner, PartnerTree | Group changed | P1 |
| INT-047 | Update LiaisonOffice | Update LiaisonOfficeId | Partner, LiaisonOffice | Office changed | P1 |
| INT-048 | Update PartnerCategory | Update PartnerCategoryId | Partner, PartnerCategory | Category changed | P1 |
| INT-049 | API Create→Controller→Manager→DB | POST /api/partner full stack | Controller, Manager, DbContext, Partner | 201, DB record | P0 |
| INT-050 | API Get→Manager→DB | GET /api/partner/123 | Controller, Manager, DbContext | 200, PartnerModel | P0 |
| INT-051 | API Update→Manager→DB | PUT /api/partner | Controller, Manager, DbContext | 200, updated | P0 |
| INT-052 | API Delete→Manager→DB | DELETE /api/partner/123 | Controller, Manager, DbContext | 200, soft delete | P0 |
| INT-053 | API List with pagination | GET /api/partner?pageIndex=2&pageSize=10 | Controller, Manager | PaginationResponse | P0 |
| INT-054 | API Search | GET /api/partner/search?q=UN | Controller, Manager, SearchService | Search results | P1 |
| INT-055 | API Activate | POST /api/partner/123/activate | Controller, Manager | Status=Active | P0 |
| INT-056 | API Close | POST /api/partner/123/close | Controller, Manager | Status=Closed | P1 |
| INT-057 | API Archive | POST /api/partner/123/archive | Controller, Manager | Status=Archived | P1 |
| INT-058 | API Approve | POST /api/partner/123/approve | Controller, Manager | Approved | P0 |
| INT-059 | API Unapprove | POST /api/partner/123/unapprove | Controller, Manager | NotApproved | P1 |
| INT-060 | API Permissions | GET /api/partner/123/permissions | Controller, AuthorizationService | Permission flags | P0 |
| INT-061 | API Logo upload | POST /api/partner/123/logo | Controller, Manager, Storage | LogoUrl updated | P1 |
| INT-062 | API By group | GET /api/partner/by-partner-group-id/5 | Controller, Manager | Filtered list | P1 |
| INT-063 | API By category | GET /api/partner/by-partner-category-code/GOV | Controller, Manager | Filtered list | P1 |
| INT-064 | API Categories summary | GET /api/partner/categories-summary | Controller, Manager | Summary | P1 |
| INT-065 | API Groups summary | GET /api/partner/groups-summary | Controller, Manager | Summary | P1 |
| INT-066 | API Categorization overview | GET /api/partner/categorization-overview | Controller, Manager | Overview | P1 |
| INT-067 | API Detect duplicates | POST /api/partner/detect-duplicates | Controller, AiContextualService | Duplicate result | P1 |
| INT-068 | API Bulk upload | POST /api/partner/bulk-upload | Controller, Manager | Bulk create | P1 |
| INT-069 | API Create opportunity | POST /api/partner/123/create-opportunity | Controller, PartnerManager, OpportunityManager | Opportunity created | P1 |
| INT-070 | API Get opportunities | GET /api/partner/123/opportunities | Controller, OpportunityManager | Opportunities list | P1 |
| INT-071 | UNOPS override — UNOPSPartnerManager | IsUNOPSOverride=true | UNOPSPartnerManager, UNOPSPartner | UNOPS logic applied | P1 |
| INT-072 | UNOPS override — UNOPSPartner entity | Create partner | UNOPSPartner extends Partner | UNOPS fields available | P1 |
| INT-073 | Specification with composite filters | PartnerCompositeSpecification | Partner, Specification | Combined filters | P1 |
| INT-074 | Export flow | GET with export=true | Partner, Export | Full dataset | P1 |
| INT-075 | filterActive=false flow | GET with filterActive=false | Partner | Inactive included | P1 |
| INT-076 | PartnerGroupId query param | GET ?partnerGroupId=5 | Partner | Filtered | P1 |
| INT-077 | OrderBy and Ascending params | GET ?orderBy=Name&ascending=true | Partner | Sorted | P1 |
| INT-078 | Audit log on create | Create partner | Partner, AuditLog | Audit entry created | P1 |
| INT-079 | Audit log on update | Update partner | Partner, AuditLog | Audit entry | P1 |
| INT-080 | Audit log on delete | Delete partner | Partner, AuditLog | Audit entry | P1 |
| INT-081 | Audit log on approve | Approve partner | Partner, AuditLog | Audit entry | P1 |
| INT-082 | AccessControlled attribute | Request without permission | Controller, AccessControlled | 403 | P0 |
| INT-083 | IAP authentication | Request without token | Controller, Auth | 401 | P0 |
| INT-084 | PartnerFilterRequest mapping | PartnerFilterRequest to Specification | PartnerFilterRequest, Specification | Correct filters | P1 |
| INT-085 | PartnerRequest to Entity mapping | Create PartnerRequest | AutoMapper, Partner | Entity populated | P0 |
| INT-086 | Partner to PartnerModel mapping | GetPartner | AutoMapper, PartnerModel | Model populated | P0 |
| INT-087 | UpdatePartnerRequest to Entity | Update | AutoMapper, Partner | Entity updated | P0 |
| INT-088 | Error handling — BusinessException | Invalid operation | Manager | 400 with message | P1 |
| INT-089 | Error handling — KeyNotFoundException | Get non-existent | Manager | 404 | P1 |
| INT-090 | Error handling — UnauthorizedAccessException | No permission | Authorization | 403 | P0 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | SQL injection in Name | `'; DROP TABLE Partner--` | Create Name | Sanitized/Rejected | P0 |
| SEC-002 | SQL injection in search | `' OR 1=1--` | Search text | Sanitized | P0 |
| SEC-003 | SQL injection in PartnerLongDescription | `'; DELETE FROM Partner WHERE 1=1--` | Create | Sanitized | P0 |
| SEC-004 | XSS in Name | `<script>alert(1)</script>` | Create/Display | Sanitized | P0 |
| SEC-005 | XSS in PartnerShortDescription | `<img src=x onerror=alert(1)>` | Create | Sanitized | P0 |
| SEC-006 | XSS in Notes/ReasonForLevy | `<script>document.cookie</script>` | Create | Sanitized | P0 |
| SEC-007 | Create without auth | No JWT | POST /api/partner | 401 Unauthorized | P0 |
| SEC-008 | Create without permission | User lacks create | POST /api/partner | 403 Forbidden | P0 |
| SEC-009 | Get without auth | No JWT | GET /api/partner/123 | 401 Unauthorized | P0 |
| SEC-010 | Get without permission | User lacks read | GET /api/partner/123 | 403 Forbidden | P0 |
| SEC-011 | Update without permission | User lacks update | PUT /api/partner | 403 Forbidden | P0 |
| SEC-012 | Delete without permission | User lacks delete | DELETE /api/partner/123 | 403 Forbidden | P0 |
| SEC-013 | IDOR — access other user's partner | Change ID in URL | GET /api/partner/456 (user owns 123) | 403 or 404 | P0 |
| SEC-014 | IDOR — update other's partner | PUT with Id=456 | Update | 403 | P0 |
| SEC-015 | IDOR — delete other's partner | DELETE /api/partner/456 | Delete | 403 | P0 |
| SEC-016 | IDOR — get permissions other's partner | GET /api/partner/456/permissions | Permissions | 403 | P0 |
| SEC-017 | IDOR — upload logo other's partner | POST /api/partner/456/logo | Logo | 403 | P0 |
| SEC-018 | IDOR — activate other's partner | POST /api/partner/456/activate | Activate | 403 | P0 |
| SEC-019 | IDOR — approve other's partner | POST /api/partner/456/approve | Approve | 403 | P0 |
| SEC-020 | Mass assignment — ErpDimValue | Include ErpDimValue in create | PartnerRequest | Ignored or overwritten | P1 |
| SEC-021 | Mass assignment — CreatedBy | Include CreatedBy | Create | Ignored | P1 |
| SEC-022 | Mass assignment — IsDeleted | Include IsDeleted=false | Update | Ignored | P1 |
| SEC-023 | Mass assignment — UniqueKey | Include UniqueKey | Create | Ignored | P1 |
| SEC-024 | Path traversal in logo filename | `../../../etc/passwd` | Logo upload | Rejected | P0 |
| SEC-025 | Logo — executable file | .exe as image | Logo upload | Rejected | P0 |
| SEC-026 | Logo — oversized file | 100MB file | Logo upload | Rejected | P0 |
| SEC-027 | Expired token | Expired JWT | Any endpoint | 401 | P0 |
| SEC-028 | Tampered token | Modified JWT signature | Any endpoint | 401 | P0 |
| SEC-029 | Token from wrong issuer | Token from other system | Any endpoint | 401 | P0 |
| SEC-030 | Role escalation — add admin claim | Forge admin role in token | Create | 403 or validated | P0 |
| SEC-031 | Horizontal privilege escalation | User A accesses User B's org data | GetPartners | Filtered by scope | P0 |
| SEC-032 | Vertical privilege escalation | Standard user approves | Approve | 403 | P0 |
| SEC-033 | Bulk upload — malicious CSV | CSV with script in cells | bulk-upload | Sanitized | P1 |
| SEC-034 | Analyse file — malicious file | File with embedded executable | analyse-file | Rejected | P1 |
| SEC-035 | Detect duplicates — injection in request | Malicious PartnerRequest | detect-duplicates | Sanitized | P1 |
| SEC-036 | Create opportunity — IDOR | partnerId=456 (other's) | create-opportunity | 403 | P0 |
| SEC-037 | Get opportunities — IDOR | partnerId=456 | GET opportunities | 403 | P0 |
| SEC-038 | Get interactions — IDOR | id=456 | GET interactions | 403 | P0 |
| SEC-039 | By group — scope bypass | by-partner-group-id for unauthorized group | GET | 403 or filtered | P0 |
| SEC-040 | By category — scope bypass | by-partner-category-code | GET | 403 or filtered | P0 |
| SEC-041 | Sensitive data in error message | Trigger error | Response | No stack trace, no internal details | P1 |
| SEC-042 | PartnerApprovedBy — user info exposure | Approve | PartnerApprovedBy | Contains approver, no sensitive data | P1 |
| SEC-043 | ErpDimValue — business data exposure | Get partner | Response | Only if user has access | P0 |
| SEC-044 | Rate limiting — create flood | 1000 POST /api/partner in 1 min | Create | 429 or throttled | P1 |
| SEC-045 | Rate limiting — search flood | 1000 searches in 1 min | Search | 429 or throttled | P1 |
| SEC-046 | CSRF — state-changing without token | POST from external site | Create | 401/403 | P0 |
| SEC-047 | HTTP verb tampering | GET for delete | DELETE | 405 Method Not Allowed | P1 |
| SEC-048 | Content-Type bypass | application/xml for JSON endpoint | Create | 400 or rejected | P1 |
| SEC-049 | Parameter pollution | id=123&id=456 | GET | First or validation | P1 |
| SEC-050 | Authorization handler — PartnerAuthorizationHandler | All operations | AuthorizationContextWrapper | Handler invoked | P0 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Two users create same name | User A and B create "UNICEF" simultaneously | Both succeed or duplicate detection | P1 |
| CON-002 | Two users update same partner | User A and B update partner 123 | Last write wins or optimistic lock | P1 |
| CON-003 | Update and delete same partner | User A updates, User B deletes | One succeeds, other gets 404 or conflict | P1 |
| CON-004 | Activate and Update same partner | User A activates, User B updates | Consistent final state | P1 |
| CON-005 | Approve and Unapprove same partner | User A approves, User B unapproves | One succeeds, consistent state | P1 |
| CON-006 | Concurrent logo uploads | Two users upload logo to same partner | Last upload wins | P1 |
| CON-007 | Concurrent bulk uploads | Two users bulk upload | Both complete, no corruption | P1 |
| CON-008 | Create and Get same partner | User A creates, User B gets by ID | Get may return 404 until committed | P1 |
| CON-009 | Delete and Get | User A deletes, User B gets | Get returns 404 | P1 |
| CON-010 | Update and Get | User A updates, User B gets | Get returns latest or previous | P1 |
| CON-011 | Concurrent list requests | 10 users GET /api/partner simultaneously | All succeed, correct data | P1 |
| CON-012 | Concurrent search requests | 10 users search simultaneously | All succeed | P1 |
| CON-013 | Create opportunity concurrent | Two users create opportunity from same partner | Both succeed, two opportunities | P1 |
| CON-014 | GetTotalPartnerCount during create | User A creates, User B gets count | Count may be N or N+1 | P1 |
| CON-015 | ErpDimValue assignment race | Two users approve different partners | Unique ErpDimValue per partner | P1 |
| CON-016 | Pagination during create | User A creates, User B pages | New partner may appear on last page | P1 |
| CON-017 | Soft delete and list | User A deletes, User B lists | Deleted not in list | P1 |
| CON-018 | Update org relationships concurrent | Two users update OrganizationHierarchyIds | Consistent final state | P1 |
| CON-019 | Close and Archive concurrent | User A closes, User B archives | One may fail (wrong state) | P1 |
| CON-020 | Transaction isolation — create rollback | Create fails mid-transaction | No partial partner | P1 |
| CON-021 | Transaction isolation — update rollback | Update fails | Original data preserved | P1 |
| CON-022 | Double submit — create | User double-clicks create | One partner created or duplicate handling | P1 |
| CON-023 | Double submit — update | User double-clicks save | One update applied | P1 |
| CON-024 | Cache poisoning — stale partner | Partner updated, cached get | Cache invalidated or TTL | P2 |
| CON-025 | DbContext concurrency | Parallel queries | No DbContext disposed errors | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | Partner.HasMinimumFieldsForCreation — Name set | Validation | Partner with Name="X" | true | P1 |
| UNT-002 | Partner.HasMinimumFieldsForCreation — Name null | Validation | Partner with Name=null | false | P1 |
| UNT-003 | Partner.CanBeActivated — all fields set | Validation | Partner with all mandatory | true | P1 |
| UNT-004 | Partner.CanBeActivated — missing PartnerShortDescription | Validation | Partner without short desc | false | P1 |
| UNT-005 | Partner.CanBeActivated — missing LiaisonOfficeId | Validation | Partner without liaison | false | P1 |
| UNT-006 | Partner.GetMissingMandatoryFieldsForActivation | Validation | Partner missing Name, Group | ["Name","Partner Group"] | P1 |
| UNT-007 | Partner.IsApproved — Approved status | Status | PartnerApprovalStatus=Approved | true | P1 |
| UNT-008 | Partner.IsApproved — NotApproved status | Status | PartnerApprovalStatus=NotApproved | false | P1 |
| UNT-009 | Partner.IsDueDiligenceExpiring — 5 months to expiry | Computed | DueDiligenceExpiryDate=now+5mo | true | P1 |
| UNT-010 | Partner.IsDueDiligenceExpiring — 7 months to expiry | Computed | DueDiligenceExpiryDate=now+7mo | false | P1 |
| UNT-011 | Partner.GetPrimaryOrgUnitId — has relationship | Org units | 1 active relationship | OrganizationHierarchyId | P1 |
| UNT-012 | Partner.GetPrimaryOrgUnitId — no relationship | Org units | 0 relationships | null | P1 |
| UNT-013 | Partner.GetAllOrgUnitIds | Org units | 3 relationships | [1,2,3] | P1 |
| UNT-014 | Partner.GetTotalInteractionsCount | Interactions | 5 contacts, 3+2+4 interactions | 9 | P1 |
| UNT-015 | Partner.GetLastInteractionDate | Interactions | Interactions with dates | Most recent | P1 |
| UNT-016 | Partner.GetRecentInteractions(5) | Interactions | 10 interactions | 5 returned | P1 |
| UNT-017 | PartnerLevyValidationAttribute — DoesNotApply no Reason | Validation | PartnerLevyStatus=DoesNotApply, Reason=null | Invalid | P1 |
| UNT-018 | PartnerLevyValidationAttribute — DoesNotApply with Reason | Validation | PartnerLevyStatus=DoesNotApply, Reason="X" | Valid | P1 |
| UNT-019 | PartnerLevyValidationAttribute — PotentiallyApplied | Validation | PartnerLevyStatus=PotentiallyApplied | Valid (no Reason required) | P1 |
| UNT-020 | Partner.ActivatePartner — from Draft | Workflow | Status=Draft, all fields | Status=Active | P1 |
| UNT-021 | Partner.ApprovePartner — sets ErpDimValue | Workflow | ErpDimValue=null, ApprovePartner(1,"Admin",100) | ErpDimValue=100 | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetPartner by ID | GET /api/partner/123 | < 200ms | P0 |
| PRF-002 | List partners — 20 per page | GET /api/partner?pageSize=20 | < 500ms | P0 |
| PRF-003 | List partners — 100 per page | GET /api/partner?pageSize=100 | < 1000ms | P1 |
| PRF-004 | Search — simple query | GET /api/partner/search?q=UN | < 1000ms | P0 |
| PRF-005 | Smart search — 50 results | PerformSmartSearchAsync, maxResults=50 | < 2000ms | P1 |
| PRF-006 | GetPartnerWithContactsAndInteractions | Partner with 10 contacts, 50 interactions | < 1000ms | P1 |
| PRF-007 | Get partners by group | GET by-partner-group-id/5 | < 500ms | P1 |
| PRF-008 | Get partners by category | GET by-partner-category-code/GOV | < 500ms | P1 |
| PRF-009 | Categories summary | GET categories-summary | < 500ms | P1 |
| PRF-010 | Groups summary | GET groups-summary | < 500ms | P1 |
| PRF-011 | Create partner | POST /api/partner | < 500ms | P0 |
| PRF-012 | Update partner | PUT /api/partner | < 500ms | P0 |
| PRF-013 | Logo upload | POST logo | < 2000ms | P1 |
| PRF-014 | Detect duplicates | POST detect-duplicates | < 3000ms | P1 |
| PRF-015 | Bulk upload — 50 rows | POST bulk-upload | < 10000ms | P1 |
| PRF-016 | GetTotalPartnerCount | GetTotalPartnerCountAsync | < 500ms | P1 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | List partners sustained | 20 req/s | 5 min | 95% < 500ms, 0% errors | P0 |
| LDT-002 | Get partner by ID sustained | 50 req/s | 5 min | 95% < 200ms, 0% errors | P0 |
| LDT-003 | Search sustained | 10 req/s | 5 min | 95% < 1000ms, 0% errors | P0 |
| LDT-004 | Create partners sustained | 5 req/s | 5 min | 95% < 500ms, 0% errors | P0 |
| LDT-005 | Mixed read workload | 30 req/s (list+get+search) | 5 min | 95% < 500ms | P0 |
| LDT-006 | Spike — list 2x | 40 req/s for 1 min | 1 min | No errors, recovery | P1 |
| LDT-007 | Spike — get 3x | 150 req/s for 30 sec | 30 sec | No errors | P1 |
| LDT-008 | Stress — find limit | Ramp to 100 req/s | Until failure | Document limit | P1 |
| LDT-009 | Endurance — 1 hour | 10 req/s mixed | 1 hour | No memory leak, stable latency | P1 |
| LDT-010 | Recovery after load | 50 req/s for 2 min, then 0 | 5 min | Latency returns to baseline | P1 |

---

## Traceability Matrix

| Requirement / Area | Test Cases Covering |
|--------------------|---------------------|
| CRUD operations | POS-001 to POS-006, INT-001 to INT-005 |
| Search & filtering | POS-007 to POS-011, FUN-037 to FUN-045 |
| Workflow (Activate/Close/Archive) | POS-012 to POS-016, FUN-010 to FUN-028 |
| Approve/Unapprove | POS-015, POS-016, FUN-020 to FUN-028 |
| Permissions | POS-017, FUN-032 to FUN-036, SEC-007 to SEC-012 |
| Logo upload | POS-018, FUN-046, INT-018 |
| Soft delete | POS-006, FUN-006 to FUN-009 |
| Org unit relationships | INT-012, FUN-053, FUN-081 to FUN-084 |
| Duplicate detection | POS-023, NEG-059, FUN-054 to FUN-055 |
| Bulk upload | NEG-046, NEG-047, INT-023, INT-068 |
| Gmail addon | POS-028, INT-031, NEG-045 |
| Opportunities linkage | POS-024 to POS-026, INT-019 to INT-021 |
| Pagination | POS-007, FUN-038 to FUN-039, BND-014 to BND-016 |
| UNOPS override | INT-071, INT-072 |

---

## Test Environment Setup

**Prerequisites:**
- PostgreSQL database with Partner, Contact, Interaction, Opportunity, PartnerTree, PartnerCategory, LiaisonOffice, OrganizationHierarchy entities
- Test user with appropriate permissions (CanCreatePartners, CanEditPartners, CanDeletePartners, CanViewPartners)
- Admin user for Approve/Unapprove tests
- Valid LiaisonOffice, PartnerGroup, PartnerCategory for create/activate
- File storage configured for logo upload
- IAP authentication configured

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
