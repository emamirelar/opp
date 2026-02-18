# EntityArtifactManager — Test Cases

**Component:** `UNOPS.PAO.Business/Managers/EntityArtifactManager`  
**Interface:** `UNOPS.PAO.Business/Interfaces/IEntityArtifactManager.cs`  
**Controller:** `UNOPS.PAO.Presentation/Controllers/Admin/EntityArtifactController.cs`  
**Entity:** `UNOPS.PAO.Domain/Entities/EntityArtifact.cs` (inherits ModifiableDeletableEntity)  
**Created:** 2026-02-18 | **Last Updated:** 2026-02-18  
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
| §6 Security | 50 | — | OUT OF SCOPE |
| §7 Concurrency | 25 | 25 | ✅ |
| §8 Unit | 21 | 21 | ✅ |
| §9 Performance | 16 | 16 | ✅ |
| §10 Load | 10 | 10 | ✅ |
| **TOTAL** | **462** | **412 written** | ✅ |

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

**EntityArtifactManager** manages entity artifacts (Country, Partner, Contact, Opportunity, etc.) including text, number, boolean, date, JSON, and document values. Key responsibilities: GetAvailableEntityTypesAsync, GetArtifactTypesByEntityTypeAsync, GetEntityRecordsAsync, GetEntityArtifactAsync, UpsertEntityArtifactAsync, UpsertDocumentArtifactAsync, bulk template download, bulk upsert. All endpoints require PARTNER_GLOB_ADMIN role. Supported entity types: country, partner, organization, contact, orgunit, organizationhierarchy, opportunity.

---

## §1 Positive Tests (30)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|--------------|-------|-----------------|----------|
| POS-001 | Get entity types — success | User has PARTNER_GLOB_ADMIN | GET /api/entity-artifacts/entity-types | 200 OK, array of EntityTypeOption with EntityType, DisplayName | P0 |
| POS-002 | Get artifact types for country | User has PARTNER_GLOB_ADMIN | GET /api/entity-artifacts/artifact-types?entityType=country | 200 OK, array of ArtifactTypeResponse (HDI_Index, FSI, etc.) | P0 |
| POS-003 | Get artifact types for partner | User has PARTNER_GLOB_ADMIN | GET /api/entity-artifacts/artifact-types?entityType=partner | 200 OK, artifact types for partner | P0 |
| POS-004 | Get bulk artifact types for country | User has PARTNER_GLOB_ADMIN | GET /api/entity-artifacts/bulk/artifact-types?entityType=country | 200 OK, artifact types with AllowBulkUpdate=true | P0 |
| POS-005 | Get entity records — country with search | User has PARTNER_GLOB_ADMIN, countries exist | GET /api/entity-artifacts/entity-records?entityType=country&searchTerm=Norway | 200 OK, EntityRecordOption list with Id, Name, Description | P0 |
| POS-006 | Get entity records — partner without search | User has PARTNER_GLOB_ADMIN | GET /api/entity-artifacts/entity-records?entityType=partner | 200 OK, all partners as EntityRecordOption | P0 |
| POS-007 | Get entity artifact — exists | Artifact exists for country 1, artifactTypeId 5 | GET /api/entity-artifacts/get?entityType=country&entityId=1&artifactTypeId=5 | 200 OK, EntityArtifactResponse with ValueText/ValueNumber/etc. | P0 |
| POS-008 | Get entity artifact — not exists | No artifact for entity+type | GET /api/entity-artifacts/get?entityType=country&entityId=1&artifactTypeId=99 | 200 OK, null in response body | P0 |
| POS-009 | Upsert artifact — create text value | Country 1 exists, artifactTypeId 5 (text) | POST /api/entity-artifacts/upsert with EntityType=country, EntityId=1, ArtifactTypeId=5, ValueText="0.95" | 200 OK, EntityArtifactResponse with Id, ValueText="0.95" | P0 |
| POS-010 | Upsert artifact — create number value | Country 1 exists, artifactTypeId for number | POST /api/entity-artifacts/upsert with ValueNumber=0.85 | 200 OK, ValueNumber=0.85 persisted | P0 |
| POS-011 | Upsert artifact — create boolean value | POST /api/entity-artifacts/upsert with ValueBoolean=true | 200 OK, ValueBoolean=true | P0 |
| POS-012 | Upsert artifact — create date value | POST /api/entity-artifacts/upsert with ValueDate="2025-01-15" | 200 OK, ValueDate persisted | P0 |
| POS-013 | Upsert artifact — create JSON value | POST /api/entity-artifacts/upsert with ValueJson="{\"key\":\"value\"}" | 200 OK, ValueJson persisted | P0 |
| POS-014 | Upsert artifact — update existing | Artifact exists for country 1, type 5 | POST /api/entity-artifacts/upsert with same keys, ValueText="0.96" | 200 OK, existing artifact updated, ValueText="0.96" | P0 |
| POS-015 | Upload document artifact — PDF | Country 1 exists, valid PDF file | POST /api/entity-artifacts/upload-document (multipart) with EntityType, EntityId, ArtifactTypeId, File | 200 OK, ValueText contains GCS URL, ValueJson has fileName, mimeType, fileSize | P0 |
| POS-016 | Get document URL — artifact has URL | Document artifact exists with gs:// URL | GET /api/entity-artifacts/document-url?entityType=country&entityId=1&artifactTypeId=10 | 200 OK, { url: signedUrl, fileName } | P0 |
| POS-017 | Get entity artifacts list | Country 1 has 5 artifacts | GET /api/entity-artifacts/list?entityType=country&entityId=1 | 200 OK, array of 5 EntityArtifactResponse | P0 |
| POS-018 | Get unique ID example — country | Countries exist | GET /api/entity-artifacts/bulk/unique-id-example?entityType=country | 200 OK, UniqueIdFieldName=Iso2Code, ExampleValue (e.g. "NO") | P0 |
| POS-019 | Get unique ID example — partner | Partners with ErpDimValue exist | GET /api/entity-artifacts/bulk/unique-id-example?entityType=partner | 200 OK, UniqueIdFieldName=ErpDimValue, ExampleValue numeric | P0 |
| POS-020 | Generate bulk template | Artifact types 1,2,3 exist for country | POST /api/entity-artifacts/bulk/template-download with EntityType=country, ArtifactTypeIds=[1,2,3] | 200 OK, CSV file with header row, UniqueId column + artifact type columns | P0 |
| POS-021 | Bulk upsert — single row | Country with Iso2Code "NO" exists | POST /api/entity-artifacts/bulk/upsert with EntityType=country, Rows=[{UniqueId:"NO", CellValues:{1:"0.95"}}], ColumnToArtifactTypeMapping={1:5} | 200 OK, BulkEntityArtifactResponse, SuccessfulRows=1 | P0 |
| POS-022 | Bulk upsert — multiple rows | 3 countries exist | POST bulk/upsert with 3 rows | 200 OK, SuccessfulRows=3 | P0 |
| POS-023 | Get entity records — contact with search | Contacts exist | GET /api/entity-artifacts/entity-records?entityType=contact&searchTerm=john | 200 OK, contacts matching FirstName/LastName/Email | P1 |
| POS-024 | Get entity records — opportunity | Opportunities exist | GET /api/entity-artifacts/entity-records?entityType=opportunity | 200 OK, opportunities as EntityRecordOption | P1 |
| POS-025 | Get entity records — orgunit | Org hierarchies exist | GET /api/entity-artifacts/entity-records?entityType=orgunit&searchTerm=HQ | 200 OK, org units matching Name or Code | P1 |
| POS-026 | Upsert with EffectiveDate and ExpiryDate | POST upsert with EffectiveDate=2025-01-01, ExpiryDate=2025-12-31 | 200 OK, dates persisted | P1 |
| POS-027 | Upsert with Source and Metadata | POST upsert with Source="External API", Metadata="{\"source\":\"UN\"}" | 200 OK, Source and Metadata persisted | P1 |
| POS-028 | Get artifact type code | ArtifactType 5 exists with ArtifactTypeCode="HDI_Index" | GetArtifactTypeCodeAsync(5) | "HDI_Index" | P1 |
| POS-029 | Country HDI artifact | Country 1, HDI_Index artifact type | Upsert ValueNumber=0.95 for HDI_Index | 200 OK, HDI value stored | P1 |
| POS-030 | Country FSI artifact | Country 1, FSI artifact type | Upsert ValueNumber=45.2 for FSI | 200 OK, FSI value stored | P1 |

---

