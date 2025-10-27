using Microsoft.AspNetCore.Http;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using System.Security.Claims;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Models.Integrations;

public interface IPartnerManager
{
    Task<PartnerModel> CreatePartnerAsync(PartnerRequest model);

    Task<PaginationResponse<PartnerModel>> GetPartners(int userId, PaginationRequest request);
    
    Task<PaginationResponse<PartnerModel>> GetPartnersWithSpecification(int userId, ISpecification<Partner> specification, PaginationRequest pagination);
    Task<object> GetPartnersWithSpecificationAsync(ClaimsPrincipal user, ISpecification<Partner> specification, PaginationRequest pagination);

    Task<PartnerModel?> GetPartner(int userId, int id);

    //IEnumerable<ExternalPartnerModel> GetPostedPartners();

    //Task<ExternalPartnerModel?> GetPostedPartner(int id);

    Task<PartnerModel?> UpdatePartnerAsync(int userId, UpdatePartnerRequest model);

    Task DeletePartnerAsync(int userId, int id);
    Task<PartnerModel?> GetPartnerAsync(int id);
    /// <summary>
    /// Gets a partner with its contacts and their interactions included
    /// </summary>
    Task<PartnerModel?> GetPartnerWithContactsAndInteractionsAsync(int id);
    Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroup(int userId, int partnerTreeId, PaginationRequest request);
    Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerCategory(int userId, string partnerCategoryCode, PaginationRequest request);
    Task<string?> UpdatePartnerLogoAsync(int partnerId, IFormFile file);
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the partner
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="partnerId">ID of the partner</param>
    /// <param name="operation">Operation to check (e.g., "Read", "Update", "Delete")</param>
    /// <returns>True if the user has permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(int userId, int partnerId, string operation);
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the partner
    /// </summary>
    /// <param name="user">ClaimsPrincipal of the user</param>
    /// <param name="partnerId">ID of the partner</param>
    /// <param name="operation">Operation to check (e.g., "Read", "Update", "Delete")</param>
    /// <returns>True if the user has permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, int partnerId, string operation);
    
    /// <summary>
    /// Checks if the user has permission to perform the specified operation on the partner
    /// </summary>
    /// <param name="user">ClaimsPrincipal of the user</param>
    /// <param name="partner">Partner entity</param>
    /// <param name="operation">Operation to check (e.g., "Read", "Update", "Delete")</param>
    /// <returns>True if the user has permission, false otherwise</returns>
    Task<bool> HasPermissionAsync(ClaimsPrincipal user, Partner partner, string operation);
    Task<PaginationResponse<PartnerModel>> GetPartnersAsync(ClaimsPrincipal user, PaginationRequest request);
    
    Task<PartnerModel?> GetPartnerAsync(ClaimsPrincipal user, int id);
    
    Task<PartnerModel?> CreatePartnerAsync(ClaimsPrincipal user, PartnerRequest model);

    Task<PartnerModel?> UpdatePartnerAsync(ClaimsPrincipal user, UpdatePartnerRequest model);

    Task<bool> DeletePartnerAsync(ClaimsPrincipal user, int id);

    Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroupAsync(ClaimsPrincipal user, int partnerGroupId, PaginationRequest request);
    Task<PaginationResponse<PartnerModel>> GetPartnersByCategoryAsync(ClaimsPrincipal user, string partnerCategoryCode, PaginationRequest request);

    Task<List<PartnerModel?>> GetPartnersForGmailAddon(GmailRelatedRecordsRequest input, ClaimsPrincipal user);
    
    /// <summary>
    /// Gets a partner by name
    /// </summary>
    /// <param name="user">The current user's claims principal</param>
    /// <param name="name">The partner name to search for (case-insensitive)</param>
    /// <returns>The partner model if found, null otherwise</returns>
    Task<PartnerModel?> GetPartnerByNameAsync(ClaimsPrincipal user, string name);
    
    // Partner Status Management Methods
    Task<PartnerModel?> ActivatePartnerAsync(ClaimsPrincipal user, int id, ActivatePartnerRequest request);
    Task<PartnerModel?> ClosePartnerAsync(ClaimsPrincipal user, int id, StatusChangeRequest request);
    Task<PartnerModel?> ArchivePartnerAsync(ClaimsPrincipal user, int id, StatusChangeRequest request);
    Task<PartnerModel?> ApprovePartnerAsync(ClaimsPrincipal user, int id, UpdatePartnerRequest request);
    Task<PartnerModel?> UnapprovePartnerAsync(ClaimsPrincipal user, int id, StatusChangeRequest request);
    
    
    /// <summary>
    /// Performs comprehensive smart search across Partners and all related entities.
    /// Searches through partner information, contacts, partner groups, liaison offices, 
    /// organization units, and applies intelligent ranking based on relevance.
    /// </summary>
    /// <param name="user">The user performing the search (for RBAC)</param>
    /// <param name="searchText">Text to search across all partner and related entity fields</param>
    /// <param name="includeInactive">Whether to include inactive/deleted partners (default: false)</param>
    /// <param name="maxResults">Maximum number of results to return (default: 50)</param>
    /// <param name="request">Pagination request for final result formatting</param>
    /// <returns>Paginated response with ranked search results and metadata</returns>
    Task<PaginationResponse<PartnerModel>> PerformSmartSearchAsync(
        ClaimsPrincipal user,
        string searchText,
        bool includeInactive = false,
        int maxResults = 50,
        PaginationRequest? request = null);

    Task<int> GetTotalPartnerCountAsync(ClaimsPrincipal user);
    Task<List<string>> GetSamplePartnerNamesAsync(ClaimsPrincipal user, int count = 5);

    // GetPartnerSearchFields removed - now handled directly in PartnerController for dynamic translation support
}