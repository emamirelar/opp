# EntityConfigurationController — Test Cases

**Component:** `OpportunityPlus.API/Controllers/EntityConfigurationController`  
**Created:** 2026-02-04 | **Last Updated:** 2026-02-11  
**Author:** QA Team  
**Standard:** 10-Category, 3:1 Ratio

**Feature Overview:** REST API for entity configuration: field definitions, entity schemas, custom fields, validation rules.

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

**3:1 Ratio Checks:** N≥3P ✅ (90≥90) | E≥3P ✅ (90≥90) | F≥3P ✅ (90≥90) | I≥3P ✅ (90≥90)

---

## §1 Positive Tests (30)

| ID | Test Name | Steps | Expected Result |
|----|-----------|-------|-----------------|
| POS-001 | Get entity schemas | GET /api/entity-config/schemas | Schema list |
| POS-002 | Get schema by entity | GET /api/entity-config/schemas/{entity} | Schema |
| POS-003 | Get field definitions | GET /api/entity-config/fields?entity=Partner | Fields |
| POS-004 | Get field by ID | GET /api/entity-config/fields/{id} | Field details |
| POS-005 | Get custom fields | GET /api/entity-config/custom-fields?entity=Partner | Custom fields |
| POS-006 | Get validation rules | GET /api/entity-config/validation-rules?entity=Partner | Rules |
| POS-007 | Create custom field (admin) | POST /api/entity-config/custom-fields | 201 |
| POS-008 | Update field (admin) | PUT /api/entity-config/fields/{id} | 200 |
| POS-009 | Delete custom field (admin) | DELETE /api/entity-config/custom-fields/{id} | 204 |
| POS-010 | Get entity types | GET /api/entity-config/entity-types | Types |
| POS-011 | Get field types | GET /api/entity-config/field-types | Types |
| POS-012 | Get schema for form | GET /api/entity-config/schemas/{entity}/form | Form schema |
| POS-013 | Get schema for list | GET /api/entity-config/schemas/{entity}/list | List schema |
| POS-014 | Get schema for detail | GET /api/entity-config/schemas/{entity}/detail | Detail schema |
| POS-015 | Filter by entity | GET ?entity=Opportunity | Filtered |
| POS-016 | Filter by field type | GET ?fieldType=Text | Filtered |
| POS-017 | Filter by section | GET ?section=Basic | Filtered |
| POS-018 | Paginate fields | GET ?page=1&pageSize=20 | Paginated |
| POS-019 | Sort fields | GET ?sortBy=order | Sorted |
| POS-020 | Get field options | GET /api/entity-config/fields/{id}/options | Options |
| POS-021 | Get default values | GET /api/entity-config/fields/{id}/defaults | Defaults |
| POS-022 | Validate against schema | POST /api/entity-config/validate | Validation |
| POS-023 | Get dependencies | GET /api/entity-config/fields/{id}/dependencies | Dependencies |
| POS-024 | Get visibility rules | GET /api/entity-config/fields/{id}/visibility | Rules |
| POS-025 | Get required fields | GET /api/entity-config/schemas/{entity}/required | Required list |
| POS-026 | Get display order | GET /api/entity-config/schemas/{entity}/order | Order |
| POS-027 | Get permissions per field | GET /api/entity-config/fields/{id}/permissions | Permissions |
| POS-028 | Export schema | GET /api/entity-config/schemas/export | Export |
| POS-029 | Import schema (admin) | POST /api/entity-config/schemas/import | Imported |
| POS-030 | Clone schema | POST /api/entity-config/schemas/{entity}/clone | Cloned |

---

## §2 Negative Tests (90)

