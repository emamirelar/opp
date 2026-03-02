# OrganizationHierarchyManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/OrganizationHierarchyManager`  
**Related:** `OrganizationHierarchyService`, `ValuesRepository`, `OrganizationHierarchyController`  
**Created:** 2026-02-18 | **Last Updated:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive (P) | 30 | 30 | ✅ |
| §2 Negative (N) | 90 | 90 | ✅ |
| §3 Boundary/Edge (E) | 90 | 90 | ✅ |
| §4 Functional (F) | 90 | 90 | ✅ |
| §5 Integration (I) | 90 | 90 | ✅ |
| §6 Security | 50 | 50 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **462** | ✅ |

### 3:1 Ratio Compliance Check

| Category | Count | Tests | N≥3P? | E≥3P? | F≥3P? | I≥3P? |
|----------|-------|-------|-------|-------|-------|-------|
| Positive (P) | 30 | POS-001..POS-030 | — | — | — | — |
| Negative (N) | 90 | NEG-001..NEG-090 | ✅ 90≥90 | — | — | — |
| Edge/Boundary (E) | 90 | BND-001..BND-090 | — | ✅ 90≥90 | — | — |
| Functional (F) | 90 | FUN-001..FUN-090 | — | — | ✅ 90≥90 | — |
| Integration (I) | 90 | INT-001..INT-090 | — | — | — | ✅ 90≥90 |

---

## Feature Overview

**OrganizationHierarchyManager** manages organization unit hierarchy, tree traversal, type filtering, and PrimeNG tree format. Key responsibilities: full tree (legacy), PrimeNG tree format, get by ID, get by type (Office/Region/Hub/OrgUnit), get all organizations. **OrganizationHierarchyService** handles paginated list, search, caching (30 min), children/entity relationship counts, filtering, and sorting. **OrganizationUnitRelationship** links org units to Partner/Contact/Interaction entities. DoA holders assigned via EntityUserRole (EntityType="OrganizationHierarchy", Codes: DoA2_OrganizationHierarchy, DoA3_OrganizationHierarchy).

**API Endpoints:**
- GET api/organizationhierarchy — Paginated list (filter request)
- POST api/organizationhierarchy/search — Search
- GET api/organizationhierarchy/{id} — By ID (service path)
- GET api/organization-hierarchy — PrimeNG tree
- GET api/organization-hierarchy/legacy — Legacy tree
- GET api/organization-hierarchy/{id} — By ID (manager path)
- GET api/organization-hierarchy/metadata-info — Metadata

