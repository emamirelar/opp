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
using UNOPS.PAO.Utilities.Helpers;
using System.Security.Claims;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Search;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Integrations;

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
        throw new NotImplementedException();
    }



    public PaginationResponse<ContactModel> GetContactsWithSpecification(int userId, ISpecification<Contact> specification, PaginationRequest pagination)
    {
        throw new NotImplementedException();
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
                Partner = new PartnerSummaryModel { Id = x.Partner.Id, Name = x.Partner.Name },
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

    /// <summary>
    /// Gets a contact with its interactions included
    /// </summary>
    public async Task<ContactModel?> GetContactWithInteractionsAsync(int id)
    {
        string[] includes = ["Documents", "Partner", "Interactions"];

        var item = await ContactRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        // Now you can access interactions directly from the contact entity
        // Examples:
        // var recentInteractions = item.Interactions?.OrderByDescending(i => i.Date).Take(5).ToList();
        // var interactionCount = item.Interactions?.Count ?? 0;

        var result = mapper.Map<ContactModel>(item);
        return result;
    }

    public async Task<string?> UpdateContactProfilePictureAsync(int contactId, IFormFile file)
    {
        return null;
    }

    // New secure methods - stub implementations for base class
    public virtual async Task<PaginationResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        // For base implementation, fall back to user ID-based method
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.TryParse(userIdClaim, out var id) ? id : 0;
        
        return GetContacts(userId, request);
    }
    
    public virtual async Task<ContactModel?> GetContactAsync(ClaimsPrincipal user, int id)
    {
        // For base implementation, fall back to user ID-based method
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;
        
        return await GetContact(userId, id);
    }
    
    public virtual async Task<ContactModel?> UpdateContactAsync(ClaimsPrincipal user, UpdateContactRequest model)
    {
        // For base implementation, fall back to user ID-based method
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.TryParse(userIdClaim, out var id) ? id : 0;
        
        return await UpdateContactAsync(userId, model);
    }
    
    public virtual async Task DeleteContactAsync(ClaimsPrincipal user, int id)
    {
        // For base implementation, fall back to user ID-based method
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int userId = int.TryParse(userIdClaim, out var uid) ? uid : 0;
        
        await DeleteContactAsync(userId, id);
    }

    public async Task<List<ContactModel?>> GetContactsForGmailAddon(GmailRelatedRecordsRequest input, ClaimsPrincipal user = null)
    {
        throw new NotImplementedException("Use UNOPSInteractionManager for UNOPS-specific implementation");
    }

    public virtual async Task<object> GetContactsWithSpecificationAsync(ClaimsPrincipal user, ISpecification<Contact> specification, PaginationRequest pagination)
    {
        // For base implementation, fall back to user ID-based method
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return GetContactsWithSpecification(userId, specification, pagination);
        }
        
        return new PaginationResponse<ContactModel>
        {
            Records = new List<ContactModel>(),
            TotalCount = 0,
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize
        };
    }

    public virtual async Task<List<UnmatchedEmailModel>> GetUnmatchedEmailsWithPartnerSuggestionsAsync(List<string> emailAddresses, ClaimsPrincipal user = null)
    {
        throw new NotImplementedException("Use UNOPSContactManager for UNOPS-specific implementation");
    }

    public virtual async Task<ContactModel?> GetContactByEmailAsync(ClaimsPrincipal user, string email)
    {
        throw new NotImplementedException("Use UNOPSContactManager for UNOPS-specific implementation");
    }
    
    public virtual List<SearchFieldInfo> GetContactSearchFields()
    {
        throw new NotSupportedException("Search fields functionality is only available in UNOPS implementation. Use UNOPSContactManager instead.");
    }
}