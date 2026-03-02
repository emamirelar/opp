# Comprehensive Unit Tests — Expansion

**Component:** All Managers/Services — Unit Test Coverage Expansion  
**Created:** 2026-02-18  
**Author:** QA Team  
**Purpose:** Expands unit test coverage across ALL managers to reach 1,020+ unit tests total. Existing 26 .md unit test files have ~21 unit tests each in §8 (546 total). This expansion adds 474 tests in under-represented areas.

---

## Compliance Summary

| Section | Count | Description |
|---------|-------|-------------|
| A: Value Transformation & Parsing | 80 | Pure functions across all managers |
| B: Business Rule Validation | 120 | Pure validation logic tests |
| C: Mapping & Serialization | 100 | Entity/Model mapping, JSON, CSV |
| D: Query Construction | 80 | Specification, Include, Sort, IsDeleted |
| E: State Management | 94 | Workflow, audit, soft-delete |
| **TOTAL** | **474** | **New unit tests** |

---

## Section A: Value Transformation & Parsing (80 tests)

### A.1 AutoMapper Profile Configuration (30 tests across 10 profiles)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| A-001 | PartnerProfile CreateMap Partner→PartnerModel | AutoMapper | Partner entity | PartnerModel with all properties mapped | P0 |
| A-002 | PartnerProfile ReverseMap PartnerModel→Partner | AutoMapper | PartnerModel | Partner entity | P1 |
| A-003 | PartnerProfile ForMember Name Ignore | AutoMapper | Partner with Name | Name not overwritten on update | P1 |
| A-004 | ContactProfile CreateMap Contact→ContactModel | AutoMapper | Contact entity | ContactModel with all properties | P0 |
| A-005 | ContactProfile ForMember Email validation | AutoMapper | Contact with invalid email | Mapping handles null/empty | P1 |
| A-006 | InteractionProfile CreateMap Interaction→InteractionModel | AutoMapper | Interaction entity | InteractionModel mapped | P0 |
| A-007 | InteractionProfile ForMember Date formatting | AutoMapper | Interaction with dates | Dates in correct format | P1 |
| A-008 | DocumentProfile CreateMap Document→DocumentModel | AutoMapper | Document entity | DocumentModel mapped | P0 |
| A-009 | DocumentProfile ForMember FilePath | AutoMapper | Document with path | Path preserved or transformed | P1 |
| A-010 | OpportunityProfile CreateMap Opportunity→OpportunityModel | AutoMapper | Opportunity entity | OpportunityModel with nested | P0 |
| A-011 | OpportunityProfile ForMember Budget | AutoMapper | Opportunity with budget | Budget decimal precision | P1 |
| A-012 | LinkProfile CreateMap Link→LinkModel | AutoMapper | Link entity | LinkModel with URL | P0 |
| A-013 | LinkProfile ForMember Url validation | AutoMapper | Link with URL | URL format preserved | P1 |
| A-014 | WorkflowProfile CreateMap WorkflowStatus→Model | AutoMapper | WorkflowStatus | Model mapped | P0 |
| A-015 | OrganizationProfile CreateMap OrgUnit→Model | AutoMapper | OrgUnit entity | Model with hierarchy | P0 |
| A-016 | NotificationProfile CreateMap Notification→Model | AutoMapper | Notification entity | Model with read state | P0 |
| A-017 | UserProfile CreateMap User→UserModel | AutoMapper | User entity | UserModel mapped | P0 |
| A-018 | ProfileProfile ForMember nested mapping | AutoMapper | Profile with nested | Nested objects mapped | P1 |
| A-019 | ValuesProfile CreateMap Value→ValueModel | AutoMapper | Value entity | ValueModel mapped | P0 |
| A-020 | EntityConfigProfile CreateMap Config→Model | AutoMapper | EntityConfig | Model mapped | P0 |
| A-021 | PartnerProfile conditional mapping | AutoMapper | Partner with null nav | Null handled | P1 |
| A-022 | ContactProfile Partner navigation | AutoMapper | Contact with Partner | Partner included in map | P1 |
| A-023 | InteractionProfile Contact navigation | AutoMapper | Interaction with Contact | Contact mapped | P1 |
| A-024 | DocumentProfile DocumentType mapping | AutoMapper | Document with type | Type mapped | P1 |
| A-025 | OpportunityProfile Partner mapping | AutoMapper | Opportunity with partners | Partners collection mapped | P1 |
| A-026 | AutoMapper configuration validation | AutoMapper | All profiles | AssertConfigurationIsValid passes | P0 |
| A-027 | PartnerProfile Id mapping | AutoMapper | Partner Id=5 | Model Id=5 | P1 |
| A-028 | ContactProfile Status mapping | AutoMapper | Contact Status=Active | Model Status=Active | P1 |
| A-029 | WorkflowProfile stage mapping | AutoMapper | Workflow stage | Model stage | P1 |
| A-030 | Null source handling | AutoMapper | null entity | null or default model | P1 |

### A.2 EntityStatus Transitions (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| A-031 | EntityStatus Active→Inactive allowed | Status | Partner Active | Transition succeeds | P0 |
| A-032 | EntityStatus Inactive→Active allowed | Status | Partner Inactive | Transition succeeds | P0 |
| A-033 | EntityStatus Draft→Active allowed | Status | Opportunity Draft | Transition succeeds | P0 |
| A-034 | EntityStatus Draft→Submitted allowed | Status | Opportunity Draft | Transition succeeds | P0 |
| A-035 | EntityStatus Submitted→Approved allowed | Status | Opportunity Submitted | Transition succeeds | P0 |
| A-036 | EntityStatus Active→Draft not allowed | Status | Partner Active | Transition rejected | P0 |
| A-037 | EntityStatus Deleted→Active not allowed | Status | Entity IsDeleted | Transition rejected | P0 |
| A-038 | EntityStatus Pending validation | Status | Status=Pending | Valid transition | P1 |
| A-039 | EntityStatus Archived transition | Status | Entity Archived | Archived→Active rejected | P1 |
| A-040 | EntityStatus enum value validation | Status | Invalid enum value | ArgumentException | P1 |

