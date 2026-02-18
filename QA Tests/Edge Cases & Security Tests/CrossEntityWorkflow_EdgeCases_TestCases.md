# Cross-Entity Workflow — Edge Cases & Test Cases

**Component:** Cross-Entity Workflows (Partner, Contact, Interaction, Document, Link, Opportunity, Organization, Workflow, Notification)  
**Created:** 2026-02-18  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

---

## Compliance Summary

| Category | Count | Min | ✓ |
|----------|-------|-----|---|
| §1 Positive | 30 | 30 | ✅ |
| §2 Negative | 90 | 90 | ✅ |
| §3 Boundary | 90 | 90 | ✅ |
| §4 Functional | 90 | 90 | ✅ |
| §5 Integration | 90 | 90 | ✅ |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **≥462** | ✅ |

**3:1 Ratio Checks:** N≥3P? 90≥90 ✅ | E≥3P? 90≥90 ✅ | F≥3P? 90≥90 ✅ | I≥3P? 90≥90 ✅

---

## Feature Overview

**Cross-Entity Workflow** covers relationships between Partner, Contact, Interaction, Document, Link, Opportunity, Organization, Workflow, and Notification. Edge cases include soft-delete cascades, orphaned records, concurrent modifications, workflow state consistency, and full lifecycle flows across multiple entities.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|--------------|-------|-----------------|----------|
| POS-001 | Partner with contacts, interactions, documents, links, org units | Partner created | Load partner with all | All populated | P0 |
| POS-002 | Opportunity with all 21 GO requirements satisfied | Complete opportunity | Submit workflow | Submit succeeds | P0 |
| POS-003 | Complete workflow lifecycle: create → profile → submit → approve → GO | Opportunity created | Full flow | GO achieved | P0 |
| POS-004 | Partner → Contact → Interaction chain | Partner, Contact | Create interaction | Interaction linked | P0 |
| POS-005 | Opportunity → Country → EntityArtifact → Tags chain | Opportunity | Add country, artifact, tags | All linked | P0 |
| POS-006 | Workflow → Notification → Actions Required | Workflow transition | Transition | Notification created | P0 |
| POS-007 | Create partner with initial contact | New partner | Create with contact | Both created | P0 |
| POS-008 | Add document to opportunity | Opportunity exists | Add document | Document linked | P0 |
| POS-009 | Add link to partner | Partner exists | Add link | Link linked | P0 |
| POS-010 | Interaction with contact and partner | Contact, Partner | Create interaction | Both linked | P0 |
| POS-011 | Organization hierarchy: Office under Region | Region, Office | Create Office | Hierarchy correct | P0 |
| POS-012 | Partner with multiple contacts | Partner | Add 5 contacts | All linked | P0 |
| POS-013 | Opportunity with multiple countries | Opportunity | Add countries | All linked | P0 |
| POS-014 | Workflow submit with DoA2 holder | DoA2 assigned | Submit | Success | P0 |
| POS-015 | Soft-delete partner, contacts excluded from list | Partner deleted | List contacts | Empty or excluded | P0 |
| POS-016 | Restore soft-deleted partner | Partner deleted | Restore | Restored | P1 |
| POS-017 | Partner with 1 contact | Partner | Add 1 contact | Linked | P0 |
| POS-018 | Opportunity with 1 country | Opportunity | Add 1 country | Linked | P0 |
| POS-019 | Notification for workflow with 5 stakeholders | 5 stakeholders | Transition | Notifications | P0 |
| POS-020 | Document type for Partner entity | Partner | Add Agreement doc | Valid | P0 |
| POS-021 | Full partner lifecycle: create → add contacts → add docs → add links | New partner | Full flow | All created | P0 |
| POS-022 | Opportunity creation through GO decision | Draft opportunity | Complete flow | GO | P0 |
| POS-023 | Data consistency after soft-delete | Partner deleted | Query contacts | Consistent | P0 |
| POS-024 | Workflow state preserved through system restart | Opportunity Submitted | Restart | State preserved | P1 |
| POS-025 | Gmail → Contact matching → Interaction creation → Partner update | Email | Full flow | All updated | P1 |
| POS-026 | Partner tree with children | Parent partner | Add children | Tree correct | P0 |
| POS-027 | Opportunity budget with currency | Opportunity | Set budget | Saved | P0 |
| POS-028 | Interaction date range valid | Interaction | Set dates | Valid | P0 |
| POS-029 | Organization hierarchy at depth 2 | Region → Office | Create | Hierarchy | P0 |
| POS-030 | Bulk operations across entities | Multiple partners | Bulk update | All updated | P1 |

---

## §2 Negative Tests (90)

