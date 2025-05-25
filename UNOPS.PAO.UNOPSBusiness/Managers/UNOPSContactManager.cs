using UNOPS.PAO.Domain.Specifications;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Utilities.Helpers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using UNOPS.PAO.UNOPSBusiness.Services;

public class UNOPSContactManager : IContactManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSContact> contactRepository;
    private BaseRepository<UNOPSPartner> partnerRepository;
    private GoogleCloudStorageService googleCloudStorageService;
    private readonly IBusinessSecurityService _securityService;

    private CommonEntityRepository commonRepository;

    //private string[] includes = ["Currency", "Documents"];

    private ContactModel MapEntityToModel(UNOPSContact entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSContact, ContactModel>(entity);
        result.Partner = mapper.Map<Partner, PartnerModel>(entity.Partner);
        return result;
    }
    
    private async Task<ContactModel> MapEntityToModelWithPermissionsAsync(UNOPSContact entity, IMapper mapper, ClaimsPrincipal user)
    {
        var result = MapEntityToModel(entity, mapper);
        
        // Add permissions if security service is available
        if (_securityService != null)
        {
            var permissions = await _securityService.GetEntityPermissionsAsync(entity, user);
            result.Permissions = new EntityPermissionsModel
            {
                CanRead = ((dynamic)permissions).canRead,
                CanUpdate = ((dynamic)permissions).canUpdate,
                CanDelete = ((dynamic)permissions).canDelete,
                CanCreate = ((dynamic)permissions).canCreate
            };
        }
        
        return result;
    }
    
    private ExternalContactModel MapEntityToExternalModel(UNOPSContact entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSContact, ExternalContactModel>(entity);
        return result;
    }

    private UNOPSContact MapModelToEntity(ContactRequest model, UNOPSContact entity)
    {
        mapper.Map(model, entity);

        entity.Name = String.Concat(model.Salutation, ' ', model.FirstName, ' ', model.MiddleName, ' ', model.LastName);
        return entity;
    }

    private UNOPSContact MapModelToEntity(ContactRequest model)
    {
        return MapModelToEntity(model, new UNOPSContact());
    }

    public UNOPSContactManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, IBusinessSecurityService securityService = null)
    {
        this.mapper = mapper;
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration);
        partnerRepository = new BaseRepository<UNOPSPartner>(context, configuration);
        commonRepository = new CommonEntityRepository(context);
        googleCloudStorageService = new GoogleCloudStorageService(configuration);
        _securityService = securityService;
    }

    public async Task<ContactModel> CreateContactAsync(ContactRequest model)
    {
        var entity = MapModelToEntity(model);
        
        await contactRepository.AddAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }

    // Original methods (kept for backward compatibility)
    public PaginationResponse<ContactModel> GetContacts(int userId, PaginationRequest request)
    {
        var query = contactRepository
            .GetAll(["Partner"])
            .AsQueryable();

        return query.Paginate(
            x => MapEntityToModel(x, mapper),
            request
        );
    }

    // New secure methods with row-level filtering and permissions
    public async Task<PaginationResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        var query = contactRepository
            .GetAll(["Partner", "Partner.PartnerOffice"])
            .AsQueryable();

        // Apply row-level security filters
        if (_securityService != null)
        {
            query = await _securityService.ApplyRowFiltersAsync(query, user, "read");
        }

        // Apply pagination with permissions
        var pagedResults = query.Paginate(
            x => MapEntityToModel(x, mapper),
            request
        );

        // Add permissions to each contact
        if (_securityService != null)
        {
            var contactsWithPermissions = new List<ContactModel>();
            foreach (var contact in pagedResults.Records)
            {
                var entity = await contactRepository.GetByIdAsync(contact.Id, ["Partner", "Partner.PartnerOffice"]);
                if (entity != null)
                {
                    var contactWithPermissions = await MapEntityToModelWithPermissionsAsync(entity, mapper, user);
                    contactsWithPermissions.Add(contactWithPermissions);
                }
            }
            
            return new PaginationResponse<ContactModel>
            {
                Records = contactsWithPermissions,
                TotalCount = pagedResults.TotalCount
            };
        }

        return pagedResults;
    }

    public async Task<ContactModel?> GetContactAsync(ClaimsPrincipal user, int id)
    {
        var item = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerOffice"]);
        if (item == null) return null;

        // Check if user can access this specific contact
        if (_securityService != null && !await _securityService.CanUserAccessEntityAsync(item, user, "read"))
        {
            return null; // User cannot access this contact
        }

        return await MapEntityToModelWithPermissionsAsync(item, mapper, user);
    }

    public async Task<ContactModel?> UpdateContactAsync(ClaimsPrincipal user, UpdateContactRequest model)
    {
        var entity = await contactRepository.GetByIdAsync(model.Id, ["Partner", "Partner.PartnerOffice"]);
        if (entity == null)
        {
            throw new BusinessException($"Contact {model.Id} does not exist.");
        }

        // Check if user can update this contact
        if (_securityService != null && !await _securityService.CanUserAccessEntityAsync(entity, user, "update"))
        {
            throw new UnauthorizedAccessException("You don't have permission to update this contact");
        }

        entity = MapModelToEntity(model, entity);
        await contactRepository.UpdateAsync(entity);

        return await MapEntityToModelWithPermissionsAsync(entity, mapper, user);
    }

    public async Task DeleteContactAsync(ClaimsPrincipal user, int id)
    {
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerOffice"]);
        if (entity == null) return;

        // Check if user can delete this contact
        if (_securityService != null && !await _securityService.CanUserAccessEntityAsync(entity, user, "delete"))
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this contact");
        }

        await contactRepository.Delete(entity);
    }

    public PaginationResponse<ContactModel> GetContactsWithSpecification(int userId, ISpecification<Contact> specification, PaginationRequest pagination)
    {
        // Apply the specification to the query
        var query = contactRepository.GetAll().AsQueryable();
        var filteredQuery = query.ApplySpecification(specification);
        
        // Apply pagination
        return filteredQuery.Paginate(
            x => mapper.Map<ContactModel>(x),
            pagination
        );
    }

    public async Task<ContactModel?> GetContact(int userId, int id)
    {
        var item = await contactRepository.GetByIdAsync(id);
        var partner = await partnerRepository.GetByIdAsync(item.PartnerId);
        if (item == null)
        {
            return default;
        }
        item.Partner = partner;
        return MapEntityToModel(item, mapper);
    }

    public IEnumerable<ExternalContactModel> GetPostedContacts()
    {
        return contactRepository
            .GetAll()
            .Select(x => MapEntityToExternalModel(x, mapper));
    }

    public async Task<ExternalContactModel?> GetPostedContact(int id)
    {
        var item = await contactRepository.GetByIdAsync(id);

        if (item == null)
        {
            throw new BusinessException($"Contact {id} does not exist.");
        }

        return MapEntityToExternalModel(item, mapper);
    }

    public IEnumerable<ContactModel> GetPartnerContacts(int partnerId)
    {
        return contactRepository
            .GetAll()
            .Where(x => x.PartnerId == partnerId)
            .Select(x => MapEntityToModel(x, mapper));
    }

    public async Task<string?> UpdateContactProfilePictureAsync(int contactId, IFormFile file)
    {
        var entity = await contactRepository.GetByIdAsync(contactId);
        if (entity == null)
        {
            return null;
        }

        try
        {
            // Upload the file to Google Cloud Storage
            var fileName = $"contacts/{contactId}/profile_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var publicUrl = await googleCloudStorageService.UploadFileAsync(file, fileName);
            
            // Update the entity with the profile picture URL
            entity.ProfilePictureUrl = publicUrl;
            await contactRepository.UpdateAsync(entity);
            
            return publicUrl;
        }
        catch (Exception ex)
        {
            // Log the error and return null
            Console.WriteLine($"Error uploading profile picture: {ex.Message}");
            return null;
        }
    }

    // Legacy interface methods (kept for backward compatibility)
    public async Task<ContactModel?> GetContactAsync(int id)
    {
        string[] includes = ["Documents", "Partner", "Partner.PartnerOffice"];

        var item = await contactRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        var result = mapper.Map<ContactModel>(item);
        return result;
    }

    public async Task<ContactModel?> UpdateContactAsync(int userId, UpdateContactRequest model)
    {
        var entity = await contactRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Contact {model.Id} does not exist.");
        }

        entity = MapModelToEntity(model, entity);

        await contactRepository.UpdateAsync(entity);

        return MapEntityToModel(entity, mapper);
    }

    public async Task DeleteContactAsync(int userId, int id)
    {
        var entity = await contactRepository.GetByIdAsync(id);

        if (entity != null)
        {
            await contactRepository.Delete(entity);
        }
    }
}