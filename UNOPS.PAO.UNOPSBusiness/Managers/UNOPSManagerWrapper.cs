using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Helpers;
using System.Net.Http;

namespace UNOPS.PAO.UNOPSBusiness.Managers;

using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Managers;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Identity.Entities;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.DataAccess.Interfaces;

public class UNOPSManagerWrapper : ManagerWrapper
{
    private readonly UNOPSSystemAdminManager systemAdminManager;
    private readonly IContactManager contactManager;
    private readonly UNOPSInteractionManager interactionManager;
    private readonly UNOPSPartnerTreeManager partnerTreeManager;
    private readonly UNOPSPartnerManager partnerManager;
    private readonly UNOPSGeminiManager geminiManager;
    private readonly ImageGenerationManager imageGenerationManager;
    private readonly LinkManager linkManager;
    private readonly UNOPSUserManagementManager userManagementManager;
    private readonly UNOPSAiPromptManager aiPromptManager;
    private readonly UNOPSEntityConfigurationManager entityConfigurationManager;
    private readonly UNOPSGmailAddonManager gmailAddonManager;
    private readonly BaseEngagementManager baseEngagementManager;
    private readonly UNOPSOpportunityManager opportunityManager;
    private readonly CommentManager commentManager;
    private readonly UNOPSAuditLogManager auditLogManager;
    private readonly UNOPSRiskManager riskManager;

