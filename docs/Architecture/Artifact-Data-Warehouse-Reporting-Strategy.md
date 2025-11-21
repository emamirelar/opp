# Artifact Data Model - Data Warehouse Reporting Strategy with Related Entity Artifacts

**Document Version:** 1.0  
**Date:** October 31, 2024  
**Author:** UNOPS Opportunity+ Development Team

---

## Executive Summary

This document outlines three comprehensive strategies for pushing the Artifact data model to a data warehouse for reporting purposes, with special emphasis on **propagating related entity artifacts**. For example, when reporting on an Opportunity, the warehouse should include artifacts from related Countries (via `OpportunityCountry`), related Partners (via `OpportunityFundingPartner`, `OpportunityClientPartner`), and related Organizational Units (via `ResponsibleOrgUnit`).

### Key Requirements

1. **Entity-Level Artifact Storage**: Store artifacts for entities (Country, Partner, OrganizationHierarchy, Opportunity, etc.)
2. **Related Entity Artifact Propagation**: Include artifacts from related entities when reporting on primary entities
3. **Temporal Tracking**: Support effective/expiry dates and historical analysis
4. **AI Traceability**: Track AI-extracted artifacts with confidence scores
5. **Multi-Type Support**: Handle string, number, date, document, JSON artifact types
6. **Performance**: Enable fast querying across entity relationships

---

## Data Model Overview

### Core Entities and Their Artifacts

```
EntityArtifact (EAV Model)
├── EntityType: "Country", "Partner", "OrganizationHierarchy", "Opportunity"
├── EntityId: Reference to specific entity instance
├── ArtifactTypeId: FK to ArtifactType
├── Values: ValueText, ValueNumber, ValueDate, ValueJson, DocumentId
├── Temporal: EffectiveDate, ExpiryDate
├── AI Tracking: IsExtracted, ConfidenceScore, SourceArtifactId
└── Metadata: Source, Metadata (JSON)
```

### Entity Relationship Graph for Artifact Propagation

```
Opportunity (Primary Entity)
    ├── OpportunityCountry (M:M) → Country
    │   └── Country Artifacts: FragileCategory, TestCountryNumber, etc.
    ├── OpportunityFundingPartner (M:M) → Partner
    │   └── Partner Artifacts: [Partner-specific artifacts]
    ├── OpportunityClientPartner (M:M) → Partner
    │   └── Partner Artifacts: [Partner-specific artifacts]
    ├── ResponsibleOrgUnit (FK) → OrganizationHierarchy
    │   └── OrgUnit Artifacts: TestOrganizationHierarchyString, etc.
    └── Direct Opportunity Artifacts: [Opportunity-specific artifacts]
```

---

## Strategy 1: Star Schema with Bridge Tables for Related Entities

### Overview

Extend the dimensional star schema to include **bridge tables** that connect primary entities to related entity artifacts. This approach maintains normalization while enabling efficient joins across entity relationships.

### Data Warehouse Schema Design

#### Core Fact and Dimension Tables

**Fact Table: `fact_entity_artifacts`**
```sql
CREATE TABLE fact_entity_artifacts (
    artifact_key BIGINT PRIMARY KEY,
    entity_dim_key BIGINT NOT NULL,
    artifact_type_dim_key INT NOT NULL,
    artifact_data_type_dim_key INT NOT NULL,
    date_effective_key INT,
    date_expiry_key INT,
    date_created_key INT,
    value_text VARCHAR(MAX),
    value_number DECIMAL(18, 4),
    value_date DATE,
    value_json TEXT,
    document_id INT,
    confidence_score DECIMAL(3, 2),
    is_extracted BOOLEAN,
    source_artifact_key BIGINT,
    metadata JSONB,
    is_current BOOLEAN DEFAULT TRUE,
    valid_from TIMESTAMP,
    valid_to TIMESTAMP,
    etl_batch_id BIGINT,
    
    FOREIGN KEY (entity_dim_key) REFERENCES dim_entity(entity_dim_key),
    FOREIGN KEY (artifact_type_dim_key) REFERENCES dim_artifact_type(artifact_type_dim_key),
    FOREIGN KEY (artifact_data_type_dim_key) REFERENCES dim_artifact_data_type(artifact_data_type_dim_key)
);

CREATE INDEX idx_fact_artifacts_entity ON fact_entity_artifacts(entity_dim_key);
CREATE INDEX idx_fact_artifacts_type ON fact_entity_artifacts(artifact_type_dim_key);
CREATE INDEX idx_fact_artifacts_current ON fact_entity_artifacts(is_current) WHERE is_current = TRUE;
```

**Dimension: `dim_entity`**
```sql
CREATE TABLE dim_entity (
    entity_dim_key BIGINT PRIMARY KEY,
    entity_id INT NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_name VARCHAR(500),
    entity_code VARCHAR(100),
    entity_status VARCHAR(50),
    parent_entity_dim_key BIGINT, -- For hierarchical entities like OrganizationHierarchy
    entity_attributes JSONB, -- Denormalized entity properties
    is_current BOOLEAN DEFAULT TRUE,
    valid_from TIMESTAMP,
    valid_to TIMESTAMP,
    
    UNIQUE (entity_id, entity_type, valid_from)
);

CREATE INDEX idx_dim_entity_type_id ON dim_entity(entity_type, entity_id);
CREATE INDEX idx_dim_entity_current ON dim_entity(is_current) WHERE is_current = TRUE;
```

#### Bridge Tables for Related Entity Artifacts

**Bridge: `bridge_opportunity_country_artifacts`**
```sql
CREATE TABLE bridge_opportunity_country_artifacts (
    bridge_key BIGINT PRIMARY KEY,
    opportunity_dim_key BIGINT NOT NULL,
    country_dim_key BIGINT NOT NULL,
    country_artifact_key BIGINT NOT NULL,
    
    -- Context from OpportunityCountry junction
    specific_areas VARCHAR(1000),
    context_warning VARCHAR(500),
    risk_score DECIMAL(3, 1),
    
    -- Effective date range
    valid_from TIMESTAMP,
    valid_to TIMESTAMP,
    is_current BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (opportunity_dim_key) REFERENCES dim_entity(entity_dim_key),
    FOREIGN KEY (country_dim_key) REFERENCES dim_entity(entity_dim_key),
    FOREIGN KEY (country_artifact_key) REFERENCES fact_entity_artifacts(artifact_key)
);

CREATE INDEX idx_bridge_opp_country_opp ON bridge_opportunity_country_artifacts(opportunity_dim_key);
CREATE INDEX idx_bridge_opp_country_country ON bridge_opportunity_country_artifacts(country_dim_key);
CREATE INDEX idx_bridge_opp_country_artifact ON bridge_opportunity_country_artifacts(country_artifact_key);
```

**Bridge: `bridge_opportunity_partner_artifacts`**
```sql
CREATE TABLE bridge_opportunity_partner_artifacts (
    bridge_key BIGINT PRIMARY KEY,
    opportunity_dim_key BIGINT NOT NULL,
    partner_dim_key BIGINT NOT NULL,
    partner_artifact_key BIGINT NOT NULL,
    
    -- Context from OpportunityFundingPartner / OpportunityClientPartner
    partner_role VARCHAR(50), -- 'Funding', 'Client'
    funding_amount DECIMAL(18, 2),
    funding_percentage DECIMAL(5, 2),
    currency_code VARCHAR(3),
    
    -- Effective date range
    valid_from TIMESTAMP,
    valid_to TIMESTAMP,
    is_current BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (opportunity_dim_key) REFERENCES dim_entity(entity_dim_key),
    FOREIGN KEY (partner_dim_key) REFERENCES dim_entity(entity_dim_key),
    FOREIGN KEY (partner_artifact_key) REFERENCES fact_entity_artifacts(artifact_key)
);

CREATE INDEX idx_bridge_opp_partner_opp ON bridge_opportunity_partner_artifacts(opportunity_dim_key);
CREATE INDEX idx_bridge_opp_partner_partner ON bridge_opportunity_partner_artifacts(partner_dim_key);
CREATE INDEX idx_bridge_opp_partner_artifact ON bridge_opportunity_partner_artifacts(partner_artifact_key);
```

