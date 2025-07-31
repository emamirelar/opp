using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace UNOPS.PAO.UNOPSBusiness.Extensions
{
    public static class InteractionExtensions
    {
        /// <summary>
        /// Loads OrganizationUnitRelationships for a single interaction
        /// </summary>
        public static async Task LoadOrganizationUnitRelationshipsAsync(this Interaction interaction, UNOPSAppDbContext context)
        {
            if (interaction?.Id > 0)
            {
                interaction.OrganizationUnitRelationships = await context.Set<OrganizationUnitRelationship>()
                    .Where(r => r.EntityId == interaction.Id && r.EntityType == "Interaction")
                    .Include(r => r.OrganizationHierarchy)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Loads OrganizationUnitRelationships for multiple interactions efficiently
        /// </summary>
        public static async Task LoadOrganizationUnitRelationshipsAsync(this IEnumerable<Interaction> interactions, UNOPSAppDbContext context)
        {
            var interactionIds = interactions.Where(i => i?.Id > 0).Select(i => i.Id).ToList();
            
            if (interactionIds.Any())
            {
                var relationships = await context.Set<OrganizationUnitRelationship>()
                    .Where(r => interactionIds.Contains(r.EntityId) && r.EntityType == "Interaction")
                    .Include(r => r.OrganizationHierarchy)
                    .ToListAsync();

                var relationshipLookup = relationships.GroupBy(r => r.EntityId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var interaction in interactions.Where(i => i?.Id > 0))
                {
                    interaction.OrganizationUnitRelationships = relationshipLookup.ContainsKey(interaction.Id) 
                        ? relationshipLookup[interaction.Id] 
                        : new List<OrganizationUnitRelationship>();
                }
            }
        }

        /// <summary>
        /// Ensures OrganizationUnitRelationships are loaded for a single interaction (loads if not already loaded)
        /// </summary>
        public static async Task EnsureOrganizationUnitRelationshipsLoadedAsync(this Interaction interaction, UNOPSAppDbContext context)
        {
            if (interaction?.OrganizationUnitRelationships == null || !interaction.OrganizationUnitRelationships.Any())
            {
                await interaction.LoadOrganizationUnitRelationshipsAsync(context);
            }
        }
    }
} 