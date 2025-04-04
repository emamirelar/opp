using UNOPS.PAO.Identity.Security.Enums;

namespace UNOPS.PAO.Identity.Security;
public class EntityRole
{
    public string Entity { get; private set; }
    public string Role { get; private set; }

    public int? Number { get; private set; }

    public EntityRole(string entity, string role, int? number = null)
    {
        Entity = entity;
        Role = role;
        Number = number;
    }

    public static List<EntityRole> Get(string entity)
    {
        var entityRoles = new List<EntityRole>
        {
            //commenting as we do not have roles yet
            /*// Contact
            new EntityRole(EntityNames.Contact, BaseRole.ProjectManager, 1),
            new EntityRole(EntityNames.Contact, BaseRole.DeputyProjectManager, 1),
            new EntityRole(EntityNames.Contact, BaseRole.Contributor),
            new EntityRole(EntityNames.Contact, BaseRole.CoOwner),
            new EntityRole(EntityNames.Contact, BaseRole.Reviewer),

            // Partner
            new EntityRole(EntityNames.Partner, BaseRole.GrantAuthority, 1),
            new EntityRole(EntityNames.Partner, BaseRole.GrantOfficial, 1),
            new EntityRole(EntityNames.Partner, BaseRole.IP, 1),
            new EntityRole(EntityNames.Partner, BaseRole.Applicant),
            new EntityRole(EntityNames.Partner, BaseRole.Evaluator),

            // Proposal*/
        };

        return entityRoles.Where(x => x.Entity == entity).ToList();
    }
}
