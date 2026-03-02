using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using System.Linq;
using System.Collections.Generic;
using UNOPS.PAO.UNOPSDomain.Authorization;

namespace UNOPS.PAO.IntegrationTests.TestData;

/// <summary>
/// Provides methods to seed consistent test data for integration tests
/// </summary>
public static class TestDataSeeder
{
    public static void SeedBasicData(UNOPSAppDbContext context)
    {
        // Ensure we have organizational hierarchy
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
        
        // Note: PartnerCategory doesn't exist in the current model
        // Partners use PartnerGroupCode property instead
        
        // Seed entity permissions for test users
        if (!context.EntityPermissions.Any())
        {
            var permissions = new[]
            {
                new EntityPermission 
                { 
                    Entity = "Partner",
                    Role = "User",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                }
            };
            
            context.EntityPermissions.AddRange(permissions);
        }
        
        context.SaveChanges();
    }
    
    public static UNOPSPartner CreatePartnerWithValidRelations(int? organizationHierarchyId = 1, string? status = "Active")
    {
        var partner = TestDataBuilder.GetPartnerFaker().Generate();
        
        // Map status string to enum
        partner.Status = status switch
        {
            "Active" => Domain.Entities.EntityStatus.Active,
            "Inactive" => Domain.Entities.EntityStatus.Closed,
            "Prospect" => Domain.Entities.EntityStatus.Draft,
            _ => Domain.Entities.EntityStatus.Active
        };
        
        // Ensure required fields are set for enhanced Partner structure
        if (string.IsNullOrEmpty(partner.Name)) partner.Name = "Test Partner";
        if (string.IsNullOrEmpty(partner.PartnerShortDescription)) partner.PartnerShortDescription = "TP";
        if (partner.PartnerCategoryId == 0) partner.PartnerCategoryId = 1;
        if (partner.LiaisonOfficeId == 0) partner.LiaisonOfficeId = 1;
        
        // Enhanced boolean fields (no need to check for string)
        // These are already set by the faker with proper types
        
        // Add organization unit relationship if specified
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
                    Status = Domain.Entities.EntityStatus.Active
                }
            };
        }
        
        return partner;
    }
    
    public static List<UNOPSPartner> CreatePartnersWithOrganizationUnits(int count, params int[] organizationHierarchyIds)
    {
        var partners = new List<UNOPSPartner>();
        var orgIndex = 0;
        
        for (int i = 0; i < count; i++)
        {
            var orgId = organizationHierarchyIds.Length > 0 ? organizationHierarchyIds[orgIndex % organizationHierarchyIds.Length] : 1;
            partners.Add(CreatePartnerWithValidRelations(orgId));
            orgIndex++;
        }
        
        return partners;
    }
    
    public static Contact CreateContactWithValidRelations(int? partnerId = null, string? status = "Active")
    {
        var contact = TestDataBuilder.GetContactFaker().Generate();
        
        // Ensure required fields are set
        if (string.IsNullOrEmpty(contact.Name)) 
            contact.Name = $"{contact.FirstName} {contact.LastName}".Trim();
        if (string.IsNullOrEmpty(contact.LastName)) 
            contact.LastName = "Test Contact";
        if (string.IsNullOrEmpty(contact.Title)) 
            contact.Title = "Test Title";
        if (string.IsNullOrEmpty(contact.Email)) 
            contact.Email = "testcontact@example.com";
        
        // Map status string to enum
        contact.Status = status switch
        {
            "Active" => Domain.Entities.EntityStatus.Active,
            "Inactive" => Domain.Entities.EntityStatus.Closed,
            "Draft" => Domain.Entities.EntityStatus.Draft,
            _ => Domain.Entities.EntityStatus.Active
        };
        
        // Link to partner if specified
        if (partnerId.HasValue)
        {
            contact.PartnerId = partnerId.Value;
        }
        
        return contact;
    }
    
    public static List<Contact> CreateContactsForPartner(int partnerId, int count)
    {
        var contacts = new List<Contact>();
        
        for (int i = 0; i < count; i++)
        {
            contacts.Add(CreateContactWithValidRelations(partnerId));
        }
        
        return contacts;
    }
    
    /// <summary>
    /// Creates an OrganizationUnitRelationship with all required properties
    /// </summary>
    /// <param name="organizationHierarchyId">The organization hierarchy ID</param>
    /// <param name="entityId">The entity ID (Partner, Contact, etc.)</param>
    /// <param name="entityType">The entity type name (e.g., "UNOPSPartner", "Contact")</param>
    /// <param name="status">The status (default: "Active")</param>
    /// <returns>OrganizationUnitRelationship with all required fields set</returns>
    public static OrganizationUnitRelationship CreateOrganizationUnitRelationship(
        int organizationHierarchyId, 
        int entityId, 
        string entityType,
        string? status = "Active")
    {
        var relationship = TestDataBuilder.GetOrganizationUnitRelationshipFaker().Generate();
        
        // Override with provided values
        relationship.OrganizationHierarchyId = organizationHierarchyId;
        relationship.EntityId = entityId;
        relationship.EntityType = entityType;
        
        // Ensure Name is set (required by ModifiableDeletableEntity)
        relationship.Name = $"{entityType}-{entityId}-OrgUnit-{organizationHierarchyId}";
        
        // Map status string to enum
        relationship.Status = status switch
        {
            "Active" => Domain.Entities.EntityStatus.Active,
            "Inactive" => Domain.Entities.EntityStatus.Closed,
            "Draft" => Domain.Entities.EntityStatus.Draft,
            _ => Domain.Entities.EntityStatus.Active
        };
        
        return relationship;
    }
}