### 2.1 Soft-Delete & Cascade (20)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-001 | Soft-delete partner while contact references it | Delete partner | Contact handled (orphan or block) | P0 |
| NEG-002 | Delete contact while interaction references it | Delete contact | Interaction handled | P0 |
| NEG-003 | Change org unit while opportunity references it | Change org | Opportunity handled | P0 |
| NEG-004 | Remove DoA2 holder while opportunity pending approval | Remove DoA2 | Rejected or warning | P0 |
| NEG-005 | Delete partner with active contacts | Delete | Block or cascade | P0 |
| NEG-006 | Delete partner with active opportunity | Delete | Block or cascade | P0 |
| NEG-007 | Delete contact with interactions | Delete | Block or cascade | P0 |
| NEG-008 | Delete document type with documents | Delete type | Block or cascade | P0 |
| NEG-009 | Delete country with opportunities | Delete country | Block or cascade | P0 |
| NEG-010 | Delete org unit with partners | Delete org | Block or cascade | P0 |
| NEG-011 | Soft-delete parent in hierarchy | Delete parent | Children handled | P0 |
| NEG-012 | Restore partner with deleted contacts | Restore | Contacts handled | P1 |
| NEG-013 | Delete link while entity exists | Delete link | Handled | P1 |
| NEG-014 | Delete notification while workflow pending | Delete | Handled | P1 |
| NEG-015 | Orphaned records from failed cascade | Partial cascade fail | No orphans | P0 |
| NEG-016 | Concurrent soft-delete and update | Concurrent | Deterministic | P0 |
| NEG-017 | Soft-delete during workflow transition | Delete during transition | Handled | P0 |
| NEG-018 | Delete entity with pending notifications | Delete | Notifications handled | P1 |
| NEG-019 | Delete workflow status in use | Delete status | Blocked | P0 |
| NEG-020 | Cascade delete depth 5 | Deep hierarchy | Handled | P1 |

### 2.2 Concurrent Modifications (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-021 | Concurrent workflow and entity modification | User A workflow, B edit | Handled | P0 |
| NEG-022 | Two users update same partner | Concurrent update | Optimistic lock or last-write | P0 |
| NEG-023 | Two users add contact to same partner | Concurrent add | Both succeed or one | P0 |
| NEG-024 | Two users submit same opportunity | Concurrent submit | One succeeds | P0 |
| NEG-025 | Update partner during interaction create | Concurrent | Handled | P0 |
| NEG-026 | Delete contact during interaction create | Concurrent | Handled | P0 |
| NEG-027 | Workflow transition during edit | Concurrent | Handled | P0 |
| NEG-028 | Add document during opportunity delete | Concurrent | Handled | P0 |
| NEG-029 | Update org during partner update | Concurrent | Handled | P1 |
| NEG-030 | Bulk update during single update | Concurrent | Handled | P1 |
| NEG-031 | Notification create during workflow rollback | Concurrent | Handled | P1 |
| NEG-032 | Link add during partner delete | Concurrent | Handled | P0 |
| NEG-033 | Contact merge during interaction create | Concurrent | Handled | P1 |
| NEG-034 | Opportunity country add during delete | Concurrent | Handled | P1 |
| NEG-035 | Stale entity update | Old version | ConcurrencyException | P0 |

### 2.3 Orphaned Records & Integrity (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-036 | Orphaned contact (partner deleted) | Partner deleted | Contact orphan or cascade | P0 |
| NEG-037 | Orphaned interaction (contact deleted) | Contact deleted | Interaction orphan or cascade | P0 |
| NEG-038 | Orphaned document (entity deleted) | Entity deleted | Document handled | P0 |
| NEG-039 | Orphaned link (entity deleted) | Entity deleted | Link handled | P0 |
| NEG-040 | Orphaned notification (entity deleted) | Entity deleted | Notification handled | P1 |
| NEG-041 | Failed cascade leaves orphans | Partial fail | Rollback or cleanup | P0 |
| NEG-042 | Invalid FK on create | PartnerId=99999 | ForeignKeyException | P0 |
| NEG-043 | Invalid FK on update | Update to invalid | Rejected | P0 |
| NEG-044 | Circular reference in hierarchy | A→B→A | Rejected | P0 |
| NEG-045 | Self-reference in hierarchy | Parent=self | Rejected | P0 |
| NEG-046 | Duplicate link same entity | Same URL | Rejected or dedupe | P1 |
| NEG-047 | Duplicate contact email same partner | Same email | Rejected | P0 |
| NEG-048 | Invalid workflow transition | Wrong transition | Rejected | P0 |
| NEG-049 | Opportunity with deleted partner | Partner deleted | Handled | P0 |
| NEG-050 | Interaction with deleted contact | Contact deleted | Handled | P0 |

### 2.4 Workflow & State (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-051 | Submit without 21 requirements | Incomplete | Rejected | P0 |
| NEG-052 | Approve without permission | No CanApprove | 403 | P0 |
| NEG-053 | Submit from wrong stage | Already Submitted | Rejected | P0 |
| NEG-054 | GO without approval | Skip approval | Rejected | P0 |
| NEG-055 | Edit opportunity in Submitted state | Wrong state | Rejected | P0 |
| NEG-056 | Delete opportunity in Active state | Wrong state | Rejected | P0 |
| NEG-057 | Workflow with missing DoA2 | No DoA2 | Rejected | P0 |
| NEG-058 | Workflow with deleted DoA2 user | User deleted | Rejected | P0 |
| NEG-059 | Transition with stale workflow state | Stale | Rejected | P0 |
| NEG-060 | Notification for deleted entity | Entity deleted | Handled | P1 |
| NEG-061 | Workflow rollback | Rollback | State restored | P1 |
| NEG-062 | Invalid workflow action | Wrong action | Rejected | P0 |
| NEG-063 | Workflow permission revoked mid-flow | Permission revoked | Rejected | P0 |
| NEG-064 | Opportunity with invalid budget | Budget < 0 | Rejected | P0 |
| NEG-065 | Workflow with soft-deleted country | Country deleted | Handled | P1 |