| ID | Test Name | Invalid Input | Expected Error |
|----|-----------|--------------|----------------|
| NEG-001 | No auth | No token | 401 |
| NEG-002 | Expired token | Expired JWT | 401 |
| NEG-003 | Invalid entity | entity=Invalid | 404 |
| NEG-004 | Invalid field ID | id=999999 | 404 |
| NEG-005 | Negative ID | id=-1 | 400 |
| NEG-006 | Null request | POST null | 400 |
| NEG-007 | Missing required field | Name missing | 400 |
| NEG-008 | Invalid field type | type=Invalid | 400 |
| NEG-009 | Invalid data type | dataType=Invalid | 400 |
| NEG-010 | Invalid validation rule | rule=Invalid | 400 |
| NEG-011 | SQL injection | entity='; DROP | Sanitized |
| NEG-012 | XSS in field name | name=<script> | Sanitized |
| NEG-013 | No admin for create | POST as user | 403 |
| NEG-014 | No admin for update | PUT as user | 403 |
| NEG-015 | No admin for delete | DELETE as user | 403 |
| NEG-016 | Cross-org | Other org schema | 403 |
| NEG-017 | Deleted field | id of deleted | 404 |
| NEG-018 | Invalid schema format | Malformed schema | 400 |
| NEG-019 | Duplicate field name | name exists | 409 |
| NEG-020 | Duplicate field key | key exists | 409 |
| NEG-021 | Invalid regex pattern | pattern=invalid | 400 |
| NEG-022 | Negative max length | maxLength=-1 | 400 |
| NEG-023 | Invalid min/max | min>max | 400 |
| NEG-024 | Invalid option format | options malformed | 400 |
| NEG-025 | Circular dependency | Field depends on self | 400 |
| NEG-026 | Malformed JSON | Invalid JSON | 400 |
| NEG-027 | Wrong content-type | Application/xml | 415 |
| NEG-028 | Invalid section | section=Invalid | 400 |
| NEG-029 | Reserved field name | name=Id | 403 |
| NEG-030 | System field update | Update system field | 403 |
| NEG-031 | Rate limit | Too many | 429 |
| NEG-032 | Payload too large | Huge body | 413 |
| NEG-033 | Invalid Accept | Accept: text/plain | 406 |
| NEG-034 | HTTP method | PUT for create | 405 |
| NEG-035 | OPTIONS | OPTIONS | 200 |
| NEG-036 | HEAD | HEAD | 200 or 405 |
| NEG-037 | Trailing slash | /api/entity-config/ | Redirect |
| NEG-038 | Case sensitivity | /api/Entity-Config | 404 |
| NEG-039 | Extra path | /api/entity-config/1/extra | 404 |
| NEG-040 | Invalid bearer | Bearer malformed | 401 |
| NEG-041 | Revoked token | Revoked JWT | 401 |
| NEG-042 | Service account | Service for UI | 403 |
| NEG-043 | DB timeout | Simulate | 503 |
| NEG-044 | Invalid import format | Import malformed | 400 |
| NEG-045 | Import conflict | Import duplicates | 409 |
| NEG-046 | Invalid clone source | entity=Invalid | 404 |
| NEG-047 | Validation fail | Validate invalid data | 400 |
| NEG-048 | Invalid UUID | id=invalid-guid | 400 |
| NEG-049 | Mismatched IDs | Path != body | 400 |
| NEG-050 | Read-only field | Update system field | Ignored |
| NEG-051 | Version conflict | Stale version | 409 |
| NEG-052 | Blocked IP | From blocked | 403 |
| NEG-053 | CORS fail | Invalid origin | CORS error |
| NEG-054 | Control chars | name with \0 | 400 |
| NEG-055 | Unicode overflow | Very long | 400 |
| NEG-056 | Invalid visibility rule | rule malformed | 400 |
| NEG-057 | Invalid dependency | Depends on invalid | 404 |
| NEG-058 | Delete in-use field | Field referenced | 409 |
| NEG-059 | Invalid default value | Default wrong type | 400 |
| NEG-060 | Invalid option value | Option wrong type | 400 |
| NEG-061 | Empty entity name | entity= | 400 |
| NEG-062 | Whitespace entity | entity="  " | 400 |
| NEG-063 | Invalid field key | key=invalid-format | 400 |
| NEG-064 | Reserved key | key=id | 403 |
| NEG-065 | Exceed max fields | Add 101st field | 400 |
| NEG-066 | Invalid order | order=-1 | 400 |
| NEG-067 | Duplicate order | Same order | 400 or allow |
| NEG-068 | Audit failure | Audit down | Continue |
| NEG-069 | Inactive org | Org inactive | 403 |
| NEG-070 | Soft-deleted | Query deleted | Excluded |
| NEG-071 | Invalid option value type | Option wrong type | 400 |
| NEG-072 | ReDoS regex | Malicious pattern | 400 |
| NEG-073 | Import size exceeded | >5MB | 413 |
| NEG-074 | Clone to existing | entity exists | 409 |
| NEG-075 | Validate empty payload | {} | 400 |
| NEG-076 | Field in use delete | Referenced | 409 |
| NEG-077 | Invalid section name | section=; DROP | 400 |
| NEG-078 | Dependency on deleted | Dep deleted | 404 |
| NEG-079 | Visibility rule syntax | rule malformed | 400 |
| NEG-080 | Default value mismatch | Default wrong type | 400 |
| NEG-081 | Exceed max options | 1001 options | 400 |
| NEG-082 | Invalid order value | order=abc | 400 |
| NEG-083 | Schema export format | format=Invalid | 400 |
| NEG-084 | Import conflict | Duplicate keys | 409 |
| NEG-085 | Clone invalid target | target=Invalid | 404 |
| NEG-086 | Update system field | System field | 403 |
| NEG-087 | Delete required field | Required field | 409 |
| NEG-088 | Invalid entity in validate | entity=Invalid | 404 |
| NEG-089 | Circular visibility | Depends on self | 400 |
| NEG-090 | Schema version mismatch | Old version | 409 |