### A.3 Date Formatting (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| A-041 | Date to ISO 8601 format | Date | DateTime.UtcNow | "2026-02-18T12:00:00Z" format | P0 |
| A-042 | Date UTC conversion | Date | DateTime.Local | Converted to UTC | P0 |
| A-043 | Date timezone handling | Date | DateTime with timezone | Correct UTC offset | P1 |
| A-044 | Null date handling | Date | null DateTime? | null or default | P1 |
| A-045 | MinValue date handling | Date | DateTime.MinValue | Handled or rejected | P1 |
| A-046 | MaxValue date handling | Date | DateTime.MaxValue | Handled or rejected | P1 |
| A-047 | Date only (no time) | Date | DateOnly | Correct serialization | P1 |
| A-048 | Date string parse | Date | "2026-02-18" | Parsed correctly | P1 |
| A-049 | Date format invariant | Date | Various locales | Consistent output | P1 |
| A-050 | Audit CreatedDate format | Date | CreatedDate | ISO 8601 | P0 |

### A.4 Pagination Calculation (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| A-051 | Page 1 Size 10 → Skip 0 Take 10 | Pagination | page=1, pageSize=10 | Skip=0, Take=10 | P0 |
| A-052 | Page 2 Size 10 → Skip 10 Take 10 | Pagination | page=2, pageSize=10 | Skip=10, Take=10 | P0 |
| A-053 | Page 5 Size 20 → Skip 80 Take 20 | Pagination | page=5, pageSize=20 | Skip=80, Take=20 | P0 |
| A-054 | Total 100 Size 10 → 10 pages | Pagination | total=100, size=10 | totalPages=10 | P0 |
| A-055 | Total 95 Size 10 → 10 pages | Pagination | total=95, size=10 | totalPages=10 | P1 |
| A-056 | Total 0 Size 10 → 0 pages | Pagination | total=0, size=10 | totalPages=0 | P1 |
| A-057 | Page 0 default to 1 | Pagination | page=0 | Treated as 1 | P1 |
| A-058 | PageSize 0 default to 10 | Pagination | pageSize=0 | Default 10 | P1 |
| A-059 | PageSize max cap 100 | Pagination | pageSize=1000 | Capped at 100 | P1 |
| A-060 | Negative page rejection | Pagination | page=-1 | ArgumentException | P0 |
| A-061 | Negative pageSize rejection | Pagination | pageSize=-5 | ArgumentException | P0 |
| A-062 | Total pages ceiling | Pagination | total=1, size=10 | totalPages=1 | P1 |
| A-063 | Skip overflow handling | Pagination | page=999, size=10 | Skip calculated | P1 |
| A-064 | Pagination with filter | Pagination | filter + page | Correct Skip/Take | P1 |
| A-065 | Pagination metadata | Pagination | page, total, size | hasNext, hasPrev | P1 |

### A.5 Filter Request Construction (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| A-066 | TypeaheadInput Id and Name | Filter | TypeaheadInput | Id, Name populated | P0 |
| A-067 | GenericFilter single field | Filter | field=Name, value="X" | Filter expression | P0 |
| A-068 | GenericFilter multiple fields | Filter | multiple fields | Combined expression | P1 |
| A-069 | Dynamic filter PartnerId | Filter | PartnerId=5 | Where PartnerId==5 | P0 |
| A-070 | Dynamic filter Status | Filter | Status=Active | Where Status==Active | P0 |
| A-071 | Dynamic filter date range | Filter | StartDate, EndDate | Date range expression | P1 |
| A-072 | Filter with null value | Filter | value=null | Ignored or handled | P1 |
| A-073 | Filter with empty string | Filter | value="" | Ignored or handled | P1 |
| A-074 | Filter search term | Filter | searchTerm="abc" | Contains expression | P0 |
| A-075 | Filter typeahead options | Filter | TypeaheadInput list | Options for dropdown | P1 |
| A-076 | Filter pagination combined | Filter | filter + pagination | Both applied | P1 |
| A-077 | Filter sort combined | Filter | filter + sort | Both applied | P1 |
| A-078 | Filter invalid field | Filter | invalid field name | Rejected or ignored | P1 |
| A-079 | Filter SQL injection attempt | Filter | '; DROP | Sanitized | P0 |
| A-080 | Filter special characters | Filter | value with %_ | Escaped | P1 |

---

## Section B: Business Rule Validation (120 tests)

### B.1 Partner Name Uniqueness (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| B-001 | Partner name unique check pass | Validation | New name "Acme Corp" | No duplicate | P0 |
| B-002 | Partner name duplicate same org | Validation | Existing name | BusinessException | P0 |
| B-003 | Partner name case insensitive | Validation | "acme corp" vs "Acme Corp" | Duplicate detected | P0 |
| B-004 | Partner name trim whitespace | Validation | " Acme " | Trimmed before check | P1 |
| B-005 | Partner name null | Validation | null | ValidationException | P0 |
| B-006 | Partner name empty | Validation | "" | ValidationException | P0 |
| B-007 | Partner name max length | Validation | 500 char name | Rejected or truncated | P1 |
| B-008 | Partner name exclude self on update | Validation | Update same partner | Not duplicate | P0 |
| B-009 | Partner name soft-deleted excluded | Validation | Deleted partner same name | Allowed | P1 |
| B-010 | Partner name special chars | Validation | "Partner & Co." | Handled | P1 |

