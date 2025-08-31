using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Authorization;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class EntityPermissionSeeder
    {
        public static async Task SeedEntityPermissionsAsync(UNOPSAppDbContext context)
        {
            // Check if permissions already exist
            if (await context.EntityPermissions.AnyAsync())
            {
                // Permissions already seeded, no need to run again
                return;
            }

            // Create list of entity permissions to add
            var permissions = new List<EntityPermission>
            {
                // Partner entity permissions,
                new EntityPermission 
                { 
                    Entity = "Partner", 
                    Role = "UNOPS_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "Partner", 
                    Role = "PARTNER_GLOB_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"Status != 2 && Status != 4\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Partner", 
                    Role = "PARTNER_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"PartnerApprovalStatus == 0 && OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit && r.OrganizationHierarchy.Type == 3)\", \"CanDelete\": \"\"}",
                    PropertyFilter = "{\"CanRead\": [], \"CanCreate\": [], \"CanUpdate\": [\"Id\", \"Name\", \"PartnerShortDescription\", \"PartnerLongDescription\", \"PartnerCategoryId\", \"PartnerFocalPointUserId\"], \"CanDelete\": []}",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "Partner", 
                    Role = "ORG_UNIT_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "Partner", 
                    Role = "GMAIL_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = "{\"CanRead\": [\"CreatedBy\", \"LastModifiedBy\", \"CreatedDate\", \"LastModifiedDate\", \"IsDeleted\", \"Status\", \"PartnerGroupCode\", \"PartnerShortDescription\", \"PartnerLongDescription\", \"PartnerCategoryId\", \"ErpDimValue\", \"LiaisonOfficeId\", \"Id\", \"Name\"], \"CanCreate\": [], \"CanUpdate\": [], \"CanDelete\": []}",
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },

                // Contact entity permissions,
                new EntityPermission 
                { 
                    Entity = "Contact", 
                    Role = "UNOPS_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "Contact", 
                    Role = "PARTNER_GLOB_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Contact", 
                    Role = "PARTNER_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"Partner != null && Partner.OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit)\", \"CanUpdate\": \"Partner != null && Partner.OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit)\", \"CanDelete\": \"Partner != null && Partner.OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit)\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Contact", 
                    Role = "ORG_UNIT_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"Partner != null && Partner.OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit)\", \"CanUpdate\": \"Partner != null && Partner.OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit)\", \"CanDelete\": \"Partner != null && Partner.OrganizationUnitRelationships.Any(r => r.Status == 1 && !r.IsDeleted && r.OrganizationHierarchy.Code == @userOrgUnit)\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Contact", 
                    Role = "GMAIL_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = "{\"CanRead\": [\"CreatedBy\", \"LastModifiedBy\", \"CreatedDate\", \"LastModifiedDate\", \"IsDeleted\", \"Status\", \"Partner\", \"PartnerId\", \"FirstName\", \"LastName\", \"Salutation\", \"Title\", \"Description\", \"Phone\", \"Mobile\", \"Email\", \"MailingStreet\", \"MailingCity\", \"MailingStateProvince\", \"MailingPostalCode\", \"MailingCountry\", \"Department\"], \"CanCreate\": [], \"CanUpdate\": [], \"CanDelete\": []}",
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = false,
                    CanDelete = false
                },

                // PartnerTree entity permissions,
                new EntityPermission 
                { 
                    Entity = "PartnerTree", 
                    Role = "UNOPS_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "PartnerTree", 
                    Role = "PARTNER_GLOB_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "PartnerTree", 
                    Role = "PARTNER_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "PartnerTree", 
                    Role = "ORG_UNIT_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "PartnerTree", 
                    Role = "GMAIL_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },

                // Interaction entity permissions,
                new EntityPermission 
                { 
                    Entity = "Interaction", 
                    Role = "UNOPS_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"(OrgUnit != null && OrgUnit.Code == @userOrgUnit) || InteractionUsers.Any(iu => iu.UserId == @currentUserId)\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = false,
                    CanUpdate = true,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "Interaction", 
                    Role = "PARTNER_GLOB_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Interaction", 
                    Role = "PARTNER_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"(OrgUnit != null && OrgUnit.Code == @userOrgUnit) || InteractionUsers.Any(iu => iu.UserId == @currentUserId)\", \"CanDelete\": \"OrgUnit != null && OrgUnit.Code == @userOrgUnit\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Interaction", 
                    Role = "ORG_UNIT_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"(OrgUnit != null && OrgUnit.Code == @userOrgUnit) || InteractionUsers.Any(iu => iu.UserId == @currentUserId)\", \"CanDelete\": \"OrgUnit != null && OrgUnit.Code == @userOrgUnit\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "Interaction", 
                    Role = "GMAIL_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = false
                },

                // UserManagement entity permissions,
                new EntityPermission 
                { 
                    Entity = "UserManagement", 
                    Role = "PARTNER_GLOB_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "UserManagement", 
                    Role = "ORG_UNIT_ADMIN",
                    RowFilter = "{\"CanRead\": \"OrgUnit == @userOrgUnit\", \"CanCreate\": \"OrgUnit == @userOrgUnit\", \"CanUpdate\": \"OrgUnit == @userOrgUnit\", \"CanDelete\": \"OrgUnit == @userOrgUnit\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "UserManagement", 
                    Role = "GMAIL_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },

                // AiPromptManagement entity permissions,
                new EntityPermission 
                { 
                    Entity = "AiPromptManagement", 
                    Role = "UNOPS_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "AiPromptManagement", 
                    Role = "PARTNER_GLOB_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "AiPromptManagement", 
                    Role = "PARTNER_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "AiPromptManagement", 
                    Role = "ORG_UNIT_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "AiPromptManagement", 
                    Role = "GMAIL_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },

                // EntityManager entity permissions,
                new EntityPermission 
                { 
                    Entity = "EntityManager", 
                    Role = "UNOPS_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "EntityManager", 
                    Role = "PARTNER_GLOB_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "EntityManager", 
                    Role = "PARTNER_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "EntityManager", 
                    Role = "ORG_UNIT_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "EntityManager", 
                    Role = "GMAIL_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },

                // EntityFieldManager entity permissions,
                new EntityPermission 
                { 
                    Entity = "EntityFieldManager", 
                    Role = "UNOPS_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "EntityFieldManager", 
                    Role = "PARTNER_GLOB_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = true,
                    CanCreate = true,
                    CanUpdate = true,
                    CanDelete = true
                },
                new EntityPermission 
                { 
                    Entity = "EntityFieldManager", 
                    Role = "PARTNER_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "EntityFieldManager", 
                    Role = "ORG_UNIT_ADMIN",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                },
                new EntityPermission 
                { 
                    Entity = "EntityFieldManager", 
                    Role = "GMAIL_GEN_USER",
                    RowFilter = "{\"CanRead\": \"\", \"CanCreate\": \"\", \"CanUpdate\": \"\", \"CanDelete\": \"\"}",
                    PropertyFilter = null,
                    CanRead = false,
                    CanCreate = false,
                    CanUpdate = false,
                    CanDelete = false
                }
            };

            await context.EntityPermissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
}