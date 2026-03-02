# PartnerTreeManager Business Logic — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/PartnerTreeManager`  
**Created:** 2026-02-18  
**Last Updated:** 2026-02-18  
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
| Security Tests | §6 | 50 | OUT OF SCOPE | — |
| Concurrency Tests | §7 | 25 | ≥25 | ✅ |
| Unit Tests | §8 | 21 | ≥21 | ✅ |
| Performance Tests | §9 | 16 | ≥16 | ✅ |
| Load Tests | §10 | 10 | ≥10 | ✅ |
| **TOTAL** | | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P (90≥90) ✅ | E≥3P (90≥90) ✅ | F≥3P (90≥90) ✅ | I≥3P (90≥90) ✅

---

## Feature Overview

PartnerTreeManager manages partner category and group hierarchy using **code-based parent matching** (ParentCode links child to parent, NOT FK-based). Key business rules: **Tree hierarchy** (Categories contain Groups, Groups contain sub-Groups; only 2 levels of nesting), **CRUD for tree nodes** (name, code, description, parentCode), **recursive descendant retrieval** (GetDescendantsRecursive includes nested children), **partner assignments to groups** (partners can belong to multiple groups), **cascading implications of category/group deletion** (soft-delete), **sorting** (display order within parent, alphabetical within same level), **AI integration** (GetTreeForAI returns formatted hierarchy for prompts), **categorization overview** (aggregate counts per category), **code uniqueness** (codes must be unique within the tree), **IsDeleted filtering** in all queries, **name validation** (required, non-empty), **tree rendering** for PrimeNG TreeNode format.

---

## §1 Positive Tests (Happy Path) — 30 tests

| ID | Test Name | Precondition | Steps (Brief) | Expected Result | Priority |
|----|-----------|-------------|---------------|-----------------|----------|
| POS-001 | Create category with valid data | User has create permission | CreatePartnerTreeAsync(model: Code="GOV", Name="Government", Type="Level_1", Parent="") | Category created with Id, Name, Code, IsDeleted=false | P0 |
| POS-002 | Create group under category | Category "GOV" exists | CreatePartnerTreeAsync(model: Code="GOV-001", Name="Ministries", Type="Level_2", Parent="GOV") | Group created under GOV, ParentCode="GOV" | P0 |
| POS-003 | Get partner trees hierarchical | Trees exist | GetPartnerTreesAsync(user, "Name", true) | Hierarchical tree returned, root categories first | P0 |
| POS-004 | Get partner tree by ID | Tree with Id=5 exists | GetPartnerTreeAsync(user, 5) | PartnerTreeModel with Data.Code, Data.Name, Children | P0 |
| POS-005 | Update partner tree name | Tree Id=5 exists | UpdatePartnerTreeAsync(user, model with Id=5, Name="Updated Name") | Name updated, LastModifiedBy/Date set | P0 |
| POS-006 | Soft delete partner tree | Tree exists, no partners assigned | DeletePartnerTreeAsync(user, 5) | IsDeleted=true, DeletedBy/DeletedDate set | P0 |
| POS-007 | Get descendants recursive | Category "GOV" has groups GOV-001, GOV-002 | GetDescendantsRecursive("GOV") | List includes GOV-001, GOV-002 and nested children | P0 |
| POS-008 | Get tree for AI | Trees exist | GetTreeForAI() | Formatted hierarchy string for prompts (e.g., "GOV > GOV-001") | P0 |
| POS-009 | Get categorization overview | Categories and groups exist | GetCategorizationOverviewAsync(user) | Aggregate counts per category, totalGroups, totalCategories | P0 |
| POS-010 | Sort by Name ascending | Trees exist | GetPartnerTreesAsync(user, "Name", true) | Sorted A→Z within each level | P1 |
| POS-011 | Sort by Code ascending | Trees exist | GetPartnerTreesAsync(user, "Code", true) | Sorted by Code | P1 |
| POS-012 | Get partners by group ID | Group Id=5 has 3 partners | GetPartnersByPartnerGroup(userId, 5, request) | Paginated partners, 3 total | P1 |
| POS-013 | Get partners by category code | Category "GOV" has groups with partners | GetPartnersByPartnerCategoryCode(userId, "GOV", request) | All partners in GOV and descendant groups | P1 |
| POS-014 | Build hierarchy with empty parent | Trees with Parent="" | BuildHierarchy(lookup, "") | Top-level categories returned | P1 |
| POS-015 | Code uniqueness on create | Code "NGO" does not exist | CreatePartnerTreeAsync(model with Code="NGO") | Created successfully | P1 |
| POS-016 | Name required validation passes | Valid name | CreatePartnerTreeAsync(model with Name="NGOs") | Created | P1 |
| POS-017 | ParentCode links child to parent | Parent "GOV" exists | Create with Parent="GOV" | Child appears under GOV in hierarchy | P1 |
| POS-018 | IsDeleted excluded from GetPartnerTrees | Tree Id=5 IsDeleted=true | GetPartnerTreesAsync | Tree 5 not in result | P1 |
| POS-019 | Get posted partner trees | Trees exist | GetPostedPartnerTrees() | ExternalPartnerTreeModel list, non-deleted only | P1 |
| POS-020 | Get category and group structure | Trees exist | GetCategoryAndGroupStructureAsync(user) | Categories with Children groups | P1 |
| POS-021 | Description optional | Create with Description=null | CreatePartnerTreeAsync | Created, Description null | P2 |
| POS-022 | Display order within parent | Siblings have SortOrder | GetPartnerTreesAsync | Siblings in SortOrder sequence | P2 |
| POS-023 | Alphabetical within same level | Same parent, no SortOrder | GetPartnerTreesAsync | Alphabetical by Name | P2 |
| POS-024 | Partner assignment to group | Partner exists | Assign partner to PartnerGroupId=5 | Partner.PartnerGroupId=5 | P2 |
| POS-025 | Partner in multiple groups | Partner can have multiple group refs | GetPartnersByPartnerGroup for each group | Partner appears in both | P2 |
| POS-026 | PrimeNG TreeNode format | Trees exist | Convert to TreeNode | label, data, children structure | P2 |
| POS-027 | Get basic category details for AI | Category Id=5 exists | GetBasicPartnerCategoryDetailsAsync(5) | Name, Code, partner count | P2 |
| POS-028 | Get basic group details for AI | Group Id=7 exists | GetBasicPartnerGroupDetailsAsync(7) | Name, Code, parent category | P2 |
| POS-029 | Categories summary | Categories exist | GetCategoriesSummary() | totalCategories, categories with counts | P2 |
| POS-030 | Groups summary | Groups exist | GetGroupsSummary() | totalGroups, groups with counts | P2 |