### B.2 Contact Email Format Validation (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| B-011 | Email valid format | Validation | user@domain.com | Valid | P0 |
| B-012 | Email invalid no @ | Validation | userdomain.com | Invalid | P0 |
| B-013 | Email invalid no domain | Validation | user@ | Invalid | P0 |
| B-014 | Email invalid double @ | Validation | user@@domain.com | Invalid | P0 |
| B-015 | Email valid with subdomain | Validation | user@mail.domain.com | Valid | P1 |
| B-016 | Email valid with plus | Validation | user+tag@domain.com | Valid | P1 |
| B-017 | Email null | Validation | null | Invalid | P0 |
| B-018 | Email empty | Validation | "" | Invalid | P0 |
| B-019 | Email max length | Validation | 254 chars | Valid | P1 |
| B-020 | Email unicode | Validation | user@münchen.de | Handled | P1 |

### B.3 Opportunity Budget Validation (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| B-021 | Budget GreaterThan 0 | Validation | 1000.00 | Valid | P0 |
| B-022 | Budget zero rejected | Validation | 0 | ValidationException | P0 |
| B-023 | Budget negative rejected | Validation | -100 | ValidationException | P0 |
| B-024 | Budget currency format | Validation | 1000.50 | Decimal precision | P0 |
| B-025 | Budget null optional | Validation | null | Handled per spec | P1 |
| B-026 | Budget max value | Validation | decimal.MaxValue | Handled | P1 |
| B-027 | Budget currency code | Validation | USD, EUR | Valid | P1 |
| B-028 | Budget invalid currency | Validation | XXX | Rejected | P1 |
| B-029 | Budget precision 2 decimals | Validation | 1000.999 | Rounded or rejected | P1 |
| B-030 | Budget multiple currencies | Validation | Mixed currencies | Handled | P1 |

### B.4 Document Type Validation (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| B-031 | Document type for Partner entity | Validation | Partner, type=Agreement | Valid | P0 |
| B-032 | Document type for Opportunity entity | Validation | Opportunity, type=Proposal | Valid | P0 |
| B-033 | Document type invalid for entity | Validation | Partner, type=Proposal | Rejected | P0 |
| B-034 | Document type null | Validation | null type | Rejected | P0 |
| B-035 | Document type deleted | Validation | IsDeleted type | Rejected | P1 |
| B-036 | Document type required per entity | Validation | Entity config | Type required | P1 |
| B-037 | Document type extension match | Validation | .pdf, type PDF | Valid | P1 |
| B-038 | Document type extension mismatch | Validation | .docx, type PDF | Warning or reject | P1 |
| B-039 | Document type max size | Validation | File size | Valid within limit | P1 |
| B-040 | Document type multiple per entity | Validation | Multiple types | All validated | P1 |

### B.5 Link URL Format Validation (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| B-041 | URL valid https | Validation | https://example.com | Valid | P0 |
| B-042 | URL valid http | Validation | http://example.com | Valid | P1 |
| B-043 | URL invalid no scheme | Validation | example.com | Invalid | P0 |
| B-044 | URL invalid javascript | Validation | javascript:alert(1) | Rejected | P0 |
| B-045 | URL invalid data | Validation | data:text/html | Rejected | P0 |
| B-046 | URL null | Validation | null | Rejected | P0 |
| B-047 | URL empty | Validation | "" | Rejected | P0 |
| B-048 | URL max length | Validation | 2048 chars | Valid or rejected | P1 |
| B-049 | URL with query params | Validation | https://x.com?a=1 | Valid | P1 |
| B-050 | URL with fragment | Validation | https://x.com#section | Valid | P1 |

### B.6 Workflow Stage Requirement Checking (42 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| B-051 | GO Req 1: Partner selected | Validation | Opportunity no partner | Requirement not met | P0 |
| B-052 | GO Req 2: Countries selected | Validation | Opportunity no countries | Requirement not met | P0 |
| B-053 | GO Req 3: Budget populated | Validation | Opportunity no budget | Requirement not met | P0 |
| B-054 | GO Req 4: Schedule populated | Validation | Opportunity no schedule | Requirement not met | P0 |
| B-055 | GO Req 5: Team section | Validation | Opportunity no team | Requirement not met | P0 |
| B-056 | GO Req 6: WHAT section | Validation | Opportunity no WHAT | Requirement not met | P0 |
| B-057 | GO Req 7: WHY section | Validation | Opportunity no WHY | Requirement not met | P0 |
| B-058 | GO Req 8: DST completed | Validation | DST not done | Requirement not met | P0 |
| B-059 | GO Req 9: Risk assessment | Validation | No risks | Requirement not met | P0 |
| B-060 | GO Req 10: DoA2 holder | Validation | No DoA2 | Requirement not met | P0 |
| B-061 | GO Req 11-21: Additional requirements | Validation | Each requirement | Correct validation | P0 |
| B-062 | All 21 requirements met | Validation | Complete opportunity | Submit allowed | P0 |
| B-063 | Submit with 1 requirement missing | Validation | 20/21 met | Submit blocked | P0 |
| B-064 | Draft stage no requirements | Validation | Draft | No requirement check | P1 |
| B-065 | Submitted stage requirements | Validation | Submitted | Requirements locked | P1 |
| B-066 | GO Req partner soft-deleted | Validation | Partner deleted | Requirement not met | P1 |
| B-067 | GO Req country inactive | Validation | Country inactive | Handled | P1 |
| B-068 | GO Req budget zero | Validation | Budget=0 | Requirement not met | P1 |
| B-069 | GO Req DST partial | Validation | DST 50% | Requirement not met | P1 |
| B-070 | GO Req DoA2 removed | Validation | DoA2 deleted | Requirement not met | P1 |
| B-071 to B-092 | GO Req 12-21 individual checks | Validation | Each requirement | Correct validation | P0 |