**OrganizationUnitType enum:** Office (0), Region (1), Hub (2), OrgUnit (3)

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | GetOrganizationHierarchyPrime returns tree | Active org units exist | Call GET api/organization-hierarchy | 200, array of root nodes with Children, each node has Data.Id/Code/Name/Type/Description/ParentId | P0 |
| POS-002 | GetOrganizationHierarchy returns legacy tree | Active org units exist | Call GET api/organization-hierarchy/legacy | 200, array of root nodes with Data.Children recursively built | P0 |
| POS-003 | GetOrganizationHierarchyById returns single unit | Org unit ID 5 exists and is active | Call GetOrganizationHierarchyById(5) | OrganizationHierarchyModel with Id=5, Code, Name, Type, Description, ParentId | P0 |
| POS-004 | GetOrganizationsByType Office returns offices | At least one Office type exists | Call GetOrganizationsByType(OrganizationUnitType.Office) | Non-empty enumerable, all items have Type=Office | P0 |
| POS-005 | GetOrganizationsByType Region returns regions | At least one Region type exists | Call GetOrganizationsByType(OrganizationUnitType.Region) | Non-empty enumerable, all items have Type=Region | P0 |
| POS-006 | GetOrganizationsByType Hub returns hubs | At least one Hub type exists | Call GetOrganizationsByType(OrganizationUnitType.Hub) | Non-empty enumerable, all items have Type=Hub | P0 |
| POS-007 | GetOrganizationsByType OrgUnit returns org units | At least one OrgUnit type exists | Call GetOrganizationsByType(OrganizationUnitType.OrgUnit) | Non-empty enumerable, all items have Type=OrgUnit | P0 |
| POS-008 | GetAllOrganizations returns flat list | Org units exist | Call GetAllOrganizations() | Flat list ordered by Name, includes non-deleted units | P0 |
| POS-009 | Paginated list returns first page | Org units exist | GET api/organizationhierarchy?PageIndex=1&PageSize=10 | 200, PaginationResponse with Records, TotalCount, PageIndex=1, PageSize=10 | P0 |
| POS-010 | Paginated list with IncludeCounts | Org units exist | GET api/organizationhierarchy?IncludeCounts=true | 200, each record has ChildrenCount and EntityRelationshipCount populated | P0 |
| POS-011 | Search by SearchTerm finds matches | Org unit "HQ" exists | POST api/organizationhierarchy/search with SearchTerm="HQ" | 200, records where Name/Code/Description contains "HQ" | P0 |
| POS-012 | Search by Type filters correctly | Org units of mixed types exist | POST search with Type="Office" | 200, all records have Type=Office | P0 |
| POS-013 | Search by ParentId filters children | Parent ID 10 has children | POST search with ParentId=10 | 200, all records have ParentId=10 | P0 |
| POS-014 | Filter by Name on list endpoint | Org unit "Europe" exists | GET api/organizationhierarchy?Name=Europe | 200, records where Name contains "Europe" | P0 |
| POS-015 | Filter by Code on list endpoint | Org unit with Code "EU-01" exists | GET api/organizationhierarchy?Code=EU-01 | 200, records where Code contains "EU-01" | P0 |
| POS-016 | Filter by ParentCode | Parent has Code "HQ" | GET api/organizationhierarchy?ParentCode=HQ | 200, records whose parent Code contains "HQ" | P1 |
| POS-017 | Filter by IsSelfManagementEnabled | Org units with IsSelfManagementEnabled=true exist | GET api/organizationhierarchy?IsSelfManagementEnabled=true | 200, all records have IsSelfManagementEnabled=true | P1 |
| POS-018 | Sort by Name ascending | Org units exist | GET api/organizationhierarchy?OrderBy=Name&Ascending=true | 200, records ordered A–Z by Name | P1 |
| POS-019 | Sort by Name descending | Org units exist | GET api/organizationhierarchy?OrderBy=Name&Ascending=false | 200, records ordered Z–A by Name | P1 |
| POS-020 | Sort by ChildrenCount | Org units with children exist | GET api/organizationhierarchy?OrderBy=ChildrenCount&IncludeCounts=true | 200, records ordered by ChildrenCount | P1 |
| POS-021 | Sort by EntityRelationshipCount | Org units with relationships exist | GET api/organizationhierarchy?OrderBy=EntityRelationshipCount&IncludeCounts=true | 200, records ordered by EntityRelationshipCount | P1 |
| POS-022 | GetOrganizationHierarchyByIdWithDetails returns full model | Org unit ID 7 exists | GET api/organizationhierarchy/7 | 200, OrganizationHierarchyModel with ChildrenCount, EntityRelationshipCount | P1 |
| POS-023 | Prime tree root nodes have Expanded=true | Org units exist | Call GetOrganizationHierarchyPrime() | Root nodes have Expanded=true | P1 |
| POS-024 | Prime tree child nodes have Expanded=false | Org units with children exist | Call GetOrganizationHierarchyPrime() | Child nodes have Expanded=false | P1 |
| POS-025 | Prime tree nodes have Type="person" | Org units exist | Call GetOrganizationHierarchyPrime() | All nodes have Type="person" | P1 |
| POS-026 | Legacy tree preserves parent-child structure | 3-level hierarchy exists | Call GetOrganizationHierarchy() | Root→Children→Children structure matches DB | P1 |
| POS-027 | GetMetadataInfo returns entity configuration | User has read permission | GET api/organization-hierarchy/metadata-info | 200, entity and field metadata for OrganizationHierarchy | P1 |
| POS-028 | Pagination second page | 25+ org units exist | GET api/organizationhierarchy?PageIndex=2&PageSize=10 | 200, PageIndex=2, records 11–20 | P1 |
| POS-029 | Search with empty SearchTerm returns all | Org units exist | POST search with SearchTerm=null | 200, all matching other filters | P1 |
| POS-030 | Filter by Status Active | Org units with Status=Active exist | GET api/organizationhierarchy?Status=Active | 200, all records have Status=Active | P1 |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input/Condition | Expected Result | Priority |
|----|-----------|------------------------|-----------------|----------|
| NEG-001 | GetOrganizationHierarchyById with non-existent ID | id=999999 | Returns null or empty model; controller returns NotFound | P0 |
| NEG-002 | GetOrganizationHierarchyById with zero | id=0 | Returns null or NotFound | P0 |
| NEG-003 | GetOrganizationHierarchyById with negative ID | id=-1 | Returns null or NotFound | P0 |
| NEG-004 | GetOrganizationHierarchyByIdWithDetails non-existent | id=999999 | 404, BusinessException "not found" | P0 |
| NEG-005 | Paginated list with PageIndex=0 | PageIndex=0 | Empty or first page; or 400 if validation rejects | P0 |
| NEG-006 | Paginated list with PageSize=0 | PageSize=0 | 400 or default page size applied | P0 |
| NEG-007 | Paginated list with negative PageSize | PageSize=-5 | 400 or validation error | P0 |
| NEG-008 | Search with invalid Type enum value | Type="InvalidType" | Empty results or 400 | P0 |
| NEG-009 | Search with Type="Office" when none exist | No Office types | 200, empty Records | P0 |
| NEG-010 | GetOrganizationsByType with invalid cast | Invalid type (if applicable) | Empty or exception | P0 |
| NEG-011 | Unauthenticated GET api/organizationhierarchy | No auth token | 401 Unauthorized | P0 |
| NEG-012 | Unauthenticated GET api/organization-hierarchy | No auth token | 401 Unauthorized | P0 |
| NEG-013 | Unauthenticated POST search | No auth token | 401 Unauthorized | P0 |
| NEG-014 | User without read permission on list | User lacks OrganizationHierarchy read | 403 Forbidden | P0 |
| NEG-015 | User without read permission on search | User lacks OrganizationHierarchy read | 403 Forbidden | P0 |
| NEG-016 | GetOrganizationHierarchyById for soft-deleted unit | id of IsDeleted=true unit | null or NotFound | P0 |
| NEG-017 | GetOrganizationHierarchy for inactive unit | Status=Inactive unit | Excluded from tree (ValuesRepository filters Active) | P0 |
| NEG-018 | GetOrganizationHierarchyPrime excludes inactive | Status=Inactive unit | Excluded from Prime tree | P0 |
| NEG-019 | Filter ParentId for non-existent parent | ParentId=999999 | 200, empty Records | P1 |
| NEG-020 | Search ParentId for non-existent | ParentId=999999 | 200, empty Records | P1 |
| NEG-021 | Paginated list PageIndex beyond total pages | PageIndex=1000, 10 total | 200, empty Records, TotalPages correct | P1 |
| NEG-022 | Search with malformed JSON body | Invalid JSON in POST search | 400 Bad Request | P1 |
| NEG-023 | Search with missing required fields | Empty body (if required) | 400 or default behavior | P1 |
| NEG-024 | GetMetadataInfo without permission | User lacks entity config read | 403 or 500 per implementation | P1 |
| NEG-025 | GetMetadataInfo for non-existent entity type | EntityType typo | 500 or 404 | P1 |
| NEG-026 | OrderBy invalid column | OrderBy="InvalidColumn" | Default sort (Name) applied | P1 |
| NEG-027 | OrderBy null/empty | OrderBy=null | Default sort applied | P1 |
| NEG-028 | Filter Name with special SQL chars | Name="'; DROP TABLE--" | No SQL injection; safe filter | P1 |
| NEG-029 | Filter Code with XSS payload | Code="<script>alert(1)</script>" | No XSS; safe handling | P1 |
| NEG-030 | SearchTerm with SQL injection attempt | SearchTerm="' OR 1=1--" | No injection; safe search | P1 |
| NEG-031 | GetOrganizationHierarchyById with very large ID | id=2147483647 | null or NotFound | P1 |
| NEG-032 | Paginated list with PageSize=2147483647 | PageSize=int.MaxValue | Throttled or capped; no OOM | P1 |
| NEG-033 | Search with PageIndex=0 | PageIndex=0 | First page or validation error | P1 |
| NEG-034 | Filter Status with invalid value | Status="InvalidStatus" | Empty or default filter | P1 |
| NEG-035 | Filter Type case mismatch | Type="office" (lowercase) | Match if OrdinalIgnoreCase; else empty | P1 |
| NEG-036 | GetOrganizationsByType when no units of type | Type=Hub, no hubs | Empty enumerable | P1 |
| NEG-037 | GetAllOrganizations when DB empty | No org units | Empty enumerable | P1 |
| NEG-038 | GetOrganizationHierarchy when DB empty | No org units | Empty array | P1 |
| NEG-039 | GetOrganizationHierarchyPrime when DB empty | No org units | Empty array | P1 |
| NEG-040 | Prime tree excludes OrgUnit with ParentId=null and Type!=0 | Type=OrgUnit root (edge case) | Excluded per ValuesRepository filter | P1 |
| NEG-041 | Expired or invalid JWT | Expired token | 401 Unauthorized | P1 |
| NEG-042 | Tampered JWT | Modified token | 401 Unauthorized | P1 |
| NEG-043 | Wrong HTTP method GET on search URL | GET api/organizationhierarchy/search | 405 Method Not Allowed | P1 |
| NEG-044 | Wrong HTTP method POST on list URL | POST api/organizationhierarchy | 405 Method Not Allowed | P1 |
| NEG-045 | GET api/organizationhierarchy/{id} with string id | id="abc" | 400 or 404 | P1 |
| NEG-046 | Search with MinChildrenCount > MaxChildrenCount | Min=10, Max=5 | Empty results | P1 |
| NEG-047 | Search with negative MinChildrenCount | MinChildrenCount=-1 | Validation or empty | P1 |
| NEG-048 | Search with negative MaxChildrenCount | MaxChildrenCount=-1 | Validation or empty | P1 |
| NEG-049 | Filter IsSelfManagementEnabled with invalid value | Non-boolean value | 400 or default | P1 |
| NEG-050 | Paginated list with negative PageIndex | PageIndex=-1 | 400 or first page | P1 |
| NEG-051 | GetOrganizationHierarchyById for inactive unit | id of Status=Inactive | May return; service filters Active in cache | P1 |
| NEG-052 | Service GetOrganizationHierarchyByIdAsync non-existent | id=999999 | null | P1 |
| NEG-053 | ValuesRepository GetOrganizationHierarchyById deleted | id of IsDeleted unit | null | P1 |
| NEG-054 | ValuesRepository GetOrganizationsByType excludes deleted | Type=Office | Only !IsDeleted units | P1 |
| NEG-055 | ValuesRepository GetAllOrganizations excludes deleted | Call GetAllOrganizations | Only !IsDeleted | P1 |
| NEG-056 | ValuesRepository GetOrganizationHierarchy excludes deleted | Call GetOrganizationHierarchy | Only !IsDeleted, Active | P1 |
| NEG-057 | ValuesRepository GetOrganizationHierarchyPrime excludes deleted | Call GetOrganizationHierarchyPrime | Only !IsDeleted, Active | P1 |
| NEG-058 | Service cache excludes deleted/inactive | Unit soft-deleted after cache load | Excluded on next cache refresh (30 min) | P1 |
| NEG-059 | Filter by non-existent ParentCode | ParentCode="NONEXISTENT" | 200, empty Records | P1 |
| NEG-060 | Search by SearchTerm no matches | SearchTerm="xyznonexistent123" | 200, empty Records | P1 |
| NEG-061 | Filter Name no matches | Name="xyznonexistent123" | 200, empty Records | P1 |
| NEG-062 | Filter Code no matches | Code="xyznonexistent123" | 200, empty Records | P1 |
| NEG-063 | Filter Type no matches | Type="Hub" when no hubs | 200, empty Records | P1 |
| NEG-064 | Filter Status no matches | Status="Inactive" when all Active | 200, empty Records or filtered | P1 |
| NEG-065 | Filter ParentId no children | ParentId with no children | 200, empty Records | P1 |
| NEG-066 | Search Type no matches | Type="Region" when no regions | 200, empty Records | P1 |
| NEG-067 | Search Status no matches | Status="Inactive" | 200, empty Records or filtered | P1 |
| NEG-068 | Search ParentId no children | ParentId with no children | 200, empty Records | P1 |
| NEG-069 | GetOrganizationHierarchyById with null mapper result | Entity exists but mapper returns null | NullReference or handled | P1 |
| NEG-070 | Paginated list with IncludeCounts=false | IncludeCounts=false | ChildrenCount/EntityRelationshipCount may be 0 | P1 |
| NEG-071 | Service PopulateCountsAsync with empty list | Empty paged list | No exception | P1 |
| NEG-072 | ApplyFilters with all null/empty | All filter fields null | Returns all (filtered by pagination) | P1 |
| NEG-073 | ApplySearchFilters with all null | All search fields null | Returns all | P1 |
| NEG-074 | ApplySorting with null orderBy | orderBy=null | Default Name sort | P1 |
| NEG-075 | BuildChildren with no children | parentId with no children | Empty Children list | P1 |
| NEG-076 | BuildPrimeChildren with no children | parentId with no children | Empty Children list | P1 |
| NEG-077 | Prime tree root with Type=OrgUnit and ParentId=null | Edge: OrgUnit as root | Excluded (filter: Type==0 \|\| ParentId!=null) | P1 |
| NEG-078 | Legacy tree root with no root units | All units have ParentId | Empty result | P1 |
| NEG-079 | Prime tree root with no root units | All units have ParentId | Empty result | P1 |
| NEG-080 | GetOrganizationHierarchyById includes deleted children | Parent not deleted, child deleted | Child excluded from Include | P1 |
| NEG-081 | OrganizationUnitRelationship with deleted org unit | OrgUnit soft-deleted | EntityRelationshipCount may exclude per filter | P1 |
| NEG-082 | Children count cache excludes deleted children | Child soft-deleted | Count updated on cache refresh (15 min) | P1 |
| NEG-083 | Entity relationship count includes deleted relationships | OrganizationUnitRelationship not filtered by IsDeleted | Depends on implementation | P1 |
| NEG-084 | Filter Ascending with invalid value | Ascending=null in search | Default true | P1 |
| NEG-085 | PaginationRequest missing PageIndex | Default PageIndex | Uses default (e.g. 1) | P1 |
| NEG-086 | PaginationRequest missing PageSize | Default PageSize | Uses default | P1 |
| NEG-087 | SearchRequest PageSize=0 | PageSize=0 | Validation or default | P1 |
| NEG-088 | SearchRequest PageIndex=0 | PageIndex=0 | First page or validation | P1 |
| NEG-089 | GetMetadataInfo when EntityConfigurationManager throws | Internal exception | 500, error response | P1 |
| NEG-090 | Controller HandleOperationAsync propagates exception | Manager throws | 500 or BusinessException response | P1 |