**Bridge: `bridge_opportunity_orgunit_artifacts`**
```sql
CREATE TABLE bridge_opportunity_orgunit_artifacts (
    bridge_key BIGINT PRIMARY KEY,
    opportunity_dim_key BIGINT NOT NULL,
    orgunit_dim_key BIGINT NOT NULL,
    orgunit_artifact_key BIGINT NOT NULL,
    
    -- Context
    relationship_type VARCHAR(50), -- 'ResponsibleUnit', 'ImplementingUnit', etc.
    
    -- Effective date range
    valid_from TIMESTAMP,
    valid_to TIMESTAMP,
    is_current BOOLEAN DEFAULT TRUE,
    
    FOREIGN KEY (opportunity_dim_key) REFERENCES dim_entity(entity_dim_key),
    FOREIGN KEY (orgunit_dim_key) REFERENCES dim_entity(entity_dim_key),
    FOREIGN KEY (orgunit_artifact_key) REFERENCES fact_entity_artifacts(artifact_key)
);

CREATE INDEX idx_bridge_opp_orgunit_opp ON bridge_opportunity_orgunit_artifacts(opportunity_dim_key);
CREATE INDEX idx_bridge_opp_orgunit_orgunit ON bridge_opportunity_orgunit_artifacts(orgunit_dim_key);
CREATE INDEX idx_bridge_opp_orgunit_artifact ON bridge_opportunity_orgunit_artifacts(orgunit_artifact_key);
```

### ETL Process Design

#### Step 1: Load Core Entity Artifacts

```sql
-- Load all entity artifacts to fact table
INSERT INTO fact_entity_artifacts (
    entity_dim_key,
    artifact_type_dim_key,
    artifact_data_type_dim_key,
    -- ... other columns
)
SELECT 
    de.entity_dim_key,
    dat.artifact_type_dim_key,
    dadt.artifact_data_type_dim_key,
    -- ... other mappings
FROM EntityArtifacts ea
JOIN dim_entity de ON ea.EntityId = de.entity_id 
    AND ea.EntityType = de.entity_type
    AND de.is_current = TRUE
JOIN dim_artifact_type dat ON ea.ArtifactTypeId = dat.artifact_type_id
LEFT JOIN ArtifactTypes at ON ea.ArtifactTypeId = at.Id
LEFT JOIN dim_artifact_data_type dadt ON at.ArtifactDataTypeId = dadt.data_type_id
WHERE NOT ea.IsDeleted;
```

#### Step 2: Populate Bridge Tables for Related Artifacts

**Load Opportunity-Country Artifact Bridge:**
```sql
INSERT INTO bridge_opportunity_country_artifacts (
    opportunity_dim_key,
    country_dim_key,
    country_artifact_key,
    specific_areas,
    context_warning,
    risk_score,
    valid_from,
    valid_to,
    is_current
)
SELECT 
    opp_dim.entity_dim_key as opportunity_dim_key,
    country_dim.entity_dim_key as country_dim_key,
    country_artifacts.artifact_key as country_artifact_key,
    oc.SpecificAreas,
    oc.ContextWarning,
    oc.RiskScore,
    GREATEST(
        country_artifacts.valid_from,
        COALESCE(country_artifacts.effective_date, '1900-01-01'::timestamp)
    ) as valid_from,
    LEAST(
        country_artifacts.valid_to,
        COALESCE(country_artifacts.expiry_date, '9999-12-31'::timestamp)
    ) as valid_to,
    (country_artifacts.is_current AND 
     (country_artifacts.expiry_date IS NULL OR country_artifacts.expiry_date > CURRENT_TIMESTAMP)
    ) as is_current
FROM OpportunityCountry oc
JOIN dim_entity opp_dim 
    ON opp_dim.entity_type = 'Opportunity' 
    AND opp_dim.entity_id = oc.OpportunityId
    AND opp_dim.is_current = TRUE
JOIN dim_entity country_dim 
    ON country_dim.entity_type = 'Country' 
    AND country_dim.entity_id = oc.CountryId
    AND country_dim.is_current = TRUE
JOIN fact_entity_artifacts country_artifacts 
    ON country_artifacts.entity_dim_key = country_dim.entity_dim_key
    AND country_artifacts.is_current = TRUE;
```

**Load Opportunity-Partner Artifact Bridge:**
```sql
-- Funding Partners
INSERT INTO bridge_opportunity_partner_artifacts (...)
SELECT 
    opp_dim.entity_dim_key,
    partner_dim.entity_dim_key,
    partner_artifacts.artifact_key,
    'Funding' as partner_role,
    ofp.Amount,
    ofp.Percentage,
    c.Code as currency_code,
    -- ... temporal fields
FROM OpportunityFundingPartner ofp
JOIN dim_entity opp_dim ON ...
JOIN dim_entity partner_dim ON ...
JOIN fact_entity_artifacts partner_artifacts ON ...
LEFT JOIN Currency c ON ofp.CurrencyId = c.Id;

-- Client Partners
INSERT INTO bridge_opportunity_partner_artifacts (...)
SELECT 
    opp_dim.entity_dim_key,
    partner_dim.entity_dim_key,
    partner_artifacts.artifact_key,
    'Client' as partner_role,
    NULL as funding_amount,
    NULL as funding_percentage,
    NULL as currency_code,
    -- ... temporal fields
FROM OpportunityClientPartner ocp
JOIN dim_entity opp_dim ON ...
JOIN dim_entity partner_dim ON ...
JOIN fact_entity_artifacts partner_artifacts ON ...;
```

**Load Opportunity-OrgUnit Artifact Bridge:**
```sql
INSERT INTO bridge_opportunity_orgunit_artifacts (...)
SELECT 
    opp_dim.entity_dim_key,
    orgunit_dim.entity_dim_key,
    orgunit_artifacts.artifact_key,
    'ResponsibleUnit' as relationship_type,
    -- ... temporal fields
FROM Opportunities o
JOIN dim_entity opp_dim 
    ON opp_dim.entity_type = 'Opportunity' 
    AND opp_dim.entity_id = o.Id
JOIN dim_entity orgunit_dim 
    ON orgunit_dim.entity_type = 'OrganizationHierarchy' 
    AND orgunit_dim.entity_id = o.ResponsibleOrgUnitId
JOIN fact_entity_artifacts orgunit_artifacts 
    ON orgunit_artifacts.entity_dim_key = orgunit_dim.entity_dim_key
WHERE o.ResponsibleOrgUnitId IS NOT NULL;
```

### Reporting Query Examples

#### Report 1: Opportunity with All Related Entity Artifacts

```sql
-- Get all artifacts for an opportunity (direct + related)
WITH opportunity_all_artifacts AS (
    -- Direct opportunity artifacts
    SELECT 
        o.entity_name as opportunity_name,
        'Direct' as artifact_source,
        NULL as related_entity_name,
        at.artifact_type_name,
        at.category as artifact_category,
        fa.value_text,
        fa.value_number,
        fa.value_date,
        fa.confidence_score,
        fa.is_extracted
    FROM dim_entity o
    JOIN fact_entity_artifacts fa ON fa.entity_dim_key = o.entity_dim_key
    JOIN dim_artifact_type at ON fa.artifact_type_dim_key = at.artifact_type_dim_key
    WHERE o.entity_type = 'Opportunity'
      AND o.entity_id = 123  -- Specific opportunity
      AND fa.is_current = TRUE
    
    UNION ALL
    
    -- Country artifacts via bridge
    SELECT 
        o.entity_name as opportunity_name,
        'Country' as artifact_source,
        c.entity_name as related_entity_name,
        at.artifact_type_name,
        at.category as artifact_category,
        fa.value_text,
        fa.value_number,
        fa.value_date,
        fa.confidence_score,
        fa.is_extracted
    FROM dim_entity o
    JOIN bridge_opportunity_country_artifacts boc ON boc.opportunity_dim_key = o.entity_dim_key
    JOIN dim_entity c ON boc.country_dim_key = c.entity_dim_key
    JOIN fact_entity_artifacts fa ON boc.country_artifact_key = fa.artifact_key
    JOIN dim_artifact_type at ON fa.artifact_type_dim_key = at.artifact_type_dim_key
    WHERE o.entity_type = 'Opportunity'
      AND o.entity_id = 123
      AND boc.is_current = TRUE
    
    UNION ALL
    
    -- Partner artifacts via bridge
    SELECT 
        o.entity_name as opportunity_name,
        CONCAT('Partner (', bop.partner_role, ')') as artifact_source,
        p.entity_name as related_entity_name,
        at.artifact_type_name,
        at.category as artifact_category,
        fa.value_text,
        fa.value_number,
        fa.value_date,
        fa.confidence_score,
        fa.is_extracted
    FROM dim_entity o
    JOIN bridge_opportunity_partner_artifacts bop ON bop.opportunity_dim_key = o.entity_dim_key
    JOIN dim_entity p ON bop.partner_dim_key = p.entity_dim_key
    JOIN fact_entity_artifacts fa ON bop.partner_artifact_key = fa.artifact_key
    JOIN dim_artifact_type at ON fa.artifact_type_dim_key = at.artifact_type_dim_key
    WHERE o.entity_type = 'Opportunity'
      AND o.entity_id = 123
      AND bop.is_current = TRUE
    
    UNION ALL
    
    -- OrgUnit artifacts via bridge
    SELECT 
        o.entity_name as opportunity_name,
        'OrganizationUnit' as artifact_source,
        ou.entity_name as related_entity_name,
        at.artifact_type_name,
        at.category as artifact_category,
        fa.value_text,
        fa.value_number,
        fa.value_date,
        fa.confidence_score,
        fa.is_extracted
    FROM dim_entity o
    JOIN bridge_opportunity_orgunit_artifacts bou ON bou.opportunity_dim_key = o.entity_dim_key
    JOIN dim_entity ou ON bou.orgunit_dim_key = ou.entity_dim_key
    JOIN fact_entity_artifacts fa ON bou.orgunit_artifact_key = fa.artifact_key
    JOIN dim_artifact_type at ON fa.artifact_type_dim_key = at.artifact_type_dim_key
    WHERE o.entity_type = 'Opportunity'
      AND o.entity_id = 123
      AND bou.is_current = TRUE
)
SELECT * FROM opportunity_all_artifacts
ORDER BY artifact_source, artifact_category, artifact_type_name;
```

