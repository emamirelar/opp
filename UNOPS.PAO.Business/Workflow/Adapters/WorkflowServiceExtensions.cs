using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Business.Workflow.Interfaces;
using UNOPS.Workflow.Business.Interfaces;
using UNOPS.Workflow.DataAccess;

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
        // Register the submodule's core workflow services (DbContext, IWorkflowManager, IWorkflowRepository)
        services.AddWorkflowServices(configure);

        // Register PAO-specific interface implementations
        services.AddScoped<IWorkflowUserContext, PaoWorkflowUserContext>();
        services.AddScoped<IEntityStageProvider, PaoEntityStageProvider>();
        services.AddScoped<IWorkflowApproverProvider, PaoWorkflowApproverProvider>();
        services.AddScoped<IPaoWorkflowApproverProvider, PaoWorkflowApproverProvider>();
        services.AddScoped<IWorkflowNotificationService, PaoWorkflowNotificationService>();

        return services;
    }
}
