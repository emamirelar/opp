# Generic Rules Engine Design for UNOPS Opportunity+ System

**Document Version:** 1.0  
**Date:** October 31, 2024  
**Author:** UNOPS Opportunity+ Development Team

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [System Overview](#system-overview)
3. [Database Schema Design](#database-schema-design)
4. [Rule Types](#rule-types)
5. [Implementation Code](#implementation-code)
6. [Usage Examples](#usage-examples)
7. [Integration Guide](#integration-guide)

---

## Executive Summary

This document defines a **generic, configurable rules engine** for the UNOPS Opportunity+ system. The rules engine calculates outputs (e.g., Opportunity Risk, Partner Risk Score, Country Risk Assessment) based on predefined, database-driven rules that evaluate:

- **Entity Artifacts** (e.g., Country FragileState, HostCountryAgreement)
- **Relationship Data** (e.g., Similar Projects, Deliverable diversity)
- **People/Organizational Factors** (e.g., User expertise, Org Unit countries)
- **AI-Based Analysis** (e.g., Similar project detection, sentiment analysis)
- **Calculated Metrics** (e.g., Budget variance, Output level distribution)

### Key Features

- ✅ **Generic**: Calculate any output for any entity type
- ✅ **Database-Driven**: Rules configured in database, no code changes needed
- ✅ **Weighted Scoring**: Support for weighted rule contributions
- ✅ **Historical Tracking**: Rule execution history and audit trail
- ✅ **Flexible Conditions**: Multiple condition types (artifact values, counts, calculations, AI)
- ✅ **Performance Optimized**: Efficient querying with caching support
- ✅ **Extensible**: Easy to add new rule types and output types

---

## System Overview

### Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Trigger Events                        │
│  • Opportunity Opened                                   │
│  • User Clicks Refresh                                  │
│  • Entity Data Changed                                  │
│  • Scheduled Recalculation                              │
└────────────────┬────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────┐
│              Rules Engine Orchestrator                   │
│  • Load Active Rules for Output Type                    │
│  • Execute Rules in Priority Order                      │
│  • Calculate Weighted Scores                            │
│  • Store Execution Results                              │
└────────────────┬────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────┐
│                    Rule Evaluators                       │
│  ┌───────────────────────────────────────────────────┐  │
│  │ Artifact Rule Evaluator                           │  │
│  │ • Query EntityArtifact by type/entity             │  │
│  │ • Compare values against thresholds               │  │
│  └───────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────┐  │
│  │ Relationship Rule Evaluator                       │  │
│  │ • Count related entities                          │  │
│  │ • Evaluate diversity metrics                      │  │
│  └───────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────┐  │
│  │ AI Rule Evaluator                                 │  │
│  │ • Call AI service for similarity analysis         │  │
│  │ • Use confidence scores                           │  │
│  └───────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────┐  │
│  │ Calculation Rule Evaluator                        │  │
│  │ • Execute custom calculations                     │  │
│  │ • Aggregate metrics                               │  │
│  └───────────────────────────────────────────────────┘  │
└────────────────┬────────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────────────────────────┐
│              Output Calculation Result                   │
│  • Final Calculated Value                               │
│  • Risk Level (e.g., High, Medium, Low)                 │
│  • Contributing Factors with Scores                     │
│  • Execution Timestamp                                  │
│  • Confidence Score                                     │
└─────────────────────────────────────────────────────────┘
```

### Example: Opportunity Risk Calculation

Based on the screenshot, **Opportunity Risk** considers:

1. **Country Factors** (from Country artifacts):
   - Host Country Agreement (missing = higher risk)
   - Country/Regional Instability
   - Fragile State (YES = higher risk)

2. **P3M Entity Factors** (from Opportunity deliverables):
   - Outputs at different levels (diversity = higher risk)
   - Similar Projects (available + good = lower risk)

3. **People Factors** (from Opportunity stakeholders):
   - Functional Area expertise
   - Standardized Title/ICS Level
   - Works At (Org Unit countries alignment)

---

## Database Schema Design

### Core Tables

#### 1. RuleOutput
Defines the outputs that can be calculated (e.g., OpportunityRisk, PartnerRiskScore).

```sql
CREATE TABLE RuleOutput (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(255) NOT NULL, -- e.g., "Opportunity Risk", "Partner Risk Score"
    OutputCode VARCHAR(100) NOT NULL UNIQUE, -- e.g., "OPPORTUNITY_RISK", "PARTNER_RISK"
    Description TEXT,
    EntityType VARCHAR(100) NOT NULL, -- e.g., "Opportunity", "Partner", "Country"
    OutputDataType VARCHAR(50) NOT NULL, -- "Number", "Text", "RiskLevel", "Score"
    
    -- Scoring configuration
    MinValue DECIMAL(18, 4), -- Minimum possible value (e.g., 0)
    MaxValue DECIMAL(18, 4), -- Maximum possible value (e.g., 100)
    DefaultValue DECIMAL(18, 4), -- Default value if no rules match
    
    -- Risk level thresholds (if applicable)
    LowThreshold DECIMAL(18, 4), -- Values <= this are "Low Risk"
    MediumThreshold DECIMAL(18, 4), -- Values <= this are "Medium Risk"
    HighThreshold DECIMAL(18, 4), -- Values > this are "High Risk"
    
    -- Metadata
    IsActive BOOLEAN DEFAULT TRUE,
    RequiresAI BOOLEAN DEFAULT FALSE, -- Whether AI service is needed
    CacheMinutes INT DEFAULT 60, -- How long to cache results
    
    -- Audit fields
    Status INT DEFAULT 1, -- EntityStatus enum
    CreatedBy INT,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastModifiedBy INT,
    LastModifiedDate TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT FK_RuleOutput_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES "PAOUser"(Id),
    CONSTRAINT FK_RuleOutput_ModifiedBy FOREIGN KEY (LastModifiedBy) REFERENCES "PAOUser"(Id)
);

CREATE INDEX IX_RuleOutput_EntityType ON RuleOutput(EntityType);
CREATE INDEX IX_RuleOutput_OutputCode ON RuleOutput(OutputCode);
```

#### 2. RuleDefinition
Defines individual rules that contribute to an output calculation.

```sql
CREATE TABLE RuleDefinition (
    Id SERIAL PRIMARY KEY,
    RuleOutputId INT NOT NULL, -- FK to RuleOutput
    
    -- Rule identification
    RuleName VARCHAR(255) NOT NULL, -- e.g., "Country Fragile State Check"
    RuleCode VARCHAR(100) NOT NULL, -- e.g., "COUNTRY_FRAGILE_STATE"
    Description TEXT,
    Category VARCHAR(100), -- e.g., "Country", "P3M Entity", "People"
    
    -- Rule type
    RuleType VARCHAR(50) NOT NULL, -- "Artifact", "Relationship", "Calculation", "AI", "Count"
    
    -- Scoring
    Weight DECIMAL(5, 2) DEFAULT 1.0, -- Relative weight (0.0 to 1.0)
    MaxScore DECIMAL(18, 4) DEFAULT 100, -- Maximum score this rule can contribute
    
    -- Execution control
    ExecutionOrder INT DEFAULT 0, -- Order of execution (lower = earlier)
    IsActive BOOLEAN DEFAULT TRUE,
    IsRequired BOOLEAN DEFAULT FALSE, -- Must evaluate successfully
    
    -- Related entity configuration
    RelatedEntityType VARCHAR(100), -- e.g., "Country", "Partner" (for relationship rules)
    RelationshipPath VARCHAR(255), -- e.g., "OpportunityCountry->Country" (navigation path)
    
    -- Audit fields
    Status INT DEFAULT 1,
    CreatedBy INT,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastModifiedBy INT,
    LastModifiedDate TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT FK_RuleDefinition_RuleOutput FOREIGN KEY (RuleOutputId) REFERENCES RuleOutput(Id),
    CONSTRAINT FK_RuleDefinition_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES "PAOUser"(Id),
    CONSTRAINT FK_RuleDefinition_ModifiedBy FOREIGN KEY (LastModifiedBy) REFERENCES "PAOUser"(Id)
);

CREATE INDEX IX_RuleDefinition_RuleOutput ON RuleDefinition(RuleOutputId);
CREATE INDEX IX_RuleDefinition_RuleType ON RuleDefinition(RuleType);
CREATE INDEX IX_RuleDefinition_ExecutionOrder ON RuleDefinition(ExecutionOrder);
```

#### 3. RuleCondition
Defines conditions that must be met for a rule to apply.

```sql
CREATE TABLE RuleCondition (
    Id SERIAL PRIMARY KEY,
    RuleDefinitionId INT NOT NULL, -- FK to RuleDefinition
    
    -- Condition configuration
    ConditionType VARCHAR(50) NOT NULL, -- "ArtifactValue", "ArtifactExists", "Count", "Calculation", "AI"
    
    -- Artifact-based conditions
    ArtifactTypeCode VARCHAR(100), -- e.g., "FRAGILE_CATEGORY", "HOST_COUNTRY_AGREEMENT"
    
    -- Comparison operator
    Operator VARCHAR(20), -- "Equals", "NotEquals", "GreaterThan", "LessThan", "Contains", "Exists", "NotExists"
    
    -- Comparison values
    ComparisonValue TEXT, -- Expected value (e.g., "YES", "HIGH", "> 3")
    ComparisonValueNumeric DECIMAL(18, 4), -- For numeric comparisons
    
    -- Count-based conditions
    MinCount INT, -- Minimum count (for relationship rules)
    MaxCount INT, -- Maximum count (for relationship rules)
    
    -- Logical grouping
    LogicalOperator VARCHAR(10) DEFAULT 'AND', -- "AND", "OR" (for multiple conditions)
    ConditionGroup INT DEFAULT 0, -- Group conditions together
    
    -- Metadata
    IsActive BOOLEAN DEFAULT TRUE,
    
    -- Audit fields
    Status INT DEFAULT 1,
    CreatedBy INT,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastModifiedBy INT,
    LastModifiedDate TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT FK_RuleCondition_RuleDefinition FOREIGN KEY (RuleDefinitionId) REFERENCES RuleDefinition(Id),
    CONSTRAINT FK_RuleCondition_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES "PAOUser"(Id),
    CONSTRAINT FK_RuleCondition_ModifiedBy FOREIGN KEY (LastModifiedBy) REFERENCES "PAOUser"(Id)
);

CREATE INDEX IX_RuleCondition_RuleDefinition ON RuleCondition(RuleDefinitionId);
CREATE INDEX IX_RuleCondition_ArtifactType ON RuleCondition(ArtifactTypeCode);
```

#### 4. RuleAction
Defines what score to assign when rule conditions are met.

```sql
CREATE TABLE RuleAction (
    Id SERIAL PRIMARY KEY,
    RuleDefinitionId INT NOT NULL, -- FK to RuleDefinition
    
    -- Action type
    ActionType VARCHAR(50) NOT NULL, -- "AssignScore", "IncrementScore", "SetRiskLevel", "CallAI"
    
    -- Score assignment
    ScoreValue DECIMAL(18, 4), -- Score to assign
    ScoreMultiplier DECIMAL(5, 2) DEFAULT 1.0, -- Multiplier for weighted scoring
    
    -- Risk level assignment
    RiskLevel VARCHAR(50), -- "Low", "Medium", "High", "Critical"
    
    -- AI configuration
    AIPromptTemplate TEXT, -- Template for AI prompt
    AIServiceEndpoint VARCHAR(500), -- AI service endpoint (if custom)
    
    -- Output formatting
    OutputMessage TEXT, -- Message to display (e.g., "Country is fragile state: +15 risk")
    OutputSeverity VARCHAR(20), -- "Info", "Warning", "Error"
    
    -- Metadata
    IsActive BOOLEAN DEFAULT TRUE,
    
    -- Audit fields
    Status INT DEFAULT 1,
    CreatedBy INT,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastModifiedBy INT,
    LastModifiedDate TIMESTAMP,
    IsDeleted BOOLEAN DEFAULT FALSE,
    
    CONSTRAINT FK_RuleAction_RuleDefinition FOREIGN KEY (RuleDefinitionId) REFERENCES RuleDefinition(Id),
    CONSTRAINT FK_RuleAction_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES "PAOUser"(Id),
    CONSTRAINT FK_RuleAction_ModifiedBy FOREIGN KEY (LastModifiedBy) REFERENCES "PAOUser"(Id)
);

CREATE INDEX IX_RuleAction_RuleDefinition ON RuleAction(RuleDefinitionId);
```

#### 5. RuleExecutionResult
Stores the results of rule execution for caching and history.

```sql
CREATE TABLE RuleExecutionResult (
    Id SERIAL PRIMARY KEY,
    
    -- Entity identification
    EntityType VARCHAR(100) NOT NULL, -- e.g., "Opportunity"
    EntityId INT NOT NULL, -- e.g., OpportunityId = 123
    
    -- Output information
    RuleOutputId INT NOT NULL, -- FK to RuleOutput
    OutputCode VARCHAR(100) NOT NULL, -- Denormalized for query performance
    
    -- Calculated result
    CalculatedValue DECIMAL(18, 4), -- Final calculated value
    CalculatedValueText TEXT, -- Text result (if applicable)
    RiskLevel VARCHAR(50), -- "Low", "Medium", "High", "Critical"
    ConfidenceScore DECIMAL(3, 2), -- Confidence in the result (0.0 to 1.0)
    
    -- Execution metadata
    ExecutionStartTime TIMESTAMP NOT NULL,
    ExecutionEndTime TIMESTAMP NOT NULL,
    ExecutionDurationMs INT, -- Duration in milliseconds
    RulesEvaluatedCount INT, -- Number of rules evaluated
    RulesMatchedCount INT, -- Number of rules that matched
    
    -- Caching
    IsCached BOOLEAN DEFAULT FALSE,
    CacheExpiryTime TIMESTAMP, -- When this result expires
    
    -- Audit fields
    CreatedBy INT,
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT FK_RuleExecutionResult_RuleOutput FOREIGN KEY (RuleOutputId) REFERENCES RuleOutput(Id),
    CONSTRAINT FK_RuleExecutionResult_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES "PAOUser"(Id)
);

CREATE INDEX IX_RuleExecutionResult_Entity ON RuleExecutionResult(EntityType, EntityId);
CREATE INDEX IX_RuleExecutionResult_OutputCode ON RuleExecutionResult(OutputCode);
CREATE INDEX IX_RuleExecutionResult_CacheExpiry ON RuleExecutionResult(CacheExpiryTime) WHERE IsCached = TRUE;
```

#### 6. RuleExecutionLog
Detailed log of each rule evaluation for debugging and audit.

```sql
CREATE TABLE RuleExecutionLog (
    Id SERIAL PRIMARY KEY,
    RuleExecutionResultId INT NOT NULL, -- FK to RuleExecutionResult
    RuleDefinitionId INT NOT NULL, -- FK to RuleDefinition
    
    -- Rule information
    RuleCode VARCHAR(100) NOT NULL,
    RuleName VARCHAR(255) NOT NULL,
    RuleType VARCHAR(50) NOT NULL,
    
    -- Evaluation result
    Matched BOOLEAN NOT NULL, -- Did the rule conditions match?
    Score DECIMAL(18, 4), -- Score contributed by this rule
    Weight DECIMAL(5, 2), -- Weight applied
    WeightedScore DECIMAL(18, 4), -- Final weighted score
    
    -- Condition evaluation details
    ConditionsEvaluated INT, -- Number of conditions evaluated
    ConditionsMatched INT, -- Number of conditions that matched
    EvaluationDetails TEXT, -- JSON with detailed evaluation info
    
    -- Performance
    EvaluationDurationMs INT, -- Time to evaluate this rule
    
    -- Output
    OutputMessage TEXT, -- Message generated by this rule
    OutputSeverity VARCHAR(20),
    
    -- Audit fields
    CreatedDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT FK_RuleExecutionLog_Result FOREIGN KEY (RuleExecutionResultId) REFERENCES RuleExecutionResult(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RuleExecutionLog_Rule FOREIGN KEY (RuleDefinitionId) REFERENCES RuleDefinition(Id)
);

CREATE INDEX IX_RuleExecutionLog_Result ON RuleExecutionLog(RuleExecutionResultId);
CREATE INDEX IX_RuleExecutionLog_Rule ON RuleExecutionLog(RuleDefinitionId);
```

---

## Rule Types

### 1. Artifact-Based Rules

Evaluate entity artifacts from the `EntityArtifact` table.

**Example**: Check if a Country is a "Fragile State"

```sql
-- Rule Definition
INSERT INTO RuleDefinition (RuleOutputId, RuleName, RuleCode, Description, RuleType, Weight, MaxScore, ExecutionOrder, RelatedEntityType, RelationshipPath)
VALUES (1, 'Country Fragile State Check', 'COUNTRY_FRAGILE_STATE', 'Check if opportunity countries are fragile states', 'Artifact', 0.25, 100, 10, 'Country', 'OpportunityCountry->Country');

-- Rule Condition
INSERT INTO RuleCondition (RuleDefinitionId, ConditionType, ArtifactTypeCode, Operator, ComparisonValue)
VALUES (1, 'ArtifactValue', 'FRAGILE_CATEGORY', 'Equals', 'YES');

-- Rule Action
INSERT INTO RuleAction (RuleDefinitionId, ActionType, ScoreValue, OutputMessage, OutputSeverity)
VALUES (1, 'AssignScore', 25.0, 'Country is a fragile state: +25 risk points', 'Warning');
```

### 2. Relationship/Count Rules

Count related entities or check relationship patterns.

**Example**: Check if Opportunity has multiple Output levels

```sql
-- Rule Definition
INSERT INTO RuleDefinition (RuleOutputId, RuleName, RuleCode, Description, RuleType, Weight, MaxScore, ExecutionOrder)
VALUES (1, 'Output Level Diversity', 'OUTPUT_LEVEL_DIVERSITY', 'Check if opportunity has outputs at different levels', 'Count', 0.15, 100, 20);

-- Rule Condition (count distinct output levels)
INSERT INTO RuleCondition (RuleDefinitionId, ConditionType, Operator, MinCount)
VALUES (2, 'Count', 'GreaterThan', 2); -- More than 2 different output levels

-- Rule Action
INSERT INTO RuleAction (RuleDefinitionId, ActionType, ScoreValue, OutputMessage, OutputSeverity)
VALUES (2, 'AssignScore', 15.0, 'Opportunity has outputs at multiple levels: +15 risk points', 'Info');
```

### 3. AI-Based Rules

Use AI service for similarity analysis or sentiment detection.

**Example**: Find similar projects and assess risk

```sql
-- Rule Definition
INSERT INTO RuleDefinition (RuleOutputId, RuleName, RuleCode, Description, RuleType, Weight, MaxScore, ExecutionOrder)
VALUES (1, 'Similar Projects Assessment', 'SIMILAR_PROJECTS', 'Use AI to find and assess similar projects', 'AI', 0.30, 100, 30);

-- Rule Condition
INSERT INTO RuleCondition (RuleDefinitionId, ConditionType, Operator)
VALUES (3, 'AI', 'Exists'); -- AI service must return results

-- Rule Action
INSERT INTO RuleAction (RuleDefinitionId, ActionType, ScoreMultiplier, AIPromptTemplate, OutputMessage)
VALUES (3, 'CallAI', -1.0, 'Find similar projects to {{OpportunityName}} in {{CountryName}} and assess risk based on past performance', 'Similar projects found - risk adjusted based on performance');
```

### 4. Calculation Rules

Execute custom calculations or aggregations.

**Example**: Calculate average stakeholder ICS level

```sql
-- Rule Definition
INSERT INTO RuleDefinition (RuleOutputId, RuleName, RuleCode, Description, RuleType, Weight, MaxScore, ExecutionOrder)
VALUES (1, 'Average Stakeholder ICS Level', 'AVG_STAKEHOLDER_ICS', 'Calculate average ICS level of opportunity stakeholders', 'Calculation', 0.20, 100, 40);

-- Rule Condition (check if average ICS level is below threshold)
INSERT INTO RuleCondition (RuleDefinitionId, ConditionType, Operator, ComparisonValueNumeric)
VALUES (4, 'Calculation', 'LessThan', 9.0); -- Average ICS level < 9

-- Rule Action
INSERT INTO RuleAction (RuleDefinitionId, ActionType, ScoreValue, OutputMessage, OutputSeverity)
VALUES (4, 'AssignScore', 20.0, 'Average stakeholder ICS level is low: +20 risk points', 'Warning');
```

### 5. People/Organization Rules

Evaluate organizational factors and user attributes.

**Example**: Check if opportunity org unit works in same region as opportunity countries

```sql
-- Rule Definition
INSERT INTO RuleDefinition (RuleOutputId, RuleName, RuleCode, Description, RuleType, Weight, MaxScore, ExecutionOrder, RelatedEntityType)
VALUES (1, 'Org Unit Regional Alignment', 'ORG_UNIT_REGION', 'Check if responsible org unit has experience in opportunity countries', 'Relationship', 0.10, 100, 50, 'OrganizationHierarchy');

-- Rule Condition
INSERT INTO RuleCondition (RuleDefinitionId, ConditionType, Operator)
VALUES (5, 'Calculation', 'Equals'); -- Check region match

-- Rule Action
INSERT INTO RuleAction (RuleDefinitionId, ActionType, ScoreValue, ScoreMultiplier, OutputMessage)
VALUES (5, 'AssignScore', -10.0, 1.0, 'Org unit has regional experience: -10 risk points', 'Info');
```

---

## Implementation Code

### Entity Classes

```csharp
using UNOPS.PAO.Domain.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace UNOPS.PAO.Domain.Entities;

/// <summary>
/// Defines outputs that can be calculated by the rules engine
/// </summary>
public class RuleOutput : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    [MaxLength(255)]
    public required string Name { get; set; }
    
    [MaxLength(100)]
    public required string OutputCode { get; set; }
    
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public required string EntityType { get; set; }
    
    [MaxLength(50)]
    public required string OutputDataType { get; set; }
    
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? DefaultValue { get; set; }
    
    public decimal? LowThreshold { get; set; }
    public decimal? MediumThreshold { get; set; }
    public decimal? HighThreshold { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool RequiresAI { get; set; } = false;
    public int CacheMinutes { get; set; } = 60;
    
    public virtual ICollection<RuleDefinition> RuleDefinitions { get; set; } = new HashSet<RuleDefinition>();
}

/// <summary>
/// Defines individual rules that contribute to an output
/// </summary>
public class RuleDefinition : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    public int RuleOutputId { get; set; }
    public virtual RuleOutput? RuleOutput { get; set; }
    
    [MaxLength(255)]
    public required string RuleName { get; set; }
    
    [MaxLength(100)]
    public required string RuleCode { get; set; }
    
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(50)]
    public required string RuleType { get; set; }
    
    public decimal Weight { get; set; } = 1.0m;
    public decimal MaxScore { get; set; } = 100m;
    
    public int ExecutionOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public bool IsRequired { get; set; } = false;
    
    [MaxLength(100)]
    public string? RelatedEntityType { get; set; }
    
    [MaxLength(255)]
    public string? RelationshipPath { get; set; }
    
    public virtual ICollection<RuleCondition> Conditions { get; set; } = new HashSet<RuleCondition>();
    public virtual ICollection<RuleAction> Actions { get; set; } = new HashSet<RuleAction>();
}

/// <summary>
/// Defines conditions for rule evaluation
/// </summary>
public class RuleCondition : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    public int RuleDefinitionId { get; set; }
    public virtual RuleDefinition? RuleDefinition { get; set; }
    
    [MaxLength(50)]
    public required string ConditionType { get; set; }
    
    [MaxLength(100)]
    public string? ArtifactTypeCode { get; set; }
    
    [MaxLength(20)]
    public string? Operator { get; set; }
    
    public string? ComparisonValue { get; set; }
    public decimal? ComparisonValueNumeric { get; set; }
    
    public int? MinCount { get; set; }
    public int? MaxCount { get; set; }
    
    [MaxLength(10)]
    public string LogicalOperator { get; set; } = "AND";
    
    public int ConditionGroup { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Defines actions to take when rule conditions are met
/// </summary>
public class RuleAction : ModifiableDeletableEntity
{
    public new int Id { get; set; }
    
    public int RuleDefinitionId { get; set; }
    public virtual RuleDefinition? RuleDefinition { get; set; }
    
    [MaxLength(50)]
    public required string ActionType { get; set; }
    
    public decimal? ScoreValue { get; set; }
    public decimal ScoreMultiplier { get; set; } = 1.0m;
    
    [MaxLength(50)]
    public string? RiskLevel { get; set; }
    
    public string? AIPromptTemplate { get; set; }
    
    [MaxLength(500)]
    public string? AIServiceEndpoint { get; set; }
    
    public string? OutputMessage { get; set; }
    
    [MaxLength(20)]
    public string? OutputSeverity { get; set; }
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Stores rule execution results
/// </summary>
public class RuleExecutionResult
{
    public int Id { get; set; }
    
    [MaxLength(100)]
    public required string EntityType { get; set; }
    
    public int EntityId { get; set; }
    
    public int RuleOutputId { get; set; }
    public virtual RuleOutput? RuleOutput { get; set; }
    
    [MaxLength(100)]
    public required string OutputCode { get; set; }
    
    public decimal? CalculatedValue { get; set; }
    public string? CalculatedValueText { get; set; }
    
    [MaxLength(50)]
    public string? RiskLevel { get; set; }
    
    public decimal? ConfidenceScore { get; set; }
    
    public DateTime ExecutionStartTime { get; set; }
    public DateTime ExecutionEndTime { get; set; }
    public int ExecutionDurationMs { get; set; }
    
    public int RulesEvaluatedCount { get; set; }
    public int RulesMatchedCount { get; set; }
    
    public bool IsCached { get; set; } = false;
    public DateTime? CacheExpiryTime { get; set; }
    
    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public virtual ICollection<RuleExecutionLog> ExecutionLogs { get; set; } = new HashSet<RuleExecutionLog>();
}

/// <summary>
/// Detailed log of each rule evaluation
/// </summary>
public class RuleExecutionLog
{
    public int Id { get; set; }
    
    public int RuleExecutionResultId { get; set; }
    public virtual RuleExecutionResult? RuleExecutionResult { get; set; }
    
    public int RuleDefinitionId { get; set; }
    public virtual RuleDefinition? RuleDefinition { get; set; }
    
    [MaxLength(100)]
    public required string RuleCode { get; set; }
    
    [MaxLength(255)]
    public required string RuleName { get; set; }
    
    [MaxLength(50)]
    public required string RuleType { get; set; }
    
    public bool Matched { get; set; }
    public decimal? Score { get; set; }
    public decimal? Weight { get; set; }
    public decimal? WeightedScore { get; set; }
    
    public int ConditionsEvaluated { get; set; }
    public int ConditionsMatched { get; set; }
    public string? EvaluationDetails { get; set; }
    
    public int EvaluationDurationMs { get; set; }
    
    public string? OutputMessage { get; set; }
    
    [MaxLength(20)]
    public string? OutputSeverity { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
```

### Manager Interface

```csharp
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Business.Interfaces;

public interface IRulesEngineManager
{
    Task<RuleExecutionResultModel> CalculateOutputAsync(string entityType, int entityId, string outputCode, int currentUserId);
    Task<RuleExecutionResultModel?> GetCachedResultAsync(string entityType, int entityId, string outputCode);
    Task InvalidateCacheAsync(string entityType, int entityId, string outputCode);
}
```

### Manager Implementation (Partial - Core Logic)

```csharp
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Newtonsoft.Json;

namespace UNOPS.PAO.Business.Managers;

public class RulesEngineManager : IRulesEngineManager
{
    private readonly AppDbContext context;
    private readonly IMapper mapper;
    private readonly IManagerWrapper managerWrapper;
    
    public RulesEngineManager(AppDbContext context, IMapper mapper, IManagerWrapper managerWrapper)
    {
        this.context = context;
        this.mapper = mapper;
        this.managerWrapper = managerWrapper;
    }
    
    /// <summary>
    /// Calculate an output for an entity using the rules engine
    /// </summary>
    public async Task<RuleExecutionResultModel> CalculateOutputAsync(string entityType, int entityId, string outputCode, int currentUserId)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // 1. Get the output definition
        var ruleOutput = await context.RuleOutput
            .Include(ro => ro.RuleDefinitions.Where(rd => rd.IsActive && !rd.IsDeleted))
                .ThenInclude(rd => rd.Conditions.Where(c => c.IsActive && !c.IsDeleted))
            .Include(ro => ro.RuleDefinitions.Where(rd => rd.IsActive && !rd.IsDeleted))
                .ThenInclude(rd => rd.Actions.Where(a => a.IsActive && !a.IsDeleted))
            .FirstOrDefaultAsync(ro => ro.OutputCode == outputCode && ro.EntityType == entityType && ro.IsActive && !ro.IsDeleted);
            
        if (ruleOutput == null)
        {
            throw new BusinessException($"Rule output '{outputCode}' not found for entity type '{entityType}'");
        }
        
        // 2. Check cache
        var cachedResult = await GetCachedResultAsync(entityType, entityId, outputCode);
        if (cachedResult != null && cachedResult.CacheExpiryTime > DateTime.UtcNow)
        {
            return cachedResult;
        }
        
        // 3. Create execution result object
        var executionResult = new RuleExecutionResult
        {
            EntityType = entityType,
            EntityId = entityId,
            RuleOutputId = ruleOutput.Id,
            OutputCode = outputCode,
            ExecutionStartTime = DateTime.UtcNow,
            CreatedBy = currentUserId,
            IsCached = true,
            CacheExpiryTime = DateTime.UtcNow.AddMinutes(ruleOutput.CacheMinutes)
        };
        
        // 4. Execute rules in order
        decimal totalScore = ruleOutput.DefaultValue ?? 0;
        decimal totalWeight = 0;
        var executionLogs = new List<RuleExecutionLog>();
        
        var orderedRules = ruleOutput.RuleDefinitions
            .Where(rd => rd.IsActive && !rd.IsDeleted)
            .OrderBy(rd => rd.ExecutionOrder)
            .ToList();
            
        foreach (var rule in orderedRules)
        {
            var ruleStopwatch = Stopwatch.StartNew();
            
            var (matched, score, details) = await EvaluateRuleAsync(rule, entityType, entityId);
            
            ruleStopwatch.Stop();
            
            // Create execution log
            var log = new RuleExecutionLog
            {
                RuleDefinitionId = rule.Id,
                RuleCode = rule.RuleCode,
                RuleName = rule.RuleName,
                RuleType = rule.RuleType,
                Matched = matched,
                Score = score,
                Weight = rule.Weight,
                WeightedScore = matched ? score * rule.Weight : 0,
                ConditionsEvaluated = rule.Conditions.Count,
                ConditionsMatched = matched ? rule.Conditions.Count : 0,
                EvaluationDetails = JsonConvert.SerializeObject(details),
                EvaluationDurationMs = (int)ruleStopwatch.ElapsedMilliseconds,
                OutputMessage = matched ? rule.Actions.FirstOrDefault()?.OutputMessage : null,
                OutputSeverity = matched ? rule.Actions.FirstOrDefault()?.OutputSeverity : null
            };
            
            executionLogs.Add(log);
            
            if (matched)
            {
                totalScore += score * rule.Weight;
                totalWeight += rule.Weight;
            }
        }
        
        stopwatch.Stop();
        
        // 5. Calculate final values
        executionResult.CalculatedValue = totalScore;
        executionResult.RiskLevel = DetermineRiskLevel(totalScore, ruleOutput);
        executionResult.ConfidenceScore = totalWeight > 0 ? totalWeight / orderedRules.Count : 0.5m;
        executionResult.ExecutionEndTime = DateTime.UtcNow;
        executionResult.ExecutionDurationMs = (int)stopwatch.ElapsedMilliseconds;
        executionResult.RulesEvaluatedCount = orderedRules.Count;
        executionResult.RulesMatchedCount = executionLogs.Count(log => log.Matched);
        executionResult.ExecutionLogs = executionLogs;
        
        // 6. Save result
        context.RuleExecutionResult.Add(executionResult);
        await context.SaveChangesAsync();
        
        return mapper.Map<RuleExecutionResultModel>(executionResult);
    }
    
    /// <summary>
    /// Evaluate a single rule
    /// </summary>
    private async Task<(bool matched, decimal score, Dictionary<string, object> details)> EvaluateRuleAsync(
        RuleDefinition rule, string entityType, int entityId)
    {
        var details = new Dictionary<string, object>();
        bool allConditionsMet = true;
        
        foreach (var condition in rule.Conditions.Where(c => c.IsActive && !c.IsDeleted))
        {
            bool conditionMet = await EvaluateConditionAsync(condition, rule, entityType, entityId, details);
            
            if (condition.LogicalOperator == "AND" && !conditionMet)
            {
                allConditionsMet = false;
                break;
            }
            else if (condition.LogicalOperator == "OR" && conditionMet)
            {
                allConditionsMet = true;
                break;
            }
        }
        
        if (!allConditionsMet)
        {
            return (false, 0, details);
        }
        
        // Calculate score from actions
        decimal score = 0;
        foreach (var action in rule.Actions.Where(a => a.IsActive && !a.IsDeleted))
        {
            if (action.ActionType == "AssignScore" && action.ScoreValue.HasValue)
            {
                score += action.ScoreValue.Value * action.ScoreMultiplier;
            }
            else if (action.ActionType == "CallAI")
            {
                // Call AI service (implementation depends on your AI service)
                var aiScore = await CallAIServiceAsync(action, entityType, entityId);
                score += aiScore * action.ScoreMultiplier;
            }
        }
        
        return (true, score, details);
    }
    
    /// <summary>
    /// Evaluate a single condition
    /// </summary>
    private async Task<bool> EvaluateConditionAsync(
        RuleCondition condition, 
        RuleDefinition rule, 
        string entityType, 
        int entityId, 
        Dictionary<string, object> details)
    {
        switch (condition.ConditionType)
        {
            case "ArtifactValue":
                return await EvaluateArtifactConditionAsync(condition, rule, entityType, entityId, details);
                
            case "ArtifactExists":
                return await EvaluateArtifactExistsAsync(condition, rule, entityType, entityId, details);
                
            case "Count":
                return await EvaluateCountConditionAsync(condition, rule, entityType, entityId, details);
                
            case "Calculation":
                return await EvaluateCalculationConditionAsync(condition, rule, entityType, entityId, details);
                
            case "AI":
                return await EvaluateAIConditionAsync(condition, rule, entityType, entityId, details);
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Evaluate artifact-based condition
    /// </summary>
    private async Task<bool> EvaluateArtifactConditionAsync(
        RuleCondition condition, 
        RuleDefinition rule, 
        string entityType, 
        int entityId, 
        Dictionary<string, object> details)
    {
        // Get related entity if specified
        string targetEntityType = rule.RelatedEntityType ?? entityType;
        int targetEntityId = entityId;
        
        if (!string.IsNullOrEmpty(rule.RelationshipPath))
        {
            // Navigate relationship path (e.g., "OpportunityCountry->Country")
            var relatedIds = await GetRelatedEntityIdsAsync(entityType, entityId, rule.RelationshipPath);
            
            // Check artifacts for all related entities
            foreach (var relatedId in relatedIds)
            {
                var artifact = await context.EntityArtifact
                    .Include(ea => ea.ArtifactType)
                    .FirstOrDefaultAsync(ea => 
                        ea.EntityType == targetEntityType &&
                        ea.EntityId == relatedId &&
                        ea.ArtifactType!.ArtifactTypeCode == condition.ArtifactTypeCode &&
                        !ea.IsDeleted);
                
                if (artifact != null)
                {
                    bool matches = CompareArtifactValue(artifact, condition);
                    if (matches)
                    {
                        details[$"Artifact_{condition.ArtifactTypeCode}"] = artifact.ValueText ?? artifact.ValueNumber?.ToString() ?? "true";
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        // Direct artifact check
        var directArtifact = await context.EntityArtifact
            .Include(ea => ea.ArtifactType)
            .FirstOrDefaultAsync(ea => 
                ea.EntityType == targetEntityType &&
                ea.EntityId == targetEntityId &&
                ea.ArtifactType!.ArtifactTypeCode == condition.ArtifactTypeCode &&
                !ea.IsDeleted);
        
        if (directArtifact == null)
        {
            return condition.Operator == "NotExists";
        }
        
        bool result = CompareArtifactValue(directArtifact, condition);
        if (result)
        {
            details[$"Artifact_{condition.ArtifactTypeCode}"] = directArtifact.ValueText ?? directArtifact.ValueNumber?.ToString() ?? "true";
        }
        
        return result;
    }
    
    /// <summary>
    /// Compare artifact value against condition
    /// </summary>
    private bool CompareArtifactValue(EntityArtifact artifact, RuleCondition condition)
    {
        string? artifactValue = artifact.ValueText?.Trim()?.ToUpper();
        string? expectedValue = condition.ComparisonValue?.Trim()?.ToUpper();
        
        switch (condition.Operator)
        {
            case "Equals":
                return artifactValue == expectedValue ||
                       (artifact.ValueNumber.HasValue && condition.ComparisonValueNumeric.HasValue && 
                        artifact.ValueNumber.Value == condition.ComparisonValueNumeric.Value);
                        
            case "NotEquals":
                return artifactValue != expectedValue ||
                       (artifact.ValueNumber.HasValue && condition.ComparisonValueNumeric.HasValue && 
                        artifact.ValueNumber.Value != condition.ComparisonValueNumeric.Value);
                        
            case "GreaterThan":
                if (artifact.ValueNumber.HasValue && condition.ComparisonValueNumeric.HasValue)
                {
                    return artifact.ValueNumber.Value > condition.ComparisonValueNumeric.Value;
                }
                return false;
                
            case "LessThan":
                if (artifact.ValueNumber.HasValue && condition.ComparisonValueNumeric.HasValue)
                {
                    return artifact.ValueNumber.Value < condition.ComparisonValueNumeric.Value;
                }
                return false;
                
            case "Contains":
                return artifactValue?.Contains(expectedValue ?? "") ?? false;
                
            case "Exists":
                return true; // Artifact exists
                
            case "NotExists":
                return false; // Artifact exists but we want it not to exist
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Evaluate count-based condition
    /// </summary>
    private async Task<bool> EvaluateCountConditionAsync(
        RuleCondition condition, 
        RuleDefinition rule, 
        string entityType, 
        int entityId, 
        Dictionary<string, object> details)
    {
        if (entityType == "Opportunity" && rule.RuleCode == "OUTPUT_LEVEL_DIVERSITY")
        {
            // Count distinct output groups/levels for this opportunity
            var distinctLevels = await context.Set<OpportunityDeliverable>()
                .Include(od => od.Output)
                .Where(od => od.OpportunityId == entityId && od.Output != null)
                .Select(od => od.Output!.OutputGroup)
                .Distinct()
                .CountAsync();
            
            details["DistinctOutputLevels"] = distinctLevels;
            
            if (condition.MinCount.HasValue && distinctLevels < condition.MinCount.Value)
            {
                return false;
            }
            
            if (condition.MaxCount.HasValue && distinctLevels > condition.MaxCount.Value)
            {
                return condition.Operator == "GreaterThan";
            }
            
            return condition.Operator == "GreaterThan" ? distinctLevels > (condition.MinCount ?? 0) : true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Evaluate calculation-based condition (placeholder - implement based on specific calculations)
    /// </summary>
    private async Task<bool> EvaluateCalculationConditionAsync(
        RuleCondition condition, 
        RuleDefinition rule, 
        string entityType, 
        int entityId, 
        Dictionary<string, object> details)
    {
        // Implement specific calculations based on rule code
        await Task.CompletedTask;
        return false;
    }
    
    /// <summary>
    /// Evaluate AI-based condition (placeholder - implement based on your AI service)
    /// </summary>
    private async Task<bool> EvaluateAIConditionAsync(
        RuleCondition condition, 
        RuleDefinition rule, 
        string entityType, 
        int entityId, 
        Dictionary<string, object> details)
    {
        await Task.CompletedTask;
        return false;
    }
    
    /// <summary>
    /// Check if artifact exists
    /// </summary>
    private async Task<bool> EvaluateArtifactExistsAsync(
        RuleCondition condition, 
        RuleDefinition rule, 
        string entityType, 
        int entityId, 
        Dictionary<string, object> details)
    {
        string targetEntityType = rule.RelatedEntityType ?? entityType;
        
        if (!string.IsNullOrEmpty(rule.RelationshipPath))
        {
            var relatedIds = await GetRelatedEntityIdsAsync(entityType, entityId, rule.RelationshipPath);
            
            foreach (var relatedId in relatedIds)
            {
                var exists = await context.EntityArtifact
                    .Include(ea => ea.ArtifactType)
                    .AnyAsync(ea => 
                        ea.EntityType == targetEntityType &&
                        ea.EntityId == relatedId &&
                        ea.ArtifactType!.ArtifactTypeCode == condition.ArtifactTypeCode &&
                        !ea.IsDeleted);
                
                if (exists)
                {
                    details[$"Artifact_{condition.ArtifactTypeCode}_Exists"] = true;
                    return condition.Operator != "NotExists";
                }
            }
            
            return condition.Operator == "NotExists";
        }
        
        var artifactExists = await context.EntityArtifact
            .Include(ea => ea.ArtifactType)
            .AnyAsync(ea => 
                ea.EntityType == targetEntityType &&
                ea.EntityId == entityId &&
                ea.ArtifactType!.ArtifactTypeCode == condition.ArtifactTypeCode &&
                !ea.IsDeleted);
        
        details[$"Artifact_{condition.ArtifactTypeCode}_Exists"] = artifactExists;
        
        return condition.Operator == "NotExists" ? !artifactExists : artifactExists;
    }
    
    /// <summary>
    /// Get related entity IDs by following relationship path
    /// </summary>
    private async Task<List<int>> GetRelatedEntityIdsAsync(string entityType, int entityId, string relationshipPath)
    {
        // Parse relationship path (e.g., "OpportunityCountry->Country")
        var parts = relationshipPath.Split("->");
        
        if (entityType == "Opportunity" && parts[0] == "OpportunityCountry" && parts[1] == "Country")
        {
            return await context.Set<OpportunityCountry>()
                .Where(oc => oc.OpportunityId == entityId)
                .Select(oc => oc.CountryId)
                .ToListAsync();
        }
        else if (entityType == "Opportunity" && parts[0] == "OpportunityFundingPartner" && parts[1] == "Partner")
        {
            return await context.Set<OpportunityFundingPartner>()
                .Where(ofp => ofp.OpportunityId == entityId)
                .Select(ofp => ofp.PartnerId)
                .ToListAsync();
        }
        
        return new List<int>();
    }
    
    /// <summary>
    /// Call AI service for advanced analysis (placeholder)
    /// </summary>
    private async Task<decimal> CallAIServiceAsync(RuleAction action, string entityType, int entityId)
    {
        await Task.CompletedTask;
        return 0;
    }
    
    /// <summary>
    /// Determine risk level based on score and thresholds
    /// </summary>
    private string DetermineRiskLevel(decimal score, RuleOutput ruleOutput)
    {
        if (ruleOutput.LowThreshold.HasValue && score <= ruleOutput.LowThreshold.Value)
        {
            return "Low";
        }
        else if (ruleOutput.MediumThreshold.HasValue && score <= ruleOutput.MediumThreshold.Value)
        {
            return "Medium";
        }
        else if (ruleOutput.HighThreshold.HasValue && score > ruleOutput.HighThreshold.Value)
        {
            return "High";
        }
        
        return "Unknown";
    }
    
    /// <summary>
    /// Get cached result if available and not expired
    /// </summary>
    public async Task<RuleExecutionResultModel?> GetCachedResultAsync(string entityType, int entityId, string outputCode)
    {
        var cachedResult = await context.RuleExecutionResult
            .Include(rer => rer.ExecutionLogs)
            .Where(rer => 
                rer.EntityType == entityType &&
                rer.EntityId == entityId &&
                rer.OutputCode == outputCode &&
                rer.IsCached &&
                rer.CacheExpiryTime > DateTime.UtcNow)
            .OrderByDescending(rer => rer.CreatedDate)
            .FirstOrDefaultAsync();
        
        return cachedResult == null ? null : mapper.Map<RuleExecutionResultModel>(cachedResult);
    }
    
    /// <summary>
    /// Invalidate cache for an entity's output
    /// </summary>
    public async Task InvalidateCacheAsync(string entityType, int entityId, string outputCode)
    {
        var cachedResults = await context.RuleExecutionResult
            .Where(rer => 
                rer.EntityType == entityType &&
                rer.EntityId == entityId &&
                rer.OutputCode == outputCode &&
                rer.IsCached)
            .ToListAsync();
        
        foreach (var result in cachedResults)
        {
            result.IsCached = false;
            result.CacheExpiryTime = DateTime.UtcNow;
        }
        
        await context.SaveChangesAsync();
    }
}
```

### Controller Implementation

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Presentation.Controllers;

[Route("api/rules-engine")]
[ApiController]
[Authorize]
public class RulesEngineController : ControllerBase
{
    private readonly IManagerWrapper manager;
    private readonly IMapper mapper;
    private readonly UserResolverService<int> userResolverService;
    
    private int CurrentUserId => userResolverService.GetCurrentUserId();
    
    public RulesEngineController(
        IManagerWrapper manager,
        IMapper mapper,
        UserResolverService<int> userResolverService)
    {
        this.manager = manager;
        this.mapper = mapper;
        this.userResolverService = userResolverService;
    }
    
    /// <summary>
    /// Calculate output for an entity
    /// POST api/rules-engine/calculate
    /// </summary>
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateOutput([FromBody] CalculateOutputRequest request)
    {
        var result = await manager.RulesEngineManager.CalculateOutputAsync(
            request.EntityType,
            request.EntityId,
            request.OutputCode,
            CurrentUserId
        );
        
        return Ok(result);
    }
    
    /// <summary>
    /// Get cached result for an entity's output
    /// GET api/rules-engine/result/{entityType}/{entityId}/{outputCode}
    /// </summary>
    [HttpGet("result/{entityType}/{entityId}/{outputCode}")]
    public async Task<IActionResult> GetResult(string entityType, int entityId, string outputCode)
    {
        var result = await manager.RulesEngineManager.GetCachedResultAsync(entityType, entityId, outputCode);
        
        if (result == null)
        {
            return NotFound();
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Invalidate cache for an entity's output
    /// POST api/rules-engine/invalidate-cache
    /// </summary>
    [HttpPost("invalidate-cache")]
    public async Task<IActionResult> InvalidateCache([FromBody] InvalidateCacheRequest request)
    {
        await manager.RulesEngineManager.InvalidateCacheAsync(
            request.EntityType,
            request.EntityId,
            request.OutputCode
        );
        
        return Ok();
    }
    
    /// <summary>
    /// Recalculate and refresh output
    /// POST api/rules-engine/refresh
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshOutput([FromBody] CalculateOutputRequest request)
    {
        // Invalidate cache first
        await manager.RulesEngineManager.InvalidateCacheAsync(
            request.EntityType,
            request.EntityId,
            request.OutputCode
        );
        
        // Recalculate
        var result = await manager.RulesEngineManager.CalculateOutputAsync(
            request.EntityType,
            request.EntityId,
            request.OutputCode,
            CurrentUserId
        );
        
        return Ok(result);
    }
}
```

### Models

```csharp
namespace UNOPS.PAO.Models;

public class CalculateOutputRequest
{
    public required string EntityType { get; set; } // e.g., "Opportunity"
    public int EntityId { get; set; } // e.g., OpportunityId = 123
    public required string OutputCode { get; set; } // e.g., "OPPORTUNITY_RISK"
}

public class InvalidateCacheRequest
{
    public required string EntityType { get; set; }
    public int EntityId { get; set; }
    public required string OutputCode { get; set; }
}

public class RuleExecutionResultModel
{
    public int Id { get; set; }
    public required string EntityType { get; set; }
    public int EntityId { get; set; }
    public required string OutputCode { get; set; }
    public decimal? CalculatedValue { get; set; }
    public string? CalculatedValueText { get; set; }
    public string? RiskLevel { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public DateTime ExecutionStartTime { get; set; }
    public DateTime ExecutionEndTime { get; set; }
    public int ExecutionDurationMs { get; set; }
    public int RulesEvaluatedCount { get; set; }
    public int RulesMatchedCount { get; set; }
    public List<RuleExecutionLogModel> ExecutionLogs { get; set; } = new();
}

public class RuleExecutionLogModel
{
    public int Id { get; set; }
    public required string RuleCode { get; set; }
    public required string RuleName { get; set; }
    public required string RuleType { get; set; }
    public bool Matched { get; set; }
    public decimal? Score { get; set; }
    public decimal? Weight { get; set; }
    public decimal? WeightedScore { get; set; }
    public string? OutputMessage { get; set; }
    public string? OutputSeverity { get; set; }
}
```

---

## Usage Examples

### Example 1: Calculate Opportunity Risk on Page Load

```typescript
// Angular component
export class OpportunityDetailComponent implements OnInit {
  opportunityId = signal<number>(0);
  opportunityRisk = signal<RuleExecutionResult | null>(null);
  isCalculatingRisk = signal<boolean>(false);
  
  private readonly rulesEngineService = inject(RulesEngineService);
  
  async ngOnInit() {
    const id = this.route.snapshot.params['id'];
    this.opportunityId.set(id);
    
    // Calculate risk on page load
    await this.calculateOpportunityRisk();
  }
  
  async calculateOpportunityRisk() {
    this.isCalculatingRisk.set(true);
    
    try {
      const result = await this.rulesEngineService.calculateOutput({
        entityType: 'Opportunity',
        entityId: this.opportunityId(),
        outputCode: 'OPPORTUNITY_RISK'
      });
      
      this.opportunityRisk.set(result);
    } finally {
      this.isCalculatingRisk.set(false);
    }
  }
  
  async refreshRisk() {
    this.isCalculatingRisk.set(true);
    
    try {
      const result = await this.rulesEngineService.refreshOutput({
        entityType: 'Opportunity',
        entityId: this.opportunityId(),
        outputCode: 'OPPORTUNITY_RISK'
      });
      
      this.opportunityRisk.set(result);
      
      this.feedbackService.showSuccessToast({
        summary: 'Success',
        detail: 'Risk assessment refreshed successfully'
      });
    } finally {
      this.isCalculatingRisk.set(false);
    }
  }
}
```

```html
<!-- Angular template -->
<div class="opportunity-risk-panel">
  <div class="flex items-center justify-between mb-4">
    <h3 class="text-lg font-semibold">Opportunity Risk Assessment</h3>
    <p-button
      [label]="'button.refresh' | translate"
      icon="pi pi-refresh"
      severity="secondary"
      size="small"
      [loading]="isCalculatingRisk()"
      (onClick)="refreshRisk()"
    />
  </div>
  
  @if (isCalculatingRisk()) {
    <p-progressBar mode="indeterminate" styleClass="mb-4" />
  }
  
  @if (opportunityRisk(); as risk) {
    <div class="risk-summary">
      <div class="risk-level" [ngClass]="'risk-level--' + risk.riskLevel.toLowerCase()">
        <span class="risk-label">Risk Level:</span>
        <span class="risk-value">{{ risk.riskLevel }}</span>
      </div>
      
      <div class="risk-score">
        <span class="score-label">Risk Score:</span>
        <span class="score-value">{{ risk.calculatedValue | number:'1.0-1' }} / 100</span>
      </div>
      
      <div class="confidence-score">
        <span class="confidence-label">Confidence:</span>
        <span class="confidence-value">{{ risk.confidenceScore * 100 | number:'1.0-0' }}%</span>
      </div>
    </div>
    
    <!-- Contributing factors -->
    <div class="contributing-factors mt-4">
      <h4 class="text-md font-medium mb-2">Contributing Factors</h4>
      
      @for (log of risk.executionLogs; track log.id) {
        @if (log.matched) {
          <div class="factor-item" [ngClass]="'severity--' + log.outputSeverity?.toLowerCase()">
            <div class="factor-header">
              <span class="factor-name">{{ log.ruleName }}</span>
              <span class="factor-score">{{ log.weightedScore > 0 ? '+' : '' }}{{ log.weightedScore | number:'1.0-1' }}</span>
            </div>
            @if (log.outputMessage) {
              <div class="factor-message">{{ log.outputMessage }}</div>
            }
          </div>
        }
      }
    </div>
  }
</div>
```

### Angular Service

```typescript
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RulesEngineService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/rules-engine';
  
  async calculateOutput(request: CalculateOutputRequest): Promise<RuleExecutionResult> {
    return firstValueFrom(
      this.http.post<RuleExecutionResult>(`${this.apiUrl}/calculate`, request)
    );
  }
  
  async getResult(entityType: string, entityId: number, outputCode: string): Promise<RuleExecutionResult> {
    return firstValueFrom(
      this.http.get<RuleExecutionResult>(`${this.apiUrl}/result/${entityType}/${entityId}/${outputCode}`)
    );
  }
  
  async refreshOutput(request: CalculateOutputRequest): Promise<RuleExecutionResult> {
    return firstValueFrom(
      this.http.post<RuleExecutionResult>(`${this.apiUrl}/refresh`, request)
    );
  }
  
  async invalidateCache(request: InvalidateCacheRequest): Promise<void> {
    return firstValueFrom(
      this.http.post<void>(`${this.apiUrl}/invalidate-cache`, request)
    );
  }
}

export interface CalculateOutputRequest {
  entityType: string;
  entityId: number;
  outputCode: string;
}

export interface InvalidateCacheRequest {
  entityType: string;
  entityId: number;
  outputCode: string;
}

export interface RuleExecutionResult {
  id: number;
  entityType: string;
  entityId: number;
  outputCode: string;
  calculatedValue?: number;
  calculatedValueText?: string;
  riskLevel?: string;
  confidenceScore?: number;
  executionStartTime: string;
  executionEndTime: string;
  executionDurationMs: number;
  rulesEvaluatedCount: number;
  rulesMatchedCount: number;
  executionLogs: RuleExecutionLog[];
}

export interface RuleExecutionLog {
  id: number;
  ruleCode: string;
  ruleName: string;
  ruleType: string;
  matched: boolean;
  score?: number;
  weight?: number;
  weightedScore?: number;
  outputMessage?: string;
  outputSeverity?: string;
}
```

---

## Integration Guide

### 1. Database Migration

Create a migration to add the rules engine tables:

```bash
dotnet ef migrations add AddRulesEngineTables --project UNOPS.PAO.DataAccess
dotnet ef database update --project UNOPS.PAO.DataAccess
```

### 2. DbContext Configuration

Add rules engine entities to your `AppDbContext`:

```csharp
public class AppDbContext : AuditableDbContext<int, int>
{
    // ... existing DbSets ...
    
    public DbSet<RuleOutput> RuleOutput => Set<RuleOutput>();
    public DbSet<RuleDefinition> RuleDefinition => Set<RuleDefinition>();
    public DbSet<RuleCondition> RuleCondition => Set<RuleCondition>();
    public DbSet<RuleAction> RuleAction => Set<RuleAction>();
    public DbSet<RuleExecutionResult> RuleExecutionResult => Set<RuleExecutionResult>();
    public DbSet<RuleExecutionLog> RuleExecutionLog => Set<RuleExecutionLog>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure rules engine entities
        modelBuilder.Entity<RuleOutput>(entity =>
        {
            entity.HasIndex(e => e.OutputCode).IsUnique();
            entity.HasIndex(e => e.EntityType);
        });
        
        modelBuilder.Entity<RuleDefinition>(entity =>
        {
            entity.HasOne(rd => rd.RuleOutput)
                .WithMany(ro => ro.RuleDefinitions)
                .HasForeignKey(rd => rd.RuleOutputId);
                
            entity.HasIndex(e => e.RuleCode);
            entity.HasIndex(e => e.ExecutionOrder);
        });
        
        modelBuilder.Entity<RuleCondition>(entity =>
        {
            entity.HasOne(rc => rc.RuleDefinition)
                .WithMany(rd => rd.Conditions)
                .HasForeignKey(rc => rc.RuleDefinitionId);
        });
        
        modelBuilder.Entity<RuleAction>(entity =>
        {
            entity.HasOne(ra => ra.RuleDefinition)
                .WithMany(rd => rd.Actions)
                .HasForeignKey(ra => ra.RuleDefinitionId);
        });
        
        modelBuilder.Entity<RuleExecutionResult>(entity =>
        {
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.HasIndex(e => e.OutputCode);
            entity.HasIndex(e => e.CacheExpiryTime).HasFilter("IsCached = TRUE");
        });
        
        modelBuilder.Entity<RuleExecutionLog>(entity =>
        {
            entity.HasOne(rel => rel.RuleExecutionResult)
                .WithMany(rer => rer.ExecutionLogs)
                .HasForeignKey(rel => rel.RuleExecutionResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

### 3. Register Manager

Add `RulesEngineManager` to `ManagerWrapper`:

```csharp
public interface IManagerWrapper
{
    // ... existing managers ...
    IRulesEngineManager RulesEngineManager { get; }
}

public class ManagerWrapper : IManagerWrapper
{
    // ... existing managers ...
    
    public IRulesEngineManager RulesEngineManager { get; }
    
    public ManagerWrapper(/* dependencies */)
    {
        // ... existing initializations ...
        
        RulesEngineManager = new RulesEngineManager(context, mapper, this);
    }
}
```

### 4. Seed Initial Rules

Create seeding methods and call during application startup.

---

## Conclusion

This generic rules engine design provides:

1. **Flexibility**: Database-driven rules allow configuration without code changes
2. **Scalability**: Can handle multiple output types and entity types
3. **Performance**: Caching mechanism reduces recalculation overhead
4. **Transparency**: Detailed execution logs show how scores are calculated
5. **Extensibility**: Easy to add new rule types and evaluation logic
6. **Maintainability**: Clear separation of concerns between rules engine and business logic

The system supports the example Opportunity Risk calculation and can easily be extended to calculate other outputs like Partner Risk Score, Country Risk Assessment, Project Complexity Score, etc.

---

**Next Steps:**

1. Implement the database migration
2. Add the entity classes to your domain layer
3. Implement the manager and controller
4. Seed initial rules for Opportunity Risk
5. Create the Angular service and UI components
6. Test with real opportunity data
7. Extend with additional rule types as needed

