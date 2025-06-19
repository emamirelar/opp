using UNOPS.PAO.Domain.Specifications;
using System.Linq;
using UNOPS.PAO.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
using UNOPS.PAO.UNOPSBusiness.Attributes;

public class UNOPSContactManager : BaseUNOPSManager, IContactManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSContact> contactRepository;
    private BaseRepository<UNOPSPartner> partnerRepository;
    private BaseRepository<UserInfo> userInfoRepository;
    private BaseRepository<OrganizationHierarchy> organizationHierarchyRepository;
    private GoogleCloudStorageService googleCloudStorageService;
    private readonly IBusinessSecurityService _securityService;

    private CommonEntityRepository commonRepository;

    //private string[] includes = ["Currency", "Documents"];

    private ContactModel MapEntityToModel(UNOPSContact entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSContact, ContactModel>(entity);
        result.Partner = mapper.Map<Partner, PartnerModel>(entity.Partner);
        
        // Convert ProfilePictureUrl to signed URL if it exists and contains Google Cloud Storage path
        if (!string.IsNullOrEmpty(result.ProfilePictureUrl) && googleCloudStorageService != null)
        {
            result.ProfilePictureUrl = googleCloudStorageService.GenerateSignedUrlFromStorageUrl(result.ProfilePictureUrl).Result;
        }
        
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
                        .Where(o => !string.IsNullOrEmpty(o.Code) && o.Type == OrganizationUnitType.OrgUnit)
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
        
        // Convert ProfilePictureUrl to signed URL if it exists and contains Google Cloud Storage path
        if (!string.IsNullOrEmpty(result.ProfilePictureUrl) && googleCloudStorageService != null)
        {
            result.ProfilePictureUrl = googleCloudStorageService.GenerateSignedUrlFromStorageUrl(result.ProfilePictureUrl).Result;
        }
        
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

    
    private async Task<ContactModel> MapEntityToModelWithPermissionsAsync(UNOPSContact entity, IMapper mapper, ClaimsPrincipal user)
    {
        var result = MapEntityToModel(entity, mapper);
        
        // Add permissions using the helper method from BaseUNOPSManager
        result.Permissions = await GetEntityPermissionsAsync(entity, user);
        
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

    public UNOPSContactManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, IBusinessSecurityService securityService)
        : base(mapper, context, configuration, null, securityService)
    {
        this.mapper = mapper;
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration);
        partnerRepository = new BaseRepository<UNOPSPartner>(context, configuration);
        userInfoRepository = new BaseRepository<UserInfo>(context, configuration);
        organizationHierarchyRepository = new BaseRepository<OrganizationHierarchy>(context, configuration);
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
            .Where(o => orgUnitCodes.Contains(o.Code) && !string.IsNullOrEmpty(o.Code) && o.Type == OrganizationUnitType.OrgUnit)
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

    // New secure methods with row-level filtering and permissions
    [RBAC("read", Entity = "Contact")]
    public async Task<PaginationResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        var query = contactRepository
            .GetAll(["Partner", "Partner.PartnerOffice"])
            .AsQueryable();

        // RBAC interceptor automatically applies row-level security filters

        // Get page index and calculate offset
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;

        // Apply ordering if specified
        if (request.OrderBy != null)
        {
            query = query.OrderByColumnName(request.OrderBy, request.Ascending ?? true);
        }

        // Get total count first
        var totalCount = query.Count();
        Console.WriteLine($"🐛 DEBUG: Final total count: {totalCount}");
        
        // Materialize the entities to avoid concurrent database operations
        var entities = query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .Cast<UNOPSContact>()
            .ToList();

        Console.WriteLine($"🐛 DEBUG: Retrieved {entities.Count} contacts for page {pageIndex}");

        // Get all unique user IDs from the contacts for efficient lookup
        var userIds = entities.Select(c => c.CreatedBy).Distinct().Where(id => id > 0).ToList();
        
        // Batch lookup all user info at once
        var userInfoLookup = new Dictionary<int, UserInfo>();
        if (userIds.Any())
        {
            userInfoLookup = userInfoRepository.GetAll()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionary(u => u.UserId, u => u);
        }

        // Get all unique org unit codes from the user info
        var orgUnitCodes = userInfoLookup.Values
            .Where(u => !string.IsNullOrEmpty(u.OrgUnit))
            .Select(u => u.OrgUnit)
            .Distinct()
            .ToList();

        // Batch lookup all organization hierarchy at once
        var orgHierarchyLookup = new Dictionary<string, OrganizationHierarchy>();
        if (orgUnitCodes.Any())
        {
            orgHierarchyLookup = organizationHierarchyRepository.GetAll()
                .Where(o => orgUnitCodes.Contains(o.Code) && !string.IsNullOrEmpty(o.Code))
                .GroupBy(o => o.Code)
                .ToDictionary(g => g.Key, g => g.First());
        }

        // Map entities to models with efficient user and org lookup
        // RBAC interceptor automatically applies row/column filtering, but we still need permissions for frontend
        var contactsWithPermissions = new List<ContactModel>();
        
        foreach (var entity in entities)
        {
            var contact = MapEntityToModelWithUserInfo(entity, mapper, userInfoLookup, orgHierarchyLookup);
            
            // Add permissions for frontend UI (RBAC interceptor handles security enforcement)
            contact.Permissions = await GetEntityPermissionsAsync(entity, user);
            
            contactsWithPermissions.Add(contact);
        }

        return new PaginationResponse<ContactModel>
        {
            Records = contactsWithPermissions,
            TotalCount = totalCount
        };
    }

    [RBAC("read", Entity = "Contact", EntityIdParameterName = "id")]
    public async Task<ContactModel?> GetContactAsync(ClaimsPrincipal user, int id)
    {
        var item = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerOffice"]);
        if (item == null) return null;

        // RBAC interceptor automatically checks entity access and applies column filtering
        // But we still need to add permissions for frontend UI
        return await MapEntityToModelWithPermissionsAsync(item, mapper, user);
    }

    [RBAC("update", Entity = "Contact")]
    public async Task<ContactModel?> UpdateContactAsync(ClaimsPrincipal user, UpdateContactRequest model)
    {
        var entity = await contactRepository.GetByIdAsync(model.Id, ["Partner", "Partner.PartnerOffice"]);
        if (entity == null)
        {
            throw new BusinessException($"Contact {model.Id} does not exist.");
        }

        // RBAC interceptor automatically checks update permissions
        // Manual entity access check still needed since interceptor can't determine 
        // entity ID from model.Id automatically
        if (_securityService != null && !await _securityService.CanUserAccessEntityAsync(entity, user, "update"))
        {
            throw new UnauthorizedAccessException("You don't have permission to update this contact");
        }

        entity = MapModelToEntity(model, entity);
        await contactRepository.UpdateAsync(entity);

        // Return with permissions for frontend UI
        return await MapEntityToModelWithPermissionsAsync(entity, mapper, user);
    }

    [RBAC("delete", Entity = "Contact", EntityIdParameterName = "id")]
    public async Task DeleteContactAsync(ClaimsPrincipal user, int id)
    {
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerOffice"]);
        if (entity == null) return;

        // RBAC interceptor automatically handles all permission and entity access checks
        await contactRepository.Delete(entity);
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
            .Where(o => orgUnitCodes.Contains(o.Code) && !string.IsNullOrEmpty(o.Code) && o.Type == OrganizationUnitType.OrgUnit)
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

    /// <summary>
    /// Gets a contact with its interactions included
    /// </summary>
    public async Task<ContactModel?> GetContactWithInteractionsAsync(int id)
    {
        string[] includes = ["Documents", "Partner", "Partner.PartnerOffice"];

        var item = await contactRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        // Manually load interactions through the InteractionContacts junction table
        var interactionContacts = await _context.InteractionContacts
            .Where(ic => ic.ContactId == id)
            .Include(ic => ic.Interaction)
            .ToListAsync();

        // Assign interactions to the contact
        if (interactionContacts.Any())
        {
            item.Interactions = interactionContacts.Select(ic => ic.Interaction).ToList();
        }

        // Now you can access interactions from the contact entity
        // Examples:
        // var recentInteractions = item.Interactions?.OrderByDescending(i => i.Date).Take(5).ToList();
        // var interactionCount = item.Interactions?.Count ?? 0;

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

    /// <summary>
    /// Implementation of abstract method from BaseUNOPSManager
    /// </summary>
    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        return await GetContactAsync(user, entityId);
    }
}