---

## §3 Boundary/Edge Tests (90)

| ID | Field/Scenario | Boundary | At Boundary | Over/Under | Expected | Priority |
|----|----------------|----------|-------------|-----------|----------|----------|
| BND-001 | PageIndex | 1 | PageIndex=1 | PageIndex=0 | First page returned | P0 |
| BND-002 | PageSize | 1 | PageSize=1 | PageSize=0 | Single record or validation | P0 |
| BND-003 | PageSize | int.MaxValue | PageSize=10000 | Larger | Capped or performance limit | P0 |
| BND-004 | Id | 1 | id=1 (min valid) | id=0 | Valid unit or NotFound | P0 |
| BND-005 | Id | int.MaxValue | id=2147483647 | Overflow | NotFound or handled | P0 |
| BND-006 | ParentId | null | Root unit ParentId=null | N/A | Root in tree | P0 |
| BND-007 | ParentId | Valid parent | ParentId=5 | Self-reference (ParentId=own Id) | No cycle; exclude or error | P0 |
| BND-008 | OrganizationUnitType | Office (0) | Type=Office | Type=-1 | Valid or exclude | P0 |
| BND-009 | OrganizationUnitType | OrgUnit (3) | Type=OrgUnit | Type=4 | Valid or exclude | P0 |
| BND-010 | Name | Empty string | Name="" | null | Filter behavior | P0 |
| BND-011 | Name | Single char | Name="A" | Empty | Match if contains | P1 |
| BND-012 | Name | Max length | Name=200 chars | 201+ chars | Truncate or reject | P1 |
| BND-013 | Code | Empty | Code="" | null | Filter behavior | P1 |
| BND-014 | Code | Single char | Code="X" | Empty | Match if contains | P1 |
| BND-015 | Description | Empty | Description="" | null | Search excludes or includes | P1 |
| BND-016 | Description | Very long | 4000 chars | 4001+ | DB limit or truncate | P1 |
| BND-017 | SearchTerm | Empty | SearchTerm="" | null | All results (no filter) | P1 |
| BND-018 | SearchTerm | Single char | SearchTerm="a" | Empty | Match if contains | P1 |
| BND-019 | SearchTerm | Exact match | SearchTerm="HQ" | Partial "H" | Match "HQ" | P1 |
| BND-020 | SearchTerm | Case insensitive | SearchTerm="hq" | "HQ" in DB | Match | P1 |
| BND-021 | Tree depth | 1 level | Root only | 0 levels | Single root | P1 |
| BND-022 | Tree depth | 2 levels | Root + children | 3+ levels | Recursive build | P1 |
| BND-023 | Tree depth | Deep hierarchy | 10+ levels | Max depth | No stack overflow | P1 |
| BND-024 | Children count | 0 | No children | Negative | ChildrenCount=0 | P1 |
| BND-025 | Children count | Many | 100+ children | Overflow | Correct count | P1 |
| BND-026 | EntityRelationshipCount | 0 | No relationships | Negative | EntityRelationshipCount=0 | P1 |
| BND-027 | EntityRelationshipCount | Many | 1000+ | Overflow | Correct count | P1 |
| BND-028 | TotalCount | 0 | No org units | Negative | TotalCount=0 | P1 |
| BND-029 | TotalCount | Large | 10000+ | Overflow | Correct count | P1 |
| BND-030 | TotalPages | 0 | TotalCount=0 | Negative | TotalPages=0 or 1 | P1 |
| BND-031 | TotalPages | Ceiling | TotalCount=25, PageSize=10 | 2.5 | TotalPages=3 | P1 |
| BND-032 | Last page partial | Page 3, 25 total, PageSize=10 | 5 records | 0 records | 5 records on last page | P1 |
| BND-033 | OrderBy | "name" | Lowercase | Mixed case | Case-insensitive sort | P1 |
| BND-034 | OrderBy | "childrencount" | Exact | "ChildrenCount" | Match | P1 |
| BND-035 | OrderBy | "entityrelationshipcount" | Exact | Typo | Default sort | P1 |
| BND-036 | Ascending | true | Default | false | Ascending order | P1 |
| BND-037 | Ascending | false | Descending | null | Descending order | P1 |
| BND-038 | IsSelfManagementEnabled | true | Filter true | false | Only true units | P1 |
| BND-039 | IsSelfManagementEnabled | false | Filter false | null | Only false units | P1 |
| BND-040 | Status | Active | Filter Active | Inactive | Only Active | P1 |
| BND-041 | Status | Inactive | Filter Inactive | Active | Only Inactive | P1 |
| BND-042 | Type filter | "Office" | Exact | "office" | Case-insensitive match | P1 |
| BND-043 | Type filter | "Region" | Exact | "REGION" | Match | P1 |
| BND-044 | Type filter | "Hub" | Exact | "hub" | Match | P1 |
| BND-045 | Type filter | "OrgUnit" | Exact | "orgunit" | Match | P1 |
| BND-046 | ParentCode | Partial match | ParentCode="HQ" | "HQ-01" | Contains match | P1 |
| BND-047 | ParentCode | No parent | Unit has ParentId=null | ParentCode filter | Excluded | P1 |
| BND-048 | Cache TTL | 30 min | After 30 min | 31 min | Cache refresh | P1 |
| BND-049 | Children count cache TTL | 15 min | After 15 min | 16 min | Count cache refresh | P1 |
| BND-050 | Entity relationship cache TTL | 15 min | After 15 min | 16 min | Count cache refresh | P1 |
| BND-051 | Prime Type=0 root | Office (0) at root | Type=0, ParentId=null | Type=1 at root | Included | P1 |
| BND-052 | Prime Type!=0 with ParentId | OrgUnit with parent | Type=3, ParentId=5 | Type=3, ParentId=null | Included | P1 |
| BND-053 | Prime Type!=0 without ParentId | OrgUnit root | Type=3, ParentId=null | Excluded | Excluded | P1 |
| BND-054 | Legacy tree root ordering | Multiple roots | OrderBy Name | Consistent order | P1 |
| BND-055 | Prime tree root ordering | Multiple roots | OrderBy Name | Consistent order | P1 |
| BND-056 | Child ordering in BuildChildren | Siblings | OrderBy Name from allUnits | Consistent order | P1 |
| BND-057 | Child ordering in BuildPrimeChildren | Siblings | OrderBy Name | Consistent order | P1 |
| BND-058 | Null Parent in hierarchy | Parent soft-deleted | ParentId=5, Parent null | ParentName/ParentCode null | P1 |
| BND-059 | Null Description in model | Description=null | Map to model | "No description available" in Prime | P1 |
| BND-060 | Null Name in entity | Name=null | Map to model | "Unnamed" in Prime | P1 |
| BND-061 | Null Code in entity | Code=null | Map to model | "N/A" in Prime | P1 |
| BND-062 | Pagination Skip | PageIndex=2, PageSize=10 | Skip(10) | Correct offset | P1 |
| BND-063 | Pagination Take | PageSize=10 | Take(10) | Max 10 records | P1 |
| BND-064 | Filter Name contains | Substring | "Europe" in "Europe Region" | Match | P1 |
| BND-065 | Filter Code contains | Substring | "EU" in "EU-01" | Match | P1 |
| BND-066 | Search Name contains | Substring | "HQ" in "Global HQ" | Match | P1 |
| BND-067 | Search Code contains | Substring | "01" in "ORG-01" | Match | P1 |
| BND-068 | Search Description contains | Substring | "regional" in description | Match | P1 |
| BND-069 | MinChildrenCount filter | Min=5 | Units with 5+ children | Units with 4 | Excluded | P1 |
| BND-070 | MaxChildrenCount filter | Max=10 | Units with ≤10 children | Units with 11 | Excluded | P1 |
| BND-071 | MinChildrenCount=MaxChildrenCount | Min=5, Max=5 | Units with exactly 5 | Units with 4 or 6 | Excluded | P1 |
| BND-072 | IncludeCounts and sort by count | IncludeCounts=true, OrderBy=ChildrenCount | Counts populated before sort | Correct order | P1 |
| BND-073 | PopulateCountsAsync single item | List of 1 | One hierarchy | Counts correct | P1 |
| BND-074 | PopulateCountsAsync many items | List of 100 | All get counts | No N+1 | P1 |
| BND-075 | GroupBy ParentId empty | No children | Empty dict | ChildrenCount=0 | P1 |
| BND-076 | GroupBy OrganizationHierarchyId empty | No relationships | Empty dict | EntityRelationshipCount=0 | P1 |
| BND-077 | GetValueOrDefault for missing parent | Hierarchy not in children dict | 0 | ChildrenCount=0 | P1 |
| BND-078 | GetValueOrDefault for missing relationship | Hierarchy not in relationship dict | 0 | EntityRelationshipCount=0 | P1 |
| BND-079 | ApplyFilters chained | Multiple filters | Name + Type + ParentId | All applied | P1 |
| BND-080 | ApplySearchFilters chained | Multiple filters | SearchTerm + Type + Status | All applied | P1 |
| BND-081 | ApplySorting default | orderBy unknown | "unknown" | Default Name ascending | P1 |
| BND-082 | ApplySorting "parentname" | orderBy="parentname" | Ascending | ParentName sort | P1 |
| BND-083 | ApplySorting "status" | orderBy="status" | Ascending | Status sort | P1 |
| BND-084 | ApplySorting "type" | orderBy="type" | Ascending | Type sort | P1 |
| BND-085 | ApplySorting "code" | orderBy="code" | Ascending | Code sort | P1 |
| BND-086 | GetAllOrganizations includes Inactive | No Status filter | Inactive units included | Flat list | P1 |
| BND-087 | GetOrganizationsByType excludes Inactive | Type=Office | Only Active | P1 |
| BND-088 | GetOrganizationHierarchy excludes Inactive | Tree build | Only Active | P1 |
| BND-089 | GetOrganizationHierarchyPrime excludes Inactive | Prime build | Only Active | P1 |
| BND-090 | Service GetAllOrganizationHierarchiesAsync excludes Inactive | Cache load | Only Active, !IsDeleted | P1 |

