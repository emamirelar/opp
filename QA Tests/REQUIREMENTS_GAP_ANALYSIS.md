# Requirements Gap Analysis - Missing Test Cases

**Generated:** January 13, 2026  
**Purpose:** Identify requirements from PRDs and design documents that lack corresponding test cases

---

## Executive Summary

This analysis cross-references all PRD documents, design documents, and implementation plans against existing test cases to identify gaps in test coverage.

### Coverage Summary

| Feature Area | Requirements | Test Cases | Coverage | Status |
|--------------|--------------|------------|----------|--------|
| **Opportunity Management** | 40+ | 0 | 0% | ❌ Not Covered |
| **AI Section Assistance** | 25+ | 0 | 0% | ❌ Not Covered |
| **Opportunity UI Options** | 30+ | 0 | 0% | ❌ Not Covered |
| **Documents & Related Items** | 20+ | 0 | 0% | ❌ Not Covered |
| **Geography Management** | 15+ | 0 | 0% | ❌ Not Covered |
| **DST (Decision Support Tool)** | 35+ | 0 | 0% | ❌ Not Covered |
| **CRM Enhancement** | 200+ | 430+ | 100%+ | ✅ Fully Covered |
| **Partnership Management** | 1,200+ | 1,200+ | 100% | ✅ Fully Covered |
| **Document Management** | 100+ | 100+ | 100% | ✅ Fully Covered |

---

## 1. Opportunity Management Features (0% Coverage)

### Source Document
`tasks/opportunity-ux/Opportunity Epics.md`

### Epic 1: Create New Opportunity - Properties and Components

**Requirements Without Test Cases:**

#### 1.1 Opportunity Entity Structure
- [ ] Create opportunity with all required P3M entity properties
- [ ] Validate opportunity status (Active, On Hold, Closed, Recovered)
- [ ] Update opportunity status with proper permissions
- [ ] Track opportunity through its full lifecycle
- [ ] Link opportunity to eventual Project/Programme/Portfolio designation

**Test Cases Needed:**
- `OpportunityManager_CreateOpportunity_WithRequiredProperties_Success`
- `OpportunityManager_UpdateOpportunityStatus_ValidTransitions_Success`
- `OpportunityManager_RecoverClosedOpportunity_WithPermissions_Success`
- `OpportunityManager_ConvertOpportunityToProject_ValidatesData_Success`

#### 1.2 AI-Powered Data Suggestions
- [ ] Suggest SDGs based on deliverable nature
- [ ] Propose UN Cooperation Framework Outcomes
- [ ] Auto-suggest related properties based on context
- [ ] Present suggestions for user verification
- [ ] Handle user acceptance/rejection of suggestions

**Test Cases Needed:**
- `OpportunityManager_SuggestSDGs_BasedOnDeliverables_ReturnsRelevant`
- `OpportunityManager_ProposeUNCFOutcomes_ByCountry_ReturnsApplicable`
- `OpportunityManager_VerifyAISuggestion_UserAccepts_UpdatesOpportunity`
- `OpportunityManager_RejectAISuggestion_MaintainsOriginalData`

---

### Epic 2: Document Upload and AI Extraction

**Requirements Without Test Cases:**

#### 2.1 Document Upload and Analysis
- [ ] Upload background documentation (concept notes, correspondence, strategic plans)
- [ ] Machine reading of documents to extract structured data
- [ ] Present extracted data for user verification
- [ ] Match document content to structured fields
- [ ] Handle multiple document types (PDF, Word, Excel)

**Test Cases Needed:**
- `DocumentManager_UploadConceptNote_ExtractsStructuredData_Success`
- `DocumentManager_AnalyzeMultipleDocuments_MergesDataCorrectly`
- `DocumentManager_ExtractDataFromPDF_MatchesStructuredFields`
- `DocumentManager_UserVerifiesExtractedData_AcceptsOrRejects`
- `AiManager_ExtractOpportunityDetails_FromFeasibilityStudy_Success`

#### 2.2 Content Proposing from Documents
- [ ] AI proposes data when content is found in source
- [ ] AI suggests data when direct content is not found
- [ ] Infer applicable SDGs from deliverables
- [ ] Suggest country-specific UN frameworks
- [ ] Present proposals with confidence levels

**Test Cases Needed:**
- `AiManager_ProposeDataFromDocument_HighConfidence_Presents`
- `AiManager_InferSDGs_FromProjectScope_ReturnsTop3`
- `AiManager_SuggestCountryFrameworks_BasedOnLocation_Relevant`
- `AiManager_NoDirectMatch_ProposesInferredData_WithConfidence`

---

### Epic 3: Manage Office - Org Structure Integration