#### Report 2: Opportunity Risk Analysis Using Country Artifacts

```sql
-- Aggregate country risk metrics for opportunities
SELECT 
    o.entity_id as opportunity_id,
    o.entity_name as opportunity_name,
    COUNT(DISTINCT boc.country_dim_key) as country_count,
    AVG(boc.risk_score) as avg_country_risk,
    MAX(boc.risk_score) as max_country_risk,
    STRING_AGG(DISTINCT c.entity_name, ', ') as countries,
    
    -- Fragile category breakdown
    STRING_AGG(
        DISTINCT CASE 
            WHEN at.artifact_type_code = 'FragileCategory' 
            THEN fa.value_text 
        END, 
        ', '
    ) as fragile_categories,
    
    -- Average numeric country metrics
    AVG(
        CASE 
            WHEN at.artifact_type_code = 'TestCountryNumber' 
            THEN fa.value_number 
        END
    ) as avg_country_metric
    
FROM dim_entity o
JOIN bridge_opportunity_country_artifacts boc 
    ON boc.opportunity_dim_key = o.entity_dim_key
    AND boc.is_current = TRUE
JOIN dim_entity c ON boc.country_dim_key = c.entity_dim_key
JOIN fact_entity_artifacts fa ON boc.country_artifact_key = fa.artifact_key
JOIN dim_artifact_type at ON fa.artifact_type_dim_key = at.artifact_type_dim_key
WHERE o.entity_type = 'Opportunity'
  AND o.is_current = TRUE
GROUP BY o.entity_id, o.entity_name
ORDER BY max_country_risk DESC;
```

### Reporting Benefits

- ✅ **Complete Artifact Coverage**: Access all entity artifacts and related entity artifacts in one report
- ✅ **Relationship Context**: Bridge tables preserve junction table context (risk scores, funding amounts)
- ✅ **Temporal Accuracy**: Maintains effective/expiry dates across relationships
- ✅ **Performance**: Indexed bridge tables enable fast joins
- ✅ **Auditability**: Clear lineage from opportunity → related entity → artifact
- ✅ **Flexibility**: Can query any combination of direct and related artifacts

---

## Strategy 2: Wide Pivoted Tables with Denormalized Related Artifacts

### Overview

Extend the wide table approach to **denormalize related entity artifacts** directly into the primary entity's row. Each artifact from related entities becomes a set of columns in the wide table, with array/JSON support for multiple related entities.

### Data Warehouse Schema Design

#### Table: `wide_opportunity_with_related_artifacts`

```sql
CREATE TABLE wide_opportunity_with_related_artifacts (
    -- Primary Keys
    opportunity_id INT PRIMARY KEY,
    opportunity_name VARCHAR(500),
    opportunity_description TEXT,
    opportunity_status VARCHAR(50),
    
    -- Core Opportunity Properties
    budget_usd DECIMAL(18, 2),
    target_signing_date DATE,
    target_delivery_date DATE,
    responsible_orgunit_id INT,
    responsible_orgunit_name VARCHAR(500),
    
    -- Direct Opportunity Artifacts (dynamically generated columns)
    -- [These would be generated based on Opportunity-applicable artifact types]
    
    -- ========================================
    -- RELATED COUNTRY ARTIFACTS (DENORMALIZED)
    -- ========================================
    
    -- Summary counts
    country_count INT,
    country_list TEXT[], -- Array of country names
    country_id_list INT[], -- Array of country IDs
    
    -- Aggregated country metrics
    avg_country_risk_score DECIMAL(3, 1),
    max_country_risk_score DECIMAL(3, 1),
    
    -- Country artifacts as arrays (one entry per country)
    country_fragile_categories TEXT[], -- ['High Fragility', 'Moderate Fragility']
    country_fragile_category_dates DATE[], -- Parallel array of effective dates
    
    country_test_numbers DECIMAL(18, 4)[], -- Numeric values from TestCountryNumber
    country_test_number_dates DATE[],
    
    country_test_strings TEXT[],
    country_test_string_dates DATE[],
    
    -- Country artifacts as JSON for complex queries
    country_artifacts_json JSONB, -- Full artifact details by country
    
    -- ========================================
    -- RELATED PARTNER ARTIFACTS (DENORMALIZED)
    -- ========================================
    
    -- Funding partners summary
    funding_partner_count INT,
    funding_partner_list TEXT[],
    funding_partner_id_list INT[],
    total_funding_amount DECIMAL(18, 2),
    
    -- Client partners summary
    client_partner_count INT,
    client_partner_list TEXT[],
    client_partner_id_list INT[],
    
    -- Partner artifacts as arrays
    partner_artifacts_json JSONB, -- Full partner artifact details
    
    -- ========================================
    -- RELATED ORGUNIT ARTIFACTS (DENORMALIZED)
    -- ========================================
    
    -- Responsible OrgUnit artifacts
    orgunit_test_string TEXT,
    orgunit_test_string_date DATE,
    orgunit_test_number DECIMAL(18, 4),
    orgunit_test_number_date DATE,
    
    orgunit_artifacts_json JSONB,
    
    -- ========================================
    -- METADATA
    -- ========================================
    total_artifact_count INT,
    direct_artifact_count INT,
    related_artifact_count INT,
    last_artifact_update TIMESTAMP,
    etl_timestamp TIMESTAMP
);

-- Indexes
CREATE INDEX idx_wide_opp_country_risk ON wide_opportunity_with_related_artifacts(max_country_risk_score);
CREATE INDEX idx_wide_opp_funding ON wide_opportunity_with_related_artifacts(total_funding_amount);
CREATE INDEX idx_wide_opp_orgunit ON wide_opportunity_with_related_artifacts(responsible_orgunit_id);
CREATE GIN INDEX idx_wide_opp_country_json ON wide_opportunity_with_related_artifacts USING gin(country_artifacts_json);
CREATE GIN INDEX idx_wide_opp_partner_json ON wide_opportunity_with_related_artifacts USING gin(partner_artifacts_json);
```

### ETL Process Design

