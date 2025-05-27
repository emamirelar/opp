namespace UNOPS.PAO.UNOPSPresentation.Helpers;
public class APIDictionary
{
    public const string APIPrefix = "/api/";
    public const string opsAPIPrefix = "/api/unops/";

    // Project
    public const string Project = APIPrefix + "project";

    // Document
    public const string Document = APIPrefix + "document";
    public const string DocumentUpload = Document + "/upload";
    public const string DocumentLink = Document + "/link";

    // User Management
    public const string UserManagement = APIPrefix + "user-management";
    public const string UserManagementUsers = UserManagement + "/users";
    public const string UserManagementRoles = UserManagement + "/roles";
    public const string UserManagementCurrentUserOrgUnit = UserManagement + "/current-user-org-unit";
}