**Requirements Without Test Cases:**

#### 3.1 Organizational Unit Management
- [ ] Capture org unit functions and responsibilities
- [ ] Link org units to geographies they're responsible for
- [ ] Assign key office roles to org units
- [ ] Cascade policies and guidance through org structure
- [ ] Apply business rules per org unit context

**Test Cases Needed:**
- `OrgUnitManager_DefineOrgUnitResponsibilities_ForGeography_Success`
- `OrgUnitManager_AssignKeyRoles_ToOrgUnit_ValidatesHierarchy`
- `OrgUnitManager_CascadePolicies_ThroughHierarchy_AppliesCorrectly`
- `OrgUnitManager_LinkGeographies_ToResponsibleUnit_Success`

#### 3.2 Office Type Management
- [ ] Different responsibilities by office type
- [ ] Personnel role assignments per office type
- [ ] Link office roles to system permissions
- [ ] Validate DoA (Delegation of Authority) assignments

**Test Cases Needed:**
- `OrgUnitManager_AssignRolesByOfficeType_ValidatesResponsibilities`
- `PermissionManager_LinkOfficeRole_ToSystemPermissions_Success`
- `OrgUnitManager_ValidateDoAAssignment_ChecksHierarchy`

---

### Epic 4: Geography Management - Country Profiles

**Requirements Without Test Cases:**

#### 4.1 Country Profile Management
- [ ] Store responsible organizational unit per country
- [ ] Track host country agreement status
- [ ] Manage UN Cooperation Framework details
- [ ] Store social and environmental risk indices
- [ ] Reference country data in business rules

**Test Cases Needed:**
- `CountryManager_AssignResponsibleOrgUnit_ToCountry_Success`
- `CountryManager_UpdateHostCountryAgreement_ValidatesStatus`
- `CountryManager_ManageUNCFDetails_PerCountry_Success`
- `CountryManager_StoreRiskIndices_ForBusinessRules_Accessible`
- `OpportunityManager_ApplyCountryRules_BasedOnProfile_Success`

#### 4.2 Country Contextual Data
- [ ] Surface known risks based on country profile
- [ ] Indicate complexity factors for delivery
- [ ] Show reporting obligations per country
- [ ] Display UNSDCF alignment requirements

**Test Cases Needed:**
- `CountryService_GetRiskFactors_ByCountry_ReturnsAll`
- `CountryService_GetComplexityIndicators_ForPlanning_Success`
- `CountryService_GetReportingObligations_ByCountry_Success`
- `CountryService_GetUNSDCFAlignment_Requirements_Success`

---

### Epic 5: Partnership Agreement Library

**Requirements Without Test Cases:**

#### 5.1 Agreement Document Management
- [ ] Store partnership agreements as artifacts
- [ ] Extract key terms of engagement
- [ ] Capture structured metadata (geography, scope, pricing)
- [ ] Link agreements to opportunities early in development
- [ ] Support global and local agreement types

**Test Cases Needed:**
- `PartnershipAgreementManager_UploadAgreement_ExtractsMetadata_Success`
- `PartnershipAgreementManager_LinkToOpportunity_EarlyStage_Success`
- `PartnershipAgreementManager_ExtractPricingTerms_FromAgreement_Success`
- `PartnershipAgreementManager_DifferentiateGlobalLocal_Correctly`

#### 5.2 Agreement Data Utilization
- [ ] Pre-populate opportunity fields from agreements
- [ ] Validate opportunity scope against agreement
- [ ] Check pricing against pre-agreed arrangements
- [ ] Verify geography restrictions from agreements

**Test Cases Needed:**
- `OpportunityManager_PrePopulateFromAgreement_ValidatesData`
- `OpportunityManager_ValidateScopeAgainstAgreement_EnforcesLimits`
- `OpportunityManager_CheckPricing_AgainstAgreement_Warns`
- `OpportunityManager_VerifyGeographyRestrictions_FromAgreement`

---

### Epic 6: Opportunity Management Products

**Requirements Without Test Cases:**

#### 6.1 Draft High-Level Budget
- [ ] Generate high-level budget from opportunity details
- [ ] Apply default fee percentage if none in sources
- [ ] Show spend rate visualization over time
- [ ] Display opportunity development costs (funded/unfunded)
- [ ] Segregate development costs from project budget

**Test Cases Needed:**
- `OpportunityBudgetManager_GenerateHighLevelBudget_FromDetails_Success`
- `OpportunityBudgetManager_ApplyDefaultFee_WhenNotSpecified_Success`
- `OpportunityBudgetManager_CalculateSpendRate_OverTime_Accurate`
- `OpportunityBudgetManager_SeparateDevelopmentCosts_FromProjectBudget`

