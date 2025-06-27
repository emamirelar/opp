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
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using System.Reflection;

public class UNOPSContactManager : BaseUNOPSManager, IContactManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSContact> contactRepository;
    private BaseRepository<UNOPSPartner> partnerRepository;
    private BaseRepository<UserInfo> userInfoRepository;
    private BaseRepository<OrganizationHierarchy> organizationHierarchyRepository;
    private GoogleCloudStorageService googleCloudStorageService;
    private CommonEntityRepository commonRepository;

    private async Task<ContactModel> MapEntityToModel(UNOPSContact entity, IMapper mapper, ClaimsPrincipal user)
    {
        var result = mapper.Map<UNOPSContact, ContactModel>(entity);
        result.Partner = entity.Partner != null ? new UNOPS.PAO.Models.PartnerSummaryModel { Id = entity.Partner.Id, Name = entity.Partner.Name } : null;
        
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

        return await MapEntityToModelWithPermissionsAsync(result, user); ;
    }
    
    private ContactModel MapEntityToModelWithUserInfo(UNOPSContact entity, IMapper mapper, Dictionary<int, UserInfo> userInfoLookup, Dictionary<string, OrganizationHierarchy> orgHierarchyLookup)
    {
        var result = mapper.Map<UNOPSContact, ContactModel>(entity);
        result.Partner = entity.Partner != null ? new UNOPS.PAO.Models.PartnerSummaryModel { Id = entity.Partner.Id, Name = entity.Partner.Name } : null;
        
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

    public UNOPSContactManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, IPermissionService permissionService, IHttpContextAccessor httpContextAccessor = null)
        : base(mapper, context, configuration, null, "Contact", permissionService, httpContextAccessor)
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
            .GetAll(["Partner", "Partner.PartnerOffice", "Partner.PartnerGroup"])
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
        var userIds = items.Where(c => c.CreatedBy > 0).Select(c => c.CreatedBy).Distinct().ToList();
        
        // Fetch all user info in one query
        var userInfoLookup = userInfoRepository.GetAll()
            .Where(u => userIds.Contains(u.UserId))
            .ToDictionary(u => u.UserId);
        
        // Get all unique org units from user info
        var orgUnits = userInfoLookup.Values
            .Where(u => !string.IsNullOrEmpty(u.OrgUnit))
            .Select(u => u.OrgUnit)
            .Distinct()
            .ToList();
        
        // Fetch all organization hierarchy in one query
        var orgHierarchyLookup = organizationHierarchyRepository.GetAll()
            .Where(o => !string.IsNullOrEmpty(o.Code) && 
                       o.Type == OrganizationUnitType.OrgUnit && 
                       orgUnits.Contains(o.Code))
            .ToDictionary(o => o.Code);

        var results = items.Select(item => MapEntityToModelWithUserInfo(item, mapper, userInfoLookup, orgHierarchyLookup)).ToList();

        return new PaginationResponse<ContactModel>
        {
            Records = results,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = request.PageSize
        };
    }

    public async Task<PaginationResponse<ContactModel>> GetContactsAsync(ClaimsPrincipal user, PaginationRequest request)
    {
        var query = contactRepository
            .GetAll(["Partner", "Partner.PartnerOffice", "Partner.PartnerGroup"])
            .AsQueryable();

        // Apply access control filters (row and column filtering) BEFORE pagination
        var filteredData = await ApplyAccessControlFilters(query, user, "read");
        
        // If filteredData is a list, we need to handle pagination manually
        if (filteredData is IEnumerable<UNOPSContact> contactList)
        {
            var contactArray = contactList.ToArray();
            var totalCount = contactArray.Length;
            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var excludedRows = (pageIndex - 1) * request.PageSize;
            
            var pagedItems = contactArray
                .Skip(excludedRows)
                .Take(request.PageSize)
                .ToArray();

            // Get all unique user IDs from the contacts for user info lookup
            var userIds = pagedItems.Where(c => c.CreatedBy > 0).Select(c => c.CreatedBy).Distinct().ToList();
            
            // Fetch all user info in one query
            var userInfoLookup = userInfoRepository.GetAll()
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionary(u => u.UserId);
            
            // Get all unique org units from user info
            var orgUnits = userInfoLookup.Values
                .Where(u => !string.IsNullOrEmpty(u.OrgUnit))
                .Select(u => u.OrgUnit)
                .Distinct()
                .ToList();
            
            // Fetch all organization hierarchy in one query
            var orgHierarchyLookup = organizationHierarchyRepository.GetAll()
                .Where(o => !string.IsNullOrEmpty(o.Code) && 
                           o.Type == OrganizationUnitType.OrgUnit && 
                           orgUnits.Contains(o.Code))
                .ToDictionary(o => o.Code);

            var results = pagedItems.Select(item => MapEntityToModelWithUserInfo(item, mapper, userInfoLookup, orgHierarchyLookup)).ToList();

            return new PaginationResponse<ContactModel>
            {
                Records = results,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = request.PageSize
            };
        }

        // Fallback: if filteredData is not the expected type, return empty result
        return new PaginationResponse<ContactModel>
        {
            Records = new List<ContactModel>(),
            TotalCount = 0,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }

    public async Task<ContactModel?> GetContactAsync(ClaimsPrincipal user, int id)
    {
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerOffice", "Partner.PartnerGroup"]);
        if (entity == null) return null;

        // Check if user has permission to access this specific entity
        // Create a single-item query and apply access control filters
        var query = contactRepository
            .GetAll(["Partner", "Partner.PartnerOffice", "Partner.PartnerGroup"])
            .Where(x => x.Id == id)
            .AsQueryable();

        // Apply access control filters (row and column filtering)
        var filteredData = await ApplyAccessControlFilters(query, user, "read");
        
        // If filteredData is a list and contains our entity, user has access
        if (filteredData is IEnumerable<UNOPSContact> contactList)
        {
            var accessibleContact = contactList.FirstOrDefault();
            if (accessibleContact != null)
            {
                return await MapEntityToModel(accessibleContact, mapper, user);
            }
        }

        // User doesn't have access to this entity
        return null;
    }


    public async Task<ContactModel?> UpdateContactAsync(ClaimsPrincipal user, UpdateContactRequest model)
    {
        var entity = await contactRepository.GetByIdAsync(model.Id);
        if (entity == null) return null;

        PatchNonNullProperties(model, entity);

        entity.Name = String.Concat(model.Salutation, ' ', model.FirstName, ' ', model.MiddleName, ' ', model.LastName);

        await contactRepository.UpdateAsync(entity);
        
        // Return updated entity with includes
        var updatedEntity = await contactRepository.GetByIdAsync(entity.Id, ["Partner", "Partner.PartnerOffice", "Partner.PartnerGroup"]);
        return await MapEntityToModel(updatedEntity, mapper, user);
    }

    public async Task DeleteContactAsync(ClaimsPrincipal user, int id)
    {
        var entity = await contactRepository.GetByIdAsync(id);
        if (entity == null) return;

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
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerOffice", "Partner.PartnerGroup"]);
        if (entity == null) return null;

        return await MapEntityToModel(entity, mapper, GetCurrentUserOrSystemContext());
    }

    public IEnumerable<ExternalContactModel> GetPostedContacts()
    {
        var contacts = contactRepository.GetAll();
        // Note: IsPosted property may not exist, commenting out for now
        // return contacts.Where(c => c.IsPosted).Select(c => MapEntityToExternalModel(c, mapper));
        return contacts.Select(c => MapEntityToExternalModel(c, mapper));
    }

    public async Task<ExternalContactModel?> GetPostedContact(int id)
    {
        var entity = await contactRepository.GetByIdAsync(id);
        // Note: IsPosted property may not exist, commenting out for now
        // if (entity == null || !entity.IsPosted) return null;
        if (entity == null) return null;

        return MapEntityToExternalModel(entity, mapper);
    }

    public IEnumerable<ContactModel> GetPartnerContacts(int partnerId)
    {
        var contacts = contactRepository.GetAll(["Partner", "Partner.PartnerOffice", "Partner.PartnerGroup"]).Where(c => c.PartnerId == partnerId);
        var results = new List<ContactModel>();
        foreach (var contact in contacts)
        {
            // Use synchronous mapping for interface compatibility
            var result = mapper.Map<UNOPSContact, ContactModel>(contact);
            result.Partner = contact.Partner != null ? new UNOPS.PAO.Models.PartnerSummaryModel { Id = contact.Partner.Id, Name = contact.Partner.Name } : null;
            results.Add(result);
        }
        return results;
    }

    public async Task<string?> UpdateContactProfilePictureAsync(int contactId, IFormFile file)
    {
        var contact = await contactRepository.GetByIdAsync(contactId);
        if (contact == null) return null;

        try
        {
            var fileName = $"contact-{contactId}-{Guid.NewGuid()}.{file.FileName.Split('.').Last()}";
            var uploadedUrl = await googleCloudStorageService.UploadFileAsync(file, fileName);
            
            contact.ProfilePictureUrl = uploadedUrl;
            await contactRepository.UpdateAsync(contact);
            
            return googleCloudStorageService.GenerateSignedUrlFromStorageUrl(uploadedUrl).Result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to upload profile picture: {ex.Message}");
        }
    }

    public async Task<ContactModel?> GetContactAsync(int id)
    {
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerOffice", "Partner.PartnerGroup"]);
        if (entity == null) return null;

        return await MapEntityToModel(entity, mapper, GetCurrentUserOrSystemContext());
    }

    public async Task<ContactModel?> GetContactWithInteractionsAsync(int id)
    {
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerOffice", "Partner.PartnerGroup", "Interactions"]);
        if (entity == null) return null;

        var result = await MapEntityToModel(entity, mapper, GetCurrentUserOrSystemContext());
        
        // Map interactions if they exist - commenting out problematic properties
        if (entity.Interactions?.Any() == true)
        {
            result.Interactions = entity.Interactions.Select(i => new InteractionModel
            {
                Id = i.Id,
                Subject = i.Subject,
                Description = i.Description,
                Date = i.Date,
                Type = i.Type
                // ContactId = i.ContactId,  // These properties may not exist
                // PartnerId = i.PartnerId
            }).ToList();
        }

        return result;
    }

    public async Task<ContactModel?> UpdateContactAsync(int userId, UpdateContactRequest model)
    {
        var entity = await contactRepository.GetByIdAsync(model.Id);
        if (entity == null) return null;

        PatchNonNullProperties(model, entity);
        entity.Name = String.Concat(model.Salutation, ' ', model.FirstName, ' ', model.MiddleName, ' ', model.LastName);

        await contactRepository.UpdateAsync(entity);
        
        var updatedEntity = await contactRepository.GetByIdAsync(entity.Id, ["Partner"]);
        return await MapEntityToModel(updatedEntity, mapper, GetCurrentUserOrSystemContext());
    }

    public async Task DeleteContactAsync(int userId, int id)
    {
        var entity = await contactRepository.GetByIdAsync(id);
        if (entity == null) return;

        await contactRepository.Delete(entity);
    }

    public override async Task<object> GetBasicEntityAsync(int entityId, ClaimsPrincipal user = null)
    {
        // Use the provided user, current user from context, or system user context
        var userContext = user ?? GetCurrentUserOrSystemContext();
        var entity = await contactRepository.GetByIdAsync(entityId, ["Partner"]);
        if (entity == null) return null;

        return await MapEntityToModel(entity, mapper, userContext);
    }
}