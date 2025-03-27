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
    public const string Partners = APIPrefix + "values/partners";

    // Workflow
    public const string Workflow = APIPrefix + "workflow";

    // Contact
    public const string Contact = APIPrefix + "contact";
    public const string ExternalContact = ExternalAPIPrefix + "contact";

    // Interaction
    public const string Interaction = APIPrefix + "interactions";

    // Partner Tree
    
    public const string PartnerTree = APIPrefix + "partner-tree";
    public const string ExternalPartnerTree = ExternalAPIPrefix + "partner-tree";

    // Partner
    public const string Partner = APIPrefix + "partner";
    public const string PartnerContacts = Partner + "/{partnerId}/contacts";

    public const string GeminiProcessDataSummary = APIPrefix + "process-data";
    public const string AiAssistantCreateSession = APIPrefix + "ai-assistant/create-session";
    public const string AiAssistantGetSession = APIPrefix + "ai-assistant/get-session";
    public const string AiAssistantGetUserSessions = APIPrefix + "ai-assistant/get-user-sessions";
    public const string AiAssistantEndSession = APIPrefix + "ai-assistant/end-session";
    public const string AiAssistantChat = APIPrefix + "ai-assistant/chat";
    public const string GeminiFileScan = APIPrefix + "scan-data";
    public const string AiAssistantAccessibility = APIPrefix + "ai-assistant/accessibility";

    public const string Link = APIPrefix + "links";
}