---

## §4 Functional Tests (90)

| ID | Test Name | Rule/Scenario | Trigger | Expected Outcome | Priority |
|----|-----------|---------------|---------|------------------|----------|
| FUN-001 | Tree structure preserves parent-child | Parent-child relationship | GetOrganizationHierarchy | Each child's ParentId matches parent's Id | P0 |
| FUN-002 | Prime tree structure matches legacy | Same data | GetOrganizationHierarchyPrime vs GetOrganizationHierarchy | Same hierarchy structure | P0 |
| FUN-003 | Root units have ParentId=null | Root definition | GetOrganizationHierarchy | All roots have ParentId=null | P0 |
| FUN-004 | Children have correct ParentId | Child definition | BuildChildren | child.ParentId==parent.Id | P0 |
| FUN-005 | GetOrganizationsByType filters by enum | Type filter | GetOrganizationsByType(Office) | All Type==Office | P0 |
| FUN-006 | GetAllOrganizations returns flat list | No hierarchy | GetAllOrganizations | No nested Children | P0 |
| FUN-007 | Pagination applies after filter | Filter then paginate | GET list with Name filter | Filtered then paged | P0 |
| FUN-008 | Pagination applies after sort | Sort then paginate | GET list with OrderBy | Sorted then paged | P0 |
| FUN-009 | TotalCount reflects filtered count | Filter reduces results | GET list with Type=Hub | TotalCount = filtered count | P0 |
| FUN-010 | TotalPages = ceil(TotalCount/PageSize) | Page calculation | Various PageSize | TotalPages correct | P0 |
| FUN-011 | IncludeCounts populates ChildrenCount | Count logic | GET list IncludeCounts=true | ChildrenCount from GroupBy ParentId | P0 |
| FUN-012 | IncludeCounts populates EntityRelationshipCount | Count logic | GET list IncludeCounts=true | EntityRelationshipCount from OrganizationUnitRelationships | P0 |
| FUN-013 | Search SearchTerm matches Name | Search logic | POST search SearchTerm="X" | Name.Contains("X") | P0 |
| FUN-014 | Search SearchTerm matches Code | Search logic | POST search SearchTerm="Y" | Code.Contains("Y") | P0 |
| FUN-015 | Search SearchTerm matches Description | Search logic | POST search SearchTerm="Z" | Description.Contains("Z") | P0 |
| FUN-016 | Filter Name case insensitive | OrdinalIgnoreCase | GET Name="europe" | Matches "Europe" | P0 |
| FUN-017 | Filter Code case insensitive | OrdinalIgnoreCase | GET Code="eu-01" | Matches "EU-01" | P0 |
| FUN-018 | Filter Type case insensitive | Equals OrdinalIgnoreCase | GET Type="office" | Matches "Office" | P0 |
| FUN-019 | Filter Status case insensitive | Equals OrdinalIgnoreCase | GET Status="active" | Matches "Active" | P0 |
| FUN-020 | Filter ParentCode case insensitive | Contains OrdinalIgnoreCase | GET ParentCode="hq" | Matches parent Code "HQ" | P0 |
| FUN-021 | Search Type filter | Type in search | POST search Type="Region" | Only Region | P0 |
| FUN-022 | Search Status filter | Status in search | POST search Status="Active" | Only Active | P0 |
| FUN-023 | Search ParentId filter | ParentId in search | POST search ParentId=5 | Only children of 5 | P0 |
| FUN-024 | Search IsSelfManagementEnabled filter | Boolean filter | POST search IsSelfManagementEnabled=true | Only true | P0 |
| FUN-025 | Default OrderBy is Name | Sort default | No OrderBy | OrderBy Name | P0 |
| FUN-026 | Default Ascending is true | Sort default | No Ascending | Ascending=true | P0 |
| FUN-027 | Sort by Name ascending | Sort logic | OrderBy=Name, Ascending=true | A–Z | P0 |
| FUN-028 | Sort by Name descending | Sort logic | OrderBy=Name, Ascending=false | Z–A | P0 |
| FUN-029 | Sort by Code | Sort logic | OrderBy=Code | By Code | P0 |
| FUN-030 | Sort by Type | Sort logic | OrderBy=Type | By Type enum | P0 |
| FUN-031 | Sort by ChildrenCount | Sort logic | OrderBy=ChildrenCount | By count | P0 |
| FUN-032 | Sort by EntityRelationshipCount | Sort logic | OrderBy=EntityRelationshipCount | By count | P0 |
| FUN-033 | Sort by ParentName | Sort logic | OrderBy=ParentName | By ParentName | P0 |
| FUN-034 | Sort by Status | Sort logic | OrderBy=Status | By Status | P0 |
| FUN-035 | Cache key ORGANIZATION_HIERARCHY_CACHE | Cache key | GetAllOrganizationHierarchiesAsync | Uses CACHE_KEY | P0 |
| FUN-036 | Cache expiration 30 min | Cache TTL | Cache entry | AbsoluteExpirationRelativeToNow=30 min | P0 |
| FUN-037 | Children count cache 15 min | Count cache TTL | PopulateCountsAsync | 15 min expiration | P0 |
| FUN-038 | Entity relationship count cache 15 min | Count cache TTL | PopulateCountsAsync | 15 min expiration | P0 |
| FUN-039 | Cache includes Parent | Include in query | GetAllOrganizationHierarchiesAsync | Include(oh=>oh.Parent) | P0 |
| FUN-040 | Cache populates ParentName | Parent mapping | Parent not null | hierarchy.ParentName=Parent.Name | P0 |
| FUN-041 | Cache populates ParentCode | Parent mapping | Parent not null | hierarchy.ParentCode=Parent.Code | P0 |
| FUN-042 | Cache filters IsDeleted | Soft delete | Query | Where(!oh.IsDeleted) | P0 |
| FUN-043 | Cache filters EntityStatus.Active | Status filter | Query | Where(oh.Status==Active) | P0 |
| FUN-044 | ValuesRepository GetOrganizationHierarchy filters Active | Tree build | allUnits | !IsDeleted, Status=Active | P0 |
| FUN-045 | ValuesRepository GetOrganizationHierarchyPrime filters | Prime build | allUnits | !IsDeleted, Status=Active, (Type==0 \|\| ParentId!=null) | P0 |
| FUN-046 | ValuesRepository GetOrganizationsByType filters | Type query | GetOrganizationsByType | !IsDeleted, Type==type, Status=Active | P0 |
| FUN-047 | ValuesRepository GetAllOrganizations filters | All query | GetAllOrganizations | !IsDeleted | P0 |
| FUN-048 | ValuesRepository GetOrganizationHierarchyById filters | By ID | GetOrganizationHierarchyById | !IsDeleted | P0 |
| FUN-049 | AutoMapper maps entity to OrganizationHierarchyModel | Mapping | GetOrganizationHierarchyById | Id, Code, Name, Type, Description, ParentId, etc. | P0 |
| FUN-050 | AutoMapper maps to OrganizationHierarchyPrimeDataModel | Prime mapping | GetOrganizationHierarchyPrime | Id, Code, Name, Type, Description, ParentId | P0 |
| FUN-051 | Prime model Expanded root=true | UI default | Root nodes | Expanded=true | P0 |
| FUN-052 | Prime model Expanded child=false | UI default | Child nodes | Expanded=false | P0 |
| FUN-053 | Prime model Type="person" | PrimeNG requirement | All nodes | Type="person" | P0 |
| FUN-054 | Controller maps entity to model for list | Controller mapping | GetOrganizationHierarchies | _mapper.Map<OrganizationHierarchyModel> | P0 |
| FUN-055 | Controller maps entity to model for search | Controller mapping | SearchOrganizationHierarchies | _mapper.Map<OrganizationHierarchyModel> | P0 |
| FUN-056 | Controller maps for GetByIdWithDetails | Controller mapping | GetOrganizationHierarchyByIdWithDetails | _mapper.Map<OrganizationHierarchyModel> | P0 |
| FUN-057 | GetOrganizationHierarchyByIdWithDetails throws BusinessException | Not found | id not found | BusinessException "not found" | P0 |
| FUN-058 | GetOrganizationHierarchyById controller returns NotFound | Not found | id not found | NotFound, "not found" message | P0 |
| FUN-059 | HandleOperationAsync wraps logic | Controller pattern | Any endpoint | Try/catch, handle exception | P0 |
| FUN-060 | AccessControlled on list | Permission | GET api/organizationhierarchy | AccessControlled(OrganizationHierarchy, "read") | P0 |
| FUN-061 | AccessControlled on search | Permission | POST search | AccessControlled(OrganizationHierarchy, "read") | P0 |
| FUN-062 | AccessControlled on GetByIdWithDetails | Permission | GET api/organizationhierarchy/{id} | AccessControlled(OrganizationHierarchy, "read") | P0 |
| FUN-063 | Metadata endpoint calls GetEntityConfigurationDetailsAsync | Metadata | GET metadata-info | EntityConfigurationManager.GetEntityConfigurationDetailsAsync(User, "OrganizationHierarchy") | P0 |
| FUN-064 | Metadata returns entity details | Metadata | Success | Entity and field metadata | P0 |
| FUN-065 | Service uses in-memory filter for pagination | No DB pagination | GetOrganizationHierarchiesAsync | Filter in memory, then Skip/Take | P0 |
| FUN-066 | Service uses in-memory filter for search | No DB search | SearchOrganizationHierarchiesAsync | Filter in memory, then Skip/Take | P0 |
| FUN-067 | Service GetOrganizationHierarchyByIdAsync uses cache | Cache usage | GetOrganizationHierarchyByIdAsync | FirstOrDefault from cached list | P0 |
| FUN-068 | Service PopulateCountsAsync uses count caches | Count caches | PopulateCountsAsync | GetOrCreateAsync for both count caches | P0 |
| FUN-069 | Children count from GroupBy ParentId | Count query | PopulateCountsAsync | GroupBy(oh=>oh.ParentId), Count | P0 |
| FUN-070 | Entity relationship count from GroupBy | Count query | PopulateCountsAsync | GroupBy(our=>our.OrganizationHierarchyId), Count | P0 |
| FUN-071 | OrganizationUnitRelationship links to Partner | EntityType | EntityType="Partner" | Links org unit to Partner | P0 |
| FUN-072 | OrganizationUnitRelationship links to Contact | EntityType | EntityType="Contact" | Links org unit to Contact | P0 |
| FUN-073 | OrganizationUnitRelationship links to Interaction | EntityType | EntityType="Interaction" | Links org unit to Interaction | P0 |
| FUN-074 | DoA2_OrganizationHierarchy role | EntityUserRole | Code=DoA2_OrganizationHierarchy | DoA level 2 holder | P0 |
| FUN-075 | DoA3_OrganizationHierarchy role | EntityUserRole | Code=DoA3_OrganizationHierarchy | DoA level 3 holder | P0 |
| FUN-076 | EntityUserRole EntityType OrganizationHierarchy | EntityType | EntityType="OrganizationHierarchy" | DoA at org unit level | P0 |
| FUN-077 | PaginationRequest inheritance | Filter request | OrganizationHierarchyFilterRequest | Extends PaginationRequest | P0 |
| FUN-078 | Filter request has PageIndex, PageSize | Pagination | FilterRequest | From PaginationRequest | P0 |
| FUN-079 | Search request has PageIndex, PageSize | Pagination | SearchRequest | PageIndex, PageSize | P0 |
| FUN-080 | Search request OrderBy default Name | Default | SearchRequest | OrderBy="Name" | P0 |
| FUN-081 | Search request Ascending default true | Default | SearchRequest | Ascending=true | P0 |
| FUN-082 | ApplyFilters handles null request fields | Null safety | All filter fields null | No filter applied | P0 |
| FUN-083 | ApplySearchFilters handles null request fields | Null safety | All search fields null | No filter applied | P0 |
| FUN-084 | ApplySorting handles null orderBy | Null safety | orderBy=null | Default sort | P0 |
| FUN-085 | BuildChildren recursive | Recursion | Multi-level hierarchy | Recursive BuildChildren | P0 |
| FUN-086 | BuildPrimeChildren recursive | Recursion | Multi-level hierarchy | Recursive BuildPrimeChildren | P0 |
| FUN-087 | OrganizationHierarchy inherits ModifiableDeletableEntity | Base class | Entity | Id, Name, Status, IsDeleted, audit fields | P0 |
| FUN-088 | OrganizationUnitRelationship inherits ModifiableDeletableEntity | Base class | Entity | Id, IsDeleted, audit fields | P0 |
| FUN-089 | OrganizationHierarchy has EntityRelationships collection | Navigation | Entity | ICollection<OrganizationUnitRelationship> | P0 |
| FUN-090 | OrganizationHierarchy has Parent, Children | Navigation | Entity | Parent, Children self-reference | P0 |