```sql
INSERT INTO wide_opportunity_with_related_artifacts
SELECT 
    o.Id as opportunity_id,
    o.Name as opportunity_name,
    o.Description as opportunity_description,
    o.Status,
    o.InitiativeBudgetUSD as budget_usd,
    o.TargetSigningDate as target_signing_date,
    o.TargetDeliveryDate as target_delivery_date,
    o.ResponsibleOrgUnitId,
    ou.Name as responsible_orgunit_name,
    
    -- ========================================
    -- AGGREGATE COUNTRY DATA
    -- ========================================
    COUNT(DISTINCT oc.CountryId) as country_count,
    ARRAY_AGG(DISTINCT c.Name) FILTER (WHERE c.Id IS NOT NULL) as country_list,
    ARRAY_AGG(DISTINCT c.Id) FILTER (WHERE c.Id IS NOT NULL) as country_id_list,
    
    AVG(oc.RiskScore) as avg_country_risk_score,
    MAX(oc.RiskScore) as max_country_risk_score,
    
    -- Country artifact arrays (using conditional aggregation)
    ARRAY_AGG(
        DISTINCT cea_fragile.ValueText 
        ORDER BY cea_fragile.ValueText
    ) FILTER (
        WHERE cat_fragile.ArtifactTypeCode = 'FragileCategory'
    ) as country_fragile_categories,
    
    ARRAY_AGG(
        DISTINCT cea_fragile.EffectiveDate
        ORDER BY cea_fragile.EffectiveDate
    ) FILTER (
        WHERE cat_fragile.ArtifactTypeCode = 'FragileCategory'
    ) as country_fragile_category_dates,
    
    ARRAY_AGG(
        cea_number.ValueNumber
        ORDER BY c.Name
    ) FILTER (
        WHERE cat_number.ArtifactTypeCode = 'TestCountryNumber'
    ) as country_test_numbers,
    
    ARRAY_AGG(
        cea_number.EffectiveDate
        ORDER BY c.Name
    ) FILTER (
        WHERE cat_number.ArtifactTypeCode = 'TestCountryNumber'
    ) as country_test_number_dates,
    
    -- Country artifacts as structured JSON
    JSON_AGG(
        JSON_BUILD_OBJECT(
            'country_id', c.Id,
            'country_name', c.Name,
            'risk_score', oc.RiskScore,
            'artifacts', (
                SELECT JSON_AGG(
                    JSON_BUILD_OBJECT(
                        'artifact_type_code', at.ArtifactTypeCode,
                        'artifact_type_name', at.Name,
                        'category', at.Category,
                        'value', CASE 
                            WHEN adt.Name = 'number' THEN to_jsonb(ea.ValueNumber)
                            WHEN adt.Name = 'date' THEN to_jsonb(ea.ValueDate)
                            ELSE to_jsonb(ea.ValueText)
                        END,
                        'effective_date', ea.EffectiveDate,
                        'is_extracted', ea.IsExtracted,
                        'confidence_score', ea.ConfidenceScore
                    )
                )
                FROM EntityArtifacts ea
                JOIN ArtifactTypes at ON ea.ArtifactTypeId = at.Id
                JOIN ArtifactDataTypes adt ON at.ArtifactDataTypeId = adt.Id
                WHERE ea.EntityType = 'Country'
                  AND ea.EntityId = c.Id
                  AND NOT ea.IsDeleted
                  AND (ea.EffectiveDate IS NULL OR ea.EffectiveDate <= CURRENT_TIMESTAMP)
                  AND (ea.ExpiryDate IS NULL OR ea.ExpiryDate > CURRENT_TIMESTAMP)
            )
        )
    ) FILTER (WHERE c.Id IS NOT NULL) as country_artifacts_json,
    
    -- ========================================
    -- AGGREGATE PARTNER DATA
    -- ========================================
    COUNT(DISTINCT ofp.PartnerId) as funding_partner_count,
    ARRAY_AGG(DISTINCT p_funding.Name) FILTER (WHERE ofp.Id IS NOT NULL) as funding_partner_list,
    ARRAY_AGG(DISTINCT ofp.PartnerId) FILTER (WHERE ofp.Id IS NOT NULL) as funding_partner_id_list,
    SUM(ofp.Amount) as total_funding_amount,
    
    COUNT(DISTINCT ocp.PartnerId) as client_partner_count,
    ARRAY_AGG(DISTINCT p_client.Name) FILTER (WHERE ocp.Id IS NOT NULL) as client_partner_list,
    ARRAY_AGG(DISTINCT ocp.PartnerId) FILTER (WHERE ocp.Id IS NOT NULL) as client_partner_id_list,
    
    -- Partner artifacts as structured JSON
    JSON_BUILD_OBJECT(
        'funding_partners', (
            SELECT JSON_AGG(
                JSON_BUILD_OBJECT(
                    'partner_id', p.Id,
                    'partner_name', p.Name,
                    'funding_amount', ofp2.Amount,
                    'artifacts', (
                        SELECT JSON_AGG(
                            JSON_BUILD_OBJECT(
                                'artifact_type_code', at_p.ArtifactTypeCode,
                                'value', CASE 
                                    WHEN adt_p.Name = 'number' THEN to_jsonb(ea_p.ValueNumber)
                                    ELSE to_jsonb(ea_p.ValueText)
                                END
                            )
                        )
                        FROM EntityArtifacts ea_p
                        JOIN ArtifactTypes at_p ON ea_p.ArtifactTypeId = at_p.Id
                        JOIN ArtifactDataTypes adt_p ON at_p.ArtifactDataTypeId = adt_p.Id
                        WHERE ea_p.EntityType = 'Partner'
                          AND ea_p.EntityId = p.Id
                          AND NOT ea_p.IsDeleted
                    )
                )
            )
            FROM OpportunityFundingPartner ofp2
            JOIN Partners p ON ofp2.PartnerId = p.Id
            WHERE ofp2.OpportunityId = o.Id
        ),
        'client_partners', (
            SELECT JSON_AGG(
                JSON_BUILD_OBJECT(
                    'partner_id', p.Id,
                    'partner_name', p.Name,
                    'artifacts', (
                        SELECT JSON_AGG(
                            JSON_BUILD_OBJECT(
                                'artifact_type_code', at_p.ArtifactTypeCode,
                                'value', CASE 
                                    WHEN adt_p.Name = 'number' THEN to_jsonb(ea_p.ValueNumber)
                                    ELSE to_jsonb(ea_p.ValueText)
                                END
                            )
                        )
                        FROM EntityArtifacts ea_p
                        JOIN ArtifactTypes at_p ON ea_p.ArtifactTypeId = at_p.Id
                        JOIN ArtifactDataTypes adt_p ON at_p.ArtifactDataTypeId = adt_p.Id
                        WHERE ea_p.EntityType = 'Partner'
                          AND ea_p.EntityId = p.Id
                          AND NOT ea_p.IsDeleted
                    )
                )
            )
            FROM OpportunityClientPartner ocp2
            JOIN Partners p ON ocp2.PartnerId = p.Id
            WHERE ocp2.OpportunityId = o.Id
        )
    ) as partner_artifacts_json,
    
    -- ========================================
    -- ORGUNIT ARTIFACTS (SINGLE FK)
    -- ========================================
    MAX(
        CASE WHEN ouat.ArtifactTypeCode = 'TestOrganizationHierarchyString' 
        THEN ouea.ValueText END
    ) as orgunit_test_string,
    MAX(
        CASE WHEN ouat.ArtifactTypeCode = 'TestOrganizationHierarchyString' 
        THEN ouea.EffectiveDate END
    ) as orgunit_test_string_date,
    MAX(
        CASE WHEN ouat.ArtifactTypeCode = 'TestOrganizationHierarchyNumber' 
        THEN ouea.ValueNumber END
    ) as orgunit_test_number,
    MAX(
        CASE WHEN ouat.ArtifactTypeCode = 'TestOrganizationHierarchyNumber' 
        THEN ouea.EffectiveDate END
    ) as orgunit_test_number_date,
    
    -- OrgUnit artifacts as JSON
    (
        SELECT JSON_AGG(
            JSON_BUILD_OBJECT(
                'artifact_type_code', at_ou.ArtifactTypeCode,
                'artifact_type_name', at_ou.Name,
                'value', CASE 
                    WHEN adt_ou.Name = 'number' THEN to_jsonb(ea_ou.ValueNumber)
                    ELSE to_jsonb(ea_ou.ValueText)
                END,
                'effective_date', ea_ou.EffectiveDate
            )
        )
        FROM EntityArtifacts ea_ou
        JOIN ArtifactTypes at_ou ON ea_ou.ArtifactTypeId = at_ou.Id
        JOIN ArtifactDataTypes adt_ou ON at_ou.ArtifactDataTypeId = adt_ou.Id
        WHERE ea_ou.EntityType = 'OrganizationHierarchy'
          AND ea_ou.EntityId = o.ResponsibleOrgUnitId
          AND NOT ea_ou.IsDeleted
    ) as orgunit_artifacts_json,
    
    -- ========================================
    -- METADATA
    -- ========================================
    (
        SELECT COUNT(*)
        FROM EntityArtifacts ea_all
        WHERE ea_all.EntityType = 'Opportunity'
          AND ea_all.EntityId = o.Id
          AND NOT ea_all.IsDeleted
    ) as direct_artifact_count,
    
    CURRENT_TIMESTAMP as etl_timestamp

FROM Opportunities o
LEFT JOIN OrganizationHierarchies ou ON o.ResponsibleOrgUnitId = ou.Id
LEFT JOIN OpportunityCountry oc ON oc.OpportunityId = o.Id
LEFT JOIN Countries c ON oc.CountryId = c.Id

-- Join country artifacts for aggregation
LEFT JOIN EntityArtifacts cea_fragile 
    ON cea_fragile.EntityType = 'Country' 
    AND cea_fragile.EntityId = c.Id
    AND NOT cea_fragile.IsDeleted
LEFT JOIN ArtifactTypes cat_fragile ON cea_fragile.ArtifactTypeId = cat_fragile.Id
    AND cat_fragile.ArtifactTypeCode = 'FragileCategory'

LEFT JOIN EntityArtifacts cea_number 
    ON cea_number.EntityType = 'Country' 
    AND cea_number.EntityId = c.Id
    AND NOT cea_number.IsDeleted
LEFT JOIN ArtifactTypes cat_number ON cea_number.ArtifactTypeId = cat_number.Id
    AND cat_number.ArtifactTypeCode = 'TestCountryNumber'

-- Join funding partners
LEFT JOIN OpportunityFundingPartner ofp ON ofp.OpportunityId = o.Id
LEFT JOIN Partners p_funding ON ofp.PartnerId = p_funding.Id

-- Join client partners
LEFT JOIN OpportunityClientPartner ocp ON ocp.OpportunityId = o.Id
LEFT JOIN Partners p_client ON ocp.PartnerId = p_client.Id

-- Join orgunit artifacts
LEFT JOIN EntityArtifacts ouea 
    ON ouea.EntityType = 'OrganizationHierarchy' 
    AND ouea.EntityId = o.ResponsibleOrgUnitId
    AND NOT ouea.IsDeleted
LEFT JOIN ArtifactTypes ouat ON ouea.ArtifactTypeId = ouat.Id

WHERE NOT o.IsDeleted

GROUP BY 
    o.Id, o.Name, o.Description, o.Status, 
    o.InitiativeBudgetUSD, o.TargetSigningDate, o.TargetDeliveryDate,
    o.ResponsibleOrgUnitId, ou.Name;
```

