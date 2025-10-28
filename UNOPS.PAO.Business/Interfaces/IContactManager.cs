using Microsoft.AspNetCore.Http;

namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using System.Security.Claims;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Integrations;

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
    Task<PaginationResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginationRequest request);
    Task<ContactModel?> GetContactAsync(ClaimsPrincipal user, int id);
    Task<ContactModel?> UpdateContactAsync(ClaimsPrincipal user, UpdateContactRequest model);
    Task DeleteContactAsync(ClaimsPrincipal user, int id);

    Task<List<ContactModel?>> GetContactsForGmailAddon(GmailRelatedRecordsRequest input, ClaimsPrincipal user);
    
    /// <summary>
    /// Get supported search fields for contacts
    /// </summary>
    List<SearchFieldInfo> GetContactSearchFields();
    Task<object> GetContactsWithSpecificationAsync(ClaimsPrincipal user, ISpecification<Contact> specification, PaginationRequest pagination);
    Task<List<UnmatchedEmailModel>> GetUnmatchedEmailsWithPartnerSuggestionsAsync(List<string> emailAddresses, ClaimsPrincipal user = null);
    
    /// <summary>
    /// Gets a contact by email address
    /// </summary>
    /// <param name="user">The current user's claims principal</param>
    /// <param name="email">The email address to search for</param>
    /// <returns>The contact model if found, null otherwise</returns>
    Task<ContactModel?> GetContactByEmailAsync(ClaimsPrincipal user, string email);
}