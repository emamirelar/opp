namespace UNOPS.PAO.Presentation.Helpers;
public class APIDictionary
{
    public const string APIPrefix = "/api/";
    public const string ExternalAPIPrefix = APIPrefix + "external/";

    // Configuration
    public const string Configuration = APIPrefix + "configuration";

    // Document
    public const string Document = APIPrefix + "document";
    public const string DocumentType = APIPrefix + "document-type";
    public const string DocumentGenerate = Document + "/generate-document";
    public const string DocumentUpload = Document + "/upload";
    public const string DocumentLink = Document + "/link";
    public const string DocumentViewUrl = Document + "/view-url";
    public const string DocumentDownload = Document + "/download";

    // Profile
    public const string Profile = APIPrefix + "profile";
    public const string ExternalProfile = ExternalAPIPrefix + "profile";

    // System Admin
    public const string SystemAdmin = APIPrefix + "system-admin";

    // Values
    public const string Currency = APIPrefix + "values/currency";
    public const string SelectionMethodology = APIPrefix + "values/selection-methodology";
    public const string EligibleEntity = APIPrefix + "values/eligible-entity";
    public const string ApplicationType = APIPrefix + "values/application-type";
    public const string SDG = APIPrefix + "values/sdg";
    public const string SDGs = APIPrefix + "values/sdgs";
    public const string SDGTargets = APIPrefix + "values/sdg-targets";
    public const string SDGIndicators = APIPrefix + "values/sdg-indicators";
    public const string UNCFOutcomes = APIPrefix + "values/uncf-outcomes";
    public const string UNCFIndicators = APIPrefix + "values/uncf-indicators";
    public const string UNOPSMissions = APIPrefix + "values/unops-missions";
    public const string Country = APIPrefix + "values/country";
    public const string Partners = APIPrefix + "values/partners";
    public const string EntityRoles = APIPrefix + "values/entity-roles";
    public const string InternalUsers = APIPrefix + "values/internal-users";
    public const string OrganizationUnits = APIPrefix + "values/organization-units";
    public const string PartnerCategories = APIPrefix + "values/partner-categories";
    public const string LiaisonOffices = APIPrefix + "values/liaison-offices";
    public const string Contacts = APIPrefix + "values/contacts";
    public const string Users = APIPrefix + "values/users";
    public const string GeminiModels = APIPrefix + "values/gemini-models";
    public const string ProposedInitiativeTypes = APIPrefix + "values/proposed-initiative-types";
    public const string Outputs = APIPrefix + "values/outputs";

    // Workflow
    public const string Workflow = APIPrefix + "workflow";

    // Contact
    public const string Contact = APIPrefix + "contact";
    public const string ExternalContact = ExternalAPIPrefix + "contact";

    // Interaction
    public const string Interaction = APIPrefix + "interactions";
    public const string SingularInteraction = APIPrefix + "interaction";
    public const string InteractionsBrief = APIPrefix + "interactions-brief";

    // Partner Tree
    
    public const string PartnerTree = APIPrefix + "partner-tree";
    
    // Engagement
    public const string Engagement = APIPrefix + "engagement";
    public const string ExternalPartnerTree = ExternalAPIPrefix + "partner-tree";

    // Partner
    public const string Partner = APIPrefix + "partner";
    public const string PartnerContacts = Partner + "/{partnerId}/contacts";
    
    // Partner Category
    public const string PartnerCategory = APIPrefix + "partnercategory";
    
    // Partner Group
    public const string PartnerGroup = APIPrefix + "partnergroup";
    
    // Partner Analytics
    public const string PartnerAnalyticsMostActive = Partner + "/analytics/mostActive";
    public const string PartnerAnalyticsByUser = Partner + "/analytics/byUser";
    public const string PartnerAnalyticsEngagementTrends = Partner + "/analytics/engagementTrends";
    public const string PartnerAnalyticsByCountry = Partner + "/analytics/byCountry";