#### 6.2 Draft High-Level Schedule
- [ ] Generate high-level schedule from opportunity
- [ ] Create work breakdown structure from deliverables
- [ ] Show visual indication of timeline
- [ ] Include opportunity development phases
- [ ] Link schedule to personnel and budget

**Test Cases Needed:**
- `OpportunityScheduleManager_GenerateSchedule_FromDeliverables_Success`
- `OpportunityScheduleManager_CreateWBS_FromOpportunity_Structured`
- `OpportunityScheduleManager_IncludeDevelopmentPhases_InTimeline`

#### 6.3 Resource Plan
- [ ] Identify roles needed for development
- [ ] Identify roles needed for implementation
- [ ] Link named resources where known
- [ ] Calculate personnel budgeting
- [ ] Show core team vs support personnel

**Test Cases Needed:**
- `ResourcePlanManager_IdentifyDevelopmentRoles_FromOpportunity`
- `ResourcePlanManager_IdentifyImplementationRoles_WithSkills`
- `ResourcePlanManager_CalculatePersonnelBudget_ByRoles`
- `ResourcePlanManager_DifferentiateCoreVsSupport_Personnel`

#### 6.4 Draft Risk Register
- [ ] Create opportunity-linked risk register
- [ ] AI suggests risks from similar projects
- [ ] Track when risks were identified (stage)
- [ ] Maintain single register through lifecycle
- [ ] Support risk mitigation planning

**Test Cases Needed:**
- `RiskManager_CreateOpportunityRiskRegister_LinkedToEntity`
- `AiManager_SuggestRisks_FromSimilarProjects_Relevant`
- `RiskManager_TrackRiskIdentificationStage_Accurate`
- `RiskManager_MaintainSingleRegister_ThroughLifecycle`

---

### Epic 7: Opportunity Profiling (DST Integration)

**Requirements Without Test Cases:**

#### 7.1 Decision Support Tool (DST) Profiling
- [ ] Systematically analyze feasibility, complexity, risk
- [ ] Assess alignment with UNOPS strategic priorities
- [ ] Analyze delivery risk and past performance
- [ ] Evaluate contextual dynamics
- [ ] Assess required resources

**Test Cases Needed:**
- `DSTManager_AnalyzeFeasibility_BasedOnFactors_ReturnsScore`
- `DSTManager_AssessComplexity_FromOpportunityData_Categorizes`
- `DSTManager_AnalyzeDeliveryRisk_ByCountryAndScope_Success`
- `DSTManager_EvaluateStrategicAlignment_WithUNOPSGoals_Scores`
- `DSTManager_AssessResourceRequirements_ComparedToCapacity`

#### 7.2 DST Profile Report Generation
- [ ] Generate profile report from structured data
- [ ] Include country context and risks
- [ ] Show past performance data
- [ ] Display organizational capabilities
- [ ] Present partner profiles and due diligence

**Test Cases Needed:**
- `DSTManager_GenerateProfileReport_WithAllSections_Complete`
- `DSTManager_IncludeCountryContext_InReport_Comprehensive`
- `DSTManager_ShowPastPerformance_BySimilarProjects_Relevant`
- `DSTManager_DisplayOrgCapabilities_ForOpportunity_Accurate`

#### 7.3 DST Nine Parameter Evaluation
- [ ] Evaluate strategic alignment
- [ ] Assess partners and stakeholders
- [ ] Analyze physical implementation
- [ ] Evaluate context
- [ ] Assess scope and scale
- [ ] Analyze timeframe
- [ ] Evaluate budget and resourcing
- [ ] Assess outcome and impact
- [ ] Evaluate safeguards, ethics, legal

**Test Cases Needed:**
- `DSTManager_EvaluateStrategicAlignment_GeneratesNarrative`
- `DSTManager_AssessPartners_AndStakeholders_Comprehensive`
- `DSTManager_AnalyzePhysicalImplementation_Feasibility`
- `DSTManager_EvaluateContext_RisksAndOpportunities`
- `DSTManager_AssessScopeAndScale_Complexity`
- `DSTManager_AnalyzeTimeframe_Realism`
- `DSTManager_EvaluateBudget_Adequacy`
- `DSTManager_AssessOutcome_AndImpact_Likelihood`
- `DSTManager_EvaluateSafeguards_ComplianceRequirements`

#### 7.4 DST Actionable Recommendations
- [ ] Suggest risks for draft risk register
- [ ] Suggest issues and lessons learned
- [ ] Flag personnel requirements (e.g., gender advisor)
- [ ] Suggest initiative structure (project/programme/portfolio)
- [ ] Allow marking recommendations (considered/actioned/rejected)

