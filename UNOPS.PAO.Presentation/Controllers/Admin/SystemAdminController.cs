namespace UNOPS.PAO.Presentation.Controllers.Admin;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Workflow;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Identity.Context;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.Identity.Security.Enums;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;
using UNOPS.PAO.UNOPSDataAccess.Utilities;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Models;

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
    private readonly AppDbContext appContext;
    private readonly IConfiguration configuration;
    private readonly ILogger<SystemAdminController> logger;
    private readonly IManagerWrapper managerWrapper;
    private readonly IWorkflowManager workflowManager;

    public SystemAdminController(
        IManagerWrapper manager,
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager,
        IPAOExecutionContext executionContext,
        UNOPSAppDbContext unopsContext,
        AppDbContext appContext,
        IConfiguration configuration,
        ILogger<SystemAdminController> logger,
        IWorkflowManager workflowManager)
    {
        systemAdminManager = manager.SystemAdminManager;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.executionContext = executionContext;
        this.unopsContext = unopsContext;
        this.appContext = appContext;
        this.configuration = configuration;
        this.logger = logger;
        managerWrapper = manager;
        this.workflowManager = workflowManager;
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
            },
            new
            {
                method = "POST",
                path = APIDictionary.SystemAdmin + "/regenerate-go-opportunity-pdfs",
                description = "Generate Submission and Approval PDFs for opportunities in Stage=GO. Checks each type separately - an opportunity may have submission PDF but not approval PDF (or vice versa).",
                parameters = new[]
                {
                    new { name = "onlyMissing", type = "bool", location = "query", description = "If true (default), only generate PDFs that are missing (checks submission and approval separately). If false, regenerate all." }
                },
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

    /// <summary>
    /// Regenerates Submission and Approval PDFs for opportunities in Stage=GO.
    /// Use to fix existing approved opportunities that are missing PDFs.
    /// Checks submission and approval PDFs separately - an opportunity may have one but not the other.
    /// Only available to System Admins (CanRunSeedings permission).
    /// </summary>
    /// <param name="onlyMissing">If true (default), only generate PDFs that are missing. If false, regenerate all.</param>
    [HttpPost(APIDictionary.SystemAdmin + "/regenerate-go-opportunity-pdfs")]
    [PermissionAuthorize(PermissionNames.CanRunSeedings)]
    public async Task<IActionResult> RegenerateGoOpportunityPdfs([FromQuery] bool onlyMissing = true)
    {
        try
        {
            logger.LogInformation("Starting regeneration of GO opportunity PDFs (onlyMissing={OnlyMissing})", onlyMissing);

            var statementDocTypeId = await unopsContext.DocumentTypes
                .AsNoTracking()
                .Where(dt => dt.EntityType == "Opportunity" && dt.Name == "Opportunity Statement" && !dt.IsDeleted)
                .Select(dt => dt.Id)
                .FirstOrDefaultAsync();

            var opportunities = await appContext.Opportunities
                .AsNoTracking()
                .Include(o => o.ResponsibleOrgUnit)
                .Include(o => o.ProposedInitiativeType)
                .Where(o => o.Stage == OpportunityWorkflow.Stages.Go && !o.IsDeleted && !string.IsNullOrEmpty(o.OpportunityStatementMarkdown))
                .ToListAsync();

            HashSet<int> opportunityIdsWithSubmissionPdf = new();
            HashSet<int> opportunityIdsWithApprovalPdf = new();

            if (onlyMissing && statementDocTypeId > 0)
            {
                var submissionDocRelationships = await appContext.DocumentRelationships
                    .AsNoTracking()
                    .Where(dr => dr.EntityType == "Opportunity" && !dr.IsDeleted)
                    .Join(appContext.Documents.AsNoTracking().Where(d =>
                        d.DocumentTypeId == statementDocTypeId && !d.IsDeleted &&
                        d.Name != null && d.Name.Contains("_Submission_")),
                        dr => dr.DocumentId,
                        d => d.Id,
                        (dr, _) => dr.EntityId)
                    .Distinct()
                    .ToListAsync();
                opportunityIdsWithSubmissionPdf = submissionDocRelationships.ToHashSet();

                var approvalDocRelationships = await appContext.DocumentRelationships
                    .AsNoTracking()
                    .Where(dr => dr.EntityType == "Opportunity" && !dr.IsDeleted)
                    .Join(appContext.Documents.AsNoTracking().Where(d =>
                        d.DocumentTypeId == statementDocTypeId && !d.IsDeleted &&
                        d.Name != null && d.Name.Contains("_Approved_")),
                        dr => dr.DocumentId,
                        d => d.Id,
                        (dr, _) => dr.EntityId)
                    .Distinct()
                    .ToListAsync();
                opportunityIdsWithApprovalPdf = approvalDocRelationships.ToHashSet();
            }

            var results = new List<object>();
            var submissionSuccess = 0;
            var submissionFailed = 0;
            var submissionSkipped = 0;
            var approvalSuccess = 0;
            var approvalFailed = 0;
            var approvalSkipped = 0;

            foreach (var opp in opportunities)
            {
                var oppId = opp.Id;
                var now = DateTime.UtcNow;
                var dateStr = now.ToString("yyyyMMdd");
                var timeStr = now.ToString("HHmm");

                var needsSubmissionPdf = !onlyMissing || !opportunityIdsWithSubmissionPdf.Contains(oppId);
                var needsApprovalPdf = !onlyMissing || !opportunityIdsWithApprovalPdf.Contains(oppId);

                try
                {
                    GeneratePdfResult? submissionResult = null;
                    if (needsSubmissionPdf)
                    {
                        var submissionFilename = $"Opportunity_{oppId}_Submission_{dateStr}_{timeStr}";
                        submissionResult = await managerWrapper.OpportunityManager.GenerateStatementPdfAsync(new GeneratePdfRequest
                        {
                            EntityName = "Opportunity",
                            EntityId = oppId,
                            Filename = submissionFilename
                        });

                        if (submissionResult.Success)
                        {
                            submissionSuccess++;
                            logger.LogInformation("Generated submission PDF for Opportunity {OpportunityId}", oppId);
                        }
                        else
                        {
                            submissionFailed++;
                            logger.LogWarning("Failed to generate submission PDF for Opportunity {OpportunityId}: {Error}", oppId, submissionResult.Error);
                        }
                    }
                    else
                    {
                        submissionSkipped++;
                    }

                    GeneratePdfResult? approvalResult = null;
                    if (needsApprovalPdf)
                    {
                        var history = workflowManager.GetWorkflowHistory(OpportunityWorkflow.StateMachine, "Opportunity", oppId).ToList();
                        var auditTrail = await BuildAuditTrailMarkdownForApprovalAsync(
                            history, oppId, opp.ResponsibleOrgUnitId,
                            opp.ResponsibleOrgUnit?.Name, opp.ProposedInitiativeType?.Name);
                        var combinedMarkdown = opp.OpportunityStatementMarkdown + "\n\n" + auditTrail;

                        var approvalFilename = $"Opportunity_{oppId}_Approved_{dateStr}";
                        approvalResult = await managerWrapper.OpportunityManager.GenerateStatementPdfAsync(new GeneratePdfRequest
                        {
                            EntityName = "Opportunity",
                            EntityId = oppId,
                            Data = combinedMarkdown,
                            Filename = approvalFilename
                        });

                        if (approvalResult.Success)
                        {
                            approvalSuccess++;
                            logger.LogInformation("Generated approval PDF for Opportunity {OpportunityId}", oppId);
                        }
                        else
                        {
                            approvalFailed++;
                            logger.LogWarning("Failed to generate approval PDF for Opportunity {OpportunityId}: {Error}", oppId, approvalResult.Error);
                        }
                    }
                    else
                    {
                        approvalSkipped++;
                    }

                    results.Add(new
                    {
                        opportunityId = oppId,
                        opportunityName = opp.Name,
                        submissionGenerated = needsSubmissionPdf,
                        submissionSuccess = submissionResult?.Success,
                        submissionError = submissionResult?.Error,
                        submissionSkipped = !needsSubmissionPdf,
                        approvalGenerated = needsApprovalPdf,
                        approvalSuccess = approvalResult?.Success,
                        approvalError = approvalResult?.Error,
                        approvalSkipped = !needsApprovalPdf
                    });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error generating PDFs for Opportunity {OpportunityId}", oppId);
                    if (needsSubmissionPdf) submissionFailed++;
                    if (needsApprovalPdf) approvalFailed++;
                    results.Add(new
                    {
                        opportunityId = oppId,
                        opportunityName = opp.Name,
                        submissionGenerated = needsSubmissionPdf,
                        submissionSuccess = (bool?)false,
                        submissionError = ex.Message,
                        submissionSkipped = !needsSubmissionPdf,
                        approvalGenerated = needsApprovalPdf,
                        approvalSuccess = (bool?)false,
                        approvalError = ex.Message,
                        approvalSkipped = !needsApprovalPdf
                    });
                }
            }

            logger.LogInformation(
                "Completed GO opportunity PDF regeneration. Processed: {Count}, Submission: {SubSuccess} success, {SubFailed} failed, {SubSkipped} skipped. Approval: {AppSuccess} success, {AppFailed} failed, {AppSkipped} skipped",
                opportunities.Count, submissionSuccess, submissionFailed, submissionSkipped, approvalSuccess, approvalFailed, approvalSkipped);

            return Ok(new
            {
                message = "GO opportunity PDF regeneration completed",
                totalProcessed = opportunities.Count,
                submissionSuccess,
                submissionFailed,
                submissionSkipped,
                approvalSuccess,
                approvalFailed,
                approvalSkipped,
                results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during GO opportunity PDF regeneration");
            return StatusCode(500, new { error = "Failed to regenerate GO opportunity PDFs", details = ex.Message });
        }
    }

    private async Task<string> BuildAuditTrailMarkdownForApprovalAsync(
        List<WorkflowHistoryModel> history,
        int opportunityId,
        int? responsibleOrgUnitId,
        string? responsibleOrgUnitName,
        string? proposedInitiativeTypeName)
    {
        var sortedHistory = history.OrderByDescending(h => h.CreatedDate).ToList();
        var submitRecord = sortedHistory.FirstOrDefault(h => string.Equals(h.Action, "Submit", StringComparison.OrdinalIgnoreCase));
        var approveRecord = sortedHistory.FirstOrDefault(h => string.Equals(h.Action, "Approve", StringComparison.OrdinalIgnoreCase));

        static string FormatDate(DateTime? date) =>
            date.HasValue ? date.Value.ToString("dd MMM yyyy, HH:mm", System.Globalization.CultureInfo.InvariantCulture) : "N/A";

        var orgUnitCode = responsibleOrgUnitName ?? "N/A";
        var initiativeType = proposedInitiativeTypeName ?? "initiative";
        var acknowledgmentStatement = $"I confirm that, based on the information presented in the Opportunity Statement, I give approval for UNOPS Org Unit \"{orgUnitCode}\" to continue development of this Opportunity as a {initiativeType}.";

        var submitDate = submitRecord != null ? submitRecord.CreatedDate : (DateTime?)null;
        var (submitUserName, submitPosition, _) = await GetUserDetailsForAuditTrailAsync(submitRecord?.User?.Id ?? 0);
        var submitRemarks = submitRecord?.Comment ?? "None provided";

        var approveDate = approveRecord?.CompletedOn ?? approveRecord?.CreatedDate;
        var (approveUserName, approvePosition, approveDoa) = await GetUserDetailsForAuditTrailAsync(
            approveRecord?.User?.Id ?? 0, opportunityId, responsibleOrgUnitId);

        return $@"
---

## Go Decision Audit Trail

### Submission Details
| Field | Value |
|-------|-------|
| **Date of Submission** | {FormatDate(submitDate)} |
| **Submitted By** | {submitUserName} |
| **Position Title** | {submitPosition} |
| **Remarks for Decision Maker** | {submitRemarks} |

### Decision Details
| Field | Value |
|-------|-------|
| **Date of Decision** | {FormatDate(approveDate)} |
| **Decision Maker** | {approveUserName} |
| **DOA Level** | {approveDoa} |
| **Position Title** | {approvePosition} |
| **Acknowledged Statement** | {acknowledgmentStatement} |
| **Decision Rationale** | {approveRecord?.Comment ?? "None provided"} |

---
";
    }

    private async Task<(string userName, string position, string? doaLevel)> GetUserDetailsForAuditTrailAsync(
        int userId, int? opportunityId = null, int? responsibleOrgUnitId = null)
    {
        if (userId <= 0)
            return ("N/A", "N/A", null);

        var user = await appContext.PAOUsers
            .AsNoTracking()
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return ("N/A", "N/A", null);

        var userName = user.UserProfile?.Name ?? user.Email ?? "N/A";
        var position = user.UserProfile?.Position ?? "N/A";

        string? doaLevel = null;
        if (opportunityId.HasValue && responsibleOrgUnitId.HasValue)
        {
            var doaEntityUserRole = await appContext.EntityUserRoles
                .AsNoTracking()
                .Include(eur => eur.EntityRole)
                .Where(eur => eur.UserId == userId
                    && eur.EntityId == responsibleOrgUnitId.Value
                    && eur.EntityType == "OrganizationHierarchy"
                    && eur.EntityRole != null
                    && eur.EntityRole.Code != null
                    && eur.EntityRole.Code.StartsWith("DoA"))
                .FirstOrDefaultAsync();

            if (doaEntityUserRole?.EntityRole != null)
                doaLevel = doaEntityUserRole.EntityRole.Name ?? doaEntityUserRole.EntityRole.Code;
        }

        return (userName, position, doaLevel ?? "N/A");
    }
}