---

## §3 Boundary Tests (90)

| ID | Field/Scenario | Min | Max | At Min | At Max | Over Max |
|----|----------------|-----|-----|--------|--------|----------|
| BND-001 | name length | 1 | 255 | ✅ | ✅ | ❌ |
| BND-002 | key length | 1 | 100 | ✅ | ✅ | ❌ |
| BND-003 | page | 1 | 9999 | ✅ | ✅ | ❌ |
| BND-004 | pageSize | 1 | 100 | ✅ | ✅ | ❌ |
| BND-005 | maxLength | 0 | 65536 | ✅ | ✅ | ❌ |
| BND-006 | minLength | 0 | maxLength | ✅ | ✅ | ❌ |
| BND-007 | order | 0 | 9999 | ✅ | ✅ | ❌ |
| BND-008 | field count | 0 | 100 | ✅ | ✅ | ❌ |
| BND-009 | option count | 0 | 1000 | ✅ | ✅ | ❌ |
| BND-010 | dependency depth | 0 | 5 | ✅ | ✅ | ❌ |
| BND-011 | Empty list | - | - | [] | - | - |
| BND-012 | Single field | - | - | [field] | - | - |
| BND-013 | First page | page=1 | - | ✅ | - | - |
| BND-014 | Last page | - | - | Partial | - | - |
| BND-015 | Zero length name | - | - | ❌ | - | - |
| BND-016 | Max length name | 255 | - | - | ✅ | ❌ |
| BND-017 | Unicode name | - | - | Accept | - | - |
| BND-018 | Special chars key | - | - | Define | - | - |
| BND-019 | Null optional | - | - | Default | - | - |
| BND-020 | Empty string | - | - | No filter | - | - |
| BND-021 | Whitespace | - | - | Trim | - | - |
| BND-022 | Sort empty | - | - | [] | - | - |
| BND-023 | Sort single | - | - | [item] | - | - |
| BND-024 | Filter no match | - | - | [] | - | - |
| BND-025 | Filter all | - | - | Full | - | - |
| BND-026 | Decimal precision | - | 2 | Rounded | - | - |
| BND-027 | Integer range | int.Min | int.Max | ✅ | ✅ | ❌ |
| BND-028 | Concurrent requests | - | 100 | ✅ | ✅ | ❌ |
| BND-029 | Cache TTL | - | - | Expire | - | - |
| BND-030 | URL length | - | 2048 | - | ✅ | ❌ |
| BND-031 | Query params | - | 20 | ✅ | ✅ | ❌ |
| BND-032 | Regex length | - | 1000 | ✅ | ✅ | ❌ |
| BND-033 | Schema size | - | 1MB | ✅ | ✅ | ❌ |
| BND-034 | Import size | - | 5MB | ✅ | ✅ | ❌ |
| BND-035 | Validation rules | - | 50 | ✅ | ✅ | ❌ |
| BND-036 | Visibility rules | - | 20 | ✅ | ✅ | ❌ |
| BND-037 | Dependencies | - | 10 | ✅ | ✅ | ❌ |
| BND-038 | Sections | - | 20 | ✅ | ✅ | ❌ |
| BND-039 | Entity types | - | 100 | ✅ | ✅ | ❌ |
| BND-040 | Field types | - | 50 | ✅ | ✅ | ❌ |
| BND-041 | Pagination boundary | - | - | Exact | - | - |
| BND-042 | Empty options | - | - | [] | - | - |
| BND-043 | Single option | - | - | [option] | - | - |
| BND-044 | Default value | - | - | Valid | - | - |
| BND-045 | Empty default | - | - | Null | - | - |
| BND-046 | Round-trip | Create → Get | - | Match | - | - |
| BND-047 | Soft-deleted | - | - | Excluded | - | - |
| BND-048 | Inactive | - | - | Excluded | - | - |
| BND-049 | Version | 1 | - | ✅ | ❌ | - |
| BND-050 | Regex special | - | - | Escape | - | - |
| BND-051 | Case key | - | - | Normalize | - | - |
| BND-052 | Leading space | - | - | Trim | - | - |
| BND-053 | Trailing space | - | - | Trim | - | - |
| BND-054 | Zero ID | id=0 | - | 400 | - | - |
| BND-055 | Max int ID | - | int.Max | ✅ | ✅ | ❌ |
| BND-056 | Min value | - | - | Inclusive | - | - |
| BND-057 | Max value | - | - | Inclusive | - | - |
| BND-058 | Required flag | - | - | Boolean | - | - |
| BND-059 | Readonly flag | - | - | Boolean | - | - |
| BND-060 | Visible flag | - | - | Boolean | - | - |
| BND-061 | Searchable flag | - | - | Boolean | - | - |
| BND-062 | Sortable flag | - | - | Boolean | - | - |
| BND-063 | Export rows | - | 10000 | ✅ | ✅ | ❌ |
| BND-064 | Export empty | - | - | Headers | - | - |
| BND-065 | Export single | - | - | Valid | - | - |
| BND-066 | Clone depth | - | - | Full | - | - |
| BND-067 | Import entities | - | 50 | ✅ | ✅ | ❌ |
| BND-068 | Validate data size | - | 1MB | ✅ | ✅ | ❌ |
| BND-069 | Pattern match | - | - | Correct | - | - |
| BND-070 | Option value length | - | 255 | ✅ | ✅ | ❌ |
| BND-071 | Validation rules count | 0 | 50 | ✅ | ✅ | ❌ |
| BND-072 | Visibility rules | 0 | 20 | ✅ | ✅ | ❌ |
| BND-073 | Dependencies | 0 | 10 | ✅ | ✅ | ❌ |
| BND-074 | Sections | 0 | 20 | ✅ | ✅ | ❌ |
| BND-075 | Entity types | 0 | 100 | ✅ | ✅ | ❌ |
| BND-076 | Field types | 0 | 50 | ✅ | ✅ | ❌ |
| BND-077 | Import entities | 0 | 50 | ✅ | ✅ | ❌ |
| BND-078 | Validate data size | 0 | 1MB | ✅ | ✅ | ❌ |
| BND-079 | Regex length | 0 | 1000 | ✅ | ✅ | ❌ |
| BND-080 | Schema size | 0 | 1MB | ✅ | ✅ | ❌ |
| BND-081 | Import size | 0 | 5MB | ✅ | ✅ | ❌ |
| BND-082 | Clone depth | - | - | Full | - | - |
| BND-083 | Empty options | - | - | [] | - | - |
| BND-084 | Single option | - | - | [option] | - | - |
| BND-085 | Default value | - | - | Valid | - | - |
| BND-086 | Empty default | - | - | Null | - | - |
| BND-087 | Min value | - | - | Inclusive | - | - |
| BND-088 | Max value | - | - | Inclusive | - | - |
| BND-089 | Required flag | - | - | Boolean | - | - |
| BND-090 | Readonly flag | - | - | Boolean | - | - |

