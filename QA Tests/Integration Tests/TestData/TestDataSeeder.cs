using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Authorization;
using UNOPS.PAO.UNOPSDataAccess.Context;
using DomainLink = UNOPS.PAO.Domain.Entities.Link;

namespace UNOPS.PAO.IntegrationTests.TestData;

/// <summary>
/// Consolidated test data seeder providing methods to create valid, related entities
/// for both integration and business tests. Uses TestDataBuilder fakers internally
/// and applies required field overrides to ensure entities pass validation.
/// </summary>
public static class TestDataSeeder
{
    // ═══════════════════════════════════════════════════════════════════════
    //  DATABASE SEEDING (for integration tests with DbContext)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Seeds the minimum reference data required by most integration tests:
    /// organization hierarchy, entity permissions, and lookup tables.
    /// </summary>
    public static void SeedBasicData(UNOPSAppDbContext context)
    {
        if (!context.OrganizationHierarchies.Any())
        {
            var orgs = new[]
            {
                new OrganizationHierarchy { Id = 1, Code = "HQ", Name = "Headquarters", Description = "Main HQ" },
                new OrganizationHierarchy { Id = 2, Code = "ROAS", Name = "Regional Office Asia", Description = "Asia Regional Office", ParentId = 1 },
                new OrganizationHierarchy { Id = 3, Code = "ROAF", Name = "Regional Office Africa", Description = "Africa Regional Office", ParentId = 1 },
                new OrganizationHierarchy { Id = 4, Code = "ROEU", Name = "Regional Office Europe", Description = "Europe Regional Office", ParentId = 1 },
                new OrganizationHierarchy { Id = 5, Code = "ROAM", Name = "Regional Office Americas", Description = "Americas Regional Office", ParentId = 1 }
            };
            context.OrganizationHierarchies.AddRange(orgs);
        }

        if (!context.EntityPermissions.Any())
        {
            var permissions = new[]
            {
                new EntityPermission { Entity = "Partner", Role = "User", CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = true },
                new EntityPermission { Entity = "Contact", Role = "User", CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = true },
                new EntityPermission { Entity = "Opportunity", Role = "User", CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = true },
                new EntityPermission { Entity = "Interaction", Role = "User", CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = true },
                new EntityPermission { Entity = "Document", Role = "User", CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = true }
            };
            context.EntityPermissions.AddRange(permissions);
        }

        context.SaveChanges();
    }

