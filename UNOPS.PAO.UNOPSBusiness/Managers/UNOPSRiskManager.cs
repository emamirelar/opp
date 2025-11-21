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
    /// UNOPS-specific implementation of Risk management operations
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

        /// <summary>
        /// Gets all risks for a specific entity
        /// </summary>
        public async Task<DSTRisksResponse> GetRisksByEntityAsync(string entityType, int entityId, ClaimsPrincipal? user = null)
        {
            var risks = await _context.Risks
                .Where(r => r.EntityType == entityType && r.EntityId == entityId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            var riskModels = risks.Select(r => new RiskModel
            {
                Id = r.Id,
                EntityType = r.EntityType,
                EntityId = r.EntityId,
                Title = r.Title,
                Description = r.Description,
                Recommendation = r.Recommendation,
                Impact = (int)r.Impact, // Convert enum to int
                Status = r.RiskStatus.ToString(),
                IdentifiedDate = r.IdentifiedDate,
                IdentifiedBy = r.IdentifiedBy?.ToString(),
                CreatedDate = r.CreatedDate,
                CreatedBy = r.CreatedBy.ToString()
            }).ToList();

            return new DSTRisksResponse
            {
                Risks = riskModels,
                TotalCount = riskModels.Count()
            };
        }

        /// <summary>
        /// Creates a new risk
        /// </summary>
        public async Task<RiskModel> CreateRiskAsync(RiskCreateRequest request, ClaimsPrincipal? user = null)
        {
            // Validate impact range (1=Low, 2=Medium, 3=High)
            if (request.Impact < 1 || request.Impact > 3)
            {
                throw new ArgumentException("Impact must be between 1 (Low) and 3 (High)");
            }

            var impactEnum = (RiskImpact)request.Impact;

            var risk = new Risk
            {
                Name = request.Title, // Use Title as Name (required by base entity)
                EntityType = "Opportunity",
                EntityId = request.EntityId,
                Title = request.Title,
                Description = request.Description,
                Recommendation = request.Recommendation,
                Impact = impactEnum,
                RiskStatus = RiskStatus.Open,
                IdentifiedDate = DateTime.UtcNow,
                IdentifiedBy = 0, // Will be set by EF audit interceptor
                Status = EntityStatus.Active
            };

            _context.Risks.Add(risk);
            await _context.SaveChangesAsync();

            return new RiskModel
            {
                Id = risk.Id,
                EntityType = risk.EntityType,
                EntityId = risk.EntityId,
                Title = risk.Title,
                Description = risk.Description,
                Recommendation = risk.Recommendation,
                Impact = (int)risk.Impact,
                Status = risk.RiskStatus.ToString(),
                IdentifiedDate = risk.IdentifiedDate,
                IdentifiedBy = risk.IdentifiedBy?.ToString(),
                CreatedDate = risk.CreatedDate,
                CreatedBy = risk.CreatedBy.ToString()
            };
        }

        /// <summary>
        /// Updates an existing risk
        /// </summary>
        public async Task<RiskModel> UpdateRiskAsync(int id, RiskCreateRequest request, ClaimsPrincipal? user = null)
        {
            var risk = await _context.Risks.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (risk == null)
            {
                throw new KeyNotFoundException($"Risk with ID {id} not found");
            }

            // Validate impact range (1=Low, 2=Medium, 3=High)
            if (request.Impact < 1 || request.Impact > 3)
            {
                throw new ArgumentException("Impact must be between 1 (Low) and 3 (High)");
            }

            risk.Impact = (RiskImpact)request.Impact;
            risk.Name = request.Title; // Update Name to match Title
            risk.Title = request.Title;
            risk.Description = request.Description;
            risk.Recommendation = request.Recommendation;

            await _context.SaveChangesAsync();

            return new RiskModel
            {
                Id = risk.Id,
                EntityType = risk.EntityType,
                EntityId = risk.EntityId,
                Title = risk.Title,
                Description = risk.Description,
                Recommendation = risk.Recommendation,
                Impact = (int)risk.Impact,
                Status = risk.RiskStatus.ToString(),
                IdentifiedDate = risk.IdentifiedDate,
                IdentifiedBy = risk.IdentifiedBy?.ToString(),
                CreatedDate = risk.CreatedDate,
                CreatedBy = risk.CreatedBy.ToString()
            };
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
            risk.DeletedBy = 0; // Will be set by EF audit interceptor

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Implementation of abstract method from BaseUNOPSManager
        /// Gets a risk by ID and returns it as an object
        /// </summary>
        public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
        {
            var risk = await _riskRepository.GetByIdAsync(entityId);
            if (risk == null)
            {
                return null;
            }

            return new RiskModel
            {
                Id = risk.Id,
                EntityType = risk.EntityType,
                EntityId = risk.EntityId,
                Title = risk.Title,
                Description = risk.Description,
                Recommendation = risk.Recommendation,
                Impact = (int)risk.Impact,
                Status = risk.RiskStatus.ToString(),
                IdentifiedDate = risk.IdentifiedDate,
                IdentifiedBy = risk.IdentifiedBy?.ToString(),
                CreatedDate = risk.CreatedDate,
                CreatedBy = risk.CreatedBy.ToString()
            };
        }
    }
}