---

## §2 Negative Tests — 90 tests

### 2.1 Invalid Input (20)
| ID | Invalid Input | Expected | Priority |
|----|--------------|----------|----------|
| NEG-001 | Duplicate Code | Create with Code="GOV" (exists) | BusinessException: code exists | P0 |
| NEG-002 | Null model | CreatePartnerTreeAsync(null, null) | ArgumentNullException | P0 |
| NEG-003 | Empty Code | Code="" | BusinessException: required | P0 |
| NEG-004 | Null Name | Name=null | BusinessException: required | P0 |
| NEG-005 | Whitespace Name | Name="   " | BusinessException | P1 |
| NEG-006 | Get by non-existent ID | GetPartnerTreeAsync(user, 99999) | null | P0 |
| NEG-007 | Get by ID zero | GetPartnerTreeAsync(user, 0) | null or error | P1 |
| NEG-008 | Get by ID negative | GetPartnerTreeAsync(user, -1) | null or error | P1 |
| NEG-009 | Update non-existent | UpdatePartnerTreeAsync(model.Id=99999) | BusinessException | P0 |
| NEG-010 | Update with null model | model=null | ArgumentNullException | P0 |
| NEG-011 | Delete non-existent | DeletePartnerTreeAsync(user, 99999) | Graceful (no-op or error) | P1 |
| NEG-012 | Delete already deleted | Tree IsDeleted=true | Graceful | P1 |
| NEG-013 | Parent references non-existent | Create with Parent="NONEXISTENT" | Orphan or BusinessException | P1 |
| NEG-014 | Circular parent reference | Parent=A, A.Parent=B, B.Parent=A | Infinite loop prevented, visitedCodes | P0 |
| NEG-015 | Invalid sortBy | GetPartnerTreesAsync(user, "InvalidField", true) | Fallback to "Name" or error | P1 |
| NEG-016 | Null ClaimsPrincipal | CreatePartnerTreeAsync(null, model) | ArgumentNullException | P1 |
| NEG-017 | Type mismatch | Type="Invalid" | BusinessException | P1 |
| NEG-018 | GetDescendantsRecursive non-existent code | GetAllDescendantsAsync("NONEXISTENT") | Empty list | P1 |
| NEG-019 | GetBasicPartnerCategoryDetailsAsync non-existent | entityId=99999 | Error object or null | P1 |
| NEG-020 | GetBasicPartnerGroupDetailsAsync non-existent | entityId=99999 | Error object or null | P1 |

### 2.2 Unauthorized Access (15)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-021 | No auth token | CreatePartnerTreeAsync | 401 Unauthorized | P0 |
| NEG-022 | No create permission | CreatePartnerTreeAsync | 403 Forbidden | P0 |
| NEG-023 | No read permission | GetPartnerTreesAsync | 403 Forbidden | P0 |
| NEG-024 | No update permission | UpdatePartnerTreeAsync | 403 Forbidden | P0 |
| NEG-025 | No delete permission | DeletePartnerTreeAsync | 403 Forbidden | P0 |
| NEG-026 | Expired JWT | Any operation | 401 | P0 |
| NEG-027 | Tampered JWT | Any operation | 401 | P0 |
| NEG-028 | Scoped user out of scope | GetPartnerTreeAsync for other OrgUnit | 403 or filtered | P0 |
| NEG-029 | Disabled account | Any operation | 401/403 | P1 |
| NEG-030 | Post-logout | Any operation | 401 | P1 |
| NEG-031 | IDOR GetPartnerTree | User A requests User B's tree | 403 or filtered | P0 |
| NEG-032 | IDOR Update | User A updates User B's tree | 403 | P0 |
| NEG-033 | IDOR Delete | User A deletes User B's tree | 403 | P0 |
| NEG-034 | Role escalation attempt | Include admin role in token | Ignored | P0 |
| NEG-035 | Anonymous GetPostedPartnerTrees | No auth | 401 or public endpoint | P1 |

### 2.3 Hierarchy & State (20)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-036 | Create Level_2 without parent | Parent="" for group | BusinessException or orphan at root | P1 |
| NEG-037 | More than 2 levels nesting | Category > Group > SubGroup > SubSubGroup | Rejected or flattened | P1 |
| NEG-038 | Update deleted tree | Tree IsDeleted=true | BusinessException | P1 |
| NEG-039 | Delete category with partners | Category has assigned partners | BusinessException or cascade soft-delete | P0 |
| NEG-040 | Delete group with partners | Group has assigned partners | BusinessException or cascade | P0 |
| NEG-041 | Get children of deleted parent | Parent IsDeleted=true | Excluded from hierarchy | P1 |
| NEG-042 | ParentCode points to deleted | Parent code exists but IsDeleted | Orphan or error | P1 |
| NEG-043 | BuildHierarchy circular | A→B→A | visitedCodes prevents infinite loop | P0 |
| NEG-044 | GetDescendantsRecursive circular data | Corrupt data | Bounded recursion | P1 |
| NEG-045 | Null parentCode in lookup | Parent=null | Normalized to empty string | P1 |
| NEG-046 | Empty parentCode | Parent="" | Root-level items | P1 |
| NEG-047 | Whitespace parentCode | Parent="   " | Normalized to empty | P1 |
| NEG-048 | Category as child of Group | Invalid hierarchy | BusinessException | P0 |
| NEG-049 | Group as root (no category) | Parent="" for Level_2 | Rejected or allowed per rule | P1 |
| NEG-050 | Update non-editable category code | CanModifyPartnerCategoryCode=false | PartnerCategoryCode not updated | P1 |
| NEG-051 | Update non-editable group code | CanModifyPartnerGroupCode=false | PartnerGroupCode not updated | P1 |
| NEG-052 | Delete with active children | Category has non-deleted groups | BusinessException or cascade | P0 |
| NEG-053 | Move group to deleted category | New parent IsDeleted | BusinessException | P1 |
| NEG-054 | Self-reference Parent | Parent=own Code | BusinessException | P0 |
| NEG-055 | Code with SQL chars | Code="'; DROP TABLE--" | Parameterized, no injection | P0 |

