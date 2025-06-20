using Microsoft.AspNetCore.Http;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.RBAC.Attributes;

namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Specifications;
using System.Security.Claims;

public interface IPartnerManager
{
    Task<PartnerModel> CreatePartnerAsync(PartnerRequest model);

    Task<PaginationResponse<PartnerModel>> GetPartners(int userId, PaginationRequest request);
    
    Task<PaginationResponse<PartnerModel>> GetPartnersWithSpecification(int userId, ISpecification<Partner> specification, PaginationRequest pagination);

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
    Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroup(int userId, string partnerTreeId, PaginationRequest request);
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
    
    #region Secure Methods for Permission-based Access
    
    /// <summary>
    /// Gets partners with row-level security applied based on user permissions
    /// </summary>
    [RBAC("read", Entity = "Partner", ApplyRowFiltering = true, ApplyColumnFiltering = true)]
    Task<PaginationResponse<PartnerModel>> GetPartnersAsync(ClaimsPrincipal user, PaginationRequest request);
    
    /// <summary>
    /// Gets a specific partner with row-level security applied
    /// </summary>
    [RBAC("read", Entity = "Partner", RequireEntityAccess = true, EntityIdParameterName = "id", ApplyColumnFiltering = true)]
    Task<PartnerModel?> GetPartnerAsync(ClaimsPrincipal user, int id);
    
    /// <summary>
    /// Creates a new partner with permission validation
    /// </summary>
    [RBAC("create", Entity = "Partner")]
    Task<PartnerModel?> CreatePartnerAsync(ClaimsPrincipal user, PartnerRequest model);
    
    /// <summary>
    /// Updates a partner with permission validation
    /// </summary>
    [RBAC("update", Entity = "Partner", RequireEntityAccess = true, EntityIdParameterName = "model.Id")]
    Task<PartnerModel?> UpdatePartnerAsync(ClaimsPrincipal user, UpdatePartnerRequest model);
    
    /// <summary>
    /// Deletes a partner with permission validation
    /// </summary>
    [RBAC("delete", Entity = "Partner", RequireEntityAccess = true, EntityIdParameterName = "id")]
    Task<bool> DeletePartnerAsync(ClaimsPrincipal user, int id);
    
    /// <summary>
    /// Gets partners by partner group with security applied
    /// </summary>
    [RBAC("read", Entity = "Partner", ApplyRowFiltering = true, ApplyColumnFiltering = true)]
    Task<PaginationResponse<PartnerModel>> GetPartnersByPartnerGroupAsync(ClaimsPrincipal user, string partnerGroupCode, PaginationRequest request);
    
    /// <summary>
    /// Gets partners by partner category with security applied
    /// </summary>
    [RBAC("read", Entity = "Partner", ApplyRowFiltering = true, ApplyColumnFiltering = true)]
    Task<PaginationResponse<PartnerModel>> GetPartnersByCategoryAsync(ClaimsPrincipal user, string partnerCategoryCode, PaginationRequest request);
    
    #endregion
}