**Test Cases Needed:**
- `DSTManager_SuggestRisks_AddableToDraftRegister`
- `DSTManager_SuggestIssues_FromLessonsLearned_Relevant`
- `DSTManager_FlagPersonnelRequirements_BySpecialization`
- `DSTManager_SuggestInitiativeStructure_BasedOnAnalysis`
- `DSTManager_TrackRecommendationStatus_UserActions`

---

### Epic 8: Opportunity Statement and Concept Note

**Requirements Without Test Cases:**

#### 8.1 Opportunity Statement Management
- [ ] Draft internal opportunity statement
- [ ] Use templates for structure
- [ ] Pre-populate with captured data
- [ ] Generate narrative text from data
- [ ] Support real-time collaboration
- [ ] Auto-save to opportunity folder

**Test Cases Needed:**
- `OpportunityStatementManager_DraftFromTemplate_PrePopulates`
- `OpportunityStatementManager_GenerateNarrative_FromStructuredData`
- `OpportunityStatementManager_SupportRealTimeCollab_MultipleUsers`
- `OpportunityStatementManager_AutoSave_ToEntityFolder_Success`

#### 8.2 Concept Note Management
- [ ] Draft partner-facing concept note
- [ ] Tailor format to partner expectations
- [ ] Pre-populate from opportunity data
- [ ] Support UNOPS or partner-specific templates
- [ ] Enable stakeholder collaboration

**Test Cases Needed:**
- `ConceptNoteManager_DraftFromTemplate_PartnerSpecific`
- `ConceptNoteManager_TailorFormat_ToPartnerExpectations`
- `ConceptNoteManager_PrePopulate_FromOpportunityData`
- `ConceptNoteManager_CollaborateWithStakeholders_Success`

---

### Epic 9: Go/No-Go Decision (Decide Stage)

**Requirements Without Test Cases:**

#### 9.1 Decision Package Review
- [ ] Review full opportunity package
- [ ] Review final opportunity statement with DST insights
- [ ] Review draft concept note
- [ ] Review opportunity development plan
- [ ] Review due diligence flags

**Test Cases Needed:**
- `DecisionManager_AssembleOpportunityPackage_Complete`
- `DecisionManager_IncludeDSTInsights_InPackage`
- `DecisionManager_ValidatePackageCompleteness_BeforeSubmission`

#### 9.2 DOA Holder Decision Making
- [ ] Provide comments and direction
- [ ] Document decision rationale
- [ ] Make Go/No-Go decision with timestamp
- [ ] Record decision maker name and DOA level
- [ ] Link decision to opportunity statement
- [ ] Note key risks and opportunities
- [ ] Apply conditions or caveats

**Test Cases Needed:**
- `DecisionManager_RecordGoDecision_WithAllMetadata_Success`
- `DecisionManager_RecordNoGoDecision_WithJustification_Success`
- `DecisionManager_ApplyConditions_ToGoDecision_Tracked`
- `DecisionManager_LinkRisks_ToDecisionRationale_Success`

#### 9.3 Decision Authorization
- [ ] Authorize use of opportunity budget
- [ ] Authorize personnel allocation
- [ ] Delegate decision-making responsibility
- [ ] Configure required documentation checklist
- [ ] Validate delegation authority

**Test Cases Needed:**
- `DecisionManager_AuthorizeBudget_OnGoDecision_Success`
- `DecisionManager_AuthorizePersonnel_FromDevelopmentPlan`
- `DecisionManager_DelegateDecision_ValidatesAuthority`
- `DecisionManager_RequireDocumentation_BeforeSubmission`

#### 9.4 Decision Audit Trail
- [ ] Store and version control opportunity statement
- [ ] Reflect decision history
- [ ] Track rationale changes
- [ ] Maintain auditable delegation chain

**Test Cases Needed:**
- `DecisionManager_VersionControlStatement_AtDecisionPoint`
- `DecisionManager_MaintainDecisionHistory_Auditable`
- `DecisionManager_TrackDelegationChain_Complete`

---

### Epic 10: Global Indices Management

**Requirements Without Test Cases:**

#### 10.1 Global Country Data Upload
- [ ] Periodically upload global indices
- [ ] Update all country records simultaneously
- [ ] Replace previous versions with current data
- [ ] Add new fields when indices are introduced
- [ ] Retire indices no longer relevant
- [ ] Maintain historical "as-at" views