### 2.4 Injection & Sanitization (10)
| ID | Attack | Expected | Priority |
|----|--------|----------|----------|
| NEG-056 | SQL injection in Name | Name="'; DROP TABLE--" | Parameterized | P0 |
| NEG-057 | SQL injection in Code | Code="1; DELETE FROM PartnerTree" | Parameterized | P0 |
| NEG-058 | SQL injection in sortBy | sortBy="Name; DROP TABLE" | Sanitized/fallback | P0 |
| NEG-059 | XSS in Name | Name="<script>alert(1)</script>" | Sanitized | P0 |
| NEG-060 | XSS in Description | Description with script | Sanitized | P0 |
| NEG-061 | Path traversal in Code | Code="../../../etc/passwd" | Rejected | P0 |
| NEG-062 | HTML injection in Name | Name="<b>Bold</b>" | Escaped | P1 |
| NEG-063 | LDAP injection in search | Search with LDAP chars | Escaped | P1 |
| NEG-064 | Control chars in Code | Code="\0\x01" | Rejected | P1 |
| NEG-065 | Unicode homograph in Code | Code="GΟV" (Greek O) | Validated | P2 |

### 2.5 Additional (25)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| NEG-066 | Mass assignment Id on create | Include Id in POST | Ignored | P0 |
| NEG-067 | Mass assignment CreatedBy | Include in request | Ignored | P0 |
| NEG-068 | Mass assignment IsDeleted | Include in request | Ignored | P0 |
| NEG-069 | API by-partner-group-id invalid | id=99999 | 400 or empty | P1 |
| NEG-070 | API by-partner-category-code empty | code="" | 400 or empty | P1 |
| NEG-071 | API by-partner-category-code non-existent | code="NONEXISTENT" | Empty list | P1 |
| NEG-072 | GetPartnerTreeByCodeAsync non-existent | GetPartnerTreeByCodeAsync("NONEXISTENT") | null | P1 |
| NEG-073 | GetPartnerCategoryByPartnerGroupCodeAsync invalid | Code="NONEXISTENT" | null | P1 |
| NEG-074 | GetAllDescendantsAsync leaf node | Code of leaf (no children) | Empty list | P1 |
| NEG-075 | GetPartnerCategoryByPartnerGroupCodeAsync top-level | Code of category | null (no parent) | P1 |
| NEG-076 | Pagination page=0 | PageIndex=0 | Default to 1 | P2 |
| NEG-077 | Pagination pageSize=-1 | PageSize=-1 | Error or default | P2 |
| NEG-078 | Pagination pageSize>1000 | PageSize=2000 | Capped to 1000 | P2 |
| NEG-079 | Multiple validation errors | Name=null, Code="" | All errors returned | P1 |
| NEG-080 | Batch create with duplicate codes | Two items same Code | Second fails | P1 |
| NEG-081 | Create for deleted parent | Parent Code exists but IsDeleted | BusinessException | P1 |
| NEG-082 | Update during concurrent delete | Another user deletes | Conflict or last-write | P1 |
| NEG-083 | Cache invalidation after update | Update tree | Fresh data on next Get | P1 |
| NEG-084 | ProcessAllLevelsForCategories null nodes | nodes=null | No exception | P1 |
| NEG-085 | Empty lookup | lookup with no items | Empty enumeration | P1 |
| NEG-086 | Code case sensitivity | "gov" vs "GOV" | Per design (case-sensitive or not) | P2 |
| NEG-087 | Special category codes | Code="MULTILATERAL" | Handled per rule | P1 |
| NEG-088 | Special category codes | Code="GOVERNMENT" | Handled per rule | P1 |
| NEG-089 | API PUT empty array | PUT [] | Empty result or error | P1 |
| NEG-090 | API PUT partial failure | One invalid in batch | Per design | P1 |

---

## §3 Boundary Tests — 90 tests

### String Lengths (15)
| ID | Field | Min | Max | At Min | At Max | Over Max | Priority |
|----|-------|-----|-----|--------|--------|----------|----------|
| BND-001 | Name | 1 | 200 | ✅ "A" | ✅ 200 chars | ❌ 201 chars | P1 |
| BND-002 | Code | 1 | 50 | ✅ "A" | ✅ 50 chars | ❌ 51 chars | P1 |
| BND-003 | Description | 0 | 4000 | ✅ null | ✅ 4000 chars | ❌ 4001 chars | P2 |
| BND-004 | Parent | 0 | 50 | ✅ "" | ✅ 50 chars | ❌ 51 chars | P2 |
| BND-005 | Name 1 char | — | — | "A" | — | — | P1 |
| BND-006 | Name 200 chars | — | — | — | 200 chars | — | P1 |
| BND-007 | Code 1 char | — | — | "X" | — | — | P1 |
| BND-008 | Code 50 chars | — | — | — | 50 chars | — | P1 |
| BND-009 | SortBy column name | — | — | "Name" | "Code" | Invalid | P1 |
| BND-010 | Search term 1 char | — | — | "G" | — | — | P1 |
| BND-011 | Search term 255 chars | — | — | — | 255 chars | — | P1 |
| BND-012 | Category code max | — | 50 | — | 50 chars | — | P2 |
| BND-013 | Group code max | — | 50 | — | 50 chars | — | P2 |
| BND-014 | AI tree output length | — | — | Small tree | Large tree | — | P2 |
| BND-015 | TreeNode label length | — | — | Short | 200 chars | — | P2 |

