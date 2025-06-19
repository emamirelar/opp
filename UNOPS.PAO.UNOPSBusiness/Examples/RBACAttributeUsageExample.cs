using System.Security.Claims;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.UNOPSBusiness.Examples;

/// <summary>
/// Example showing how to use the unified RBAC attribute in manager classes
/// The interceptor will automatically handle security checks based on these attributes
/// </summary>
public interface IContactManagerWithRBAC
{
    Task<PaginationResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginationRequest request);
    Task<ContactModel?> GetContactByIdAsync(ClaimsPrincipal user, int contactId);
    Task<ContactModel> CreateContactAsync(ClaimsPrincipal user, ContactCreateModel createModel);
    Task<ContactModel?> UpdateContactAsync(ClaimsPrincipal user, int contactId, ContactUpdateModel updateModel);
    Task<bool> DeleteContactAsync(ClaimsPrincipal user, int contactId);
    Task<IQueryable<UNOPSContact>> GetContactsQueryAsync(ClaimsPrincipal user);
    Task<List<string>> GetAllowedColumnsAsync(ClaimsPrincipal user);
}

public class ContactManagerWithRBAC : IContactManagerWithRBAC
{
    /// <summary>
    /// Gets contacts with automatic row and column filtering applied
    /// The [RBAC("read")] attribute automatically:
    /// - Checks if user has read permission for Contact entity
    /// - Applies row filtering to the returned query
    /// - Applies column filtering to the returned entities
    /// </summary>
    [RBAC("read", Entity = "Contact")]
    public async Task<PaginationResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        // Your business logic here - no manual security checks needed!
        // The interceptor will handle:
        // 1. Checking user permissions
        // 2. Applying row filters to any IQueryable<Contact> in your logic
        // 3. Applying column filters to the returned ContactModel objects
        
        // Simulate getting contacts
        var contacts = await GetContactsFromDatabase();
        
