using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Identity.Models;
using UNOPS.PAO.Identity.Services;

namespace UNOPS.PAO.Identity.Controllers;

[Route("/")]
[ApiController]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _authService;

    public GoogleAuthController(IGoogleAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("api/auth/google")]
    public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
    {
        try
        {
            var response = await _authService.AuthenticateWithGoogleAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("api/auth/refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("api/auth/revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            await _authService.RevokeTokenAsync(request.RefreshToken);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
} 