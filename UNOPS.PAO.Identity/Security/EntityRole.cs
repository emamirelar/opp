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
        {};

        return entityRoles.Where(x => x.Entity == entity).ToList();
    }
}
