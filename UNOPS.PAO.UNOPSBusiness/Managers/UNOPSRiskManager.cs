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
        /// </summary>
        public async Task<DSTRisksResponse> GetRisksByEntityAsync(string entityType, int entityId, ClaimsPrincipal? user = null)
        {
            var risks = await _context.Risks
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
        /// </summary>
        public async Task<RiskModel> CreateRiskAsync(RiskCreateRequest request, ClaimsPrincipal? user = null)
        {
            // Validate mandatory FK fields exist
            await ValidateRiskForeignKeysAsync(request);

            // Validate ResponseType is provided for Opportunity type
            var riskType = await _context.RiskTypes.FindAsync(request.RiskTypeId);
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

            var risk = new Risk
            {
                Name = request.Title,
                EntityType = "Opportunity",
                EntityId = request.EntityId,
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                Recommendation = request.Recommendation ?? string.Empty,

                // New oUP-aligned fields
                RiskTypeId = request.RiskTypeId,
                RiskCategoryId = request.RiskCategoryId,
                RiskProbabilityId = request.RiskProbabilityId,
                RiskProximityId = request.RiskProximityId,
                RiskImpactLevelId = request.RiskImpactLevelId,
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
            var createdRisk = await _context.Risks
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

            // Validate FK fields
            await ValidateRiskForeignKeysAsync(request);

            // Validate ResponseType for Opportunity type
            var riskType = await _context.RiskTypes.FindAsync(request.RiskTypeId);
            if (riskType?.IsResponseTypeMandatory == true && !request.RiskResponseTypeId.HasValue)
            {
                throw new ArgumentException("ResponseType is mandatory for Opportunity risk type");
            }

            // Update all fields
            risk.Name = request.Title;
            risk.Title = request.Title;
            risk.Description = request.Description ?? string.Empty;
            risk.Recommendation = request.Recommendation ?? string.Empty;
            risk.RiskTypeId = request.RiskTypeId;
            risk.RiskCategoryId = request.RiskCategoryId;
            risk.RiskProbabilityId = request.RiskProbabilityId;
            risk.RiskProximityId = request.RiskProximityId;
            risk.RiskImpactLevelId = request.RiskImpactLevelId;
            risk.RiskResponseTypeId = request.RiskResponseTypeId;
            risk.Impact = (RiskImpact)Math.Min(Math.Max(request.Impact, 1), 3);

            await _context.SaveChangesAsync();

            // Reload with includes
            var updatedRisk = await _context.Risks
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
        /// </summary>
        public async Task<RiskLookupsResponse> GetRiskLookupsAsync()
        {
            var riskTypes = await _context.RiskTypes
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
        /// </summary>
        public async Task<RiskCategoryHierarchyResponse> GetRiskCategoriesAsync()
        {
            var allCategories = await _context.RiskCategories
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
        /// </summary>
        public async Task<List<PreDefinedHighRiskModel>> GetPreDefinedHighRisksAsync()
        {
            return await _context.PreDefinedHighRisks
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
                    RiskCategoryName = r.RiskCategory != null ? r.RiskCategory.Name : null
                })
                .ToListAsync();
        }

        #endregion

        #region High Risk Analysis

        /// <summary>
        /// Analyzes an opportunity and returns high risk recommendations
        /// </summary>
        public async Task<HighRiskAnalysisResponse> GetHighRiskAnalysisAsync(int opportunityId, ClaimsPrincipal? user = null)
        {
            // Get all predefined high risks
            var allHighRisks = await GetPreDefinedHighRisksAsync();

            // Get existing risks for this opportunity to find already added high risks
            var existingRisks = await _context.Risks
                .Where(r => r.EntityType == "Opportunity" && r.EntityId == opportunityId && !r.IsDeleted)
                .Select(r => r.PreDefinedHighRiskId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToListAsync();

            // Get opportunity data for auto-detection
            var opportunity = await _context.Set<Opportunity>()
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
            // Validate RiskType exists
            if (!await _context.RiskTypes.AnyAsync(r => r.Id == request.RiskTypeId && !r.IsDeleted))
            {
                throw new ArgumentException($"Invalid RiskTypeId: {request.RiskTypeId}");
            }

            // Validate RiskCategory exists and is Level 3 (leaf)
            var category = await _context.RiskCategories.FirstOrDefaultAsync(r => r.Id == request.RiskCategoryId && !r.IsDeleted);
            if (category == null)
            {
                throw new ArgumentException($"Invalid RiskCategoryId: {request.RiskCategoryId}");
            }
            if (category.Level != 3)
            {
                throw new ArgumentException("Only Level 3 (leaf) categories can be selected for risks");
            }

            // Validate RiskProbability exists
            if (!await _context.RiskProbabilities.AnyAsync(r => r.Id == request.RiskProbabilityId && !r.IsDeleted))
            {
                throw new ArgumentException($"Invalid RiskProbabilityId: {request.RiskProbabilityId}");
            }

            // Validate RiskProximity exists
            if (!await _context.RiskProximities.AnyAsync(r => r.Id == request.RiskProximityId && !r.IsDeleted))
            {
                throw new ArgumentException($"Invalid RiskProximityId: {request.RiskProximityId}");
            }

            // Validate RiskImpactLevel exists
            if (!await _context.RiskImpactLevels.AnyAsync(r => r.Id == request.RiskImpactLevelId && !r.IsDeleted))
            {
                throw new ArgumentException($"Invalid RiskImpactLevelId: {request.RiskImpactLevelId}");
            }

            // Validate RiskResponseType if provided
            if (request.RiskResponseTypeId.HasValue)
            {
                if (!await _context.RiskResponseTypes.AnyAsync(r => r.Id == request.RiskResponseTypeId.Value && !r.IsDeleted))
                {
                    throw new ArgumentException($"Invalid RiskResponseTypeId: {request.RiskResponseTypeId}");
                }
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
        /// </summary>
        public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
        {
            var risk = await _context.Risks
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