### 2.5 Validation & Permission (25)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| NEG-066 | Create contact without partner | No PartnerId | Rejected | P0 |
| NEG-067 | Create interaction without contact | No ContactId | Rejected | P0 |
| NEG-068 | Create document without entity | No EntityId | Rejected | P0 |
| NEG-069 | Create link without entity | No EntityId | Rejected | P0 |
| NEG-070 | Create opportunity without partner | No PartnerId | Rejected | P0 |
| NEG-071 | Cross-org partner access | User org A, partner org B | 403 | P0 |
| NEG-072 | Cross-org opportunity access | User org A, opp org B | 403 | P0 |
| NEG-073 | Create without CanCreate | No permission | 403 | P0 |
| NEG-074 | Update without CanEdit | No permission | 403 | P0 |
| NEG-075 | Delete without CanDelete | No permission | 403 | P0 |
| NEG-076 | View without CanView | No permission | 403 | P0 |
| NEG-077 | Invalid contact email | Bad email | Rejected | P0 |
| NEG-078 | Invalid link URL | Bad URL | Rejected | P0 |
| NEG-079 | Invalid interaction date range | End < Start | Rejected | P0 |
| NEG-080 | Invalid document type for entity | Wrong type | Rejected | P0 |
| NEG-081 | Null required field | Null name | Rejected | P0 |
| NEG-082 | Empty required field | Empty name | Rejected | P0 |
| NEG-083 | Invalid org hierarchy | Invalid parent | Rejected | P0 |
| NEG-084 | Invalid country for opportunity | Invalid country | Rejected | P0 |
| NEG-085 | Bulk operation partial permission | Some no access | Filtered or rejected | P0 |
| NEG-086 | Import with invalid FK | Bad PartnerId in CSV | Rejected | P0 |
| NEG-087 | Export without permission | No CanExport | 403 | P0 |
| NEG-088 | Workflow action wrong user | Not DoA2 | Rejected | P0 |
| NEG-089 | Entity configuration missing | No config | Default or error | P1 |
| NEG-090 | Audit trail tampering | Tamper audit | Detected | P0 |

---

## §3 Boundary Tests (90)

### 3.1 Collection Sizes (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-001 | Partner with 0 contacts | No contacts | Empty list | P0 |
| BND-002 | Partner with 1 contact | 1 contact | 1 in list | P0 |
| BND-003 | Partner with 1000 contacts | 1000 contacts | All or paginated | P0 |
| BND-004 | Opportunity with 0 countries | No countries | Empty | P0 |
| BND-005 | Opportunity with 1 country | 1 country | 1 in list | P0 |
| BND-006 | Opportunity with all countries | Max countries | All or capped | P1 |
| BND-007 | Partner with 0 documents | No docs | Empty | P0 |
| BND-008 | Partner with 100 documents | 100 docs | Paginated | P1 |
| BND-009 | Partner with 0 links | No links | Empty | P0 |
| BND-010 | Partner with 50 links | 50 links | All or paginated | P1 |
| BND-011 | Opportunity with 0 stakeholders | No stakeholders | Empty | P0 |
| BND-012 | Opportunity with 100 stakeholders | 100 stakeholders | Paginated | P1 |
| BND-013 | Workflow with 0 notifications | No notifications | Empty | P0 |
| BND-014 | Workflow with 100+ notifications | 100+ | Paginated | P0 |
| BND-015 | Organization with 0 children | Leaf node | Empty | P0 |

### 3.2 Hierarchy Depth (10)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-016 | Organization hierarchy at max depth | Max depth | Valid or rejected | P0 |
| BND-017 | Organization hierarchy depth 1 | Region only | Valid | P0 |
| BND-018 | Organization hierarchy depth 2 | Region → Office | Valid | P0 |
| BND-019 | Organization hierarchy depth 3 | Region → Office → Unit | Valid | P0 |
| BND-020 | Partner tree depth 1 | Parent only | Valid | P0 |
| BND-021 | Partner tree depth 5 | 5 levels | Valid or capped | P1 |
| BND-022 | Partner tree max depth | At limit | Valid | P1 |
| BND-023 | Partner tree over max depth | Over limit | Rejected | P0 |
| BND-024 | Org unit with no parent | Root | Valid | P0 |
| BND-025 | Org unit with invalid parent | Parent=99999 | Rejected | P0 |

