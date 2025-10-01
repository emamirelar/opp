using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Extensions;

public static class ContactExtensions
{
    /// <summary>
    /// Efficiently loads OrganizationUnitRelationships for a single UNOPS contact
    /// </summary>
    public static async Task LoadOrganizationUnitRelationshipsAsync(this UNOPSContact contact, UNOPSAppDbContext context)
    {
        if (contact?.Id > 0)
        {
            contact.OrganizationUnitRelationships = await context.OrganizationUnitRelationships
                .Include(r => r.OrganizationHierarchy)
                .Where(r => r.EntityId == contact.Id && r.EntityType == nameof(Contact))
                .ToListAsync();
        }
    }

    /// <summary>
    /// Efficiently batch loads OrganizationUnitRelationships for multiple UNOPS contacts (avoids N+1)
    /// </summary>
    public static async Task LoadOrganizationUnitRelationshipsAsync(this IEnumerable<UNOPSContact> contacts, UNOPSAppDbContext context)
    {
        var contactList = contacts.ToList();
        if (!contactList.Any()) return;

        var contactIds = contactList.Select(c => c.Id).ToList();

        // Single query to get all relationships for all contacts
        var allRelationships = await context.OrganizationUnitRelationships
            .Include(r => r.OrganizationHierarchy)
            .Where(r => contactIds.Contains(r.EntityId) && r.EntityType == nameof(Contact))
            .ToListAsync();

        // Group by contact and assign to each contact
        var relationshipsByContact = allRelationships.GroupBy(r => r.EntityId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var contact in contactList)
        {
            contact.OrganizationUnitRelationships = relationshipsByContact.TryGetValue(contact.Id, out var relationships)
                ? relationships
                : new List<OrganizationUnitRelationship>();
        }
    }

    /// <summary>
    /// Loads relationships only for UNOPS contacts that don't already have them loaded
    /// </summary>
    public static async Task EnsureOrganizationUnitRelationshipsLoadedAsync(this IEnumerable<UNOPSContact> contacts, UNOPSAppDbContext context)
    {
        var contactsNeedingLoad = contacts.Where(c =>
            c.OrganizationUnitRelationships == null || !c.OrganizationUnitRelationships.Any()).ToList();

        if (contactsNeedingLoad.Any())
        {
            await contactsNeedingLoad.LoadOrganizationUnitRelationshipsAsync(context);
        }
    }
}