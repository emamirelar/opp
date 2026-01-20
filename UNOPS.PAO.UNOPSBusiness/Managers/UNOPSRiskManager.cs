using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSBusiness.Managers
{
    /// <summary>
    /// UNOPS-specific implementation of Risk management operations (aligned with oUP)
    /// </summary>
    public class UNOPSRiskManager : BaseUNOPSManager, IRiskManager
    {
        private readonly IMapper _mapper;
        private readonly UNOPSAppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly BaseRepository<Risk> _riskRepository;

        public UNOPSRiskManager(
            IMapper mapper,
            UNOPSAppDbContext context,
            IConfiguration configuration,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor = null,
            IServiceProvider serviceProvider = null)
            : base(mapper, context, configuration, null, "Risk", permissionService, httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _configuration = configuration;
            _riskRepository = new BaseRepository<Risk>(context, configuration, serviceProvider);
        }

        #region Risk CRUD Operations

        /// <summary>
        /// Gets all risks for a specific entity with full lookup data
        /// OPTIMIZED: Uses AsNoTracking for read-only query (Priority 2)
        /// </summary>
        public async Task<DSTRisksResponse> GetRisksByEntityAsync(string entityType, int entityId, ClaimsPrincipal? user = null)
        {
            var risks = await _context.Risks
                .AsNoTracking() // ✅ Read-only query optimization
                .Include(r => r.RiskTypeEntity)
                .Include(r => r.RiskCategory)
                .Include(r => r.RiskProbabilityEntity)
                .Include(r => r.RiskProximityEntity)
                .Include(r => r.RiskImpactLevelEntity)
                .Include(r => r.RiskResponseTypeEntity)
                .Include(r => r.PreDefinedHighRisk)
                .Where(r => r.EntityType == entityType && r.EntityId == entityId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            var riskModels = risks.Select(MapRiskToModel).ToList();

            return new DSTRisksResponse
            {
                Risks = riskModels,
                TotalCount = riskModels.Count
            };
        }

        /// <summary>
        /// Creates a new risk with oUP-aligned fields
        /// Mode A (Predefined High Risk): All oUP fields mandatory
        /// Mode B (Manual Entry): Only Title mandatory, oUP fields get defaults
        /// </summary>
        public async Task<RiskModel> CreateRiskAsync(RiskCreateRequest request, ClaimsPrincipal? user = null)
        {
            // Title is ALWAYS required
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Risk title is required");
            }

            // Determine mode: Predefined High Risk OR Manual Entry
            bool isPredefinedHighRisk = request.PreDefinedHighRiskId.HasValue;

            if (isPredefinedHighRisk)
            {
                // MODE A: Predefined High Risk - All oUP fields are mandatory
                if (!request.RiskTypeId.HasValue)
                    throw new ArgumentException("Risk type is required for predefined high risks");
                if (!request.RiskCategoryId.HasValue)
                    throw new ArgumentException("Risk category is required for predefined high risks");
                if (!request.RiskProbabilityId.HasValue)
                    throw new ArgumentException("Risk probability is required for predefined high risks");
                if (!request.RiskProximityId.HasValue)
                    throw new ArgumentException("Risk proximity is required for predefined high risks");
                if (!request.RiskImpactLevelId.HasValue)
                    throw new ArgumentException("Risk impact level is required for predefined high risks");

                // Validate all FK fields exist
                await ValidateRiskForeignKeysAsync(request);

                // Validate ResponseType for Opportunity type risks
                var riskType = await _context.RiskTypes.FindAsync(request.RiskTypeId.Value);
                if (riskType?.IsResponseTypeMandatory == true && !request.RiskResponseTypeId.HasValue)
                {
                    throw new ArgumentException("ResponseType is mandatory for Opportunity risk type");
                }

                // Validate ResponseType compatibility with RiskType
                if (request.RiskResponseTypeId.HasValue)
                {
                    var responseType = await _context.RiskResponseTypes.FindAsync(request.RiskResponseTypeId.Value);
                    if (responseType != null)
                    {
                        var isValidForType = riskType?.Code == "THREAT" ? responseType.ValidForThreat : responseType.ValidForOpportunity;
                        if (!isValidForType)
                        {
                            throw new ArgumentException($"ResponseType '{responseType.Name}' is not valid for RiskType '{riskType?.Name}'");
                        }
                    }
                }
            }
            else
            {
                // MODE B: Manual Entry - Apply defaults for missing oUP fields
                // Users can quickly add risks with just a title; fields will be populated with sensible defaults

                // Default to THREAT type if not provided
                if (!request.RiskTypeId.HasValue)
                {
                    var defaultType = await _context.RiskTypes
                        .Where(rt => rt.Code == "THREAT" && !rt.IsDeleted && rt.Status == EntityStatus.Active)
                        .FirstOrDefaultAsync();
                    request.RiskTypeId = defaultType?.Id ?? throw new InvalidOperationException("Default THREAT risk type not found in system");
                }

                // Default to LOW_TO_MEDIUM probability if not provided
                if (!request.RiskProbabilityId.HasValue)
                {
                    var defaultProbability = await _context.RiskProbabilities
                        .Where(rp => rp.Code == "LOW_TO_MEDIUM" && !rp.IsDeleted && rp.Status == EntityStatus.Active)
                        .FirstOrDefaultAsync();
                    request.RiskProbabilityId = defaultProbability?.Id ?? throw new InvalidOperationException("Default LOW_TO_MEDIUM probability not found in system");
                }

                // Default to LOW_TO_MEDIUM impact if not provided
                if (!request.RiskImpactLevelId.HasValue)
                {
                    var defaultImpact = await _context.RiskImpactLevels
                        .Where(ril => ril.Code == "LOW_TO_MEDIUM" && !ril.IsDeleted && ril.Status == EntityStatus.Active)
                        .FirstOrDefaultAsync();
                    request.RiskImpactLevelId = defaultImpact?.Id ?? throw new InvalidOperationException("Default LOW_TO_MEDIUM impact level not found in system");
                }

                // Default to WITHIN_SIX_MONTHS proximity if not provided
                if (!request.RiskProximityId.HasValue)
                {
                    var defaultProximity = await _context.RiskProximities
                        .Where(rp => rp.Code == "WITHIN_SIX_MONTHS" && !rp.IsDeleted && rp.Status == EntityStatus.Active)
                        .FirstOrDefaultAsync();
                    request.RiskProximityId = defaultProximity?.Id ?? throw new InvalidOperationException("Default WITHIN_SIX_MONTHS proximity not found in system");
                }

                // Default to a general category if not provided (use first active Level 3 category as fallback)
                if (!request.RiskCategoryId.HasValue)
                {
                    var defaultCategory = await _context.RiskCategories
                        .Where(rc => rc.Level == 3 && !rc.IsDeleted && rc.Status == EntityStatus.Active)
                        .OrderBy(rc => rc.DisplayOrder)
                        .FirstOrDefaultAsync();
                    request.RiskCategoryId = defaultCategory?.Id ?? throw new InvalidOperationException("No Level 3 risk categories found in system");
                }

                // Response type remains optional for manual entry (will be null for Threat type)
                // No validation needed for manual entry mode
            }

            var risk = new Risk
            {
                Name = request.Title,
                EntityType = "Opportunity",
                EntityId = request.EntityId,
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                Recommendation = request.Recommendation ?? string.Empty,

                // New oUP-aligned fields (now guaranteed to have values after default logic)
                RiskTypeId = request.RiskTypeId!.Value,
                RiskCategoryId = request.RiskCategoryId!.Value,
                RiskProbabilityId = request.RiskProbabilityId!.Value,
                RiskProximityId = request.RiskProximityId!.Value,
                RiskImpactLevelId = request.RiskImpactLevelId!.Value,
                RiskResponseTypeId = request.RiskResponseTypeId,
                PreDefinedHighRiskId = request.PreDefinedHighRiskId,

                // Legacy fields (for backward compatibility)
                Impact = (RiskImpact)Math.Min(Math.Max(request.Impact, 1), 3),
                RiskStatus = RiskStatus.Open,

                // Audit fields
                IdentifiedDate = DateTime.UtcNow,
                IdentifiedBy = 0,
                Status = EntityStatus.Active
            };

            _context.Risks.Add(risk);
            await _context.SaveChangesAsync();

            // Reload with includes to get navigation properties
            // ✅ AsNoTracking for read-only reload after save
            var createdRisk = await _context.Risks
                .AsNoTracking()
                .Include(r => r.RiskTypeEntity)
                .Include(r => r.RiskCategory)
                .Include(r => r.RiskProbabilityEntity)
                .Include(r => r.RiskProximityEntity)
                .Include(r => r.RiskImpactLevelEntity)
                .Include(r => r.RiskResponseTypeEntity)
                .Include(r => r.PreDefinedHighRisk)
                .FirstAsync(r => r.Id == risk.Id);

            return MapRiskToModel(createdRisk);
        }

        /// <summary>
        /// Updates an existing risk
        /// </summary>
        public async Task<RiskModel> UpdateRiskAsync(int id, RiskCreateRequest request, ClaimsPrincipal? user = null)
        {
            var risk = await _context.Risks
                .Include(r => r.RiskTypeEntity)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (risk == null)
            {
                throw new KeyNotFoundException($"Risk with ID {id} not found");
            }

            // Title is ALWAYS required
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Risk title is required");
            }

            // Determine mode: Predefined High Risk OR Manual Entry
            bool isPredefinedHighRisk = request.PreDefinedHighRiskId.HasValue || risk.PreDefinedHighRiskId.HasValue;

            if (isPredefinedHighRisk)
            {
                // MODE A: Predefined High Risk - All oUP fields are mandatory
                if (!request.RiskTypeId.HasValue)
                    throw new ArgumentException("Risk type is required for predefined high risks");
                if (!request.RiskCategoryId.HasValue)
                    throw new ArgumentException("Risk category is required for predefined high risks");
                if (!request.RiskProbabilityId.HasValue)
                    throw new ArgumentException("Risk probability is required for predefined high risks");
                if (!request.RiskProximityId.HasValue)
                    throw new ArgumentException("Risk proximity is required for predefined high risks");
                if (!request.RiskImpactLevelId.HasValue)
                    throw new ArgumentException("Risk impact level is required for predefined high risks");

                // Validate FK fields
                await ValidateRiskForeignKeysAsync(request);

                // Validate ResponseType for Opportunity type
                var riskType = await _context.RiskTypes.FindAsync(request.RiskTypeId.Value);
                if (riskType?.IsResponseTypeMandatory == true && !request.RiskResponseTypeId.HasValue)
                {
                    throw new ArgumentException("ResponseType is mandatory for Opportunity risk type");
                }
            }
            else
            {
                // MODE B: Manual Entry - Apply defaults for missing oUP fields (same logic as Create)
                if (!request.RiskTypeId.HasValue)
                {
                    var defaultType = await _context.RiskTypes
                        .Where(rt => rt.Code == "THREAT" && !rt.IsDeleted && rt.Status == EntityStatus.Active)
                        .FirstOrDefaultAsync();
                    request.RiskTypeId = defaultType?.Id ?? throw new InvalidOperationException("Default THREAT risk type not found in system");
                }

                if (!request.RiskProbabilityId.HasValue)
                {
                    var defaultProbability = await _context.RiskProbabilities
                        .Where(rp => rp.Code == "LOW_TO_MEDIUM" && !rp.IsDeleted && rp.Status == EntityStatus.Active)
                        .FirstOrDefaultAsync();
                    request.RiskProbabilityId = defaultProbability?.Id ?? throw new InvalidOperationException("Default LOW_TO_MEDIUM probability not found in system");
                }

                if (!request.RiskImpactLevelId.HasValue)
                {
                    var defaultImpact = await _context.RiskImpactLevels
                        .Where(ril => ril.Code == "LOW_TO_MEDIUM" && !ril.IsDeleted && ril.Status == EntityStatus.Active)
                        .FirstOrDefaultAsync();
                    request.RiskImpactLevelId = defaultImpact?.Id ?? throw new InvalidOperationException("Default LOW_TO_MEDIUM impact level not found in system");
                }

                if (!request.RiskProximityId.HasValue)
                {
                    var defaultProximity = await _context.RiskProximities
                        .Where(rp => rp.Code == "WITHIN_SIX_MONTHS" && !rp.IsDeleted && rp.Status == EntityStatus.Active)
                        .FirstOrDefaultAsync();
                    request.RiskProximityId = defaultProximity?.Id ?? throw new InvalidOperationException("Default WITHIN_SIX_MONTHS proximity not found in system");
                }

                if (!request.RiskCategoryId.HasValue)
                {
                    var defaultCategory = await _context.RiskCategories
                        .Where(rc => rc.Level == 3 && !rc.IsDeleted && rc.Status == EntityStatus.Active)
                        .OrderBy(rc => rc.DisplayOrder)
                        .FirstOrDefaultAsync();
                    request.RiskCategoryId = defaultCategory?.Id ?? throw new InvalidOperationException("No Level 3 risk categories found in system");
                }
            }

            // Update all fields (now guaranteed to have values after default logic)
            risk.Name = request.Title;
            risk.Title = request.Title;
            risk.Description = request.Description ?? string.Empty;
            risk.Recommendation = request.Recommendation ?? string.Empty;
            risk.RiskTypeId = request.RiskTypeId!.Value;
            risk.RiskCategoryId = request.RiskCategoryId!.Value;
            risk.RiskProbabilityId = request.RiskProbabilityId!.Value;
            risk.RiskProximityId = request.RiskProximityId!.Value;
            risk.RiskImpactLevelId = request.RiskImpactLevelId!.Value;
            risk.RiskResponseTypeId = request.RiskResponseTypeId;
            risk.Impact = (RiskImpact)Math.Min(Math.Max(request.Impact, 1), 3);

            await _context.SaveChangesAsync();

            // Reload with includes
            // ✅ AsNoTracking for read-only reload after save
            var updatedRisk = await _context.Risks
                .AsNoTracking()
                .Include(r => r.RiskTypeEntity)
                .Include(r => r.RiskCategory)
                .Include(r => r.RiskProbabilityEntity)
                .Include(r => r.RiskProximityEntity)
                .Include(r => r.RiskImpactLevelEntity)
                .Include(r => r.RiskResponseTypeEntity)
                .Include(r => r.PreDefinedHighRisk)
                .FirstAsync(r => r.Id == risk.Id);

            return MapRiskToModel(updatedRisk);
        }

        /// <summary>
        /// Deletes a risk (soft delete)
        /// </summary>
        public async Task<bool> DeleteRiskAsync(int id, ClaimsPrincipal? user = null)
        {
            var risk = await _context.Risks.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (risk == null)
            {
                return false;
            }

            risk.IsDeleted = true;
            risk.DeletedDate = DateTime.UtcNow;
            risk.DeletedBy = 0;

            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Risk Lookups

        /// <summary>
        /// Gets all risk lookup data
        /// OPTIMIZED: Uses AsNoTracking for all read-only queries (Priority 2)
        /// </summary>
        public async Task<RiskLookupsResponse> GetRiskLookupsAsync()
        {
            var riskTypes = await _context.RiskTypes
                .AsNoTracking() // ✅ Read-only lookup query
                .Where(r => !r.IsDeleted && r.Status == EntityStatus.Active)
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new RiskTypeModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    IsResponseTypeMandatory = r.IsResponseTypeMandatory,
                    DisplayOrder = r.DisplayOrder
                })
                .ToListAsync();

            var probabilities = await _context.RiskProbabilities
                .AsNoTracking() // ✅ Read-only lookup query
                .Where(r => !r.IsDeleted && r.Status == EntityStatus.Active)
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new RiskProbabilityModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    DisplayLabel = r.DisplayLabel,
                    NumericValue = r.NumericValue,
                    DisplayOrder = r.DisplayOrder
                })
                .ToListAsync();

            var proximities = await _context.RiskProximities
                .AsNoTracking() // ✅ Read-only lookup query
                .Where(r => !r.IsDeleted && r.Status == EntityStatus.Active)
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new RiskProximityModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    MonthsValue = r.MonthsValue,
                    DisplayOrder = r.DisplayOrder
                })
                .ToListAsync();

            var impactLevels = await _context.RiskImpactLevels
                .AsNoTracking() // ✅ Read-only lookup query
                .Where(r => !r.IsDeleted && r.Status == EntityStatus.Active)
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new RiskImpactLevelModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    DisplayLabel = r.DisplayLabel,
                    NumericValue = r.NumericValue,
                    DisplayOrder = r.DisplayOrder
                })
                .ToListAsync();

            var responseTypes = await _context.RiskResponseTypes
                .AsNoTracking() // ✅ Read-only lookup query
                .Where(r => !r.IsDeleted && r.Status == EntityStatus.Active)
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new RiskResponseTypeModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    ValidForThreat = r.ValidForThreat,
                    ValidForOpportunity = r.ValidForOpportunity,
                    DisplayOrder = r.DisplayOrder
                })
                .ToListAsync();

            return new RiskLookupsResponse
            {
                RiskTypes = riskTypes,
                Probabilities = probabilities,
                Proximities = proximities,
                ImpactLevels = impactLevels,
                ResponseTypes = responseTypes
            };
        }

        /// <summary>
        /// Gets risk categories in hierarchical format
        /// OPTIMIZED: Uses AsNoTracking for read-only query (Priority 2)
        /// </summary>
        public async Task<RiskCategoryHierarchyResponse> GetRiskCategoriesAsync()
        {
            var allCategories = await _context.RiskCategories
                .AsNoTracking() // ✅ Read-only lookup query
                .Where(c => !c.IsDeleted && c.Status == EntityStatus.Active)
                .OrderBy(c => c.Level)
                .ThenBy(c => c.DisplayOrder)
                .ToListAsync();

            // Build hierarchy starting from Level 1
            var level1Categories = allCategories
                .Where(c => c.Level == 1)
                .Select(c => BuildCategoryHierarchy(c, allCategories))
                .ToList();

            // Get flat list of selectable (Level 3) categories
            var selectableCategories = allCategories
                .Where(c => c.Level == 3)
                .Select(c => new RiskCategoryModel
                {
                    Id = c.Id,
                    Code = c.Code,
                    ShortCode = c.ShortCode,
                    Name = c.Name,
                    Level = c.Level,
                    ParentCategoryId = c.ParentCategoryId,
                    DisplayOrder = c.DisplayOrder,
                    IsSelectable = true
                })
                .ToList();

            return new RiskCategoryHierarchyResponse
            {
                Categories = level1Categories,
                SelectableCategories = selectableCategories,
                TotalLevel1 = allCategories.Count(c => c.Level == 1),
                TotalLevel2 = allCategories.Count(c => c.Level == 2),
                TotalLevel3 = allCategories.Count(c => c.Level == 3)
            };
        }

        /// <summary>
        /// Gets all predefined high risks
        /// Includes fallback lookup for RiskCategoryId using CategoryCode if not set
        /// OPTIMIZED: Uses AsNoTracking for read-only queries (Priority 2)
        /// </summary>
        public async Task<List<PreDefinedHighRiskModel>> GetPreDefinedHighRisksAsync()
        {
            // First, get the raw data from PreDefinedHighRisks
            var highRisks = await _context.PreDefinedHighRisks
                .AsNoTracking() // ✅ Read-only query
                .Include(r => r.RiskCategory)
                .Where(r => !r.IsDeleted && r.Status == EntityStatus.Active)
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new PreDefinedHighRiskModel
                {
                    Id = r.Id,
                    Code = r.Code,
                    DisplayCode = r.DisplayCode,
                    Name = r.Name,
                    ShortTitle = r.ShortTitle,
                    Description = r.Description,
                    CategoryCode = r.CategoryCode,
                    Level1 = r.Level1,
                    Level2Code = r.Level2Code,
                    IsAutoDetectable = r.IsAutoDetectable,
                    DetectionRuleType = r.DetectionRuleType,
                    DisplayOrder = r.DisplayOrder,
                    RiskCategoryId = r.RiskCategoryId,
                    RiskCategoryName = r.RiskCategory != null ? r.RiskCategory.Name : null,
                    OupQuestionId = r.OupQuestionId
                })
                .ToListAsync();

            // Fallback: If any RiskCategoryId is null or 0, try to lookup by CategoryCode (ShortCode)
            var missingCategoryHighRisks = highRisks.Where(hr => !hr.RiskCategoryId.HasValue || hr.RiskCategoryId == 0).ToList();
            if (missingCategoryHighRisks.Any())
            {
                // Get category lookup by ShortCode (Level 3 categories only)
                var categoryLookup = await _context.RiskCategories
                    .AsNoTracking() // ✅ Read-only lookup query
                    .Where(c => c.Level == 3 && !c.IsDeleted)
                    .ToDictionaryAsync(c => c.ShortCode, c => new { c.Id, c.Name });

                foreach (var hr in missingCategoryHighRisks)
                {
                    if (!string.IsNullOrEmpty(hr.CategoryCode) && categoryLookup.TryGetValue(hr.CategoryCode, out var category))
                    {
                        hr.RiskCategoryId = category.Id;
                        hr.RiskCategoryName = category.Name;
                    }
                }
            }

            return highRisks;
        }

        #endregion

        #region High Risk Analysis

        /// <summary>
        /// Analyzes an opportunity and returns high risk recommendations
        /// OPTIMIZED: Uses AsNoTracking for all read-only queries (Priority 2)
        /// </summary>
        public async Task<HighRiskAnalysisResponse> GetHighRiskAnalysisAsync(int opportunityId, ClaimsPrincipal? user = null)
        {
            // Get all predefined high risks
            var allHighRisks = await GetPreDefinedHighRisksAsync();

            // Get existing risks for this opportunity to find already added high risks
            var existingRisks = await _context.Risks
                .AsNoTracking() // ✅ Read-only query
                .Where(r => r.EntityType == "Opportunity" && r.EntityId == opportunityId && !r.IsDeleted)
                .Select(r => r.PreDefinedHighRiskId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToListAsync();

            // Get opportunity data for auto-detection
            var opportunity = await _context.Set<Opportunity>()
                .AsNoTracking() // ✅ Read-only query for analysis
                .Include(o => o.FundingPartners)
                    .ThenInclude(fp => fp.Partner)
                .Include(o => o.Countries)
                    .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(o => o.Id == opportunityId);

            var recommendations = new List<HighRiskRecommendation>();

            if (opportunity != null)
            {
                // Auto-detect high risks based on opportunity data
                foreach (var highRisk in allHighRisks.Where(hr => hr.IsAutoDetectable))
                {
                    var (isDetected, confidence, reason, triggerData) = DetectHighRisk(highRisk, opportunity);
                    if (isDetected)
                    {
                        recommendations.Add(new HighRiskRecommendation
                        {
                            PreDefinedHighRisk = highRisk,
                            ConfidenceLevel = confidence,
                            DetectionReason = reason,
                            TriggerData = triggerData
                        });
                    }
                }
            }

            return new HighRiskAnalysisResponse
            {
                AvailableHighRisks = allHighRisks,
                Recommendations = recommendations.OrderByDescending(r => r.ConfidenceLevel).ToList(),
                AlreadyAddedHighRiskIds = existingRisks,
                TotalHighRisks = allHighRisks.Count,
                StronglyRecommendedCount = recommendations.Count(r => r.IsStronglyRecommended)
            };
        }

        #endregion

        #region Helper Methods

        private RiskModel MapRiskToModel(Risk r)
        {
            return new RiskModel
            {
                Id = r.Id,
                EntityType = r.EntityType,
                EntityId = r.EntityId,
                Title = r.Title,
                Description = r.Description,
                Recommendation = r.Recommendation,

                // New oUP-aligned fields
                RiskTypeId = r.RiskTypeId,
                RiskTypeName = r.RiskTypeEntity?.Name,
                RiskTypeCode = r.RiskTypeEntity?.Code,
                RiskCategoryId = r.RiskCategoryId,
                RiskCategoryName = r.RiskCategory?.Name,
                RiskCategoryFullPath = GetCategoryFullPath(r.RiskCategory),
                RiskProbabilityId = r.RiskProbabilityId,
                RiskProbabilityName = r.RiskProbabilityEntity?.Name,
                RiskProximityId = r.RiskProximityId,
                RiskProximityName = r.RiskProximityEntity?.Name,
                RiskImpactLevelId = r.RiskImpactLevelId,
                RiskImpactLevelName = r.RiskImpactLevelEntity?.Name,
                RiskResponseTypeId = r.RiskResponseTypeId,
                RiskResponseTypeName = r.RiskResponseTypeEntity?.Name,

                // PreDefined High Risk reference
                PreDefinedHighRiskId = r.PreDefinedHighRiskId,
                PreDefinedHighRiskCode = r.PreDefinedHighRisk?.Code,
                PreDefinedHighRiskTitle = r.PreDefinedHighRisk?.ShortTitle,

                // Legacy fields
                Impact = (int)r.Impact,
                Status = r.RiskStatus.ToString(),

                // Audit fields
                IdentifiedDate = r.IdentifiedDate,
                IdentifiedBy = r.IdentifiedBy?.ToString(),
                CreatedDate = r.CreatedDate,
                CreatedBy = r.CreatedBy.ToString()
            };
        }

        private string? GetCategoryFullPath(RiskCategory? category)
        {
            if (category == null) return null;

            var path = new List<string> { category.Name };
            // Note: For full path, we'd need to load parent categories
            // For now, just return the category name
            return category.Name;
        }

        private RiskCategoryModel BuildCategoryHierarchy(RiskCategory category, List<RiskCategory> allCategories)
        {
            var model = new RiskCategoryModel
            {
                Id = category.Id,
                Code = category.Code,
                ShortCode = category.ShortCode,
                Name = category.Name,
                Level = category.Level,
                ParentCategoryId = category.ParentCategoryId,
                DisplayOrder = category.DisplayOrder,
                IsSelectable = category.Level == 3
            };

            // Find children
            var children = allCategories.Where(c => c.ParentCategoryId == category.Id);
            model.Children = children.Select(c => BuildCategoryHierarchy(c, allCategories)).ToList();

            return model;
        }

        private async Task ValidateRiskForeignKeysAsync(RiskCreateRequest request)
        {
            // Validate RiskType exists (if provided)
            if (request.RiskTypeId.HasValue && !await _context.RiskTypes.AnyAsync(r => r.Id == request.RiskTypeId.Value && !r.IsDeleted))
            {
                throw new ArgumentException($"Invalid RiskTypeId: {request.RiskTypeId}");
            }

            // Validate RiskCategory exists and is Level 3 (leaf) - if provided
            if (request.RiskCategoryId.HasValue)
            {
                var category = await _context.RiskCategories.FirstOrDefaultAsync(r => r.Id == request.RiskCategoryId.Value && !r.IsDeleted);
                if (category == null)
                {
                    throw new ArgumentException($"Invalid RiskCategoryId: {request.RiskCategoryId}");
                }
                if (category.Level != 3)
                {
                    throw new ArgumentException("Only Level 3 (leaf) categories can be selected for risks");
                }
            }

            // Validate RiskProbability exists (if provided)
            if (request.RiskProbabilityId.HasValue && !await _context.RiskProbabilities.AnyAsync(r => r.Id == request.RiskProbabilityId.Value && !r.IsDeleted))
            {
                throw new ArgumentException($"Invalid RiskProbabilityId: {request.RiskProbabilityId}");
            }

            // Validate RiskProximity exists (if provided)
            if (request.RiskProximityId.HasValue && !await _context.RiskProximities.AnyAsync(r => r.Id == request.RiskProximityId.Value && !r.IsDeleted))
            {
                throw new ArgumentException($"Invalid RiskProximityId: {request.RiskProximityId}");
            }

            // Validate RiskImpactLevel exists (if provided)
            if (request.RiskImpactLevelId.HasValue && !await _context.RiskImpactLevels.AnyAsync(r => r.Id == request.RiskImpactLevelId.Value && !r.IsDeleted))
            {
                throw new ArgumentException($"Invalid RiskImpactLevelId: {request.RiskImpactLevelId}");
            }

            // Validate RiskResponseType exists (if provided)
            if (request.RiskResponseTypeId.HasValue && !await _context.RiskResponseTypes.AnyAsync(r => r.Id == request.RiskResponseTypeId.Value && !r.IsDeleted))
            {
                throw new ArgumentException($"Invalid RiskResponseTypeId: {request.RiskResponseTypeId}");
            }

            // Validate PreDefinedHighRisk if provided
            if (request.PreDefinedHighRiskId.HasValue)
            {
                if (!await _context.PreDefinedHighRisks.AnyAsync(r => r.Id == request.PreDefinedHighRiskId.Value && !r.IsDeleted))
                {
                    throw new ArgumentException($"Invalid PreDefinedHighRiskId: {request.PreDefinedHighRiskId}");
                }
            }
        }

        private (bool IsDetected, int Confidence, string Reason, string TriggerData) DetectHighRisk(
            PreDefinedHighRiskModel highRisk, Opportunity opportunity)
        {
            switch (highRisk.DetectionRuleType)
            {
                case "COUNTRY_FRAGILE":
                    // Check if any country is in a fragile/conflict state
                    // This would need a fragile countries list - for now, return false
                    return (false, 0, string.Empty, string.Empty);

                case "PARTNER_DRAFT":
                    // Check if any funding partner is new (draft status)
                    var draftPartners = opportunity.FundingPartners?
                        .Where(fp => fp.Partner?.Status == EntityStatus.Draft)
                        .Select(fp => fp.Partner?.Name)
                        .ToList();

                    if (draftPartners?.Any() == true)
                    {
                        return (true, 85, "New funding source or client detected",
                            $"Partners in draft status: {string.Join(", ", draftPartners)}");
                    }
                    return (false, 0, string.Empty, string.Empty);

                case "NON_USD_CURRENCY":
                    // Check if opportunity has non-USD currency
                    // This would need currency field - for now, return false
                    return (false, 0, string.Empty, string.Empty);

                default:
                    return (false, 0, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Implementation of abstract method from BaseUNOPSManager
        /// OPTIMIZED: Uses AsNoTracking for read-only query (Priority 2)
        /// </summary>
        public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
        {
            var risk = await _context.Risks
                .AsNoTracking() // ✅ Read-only query
                .Include(r => r.RiskTypeEntity)
                .Include(r => r.RiskCategory)
                .Include(r => r.RiskProbabilityEntity)
                .Include(r => r.RiskProximityEntity)
                .Include(r => r.RiskImpactLevelEntity)
                .Include(r => r.RiskResponseTypeEntity)
                .Include(r => r.PreDefinedHighRisk)
                .FirstOrDefaultAsync(r => r.Id == entityId);

            if (risk == null)
            {
                return null;
            }

            return MapRiskToModel(risk);
        }

        #endregion
    }
}

