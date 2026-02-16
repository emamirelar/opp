SELECT
    o."Id"
    , o."Name"
    , (
        JSONB_BUILD_OBJECT(
            'Id', o."Id"
            , 'Name', o."Name"
            , 'Description', o."Description"
            , 'PartnerReference', o."PartnerReference"
            , 'Stage', o."Stage"
            , 'ResponsibleOrgUnitId', o."ResponsibleOrgUnitId"
            , 'ResponsibleOrgUnitCode', roh."Code"
            , 'ResponsibleOrgUnitName', roh."Name"
            , 'ResponsibleOrgUnitType', roh."Type"
            , 'ResponsibleOrgUnitDescription', roh."Description"
            , 'InitiativeBudgetUSD', o."InitiativeBudgetUSD"
            , 'TargetSigningDate', TO_CHAR(o."TargetSigningDate", 'YYYY-MM-DD')
            , 'ImplementationStartDate', TO_CHAR(o."ImplementationStartDate", 'YYYY-MM-DD')
            , 'TargetDeliveryDate', TO_CHAR(o."TargetDeliveryDate", 'YYYY-MM-DD')
            , 'IsTargetSigningDateFirm', o."IsTargetSigningDateFirm"
            , 'SigningDateNotes', o."SigningDateNotes"
            , 'SubmissionDeadline', o."SubmissionDeadline"
            , 'ProposedInitiativeTypeId', o."ProposedInitiativeTypeId"
            , 'ResultsFocus', o."ResultsFocus"
            , 'ExpectedImpact', o."ExpectedImpact"
            , 'ExpectedOutcomes', o."ExpectedOutcomes"
            , 'ExpectedBeneficiaries', o."ExpectedBeneficiaries"
            , 'EstimatedDirectBeneficiaries', o."EstimatedDirectBeneficiaries"
            , 'EstimatedIndirectBeneficiaries', o."EstimatedIndirectBeneficiaries"
            , 'BeneficiariesToBeDetermined', o."BeneficiariesToBeDetermined"
            , 'Challenges', o."Challenges"
            , 'IsPooledFunding', o."IsPooledFunding"
            , 'HighRisksAcknowledged', o."HighRisksAcknowledged"
            , 'DeliveryModality', o."DeliveryModality"
            , 'MiscExternalStakeholders', o."MiscExternalStakeholders"
            , 'ExternalStakeholderNotes', o."ExternalStakeholderNotes"
        ) || JSONB_BUILD_OBJECT(
            'CreatedBy', o."CreatedBy"
            , 'CreatedDate', o."CreatedDate"
            , 'CreatedByEmail', (
                SELECT anu."Email"
                FROM "AspNetUsers" anu
                WHERE anu."Id" = o."CreatedBy"
            )
            , 'LastModifiedBy', o."LastModifiedBy"
            , 'LastModifiedDate', o."LastModifiedDate"
            , 'LastModifiedByEmail', (
                SELECT anu."Email"
                FROM "AspNetUsers" anu
                WHERE anu."Id" = o."LastModifiedBy"
            )
            , 'OpportunityManagerEmail', (
                SELECT anu."Email"
                FROM "OpportunityStakeholders" os
                INNER JOIN "EntityRoles" er ON os."EntityRoleId" = er."Id"
                LEFT JOIN "AspNetUsers" anu ON anu."Id" = os."UserId"
                WHERE os."OpportunityId" = o."Id"
                    AND er."Code" = 'Opportunity_Manager_Opportunity'
                    AND os."IsDeleted" = false
                LIMIT 1
            )
            , 'ExecutiveEmail', (
                SELECT anu."Email"
                FROM "AspNetUsers" anu
                WHERE anu."Id" = o."ExecutiveId"
            )
            , 'DoA2Email', (
                SELECT anu."Email"
                FROM workflow."WorkflowLogs" wl
                LEFT JOIN "AspNetUsers" anu ON anu."Id" = CAST(wl."UserId" AS INTEGER)
                WHERE wl."EntityName" = 'Opportunity'
                    AND wl."EntityId" = CAST(o."Id" AS TEXT)
                    AND wl."NewStage" = 'GO'
                    AND LOWER(wl."Action") = 'approve'
                ORDER BY wl."CreatedDate" DESC
                LIMIT 1
            )
            , 'DoA3Email', (
                SELECT anu."Email"
                FROM "EntityUserRoles" eur
                LEFT JOIN "AspNetUsers" anu ON eur."UserId" = anu."Id"
                WHERE eur."EntityType" = 'OrganizationHierarchy'
                    AND eur."EntityId" = o."ResponsibleOrgUnitId"
                    AND eur."Name" LIKE 'DoA3%OrganizationHierarchy%'
                    AND eur."IsDeleted" = false
                    AND eur."UserId" IS NOT NULL
                ORDER BY eur."LastModifiedDate" DESC
                LIMIT 1
            )
            , 'NotificationCcEmails', (
                -- Get CC list: Workflow initiator (last Submit) + Org Unit Managers/Directors
                WITH workflow_initiator AS (
                    SELECT anu."Email" as email
                    FROM workflow."WorkflowLogs" wl
                    LEFT JOIN "AspNetUsers" anu ON anu."Id" = CAST(wl."UserId" AS INTEGER)
                    WHERE wl."EntityName" = 'Opportunity'
                        AND wl."EntityId" = CAST(o."Id" AS TEXT)
                        AND LOWER(wl."Action") = 'submit'
                    ORDER BY wl."CompletedOn" DESC
                    LIMIT 1
                ),
                org_managers AS (
                    SELECT anu."Email" as email
                    FROM "EntityUserRoles" eur
                    LEFT JOIN "AspNetUsers" anu ON eur."UserId" = anu."Id"
                    WHERE eur."EntityType" = 'OrganizationHierarchy'
                        AND eur."EntityId" = o."ResponsibleOrgUnitId"
                        AND eur."Name" NOT LIKE '%DoA%'
                        AND eur."IsDeleted" = false
                        AND eur."UserId" IS NOT NULL
                        AND anu."Email" IS NOT NULL
                )
                SELECT STRING_AGG(DISTINCT email, ';')
                FROM (
                    SELECT email FROM workflow_initiator
                    UNION
                    SELECT email FROM org_managers
                ) all_cc_emails
            )
        ) || JSONB_BUILD_OBJECT(
            'Countries', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'CountryId', oc."CountryId"
                        , 'Iso2Code', c."Iso2Code"
                        , 'Iso3Code', c."Iso3Code"
                        , 'Name', c."Name"
                        , 'SpecificAreas', oc."SpecificAreas"
                        , 'ContextWarning', oc."ContextWarning"
                        , 'RiskScore', oc."RiskScore"
                        , 'HumanitarianFrameworkAlignment', oc."HumanitarianFrameworkAlignment"
                        , 'NdcAlignment', oc."NdcAlignment"
                        , 'NapAlignment', oc."NapAlignment"
                        , 'OrgUnitStrategyAlignment', oc."OrgUnitStrategyAlignment"
                    )
                )
                FROM "OpportunityCountries" oc
                INNER JOIN "Countries" c ON oc."CountryId" = c."Id"
                WHERE oc."OpportunityId" = o."Id" AND oc."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'FundingPartners', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'PartnerId', ofp."PartnerId"
                        , 'PartnerName', p."Name"
                        , 'PartnerShortDescription', p."PartnerShortDescription"
                        , 'PartnerErpDimValue', p."ErpDimValue"
                        , 'PooledFund', p."PooledFund"
                        , 'Amount', ofp."Amount"
                        , 'CurrencyId', ofp."CurrencyId"
                        , 'CurrencyCode', cur."Code"
                        , 'CurrencyName', cur."Name"
                        , 'CurrencySymbol', cur."Symbol"
                        , 'Percentage', ofp."Percentage"
                        , 'FeePercentage', ofp."FeePercentage"
                        , 'FeeAmount', ofp."FeeAmount"
                        , 'FeeAmountUSD', ofp."FeeAmountUSD"
                        , 'IsAmountBasedFee', ofp."IsAmountBasedFee"
                        , 'PartnershipAgreementReference', ofp."PartnershipAgreementReference"
                        , 'CommitmentStatus', ofp."CommitmentStatus"
                        , 'AmountUSD', ofp."AmountUSD"
                        , 'ExchangeRate', ofp."ExchangeRate"
                        , 'ExchangeRateDate', ofp."ExchangeRateDate"
                        , 'IsPooledContribution', ofp."IsPooledContribution"
                        , 'SelectedPartnerAgreementNumber', ofp."SelectedPartnerAgreementNumber"
                    )
                )
                FROM "OpportunityFundingPartners" ofp
                INNER JOIN "Partners" p ON ofp."PartnerId" = p."Id"
                LEFT JOIN "Currencies" cur ON ofp."CurrencyId" = cur."Id"
                WHERE ofp."OpportunityId" = o."Id" AND ofp."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'ClientPartners', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'PartnerId', ocp."PartnerId"
                        , 'PartnerName', p."Name"
                        , 'PartnerShortDescription', p."PartnerShortDescription"
                        , 'PartnerErpDimValue', p."ErpDimValue"
                        , 'PooledFund', p."PooledFund"
                        , 'SelectedPartnerAgreementNumber', ocp."SelectedPartnerAgreementNumber"
                    )
                )
                FROM "OpportunityClientPartners" ocp
                INNER JOIN "Partners" p ON ocp."PartnerId" = p."Id"
                WHERE ocp."OpportunityId" = o."Id" AND ocp."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'Deliverables', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'DeliverableId', od."Id"
                        , 'OutputId', od."OutputId"
                        , 'OutputLevel', CASE 
                            WHEN out."Level4" IS NOT NULL THEN out."Level4"
                            WHEN out."Level3" IS NOT NULL THEN out."Level3"
                            WHEN out."Level2" IS NOT NULL THEN out."Level2"
                            WHEN out."Level1" IS NOT NULL THEN out."Level1"
                            WHEN out."Level0" IS NOT NULL THEN out."Level0"
                            ELSE NULL
                        END
                        , 'OutputDefinition', CASE 
                            WHEN out."Level4" IS NOT NULL THEN out."DefinitionLevel4"
                            WHEN out."Level3" IS NOT NULL THEN out."DefinitionLevel3"
                            WHEN out."Level2" IS NOT NULL THEN out."DefinitionLevel2"
                            WHEN out."Level1" IS NOT NULL THEN out."DefinitionLevel1"
                            ELSE NULL
                        END
                        , 'OutputLevel0', out."Level0"
                        , 'OutputLevel1', out."Level1"
                        , 'OutputLevel2', out."Level2"
                        , 'OutputLevel3', out."Level3"
                        , 'OutputLevel4', out."Level4"
                        , 'DefinitionLevel1', out."DefinitionLevel1"
                        , 'DefinitionLevel2', out."DefinitionLevel2"
                        , 'DefinitionLevel3', out."DefinitionLevel3"
                        , 'DefinitionLevel4', out."DefinitionLevel4"
                        , 'ServiceLine', out."ServiceLine"
                        , 'GrantSupportImplementingModality', out."GrantSupportImplementingModality"
                        , 'GrantSupportComponent', out."GrantSupportComponent"
                        , 'ProcurementComponent', out."ProcurementComponent"
                        , 'ProcurementInstallationComponent', out."ProcurementInstallationComponent"
                        , 'InfrastructureComponent', out."InfrastructureComponent"
                        , 'Quantity', od."Quantity"
                        , 'Notes', od."Notes"
                        , 'SequenceOrder', od."SequenceOrder"
                        , 'PlannedStartDate', od."PlannedStartDate"
                        , 'PlannedEndDate', od."PlannedEndDate"
                    )
                )
                FROM "OpportunityDeliverables" od
                LEFT JOIN "Outputs" out ON od."OutputId" = out."Id"
                WHERE od."OpportunityId" = o."Id" AND od."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'SDGs', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'OpportunitySDGId', osdg."Id"
                        , 'SDGId', osdg."SDGId"
                        , 'SDGNumber', sdg."SDGNumber"
                        , 'SDGName', sdg."Name"
                        , 'SDGDescription', sdg."SDGDescription"
                        , 'SDGLongDescription', sdg."SDGLongDescription"
                        , 'IsPrimary', osdg."IsPrimary"
                        , 'SkipTargetsAndIndicators', osdg."SkipTargetsAndIndicators"
                        , 'Notes', osdg."Notes"
                    )
                )
                FROM "OpportunitySDGs" osdg
                INNER JOIN "SDGs" sdg ON osdg."SDGId" = sdg."Id"
                WHERE osdg."OpportunityId" = o."Id" AND osdg."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'SDGTargets', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'OpportunitySDGTargetId', osdgt."Id"
                        , 'OpportunityId', osdgt."OpportunityId"
                        , 'OpportunitySDGId', osdgt."OpportunitySDGId"
                        , 'SDGTargetId', osdgt."SDGTargetId"
                        , 'SDGTargetName', sdgt."Name"
                        , 'SDGTargetExternalId', sdgt."SDGTargetId"
                        , 'TargetDescription', sdgt."TargetDescription"
                        , 'TargetType', sdgt."TargetType"
                        , 'Notes', osdgt."Notes"
                    )
                )
                FROM "OpportunitySDGTargets" osdgt
                LEFT JOIN "SDGTargets" sdgt ON osdgt."SDGTargetId" = sdgt."Id"
                WHERE osdgt."OpportunityId" = o."Id" AND osdgt."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'SDGIndicators', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'OpportunitySDGIndicatorId', osdgi."Id"
                        , 'OpportunityId', osdgi."OpportunityId"
                        , 'OpportunitySDGTargetId', osdgi."OpportunitySDGTargetId"
                        , 'SDGIndicatorId', osdgi."SDGIndicatorId"
                        , 'SDGIndicatorName', sdgi."Name"
                        , 'SDGIndicatorExternalId', sdgi."SDGIndicatorId"
                        , 'SDGIndicatorLongDescription', sdgi."SDGIndicatorLongDescription"
                        , 'Notes', osdgi."Notes"
                    )
                )
                FROM "OpportunitySDGIndicators" osdgi
                LEFT JOIN "SDGIndicators" sdgi ON osdgi."SDGIndicatorId" = sdgi."Id"
                WHERE osdgi."OpportunityId" = o."Id" AND osdgi."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'Collaborators', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'CollaboratorId', oc."Id"
                        , 'UserId', oc."UserId"
                        , 'UserEmail', pau."Email"
                        , 'UserFirstName', up."FirstName"
                        , 'UserLastName', up."LastName"
                        , 'UserName', COALESCE(up."Name", CONCAT(up."FirstName", ' ', up."LastName"), pau."Email")
                        , 'AddedDate', oc."AddedDate"
                        , 'AddedBy', oc."AddedBy"
                        , 'Expertises', COALESCE((
                            SELECT JSONB_AGG(
                                JSONB_BUILD_OBJECT(
                                    'OpportunityCollaboratorExpertiseId', oce."Id"
                                    , 'CollaboratorExpertiseId', oce."CollaboratorExpertiseId"
                                    , 'ExpertiseCode', ce."Code"
                                    , 'ExpertiseName', ce."Name"
                                    , 'ExpertiseDescription', ce."Description"
                                    , 'DisplayOrder', ce."DisplayOrder"
                                )
                            )
                            FROM "OpportunityCollaboratorExpertises" oce
                            INNER JOIN "CollaboratorExpertises" ce ON oce."CollaboratorExpertiseId" = ce."Id"
                            WHERE oce."OpportunityCollaboratorId" = oc."Id" AND oce."IsDeleted" = false
                        ), '[]'::jsonb)
                    )
                )
                FROM "OpportunityCollaborators" oc
                INNER JOIN "AspNetUsers" pau ON oc."UserId" = pau."Id"
                LEFT JOIN "UserProfile" up ON pau."Id" = up."UserId"
                WHERE oc."OpportunityId" = o."Id" AND oc."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'Stakeholders', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'StakeholderId', os."Id"
                        , 'EntityRoleId', os."EntityRoleId"
                        , 'EntityRoleName', er."Name"
                        , 'EntityRoleCode', er."Code"
                        , 'EntityRoleDescription', er."Description"
                        , 'EntityRoleType', er."Type"
                        , 'IsInternal', os."IsInternal"
                        , 'StakeholderType', CASE WHEN os."IsInternal" = true THEN 'Internal' ELSE 'External' END
                        , 'UserId', os."UserId"
                        , 'UserEmail', anu."Email"
                        , 'UserName', CONCAT(up."FirstName", ' ', up."LastName")
                        , 'UserFirstName', up."FirstName"
                        , 'UserLastName', up."LastName"
                        , 'OrganizationHierarchyId', os."OrganizationHierarchyId"
                        , 'OrganizationHierarchyCode', oh."Code"
                        , 'OrganizationHierarchyName', oh."Name"
                        , 'OrganizationHierarchyType', oh."Type"
                        , 'OrganizationHierarchyDescription', oh."Description"
                        , 'Notes', os."Notes"
                    )
                )
                FROM "OpportunityStakeholders" os
                INNER JOIN "EntityRoles" er ON os."EntityRoleId" = er."Id"
                LEFT JOIN "UserProfile" up ON os."UserId" = up."Id"
                LEFT JOIN "AspNetUsers" anu ON up."UserId" = anu."Id"
                LEFT JOIN "OrganizationHierarchies" oh ON os."OrganizationHierarchyId" = oh."Id"
                WHERE os."OpportunityId" = o."Id" AND os."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'ExternalStakeholders', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'OpportunityExternalStakeholderId', oes."Id"
                        , 'ContactId', oes."ContactId"
                        , 'ContactFirstName', c."FirstName"
                        , 'ContactLastName', c."LastName"
                        , 'ContactEmail', c."Email"
                        , 'ContactTitle', c."Title"
                        , 'ContactPhone', c."Phone"
                        , 'ContactMobile', c."Mobile"
                        , 'ContactDepartment', c."Department"
                        , 'ContactDescription', c."Description"
                        , 'PartnerId', c."PartnerId"
                        , 'PartnerName', p."Name"
                    )
                )
                FROM "OpportunityExternalStakeholder" oes
                INNER JOIN "Contacts" c ON oes."ContactId" = c."Id"
                LEFT JOIN "Partners" p ON c."PartnerId" = p."Id"
                WHERE oes."OpportunityId" = o."Id" AND oes."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'UNCFOutcomes', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'OpportunityUNCFOutcomeId', ouo."Id"
                        , 'OpportunityId', ouo."OpportunityId"
                        , 'OpportunityCountryId', ouo."OpportunityCountryId"
                        , 'UNCFOutcomeId', ouo."UNCFOutcomeId"
                        , 'UNCFOutcomeName', uncfo."Name"
                        , 'UNCFOutcomeExternalId', uncfo."UNCFOutcomeId"
                        , 'UNCooperationFrameworkVersionNo', uncfo."UNCooperationFrameworkVersionNo"
                        , 'Country', uncfo."Country"
                        , 'UNCFOutcomeLastUpdatedDate', uncfo."UNCFOutcomeLastUpdatedDate"
                        , 'Notes', ouo."Notes"
                    )
                )
                FROM "OpportunityUNCFOutcomes" ouo
                LEFT JOIN "UNCFOutcomes" uncfo ON ouo."UNCFOutcomeId" = uncfo."Id"
                WHERE ouo."OpportunityId" = o."Id" AND ouo."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'UNCFIndicators', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'OpportunityUNCFIndicatorId', oui."Id"
                        , 'OpportunityId', oui."OpportunityId"
                        , 'OpportunityUNCFOutcomeId', oui."OpportunityUNCFOutcomeId"
                        , 'UNCFIndicatorId', oui."UNCFIndicatorId"
                        , 'UNCFIndicatorName', uncfi."Name"
                        , 'UNCFIndicatorExternalId', uncfi."UNCFIndicatorId"
                        , 'Unit', uncfi."Unit"
                        , 'Description', uncfi."Description"
                        , 'Indicators', uncfi."Indicators"
                        , 'Baseline', uncfi."Baseline"
                        , 'Narrative', uncfi."Narrative"
                        , 'UNCFIndicatorStartDate', uncfi."UNCFIndicatorStartDate"
                        , 'UNCFIndicatorEndDate', uncfi."UNCFIndicatorEndDate"
                        , 'UNCooperationFrameworkVersionNo', uncfi."UNCooperationFrameworkVersionNo"
                        , 'UNCFOutcomeExternalId', uncfi."UNCFOutcomeExternalId"
                        , 'Country', uncfi."Country"
                        , 'Notes', oui."Notes"
                    )
                )
                FROM "OpportunityUNCFIndicators" oui
                LEFT JOIN "UNCFIndicators" uncfi ON oui."UNCFIndicatorId" = uncfi."Id"
                WHERE oui."OpportunityId" = o."Id" AND oui."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'UNOPSMissions', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'OpportunityUNOPSMissionId', oum."Id"
                        , 'UNOPSMissionId', oum."UNOPSMissionId"
                        , 'MissionName', um."Name"
                        , 'MissionCode', um."Code"
                        , 'MissionDescription', um."Description"
                        , 'DisplayOrder', um."DisplayOrder"
                    )
                )
                FROM "OpportunityUNOPSMissions" oum
                INNER JOIN "UNOPSMissions" um ON oum."UNOPSMissionId" = um."Id"
                WHERE oum."OpportunityId" = o."Id" AND oum."IsDeleted" = false
            ), '[]'::jsonb)
            
            , 'Risks', COALESCE((
                SELECT JSONB_AGG(
                    JSONB_BUILD_OBJECT(
                        'RiskId', r."Id"
                        , 'EntityType', r."EntityType"
                        , 'EntityId', r."EntityId"
                        , 'Title', r."Title"
                        , 'Description', r."Description"
                        , 'Recommendation', r."Recommendation"
                        , 'RiskTypeId', r."RiskTypeId"
                        , 'RiskCategoryId', r."RiskCategoryId"
                        , 'RiskProbabilityId', r."RiskProbabilityId"
                        , 'RiskProximityId', r."RiskProximityId"
                        , 'RiskImpactLevelId', r."RiskImpactLevelId"
                        , 'RiskResponseTypeId', r."RiskResponseTypeId"
                        , 'RiskStatus', r."RiskStatus"
                        , 'IdentifiedDate', r."IdentifiedDate"
                        , 'IdentifiedBy', r."IdentifiedBy"
                        , 'PreDefinedHighRiskId', r."PreDefinedHighRiskId"
                        , 'OupQuestionId', phr."OupQuestionId"
                        , 'IsDeleted', r."IsDeleted"
                        , 'CreatedBy', r."CreatedBy"
                        , 'CreatedDate', r."CreatedDate"
                        , 'LastModifiedBy', r."LastModifiedBy"
                        , 'LastModifiedDate', r."LastModifiedDate"
                    )
                )
                FROM "Risks" r
                LEFT JOIN "PreDefinedHighRisks" phr ON r."PreDefinedHighRiskId" = phr."Id"
                WHERE r."EntityType" = 'Opportunity'
                    AND r."EntityId" = o."Id"
                    AND r."IsDeleted" = false
            ), '[]'::jsonb)
        )
    ) AS "Data"
    , GREATEST(
        COALESCE(o."LastModifiedDate", o."CreatedDate"),                    
        COALESCE(r_dates.max_date, o."LastModifiedDate", o."CreatedDate"),   
        COALESCE(countries_dates.max_date, o."LastModifiedDate", o."CreatedDate"),
        COALESCE(funding_dates.max_date, o."LastModifiedDate", o."CreatedDate"), 
        COALESCE(client_dates.max_date, o."LastModifiedDate", o."CreatedDate"), 
        COALESCE(collab_dates.max_date, o."LastModifiedDate", o."CreatedDate"), 
        COALESCE(collab_exp_dates.max_date, o."LastModifiedDate", o."CreatedDate"),
        COALESCE(deliverable_dates.max_date, o."LastModifiedDate", o."CreatedDate"),
        COALESCE(stakeholder_dates.max_date, o."LastModifiedDate", o."CreatedDate"),
        COALESCE(sdg_dates.max_date, o."LastModifiedDate", o."CreatedDate"),  
        COALESCE(sdg_target_dates.max_date, o."LastModifiedDate", o."CreatedDate"),
        COALESCE(sdg_ind_dates.max_date, o."LastModifiedDate", o."CreatedDate"), 
        COALESCE(uncf_out_dates.max_date, o."LastModifiedDate", o."CreatedDate"), 
        COALESCE(uncf_ind_dates.max_date, o."LastModifiedDate", o."CreatedDate"), 
        COALESCE(mission_dates.max_date, o."LastModifiedDate", o."CreatedDate"), 
        COALESCE(interaction_dates.max_date, o."LastModifiedDate", o."CreatedDate"),
        COALESCE(ext_stake_dates.max_date, o."LastModifiedDate", o."CreatedDate") 
    ) AS "LastModifiedDate"
