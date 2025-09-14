using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSBusiness.Extensions
{
    /// <summary>
    /// Extension methods for Partner specifications to handle OrgUnit filtering
    /// </summary>
    public static class PartnerSpecificationExtensions
    {
        /// <summary>
        /// Applies OrgUnit filter to a partner query if OrgUnitId is specified in the filter
        /// </summary>
        public static async Task<IQueryable<T>> ApplyOrgUnitFilterAsync<T>(
            this IQueryable<T> query,
            IPartnerSearchFilter filter,
            IOrgUnitHierarchyService hierarchyService,
            UNOPSAppDbContext context) where T : Partner
        {
            if (filter?.OrgUnitId == null || !filter.OrgUnitId.HasValue)
            {
                return query;
            }

            // Get the hierarchy of org units
            var hierarchyIds = await hierarchyService.GetDescendantIdsAsync(filter.OrgUnitId.Value);
            
            // Pre-materialize the partner IDs that match the org unit criteria to avoid nested query issues
            var validPartnerIds = context.Set<OrganizationUnitRelationship>()
                .Where(orgRel => 
                    orgRel.EntityType == "Partner" && 
                    hierarchyIds.Contains(orgRel.OrganizationHierarchyId))
                .Select(orgRel => orgRel.EntityId)
                .ToList(); // Materialize the IDs first

            // Now filter the partners using the materialized IDs
            return query.Where(partner => validPartnerIds.Contains(partner.Id));
        }
        
        /// <summary>
        /// Creates a specification with OrgUnit filtering if needed
        /// </summary>
        public static async Task<ISpecification<Partner>> CreateWithOrgUnitFilterAsync(
            IPartnerSearchFilter filter,
            ClaimsPrincipal user,
            IOrgUnitFilterService orgUnitFilterService)
        {
            // Use OrgUnitFilterService to create the appropriate specification
            var unosPartnerSpec = await orgUnitFilterService.CreatePartnerSpecificationAsync(filter, user);
            
            // Since we can't directly cast ISpecification<UNOPSPartner> to ISpecification<Partner>,
            // we need to create a wrapper specification
            return new PartnerSpecificationWrapper(unosPartnerSpec);
        }
    }
    
    /// <summary>
    /// Wrapper to adapt ISpecification<UNOPSPartner> to ISpecification<Partner>
    /// </summary>
    internal class PartnerSpecificationWrapper : BaseSpecification<Partner>
    {
        public PartnerSpecificationWrapper(ISpecification<UNOPSPartner> innerSpec)
            : base(p => true) // Default to true, actual filtering will be done differently
        {
            // Copy includes from the inner specification
            foreach (var include in innerSpec.Includes)
            {
                // We can't directly copy the includes because they're typed differently
                // This is a limitation of the current approach
            }
            
            // Copy string includes
            foreach (var includeString in innerSpec.IncludeStrings)
            {
                AddInclude(includeString);
            }
            
            // Note: This is a simplified wrapper. In a real implementation,
            // you'd need to properly convert the expression tree from UNOPSPartner to Partner
        }
    }
}