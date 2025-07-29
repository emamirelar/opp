using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Business.Extensions;

public static class PartnerExtensions
{
    /// <summary>
    /// Efficiently loads OrganizationUnitRelationships for a single partner
    /// </summary>
    public static async Task LoadOrganizationUnitRelationshipsAsync(this Partner partner, AppDbContext context)
    {
        if (partner?.Id > 0)
        {
            partner.OrganizationUnitRelationships = await context.OrganizationUnitRelationships
                .Include(r => r.OrganizationHierarchy)
                .Where(r => r.EntityId == partner.Id && r.EntityType == nameof(Partner))
                .ToListAsync();
        }
    }

    /// <summary>
    /// Efficiently batch loads OrganizationUnitRelationships for multiple partners (avoids N+1)
    /// </summary>
    public static async Task LoadOrganizationUnitRelationshipsAsync(this IEnumerable<Partner> partners, AppDbContext context)
    {
        var partnerList = partners.ToList();
        if (!partnerList.Any()) return;

        var partnerIds = partnerList.Select(p => p.Id).ToList();

        // Single query to get all relationships for all partners
        var allRelationships = await context.OrganizationUnitRelationships
            .Include(r => r.OrganizationHierarchy)
            .Where(r => partnerIds.Contains(r.EntityId) && r.EntityType == nameof(Partner))
            .ToListAsync();

        // Group by partner and assign to each partner
        var relationshipsByPartner = allRelationships.GroupBy(r => r.EntityId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var partner in partnerList)
        {
            partner.OrganizationUnitRelationships = relationshipsByPartner.TryGetValue(partner.Id, out var relationships)
                ? relationships
                : new List<OrganizationUnitRelationship>();
        }
    }

    /// <summary>
    /// Loads relationships only for partners that don't already have them loaded
    /// </summary>
    public static async Task EnsureOrganizationUnitRelationshipsLoadedAsync(this IEnumerable<Partner> partners, AppDbContext context)
    {
        var partnersNeedingLoad = partners.Where(p => 
            p.OrganizationUnitRelationships == null || !p.OrganizationUnitRelationships.Any()).ToList();
        
        if (partnersNeedingLoad.Any())
        {
            await partnersNeedingLoad.LoadOrganizationUnitRelationshipsAsync(context);
        }
    }
} 