**Test Cases Needed:**
- `GlobalIndicesManager_UploadNewIndices_UpdatesAllCountries`
- `GlobalIndicesManager_ReplaceOutdatedData_MaintainsHistory`
- `GlobalIndicesManager_AddNewIndexField_ToAllCountries`
- `GlobalIndicesManager_RetireIndex_MaintainsHistoricalData`
- `GlobalIndicesManager_QueryHistoricalData_AsAtDate_Accurate`

#### 10.2 Indices Utilization
- [ ] Report on countries by MVI score threshold
- [ ] Identify fragile states for business rules
- [ ] Use corruption index in risk assessment
- [ ] Apply indices to opportunity profiling

**Test Cases Needed:**
- `ReportingManager_CountriesByMVIScore_InPeriod_Accurate`
- `BusinessRulesEngine_IdentifyFragileStates_AppliesRules`
- `RiskManager_UseCorruptionIndex_InAssessment_Success`
- `DSTManager_ApplyIndices_ToOpportunityProfiling_Relevant`

---

## 2. AI Section Assistance Feature (0% Coverage)

### Source Documents
- `tasks/app-assistance-and-transcribe-feature/IMPLEMENTATION-PLAN.md`
- `tasks/app-assistance-and-transcribe-feature/RECOMMENDATIONS.md`

### 2.1 Application Guidance Management

**Requirements Without Test Cases:**

#### Guidance Storage and Retrieval
- [ ] Store guidance markdown per section
- [ ] Retrieve guidance by feature and section
- [ ] Filter guidance by route
- [ ] Search guidance content
- [ ] Priority-based sorting
- [ ] Version control for guidance

**Test Cases Needed:**
- `ApplicationGuidanceManager_StoreGuidance_ForSection_Success`
- `ApplicationGuidanceManager_RetrieveGuidance_ByFeatureAndSection`
- `ApplicationGuidanceManager_FilterByRoute_ReturnsRelevant`
- `ApplicationGuidanceManager_SearchGuidance_ByKeyword_Success`
- `ApplicationGuidanceManager_SortByPriority_OrdersCorrectly`
- `ApplicationGuidanceManager_VersionControl_MaintainsHistory`

---

### 2.2 AI Section Assistance Processing

**Requirements Without Test Cases:**

#### Request Processing
- [ ] Accept section assistance requests
- [ ] Retrieve relevant guidance
- [ ] Build Gemini prompts with context
- [ ] Include entity data in context
- [ ] Generate AI responses
- [ ] Support document upload for analysis
- [ ] Support autofill mode

**Test Cases Needed:**
- `AiSectionAssistanceManager_ProcessRequest_WithGuidance_Success`
- `AiSectionAssistanceManager_BuildPrompt_IncludesContext_Correctly`
- `AiSectionAssistanceManager_IncludeEntityData_InPrompt_Success`
- `AiSectionAssistanceManager_GenerateResponse_MarkdownFormat`
- `AiSectionAssistanceManager_ProcessDocument_ExtractsData`
- `AiSectionAssistanceManager_AutofillMode_ReturnsJSON_Success`

#### Response Generation
- [ ] Generate markdown responses for display
- [ ] Generate JSON for autofill
- [ ] Include guidance references in responses
- [ ] Handle missing guidance gracefully
- [ ] Track response quality metrics

**Test Cases Needed:**
- `AiSectionAssistanceManager_GenerateMarkdown_Formatted`
- `AiSectionAssistanceManager_GenerateJSON_ForAutofill_Valid`
- `AiSectionAssistanceManager_IncludeReferences_InResponse`
- `AiSectionAssistanceManager_NoGuidance_UsesGenericPrompt`
- `AiSectionAssistanceManager_TrackResponseMetrics_Success`

---

### 2.3 Frontend Component

**Requirements Without Test Cases:**

#### Component Functionality
- [ ] Open popup from section headers
- [ ] Display loading states
- [ ] Show AI responses with markdown
- [ ] Support document upload
- [ ] Autofill form fields
- [ ] Track user interactions
- [ ] Support quick actions

**Test Cases Needed:**
- `AiSectionAssistanceComponent_OpenPopup_DisplaysCorrectly`
- `AiSectionAssistanceComponent_ShowLoadingState_WhileProcessing`
- `AiSectionAssistanceComponent_RenderMarkdown_Properly`
- `AiSectionAssistanceComponent_UploadDocument_TriggersAnalysis`
- `AiSectionAssistanceComponent_AutofillFields_FromResponse`
- `AiSectionAssistanceComponent_TrackInteractions_ForAnalytics`
- `AiSectionAssistanceComponent_ExecuteQuickActions_Success`

---

## 3. Opportunity UI Options (0% Coverage)

