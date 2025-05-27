using UNOPS.PAO.Identity.Models;

namespace UNOPS.PAO.Identity.Services;

public interface IAuthService
{
    Task<AuthResponse> AuthenticateWithGoogleAsync(GoogleSignInRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
}

public class AuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public List<string> Roles { get; set; }
} 