### Numeric (15)
| ID | Field | Min | Max | Zero | Negative | Priority |
|----|-------|-----|-----|------|----------|----------|
| BND-016 | PartnerTree Id | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-017 | PartnerGroupId | 1 | MAX_INT | ❌ | ❌ | P1 |
| BND-018 | Page | 1 | 10000 | ❌ | ❌ | P1 |
| BND-019 | PageSize | 1 | 1000 | ❌ | ❌ | P1 |
| BND-020 | Id = 1 | — | — | — | — | P2 |
| BND-021 | Id = MAX_INT | — | — | — | — | P2 |
| BND-022 | Partner count per group | 0 | 10000 | ✅ | — | P1 |
| BND-023 | Children per parent | 0 | 500 | ✅ | — | P1 |
| BND-024 | Tree depth (levels) | 0 | 2 | ✅ | — | P1 |
| BND-025 | Categories count | 0 | 1000 | ✅ | — | P1 |
| BND-026 | Groups count | 0 | 5000 | ✅ | — | P1 |
| BND-027 | Display order | 0 | 999 | ✅ | ❌ | P2 |
| BND-028 | Pagination skip | 0 | (total-1)*pageSize | — | — | P1 |
| BND-029 | Last page partial | — | — | 1 item | — | P1 |
| BND-030 | Exactly page size | — | — | 20 items | — | P1 |

### Collections (20)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-031 | 0 trees | Empty list | P1 |
| BND-032 | 1 category only | Single root | P1 |
| BND-033 | 1 category + 1 group | Two levels | P1 |
| BND-034 | 100 trees | Loaded <2s | P1 |
| BND-035 | 1000 trees | Loaded <5s | P1 |
| BND-036 | Category with 0 groups | No children | P1 |
| BND-037 | Category with 100 groups | All listed | P1 |
| BND-038 | Category with 500 groups | Paginated or all | P1 |
| BND-039 | Root with 1 child | Simple tree | P2 |
| BND-040 | Root with 500 children | Wide tree | P1 |
| BND-041 | Depth 0 (root) | Valid | P1 |
| BND-042 | Depth 1 (category) | Valid | P1 |
| BND-043 | Depth 2 (group) | Valid, max | P1 |
| BND-044 | Depth 3 attempt | Rejected or flattened | P1 |
| BND-045 | Descendants 0 | Leaf node | Empty list | P1 |
| BND-046 | Descendants 200 | Large subtree | All returned | P1 |
| BND-047 | BuildHierarchy empty lookup | No items | Empty | P1 |
| BND-048 | BuildHierarchy single item | One root | One item | P1 |
| BND-049 | Partners per group 0 | Empty | 0 | P1 |
| BND-050 | Partners per group 500 | Large | 500 | P1 |

### Unicode & Special (15)
| ID | Field | Input | Expected | Priority |
|----|-------|-------|----------|----------|
| BND-051 | Name (Arabic) | `مؤسسة` | Stored | P2 |
| BND-052 | Name (Chinese) | `政府` | Stored | P2 |
| BND-053 | Name (Cyrillic) | `Организация` | Stored | P2 |
| BND-054 | Name (French) | `Société` | Accents preserved | P2 |
| BND-055 | Name (Emoji) | `🏢 NGO` | Stored | P2 |
| BND-056 | Name with apostrophe | `O'Brien & Co` | Preserved | P1 |
| BND-057 | Name with ampersand | `Smith & Partners` | Preserved | P1 |
| BND-058 | Code with dashes | "GOV-001" | Stored | P1 |
| BND-059 | Code with underscores | "GOV_001" | Stored | P2 |
| BND-060 | Parent with special chars | "GOV-001" | Matched | P1 |
| BND-061 | Description multi-line | "Line1\nLine2" | Preserved | P2 |
| BND-062 | Name with RTL | Arabic text | Correct display | P2 |
| BND-063 | Code alphanumeric | "GOV001" | Accepted | P1 |
| BND-064 | Name 100 chars | 100 chars | Accepted | P1 |
| BND-065 | Code 25 chars | 25 chars | Accepted | P1 |

### Tree Structure (15)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-066 | Flat list (all root) | All at root | P1 |
| BND-067 | Single branch (linear) | Category→Group | P1 |
| BND-068 | Balanced tree | Multiple categories, each with groups | P1 |
| BND-069 | Unbalanced (one category, many groups) | Handled | P1 |
| BND-070 | GetDescendantsRecursive root | All descendants | P1 |
| BND-071 | GetDescendantsRecursive leaf | Empty | P1 |
| BND-072 | GetTreeForAI small tree | Short string | P2 |
| BND-073 | GetTreeForAI large tree | Full hierarchy | P2 |
| BND-074 | TreeNode children null | Leaf node | children=[] | P2 |
| BND-075 | TreeNode children populated | Parent | children.length>0 | P2 |
| BND-076 | Categorization overview empty | No categories | totalCategories=0 | P1 |
| BND-077 | Categorization overview full | Many categories | Correct counts | P1 |
| BND-078 | Sort siblings same parent | 5 siblings | Ordered | P1 |
| BND-079 | Display order 0 | SortOrder=0 | First | P2 |
| BND-080 | Display order max | SortOrder=999 | Last | P2 |

