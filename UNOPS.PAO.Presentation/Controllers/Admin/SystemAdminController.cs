namespace UNOPS.PAO.Presentation.Controllers.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Identity.Security.Enums;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;
using UNOPS.PAO.UNOPSDataAccess.Utilities;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]
public class SystemAdminController : ControllerBase
{
    private readonly ISystemAdminManager systemAdminManager;
    private readonly UserManager<PAOIdentityUser> userManager;
    private readonly RoleManager<PAOIdentityRole> roleManager;
    private readonly IPAOExecutionContext executionContext;
    private readonly UNOPSAppDbContext unopsContext;
    private readonly IConfiguration configuration;
    private readonly ILogger<SystemAdminController> logger;
    private readonly IManagerWrapper managerWrapper;
    
    public SystemAdminController(
        IManagerWrapper manager, 
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager,
        IPAOExecutionContext executionContext,
        UNOPSAppDbContext unopsContext,
        IConfiguration configuration,
        ILogger<SystemAdminController> logger)
    {
        this.systemAdminManager = manager.SystemAdminManager;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.executionContext = executionContext;
        this.unopsContext = unopsContext;
        this.configuration = configuration;
        this.logger = logger;
        this.managerWrapper = manager;
    }

    /// <summary>
    /// Get list of all available system admin endpoints with descriptions
    /// </summary>
    [HttpGet(APIDictionary.SystemAdmin + "/endpoints")]
    [PermissionAuthorize(PermissionNames.CanRunMigrations)]
    public IActionResult GetAvailableEndpoints()
    {
        var endpoints = new object[]
        {
            new
            {
                method = "GET",
                path = APIDictionary.SystemAdmin + "/endpoints",
                description = "Get list of all available system admin endpoints",
                parameters = Array.Empty<object>(),
                permission = "CanRunMigrations",
                examples = (string[]?)null
            },
            new
            {
                method = "GET",
                path = APIDictionary.SystemAdmin + "/auth-debug",
                description = "Debug authentication and permission information for current user",
                parameters = Array.Empty<object>(),
                permission = "Authenticated (any user)",
                examples = (string[]?)null
            },
            new
            {
                method = "GET",
                path = APIDictionary.SystemAdmin + "/migrations/run",
                description = "Run all pending database migrations",
                parameters = Array.Empty<object>(),
                permission = "CanRunMigrations",
                examples = (string[]?)null
            },
            new
            {
                method = "GET",
                path = APIDictionary.SystemAdmin + "/seeding/run",
                description = "Run all configured seed steps (only changed/new ones will execute)",
                parameters = Array.Empty<object>(),
                permission = "CanRunSeedings",
                examples = (string[]?)null
            },
            new
            {
                method = "GET",
                path = APIDictionary.SystemAdmin + "/seeding/run/{name}",
                description = "Run a specific seeder by name, forcing execution regardless of changes",
                parameters = new[]
                {
                    new { name = "name", type = "string", location = "path", description = "Seeder name from SeedConfiguration.json (e.g., 'Roles', 'Entities', 'DocumentTypes')" }
                },
                permission = "CanRunSeedings",
                examples = new[] { "Roles", "Entities", "DocumentTypes", "LiaisonOffices", "AspNetUsers", "EntityManagers", "PartnerTree", "AiPrompts", "EntityPermissions", "UserProfiles", "Partners", "SequenceResync" }
            },
            new
            {
                method = "GET",
                path = APIDictionary.SystemAdmin + "/seed-scripts/truncate",
                description = "Truncate the entire SeedScripts table (all seeders will re-run on next execution)",
                parameters = Array.Empty<object>(),
                permission = "CanRunSeedings",
                examples = (string[]?)null
            },
            new
            {
                method = "GET",
                path = APIDictionary.SystemAdmin + "/seed-scripts/delete/{name}",
                description = "Delete a specific seed script record (that seeder will re-run on next execution)",
                parameters = new[]
                {
                    new { name = "name", type = "string", location = "path", description = "Seed script name (e.g., 'Roles', 'Entities')" }
                },
                permission = "CanRunSeedings",
                examples = new[] { "Roles", "Entities", "DocumentTypes", "LiaisonOffices", "Partners" }
            },
            new
            {
                method = "POST",
                path = APIDictionary.SystemAdmin + "/output-embeddings/generate",
                description = "Generate embeddings and keywords for all Output entities (takes ~2 minutes)",
                parameters = Array.Empty<object>(),
                permission = "CanRunSeedings",
                examples = (string[]?)null
            },
            new
            {
                method = "POST",
                path = APIDictionary.SystemAdmin + "/clean-up-users",
                description = "Migrate placeholder AspNetUsers IDs to ERP Resource IDs (runs Fix_AspNetUsers_conflicts.sql)",
                parameters = Array.Empty<object>(),
                permission = "CanRunSeedings",
                examples = (string[]?)null
            }
        };

        return Ok(new
        {
            totalEndpoints = endpoints.Length,
            baseUrl = APIDictionary.SystemAdmin,
            endpoints = endpoints
        });
    }

