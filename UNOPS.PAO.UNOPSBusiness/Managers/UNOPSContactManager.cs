using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSBusiness.Extensions;
using UNOPS.PAO.UNOPSBusiness.Specifications;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Utilities.Helpers;

public class UNOPSContactManager : BaseUNOPSManager, IContactManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSContact> contactRepository;
    private BaseRepository<UNOPSPartner> partnerRepository;
    private BaseRepository<UserProfile> userInfoRepository;
    private BaseRepository<OrganizationHierarchy> organizationHierarchyRepository;
    private GoogleCloudStorageService googleCloudStorageService;
    private CommonEntityRepository commonRepository;
    private readonly ILogger<UNOPSContactManager>? _logger;
    private readonly DataRepository<AiPrompt> promptRepository;
    private readonly UNOPSAppDbContext _context;

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
    
    private ContactModel MapEntityToModelWithUserInfo(UNOPSContact entity, IMapper mapper, Dictionary<int, UserProfile> userInfoLookup, Dictionary<string, OrganizationHierarchy> orgHierarchyLookup)
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
        return MapModelToEntity(model, new UNOPSContact
        {
            LastName = model.LastName ?? "Unknown",
            Title = model.Title ?? "Unknown",
            Email = model.Email ?? "unknown@example.com"
        });
    }

    public UNOPSContactManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration, IPermissionService permissionService, IHttpContextAccessor httpContextAccessor = null, ILogger<UNOPSContactManager> logger = null, IServiceProvider serviceProvider = null)
        : base(mapper, context, configuration, null, "Contact", permissionService, httpContextAccessor)
    {
        this.mapper = mapper;
        _context = context;
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration, serviceProvider);
        partnerRepository = new BaseRepository<UNOPSPartner>(context, configuration, serviceProvider);
        userInfoRepository = new BaseRepository<UserProfile>(context, configuration, serviceProvider);
        organizationHierarchyRepository = new BaseRepository<OrganizationHierarchy>(context, configuration, serviceProvider);
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
            .GetAll(["Partner", "Partner.PartnerGroup"])
            .AsQueryable();

        // Custom pagination with efficient user lookup
        var totalCount = query.Count();
        var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
        var excludedRows = (pageIndex - 1) * request.PageSize;
        
        var items = query
            .Skip(excludedRows)
            .Take(request.PageSize)
            .ToList();

        // Load OrganizationUnitRelationships manually for all items
        /*foreach (var item in items.Where(i => i.Partner != null))
        {
            item.Partner.LoadOrganizationUnitRelationshipsAsync(_context).Wait();
        }*/

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
            .GetAll(["Partner", "Partner.PartnerGroup"])
            .AsQueryable();

        // Load OrganizationUnitRelationships manually for all items
        /*foreach (var qItem in query.Where(i => i.Partner != null))
        {
            await qItem.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
        }*/

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

            // Load OrganizationUnitRelationships manually for all items
            /*foreach (var item in pagedItems.Where(i => i.Partner != null))
            {
                await item.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
            }*/

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
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerGroup"]);
        if (entity == null) return null;

        // Load OrganizationUnitRelationships manually
        /*if (entity.Partner != null)
        {
            await entity.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
        }*/

        // Check if user has permission to access this specific entity
        // Create a single-item query and apply access control filters
        var query = contactRepository
            .GetAll(["Partner", "Partner.PartnerGroup"])
            .Where(x => x.Id == id)
            .AsQueryable();

        // Load OrganizationUnitRelationships manually for all items
        /*foreach (var qItem in query.Where(i => i.Partner != null))
        {
            await qItem.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
        }*/

        // Apply access control filters (row and column filtering)
        var filteredData = await ApplyAccessControlFilters(query, user, "read");
        
        // If filteredData is a list and contains our entity, user has access
        if (filteredData is IEnumerable<UNOPSContact> contactList)
        {
            var accessibleContact = contactList.FirstOrDefault();
            if (accessibleContact != null)
            {
                // Load OrganizationUnitRelationships manually
                /*if (accessibleContact.Partner != null)
                {
                    await accessibleContact.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
                }*/
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
        var updatedEntity = await contactRepository.GetByIdAsync(entity.Id, ["Partner", "Partner.PartnerGroup"]);
        
        // Load OrganizationUnitRelationships manually
        /*if (updatedEntity?.Partner != null)
        {
            await updatedEntity.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
        }*/
        
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

        // Load OrganizationUnitRelationships manually for all items
        /*foreach (var qItem in query.Where(i => i.Partner != null))
        {
            qItem.Partner.LoadOrganizationUnitRelationshipsAsync(_context).Wait();
        }*/

        // Cast to base type to apply specification, then cast back to derived type
        var baseQuery = query.Cast<Contact>();
        var filteredBaseQuery = baseQuery.ApplySpecification(specification);
        var filteredQuery = filteredBaseQuery.OfType<UNOPSContact>();
        
        // Apply org unit filtering if the specification supports it
        filteredQuery = ApplyOrgUnitFilterIfSupported(filteredQuery, specification);

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
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerGroup"]);
        if (entity == null) return null;

        // Load OrganizationUnitRelationships manually
        /*if (entity.Partner != null)
        {
            await entity.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
        }*/

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
        var contacts = contactRepository.GetAll(["Partner", "Partner.PartnerGroup"]).Where(c => c.PartnerId == partnerId).ToList();
        
        // Load OrganizationUnitRelationships manually for all contacts
        /*foreach (var contact in contacts.Where(c => c.Partner != null))
        {
            contact.Partner.LoadOrganizationUnitRelationshipsAsync(_context).Wait();
        }*/
        
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
            .GetAll(["Partner", "Partner.PartnerGroup"])
            .AsQueryable();

        // Load OrganizationUnitRelationships manually for all items
        /*foreach (var qItem in query.Where(i => i.Partner != null))
        {
            await qItem.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
        }*/

        var filteredQuery = query.ApplySpecification(specification);
        
        // Apply org unit filtering if the specification supports it
        filteredQuery = ApplyUNOPSContactOrgUnitFilterIfSupported(filteredQuery, specification);

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

            // Load OrganizationUnitRelationships manually for all items
            /*foreach (var item in pagedItems.Where(i => i.Partner != null))
            {
                await item.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
            }*/

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
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerGroup"]);
        if (entity == null) return null;

        // Load OrganizationUnitRelationships manually
        /*if (entity.Partner != null)
        {
            await entity.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
        }*/

        return await MapEntityToModel(entity, mapper, GetCurrentUserOrSystemContext());
    }

    /// <summary>
    /// Gets contact with interactions formatted for AI prompt processing
    /// </summary>
    public async Task<object> GetContactWithInteractionsAsync(ClaimsPrincipal user, int id)
    {
        var entity = await contactRepository.GetByIdAsync(id, ["Partner", "Partner.PartnerGroup", "Interactions", "Documents"]);
        if (entity == null) return new { error = "Contact not found" };

        // Create structured JSON for AI prompt placeholders
        var result = new
        {
            id = entity.Id,
            fullName = $"{entity.FirstName} {entity.LastName}".Trim(),
            firstName = entity.FirstName,
            middleName = entity.MiddleName,
            lastName = entity.LastName,
            suffix = entity.Suffix,
            salutation = entity.Salutation,
            email = entity.Email,
            title = entity.Title,
            department = entity.Department,
            description = entity.Description,
            phone = entity.Phone,
            mobile = entity.Mobile,
            status = entity.Status.ToString(),
            
            // Partner information
            partner = entity.Partner != null ? new
            {
                id = entity.Partner.Id,
                name = entity.Partner.Name,
                status = entity.Partner.Status.ToString(),
                partnerGroup = entity.Partner.PartnerGroup?.Name,
                liaisonOffice = entity.Partner.LiaisonOffice?.Name
            } : null,
            
            // Interaction history with full details
            interactions = entity.Interactions?.Select(i => new
            {
                id = i.Id,
                subject = i.Subject,
                description = i.Description,
                date = i.Date.ToString("yyyy-MM-dd HH:mm"),
                type = i.Type.ToString(),
                location = i.Location,
                status = "Active" // Default status for interactions
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Contact details and communication info
            contactDetails = new
            {
                profilePictureUrl = entity.ProfilePictureUrl,
                hasProfilePicture = !string.IsNullOrEmpty(entity.ProfilePictureUrl)
            },
            
            // Mailing address information
            mailingAddress = !string.IsNullOrEmpty(entity.MailingStreet) ? new
            {
                street = entity.MailingStreet,
                street2 = entity.MailingStreet2,
                city = entity.MailingCity,
                state = entity.MailingStateProvince,
                postalCode = entity.MailingPostalCode,
                country = entity.MailingCountry,
                fullAddress = string.Join(", ", new[] {
                    entity.MailingStreet,
                    entity.MailingStreet2,
                    entity.MailingCity,
                    entity.MailingStateProvince,
                    entity.MailingPostalCode,
                    entity.MailingCountry
                }.Where(s => !string.IsNullOrEmpty(s)))
            } : null,
            
            // Assistant information
            assistant = !string.IsNullOrEmpty(entity.Assistant) ? new
            {
                name = entity.Assistant,
                phone = entity.AssistantPhone,
                email = entity.AssistantEmail
            } : null,
            
            // Documents and attachments
            documents = entity.Documents?.Select(d => new
            {
                id = d.Id,
                link = d.Link,
                type = d.Type,
                documentType = d.DocumentType?.Name,
                uploadDate = d.CreatedDate.ToString("yyyy-MM-dd"),
                isCV = d.Link?.ToLower().Contains("cv") == true || 
                       d.Link?.ToLower().Contains("resume") == true ||
                       d.Type?.ToLower().Contains("cv") == true ||
                       d.Type?.ToLower().Contains("resume") == true,
                downloadUrl = d.Link
            }).Cast<dynamic>().ToList() ?? new List<dynamic>(),
            
            // Summary statistics
            summary = new
            {
                totalInteractions = entity.Interactions?.Count ?? 0,
                totalDocuments = entity.Documents?.Count ?? 0,
                hasCV = entity.Documents?.Any(d => 
                    d.Link?.ToLower().Contains("cv") == true || 
                    d.Link?.ToLower().Contains("resume") == true ||
                    d.Type?.ToLower().Contains("cv") == true ||
                    d.Type?.ToLower().Contains("resume") == true) ?? false,
                lastInteractionDate = entity.Interactions?.OrderByDescending(i => i.Date).FirstOrDefault()?.Date.ToString("yyyy-MM-dd"),
                recentInteractions = entity.Interactions?.Where(i => i.Date >= DateTime.UtcNow.AddDays(-30)).Count() ?? 0
            },
            
            // Audit information
            auditInfo = new
            {
                createdDate = entity.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                lastModifiedDate = entity.LastModifiedDate?.ToString("yyyy-MM-dd HH:mm") ?? "Not modified",
                createdBy = entity.CreatedBy,
                lastModifiedBy = entity.LastModifiedBy
            },
            
            // User profile information for context
            userProfile = await GetUserProfileForAIAsync(user)
        };

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
            .GetAll(["Partner", "Partner.PartnerGroup"])
            .AsQueryable();

        // Load OrganizationUnitRelationships manually for all items
        /*foreach (var qItem in query.Where(i => i.Partner != null))
        {
            await qItem.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
        }*/

        // Cast to base type to apply specification, then cast back to derived type
        var baseQuery = query.Cast<Contact>();
        var filteredBaseQuery = baseQuery.ApplySpecification(specification);
        var filteredQuery = filteredBaseQuery.OfType<UNOPSContact>();
        
        // Apply org unit filtering if the specification supports it
        filteredQuery = ApplyOrgUnitFilterIfSupported(filteredQuery, specification);

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

            // Load OrganizationUnitRelationships manually for all items
            /*foreach (var item in pagedItems.Where(i => i.Partner != null))
            {
                await item.Partner.LoadOrganizationUnitRelationshipsAsync(_context);
            }*/

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
        var contact = await _context.Contacts.FirstOrDefaultAsync(e => e.Id == entityId);
        if (contact != null)
        {
            return mapper.Map<UNOPSContact, ContactModel>(contact);
        }
        return null;
    }

    /// <summary>
    /// Gets basic contact data by ID without nested entities
    /// </summary>
    public override async Task<object> GetBasicEntityDataAsync(int id)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(e => e.Id == id);
        if (contact != null)
        {
            return mapper.Map<UNOPSContact, ContactModel>(contact);
        }
        return null;
    }

    public async Task<List<ContactModel?>> GetContactsForGmailAddon(GmailRelatedRecordsRequest input, ClaimsPrincipal user = null)
    {
        // Convert input email addresses to lowercase for case-insensitive comparison
        var lowercaseEmailAddresses = input.EmailAddresses.Select(e => e.ToLower()).ToList();
        
        var contacts = await _context.Contacts
                                    .Where(c => (c.Email != null && lowercaseEmailAddresses.Contains(c.Email.ToLower())))
                                    .Include(c => c.Partner)
                                    .ToListAsync();

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
        //var userPermissions = await GetEntityPermissionsAsync(user, "Contact");

        var mappedContacts = new List<ContactModel>();
        foreach (var contact in contacts)
        {
            var model = await MapEntityToModel(contact, mapper, user);
            model.Permissions = new EntityPermissionsModel
            {
                CanRead = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "read"),
                CanCreate = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "create"),
                CanUpdate = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "update"),
                CanDelete = await _permissionService.HasInstanceAccessAsync("Contact", contact, user, "delete")
            };

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

    public async Task<ContactModel?> GetContactByEmailAsync(ClaimsPrincipal user, string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            // Query for contact with the specified email (case-insensitive)
            var contact = await _context.Contacts
                                .Where(c => c.Email.ToLower() == email.ToLower() && !c.IsDeleted)
                                .Include(c => c.Partner)
                                    .ThenInclude(cp => cp.PartnerGroup)
                                .AsQueryable()
                                .FirstOrDefaultAsync();

            if (contact == null)
            {
                return null;
            }

            // Apply access control filters to ensure user has permission to access this contact
            var query = _context.Contacts
                                .Where(c => c.Id == contact.Id)
                                .Include(c => c.Partner)
                                    .ThenInclude(cp => cp.PartnerGroup)
                                .AsQueryable();

            var filteredData = await ApplyAccessControlFilters(query, user, "read");
            
            if (filteredData is IEnumerable<UNOPSContact> contactList)
            {
                var accessibleContact = contactList.FirstOrDefault();
                if (accessibleContact != null)
                {
                    return await MapEntityToModel(accessibleContact, mapper, user);
                }
            }

            // User doesn't have access to this contact
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting contact by email: {Email}", email);
            return null;
        }
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
            var contactsWithSameDomain = await _context.Contacts
                                                        .Where(c => !string.IsNullOrEmpty(c.Email) && c.Email.Contains($"@{emailDomain}"))
                                                        .Include(c => c.Partner)
                                                        .AsQueryable()
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
    
    /// <summary>
    /// Gets partner names from domain lookup using AI - original private method for internal use
    /// </summary>
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

    /// <summary>
    /// Gets comprehensive partner names from domain lookup using AI, with full user context and analytics
    /// </summary>
    /// <summary>
    /// Gets partner names from Gemini processing for AI prompts - reflection-compatible version
    /// </summary>
    public async Task<object> GetPartnerNamesFromGeminiAsync(ClaimsPrincipal user, int id)
    {
        // Get contact details to extract domain information
        var contact = await contactRepository.GetByIdAsync(id, ["Partner"]);
        if (contact == null)
        {
            return new
            {
                contactId = id,
                domains = new List<string>(),
                partnerNames = new Dictionary<string, string>(),
                searchResults = new List<object>(),
                summary = new
                {
                    total = 0,
                    resolved = 0,
                    unresolved = 0,
                    successRate = "0%"
                },
                searchMetadata = new
                {
                    searchDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    searchMethod = "Gemini AI Processing"
                },
                domainAnalysis = new List<object>(),
                userProfile = await GetUserProfileForAIAsync(user)
            };
        }

        // Extract domain from contact's email if available
        var domains = new List<string>();
        if (!string.IsNullOrEmpty(contact.Email) && contact.Email.Contains("@"))
        {
            var domain = contact.Email.Split('@')[1];
            domains.Add(domain);
        }

        // If contact has partner, also try to extract domain from partner name or other info
        if (contact.Partner != null && !string.IsNullOrEmpty(contact.Partner.Name))
        {
            // You could add logic here to derive domains from partner names
            // For now, we'll work with email domain
        }

        // Call the existing method with domains
        return await GetPartnerNamesForAIAsync(user, domains);
    }

    public async Task<object> GetPartnerNamesForAIAsync(ClaimsPrincipal user, List<string> domains)
    {
        var partnerNameResults = new Dictionary<string, string>();
        
        // Initialize with fallback values
        foreach (var domain in domains)
        {
            partnerNameResults[domain] = $"Organization for {domain}";
        }
        
        if (!domains.Any())
        {
            // Return comprehensive response even with empty domains
            return new
            {
                domains = new List<string>(),
                partnerNames = partnerNameResults,
                searchResults = new List<object>(),
                summary = new
                {
                    totalDomains = 0,
                    resolvedDomains = 0,
                    unresolvedDomains = 0,
                    successRate = 0.0
                },
                searchMetadata = new
                {
                    searchDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
                    searchMethod = "AI Domain Lookup",
                    promptType = "domain_organization_lookup"
                },
                userProfile = await GetUserProfileForAIAsync(user)
            };
        }
        
        var searchResults = new List<object>();
        var resolvedCount = 0;
        
        try
        {
            // Get the prompt configuration from the AiPrompt table
            var promptConfig = promptRepository.GetAll()
                .Where(p => p.Type == "domain_organization_lookup" && p.Status == EntityStatus.Active)
                .FirstOrDefault();
            
            if (promptConfig == null)
            {
                _logger?.LogWarning("No active AiPrompt found for domain_organization_lookup");
                
                // Return comprehensive response with error info
                return new
                {
                    domains = domains,
                    partnerNames = partnerNameResults,
                    searchResults = searchResults,
                    error = "No active AI prompt configuration found for domain organization lookup",
                    summary = new
                    {
                        totalDomains = domains.Count,
                        resolvedDomains = 0,
                        unresolvedDomains = domains.Count,
                        successRate = 0.0
                    },
                    searchMetadata = new
                    {
                        searchDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
                        searchMethod = "AI Domain Lookup",
                        promptType = "domain_organization_lookup",
                        promptFound = false
                    },
                    userProfile = await GetUserProfileForAIAsync(user)
                };
            }
            
            // Create comprehensive prompt data with user context
            var promptData = new
            {
                domains = domains,
                searchContext = new
                {
                    requestedBy = user?.Identity?.Name ?? "System",
                    searchDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
                    totalDomains = domains.Count
                },
                instructions = "Identify the primary organization name for each domain. Return accurate, well-known organization names."
            };
            
            var domainsJson = System.Text.Json.JsonSerializer.Serialize(promptData);
            
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
                                var searchResult = new
                                {
                                    domain = domain,
                                    organization = organization,
                                    resolved = !string.IsNullOrEmpty(organization) && 
                                              organization != "Unknown" && 
                                              !organization.Contains("cannot") &&
                                              !organization.Contains("unable"),
                                    confidence = "AI Generated",
                                    source = "Gemini AI"
                                };
                                
                                searchResults.Add(searchResult);
                                
                                if (searchResult.resolved)
                                {
                                    partnerNameResults[domain] = organization;
                                    resolvedCount++;
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
                        partnerNameResults[domains[0]] = cleanResponse;
                        resolvedCount = 1;
                        
                        searchResults.Add(new
                        {
                            domain = domains[0],
                            organization = cleanResponse,
                            resolved = true,
                            confidence = "AI Generated (Fallback)",
                            source = "Gemini AI (Text Parse)"
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning($"Failed to get organization names from Gemini for domains: {ex.Message}");
            
            // Add error info to search results
            foreach (var domain in domains)
            {
                searchResults.Add(new
                {
                    domain = domain,
                    organization = partnerNameResults[domain],
                    resolved = false,
                    confidence = "Fallback",
                    source = "System Generated",
                    error = ex.Message
                });
            }
        }
        
        // Return comprehensive response with all context
        return new
        {
            domains = domains,
            partnerNames = partnerNameResults,
            searchResults = searchResults,
            
            // Summary statistics
            summary = new
            {
                totalDomains = domains.Count,
                resolvedDomains = resolvedCount,
                unresolvedDomains = domains.Count - resolvedCount,
                successRate = domains.Count > 0 ? (double)resolvedCount / domains.Count : 0.0,
                fallbacksUsed = domains.Count - resolvedCount
            },
            
            // Search metadata
            searchMetadata = new
            {
                searchDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm"),
                searchMethod = "AI Domain Lookup",
                promptType = "domain_organization_lookup",
                promptFound = true,
                aiModel = "Gemini",
                processingTime = "Real-time"
            },
            
            // Domain analysis
            domainAnalysis = domains.Select(d => new
            {
                domain = d,
                tld = d.Contains('.') ? d.Substring(d.LastIndexOf('.')) : "unknown",
                length = d.Length,
                hasSubdomain = d.Count(c => c == '.') > 1,
                resolvedName = partnerNameResults.ContainsKey(d) ? partnerNameResults[d] : null
            }).Cast<dynamic>().ToList(),
            
            // User profile information for context
            userProfile = await GetUserProfileForAIAsync(user)
        };
    }

    /// <summary>
    /// Gets multiple contacts by their IDs for search results
    /// </summary>
    public override async Task<List<object>> GetByIdsAsync(int[] ids, ClaimsPrincipal user = null)
    {
        if (ids == null || ids.Length == 0)
            return new List<object>();

        var contacts = contactRepository
            .GetAll(["Partner", "Partner.PartnerGroup"])
            .Where(c => ids.Contains(c.Id))
            .ToList();

        // Load OrganizationUnitRelationships manually for all contacts
        /*foreach (var contact in contacts.Where(c => c.Partner != null))
        {
            contact.Partner.LoadOrganizationUnitRelationshipsAsync(_context).Wait();
        }*/

        // Apply access control if user context is provided
        if (user != null)
        {
            var filteredData = await ApplyAccessControlFilters(contacts.AsQueryable(), user, "read");
            if (filteredData is IEnumerable<UNOPSContact> contactList)
            {
                contacts = contactList.ToList();
            }
        }

        // Get all unique user IDs for efficient mapping
        var userIds = contacts.Where(c => c.CreatedBy > 0).Select(c => c.CreatedBy).Distinct().ToList();
        
        var userInfoLookup = userInfoRepository.GetAll()
            .Where(u => userIds.Contains(u.UserId))
            .ToDictionary(u => u.UserId);
        
        var orgUnits = userInfoLookup.Values
            .Where(u => !string.IsNullOrEmpty(u.OrgUnit))
            .Select(u => u.OrgUnit)
            .Distinct()
            .ToList();
        
        var orgHierarchyLookup = organizationHierarchyRepository.GetAll()
            .Where(o => !string.IsNullOrEmpty(o.Code) && 
                       o.Type == OrganizationUnitType.OrgUnit && 
                       orgUnits.Contains(o.Code))
            .ToDictionary(o => o.Code);

        return contacts.Select(contact => (object)MapEntityToModelWithUserInfo(contact, mapper, userInfoLookup, orgHierarchyLookup)).ToList();
    }
    
    /// <summary>
    /// Applies org unit filtering if the specification supports it using manual joins for Contact specifications
    /// </summary>
    private IQueryable<UNOPSContact> ApplyOrgUnitFilterIfSupported(IQueryable<UNOPSContact> query, ISpecification<Contact> specification)
    {
        // Check if specification has ApplyOrgUnitFilter method and call it
        var specType = specification.GetType();
        var filterMethod = specType.GetMethod("ApplyOrgUnitFilter", new[] { typeof(IQueryable<Contact>), typeof(DbContext) });
        
        if (filterMethod != null)
        {
            try
            {
                // Cast to base type for the ApplyOrgUnitFilter method
                var baseQuery = query.Cast<Contact>();
                var result = filterMethod.Invoke(specification, new object[] { baseQuery, _context });
                if (result is IQueryable<Contact> filteredBaseQuery)
                {
                    // Cast back to UNOPSContact using OfType for safety
                    return filteredBaseQuery.OfType<UNOPSContact>();
                }
            }
            catch (Exception ex)
            {
                // Log error but continue without org unit filtering
                Console.WriteLine($"Error applying org unit filter: {ex.Message}");
            }
        }
        
        // If no ApplyOrgUnitFilter method found, return original query
        // This is normal for composite specifications that handle filtering internally
        return query;
    }
    
    /// <summary>
    /// Applies org unit filtering if the specification supports it using manual joins for UNOPSContact specifications
    /// </summary>
    private IQueryable<UNOPSContact> ApplyUNOPSContactOrgUnitFilterIfSupported(IQueryable<UNOPSContact> query, ISpecification<UNOPSContact> specification)
    {
        // Check if specification has ApplyOrgUnitFilter method and call it
        var specType = specification.GetType();
        var filterMethod = specType.GetMethod("ApplyOrgUnitFilter", new[] { typeof(IQueryable<UNOPSContact>), typeof(DbContext) });
        
        if (filterMethod != null)
        {
            try
            {
                // Direct call for UNOPSContact specifications
                var result = filterMethod.Invoke(specification, new object[] { query, _context });
                if (result is IQueryable<UNOPSContact> filteredQuery)
                {
                    return filteredQuery;
                }
            }
            catch (Exception ex)
            {
                // Log error but continue without org unit filtering
                Console.WriteLine($"Error applying org unit filter: {ex.Message}");
            }
        }
        
        // If no ApplyOrgUnitFilter method found, return original query
        // This is normal for specifications that don't support org unit filtering
        return query;
    }
    
    /// <summary>
    /// Get supported search fields for contacts - helps frontend build dynamic search forms
    /// </summary>
    /// <returns>List of all supported search fields with their metadata</returns>
    public List<SearchFieldInfo> GetContactSearchFields()
    {
        try
        {
            var fields = new List<SearchFieldInfo>
            {
                // Direct Contact fields - using translation keys
                new() { Field = "fullName", DisplayName = "label.contact.fullName", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "firstName", DisplayName = "label.contact.firstName", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "lastName", DisplayName = "label.contact.lastName", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "email", DisplayName = "label.contact.email", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "title", DisplayName = "label.contact.title", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "department", DisplayName = "label.contact.department", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "phone", DisplayName = "label.contact.phone", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "mobile", DisplayName = "label.contact.mobile", FieldType = "text", AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { 
                    Field = "status", 
                    DisplayName = "label.common.status", 
                    FieldType = "enum", 
                    AllowedOperators = new List<string> { "entityCards.operators.eq", "entityCards.operators.neq" },
                    DropdownOptions = new List<DropdownOption>
                    {
                        new() { Value = "Inactive", Label = "enums.entityStatus.inactive" },
                        new() { Value = "Active", Label = "enums.entityStatus.active" },
                        new() { Value = "Closed", Label = "enums.entityStatus.closed" },
                        new() { Value = "Draft", Label = "enums.entityStatus.draft" },
                        new() { Value = "Archived", Label = "enums.entityStatus.archived" }
                    }
                },
                new() { Field = "createdDate", DisplayName = "label.common.createdDate", FieldType = "date", AllowedOperators = new List<string> { "entityCards.operators.on", "entityCards.operators.after", "entityCards.operators.before", "entityCards.operators.between" } },

                // Partner relationship fields - using translation keys
                new() { Field = "partner.name", DisplayName = "label.partner.name", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "partner.partnerGroup.name", DisplayName = "label.partnerGroup.name", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
                new() { Field = "partner.liaisonOffice.name", DisplayName = "label.liaisonOffice.name", FieldType = "text", IsNavigationProperty = true, AllowedOperators = new List<string> { "entityCards.operators.like", "entityCards.operators.eq", "entityCards.operators.neq" } },
            };
            
            return fields;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error retrieving contact search fields");
            return new List<SearchFieldInfo>();
        }
    }
}