### Source Documents
- `tasks/opportunity-ux/IMPLEMENTATION-README.md`
- `tasks/opportunity-ux/Option1-Unified-Dashboard-View.md`
- `tasks/opportunity-ux/Option2-Tabbed-Content-Organization.md`
- `tasks/opportunity-ux/Option3-Wizard-Guided-Workflow.md`

### 3.1 Option 1: Unified Dashboard View

**Requirements Without Test Cases:**

#### Layout and Navigation
- [ ] Single scrolling page with all sections
- [ ] 5W framework organization (What, Who, Why, When, Where)
- [ ] Collapsible sections
- [ ] Sticky action bar at bottom
- [ ] Persistent AI assistant panel

**Test Cases Needed:**
- `OpportunityOption1_RenderAllSections_InSingleView`
- `OpportunityOption1_CollapseSection_HidesContent`
- `OpportunityOption1_ScrollToSection_Smoothly`
- `OpportunityOption1_StickyActionBar_AlwaysVisible`
- `OpportunityOption1_AIPanel_PersistentAndResponsive`

#### Data Display
- [ ] Complete information at a glance
- [ ] Quick stats sidebar
- [ ] Inline editing capabilities
- [ ] Comment thread at bottom

**Test Cases Needed:**
- `OpportunityOption1_DisplayCompleteData_NoNavigation`
- `OpportunityOption1_QuickStats_UpdatesInRealTime`
- `OpportunityOption1_InlineEdit_UpdatesImmediately`
- `OpportunityOption1_CommentThread_AddsComments_Success`

---

### 3.2 Option 2: Tabbed Content Organization

**Requirements Without Test Cases:**

#### Tab Management
- [ ] 6 organized tabs (Overview, Stakeholders, Finances, Timeline, Geography, AI Insights)
- [ ] Context bar with key metrics always visible
- [ ] Adaptive AI panel per tab
- [ ] Progress indicators on tabs
- [ ] Completion tracking

**Test Cases Needed:**
- `OpportunityOption2_SwitchTabs_LoadsContent_Quickly`
- `OpportunityOption2_ContextBar_DisplaysKeyMetrics_Always`
- `OpportunityOption2_AIPanel_AdaptsToTab_Context`
- `OpportunityOption2_ProgressIndicators_ShowCompletion`
- `OpportunityOption2_TrackCompletion_PerTab_Accurate`

#### Tab Features
- [ ] Tab badges show completion and warnings
- [ ] Context-aware AI suggestions per tab
- [ ] Master-detail patterns
- [ ] Lazy loading of tab content

**Test Cases Needed:**
- `OpportunityOption2_TabBadges_ShowWarnings_Correctly`
- `OpportunityOption2_ContextAwareAI_ChangesWithTab`
- `OpportunityOption2_MasterDetail_Navigation_Success`
- `OpportunityOption2_LazyLoad_Improves_Performance`

---

### 3.3 Option 3: Wizard-Guided Workflow

**Requirements Without Test Cases:**

#### Wizard Navigation
- [ ] 6-step workflow with clear progression
- [ ] Visual progress indicator
- [ ] Step-specific AI guidance
- [ ] Validation at each step
- [ ] Flexible navigation to completed steps

**Test Cases Needed:**
- `OpportunityOption3_NavigateSteps_InSequence_Success`
- `OpportunityOption3_ProgressIndicator_ShowsCurrentStep`
- `OpportunityOption3_StepValidation_PreventsAdvance_WhenIncomplete`
- `OpportunityOption3_JumpToCompletedStep_AllowsEdits`

#### Wizard Features
- [ ] Step completion checklist
- [ ] Review and submit final stage
- [ ] Completion requirements per step
- [ ] Examples shown for specific steps

**Test Cases Needed:**
- `OpportunityOption3_CompletionChecklist_TracksProgress`
- `OpportunityOption3_ReviewStage_ShowsAllData_BeforeSubmit`
- `OpportunityOption3_StepRequirements_EnforcedCorrectly`
- `OpportunityOption3_ShowExamples_ForSteps3And6`

---

## 4. Documents & Related Items Panel (0% Coverage)

### Source Document
`tasks/opportunity-ux/DOCUMENTS-AND-RELATED-ITEMS-IMPLEMENTATION.md`

### 4.1 Documents Panel

**Requirements Without Test Cases:**

#### Document Management
- [ ] Drag & drop upload zone
- [ ] Display document list with metadata
- [ ] Show AI processing status badges
- [ ] Document categories
- [ ] File type icons with color coding
- [ ] Filter by document category
- [ ] Collapsible sidebar
- [ ] Sticky positioning