### Reporting Query Examples

#### Report 1: Simple Opportunity Report with Country Risk

```sql
SELECT 
    opportunity_name,
    country_count,
    avg_country_risk_score,
    max_country_risk_score,
    ARRAY_TO_STRING(country_list, ', ') as countries,
    ARRAY_TO_STRING(country_fragile_categories, ', ') as fragile_categories,
    funding_partner_count,
    total_funding_amount
FROM wide_opportunity_with_related_artifacts
WHERE max_country_risk_score > 2.5
ORDER BY max_country_risk_score DESC;
```

#### Report 2: JSON Query for Specific Country Artifacts

```sql
-- Find opportunities in countries with specific artifact values
SELECT 
    opportunity_name,
    jsonb_pretty(country_artifacts_json) as country_details
FROM wide_opportunity_with_related_artifacts
WHERE country_artifacts_json @> '[{"country_name": "Afghanistan"}]'::jsonb
   OR EXISTS (
       SELECT 1
       FROM jsonb_array_elements(country_artifacts_json) as country
       WHERE country->'artifacts' @> '[{"artifact_type_code": "FragileCategory"}]'::jsonb
   );
```

### Reporting Benefits

- ✅ **Single Table Queries**: Most reports need only one table
- ✅ **Excel Export Ready**: Wide format perfect for business users
- ✅ **Array Operations**: PostgreSQL array functions for filtering and aggregation
- ✅ **JSON Flexibility**: Complex artifact structures queryable via JSONB operators
- ✅ **Pre-Aggregated Metrics**: Summary columns (counts, averages) for fast dashboards

---

## Strategy 3: Hybrid JSON Document Model with Entity Relationship Graph

### Overview

Create a **document-oriented model** where each opportunity is a rich JSON document containing all related entity artifacts in a nested structure. Use materialized views to flatten for specific reporting needs.

### Data Warehouse Schema Design

#### Table: `entity_documents_with_relationships`

```sql
CREATE TABLE entity_documents_with_relationships (
    document_key BIGINT PRIMARY KEY,
    entity_type VARCHAR(100) NOT NULL,
    entity_id INT NOT NULL,
    entity_name VARCHAR(500),
    entity_status VARCHAR(50),
    
    -- Core entity properties
    entity_properties JSONB,
    
    -- Direct entity artifacts
    direct_artifacts JSONB,
    
    -- Related entity artifacts (nested structure)
    related_entities JSONB,
    
    -- Flattened artifact index for fast searching
    artifact_index JSONB, -- {"ArtifactTypeCode": [values], ...}
    
    -- Metadata
    artifact_count INT,
    related_entity_count INT,
    relationship_graph JSONB, -- Entity relationship visualization data
    last_updated TIMESTAMP,
    etl_timestamp TIMESTAMP,
    
    UNIQUE (entity_type, entity_id)
);

-- Indexes
CREATE INDEX idx_entity_docs_type_id ON entity_documents_with_relationships(entity_type, entity_id);
CREATE GIN INDEX idx_entity_docs_props ON entity_documents_with_relationships USING gin(entity_properties);
CREATE GIN INDEX idx_entity_docs_artifacts ON entity_documents_with_relationships USING gin(direct_artifacts);
CREATE GIN INDEX idx_entity_docs_related ON entity_documents_with_relationships USING gin(related_entities);
CREATE GIN INDEX idx_entity_docs_index ON entity_documents_with_relationships USING gin(artifact_index);
```

### JSON Document Structure

```json
{
  "document_key": 98765,
  "entity_type": "Opportunity",
  "entity_id": 123,
  "entity_name": "Health Infrastructure Development Project",
  "entity_status": "Active",
  
  "entity_properties": {
    "description": "Comprehensive health infrastructure project",
    "budget_usd": 5000000.00,
    "target_signing_date": "2025-06-01",
    "target_delivery_date": "2027-12-31",
    "workflow_stage": "Planning"
  },
  
  "direct_artifacts": [
    {
      "artifact_type_code": "OpportunityScope",
      "artifact_type_name": "Project Scope",
      "category": "Strategy",
      "data_type": "string",
      "value": "Build 5 health clinics in rural areas",
      "effective_date": "2024-01-01",
      "source": "User Input",
      "is_extracted": false
    }
  ],
  
  "related_entities": {
    "countries": [
      {
        "relationship_type": "OpportunityCountry",
        "country_id": 1,
        "country_name": "Afghanistan",
        "country_iso2": "AF",
        "relationship_context": {
          "specific_areas": "Kabul, Herat provinces",
          "context_warning": "Security concerns in region",
          "risk_score": 3.5
        },
        "artifacts": [
          {
            "artifact_type_code": "FragileCategory",
            "artifact_type_name": "Fragile Category",
            "category": "Assessment",
            "data_type": "string",
            "value": "High Fragility",
            "effective_date": "2024-01-01",
            "source": "External API",
            "is_extracted": false
          },
          {
            "artifact_type_code": "TestCountryNumber",
            "artifact_type_name": "Test Country Number",
            "category": "Test",
            "data_type": "number",
            "value": 85.5,
            "effective_date": "2024-01-01",
            "source": "AI Extraction",
            "is_extracted": true,
            "confidence_score": 0.92
          }
        ]
      },
      {
        "relationship_type": "OpportunityCountry",
        "country_id": 45,
        "country_name": "Kenya",
        "country_iso2": "KE",
        "relationship_context": {
          "specific_areas": "Nairobi, Mombasa",
          "risk_score": 1.8
        },
        "artifacts": [
          {
            "artifact_type_code": "FragileCategory",
            "value": "Low Fragility"
          }
        ]
      }
    ],
    
    "partners": {
      "funding_partners": [
        {
          "relationship_type": "OpportunityFundingPartner",
          "partner_id": 501,
          "partner_name": "World Bank",
          "relationship_context": {
            "funding_amount": 3000000.00,
            "funding_percentage": 60.0,
            "currency_code": "USD"
          },
          "artifacts": [
            {
              "artifact_type_code": "PartnerFinancialRating",
              "value": "AAA",
              "effective_date": "2024-01-01"
            }
          ]
        }
      ],
      "client_partners": [
        {
          "relationship_type": "OpportunityClientPartner",
          "partner_id": 702,
          "partner_name": "Ministry of Health - Afghanistan",
          "artifacts": []
        }
      ]
    },
    
    "organization_units": [
      {
        "relationship_type": "ResponsibleOrgUnit",
        "orgunit_id": 25,
        "orgunit_name": "Health Practice Group",
        "orgunit_type": "Practice",
        "artifacts": [
          {
            "artifact_type_code": "TestOrganizationHierarchyString",
            "value": "Health Sector Focus",
            "effective_date": "2024-01-01"
          },
          {
            "artifact_type_code": "TestOrganizationHierarchyNumber",
            "value": 95.0,
            "effective_date": "2024-01-01"
          }
        ]
      }
    ]
  },
  
  "artifact_index": {
    "FragileCategory": ["High Fragility", "Low Fragility"],
    "TestCountryNumber": [85.5],
    "PartnerFinancialRating": ["AAA"],
    "TestOrganizationHierarchyNumber": [95.0]
  },
  
  "relationship_graph": {
    "nodes": [
      {"id": "Opportunity-123", "type": "Opportunity"},
      {"id": "Country-1", "type": "Country", "name": "Afghanistan"},
      {"id": "Country-45", "type": "Country", "name": "Kenya"},
      {"id": "Partner-501", "type": "Partner", "name": "World Bank"},
      {"id": "OrgUnit-25", "type": "OrganizationHierarchy"}
    ],
    "edges": [
      {"from": "Opportunity-123", "to": "Country-1", "type": "OpportunityCountry", "risk": 3.5},
      {"from": "Opportunity-123", "to": "Country-45", "type": "OpportunityCountry", "risk": 1.8},
      {"from": "Opportunity-123", "to": "Partner-501", "type": "FundingPartner", "amount": 3000000},
      {"from": "Opportunity-123", "to": "OrgUnit-25", "type": "ResponsibleUnit"}
    ]
  },
  
  "artifact_count": 1,
  "related_entity_count": 5,
  "last_updated": "2024-10-31T10:30:00Z",
  "etl_timestamp": "2024-10-31T12:00:00Z"
}
```

