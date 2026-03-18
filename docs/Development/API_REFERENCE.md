# API Reference — UNOPS Opportunity+

Comprehensive reference for all REST API endpoints in the system. Base URL: `/api/`.

Authentication: All endpoints require IAP authentication (`[Authorize(AuthenticationSchemes = "IAP")]`) unless otherwise noted. Some endpoints require additional permissions via `[PermissionAuthorize]`.

---

## Table of Contents

- [Opportunities](#opportunities)
- [Risk (Opportunity DST)](#risk-opportunity-dst)
- [Partners](#partners)
- [Partner Analytics](#partner-analytics)
- [Partner Tree](#partner-tree)
- [Partner Categories](#partner-categories)
- [Partner Groups](#partner-groups)
- [Contacts](#contacts)
- [Contact Analytics](#contact-analytics)
- [Interactions](#interactions)
- [Documents](#documents)
- [Document Types](#document-types)
- [Workflow](#workflow)
- [Dashboard](#dashboard)
- [Links](#links)
- [Comments](#comments)
- [Notifications](#notifications)
- [Audit Log](#audit-log)
- [Configuration](#configuration)
- [Values (Lookups)](#values-lookups)
- [Global Search and Preferences](#global-search-and-preferences)
- [User Preferences](#user-preferences)
- [User Profile](#user-profile)
- [User Management (Admin)](#user-management-admin)
- [Permissions](#permissions)
- [Saved Filters](#saved-filters)
- [Organization Hierarchy](#organization-hierarchy)
- [Entity Configuration (Admin)](#entity-configuration-admin)
- [Entity Artifacts (Admin)](#entity-artifacts-admin)
- [System Admin](#system-admin)
- [AI / Gemini](#ai--gemini)
- [AI Retriever](#ai-retriever)
- [Base Engagements](#base-engagements)
- [Roles](#roles)
- [Liaison Offices](#liaison-offices)
- [Countries](#countries)
- [Gmail Addon](#gmail-addon)
- [Development (Dev Only)](#development-dev-only)

---

## Opportunities

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `/api/opportunity` | Create | Create opportunity |
| GET | `/api/opportunity` | GetAllOpportunities | List opportunities (paginated) |
| GET | `/api/opportunity/{id}` | Get | Get opportunity by ID |
| PUT | `/api/opportunity/{id}` | Update | Update opportunity |
| DELETE | `/api/opportunity/{id}` | Delete | Delete opportunity |
| GET | `/api/opportunity/search` | SearchOpportunities | Text search |
| GET | `/api/opportunity/advanced-search` | AdvancedSearchOpportunities | Advanced search |
| GET | `/api/opportunity/search-fields` | GetOpportunitySearchFields | Supported search fields |
| PATCH | `/api/opportunity/{id}/overview` | UpdateOverviewSection | Update overview section |
| PATCH | `/api/opportunity/{id}/what` | UpdateWhatSection | Update WHAT section |
| PATCH | `/api/opportunity/{id}/why` | UpdateWhySection | Update WHY section |
| PATCH | `/api/opportunity/{id}/who` | UpdateWhoSection | Update WHO section |
| PATCH | `/api/opportunity/{id}/team` | UpdateTeamSection | Update team section |
| PATCH | `/api/opportunity/{id}/where` | UpdateWhereSection | Update WHERE section |
| PATCH | `/api/opportunity/{id}/when` | UpdateWhenSection | Update WHEN section |
| PATCH | `/api/opportunity/{id}/apply-ai-changes` | ApplyAiChanges | Apply AI-suggested changes |
| GET | `/api/opportunity/{id}/related` | GetRelatedItems | Related items |
| GET | `/api/opportunity/{id}/source-interactions` | GetSourceInteractions | Source interactions |
| GET | `/api/opportunity/{id}/executives` | GetExecutives | Executives for Go Decision |
| GET | `/api/opportunity/{id}/high-risk-analysis` | GetHighRiskAnalysis | High risk analysis |
| GET | `/api/opportunity/{id}/insights` | GetInsights | AI insights |
| GET | `/api/opportunity/{id}/framework-status` | GetFrameworkStatus | Partner Results Framework status |
| GET | `/api/opportunity/{id}/similar-opportunities` | GetSimilarOpportunities | Similar opportunities |
| GET | `/api/opportunity/{id}/similar-projects` | GetSimilarProjects | Similar projects |
| GET | `/api/opportunity/{id}/relevant-people` | GetRelevantPeople | Relevant people |
| POST | `/api/opportunity/{id}/dst-recommendations` | GetDSTRecommendations | DST recommendations |
| GET | `/api/opportunity/{id}/dst-risks` | GetDSTRisks | DST risks |
| POST | `/api/opportunity/{id}/dst-risks` | AddDSTRisk | Add DST risk |
| PUT | `/api/opportunity/{id}/dst-risks/{riskId}` | UpdateDSTRisk | Update DST risk |
| DELETE | `/api/opportunity/{id}/dst-risks/{riskId}` | DeleteDSTRisk | Delete DST risk |
| PUT | `/api/opportunity/{id}/acknowledge-high-risks` | AcknowledgeHighRisks | Acknowledge high risks |
| POST | `/api/opportunity/{id}/generate-images` | GenerateOpportunityImages | Generate AI banner/thumbnail |
| POST | `/api/opportunity/{id}/generate-statement` | GenerateStatement | Generate statement |
| POST | `/api/opportunity/{id}/validate-statement` | ValidateStatement | Validate statement |
| POST | `/api/opportunity/generate-statement-pdf` | GenerateStatementPdf | Generate statement PDF |
| POST | `/api/opportunity/generate-proposal` | GenerateOpportunityProposal | Generate AI proposal |
| POST | `/api/opportunity/create-from-proposal` | CreateOpportunityFromProposal | Create from AI proposal |
| POST | `/api/opportunity/find-deliverable` | FindDeliverable | AI deliverable search |
| POST | `/api/opportunity/{id}/extract-deliverables` | ExtractDeliverablesFromSources | Extract deliverables |
| GET | `/api/opportunity/retrieve-partner-document-association/{documentId}` | RetrievePartnerDocumentAssociation | Partner-document associations |
| POST | `/api/opportunity/{opportunityId}/tag-related-partner-to-doc` | TagDocumentToPartners | Tag document to partners |
| GET | `/api/opportunity/collaborator-expertises` | GetCollaboratorExpertises | Collaborator expertises |

---

## Risk (Opportunity DST)

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/risk/lookups` | GetRiskLookups | Risk lookups |
| GET | `/api/risk/categories` | GetRiskCategories | Risk categories |
| GET | `/api/risk/high-risk-checklist` | GetHighRiskChecklist | High risk checklist |

---

## Partners

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `/api/partner` | Create | Create partner |
| GET | `/api/partner` | ListAllPartners | List partners (paginated) |
| GET | `/api/partner/{id}` | Get | Get partner by ID |
| PUT | `/api/partner` | Update | Update partner |
| DELETE | `/api/partner/{id}` | Delete | Delete partner |
| GET | `/api/partner/search` | SearchPartners | Search partners |
| GET | `/api/partner/advanced-search` | AdvancedSearchPartners | Advanced search |
| GET | `/api/partner/search-fields` | GetPartnerSearchFields | Search fields |
| GET | `/api/partner/{id}/permissions` | GetPermissions | Partner permissions |
| GET | `/api/partner/{id}/interactions` | GetInteractions | Partner interactions |
| GET | `/api/partner/{partnerId}/opportunities` | GetOpportunities | Partner opportunities |
| GET | `/api/partner/{partnerId}/opportunities/search` | SearchOpportunities | Search partner opportunities |
| POST | `/api/partner/{partnerId}/create-opportunity` | CreateOpportunity | Create opportunity from partner |
| POST | `/api/partner/{id}/activate` | Activate | Activate partner |
| POST | `/api/partner/{id}/close` | Close | Close partner |
| POST | `/api/partner/{id}/archive` | Archive | Archive partner |
| POST | `/api/partner/{id}/approve` | Approve | Approve partner |
| POST | `/api/partner/{id}/unapprove` | Unapprove | Unapprove partner |
| POST | `/api/partner/{id}/logo` | UploadLogo | Upload logo |
| GET | `/api/partner/by-partner-group-id/{id}` | GetByPartnerGroupId | Partners by group |
| GET | `/api/partner/by-partner-category-code/{code}` | GetByPartnerCategoryCode | Partners by category |
| GET | `/api/partner/categories-summary` | GetCategoriesSummary | Categories summary |
| GET | `/api/partner/groups-summary` | GetGroupsSummary | Groups summary |
| GET | `/api/partner/categorization-overview` | GetCategorizationOverview | Categorization overview |
| GET | `/api/partner/metadata-info` | GetMetadataInfo | Metadata info |
| POST | `/api/partner/scan-data` | ScanData | Scan import data |
| POST | `/api/partner/analyse-file` | AnalyseFile | Analyse import file |
| POST | `/api/partner/bulk-upload` | BulkUpload | Bulk upload |
| POST | `/api/partner/detect-duplicates` | DetectDuplicates | Detect duplicates |

---

## Partner Analytics

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/partner/analytics/mostActive` | GetMostActivePartners | Most active partners |
| GET | `/api/partner/analytics/byUser/{userId}` | GetPartnersByUser | Partners by user |
| GET | `/api/partner/analytics/engagementTrends` | GetEngagementTrends | Engagement trends |
| GET | `/api/partner/analytics/byCountry` | GetPartnersByCountry | Partners by country |

---

## Partner Tree

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `/api/partner-tree` | Create | Create partner tree node |
| GET | `/api/partner-tree` | GetAll | List partner tree nodes |
| GET | `/api/partner-tree/{id}` | Get | Get node by ID |
| PUT | `/api/partner-tree` | Update | Update node |
| DELETE | `/api/partner-tree/{id}` | Delete | Delete node |
| GET | `/api/partner-tree/{id}/permissions` | GetPermissions | Node permissions |
| GET | `/api/partner-tree-structure` | GetStructure | Full tree structure |
| GET | `/api/partner-tree/describe` | Describe | Describe tree |
| GET | `/api/partner-tree/by-partner-group-id/{id}` | GetByPartnerGroupId | By group |
| GET | `/api/partner-tree/by-partner-category-code/{code}` | GetByPartnerCategoryCode | By category |
| GET | `/api/partner-tree/categories-summary` | GetCategoriesSummary | Categories summary |
| GET | `/api/partner-tree/groups-summary` | GetGroupsSummary | Groups summary |
| GET | `/api/partner-tree/categorization-overview` | GetCategorizationOverview | Categorization overview |

---

## Partner Categories

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `api/PartnerCategory` | GetPartnerCategories | List categories |
| POST | `api/PartnerCategory/search` | SearchPartnerCategories | Search categories |
| GET | `api/PartnerCategory/{id}` | GetPartnerCategory | Get by ID |
| GET | `api/PartnerCategory/by-code/{code}` | GetByCode | Get by code |
| GET | `api/PartnerCategory/summary` | GetSummary | Summary |
| GET | `api/PartnerCategory/by-type` | GetPartnerCategoriesByType | By type |
| POST | `api/PartnerCategory/refresh-cache` | RefreshCache | Refresh cache |

---

## Partner Groups

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `api/PartnerGroup` | GetPartnerGroups | List groups |
| POST | `api/PartnerGroup/search` | SearchPartnerGroups | Search groups |
| GET | `api/PartnerGroup/{id}` | GetPartnerGroup | Get by ID |
| GET | `api/PartnerGroup/by-code/{code}` | GetByCode | Get by code |
| GET | `api/PartnerGroup/by-category/{categoryId}` | GetByCategory | By category |
| GET | `api/PartnerGroup/summary` | GetSummary | Summary |
| GET | `api/PartnerGroup/by-category-summary` | GetByCategorySummary | By category summary |
| GET | `api/PartnerGroup/by-type` | GetByType | By type |
| POST | `api/PartnerGroup/refresh-cache` | RefreshCache | Refresh cache |

---

## Contacts

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `/api/contact` | Create | Create contact |
| GET | `/api/contact` | GetAll | List contacts (paginated) |
| GET | `/api/contact/{id}` | Get | Get contact by ID |
| PUT | `/api/contact` | Update | Update contact |
| DELETE | `/api/contact/{id}` | Delete | Delete contact |
| GET | `/api/contact/search` | Search | Search contacts |
| GET | `/api/contact/advanced-search` | AdvancedSearch | Advanced search |
| GET | `/api/contact/search-fields` | GetSearchFields | Search fields |
| GET | `/api/contact/{id}/permissions` | GetPermissions | Contact permissions |
| GET | `/api/partner/{partnerId}/contacts` | GetPartnerContacts | Partner contacts |
| POST | `/api/contact/{id}/profile-picture` | UploadProfilePicture | Upload profile picture |
| GET | `/api/contact/metadata-info` | GetMetadataInfo | Metadata info |
| POST | `/api/contact/scan-data` | ScanData | Scan import data |
| POST | `/api/contact/analyse-file` | AnalyseFile | Analyse import file |
| POST | `/api/contact/bulk-upload` | BulkUpload | Bulk upload |
| POST | `/api/contact/detect-duplicates` | DetectDuplicates | Detect duplicates |

---

## Contact Analytics

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/contact-analytics/getMostActiveContacts` | GetMostActiveContacts | Most active contacts |
| GET | `/api/contact-analytics/getContactsByGeographicRegion` | GetContactsByGeographicRegion | By region |
| GET | `/api/contact-analytics/getContactEngagementTrends` | GetContactEngagementTrends | Engagement trends |
| GET | `/api/contact-analytics/getContactsByInteractionType` | GetContactsByInteractionType | By interaction type |
| GET | `/api/contact-analytics/getContactsByPartner` | GetContactsByPartner | By partner |
| GET | `/api/contact-analytics/getRecentlyActiveContacts` | GetRecentlyActiveContacts | Recently active |
| GET | `/api/contact-analytics/getContactsByJobTitle` | GetContactsByJobTitle | By job title |
| GET | `/api/contact-analytics/getContactGrowthTrends` | GetContactGrowthTrends | Growth trends |
| GET | `/api/contact-analytics/getContactsWithMostDocuments` | GetContactsWithMostDocuments | With most documents |

---

## Interactions

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `/api/interactions` | Create | Create interaction |
| GET | `/api/interactions` | GetAll | List interactions (paginated) |
| GET | `/api/interactions/{id}` | Get | Get interaction by ID |
| PUT | `/api/interactions` | Update | Update interaction |
| DELETE | `/api/interactions/{id}` | Delete | Delete interaction |
| GET | `/api/interactions/search` | Search | Search interactions |
| GET | `/api/interactions/advanced-search` | AdvancedSearch | Advanced search |
| GET | `/api/interaction/search-fields` | GetSearchFields | Search fields |
| GET | `/api/interactions/{id}/permissions` | GetPermissions | Interaction permissions |
| GET | `/api/interactions/deepSearch` | DeepSearch | Deep search |
| GET | `/api/interactions-brief` | GetInteractionsBrief | Brief list |
| GET | `/api/interactions/metadata-info` | GetMetadataInfo | Metadata info |
| POST | `/api/interactions/scan-data` | ScanData | Scan import data |
| POST | `/api/interactions/analyse-file` | AnalyseFile | Analyse import file |
| POST | `/api/interactions/bulk-upload` | BulkUpload | Bulk upload |
| POST | `/api/interactions/detect-duplicates` | DetectDuplicates | Detect duplicates |

---

## Documents

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/document/entity/{entityType}/{entityId}` | GetDocumentsByEntity | Documents by entity |
| GET | `/api/document/{id}` | Get | Get document by ID |
| POST | `/api/document/upload` | Create | Upload document |
| POST | `/api/document/link` | Link | Link external document |
| PUT | `/api/document` | Update | Update document |
| DELETE | `/api/document/{id}` | Delete | Delete document |
| GET | `/api/document/Download/{id}` | Download | Download document |
| GET | `/api/document/view-url/{id}` | GetDocumentViewUrl | Get document view URL |
| POST | `/api/document/generate-document` | GenerateGoogleDoc | Generate Google Doc |

---

## Document Types

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/document-type/{entityName}` | GetAll | Document types by entity |

---

## Workflow

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/workflow/{entityName}` | GetWorkflowStages | Workflow stages for entity |
| GET | `/api/workflow/{entityName}/{id}` | GetWorkflowState | Current workflow state |
| GET | `/api/workflow/{entityName}/{id}/details` | GetWorkflowDetails | Workflow details |
| GET | `/api/workflow/{entityName}/{id}/requirements/{nextStage?}` | GetRequirementsForStageChange | Stage change requirements |
| POST | `/api/workflow/submit` | Submit | Submit for approval |
| POST | `/api/workflow/approve` | Approve | Approve workflow |
| POST | `/api/workflow/reject` | Reject | Reject workflow |
| POST | `/api/workflow/recall` | Recall | Recall submission |
| POST | `/api/workflow/cancel` | Cancel | Cancel workflow |
| POST | `/api/workflow/reopen` | Reopen | Reopen workflow |
| GET | `/api/workflow/{entityName}/{id}/history` | GetWorkflowHistory | Workflow history |
| GET | `/api/workflow/pending-approvals` | GetPendingApprovals | Pending approvals |

---

## Dashboard

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/dashboard/content` | GetDashboardContent | All dashboard content |
| GET | `/api/dashboard/my-partners` | GetMyPartners | My partners |
| GET | `/api/dashboard/my-contacts` | GetMyContacts | My contacts |
| GET | `/api/dashboard/my-interactions` | GetMyInteractions | My interactions |
| GET | `/api/dashboard/my-opportunities` | GetMyOpportunities | My opportunities |
| GET | `/api/dashboard/my-draft-partners` | GetMyDraftPartners | My draft partners |
| GET | `/api/dashboard/my-draft-contacts` | GetMyDraftContacts | My draft contacts |
| GET | `/api/dashboard/my-draft-interactions` | GetMyDraftInteractions | My draft interactions |
| GET | `/api/dashboard/my-draft-opportunities` | GetMyDraftOpportunities | My draft opportunities |
| GET | `/api/dashboard/org-unit-recent-updates` | GetOrgUnitRecentUpdates | Org unit recent updates |

---

## Links

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/links` | GetLinks | Get entity links |
| POST | `/api/links` | Create | Create link |
| PUT | `/api/links` | Update | Update link |
| DELETE | `/api/links` | Delete | Delete link |

---

## Comments

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/comment/{entityType}/{entityId}` | GetCommentsByEntity | Comments by entity |
| GET | `/api/comment/{entityType}/{entityId}/count` | GetCommentCount | Comment count |
| GET | `/api/comment/{id}` | GetCommentById | Get comment by ID |
| POST | `/api/comment` | CreateComment | Create comment |
| PUT | `/api/comment/{id}` | UpdateComment | Update comment |
| DELETE | `/api/comment/{id}` | DeleteComment | Delete comment |
| POST | `/api/comment/{id}/toggle-pin` | TogglePin | Toggle pin status |

---

## Notifications

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `api/notifications` | GetNotifications | List notifications |
| PUT | `api/notifications/{notificationId}/read` | MarkAsRead | Mark as read |
| PUT | `api/notifications/{notificationId}/update` | UpdateNotification | Update notification |

---

## Audit Log

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/auditlog/latest` | GetLatestAuditLog | Latest audit log for entity |

---

## Configuration

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/configuration` | Get | Application configuration |

---

## Values (Lookups)

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/values/config` | GetConfig | Config values |
| GET | `/api/values/currency` | GetCurrencies | Currencies |
| GET | `/api/values/eligible-entity` | GetEligibleEntities | Eligible entities |
| GET | `/api/values/country` | GetCountries | Countries |
| GET | `/api/values/application-type` | GetApplicationTypes | Application types |
| GET | `/api/values/partners` | GetPartners | Partners (for filtering) |
| GET | `/api/values/organization-units` | GetOrganizationUnits | Organization units |
| GET | `/api/values/opportunity-organization-units` | GetOpportunityOrgUnits | Opportunity org units |
| GET | `/api/values/liaison-offices` | GetLiaisonOffices | Liaison offices |
| GET | `/api/values/contacts` | GetContacts | Contacts |
| GET | `/api/values/users` | GetUsers | Users |
| POST | `/api/values/users/paged` | GetUsersPaged | Users (paged) |
| GET | `/api/values/users/search` | SearchUsers | Search users |
| GET | `/api/values/proposed-initiative-types` | GetProposedInitiativeTypes | Proposed initiative types |
| GET | `/api/values/outputs` | GetOutputs | Outputs |
| GET | `/api/values/sdgs` | GetSDGs | SDGs |
| GET | `/api/values/sdg-targets` | GetSDGTargets | SDG targets |
| GET | `/api/values/sdg-indicators` | GetSDGIndicators | SDG indicators |
| GET | `/api/values/uncf-outcomes` | GetUNCFOutcomes | UNCF outcomes |
| GET | `/api/values/uncf-indicators` | GetUNCFIndicators | UNCF indicators |
| GET | `/api/values/unops-missions` | GetUNOPSMissions | UNOPS missions |
| GET | `/api/values/gemini-models` | GetGeminiModels | Gemini models |
| GET | `/api/values/entity-roles/{entityType}` | GetEntityRoles | Entity roles |
| GET | `/api/values/internal-users` | GetInternalUsers | Internal users |
| GET | `/api/values/suggested-org-units` | GetSuggestedOrgUnits | Suggested org units |
| POST | `/api/values/entity-user-roles-by-org-unit` | GetEntityUserRolesByOrgUnits | Entity user roles by org unit |
| POST | `/api/values/org-unit-ids-for-countries` | GetOrgUnitIdsForCountries | Org unit IDs for countries |
| POST | `/api/values/child-org-unit-ids-for-hub-region/{parentOrgUnitId}` | GetChildOrgUnitIdsForHubRegion | Child org units for hub/region |

---

## Global Search and Preferences

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/global/search` | GlobalSearch | Global search |
| GET | `/api/global/user-preferences` | GetUserPreferences | User preferences |
| PUT | `/api/global/user-preferences` | UpdateUserPreferences | Update preferences |
| GET | `/api/global/filters` | GetGlobalFilters | Global filters |
| PUT | `/api/global/filters` | UpdateGlobalFilters | Update filters |
| POST | `/api/global/filters/reset` | ResetGlobalFilters | Reset filters |
| GET | `/api/global/preferred-language` | GetPreferredLanguage | Preferred language |
| PUT | `/api/global/preferred-language` | UpdatePreferredLanguage | Update preferred language |

---

## User Preferences

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `api/user-preferences/default-org-unit` | GetDefaultOrgUnit | Default org unit |
| PUT | `api/user-preferences/default-org-unit` | SetDefaultOrgUnit | Set default org unit |

---

## User Profile

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `/api/profile` | UpdateProfile | Update profile |
| PUT | `/api/user-info/update` | UpdateUserInfo | Update user info |
| GET | `/api/user-info/current` | GetUserProfileDetails | Current user profile |

---

## User Management (Admin)

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `/api/user-management/users` | GetUsers | List users (paginated) |
| GET | `/api/user-management/users/{userId}` | GetUser | Get user by ID |
| PUT | `/api/user-management/users/{userId}/roles` | UpdateUserRoles | Update user roles |
| GET | `/api/user-management/roles` | GetAvailableRoles | Available roles |
| GET | `/api/user-management/org-units` | GetAvailableOrgUnits | Organization units |
| GET | `/api/user-management/current-user-org-unit` | GetCurrentUserOrgUnit | Current user org unit |
| GET | `/api/user-management/org-units/{orgUnitCode}/self-management` | GetOrgUnitSelfManagement | Org unit self-management |
| PUT | `/api/user-management/org-units/{orgUnitCode}/self-management` | UpdateOrgUnitSelfManagement | Update self-management |
| POST | `/api/user-management/analyse-file` | AnalyzeUserRoleFile | Analyse user role file |
| POST | `/api/user-management/bulk-upload` | BulkUploadUserRoles | Bulk upload user roles |
| POST | `/api/user-management/resolve-users` | ResolveUsers | Resolve users |
| POST | `/api/user-management/resolve-roles` | ResolveRoles | Resolve roles |

---

## Permissions

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `api/permissions` | GetSystemPermissionConfiguration | System permission config |
| GET | `api/permissions/check/{*route}` | CheckUserRoutePermission | Route permission check |
| GET | `api/permissions/entity-permissions/{entityName}` | GetEntityPermissionDetails | Entity permissions |
| GET | `api/permissions/user-roles` | GetCurrentUserRoles | Current user roles |
| GET | `api/permissions/user/{userId}` | GetUserRolesByUserId | User roles by ID |
| GET | `api/permissions/available-roles` | GetAvailableSystemRoles | Available system roles |

---

## Saved Filters

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `api/SavedFilter` | CreateSavedFilter | Create saved filter |
| PUT | `api/SavedFilter` | UpdateSavedFilter | Update saved filter |
| DELETE | `api/SavedFilter/{id}` | DeleteSavedFilter | Delete saved filter |
| GET | `api/SavedFilter/{id}` | GetSavedFilter | Get saved filter |
| GET | `api/SavedFilter` | GetSavedFilters | List saved filters |
| GET | `api/SavedFilter/{id}/apply` | ApplySavedFilter | Apply saved filter |
| GET | `api/SavedFilter/statistics` | GetFilterStatistics | Filter statistics |

---

## Organization Hierarchy

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/organization-hierarchy` | GetOrganizationHierarchy | Organization hierarchy |
| GET | `/api/organization-hierarchy/legacy` | GetOrganizationHierarchyLegacy | Legacy hierarchy |
| GET | `/api/organization-hierarchy/{id}` | GetOrganizationHierarchyById | Get by ID |
| GET | `/api/organization-hierarchy/metadata-info` | GetMetadataInfo | Metadata info |
| GET | `api/organizationhierarchy` | GetOrganizationHierarchy (legacy) | Legacy endpoint |
| POST | `api/organizationhierarchy/search` | Search | Search hierarchy |
| GET | `api/organizationhierarchy/{id}` | GetById (legacy) | Legacy get by ID |

---

## Entity Configuration (Admin)

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/entities` | GetEntities | List entities |
| GET | `/api/entity-configuration` | GetAllEntityConfigurations | All entity configurations |
| GET | `/api/entity-configuration/{entityName}` | GetEntityConfiguration | Entity configuration |
| POST | `/api/entity-configuration/create` | CreateEntityConfiguration | Create configuration |
| POST | `/api/entity-configuration/{entityName}/save` | SaveEntityConfiguration | Save configuration |
| PUT | `/api/entity-configuration/{id}` | UpdateEntityConfiguration | Update configuration |
| DELETE | `/api/entity-configuration/{id}` | DeleteEntityConfiguration | Delete configuration |
| GET | `/api/entity-configuration/{entityManagerId}/fields` | GetEntityFields | Entity fields |
| GET | `/api/entity-configuration/related-fields/{entityType}` | GetRelatedFields | Related fields |
| POST | `/api/entity-field/create` | CreateEntityField | Create field |
| PUT | `/api/entity-field/{id}` | UpdateEntityField | Update field |
| DELETE | `/api/entity-field/{id}` | DeleteEntityField | Delete field |

---

## Entity Artifacts (Admin)

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/entity-artifacts/entity-types` | GetEntityTypes | Entity types |
| GET | `/api/entity-artifacts/artifact-types` | GetArtifactTypes | Artifact types |
| GET | `/api/entity-artifacts/entity-records` | GetEntityRecords | Entity records |
| GET | `/api/entity-artifacts/get` | GetEntityArtifact | Get artifact |
| POST | `/api/entity-artifacts/upsert` | UpsertEntityArtifact | Upsert artifact |
| POST | `/api/entity-artifacts/upload-document` | UploadDocument | Upload artifact document |
| GET | `/api/entity-artifacts/document-url` | GetDocumentUrl | Document URL |
| GET | `/api/entity-artifacts/list` | ListArtifacts | List artifacts |
| GET | `/api/entity-artifacts/bulk/artifact-types` | GetBulkArtifactTypes | Bulk artifact types |
| GET | `/api/entity-artifacts/bulk/unique-id-example` | GetUniqueIdExample | Unique ID example |
| POST | `/api/entity-artifacts/bulk/template-download` | DownloadBulkTemplate | Bulk template download |
| POST | `/api/entity-artifacts/bulk/upsert` | BulkUpsert | Bulk upsert |

---

## System Admin

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/system-admin/endpoints` | GetAvailableEndpoints | List admin endpoints |
| GET | `/api/system-admin/auth-debug` | AuthDebug | Auth debug info |
| GET | `/api/system-admin/migrations/run` | RunMigrations | Run migrations |
| GET | `/api/system-admin/seeding/run` | RunSeeding | Run all seeders |
| GET | `/api/system-admin/seeding/run/{name}` | RunSeederByName | Run specific seeder |
| GET | `/api/system-admin/seed-scripts/truncate` | TruncateSeedScripts | Truncate seed scripts |
| GET | `/api/system-admin/seed-scripts/delete/{name}` | DeleteSeedScript | Delete seed script |
| GET | `/api/system-admin/output-embeddings/generate` | GenerateOutputEmbeddings | Generate output embeddings |
| POST | `/api/system-admin/clean-up-users` | CleanUpUsers | Clean up users |
| GET | `/api/system-admin/regenerate-go-opportunity-pdfs` | RegenerateGoOpportunityPdfs | Regenerate GO PDFs |

---

## AI / Gemini

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `/api/ai-prompt-management/list` | ListPrompts | List AI prompts |
| GET | `/api/ai-prompt-management/{id}` | GetPrompt | Get prompt by ID |
| POST | `/api/ai-prompt-management` | CreatePrompt | Create prompt |
| PUT | `/api/ai-prompt-management/{id}` | UpdatePrompt | Update prompt |
| DELETE | `/api/ai-prompt-management/{id}` | DeletePrompt | Delete prompt |
| GET | `/api/ai-prompt-management/export-sql` | ExportPromptsSql | Export prompts as SQL |
| GET | `/api/ai-prompt-management/type/{type}` | GetPromptsByType | Prompts by type |
| GET | `/api/ai-prompt-management/types` | GetPromptTypes | Prompt types |
| GET | `/api/ai-prompt-management/models` | GetModels | Available models |
| GET | `/api/ai-prompt-management/projects` | GetProjects | Available projects |
| GET | `/api/ai-prompt-management/locations` | GetLocations | Available locations |
| POST | `/api/ai-prompt-management/test` | TestPrompt | Test prompt |
| POST | `/api/ai-prompt-management/upgrade-model` | UpgradeModel | Upgrade model |
| POST | `/api/ai-assistant/get-user-sessions` | GetUserSessions | User AI sessions |
| POST | `/api/ai-assistant/get-session` | GetSession | Get AI session |
| POST | `/api/ai-assistant/chat` | Chat | AI chat |
| POST | `/api/ai-assistant/accessibility` | Accessibility | Accessibility analysis |
| POST | `/api/ai-assistant/update-star` | UpdateStar | Star/unstar session |
| POST | `/api/ai-assistant/update-archive` | UpdateArchive | Archive session |
| POST | `/api/ai-assistant/update-title` | UpdateTitle | Update session title |
| POST | `/api/process-data` | ProcessDataSummary | Process data summary |
| POST | `/api/document-transcribe` | DocumentTranscribe | Document transcribe |
| GET | `/api/generate-embeddings` | GenerateEmbeddings | Generate embeddings |

---

## AI Retriever

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `/api/ai-retriever/vector-store/search` | SearchVectorStore | Vector store search |
| POST | `/api/ai-retriever/convert/url` | ConvertUrl | Convert URL content |
| POST | `/api/ai-retriever/convert/markdown-to-google-doc` | ConvertMarkdownToGoogleDoc | Markdown to Google Doc |
| GET | `/api/ai-retriever/health` | Health | Health check |

---

## Base Engagements

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `/api/base-engagements` | GetAll | List base engagements |
| GET | `/api/base-engagements/{id}` | GetById | Get by ID |
| GET | `/api/partners/{partnerId}/base-engagements` | GetByPartner | By partner |
| GET | `/api/base-engagements/{engagementId}/partners` | GetPartners | Partners for engagement |

---

## Roles

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `api/Role/all` | GetAllRoles | All roles |
| GET | `api/Role/user` | GetUserRoles | Current user roles |
| PUT | `api/Role/update` | UpdateUserRoles | Update user roles |
| POST | `api/Role/assign-doa-roles` | AssignDoaRoles | Assign DOA roles |

---

## Liaison Offices

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `api/LiaisonOffice` | GetLiaisonOffices | List liaison offices |
| POST | `api/LiaisonOffice/search` | SearchLiaisonOffices | Search |
| GET | `api/LiaisonOffice/{id}` | GetLiaisonOffice | Get by ID |

---

## Countries

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `api/Country` | GetCountries | List countries |
| POST | `api/Country/search` | SearchCountries | Search |
| GET | `api/Country/{id}` | GetCountryById | Get by ID |
| POST | `api/Country/dynamic-search` | DynamicSearchCountries | Dynamic search |

---

## Gmail Addon

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| POST | `api/gmail-addon/interactions` | CreateInteraction | Create interaction |
| POST | `api/gmail-addon/interactions/find` | FindInteraction | Find interaction |
| POST | `api/gmail-addon/interactions/find-related-records` | FindRelatedRecords | Find related records |
| POST | `api/gmail-addon/create-records` | CreateRecords | Create records |

---

## Development (Dev Only)

These endpoints are only available in the development environment.

| Method | Route | Name | Description |
|--------|-------|------|-------------|
| GET | `api/dev/users` | GetDevelopmentUsers | Dev users |
| POST | `api/dev/login/{email}` | Login | Dev login |
| POST | `api/dev/seed-dev-users` | SeedDevUsers | Seed dev users |
| GET | `api/dev/check-iap-simulation` | CheckIapSimulation | Check IAP simulation |
| GET | `api/dev/debug` | Debug | Debug info |
| GET | `api/dev/direct-login/{email}` | DirectLogin | Direct login |
| GET | `api/dev/set-cookie/{email}` | SetCookie | Set auth cookie |
| GET | `api/dev/debug-auth` | DebugAuth | Debug auth |
| GET | `api/dev/verify-roles` | VerifyRoles | Verify roles |
| POST | `api/dev/clear-all-users` | ClearAllUsers | Clear all users |
| POST | `api/dev/setup-row-level-filters` | SetupRowLevelFilters | Setup row-level filters |
| POST | `api/dev/create-user` | CreateUser | Create user |
| GET | `api/dev/configured-user` | GetConfiguredUser | Get configured user |