**Test Cases Needed:**
- `DocumentsPanel_DragDrop_UploadsFile_Success`
- `DocumentsPanel_DisplayMetadata_FileSize_ProcessingStatus`
- `DocumentsPanel_ShowAIBadge_OnProcessedDocuments`
- `DocumentsPanel_FilterByCategory_ShowsRelevant`
- `DocumentsPanel_CollapseSidebar_MaximizesContent`

#### Document Display
- [ ] File size display
- [ ] Document categories (Planning, Communication, Financial, etc.)
- [ ] Hover actions for quick access
- [ ] Color-coded file type icons

**Test Cases Needed:**
- `DocumentsPanel_DisplayFileSize_Formatted`
- `DocumentsPanel_ShowCategory_OnHover`
- `DocumentsPanel_HoverActions_QuickAccess`
- `DocumentsPanel_FileTypeIcons_ColorCoded`

---

### 4.2 Related Items Panel

**Requirements Without Test Cases:**

#### Related Contacts
- [ ] Show top 5 contacts with avatars
- [ ] Display name, organization, email
- [ ] "View all" link for complete list
- [ ] Quick external link icons

**Test Cases Needed:**
- `RelatedItemsPanel_ShowTop5Contacts_MostRecent`
- `RelatedItemsPanel_DisplayContactDetails_Complete`
- `RelatedItemsPanel_ViewAllContacts_NavigatesToList`
- `RelatedItemsPanel_ExternalLinks_OpenCorrectly`

#### Related Partners
- [ ] Show partner relationship types
- [ ] Display engagement levels
- [ ] Distinguish partner types (Funding, Client, Implementing)

**Test Cases Needed:**
- `RelatedItemsPanel_ShowPartners_WithRelationshipType`
- `RelatedItemsPanel_DisplayEngagementLevel_Accurate`
- `RelatedItemsPanel_DistinguishPartnerTypes_Visually`

#### Recent Interactions
- [ ] Show recent interactions (meetings, calls, emails, visits)
- [ ] Color-code by interaction type
- [ ] Display participants
- [ ] Show interaction dates

**Test Cases Needed:**
- `RelatedItemsPanel_ShowRecentInteractions_Chronological`
- `RelatedItemsPanel_ColorCodeByType_Consistent`
- `RelatedItemsPanel_DisplayParticipants_Complete`
- `RelatedItemsPanel_ShowDates_Formatted`

---

### 4.3 Tabbed Interface

**Requirements Without Test Cases:**

#### AI Assistant Tab
- [ ] Quick stats panel (Budget, Date, Countries)
- [ ] AI suggestions contextual
- [ ] AI action buttons (View Full Analysis, Generate Draft Budget)

**Test Cases Needed:**
- `TabbedInterface_AITab_ShowsQuickStats`
- `TabbedInterface_AITab_ContextualSuggestions`
- `TabbedInterface_AITab_ActionButtons_Execute`

#### Related Items Tab
- [ ] Switch between AI and Related Items
- [ ] Maintain state when switching
- [ ] Load related data on demand

**Test Cases Needed:**
- `TabbedInterface_SwitchTabs_MaintainsState`
- `TabbedInterface_RelatedTab_LoadsOnDemand`
- `TabbedInterface_TabChange_UpdatesAIContext`

---

## 5. Advanced Opportunity Features

### 5.1 Opportunity Development Plan

**Requirements Without Test Cases:**

#### Development Planning
- [ ] Work plan at appropriate detail level for stage
- [ ] Personnel plan including development team
- [ ] Opportunity-stage budget
- [ ] Capture financial resources before signing
- [ ] Track development costs vs implementation costs

**Test Cases Needed:**
- `OpportunityDevelopmentPlan_CreateWorkPlan_StageAppropriate`
- `OpportunityDevelopmentPlan_DefinePersonnelPlan_WithRoles`
- `OpportunityDevelopmentPlan_CalculateBudget_OpportunityStage`
- `OpportunityDevelopmentPlan_TrackDevelopmentCosts_Separately`

---

### 5.2 Define Management Products

**Requirements Without Test Cases:**

#### Template Management
- [ ] Define Opportunity Statement template
- [ ] Create section prompts for templates
- [ ] Different templates per capability
- [ ] Version control for templates
- [ ] Template collaboration features

**Test Cases Needed:**
- `TemplateManager_DefineOpportunityStatement_Template`
- `TemplateManager_CreateSectionPrompts_ForAIAssistance`
- `TemplateManager_ManageTemplates_PerCapability`
- `TemplateManager_VersionControl_Templates_Success`

---

### 5.3 Business Rules Engine

**Requirements Without Test Cases:**

#### Rules Configuration
- [ ] Define business rules per practice capability
- [ ] Configure process triggers
- [ ] Set effective dates for rules
- [ ] Handle rule validity periods
- [ ] Track rule change impacts

