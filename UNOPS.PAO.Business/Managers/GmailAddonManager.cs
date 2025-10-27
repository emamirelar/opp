using System.Security.Claims;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Integrations;

namespace UNOPS.PAO.Business.Managers;

public class GmailAddonManager : IGmailAddonManager
{
    private IMapper mapper;

    public GmailAddonManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
    }

    public virtual async Task<GmailRelatedRecordsResponse> FindRelatedRecordsAsync(GmailRelatedRecordsRequest input, ClaimsPrincipal user)
    {
        throw new NotImplementedException("Use UNOPSGmailAddonManager for UNOPS-specific implementation");
    }

    public virtual async Task<GmailCreateRecordsResult> CreateRecordsFromEmailsAsync(GmailCreateRecordsRequest request, ClaimsPrincipal user)
    {
        throw new NotImplementedException("Use UNOPSGmailAddonManager for UNOPS-specific implementation");
    }
}
