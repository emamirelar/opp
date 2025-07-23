using UNOPS.PAO.Domain.Specifications;
using System.Linq;
using UNOPS.PAO.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.UNOPSBusiness.Specifications;

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
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
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
    private readonly ILogger<UNOPSContactManager>? _logger;
    private readonly DataRepository<AiPrompt> promptRepository;

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

    public UNOPSContactManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, IPermissionService permissionService, IHttpContextAccessor httpContextAccessor = null, ILogger<UNOPSContactManager> logger = null)
        : base(mapper, context, configuration, null, "Contact", permissionService, httpContextAccessor)
    {
        this.mapper = mapper;
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration);
        partnerRepository = new BaseRepository<UNOPSPartner>(context, configuration);
        userInfoRepository = new BaseRepository<UserInfo>(context, configuration);
        organizationHierarchyRepository = new BaseRepository<OrganizationHierarchy>(context, configuration);
        promptRepository = new DataRepository<AiPrompt>(context);
        commonRepository = new CommonEntityRepository(context);
        googleCloudStorageService = new GoogleCloudStorageService(configuration);
        _logger = logger;
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
            .GetAll(["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"])
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
            .GetAll(["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"])
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
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"]);
        if (entity == null) return null;

        // Check if user has permission to access this specific entity
        // Create a single-item query and apply access control filters
        var query = contactRepository
            .GetAll(["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"])
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
        var updatedEntity = await contactRepository.GetByIdAsync(entity.Id, ["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"]);
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
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"]);
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
        var contacts = contactRepository.GetAll(["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"]).Where(c => c.PartnerId == partnerId);
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

    private async Task<object> GetUNOPSContactsWithSpecificationAsync(ClaimsPrincipal user, ISpecification<UNOPSContact> specification, PaginationRequest pagination)
    {
        // Apply the specification directly to the UNOPSContact query
        var query = contactRepository
            .GetAll(["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"])
            .AsQueryable();
        
        var filteredQuery = query.ApplySpecification(specification);

        // Apply access control filters (row and column filtering) BEFORE pagination
        var filteredData = await ApplyAccessControlFilters(filteredQuery, user, "read");
        
        // If filteredData is a list, we need to handle pagination manually
        if (filteredData is IEnumerable<UNOPSContact> contactList)
        {
            var contactArray = contactList.ToArray();
            var totalCount = contactArray.Length;
            var pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
            var excludedRows = (pageIndex - 1) * pagination.PageSize;
            
            var pagedItems = contactArray
                .Skip(excludedRows)
                .Take(pagination.PageSize)
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
                PageSize = pagination.PageSize
            };
        }

        // Fallback: if filteredData is not the expected type, return empty result
        return new PaginationResponse<ContactModel>
        {
            Records = new List<ContactModel>(),
            TotalCount = 0,
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize
        };
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
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"]);
        if (entity == null) return null;

        return await MapEntityToModel(entity, mapper, GetCurrentUserOrSystemContext());
    }

    public async Task<ContactModel?> GetContactWithInteractionsAsync(int id)
    {
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup", "Interactions"]);
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

    public async Task<object> GetContactsWithSpecificationAsync(ClaimsPrincipal user, ISpecification<Contact> specification, PaginationRequest pagination)
    {
        // Check if this is an adapted UNOPSContact specification
        if (specification is ContactSpecificationAdapter adapter)
        {
            return await GetUNOPSContactsWithSpecificationAsync(user, adapter.GetOriginalSpecification(), pagination);
        }
        
        // Apply the specification to the query
        var query = contactRepository
            .GetAll(["Partner", "Partner.OrganizationUnitRelationships", "Partner.OrganizationUnitRelationships.OrganizationHierarchy", "Partner.PartnerGroup"])
            .AsQueryable();
        
        var filteredQuery = query.ApplySpecification(specification);

        // Apply access control filters (row and column filtering) BEFORE pagination
        var filteredData = await ApplyAccessControlFilters(filteredQuery, user, "read");
        
        // If filteredData is a list, we need to handle pagination manually
        if (filteredData is IEnumerable<UNOPSContact> contactList)
        {
            var contactArray = contactList.ToArray();
            var totalCount = contactArray.Length;
            var pageIndex = pagination.PageIndex < 1 ? 1 : pagination.PageIndex;
            var excludedRows = (pageIndex - 1) * pagination.PageSize;
            
            var pagedItems = contactArray
                .Skip(excludedRows)
                .Take(pagination.PageSize)
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
                PageSize = pagination.PageSize
            };
        }

        // Fallback: if filteredData is not the expected type, return empty result
        return new PaginationResponse<ContactModel>
        {
            Records = new List<ContactModel>(),
            TotalCount = 0,
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize
        };
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

    public async Task<List<ContactModel?>> GetContactsForGmailAddon(GmailRelatedRecordsRequest input, ClaimsPrincipal user = null)
    {
        var contacts = contactRepository
                        .GetAll(["Partner"])
                        .AsQueryable()
                        .Where(c => (c.Email != null && input.EmailAddresses.Contains(c.Email)))
                        .Cast<UNOPSContact>()
                        .ToList();

        // Get all contact IDs to load interactions
        var allContactIds = contacts.Select(c => c.Id).ToList();

        // Get interactions through the InteractionContacts junction table with full interaction entities for permission checking
        var interactionContacts = await _context.InteractionContacts
            .Where(ic => allContactIds.Contains(ic.ContactId))
            .Include(ic => ic.Interaction)
            .Select(ic => new
            {
                ic.ContactId,
                Interaction = ic.Interaction
            })
            .ToListAsync();

        // Group interactions by contact ID for efficient lookup
        var interactionsByContact = interactionContacts
            .GroupBy(ic => ic.ContactId)
            .ToDictionary(g => g.Key, g => g.Select(ic => ic.Interaction).ToList());

        // Batch permission lookup once for all contacts
        var userPermissions = await GetEntityPermissionsAsync(user, "Contact");

        var mappedContacts = new List<ContactModel>();
        foreach (var contact in contacts)
        {
            var model = await MapEntityToModel(contact, mapper, user);
            /*model.Permissions = new EntityPermissionsModel
            {
                CanRead = userPermissions.CanRead,
                CanCreate = userPermissions.CanCreate,
                CanUpdate = userPermissions.CanUpdate && await CanUserAccessEntityAsync(contact, user, "update"),
                CanDelete = userPermissions.CanDelete && await CanUserAccessEntityAsync(contact, user, "delete")
            };*/

            // Add interactions directly to the contact with Id, Type, Description, Date, and Permissions
            if (interactionsByContact.TryGetValue(contact.Id, out var contactInteractions))
            {
                var interactionModels = new List<InteractionModel>();
                foreach (var interaction in contactInteractions)
                {
                    var interactionModel = new InteractionModel
                    {
                        Id = interaction.Id,
                        Type = interaction.Type,
                        Description = interaction.Description,
                        Date = interaction.Date,
                        Permissions = new EntityPermissionsModel
                        {
                            CanRead = await _permissionService.HasInstanceAccessAsync("Interaction", interaction, user, "read"),
                            CanCreate = await _permissionService.HasInstanceAccessAsync("Interaction", interaction, user, "create"),
                            CanUpdate = await _permissionService.HasInstanceAccessAsync("Interaction", interaction, user, "update"),
                            CanDelete = await _permissionService.HasInstanceAccessAsync("Interaction", interaction, user, "delete")
                        }
                    };
                    interactionModels.Add(interactionModel);
                }
                model.Interactions = interactionModels;
            }

            mappedContacts.Add(model);
        }

        return mappedContacts;
    }

    public async Task<List<UnmatchedEmailModel>> GetUnmatchedEmailsWithPartnerSuggestionsAsync(List<string> emailAddresses, ClaimsPrincipal user = null)
    {
        var unmatchedEmails = new List<UnmatchedEmailModel>();
        var domainsForGemini = new List<string>();
        var emailDomainMapping = new Dictionary<string, string>();
        
        // First pass: Check database for existing matches and collect domains for Gemini lookup
        foreach (var email in emailAddresses)
        {
            var unmatchedEmail = new UnmatchedEmailModel
            {
                UnmatchedEmail = email
            };
            
            // Extract domain from email
            var emailDomain = email.Split('@').LastOrDefault();
            if (string.IsNullOrEmpty(emailDomain))
            {
                unmatchedEmails.Add(unmatchedEmail);
                continue;
            }
            
            emailDomainMapping[email] = emailDomain;
            
            // Look up contacts with the same domain
            var contactsWithSameDomain = await contactRepository.GetAll(["Partner"])
                .AsQueryable()
                .Where(c => !string.IsNullOrEmpty(c.Email) && c.Email.Contains($"@{emailDomain}"))
                .ToListAsync();
            
            if (contactsWithSameDomain.Any())
            {
                // Find the most occurring PartnerId
                var partnerIdCounts = contactsWithSameDomain
                    .GroupBy(c => c.PartnerId)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                
                if (partnerIdCounts != null)
                {
                    var mostCommonPartnerId = partnerIdCounts.Key;
                    var partner = await partnerRepository.GetByIdAsync(mostCommonPartnerId);
                    
                    if (partner != null)
                    {
                        unmatchedEmail.PartnerId = mostCommonPartnerId;
                        unmatchedEmail.PartnerName = partner.Name;
                    }
                }
            }
            else
            {
                // No contacts found with same domain, collect for Gemini lookup
                if (!domainsForGemini.Contains(emailDomain))
                {
                    domainsForGemini.Add(emailDomain);
                }
            }
            
            unmatchedEmails.Add(unmatchedEmail);
        }
        
        // Second pass: Batch Gemini lookup for all domains without database matches
        if (domainsForGemini.Any())
        {
            try
            {
                var geminiResults = await GetPartnerNamesFromGeminiAsync(domainsForGemini);
                
                // Apply Gemini results to unmatched emails
                foreach (var unmatchedEmail in unmatchedEmails)
                {
                    if (string.IsNullOrEmpty(unmatchedEmail.PartnerName) && 
                        emailDomainMapping.TryGetValue(unmatchedEmail.UnmatchedEmail, out var domain) &&
                        geminiResults.TryGetValue(domain, out var organizationName))
                    {
                        unmatchedEmail.PartnerName = organizationName;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"Failed to get partner names from Gemini for domains: {ex.Message}");
            }
        }
        
        return unmatchedEmails;
    }
    
    private async Task<Dictionary<string, string>> GetPartnerNamesFromGeminiAsync(List<string> domains)
    {
        var result = new Dictionary<string, string>();
        
        // Initialize with fallback values
        foreach (var domain in domains)
        {
            result[domain] = $"Organization for {domain}";
        }
        
        if (!domains.Any())
        {
            return result;
        }
        
        try
        {
            // Get the prompt configuration from the AiPrompt table
            var promptConfig = promptRepository.GetAll()
                .Where(p => p.Type == "domain_organization_lookup" && p.Status == EntityStatus.Active)
                .FirstOrDefault();
            
            if (promptConfig == null)
            {
                _logger?.LogWarning("No active AiPrompt found for domain_organization_lookup");
                return result;
            }
            
            // Create the prompt data as JSON array of domains
            var domainsJson = System.Text.Json.JsonSerializer.Serialize(domains);
            
            // Use the existing AI service to make the call
            var aiService = new AiContextualService(_configuration, _context, null);
            var response = await aiService.FetchResultFromGemini(promptConfig, domainsJson);
            
            // Parse the response - expecting a JSON array
            try
            {
                var parsedResponse = aiService.GetDetailsFromGeminiResponse(response);
                var responseText = parsedResponse["Message"]?.ToString() ??
                                    parsedResponse["text"]?.ToString() ?? 
                                    parsedResponse["content"]?.ToString() ?? 
                                    response.Trim();
                
                // Clean up the response text
                responseText = responseText?.Trim()?.Trim('"');
                
                // Parse the JSON response
                if (!string.IsNullOrEmpty(responseText))
                {
                    var organizationResults = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(responseText);
                    
                    if (organizationResults != null)
                    {
                        foreach (var orgResult in organizationResults)
                        {
                            if (orgResult.TryGetValue("domain", out var domain) && 
                                orgResult.TryGetValue("organization", out var organization))
                            {
                                if (!string.IsNullOrEmpty(organization) && 
                                    organization != "Unknown" && 
                                    !organization.Contains("cannot") &&
                                    !organization.Contains("unable"))
                                {
                                    result[domain] = organization;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception parseEx)
            {
                _logger?.LogWarning($"Failed to parse Gemini response for domain lookup: {parseEx.Message}. Response: {response}");
                
                // Try to extract text directly from response if JSON parsing fails
                var cleanResponse = response?.Trim()?.Trim('"');
                if (!string.IsNullOrEmpty(cleanResponse) && cleanResponse != "Unknown")
                {
                    // If single domain and simple text response, use it
                    if (domains.Count == 1)
                    {
                        result[domains[0]] = cleanResponse;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Failed to get organization names from Gemini for domains: {ex.Message}");
        }
        
        return result;
    }
}