### Additional (10)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| BND-081 | Created at midnight UTC | No boundary error | P2 |
| BND-082 | Deleted at end of year | Correct | P2 |
| BND-083 | Leap year date | Correct | P2 |
| BND-084 | DST transition | Correct | P2 |
| BND-085 | Each valid Type | Level_1, Level_2 | Accepted | P1 |
| BND-086 | Type enum boundaries | First, last | Handled | P2 |
| BND-087 | Pagination last page 1 item | Single item | P2 |
| BND-088 | Pagination empty results | 0 total | P1 |
| BND-089 | All optional null on create | Only required | Created | P1 |
| BND-090 | All optional filled on create | Full model | Created | P1 |

---

## §4 Functional Tests — 90 tests

### 4.1 Workflow (15)
| ID | Rule | Trigger | Expected | Priority |
|----|------|---------|----------|----------|
| FUN-001 | Queries exclude IsDeleted | GetPartnerTreesAsync | Deleted filtered | P0 |
| FUN-002 | Create sets audit | CreatePartnerTreeAsync | CreatedBy, CreatedDate | P0 |
| FUN-003 | Update sets audit | UpdatePartnerTreeAsync | LastModifiedBy, LastModifiedDate | P0 |
| FUN-004 | Delete sets soft-delete | DeletePartnerTreeAsync | IsDeleted, DeletedBy, DeletedDate | P0 |
| FUN-005 | ParentCode links child | Create with Parent="GOV" | Child under GOV in hierarchy | P0 |
| FUN-006 | Code uniqueness | Create duplicate Code | BusinessException | P0 |
| FUN-007 | Name required | Create with Name=null | BusinessException | P0 |
| FUN-008 | Category contains Groups | Create Level_2 with Parent=category | Group under category | P0 |
| FUN-009 | Only 2 levels | Category (Level_1), Group (Level_2) | No Level_3 | P0 |
| FUN-010 | GetDescendantsRecursive nested | Category with groups | All nested children included | P0 |
| FUN-011 | GetTreeForAI formatted | GetTreeForAI | "Category > Group" format | P0 |
| FUN-012 | Categorization overview counts | GetCategorizationOverview | Aggregate per category | P0 |
| FUN-013 | Sort display order | SortOrder set | Siblings in order | P1 |
| FUN-014 | Sort alphabetical | No SortOrder | Alphabetical by Name | P1 |
| FUN-015 | Partner assignment to group | Assign partner | PartnerGroupId set | P1 |

### 4.2 Validation (15)
| ID | Rule | Valid | Invalid | Priority |
|----|------|-------|---------|----------|
| FUN-016 | Name required | "NGO" | null, "" | P0 |
| FUN-017 | Code required | "GOV" | "" | P0 |
| FUN-018 | Code unique | "GOV-002" | "GOV" (exists) | P0 |
| FUN-019 | Parent exists for Level_2 | Parent="GOV" | Parent="NONEXISTENT" | P1 |
| FUN-020 | Type valid | Level_1, Level_2 | "Invalid" | P1 |
| FUN-021 | No circular reference | A→B | A→B→A | P0 |
| FUN-022 | Category at root | Parent="" for Level_1 | — | P1 |
| FUN-023 | Group under category | Parent=category Code | Parent="" for Level_2 | P1 |
| FUN-024 | XSS prevention | "ACME" | "<script>" | P0 |
| FUN-025 | SQL injection prevention | "GOV-001" | "'; DROP" | P0 |
| FUN-026 | Name trimmed | " NGO " | "NGO" | P2 |
| FUN-027 | Code format | Alphanumeric, dash | Control chars | P1 |
| FUN-028 | Description max length | 4000 | 4001 | P2 |
| FUN-029 | Id positive on update | 5 | 0, -1 | P1 |
| FUN-030 | SortBy valid column | "Name", "Code" | "Invalid" | P2 |

### 4.3 Constraints (10)
| ID | Constraint | Expected | Priority |
|----|-----------|----------|----------|
| FUN-031 | Max page size 1000 | Capped | P1 |
| FUN-032 | Code unique DB constraint | Enforced | P0 |
| FUN-033 | Soft-delete no physical delete | Record remains | P0 |
| FUN-034 | Hierarchy depth ≤ 2 | Enforced | P0 |
| FUN-035 | ParentCode references valid Code | Exists in tree | P1 |
| FUN-036 | Partner group FK | PartnerGroupId valid | P1 |
| FUN-037 | BuildHierarchy visitedCodes | Prevents infinite loop | P0 |
| FUN-038 | GetDescendantsRecursive bounded | No stack overflow | P0 |
| FUN-039 | Pagination default | Page=1, Size=20 | P1 |
| FUN-040 | TreeNode structure | label, data, children | Valid | P1 |

### 4.4 Audit (10)
| ID | Action | Expected Audit | Priority |
|----|--------|---------------|----------|
| FUN-041 | Create | CreatedBy=current, CreatedDate | P0 |
| FUN-042 | Update | LastModifiedBy=current, LastModifiedDate | P0 |
| FUN-043 | Delete | DeletedBy=current, DeletedDate | P0 |
| FUN-044 | Read | No audit change | P1 |
| FUN-045 | Failed create | No audit entry | P1 |
| FUN-046 | Batch update | Each item audit set | P1 |
| FUN-047 | Name change | LastModifiedBy updated | P1 |
| FUN-048 | Code change | LastModifiedBy updated | P1 |
| FUN-049 | Description change | LastModifiedBy updated | P1 |
| FUN-050 | Parent change | LastModifiedBy updated | P1 |