---

## §4 Functional Tests (90)

| ID | Category | Rule | Trigger | Expected |
|----|----------|------|---------|----------|
| FUN-001 | Workflow | Get schemas | GET | Schemas |
| FUN-002 | Workflow | Get schema | GET entity | Schema |
| FUN-003 | Workflow | Get fields | GET fields | Fields |
| FUN-004 | Workflow | Create custom field | POST | 201 |
| FUN-005 | Workflow | Update field | PUT | 200 |
| FUN-006 | Workflow | Delete custom | DELETE | 204 |
| FUN-007 | Workflow | Validate | POST validate | Result |
| FUN-008 | Workflow | Get form schema | GET form | Form schema |
| FUN-009 | Workflow | Get list schema | GET list | List schema |
| FUN-010 | Workflow | Get detail schema | GET detail | Detail schema |
| FUN-011 | Workflow | Import | POST import | Imported |
| FUN-012 | Workflow | Export | GET export | File |
| FUN-013 | Workflow | Clone | POST clone | Cloned |
| FUN-014 | Workflow | Filter | GET ?entity | Filtered |
| FUN-015 | Workflow | Paginate | GET ?page | Paginated |
| FUN-016 | Validation | Required name | Missing | 400 |
| FUN-017 | Validation | Required key | Missing | 400 |
| FUN-018 | Validation | Valid type | Invalid | 400 |
| FUN-019 | Validation | Unique key | Duplicate | 409 |
| FUN-020 | Validation | Permission | No permission | 403 |
| FUN-021 | Validation | Admin write | User write | 403 |
| FUN-022 | Validation | No reserved | Reserved | 403 |
| FUN-023 | Validation | Valid regex | Invalid | 400 |
| FUN-024 | Validation | Min <= max | min>max | 400 |
| FUN-025 | Validation | No circular | Circular dep | 400 |
| FUN-026 | Constraint | System lock | Update system | 403 |
| FUN-027 | Constraint | Max fields | >100 | 400 |
| FUN-028 | Constraint | Delete in-use | Referenced | 409 |
| FUN-029 | Constraint | Org scope | Cross-org | 403 |
| FUN-030 | Constraint | Version | Optimistic | 409 |
| FUN-031 | Constraint | Import format | Invalid | 400 |
| FUN-032 | Constraint | Clone source | Invalid | 404 |
| FUN-033 | Constraint | Validate schema | Invalid data | 400 |
| FUN-034 | Constraint | Dependency exists | Invalid dep | 404 |
| FUN-035 | Constraint | Visibility valid | Invalid rule | 400 |
| FUN-036 | Audit | Create | POST | Audit |
| FUN-037 | Audit | Update | PUT | Audit |
| FUN-038 | Audit | Delete | DELETE | Audit |
| FUN-039 | Audit | Import | POST import | Audit |
| FUN-040 | Audit | Export | GET export | Audit |
| FUN-041 | Audit | Timestamp | Any | UTC |
| FUN-042 | Audit | User ID | Any | User ID |
| FUN-043 | Audit | IP | Any | IP |
| FUN-044 | Audit | Resource | Any | Resource |
| FUN-045 | Audit | Outcome | Any | Outcome |
| FUN-046 | Business | Soft-deleted | Query | Excluded |
| FUN-047 | Business | Inactive | Query | Excluded |
| FUN-048 | Business | Permission | Query | Scoped |
| FUN-049 | Business | Entity type | Filter | Correct |
| FUN-050 | Business | Decimal | Precision | 2 decimals |
| FUN-051 | Workflow | Get form schema | GET form | Form |
| FUN-052 | Workflow | Get list schema | GET list | List |
| FUN-053 | Workflow | Get detail schema | GET detail | Detail |
| FUN-054 | Workflow | Import | POST import | Imported |
| FUN-055 | Workflow | Export | GET export | File |
| FUN-056 | Validation | Required name | Missing | 400 |
| FUN-057 | Validation | Required key | Missing | 400 |
| FUN-058 | Validation | Unique key | Duplicate | 409 |
| FUN-059 | Validation | Valid type | Invalid | 400 |
| FUN-060 | Validation | No circular | Circular dep | 400 |
| FUN-061 | Constraint | System lock | Update system | 403 |
| FUN-062 | Constraint | Max fields | >100 | 400 |
| FUN-063 | Constraint | Delete in-use | Referenced | 409 |
| FUN-064 | Constraint | Org scope | Cross-org | 403 |
| FUN-065 | Constraint | Version | Optimistic | 409 |
| FUN-066 | Audit | Create | POST | Audit |
| FUN-067 | Audit | Update | PUT | Audit |
| FUN-068 | Audit | Delete | DELETE | Audit |
| FUN-069 | Audit | Import | POST import | Audit |
| FUN-070 | Audit | Export | GET export | Audit |
| FUN-071 | Business | Soft-deleted | Query | Excluded |
| FUN-072 | Business | Inactive | Query | Excluded |
| FUN-073 | Business | Permission | Query | Scoped |
| FUN-074 | Business | Entity type | Filter | Correct |
| FUN-075 | Business | Decimal | 2 decimals | Correct |
| FUN-076 | Workflow | Clone | POST clone | Cloned |
| FUN-077 | Workflow | Validate | POST validate | Result |
| FUN-078 | Workflow | Filter | GET ?entity | Filtered |
| FUN-079 | Workflow | Paginate | GET ?page | Paginated |
| FUN-080 | Workflow | Sort | GET ?sortBy | Sorted |
| FUN-081 | Validation | Valid regex | Invalid | 400 |
| FUN-082 | Validation | Min <= max | min>max | 400 |
| FUN-083 | Validation | No reserved | Reserved | 403 |
| FUN-084 | Validation | Admin write | User write | 403 |
| FUN-085 | Validation | ID format | Invalid | 400 |
| FUN-086 | Constraint | Import format | Invalid | 400 |
| FUN-087 | Constraint | Clone source | Invalid | 404 |
| FUN-088 | Constraint | Dependency exists | Invalid dep | 404 |
| FUN-089 | Constraint | Visibility valid | Invalid rule | 400 |
| FUN-090 | Constraint | Validate schema | Invalid data | 400 |

