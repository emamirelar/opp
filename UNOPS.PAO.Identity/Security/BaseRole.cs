namespace UNOPS.PAO.Identity.Security;

public class BaseRole
{
    public const string SystemAdmin = "SystemAdmin";
    public const string GrantAuthority = "GrantAuthority";
    public const string GrantOfficial = "GrantOfficial";
    public const string Applicant = "Applicant";
    public const string IP = "ImplementingPartner";
    public const string Evaluator = "Evaluator";
    public const string ProjectManager = "Project Manager";
    public const string DeputyProjectManager = "Deputy Project Manager";
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
                                GrantAuthority,
                                "The Grant Authority, sometimes referred to as the Delegation of Authority (DOA) holder, serves as the primary approver in the grant process. This role is accountable for ensuring compliance with policies and procedures as outlined in the DOA table. Key tasks include approving Calls for Proposals (CFPs), awards, amendments, commitments, and reports while providing oversight and ensuring alignment with organizational goals.",
                                new List<Permission>()
                            ),
                            (
                                GrantOfficial,
                                "The Grant Official manages the entire grant lifecycle from start to finish. They set up new Funding Opportunities/PAO with applicant criteria, monitor applications through the review phases, manage the selection process, and prepare documentation for approval, such as agreements and commitments. Additionally, they handle agreement management, initiate payments, coordinate reporting, hold meetings with Implementing Partners (IPs), document minutes and decisions, and manage grant closures. This role also initiates workflows requiring approval from the Grant Authority.",
                                new List<Permission>()
                            ),
                            (
                                Applicant,
                                "The Applicant role is for organizations or individuals who log into the portal to submit applications for Funding Opportunities. This role is maintained throughout the evaluation process. Upon successful selection, the Applicant transitions to the Implementing Partner (IP) role.",
                                new List<Permission>()
                            ),
                            (
                                IP,
                                "The Implementing Partner (IP) role is assigned to Applicants selected as Implementing Partners of a Funding Opportunity. IPs engage with the organization under legal terms, manage project execution, and ensure deliverables meet the agreed objectives.",
                                new List<Permission>()
                            ),
                            (
                                Evaluator,
                                "The Evaluator role allows individuals to review, score, and evaluate all applications submitted for a specific Funding Opportunity. Evaluators play a critical role in the selection process by providing impartial and systematic assessments of submissions based on predefined criteria.",
                                new List<Permission>()
                            ),
                            (
                                ProjectManager,
                                "",
                                new List<Permission>()
                            ),
                            (
                                DeputyProjectManager,
                                "",
                                new List<Permission>()
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