### 3.3 Concurrent Updates (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-026 | Concurrent updates to same partner | 2 users | One or both | P0 |
| BND-027 | Concurrent updates to same contact | 2 users | One or both | P0 |
| BND-028 | Concurrent updates to same opportunity | 2 users | One or both | P0 |
| BND-029 | Concurrent updates from different managers | Partner + Contact | Handled | P0 |
| BND-030 | Concurrent add contact to same partner | 2 users | Both succeed | P0 |
| BND-031 | Concurrent add document to same opportunity | 2 users | Both succeed | P0 |
| BND-032 | Concurrent workflow transitions | 2 users | One succeeds | P0 |
| BND-033 | Concurrent soft-delete same entity | 2 users | One succeeds | P0 |
| BND-034 | Concurrent restore same entity | 2 users | One succeeds | P1 |
| BND-035 | Update during read | Read during update | Consistent read | P0 |
| BND-036 | Delete during read | Read during delete | Handled | P0 |
| BND-037 | Bulk update concurrent with single | Concurrent | Handled | P1 |
| BND-038 | Import concurrent with create | Concurrent | Handled | P1 |
| BND-039 | Export concurrent with update | Concurrent | Handled | P1 |
| BND-040 | Notification create concurrent with read | Concurrent | Handled | P1 |

### 3.4 Notification Volume (10)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-041 | Notification for workflow with 100+ stakeholders | 100 stakeholders | All notified or batched | P0 |
| BND-042 | Notification for 1 stakeholder | 1 stakeholder | 1 notification | P0 |
| BND-043 | Notification for 0 stakeholders | No stakeholders | No notifications | P0 |
| BND-044 | Bulk notification mark read | 100 notifications | All marked | P1 |
| BND-045 | Notification queue full | Queue at limit | Throttled or queued | P1 |
| BND-046 | Notification for deleted stakeholder | User deleted | Handled | P1 |
| BND-047 | Notification duplicate | Same event twice | Dedupe or both | P1 |
| BND-048 | Notification priority | High priority | Delivered first | P1 |
| BND-049 | Notification batch size | 50 per batch | Batched | P1 |
| BND-050 | Notification TTL | Expired | Cleanup | P1 |

### 3.5 Numeric & Length (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-051 | Partner name max length | 500 chars | Valid or truncated | P1 |
| BND-052 | Contact email max length | 254 chars | Valid | P1 |
| BND-053 | Opportunity description 10,000 chars | 10000 | Valid | P1 |
| BND-054 | Opportunity budget max | decimal.MaxValue | Handled | P1 |
| BND-055 | Opportunity budget zero | 0 | Rejected | P0 |
| BND-056 | Interaction date range max | 1 year | Valid or capped | P1 |
| BND-057 | Link URL max length | 2048 | Valid or rejected | P1 |
| BND-058 | Document name max length | 255 | Valid or truncated | P1 |
| BND-059 | Pagination page 0 | page=0 | First page | P1 |
| BND-060 | Pagination pageSize max | 100 | Capped | P1 |
| BND-061 | Total count overflow | Very large | Handled | P1 |
| BND-062 | Id at max int | Id=int.MaxValue | Handled | P1 |
| BND-063 | Id at zero | Id=0 | Rejected | P0 |
| BND-064 | Negative Id | Id=-1 | Rejected | P0 |
| BND-065 | Float precision budget | 1000.999 | Rounded | P1 |

### 3.6 Null & Empty (15)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-066 | Null partner in contact | PartnerId=null | Rejected | P0 |
| BND-067 | Null contact in interaction | ContactId=null | Rejected | P0 |
| BND-068 | Null entity in document | EntityId=null | Rejected | P0 |
| BND-069 | Null entity in link | EntityId=null | Rejected | P0 |
| BND-070 | Null partner in opportunity | PartnerId=null | Rejected | P0 |
| BND-071 | Empty contact list | Partner no contacts | Empty list | P0 |
| BND-072 | Empty interaction list | Contact no interactions | Empty list | P0 |
| BND-073 | Empty document list | Entity no documents | Empty list | P0 |
| BND-074 | Null workflow status | Status=null | Handled | P1 |
| BND-075 | Null org unit | OrgId=null | Handled | P1 |
| BND-076 | Empty opportunity countries | No countries | Empty | P0 |
| BND-077 | Null DoA2 holder | DoA2=null | Rejected for submit | P0 |
| BND-078 | Empty notification list | No notifications | Empty | P0 |
| BND-079 | Null parent in hierarchy | ParentId=null | Root | P1 |
| BND-080 | Null name | Name=null | Rejected | P0 |

### 3.7 State Transitions (10)

| ID | Test Name | Input | Expected | Priority |
|----|-----------|-------|----------|----------|
| BND-081 | Workflow Draft→Submitted | Valid | Success | P0 |
| BND-082 | Workflow Submitted→Approved | Valid | Success | P0 |
| BND-083 | Workflow Approved→GO | Valid | Success | P0 |
| BND-084 | Workflow Draft→NoGo | Valid | Success | P0 |
| BND-085 | Partner Active→Inactive | Valid | Success | P0 |
| BND-086 | Partner Inactive→Active | Valid | Success | P0 |
| BND-087 | Contact status change | Valid | Success | P0 |
| BND-088 | Document status change | Valid | Success | P1 |
| BND-089 | Notification unread→read | Valid | Success | P0 |
| BND-090 | Workflow at final state | GO | No further transition | P0 |

---

## §4 Functional Tests (90)

