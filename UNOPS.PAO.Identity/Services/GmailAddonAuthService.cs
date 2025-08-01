using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UNOPS.PAO.Identity.Models;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.Identity.Entities;
using Google.Cloud.SecretManager.V1;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace UNOPS.PAO.Identity.Services;
/***********************************************************************************************************************************
* This class might not be needed after switching to IAP authentication. Will cleanup later after end to end testing.
***********************************************************************************************************************************/
public class GmailAddonAuthService : IGmailAddonAuthService
{
    private readonly IConfiguration _configuration;
    private readonly string _googleClientId;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly RoleManager<PAOIdentityRole> _roleManager;
    private readonly SecretManagerServiceClient _secretManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GmailAddonAuthService(
        IConfiguration configuration, 
        UserManager<PAOIdentityUser> userManager,
        RoleManager<PAOIdentityRole> roleManager,
        IHttpContextAccessor httpContextAccessor)
    {
        /*_configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _googleClientId = _configuration["GmailAddonAuthSettings:clientId"];
        _jwtIssuer = _configuration["JWTSettings:validIssuer"];
        _jwtAudience = new GmailAddonHelper(_configuration, _httpContextAccessor).GetValidAudienceForCurrentHost();
        _userManager = userManager;
        _roleManager = roleManager;
        _secretManager = SecretManagerServiceClient.Create();
        
        // Get the JWT secret from Secret Manager
        var projectId = _configuration["AppConfig:ProjectId"];
        var secretName = $"projects/{projectId}/secrets/QA_Gmail_Plugin_Secret/versions/latest";
        var secret = _secretManager.AccessSecretVersion(secretName);
        _jwtSecret = secret.Payload.Data.ToStringUtf8();*/
    }

    /*public async Task<GmailAddonAuthResponse> AuthenticateForGmailAddonAsync(GmailAddonSignInRequest request)
    {
        try
        {
            string email = "";
            string subject;

            if (!string.IsNullOrEmpty(request.IdToken))
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _googleClientId }
                };
                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                email = payload.Email;
                subject = payload.Subject;
            }

            // Check if email is from UNOPS domain
            if (!email.EndsWith("@unops.org"))
            {
                throw new Exception("Only UNOPS email addresses (@unops.org) are allowed to access this application.");
            }
            
            // 1. Check if user exists in your database
            var user = await _userManager.FindByEmailAsync(email);
            
            if (user == null)
            {
                // 2. Create user if they don't exist
                user = new PAOIdentityUser
                {
                    Email = email,
                    UserName = email,
                    GoogleSignIn = true,
                    IsInternal = true // Since we only allow @unops.org emails, all users are internal
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    throw new Exception("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                // Ensure GMAIL_GEN_USER role exists
                if (!await _roleManager.RoleExistsAsync("GMAIL_GEN_USER"))
                {
                    await _roleManager.CreateAsync(new PAOIdentityRole { Name = "GMAIL_GEN_USER" });
                }

                // Assign GMAIL_GEN_USER role
                await _userManager.AddToRoleAsync(user, "GMAIL_GEN_USER");
            }
            
            // 3. Get user roles and other information
            var roles = await _userManager.GetRolesAsync(user);

            // **Crucially, use the user's internal database ID for the 'sub' claim (subject)**
            string userIdForClaims = user.Id.ToString(); // This is your internal user ID
            string name = user.UserName; // use email as name

            var token = GenerateJwtToken(userIdForClaims, email, name, roles.ToList());
            var refreshToken = GenerateRefreshToken();

            // TO-DO: Store the refresh token securely, associated with the user
            //user.RefreshToken = refreshToken;
            //user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Example: Refresh token valid for 7 days
            //await _userManager.UpdateAsync(user);

            return new GmailAddonAuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                UserId = userIdForClaims,
                Email = email,
                Name = name
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Invalid Google token", ex);
        }
    }*/

    /*public async Task<GmailAddonAuthResponse> RefreshTokenAsync(string refreshToken)
    {
        //TO-DO: Implement refresh token logic
        // 1. Validate the refresh token
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var user = _userManager.Users.SingleOrDefault(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow);

        if (user == null)
        {
            throw new Exception("Invalid or expired refresh token.");
        }

        // 2. Generate new JWT token
        var newAccessToken = GenerateJwtToken(user.Id.ToString(), user.Email, user.UserName);

        // 3. Generate a new refresh token and update in database (optional but recommended for better security)
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Extend expiry
        await _userManager.UpdateAsync(user);

        // 4. Get user roles and other information for the response
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // New access token expiry
            UserId = user.Id.ToString(),
            Email = user.Email,
            Name = user.UserName,
            Roles = roles.ToList()
        };

        // Implement refresh token validation and generation of new access token
        throw new NotImplementedException();
    }*/

    public async Task RevokeTokenAsync(string refreshToken)
    {
        //TO-DO: Implement token revocation logic
        /*if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        var user = _userManager.Users.SingleOrDefault(u => u.RefreshToken == refreshToken);

        if (user == null)
        {
            // Token not found or already revoked, consider it a successful revocation
            return;
        }

        // Clear the refresh token and its expiry from the user
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userManager.UpdateAsync(user);*/

        // Implement refresh token validation and generation of new access token
        throw new NotImplementedException();
    }

    /*private string GenerateJwtToken(string subject, string email, string name, List<string> roles = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        // Create the base claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, subject),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
        };
    
        // Add role claims
        if (roles != null && roles.Any())
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }*/

    private string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
} 