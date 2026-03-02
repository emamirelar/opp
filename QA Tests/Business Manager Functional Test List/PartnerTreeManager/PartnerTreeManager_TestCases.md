# PartnerTreeManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/PartnerTreeManager`  
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

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

### 3:1 Ratio Compliance Check
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

**PartnerTreeManager** manages partner category and group hierarchy structure using Code/Parent string matching (not FK). Key responsibilities: CRUD operations (CreatePartnerTreeAsync, UpdatePartnerTreeAsync, DeletePartnerTreeAsync), hierarchical tree building (BuildHierarchy), category vs group distinction (PartnerCategoryEditable, PartnerGroupEditable), recursive descendants (GetAllDescendantsAsync), sorting (sortBy, ascending), permissions, AI integration (GetBasicPartnerCategoryDetailsAsync, GetBasicPartnerGroupDetailsAsync, GetPartnerCategoryNewsDetailsAsync, GetPartnerGroupNewsDetailsAsync), partner assignments to groups, categorization overview, and partner filtering by group/category.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Create partner category | User has create permission | CreatePartnerTreeAsync(ClaimsPrincipal, model with Code="NGO", Type="Level_1", Parent="") | Partner tree created with ID, Name, Code | P0 |
| POS-002 | Create partner group under category | Category "GOV" exists | CreatePartnerTreeAsync(model with Code="GOV-001", Type="Level_2", Parent="GOV") | Group created under GOV category | P0 |
| POS-003 | Get partner trees hierarchical | Trees exist | GetPartnerTreesAsync(user, "Name", true) | Hierarchical tree returned | P0 |
| POS-004 | Get partner tree by ID | Tree exists | GetPartnerTreeAsync(user, 5) | PartnerTreeModel returned | P0 |
| POS-005 | Update partner tree | Tree exists | UpdatePartnerTreeAsync(user, model with Id=5, Description="Updated") | Tree updated | P0 |
| POS-006 | Delete partner tree | Tree exists, no partners assigned | DeletePartnerTreeAsync(user, 5) | Tree soft-deleted | P0 |
| POS-007 | Get posted partner trees | Trees exist | GetPostedPartnerTrees() | ExternalPartnerTreeModel list | P1 |
| POS-008 | Get posted partner tree by ID | Tree exists | GetPostedPartnerTree(5) | ExternalPartnerTreeModel with EligibleEntities | P1 |
| POS-009 | Get category and group structure | Trees exist | GetCategoryAndGroupStructureAsync(user) | Categories with children groups | P0 |
| POS-010 | Sort by Name ascending | Trees exist | GetPartnerTreesAsync(user, "Name", true) | Sorted A→Z | P1 |
| POS-011 | Sort by Name descending | Trees exist | GetPartnerTreesAsync(user, "Name", false) | Sorted Z→A | P1 |
| POS-012 | Sort by Code ascending | Trees exist | GetPartnerTreesAsync(user, "Code", true) | Sorted by Code | P1 |
| POS-013 | API POST create | Auth | POST /api/partner-tree with valid body | 201 Created | P0 |
| POS-014 | API GET list | Auth | GET /api/partner-tree | 200 with hierarchical list | P0 |
| POS-015 | API GET by ID | Auth | GET /api/partner-tree/5 | 200 with tree | P0 |
| POS-016 | API PUT update | Auth | PUT /api/partner-tree with valid body | 200 with updated | P0 |
| POS-017 | API DELETE | Auth | DELETE /api/partner-tree/5 | 204 No Content | P0 |
| POS-018 | API GET permissions | Auth | GET /api/partner-tree/5/permissions | 200 with canRead, canUpdate, canDelete | P0 |
| POS-019 | API GET structure | Auth | GET /api/partner-tree-structure | 200 with category/group | P0 |
| POS-020 | API GET partners by group ID | Auth | GET /api/partner-tree/by-partner-group-id/5 | 200 with paginated partners | P1 |
| POS-021 | API GET partners by category code | Auth | GET /api/partner-tree/by-partner-category-code/GOV | 200 with paginated partners | P1 |
| POS-022 | API GET categories-summary | Auth | GET /api/partner-tree/categories-summary | 200 with totalCategories, categories | P1 |
| POS-023 | API GET groups-summary | Auth | GET /api/partner-tree/groups-summary | 200 with totalGroups, groups | P1 |
| POS-024 | API GET categorization-overview | Auth | GET /api/partner-tree/categorization-overview | 200 with summary, categories, groups | P1 |
| POS-025 | API GET describe | Auth | GET /api/partner-tree/describe | 200 with entity config | P1 |
| POS-026 | PartnerTreeService GetPartnerTreeByIdAsync | Tree exists | GetPartnerTreeByIdAsync(5) | UNOPSPartnerTree returned | P1 |
| POS-027 | PartnerTreeService GetPartnerTreeByCodeAsync | Code "GOV" exists | GetPartnerTreeByCodeAsync("GOV") | Tree returned | P1 |
| POS-028 | PartnerTreeService GetPartnerCategoryByPartnerGroupCodeAsync | Group under category | GetPartnerCategoryByPartnerGroupCodeAsync("GOV-001") | Parent category | P1 |
| POS-029 | PartnerTreeService GetAllDescendantsAsync | Category with 3 groups | GetAllDescendantsAsync("GOV") | List of 3 group IDs | P1 |
| POS-030 | BuildHierarchy with null parent | Trees with Parent="" | BuildHierarchy(lookup, "") | Top-level items | P0 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | Create with duplicate code | Code already exists | BusinessException | P0 |
| NEG-002 | Create with null model | model=null | ArgumentNullException | P0 |
| NEG-003 | Create with empty Code | Code="" | Validation or error | P0 |
| NEG-004 | Create with null Name | Name=null | Validation error | P0 |
| NEG-005 | Get by non-existent ID | GetPartnerTreeAsync(user, 99999) | null | P0 |
| NEG-006 | Get by ID zero | GetPartnerTreeAsync(user, 0) | null or error | P1 |
| NEG-007 | Get by ID negative | GetPartnerTreeAsync(user, -1) | null or error | P1 |
| NEG-008 | Update non-existent | UpdatePartnerTreeAsync(model.Id=99999) | BusinessException | P0 |
| NEG-009 | Update with null model | model=null | ArgumentNullException | P0 |
| NEG-010 | Delete non-existent | DeletePartnerTreeAsync(user, 99999) | Graceful | P1 |
| NEG-011 | Delete already deleted | Tree IsDeleted=true | Graceful | P1 |
| NEG-012 | GetPostedPartnerTree non-existent | GetPostedPartnerTree(99999) | BusinessException | P0 |
| NEG-013 | Invalid sortBy | GetPartnerTreesAsync(user, "InvalidField", true) | Error or fallback | P1 |
| NEG-014 | Parent references non-existent | Create with Parent="NONEXISTENT" | Error or orphan | P1 |
| NEG-015 | Circular parent reference | Parent=A, A.Parent=B, B.Parent=A | Infinite loop prevented | P0 |
| NEG-016 | API POST without auth | No token | 401 | P0 |
| NEG-017 | API POST without create permission | User lacks create | 403 | P0 |
| NEG-018 | API GET without auth | No token | 401 | P0 |
| NEG-019 | API GET without read permission | User lacks read | 403 | P0 |
| NEG-020 | API PUT without update permission | User lacks update | 403 | P0 |
| NEG-021 | API DELETE without delete permission | User lacks delete | 403 | P0 |
| NEG-022 | API GET by invalid ID | GET /api/partner-tree/abc | 400 | P0 |
| NEG-023 | API GET by ID not found | GET /api/partner-tree/99999 | 404 | P0 |
| NEG-024 | API POST malformed JSON | Invalid JSON body | 400 | P0 |
| NEG-025 | API POST missing required fields | Body without Code | 400 | P0 |
| NEG-026 | GetPartnerTreeByCodeAsync non-existent | GetPartnerTreeByCodeAsync("NONEXISTENT") | null | P1 |
| NEG-027 | GetPartnerCategoryByPartnerGroupCodeAsync invalid | Code="NONEXISTENT" | null | P1 |
| NEG-028 | GetAllDescendantsAsync non-existent code | GetAllDescendantsAsync("NONEXISTENT") | Empty list | P1 |
| NEG-029 | GetBasicPartnerCategoryDetailsAsync non-existent | entityId=99999 | Error object | P1 |
| NEG-030 | GetBasicPartnerGroupDetailsAsync non-existent | entityId=99999 | Error object | P1 |
| NEG-031 | GetPartnerCategoryNewsDetailsAsync non-existent | entityId=99999 | Error object | P1 |
| NEG-032 | GetPartnerGroupNewsDetailsAsync non-existent | entityId=99999 | Error object | P1 |
| NEG-033 | Create Level_1 with special code | Code="MULTILATERAL" | PartnerCategoryCode handled per rule | P1 |
| NEG-034 | Create Level_1 with special code | Code="GOVERNMENT" | PartnerCategoryCode handled per rule | P1 |
| NEG-035 | Update without CanModifyPartnerCategoryCode | Non-editable category | PartnerCategoryCode not updated | P1 |
| NEG-036 | Update without CanModifyPartnerGroupCode | Non-editable group | PartnerGroupCode not updated | P1 |
| NEG-037 | API by-partner-group-id invalid | id=99999 | 400 or empty | P1 |
| NEG-038 | API by-partner-category-code empty | code="" | 400 or empty | P1 |
| NEG-039 | API by-partner-category-code non-existent | code="NONEXISTENT" | Empty | P1 |
| NEG-040 | Expired JWT | Expired token | 401 | P0 |
| NEG-041 | Tampered JWT | Modified token | 401 | P0 |
| NEG-042 | SQL injection in sortBy | sortBy="Name; DROP TABLE" | Sanitized | P0 |
| NEG-043 | SQL injection in Code | Code="'; DROP TABLE--" | Error | P0 |
| NEG-044 | XSS in Name | Name="<script>alert(1)</script>" | Sanitized | P0 |
| NEG-045 | XSS in Description | Description with script | Sanitized | P0 |
| NEG-046 | IDOR GetPartnerTree | User A requests User B's tree | 403 or filtered | P0 |
| NEG-047 | IDOR Update | User A updates User B's tree | 403 | P0 |
| NEG-048 | IDOR Delete | User A deletes User B's tree | 403 | P0 |
| NEG-049 | Mass assignment Id on create | Include Id in POST | Ignored | P0 |
| NEG-050 | Mass assignment CreatedBy | Include in request | Ignored | P0 |
| NEG-051 | Null ClaimsPrincipal | CreatePartnerTreeAsync(null, model) | Error | P1 |
| NEG-052 | Empty parent | Parent="" | Normalized to empty string | P1 |
| NEG-053 | Whitespace parent | Parent="   " | Normalized to empty | P1 |
| NEG-054 | Type mismatch | Type="Invalid" | Error or fallback | P1 |
| NEG-055 | API PUT empty array | PUT [] | Empty result | P1 |
| NEG-056 | API PUT partial failure | One invalid in array | Per design | P1 |
| NEG-057 | GetPartnerCategoryByPartnerGroupCodeAsync top-level | Code of category | null | P1 |
| NEG-058 | GetAllDescendantsAsync leaf node | Code of leaf | Empty list | P1 |
| NEG-059 | Cache invalidation | PartnerTreeService cache | After update/delete | P1 |
| NEG-060 | BuildHierarchy visitedCodes | Circular reference | Prevent infinite loop | P0 |
| NEG-061 | ProcessAllLevelsForCategories null nodes | nodes=null | No exception | P1 |
| NEG-062 | CollectAllEditableGroups null | nodes=null | Handled | P1 |
| NEG-063 | MapEntityToModel null entity | entity=null | Error | P1 |
| NEG-064 | API pagination invalid pageIndex | pageIndex=-1 | 400 | P1 |
| NEG-065 | API pagination invalid pageSize | pageSize=0 | 400 | P1 |
| NEG-066 | GetPartnersByPartnerGroupAsync no permission | User lacks read | 403 | P0 |
| NEG-067 | GetPartnersByCategoryAsync no permission | User lacks read | 403 | P0 |
| NEG-068 | Describe entity config error | Entity config fails | 500 | P1 |
| NEG-069 | Create with Name empty | Name="" | Per ModifiableDeletableEntity | P1 |
| NEG-070 | Update with invalid Id | model.Id=0 | Error | P1 |
| NEG-071 | Delete with partners assigned | Tree has Partners | Per cascade rule | P0 |
| NEG-072 | GetPartnerTreeByCodeIncludingDeleted | Code of deleted | Returns entity | P1 |
| NEG-073 | CanModifyPartnerCategoryCode Level_2 non-special | Type=Level_2, Parent not special | false | P1 |
| NEG-074 | CanModifyPartnerGroupCode no parent | Parent=null | false | P1 |
| NEG-075 | CanModifyPartnerGroupCode parent not found | Parent code not in list | false | P1 |
| NEG-076 | API permissions for non-existent | GET /permissions/99999 | 404 | P0 |
| NEG-077 | GetPartnerTreeAsync with deleted | Entity IsDeleted | null | P0 |
| NEG-078 | LoadPartnerTreesAsync cache miss | Cache empty | Load from DB | P1 |
| NEG-079 | CreatePartnerTreeAsync cache invalidation | After create | Cache cleared | P1 |
| NEG-080 | UpdatePartnerTreeAsync entity null | entity=null | ArgumentNullException | P0 |
| NEG-081 | UpdatePartnerTreeAsync entity deleted | existing.IsDeleted | false | P1 |
| NEG-082 | DeletePartnerTreeAsync code not found | Code not in DB | false | P1 |
| NEG-083 | GetParentCategory null parent | Parent not found | null | P1 |
| NEG-084 | GetPartnerCategoryByPartnerGroupCodeAsync null tree | GetPartnerTreeByCode returns null | null | P1 |
| NEG-085 | GetDescendantsRecursive empty children | No children | Empty descendants | P1 |
| NEG-086 | API categories-summary exception | PartnerManager error | 400 | P1 |
| NEG-087 | API groups-summary exception | PartnerManager error | 400 | P1 |
| NEG-088 | API categorization-overview exception | PartnerManager error | 400 | P1 |
| NEG-089 | GetBasicEntityAsync null user | user=null | Fallback to entity | P1 |
| NEG-090 | GetBasicEntityAsync null entity | entityId not found | null | P1 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max | Priority |
|----|----------------|-----|-----|--------|--------|----------|----------|
| BND-001 | Code length | 1 | 255 | "A" | 255 chars | 256 chars | P1 |
| BND-002 | Name length | 1 | 255 | "A" | 255 chars | 256 chars | P1 |
| BND-003 | Description length | 0 | 4000 | "" | 4000 chars | 4001 chars | P1 |
| BND-004 | Parent length | 0 | 255 | "" | 255 chars | 256 chars | P1 |
| BND-005 | Type | — | — | "Level_1" | "Level_2" | "Level_3" | P1 |
| BND-006 | Id | 1 | 2147483647 | 1 | Max int | Overflow | P1 |
| BND-007 | Parent null vs empty | — | — | null | "" | — | P1 |
| BND-008 | Code empty string | — | — | Code="" | — | — | P1 |
| BND-009 | Code single char | — | — | "X" | — | — | P1 |
| BND-010 | Code with special chars | — | — | "GOV-001" | — | — | P1 |
| BND-011 | Code Unicode | — | — | "政府" | — | — | P1 |
| BND-012 | Parent empty | — | — | Parent="" | — | — | P1 |
| BND-013 | Parent null | — | — | Parent=null | — | — | P1 |
| BND-014 | Hierarchy depth | 1 | 10 | 1 level | 10 levels | 11 levels | P1 |
| BND-015 | Children count | 0 | 1000 | 0 | 1000 | 1001 | P1 |
| BND-016 | sortBy | — | — | "Name" | "Code" | — | P1 |
| BND-017 | ascending true | — | — | true | — | — | P1 |
| BND-018 | ascending false | — | — | false | — | — | P1 |
| BND-019 | PartnerCategoryCode null | — | — | null | — | — | P1 |
| BND-020 | PartnerGroupCode null | — | — | null | — | — | P1 |
| BND-021 | PartnerCategoryEditable | — | — | true | false | — | P1 |
| BND-022 | PartnerGroupEditable | — | — | true | false | — | P1 |
| BND-023 | specialCategoryCodes | — | — | "MULTILATERAL" | "GOVERNMENT" | — | P1 |
| BND-024 | Level_1 not special | — | — | Type=Level_1, Code not special | — | — | P1 |
| BND-025 | Level_2 child of special | — | — | Parent="MULTILATERAL" | — | — | P1 |
| BND-026 | GetAllDescendantsAsync empty | — | — | No children | — | — | P1 |
| BND-027 | GetAllDescendantsAsync single | — | — | 1 child | — | — | P1 |
| BND-028 | GetAllDescendantsAsync many | — | — | 50 descendants | — | — | P1 |
| BND-029 | BuildHierarchy empty lookup | — | — | lookup.Empty | — | — | P1 |
| BND-030 | BuildHierarchy single root | — | — | 1 top-level | — | — | P1 |
| BND-031 | visitedCodes | — | — | Empty | — | — | P1 |
| BND-032 | Lookup key empty | — | — | "" | — | — | P1 |
| BND-033 | Lookup key null | — | — | null→"" | — | — | P1 |
| BND-034 | ProcessAllLevelsForCategories empty | — | — | nodes=[] | — | — | P1 |
| BND-035 | CollectAllEditableGroups empty | — | — | nodes=[] | — | — | P1 |
| BND-036 | tree.Data null | — | — | tree.Data=null | — | — | P1 |
| BND-037 | tree.Children null | — | — | tree.Children=null | — | — | P1 |
| BND-038 | tree.Children empty | — | — | tree.Children=[] | — | — | P1 |
| BND-039 | Pagination pageIndex | 0 | Max | 0 | Valid | -1 | P1 |
| BND-040 | Pagination pageSize | 1 | 1000 | 1 | 1000 | 1001 | P1 |
| BND-041 | Partner count per group | 0 | 10000 | 0 | 10000 | — | P1 |
| BND-042 | Category count | 0 | 100 | 0 | 100 | — | P1 |
| BND-043 | Group count | 0 | 500 | 0 | 500 | — | P1 |
| BND-044 | API id path param | 1 | 2147483647 | 1 | Max | — | P1 |
| BND-045 | API code path param | 1 | 255 | "A" | 255 chars | — | P1 |
| BND-046 | Cache expiry | — | — | 1 hour sliding | 2 hour absolute | — | P1 |
| BND-047 | Cache key | — | — | CACHE_KEY | — | — | P1 |
| BND-048 | GetDescendantsRecursive deleted | — | — | Child IsDeleted | — | — | P1 |
| BND-049 | GetDescendantsRecursive not deleted | — | — | Child !IsDeleted | — | — | P1 |
| BND-050 | existingCategory | — | — | partnerCategoryId already in list | — | — | P1 |
| BND-051 | existingGroup | — | — | partnerGroupId already in groupList | — | — | P1 |
| BND-052 | PartnerGroupCode fallback | — | — | node.Data.PartnerGroupCode null | Use Code | — | P1 |
| BND-053 | PartnerGroupName fallback | — | — | node.Data.PartnerGroupName null | Use Name | — | P1 |
| BND-054 | PartnerCategoryCode null on create | — | — | CanModifyPartnerCategoryCode false | — | — | P1 |
| BND-055 | PartnerGroupCode null on create | — | — | CanModifyPartnerGroupCode false | — | — | P1 |
| BND-056 | UpdatePartnerTreeAsync Parent whitespace | — | — | Parent="   " | Normalized to "" | — | P1 |
| BND-057 | CreatePartnerTreeAsync Parent whitespace | — | — | Parent="   " | Normalized to "" | — | P1 |
| BND-058 | GetDescendantsRecursive parentCode | — | — | parentCode="" | — | — | P1 |
| BND-059 | GetDescendantsRecursive parentCode non-existent | — | — | No children | Empty | — | P1 |
| BND-060 | MapEntityToModel PartnerGroupCode | — | — | PartnerGroupCode set | PartnerGroupName resolved | — | P1 |
| BND-061 | MapEntityToModel PartnerCategoryCode | — | — | PartnerCategoryCode set | PartnerCategoryName resolved | — | P1 |
| BND-062 | MapEntityToModel both null | — | — | Both null | — | — | P1 |
| BND-063 | GetBasicPartnerCategoryDetailsAsync 30 days | — | — | recentInteractions | Date >= thirtyDaysAgo | — | P1 |
| BND-064 | GetBasicPartnerGroupDetailsAsync partners | — | — | PartnerGroupId match | — | — | P1 |
| BND-065 | GetPartnerCategoryNewsDetailsAsync partnerGroupIds | — | — | GetAllDescendantsAsync | — | — | P1 |
| BND-066 | GetPartnerGroupNewsDetailsAsync direct | — | — | PartnerGroupId == entityId | — | — | P1 |
| BND-067 | EligibleEntities | — | — | GetPostedPartnerTree include | — | — | P1 |
| BND-068 | Update array single | — | — | PUT [one item] | — | — | P1 |
| BND-069 | Update array multiple | — | — | PUT [5 items] | — | — | P1 |
| BND-070 | Update array max | — | — | PUT [100 items] | — | — | P1 |
| BND-071 | Id zero | — | — | Id=0 | — | — | P1 |
| BND-072 | Status | — | — | Active | Inactive | — | P1 |
| BND-073 | IsDeleted | — | — | false | true | — | P1 |
| BND-074 | CreatedDate | — | — | Min/Max DateTime | — | — | P2 |
| BND-075 | LastModifiedDate | — | — | null | Set | — | P1 |
| BND-076 | PartnerCategoryId | — | — | null | int | — | P1 |
| BND-077 | PartnerGroupId | — | — | null | int | — | P1 |
| BND-078 | PartnerGroupId.Value | — | — | partnerGroupIds.Contains | — | — | P1 |
| BND-079 | orgUnitCodes | — | — | Empty | Multiple | — | P1 |
| BND-080 | orgUnitLookup | — | — | Empty | Populated | — | P1 |
| BND-081 | recentInteractions | — | — | 0 | Many | — | P1 |
| BND-082 | allInteractions | — | — | 0 | Many | — | P1 |
| BND-083 | interactionPartners | — | — | Empty | Populated | — | P1 |
| BND-084 | interactionContacts | — | — | Empty | Populated | — | P1 |
| BND-085 | interactionUsers | — | — | Empty | Populated | — | P1 |
| BND-086 | searchContext focusAreas | — | — | Array | — | — | P1 |
| BND-087 | searchContext newsSources | — | — | Array | — | — | P1 |
| BND-088 | summary totalPartners | — | — | 0 | N | — | P1 |
| BND-089 | metadata generatedAt | — | — | DateTime.UtcNow | — | — | P1 |
| BND-090 | GetBasicEntityAsync entityId | — | — | 0 | Valid | — | P1 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | BuildHierarchy uses Code/Parent | Hierarchy not FK | BuildHierarchy with lookup | Parent matches children | P0 |
| FUN-002 | Null/empty Parent normalized | Parent null or "" | Lookup key | Both map to "" | P0 |
| FUN-003 | visitedCodes prevents cycles | Circular reference | BuildHierarchy | No infinite loop | P0 |
| FUN-004 | PartnerCategoryEditable Level_1 | Type=Level_1, Code not special | CanModifyPartnerCategoryCode | true | P0 |
| FUN-005 | PartnerCategoryEditable Level_2 special child | Type=Level_2, Parent=GOVERNMENT | CanModifyPartnerCategoryCode | true | P0 |
| FUN-006 | PartnerCategoryEditable false for MULTILATERAL | Code=MULTILATERAL | CanModifyPartnerCategoryCode | false | P0 |
| FUN-007 | PartnerCategoryEditable false for GOVERNMENT | Code=GOVERNMENT | CanModifyPartnerCategoryCode | false | P0 |
| FUN-008 | PartnerGroupEditable requires parent | Parent=null | CanModifyPartnerGroupCode | false | P0 |
| FUN-009 | PartnerGroupEditable child of category | Parent is category | CanModifyPartnerGroupCode | true | P0 |
| FUN-010 | PartnerGroupEditable recursive | Parent is group | CanModifyPartnerGroupCode | Recursive check | P0 |
| FUN-011 | Code uniqueness | Create | CreatePartnerTreeAsync | Duplicate code rejected | P0 |
| FUN-012 | Soft delete | Delete | DeletePartnerTreeAsync | IsDeleted=true | P0 |
| FUN-013 | Deleted excluded from list | GetPartnerTrees | LoadPartnerTreesAsync | !IsDeleted filter | P0 |
| FUN-014 | Cache invalidation on create | Create | CreatePartnerTreeAsync | Cache removed | P0 |
| FUN-015 | Cache invalidation on update | Update | UpdatePartnerTreeAsync | Cache removed | P0 |
| FUN-016 | Cache invalidation on delete | Delete | DeletePartnerTreeAsync | Cache removed | P0 |
| FUN-017 | GetAllDescendantsAsync recursive | Category with nested groups | GetAllDescendantsAsync | All descendant IDs | P0 |
| FUN-018 | GetPartnerCategoryByPartnerGroupCodeAsync | Group under category | GetPartnerCategoryByPartnerGroupCodeAsync | Parent category | P0 |
| FUN-019 | GetParentCategory recursive | Multi-level | GetParentCategory | Traverse to category | P0 |
| FUN-020 | GetCategoryAndGroupStructureAsync | PartnerCategoryEditable | ProcessAllLevelsForCategories | Categories only | P0 |
| FUN-021 | CollectAllEditableGroups | PartnerGroupEditable | CollectAllEditableGroups | Groups only | P0 |
| FUN-022 | MapEntityToModel PartnerGroupCode | PartnerGroupCode set | MapEntityToModel | PartnerGroupName resolved | P0 |
| FUN-023 | MapEntityToModel PartnerCategoryCode | PartnerCategoryCode set | MapEntityToModel | PartnerCategoryName resolved | P0 |
| FUN-024 | MapEntityToModel PartnerCategoryEditable | Category | MapEntityToModel | PartnerCategoryEditable=true | P0 |
| FUN-025 | MapEntityToModel PartnerGroupEditable | Group | MapEntityToModel | PartnerGroupEditable=true | P0 |
| FUN-026 | Permission on create | Permission check | CreatePartnerTreeAsync | RBAC enforced | P0 |
| FUN-027 | Permission on update | Permission check | UpdatePartnerTreeAsync | RBAC enforced | P0 |
| FUN-028 | Permission on delete | Permission check | DeletePartnerTreeAsync | RBAC enforced | P0 |
| FUN-029 | Permission on read | Permission check | GetPartnerTreeAsync | RBAC enforced | P0 |
| FUN-030 | AccessControlled create | POST | Create | [AccessControlled] | P0 |
| FUN-031 | AccessControlled read | GET | GetAll | [AccessControlled] | P0 |
| FUN-032 | AccessControlled update | PUT | Update | [AccessControlled] | P0 |
| FUN-033 | AccessControlled delete | DELETE | Delete | [AccessControlled] | P0 |
| FUN-034 | GetEntityPermissionsAsync | Permissions endpoint | GetEntityPermissionsAsync | canRead, canUpdate, canDelete | P0 |
| FUN-035 | HandleOperationAsync | Controller | All endpoints | Consistent error handling | P0 |
| FUN-036 | Update array iteration | PUT array | Update | Each item updated | P0 |
| FUN-037 | Update null result | Update non-existent | UpdatePartnerTreeAsync | Null not added | P0 |
| FUN-038 | GetBasicPartnerCategoryDetailsAsync structure | Partners, interactions | GetBasicPartnerCategoryDetailsAsync | Correct JSON | P0 |
| FUN-039 | GetBasicPartnerGroupDetailsAsync structure | Partners, interactions | GetBasicPartnerGroupDetailsAsync | Correct JSON | P0 |
| FUN-040 | GetPartnerCategoryNewsDetailsAsync | News context | GetPartnerCategoryNewsDetailsAsync | searchContext | P0 |
| FUN-041 | GetPartnerGroupNewsDetailsAsync | News context | GetPartnerGroupNewsDetailsAsync | searchContext | P0 |
| FUN-042 | GetBasicPartnerCategoryDetailsAsync descendants | GetAllDescendantsAsync | GetBasicPartnerCategoryDetailsAsync | Correct partnerGroupIds | P0 |
| FUN-043 | GetBasicPartnerGroupDetailsAsync direct | PartnerGroupId match | GetBasicPartnerGroupDetailsAsync | Direct partners | P0 |
| FUN-044 | GetUserProfileForAIAsync | AI context | GetBasicPartnerCategoryDetailsAsync | userProfile included | P1 |
| FUN-045 | auditInfo | Audit | GetBasicPartnerCategoryDetailsAsync | createdDate, lastModifiedDate | P1 |
| FUN-046 | summary statistics | Stats | GetBasicPartnerCategoryDetailsAsync | totalInteractions, recentInteractions | P1 |
| FUN-047 | mostActivePartners | Top 3 | GetBasicPartnerCategoryDetailsAsync | Take(3) | P1 |
| FUN-048 | commonInteractionTypes | Top 3 | GetBasicPartnerCategoryDetailsAsync | Take(3) | P1 |
| FUN-049 | thirtyDaysAgo filter | Recent | recentInteractions | Date >= thirtyDaysAgo | P1 |
| FUN-050 | Split query optimization | Load | GetBasicPartnerCategoryDetailsAsync | Separate queries | P1 |
| FUN-051 | AsNoTracking | Read-only | All queries | AsNoTracking | P1 |
| FUN-052 | categories-summary | PartnerManager | GetPartnersAsync | GroupBy PartnerCategoryCode | P1 |
| FUN-053 | groups-summary | PartnerManager | GetPartnersAsync | GroupBy PartnerGroupId | P1 |
| FUN-054 | categorization-overview | PartnerManager | GetPartnersAsync | Both categories and groups | P1 |
| FUN-055 | GetPartnersByPartnerGroupAsync | PartnerManager | GetPartnersByPartnerGroupAsync | Paginated | P1 |
| FUN-056 | GetPartnersByCategoryAsync | PartnerManager | GetPartnersByCategoryAsync | Paginated | P1 |
| FUN-057 | Describe entity config | EntityConfigurationManager | GetEntityConfigurationDetailsAsync | PartnerTree config | P1 |
| FUN-058 | CreatePartnerTreeAsync Name | ModifiableDeletableEntity | Create | Name required | P1 |
| FUN-059 | UpdatePartnerTreeAsync Description | Update | UpdatePartnerTreeAsync | Description updated | P1 |
| FUN-060 | UpdatePartnerTreeAsync Type | Update | UpdatePartnerTreeAsync | Type updated | P1 |
| FUN-061 | UpdatePartnerTreeAsync Parent | Update | UpdatePartnerTreeAsync | Parent normalized | P1 |
| FUN-062 | CreatePartnerTreeAsync second pass | PartnerGroupCode | Create | After AddAsync, UpdateAsync | P1 |
| FUN-063 | LoadPartnerTreesAsync cache | Cache hit | LoadPartnerTreesAsync | No DB call | P1 |
| FUN-064 | LoadPartnerTreesAsync cache miss | Cache empty | LoadPartnerTreesAsync | DB call | P1 |
| FUN-065 | CanModifyPartnerCategoryCode Level_2 non-special parent | Parent not special | CanModifyPartnerCategoryCode | false | P1 |
| FUN-066 | CanModifyPartnerGroupCode parent not category | Parent is group | CanModifyPartnerGroupCode | Recursive | P1 |
| FUN-067 | GetDescendantsRecursive | Children | GetDescendantsRecursive | Children added | P1 |
| FUN-068 | GetDescendantsRecursive nested | Grandchildren | GetDescendantsRecursive | All levels | P1 |
| FUN-069 | ProcessAllLevelsForCategories nested | Children have categories | ProcessAllLevelsForCategories | Categories collected | P1 |
| FUN-070 | CollectAllEditableGroups nested | Group has children | CollectAllEditableGroups | All groups | P1 |
| FUN-071 | existingCategory check | partnerCategoryId | ProcessAllLevelsForCategories | Reuse children list | P1 |
| FUN-072 | existingGroup check | partnerGroupId | CollectAllEditableGroups | Skip duplicate | P1 |
| FUN-073 | GetPostedPartnerTree EligibleEntities | Include | GetPostedPartnerTree | EligibleEntities loaded | P1 |
| FUN-074 | GetBasicEntityAsync with user | user | GetBasicEntityAsync | GetPartnerTreeAsync | P1 |
| FUN-075 | GetBasicEntityAsync without user | user=null | GetBasicEntityAsync | MapEntityToModel | P1 |
| FUN-076 | GetPartnerTreeByCode legacy | GetPartnerTreeByCode | Legacy method | Returns model | P1 |
| FUN-077 | Base controller HandleOperationAsync | 201 | Create | 201 on success | P0 |
| FUN-078 | Base controller HandleOperationAsync | 200 | Get | 200 on success | P0 |
| FUN-079 | GetCategoryAndGroupStructureAsync structure | ProcessAllLevelsForCategories | GetCategoryAndGroupStructureAsync | Categories with children | P0 |
| FUN-080 | Partner.PartnerGroupId | FK | Partner | PartnerGroupId | P1 |
| FUN-081 | Partner inverse | Partners | PartnerTree | InverseProperty | P1 |
| FUN-082 | DeletePartnerTreeAsync code | Delete by code | DeletePartnerTreeAsync | GetPartnerTreeByCodeIncludingDeleted | P1 |
| FUN-083 | UpdatePartnerTreeAsync existing | GetByIdAsync | UpdatePartnerTreeAsync | Existing entity | P1 |
| FUN-084 | CreatePartnerTreeAsync Status | EntityStatus | Create | Status=Active | P1 |
| FUN-085 | CreatePartnerTreeAsync Parent | Parent | Create | Normalized | P1 |
| FUN-086 | MapEntityToModelWithPermissionsAsync | Permissions | MapEntityToModelWithPermissionsAsync | Permissions set | P1 |
| FUN-087 | GetEntityPermissionsAsync PartnerTree | Entity type | GetEntityPermissionsAsync | PartnerTree permissions | P1 |
| FUN-088 | BusinessException on create fail | Create returns null | Create | BusinessException | P0 |
| FUN-089 | BusinessException on not found | Get returns null | Get | BusinessException | P0 |
| FUN-090 | GetPartnerTreeAsync null | Entity not found | GetPartnerTreeAsync | null | P0 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Full CRUD flow | Create→Get→Update→Delete | PartnerTree | All succeed | P0 |
| INT-002 | Create then GetPartnerTrees | Create | PartnerTree | New tree in hierarchy | P0 |
| INT-003 | Create category then group | Create category, create group | PartnerTree | Group under category | P0 |
| INT-004 | GetCategoryAndGroupStructure | After create | PartnerTree | New category/group in structure | P0 |
| INT-005 | Update then Get | Update | PartnerTree | Updated data returned | P0 |
| INT-006 | Delete then Get | Delete | PartnerTree | null or 404 | P0 |
| INT-007 | API POST→GET | POST then GET | PartnerTree | Created tree returned | P0 |
| INT-008 | API GET→PUT→GET | Get, update, get | PartnerTree | Updated data | P0 |
| INT-009 | API GET→DELETE→GET | Get, delete, get | PartnerTree | 404 | P0 |
| INT-010 | Permissions after create | Create then permissions | PartnerTree | Permissions returned | P0 |
| INT-011 | PartnerTreeManager→PartnerTreeService | Create | PartnerTree, PartnerTreeService | Service.CreatePartnerTreeAsync | P0 |
| INT-012 | PartnerTreeService→Repository | Create | PartnerTreeService, DataRepository | AddAsync | P0 |
| INT-013 | PartnerTreeManager→PartnerManager | by-partner-group-id | PartnerTreeController, PartnerManager | GetPartnersByPartnerGroupAsync | P0 |
| INT-014 | PartnerTreeManager→PartnerManager | by-partner-category-code | PartnerTreeController, PartnerManager | GetPartnersByCategoryAsync | P0 |
| INT-015 | PartnerTreeManager→PartnerManager | categories-summary | PartnerTreeController, PartnerManager | GetPartnersAsync | P0 |
| INT-016 | PartnerTreeManager→PartnerManager | groups-summary | PartnerTreeController, PartnerManager | GetPartnersAsync | P0 |
| INT-017 | PartnerTreeManager→PartnerManager | categorization-overview | PartnerTreeController, PartnerManager | GetPartnersAsync | P0 |
| INT-018 | PartnerTreeManager→EntityConfigurationManager | describe | PartnerTreeController | GetEntityConfigurationDetailsAsync | P0 |
| INT-019 | PartnerTree→Partner | FK | PartnerTree, Partner | Partners collection | P0 |
| INT-020 | GetPartnersByPartnerGroup | Partner.PartnerGroupId | Partner, PartnerTree | Partners filtered | P0 |
| INT-021 | GetPartnersByCategory | Partner.PartnerCategoryCode | Partner, PartnerTree | Partners filtered | P0 |
| INT-022 | GetBasicPartnerCategoryDetailsAsync→Partners | Category | PartnerTree, Partner | Partners in category | P0 |
| INT-023 | GetBasicPartnerCategoryDetailsAsync→Interactions | Category | PartnerTree, Partner, Interaction | recentInteractions | P0 |
| INT-024 | GetBasicPartnerGroupDetailsAsync→Partners | Group | PartnerTree, Partner | Partners in group | P0 |
| INT-025 | GetBasicPartnerGroupDetailsAsync→Interactions | Group | PartnerTree, Partner, Interaction | recentInteractions | P0 |
| INT-026 | GetPartnerCategoryNewsDetailsAsync→Partners | Category | PartnerTree, Partner | Partners for news | P0 |
| INT-027 | GetPartnerGroupNewsDetailsAsync→Partners | Group | PartnerTree, Partner | Partners for news | P0 |
| INT-028 | GetAllDescendantsAsync→Partner | PartnerGroupId | PartnerTreeService, Partner | PartnerGroupId in list | P0 |
| INT-029 | GetPartnerCategoryByPartnerGroupCodeAsync | Group→Category | PartnerTreeService | Parent category | P0 |
| INT-030 | MapEntityToModel→PartnerTreeService | PartnerGroupCode | UNOPSPartnerTreeManager, PartnerTreeService | GetPartnerTreeByCodeAsync | P0 |
| INT-031 | MapEntityToModel→PartnerTreeService | PartnerCategoryCode | UNOPSPartnerTreeManager, PartnerTreeService | GetPartnerTreeByCodeAsync | P0 |
| INT-032 | MapEntityToModel→PartnerTreeService | GetPartnerCategoryByPartnerGroupCodeAsync | UNOPSPartnerTreeManager, PartnerTreeService | Category resolved | P0 |
| INT-033 | Cache→PartnerTreeService | LoadPartnerTreesAsync | PartnerTreeService, MemoryCache | Cache | P0 |
| INT-034 | Cache invalidation→Create | Create | PartnerTreeService | Cache removed | P0 |
| INT-035 | Cache invalidation→Update | Update | PartnerTreeService | Cache removed | P0 |
| INT-036 | Cache invalidation→Delete | Delete | PartnerTreeService | Cache removed | P0 |
| INT-037 | Controller→ManagerWrapper | Manager | PartnerTreeController | _managerWrapper.PartnerTreeManager | P0 |
| INT-038 | Controller→User | User | PartnerTreeController | UserResolverService | P0 |
| INT-039 | Controller→AuthorizationService | Authorization | PartnerTreeController | GetEntityPermissionsAsync | P0 |
| INT-040 | BaseController HandleOperationAsync | All endpoints | PartnerTreeController | Error handling | P0 |
| INT-041 | AccessControlled→EntityTypes | EntityTypes.PartnerTree | PartnerTreeController | create, read, update, delete | P0 |
| INT-042 | PartnerTreeAuthorizationHandler | Authorization | PartnerTreeAuthorizationHandler | PartnerTreeModel | P0 |
| INT-043 | UNOPSPartnerTreeManager→BaseUNOPSManager | Base | UNOPSPartnerTreeManager | BaseUNOPSManager | P0 |
| INT-044 | UNOPSPartnerTreeManager→PermissionService | Permission | UNOPSPartnerTreeManager | IPermissionService | P0 |
| INT-045 | GetBasicPartnerCategoryDetailsAsync→OrganizationHierarchy | orgUnitLookup | UNOPSPartnerTreeManager | OrganizationHierarchies | P0 |
| INT-046 | GetBasicPartnerGroupDetailsAsync→OrganizationHierarchy | orgUnitLookup | UNOPSPartnerTreeManager | OrganizationHierarchies | P0 |
| INT-047 | InteractionPartner→Partner | Include | GetBasicPartnerCategoryDetailsAsync | InteractionPartner.Partner | P0 |
| INT-048 | InteractionContact→Contact | Include | GetBasicPartnerCategoryDetailsAsync | InteractionContact.Contact | P0 |
| INT-049 | InteractionUser→User | Include | GetBasicPartnerCategoryDetailsAsync | InteractionUser.User | P0 |
| INT-050 | User→UserProfile | ThenInclude | GetBasicPartnerCategoryDetailsAsync | User.UserProfile | P0 |
| INT-051 | Partner→PartnerGroup | Include | GetBasicPartnerCategoryDetailsAsync | Partner.PartnerGroup | P0 |
| INT-052 | Partner→LiaisonOffice | Include | GetBasicPartnerCategoryDetailsAsync | Partner.LiaisonOffice | P0 |
| INT-053 | PaginationRequest | by-partner-group-id | PartnerTreeController | PaginationRequest | P0 |
| INT-054 | PaginationRequest | by-partner-category-code | PartnerTreeController | PaginationRequest | P0 |
| INT-055 | PaginationResponse | Pagination | PartnerManager | PaginationResponse | P0 |
| INT-056 | AutoMapper PartnerTree→PartnerTreeDataModel | Map | UNOPSPartnerTreeManager | mapper.Map | P0 |
| INT-057 | AutoMapper PartnerTreeDataModel→UNOPSPartnerTree | Map | UNOPSPartnerTreeManager | mapper.Map | P0 |
| INT-058 | AutoMapper UNOPSPartnerTree→ExternalPartnerTreeModel | Map | UNOPSPartnerTreeManager | mapper.Map | P0 |
| INT-059 | UNOPSPartnerTree→PartnerTree | Inherit | UNOPSPartnerTree | PartnerTree | P0 |
| INT-060 | ModifiableDeletableEntity | Base | PartnerTree | ModifiableDeletableEntity | P0 |
| INT-061 | PartnerTree.Name | Required | ModifiableDeletableEntity | Name required | P0 |
| INT-062 | PartnerTree.Partners | InverseProperty | Partner | PartnerGroup | P0 |
| INT-063 | DataRepository GetAllSortedAsync | Sort | PartnerTreeService | GetAllSortedAsync("Type") | P0 |
| INT-064 | DataRepository GetByIdAsync | Get | PartnerTreeService | GetByIdAsync | P0 |
| INT-065 | DataRepository AddAsync | Create | PartnerTreeService | AddAsync | P0 |
| INT-066 | DataRepository UpdateAsync | Update | PartnerTreeService | UpdateAsync | P0 |
| INT-067 | DataRepository Delete | Delete | PartnerTreeService | Delete | P0 |
| INT-068 | GetDescendantsRecursive→GetDescendantsRecursive | Recursive | PartnerTreeService | Recursive call | P0 |
| INT-069 | GetParentCategory→GetPartnerTreeByCodeAsync | Recursive | PartnerTreeService | GetPartnerTreeByCodeAsync(Parent) | P0 |
| INT-070 | GetParentCategory→GetParentCategory | Recursive | PartnerTreeService | Recursive until category | P0 |
| INT-071 | BuildHierarchy→BuildHierarchy | Recursive | UNOPSPartnerTreeManager | Recursive | P0 |
| INT-072 | ProcessAllLevelsForCategories→ProcessAllLevelsForCategories | Recursive | UNOPSPartnerTreeManager | Recursive | P0 |
| INT-073 | CollectAllEditableGroups→CollectAllEditableGroups | Recursive | UNOPSPartnerTreeManager | Recursive | P0 |
| INT-074 | PartnerTreeDataModel | DTO | PartnerTreeController | Request/response | P0 |
| INT-075 | PartnerTreeModel | Model | PartnerTreeController | Response | P0 |
| INT-076 | ExternalPartnerTreeModel | Model | GetPostedPartnerTree | Response | P0 |
| INT-077 | APIDictionary.PartnerTree | Route | PartnerTreeController | /api/partner-tree | P0 |
| INT-078 | APIDictionary.PartnerTree + "-structure" | Route | PartnerTreeController | /api/partner-tree-structure | P0 |
| INT-079 | IAP authentication | Auth | PartnerTreeController | [Authorize(AuthenticationSchemes = "IAP")] | P0 |
| INT-080 | BusinessException handling | Create | PartnerTreeController | 400 | P0 |
| INT-081 | UnauthorizedAccessException | Get | PartnerTreeController | 403 | P0 |
| INT-082 | Exception handling | GetAll | PartnerTreeController | 500 | P0 |
| INT-083 | GetPartnersByPartnerGroupAsync exception | PartnerManager | PartnerTreeController | 400 | P0 |
| INT-084 | GetPartnersByCategoryAsync exception | PartnerManager | PartnerTreeController | 400 | P0 |
| INT-085 | Describe exception | EntityConfigurationManager | PartnerTreeController | 500 | P0 |
| INT-086 | GetBasicEntityAsync | AI | UNOPSPartnerTreeManager | GetPartnerTreeAsync or MapEntityToModel | P0 |
| INT-087 | GetBasicPartnerCategoryDetailsAsync | AI | UNOPSPartnerTreeManager | Full partner details | P0 |
| INT-088 | GetBasicPartnerGroupDetailsAsync | AI | UNOPSPartnerTreeManager | Full partner details | P0 |
| INT-089 | GetPartnerCategoryNewsDetailsAsync | AI | UNOPSPartnerTreeManager | News context | P0 |
| INT-090 | GetPartnerGroupNewsDetailsAsync | AI | UNOPSPartnerTreeManager | News context | P0 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | SQL injection in Code | Code="'; DROP TABLE--" | CreatePartnerTreeAsync | Error | P0 |
| SEC-002 | SQL injection in sortBy | sortBy="Name; DROP TABLE" | GetPartnerTreesAsync | Sanitized | P0 |
| SEC-003 | SQL injection in Name | Name with SQL | CreatePartnerTreeAsync | Sanitized | P0 |
| SEC-004 | XSS in Name | Name="<script>alert(1)</script>" | CreatePartnerTreeAsync | Sanitized | P0 |
| SEC-005 | XSS in Description | Description with script | CreatePartnerTreeAsync | Sanitized | P0 |
| SEC-006 | Unauthorized create | No create permission | POST /api/partner-tree | 403 | P0 |
| SEC-007 | Unauthorized read | No read permission | GET /api/partner-tree | 403 | P0 |
| SEC-008 | Unauthorized update | No update permission | PUT /api/partner-tree | 403 | P0 |
| SEC-009 | Unauthorized delete | No delete permission | DELETE /api/partner-tree/5 | 403 | P0 |
| SEC-010 | Unauthenticated | No token | All endpoints | 401 | P0 |
| SEC-011 | IDOR GetPartnerTree | User A gets User B's tree | GetPartnerTreeAsync | 403 or filtered | P0 |
| SEC-012 | IDOR Update | User A updates User B's tree | UpdatePartnerTreeAsync | 403 | P0 |
| SEC-013 | IDOR Delete | User A deletes User B's tree | DeletePartnerTreeAsync | 403 | P0 |
| SEC-014 | IDOR Permissions | User A gets User B's permissions | GET /permissions | 403 | P0 |
| SEC-015 | Mass assignment Id | Include Id in POST | Create | Ignored | P0 |
| SEC-016 | Mass assignment CreatedBy | Include in request | Create | Ignored | P0 |
| SEC-017 | Mass assignment CreatedDate | Include in request | Create | Ignored | P0 |
| SEC-018 | Path traversal in code | Code="../../../etc" | CreatePartnerTreeAsync | Error | P0 |
| SEC-019 | LDAP injection in Code | Code="*)(uid=*" | CreatePartnerTreeAsync | Sanitized | P0 |
| SEC-020 | Command injection | Code="; rm -rf" | CreatePartnerTreeAsync | Sanitized | P0 |
| SEC-021 | Expired JWT | Expired token | All endpoints | 401 | P0 |
| SEC-022 | Tampered JWT | Modified token | All endpoints | 401 | P0 |
| SEC-023 | Wrong JWT audience | Wrong audience | All endpoints | 401 | P0 |
| SEC-024 | CSRF | Cross-site request | POST | Token validation | P0 |
| SEC-025 | Broken access control structure | No read | GET /structure | 403 | P0 |
| SEC-026 | Broken access control by-group | No read | GET /by-partner-group-id | 403 | P0 |
| SEC-027 | Broken access control by-category | No read | GET /by-partner-category-code | 403 | P0 |
| SEC-028 | Broken access control categories-summary | No read | GET /categories-summary | 403 | P0 |
| SEC-029 | Broken access control groups-summary | No read | GET /groups-summary | 403 | P0 |
| SEC-030 | Broken access control categorization-overview | No read | GET /categorization-overview | 403 | P0 |
| SEC-031 | Broken access control describe | No read | GET /describe | 403 | P0 |
| SEC-032 | Data exposure | Sensitive data in response | PartnerTree | No secrets | P0 |
| SEC-033 | Information disclosure | Stack trace | Exception | No stack trace | P0 |
| SEC-034 | Rate limiting | Too many requests | All endpoints | 429 | P1 |
| SEC-035 | Input length DoS | Very long Code | CreatePartnerTreeAsync | Rejected | P0 |
| SEC-036 | Input length DoS | Very long Name | CreatePartnerTreeAsync | Rejected | P0 |
| SEC-037 | Null byte injection | Code="valid%00" | CreatePartnerTreeAsync | Rejected | P0 |
| SEC-038 | Unicode normalization | Code with homoglyphs | CreatePartnerTreeAsync | Normalized | P1 |
| SEC-039 | Authorization handler | PartnerTreeAuthorizationHandler | PartnerTreeModel | Handler invoked | P0 |
| SEC-040 | AccessControlled attribute | Attribute | All endpoints | Enforced | P0 |
| SEC-041 | Entity permission check | GetEntityPermissionsAsync | PartnerTree | Correct permissions | P0 |
| SEC-042 | Row-level security | Entity-level | GetPartnerTreeAsync | Filtered by user | P0 |
| SEC-043 | Partner data isolation | GetPartnersByPartnerGroup | PartnerManager | User-scoped | P0 |
| SEC-044 | Partner data isolation | GetPartnersByCategory | PartnerManager | User-scoped | P0 |
| SEC-045 | AI context security | GetBasicPartnerCategoryDetailsAsync | User | User-scoped | P0 |
| SEC-046 | AI context security | GetBasicPartnerGroupDetailsAsync | User | User-scoped | P0 |
| SEC-047 | Session fixation | Session | Auth | New session | P1 |
| SEC-048 | Privilege escalation | Role manipulation | Request | Rejected | P0 |
| SEC-049 | Horizontal privilege | Access other org | GetPartnerTree | 403 | P0 |
| SEC-050 | Vertical privilege | User accesses admin | Admin endpoint | 403 | P0 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Concurrent create same code | 2 users create Code="X" | One succeeds, one BusinessException | P0 |
| CON-002 | Concurrent update same tree | 2 users update same tree | Last-write-wins or conflict | P1 |
| CON-003 | Concurrent delete same tree | 2 users delete same tree | One succeeds, one graceful | P1 |
| CON-004 | Concurrent read during update | Read while update | Consistent read | P1 |
| CON-005 | Cache invalidation race | Update and read | Cache invalidated | P1 |
| CON-006 | LoadPartnerTreesAsync concurrent | 2 threads LoadPartnerTreesAsync | No corruption | P1 |
| CON-007 | BuildHierarchy concurrent | 2 threads BuildHierarchy | No shared state | P1 |
| CON-008 | GetAllDescendantsAsync concurrent | 2 threads same code | Correct results | P1 |
| CON-009 | CreatePartnerTreeAsync concurrent | 2 threads create different | Both succeed | P1 |
| CON-010 | UpdatePartnerTreeAsync concurrent | 2 threads update different | Both succeed | P1 |
| CON-011 | DeletePartnerTreeAsync concurrent | 2 threads delete different | Both succeed | P1 |
| CON-012 | GetPartnerTreesAsync concurrent | 10 threads | All return correct | P1 |
| CON-013 | GetCategoryAndGroupStructureAsync concurrent | 5 threads | All return correct | P1 |
| CON-014 | Cache read during write | Read while cache populate | No deadlock | P1 |
| CON-015 | MemoryCache concurrent | Multiple cache operations | Thread-safe | P1 |
| CON-016 | visitedCodes in BuildHierarchy | Concurrent BuildHierarchy | Separate HashSet | P1 |
| CON-017 | DbContext concurrent | Multiple EF operations | Per-request scope | P1 |
| CON-018 | API concurrent POST | 5 concurrent POST | All succeed or conflict | P1 |
| CON-019 | API concurrent GET | 20 concurrent GET | All return 200 | P1 |
| CON-020 | API concurrent PUT | 3 concurrent PUT same | Last-write-wins | P1 |
| CON-021 | API concurrent DELETE | 2 concurrent DELETE same | One 204, one 404 | P1 |
| CON-022 | GetBasicPartnerCategoryDetailsAsync concurrent | 5 threads | All return correct | P1 |
| CON-023 | GetBasicPartnerGroupDetailsAsync concurrent | 5 threads | All return correct | P1 |
| CON-024 | MapEntityToModel concurrent | Multiple threads | No shared state | P1 |
| CON-025 | GetDescendantsRecursive concurrent | 2 threads | Independent execution | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | CanModifyPartnerCategoryCode Level_1 | Validation | Type=Level_1, Code not special | true | P0 |
| UNT-002 | CanModifyPartnerCategoryCode Level_2 special | Validation | Type=Level_2, Parent=GOVERNMENT | true | P0 |
| UNT-003 | CanModifyPartnerCategoryCode MULTILATERAL | Validation | Code=MULTILATERAL | false | P0 |
| UNT-004 | CanModifyPartnerCategoryCode GOVERNMENT | Validation | Code=GOVERNMENT | false | P0 |
| UNT-005 | CanModifyPartnerGroupCode no parent | Validation | Parent=null | false | P0 |
| UNT-006 | CanModifyPartnerGroupCode parent category | Validation | Parent is category | true | P0 |
| UNT-007 | CanModifyPartnerGroupCode parent group | Validation | Parent is group | CanModifyPartnerGroupCode(parent) | P0 |
| UNT-008 | BuildHierarchy lookup empty | Logic | lookup[""] empty | Empty sequence | P1 |
| UNT-009 | BuildHierarchy single root | Logic | 1 item with Parent="" | 1 item | P1 |
| UNT-010 | BuildHierarchy parent-child | Logic | Parent="A", Child Parent="A" | Child under parent | P1 |
| UNT-011 | BuildHierarchy visitedCodes | Logic | Circular reference | No duplicate | P1 |
| UNT-012 | Parent normalization | Formatting | Parent="   " | "" | P1 |
| UNT-013 | Parent null | Formatting | Parent=null | "" | P1 |
| UNT-014 | GetAllDescendantsAsync empty | Logic | No children | Empty list | P1 |
| UNT-015 | GetAllDescendantsAsync one level | Logic | 1 child | List with 1 ID | P1 |
| UNT-016 | GetAllDescendantsAsync multi level | Logic | 3 levels | All IDs | P1 |
| UNT-017 | GetParentCategory direct | Logic | Parent is category | Parent | P1 |
| UNT-018 | GetParentCategory recursive | Logic | Parent is group | Traverse up | P1 |
| UNT-019 | ProcessAllLevelsForCategories PartnerCategoryEditable | Logic | PartnerCategoryEditable=true | Category added | P1 |
| UNT-020 | CollectAllEditableGroups PartnerGroupEditable | Logic | PartnerGroupEditable=true | Group added | P1 |
| UNT-021 | Lookup key empty | Logic | Parent null | "" | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetPartnerTreesAsync | 100 trees | < 500ms | P0 |
| PRF-002 | GetPartnerTreeAsync | Single get | < 100ms | P0 |
| PRF-003 | CreatePartnerTreeAsync | Create | < 200ms | P0 |
| PRF-004 | UpdatePartnerTreeAsync | Update | < 200ms | P0 |
| PRF-005 | DeletePartnerTreeAsync | Delete | < 200ms | P0 |
| PRF-006 | GetCategoryAndGroupStructureAsync | Structure | < 500ms | P0 |
| PRF-007 | GetAllDescendantsAsync | 50 descendants | < 100ms | P1 |
| PRF-008 | BuildHierarchy | 100 nodes | < 100ms | P1 |
| PRF-009 | GetBasicPartnerCategoryDetailsAsync | Full load | < 2s | P1 |
| PRF-010 | GetBasicPartnerGroupDetailsAsync | Full load | < 2s | P1 |
| PRF-011 | LoadPartnerTreesAsync cache hit | Cached | < 50ms | P1 |
| PRF-012 | LoadPartnerTreesAsync cache miss | Uncached | < 500ms | P1 |
| PRF-013 | API GET list | Full list | < 500ms | P1 |
| PRF-014 | API GET by-partner-group-id | Paginated | < 500ms | P1 |
| PRF-015 | API GET categorization-overview | Overview | < 500ms | P1 |
| PRF-016 | MapEntityToModel | Single model | < 50ms | P1 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | GET /api/partner-tree | 20 req/s | 5 min | 95% < 500ms | P0 |
| LDT-002 | GET /api/partner-tree/{id} | 20 req/s | 5 min | 95% < 200ms | P0 |
| LDT-003 | GET /api/partner-tree-structure | 10 req/s | 5 min | 95% < 500ms | P0 |
| LDT-004 | POST /api/partner-tree | 5 req/s | 5 min | 95% < 500ms | P0 |
| LDT-005 | PUT /api/partner-tree | 5 req/s | 5 min | 95% < 500ms | P0 |
| LDT-006 | DELETE /api/partner-tree/{id} | 5 req/s | 5 min | 95% < 500ms | P0 |
| LDT-007 | GET /api/partner-tree/{id}/permissions | 20 req/s | 5 min | 95% < 200ms | P0 |
| LDT-008 | GET /api/partner-tree/by-partner-group-id | 15 req/s | 5 min | 95% < 500ms | P0 |
| LDT-009 | GET /api/partner-tree/categorization-overview | 10 req/s | 5 min | 95% < 500ms | P0 |
| LDT-010 | Mixed GET/POST/PUT/DELETE | 20 req/s total | 5 min | 95% < 500ms | P0 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