### B.7 Interaction Date Range Validation (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| B-093 | Interaction EndDate after StartDate | Validation | Valid range | Valid | P0 |
| B-094 | Interaction EndDate before StartDate | Validation | End < Start | ValidationException | P0 |
| B-095 | Interaction same date | Validation | Start=End | Valid | P1 |
| B-096 | Interaction future date | Validation | Future dates | Valid or rejected | P1 |
| B-097 | Interaction null dates | Validation | null Start | Rejected | P0 |
| B-098 | Interaction date range max | Validation | 1 year range | Valid or capped | P1 |
| B-099 | Interaction timezone | Validation | Different zones | Consistent | P1 |
| B-100 | Interaction recurring dates | Validation | Recurring | Valid | P1 |
| B-101 | Interaction date format | Validation | Various formats | Parsed correctly | P1 |
| B-102 | Interaction audit date | Validation | CreatedDate | Set correctly | P1 |

### B.8 Organization Hierarchy Type Validation (8 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| B-103 | Org type Office | Validation | Type=Office | Valid | P0 |
| B-104 | Org type Region | Validation | Type=Region | Valid | P0 |
| B-105 | Org type Hub | Validation | Type=Hub | Valid | P0 |
| B-106 | Org type OrgUnit | Validation | Type=OrgUnit | Valid | P0 |
| B-107 | Org type invalid | Validation | Type=Invalid | Rejected | P0 |
| B-108 | Org hierarchy depth | Validation | Max depth | Valid or rejected | P1 |
| B-109 | Org parent type compatibility | Validation | Office under Region | Valid | P1 |
| B-110 | Org circular reference | Validation | A→B→A | Rejected | P0 |

### B.9 CountryModel.CalculateConditionalTags (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| B-111 | ConditionalTags no conditions | Validation | Country default | Empty or default tags | P0 |
| B-112 | ConditionalTags with region | Validation | Country in region | Region tag | P0 |
| B-113 | ConditionalTags with income level | Validation | Country LDC | LDC tag | P0 |
| B-114 | ConditionalTags multiple | Validation | Multiple conditions | All tags | P1 |
| B-115 | ConditionalTags null country | Validation | null | Handled | P1 |
| B-116 | ConditionalTags inactive country | Validation | Inactive | Handled | P1 |
| B-117 | ConditionalTags empty result | Validation | No matching | Empty list | P1 |
| B-118 | ConditionalTags ordering | Validation | Multiple tags | Consistent order | P1 |
| B-119 | ConditionalTags deduplication | Validation | Duplicate conditions | No duplicates | P1 |
| B-120 | ConditionalTags cache | Validation | Same country twice | Same result | P1 |

---

## Section C: Mapping & Serialization (100 tests)

### C.1 Entity → Model Mapping Accuracy (20 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| C-001 | Partner entity to model | Mapping | Partner full | All fields mapped | P0 |
| C-002 | Contact entity to model | Mapping | Contact full | All fields mapped | P0 |
| C-003 | Interaction entity to model | Mapping | Interaction full | All fields mapped | P0 |
| C-004 | Document entity to model | Mapping | Document full | All fields mapped | P0 |
| C-005 | Opportunity entity to model | Mapping | Opportunity full | All fields mapped | P0 |
| C-006 | Link entity to model | Mapping | Link full | All fields mapped | P0 |
| C-007 | Organization entity to model | Mapping | OrgUnit full | All fields mapped | P0 |
| C-008 | Notification entity to model | Mapping | Notification full | All fields mapped | P0 |
| C-009 | Workflow entity to model | Mapping | Workflow full | All fields mapped | P0 |
| C-010 | User entity to model | Mapping | User full | All fields mapped | P0 |
| C-011 | Partner nested contacts | Mapping | Partner with contacts | Contacts in model | P1 |
| C-012 | Opportunity nested budget | Mapping | Opportunity with budget | Budget in model | P1 |
| C-013 | Entity null navigation | Mapping | Entity with null nav | Null in model | P1 |
| C-014 | Entity collection empty | Mapping | Entity empty collection | Empty in model | P1 |
| C-015 | Entity enum mapping | Mapping | Entity with enum | Correct enum in model | P1 |
| C-016 | Entity decimal precision | Mapping | Decimal values | No precision loss | P1 |
| C-017 | Entity DateTime mapping | Mapping | DateTime values | Correct format | P1 |
| C-018 | Entity soft-deleted | Mapping | IsDeleted entity | Flag in model | P1 |
| C-019 | Entity audit fields | Mapping | Entity with audit | Audit in model | P1 |
| C-020 | Entity ID mapping | Mapping | Entity Id | Model Id match | P0 |

### C.2 Request → Entity Mapping Accuracy (20 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| C-021 | CreatePartnerRequest to Partner | Mapping | Request | Entity created | P0 |
| C-022 | UpdatePartnerRequest to Partner | Mapping | Request | Entity updated | P0 |
| C-023 | CreateContactRequest to Contact | Mapping | Request | Entity created | P0 |
| C-024 | CreateInteractionRequest to Interaction | Mapping | Request | Entity created | P0 |
| C-025 | CreateDocumentRequest to Document | Mapping | Request | Entity created | P0 |
| C-026 | CreateOpportunityRequest to Opportunity | Mapping | Request | Entity created | P0 |
| C-027 | CreateLinkRequest to Link | Mapping | Request | Entity created | P0 |
| C-028 | Request null optional fields | Mapping | Request partial | Defaults applied | P1 |
| C-029 | Request ID ignored on create | Mapping | Request with Id | Id not set | P0 |
| C-030 | Request audit fields ignored | Mapping | Request with CreatedBy | Ignored | P0 |
| C-031 | Request IsDeleted ignored | Mapping | Request IsDeleted=false | Ignored | P0 |
| C-032 | Request nested objects | Mapping | Request with nested | Nested mapped | P1 |
| C-033 | Request validation before map | Mapping | Invalid request | Validation first | P1 |
| C-034 | Request trim strings | Mapping | Request with spaces | Trimmed | P1 |
| C-035 | Request enum conversion | Mapping | Request string enum | Correct enum | P1 |
| C-036 | Request date parsing | Mapping | Request date string | Parsed | P1 |
| C-037 | Request decimal parsing | Mapping | Request number string | Parsed | P1 |
| C-038 | Request FK mapping | Mapping | Request PartnerId | FK set | P0 |
| C-039 | Request collection mapping | Mapping | Request with list | Collection created | P1 |
| C-040 | Request Name required | Mapping | Request no Name | Name set or rejected | P0 |

