namespace UNOPS.PAO.Presentation.Helpers;
public class APIDictionary
{
    public const string APIPrefix = "/api/";
    public const string ExternalAPIPrefix = APIPrefix + "external/";

    // Configuration
    public const string Configuration = APIPrefix + "configuration";

    // Document
    public const string Document = APIPrefix + "document";

    // FundingOpportunity
    public const string FundingOpportunity = APIPrefix + "funding-opportunity";
    public const string ExternalFundingOpportunity = ExternalAPIPrefix + "funding-opportunity";

    // Profile
    public const string Profile = APIPrefix + "profile";
    public const string ExternalProfile = ExternalAPIPrefix + "profile";

    // Proposal
    public const string Proposal = APIPrefix + "proposal";
    public const string FundingOpportunityProposal = FundingOpportunity + "/{opportunityId}/proposal";
    public const string ExternalProposal = ExternalAPIPrefix + "proposal";

    // System Admin
    public const string SystemAdmin = APIPrefix + "system-admin";

    // Values
    public const string Currency = APIPrefix + "values/currency";
    public const string SelectionMethodology = APIPrefix + "values/selection-methodology";
    public const string EligibleEntity = APIPrefix + "values/eligible-entity";
    public const string ApplicationType = APIPrefix + "values/application-type";
    public const string SDG = APIPrefix + "values/sdg";
    public const string Country = APIPrefix + "values/country";

    // Workflow
    public const string Workflow = APIPrefix + "workflow";

}
