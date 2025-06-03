using Microsoft.AspNetCore.Http;

namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Models;
using System.Security.Claims;

public interface IContactManager
{
    Task<ContactModel> CreateContactAsync(ContactRequest model);

    PaginationResponse<ContactModel> GetContacts(int userId, PaginationRequest request);
    
    PaginationResponse<ContactModel> GetContactsWithSpecification(int userId, ISpecification<Contact> specification, PaginationRequest pagination);

    Task<ContactModel?> GetContact(int userId, int id);

    IEnumerable<ExternalContactModel> GetPostedContacts();

    Task<ExternalContactModel?> GetPostedContact(int id);

    Task<ContactModel?> UpdateContactAsync(int userId, UpdateContactRequest model);

    Task DeleteContactAsync(int userId, int id);

    IEnumerable<ContactModel> GetPartnerContacts(int partnerId);
    Task<ContactModel?> GetContactAsync(int id);
    
    Task<string?> UpdateContactProfilePictureAsync(int contactId, IFormFile file);

    // New secure methods with ClaimsPrincipal for row-level security
    
    /// <summary>
    /// Gets contacts with row-level filtering and entity permissions applied
    /// </summary>
    Task<PaginationResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginationRequest request);
    
    /// <summary>
    /// Gets a specific contact with entity-level access check and permissions
    /// </summary>
    Task<ContactModel?> GetContactAsync(ClaimsPrincipal user, int id);
    
    /// <summary>
    /// Updates a contact with entity-level access check
    /// </summary>
    Task<ContactModel?> UpdateContactAsync(ClaimsPrincipal user, UpdateContactRequest model);
    
    /// <summary>
    /// Deletes a contact with entity-level access check
    /// </summary>
    Task DeleteContactAsync(ClaimsPrincipal user, int id);

    Task<List<ContactModel?>> GetContactsForGmailAddon(GmailRelatedRecordsRequest input);
}