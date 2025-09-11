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
    public const string Country = APIPrefix + "values/country";
    public const string Partners = APIPrefix + "values/partners";
    public const string OrganizationUnits = APIPrefix + "values/organization-units";
    public const string PartnerCategories = APIPrefix + "values/partner-categories";
    public const string LiaisonOffices = APIPrefix + "values/liaison-offices";
    public const string Contacts = APIPrefix + "values/contacts";
    public const string Users = APIPrefix + "values/users";
    public const string GeminiModels = APIPrefix + "values/gemini-models";

    // Workflow
    public const string Workflow = APIPrefix + "workflow";

    // Contact
    public const string Contact = APIPrefix + "contact";
    public const string ExternalContact = ExternalAPIPrefix + "contact";

    // Interaction
    public const string Interaction = APIPrefix + "interactions";

    // Partner Tree
    
    public const string PartnerTree = APIPrefix + "partner-tree";
    
    // Engagement
    public const string Engagement = APIPrefix + "engagement";
    public const string ExternalPartnerTree = ExternalAPIPrefix + "partner-tree";

    // Partner
    public const string Partner = APIPrefix + "partner";
    public const string PartnerContacts = Partner + "/{partnerId}/contacts";
    
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

    public const string AiAssistantGenerateTitle = APIPrefix + "ai-assistant/generate-title";
    public const string AiAssistantGenerateSuggestions = APIPrefix + "ai-assistant/generate-suggestions";

    // Dashboard
    public const string Dashboard = APIPrefix + "dashboard";
    public const string DashboardMyPartners = Dashboard + "/my-partners";
    public const string DashboardMyContacts = Dashboard + "/my-contacts";
    public const string DashboardMyInteractions = Dashboard + "/my-interactions";
    public const string DashboardMyDraftPartners = Dashboard + "/my-draft-partners";
    public const string DashboardMyDraftContacts = Dashboard + "/my-draft-contacts";
    public const string DashboardMyDraftInteractions = Dashboard + "/my-draft-interactions";
    public const string DashboardOrgUnitRecentUpdates = Dashboard + "/org-unit-recent-updates";
}
