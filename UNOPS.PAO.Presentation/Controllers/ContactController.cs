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
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Managers;

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
    private readonly IGeminiManager _geminiManager;
    private readonly IUNOPSEntityConfigurationManager _entityConfigurationManager;

    public ContactController(
        IManagerWrapper manager, 
        UserResolverService<int> userResolverService, 
        ILogger<ContactController> logger,
        IAuthorizationService authorizationService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.ContactManager;
        _geminiManager = manager.GeminiManager;
        _entityConfigurationManager = ((UNOPSManagerWrapper)manager).EntityConfigurationManager;
    }

    /// <summary>
    /// Creates a new contact with comprehensive personal and professional details.
    /// </summary>
    /// <param name="req">Contact creation request with required fields</param>
    /// <param name="req.firstName">Contact's first name (optional)</param>
    /// <param name="req.lastName">Contact's last name (required) - validation enforced</param>
    /// <param name="req.email">Primary email address (required) - validation enforced for format and presence</param>
    /// <param name="req.partnerId">Associated partner organization ID (required) - must be valid existing partner</param>
    /// <param name="req.title">Job title/position (required) - validation enforced</param>
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
        // Validate mandatory fields for contact creation
        var validationErrors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(req.LastName))
        {
            validationErrors.Add("LastName is required for contact creation");
        }
        
        if (string.IsNullOrWhiteSpace(req.Title))
        {
            validationErrors.Add("Title is required for contact creation");
        }
        
        if (string.IsNullOrWhiteSpace(req.Email))
        {
            validationErrors.Add("Email is required for contact creation");
        }
        else if (!IsValidEmail(req.Email))
        {
            validationErrors.Add("Email format is invalid");
        }
        
        if (req.PartnerId <= 0)
        {
            validationErrors.Add("PartnerId is required and must be a valid partner ID");
        }
        
        // Return validation errors if any
        if (validationErrors.Any())
        {
            var errorMessage = $"Missing required fields for contact creation: {string.Join(", ", validationErrors)}";
            return BadRequest(new { 
                error = errorMessage,
                missingFields = validationErrors
            });
        }
        
        var result = await _manager.CreateContactAsync(req);
        if (result == null)
        {
            throw new BusinessException("Failed to create contact");
        }
        return StatusCode(201, result);
    }

    /// <summary>
    /// Retrieves all contacts with basic pagination and ordering (no search criteria).
    /// </summary>
    /// <param name="pageIndex">Page number (1-based, default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20)</param>
    /// <param name="orderBy">Field to order results by (optional)</param>
    /// <param name="ascending">Sort direction - true for ascending, false for descending (default: true)</param>
    /// <example_uses>
    /// Show me all contacts
    /// List all contacts in the system
    /// Display the contact directory
    /// Get all contact records
    /// Browse contacts
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to see ALL contacts without any search criteria or when asking for a general contact list.</when_to_use>
    /// <returns>Paginated list of all contacts</returns>
    [HttpGet(APIDictionary.Contact)]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> ListAllContacts(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? orderBy = null,
        [FromQuery] bool ascending = true)
    {
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(pageIndex, pageSize);
        if (validationResult != null) return validationResult;
        
        return await HandleSearchOperationAsync(async () =>
        {
            // Create a basic ContactFilterRequest with just pagination and ordering
            var request = new ContactFilterRequest
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                OrderBy = orderBy,
                Ascending = ascending
            };
            
            // Create simple specification - global filters will be applied by the manager
            var specification = new ContactCompositeSpecification(request);
            
            var result = await _manager.GetContactsWithSpecificationAsync(User, specification, request);
            return (PaginationResponse<ContactModel>)result;
        }, "contact list all");
    }

    /// <summary>
    /// Performs simple text search across multiple contact fields (name, email, title, etc.).
    /// </summary>
    /// <param name="request">Pagination request containing only pagination and sorting parameters</param>
    /// <param name="searchText">Text to search across contact name, email, title, and other basic fields</param>
    /// <example_uses>
    /// Search for contacts named John
    /// Find contacts with @unicef.org email
    /// Search for contacts containing 'Smith'
    /// Find contact with phone number 555-1234
    /// Look for contacts with title 'Manager'
    /// </example_uses>
    /// <when_to_use>Use this for simple name, email, title, or basic field searches. NOT for partner relationship searches.</when_to_use>
    /// <returns>Paginated list of contacts matching the search text</returns>
    [HttpGet(APIDictionary.Contact + "/search")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> SearchContacts(
        [FromQuery] PaginationRequest request,
        [FromQuery] string searchText)
    {
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
        if (validationResult != null) return validationResult;
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            throw new BusinessException("Search text is required for contact search");
        }

        return await HandleSearchOperationAsync(async () =>
        {
            // Create a ContactFilterRequest with pagination/sorting info and search text
            var contactFilterRequest = new ContactFilterRequest
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                OrderBy = request.OrderBy,
                Ascending = request.Ascending,
                SearchText = searchText
            };

            return await SearchControllerHelper.ProcessSimpleTextSearch<ContactFilterRequest, ContactCompositeSpecification, PaginationResponse<ContactModel>>(
                searchText, request.PageIndex, request.PageSize, request.OrderBy, request.Ascending,
                contactFilterRequest,
                "Contact",
                filterRequest => new ContactCompositeSpecification(filterRequest),
                async (userId, spec, pagination) => {
                    // Use regular specification - global filters handled automatically by BaseRepository
                    return (PaginationResponse<ContactModel>)await _manager.GetContactsWithSpecificationAsync(User, spec, (ContactFilterRequest)pagination);
                },
                CurrentUserId, _logger);
        }, "contact simple search");
    }

    /// <summary>
    /// Performs advanced search with structured criteria including relationships with partners, departments, and complex filters.
    /// </summary>
    /// <param name="request">Pagination request containing only pagination and sorting parameters</param>
    /// <param name="searchCriteria">JSON array of search criteria objects with field, operator, value, and logicalOperator</param>
    /// <param name="searchText">Optional additional text search to combine with criteria</param>
    /// <example_uses>
    /// Find contacts from UNICEF partner organization
    /// Show contacts in Finance department created this month
    /// Get contacts working at Asian Infrastructure partners
    /// Find contacts where partner status is Active
    /// List contacts from climate-related interactions
    /// Search for contacts by department and creation date
    /// </example_uses>
    /// <when_to_use>Use this for searches involving partner relationships, departments, dates, status, or any complex multi-field criteria.</when_to_use>
    /// <searchCriteria_format>
    /// JSON array format: [{"field": "partner.name", "operator": "like", "value": "UNICEF", "logicalOperator": "AND"}]
    /// Available operators: is, is not, like, not like, greater than, less than, greater than or equal, less than or equal, this week, this month, this year
    /// Available fields: firstName, lastName, email, title, department, partner.name, partner.status, createdDate, modifiedDate
    /// Logical operators: AND, OR
    /// </searchCriteria_format>
    /// <returns>Paginated list of contacts matching the advanced search criteria</returns>
    [HttpGet(APIDictionary.Contact + "/advanced-search")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> AdvancedSearchContacts(
        [FromQuery] PaginationRequest request,
        [FromQuery] string searchCriteria,
        [FromQuery] string? searchText = null)
    {
        // Validate pagination parameters
        var validationResult = ValidatePaginationParameters(request.PageIndex, request.PageSize);
        if (validationResult != null) return validationResult;
        
        if (string.IsNullOrWhiteSpace(searchCriteria))
        {
            throw new BusinessException("Search criteria is required for advanced contact search");
        }

        return await HandleSearchOperationAsync(async () =>
        {
            // Create a ContactFilterRequest with pagination/sorting info and search criteria
            var contactFilterRequest = new ContactFilterRequest
            {
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                OrderBy = request.OrderBy,
                Ascending = request.Ascending,
                SearchCriteria = searchCriteria,
                SearchText = searchText,
                AdvancedSearch = true // Set this internally since we know this is an advanced search
            };

            return await SearchControllerHelper.ProcessAdvancedSearch<ContactFilterRequest, ContactCompositeSpecification, PaginationResponse<ContactModel>>(
                searchCriteria, searchText, request.PageIndex, request.PageSize, request.OrderBy, request.Ascending, 
                contactFilterRequest,
                "Contact",
                filterRequest => new ContactCompositeSpecification(filterRequest),
                async (userId, spec, pagination) => {
                    // Use regular specification - global filters handled automatically by BaseRepository
                    return (PaginationResponse<ContactModel>)await _manager.GetContactsWithSpecificationAsync(User, spec, (ContactFilterRequest)pagination);
                },
                CurrentUserId, _logger);
        }, "contact advanced search");
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

    #region AI-Powered Contact Data Processing

    /// <summary>
    /// Scans and processes uploaded files for contact data extraction using AI-powered analysis.
    /// </summary>
    /// <param name="req">File scan request containing the file to be processed</param>
    /// <param name="req.File">File to scan for contact data (required)</param>
    /// <example_uses>
    /// Scan business cards for contact information
    /// Upload contact forms for processing
    /// Analyze contact documents with AI
    /// Extract data from contact lists
    /// Process contact information from uploaded files
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to upload and scan documents for contact data extraction using AI.</when_to_use>
    /// <returns>Extracted contact data from the scanned file</returns>
    [HttpPost(APIDictionary.Contact + "/scan-data")]
    [AccessControlled(EntityTypes.Contact, "create")]
    public async Task<ActionResult> ScanContactData([FromForm] GeminiFileRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req?.File == null || req?.File.Length == 0)
            {
                throw new BusinessException("No valid file detected.");
            }

            string fileType = _geminiManager.FindFileType(req.File);

            if (string.IsNullOrEmpty(fileType)) 
            {
                throw new BusinessException("File type not compatible");
            }

            string response = await _geminiManager.ScanFileForGeminiProcessing(req);

            if (string.IsNullOrEmpty(response))
            {
                throw new BusinessException("Prompt configuration for contact data scanning is not found.");
            }

            return response.Trim();
        });
    }

    /// <summary>
    /// Analyzes uploaded files and extracts structured contact data using AI-powered data analysis.
    /// </summary>
    /// <param name="request">Analysis request containing file and analysis parameters</param>
    /// <param name="request.entityType">Should be set to 'Contact' for contact data analysis</param>
    /// <param name="request.analysisType">Type of analysis to perform on contact data</param>
    /// <example_uses>
    /// Analyze contact directories for structured data extraction
    /// Extract contact information from uploaded forms
    /// Process contact documents with AI
    /// Convert contact files into structured database entries
    /// Analyze business card data for key information
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to analyze files and extract structured contact data for database import.</when_to_use>
    /// <returns>Structured contact data extracted from the analyzed file</returns>
    [HttpPost(APIDictionary.Contact + "/analyse-file")]
    [AccessControlled(EntityTypes.Contact, "create")]
    public async Task<ActionResult> AnalyseContactData([FromBody] AnalyseFileRequest request)
    {
        return await HandleOperationAsync(async () => 
        {
            if (request == null)
            {
                throw new BusinessException("Invalid request.");
            }

            return await _geminiManager.ExtractDataAfterAnalysis(request, CurrentUserId);
        });
    }

    /// <summary>
    /// Bulk uploads multiple contact records using AI-assisted data processing and validation.
    /// </summary>
    /// <param name="req">Bulk upload request containing contact data</param>
    /// <param name="req.Type">Should be set to 'Contact' for contact bulk upload</param>
    /// <param name="req.Data">Array of contact data objects to upload</param>
    /// <param name="req.Options">Upload options and validation settings</param>
    /// <example_uses>
    /// Bulk upload 500 contacts from Excel
    /// Import multiple contacts from CSV file
    /// Mass upload contact data with AI validation
    /// Bulk import contact records with duplicate detection
    /// Upload large contact dataset with automated processing
    /// </example_uses>
    /// <when_to_use>Use this when the user wants to upload multiple contact records at once with AI-assisted processing.</when_to_use>
    /// <returns>Bulk upload results with success/failure status for each contact</returns>
    [HttpPost(APIDictionary.Contact + "/bulk-upload")]
    [AccessControlled(EntityTypes.Contact, "create")]
    public async Task<ActionResult> BulkUploadContacts([FromBody] BulkUploadRequest req) 
    {
        return await HandleOperationAsync(async () => 
        {
            if (req == null || string.IsNullOrEmpty(req.Type))
            {
                throw new BusinessException("Invalid request.");
            }

            // Ensure the request is for contact entities
            if (!req.Type.Equals("Contact", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("This endpoint only supports Contact bulk uploads.");
            }

            string response = await _geminiManager.BulkInsertRecordsAsync(req);
            return new { message = response };
        });
    }

    #endregion
    
    #region Private Helper Methods
    
    /// <summary>
    /// Validates email format using a simple regex pattern
    /// </summary>
    /// <param name="email">Email address to validate</param>
    /// <returns>True if email format is valid, false otherwise</returns>
    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
            
        try
        {
            // Use .NET's built-in email validation
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    #endregion

    /// <summary>
    /// Describes the Contact entity structure including all field configurations
    /// </summary>
    /// <returns>Entity and field metadata for Contact</returns>
    [HttpGet(APIDictionary.Contact + "/metadata-info")]
    [AccessControlled(EntityTypes.Contact, "read")]
    public async Task<ActionResult> GetMetadataInfo()
    {
        try
        {
            var entityDetails = await _entityConfigurationManager.GetEntityConfigurationDetailsAsync(User, "Contact");
            return Ok(entityDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Contact entity description");
            return StatusCode(500, new { error = "Failed to retrieve Contact entity description" });
        }
    }
}