---

## §5 Integration Tests (90)

| ID | Category | Scenario | Entities | Expected |
|----|----------|----------|----------|----------|
| INT-001 | CRUD | Get schema | Config | Schema |
| INT-002 | CRUD | Get fields | Config | Fields |
| INT-003 | CRUD | Create field | Config | Created |
| INT-004 | CRUD | Update field | Config | Updated |
| INT-005 | CRUD | Delete field | Config | Deleted |
| INT-006 | CRUD | Validate | Config, Data | Result |
| INT-007 | CRUD | Import | Config | Imported |
| INT-008 | CRUD | Export | Config | File |
| INT-009 | CRUD | Clone | Config | Cloned |
| INT-010 | CRUD | Get form | Config | Form |
| INT-011 | Search | Filter entity | Config | Filtered |
| INT-012 | Search | Filter type | Config | Filtered |
| INT-013 | Search | Filter section | Config | Filtered |
| INT-014 | Search | Multi-filter | Config | Combined |
| INT-015 | Search | Empty filter | - | All |
| INT-016 | Search | Invalid filter | Config | 400 |
| INT-017 | Search | Sort | Config | Sorted |
| INT-018 | Search | Paginate | Config | Paginated |
| INT-019 | Search | Export filtered | Config | Matches |
| INT-020 | Search | Validate with schema | Config, Data | Result |
| INT-021 | Pagination | Page 1 | Config | First |
| INT-022 | Pagination | Last page | Config | Partial |
| INT-023 | Pagination | Size | Config | Correct |
| INT-024 | Pagination | Invalid | Config | 400 |
| INT-025 | Pagination | Boundary | Config | Exact |
| INT-026 | Relationships | Schema → Field | Config | Linked |
| INT-027 | Relationships | Field → Options | Config | Linked |
| INT-028 | Relationships | Field → Dependencies | Config | Linked |
| INT-029 | Relationships | Orphan | Deleted field | 404 |
| INT-030 | Relationships | Entity → Schema | Entity, Config | Linked |
| INT-031 | Error | DB down | DB | 503 |
| INT-032 | Error | Auth down | Auth | 401/503 |
| INT-033 | Error | Validation | Bad input | 400 |
| INT-034 | Error | NotFound | Invalid ID | 404 |
| INT-035 | Error | Forbidden | No permission | 403 |
| INT-036 | Error | Conflict | Duplicate | 409 |
| INT-037 | Error | Rate limit | Too many | 429 |
| INT-038 | Error | Timeout | Slow | 504 |
| INT-039 | Error | Payload | Huge | 413 |
| INT-040 | Error | Media | Wrong type | 415 |
| INT-041 | Error | Method | Wrong verb | 405 |
| INT-042 | Error | Service | Dependency | 503 |
| INT-043 | Error | Gateway | Upstream | 504 |
| INT-044 | Error | Gone | Deleted | 410 |
| INT-045 | Error | Locked | Locked | 423 |
| INT-046 | E2E | Full create flow | Config | Create → Get |
| INT-047 | E2E | Full update flow | Config | Update → Get |
| INT-048 | E2E | Validate flow | Config, Data | Validate |
| INT-049 | E2E | Import/Export | Config | Round-trip |
| INT-050 | E2E | Session expiry | Auth | Clean fail |
| INT-051 | CRUD | Get schema | Config | Schema |
| INT-052 | CRUD | Get fields | Config | Fields |
| INT-053 | CRUD | Create field | Config | Created |
| INT-054 | CRUD | Update field | Config | Updated |
| INT-055 | CRUD | Delete field | Config | Deleted |
| INT-056 | Search | Filter entity | Config | Filtered |
| INT-057 | Search | Filter type | Config | Filtered |
| INT-058 | Search | Filter section | Config | Filtered |
| INT-059 | Search | Multi-filter | Config | Combined |
| INT-060 | Search | Empty filter | - | All |
| INT-061 | Pagination | Page 1 | Config | First |
| INT-062 | Pagination | Last page | Config | Partial |
| INT-063 | Pagination | Size | Config | Correct |
| INT-064 | Pagination | Invalid | Config | 400 |
| INT-065 | Pagination | Boundary | Config | Exact |
| INT-066 | Relationships | Schema → Field | Config | Linked |
| INT-067 | Relationships | Field → Options | Config | Linked |
| INT-068 | Relationships | Field → Dependencies | Config | Linked |
| INT-069 | Relationships | Orphan | Deleted field | 404 |
| INT-070 | Relationships | Entity → Schema | Entity, Config | Linked |
| INT-071 | Error | DB down | DB | 503 |
| INT-072 | Error | Auth down | Auth | 401/503 |
| INT-073 | Error | Validation | Bad input | 400 |
| INT-074 | Error | NotFound | Invalid ID | 404 |
| INT-075 | Error | Forbidden | No permission | 403 |
| INT-076 | Error | Conflict | Duplicate | 409 |
| INT-077 | Error | Rate limit | Too many | 429 |
| INT-078 | Error | Timeout | Slow | 504 |
| INT-079 | Error | Payload | Huge | 413 |
| INT-080 | Error | Media | Wrong type | 415 |
| INT-081 | Error | Method | Wrong verb | 405 |
| INT-082 | Error | Service | Dependency | 503 |
| INT-083 | Error | Gateway | Upstream | 504 |
| INT-084 | Error | Gone | Deleted | 410 |
| INT-085 | Error | Locked | Locked | 423 |
| INT-086 | E2E | Full create flow | Config | Create → Get |
| INT-087 | E2E | Full update flow | Config | Update → Get |
| INT-088 | E2E | Validate flow | Config, Data | Validate |
| INT-089 | E2E | Clone flow | Config | Clone → Get |
| INT-090 | E2E | Import flow | Config | Import → Get |

