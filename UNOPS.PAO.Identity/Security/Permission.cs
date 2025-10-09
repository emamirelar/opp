using UNOPS.PAO.Identity.Security.Enums;

namespace UNOPS.PAO.Identity.Security
{
    public class Permission
    {
        public string Name { get; private set; }
        public string Description { get; private set; }

        public Permission(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public static Permission CanRunMigrations = new Permission(PermissionNames.CanRunMigrations, "Can run DB migrations.");
        public static Permission CanRunSeedings = new Permission(PermissionNames.CanRunSeedings, "Can run data seedings.");
    }
}
