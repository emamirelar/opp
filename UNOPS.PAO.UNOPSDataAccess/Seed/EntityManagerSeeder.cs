using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    public static class EntityManagerSeeder
    {
        public static async Task SeedEntityManagersAsync(UNOPSAppDbContext context)
        {
            if (await context.EntityManagers.AnyAsync())
            {
                return;
            }

            var entityManagers = new List<EntityManager>
            {
                new EntityManager
                {
                    EntityName = "Contact",
                    TableName = "Contacts",
                    Description = "Individual contact persons associated with partners",
                    IsActive = true,
                    EnableChangeLog = false,
                    Name = "Contact",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityManager
                {
                    EntityName = "Partner",
                    TableName = "Partners",
                    Description = "Organizations and entities that work with UNOPS",
                    IsActive = true,
                    EnableChangeLog = false,
                    Name = "Partner",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityManager
                {
                    EntityName = "Interaction",
                    TableName = "Interactions",
                    Description = "Communication and interaction records between UNOPS and partners/contacts",
                    IsActive = true,
                    EnableChangeLog = false,
                    Name = "Interaction",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityManager
                {
                    EntityName = "PartnerTree",
                    TableName = "PartnerTrees",
                    Description = "Hierarchical structure and classification of partners",
                    IsActive = true,
                    EnableChangeLog = false,
                    Name = "PartnerTree",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityManager
                {
                    EntityName = "OrganizationHierarchy",
                    TableName = "OrganizationHierarchies",
                    Description = "UNOPS organizational hierarchy and office structure",
                    IsActive = true,
                    EnableChangeLog = false,
                    Name = "OrganizationHierarchy",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                }
            };

            await context.EntityManagers.AddRangeAsync(entityManagers);
            await context.SaveChangesAsync();
        }

        public static async Task SeedEntityFieldManagersAsync(UNOPSAppDbContext context)
        {
            if (await context.EntityFieldManagers.AnyAsync())
            {
                return;
            }

            var entityFieldManagers = new List<EntityFieldManager>();

            // Contact Entity Fields (EntityManagerId = 1)
            entityFieldManagers.AddRange(new[]
            {
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "ProfilePictureUrl",
                    DataType = "string",
                    Description = "URL to contact profile picture",
                    IsRequired = false,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 500,
                    DisplayOrder = 1,
                    ShowInListView = true,
                    ListViewOrder = 1,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "profilePictureUrl",
                    DisplayTemplate = null,
                    ListViewLabel = "Photo",
                    ListViewType = "avatar",
                    ListViewWidth = "8%",
                    ListViewEllipsis = false,
                    ListViewSortable = false,
                    FirstLetterFallbackField = "firstName",
                    HelperText = "Contact profile photo",
                    Name = "ProfilePictureUrl",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "Partner",
                    DataType = "Partner",
                    Description = "Associated partner organization",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = null,
                    DisplayOrder = 2,
                    ShowInListView = true,
                    ListViewOrder = 2,
                    RelatedDisplayProperty = "name",
                    DisplayFieldPath = "partnerName",
                    DisplayTemplate = null,
                    ListViewLabel = "Partner",
                    ListViewType = "text",
                    ListViewWidth = "20%",
                    ListViewEllipsis = true,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = "Partner organization",
                    Name = "Partner",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "ContactName",
                    DataType = "string",
                    Description = "Full contact name combining first, middle, and last names",
                    IsRequired = false,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = null,
                    DisplayOrder = 3,
                    ShowInListView = true,
                    ListViewOrder = 3,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "firstName,middleName,lastName",
                    DisplayTemplate = "{firstName} {middleName} {lastName}",
                    ListViewLabel = "Contact Name",
                    ListViewType = "template",
                    ListViewWidth = "25%",
                    ListViewEllipsis = true,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = "Complete name of the contact",
                    Name = "ContactName",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "Title",
                    DataType = "string",
                    Description = "Job title or position",
                    IsRequired = false,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 150,
                    DisplayOrder = 4,
                    ShowInListView = true,
                    ListViewOrder = 4,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "title",
                    DisplayTemplate = null,
                    ListViewLabel = "Title",
                    ListViewType = "text",
                    ListViewWidth = "15%",
                    ListViewEllipsis = true,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = "Job title or position",
                    Name = "Title",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "CreatedByName",
                    DataType = "string",
                    Description = "Name of user who created this contact",
                    IsRequired = false,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 200,
                    DisplayOrder = 5,
                    ShowInListView = true,
                    ListViewOrder = 5,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "createdByName",
                    DisplayTemplate = null,
                    ListViewLabel = "Created By",
                    ListViewType = "text",
                    ListViewWidth = "15%",
                    ListViewEllipsis = true,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = "User who created this contact",
                    Name = "CreatedByName",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "CreatedByOfficeName",
                    DataType = "string",
                    Description = "Office name of user who created this contact",
                    IsRequired = false,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 200,
                    DisplayOrder = 6,
                    ShowInListView = true,
                    ListViewOrder = 6,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "createdByOfficeName",
                    DisplayTemplate = null,
                    ListViewLabel = "Created By Office",
                    ListViewType = "text",
                    ListViewWidth = "17%",
                    ListViewEllipsis = true,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = "Office of the user who created this contact",
                    Name = "CreatedByOfficeName",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                }
            });

            // Add non-list view Contact fields
            entityFieldManagers.AddRange(GetContactNonListViewFields());

            // Partner Entity Fields (EntityManagerId = 2)
            entityFieldManagers.AddRange(GetPartnerFields());

            // Interaction Entity Fields (EntityManagerId = 3)
            entityFieldManagers.AddRange(GetInteractionFields());

            // PartnerTree Entity Fields (EntityManagerId = 4)
            entityFieldManagers.AddRange(GetPartnerTreeFields());

            // OrganizationHierarchy Entity Fields (EntityManagerId = 5)
            entityFieldManagers.AddRange(GetOrganizationHierarchyFields());

            await context.EntityFieldManagers.AddRangeAsync(entityFieldManagers);
            await context.SaveChangesAsync();
        }

        private static List<EntityFieldManager> GetContactNonListViewFields()
        {
            return new List<EntityFieldManager>
            {
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "Id",
                    DataType = "int",
                    Description = "Unique identifier for the contact",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = null,
                    DisplayOrder = 7,
                    ShowInListView = false,
                    ListViewOrder = null,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "id",
                    DisplayTemplate = null,
                    ListViewLabel = null,
                    ListViewType = "text",
                    ListViewWidth = null,
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = null,
                    Name = "Id",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "FirstName",
                    DataType = "string",
                    Description = "First name of the contact",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 100,
                    DisplayOrder = 8,
                    ShowInListView = false,
                    ListViewOrder = null,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "firstName",
                    DisplayTemplate = null,
                    ListViewLabel = null,
                    ListViewType = "text",
                    ListViewWidth = null,
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = null,
                    Name = "FirstName",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "LastName",
                    DataType = "string",
                    Description = "Last name of the contact",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 100,
                    DisplayOrder = 9,
                    ShowInListView = false,
                    ListViewOrder = null,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "lastName",
                    DisplayTemplate = null,
                    ListViewLabel = null,
                    ListViewType = "text",
                    ListViewWidth = null,
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = null,
                    Name = "LastName",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "Email",
                    DataType = "string",
                    Description = "Primary email address",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 255,
                    DisplayOrder = 11,
                    ShowInListView = false,
                    ListViewOrder = null,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "email",
                    DisplayTemplate = null,
                    ListViewLabel = null,
                    ListViewType = "text",
                    ListViewWidth = null,
                    ListViewEllipsis = true,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = null,
                    Name = "Email",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "Phone",
                    DataType = "string",
                    Description = "Primary phone number",
                    IsRequired = false,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 20,
                    DisplayOrder = 12,
                    ShowInListView = false,
                    ListViewOrder = null,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "phone",
                    DisplayTemplate = null,
                    ListViewLabel = null,
                    ListViewType = "text",
                    ListViewWidth = null,
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = null,
                    Name = "Phone",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 1,
                    FieldName = "Status",
                    DataType = "string",
                    Description = "Contact status",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = "Active",
                    MaxLength = 50,
                    DisplayOrder = 21,
                    ShowInListView = false,
                    ListViewOrder = null,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "status",
                    DisplayTemplate = null,
                    ListViewLabel = null,
                    ListViewType = "text",
                    ListViewWidth = null,
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = null,
                    Name = "Status",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                }
            };
        }

        private static List<EntityFieldManager> GetPartnerFields()
        {
            return new List<EntityFieldManager>
            {
                new EntityFieldManager
                {
                    EntityManagerId = 2,
                    FieldName = "PartnerCategoryName",
                    DataType = "string",
                    Description = "Partner category classification",
                    IsRequired = false,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 200,
                    DisplayOrder = 1,
                    ShowInListView = true,
                    ListViewOrder = 1,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "partnerCategoryName",
                    DisplayTemplate = null,
                    ListViewLabel = "Partner Category",
                    ListViewType = "text",
                    ListViewWidth = "15%",
                    ListViewEllipsis = true,
                    ListViewSortable = false,
                    FirstLetterFallbackField = null,
                    HelperText = "Partner category classification",
                    Name = "PartnerCategoryName",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 2,
                    FieldName = "Name",
                    DataType = "string",
                    Description = "Partner organization name",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 300,
                    DisplayOrder = 4,
                    ShowInListView = true,
                    ListViewOrder = 4,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "name",
                    DisplayTemplate = null,
                    ListViewLabel = "Name",
                    ListViewType = "text",
                    ListViewWidth = null,
                    ListViewEllipsis = false,
                    ListViewSortable = false,
                    FirstLetterFallbackField = null,
                    HelperText = "Partner organization name",
                    Name = "Name",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                }
            };
        }

        private static List<EntityFieldManager> GetInteractionFields()
        {
            return new List<EntityFieldManager>
            {
                new EntityFieldManager
                {
                    EntityManagerId = 3,
                    FieldName = "Type",
                    DataType = "enum",
                    Description = "Type of interaction (Meeting, Email, Call, etc.)",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = null,
                    DisplayOrder = 1,
                    ShowInListView = true,
                    ListViewOrder = 1,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "type",
                    DisplayTemplate = null,
                    ListViewLabel = "Type",
                    ListViewType = "text",
                    ListViewWidth = null,
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = "Interaction type classification",
                    Name = "Type",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 3,
                    FieldName = "Date",
                    DataType = "datetime",
                    Description = "Date and time of the interaction",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = null,
                    DisplayOrder = 2,
                    ShowInListView = true,
                    ListViewOrder = 2,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "date",
                    DisplayTemplate = null,
                    ListViewLabel = "Date",
                    ListViewType = "date",
                    ListViewWidth = null,
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = "When the interaction occurred",
                    Name = "Date",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 3,
                    FieldName = "Subject",
                    DataType = "string",
                    Description = "Subject or title of the interaction",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 300,
                    DisplayOrder = 3,
                    ShowInListView = true,
                    ListViewOrder = 3,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "subject",
                    DisplayTemplate = null,
                    ListViewLabel = "Subject",
                    ListViewType = "text",
                    ListViewWidth = null,
                    ListViewEllipsis = false,
                    ListViewSortable = false,
                    FirstLetterFallbackField = null,
                    HelperText = "Interaction subject or title",
                    Name = "Subject",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                }
            };
        }

        private static List<EntityFieldManager> GetPartnerTreeFields()
        {
            return new List<EntityFieldManager>
            {
                new EntityFieldManager
                {
                    EntityManagerId = 4,
                    FieldName = "Name",
                    DataType = "string",
                    Description = "Name of the partner tree node",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 300,
                    DisplayOrder = 1,
                    ShowInListView = true,
                    ListViewOrder = 1,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "name",
                    DisplayTemplate = null,
                    ListViewLabel = "Name",
                    ListViewType = "text",
                    ListViewWidth = "25%",
                    ListViewEllipsis = true,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = "The display name for this partner tree level or category",
                    Name = "Name",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 4,
                    FieldName = "Type",
                    DataType = "string",
                    Description = "Type/Level of partner tree node (Level_1, Level_2, Level_3)",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 100,
                    DisplayOrder = 3,
                    ShowInListView = true,
                    ListViewOrder = 3,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "type",
                    DisplayTemplate = null,
                    ListViewLabel = "Level",
                    ListViewType = "text",
                    ListViewWidth = "10%",
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = "Hierarchical level in the partner tree structure",
                    Name = "Type",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                }
            };
        }

        private static List<EntityFieldManager> GetOrganizationHierarchyFields()
        {
            return new List<EntityFieldManager>
            {
                new EntityFieldManager
                {
                    EntityManagerId = 5,
                    FieldName = "Code",
                    DataType = "string",
                    Description = "Unique organizational code",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 50,
                    DisplayOrder = 1,
                    ShowInListView = true,
                    ListViewOrder = 1,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "code",
                    DisplayTemplate = null,
                    ListViewLabel = "Code",
                    ListViewType = "text",
                    ListViewWidth = "15%",
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = null,
                    Name = "Code",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                },
                new EntityFieldManager
                {
                    EntityManagerId = 5,
                    FieldName = "Name",
                    DataType = "string",
                    Description = "Organization unit name",
                    IsRequired = true,
                    IsActive = true,
                    DefaultValue = null,
                    MaxLength = 200,
                    DisplayOrder = 2,
                    ShowInListView = true,
                    ListViewOrder = 2,
                    RelatedDisplayProperty = null,
                    DisplayFieldPath = "name",
                    DisplayTemplate = null,
                    ListViewLabel = "Name",
                    ListViewType = "text",
                    ListViewWidth = "25%",
                    ListViewEllipsis = false,
                    ListViewSortable = true,
                    FirstLetterFallbackField = null,
                    HelperText = null,
                    Name = "Name",
                    Status = 0,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = null,
                    IsDeleted = false,
                    DeletedBy = 0,
                    DeletedDate = null
                }
            };
        }
    }
}