### 4.1 Full Partner Lifecycle (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-001 | Full partner lifecycle with all related entities | Create → contacts → docs → links → org | All created | P0 |
| FUN-002 | Partner create with audit | Create | CreatedBy, CreatedDate | P0 |
| FUN-003 | Partner update with audit | Update | LastModifiedBy, LastModifiedDate | P0 |
| FUN-004 | Partner soft-delete with audit | Delete | DeletedBy, DeletedDate | P0 |
| FUN-005 | Partner restore | Restore | IsDeleted=false | P0 |
| FUN-006 | Partner with contact cascade | Add contact | Contact linked | P0 |
| FUN-007 | Partner with document cascade | Add document | Document linked | P0 |
| FUN-008 | Partner with link cascade | Add link | Link linked | P0 |
| FUN-009 | Partner with org unit | Assign org | Org linked | P0 |
| FUN-010 | Partner filter by org | Filter | Filtered | P0 |
| FUN-011 | Partner filter by status | Filter | Filtered | P0 |
| FUN-012 | Partner search | Search | Results | P0 |
| FUN-013 | Partner pagination | Paginate | Correct page | P0 |
| FUN-014 | Partner sort | Sort | Ordered | P0 |
| FUN-015 | Partner export | Export | Exported | P0 |

### 4.2 Opportunity Creation through GO (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-016 | Opportunity creation through GO decision | Full flow | GO | P0 |
| FUN-017 | Opportunity with all validations | All 21 requirements | Submit allowed | P0 |
| FUN-018 | Opportunity DST before submit | DST run | Recommendations | P0 |
| FUN-019 | Opportunity workflow submit | Submit | Submitted | P0 |
| FUN-020 | Opportunity workflow approve | Approve | Approved | P0 |
| FUN-021 | Opportunity workflow GO | GO | GO | P0 |
| FUN-022 | Opportunity budget validation | Budget > 0 | Valid | P0 |
| FUN-023 | Opportunity country validation | Valid country | Valid | P0 |
| FUN-024 | Opportunity DoA2 validation | DoA2 assigned | Valid | P0 |
| FUN-025 | Opportunity team section | Team populated | Valid | P0 |
| FUN-026 | Opportunity WHAT section | WHAT populated | Valid | P0 |
| FUN-027 | Opportunity WHY section | WHY populated | Valid | P0 |
| FUN-028 | Opportunity risk assessment | Risks | Valid | P0 |
| FUN-029 | Opportunity schedule | Schedule | Valid | P0 |
| FUN-030 | Opportunity partner validation | Partner valid | Valid | P0 |

### 4.3 Data Consistency (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-031 | Data consistency after soft-delete across related entities | Partner deleted | Contacts excluded | P0 |
| FUN-032 | Data consistency on restore | Partner restored | Contacts visible | P0 |
| FUN-033 | Data consistency on cascade | Delete partner | Cascade handled | P0 |
| FUN-034 | Data consistency on concurrent update | 2 updates | Consistent | P0 |
| FUN-035 | Data consistency on transaction rollback | Rollback | All rolled back | P0 |
| FUN-036 | IsDeleted filter on all queries | Query | Deleted excluded | P0 |
| FUN-037 | FK integrity on create | Create | FK valid | P0 |
| FUN-038 | FK integrity on update | Update | FK valid | P0 |
| FUN-039 | FK integrity on delete | Delete | Cascade or block | P0 |
| FUN-040 | Audit trail consistency | Operation | Audit correct | P0 |
| FUN-041 | Workflow state consistency | Transition | State correct | P0 |
| FUN-042 | Notification consistency | Create | Delivered | P0 |
| FUN-043 | Cache invalidation on update | Update | Cache invalidated | P1 |
| FUN-044 | Cache invalidation on delete | Delete | Cache invalidated | P1 |
| FUN-045 | Orphan cleanup | Orphans | Cleanup or prevent | P0 |

### 4.4 Workflow State (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-046 | Workflow state preserved through system restart | Restart | State preserved | P0 |
| FUN-047 | Workflow state transitions | All transitions | Valid | P0 |
| FUN-048 | Workflow state validation | Invalid transition | Rejected | P0 |
| FUN-049 | Workflow state permission | Permission | Checked | P0 |
| FUN-050 | Workflow state notification | Transition | Notification | P0 |
| FUN-051 | Workflow state audit | Transition | Audit | P0 |
| FUN-052 | Workflow state lock | During transition | Locked | P1 |
| FUN-053 | Workflow state unlock | After transition | Unlocked | P1 |
| FUN-054 | Workflow state rollback | Rollback | State restored | P1 |
| FUN-055 | Workflow state history | History | Preserved | P1 |
| FUN-056 | Workflow state display | Display | Correct label | P0 |
| FUN-057 | Workflow state requirements | Requirements | Checked | P0 |
| FUN-058 | Workflow state DoA2 | DoA2 | Validated | P0 |
| FUN-059 | Workflow state concurrent | Concurrent | One wins | P0 |
| FUN-060 | Workflow state final | Final state | No further | P0 |