FROM
    "Opportunities" o
    LEFT JOIN "OrganizationHierarchies" roh ON o."ResponsibleOrgUnitId" = roh."Id"

    LEFT JOIN (
        SELECT "EntityId" AS "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "Risks"
        WHERE "EntityType" = 'Opportunity' AND "IsDeleted" = false
        GROUP BY "EntityId"
    ) r_dates ON r_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityCountries"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) countries_dates ON countries_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityFundingPartners"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) funding_dates ON funding_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityClientPartners"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) client_dates ON client_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityCollaborators"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) collab_dates ON collab_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityCollaboratorExpertises"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) collab_exp_dates ON collab_exp_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityDeliverables"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) deliverable_dates ON deliverable_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityStakeholders"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) stakeholder_dates ON stakeholder_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunitySDGs"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) sdg_dates ON sdg_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunitySDGTargets"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) sdg_target_dates ON sdg_target_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunitySDGIndicators"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) sdg_ind_dates ON sdg_ind_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityUNCFOutcomes"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) uncf_out_dates ON uncf_out_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityUNCFIndicators"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) uncf_ind_dates ON uncf_ind_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityUNOPSMissions"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) mission_dates ON mission_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityInteractions"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) interaction_dates ON interaction_dates."OpportunityId" = o."Id"
    
    LEFT JOIN (
        SELECT "OpportunityId", MAX("LastModifiedDate") AS max_date
        FROM "OpportunityExternalStakeholder"
        WHERE "IsDeleted" = false
        GROUP BY "OpportunityId"
    ) ext_stake_dates ON ext_stake_dates."OpportunityId" = o."Id"
    
WHERE
    1 = 1
    AND o."IsDeleted" = false
    AND o."Stage" = 'GO'