using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;

namespace UNOPS.PAO.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RBACTestController : ControllerBase
{
    private readonly IContactManager _contactManager;
    private readonly ILogger<RBACTestController> _logger;

    public RBACTestController(IContactManager contactManager, ILogger<RBACTestController> logger)
    {
        _contactManager = contactManager;
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint to verify RBAC is working for Contact operations
    /// This will call the GetContactsAsync method which has [RBAC("read", Entity = "Contact")] attribute
    /// </summary>
    [HttpGet("contacts")]
    public async Task<ActionResult<PaginationResponse<ContactModel>>> TestGetContacts([FromQuery] PaginationRequest request)
    {
        try
        {
            _logger.LogInformation("Testing RBAC for Contact read operation");
            
            // This will trigger the RBAC interceptor
            var result = await _contactManager.GetContactsAsync(User, request);
            
            _logger.LogInformation("RBAC test successful - returned {Count} contacts", result.Records?.Count ?? 0);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("RBAC blocked access: {Message}", ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RBAC test");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint to verify RBAC is working for specific Contact access
    /// This will call the GetContactAsync method which has [RBAC("read", Entity = "Contact", EntityIdParameterName = "id")] attribute
    /// </summary>
    [HttpGet("contacts/{id}")]
    public async Task<ActionResult<ContactModel>> TestGetContact(int id)
    {
        try
        {
            _logger.LogInformation("Testing RBAC for Contact read operation with ID {ContactId}", id);
            
            // This will trigger the RBAC interceptor with entity-specific access check
            var result = await _contactManager.GetContactAsync(User, id);
            
            if (result == null)
            {
                _logger.LogInformation("Contact {ContactId} not found or access denied", id);
                return NotFound();
            }
            
            _logger.LogInformation("RBAC test successful - returned contact {ContactId}", id);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("RBAC blocked access to contact {ContactId}: {Message}", id, ex.Message);
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RBAC test for contact {ContactId}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test endpoint to check user permissions for Contact entity
    /// </summary>
    [HttpGet("permissions")]
    public async Task<ActionResult<object>> TestGetPermissions()
    {
        try
        {
            _logger.LogInformation("Testing permission check for Contact entity");
            
            // Test the business security service directly
            var businessSecurityService = HttpContext.RequestServices.GetService<UNOPS.PAO.UNOPSBusiness.Services.IBusinessSecurityService>();
            if (businessSecurityService == null)
            {
                return StatusCode(500, new { error = "Business security service not found" });
            }
            
            var permissions = await businessSecurityService.GetEntityPermissionsAsync(User, "Contact");
            
            _logger.LogInformation("Permission check successful: CanRead={CanRead}, CanCreate={CanCreate}, CanUpdate={CanUpdate}, CanDelete={CanDelete}", 
                permissions.CanRead, permissions.CanCreate, permissions.CanUpdate, permissions.CanDelete);
                
            return Ok(new 
            { 
                Entity = "Contact",
                Permissions = permissions,
                UserClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during permission test");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Debug endpoint to check if ContactManager has security service injected
    /// </summary>
    [HttpGet("debug")]
    public ActionResult<object> TestDependencyInjection()
    {
        try
        {
            _logger.LogInformation("Testing dependency injection for ContactManager");
            
            // Check if ContactManager is properly injected
            var contactManagerType = _contactManager.GetType();
            _logger.LogInformation("ContactManager type: {Type}", contactManagerType.FullName);
            
            // Check if it's a proxy (Castle DynamicProxy)
            var isProxy = contactManagerType.FullName?.Contains("Proxy") == true;
            _logger.LogInformation("Is ContactManager a proxy? {IsProxy}", isProxy);
            
            // Try to get the business security service
            var businessSecurityService = HttpContext.RequestServices.GetService<UNOPS.PAO.UNOPSBusiness.Services.IBusinessSecurityService>();
            var hasBusinessService = businessSecurityService != null;
            _logger.LogInformation("Business security service available? {HasService}", hasBusinessService);
            
            return Ok(new 
            { 
                ContactManagerType = contactManagerType.FullName,
                IsProxy = isProxy,
                HasBusinessSecurityService = hasBusinessService,
                Message = isProxy ? "✅ RBAC proxy is working" : "❌ RBAC proxy not detected"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during dependency injection test");
            return StatusCode(500, new { error = ex.Message });
        }
    }
} 