### 4.5 Entity Chain Operations (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-061 | Partner → Contact → Interaction chain | Full chain | All linked | P0 |
| FUN-062 | Opportunity → Country → EntityArtifact → Tags chain | Full chain | All linked | P0 |
| FUN-063 | Workflow → Notification → Actions Required → Email chain | Full chain | All linked | P0 |
| FUN-064 | Gmail → Contact matching → Interaction creation → Partner update | Full chain | All updated | P1 |
| FUN-065 | Document → Entity | Document | Linked | P0 |
| FUN-066 | Link → Entity | Link | Linked | P0 |
| FUN-067 | Contact → Partner | Contact | Linked | P0 |
| FUN-068 | Interaction → Contact | Interaction | Linked | P0 |
| FUN-069 | Opportunity → Partner | Opportunity | Linked | P0 |
| FUN-070 | Organization → OrgUnit | Org | Linked | P0 |
| FUN-071 | Chain delete | Delete parent | Cascade | P0 |
| FUN-072 | Chain update | Update parent | Children consistent | P0 |
| FUN-073 | Chain filter | Filter | Filtered | P0 |
| FUN-074 | Chain pagination | Paginate | Correct | P0 |
| FUN-075 | Chain sort | Sort | Ordered | P0 |

### 4.6 Business Rules (15)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| FUN-076 | Partner name uniqueness | Duplicate name | Rejected | P0 |
| FUN-077 | Contact email uniqueness per partner | Duplicate email | Rejected | P0 |
| FUN-078 | Interaction date range | End >= Start | Validated | P0 |
| FUN-079 | Document type per entity | Valid type | Validated | P0 |
| FUN-080 | Link URL format | Valid URL | Validated | P0 |
| FUN-081 | Opportunity budget > 0 | Budget | Validated | P0 |
| FUN-082 | Org hierarchy no cycle | Cycle | Rejected | P0 |
| FUN-083 | Workflow requirements | 21 requirements | Validated | P0 |
| FUN-084 | DoA2 for submit | DoA2 | Required | P0 |
| FUN-085 | Permission per action | Action | Checked | P0 |
| FUN-086 | Org scope | User org | Filtered | P0 |
| FUN-087 | Soft-delete filter | Query | Excluded | P0 |
| FUN-088 | Audit on create | Create | Audit | P0 |
| FUN-089 | Audit on update | Update | Audit | P0 |
| FUN-090 | Audit on delete | Delete | Audit | P0 |

---

## §5 Integration Tests (90)

### 5.1 Partner → Contact → Interaction → Document Chain (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-001 | Partner create | Create | Partner created | P0 |
| INT-002 | Contact create with Partner | Create | Contact linked | P0 |
| INT-003 | Interaction create with Contact | Create | Interaction linked | P0 |
| INT-004 | Document create with Partner | Create | Document linked | P0 |
| INT-005 | Get Partner with all | Get | All loaded | P0 |
| INT-006 | Update Partner | Update | Updated | P0 |
| INT-007 | Update Contact | Update | Updated | P0 |
| INT-008 | Update Interaction | Update | Updated | P0 |
| INT-009 | Soft-delete Partner | Delete | Partner deleted | P0 |
| INT-010 | List Contacts by Partner | List | Filtered | P0 |
| INT-011 | List Interactions by Contact | List | Filtered | P0 |
| INT-012 | List Documents by Partner | List | Filtered | P0 |
| INT-013 | Search across chain | Search | Results | P0 |
| INT-014 | Export chain | Export | Exported | P0 |
| INT-015 | Import chain | Import | Imported | P0 |

### 5.2 Opportunity → Country → EntityArtifact → Tags Chain (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-016 | Opportunity create | Create | Created | P0 |
| INT-017 | Country add to Opportunity | Add | Linked | P0 |
| INT-018 | EntityArtifact add | Add | Linked | P0 |
| INT-019 | Tags add | Add | Linked | P0 |
| INT-020 | Get Opportunity with all | Get | All loaded | P0 |
| INT-021 | Update Opportunity | Update | Updated | P0 |
| INT-022 | Update Country | Update | Updated | P0 |
| INT-023 | Remove Country | Remove | Unlinked | P0 |
| INT-024 | Workflow submit | Submit | Submitted | P0 |
| INT-025 | Workflow approve | Approve | Approved | P0 |
| INT-026 | Workflow GO | GO | GO | P0 |
| INT-027 | List by Country | List | Filtered | P0 |
| INT-028 | List by Tag | List | Filtered | P0 |
| INT-029 | Search Opportunity | Search | Results | P0 |
| INT-030 | Export Opportunity | Export | Exported | P0 |

### 5.3 Workflow → Notification → Actions Required → Email Chain (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-031 | Workflow transition | Transition | Notification created | P0 |
| INT-032 | Notification create | Create | Created | P0 |
| INT-033 | Actions Required create | Create | Created | P0 |
| INT-034 | Email send | Send | Sent | P0 |
| INT-035 | Notification mark read | Mark read | Read | P0 |
| INT-036 | Actions Required complete | Complete | Completed | P0 |
| INT-037 | Get notifications for user | Get | Filtered | P0 |
| INT-038 | Get unread count | Count | Count | P0 |
| INT-039 | Bulk mark read | Bulk | All read | P0 |
| INT-040 | Notification for workflow | Create | Linked | P0 |
| INT-041 | Notification for entity | Create | Linked | P0 |
| INT-042 | Notification cleanup | Cleanup | Cleaned | P1 |
| INT-043 | Email template | Template | Rendered | P1 |
| INT-044 | Email delivery | Send | Delivered | P1 |
| INT-045 | Notification permission | Permission | Checked | P0 |