### 4.5 Extended Functional (40)
| ID | Rule | Expected | Priority |
|----|------|----------|----------|
| FUN-051 | IsDeleted filter GetPartnerTrees | Excluded | P0 |
| FUN-052 | IsDeleted filter GetPartnerTree | null if deleted | P0 |
| FUN-053 | IsDeleted filter GetDescendants | Excluded | P0 |
| FUN-054 | IsDeleted filter GetPartnersByGroup | Excluded | P1 |
| FUN-055 | IsDeleted filter GetCategorizationOverview | Excluded | P1 |
| FUN-056 | Name from input | Stored | P0 |
| FUN-057 | Code from input | Stored | P0 |
| FUN-058 | Description from input | Stored | P2 |
| FUN-059 | Parent from input | Stored, links child | P0 |
| FUN-060 | Type Level_1 = Category | Category behavior | P0 |
| FUN-061 | Type Level_2 = Group | Group behavior | P0 |
| FUN-062 | Partner multiple groups | Via junction or multiple refs | P2 |
| FUN-063 | Cascade delete category | Groups soft-deleted or error | P1 |
| FUN-064 | Cascade delete group | Partners unassigned or error | P1 |
| FUN-065 | GetTreeForAI excludes deleted | Only active | P0 |
| FUN-066 | GetPostedPartnerTrees excludes deleted | Only active | P1 |
| FUN-067 | GetCategoryAndGroupStructure excludes deleted | Only active | P1 |
| FUN-068 | BuildHierarchy excludes deleted | Only active | P0 |
| FUN-069 | Code case sensitivity | Per design | P2 |
| FUN-070 | SortOrder tie-breaker | Name alphabetical | P2 |
| FUN-071 | Pagination OrderBy | Applied | P1 |
| FUN-072 | Pagination Ascending | Applied | P1 |
| FUN-073 | GetPartnersByPartnerGroup pagination | Page, PageSize | P1 |
| FUN-074 | GetPartnersByPartnerCategoryCode pagination | Page, PageSize | P1 |
| FUN-075 | Categories summary excludes deleted | Only active | P1 |
| FUN-076 | Groups summary excludes deleted | Only active | P1 |
| FUN-077 | GetBasicPartnerCategoryDetails excludes deleted | null if deleted | P1 |
| FUN-078 | GetBasicPartnerGroupDetails excludes deleted | null if deleted | P1 |
| FUN-079 | PrimeNG TreeNode expandable | Has children | P2 |
| FUN-080 | PrimeNG TreeNode selectable | data.id | P2 |
| FUN-081 | GetPartnerTreeByCode case | Exact match | P2 |
| FUN-082 | GetPartnerCategoryByPartnerGroupCode | Parent category | P1 |
| FUN-083 | GetAllDescendantsAsync depth | All levels | P0 |
| FUN-084 | Name property ModifiableDeletableEntity | Required, set | P0 |
| FUN-085 | Status property | EntityStatus | P2 |
| FUN-086 | WorkflowStatus property | If applicable | P2 |
| FUN-087 | Permission check create | CanCreate | P0 |
| FUN-088 | Permission check update | CanUpdate | P0 |
| FUN-089 | Permission check delete | CanDelete | P0 |
| FUN-090 | Permission check read | CanRead | P0 |

---

## §5 Integration Tests — 90 tests

### 5.1 CRUD (10)
| ID | Operation | Entities | Expected | Priority |
|----|----------|----------|----------|----------|
| INT-001 | Full CRUD lifecycle | PartnerTree | Create→Read→Update→Delete | P0 |
| INT-002 | Create → listed | PartnerTree | In GetPartnerTreesAsync | P0 |
| INT-003 | Delete → excluded | PartnerTree | Not in GetPartnerTreesAsync | P0 |
| INT-004 | Update → persisted | PartnerTree | Changes in GetPartnerTreeAsync | P0 |
| INT-005 | Create category + group chain | PartnerTree x2 | Both in hierarchy | P0 |
| INT-006 | Create → GetDescendantsRecursive | PartnerTree | In descendants | P1 |
| INT-007 | Update → GetTreeForAI | PartnerTree | Updated in AI output | P1 |
| INT-008 | Delete → GetCategorizationOverview | PartnerTree | Excluded from counts | P1 |
| INT-009 | Batch create | PartnerTree x10 | All created | P2 |
| INT-010 | Create with all optional | PartnerTree | Created | P1 |

### 5.2 Search & Filter (10)
| ID | Criteria | Expected | Priority |
|----|----------|----------|----------|
| INT-011 | GetPartnersByPartnerGroup | Group's partners | P0 |
| INT-012 | GetPartnersByPartnerCategoryCode | Category + descendants' partners | P0 |
| INT-013 | GetPartnerTreesAsync sort Name | Sorted | P1 |
| INT-014 | GetPartnerTreesAsync sort Code | Sorted | P1 |
| INT-015 | GetCategoryAndGroupStructure | Categories with Children | P1 |
| INT-016 | GetCategoriesSummary | totalCategories, categories | P1 |
| INT-017 | GetGroupsSummary | totalGroups, groups | P1 |
| INT-018 | GetCategorizationOverview | Full summary | P1 |
| INT-019 | GetPostedPartnerTrees | External model list | P1 |
| INT-020 | GetPartnerTreeByCode | By code | P1 |

### 5.3 Pagination (5)
| ID | Page/Size | Expected | Priority |
|----|-----------|----------|----------|
| INT-021 | Page 1, Size 20 | 20 items | P1 |
| INT-022 | Last page partial | Remaining | P1 |
| INT-023 | Empty results | 0 total | P1 |
| INT-024 | Single page | All items | P2 |
| INT-025 | Max page size 1000 | 1000 items | P2 |

### 5.4 Relationships (10)
| ID | Relationship | Expected | Priority |
|----|-------------|----------|----------|
| INT-026 | Category → Groups | Children loaded | P0 |
| INT-027 | Group → Category | Parent reference | P0 |
| INT-028 | PartnerTree → Partners | Partners by PartnerGroupId | P1 |
| INT-029 | PartnerTree → PartnerTree (parent) | ParentCode link | P0 |
| INT-030 | PartnerTree → PartnerTree (children) | Children in hierarchy | P0 |
| INT-031 | Delete category → groups | Cascade or error | P1 |
| INT-032 | Delete group → partners | Unassign or error | P1 |
| INT-033 | Partner multiple groups | Multiple PartnerGroupIds | P2 |
| INT-034 | BuildHierarchy → TreeNode | Convertible | P1 |
| INT-035 | GetTreeForAI → prompt | Usable string | P1 |

