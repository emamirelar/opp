using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly LinkManager linkManager;
    private readonly UNOPSUserManagementManager userManagementManager;
    private readonly UNOPSAiPromptManager aiPromptManager;
    private readonly UNOPSEntityConfigurationManager entityConfigurationManager;
    private readonly UNOPSGmailAddonManager gmailAddonManager;

    public UNOPSManagerWrapper(IMapper mapper, AppDbContext context, UNOPSAppDbContext opsContext, IConfiguration configuration,
                               UserManager<PAOIdentityUser> userManager, RoleManager<PAOIdentityRole> roleManager, IHttpContextAccessor httpContextAccessor, IPermissionService permissionService, HttpClient httpClient, ILoggerFactory loggerFactory, IServiceProvider serviceProvider, IUserInfoService userInfoService, IUserPreferenceService userPreferenceService, IUserProfileCacheService userProfileCacheService, IScreenContextCacheService screenContextCacheService, IGeoTimeCacheService geoTimeCacheService) : base(mapper, context, userManager, httpContextAccessor)
    {
        // Create a MemoryCache instance for services that need it
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        
        // Create logger for UNOPSPartnerManager
        var partnerManagerLogger = loggerFactory.CreateLogger<UNOPSPartnerManager>();
        
        // Create logger for UNOPSContactManager
        var contactManagerLogger = loggerFactory.CreateLogger<UNOPSContactManager>();
        
        // Create logger for UNOPSGeminiManager
        var geminiManagerLogger = loggerFactory.CreateLogger<UNOPSGeminiManager>();
        
        // Create a PartnerTreeService instance
        var partnerTreeRepository = new DataRepository<UNOPSPartnerTree>(opsContext);
        var partnerTreeService = new PartnerTreeService(partnerTreeRepository, memoryCache);

        systemAdminManager = new UNOPSSystemAdminManager(opsContext);
        contactManager = new UNOPSContactManager(mapper, opsContext, configuration, permissionService, httpContextAccessor, contactManagerLogger, serviceProvider);
        interactionManager = new UNOPSInteractionManager(mapper, opsContext, configuration, permissionService, httpContextAccessor, serviceProvider);
        partnerTreeManager = new UNOPSPartnerTreeManager(mapper, opsContext, configuration, partnerTreeService, permissionService);
        partnerManager = new UNOPSPartnerManager(mapper, opsContext, configuration, partnerTreeService, partnerManagerLogger, permissionService, httpContextAccessor, serviceProvider);
        linkManager = new LinkManager(mapper, opsContext);
        userManagementManager = new UNOPSUserManagementManager(mapper, opsContext, configuration, userManager, roleManager, permissionService);
        geminiManager = new UNOPSGeminiManager(mapper, opsContext, configuration, geminiManagerLogger, userManagementManager, userInfoService, userManager, userPreferenceService, userProfileCacheService, screenContextCacheService, geoTimeCacheService);
        aiPromptManager = new UNOPSAiPromptManager(mapper, opsContext, configuration, userManager, this, permissionService);
        entityConfigurationManager = new UNOPSEntityConfigurationManager(mapper, opsContext, configuration, permissionService);
        
        // Create GmailAddonManager with required dependencies (no longer needs GmailAddonHelper)
        var gmailAddonManagerLogger = loggerFactory.CreateLogger<UNOPSGmailAddonManager>();
        gmailAddonManager = new UNOPSGmailAddonManager(mapper, opsContext, contactManager, partnerManager, UserDataManager, interactionManager, permissionService, configuration, httpContextAccessor, userInfoService, gmailAddonManagerLogger);
    }

    public override ISystemAdminManager SystemAdminManager => systemAdminManager;
    public override IContactManager ContactManager => contactManager;
    public override IInteractionManager InteractionManager => interactionManager;
    public override IPartnerTreeManager PartnerTreeManager => partnerTreeManager;
    public override IPartnerManager PartnerManager => partnerManager;
    public override IGeminiManager GeminiManager => geminiManager;
    public override ILinkManager LinkManager => linkManager;
    public override IUserManagementManager UserManagementManager => userManagementManager;
    public override IAiPromptManager AiPromptManager => aiPromptManager;
    public override IGmailAddonManager GmailAddonManager => gmailAddonManager;
    
    // UNOPS-specific managers
    public IUNOPSEntityConfigurationManager EntityConfigurationManager => entityConfigurationManager;
}