### C.3 JSON Serialization/Deserialization (20 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| C-041 | PartnerModel JSON round-trip | Serialization | PartnerModel | Deserialize matches | P0 |
| C-042 | OpportunityModel JSON round-trip | Serialization | OpportunityModel | Deserialize matches | P0 |
| C-043 | Complex nested JSON | Serialization | Nested object | Round-trip | P1 |
| C-044 | JSON DateTime format | Serialization | DateTime | ISO 8601 | P0 |
| C-045 | JSON decimal format | Serialization | Decimal | No precision loss | P0 |
| C-046 | JSON null handling | Serialization | Null properties | Preserved | P1 |
| C-047 | JSON enum as string | Serialization | Enum value | String in JSON | P1 |
| C-048 | JSON circular reference | Serialization | Circular ref | Handled | P1 |
| C-049 | JSON special characters | Serialization | Unicode, quotes | Escaped | P1 |
| C-050 | JSON empty object | Serialization | {} | Deserialize | P1 |
| C-051 | JSON empty array | Serialization | [] | Deserialize | P1 |
| C-052 | JSON large payload | Serialization | Large object | Success | P1 |
| C-053 | JSON property naming | Serialization | camelCase | Correct case | P1 |
| C-054 | JSON ignore attribute | Serialization | [JsonIgnore] | Not serialized | P1 |
| C-055 | JSON custom converter | Serialization | Custom type | Converted | P1 |
| C-056 | JSON deserialize invalid | Serialization | Invalid JSON | Exception | P0 |
| C-057 | JSON deserialize wrong type | Serialization | Wrong structure | Exception | P1 |
| C-058 | JSON RecordData structure | Serialization | RecordData | Valid structure | P0 |
| C-059 | JSON ExtensibleModel | Serialization | Dynamic props | Preserved | P1 |
| C-060 | JSON TypeaheadInput | Serialization | TypeaheadInput | Id, Name | P1 |

### C.4 RecordData JSON Structure (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| C-061 | RecordData entity type | RecordData | EntityType | Correct type | P0 |
| C-062 | RecordData entity ID | RecordData | EntityId | Correct ID | P0 |
| C-063 | RecordData fields structure | RecordData | Fields dict | Key-value pairs | P0 |
| C-064 | RecordData nested objects | RecordData | Nested | Valid JSON | P1 |
| C-065 | RecordData null values | RecordData | Null field | Handled | P1 |
| C-066 | RecordData array values | RecordData | Array field | Serialized | P1 |
| C-067 | RecordData date values | RecordData | Date field | ISO format | P1 |
| C-068 | RecordData from entity | RecordData | Entity | Correct structure | P0 |
| C-069 | RecordData to entity | RecordData | RecordData | Entity populated | P1 |
| C-070 | RecordData validation | RecordData | Invalid | Rejected | P1 |

### C.5 Bulk CSV Parsing (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| C-071 | CSV parse headers | CSV | Header row | Column names | P0 |
| C-072 | CSV parse row | CSV | Data row | Values array | P0 |
| C-073 | CSV value coercion string | CSV | "text" | String | P0 |
| C-074 | CSV value coercion int | CSV | "123" | int 123 | P0 |
| C-075 | CSV value coercion decimal | CSV | "123.45" | decimal | P0 |
| C-076 | CSV value coercion date | CSV | "2026-02-18" | DateTime | P0 |
| C-077 | CSV value coercion bool | CSV | "true" | true | P1 |
| C-078 | CSV quoted values | CSV | "value, with comma" | Correct parse | P0 |
| C-079 | CSV empty cell | CSV | ,, | Null or empty | P1 |
| C-080 | CSV encoding UTF-8 | CSV | Unicode content | Correct | P1 |
| C-081 | CSV invalid format | CSV | Malformed | Exception | P0 |
| C-082 | CSV missing column | CSV | Row short | Handled | P1 |
| C-083 | CSV extra column | CSV | Row long | Handled | P1 |
| C-084 | CSV bulk 1000 rows | CSV | 1000 rows | All parsed | P1 |
| C-085 | CSV template columns | CSV | Template | Correct order | P1 |

### C.6 Template Generation Column Ordering (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| C-086 | Partner template column order | Template | Partner entity | Name, Status, ... | P0 |
| C-087 | Contact template column order | Template | Contact entity | Name, Email, ... | P0 |
| C-088 | Opportunity template column order | Template | Opportunity entity | Title, Budget, ... | P0 |
| C-089 | Template required columns first | Template | Entity config | Required first | P1 |
| C-090 | Template optional columns | Template | Entity config | Optional after | P1 |
| C-091 | Template column headers | Template | Export | Match template | P0 |
| C-092 | Template localization | Template | Locale | Translated headers | P1 |
| C-093 | Template dynamic columns | Template | EntityConfig | Config-driven | P1 |
| C-094 | Template column validation | Template | Invalid column | Rejected | P1 |
| C-095 | Template export matches import | Template | Round-trip | Import works | P0 |
| C-096 | Template empty template | Template | No config | Default columns | P1 |
| C-097 | Template column count | Template | Entity | Correct count | P1 |
| C-098 | Template column data types | Template | Columns | Correct types | P1 |
| C-099 | Template nested columns | Template | Nested entity | Flattened | P1 |
| C-100 | Template column visibility | Template | Hidden columns | Excluded | P1 |

