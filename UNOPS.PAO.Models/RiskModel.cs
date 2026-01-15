namespace UNOPS.PAO.Models
{
    #region Risk Models

    /// <summary>
    /// Model representing a risk in the risk register (aligned with oUP)
    /// </summary>
    public class RiskModel
    {
        /// <summary>
        /// Risk ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The type of entity this risk is associated with (e.g., "Opportunity", "Project")
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the entity this risk is associated with
        /// </summary>
        public int EntityId { get; set; }

        #region Mandatory Fields (oUP aligned)

        /// <summary>
        /// Risk title (MANDATORY)
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Risk type ID (MANDATORY) - FK to RiskType
        /// </summary>
        public int RiskTypeId { get; set; }

        /// <summary>
        /// Risk type name (e.g., "Threat", "Opportunity")
        /// </summary>
        public string? RiskTypeName { get; set; }

        /// <summary>
        /// Risk type code (e.g., "THREAT", "OPPORTUNITY")
        /// </summary>
        public string? RiskTypeCode { get; set; }

        /// <summary>
        /// Risk category ID (MANDATORY) - FK to RiskCategory (Level 3)
        /// </summary>
        public int RiskCategoryId { get; set; }

        /// <summary>
        /// Risk category name
        /// </summary>
        public string? RiskCategoryName { get; set; }

        /// <summary>
        /// Full category path (e.g., "Finance > Contributions > Engagement costing and pricing")
        /// </summary>
        public string? RiskCategoryFullPath { get; set; }

        /// <summary>
        /// Risk probability ID (MANDATORY) - FK to RiskProbability
        /// </summary>
        public int RiskProbabilityId { get; set; }

        /// <summary>
        /// Probability name (e.g., "Low", "High")
        /// </summary>
        public string? RiskProbabilityName { get; set; }

        /// <summary>
        /// Risk proximity ID (MANDATORY) - FK to RiskProximity
        /// </summary>
        public int RiskProximityId { get; set; }

        /// <summary>
        /// Proximity name (e.g., "Within one month")
        /// </summary>
        public string? RiskProximityName { get; set; }

        /// <summary>
        /// Risk impact level ID (MANDATORY) - FK to RiskImpactLevel
        /// </summary>
        public int RiskImpactLevelId { get; set; }

        /// <summary>
        /// Impact level name (e.g., "Low", "High")
        /// </summary>
        public string? RiskImpactLevelName { get; set; }

        /// <summary>
        /// Risk response type ID (CONDITIONAL - mandatory for Opportunity type)
        /// </summary>
        public int? RiskResponseTypeId { get; set; }

        /// <summary>
        /// Response type name (e.g., "Accept", "Reduce")
        /// </summary>
        public string? RiskResponseTypeName { get; set; }

        #endregion

        #region Optional Fields

        /// <summary>
        /// Detailed description of the risk (OPTIONAL)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Recommendation for mitigating the risk (OPTIONAL)
        /// </summary>
        public string Recommendation { get; set; } = string.Empty;

        #endregion

        #region Legacy Fields (for backward compatibility)

        /// <summary>
        /// Impact level: 1=Low, 2=Medium, 3=High (LEGACY - use RiskImpactLevelId instead)
        /// </summary>
        public int Impact { get; set; }

        /// <summary>
        /// Current status of the risk
        /// </summary>
        public string Status { get; set; } = string.Empty;

        #endregion

        #region Audit Fields

        /// <summary>
        /// Date when the risk was identified
        /// </summary>
        public DateTime? IdentifiedDate { get; set; }

        /// <summary>
        /// Name of the person who identified the risk
        /// </summary>
        public string? IdentifiedBy { get; set; }

        /// <summary>
        /// Date when the risk was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Name of the person who created the risk record
        /// </summary>
        public string? CreatedBy { get; set; }

        #endregion

        #region PreDefined High Risk Reference

        /// <summary>
        /// PreDefined High Risk ID (if created from checklist)
        /// </summary>
        public int? PreDefinedHighRiskId { get; set; }

        /// <summary>
        /// PreDefined High Risk code (e.g., "1.1.1", "3.2.1")
        /// </summary>
        public string? PreDefinedHighRiskCode { get; set; }

        /// <summary>
        /// PreDefined High Risk short title
        /// </summary>
        public string? PreDefinedHighRiskTitle { get; set; }

        #endregion
    }

    /// <summary>
    /// Request model for creating a new risk
    /// For predefined high risks: All oUP fields are mandatory
    /// For manual entry: Only Title is mandatory, oUP fields will get defaults
    /// </summary>
    public class RiskCreateRequest
    {
        /// <summary>
        /// The ID of the entity this risk is associated with
        /// </summary>
        public int EntityId { get; set; }

        #region Always Mandatory Fields

        /// <summary>
        /// Risk title (ALWAYS MANDATORY - for both predefined and manual entry)
        /// </summary>
        public string Title { get; set; } = string.Empty;

        #endregion

        #region oUP Fields (Mandatory for predefined high risks, optional with defaults for manual entry)

        /// <summary>
        /// Risk type ID - FK to RiskType
        /// MANDATORY if PreDefinedHighRiskId is set (predefined mode)
        /// OPTIONAL for manual entry (will default to THREAT if not provided)
        /// </summary>
        public int? RiskTypeId { get; set; }

        /// <summary>
        /// Risk category ID - FK to RiskCategory (Level 3 leaf)
        /// MANDATORY if PreDefinedHighRiskId is set (predefined mode)
        /// OPTIONAL for manual entry (will get default category if not provided)
        /// </summary>
        public int? RiskCategoryId { get; set; }

        /// <summary>
        /// Risk probability ID - FK to RiskProbability
        /// MANDATORY if PreDefinedHighRiskId is set (predefined mode)
        /// OPTIONAL for manual entry (will default to MEDIUM if not provided)
        /// </summary>
        public int? RiskProbabilityId { get; set; }

        /// <summary>
        /// Risk proximity ID - FK to RiskProximity
        /// MANDATORY if PreDefinedHighRiskId is set (predefined mode)
        /// OPTIONAL for manual entry (will default to WITHIN_SIX_MONTHS if not provided)
        /// </summary>
        public int? RiskProximityId { get; set; }

        /// <summary>
        /// Risk impact level ID - FK to RiskImpactLevel
        /// MANDATORY if PreDefinedHighRiskId is set (predefined mode)
        /// OPTIONAL for manual entry (will default to MEDIUM if not provided)
        /// </summary>
        public int? RiskImpactLevelId { get; set; }

        /// <summary>
        /// Risk response type ID (CONDITIONAL)
        /// MANDATORY if PreDefinedHighRiskId is set AND RiskType = Opportunity
        /// OPTIONAL for manual entry
        /// </summary>
        public int? RiskResponseTypeId { get; set; }

        #endregion

        #region Optional Fields

        /// <summary>
        /// Detailed description of the risk (ALWAYS OPTIONAL)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Recommendation for mitigating the risk (ALWAYS OPTIONAL)
        /// </summary>
        public string? Recommendation { get; set; }

        /// <summary>
        /// PreDefined High Risk ID (when adding from organizational checklist)
        /// If set, all oUP fields become mandatory and cannot be changed
        /// If null, this is manual entry mode with simplified requirements
        /// </summary>
        public int? PreDefinedHighRiskId { get; set; }

        #endregion

        #region Legacy Fields (for backward compatibility)

        /// <summary>
        /// Impact level: 1=Low, 2=Medium, 3=High (LEGACY - use RiskImpactLevelId instead)
        /// </summary>
        public int Impact { get; set; } = 2;

        #endregion
    }

    /// <summary>
    /// Response model for DST risks endpoint
    /// </summary>
    public class DSTRisksResponse
    {
        /// <summary>
        /// List of risks
        /// </summary>
        public List<RiskModel> Risks { get; set; } = new();

        /// <summary>
        /// Total count of risks
        /// </summary>
        public int TotalCount { get; set; }
    }

    #endregion

    #region Risk Lookup Models

    /// <summary>
    /// Model for RiskType lookup
    /// </summary>
    public class RiskTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsResponseTypeMandatory { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Model for RiskProbability lookup
    /// </summary>
    public class RiskProbabilityModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? DisplayLabel { get; set; }
        public int NumericValue { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Model for RiskProximity lookup
    /// </summary>
    public class RiskProximityModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? MonthsValue { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Model for RiskImpactLevel lookup
    /// </summary>
    public class RiskImpactLevelModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? DisplayLabel { get; set; }
        public int NumericValue { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Model for RiskResponseType lookup
    /// </summary>
    public class RiskResponseTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool ValidForThreat { get; set; }
        public bool ValidForOpportunity { get; set; }
        public int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Response model for all risk lookups
    /// </summary>
    public class RiskLookupsResponse
    {
        public List<RiskTypeModel> RiskTypes { get; set; } = new();
        public List<RiskProbabilityModel> Probabilities { get; set; } = new();
        public List<RiskProximityModel> Proximities { get; set; } = new();
        public List<RiskImpactLevelModel> ImpactLevels { get; set; } = new();
        public List<RiskResponseTypeModel> ResponseTypes { get; set; } = new();
    }

    #endregion

    #region Risk Category Models

    /// <summary>
    /// Model for RiskCategory (3-level hierarchy)
    /// </summary>
    public class RiskCategoryModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSelectable { get; set; }
        public List<RiskCategoryModel> Children { get; set; } = new();
    }

    /// <summary>
    /// Response model for Risk Category hierarchy
    /// </summary>
    public class RiskCategoryHierarchyResponse
    {
        /// <summary>
        /// Hierarchical list of categories (Level 1 with nested children)
        /// </summary>
        public List<RiskCategoryModel> Categories { get; set; } = new();

        /// <summary>
        /// Flat list of all Level 3 (leaf) categories for dropdown selection
        /// </summary>
        public List<RiskCategoryModel> SelectableCategories { get; set; } = new();

        public int TotalLevel1 { get; set; }
        public int TotalLevel2 { get; set; }
        public int TotalLevel3 { get; set; }
    }

    #endregion

    #region PreDefined High Risk Models

    /// <summary>
    /// Model for PreDefinedHighRisk item
    /// </summary>
    public class PreDefinedHighRiskModel
    {
        public int Id { get; set; }
        
        /// <summary>
        /// oUP Question ID for mapping to legacy system
        /// </summary>
        public int? OupQuestionId { get; set; }
        
        public string Code { get; set; } = string.Empty;
        public string DisplayCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ShortTitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public int Level1 { get; set; }
        public string Level2Code { get; set; } = string.Empty;
        public bool IsAutoDetectable { get; set; }
        public string? DetectionRuleType { get; set; }
        public int DisplayOrder { get; set; }
        public int? RiskCategoryId { get; set; }
        public string? RiskCategoryName { get; set; }
    }

    /// <summary>
    /// AI-detected high risk recommendation with confidence level
    /// </summary>
    public class HighRiskRecommendation
    {
        /// <summary>
        /// The PreDefined High Risk that was detected
        /// </summary>
        public PreDefinedHighRiskModel PreDefinedHighRisk { get; set; } = null!;

        /// <summary>
        /// Confidence level (0-100) indicating how strongly this risk applies
        /// </summary>
        public int ConfidenceLevel { get; set; }

        /// <summary>
        /// Explanation of why this risk was detected
        /// </summary>
        public string DetectionReason { get; set; } = string.Empty;

        /// <summary>
        /// Specific data that triggered this detection
        /// </summary>
        public string TriggerData { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is a strong recommendation (confidence >= 80)
        /// </summary>
        public bool IsStronglyRecommended => ConfidenceLevel >= 80;
    }

    /// <summary>
    /// Response model for High Risk Analysis with AI recommendations
    /// </summary>
    public class HighRiskAnalysisResponse
    {
        /// <summary>
        /// All available PreDefined High Risks
        /// </summary>
        public List<PreDefinedHighRiskModel> AvailableHighRisks { get; set; } = new();

        /// <summary>
        /// AI-detected high risk recommendations based on opportunity data
        /// </summary>
        public List<HighRiskRecommendation> Recommendations { get; set; } = new();

        /// <summary>
        /// IDs of PreDefined High Risks already added to the risk register
        /// </summary>
        public List<int> AlreadyAddedHighRiskIds { get; set; } = new();

        /// <summary>
        /// Total count of available PreDefined High Risks
        /// </summary>
        public int TotalHighRisks { get; set; }

        /// <summary>
        /// Count of strongly recommended items (confidence >= 80)
        /// </summary>
        public int StronglyRecommendedCount { get; set; }
    }

    #endregion
}