## §2 Negative Tests (90)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|--------------|-------|-----------------|----------|
| NEG-001 | Get entity types — no auth | No JWT | GET /api/entity-artifacts/entity-types | 401 Unauthorized | P0 |
| NEG-002 | Get entity types — wrong role | User lacks PARTNER_GLOB_ADMIN | GET /api/entity-artifacts/entity-types | 403 Forbidden | P0 |
| NEG-003 | Get artifact types — entityType empty | User has PARTNER_GLOB_ADMIN | GET /api/entity-artifacts/artifact-types?entityType= | 400 Bad Request, "Entity type is required" | P0 |
| NEG-004 | Get artifact types — entityType missing | GET /api/entity-artifacts/artifact-types | 400 Bad Request | P0 |
| NEG-005 | Get artifact types — invalid entity type | entityType=invalidtype | GET /api/entity-artifacts/artifact-types?entityType=invalidtype | 200 OK, empty array (no artifact types) | P1 |
| NEG-006 | Get entity records — entityType empty | GET /api/entity-artifacts/entity-records?entityType= | 400 Bad Request | P0 |
| NEG-007 | Get entity records — unsupported entity type | entityType=nonexistent | GET /api/entity-artifacts/entity-records?entityType=nonexistent | 200 OK, empty array | P1 |
| NEG-008 | Get entity artifact — entityId zero | GET /api/entity-artifacts/get?entityType=country&entityId=0&artifactTypeId=5 | 400 Bad Request, "Entity ID must be greater than 0" | P0 |
| NEG-009 | Get entity artifact — entityId negative | entityId=-1 | 400 Bad Request | P0 |
| NEG-010 | Get entity artifact — artifactTypeId zero | artifactTypeId=0 | 400 Bad Request, "Artifact type ID must be greater than 0" | P0 |
| NEG-011 | Get entity artifact — non-existent entity | entityId=999999 | GET get?entityType=country&entityId=999999&artifactTypeId=5 | 200 OK, null (no artifact) | P1 |
| NEG-012 | Get entity artifact — non-existent artifact type | artifactTypeId=99999 | 200 OK, null | P1 |
| NEG-013 | Upsert — EntityType null | POST upsert with EntityType=null | 400 Bad Request | P0 |
| NEG-014 | Upsert — EntityType empty | EntityType="" | 400 Bad Request | P0 |
| NEG-015 | Upsert — EntityId zero | EntityId=0 | 400 Bad Request, "Entity ID must be greater than 0" | P0 |
| NEG-016 | Upsert — EntityId negative | EntityId=-5 | 400 Bad Request | P0 |
| NEG-017 | Upsert — ArtifactTypeId zero | ArtifactTypeId=0 | 400 Bad Request | P0 |
| NEG-018 | Upsert — non-existent entity | EntityId=999999, country 999999 does not exist | POST upsert | 500 or BusinessException (entity FK may not be validated) | P1 |
| NEG-019 | Upsert — invalid artifact type ID | ArtifactTypeId=99999 (not in ArtifactType) | POST upsert | 500 or FK violation | P1 |
| NEG-020 | Upsert — text value for number artifact type | ArtifactType expects number, ValueText="abc" | POST upsert with ValueText="abc", ValueNumber=null | May succeed (manager doesn't validate type) or 400 | P1 |
| NEG-021 | Upsert — invalid number format | ArtifactType number, ValueNumber from invalid string | Bulk upsert with "not-a-number" in number column | FormatException, cell fails | P1 |
| NEG-022 | Upsert — invalid boolean format | Bulk upsert with "maybe" in boolean column | FormatException | P1 |
| NEG-023 | Upsert — invalid date format | Bulk upsert with "not-a-date" in date column | FormatException | P1 |
| NEG-024 | Upload document — File null | POST upload-document without File | 400 Bad Request, "File is required" | P0 |
| NEG-025 | Upload document — 0-byte file | File.Length=0 | 400 Bad Request, "File is required" | P0 |
| NEG-026 | Upload document — EntityType empty | EntityType="" in request | 400 Bad Request | P0 |
| NEG-027 | Upload document — EntityId zero | EntityId=0 | 400 Bad Request | P0 |
| NEG-028 | Upload document — ArtifactTypeId zero | ArtifactTypeId=0 | 400 Bad Request | P0 |
| NEG-029 | Get document URL — artifact not found | No artifact for entity+type | GET document-url | 404 Not Found, "Artifact not found" | P0 |
| NEG-030 | Get document URL — artifact has no URL | Artifact exists but ValueText is null | GET document-url | 400 Bad Request, "Artifact does not have a document URL" | P0 |
| NEG-031 | Get list — entityId zero | GET /api/entity-artifacts/list?entityType=country&entityId=0 | 400 Bad Request | P0 |
| NEG-032 | Get list — entityType empty | entityType= | 400 Bad Request | P0 |
| NEG-033 | Bulk template — EntityType empty | POST bulk/template-download with EntityType="" | 400 Bad Request | P0 |
| NEG-034 | Bulk template — ArtifactTypeIds empty | ArtifactTypeIds=[] | 400 Bad Request, "At least one artifact type is required" | P0 |
| NEG-035 | Bulk template — ArtifactTypeIds null | ArtifactTypeIds=null | 400 Bad Request | P0 |
| NEG-036 | Bulk upsert — EntityType empty | POST bulk/upsert with EntityType="" | 400 Bad Request | P0 |
| NEG-037 | Bulk upsert — Rows empty | Rows=[] | 400 Bad Request, "No rows provided for import" | P0 |
| NEG-038 | Bulk upsert — Rows null | Rows=null | 400 Bad Request | P0 |
| NEG-039 | Bulk upsert — ColumnToArtifactTypeMapping empty | ColumnToArtifactTypeMapping={} | 400 Bad Request, "Column to artifact type mapping is required" | P0 |
| NEG-040 | Bulk upsert — entity not found by unique ID | UniqueId="XX" (country XX does not exist) | POST bulk/upsert | Row fails, ErrorMessage="Entity not found with unique identifier: XX" | P0 |
| NEG-041 | Bulk upsert — wrong column mapping | Column 1 maps to non-existent ArtifactTypeId | POST bulk/upsert | Cell or row fails | P1 |
| NEG-042 | Get unique ID example — unsupported entity type | entityType=opportunity | GET bulk/unique-id-example?entityType=opportunity | 500 or ArgumentException (opportunity not in switch) | P1 |
| NEG-043 | Get unique ID example — entityType empty | entityType= | 400 Bad Request | P0 |
| NEG-044 | Get bulk artifact types — entityType empty | GET bulk/artifact-types?entityType= | 400 Bad Request | P0 |
| NEG-045 | Upsert — no auth | No JWT | POST /api/entity-artifacts/upsert | 401 Unauthorized | P0 |
| NEG-046 | Upload — no auth | No JWT | POST upload-document | 401 Unauthorized | P0 |
| NEG-047 | Get document URL — no auth | No JWT | GET document-url | 401 Unauthorized | P0 |
| NEG-048 | Get list — no auth | No JWT | GET list | 401 Unauthorized | P0 |
| NEG-049 | Bulk template — no auth | No JWT | POST bulk/template-download | 401 Unauthorized | P0 |
| NEG-050 | Bulk upsert — no auth | No JWT | POST bulk/upsert | 401 Unauthorized | P0 |
| NEG-051 | Get entity records — no auth | No JWT | GET entity-records | 401 Unauthorized | P0 |
| NEG-052 | Get entity artifact — no auth | No JWT | GET get | 401 Unauthorized | P0 |
| NEG-053 | Get artifact types — no auth | No JWT | GET artifact-types | 401 Unauthorized | P0 |
| NEG-054 | Get bulk artifact types — no auth | No JWT | GET bulk/artifact-types | 401 Unauthorized | P0 |
| NEG-055 | Get unique ID example — no auth | No JWT | GET bulk/unique-id-example | 401 Unauthorized | P0 |
| NEG-056 | SQL injection in entityType | entityType="'; DROP TABLE EntityArtifact--" | GET artifact-types?entityType=... | Sanitized or parameterized, no SQL execution | P0 |
| NEG-057 | SQL injection in searchTerm | searchTerm="' OR 1=1--" | GET entity-records?entityType=country&searchTerm=... | Sanitized, no injection | P0 |
| NEG-058 | Get entity artifact — soft-deleted artifact | Artifact exists but IsDeleted=true | GET get | 200 OK, null (filtered out) | P0 |
| NEG-059 | Get list — soft-deleted artifacts excluded | Entity has 1 active, 1 deleted artifact | GET list | 200 OK, only 1 artifact in response | P0 |
| NEG-060 | Upsert — deleted entity (country) | Country 999 soft-deleted | POST upsert for that country | May succeed (FK may not check IsDeleted) or fail | P1 |
| NEG-061 | Bulk upsert — malformed CSV data | Rows with wrong column count | POST bulk/upsert with mismatched CellValues | Row/cell fails | P1 |
| NEG-062 | Bulk upsert — type mismatch number column | Number column has "abc" | Cell fails with FormatException | P1 |
| NEG-063 | Bulk upsert — type mismatch date column | Date column has "invalid" | Cell fails | P1 |
| NEG-064 | Bulk upsert — type mismatch boolean column | Boolean column has "2" | Cell fails | P1 |
| NEG-065 | Get entity artifact — entityType empty | entityType= | GET get | 400 Bad Request | P0 |
| NEG-066 | Get document URL — entityType empty | entityType= | 400 Bad Request | P0 |
| NEG-067 | Get document URL — entityId zero | entityId=0 | 400 Bad Request | P0 |
| NEG-068 | Get document URL — artifactTypeId zero | artifactTypeId=0 | 400 Bad Request | P0 |
| NEG-069 | Upload — GCS failure | GCS service unavailable | POST upload-document | 500 or BusinessException | P1 |
| NEG-070 | Get unique ID example — no countries | No Country records | GET unique-id-example?entityType=country | 500, "No country records found" | P1 |
| NEG-071 | Get unique ID example — no partners with ErpDimValue | No partners have ErpDimValue | GET unique-id-example?entityType=partner | 500, "No partner records with ERP dimension value found" | P1 |
| NEG-072 | Bulk upsert — opportunity entity type | entityType=opportunity (ResolveEntityIdFromUniqueIdAsync has no case) | POST bulk/upsert | Rows fail, Entity not found | P1 |
| NEG-073 | Upsert — request body null | POST upsert with null body | 400 Bad Request | P0 |
| NEG-074 | Bulk template — request body null | POST bulk/template-download with null | 400 Bad Request | P0 |
| NEG-075 | Bulk upsert — request body null | POST bulk/upsert with null | 400 Bad Request | P0 |
| NEG-076 | Get artifact types — wrong role | User has CanViewPartners but not PARTNER_GLOB_ADMIN | GET artifact-types | 403 Forbidden | P0 |
| NEG-077 | Get entity records — wrong role | User lacks PARTNER_GLOB_ADMIN | GET entity-records | 403 Forbidden | P0 |
| NEG-078 | Get entity artifact — wrong role | User lacks PARTNER_GLOB_ADMIN | GET get | 403 Forbidden | P0 |
| NEG-079 | Upsert — wrong role | User lacks PARTNER_GLOB_ADMIN | POST upsert | 403 Forbidden | P0 |
| NEG-080 | Upload — wrong role | User lacks PARTNER_GLOB_ADMIN | POST upload-document | 403 Forbidden | P0 |
| NEG-081 | Get document URL — wrong role | User lacks PARTNER_GLOB_ADMIN | GET document-url | 403 Forbidden | P0 |
| NEG-082 | Get list — wrong role | User lacks PARTNER_GLOB_ADMIN | GET list | 403 Forbidden | P0 |
| NEG-083 | Get bulk artifact types — wrong role | User lacks PARTNER_GLOB_ADMIN | GET bulk/artifact-types | 403 Forbidden | P0 |
| NEG-084 | Get unique ID example — wrong role | User lacks PARTNER_GLOB_ADMIN | GET bulk/unique-id-example | 403 Forbidden | P0 |
| NEG-085 | Bulk template — wrong role | User lacks PARTNER_GLOB_ADMIN | POST bulk/template-download | 403 Forbidden | P0 |
| NEG-086 | Bulk upsert — wrong role | User lacks PARTNER_GLOB_ADMIN | POST bulk/upsert | 403 Forbidden | P0 |
| NEG-087 | Upload — invalid file type (executable) | File with .exe extension | POST upload-document | 400 or rejected by GCS validation | P1 |
| NEG-088 | Upload — missing document URL from GCS | GCS returns empty URL | POST upload-document | 500, BusinessException "Failed to upload file to Google Cloud Storage" | P1 |
| NEG-089 | Get entity artifact — Status not Active | Artifact exists but Status=Inactive | GET get | 200 OK, null (filtered by Status==Active) | P1 |
| NEG-090 | Bulk upsert — invalid ArtifactTypeIds in mapping | ColumnToArtifactTypeMapping references non-existent ArtifactTypeId | POST bulk/upsert | 500 or KeyNotFoundException in artifactTypes dict | P1 |

---

## §3 Boundary Tests (90)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|--------------|-------|-----------------|----------|
| BND-001 | EntityType max length (100) | EntityType = 100-char string | POST upsert with EntityType of length 100 | 200 OK or 400 if validated | P1 |
| BND-002 | EntityType 101 chars | EntityType = 101 chars | POST upsert | 400 or DB truncation/error | P1 |
| BND-003 | Name max length (500) | Name = 500 chars | POST upsert with Name length 500 | 200 OK | P1 |
| BND-004 | Name 501 chars | Name = 501 chars | POST upsert | 400 or DB error | P1 |
| BND-005 | ValueNumber max (decimal 18,4) | ValueNumber = 9999999999999999.9999 | POST upsert | 200 OK, value persisted | P1 |
| BND-006 | ValueNumber min negative | ValueNumber = -9999999999999999.9999 | POST upsert | 200 OK | P1 |
| BND-007 | ValueNumber zero | ValueNumber = 0 | POST upsert | 200 OK | P1 |
| BND-008 | ValueNumber 4 decimal places | ValueNumber = 0.1234 | POST upsert | 200 OK, precision preserved | P1 |
| BND-009 | ValueNumber 5 decimal places | ValueNumber = 0.12345 | Rounded to 4 or rejected | P1 |
| BND-010 | ConfidenceScore 0.00 | ConfidenceScore = 0 | Artifact with ConfidenceScore 0 | 200 OK | P1 |
| BND-011 | ConfidenceScore 1.00 | ConfidenceScore = 1 | Artifact with ConfidenceScore 1 | 200 OK | P1 |
| BND-012 | ConfidenceScore 1.01 | ConfidenceScore = 1.01 (exceeds decimal(3,2)) | May fail DB constraint | P1 |
| BND-013 | ConfidenceScore -0.01 | ConfidenceScore = -0.01 | May fail validation | P1 |
| BND-014 | Source max length (255) | Source = 255 chars | POST upsert | 200 OK | P1 |
| BND-015 | Source 256 chars | Source = 256 chars | 400 or DB error | P1 |
| BND-016 | Empty string vs null — Name | Name="" vs Name=null | POST upsert with each | Both handled, null/empty stored per design | P1 |
| BND-017 | Empty string vs null — ValueText | ValueText="" vs null | Both handled | P1 |
| BND-018 | Empty string vs null — Source | Source="" vs null | Source defaults to "User Input" when null | P1 |
| BND-019 | Bulk upsert — 0 rows | Rows=[] | 400 Bad Request (already in NEG) | P1 |
| BND-020 | Bulk upsert — 1 row | Rows with 1 item | 200 OK, SuccessfulRows=1 | P0 |
| BND-021 | Bulk upsert — 100 rows | Rows with 100 items | 200 OK, all processed | P1 |
| BND-022 | Bulk upsert — 1000 rows | Rows with 1000 items | 200 OK or timeout, performance test | P1 |
| BND-023 | Search term empty | searchTerm="" | GET entity-records?entityType=country&searchTerm= | 200 OK, all records (no filter) | P1 |
| BND-024 | Search term null | searchTerm omitted | GET entity-records?entityType=country | 200 OK, all records | P1 |
| BND-025 | Search term special chars | searchTerm="%_[]" | GET entity-records | 200 OK, no SQL injection | P1 |
| BND-026 | Search term very long | searchTerm = 10000 chars | GET entity-records | 200 OK or 400 if length limited | P1 |
| BND-027 | Search term unicode | searchTerm="日本" | GET entity-records | 200 OK, matching if any | P1 |
| BND-028 | EffectiveDate min (DateTime.MinValue) | EffectiveDate=0001-01-01 | POST upsert | 200 OK or validation | P1 |
| BND-029 | EffectiveDate max (DateTime.MaxValue) | EffectiveDate=9999-12-31 | POST upsert | 200 OK or validation | P1 |
| BND-030 | EffectiveDate > ExpiryDate | EffectiveDate=2025-12-31, ExpiryDate=2025-01-01 | POST upsert | 200 OK (manager doesn't validate order) | P1 |
| BND-031 | EffectiveDate = ExpiryDate | Same date | POST upsert | 200 OK | P1 |
| BND-032 | ArtifactTypeId = 0 | artifactTypeId=0 | GET get | 400 Bad Request | P0 |
| BND-033 | ArtifactTypeId = -1 | artifactTypeId=-1 | 400 Bad Request | P1 |
| BND-034 | ArtifactTypeId = int.MaxValue | artifactTypeId=2147483647 | GET get | 200 OK null or 500 | P1 |
| BND-035 | EntityId = 1 (min valid) | entityId=1 | GET get?entityId=1 | 200 OK | P0 |
| BND-036 | EntityId = int.MaxValue | entityId=2147483647 | GET get | 200 OK null (likely no entity) | P1 |
| BND-037 | ValueJson empty object | ValueJson="{}" | POST upsert | 200 OK | P1 |
| BND-038 | ValueJson large (10KB) | ValueJson with 10KB string | POST upsert | 200 OK or size limit | P1 |
| BND-039 | ValueText empty | ValueText="" | POST upsert | 200 OK | P1 |
| BND-040 | ValueText very long (64KB) | ValueText = 64KB | POST upsert | 200 OK or limit | P1 |
| BND-041 | ValueBoolean true | ValueBoolean=true | POST upsert | 200 OK | P0 |
| BND-042 | ValueBoolean false | ValueBoolean=false | POST upsert | 200 OK | P0 |
| BND-043 | ValueDate UTC | ValueDate with Kind=Utc | POST upsert | 200 OK, stored correctly | P1 |
| BND-044 | ValueDate local | ValueDate with Kind=Local | Converted to UTC per manager logic | P1 |
| BND-045 | Metadata null | Metadata=null | POST upsert | 200 OK | P1 |
| BND-046 | Metadata JSON | Metadata="{\"key\":\"value\"}" | POST upsert | 200 OK | P1 |
| BND-047 | DocumentId null | DocumentId=null | POST upsert | 200 OK | P1 |
| BND-048 | DocumentId valid FK | DocumentId=5 (Document exists) | POST upsert | 200 OK | P1 |
| BND-049 | DocumentId invalid FK | DocumentId=99999 (no Document) | POST upsert | May fail FK or succeed if not validated | P1 |
| BND-050 | entityType case — country vs Country | entityType=country vs Country | GET artifact-types?entityType=Country | 200 OK (Contains is case-sensitive, may return empty) | P1 |
| BND-051 | entityType lowercase | entityType=country (lowercase) | Manager uses ToLower() in switch | 200 OK | P1 |
| BND-052 | Bulk template — 1 artifact type | ArtifactTypeIds=[5] | POST template-download | 200 OK, CSV with UniqueId + 1 column | P1 |
| BND-053 | Bulk template — 20 artifact types | ArtifactTypeIds with 20 ids | POST template-download | 200 OK, 21 columns | P1 |
| BND-054 | Column mapping — single column | ColumnToArtifactTypeMapping={0:5} | POST bulk/upsert | 200 OK | P1 |
| BND-055 | Column mapping — many columns | 10 columns mapped | POST bulk/upsert | 200 OK | P1 |
| BND-056 | CellValues — missing column index | Row has CellValues={0:"x"} but mapping has 1 | Cell for 1 may be empty, skipped | P1 |
| BND-057 | CellValues — extra column index | Row has CellValues={0:"x",1:"y",2:"z"} but mapping has 0,1 | Column 2 ignored | P1 |
| BND-058 | UniqueId — whitespace only | UniqueId="   " | Row fails, entity not found | P1 |
| BND-059 | UniqueId — leading/trailing spaces | UniqueId=" NO " (country "NO") | Trimmed or exact match, may fail | P1 |
| BND-060 | Boolean "1" and "0" | Bulk upsert ValueText "1", "0" for boolean | TryParseFlexibleBoolean accepts | 200 OK | P1 |
| BND-061 | Boolean "TRUE" "FALSE" | Case variations | Parsed correctly | P1 |
| BND-062 | Number with comma decimal | "0,95" (European format) | decimal.TryParse with InvariantCulture | May parse or fail | P1 |
| BND-063 | Number with leading zeros | "007" | Parsed as 7 | P1 |
| BND-064 | Date format ISO | "2025-01-15" | DateTime.TryParse | 200 OK | P1 |
| BND-065 | Date format with time | "2025-01-15T12:00:00" | Parsed, time may be stripped for date type | P1 |
| BND-066 | Empty cell — no existing artifact | Cell value empty, no artifact | Skipped, nothing created | P1 |
| BND-067 | Empty cell — existing artifact | Cell value empty, artifact exists | Skipped, previous value retained | P1 |
| BND-068 | Value unchanged — same text | Bulk row has same ValueText as existing | Skipped, "Value unchanged" in cell result | P1 |
| BND-069 | Value unchanged — same number | Same ValueNumber | Skipped | P1 |
| BND-070 | Value unchanged — same date | Same ValueDate (date part) | Skipped | P1 |
| BND-071 | ApplicableEntityTypes contains substring | ArtifactType ApplicableEntityTypes="country,country_region" | entityType=count (substring) | Should not match (Contains) | P1 |
| BND-072 | ApplicableEntityTypes exact match | entityType=country, ApplicableEntityTypes="country" | Match | P1 |
| BND-073 | Multiple artifacts same entity+type | Two artifacts (one soft-deleted) | GetEntityArtifact returns latest non-deleted | 200 OK, one artifact | P1 |
| BND-074 | OrderBy CreatedDate desc | Multiple artifacts | GetEntityArtifact OrderByDescending(CreatedDate) | Most recent returned | P1 |
| BND-075 | GetEntityArtifacts Order | Artifacts by ArtifactType.Order, then CreatedDate | GET list | Ordered correctly | P1 |
| BND-076 | File size 1 byte | Upload 1-byte file | POST upload-document | 200 OK (min valid) | P1 |
| BND-077 | File size 5MB | Upload 5MB file | 200 OK or size limit | P1 |
| BND-078 | File name with spaces | File.FileName="my document.pdf" | Sanitized for GCS | 200 OK | P1 |
| BND-079 | File name with special chars | File.FileName="file%20#1.pdf" | SanitizeFileName replaces | 200 OK | P1 |
| BND-080 | CSV header with comma | Header value contains comma | EscapeCsvValue wraps in quotes | 200 OK | P1 |
| BND-081 | CSV value with quote | Value contains " | Escaped as "" | P1 |
| BND-082 | CSV value with newline | Value contains \n | Wrapped in quotes | P1 |
| BND-083 | Document artifact excluded from bulk | ArtifactType DataType=document | Filtered out of ColumnToArtifactTypeMapping | Document column skipped | P1 |
| BND-084 | ChangeTracker detach on cell error | One cell fails in bulk | Failed entries detached, next row continues | P1 |
| BND-085 | entityType "organization" vs "partner" | Both map to Partner in GetEntityRecords | Same result | P1 |
| BND-086 | entityType "organizationhierarchy" vs "orgunit" | Both map to OrganizationHierarchy | Same result | P1 |
| BND-087 | GetEntityRecords default — no search | searchTerm=null | All entities returned | P1 |
| BND-088 | Name required for ModifiableDeletableEntity | EntityArtifact inherits, Name nullable | AddAsync sets Name from request.Name or "" | P1 |
| BND-089 | ArtifactType Include ArtifactDataType | GetArtifactTypes includes ArtifactDataType | DataTypeName in response | P1 |
| BND-090 | ArtifactType Include for GetEntityArtifact | GetEntityArtifact includes ArtifactType, ArtifactDataType | Full response with type info | P1 |

---

## §4 Functional Tests (90)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|--------------|-------|-----------------|----------|
| FUN-001 | Upsert creates new when none exists | No artifact for entity+type | POST upsert | New EntityArtifact created, Id assigned | P0 |
| FUN-002 | Upsert updates when exists | Artifact exists | POST upsert with new ValueText | Existing artifact updated, same Id | P0 |
| FUN-003 | Upsert idempotent — same data twice | Artifact exists with ValueText="X" | POST upsert ValueText="X" twice | Second call updates, no duplicate | P0 |
| FUN-004 | Document artifact links ValueText to GCS URL | Upload document | UpsertDocumentArtifact stores documentUrl in ValueText | ValueText contains gs:// or https:// | P0 |
| FUN-005 | Document artifact ValueJson has file metadata | Upload PDF | ValueJson contains fileName, mimeType, fileSize, uploadedAt | P0 |
| FUN-006 | Bulk upsert creates multiple artifacts | 3 rows, 3 countries | POST bulk/upsert | 3 artifacts created/updated | P0 |
| FUN-007 | Bulk upsert atomic per row | Row 1 succeeds, row 2 fails | POST bulk/upsert | Row 1 persisted, row 2 in FailedRows | P1 |
| FUN-008 | Get artifacts filters by EntityType | Country 1 and Partner 1 both have artifacts | GET list?entityType=country&entityId=1 | Only country 1 artifacts | P0 |
| FUN-009 | Get artifacts filters by EntityId | Country 1 and 2 have artifacts | GET list?entityType=country&entityId=1 | Only country 1 artifacts | P0 |
| FUN-010 | Artifact types filtered by entity type | ArtifactType ApplicableEntityTypes="country,partner" | GetArtifactTypesByEntityTypeAsync("country") | Only types containing "country" | P0 |
| FUN-011 | Bulk artifact types filtered by AllowBulkUpdate | Some types AllowBulkUpdate=false | GetBulkUpdateArtifactTypesByEntityTypeAsync | Only AllowBulkUpdate=true | P0 |
| FUN-012 | Search term filters entity records | Countries "Norway", "Kenya" exist | GET entity-records?entityType=country&searchTerm=Nor | Only Norway | P0 |
| FUN-013 | Search term filters contacts by FirstName | Contact "John Doe" | GET entity-records?entityType=contact&searchTerm=John | John Doe in results | P1 |
| FUN-014 | Search term filters contacts by Email | searchTerm=john@example.com | Contact with that email returned | P1 |
| FUN-015 | Country tag World_Bank_Fragile_Situation | Artifact World_Bank_Fragile_Situation=true | CountryModel.CalculateConditionalTags | "Fragile State" (red) tag | P1 |
| FUN-016 | Country tag SIDS | Artifact SIDS=true | "SIDS" (yellow) tag | P1 |
| FUN-017 | Country tag Host_Agreement present | Artifact Host_Agreement indicates present | "HCA Present" (green) tag | P1 |
| FUN-018 | Country tag Host_Agreement not present | Artifact Host_Agreement indicates not present | "HCA Not Present" (yellow) tag | P1 |
| FUN-019 | Bulk template column headers | ArtifactTypes 1,2,3 | GenerateBulkTemplateAsync | Header: UniqueIdFieldLabel, ArtifactType1.Name, ArtifactType2.Name, ArtifactType3.Name | P0 |
| FUN-020 | Bulk template data type hints row | Second row has data type hints | Example: {ExampleValue}, (string), (number), (date) | P1 |
| FUN-021 | Soft-delete — GetEntityArtifact excludes deleted | Artifact IsDeleted=true | GetEntityArtifactAsync | null returned | P0 |
| FUN-022 | Soft-delete — GetEntityArtifacts excludes deleted | One artifact deleted | GetEntityArtifactsAsync | Deleted not in list | P0 |
| FUN-023 | Soft-delete — Upsert does not revive deleted | Artifact IsDeleted=true, same entity+type | Upsert creates NEW artifact (existing filtered out) | New artifact created | P1 |
| FUN-024 | Value type coercion — number from string | ArtifactType number, cell "123.45" | Bulk upsert | ValueNumber=123.45 | P0 |
| FUN-025 | Value type coercion — boolean from "1" | Cell "1" for boolean | ValueBoolean=true | P0 |
| FUN-026 | Value type coercion — boolean from "0" | Cell "0" | ValueBoolean=false | P0 |
| FUN-027 | Value type coercion — date from string | Cell "2025-01-15" | ValueDate parsed, UTC | P0 |
| FUN-028 | Audit CreatedBy populated | Create artifact | CreatedBy set to current user | P1 |
| FUN-029 | Audit CreatedDate populated | Create artifact | CreatedDate set | P1 |
| FUN-030 | Audit LastModifiedBy on update | Update artifact | LastModifiedBy set | P1 |
| FUN-031 | Audit LastModifiedDate on update | Update artifact | LastModifiedDate set | P1 |
| FUN-032 | Source default "User Input" | Upsert without Source | Source="User Input" | P0 |
| FUN-033 | Source "Bulk Import" on bulk upsert | Bulk upsert | Source="Bulk Import" in created artifacts | P0 |
| FUN-034 | ResolveEntityId — country by Iso2Code | Country Iso2Code="NO" | ResolveEntityIdFromUniqueIdAsync("country","NO") | Country.Id | P0 |
| FUN-035 | ResolveEntityId — partner by ErpDimValue | Partner ErpDimValue=100 | ResolveEntityIdFromUniqueIdAsync("partner","100") | Partner.Id | P0 |
| FUN-036 | ResolveEntityId — orgunit by Code | OrgUnit Code="HQ" | ResolveEntityIdFromUniqueIdAsync("orgunit","HQ") | OrgUnit.Id | P0 |
| FUN-037 | ResolveEntityId — contact by Email | Contact Email="a@b.com" | ResolveEntityIdFromUniqueIdAsync("contact","a@b.com") | Contact.Id | P0 |
| FUN-038 | GetEntityName — country | EntityId=1, entityType=country | GetEntityNameAsync returns Country.Name | P1 |
| FUN-039 | GetEntityName — partner | entityType=partner | Partner.Name | P1 |
| FUN-040 | GetDisplayValue — text | Artifact ValueText="x" | GetDisplayValue returns "x" | P1 |
| FUN-041 | GetDisplayValue — number | Artifact ValueNumber=0.95 | Returns "0.95" | P1 |
| FUN-042 | GetDisplayValue — boolean | ValueBoolean=true | Returns "True" | P1 |
| FUN-043 | GetDisplayValue — date | ValueDate=2025-01-15 | Returns "2025-01-15" | P1 |
| FUN-044 | GetDisplayValue — json | ValueJson="{}" | Returns "{}" | P1 |
| FUN-045 | Bulk row result — EntityId, EntityName | Row succeeds | RowResult has EntityId, EntityName | P1 |
| FUN-046 | Bulk cell result — IsNew | New artifact | cellResult.IsNew=true | P1 |
| FUN-047 | Bulk cell result — IsNew false | Update existing | cellResult.IsNew=false | P1 |
| FUN-048 | Bulk cell result — PreviousValue | Existing artifact | cellResult.PreviousValue=GetDisplayValue(existing) | P1 |
| FUN-049 | Bulk cell result — CurrentValue | After upsert | cellResult.CurrentValue=GetDisplayValue(result) | P1 |
| FUN-050 | Bulk cell result — Skipped empty | Empty cell, existing artifact | cellResult.Skipped=true, CurrentValue=PreviousValue | P1 |
| FUN-051 | Bulk cell result — Skipped unchanged | Same value | cellResult.Skipped=true, ErrorMessage="Value unchanged" | P1 |
| FUN-052 | Bulk response TotalRows | 10 rows | TotalRows=10 | P1 |
| FUN-053 | Bulk response SuccessfulRows | 8 succeed, 2 fail | SuccessfulRows=8, FailedRows=2 | P1 |
| FUN-054 | Bulk response RowResults | Each row has RowResult | RowResults.Count=10 | P1 |
| FUN-055 | GetAvailableEntityTypes — unique | Duplicate entity types in ArtifactTypes | HashSet ensures unique | P1 |
| FUN-056 | GetAvailableEntityTypes — ordered | Entity types | OrderBy(et) | P1 |
| FUN-057 | GetEntityRecords — country ordered by Name | Countries | OrderBy(c => c.Name) | P1 |
| FUN-058 | GetEntityRecords — partner ordered by Name | Partners | OrderBy(p => p.Name) | P1 |
| FUN-059 | GetEntityRecords — contact Name FirstName+LastName | Contact | Name = FirstName + " " + LastName | P1 |
| FUN-060 | GetEntityRecords — contact Description Email | Contact | Description = Email | P1 |
| FUN-061 | GetEntityRecords — orgunit Description Code+Type | OrgUnit | Description = Code + " - " + Type | P1 |
| FUN-062 | GetEntityRecords — opportunity Description | Opportunity | Description = Opportunity.Description | P1 |
| FUN-063 | GetEntityRecords — default returns empty | entityType=unsupported | Empty list | P1 |
| FUN-064 | UniqueId example — country Iso2Code | Country with Iso2Code | UniqueIdFieldName=Iso2Code, ExampleValue=Iso2Code | P1 |
| FUN-065 | UniqueId example — partner ErpDimValue | Partner with ErpDimValue | UniqueIdFieldName=ErpDimValue | P1 |
| FUN-066 | UniqueId example — orgunit Code | OrgUnit | UniqueIdFieldName=Code | P1 |
| FUN-067 | UniqueId example — contact Email | Contact | UniqueIdFieldName=Email | P1 |
| FUN-068 | Upsert existing — all value fields updated | Existing artifact | Name, ValueText, ValueNumber, ValueBoolean, ValueDate, ValueJson, EffectiveDate, ExpiryDate, Source, Metadata updated | P0 |
| FUN-069 | Upsert new — Status=Active | New artifact | Status=EntityStatus.Active | P0 |
| FUN-070 | Upsert new — IsExtracted=false | New artifact | IsExtracted=false | P0 |
| FUN-071 | Document artifact Name from request or fileName | request.Name=null | Name=fileName | P1 |
| FUN-072 | Document artifact Name from request | request.Name="Custom" | Name="Custom" | P1 |
| FUN-073 | Document artifact clears non-document values | UpsertDocumentArtifact | ValueNumber=null, ValueBoolean=null, ValueDate=null | P1 |
| FUN-074 | GetEntityArtifact includes Document | Artifact has DocumentId | Response has DocumentName | P1 |
| FUN-075 | GetEntityArtifact includes ArtifactType | All artifacts | ArtifactTypeName, ArtifactTypeCode, DataTypeName in response | P1 |
| FUN-076 | ArtifactType Contains entityType | ApplicableEntityTypes="country,partner" | Contains("country") true | P1 |
| FUN-077 | ArtifactType Order, ThenBy Name | Multiple types | OrderBy(Order).ThenBy(Name) | P1 |
| FUN-078 | EscapeCsvValue — no special chars | "simple" | Returns "simple" | P1 |
| FUN-079 | EscapeCsvValue — contains comma | "a,b" | Returns "\"a,b\"" | P1 |
| FUN-080 | EscapeCsvValue — contains quote | "a\"b" | Returns "\"a\"\"b\"" | P1 |
| FUN-081 | TryParseFlexibleBoolean — "true" | "true" | result=true | P1 |
| FUN-082 | TryParseFlexibleBoolean — "false" | "false" | result=false | P1 |
| FUN-083 | TryParseFlexibleBoolean — "1" | "1" | result=true | P1 |
| FUN-084 | TryParseFlexibleBoolean — "0" | "0" | result=false | P1 |
| FUN-085 | TryParseFlexibleBoolean — "maybe" | "maybe" | returns false, parse fails | P1 |
| FUN-086 | Template artifact type order | ArtifactTypeIds=[3,1,2] | Columns in order 3,1,2 per IndexOf | P1 |
| FUN-087 | Template UTF-8 encoding | GenerateBulkTemplateAsync | Encoding.UTF8.GetBytes | P1 |
| FUN-088 | GCS folder path format | Upload for country 1, HDI_Index | entityartifacts/hdi_index/country/1/ | P1 |
| FUN-089 | GCS unique filename | Upload same file twice | Unique filename with Guid | P1 |
| FUN-090 | Signed URL expiry | Get document URL for gs:// | Signed URL with 60 min expiry | P1 |

---

## §5 Integration Tests (90)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|--------------|-------|-----------------|----------|
| INT-001 | Controller→Manager→DB — GetEntityTypes | Full stack | GET /api/entity-artifacts/entity-types | 200 OK, entity types from ArtifactType table | P0 |
| INT-002 | Controller→Manager→DB — GetArtifactTypes | Full stack | GET artifact-types?entityType=country | 200 OK, from ArtifactType filtered by ApplicableEntityTypes | P0 |
| INT-003 | Controller→Manager→DB — GetEntityRecords | Full stack | GET entity-records?entityType=country | 200 OK, from Country table | P0 |
| INT-004 | Controller→Manager→DB — GetEntityArtifact | Full stack | GET get?entityType=country&entityId=1&artifactTypeId=5 | 200 OK, from EntityArtifact | P0 |
| INT-005 | Controller→Manager→DB — Upsert | Full stack | POST upsert | 200 OK, EntityArtifact saved to DB | P0 |
| INT-006 | Controller→Manager→DB — Upload document | Full stack | POST upload-document | 200 OK, GCS upload + EntityArtifact saved | P0 |
| INT-007 | Controller→Manager→DB — Get document URL | Full stack | GET document-url | 200 OK, signed URL from GCS | P0 |
| INT-008 | Controller→Manager→DB — Get list | Full stack | GET list | 200 OK, artifacts from DB | P0 |
| INT-009 | Controller→Manager→DB — Get bulk artifact types | Full stack | GET bulk/artifact-types | 200 OK | P0 |
| INT-010 | Controller→Manager→DB — Get unique ID example | Full stack | GET bulk/unique-id-example | 200 OK | P0 |
| INT-011 | Controller→Manager→DB — Template download | Full stack | POST bulk/template-download | 200 OK, CSV file | P0 |
| INT-012 | Controller→Manager→DB — Bulk upsert | Full stack | POST bulk/upsert | 200 OK, BulkEntityArtifactResponse | P0 |
| INT-013 | EntityArtifact→ArtifactType relationship | Artifact has ArtifactTypeId | Include(ArtifactType), ArtifactType loaded | P0 |
| INT-014 | EntityArtifact→ArtifactType→ArtifactDataType | Include ArtifactDataType | DataTypeName in response | P0 |
| INT-015 | EntityArtifact→Document relationship | Artifact has DocumentId | Include(Document), DocumentName in response | P0 |
| INT-016 | Country artifacts→CalculateConditionalTags | Country has HDI, FSI, World_Bank_Fragile | CountryService/CountryModel uses artifacts for tags | P1 |
| INT-017 | Country DynamicSearchCountries IncludeArtifacts | IncludeArtifacts=true | Country artifacts loaded | P1 |
| INT-018 | Bulk upsert→individual artifact verification | Bulk upsert 3 rows | GET list for each entity, artifacts exist | P0 |
| INT-019 | Document upload→Google Cloud Storage | Upload PDF | GCS has file at entityartifacts/{code}/{entity}/{id}/ | P0 |
| INT-020 | Admin UI→EntityArtifactController | User in admin UI | All 12 endpoints callable from UI | P1 |
| INT-021 | EntityArtifactRepository GetAll | Repository filters IsDeleted | GetAll() excludes soft-deleted per base | P1 |
| INT-022 | ArtifactTypeRepository GetAll | ArtifactType | No IsDeleted (ArtifactType may not have it) | P1 |
| INT-023 | DataRepository AddAsync | Add EntityArtifact | AuditableDbContext sets CreatedBy, CreatedDate | P1 |
| INT-024 | DataRepository UpdateAsync | Update EntityArtifact | LastModifiedBy, LastModifiedDate set | P1 |
| INT-025 | AutoMapper EntityArtifact→Response | Manager builds response manually | No AutoMapper for EntityArtifact (manual mapping) | P1 |
| INT-026 | CheckRoleAuthorizationAsync PARTNER_GLOB_ADMIN | All endpoints | BaseController checks role before logic | P0 |
| INT-027 | IManagerWrapper EntityArtifactManager | Controller gets manager | _manager = manager.EntityArtifactManager | P1 |
| INT-028 | GoogleCloudStorageService UploadFileAsync | Upload document | GCS service called with file, path | P1 |
| INT-029 | GoogleCloudStorageService GetSignedUrlFromGsUri | document-url for gs:// | Signed URL generated | P1 |
| INT-030 | GoogleCloudStorageService GenerateSignedUrlFromStorageUrl | document-url for https://storage. | Signed URL | P1 |
| INT-031 | Country entity in GetEntityRecords | entityType=country | context.Set<Country>() | P1 |
| INT-032 | Partner entity in GetEntityRecords | entityType=partner | context.Set<Partner>() | P1 |
| INT-033 | Contact entity in GetEntityRecords | entityType=contact | context.Set<Contact>() | P1 |
| INT-034 | OrganizationHierarchy in GetEntityRecords | entityType=orgunit | context.Set<OrganizationHierarchy>() | P1 |
| INT-035 | Opportunity entity in GetEntityRecords | entityType=opportunity | context.Set<Opportunity>() | P1 |
| INT-036 | ResolveEntityId Country→Country.Id | Iso2Code lookup | context.Set<Country>().Where(c=>c.Iso2Code==uniqueId) | P1 |
| INT-037 | ResolveEntityId Partner→Partner.Id | ErpDimValue lookup | context.Set<Partner>().Where(p=>p.ErpDimValue==erpDimValue) | P1 |
| INT-038 | ResolveEntityId OrgUnit→OrgHierarchy.Id | Code lookup | context.Set<OrganizationHierarchy>().Where(o=>o.Code==uniqueId) | P1 |
| INT-039 | ResolveEntityId Contact→Contact.Id | Email lookup | context.Set<Contact>().Where(c=>c.Email==uniqueId) | P1 |
| INT-040 | Bulk upsert calls UpsertEntityArtifactAsync | Per cell | Each cell upsert invokes manager Upsert | P1 |
| INT-041 | Bulk upsert calls GetEntityArtifactAsync | Per cell | Check existing before upsert | P1 |
| INT-042 | Bulk upsert calls ResolveEntityIdFromUniqueIdAsync | Per row | Entity ID resolved from UniqueId | P1 |
| INT-043 | Bulk upsert calls GetEntityNameAsync | Per row | EntityName for display | P1 |
| INT-044 | Template calls GetUniqueIdExampleAsync | GenerateBulkTemplate | UniqueIdFieldLabel, ExampleValue in CSV | P1 |
| INT-045 | Template calls ArtifactTypeRepository | Get artifact types by ids | ArtifactType names in headers | P1 |
| INT-046 | Upload calls GetArtifactTypeCodeAsync | When ArtifactTypeCode empty in request | Code for folder path | P1 |
| INT-047 | Upload builds folder path | entityartifacts/{code}/{entity}/{id}/ | Lowercase code, entity | P1 |
| INT-048 | SanitizeFileName | Filename with special chars | Spaces, %, #, &, ?, +, = replaced with _ | P1 |
| INT-049 | Error handling — 500 on exception | Manager throws | Controller returns StatusCode(500) | P1 |
| INT-050 | Error handling — BusinessException on upload | GCS fails | BadRequest with message | P1 |
| INT-051 | Error handling — 404 on artifact not found | Get document-url, no artifact | NotFound | P1 |
| INT-052 | API route entity-types | APIDictionary.EntityArtifactEntityTypes | /api/entity-artifacts/entity-types | P1 |
| INT-053 | API route artifact-types | APIDictionary.EntityArtifactTypes | /api/entity-artifacts/artifact-types | P1 |
| INT-054 | API route entity-records | APIDictionary.EntityArtifactRecords | /api/entity-artifacts/entity-records | P1 |
| INT-055 | API route get | APIDictionary.EntityArtifactGet | /api/entity-artifacts/get | P1 |
| INT-056 | API route upsert | APIDictionary.EntityArtifactUpsert | /api/entity-artifacts/upsert | P1 |
| INT-057 | API route upload-document | APIDictionary.EntityArtifactUploadDocument | /api/entity-artifacts/upload-document | P1 |
| INT-058 | API route document-url | APIDictionary.EntityArtifactDocumentUrl | /api/entity-artifacts/document-url | P1 |
| INT-059 | API route list | APIDictionary.EntityArtifactList | /api/entity-artifacts/list | P1 |
| INT-060 | API route bulk/artifact-types | APIDictionary.EntityArtifactBulkArtifactTypes | /api/entity-artifacts/bulk/artifact-types | P1 |
| INT-061 | API route bulk/unique-id-example | APIDictionary.EntityArtifactBulkUniqueIdExample | /api/entity-artifacts/bulk/unique-id-example | P1 |
| INT-062 | API route bulk/template-download | APIDictionary.EntityArtifactBulkTemplateDownload | /api/entity-artifacts/bulk/template-download | P1 |
| INT-063 | API route bulk/upsert | APIDictionary.EntityArtifactBulkUpsert | /api/entity-artifacts/bulk/upsert | P1 |
| INT-064 | EntityArtifact ModifiableDeletableEntity | Entity inherits | Id, Name, Status, IsDeleted, CreatedBy, etc. | P1 |
| INT-065 | EntityArtifact EntityType max 100 | [MaxLength(100)] | DB column length | P1 |
| INT-066 | EntityArtifact Name max 500 | [MaxLength(500)] | DB column length | P1 |
| INT-067 | EntityArtifact Source max 255 | [MaxLength(255)] | DB column length | P1 |
| INT-068 | EntityArtifact ValueNumber decimal(18,4) | [Column(TypeName)] | DB type | P1 |
| INT-069 | EntityArtifact ConfidenceScore decimal(3,2) | [Column(TypeName)] | DB type | P1 |
| INT-070 | EntityArtifact DocumentId FK | DocumentId nullable | FK to Document | P1 |
| INT-071 | EntityArtifact ArtifactTypeId FK | ArtifactTypeId required | FK to ArtifactType | P1 |
| INT-072 | EntityArtifactManager constructor | IMapper, AppDbContext | Repositories initialized | P1 |
| INT-073 | EntityArtifactService Angular→API | Frontend service | HTTP calls to all endpoints | P1 |
| INT-074 | EntityArtifactManagerComponent→Service | Admin UI component | Load entity types, artifact types, records | P1 |
| INT-075 | BulkEntityArtifactUpdateComponent→API | Bulk update UI | Template download, bulk upsert | P1 |
| INT-076 | File upload Content-Type | multipart/form-data | Controller accepts [FromForm] | P1 |
| INT-077 | CSV Content-Type response | template-download | File(csvBytes, "text/csv", fileName) | P1 |
| INT-078 | IAP authentication scheme | [Authorize(AuthenticationSchemes = "IAP")] | All endpoints require IAP | P0 |
| INT-079 | BaseController CheckRoleAuthorizationAsync | Inherited | Returns 403 if role check fails | P0 |
| INT-080 | Logger on error | _logger.LogError | Exceptions logged | P1 |
| INT-081 | Logger on upload success | _logger.LogInformation | GCS URL logged | P1 |
| INT-082 | EntityStatus.Active filter | GetEntityArtifact, GetEntityArtifacts | Status==Active | P1 |
| INT-083 | IsDeleted filter | All artifact queries | !ea.IsDeleted | P1 |
| INT-084 | ArtifactType Contains vs Split | ApplicableEntityTypes "country,partner" | Contains("country") matches | P1 |
| INT-085 | GetEntityRecords case-insensitive | entityType=Country | ToLower() in switch | P1 |
| INT-086 | ResolveEntityId case | entityType=COUNTRY | ToLower() in switch | P1 |
| INT-087 | GetUniqueIdExample case | entityType=PARTNER | ToLower() in switch | P1 |
| INT-088 | Partner ErpDimValue int parse | UniqueId="100" | int.TryParse for partner/organization | P1 |
| INT-089 | Contact Name format | FirstName + " " + LastName | GetEntityRecords, GetEntityNameAsync | P1 |
| INT-090 | OrgUnit Description format | Code + " - " + Type | GetEntityRecords | P1 |

---

## §6 Security Tests (50) — OUT OF SCOPE FOR QA

Security tests are out of scope for QA per project guidelines. The following areas would be covered by security-focused testing:

- Authentication (401) and authorization (403) for all 12 endpoints
- SQL injection in entityType, searchTerm parameters
- XSS in artifact values (Name, ValueText, ValueJson, Metadata)
- IDOR and privilege escalation
- File upload validation (type, size, path traversal)
- Mass assignment
- Token validation (expired, tampered, wrong issuer)

**Count:** 50 placeholder | **Status:** OUT OF SCOPE

---

## §7 Concurrency Tests (25)

| ID | Test Name | Precondition | Steps | Expected Result | Priority |
|----|-----------|--------------|-------|-----------------|----------|
| CON-001 | Concurrent upserts same entity+artifactType | Two users, same country 1, artifactType 5 | User A and B POST upsert simultaneously | One creates, one updates; or last write wins; no duplicate | P0 |
| CON-002 | Bulk upsert while individual upsert | User A bulk upsert, User B single upsert same entity | Both complete; consistent final state | P1 |
| CON-003 | Multiple admins editing artifacts | 3 users upsert different artifact types for country 1 | All succeed, no corruption | P1 |
| CON-004 | Concurrent GetEntityArtifact | 10 users GET same artifact | All 200 OK, same data | P1 |
| CON-005 | Concurrent GetEntityArtifacts list | 10 users GET list same entity | All 200 OK | P1 |
| CON-006 | Concurrent GetEntityRecords | 10 users GET entity-records | All 200 OK | P1 |
| CON-007 | Concurrent GetArtifactTypes | 10 users GET artifact-types | All 200 OK | P1 |
| CON-008 | Upsert and Get same artifact | User A upserts, User B gets | Get may return old or new depending on timing | P1 |
| CON-009 | Two bulk upserts same entity | User A and B bulk upsert overlapping rows | Both complete; last write wins per cell | P1 |
| CON-010 | Upload and Get document URL | User A uploads, User B gets document-url | Get may 404 until upload completes | P1 |
| CON-011 | Concurrent template downloads | 5 users POST template-download | All 200 OK, same CSV structure | P1 |
| CON-012 | Update and list | User A upserts, User B GET list | List may or may not include update | P1 |
| CON-013 | DbContext concurrency | Parallel manager calls | No DbContext disposed/concurrency errors | P1 |
| CON-014 | Transaction isolation — upsert rollback | Upsert fails mid-save | No partial artifact | P1 |
| CON-015 | Double submit — upsert | User double-clicks save | One artifact, no duplicate | P1 |
| CON-016 | Double submit — bulk upsert | User double-clicks import | One import applied | P1 |
| CON-017 | Concurrent unique ID example | 5 users GET unique-id-example | All 200 OK | P1 |
| CON-018 | Bulk upsert row isolation | Row 1 and row 2 same entity, different columns | Both cells updated correctly | P1 |
| CON-019 | ChangeTracker detach on concurrent error | One bulk row fails | Other rows unaffected | P1 |
| CON-020 | Upload same file twice | Two uploads for same entity+type | Second overwrites first (upsert) | P1 |
| CON-021 | GetEntityArtifact OrderByDescending CreatedDate | Two artifacts, concurrent create | Most recent returned | P1 |
| CON-022 | Soft delete during upsert | Artifact soft-deleted while another upserts | Upsert creates new (existing filtered) | P1 |
| CON-023 | Concurrent GetEntityRecords with search | 5 users search different terms | All succeed | P1 |
| CON-024 | Bulk template during artifact type update | Admin updates ArtifactType while user downloads template | Template reflects one state | P1 |
| CON-025 | Parallel GetEntityArtifact for different entities | 10 GET requests for 10 different entity+type combos | All 200 OK | P1 |

---

## §8 Unit Tests (21)

| ID | Test Name | Category | Input | Expected Output | Priority |
|----|-----------|----------|-------|-----------------|----------|
| UNT-001 | TryParseFlexibleBoolean "true" | Value parsing | "true" | true | P1 |
| UNT-002 | TryParseFlexibleBoolean "false" | Value parsing | "false" | false | P1 |
| UNT-003 | TryParseFlexibleBoolean "1" | Value parsing | "1" | true | P1 |
| UNT-004 | TryParseFlexibleBoolean "0" | Value parsing | "0" | false | P1 |
| UNT-005 | TryParseFlexibleBoolean "yes" | Value parsing | "yes" | false (parse fails) | P1 |
| UNT-006 | TryParseFlexibleBoolean empty | Value parsing | "" | false (parse fails) | P1 |
| UNT-007 | EscapeCsvValue simple | Template generation | "hello" | "hello" | P1 |
| UNT-008 | EscapeCsvValue with comma | Template generation | "a,b" | "\"a,b\"" | P1 |
| UNT-009 | EscapeCsvValue with quote | Template generation | "a\"b" | "\"a\"\"b\"" | P1 |
| UNT-010 | EscapeCsvValue with newline | Template generation | "a\nb" | "\"a\nb\"" (quoted) | P1 |
| UNT-011 | GetDisplayValue text | Display value | Artifact ValueText="x" | "x" | P1 |
| UNT-012 | GetDisplayValue number | Display value | Artifact ValueNumber=0.95 | "0.95" | P1 |
| UNT-013 | GetDisplayValue date | Display value | Artifact ValueDate | "yyyy-MM-dd" format | P1 |
| UNT-014 | Search term normalization | GetEntityRecords | searchTerm="  Norway  " | Trim or use as-is; query uses Contains | P1 |
| UNT-015 | ApplicableEntityTypes Split | GetAvailableEntityTypes | "country,partner,contact" | ["country","partner","contact"] | P1 |
| UNT-016 | ApplicableEntityTypes Trim | Split result | " country " | "country" | P1 |
| UNT-017 | Template header order | GenerateBulkTemplate | ArtifactTypeIds=[3,1,2] | Columns in request order | P1 |
| UNT-018 | Template UTF-8 BOM | GenerateBulkTemplate | CSV bytes | UTF-8 encoding, no BOM or with BOM per spec | P1 |
| UNT-019 | SanitizeFileName empty | SanitizeFileName | "" | "document" | P1 |
| UNT-020 | SanitizeFileName special chars | SanitizeFileName | "file%20#1" | "file_20_1" or similar | P1 |
| UNT-021 | decimal TryParse InvariantCulture | Bulk number parsing | "0.95" vs "0,95" | InvariantCulture handles . or , per locale | P1 |

---

## §9 Performance Tests (16)

| ID | Test Name | Operation | Threshold | Priority |
|----|-----------|----------|-----------|----------|
| PRF-001 | GetEntityTypes | GET entity-types | < 500ms | P0 |
| PRF-002 | GetArtifactTypes | GET artifact-types?entityType=country | < 500ms | P0 |
| PRF-003 | GetEntityRecords — 1000 countries | GET entity-records?entityType=country | < 2s | P1 |
| PRF-004 | GetEntityArtifact | GET get | < 200ms | P0 |
| PRF-005 | Upsert | POST upsert | < 500ms | P0 |
| PRF-006 | GetEntityArtifacts list — 50 artifacts | GET list for entity with 50 artifacts | < 1s | P1 |
| PRF-007 | Bulk upsert 100 rows | POST bulk/upsert 100 rows | < 30s | P0 |
| PRF-008 | Bulk upsert 500 rows | POST bulk/upsert 500 rows | < 2 min | P1 |
| PRF-009 | Bulk upsert 1000 rows | POST bulk/upsert 1000 rows | < 5 min | P1 |
| PRF-010 | Template download — 20 columns | POST template-download 20 artifact types | < 1s | P1 |
| PRF-011 | Upload document 1MB | POST upload-document 1MB file | < 10s | P1 |
| PRF-012 | Get document URL | GET document-url | < 2s (includes GCS signed URL) | P1 |
| PRF-013 | GetEntityRecords with search — 5000 partners | searchTerm="UN" | < 3s | P1 |
| PRF-014 | GetBulkArtifactTypes | GET bulk/artifact-types | < 500ms | P1 |
| PRF-015 | GetUniqueIdExample | GET bulk/unique-id-example | < 200ms | P1 |
| PRF-016 | Artifact type query with Include | GetArtifactTypesByEntityTypeAsync with Include(ArtifactDataType) | < 500ms | P1 |

---

## §10 Load Tests (10)

| ID | Test Name | Scenario | Expected | Priority |
|----|-----------|----------|----------|----------|
| LOD-001 | 50 concurrent GetEntityTypes | 50 users GET entity-types simultaneously | All 200 OK, < 2s p95 | P1 |
| LOD-002 | 50 concurrent GetArtifactTypes | 50 users GET artifact-types | All 200 OK | P1 |
| LOD-003 | 50 concurrent GetEntityArtifact | 50 users GET get (same artifact) | All 200 OK | P1 |
| LOD-004 | 20 concurrent Upserts | 20 users upsert different entities | All 200 OK | P1 |
| LOD-005 | 10 concurrent bulk upserts | 10 users bulk upsert 50 rows each | All complete, no deadlock | P1 |
| LOD-006 | 100 concurrent GetEntityRecords | 100 users GET entity-records | All 200 OK | P1 |
| LOD-007 | 30 concurrent document uploads | 30 users upload different files | All 200 OK | P1 |
| LOD-008 | 50 concurrent Get list | 50 users GET list for same entity | All 200 OK | P1 |
| LOD-009 | 20 concurrent template downloads | 20 users POST template-download | All 200 OK | P1 |
| LOD-010 | Mixed load — 100 reads, 20 writes | 100 GET + 20 POST over 1 min | All succeed, no 503 | P1 |

---

*End of EntityArtifactManager Test Cases*