---

## Section D: Query Construction (80 tests)

### D.1 Specification Pattern (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| D-001 | Specification And | Query | Spec1 And Spec2 | Combined expression | P0 |
| D-002 | Specification Or | Query | Spec1 Or Spec2 | Combined expression | P0 |
| D-003 | Specification Not | Query | Not Spec | Negated expression | P0 |
| D-004 | Specification filter by Id | Query | Id=5 | Where Id==5 | P0 |
| D-005 | Specification filter by Status | Query | Status=Active | Where Status==Active | P0 |
| D-006 | Specification filter by PartnerId | Query | PartnerId=10 | Where PartnerId==10 | P0 |
| D-007 | Specification filter IsDeleted | Query | IncludeDeleted=false | Where !IsDeleted | P0 |
| D-008 | Specification multiple criteria | Query | Multiple | All combined | P1 |
| D-009 | Specification empty | Query | No criteria | No filter | P1 |
| D-010 | Specification null handling | Query | Null value | Ignored | P1 |
| D-011 | Specification date range | Query | Start, End | Date range | P1 |
| D-012 | Specification search term | Query | Search "x" | Contains | P1 |
| D-013 | Specification pagination | Query | Page, Size | Skip, Take | P1 |
| D-014 | Specification sort | Query | Sort field | OrderBy | P1 |
| D-015 | Specification compile | Query | Complex spec | Executable | P0 |

### D.2 Include Chain Building (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| D-016 | Include Partner | Query | Partner entity | .Include(p=>p.Partner) | P0 |
| D-017 | Include Contacts | Query | Partner | .Include(p=>p.Contacts) | P0 |
| D-018 | Include ThenInclude | Query | Partner.Contacts | .ThenInclude(c=>c.User) | P0 |
| D-019 | Include chain depth 3 | Query | A.B.C | Full chain | P1 |
| D-020 | Include multiple | Query | Partner, Contacts, Docs | Multiple Include | P0 |
| D-021 | Include filtered | Query | Contacts.Where(!IsDeleted) | Filtered include | P1 |
| D-022 | Include null navigation | Query | Optional nav | No exception | P1 |
| D-023 | Include AsNoTracking | Query | Read-only | AsNoTracking | P0 |
| D-024 | Include invalid path | Query | Invalid path | Exception | P1 |
| D-025 | Include collection | Query | Collection nav | Loaded | P0 |
| D-026 | Include reference | Query | Reference nav | Loaded | P0 |
| D-027 | Include duplicate | Query | Same include twice | Deduplicated | P1 |
| D-028 | Include conditional | Query | Conditional include | Applied | P1 |
| D-029 | Include performance | Query | Many includes | Single query or split | P1 |
| D-030 | Include soft-deleted filter | Query | Child include | !IsDeleted | P0 |

### D.3 Sort Expression Building (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| D-031 | Sort by Name ascending | Query | sortField=Name | OrderBy(Name) | P0 |
| D-032 | Sort by Name descending | Query | sortDesc=true | OrderByDescending(Name) | P0 |
| D-033 | Sort by Date | Query | sortField=CreatedDate | OrderBy | P0 |
| D-034 | Sort by multiple | Query | Name, then Status | ThenBy | P1 |
| D-035 | Sort invalid field | Query | Invalid field | Exception or default | P1 |
| D-036 | Sort null handling | Query | sortField=null | Default sort | P1 |
| D-037 | Sort navigation property | Query | sort Partner.Name | Include + OrderBy | P1 |
| D-038 | Sort case insensitive | Query | Name sort | Case insensitive | P1 |
| D-039 | Sort nulls first/last | Query | Nullable field | Nulls position | P1 |
| D-040 | Sort with filter | Query | Filter + Sort | Both applied | P0 |

### D.4 Search Term Normalization (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| D-041 | Search term trim | Query | "  x  " | "x" | P0 |
| D-042 | Search term lowercase | Query | "ABC" | "abc" (if case-insensitive) | P1 |
| D-043 | Search term wildcard | Query | "*x*" | Contains | P1 |
| D-044 | Search term escape | Query | Special chars | Escaped | P0 |
| D-045 | Search term null | Query | null | No search filter | P1 |
| D-046 | Search term empty | Query | "" | No search filter | P1 |
| D-047 | Search term max length | Query | Very long | Truncated or rejected | P1 |
| D-048 | Search term SQL injection | Query | '; DROP | Sanitized | P0 |
| D-049 | Search term unicode | Query | Unicode | Handled | P1 |
| D-050 | Search term multiple words | Query | "word1 word2" | AND or OR | P1 |

### D.5 IsDeleted Filter Application (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| D-051 | IsDeleted filter default | Query | Standard query | Where !IsDeleted | P0 |
| D-052 | IsDeleted include deleted | Query | IncludeDeleted=true | No filter | P1 |
| D-053 | IsDeleted on Partner | Query | Partner list | !IsDeleted | P0 |
| D-054 | IsDeleted on Contact | Query | Contact list | !IsDeleted | P0 |
| D-055 | IsDeleted on Include | Query | Partner.Contacts | Children !IsDeleted | P0 |
| D-056 | IsDeleted GetById | Query | GetById | Exclude deleted | P0 |
| D-057 | IsDeleted count | Query | Count | Exclude deleted | P0 |
| D-058 | IsDeleted soft-delete query | Query | Deleted records | Not returned | P0 |
| D-059 | IsDeleted admin override | Query | Admin IncludeDeleted | All returned | P1 |
| D-060 | IsDeleted cascade check | Query | Parent deleted | Children excluded | P1 |
| D-061 | IsDeleted filter position | Query | Complex query | Filter applied | P1 |
| D-062 | IsDeleted specification | Query | Spec with IsDeleted | Combined | P1 |
| D-063 | IsDeleted join | Query | Join with deleted | Excluded | P1 |
| D-064 | IsDeleted bulk operations | Query | Bulk query | Exclude deleted | P1 |
| D-065 | IsDeleted export | Query | Export | Exclude deleted | P0 |