### 5.4 Gmail → Contact → Interaction → Partner Chain (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-046 | Gmail receive email | Receive | Received | P0 |
| INT-047 | Contact matching | Match | Contact found | P0 |
| INT-048 | Interaction creation | Create | Created | P0 |
| INT-049 | Partner update | Update | Updated | P0 |
| INT-050 | No contact match | No match | Create or skip | P0 |
| INT-051 | Multiple contact match | Multiple | Best match | P0 |
| INT-052 | New contact from email | Create | Created | P0 |
| INT-053 | Link email to interaction | Link | Linked | P0 |
| INT-054 | Partner from email domain | Domain | Partner found | P1 |
| INT-055 | Interaction update from email | Update | Updated | P1 |
| INT-056 | Attachment to document | Attach | Document created | P1 |
| INT-057 | Email thread | Thread | Linked | P1 |
| INT-058 | Email permission | Permission | Checked | P0 |
| INT-059 | Email rate limit | Limit | Throttled | P1 |
| INT-060 | Email error handling | Error | Handled | P0 |

### 5.5 Cross-Manager Integration (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-061 | PartnerManager → ContactManager | Create contact | Contact created | P0 |
| INT-062 | ContactManager → InteractionManager | Create interaction | Interaction created | P0 |
| INT-063 | DocumentManager → PartnerManager | Add document | Document linked | P0 |
| INT-064 | LinkManager → PartnerManager | Add link | Link linked | P0 |
| INT-065 | OpportunityManager → PartnerManager | Link opportunity | Linked | P0 |
| INT-066 | WorkflowManager → NotificationManager | Transition | Notification | P0 |
| INT-067 | OrganizationHierarchyManager → PartnerManager | Assign org | Assigned | P0 |
| INT-068 | ValuesManager → OpportunityManager | Budget | Currency | P0 |
| INT-069 | EntityConfigurationManager → DocumentManager | Doc type | Validated | P0 |
| INT-070 | ProfileManager → all | User profile | Applied | P1 |
| INT-071 | PermissionService → all | Permission | Checked | P0 |
| INT-072 | AuditTrail → all | Audit | Logged | P0 |
| INT-073 | ManagerWrapper → all | All managers | Resolved | P0 |
| INT-074 | Transaction across managers | Transaction | Atomic | P0 |
| INT-075 | Rollback across managers | Rollback | All rolled back | P0 |

### 5.6 End-to-End Flows (15)

| ID | Test Name | Flow | Expected | Priority |
|----|-----------|------|----------|----------|
| INT-076 | Full partner lifecycle | Create → contacts → docs → links → org → delete | Success | P0 |
| INT-077 | Full opportunity lifecycle | Create → profile → submit → approve → GO | Success | P0 |
| INT-078 | Full contact lifecycle | Create → interactions → update → delete | Success | P0 |
| INT-079 | Full workflow lifecycle | Draft → Submit → Approve → GO | Success | P0 |
| INT-080 | Full document lifecycle | Upload → link → update → delete | Success | P0 |
| INT-081 | Full interaction lifecycle | Create → update → delete | Success | P0 |
| INT-082 | Full link lifecycle | Create → update → delete | Success | P0 |
| INT-083 | Full notification lifecycle | Create → read → cleanup | Success | P0 |
| INT-084 | Full org hierarchy | Create → assign → update | Success | P0 |
| INT-085 | Import → validate → create | Import | All created | P0 |
| INT-086 | Export → filter → format | Export | Exported | P0 |
| INT-087 | Search → filter → paginate | Search | Results | P0 |
| INT-088 | Bulk create | Bulk | All created | P0 |
| INT-089 | Bulk update | Bulk | All updated | P0 |
| INT-090 | Bulk delete | Bulk | All deleted | P0 |

---