    public UNOPSManagerWrapper(IMapper mapper, AppDbContext context, UNOPSAppDbContext opsContext, IConfiguration configuration,
                               UserManager<PAOIdentityUser> userManager, RoleManager<PAOIdentityRole> roleManager, IHttpContextAccessor httpContextAccessor, IPermissionService permissionService, GlobalFilterService globalFilterService, HttpClient httpClient, ILoggerFactory loggerFactory, IServiceProvider serviceProvider, IUserInfoService userInfoService, IUserPreferenceService userPreferenceService, IUserProfileCacheService userProfileCacheService, IScreenContextCacheService screenContextCacheService, IGeoTimeCacheService geoTimeCacheService, IAiPromptCacheService aiPromptCacheService) : base(mapper, context, userManager, httpContextAccessor, configuration, serviceProvider)
    {
        // Create a MemoryCache instance for services that need it
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        // Create logger for UNOPSPartnerManager
        var partnerManagerLogger = loggerFactory.CreateLogger<UNOPSPartnerManager>();
        
        // Create logger for UNOPSContactManager
        var contactManagerLogger = loggerFactory.CreateLogger<UNOPSContactManager>();
        
        // Create logger for UNOPSGeminiManager
        var geminiManagerLogger = loggerFactory.CreateLogger<UNOPSGeminiManager>();
        
        // Create logger for ImageGenerationManager
        var imageGenerationManagerLogger = loggerFactory.CreateLogger<ImageGenerationManager>();
        
        // Create logger for UNOPSUserManagementManager
        var userManagementManagerLogger = loggerFactory.CreateLogger<UNOPSUserManagementManager>();
        
        // Create a PartnerTreeService instance
        var partnerTreeRepository = new DataRepository<UNOPSPartnerTree>(opsContext);
        var partnerTreeService = new PartnerTreeService(partnerTreeRepository, memoryCache);

        var notificationManager = serviceProvider.GetRequiredService<NotificationManager>();

        // Create DbContextFactory for parallel query execution in InteractionManager and OpportunityManager
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<UNOPSAppDbContext>>();

        systemAdminManager = new UNOPSSystemAdminManager(opsContext, configuration, serviceProvider);
        contactManager = new UNOPSContactManager(mapper, opsContext, configuration, permissionService, globalFilterService, httpContextAccessor, contactManagerLogger, serviceProvider);
        interactionManager = new UNOPSInteractionManager(mapper, opsContext, configuration, partnerTreeService, permissionService, globalFilterService, httpContextAccessor, serviceProvider, userProfileCacheService, dbContextFactory);
        
        partnerTreeManager = new UNOPSPartnerTreeManager(mapper, opsContext, configuration, partnerTreeService, permissionService);
        partnerManager = new UNOPSPartnerManager(mapper, opsContext, configuration, partnerTreeService, partnerManagerLogger, permissionService, globalFilterService, httpContextAccessor, serviceProvider);
        linkManager = new LinkManager(mapper, opsContext);
        
        // Create ImageGenerationManager for AI-based image generation (uses same auth pattern as AiContextualService)
        imageGenerationManager = new ImageGenerationManager(configuration, imageGenerationManagerLogger);
        
        // Create GeminiManager first (without userManagementManager dependency)
        geminiManager = new UNOPSGeminiManager(mapper, opsContext, configuration, geminiManagerLogger, null, userInfoService, userManager, roleManager, userPreferenceService, userProfileCacheService, screenContextCacheService, geoTimeCacheService, aiPromptCacheService, memoryCache, httpClient);
        
        // Create UserManagementManager with GeminiManager dependency
        userManagementManager = new UNOPSUserManagementManager(mapper, opsContext, configuration, userManager, roleManager, permissionService, geminiManager, userManagementManagerLogger);
        
        // Set the manager wrapper reference in GeminiManager after all managers are created
        geminiManager.SetManagerWrapper(this);
        aiPromptManager = new UNOPSAiPromptManager(mapper, opsContext, configuration, userManager, this, permissionService, aiPromptCacheService);
        entityConfigurationManager = new UNOPSEntityConfigurationManager(mapper, opsContext, configuration, permissionService);
        
        // Create GmailAddonManager with required dependencies (no longer needs GmailAddonHelper)
        var gmailAddonManagerLogger = loggerFactory.CreateLogger<UNOPSGmailAddonManager>();
        gmailAddonManager = new UNOPSGmailAddonManager(mapper, opsContext, contactManager, partnerManager, UserDataManager, interactionManager, permissionService, configuration, httpContextAccessor, userInfoService, gmailAddonManagerLogger, notificationManager);
        
        // Create BaseEngagementManager
        baseEngagementManager = new BaseEngagementManager(mapper, opsContext, configuration, permissionService, httpContextAccessor);
        
        // Create OpportunityManager with DbContextFactory for parallel query execution
        opportunityManager = new UNOPSOpportunityManager(mapper, opsContext, configuration, dbContextFactory, permissionService, httpContextAccessor, serviceProvider);
        
        // Create CommentManager
        commentManager = new CommentManager(mapper, opsContext, this);
        
        // Create AuditLogManager
        auditLogManager = new UNOPSAuditLogManager(mapper, opsContext, configuration, userManager, permissionService, httpContextAccessor);
        
        // Create RiskManager
        riskManager = new UNOPSRiskManager(mapper, opsContext, configuration, permissionService, httpContextAccessor, serviceProvider);
    }

    public override ISystemAdminManager SystemAdminManager => systemAdminManager;
    public override IContactManager ContactManager => contactManager;
    public override IInteractionManager InteractionManager => interactionManager;
    public override IPartnerTreeManager PartnerTreeManager => partnerTreeManager;
    public override IPartnerManager PartnerManager => partnerManager;
    public override IGeminiManager GeminiManager => geminiManager;
    public override IImageGenerationManager ImageGenerationManager => imageGenerationManager;
    public override ILinkManager LinkManager => linkManager;
    public override IUserManagementManager UserManagementManager => userManagementManager;
    public override IAiPromptManager AiPromptManager => aiPromptManager;
    public override IGmailAddonManager GmailAddonManager => gmailAddonManager;
    public override IOpportunityManager OpportunityManager => opportunityManager;
    public override ICommentManager CommentManager => commentManager;
    public override IAuditLogManager AuditLogManager => auditLogManager;
    public override IRiskManager RiskManager => riskManager;
    
    // UNOPS-specific managers
    public IUNOPSEntityConfigurationManager EntityConfigurationManager => entityConfigurationManager;
    public IBaseEngagementManager BaseEngagementManager => baseEngagementManager;
}