### D.6 Pagination Application to IQueryable (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| D-066 | Pagination Skip | Query | page=2, size=10 | .Skip(10) | P0 |
| D-067 | Pagination Take | Query | size=10 | .Take(10) | P0 |
| D-068 | Pagination Skip Take | Query | page, size | .Skip().Take() | P0 |
| D-069 | Pagination with OrderBy | Query | Pagination + Sort | Sort before Skip | P0 |
| D-070 | Pagination with filter | Query | Filter + Page | Filter then Skip | P0 |
| D-071 | Pagination total count | Query | Paginated query | Count before Skip | P0 |
| D-072 | Pagination page 1 | Query | page=1 | Skip(0) | P0 |
| D-073 | Pagination last page | Query | Partial last page | Correct Take | P1 |
| D-074 | Pagination empty result | Query | No data | Empty list | P1 |
| D-075 | Pagination overflow | Query | page=999 | Empty or last | P1 |
| D-076 | Pagination IQueryable | Query | IQueryable | Deferred execution | P1 |
| D-077 | Pagination materialize | Query | ToListAsync | Executed | P0 |
| D-078 | Pagination metadata | Query | Result | TotalCount, Page | P1 |
| D-079 | Pagination hasNext | Query | Result | hasNext flag | P1 |
| D-080 | Pagination hasPrevious | Query | Result | hasPrev flag | P1 |

---

## Section E: State Management (94 tests)

### E.1 Workflow State Machine Transitions (20 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| E-001 | Workflow Draft→Submitted | State | Opportunity Draft | Transition succeeds | P0 |
| E-002 | Workflow Submitted→Approved | State | Opportunity Submitted | Transition succeeds | P0 |
| E-003 | Workflow Approved→GO | State | Opportunity Approved | Transition succeeds | P0 |
| E-004 | Workflow GO→Active | State | Opportunity GO | Transition succeeds | P0 |
| E-005 | Workflow Draft→NoGo | State | Opportunity Draft | Transition succeeds | P0 |
| E-006 | Workflow invalid transition | State | Active→Draft | Rejected | P0 |
| E-007 | Workflow permission check | State | Transition | Permission required | P0 |
| E-008 | Workflow requirement check | State | Submit | 21 requirements | P0 |
| E-009 | Workflow audit trail | State | Transition | Audit logged | P0 |
| E-010 | Workflow concurrent transition | State | Two users | One succeeds | P1 |
| E-011 | Workflow state persistence | State | Transition | DB updated | P0 |
| E-012 | Workflow notification | State | Transition | Notification sent | P1 |
| E-013 | Workflow Partner status | State | Partner Active/Inactive | Valid | P0 |
| E-014 | Workflow Contact status | State | Contact status | Valid | P1 |
| E-015 | Workflow Document status | State | Document status | Valid | P1 |
| E-016 | Workflow rollback | State | Failed transition | State unchanged | P1 |
| E-017 | Workflow initial state | State | New entity | Draft/Default | P0 |
| E-018 | Workflow final states | State | GO, NoGo, Archived | No further transition | P1 |
| E-019 | Workflow state display | State | Current state | Correct label | P1 |
| E-020 | Workflow state history | State | Multiple transitions | History preserved | P1 |

### E.2 Entity Status Changes (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| E-021 | Status change Active→Inactive | Status | Partner | Status updated | P0 |
| E-022 | Status change Inactive→Active | Status | Partner | Status updated | P0 |
| E-023 | Status change audit | Status | Any change | LastModifiedBy set | P0 |
| E-024 | Status change validation | Status | Invalid change | Rejected | P0 |
| E-025 | Status change permission | Status | Change | Permission required | P0 |
| E-026 | Status change notification | Status | Significant change | Notification | P1 |
| E-027 | Status change related entities | Status | Partner inactive | Contacts affected | P1 |
| E-028 | Status change cache | Status | Change | Cache invalidated | P1 |
| E-029 | Status change concurrent | Status | Concurrent | Deterministic | P1 |
| E-030 | Status change batch | Status | Bulk update | All updated | P1 |
| E-031 | Status change default | Status | New entity | Default status | P0 |
| E-032 | Status change enum | Status | EntityStatus enum | Valid value | P0 |
| E-033 | Status change workflow | Status | Workflow status | Coupled | P1 |
| E-034 | Status change soft-delete | Status | Delete | IsDeleted set | P0 |
| E-035 | Status change restore | Status | Restore | IsDeleted cleared | P1 |

### E.3 Notification Read/Unread State (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| E-036 | Notification mark read | State | MarkRead(id) | IsRead=true | P0 |
| E-037 | Notification mark unread | State | MarkUnread(id) | IsRead=false | P0 |
| E-038 | Notification bulk mark read | State | MarkRead(ids) | All read | P1 |
| E-039 | Notification filter unread | State | GetUnread | Only unread | P0 |
| E-040 | Notification count unread | State | CountUnread | Correct count | P0 |
| E-041 | Notification created unread | State | New notification | IsRead=false | P0 |
| E-042 | Notification audit | State | Mark read | ReadDate set | P1 |
| E-043 | Notification user scope | State | Get for user | User's only | P0 |
| E-044 | Notification permission | State | Mark read | Permission | P1 |
| E-045 | Notification concurrent read | State | Concurrent mark | Consistent | P1 |

