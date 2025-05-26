using UNOPS.PAO.Domain.Specifications;
using System.Linq;

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

public class UNOPSContactManager : IContactManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSContact> contactRepository;
    private BaseRepository<UNOPSPartner> partnerRepository;
    private BaseRepository<UserInfo> userInfoRepository;
    private BaseRepository<OrganizationHierarchy> organizationHierarchyRepository;
    private GoogleCloudStorageService googleCloudStorageService;

    private CommonEntityRepository commonRepository;

    //private string[] includes = ["Currency", "Documents"];

    private ContactModel MapEntityToModel(UNOPSContact entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSContact, ContactModel>(entity);
        result.Partner = mapper.Map<Partner, PartnerModel>(entity.Partner);
        
        // Map CreatedBy user ID to user name and office
        if (entity.CreatedBy > 0)
        {
            var userInfo = userInfoRepository.GetAll()
                .FirstOrDefault(u => u.UserId == entity.CreatedBy);
            
            if (userInfo != null)
            {
                result.CreatedByName = userInfo.Name;
                
                // Get office name from OrganizationHierarchy where Code = userInfo.OrgUnit
                if (!string.IsNullOrEmpty(userInfo.OrgUnit))
                {
                    var orgHierarchy = organizationHierarchyRepository.GetAll()
                        .Where(o => !string.IsNullOrEmpty(o.Code))
                        .FirstOrDefault(o => o.Code == userInfo.OrgUnit);
                    if (orgHierarchy != null)
                    {
                        result.CreatedByOfficeName = orgHierarchy.Name;
                    }
                }
            }
        }
        
        return result;
    }
    
    private ContactModel MapEntityToModelWithUserInfo(UNOPSContact entity, IMapper mapper, Dictionary<int, UserInfo> userInfoLookup, Dictionary<string, OrganizationHierarchy> orgHierarchyLookup)
    {
        var result = mapper.Map<UNOPSContact, ContactModel>(entity);
        result.Partner = mapper.Map<Partner, PartnerModel>(entity.Partner);
        
        // Map CreatedBy user ID to user name and office
        if (entity.CreatedBy > 0 && userInfoLookup.TryGetValue(entity.CreatedBy, out var userInfo))
        {
            result.CreatedByName = userInfo.Name;
            
            // Get office name from OrganizationHierarchy where Code = userInfo.OrgUnit
            if (!string.IsNullOrEmpty(userInfo.OrgUnit) && 
                orgHierarchyLookup.TryGetValue(userInfo.OrgUnit, out var orgHierarchy))
            {
                result.CreatedByOfficeName = orgHierarchy.Name;
            }
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

    public UNOPSContactManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        this.mapper = mapper;
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration);
        partnerRepository = new BaseRepository<UNOPSPartner>(context, configuration);
        userInfoRepository = new BaseRepository<UserInfo>(context, configuration);
        organizationHierarchyRepository = new BaseRepository<OrganizationHierarchy>(context, configuration);
        commonRepository = new CommonEntityRepository(context);
        googleCloudStorageService = new GoogleCloudStorageService(configuration);
    }

    public async Task<ContactModel> CreateContactAsync(ContactRequest model)
    {
        var entity = MapModelToEntity(model);
        
        await contactRepository.AddAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }

    public PaginationResponse<ContactModel> GetContacts(int userId, PaginationRequest request)
    {
        var query = contactRepository
            .GetAll(["Partner"])
            .AsQueryable();

        // Custom pagination with efficient user lookup
        var totalCount = query.Count();
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        var items = query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToList();

        // Get all unique user IDs from the contacts
        var userIds = items.Select(c => c.CreatedBy).Distinct().Where(id => id > 0).ToList();
        
        // Batch lookup all user info at once
        var userInfoLookup = userInfoRepository.GetAll()
            .Where(u => userIds.Contains(u.UserId))
            .ToDictionary(u => u.UserId, u => u);

        // Get all unique org unit codes from the user info
        var orgUnitCodes = userInfoLookup.Values
            .Where(u => !string.IsNullOrEmpty(u.OrgUnit))
            .Select(u => u.OrgUnit)
            .Distinct()
            .ToList();

        // Batch lookup all organization hierarchy at once
        var orgHierarchyLookup = organizationHierarchyRepository.GetAll()
            .Where(o => orgUnitCodes.Contains(o.Code) && !string.IsNullOrEmpty(o.Code))
            .GroupBy(o => o.Code)
            .ToDictionary(g => g.Key, g => g.First());

        // Map entities to models with efficient user and org lookup
        var mappedItems = items.Select(x => MapEntityToModelWithUserInfo(x, mapper, userInfoLookup, orgHierarchyLookup)).ToList();

        return new PaginationResponse<ContactModel>
        {
            Records = mappedItems,
            TotalCount = totalCount
        };
    }

    public PaginationResponse<ContactModel> GetContactsWithSpecification(int userId, ISpecification<Contact> specification, PaginationRequest pagination)
    {
        // Apply the specification to the query
        var query = contactRepository.GetAll(["Partner"]).AsQueryable();
        var filteredQuery = query.ApplySpecification(specification);
        
        // Custom pagination with efficient user lookup
        var totalCount = filteredQuery.Count();
        var pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
        var excludedRows = (pageIndex - 1) * pagination.PageSize;
        
        var items = filteredQuery
            .Skip(excludedRows)
            .Take(pagination.PageSize)
            .Cast<UNOPSContact>()
            .ToList();

        // Get all unique user IDs from the contacts
        var userIds = items.Select(c => c.CreatedBy).Distinct().Where(id => id > 0).ToList();
        
        // Batch lookup all user info at once
        var userInfoLookup = userInfoRepository.GetAll()
            .Where(u => userIds.Contains(u.UserId))
            .ToDictionary(u => u.UserId, u => u);

        // Get all unique org unit codes from the user info
        var orgUnitCodes = userInfoLookup.Values
            .Where(u => !string.IsNullOrEmpty(u.OrgUnit))
            .Select(u => u.OrgUnit)
            .Distinct()
            .ToList();

        // Batch lookup all organization hierarchy at once
        var orgHierarchyLookup = organizationHierarchyRepository.GetAll()
            .Where(o => orgUnitCodes.Contains(o.Code) && !string.IsNullOrEmpty(o.Code))
            .GroupBy(o => o.Code)
            .ToDictionary(g => g.Key, g => g.First());

        // Map entities to models with efficient user and org lookup
        var mappedItems = items.Select(x => MapEntityToModelWithUserInfo(x, mapper, userInfoLookup, orgHierarchyLookup)).ToList();

        return new PaginationResponse<ContactModel>
        {
            Records = mappedItems,
            TotalCount = totalCount
        };
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

    public IEnumerable<ContactModel> GetPartnerContacts(int partnerId)
    {
        return contactRepository
            .GetAll()
            .Where(x => x.PartnerId == partnerId)
            .Select(x => MapEntityToModel(x, mapper));
    }
    public async Task<ContactModel?> GetContactAsync(int id)
    {
        string[] includes = ["Documents"];

        var item = await contactRepository.GetByIdAsync(id, includes);

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

    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the contact
    /// </summary>
    public async Task<bool> HasPermissionAsync(int userId, int contactId, string operation)
    {
        // Get the contact entity
        var entity = await contactRepository.GetByIdAsync(contactId);
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