using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Identity.Models;
using UNOPS.PAO.Identity.Services;

namespace UNOPS.PAO.Identity.Controllers;
/*************************************************************************************************************************************
 * This controller NOT USED currently, will clean up after end to end testing.
 * ***********************************************************************************************************************************/
[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]  
[ApiController]
public class GmailAddonAuthController : ControllerBase
{
    /*private readonly IGmailAddonAuthService _authService;

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
    }*/
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; }
} 