### E.4 Cache Invalidation Triggers (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| E-046 | Cache invalidate on create | Cache | Create entity | Cache cleared | P1 |
| E-047 | Cache invalidate on update | Cache | Update entity | Cache cleared | P1 |
| E-048 | Cache invalidate on delete | Cache | Delete entity | Cache cleared | P1 |
| E-049 | Cache invalidate on bulk | Cache | Bulk operation | Cache cleared | P1 |
| E-050 | Cache scope invalidation | Cache | Partner update | Partner cache only | P1 |
| E-051 | Cache TTL expiry | Cache | Time passed | Expired | P1 |
| E-052 | Cache key consistency | Cache | Same query | Same key | P1 |
| E-053 | Cache miss population | Cache | Miss | Populated | P1 |
| E-054 | Cache permission scope | Cache | User A | Not see B's cache | P1 |
| E-055 | Cache clear all | Cache | ClearAll | All cleared | P1 |

### E.5 Audit Field Population (15 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| E-056 | Audit CreatedBy on create | Audit | Create | CreatedBy=currentUser | P0 |
| E-057 | Audit CreatedDate on create | Audit | Create | CreatedDate=UtcNow | P0 |
| E-058 | Audit LastModifiedBy on update | Audit | Update | LastModifiedBy=currentUser | P0 |
| E-059 | Audit LastModifiedDate on update | Audit | Update | LastModifiedDate=UtcNow | P0 |
| E-060 | Audit DeletedBy on delete | Audit | Delete | DeletedBy=currentUser | P0 |
| E-061 | Audit DeletedDate on delete | Audit | Delete | DeletedDate=UtcNow | P0 |
| E-062 | Audit CreatedBy not overwritten | Audit | Update | CreatedBy unchanged | P0 |
| E-063 | Audit CreatedDate not overwritten | Audit | Update | CreatedDate unchanged | P0 |
| E-064 | Audit null user | Audit | User=0 | Handled | P1 |
| E-065 | Audit UTC timezone | Audit | Any | All UTC | P0 |
| E-066 | Audit mass assign blocked | Audit | Request with CreatedBy | Ignored | P0 |
| E-067 | Audit workflow transition | Audit | Transition | Logged | P1 |
| E-068 | Audit batch operation | Audit | Bulk | All audited | P1 |
| E-069 | Audit soft-delete | Audit | Soft delete | DeletedBy/Date | P0 |
| E-070 | Audit restore | Audit | Restore | DeletedBy cleared | P1 |

### E.6 Soft-Delete Flag Management (10 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| E-071 | Soft-delete set IsDeleted | SoftDelete | Delete(id) | IsDeleted=true | P0 |
| E-072 | Soft-delete set DeletedBy | SoftDelete | Delete(id) | DeletedBy set | P0 |
| E-073 | Soft-delete set DeletedDate | SoftDelete | Delete(id) | DeletedDate set | P0 |
| E-074 | Soft-delete no physical delete | SoftDelete | Delete | Record exists | P0 |
| E-075 | Soft-delete filter queries | SoftDelete | List | Excluded | P0 |
| E-076 | Soft-delete restore | SoftDelete | Restore(id) | IsDeleted=false | P1 |
| E-077 | Soft-delete restore clear | SoftDelete | Restore | DeletedBy/Date null | P1 |
| E-078 | Soft-delete permission | SoftDelete | Delete | Permission required | P0 |
| E-079 | Soft-delete cascade | SoftDelete | Parent deleted | Children handled | P1 |
| E-080 | Soft-delete audit | SoftDelete | Delete | Audit trail | P0 |

### E.7 Document Type Entity Association (14 tests)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| E-081 | DocType Partner association | DocType | Partner entity | Types for Partner | P0 |
| E-082 | DocType Opportunity association | DocType | Opportunity entity | Types for Opportunity | P0 |
| E-083 | DocType Contact association | DocType | Contact entity | Types for Contact | P0 |
| E-084 | DocType multiple entities | DocType | Config | Multiple associations | P1 |
| E-085 | DocType create document | DocType | Create with type | Type validated | P0 |
| E-086 | DocType update document | DocType | Update type | Validation | P1 |
| E-087 | DocType delete type | DocType | Delete type | Documents handled | P1 |
| E-088 | DocType inactive type | DocType | Inactive type | Rejected for new | P1 |
| E-089 | DocType required per entity | DocType | Entity config | Required check | P1 |
| E-090 | DocType default type | DocType | No type specified | Default applied | P1 |
| E-091 | DocType entity filter | DocType | GetTypes(entity) | Filtered list | P0 |
| E-092 | DocType dropdown options | DocType | Typeahead | Options | P1 |
| E-093 | DocType soft-delete | DocType | Type deleted | Excluded | P1 |
| E-094 | DocType ordering | DocType | Type list | Ordered | P1 |

---

## 3:1 Ratio Compliance Check (Expansion Only)

| Category | Count | Description |
|----------|-------|-------------|
| Value Transformation (A) | 80 | Pure functions, parsing, mapping config |
| Business Validation (B) | 120 | Validation logic, rules |
| Mapping/Serialization (C) | 100 | Entity/Model mapping, JSON, CSV |
| Query Construction (D) | 80 | Specification, Include, Sort, IsDeleted |
| State Management (E) | 94 | Workflow, audit, soft-delete |
| **TOTAL** | **474** | **New unit tests** |

**Combined with existing §8 (26 × 21 ≈ 546):** 546 + 474 = **1,020 total unit tests**

---

**Last Updated:** 2026-02-18  
**Status:** Ready for Implementation
