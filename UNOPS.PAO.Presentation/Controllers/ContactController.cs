using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Net;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Models;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Specifications;
using Microsoft.Extensions.DependencyInjection;

namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Presentation.Security;
using System.Text.Json.Nodes;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Presentation;


[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class ContactController : BaseController
{
    private readonly IContactManager _manager;
    private readonly IOrgUnitFilterService _orgUnitFilterService;

    public ContactController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        ILogger<ContactController> logger,
        IAuthorizationService authorizationService,
        IOrgUnitFilterService orgUnitFilterService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.ContactManager;
        _orgUnitFilterService = orgUnitFilterService;
    }

    /// <summary>
    /// Creates a new contact with comprehensive personal and professional details.
    /// </summary>
    /// <param name="req">Contact creation request with required fields</param>
    /// <param name="req.firstName">Contact's first name (required)</param>
    /// <param name="req.lastName">Contact's last name (required)</param>
    /// <param name="req.email">Primary email address (required)</param>
    /// <param name="req.partnerId">Associated partner organization ID (required)</param>
    /// <param name="req.title">Job title/position (required)</param>
    /// <param name="req.salutation">Title/salutation (Mr., Ms., Dr., etc.)</param>
    /// <param name="req.middleName">Middle name or initial</param>
    /// <param name="req.suffix">Name suffix (Jr., Sr., III, etc.)</param>
    /// <param name="req.department">Department or division</param>
    /// <param name="req.phone">Primary phone number</param>
    /// <param name="req.mobile">Mobile phone number</param>
    /// <param name="req.status">Contact status (defaults to 'Active')</param>
    /// <param name="req.mailingStreet">Mailing address street</param>
    /// <param name="req.mailingCity">Mailing address city</param>
    /// <param name="req.mailingCountry">Mailing address country</param>
    /// <example_uses>
    /// Create a contact named John Doe with email john@unicef.org
    /// Add a new program manager contact for partner 123
    /// Register Dr. Jane Smith as the technical lead
    /// Create contact with full address information
    /// Add executive contact to organization
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to add, create, register, or set up a new contact.</when_to_use>
    /// <returns>Created contact with ID and metadata</returns>
    [HttpPost(APIDictionary.Contact)]
    [AccessControlled(EntityTypes.Contact, "create")]
    public async Task<ActionResult> Create([FromBody] ContactRequest req)
    {
        var result = await _manager.CreateContactAsync(req);
        if (result == null)
        {
            throw new BusinessException("Failed to create contact");
        }
        return StatusCode(201, result);
    }

    /// <summary>
    /// Retrieves a list of contacts with advanced filtering, pagination, search capabilities, and access control.
    /// </summary>
    /// <param name="request">Contact filter request containing search and pagination parameters</param>
    /// <param name="request.pageIndex">Page number (1-based)</param>
    /// <param name="request.pageSize">Number of items per page</param>
    /// <param name="request.searchText">Text to search across contact fields</param>
    /// <param name="request.orderBy">Field to order results by</param>
    /// <param name="request.ascending">Sort direction (true for ascending)</param>
    /// <param name="request.orgUnitId">Filter by organizational unit ID for access control</param>
    /// <param name="advancedSearch">Enable advanced search mode for complex entity relationship searches</param>
    /// <param name="searchCriteria">JSON string containing advanced search filters with nested entity criteria. Required when advancedSearch=true</param>
    /// <param name="searchText">Text to search across contact fields (override for request.searchText)</param>
    /// <example_uses>
    /// Show me all contacts
    /// List contacts with email domain @unicef.org
    /// Find contacts working at government partners (use advancedSearch=true, searchCriteria with partner.name)
    /// Search for contacts named John
    /// Show contacts from my office only
    /// Get contacts sorted by last name
    /// Find technical experts in partner organizations (use advancedSearch=true)
    /// Find contacts from specific interactions (use advancedSearch=true, searchCriteria with interaction.subject)
    /// Get contacts from UNICEF partner organization (advancedSearch=true)
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to search, list, filter, or browse contacts. Use advancedSearch=true for relationship-based searches involving partners or interactions.</when_to_use>
    /// <advanced_search_guidance>
    /// **CRITICAL: When to use advancedSearch=true:**
    /// - User searches for contacts BY PARTNER: "contacts from UNICEF", "contacts working at partner X"
    /// - User searches for contacts BY ORGANIZATION: "contacts from organization Y"
    /// - User searches for contacts BY INTERACTION: "contacts from meeting Z", "contacts involved in interaction"
    /// - Any search involving related entities (partners, interactions, organizations)
    /// 
            /// **searchCriteria JSON format with operators (CRITICAL - Must use this exact format):**
        /// searchCriteria must be a JSON array of SearchCriteria objects with field, operator, value, and logicalOperator
        /// 
        /// **Available Operators:**
        /// - "is" (exact match), "is not" (not equal), "like" (contains), "not like" (does not contain)
        /// - ">", "<", ">=", "<=" (comparisons), "after", "before", "between" (dates)
        /// 
        /// **Logical Operators:** "AND", "OR"
        /// 
        /// **Examples:**
        /// - Find contacts from partner "Asian Infrastructure": 
        ///   searchCriteria=[{"field": "partner.name", "operator": "like", "value": "Asian Infrastructure"}]
        /// - Find contacts from UNICEF organization: 
        ///   searchCriteria=[{"field": "partner.name", "operator": "like", "value": "UNICEF"}]
        /// - Find contacts involved in climate meetings: 
        ///   searchCriteria=[{"field": "interaction.subject", "operator": "like", "value": "climate", "logicalOperator": "AND"}, {"field": "interaction.type", "operator": "is", "value": "Meeting"}]
        /// - Find contacts named John from WHO: 
        ///   searchCriteria=[{"field": "firstName", "operator": "like", "value": "John", "logicalOperator": "AND"}, {"field": "partner.name", "operator": "like", "value": "WHO"}]
    /// 
    /// **Simple search (advancedSearch=false) for:**
    /// - Name/email text search: "contacts named Smith"
    /// - Basic filtering: "contacts with gmail", "active contacts"
    /// - Role/title searches: "technical contacts"
    /// </advanced_search_guidance>
    /// <returns>Paginated list of contacts with metadata</returns>
    [HttpGet(APIDictionary.Contact)]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> Get(
        [FromQuery] ContactFilterRequest request,
        [FromQuery] bool advancedSearch = false, 
        [FromQuery] string? searchCriteria = null, 
        [FromQuery] string? searchText = null)
    {
        
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
        if (validationResult != null) return validationResult;
        
        // Handle different search scenarios using the new helper methods
        return await HandleSearchOperationAsync(async () =>
        {
            if (advancedSearch && !string.IsNullOrEmpty(searchCriteria))
            {
                // For advanced search, we need to parse the search criteria and combine with org unit filter
                return await SearchControllerHelper.ProcessAdvancedSearch<ContactFilterRequest, ContactCompositeSpecification, PaginationResponse<ContactModel>>(
                    searchCriteria, searchText ?? request.SearchText, request.PageIndex, request.PageSize, request.OrderBy, request.Ascending, 
                    request,
                    "Contact",
                    filterRequest => new ContactCompositeSpecification(filterRequest),
                    async (userId, spec, pagination) => {
                        // If OrgUnitId is specified, use the OrgUnitFilterService to create a proper specification
                        if (pagination is ContactFilterRequest contactPagination && contactPagination.OrgUnitId.HasValue)
                        {
                            var orgUnitSpec = await _orgUnitFilterService.CreateContactSpecificationAsync(contactPagination, User);
                            var adaptedSpec = new ContactSpecificationAdapter(orgUnitSpec);
                            return (PaginationResponse<ContactModel>)await _manager.GetContactsWithSpecificationAsync(User, adaptedSpec, contactPagination);
                        }
                        return (PaginationResponse<ContactModel>)await _manager.GetContactsWithSpecificationAsync(User, spec, (ContactFilterRequest)pagination);
                    },
                    CurrentUserId, _logger);
            }

            // Handle simple text search
            if (!string.IsNullOrWhiteSpace(searchText) || !string.IsNullOrWhiteSpace(request.SearchText))
            {
                var textToSearch = searchText ?? request.SearchText;
                return await SearchControllerHelper.ProcessSimpleTextSearch<ContactFilterRequest, ContactCompositeSpecification, PaginationResponse<ContactModel>>(
                    textToSearch!, request.PageIndex, request.PageSize, request.OrderBy, request.Ascending,
                    request,
                    "Contact",
                    filterRequest => new ContactCompositeSpecification(filterRequest),
                    async (userId, spec, pagination) => {
                        // If OrgUnitId is specified, use the OrgUnitFilterService to create a proper specification
                        if (pagination is ContactFilterRequest contactPagination && contactPagination.OrgUnitId.HasValue)
                        {
                            var orgUnitSpec = await _orgUnitFilterService.CreateContactSpecificationAsync(contactPagination, User);
                            var adaptedSpec = new ContactSpecificationAdapter(orgUnitSpec);
                            return (PaginationResponse<ContactModel>)await _manager.GetContactsWithSpecificationAsync(User, adaptedSpec, contactPagination);
                        }
                        return (PaginationResponse<ContactModel>)await _manager.GetContactsWithSpecificationAsync(User, spec, (ContactFilterRequest)pagination);
                    },
                    CurrentUserId, _logger);
            }

            // Use OrgUnitFilterService to create the appropriate specification
            var unosContactSpec = await _orgUnitFilterService.CreateContactSpecificationAsync(request, User);
            var specification = new ContactSpecificationAdapter(unosContactSpec);
            
            var result = await _manager.GetContactsWithSpecificationAsync(User, specification, request);
            return (PaginationResponse<ContactModel>)result;
        }, "contact search");
    }

    /// <summary>
    /// Retrieves a specific contact by ID with complete details including partner information and all contact methods.
    /// </summary>
    /// <param name="id">Contact ID</param>
    /// <example_uses>
    /// Show me details for contact ID 123
    /// Get full information about contact 456
    /// Display contact record 789
    /// Get complete contact profile
    /// Show contact with partner information
    /// </example_uses>
    /// <when_to_use>Use this when the user asks for specific contact details by ID or when you need complete contact information.</when_to_use>
    /// <returns>Complete contact details with related information</returns>
    [HttpGet(APIDictionary.Contact + "/{id}")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> Get(int id)
    {
        // Use the new secure method that checks entity-level access
        var contact = await _manager.GetContactAsync(User, id);
        if (contact == null)
        {
            return NotFound();
        }
        return Ok(contact);
    }

    /// <summary>
    /// Updates an existing contact's information including personal details, contact methods, and professional information.
    /// </summary>
    /// <param name="req">Contact update request containing modified fields</param>
    /// <param name="req.id">Contact ID to update (required)</param>
    /// <param name="req.firstName">Updated first name</param>
    /// <param name="req.lastName">Updated last name</param>
    /// <param name="req.email">Updated email address</param>
    /// <param name="req.title">Updated job title</param>
    /// <param name="req.phone">Updated phone number</param>
    /// <param name="req.mobile">Updated mobile number</param>
    /// <param name="req.department">Updated department</param>
    /// <param name="req.status">Updated status</param>
    /// <example_uses>
    /// Update contact 123's email to newemail@unicef.org
    /// Change contact 456's title to Senior Manager
    /// Update phone number for contact 789
    /// Modify contact's department information
    /// Change contact status to Inactive
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to update, modify, edit, or change contact information.</when_to_use>
    /// <returns>Updated contact data</returns>
    [HttpPut(APIDictionary.Contact)]
    [AccessControlled(EntityTypes.Contact, "update")]
    public async Task<ActionResult> Update([FromBody] UpdateContactRequest req)
    {
        // Use the new secure method that checks entity-level permissions
        var result = await _manager.UpdateContactAsync(User, req);
        return Ok(result);
    }

    /// <summary>
    /// Soft deletes a contact from the system (marks as deleted rather than permanent removal).
    /// </summary>
    /// <param name="id">Contact ID to delete</param>
    /// <example_uses>
    /// Delete contact ID 123
    /// Remove contact 456 from the system
    /// Deactivate contact record
    /// Soft delete contact John Doe
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to delete, remove, or eliminate a contact.</when_to_use>
    /// <returns>No content on successful deletion</returns>
    [HttpDelete(APIDictionary.Contact + "/{id}")]
    [AccessControlled(EntityTypes.Contact, "delete")]
    public async Task<ActionResult> Delete(int id)
    {
        // Use the new secure method that checks entity-level permissions
        await _manager.DeleteContactAsync(User, id);
        return NoContent();
    }


    /// <summary>
    /// Retrieves all contacts associated with a specific partner organization with access control.
    /// </summary>
    /// <param name="partnerId">Partner organization ID to get contacts for</param>
    /// <example_uses>
    /// Show all contacts for partner organization 123
    /// Get contact list for UNICEF partner
    /// Find all people working at partner ID 456
    /// List contacts belonging to a specific organization
    /// Get partner's contact directory
    /// </example_uses>
    /// <when_to_use>Use this when the user asks to see contacts for a specific partner, organization, or wants to view who works at a particular partner.</when_to_use>
    /// <returns>List of contacts belonging to the specified partner organization</returns>
    [HttpGet(APIDictionary.PartnerContacts)]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> PartnerContacts(int partnerId)
    {
        return Ok(_manager.GetPartnerContacts(partnerId));
    }

    /// <summary>
    /// Retrieves the current user's permissions for a specific contact (read, update, delete).
    /// </summary>
    /// <param name="id">Contact ID to check permissions for</param>
    /// <example_uses>
    /// Check my permissions for contact 123
    /// What can I do with contact 456?
    /// Get access rights for this contact
    /// Verify contact permissions before editing
    /// Can I update this contact's information?
    /// </example_uses>
    /// <when_to_use>Use this when you need to check user permissions before performing operations or showing UI elements for contact management.</when_to_use>
    /// <returns>Permission object with CanRead, CanUpdate, CanDelete flags</returns>
    [HttpGet(APIDictionary.Contact + "/{id}/permissions")]
    public async Task<ActionResult> PermissionsGet(int id)
    {
        var contact = await _manager.GetContact(CurrentUserId, id);
        if (contact == null)
        {
            return NotFound(new { error = $"Contact with ID {id} not found" });
        }
        
        // Return permissions for this contact
        var permissions = await GetEntityPermissionsAsync("Contact", contact);
        
        return Ok(permissions);
    }

    /// <summary>
    /// Uploads and associates a profile picture with a contact (max 1MB, JPEG/PNG/WEBP only).
    /// </summary>
    /// <param name="id">Contact ID to upload profile picture for</param>
    /// <param name="file">Image file (max 1MB, JPEG/PNG/WEBP formats only)</param>
    /// <example_uses>
    /// Upload a profile picture for contact 123
    /// Add photo to contact John Doe
    /// Set contact profile image
    /// Update contact's profile picture
    /// Add headshot to contact record
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to add or update a contact's profile picture or photo.</when_to_use>
    /// <returns>Success confirmation with image URL or error details</returns>
    [HttpPost(APIDictionary.Contact + "/{id}/profile-picture")]
    [AccessControlled(EntityTypes.Contact, "update")]
    public async Task<ActionResult> UploadProfilePicture(int id, IFormFile file)
    {
        
        if (file == null || file.Length == 0)
        {
            throw new BusinessException("No file was uploaded");
        }

        // Check file size (1MB max)
        if (file.Length > 1024 * 1024)
        {
            throw new BusinessException("File size exceeds maximum limit of 1MB");
        }

        // Validate file type
        var validImageTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!validImageTypes.Contains(file.ContentType))
        {
            throw new BusinessException("Invalid file type. Only JPEG, PNG, and WEBP files are allowed.");
        }

        var result = await _manager.UpdateContactProfilePictureAsync(id, file);
        return Ok(new { imageUrl = result });
    }
}