---

## §5 Integration Tests (90)

| ID | Test Name | Operation | Entities Involved | Expected Result | Priority |
|----|-----------|----------|-------------------|-----------------|----------|
| INT-001 | Manager→Repository→DB GetOrganizationHierarchy | Full tree | Manager, ValuesRepository, OrganizationHierarchy | Tree from DB | P0 |
| INT-002 | Manager→Repository→DB GetOrganizationHierarchyPrime | Prime tree | Manager, ValuesRepository, OrganizationHierarchy | Prime tree from DB | P0 |
| INT-003 | Manager→Repository→DB GetOrganizationHierarchyById | By ID | Manager, ValuesRepository, OrganizationHierarchy | Single unit from DB | P0 |
| INT-004 | Manager→Repository GetOrganizationsByType | By type | Manager, ValuesRepository, OrganizationHierarchy | Filtered list | P0 |
| INT-005 | Manager→Repository GetAllOrganizations | All | Manager, ValuesRepository, OrganizationHierarchy | Flat list | P0 |
| INT-006 | Controller→Service→DB paginated list | List API | Controller, Service, DbContext, OrganizationHierarchy | 200, paginated response | P0 |
| INT-007 | Controller→Service→DB search | Search API | Controller, Service, DbContext, OrganizationHierarchy | 200, search response | P0 |
| INT-008 | Controller→Service GetByIdWithDetails | By ID API | Controller, Service, DbContext | 200, full model | P0 |
| INT-009 | Controller→Manager GetOrganizationHierarchy | Prime tree API | Controller, Manager, ValuesRepository | 200, Prime tree | P0 |
| INT-010 | Controller→Manager GetOrganizationHierarchyLegacy | Legacy tree API | Controller, Manager, ValuesRepository | 200, legacy tree | P0 |
| INT-011 | Controller→Manager GetOrganizationHierarchyById | By ID alt API | Controller, Manager, ValuesRepository | 200 or 404 | P0 |
| INT-012 | Controller→EntityConfigurationManager GetMetadataInfo | Metadata API | Controller, EntityConfigurationManager | 200, metadata | P0 |
| INT-013 | Service→DbContext→OrganizationHierarchies | Cache load | Service, DbContext, OrganizationHierarchies | Cached list | P0 |
| INT-014 | Service→DbContext→OrganizationUnitRelationships | Entity count | Service, DbContext, OrganizationUnitRelationships | Count per org unit | P0 |
| INT-015 | Service→MemoryCache GetOrCreateAsync | Cache read | Service, IMemoryCache | Cached or fresh load | P0 |
| INT-016 | ValuesRepository→DbContext OrganizationHierarchies | Tree query | ValuesRepository, DbContext | allUnits list | P0 |
| INT-017 | ValuesRepository BuildChildren→allUnits | Tree build | ValuesRepository, in-memory | Nested Children | P0 |
| INT-018 | ValuesRepository BuildPrimeChildren→allUnits | Prime build | ValuesRepository, in-memory | Nested Children | P0 |
| INT-019 | AutoMapper OrganizationHierarchy→Model | Mapping | AutoMapper, OrganizationHierarchy, OrganizationHierarchyModel | Mapped model | P0 |
| INT-020 | AutoMapper OrganizationHierarchy→PrimeDataModel | Prime mapping | AutoMapper, OrganizationHierarchy, OrganizationHierarchyPrimeDataModel | Mapped data | P0 |
| INT-021 | OrganizationHierarchy→OrganizationUnitRelationship | Entity link | OrganizationHierarchy, OrganizationUnitRelationship | FK relationship | P0 |
| INT-022 | OrganizationUnitRelationship→Partner | Entity link | OrganizationUnitRelationship, Partner | EntityId, EntityType=Partner | P0 |
| INT-023 | OrganizationUnitRelationship→Contact | Entity link | OrganizationUnitRelationship, Contact | EntityId, EntityType=Contact | P0 |
| INT-024 | OrganizationUnitRelationship→Interaction | Entity link | OrganizationUnitRelationship, Interaction | EntityId, EntityType=Interaction | P0 |
| INT-025 | EntityUserRole→OrganizationHierarchy DoA | DoA link | EntityUserRole, OrganizationHierarchy | EntityType=OrganizationHierarchy, Code=DoA2/DoA3 | P0 |
| INT-026 | WorkflowController uses OrganizationHierarchy for DoA | Workflow | WorkflowController, EntityUserRole, OrganizationHierarchy | DoA resolution by org unit | P0 |
| INT-027 | Opportunity ResponsibleOrgUnitId→OrganizationHierarchy | Opportunity link | Opportunity, OrganizationHierarchy | ResponsibleOrgUnitId FK | P0 |
| INT-028 | ValuesController OrganizationUnits | Lookup | ValuesController, OrganizationHierarchy | GET OrganizationUnits | P0 |
| INT-029 | ValuesController OpportunityOrganizationUnits | Lookup | ValuesController, OrganizationHierarchy | Filtered org units for Opportunity | P0 |
| INT-030 | UserResolverService in controller | Auth | Controller, UserResolverService | Current user for auth | P0 |
| INT-031 | AuthorizationService in controller | Auth | Controller, IAuthorizationService | Permission check | P0 |
| INT-032 | IAP authentication scheme | Auth | Controller, [Authorize(IAP)] | IAP scheme | P0 |
| INT-033 | AccessControlled attribute evaluation | Permission | AccessControlled, EntityTypes.OrganizationHierarchy | read permission | P0 |
| INT-034 | PaginationResponse structure | Response | PaginationResponse<OrganizationHierarchyModel> | Records, TotalCount, PageIndex, PageSize, TotalPages | P0 |
| INT-035 | OrganizationHierarchyFilterRequest binding | Request | FromQuery, FilterRequest | Query params bound | P0 |
| INT-036 | OrganizationHierarchySearchRequest binding | Request | FromBody, SearchRequest | JSON body bound | P0 |
| INT-037 | BaseController HandleOperationAsync | Base | OrganizationHierarchyController, BaseController | Exception handling | P0 |
| INT-038 | UNOPSManagerWrapper EntityConfigurationManager | UNOPS | Controller, UNOPSManagerWrapper | Entity config from UNOPS override | P0 |
| INT-039 | IOrganizationHierarchyManager injection | DI | Controller, Manager | Manager injected | P0 |
| INT-040 | OrganizationHierarchyService injection | DI | Controller, Service | Service injected | P0 |
| INT-041 | ValuesRepository injection in Manager | DI | OrganizationHierarchyManager, ValuesRepository | Repository injected | P0 |
| INT-042 | IMapper injection in Manager | DI | OrganizationHierarchyManager, IMapper | Mapper injected | P0 |
| INT-043 | AppDbContext injection in Service | DI | OrganizationHierarchyService, AppDbContext | DbContext injected | P0 |
| INT-044 | IMemoryCache injection in Service | DI | OrganizationHierarchyService, IMemoryCache | Cache injected | P0 |
| INT-045 | DbContext OrganizationHierarchies DbSet | DbSet | AppDbContext, OrganizationHierarchies | DbSet access | P0 |
| INT-046 | DbContext OrganizationUnitRelationships DbSet | DbSet | AppDbContext, OrganizationUnitRelationships | DbSet access | P0 |
| INT-047 | Include Parent in cache query | Eager load | Service cache query | Include(oh=>oh.Parent) | P0 |
| INT-048 | GetOrganizationHierarchyById Include Children | Eager load | ValuesRepository | Include(x=>x.Children).ThenInclude(child=>child.Children) | P0 |
| INT-049 | Pagination Skip/Take in Service | Pagination | Service GetOrganizationHierarchiesAsync | Skip((PageIndex-1)*PageSize).Take(PageSize) | P0 |
| INT-050 | Pagination Skip/Take in Search | Pagination | Service SearchOrganizationHierarchiesAsync | Same pattern | P0 |
| INT-051 | Filter before pagination | Order | Service | ApplyFilters→ApplySorting→Count→Skip/Take | P0 |
| INT-052 | PopulateCounts after pagination | Order | Service | Paginate then PopulateCounts on paged list | P0 |
| INT-053 | Search always PopulateCounts | Search | Service SearchOrganizationHierarchiesAsync | PopulateCountsAsync called | P0 |
| INT-054 | List PopulateCounts when IncludeCounts | List | Service GetOrganizationHierarchiesAsync | PopulateCounts only if IncludeCounts | P0 |
| INT-055 | GetByIdWithDetails PopulateCounts | By ID | Service GetOrganizationHierarchyByIdAsync | PopulateCounts on single item | P0 |
| INT-056 | Children count excludes deleted children | Soft delete | Service, OrganizationHierarchies | Where(!oh.IsDeleted) in count query | P0 |
| INT-057 | Entity relationship count (no IsDeleted filter) | Count | Service, OrganizationUnitRelationships | GroupBy; check if IsDeleted filtered | P0 |
| INT-058 | APIDictionary.OrganizationHierarchy route | Route | APIDictionary | "api/organization-hierarchy" | P0 |
| INT-059 | api/organizationhierarchy (no hyphen) list | Route | Controller | Different route for list | P0 |
| INT-060 | api/organizationhierarchy/search | Route | Controller | POST search route | P0 |
| INT-061 | api/organizationhierarchy/{id} service path | Route | Controller | Service GetById path | P0 |
| INT-062 | api/organization-hierarchy manager path | Route | Controller | Manager Prime tree path | P0 |
| INT-063 | api/organization-hierarchy/legacy | Route | Controller | Manager legacy tree path | P0 |
| INT-064 | api/organization-hierarchy/{id} manager path | Route | Controller | Manager GetById path | P0 |
| INT-065 | api/organization-hierarchy/metadata-info | Route | Controller | Metadata path | P0 |
| INT-066 | EntityTypes.OrganizationHierarchy constant | Entity type | EntityTypes | "OrganizationHierarchy" | P0 |
| INT-067 | OrganizationUnitType enum in model | Enum | OrganizationHierarchyDataModel, PrimeDataModel | Type property | P0 |
| INT-068 | OrganizationHierarchyModel Type as string | Model | OrganizationHierarchyModel | Type string (from enum) | P0 |
| INT-069 | OrganizationHierarchyTreeModel structure | Model | TreeModel | Data (OrganizationHierarchyDataModel) | P0 |
| INT-070 | OrganizationHierarchyPrimeModel structure | Model | PrimeModel | Expanded, Type, Data, Children | P0 |
| INT-071 | OrganizationHierarchyDataModel Children | Model | DataModel | List<OrganizationHierarchyDataModel> | P0 |
| INT-072 | OrganizationHierarchyPrimeModel Children | Model | PrimeModel | List<OrganizationHierarchyPrimeModel> | P0 |
| INT-073 | ModifiableDeletableEntity base | Entity | OrganizationHierarchy | Id, Name, Status, IsDeleted, audit | P0 |
| INT-074 | OrganizationHierarchy required Code, Name, Description | Entity | OrganizationHierarchy | required properties | P0 |
| INT-075 | OrganizationHierarchy ParentId nullable | Entity | OrganizationHierarchy | int? ParentId | P0 |
| INT-076 | OrganizationHierarchy IsSelfManagementEnabled default false | Entity | OrganizationHierarchy | Default false | P0 |
| INT-077 | OrganizationUnitRelationship OrganizationHierarchyId | Entity | OrganizationUnitRelationship | FK to OrganizationHierarchy | P0 |
| INT-078 | OrganizationUnitRelationship EntityId, EntityType | Entity | OrganizationUnitRelationship | Generic entity link | P0 |
| INT-079 | SuggestedOrgUnits endpoint uses org hierarchy | Integration | ValuesController | SuggestedOrgUnits | P0 |
| INT-080 | EntityUserRolesByOrgUnit endpoint | Integration | ValuesController | POST EntityUserRolesByOrgUnit | P0 |
| INT-081 | OrgUnitIdsForCountries endpoint | Integration | ValuesController | POST OrgUnitIdsForCountries | P0 |
| INT-082 | ChildOrgUnitIdsForHubRegion endpoint | Integration | ValuesController | POST ChildOrgUnitIdsForHubRegion | P0 |
| INT-083 | DashboardOrgUnitRecentUpdates | Integration | DashboardController | Org unit recent updates | P0 |
| INT-084 | UserProfile OrgUnit string | Integration | UserProfile, OrganizationHierarchy | OrgUnit name/code | P0 |
| INT-085 | GlobalSearch includes OrganizationHierarchy | Integration | GlobalController | Search org units | P0 |
| INT-086 | Opportunity ResponsibleOrgUnitId dropdown | Integration | Opportunity form, OrganizationHierarchy | Org unit selection | P0 |
| INT-087 | Contact org unit assignment | Integration | Contact, OrganizationHierarchy | Org unit link | P0 |
| INT-088 | Partner org unit assignment | Integration | Partner, OrganizationHierarchy | Org unit link | P0 |
| INT-089 | Interaction org unit assignment | Integration | Interaction, OrganizationHierarchy | Org unit link | P0 |
| INT-090 | Audit trail for OrganizationHierarchy | Integration | AuditableDbContext, OrganizationHierarchy | Created/Modified audit | P0 |