**Test Cases Needed:**
- `BusinessRulesEngine_DefineRules_PerCapability`
- `BusinessRulesEngine_ConfigureTriggers_ForProcesses`
- `BusinessRulesEngine_SetEffectiveDates_ValidatesTransitions`
- `BusinessRulesEngine_TrackRuleChanges_ImpactAnalysis`

#### Rules Application
- [ ] Apply rules to opportunity profiling
- [ ] Trigger processes based on rules
- [ ] Flag risks based on business rules
- [ ] Validate against practice standards

**Test Cases Needed:**
- `BusinessRulesEngine_ApplyRules_ToProfiling_Success`
- `BusinessRulesEngine_TriggerProcesses_OnRuleMatch`
- `BusinessRulesEngine_FlagRisks_BasedOnRules`
- `BusinessRulesEngine_ValidateStandards_Compliance`

---

## 6. Programme and Portfolio Recognition

### Source
`tasks/opportunity-ux/Opportunity Epics.md` - Epic "Adapting oUP Engagement"

**Requirements Without Test Cases:**

### 6.1 Programme/Portfolio Tagging
- [ ] Tag engagements as Programme or Portfolio
- [ ] Associate child engagements to parent
- [ ] Maintain validated list of programmes/portfolios
- [ ] Link new engagements to existing programmes
- [ ] Track programme/portfolio hierarchy

**Test Cases Needed:**
- `EngagementManager_TagAsProgram_ValidatesType`
- `EngagementManager_TagAsPortfolio_ValidatesType`
- `EngagementManager_AssociateToParent_ValidatesHierarchy`
- `EngagementManager_MaintainValidatedList_Accessible`
- `EngagementManager_LinkToExistingProgram_Success`

### 6.2 Engagement Hierarchy
- [ ] Support standalone projects
- [ ] Support child projects of programmes
- [ ] Support new funding for existing programmes
- [ ] Track programme/portfolio relationships
- [ ] Aggregate data from children to parent

**Test Cases Needed:**
- `EngagementManager_CreateStandaloneProject_Success`
- `EngagementManager_CreateChildProject_LinksToProgram`
- `EngagementManager_AddFunding_ToExistingProgram_Updates`
- `EngagementManager_TrackHierarchy_Relationships_Accurate`
- `EngagementManager_AggregateData_ToParent_Success`

---

## Summary of Priority Test Development

### Immediate Priority (P0) - Core Opportunity Management
1. **Opportunity Entity & CRUD** (~40 tests)
2. **Document Upload & AI Extraction** (~30 tests)
3. **DST Profiling & Analysis** (~35 tests)
4. **Go/No-Go Decision Process** (~25 tests)

**Estimated Total:** ~130 high-priority tests

---

### High Priority (P1) - Extended Features
1. **Country Profile Management** (~20 tests)
2. **Partnership Agreement Library** (~20 tests)
3. **Opportunity Management Products** (~30 tests)
4. **AI Section Assistance** (~25 tests)

**Estimated Total:** ~95 high-priority tests

---

### Medium Priority (P2) - UI and UX
1. **Opportunity UI Options** (~60 tests)
2. **Documents & Related Items Panel** (~30 tests)
3. **Business Rules Engine** (~20 tests)

**Estimated Total:** ~110 medium-priority tests

---

### Lower Priority (P3) - Advanced Features
1. **Global Indices Management** (~15 tests)
2. **Programme/Portfolio Recognition** (~15 tests)
3. **Template Management** (~10 tests)

**Estimated Total:** ~40 lower-priority tests

---

## Grand Total

**Total Requirements Identified:** ~375 requirements  
**Total Test Cases Needed:** ~375+ tests  
**Current Coverage:** 0%

**Existing Test Coverage (Other Features):**
- Partnership Management: 1,200+ tests (100%)
- CRM Enhancement: 430+ tests (100%)
- Document Management: 100+ tests (100%)
- Business Managers: 1,200+ tests (100%)

---

## Recommendations

### Phase 1: Core Opportunity Tests (Weeks 1-4)
Focus on P0 tests for Opportunity entity, DST, and decision processes.

### Phase 2: Extended Features (Weeks 5-8)
Implement P1 tests for country management, agreements, and AI assistance.

### Phase 3: UI and UX (Weeks 9-12)
Create P2 tests for UI options and document panels.

### Phase 4: Advanced Features (Weeks 13-14)
Complete P3 tests for remaining features.

---

**Document Status:** ✅ Complete  
**Last Updated:** January 13, 2026  
**Next Review:** After Phase 1 test implementation