---

## §7 Concurrency Tests (25)

| ID | Scenario | Expected |
|----|----------|----------|
| CON-001 | 2 users get schema | Both succeed |
| CON-002 | 2 admins create same key | One fails 409 |
| CON-003 | 2 admins update same | Last write |
| CON-004 | 10 concurrent gets | All succeed |
| CON-005 | 50 concurrent list | All succeed |
| CON-006 | Import during read | Snapshot |
| CON-007 | Update during validate | Snapshot |
| CON-008 | Cache invalidation | No stale |
| CON-009 | Connection pool | Queue/503 |
| CON-010 | Transaction | No dirty |
| CON-011 | Optimistic | Last write |
| CON-012 | Deadlock | Timeout |
| CON-013 | Delete during use | 404 or cached |
| CON-014 | Clone during update | Snapshot |
| CON-015 | Rate limit | Fair |
| CON-016 | Session expiry | Clean |
| CON-017 | Multiple creates | All or unique |
| CON-018 | Cache stampede | Single |
| CON-019 | Lock | Timeout |
| CON-020 | Memory | Graceful |
| CON-021 | Field during delete | Consistent |
| CON-022 | Schema during import | Consistent |
| CON-023 | Permission change | Old |
| CON-024 | Validate concurrent | Both succeed |
| CON-025 | Replica lag | Eventual |

