namespace UNOPS.PAO.Identity.Security;

public class BaseRole
{
    public const string SystemAdmin = "SystemAdmin";
    public const string Contributor = "Contributor";
    public const string CoOwner = "Co-owner";
    public const string Reviewer = "Reviewer";    

    public static List<(string Name, string Description, List<Permission> Permissions)> GetAllRoles()
    {
        return new List<(string Name, string Description, List<Permission> Permissions)>
                        {
                            (
                                SystemAdmin,
                                "The System Administrator is responsible for maintaining the database and performing essential administrative tasks. This includes creating new user accounts, resetting passwords, managing system configurations, and ensuring the system's overall functionality and security. They provide technical support and resolve user access issues to ensure smooth operations.",
                                new List<Permission> { Permission.CanRunMigrations }
                            ),
                            (
                                Contributor,
                                "",
                                new List<Permission>()
                            ),
                            (
                                CoOwner,
                                "",
                                new List<Permission>()
                            ),
                            (
                                Reviewer,
                                "",
                                new List<Permission>()
                            ),                           
                        };
    }
}