    [HttpGet(APIDictionary.SystemAdmin + "/auth-debug")]
    [Authorize]
    public async Task<IActionResult> DebugAuth()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Ok(new { 
                authenticated = false, 
                message = "No user found" 
            });
        }

        var roles = await userManager.GetRolesAsync(user);
        var permissions = new List<string>();

        foreach (var roleName in roles)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role != null)
            {
                var claims = await roleManager.GetClaimsAsync(role);
                var rolePermissions = claims
                    .Where(c => c.Type == "permission")
                    .Select(c => c.Value)
                    .ToList();
                permissions.AddRange(rolePermissions);
            }
        }

        var contextPermissions = executionContext.UserPermissions.Select(p => p.Name).ToList();

        return Ok(new { 
            authenticated = true,
            userId = user.Id,
            email = user.Email,
            userName = user.UserName,
            roles = roles,
            permissionsFromDatabase = permissions,
            permissionsFromContext = contextPermissions,
            claimsPrincipalClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    [HttpGet(APIDictionary.SystemAdmin + "/migrations/run")]
    [PermissionAuthorize(PermissionNames.CanRunMigrations)]
    public async Task<IActionResult> RunMigrations()
    {
        await systemAdminManager.RunMigrations();

        return Ok(new { message = "Migrations completed successfully" });
    }

    [HttpGet(APIDictionary.SystemAdmin + "/seeding/run")]
    [PermissionAuthorize(PermissionNames.CanRunSeedings)]
    public async Task<IActionResult> RunSeeding()
    {
        await systemAdminManager.RunSeeding();

        return Ok(new { message = "Seeding completed successfully" });
    }

    /// <summary>
    /// Run a specific seeder by name (e.g., "Roles", "Entities", "DocumentTypes")
    /// Forces execution regardless of whether the file has changed
    /// </summary>
    /// <param name="name">The name of the seeder from SeedConfiguration.json</param>
    [HttpGet(APIDictionary.SystemAdmin + "/seeding/run/{name}")]
    [PermissionAuthorize(PermissionNames.CanRunSeedings)]
    public async Task<IActionResult> RunSpecificSeeder(string name)
    {
        try
        {
            await systemAdminManager.RunSpecificSeeder(name);
            return Ok(new { message = $"Seeder '{name}' executed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Truncate the entire SeedScripts table
    /// This will cause all seeders to re-run on next seeding execution
    /// </summary>
    [HttpGet(APIDictionary.SystemAdmin + "/seed-scripts/truncate")]
    [PermissionAuthorize(PermissionNames.CanRunSeedings)]
    public async Task<IActionResult> TruncateSeedScripts()
    {
        await systemAdminManager.TruncateSeedScripts();
        return Ok(new { message = "SeedScripts table truncated successfully. All seeders will re-run on next execution." });
    }

    /// <summary>
    /// Delete a specific seed script record by name
    /// This will cause that specific seeder to re-run on next seeding execution
    /// </summary>
    /// <param name="name">The name of the seed script (e.g., "Roles", "Entities")</param>
    [HttpGet(APIDictionary.SystemAdmin + "/seed-scripts/delete/{name}")]
    [PermissionAuthorize(PermissionNames.CanRunSeedings)]
    public async Task<IActionResult> DeleteSeedScript(string name)
    {
        await systemAdminManager.DeleteSeedScript(name);
        return Ok(new { message = $"Seed script '{name}' deleted successfully. It will re-run on next execution." });
    }

    /// <summary>
    /// Generate embeddings and keywords for all Output entities
    /// This is a long-running operation (~2 minutes)
    /// Creates semantic embeddings and AI-generated keywords for hybrid search
    /// </summary>
    [HttpGet(APIDictionary.SystemAdmin + "/output-embeddings/generate")]
    [PermissionAuthorize(PermissionNames.CanRunSeedings)]
    public async Task<IActionResult> GenerateOutputEmbeddings()
    {
        try
        {
            logger.LogInformation("🚀 Starting Output embeddings generation via API endpoint");

            // Get the GeminiManager which already has AiContextualService configured
            var geminiManager = managerWrapper.GeminiManager;
            if (geminiManager == null)
            {
                return StatusCode(500, new { error = "GeminiManager not available in ManagerWrapper" });
            }

            // Create OutputEmbeddingSeeder instance - it will use the GeminiManager's AiContextualService
            var seeder = new OutputEmbeddingSeeder(
                unopsContext,
                configuration,
                geminiManager
            );

            // Generate embeddings
            await seeder.GenerateOutputEmbeddingsAsync();

            logger.LogInformation("✅ Output embeddings generation completed successfully");

            return Ok(new 
            { 
                message = "Output embeddings generated successfully",
                note = "Embeddings have been generated for all active Outputs with keywords for hybrid search"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error generating output embeddings");
            return StatusCode(500, new { error = "Failed to generate output embeddings", details = ex.Message });
        }
    }

    /// <summary>
    /// Run AspNetUsers cleanup script: migrate placeholder IDs to ERP Resource IDs.
    /// Use when EDS fails due to placeholder vs ERP ID conflict (manual intervention).
    /// </summary>
    [HttpPost(APIDictionary.SystemAdmin + "/clean-up-users")]
    [PermissionAuthorize(PermissionNames.CanRunSeedings)]
    public async Task<IActionResult> CleanUpUsers()
    {
        try
        {
            logger.LogInformation("Starting AspNetUsers cleanup (Fix_AspNetUsers_conflicts.sql)");

            var sqlScript = MigrationSqlScriptExecutor.ReadSqlScript("Fix_AspNetUsers_conflicts.sql");
            await unopsContext.Database.ExecuteSqlRawAsync(sqlScript);

            logger.LogInformation("AspNetUsers cleanup completed successfully");

            return Ok(new
            {
                message = "AspNetUsers cleanup completed successfully",
                note = "Placeholder user IDs have been migrated to ERP Resource IDs where applicable"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error running AspNetUsers cleanup");
            return StatusCode(500, new { error = "Failed to run AspNetUsers cleanup", details = ex.Message });
        }
    }
}