## §7 Concurrency Tests (25)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| CON-001 | 2 users create partner | Concurrent | Both succeed | P0 |
| CON-002 | 2 users update same partner | Concurrent | One or both | P0 |
| CON-003 | 2 users add contact to same partner | Concurrent | Both succeed | P0 |
| CON-004 | 2 users submit same opportunity | Concurrent | One succeeds | P0 |
| CON-005 | 2 users workflow transition | Concurrent | One succeeds | P0 |
| CON-006 | 2 users soft-delete same entity | Concurrent | One succeeds | P0 |
| CON-007 | 10 users create partners | Concurrent | All succeed | P0 |
| CON-008 | 10 users read same partner | Concurrent | All succeed | P0 |
| CON-009 | Update during delete | Concurrent | Handled | P0 |
| CON-010 | Delete during update | Concurrent | Handled | P0 |
| CON-011 | Workflow during edit | Concurrent | Handled | P0 |
| CON-012 | Import during export | Concurrent | Both succeed | P1 |
| CON-013 | Bulk create concurrent | Concurrent | All succeed | P1 |
| CON-014 | Transaction isolation | Parallel | Isolated | P0 |
| CON-015 | Connection pool | Many concurrent | Pool holds | P1 |
| CON-016 | Deadlock | Circular | Timeout or avoid | P1 |
| CON-017 | Optimistic lock | Stale update | ConcurrencyException | P0 |
| CON-018 | Idempotency | Same request 2x | Same result | P1 |
| CON-019 | Cache concurrent | Read/write | Consistent | P1 |
| CON-020 | Notification concurrent | 10 create | All created | P1 |
| CON-021 | 50 concurrent reads | 50 parallel | All succeed | P1 |
| CON-022 | 20 concurrent writes | 20 parallel | All succeed | P1 |
| CON-023 | Mixed concurrent | 10 read, 10 write | All succeed | P1 |
| CON-024 | Stress concurrent | Ramp | Until limit | P2 |
| CON-025 | Recovery after concurrent | Stop | Recovery | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected | Priority |
|----|-----------|----------|-------|----------|----------|
| UNT-001 | Partner name validation | Validation | Name | Valid | P1 |
| UNT-002 | Contact email validation | Validation | Email | Valid | P1 |
| UNT-003 | Link URL validation | Validation | URL | Valid | P1 |
| UNT-004 | Interaction date range | Validation | Dates | Valid | P1 |
| UNT-005 | Opportunity budget validation | Validation | Budget | Valid | P1 |
| UNT-006 | Org hierarchy validation | Validation | Hierarchy | Valid | P1 |
| UNT-007 | Workflow transition validation | Validation | Transition | Valid | P1 |
| UNT-008 | Pagination calculation | Calculation | Page, size | Skip, take | P1 |
| UNT-009 | Total pages calculation | Calculation | Total, size | Pages | P1 |
| UNT-010 | Filter construction | Query | Filter | Expression | P1 |
| UNT-011 | Sort expression | Query | Sort | Expression | P1 |
| UNT-012 | IsDeleted filter | Query | Query | Filter | P1 |
| UNT-013 | Audit field population | Audit | Create | CreatedBy | P1 |
| UNT-014 | Soft-delete flag | SoftDelete | Delete | IsDeleted | P1 |
| UNT-015 | Workflow state check | State | State | Valid | P1 |
| UNT-016 | Entity type mapping | Mapping | Type | Mapped | P1 |
| UNT-017 | FK validation | Validation | FK | Valid | P1 |
| UNT-018 | Name uniqueness check | Validation | Name | Duplicate | P1 |
| UNT-019 | Email uniqueness check | Validation | Email | Duplicate | P1 |
| UNT-020 | Requirement count | Calculation | Opportunity | 21 | P1 |
| UNT-021 | Notification format | Formatting | Notification | Formatted | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|-----------|-----------|----------|
| PRF-001 | Partner get with all | Get | < 500 ms | P0 |
| PRF-002 | Partner list 1000 | List | < 2 s | P0 |
| PRF-003 | Opportunity get with all | Get | < 1 s | P0 |
| PRF-004 | Opportunity list 1000 | List | < 2 s | P0 |
| PRF-005 | Contact list by partner | List | < 300 ms | P0 |
| PRF-006 | Interaction list by contact | List | < 300 ms | P0 |
| PRF-007 | Document list by entity | List | < 300 ms | P0 |
| PRF-008 | Workflow transition | Transition | < 500 ms | P0 |
| PRF-009 | Notification list | List | < 300 ms | P0 |
| PRF-010 | Search across entities | Search | < 2 s | P0 |
| PRF-011 | No N+1 partner with contacts | Get | Single query | P0 |
| PRF-012 | No N+1 opportunity with all | Get | Single or split | P0 |
| PRF-013 | Bulk create 100 | Bulk | < 10 s | P1 |
| PRF-014 | Export 1000 | Export | < 30 s | P1 |
| PRF-015 | Import 100 | Import | < 30 s | P1 |
| PRF-016 | 20 concurrent operations | 20 parallel | < 10 s | P1 |

---

## §10 Load Tests (10)

| ID | Test Name | Load Profile | Duration | Success Criteria | Priority |
|----|-----------|-------------|----------|-------------------|----------|
| LDT-001 | 50 partner reads/min | 50/min | 10 min | 99% success | P1 |
| LDT-002 | 20 partner creates/min | 20/min | 10 min | 99% success | P1 |
| LDT-003 | 30 opportunity reads/min | 30/min | 10 min | 99% success | P1 |
| LDT-004 | 10 workflow transitions/min | 10/min | 10 min | 99% success | P1 |
| LDT-005 | Spike 100 reads | 0→100→0 | 2 min | Graceful | P1 |
| LDT-006 | Spike 50 creates | 0→50→0 | 2 min | Graceful | P2 |
| LDT-007 | Stress partner list | Ramp | Until fail | Document limit | P2 |
| LDT-008 | Stress workflow | 20 concurrent | 5 min | No errors | P2 |
| LDT-009 | Recovery after spike | Spike then normal | 5 min | Recovery | P1 |
| LDT-010 | Recovery after stress | Stress then stop | 10 min | Full recovery | P2 |

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
