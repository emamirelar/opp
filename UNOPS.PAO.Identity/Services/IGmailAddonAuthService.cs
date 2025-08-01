using UNOPS.PAO.Identity.Models;

namespace UNOPS.PAO.Identity.Services;
/***********************************************************************************************************************************
* This class might not be needed after switching to IAP authentication. Will cleanup later after end to end testing.
***********************************************************************************************************************************/
public interface IGmailAddonAuthService
{
    /*Task<GmailAddonAuthResponse> AuthenticateForGmailAddonAsync(GmailAddonSignInRequest request);
    Task<GmailAddonAuthResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);*/
}

public class GmailAddonAuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public List<string> Roles { get; set; }
} 