    /// <summary>
    /// Seeds a complete set of reference/lookup data needed by risk, opportunity, and SDG tests.
    /// </summary>
    public static void SeedLookupData(UNOPSAppDbContext context)
    {
        SeedBasicData(context);

        if (!context.Set<Currency>().Any())
        {
            context.Set<Currency>().AddRange(
                new Currency { Id = 1, Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2, Status = EntityStatus.Active },
                new Currency { Id = 2, Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2, Status = EntityStatus.Active },
                new Currency { Id = 3, Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2, Status = EntityStatus.Active }
            );
        }

        if (!context.Set<ProposedInitiativeType>().Any())
        {
            context.Set<ProposedInitiativeType>().AddRange(
                new ProposedInitiativeType { Id = 1, Name = "Grant", Description = "Grant funding", Order = 1 },
                new ProposedInitiativeType { Id = 2, Name = "Loan", Description = "Loan financing", Order = 2 },
                new ProposedInitiativeType { Id = 3, Name = "Technical Assistance", Description = "Technical support", Order = 3 }
            );
        }

        if (!context.Set<LiaisonOffice>().Any())
        {
            context.Set<LiaisonOffice>().AddRange(
                new LiaisonOffice { Id = 1, Code = "NYK", Name = "New York Liaison Office", Region = "Americas", Country = "United States", IsActive = true },
                new LiaisonOffice { Id = 2, Code = "GNV", Name = "Geneva Liaison Office", Region = "Europe", Country = "Switzerland", IsActive = true },
                new LiaisonOffice { Id = 3, Code = "TKY", Name = "Tokyo Liaison Office", Region = "Asia Pacific", Country = "Japan", IsActive = true }
            );
        }

        if (!context.Set<RiskType>().Any())
        {
            context.Set<RiskType>().AddRange(
                new RiskType { Id = 1, Name = "Strategic", Code = "STR", DisplayOrder = 1, Status = EntityStatus.Active },
                new RiskType { Id = 2, Name = "Operational", Code = "OPR", DisplayOrder = 2, Status = EntityStatus.Active },
                new RiskType { Id = 3, Name = "Financial", Code = "FIN", DisplayOrder = 3, Status = EntityStatus.Active },
                new RiskType { Id = 4, Name = "Compliance", Code = "CMP", DisplayOrder = 4, Status = EntityStatus.Active },
                new RiskType { Id = 5, Name = "Reputational", Code = "REP", DisplayOrder = 5, Status = EntityStatus.Active }
            );
        }

        if (!context.Set<RiskProbability>().Any())
        {
            context.Set<RiskProbability>().AddRange(
                new RiskProbability { Id = 1, Name = "Rare", Code = "RAR", NumericValue = 1, DisplayOrder = 1, Status = EntityStatus.Active },
                new RiskProbability { Id = 2, Name = "Unlikely", Code = "UNL", NumericValue = 2, DisplayOrder = 2, Status = EntityStatus.Active },
                new RiskProbability { Id = 3, Name = "Possible", Code = "POS", NumericValue = 3, DisplayOrder = 3, Status = EntityStatus.Active },
                new RiskProbability { Id = 4, Name = "Likely", Code = "LIK", NumericValue = 4, DisplayOrder = 4, Status = EntityStatus.Active },
                new RiskProbability { Id = 5, Name = "Almost Certain", Code = "CER", NumericValue = 5, DisplayOrder = 5, Status = EntityStatus.Active }
            );
        }

        if (!context.Set<RiskProximity>().Any())
        {
            context.Set<RiskProximity>().AddRange(
                new RiskProximity { Id = 1, Name = "Immediate", Code = "IMM", MonthsValue = 1, DisplayOrder = 1, Status = EntityStatus.Active },
                new RiskProximity { Id = 2, Name = "Short-term", Code = "SHT", MonthsValue = 3, DisplayOrder = 2, Status = EntityStatus.Active },
                new RiskProximity { Id = 3, Name = "Medium-term", Code = "MED", MonthsValue = 6, DisplayOrder = 3, Status = EntityStatus.Active },
                new RiskProximity { Id = 4, Name = "Long-term", Code = "LNG", MonthsValue = 12, DisplayOrder = 4, Status = EntityStatus.Active }
            );
        }

        if (!context.Set<RiskImpactLevel>().Any())
        {
            context.Set<RiskImpactLevel>().AddRange(
                new RiskImpactLevel { Id = 1, Name = "Negligible", Code = "NEG", NumericValue = 1, DisplayOrder = 1, Status = EntityStatus.Active },
                new RiskImpactLevel { Id = 2, Name = "Minor", Code = "MIN", NumericValue = 2, DisplayOrder = 2, Status = EntityStatus.Active },
                new RiskImpactLevel { Id = 3, Name = "Moderate", Code = "MOD", NumericValue = 3, DisplayOrder = 3, Status = EntityStatus.Active },
                new RiskImpactLevel { Id = 4, Name = "Major", Code = "MAJ", NumericValue = 4, DisplayOrder = 4, Status = EntityStatus.Active },
                new RiskImpactLevel { Id = 5, Name = "Severe", Code = "SEV", NumericValue = 5, DisplayOrder = 5, Status = EntityStatus.Active }
            );
        }

        if (!context.Set<RiskResponseType>().Any())
        {
            context.Set<RiskResponseType>().AddRange(
                new RiskResponseType { Id = 1, Name = "Avoid", Code = "AVO", ValidForThreat = true, ValidForOpportunity = false, DisplayOrder = 1, Status = EntityStatus.Active },
                new RiskResponseType { Id = 2, Name = "Mitigate", Code = "MIT", ValidForThreat = true, ValidForOpportunity = false, DisplayOrder = 2, Status = EntityStatus.Active },
                new RiskResponseType { Id = 3, Name = "Transfer", Code = "TRN", ValidForThreat = true, ValidForOpportunity = false, DisplayOrder = 3, Status = EntityStatus.Active },
                new RiskResponseType { Id = 4, Name = "Accept", Code = "ACC", ValidForThreat = true, ValidForOpportunity = true, DisplayOrder = 4, Status = EntityStatus.Active },
                new RiskResponseType { Id = 5, Name = "Exploit", Code = "EXP", ValidForThreat = false, ValidForOpportunity = true, DisplayOrder = 5, Status = EntityStatus.Active }
            );
        }

        if (!context.Set<EntityRole>().Any())
        {
            context.Set<EntityRole>().AddRange(
                new EntityRole { Id = 1, EntityType = "Opportunity", Name = "Opportunity Manager", IsInternal = true, AllowsMultiple = false },
                new EntityRole { Id = 2, EntityType = "Opportunity", Name = "Business Developer", IsInternal = true, AllowsMultiple = false },
                new EntityRole { Id = 3, EntityType = "Opportunity", Name = "Project Executive", IsInternal = true, AllowsMultiple = false },
                new EntityRole { Id = 4, EntityType = "Partner", Name = "Focal Point", IsInternal = true, AllowsMultiple = false },
                new EntityRole { Id = 5, EntityType = "Opportunity", Name = "Reviewer", IsInternal = true, AllowsMultiple = true }
            );
        }

        if (!context.Set<SDG>().Any())
        {
            var sdgNames = new[] { "No Poverty", "Zero Hunger", "Good Health", "Quality Education", "Gender Equality" };
            for (int i = 0; i < sdgNames.Length; i++)
            {
                context.Set<SDG>().Add(new SDG
                {
                    Id = i + 1,
                    Name = sdgNames[i],
                    SDGNumber = (i + 1).ToString(),
                    SDGId = $"SDG-{i + 1}",
                    Status = EntityStatus.Active
                });
            }
        }

        if (!context.Set<UNOPSMission>().Any())
        {
            context.Set<UNOPSMission>().AddRange(
                new UNOPSMission { Id = 1, Name = "Infrastructure", Code = "INF", DisplayOrder = 1, Status = EntityStatus.Active },
                new UNOPSMission { Id = 2, Name = "Procurement", Code = "PRO", DisplayOrder = 2, Status = EntityStatus.Active },
                new UNOPSMission { Id = 3, Name = "Project Management", Code = "PMG", DisplayOrder = 3, Status = EntityStatus.Active }
            );
        }

        context.SaveChanges();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PARTNER CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static UNOPSPartner CreatePartnerWithValidRelations(int? organizationHierarchyId = 1, string? status = "Active")
    {
        var partner = TestDataBuilder.GetPartnerFaker().Generate();
        partner.Status = MapStatus(status);
        EnsurePartnerRequiredFields(partner);

        if (organizationHierarchyId.HasValue)
        {
            partner.OrganizationUnitRelationships = new List<OrganizationUnitRelationship>
            {
                new OrganizationUnitRelationship
                {
                    OrganizationHierarchyId = organizationHierarchyId.Value,
                    EntityId = partner.Id,
                    EntityType = nameof(UNOPSPartner),
                    Name = $"Partner-{partner.Id}-TestOrgUnit",
                    Status = EntityStatus.Active
                }
            };
        }

        return partner;
    }

    public static List<UNOPSPartner> CreatePartnersWithOrganizationUnits(int count, params int[] organizationHierarchyIds)
    {
        var partners = new List<UNOPSPartner>();
        for (int i = 0; i < count; i++)
        {
            var orgId = organizationHierarchyIds.Length > 0 ? organizationHierarchyIds[i % organizationHierarchyIds.Length] : 1;
            partners.Add(CreatePartnerWithValidRelations(orgId));
        }
        return partners;
    }

    public static UNOPSPartner CreateApprovedPartner(int? erpDimValue = null)
    {
        var partner = CreatePartnerWithValidRelations();
        partner.PartnerApprovalStatus = PartnerApprovalStatus.Approved;
        partner.PartnerApprovalDate = DateTime.UtcNow;
        partner.ErpDimValue = erpDimValue ?? new Random().Next(1000, 7999);
        partner.CanCreateNewOpportunities = true;
        return partner;
    }

    public static UNOPSPartner CreateDeletedPartner()
    {
        var partner = CreatePartnerWithValidRelations();
        partner.IsDeleted = true;
        partner.DeletedDate = DateTime.UtcNow;
        partner.DeletedBy = 1;
        return partner;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  CONTACT CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static Contact CreateContactWithValidRelations(int? partnerId = null, string? status = "Active")
    {
        var contact = TestDataBuilder.GetContactFaker().Generate();
        contact.Status = MapStatus(status);
        EnsureContactRequiredFields(contact);

        if (partnerId.HasValue)
            contact.PartnerId = partnerId.Value;

        return contact;
    }

    public static UNOPSContact CreateUNOPSContact(int? partnerId = null, string? status = "Active")
    {
        var contact = TestDataBuilder.GetUNOPSContactFaker().Generate();
        contact.Status = MapStatus(status);
        EnsureContactRequiredFields(contact);

        if (partnerId.HasValue)
            contact.PartnerId = partnerId.Value;

        return contact;
    }

    public static List<Contact> CreateContactsForPartner(int partnerId, int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateContactWithValidRelations(partnerId))
            .ToList();
    }

    public static UNOPSContact CreateDeletedContact(int? partnerId = null)
    {
        var contact = CreateUNOPSContact(partnerId);
        contact.IsDeleted = true;
        contact.DeletedDate = DateTime.UtcNow;
        contact.DeletedBy = 1;
        return contact;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  INTERACTION CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static Interaction CreateInteraction(InteractionType? type = null, DateTime? date = null)
    {
        var interaction = TestDataBuilder.GetInteractionFaker().Generate();
        if (type.HasValue) interaction.Type = type.Value;
        if (date.HasValue) interaction.Date = date.Value;
        EnsureInteractionRequiredFields(interaction);
        return interaction;
    }

    public static UNOPSInteraction CreateUNOPSInteraction(InteractionType? type = null, DateTime? date = null)
    {
        var interaction = TestDataBuilder.GetUNOPSInteractionFaker().Generate();
        if (type.HasValue) interaction.Type = type.Value;
        if (date.HasValue) interaction.Date = date.Value;
        EnsureInteractionRequiredFields(interaction);
        return interaction;
    }

    public static Interaction CreateInteractionWithParticipants(int partnerId, int contactId, int userId)
    {
        var interaction = CreateInteraction();
        interaction.InteractionPartners = new List<InteractionPartner>
        {
            new InteractionPartner { PartnerId = partnerId }
        };
        interaction.InteractionContacts = new List<InteractionContact>
        {
            new InteractionContact { ContactId = contactId }
        };
        interaction.InteractionUsers = new List<InteractionUser>
        {
            new InteractionUser { UserId = userId }
        };
        return interaction;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  OPPORTUNITY CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static Opportunity CreateOpportunity(string? stage = null, string? status = "Active")
    {
        var opportunity = TestDataBuilder.GetOpportunityFaker().Generate();
        opportunity.Status = MapStatus(status);
        if (stage != null) opportunity.Stage = stage;
        EnsureOpportunityRequiredFields(opportunity);
        return opportunity;
    }

    public static Opportunity CreateOpportunityWithFundingPartners(int fundingPartnerCount, int? partnerId = null)
    {
        var opportunity = CreateOpportunity();
        opportunity.FundingPartners = Enumerable.Range(0, fundingPartnerCount)
            .Select(_ =>
            {
                var fp = TestDataBuilder.GetOpportunityFundingPartnerFaker().Generate();
                if (partnerId.HasValue) fp.PartnerId = partnerId.Value;
                return fp;
            })
            .ToList();
        return opportunity;
    }

    public static Opportunity CreateOpportunityWithStakeholders(int internalCount, int externalCount)
    {
        var opportunity = CreateOpportunity();
        opportunity.Stakeholders = Enumerable.Range(0, internalCount)
            .Select(_ =>
            {
                var s = TestDataBuilder.GetOpportunityStakeholderFaker().Generate();
                s.IsInternal = true;
                return s;
            })
            .ToList();
        opportunity.ExternalStakeholders = Enumerable.Range(0, externalCount)
            .Select(_ => TestDataBuilder.GetOpportunityExternalStakeholderFaker().Generate())
            .ToList();
        return opportunity;
    }

    public static Opportunity CreateFullOpportunity()
    {
        var opportunity = CreateOpportunity();
        opportunity.FundingPartners = TestDataBuilder.GetOpportunityFundingPartnerFaker().Generate(2);
        opportunity.ClientPartners = TestDataBuilder.GetOpportunityClientPartnerFaker().Generate(1);
        opportunity.Stakeholders = TestDataBuilder.GetOpportunityStakeholderFaker().Generate(3);
        opportunity.ExternalStakeholders = TestDataBuilder.GetOpportunityExternalStakeholderFaker().Generate(2);
        opportunity.Deliverables = TestDataBuilder.GetOpportunityDeliverableFaker().Generate(3);
        opportunity.Countries = TestDataBuilder.GetOpportunityCountryFaker().Generate(2);
        opportunity.SDGs = TestDataBuilder.GetOpportunitySDGFaker().Generate(2);
        opportunity.Collaborators = TestDataBuilder.GetOpportunityCollaboratorFaker().Generate(2);
        return opportunity;
    }

    public static Opportunity CreateDeletedOpportunity()
    {
        var opportunity = CreateOpportunity();
        opportunity.IsDeleted = true;
        opportunity.DeletedDate = DateTime.UtcNow;
        opportunity.DeletedBy = 1;
        return opportunity;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DOCUMENT CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static Document CreateDocument(string? entityType = null, int? entityId = null)
    {
        var document = TestDataBuilder.GetDocumentFaker().Generate();
        if (entityType != null && entityId.HasValue)
        {
            document.DocumentRelationships = new List<DocumentRelationship>
            {
                new DocumentRelationship
                {
                    Name = $"Doc-{entityType}-{entityId}",
                    EntityType = entityType,
                    EntityId = entityId.Value,
                    Status = EntityStatus.Active
                }
            };
        }
        return document;
    }

    public static UNOPSDocument CreateUNOPSDocument(bool linkedFile = false)
    {
        var document = TestDataBuilder.GetUNOPSDocumentFaker().Generate();
        document.LinkedFile = linkedFile;
        return document;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  RISK CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static Risk CreateRisk(string? entityType = "Opportunity", int? entityId = null)
    {
        var risk = TestDataBuilder.GetRiskFaker().Generate();
        if (entityType != null) risk.EntityType = entityType;
        if (entityId.HasValue) risk.EntityId = entityId.Value;
        return risk;
    }

    public static List<Risk> CreateRisksForEntity(string entityType, int entityId, int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => CreateRisk(entityType, entityId))
            .ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  AUDIT LOG CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static AuditLog CreateAuditLog(string? entityType = null, int? entityId = null, string? action = null)
    {
        var log = TestDataBuilder.GetAuditLogFaker().Generate();
        if (entityType != null) log.EntityType = entityType;
        if (entityId.HasValue) log.EntityId = entityId.Value;
        if (action != null) log.Action = action;
        return log;
    }

    public static List<AuditLog> CreateAuditTrail(string entityType, int entityId, int count)
    {
        return Enumerable.Range(0, count)
            .Select(i =>
            {
                var log = CreateAuditLog(entityType, entityId);
                log.Timestamp = DateTime.UtcNow.AddHours(-count + i);
                return log;
            })
            .ToList();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  ORGANIZATION & COUNTRY CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static OrganizationHierarchy CreateOrganizationHierarchy(int? parentId = null)
    {
        var org = TestDataBuilder.GetOrganizationHierarchyFaker().Generate();
        org.ParentId = parentId;
        return org;
    }

    public static List<OrganizationHierarchy> CreateOrganizationTree(int childCount)
    {
        var root = CreateOrganizationHierarchy();
        root.Id = 100;
        var children = Enumerable.Range(0, childCount)
            .Select(i =>
            {
                var child = CreateOrganizationHierarchy(root.Id);
                child.Id = 101 + i;
                return child;
            })
            .ToList();

        var result = new List<OrganizationHierarchy> { root };
        result.AddRange(children);
        return result;
    }

    public static Country CreateCountry(string? iso2Code = null)
    {
        var country = TestDataBuilder.GetCountryFaker().Generate();
        if (iso2Code != null) country.Iso2Code = iso2Code;
        return country;
    }

    public static List<Country> CreateCountries(int count)
    {
        return TestDataBuilder.GetCountryFaker().Generate(count);
    }

    public static OrganizationUnitRelationship CreateOrganizationUnitRelationship(
        int organizationHierarchyId,
        int entityId,
        string entityType,
        string? status = "Active")
    {
        var relationship = TestDataBuilder.GetOrganizationUnitRelationshipFaker().Generate();
        relationship.OrganizationHierarchyId = organizationHierarchyId;
        relationship.EntityId = entityId;
        relationship.EntityType = entityType;
        relationship.Name = $"{entityType}-{entityId}-OrgUnit-{organizationHierarchyId}";
        relationship.Status = MapStatus(status);
        return relationship;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  USER & ROLE CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static PAOUser CreateUser(bool isInternal = true)
    {
        var user = TestDataBuilder.GetPAOUserFaker().Generate();
        user.IsInternal = isInternal;
        return user;
    }

    public static UserProfile CreateUserProfile(int userId)
    {
        var profile = TestDataBuilder.GetUserProfileFaker().Generate();
        profile.UserId = userId;
        return profile;
    }

    public static EntityUserRole CreateEntityUserRole(int userId, int entityId, string entityType, int entityRoleId)
    {
        var role = TestDataBuilder.GetEntityUserRoleFaker().Generate();
        role.UserId = userId;
        role.EntityId = entityId;
        role.EntityType = entityType;
        role.EntityRoleId = entityRoleId;
        return role;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  COMMENT & NOTIFICATION CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static Comment CreateComment(string entityType, int entityId, int? userId = null)
    {
        var comment = TestDataBuilder.GetCommentFaker().Generate();
        comment.EntityType = entityType;
        comment.EntityId = entityId;
        if (userId.HasValue) comment.CreatedBy = userId.Value;
        return comment;
    }

    public static Notification CreateNotification(int userId, string? category = null)
    {
        var notification = TestDataBuilder.GetNotificationFaker().Generate();
        notification.UserId = userId;
        if (category != null) notification.Category = category;
        return notification;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  LINK CREATION
    // ═══════════════════════════════════════════════════════════════════════

    public static DomainLink CreateLink(LinkEntityType entityType, int entityId)
    {
        var link = TestDataBuilder.GetLinkFaker().Generate();
        link.Entity = entityType;
        link.EntityId = entityId;
        return link;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  BATCH / SCENARIO CREATION
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a partner with contacts, interactions, and documents for comprehensive testing.
    /// Returns a tuple of (partner, contacts, interactions, documents).
    /// </summary>
    public static (UNOPSPartner Partner, List<Contact> Contacts, List<Interaction> Interactions, List<Document> Documents)
        CreatePartnerWithFullRelationships(int contactCount = 3, int interactionCount = 2, int documentCount = 1)
    {
        var partner = CreatePartnerWithValidRelations();
        var contacts = CreateContactsForPartner(partner.Id, contactCount);
        var interactions = Enumerable.Range(0, interactionCount)
            .Select(_ => CreateInteraction())
            .ToList();
        var documents = Enumerable.Range(0, documentCount)
            .Select(_ => CreateDocument("Partner", partner.Id))
            .ToList();

        return (partner, contacts, interactions, documents);
    }

    /// <summary>
    /// Creates an opportunity with all related entities for end-to-end testing.
    /// Returns a tuple with the opportunity and all related collections.
    /// </summary>
    public static (Opportunity Opportunity, List<Risk> Risks, List<AuditLog> AuditLogs)
        CreateOpportunityWithFullContext(int riskCount = 2, int auditLogCount = 5)
    {
        var opportunity = CreateFullOpportunity();
        var risks = CreateRisksForEntity("Opportunity", opportunity.Id, riskCount);
        var auditLogs = CreateAuditTrail("Opportunity", opportunity.Id, auditLogCount);

        return (opportunity, risks, auditLogs);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static EntityStatus MapStatus(string? status) => status switch
    {
        "Active" => EntityStatus.Active,
        "Inactive" => EntityStatus.Closed,
        "Prospect" => EntityStatus.Draft,
        "Draft" => EntityStatus.Draft,
        "Archived" => EntityStatus.Archived,
        "Closed" => EntityStatus.Closed,
        _ => EntityStatus.Active
    };

    private static void EnsurePartnerRequiredFields(UNOPSPartner partner)
    {
        if (string.IsNullOrEmpty(partner.Name)) partner.Name = "Test Partner";
        if (string.IsNullOrEmpty(partner.PartnerShortDescription)) partner.PartnerShortDescription = "TP";
        if (partner.PartnerCategoryId == 0) partner.PartnerCategoryId = 1;
        if (partner.LiaisonOfficeId == 0) partner.LiaisonOfficeId = 1;
    }

    private static void EnsureContactRequiredFields(Contact contact)
    {
        if (string.IsNullOrEmpty(contact.Name))
            contact.Name = $"{contact.FirstName} {contact.LastName}".Trim();
        if (string.IsNullOrEmpty(contact.LastName))
            contact.LastName = "Test Contact";
        if (string.IsNullOrEmpty(contact.Title))
            contact.Title = "Test Title";
        if (string.IsNullOrEmpty(contact.Email))
            contact.Email = "testcontact@example.com";
    }

    private static void EnsureInteractionRequiredFields(Interaction interaction)
    {
        if (string.IsNullOrEmpty(interaction.Name))
            interaction.Name = "Test Interaction";
        if (string.IsNullOrEmpty(interaction.Subject))
            interaction.Subject = "Test Subject";
    }

    private static void EnsureOpportunityRequiredFields(Opportunity opportunity)
    {
        if (string.IsNullOrEmpty(opportunity.Name))
            opportunity.Name = "Test Opportunity";
        if (string.IsNullOrEmpty(opportunity.Description))
            opportunity.Description = "Test opportunity description for integration testing.";
        if (string.IsNullOrEmpty(opportunity.Stage))
            opportunity.Stage = "IDENTIFY & PROFILE";
    }
}