### 5.5 Error Handling (15)
| ID | Error | Expected | Priority |
|----|------|----------|----------|
| INT-036 | Invalid data → 400 | BusinessException | P0 |
| INT-037 | Not found → 404 | KeyNotFound or null | P0 |
| INT-038 | Unauthorized → 403 | Forbidden | P0 |
| INT-039 | Duplicate code → 400 | BusinessException | P0 |
| INT-040 | Circular reference → 400 | Prevented | P0 |
| INT-041 | Null model → 400 | ArgumentNull | P0 |
| INT-042 | Invalid Type → 400 | BusinessException | P1 |
| INT-043 | DB timeout → 500 | Graceful error | P1 |
| INT-044 | Concurrency conflict → 409 | Optimistic concurrency | P1 |
| INT-045 | Malformed request → 400 | Validation | P1 |
| INT-046 | SQL injection → sanitized | No harm | P0 |
| INT-047 | Rate limit → 429 | Rate limit response | P2 |
| INT-048 | Session expired → 401 | Auth required | P1 |
| INT-049 | FK violation → 400 | BusinessException | P1 |
| INT-050 | Constraint violation → 400 | BusinessException | P1 |

### 5.6 Extended Integration (40)
| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| INT-051 | API POST create | 201 Created | P0 |
| INT-052 | API GET list | 200 hierarchical | P0 |
| INT-053 | API GET by ID | 200 with tree | P0 |
| INT-054 | API PUT update | 200 updated | P0 |
| INT-055 | API DELETE | 204 No Content | P0 |
| INT-056 | API GET permissions | 200 canRead, canUpdate, canDelete | P0 |
| INT-057 | API GET structure | 200 category/group | P0 |
| INT-058 | API GET by-partner-group-id | 200 paginated partners | P1 |
| INT-059 | API GET by-partner-category-code | 200 paginated partners | P1 |
| INT-060 | API GET categories-summary | 200 summary | P1 |
| INT-061 | API GET groups-summary | 200 summary | P1 |
| INT-062 | API GET categorization-overview | 200 full | P1 |
| INT-063 | Controller → Manager | Correct resolution | P1 |
| INT-064 | Manager → Repository | DbContext | P1 |
| INT-065 | AutoMapper Entity→Model | All fields | P1 |
| INT-066 | AutoMapper Request→Entity | All fields | P1 |
| INT-067 | RBAC interceptor | Row filtering | P1 |
| INT-068 | Permission endpoint | canRead, canUpdate, canDelete | P0 |
| INT-069 | Full hierarchy API flow | Create→Get→Update→Delete | P0 |
| INT-070 | GetDescendantsRecursive API | Via service | P1 |
| INT-071 | GetTreeForAI API | Via service | P1 |
| INT-072 | Partner assignment flow | Assign→GetPartnersByGroup | P1 |
| INT-073 | Category deletion flow | Delete→GetCategorizationOverview | P1 |
| INT-074 | Group deletion flow | Delete→GetPartnersByGroup | P1 |
| INT-075 | Multi-tenant isolation | OrgUnit scope | P1 |
| INT-076 | Audit trail integration | Create→Audit entry | P1 |
| INT-077 | Soft-delete integration | Delete→IsDeleted | P0 |
| INT-078 | Pagination integration | GetEntityLinks paginated | P1 |
| INT-079 | Sort integration | OrderBy applied | P1 |
| INT-080 | TreeNode conversion | Hierarchy→PrimeNG | P2 |
| INT-081 | AI prompt integration | GetTreeForAI→prompt | P2 |
| INT-082 | Notification on delete | If applicable | P2 |
| INT-083 | Cache integration | Invalidation on update | P1 |
| INT-084 | Logging integration | Operations logged | P2 |
| INT-085 | Error handler integration | Exceptions→ProblemDetails | P1 |
| INT-086 | Validation integration | Data annotations | P1 |
| INT-087 | DbContext scope | Per request | P1 |
| INT-088 | ManagerWrapper resolution | IPartnerTreeManager | P1 |
| INT-089 | UNOPS override | UNOPSPartnerTreeManager when configured | P1 |
| INT-090 | End-to-end create hierarchy | Category→Groups→Partners | P0 |

---

## §6 Security Tests — 50 tests (OUT OF SCOPE)

Security tests are covered in a separate Security test suite. Categories: Injection (10), Access Control (10), IDOR (10), Mass Assignment (5), Auth & Session (10), Data Exposure (5).

---

## §7 Concurrency Tests — 25 tests

| ID | Scenario | Expected | Priority |
|----|----------|----------|----------|
| CON-001 | Two users update same tree | Conflict or last-write | P1 |
| CON-002 | Two users create same Code | One succeeds, one fails | P0 |
| CON-003 | Create during GetPartnerTrees | Consistent view | P1 |
| CON-004 | Delete during read | Null or pre-delete | P1 |
| CON-005 | Update during BuildHierarchy | Consistent snapshot | P1 |
| CON-006 | Concurrent GetDescendantsRecursive | Both succeed | P1 |
| CON-007 | Concurrent GetCategorizationOverview | Both succeed | P1 |
| CON-008 | Delete during GetPartnersByGroup | Handled | P1 |
| CON-009 | DB deadlock | Resolved, retry | P1 |
| CON-010 | Token refresh during update | Retry | P1 |
| CON-011 | Bulk create concurrent | All complete | P2 |
| CON-012 | Optimistic concurrency | Conflict detected | P1 |
| CON-013 | Concurrent soft-delete | One succeeds | P1 |
| CON-014 | Rapid create/delete | Final state correct | P1 |
| CON-015 | Connection pool exhaustion | Graceful | P1 |
| CON-016 | Cache invalidation concurrent | Fresh data | P1 |
| CON-017 | Multiple users creating categories | All succeed | P2 |
| CON-018 | Concurrent hierarchy build | Correct structure | P1 |
| CON-019 | Update during delete | Conflict | P1 |
| CON-020 | Session timeout during update | Rolled back | P1 |
| CON-021 | Concurrent partner assignment | Both succeed | P2 |
| CON-022 | Tree rebuild during browse | Consistent | P1 |
| CON-023 | Parallel GetPartnerTreeAsync | No interference | P1 |
| CON-024 | Concurrent GetTreeForAI | Both succeed | P2 |
| CON-025 | Real-time update propagation | Eventually consistent | P2 |

