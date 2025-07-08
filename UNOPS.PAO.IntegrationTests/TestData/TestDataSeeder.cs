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
    
    public static UNOPSPartner CreatePartnerWithValidRelations(int? officeId = 1, string? status = "Active")
    {
        var partner = TestDataBuilder.GetPartnerFaker().Generate();
        partner.PartnerOfficeId = officeId;
        partner.Status = status ?? "Active";
        
        // Ensure required fields are set
        if (string.IsNullOrEmpty(partner.NewEngagement)) partner.NewEngagement = "false";
        if (string.IsNullOrEmpty(partner.PooledFund)) partner.PooledFund = "false";
        if (string.IsNullOrEmpty(partner.DDRequired)) partner.DDRequired = "false";
        if (string.IsNullOrEmpty(partner.DDEACDone)) partner.DDEACDone = "false";
        if (string.IsNullOrEmpty(partner.LevyPotentiallyApplies)) partner.LevyPotentiallyApplies = "false";
        
        return partner;
    }
    
    public static List<UNOPSPartner> CreatePartnersWithOffices(int count, params int[] officeIds)
    {
        var partners = new List<UNOPSPartner>();
        var officeIndex = 0;
        
        for (int i = 0; i < count; i++)
        {
            var officeId = officeIds.Length > 0 ? officeIds[officeIndex % officeIds.Length] : 1;
            partners.Add(CreatePartnerWithValidRelations(officeId));
            officeIndex++;
        }
        
        return partners;
    }
}