---

## §6 Security Tests (50)

| ID | Test Name | Attack Vector | Target | Expected Block | Priority |
|----|-----------|--------------|--------|----------------|----------|
| SEC-001 | Unauthenticated list | No token | GET api/organizationhierarchy | 401 | P0 |
| SEC-002 | Unauthenticated search | No token | POST search | 401 | P0 |
| SEC-003 | Unauthenticated GetById | No token | GET api/organizationhierarchy/5 | 401 | P0 |
| SEC-004 | Unauthenticated Prime tree | No token | GET api/organization-hierarchy | 401 | P0 |
| SEC-005 | Unauthenticated legacy tree | No token | GET api/organization-hierarchy/legacy | 401 | P0 |
| SEC-006 | Unauthenticated metadata | No token | GET metadata-info | 401 | P0 |
| SEC-007 | Expired token list | Expired JWT | GET list | 401 | P0 |
| SEC-008 | Expired token search | Expired JWT | POST search | 401 | P0 |
| SEC-009 | Invalid token list | Malformed JWT | GET list | 401 | P0 |
| SEC-010 | Invalid token search | Malformed JWT | POST search | 401 | P0 |
| SEC-011 | User without read permission list | No OrganizationHierarchy read | GET list | 403 | P0 |
| SEC-012 | User without read permission search | No OrganizationHierarchy read | POST search | 403 | P0 |
| SEC-013 | User without read permission GetById | No read | GET api/organizationhierarchy/5 | 403 | P0 |
| SEC-014 | User without read permission Prime | No read | GET api/organization-hierarchy | 403 | P0 |
| SEC-015 | User without read permission metadata | No read | GET metadata-info | 403 | P0 |
| SEC-016 | SQL injection in Name filter | Name='; DROP TABLE-- | GET list | No injection, safe filter | P0 |
| SEC-017 | SQL injection in Code filter | Code=' OR 1=1-- | GET list | No injection | P0 |
| SEC-018 | SQL injection in SearchTerm | SearchTerm='; DELETE-- | POST search | No injection | P0 |
| SEC-019 | XSS in Name filter | Name=<script>alert(1)</script> | GET list | Encoded/sanitized | P0 |
| SEC-020 | XSS in SearchTerm | SearchTerm=<img src=x onerror=alert(1)> | POST search | Encoded/sanitized | P0 |
| SEC-021 | IDOR GetById other user's scope | id=999 (if scoped) | GET api/organizationhierarchy/999 | 403 or 404 | P0 |
| SEC-022 | Mass assignment FilterRequest | Extra fields in query | GET list | Only expected fields bound | P0 |
| SEC-023 | Mass assignment SearchRequest | Extra fields in body | POST search | Only expected fields bound | P0 |
| SEC-024 | HTTP method override | X-HTTP-Method-Override | POST to GET endpoint | Rejected | P0 |
| SEC-025 | CSRF on search | Cross-site POST | POST search | CSRF token or SameSite cookie | P0 |
| SEC-026 | Token from wrong issuer | Wrong issuer JWT | GET list | 401 | P1 |
| SEC-027 | Token with wrong audience | Wrong audience JWT | GET list | 401 | P1 |
| SEC-028 | Role escalation via filter | Manipulated filter | GET list | No privilege escalation | P1 |
| SEC-029 | Path traversal in id | id=../../../etc/passwd | GET api/organizationhierarchy/{id} | 400 or 404 | P1 |
| SEC-030 | Integer overflow in id | id=2147483648 (if long) | GET | 400 or 404 | P1 |
| SEC-031 | Replay attack | Old valid token | GET list | Token expiry prevents | P1 |
| SEC-032 | Session fixation | Fixated session | GET list | New session | P1 |
| SEC-033 | LDAP injection in SearchTerm | SearchTerm=*)(uid=* | POST search | No injection | P1 |
| SEC-034 | NoSQL injection (if applicable) | N/A | N/A | N/A | P1 |
| SEC-035 | Parameter pollution PageIndex | PageIndex=1&PageIndex=2 | GET list | Last or first wins | P1 |
| SEC-036 | Parameter pollution PageSize | PageSize=10&PageSize=99999 | GET list | Validation | P1 |
| SEC-037 | Oversized request body search | 10MB JSON | POST search | 413 or reject | P1 |
| SEC-038 | Deeply nested JSON search | Nested 100 levels | POST search | Reject or limit | P1 |
| SEC-039 | Authorization handler for OrganizationHierarchy | AccessControlled | Read operation | Handler invoked | P1 |
| SEC-040 | EntityConfigurationManager permission check | Metadata | GetEntityConfigurationDetailsAsync | User permission checked | P1 |
| SEC-041 | IAP scheme validation | Auth | All endpoints | IAP validation | P1 |
| SEC-042 | UserResolverService extracts user | Auth | Controller | Valid user ID | P1 |
| SEC-043 | Logging does not expose secrets | Error log | 500 response | No tokens/secrets in log | P1 |
| SEC-044 | BusinessException message not sensitive | BusinessException | Not found | No internal details | P1 |
| SEC-045 | Cache key not predictable by user | Cache | CACHE_KEY | Same key for all users | P1 |
| SEC-046 | Cache isolation per tenant (if multi-tenant) | Cache | Multi-tenant | Tenant isolation | P1 |
| SEC-047 | OrganizationUnitRelationship EntityType validation | EntityType | Invalid EntityType | Reject or ignore | P1 |
| SEC-048 | DoA role scope | EntityUserRole | DoA2/DoA3 | User can only see own scope | P1 |
| SEC-049 | Metadata info does not expose config secrets | Metadata | Entity config | No connection strings, etc. | P1 |
| SEC-050 | Rate limiting on list (if implemented) | Many requests | GET list | 429 or throttle | P1 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Concurrent Scenario | Expected Behavior | Priority |
|----|-----------|---------------------|-------------------|----------|
| CON-001 | Cache GetOrCreateAsync concurrent | 10 threads call GetAllOrganizationHierarchiesAsync | Single cache population, others wait | P0 |
| CON-002 | Children count cache concurrent | 10 threads trigger PopulateCountsAsync | Single count cache load | P0 |
| CON-003 | Entity relationship cache concurrent | 10 threads trigger PopulateCountsAsync | Single relationship cache load | P0 |
| CON-004 | Concurrent list requests | 20 GET list simultaneous | All 200, consistent data | P0 |
| CON-005 | Concurrent search requests | 20 POST search simultaneous | All 200, consistent data | P0 |
| CON-006 | Concurrent GetById requests | 20 GET by same id | All 200, same data | P0 |
| CON-007 | Concurrent Prime tree requests | 20 GET Prime tree | All 200, same structure | P0 |
| CON-008 | Cache refresh during read | Cache expires mid-request | No exception, fresh or stale | P0 |
| CON-009 | Count cache refresh during read | Count cache expires mid-request | No exception | P0 |
| CON-010 | DbContext concurrent read | Multiple Service instances | Each has own DbContext, no conflict | P0 |
| CON-011 | MemoryCache thread safety | Concurrent GetOrCreateAsync | MemoryCache handles locking | P0 |
| CON-012 | Pagination concurrent same page | 10 GET list PageIndex=1 | Same results | P1 |
| CON-013 | Pagination concurrent different pages | 10 GET list different PageIndex | Correct pages | P1 |
| CON-014 | Search concurrent same criteria | 10 POST search same body | Same results | P1 |
| CON-015 | Search concurrent different criteria | 10 POST search different bodies | Correct results each | P1 |
| CON-016 | Filter concurrent | 10 GET list different filters | Correct filtered results | P1 |
| CON-017 | Sort concurrent | 10 GET list different OrderBy | Correct sorted results | P1 |
| CON-018 | Manager GetOrganizationHierarchyPrime concurrent | 10 calls | All return same tree | P1 |
| CON-019 | Manager GetOrganizationHierarchy concurrent | 10 calls | All return same tree | P1 |
| CON-020 | Manager GetOrganizationHierarchyById concurrent | 10 calls same id | All return same model | P1 |
| CON-021 | ValuesRepository concurrent | Multiple Manager calls | Repository stateless, safe | P1 |
| CON-022 | AutoMapper concurrent | Multiple Map calls | AutoMapper thread-safe | P1 |
| CON-023 | Cache eviction and repopulation | Manual evict, then request | Fresh load | P1 |
| CON-024 | High concurrency list | 100 simultaneous GET list | No deadlock, all complete | P1 |
| CON-025 | High concurrency mixed | 50 list, 50 search, 50 GetById | All succeed | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | ApplyFilters Name | Filter | Name="Europe" | Filtered where Name contains "Europe" | P0 |
| UNT-002 | ApplyFilters Code | Filter | Code="EU" | Filtered where Code contains "EU" | P0 |
| UNT-003 | ApplyFilters Type | Filter | Type="Office" | Filtered where Type="Office" | P0 |
| UNT-004 | ApplyFilters ParentId | Filter | ParentId=5 | Filtered where ParentId=5 | P0 |
| UNT-005 | ApplyFilters ParentCode | Filter | ParentCode="HQ" | Filtered where ParentCode contains "HQ" | P0 |
| UNT-006 | ApplyFilters Status | Filter | Status="Active" | Filtered where Status="Active" | P0 |
| UNT-007 | ApplyFilters IsSelfManagementEnabled | Filter | IsSelfManagementEnabled=true | Filtered where true | P0 |
| UNT-008 | ApplySearchFilters SearchTerm | Filter | SearchTerm="HQ" | Filtered where Name/Code/Description contains "HQ" | P0 |
| UNT-009 | ApplySearchFilters Type | Filter | Type="Region" | Filtered where Type="Region" | P0 |
| UNT-010 | ApplySearchFilters ParentId | Filter | ParentId=10 | Filtered where ParentId=10 | P0 |
| UNT-011 | ApplySorting Name ascending | Sort | orderBy="name", asc=true | OrderBy(oh=>oh.Name) | P0 |
| UNT-012 | ApplySorting Name descending | Sort | orderBy="name", asc=false | OrderByDescending(oh=>oh.Name) | P0 |
| UNT-013 | ApplySorting ChildrenCount | Sort | orderBy="childrencount" | OrderBy(oh=>oh.ChildrenCount) | P0 |
| UNT-014 | ApplySorting default | Sort | orderBy=null | OrderBy(oh=>oh.Name) | P0 |
| UNT-015 | BuildChildren empty | Tree | parentId with no children | Empty list | P0 |
| UNT-016 | BuildChildren one child | Tree | parentId with 1 child | List of 1 with nested Children | P0 |
| UNT-017 | BuildPrimeChildren empty | Tree | parentId with no children | Empty list | P0 |
| UNT-018 | BuildPrimeChildren one child | Tree | parentId with 1 child | List of 1, Expanded=false | P0 |
| UNT-019 | GetOrganizationsByType empty | Type | Type=Hub, no hubs | Empty enumerable | P0 |
| UNT-020 | GetOrganizationHierarchyById null | By ID | id=999999 | null | P0 |
| UNT-021 | Pagination TotalPages calculation | Pagination | TotalCount=25, PageSize=10 | TotalPages=3 | P0 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetOrganizationHierarchyPrime cold cache | First call | < 2s | P0 |
| PRF-002 | GetOrganizationHierarchyPrime warm cache | Second call | < 200ms | P0 |
| PRF-003 | GetOrganizationHierarchy legacy cold | First call | < 2s | P0 |
| PRF-004 | Paginated list cold cache | First call | < 2s | P0 |
| PRF-005 | Paginated list warm cache | Second call | < 500ms | P0 |
| PRF-006 | Search cold cache | First call | < 2s | P0 |
| PRF-007 | Search warm cache | Second call | < 500ms | P0 |
| PRF-008 | GetById warm cache | Second call | < 100ms | P0 |
| PRF-009 | Paginated list PageSize=100 | 100 records | < 1s | P1 |
| PRF-010 | Search with IncludeCounts | Full search | < 1s | P1 |
| PRF-011 | Tree build 100 nodes | 100 org units | < 500ms | P1 |
| PRF-012 | Tree build 500 nodes | 500 org units | < 2s | P1 |
| PRF-013 | PopulateCountsAsync 50 items | 50 hierarchies | < 500ms | P1 |
| PRF-014 | Filter + sort + paginate | Full pipeline | < 1s | P1 |
| PRF-015 | Cache expiration and reload | After 30 min | < 2s for reload | P1 |
| PRF-016 | Metadata endpoint | GetMetadataInfo | < 500ms | P1 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-----------------|----------|
| LDT-001 | List endpoint sustained load | 20 req/s | 5 min | 95% < 500ms, 0% error | P0 |
| LDT-002 | Search endpoint sustained load | 20 req/s | 5 min | 95% < 500ms, 0% error | P0 |
| LDT-003 | GetById endpoint sustained load | 30 req/s | 5 min | 95% < 200ms, 0% error | P0 |
| LDT-004 | Prime tree endpoint sustained load | 10 req/s | 5 min | 95% < 500ms, 0% error | P0 |
| LDT-005 | Mixed workload | 10 list, 10 search, 20 GetById/s | 5 min | 95% < 500ms | P0 |
| LDT-006 | Spike test list | 0→100 req/s in 10s | 2 min | No 503, recover | P0 |
| LDT-007 | Spike test search | 0→50 req/s in 10s | 2 min | No 503, recover | P0 |
| LDT-008 | Endurance list | 10 req/s | 30 min | No memory leak, stable latency | P0 |
| LDT-009 | Cache warm-up then load | Warm cache, 50 req/s list | 5 min | 95% < 300ms | P0 |
| LDT-010 | Concurrent Prime tree | 20 concurrent Prime tree | 1 min | All complete, no timeout | P0 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