---

## §8 Unit Tests — 21 tests

| ID | Category | Input | Expected | Priority |
|----|----------|-------|----------|----------|
| UNT-001 | Validation | Null Name | Invalid | P1 |
| UNT-002 | Validation | Empty Code | Invalid | P1 |
| UNT-003 | Validation | Duplicate Code | Invalid | P0 |
| UNT-004 | Validation | Circular hierarchy | Invalid | P0 |
| UNT-005 | Validation | Invalid Type | Invalid | P1 |
| UNT-006 | Formatting | Name trim | " NGO " → "NGO" | P1 |
| UNT-007 | Formatting | Parent empty normalize | null → "" | P1 |
| UNT-008 | Formatting | Hierarchy path | "GOV > GOV-001" | P2 |
| UNT-009 | Calculation | Tree depth | 2 levels | P1 |
| UNT-010 | Calculation | Descendant count | Correct | P1 |
| UNT-011 | Calculation | Pagination pages | 55/20=3 | P1 |
| UNT-012 | Calculation | HasNext | True for page 1 of 3 | P1 |
| UNT-013 | Calculation | Subtree size | Correct | P1 |
| UNT-014 | Status | IsDeleted check | True → excluded | P1 |
| UNT-015 | Status | Valid parent type | Level_2 under Level_1 | P1 |
| UNT-016 | Status | Circular detection | visitedCodes | P0 |
| UNT-017 | Status | Deletion eligibility | No partners | P1 |
| UNT-018 | Status | Code uniqueness | Enforced | P0 |
| UNT-019 | Collections | BuildHierarchy from flat | Correct hierarchy | P1 |
| UNT-020 | Collections | Filter by Type | Level_1 only | P1 |
| UNT-021 | Collections | Sort siblings | Ordered | P1 |

---

## §9 Performance Tests — 16 tests

| ID | Operation | Threshold | Priority |
|----|----------|----------|----------|
| PRF-001 | Create single | < 200ms | P1 |
| PRF-002 | GetPartnerTreeAsync | < 100ms | P1 |
| PRF-003 | GetPartnerTreesAsync 100 trees | < 500ms | P1 |
| PRF-004 | GetPartnerTreesAsync 1000 trees | < 2s | P1 |
| PRF-005 | GetDescendantsRecursive 500 | < 1s | P1 |
| PRF-006 | GetCategorizationOverview | < 500ms | P1 |
| PRF-007 | GetTreeForAI | < 300ms | P1 |
| PRF-008 | GetPartnersByPartnerGroup paginated | < 500ms | P1 |
| PRF-009 | BuildHierarchy 1000 | < 1s | P1 |
| PRF-010 | Count query | < 100ms | P1 |
| PRF-011 | 10 concurrent creates | < 1s each | P2 |
| PRF-012 | 50 concurrent reads | < 500ms each | P2 |
| PRF-013 | 10 concurrent GetPartnerTrees | < 1s each | P2 |
| PRF-014 | Memory 10,000 trees | < 200MB | P2 |
| PRF-015 | Memory 50,000 trees | < 500MB | P2 |
| PRF-016 | Memory leak check | No growth > 10% | P1 |

---

## §10 Load Tests — 10 tests

| ID | Profile | Duration | Criteria | Priority |
|----|---------|----------|----------|----------|
| LDT-001 | 50 concurrent CRUD | 30 min | 95% < 500ms | P2 |
| LDT-002 | 100 concurrent reads | 30 min | 95% < 300ms | P2 |
| LDT-003 | 50 concurrent GetPartnerTrees | 15 min | < 1s | P2 |
| LDT-004 | Spike 10→200 req/s | 5 min | Recovery < 30s | P2 |
| LDT-005 | Spike + hierarchy build | 5 min | All correct | P2 |
| LDT-006 | 500 concurrent | 10 min | Graceful degradation | P2 |
| LDT-007 | 100K trees in DB | 15 min | Queries < 1s | P2 |
| LDT-008 | Continuous create/delete | 10 min | Stable | P2 |
| LDT-009 | Recovery after DB crash | N/A | < 60s | P2 |
| LDT-010 | Recovery after restart | N/A | < 30s | P2 |

---

## Traceability Matrix

| Business Rule | Test Cases |
|--------------|------------|
| Tree hierarchy (Category > Group) | POS-001–002, FUN-005–009, NEG-036–037, BND-041–044 |
| Code-based parent matching | POS-017, FUN-005, FUN-059, NEG-013, BND-060 |
| Code uniqueness | POS-015, FUN-006, NEG-001, CON-002 |
| IsDeleted filtering | POS-018, FUN-001, FUN-051–055, NEG-041–042 |
| Recursive descendants | POS-007, FUN-010, NEG-018, BND-045–046 |
| GetTreeForAI | POS-008, FUN-011, INT-007, PRF-007 |
| Categorization overview | POS-009, FUN-012, INT-008, BND-076–077 |
| Soft-delete cascade | NEG-039–040, FUN-063–064, INT-031–032 |
| Partner assignments | POS-024–025, FUN-015, INT-033 |
| PrimeNG TreeNode | POS-026, FUN-040, BND-074–075, INT-034 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Execution