    // Contact Analytics
    public const string ContactAnalytics = APIPrefix + "contact-analytics";
    public const string ContactAnalyticsMostActive = ContactAnalytics + "/getMostActiveContacts";
    public const string ContactAnalyticsByGeographicRegion = ContactAnalytics + "/getContactsByGeographicRegion";
    public const string ContactAnalyticsEngagementTrends = ContactAnalytics + "/getContactEngagementTrends";
    public const string ContactAnalyticsByInteractionType = ContactAnalytics + "/getContactsByInteractionType";
    public const string ContactAnalyticsByPartner = ContactAnalytics + "/getContactsByPartner";
    public const string ContactAnalyticsRecentlyActive = ContactAnalytics + "/getRecentlyActiveContacts";
    public const string ContactAnalyticsByJobTitle = ContactAnalytics + "/getContactsByJobTitle";
    public const string ContactAnalyticsGrowthTrends = ContactAnalytics + "/getContactGrowthTrends";
    public const string ContactAnalyticsWithMostDocuments = ContactAnalytics + "/getContactsWithMostDocuments";

    public const string OrganizationHierarchy = APIPrefix + "organization-hierarchy";

    public const string GeminiProcessDataSummary = APIPrefix + "process-data";
    public const string GeminiDocumentTranscribe = APIPrefix + "document-transcribe";
    public const string AuditLog = APIPrefix + "auditlog";
    public const string AuditLogLatest = APIPrefix + "auditlog/latest";
    public const string AiAssistantCreateSession = APIPrefix + "ai-assistant/create-session";
    public const string AiAssistantGetSession = APIPrefix + "ai-assistant/get-session";
    public const string AiAssistantGetUserSessions = APIPrefix + "ai-assistant/get-user-sessions";
    public const string AiAssistantEndSession = APIPrefix + "ai-assistant/end-session";
    public const string AiAssistantChat = APIPrefix + "ai-assistant/chat";
    public const string GeminiFileScan = APIPrefix + "scan-data";
    public const string AiAssistantAccessibility = APIPrefix + "ai-assistant/accessibility";
    public const string AiAssistantUpdateStar = APIPrefix + "ai-assistant/update-star";
    public const string AiAssistantUpdateArchive = APIPrefix + "ai-assistant/update-archive";
        public const string AiAssistantUpdateTitle = APIPrefix + "ai-assistant/update-title";
        public const string GenerateEmbeddings = APIPrefix + "generate-embeddings";

    public const string Link = APIPrefix + "links";

    public const string Notifications = "api/notifications";
    public const string NotificationRead = "api/notifications/{notificationId}/read";

    // User Data
    public const string CurrentUserData = APIPrefix + "current-user-data";

    //Gmail Addon
    public const string GmailAddonInteraction = "api/gmail-addon/interactions";
    public const string GmailAddonFindInteraction = "api/gmail-addon/interactions/find";
    public const string GmailAddonFindRelatedRecords = "api/gmail-addon/interactions/find-related-records";
    public const string GmailAddonCreateRecords = "api/gmail-addon/create-records";
    //public const string GmailAddonAuth = "api/gmail-addon/auth";
    //public const string GmailAddonRefresh = "api/gmail-addon/refresh";
    //public const string GmailAddonRevoke = "api/gmail-addon/revoke";

    public const string UserInfo = APIPrefix + "user-info/by-email";
    public const string CurrentUserInfo = APIPrefix + "user-info/current";

    public const string UserInfoUpdate = APIPrefix + "user-info/update";

    // AI Prompts
    public const string AiPrompts = APIPrefix + "ai-prompt-management";
    public const string AiPromptsTypes = AiPrompts + "/types";
    public const string AiPromptsModels = AiPrompts + "/models";
    public const string AiPromptsProjects = AiPrompts + "/projects";
    public const string AiPromptsLocations = AiPrompts + "/locations";
    public const string AiPromptsByType = AiPrompts + "/type";
    public const string AiPromptsList = AiPrompts + "/list";
    public const string AiPromptsTest = AiPrompts + "/test";
    public const string AiPromptsUpgradeModel = AiPrompts + "/upgrade-model";

    // Entity Configuration Management
    public const string EntityList = APIPrefix + "entities";
    public const string EntityConfiguration = APIPrefix + "entity-configuration";
    public const string EntityConfigurationCreate = EntityConfiguration + "/create";
    public const string EntityField = APIPrefix + "entity-field";
    public const string EntityFieldCreate = EntityField + "/create";

