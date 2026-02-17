using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.PAO.Business.Workflow.StageRequirements;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.Business.Managers;
using UNOPS.Workflow.DataAccess;
using UNOPS.Workflow.Models.Requirements;

namespace UNOPS.PAO.Business.Workflow.Adapters;

/// <summary>
/// Extension methods for registering PAO-specific workflow services.
/// </summary>
public static class WorkflowServiceExtensions
{
    /// <summary>
    /// Adds PAO-specific workflow services to the service collection.
    /// Includes both the submodule's core services and PAO's implementations.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action for workflow options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPaoWorkflowServices(
        this IServiceCollection services,
        Action<WorkflowOptions> configure)
    {
        var options = new WorkflowOptions();
        configure(options);

        // Check if IAM authentication is being used (connection string has no password)
        var useIamAuth = false;
        if (!string.IsNullOrEmpty(options.ConnectionString))
        {
            var connStringBuilder = new NpgsqlConnectionStringBuilder(options.ConnectionString);
            useIamAuth = string.IsNullOrEmpty(connStringBuilder.Password);
        }

        if (useIamAuth)
        {
            // IAM authentication detected - skip submodule's AddWorkflowServices
            // because it tries to register DbContext with connection string (which fails)
            // Instead, manually register only the services we need
            // DbContext will be registered separately in Startup.cs with dataSource
            
            // Register the core workflow manager
            services.AddScoped<IWorkflowManager, WorkflowManager>();

            // Register the repository implementation
            services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        }
        else
        {
            // Traditional authentication - use submodule's registration
            // Register the submodule's core workflow services (DbContext, IWorkflowManager, IWorkflowRepository)
            services.AddWorkflowServices(configure);
        }

        // Register PAO-specific interface implementations
        services.AddScoped<IWorkflowUserContext, PaoWorkflowUserContext>();
        services.AddScoped<IEntityStageProvider, PaoEntityStageProvider>();
        services.AddScoped<IWorkflowApproverProvider, PaoWorkflowApproverProvider>();
        services.AddScoped<IPaoWorkflowApproverProvider, PaoWorkflowApproverProvider>();
        
        // Register notification service as both interface and concrete type
        // Concrete type is needed for PAO-specific methods like NotifyInternalStakeholdersOnGoDecisionAsync
        services.AddScoped<PaoWorkflowNotificationService>();
        services.AddScoped<IWorkflowNotificationService>(sp => sp.GetRequiredService<PaoWorkflowNotificationService>());

        // Register stage requirements provider for Opportunity workflow validation
        services.AddScoped<IStageRequirementsProvider, OpportunityStageRequirementsProvider>();

        return services;
    }
}