        return new PaginationResponse<ContactModel>
        {
            Records = contacts.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList(),
            TotalCount = contacts.Count
        };
    }

    /// <summary>
    /// Gets a specific contact with automatic entity access check and column filtering
    /// The [RBAC("read")] automatically:
    /// - Checks if user has read permission for Contact entity
    /// - Checks if user can access this specific contact (row-level security)
    /// - Applies column filtering to the returned contact
    /// </summary>
    [RBAC("read", Entity = "Contact")]
    public async Task<ContactModel?> GetContactByIdAsync(ClaimsPrincipal user, int contactId)
    {
        // The interceptor will automatically check if the user can access this specific contact
        // based on the contactId parameter and configured row filters
        
        var contact = await GetContactFromDatabaseById(contactId);
        return MapToContactModel(contact);
    }

    /// <summary>
    /// Creates a contact with automatic permission and column validation
    /// The [RBAC("create")] attribute automatically:
    /// - Checks if user has create permission for Contact entity
    /// - Validates that user can set all fields in the createModel (column permissions)
    /// </summary>
    [RBAC("create", Entity = "Contact")]
    public async Task<ContactModel> CreateContactAsync(ClaimsPrincipal user, ContactCreateModel createModel)
    {
        // No manual security checks needed!
        // The interceptor ensures user can create contacts and set all the specified fields
        
        var contact = new UNOPSContact
        {
            FirstName = createModel.FirstName,
            LastName = createModel.LastName,
            Email = createModel.Email,
            // ... other properties
        };
        
        // Save to database
        var savedContact = await SaveContactToDatabase(contact);
        return MapToContactModel(savedContact);
    }

    /// <summary>
    /// Updates a contact with automatic entity access check and column validation
    /// The [RBAC("update")] attribute automatically:
    /// - Checks if user has update permission for Contact entity
    /// - Checks if user can access this specific contact (row-level security)
    /// - Validates that user can modify all fields in the updateModel (column permissions)
    /// </summary>
    [RBAC("update", Entity = "Contact", EntityIdParameterName = "contactId")]
    public async Task<ContactModel?> UpdateContactAsync(ClaimsPrincipal user, int contactId, ContactUpdateModel updateModel)
    {
        // The interceptor automatically checks:
        // 1. User has update permission for Contact entity
        // 2. User can access the contact with this contactId
        // 3. User can modify all fields being updated
        
        var contact = await GetContactFromDatabaseById(contactId);
        if (contact == null) return null;
        
        // Update properties
        contact.FirstName = updateModel.FirstName;
        contact.LastName = updateModel.LastName;
        // ... other properties
        
        var updatedContact = await SaveContactToDatabase(contact);
        return MapToContactModel(updatedContact);
    }

    /// <summary>
    /// Deletes a contact with automatic entity access check
    /// The [RBAC("delete")] attribute automatically:
    /// - Checks if user has delete permission for Contact entity
    /// - Checks if user can access this specific contact (row-level security)
    /// </summary>
    [RBAC("delete", Entity = "Contact", EntityIdParameterName = "contactId")]
    public async Task<bool> DeleteContactAsync(ClaimsPrincipal user, int contactId)
    {
        // The interceptor automatically validates delete permissions and entity access
        
        var contact = await GetContactFromDatabaseById(contactId);
        if (contact == null) return false;
        
        return await DeleteContactFromDatabase(contactId);
    }

    /// <summary>
    /// Returns a queryable with automatic row filtering applied
    /// The [RBAC("read")] attribute automatically:
    /// - Checks if user has read permission for Contact entity
    /// - Applies row filtering to the returned IQueryable<UNOPSContact>
    /// </summary>
    [RBAC("read", Entity = "Contact")]
    public async Task<IQueryable<UNOPSContact>> GetContactsQueryAsync(ClaimsPrincipal user)
    {
        // Return the base query - the interceptor will apply row filtering automatically
        return GetBaseContactQuery();
    }

    /// <summary>
    /// Gets allowed columns for the current user
    /// The [RBAC("read")] attribute automatically checks permissions
    /// </summary>
    [RBAC("read", Entity = "Contact")]
    public async Task<List<string>> GetAllowedColumnsAsync(ClaimsPrincipal user)
    {
        // This method doesn't need the interceptor to do column filtering,
        // but we still want to check if user has read permission for Contact entity
        return new List<string> { "Id", "FirstName", "LastName", "Email" };
    }

    /// <summary>
    /// Internal method that skips RBAC checks
    /// Use [SkipRBAC] for internal system operations that shouldn't be subject to user permissions
    /// </summary>
    [SkipRBAC("Internal system operation")]
    public async Task<List<UNOPSContact>> GetAllContactsForSystemProcessingAsync()
    {
        // This method bypasses all RBAC checks - use carefully!
        return await GetAllContactsFromDatabase();
    }

    /// <summary>
    /// Custom RBAC example for complex scenarios
    /// </summary>
    [RBAC("approve", Entity = "Contact", EntityIdParameterName = "contactId")]
    public async Task<bool> ApproveContactAsync(ClaimsPrincipal user, int contactId)
    {
        // Custom action "approve" - you'd need to configure this in your EntityPermissions table
        var contact = await GetContactFromDatabaseById(contactId);
        if (contact == null) return false;
        
        // Approval logic here
        return true;
    }

    /// <summary>
    /// Example with advanced configuration - skip column filtering but keep row filtering
    /// </summary>
    [RBAC("read", Entity = "Contact", ApplyColumnFiltering = false)]
    public async Task<List<UNOPSContact>> GetRawContactsAsync(ClaimsPrincipal user)
    {
        // Returns contacts with row filtering but no column filtering
        return await GetAllContactsFromDatabase();
    }

    /// <summary>
    /// Example with entity auto-detection (Entity name inferred from class name)
    /// </summary>
    [RBAC("read")] // Entity auto-detected as "Contact" from "UNOPSContactManager"
    public async Task<List<ContactModel>> GetMyContactsAsync(ClaimsPrincipal user)
    {
        // Entity name automatically detected from manager class name
        var contacts = await GetContactsFromDatabase();
        return contacts;
    }

    /// <summary>
    /// Example using entity parameter instead of entity ID
    /// </summary>
    [RBAC("update", Entity = "Contact", EntityParameterName = "contactEntity")]
    public async Task<ContactModel> UpdateContactDirectAsync(ClaimsPrincipal user, UNOPSContact contactEntity)
    {
        // Interceptor uses the contactEntity parameter for access checks
        contactEntity.LastModifiedDate = DateTime.UtcNow;
        var updatedContact = await SaveContactToDatabase(contactEntity);
        return MapToContactModel(updatedContact);
    }

    #region Private Helper Methods (No RBAC attributes needed)

    private async Task<List<ContactModel>> GetContactsFromDatabase()
    {
        // Simulate database call
        await Task.Delay(10);
        return new List<ContactModel>();
    }

    private async Task<UNOPSContact?> GetContactFromDatabaseById(int id)
    {
        // Simulate database call
        await Task.Delay(10);
        return new UNOPSContact { Id = id };
    }

    private async Task<UNOPSContact> SaveContactToDatabase(UNOPSContact contact)
    {
        // Simulate database save
        await Task.Delay(10);
        return contact;
    }

    private async Task<bool> DeleteContactFromDatabase(int id)
    {
        // Simulate database delete
        await Task.Delay(10);
        return true;
    }

    private async Task<List<UNOPSContact>> GetAllContactsFromDatabase()
    {
        // Simulate database call
        await Task.Delay(10);
        return new List<UNOPSContact>();
    }

    private IQueryable<UNOPSContact> GetBaseContactQuery()
    {
        // Return base queryable - interceptor will apply security filters
        return new List<UNOPSContact>().AsQueryable();
    }

    private ContactModel MapToContactModel(UNOPSContact? contact)
    {
        if (contact == null) return null!;
        
        return new ContactModel
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email
        };
    }

    #endregion
}

// Supporting models for the example
public class ContactCreateModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}

public class ContactUpdateModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
} 