    // User Management
    public const string UserManagement = APIPrefix + "user-management";
    public const string UserManagementUsers = UserManagement + "/users";
    public const string UserManagementRoles = UserManagement + "/roles";
    public const string UserManagementOrgUnits = UserManagement + "/org-units";
    public const string UserManagementCurrentUserOrgUnit = UserManagement + "/current-user-org-unit";

    // Global Filters and User Preferences
    public const string Global = APIPrefix + "global";
    public const string GlobalUserPreferences = Global + "/user-preferences";
    public const string GlobalFilters = Global + "/filters";
    public const string GlobalFiltersReset = GlobalFilters + "/reset";
    public const string GlobalSearch = Global + "/search";
    public const string PreferredLanguage = Global + "/preferred-language";


    // Dashboard
    public const string Dashboard = APIPrefix + "dashboard";
    public const string DashboardMyPartners = Dashboard + "/my-partners";
    public const string DashboardMyContacts = Dashboard + "/my-contacts";
    public const string DashboardMyInteractions = Dashboard + "/my-interactions";
    public const string DashboardMyOpportunities = Dashboard + "/my-opportunities";
    public const string DashboardMyDraftPartners = Dashboard + "/my-draft-partners";
    public const string DashboardMyDraftContacts = Dashboard + "/my-draft-contacts";
    public const string DashboardMyDraftInteractions = Dashboard + "/my-draft-interactions";
    public const string DashboardMyDraftOpportunities = Dashboard + "/my-draft-opportunities";
    public const string DashboardOrgUnitRecentUpdates = Dashboard + "/org-unit-recent-updates";

    // Opportunity
    public const string Opportunity = APIPrefix + "opportunity";
    public const string OpportunityOverview = Opportunity + "/{id}/overview";
    public const string OpportunityWhat = Opportunity + "/{id}/what";
    public const string OpportunityWhy = Opportunity + "/{id}/why";
    public const string OpportunityWho = Opportunity + "/{id}/who";
    public const string OpportunityTeam = Opportunity + "/{id}/team";
    public const string OpportunityWhere = Opportunity + "/{id}/where";
    public const string OpportunityWhen = Opportunity + "/{id}/when";
    public const string OpportunityRelated = Opportunity + "/{id}/related";
    public const string OpportunityApplyAiChanges = Opportunity + "/{id}/apply-ai-changes";
    public const string OpportunityGenerateStatement = Opportunity + "/{id}/generate-statement";
    public const string OpportunityValidateStatement = Opportunity + "/{id}/validate-statement";

    // Comment
    public const string Comment = APIPrefix + "comment";
    public const string CommentsByEntity = Comment + "/{entityType}/{entityId}";
    public const string CommentTogglePin = Comment + "/{id}/toggle-pin";
    public const string CommentCount = Comment + "/{entityType}/{entityId}/count";

    // Entity Artifacts
    public const string EntityArtifacts = APIPrefix + "entity-artifacts";
    public const string EntityArtifactEntityTypes = EntityArtifacts + "/entity-types";
    public const string EntityArtifactTypes = EntityArtifacts + "/artifact-types";
    public const string EntityArtifactRecords = EntityArtifacts + "/entity-records";
    public const string EntityArtifactGet = EntityArtifacts + "/get";
    public const string EntityArtifactUpsert = EntityArtifacts + "/upsert";
    public const string EntityArtifactList = EntityArtifacts + "/list";
    
    // Bulk Entity Artifacts
    public const string EntityArtifactBulkArtifactTypes = EntityArtifacts + "/bulk/artifact-types";
    public const string EntityArtifactBulkUniqueIdExample = EntityArtifacts + "/bulk/unique-id-example";
    public const string EntityArtifactBulkTemplateDownload = EntityArtifacts + "/bulk/template-download";
    public const string EntityArtifactBulkUpsert = EntityArtifacts + "/bulk/upsert";

    // AI Retriever Service (External API)
    public static class AIRetriever
    {
        private const string BaseRoute = APIPrefix + "ai-retriever";
        
        // Vector Store
        public const string VectorStoreSearch = BaseRoute + "/vector-store/search";
        
        // Document Conversion
        public const string ConvertUrl = BaseRoute + "/convert/url";
        public const string ConvertMarkdownToGoogleDoc = BaseRoute + "/convert/markdown-to-google-doc";
        
        // Health Check
        public const string Health = BaseRoute + "/health";
    }
}
