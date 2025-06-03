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

namespace UNOPS.PAO.Identity.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly string _googleClientId;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly UserManager<PAOIdentityUser> _userManager;
    private readonly SecretManagerServiceClient _secretManager;

    public AuthService(
        IConfiguration configuration, 
        UserManager<PAOIdentityUser> userManager)
    {
        _configuration = configuration;
        _googleClientId = _configuration["GoogleAuthSettings:clientId"];
        _jwtIssuer = _configuration["JWTSettings:validIssuer"];
        _jwtAudience = _configuration["JWTSettings:validAudienceDev"];
        _userManager = userManager;
        _secretManager = SecretManagerServiceClient.Create();
        
        // Get the JWT secret from Secret Manager
        var projectId = _configuration["AppConfig:ProjectId"];
        var secretName = $"projects/{projectId}/secrets/QA_Gmail_Plugin_Secret/versions/latest";
        var secret = _secretManager.AccessSecretVersion(secretName);
        _jwtSecret = secret.Payload.Data.ToStringUtf8();
    }

    public async Task<AuthResponse> AuthenticateWithGoogleAsync(GoogleSignInRequest request)
    {
        try
        {
            /*if (!request.IsValid())
            {
                throw new Exception("Invalid request. For Gmail plugin requests, Provider and Email are required. For web client requests, Provider and IdToken are required.");
            }*/

            string email;
            string subject;

            if (!string.IsNullOrEmpty(request.Email))
            {
                // This is a Gmail plugin request using email
                email = request.Email;
                
                // For Gmail plugin requests, we'll use the email as both the name and subject
                // since we can't get additional user info without OAuth
                //name = email.Split('@')[0]; // Use the part before @ as the name
                subject = email; // Use email as the subject
            }
            else
            {
                // This is a regular web client using Google ID token
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _googleClientId }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                email = payload.Email;
                //name = payload.Name;
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
            }
            
            // 3. Get user roles and other information
            var roles = await _userManager.GetRolesAsync(user);
            string name = Convert.ToString(user?.Id);

            var token = GenerateJwtToken(subject, email, name);
            var refreshToken = GenerateRefreshToken();

            return new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                UserId = subject,
                Email = email,
                Name = name,
                Roles = roles.ToList()
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Invalid Google token", ex);
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // Implement refresh token validation and generation of new access token
        throw new NotImplementedException();
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        // Implement token revocation logic
        throw new NotImplementedException();
    }

    private string GenerateJwtToken(string subject, string email, string name)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, subject),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, name),
                // Add more claims as needed
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
} 