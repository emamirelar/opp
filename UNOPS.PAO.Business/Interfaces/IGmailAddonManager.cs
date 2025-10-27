using System.Security.Claims;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Integrations;

namespace UNOPS.PAO.Business.Interfaces;

public interface IGmailAddonManager
{
    /// <summary>
    /// Finds related records (contacts, partners, users) based on email addresses from Gmail
    /// </summary>
    /// <param name="input">Request containing email addresses to search for</param>
    /// <param name="user">The current user's claims principal</param>
    /// <returns>Response containing found contacts, partners, users and unmatched emails</returns>
    Task<GmailRelatedRecordsResponse> FindRelatedRecordsAsync(GmailRelatedRecordsRequest input, ClaimsPrincipal user);

    /// <summary>
    /// Creates contacts and partners from selected emails in Gmail addon
    /// </summary>
    /// <param name="request">Request containing selected email addresses and partner information</param>
    /// <param name="user">The current user's claims principal</param>
    /// <returns>Response containing creation results and statistics</returns>
    Task<GmailCreateRecordsResult> CreateRecordsFromEmailsAsync(GmailCreateRecordsRequest request, ClaimsPrincipal user);
}