### ETL Process Design

```sql
INSERT INTO entity_documents_with_relationships (
    entity_type,
    entity_id,
    entity_name,
    entity_status,
    entity_properties,
    direct_artifacts,
    related_entities,
    artifact_index,
    artifact_count,
    related_entity_count,
    relationship_graph,
    last_updated,
    etl_timestamp
)
SELECT 
    'Opportunity' as entity_type,
    o.Id as entity_id,
    o.Name as entity_name,
    o.Status as entity_status,
    
    -- Entity properties as JSON
    JSON_BUILD_OBJECT(
        'description', o.Description,
        'budget_usd', o.InitiativeBudgetUSD,
        'target_signing_date', o.TargetSigningDate,
        'target_delivery_date', o.TargetDeliveryDate,
        'workflow_stage', ws.Name
    ) as entity_properties,
    
    -- Direct artifacts
    (
        SELECT COALESCE(JSON_AGG(
            JSON_BUILD_OBJECT(
                'artifact_type_code', at.ArtifactTypeCode,
                'artifact_type_name', at.Name,
                'category', at.Category,
                'data_type', adt.Name,
                'value', CASE adt.Name
                    WHEN 'number' THEN to_jsonb(ea.ValueNumber)
                    WHEN 'date' THEN to_jsonb(ea.ValueDate)
                    WHEN 'json' THEN ea.ValueJson::jsonb
                    ELSE to_jsonb(ea.ValueText)
                END,
                'effective_date', ea.EffectiveDate,
                'expiry_date', ea.ExpiryDate,
                'source', ea.Source,
                'is_extracted', ea.IsExtracted,
                'confidence_score', ea.ConfidenceScore
            )
        ), '[]'::json)
        FROM EntityArtifacts ea
        JOIN ArtifactTypes at ON ea.ArtifactTypeId = at.Id
        JOIN ArtifactDataTypes adt ON at.ArtifactDataTypeId = adt.Id
        WHERE ea.EntityType = 'Opportunity'
          AND ea.EntityId = o.Id
          AND NOT ea.IsDeleted
          AND (ea.EffectiveDate IS NULL OR ea.EffectiveDate <= CURRENT_TIMESTAMP)
          AND (ea.ExpiryDate IS NULL OR ea.ExpiryDate > CURRENT_TIMESTAMP)
    ) as direct_artifacts,
    
    -- Related entities with their artifacts
    JSON_BUILD_OBJECT(
        'countries', (
            SELECT COALESCE(JSON_AGG(
                JSON_BUILD_OBJECT(
                    'relationship_type', 'OpportunityCountry',
                    'country_id', c.Id,
                    'country_name', c.Name,
                    'country_iso2', c.Iso2Code,
                    'relationship_context', JSON_BUILD_OBJECT(
                        'specific_areas', oc.SpecificAreas,
                        'context_warning', oc.ContextWarning,
                        'risk_score', oc.RiskScore
                    ),
                    'artifacts', (
                        SELECT COALESCE(JSON_AGG(
                            JSON_BUILD_OBJECT(
                                'artifact_type_code', at_c.ArtifactTypeCode,
                                'artifact_type_name', at_c.Name,
                                'category', at_c.Category,
                                'data_type', adt_c.Name,
                                'value', CASE adt_c.Name
                                    WHEN 'number' THEN to_jsonb(ea_c.ValueNumber)
                                    WHEN 'date' THEN to_jsonb(ea_c.ValueDate)
                                    ELSE to_jsonb(ea_c.ValueText)
                                END,
                                'effective_date', ea_c.EffectiveDate,
                                'source', ea_c.Source,
                                'is_extracted', ea_c.IsExtracted,
                                'confidence_score', ea_c.ConfidenceScore
                            )
                        ), '[]'::json)
                        FROM EntityArtifacts ea_c
                        JOIN ArtifactTypes at_c ON ea_c.ArtifactTypeId = at_c.Id
                        JOIN ArtifactDataTypes adt_c ON at_c.ArtifactDataTypeId = adt_c.Id
                        WHERE ea_c.EntityType = 'Country'
                          AND ea_c.EntityId = c.Id
                          AND NOT ea_c.IsDeleted
                          AND (ea_c.EffectiveDate IS NULL OR ea_c.EffectiveDate <= CURRENT_TIMESTAMP)
                    )
                )
            ), '[]'::json)
            FROM OpportunityCountry oc
            JOIN Countries c ON oc.CountryId = c.Id
            WHERE oc.OpportunityId = o.Id
        ),
        
        'partners', JSON_BUILD_OBJECT(
            'funding_partners', (
                SELECT COALESCE(JSON_AGG(
                    JSON_BUILD_OBJECT(
                        'relationship_type', 'OpportunityFundingPartner',
                        'partner_id', p.Id,
                        'partner_name', p.Name,
                        'relationship_context', JSON_BUILD_OBJECT(
                            'funding_amount', ofp.Amount,
                            'funding_percentage', ofp.Percentage,
                            'currency_code', cur.Code
                        ),
                        'artifacts', (
                            SELECT COALESCE(JSON_AGG(
                                JSON_BUILD_OBJECT(
                                    'artifact_type_code', at_p.ArtifactTypeCode,
                                    'artifact_type_name', at_p.Name,
                                    'value', CASE adt_p.Name
                                        WHEN 'number' THEN to_jsonb(ea_p.ValueNumber)
                                        ELSE to_jsonb(ea_p.ValueText)
                                    END,
                                    'effective_date', ea_p.EffectiveDate
                                )
                            ), '[]'::json)
                            FROM EntityArtifacts ea_p
                            JOIN ArtifactTypes at_p ON ea_p.ArtifactTypeId = at_p.Id
                            JOIN ArtifactDataTypes adt_p ON at_p.ArtifactDataTypeId = adt_p.Id
                            WHERE ea_p.EntityType = 'Partner'
                              AND ea_p.EntityId = p.Id
                              AND NOT ea_p.IsDeleted
                        )
                    )
                ), '[]'::json)
                FROM OpportunityFundingPartner ofp
                JOIN Partners p ON ofp.PartnerId = p.Id
                LEFT JOIN Currencies cur ON ofp.CurrencyId = cur.Id
                WHERE ofp.OpportunityId = o.Id
            ),
            'client_partners', (
                SELECT COALESCE(JSON_AGG(
                    JSON_BUILD_OBJECT(
                        'relationship_type', 'OpportunityClientPartner',
                        'partner_id', p.Id,
                        'partner_name', p.Name,
                        'artifacts', (
                            SELECT COALESCE(JSON_AGG(
                                JSON_BUILD_OBJECT(
                                    'artifact_type_code', at_p.ArtifactTypeCode,
                                    'value', ea_p.ValueText
                                )
                            ), '[]'::json)
                            FROM EntityArtifacts ea_p
                            JOIN ArtifactTypes at_p ON ea_p.ArtifactTypeId = at_p.Id
                            WHERE ea_p.EntityType = 'Partner'
                              AND ea_p.EntityId = p.Id
                              AND NOT ea_p.IsDeleted
                        )
                    )
                ), '[]'::json)
                FROM OpportunityClientPartner ocp
                JOIN Partners p ON ocp.PartnerId = p.Id
                WHERE ocp.OpportunityId = o.Id
            )
        ),
        
        'organization_units', (
            SELECT COALESCE(JSON_AGG(
                JSON_BUILD_OBJECT(
                    'relationship_type', 'ResponsibleOrgUnit',
                    'orgunit_id', ou_rel.Id,
                    'orgunit_name', ou_rel.Name,
                    'orgunit_type', ou_rel.Type,
                    'artifacts', (
                        SELECT COALESCE(JSON_AGG(
                            JSON_BUILD_OBJECT(
                                'artifact_type_code', at_ou.ArtifactTypeCode,
                                'artifact_type_name', at_ou.Name,
                                'value', CASE adt_ou.Name
                                    WHEN 'number' THEN to_jsonb(ea_ou.ValueNumber)
                                    ELSE to_jsonb(ea_ou.ValueText)
                                END,
                                'effective_date', ea_ou.EffectiveDate
                            )
                        ), '[]'::json)
                        FROM EntityArtifacts ea_ou
                        JOIN ArtifactTypes at_ou ON ea_ou.ArtifactTypeId = at_ou.Id
                        JOIN ArtifactDataTypes adt_ou ON at_ou.ArtifactDataTypeId = adt_ou.Id
                        WHERE ea_ou.EntityType = 'OrganizationHierarchy'
                          AND ea_ou.EntityId = ou_rel.Id
                          AND NOT ea_ou.IsDeleted
                    )
                )
            ), '[]'::json)
            FROM OrganizationHierarchies ou_rel
            WHERE ou_rel.Id = o.ResponsibleOrgUnitId
        )
    ) as related_entities,
    
    -- Build artifact index for fast lookups
    (
        WITH all_artifacts AS (
            -- Direct artifacts
            SELECT at.ArtifactTypeCode, 
                   CASE adt.Name
                       WHEN 'number' THEN to_jsonb(ea.ValueNumber)
                       WHEN 'date' THEN to_jsonb(ea.ValueDate)
                       ELSE to_jsonb(ea.ValueText)
                   END as value
            FROM EntityArtifacts ea
            JOIN ArtifactTypes at ON ea.ArtifactTypeId = at.Id
            JOIN ArtifactDataTypes adt ON at.ArtifactDataTypeId = adt.Id
            WHERE ea.EntityType = 'Opportunity' AND ea.EntityId = o.Id AND NOT ea.IsDeleted
            
            UNION ALL
            
            -- Country artifacts
            SELECT at.ArtifactTypeCode,
                   CASE adt.Name
                       WHEN 'number' THEN to_jsonb(ea.ValueNumber)
                       ELSE to_jsonb(ea.ValueText)
                   END as value
            FROM OpportunityCountry oc
            JOIN EntityArtifacts ea ON ea.EntityType = 'Country' AND ea.EntityId = oc.CountryId
            JOIN ArtifactTypes at ON ea.ArtifactTypeId = at.Id
            JOIN ArtifactDataTypes adt ON at.ArtifactDataTypeId = adt.Id
            WHERE oc.OpportunityId = o.Id AND NOT ea.IsDeleted
            
            -- Add Partner and OrgUnit artifacts similarly...
        )
        SELECT JSON_OBJECT_AGG(
            ArtifactTypeCode,
            artifact_values
        )
        FROM (
            SELECT ArtifactTypeCode, JSON_AGG(DISTINCT value) as artifact_values
            FROM all_artifacts
            GROUP BY ArtifactTypeCode
        ) indexed_artifacts
    ) as artifact_index,
    
    -- Counts
    (SELECT COUNT(*) FROM EntityArtifacts ea 
     WHERE ea.EntityType = 'Opportunity' AND ea.EntityId = o.Id AND NOT ea.IsDeleted
    ) as artifact_count,
    
    (
        (SELECT COUNT(DISTINCT oc.CountryId) FROM OpportunityCountry oc WHERE oc.OpportunityId = o.Id) +
        (SELECT COUNT(DISTINCT ofp.PartnerId) FROM OpportunityFundingPartner ofp WHERE ofp.OpportunityId = o.Id) +
        (SELECT COUNT(DISTINCT ocp.PartnerId) FROM OpportunityClientPartner ocp WHERE ocp.OpportunityId = o.Id) +
        (CASE WHEN o.ResponsibleOrgUnitId IS NOT NULL THEN 1 ELSE 0 END)
    ) as related_entity_count,
    
    -- Relationship graph for visualization
    (
        WITH nodes AS (
            SELECT JSON_BUILD_OBJECT('id', CONCAT('Opportunity-', o.Id), 'type', 'Opportunity', 'name', o.Name) as node
            UNION ALL
            SELECT JSON_BUILD_OBJECT('id', CONCAT('Country-', c.Id), 'type', 'Country', 'name', c.Name)
            FROM OpportunityCountry oc JOIN Countries c ON oc.CountryId = c.Id
            WHERE oc.OpportunityId = o.Id
            UNION ALL
            SELECT JSON_BUILD_OBJECT('id', CONCAT('Partner-', p.Id), 'type', 'Partner', 'name', p.Name)
            FROM OpportunityFundingPartner ofp JOIN Partners p ON ofp.PartnerId = p.Id
            WHERE ofp.OpportunityId = o.Id
            -- Add other nodes...
        ),
        edges AS (
            SELECT JSON_BUILD_OBJECT(
                'from', CONCAT('Opportunity-', oc.OpportunityId),
                'to', CONCAT('Country-', oc.CountryId),
                'type', 'OpportunityCountry',
                'risk', oc.RiskScore
            ) as edge
            FROM OpportunityCountry oc
            WHERE oc.OpportunityId = o.Id
            UNION ALL
            SELECT JSON_BUILD_OBJECT(
                'from', CONCAT('Opportunity-', ofp.OpportunityId),
                'to', CONCAT('Partner-', ofp.PartnerId),
                'type', 'FundingPartner',
                'amount', ofp.Amount
            )
            FROM OpportunityFundingPartner ofp
            WHERE ofp.OpportunityId = o.Id
            -- Add other edges...
        )
        SELECT JSON_BUILD_OBJECT(
            'nodes', (SELECT JSON_AGG(node) FROM nodes),
            'edges', (SELECT JSON_AGG(edge) FROM edges)
        )
    ) as relationship_graph,
    
    GREATEST(
        o.LastModifiedDate,
        (SELECT MAX(ea.LastModifiedDate) FROM EntityArtifacts ea 
         WHERE ea.EntityType = 'Opportunity' AND ea.EntityId = o.Id)
    ) as last_updated,
    
    CURRENT_TIMESTAMP as etl_timestamp

FROM Opportunities o
LEFT JOIN WorkflowStages ws ON o.WorkflowStageId = ws.Id
WHERE NOT o.IsDeleted;
```

