namespace UNOPS.PAO.UNOPSPresentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Presentation.Controllers;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.UNOPSPresentation.Authorization;

[Route("api/[controller]")]
public class SecureResourceController : BaseController
{
    private readonly IPermissionService _permissionService;

    public SecureResourceController(
        IPermissionService permissionService,
        ILogger<SecureResourceController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _permissionService = permissionService;
    }

    // A simple test endpoint that returns auth info
    [HttpGet("authtest")]
    public async Task<ActionResult> GetAuthInfo()
    {
        return await HandleOperationAsync(async () =>
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            var isInternal = User.HasClaim(c => c.Type == "IsInternal" && c.Value.ToLower() == "true");
            var roles = User.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var username = User.Identity.Name;
            var email = User.Claims
                .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

            return new
            {
                username,
                email,
                isAuthenticated = true,
                isInternal,
                roles,
                claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };
        });
    }

    // Define a class for Partner objects to avoid implicitly typed arrays
    private class PartnerModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }
        public string CreatedBy { get; set; }

        public PartnerModel(int id, string name, bool isPublic, string createdBy)
        {
            Id = id;
            Name = name;
            IsPublic = isPublic;
            CreatedBy = createdBy;
        }
    }

    // Define a class for Contact objects
    private class ContactModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPublic { get; set; }
        public string CreatedBy { get; set; }

        public ContactModel(int id, string name, bool isPublic, string createdBy)
        {
            Id = id;
            Name = name;
            IsPublic = isPublic;
            CreatedBy = createdBy;
        }
    }

    // Example using the attribute-based authorization
    [HttpGet("partners")]
    [EntityPermission("Partner", "Read")]
    public async Task<ActionResult> GetPartners()
    {
        return await HandleOperationAsync(async () =>
        {
            string currentUser = User.Identity?.Name ?? string.Empty;

            // This endpoint is protected by the EntityPermission attribute
            // Only users with permissions to read Partner entities can access it
            return new 
            { 
                message = "You have access to read partners",
                partners = new PartnerModel[]
                {
                    new PartnerModel(1, "Test Partner 1", true, "system"),
                    new PartnerModel(2, "Test Partner 2", true, "system"),
                    new PartnerModel(11, "Private Partner 1", false, currentUser),
                    new PartnerModel(12, "Private Partner 2", false, "someone.else@example.com")
                }
            };
        });
    }

    // Example using programmatic authorization
    [HttpGet("partners/{id}")]
    public async Task<ActionResult> GetPartner(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            // Check permission programmatically
            var canRead = await _permissionService.CanPerformActionAsync("Partner", "Read", User);
            if (!canRead)
            {
                throw new UnauthorizedAccessException("You don't have permission to read partners");
            }

            string currentUser = User.Identity?.Name ?? string.Empty;

            // Create test entities with different permissions
            var partner = id switch
            {
                // Public partners (accessible to everyone)
                <= 10 => new PartnerModel(id, $"Public Partner {id}", true, "system"),
                // User's own partners
                <= 20 => new PartnerModel(id, $"Your Partner {id}", false, currentUser),
                // Other users' partners
                _ => new PartnerModel(id, $"Private Partner {id}", false, "someone.else@example.com")
            };
            
            // Additional row-level permission check
            var canAccessThisPartner = await _permissionService.CanPerformActionAsync(
                "Partner", "Read", User, partner);
            if (!canAccessThisPartner)
            {
                throw new UnauthorizedAccessException("You don't have permission to read this specific partner");
            }

            return partner;
        });
    }

    // Example combining attribute-based authorization with query filtering
    [HttpGet("contacts")]
    [EntityPermission("Contact", "Read")]
    public async Task<ActionResult> GetContacts()
    {
        return await HandleOperationAsync(async () =>
        {
            string currentUser = User.Identity?.Name ?? string.Empty;

            // This is a simplified example, you would normally use a repository
            // and apply the filter to the database query
            var allContacts = new List<ContactModel>
            {
                new ContactModel(1, "Public Contact 1", true, "system"),
                new ContactModel(2, "Public Contact 2", true, "system"),
                new ContactModel(11, "Your Contact 1", false, currentUser),
                new ContactModel(12, "Your Contact 2", false, currentUser),
                new ContactModel(21, "Private Contact 1", false, "someone.else@example.com"),
                new ContactModel(22, "Private Contact 2", false, "someone.else@example.com")
            }.AsQueryable();

            // Apply security filters
            // In a real application, this would filter at the database level
            // var filteredQuery = await _permissionService.ApplySecurityFiltersAsync(repository.Contacts, User);
            
            // Simulate filtering for demo purposes without using null propagation operator
            var result = allContacts.Where(c => 
                // Check if createdBy matches current user (handles null safely)
                (currentUser.Length > 0 && c.CreatedBy == currentUser) || 
                // Check if contact is public
                c.IsPublic ||
                // Check if user has admin or internal role
                User.IsInRole("Administrator") || 
                User.IsInRole("Internal"));

            return result;
        });
    }

    [HttpPost("partners")]
    [EntityPermission("Partner", "Create")]
    public async Task<ActionResult> CreatePartner([FromBody] object partnerData)
    {
        return await HandleOperationAsync(async () =>
        {
            // In a real application, you would create the partner in the database
            return new { message = "Partner created", data = partnerData };
        });
    }

    [HttpPut("partners/{id}")]
    [EntityPermission("Partner", "Update")]
    public async Task<ActionResult> UpdatePartner(int id, [FromBody] object partnerData)
    {
        return await HandleOperationAsync(async () =>
        {
            string currentUser = User.Identity?.Name ?? string.Empty;

            // Simulate fetching the partner by ID
            var partner = id switch
            {
                // Public partners (accessible to everyone)
                <= 10 => new PartnerModel(id, $"Public Partner {id}", true, "system"),
                // User's own partners
                <= 20 => new PartnerModel(id, $"Your Partner {id}", false, currentUser),
                // Other users' partners
                _ => new PartnerModel(id, $"Private Partner {id}", false, "someone.else@example.com")
            };
            
            // Row-level permission check
            var canUpdateThisPartner = await _permissionService.CanPerformActionAsync(
                "Partner", "Update", User, partner);
            if (!canUpdateThisPartner)
            {
                throw new UnauthorizedAccessException("You don't have permission to update this specific partner");
            }

            // In a real application, you would update the partner in the database
            return new { message = "Partner updated", data = partnerData };
        });
    }

    [HttpDelete("partners/{id}")]
    [EntityPermission("Partner", "Delete")]
    public async Task<ActionResult> DeletePartner(int id)
    {
        return await HandleOperationAsync(async () =>
        {
            // In a real application, you would delete the partner from the database
            return new { message = $"Partner with ID {id} deleted" };
        });
    }
} 