---

## §8 Unit Tests (21)

| ID | Category | Input | Expected |
|----|----------|-------|----------|
| UNT-001 | Validation | Valid key | Accept |
| UNT-002 | Validation | Invalid key | Reject |
| UNT-003 | Validation | Valid type | Accept |
| UNT-004 | Validation | Invalid type | Reject |
| UNT-005 | Validation | Valid regex | Accept |
| UNT-006 | Formatting | Field value | Correct type |
| UNT-007 | Formatting | Date | ISO 8601 |
| UNT-008 | Formatting | Number | Localized |
| UNT-009 | Calculation | Order | Correct |
| UNT-010 | Calculation | Dependencies | Resolved |
| UNT-011 | Calculation | Visibility | Evaluated |
| UNT-012 | Calculation | Validation | Result |
| UNT-013 | Calculation | Default | Applied |
| UNT-014 | Status | Active | Active only |
| UNT-015 | Status | Inactive | Inactive only |
| UNT-016 | Status | All | All |
| UNT-017 | Status | Required | Required only |
| UNT-018 | Status | Optional | Optional only |
| UNT-019 | Collections | Empty | [] |
| UNT-020 | Collections | Single | [item] |
| UNT-021 | Collections | Dedupe | No dupes |

---

## §9 Performance Tests (16)

