using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Identity.Models;
using UNOPS.PAO.Identity.Services;

namespace UNOPS.PAO.Identity.Controllers;
/*************************************************************************************************************************************
 * This controller is already authorized by IAP as public URLs seem to be protected by GCP / Cloudflare for the test server. 
 * Will look into optimizing the overall authentication and session flow if this approach works on the test server.
 * ***********************************************************************************************************************************/
[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]  
[ApiController]
public class GmailAddonAuthController : ControllerBase
{
    private readonly IGmailAddonAuthService _authService;

    public GmailAddonAuthController(IGmailAddonAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("api/gmail-addon/auth")]
    public async Task<IActionResult> GmailAddonAuthAsync([FromBody] GmailAddonSignInRequest request)
    {
        try
        {
            var response = await _authService.AuthenticateForGmailAddonAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("api/gmail-addon/refresh")]
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

    [HttpPost("api/gmail-addon/revoke")]
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