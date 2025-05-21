using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Specifications;

namespace UNOPS.PAO.Business.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Helpers;
using System.Security.Claims;

public class ContactManager : IContactManager
{
    private IMapper mapper;

    private DataRepository<Contact> ContactRepository;

    public ContactManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.ContactRepository = new DataRepository<Contact>(context);
    }

    public async Task<ContactModel> CreateContactAsync(ContactRequest model)
    {
        var entity = mapper.Map<Contact>(model);

        await ContactRepository.AddAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }

    public PaginationResponse<ContactModel> GetContacts(int userId, PaginationRequest request)
    {
        var query = ContactRepository
            .GetAll()
            .AsQueryable();

        return query.Paginate(
            x => mapper.Map<ContactModel>(x),
            request
        );
    }

    public PaginationResponse<ContactModel> GetContactsWithSpecification(int userId, ISpecification<Contact> specification, PaginationRequest pagination)
    {
        // Apply the specification to the query
        var query = ContactRepository.GetAll().AsQueryable();
        var filteredQuery = query.ApplySpecification(specification);
        
        // Apply pagination
        return filteredQuery.Paginate(
            x => mapper.Map<ContactModel>(x),
            pagination
        );
    }

    public async Task<ContactModel?> GetContact(int userId, int id)
    {
        var item = await ContactRepository.GetByIdAsync(id);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ContactModel>(item);
    }

    public IEnumerable<ExternalContactModel> GetPostedContacts()
    {
        return ContactRepository
            .GetAll()
            .Select(mapper.Map<ExternalContactModel>);
    }

    public async Task<ExternalContactModel?> GetPostedContact(int id)
    {
        var item = await ContactRepository.GetByIdAsync(id, ["EligibleEntities"]);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ExternalContactModel>(item);
    }

    public async Task<ContactModel?> UpdateContactAsync(int userId, UpdateContactRequest model)
    {
        var entity = await ContactRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            return default;
        }

        mapper.Map<UpdateContactRequest, Contact>(model, entity);

        await ContactRepository.UpdateAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }

    /*public async Task<ContactModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await ContactRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        entity.Stage = newStage;

        if (newStage == "Open")
        {
            entity.PostingDate = DateTime.Now.ToUniversalTime();
        }

        await ContactRepository.UpdateAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }*/

    public async Task DeleteContactAsync(int userId, int id)
    {
        var entity = await ContactRepository.GetByIdAsync(id);
        if (entity != null)
        {
            await ContactRepository.Delete(entity);
        }
    }

    public IEnumerable<ContactModel> GetPartnerContacts(int partnerId)
    {
        // TODO: get stage from workflow?
        return ContactRepository
            .GetAll(["Partner"])
            .Where(x => x.PartnerId == partnerId)
            .Select(x => new ContactModel()
            {
                Id = x.Id,
                PartnerId = x.Partner.Id,
                PartnerName = x.Partner.Name,
                Salutation = x.Salutation,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Mobile = x.Mobile
            });
    }

    public async Task<ContactModel?> GetContactAsync(int id)
    {
        string[] includes = ["Documents"];

        var item = await ContactRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        var result = mapper.Map<ContactModel>(item);

        //result.ApplicationType = applicationTypeManager.GetApplicationTypeByCode(item.ApplicationTypeCode);

        return result;
    }

    public async Task<string?> UpdateContactProfilePictureAsync(int contactId, IFormFile file)
    {
        return null;
    }

    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the contact
    /// </summary>
    public async Task<bool> HasPermissionAsync(int userId, int contactId, string operation)
    {
        // Get the contact entity
        var entity = await ContactRepository.GetByIdAsync(contactId);
        if (entity == null)
        {
            return false;
        }
        
        // Basic permission rules:
        // 1. Administrator can do anything
        // 2. Creator of the contact can do anything with their own contacts
        // 3. For Read operations, any Internal or Partner role can access
        // 4. For Update/Delete, only creator or admin can perform
        
        // Check if user is the creator
        bool isCreator = entity.CreatedBy == userId;
        
        // If user is creator, they have full access
        if (isCreator)
        {
            return true;
        }
        
        // For Read operations, allow access to Partner users
        if (operation == "Read")
        {
            // Partner users should be able to view contacts
            return true;
        }
        
        // For other operations (Update, Delete), only allow if user is creator
        // In a real implementation, you would check if the user has Administrator role
        return false;
    }
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the contact
    /// </summary>
    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, int contactId, string operation)
    {
        // Get user ID from claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return false;
        }
        
        // Use the existing method
        return await HasPermissionAsync(userId, contactId, operation);
    }
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the contact
    /// </summary>
    public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, Contact contact, string operation)
    {
        // Get user ID from claims
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return false;
        }
        
        // Check if user is the creator
        bool isCreator = contact.CreatedBy == userId;
        
        // If user is creator, they have full access
        if (isCreator)
        {
            return true;
        }
        
        // Check if user is administrator
        bool isAdmin = user.IsInRole("Administrator");
        if (isAdmin)
        {
            return true;
        }
        
        // For Read operations, allow access to all users with Partner role or higher
        if (operation == "Read")
        {
            return user.IsInRole("Partner") || user.IsInRole("Internal");
        }
        
        // For other operations (Update, Delete), only allow if user is creator or has admin privileges
        return false;
    }
}