### Materialized Views for Common Reports

**View: `mv_opportunity_country_artifacts_flat`**
```sql
CREATE MATERIALIZED VIEW mv_opportunity_country_artifacts_flat AS
SELECT 
    e.entity_id as opportunity_id,
    e.entity_name as opportunity_name,
    country->>'country_id' as country_id,
    country->>'country_name' as country_name,
    country->>'country_iso2' as country_iso2,
    (country->'relationship_context'->>'risk_score')::decimal as risk_score,
    artifact->>'artifact_type_code' as artifact_type_code,
    artifact->>'artifact_type_name' as artifact_type_name,
    artifact->>'category' as artifact_category,
    artifact->'value' as artifact_value,
    (artifact->>'effective_date')::date as artifact_effective_date,
    (artifact->>'is_extracted')::boolean as is_extracted,
    (artifact->>'confidence_score')::decimal as confidence_score
FROM entity_documents_with_relationships e,
     jsonb_array_elements(e.related_entities->'countries') as country,
     jsonb_array_elements(country->'artifacts') as artifact
WHERE e.entity_type = 'Opportunity';

CREATE INDEX idx_mv_opp_country_opp_id ON mv_opportunity_country_artifacts_flat(opportunity_id);
CREATE INDEX idx_mv_opp_country_country_id ON mv_opportunity_country_artifacts_flat(country_id);
CREATE INDEX idx_mv_opp_country_artifact_type ON mv_opportunity_country_artifacts_flat(artifact_type_code);
```