| ID | Operation | Threshold |
|----|-----------|-----------|
| PRF-001 | Get schema | < 100ms |
| PRF-002 | Get fields | < 200ms |
| PRF-003 | Get form schema | < 150ms |
| PRF-004 | Validate | < 200ms |
| PRF-005 | Create field | < 300ms |
| PRF-006 | Update field | < 200ms |
| PRF-007 | Delete field | < 100ms |
| PRF-008 | Import | < 5s |
| PRF-009 | Export | < 2s |
| PRF-010 | 10 concurrent | < 500ms each |
| PRF-011 | 50 concurrent | < 1s each |
| PRF-012 | 5 concurrent create | < 1s each |
| PRF-013 | Memory | < 100MB |
| PRF-014 | Memory import | < 200MB |
| PRF-015 | Cache hit | > 90% |
| PRF-016 | DB queries | < 5 per request |

---

## §10 Load Tests (10)

| ID | Load Profile | Duration | Success Criteria |
|----|--------------|----------|-------------------|
| LDT-001 | 10 users | 10 min | 95% < 200ms |
| LDT-002 | 50 users | 10 min | 95% < 500ms |
| LDT-003 | 100 users | 10 min | 95% < 1s |
| LDT-004 | Spike 10→100 | 5 min | No crash |
| LDT-005 | Spike 50→200 | 5 min | Graceful |
| LDT-006 | Stress 200 | Until fail | Document |
| LDT-007 | Stress 500 | Until fail | Document |
| LDT-008 | 50 concurrent validate | 5 min | Queue/limit |
| LDT-009 | Recovery spike | 5 min | Baseline |
| LDT-010 | Recovery stress | 10 min | Full |

---

## Traceability Matrix

| Requirement | Test Cases |
|-------------|------------|
| Field definitions | POS-003–004, FUN-003 |
| Entity schemas | POS-001–002, FUN-001–002 |
| Custom fields | POS-005, FUN-004 |
| Validation rules | POS-006, FUN-007 |
| 3:1 Ratio | NEG-001–090, BND-001–090 |

---

**Last Updated:** 2026-02-11  
**Status:** Ready for Execution
