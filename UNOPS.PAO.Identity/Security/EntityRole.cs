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
            // Project
            new EntityRole(RoleEntityNames.Project, BaseRole.ProjectManager, 1),
            new EntityRole(RoleEntityNames.Project, BaseRole.DeputyProjectManager, 1),
            new EntityRole(RoleEntityNames.Project, BaseRole.Contributor),
            new EntityRole(RoleEntityNames.Project, BaseRole.CoOwner),
            new EntityRole(RoleEntityNames.Project, BaseRole.Reviewer),

            // Funding Opportunity
            new EntityRole(RoleEntityNames.FundingOpportunity, BaseRole.GrantAuthority, 1),
            new EntityRole(RoleEntityNames.FundingOpportunity, BaseRole.GrantOfficial, 1),
            new EntityRole(RoleEntityNames.FundingOpportunity, BaseRole.IP, 1),
            new EntityRole(RoleEntityNames.FundingOpportunity, BaseRole.Applicant),
            new EntityRole(RoleEntityNames.FundingOpportunity, BaseRole.Evaluator),

            // Proposal
        };

        return entityRoles.Where(x => x.Entity == entity).ToList();
    }
}
