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
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the contact
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="contactId">ID of the contact</param>
    /// <param name="operation">Operation to check (e.g., "Read", "Update", "Delete")</param>
    /// <returns>True if the user has permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(int userId, int contactId, string operation);
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the contact
    /// </summary>
    /// <param name="user">ClaimsPrincipal of the user</param>
    /// <param name="contactId">ID of the contact</param>
    /// <param name="operation">Operation to check (e.g., "Read", "Update", "Delete")</param>
    /// <returns>True if the user has permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, int contactId, string operation);
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the contact
    /// </summary>
    /// <param name="user">ClaimsPrincipal of the user</param>
    /// <param name="contact">Contact entity</param>
    /// <param name="operation">Operation to check (e.g., "Read", "Update", "Delete")</param>
    /// <returns>True if the user has permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, Contact contact, string operation);
}