### Reporting Query Examples

#### Report 1: All Artifacts for Opportunity (Direct + Related)

```sql
-- Single JSON document contains everything
SELECT 
    entity_name as opportunity_name,
    jsonb_pretty(direct_artifacts) as direct_artifacts,
    jsonb_pretty(related_entities) as related_entities,
    artifact_count,
    related_entity_count
FROM entity_documents_with_relationships
WHERE entity_type = 'Opportunity'
  AND entity_id = 123;
```

#### Report 2: Opportunities with High-Risk Countries

```sql
-- Query using artifact_index for fast lookup
SELECT 
    entity_id,
    entity_name,
    artifact_index->'FragileCategory' as fragile_categories,
    related_entities->'countries' as country_details
FROM entity_documents_with_relationships
WHERE entity_type = 'Opportunity'
  AND artifact_index ? 'FragileCategory'
  AND artifact_index->'FragileCategory' @> '["High Fragility"]'::jsonb;
```

#### Report 3: Using Flattened Materialized View

```sql
SELECT 
    opportunity_name,
    country_name,
    risk_score,
    artifact_type_name,
    artifact_value,
    confidence_score
FROM mv_opportunity_country_artifacts_flat
WHERE artifact_type_code = 'FragileCategory'
  AND risk_score > 2.5
ORDER BY risk_score DESC, opportunity_name;
```

#### Report 4: Entity Relationship Graph Visualization

```sql
-- Export relationship graph for visualization tools (D3.js, Gephi, etc.)
SELECT 
    entity_id,
    entity_name,
    jsonb_pretty(relationship_graph) as graph_json
FROM entity_documents_with_relationships
WHERE entity_type = 'Opportunity'
  AND entity_id = 123;
```

### Reporting Benefits

- ✅ **Complete Context**: Single document contains all related entity data
- ✅ **Flexible Querying**: JSONB operators enable complex filtering
- ✅ **Fast Indexing**: artifact_index provides O(1) lookups for specific artifact types
- ✅ **Nested Traversal**: Natural representation of entity relationships
- ✅ **Materialized Views**: Flatten for traditional SQL reporting when needed
- ✅ **Graph Visualization**: Relationship graph ready for network analysis tools
- ✅ **API Integration**: JSON documents directly consumable by REST APIs

---

## Comparison Matrix: Strategy Selection Guide

| **Criteria** | **Strategy 1: Star + Bridge** | **Strategy 2: Wide Pivoted** | **Strategy 3: Hybrid JSON** |
|--------------|-------------------------------|------------------------------|----------------------------|
| **Query Complexity** | Medium (multiple joins) | Low (single table) | Medium-High (JSON operators) |
| **Related Entity Handling** | ⭐⭐⭐⭐⭐ Explicit bridges | ⭐⭐⭐⭐ Arrays/JSON | ⭐⭐⭐⭐⭐ Nested documents |
| **Performance** | ⭐⭐⭐⭐⭐ Indexed joins | ⭐⭐⭐⭐⭐ Pre-aggregated | ⭐⭐⭐⭐ JSON indexing |
| **Storage Efficiency** | ⭐⭐⭐⭐ Normalized | ⭐⭐ Denormalized | ⭐⭐⭐ Compressed JSON |
| **Schema Flexibility** | ⭐⭐ New tables needed | ⭐⭐ ALTER TABLE | ⭐⭐⭐⭐⭐ Schemaless |
| **Temporal Tracking** | ⭐⭐⭐⭐⭐ SCD Type 2 | ⭐⭐⭐ Snapshot tables | ⭐⭐⭐ JSON versioning |
| **BI Tool Support** | ⭐⭐⭐⭐⭐ Native | ⭐⭐⭐⭐⭐ Excel-friendly | ⭐⭐⭐⭐ Modern tools |
| **Relationship Tracing** | ⭐⭐⭐⭐⭐ Explicit lineage | ⭐⭐⭐ Context columns | ⭐⭐⭐⭐⭐ Graph structure |
| **ETL Complexity** | ⭐⭐⭐ Multiple stages | ⭐⭐⭐⭐ Complex pivots | ⭐⭐ JSON aggregation |
| **Multi-Entity Reports** | ⭐⭐⭐⭐⭐ Join across bridges | ⭐⭐⭐⭐ Parallel arrays | ⭐⭐⭐⭐⭐ Nested navigation |

## Recommendation

### **Primary Recommendation: Strategy 1 (Star Schema with Bridge Tables)**

**Why:**
- ✅ **Explicit Relationship Modeling**: Bridge tables clearly represent how entities relate
- ✅ **Maintainability**: Easier to understand and troubleshoot
- ✅ **BI Tool Compatibility**: Works with all traditional reporting tools
- ✅ **Performance**: Optimized for analytical queries with proper indexing
- ✅ **Auditability**: Clear data lineage from opportunity → relationship → entity → artifact

**Best For:**
- Enterprise data warehouses (Snowflake, Redshift, Synapse)
- Organizations using traditional BI tools (Tableau, Power BI, Looker)
- Teams requiring strong data governance and auditability
- Complex multi-entity relationship reporting

### **Secondary Recommendation: Strategy 3 (Hybrid JSON) with Materialized Views**

**Why:**
- ✅ **Flexibility**: Easy to add new artifact types and relationships
- ✅ **Modern Analytics**: Great for API-driven reporting and data science
- ✅ **Graph Analysis**: Natural representation for network analysis
- ✅ **Hybrid Approach**: Combine JSON flexibility with SQL performance via materialized views

**Best For:**
- Modern cloud data warehouses (BigQuery, Snowflake with VARIANT)
- Organizations with data science/ML teams
- API-first architecture
- Rapidly evolving artifact schemas

### **Use Strategy 2 (Wide Pivoted) When:**
- Business users primarily work in Excel
- Limited artifact types (< 20 per entity type)
- Simple reporting needs
- Performance is the absolute top priority

---

## Implementation Roadmap

### Phase 1: Foundation (Weeks 1-2)
1. ✅ Define entity relationship mappings
2. ✅ Create dimension tables (dim_entity, dim_artifact_type, etc.)
3. ✅ Load fact_entity_artifacts from source system
4. ✅ Validate artifact data quality

### Phase 2: Related Entity Integration (Weeks 3-4)
1. ✅ Create bridge tables for relationships
2. ✅ Build ETL processes to populate bridges
3. ✅ Implement SCD Type 2 for temporal tracking
4. ✅ Create initial reports for validation

### Phase 3: Optimization & Testing (Weeks 5-6)
1. ✅ Index tuning and query optimization
2. ✅ ETL scheduling and incremental loads
3. ✅ Performance testing with production-scale data
4. ✅ User acceptance testing

### Phase 4: Production Deployment (Week 7)
1. ✅ Production deployment
2. ✅ Monitoring and alerting setup
3. ✅ User training and documentation
4. ✅ Ongoing optimization

---

## Appendix: Technology-Specific Considerations

### Snowflake
- Use `VARIANT` type for JSON storage in Strategy 3
- Leverage `FLATTEN` function for nested JSON queries
- Implement `STREAMS` and `TASKS` for incremental ETL

### Amazon Redshift
- Use `SUPER` data type for semi-structured data
- Consider distribution keys on entity_dim_key
- Use `SORTKEY` on temporal columns

### Google BigQuery
- Use `STRUCT` and `ARRAY` types for nested data
- Partition tables by date for temporal queries
- Leverage `UNNEST` for array operations

### PostgreSQL
- Use `JSONB` with GIN indexes
- Implement `BRIN` indexes for large temporal datasets
- Consider `pg_partman` for table partitioning

---

**Document Control:**
- Version: 1.0
- Last Updated: October 31, 2024
- Next Review: January 2025
- Owner: UNOPS Opportunity+ Development Team

