namespace UNOPS.PAO.Identity.Security;

public class BaseRole
{
    public const string UNOPS_GEN_USER = "UNOPS_GEN_USER";
    public const string PARTNER_GLOB_ADMIN = "PARTNER_GLOB_ADMIN";
    public const string PARTNER_USER = "PARTNER_USER";
    public const string ORG_UNIT_ADMIN = "ORG_UNIT_ADMIN";
    public const string SYSTEM_ADMIN = "SYSTEM_ADMIN";


    public static List<(string Name, string Description, List<Permission> Permissions)> GetAllRoles()
    {
        return new List<(string Name, string Description, List<Permission> Permissions)>
        {
            (
                UNOPS_GEN_USER,
                "General User",
                new List<Permission>()
            ),
            (
                PARTNER_GLOB_ADMIN,
                "Partnership Global Admin",
                new List<Permission>()
            ),
            (
                PARTNER_USER,
                "Partnership User",
                new List<Permission>()
            ),
            (
                ORG_UNIT_ADMIN,
                "Org Unit Admin",
                new List<Permission>()
            ),
            (
                SYSTEM_ADMIN,
                "System Administrator",
                new List<Permission>
                {
                    Permission.CanRunMigrations,
                    Permission.CanRunSeedings
                }
            )
        };
    }
}
